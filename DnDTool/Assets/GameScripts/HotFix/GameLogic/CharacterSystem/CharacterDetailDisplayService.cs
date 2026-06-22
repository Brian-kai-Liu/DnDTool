using System;
using System.Collections.Generic;
using System.Text;

namespace GameLogic
{
    internal sealed class CharacterDetailDisplayService
    {
        private static readonly string[] SubclassChoiceFeatureIdMarkers =
        {
            "primal_path",
            "bard_college",
            "divine_domain",
            "druid_circle",
            "martial_archetype",
            "monastic_tradition",
            "sacred_oath",
            "ranger_archetype",
            "roguish_archetype",
            "sorcerous_origin",
            "otherworldly_patron",
            "arcane_tradition"
        };

        private static readonly Lazy<CharacterDetailDisplayService> s_instance =
            new Lazy<CharacterDetailDisplayService>(() => new CharacterDetailDisplayService());

        private CharacterDetailDisplayService()
        {
        }

        public static CharacterDetailDisplayService Instance => s_instance.Value;

        public List<CharacterStatusEffectDisplayEntry> BuildStatusEffectEntries(CharacterCardDraftSaveData character)
        {
            List<CharacterStatusEffectDisplayEntry> entries = new List<CharacterStatusEffectDisplayEntry>();
            if (character == null)
            {
                return entries;
            }

            AppendConditionStatusEffects(entries, character.Conditions);
            AppendTemporaryStatusEffects(entries, character.TemporaryEffects);
            return entries;
        }

        public List<string> BuildEquipmentAndToolEntries(CharacterRuntimeSnapshotData snapshot)
        {
            List<string> entries = new List<string>();
            if (snapshot == null)
            {
                return entries;
            }

            AppendUniqueValues(entries, snapshot.ArmorProficiencyIds);
            AppendUniqueValues(entries, snapshot.WeaponProficiencyIds);
            AppendUniqueValues(entries, snapshot.ToolProficiencyIds);
            AppendEquipmentSummaryValues(entries, snapshot.ArmorProficiencies);
            AppendEquipmentSummaryValues(entries, snapshot.WeaponProficiencies);
            AppendEquipmentSummaryValues(entries, snapshot.ToolProficiencies);
            return entries;
        }

        public List<CharacterInventoryDisplayEntry> BuildInventoryEntries(CharacterEquipmentSetSaveData equipment)
        {
            List<CharacterInventoryDisplayEntry> entries = new List<CharacterInventoryDisplayEntry>();
            if (equipment == null)
            {
                return entries;
            }

            AppendInventoryItemEntry(entries, equipment.Armor, true);
            AppendInventoryItemEntry(entries, equipment.Shield, true);
            AppendInventoryItemEntries(entries, equipment.EquippedItems, true);
            AppendInventoryItemEntries(entries, equipment.InventoryItems, false);
            return entries;
        }

        public List<CharacterHitDicePoolSaveData> BuildDisplayHitDicePools(CharacterCardDraftSaveData character)
        {
            List<CharacterHitDicePoolSaveData> explicitPools = CharacterHitDicePoolSaveData.CloneList(character?.HitDicePools);
            if (explicitPools.Count > 0)
            {
                return explicitPools;
            }

            List<CharacterHitDicePoolSaveData> result = new List<CharacterHitDicePoolSaveData>();
            List<CharacterClassProgressSaveData> progresses = GetCharacterClassProgresses(character);
            for (int index = 0; index < progresses.Count; index++)
            {
                CharacterClassProgressSaveData progress = progresses[index];
                if (progress == null || string.IsNullOrWhiteSpace(progress.ClassId))
                {
                    continue;
                }

                DndClassDefineData classData = FindClass(progress.ClassId);
                int level = Math.Max(1, progress.Level);
                int hitDie = classData != null ? Math.Max(0, classData.HitDie) : 0;
                if (hitDie <= 0)
                {
                    continue;
                }

                result.Add(new CharacterHitDicePoolSaveData
                {
                    ClassId = progress.ClassId,
                    DieSize = hitDie,
                    Total = level,
                    Remaining = level
                });
            }

            return result;
        }

        public CharacterRaceFeatureHeaderState BuildRaceFeatureHeader(CharacterRuntimeSnapshotData snapshot, string raceId)
        {
            CharacterRaceFeatureHeaderState state = new CharacterRaceFeatureHeaderState
            {
                MainRaceName = snapshot?.MainRaceName ?? string.Empty
            };

            if (!string.IsNullOrWhiteSpace(raceId))
            {
                if (DndRuleContentService.Instance.TryGetRaceSub(raceId, out DndRaceSubDefineData subRace))
                {
                    state.SubRaceName = subRace.Name ?? string.Empty;
                    if (DndRuleContentService.Instance.TryGetRaceMain(subRace.MainRaceId, out DndRaceMainDefineData mainRace))
                    {
                        state.MainRaceName = mainRace.Name ?? state.MainRaceName;
                    }
                }
                else if (DndRuleContentService.Instance.TryGetRaceMain(raceId, out DndRaceMainDefineData mainRace))
                {
                    state.MainRaceName = mainRace.Name ?? state.MainRaceName;
                }
            }

            if (string.IsNullOrWhiteSpace(state.MainRaceName))
            {
                state.MainRaceName = snapshot?.RaceName ?? string.Empty;
            }

            return state;
        }

        public List<CharacterClassDetailDisplayState> BuildClassDetailSections(CharacterCardDraftSaveData character)
        {
            List<CharacterClassDetailDisplayState> sections = new List<CharacterClassDetailDisplayState>();
            List<CharacterClassProgressSaveData> progresses = GetCharacterClassProgresses(character);
            for (int index = 0; index < progresses.Count; index++)
            {
                CharacterClassProgressSaveData progress = progresses[index];
                CharacterClassDetailDisplayState section = new CharacterClassDetailDisplayState
                {
                    ClassName = GetClassDisplayName(progress),
                    SubclassName = ResolveSubclassDisplayName(progress),
                    LevelText = $"Lv.{Math.Max(1, progress.Level)}"
                };

                section.Features.AddRange(BuildClassFeatureEntries(character, progress));
                sections.Add(section);
            }

            return sections;
        }

        public string BuildClassNameSummary(CharacterCardDraftSaveData character, CharacterRuntimeSnapshotData snapshot)
        {
            List<CharacterClassProgressSaveData> progresses = GetCharacterClassProgresses(character);
            if (progresses.Count == 0)
            {
                return $"{FormatTextOrDefault(snapshot?.ClassName, "未选择职业")}  Lv.{Math.Max(1, snapshot?.Level ?? 1)}";
            }

            List<string> labels = new List<string>();
            for (int index = 0; index < progresses.Count; index++)
            {
                CharacterClassProgressSaveData progress = progresses[index];
                string className = GetClassDisplayName(progress);
                string subclassName = ResolveSubclassDisplayName(progress);
                string levelLabel = $"Lv.{Math.Max(1, progress.Level)}";
                if (!string.IsNullOrWhiteSpace(subclassName))
                {
                    labels.Add($"{className}({subclassName})  {levelLabel}");
                }
                else
                {
                    labels.Add($"{className}  {levelLabel}");
                }
            }

            return string.Join(" / ", labels);
        }

        public List<ClassFeatureDisplayEntry> BuildRaceFeatureEntries(CharacterCardDraftSaveData character, string raceId)
        {
            List<ClassFeatureDisplayEntry> entries = new List<ClassFeatureDisplayEntry>();
            DndRaceDefineData raceData = FindRace(raceId);
            AppendFeatureDisplayEntries(entries, raceData?.FeatureIds);
            AppendGeneralFeatureChoiceGrantedFeatureEntries(entries, raceData?.FeatureIds, "Race", raceId, character?.ChoiceSelections);
            return entries;
        }

        public List<ClassFeatureDisplayEntry> BuildOtherFeatureEntries(CharacterCardDraftSaveData character)
        {
            List<ClassFeatureDisplayEntry> entries = new List<ClassFeatureDisplayEntry>();
            if (character == null)
            {
                return entries;
            }

            DndFeatDefineData featData = FindFeat(character.FeatId);
            AppendFeatureDisplayEntries(entries, featData?.FeatureIds);
            return entries;
        }

        public string BuildFeatureDescription(DndFeatureDefineData feature)
        {
            if (feature == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(feature.Description))
            {
                return feature.Description.Trim();
            }

            if (feature.EffectIds == null)
            {
                return string.Empty;
            }

            for (int index = 0; index < feature.EffectIds.Count; index++)
            {
                string effectId = feature.EffectIds[index];
                if (string.IsNullOrWhiteSpace(effectId))
                {
                    continue;
                }

                if (DndRuleContentService.Instance.TryGetFeatureEffect(effectId, out DndFeatureEffectData effect)
                    && !string.IsNullOrWhiteSpace(effect.ManualNote))
                {
                    return effect.ManualNote.Trim();
                }
            }

            return string.Empty;
        }

        private List<ClassFeatureDisplayEntry> BuildClassFeatureEntries(CharacterCardDraftSaveData character, CharacterClassProgressSaveData progress)
        {
            List<ClassFeatureDisplayEntry> entries = new List<ClassFeatureDisplayEntry>();
            if (progress == null || string.IsNullOrWhiteSpace(progress.ClassId))
            {
                return entries;
            }

            int classLevel = Math.Max(1, progress.Level);
            for (int level = 1; level <= classLevel; level++)
            {
                if (!DndRuleContentService.Instance.TryGetClassLevelProgression(progress.ClassId, level, out DndLevelProgressionData progression))
                {
                    continue;
                }

                AppendClassFeatureEntries(entries, progression, progress, level, character?.ChoiceSelections);
            }

            AppendSubclassFeatureEntries(entries, progress, classLevel);
            return entries;
        }

        private void AppendClassFeatureEntries(
            List<ClassFeatureDisplayEntry> entries,
            DndLevelProgressionData progression,
            CharacterClassProgressSaveData progress,
            int level,
            IReadOnlyList<CharacterChoiceSelectionSaveData> choiceSelections)
        {
            IReadOnlyList<string> featureIds = progression?.FeatureIds;
            if (entries == null || featureIds == null)
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

                string title = featureId.Trim();
                string description = string.Empty;
                if (DndRuleContentService.Instance.TryGetFeature(featureId, out DndFeatureDefineData feature))
                {
                    if (ShouldHideSubclassChoiceFeature(feature, progression, progress))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(feature.Name))
                    {
                        title = feature.Name.Trim();
                    }

                    description = BuildFeatureDescription(feature);
                    if (TryBuildSelectedFeatureChoiceDisplay(feature, progress, level, choiceSelections, out string choiceSuffix, out string choiceDescription))
                    {
                        title = $"{title}-{choiceSuffix}";
                        if (!string.IsNullOrWhiteSpace(choiceDescription))
                        {
                            description = choiceDescription;
                        }
                    }
                }

                entries.Add(new ClassFeatureDisplayEntry(title, description));
            }

            AppendSelectedChoiceGrantedFeatureEntries(entries, progression, progress, level, choiceSelections);
        }

        private void AppendSubclassFeatureEntries(
            List<ClassFeatureDisplayEntry> entries,
            CharacterClassProgressSaveData progress,
            int classLevel)
        {
            if (entries == null || progress == null || string.IsNullOrWhiteSpace(progress.SubclassId))
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

                AppendFeatureDisplayEntries(entries, progression.FeatureIds);
            }
        }

        private void AppendSelectedChoiceGrantedFeatureEntries(
            List<ClassFeatureDisplayEntry> entries,
            DndLevelProgressionData progression,
            CharacterClassProgressSaveData progress,
            int level,
            IReadOnlyList<CharacterChoiceSelectionSaveData> choiceSelections)
        {
            if (entries == null || progression == null || choiceSelections == null || progression.ChoiceGroupIds == null)
            {
                return;
            }

            for (int index = 0; index < choiceSelections.Count; index++)
            {
                CharacterChoiceSelectionSaveData selection = choiceSelections[index];
                if (!MatchesProgressionChoiceSelection(progression, progress, level, selection))
                {
                    continue;
                }

                DndChoiceOptionData option = FindChoiceOption(selection.ChoiceGroupId, selection.OptionId);
                if (option?.GrantFeatureIds == null)
                {
                    continue;
                }

                AppendFeatureDisplayEntries(entries, option.GrantFeatureIds);
            }

            if (HasSubclassChoiceGroup(progression))
            {
                DndChoiceOptionData subclassOption = FindSelectedSubclassOption(progress);
                if (subclassOption?.GrantFeatureIds != null)
                {
                    AppendFeatureDisplayEntries(entries, subclassOption.GrantFeatureIds);
                }
            }
        }

        private void AppendGeneralFeatureChoiceGrantedFeatureEntries(
            List<ClassFeatureDisplayEntry> entries,
            IReadOnlyList<string> featureIds,
            string sourceType,
            string sourceId,
            IReadOnlyList<CharacterChoiceSelectionSaveData> choiceSelections)
        {
            if (entries == null || featureIds == null || choiceSelections == null)
            {
                return;
            }

            for (int featureIndex = 0; featureIndex < featureIds.Count; featureIndex++)
            {
                string featureId = featureIds[featureIndex];
                if (string.IsNullOrWhiteSpace(featureId)
                    || !DndRuleContentService.Instance.TryGetFeature(featureId, out DndFeatureDefineData feature)
                    || feature.ChoiceGroupIds == null
                    || feature.ChoiceGroupIds.Count == 0)
                {
                    continue;
                }

                for (int selectionIndex = 0; selectionIndex < choiceSelections.Count; selectionIndex++)
                {
                    CharacterChoiceSelectionSaveData selection = choiceSelections[selectionIndex];
                    if (!MatchesGeneralFeatureChoiceSelection(feature, sourceType, sourceId, selection))
                    {
                        continue;
                    }

                    DndChoiceOptionData option = FindChoiceOption(selection.ChoiceGroupId, selection.OptionId);
                    if (option?.GrantFeatureIds == null)
                    {
                        continue;
                    }

                    AppendFeatureDisplayEntries(entries, option.GrantFeatureIds);
                }
            }
        }

        private static bool ShouldHideSubclassChoiceFeature(
            DndFeatureDefineData feature,
            DndLevelProgressionData progression,
            CharacterClassProgressSaveData progress)
        {
            if (feature == null || string.IsNullOrWhiteSpace(progress?.SubclassId))
            {
                return false;
            }

            if (!HasSubclassChoiceGroup(progression) && !HasSubclassChoiceGroup(feature))
            {
                return false;
            }

            string normalizedFeatureId = feature.FeatureId?.Trim().ToLowerInvariant() ?? string.Empty;
            for (int index = 0; index < SubclassChoiceFeatureIdMarkers.Length; index++)
            {
                if (normalizedFeatureId.Contains(SubclassChoiceFeatureIdMarkers[index]))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasSubclassChoiceGroup(DndLevelProgressionData progression)
        {
            if (progression?.ChoiceGroupIds == null)
            {
                return false;
            }

            for (int index = 0; index < progression.ChoiceGroupIds.Count; index++)
            {
                string choiceGroupId = progression.ChoiceGroupIds[index];
                if (!string.IsNullOrWhiteSpace(choiceGroupId)
                    && choiceGroupId.Trim().StartsWith("choice_subclass_", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasSubclassChoiceGroup(DndFeatureDefineData feature)
        {
            if (feature?.ChoiceGroupIds == null)
            {
                return false;
            }

            for (int index = 0; index < feature.ChoiceGroupIds.Count; index++)
            {
                string choiceGroupId = feature.ChoiceGroupIds[index];
                if (IsSubclassChoiceGroup(choiceGroupId))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsSubclassChoiceGroup(string choiceGroupId)
        {
            if (string.IsNullOrWhiteSpace(choiceGroupId))
            {
                return false;
            }

            if (choiceGroupId.Trim().StartsWith("choice_subclass_", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return DndRuleContentService.Instance.TryGetChoiceGroup(choiceGroupId.Trim(), out DndChoiceGroupData choiceGroup)
                && string.Equals(choiceGroup.ChoiceType, "Subclass", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryBuildSelectedFeatureChoiceDisplay(
            DndFeatureDefineData feature,
            CharacterClassProgressSaveData progress,
            int level,
            IReadOnlyList<CharacterChoiceSelectionSaveData> choiceSelections,
            out string choiceSuffix,
            out string choiceDescription)
        {
            choiceSuffix = string.Empty;
            choiceDescription = string.Empty;
            if (feature == null || feature.ChoiceGroupIds == null || feature.ChoiceGroupIds.Count == 0 || choiceSelections == null)
            {
                return false;
            }

            List<string> suffixes = new List<string>();
            List<string> descriptions = new List<string>();
            for (int index = 0; index < choiceSelections.Count; index++)
            {
                CharacterChoiceSelectionSaveData selection = choiceSelections[index];
                if (!MatchesFeatureChoiceSelection(feature, progress, level, selection))
                {
                    continue;
                }

                DndChoiceOptionData option = FindChoiceOption(selection.ChoiceGroupId, selection.OptionId);
                if (option == null)
                {
                    continue;
                }

                if (TryBuildAdvancementSelectionDisplay(selection, option, choiceSelections, out string advancementSuffix, out string advancementDescription))
                {
                    if (!string.IsNullOrWhiteSpace(advancementSuffix))
                    {
                        suffixes.Add(advancementSuffix);
                    }

                    if (!string.IsNullOrWhiteSpace(advancementDescription))
                    {
                        descriptions.Add(advancementDescription);
                    }

                    continue;
                }

                string suffix = BuildChoiceOptionDisplayName(option);
                if (!string.IsNullOrWhiteSpace(suffix))
                {
                    suffixes.Add(suffix);
                }

                string description = BuildChoiceOptionDescription(option);
                if (!string.IsNullOrWhiteSpace(description))
                {
                    descriptions.Add(description);
                }
            }

            if (suffixes.Count == 0)
            {
                return false;
            }

            choiceSuffix = string.Join("/", suffixes);
            choiceDescription = descriptions.Count > 0 ? string.Join("\n", descriptions) : string.Empty;
            return true;
        }

        private static bool MatchesFeatureChoiceSelection(
            DndFeatureDefineData feature,
            CharacterClassProgressSaveData progress,
            int level,
            CharacterChoiceSelectionSaveData selection)
        {
            if (feature == null || selection == null || string.IsNullOrWhiteSpace(selection.ChoiceGroupId))
            {
                return false;
            }

            string selectionChoiceGroupId = selection.ChoiceGroupId.Trim();
            bool groupMatched = false;
            for (int index = 0; index < feature.ChoiceGroupIds.Count; index++)
            {
                string featureChoiceGroupId = feature.ChoiceGroupIds[index];
                if (!string.IsNullOrWhiteSpace(featureChoiceGroupId)
                    && string.Equals(featureChoiceGroupId.Trim(), selectionChoiceGroupId, StringComparison.OrdinalIgnoreCase))
                {
                    groupMatched = true;
                    break;
                }
            }

            if (!groupMatched)
            {
                return false;
            }

            string sourceId = selection.SourceId?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(selection.SourceId)
                && !string.Equals(sourceId, feature.FeatureId?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string selectionClassId = selection.ClassId?.Trim() ?? string.Empty;
            string progressClassId = progress?.ClassId?.Trim() ?? string.Empty;
            if (progress != null
                && !string.IsNullOrWhiteSpace(selectionClassId)
                && !string.Equals(selectionClassId, progressClassId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return selection.Level <= 0 || selection.Level == level;
        }

        private static bool TryBuildAdvancementSelectionDisplay(
            CharacterChoiceSelectionSaveData parentSelection,
            DndChoiceOptionData parentOption,
            IReadOnlyList<CharacterChoiceSelectionSaveData> choiceSelections,
            out string suffix,
            out string description)
        {
            suffix = string.Empty;
            description = string.Empty;
            if (parentSelection == null
                || parentOption == null
                || choiceSelections == null
                || !DndRuleContentService.Instance.TryGetChoiceGroup(parentSelection.ChoiceGroupId, out DndChoiceGroupData parentGroup)
                || parentGroup == null
                || !IsAdvancementOptionChoiceType(parentGroup.ChoiceType))
            {
                return false;
            }

            string followupChoiceGroupId = ResolveAdvancementFollowupChoiceGroupId(parentGroup, parentSelection.OptionId);
            if (string.IsNullOrWhiteSpace(followupChoiceGroupId))
            {
                return false;
            }

            List<string> suffixes = new List<string>();
            List<string> descriptions = new List<string>();
            for (int index = 0; index < choiceSelections.Count; index++)
            {
                CharacterChoiceSelectionSaveData selection = choiceSelections[index];
                if (!MatchesFollowupChoiceSelection(parentSelection, followupChoiceGroupId, selection))
                {
                    continue;
                }

                DndChoiceOptionData option = FindChoiceOption(selection.ChoiceGroupId, selection.OptionId);
                string optionName = BuildChoiceOptionDisplayName(option);
                if (string.IsNullOrWhiteSpace(optionName))
                {
                    optionName = selection.OptionId?.Trim() ?? string.Empty;
                }

                if (!string.IsNullOrWhiteSpace(optionName))
                {
                    suffixes.Add(optionName);
                }

                string optionDescription = BuildChoiceOptionDescription(option);
                if (!string.IsNullOrWhiteSpace(optionDescription))
                {
                    descriptions.Add(optionDescription);
                }
            }

            string parentName = BuildChoiceOptionDisplayName(parentOption);
            if (suffixes.Count > 0)
            {
                suffix = string.IsNullOrWhiteSpace(parentName)
                    ? string.Join("/", suffixes)
                    : $"{parentName}-{string.Join("/", suffixes)}";
            }
            else
            {
                suffix = parentName;
            }

            description = descriptions.Count > 0
                ? string.Join("\n", descriptions)
                : BuildChoiceOptionDescription(parentOption);
            return !string.IsNullOrWhiteSpace(suffix) || !string.IsNullOrWhiteSpace(description);
        }

        private static bool MatchesFollowupChoiceSelection(
            CharacterChoiceSelectionSaveData parentSelection,
            string followupChoiceGroupId,
            CharacterChoiceSelectionSaveData selection)
        {
            if (parentSelection == null
                || selection == null
                || string.IsNullOrWhiteSpace(followupChoiceGroupId)
                || string.IsNullOrWhiteSpace(selection.ChoiceGroupId)
                || !string.Equals(selection.ChoiceGroupId.Trim(), followupChoiceGroupId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(parentSelection.SourceType)
                && !string.Equals(parentSelection.SourceType.Trim(), selection.SourceType?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(parentSelection.SourceId)
                && !string.Equals(parentSelection.SourceId.Trim(), selection.SourceId?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(parentSelection.ClassId)
                && !string.Equals(parentSelection.ClassId.Trim(), selection.ClassId?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return parentSelection.Level <= 0 || selection.Level <= 0 || parentSelection.Level == selection.Level;
        }

        private static bool IsAdvancementOptionChoiceType(string choiceType)
        {
            return string.Equals(choiceType, "AdvancementOption", StringComparison.OrdinalIgnoreCase)
                || string.Equals(choiceType, "FeatOrAbilityScore", StringComparison.OrdinalIgnoreCase);
        }

        private static string ResolveAdvancementFollowupChoiceGroupId(DndChoiceGroupData parentGroup, string optionId)
        {
            string normalized = optionId?.Trim() ?? string.Empty;
            if (parentGroup?.NextChoiceGroupIds != null && parentGroup.NextChoiceGroupIds.Count > 0)
            {
                if (string.Equals(normalized, "option_asi", StringComparison.OrdinalIgnoreCase))
                {
                    return parentGroup.NextChoiceGroupIds[0]?.Trim() ?? string.Empty;
                }

                if (string.Equals(normalized, "option_feat", StringComparison.OrdinalIgnoreCase)
                    && parentGroup.NextChoiceGroupIds.Count > 1)
                {
                    return parentGroup.NextChoiceGroupIds[1]?.Trim() ?? string.Empty;
                }
            }

            if (string.Equals(normalized, "option_asi", StringComparison.OrdinalIgnoreCase))
            {
                return "choice_asi_attributes";
            }

            if (string.Equals(normalized, "option_feat", StringComparison.OrdinalIgnoreCase))
            {
                return "choice_feat_any";
            }

            return string.Empty;
        }

        private static bool MatchesGeneralFeatureChoiceSelection(
            DndFeatureDefineData feature,
            string sourceType,
            string sourceId,
            CharacterChoiceSelectionSaveData selection)
        {
            if (feature == null || selection == null || string.IsNullOrWhiteSpace(selection.ChoiceGroupId))
            {
                return false;
            }

            bool groupMatched = false;
            for (int index = 0; index < feature.ChoiceGroupIds.Count; index++)
            {
                string featureChoiceGroupId = feature.ChoiceGroupIds[index];
                if (!string.IsNullOrWhiteSpace(featureChoiceGroupId)
                    && string.Equals(featureChoiceGroupId.Trim(), selection.ChoiceGroupId.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    groupMatched = true;
                    break;
                }
            }

            if (!groupMatched)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(selection.SourceType)
                && !string.Equals(selection.SourceType.Trim(), sourceType?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(selection.SourceId)
                && !string.Equals(selection.SourceId.Trim(), sourceId?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        private static bool MatchesProgressionChoiceSelection(
            DndLevelProgressionData progression,
            CharacterClassProgressSaveData progress,
            int level,
            CharacterChoiceSelectionSaveData selection)
        {
            if (progression == null || selection == null || string.IsNullOrWhiteSpace(selection.ChoiceGroupId))
            {
                return false;
            }

            string selectionChoiceGroupId = selection.ChoiceGroupId.Trim();
            bool groupMatched = false;
            for (int index = 0; index < progression.ChoiceGroupIds.Count; index++)
            {
                string progressionChoiceGroupId = progression.ChoiceGroupIds[index];
                if (!string.IsNullOrWhiteSpace(progressionChoiceGroupId)
                    && string.Equals(progressionChoiceGroupId.Trim(), selectionChoiceGroupId, StringComparison.OrdinalIgnoreCase))
                {
                    groupMatched = true;
                    break;
                }
            }

            if (!groupMatched)
            {
                return false;
            }

            string selectionClassId = selection.ClassId?.Trim() ?? string.Empty;
            string progressClassId = progress?.ClassId?.Trim() ?? string.Empty;
            if (progress != null
                && !string.IsNullOrWhiteSpace(selectionClassId)
                && !string.Equals(selectionClassId, progressClassId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return selection.Level <= 0 || selection.Level == level;
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

        private static DndChoiceOptionData FindSelectedSubclassOption(CharacterClassProgressSaveData progress)
        {
            if (progress == null || string.IsNullOrWhiteSpace(progress.ClassId) || string.IsNullOrWhiteSpace(progress.SubclassId))
            {
                return null;
            }

            return FindChoiceOption($"choice_subclass_{progress.ClassId.Trim()}", progress.SubclassId.Trim());
        }

        private static string BuildChoiceOptionDisplayName(DndChoiceOptionData option)
        {
            if (option == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(option.Name))
            {
                return option.Name.Trim();
            }

            string featureName = GetFirstGrantedFeatureName(option);
            return !string.IsNullOrWhiteSpace(featureName)
                ? featureName
                : string.Empty;
        }

        private static string BuildChoiceOptionDescription(DndChoiceOptionData option)
        {
            if (option == null)
            {
                return string.Empty;
            }

            string effectDescription = GetFirstGrantedEffectDescription(option);
            if (!string.IsNullOrWhiteSpace(effectDescription))
            {
                return effectDescription;
            }

            if (!string.IsNullOrWhiteSpace(option.Description))
            {
                return option.Description.Trim();
            }

            string featureDescription = GetFirstGrantedFeatureDescription(option);
            if (!string.IsNullOrWhiteSpace(featureDescription))
            {
                return featureDescription;
            }

            return string.Empty;
        }

        private static string GetFirstGrantedFeatureName(DndChoiceOptionData option)
        {
            if (option?.GrantFeatureIds == null)
            {
                return string.Empty;
            }

            for (int index = 0; index < option.GrantFeatureIds.Count; index++)
            {
                string featureId = option.GrantFeatureIds[index];
                if (DndRuleContentService.Instance.TryGetFeature(featureId, out DndFeatureDefineData feature)
                    && !string.IsNullOrWhiteSpace(feature.Name))
                {
                    return feature.Name.Trim();
                }
            }

            return string.Empty;
        }

        private static string GetFirstGrantedFeatureDescription(DndChoiceOptionData option)
        {
            if (option?.GrantFeatureIds == null)
            {
                return string.Empty;
            }

            for (int index = 0; index < option.GrantFeatureIds.Count; index++)
            {
                string featureId = option.GrantFeatureIds[index];
                if (DndRuleContentService.Instance.TryGetFeature(featureId, out DndFeatureDefineData feature)
                    && !string.IsNullOrWhiteSpace(feature.Description))
                {
                    return feature.Description.Trim();
                }
            }

            return string.Empty;
        }

        private static string GetFirstGrantedEffectDescription(DndChoiceOptionData option)
        {
            List<string> effectIds = new List<string>();
            if (option?.GrantEffectIds != null)
            {
                effectIds.AddRange(option.GrantEffectIds);
            }

            if (option?.GrantFeatureIds != null)
            {
                for (int index = 0; index < option.GrantFeatureIds.Count; index++)
                {
                    if (DndRuleContentService.Instance.TryGetFeature(option.GrantFeatureIds[index], out DndFeatureDefineData feature)
                        && feature.EffectIds != null)
                    {
                        effectIds.AddRange(feature.EffectIds);
                    }
                }
            }

            for (int index = 0; index < effectIds.Count; index++)
            {
                string effectId = effectIds[index];
                if (DndRuleContentService.Instance.TryGetFeatureEffect(effectId, out DndFeatureEffectData effect)
                    && !string.IsNullOrWhiteSpace(effect.ManualNote))
                {
                    return effect.ManualNote.Trim();
                }
            }

            return string.Empty;
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

        private static string GetClassDisplayName(CharacterClassProgressSaveData progress)
        {
            if (progress == null || string.IsNullOrWhiteSpace(progress.ClassId))
            {
                return string.Empty;
            }

            DndClassDefineData classData = FindClass(progress.ClassId);
            return classData != null && !string.IsNullOrWhiteSpace(classData.Name)
                ? classData.Name.Trim()
                : progress.ClassId.Trim();
        }

        private static string ResolveSubclassDisplayName(CharacterClassProgressSaveData progress)
        {
            if (progress == null || string.IsNullOrWhiteSpace(progress.ClassId) || string.IsNullOrWhiteSpace(progress.SubclassId))
            {
                return string.Empty;
            }

            string choiceGroupId = $"choice_subclass_{progress.ClassId.Trim()}";
            IReadOnlyList<DndChoiceOptionData> options = DndRuleContentService.Instance.GetChoiceOptions(choiceGroupId.Trim());
            for (int index = 0; index < options.Count; index++)
            {
                DndChoiceOptionData option = options[index];
                if (option == null || !string.Equals(option.OptionId, progress.SubclassId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return string.IsNullOrWhiteSpace(option.Name) ? string.Empty : option.Name.Trim();
            }

            return string.Empty;
        }

        private static void AppendConditionStatusEffects(
            List<CharacterStatusEffectDisplayEntry> entries,
            IReadOnlyList<CharacterConditionStateSaveData> conditions)
        {
            if (entries == null || conditions == null)
            {
                return;
            }

            for (int index = 0; index < conditions.Count; index++)
            {
                CharacterConditionStateSaveData condition = conditions[index];
                if (condition == null)
                {
                    continue;
                }

                string name = FirstNonEmpty(condition.Name, condition.ConditionId);
                if (condition.ExhaustionLevel > 0)
                {
                    name = string.IsNullOrWhiteSpace(name)
                        ? $"Exhaustion {condition.ExhaustionLevel}"
                        : $"{name} {condition.ExhaustionLevel}";
                }

                if (!string.IsNullOrWhiteSpace(name))
                {
                    entries.Add(new CharacterStatusEffectDisplayEntry(name.Trim(), condition.Duration));
                }
            }
        }

        private static void AppendTemporaryStatusEffects(
            List<CharacterStatusEffectDisplayEntry> entries,
            IReadOnlyList<CharacterTemporaryEffectSaveData> effects)
        {
            if (entries == null || effects == null)
            {
                return;
            }

            for (int index = 0; index < effects.Count; index++)
            {
                CharacterTemporaryEffectSaveData effect = effects[index];
                if (effect == null || !effect.IsActive)
                {
                    continue;
                }

                string name = FirstNonEmpty(effect.Name, effect.EffectId);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    entries.Add(new CharacterStatusEffectDisplayEntry(name.Trim(), effect.Duration));
                }
            }
        }

        private static void AppendInventoryItemEntries(List<CharacterInventoryDisplayEntry> entries, IReadOnlyList<CharacterEquipmentItemSaveData> items, bool equipped)
        {
            if (entries == null || items == null)
            {
                return;
            }

            for (int index = 0; index < items.Count; index++)
            {
                AppendInventoryItemEntry(entries, items[index], equipped);
            }
        }

        private static void AppendInventoryItemEntry(List<CharacterInventoryDisplayEntry> entries, CharacterEquipmentItemSaveData item, bool equipped)
        {
            if (entries == null || !CharacterEquipmentItemSaveData.HasItem(item))
            {
                return;
            }

            bool isEquipped = equipped || item.IsEquipped;
            string label = BuildInventoryItemLabel(item);
            if (!string.IsNullOrWhiteSpace(label))
            {
                string title = BuildInventoryItemName(item);
                string description = BuildInventoryItemDetailDescription(item, isEquipped);
                entries.Add(new CharacterInventoryDisplayEntry(label, title, description, isEquipped));
            }
        }

        private static string BuildInventoryItemLabel(CharacterEquipmentItemSaveData item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            string name = !string.IsNullOrWhiteSpace(item.ItemName)
                ? item.ItemName.Trim()
                : (item.ItemId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(name);
            if (item.Quantity > 1)
            {
                builder.Append(" x");
                builder.Append(item.Quantity);
            }

            return builder.ToString();
        }

        private static string BuildInventoryItemDetailDescription(CharacterEquipmentItemSaveData item, bool isEquipped)
        {
            if (item == null)
            {
                return string.Empty;
            }

            DndItemDefineData ruleItem = FindDndItemDefinition(item);
            StringBuilder builder = new StringBuilder();
            AppendDetailLine(builder, "类型", FirstNonEmpty(item.ItemType, ruleItem?.ItemType));
            AppendDetailLine(builder, "数量", Math.Max(1, item.Quantity).ToString());
            AppendDetailLine(builder, "状态", isEquipped || item.IsEquipped ? "已装备" : "背包中");
            AppendDetailLine(builder, "来源", BuildInventoryItemSourceText(item, ruleItem));
            AppendDetailLine(builder, "稀有度", ruleItem?.Rarity);
            AppendDetailLine(builder, "装备栏位", ruleItem?.EquipmentSlot);
            AppendDetailLine(builder, "护甲类型", FirstNonEmpty(item.ArmorCategory, ruleItem?.ArmorCategory));

            int armorBaseAc = item.ArmorBaseAc > 0 ? item.ArmorBaseAc : ruleItem?.ArmorBaseAc ?? 0;
            if (armorBaseAc > 0)
            {
                AppendDetailLine(builder, "护甲AC", armorBaseAc.ToString());
            }

            int acBonus = item.AcBonus != 0 ? item.AcBonus : ruleItem?.AcBonus ?? 0;
            if (acBonus != 0)
            {
                AppendDetailLine(builder, "AC加值", FormatSignedNumber(acBonus));
            }

            AppendDetailLine(builder, "伤害", BuildRuleItemDamageText(ruleItem));
            AppendDetailLine(builder, "武器属性", FormatList(ruleItem?.WeaponProperties));
            AppendDetailLine(builder, "重量", ruleItem != null && ruleItem.Weight > 0f ? $"{ruleItem.Weight:g}" : string.Empty);
            AppendDetailLine(builder, "价格", ruleItem != null && ruleItem.PriceGp > 0 ? $"{ruleItem.PriceGp} gp" : string.Empty);

            if (item.RequiresAttunement || ruleItem != null && ruleItem.RequiresAttunement)
            {
                AppendDetailLine(builder, "同调", item.IsAttuned ? "已同调" : "需要同调");
            }

            AppendDetailLine(builder, "描述", FirstNonEmpty(item.Description, ruleItem?.Description));
            AppendDetailLine(builder, "效果", BuildInventoryItemEffectText(item, ruleItem));
            AppendDetailLine(builder, "备注", item.Notes);
            return builder.ToString();
        }

        private static string BuildInventoryItemName(CharacterEquipmentItemSaveData item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            DndItemDefineData ruleItem = FindDndItemDefinition(item);
            return FirstNonEmpty(item.ItemName, ruleItem?.Name, item.ItemId, item.SourceItemId);
        }

        private static DndItemDefineData FindDndItemDefinition(CharacterEquipmentItemSaveData item)
        {
            if (item == null)
            {
                return null;
            }

            if (DndRuleContentService.Instance.TryGetItem(item.SourceItemId, out DndItemDefineData sourceItem))
            {
                return sourceItem;
            }

            if (DndRuleContentService.Instance.TryGetItem(item.ItemId, out DndItemDefineData itemData))
            {
                return itemData;
            }

            return null;
        }

        private static string BuildInventoryItemSourceText(CharacterEquipmentItemSaveData item, DndItemDefineData ruleItem)
        {
            string sourceType = item != null
                ? CharacterItemSourceTypes.Normalize(item.ItemSourceType)
                : string.Empty;
            if (sourceType == CharacterItemSourceTypes.RuleTable)
            {
                return FirstNonEmpty(ruleItem?.SourceBook, "规则表");
            }

            if (sourceType == CharacterItemSourceTypes.Custom)
            {
                return "自定义物品";
            }

            return string.IsNullOrWhiteSpace(sourceType) ? string.Empty : sourceType;
        }

        private static string BuildRuleItemDamageText(DndItemDefineData item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.DamageDice))
            {
                return string.Empty;
            }

            string damage = item.DamageDice.Trim();
            if (!string.IsNullOrWhiteSpace(item.DamageType))
            {
                damage = $"{damage} {item.DamageType.Trim()}";
            }

            if (!string.IsNullOrWhiteSpace(item.TwoHandDamageDice))
            {
                damage = $"{damage} / two-hand {item.TwoHandDamageDice.Trim()}";
            }

            if (item.NormalRange > 0 || item.LongRange > 0)
            {
                damage = $"{damage} ({item.NormalRange}/{item.LongRange})";
            }

            return damage;
        }

        private static string BuildInventoryItemEffectText(CharacterEquipmentItemSaveData item, DndItemDefineData ruleItem)
        {
            List<string> parts = new List<string>();
            AppendFeatureEffectTexts(parts, ruleItem?.EffectIds);
            AppendFeatureEffectTexts(parts, item?.EffectIds);

            if (item?.CustomEffects != null)
            {
                for (int index = 0; index < item.CustomEffects.Count; index++)
                {
                    CharacterItemEffectSaveData effect = item.CustomEffects[index];
                    if (effect == null)
                    {
                        continue;
                    }

                    string text = FirstNonEmpty(effect.Description, BuildInlineEffectText(effect.EffectType, effect.Target, effect.Value, effect.Condition));
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        parts.Add(text);
                    }
                }
            }

            return parts.Count > 0 ? string.Join("\n", parts) : string.Empty;
        }

        private static void AppendFeatureEffectTexts(List<string> target, IReadOnlyList<string> effectIds)
        {
            if (target == null || effectIds == null)
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

                if (DndRuleContentService.Instance.TryGetFeatureEffect(effectId.Trim(), out DndFeatureEffectData effect))
                {
                    string text = FirstNonEmpty(effect.ManualNote, BuildInlineEffectText(effect.EffectType, effect.Target, effect.Value, effect.Condition));
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        target.Add(text);
                    }
                }
                else
                {
                    target.Add(effectId.Trim());
                }
            }
        }

        private static string BuildInlineEffectText(string effectType, string target, string value, string condition)
        {
            string main = JoinNonEmpty(new[] { effectType, target, value }, string.Empty);
            if (string.IsNullOrWhiteSpace(condition))
            {
                return main;
            }

            return string.IsNullOrWhiteSpace(main) ? condition.Trim() : $"{main} ({condition.Trim()})";
        }

        private void AppendFeatureDisplayEntries(List<ClassFeatureDisplayEntry> entries, IReadOnlyList<string> featureIds)
        {
            if (entries == null || featureIds == null)
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

                string title = featureId.Trim();
                string description = string.Empty;
                if (DndRuleContentService.Instance.TryGetFeature(featureId, out DndFeatureDefineData feature))
                {
                    if (!string.IsNullOrWhiteSpace(feature.Name))
                    {
                        title = feature.Name.Trim();
                    }

                    description = BuildFeatureDescription(feature);
                }

                if (!ContainsFeatureDisplayEntry(entries, title, description))
                {
                    entries.Add(new ClassFeatureDisplayEntry(title, description));
                }
            }
        }

        private static bool ContainsFeatureDisplayEntry(IReadOnlyList<ClassFeatureDisplayEntry> entries, string title, string description)
        {
            if (entries == null)
            {
                return false;
            }

            string normalizedTitle = title?.Trim() ?? string.Empty;
            string normalizedDescription = description?.Trim() ?? string.Empty;
            for (int index = 0; index < entries.Count; index++)
            {
                ClassFeatureDisplayEntry entry = entries[index];
                if (string.Equals(entry.Title?.Trim() ?? string.Empty, normalizedTitle, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(entry.Description?.Trim() ?? string.Empty, normalizedDescription, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static void AppendEquipmentSummaryValues(List<string> target, string summary)
        {
            if (target == null || string.IsNullOrWhiteSpace(summary))
            {
                return;
            }

            string[] parts = summary.Split(new[] { ',', '\uFF0C', ';', '\uFF1B', '/', '\u3001', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            for (int index = 0; index < parts.Length; index++)
            {
                string value = parts[index]?.Trim();
                if (string.IsNullOrWhiteSpace(value) || string.Equals(value, "无", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                AppendUniqueValue(target, value);
            }
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

        private static void AppendDetailLine(StringBuilder builder, string label, string value)
        {
            if (builder == null || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.Append(label);
            builder.Append(": ");
            builder.Append(value.Trim());
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

            return labels.Count > 0 ? string.Join("、", labels) : string.Empty;
        }

        private static string FormatSignedNumber(int value)
        {
            return value >= 0 ? $"+{value}" : value.ToString();
        }

        private static string JoinNonEmpty(IEnumerable<string> values, string emptyText)
        {
            List<string> parts = new List<string>();
            if (values != null)
            {
                foreach (string value in values)
                {
                    if (!string.IsNullOrWhiteSpace(value) && value.Trim() != "无")
                    {
                        parts.Add(value.Trim());
                    }
                }
            }

            return parts.Count > 0 ? string.Join(" / ", parts) : emptyText;
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            for (int index = 0; index < values.Length; index++)
            {
                string value = values[index];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return string.Empty;
        }

        private static DndRaceDefineData FindRace(string raceId)
        {
            return FindById(DndRuleContentService.Instance.Races, raceId, data => data.RaceId);
        }

        private static DndClassDefineData FindClass(string classId)
        {
            return DndRuleContentService.Instance.TryGetClass(classId, out DndClassDefineData classData)
                ? classData
                : null;
        }

        private static DndFeatDefineData FindFeat(string featId)
        {
            return FindById(DndRuleContentService.Instance.Feats, featId, data => data.FeatId);
        }

        private static string FormatTextOrDefault(string value, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
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
