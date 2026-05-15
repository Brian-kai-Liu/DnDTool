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
        private readonly Dictionary<string, DndChoiceGroupData> m_choiceGroupById = new Dictionary<string, DndChoiceGroupData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<DndChoiceOptionData>> m_choiceOptionsByGroupId = new Dictionary<string, List<DndChoiceOptionData>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DndSkillDefineData> m_skillById = new Dictionary<string, DndSkillDefineData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DndSpellDefineData> m_spellById = new Dictionary<string, DndSpellDefineData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DndAlignmentData> m_alignmentById = new Dictionary<string, DndAlignmentData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DndRaceMainDefineData> m_raceMainById = new Dictionary<string, DndRaceMainDefineData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DndRaceSubDefineData> m_raceSubById = new Dictionary<string, DndRaceSubDefineData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<DndLevelProgressionData>> m_classProgressionsByClassId = new Dictionary<string, List<DndLevelProgressionData>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<DndClassSpellListData>> m_classSpellListsByClassId = new Dictionary<string, List<DndClassSpellListData>>(StringComparer.OrdinalIgnoreCase);

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

        public bool TryGetChoiceGroup(string choiceGroupId, out DndChoiceGroupData choiceGroup)
        {
            GetLibrary();
            return m_choiceGroupById.TryGetValue(choiceGroupId ?? string.Empty, out choiceGroup);
        }

        public IReadOnlyList<DndChoiceOptionData> GetChoiceOptions(string choiceGroupId)
        {
            GetLibrary();
            string key = choiceGroupId ?? string.Empty;
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
            m_choiceGroupById.Clear();
            m_choiceOptionsByGroupId.Clear();
            m_skillById.Clear();
            m_spellById.Clear();
            m_alignmentById.Clear();
            m_raceMainById.Clear();
            m_raceSubById.Clear();
            m_classProgressionsByClassId.Clear();
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
        }

        private bool TryBuildDynamicChoiceOptions(string choiceGroupId, out List<DndChoiceOptionData> options)
        {
            options = null;
            if (string.IsNullOrWhiteSpace(choiceGroupId) || !m_choiceGroupById.TryGetValue(choiceGroupId, out DndChoiceGroupData choiceGroup))
            {
                return false;
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
                        : $"{skill.AbilityId}。{skill.Description}"
                });
            }

            return options.Count > 0;
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
    }
}
