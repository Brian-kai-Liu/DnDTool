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
        private readonly Dictionary<string, DndSpellDefineData> m_spellById = new Dictionary<string, DndSpellDefineData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DndAlignmentData> m_alignmentById = new Dictionary<string, DndAlignmentData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DndRaceMainDefineData> m_raceMainById = new Dictionary<string, DndRaceMainDefineData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<DndClassLevelProgressionData>> m_classProgressionsByClassId = new Dictionary<string, List<DndClassLevelProgressionData>>(StringComparer.OrdinalIgnoreCase);
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

        public IReadOnlyList<DndBackgroundDefineData> Backgrounds => GetLibrary().Backgrounds;

        public IReadOnlyList<DndFeatDefineData> Feats => GetLibrary().Feats;

        public IReadOnlyList<DndSpellDefineData> Spells => GetLibrary().Spells;

        public IReadOnlyList<DndAlignmentData> Alignments => GetLibrary().Alignments;

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

        public bool TryGetSpell(string spellId, out DndSpellDefineData spell)
        {
            GetLibrary();
            return m_spellById.TryGetValue(spellId ?? string.Empty, out spell);
        }

        public bool TryGetRaceMain(string mainRaceId, out DndRaceMainDefineData raceMain)
        {
            GetLibrary();
            return m_raceMainById.TryGetValue(mainRaceId ?? string.Empty, out raceMain);
        }

        public bool TryGetClassLevelProgression(string classId, int level, out DndClassLevelProgressionData progression)
        {
            progression = null;
            GetLibrary();

            if (!m_classProgressionsByClassId.TryGetValue(classId ?? string.Empty, out List<DndClassLevelProgressionData> progressions))
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

        public IReadOnlyList<DndClassLevelProgressionData> GetClassProgressions(string classId)
        {
            GetLibrary();
            return m_classProgressionsByClassId.TryGetValue(classId ?? string.Empty, out List<DndClassLevelProgressionData> progressions)
                ? progressions
                : Array.Empty<DndClassLevelProgressionData>();
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
            m_spellById.Clear();
            m_alignmentById.Clear();
            m_raceMainById.Clear();
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

            for (int index = 0; index < m_library.ClassLevelProgressions.Count; index++)
            {
                DndClassLevelProgressionData progression = m_library.ClassLevelProgressions[index];
                if (string.IsNullOrWhiteSpace(progression.ClassId))
                {
                    continue;
                }

                if (!m_classProgressionsByClassId.TryGetValue(progression.ClassId, out List<DndClassLevelProgressionData> list))
                {
                    list = new List<DndClassLevelProgressionData>();
                    m_classProgressionsByClassId[progression.ClassId] = list;
                }

                list.Add(progression);
            }

            foreach (List<DndClassLevelProgressionData> progressions in m_classProgressionsByClassId.Values)
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
    }
}
