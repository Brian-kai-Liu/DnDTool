using System;
using System.Collections.Generic;

namespace GameLogic
{
    internal sealed class CharacterDetailCalculationService
    {
        private const int DefaultAbilityScore = 10;
        private static readonly Lazy<CharacterDetailCalculationService> s_instance =
            new Lazy<CharacterDetailCalculationService>(() => new CharacterDetailCalculationService());

        private CharacterDetailCalculationService()
        {
        }

        public static CharacterDetailCalculationService Instance => s_instance.Value;

        public CharacterRuntimeSnapshotData BuildDisplaySnapshot(CharacterCardDraftSaveData character)
        {
            character = CharacterCardLocalRepository.Normalize(character);
            CharacterRuntimeSnapshotData snapshot = CharacterRuntimeSnapshotData.Clone(character.RuntimeSnapshot);
            snapshot.CharacterId = character.CharacterId ?? string.Empty;
            snapshot.CharacterName = FormatTextOrDefault(character.CharacterName, "\u672A\u547D\u540D\u89D2\u8272");
            snapshot.Alignment = character.Alignment ?? string.Empty;
            snapshot.Level = Math.Max(1, character.Level);
            snapshot.Experience = Math.Max(0, character.Experience);
            snapshot.RaceId = character.RaceId ?? string.Empty;
            snapshot.ClassId = character.ClassId ?? string.Empty;
            snapshot.BackgroundId = character.BackgroundId ?? string.Empty;
            snapshot.FeatId = character.FeatId ?? string.Empty;
            snapshot.SpellId = character.SpellId ?? string.Empty;
            snapshot.HpModeId = CharacterHpModeIds.Normalize(character.HpModeId);
            snapshot.MaxHp = character.MaxHp;
            snapshot.CurrentHp = CharacterCreationCalculationService.Instance.NormalizeCurrentHp(character.CurrentHp, character.MaxHp);
            snapshot.TemporaryHp = Math.Max(0, character.TemporaryHp);
            snapshot.DeathSaveSuccesses = character.DeathSaves != null ? Clamp(character.DeathSaves.Successes, 0, 3) : 0;
            snapshot.DeathSaveFailures = character.DeathSaves != null ? Clamp(character.DeathSaves.Failures, 0, 3) : 0;
            snapshot.ActiveConditions = BuildConditionSummary(character.Conditions);
            snapshot.ActiveResources = BuildResourceSummary(character.Resources);
            ApplyEquippedItemAcData(snapshot, character.Equipment);

            DndClassDefineData classData = FindClass(character.ClassId);
            DndRaceDefineData raceData = FindRace(character.RaceId);
            DndBackgroundDefineData backgroundData = FindBackground(character.BackgroundId);
            ApplyChoiceAbilityScoreIncreases(character.ChoiceSelections, snapshot);
            ApplyCharacterSkillProficiencyEffects(character, snapshot);
            AppendUniqueValues(snapshot.SkillProficiencyIds, backgroundData?.SkillProficiencies);
            AppendSkillSummaryValues(snapshot.SkillProficiencyIds, snapshot.Skills);
            AppendEquipmentSummaryValues(snapshot.ArmorProficiencyIds, snapshot.ArmorProficiencies);
            AppendEquipmentSummaryValues(snapshot.WeaponProficiencyIds, snapshot.WeaponProficiencies);
            AppendEquipmentSummaryValues(snapshot.ToolProficiencyIds, snapshot.ToolProficiencies);
            AppendUniqueValues(snapshot.ArmorProficiencyIds, classData?.ArmorProficiencies);
            AppendUniqueValues(snapshot.WeaponProficiencyIds, classData?.WeaponProficiencies);
            AppendUniqueValues(snapshot.ToolProficiencyIds, classData?.ToolProficiencies);
            AppendUniqueValues(snapshot.ToolProficiencyIds, backgroundData?.ToolProficiencies);
            ApplyFeatureEquipmentProficiencyEffects(snapshot, raceData?.FeatureIds);
            ApplyChoiceEquipmentProficiencyEffects(snapshot, character.ChoiceSelections);
            ApplySubclassEquipmentProficiencyEffects(snapshot, character);

            if (string.IsNullOrWhiteSpace(snapshot.ClassName))
            {
                snapshot.ClassName = classData?.Name ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(snapshot.RaceName))
            {
                snapshot.RaceName = raceData?.Name ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(snapshot.BackgroundName))
            {
                snapshot.BackgroundName = backgroundData?.Name ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(snapshot.Size))
            {
                snapshot.Size = raceData?.Size ?? string.Empty;
            }

            if (snapshot.Speed <= 0)
            {
                snapshot.Speed = raceData?.Speed ?? 0;
            }

            ApplyEquippedItemAttributeEffects(snapshot, character.Equipment);
            snapshot.ArmorClass = CalculateArmorClass(character, snapshot);

            if (string.IsNullOrWhiteSpace(snapshot.SavingThrows))
            {
                snapshot.SavingThrows = FormatList(classData?.SavingThrowProficiencies);
            }

            if (string.IsNullOrWhiteSpace(snapshot.Skills))
            {
                snapshot.Skills = FormatList(backgroundData?.SkillProficiencies);
            }

            if (string.IsNullOrWhiteSpace(snapshot.ArmorProficiencies))
            {
                snapshot.ArmorProficiencies = FormatList(snapshot.ArmorProficiencyIds);
            }

            if (string.IsNullOrWhiteSpace(snapshot.WeaponProficiencies))
            {
                snapshot.WeaponProficiencies = FormatList(snapshot.WeaponProficiencyIds);
            }

            if (string.IsNullOrWhiteSpace(snapshot.ToolProficiencies))
            {
                snapshot.ToolProficiencies = FormatList(snapshot.ToolProficiencyIds);
            }

            if (string.IsNullOrWhiteSpace(snapshot.Languages))
            {
                List<string> languages = new List<string>();
                AppendUniqueValues(languages, raceData?.LanguageIds);
                AppendUniqueValues(languages, backgroundData?.LanguageIds);
                AppendChoiceLanguageIds(languages, character.ChoiceSelections);
                snapshot.Languages = FormatLanguageList(languages);
            }

            if (string.IsNullOrWhiteSpace(snapshot.Traits))
            {
                snapshot.Traits = JoinNonEmpty(
                    BuildRaceFeatureSummary(character.RaceId),
                    BuildBackgroundFeatureSummary(character.BackgroundId));
            }

            return snapshot;
        }

        public CharacterCombatOverviewViewState BuildCombatOverview(CharacterCardDraftSaveData character, CharacterRuntimeSnapshotData snapshot)
        {
            CharacterCombatOverviewViewState state = new CharacterCombatOverviewViewState();
            if (snapshot == null)
            {
                return state;
            }

            state.ArmorClass = snapshot.ArmorClass > 0 ? snapshot.ArmorClass : CalculateArmorClass(character, snapshot);
            state.InitiativeBonus = CalculateInitiativeBonus(character, snapshot);
            state.Speed = Math.Max(0, snapshot.Speed);
            state.PassivePerception = CalculatePassivePerception(character, snapshot);
            state.HasSpellcasting = TryCalculateSpellcastingNumbers(character, snapshot, out int spellSaveDc, out int spellAttackBonus);
            state.SpellSaveDc = spellSaveDc;
            state.SpellAttackBonus = spellAttackBonus;
            return state;
        }

        public CharacterAbilityDisplayViewState BuildAbilityDisplay(CharacterRuntimeSnapshotData snapshot)
        {
            snapshot ??= new CharacterRuntimeSnapshotData();
            int strength = NormalizeAbility(snapshot.Strength);
            int dexterity = NormalizeAbility(snapshot.Dexterity);
            int constitution = NormalizeAbility(snapshot.Constitution);
            int intelligence = NormalizeAbility(snapshot.Intelligence);
            int wisdom = NormalizeAbility(snapshot.Wisdom);
            int charisma = NormalizeAbility(snapshot.Charisma);

            return new CharacterAbilityDisplayViewState
            {
                Strength = strength,
                Dexterity = dexterity,
                Constitution = constitution,
                Intelligence = intelligence,
                Wisdom = wisdom,
                Charisma = charisma,
                StrengthModifier = CharacterCreationCalculationService.Instance.FormatSigned(CalculateAbilityModifier(strength)),
                DexterityModifier = CharacterCreationCalculationService.Instance.FormatSigned(CalculateAbilityModifier(dexterity)),
                ConstitutionModifier = CharacterCreationCalculationService.Instance.FormatSigned(CalculateAbilityModifier(constitution)),
                IntelligenceModifier = CharacterCreationCalculationService.Instance.FormatSigned(CalculateAbilityModifier(intelligence)),
                WisdomModifier = CharacterCreationCalculationService.Instance.FormatSigned(CalculateAbilityModifier(wisdom)),
                CharismaModifier = CharacterCreationCalculationService.Instance.FormatSigned(CalculateAbilityModifier(charisma))
            };
        }

        public CharacterHpDisplayViewState BuildHpDisplay(CharacterRuntimeSnapshotData snapshot)
        {
            snapshot ??= new CharacterRuntimeSnapshotData();
            int maxHp = Math.Max(0, snapshot.MaxHp);
            return new CharacterHpDisplayViewState
            {
                CurrentHp = CharacterCreationCalculationService.Instance.NormalizeCurrentHp(snapshot.CurrentHp, maxHp),
                MaxHp = maxHp,
                TemporaryHp = Math.Max(0, snapshot.TemporaryHp)
            };
        }

        public CharacterExperienceDisplayViewState BuildExperienceDisplay(int experience, int level)
        {
            int normalizedExperience = Math.Max(0, experience);
            int currentLevel = Math.Max(1, level);
            int currentLevelXp = CharacterCreationCalculationService.Instance.GetExperienceThreshold(currentLevel);
            int nextLevelXp = CharacterCreationCalculationService.Instance.GetExperienceThreshold(currentLevel + 1);
            float progress = nextLevelXp > currentLevelXp
                ? (normalizedExperience - currentLevelXp) / (float)(nextLevelXp - currentLevelXp)
                : 1f;

            return new CharacterExperienceDisplayViewState
            {
                Experience = normalizedExperience,
                NextLevelExperience = nextLevelXp,
                Progress = Math.Max(0f, Math.Min(1f, progress)),
                Label = nextLevelXp > currentLevelXp
                    ? $"{normalizedExperience}/{nextLevelXp}"
                    : normalizedExperience.ToString()
            };
        }

        public CharacterProficiencyBonusDisplayViewState BuildProficiencyBonusDisplay(int level)
        {
            int bonus = CalculateProficiencyBonus(level);
            return new CharacterProficiencyBonusDisplayViewState
            {
                Bonus = bonus,
                Label = bonus.ToString()
            };
        }

        public CharacterSkillBonusViewState BuildSkillBonus(
            CharacterCardDraftSaveData character,
            CharacterRuntimeSnapshotData snapshot,
            string skillId,
            string displayName,
            AbilityKind ability)
        {
            CharacterSkillBonusViewState state = new CharacterSkillBonusViewState();
            if (snapshot == null)
            {
                return state;
            }

            int abilityModifier = GetAbilityModifier(snapshot, ability);
            int proficiencyBonus = CalculateProficiencyBonus(snapshot.Level);
            state.HasExpertise = ContainsId(snapshot.SkillExpertiseIds, skillId, displayName);
            state.HasProficiency = state.HasExpertise || ContainsId(snapshot.SkillProficiencyIds, skillId, displayName);
            state.Bonus = abilityModifier;
            if (state.HasExpertise)
            {
                state.Bonus += proficiencyBonus * 2;
            }
            else if (state.HasProficiency)
            {
                state.Bonus += proficiencyBonus;
            }

            state.Bonus += CalculateCharacterAndItemEffectBonus(character, snapshot, "SkillBonus", skillId, displayName);
            return state;
        }

        public void ApplyEquippedItemAcData(CharacterRuntimeSnapshotData snapshot, CharacterEquipmentSetSaveData equipment)
        {
            if (snapshot == null || equipment == null)
            {
                return;
            }

            CharacterEquipmentItemSaveData armor = equipment.Armor;
            if (CharacterEquipmentItemSaveData.HasItem(armor))
            {
                snapshot.ArmorCategory = CharacterArmorCategoryIds.Normalize(armor.ArmorCategory);
                if (armor.ArmorBaseAc > 0)
                {
                    snapshot.ArmorBaseAc = armor.ArmorBaseAc;
                }

                snapshot.EquipmentAcBonus += CalculateItemAcBonus(armor, snapshot);
            }

            CharacterEquipmentItemSaveData shield = equipment.Shield;
            if (CharacterEquipmentItemSaveData.HasItem(shield))
            {
                snapshot.ShieldAcBonus += Math.Max(0, shield.ArmorBaseAc) + CalculateItemAcBonus(shield, snapshot);
            }

            AppendEquippedItemAcBonus(snapshot, equipment.EquippedItems);
        }

        public void ApplyEquippedItemAttributeEffects(CharacterRuntimeSnapshotData snapshot, CharacterEquipmentSetSaveData equipment)
        {
            if (snapshot == null || equipment == null)
            {
                return;
            }

            ApplyItemAttributeEffects(snapshot, equipment.Armor);
            ApplyItemAttributeEffects(snapshot, equipment.Shield);
            if (equipment.EquippedItems == null)
            {
                return;
            }

            for (int index = 0; index < equipment.EquippedItems.Count; index++)
            {
                CharacterEquipmentItemSaveData item = equipment.EquippedItems[index];
                if (CharacterEquipmentItemSaveData.HasItem(item) && item.IsEquipped)
                {
                    ApplyItemAttributeEffects(snapshot, item);
                }
            }
        }

        public int CalculateArmorClass(CharacterCardDraftSaveData character, CharacterRuntimeSnapshotData snapshot)
        {
            if (snapshot == null)
            {
                return 0;
            }

            int dexterityModifier = CalculateAbilityModifier(NormalizeAbility(snapshot.Dexterity));
            int armorBaseAc = snapshot.ArmorBaseAc > 0 ? snapshot.ArmorBaseAc : 10;
            int dexterityAcBonus = CalculateArmorDexterityAcBonus(snapshot.ArmorCategory, dexterityModifier);
            int featureAcBonus = snapshot.FeatureAcBonus + CalculateStructuredEffectBonus(character, snapshot, "ACBonus", "AC");
            return armorBaseAc
                + dexterityAcBonus
                + snapshot.EquipmentAcBonus
                + snapshot.ShieldAcBonus
                + featureAcBonus
                + snapshot.SkillAcBonus;
        }

        private static string BuildConditionSummary(IReadOnlyList<CharacterConditionStateSaveData> conditions)
        {
            if (conditions == null || conditions.Count == 0)
            {
                return string.Empty;
            }

            List<string> labels = new List<string>();
            for (int index = 0; index < conditions.Count; index++)
            {
                CharacterConditionStateSaveData condition = conditions[index];
                if (condition == null)
                {
                    continue;
                }

                string label = FirstNonEmpty(condition.Name, condition.ConditionId);
                if (condition.ExhaustionLevel > 0)
                {
                    label = string.IsNullOrWhiteSpace(label)
                        ? $"Exhaustion {condition.ExhaustionLevel}"
                        : $"{label} {condition.ExhaustionLevel}";
                }

                if (!string.IsNullOrWhiteSpace(label))
                {
                    labels.Add(label);
                }
            }

            return labels.Count > 0 ? string.Join(" / ", labels) : string.Empty;
        }

        private static string BuildResourceSummary(IReadOnlyList<CharacterResourceSaveData> resources)
        {
            if (resources == null || resources.Count == 0)
            {
                return string.Empty;
            }

            List<string> labels = new List<string>();
            for (int index = 0; index < resources.Count; index++)
            {
                CharacterResourceSaveData resource = resources[index];
                if (resource == null)
                {
                    continue;
                }

                string label = FirstNonEmpty(resource.Name, resource.ResourceId);
                if (!string.IsNullOrWhiteSpace(label))
                {
                    labels.Add($"{label} {resource.Current}/{resource.Maximum}");
                }
            }

            return labels.Count > 0 ? string.Join(" / ", labels) : string.Empty;
        }

        private static string BuildRaceFeatureSummary(string raceId)
        {
            DndRaceDefineData raceData = FindRace(raceId);
            return raceData == null ? string.Empty : BuildFeatureNameSummary(raceData.FeatureIds);
        }

        private static string BuildBackgroundFeatureSummary(string backgroundId)
        {
            DndBackgroundDefineData backgroundData = FindBackground(backgroundId);
            return backgroundData == null ? string.Empty : BuildFeatureNameSummary(backgroundData.FeatureIds);
        }

        private static string BuildFeatureNameSummary(IReadOnlyList<string> featureIds)
        {
            if (featureIds == null || featureIds.Count == 0)
            {
                return string.Empty;
            }

            List<string> names = new List<string>();
            for (int index = 0; index < featureIds.Count; index++)
            {
                string featureId = featureIds[index];
                if (string.IsNullOrWhiteSpace(featureId))
                {
                    continue;
                }

                if (DndRuleContentService.Instance.TryGetFeature(featureId, out DndFeatureDefineData feature)
                    && !string.IsNullOrWhiteSpace(feature.Name))
                {
                    names.Add(feature.Name.Trim());
                }
                else
                {
                    names.Add(featureId.Trim());
                }
            }

            return names.Count > 0 ? string.Join("\n", names) : string.Empty;
        }

        private static void ApplyChoiceEquipmentProficiencyEffects(
            CharacterRuntimeSnapshotData snapshot,
            IReadOnlyList<CharacterChoiceSelectionSaveData> choiceSelections)
        {
            if (snapshot == null || choiceSelections == null)
            {
                return;
            }

            for (int index = 0; index < choiceSelections.Count; index++)
            {
                CharacterChoiceSelectionSaveData selection = choiceSelections[index];
                if (selection == null || string.IsNullOrWhiteSpace(selection.ChoiceGroupId) || string.IsNullOrWhiteSpace(selection.OptionId))
                {
                    continue;
                }

                IReadOnlyList<DndChoiceOptionData> options = DndRuleContentService.Instance.GetChoiceOptions(selection.ChoiceGroupId.Trim());
                for (int optionIndex = 0; optionIndex < options.Count; optionIndex++)
                {
                    DndChoiceOptionData option = options[optionIndex];
                    if (option == null || !string.Equals(option.OptionId, selection.OptionId, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    ApplyFeatureEquipmentProficiencyEffects(snapshot, option.GrantFeatureIds);
                    ApplyEquipmentProficiencyEffects(snapshot, option.GrantEffectIds);
                    break;
                }
            }
        }

        private static void ApplyFeatureEquipmentProficiencyEffects(CharacterRuntimeSnapshotData snapshot, IReadOnlyList<string> featureIds)
        {
            if (snapshot == null || featureIds == null)
            {
                return;
            }

            for (int index = 0; index < featureIds.Count; index++)
            {
                string featureId = featureIds[index];
                if (string.IsNullOrWhiteSpace(featureId))
                {
                    continue;
                }

                if (DndRuleContentService.Instance.TryGetFeature(featureId, out DndFeatureDefineData feature))
                {
                    ApplyEquipmentProficiencyEffects(snapshot, feature.EffectIds);
                }
            }
        }

        private static void ApplySubclassEquipmentProficiencyEffects(CharacterRuntimeSnapshotData snapshot, CharacterCardDraftSaveData character)
        {
            if (snapshot == null || character == null)
            {
                return;
            }

            List<CharacterClassProgressSaveData> progresses = GetCharacterClassProgresses(character);
            for (int progressIndex = 0; progressIndex < progresses.Count; progressIndex++)
            {
                CharacterClassProgressSaveData progress = progresses[progressIndex];
                if (progress == null || string.IsNullOrWhiteSpace(progress.SubclassId))
                {
                    continue;
                }

                int classLevel = Math.Max(1, progress.Level);
                IReadOnlyList<DndSubclassLevelProgressionData> progressions = DndRuleContentService.Instance.GetSubclassProgressions(progress.SubclassId.Trim());
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

                    ApplyFeatureEquipmentProficiencyEffects(snapshot, progression.FeatureIds);
                }
            }
        }

        private static void ApplyEquipmentProficiencyEffects(CharacterRuntimeSnapshotData snapshot, IReadOnlyList<string> effectIds)
        {
            if (snapshot == null || effectIds == null)
            {
                return;
            }

            for (int index = 0; index < effectIds.Count; index++)
            {
                string effectId = effectIds[index];
                if (string.IsNullOrWhiteSpace(effectId))
                {
                    continue;
                }

                if (DndRuleContentService.Instance.TryGetFeatureEffect(effectId, out DndFeatureEffectData effect))
                {
                    ApplyEquipmentProficiencyEffect(snapshot, effect);
                }
            }
        }

        private static void ApplyEquipmentProficiencyEffect(CharacterRuntimeSnapshotData snapshot, DndFeatureEffectData effect)
        {
            if (snapshot == null || effect == null || string.IsNullOrWhiteSpace(effect.EffectType))
            {
                return;
            }

            if (string.Equals(effect.EffectType, "ArmorProficiency", StringComparison.OrdinalIgnoreCase))
            {
                AppendEquipmentSummaryValues(snapshot.ArmorProficiencyIds, effect.Target);
            }
            else if (string.Equals(effect.EffectType, "WeaponProficiency", StringComparison.OrdinalIgnoreCase))
            {
                AppendEquipmentSummaryValues(snapshot.WeaponProficiencyIds, effect.Target);
            }
            else if (string.Equals(effect.EffectType, "ToolProficiency", StringComparison.OrdinalIgnoreCase))
            {
                AppendEquipmentSummaryValues(snapshot.ToolProficiencyIds, effect.Target);
            }
        }

        private void ApplyCharacterSkillProficiencyEffects(CharacterCardDraftSaveData character, CharacterRuntimeSnapshotData snapshot)
        {
            if (character == null || snapshot == null)
            {
                return;
            }

            ApplyCharacterEffects(character, effect =>
            {
                if (effect != null && string.Equals(effect.EffectType, "SkillProficiency", StringComparison.OrdinalIgnoreCase))
                {
                    AppendSkillSummaryValues(snapshot.SkillProficiencyIds, effect.Target);
                }
            });
        }

        private static void AppendUniqueValues(List<string> target, IReadOnlyList<string> values)
        {
            if (target == null || values == null)
            {
                return;
            }

            for (int index = 0; index < values.Count; index++)
            {
                AppendUniqueValue(target, values[index]);
            }
        }

        private static void AppendUniqueValue(List<string> target, string value)
        {
            if (target == null || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            string normalized = value.Trim();
            if (!target.Exists(item => string.Equals(item, normalized, StringComparison.OrdinalIgnoreCase)))
            {
                target.Add(normalized);
            }
        }

        private static bool ContainsExactString(IReadOnlyList<string> values, string value)
        {
            if (values == null || string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string normalized = value.Trim();
            for (int index = 0; index < values.Count; index++)
            {
                if (string.Equals(values[index]?.Trim(), normalized, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static void AppendSkillSummaryValues(List<string> target, string summary)
        {
            if (target == null || string.IsNullOrWhiteSpace(summary))
            {
                return;
            }

            string[] parts = SplitSummary(summary);
            for (int index = 0; index < parts.Length; index++)
            {
                string normalized = NormalizeSkillId(parts[index]);
                if (!string.IsNullOrWhiteSpace(normalized))
                {
                    AppendUniqueValue(target, normalized);
                }
            }
        }

        private static void AppendEquipmentSummaryValues(List<string> target, string summary)
        {
            if (target == null || string.IsNullOrWhiteSpace(summary))
            {
                return;
            }

            string[] parts = SplitSummary(summary);
            for (int index = 0; index < parts.Length; index++)
            {
                string value = parts[index]?.Trim();
                if (string.IsNullOrWhiteSpace(value) || string.Equals(value, "\u65E0", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                AppendUniqueValue(target, value);
            }
        }

        private static string[] SplitSummary(string summary)
        {
            return summary.Split(new[] { ',', '\uFF0C', ';', '\uFF1B', '/', '\u3001', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static void ApplyChoiceAbilityScoreIncreases(IReadOnlyList<CharacterChoiceSelectionSaveData> choiceSelections, CharacterRuntimeSnapshotData snapshot)
        {
            if (choiceSelections == null || snapshot == null)
            {
                return;
            }

            for (int index = 0; index < choiceSelections.Count; index++)
            {
                CharacterChoiceSelectionSaveData selection = choiceSelections[index];
                if (selection == null
                    || string.IsNullOrWhiteSpace(selection.ChoiceGroupId)
                    || string.IsNullOrWhiteSpace(selection.OptionId)
                    || !DndRuleContentService.Instance.TryGetChoiceGroup(selection.ChoiceGroupId.Trim(), out DndChoiceGroupData choiceGroup)
                    || choiceGroup == null
                    || !IsAbilityScoreChoiceGroup(choiceGroup))
                {
                    continue;
                }

                int value = choiceGroup.ValuePerSelection != 0 ? choiceGroup.ValuePerSelection : 1;
                AddAbilityScoreValue(snapshot, selection.OptionId, value, choiceGroup.TargetValueCap);
            }
        }

        private static void AddAbilityScoreValue(CharacterRuntimeSnapshotData snapshot, string abilityId, int value, int cap)
        {
            if (snapshot == null || string.IsNullOrWhiteSpace(abilityId) || value == 0)
            {
                return;
            }

            string normalized = NormalizeAbilityId(abilityId);
            if (string.Equals(normalized, "Strength", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.Strength = AddCappedValue(snapshot.Strength, value, cap);
            }
            else if (string.Equals(normalized, "Dexterity", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.Dexterity = AddCappedValue(snapshot.Dexterity, value, cap);
            }
            else if (string.Equals(normalized, "Constitution", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.Constitution = AddCappedValue(snapshot.Constitution, value, cap);
            }
            else if (string.Equals(normalized, "Intelligence", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.Intelligence = AddCappedValue(snapshot.Intelligence, value, cap);
            }
            else if (string.Equals(normalized, "Wisdom", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.Wisdom = AddCappedValue(snapshot.Wisdom, value, cap);
            }
            else if (string.Equals(normalized, "Charisma", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.Charisma = AddCappedValue(snapshot.Charisma, value, cap);
            }
        }

        private static int AddCappedValue(int current, int value, int cap)
        {
            int next = current + value;
            return cap > 0 ? Math.Min(cap, next) : next;
        }

        private static string FormatList(IReadOnlyList<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return string.Empty;
            }

            List<string> labels = new List<string>();
            for (int index = 0; index < values.Count; index++)
            {
                if (!string.IsNullOrWhiteSpace(values[index]))
                {
                    labels.Add(values[index].Trim());
                }
            }

            return labels.Count > 0 ? string.Join("\u3001", labels) : string.Empty;
        }

        private static string FormatLanguageList(IReadOnlyList<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return string.Empty;
            }

            List<string> labels = new List<string>();
            for (int index = 0; index < values.Count; index++)
            {
                string languageId = values[index]?.Trim();
                if (string.IsNullOrWhiteSpace(languageId))
                {
                    continue;
                }

                if (DndRuleContentService.Instance.TryGetLanguage(languageId, out DndLanguageDefineData language)
                    && language != null)
                {
                    labels.Add(FirstNonEmpty(language.Name, language.LanguageId));
                }
                else
                {
                    labels.Add(languageId);
                }
            }

            return labels.Count > 0 ? string.Join(" / ", labels) : string.Empty;
        }

        private static void AppendChoiceLanguageIds(List<string> target, IReadOnlyList<CharacterChoiceSelectionSaveData> choiceSelections)
        {
            if (target == null || choiceSelections == null)
            {
                return;
            }

            for (int index = 0; index < choiceSelections.Count; index++)
            {
                CharacterChoiceSelectionSaveData selection = choiceSelections[index];
                if (selection == null
                    || string.IsNullOrWhiteSpace(selection.ChoiceGroupId)
                    || string.IsNullOrWhiteSpace(selection.OptionId))
                {
                    continue;
                }

                if (!DndRuleContentService.Instance.TryGetChoiceGroup(selection.ChoiceGroupId.Trim(), out DndChoiceGroupData choiceGroup)
                    || choiceGroup == null
                    || !string.Equals(choiceGroup.ChoiceType, "Language", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                AppendUniqueValue(target, selection.OptionId.Trim());
            }
        }

        private static string JoinNonEmpty(params string[] values)
        {
            List<string> parts = new List<string>();
            if (values != null)
            {
                for (int index = 0; index < values.Length; index++)
                {
                    string value = values[index];
                    if (!string.IsNullOrWhiteSpace(value) && value.Trim() != "\u65E0")
                    {
                        parts.Add(value.Trim());
                    }
                }
            }

            return parts.Count > 0 ? string.Join(" / ", parts) : string.Empty;
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            for (int index = 0; index < values.Length; index++)
            {
                if (!string.IsNullOrWhiteSpace(values[index]))
                {
                    return values[index].Trim();
                }
            }

            return string.Empty;
        }

        private static string FormatTextOrDefault(string value, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            return value > max ? max : value;
        }

        private static void AppendEquippedItemAcBonus(CharacterRuntimeSnapshotData snapshot, IReadOnlyList<CharacterEquipmentItemSaveData> items)
        {
            if (snapshot == null || items == null)
            {
                return;
            }

            for (int index = 0; index < items.Count; index++)
            {
                CharacterEquipmentItemSaveData item = items[index];
                if (CharacterEquipmentItemSaveData.HasItem(item) && item.IsEquipped)
                {
                    snapshot.EquipmentAcBonus += CalculateItemAcBonus(item, snapshot);
                }
            }
        }

        private static void ApplyItemAttributeEffects(CharacterRuntimeSnapshotData snapshot, CharacterEquipmentItemSaveData item)
        {
            if (snapshot == null
                || !CharacterEquipmentItemSaveData.HasItem(item)
                || !IsCharacterItemEffectConditionMet(item.EffectApplyCondition, snapshot))
            {
                return;
            }

            ApplyRuleItemAttributeEffects(snapshot, item.EffectIds);
            ApplyCustomItemAttributeEffects(snapshot, item.CustomEffects);
        }

        private static void ApplyRuleItemAttributeEffects(CharacterRuntimeSnapshotData snapshot, IReadOnlyList<string> effectIds)
        {
            if (effectIds == null)
            {
                return;
            }

            for (int index = 0; index < effectIds.Count; index++)
            {
                string effectId = effectIds[index];
                if (string.IsNullOrWhiteSpace(effectId)
                    || !DndRuleContentService.Instance.TryGetFeatureEffect(effectId, out DndFeatureEffectData effect)
                    || !IsStructuredEffectConditionMet(effect, snapshot))
                {
                    continue;
                }

                ApplyItemAttributeEffect(snapshot, effect.EffectType, effect.Target, effect.Value, effect.Condition);
            }
        }

        private static void ApplyCustomItemAttributeEffects(CharacterRuntimeSnapshotData snapshot, IReadOnlyList<CharacterItemEffectSaveData> effects)
        {
            if (effects == null)
            {
                return;
            }

            for (int index = 0; index < effects.Count; index++)
            {
                CharacterItemEffectSaveData effect = effects[index];
                if (effect == null || !IsCharacterItemEffectConditionMet(effect.Condition, snapshot))
                {
                    continue;
                }

                ApplyItemAttributeEffect(snapshot, effect.EffectType, effect.Target, effect.Value, effect.Condition);
            }
        }

        private static void ApplyItemAttributeEffect(
            CharacterRuntimeSnapshotData snapshot,
            string effectType,
            string target,
            string valueText,
            string condition)
        {
            if (snapshot == null
                || string.IsNullOrWhiteSpace(effectType)
                || !IsCharacterItemEffectConditionMet(condition, snapshot)
                || !int.TryParse(valueText, out int value))
            {
                return;
            }

            if (string.Equals(effectType, "AbilityScoreBonus", StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectType, "AbilityBonus", StringComparison.OrdinalIgnoreCase))
            {
                ApplyAbilityScoreBonus(snapshot, target, value);
            }
            else if (string.Equals(effectType, "SpeedBonus", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.Speed = Math.Max(0, snapshot.Speed + value);
            }
            else if (string.Equals(effectType, "InitiativeBonus", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.InitiativeBonus += value;
            }
            else if (string.Equals(effectType, "SpellAttackBonus", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.SpellAttackBonus += value;
            }
            else if (string.Equals(effectType, "SpellSaveDcBonus", StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectType, "SpellSaveDCBonus", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.SpellSaveDcBonus += value;
            }
            else if (string.Equals(effectType, "AttackBonus", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.AttackBonus += value;
            }
            else if (string.Equals(effectType, "WeaponAttackBonus", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.WeaponAttackBonus += value;
            }
            else if (string.Equals(effectType, "DamageBonus", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.DamageBonus += value;
            }
            else if (string.Equals(effectType, "SavingThrowBonus", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.SavingThrowBonus += value;
            }
        }

        private static void ApplyAbilityScoreBonus(CharacterRuntimeSnapshotData snapshot, string target, int value)
        {
            string normalized = target?.Trim() ?? string.Empty;
            if (string.Equals(normalized, "All", StringComparison.OrdinalIgnoreCase)
                || normalized == "\u5168\u90E8")
            {
                snapshot.Strength += value;
                snapshot.Dexterity += value;
                snapshot.Constitution += value;
                snapshot.Intelligence += value;
                snapshot.Wisdom += value;
                snapshot.Charisma += value;
                return;
            }

            normalized = NormalizeAbilityId(normalized);
            if (string.Equals(normalized, "Strength", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.Strength += value;
            }
            else if (string.Equals(normalized, "Dexterity", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.Dexterity += value;
            }
            else if (string.Equals(normalized, "Constitution", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.Constitution += value;
            }
            else if (string.Equals(normalized, "Intelligence", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.Intelligence += value;
            }
            else if (string.Equals(normalized, "Wisdom", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.Wisdom += value;
            }
            else if (string.Equals(normalized, "Charisma", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.Charisma += value;
            }
        }

        private static int CalculateItemAcBonus(CharacterEquipmentItemSaveData item, CharacterRuntimeSnapshotData snapshot)
        {
            if (!CharacterEquipmentItemSaveData.HasItem(item))
            {
                return 0;
            }

            if (!IsCharacterItemEffectConditionMet(item.EffectApplyCondition, snapshot))
            {
                return 0;
            }

            int bonus = item.AcBonus;
            bonus += CalculateRuleEffectBonus(item.EffectIds, snapshot, "ACBonus", "AC");
            bonus += CalculateCustomItemEffectBonus(item.CustomEffects, snapshot, "ACBonus", "AC");
            return bonus;
        }

        private static int CalculateRuleEffectBonus(IReadOnlyList<string> effectIds, CharacterRuntimeSnapshotData snapshot, string effectType, string target)
        {
            if (effectIds == null)
            {
                return 0;
            }

            int bonus = 0;
            for (int index = 0; index < effectIds.Count; index++)
            {
                string effectId = effectIds[index];
                if (string.IsNullOrWhiteSpace(effectId)
                    || !DndRuleContentService.Instance.TryGetFeatureEffect(effectId, out DndFeatureEffectData effect)
                    || !IsAcBonusEffect(effect.EffectType, effect.Target, effectType, target)
                    || !IsStructuredEffectConditionMet(effect, snapshot))
                {
                    continue;
                }

                if (int.TryParse(effect.Value, out int value))
                {
                    bonus += value;
                }
            }

            return bonus;
        }

        private static int CalculateCustomItemEffectBonus(
            IReadOnlyList<CharacterItemEffectSaveData> effects,
            CharacterRuntimeSnapshotData snapshot,
            string effectType,
            string target)
        {
            if (effects == null)
            {
                return 0;
            }

            int bonus = 0;
            for (int index = 0; index < effects.Count; index++)
            {
                CharacterItemEffectSaveData effect = effects[index];
                if (effect == null
                    || !IsAcBonusEffect(effect.EffectType, effect.Target, effectType, target)
                    || !IsCharacterItemEffectConditionMet(effect.Condition, snapshot))
                {
                    continue;
                }

                if (int.TryParse(effect.Value, out int value))
                {
                    bonus += value;
                }
            }

            return bonus;
        }

        private static bool IsAcBonusEffect(string actualEffectType, string actualTarget, string expectedEffectType, string expectedTarget)
        {
            return string.Equals(actualEffectType, expectedEffectType, StringComparison.OrdinalIgnoreCase)
                && (string.IsNullOrWhiteSpace(expectedTarget)
                    || string.Equals(actualTarget, expectedTarget, StringComparison.OrdinalIgnoreCase));
        }

        private int CalculateInitiativeBonus(CharacterCardDraftSaveData character, CharacterRuntimeSnapshotData snapshot)
        {
            if (snapshot == null)
            {
                return 0;
            }

            int dexterityModifier = CalculateAbilityModifier(NormalizeAbility(snapshot.Dexterity));
            return dexterityModifier
                + snapshot.InitiativeBonus
                + CalculateStructuredEffectBonus(character, snapshot, "InitiativeBonus", "Initiative");
        }

        private int CalculatePassivePerception(CharacterCardDraftSaveData character, CharacterRuntimeSnapshotData snapshot)
        {
            if (snapshot == null)
            {
                return 10;
            }

            CharacterSkillBonusViewState perception = BuildSkillBonus(character, snapshot, "perception", "瀵熻", AbilityKind.Wisdom);
            return 10 + perception.Bonus;
        }

        private bool TryCalculateSpellcastingNumbers(
            CharacterCardDraftSaveData character,
            CharacterRuntimeSnapshotData snapshot,
            out int spellSaveDc,
            out int spellAttackBonus)
        {
            spellSaveDc = 0;
            spellAttackBonus = 0;
            if (snapshot == null)
            {
                return false;
            }

            string abilityId = FindSpellcastingAbilityId(character);
            if (string.IsNullOrWhiteSpace(abilityId))
            {
                return false;
            }

            int abilityScore = GetAbilityScore(snapshot, abilityId);
            if (abilityScore <= 0)
            {
                return false;
            }

            int abilityModifier = CalculateAbilityModifier(NormalizeAbility(abilityScore));
            spellAttackBonus = CalculateProficiencyBonus(snapshot.Level) + abilityModifier + snapshot.AttackBonus + snapshot.SpellAttackBonus;
            spellSaveDc = 8 + spellAttackBonus + snapshot.SpellSaveDcBonus;
            return true;
        }

        public int CalculateCharacterAndItemEffectBonus(
            CharacterCardDraftSaveData character,
            CharacterRuntimeSnapshotData snapshot,
            string effectType,
            params string[] targets)
        {
            int bonus = CalculateStructuredEffectBonus(character, snapshot, effectType, targets);
            bonus += CalculateEquippedItemEffectBonus(character?.Equipment, snapshot, effectType, targets);
            return bonus;
        }

        private int CalculateStructuredEffectBonus(
            CharacterCardDraftSaveData character,
            CharacterRuntimeSnapshotData snapshot,
            string effectType,
            string target)
        {
            return CalculateStructuredEffectBonus(character, snapshot, effectType, new[] { target });
        }

        private int CalculateStructuredEffectBonus(
            CharacterCardDraftSaveData character,
            CharacterRuntimeSnapshotData snapshot,
            string effectType,
            IReadOnlyList<string> targets)
        {
            if (character == null || string.IsNullOrWhiteSpace(effectType))
            {
                return 0;
            }

            int bonus = 0;
            ApplyCharacterEffects(character, effect =>
            {
                if (effect == null
                    || !IsEffectMatch(effect.EffectType, effect.Target, effectType, targets)
                    || !IsStructuredEffectConditionMet(effect, snapshot))
                {
                    return;
                }

                if (int.TryParse(effect.Value, out int value))
                {
                    bonus += value;
                }
            });

            return bonus;
        }

        private static int CalculateEquippedItemEffectBonus(
            CharacterEquipmentSetSaveData equipment,
            CharacterRuntimeSnapshotData snapshot,
            string effectType,
            IReadOnlyList<string> targets)
        {
            if (equipment == null || string.IsNullOrWhiteSpace(effectType))
            {
                return 0;
            }

            int bonus = 0;
            bonus += CalculateItemEffectBonus(equipment.Armor, snapshot, effectType, targets);
            bonus += CalculateItemEffectBonus(equipment.Shield, snapshot, effectType, targets);

            if (equipment.EquippedItems == null)
            {
                return bonus;
            }

            for (int index = 0; index < equipment.EquippedItems.Count; index++)
            {
                CharacterEquipmentItemSaveData item = equipment.EquippedItems[index];
                if (CharacterEquipmentItemSaveData.HasItem(item) && item.IsEquipped)
                {
                    bonus += CalculateItemEffectBonus(item, snapshot, effectType, targets);
                }
            }

            return bonus;
        }

        private static int CalculateItemEffectBonus(
            CharacterEquipmentItemSaveData item,
            CharacterRuntimeSnapshotData snapshot,
            string effectType,
            IReadOnlyList<string> targets)
        {
            if (!CharacterEquipmentItemSaveData.HasItem(item)
                || !IsCharacterItemEffectConditionMet(item.EffectApplyCondition, snapshot))
            {
                return 0;
            }

            int bonus = 0;
            bonus += CalculateRuleItemEffectBonus(item.EffectIds, snapshot, effectType, targets);
            bonus += CalculateCustomItemEffectBonus(item.CustomEffects, snapshot, effectType, targets);
            return bonus;
        }

        private static int CalculateRuleItemEffectBonus(
            IReadOnlyList<string> effectIds,
            CharacterRuntimeSnapshotData snapshot,
            string effectType,
            IReadOnlyList<string> targets)
        {
            if (effectIds == null)
            {
                return 0;
            }

            int bonus = 0;
            for (int index = 0; index < effectIds.Count; index++)
            {
                string effectId = effectIds[index];
                if (string.IsNullOrWhiteSpace(effectId)
                    || !DndRuleContentService.Instance.TryGetFeatureEffect(effectId, out DndFeatureEffectData effect)
                    || !IsEffectMatch(effect.EffectType, effect.Target, effectType, targets)
                    || !IsStructuredEffectConditionMet(effect, snapshot)
                    || !int.TryParse(effect.Value, out int value))
                {
                    continue;
                }

                bonus += value;
            }

            return bonus;
        }

        private static int CalculateCustomItemEffectBonus(
            IReadOnlyList<CharacterItemEffectSaveData> effects,
            CharacterRuntimeSnapshotData snapshot,
            string effectType,
            IReadOnlyList<string> targets)
        {
            if (effects == null)
            {
                return 0;
            }

            int bonus = 0;
            for (int index = 0; index < effects.Count; index++)
            {
                CharacterItemEffectSaveData effect = effects[index];
                if (effect == null
                    || !IsEffectMatch(effect.EffectType, effect.Target, effectType, targets)
                    || !IsCharacterItemEffectConditionMet(effect.Condition, snapshot)
                    || !int.TryParse(effect.Value, out int value))
                {
                    continue;
                }

                bonus += value;
            }

            return bonus;
        }

        private void ApplyCharacterEffects(CharacterCardDraftSaveData character, Action<DndFeatureEffectData> action)
        {
            if (character == null || action == null)
            {
                return;
            }

            DndRaceDefineData raceData = FindRace(character.RaceId);
            ApplyFeatureEffects(raceData?.FeatureIds, action);

            DndFeatDefineData featData = FindFeat(character.FeatId);
            ApplyFeatureEffects(featData?.FeatureIds, action);

            List<CharacterClassProgressSaveData> progresses = GetCharacterClassProgresses(character);
            for (int progressIndex = 0; progressIndex < progresses.Count; progressIndex++)
            {
                CharacterClassProgressSaveData progress = progresses[progressIndex];
                int classLevel = Math.Max(1, progress.Level);
                for (int level = 1; level <= classLevel; level++)
                {
                    if (DndRuleContentService.Instance.TryGetClassLevelProgression(progress.ClassId, level, out DndLevelProgressionData progression))
                    {
                        ApplyFeatureEffects(progression.FeatureIds, action);
                    }
                }

                ApplySubclassProgressionFeatureEffects(progress, classLevel, action);
            }

            ApplyChoiceEffects(character.ChoiceSelections, action);
        }

        private static void ApplySubclassProgressionFeatureEffects(
            CharacterClassProgressSaveData progress,
            int classLevel,
            Action<DndFeatureEffectData> action)
        {
            if (progress == null || string.IsNullOrWhiteSpace(progress.SubclassId) || action == null)
            {
                return;
            }

            IReadOnlyList<DndSubclassLevelProgressionData> progressions = DndRuleContentService.Instance.GetSubclassProgressions(progress.SubclassId.Trim());
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

                ApplyFeatureEffects(progression.FeatureIds, action);
            }
        }

        private string FindSpellcastingAbilityId(CharacterCardDraftSaveData character)
        {
            List<CharacterClassProgressSaveData> progresses = GetCharacterClassProgresses(character);
            for (int index = 0; index < progresses.Count; index++)
            {
                DndClassDefineData classData = FindClass(progresses[index].ClassId);
                if (classData != null && !string.IsNullOrWhiteSpace(classData.SpellcastingAbility))
                {
                    return classData.SpellcastingAbility.Trim();
                }

                string subclassSpellcastingAbility = FindSubclassSpellcastingAbilityId(progresses[index]);
                if (!string.IsNullOrWhiteSpace(subclassSpellcastingAbility))
                {
                    return subclassSpellcastingAbility;
                }
            }

            return string.Empty;
        }

        private string FindSubclassSpellcastingAbilityId(CharacterClassProgressSaveData progress)
        {
            if (progress != null && !string.IsNullOrWhiteSpace(progress.SubclassId))
            {
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

                    string abilityId = FindSpellcastingAbilityId(null, progression.FeatureIds);
                    if (!string.IsNullOrWhiteSpace(abilityId))
                    {
                        return abilityId;
                    }
                }
            }

            DndChoiceOptionData option = FindSelectedSubclassOption(progress);
            return option == null ? string.Empty : FindSpellcastingAbilityId(option.GrantEffectIds, option.GrantFeatureIds);
        }

        private string FindSpellcastingAbilityId(IReadOnlyList<string> effectIds, IReadOnlyList<string> featureIds)
        {
            string abilityId = FindSpellcastingAbilityId(effectIds);
            if (!string.IsNullOrWhiteSpace(abilityId))
            {
                return abilityId;
            }

            if (featureIds == null)
            {
                return string.Empty;
            }

            for (int index = 0; index < featureIds.Count; index++)
            {
                if (DndRuleContentService.Instance.TryGetFeature(featureIds[index], out DndFeatureDefineData feature))
                {
                    abilityId = FindSpellcastingAbilityId(feature.EffectIds);
                    if (!string.IsNullOrWhiteSpace(abilityId))
                    {
                        return abilityId;
                    }
                }
            }

            return string.Empty;
        }

        private static string FindSpellcastingAbilityId(IReadOnlyList<string> effectIds)
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
                    if (!string.IsNullOrWhiteSpace(effect.Value))
                    {
                        return effect.Value.Trim();
                    }

                    return effect.Target?.Trim() ?? string.Empty;
                }
            }

            return string.Empty;
        }

        private static void ApplyFeatureEffects(IReadOnlyList<string> featureIds, Action<DndFeatureEffectData> action)
        {
            if (featureIds == null || action == null)
            {
                return;
            }

            for (int index = 0; index < featureIds.Count; index++)
            {
                if (DndRuleContentService.Instance.TryGetFeature(featureIds[index], out DndFeatureDefineData feature))
                {
                    ApplyEffects(feature.EffectIds, action);
                }
            }
        }

        private static void ApplyEffects(IReadOnlyList<string> effectIds, Action<DndFeatureEffectData> action)
        {
            if (effectIds == null || action == null)
            {
                return;
            }

            for (int index = 0; index < effectIds.Count; index++)
            {
                if (DndRuleContentService.Instance.TryGetFeatureEffect(effectIds[index], out DndFeatureEffectData effect))
                {
                    action(effect);
                }
            }
        }

        private static void ApplyChoiceEffects(IReadOnlyList<CharacterChoiceSelectionSaveData> choiceSelections, Action<DndFeatureEffectData> action)
        {
            if (choiceSelections == null || action == null)
            {
                return;
            }

            for (int index = 0; index < choiceSelections.Count; index++)
            {
                CharacterChoiceSelectionSaveData selection = choiceSelections[index];
                if (selection == null || string.IsNullOrWhiteSpace(selection.ChoiceGroupId) || string.IsNullOrWhiteSpace(selection.OptionId))
                {
                    continue;
                }


                if (DndRuleContentService.Instance.TryGetChoiceGroup(selection.ChoiceGroupId.Trim(), out DndChoiceGroupData choiceGroup)
                    && choiceGroup != null
                    && IsFeatChoiceGroup(choiceGroup)
                    && TryResolveFeatFromSelection(selection, out DndFeatDefineData feat)
                    && feat != null)
                {
                    ApplyFeatureEffects(feat.FeatureIds, action);
                    continue;
                }

                IReadOnlyList<DndChoiceOptionData> options = DndRuleContentService.Instance.GetChoiceOptions(selection.ChoiceGroupId.Trim());
                for (int optionIndex = 0; optionIndex < options.Count; optionIndex++)
                {
                    DndChoiceOptionData option = options[optionIndex];
                    if (option == null || !string.Equals(option.OptionId, selection.OptionId, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    ApplyEffects(option.GrantEffectIds, action);
                    ApplyFeatureEffects(option.GrantFeatureIds, action);
                    break;
                }
            }
        }

        private static bool IsAbilityScoreChoiceGroup(DndChoiceGroupData choiceGroup)
        {
            return choiceGroup != null
                && (string.Equals(choiceGroup.ChoiceType, "AbilityScore", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(choiceGroup.ChoiceGroupId, "choice_asi_attributes", StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsFeatChoiceGroup(DndChoiceGroupData choiceGroup)
        {
            return choiceGroup != null
                && (string.Equals(choiceGroup.ChoiceType, "Feat", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(choiceGroup.ChoiceGroupId, "choice_feat_any", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(choiceGroup.ChoiceGroupId, "choice_feat", StringComparison.OrdinalIgnoreCase));
        }

        private static bool TryResolveFeatFromSelection(CharacterChoiceSelectionSaveData selection, out DndFeatDefineData feat)
        {
            feat = null;
            if (selection == null || string.IsNullOrWhiteSpace(selection.OptionId))
            {
                return false;
            }

            string optionId = selection.OptionId.Trim();
            if (DndRuleContentService.Instance.TryGetFeat(optionId, out feat))
            {
                return true;
            }

            DndChoiceOptionData option = FindChoiceOption(selection.ChoiceGroupId, optionId);
            if (option?.GrantFeatureIds == null || option.GrantFeatureIds.Count == 0)
            {
                return false;
            }

            IReadOnlyList<DndFeatDefineData> feats = DndRuleContentService.Instance.Feats;
            for (int featIndex = 0; featIndex < feats.Count; featIndex++)
            {
                DndFeatDefineData candidate = feats[featIndex];
                if (candidate?.FeatureIds == null)
                {
                    continue;
                }

                for (int featureIndex = 0; featureIndex < option.GrantFeatureIds.Count; featureIndex++)
                {
                    if (ContainsExactString(candidate.FeatureIds, option.GrantFeatureIds[featureIndex]))
                    {
                        feat = candidate;
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsEffectMatch(string actualEffectType, string actualTarget, string expectedEffectType, IReadOnlyList<string> expectedTargets)
        {
            if (!string.Equals(actualEffectType, expectedEffectType, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (expectedTargets == null || expectedTargets.Count == 0)
            {
                return true;
            }

            for (int index = 0; index < expectedTargets.Count; index++)
            {
                string expectedTarget = expectedTargets[index];
                if (string.IsNullOrWhiteSpace(expectedTarget)
                    || string.Equals(actualTarget, expectedTarget, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsStructuredEffectConditionMet(DndFeatureEffectData effect, CharacterRuntimeSnapshotData snapshot)
        {
            if (effect == null || string.IsNullOrWhiteSpace(effect.Condition))
            {
                return true;
            }

            if (string.Equals(effect.Condition, "WearingArmor", StringComparison.OrdinalIgnoreCase))
            {
                return snapshot != null
                    && !string.Equals(CharacterArmorCategoryIds.Normalize(snapshot.ArmorCategory), CharacterArmorCategoryIds.None, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static bool IsCharacterItemEffectConditionMet(string condition, CharacterRuntimeSnapshotData snapshot)
        {
            if (string.IsNullOrWhiteSpace(condition))
            {
                return true;
            }

            if (string.Equals(condition, "WearingArmor", StringComparison.OrdinalIgnoreCase))
            {
                return snapshot != null
                    && !string.Equals(CharacterArmorCategoryIds.Normalize(snapshot.ArmorCategory), CharacterArmorCategoryIds.None, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static int CalculateArmorDexterityAcBonus(string armorCategory, int dexterityModifier)
        {
            string normalized = CharacterArmorCategoryIds.Normalize(armorCategory);
            if (string.Equals(normalized, CharacterArmorCategoryIds.Heavy, StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            if (string.Equals(normalized, CharacterArmorCategoryIds.Medium, StringComparison.OrdinalIgnoreCase))
            {
                return Math.Min(dexterityModifier, 2);
            }

            return dexterityModifier;
        }

        private static int GetAbilityModifier(CharacterRuntimeSnapshotData snapshot, AbilityKind ability)
        {
            return ability switch
            {
                AbilityKind.Strength => CalculateAbilityModifier(NormalizeAbility(snapshot.Strength)),
                AbilityKind.Dexterity => CalculateAbilityModifier(NormalizeAbility(snapshot.Dexterity)),
                AbilityKind.Intelligence => CalculateAbilityModifier(NormalizeAbility(snapshot.Intelligence)),
                AbilityKind.Wisdom => CalculateAbilityModifier(NormalizeAbility(snapshot.Wisdom)),
                AbilityKind.Charisma => CalculateAbilityModifier(NormalizeAbility(snapshot.Charisma)),
                _ => 0
            };
        }

        private static int GetAbilityScore(CharacterRuntimeSnapshotData snapshot, string abilityId)
        {
            if (snapshot == null || string.IsNullOrWhiteSpace(abilityId))
            {
                return 0;
            }

            string normalized = NormalizeAbilityId(abilityId);
            if (string.Equals(normalized, "Strength", StringComparison.OrdinalIgnoreCase))
            {
                return snapshot.Strength;
            }

            if (string.Equals(normalized, "Dexterity", StringComparison.OrdinalIgnoreCase))
            {
                return snapshot.Dexterity;
            }

            if (string.Equals(normalized, "Constitution", StringComparison.OrdinalIgnoreCase))
            {
                return snapshot.Constitution;
            }

            if (string.Equals(normalized, "Intelligence", StringComparison.OrdinalIgnoreCase))
            {
                return snapshot.Intelligence;
            }

            if (string.Equals(normalized, "Wisdom", StringComparison.OrdinalIgnoreCase))
            {
                return snapshot.Wisdom;
            }

            if (string.Equals(normalized, "Charisma", StringComparison.OrdinalIgnoreCase))
            {
                return snapshot.Charisma;
            }

            return 0;
        }

        private static string NormalizeAbilityId(string value)
        {
            string normalized = value?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return string.Empty;
            }

            switch (normalized.ToLowerInvariant())
            {
                case "str":
                case "strength":
                    return "Strength";
                case "dex":
                case "dexterity":
                    return "Dexterity";
                case "con":
                case "constitution":
                    return "Constitution";
                case "int":
                case "intelligence":
                    return "Intelligence";
                case "wis":
                case "wisdom":
                    return "Wisdom";
                case "cha":
                case "charisma":
                    return "Charisma";
                case "all":
                    return "All";
                default:
                    return normalized;
            }
        }

        private static DndClassDefineData FindClass(string classId)
        {
            return DndRuleContentService.Instance.TryGetClass(classId, out DndClassDefineData classData)
                ? classData
                : null;
        }

        private static DndRaceDefineData FindRace(string raceId)
        {
            return FindById(DndRuleContentService.Instance.Races, raceId, data => data.RaceId);
        }

        private static DndBackgroundDefineData FindBackground(string backgroundId)
        {
            return FindById(DndRuleContentService.Instance.Backgrounds, backgroundId, data => data.BackgroundId);
        }

        private static DndFeatDefineData FindFeat(string featId)
        {
            return FindById(DndRuleContentService.Instance.Feats, featId, data => data.FeatId);
        }

        private static DndChoiceOptionData FindSelectedSubclassOption(CharacterClassProgressSaveData progress)
        {
            if (progress == null || string.IsNullOrWhiteSpace(progress.ClassId) || string.IsNullOrWhiteSpace(progress.SubclassId))
            {
                return null;
            }

            return FindChoiceOption($"choice_subclass_{progress.ClassId.Trim()}", progress.SubclassId.Trim());
        }

        private static DndChoiceOptionData FindChoiceOption(string choiceGroupId, string optionId)
        {
            if (string.IsNullOrWhiteSpace(choiceGroupId) || string.IsNullOrWhiteSpace(optionId))
            {
                return null;
            }

            IReadOnlyList<DndChoiceOptionData> options = DndRuleContentService.Instance.GetChoiceOptions(choiceGroupId.Trim());
            string normalizedOptionId = optionId.Trim();
            for (int index = 0; index < options.Count; index++)
            {
                DndChoiceOptionData option = options[index];
                if (option != null
                    && !string.IsNullOrWhiteSpace(option.OptionId)
                    && string.Equals(option.OptionId.Trim(), normalizedOptionId, StringComparison.OrdinalIgnoreCase))
                {
                    return option;
                }
            }

            return null;
        }

        private static List<CharacterClassProgressSaveData> GetCharacterClassProgresses(CharacterCardDraftSaveData character)
        {
            List<CharacterClassProgressSaveData> progresses = new List<CharacterClassProgressSaveData>();
            if (character == null)
            {
                return progresses;
            }

            if (character.ClassProgresses != null)
            {
                for (int index = 0; index < character.ClassProgresses.Count; index++)
                {
                    CharacterClassProgressSaveData progress = character.ClassProgresses[index];
                    if (progress == null || string.IsNullOrWhiteSpace(progress.ClassId))
                    {
                        continue;
                    }

                    progresses.Add(new CharacterClassProgressSaveData
                    {
                        ClassId = progress.ClassId.Trim(),
                        SubclassId = progress.SubclassId ?? string.Empty,
                        Level = Math.Max(1, progress.Level)
                    });
                }
            }

            if (progresses.Count == 0 && !string.IsNullOrWhiteSpace(character.ClassId))
            {
                progresses.Add(new CharacterClassProgressSaveData
                {
                    ClassId = character.ClassId.Trim(),
                    Level = Math.Max(1, character.Level)
                });
            }

            return progresses;
        }

        private static bool ContainsId(IReadOnlyList<string> values, string id, string displayName = null)
        {
            if (values == null)
            {
                return false;
            }

            for (int index = 0; index < values.Count; index++)
            {
                string normalized = NormalizeSkillId(values[index]);
                if (string.Equals(normalized, id, StringComparison.OrdinalIgnoreCase)
                    || !string.IsNullOrWhiteSpace(displayName) && string.Equals(values[index]?.Trim(), displayName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string NormalizeSkillId(string value)
        {
            string normalized = value?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return string.Empty;
            }

            normalized = normalized
                .Replace("Skill:", string.Empty)
                .Replace("\u6280\u80FD\uFF1A", string.Empty)
                .Replace("\u6280\u80FD:", string.Empty)
                .Trim();

            if (DndRuleContentService.Instance.TryGetSkill(normalized, out DndSkillDefineData directSkill))
            {
                return directSkill.SkillId;
            }

            IReadOnlyList<DndSkillDefineData> skills = DndRuleContentService.Instance.Skills;
            for (int index = 0; index < skills.Count; index++)
            {
                DndSkillDefineData skill = skills[index];
                if (skill == null)
                {
                    continue;
                }

                if (string.Equals(skill.SkillId, normalized, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(skill.Name, normalized, StringComparison.OrdinalIgnoreCase))
                {
                    return skill.SkillId;
                }
            }

            return normalized;
        }

        private static int NormalizeAbility(int value)
        {
            return value > 0 ? value : DefaultAbilityScore;
        }

        private static int CalculateAbilityModifier(int abilityScore)
        {
            return CharacterCreationCalculationService.Instance.CalculateAbilityModifier(abilityScore);
        }

        private static int CalculateProficiencyBonus(int level)
        {
            return CharacterCreationCalculationService.Instance.CalculateProficiencyBonus(level);
        }

        private static T FindById<T>(IReadOnlyList<T> list, string id, Func<T, string> idGetter)
            where T : class
        {
            if (list == null || string.IsNullOrWhiteSpace(id) || idGetter == null)
            {
                return null;
            }

            for (int index = 0; index < list.Count; index++)
            {
                T item = list[index];
                if (item != null && string.Equals(idGetter(item), id, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }

            return null;
        }
    }
}
