using System;
using System.Collections.Generic;

namespace GameLogic
{
    internal sealed class CharacterCreationFeatureDisplayService
    {
        private static readonly Lazy<CharacterCreationFeatureDisplayService> s_instance =
            new Lazy<CharacterCreationFeatureDisplayService>(() => new CharacterCreationFeatureDisplayService());

        private CharacterCreationFeatureDisplayService()
        {
        }

        public static CharacterCreationFeatureDisplayService Instance => s_instance.Value;

        public List<CharacterCreationFeatureDisplayEntry> BuildClassFeatureEntries(string classId, int level)
        {
            List<CharacterCreationFeatureDisplayEntry> entries = new List<CharacterCreationFeatureDisplayEntry>();
            IReadOnlyList<DndLevelProgressionData> progressions = DndRuleContentService.Instance.GetClassProgressions(classId);
            int maxLevel = Math.Max(1, level);
            for (int index = 0; index < progressions.Count; index++)
            {
                DndLevelProgressionData progression = progressions[index];
                if (progression == null || progression.Level < 1 || progression.Level > maxLevel)
                {
                    continue;
                }

                for (int featureIndex = 0; featureIndex < progression.FeatureIds.Count; featureIndex++)
                {
                    string featureId = progression.FeatureIds[featureIndex];
                    if (string.IsNullOrWhiteSpace(featureId))
                    {
                        continue;
                    }

                    if (DndRuleContentService.Instance.TryGetFeature(featureId, out DndFeatureDefineData feature))
                    {
                        IReadOnlyList<string> choiceGroupIds = BuildFeatureChoiceGroupIds(featureIndex == 0 ? progression : null, feature);
                        if (IsCompletedSubclassChoiceFeature(choiceGroupIds))
                        {
                            continue;
                        }

                        AddFeatureDisplayEntries(entries, CreateFeatureEntry(
                            feature.FeatureId,
                            feature.Name,
                            feature.Description,
                            "Class",
                            classId,
                            progression.Level,
                            choiceGroupIds));
                    }
                    else
                    {
                        IReadOnlyList<string> choiceGroupIds = featureIndex == 0
                            ? BuildFeatureChoiceGroupIds(progression, null)
                            : Array.Empty<string>();
                        AddFeatureDisplayEntries(entries, CreateFeatureEntry(
                            featureId.Trim(),
                            featureId.Trim(),
                            string.Empty,
                            "Class",
                            classId,
                            progression.Level,
                            choiceGroupIds));
                    }
                }
            }

            AppendSelectedSubclassFeatureEntries(entries, classId, maxLevel);
            return entries;
        }

        public List<CharacterCreationFeatureDisplayEntry> BuildFeatureEntries(IReadOnlyList<string> featureIds, string sourceType, string sourceId)
        {
            List<CharacterCreationFeatureDisplayEntry> entries = new List<CharacterCreationFeatureDisplayEntry>();
            if (featureIds == null)
            {
                return entries;
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
                    AddFeatureDisplayEntries(entries, CreateFeatureEntry(feature.FeatureId, feature.Name, feature.Description, sourceType, sourceId, 0, feature.ChoiceGroupIds));
                }
                else
                {
                    AddFeatureDisplayEntries(entries, CreateFeatureEntry(featureId.Trim(), featureId.Trim(), string.Empty, sourceType, sourceId, 0, null));
                }
            }

            return entries;
        }

        public string BuildFeatureEntryDisplayTitle(CharacterCreationFeatureDisplayEntry entry)
        {
            if (entry == null)
            {
                return string.Empty;
            }

            if (entry.IsChoiceOptionDisplay)
            {
                return entry.Title;
            }

            if (TryBuildSelectedToolChoiceTitle(entry, out string toolChoiceTitle))
            {
                return toolChoiceTitle;
            }

            CharacterCreationFeatureChoiceState state = TryGetDisplayFeatureChoiceState(entry);
            if (!IsFeatureChoiceCompleted(state))
            {
                return entry.Title;
            }

            if (IsAdvancementFollowupIncomplete(state))
            {
                return entry.Title;
            }

            if (TryBuildAdvancementChoiceTitle(state, out string advancementTitle))
            {
                return advancementTitle;
            }

            string selectedTitle = BuildSelectedFeatureChoiceTitle(state);
            return string.IsNullOrWhiteSpace(selectedTitle)
                ? entry.Title
                : $"{entry.Title}-{selectedTitle}";
        }

        public string BuildFeatureEntryDisplayDescription(CharacterCreationFeatureDisplayEntry entry)
        {
            if (entry == null)
            {
                return string.Empty;
            }

            if (entry.IsChoiceOptionDisplay)
            {
                return entry.Description;
            }

            if (TryBuildSelectedToolChoiceDescription(entry, out string toolChoiceDescription))
            {
                return toolChoiceDescription;
            }

            CharacterCreationFeatureChoiceState state = TryGetDisplayFeatureChoiceState(entry);
            if (!IsFeatureChoiceCompleted(state))
            {
                return entry.Description;
            }

            if (IsAdvancementFollowupIncomplete(state))
            {
                return entry.Description;
            }

            if (TryBuildAdvancementChoiceDescription(state, out string advancementDescription))
            {
                return advancementDescription;
            }

            string description = BuildSelectedFeatureChoiceDescription(state);
            return string.IsNullOrWhiteSpace(description) ? entry.Description : description;
        }

        public string GetSelectedFeatureChoiceDisplayName(CharacterCreationFeatureChoiceState state)
        {
            if (state == null || state.SelectedOptionIds.Count == 0)
            {
                return string.Empty;
            }

            string optionId = state.SelectedOptionIds[0];
            if (state.OptionDisplayNameById.TryGetValue(optionId, out string displayName) && !string.IsNullOrWhiteSpace(displayName))
            {
                return displayName;
            }

            return GetChoiceOptionDisplayName(state.ChoiceGroupId, optionId);
        }

        public bool IsFeatureChoiceCompleted(CharacterCreationFeatureChoiceState state)
        {
            if (state == null || !state.IsConfirmed || state.SelectedOptionIds.Count == 0)
            {
                return false;
            }

            int requiredCount = state.MinSelect > 0 ? state.MinSelect : Math.Max(1, state.MaxSelect);
            return state.MaxSelect <= 0
                ? state.SelectedOptionIds.Count > 0
                : state.SelectedOptionIds.Count >= requiredCount;
        }

        public CharacterCreationFeatureChoiceState TryGetDisplayFeatureChoiceState(CharacterCreationFeatureDisplayEntry entry)
        {
            if (entry == null)
            {
                return null;
            }

            if (entry.IsChoiceOptionDisplay)
            {
                return CharacterCreationSessionService.Instance.FindFeatureChoiceState(entry.ChoiceGroupId);
            }

            if (entry.ChoiceGroupIds.Count == 0)
            {
                return null;
            }

            for (int index = 0; index < entry.ChoiceGroupIds.Count; index++)
            {
                CharacterCreationFeatureChoiceState state = CharacterCreationSessionService.Instance.FindFeatureChoiceState(entry.ChoiceGroupIds[index]);
                if (state != null)
                {
                    return state;
                }
            }

            return null;
        }

        public CharacterCreationToolChoiceState TryGetDisplayToolChoiceState(CharacterCreationFeatureDisplayEntry entry)
        {
            if (entry == null || entry.ChoiceGroupIds.Count == 0)
            {
                return null;
            }

            for (int index = 0; index < entry.ChoiceGroupIds.Count; index++)
            {
                CharacterCreationToolChoiceState state = CharacterCreationSessionService.Instance.FindToolChoiceState(entry.ChoiceGroupIds[index]);
                if (state != null)
                {
                    return state;
                }
            }

            return null;
        }

        public string GetChoiceOptionDisplayName(string choiceGroupId, string optionId)
        {
            if (string.IsNullOrWhiteSpace(choiceGroupId) || string.IsNullOrWhiteSpace(optionId))
            {
                return string.Empty;
            }

            IReadOnlyList<DndChoiceOptionData> options = DndRuleContentService.Instance.GetChoiceOptions(choiceGroupId);
            for (int index = 0; index < options.Count; index++)
            {
                DndChoiceOptionData option = options[index];
                if (option != null && string.Equals(option.OptionId, optionId, StringComparison.OrdinalIgnoreCase))
                {
                    return FormatChoiceOptionDisplayName(choiceGroupId, FirstNonEmpty(option.Name, option.OptionId));
                }
            }

            return FormatChoiceOptionDisplayName(choiceGroupId, optionId.Trim());
        }

        public string GetChoiceOptionDescription(string choiceGroupId, string optionId)
        {
            DndChoiceOptionData option = FindChoiceOption(choiceGroupId, optionId);
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
            return !string.IsNullOrWhiteSpace(featureDescription) ? featureDescription : string.Empty;
        }

        public string GetChoiceOptionDetailTitle(string choiceGroupId, string optionId)
        {
            DndChoiceOptionData option = FindChoiceOption(choiceGroupId, optionId);
            if (TryResolveFeatFromChoiceOption(option, out DndFeatDefineData feat)
                && !string.IsNullOrWhiteSpace(feat.Name))
            {
                return feat.Name.Trim();
            }

            string optionName = GetChoiceOptionDisplayName(choiceGroupId, optionId);
            if (!string.IsNullOrWhiteSpace(optionName))
            {
                return optionName.Trim();
            }

            return optionId?.Trim() ?? string.Empty;
        }

        public string GetChoiceOptionDetailDescription(string choiceGroupId, string optionId)
        {
            DndChoiceOptionData option = FindChoiceOption(choiceGroupId, optionId);
            if (TryResolveFeatFromChoiceOption(option, out DndFeatDefineData feat)
                && !string.IsNullOrWhiteSpace(feat.Description))
            {
                return feat.Description.Trim();
            }

            return GetChoiceOptionDescription(choiceGroupId, optionId);
        }

        private static string FormatChoiceOptionDisplayName(string choiceGroupId, string optionId)
        {
            string normalized = optionId?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return string.Empty;
            }

            if (normalized.StartsWith("skill:", StringComparison.OrdinalIgnoreCase))
            {
                return GetSkillDisplayName(normalized.Substring("skill:".Length));
            }

            if (normalized.StartsWith("tool:", StringComparison.OrdinalIgnoreCase))
            {
                return CharacterEquipmentProficiencyDisplayService.Instance.GetToolDisplayName(normalized.Substring("tool:".Length));
            }

            if (DndRuleContentService.Instance.TryGetChoiceGroup(choiceGroupId, out DndChoiceGroupData choiceGroup)
                && choiceGroup != null)
            {
                if (string.Equals(choiceGroup.ChoiceType, "Weapon", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(choiceGroup.ResultValueType, "WeaponProficiency", StringComparison.OrdinalIgnoreCase))
                {
                    return CharacterEquipmentProficiencyDisplayService.Instance.GetWeaponDisplayName(normalized);
                }

                if (string.Equals(choiceGroup.ChoiceType, "Skill", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(choiceGroup.ResultValueType, "SkillProficiency", StringComparison.OrdinalIgnoreCase))
                {
                    return GetSkillDisplayName(normalized);
                }

                if (string.Equals(choiceGroup.ChoiceType, "Tool", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(choiceGroup.ResultValueType, "ToolProficiency", StringComparison.OrdinalIgnoreCase))
                {
                    return CharacterEquipmentProficiencyDisplayService.Instance.GetToolDisplayName(normalized);
                }
            }

            return normalized;
        }

        private static string GetSkillDisplayName(string skillId)
        {
            string normalized = skillId?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return string.Empty;
            }

            return DndRuleContentService.Instance.TryGetSkill(normalized, out DndSkillDefineData skill)
                && skill != null
                && !string.IsNullOrWhiteSpace(skill.Name)
                    ? skill.Name.Trim()
                    : normalized;
        }

        private void AddFeatureDisplayEntries(List<CharacterCreationFeatureDisplayEntry> entries, CharacterCreationFeatureDisplayEntry entry)
        {
            if (entries == null || entry == null)
            {
                return;
            }

            if (TryBuildSelectedToolChoiceEntries(entry, entries))
            {
                return;
            }

            CharacterCreationFeatureChoiceState state = TryGetDisplayFeatureChoiceState(entry);
            if (!IsFeatureChoiceCompleted(state) || state.SelectedOptionIds.Count <= 1)
            {
                AddUniqueFeatureEntry(entries, entry);
                return;
            }

            for (int index = 0; index < state.SelectedOptionIds.Count; index++)
            {
                string optionId = state.SelectedOptionIds[index];
                if (string.IsNullOrWhiteSpace(optionId))
                {
                    continue;
                }

                string optionName = GetFeatureChoiceOptionDisplayName(state, optionId);
                string optionDescription = GetChoiceOptionDescription(state.ChoiceGroupId, optionId);
                CharacterCreationFeatureDisplayEntry optionEntry = new CharacterCreationFeatureDisplayEntry(
                    entry.FeatureId,
                    string.IsNullOrWhiteSpace(optionName) ? entry.Title : $"{entry.Title}-{optionName}",
                    string.IsNullOrWhiteSpace(optionDescription) ? entry.Description : optionDescription.Trim(),
                    entry.SourceType,
                    entry.SourceId,
                    entry.Level)
                {
                    IsChoiceOptionDisplay = true,
                    ChoiceGroupId = state.ChoiceGroupId,
                    ChoiceOptionId = optionId.Trim()
                };

                AddUniqueFeatureEntry(entries, optionEntry);
            }
        }

        private bool TryBuildSelectedToolChoiceEntries(CharacterCreationFeatureDisplayEntry entry, List<CharacterCreationFeatureDisplayEntry> entries)
        {
            CharacterCreationToolChoiceState state = TryGetDisplayToolChoiceState(entry);
            if (state == null || !state.IsCompleted)
            {
                return false;
            }

            bool added = false;
            for (int index = 0; index < state.SelectedToolIds.Count; index++)
            {
                string toolId = state.SelectedToolIds[index];
                DndChoiceOptionData option = FindToolChoiceOption(state, toolId);
                if (option == null)
                {
                    continue;
                }

                if (option.GrantFeatureIds != null && option.GrantFeatureIds.Count > 0)
                {
                    for (int featureIndex = 0; featureIndex < option.GrantFeatureIds.Count; featureIndex++)
                    {
                        string featureId = option.GrantFeatureIds[featureIndex];
                        if (string.IsNullOrWhiteSpace(featureId))
                        {
                            continue;
                        }

                        CharacterCreationFeatureDisplayEntry selectedEntry = DndRuleContentService.Instance.TryGetFeature(featureId, out DndFeatureDefineData feature)
                            ? CreateFeatureEntry(feature.FeatureId, feature.Name, feature.Description, entry.SourceType, entry.SourceId, entry.Level, null)
                            : CreateFeatureEntry(featureId.Trim(), featureId.Trim(), string.Empty, entry.SourceType, entry.SourceId, entry.Level, null);
                        selectedEntry.IsChoiceOptionDisplay = true;
                        selectedEntry.ChoiceGroupId = state.ChoiceGroupId;
                        selectedEntry.ChoiceOptionId = FirstNonEmpty(option.OptionId, toolId);
                        AddUniqueFeatureEntry(entries, selectedEntry);
                        added = true;
                    }

                    continue;
                }

                string toolName = CharacterCreationEquipmentToolDisplayService.Instance.GetToolDisplayName(toolId);
                string title = string.IsNullOrWhiteSpace(toolName) ? entry.Title : $"{entry.Title}-{toolName}";
                CharacterCreationFeatureDisplayEntry fallbackEntry = new CharacterCreationFeatureDisplayEntry(
                    entry.FeatureId,
                    title,
                    GetChoiceOptionDescription(state.ChoiceGroupId, option.OptionId),
                    entry.SourceType,
                    entry.SourceId,
                    entry.Level)
                {
                    IsChoiceOptionDisplay = true,
                    ChoiceGroupId = state.ChoiceGroupId,
                    ChoiceOptionId = FirstNonEmpty(option.OptionId, toolId)
                };
                AddUniqueFeatureEntry(entries, fallbackEntry);
                added = true;
            }

            return added;
        }

        private void AppendSelectedSubclassFeatureEntries(
            List<CharacterCreationFeatureDisplayEntry> entries,
            string classId,
            int classLevel)
        {
            string subclassId = CharacterCreationSessionService.Instance.GetSelectedSubclassId(classId);
            if (entries == null || string.IsNullOrWhiteSpace(classId) || string.IsNullOrWhiteSpace(subclassId))
            {
                return;
            }

            IReadOnlyList<DndSubclassLevelProgressionData> progressions = DndRuleContentService.Instance.GetSubclassProgressions(subclassId.Trim());
            for (int index = 0; index < progressions.Count; index++)
            {
                DndSubclassLevelProgressionData progression = progressions[index];
                if (progression == null || progression.Level < 1 || progression.Level > classLevel)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(progression.ClassId)
                    && !string.Equals(progression.ClassId.Trim(), classId.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                AppendSubclassProgressionFeatureEntries(entries, progression);
            }
        }

        private void AppendSubclassProgressionFeatureEntries(
            List<CharacterCreationFeatureDisplayEntry> entries,
            DndSubclassLevelProgressionData progression)
        {
            if (entries == null || progression?.FeatureIds == null)
            {
                return;
            }

            for (int featureIndex = 0; featureIndex < progression.FeatureIds.Count; featureIndex++)
            {
                string featureId = progression.FeatureIds[featureIndex];
                if (string.IsNullOrWhiteSpace(featureId))
                {
                    continue;
                }

                IReadOnlyList<string> choiceGroupIds = featureIndex == 0
                    ? BuildFeatureChoiceGroupIds(progression, null)
                    : Array.Empty<string>();
                if (DndRuleContentService.Instance.TryGetFeature(featureId, out DndFeatureDefineData feature))
                {
                    List<string> featureChoiceGroupIds = new List<string>();
                    AppendUniqueValues(featureChoiceGroupIds, choiceGroupIds);
                    AppendUniqueValues(featureChoiceGroupIds, feature.ChoiceGroupIds);
                    AddUniqueFeatureEntry(entries, CreateFeatureEntry(feature.FeatureId, feature.Name, feature.Description, "Subclass", progression.SubclassId, progression.Level, featureChoiceGroupIds));
                }
                else
                {
                    AddUniqueFeatureEntry(entries, CreateFeatureEntry(featureId.Trim(), featureId.Trim(), string.Empty, "Subclass", progression.SubclassId, progression.Level, choiceGroupIds));
                }
            }
        }

        private static void AddUniqueFeatureEntry(List<CharacterCreationFeatureDisplayEntry> entries, CharacterCreationFeatureDisplayEntry entry)
        {
            if (entries == null || entry == null)
            {
                return;
            }

            for (int index = 0; index < entries.Count; index++)
            {
                CharacterCreationFeatureDisplayEntry existing = entries[index];
                if (existing != null
                    && string.Equals(existing.FeatureId, entry.FeatureId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(existing.Title, entry.Title, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            entries.Add(entry);
        }

        private string BuildSelectedFeatureChoiceTitle(CharacterCreationFeatureChoiceState state)
        {
            if (state == null || state.SelectedOptionIds.Count == 0)
            {
                return string.Empty;
            }

            List<string> names = new List<string>();
            for (int index = 0; index < state.SelectedOptionIds.Count; index++)
            {
                string optionId = state.SelectedOptionIds[index];
                string name = state.OptionDisplayNameById.TryGetValue(optionId, out string displayName) && !string.IsNullOrWhiteSpace(displayName)
                    ? displayName.Trim()
                    : GetChoiceOptionDisplayName(state.ChoiceGroupId, optionId);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    names.Add(name);
                }
            }

            return string.Join("/", names);
        }

        private string GetFeatureChoiceOptionDisplayName(CharacterCreationFeatureChoiceState state, string optionId)
        {
            if (state == null || string.IsNullOrWhiteSpace(optionId))
            {
                return string.Empty;
            }

            if (state.OptionDisplayNameById.TryGetValue(optionId, out string displayName) && !string.IsNullOrWhiteSpace(displayName))
            {
                return displayName.Trim();
            }

            return GetChoiceOptionDisplayName(state.ChoiceGroupId, optionId);
        }

        private bool TryBuildSelectedToolChoiceTitle(CharacterCreationFeatureDisplayEntry entry, out string title)
        {
            title = string.Empty;
            CharacterCreationToolChoiceState state = TryGetDisplayToolChoiceState(entry);
            if (entry == null || state == null || !state.IsCompleted)
            {
                return false;
            }

            List<string> titles = new List<string>();
            for (int index = 0; index < state.SelectedToolIds.Count; index++)
            {
                string toolId = state.SelectedToolIds[index];
                DndChoiceOptionData option = FindToolChoiceOption(state, toolId);
                string grantedFeatureName = GetFirstGrantedFeatureName(option);
                if (!string.IsNullOrWhiteSpace(grantedFeatureName))
                {
                    titles.Add(grantedFeatureName);
                    continue;
                }

                string toolName = CharacterCreationEquipmentToolDisplayService.Instance.GetToolDisplayName(toolId);
                titles.Add(string.IsNullOrWhiteSpace(toolName) ? entry.Title : $"{entry.Title}-{toolName}");
            }

            title = titles.Count > 0 ? string.Join("/", titles) : string.Empty;
            return !string.IsNullOrWhiteSpace(title);
        }

        private bool TryBuildSelectedToolChoiceDescription(CharacterCreationFeatureDisplayEntry entry, out string description)
        {
            description = string.Empty;
            CharacterCreationToolChoiceState state = TryGetDisplayToolChoiceState(entry);
            if (state == null || !state.IsCompleted)
            {
                return false;
            }

            List<string> descriptions = new List<string>();
            for (int index = 0; index < state.SelectedToolIds.Count; index++)
            {
                DndChoiceOptionData option = FindToolChoiceOption(state, state.SelectedToolIds[index]);
                string optionDescription = BuildToolChoiceOptionDescription(option);
                if (!string.IsNullOrWhiteSpace(optionDescription))
                {
                    descriptions.Add(optionDescription);
                }
            }

            description = descriptions.Count > 0 ? string.Join("\n", descriptions) : string.Empty;
            return !string.IsNullOrWhiteSpace(description);
        }

        private string BuildSelectedFeatureChoiceDescription(CharacterCreationFeatureChoiceState state)
        {
            if (state == null || state.SelectedOptionIds.Count == 0)
            {
                return string.Empty;
            }

            List<string> descriptions = new List<string>();
            for (int index = 0; index < state.SelectedOptionIds.Count; index++)
            {
                string description = GetChoiceOptionDescription(state.ChoiceGroupId, state.SelectedOptionIds[index]);
                if (!string.IsNullOrWhiteSpace(description))
                {
                    descriptions.Add(description.Trim());
                }
            }

            return string.Join("\n", descriptions);
        }

        private bool TryBuildAdvancementChoiceTitle(CharacterCreationFeatureChoiceState state, out string title)
        {
            title = string.Empty;
            if (!IsAdvancementOptionChoice(state) || state.SelectedOptionIds.Count == 0)
            {
                return false;
            }

            string parentTitle = GetFeatureChoiceOptionDisplayName(state, state.SelectedOptionIds[0]);
            CharacterCreationFeatureChoiceState followupState = TryGetAdvancementFollowupChoiceState(state);
            if (!IsFeatureChoiceCompleted(followupState))
            {
                title = parentTitle;
                return !string.IsNullOrWhiteSpace(title);
            }

            string followupTitle = BuildSelectedFeatureChoiceTitle(followupState);
            title = string.IsNullOrWhiteSpace(followupTitle)
                ? parentTitle
                : $"{parentTitle}-{followupTitle}";
            return !string.IsNullOrWhiteSpace(title);
        }

        private static bool IsAdvancementFollowupIncomplete(CharacterCreationFeatureChoiceState state)
        {
            if (!IsAdvancementOptionChoice(state) || state.SelectedOptionIds.Count == 0)
            {
                return false;
            }

            CharacterCreationFeatureChoiceState followupState = TryGetAdvancementFollowupChoiceState(state);
            return followupState != null && !CharacterCreationFeatureDisplayService.Instance.IsFeatureChoiceCompleted(followupState);
        }

        private bool TryBuildAdvancementChoiceDescription(CharacterCreationFeatureChoiceState state, out string description)
        {
            description = string.Empty;
            if (!IsAdvancementOptionChoice(state) || state.SelectedOptionIds.Count == 0)
            {
                return false;
            }

            CharacterCreationFeatureChoiceState followupState = TryGetAdvancementFollowupChoiceState(state);
            if (IsFeatureChoiceCompleted(followupState))
            {
                description = BuildSelectedFeatureChoiceDescription(followupState);
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                description = GetChoiceOptionDescription(state.ChoiceGroupId, state.SelectedOptionIds[0]);
            }

            return !string.IsNullOrWhiteSpace(description);
        }

        private static bool IsAdvancementOptionChoice(CharacterCreationFeatureChoiceState state)
        {
            return state != null
                && (string.Equals(state.ChoiceType, "AdvancementOption", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(state.ChoiceType, "FeatOrAbilityScore", StringComparison.OrdinalIgnoreCase));
        }

        private static CharacterCreationFeatureChoiceState TryGetAdvancementFollowupChoiceState(CharacterCreationFeatureChoiceState state)
        {
            if (!IsAdvancementOptionChoice(state) || state.SelectedOptionIds.Count == 0)
            {
                return null;
            }

            string followupChoiceGroupId = ResolveAdvancementFollowupChoiceGroupId(state.ChoiceGroupId, state.SelectedOptionIds[0]);
            return string.IsNullOrWhiteSpace(followupChoiceGroupId)
                ? null
                : CharacterCreationSessionService.Instance.FindFeatureChoiceState(followupChoiceGroupId);
        }

        private static string ResolveAdvancementFollowupChoiceGroupId(string parentChoiceGroupId, string optionId)
        {
            string normalized = optionId?.Trim() ?? string.Empty;
            if (DndRuleContentService.Instance.TryGetChoiceGroup(parentChoiceGroupId, out DndChoiceGroupData parentGroup)
                && parentGroup?.NextChoiceGroupIds != null
                && parentGroup.NextChoiceGroupIds.Count > 0)
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
                return "choice_feat";
            }

            return string.Empty;
        }

        private static CharacterCreationFeatureDisplayEntry CreateFeatureEntry(
            string featureId,
            string title,
            string description,
            string sourceType,
            string sourceId,
            int level,
            IReadOnlyList<string> choiceGroupIds)
        {
            string normalizedTitle = FirstNonEmpty(title, featureId);
            CharacterCreationFeatureDisplayEntry entry = new CharacterCreationFeatureDisplayEntry(
                featureId ?? string.Empty,
                normalizedTitle,
                string.IsNullOrWhiteSpace(description) ? "暂无描述" : description.Trim(),
                sourceType,
                sourceId,
                level);
            AppendUniqueValues(entry.ChoiceGroupIds, choiceGroupIds);
            return entry;
        }

        private static List<string> BuildFeatureChoiceGroupIds(DndLevelProgressionData progression, DndFeatureDefineData feature)
        {
            List<string> choiceGroupIds = new List<string>();
            AppendUniqueValues(choiceGroupIds, progression?.ChoiceGroupIds);
            AppendUniqueValues(choiceGroupIds, feature?.ChoiceGroupIds);
            return choiceGroupIds;
        }

        private static List<string> BuildFeatureChoiceGroupIds(DndSubclassLevelProgressionData progression, DndFeatureDefineData feature)
        {
            List<string> choiceGroupIds = new List<string>();
            AppendUniqueValues(choiceGroupIds, progression?.ChoiceGroupIds);
            AppendUniqueValues(choiceGroupIds, feature?.ChoiceGroupIds);
            return choiceGroupIds;
        }

        private static bool IsCompletedSubclassChoiceFeature(IReadOnlyList<string> choiceGroupIds)
        {
            if (choiceGroupIds == null)
            {
                return false;
            }

            for (int index = 0; index < choiceGroupIds.Count; index++)
            {
                string choiceGroupId = choiceGroupIds[index];
                if (!IsSubclassChoiceGroup(choiceGroupId))
                {
                    continue;
                }

                CharacterCreationFeatureChoiceState state = CharacterCreationSessionService.Instance.FindFeatureChoiceState(choiceGroupId);
                if (CharacterCreationFeatureDisplayService.Instance.IsFeatureChoiceCompleted(state))
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

        private static DndChoiceOptionData FindChoiceOption(string choiceGroupId, string optionId)
        {
            if (string.IsNullOrWhiteSpace(choiceGroupId) || string.IsNullOrWhiteSpace(optionId))
            {
                return null;
            }

            IReadOnlyList<DndChoiceOptionData> options = DndRuleContentService.Instance.GetChoiceOptions(choiceGroupId.Trim());
            for (int index = 0; index < options.Count; index++)
            {
                DndChoiceOptionData option = options[index];
                if (option != null && string.Equals(option.OptionId, optionId.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return option;
                }
            }

            return null;
        }

        private static DndChoiceOptionData FindToolChoiceOption(CharacterCreationToolChoiceState state, string toolId)
        {
            if (state == null || string.IsNullOrWhiteSpace(toolId))
            {
                return null;
            }

            string optionId = state.OptionIdByToolId.TryGetValue(toolId.Trim(), out string mappedOptionId)
                ? mappedOptionId
                : toolId.Trim();
            return FindChoiceOption(state.ChoiceGroupId, optionId);
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

        private static bool TryResolveFeatFromChoiceOption(DndChoiceOptionData option, out DndFeatDefineData feat)
        {
            feat = null;
            if (option == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(option.OptionId)
                && DndRuleContentService.Instance.TryGetFeat(option.OptionId.Trim(), out feat))
            {
                return true;
            }

            if (option.GrantFeatureIds == null || option.GrantFeatureIds.Count == 0)
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
                    if (ContainsExactValue(candidate.FeatureIds, option.GrantFeatureIds[featureIndex]))
                    {
                        feat = candidate;
                        return true;
                    }
                }
            }

            return false;
        }

        private static string BuildToolChoiceOptionDescription(DndChoiceOptionData option)
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

            string featureDescription = GetFirstGrantedFeatureDescription(option);
            if (!string.IsNullOrWhiteSpace(featureDescription))
            {
                return featureDescription;
            }

            return !string.IsNullOrWhiteSpace(option.Description) ? option.Description.Trim() : string.Empty;
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

        private static string FirstNonEmpty(string first, string second)
        {
            return !string.IsNullOrWhiteSpace(first) ? first.Trim() : second?.Trim() ?? string.Empty;
        }

        private static bool ContainsExactValue(IReadOnlyList<string> values, string value)
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

        private static void AppendUniqueValues(List<string> target, IReadOnlyList<string> values)
        {
            if (target == null || values == null)
            {
                return;
            }

            for (int index = 0; index < values.Count; index++)
            {
                string value = values[index];
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                bool exists = false;
                for (int targetIndex = 0; targetIndex < target.Count; targetIndex++)
                {
                    if (string.Equals(target[targetIndex], value, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    target.Add(value.Trim());
                }
            }
        }
    }
}
