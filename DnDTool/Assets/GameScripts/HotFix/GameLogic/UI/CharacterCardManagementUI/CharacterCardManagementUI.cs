using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using TEngine;
using UnityEngine.UI;
using Log = TEngine.Log;

namespace GameLogic
{
    [Window(UILayer.UI, location: "CharacterCardManagementUI", fullScreen: true)]
    internal sealed class CharacterCardManagementUI : UIWindow
    {
        private const int DefaultAbilityScore = 10;
        private const int LevelOneProficiencyBonus = 2;

        private Button m_btnBack = null!;
        private Button m_btnPrevClass = null!;
        private Button m_btnNextClass = null!;
        private Button m_btnPrevRace = null!;
        private Button m_btnNextRace = null!;
        private Button m_btnPrevBackground = null!;
        private Button m_btnNextBackground = null!;
        private Button m_btnPrevFeat = null!;
        private Button m_btnNextFeat = null!;
        private Button m_btnPrevSpell = null!;
        private Button m_btnNextSpell = null!;
        private Button m_btnCreateDraft = null!;
        private TMP_Text m_tmpRuleStatus = null!;
        private TMP_Text m_tmpClassLabel = null!;
        private TMP_Text m_tmpRaceLabel = null!;
        private TMP_Text m_tmpBackgroundLabel = null!;
        private TMP_Text m_tmpFeatLabel = null!;
        private TMP_Text m_tmpSpellLabel = null!;
        private TMP_Text m_tmpClassValue = null!;
        private TMP_Text m_tmpRaceValue = null!;
        private TMP_Text m_tmpBackgroundValue = null!;
        private TMP_Text m_tmpFeatValue = null!;
        private TMP_Text m_tmpSpellValue = null!;
        private TMP_Text m_tmpSelectionDetail = null!;
        private TMP_Text m_tmpCharacterPreview = null!;

        private readonly List<DndClassDefineData> m_classes = new List<DndClassDefineData>();
        private readonly List<DndRaceDefineData> m_races = new List<DndRaceDefineData>();
        private readonly List<DndBackgroundDefineData> m_backgrounds = new List<DndBackgroundDefineData>();
        private readonly List<DndFeatDefineData> m_feats = new List<DndFeatDefineData>();
        private readonly List<DndSpellDefineData> m_spells = new List<DndSpellDefineData>();

        private int m_selectedClassIndex;
        private int m_selectedRaceIndex;
        private int m_selectedBackgroundIndex;
        private int m_selectedFeatIndex;
        private int m_selectedSpellIndex;

        protected override void ScriptGenerator()
        {
            m_btnBack = FindChildComponent<Button>("m_panelTopBar/m_btnBack");
            m_btnPrevClass = FindChildComponent<Button>("m_panelLeft/m_rowClass/m_btnPrevClass");
            m_btnNextClass = FindChildComponent<Button>("m_panelLeft/m_rowClass/m_btnNextClass");
            m_btnPrevRace = FindChildComponent<Button>("m_panelLeft/m_rowRace/m_btnPrevRace");
            m_btnNextRace = FindChildComponent<Button>("m_panelLeft/m_rowRace/m_btnNextRace");
            m_btnPrevBackground = FindChildComponent<Button>("m_panelLeft/m_rowBackground/m_btnPrevBackground");
            m_btnNextBackground = FindChildComponent<Button>("m_panelLeft/m_rowBackground/m_btnNextBackground");
            m_btnPrevFeat = FindChildComponent<Button>("m_panelLeft/m_rowFeat/m_btnPrevFeat");
            m_btnNextFeat = FindChildComponent<Button>("m_panelLeft/m_rowFeat/m_btnNextFeat");
            m_btnPrevSpell = FindChildComponent<Button>("m_panelLeft/m_rowSpell/m_btnPrevSpell");
            m_btnNextSpell = FindChildComponent<Button>("m_panelLeft/m_rowSpell/m_btnNextSpell");
            m_btnCreateDraft = FindChildComponent<Button>("m_panelLeft/m_btnCreateDraft");
            m_tmpRuleStatus = FindChildComponent<TMP_Text>("m_panelLeft/m_tmpRuleStatus");
            m_tmpClassLabel = FindChildComponent<TMP_Text>("m_panelLeft/m_rowClass/Label");
            m_tmpRaceLabel = FindChildComponent<TMP_Text>("m_panelLeft/m_rowRace/Label");
            m_tmpBackgroundLabel = FindChildComponent<TMP_Text>("m_panelLeft/m_rowBackground/Label");
            m_tmpFeatLabel = FindChildComponent<TMP_Text>("m_panelLeft/m_rowFeat/Label");
            m_tmpSpellLabel = FindChildComponent<TMP_Text>("m_panelLeft/m_rowSpell/Label");
            m_tmpClassValue = FindChildComponent<TMP_Text>("m_panelLeft/m_rowClass/m_tmpClassValue");
            m_tmpRaceValue = FindChildComponent<TMP_Text>("m_panelLeft/m_rowRace/m_tmpRaceValue");
            m_tmpBackgroundValue = FindChildComponent<TMP_Text>("m_panelLeft/m_rowBackground/m_tmpBackgroundValue");
            m_tmpFeatValue = FindChildComponent<TMP_Text>("m_panelLeft/m_rowFeat/m_tmpFeatValue");
            m_tmpSpellValue = FindChildComponent<TMP_Text>("m_panelLeft/m_rowSpell/m_tmpSpellValue");
            m_tmpSelectionDetail = FindChildComponent<TMP_Text>("m_panelRight/m_tmpSelectionDetail");
            m_tmpCharacterPreview = FindChildComponent<TMP_Text>("m_panelRight/m_tmpCharacterPreview");

            ApplyCreationStepLabels();

            BindButton(m_btnBack, OnClickBack);
            BindButton(m_btnPrevClass, () => MoveSelection(m_classes.Count, ref m_selectedClassIndex, -1));
            BindButton(m_btnNextClass, () => MoveSelection(m_classes.Count, ref m_selectedClassIndex, 1));
            BindButton(m_btnPrevRace, () => MoveSelection(m_races.Count, ref m_selectedRaceIndex, -1));
            BindButton(m_btnNextRace, () => MoveSelection(m_races.Count, ref m_selectedRaceIndex, 1));
            BindButton(m_btnPrevBackground, () => MoveSelection(m_backgrounds.Count, ref m_selectedBackgroundIndex, -1));
            BindButton(m_btnNextBackground, () => MoveSelection(m_backgrounds.Count, ref m_selectedBackgroundIndex, 1));
            BindButton(m_btnPrevFeat, () => MoveSelection(m_feats.Count, ref m_selectedFeatIndex, -1));
            BindButton(m_btnNextFeat, () => MoveSelection(m_feats.Count, ref m_selectedFeatIndex, 1));
            BindButton(m_btnPrevSpell, () => MoveSelection(m_spells.Count, ref m_selectedSpellIndex, -1));
            BindButton(m_btnNextSpell, () => MoveSelection(m_spells.Count, ref m_selectedSpellIndex, 1));
            BindButton(m_btnCreateDraft, OnClickCreateDraft);
        }

        protected override void OnRefresh()
        {
            LoadRuleContent();
            ClampSelectionIndexes();
            RefreshView();
        }

        private static void BindButton(Button button, Action action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action?.Invoke());
        }

        private void OnClickBack()
        {
            GameModule.UI.CloseUI<CharacterCardManagementUI>();
            GameModule.UI.ShowUIAsync<HomeUI>();
        }

        private void OnClickCreateDraft()
        {
            RefreshView();
            Log.Info("角色卡管理：已生成当前选择的角色草稿预览，正式保存功能将在角色存档结构完成后接入。");
        }

        private void LoadRuleContent()
        {
            DndRuleContentService service = DndRuleContentService.Instance;

            m_classes.Clear();
            m_races.Clear();
            m_backgrounds.Clear();
            m_feats.Clear();
            m_spells.Clear();

            AddRange(m_classes, service.Classes);
            AddRange(m_races, service.Races);
            AddRange(m_backgrounds, service.Backgrounds);
            AddRange(m_feats, service.Feats);
            AddRange(m_spells, service.Spells);
        }

        private static void AddRange<T>(List<T> target, IReadOnlyList<T> source)
        {
            if (source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                target.Add(source[index]);
            }
        }

        private void MoveSelection(int count, ref int selectedIndex, int direction)
        {
            if (count <= 0)
            {
                selectedIndex = 0;
                RefreshView();
                return;
            }

            selectedIndex = (selectedIndex + direction + count) % count;
            RefreshView();
        }

        private void ClampSelectionIndexes()
        {
            m_selectedClassIndex = ClampIndex(m_selectedClassIndex, m_classes.Count);
            m_selectedRaceIndex = ClampIndex(m_selectedRaceIndex, m_races.Count);
            m_selectedBackgroundIndex = ClampIndex(m_selectedBackgroundIndex, m_backgrounds.Count);
            m_selectedFeatIndex = ClampIndex(m_selectedFeatIndex, m_feats.Count);
            m_selectedSpellIndex = ClampIndex(m_selectedSpellIndex, m_spells.Count);
        }

        private static int ClampIndex(int index, int count)
        {
            if (count <= 0)
            {
                return 0;
            }

            if (index < 0)
            {
                return 0;
            }

            return index >= count ? count - 1 : index;
        }

        private void RefreshView()
        {
            RefreshRuleStatus();
            SetText(m_tmpRaceValue, GetRaceLabel());
            SetText(m_tmpClassValue, GetClassLabel());
            SetText(m_tmpBackgroundValue, GetBackgroundLabel());
            SetText(m_tmpFeatValue, GetFeatLabel());
            SetText(m_tmpSpellValue, GetSpellLabel());
            SetText(m_tmpSelectionDetail, BuildSelectionDetail());
            SetText(m_tmpCharacterPreview, BuildCharacterPreview());
        }

        private void ApplyCreationStepLabels()
        {
            SetText(m_tmpRaceLabel, "1. 种族");
            SetText(m_tmpClassLabel, "2. 职业");
            SetText(m_tmpBackgroundLabel, "3. 背景");
            SetText(m_tmpFeatLabel, "4. 专长");
            SetText(m_tmpSpellLabel, "5. 法术");
        }

        private void RefreshRuleStatus()
        {
            DndRuleContentService service = DndRuleContentService.Instance;
            if (service.HasLoadedContent())
            {
                SetText(m_tmpRuleStatus, $"规则内容已读取：职业 {m_classes.Count}，种族 {m_races.Count}，背景 {m_backgrounds.Count}，专长 {m_feats.Count}，法术 {m_spells.Count}");
                return;
            }

            string error = string.IsNullOrWhiteSpace(service.LastErrorMessage)
                ? "尚未读取到角色规则表数据。请先通过 Luban 生成配置。"
                : service.LastErrorMessage;
            SetText(m_tmpRuleStatus, $"规则内容未就绪：{error}");
        }

        private string GetClassLabel()
        {
            return TryGetSelected(m_classes, m_selectedClassIndex, out DndClassDefineData data)
                ? $"{data.Name}  d{data.HitDie}"
                : "未选择职业";
        }

        private string GetRaceLabel()
        {
            return TryGetSelected(m_races, m_selectedRaceIndex, out DndRaceDefineData data)
                ? $"{data.Name}  {data.Size}"
                : "未选择种族";
        }

        private string GetBackgroundLabel()
        {
            return TryGetSelected(m_backgrounds, m_selectedBackgroundIndex, out DndBackgroundDefineData data)
                ? data.Name
                : "未选择背景";
        }

        private string GetFeatLabel()
        {
            return TryGetSelected(m_feats, m_selectedFeatIndex, out DndFeatDefineData data)
                ? data.Name
                : "可暂不选择专长";
        }

        private string GetSpellLabel()
        {
            return TryGetSelected(m_spells, m_selectedSpellIndex, out DndSpellDefineData data)
                ? $"{data.Name}  {FormatSpellLevel(data.Level)}"
                : "可暂不选择法术";
        }

        private string BuildSelectionDetail()
        {
            StringBuilder builder = new StringBuilder(768);

            if (TryGetSelected(m_races, m_selectedRaceIndex, out DndRaceDefineData raceData))
            {
                AppendSection(builder, "种族", raceData.Name);
                AppendLine(builder, "体型/速度", $"{raceData.Size} / {raceData.Speed}");
                AppendLine(builder, "语言", FormatList(raceData.LanguageIds));
                AppendLine(builder, "属性加值", BuildRaceAbilityBonusSummary(raceData));
                AppendLine(builder, "种族特性", BuildFeatureSummary(raceData.FeatureIds));
                AppendLine(builder, "说明", raceData.Description);
            }

            if (TryGetSelected(m_classes, m_selectedClassIndex, out DndClassDefineData classData))
            {
                AppendSection(builder, "职业", classData.Name);
                AppendLine(builder, "主属性", $"{FormatList(classData.PrimaryAbilityIds)} / {classData.PrimaryAbilityMode}");
                AppendLine(builder, "豁免熟练", FormatList(classData.SavingThrowProficiencies));
                AppendLine(builder, "护甲熟练", FormatList(classData.ArmorProficiencies));
                AppendLine(builder, "武器熟练", FormatList(classData.WeaponProficiencies));
                AppendLine(builder, "说明", classData.Description);
            }

            if (TryGetSelected(m_backgrounds, m_selectedBackgroundIndex, out DndBackgroundDefineData backgroundData))
            {
                AppendSection(builder, "背景", backgroundData.Name);
                AppendLine(builder, "技能熟练", FormatList(backgroundData.SkillProficiencies));
                AppendLine(builder, "工具熟练", FormatList(backgroundData.ToolProficiencies));
                AppendLine(builder, "说明", backgroundData.Description);
            }

            if (TryGetSelected(m_feats, m_selectedFeatIndex, out DndFeatDefineData featData))
            {
                AppendSection(builder, "专长", featData.Name);
                AppendLine(builder, "前置条件", FormatList(featData.PrerequisiteIds));
                AppendLine(builder, "说明", featData.Description);
            }

            if (TryGetSelected(m_spells, m_selectedSpellIndex, out DndSpellDefineData spellData))
            {
                AppendSection(builder, "法术", spellData.Name);
                AppendLine(builder, "环级/学派", $"{FormatSpellLevel(spellData.Level)} / {spellData.School}");
                AppendLine(builder, "施法/距离/持续", $"{spellData.CastingTime} / {spellData.Range} / {spellData.Duration}");
                AppendLine(builder, "说明", spellData.Description);
            }

            return builder.Length == 0
                ? "当前没有可展示的规则详情。完成 Luban 转表后，这里会按种族、职业、背景、专长和法术的建卡顺序展示录入内容。"
                : builder.ToString();
        }

        private string BuildCharacterPreview()
        {
            StringBuilder builder = new StringBuilder(768);
            builder.AppendLine("当前角色详情预览");
            builder.AppendLine("名称：未命名角色");
            builder.AppendLine("阵营：待选择");
            builder.AppendLine($"种族：{GetRaceLabel()}");
            builder.AppendLine($"职业：{GetClassLabel()}");
            builder.AppendLine($"背景：{GetBackgroundLabel()}");
            builder.AppendLine("等级：1");
            builder.AppendLine($"专长：{GetFeatLabel()}");
            builder.AppendLine($"法术：{GetSpellLabel()}");
            builder.AppendLine();
            builder.AppendLine("六维属性");
            builder.Append(BuildAbilityScorePreview());
            builder.AppendLine();
            builder.AppendLine("熟练项与加值");
            builder.Append(BuildProficiencyPreview());
            return builder.ToString();
        }

        private string BuildAbilityScorePreview()
        {
            int[] raceBonuses = new int[6];
            if (TryGetSelected(m_races, m_selectedRaceIndex, out DndRaceDefineData raceData))
            {
                ApplyRaceAbilityBonuses(raceData, raceBonuses);
            }

            StringBuilder builder = new StringBuilder(256);
            AppendAbilityLine(builder, "力量", DefaultAbilityScore + raceBonuses[0], raceBonuses[0]);
            AppendAbilityLine(builder, "敏捷", DefaultAbilityScore + raceBonuses[1], raceBonuses[1]);
            AppendAbilityLine(builder, "体质", DefaultAbilityScore + raceBonuses[2], raceBonuses[2]);
            AppendAbilityLine(builder, "智力", DefaultAbilityScore + raceBonuses[3], raceBonuses[3]);
            AppendAbilityLine(builder, "感知", DefaultAbilityScore + raceBonuses[4], raceBonuses[4]);
            AppendAbilityLine(builder, "魅力", DefaultAbilityScore + raceBonuses[5], raceBonuses[5]);
            return builder.ToString();
        }

        private string BuildProficiencyPreview()
        {
            StringBuilder builder = new StringBuilder(320);
            builder.AppendLine($"熟练加值：+{LevelOneProficiencyBonus}");

            if (TryGetSelected(m_classes, m_selectedClassIndex, out DndClassDefineData classData))
            {
                AppendLine(builder, "豁免熟练", FormatListWithBonus(classData.SavingThrowProficiencies));
                AppendLine(builder, "护甲熟练", FormatList(classData.ArmorProficiencies));
                AppendLine(builder, "武器熟练", FormatList(classData.WeaponProficiencies));
            }
            else
            {
                AppendLine(builder, "豁免熟练", "未选择职业");
            }

            if (TryGetSelected(m_backgrounds, m_selectedBackgroundIndex, out DndBackgroundDefineData backgroundData))
            {
                AppendLine(builder, "技能熟练", FormatListWithBonus(backgroundData.SkillProficiencies));
                AppendLine(builder, "工具熟练", FormatList(backgroundData.ToolProficiencies));
            }
            else
            {
                AppendLine(builder, "技能熟练", "未选择背景");
            }

            if (TryGetSelected(m_races, m_selectedRaceIndex, out DndRaceDefineData raceData))
            {
                AppendLine(builder, "语言", FormatList(raceData.LanguageIds));
            }

            return builder.ToString();
        }

        private string BuildRaceAbilityBonusSummary(DndRaceDefineData raceData)
        {
            int[] raceBonuses = new int[6];
            ApplyRaceAbilityBonuses(raceData, raceBonuses);

            List<string> summaries = new List<string>();
            AddAbilityBonusSummary(summaries, "力量", raceBonuses[0]);
            AddAbilityBonusSummary(summaries, "敏捷", raceBonuses[1]);
            AddAbilityBonusSummary(summaries, "体质", raceBonuses[2]);
            AddAbilityBonusSummary(summaries, "智力", raceBonuses[3]);
            AddAbilityBonusSummary(summaries, "感知", raceBonuses[4]);
            AddAbilityBonusSummary(summaries, "魅力", raceBonuses[5]);
            return summaries.Count > 0 ? string.Join(" / ", summaries) : "当前种族未录入可自动解析的属性加值";
        }

        private string BuildFeatureSummary(IReadOnlyList<string> featureIds)
        {
            if (featureIds == null || featureIds.Count == 0)
            {
                return "无";
            }

            DndRuleContentService service = DndRuleContentService.Instance;
            List<string> labels = new List<string>();
            for (int index = 0; index < featureIds.Count; index++)
            {
                string featureId = featureIds[index];
                if (service.TryGetFeature(featureId, out DndFeatureDefineData feature))
                {
                    labels.Add(string.IsNullOrWhiteSpace(feature.Name) ? feature.FeatureId : feature.Name);
                }
                else if (!string.IsNullOrWhiteSpace(featureId))
                {
                    labels.Add(featureId);
                }
            }

            return labels.Count > 0 ? string.Join(" / ", labels) : "无";
        }

        private void ApplyRaceAbilityBonuses(DndRaceDefineData raceData, int[] raceBonuses)
        {
            if (raceData == null || raceBonuses == null || raceBonuses.Length < 6)
            {
                return;
            }

            DndRuleContentService service = DndRuleContentService.Instance;
            for (int featureIndex = 0; featureIndex < raceData.FeatureIds.Count; featureIndex++)
            {
                if (!service.TryGetFeature(raceData.FeatureIds[featureIndex], out DndFeatureDefineData feature))
                {
                    continue;
                }

                for (int effectIndex = 0; effectIndex < feature.EffectIds.Count; effectIndex++)
                {
                    if (!service.TryGetFeatureEffect(feature.EffectIds[effectIndex], out DndFeatureEffectData effect))
                    {
                        continue;
                    }

                    if (!IsAbilityBonusEffect(effect) || !TryGetAbilityIndex(effect.Target, out int abilityIndex) || !TryParseSignedInt(effect.Value, out int value))
                    {
                        continue;
                    }

                    raceBonuses[abilityIndex] += value;
                }
            }
        }

        private static bool TryGetSelected<T>(List<T> list, int index, out T value)
        {
            if (list != null && index >= 0 && index < list.Count)
            {
                value = list[index];
                return true;
            }

            value = default;
            return false;
        }

        private static void AppendSection(StringBuilder builder, string title, string value)
        {
            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.AppendLine($"[{title}] {value}");
        }

        private static void AppendLine(StringBuilder builder, string label, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                value = "无";
            }

            builder.AppendLine($"{label}：{value}");
        }

        private static void AppendAbilityLine(StringBuilder builder, string label, int score, int raceBonus)
        {
            string bonusText = raceBonus == 0 ? string.Empty : $"，种族{FormatSignedNumber(raceBonus)}";
            builder.AppendLine($"{label}：{score}（调整值{FormatSignedNumber(CalculateAbilityModifier(score))}{bonusText}）");
        }

        private static void AddAbilityBonusSummary(List<string> summaries, string label, int bonus)
        {
            if (bonus != 0)
            {
                summaries.Add($"{label}{FormatSignedNumber(bonus)}");
            }
        }

        private static int CalculateAbilityModifier(int score)
        {
            return (int)Math.Floor((score - 10) / 2f);
        }

        private static string FormatSignedNumber(int value)
        {
            return value >= 0 ? $"+{value}" : value.ToString();
        }

        private static string FormatList(IReadOnlyList<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return "无";
            }

            return string.Join(" / ", values);
        }

        private static string FormatListWithBonus(IReadOnlyList<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return "无";
            }

            List<string> labels = new List<string>();
            for (int index = 0; index < values.Count; index++)
            {
                if (!string.IsNullOrWhiteSpace(values[index]))
                {
                    labels.Add($"{values[index]} +{LevelOneProficiencyBonus}");
                }
            }

            return labels.Count > 0 ? string.Join(" / ", labels) : "无";
        }

        private static bool IsAbilityBonusEffect(DndFeatureEffectData effect)
        {
            string effectType = NormalizeKey(effect?.EffectType);
            return effectType.Contains("abilitybonus")
                || effectType.Contains("abilityscorebonus")
                || effectType.Contains("attributebonus")
                || effectType.Contains("attributescorebonus")
                || effectType.Contains("属性加值")
                || effectType.Contains("属性提升");
        }

        private static bool TryGetAbilityIndex(string abilityId, out int index)
        {
            string normalized = NormalizeKey(abilityId);
            switch (normalized)
            {
                case "str":
                case "strength":
                case "力量":
                    index = 0;
                    return true;
                case "dex":
                case "dexterity":
                case "敏捷":
                    index = 1;
                    return true;
                case "con":
                case "constitution":
                case "体质":
                    index = 2;
                    return true;
                case "int":
                case "intelligence":
                case "智力":
                    index = 3;
                    return true;
                case "wis":
                case "wisdom":
                case "感知":
                    index = 4;
                    return true;
                case "cha":
                case "charisma":
                case "魅力":
                    index = 5;
                    return true;
                default:
                    index = -1;
                    return false;
            }
        }

        private static bool TryParseSignedInt(string value, out int result)
        {
            result = 0;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string text = value.Trim().Replace("+", string.Empty);
            return int.TryParse(text, out result);
        }

        private static string NormalizeKey(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Replace("_", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty).ToLowerInvariant();
        }

        private static string FormatSpellLevel(int level)
        {
            return level <= 0 ? "戏法" : $"{level}环";
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value ?? string.Empty;
            }
        }
    }
}
