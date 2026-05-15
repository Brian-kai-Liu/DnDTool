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
        private TMP_InputField m_tmpInputCharacterLevel = null!;
        private TMP_InputField m_tmpHpValue = null!;
        private TMP_Dropdown m_tmpDropdownAlignment = null!;
        private RectTransform m_rectPanelLeft = null!;
        private RectTransform m_rectPanelCenter = null!;
        private GameObject m_goCardListRoot = null!;
        private GameObject m_goCreationOptionListRoot = null!;
        private GameObject m_goRowCharacterBase = null!;
        private GameObject m_goRowFeat = null!;
        private GameObject m_goRowSpell = null!;
        private RectTransform m_rectCreationOptionListRoot = null!;
        private RectTransform m_rectHpChoiceContent = null!;
        private RectTransform m_rectHpRollContent = null!;
        private RectTransform m_rectClassChoiceContent = null!;
        private RectTransform m_rectFeatChoiceContent = null!;
        private RectTransform m_rectSpellChoiceContent = null!;
        private Button m_btnHpChoiceTemplate = null!;
        private Button m_btnHpRoll = null!;
        private Button m_btnHpRollConfirm = null!;
        private Button m_btnClassChoiceTemplate = null!;
        private Button m_btnFeatChoiceTemplate = null!;
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
        private GameObject m_goPreviewDetailMainRaceBlock = null!;
        private GameObject m_goPreviewDetailSubRaceBlock = null!;
        private GameObject m_goPreviewDetailClassBlock = null!;
        private GameObject m_goPreviewDetailBackgroundBlock = null!;
        private RectTransform m_rectSelectionDetailContent = null!;
        private TMP_Text m_tmpPreviewMainRaceName = null!;
        private TMP_Text m_tmpPreviewBodySize = null!;
        private TMP_Text m_tmpPreviewSpeed = null!;
        private TMP_Text m_tmpPreviewLanguage = null!;
        private TMP_Text m_tmpPreviewFeatureTitle = null!;
        private TMP_Text m_tmpPreviewFeature = null!;
        private TMP_Text m_tmpPreviewRaceDes = null!;
        private TMP_Text m_tmpPreviewSubRaceName = null!;
        private TMP_Text m_tmpPreviewSubRaceBodySize = null!;
        private TMP_Text m_tmpPreviewSubRaceSpeed = null!;
        private TMP_Text m_tmpPreviewSubFeatureTitle = null!;
        private TMP_Text m_tmpPreviewSubRaceFeature = null!;
        private TMP_Text m_tmpPreviewSubRaceDes = null!;
        private TMP_Text m_tmpPreviewClassName = null!;
        private TMP_Text m_tmpPreviewClassHPDice = null!;
        private TMP_Text m_tmpPreviewClassMainAttri = null!;
        private TMP_Text m_tmpPreviewClassDC = null!;
        private TMP_Text m_tmpPreviewClassArmor = null!;
        private TMP_Text m_tmpPreviewClassWeapon = null!;
        private TMP_Text m_tmpPreviewClassTitle = null!;
        private TMP_Text m_tmpPreviewClassFeature = null!;
        private TMP_Text m_tmpPreviewBackgroundName = null!;
        private TMP_Text m_tmpPreviewBackgroundSkill = null!;
        private TMP_Text m_tmpPreviewBackgroundTools = null!;
        private TMP_Text m_tmpPreviewBackgroundLanguage = null!;
        private TMP_Text m_tmpPreviewBackgroundItem = null!;
        private TMP_Text m_tmpPreviewBackgroundFeatureTitle = null!;
        private TMP_Text m_tmpPreviewBackgroundFeature = null!;
        private TMP_Text m_tmpPreviewBackgroundDes = null!;
        private TMP_Text m_tmpHpRollLevel = null!;
        private TMP_Text m_tmpHpRollHitDie = null!;
        private TMP_Text m_tmpHpRollValue = null!;
        private TMP_Text m_tmpHpRollConModifier = null!;
        private TMP_Text m_tmpHpRollGain = null!;

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
        private readonly List<CharacterCardListItemView> m_cardViews = new List<CharacterCardListItemView>();
        private readonly List<CharacterCreationOptionCardView> m_creationOptionCardViews = new List<CharacterCreationOptionCardView>();

        private int m_selectedClassIndex;
        private int m_selectedRaceIndex;
        private int m_selectedBackgroundIndex;
        private int m_selectedFeatIndex;
        private int m_selectedSpellIndex;
        private int m_selectedListCharacterIndex = -1;
        private string m_selectedMainRaceGroupKey = string.Empty;
        private bool m_isRefreshingCharacterInputs;
        private int m_pendingHpRollLevel;
        private int m_pendingHpRollValue;
        private CharacterCardManagementMode m_mode = CharacterCardManagementMode.List;
        private CharacterCreationOptionPanelMode m_centerOptionMode = CharacterCreationOptionPanelMode.None;
        private CharacterCardDraftSaveData m_currentCharacter;

        protected override void ScriptGenerator()
        {
            m_btnBack = FindChildComponent<Button>("m_panelTopBar/m_btnBack");
            const string creationEditorRootPath = "m_panelLeft/m_scrollCreationEditor/Viewport/m_rectCreationEditorContent";
            m_btnPrevClass = FindChildComponent<Button>(creationEditorRootPath + "/m_rowClass/m_btnPrevClass");
            m_btnNextClass = FindChildComponent<Button>(creationEditorRootPath + "/m_rowClass/m_btnNextClass");
            m_btnPrevRace = FindChildComponent<Button>(creationEditorRootPath + "/m_rowRace/m_btnPrevRace");
            m_btnNextRace = FindChildComponent<Button>(creationEditorRootPath + "/m_rowRace/m_btnNextRace");
            m_btnPrevBackground = FindChildComponent<Button>(creationEditorRootPath + "/m_rowBackground/m_btnPrevBackground");
            m_btnNextBackground = FindChildComponent<Button>(creationEditorRootPath + "/m_rowBackground/m_btnNextBackground");
            m_btnPrevFeat = FindChildComponent<Button>(creationEditorRootPath + "/m_rowFeat/m_btnPrevFeat");
            m_btnNextFeat = FindChildComponent<Button>(creationEditorRootPath + "/m_rowFeat/m_btnNextFeat");
            m_btnPrevSpell = FindChildComponent<Button>(creationEditorRootPath + "/m_rowSpell/m_btnPrevSpell");
            m_btnNextSpell = FindChildComponent<Button>(creationEditorRootPath + "/m_rowSpell/m_btnNextSpell");
            m_btnCreateDraft = FindChildComponent<Button>("m_panelLeft/m_btnCreateDraft");
            m_tmpTitle = FindChildComponent<TMP_Text>("m_panelTopBar/m_tmpTitle");
            m_tmpRuleStatus = FindChildComponent<TMP_Text>("m_panelLeft/m_tmpRuleStatus");
            m_tmpClassLabel = FindChildComponent<TMP_Text>(creationEditorRootPath + "/m_rowClass/Label");
            m_tmpRaceLabel = FindChildComponent<TMP_Text>(creationEditorRootPath + "/m_rowRace/Label");
            m_tmpBackgroundLabel = FindChildComponent<TMP_Text>(creationEditorRootPath + "/m_rowBackground/Label");
            m_tmpFeatLabel = FindChildComponent<TMP_Text>(creationEditorRootPath + "/m_rowFeat/Label");
            m_tmpSpellLabel = FindChildComponent<TMP_Text>(creationEditorRootPath + "/m_rowSpell/Label");
            m_tmpClassValue = FindChildComponent<TMP_Text>(creationEditorRootPath + "/m_rowClass/m_tmpClassValue");
            m_tmpRaceValue = FindChildComponent<TMP_Text>(creationEditorRootPath + "/m_rowRace/m_tmpRaceValue");
            m_tmpBackgroundValue = FindChildComponent<TMP_Text>(creationEditorRootPath + "/m_rowBackground/m_tmpBackgroundValue");
            m_tmpFeatValue = FindChildComponent<TMP_Text>(creationEditorRootPath + "/m_rowFeat/m_tmpFeatValue");
            m_tmpSpellValue = FindChildComponent<TMP_Text>(creationEditorRootPath + "/m_rowSpell/m_tmpSpellValue");
            m_goRowCharacterBase = FindChild(creationEditorRootPath + "/m_rowCharacterBase")?.gameObject;
            m_goRowCharacterName = FindChild(creationEditorRootPath + "/m_rowCharacterName")?.gameObject;
            m_goRowAlignment = FindChild(creationEditorRootPath + "/m_rowAlignment")?.gameObject;
            m_goRowFeat = FindChild(creationEditorRootPath + "/m_rowFeat")?.gameObject;
            m_goRowSpell = FindChild(creationEditorRootPath + "/m_rowSpell")?.gameObject;
            m_tmpCharacterNameLabel = FindChildComponent<TMP_Text>(creationEditorRootPath + "/m_rowCharacterName/Label");
            m_tmpAlignmentLabel = FindChildComponent<TMP_Text>(creationEditorRootPath + "/m_rowAlignment/Label");
            m_tmpInputCharacterName = FindChildComponent<TMP_InputField>(creationEditorRootPath + "/m_rowCharacterName/m_tmpInputCharacterName");
            m_tmpInputCharacterLevel = FindChildComponent<TMP_InputField>(creationEditorRootPath + "/m_rowCharacterBase/m_rowCharacterLevel/m_tmpInputCharacterLevel");
            m_tmpHpValue = FindChildComponent<TMP_InputField>(creationEditorRootPath + "/m_rowCharacterBase/m_rowCharacterHp/m_tmpHpValue");
            m_rectHpRollContent = FindHpRollRoot(creationEditorRootPath)?.GetComponent<RectTransform>();
            m_tmpHpRollLevel = FindHpRollText("m_tmpHpRollLevel");
            m_tmpHpRollHitDie = FindHpRollText("m_tmpHpRollHitDie");
            m_tmpHpRollValue = FindHpRollText("m_tmpHpRollValue");
            m_tmpHpRollConModifier = FindHpRollText("m_tmpHpRollConModifier");
            m_tmpHpRollGain = FindHpRollText("m_tmpHpRollGain");
            m_btnHpRoll = FindHpRollButton("m_btnHpRoll");
            m_btnHpRollConfirm = FindHpRollButton("m_btnHpRollConfirm");
            m_tmpDropdownAlignment = FindChildComponent<TMP_Dropdown>(creationEditorRootPath + "/m_rowAlignment/m_tmpDropdownAlignment");
            m_rectHpChoiceContent = FindChildComponent<RectTransform>(creationEditorRootPath + "/m_rectHpChoiceContent");
            m_rectClassChoiceContent = FindChildComponent<RectTransform>(creationEditorRootPath + "/m_rectClassChoiceContent");
            m_rectFeatChoiceContent = FindChildComponent<RectTransform>(creationEditorRootPath + "/m_rectFeatChoiceContent");
            m_rectSpellChoiceContent = FindChildComponent<RectTransform>(creationEditorRootPath + "/m_rectSpellChoiceContent");
            m_btnHpChoiceTemplate = FindChildComponent<Button>(creationEditorRootPath + "/m_rectHpChoiceContent/m_btnChoiceTemplate");
            m_btnClassChoiceTemplate = FindChildComponent<Button>(creationEditorRootPath + "/m_rectClassChoiceContent/m_btnChoiceTemplate");
            m_btnFeatChoiceTemplate = FindChildComponent<Button>(creationEditorRootPath + "/m_rectFeatChoiceContent/m_btnChoiceTemplate");
            m_btnRowRace = FindChildComponent<Button>(creationEditorRootPath + "/m_rowRace");
            m_btnRowClass = FindChildComponent<Button>(creationEditorRootPath + "/m_rowClass");
            m_btnRowBackground = FindChildComponent<Button>(creationEditorRootPath + "/m_rowBackground");
            m_btnRowFeat = FindChildComponent<Button>(creationEditorRootPath + "/m_rowFeat");
            m_btnRowSpell = FindChildComponent<Button>(creationEditorRootPath + "/m_rowSpell");
            m_tmpPrevClassText = FindChildComponent<TMP_Text>(creationEditorRootPath + "/m_rowClass/m_btnPrevClass");
            m_tmpNextClassText = FindChildComponent<TMP_Text>(creationEditorRootPath + "/m_rowClass/m_btnNextClass");
            m_tmpPrevRaceText = FindChildComponent<TMP_Text>(creationEditorRootPath + "/m_rowRace/m_btnPrevRace");
            m_tmpNextRaceText = FindChildComponent<TMP_Text>(creationEditorRootPath + "/m_rowRace/m_btnNextRace");
            m_tmpPrevBackgroundText = FindChildComponent<TMP_Text>(creationEditorRootPath + "/m_rowBackground/m_btnPrevBackground");
            m_tmpNextBackgroundText = FindChildComponent<TMP_Text>(creationEditorRootPath + "/m_rowBackground/m_btnNextBackground");
            m_tmpPrevFeatText = FindChildComponent<TMP_Text>(creationEditorRootPath + "/m_rowFeat/m_btnPrevFeat");
            m_tmpNextFeatText = FindChildComponent<TMP_Text>(creationEditorRootPath + "/m_rowFeat/m_btnNextFeat");
            m_tmpPrevSpellText = FindChildComponent<TMP_Text>(creationEditorRootPath + "/m_rowSpell/m_btnPrevSpell");
            m_tmpNextSpellText = FindChildComponent<TMP_Text>(creationEditorRootPath + "/m_rowSpell/m_btnNextSpell");
            m_tmpCreateDraftText = FindChildComponent<TMP_Text>("m_panelLeft/m_btnCreateDraft");
            m_rectPanelLeft = FindChildComponent<RectTransform>("m_panelLeft");
            m_rectPanelCenter = FindChildComponent<RectTransform>("m_panelCenter");
            m_goCardListRoot = FindChild("m_panelLeft/m_scrollCharacterCardList").gameObject;
            m_goCreationOptionListRoot = FindChild(creationEditorRootPath + "/m_scrollCreationOptionCards")?.gameObject;
            m_rectCreationOptionListRoot = m_goCreationOptionListRoot != null
                ? m_goCreationOptionListRoot.GetComponent<RectTransform>()
                : null;
            m_rectCardContent = FindChildComponent<RectTransform>("m_panelLeft/m_scrollCharacterCardList/Viewport/m_rectCharacterCardContent");
            m_rectCreationOptionViewport = FindChildComponent<RectTransform>(creationEditorRootPath + "/m_scrollCreationOptionCards/Viewport");
            m_rectCreationOptionCardContent = FindChildComponent<RectTransform>(creationEditorRootPath + "/m_scrollCreationOptionCards/Viewport/m_rectCreationOptionCardContent");
            m_goCharacterCardTemplate = FindChild("m_panelLeft/m_scrollCharacterCardList/Viewport/m_rectCharacterCardContent/m_itemCharacterCardTemplate").gameObject;
            m_goCreationOptionCardTemplate = FindChild(creationEditorRootPath + "/m_scrollCreationOptionCards/Viewport/m_rectCreationOptionCardContent/m_itemCreationOptionCardTemplate")?.gameObject;
            m_btnDeleteSelected = FindChildComponent<Button>("m_panelLeft/m_btnDeleteSelectedCharacter");
            m_tmpDeleteSelectedText = FindChildComponent<TMP_Text>("m_panelLeft/m_btnDeleteSelectedCharacter");
            const string previewContentRootPath = "m_panelCenter/m_scrollSelectionDetail/Viewport/m_rectSelectionDetailContent";
            m_rectSelectionDetailContent = FindChildComponent<RectTransform>(previewContentRootPath);
            m_goPreviewDetailMainRaceBlock = FindChild(previewContentRootPath + "/m_previewDetailMainRaceBlock")?.gameObject;
            m_goPreviewDetailSubRaceBlock = FindChild(previewContentRootPath + "/m_previewDetailSubRaceBlock")?.gameObject;
            m_goPreviewDetailClassBlock = FindChild(previewContentRootPath + "/m_previewDetailClassBlock")?.gameObject;
            m_goPreviewDetailBackgroundBlock = FindChild(previewContentRootPath + "/m_previewDetailBackgroundBlock")?.gameObject;
            m_tmpPreviewMainRaceName = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailMainRaceBlock/m_tmpPreviewMainRaceName");
            m_tmpPreviewBodySize = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailMainRaceBlock/m_tmpPreviewBodySize");
            m_tmpPreviewSpeed = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailMainRaceBlock/m_tmpPreviewSpeed");
            m_tmpPreviewLanguage = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailMainRaceBlock/m_tmpPreviewlanguage");
            m_tmpPreviewFeatureTitle = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailMainRaceBlock/m_tmpPreviewFeatureTitle");
            m_tmpPreviewFeature = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailMainRaceBlock/m_tmpPreviewFeature");
            m_tmpPreviewRaceDes = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailMainRaceBlock/m_tmpPreviewRaceDes");
            m_tmpPreviewSubRaceName = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailSubRaceBlock/m_tmpPreviewSubRaceName");
            m_tmpPreviewSubRaceBodySize = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailSubRaceBlock/m_tmpPreviewSubRaceBodySize");
            m_tmpPreviewSubRaceSpeed = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailSubRaceBlock/m_tmpPreviewSubRaceRaceSpeed");
            m_tmpPreviewSubFeatureTitle = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailSubRaceBlock/m_tmpPreviewSubFeatureTitle");
            m_tmpPreviewSubRaceFeature = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailSubRaceBlock/m_tmpPreviewSubRaceFeature");
            m_tmpPreviewSubRaceDes = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailSubRaceBlock/m_tmpPreviewSubRaceDes");
            m_tmpPreviewClassName = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailClassBlock/m_tmpPreviewClassName");
            m_tmpPreviewClassHPDice = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailClassBlock/m_tmpPreviewClassHPDice");
            m_tmpPreviewClassMainAttri = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailClassBlock/m_tmpPreviewClassMainAttri");
            m_tmpPreviewClassDC = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailClassBlock/m_tmpPreviewClassDC");
            m_tmpPreviewClassArmor = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailClassBlock/m_tmpPreviewClassArmr");
            m_tmpPreviewClassWeapon = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailClassBlock/m_tmpPreviewClassWeapon");
            m_tmpPreviewClassTitle = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailClassBlock/m_tmpPreviewClassTitle");
            m_tmpPreviewClassFeature = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailClassBlock/m_tmpPreviewClassFeature");
            m_tmpPreviewBackgroundName = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailBackgroundBlock/m_tmpPreviewBackgroundName");
            m_tmpPreviewBackgroundSkill = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailBackgroundBlock/m_tmpPreviewBackgroundSkill");
            m_tmpPreviewBackgroundTools = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailBackgroundBlock/m_tmpPreviewBackgroundTools");
            m_tmpPreviewBackgroundLanguage = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailBackgroundBlock/m_tmpPreviewBackgroundLanguage");
            m_tmpPreviewBackgroundItem = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailBackgroundBlock/m_tmpPreviewBackgroundItem");
            m_tmpPreviewBackgroundFeatureTitle = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailBackgroundBlock/reviewClassBackgroundFeatureTitle");
            m_tmpPreviewBackgroundFeature = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailBackgroundBlock/m_tmpPreviewBackgroundFeature");
            m_tmpPreviewBackgroundDes = FindChildComponent<TMP_Text>(previewContentRootPath + "/m_previewDetailBackgroundBlock/m_tmpPreviewBackgroundDes");

            m_rowLabels = new[] { m_tmpRaceLabel, m_tmpClassLabel, m_tmpBackgroundLabel, m_tmpFeatLabel, m_tmpSpellLabel };
            m_rowValues = new[] { m_tmpRaceValue, m_tmpClassValue, m_tmpBackgroundValue, m_tmpFeatValue, m_tmpSpellValue };
            m_rowLeftButtons = new[] { m_btnPrevRace, m_btnPrevClass, m_btnPrevBackground, m_btnPrevFeat, m_btnPrevSpell };
            m_rowRightButtons = new[] { m_btnNextRace, m_btnNextClass, m_btnNextBackground, m_btnNextFeat, m_btnNextSpell };
            m_rowLeftButtonTexts = new[] { m_tmpPrevRaceText, m_tmpPrevClassText, m_tmpPrevBackgroundText, m_tmpPrevFeatText, m_tmpPrevSpellText };
            m_rowRightButtonTexts = new[] { m_tmpNextRaceText, m_tmpNextClassText, m_tmpNextBackgroundText, m_tmpNextFeatText, m_tmpNextSpellText };

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

            InitializeChoiceContentTemplates();
            ApplyCharacterListPanelLayout();
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
                ClearMainRaceDetailSelection();
                RefreshView();
                return;
            }

            selectedIndex = (selectedIndex + direction + count) % count;
            ClearMainRaceDetailSelection();
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

        private void ClearMainRaceDetailSelection()
        {
            m_selectedMainRaceGroupKey = string.Empty;
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
            RefreshInlineChoiceContents();
            ApplyEditorRowAccordionLayout();
            RefreshSelectionPreview();
            RefreshCreationOptionCards();
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

            if (m_tmpInputCharacterLevel != null)
            {
                m_tmpInputCharacterLevel.contentType = TMP_InputField.ContentType.IntegerNumber;
                m_tmpInputCharacterLevel.characterLimit = 2;
                m_tmpInputCharacterLevel.onEndEdit.RemoveAllListeners();
                m_tmpInputCharacterLevel.onEndEdit.AddListener(OnCharacterLevelInputEndEdit);
            }

            if (m_tmpHpValue != null)
            {
                m_tmpHpValue.readOnly = true;
                m_tmpHpValue.onSelect.RemoveAllListeners();
                m_tmpHpValue.onSelect.AddListener(_ => ToggleHpChoiceContent());
                m_tmpHpValue.onEndEdit.RemoveAllListeners();
                m_tmpHpValue.onEndEdit.AddListener(OnHpValueInputEndEdit);
            }

            BindButton(m_btnHpRoll, OnClickHpRoll);
            BindButton(m_btnHpRollConfirm, OnClickHpRollConfirm);

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
        }

        private void OnAlignmentDropdownChanged(int index)
        {
            if (m_isRefreshingCharacterInputs || m_currentCharacter == null || m_tmpDropdownAlignment == null)
            {
                return;
            }

            m_currentCharacter.Alignment = GetAlignmentDropdownText(index);
        }

        private void OnCharacterLevelInputEndEdit(string value)
        {
            if (m_isRefreshingCharacterInputs || m_currentCharacter == null)
            {
                return;
            }

            m_currentCharacter.Level = ClampCharacterLevel(value);
            RefreshCharacterHpByMode(m_currentCharacter);
            RefreshView();
        }

        private void OnHpValueInputEndEdit(string value)
        {
            if (m_isRefreshingCharacterInputs || m_currentCharacter == null)
            {
                return;
            }

            if (!string.Equals(CharacterHpModeIds.Normalize(m_currentCharacter.HpModeId), CharacterHpModeIds.Custom, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            int hp = TryParseNonNegativeInt(value, out int parsedValue) ? parsedValue : 0;
            m_currentCharacter.ManualHp = hp;
            m_currentCharacter.MaxHp = hp;
            if (m_tmpHpValue != null)
            {
                m_tmpHpValue.SetTextWithoutNotify(hp > 0 ? hp.ToString() : string.Empty);
            }
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

        private bool HasSelectedCharacter()
        {
            return m_selectedListCharacterIndex >= 0 && m_selectedListCharacterIndex < m_characterCards.Count;
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

        private void RefreshInlineChoiceContents()
        {
            RefreshHpChoiceContent(false);
            RefreshHpRollContent();
            RefreshClassChoiceContent();
            RefreshFeatChoiceContent();
            SetActive(m_goRowSpell, false);
            SetActive(m_rectSpellChoiceContent != null ? m_rectSpellChoiceContent.gameObject : null, false);
        }

        private void ToggleHpChoiceContent()
        {
            bool nextVisible = m_rectHpChoiceContent != null && !m_rectHpChoiceContent.gameObject.activeSelf;
            RefreshHpChoiceContent(nextVisible);
        }

        private void RefreshHpChoiceContent(bool visible)
        {
            SetActive(m_rectHpChoiceContent != null ? m_rectHpChoiceContent.gameObject : null, visible);
            if (!visible)
            {
                return;
            }

            SetChoiceButtons(
                m_rectHpChoiceContent,
                m_btnHpChoiceTemplate,
                new[] { "手动/自定义", "掷骰", "均值" },
                new Action[]
                {
                    () => SelectHpMode(CharacterHpModeIds.Custom),
                    () => SelectHpMode(CharacterHpModeIds.Rolled),
                    () => SelectHpMode(CharacterHpModeIds.Average)
                });
        }

        private void RefreshHpRollContent()
        {
            bool visible = m_currentCharacter != null
                && string.Equals(CharacterHpModeIds.Normalize(m_currentCharacter.HpModeId), CharacterHpModeIds.Rolled, StringComparison.OrdinalIgnoreCase);

            SetActive(m_rectHpRollContent != null ? m_rectHpRollContent.gameObject : null, visible);
            RefreshCreationEditorHpLayout();
            if (!visible)
            {
                return;
            }

            int level = GetNextPendingHpRollLevel(GetCurrentCharacterLevel());
            DndClassDefineData classData = TryGetSelected(m_classes, m_selectedClassIndex, out DndClassDefineData selectedClass) ? selectedClass : null;
            int hitDie = classData?.HitDie ?? 0;
            int constitutionModifier = GetCurrentConstitutionModifier();
            CharacterHpRollSaveData roll = level > 0 ? FindHpRoll(level) : null;

            bool isCompleted = level <= 0;
            bool canRoll = !isCompleted && level > 1 && classData != null && hitDie > 0 && roll == null && (m_pendingHpRollLevel != level || m_pendingHpRollValue <= 0);
            bool canConfirm = !isCompleted && classData != null && hitDie > 0 && (level == 1 || m_pendingHpRollLevel == level || roll != null);

            SetText(m_tmpHpRollLevel, isCompleted ? "已完成" : (level == 1 ? "1级（自动确认）" : $"当前等级：{level}"));
            SetText(m_tmpHpRollHitDie, hitDie > 0 ? $"生命骰：d{hitDie}" : "生命骰：-");
            SetText(m_tmpHpRollConModifier, $"体质调整值：{FormatSignedNumber(constitutionModifier)}");

            if (isCompleted)
            {
                SetText(m_tmpHpRollValue, m_currentCharacter != null ? m_currentCharacter.MaxHp.ToString() : string.Empty);
                SetText(m_tmpHpRollGain, "本级生命值：-");
            }
            else if (roll != null)
            {
                SetText(m_tmpHpRollValue, roll.Level == 1 ? roll.HitDie.ToString() : roll.RollValue.ToString());
                SetText(m_tmpHpRollGain, $"本级生命值：{roll.HpGain}");
            }
            else if (level == 1)
            {
                int gain = CalculateHpGain(hitDie, constitutionModifier, true, 0);
                SetText(m_tmpHpRollValue, hitDie > 0 ? hitDie.ToString() : string.Empty);
                SetText(m_tmpHpRollGain, $"本级生命值：{gain}");
            }
            else if (m_pendingHpRollLevel == level && m_pendingHpRollValue > 0)
            {
                int gain = CalculateHpGain(hitDie, constitutionModifier, false, m_pendingHpRollValue);
                SetText(m_tmpHpRollValue, m_pendingHpRollValue.ToString());
                SetText(m_tmpHpRollGain, $"本级生命值：{gain}");
            }
            else
            {
                SetText(m_tmpHpRollValue, "未掷骰");
                SetText(m_tmpHpRollGain, "本级生命值：-");
            }

            if (m_btnHpRoll != null)
            {
                m_btnHpRoll.interactable = canRoll;
            }

            if (m_btnHpRollConfirm != null)
            {
                m_btnHpRollConfirm.interactable = canConfirm;
            }
        }

        private void RefreshCreationEditorHpLayout()
        {
            if (m_rectHpRollContent != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(m_rectHpRollContent);
                if (m_rectHpRollContent.parent is RectTransform parentRect)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
                }
            }

            RefreshCreationEditorScrollLayout(m_rectHpRollContent);
        }

        private void SelectHpMode(string hpModeId)
        {
            if (m_currentCharacter == null)
            {
                return;
            }

            m_currentCharacter.HpModeId = CharacterHpModeIds.Normalize(hpModeId);
            m_pendingHpRollLevel = 0;
            m_pendingHpRollValue = 0;
            RefreshHpChoiceContent(false);
            RefreshCharacterHpByMode(m_currentCharacter);
            RefreshCharacterBasicInfoInputs();
            RefreshHpRollContent();
        }

        private void OnClickHpRoll()
        {
            if (m_currentCharacter == null || !TryGetSelected(m_classes, m_selectedClassIndex, out DndClassDefineData classData) || classData == null || classData.HitDie <= 0)
            {
                return;
            }

            int level = GetNextPendingHpRollLevel(GetCurrentCharacterLevel());
            if (level <= 1 || FindHpRoll(level) != null)
            {
                return;
            }

            m_pendingHpRollLevel = level;
            m_pendingHpRollValue = UnityEngine.Random.Range(1, classData.HitDie + 1);
            RefreshHpRollContent();
        }

        private void OnClickHpRollConfirm()
        {
            if (m_currentCharacter == null || !TryGetSelected(m_classes, m_selectedClassIndex, out DndClassDefineData classData) || classData == null || classData.HitDie <= 0)
            {
                return;
            }

            int level = GetNextPendingHpRollLevel(GetCurrentCharacterLevel());
            if (level <= 0)
            {
                return;
            }

            if (FindHpRoll(level) != null)
            {
                RefreshHpRollContent();
                return;
            }

            if (level > 1 && (m_pendingHpRollLevel != level || m_pendingHpRollValue <= 0))
            {
                return;
            }

            int constitutionModifier = GetCurrentConstitutionModifier();
            int rollValue = level == 1 ? classData.HitDie : m_pendingHpRollValue;
            int hpGain = CalculateHpGain(classData.HitDie, constitutionModifier, level == 1, rollValue);
            m_currentCharacter.HpRolls ??= new List<CharacterHpRollSaveData>();
            m_currentCharacter.HpRolls.RemoveAll(data => data != null && data.Level == level);
            m_currentCharacter.HpRolls.Add(new CharacterHpRollSaveData
            {
                Level = level,
                ClassId = classData.ClassId ?? string.Empty,
                HitDie = classData.HitDie,
                RollValue = rollValue,
                ConstitutionModifier = constitutionModifier,
                HpGain = hpGain
            });

            m_pendingHpRollLevel = 0;
            m_pendingHpRollValue = 0;
            RefreshCharacterHpByMode(m_currentCharacter);
            RefreshCharacterBasicInfoInputs();
            RefreshHpRollContent();
        }

        private void RefreshClassChoiceContent()
        {
            List<string> labels = BuildClassChoiceEntryLabels();
            bool visible = labels.Count > 0;
            SetActive(m_rectClassChoiceContent != null ? m_rectClassChoiceContent.gameObject : null, visible);
            if (visible)
            {
                SetChoiceButtons(m_rectClassChoiceContent, m_btnClassChoiceTemplate, labels);
            }
        }

        private void RefreshFeatChoiceContent()
        {
            List<string> labels = BuildFeatChoiceEntryLabels();
            bool visible = labels.Count > 0;
            SetActive(m_goRowFeat, visible);
            SetActive(m_rectFeatChoiceContent != null ? m_rectFeatChoiceContent.gameObject : null, visible);
            if (visible)
            {
                SetChoiceButtons(m_rectFeatChoiceContent, m_btnFeatChoiceTemplate, labels);
            }
        }

        private List<string> BuildClassChoiceEntryLabels()
        {
            List<string> labels = new List<string>();
            if (!TryGetSelected(m_classes, m_selectedClassIndex, out DndClassDefineData classData) || classData == null)
            {
                return labels;
            }

            IReadOnlyList<DndLevelProgressionData> progressions = DndRuleContentService.Instance.GetClassProgressions(classData.ClassId);
            int level = GetCurrentCharacterLevel();
            bool subclassChoiceAdded = false;
            for (int index = 0; index < progressions.Count; index++)
            {
                DndLevelProgressionData progression = progressions[index];
                if (progression.Level < 1 || progression.Level > level)
                {
                    continue;
                }

                bool hasSubclassChoice = !string.IsNullOrWhiteSpace(progression.SubclassChoiceGroupId) ||
                    progression.ChoiceGroupIds.Contains("choice_subclass");
                if (hasSubclassChoice && !subclassChoiceAdded)
                {
                    labels.Add($"{progression.Level}级 选择子职业");
                    subclassChoiceAdded = true;
                }
            }

            return labels;
        }

        private List<string> BuildFeatChoiceEntryLabels()
        {
            List<string> labels = new List<string>();
            if (!TryGetSelected(m_classes, m_selectedClassIndex, out DndClassDefineData classData) || classData == null)
            {
                return labels;
            }

            IReadOnlyList<DndLevelProgressionData> progressions = DndRuleContentService.Instance.GetClassProgressions(classData.ClassId);
            int level = GetCurrentCharacterLevel();
            for (int index = 0; index < progressions.Count; index++)
            {
                DndLevelProgressionData progression = progressions[index];
                if (progression.Level < 1 || progression.Level > level || !progression.AsiAvailable)
                {
                    continue;
                }

                labels.Add($"{progression.Level}级 属性值提升");
                labels.Add($"{progression.Level}级 专长");
            }

            return labels;
        }

        private int GetCurrentCharacterLevel()
        {
            if (m_tmpInputCharacterLevel != null && !m_isRefreshingCharacterInputs)
            {
                return ClampCharacterLevel(m_tmpInputCharacterLevel.text);
            }

            return m_currentCharacter == null ? 1 : ClampCharacterLevel(m_currentCharacter.Level.ToString());
        }

        private int ResolveDisplayedHpValue()
        {
            if (m_currentCharacter == null)
            {
                return 0;
            }

            RefreshCharacterHpByMode(m_currentCharacter);
            return Math.Max(0, m_currentCharacter.MaxHp);
        }

        private void RefreshCharacterHpByMode(CharacterCardDraftSaveData character)
        {
            if (character == null)
            {
                return;
            }

            character.HpModeId = CharacterHpModeIds.Normalize(character.HpModeId);
            int level = Math.Max(1, character.Level);
            DndClassDefineData classData = FindDataById(m_classes, character.ClassId, data => data.ClassId)
                ?? (TryGetSelected(m_classes, m_selectedClassIndex, out DndClassDefineData selectedClass) ? selectedClass : null);

            switch (character.HpModeId)
            {
                case CharacterHpModeIds.Average:
                    character.MaxHp = CalculateAverageHp(classData, level, GetCurrentConstitutionModifier());
                    break;
                case CharacterHpModeIds.Rolled:
                    NormalizeCurrentHpRolls(character, classData, level);
                    character.MaxHp = SumHpRolls(character.HpRolls);
                    break;
                default:
                    if (m_tmpHpValue != null && !m_isRefreshingCharacterInputs && TryParseNonNegativeInt(m_tmpHpValue.text, out int manualHp))
                    {
                        character.ManualHp = manualHp;
                    }

                    character.MaxHp = Math.Max(0, character.ManualHp);
                    break;
            }
        }

        private void NormalizeCurrentHpRolls(CharacterCardDraftSaveData character, DndClassDefineData classData, int level)
        {
            if (character == null)
            {
                return;
            }

            character.HpRolls ??= new List<CharacterHpRollSaveData>();
            string classId = classData?.ClassId ?? string.Empty;
            int hitDie = classData?.HitDie ?? 0;
            character.HpRolls.RemoveAll(data =>
                data == null ||
                data.Level < 1 ||
                data.Level > level ||
                (!string.IsNullOrWhiteSpace(classId) && !string.Equals(data.ClassId, classId, StringComparison.OrdinalIgnoreCase)) ||
                (hitDie > 0 && data.HitDie != hitDie));
        }

        private CharacterHpRollSaveData FindHpRoll(int level)
        {
            if (m_currentCharacter?.HpRolls == null)
            {
                return null;
            }

            for (int index = 0; index < m_currentCharacter.HpRolls.Count; index++)
            {
                CharacterHpRollSaveData roll = m_currentCharacter.HpRolls[index];
                if (roll != null && roll.Level == level)
                {
                    return roll;
                }
            }

            return null;
        }

        private int GetCurrentConstitutionModifier()
        {
            DndRaceDefineData raceData = TryGetSelected(m_races, m_selectedRaceIndex, out DndRaceDefineData selectedRace)
                ? selectedRace
                : null;
            int[] raceBonuses = new int[6];
            ApplyRaceAbilityBonuses(raceData, raceBonuses);
            return CalculateAbilityModifier(DefaultAbilityScore + raceBonuses[2]);
        }

        private int GetNextPendingHpRollLevel(int currentLevel)
        {
            int level = Math.Max(1, currentLevel);
            for (int checkLevel = 1; checkLevel <= level; checkLevel++)
            {
                if (FindHpRoll(checkLevel) == null)
                {
                    return checkLevel;
                }
            }

            return -1;
        }

        private static int CalculateAverageHp(DndClassDefineData classData, int level, int constitutionModifier)
        {
            if (classData == null || classData.HitDie <= 0)
            {
                return 0;
            }

            int total = CalculateHpGain(classData.HitDie, constitutionModifier, true, 0);
            int averageRoll = classData.HitDie / 2 + 1;
            for (int currentLevel = 2; currentLevel <= Math.Max(1, level); currentLevel++)
            {
                total += CalculateHpGain(classData.HitDie, constitutionModifier, false, averageRoll);
            }

            return total;
        }

        private static int CalculateHpGain(int hitDie, int constitutionModifier, bool isLevelOne, int rollValue)
        {
            if (hitDie <= 0)
            {
                return 0;
            }

            int baseValue = isLevelOne ? hitDie : Math.Max(1, Math.Min(hitDie, rollValue));
            return Math.Max(1, baseValue + constitutionModifier);
        }

        private static int SumHpRolls(List<CharacterHpRollSaveData> hpRolls)
        {
            if (hpRolls == null)
            {
                return 0;
            }

            int total = 0;
            for (int index = 0; index < hpRolls.Count; index++)
            {
                total += Math.Max(0, hpRolls[index]?.HpGain ?? 0);
            }

            return total;
        }

        private static int CalculateAbilityModifier(int abilityScore)
        {
            return Mathf.FloorToInt((abilityScore - 10) / 2f);
        }

        private static bool TryParseNonNegativeInt(string value, out int result)
        {
            if (int.TryParse(value, out int parsedValue))
            {
                result = Math.Max(0, parsedValue);
                return true;
            }

            result = 0;
            return false;
        }

        private void InitializeChoiceContentTemplates()
        {
            ConfigureChoiceContentAutoLayout(m_rectHpChoiceContent, m_btnHpChoiceTemplate);
            ConfigureChoiceContentAutoLayout(m_rectClassChoiceContent, m_btnClassChoiceTemplate);
            ConfigureChoiceContentAutoLayout(m_rectFeatChoiceContent, m_btnFeatChoiceTemplate);
            SetActive(m_rectHpChoiceContent != null ? m_rectHpChoiceContent.gameObject : null, false);
            SetActive(m_rectClassChoiceContent != null ? m_rectClassChoiceContent.gameObject : null, false);
            SetActive(m_rectFeatChoiceContent != null ? m_rectFeatChoiceContent.gameObject : null, false);
            SetActive(m_rectSpellChoiceContent != null ? m_rectSpellChoiceContent.gameObject : null, false);
            SetTemplateInactive(m_btnHpChoiceTemplate);
            SetTemplateInactive(m_btnClassChoiceTemplate);
            SetTemplateInactive(m_btnFeatChoiceTemplate);
        }

        private static void SetTemplateInactive(Button template)
        {
            if (template != null)
            {
                template.gameObject.SetActive(false);
            }
        }

        private static void ConfigureChoiceContentAutoLayout(RectTransform content, Button template)
        {
            if (content == null)
            {
                return;
            }

            VerticalLayoutGroup layoutGroup = content.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = content.gameObject.AddComponent(typeof(VerticalLayoutGroup)) as VerticalLayoutGroup;
            }

            if (layoutGroup != null)
            {
                layoutGroup.childAlignment = TextAnchor.UpperLeft;
                layoutGroup.childControlWidth = true;
                layoutGroup.childControlHeight = true;
                layoutGroup.childForceExpandWidth = true;
                layoutGroup.childForceExpandHeight = false;
                layoutGroup.spacing = 6f;
            }

            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = content.gameObject.AddComponent(typeof(ContentSizeFitter)) as ContentSizeFitter;
            }

            if (fitter != null)
            {
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

        }

        private static void SetChoiceButtons(RectTransform content, Button template, IReadOnlyList<string> labels, IReadOnlyList<Action> actions = null)
        {
            if (content == null || template == null)
            {
                return;
            }

            for (int index = content.childCount - 1; index >= 0; index--)
            {
                Transform child = content.GetChild(index);
                if (child == template.transform)
                {
                    continue;
                }

                UnityEngine.Object.Destroy(child.gameObject);
            }

            template.gameObject.SetActive(false);
            if (labels == null)
            {
                return;
            }

            for (int index = 0; index < labels.Count; index++)
            {
                Button button = UnityEngine.Object.Instantiate(template, content);
                button.name = $"{template.name}_{index + 1}";
                button.gameObject.SetActive(true);
                button.onClick.RemoveAllListeners();
                if (actions != null && index < actions.Count && actions[index] != null)
                {
                    Action action = actions[index];
                    button.onClick.AddListener(() => action?.Invoke());
                }
                TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
                SetText(label, labels[index]);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            if (content.parent is RectTransform parentRect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            }

            RefreshCreationEditorScrollLayout(content);
        }

        private TMP_Text FindHpRollText(string name)
        {
            Transform root = FindHpRollRoot("m_panelLeft/m_scrollCreationEditor/Viewport/m_rectCreationEditorContent");
            return root != null ? root.Find(name)?.GetComponent<TMP_Text>() : null;
        }

        private Button FindHpRollButton(string name)
        {
            Transform root = FindHpRollRoot("m_panelLeft/m_scrollCreationEditor/Viewport/m_rectCreationEditorContent");
            return root != null ? root.Find(name)?.GetComponent<Button>() : null;
        }

        private Transform FindHpRollRoot(string creationEditorRootPath)
        {
            return FindChild(creationEditorRootPath + "/m_rectHpRollContent")
                ?? FindChild(creationEditorRootPath + "/m_rowCharacterBase/m_rectHpRollContent");
        }

        private static void RefreshCreationEditorScrollLayout(RectTransform changedContent)
        {
            if (changedContent == null)
            {
                return;
            }

            ScrollRect scrollRect = changedContent.GetComponentInParent<ScrollRect>();
            if (scrollRect == null || scrollRect.content == null)
            {
                return;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
        }

        private void ApplyEditorRowAccordionLayout()
        {
            if (m_goCreationOptionListRoot == null || m_centerOptionMode == CharacterCreationOptionPanelMode.None)
            {
                return;
            }

            Transform anchorRow = GetCreationOptionAnchorRowTransform(m_centerOptionMode);
            if (anchorRow == null || anchorRow.parent == null)
            {
                return;
            }

            Transform optionListTransform = m_goCreationOptionListRoot.transform;
            if (optionListTransform.parent != anchorRow.parent)
            {
                optionListTransform.SetParent(anchorRow.parent, false);
            }

            int targetSiblingIndex = anchorRow.GetSiblingIndex() + 1;
            if (optionListTransform.GetSiblingIndex() != targetSiblingIndex)
            {
                optionListTransform.SetSiblingIndex(targetSiblingIndex);
            }
        }

        private void RefreshSelectionPreview()
        {
            RefreshRaceSelectionPreview();
            RefreshClassSelectionPreview();
            RefreshBackgroundSelectionPreview();
            RebuildSelectionDetailLayout();
        }

        private void RebuildSelectionDetailLayout()
        {
            if (m_rectSelectionDetailContent == null)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_rectSelectionDetailContent);
        }

        private void RefreshRaceSelectionPreview()
        {
            DndRaceDefineData selectedRace = TryGetSelected(m_races, m_selectedRaceIndex, out DndRaceDefineData raceData)
                ? raceData
                : null;
            DndRaceSubDefineData raceSub = null;
            DndRaceMainDefineData raceMain = null;

            if (selectedRace != null && TryGetRaceSubData(selectedRace.RaceId, out DndRaceSubDefineData subData))
            {
                raceSub = subData;
                if (!string.IsNullOrWhiteSpace(raceSub.MainRaceId))
                {
                    DndRuleContentService.Instance.TryGetRaceMain(raceSub.MainRaceId, out raceMain);
                }
            }
            else if (!string.IsNullOrWhiteSpace(m_selectedMainRaceGroupKey))
            {
                DndRuleContentService.Instance.TryGetRaceMain(m_selectedMainRaceGroupKey, out raceMain);
            }
            else if (selectedRace != null)
            {
                DndRuleContentService.Instance.TryGetRaceMain(selectedRace.RaceId, out raceMain);
            }

            if (raceMain != null)
            {
                SetActive(m_goPreviewDetailMainRaceBlock, true);
                SetText(m_tmpPreviewMainRaceName, FormatTextOrDefault(raceMain.Name, "未选择主要种族"));
                SetText(m_tmpPreviewBodySize, $"体型：{FormatTextOrDefault(raceMain.Size, "未定")}");
                SetText(m_tmpPreviewSpeed, $"速度：{FormatSpeed(raceMain.Speed)}");
                SetText(m_tmpPreviewLanguage, $"语言：{FormatList(raceMain.LanguageIds)}");
                SetText(m_tmpPreviewFeatureTitle, "主要种族特性");
                SetText(m_tmpPreviewFeature, BuildFeatureDetailSummary(raceMain.MainFeatureIds));
                SetText(m_tmpPreviewRaceDes, $"说明：{SummarizeText(raceMain.Description)}");
            }
            else if (selectedRace != null)
            {
                SetActive(m_goPreviewDetailMainRaceBlock, true);
                SetText(m_tmpPreviewMainRaceName, FormatTextOrDefault(selectedRace.Name, "未选择种族"));
                SetText(m_tmpPreviewBodySize, $"体型：{FormatTextOrDefault(selectedRace.Size, "未定")}");
                SetText(m_tmpPreviewSpeed, $"速度：{FormatSpeed(selectedRace.Speed)}");
                SetText(m_tmpPreviewLanguage, $"语言：{FormatList(selectedRace.LanguageIds)}");
                SetText(m_tmpPreviewFeatureTitle, "种族特性");
                SetText(m_tmpPreviewFeature, BuildFeatureDetailSummary(selectedRace.FeatureIds));
                SetText(m_tmpPreviewRaceDes, $"说明：{SummarizeText(selectedRace.Description)}");
            }
            else
            {
                SetActive(m_goPreviewDetailMainRaceBlock, false);
            }

            if (raceSub != null)
            {
                SetActive(m_goPreviewDetailSubRaceBlock, true);
                SetText(m_tmpPreviewSubRaceName, FormatTextOrDefault(raceSub.Name, "未选择亚种"));
                SetText(m_tmpPreviewSubRaceBodySize, $"体型：{FormatTextOrDefault(raceSub.Size, "沿用主要种族")}");
                SetText(m_tmpPreviewSubRaceSpeed, $"速度：{FormatSpeed(raceSub.Speed, "沿用主要种族")}");
                SetText(m_tmpPreviewSubFeatureTitle, "亚种特性");
                SetText(m_tmpPreviewSubRaceFeature, BuildFeatureDetailSummary(raceSub.FeatureIds));
                SetText(m_tmpPreviewSubRaceDes, $"说明：{SummarizeText(raceSub.Description)}");
            }
            else
            {
                SetActive(m_goPreviewDetailSubRaceBlock, false);
            }
        }

        private void RefreshClassSelectionPreview()
        {
            if (!TryGetSelected(m_classes, m_selectedClassIndex, out DndClassDefineData classData) || classData == null)
            {
                SetActive(m_goPreviewDetailClassBlock, false);
                return;
            }

            SetActive(m_goPreviewDetailClassBlock, true);
            SetText(m_tmpPreviewClassName, FormatTextOrDefault(classData.Name, "未选择职业"));
            SetText(m_tmpPreviewClassHPDice, $"生命骰：d{Math.Max(0, classData.HitDie)}");
            SetText(m_tmpPreviewClassMainAttri, $"主要属性：{FormatList(classData.PrimaryAbilityIds)}");
            SetText(m_tmpPreviewClassDC, $"豁免熟练：{FormatList(classData.SavingThrowProficiencies)}");
            SetText(m_tmpPreviewClassArmor, $"护甲熟练：{FormatList(classData.ArmorProficiencies)}");
            SetText(m_tmpPreviewClassWeapon, $"武器熟练：{FormatList(classData.WeaponProficiencies)}");
            SetText(m_tmpPreviewClassTitle, "职业特性");
            SetText(m_tmpPreviewClassFeature, BuildClassLevelOneFeatureSummary(classData.ClassId));
        }

        private void RefreshBackgroundSelectionPreview()
        {
            if (!TryGetSelected(m_backgrounds, m_selectedBackgroundIndex, out DndBackgroundDefineData backgroundData) || backgroundData == null)
            {
                SetActive(m_goPreviewDetailBackgroundBlock, false);
                return;
            }

            SetActive(m_goPreviewDetailBackgroundBlock, true);
            SetText(m_tmpPreviewBackgroundName, FormatTextOrDefault(backgroundData.Name, "未选择背景"));
            SetText(m_tmpPreviewBackgroundSkill, $"技能熟练：{FormatList(backgroundData.SkillProficiencies)}");
            SetText(m_tmpPreviewBackgroundTools, $"工具熟练：{FormatList(backgroundData.ToolProficiencies)}");
            SetText(m_tmpPreviewBackgroundLanguage, $"语言：{FormatList(backgroundData.LanguageIds)}");
            SetText(m_tmpPreviewBackgroundItem, $"物品：{FormatList(backgroundData.EquipmentGrantIds)}");
            SetText(m_tmpPreviewBackgroundFeatureTitle, "背景特性");
            SetText(m_tmpPreviewBackgroundFeature, BuildFeatureDetailSummary(backgroundData.FeatureIds));
            SetText(m_tmpPreviewBackgroundDes, $"说明：{SummarizeText(backgroundData.Description)}");
        }

        private string BuildClassLevelOneFeatureSummary(string classId)
        {
            if (!string.IsNullOrWhiteSpace(classId)
                && DndRuleContentService.Instance.TryGetClassLevelProgression(classId, 1, out DndLevelProgressionData progression))
            {
                return BuildFeatureDetailSummary(progression.FeatureIds);
            }

            return "无";
        }

        private static string FormatSpeed(int speed, string emptyText = "未定")
        {
            return speed > 0 ? speed.ToString() : emptyText;
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
            {
                target.SetActive(active);
            }
        }

        private Transform GetCreationOptionAnchorRowTransform(CharacterCreationOptionPanelMode mode)
        {
            switch (mode)
            {
                case CharacterCreationOptionPanelMode.Race:
                    return m_btnRowRace != null ? m_btnRowRace.transform : null;
                case CharacterCreationOptionPanelMode.Class:
                    return m_btnRowClass != null ? m_btnRowClass.transform : null;
                case CharacterCreationOptionPanelMode.Background:
                    return m_btnRowBackground != null ? m_btnRowBackground.transform : null;
                case CharacterCreationOptionPanelMode.Feat:
                    return m_btnRowFeat != null ? m_btnRowFeat.transform : null;
                case CharacterCreationOptionPanelMode.Spell:
                    return m_btnRowSpell != null ? m_btnRowSpell.transform : null;
                default:
                    return null;
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

        private void ApplyEditorPanelLayout()
        {
            if (m_rectPanelCenter != null)
            {
                m_rectPanelCenter.gameObject.SetActive(true);
            }
        }

        private void ApplyCharacterListPanelLayout()
        {
            if (m_rectPanelCenter != null)
            {
                m_rectPanelCenter.gameObject.SetActive(true);
            }
        }

        private void SetBasicInfoRowsActive(bool active)
        {
            if (m_goRowCharacterBase != null)
            {
                m_goRowCharacterBase.SetActive(active);
            }

            if (m_goRowCharacterName != null)
            {
                m_goRowCharacterName.SetActive(active);
            }

            if (m_goRowAlignment != null)
            {
                m_goRowAlignment.SetActive(active);
            }

            if (!active)
            {
                SetActive(m_rectHpChoiceContent != null ? m_rectHpChoiceContent.gameObject : null, false);
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

        private CharacterRuntimeSnapshotData BuildCurrentRuntimeSnapshot()
        {
            CharacterCardDraftSaveData draft = CloneCharacterDraft(m_currentCharacter) ?? CharacterCardLocalRepository.CreateDraft();
            ApplyCurrentSelectionsToCharacter(draft);
            return BuildRuntimeSnapshot(draft);
        }

        private CharacterRuntimeSnapshotData BuildRuntimeSnapshot(CharacterCardDraftSaveData character)
        {
            CharacterRuntimeSnapshotData snapshot = new CharacterRuntimeSnapshotData();
            if (character == null)
            {
                return snapshot;
            }

            snapshot.CharacterId = character.CharacterId ?? string.Empty;
            snapshot.CharacterName = string.IsNullOrWhiteSpace(character.CharacterName) ? "未命名角色" : character.CharacterName.Trim();
            snapshot.Alignment = character.Alignment ?? string.Empty;
            snapshot.Level = Math.Max(1, character.Level);
            snapshot.RaceId = character.RaceId ?? string.Empty;
            snapshot.ClassId = character.ClassId ?? string.Empty;
            snapshot.BackgroundId = character.BackgroundId ?? string.Empty;
            snapshot.FeatId = character.FeatId ?? string.Empty;
            snapshot.SpellId = character.SpellId ?? string.Empty;

            DndRaceDefineData raceData = FindDataById(m_races, snapshot.RaceId, data => data.RaceId);
            DndClassDefineData classData = FindDataById(m_classes, snapshot.ClassId, data => data.ClassId);
            DndBackgroundDefineData backgroundData = FindDataById(m_backgrounds, snapshot.BackgroundId, data => data.BackgroundId);
            DndFeatDefineData featData = FindDataById(m_feats, snapshot.FeatId, data => data.FeatId);
            DndSpellDefineData spellData = FindDataById(m_spells, snapshot.SpellId, data => data.SpellId);

            snapshot.RaceName = raceData?.Name ?? string.Empty;
            snapshot.ClassName = classData?.Name ?? string.Empty;
            snapshot.BackgroundName = backgroundData?.Name ?? string.Empty;
            snapshot.FeatName = featData?.Name ?? string.Empty;
            snapshot.SpellName = spellData?.Name ?? string.Empty;
            snapshot.Size = raceData?.Size ?? string.Empty;
            snapshot.Speed = raceData?.Speed ?? 0;

            if (raceData != null && TryGetRaceSubData(raceData.RaceId, out DndRaceSubDefineData raceSubData))
            {
                snapshot.MainRaceId = raceSubData.MainRaceId ?? string.Empty;
                snapshot.MainRaceName = GetMainRaceDisplayName(raceSubData.MainRaceId);
            }
            else
            {
                snapshot.MainRaceId = raceData?.RaceId ?? string.Empty;
                snapshot.MainRaceName = raceData?.Name ?? string.Empty;
            }

            int[] raceBonuses = new int[6];
            ApplyRaceAbilityBonuses(raceData, raceBonuses);
            snapshot.Strength = DefaultAbilityScore + raceBonuses[0];
            snapshot.Dexterity = DefaultAbilityScore + raceBonuses[1];
            snapshot.Constitution = DefaultAbilityScore + raceBonuses[2];
            snapshot.Intelligence = DefaultAbilityScore + raceBonuses[3];
            snapshot.Wisdom = DefaultAbilityScore + raceBonuses[4];
            snapshot.Charisma = DefaultAbilityScore + raceBonuses[5];

            snapshot.SavingThrows = classData != null ? FormatListWithBonus(classData.SavingThrowProficiencies) : "无";
            snapshot.Skills = backgroundData != null ? FormatListWithBonus(backgroundData.SkillProficiencies) : "无";
            snapshot.WeaponProficiencies = BuildWeaponProficiencySummary(classData?.WeaponProficiencies, raceData?.FeatureIds);
            snapshot.ToolProficiencies = BuildToolProficiencySummary(backgroundData?.ToolProficiencies, raceData?.FeatureIds);

            List<string> languages = new List<string>();
            AppendUniqueValues(languages, raceData?.LanguageIds);
            AppendUniqueValues(languages, backgroundData?.LanguageIds);
            snapshot.Languages = FormatList(languages);

            snapshot.Senses = BuildFeatureSenseSummary(raceData?.FeatureIds);
            snapshot.DamageResistances = BuildRaceDamageResistanceSummary(raceData?.FeatureIds);
            snapshot.PendingSelections = BuildRacePendingSelectionSummary(raceData?.FeatureIds);
            snapshot.ConditionalBenefits = BuildRaceConditionalBenefitSummary(raceData?.FeatureIds);

            List<string> traitEntries = new List<string>();
            AppendUniqueValues(traitEntries, BuildFeatureDetailEntries(raceData?.FeatureIds));
            AppendUniqueValues(traitEntries, BuildFeatureDetailEntries(backgroundData?.FeatureIds));
            AppendUniqueValues(traitEntries, BuildFeatureDetailEntries(featData?.FeatureIds));
            snapshot.Traits = FormatFeatureDetailEntries(traitEntries);

            List<string> notes = new List<string>();
            if (classData != null && classData.HitDie > 0)
            {
                notes.Add($"职业生命骰：d{classData.HitDie}");
            }

            if (!string.IsNullOrWhiteSpace(snapshot.MainRaceName)
                && !string.Equals(snapshot.MainRaceName, snapshot.RaceName, StringComparison.OrdinalIgnoreCase))
            {
                notes.Add($"所属主种族：{snapshot.MainRaceName}");
            }

            snapshot.Notes = notes.Count > 0 ? string.Join(" / ", notes) : string.Empty;
            return snapshot;
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
            ClearMainRaceDetailSelection();
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
            m_selectedFeatIndex = 0;
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
            if (raceData != null && TryGetRaceSubData(raceData.RaceId, out DndRaceSubDefineData raceSubData))
            {
                return BuildSubRaceOptionDetail(raceSubData);
            }

            StringBuilder builder = new StringBuilder(256);
            AppendLine(builder, "属性加值", BuildRaceAbilityBonusSummary(raceData));
            AppendLine(builder, "语言", FormatList(raceData?.LanguageIds));
            AppendLine(builder, "特性", BuildFeatureDetailSummary(raceData?.FeatureIds));
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
            for (int index = 0; index < optionCount && index < m_creationOptionCardViews.Count; index++)
            {
                m_creationOptionCardViews[index].ResetLayout();
            }

            if (m_rectCreationOptionCardContent != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(m_rectCreationOptionCardContent);
            }
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

                bool hasSubRaces = HasSubRaces(service, raceMain.MainRaceId);
                List<int> childRaceIndexes = hasSubRaces
                    ? FindSubRaceIndexesByMainRace(service, raceMain.MainRaceId)
                    : FindRaceIndexesByMainRace(raceMain.MainRaceId);
                bool isGrouped = hasSubRaces;
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

        private bool HasSubRaces(DndRuleContentService service, string mainRaceId)
        {
            if (service == null || string.IsNullOrWhiteSpace(mainRaceId))
            {
                return false;
            }

            IReadOnlyList<DndRaceSubDefineData> raceSubs = service.RaceSubs;
            for (int index = 0; index < raceSubs.Count; index++)
            {
                DndRaceSubDefineData raceSub = raceSubs[index];
                if (raceSub != null
                    && !string.IsNullOrWhiteSpace(raceSub.SubRaceId)
                    && string.Equals(raceSub.MainRaceId, mainRaceId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private List<int> FindSubRaceIndexesByMainRace(DndRuleContentService service, string mainRaceId)
        {
            List<int> indexes = new List<int>();
            if (service == null || string.IsNullOrWhiteSpace(mainRaceId))
            {
                return indexes;
            }

            HashSet<int> addedIndexes = new HashSet<int>();
            IReadOnlyList<DndRaceSubDefineData> raceSubs = service.RaceSubs;
            for (int index = 0; index < raceSubs.Count; index++)
            {
                DndRaceSubDefineData raceSub = raceSubs[index];
                if (raceSub == null
                    || string.IsNullOrWhiteSpace(raceSub.SubRaceId)
                    || !string.Equals(raceSub.MainRaceId, mainRaceId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                int raceIndex = FindIndexById(m_races, raceSub.SubRaceId, data => data.RaceId);
                if (raceIndex >= 0
                    && raceIndex < m_races.Count
                    && string.Equals(m_races[raceIndex].RaceId, raceSub.SubRaceId, StringComparison.OrdinalIgnoreCase)
                    && addedIndexes.Add(raceIndex))
                {
                    indexes.Add(raceIndex);
                }
            }

            return indexes.Count > 0 ? indexes : FindRaceIndexesByMainRace(mainRaceId);
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
            return BuildAbilityBonusSummary(raceBonuses);
        }

        private string BuildFeatureAbilityBonusSummary(IReadOnlyList<string> featureIds)
        {
            int[] raceBonuses = new int[6];
            ApplyFeatureAbilityBonuses(featureIds, raceBonuses);
            return BuildAbilityBonusSummary(raceBonuses);
        }

        private string BuildAbilityBonusSummary(int[] raceBonuses)
        {
            if (raceBonuses == null || raceBonuses.Length < 6)
            {
                return "当前种族未录入可自动解析的属性加值";
            }

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

            ApplyFeatureAbilityBonuses(raceData.FeatureIds, raceBonuses);
        }

        private void ApplyFeatureAbilityBonuses(IReadOnlyList<string> featureIds, int[] raceBonuses)
        {
            if (featureIds == null || raceBonuses == null || raceBonuses.Length < 6)
            {
                return;
            }

            DndRuleContentService service = DndRuleContentService.Instance;
            for (int featureIndex = 0; featureIndex < featureIds.Count; featureIndex++)
            {
                if (!service.TryGetFeature(featureIds[featureIndex], out DndFeatureDefineData feature))
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

        private string BuildFeatureSenseSummary(IReadOnlyList<string> featureIds)
        {
            if (featureIds == null || featureIds.Count == 0)
            {
                return "无";
            }

            DndRuleContentService service = DndRuleContentService.Instance;
            List<string> senses = new List<string>();
            for (int featureIndex = 0; featureIndex < featureIds.Count; featureIndex++)
            {
                string featureId = featureIds[featureIndex];
                if (string.IsNullOrWhiteSpace(featureId) || !service.TryGetFeature(featureId, out DndFeatureDefineData feature))
                {
                    continue;
                }

                for (int effectIndex = 0; effectIndex < feature.EffectIds.Count; effectIndex++)
                {
                    if (!service.TryGetFeatureEffect(feature.EffectIds[effectIndex], out DndFeatureEffectData effect))
                    {
                        continue;
                    }

                    string senseEntry = TryBuildSenseEntry(effect);
                    if (!string.IsNullOrWhiteSpace(senseEntry) && !ContainsIgnoreCase(senses, senseEntry))
                    {
                        senses.Add(senseEntry);
                    }
                }
            }

            return senses.Count > 0 ? string.Join(" / ", senses) : "无";
        }

        private string BuildRaceDamageResistanceSummary(IReadOnlyList<string> featureIds)
        {
            List<string> values = new List<string>();
            if (HasFeature(featureIds, "矮人体魄", "dwarvenresilience"))
            {
                values.Add("毒素");
            }

            return values.Count > 0 ? string.Join(" / ", values) : "无";
        }

        private string BuildWeaponProficiencySummary(IReadOnlyList<string> baseWeaponProficiencies, IReadOnlyList<string> raceFeatureIds)
        {
            List<string> values = new List<string>();
            AppendUniqueValues(values, baseWeaponProficiencies);

            if (HasFeature(raceFeatureIds, "矮人战斗训练", "dwarvencombattraining"))
            {
                AppendUniqueValues(values, new[]
                {
                    "战斧",
                    "手斧",
                    "轻锤",
                    "战锤"
                });
            }

            return FormatList(values);
        }

        private string BuildToolProficiencySummary(IReadOnlyList<string> baseToolProficiencies, IReadOnlyList<string> raceFeatureIds)
        {
            List<string> values = new List<string>();
            AppendUniqueValues(values, baseToolProficiencies);

            if (HasFeature(raceFeatureIds, "工具熟练", "toolproficiency"))
            {
                values.Add("待选择：铁匠工具 / 酿酒工具 / 石匠工具");
            }

            return FormatList(values);
        }

        private string BuildRacePendingSelectionSummary(IReadOnlyList<string> featureIds)
        {
            List<string> values = new List<string>();
            if (HasFeature(featureIds, "工具熟练", "toolproficiency"))
            {
                values.Add("工匠工具三选一：铁匠工具 / 酿酒工具 / 石匠工具");
            }

            return values.Count > 0 ? string.Join(" / ", values) : "无";
        }

        private string BuildRaceConditionalBenefitSummary(IReadOnlyList<string> featureIds)
        {
            List<string> values = new List<string>();
            if (HasFeature(featureIds, "石工巧匠", "stonecunning"))
            {
                values.Add("石制品相关历史检定：视为熟练，且熟练加值翻倍");
            }

            if (HasFeature(featureIds, "矮人体魄", "dwarvenresilience"))
            {
                values.Add("对抗毒素的豁免：具有优势");
            }

            return values.Count > 0 ? string.Join(" / ", values) : "无";
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
                AppendLine(previewBuilder, "属性加值", BuildFeatureAbilityBonusSummary(previewData.FeatureIds));
                AppendLine(previewBuilder, "语言", FormatList(previewData.LanguageIds));
                AppendLine(previewBuilder, "特性", BuildFeatureDetailSummary(previewData.FeatureIds));
                AppendLine(previewBuilder, "说明", SummarizeText(previewData.Description));
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
            AppendLine(builder, "特性", BuildGroupedRaceFeatureDetailSummary(groupedRaces));
            return builder.ToString().TrimEnd();
        }

        private string BuildSubRaceOptionDetail(DndRaceSubDefineData raceSubData)
        {
            StringBuilder builder = new StringBuilder(256);
            AppendLine(builder, "所属主种族", GetMainRaceDisplayName(raceSubData?.MainRaceId));
            AppendLine(builder, "体型", string.IsNullOrWhiteSpace(raceSubData?.Size) ? "沿用主种族" : raceSubData.Size);
            AppendLine(builder, "速度", raceSubData != null && raceSubData.Speed > 0 ? raceSubData.Speed.ToString() : "沿用主种族");
            AppendLine(builder, "属性加值", BuildFeatureAbilityBonusSummary(raceSubData?.FeatureIds));
            AppendLine(builder, "特性", BuildFeatureDetailSummary(raceSubData?.FeatureIds));
            AppendLine(builder, "说明", SummarizeText(raceSubData?.Description));
            return builder.ToString().TrimEnd();
        }

        private bool TryGetRaceSubData(string raceId, out DndRaceSubDefineData raceSubData)
        {
            raceSubData = null;
            if (string.IsNullOrWhiteSpace(raceId))
            {
                return false;
            }

            return DndRuleContentService.Instance.TryGetRaceSub(raceId, out raceSubData);
        }

        private string GetMainRaceDisplayName(string mainRaceId)
        {
            if (!string.IsNullOrWhiteSpace(mainRaceId)
                && DndRuleContentService.Instance.TryGetRaceMain(mainRaceId, out DndRaceMainDefineData raceMain)
                && !string.IsNullOrWhiteSpace(raceMain.Name))
            {
                return raceMain.Name;
            }

            return FormatTextOrDefault(mainRaceId, "未定");
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
            m_currentCharacter.RuntimeSnapshot = BuildRuntimeSnapshot(m_currentCharacter);
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
            character.Level = ClampCharacterLevel(m_tmpInputCharacterLevel != null ? m_tmpInputCharacterLevel.text : character.Level.ToString());
            character.HpModeId = CharacterHpModeIds.Normalize(character.HpModeId);
            RefreshCharacterHpByMode(character);
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

                if (m_tmpInputCharacterLevel != null)
                {
                    int level = m_currentCharacter == null ? 1 : ClampCharacterLevel(m_currentCharacter.Level.ToString());
                    m_tmpInputCharacterLevel.SetTextWithoutNotify(level.ToString());
                }

                if (m_tmpHpValue != null)
                {
                    int hpValue = ResolveDisplayedHpValue();
                    m_tmpHpValue.readOnly = !string.Equals(CharacterHpModeIds.Normalize(m_currentCharacter?.HpModeId), CharacterHpModeIds.Custom, StringComparison.OrdinalIgnoreCase);
                    m_tmpHpValue.SetTextWithoutNotify(hpValue > 0 ? hpValue.ToString() : string.Empty);
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

        private static int ClampCharacterLevel(string value)
        {
            return int.TryParse(value, out int level) ? Math.Max(1, Math.Min(20, level)) : 1;
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

            if (active)
            {
                SetActive(m_goRowFeat, BuildFeatChoiceEntryLabels().Count > 0);
                SetActive(m_goRowSpell, false);
            }
            else
            {
                SetActive(m_rectHpChoiceContent != null ? m_rectHpChoiceContent.gameObject : null, false);
                SetActive(m_rectClassChoiceContent != null ? m_rectClassChoiceContent.gameObject : null, false);
                SetActive(m_rectFeatChoiceContent != null ? m_rectFeatChoiceContent.gameObject : null, false);
                SetActive(m_rectSpellChoiceContent != null ? m_rectSpellChoiceContent.gameObject : null, false);
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

        private static void AppendLine(StringBuilder builder, string label, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                value = "无";
            }

            builder.AppendLine($"{label}：{value}");
        }

        private static void AddAbilityBonusSummary(List<string> summaries, string label, int bonus)
        {
            if (bonus != 0)
            {
                summaries.Add($"{label}{FormatSignedNumber(bonus)}");
            }
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

        private static string BuildPrimaryAbilitySummary(DndClassDefineData classData)
        {
            if (classData == null)
            {
                return "无";
            }

            string abilityText = FormatAbilityList(classData.PrimaryAbilityIds);
            switch (classData.PrimaryAbilityMode)
            {
                case DndPrimaryAbilityMode.AnyOne:
                    return $"{abilityText} / 任选其一";
                case DndPrimaryAbilityMode.All:
                    return $"{abilityText} / 全部适用";
                case DndPrimaryAbilityMode.Manual:
                    return $"{abilityText} / 需手动确认";
                default:
                    return $"{abilityText} / 固定";
            }
        }

        private static string BuildPrimaryAbilityChoiceDescription(DndPrimaryAbilityMode mode, IReadOnlyList<string> optionLabels)
        {
            string abilityText = FormatTextOrDefault(FormatList(optionLabels), "无");
            switch (mode)
            {
                case DndPrimaryAbilityMode.AnyOne:
                    return $"该职业的主要属性需要从以下候选项中选择 1 项：{abilityText}";
                case DndPrimaryAbilityMode.Manual:
                    return $"该职业的主要属性需要在以下候选项中手动确认：{abilityText}";
                case DndPrimaryAbilityMode.All:
                    return $"该职业会同时使用以下主要属性：{abilityText}";
                default:
                    return $"该职业的主要属性为：{abilityText}";
            }
        }

        private static bool RequiresPrimaryAbilitySelection(DndPrimaryAbilityMode mode, int optionCount)
        {
            switch (mode)
            {
                case DndPrimaryAbilityMode.AnyOne:
                case DndPrimaryAbilityMode.Manual:
                    return optionCount > 0;
                default:
                    return false;
            }
        }

        private static string FormatAbilityList(IReadOnlyList<string> abilityIds)
        {
            if (abilityIds == null || abilityIds.Count == 0)
            {
                return "无";
            }

            List<string> labels = new List<string>();
            for (int index = 0; index < abilityIds.Count; index++)
            {
                labels.Add(GetAbilityDisplayName(abilityIds[index]));
            }

            return FormatList(labels);
        }

        private static string GetAbilityDisplayName(string abilityId)
        {
            string normalized = NormalizeKey(abilityId);
            switch (normalized)
            {
                case "str":
                case "strength":
                case "力量":
                    return "力量";
                case "dex":
                case "dexterity":
                case "敏捷":
                    return "敏捷";
                case "con":
                case "constitution":
                case "体质":
                    return "体质";
                case "int":
                case "intelligence":
                case "智力":
                    return "智力";
                case "wis":
                case "wisdom":
                case "感知":
                    return "感知";
                case "cha":
                case "charisma":
                case "魅力":
                    return "魅力";
                default:
                    return FormatTextOrDefault(abilityId, "未定");
            }
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

        private static bool HasFeature(IReadOnlyList<string> featureIds, string expectedName, string expectedFeatureIdKey)
        {
            if (featureIds == null || featureIds.Count == 0)
            {
                return false;
            }

            string normalizedName = NormalizeKey(expectedName);
            string normalizedFeatureIdKey = NormalizeKey(expectedFeatureIdKey);
            DndRuleContentService service = DndRuleContentService.Instance;
            for (int index = 0; index < featureIds.Count; index++)
            {
                string featureId = featureIds[index];
                if (string.IsNullOrWhiteSpace(featureId))
                {
                    continue;
                }

                string normalizedFeatureId = NormalizeKey(featureId);
                if (!string.IsNullOrWhiteSpace(normalizedFeatureIdKey) && normalizedFeatureId.Contains(normalizedFeatureIdKey))
                {
                    return true;
                }

                if (!service.TryGetFeature(featureId, out DndFeatureDefineData feature))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(normalizedName) && NormalizeKey(feature.Name).Contains(normalizedName))
                {
                    return true;
                }
            }

            return false;
        }

        private static string TryBuildSenseEntry(DndFeatureEffectData effect)
        {
            string effectType = NormalizeKey(effect?.EffectType);
            string target = NormalizeKey(effect?.Target);
            if (!effectType.Contains("vision") && !effectType.Contains("sense") && !target.Contains("vision") && !target.Contains("sense"))
            {
                return string.Empty;
            }

            string label;
            if (target.Contains("superiordarkvision"))
            {
                label = "增强黑暗视觉";
            }
            else if (target.Contains("darkvision"))
            {
                label = "黑暗视觉";
            }
            else if (target.Contains("blindsight"))
            {
                label = "盲视";
            }
            else if (target.Contains("tremorsense"))
            {
                label = "震颤感知";
            }
            else if (target.Contains("truesight"))
            {
                label = "真实视觉";
            }
            else
            {
                label = FormatTextOrDefault(effect?.Target, "特殊感官");
            }

            if (TryParseSignedInt(effect?.Value, out int range) && range > 0)
            {
                return $"{label} {range} 尺";
            }

            return label;
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
                if (string.IsNullOrWhiteSpace(value) || ContainsIgnoreCase(target, value))
                {
                    continue;
                }

                target.Add(value.Trim());
            }
        }

        private static CharacterCardDraftSaveData CloneCharacterDraft(CharacterCardDraftSaveData source)
        {
            if (source == null)
            {
                return null;
            }

            return new CharacterCardDraftSaveData
            {
                CharacterId = source.CharacterId ?? string.Empty,
                CharacterName = source.CharacterName ?? string.Empty,
                Alignment = source.Alignment ?? string.Empty,
                RaceId = source.RaceId ?? string.Empty,
                ClassId = source.ClassId ?? string.Empty,
                BackgroundId = source.BackgroundId ?? string.Empty,
                FeatId = source.FeatId ?? string.Empty,
                SpellId = source.SpellId ?? string.Empty,
                PreviewImagePath = source.PreviewImagePath ?? string.Empty,
                Level = Math.Max(1, source.Level),
                HpModeId = CharacterHpModeIds.Normalize(source.HpModeId),
                MaxHp = Math.Max(0, source.MaxHp),
                ManualHp = Math.Max(0, source.ManualHp),
                HpRolls = CloneHpRolls(source.HpRolls),
                IsCompleted = source.IsCompleted,
                CreatedAt = source.CreatedAt ?? string.Empty,
                UpdatedAt = source.UpdatedAt ?? string.Empty,
                RuntimeSnapshot = CharacterRuntimeSnapshotData.Clone(source.RuntimeSnapshot)
            };
        }

        private static List<CharacterHpRollSaveData> CloneHpRolls(List<CharacterHpRollSaveData> source)
        {
            List<CharacterHpRollSaveData> result = new List<CharacterHpRollSaveData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterHpRollSaveData roll = source[index];
                if (roll == null)
                {
                    continue;
                }

                result.Add(new CharacterHpRollSaveData
                {
                    Level = Math.Max(1, roll.Level),
                    ClassId = roll.ClassId ?? string.Empty,
                    HitDie = Math.Max(0, roll.HitDie),
                    RollValue = Math.Max(0, roll.RollValue),
                    ConstitutionModifier = roll.ConstitutionModifier,
                    HpGain = Math.Max(0, roll.HpGain)
                });
            }

            return result;
        }

        private static T FindDataById<T>(List<T> list, string id, Func<T, string> idGetter)
            where T : class
        {
            int index = FindIndexById(list, id, idGetter);
            return index >= 0 && index < list.Count ? list[index] : null;
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
        public string HpModeId = CharacterHpModeIds.Custom;
        public int MaxHp;
        public int ManualHp;
        public List<CharacterHpRollSaveData> HpRolls = new List<CharacterHpRollSaveData>();
        public bool IsCompleted;
        public string CreatedAt = string.Empty;
        public string UpdatedAt = string.Empty;
        public CharacterRuntimeSnapshotData RuntimeSnapshot = new CharacterRuntimeSnapshotData();
    }

    internal static class CharacterHpModeIds
    {
        public const string Custom = "custom";
        public const string Rolled = "rolled";
        public const string Average = "average";

        public static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Custom;
            }

            string normalized = value.Trim();
            return normalized.Equals(Custom, StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals(Rolled, StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals(Average, StringComparison.OrdinalIgnoreCase)
                ? normalized.ToLowerInvariant()
                : Custom;
        }
    }

    [Serializable]
    internal sealed class CharacterHpRollSaveData
    {
        public int Level;
        public string ClassId = string.Empty;
        public int HitDie;
        public int RollValue;
        public int ConstitutionModifier;
        public int HpGain;
    }

    [Serializable]
    internal sealed class CharacterRuntimeSnapshotData
    {
        public string CharacterId = string.Empty;
        public string CharacterName = string.Empty;
        public string Alignment = string.Empty;
        public int Level = 1;
        public string RaceId = string.Empty;
        public string RaceName = string.Empty;
        public string MainRaceId = string.Empty;
        public string MainRaceName = string.Empty;
        public string ClassId = string.Empty;
        public string ClassName = string.Empty;
        public string BackgroundId = string.Empty;
        public string BackgroundName = string.Empty;
        public string FeatId = string.Empty;
        public string FeatName = string.Empty;
        public string SpellId = string.Empty;
        public string SpellName = string.Empty;
        public string Size = string.Empty;
        public int Speed;
        public string HpModeId = CharacterHpModeIds.Custom;
        public int MaxHp;
        public int Strength = 10;
        public int Dexterity = 10;
        public int Constitution = 10;
        public int Intelligence = 10;
        public int Wisdom = 10;
        public int Charisma = 10;
        public string SavingThrows = string.Empty;
        public string Skills = string.Empty;
        public string WeaponProficiencies = string.Empty;
        public string ToolProficiencies = string.Empty;
        public string Senses = string.Empty;
        public string Languages = string.Empty;
        public string DamageResistances = string.Empty;
        public string PendingSelections = string.Empty;
        public string ConditionalBenefits = string.Empty;
        public string Traits = string.Empty;
        public string Notes = string.Empty;

        public ChapterCreatureData ToChapterCreatureData()
        {
            return ChapterCreatureDataStructureUtility.NormalizeCreatureTemplateData(new ChapterCreatureData
            {
                CreatureId = string.IsNullOrWhiteSpace(CharacterId) ? string.Empty : CharacterId,
                Name = string.IsNullOrWhiteSpace(CharacterName) ? "未命名角色" : CharacterName,
                CreatureType = "玩家角色",
                CreatureSize = Size ?? string.Empty,
                Alignment = Alignment ?? string.Empty,
                Speed = Speed > 0 ? $"{Speed} 尺" : string.Empty,
                Strength = Strength.ToString(),
                Dexterity = Dexterity.ToString(),
                Constitution = Constitution.ToString(),
                Intelligence = Intelligence.ToString(),
                Wisdom = Wisdom.ToString(),
                Charisma = Charisma.ToString(),
                DamageResistances = DamageResistances ?? string.Empty,
                SavingThrows = SavingThrows ?? string.Empty,
                Skills = Skills ?? string.Empty,
                Senses = Senses ?? string.Empty,
                Languages = Languages ?? string.Empty,
                Traits = Traits ?? string.Empty,
                BattleNotes = Notes ?? string.Empty
            });
        }

        public static CharacterRuntimeSnapshotData Clone(CharacterRuntimeSnapshotData source)
        {
            if (source == null)
            {
                return new CharacterRuntimeSnapshotData();
            }

            return new CharacterRuntimeSnapshotData
            {
                CharacterId = source.CharacterId ?? string.Empty,
                CharacterName = source.CharacterName ?? string.Empty,
                Alignment = source.Alignment ?? string.Empty,
                Level = Math.Max(1, source.Level),
                RaceId = source.RaceId ?? string.Empty,
                RaceName = source.RaceName ?? string.Empty,
                MainRaceId = source.MainRaceId ?? string.Empty,
                MainRaceName = source.MainRaceName ?? string.Empty,
                ClassId = source.ClassId ?? string.Empty,
                ClassName = source.ClassName ?? string.Empty,
                BackgroundId = source.BackgroundId ?? string.Empty,
                BackgroundName = source.BackgroundName ?? string.Empty,
                FeatId = source.FeatId ?? string.Empty,
                FeatName = source.FeatName ?? string.Empty,
                SpellId = source.SpellId ?? string.Empty,
                SpellName = source.SpellName ?? string.Empty,
                Size = source.Size ?? string.Empty,
                Speed = source.Speed,
                HpModeId = CharacterHpModeIds.Normalize(source.HpModeId),
                MaxHp = Math.Max(0, source.MaxHp),
                Strength = source.Strength,
                Dexterity = source.Dexterity,
                Constitution = source.Constitution,
                Intelligence = source.Intelligence,
                Wisdom = source.Wisdom,
                Charisma = source.Charisma,
                WeaponProficiencies = source.WeaponProficiencies ?? string.Empty,
                SavingThrows = source.SavingThrows ?? string.Empty,
                Skills = source.Skills ?? string.Empty,
                ToolProficiencies = source.ToolProficiencies ?? string.Empty,
                Senses = source.Senses ?? string.Empty,
                Languages = source.Languages ?? string.Empty,
                DamageResistances = source.DamageResistances ?? string.Empty,
                PendingSelections = source.PendingSelections ?? string.Empty,
                ConditionalBenefits = source.ConditionalBenefits ?? string.Empty,
                Traits = source.Traits ?? string.Empty,
                Notes = source.Notes ?? string.Empty
            };
        }
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
                HpModeId = CharacterHpModeIds.Custom,
                MaxHp = 0,
                ManualHp = 0,
                HpRolls = new List<CharacterHpRollSaveData>(),
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
            character.HpModeId = CharacterHpModeIds.Normalize(character.HpModeId);
            character.MaxHp = Math.Max(0, character.MaxHp);
            character.ManualHp = Math.Max(0, character.ManualHp);
            character.HpRolls = NormalizeHpRolls(character.HpRolls, character.ClassId);

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

        private static List<CharacterHpRollSaveData> NormalizeHpRolls(List<CharacterHpRollSaveData> source, string classId)
        {
            List<CharacterHpRollSaveData> result = new List<CharacterHpRollSaveData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterHpRollSaveData roll = source[index];
                if (roll == null)
                {
                    continue;
                }

                result.Add(new CharacterHpRollSaveData
                {
                    Level = Math.Max(1, roll.Level),
                    ClassId = string.IsNullOrWhiteSpace(roll.ClassId) ? (classId ?? string.Empty) : roll.ClassId,
                    HitDie = Math.Max(0, roll.HitDie),
                    RollValue = Math.Max(0, roll.RollValue),
                    ConstitutionModifier = roll.ConstitutionModifier,
                    HpGain = Math.Max(0, roll.HpGain)
                });
            }

            return result;
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

            // Layout stays on the prefab. Runtime only changes data and visual state.
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
