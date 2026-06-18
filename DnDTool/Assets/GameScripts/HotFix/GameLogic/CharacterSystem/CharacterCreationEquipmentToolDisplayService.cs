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
            CharacterCreationEquipmentToolDisplayState state = new CharacterCreationEquipmentToolDisplayState();

            if (classData != null)
            {
                AppendPrefixedLabels(state.Labels, "护甲", classData.ArmorProficiencies);
                AppendPrefixedLabels(state.Labels, "武器", classData.WeaponProficiencies);
                AppendToolProficiencyLabels(state, classData.ToolProficiencies);
            }

            if (raceData != null)
            {
                AppendRaceEquipmentToolLabels(state.Labels, raceData);
                AppendRaceToolChoiceLabels(state, raceData);
            }

            AppendToolProficiencyLabels(state, backgroundData?.ToolProficiencies);
            AppendCurrentToolChoiceLabels(state.Labels);
            return state;
        }

        public string GetToolDisplayName(string toolId)
        {
            if (string.IsNullOrWhiteSpace(toolId))
            {
                return string.Empty;
            }

            if (DndRuleContentService.Instance.TryGetTool(toolId, out DndToolDefineData tool))
            {
                return FirstNonEmpty(tool.Name, tool.ToolId);
            }

            return toolId.Trim();
        }

        public string BuildToolChoiceLabel(DndChoiceGroupData choiceGroup)
        {
            if (choiceGroup == null)
            {
                return string.Empty;
            }

            return $"工具：{FirstNonEmpty(choiceGroup.Name, choiceGroup.ChoiceGroupId)}";
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
                    labels.Add($"工具：{FirstNonEmpty(tool.Name, tool.ToolId)}");
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
                    AppendDelimitedEffectTargets(labels, "护甲", effect.Target);
                }
                else if (string.Equals(effect.EffectType, "WeaponProficiency", StringComparison.OrdinalIgnoreCase))
                {
                    AppendDelimitedEffectTargets(labels, "武器", effect.Target);
                }
                else if (string.Equals(effect.EffectType, "ToolProficiency", StringComparison.OrdinalIgnoreCase))
                {
                    AppendDelimitedEffectTargets(labels, "工具", effect.Target);
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

                state.Labels.Add(label);
                state.ChoiceGroupIdByLabel[label] = choiceGroup.ChoiceGroupId;
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
                        labels.Add($"工具：{GetToolDisplayName(toolId)}");
                    }
                }
            }
        }

        private static void AppendPrefixedLabels(List<string> labels, string prefix, IReadOnlyList<string> values)
        {
            if (labels == null || values == null || values.Count == 0)
            {
                return;
            }

            for (int index = 0; index < values.Count; index++)
            {
                string value = values[index];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    labels.Add($"{prefix}：{value.Trim()}");
                }
            }
        }

        private static void AppendDelimitedEffectTargets(List<string> labels, string prefix, string value)
        {
            int startCount = labels?.Count ?? 0;
            AppendDelimitedValues(labels, value);
            if (labels == null)
            {
                return;
            }

            for (int index = startCount; index < labels.Count; index++)
            {
                labels[index] = $"{prefix}：{labels[index]}";
            }
        }

        private static void AppendDelimitedValues(List<string> labels, string value)
        {
            if (labels == null || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            string[] parts = value.Split(new[] { ';', '；', ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
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
