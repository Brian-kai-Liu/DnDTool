using System;
using System.Collections.Generic;

namespace GameLogic
{
    internal sealed class CharacterCreationEquipmentToolDisplayService
    {
        private static readonly Lazy<CharacterCreationEquipmentToolDisplayService> s_instance =
            new Lazy<CharacterCreationEquipmentToolDisplayService>(() => new CharacterCreationEquipmentToolDisplayService());

        private CharacterCreationEquipmentToolDisplayService()
        {
        }

        public static CharacterCreationEquipmentToolDisplayService Instance => s_instance.Value;

        public CharacterCreationEquipmentToolDisplayState BuildDisplayState(
            DndClassDefineData classData,
            DndRaceDefineData raceData,
            DndBackgroundDefineData backgroundData)
        {
            return BuildDisplayState(classData, raceData, backgroundData, null);
        }

        public CharacterCreationEquipmentToolDisplayState BuildDisplayState(
            DndClassDefineData classData,
            DndRaceDefineData raceData,
            DndBackgroundDefineData backgroundData,
            CharacterRuntimeSnapshotData snapshot)
        {
            CharacterCreationEquipmentToolDisplayState state = new CharacterCreationEquipmentToolDisplayState();

            if (snapshot != null)
            {
                AppendDisplayLabels(state.Labels, snapshot.ArmorProficiencyIds, CharacterEquipmentProficiencyDisplayService.Instance.FormatArmorLabel);
                AppendDisplayLabels(state.Labels, snapshot.WeaponProficiencyIds, CharacterEquipmentProficiencyDisplayService.Instance.FormatWeaponLabel);
                AppendDisplayLabels(state.Labels, snapshot.ToolProficiencyIds, CharacterEquipmentProficiencyDisplayService.Instance.FormatToolLabel);
            }
            else if (classData != null)
            {
                AppendDisplayLabels(state.Labels, classData.ArmorProficiencies, CharacterEquipmentProficiencyDisplayService.Instance.FormatArmorLabel);
                AppendDisplayLabels(state.Labels, classData.WeaponProficiencies, CharacterEquipmentProficiencyDisplayService.Instance.FormatWeaponLabel);
                AppendToolProficiencyLabels(state, classData.ToolProficiencies);
            }

            if (snapshot == null && raceData != null)
            {
                AppendRaceEquipmentToolLabels(state.Labels, raceData);
                AppendRaceToolChoiceLabels(state, raceData);
            }
            else if (raceData != null)
            {
                AppendRaceToolChoiceLabels(state, raceData);
            }

            if (snapshot == null)
            {
                AppendToolProficiencyLabels(state, backgroundData?.ToolProficiencies);
                AppendCurrentToolChoiceLabels(state.Labels);
            }
            else
            {
                AppendPendingToolChoiceLabels(state, classData?.ToolProficiencies);
                AppendPendingToolChoiceLabels(state, backgroundData?.ToolProficiencies);
            }

            AppendFeatToolChoiceLabels(state);
            return state;
        }

        public string GetToolDisplayName(string toolId)
        {
            return CharacterEquipmentProficiencyDisplayService.Instance.GetToolDisplayName(toolId);
        }

        public string BuildToolChoiceLabel(DndChoiceGroupData choiceGroup)
        {
            if (choiceGroup == null)
            {
                return string.Empty;
            }

            return $"\u5DE5\u5177\uFF1A{FirstNonEmpty(choiceGroup.Name, choiceGroup.ChoiceGroupId)}";
        }

        private void AppendToolProficiencyLabels(CharacterCreationEquipmentToolDisplayState state, IReadOnlyList<string> toolProficiencies)
        {
            if (state == null)
            {
                return;
            }

            AppendToolProficiencyLabels(state.Labels, toolProficiencies, state.ChoiceGroupIdByLabel);
        }

        private void AppendToolProficiencyLabels(
            List<string> labels,
            IReadOnlyList<string> toolProficiencies,
            Dictionary<string, string> choiceGroupIdByLabel)
        {
            if (labels == null || toolProficiencies == null)
            {
                return;
            }

            for (int index = 0; index < toolProficiencies.Count; index++)
            {
                string value = toolProficiencies[index]?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                if (DndRuleContentService.Instance.TryGetChoiceGroup(value, out DndChoiceGroupData choiceGroup)
                    && string.Equals(choiceGroup.ChoiceType, "Tool", StringComparison.OrdinalIgnoreCase))
                {
                    string label = BuildToolChoiceLabel(choiceGroup);
                    CharacterCreationToolChoiceState state = CharacterCreationSessionService.Instance.FindToolChoiceState(choiceGroup.ChoiceGroupId);
                    if (state != null && state.IsCompleted)
                    {
                        continue;
                    }

                    labels.Add(label);
                    if (choiceGroupIdByLabel != null)
                    {
                        choiceGroupIdByLabel[label] = choiceGroup.ChoiceGroupId;
                    }

                    continue;
                }

                if (DndRuleContentService.Instance.TryGetTool(value, out DndToolDefineData tool))
                {
                    labels.Add(CharacterEquipmentProficiencyDisplayService.Instance.FormatToolLabel(tool.ToolId));
                }
            }
        }

        private void AppendRaceEquipmentToolLabels(List<string> labels, DndRaceDefineData raceData)
        {
            CharacterCreationRuleService.Instance.ApplyRaceFeatureEffects(raceData?.FeatureIds, effect =>
            {
                if (effect == null)
                {
                    return;
                }

                if (string.Equals(effect.EffectType, "ArmorProficiency", StringComparison.OrdinalIgnoreCase))
                {
                    AppendDelimitedEffectTargets(labels, effect.Target, CharacterEquipmentProficiencyDisplayService.Instance.FormatArmorLabel);
                }
                else if (string.Equals(effect.EffectType, "WeaponProficiency", StringComparison.OrdinalIgnoreCase))
                {
                    AppendDelimitedEffectTargets(labels, effect.Target, CharacterEquipmentProficiencyDisplayService.Instance.FormatWeaponLabel);
                }
                else if (string.Equals(effect.EffectType, "ToolProficiency", StringComparison.OrdinalIgnoreCase))
                {
                    AppendDelimitedEffectTargets(labels, effect.Target, CharacterEquipmentProficiencyDisplayService.Instance.FormatToolLabel);
                }
            });
        }

        private void AppendRaceToolChoiceLabels(CharacterCreationEquipmentToolDisplayState state, DndRaceDefineData raceData)
        {
            if (state == null || raceData == null)
            {
                return;
            }

            List<string> choiceGroupIds = CharacterCreationRuleService.Instance.BuildRaceChoiceGroupIds(raceData);
            for (int index = 0; index < choiceGroupIds.Count; index++)
            {
                string choiceGroupId = choiceGroupIds[index];
                if (string.IsNullOrWhiteSpace(choiceGroupId)
                    || !DndRuleContentService.Instance.TryGetChoiceGroup(choiceGroupId, out DndChoiceGroupData choiceGroup)
                    || !string.Equals(choiceGroup.ChoiceType, "Tool", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string label = BuildToolChoiceLabel(choiceGroup);
                CharacterCreationToolChoiceState choiceState = CharacterCreationSessionService.Instance.FindToolChoiceState(choiceGroup.ChoiceGroupId);
                if (choiceState != null && choiceState.IsCompleted)
                {
                    continue;
                }

                AddChoiceLabel(state, label, choiceGroup.ChoiceGroupId, "Race", raceData.RaceId);
            }
        }

        private void AppendPendingToolChoiceLabels(CharacterCreationEquipmentToolDisplayState state, IReadOnlyList<string> toolProficiencies)
        {
            if (state == null || toolProficiencies == null)
            {
                return;
            }

            for (int index = 0; index < toolProficiencies.Count; index++)
            {
                string value = toolProficiencies[index]?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(value)
                    || !DndRuleContentService.Instance.TryGetChoiceGroup(value, out DndChoiceGroupData choiceGroup)
                    || !string.Equals(choiceGroup.ChoiceType, "Tool", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                CharacterCreationToolChoiceState choiceState = CharacterCreationSessionService.Instance.FindToolChoiceState(choiceGroup.ChoiceGroupId);
                if (choiceState != null && choiceState.IsCompleted)
                {
                    continue;
                }

                string label = BuildToolChoiceLabel(choiceGroup);
                AddChoiceLabel(state, label, choiceGroup.ChoiceGroupId, string.Empty, string.Empty);
            }
        }

        private void AppendFeatToolChoiceLabels(CharacterCreationEquipmentToolDisplayState state)
        {
            if (state == null)
            {
                return;
            }

            List<CharacterCreationFeatureChoiceState> featureStates = CharacterCreationSessionService.Instance.FeatureChoiceStates;
            for (int stateIndex = 0; stateIndex < featureStates.Count; stateIndex++)
            {
                CharacterCreationFeatureChoiceState featureState = featureStates[stateIndex];
                if (!TryGetSelectedFeat(featureState, out DndFeatDefineData feat))
                {
                    continue;
                }

                List<string> choiceGroupIds = BuildFeatChoiceGroupIds(feat);
                for (int groupIndex = 0; groupIndex < choiceGroupIds.Count; groupIndex++)
                {
                    string choiceGroupId = choiceGroupIds[groupIndex];
                    if (string.IsNullOrWhiteSpace(choiceGroupId)
                        || !DndRuleContentService.Instance.TryGetChoiceGroup(choiceGroupId, out DndChoiceGroupData choiceGroup)
                        || (!string.Equals(choiceGroup.ChoiceType, "Tool", StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(choiceGroup.ChoiceType, "SkillOrTool", StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    CharacterCreationToolChoiceState choiceState = CharacterCreationSessionService.Instance.FindToolChoiceState(choiceGroup.ChoiceGroupId);
                    CharacterCreationMixedProficiencyChoiceState mixedState = CharacterCreationSessionService.Instance.FindMixedProficiencyChoiceState(choiceGroup.ChoiceGroupId);
                    bool isCompleted = string.Equals(choiceGroup.ChoiceType, "SkillOrTool", StringComparison.OrdinalIgnoreCase)
                        ? mixedState != null && mixedState.IsCompleted
                        : choiceState != null && choiceState.IsCompleted;
                    if (isCompleted)
                    {
                        continue;
                    }

                    string label = BuildToolChoiceLabel(choiceGroup);
                    AddChoiceLabel(state, label, choiceGroup.ChoiceGroupId, "Feat", feat.FeatId);
                }
            }
        }

        private void AppendCurrentToolChoiceLabels(List<string> labels)
        {
            if (labels == null)
            {
                return;
            }

            List<CharacterCreationToolChoiceState> states = CharacterCreationSessionService.Instance.ToolChoiceStates;
            for (int stateIndex = 0; stateIndex < states.Count; stateIndex++)
            {
                CharacterCreationToolChoiceState state = states[stateIndex];
                if (state == null)
                {
                    continue;
                }

                for (int toolIndex = 0; toolIndex < state.SelectedToolIds.Count; toolIndex++)
                {
                    string toolId = state.SelectedToolIds[toolIndex];
                    if (!string.IsNullOrWhiteSpace(toolId))
                    {
                        labels.Add($"\u5DE5\u5177\uFF1A{GetToolDisplayName(toolId)}");
                    }
                }
            }
        }

        private static void AppendDisplayLabels(List<string> labels, IReadOnlyList<string> values, Func<string, string> formatLabel)
        {
            if (labels == null || values == null || values.Count == 0 || formatLabel == null)
            {
                return;
            }

            for (int index = 0; index < values.Count; index++)
            {
                string label = formatLabel(values[index]);
                if (!string.IsNullOrWhiteSpace(label))
                {
                    labels.Add(label);
                }
            }
        }

        private static void AppendDelimitedEffectTargets(List<string> labels, string value, Func<string, string> formatLabel)
        {
            if (labels == null || formatLabel == null)
            {
                return;
            }

            List<string> values = new List<string>();
            AppendDelimitedValues(values, value);
            for (int index = 0; index < values.Count; index++)
            {
                string label = formatLabel(values[index]);
                if (!string.IsNullOrWhiteSpace(label))
                {
                    labels.Add(label);
                }
            }
        }

        private static void AddChoiceLabel(
            CharacterCreationEquipmentToolDisplayState state,
            string label,
            string choiceGroupId,
            string sourceType,
            string sourceId)
        {
            if (state == null || string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(choiceGroupId))
            {
                return;
            }

            if (HasChoiceGroupLabel(state, choiceGroupId))
            {
                return;
            }

            state.Labels.Add(label);
            AddChoiceLabelMapping(state, label, choiceGroupId, sourceType, sourceId);
        }

        private static void AddChoiceLabelMapping(
            CharacterCreationEquipmentToolDisplayState state,
            string label,
            string choiceGroupId,
            string sourceType,
            string sourceId)
        {
            if (state == null || string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(choiceGroupId))
            {
                return;
            }

            state.ChoiceGroupIdByLabel[label] = choiceGroupId;
            state.ChoiceSourceTypeByLabel[label] = sourceType ?? string.Empty;
            state.ChoiceSourceIdByLabel[label] = sourceId ?? string.Empty;
        }

        private static bool HasChoiceGroupLabel(CharacterCreationEquipmentToolDisplayState state, string choiceGroupId)
        {
            if (state == null || string.IsNullOrWhiteSpace(choiceGroupId))
            {
                return false;
            }

            foreach (string existingChoiceGroupId in state.ChoiceGroupIdByLabel.Values)
            {
                if (string.Equals(existingChoiceGroupId?.Trim(), choiceGroupId.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }


        private static bool TryGetSelectedFeat(CharacterCreationFeatureChoiceState state, out DndFeatDefineData feat)
        {
            feat = null;
            if (state == null)
            {
                return false;
            }

            if (string.Equals(state.SourceType, "Feat", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(state.SourceId)
                && DndRuleContentService.Instance.TryGetFeat(state.SourceId.Trim(), out feat))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(state.ChoiceGroupId)
                || !DndRuleContentService.Instance.TryGetChoiceGroup(state.ChoiceGroupId, out DndChoiceGroupData choiceGroup)
                || !IsFeatChoiceGroup(choiceGroup))
            {
                return false;
            }

            IReadOnlyList<string> optionIds = state.IsConfirmed && state.SelectedOptionIds.Count > 0
                ? state.SelectedOptionIds
                : state.PendingOptionIds;
            for (int optionIndex = 0; optionIndex < optionIds.Count; optionIndex++)
            {
                string optionId = optionIds[optionIndex]?.Trim() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(optionId)
                    && DndRuleContentService.Instance.TryGetFeat(optionId, out feat))
                {
                    return true;
                }

                if (TryResolveFeatFromOption(state.ChoiceGroupId, optionId, out feat))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryResolveFeatFromOption(string choiceGroupId, string optionId, out DndFeatDefineData feat)
        {
            feat = null;
            if (string.IsNullOrWhiteSpace(choiceGroupId) || string.IsNullOrWhiteSpace(optionId))
            {
                return false;
            }

            IReadOnlyList<DndChoiceOptionData> options = DndRuleContentService.Instance.GetChoiceOptions(choiceGroupId.Trim());
            for (int optionIndex = 0; optionIndex < options.Count; optionIndex++)
            {
                DndChoiceOptionData option = options[optionIndex];
                if (option == null || !string.Equals(option.OptionId, optionId.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                for (int featureIndex = 0; featureIndex < option.GrantFeatureIds.Count; featureIndex++)
                {
                    string featureId = option.GrantFeatureIds[featureIndex];
                    IReadOnlyList<DndFeatDefineData> feats = DndRuleContentService.Instance.Feats;
                    for (int featIndex = 0; featIndex < feats.Count; featIndex++)
                    {
                        DndFeatDefineData candidate = feats[featIndex];
                        if (candidate?.FeatureIds != null
                            && ContainsExactValue(candidate.FeatureIds, featureId))
                        {
                            feat = candidate;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool IsFeatChoiceGroup(DndChoiceGroupData choiceGroup)
        {
            return choiceGroup != null
                && (string.Equals(choiceGroup.ChoiceType, "Feat", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(choiceGroup.ChoiceGroupId, "choice_feat", StringComparison.OrdinalIgnoreCase));
        }

        private static List<string> BuildFeatChoiceGroupIds(DndFeatDefineData feat)
        {
            List<string> choiceGroupIds = new List<string>();
            if (feat == null)
            {
                return choiceGroupIds;
            }

            AppendUniqueExactValues(choiceGroupIds, feat.ChoiceGroupIds);
            if (feat.FeatureIds == null)
            {
                return choiceGroupIds;
            }

            for (int index = 0; index < feat.FeatureIds.Count; index++)
            {
                string featureId = feat.FeatureIds[index];
                if (!string.IsNullOrWhiteSpace(featureId)
                    && DndRuleContentService.Instance.TryGetFeature(featureId, out DndFeatureDefineData feature))
                {
                    AppendUniqueExactValues(choiceGroupIds, feature.ChoiceGroupIds);
                }
            }

            return choiceGroupIds;
        }

        private static void AppendUniqueExactValues(List<string> target, IReadOnlyList<string> values)
        {
            if (target == null || values == null)
            {
                return;
            }

            for (int index = 0; index < values.Count; index++)
            {
                string value = values[index]?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(value) || target.Contains(value))
                {
                    continue;
                }

                target.Add(value);
            }
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

        private static void AppendDelimitedValues(List<string> labels, string value)
        {
            if (labels == null || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            string[] parts = value.Split(new[] { ';', '\uFF1B', ',', '\uFF0C' }, StringSplitOptions.RemoveEmptyEntries);
            for (int index = 0; index < parts.Length; index++)
            {
                string part = parts[index].Trim();
                if (!string.IsNullOrWhiteSpace(part))
                {
                    labels.Add(part);
                }
            }
        }

        private static string FirstNonEmpty(string first, string second)
        {
            return !string.IsNullOrWhiteSpace(first) ? first.Trim() : second?.Trim() ?? string.Empty;
        }
    }
}
