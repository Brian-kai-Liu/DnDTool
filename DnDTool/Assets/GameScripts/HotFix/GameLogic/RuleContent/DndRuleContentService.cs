using System;
using System.Collections.Generic;

namespace GameLogic
{
    internal sealed class DndRuleContentService
    {
        private static readonly Lazy<DndRuleContentService> s_instance = new Lazy<DndRuleContentService>(() => new DndRuleContentService());

        private readonly IDndRuleContentSource m_source;
        private readonly Dictionary<string, DndClassDefineData> m_classById = new Dictionary<string, DndClassDefineData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DndFeatureDefineData> m_featureById = new Dictionary<string, DndFeatureDefineData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DndFeatureEffectData> m_featureEffectById = new Dictionary<string, DndFeatureEffectData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DndFeatureEffectConditionData> m_featureEffectConditionById = new Dictionary<string, DndFeatureEffectConditionData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DndFeatDefineData> m_featById = new Dictionary<string, DndFeatDefineData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DndChoiceGroupData> m_choiceGroupById = new Dictionary<string, DndChoiceGroupData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<DndChoiceOptionData>> m_choiceOptionsByGroupId = new Dictionary<string, List<DndChoiceOptionData>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DndSkillDefineData> m_skillById = new Dictionary<string, DndSkillDefineData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DndSpellDefineData> m_spellById = new Dictionary<string, DndSpellDefineData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DndItemDefineData> m_itemById = new Dictionary<string, DndItemDefineData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DndToolDefineData> m_toolById = new Dictionary<string, DndToolDefineData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DndLanguageDefineData> m_languageById = new Dictionary<string, DndLanguageDefineData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DndAlignmentData> m_alignmentById = new Dictionary<string, DndAlignmentData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DndTextLocalizeData> m_textLocalizeByKey = new Dictionary<string, DndTextLocalizeData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DndRaceMainDefineData> m_raceMainById = new Dictionary<string, DndRaceMainDefineData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DndRaceSubDefineData> m_raceSubById = new Dictionary<string, DndRaceSubDefineData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<DndLevelProgressionData>> m_classProgressionsByClassId = new Dictionary<string, List<DndLevelProgressionData>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<DndSubclassLevelProgressionData>> m_subclassProgressionsBySubclassId = new Dictionary<string, List<DndSubclassLevelProgressionData>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<DndClassSpellListData>> m_classSpellListsByClassId = new Dictionary<string, List<DndClassSpellListData>>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, string[]> ClassSkillListByClassId = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["barbarian"] = new[] { "animal_handling", "athletics", "intimidation", "nature", "perception", "survival" },
            ["bard"] = new[] { "acrobatics", "animal_handling", "arcana", "athletics", "deception", "history", "insight", "intimidation", "investigation", "medicine", "nature", "perception", "performance", "persuasion", "religion", "sleight_of_hand", "stealth", "survival" },
            ["cleric"] = new[] { "history", "insight", "medicine", "persuasion", "religion" },
            ["druid"] = new[] { "arcana", "animal_handling", "insight", "medicine", "nature", "perception", "religion", "survival" },
            ["fighter"] = new[] { "acrobatics", "animal_handling", "athletics", "history", "insight", "intimidation", "perception", "survival" },
            ["monk"] = new[] { "acrobatics", "athletics", "history", "insight", "religion", "stealth" },
            ["paladin"] = new[] { "athletics", "insight", "intimidation", "medicine", "persuasion", "religion" },
            ["ranger"] = new[] { "animal_handling", "athletics", "insight", "investigation", "nature", "perception", "stealth", "survival" },
            ["rogue"] = new[] { "acrobatics", "athletics", "deception", "insight", "intimidation", "investigation", "perception", "performance", "persuasion", "sleight_of_hand", "stealth" },
            ["sorcerer"] = new[] { "arcana", "deception", "insight", "intimidation", "persuasion", "religion" },
            ["warlock"] = new[] { "arcana", "deception", "history", "intimidation", "investigation", "nature", "religion" },
            ["wizard"] = new[] { "arcana", "history", "insight", "investigation", "medicine", "religion" }
        };

        private DndRuleContentLibraryData m_library = new DndRuleContentLibraryData();
        private bool m_loaded;
        private string m_lastErrorMessage = string.Empty;

        private DndRuleContentService()
            : this(new LubanDndRuleContentSource())
        {
        }

        internal DndRuleContentService(IDndRuleContentSource source)
        {
            m_source = source;
        }

        public static DndRuleContentService Instance => s_instance.Value;

        public string LastErrorMessage => m_lastErrorMessage;

        public IReadOnlyList<DndRulePackageData> RulePackages => GetLibrary().RulePackages;

        public IReadOnlyList<DndClassDefineData> Classes => GetLibrary().Classes;

        public IReadOnlyList<DndRaceDefineData> Races => GetLibrary().Races;

        public IReadOnlyList<DndRaceMainDefineData> RaceMains => GetLibrary().RaceMains;

        public IReadOnlyList<DndRaceSubDefineData> RaceSubs => GetLibrary().RaceSubs;

        public IReadOnlyList<DndBackgroundDefineData> Backgrounds => GetLibrary().Backgrounds;

        public IReadOnlyList<DndFeatDefineData> Feats => GetLibrary().Feats;

        public IReadOnlyList<DndSpellDefineData> Spells => GetLibrary().Spells;

        public IReadOnlyList<DndAlignmentData> Alignments => GetLibrary().Alignments;

        public IReadOnlyList<DndSkillDefineData> Skills => GetLibrary().Skills;

        public IReadOnlyList<DndItemDefineData> Items => GetLibrary().Items;

        public IReadOnlyList<DndToolDefineData> Tools => GetLibrary().Tools;

        public IReadOnlyList<DndLanguageDefineData> Languages => GetLibrary().Languages;

        public IReadOnlyList<DndTextLocalizeData> TextLocalizations => GetLibrary().TextLocalizations;

        public bool HasLoadedContent()
        {
            DndRuleContentLibraryData library = GetLibrary();
            return library.RulePackages.Count > 0
                || library.Classes.Count > 0
                || library.Features.Count > 0
                || library.Spells.Count > 0
                || library.Alignments.Count > 0
                || library.EnumLists.Count > 0;
        }

        public void Reload()
        {
            m_loaded = false;
            m_library = new DndRuleContentLibraryData();
            m_lastErrorMessage = string.Empty;
            ClearIndexes();
            LoadIfNeeded();
        }

        public List<string> GetRulePackageOptionLabels()
        {
            List<string> labels = new List<string>();
            IReadOnlyList<DndRulePackageData> packages = RulePackages;
            for (int index = 0; index < packages.Count; index++)
            {
                string label = packages[index].GetDisplayLabel();
                if (!string.IsNullOrWhiteSpace(label))
                {
                    labels.Add(label);
                }
            }

            return labels;
        }

        public bool TryGetClass(string classId, out DndClassDefineData classDefine)
        {
            GetLibrary();
            return m_classById.TryGetValue(classId ?? string.Empty, out classDefine);
        }

        public bool TryGetFeature(string featureId, out DndFeatureDefineData feature)
        {
            GetLibrary();
            return m_featureById.TryGetValue(featureId ?? string.Empty, out feature);
        }

        public bool TryGetFeatureEffect(string effectId, out DndFeatureEffectData effect)
        {
            GetLibrary();
            return m_featureEffectById.TryGetValue(effectId ?? string.Empty, out effect);
        }

        public bool TryGetFeatureEffectCondition(string conditionId, out DndFeatureEffectConditionData condition)
        {
            GetLibrary();
            return m_featureEffectConditionById.TryGetValue(conditionId ?? string.Empty, out condition);
        }

        public bool TryGetSpell(string spellId, out DndSpellDefineData spell)
        {
            GetLibrary();
            return m_spellById.TryGetValue(spellId ?? string.Empty, out spell);
        }

        public bool TryGetFeat(string featId, out DndFeatDefineData feat)
        {
            GetLibrary();
            return m_featById.TryGetValue(featId ?? string.Empty, out feat);
        }

        public bool TryGetItem(string itemId, out DndItemDefineData item)
        {
            GetLibrary();
            return m_itemById.TryGetValue(itemId ?? string.Empty, out item);
        }

        public bool TryGetTool(string toolId, out DndToolDefineData tool)
        {
            GetLibrary();
            return m_toolById.TryGetValue(toolId ?? string.Empty, out tool);
        }

        public bool TryGetLanguage(string languageId, out DndLanguageDefineData language)
        {
            GetLibrary();
            return m_languageById.TryGetValue(languageId ?? string.Empty, out language);
        }

        public bool TryGetText(string textKey, out string text)
        {
            GetLibrary();
            text = string.Empty;
            if (!m_textLocalizeByKey.TryGetValue(textKey ?? string.Empty, out DndTextLocalizeData row) || row == null)
            {
                return false;
            }

            text = row.Text ?? string.Empty;
            return !string.IsNullOrWhiteSpace(text);
        }

        public bool TryGetChoiceGroup(string choiceGroupId, out DndChoiceGroupData choiceGroup)
        {
            GetLibrary();
            string normalized = choiceGroupId?.Trim() ?? string.Empty;
            if (m_choiceGroupById.TryGetValue(normalized, out choiceGroup))
            {
                return true;
            }

            choiceGroup = CreateBuiltInChoiceGroup(normalized);
            return choiceGroup != null;
        }

        public IReadOnlyList<DndChoiceOptionData> GetChoiceOptions(string choiceGroupId)
        {
            GetLibrary();
            string key = choiceGroupId?.Trim() ?? string.Empty;
            if (m_choiceOptionsByGroupId.TryGetValue(key, out List<DndChoiceOptionData> options) && options.Count > 0)
            {
                return options;
            }

            return TryBuildDynamicChoiceOptions(key, out List<DndChoiceOptionData> dynamicOptions)
                ? dynamicOptions
                : Array.Empty<DndChoiceOptionData>();
        }

        public bool TryGetSkill(string skillId, out DndSkillDefineData skill)
        {
            GetLibrary();
            return m_skillById.TryGetValue(skillId ?? string.Empty, out skill);
        }

        public bool TryGetRaceMain(string mainRaceId, out DndRaceMainDefineData raceMain)
        {
            GetLibrary();
            return m_raceMainById.TryGetValue(mainRaceId ?? string.Empty, out raceMain);
        }

        public bool TryGetRaceSub(string subRaceId, out DndRaceSubDefineData raceSub)
        {
            GetLibrary();
            return m_raceSubById.TryGetValue(subRaceId ?? string.Empty, out raceSub);
        }

        public bool TryGetClassLevelProgression(string classId, int level, out DndLevelProgressionData progression)
        {
            progression = null;
            GetLibrary();

            if (!m_classProgressionsByClassId.TryGetValue(classId ?? string.Empty, out List<DndLevelProgressionData> progressions))
            {
                return false;
            }

            for (int index = 0; index < progressions.Count; index++)
            {
                if (progressions[index].Level == level)
                {
                    progression = progressions[index];
                    return true;
                }
            }

            return false;
        }

        public IReadOnlyList<DndLevelProgressionData> GetClassProgressions(string classId)
        {
            GetLibrary();
            return m_classProgressionsByClassId.TryGetValue(classId ?? string.Empty, out List<DndLevelProgressionData> progressions)
                ? progressions
                : Array.Empty<DndLevelProgressionData>();
        }

        public IReadOnlyList<DndSubclassLevelProgressionData> GetSubclassProgressions(string subclassId)
        {
            GetLibrary();
            return m_subclassProgressionsBySubclassId.TryGetValue(subclassId ?? string.Empty, out List<DndSubclassLevelProgressionData> progressions)
                ? progressions
                : Array.Empty<DndSubclassLevelProgressionData>();
        }

        public IReadOnlyList<DndClassSpellListData> GetClassSpellList(string classId)
        {
            GetLibrary();
            return m_classSpellListsByClassId.TryGetValue(classId ?? string.Empty, out List<DndClassSpellListData> spellList)
                ? spellList
                : Array.Empty<DndClassSpellListData>();
        }

        public List<string> GetAlignmentOptionLabels()
        {
            DndRuleContentLibraryData library = GetLibrary();
            List<string> labels = new List<string>();

            for (int index = 0; index < library.Alignments.Count; index++)
            {
                string name = library.Alignments[index].Name;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    labels.Add(name.Trim());
                }
            }

            if (labels.Count > 0)
            {
                return labels;
            }

            for (int index = 0; index < library.EnumLists.Count; index++)
            {
                DndEnumListData item = library.EnumLists[index];
                if (!string.Equals(item.EnumType, "Alignment", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string label = string.IsNullOrWhiteSpace(item.Description) ? item.Value : item.Description;
                if (!string.IsNullOrWhiteSpace(label))
                {
                    labels.Add(label.Trim());
                }
            }

            return labels;
        }

        private DndRuleContentLibraryData GetLibrary()
        {
            LoadIfNeeded();
            return m_library;
        }

        private void LoadIfNeeded()
        {
            if (m_loaded)
            {
                return;
            }

            m_loaded = true;

            if (m_source.TryLoad(out DndRuleContentLibraryData loadedLibrary, out string errorMessage))
            {
                m_library = loadedLibrary ?? new DndRuleContentLibraryData();
                m_lastErrorMessage = string.Empty;
            }
            else
            {
                m_library = new DndRuleContentLibraryData();
                m_lastErrorMessage = errorMessage ?? string.Empty;
            }

            RebuildIndexes();
        }

        private void ClearIndexes()
        {
            m_classById.Clear();
            m_featureById.Clear();
            m_featureEffectById.Clear();
            m_featureEffectConditionById.Clear();
            m_featById.Clear();
            m_choiceGroupById.Clear();
            m_choiceOptionsByGroupId.Clear();
            m_skillById.Clear();
            m_spellById.Clear();
            m_itemById.Clear();
            m_toolById.Clear();
            m_languageById.Clear();
            m_alignmentById.Clear();
            m_textLocalizeByKey.Clear();
            m_raceMainById.Clear();
            m_raceSubById.Clear();
            m_classProgressionsByClassId.Clear();
            m_subclassProgressionsBySubclassId.Clear();
            m_classSpellListsByClassId.Clear();
        }

        private void RebuildIndexes()
        {
            ClearIndexes();

            for (int index = 0; index < m_library.Classes.Count; index++)
            {
                DndClassDefineData classDefine = m_library.Classes[index];
                if (!string.IsNullOrWhiteSpace(classDefine.ClassId))
                {
                    m_classById[classDefine.ClassId] = classDefine;
                }
            }

            for (int index = 0; index < m_library.Features.Count; index++)
            {
                DndFeatureDefineData feature = m_library.Features[index];
                if (!string.IsNullOrWhiteSpace(feature.FeatureId))
                {
                    m_featureById[feature.FeatureId] = feature;
                }
            }

            for (int index = 0; index < m_library.FeatureEffects.Count; index++)
            {
                DndFeatureEffectData effect = m_library.FeatureEffects[index];
                if (!string.IsNullOrWhiteSpace(effect.EffectId))
                {
                    m_featureEffectById[effect.EffectId] = effect;
                }
            }

            for (int index = 0; index < m_library.FeatureEffectConditions.Count; index++)
            {
                DndFeatureEffectConditionData condition = m_library.FeatureEffectConditions[index];
                if (!string.IsNullOrWhiteSpace(condition.ConditionId))
                {
                    m_featureEffectConditionById[condition.ConditionId] = condition;
                }
            }

            for (int index = 0; index < m_library.Feats.Count; index++)
            {
                DndFeatDefineData feat = m_library.Feats[index];
                if (!string.IsNullOrWhiteSpace(feat.FeatId))
                {
                    m_featById[feat.FeatId] = feat;
                }
            }

            for (int index = 0; index < m_library.ChoiceGroups.Count; index++)
            {
                DndChoiceGroupData choiceGroup = m_library.ChoiceGroups[index];
                if (!string.IsNullOrWhiteSpace(choiceGroup.ChoiceGroupId))
                {
                    m_choiceGroupById[choiceGroup.ChoiceGroupId] = choiceGroup;
                }
            }

            for (int index = 0; index < m_library.ChoiceOptions.Count; index++)
            {
                DndChoiceOptionData option = m_library.ChoiceOptions[index];
                if (string.IsNullOrWhiteSpace(option.ChoiceGroupId))
                {
                    continue;
                }

                if (!m_choiceOptionsByGroupId.TryGetValue(option.ChoiceGroupId, out List<DndChoiceOptionData> options))
                {
                    options = new List<DndChoiceOptionData>();
                    m_choiceOptionsByGroupId[option.ChoiceGroupId] = options;
                }

                options.Add(option);
            }

            for (int index = 0; index < m_library.Skills.Count; index++)
            {
                DndSkillDefineData skill = m_library.Skills[index];
                if (!string.IsNullOrWhiteSpace(skill.SkillId))
                {
                    m_skillById[skill.SkillId] = skill;
                }
            }

            for (int index = 0; index < m_library.Spells.Count; index++)
            {
                DndSpellDefineData spell = m_library.Spells[index];
                if (!string.IsNullOrWhiteSpace(spell.SpellId))
                {
                    m_spellById[spell.SpellId] = spell;
                }
            }

            for (int index = 0; index < m_library.Items.Count; index++)
            {
                DndItemDefineData item = m_library.Items[index];
                if (!string.IsNullOrWhiteSpace(item.ItemId))
                {
                    m_itemById[item.ItemId] = item;
                }
            }

            for (int index = 0; index < m_library.Tools.Count; index++)
            {
                DndToolDefineData tool = m_library.Tools[index];
                if (!string.IsNullOrWhiteSpace(tool.ToolId))
                {
                    m_toolById[tool.ToolId] = tool;
                }
            }

            for (int index = 0; index < m_library.Languages.Count; index++)
            {
                DndLanguageDefineData language = m_library.Languages[index];
                if (!string.IsNullOrWhiteSpace(language.LanguageId))
                {
                    m_languageById[language.LanguageId] = language;
                }
            }

            for (int index = 0; index < m_library.Alignments.Count; index++)
            {
                DndAlignmentData alignment = m_library.Alignments[index];
                if (!string.IsNullOrWhiteSpace(alignment.AlignmentId))
                {
                    m_alignmentById[alignment.AlignmentId] = alignment;
                }
            }

            for (int index = 0; index < m_library.RaceMains.Count; index++)
            {
                DndRaceMainDefineData raceMain = m_library.RaceMains[index];
                if (!string.IsNullOrWhiteSpace(raceMain.MainRaceId))
                {
                    m_raceMainById[raceMain.MainRaceId] = raceMain;
                }
            }

            for (int index = 0; index < m_library.RaceSubs.Count; index++)
            {
                DndRaceSubDefineData raceSub = m_library.RaceSubs[index];
                if (!string.IsNullOrWhiteSpace(raceSub.SubRaceId))
                {
                    m_raceSubById[raceSub.SubRaceId] = raceSub;
                }
            }

            for (int index = 0; index < m_library.LevelProgressions.Count; index++)
            {
                DndLevelProgressionData progression = m_library.LevelProgressions[index];
                if (string.IsNullOrWhiteSpace(progression.ClassId)
                    && string.Equals(progression.OwnerType, "Class", StringComparison.OrdinalIgnoreCase))
                {
                    progression.ClassId = progression.OwnerId;
                }

                if (string.IsNullOrWhiteSpace(progression.ClassId))
                {
                    continue;
                }

                if (!m_classProgressionsByClassId.TryGetValue(progression.ClassId, out List<DndLevelProgressionData> list))
                {
                    list = new List<DndLevelProgressionData>();
                    m_classProgressionsByClassId[progression.ClassId] = list;
                }

                list.Add(progression);
            }

            foreach (List<DndLevelProgressionData> progressions in m_classProgressionsByClassId.Values)
            {
                progressions.Sort((left, right) => left.Level.CompareTo(right.Level));
            }

            for (int index = 0; index < m_library.SubclassLevelProgressions.Count; index++)
            {
                DndSubclassLevelProgressionData progression = m_library.SubclassLevelProgressions[index];
                if (string.IsNullOrWhiteSpace(progression.SubclassId))
                {
                    continue;
                }

                if (!m_subclassProgressionsBySubclassId.TryGetValue(progression.SubclassId, out List<DndSubclassLevelProgressionData> list))
                {
                    list = new List<DndSubclassLevelProgressionData>();
                    m_subclassProgressionsBySubclassId[progression.SubclassId] = list;
                }

                list.Add(progression);
            }

            foreach (List<DndSubclassLevelProgressionData> progressions in m_subclassProgressionsBySubclassId.Values)
            {
                progressions.Sort((left, right) => left.Level.CompareTo(right.Level));
            }

            for (int index = 0; index < m_library.ClassSpellLists.Count; index++)
            {
                DndClassSpellListData spell = m_library.ClassSpellLists[index];
                if (string.IsNullOrWhiteSpace(spell.ClassId))
                {
                    continue;
                }

                if (!m_classSpellListsByClassId.TryGetValue(spell.ClassId, out List<DndClassSpellListData> list))
                {
                    list = new List<DndClassSpellListData>();
                    m_classSpellListsByClassId[spell.ClassId] = list;
                }

                list.Add(spell);
            }

            foreach (List<DndClassSpellListData> spellList in m_classSpellListsByClassId.Values)
            {
                spellList.Sort((left, right) =>
                {
                    int levelComparison = left.MinClassLevel.CompareTo(right.MinClassLevel);
                    return levelComparison != 0 ? levelComparison : string.Compare(left.SpellId, right.SpellId, StringComparison.OrdinalIgnoreCase);
                });
            }

            for (int index = 0; index < m_library.TextLocalizations.Count; index++)
            {
                DndTextLocalizeData row = m_library.TextLocalizations[index];
                if (!string.IsNullOrWhiteSpace(row.TextKey))
                {
                    m_textLocalizeByKey[row.TextKey] = row;
                }
            }
        }

        private bool TryBuildDynamicChoiceOptions(string choiceGroupId, out List<DndChoiceOptionData> options)
        {
            options = null;
            if (string.IsNullOrWhiteSpace(choiceGroupId) || !TryGetChoiceGroup(choiceGroupId, out DndChoiceGroupData choiceGroup))
            {
                return false;
            }

            if (TryGetClassSkillListFilter(choiceGroup.OptionFilter, out string classId))
            {
                options = BuildClassSkillChoiceOptions(choiceGroupId, classId);
                return options.Count > 0;
            }

            if (TryGetToolCategoryFilter(choiceGroup.OptionFilter, out List<string> toolCategories))
            {
                options = BuildToolChoiceOptions(choiceGroupId, toolCategories);
                return options.Count > 0;
            }

            if (IsLanguageChoiceFilter(choiceGroup.OptionFilter))
            {
                options = BuildLanguageChoiceOptions(choiceGroupId);
                return options.Count > 0;
            }

            if (IsWizardCantripChoiceFilter(choiceGroup.OptionFilter))
            {
                options = BuildWizardCantripChoiceOptions(choiceGroupId);
                return options.Count > 0;
            }

            if (IsAbilityChoiceFilter(choiceGroup.OptionFilter))
            {
                options = BuildAbilityChoiceOptions(choiceGroupId);
                return options.Count > 0;
            }

            if (IsFeatChoiceFilter(choiceGroup.OptionFilter))
            {
                options = BuildFeatChoiceOptions(choiceGroupId);
                return options.Count > 0;
            }

            if (!IsSkillChoiceFilter(choiceGroup.OptionFilter))
            {
                return false;
            }

            options = new List<DndChoiceOptionData>();
            for (int index = 0; index < m_library.Skills.Count; index++)
            {
                DndSkillDefineData skill = m_library.Skills[index];
                if (string.IsNullOrWhiteSpace(skill.SkillId))
                {
                    continue;
                }

                options.Add(new DndChoiceOptionData
                {
                    ChoiceGroupId = choiceGroupId,
                    OptionId = skill.SkillId,
                    Name = string.IsNullOrWhiteSpace(skill.Name) ? skill.SkillId : skill.Name,
                    Description = string.IsNullOrWhiteSpace(skill.AbilityId)
                        ? skill.Description
                        : $"{skill.AbilityId}: {skill.Description}"
                });
            }

            return options.Count > 0;
        }

        private static DndChoiceGroupData CreateBuiltInChoiceGroup(string choiceGroupId)
        {
            if (string.IsNullOrWhiteSpace(choiceGroupId))
            {
                return null;
            }

            if (string.Equals(choiceGroupId, "choice_asi_attributes", StringComparison.OrdinalIgnoreCase))
            {
                return new DndChoiceGroupData
                {
                    ChoiceGroupId = "choice_asi_attributes",
                    PackageId = "built_in",
                    Name = "\u5C5E\u6027\u503C\u63D0\u5347",
                    ChoiceType = "AbilityScore",
                    MinSelect = 2,
                    MaxSelect = 2,
                    OptionFilter = "AnyAbility",
                    SelectionMode = "Repeatable",
                    ValuePerSelection = 1,
                    MaxValuePerOption = 2,
                    TargetValueCap = 20,
                    UiMode = "AbilityStepper",
                    Description = "\u9009\u62E9\u4E24\u6B21\u5C5E\u6027\u503C\u63D0\u5347\uFF0C\u53EF\u540C\u4E00\u5C5E\u6027\u63D0\u53472\u70B9\u6216\u4E24\u9879\u5C5E\u6027\u54041\u70B9\u3002"
                };
            }

            if (string.Equals(choiceGroupId, "choice_feat_any", StringComparison.OrdinalIgnoreCase))
            {
                return new DndChoiceGroupData
                {
                    ChoiceGroupId = "choice_feat_any",
                    PackageId = "built_in",
                    Name = "\u9009\u62E9\u4E13\u957F",
                    ChoiceType = "Feat",
                    MinSelect = 1,
                    MaxSelect = 1,
                    OptionFilter = "AnyFeat",
                    SelectionMode = "Distinct",
                    Description = "\u4ECE\u6240\u6709\u4E13\u957F\u4E2D\u9009\u62E9\u4E00\u9879\u3002"
                };
            }

            return null;
        }

        private static List<DndChoiceOptionData> BuildAbilityChoiceOptions(string choiceGroupId)
        {
            return new List<DndChoiceOptionData>
            {
                new DndChoiceOptionData { ChoiceGroupId = choiceGroupId, OptionId = "Strength", Name = "\u529B\u91CF", Description = "\u63D0\u5347\u529B\u91CF\u5C5E\u6027\u3002" },
                new DndChoiceOptionData { ChoiceGroupId = choiceGroupId, OptionId = "Dexterity", Name = "\u654F\u6377", Description = "\u63D0\u5347\u654F\u6377\u5C5E\u6027\u3002" },
                new DndChoiceOptionData { ChoiceGroupId = choiceGroupId, OptionId = "Constitution", Name = "\u4F53\u8D28", Description = "\u63D0\u5347\u4F53\u8D28\u5C5E\u6027\u3002" },
                new DndChoiceOptionData { ChoiceGroupId = choiceGroupId, OptionId = "Intelligence", Name = "\u667A\u529B", Description = "\u63D0\u5347\u667A\u529B\u5C5E\u6027\u3002" },
                new DndChoiceOptionData { ChoiceGroupId = choiceGroupId, OptionId = "Wisdom", Name = "\u611F\u77E5", Description = "\u63D0\u5347\u611F\u77E5\u5C5E\u6027\u3002" },
                new DndChoiceOptionData { ChoiceGroupId = choiceGroupId, OptionId = "Charisma", Name = "\u9B45\u529B", Description = "\u63D0\u5347\u9B45\u529B\u5C5E\u6027\u3002" }
            };
        }

        private List<DndChoiceOptionData> BuildFeatChoiceOptions(string choiceGroupId)
        {
            List<DndChoiceOptionData> options = new List<DndChoiceOptionData>();
            for (int index = 0; index < m_library.Feats.Count; index++)
            {
                DndFeatDefineData feat = m_library.Feats[index];
                if (feat == null || string.IsNullOrWhiteSpace(feat.FeatId))
                {
                    continue;
                }

                options.Add(new DndChoiceOptionData
                {
                    ChoiceGroupId = choiceGroupId,
                    OptionId = feat.FeatId,
                    Name = string.IsNullOrWhiteSpace(feat.Name) ? feat.FeatId : feat.Name,
                    Description = feat.Description
                });
            }

            return options;
        }

        private List<DndChoiceOptionData> BuildToolChoiceOptions(string choiceGroupId, IReadOnlyList<string> toolCategories)
        {
            List<DndChoiceOptionData> options = new List<DndChoiceOptionData>();
            if (toolCategories == null || toolCategories.Count == 0)
            {
                return options;
            }

            for (int index = 0; index < m_library.Tools.Count; index++)
            {
                DndToolDefineData tool = m_library.Tools[index];
                if (tool == null
                    || string.IsNullOrWhiteSpace(tool.ToolId)
                    || !ContainsToolCategory(toolCategories, tool.ToolCategory))
                {
                    continue;
                }

                options.Add(new DndChoiceOptionData
                {
                    ChoiceGroupId = choiceGroupId,
                    OptionId = tool.ToolId,
                    Name = string.IsNullOrWhiteSpace(tool.Name) ? tool.ToolId : tool.Name,
                    Description = tool.Description
                });
            }

            return options;
        }

        private List<DndChoiceOptionData> BuildLanguageChoiceOptions(string choiceGroupId)
        {
            List<DndChoiceOptionData> options = new List<DndChoiceOptionData>();
            for (int index = 0; index < m_library.Languages.Count; index++)
            {
                DndLanguageDefineData language = m_library.Languages[index];
                if (language == null || string.IsNullOrWhiteSpace(language.LanguageId))
                {
                    continue;
                }

                options.Add(new DndChoiceOptionData
                {
                    ChoiceGroupId = choiceGroupId,
                    OptionId = language.LanguageId,
                    Name = string.IsNullOrWhiteSpace(language.Name) ? language.LanguageId : language.Name,
                    Description = language.Description
                });
            }

            return options;
        }

        private List<DndChoiceOptionData> BuildWizardCantripChoiceOptions(string choiceGroupId)
        {
            List<DndChoiceOptionData> options = new List<DndChoiceOptionData>();
            IReadOnlyList<DndClassSpellListData> spellList = GetClassSpellList("wizard");
            HashSet<string> addedSpellIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int index = 0; index < spellList.Count; index++)
            {
                DndClassSpellListData classSpell = spellList[index];
                if (classSpell == null
                    || string.IsNullOrWhiteSpace(classSpell.SpellId)
                    || !IsWizardCantripSpell(classSpell))
                {
                    continue;
                }

                string spellId = classSpell.SpellId.Trim();
                if (!addedSpellIds.Add(spellId))
                {
                    continue;
                }

                TryGetSpell(spellId, out DndSpellDefineData spell);
                options.Add(new DndChoiceOptionData
                {
                    ChoiceGroupId = choiceGroupId,
                    OptionId = spellId,
                    Name = spell != null && !string.IsNullOrWhiteSpace(spell.Name) ? spell.Name : spellId,
                    Description = spell != null ? spell.Description : string.Empty
                });
            }

            return options;
        }

        private List<DndChoiceOptionData> BuildClassSkillChoiceOptions(string choiceGroupId, string classId)
        {
            List<DndChoiceOptionData> options = new List<DndChoiceOptionData>();
            if (string.IsNullOrWhiteSpace(classId) || !ClassSkillListByClassId.TryGetValue(classId, out string[] skillIds))
            {
                return options;
            }

            for (int index = 0; index < skillIds.Length; index++)
            {
                string skillId = skillIds[index];
                if (string.IsNullOrWhiteSpace(skillId))
                {
                    continue;
                }

                TryGetSkill(skillId, out DndSkillDefineData skill);
                options.Add(new DndChoiceOptionData
                {
                    ChoiceGroupId = choiceGroupId,
                    OptionId = skillId,
                    Name = skill != null && !string.IsNullOrWhiteSpace(skill.Name) ? skill.Name : skillId,
                    Description = skill != null ? skill.Description : string.Empty
                });
            }

            return options;
        }

        private static bool TryGetClassSkillListFilter(string optionFilter, out string classId)
        {
            classId = string.Empty;
            if (string.IsNullOrWhiteSpace(optionFilter))
            {
                return false;
            }

            string normalized = optionFilter.Trim();
            const string suffix = ":classSkillList";
            if (!normalized.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            classId = normalized.Substring(0, normalized.Length - suffix.Length).Trim();
            return !string.IsNullOrWhiteSpace(classId);
        }

        private static bool TryGetToolCategoryFilter(string optionFilter, out List<string> categories)
        {
            categories = null;
            if (string.IsNullOrWhiteSpace(optionFilter))
            {
                return false;
            }

            string normalized = optionFilter.Trim();
            const string prefix = "ToolCategory:";
            if (!normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            categories = new List<string>();
            string value = normalized.Substring(prefix.Length);
            string[] parts = value.Split(new[] { '|', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int index = 0; index < parts.Length; index++)
            {
                string category = parts[index].Trim();
                if (!string.IsNullOrWhiteSpace(category))
                {
                    categories.Add(category);
                }
            }

            return categories.Count > 0;
        }

        private static bool ContainsToolCategory(IReadOnlyList<string> categories, string toolCategory)
        {
            if (categories == null || string.IsNullOrWhiteSpace(toolCategory))
            {
                return false;
            }

            for (int index = 0; index < categories.Count; index++)
            {
                if (string.Equals(categories[index], toolCategory, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsSkillChoiceFilter(string optionFilter)
        {
            if (string.IsNullOrWhiteSpace(optionFilter))
            {
                return false;
            }

            string normalized = optionFilter.Trim();
            return string.Equals(normalized, "TbSkillDefine:all", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "Skill:all", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "AnySkill", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsLanguageChoiceFilter(string optionFilter)
        {
            if (string.IsNullOrWhiteSpace(optionFilter))
            {
                return false;
            }

            string normalized = optionFilter.Trim();
            return string.Equals(normalized, "TbDndLanguageDefine:all", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "Language:all", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "AnyLanguage", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsWizardCantripChoiceFilter(string optionFilter)
        {
            return string.Equals(optionFilter?.Trim(), "WizardCantrip", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAbilityChoiceFilter(string optionFilter)
        {
            string normalized = optionFilter?.Trim() ?? string.Empty;
            return string.Equals(normalized, "TbAbilityDefine:all", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "AbilityScore:all", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "AnyAbility", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsFeatChoiceFilter(string optionFilter)
        {
            string normalized = optionFilter?.Trim() ?? string.Empty;
            return string.Equals(normalized, "TbFeatDefine:all", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "Feat:all", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "AnyFeat", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsWizardCantripSpell(DndClassSpellListData classSpell)
        {
            if (classSpell == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(classSpell.Note) && classSpell.Note.IndexOf("戏法", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (TryGetSpell(classSpell.SpellId, out DndSpellDefineData spell))
            {
                return spell != null && spell.Level == 0;
            }

            return false;
        }
    }
}
