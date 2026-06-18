using System;
using System.Collections.Generic;

namespace GameLogic
{
    internal sealed class CharacterCreationRuleService
    {
        private static readonly Lazy<CharacterCreationRuleService> s_instance =
            new Lazy<CharacterCreationRuleService>(() => new CharacterCreationRuleService());

        private CharacterCreationRuleService()
        {
        }

        public static CharacterCreationRuleService Instance => s_instance.Value;

        public IReadOnlyList<DndClassDefineData> Classes => DndRuleContentService.Instance.Classes;

        public IReadOnlyList<DndRaceDefineData> Races => DndRuleContentService.Instance.Races;

        public IReadOnlyList<DndBackgroundDefineData> Backgrounds => DndRuleContentService.Instance.Backgrounds;

        public IReadOnlyList<DndAlignmentData> Alignments => DndRuleContentService.Instance.Alignments;

        public List<CharacterCreationOptionViewState> GetClassOptions()
        {
            List<CharacterCreationOptionViewState> result = new List<CharacterCreationOptionViewState>();
            IReadOnlyList<DndClassDefineData> classes = Classes;
            for (int index = 0; index < classes.Count; index++)
            {
                DndClassDefineData classData = classes[index];
                string id = classData?.ClassId ?? string.Empty;
                result.Add(new CharacterCreationOptionViewState
                {
                    Id = id,
                    Name = FirstNonEmpty(classData?.Name, id)
                });
            }

            return result;
        }

        public List<CharacterCreationOptionViewState> GetRaceOptions()
        {
            List<CharacterCreationOptionViewState> result = new List<CharacterCreationOptionViewState>();
            IReadOnlyList<DndRaceDefineData> races = Races;
            for (int index = 0; index < races.Count; index++)
            {
                DndRaceDefineData raceData = races[index];
                string id = raceData?.RaceId ?? string.Empty;
                result.Add(new CharacterCreationOptionViewState
                {
                    Id = id,
                    Name = FirstNonEmpty(raceData?.Name, id)
                });
            }

            return result;
        }

        public List<CharacterCreationOptionViewState> GetBackgroundOptions()
        {
            List<CharacterCreationOptionViewState> result = new List<CharacterCreationOptionViewState>();
            IReadOnlyList<DndBackgroundDefineData> backgrounds = Backgrounds;
            for (int index = 0; index < backgrounds.Count; index++)
            {
                DndBackgroundDefineData backgroundData = backgrounds[index];
                string id = backgroundData?.BackgroundId ?? string.Empty;
                result.Add(new CharacterCreationOptionViewState
                {
                    Id = id,
                    Name = FirstNonEmpty(backgroundData?.Name, id)
                });
            }

            return result;
        }

        public List<CharacterCreationOptionViewState> GetAlignmentOptions()
        {
            List<CharacterCreationOptionViewState> result = new List<CharacterCreationOptionViewState>();
            IReadOnlyList<DndAlignmentData> alignments = Alignments;
            for (int index = 0; index < alignments.Count; index++)
            {
                DndAlignmentData alignmentData = alignments[index];
                string id = alignmentData?.AlignmentId ?? string.Empty;
                result.Add(new CharacterCreationOptionViewState
                {
                    Id = id,
                    Name = FirstNonEmpty(alignmentData?.Name, id)
                });
            }

            return result;
        }

        public bool TryGetClass(string classId, out DndClassDefineData classData)
        {
            return DndRuleContentService.Instance.TryGetClass(classId, out classData);
        }

        public bool TryGetRace(string raceId, out DndRaceDefineData raceData)
        {
            raceData = null;
            if (string.IsNullOrWhiteSpace(raceId))
            {
                return false;
            }

            IReadOnlyList<DndRaceDefineData> races = Races;
            for (int index = 0; index < races.Count; index++)
            {
                DndRaceDefineData candidate = races[index];
                if (candidate != null && string.Equals(candidate.RaceId, raceId, StringComparison.OrdinalIgnoreCase))
                {
                    raceData = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetBackground(string backgroundId, out DndBackgroundDefineData backgroundData)
        {
            backgroundData = null;
            if (string.IsNullOrWhiteSpace(backgroundId))
            {
                return false;
            }

            IReadOnlyList<DndBackgroundDefineData> backgrounds = Backgrounds;
            for (int index = 0; index < backgrounds.Count; index++)
            {
                DndBackgroundDefineData candidate = backgrounds[index];
                if (candidate != null && string.Equals(candidate.BackgroundId, backgroundId, StringComparison.OrdinalIgnoreCase))
                {
                    backgroundData = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetAlignment(string alignmentId, out DndAlignmentData alignmentData)
        {
            alignmentData = null;
            if (string.IsNullOrWhiteSpace(alignmentId))
            {
                return false;
            }

            IReadOnlyList<DndAlignmentData> alignments = Alignments;
            for (int index = 0; index < alignments.Count; index++)
            {
                DndAlignmentData candidate = alignments[index];
                if (candidate != null && string.Equals(candidate.AlignmentId, alignmentId, StringComparison.OrdinalIgnoreCase))
                {
                    alignmentData = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetToolChoiceGroup(string choiceGroupId, out DndChoiceGroupData choiceGroup)
        {
            choiceGroup = null;
            if (string.IsNullOrWhiteSpace(choiceGroupId)
                || !DndRuleContentService.Instance.TryGetChoiceGroup(choiceGroupId.Trim(), out DndChoiceGroupData candidate)
                || !string.Equals(candidate.ChoiceType, "Tool", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            choiceGroup = candidate;
            return true;
        }

        public string GetClassDisplayName(string classId)
        {
            return TryGetClass(classId, out DndClassDefineData classData)
                ? FirstNonEmpty(classData.Name, classData.ClassId)
                : string.Empty;
        }

        public string GetRaceDisplayName(string raceId)
        {
            return TryGetRace(raceId, out DndRaceDefineData raceData)
                ? FirstNonEmpty(raceData.Name, raceData.RaceId)
                : string.Empty;
        }

        public string GetBackgroundDisplayName(string backgroundId)
        {
            return TryGetBackground(backgroundId, out DndBackgroundDefineData backgroundData)
                ? FirstNonEmpty(backgroundData.Name, backgroundData.BackgroundId)
                : string.Empty;
        }

        public string GetAlignmentDisplayName(string alignmentId)
        {
            return TryGetAlignment(alignmentId, out DndAlignmentData alignmentData)
                ? FirstNonEmpty(alignmentData.Name, alignmentData.AlignmentId)
                : string.Empty;
        }

        public List<string> BuildRaceChoiceGroupIds(DndRaceDefineData raceData)
        {
            List<string> choiceGroupIds = new List<string>();
            AppendUniqueValues(choiceGroupIds, raceData?.ChoiceGroupIds);
            if (raceData?.FeatureIds == null)
            {
                return choiceGroupIds;
            }

            for (int index = 0; index < raceData.FeatureIds.Count; index++)
            {
                string featureId = raceData.FeatureIds[index];
                if (!string.IsNullOrWhiteSpace(featureId)
                    && DndRuleContentService.Instance.TryGetFeature(featureId, out DndFeatureDefineData feature))
                {
                    AppendUniqueValues(choiceGroupIds, feature.ChoiceGroupIds);
                }
            }

            return choiceGroupIds;
        }

        public void ApplyRaceFeatureEffects(IReadOnlyList<string> featureIds, Action<DndFeatureEffectData> action)
        {
            if (featureIds == null || action == null)
            {
                return;
            }

            for (int index = 0; index < featureIds.Count; index++)
            {
                string featureId = featureIds[index];
                if (string.IsNullOrWhiteSpace(featureId)
                    || !DndRuleContentService.Instance.TryGetFeature(featureId, out DndFeatureDefineData feature))
                {
                    continue;
                }

                ApplyEffects(feature.EffectIds, action);
            }
        }

        public void ApplyEffects(IReadOnlyList<string> effectIds, Action<DndFeatureEffectData> action)
        {
            if (effectIds == null || action == null)
            {
                return;
            }

            for (int index = 0; index < effectIds.Count; index++)
            {
                string effectId = effectIds[index];
                if (!string.IsNullOrWhiteSpace(effectId)
                    && DndRuleContentService.Instance.TryGetFeatureEffect(effectId, out DndFeatureEffectData effect))
                {
                    action(effect);
                }
            }
        }

        public List<string> BuildRaceSkillProficiencyIds(DndRaceDefineData raceData)
        {
            List<string> labels = new List<string>();
            ApplyRaceFeatureEffects(raceData?.FeatureIds, effect =>
            {
                if (effect != null && string.Equals(effect.EffectType, "SkillProficiency", StringComparison.OrdinalIgnoreCase))
                {
                    AppendDelimitedValues(labels, effect.Target);
                }
            });

            return labels;
        }

        public List<string> BuildFixedSkillProficiencyIds(DndRaceDefineData raceData, DndBackgroundDefineData backgroundData)
        {
            List<string> result = new List<string>();
            AppendUniqueValues(result, BuildRaceSkillProficiencyIds(raceData));
            AppendUniqueValues(result, backgroundData?.SkillProficiencies);
            return result;
        }

        public List<string> BuildFixedToolProficiencyIds(DndClassDefineData classData, DndBackgroundDefineData backgroundData)
        {
            List<string> result = new List<string>();
            AppendFixedToolProficiencyIds(result, classData?.ToolProficiencies);
            AppendFixedToolProficiencyIds(result, backgroundData?.ToolProficiencies);
            return result;
        }

        public string GetToolChoiceSourceType(string choiceGroupId, DndRaceDefineData raceData, DndBackgroundDefineData backgroundData)
        {
            if (!string.IsNullOrWhiteSpace(choiceGroupId)
                && raceData != null
                && ContainsExactValue(BuildRaceChoiceGroupIds(raceData), choiceGroupId))
            {
                return "Race";
            }

            if (!string.IsNullOrWhiteSpace(choiceGroupId)
                && backgroundData != null
                && ContainsExactValue(backgroundData.ToolProficiencies, choiceGroupId))
            {
                return "Background";
            }

            return "Class";
        }

        public string GetToolChoiceSourceId(string choiceGroupId, string selectedClassId, DndRaceDefineData raceData, DndBackgroundDefineData backgroundData)
        {
            string sourceType = GetToolChoiceSourceType(choiceGroupId, raceData, backgroundData);
            if (string.Equals(sourceType, "Race", StringComparison.OrdinalIgnoreCase))
            {
                return raceData?.RaceId ?? string.Empty;
            }

            if (string.Equals(sourceType, "Background", StringComparison.OrdinalIgnoreCase))
            {
                return backgroundData?.BackgroundId ?? string.Empty;
            }

            return selectedClassId?.Trim() ?? string.Empty;
        }

        public string ResolveToolIdFromChoiceOption(DndChoiceOptionData option)
        {
            if (option == null)
            {
                return string.Empty;
            }

            string toolId = NormalizeToolId(option.OptionId);
            if (!string.IsNullOrWhiteSpace(toolId))
            {
                return toolId;
            }

            toolId = NormalizeToolId(option.Name);
            if (!string.IsNullOrWhiteSpace(toolId))
            {
                return toolId;
            }

            for (int index = 0; index < option.GrantFeatureIds.Count; index++)
            {
                if (DndRuleContentService.Instance.TryGetFeature(option.GrantFeatureIds[index], out DndFeatureDefineData feature))
                {
                    toolId = ResolveToolIdFromEffects(feature.EffectIds);
                    if (!string.IsNullOrWhiteSpace(toolId))
                    {
                        return toolId;
                    }
                }
            }

            return ResolveToolIdFromEffects(option.GrantEffectIds);
        }

        public void ConfigureRaceAbilityChoice(DndRaceDefineData raceData)
        {
            Dictionary<string, int> fixedAbilityBonuses = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, string> optionIdByAbility = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string abilityChoiceGroupId = string.Empty;
            string abilityChoiceSelectionMode = string.Empty;
            int abilityChoiceMaxSelect = 0;

            ApplyRaceFeatureEffects(raceData?.FeatureIds, effect =>
            {
                if (effect == null
                    || !string.Equals(effect.EffectType, "AbilityScoreBonus", StringComparison.OrdinalIgnoreCase)
                    || !int.TryParse(effect.Value, out int value))
                {
                    return;
                }

                AddAbilityBonus(fixedAbilityBonuses, effect.Target, value);
            });

            List<string> choiceGroupIds = BuildRaceChoiceGroupIds(raceData);
            for (int index = 0; index < choiceGroupIds.Count; index++)
            {
                string choiceGroupId = choiceGroupIds[index];
                if (string.IsNullOrWhiteSpace(choiceGroupId)
                    || !DndRuleContentService.Instance.TryGetChoiceGroup(choiceGroupId, out DndChoiceGroupData choiceGroup)
                    || !string.Equals(choiceGroup.ChoiceType, "AbilityScore", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                abilityChoiceGroupId = choiceGroup.ChoiceGroupId;
                abilityChoiceSelectionMode = choiceGroup.SelectionMode;
                abilityChoiceMaxSelect = Math.Max(choiceGroup.MinSelect, choiceGroup.MaxSelect);
                IReadOnlyList<DndChoiceOptionData> options = DndRuleContentService.Instance.GetChoiceOptions(choiceGroup.ChoiceGroupId);
                for (int optionIndex = 0; optionIndex < options.Count; optionIndex++)
                {
                    DndChoiceOptionData option = options[optionIndex];
                    string abilityId = ResolveAbilityIdFromChoiceOption(option);
                    if (!string.IsNullOrWhiteSpace(abilityId) && !optionIdByAbility.ContainsKey(abilityId))
                    {
                        optionIdByAbility[abilityId] = option.OptionId;
                    }
                }

                break;
            }

            CharacterCreationSessionService.Instance.ConfigureRaceAbilityChoice(
                raceData?.RaceId ?? string.Empty,
                fixedAbilityBonuses,
                abilityChoiceGroupId,
                abilityChoiceSelectionMode,
                abilityChoiceMaxSelect,
                optionIdByAbility);
        }

        private static string FirstNonEmpty(string first, string second)
        {
            return !string.IsNullOrWhiteSpace(first) ? first.Trim() : second?.Trim() ?? string.Empty;
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

        private static void AppendFixedToolProficiencyIds(List<string> result, IReadOnlyList<string> values)
        {
            if (result == null || values == null)
            {
                return;
            }

            for (int index = 0; index < values.Count; index++)
            {
                string toolId = NormalizeToolId(values[index]);
                if (!string.IsNullOrWhiteSpace(toolId))
                {
                    AppendUniqueValues(result, new[] { toolId });
                }
            }
        }

        private static bool ContainsExactValue(IReadOnlyList<string> values, string id)
        {
            if (values == null || string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            string normalized = id.Trim();
            for (int index = 0; index < values.Count; index++)
            {
                if (string.Equals(values[index]?.Trim(), normalized, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string ResolveAbilityIdFromChoiceOption(DndChoiceOptionData option)
        {
            if (option == null)
            {
                return string.Empty;
            }

            string abilityId = NormalizeAbilityId(option.Name);
            if (!string.IsNullOrWhiteSpace(abilityId))
            {
                return abilityId;
            }

            abilityId = NormalizeAbilityId(option.OptionId);
            if (!string.IsNullOrWhiteSpace(abilityId))
            {
                return abilityId;
            }

            for (int index = 0; index < option.GrantFeatureIds.Count; index++)
            {
                if (DndRuleContentService.Instance.TryGetFeature(option.GrantFeatureIds[index], out DndFeatureDefineData feature))
                {
                    abilityId = ResolveAbilityIdFromEffects(feature.EffectIds);
                    if (!string.IsNullOrWhiteSpace(abilityId))
                    {
                        return abilityId;
                    }
                }
            }

            return ResolveAbilityIdFromEffects(option.GrantEffectIds);
        }

        private static string ResolveAbilityIdFromEffects(IReadOnlyList<string> effectIds)
        {
            if (effectIds == null)
            {
                return string.Empty;
            }

            for (int index = 0; index < effectIds.Count; index++)
            {
                if (DndRuleContentService.Instance.TryGetFeatureEffect(effectIds[index], out DndFeatureEffectData effect)
                    && string.Equals(effect.EffectType, "AbilityScoreBonus", StringComparison.OrdinalIgnoreCase))
                {
                    string abilityId = NormalizeAbilityId(effect.Target);
                    if (!string.IsNullOrWhiteSpace(abilityId))
                    {
                        return abilityId;
                    }
                }
            }

            return string.Empty;
        }

        private static string ResolveToolIdFromEffects(IReadOnlyList<string> effectIds)
        {
            if (effectIds == null)
            {
                return string.Empty;
            }

            for (int index = 0; index < effectIds.Count; index++)
            {
                if (DndRuleContentService.Instance.TryGetFeatureEffect(effectIds[index], out DndFeatureEffectData effect)
                    && string.Equals(effect.EffectType, "ToolProficiency", StringComparison.OrdinalIgnoreCase))
                {
                    string toolId = NormalizeToolId(effect.Target);
                    if (!string.IsNullOrWhiteSpace(toolId))
                    {
                        return toolId;
                    }
                }
            }

            return string.Empty;
        }

        private static void AddAbilityBonus(Dictionary<string, int> bonuses, string target, int value)
        {
            if (bonuses == null || value == 0)
            {
                return;
            }

            string normalized = NormalizeAbilityId(target);
            if (string.Equals(normalized, "All", StringComparison.OrdinalIgnoreCase))
            {
                AddDictionaryValue(bonuses, "Strength", value);
                AddDictionaryValue(bonuses, "Dexterity", value);
                AddDictionaryValue(bonuses, "Constitution", value);
                AddDictionaryValue(bonuses, "Intelligence", value);
                AddDictionaryValue(bonuses, "Wisdom", value);
                AddDictionaryValue(bonuses, "Charisma", value);
                return;
            }

            if (!string.IsNullOrWhiteSpace(normalized))
            {
                AddDictionaryValue(bonuses, normalized, value);
            }
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
                case "力量":
                    return "Strength";
                case "dex":
                case "dexterity":
                case "敏捷":
                    return "Dexterity";
                case "con":
                case "constitution":
                case "体质":
                    return "Constitution";
                case "int":
                case "intelligence":
                case "智力":
                    return "Intelligence";
                case "wis":
                case "wisdom":
                case "感知":
                    return "Wisdom";
                case "cha":
                case "charisma":
                case "魅力":
                    return "Charisma";
                case "all":
                case "全部":
                    return "All";
                default:
                    return normalized;
            }
        }

        private static int GetDictionaryValue(Dictionary<string, int> values, string key)
        {
            return values != null && !string.IsNullOrWhiteSpace(key) && values.TryGetValue(key, out int value) ? value : 0;
        }

        private static void AddDictionaryValue(Dictionary<string, int> values, string key, int value)
        {
            if (values == null || string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            values[key] = GetDictionaryValue(values, key) + value;
        }

        private static string NormalizeToolId(string value)
        {
            string normalized = value?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return string.Empty;
            }

            if (DndRuleContentService.Instance.TryGetTool(normalized, out DndToolDefineData directTool))
            {
                return directTool.ToolId;
            }

            IReadOnlyList<DndToolDefineData> tools = DndRuleContentService.Instance.Tools;
            for (int index = 0; index < tools.Count; index++)
            {
                DndToolDefineData tool = tools[index];
                if (tool == null)
                {
                    continue;
                }

                if (string.Equals(normalized, tool.ToolId, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(normalized, tool.Name, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(normalized, tool.EnglishName, StringComparison.OrdinalIgnoreCase))
                {
                    return tool.ToolId;
                }
            }

            return string.Empty;
        }
    }
}
