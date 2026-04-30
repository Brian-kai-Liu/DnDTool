using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using TEngine;
using UnityEngine;
using UnityEngine.UI;
using Log = TEngine.Log;

namespace GameLogic
{
    internal enum CharacterCardManagementMode
    {
        List,
        Editor
    }

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
        private TMP_Text m_tmpTitle = null!;
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
        private TMP_Text m_tmpPrevClassText = null!;
        private TMP_Text m_tmpNextClassText = null!;
        private TMP_Text m_tmpPrevRaceText = null!;
        private TMP_Text m_tmpNextRaceText = null!;
        private TMP_Text m_tmpPrevBackgroundText = null!;
        private TMP_Text m_tmpNextBackgroundText = null!;
        private TMP_Text m_tmpPrevFeatText = null!;
        private TMP_Text m_tmpNextFeatText = null!;
        private TMP_Text m_tmpPrevSpellText = null!;
        private TMP_Text m_tmpNextSpellText = null!;
        private TMP_Text m_tmpCreateDraftText = null!;
        private GameObject m_goCardListRoot = null!;
        private RectTransform m_rectCardContent = null!;
        private GameObject m_goCharacterCardTemplate = null!;
        private Button m_btnDeleteSelected = null!;
        private TMP_Text m_tmpDeleteSelectedText = null!;

        private readonly List<DndClassDefineData> m_classes = new List<DndClassDefineData>();
        private readonly List<DndRaceDefineData> m_races = new List<DndRaceDefineData>();
        private readonly List<DndBackgroundDefineData> m_backgrounds = new List<DndBackgroundDefineData>();
        private readonly List<DndFeatDefineData> m_feats = new List<DndFeatDefineData>();
        private readonly List<DndSpellDefineData> m_spells = new List<DndSpellDefineData>();
        private readonly List<CharacterCardDraftSaveData> m_characterCards = new List<CharacterCardDraftSaveData>();

        private TMP_Text[] m_rowLabels = Array.Empty<TMP_Text>();
        private TMP_Text[] m_rowValues = Array.Empty<TMP_Text>();
        private Button[] m_rowLeftButtons = Array.Empty<Button>();
        private Button[] m_rowRightButtons = Array.Empty<Button>();
        private TMP_Text[] m_rowLeftButtonTexts = Array.Empty<TMP_Text>();
        private TMP_Text[] m_rowRightButtonTexts = Array.Empty<TMP_Text>();
        private readonly List<CharacterCardListItemView> m_cardViews = new List<CharacterCardListItemView>();

        private int m_selectedClassIndex;
        private int m_selectedRaceIndex;
        private int m_selectedBackgroundIndex;
        private int m_selectedFeatIndex;
        private int m_selectedSpellIndex;
        private int m_selectedListCharacterIndex = -1;
        private CharacterCardManagementMode m_mode = CharacterCardManagementMode.List;
        private CharacterCardDraftSaveData m_currentCharacter;

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
            m_tmpTitle = FindChildComponent<TMP_Text>("m_panelTopBar/m_tmpTitle");
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
            m_tmpPrevClassText = FindChildComponent<TMP_Text>("m_panelLeft/m_rowClass/m_btnPrevClass");
            m_tmpNextClassText = FindChildComponent<TMP_Text>("m_panelLeft/m_rowClass/m_btnNextClass");
            m_tmpPrevRaceText = FindChildComponent<TMP_Text>("m_panelLeft/m_rowRace/m_btnPrevRace");
            m_tmpNextRaceText = FindChildComponent<TMP_Text>("m_panelLeft/m_rowRace/m_btnNextRace");
            m_tmpPrevBackgroundText = FindChildComponent<TMP_Text>("m_panelLeft/m_rowBackground/m_btnPrevBackground");
            m_tmpNextBackgroundText = FindChildComponent<TMP_Text>("m_panelLeft/m_rowBackground/m_btnNextBackground");
            m_tmpPrevFeatText = FindChildComponent<TMP_Text>("m_panelLeft/m_rowFeat/m_btnPrevFeat");
            m_tmpNextFeatText = FindChildComponent<TMP_Text>("m_panelLeft/m_rowFeat/m_btnNextFeat");
            m_tmpPrevSpellText = FindChildComponent<TMP_Text>("m_panelLeft/m_rowSpell/m_btnPrevSpell");
            m_tmpNextSpellText = FindChildComponent<TMP_Text>("m_panelLeft/m_rowSpell/m_btnNextSpell");
            m_tmpCreateDraftText = FindChildComponent<TMP_Text>("m_panelLeft/m_btnCreateDraft");
            m_goCardListRoot = FindChild("m_panelLeft/m_scrollCharacterCardList").gameObject;
            m_rectCardContent = FindChildComponent<RectTransform>("m_panelLeft/m_scrollCharacterCardList/Viewport/m_rectCharacterCardContent");
            m_goCharacterCardTemplate = FindChild("m_panelLeft/m_scrollCharacterCardList/Viewport/m_rectCharacterCardContent/m_itemCharacterCardTemplate").gameObject;
            m_btnDeleteSelected = FindChildComponent<Button>("m_panelLeft/m_btnDeleteSelectedCharacter");
            m_tmpDeleteSelectedText = FindChildComponent<TMP_Text>("m_panelLeft/m_btnDeleteSelectedCharacter");

            m_rowLabels = new[] { m_tmpRaceLabel, m_tmpClassLabel, m_tmpBackgroundLabel, m_tmpFeatLabel, m_tmpSpellLabel };
            m_rowValues = new[] { m_tmpRaceValue, m_tmpClassValue, m_tmpBackgroundValue, m_tmpFeatValue, m_tmpSpellValue };
            m_rowLeftButtons = new[] { m_btnPrevRace, m_btnPrevClass, m_btnPrevBackground, m_btnPrevFeat, m_btnPrevSpell };
            m_rowRightButtons = new[] { m_btnNextRace, m_btnNextClass, m_btnNextBackground, m_btnNextFeat, m_btnNextSpell };
            m_rowLeftButtonTexts = new[] { m_tmpPrevRaceText, m_tmpPrevClassText, m_tmpPrevBackgroundText, m_tmpPrevFeatText, m_tmpPrevSpellText };
            m_rowRightButtonTexts = new[] { m_tmpNextRaceText, m_tmpNextClassText, m_tmpNextBackgroundText, m_tmpNextFeatText, m_tmpNextSpellText };

            BindButton(m_btnBack, OnClickBack);
            if (m_goCharacterCardTemplate != null)
            {
                m_goCharacterCardTemplate.SetActive(false);
            }

            SetCharacterCardListVisible(false);
        }

        protected override void OnRefresh()
        {
            LoadRuleContent();
            LoadCharacterCards();
            ClampSelectionIndexes();
            ShowCharacterList();
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
            if (m_mode == CharacterCardManagementMode.Editor)
            {
                SaveCurrentCharacterDraft();
                ShowCharacterList();
                return;
            }

            CloseToHome();
        }

        private void OnClickCreateDraft()
        {
            SaveCurrentCharacterDraft();
            RefreshView();
            Log.Info("角色卡管理：已保存当前角色草稿。");
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

        private void LoadCharacterCards()
        {
            m_characterCards.Clear();
            CharacterCardLibrarySaveData library = CharacterCardLocalRepository.Load();
            if (library?.Characters == null)
            {
                return;
            }

            for (int index = 0; index < library.Characters.Count; index++)
            {
                CharacterCardDraftSaveData character = CharacterCardLocalRepository.Normalize(library.Characters[index]);
                if (!string.IsNullOrWhiteSpace(character.CharacterId))
                {
                    m_characterCards.Add(character);
                }
            }

            m_characterCards.Sort((left, right) => string.Compare(right.UpdatedAt, left.UpdatedAt, StringComparison.OrdinalIgnoreCase));
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

        private void ShowCharacterList()
        {
            m_mode = CharacterCardManagementMode.List;
            m_currentCharacter = null;
            LoadCharacterCards();
            ClampSelectedListCharacterIndex();
            RefreshCharacterListView();
        }

        private void ShowCharacterEditor(CharacterCardDraftSaveData character)
        {
            m_mode = CharacterCardManagementMode.Editor;
            m_currentCharacter = CharacterCardLocalRepository.Normalize(character);
            ApplySelectionsFromCharacter(m_currentCharacter);
            ClampSelectionIndexes();
            RefreshView();
        }

        private void RefreshCharacterListView()
        {
            SetText(m_tmpTitle, "角色卡管理");
            SetText(m_tmpRuleStatus, $"本机角色：{m_characterCards.Count} 个。包含未完成角色，数据保存在当前电脑本地。");
            SetText(m_tmpCreateDraftText, "新建角色");
            BindButton(m_btnCreateDraft, OnClickCreateNewCharacter);
            BindButton(m_btnDeleteSelected, OnClickDeleteSelectedCharacter);
            SetButtonInteractable(m_btnDeleteSelected, HasSelectedCharacter());
            SetCharacterCardListVisible(true);
            SetAllRowsActive(false);

            EnsureCardViewCount(m_characterCards.Count);
            for (int index = 0; index < m_cardViews.Count; index++)
            {
                if (index >= m_characterCards.Count)
                {
                    m_cardViews[index].SetActive(false);
                    continue;
                }

                int capturedIndex = index;
                m_cardViews[index].Bind(
                    m_characterCards[index],
                    BuildCharacterCardClassLine(m_characterCards[index]),
                    BuildCharacterCardStatusLine(m_characterCards[index]),
                    index == m_selectedListCharacterIndex,
                    () => OnClickSelectCharacterCard(capturedIndex));
            }

            SetText(m_tmpSelectionDetail, BuildCharacterListDetail());
            SetText(m_tmpCharacterPreview, BuildCharacterListPreview());
        }

        private void OnClickCreateNewCharacter()
        {
            CharacterCardDraftSaveData character = CharacterCardLocalRepository.CreateDraft();
            ApplyCurrentSelectionsToCharacter(character);
            CharacterCardLocalRepository.Upsert(character);
            ShowCharacterEditor(character);
        }

        private void OnClickSelectCharacterCard(int characterIndex)
        {
            if (characterIndex < 0 || characterIndex >= m_characterCards.Count)
            {
                return;
            }

            m_selectedListCharacterIndex = characterIndex;
            RefreshCharacterListView();
        }

        private void OnClickDeleteSelectedCharacter()
        {
            if (!HasSelectedCharacter())
            {
                return;
            }

            string characterId = m_characterCards[m_selectedListCharacterIndex].CharacterId;
            CharacterCardLocalRepository.Delete(characterId);
            LoadCharacterCards();
            ClampSelectedListCharacterIndex();
            RefreshCharacterListView();
        }

        private void CloseToHome()
        {
            GameModule.UI.CloseUI<CharacterCardManagementUI>();
            GameModule.UI.ShowUIAsync<HomeUI>();
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
            m_mode = CharacterCardManagementMode.Editor;
            SetText(m_tmpTitle, "角色预览/创建");
            SetText(m_tmpCreateDraftText, "保存角色草稿");
            SetCharacterCardListVisible(false);
            ApplyCreationStepLabels();
            ApplyEditorButtonTexts();
            BindEditorButtons();
            SetAllRowsActive(true);
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

        private void ApplyEditorButtonTexts()
        {
            for (int index = 0; index < m_rowLeftButtonTexts.Length; index++)
            {
                SetText(m_rowLeftButtonTexts[index], "<");
            }

            for (int index = 0; index < m_rowRightButtonTexts.Length; index++)
            {
                SetText(m_rowRightButtonTexts[index], ">");
            }
        }

        private void BindEditorButtons()
        {
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
            SetAllRowButtonsInteractable(true);
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

        private string BuildCharacterListItemSummary(CharacterCardDraftSaveData character)
        {
            if (character == null)
            {
                return "空角色";
            }

            string status = character.IsCompleted ? "已完成" : "未完成";
            string characterName = string.IsNullOrWhiteSpace(character.CharacterName) ? "未命名角色" : character.CharacterName;
            string race = FormatSelectedName(character.RaceId, m_races, data => data.RaceId, data => data.Name, "未选种族");
            string className = FormatSelectedName(character.ClassId, m_classes, data => data.ClassId, data => data.Name, "未选职业");
            return $"{characterName} [{status}]  {race} / {className}";
        }

        private string BuildCharacterCardClassLine(CharacterCardDraftSaveData character)
        {
            string className = FormatSelectedName(character?.ClassId, m_classes, data => data.ClassId, data => data.Name, "未选职业");
            int level = character == null ? 1 : Math.Max(1, character.Level);
            return $"{className}  Lv.{level}";
        }

        private string BuildCharacterCardStatusLine(CharacterCardDraftSaveData character)
        {
            if (character == null)
            {
                return "未完成";
            }

            string status = character.IsCompleted ? "已完成" : "未完成";
            string race = FormatSelectedName(character.RaceId, m_races, data => data.RaceId, data => data.Name, "未选种族");
            return $"{status} / {race}";
        }

        private string BuildCharacterListDetail()
        {
            StringBuilder builder = new StringBuilder(512);
            builder.AppendLine("[角色选择]");
            builder.AppendLine("这里展示当前电脑上通过本软件创建的角色草稿和已完成角色。");
            builder.AppendLine("点击左侧角色卡片后，右侧会显示该角色的完整详细信息；点击“删除所选”会移除本地角色记录。");
            builder.AppendLine();
            builder.AppendLine($"存档位置：{CharacterCardLocalRepository.GetSaveFilePath()}");
            return builder.ToString();
        }

        private string BuildCharacterListPreview()
        {
            if (m_characterCards.Count == 0)
            {
                return "当前还没有角色。\n\n点击左侧“新建角色”创建一个未完成角色草稿。";
            }

            if (HasSelectedCharacter())
            {
                return BuildSelectedCharacterDetail(m_characterCards[m_selectedListCharacterIndex]);
            }

            StringBuilder builder = new StringBuilder(768);
            builder.AppendLine("请选择一个角色卡片");
            builder.AppendLine();
            builder.AppendLine("点击左侧角色卡片后，这里会显示该角色的完整详细信息。");
            return builder.ToString();
        }

        private string BuildSelectedCharacterDetail(CharacterCardDraftSaveData character)
        {
            StringBuilder builder = new StringBuilder(1024);
            builder.AppendLine("角色详细信息");
            builder.AppendLine($"名称：{(string.IsNullOrWhiteSpace(character.CharacterName) ? "未命名角色" : character.CharacterName)}");
            builder.AppendLine($"状态：{(character.IsCompleted ? "已完成" : "未完成")}");
            builder.AppendLine($"等级：{Math.Max(1, character.Level)}");
            builder.AppendLine($"阵营：{(string.IsNullOrWhiteSpace(character.Alignment) ? "待选择" : character.Alignment)}");
            builder.AppendLine($"种族：{FormatSelectedName(character.RaceId, m_races, data => data.RaceId, data => data.Name, "未选种族")}");
            builder.AppendLine($"职业：{FormatSelectedName(character.ClassId, m_classes, data => data.ClassId, data => data.Name, "未选职业")}");
            builder.AppendLine($"背景：{FormatSelectedName(character.BackgroundId, m_backgrounds, data => data.BackgroundId, data => data.Name, "未选背景")}");
            builder.AppendLine($"专长：{FormatSelectedName(character.FeatId, m_feats, data => data.FeatId, data => data.Name, "未选专长")}");
            builder.AppendLine($"法术：{FormatSelectedName(character.SpellId, m_spells, data => data.SpellId, data => data.Name, "未选法术")}");
            builder.AppendLine();
            builder.AppendLine($"创建时间：{FormatSavedTime(character.CreatedAt)}");
            builder.AppendLine($"更新时间：{FormatSavedTime(character.UpdatedAt)}");
            builder.AppendLine();
            builder.AppendLine("说明：当前角色卡片已经保存了基础建卡选择。后续接入姓名、阵营、属性分配和预览图编辑后，这里会继续展示完整角色卡。");
            return builder.ToString();
        }

        private bool HasSelectedCharacter()
        {
            return m_selectedListCharacterIndex >= 0 && m_selectedListCharacterIndex < m_characterCards.Count;
        }

        private void EnsureCardViewCount(int count)
        {
            while (m_cardViews.Count < count)
            {
                if (m_goCharacterCardTemplate == null || m_rectCardContent == null)
                {
                    return;
                }

                GameObject itemObject = UnityEngine.Object.Instantiate(m_goCharacterCardTemplate, m_rectCardContent);
                itemObject.name = $"m_itemCharacterCard_{m_cardViews.Count + 1}";
                CharacterCardListItemView cardView = CharacterCardListItemView.BindTemplate(itemObject);
                m_cardViews.Add(cardView);
            }
        }

        private void SetCharacterCardListVisible(bool visible)
        {
            if (m_goCardListRoot != null)
            {
                m_goCardListRoot.SetActive(visible);
            }

            if (m_btnDeleteSelected != null)
            {
                m_btnDeleteSelected.gameObject.SetActive(visible);
            }
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
            builder.AppendLine($"名称：{GetCurrentCharacterName()}");
            builder.AppendLine($"阵营：{GetCurrentAlignment()}");
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

        private string GetCurrentCharacterName()
        {
            return m_currentCharacter == null || string.IsNullOrWhiteSpace(m_currentCharacter.CharacterName)
                ? "未命名角色"
                : m_currentCharacter.CharacterName;
        }

        private string GetCurrentAlignment()
        {
            return m_currentCharacter == null || string.IsNullOrWhiteSpace(m_currentCharacter.Alignment)
                ? "待选择"
                : m_currentCharacter.Alignment;
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

        private void SaveCurrentCharacterDraft()
        {
            if (m_currentCharacter == null)
            {
                return;
            }

            ApplyCurrentSelectionsToCharacter(m_currentCharacter);
            CharacterCardLocalRepository.Upsert(m_currentCharacter);
        }

        private void ApplyCurrentSelectionsToCharacter(CharacterCardDraftSaveData character)
        {
            if (character == null)
            {
                return;
            }

            character.RaceId = TryGetSelected(m_races, m_selectedRaceIndex, out DndRaceDefineData raceData) ? raceData.RaceId : string.Empty;
            character.ClassId = TryGetSelected(m_classes, m_selectedClassIndex, out DndClassDefineData classData) ? classData.ClassId : string.Empty;
            character.BackgroundId = TryGetSelected(m_backgrounds, m_selectedBackgroundIndex, out DndBackgroundDefineData backgroundData) ? backgroundData.BackgroundId : string.Empty;
            character.FeatId = TryGetSelected(m_feats, m_selectedFeatIndex, out DndFeatDefineData featData) ? featData.FeatId : string.Empty;
            character.SpellId = TryGetSelected(m_spells, m_selectedSpellIndex, out DndSpellDefineData spellData) ? spellData.SpellId : string.Empty;
            character.Level = Math.Max(1, character.Level);
            character.IsCompleted = false;
            character.UpdatedAt = DateTime.UtcNow.ToString("O");
        }

        private void ApplySelectionsFromCharacter(CharacterCardDraftSaveData character)
        {
            m_selectedRaceIndex = FindIndexById(m_races, character?.RaceId, data => data.RaceId);
            m_selectedClassIndex = FindIndexById(m_classes, character?.ClassId, data => data.ClassId);
            m_selectedBackgroundIndex = FindIndexById(m_backgrounds, character?.BackgroundId, data => data.BackgroundId);
            m_selectedFeatIndex = FindIndexById(m_feats, character?.FeatId, data => data.FeatId);
            m_selectedSpellIndex = FindIndexById(m_spells, character?.SpellId, data => data.SpellId);
        }

        private void ClampSelectedListCharacterIndex()
        {
            if (m_characterCards.Count <= 0)
            {
                m_selectedListCharacterIndex = -1;
                return;
            }

            if (m_selectedListCharacterIndex < 0 || m_selectedListCharacterIndex >= m_characterCards.Count)
            {
                m_selectedListCharacterIndex = 0;
            }
        }

        private void SetAllRowsActive(bool active)
        {
            for (int index = 0; index < m_rowLabels.Length; index++)
            {
                SetRowActive(index, active);
            }
        }

        private void SetRowActive(int rowIndex, bool active)
        {
            if (rowIndex < 0 || rowIndex >= m_rowLabels.Length || m_rowLabels[rowIndex] == null)
            {
                return;
            }

            Transform rowTransform = m_rowLabels[rowIndex].transform.parent;
            if (rowTransform != null)
            {
                rowTransform.gameObject.SetActive(active);
            }
        }

        private void SetAllRowButtonsInteractable(bool interactable)
        {
            for (int index = 0; index < m_rowLeftButtons.Length; index++)
            {
                SetButtonInteractable(m_rowLeftButtons[index], interactable);
                SetButtonInteractable(m_rowRightButtons[index], interactable);
            }
        }

        private static void SetButtonInteractable(Button button, bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
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

        private static int FindIndexById<T>(List<T> list, string id, Func<T, string> idGetter)
        {
            if (list == null || idGetter == null || string.IsNullOrWhiteSpace(id))
            {
                return 0;
            }

            for (int index = 0; index < list.Count; index++)
            {
                if (string.Equals(idGetter(list[index]), id, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return 0;
        }

        private static string FormatSelectedName<T>(string id, List<T> list, Func<T, string> idGetter, Func<T, string> nameGetter, string fallback)
        {
            int index = FindIndexById(list, id, idGetter);
            if (list != null && index >= 0 && index < list.Count && string.Equals(idGetter(list[index]), id, StringComparison.OrdinalIgnoreCase))
            {
                string name = nameGetter(list[index]);
                return string.IsNullOrWhiteSpace(name) ? fallback : name;
            }

            return fallback;
        }

        private static string FormatSavedTime(string savedAt)
        {
            if (DateTime.TryParse(savedAt, out DateTime dateTime))
            {
                return dateTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
            }

            return "未知";
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

    [Serializable]
    internal sealed class CharacterCardLibrarySaveData
    {
        public List<CharacterCardDraftSaveData> Characters = new List<CharacterCardDraftSaveData>();
    }

    [Serializable]
    internal sealed class CharacterCardDraftSaveData
    {
        public string CharacterId = string.Empty;
        public string CharacterName = string.Empty;
        public string Alignment = string.Empty;
        public string RaceId = string.Empty;
        public string ClassId = string.Empty;
        public string BackgroundId = string.Empty;
        public string FeatId = string.Empty;
        public string SpellId = string.Empty;
        public string PreviewImagePath = string.Empty;
        public int Level = 1;
        public bool IsCompleted;
        public string CreatedAt = string.Empty;
        public string UpdatedAt = string.Empty;
    }

    internal static class CharacterCardLocalRepository
    {
        private const string SaveDirectoryName = "CharacterCards";
        private const string SaveFileName = "character_cards.json";

        public static string GetSaveFilePath()
        {
            return Path.Combine(Application.persistentDataPath, SaveDirectoryName, SaveFileName);
        }

        public static CharacterCardLibrarySaveData Load()
        {
            string filePath = GetSaveFilePath();
            if (!File.Exists(filePath))
            {
                return new CharacterCardLibrarySaveData();
            }

            try
            {
                string json = File.ReadAllText(filePath, Encoding.UTF8);
                CharacterCardLibrarySaveData data = Utility.Json.ToObject<CharacterCardLibrarySaveData>(json);
                return data ?? new CharacterCardLibrarySaveData();
            }
            catch (Exception exception)
            {
                Log.Error($"角色卡管理：读取本地角色存档失败。{exception.Message}");
                return new CharacterCardLibrarySaveData();
            }
        }

        public static void Save(CharacterCardLibrarySaveData data)
        {
            string filePath = GetSaveFilePath();
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, Utility.Json.ToJson(data ?? new CharacterCardLibrarySaveData()), Encoding.UTF8);
        }

        public static CharacterCardDraftSaveData CreateDraft()
        {
            string now = DateTime.UtcNow.ToString("O");
            return new CharacterCardDraftSaveData
            {
                CharacterId = $"character_{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                CharacterName = "未命名角色",
                Alignment = string.Empty,
                Level = 1,
                IsCompleted = false,
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        public static void Upsert(CharacterCardDraftSaveData character)
        {
            if (character == null)
            {
                return;
            }

            CharacterCardLibrarySaveData library = Load();
            Normalize(character);
            int index = -1;
            for (int i = 0; i < library.Characters.Count; i++)
            {
                if (string.Equals(library.Characters[i].CharacterId, character.CharacterId, StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
            {
                library.Characters[index] = character;
            }
            else
            {
                library.Characters.Add(character);
            }

            Save(library);
        }

        public static void Delete(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return;
            }

            CharacterCardLibrarySaveData library = Load();
            for (int index = library.Characters.Count - 1; index >= 0; index--)
            {
                if (string.Equals(library.Characters[index].CharacterId, characterId, StringComparison.OrdinalIgnoreCase))
                {
                    library.Characters.RemoveAt(index);
                }
            }

            Save(library);
        }

        public static CharacterCardDraftSaveData Normalize(CharacterCardDraftSaveData character)
        {
            if (character == null)
            {
                return CreateDraft();
            }

            if (string.IsNullOrWhiteSpace(character.CharacterId))
            {
                character.CharacterId = $"character_{DateTime.UtcNow:yyyyMMddHHmmssfff}";
            }

            if (string.IsNullOrWhiteSpace(character.CharacterName))
            {
                character.CharacterName = "未命名角色";
            }

            character.Alignment ??= string.Empty;
            character.RaceId ??= string.Empty;
            character.ClassId ??= string.Empty;
            character.BackgroundId ??= string.Empty;
            character.FeatId ??= string.Empty;
            character.SpellId ??= string.Empty;
            character.PreviewImagePath ??= string.Empty;
            character.Level = Math.Max(1, character.Level);

            string now = DateTime.UtcNow.ToString("O");
            if (string.IsNullOrWhiteSpace(character.CreatedAt))
            {
                character.CreatedAt = now;
            }

            if (string.IsNullOrWhiteSpace(character.UpdatedAt))
            {
                character.UpdatedAt = character.CreatedAt;
            }

            return character;
        }
    }

    internal sealed class CharacterCardListItemView
    {
        private readonly GameObject m_root;
        private readonly Image m_background;
        private readonly Image m_previewImage;
        private readonly TMP_Text m_previewPlaceholder;
        private readonly TMP_Text m_nameText;
        private readonly TMP_Text m_classText;
        private readonly TMP_Text m_statusText;
        private readonly Button m_button;

        private Texture2D m_loadedPreviewTexture;
        private Sprite m_loadedPreviewSprite;
        private string m_loadedPreviewPath = string.Empty;

        private CharacterCardListItemView(
            GameObject root,
            Image background,
            Image previewImage,
            TMP_Text previewPlaceholder,
            TMP_Text nameText,
            TMP_Text classText,
            TMP_Text statusText,
            Button button)
        {
            m_root = root;
            m_background = background;
            m_previewImage = previewImage;
            m_previewPlaceholder = previewPlaceholder;
            m_nameText = nameText;
            m_classText = classText;
            m_statusText = statusText;
            m_button = button;
        }

        public static CharacterCardListItemView BindTemplate(GameObject root)
        {
            Image background = root.GetComponent<Image>();
            Button button = root.GetComponent<Button>();
            Image previewImage = root.transform.Find("m_imgPreview")?.GetComponent<Image>();
            TMP_Text placeholder = root.transform.Find("m_imgPreview/m_tmpPreviewPlaceholder")?.GetComponent<TMP_Text>();
            TMP_Text nameText = root.transform.Find("m_tmpCharacterName")?.GetComponent<TMP_Text>();
            TMP_Text classText = root.transform.Find("m_tmpCharacterClass")?.GetComponent<TMP_Text>();
            TMP_Text statusText = root.transform.Find("m_tmpCharacterStatus")?.GetComponent<TMP_Text>();

            return new CharacterCardListItemView(root, background, previewImage, placeholder, nameText, classText, statusText, button);
        }

        public void Bind(CharacterCardDraftSaveData character, string classLine, string statusLine, bool selected, Action clickAction)
        {
            SetActive(true);
            if (m_background != null)
            {
                m_background.color = selected
                    ? new Color(0.24f, 0.30f, 0.38f, 1f)
                    : new Color(0.12f, 0.15f, 0.19f, 0.96f);
            }

            SetText(m_nameText, string.IsNullOrWhiteSpace(character.CharacterName) ? "未命名角色" : character.CharacterName);
            SetText(m_classText, classLine ?? string.Empty);
            SetText(m_statusText, statusLine ?? string.Empty);
            ApplyPreview(character.PreviewImagePath);
            if (m_button != null)
            {
                m_button.onClick.RemoveAllListeners();
                m_button.onClick.AddListener(() => clickAction?.Invoke());
            }
        }

        public void SetActive(bool active)
        {
            if (m_root != null)
            {
                m_root.SetActive(active);
            }
        }

        private void ApplyPreview(string previewPath)
        {
            bool hasPreview = TryLoadPreview(previewPath);
            if (m_previewImage != null)
            {
                m_previewImage.color = hasPreview ? Color.white : new Color(0.05f, 0.06f, 0.08f, 1f);
            }

            if (m_previewPlaceholder != null)
            {
                m_previewPlaceholder.gameObject.SetActive(!hasPreview);
            }
        }

        private bool TryLoadPreview(string previewPath)
        {
            if (m_previewImage == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(previewPath) || !File.Exists(previewPath))
            {
                m_previewImage.sprite = null;
                return false;
            }

            if (string.Equals(m_loadedPreviewPath, previewPath, StringComparison.OrdinalIgnoreCase) && m_loadedPreviewSprite != null)
            {
                m_previewImage.sprite = m_loadedPreviewSprite;
                return true;
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(previewPath);
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!texture.LoadImage(bytes))
                {
                    UnityEngine.Object.Destroy(texture);
                    m_previewImage.sprite = null;
                    return false;
                }

                ReleasePreview();
                m_loadedPreviewTexture = texture;
                m_loadedPreviewSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                m_loadedPreviewPath = previewPath;
                m_previewImage.sprite = m_loadedPreviewSprite;
                return true;
            }
            catch (Exception exception)
            {
                Log.Warning($"角色卡管理：加载角色预览图失败。{exception.Message}");
                m_previewImage.sprite = null;
                return false;
            }
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value ?? string.Empty;
            }
        }

        private void ReleasePreview()
        {
            if (m_loadedPreviewSprite != null)
            {
                UnityEngine.Object.Destroy(m_loadedPreviewSprite);
                m_loadedPreviewSprite = null;
            }

            if (m_loadedPreviewTexture != null)
            {
                UnityEngine.Object.Destroy(m_loadedPreviewTexture);
                m_loadedPreviewTexture = null;
            }

            m_loadedPreviewPath = string.Empty;
        }
    }
}
