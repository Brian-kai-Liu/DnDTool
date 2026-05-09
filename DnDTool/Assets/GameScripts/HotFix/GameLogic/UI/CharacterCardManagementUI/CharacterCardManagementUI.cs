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

    internal enum CharacterCreationOptionPanelMode
    {
        None,
        Race,
        Class,
        Background,
        Feat,
        Spell
    }

    internal enum CharacterCreationOptionCardStyle
    {
        Default,
        GroupHeader,
        SubRace
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
        private TMP_Text m_tmpCharacterNameLabel = null!;
        private TMP_Text m_tmpAlignmentLabel = null!;
        private TMP_InputField m_tmpInputCharacterName = null!;
        private TMP_Dropdown m_tmpDropdownAlignment = null!;
        private RectTransform m_rectPanelLeft = null!;
        private RectTransform m_rectPanelCenter = null!;
        private RectTransform m_rectCreateDraft = null!;
        private GameObject m_goCardListRoot = null!;
        private GameObject m_goCreationOptionListRoot = null!;
        private RectTransform m_rectCreationOptionListRoot = null!;
        private GameObject m_goRowCharacterName = null!;
        private GameObject m_goRowAlignment = null!;
        private RectTransform m_rectCardContent = null!;
        private RectTransform m_rectCreationOptionCardContent = null!;
        private RectTransform m_rectCreationOptionViewport = null!;
        private GameObject m_goCharacterCardTemplate = null!;
        private GameObject m_goCreationOptionCardTemplate = null!;
        private Button m_btnDeleteSelected = null!;
        private TMP_Text m_tmpDeleteSelectedText = null!;
        private Button m_btnRowClass = null!;
        private Button m_btnRowRace = null!;
        private Button m_btnRowBackground = null!;
        private Button m_btnRowFeat = null!;
        private Button m_btnRowSpell = null!;

        private readonly List<DndClassDefineData> m_classes = new List<DndClassDefineData>();
        private readonly List<DndRaceDefineData> m_races = new List<DndRaceDefineData>();
        private readonly List<DndBackgroundDefineData> m_backgrounds = new List<DndBackgroundDefineData>();
        private readonly List<DndFeatDefineData> m_feats = new List<DndFeatDefineData>();
        private readonly List<DndSpellDefineData> m_spells = new List<DndSpellDefineData>();
        private readonly List<string> m_alignmentOptions = new List<string>();
        private readonly List<CharacterCardDraftSaveData> m_characterCards = new List<CharacterCardDraftSaveData>();
        private readonly List<RaceOptionEntry> m_visibleRaceOptionEntries = new List<RaceOptionEntry>();
        private readonly Dictionary<string, bool> m_expandedRaceGroups = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, bool> m_expandedEditorSections = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, MainRacePreviewData> m_mainRacePreviewDataByGroupKey = new Dictionary<string, MainRacePreviewData>(StringComparer.OrdinalIgnoreCase);

        private TMP_Text[] m_rowLabels = Array.Empty<TMP_Text>();
        private TMP_Text[] m_rowValues = Array.Empty<TMP_Text>();
        private Button[] m_rowLeftButtons = Array.Empty<Button>();
        private Button[] m_rowRightButtons = Array.Empty<Button>();
        private TMP_Text[] m_rowLeftButtonTexts = Array.Empty<TMP_Text>();
        private TMP_Text[] m_rowRightButtonTexts = Array.Empty<TMP_Text>();
        private RectTransform[] m_rowRects = Array.Empty<RectTransform>();
        private Vector2[] m_rowOriginalAnchoredPositions = Array.Empty<Vector2>();
        private readonly List<CharacterCardListItemView> m_cardViews = new List<CharacterCardListItemView>();
        private readonly List<CharacterCreationOptionCardView> m_creationOptionCardViews = new List<CharacterCreationOptionCardView>();

        private GridLayoutGroup m_creationOptionGridLayout = null!;
        private ContentSizeFitter m_creationOptionContentFitter = null!;
        private Vector2 m_creationOptionOriginalCellSize;
        private Vector2 m_creationOptionOriginalSpacing;
        private RectOffset m_creationOptionOriginalPadding = null!;
        private int m_creationOptionOriginalConstraintCount;
        private Vector2 m_panelLeftOriginalAnchorMin;
        private Vector2 m_panelLeftOriginalAnchorMax;
        private Vector2 m_panelLeftOriginalOffsetMin;
        private Vector2 m_panelLeftOriginalOffsetMax;
        private Vector2 m_panelCenterOriginalAnchorMax;
        private Vector2 m_panelCenterOriginalOffsetMax;
        private Vector2 m_createDraftOriginalAnchoredPosition;
        private bool m_hasMovedCreationOptionListToLeft;

        private int m_selectedClassIndex;
        private int m_selectedRaceIndex;
        private int m_selectedBackgroundIndex;
        private int m_selectedFeatIndex;
        private int m_selectedSpellIndex;
        private int m_selectedListCharacterIndex = -1;
        private string m_selectedMainRaceGroupKey = string.Empty;
        private bool m_isRefreshingCharacterInputs;
        private CharacterCardManagementMode m_mode = CharacterCardManagementMode.List;
        private CharacterCreationOptionPanelMode m_centerOptionMode = CharacterCreationOptionPanelMode.None;
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
            m_goRowCharacterName = FindChild("m_panelLeft/m_rowCharacterName")?.gameObject;
            m_goRowAlignment = FindChild("m_panelLeft/m_rowAlignment")?.gameObject;
            m_tmpCharacterNameLabel = FindChildComponent<TMP_Text>("m_panelLeft/m_rowCharacterName/Label");
            m_tmpAlignmentLabel = FindChildComponent<TMP_Text>("m_panelLeft/m_rowAlignment/Label");
            m_tmpInputCharacterName = FindChildComponent<TMP_InputField>("m_panelLeft/m_rowCharacterName/m_tmpInputCharacterName");
            m_tmpDropdownAlignment = FindChildComponent<TMP_Dropdown>("m_panelLeft/m_rowAlignment/m_tmpDropdownAlignment");
            m_btnRowRace = FindChildComponent<Button>("m_panelLeft/m_rowRace");
            m_btnRowClass = FindChildComponent<Button>("m_panelLeft/m_rowClass");
            m_btnRowBackground = FindChildComponent<Button>("m_panelLeft/m_rowBackground");
            m_btnRowFeat = FindChildComponent<Button>("m_panelLeft/m_rowFeat");
            m_btnRowSpell = FindChildComponent<Button>("m_panelLeft/m_rowSpell");
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
            m_rectPanelLeft = FindChildComponent<RectTransform>("m_panelLeft");
            m_rectPanelCenter = FindChildComponent<RectTransform>("m_panelCenter");
            m_rectCreateDraft = m_btnCreateDraft != null ? m_btnCreateDraft.GetComponent<RectTransform>() : null;
            m_goCardListRoot = FindChild("m_panelLeft/m_scrollCharacterCardList").gameObject;
            m_goCreationOptionListRoot = FindChild("m_panelCenter/m_scrollCreationOptionCards")?.gameObject;
            m_rectCreationOptionListRoot = m_goCreationOptionListRoot != null
                ? m_goCreationOptionListRoot.GetComponent<RectTransform>()
                : null;
            m_rectCardContent = FindChildComponent<RectTransform>("m_panelLeft/m_scrollCharacterCardList/Viewport/m_rectCharacterCardContent");
            m_rectCreationOptionViewport = FindChildComponent<RectTransform>("m_panelCenter/m_scrollCreationOptionCards/Viewport");
            m_rectCreationOptionCardContent = FindChildComponent<RectTransform>("m_panelCenter/m_scrollCreationOptionCards/Viewport/m_rectCreationOptionCardContent");
            m_goCharacterCardTemplate = FindChild("m_panelLeft/m_scrollCharacterCardList/Viewport/m_rectCharacterCardContent/m_itemCharacterCardTemplate").gameObject;
            m_goCreationOptionCardTemplate = FindChild("m_panelCenter/m_scrollCreationOptionCards/Viewport/m_rectCreationOptionCardContent/m_itemCreationOptionCardTemplate")?.gameObject;
            m_btnDeleteSelected = FindChildComponent<Button>("m_panelLeft/m_btnDeleteSelectedCharacter");
            m_tmpDeleteSelectedText = FindChildComponent<TMP_Text>("m_panelLeft/m_btnDeleteSelectedCharacter");
            m_creationOptionGridLayout = m_rectCreationOptionCardContent != null
                ? m_rectCreationOptionCardContent.GetComponent<GridLayoutGroup>()
                : null;
            m_creationOptionContentFitter = m_rectCreationOptionCardContent != null
                ? m_rectCreationOptionCardContent.GetComponent<ContentSizeFitter>()
                : null;
            if (m_creationOptionGridLayout != null)
            {
                m_creationOptionOriginalCellSize = m_creationOptionGridLayout.cellSize;
                m_creationOptionOriginalSpacing = m_creationOptionGridLayout.spacing;
                m_creationOptionOriginalConstraintCount = m_creationOptionGridLayout.constraintCount;
                m_creationOptionOriginalPadding = new RectOffset(
                    m_creationOptionGridLayout.padding.left,
                    m_creationOptionGridLayout.padding.right,
                    m_creationOptionGridLayout.padding.top,
                    m_creationOptionGridLayout.padding.bottom);
            }

            if (m_rectPanelLeft != null)
            {
                m_panelLeftOriginalAnchorMin = m_rectPanelLeft.anchorMin;
                m_panelLeftOriginalAnchorMax = m_rectPanelLeft.anchorMax;
                m_panelLeftOriginalOffsetMin = m_rectPanelLeft.offsetMin;
                m_panelLeftOriginalOffsetMax = m_rectPanelLeft.offsetMax;
            }

            if (m_rectPanelCenter != null)
            {
                m_panelCenterOriginalAnchorMax = m_rectPanelCenter.anchorMax;
                m_panelCenterOriginalOffsetMax = m_rectPanelCenter.offsetMax;
            }

            m_rowLabels = new[] { m_tmpRaceLabel, m_tmpClassLabel, m_tmpBackgroundLabel, m_tmpFeatLabel, m_tmpSpellLabel };
            m_rowValues = new[] { m_tmpRaceValue, m_tmpClassValue, m_tmpBackgroundValue, m_tmpFeatValue, m_tmpSpellValue };
            m_rowLeftButtons = new[] { m_btnPrevRace, m_btnPrevClass, m_btnPrevBackground, m_btnPrevFeat, m_btnPrevSpell };
            m_rowRightButtons = new[] { m_btnNextRace, m_btnNextClass, m_btnNextBackground, m_btnNextFeat, m_btnNextSpell };
            m_rowLeftButtonTexts = new[] { m_tmpPrevRaceText, m_tmpPrevClassText, m_tmpPrevBackgroundText, m_tmpPrevFeatText, m_tmpPrevSpellText };
            m_rowRightButtonTexts = new[] { m_tmpNextRaceText, m_tmpNextClassText, m_tmpNextBackgroundText, m_tmpNextFeatText, m_tmpNextSpellText };
            m_rowRects = new[]
            {
                m_btnRowRace != null ? m_btnRowRace.GetComponent<RectTransform>() : null,
                m_btnRowClass != null ? m_btnRowClass.GetComponent<RectTransform>() : null,
                m_btnRowBackground != null ? m_btnRowBackground.GetComponent<RectTransform>() : null,
                m_btnRowFeat != null ? m_btnRowFeat.GetComponent<RectTransform>() : null,
                m_btnRowSpell != null ? m_btnRowSpell.GetComponent<RectTransform>() : null
            };
            m_rowOriginalAnchoredPositions = new Vector2[m_rowRects.Length];
            for (int index = 0; index < m_rowRects.Length; index++)
            {
                m_rowOriginalAnchoredPositions[index] = m_rowRects[index] != null
                    ? m_rowRects[index].anchoredPosition
                    : Vector2.zero;
            }

            BindButton(m_btnBack, OnClickBack);
            BindEditorInputFields();
            if (m_goCharacterCardTemplate != null)
            {
                m_goCharacterCardTemplate.SetActive(false);
            }

            if (m_goCreationOptionCardTemplate != null)
            {
                m_goCreationOptionCardTemplate.SetActive(false);
            }

            MoveCreationOptionListToLeftPanel();
            ApplyCharacterListPanelLayout();
            if (m_rectCreateDraft != null)
            {
                m_createDraftOriginalAnchoredPosition = m_rectCreateDraft.anchoredPosition;
            }
            SetCharacterCardListVisible(false);
            SetCreationOptionListVisible(false);
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
            m_alignmentOptions.Clear();

            AddRange(m_classes, service.Classes);
            AddRange(m_races, service.Races);
            AddRange(m_backgrounds, service.Backgrounds);
            AddRange(m_feats, service.Feats);
            AddRange(m_spells, service.Spells);
            AddRange(m_alignmentOptions, service.GetAlignmentOptionLabels());
            ApplyAlignmentDropdownOptions();
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
            m_centerOptionMode = CharacterCreationOptionPanelMode.None;
            m_currentCharacter = null;
            ApplyCharacterListPanelLayout();
            LoadCharacterCards();
            ClampSelectedListCharacterIndex();
            RefreshCharacterListView();
        }

        private void ShowCharacterEditor(CharacterCardDraftSaveData character, bool applyCharacterSelections = true)
        {
            m_mode = CharacterCardManagementMode.Editor;
            ResetEditorState();
            m_currentCharacter = CharacterCardLocalRepository.Normalize(character);
            if (applyCharacterSelections)
            {
                ApplySelectionsFromCharacter(m_currentCharacter);
            }
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
            SetCreationOptionListVisible(false);
            HideAllCreationOptionCards();
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
                    () => OnClickSelectCharacterCard(capturedIndex),
                    () => OnClickEditCharacterCard(capturedIndex));
            }

            SetText(m_tmpSelectionDetail, BuildCharacterListDetail());
            SetText(m_tmpCharacterPreview, BuildCharacterListPreview());
        }

        private void OnClickCreateNewCharacter()
        {
            CharacterCardDraftSaveData character = CharacterCardLocalRepository.CreateDraft();
            ShowCharacterEditor(character, false);
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

        private void OnClickEditCharacterCard(int characterIndex)
        {
            if (characterIndex < 0 || characterIndex >= m_characterCards.Count)
            {
                return;
            }

            m_selectedListCharacterIndex = characterIndex;
            ShowCharacterEditor(m_characterCards[characterIndex]);
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
                SyncSelectedMainRaceGroupFromSelection();
                RefreshView();
                return;
            }

            selectedIndex = (selectedIndex + direction + count) % count;
            SyncSelectedMainRaceGroupFromSelection();
            RefreshView();
        }

        private void ClampSelectionIndexes()
        {
            m_selectedClassIndex = ClampIndex(m_selectedClassIndex, m_classes.Count);
            m_selectedRaceIndex = ClampIndex(m_selectedRaceIndex, m_races.Count);
            m_selectedBackgroundIndex = ClampIndex(m_selectedBackgroundIndex, m_backgrounds.Count);
            m_selectedFeatIndex = ClampIndex(m_selectedFeatIndex, m_feats.Count);
            m_selectedSpellIndex = ClampIndex(m_selectedSpellIndex, m_spells.Count);
            SyncSelectedMainRaceGroupFromSelection();
        }

        private void SyncSelectedMainRaceGroupFromSelection()
        {
            m_selectedMainRaceGroupKey = string.Empty;
            if (!TryGetSelected(m_races, m_selectedRaceIndex, out DndRaceDefineData raceData))
            {
                return;
            }

            if (!TryGetRaceGroupPrefix(raceData, out string prefix))
            {
                return;
            }

            List<RaceGroupInfo> groups = BuildRaceGroups();
            for (int index = 0; index < groups.Count; index++)
            {
                RaceGroupInfo group = groups[index];
                if (group.IsGrouped && string.Equals(group.GroupKey, prefix, StringComparison.OrdinalIgnoreCase))
                {
                    m_selectedMainRaceGroupKey = group.GroupKey;
                    return;
                }
            }
        }

        private void ResetEditorState()
        {
            m_selectedClassIndex = 0;
            m_selectedRaceIndex = 0;
            m_selectedBackgroundIndex = 0;
            m_selectedFeatIndex = 0;
            m_selectedSpellIndex = 0;
            m_selectedMainRaceGroupKey = string.Empty;
            m_isRefreshingCharacterInputs = false;
            m_centerOptionMode = CharacterCreationOptionPanelMode.None;
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
            ApplyEditorPanelLayout();
            SetText(m_tmpTitle, "角色预览/创建");
            SetText(m_tmpCreateDraftText, "保存角色草稿");
            SetCharacterCardListVisible(false);
            SetCreationOptionListVisible(m_centerOptionMode != CharacterCreationOptionPanelMode.None);
            ApplyCreationStepLabels();
            ApplyEditorButtonTexts();
            BindEditorButtons();
            SetAllRowsActive(true);
            SetBasicInfoRowsActive(true);
            RefreshCharacterBasicInfoInputs();
            RefreshRuleStatus();
            SetText(m_tmpRaceValue, GetRaceLabel());
            SetText(m_tmpClassValue, GetClassLabel());
            SetText(m_tmpBackgroundValue, GetBackgroundLabel());
            SetText(m_tmpFeatValue, GetFeatLabel());
            SetText(m_tmpSpellValue, GetSpellLabel());
            ApplyEditorRowAccordionLayout();
            RefreshCreationOptionCards();
            SetText(m_tmpSelectionDetail, BuildSelectionDetail());
            SetText(m_tmpCharacterPreview, BuildCharacterPreview());
        }

        private void ApplyCreationStepLabels()
        {
            SetText(m_tmpCharacterNameLabel, "角色名称");
            SetText(m_tmpAlignmentLabel, "阵营");
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
            BindButton(m_btnRowRace, () => ToggleCreationOptionPanelMode(CharacterCreationOptionPanelMode.Race));
            BindButton(m_btnRowClass, () => ToggleCreationOptionPanelMode(CharacterCreationOptionPanelMode.Class));
            BindButton(m_btnRowBackground, () => ToggleCreationOptionPanelMode(CharacterCreationOptionPanelMode.Background));
            BindButton(m_btnRowFeat, () => ToggleCreationOptionPanelMode(CharacterCreationOptionPanelMode.Feat));
            BindButton(m_btnRowSpell, () => ToggleCreationOptionPanelMode(CharacterCreationOptionPanelMode.Spell));
            BindButton(m_btnPrevClass, () => MoveSelection(m_classes.Count, ref m_selectedClassIndex, -1, CharacterCreationOptionPanelMode.Class));
            BindButton(m_btnNextClass, () => MoveSelection(m_classes.Count, ref m_selectedClassIndex, 1, CharacterCreationOptionPanelMode.Class));
            BindButton(m_btnPrevRace, () => MoveSelection(m_races.Count, ref m_selectedRaceIndex, -1, CharacterCreationOptionPanelMode.Race));
            BindButton(m_btnNextRace, () => MoveSelection(m_races.Count, ref m_selectedRaceIndex, 1, CharacterCreationOptionPanelMode.Race));
            BindButton(m_btnPrevBackground, () => MoveSelection(m_backgrounds.Count, ref m_selectedBackgroundIndex, -1, CharacterCreationOptionPanelMode.Background));
            BindButton(m_btnNextBackground, () => MoveSelection(m_backgrounds.Count, ref m_selectedBackgroundIndex, 1, CharacterCreationOptionPanelMode.Background));
            BindButton(m_btnPrevFeat, () => MoveSelection(m_feats.Count, ref m_selectedFeatIndex, -1, CharacterCreationOptionPanelMode.Feat));
            BindButton(m_btnNextFeat, () => MoveSelection(m_feats.Count, ref m_selectedFeatIndex, 1, CharacterCreationOptionPanelMode.Feat));
            BindButton(m_btnPrevSpell, () => MoveSelection(m_spells.Count, ref m_selectedSpellIndex, -1, CharacterCreationOptionPanelMode.Spell));
            BindButton(m_btnNextSpell, () => MoveSelection(m_spells.Count, ref m_selectedSpellIndex, 1, CharacterCreationOptionPanelMode.Spell));
            BindButton(m_btnCreateDraft, OnClickCreateDraft);
            SetAllRowButtonsInteractable(true);
        }

        private void ToggleCreationOptionPanelMode(CharacterCreationOptionPanelMode mode)
        {
            string sectionKey = GetSectionKey(mode);
            bool expanded = !string.IsNullOrWhiteSpace(sectionKey)
                && m_expandedEditorSections.TryGetValue(sectionKey, out bool value)
                && value;

            if (expanded)
            {
                m_expandedEditorSections[sectionKey] = false;
                m_centerOptionMode = CharacterCreationOptionPanelMode.None;
            }
            else
            {
                CollapseAllEditorSections();
                if (!string.IsNullOrWhiteSpace(sectionKey))
                {
                    m_expandedEditorSections[sectionKey] = true;
                }

                m_centerOptionMode = mode;
            }

            RefreshView();
        }

        private void MoveSelection(int count, ref int selectedIndex, int direction, CharacterCreationOptionPanelMode optionMode)
        {
            m_centerOptionMode = optionMode;
            MoveSelection(count, ref selectedIndex, direction);
        }

        private void BindEditorInputFields()
        {
            if (m_tmpInputCharacterName != null)
            {
                m_tmpInputCharacterName.onValueChanged.RemoveAllListeners();
                m_tmpInputCharacterName.onValueChanged.AddListener(OnCharacterNameInputChanged);
            }

            if (m_tmpDropdownAlignment != null)
            {
                m_tmpDropdownAlignment.onValueChanged.RemoveAllListeners();
                m_tmpDropdownAlignment.onValueChanged.AddListener(OnAlignmentDropdownChanged);
            }
        }

        private void ApplyAlignmentDropdownOptions()
        {
            if (m_tmpDropdownAlignment == null)
            {
                return;
            }

            m_tmpDropdownAlignment.ClearOptions();
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData("待选择")
            };

            for (int index = 0; index < m_alignmentOptions.Count; index++)
            {
                string label = m_alignmentOptions[index];
                if (!string.IsNullOrWhiteSpace(label))
                {
                    options.Add(new TMP_Dropdown.OptionData(label));
                }
            }

            m_tmpDropdownAlignment.AddOptions(options);
            m_tmpDropdownAlignment.SetValueWithoutNotify(0);
            m_tmpDropdownAlignment.RefreshShownValue();
        }

        private void OnCharacterNameInputChanged(string value)
        {
            if (m_isRefreshingCharacterInputs || m_currentCharacter == null)
            {
                return;
            }

            m_currentCharacter.CharacterName = string.IsNullOrWhiteSpace(value) ? "未命名角色" : value.Trim();
            SetText(m_tmpCharacterPreview, BuildCharacterPreview());
        }

        private void OnAlignmentDropdownChanged(int index)
        {
            if (m_isRefreshingCharacterInputs || m_currentCharacter == null || m_tmpDropdownAlignment == null)
            {
                return;
            }

            m_currentCharacter.Alignment = GetAlignmentDropdownText(index);
            SetText(m_tmpCharacterPreview, BuildCharacterPreview());
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

            SetBasicInfoRowsActive(!visible);
        }

        private void SetCreationOptionListVisible(bool visible)
        {
            if (m_goCreationOptionListRoot != null)
            {
                m_goCreationOptionListRoot.SetActive(visible);
            }

            if (!visible)
            {
                HideAllCreationOptionCards();
            }
        }

        private void ApplyEditorRowAccordionLayout()
        {
            if (m_rectCreationOptionListRoot == null)
            {
                return;
            }

            float rowHeight = 74f;
            float extraSpacing = 12f;
            float expandedHeight = m_centerOptionMode == CharacterCreationOptionPanelMode.None ? 0f : GetCreationOptionListHeight();
            int expandedRowIndex = GetExpandedRowIndex();

            for (int index = 0; index < m_rowRects.Length; index++)
            {
                RectTransform rowRect = m_rowRects[index];
                if (rowRect == null)
                {
                    continue;
                }

                float offset = expandedRowIndex >= 0 && index > expandedRowIndex
                    ? expandedHeight + extraSpacing
                    : 0f;
                Vector2 basePosition = GetEditorRowBaseAnchoredPosition(index);
                rowRect.anchoredPosition = new Vector2(
                    basePosition.x,
                    basePosition.y - offset);
            }

            if (expandedRowIndex >= 0 && expandedRowIndex < m_rowRects.Length)
            {
                RectTransform expandedRow = m_rowRects[expandedRowIndex];
                if (expandedRow != null)
                {
                    m_rectCreationOptionListRoot.anchoredPosition = new Vector2(0f, expandedRow.anchoredPosition.y - rowHeight - extraSpacing);
                    m_rectCreationOptionListRoot.sizeDelta = new Vector2(-40f, expandedHeight);
                    m_rectCreationOptionListRoot.SetAsLastSibling();
                }
            }

            if (m_rectCreateDraft != null)
            {
                float offset = expandedRowIndex >= 0 ? expandedHeight + extraSpacing : 0f;
                m_rectCreateDraft.anchoredPosition = new Vector2(
                    m_createDraftOriginalAnchoredPosition.x,
                    m_createDraftOriginalAnchoredPosition.y - offset);
            }
        }

        private int GetExpandedRowIndex()
        {
            switch (m_centerOptionMode)
            {
                case CharacterCreationOptionPanelMode.Race:
                    return 0;
                case CharacterCreationOptionPanelMode.Class:
                    return 1;
                case CharacterCreationOptionPanelMode.Background:
                    return 2;
                case CharacterCreationOptionPanelMode.Feat:
                    return 3;
                case CharacterCreationOptionPanelMode.Spell:
                    return 4;
                default:
                    return -1;
            }
        }

        private Vector2 GetEditorRowBaseAnchoredPosition(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= m_rowOriginalAnchoredPositions.Length)
            {
                return Vector2.zero;
            }

            switch (rowIndex)
            {
                case 0:
                    return m_rowOriginalAnchoredPositions.Length > 1
                        ? m_rowOriginalAnchoredPositions[1]
                        : m_rowOriginalAnchoredPositions[0];
                case 1:
                    return m_rowOriginalAnchoredPositions[0];
                default:
                    return m_rowOriginalAnchoredPositions[rowIndex];
            }
        }

        private static string GetSectionKey(CharacterCreationOptionPanelMode mode)
        {
            switch (mode)
            {
                case CharacterCreationOptionPanelMode.Race:
                    return "race";
                case CharacterCreationOptionPanelMode.Class:
                    return "class";
                case CharacterCreationOptionPanelMode.Background:
                    return "background";
                case CharacterCreationOptionPanelMode.Feat:
                    return "feat";
                case CharacterCreationOptionPanelMode.Spell:
                    return "spell";
                default:
                    return string.Empty;
            }
        }

        private void CollapseAllEditorSections()
        {
            m_expandedEditorSections.Clear();
        }

        private float GetCreationOptionListHeight()
        {
            int optionCount = GetCreationOptionCount();
            if (optionCount <= 0)
            {
                return 220f;
            }

            float cardHeight = m_creationOptionOriginalCellSize.y;
            float spacingY = m_creationOptionOriginalSpacing.y;
            int topPadding = m_creationOptionOriginalPadding != null ? m_creationOptionOriginalPadding.top : 6;
            int bottomPadding = m_creationOptionOriginalPadding != null ? m_creationOptionOriginalPadding.bottom : 18;
            float contentHeight = topPadding + bottomPadding + (cardHeight * optionCount) + (spacingY * Math.Max(0, optionCount - 1));
            return Mathf.Min(420f, Mathf.Max(220f, contentHeight + 24f));
        }

        private void MoveCreationOptionListToLeftPanel()
        {
            if (m_hasMovedCreationOptionListToLeft || m_rectPanelLeft == null || m_rectCreationOptionListRoot == null)
            {
                return;
            }

            m_rectCreationOptionListRoot.SetParent(m_rectPanelLeft, false);
            m_rectCreationOptionListRoot.anchorMin = new Vector2(0f, 1f);
            m_rectCreationOptionListRoot.anchorMax = new Vector2(1f, 1f);
            m_rectCreationOptionListRoot.pivot = new Vector2(0.5f, 1f);
            m_rectCreationOptionListRoot.anchoredPosition = Vector2.zero;
            m_rectCreationOptionListRoot.sizeDelta = new Vector2(-40f, 340f);
            m_rectCreationOptionListRoot.SetAsLastSibling();
            m_hasMovedCreationOptionListToLeft = true;
        }

        private void ApplyEditorPanelLayout()
        {
            MoveCreationOptionListToLeftPanel();
            if (m_rectPanelLeft != null)
            {
                m_rectPanelLeft.anchorMin = m_panelLeftOriginalAnchorMin;
                m_rectPanelLeft.anchorMax = m_panelLeftOriginalAnchorMax;
                m_rectPanelLeft.offsetMin = m_panelLeftOriginalOffsetMin;
                m_rectPanelLeft.offsetMax = m_panelLeftOriginalOffsetMax;
            }

            if (m_rectPanelCenter != null)
            {
                m_rectPanelCenter.gameObject.SetActive(false);
            }
        }

        private void ApplyCharacterListPanelLayout()
        {
            if (m_rectPanelLeft != null)
            {
                m_rectPanelLeft.anchorMin = m_panelLeftOriginalAnchorMin;
                m_rectPanelLeft.anchorMax = m_panelLeftOriginalAnchorMax;
                m_rectPanelLeft.offsetMin = m_panelLeftOriginalOffsetMin;
                m_rectPanelLeft.offsetMax = m_panelLeftOriginalOffsetMax;
            }

            if (m_rectPanelCenter != null)
            {
                m_rectPanelCenter.gameObject.SetActive(false);
            }
        }

        private void SetBasicInfoRowsActive(bool active)
        {
            if (m_goRowCharacterName != null)
            {
                m_goRowCharacterName.SetActive(active);
            }

            if (m_goRowAlignment != null)
            {
                m_goRowAlignment.SetActive(active);
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

            if (!TryAppendSelectedMainRaceDetail(builder) && TryGetSelected(m_races, m_selectedRaceIndex, out DndRaceDefineData raceData))
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

        private void SetCreationOptionPanelMode(CharacterCreationOptionPanelMode mode)
        {
            m_centerOptionMode = mode;
            SetCreationOptionListVisible(m_centerOptionMode != CharacterCreationOptionPanelMode.None);
            RefreshCreationOptionCards();
        }

        private void RefreshCreationOptionCards()
        {
            if (m_centerOptionMode == CharacterCreationOptionPanelMode.Race)
            {
                BuildVisibleRaceOptionEntries();
            }

            int optionCount = GetCreationOptionCount();
            EnsureCreationOptionCardViewCount(optionCount);

            for (int index = 0; index < m_creationOptionCardViews.Count; index++)
            {
                if (index >= optionCount)
                {
                    m_creationOptionCardViews[index].SetActive(false);
                    continue;
                }

                BindCreationOptionCard(m_creationOptionCardViews[index], index);
            }

            ApplyEditorRowAccordionLayout();
            ApplyCreationOptionLayout(optionCount);
        }

        private int GetCreationOptionCount()
        {
            switch (m_centerOptionMode)
            {
                case CharacterCreationOptionPanelMode.Race:
                    return m_visibleRaceOptionEntries.Count;
                case CharacterCreationOptionPanelMode.Class:
                    return m_classes.Count;
                case CharacterCreationOptionPanelMode.Background:
                    return m_backgrounds.Count;
                case CharacterCreationOptionPanelMode.Feat:
                    return m_feats.Count;
                case CharacterCreationOptionPanelMode.Spell:
                    return m_spells.Count;
                default:
                    return 0;
            }
        }

        private void EnsureCreationOptionCardViewCount(int count)
        {
            while (m_creationOptionCardViews.Count < count)
            {
                if (m_goCreationOptionCardTemplate == null || m_rectCreationOptionCardContent == null)
                {
                    return;
                }

                GameObject itemObject = UnityEngine.Object.Instantiate(m_goCreationOptionCardTemplate, m_rectCreationOptionCardContent);
                itemObject.name = $"m_itemCreationOptionCard_{m_creationOptionCardViews.Count + 1}";
                CharacterCreationOptionCardView cardView = CharacterCreationOptionCardView.BindTemplate(itemObject);
                m_creationOptionCardViews.Add(cardView);
            }
        }

        private void HideAllCreationOptionCards()
        {
            for (int index = 0; index < m_creationOptionCardViews.Count; index++)
            {
                m_creationOptionCardViews[index].SetActive(false);
            }
        }

        private void BindCreationOptionCard(CharacterCreationOptionCardView cardView, int index)
        {
            switch (m_centerOptionMode)
            {
                case CharacterCreationOptionPanelMode.Race:
                    BindRaceOptionCard(cardView, index);
                    return;
                case CharacterCreationOptionPanelMode.Class:
                    BindClassOptionCard(cardView, index);
                    return;
                case CharacterCreationOptionPanelMode.Background:
                    BindBackgroundOptionCard(cardView, index);
                    return;
                case CharacterCreationOptionPanelMode.Feat:
                    BindFeatOptionCard(cardView, index);
                    return;
                case CharacterCreationOptionPanelMode.Spell:
                    BindSpellOptionCard(cardView, index);
                    return;
                default:
                    cardView.SetActive(false);
                    return;
            }
        }

        private void BindRaceOptionCard(CharacterCreationOptionCardView cardView, int index)
        {
            if (index < 0 || index >= m_visibleRaceOptionEntries.Count)
            {
                cardView.SetActive(false);
                return;
            }

            RaceOptionEntry entry = m_visibleRaceOptionEntries[index];
            if (entry.IsGroupHeader)
            {
                bool expanded = IsRaceGroupExpanded(entry.GroupKey);
                cardView.Bind(
                    entry.GroupName,
                    expanded ? $"已展开 {entry.ChildRaceIndexes.Count} 个亚种，点击收起" : $"包含 {entry.ChildRaceIndexes.Count} 个亚种，点击展开",
                    BuildRaceGroupDetail(entry.ChildRaceIndexes),
                    expanded,
                    false,
                    CharacterCreationOptionCardStyle.GroupHeader,
                    () => ToggleRaceGroup(entry.GroupKey));
                return;
            }

            DndRaceDefineData race = m_races[entry.RaceIndex];
            if (entry.IsMainRace)
            {
                bool expanded = IsRaceGroupExpanded(entry.GroupKey);
                int childCount = BuildRaceGroupChildCount(entry.GroupKey);
                bool hasSubRaces = childCount > 0;
                bool isViewingMainRace = string.Equals(entry.GroupKey, m_selectedMainRaceGroupKey, StringComparison.OrdinalIgnoreCase);
                string detail = BuildMainRaceCardDetail(entry.GroupKey, BuildRaceOptionDetail(race));
                cardView.Bind(
                    string.IsNullOrWhiteSpace(entry.GroupName) ? race.Name : entry.GroupName,
                    hasSubRaces
                        ? (expanded ? $"已展开 {childCount} 个亚种，点击收起" : "点击展开亚种")
                        : "点击查看详情",
                    detail,
                    isViewingMainRace,
                    true,
                    CharacterCreationOptionCardStyle.Default,
                    () => OnClickMainRaceOption(entry.RaceIndex, entry.GroupKey, hasSubRaces));
                return;
            }

            cardView.Bind(
                entry.IsSubRace ? $"  {race.Name}" : race.Name,
                BuildRaceOptionSubtitle(race, entry.IsSubRace),
                BuildRaceOptionDetail(race),
                entry.RaceIndex == m_selectedRaceIndex,
                true,
                entry.IsSubRace ? CharacterCreationOptionCardStyle.SubRace : CharacterCreationOptionCardStyle.Default,
                () => OnClickSelectRaceOption(entry.RaceIndex));
        }
        private void BindClassOptionCard(CharacterCreationOptionCardView cardView, int index)
        {
            DndClassDefineData classData = m_classes[index];
            cardView.Bind(
                classData.Name,
                BuildClassOptionSubtitle(classData),
                BuildClassOptionDetail(classData),
                index == m_selectedClassIndex,
                true,
                CharacterCreationOptionCardStyle.Default,
                () => OnClickSelectClassOption(index));
        }

        private void BindBackgroundOptionCard(CharacterCreationOptionCardView cardView, int index)
        {
            DndBackgroundDefineData background = m_backgrounds[index];
            cardView.Bind(
                background.Name,
                BuildBackgroundOptionSubtitle(background),
                BuildBackgroundOptionDetail(background),
                index == m_selectedBackgroundIndex,
                true,
                CharacterCreationOptionCardStyle.Default,
                () => OnClickSelectBackgroundOption(index));
        }

        private void BindFeatOptionCard(CharacterCreationOptionCardView cardView, int index)
        {
            DndFeatDefineData feat = m_feats[index];
            cardView.Bind(
                feat.Name,
                BuildFeatOptionSubtitle(feat),
                BuildFeatOptionDetail(feat),
                index == m_selectedFeatIndex,
                true,
                CharacterCreationOptionCardStyle.Default,
                () => OnClickSelectFeatOption(index));
        }

        private void BindSpellOptionCard(CharacterCreationOptionCardView cardView, int index)
        {
            DndSpellDefineData spell = m_spells[index];
            cardView.Bind(
                spell.Name,
                BuildSpellOptionSubtitle(spell),
                BuildSpellOptionDetail(spell),
                index == m_selectedSpellIndex,
                true,
                CharacterCreationOptionCardStyle.Default,
                () => OnClickSelectSpellOption(index));
        }

        private void OnClickSelectRaceOption(int index)
        {
            m_centerOptionMode = CharacterCreationOptionPanelMode.Race;
            m_selectedRaceIndex = ClampIndex(index, m_races.Count);
            SyncSelectedMainRaceGroupFromSelection();
            RefreshView();
        }

        private void OnClickMainRaceOption(int index, string groupKey, bool hasSubRaces)
        {
            m_centerOptionMode = CharacterCreationOptionPanelMode.Race;
            m_selectedMainRaceGroupKey = groupKey ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(groupKey))
            {
                m_expandedRaceGroups[groupKey] = hasSubRaces && !IsRaceGroupExpanded(groupKey);
            }

            RefreshView();
        }

        private void OnClickSelectClassOption(int index)
        {
            m_centerOptionMode = CharacterCreationOptionPanelMode.Class;
            m_selectedClassIndex = ClampIndex(index, m_classes.Count);
            RefreshView();
        }

        private void OnClickSelectBackgroundOption(int index)
        {
            m_centerOptionMode = CharacterCreationOptionPanelMode.Background;
            m_selectedBackgroundIndex = ClampIndex(index, m_backgrounds.Count);
            RefreshView();
        }

        private void OnClickSelectFeatOption(int index)
        {
            m_centerOptionMode = CharacterCreationOptionPanelMode.Feat;
            m_selectedFeatIndex = ClampIndex(index, m_feats.Count);
            RefreshView();
        }

        private void OnClickSelectSpellOption(int index)
        {
            m_centerOptionMode = CharacterCreationOptionPanelMode.Spell;
            m_selectedSpellIndex = ClampIndex(index, m_spells.Count);
            RefreshView();
        }

        private string BuildRaceOptionSubtitle(DndRaceDefineData raceData, bool isSubRace = false)
        {
            string size = string.IsNullOrWhiteSpace(raceData?.Size) ? "体型未定" : raceData.Size;
            return isSubRace
                ? $"亚种 / {size} / 速度 {raceData?.Speed ?? 0}"
                : $"{size} / 速度 {raceData?.Speed ?? 0}";
        }

        private string BuildRaceOptionDetail(DndRaceDefineData raceData)
        {
            StringBuilder builder = new StringBuilder(256);
            AppendLine(builder, "属性加值", BuildRaceAbilityBonusSummary(raceData));
            AppendLine(builder, "语言", FormatList(raceData?.LanguageIds));
            AppendLine(builder, "特性", BuildFeatureSummary(raceData?.FeatureIds));
            AppendLine(builder, "说明", SummarizeText(raceData?.Description));
            return builder.ToString().TrimEnd();
        }

        private string BuildClassOptionSubtitle(DndClassDefineData classData)
        {
            string primaryAbilities = FormatList(classData?.PrimaryAbilityIds);
            return $"生命骰 d{Math.Max(0, classData?.HitDie ?? 0)} / 主属性 {primaryAbilities}";
        }

        private string BuildClassOptionDetail(DndClassDefineData classData)
        {
            StringBuilder builder = new StringBuilder(256);
            AppendLine(builder, "豁免熟练", FormatList(classData?.SavingThrowProficiencies));
            AppendLine(builder, "护甲熟练", FormatList(classData?.ArmorProficiencies));
            AppendLine(builder, "武器熟练", FormatList(classData?.WeaponProficiencies));
            AppendLine(builder, "说明", SummarizeText(classData?.Description));
            return builder.ToString().TrimEnd();
        }

        private string BuildBackgroundOptionSubtitle(DndBackgroundDefineData backgroundData)
        {
            return $"技能 {FormatList(backgroundData?.SkillProficiencies)}";
        }

        private string BuildBackgroundOptionDetail(DndBackgroundDefineData backgroundData)
        {
            StringBuilder builder = new StringBuilder(256);
            AppendLine(builder, "技能熟练", FormatList(backgroundData?.SkillProficiencies));
            AppendLine(builder, "工具熟练", FormatList(backgroundData?.ToolProficiencies));
            AppendLine(builder, "语言", FormatList(backgroundData?.LanguageIds));
            AppendLine(builder, "特性", BuildFeatureSummary(backgroundData?.FeatureIds));
            AppendLine(builder, "说明", SummarizeText(backgroundData?.Description));
            return builder.ToString().TrimEnd();
        }

        private string BuildFeatOptionSubtitle(DndFeatDefineData featData)
        {
            return $"前置条件 {FormatList(featData?.PrerequisiteIds)}";
        }

        private string BuildFeatOptionDetail(DndFeatDefineData featData)
        {
            StringBuilder builder = new StringBuilder(256);
            AppendLine(builder, "授予特性", BuildFeatureSummary(featData?.FeatureIds));
            AppendLine(builder, "说明", SummarizeText(featData?.Description));
            return builder.ToString().TrimEnd();
        }

        private string BuildSpellOptionSubtitle(DndSpellDefineData spellData)
        {
            return $"{FormatSpellLevel(spellData?.Level ?? 0)} / {FormatTextOrDefault(spellData?.School, "未定学派")}";
        }

        private string BuildSpellOptionDetail(DndSpellDefineData spellData)
        {
            StringBuilder builder = new StringBuilder(256);
            AppendLine(builder, "施法", FormatTextOrDefault(spellData?.CastingTime, "未定"));
            AppendLine(builder, "距离/持续", $"{FormatTextOrDefault(spellData?.Range, "未定")} / {FormatTextOrDefault(spellData?.Duration, "未定")}");
            AppendLine(builder, "说明", SummarizeText(spellData?.Description));
            return builder.ToString().TrimEnd();
        }

        private void BuildVisibleRaceOptionEntries()
        {
            m_visibleRaceOptionEntries.Clear();
            List<RaceGroupInfo> groups = BuildRaceGroups();
            for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
            {
                RaceGroupInfo group = groups[groupIndex];
                if (group.IsGrouped)
                {
                    int mainRaceIndex = group.ChildRaceIndexes.Count > 0 ? group.ChildRaceIndexes[0] : -1;
                    if (mainRaceIndex < 0)
                    {
                        continue;
                    }

                    if (!m_expandedRaceGroups.ContainsKey(group.GroupKey) && group.ChildRaceIndexes.Contains(m_selectedRaceIndex))
                    {
                        m_expandedRaceGroups[group.GroupKey] = true;
                    }

                    m_visibleRaceOptionEntries.Add(RaceOptionEntry.CreateRace(mainRaceIndex, false, group.GroupKey, group.GroupName));
                    if (IsRaceGroupExpanded(group.GroupKey))
                    {
                        for (int childIndex = 0; childIndex < group.ChildRaceIndexes.Count; childIndex++)
                        {
                            int raceIndex = group.ChildRaceIndexes[childIndex];
                            m_visibleRaceOptionEntries.Add(RaceOptionEntry.CreateRace(raceIndex, true, group.GroupKey));
                        }
                    }

                    continue;
                }

                if (group.ChildRaceIndexes.Count > 0)
                {
                    m_visibleRaceOptionEntries.Add(RaceOptionEntry.CreateRace(group.ChildRaceIndexes[0], false, string.Empty));
                }
            }
        }
        private void ApplyCreationOptionLayout(int optionCount)
        {
            if (m_creationOptionGridLayout == null)
            {
                return;
            }

            if (m_creationOptionContentFitter != null)
            {
                m_creationOptionContentFitter.enabled = true;
            }

            m_creationOptionGridLayout.enabled = true;
            m_creationOptionGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            m_creationOptionGridLayout.constraintCount = 1;
            float availableWidth = m_rectCreationOptionViewport != null && m_rectCreationOptionViewport.rect.width > 0f
                ? m_rectCreationOptionViewport.rect.width
                : m_creationOptionOriginalCellSize.x;
            int leftPadding = m_creationOptionOriginalPadding != null ? m_creationOptionOriginalPadding.left : 6;
            int rightPadding = m_creationOptionOriginalPadding != null ? m_creationOptionOriginalPadding.right : 6;
            float cellWidth = Mathf.Max(240f, availableWidth - leftPadding - rightPadding);
            m_creationOptionGridLayout.cellSize = new Vector2(cellWidth, m_creationOptionOriginalCellSize.y);
            m_creationOptionGridLayout.spacing = m_creationOptionOriginalSpacing;
            if (m_creationOptionOriginalPadding != null)
            {
                m_creationOptionGridLayout.padding = new RectOffset(
                    m_creationOptionOriginalPadding.left,
                    m_creationOptionOriginalPadding.right,
                    m_creationOptionOriginalPadding.top,
                    m_creationOptionOriginalPadding.bottom);
            }

            for (int index = 0; index < optionCount && index < m_creationOptionCardViews.Count; index++)
            {
                m_creationOptionCardViews[index].ResetLayout();
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(m_rectCreationOptionCardContent);
        }

        private List<RaceGroupInfo> BuildRaceGroups()
        {
            DndRuleContentService service = DndRuleContentService.Instance;
            if (service.RaceMains.Count > 0)
            {
                return BuildRaceGroupsFromRaceMainData(service);
            }

            Dictionary<string, List<int>> prefixBuckets = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < m_races.Count; index++)
            {
                if (!TryGetRaceGroupPrefix(m_races[index], out string prefix))
                {
                    continue;
                }

                if (!prefixBuckets.TryGetValue(prefix, out List<int> list))
                {
                    list = new List<int>();
                    prefixBuckets[prefix] = list;
                }

                list.Add(index);
            }

            Dictionary<string, RaceGroupInfo> groupedInfoByPrefix = new Dictionary<string, RaceGroupInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, List<int>> pair in prefixBuckets)
            {
                if (pair.Value.Count < 2)
                {
                    continue;
                }

                string groupName = BuildRaceGroupName(pair.Value);
                if (string.IsNullOrWhiteSpace(groupName))
                {
                    continue;
                }

                RaceGroupInfo groupInfo = new RaceGroupInfo(pair.Key, groupName, pair.Value, true);
                groupedInfoByPrefix[pair.Key] = groupInfo;
                m_mainRacePreviewDataByGroupKey[pair.Key] = new MainRacePreviewData(
                    groupInfo.GroupKey,
                    groupInfo.GroupName,
                    string.Empty,
                    0,
                    new List<string>(),
                    new List<string>(),
                    string.Empty);
            }

            List<RaceGroupInfo> groups = new List<RaceGroupInfo>();
            HashSet<string> addedGroupedPrefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < m_races.Count; index++)
            {
                if (TryGetRaceGroupPrefix(m_races[index], out string prefix) && groupedInfoByPrefix.TryGetValue(prefix, out RaceGroupInfo groupInfo))
                {
                    if (addedGroupedPrefixes.Add(prefix))
                    {
                        groups.Add(groupInfo);
                    }

                    continue;
                }

                groups.Add(new RaceGroupInfo(m_races[index].RaceId, m_races[index].Name, new List<int> { index }, false));
            }

            return groups;
        }

        private List<RaceGroupInfo> BuildRaceGroupsFromRaceMainData(DndRuleContentService service)
        {
            List<RaceGroupInfo> groups = new List<RaceGroupInfo>();
            m_mainRacePreviewDataByGroupKey.Clear();

            for (int mainIndex = 0; mainIndex < service.RaceMains.Count; mainIndex++)
            {
                DndRaceMainDefineData raceMain = service.RaceMains[mainIndex];
                if (raceMain == null || string.IsNullOrWhiteSpace(raceMain.MainRaceId))
                {
                    continue;
                }

                List<int> childRaceIndexes = FindRaceIndexesByMainRace(raceMain.MainRaceId);
                bool isGrouped = childRaceIndexes.Count > 1;
                string groupName = string.IsNullOrWhiteSpace(raceMain.Name) ? raceMain.MainRaceId : raceMain.Name;
                groups.Add(new RaceGroupInfo(raceMain.MainRaceId, groupName, childRaceIndexes, isGrouped));
                m_mainRacePreviewDataByGroupKey[raceMain.MainRaceId] = new MainRacePreviewData(
                    raceMain.MainRaceId,
                    groupName,
                    raceMain.Size,
                    raceMain.Speed,
                    new List<string>(raceMain.LanguageIds),
                    new List<string>(raceMain.MainFeatureIds),
                    raceMain.Description);
            }

            HashSet<int> usedRaceIndexes = new HashSet<int>();
            for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
            {
                for (int childIndex = 0; childIndex < groups[groupIndex].ChildRaceIndexes.Count; childIndex++)
                {
                    usedRaceIndexes.Add(groups[groupIndex].ChildRaceIndexes[childIndex]);
                }
            }

            for (int raceIndex = 0; raceIndex < m_races.Count; raceIndex++)
            {
                if (usedRaceIndexes.Contains(raceIndex))
                {
                    continue;
                }

                groups.Add(new RaceGroupInfo(m_races[raceIndex].RaceId, m_races[raceIndex].Name, new List<int> { raceIndex }, false));
            }

            return groups;
        }

        private List<int> FindRaceIndexesByMainRace(string mainRaceId)
        {
            List<int> indexes = new List<int>();
            if (string.IsNullOrWhiteSpace(mainRaceId))
            {
                return indexes;
            }

            for (int index = 0; index < m_races.Count; index++)
            {
                if (IsRaceInMainGroup(m_races[index], mainRaceId))
                {
                    indexes.Add(index);
                }
            }

            return indexes;
        }

        private bool IsRaceInMainGroup(DndRaceDefineData raceData, string mainRaceId)
        {
            if (raceData == null || string.IsNullOrWhiteSpace(mainRaceId))
            {
                return false;
            }

            if (string.Equals(raceData.RaceId, mainRaceId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return raceData.RaceId.StartsWith(mainRaceId + "_", StringComparison.OrdinalIgnoreCase);
        }

        private void ToggleRaceGroup(string groupKey)
        {
            if (string.IsNullOrWhiteSpace(groupKey))
            {
                return;
            }

            bool expanded = IsRaceGroupExpanded(groupKey);
            m_expandedRaceGroups[groupKey] = !expanded;
            RefreshCreationOptionCards();
        }

        private bool IsRaceGroupExpanded(string groupKey)
        {
            return !string.IsNullOrWhiteSpace(groupKey)
                && m_expandedRaceGroups.TryGetValue(groupKey, out bool expanded)
                && expanded;
        }

        private int BuildRaceGroupChildCount(string groupKey)
        {
            if (string.IsNullOrWhiteSpace(groupKey))
            {
                return 0;
            }

            List<RaceGroupInfo> groups = BuildRaceGroups();
            for (int index = 0; index < groups.Count; index++)
            {
                if (string.Equals(groups[index].GroupKey, groupKey, StringComparison.OrdinalIgnoreCase))
                {
                    return groups[index].ChildRaceIndexes.Count;
                }
            }

            return 0;
        }

        private string BuildRaceGroupDetail(IReadOnlyList<int> raceIndexes)
        {
            if (raceIndexes == null || raceIndexes.Count == 0)
            {
                return "无亚种数据";
            }

            List<string> labels = new List<string>();
            for (int index = 0; index < raceIndexes.Count; index++)
            {
                int raceIndex = raceIndexes[index];
                if (raceIndex >= 0 && raceIndex < m_races.Count && !string.IsNullOrWhiteSpace(m_races[raceIndex].Name))
                {
                    labels.Add(m_races[raceIndex].Name);
                }
            }

            return labels.Count > 0 ? string.Join(" / ", labels) : "无亚种数据";
        }

        private bool TryGetRaceGroupPrefix(DndRaceDefineData raceData, out string prefix)
        {
            prefix = string.Empty;
            if (raceData == null || string.IsNullOrWhiteSpace(raceData.RaceId))
            {
                return false;
            }

            int separatorIndex = raceData.RaceId.IndexOf('_');
            if (separatorIndex <= 0 || separatorIndex >= raceData.RaceId.Length - 1)
            {
                return false;
            }

            prefix = raceData.RaceId.Substring(0, separatorIndex).Trim();
            return !string.IsNullOrWhiteSpace(prefix);
        }

        private string BuildRaceGroupName(IReadOnlyList<int> raceIndexes)
        {
            List<string> names = new List<string>();
            for (int index = 0; index < raceIndexes.Count; index++)
            {
                int raceIndex = raceIndexes[index];
                if (raceIndex >= 0 && raceIndex < m_races.Count && !string.IsNullOrWhiteSpace(m_races[raceIndex].Name))
                {
                    names.Add(m_races[raceIndex].Name.Trim());
                }
            }

            if (names.Count < 2)
            {
                return string.Empty;
            }

            string suffix = GetLongestCommonSuffix(names);
            if (suffix.Length >= 2)
            {
                return suffix;
            }

            string prefix = GetLongestCommonPrefix(names);
            return prefix.Length >= 2 ? prefix : string.Empty;
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

        private bool TryAppendSelectedMainRaceDetail(StringBuilder builder)
        {
            if (builder == null
                || m_centerOptionMode != CharacterCreationOptionPanelMode.Race
                || string.IsNullOrWhiteSpace(m_selectedMainRaceGroupKey))
            {
                return false;
            }

            if (!TryGetMainRacePreviewData(m_selectedMainRaceGroupKey, out MainRacePreviewData previewData))
            {
                return false;
            }

            AppendSection(builder, "种族", previewData.Name);
            AppendLine(builder, "体型", FormatTextOrDefault(previewData.Size, "未定"));
            AppendLine(builder, "速度", previewData.Speed > 0 ? previewData.Speed.ToString() : "未定");
            AppendLine(builder, "语言", FormatList(previewData.LanguageIds));
            AppendLine(builder, "特性", BuildFeatureDetailSummary(previewData.FeatureIds));
            AppendLine(builder, "描述", FormatTextOrDefault(previewData.Description, "无"));
            return true;
        }

        private bool TryGetMainRacePreviewData(string groupKey, out MainRacePreviewData previewData)
        {
            previewData = null;
            if (string.IsNullOrWhiteSpace(groupKey))
            {
                return false;
            }

            if (m_mainRacePreviewDataByGroupKey.TryGetValue(groupKey, out previewData))
            {
                return previewData != null;
            }

            return false;
        }

        private bool TryGetRaceGroupInfo(string groupKey, out RaceGroupInfo groupInfo)
        {
            List<RaceGroupInfo> groups = BuildRaceGroups();
            for (int index = 0; index < groups.Count; index++)
            {
                if (string.Equals(groups[index].GroupKey, groupKey, StringComparison.OrdinalIgnoreCase))
                {
                    groupInfo = groups[index];
                    return true;
                }
            }

            groupInfo = null;
            return false;
        }

        private List<DndRaceDefineData> GetGroupedRaces(RaceGroupInfo groupInfo)
        {
            List<DndRaceDefineData> groupedRaces = new List<DndRaceDefineData>();
            if (groupInfo == null || groupInfo.ChildRaceIndexes == null)
            {
                return groupedRaces;
            }

            for (int index = 0; index < groupInfo.ChildRaceIndexes.Count; index++)
            {
                if (TryGetSelected(m_races, groupInfo.ChildRaceIndexes[index], out DndRaceDefineData raceData))
                {
                    groupedRaces.Add(raceData);
                }
            }

            return groupedRaces;
        }

        private string BuildGroupedRaceSizeAndSpeed(IReadOnlyList<DndRaceDefineData> groupedRaces)
        {
            string size = GetSharedStringValue(groupedRaces, race => race.Size);
            int speed = GetSharedIntValue(groupedRaces, race => race.Speed);
            return $"{FormatTextOrDefault(size, "未定")} / {(speed > 0 ? speed.ToString() : "未定")}";
        }

        private string BuildGroupedRaceSpeed(IReadOnlyList<DndRaceDefineData> groupedRaces)
        {
            int speed = GetSharedIntValue(groupedRaces, race => race.Speed);
            return speed > 0 ? speed.ToString() : "未定";
        }

        private string BuildGroupedRaceLanguages(IReadOnlyList<DndRaceDefineData> groupedRaces)
        {
            return FormatList(GetCommonValues(groupedRaces, race => race.LanguageIds));
        }

        private string BuildGroupedRaceAbilityBonusSummary(IReadOnlyList<DndRaceDefineData> groupedRaces)
        {
            if (groupedRaces == null || groupedRaces.Count == 0)
            {
                return "无";
            }

            int[] sharedBonuses = null;
            for (int index = 0; index < groupedRaces.Count; index++)
            {
                int[] bonuses = new int[6];
                ApplyRaceAbilityBonuses(groupedRaces[index], bonuses);
                if (sharedBonuses == null)
                {
                    sharedBonuses = bonuses;
                    continue;
                }

                for (int abilityIndex = 0; abilityIndex < sharedBonuses.Length; abilityIndex++)
                {
                    if (sharedBonuses[abilityIndex] != bonuses[abilityIndex])
                    {
                        sharedBonuses[abilityIndex] = 0;
                    }
                }
            }

            if (sharedBonuses == null)
            {
                return "无";
            }

            List<string> summaries = new List<string>();
            AddAbilityBonusSummary(summaries, "力量", sharedBonuses[0]);
            AddAbilityBonusSummary(summaries, "敏捷", sharedBonuses[1]);
            AddAbilityBonusSummary(summaries, "体质", sharedBonuses[2]);
            AddAbilityBonusSummary(summaries, "智力", sharedBonuses[3]);
            AddAbilityBonusSummary(summaries, "感知", sharedBonuses[4]);
            AddAbilityBonusSummary(summaries, "魅力", sharedBonuses[5]);
            return summaries.Count > 0 ? string.Join(" / ", summaries) : "无";
        }

        private string BuildGroupedRaceFeatureSummary(IReadOnlyList<DndRaceDefineData> groupedRaces)
        {
            if (!string.IsNullOrWhiteSpace(m_selectedMainRaceGroupKey)
                && TryGetMainRacePreviewData(m_selectedMainRaceGroupKey, out MainRacePreviewData previewData))
            {
                return BuildFeatureSummary(previewData.FeatureIds);
            }

            List<string> commonFeatureNames = GetCommonValues(
                groupedRaces,
                race => BuildFeatureDisplayNames(race.FeatureIds));
            return FormatList(commonFeatureNames);
        }

        private string BuildGroupedRaceFeatureDetailSummary(IReadOnlyList<DndRaceDefineData> groupedRaces)
        {
            if (!string.IsNullOrWhiteSpace(m_selectedMainRaceGroupKey)
                && TryGetMainRacePreviewData(m_selectedMainRaceGroupKey, out MainRacePreviewData previewData))
            {
                return BuildFeatureDetailSummary(previewData.FeatureIds);
            }

            List<string> commonFeatureNames = GetCommonValues(
                groupedRaces,
                race => BuildFeatureDetailEntries(race.FeatureIds));
            return FormatFeatureDetailEntries(commonFeatureNames);
        }

        private string BuildGroupedRaceSubRaceSummary(IReadOnlyList<DndRaceDefineData> groupedRaces)
        {
            List<string> names = new List<string>();
            if (groupedRaces == null)
            {
                return "无";
            }

            for (int index = 0; index < groupedRaces.Count; index++)
            {
                string name = groupedRaces[index]?.Name;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    names.Add(name.Trim());
                }
            }

            return FormatList(names);
        }

        private string BuildGroupedRaceDescription(IReadOnlyList<DndRaceDefineData> groupedRaces)
        {
            return SummarizeText(BuildGroupedRaceSubRaceSummary(groupedRaces));
        }

        private string BuildFeatureDetailSummary(IReadOnlyList<string> featureIds)
        {
            return FormatFeatureDetailEntries(BuildFeatureDetailEntries(featureIds));
        }

        private List<string> BuildFeatureDetailEntries(IReadOnlyList<string> featureIds)
        {
            List<string> entries = new List<string>();
            if (featureIds == null)
            {
                return entries;
            }

            DndRuleContentService service = DndRuleContentService.Instance;
            for (int index = 0; index < featureIds.Count; index++)
            {
                string featureId = featureIds[index];
                if (string.IsNullOrWhiteSpace(featureId))
                {
                    continue;
                }

                if (service.TryGetFeature(featureId, out DndFeatureDefineData feature))
                {
                    string featureName = string.IsNullOrWhiteSpace(feature.Name) ? featureId : feature.Name.Trim();
                    string featureDescription = FormatTextOrDefault(feature.Description, "无");
                    string entry = $"{featureName}:{featureDescription}";
                    if (!ContainsIgnoreCase(entries, entry))
                    {
                        entries.Add(entry);
                    }
                    continue;
                }

                if (!ContainsIgnoreCase(entries, featureId))
                {
                    entries.Add(featureId);
                }
            }

            return entries;
        }

        private static string FormatFeatureDetailEntries(IReadOnlyList<string> entries)
        {
            if (entries == null || entries.Count == 0)
            {
                return "无";
            }

            return string.Join("\n", entries);
        }

        private List<string> BuildFeatureDisplayNames(IReadOnlyList<string> featureIds)
        {
            List<string> names = new List<string>();
            if (featureIds == null)
            {
                return names;
            }

            DndRuleContentService service = DndRuleContentService.Instance;
            for (int index = 0; index < featureIds.Count; index++)
            {
                string featureId = featureIds[index];
                if (string.IsNullOrWhiteSpace(featureId))
                {
                    continue;
                }

                string name = featureId;
                if (service.TryGetFeature(featureId, out DndFeatureDefineData feature) && !string.IsNullOrWhiteSpace(feature.Name))
                {
                    name = feature.Name.Trim();
                }

                if (!ContainsIgnoreCase(names, name))
                {
                    names.Add(name);
                }
            }

            return names;
        }

        private string BuildMainRaceCardDetail(string groupKey, string fallbackDetail)
        {
            if (TryGetMainRacePreviewData(groupKey, out MainRacePreviewData previewData))
            {
                StringBuilder previewBuilder = new StringBuilder(256);
                AppendLine(previewBuilder, "语言", FormatList(previewData.LanguageIds));
                AppendLine(previewBuilder, "特性", BuildFeatureSummary(previewData.FeatureIds));
                if (TryGetRaceGroupInfo(groupKey, out RaceGroupInfo previewGroupInfo))
                {
                    AppendLine(previewBuilder, "亚种", BuildGroupedRaceSubRaceSummary(GetGroupedRaces(previewGroupInfo)));
                }

                return previewBuilder.ToString().TrimEnd();
            }

            if (!TryGetRaceGroupInfo(groupKey, out RaceGroupInfo groupInfo) || !groupInfo.IsGrouped)
            {
                return fallbackDetail;
            }

            List<DndRaceDefineData> groupedRaces = GetGroupedRaces(groupInfo);
            if (groupedRaces.Count == 0)
            {
                return fallbackDetail;
            }

            StringBuilder builder = new StringBuilder(256);
            AppendLine(builder, "属性加值", BuildGroupedRaceAbilityBonusSummary(groupedRaces));
            AppendLine(builder, "语言", BuildGroupedRaceLanguages(groupedRaces));
            AppendLine(builder, "特性", BuildGroupedRaceFeatureSummary(groupedRaces));
            AppendLine(builder, "亚种", BuildGroupedRaceSubRaceSummary(groupedRaces));
            return builder.ToString().TrimEnd();
        }

        private List<string> GetCommonValues(IReadOnlyList<DndRaceDefineData> groupedRaces, Func<DndRaceDefineData, IReadOnlyList<string>> selector)
        {
            List<string> commonValues = new List<string>();
            if (groupedRaces == null || groupedRaces.Count == 0 || selector == null)
            {
                return commonValues;
            }

            IReadOnlyList<string> firstValues = selector(groupedRaces[0]);
            if (firstValues == null)
            {
                return commonValues;
            }

            for (int valueIndex = 0; valueIndex < firstValues.Count; valueIndex++)
            {
                string value = firstValues[valueIndex];
                if (string.IsNullOrWhiteSpace(value) || ContainsIgnoreCase(commonValues, value))
                {
                    continue;
                }

                bool existsInAll = true;
                for (int raceIndex = 1; raceIndex < groupedRaces.Count; raceIndex++)
                {
                    IReadOnlyList<string> values = selector(groupedRaces[raceIndex]);
                    if (!ContainsIgnoreCase(values, value))
                    {
                        existsInAll = false;
                        break;
                    }
                }

                if (existsInAll)
                {
                    commonValues.Add(value);
                }
            }

            return commonValues;
        }

        private string GetSharedStringValue(IReadOnlyList<DndRaceDefineData> groupedRaces, Func<DndRaceDefineData, string> selector)
        {
            if (groupedRaces == null || groupedRaces.Count == 0 || selector == null)
            {
                return string.Empty;
            }

            string value = selector(groupedRaces[0]) ?? string.Empty;
            for (int index = 1; index < groupedRaces.Count; index++)
            {
                if (!string.Equals(value, selector(groupedRaces[index]) ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                {
                    return string.Empty;
                }
            }

            return value;
        }

        private int GetSharedIntValue(IReadOnlyList<DndRaceDefineData> groupedRaces, Func<DndRaceDefineData, int> selector)
        {
            if (groupedRaces == null || groupedRaces.Count == 0 || selector == null)
            {
                return 0;
            }

            int value = selector(groupedRaces[0]);
            for (int index = 1; index < groupedRaces.Count; index++)
            {
                if (value != selector(groupedRaces[index]))
                {
                    return 0;
                }
            }

            return value;
        }

        private static bool ContainsIgnoreCase(IReadOnlyList<string> values, string target)
        {
            if (values == null || string.IsNullOrWhiteSpace(target))
            {
                return false;
            }

            for (int index = 0; index < values.Count; index++)
            {
                if (string.Equals(values[index], target, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
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

            character.CharacterName = GetInputValue(m_tmpInputCharacterName, character.CharacterName);
            character.Alignment = GetAlignmentDropdownText(m_tmpDropdownAlignment != null ? m_tmpDropdownAlignment.value : -1);
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

        private void RefreshCharacterBasicInfoInputs()
        {
            m_isRefreshingCharacterInputs = true;
            try
            {
                if (m_tmpInputCharacterName != null)
                {
                    string characterName = m_currentCharacter == null || string.IsNullOrWhiteSpace(m_currentCharacter.CharacterName)
                        ? "未命名角色"
                        : m_currentCharacter.CharacterName;
                    m_tmpInputCharacterName.SetTextWithoutNotify(characterName);
                }

                if (m_tmpDropdownAlignment != null)
                {
                    int alignmentIndex = FindAlignmentDropdownIndex(m_currentCharacter?.Alignment);
                    m_tmpDropdownAlignment.SetValueWithoutNotify(alignmentIndex);
                    m_tmpDropdownAlignment.RefreshShownValue();
                }
            }
            finally
            {
                m_isRefreshingCharacterInputs = false;
            }
        }

        private string NormalizeAlignmentText(string alignment)
        {
            return string.IsNullOrWhiteSpace(alignment) ? string.Empty : alignment.Trim();
        }

        private static string GetInputValue(TMP_InputField inputField, string fallback)
        {
            if (inputField == null)
            {
                return string.IsNullOrWhiteSpace(fallback) ? "未命名角色" : fallback.Trim();
            }

            return string.IsNullOrWhiteSpace(inputField.text) ? "未命名角色" : inputField.text.Trim();
        }

        private int FindAlignmentDropdownIndex(string alignment)
        {
            if (m_tmpDropdownAlignment == null || m_tmpDropdownAlignment.options == null || m_tmpDropdownAlignment.options.Count == 0)
            {
                return 0;
            }

            alignment = NormalizeAlignmentText(alignment);
            if (string.IsNullOrWhiteSpace(alignment))
            {
                return 0;
            }

            for (int index = 0; index < m_tmpDropdownAlignment.options.Count; index++)
            {
                if (string.Equals(m_tmpDropdownAlignment.options[index].text, alignment, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }

                if (index > 0 && index - 1 < m_alignmentOptions.Count && string.Equals(m_alignmentOptions[index - 1], alignment, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return 0;
        }

        private string GetAlignmentDropdownText(int index)
        {
            if (m_tmpDropdownAlignment == null || m_tmpDropdownAlignment.options == null || index <= 0 || index >= m_tmpDropdownAlignment.options.Count)
            {
                return string.Empty;
            }

            if (index - 1 >= 0 && index - 1 < m_alignmentOptions.Count)
            {
                return m_alignmentOptions[index - 1];
            }

            return m_tmpDropdownAlignment.options[index].text ?? string.Empty;
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

        private static string FormatTextOrDefault(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private static string SummarizeText(string value, int maxLength = 80)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "无";
            }

            string compact = value.Replace("\r", " ").Replace("\n", " ").Trim();
            return compact.Length <= maxLength
                ? compact
                : $"{compact.Substring(0, maxLength).TrimEnd()}...";
        }

        private static string GetLongestCommonPrefix(IReadOnlyList<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return string.Empty;
            }

            string prefix = values[0] ?? string.Empty;
            for (int index = 1; index < values.Count; index++)
            {
                prefix = GetCommonPrefix(prefix, values[index] ?? string.Empty);
                if (prefix.Length == 0)
                {
                    return string.Empty;
                }
            }

            return prefix.Trim();
        }

        private static string GetLongestCommonSuffix(IReadOnlyList<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return string.Empty;
            }

            string suffix = values[0] ?? string.Empty;
            for (int index = 1; index < values.Count; index++)
            {
                suffix = GetCommonSuffix(suffix, values[index] ?? string.Empty);
                if (suffix.Length == 0)
                {
                    return string.Empty;
                }
            }

            return suffix.Trim();
        }

        private static string GetCommonPrefix(string left, string right)
        {
            int maxLength = Math.Min(left.Length, right.Length);
            int index = 0;
            while (index < maxLength && left[index] == right[index])
            {
                index++;
            }

            return index <= 0 ? string.Empty : left.Substring(0, index);
        }

        private static string GetCommonSuffix(string left, string right)
        {
            int leftIndex = left.Length - 1;
            int rightIndex = right.Length - 1;
            int matchLength = 0;
            while (leftIndex >= 0 && rightIndex >= 0 && left[leftIndex] == right[rightIndex])
            {
                leftIndex--;
                rightIndex--;
                matchLength++;
            }

            return matchLength <= 0 ? string.Empty : left.Substring(left.Length - matchLength, matchLength);
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
        private readonly Button m_btnEdit;

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
            Button button,
            Button btnEdit)
        {
            m_root = root;
            m_background = background;
            m_previewImage = previewImage;
            m_previewPlaceholder = previewPlaceholder;
            m_nameText = nameText;
            m_classText = classText;
            m_statusText = statusText;
            m_button = button;
            m_btnEdit = btnEdit;
        }

        public static CharacterCardListItemView BindTemplate(GameObject root)
        {
            Image background = root.GetComponent<Image>();
            Button button = root.GetComponent<Button>();
            Button btnEdit = root.transform.Find("m_btnEditCharacterCard")?.GetComponent<Button>();
            Image previewImage = root.transform.Find("m_imgPreview")?.GetComponent<Image>();
            TMP_Text placeholder = root.transform.Find("m_imgPreview/m_tmpPreviewPlaceholder")?.GetComponent<TMP_Text>();
            TMP_Text nameText = root.transform.Find("m_tmpCharacterName")?.GetComponent<TMP_Text>();
            TMP_Text classText = root.transform.Find("m_tmpCharacterClass")?.GetComponent<TMP_Text>();
            TMP_Text statusText = root.transform.Find("m_tmpCharacterStatus")?.GetComponent<TMP_Text>();

            return new CharacterCardListItemView(root, background, previewImage, placeholder, nameText, classText, statusText, button, btnEdit);
        }

        public void Bind(CharacterCardDraftSaveData character, string classLine, string statusLine, bool selected, Action clickAction, Action editAction)
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

            if (m_btnEdit != null)
            {
                m_btnEdit.onClick.RemoveAllListeners();
                m_btnEdit.onClick.AddListener(() => editAction?.Invoke());
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

    internal sealed class CharacterCreationOptionCardView
    {
        private readonly GameObject m_root;
        private readonly Image m_background;
        private readonly TMP_Text m_titleText;
        private readonly TMP_Text m_subtitleText;
        private readonly TMP_Text m_detailText;
        private readonly TMP_Text m_selectedBadge;
        private readonly Button m_button;
        private readonly GameObject m_editButtonObject;
        private readonly RectTransform m_rootRect;
        private readonly LayoutElement m_layoutElement;
        private readonly Vector2 m_originalOffsetMin;
        private readonly Vector2 m_originalOffsetMax;
        private readonly Vector2 m_originalAnchorMin;
        private readonly Vector2 m_originalAnchorMax;
        private readonly Vector2 m_originalPivot;
        private readonly Vector2 m_originalSizeDelta;
        private readonly Vector2 m_originalAnchoredPosition;

        private CharacterCreationOptionCardView(
            GameObject root,
            Image background,
            TMP_Text titleText,
            TMP_Text subtitleText,
            TMP_Text detailText,
            TMP_Text selectedBadge,
            Button button,
            GameObject editButtonObject,
            RectTransform rootRect,
            LayoutElement layoutElement)
        {
            m_root = root;
            m_background = background;
            m_titleText = titleText;
            m_subtitleText = subtitleText;
            m_detailText = detailText;
            m_selectedBadge = selectedBadge;
            m_button = button;
            m_editButtonObject = editButtonObject;
            m_rootRect = rootRect;
            m_layoutElement = layoutElement;
            if (m_rootRect != null)
            {
                m_originalOffsetMin = m_rootRect.offsetMin;
                m_originalOffsetMax = m_rootRect.offsetMax;
                m_originalAnchorMin = m_rootRect.anchorMin;
                m_originalAnchorMax = m_rootRect.anchorMax;
                m_originalPivot = m_rootRect.pivot;
                m_originalSizeDelta = m_rootRect.sizeDelta;
                m_originalAnchoredPosition = m_rootRect.anchoredPosition;
            }
        }

        public static CharacterCreationOptionCardView BindTemplate(GameObject root)
        {
            Image background = root.GetComponent<Image>();
            Button button = root.GetComponent<Button>();
            LayoutElement layoutElement = root.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = root.AddComponent<LayoutElement>();
            }
            TMP_Text titleText = root.transform.Find("m_tmpOptionTitle")?.GetComponent<TMP_Text>();
            TMP_Text subtitleText = root.transform.Find("m_tmpOptionSubtitle")?.GetComponent<TMP_Text>();
            TMP_Text detailText = root.transform.Find("m_tmpOptionDetail")?.GetComponent<TMP_Text>();
            TMP_Text selectedBadge = root.transform.Find("m_tmpSelectedBadge")?.GetComponent<TMP_Text>();
            GameObject editButtonObject = root.transform.Find("m_btnEditCharacterCard")?.gameObject;
            RectTransform rootRect = root.GetComponent<RectTransform>();

            if (editButtonObject != null)
            {
                editButtonObject.SetActive(false);
            }

            return new CharacterCreationOptionCardView(root, background, titleText, subtitleText, detailText, selectedBadge, button, editButtonObject, rootRect, layoutElement);
        }

        public void Bind(string title, string subtitle, string detail, bool selected, bool showSelectedBadge, CharacterCreationOptionCardStyle style, Action clickAction)
        {
            SetActive(true);
            ApplyStyle(style, selected);

            SetText(m_titleText, title);
            SetText(m_subtitleText, subtitle);
            SetText(m_detailText, detail);

            if (m_selectedBadge != null)
            {
                m_selectedBadge.gameObject.SetActive(showSelectedBadge && selected);
            }

            if (m_editButtonObject != null)
            {
                m_editButtonObject.SetActive(false);
            }

            if (m_button != null)
            {
                m_button.interactable = true;
                m_button.onClick.RemoveAllListeners();
                m_button.onClick.AddListener(() => clickAction?.Invoke());
            }
        }

        private void ApplyStyle(CharacterCreationOptionCardStyle style, bool selected)
        {
            if (m_background != null)
            {
                m_background.raycastTarget = true;
                switch (style)
                {
                    case CharacterCreationOptionCardStyle.GroupHeader:
                        m_background.color = selected
                            ? new Color(0.20f, 0.26f, 0.34f, 1f)
                            : new Color(0.15f, 0.19f, 0.25f, 0.98f);
                        break;
                    case CharacterCreationOptionCardStyle.SubRace:
                        m_background.color = selected
                            ? new Color(0.22f, 0.20f, 0.14f, 1f)
                            : new Color(0.16f, 0.15f, 0.11f, 0.96f);
                        break;
                    default:
                        m_background.color = selected
                            ? new Color(0.18f, 0.24f, 0.32f, 1f)
                            : new Color(0.12f, 0.15f, 0.19f, 0.96f);
                        break;
                }
            }

            if (m_titleText != null)
            {
                m_titleText.color = style == CharacterCreationOptionCardStyle.SubRace
                    ? new Color(0.98f, 0.90f, 0.70f, 1f)
                    : new Color(0.92f, 0.95f, 1f, 1f);
                m_titleText.fontStyle = style == CharacterCreationOptionCardStyle.GroupHeader ? FontStyles.Bold : FontStyles.Normal;
            }

            if (m_subtitleText != null)
            {
                m_subtitleText.color = style == CharacterCreationOptionCardStyle.SubRace
                    ? new Color(0.86f, 0.74f, 0.46f, 1f)
                    : new Color(0.94f, 0.80f, 0.46f, 1f);
                m_subtitleText.fontStyle = style == CharacterCreationOptionCardStyle.GroupHeader ? FontStyles.Bold : FontStyles.Normal;
            }

            if (m_detailText != null)
            {
                m_detailText.color = style == CharacterCreationOptionCardStyle.SubRace
                    ? new Color(0.82f, 0.80f, 0.72f, 1f)
                    : new Color(0.72f, 0.78f, 0.86f, 1f);
            }

            if (m_rootRect != null)
            {
                if (style == CharacterCreationOptionCardStyle.SubRace)
                {
                    m_rootRect.offsetMin = new Vector2(m_originalOffsetMin.x + 20f, m_originalOffsetMin.y);
                    m_rootRect.offsetMax = new Vector2(m_originalOffsetMax.x - 12f, m_originalOffsetMax.y);
                }
                else
                {
                    m_rootRect.offsetMin = m_originalOffsetMin;
                    m_rootRect.offsetMax = m_originalOffsetMax;
                }
            }
        }

        public void ResetLayout()
        {
            if (m_layoutElement != null)
            {
                m_layoutElement.ignoreLayout = false;
                m_layoutElement.minWidth = -1f;
                m_layoutElement.minHeight = -1f;
                m_layoutElement.preferredWidth = -1f;
                m_layoutElement.preferredHeight = -1f;
                m_layoutElement.flexibleWidth = -1f;
                m_layoutElement.flexibleHeight = -1f;
            }

            if (m_rootRect != null)
            {
                m_rootRect.anchorMin = m_originalAnchorMin;
                m_rootRect.anchorMax = m_originalAnchorMax;
                m_rootRect.pivot = m_originalPivot;
                m_rootRect.sizeDelta = m_originalSizeDelta;
                m_rootRect.anchoredPosition = m_originalAnchoredPosition;
                m_rootRect.offsetMin = m_originalOffsetMin;
                m_rootRect.offsetMax = m_originalOffsetMax;
            }
        }

        public void ApplyManualLayout(float x, float y, float width, float height)
        {
            if (m_layoutElement != null)
            {
                m_layoutElement.ignoreLayout = true;
            }

            if (m_rootRect == null)
            {
                return;
            }

            m_rootRect.anchorMin = new Vector2(0f, 1f);
            m_rootRect.anchorMax = new Vector2(0f, 1f);
            m_rootRect.pivot = new Vector2(0f, 1f);
            m_rootRect.anchoredPosition = new Vector2(x, -y);
            m_rootRect.sizeDelta = new Vector2(width, height);
        }

        public void SetActive(bool active)
        {
            if (m_root != null)
            {
                m_root.SetActive(active);
            }
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value ?? string.Empty;
            }
        }
    }

    internal sealed class MainRacePreviewData
    {
        public MainRacePreviewData(string groupKey, string name, string size, int speed, List<string> languageIds, List<string> featureIds, string description)
        {
            GroupKey = groupKey ?? string.Empty;
            Name = name ?? string.Empty;
            Size = size ?? string.Empty;
            Speed = speed;
            LanguageIds = languageIds ?? new List<string>();
            FeatureIds = featureIds ?? new List<string>();
            Description = description ?? string.Empty;
        }

        public string GroupKey { get; }

        public string Name { get; }

        public string Size { get; }

        public int Speed { get; }

        public List<string> LanguageIds { get; }

        public List<string> FeatureIds { get; }

        public string Description { get; }
    }

    internal sealed class RaceOptionEntry
    {
        public bool IsGroupHeader { get; private set; }

        public bool IsMainRace { get; private set; }

        public int RaceIndex { get; private set; } = -1;

        public bool IsSubRace { get; private set; }

        public string GroupKey { get; private set; } = string.Empty;

        public string GroupName { get; private set; } = string.Empty;

        public List<int> ChildRaceIndexes { get; private set; } = new List<int>();

        public static RaceOptionEntry CreateGroupHeader(string groupKey, string groupName, IReadOnlyList<int> childRaceIndexes)
        {
            RaceOptionEntry entry = new RaceOptionEntry
            {
                IsGroupHeader = true,
                GroupKey = groupKey ?? string.Empty,
                GroupName = groupName ?? string.Empty
            };

            if (childRaceIndexes != null)
            {
                entry.ChildRaceIndexes.AddRange(childRaceIndexes);
            }

            return entry;
        }

        public static RaceOptionEntry CreateRace(int raceIndex, bool isSubRace, string groupKey, string groupName = "")
        {
            return new RaceOptionEntry
            {
                IsGroupHeader = false,
                IsMainRace = !isSubRace && !string.IsNullOrWhiteSpace(groupKey),
                RaceIndex = raceIndex,
                IsSubRace = isSubRace,
                GroupKey = groupKey ?? string.Empty,
                GroupName = groupName ?? string.Empty
            };
        }
    }

    internal sealed class RaceGroupInfo
    {
        public RaceGroupInfo(string groupKey, string groupName, List<int> childRaceIndexes, bool isGrouped)
        {
            GroupKey = groupKey ?? string.Empty;
            GroupName = groupName ?? string.Empty;
            ChildRaceIndexes = childRaceIndexes ?? new List<int>();
            IsGrouped = isGrouped;
        }

        public string GroupKey { get; }

        public string GroupName { get; }

        public List<int> ChildRaceIndexes { get; }

        public bool IsGrouped { get; }
    }
}
