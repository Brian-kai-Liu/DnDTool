using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace GameLogic
{
    internal interface IDndRuleContentSource
    {
        bool TryLoad(out DndRuleContentLibraryData library, out string errorMessage);
    }

    internal sealed class LubanDndRuleContentSource : IDndRuleContentSource
    {
        public bool TryLoad(out DndRuleContentLibraryData library, out string errorMessage)
        {
            library = new DndRuleContentLibraryData();
            errorMessage = string.Empty;

            try
            {
                if (!TryGetConfigTables(out object tables, out errorMessage))
                {
                    return false;
                }

                LoadRows(tables, "TbRulePackage", library.RulePackages, CreateRulePackage);
                LoadRows(tables, "TbClassDefine", library.Classes, CreateClassDefine);
                LoadRows(tables, "TbLevelProgression", library.LevelProgressions, CreateLevelProgression);
                if (library.LevelProgressions.Count == 0)
                {
                    LoadRows(tables, "TbClassLevelProgression", library.LevelProgressions, CreateLevelProgression);
                }
                LoadRows(tables, "TbSubclassLevelProgression", library.SubclassLevelProgressions, CreateSubclassLevelProgression);
                LoadRows(tables, "TbFeatureDefine", library.Features, CreateFeatureDefine);
                LoadRows(tables, "TbFeatureEffect", library.FeatureEffects, CreateFeatureEffect);
                LoadRows(tables, "TbFeatureEffectCondition", library.FeatureEffectConditions, CreateFeatureEffectCondition);
                LoadRows(tables, "TbItemEffect", library.ItemEffects, CreateItemEffect);
                LoadRows(tables, "TbChoiceGroup", library.ChoiceGroups, CreateChoiceGroup);
                LoadRows(tables, "TbChoiceOption", library.ChoiceOptions, CreateChoiceOption);
                LoadRows(tables, "TbSkillDefine", library.Skills, CreateSkillDefine);
                LoadRows(tables, "TbRaceMainDefine", library.RaceMains, CreateRaceMainDefine);
                LoadRows(tables, "TbRaceSubDefine", library.RaceSubs, CreateRaceSubDefine);
                LoadRows(tables, "TbBackgroundDefine", library.Backgrounds, CreateBackgroundDefine);
                LoadRows(tables, "TbFeatDefine", library.Feats, CreateFeatDefine);
                LoadRows(tables, "TbSpellDefine", library.Spells, CreateSpellDefine);
                LoadRows(tables, "TbClassSpellList", library.ClassSpellLists, CreateClassSpellList);
                LoadRows(tables, "TbSpellSlotProgression", library.SpellSlotProgressions, CreateSpellSlotProgression);
                LoadRows(tables, "TbLevelExperience", library.LevelExperiences, CreateLevelExperience);
                LoadRows(tables, "TbDndItemTypeDefine", library.ItemTypes, CreateDndItemTypeDefine);
                LoadRows(tables, "TbDndItemDefine", library.Items, CreateDndItemDefine);
                LoadRows(tables, "TbDndToolDefine", library.Tools, CreateDndToolDefine);
                LoadRows(tables, "TbDndLanguageDefine", library.Languages, CreateDndLanguageDefine);
                LoadRows(tables, "TbEnumList", library.EnumLists, CreateEnumList);
                LoadRows(tables, "TbAlignment", library.Alignments, CreateAlignment);
                LoadRows(tables, "TbTextLocalize", library.TextLocalizations, CreateTextLocalize);

                ApplyTextLocalizations(library, "zh_CN");

                if (library.RaceMains.Count > 0 || library.RaceSubs.Count > 0)
                {
                    NormalizeRaceData(library);
                }
            }
            catch (Exception exception)
            {
                errorMessage = $"Failed to load Luban DnD rule content: {exception.Message}";
                library = new DndRuleContentLibraryData();
                return false;
            }

            return library.RulePackages.Count > 0
                || library.Classes.Count > 0
                || library.Features.Count > 0
                || library.Spells.Count > 0;
        }

        private static bool TryGetConfigTables(out object tables, out string errorMessage)
        {
            tables = null;
            errorMessage = string.Empty;

            Type configSystemType = FindType("ConfigSystem");
            if (configSystemType == null)
            {
                errorMessage = "ConfigSystem type was not found. Luban generated code may not have been generated yet.";
                return false;
            }

            object instance = GetStaticValue(configSystemType, "Instance");
            if (instance == null)
            {
                errorMessage = "ConfigSystem.Instance is null.";
                return false;
            }

            tables = GetMemberValue(instance, "Tables");
            if (tables == null)
            {
                errorMessage = "ConfigSystem.Tables is null.";
                return false;
            }

            return true;
        }

        private static Type FindType(string typeName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int index = 0; index < assemblies.Length; index++)
            {
                Type type = assemblies[index].GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static void LoadRows<T>(object tables, string tableName, ICollection<T> output, Func<object, T> factory)
        {
            object table = GetMemberValue(tables, tableName);
            if (table == null)
            {
                return;
            }

            foreach (object row in EnumerateTableRows(table))
            {
                if (row == null)
                {
                    continue;
                }

                output.Add(factory(row));
            }
        }

        private static IEnumerable<object> EnumerateTableRows(object table)
        {
            object dataList = GetMemberValue(table, "DataList");
            if (dataList is IEnumerable list)
            {
                foreach (object item in list)
                {
                    yield return item;
                }

                yield break;
            }

            object dataMap = GetMemberValue(table, "DataMap") ?? GetMemberValue(table, "_dataMap") ?? GetMemberValue(table, "Data");
            if (dataMap is IDictionary dictionary)
            {
                foreach (object value in dictionary.Values)
                {
                    yield return value;
                }

                yield break;
            }

            if (table is IEnumerable tableEnumerable)
            {
                foreach (object item in tableEnumerable)
                {
                    yield return item;
                }
            }
        }

        private static object GetStaticValue(Type type, string memberName)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            PropertyInfo property = type.GetProperty(memberName, flags);
            if (property != null)
            {
                return property.GetValue(null);
            }

            FieldInfo field = type.GetField(memberName, flags);
            return field?.GetValue(null);
        }

        private static object GetMemberValue(object target, string memberName)
        {
            if (target == null || string.IsNullOrWhiteSpace(memberName))
            {
                return null;
            }

            Type type = target.GetType();
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            MemberInfo bestMatch = null;
            string normalizedMemberName = NormalizeMemberName(memberName);

            foreach (PropertyInfo property in type.GetProperties(flags))
            {
                if (string.Equals(property.Name, memberName, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(NormalizeMemberName(property.Name), normalizedMemberName, StringComparison.Ordinal))
                {
                    bestMatch = property;
                    break;
                }
            }

            if (bestMatch is PropertyInfo matchedProperty)
            {
                return matchedProperty.GetValue(target);
            }

            foreach (FieldInfo field in type.GetFields(flags))
            {
                if (string.Equals(field.Name, memberName, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(NormalizeMemberName(field.Name), normalizedMemberName, StringComparison.Ordinal))
                {
                    bestMatch = field;
                    break;
                }
            }

            return bestMatch is FieldInfo matchedField ? matchedField.GetValue(target) : null;
        }

        private static string NormalizeMemberName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value.Replace("_", string.Empty).Replace("-", string.Empty).ToLowerInvariant();
        }

        private static string GetString(object row, params string[] memberNames)
        {
            object value = GetFirstValue(row, memberNames);
            if (value == null)
            {
                return string.Empty;
            }

            string text = value.ToString();
            return string.Equals(text, "null", StringComparison.OrdinalIgnoreCase) ? string.Empty : text;
        }

        private static int GetInt(object row, params string[] memberNames)
        {
            object value = GetFirstValue(row, memberNames);
            if (value == null)
            {
                return 0;
            }

            if (value is int intValue)
            {
                return intValue;
            }

            return int.TryParse(value.ToString(), out int parsed) ? parsed : 0;
        }

        private static float GetFloat(object row, params string[] memberNames)
        {
            object value = GetFirstValue(row, memberNames);
            if (value == null)
            {
                return 0f;
            }

            if (value is float floatValue)
            {
                return floatValue;
            }

            if (value is double doubleValue)
            {
                return (float)doubleValue;
            }

            if (value is int intValue)
            {
                return intValue;
            }

            return float.TryParse(value.ToString(), out float parsed) ? parsed : 0f;
        }

        private static int? GetNullableInt(object row, params string[] memberNames)
        {
            object value = GetFirstValue(row, memberNames);
            if (value == null)
            {
                return null;
            }

            if (value is int intValue)
            {
                return intValue;
            }

            string text = value.ToString();
            if (string.IsNullOrWhiteSpace(text) || string.Equals(text, "null", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return int.TryParse(text, out int parsed) ? parsed : null;
        }

        private static bool GetBool(object row, params string[] memberNames)
        {
            object value = GetFirstValue(row, memberNames);
            if (value == null)
            {
                return false;
            }

            if (value is bool boolValue)
            {
                return boolValue;
            }

            string text = value.ToString();
            return string.Equals(text, "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(text, "yes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(text, "y", StringComparison.OrdinalIgnoreCase)
                || text == "1";
        }

        private static List<string> GetStringList(object row, params string[] memberNames)
        {
            object value = GetFirstValue(row, memberNames);
            List<string> result = new List<string>();
            AppendStringListValue(result, value);
            return result;
        }

        private static object GetFirstValue(object row, params string[] memberNames)
        {
            for (int index = 0; index < memberNames.Length; index++)
            {
                object value = GetMemberValue(row, memberNames[index]);
                if (value != null)
                {
                    return value;
                }
            }

            return null;
        }

        private static void AppendStringListValue(List<string> result, object value)
        {
            if (value == null)
            {
                return;
            }

            if (value is string text)
            {
                string[] parts = text.Split(new[] { ';', '|', ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int index = 0; index < parts.Length; index++)
                {
                    string item = parts[index].Trim();
                    if (!string.IsNullOrEmpty(item) && !string.Equals(item, "null", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Add(item);
                    }
                }

                return;
            }

            if (value is IEnumerable enumerable)
            {
                foreach (object itemValue in enumerable)
                {
                    AppendStringListValue(result, itemValue);
                }

                return;
            }

            string fallback = value.ToString();
            if (!string.IsNullOrWhiteSpace(fallback) && !string.Equals(fallback, "null", StringComparison.OrdinalIgnoreCase))
            {
                result.Add(fallback);
            }
        }

        private static DndPrimaryAbilityMode GetPrimaryAbilityMode(object row)
        {
            string value = GetString(row, "PrimaryAbilityMode", "primary_ability_mode", "primaryAbilityMode");
            return Enum.TryParse(value, true, out DndPrimaryAbilityMode parsed) ? parsed : DndPrimaryAbilityMode.Fixed;
        }

        private static DndRulePackageData CreateRulePackage(object row)
        {
            return new DndRulePackageData
            {
                PackageId = GetString(row, "PackageId", "package_id", "packageId"),
                PackageName = GetString(row, "PackageName", "package_name", "packageName"),
                Version = GetString(row, "Version", "version"),
                Author = GetString(row, "Author", "author"),
                License = GetString(row, "License", "license"),
                Description = GetString(row, "Description", "description")
            };
        }

        private static DndClassDefineData CreateClassDefine(object row)
        {
            DndClassDefineData data = new DndClassDefineData
            {
                ClassId = GetString(row, "ClassId", "class_id", "classId"),
                PackageId = GetString(row, "PackageId", "package_id", "packageId"),
                Name = GetString(row, "Name", "name"),
                HitDie = GetInt(row, "HitDie", "hit_die", "hitDie"),
                PrimaryAbilityMode = GetPrimaryAbilityMode(row),
                SpellcastingAbility = GetString(row, "SpellcastingAbility", "spellcasting_ability", "spellcastingAbility"),
                SpellSlotProgressionId = GetString(row, "SpellSlotProgressionId", "spell_slot_progression_id", "spellSlotProgressionId"),
                Description = GetString(row, "Description", "description")
            };
            data.PrimaryAbilityIds.AddRange(GetStringList(row, "PrimaryAbilityIds", "primary_ability_ids", "primaryAbilityIds"));
            data.SavingThrowProficiencies.AddRange(GetStringList(row, "SavingThrowProficiencies", "saving_throw_proficiencies", "savingThrowProficiencies"));
            data.ArmorProficiencies.AddRange(GetStringList(row, "ArmorProficiencies", "armor_proficiencies", "armorProficiencies"));
            data.WeaponProficiencies.AddRange(GetStringList(row, "WeaponProficiencies", "weapon_proficiencies", "weaponProficiencies"));
            data.ToolProficiencies.AddRange(GetStringList(row, "ToolProficiencies", "tool_proficiencies", "toolProficiencies"));
            return data;
        }

        private static DndLevelProgressionData CreateLevelProgression(object row)
        {
            DndLevelProgressionData data = new DndLevelProgressionData
            {
                ProgressionId = GetString(row, "ProgressionId", "progression_id", "progressionId"),
                OwnerType = GetString(row, "OwnerType", "owner_type", "ownerType"),
                OwnerId = GetString(row, "OwnerId", "owner_id", "ownerId"),
                ClassId = GetString(row, "ClassId", "class_id", "classId"),
                SubclassId = GetString(row, "SubclassId", "subclass_id", "subclassId"),
                Level = GetInt(row, "Level", "level"),
                ProficiencyBonus = GetInt(row, "ProficiencyBonus", "proficiency_bonus", "proficiencyBonus"),
                FixedHpGain = GetInt(row, "FixedHpGain", "fixed_hp_gain", "fixedHpGain"),
                SpellSlotProgressionLevel = GetNullableInt(row, "SpellSlotProgressionLevel", "spell_slot_progression_level", "spellSlotProgressionLevel"),
                CantripKnown = GetNullableInt(row, "CantripKnown", "cantrip_known", "cantripKnown"),
                SpellKnown = GetNullableInt(row, "SpellKnown", "spell_known", "spellKnown"),
                PreparedSpellFormula = GetString(row, "PreparedSpellFormula", "prepared_spell_formula", "preparedSpellFormula"),
                AsiAvailable = GetBool(row, "AsiAvailable", "asi_available", "asiAvailable"),
                AsiRuleId = GetString(row, "AsiRuleId", "asi_rule_id", "asiRuleId"),
                SubclassFeature = GetBool(row, "SubclassFeature", "subclass_feature", "subclassFeature"),
                Note = GetString(row, "Note", "note")
            };
            data.FeatureIds.AddRange(GetStringList(row, "FeatureIds", "feature_ids", "featureIds"));
            data.ChoiceGroupIds.AddRange(GetStringList(row, "ChoiceGroupIds", "choice_group_ids", "choiceGroupIds"));
            data.ResourceGrantIds.AddRange(GetStringList(row, "ResourceGrantIds", "resource_grant_ids", "resourceGrantIds"));
            return data;
        }

        private static DndSubclassLevelProgressionData CreateSubclassLevelProgression(object row)
        {
            DndSubclassLevelProgressionData data = new DndSubclassLevelProgressionData
            {
                SubclassId = GetString(row, "SubclassId", "subclass_id", "subclassId"),
                ClassId = GetString(row, "ClassId", "class_id", "classId"),
                Level = GetInt(row, "Level", "level"),
                SpellSlotProgressionLevel = GetNullableInt(row, "SpellSlotProgressionLevel", "spell_slot_progression_level", "spellSlotProgressionLevel"),
                CantripKnown = GetNullableInt(row, "CantripKnown", "cantrip_known", "cantripKnown"),
                SpellKnown = GetNullableInt(row, "SpellKnown", "spell_known", "spellKnown"),
                PreparedSpellFormula = GetString(row, "PreparedSpellFormula", "prepared_spell_formula", "preparedSpellFormula"),
                Note = GetString(row, "Note", "note")
            };
            data.FeatureIds.AddRange(GetStringList(row, "FeatureIds", "feature_ids", "featureIds"));
            data.ChoiceGroupIds.AddRange(GetStringList(row, "ChoiceGroupIds", "choice_group_ids", "choiceGroupIds"));
            data.ResourceGrantIds.AddRange(GetStringList(row, "ResourceGrantIds", "resource_grant_ids", "resourceGrantIds"));
            return data;
        }

        private static DndFeatureDefineData CreateFeatureDefine(object row)
        {
            DndFeatureDefineData data = new DndFeatureDefineData
            {
                FeatureId = GetString(row, "FeatureId", "feature_id", "featureId"),
                PackageId = GetString(row, "PackageId", "package_id", "packageId"),
                Name = GetString(row, "Name", "name"),
                FeatureType = GetString(row, "FeatureType", "feature_type", "featureType"),
                SourceRef = GetString(row, "SourceRef", "source_ref", "sourceRef"),
                Description = GetString(row, "Description", "description")
            };
            data.PrerequisiteIds.AddRange(GetStringList(row, "PrerequisiteIds", "prerequisite_ids", "prerequisiteIds"));
            data.EffectIds.AddRange(GetStringList(row, "EffectIds", "effect_ids", "effectIds"));
            data.ChoiceGroupIds.AddRange(GetStringList(row, "ChoiceGroupIds", "choice_group_ids", "choiceGroupIds"));
            return data;
        }

        private static DndFeatureEffectData CreateFeatureEffect(object row)
        {
            DndFeatureEffectData data = new DndFeatureEffectData
            {
                EffectId = GetString(row, "EffectId", "effect_id", "effectId"),
                EffectType = GetString(row, "EffectType", "effect_type", "effectType"),
                Target = GetString(row, "Target", "target"),
                Value = GetString(row, "Value", "value"),
                Condition = GetString(row, "Condition", "condition"),
                StackingRule = GetString(row, "StackingRule", "stacking_rule", "stackingRule"),
                ManualNote = GetString(row, "ManualNote", "manual_note", "manualNote")
            };
            data.ConditionIds.AddRange(GetStringList(row, "ConditionIds", "condition_ids", "conditionIds"));
            return data;
        }

        private static DndFeatureEffectConditionData CreateFeatureEffectCondition(object row)
        {
            return new DndFeatureEffectConditionData
            {
                ConditionId = GetString(row, "ConditionId", "condition_id", "conditionId"),
                PackageId = GetString(row, "PackageId", "package_id", "packageId"),
                ConditionType = GetString(row, "ConditionType", "condition_type", "conditionType"),
                Target = GetString(row, "Target", "target"),
                Operator = GetString(row, "Operator", "operator"),
                Value = GetString(row, "Value", "value"),
                Description = GetString(row, "Description", "description")
            };
        }

        private static DndChoiceGroupData CreateChoiceGroup(object row)
        {
            DndChoiceGroupData data = new DndChoiceGroupData
            {
                ChoiceGroupId = GetString(row, "ChoiceGroupId", "choice_group_id", "choiceGroupId"),
                PackageId = GetString(row, "PackageId", "package_id", "packageId"),
                Name = GetString(row, "Name", "name"),
                ChoiceType = GetString(row, "ChoiceType", "choice_type", "choiceType"),
                MinSelect = GetInt(row, "MinSelect", "min_select", "minSelect"),
                MaxSelect = GetInt(row, "MaxSelect", "max_select", "maxSelect"),
                OptionFilter = GetString(row, "OptionFilter", "option_filter", "optionFilter"),
                SelectionMode = GetString(row, "SelectionMode", "selection_mode", "selectionMode"),
                ValuePerSelection = GetInt(row, "ValuePerSelection", "value_per_selection", "valuePerSelection"),
                MaxValuePerOption = GetInt(row, "MaxValuePerOption", "max_value_per_option", "maxValuePerOption"),
                TargetValueCap = GetInt(row, "TargetValueCap", "target_value_cap", "targetValueCap"),
                TargetValueFloor = GetInt(row, "TargetValueFloor", "target_value_floor", "targetValueFloor"),
                SelectionValueMode = GetString(row, "SelectionValueMode", "selection_value_mode", "selectionValueMode"),
                ResultValueType = GetString(row, "ResultValueType", "result_value_type", "resultValueType"),
                ResultStorage = GetString(row, "ResultStorage", "result_storage", "resultStorage"),
                UiMode = GetString(row, "UiMode", "ui_mode", "uiMode"),
                Description = GetString(row, "Description", "description")
            };
            data.NextChoiceGroupIds.AddRange(GetStringList(row, "NextChoiceGroupIds", "next_choice_group_ids", "nextChoiceGroupIds"));
            return data;
        }

        private static DndChoiceOptionData CreateChoiceOption(object row)
        {
            DndChoiceOptionData data = new DndChoiceOptionData
            {
                ChoiceGroupId = GetString(row, "ChoiceGroupId", "choice_group_id", "choiceGroupId"),
                OptionId = GetString(row, "OptionId", "option_id", "optionId"),
                Name = GetString(row, "Name", "name"),
                Description = GetString(row, "Description", "description")
            };
            data.GrantFeatureIds.AddRange(GetStringList(row, "GrantFeatureIds", "grant_feature_ids", "grantFeatureIds"));
            data.GrantEffectIds.AddRange(GetStringList(row, "GrantEffectIds", "grant_effect_ids", "grantEffectIds"));
            return data;
        }

        private static DndSkillDefineData CreateSkillDefine(object row)
        {
            return new DndSkillDefineData
            {
                SkillId = GetString(row, "SkillId", "skill_id", "skillId"),
                PackageId = GetString(row, "PackageId", "package_id", "packageId"),
                Name = GetString(row, "Name", "name"),
                AbilityId = GetString(row, "AbilityId", "ability_id", "abilityId"),
                Description = GetString(row, "Description", "description")
            };
        }

        private static DndRaceMainDefineData CreateRaceMainDefine(object row)
        {
            DndRaceMainDefineData data = new DndRaceMainDefineData
            {
                MainRaceId = GetString(row, "MainRaceId", "main_race_id", "mainRaceId", "RaceId", "race_id", "raceId", "Id", "id"),
                PackageId = GetString(row, "PackageId", "package_id", "packageId"),
                Name = GetString(row, "Name", "name", "DisplayName", "display_name", "displayName"),
                Size = GetString(row, "Size", "size"),
                Speed = GetInt(row, "Speed", "speed", "BaseSpeed", "base_speed", "baseSpeed"),
                Description = GetString(row, "Description", "description", "Note", "note")
            };
            data.LanguageIds.AddRange(GetStringList(row, "LanguageIds", "language_ids", "languageIds", "Languages", "languages"));
            data.MainFeatureIds.AddRange(GetStringList(row, "MainFeatureIds", "main_feature_ids", "mainFeatureIds", "FeatureIds", "feature_ids", "featureIds"));
            data.ChoiceGroupIds.AddRange(GetStringList(row, "ChoiceGroupIds", "choice_group_ids", "choiceGroupIds"));
            return data;
        }

        private static DndRaceSubDefineData CreateRaceSubDefine(object row)
        {
            DndRaceSubDefineData data = new DndRaceSubDefineData
            {
                SubRaceId = GetString(row, "SubRaceId", "sub_race_id", "subRaceId", "RaceId", "race_id", "raceId", "Id", "id"),
                PackageId = GetString(row, "PackageId", "package_id", "packageId"),
                MainRaceId = GetString(row, "MainRaceId", "main_race_id", "mainRaceId", "ParentRaceId", "parent_race_id", "parentRaceId", "BelongMainRaceId", "belong_main_race_id", "belongMainRaceId"),
                Name = GetString(row, "Name", "name", "DisplayName", "display_name", "displayName"),
                Size = GetString(row, "Size", "size"),
                Speed = GetInt(row, "Speed", "speed", "BaseSpeed", "base_speed", "baseSpeed"),
                Description = GetString(row, "Description", "description", "Note", "note")
            };
            data.FeatureIds.AddRange(GetStringList(row, "FeatureIds", "feature_ids", "featureIds", "SubFeatureIds", "sub_feature_ids", "subFeatureIds", "UniqueFeatureIds", "unique_feature_ids", "uniqueFeatureIds"));
            data.ChoiceGroupIds.AddRange(GetStringList(row, "ChoiceGroupIds", "choice_group_ids", "choiceGroupIds"));
            return data;
        }

        private static void NormalizeRaceData(DndRuleContentLibraryData library)
        {
            library.Races.Clear();

            Dictionary<string, DndRaceMainDefineData> raceMainById = new Dictionary<string, DndRaceMainDefineData>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < library.RaceMains.Count; index++)
            {
                DndRaceMainDefineData raceMain = library.RaceMains[index];
                if (string.IsNullOrWhiteSpace(raceMain.MainRaceId))
                {
                    continue;
                }

                raceMainById[raceMain.MainRaceId] = raceMain;
            }

            Dictionary<string, List<DndRaceSubDefineData>> subRacesByMainId = new Dictionary<string, List<DndRaceSubDefineData>>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < library.RaceSubs.Count; index++)
            {
                DndRaceSubDefineData raceSub = library.RaceSubs[index];
                if (string.IsNullOrWhiteSpace(raceSub.MainRaceId))
                {
                    continue;
                }

                if (!subRacesByMainId.TryGetValue(raceSub.MainRaceId, out List<DndRaceSubDefineData> subRaces))
                {
                    subRaces = new List<DndRaceSubDefineData>();
                    subRacesByMainId[raceSub.MainRaceId] = subRaces;
                }

                subRaces.Add(raceSub);
            }

            foreach (DndRaceMainDefineData raceMain in library.RaceMains)
            {
                if (string.IsNullOrWhiteSpace(raceMain.MainRaceId))
                {
                    continue;
                }

                if (!subRacesByMainId.TryGetValue(raceMain.MainRaceId, out List<DndRaceSubDefineData> subRaces) || subRaces.Count == 0)
                {
                    library.Races.Add(CreateRaceFromMain(raceMain));
                    continue;
                }

                for (int index = 0; index < subRaces.Count; index++)
                {
                    library.Races.Add(CreateRaceFromMainAndSub(raceMain, subRaces[index]));
                }
            }

            for (int index = 0; index < library.RaceSubs.Count; index++)
            {
                DndRaceSubDefineData raceSub = library.RaceSubs[index];
                if (string.IsNullOrWhiteSpace(raceSub.SubRaceId)
                    || string.IsNullOrWhiteSpace(raceSub.MainRaceId)
                    || raceMainById.ContainsKey(raceSub.MainRaceId))
                {
                    continue;
                }

                library.Races.Add(CreateRaceFromSubOnly(raceSub));
            }
        }

        private static DndRaceDefineData CreateRaceFromMain(DndRaceMainDefineData raceMain)
        {
            DndRaceDefineData data = new DndRaceDefineData
            {
                RaceId = raceMain.MainRaceId,
                PackageId = raceMain.PackageId,
                Name = raceMain.Name,
                Size = raceMain.Size,
                Speed = raceMain.Speed,
                Description = raceMain.Description
            };
            data.LanguageIds.AddRange(raceMain.LanguageIds);
            data.FeatureIds.AddRange(raceMain.MainFeatureIds);
            data.ChoiceGroupIds.AddRange(raceMain.ChoiceGroupIds);
            return data;
        }

        private static DndRaceDefineData CreateRaceFromMainAndSub(DndRaceMainDefineData raceMain, DndRaceSubDefineData raceSub)
        {
            DndRaceDefineData data = new DndRaceDefineData
            {
                RaceId = string.IsNullOrWhiteSpace(raceSub.SubRaceId) ? raceMain.MainRaceId : raceSub.SubRaceId,
                PackageId = !string.IsNullOrWhiteSpace(raceSub.PackageId) ? raceSub.PackageId : raceMain.PackageId,
                Name = string.IsNullOrWhiteSpace(raceSub.Name) ? raceMain.Name : raceSub.Name,
                Size = string.IsNullOrWhiteSpace(raceSub.Size) ? raceMain.Size : raceSub.Size,
                Speed = raceSub.Speed > 0 ? raceSub.Speed : raceMain.Speed,
                Description = string.IsNullOrWhiteSpace(raceSub.Description) ? raceMain.Description : raceSub.Description
            };
            data.LanguageIds.AddRange(raceMain.LanguageIds);
            data.FeatureIds.AddRange(raceMain.MainFeatureIds);
            data.FeatureIds.AddRange(raceSub.FeatureIds);
            data.ChoiceGroupIds.AddRange(raceMain.ChoiceGroupIds);
            data.ChoiceGroupIds.AddRange(raceSub.ChoiceGroupIds);
            return data;
        }

        private static DndRaceDefineData CreateRaceFromSubOnly(DndRaceSubDefineData raceSub)
        {
            DndRaceDefineData data = new DndRaceDefineData
            {
                RaceId = raceSub.SubRaceId,
                PackageId = raceSub.PackageId,
                Name = raceSub.Name,
                Size = raceSub.Size,
                Speed = raceSub.Speed,
                Description = raceSub.Description
            };
            data.FeatureIds.AddRange(raceSub.FeatureIds);
            data.ChoiceGroupIds.AddRange(raceSub.ChoiceGroupIds);
            return data;
        }

        private static DndBackgroundDefineData CreateBackgroundDefine(object row)
        {
            DndBackgroundDefineData data = new DndBackgroundDefineData
            {
                BackgroundId = GetString(row, "BackgroundId", "background_id", "backgroundId"),
                PackageId = GetString(row, "PackageId", "package_id", "packageId"),
                Name = GetString(row, "Name", "name"),
                Description = GetString(row, "Description", "description")
            };
            data.SkillProficiencies.AddRange(GetStringList(row, "SkillProficiencies", "skill_proficiencies", "skillProficiencies"));
            data.ToolProficiencies.AddRange(GetStringList(row, "ToolProficiencies", "tool_proficiencies", "toolProficiencies"));
            data.LanguageIds.AddRange(GetStringList(row, "LanguageIds", "language_ids", "languageIds"));
            data.FeatureIds.AddRange(GetStringList(row, "FeatureIds", "feature_ids", "featureIds"));
            data.EquipmentGrantIds.AddRange(GetStringList(row, "EquipmentGrantIds", "equipment_grant_ids", "equipmentGrantIds"));
            return data;
        }

        private static DndFeatDefineData CreateFeatDefine(object row)
        {
            DndFeatDefineData data = new DndFeatDefineData
            {
                FeatId = GetString(row, "FeatId", "feat_id", "featId"),
                PackageId = GetString(row, "PackageId", "package_id", "packageId"),
                Name = GetString(row, "Name", "name"),
                Description = GetString(row, "Description", "description")
            };
            data.PrerequisiteIds.AddRange(GetStringList(row, "PrerequisiteIds", "prerequisite_ids", "prerequisiteIds"));
            data.FeatureIds.AddRange(GetStringList(row, "FeatureIds", "feature_ids", "featureIds"));
            data.ChoiceGroupIds.AddRange(GetStringList(row, "ChoiceGroupIds", "choice_group_ids", "choiceGroupIds"));
            return data;
        }

        private static DndSpellDefineData CreateSpellDefine(object row)
        {
            DndSpellDefineData data = new DndSpellDefineData
            {
                SpellId = GetString(row, "SpellId", "spell_id", "spellId"),
                PackageId = GetString(row, "PackageId", "package_id", "packageId"),
                Name = GetString(row, "Name", "name"),
                Level = GetInt(row, "Level", "level"),
                School = GetString(row, "School", "school"),
                CastingTime = GetString(row, "CastingTime", "casting_time", "castingTime"),
                Range = GetString(row, "Range", "range"),
                Components = GetString(row, "Components", "components"),
                Duration = GetString(row, "Duration", "duration"),
                Concentration = GetBool(row, "Concentration", "concentration"),
                Ritual = GetBool(row, "Ritual", "ritual"),
                AttackType = GetString(row, "AttackType", "attack_type", "attackType"),
                SaveAbility = GetString(row, "SaveAbility", "save_ability", "saveAbility"),
                DamageFormula = GetString(row, "DamageFormula", "damage_formula", "damageFormula"),
                DamageType = GetString(row, "DamageType", "damage_type", "damageType"),
                Description = GetString(row, "Description", "description"),
                HigherLevelDescription = GetString(row, "HigherLevelDescription", "higher_level_description", "higherLevelDescription")
            };
            data.EffectTags.AddRange(GetStringList(row, "EffectTags", "effect_tags", "effectTags"));
            return data;
        }

        private static DndClassSpellListData CreateClassSpellList(object row)
        {
            return new DndClassSpellListData
            {
                ClassId = GetString(row, "ClassId", "class_id", "classId"),
                SpellId = GetString(row, "SpellId", "spell_id", "spellId"),
                MinClassLevel = GetInt(row, "MinClassLevel", "min_class_level", "minClassLevel"),
                AlwaysPrepared = GetBool(row, "AlwaysPrepared", "always_prepared", "alwaysPrepared"),
                SourceFeatureId = GetString(row, "SourceFeatureId", "source_feature_id", "sourceFeatureId"),
                Note = GetString(row, "Note", "note")
            };
        }

        private static DndSpellSlotProgressionData CreateSpellSlotProgression(object row)
        {
            return new DndSpellSlotProgressionData
            {
                ProgressionId = GetString(row, "ProgressionId", "progression_id", "progressionId"),
                ProgressionLevel = GetInt(row, "ProgressionLevel", "progression_level", "progressionLevel"),
                MaxSpellLevel = GetInt(row, "MaxSpellLevel", "max_spell_level", "maxSpellLevel"),
                Slot1 = GetInt(row, "Slot1", "slot_1", "slot1"),
                Slot2 = GetInt(row, "Slot2", "slot_2", "slot2"),
                Slot3 = GetInt(row, "Slot3", "slot_3", "slot3"),
                Slot4 = GetInt(row, "Slot4", "slot_4", "slot4"),
                Slot5 = GetInt(row, "Slot5", "slot_5", "slot5"),
                Slot6 = GetInt(row, "Slot6", "slot_6", "slot6"),
                Slot7 = GetInt(row, "Slot7", "slot_7", "slot7"),
                Slot8 = GetInt(row, "Slot8", "slot_8", "slot8"),
                Slot9 = GetInt(row, "Slot9", "slot_9", "slot9")
            };
        }

        private static DndLevelExperienceData CreateLevelExperience(object row)
        {
            return new DndLevelExperienceData
            {
                Level = GetInt(row, "Level", "level"),
                ExperienceThreshold = GetInt(row, "ExperienceThreshold", "experience_threshold", "experienceThreshold"),
                ProficiencyBonus = GetInt(row, "ProficiencyBonus", "proficiency_bonus", "proficiencyBonus"),
                Description = GetString(row, "Description", "description")
            };
        }

        private static DndItemEffectData CreateItemEffect(object row)
        {
            return new DndItemEffectData
            {
                EffectId = GetString(row, "EffectId", "effect_id", "effectId", "ItemEffectId", "item_effect_id", "itemEffectId"),
                PackageId = GetString(row, "PackageId", "package_id", "packageId"),
                EffectType = GetString(row, "EffectType", "effect_type", "effectType"),
                Target = GetString(row, "Target", "target"),
                Value = GetString(row, "Value", "value"),
                ApplyMode = GetString(row, "ApplyMode", "apply_mode", "applyMode"),
                Condition = GetString(row, "Condition", "condition"),
                ConditionDescription = GetString(row, "ConditionDescription", "condition_description", "conditionDescription"),
                Description = GetString(row, "Description", "description")
            };
        }

        private static DndItemDefineData CreateDndItemDefine(object row)
        {
            DndItemDefineData data = new DndItemDefineData
            {
                ItemId = GetString(row, "ItemId", "item_id", "itemId"),
                PackageId = GetString(row, "PackageId", "package_id", "packageId"),
                Name = GetString(row, "Name", "name"),
                ItemType = GetString(row, "ItemType", "item_type", "itemType"),
                Rarity = GetString(row, "Rarity", "rarity"),
                Description = GetString(row, "Description", "description"),
                SourceBook = GetString(row, "SourceBook", "source_book", "sourceBook"),
                SourcePage = GetString(row, "SourcePage", "source_page", "sourcePage"),
                Stackable = GetBool(row, "Stackable", "stackable"),
                MaxStack = GetInt(row, "MaxStack", "max_stack", "maxStack"),
                Weight = GetFloat(row, "Weight", "weight"),
                PriceGp = GetInt(row, "PriceGp", "price_gp", "priceGp"),
                IsEquippable = GetBool(row, "IsEquippable", "is_equippable", "isEquippable"),
                DefaultQuantity = GetInt(row, "DefaultQuantity", "default_quantity", "defaultQuantity"),
                EquipmentSlot = GetString(row, "EquipmentSlot", "equipment_slot", "equipmentSlot"),
                RequiresAttunement = GetBool(row, "RequiresAttunement", "requires_attunement", "requiresAttunement"),
                DefaultEquipped = GetBool(row, "DefaultEquipped", "default_equipped", "defaultEquipped"),
                ArmorCategory = GetString(row, "ArmorCategory", "armor_category", "armorCategory"),
                ArmorBaseAc = GetInt(row, "ArmorBaseAc", "armor_base_ac", "armorBaseAc"),
                AcBonus = GetInt(row, "AcBonus", "ac_bonus", "acBonus"),
                MaxDexBonus = GetInt(row, "MaxDexBonus", "max_dex_bonus", "maxDexBonus"),
                StrengthRequirement = GetInt(row, "StrengthRequirement", "strength_requirement", "strengthRequirement"),
                StealthDisadvantage = GetBool(row, "StealthDisadvantage", "stealth_disadvantage", "stealthDisadvantage"),
                WeaponCategory = GetString(row, "WeaponCategory", "weapon_category", "weaponCategory"),
                WeaponRangeType = GetString(row, "WeaponRangeType", "weapon_range_type", "weaponRangeType"),
                DamageDice = GetString(row, "DamageDice", "damage_dice", "damageDice"),
                DamageType = GetString(row, "DamageType", "damage_type", "damageType"),
                NormalRange = GetInt(row, "NormalRange", "normal_range", "normalRange"),
                LongRange = GetInt(row, "LongRange", "long_range", "longRange"),
                TwoHandDamageDice = GetString(row, "TwoHandDamageDice", "two_hand_damage_dice", "twoHandDamageDice"),
                ToolCategory = GetString(row, "ToolCategory", "tool_category", "toolCategory"),
                Consumable = GetBool(row, "Consumable", "consumable"),
                Charges = GetInt(row, "Charges", "charges"),
                ConsumeOnUse = GetBool(row, "ConsumeOnUse", "consume_on_use", "consumeOnUse"),
                EffectApplyCondition = GetString(row, "EffectApplyCondition", "effect_apply_condition", "effectApplyCondition")
            };
            data.WeaponProperties.AddRange(GetStringList(row, "WeaponProperties", "weapon_properties", "weaponProperties"));
            data.EffectIds.AddRange(GetStringList(row, "EffectIds", "effect_ids", "effectIds"));
            return data;
        }

        private static DndItemTypeDefineData CreateDndItemTypeDefine(object row)
        {
            return new DndItemTypeDefineData
            {
                ItemTypeId = GetString(row, "ItemTypeId", "item_type_id", "itemTypeId"),
                ParentTypeId = GetString(row, "ParentTypeId", "parent_type_id", "parentTypeId"),
                Name = GetString(row, "Name", "name"),
                Selectable = GetBool(row, "Selectable", "selectable"),
                CanUseByDefault = GetBool(row, "CanUseByDefault", "can_use_by_default", "canUseByDefault"),
                StackableByDefault = GetBool(row, "StackableByDefault", "stackable_by_default", "stackableByDefault"),
                ConsumeQuantityOnUseByDefault = GetBool(row, "ConsumeQuantityOnUseByDefault", "consume_quantity_on_use_by_default", "consumeQuantityOnUseByDefault"),
                CanHaveCharges = GetBool(row, "CanHaveCharges", "can_have_charges", "canHaveCharges"),
                ConsumeChargeOnUseByDefault = GetBool(row, "ConsumeChargeOnUseByDefault", "consume_charge_on_use_by_default", "consumeChargeOnUseByDefault"),
                IsEquipmentType = GetBool(row, "IsEquipmentType", "is_equipment_type", "isEquipmentType"),
                DefaultEquipmentSlot = GetString(row, "DefaultEquipmentSlot", "default_equipment_slot", "defaultEquipmentSlot"),
                SortOrder = GetInt(row, "SortOrder", "sort_order", "sortOrder"),
                Description = GetString(row, "Description", "description")
            };
        }

        private static DndEnumListData CreateEnumList(object row)
        {
            return new DndEnumListData
            {
                EnumType = GetString(row, "EnumType", "enum_type", "enumType"),
                Value = GetString(row, "Value", "value"),
                Description = GetString(row, "Description", "description")
            };
        }

        private static DndToolDefineData CreateDndToolDefine(object row)
        {
            return new DndToolDefineData
            {
                ToolId = GetString(row, "ToolId", "tool_id", "toolId"),
                PackageId = GetString(row, "PackageId", "package_id", "packageId"),
                Name = GetString(row, "Name", "name"),
                ToolCategory = GetString(row, "ToolCategory", "tool_category", "toolCategory"),
                EnglishName = GetString(row, "EnglishName", "english_name", "englishName"),
                PriceGp = GetFloat(row, "PriceGp", "price_gp", "priceGp"),
                Weight = GetFloat(row, "Weight", "weight"),
                SourceBook = GetString(row, "SourceBook", "source_book", "sourceBook"),
                SourcePage = GetString(row, "SourcePage", "source_page", "sourcePage"),
                Description = GetString(row, "Description", "description")
            };
        }

        private static DndLanguageDefineData CreateDndLanguageDefine(object row)
        {
            return new DndLanguageDefineData
            {
                LanguageId = GetString(row, "LanguageId", "language_id", "languageId"),
                PackageId = GetString(row, "PackageId", "package_id", "packageId"),
                Name = GetString(row, "Name", "name"),
                EnglishName = GetString(row, "EnglishName", "english_name", "englishName"),
                LanguageCategory = GetString(row, "LanguageCategory", "language_category", "languageCategory"),
                Description = GetString(row, "Description", "description")
            };
        }

        private static DndAlignmentData CreateAlignment(object row)
        {
            return new DndAlignmentData
            {
                AlignmentId = GetString(row, "AlignmentId", "alignment_id", "alignmentId"),
                Name = GetString(row, "Name", "name"),
                Description = GetString(row, "Description", "description")
            };
        }

        private static DndTextLocalizeData CreateTextLocalize(object row)
        {
            return new DndTextLocalizeData
            {
                TextKey = GetString(row, "TextKey", "text_key", "textKey"),
                Language = GetString(row, "Language", "language"),
                Text = GetString(row, "Text", "text"),
                Context = GetString(row, "Context", "context")
            };
        }

        private static void ApplyTextLocalizations(DndRuleContentLibraryData library, string language)
        {
            Dictionary<string, string> textByKey = BuildTextLocalizationMap(library, language);
            if (textByKey.Count == 0)
            {
                return;
            }

            for (int index = 0; index < library.RulePackages.Count; index++)
            {
                DndRulePackageData item = library.RulePackages[index];
                item.PackageName = GetText(textByKey, "TbRulePackage", item.PackageId, "package_name", item.PackageName);
                item.Description = GetText(textByKey, "TbRulePackage", item.PackageId, "description", item.Description);
            }

            for (int index = 0; index < library.Classes.Count; index++)
            {
                DndClassDefineData item = library.Classes[index];
                item.Name = GetText(textByKey, "TbClassDefine", item.ClassId, "name", item.Name);
                item.Description = GetText(textByKey, "TbClassDefine", item.ClassId, "description", item.Description);
            }

            for (int index = 0; index < library.LevelProgressions.Count; index++)
            {
                DndLevelProgressionData item = library.LevelProgressions[index];
                string rowKey = BuildProgressionKey(item.ClassId, item.Level);
                item.Note = GetText(textByKey, "TbClassLevelProgression", rowKey, "note", item.Note);
            }

            for (int index = 0; index < library.SubclassLevelProgressions.Count; index++)
            {
                DndSubclassLevelProgressionData item = library.SubclassLevelProgressions[index];
                string rowKey = BuildProgressionKey(item.SubclassId, item.Level);
                item.Note = GetText(textByKey, "TbSubclassLevelProgression", rowKey, "note", item.Note);
            }

            for (int index = 0; index < library.Features.Count; index++)
            {
                DndFeatureDefineData item = library.Features[index];
                item.Name = GetText(textByKey, "TbFeatureDefine", item.FeatureId, "name", item.Name);
                item.Description = GetText(textByKey, "TbFeatureDefine", item.FeatureId, "description", item.Description);
            }

            for (int index = 0; index < library.FeatureEffects.Count; index++)
            {
                DndFeatureEffectData item = library.FeatureEffects[index];
                item.ManualNote = GetText(textByKey, "TbFeatureEffect", item.EffectId, "manual_note", item.ManualNote);
            }

            for (int index = 0; index < library.FeatureEffectConditions.Count; index++)
            {
                DndFeatureEffectConditionData item = library.FeatureEffectConditions[index];
                item.Description = GetText(textByKey, "TbFeatureEffectCondition", item.ConditionId, "description", item.Description);
            }

            for (int index = 0; index < library.ChoiceGroups.Count; index++)
            {
                DndChoiceGroupData item = library.ChoiceGroups[index];
                item.Name = GetText(textByKey, "TbChoiceGroup", item.ChoiceGroupId, "name", item.Name);
                item.Description = GetText(textByKey, "TbChoiceGroup", item.ChoiceGroupId, "description", item.Description);
            }

            for (int index = 0; index < library.ChoiceOptions.Count; index++)
            {
                DndChoiceOptionData item = library.ChoiceOptions[index];
                string rowKey = $"{item.ChoiceGroupId}.{item.OptionId}";
                item.Name = GetText(textByKey, "TbChoiceOption", rowKey, "name", item.Name);
                item.Description = GetText(textByKey, "TbChoiceOption", rowKey, "description", item.Description);
            }

            for (int index = 0; index < library.Skills.Count; index++)
            {
                DndSkillDefineData item = library.Skills[index];
                item.Name = GetText(textByKey, "TbSkillDefine", item.SkillId, "name", item.Name);
                item.Description = GetText(textByKey, "TbSkillDefine", item.SkillId, "description", item.Description);
            }

            for (int index = 0; index < library.RaceMains.Count; index++)
            {
                DndRaceMainDefineData item = library.RaceMains[index];
                item.Name = GetText(textByKey, "TbRaceMainDefine", item.MainRaceId, "name", item.Name);
                item.Description = GetText(textByKey, "TbRaceMainDefine", item.MainRaceId, "description", item.Description);
            }

            for (int index = 0; index < library.RaceSubs.Count; index++)
            {
                DndRaceSubDefineData item = library.RaceSubs[index];
                item.Name = GetText(textByKey, "TbRaceSubDefine", item.SubRaceId, "name", item.Name);
                item.Description = GetText(textByKey, "TbRaceSubDefine", item.SubRaceId, "description", item.Description);
            }

            for (int index = 0; index < library.Backgrounds.Count; index++)
            {
                DndBackgroundDefineData item = library.Backgrounds[index];
                item.Name = GetText(textByKey, "TbBackgroundDefine", item.BackgroundId, "name", item.Name);
                item.Description = GetText(textByKey, "TbBackgroundDefine", item.BackgroundId, "description", item.Description);
            }

            for (int index = 0; index < library.Feats.Count; index++)
            {
                DndFeatDefineData item = library.Feats[index];
                item.Name = GetText(textByKey, "TbFeatDefine", item.FeatId, "name", item.Name);
                item.Description = GetText(textByKey, "TbFeatDefine", item.FeatId, "description", item.Description);
            }

            for (int index = 0; index < library.Spells.Count; index++)
            {
                DndSpellDefineData item = library.Spells[index];
                item.Name = GetText(textByKey, "TbSpellDefine", item.SpellId, "name", item.Name);
                item.Description = GetText(textByKey, "TbSpellDefine", item.SpellId, "description", item.Description);
                item.HigherLevelDescription = GetText(textByKey, "TbSpellDefine", item.SpellId, "higher_level_description", item.HigherLevelDescription);
            }

            for (int index = 0; index < library.Items.Count; index++)
            {
                DndItemDefineData item = library.Items[index];
                item.Name = GetText(textByKey, "TbDndItemDefine", item.ItemId, "name", item.Name);
                item.Description = GetText(textByKey, "TbDndItemDefine", item.ItemId, "description", item.Description);
            }

            for (int index = 0; index < library.Tools.Count; index++)
            {
                DndToolDefineData item = library.Tools[index];
                item.Name = GetText(textByKey, "TbDndToolDefine", item.ToolId, "name", item.Name);
                item.Description = GetText(textByKey, "TbDndToolDefine", item.ToolId, "description", item.Description);
            }

            for (int index = 0; index < library.Languages.Count; index++)
            {
                DndLanguageDefineData item = library.Languages[index];
                item.Name = GetText(textByKey, "TbDndLanguageDefine", item.LanguageId, "name", item.Name);
                item.Description = GetText(textByKey, "TbDndLanguageDefine", item.LanguageId, "description", item.Description);
            }

            for (int index = 0; index < library.EnumLists.Count; index++)
            {
                DndEnumListData item = library.EnumLists[index];
                string rowKey = $"{item.EnumType}.{item.Value}";
                item.Description = GetText(textByKey, "TbEnumList", rowKey, "description", item.Description);
            }

            for (int index = 0; index < library.Alignments.Count; index++)
            {
                DndAlignmentData item = library.Alignments[index];
                item.Name = GetText(textByKey, "TbAlignment", item.AlignmentId, "name", item.Name);
                item.Description = GetText(textByKey, "TbAlignment", item.AlignmentId, "description", item.Description);
            }
        }

        private static Dictionary<string, string> BuildTextLocalizationMap(DndRuleContentLibraryData library, string language)
        {
            Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < library.TextLocalizations.Count; index++)
            {
                DndTextLocalizeData row = library.TextLocalizations[index];
                if (row == null
                    || string.IsNullOrWhiteSpace(row.TextKey)
                    || string.IsNullOrWhiteSpace(row.Text)
                    || (!string.IsNullOrWhiteSpace(language) && !string.Equals(row.Language, language, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                result[row.TextKey.Trim()] = row.Text.Trim();
            }

            return result;
        }

        private static string GetText(Dictionary<string, string> textByKey, string tableName, string rowKey, string fieldName, string fallback)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(rowKey) || string.IsNullOrWhiteSpace(fieldName))
            {
                return fallback;
            }

            string textKey = $"{tableName}.{rowKey.Trim()}.{fieldName.Trim()}";
            return textByKey.TryGetValue(textKey, out string text) && !string.IsNullOrWhiteSpace(text)
                ? text
                : fallback;
        }

        private static string BuildProgressionKey(string id, int level)
        {
            return string.IsNullOrWhiteSpace(id) ? level.ToString() : $"{id.Trim()}.{level}";
        }
    }
}
