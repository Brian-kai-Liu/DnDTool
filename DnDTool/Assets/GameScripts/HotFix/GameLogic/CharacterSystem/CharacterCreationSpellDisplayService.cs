using System;
using System.Collections.Generic;
using System.Text;

namespace GameLogic
{
    internal sealed class CharacterCreationSpellDisplayService
    {
        private static readonly Lazy<CharacterCreationSpellDisplayService> s_instance =
            new Lazy<CharacterCreationSpellDisplayService>(() => new CharacterCreationSpellDisplayService());

        private CharacterCreationSpellDisplayService()
        {
        }

        public static CharacterCreationSpellDisplayService Instance => s_instance.Value;

        public CharacterCreationSpellbookViewState BuildSpellbook(CharacterCardDraftSaveData character, int level, int filterLevel)
        {
            CharacterCreationSpellbookViewState state = new CharacterCreationSpellbookViewState
            {
                FilterLevel = filterLevel
            };

            character ??= new CharacterCardDraftSaveData();
            string classId = character.ClassId?.Trim() ?? string.Empty;
            int normalizedLevel = Math.Max(1, level);
            DndClassDefineData classData = null;
            DndLevelProgressionData levelProgression = null;

            if (string.IsNullOrWhiteSpace(classId)
                || !DndRuleContentService.Instance.TryGetClass(classId, out classData)
                || !DndRuleContentService.Instance.TryGetClassLevelProgression(classId, normalizedLevel, out levelProgression))
            {
                state.SummaryText = "请选择施法职业后查看可学习法术。";
                AppendLearnedSpells(state, character, string.Empty);
                return state;
            }

            FillSpellLimits(state, character, classData, levelProgression);
            AppendAvailableSpells(state, character, classData, normalizedLevel, filterLevel);
            AppendLearnedSpells(state, character, classId);
            state.SummaryText = BuildSpellbookSummary(state, classData);
            return state;
        }

        public List<CharacterCreationSpellCardViewState> BuildLearnedSpellCards(CharacterCardDraftSaveData character)
        {
            CharacterCreationSpellbookViewState state = new CharacterCreationSpellbookViewState();
            AppendLearnedSpells(state, character ?? new CharacterCardDraftSaveData(), character?.ClassId ?? string.Empty);
            return state.LearnedSpells;
        }

        public string GetSpellDetailTitle(string spellId)
        {
            return DndRuleContentService.Instance.TryGetSpell(spellId, out DndSpellDefineData spell)
                ? FirstNonEmpty(spell.Name, spell.SpellId)
                : spellId?.Trim() ?? string.Empty;
        }

        public string GetSpellDetailDescription(string spellId)
        {
            if (string.IsNullOrWhiteSpace(spellId)
                || !DndRuleContentService.Instance.TryGetSpell(spellId.Trim(), out DndSpellDefineData spell)
                || spell == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            AppendDetailLine(builder, "环位", FormatSpellLevel(spell.Level));
            AppendDetailLine(builder, "学派", spell.School);
            AppendDetailLine(builder, "施法时间", spell.CastingTime);
            AppendDetailLine(builder, "距离", spell.Range);
            AppendDetailLine(builder, "成分", spell.Components);
            AppendDetailLine(builder, "持续时间", spell.Duration);
            AppendDetailLine(builder, "专注", spell.Concentration ? "是" : "否");
            AppendDetailLine(builder, "仪式", spell.Ritual ? "是" : "否");
            AppendDetailLine(builder, "攻击/豁免", FormatAttackOrSave(spell));
            AppendDetailLine(builder, "伤害", FormatDamage(spell));

            if (!string.IsNullOrWhiteSpace(spell.Description))
            {
                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                builder.AppendLine(spell.Description.Trim());
            }

            if (!string.IsNullOrWhiteSpace(spell.HigherLevelDescription))
            {
                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                builder.Append("升环：");
                builder.Append(spell.HigherLevelDescription.Trim());
            }

            return builder.ToString().Trim();
        }

        public bool IsSpellKnown(CharacterCardDraftSaveData character, string spellId)
        {
            if (character?.Spellcasting?.Spells == null || string.IsNullOrWhiteSpace(spellId))
            {
                return false;
            }

            for (int index = 0; index < character.Spellcasting.Spells.Count; index++)
            {
                CharacterKnownSpellSaveData spell = character.Spellcasting.Spells[index];
                if (spell != null
                    && spell.IsKnown
                    && string.Equals(spell.SpellId, spellId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static void FillSpellLimits(
            CharacterCreationSpellbookViewState state,
            CharacterCardDraftSaveData character,
            DndClassDefineData classData,
            DndLevelProgressionData levelProgression)
        {
            state.MaxKnownCantrips = Math.Max(0, levelProgression?.CantripKnown ?? 0);
            state.MaxKnownSpells = Math.Max(0, levelProgression?.SpellKnown ?? 0);
            state.MaxPreparedSpells = CalculatePreparedSpellLimit(character, classData, levelProgression);

            if (classData == null
                || string.IsNullOrWhiteSpace(classData.SpellSlotProgressionId)
                || levelProgression?.SpellSlotProgressionLevel == null
                || !DndRuleContentService.Instance.TryGetSpellSlotProgression(
                    classData.SpellSlotProgressionId,
                    levelProgression.SpellSlotProgressionLevel.Value,
                    out DndSpellSlotProgressionData progression))
            {
                state.MaxSpellLevel = 0;
                return;
            }

            state.MaxSpellLevel = Math.Max(0, progression.MaxSpellLevel);
        }

        private static int CalculatePreparedSpellLimit(
            CharacterCardDraftSaveData character,
            DndClassDefineData classData,
            DndLevelProgressionData levelProgression)
        {
            string formula = levelProgression?.PreparedSpellFormula?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(formula))
            {
                return 0;
            }

            int level = Math.Max(1, character?.Level ?? 1);
            int abilityModifier = 0;
            if (classData != null && !string.IsNullOrWhiteSpace(classData.SpellcastingAbility))
            {
                int abilityScore = GetAbilityScore(character, classData.SpellcastingAbility);
                abilityModifier = CharacterCreationCalculationService.Instance.CalculateAbilityModifier(abilityScore);
            }

            string compact = formula.Replace(" ", string.Empty);
            int result = 0;
            string[] plusParts = compact.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
            for (int index = 0; index < plusParts.Length; index++)
            {
                string part = plusParts[index];
                if (string.Equals(part, "level", StringComparison.OrdinalIgnoreCase))
                {
                    result += level;
                }
                else if (part.StartsWith("floor(", StringComparison.OrdinalIgnoreCase)
                    && part.EndsWith(")", StringComparison.Ordinal))
                {
                    result += EvaluateFloorFormula(part, level, abilityModifier);
                }
                else if (part.EndsWith("Mod", StringComparison.OrdinalIgnoreCase))
                {
                    result += abilityModifier;
                }
                else if (int.TryParse(part, out int value))
                {
                    result += value;
                }
            }

            return Math.Max(0, result);
        }

        private static int EvaluateFloorFormula(string formula, int level, int abilityModifier)
        {
            if (string.IsNullOrWhiteSpace(formula)
                || !formula.StartsWith("floor(", StringComparison.OrdinalIgnoreCase)
                || !formula.EndsWith(")", StringComparison.Ordinal))
            {
                return 0;
            }

            string inner = formula.Substring("floor(".Length, formula.Length - "floor(".Length - 1);
            string[] divideParts = inner.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (divideParts.Length != 2)
            {
                return 0;
            }

            int dividend = ResolveFormulaValue(divideParts[0], level, abilityModifier);
            int divisor = ResolveFormulaValue(divideParts[1], level, abilityModifier);
            return divisor == 0 ? 0 : dividend / divisor;
        }

        private static int ResolveFormulaValue(string token, int level, int abilityModifier)
        {
            string trimmed = token?.Trim() ?? string.Empty;
            if (string.Equals(trimmed, "level", StringComparison.OrdinalIgnoreCase))
            {
                return level;
            }

            if (trimmed.EndsWith("Mod", StringComparison.OrdinalIgnoreCase))
            {
                return abilityModifier;
            }

            return int.TryParse(trimmed, out int value) ? value : 0;
        }

        private static int GetAbilityScore(CharacterCardDraftSaveData character, string abilityId)
        {
            CharacterRuntimeSnapshotData snapshot = character?.RuntimeSnapshot;
            if (snapshot == null)
            {
                return 10;
            }

            if (string.Equals(abilityId, "Strength", StringComparison.OrdinalIgnoreCase))
            {
                return snapshot.Strength;
            }

            if (string.Equals(abilityId, "Dexterity", StringComparison.OrdinalIgnoreCase))
            {
                return snapshot.Dexterity;
            }

            if (string.Equals(abilityId, "Constitution", StringComparison.OrdinalIgnoreCase))
            {
                return snapshot.Constitution;
            }

            if (string.Equals(abilityId, "Intelligence", StringComparison.OrdinalIgnoreCase))
            {
                return snapshot.Intelligence;
            }

            if (string.Equals(abilityId, "Wisdom", StringComparison.OrdinalIgnoreCase))
            {
                return snapshot.Wisdom;
            }

            if (string.Equals(abilityId, "Charisma", StringComparison.OrdinalIgnoreCase))
            {
                return snapshot.Charisma;
            }

            return 10;
        }

        private static void AppendAvailableSpells(
            CharacterCreationSpellbookViewState state,
            CharacterCardDraftSaveData character,
            DndClassDefineData classData,
            int level,
            int filterLevel)
        {
            if (classData == null)
            {
                return;
            }

            HashSet<string> added = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            IReadOnlyList<DndClassSpellListData> classSpellList = DndRuleContentService.Instance.GetClassSpellList(classData.ClassId);
            for (int index = 0; index < classSpellList.Count; index++)
            {
                DndClassSpellListData classSpell = classSpellList[index];
                if (classSpell == null
                    || string.IsNullOrWhiteSpace(classSpell.SpellId)
                    || classSpell.MinClassLevel > level
                    || !DndRuleContentService.Instance.TryGetSpell(classSpell.SpellId, out DndSpellDefineData spell)
                    || spell == null
                    || spell.Level > state.MaxSpellLevel
                    || (filterLevel >= 0 && spell.Level != filterLevel)
                    || !added.Add(spell.SpellId))
                {
                    continue;
                }

                state.AvailableSpells.Add(BuildSpellCard(spell, character, classData.ClassId));
                CharacterCreationSpellCardViewState card = state.AvailableSpells[state.AvailableSpells.Count - 1];
                card.IsSelected = string.Equals(
                    CharacterCreationSessionService.Instance.SpellSelectionState.PendingSpellId,
                    card.SpellId,
                    StringComparison.OrdinalIgnoreCase);
            }
        }

        private static void AppendLearnedSpells(CharacterCreationSpellbookViewState state, CharacterCardDraftSaveData character, string fallbackClassId)
        {
            if (state == null || character?.Spellcasting?.Spells == null)
            {
                return;
            }

            for (int index = 0; index < character.Spellcasting.Spells.Count; index++)
            {
                CharacterKnownSpellSaveData knownSpell = character.Spellcasting.Spells[index];
                if (knownSpell == null
                    || string.IsNullOrWhiteSpace(knownSpell.SpellId)
                    || !knownSpell.IsKnown
                    || !DndRuleContentService.Instance.TryGetSpell(knownSpell.SpellId, out DndSpellDefineData spell))
                {
                    continue;
                }

                CharacterCreationSpellCardViewState card = BuildSpellCard(spell, character, FirstNonEmpty(knownSpell.SourceClassId, fallbackClassId));
                card.IsPrepared = knownSpell.IsPrepared;
                card.IsAlwaysPrepared = knownSpell.IsAlwaysPrepared;
                state.LearnedSpells.Add(card);
                if (spell.Level == 0)
                {
                    state.KnownCantrips++;
                }
                else
                {
                    state.KnownSpells++;
                }

                if (knownSpell.IsPrepared || knownSpell.IsAlwaysPrepared)
                {
                    state.PreparedSpells++;
                }
            }
        }

        private static CharacterCreationSpellCardViewState BuildSpellCard(DndSpellDefineData spell, CharacterCardDraftSaveData character, string classId)
        {
            bool known = CharacterCreationSpellDisplayService.Instance.IsSpellKnown(character, spell?.SpellId);
            return new CharacterCreationSpellCardViewState
            {
                SpellId = spell?.SpellId ?? string.Empty,
                Name = FirstNonEmpty(spell?.Name, spell?.SpellId),
                LevelText = FormatSpellLevel(spell?.Level ?? 0),
                SchoolText = spell?.School?.Trim() ?? string.Empty,
                IsKnown = known,
                IsPrepared = known && IsKnownSpellPrepared(character, spell?.SpellId),
                IsAlwaysPrepared = known && IsKnownSpellAlwaysPrepared(character, spell?.SpellId)
            };
        }

        private static bool IsKnownSpellPrepared(CharacterCardDraftSaveData character, string spellId)
        {
            CharacterKnownSpellSaveData spell = FindKnownSpell(character, spellId);
            return spell != null && spell.IsPrepared;
        }

        private static bool IsKnownSpellAlwaysPrepared(CharacterCardDraftSaveData character, string spellId)
        {
            CharacterKnownSpellSaveData spell = FindKnownSpell(character, spellId);
            return spell != null && spell.IsAlwaysPrepared;
        }

        private static CharacterKnownSpellSaveData FindKnownSpell(CharacterCardDraftSaveData character, string spellId)
        {
            if (character?.Spellcasting?.Spells == null || string.IsNullOrWhiteSpace(spellId))
            {
                return null;
            }

            for (int index = 0; index < character.Spellcasting.Spells.Count; index++)
            {
                CharacterKnownSpellSaveData spell = character.Spellcasting.Spells[index];
                if (spell != null && string.Equals(spell.SpellId, spellId, StringComparison.OrdinalIgnoreCase))
                {
                    return spell;
                }
            }

            return null;
        }

        private static string BuildSpellbookSummary(CharacterCreationSpellbookViewState state, DndClassDefineData classData)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(FirstNonEmpty(classData?.Name, classData?.ClassId));
            builder.Append(" 法术");
            builder.AppendLine();
            builder.Append("最高环位：");
            builder.Append(FormatSpellLevel(state.MaxSpellLevel));

            if (state.MaxKnownCantrips > 0)
            {
                builder.AppendLine();
                builder.Append("已知戏法：");
                builder.Append(state.KnownCantrips);
                builder.Append("/");
                builder.Append(state.MaxKnownCantrips);
            }

            if (state.MaxKnownSpells > 0)
            {
                builder.AppendLine();
                builder.Append("已知法术：");
                builder.Append(state.KnownSpells);
                builder.Append("/");
                builder.Append(state.MaxKnownSpells);
            }

            if (state.MaxPreparedSpells > 0)
            {
                builder.AppendLine();
                builder.Append("已准备法术：");
                builder.Append(state.PreparedSpells);
                builder.Append("/");
                builder.Append(state.MaxPreparedSpells);
            }

            return builder.ToString();
        }

        private static string FormatSpellLevel(int level)
        {
            if (level <= 0)
            {
                return "戏法";
            }

            return $"{level}环";
        }

        private static string FormatAttackOrSave(DndSpellDefineData spell)
        {
            if (spell == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(spell.SaveAbility))
            {
                return $"豁免：{spell.SaveAbility.Trim()}";
            }

            return spell.AttackType?.Trim() ?? string.Empty;
        }

        private static string FormatDamage(DndSpellDefineData spell)
        {
            if (spell == null || string.IsNullOrWhiteSpace(spell.DamageFormula))
            {
                return string.Empty;
            }

            return string.IsNullOrWhiteSpace(spell.DamageType)
                ? spell.DamageFormula.Trim()
                : $"{spell.DamageFormula.Trim()} {spell.DamageType.Trim()}";
        }

        private static void AppendDetailLine(StringBuilder builder, string label, string value)
        {
            if (builder == null || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            builder.Append(label);
            builder.Append("：");
            builder.AppendLine(value.Trim());
        }

        private static string FirstNonEmpty(string first, string second)
        {
            return !string.IsNullOrWhiteSpace(first) ? first.Trim() : second?.Trim() ?? string.Empty;
        }
    }
}
