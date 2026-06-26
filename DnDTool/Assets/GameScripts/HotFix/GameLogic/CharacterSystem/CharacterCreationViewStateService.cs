using System;
using System.Collections.Generic;

namespace GameLogic
{
    internal sealed class CharacterCreationViewStateService
    {
        private const int MinCharacterLevel = 1;
        private const int MaxCharacterLevel = 20;
        private const int BaseAbilityScore = 10;

        private static readonly Lazy<CharacterCreationViewStateService> s_instance =
            new Lazy<CharacterCreationViewStateService>(() => new CharacterCreationViewStateService());

        private CharacterCreationViewStateService()
        {
        }

        public static CharacterCreationViewStateService Instance => s_instance.Value;

        public CharacterCreationViewState BuildCurrentViewState(int level)
        {
            CharacterDraftState currentState = CharacterCreationSessionService.Instance.CurrentState;
            CharacterCardDraftSaveData character = currentState?.Character ?? new CharacterCardDraftSaveData();
            int normalizedLevel = Clamp(level, MinCharacterLevel, MaxCharacterLevel);

            DndClassDefineData classData = null;
            DndRaceDefineData raceData = null;
            DndBackgroundDefineData backgroundData = null;
            CharacterCreationRuleService.Instance.TryGetClass(character.ClassId, out classData);
            CharacterCreationRuleService.Instance.TryGetRace(character.RaceId, out raceData);
            CharacterCreationRuleService.Instance.TryGetBackground(character.BackgroundId, out backgroundData);

            CharacterCreationViewState state = new CharacterCreationViewState
            {
                ClassSummary = FirstNonEmpty(CharacterCreationRuleService.Instance.GetClassDisplayName(character.ClassId), "未选择职业"),
                RaceSummary = FirstNonEmpty(CharacterCreationRuleService.Instance.GetRaceDisplayName(character.RaceId), "未选择种族"),
                BackgroundSummary = FirstNonEmpty(CharacterCreationRuleService.Instance.GetBackgroundDisplayName(character.BackgroundId), "未选择背景"),
                AlignmentSummary = FirstNonEmpty(CharacterCreationRuleService.Instance.GetAlignmentDisplayName(character.Alignment), "未选择阵营"),
                SkillsSummary = "技能熟练"
            };

            FillClassState(state, classData, normalizedLevel);
            FillRaceState(state, raceData);
            FillAbilityStates(state);
            state.AbilityScoreOptions.AddRange(CharacterCreationSessionService.Instance.BuildGeneratedAbilityScoreOptions());
            FillSkillStates(state, raceData, backgroundData, normalizedLevel);
            FillPassivePerceptionState(state);
            FillCombatOverviewState(state, character, normalizedLevel);
            FillCurrencyState(state, character);
            FillEquipmentToolState(state, classData, raceData, backgroundData);
            return state;
        }

        public List<string> BuildFixedSkillProficiencyIds()
        {
            CharacterCardDraftSaveData character = CharacterCreationSessionService.Instance.CurrentState?.Character;
            DndRaceDefineData raceData = null;
            DndBackgroundDefineData backgroundData = null;
            CharacterCreationRuleService.Instance.TryGetRace(character?.RaceId, out raceData);
            CharacterCreationRuleService.Instance.TryGetBackground(character?.BackgroundId, out backgroundData);
            return CharacterCreationRuleService.Instance.BuildFixedSkillProficiencyIds(raceData, backgroundData);
        }

        public List<string> BuildFixedToolProficiencyIds()
        {
            CharacterCardDraftSaveData character = CharacterCreationSessionService.Instance.CurrentState?.Character;
            DndClassDefineData classData = null;
            DndBackgroundDefineData backgroundData = null;
            CharacterCreationRuleService.Instance.TryGetClass(character?.ClassId, out classData);
            CharacterCreationRuleService.Instance.TryGetBackground(character?.BackgroundId, out backgroundData);
            return CharacterCreationRuleService.Instance.BuildFixedToolProficiencyIds(classData, backgroundData);
        }

        public int GetSelectedRaceSpeed()
        {
            CharacterCardDraftSaveData character = CharacterCreationSessionService.Instance.CurrentState?.Character;
            return CharacterCreationRuleService.Instance.TryGetRace(character?.RaceId, out DndRaceDefineData raceData)
                ? Math.Max(0, raceData.Speed)
                : 0;
        }

        private static void FillClassState(CharacterCreationViewState state, DndClassDefineData classData, int level)
        {
            if (classData == null)
            {
                state.HitDiceCountText = "-";
                state.HitDiceDieText = "-";
                state.ClassFeatureClassName = "未选择职业";
                state.ClassFeatureSubclassName = string.Empty;
                state.ClassLevelText = string.Empty;
                state.EquipmentToolsSummary = "装备与工具熟练项";
                return;
            }

            state.HitDiceCountText = $"x{Math.Max(1, level)}";
            state.HitDiceDieText = classData.HitDie > 0 ? $"d{classData.HitDie}" : "-";
            state.ProficiencyBonusText = GetClassLevelProficiencyBonusText(classData.ClassId, level);
            state.ClassFeatureClassName = FirstNonEmpty(classData.Name, classData.ClassId);
            state.ClassFeatureSubclassName = GetSelectedSubclassDisplayName(classData.ClassId);
            state.ClassLevelText = $"Lv{Math.Max(1, level)}";
            state.ClassFeatures.AddRange(CharacterCreationFeatureDisplayService.Instance.BuildClassFeatureEntries(classData.ClassId, level));
        }

        private static void FillRaceState(CharacterCreationViewState state, DndRaceDefineData raceData)
        {
            if (raceData == null)
            {
                state.SpeedText = "-";
                state.RaceFeatureRaceName = "未选择种族";
                state.RaceFeatureSubRaceName = string.Empty;
                return;
            }

            state.SpeedText = raceData.Speed > 0 ? raceData.Speed.ToString() : "-";
            state.RaceFeatureRaceName = FirstNonEmpty(raceData.Name, raceData.RaceId);
            state.RaceFeatureSubRaceName = string.Empty;
            state.RaceFeatures.AddRange(CharacterCreationFeatureDisplayService.Instance.BuildFeatureEntries(raceData.FeatureIds, "Race", raceData.RaceId));
        }

        private static void FillAbilityStates(CharacterCreationViewState state)
        {
            AppendAbilityState(state, "Strength");
            AppendAbilityState(state, "Dexterity");
            AppendAbilityState(state, "Constitution");
            AppendAbilityState(state, "Intelligence");
            AppendAbilityState(state, "Wisdom");
            AppendAbilityState(state, "Charisma");
        }

        private static void AppendAbilityState(CharacterCreationViewState state, string abilityId)
        {
            int score = CharacterCreationSessionService.Instance.GetCurrentAbilityScore(abilityId, BaseAbilityScore);
            state.Abilities.Add(new CharacterCreationAbilityViewState
            {
                AbilityId = abilityId,
                Score = score,
                ModifierText = CharacterCreationCalculationService.Instance.FormatSigned(
                    CharacterCreationCalculationService.Instance.CalculateAbilityModifier(score)),
                CanIncrease = CharacterCreationSessionService.Instance.CanIncreaseRaceAbility(abilityId),
                CanDecrease = CharacterCreationSessionService.Instance.CanDecreaseRaceAbility(abilityId),
                CanManualInput = CharacterCreationSessionService.Instance.CanManualInputAbilityScore(abilityId)
            });
        }

        private static void FillSkillStates(
            CharacterCreationViewState state,
            DndRaceDefineData raceData,
            DndBackgroundDefineData backgroundData,
            int level)
        {
            List<string> fixedSkillIds = CharacterCreationRuleService.Instance.BuildFixedSkillProficiencyIds(raceData, backgroundData);
            List<string> proficiencyIds = CharacterCreationSessionService.Instance.BuildCurrentSkillProficiencyIds(fixedSkillIds);
            CharacterCardDraftSaveData character = CharacterCreationSessionService.Instance.CurrentState?.Character;
            CharacterCardDraftSaveData previewCharacter = BuildPreviewCharacter(character, level);
            CharacterRuntimeSnapshotData snapshot = CharacterDetailCalculationService.Instance.BuildDisplaySnapshot(previewCharacter);
            if (snapshot?.SkillProficiencyIds != null)
            {
                proficiencyIds = snapshot.SkillProficiencyIds;
            }

            TryGetClassLevelProficiencyBonus(character?.ClassId, level, out int proficiencyBonus);

            AppendSkillState(state, proficiencyIds, proficiencyBonus, "athletics", "运动", AbilityKind.Strength);
            AppendSkillState(state, proficiencyIds, proficiencyBonus, "acrobatics", "体操", AbilityKind.Dexterity);
            AppendSkillState(state, proficiencyIds, proficiencyBonus, "sleight_of_hand", "巧手", AbilityKind.Dexterity);
            AppendSkillState(state, proficiencyIds, proficiencyBonus, "stealth", "隐匿", AbilityKind.Dexterity);
            AppendSkillState(state, proficiencyIds, proficiencyBonus, "arcana", "奥秘", AbilityKind.Intelligence);
            AppendSkillState(state, proficiencyIds, proficiencyBonus, "history", "历史", AbilityKind.Intelligence);
            AppendSkillState(state, proficiencyIds, proficiencyBonus, "investigation", "调查", AbilityKind.Intelligence);
            AppendSkillState(state, proficiencyIds, proficiencyBonus, "nature", "自然", AbilityKind.Intelligence);
            AppendSkillState(state, proficiencyIds, proficiencyBonus, "religion", "宗教", AbilityKind.Intelligence);
            AppendSkillState(state, proficiencyIds, proficiencyBonus, "animal_handling", "驯兽", AbilityKind.Wisdom);
            AppendSkillState(state, proficiencyIds, proficiencyBonus, "insight", "洞悉", AbilityKind.Wisdom);
            AppendSkillState(state, proficiencyIds, proficiencyBonus, "medicine", "医药", AbilityKind.Wisdom);
            AppendSkillState(state, proficiencyIds, proficiencyBonus, "perception", "察觉", AbilityKind.Wisdom);
            AppendSkillState(state, proficiencyIds, proficiencyBonus, "survival", "求生", AbilityKind.Wisdom);
            AppendSkillState(state, proficiencyIds, proficiencyBonus, "deception", "欺瞒", AbilityKind.Charisma);
            AppendSkillState(state, proficiencyIds, proficiencyBonus, "intimidation", "威吓", AbilityKind.Charisma);
            AppendSkillState(state, proficiencyIds, proficiencyBonus, "performance", "表演", AbilityKind.Charisma);
            AppendSkillState(state, proficiencyIds, proficiencyBonus, "persuasion", "游说", AbilityKind.Charisma);
        }

        private static void AppendSkillState(
            CharacterCreationViewState state,
            IReadOnlyList<string> proficiencyIds,
            int proficiencyBonus,
            string skillId,
            string displayName,
            AbilityKind ability)
        {
            bool hasProficiency = ContainsSkillId(proficiencyIds, skillId, displayName);
            int skillBonus = GetAbilityModifier(ability);
            if (hasProficiency)
            {
                skillBonus += proficiencyBonus;
            }

            state.Skills.Add(new CharacterCreationSkillViewState
            {
                SkillId = skillId,
                Bonus = skillBonus,
                BonusText = CharacterCreationCalculationService.Instance.FormatSigned(skillBonus),
                HasProficiency = hasProficiency,
                IsChoiceCandidate = CharacterCreationSessionService.Instance.IsSkillChoiceCandidate(skillId, proficiencyIds)
            });
        }

        private static void FillPassivePerceptionState(CharacterCreationViewState state)
        {
            CharacterCreationSkillViewState perception = FindSkillState(state.Skills, "perception");
            state.PassivePerceptionText = perception != null ? (10 + perception.Bonus).ToString() : "-";
        }

        private static CharacterCreationSkillViewState FindSkillState(IReadOnlyList<CharacterCreationSkillViewState> skills, string skillId)
        {
            if (skills == null || string.IsNullOrWhiteSpace(skillId))
            {
                return null;
            }

            for (int index = 0; index < skills.Count; index++)
            {
                CharacterCreationSkillViewState skill = skills[index];
                if (skill != null && string.Equals(skill.SkillId, skillId, StringComparison.OrdinalIgnoreCase))
                {
                    return skill;
                }
            }

            return null;
        }

        private static string GetClassLevelProficiencyBonusText(string classId, int level)
        {
            return TryGetClassLevelProficiencyBonus(classId, level, out int proficiencyBonus)
                ? CharacterCreationCalculationService.Instance.FormatSigned(proficiencyBonus)
                : "-";
        }

        private static bool TryGetClassLevelProficiencyBonus(string classId, int level, out int proficiencyBonus)
        {
            proficiencyBonus = 0;
            if (string.IsNullOrWhiteSpace(classId))
            {
                return false;
            }

            if (!DndRuleContentService.Instance.TryGetClassLevelProgression(classId, Math.Max(1, level), out DndLevelProgressionData progression))
            {
                return false;
            }

            proficiencyBonus = progression.ProficiencyBonus;
            return true;
        }

        private static void FillCombatOverviewState(CharacterCreationViewState state, CharacterCardDraftSaveData character, int level)
        {
            CharacterCardDraftSaveData previewCharacter = BuildPreviewCharacter(character, level);
            CharacterRuntimeSnapshotData snapshot = CharacterDetailCalculationService.Instance.BuildDisplaySnapshot(previewCharacter);
            CharacterCombatOverviewViewState overview = CharacterDetailCalculationService.Instance.BuildCombatOverview(previewCharacter, snapshot);
            CharacterHpDisplayViewState hp = CharacterDetailCalculationService.Instance.BuildHpDisplay(snapshot);

            state.ArmorClassText = overview.ArmorClass > 0 ? overview.ArmorClass.ToString() : "-";
            state.InitiativeText = CharacterCreationCalculationService.Instance.FormatSigned(overview.InitiativeBonus);
            state.CurrentHpText = hp.MaxHp > 0 ? hp.CurrentHp.ToString() : "-";
            state.MaxHpText = hp.MaxHp > 0 ? hp.MaxHp.ToString() : "-";
            state.TemporaryHpText = hp.TemporaryHp > 0 ? hp.TemporaryHp.ToString() : "0";
            state.ShouldShowHitPointGenerationButton = hp.MaxHp <= 0;
            FillSpellcastingCombatText(state, previewCharacter, snapshot, level);
        }

        private static void FillCurrencyState(CharacterCreationViewState state, CharacterCardDraftSaveData character)
        {
            CharacterCurrencySaveData currency = CharacterCurrencySaveData.Clone(character?.Currency);
            state.CopperText = currency.Copper.ToString();
            state.SilverText = currency.Silver.ToString();
            state.ElectrumText = currency.Electrum.ToString();
            state.GoldText = currency.Gold.ToString();
            state.PlatinumText = currency.Platinum.ToString();
        }

        private static CharacterCardDraftSaveData BuildPreviewCharacter(CharacterCardDraftSaveData source, int level)
        {
            source ??= new CharacterCardDraftSaveData();
            string classId = source.ClassId?.Trim() ?? string.Empty;
            string raceId = source.RaceId?.Trim() ?? string.Empty;
            string backgroundId = source.BackgroundId?.Trim() ?? string.Empty;

            DndClassDefineData classData = null;
            DndRaceDefineData raceData = null;
            DndBackgroundDefineData backgroundData = null;
            CharacterCreationRuleService.Instance.TryGetClass(classId, out classData);
            CharacterCreationRuleService.Instance.TryGetRace(raceId, out raceData);
            CharacterCreationRuleService.Instance.TryGetBackground(backgroundId, out backgroundData);

            List<string> fixedSkillIds = CharacterCreationRuleService.Instance.BuildFixedSkillProficiencyIds(raceData, backgroundData);
            List<string> fixedToolIds = CharacterCreationRuleService.Instance.BuildFixedToolProficiencyIds(classData, backgroundData);

            CharacterCardDraftSaveData preview = new CharacterCardDraftSaveData
            {
                CharacterId = source.CharacterId?.Trim() ?? string.Empty,
                CharacterName = source.CharacterName?.Trim() ?? string.Empty,
                ClassId = classId,
                RaceId = raceId,
                BackgroundId = backgroundId,
                Alignment = source.Alignment?.Trim() ?? string.Empty,
                FeatId = source.FeatId?.Trim() ?? string.Empty,
                SpellId = source.SpellId?.Trim() ?? string.Empty,
                Level = Math.Max(1, level),
                HpModeId = CharacterHpModeIds.Normalize(source.HpModeId),
                MaxHp = Math.Max(0, source.MaxHp),
                CurrentHp = CharacterCreationCalculationService.Instance.NormalizeCurrentHp(source.CurrentHp, source.MaxHp),
                TemporaryHp = Math.Max(0, source.TemporaryHp),
                ManualHp = Math.Max(0, source.ManualHp),
                HpRolls = CharacterHpRollSaveDataCloneList(source.HpRolls),
                Equipment = CharacterEquipmentSetSaveData.Clone(source.Equipment),
                Resources = CharacterResourceSaveData.CloneList(source.Resources),
                Conditions = CharacterConditionStateSaveData.CloneList(source.Conditions),
                TemporaryEffects = CharacterTemporaryEffectSaveData.CloneList(source.TemporaryEffects),
                RuntimeSnapshot = CharacterRuntimeSnapshotData.Clone(source.RuntimeSnapshot)
            };

            preview.ClassProgresses = new List<CharacterClassProgressSaveData>();
            if (!string.IsNullOrWhiteSpace(classId))
            {
                preview.ClassProgresses.Add(new CharacterClassProgressSaveData
                {
                    ClassId = classId,
                    SubclassId = CharacterCreationSessionService.Instance.GetSelectedSubclassId(classId),
                    Level = Math.Max(1, level)
                });
            }

            preview.ChoiceSelections = BuildPreviewChoiceSelections(classId);
            CharacterRuntimeSnapshotData snapshot = preview.RuntimeSnapshot;
            snapshot.Level = Math.Max(1, level);
            snapshot.ClassId = classId;
            snapshot.RaceId = raceId;
            snapshot.BackgroundId = backgroundId;
            snapshot.Speed = raceData?.Speed ?? snapshot.Speed;
            snapshot.HpModeId = preview.HpModeId;
            snapshot.MaxHp = preview.MaxHp;
            snapshot.CurrentHp = CharacterCreationCalculationService.Instance.NormalizeCurrentHp(preview.CurrentHp, preview.MaxHp);
            snapshot.TemporaryHp = preview.TemporaryHp;
            snapshot.Strength = CharacterCreationSessionService.Instance.GetCurrentAbilityScore("Strength", BaseAbilityScore);
            snapshot.Dexterity = CharacterCreationSessionService.Instance.GetCurrentAbilityScore("Dexterity", BaseAbilityScore);
            snapshot.Constitution = CharacterCreationSessionService.Instance.GetCurrentAbilityScore("Constitution", BaseAbilityScore);
            snapshot.Intelligence = CharacterCreationSessionService.Instance.GetCurrentAbilityScore("Intelligence", BaseAbilityScore);
            snapshot.Wisdom = CharacterCreationSessionService.Instance.GetCurrentAbilityScore("Wisdom", BaseAbilityScore);
            snapshot.Charisma = CharacterCreationSessionService.Instance.GetCurrentAbilityScore("Charisma", BaseAbilityScore);
            snapshot.SkillProficiencyIds = CharacterCreationSessionService.Instance.BuildCurrentSkillProficiencyIds(fixedSkillIds);
            snapshot.ToolProficiencyIds = CharacterCreationSessionService.Instance.BuildCurrentToolProficiencyIds(fixedToolIds);
            preview.RuntimeSnapshot = snapshot;
            return preview;
        }

        private static List<CharacterHpRollSaveData> CharacterHpRollSaveDataCloneList(List<CharacterHpRollSaveData> source)
        {
            List<CharacterHpRollSaveData> result = new List<CharacterHpRollSaveData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterHpRollSaveData roll = source[index];
                if (roll == null)
                {
                    continue;
                }

                result.Add(new CharacterHpRollSaveData
                {
                    Level = Math.Max(1, roll.Level),
                    ClassId = roll.ClassId?.Trim() ?? string.Empty,
                    HitDie = Math.Max(0, roll.HitDie),
                    RollValue = Math.Max(0, roll.RollValue),
                    ConstitutionModifier = roll.ConstitutionModifier,
                    HpGain = Math.Max(0, roll.HpGain)
                });
            }

            return result;
        }

        private static List<CharacterChoiceSelectionSaveData> BuildPreviewChoiceSelections(string classId)
        {
            List<CharacterChoiceSelectionSaveData> result = new List<CharacterChoiceSelectionSaveData>();
            AppendFeatureChoices(result, CharacterCreationSessionService.Instance.BuildPreviewFeatureChoiceInputs());
            AppendToolChoices(result, CharacterCreationSessionService.Instance.BuildToolChoiceInputs(classId));
            AppendSkillChoices(result, CharacterCreationSessionService.Instance.BuildSkillChoiceInputs());
            AppendRaceAbilityChoices(result, CharacterCreationSessionService.Instance.BuildRaceAbilityChoiceInputs());
            return result;
        }

        private static void AppendRaceAbilityChoices(List<CharacterChoiceSelectionSaveData> target, List<CharacterCreationRaceAbilityChoiceInput> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterCreationRaceAbilityChoiceInput choice = source[index];
                if (choice == null || string.IsNullOrWhiteSpace(choice.ChoiceGroupId) || string.IsNullOrWhiteSpace(choice.OptionId))
                {
                    continue;
                }

                int count = Math.Max(1, choice.Count);
                for (int repeat = 0; repeat < count; repeat++)
                {
                    target.Add(new CharacterChoiceSelectionSaveData
                    {
                        ChoiceGroupId = choice.ChoiceGroupId.Trim(),
                        OptionId = choice.OptionId.Trim(),
                        SourceType = "Race",
                        SourceId = choice.SourceId?.Trim() ?? string.Empty,
                        Level = 1
                    });
                }
            }
        }

        private static void AppendSkillChoices(List<CharacterChoiceSelectionSaveData> target, List<CharacterCreationSkillChoiceInput> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterCreationSkillChoiceInput choice = source[index];
                if (choice == null || string.IsNullOrWhiteSpace(choice.ChoiceGroupId) || string.IsNullOrWhiteSpace(choice.SkillId))
                {
                    continue;
                }

                target.Add(new CharacterChoiceSelectionSaveData
                {
                    ChoiceGroupId = choice.ChoiceGroupId.Trim(),
                    OptionId = choice.SkillId.Trim(),
                    SourceType = choice.SourceType?.Trim() ?? string.Empty,
                    SourceId = choice.SourceId?.Trim() ?? string.Empty,
                    Level = Math.Max(1, choice.Level)
                });
            }
        }

        private static void AppendToolChoices(List<CharacterChoiceSelectionSaveData> target, List<CharacterCreationToolChoiceInput> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterCreationToolChoiceInput choice = source[index];
                if (choice == null || string.IsNullOrWhiteSpace(choice.ChoiceGroupId) || string.IsNullOrWhiteSpace(choice.OptionId))
                {
                    continue;
                }

                target.Add(new CharacterChoiceSelectionSaveData
                {
                    ChoiceGroupId = choice.ChoiceGroupId.Trim(),
                    OptionId = choice.OptionId.Trim(),
                    SourceType = choice.SourceType?.Trim() ?? string.Empty,
                    SourceId = choice.SourceId?.Trim() ?? string.Empty,
                    ClassId = choice.ClassId?.Trim() ?? string.Empty,
                    Level = Math.Max(1, choice.Level)
                });
            }
        }

        private static void AppendFeatureChoices(List<CharacterChoiceSelectionSaveData> target, List<CharacterCreationFeatureChoiceInput> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterCreationFeatureChoiceInput choice = source[index];
                if (choice == null || string.IsNullOrWhiteSpace(choice.ChoiceGroupId) || string.IsNullOrWhiteSpace(choice.OptionId))
                {
                    continue;
                }

                target.Add(new CharacterChoiceSelectionSaveData
                {
                    ChoiceGroupId = choice.ChoiceGroupId.Trim(),
                    OptionId = choice.OptionId.Trim(),
                    SourceType = choice.SourceType?.Trim() ?? string.Empty,
                    SourceId = choice.SourceId?.Trim() ?? string.Empty,
                    ClassId = choice.ClassId?.Trim() ?? string.Empty,
                    Level = Math.Max(1, choice.Level)
                });
            }
        }

        private static void FillSpellcastingCombatText(CharacterCreationViewState state, CharacterCardDraftSaveData character, CharacterRuntimeSnapshotData snapshot, int level)
        {
            string abilityId = FindSpellcastingAbilityId(character);
            if (string.IsNullOrWhiteSpace(abilityId) || !TryGetClassLevelProficiencyBonus(character?.ClassId, level, out int proficiencyBonus))
            {
                state.SpellSaveDcText = "-";
                state.SpellAttackBonusText = "-";
                return;
            }

            int abilityScore = GetAbilityScore(snapshot, abilityId);
            if (abilityScore <= 0)
            {
                state.SpellSaveDcText = "-";
                state.SpellAttackBonusText = "-";
                return;
            }

            int abilityModifier = CharacterCreationCalculationService.Instance.CalculateAbilityModifier(abilityScore);
            int spellAttackBonus = proficiencyBonus + abilityModifier;
            int spellSaveDc = 8 + proficiencyBonus + abilityModifier;
            state.SpellSaveDcText = spellSaveDc.ToString();
            state.SpellAttackBonusText = CharacterCreationCalculationService.Instance.FormatSigned(spellAttackBonus);
        }

        private static string FindSpellcastingAbilityId(CharacterCardDraftSaveData character)
        {
            if (character == null)
            {
                return string.Empty;
            }

            List<CharacterClassProgressSaveData> progresses = character.ClassProgresses ?? new List<CharacterClassProgressSaveData>();
            for (int index = 0; index < progresses.Count; index++)
            {
                CharacterClassProgressSaveData progress = progresses[index];
                if (progress == null)
                {
                    continue;
                }

                if (DndRuleContentService.Instance.TryGetClass(progress.ClassId, out DndClassDefineData classData)
                    && !string.IsNullOrWhiteSpace(classData.SpellcastingAbility))
                {
                    return classData.SpellcastingAbility.Trim();
                }

                string subclassAbility = FindSubclassSpellcastingAbilityId(progress, character.ChoiceSelections);
                if (!string.IsNullOrWhiteSpace(subclassAbility))
                {
                    return subclassAbility;
                }
            }

            return string.Empty;
        }

        private static string FindSubclassSpellcastingAbilityId(CharacterClassProgressSaveData progress, IReadOnlyList<CharacterChoiceSelectionSaveData> selections)
        {
            if (progress == null || string.IsNullOrWhiteSpace(progress.SubclassId))
            {
                return string.Empty;
            }

            IReadOnlyList<DndSubclassLevelProgressionData> progressions = DndRuleContentService.Instance.GetSubclassProgressions(progress.SubclassId.Trim());
            int classLevel = Math.Max(1, progress.Level);
            for (int index = 0; index < progressions.Count; index++)
            {
                DndSubclassLevelProgressionData progression = progressions[index];
                if (progression == null || progression.Level > classLevel)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(progression.ClassId)
                    && !string.IsNullOrWhiteSpace(progress.ClassId)
                    && !string.Equals(progression.ClassId, progress.ClassId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string abilityId = FindSpellcastingAbilityId(progression.FeatureIds);
                if (!string.IsNullOrWhiteSpace(abilityId))
                {
                    return abilityId;
                }
            }

            return FindSelectedChoiceSpellcastingAbility(selections);
        }

        private static string FindSelectedChoiceSpellcastingAbility(IReadOnlyList<CharacterChoiceSelectionSaveData> selections)
        {
            if (selections == null)
            {
                return string.Empty;
            }

            for (int index = 0; index < selections.Count; index++)
            {
                CharacterChoiceSelectionSaveData selection = selections[index];
                if (selection == null || string.IsNullOrWhiteSpace(selection.ChoiceGroupId) || string.IsNullOrWhiteSpace(selection.OptionId))
                {
                    continue;
                }

                DndChoiceOptionData option = FindChoiceOption(selection.ChoiceGroupId, selection.OptionId);
                string abilityId = FindSpellcastingAbilityId(option?.GrantEffectIds, option?.GrantFeatureIds);
                if (!string.IsNullOrWhiteSpace(abilityId))
                {
                    return abilityId;
                }
            }

            return string.Empty;
        }

        private static DndChoiceOptionData FindChoiceOption(string choiceGroupId, string optionId)
        {
            IReadOnlyList<DndChoiceOptionData> options = DndRuleContentService.Instance.GetChoiceOptions(choiceGroupId);
            for (int index = 0; index < options.Count; index++)
            {
                DndChoiceOptionData option = options[index];
                if (option != null && string.Equals(option.OptionId, optionId, StringComparison.OrdinalIgnoreCase))
                {
                    return option;
                }
            }

            return null;
        }

        private static string FindSpellcastingAbilityId(IReadOnlyList<string> featureIds)
        {
            if (featureIds == null)
            {
                return string.Empty;
            }

            for (int index = 0; index < featureIds.Count; index++)
            {
                if (!DndRuleContentService.Instance.TryGetFeature(featureIds[index], out DndFeatureDefineData feature))
                {
                    continue;
                }

                string abilityId = FindSpellcastingAbilityId(feature.EffectIds, null);
                if (!string.IsNullOrWhiteSpace(abilityId))
                {
                    return abilityId;
                }
            }

            return string.Empty;
        }

        private static string FindSpellcastingAbilityId(IReadOnlyList<string> effectIds, IReadOnlyList<string> featureIds)
        {
            string abilityId = FindSpellcastingAbilityFromEffects(effectIds);
            if (!string.IsNullOrWhiteSpace(abilityId))
            {
                return abilityId;
            }

            return FindSpellcastingAbilityId(featureIds);
        }

        private static string FindSpellcastingAbilityFromEffects(IReadOnlyList<string> effectIds)
        {
            if (effectIds == null)
            {
                return string.Empty;
            }

            for (int index = 0; index < effectIds.Count; index++)
            {
                if (DndRuleContentService.Instance.TryGetFeatureEffect(effectIds[index], out DndFeatureEffectData effect)
                    && string.Equals(effect.EffectType, "SpellcastingAbility", StringComparison.OrdinalIgnoreCase))
                {
                    return !string.IsNullOrWhiteSpace(effect.Value)
                        ? effect.Value.Trim()
                        : effect.Target?.Trim() ?? string.Empty;
                }
            }

            return string.Empty;
        }

        private static int GetAbilityScore(CharacterRuntimeSnapshotData snapshot, string abilityId)
        {
            if (snapshot == null || string.IsNullOrWhiteSpace(abilityId))
            {
                return 0;
            }

            string normalized = abilityId.Trim();
            if (string.Equals(normalized, "Strength", StringComparison.OrdinalIgnoreCase) || normalized == "力气" || normalized == "力量")
            {
                return snapshot.Strength;
            }

            if (string.Equals(normalized, "Dexterity", StringComparison.OrdinalIgnoreCase) || normalized == "敏捷")
            {
                return snapshot.Dexterity;
            }

            if (string.Equals(normalized, "Constitution", StringComparison.OrdinalIgnoreCase) || normalized == "体质")
            {
                return snapshot.Constitution;
            }

            if (string.Equals(normalized, "Intelligence", StringComparison.OrdinalIgnoreCase) || normalized == "智力")
            {
                return snapshot.Intelligence;
            }

            if (string.Equals(normalized, "Wisdom", StringComparison.OrdinalIgnoreCase) || normalized == "感知")
            {
                return snapshot.Wisdom;
            }

            if (string.Equals(normalized, "Charisma", StringComparison.OrdinalIgnoreCase) || normalized == "魅力")
            {
                return snapshot.Charisma;
            }

            return 0;
        }

        private static void FillEquipmentToolState(
            CharacterCreationViewState state,
            DndClassDefineData classData,
            DndRaceDefineData raceData,
            DndBackgroundDefineData backgroundData)
        {
            state.EquipmentTools = CharacterCreationEquipmentToolDisplayService.Instance.BuildDisplayState(classData, raceData, backgroundData);
            int count = state.EquipmentTools.Labels.Count;
            state.EquipmentToolsSummary = count > 0 ? $"装备与工具熟练项：{count} 项" : "装备与工具熟练项";
        }

        private static int GetAbilityModifier(AbilityKind ability)
        {
            string abilityId = ability switch
            {
                AbilityKind.Strength => "Strength",
                AbilityKind.Dexterity => "Dexterity",
                AbilityKind.Constitution => "Constitution",
                AbilityKind.Intelligence => "Intelligence",
                AbilityKind.Wisdom => "Wisdom",
                AbilityKind.Charisma => "Charisma",
                _ => string.Empty
            };

            int score = CharacterCreationSessionService.Instance.GetCurrentAbilityScore(abilityId, BaseAbilityScore);
            return CharacterCreationCalculationService.Instance.CalculateAbilityModifier(score);
        }

        private static string GetSelectedSubclassDisplayName(string classId)
        {
            if (string.IsNullOrWhiteSpace(classId))
            {
                return string.Empty;
            }

            List<CharacterCreationFeatureChoiceState> states = CharacterCreationSessionService.Instance.FeatureChoiceStates;
            for (int index = 0; index < states.Count; index++)
            {
                CharacterCreationFeatureChoiceState state = states[index];
                if (state == null
                    || !string.Equals(state.SourceType, "Class", StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(state.ClassId, classId, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(state.ChoiceType, "Subclass", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return CharacterCreationFeatureDisplayService.Instance.GetSelectedFeatureChoiceDisplayName(state);
            }

            return string.Empty;
        }

        private static bool ContainsSkillId(IReadOnlyList<string> values, string id, string displayName)
        {
            if (values == null)
            {
                return false;
            }

            string normalizedId = NormalizeSkillId(id);
            string normalizedDisplayName = NormalizeSkillId(displayName);
            for (int index = 0; index < values.Count; index++)
            {
                string normalizedValue = NormalizeSkillId(values[index]);
                if (string.IsNullOrWhiteSpace(normalizedValue))
                {
                    continue;
                }

                if ((!string.IsNullOrWhiteSpace(normalizedId) && string.Equals(normalizedValue, normalizedId, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrWhiteSpace(normalizedDisplayName) && string.Equals(normalizedValue, normalizedDisplayName, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        private static string NormalizeSkillId(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToLowerInvariant();
        }

        private static string FirstNonEmpty(string first, string second)
        {
            return !string.IsNullOrWhiteSpace(first) ? first.Trim() : second?.Trim() ?? string.Empty;
        }

        private static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}
