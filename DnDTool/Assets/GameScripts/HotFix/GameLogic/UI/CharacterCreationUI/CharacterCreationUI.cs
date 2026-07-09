using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using TMPro;
using TEngine;
using UnityEngine;
using UnityEngine.UI;
using Log = TEngine.Log;

namespace GameLogic
{
    internal enum CharacterInventoryQuickRollPurpose
    {
        DisplayOnly,
        HealHp,
        AttackHit,
        Damage,
        SkillCheck,
        SavingThrow,
        SpellAttack,
        SpellSaveDc,
        Custom
    }

    internal readonly struct CharacterCreationSkillDisplayBinding
    {
        public readonly string SkillId;
        public readonly string DisplayName;
        public readonly AbilityKind Ability;
        public readonly string ItemName;
        public readonly string LabelName;
        public readonly string BonusName;

        public CharacterCreationSkillDisplayBinding(string skillId, string displayName, AbilityKind ability, string itemName, string labelName, string bonusName)
        {
            SkillId = skillId;
            DisplayName = displayName;
            Ability = ability;
            ItemName = itemName;
            LabelName = labelName;
            BonusName = bonusName;
        }
    }

    [Window(UILayer.UI, location: "CharacterCreationUI", fullScreen: true)]
    public partial class CharacterCreationUI
    {
        private readonly Button[] m_btnSpellFilters = new Button[11];
        private Button m_btnPortraitUpload;
        private Image m_imgPortraitUpload;
        private TMP_Text m_tmpPortraitUploadHint;
        private GameObject m_goClassTemplate;
        private GameObject m_goRaceTemplate;
        private GameObject m_goBackgroundTemplate;
        private GameObject m_goAlignmentTemplate;
        private GameObject m_goEquipmentToolTemplate;
        private GameObject m_goClassFeatureTemplate;
        private GameObject m_goRaceFeatureTemplate;
        private RectTransform m_rectSkillProficiencies;
        private GameObject m_goOtherFeatureTemplate;
        private RectTransform m_rectStatusEffectContent;
        private GameObject m_goStatusEffectTemplate;
        private GameObject m_goSpellTemplate;
        private GameObject m_goSpellInfoTemplate;
        private GameObject m_goItemInfoTemplate;
        private GameObject m_goLearnedSpellTemplate;
        private TMP_InputField m_inputStrength;
        private TMP_InputField m_inputDexterity;
        private TMP_InputField m_inputConstitution;
        private TMP_InputField m_inputIntelligence;
        private TMP_InputField m_inputWisdom;
        private TMP_InputField m_inputCharisma;
        private readonly TMP_Text[] m_tmpSkillBonuses = new TMP_Text[18];
        private readonly Image[] m_imgSkillBackgrounds = new Image[18];
        private readonly Color[] m_defaultSkillBackgroundColors = new Color[18];
        private readonly CanvasGroup[] m_canvasSkillItems = new CanvasGroup[18];
        private readonly Button[] m_btnSkillItems = new Button[18];
        private readonly Button[] m_btnAbilityItems = new Button[6];
        private Button m_btnCurrentHpDecrease;
        private Button m_btnCurrentHpIncrease;
        private Button m_btnInventoryQuickRollAction;
        private TMP_Text m_tmpInventoryQuickRollActionLabel;
        private RectTransform m_rectInventoryQuickRollPurposeButtons;
        private Button m_btnQuickRollPurposeDisplayOnly;
        private Button m_btnQuickRollPurposeHealHp;
        private Button m_btnQuickRollPurposeAttackHit;
        private Button m_btnQuickRollPurposeDamage;
        private Button m_btnQuickRollPurposeSkillCheck;
        private Button m_btnQuickRollPurposeSavingThrow;
        private Button m_btnQuickRollPurposeSpellAttack;
        private Button m_btnQuickRollPurposeSpellSaveDc;
        private Button m_btnQuickRollPurposeCustom;
        private Button m_btnQuickRollApplyResult;
        private GameObject m_goCustomFeaturePopup;
        private bool m_isUpdatingLevelInput;
        private string m_pendingAbilityGenerationMethodId = string.Empty;
        private string m_pendingHitPointGenerationMethodId = string.Empty;
        private Texture2D m_portraitTexture;
        private Sprite m_portraitSprite;
        private string m_previewImagePath = string.Empty;
        private int m_portraitLoadVersion;
        private CharacterCreationFeatureChoiceState m_visibleFeatureChoiceState;
        private string m_visibleInventoryItemInstanceId = string.Empty;
        private CharacterInventoryQuickRollContext m_visibleInventoryQuickRollContext;
        private CharacterDiceRollResultData m_visibleInventoryQuickRollResult;
        private CharacterInventoryQuickRollPurpose m_visibleInventoryQuickRollPurpose = CharacterInventoryQuickRollPurpose.DisplayOnly;
        private string m_visibleInventoryQuickRollHistoryEntryId = string.Empty;

        private const int MinCharacterLevel = 1;
        private const int MaxCharacterLevel = 20;
        private const int BaseAbilityScore = 10;
        private const float SectionButtonMinHeight = 60f;
        private const float SkillProficientAlpha = 1f;
        private const float SkillNotProficientAlpha = 0.5f;
        private static readonly Color SkillChoiceCandidateBackgroundColor = new Color(1f, 0.92f, 0.55f, 1f);
        private static readonly CharacterCreationSkillDisplayBinding[] SkillDisplayBindings =
        {
            new CharacterCreationSkillDisplayBinding("athletics", "运动", AbilityKind.Strength, "m_itemSkillAthletics", "m_tmpSkillAthleticsLabel", "m_tmpSkillAthleticsBonus"),
            new CharacterCreationSkillDisplayBinding("acrobatics", "体操", AbilityKind.Dexterity, "m_itemSkillAcrobatics", "m_tmpSkillAcrobaticsLabel", "m_tmpSkillAcrobaticsBonus"),
            new CharacterCreationSkillDisplayBinding("sleight_of_hand", "巧手", AbilityKind.Dexterity, "m_itemSkillSleightOfHand", "m_tmpSkillSleightOfHandLabel", "m_tmpSkillSleightOfHandBonus"),
            new CharacterCreationSkillDisplayBinding("stealth", "隐匿", AbilityKind.Dexterity, "m_itemSkillStealth", "m_tmpSkillStealthLabel", "m_tmpSkillStealthBonus"),
            new CharacterCreationSkillDisplayBinding("arcana", "奥秘", AbilityKind.Intelligence, "m_itemSkillArcana", "m_tmpSkillArcanaLabel", "m_tmpSkillArcanaBonus"),
            new CharacterCreationSkillDisplayBinding("history", "历史", AbilityKind.Intelligence, "m_itemSkillHistory", "m_tmpSkillHistoryLabel", "m_tmpSkillHistoryBonus"),
            new CharacterCreationSkillDisplayBinding("investigation", "调查", AbilityKind.Intelligence, "m_itemSkillInvestigation", "m_tmpSkillInvestigationLabel", "m_tmpSkillInvestigationBonus"),
            new CharacterCreationSkillDisplayBinding("nature", "自然", AbilityKind.Intelligence, "m_itemSkillNature", "m_tmpSkillNatureLabel", "m_tmpSkillNatureBonus"),
            new CharacterCreationSkillDisplayBinding("religion", "宗教", AbilityKind.Intelligence, "m_itemSkillReligion", "m_tmpSkillReligionLabel", "m_tmpSkillReligionBonus"),
            new CharacterCreationSkillDisplayBinding("animal_handling", "驯兽", AbilityKind.Wisdom, "m_itemSkillAnimalHandling", "m_tmpSkillAnimalHandlingLabel", "m_tmpSkillAnimalHandlingBonus"),
            new CharacterCreationSkillDisplayBinding("insight", "洞悉", AbilityKind.Wisdom, "m_itemSkillInsight", "m_tmpSkillInsightLabel", "m_tmpSkillInsightBonus"),
            new CharacterCreationSkillDisplayBinding("medicine", "医药", AbilityKind.Wisdom, "m_itemSkillMedicine", "m_tmpSkillMedicineLabel", "m_tmpSkillMedicineBonus"),
            new CharacterCreationSkillDisplayBinding("perception", "察觉", AbilityKind.Wisdom, "m_itemSkillPerception", "m_tmpSkillPerceptionLabel", "m_tmpSkillPerceptionBonus"),
            new CharacterCreationSkillDisplayBinding("survival", "求生", AbilityKind.Wisdom, "m_itemSkillSurvival", "m_tmpSkillSurvivalLabel", "m_tmpSkillSurvivalBonus"),
            new CharacterCreationSkillDisplayBinding("deception", "欺瞒", AbilityKind.Charisma, "m_itemSkillDeception", "m_tmpSkillDeceptionLabel", "m_tmpSkillDeceptionBonus"),
            new CharacterCreationSkillDisplayBinding("intimidation", "威吓", AbilityKind.Charisma, "m_itemSkillIntimidation", "m_tmpSkillIntimidationLabel", "m_tmpSkillIntimidationBonus"),
            new CharacterCreationSkillDisplayBinding("performance", "表演", AbilityKind.Charisma, "m_itemSkillPerformance", "m_tmpSkillPerformanceLabel", "m_tmpSkillPerformanceBonus"),
            new CharacterCreationSkillDisplayBinding("persuasion", "游说", AbilityKind.Charisma, "m_itemSkillPersuasion", "m_tmpSkillPersuasionLabel", "m_tmpSkillPersuasionBonus")
        };

        private readonly List<CharacterCreationClassCardView> m_classCards = new List<CharacterCreationClassCardView>();
        private readonly List<CharacterCreationRaceCardView> m_raceCards = new List<CharacterCreationRaceCardView>();
        private readonly List<CharacterCreationBackgroundCardView> m_backgroundCards = new List<CharacterCreationBackgroundCardView>();
        private readonly List<CharacterCreationAlignmentCardView> m_alignmentCards = new List<CharacterCreationAlignmentCardView>();
        private readonly List<CharacterCreationSelectionOptionCardView> m_selectionOptionCards = new List<CharacterCreationSelectionOptionCardView>();
        private readonly List<CharacterCreationLabelItemView> m_equipmentToolItems = new List<CharacterCreationLabelItemView>();
        private readonly List<CharacterCreationLabelItemView> m_inventoryItems = new List<CharacterCreationLabelItemView>();
        private readonly List<CharacterCreationLabelItemView> m_classFeatureItems = new List<CharacterCreationLabelItemView>();
        private readonly List<CharacterCreationLabelItemView> m_raceFeatureItems = new List<CharacterCreationLabelItemView>();
        private readonly List<CharacterCreationLabelItemView> m_otherFeatureItems = new List<CharacterCreationLabelItemView>();
        private readonly List<CharacterCreationStatusEffectItemView> m_statusEffectItems = new List<CharacterCreationStatusEffectItemView>();
        private readonly List<CharacterCreationSpellCardView> m_availableSpellCards = new List<CharacterCreationSpellCardView>();
        private readonly List<CharacterCreationSpellCardView> m_learnedSpellCards = new List<CharacterCreationSpellCardView>();
        private readonly List<CharacterCreationItemInfoCardView> m_localItemCards = new List<CharacterCreationItemInfoCardView>();
        private List<LocalCustomItemSaveData> m_localItemOptions = new List<LocalCustomItemSaveData>();
        private readonly Dictionary<string, string> m_toolChoiceGroupIdByLabel = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> m_toolChoiceSourceTypeByLabel = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> m_toolChoiceSourceIdByLabel = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private List<CharacterCreationSkillChoiceState> m_skillChoiceStates => CharacterCreationSessionService.Instance.SkillChoiceStates;
        private List<CharacterCreationToolChoiceState> m_toolChoiceStates => CharacterCreationSessionService.Instance.ToolChoiceStates;
        private List<CharacterCreationFeatureChoiceState> m_featureChoiceStates => CharacterCreationSessionService.Instance.FeatureChoiceStates;
        private int m_activeSpellFilterLevel = -1;
        private string m_pendingLocalItemId = string.Empty;
        private string SelectedClassId
        {
            get => CharacterCreationSessionService.Instance.CurrentState?.Character?.ClassId ?? string.Empty;
            set => CharacterCreationSessionService.Instance.SetSelectedClass(value ?? string.Empty);
        }

        private string SelectedRaceId
        {
            get => CharacterCreationSessionService.Instance.CurrentState?.Character?.RaceId ?? string.Empty;
            set => CharacterCreationSessionService.Instance.SetSelectedRace(value ?? string.Empty);
        }

        private string SelectedBackgroundId
        {
            get => CharacterCreationSessionService.Instance.CurrentState?.Character?.BackgroundId ?? string.Empty;
            set => CharacterCreationSessionService.Instance.SetSelectedBackground(value ?? string.Empty);
        }

        private string SelectedAlignmentId
        {
            get => CharacterCreationSessionService.Instance.CurrentState?.Character?.Alignment ?? string.Empty;
            set => CharacterCreationSessionService.Instance.SetSelectedAlignment(value ?? string.Empty);
        }
        private CharacterCreationToolChoiceState m_activeToolChoiceState
        {
            get => CharacterCreationSessionService.Instance.ActiveToolChoiceState;
            set
            {
                if (value == null)
                {
                    CharacterCreationSessionService.Instance.ClearActiveChoice();
                }
                else
                {
                    CharacterCreationSessionService.Instance.SetActiveToolChoice(value);
                }
            }
        }

        private CharacterCreationFeatureChoiceState m_activeFeatureChoiceState
        {
            get => CharacterCreationSessionService.Instance.ActiveFeatureChoiceState;
            set
            {
                if (value == null)
                {
                    CharacterCreationSessionService.Instance.ClearActiveChoice();
                }
                else
                {
                    CharacterCreationSessionService.Instance.SetActiveFeatureChoice(value);
                }
            }
        }

        protected override void OnCreate()
        {
            CharacterCreationSessionService.Instance.BeginNewDraft();
            InitializeGeneratedBindings();
            BindLevelInput();
            BindRoleplayInputs();
            BindCustomFeatureInputs();
            HideRightPanelTemplates();
            CloseCustomFeaturePopup();
            ClearSelectionList();
            RebuildSkillChoiceStates();
            RefreshCreationView();
        }

        protected override void OnDestroy()
        {
            CleanupPortraitResources();
        }

        private void InitializeGeneratedBindings()
        {
            BindSpellFilterControls();
            m_btnPortraitUpload = m_btnCharacterPortraitUploadArea;
            m_imgPortraitUpload = m_btnCharacterPortraitUploadArea != null ? m_btnCharacterPortraitUploadArea.GetComponent<Image>() : null;
            m_tmpPortraitUploadHint = m_tmpCharacterPortraitUploadHint;
            if (m_imgPortraitUpload != null)
            {
                m_imgPortraitUpload.preserveAspect = true;
            }

            m_goClassTemplate = m_itemClassTemplate;
            m_goRaceTemplate = m_itemRaceTemplate;
            m_goBackgroundTemplate = m_itemBackgroundTemplate;
            m_goAlignmentTemplate = m_itemAlignmentTemplate;
            m_goEquipmentToolTemplate = m_itemEquipmentToolTemplate;
            m_goClassFeatureTemplate = m_itemClassFeatureTemplate;
            m_goRaceFeatureTemplate = m_itemRaceFeatureTemplate;
            m_rectSkillProficiencies = m_gridSkillProficiencies != null ? m_gridSkillProficiencies.transform as RectTransform : null;
            m_goOtherFeatureTemplate = m_itemOtherFeatureTemplate;
            m_rectStatusEffectContent = m_gridStatusEffects != null ? m_gridStatusEffects.transform as RectTransform : null;
            m_goStatusEffectTemplate = m_itemStatusEffectTemplate;
            m_goSpellTemplate = m_itemSpellTemplate;
            m_goSpellInfoTemplate = m_itemSpellInfoTemplate;
            m_goItemInfoTemplate = m_itemItemInfoTemplate;
            m_goLearnedSpellTemplate = m_goSpellTemplate;
            BindCurrentHpAdjustButtons();
            BindInventoryQuickRollAction();
            BindInventoryQuickRollPurposeActions();
            HideInventoryActionButtons();
            BindSkillItems();
            BindAbilityItems();
            BindAbilityScoreInputs();
            m_goCustomFeaturePopup = FindChildComponent<RectTransform>("m_popupCustomFeature")?.gameObject;
        }

        private void BindCurrentHpAdjustButtons()
        {
            m_btnCurrentHpDecrease = FindChildComponent<Button>("m_btnCurrentHpDecrease");
            m_btnCurrentHpIncrease = FindChildComponent<Button>("m_btnCurrentHpIncrease");
            if (m_btnCurrentHpDecrease != null)
            {
                m_btnCurrentHpDecrease.onClick.RemoveAllListeners();
                m_btnCurrentHpDecrease.onClick.AddListener(() => ChangeCurrentHpByButton(-1));
            }

            if (m_btnCurrentHpIncrease != null)
            {
                m_btnCurrentHpIncrease.onClick.RemoveAllListeners();
                m_btnCurrentHpIncrease.onClick.AddListener(() => ChangeCurrentHpByButton(1));
            }
        }

        private void BindInventoryQuickRollAction()
        {
            m_btnInventoryQuickRollAction = FindChildComponent<Button>("m_btnInventoryQuickRollAction");
            m_tmpInventoryQuickRollActionLabel = FindChildComponent<TMP_Text>("m_tmpInventoryQuickRollActionLabel");
            if (m_btnInventoryQuickRollAction == null)
            {
                return;
            }

            m_btnInventoryQuickRollAction.onClick.RemoveAllListeners();
            m_btnInventoryQuickRollAction.onClick.AddListener(OnClickInventoryQuickRollActionBtn);
        }

        private void BindInventoryQuickRollPurposeActions()
        {
            m_rectInventoryQuickRollPurposeButtons = FindChildComponent<RectTransform>("m_rectInventoryQuickRollPurposeButtons");
            m_btnQuickRollPurposeDisplayOnly = BindQuickRollPurposeButton("m_btnQuickRollPurposeDisplayOnly", CharacterInventoryQuickRollPurpose.DisplayOnly);
            m_btnQuickRollPurposeHealHp = BindQuickRollPurposeButton("m_btnQuickRollPurposeHealHp", CharacterInventoryQuickRollPurpose.HealHp);
            m_btnQuickRollPurposeAttackHit = BindQuickRollPurposeButton("m_btnQuickRollPurposeAttackHit", CharacterInventoryQuickRollPurpose.AttackHit);
            m_btnQuickRollPurposeDamage = BindQuickRollPurposeButton("m_btnQuickRollPurposeDamage", CharacterInventoryQuickRollPurpose.Damage);
            m_btnQuickRollPurposeSkillCheck = BindQuickRollPurposeButton("m_btnQuickRollPurposeSkillCheck", CharacterInventoryQuickRollPurpose.SkillCheck);
            m_btnQuickRollPurposeSavingThrow = BindQuickRollPurposeButton("m_btnQuickRollPurposeSavingThrow", CharacterInventoryQuickRollPurpose.SavingThrow);
            m_btnQuickRollPurposeSpellAttack = BindQuickRollPurposeButton("m_btnQuickRollPurposeSpellAttack", CharacterInventoryQuickRollPurpose.SpellAttack);
            m_btnQuickRollPurposeSpellSaveDc = BindQuickRollPurposeButton("m_btnQuickRollPurposeSpellSaveDc", CharacterInventoryQuickRollPurpose.SpellSaveDc);
            m_btnQuickRollPurposeCustom = BindQuickRollPurposeButton("m_btnQuickRollPurposeCustom", CharacterInventoryQuickRollPurpose.Custom);
            m_btnQuickRollApplyResult = FindChildComponent<Button>("m_btnQuickRollApplyResult");
            if (m_btnQuickRollApplyResult != null)
            {
                m_btnQuickRollApplyResult.onClick.RemoveAllListeners();
                m_btnQuickRollApplyResult.onClick.AddListener(OnClickQuickRollApplyResultButton);
            }

            SetActive(m_rectInventoryQuickRollPurposeButtons != null ? m_rectInventoryQuickRollPurposeButtons.gameObject : null, false);
        }

        private Button BindQuickRollPurposeButton(string buttonName, CharacterInventoryQuickRollPurpose purpose)
        {
            Button button = FindChildComponent<Button>(buttonName);
            if (button == null)
            {
                return null;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnClickQuickRollPurposeButton(purpose));
            return button;
        }

        private void ChangeCurrentHpByButton(int delta)
        {
            CharacterCreationViewState state = CharacterCreationViewStateService.Instance.BuildCurrentViewState(
                ParseLevel(m_tmpInputLevel != null ? m_tmpInputLevel.text : string.Empty));
            int maxHp = ParsePlainInt(state?.MaxHpText);
            if (CharacterCreationSessionService.Instance.ChangeCurrentHp(delta, maxHp))
            {
                RefreshCreationView();
            }
        }

        private void BindSkillItems()
        {
            for (int index = 0; index < SkillDisplayBindings.Length; index++)
            {
                CharacterCreationSkillDisplayBinding binding = SkillDisplayBindings[index];
                RectTransform item = FindChildComponent<RectTransform>(binding.ItemName);
                m_tmpSkillBonuses[index] = FindChildComponent<TMP_Text>(binding.BonusName);
                m_imgSkillBackgrounds[index] = item != null ? item.GetComponent<Image>() : null;
                m_defaultSkillBackgroundColors[index] = m_imgSkillBackgrounds[index] != null ? m_imgSkillBackgrounds[index].color : Color.white;
                if (item != null)
                {
                    CanvasGroup canvasGroup = item.GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                    {
                        canvasGroup = item.gameObject.AddComponent<CanvasGroup>();
                    }

                    m_canvasSkillItems[index] = canvasGroup;
                    Button button = item.GetComponent<Button>();
                    if (button == null)
                    {
                        button = item.gameObject.AddComponent<Button>();
                    }

                    button.transition = Selectable.Transition.None;
                    int capturedIndex = index;
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => OnClickSkillItem(capturedIndex));
                    m_btnSkillItems[index] = button;
                }
            }

            SetActive(m_rectSkillProficiencies != null ? m_rectSkillProficiencies.gameObject : null, true);
        }

        private void BindAbilityItems()
        {
            BindAbilityItem(0, "m_abilityStrength", "Strength");
            BindAbilityItem(1, "m_abilityDexterity", "Dexterity");
            BindAbilityItem(2, "m_abilityConstitution", "Constitution");
            BindAbilityItem(3, "m_abilityIntelligence", "Intelligence");
            BindAbilityItem(4, "m_abilityWisdom", "Wisdom");
            BindAbilityItem(5, "m_abilityCharisma", "Charisma");
        }

        private void BindAbilityItem(int index, string itemName, string abilityId)
        {
            RectTransform item = FindChildComponent<RectTransform>(itemName);
            if (item == null || index < 0 || index >= m_btnAbilityItems.Length)
            {
                return;
            }

            Button button = item.GetComponent<Button>();
            if (button == null)
            {
                button = item.gameObject.AddComponent<Button>();
            }

            button.transition = Selectable.Transition.None;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnClickAbilityItem(abilityId));
            m_btnAbilityItems[index] = button;
        }

        private void BindSpellFilterControls()
        {
            m_btnSpellFilters[0] = FindChildComponent<Button>("m_btnSpellFilterAll");
            m_btnSpellFilters[1] = FindChildComponent<Button>("m_btnSpellFilterCantrip");
            for (int level = 1; level <= 9; level++)
            {
                m_btnSpellFilters[level + 1] = FindChildComponent<Button>($"m_btnSpellFilterLevel{level}");
            }
        }

        private void BindAbilityScoreInputs()
        {
            m_inputStrength = BindAbilityScoreInput(m_tmpStrength, "Strength");
            m_inputDexterity = BindAbilityScoreInput(m_tmpDexterity, "Dexterity");
            m_inputConstitution = BindAbilityScoreInput(m_tmpConstitution, "Constitution");
            m_inputIntelligence = BindAbilityScoreInput(m_tmpIntelligence, "Intelligence");
            m_inputWisdom = BindAbilityScoreInput(m_tmpWisdom, "Wisdom");
            m_inputCharisma = BindAbilityScoreInput(m_tmpCharisma, "Charisma");
        }

        private TMP_InputField BindAbilityScoreInput(TMP_Text scoreText, string abilityId)
        {
            if (scoreText == null)
            {
                return null;
            }

            TMP_InputField input = scoreText.GetComponent<TMP_InputField>();
            if (input == null)
            {
                Log.Warning($"CharacterCreationUI ability score input is missing TMP_InputField: {scoreText.name}");
                return null;
            }

            if (input.textComponent == null)
            {
                input.textComponent = scoreText;
            }

            if (input.textViewport == null)
            {
                input.textViewport = input.textComponent != null ? input.textComponent.rectTransform : scoreText.rectTransform;
            }

            input.contentType = TMP_InputField.ContentType.IntegerNumber;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.characterLimit = 2;
            input.interactable = false;
            input.onEndEdit.RemoveAllListeners();
            input.onEndEdit.AddListener(value => OnAbilityScoreInputEndEdit(abilityId, value));
            return input;
        }

        private void OpenCustomFeaturePopup()
        {
            if (m_tmpInputCustomFeatureName != null)
            {
                m_tmpInputCustomFeatureName.SetTextWithoutNotify(string.Empty);
            }

            if (m_tmpInputCustomFeatureDescription != null)
            {
                m_tmpInputCustomFeatureDescription.SetTextWithoutNotify(string.Empty);
            }

            SetActive(m_goCustomFeaturePopup, true);
            if (m_tmpInputCustomFeatureName != null)
            {
                m_tmpInputCustomFeatureName.Select();
                m_tmpInputCustomFeatureName.ActivateInputField();
            }
        }

        private void CloseCustomFeaturePopup()
        {
            SetActive(m_goCustomFeaturePopup, false);
        }

        private void ConfirmCustomFeaturePopup()
        {
            string featureName = m_tmpInputCustomFeatureName != null ? m_tmpInputCustomFeatureName.text : string.Empty;
            string description = m_tmpInputCustomFeatureDescription != null ? m_tmpInputCustomFeatureDescription.text : string.Empty;
            if (!CharacterCreationSessionService.Instance.AddCustomFeature(featureName, description))
            {
                Log.Warning("自定义特性名称与描述不能同时为空。");
                return;
            }

            CloseCustomFeaturePopup();
            SetActive(m_rectOtherFeatureContent != null ? m_rectOtherFeatureContent.gameObject : null, true);
            RefreshCreationView();
        }

        private void ShowLocalInventoryItemOptions()
        {
            HideInventoryActionButtons();
            if (m_rectInfoListContent == null)
            {
                Log.Warning("CharacterCreationUI: item option list binding is missing.");
                return;
            }

            ClearSelectionList();
            HideRightPanelTemplates();
            m_pendingLocalItemId = string.Empty;
            m_localItemOptions = LoadLocalItemOptions();
            RefreshLocalItemOptionCards();

            if (m_localItemOptions.Count == 0)
            {
                SetText(m_tmpFeatureDetailTitle, "本地物品库");
                SetText(m_tmpFeatureDetailDescription, "当前没有已保存的本地物品。请先在物品编辑界面保存物品。");
                return;
            }

            SetText(m_tmpFeatureDetailTitle, "本地物品库");
            SetText(m_tmpFeatureDetailDescription, "选择一个物品查看详情，点击确认后加入角色背包。");
        }

        private void RefreshLocalItemOptionCards()
        {
            for (int index = 0; index < m_localItemCards.Count; index++)
            {
                m_localItemCards[index].SetActive(false);
            }

            EnsureLocalItemCardCount(m_localItemOptions.Count);

            for (int index = 0; index < m_localItemCards.Count; index++)
            {
                CharacterCreationItemInfoCardView card = m_localItemCards[index];
                bool active = index < m_localItemOptions.Count;
                card.SetActive(active);
                if (!active)
                {
                    continue;
                }

                LocalCustomItemSaveData option = m_localItemOptions[index];
                string customItemId = option.CustomItemId?.Trim() ?? string.Empty;
                card.Bind(
                    option,
                    string.Equals(m_pendingLocalItemId, customItemId, StringComparison.OrdinalIgnoreCase),
                    () => OnClickLocalItemOption(customItemId),
                    () => OnDeleteLocalItemOption(customItemId));
            }
        }

        private void BindLevelInput()
        {
            if (m_tmpInputLevel == null)
            {
                return;
            }

            m_tmpInputLevel.contentType = TMP_InputField.ContentType.IntegerNumber;
            m_tmpInputLevel.lineType = TMP_InputField.LineType.SingleLine;
            m_tmpInputLevel.characterLimit = 2;
            if (string.IsNullOrWhiteSpace(m_tmpInputLevel.text))
            {
                m_tmpInputLevel.SetTextWithoutNotify(MinCharacterLevel.ToString());
            }

            m_tmpInputLevel.onValueChanged.RemoveAllListeners();
            m_tmpInputLevel.onEndEdit.RemoveAllListeners();
            m_tmpInputLevel.onValueChanged.AddListener(OnLevelInputChanged);
            m_tmpInputLevel.onEndEdit.AddListener(OnLevelInputEndEdit);
        }

        private void BindRoleplayInputs()
        {
            ConfigureRoleplayInput(m_tmpInputPersonalityTraits);
            ConfigureRoleplayInput(m_tmpInputIdeals);
            ConfigureRoleplayInput(m_tmpInputBonds);
            ConfigureRoleplayInput(m_tmpInputFlaws);
        }

        private void BindCustomFeatureInputs()
        {
            ConfigureRoleplayInput(m_tmpInputCustomFeatureName);
            ConfigureRoleplayInput(m_tmpInputCustomFeatureDescription);
            if (m_tmpInputCustomFeatureName != null)
            {
                m_tmpInputCustomFeatureName.lineType = TMP_InputField.LineType.SingleLine;
            }
        }

        private static void ConfigureRoleplayInput(TMP_InputField input)
        {
            if (input == null)
            {
                return;
            }

            input.contentType = TMP_InputField.ContentType.Standard;
            input.lineType = TMP_InputField.LineType.MultiLineNewline;
            input.characterLimit = 0;
        }

        private void OnLevelInputChanged(string value)
        {
            if (m_isUpdatingLevelInput || m_tmpInputLevel == null)
            {
                return;
            }

            string digits = KeepDigitsOnly(value);
            if (digits == value)
            {
                return;
            }

            m_isUpdatingLevelInput = true;
            m_tmpInputLevel.SetTextWithoutNotify(digits);
            m_isUpdatingLevelInput = false;
        }

        private void OnLevelInputEndEdit(string value)
        {
            if (m_tmpInputLevel == null)
            {
                return;
            }

            int level = ParseLevel(value);
            m_tmpInputLevel.SetTextWithoutNotify(level.ToString());
            CharacterCreationSessionService.Instance.SetLevel(level);
            RebuildFeatureChoiceStates();
            RefreshCreationView();
        }

        private void ShowClassOptions()
        {
            if (m_rectInfoListContent == null || m_goClassTemplate == null)
            {
                Log.Warning("CharacterCreationUI: class option list binding is missing.");
                return;
            }

            ClearPendingSpellSelection();
            HideRightPanelTemplates();
            List<CharacterCreationOptionViewState> options = CharacterCreationRuleService.Instance.GetClassOptions();
            EnsureClassCardCount(options.Count);

            for (int index = 0; index < m_classCards.Count; index++)
            {
                CharacterCreationClassCardView card = m_classCards[index];
                bool active = index < options.Count;
                card.SetActive(active);
                if (!active)
                {
                    continue;
                }

                CharacterCreationOptionViewState option = options[index];
                string classId = option?.Id ?? string.Empty;
                card.Bind(
                    classId,
                    option?.Name ?? string.Empty,
                    string.Equals(SelectedClassId, classId, StringComparison.OrdinalIgnoreCase),
                    OnClickClassCard);
            }
        }

        private void ShowRaceOptions()
        {
            if (m_rectInfoListContent == null || m_goRaceTemplate == null)
            {
                Log.Warning("CharacterCreationUI: race option list binding is missing.");
                return;
            }

            ClearPendingSpellSelection();
            HideRightPanelTemplates();
            List<CharacterCreationOptionViewState> options = CharacterCreationRuleService.Instance.GetRaceOptions();
            EnsureRaceCardCount(options.Count);

            for (int index = 0; index < m_raceCards.Count; index++)
            {
                CharacterCreationRaceCardView card = m_raceCards[index];
                bool active = index < options.Count;
                card.SetActive(active);
                if (!active)
                {
                    continue;
                }

                CharacterCreationOptionViewState option = options[index];
                string raceId = option?.Id ?? string.Empty;
                card.Bind(
                    raceId,
                    option?.Name ?? string.Empty,
                    string.Equals(SelectedRaceId, raceId, StringComparison.OrdinalIgnoreCase),
                    OnClickRaceCard);
            }
        }

        private void ShowBackgroundOptions()
        {
            if (m_rectInfoListContent == null || m_goBackgroundTemplate == null)
            {
                Log.Warning("CharacterCreationUI: background option list binding is missing.");
                return;
            }

            ClearPendingSpellSelection();
            HideRightPanelTemplates();
            List<CharacterCreationOptionViewState> options = CharacterCreationRuleService.Instance.GetBackgroundOptions();
            EnsureBackgroundCardCount(options.Count);

            for (int index = 0; index < m_backgroundCards.Count; index++)
            {
                CharacterCreationBackgroundCardView card = m_backgroundCards[index];
                bool active = index < options.Count;
                card.SetActive(active);
                if (!active)
                {
                    continue;
                }

                CharacterCreationOptionViewState option = options[index];
                string backgroundId = option?.Id ?? string.Empty;
                card.Bind(
                    backgroundId,
                    option?.Name ?? string.Empty,
                    string.Equals(SelectedBackgroundId, backgroundId, StringComparison.OrdinalIgnoreCase),
                    OnClickBackgroundCard);
            }
        }

        private void ShowAlignmentOptions()
        {
            if (m_rectInfoListContent == null || m_goAlignmentTemplate == null)
            {
                Log.Warning("CharacterCreationUI: alignment option list binding is missing.");
                return;
            }

            ClearPendingSpellSelection();
            HideRightPanelTemplates();
            List<CharacterCreationOptionViewState> options = CharacterCreationRuleService.Instance.GetAlignmentOptions();
            EnsureAlignmentCardCount(options.Count);

            for (int index = 0; index < m_alignmentCards.Count; index++)
            {
                CharacterCreationAlignmentCardView card = m_alignmentCards[index];
                bool active = index < options.Count;
                card.SetActive(active);
                if (!active)
                {
                    continue;
                }

                CharacterCreationOptionViewState option = options[index];
                string alignmentId = option?.Id ?? string.Empty;
                card.Bind(
                    alignmentId,
                    option?.Name ?? string.Empty,
                    string.Equals(SelectedAlignmentId, alignmentId, StringComparison.OrdinalIgnoreCase),
                    OnClickAlignmentCard);
            }
        }

        private void OnClickSpellSection()
        {
            if (m_rectSpellContent != null && !m_rectSpellContent.gameObject.activeSelf)
            {
                m_rectSpellContent.gameObject.SetActive(true);
            }

            ShowSpellOptions(m_activeSpellFilterLevel);
        }

        private partial void OnClickBackBtn()
        {
            ReturnToCharacterManagement();
        }

        private partial void OnClickSaveBtn()
        {
            SaveDraft();
        }

        private partial void OnClickConfirmDraftBtn()
        {
            ConfirmCurrentRightPanelSelection();
        }

        private partial void OnClickPanelRaceBtn()
        {
            ShowRaceOptions();
        }

        private partial void OnClickPanelBackgroundBtn()
        {
            ShowBackgroundOptions();
        }

        private partial void OnClickPanelAlignmentBtn()
        {
            ShowAlignmentOptions();
        }

        private partial void OnClickPanelClassBtn()
        {
            ShowClassOptions();
        }

        private partial void OnClickCharacterPortraitUploadAreaBtn()
        {
            UploadPortraitImageAsync().Forget();
        }

        private partial void OnClickGenerateHitPointsBtn()
        {
            OnClickHitPointGenerationButton();
        }

        private partial void OnClickSpellListBtn()
        {
            OnClickSpellSection();
        }

        private partial void OnSliderExperienceChange(float value)
        {
        }

        private partial void OnClickStrengthIncreaseBtn()
        {
            ChangeAbilityScore("Strength", 1);
        }

        private partial void OnClickStrengthDecreaseBtn()
        {
            ChangeAbilityScore("Strength", -1);
        }

        private partial void OnClickDexterityIncreaseBtn()
        {
            ChangeAbilityScore("Dexterity", 1);
        }

        private partial void OnClickDexterityDecreaseBtn()
        {
            ChangeAbilityScore("Dexterity", -1);
        }

        private partial void OnClickConstitutionIncreaseBtn()
        {
            ChangeAbilityScore("Constitution", 1);
        }

        private partial void OnClickConstitutionDecreaseBtn()
        {
            ChangeAbilityScore("Constitution", -1);
        }

        private partial void OnClickIntelligenceIncreaseBtn()
        {
            ChangeAbilityScore("Intelligence", 1);
        }

        private partial void OnClickIntelligenceDecreaseBtn()
        {
            ChangeAbilityScore("Intelligence", -1);
        }

        private partial void OnClickWisdomIncreaseBtn()
        {
            ChangeAbilityScore("Wisdom", 1);
        }

        private partial void OnClickWisdomDecreaseBtn()
        {
            ChangeAbilityScore("Wisdom", -1);
        }

        private partial void OnClickCharismaIncreaseBtn()
        {
            ChangeAbilityScore("Charisma", 1);
        }

        private partial void OnClickCharismaDecreaseBtn()
        {
            ChangeAbilityScore("Charisma", -1);
        }

        private partial void OnClickAbilityGenerationBtn()
        {
            OnClickAbilityGenerationButton();
        }

        private partial void OnClickSectionInventoryBtn()
        {
            ToggleRectActive(m_rectInventoryContent);
        }

        private partial void OnClickAddInventoryItemBtn()
        {
            ShowLocalInventoryItemOptions();
        }

        private partial void OnClickSectionSkillsBtn()
        {
            ToggleRectActive(m_rectSkillProficiencies);
        }

        private partial void OnClickSectionEquipmentToolsBtn()
        {
            ToggleRectActive(m_rectEquipmentToolContent);
        }

        private partial void OnClickSectionClassBtn()
        {
            ToggleRectActive(m_rectClassFeatureContent);
        }

        private partial void OnClickSectionRaceBtn()
        {
            ToggleRectActive(m_rectRaceFeatureContent);
        }

        private partial void OnClickSectionOtherFeaturesBtn()
        {
            ToggleRectActive(m_rectOtherFeatureContent);
        }

        private partial void OnClickAddCustomFeatureBtn()
        {
            OpenCustomFeaturePopup();
        }

        private partial void OnClickSectionSpellsBtn()
        {
            OnClickSpellSection();
        }

        private partial void OnClickCloseCustomFeatureBtn()
        {
            CloseCustomFeaturePopup();
        }

        private partial void OnClickCancelCustomFeatureBtn()
        {
            CloseCustomFeaturePopup();
        }

        private partial void OnClickConfirmCustomFeatureBtn()
        {
            ConfirmCustomFeaturePopup();
        }

        private partial void OnClickSpellFilterAllBtn()
        {
            ShowSpellOptions(-1);
        }

        private partial void OnClickSpellFilterCantripBtn()
        {
            ShowSpellOptions(0);
        }

        private partial void OnClickSpellFilterLevel1Btn()
        {
            ShowSpellOptions(1);
        }

        private partial void OnClickSpellFilterLevel2Btn()
        {
            ShowSpellOptions(2);
        }

        private partial void OnClickSpellFilterLevel3Btn()
        {
            ShowSpellOptions(3);
        }

        private partial void OnClickSpellFilterLevel4Btn()
        {
            ShowSpellOptions(4);
        }

        private partial void OnClickSpellFilterLevel5Btn()
        {
            ShowSpellOptions(5);
        }

        private partial void OnClickSpellFilterLevel6Btn()
        {
            ShowSpellOptions(6);
        }

        private partial void OnClickSpellFilterLevel7Btn()
        {
            ShowSpellOptions(7);
        }

        private partial void OnClickSpellFilterLevel8Btn()
        {
            ShowSpellOptions(8);
        }

        private partial void OnClickSpellFilterLevel9Btn()
        {
            ShowSpellOptions(9);
        }

        private void ShowSpellOptions(int filterLevel)
        {
            if (m_rectInfoListContent == null || m_goSpellInfoTemplate == null)
            {
                Log.Warning("CharacterCreationUI: spell option list binding is missing.");
                return;
            }

            m_activeSpellFilterLevel = filterLevel;
            HideRightPanelTemplates();
            CharacterCardDraftSaveData character = CharacterCreationSessionService.Instance.CurrentState?.Character ?? new CharacterCardDraftSaveData();
            int level = ParseLevel(m_tmpInputLevel != null ? m_tmpInputLevel.text : string.Empty);
            CharacterCreationSpellbookViewState spellbook = CharacterCreationSpellDisplayService.Instance.BuildSpellbook(character, level, filterLevel);
            RefreshSpellCardItems(
                m_availableSpellCards,
                m_rectInfoListContent,
                m_goSpellInfoTemplate,
                spellbook.AvailableSpells,
                "m_itemSpellOption_",
                OnClickAvailableSpellCard);
            HideInventoryActionButtons();
            SetText(m_tmpFeatureDetailTitle, "法术");
            SetText(m_tmpFeatureDetailDescription, spellbook.SummaryText);
        }


        private CharacterCreationToolChoiceState TryCreateToolChoiceState(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                return null;
            }

            string trimmedLabel = label.Trim();
            if (m_toolChoiceGroupIdByLabel.TryGetValue(trimmedLabel, out string mappedChoiceGroupId)
                && DndRuleContentService.Instance.TryGetChoiceGroup(mappedChoiceGroupId, out DndChoiceGroupData choiceGroup))
            {
                if (string.Equals(choiceGroup.ChoiceType, "SkillOrTool", StringComparison.OrdinalIgnoreCase))
                {
                    return CharacterCreationSessionService.Instance.CreateOrRefreshMixedToolChoiceState(choiceGroup.ChoiceGroupId, trimmedLabel);
                }

                if (!string.Equals(choiceGroup.ChoiceType, "Tool", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                if (m_toolChoiceSourceTypeByLabel.TryGetValue(trimmedLabel, out string mappedSourceType)
                    && !string.IsNullOrWhiteSpace(mappedSourceType))
                {
                    m_toolChoiceSourceIdByLabel.TryGetValue(trimmedLabel, out string mappedSourceId);
                    return TryCreateStructuredToolChoiceState(
                        choiceGroup,
                        trimmedLabel,
                        mappedSourceType,
                        mappedSourceId);
                }

                DndRaceDefineData raceData = null;
                DndBackgroundDefineData backgroundData = null;
                TryGetSelectedRace(out raceData);
                if (!string.IsNullOrWhiteSpace(SelectedBackgroundId))
                {
                    TryGetBackground(SelectedBackgroundId, out backgroundData);
                }

                return TryCreateStructuredToolChoiceState(
                    choiceGroup,
                    trimmedLabel,
                    CharacterCreationRuleService.Instance.GetToolChoiceSourceType(choiceGroup.ChoiceGroupId, raceData, backgroundData),
                    CharacterCreationRuleService.Instance.GetToolChoiceSourceId(choiceGroup.ChoiceGroupId, SelectedClassId, raceData, backgroundData));
            }

            return null;
        }

        private CharacterCreationToolChoiceState TryCreateStructuredToolChoiceState(DndChoiceGroupData choiceGroup, string label, string sourceType, string sourceId)
        {
            return CharacterCreationSessionService.Instance.CreateOrRefreshToolChoiceState(
                choiceGroup,
                label,
                sourceType,
                sourceId,
                CharacterCreationRuleService.Instance.ResolveToolIdFromChoiceOption);
        }

        private void EnsureClassCardCount(int count)
        {
            if (m_rectInfoListContent == null || m_goClassTemplate == null)
            {
                return;
            }

            while (m_classCards.Count < count)
            {
                GameObject itemObject = UnityEngine.Object.Instantiate(m_goClassTemplate, m_rectInfoListContent);
                itemObject.name = $"m_itemClass_{m_classCards.Count + 1}";
                itemObject.SetActive(true);
                m_classCards.Add(CharacterCreationClassCardView.Bind(itemObject));
            }
        }

        private void EnsureRaceCardCount(int count)
        {
            if (m_rectInfoListContent == null || m_goRaceTemplate == null)
            {
                return;
            }

            while (m_raceCards.Count < count)
            {
                GameObject itemObject = UnityEngine.Object.Instantiate(m_goRaceTemplate, m_rectInfoListContent);
                itemObject.name = $"m_itemRace_{m_raceCards.Count + 1}";
                itemObject.SetActive(true);
                m_raceCards.Add(CharacterCreationRaceCardView.Bind(itemObject));
            }
        }

        private void EnsureBackgroundCardCount(int count)
        {
            if (m_rectInfoListContent == null || m_goBackgroundTemplate == null)
            {
                return;
            }

            while (m_backgroundCards.Count < count)
            {
                GameObject itemObject = UnityEngine.Object.Instantiate(m_goBackgroundTemplate, m_rectInfoListContent);
                itemObject.name = $"m_itemBackground_{m_backgroundCards.Count + 1}";
                itemObject.SetActive(true);
                m_backgroundCards.Add(CharacterCreationBackgroundCardView.Bind(itemObject));
            }
        }

        private void EnsureAlignmentCardCount(int count)
        {
            if (m_rectInfoListContent == null || m_goAlignmentTemplate == null)
            {
                return;
            }

            while (m_alignmentCards.Count < count)
            {
                GameObject itemObject = UnityEngine.Object.Instantiate(m_goAlignmentTemplate, m_rectInfoListContent);
                itemObject.name = $"m_itemAlignment_{m_alignmentCards.Count + 1}";
                itemObject.SetActive(true);
                m_alignmentCards.Add(CharacterCreationAlignmentCardView.Bind(itemObject));
            }
        }

        private void EnsureLocalItemCardCount(int count)
        {
            if (m_rectInfoListContent == null)
            {
                return;
            }

            GameObject template = GetLocalItemOptionTemplate();
            if (template == null)
            {
                Log.Warning("CharacterCreationUI: m_itemItemInfoTemplate binding is missing.");
                return;
            }

            while (m_localItemCards.Count < count)
            {
                GameObject itemObject = UnityEngine.Object.Instantiate(template, m_rectInfoListContent);
                itemObject.name = $"m_itemLocalInventory_{m_localItemCards.Count + 1}";
                itemObject.SetActive(true);
                m_localItemCards.Add(CharacterCreationItemInfoCardView.Bind(itemObject));
            }
        }

        private GameObject GetLocalItemOptionTemplate()
        {
            return m_goItemInfoTemplate;
        }


        private void EnsureSelectionOptionCardCount(int count)
        {
            if (m_rectSelectionListContent == null)
            {
                return;
            }

            GameObject template = GetSelectionOptionTemplate();
            if (template == null)
            {
                return;
            }

            while (m_selectionOptionCards.Count < count)
            {
                GameObject itemObject = UnityEngine.Object.Instantiate(template, m_rectSelectionListContent);
                itemObject.name = $"m_itemSelectionOption_{m_selectionOptionCards.Count + 1}";
                itemObject.SetActive(true);
                m_selectionOptionCards.Add(CharacterCreationSelectionOptionCardView.Bind(itemObject));
            }
        }

        private GameObject GetSelectionOptionTemplate()
        {
            if (m_rectSelectionListContent == null)
            {
                return null;
            }

            if (m_selectionOptionCards.Count > 0)
            {
                return m_selectionOptionCards[0].Root;
            }

            for (int index = 0; index < m_rectSelectionListContent.childCount; index++)
            {
                Transform child = m_rectSelectionListContent.GetChild(index);
                if (child != null && !child.name.StartsWith("m_itemSelectionOption_", StringComparison.Ordinal))
                {
                    return child.gameObject;
                }
            }

            return null;
        }

        private void ClearSelectionList()
        {
            m_activeToolChoiceState = null;
            m_activeFeatureChoiceState = null;
            m_visibleFeatureChoiceState = null;
            m_pendingAbilityGenerationMethodId = string.Empty;
            m_pendingHitPointGenerationMethodId = string.Empty;
            m_pendingLocalItemId = string.Empty;
            ClearPendingSpellSelection();
            HideSelectionListOptions();
        }

        private void ClearPendingSpellSelection()
        {
            CharacterCreationSessionService.Instance.ClearPendingSpellSelection();
        }

        private void HideSelectionListOptions()
        {
            if (m_rectSelectionListContent == null)
            {
                return;
            }

            for (int index = 0; index < m_selectionOptionCards.Count; index++)
            {
                m_selectionOptionCards[index].SetActive(false);
            }

            for (int index = 0; index < m_rectSelectionListContent.childCount; index++)
            {
                Transform child = m_rectSelectionListContent.GetChild(index);
                if (child != null)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        private void OnClickClassCard(string classId)
        {
            if (string.IsNullOrWhiteSpace(classId))
            {
                return;
            }

            CharacterCreationSessionService.Instance.ToggleSelectedClass(classId);

            RemoveToolChoiceStatesBySource("Class");
            RemoveFeatureChoiceStatesBySource("Class");
            ClearSelectionList();
            RebuildSkillChoiceStates();
            RefreshCreationView();
            ShowClassOptions();
        }

        private void OnClickRaceCard(string raceId)
        {
            if (string.IsNullOrWhiteSpace(raceId))
            {
                return;
            }

            CharacterCreationSessionService.Instance.ToggleSelectedRace(raceId);

            RemoveToolChoiceStatesBySource("Race");
            RemoveFeatureChoiceStatesBySource("Race");
            SyncSelectedRaceRuleState();
            ClearSelectionList();
            RebuildSkillChoiceStates();
            RefreshCreationView();
            ShowRaceOptions();
        }

        private void OnClickBackgroundCard(string backgroundId)
        {
            if (string.IsNullOrWhiteSpace(backgroundId))
            {
                return;
            }

            CharacterCreationSessionService.Instance.ToggleSelectedBackground(backgroundId);

            RemoveToolChoiceStatesBySource("Background");
            ClearSelectionList();
            RebuildSkillChoiceStates();
            RefreshCreationView();
            ShowBackgroundOptions();
        }

        private void OnClickAlignmentCard(string alignmentId)
        {
            if (string.IsNullOrWhiteSpace(alignmentId))
            {
                return;
            }

            CharacterCreationSessionService.Instance.ToggleSelectedAlignment(alignmentId);

            RefreshCreationView();
            ShowAlignmentOptions();
        }

        private void ShowToolSelectionOptions(CharacterCreationToolChoiceState state)
        {
            HideInventoryActionButtons();
            if (state == null)
            {
                ClearSelectionList();
                return;
            }

            ClearPendingSpellSelection();
            m_activeFeatureChoiceState = null;
            m_visibleFeatureChoiceState = null;
            m_activeToolChoiceState = state;
            EnsureSelectionOptionCardCount(state.OptionToolIds.Count);

            for (int index = 0; index < m_selectionOptionCards.Count; index++)
            {
                CharacterCreationSelectionOptionCardView card = m_selectionOptionCards[index];
                bool active = index < state.OptionToolIds.Count;
                card.SetActive(active);
                if (!active)
                {
                    continue;
                }

                string toolId = state.OptionToolIds[index];
                string label = CharacterCreationEquipmentToolDisplayService.Instance.GetToolDisplayName(toolId);
                card.Bind(label, ContainsExactId(state.PendingToolIds, toolId), () => OnClickToolSelectionOption(toolId));
            }
        }

        private void ShowFeatureSelectionOptions(CharacterCreationFeatureChoiceState state)
        {
            ShowFeatureSelectionOptions(state, true);
        }

        private void ShowFeatureSelectionOptions(CharacterCreationFeatureChoiceState state, bool activateState)
        {
            HideInventoryActionButtons();
            if (state == null)
            {
                ClearSelectionList();
                return;
            }

            ClearPendingSpellSelection();
            if (activateState)
            {
                m_activeToolChoiceState = null;
                m_activeFeatureChoiceState = state;
            }

            m_visibleFeatureChoiceState = state;
            if (CharacterCreationSessionService.Instance.IsAbilityScoreFeatureChoice(state))
            {
                RefreshCreationView();
                HideRightPanelTemplates();
                HideSelectionListOptions();
                return;
            }

            if (state.PendingOptionIds.Count == 0 && state.SelectedOptionIds.Count > 0)
            {
                for (int index = 0; index < state.SelectedOptionIds.Count; index++)
                {
                    AppendUniqueExactValue(state.PendingOptionIds, state.SelectedOptionIds[index]);
                }
            }

            EnsureSelectionOptionCardCount(state.OptionIds.Count);

            for (int index = 0; index < m_selectionOptionCards.Count; index++)
            {
                CharacterCreationSelectionOptionCardView card = m_selectionOptionCards[index];
                bool active = index < state.OptionIds.Count;
                card.SetActive(active);
                if (!active)
                {
                    continue;
                }

                string optionId = state.OptionIds[index];
                int pendingCount = CountExactId(state.PendingOptionIds, optionId);
                string label = CharacterCreationFeatureDisplayService.Instance.GetChoiceOptionDisplayName(state.ChoiceGroupId, optionId);
                if (pendingCount > 1)
                {
                    label = $"{label} x{pendingCount}";
                }

                card.Bind(label, pendingCount > 0, () => OnClickFeatureSelectionOption(optionId));
            }
        }

        private void OnClickAbilityGenerationButton()
        {
            m_pendingAbilityGenerationMethodId = string.Empty;
            m_pendingHitPointGenerationMethodId = string.Empty;
            ShowAbilityGenerationMethodOptions();
        }

        private void ShowAbilityGenerationMethodOptions()
        {
            ClearPendingSpellSelection();
            m_activeToolChoiceState = null;
            m_activeFeatureChoiceState = null;
            m_visibleFeatureChoiceState = null;
            m_pendingHitPointGenerationMethodId = string.Empty;
            List<CharacterCreationAbilityGenerationMethodViewState> methods = CharacterCreationSessionService.Instance.GetAbilityGenerationMethods();
            EnsureSelectionOptionCardCount(methods.Count);

            for (int index = 0; index < m_selectionOptionCards.Count; index++)
            {
                CharacterCreationSelectionOptionCardView card = m_selectionOptionCards[index];
                bool active = index < methods.Count;
                card.SetActive(active);
                if (!active)
                {
                    continue;
                }

                CharacterCreationAbilityGenerationMethodViewState method = methods[index];
                card.Bind(method.Name, string.Equals(m_pendingAbilityGenerationMethodId, method.MethodId, StringComparison.OrdinalIgnoreCase), () => OnClickAbilityGenerationMethod(method.MethodId));
            }
        }

        private void OnClickHitPointGenerationButton()
        {
            m_pendingHitPointGenerationMethodId = string.Empty;
            ShowHitPointGenerationMethodOptions();
        }

        private void ShowHitPointGenerationMethodOptions()
        {
            ClearPendingSpellSelection();
            m_activeToolChoiceState = null;
            m_activeFeatureChoiceState = null;
            m_visibleFeatureChoiceState = null;
            m_pendingAbilityGenerationMethodId = string.Empty;
            List<CharacterCreationHitPointGenerationMethodViewState> methods = CharacterCreationSessionService.Instance.GetHitPointGenerationMethods();
            EnsureSelectionOptionCardCount(methods.Count);

            for (int index = 0; index < m_selectionOptionCards.Count; index++)
            {
                CharacterCreationSelectionOptionCardView card = m_selectionOptionCards[index];
                bool active = index < methods.Count;
                card.SetActive(active);
                if (!active)
                {
                    continue;
                }

                CharacterCreationHitPointGenerationMethodViewState method = methods[index];
                card.Bind(method.Name, string.Equals(m_pendingHitPointGenerationMethodId, method.MethodId, StringComparison.OrdinalIgnoreCase), () => OnClickHitPointGenerationMethod(method.MethodId));
            }
        }

        private void ShowGeneratedAbilityScoreOptions()
        {
            m_activeToolChoiceState = null;
            m_activeFeatureChoiceState = null;
            m_visibleFeatureChoiceState = null;
            m_pendingAbilityGenerationMethodId = string.Empty;
            List<CharacterCreationGeneratedAbilityScoreViewState> options = CharacterCreationSessionService.Instance.BuildGeneratedAbilityScoreOptions();
            EnsureSelectionOptionCardCount(options.Count);

            for (int index = 0; index < m_selectionOptionCards.Count; index++)
            {
                CharacterCreationSelectionOptionCardView card = m_selectionOptionCards[index];
                bool active = index < options.Count;
                card.SetActive(active);
                if (!active)
                {
                    continue;
                }

                CharacterCreationGeneratedAbilityScoreViewState option = options[index];
                string label = option.IsAssigned ? $"{option.Label} 已分配" : option.Label;
                card.Bind(label, option.IsSelected || option.IsAssigned, () => OnClickGeneratedAbilityScore(option.ScoreId));
            }
        }

        private void OnClickAbilityGenerationMethod(string methodId)
        {
            m_pendingHitPointGenerationMethodId = string.Empty;
            m_pendingAbilityGenerationMethodId = string.Equals(m_pendingAbilityGenerationMethodId, methodId, StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : methodId?.Trim() ?? string.Empty;
            ShowAbilityGenerationMethodOptions();
        }

        private void OnClickHitPointGenerationMethod(string methodId)
        {
            m_pendingAbilityGenerationMethodId = string.Empty;
            m_pendingHitPointGenerationMethodId = string.Equals(m_pendingHitPointGenerationMethodId, methodId, StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : methodId?.Trim() ?? string.Empty;
            ShowHitPointGenerationMethodOptions();
        }

        private void OnClickGeneratedAbilityScore(string scoreId)
        {
            if (CharacterCreationSessionService.Instance.SelectGeneratedAbilityScore(scoreId))
            {
                RefreshCreationView();
                ShowGeneratedAbilityScoreOptions();
            }
        }

        private void OnClickAbilityItem(string abilityId)
        {
            if (CharacterCreationSessionService.Instance.ChangeAbilityScore(abilityId, 1))
            {
                RefreshCreationView();
                if (m_activeFeatureChoiceState != null && !CharacterCreationSessionService.Instance.IsActiveAbilityScoreFeatureChoice())
                {
                    ShowFeatureSelectionOptions(m_activeFeatureChoiceState);
                }

                return;
            }

            if (CharacterCreationSessionService.Instance.AssignPendingGeneratedAbilityScore(abilityId))
            {
                RefreshCreationView();
                ShowGeneratedAbilityScoreOptions();
            }
        }

        private void OnAbilityScoreInputEndEdit(string abilityId, string value)
        {
            if (!int.TryParse(KeepDigitsOnly(value), out int score))
            {
                RefreshCreationView();
                return;
            }

            if (CharacterCreationSessionService.Instance.SetManualAbilityScore(abilityId, score))
            {
                RefreshCreationView();
            }
        }

        private void OnClickToolSelectionOption(string toolId)
        {
            CharacterCreationToolChoiceState state = m_activeToolChoiceState;
            if (state == null || string.IsNullOrWhiteSpace(toolId))
            {
                return;
            }

            CharacterCreationSessionService.Instance.TogglePendingToolChoice(toolId);
            if (state is CharacterCreationMixedToolChoiceState)
            {
                state = CharacterCreationSessionService.Instance.CreateOrRefreshMixedToolChoiceState(state.ChoiceGroupId, state.Label) ?? state;
                m_activeToolChoiceState = state;
            }

            ShowToolSelectionOptions(state);
        }

        private void OnClickFeatureSelectionOption(string optionId)
        {
            CharacterCreationFeatureChoiceState state = m_visibleFeatureChoiceState ?? m_activeFeatureChoiceState;
            if (state == null || string.IsNullOrWhiteSpace(optionId))
            {
                return;
            }

            ShowFeatureChoiceOptionDetail(state.ChoiceGroupId, optionId);

            if (CharacterCreationSessionService.Instance.IsFeatFeatureChoice(state))
            {
                CharacterCreationSessionService.Instance.SelectFeatFeatureChoice(state, optionId);
                CharacterCreationSessionService.Instance.StartFollowupFeatureChoice(state);
                RebuildSkillChoiceStates();
            }
            else
            {
                CharacterCreationSessionService.Instance.TogglePendingFeatureChoice(optionId);
            }

            RefreshCreationView();
            ShowFeatureSelectionOptions(state, !CharacterCreationSessionService.Instance.IsFeatFeatureChoice(state));
        }

        private void OnClickLocalItemOption(string customItemId)
        {
            if (string.IsNullOrWhiteSpace(customItemId))
            {
                return;
            }

            m_pendingLocalItemId = string.Equals(m_pendingLocalItemId, customItemId, StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : customItemId.Trim();

            LocalCustomItemSaveData item = FindLocalItemOption(customItemId);
            if (item != null)
            {
                ShowLocalItemDetail(item);
            }
            else
            {
                HideInventoryActionButtons();
                SetText(m_tmpFeatureDetailTitle, "本地物品");
                SetText(m_tmpFeatureDetailDescription, "未找到该物品。");
            }

            RefreshLocalItemOptionCards();
        }

        private bool ConfirmPendingLocalItemSelection()
        {
            LocalCustomItemSaveData item = FindLocalItemOption(m_pendingLocalItemId);
            if (item == null)
            {
                Log.Warning("本地物品不存在，无法加入角色背包。");
                return false;
            }

            CharacterEquipmentItemSaveData snapshot = LocalCustomItemRepository.CreateCharacterItemSnapshot(item, Math.Max(1, item.Item?.Quantity ?? 1));
            CharacterInventoryOperationResult result = CharacterCreationSessionService.Instance.AddInventoryItem(snapshot, snapshot.Quantity);
            if (!result.Success)
            {
                Log.Warning(string.IsNullOrWhiteSpace(result.Message) ? "物品加入角色背包失败。" : result.Message);
                return false;
            }

            string itemName = FirstNonEmpty(snapshot.ItemName, snapshot.ItemId);
            HideInventoryActionButtons();
            SetText(m_tmpFeatureDetailTitle, itemName);
            SetText(m_tmpFeatureDetailDescription, $"{itemName} 已加入角色背包。");
            m_pendingLocalItemId = string.Empty;
            return true;
        }

        private void OnDeleteLocalItemOption(string customItemId)
        {
            if (string.IsNullOrWhiteSpace(customItemId))
            {
                return;
            }

            LocalCustomItemSaveData item = FindLocalItemOption(customItemId);
            string itemName = BuildLocalItemOptionLabel(item);
            LocalCustomItemRepository.Delete(customItemId);

            if (string.Equals(m_pendingLocalItemId, customItemId, StringComparison.OrdinalIgnoreCase))
            {
                m_pendingLocalItemId = string.Empty;
            }

            m_localItemOptions = LoadLocalItemOptions();
            RefreshLocalItemOptionCards();

            HideInventoryActionButtons();
            SetText(m_tmpFeatureDetailTitle, "本地物品库");
            if (m_localItemOptions.Count == 0)
            {
                SetText(m_tmpFeatureDetailDescription, "当前没有已保存的本地物品。请先在物品编辑界面保存物品。");
            }
            else
            {
                SetText(m_tmpFeatureDetailDescription, $"{itemName} 已从本地物品库删除。");
            }
        }

        private void ConfirmCurrentRightPanelSelection()
        {
            if (!string.IsNullOrWhiteSpace(m_pendingHitPointGenerationMethodId))
            {
                int level = ParseLevel(m_tmpInputLevel != null ? m_tmpInputLevel.text : string.Empty);
                if (!CharacterCreationSessionService.Instance.GenerateHitPoints(m_pendingHitPointGenerationMethodId, level))
                {
                    return;
                }

                ClearSelectionList();
                HideRightPanelTemplates();
                RefreshCreationView();
                return;
            }

            if (!string.IsNullOrWhiteSpace(m_pendingAbilityGenerationMethodId))
            {
                if (!CharacterCreationSessionService.Instance.StartAbilityGeneration(m_pendingAbilityGenerationMethodId))
                {
                    return;
                }

                RefreshCreationView();
                SetActive(m_btnAbilityGeneration != null ? m_btnAbilityGeneration.gameObject : null, false);
                if (CharacterCreationSessionService.Instance.BuildGeneratedAbilityScoreOptions().Count > 0)
                {
                    ShowGeneratedAbilityScoreOptions();
                }
                else
                {
                    ClearSelectionList();
                    HideRightPanelTemplates();
                }

                return;
            }

            if (!string.IsNullOrWhiteSpace(CharacterCreationSessionService.Instance.SpellSelectionState.PendingSpellId))
            {
                if (!CharacterCreationSessionService.Instance.ConfirmPendingSpellSelection())
                {
                    return;
                }

                RefreshCreationView();
                ShowSpellOptions(m_activeSpellFilterLevel);
                ClearSelectionList();
                return;
            }

            if (!string.IsNullOrWhiteSpace(m_pendingLocalItemId))
            {
                if (!ConfirmPendingLocalItemSelection())
                {
                    return;
                }

                RefreshCreationView();
                HideRightPanelTemplates();
                m_localItemOptions = LoadLocalItemOptions();
                RefreshLocalItemOptionCards();
                return;
            }

            if (m_visibleFeatureChoiceState != null
                && !CharacterCreationFeatureDisplayService.Instance.IsFeatureChoiceCompleted(m_visibleFeatureChoiceState))
            {
                CharacterCreationFeatureChoiceState visibleState = m_visibleFeatureChoiceState;
                if (!ConfirmFeatureSelection(visibleState))
                {
                    return;
                }

                CharacterCreationFeatureChoiceState followupState = CharacterCreationSessionService.Instance.GetActiveFeatureChoiceState();
                if (followupState != null
                    && !ReferenceEquals(followupState, visibleState)
                    && IsFeatureChoiceReadyToConfirm(followupState))
                {
                    ConfirmFeatureSelection(followupState);
                    followupState = CharacterCreationSessionService.Instance.GetActiveFeatureChoiceState();
                }

                if (followupState != null
                    && !ReferenceEquals(followupState, visibleState)
                    && CharacterCreationSessionService.Instance.IsAbilityScoreFeatureChoice(followupState)
                    && !CharacterCreationFeatureDisplayService.Instance.IsFeatureChoiceCompleted(followupState))
                {
                    RefreshCreationView();
                    HideRightPanelTemplates();
                    HideSelectionListOptions();
                }
                else if (followupState != null
                    && !ReferenceEquals(followupState, visibleState)
                    && !CharacterCreationFeatureDisplayService.Instance.IsFeatureChoiceCompleted(followupState))
                {
                    RefreshCreationView();
                    ShowFeatureSelectionOptions(followupState);
                }
                else
                {
                    RefreshCreationView();
                    HideRightPanelTemplates();
                    ClearSelectionList();
                }

                return;
            }

            if (m_activeFeatureChoiceState != null)
            {
                if (!ConfirmFeatureSelection(m_activeFeatureChoiceState))
                {
                    return;
                }

                CharacterCreationFeatureChoiceState followupState = CharacterCreationSessionService.Instance.GetActiveFeatureChoiceState();
                if (followupState != null && CharacterCreationSessionService.Instance.IsActiveAbilityScoreFeatureChoice())
                {
                    RefreshCreationView();
                    HideRightPanelTemplates();
                    HideSelectionListOptions();
                }
                else if (followupState != null && CharacterCreationSessionService.Instance.IsAbilityScoreFeatureChoice(followupState))
                {
                    RefreshCreationView();
                    HideRightPanelTemplates();
                    HideSelectionListOptions();
                }
                else if (followupState != null && !CharacterCreationFeatureDisplayService.Instance.IsFeatureChoiceCompleted(followupState))
                {
                    RefreshCreationView();
                    ShowFeatureSelectionOptions(followupState);
                }
                else
                {
                    RefreshCreationView();
                    HideRightPanelTemplates();
                    ClearSelectionList();
                }

                return;
            }

            if (m_visibleFeatureChoiceState != null)
            {
                if (!ConfirmFeatureSelection(m_visibleFeatureChoiceState))
                {
                    return;
                }

                RefreshCreationView();
                HideRightPanelTemplates();
                ClearSelectionList();
                return;
            }

            if (CharacterCreationSessionService.Instance.IsAbilityGenerationAssignmentComplete()
                && CharacterCreationSessionService.Instance.BuildGeneratedAbilityScoreOptions().Count > 0)
            {
                ClearSelectionList();
                HideRightPanelTemplates();
                RefreshCreationView();
                return;
            }

            if (m_activeToolChoiceState == null)
            {
                return;
            }

            CharacterCreationToolChoiceState state = m_activeToolChoiceState;
            CharacterCreationSessionService.Instance.ConfirmActiveToolChoice();

            RefreshCreationView();
            HideRightPanelTemplates();
            ClearSelectionList();
        }

        private bool ConfirmFeatureSelection(CharacterCreationFeatureChoiceState state)
        {
            if (state == null)
            {
                return false;
            }

            bool confirmed = CharacterCreationSessionService.Instance.ConfirmFeatureChoice(state);
            if (!confirmed)
            {
                return false;
            }

            CharacterCreationSessionService.Instance.StartFollowupFeatureChoice(state);
            RebuildSkillChoiceStates();
            return true;
        }

        private static bool IsFeatureChoiceReadyToConfirm(CharacterCreationFeatureChoiceState state)
        {
            if (state == null || state.PendingOptionIds.Count == 0)
            {
                return false;
            }

            int requiredCount = state.MinSelect > 0 ? state.MinSelect : Math.Max(1, state.MaxSelect);
            return state.MaxSelect <= 0
                ? state.PendingOptionIds.Count > 0
                : state.PendingOptionIds.Count >= requiredCount;
        }

        private void RefreshCreationView()
        {
            int level = ParseLevel(m_tmpInputLevel != null ? m_tmpInputLevel.text : string.Empty);
            CharacterCreationViewState state = CharacterCreationViewStateService.Instance.BuildCurrentViewState(level);
            ApplyCreationViewState(state);
        }

        private void ApplyCreationViewState(CharacterCreationViewState state)
        {
            if (state == null)
            {
                return;
            }

            SetText(m_tmpDetailClass, state.ClassSummary);
            SetText(m_tmpDetailRace, state.RaceSummary);
            SetText(m_tmpDetailBackground, state.BackgroundSummary);
            SetText(m_tmpDetailAlignment, state.AlignmentSummary);
            SetText(m_tmpHitDiceDie, state.HitDiceCountText);
            SetText(m_tmpHitDiceRemaining, state.HitDiceDieText);
            SetText(m_tmpCurrentHp, state.CurrentHpText);
            SetText(m_tmpMaxHp, state.MaxHpText);
            SetText(m_tmpTempHp, state.TemporaryHpText);
            SetActive(m_btnGenerateHitPoints != null ? m_btnGenerateHitPoints.gameObject : null, state.ShouldShowHitPointGenerationButton);
            SetText(m_tmpCopper, state.CopperText);
            SetText(m_tmpSilver, state.SilverText);
            SetText(m_tmpElectrum, state.ElectrumText);
            SetText(m_tmpGold, state.GoldText);
            SetText(m_tmpPlatinum, state.PlatinumText);
            ApplyExperienceViewState(state.Experience);
            SetText(m_tmpSpeed, state.SpeedText);
            SetText(m_tmpAc, state.ArmorClassText);
            SetText(m_tmpInitiative, state.InitiativeText);
            SetText(m_tmpProficiencyBonus, state.ProficiencyBonusText);
            SetText(m_tmpPassivePerception, state.PassivePerceptionText);
            SetText(m_tmpDc, state.SpellSaveDcText);
            SetText(m_tmpSpellAttackBonus, state.SpellAttackBonusText);
            SetText(m_tmpSkillsLabel, state.SkillsSummary);
            SetText(m_tmpEquipmentToolsLabel, state.EquipmentToolsSummary);
            SetText(m_tmpClassFeatureClassName, state.ClassFeatureClassName);
            SetText(m_tmpClassFeatureSubclassName, state.ClassFeatureSubclassName);
            SetText(m_tmpClassLevel, state.ClassLevelText);
            SetText(m_tmpRaceFeatureRaceName, state.RaceFeatureRaceName);
            SetText(m_tmpRaceFeatureSubRaceName, state.RaceFeatureSubRaceName);

            ApplyAbilityViewStates(state.Abilities);
            ApplySkillViewStates(state.Skills);
            ApplyEquipmentToolViewState(state.EquipmentTools);
            RefreshInventoryItems(state.InventoryItems);
            RefreshClassFeatureItems(state.ClassFeatures);
            RefreshRaceFeatureItems(state.RaceFeatures);
            RefreshOtherFeatureItems(state.OtherFeatures);
            RefreshLearnedSpellItems(state.LearnedSpells);
            RefreshStatusEffectItems(state.StatusEffects);
        }

        private void ApplyExperienceViewState(CharacterExperienceDisplayViewState state)
        {
            if (m_sliderExperience != null)
            {
                m_sliderExperience.minValue = 0f;
                m_sliderExperience.maxValue = 1f;
                m_sliderExperience.value = Mathf.Clamp01(state?.Progress ?? 0f);
            }

            SetText(m_tmpExperienceValue, state?.Label ?? string.Empty);
        }

        private void ApplyAbilityViewStates(IReadOnlyList<CharacterCreationAbilityViewState> abilities)
        {
            ApplyAbilityViewState(abilities, "Strength", m_tmpStrength, m_tmpStrengthModifier, m_btnStrengthIncrease, m_btnStrengthDecrease, m_inputStrength);
            ApplyAbilityViewState(abilities, "Dexterity", m_tmpDexterity, m_tmpDexterityModifier, m_btnDexterityIncrease, m_btnDexterityDecrease, m_inputDexterity);
            ApplyAbilityViewState(abilities, "Constitution", m_tmpConstitution, m_tmpConstitutionModifier, m_btnConstitutionIncrease, m_btnConstitutionDecrease, m_inputConstitution);
            ApplyAbilityViewState(abilities, "Intelligence", m_tmpIntelligence, m_tmpIntelligenceModifier, m_btnIntelligenceIncrease, m_btnIntelligenceDecrease, m_inputIntelligence);
            ApplyAbilityViewState(abilities, "Wisdom", m_tmpWisdom, m_tmpWisdomModifier, m_btnWisdomIncrease, m_btnWisdomDecrease, m_inputWisdom);
            ApplyAbilityViewState(abilities, "Charisma", m_tmpCharisma, m_tmpCharismaModifier, m_btnCharismaIncrease, m_btnCharismaDecrease, m_inputCharisma);
        }

        private void ApplyAbilityViewState(
            IReadOnlyList<CharacterCreationAbilityViewState> abilities,
            string abilityId,
            TMP_Text scoreText,
            TMP_Text modifierText,
            Button increaseButton,
            Button decreaseButton,
            TMP_InputField scoreInput)
        {
            CharacterCreationAbilityViewState state = FindAbilityViewState(abilities, abilityId);
            if (state == null)
            {
                return;
            }

            string scoreValue = state.Score.ToString();
            if (scoreInput != null)
            {
                scoreInput.SetTextWithoutNotify(scoreValue);
                scoreInput.interactable = state.CanManualInput;
            }
            else
            {
                SetText(scoreText, scoreValue);
            }

            SetText(modifierText, state.ModifierText);
            SetActive(increaseButton != null ? increaseButton.gameObject : null, state.CanIncrease);
            SetActive(decreaseButton != null ? decreaseButton.gameObject : null, state.CanDecrease);
        }

        private static CharacterCreationAbilityViewState FindAbilityViewState(IReadOnlyList<CharacterCreationAbilityViewState> abilities, string abilityId)
        {
            if (abilities == null || string.IsNullOrWhiteSpace(abilityId))
            {
                return null;
            }

            for (int index = 0; index < abilities.Count; index++)
            {
                CharacterCreationAbilityViewState state = abilities[index];
                if (state != null && string.Equals(state.AbilityId, abilityId, StringComparison.OrdinalIgnoreCase))
                {
                    return state;
                }
            }

            return null;
        }

        private void ApplySkillViewStates(IReadOnlyList<CharacterCreationSkillViewState> skills)
        {
            for (int index = 0; index < SkillDisplayBindings.Length; index++)
            {
                CharacterCreationSkillViewState state = index < (skills?.Count ?? 0) ? skills[index] : null;
                if (state == null)
                {
                    continue;
                }

                SetText(m_tmpSkillBonuses[index], state.BonusText);
                SetSkillBackground(index, state.HasProficiency, state.IsChoiceCandidate);
            }
        }

        private void ApplyEquipmentToolViewState(CharacterCreationEquipmentToolDisplayState state)
        {
            m_toolChoiceGroupIdByLabel.Clear();
            m_toolChoiceSourceTypeByLabel.Clear();
            m_toolChoiceSourceIdByLabel.Clear();
            if (state != null)
            {
                foreach (KeyValuePair<string, string> pair in state.ChoiceGroupIdByLabel)
                {
                    m_toolChoiceGroupIdByLabel[pair.Key] = pair.Value;
                }

                foreach (KeyValuePair<string, string> pair in state.ChoiceSourceTypeByLabel)
                {
                    m_toolChoiceSourceTypeByLabel[pair.Key] = pair.Value;
                }

                foreach (KeyValuePair<string, string> pair in state.ChoiceSourceIdByLabel)
                {
                    m_toolChoiceSourceIdByLabel[pair.Key] = pair.Value;
                }
            }

            IReadOnlyList<string> labels = state != null ? state.Labels : Array.Empty<string>();
            RefreshEquipmentToolItems(labels);
        }

        private void RefreshSkillPanel()
        {
            SetActive(m_rectSkillProficiencies != null ? m_rectSkillProficiencies.gameObject : null, true);
            CharacterCreationViewState state = CharacterCreationViewStateService.Instance.BuildCurrentViewState(ParseLevel(m_tmpInputLevel != null ? m_tmpInputLevel.text : string.Empty));
            ApplySkillViewStates(state.Skills);
        }

        private List<string> BuildCurrentSkillProficiencyIds()
        {
            return CharacterCreationSessionService.Instance.BuildCurrentSkillProficiencyIds(BuildFixedSkillProficiencyIds());
        }

        private void RebuildSkillChoiceStates()
        {
            DndClassDefineData classData = null;
            DndRaceDefineData raceData = null;
            DndBackgroundDefineData backgroundData = null;
            TryGetSelectedClass(out classData);
            TryGetSelectedRace(out raceData);
            if (!string.IsNullOrWhiteSpace(SelectedBackgroundId))
            {
                TryGetBackground(SelectedBackgroundId, out backgroundData);
            }

            CharacterCreationSessionService.Instance.RebuildSkillChoiceStatesForCurrentSelection(classData, raceData, backgroundData);
        }

        private void SyncSelectedRaceRuleState()
        {
            if (TryGetSelectedRace(out DndRaceDefineData raceData))
            {
                RebuildRaceAbilityBonuses(raceData);
                RebuildFeatureChoiceStates();
            }
            else
            {
                CharacterCreationSessionService.Instance.ClearRaceAbilityChoiceState();
                RebuildFeatureChoiceStates();
            }
        }

        private List<string> BuildFixedSkillProficiencyIds()
        {
            DndRaceDefineData raceData = null;
            DndBackgroundDefineData backgroundData = null;
            TryGetSelectedRace(out raceData);
            if (!string.IsNullOrWhiteSpace(SelectedBackgroundId))
            {
                TryGetBackground(SelectedBackgroundId, out backgroundData);
            }

            return CharacterCreationRuleService.Instance.BuildFixedSkillProficiencyIds(raceData, backgroundData);
        }

        private static bool TryGetBackground(string backgroundId, out DndBackgroundDefineData backgroundData)
        {
            return CharacterCreationRuleService.Instance.TryGetBackground(backgroundId, out backgroundData);
        }

        private void SetSkillBackground(int index, bool hasProficiency, bool isChoiceCandidate)
        {
            if (index < 0 || index >= m_imgSkillBackgrounds.Length)
            {
                return;
            }

            Image image = m_imgSkillBackgrounds[index];
            if (image != null)
            {
                image.color = isChoiceCandidate ? SkillChoiceCandidateBackgroundColor : m_defaultSkillBackgroundColors[index];
            }

            CanvasGroup canvasGroup = m_canvasSkillItems[index];
            if (canvasGroup != null)
            {
                canvasGroup.alpha = hasProficiency ? SkillProficientAlpha : SkillNotProficientAlpha;
            }
        }

        private void OnClickSkillItem(int index)
        {
            if (index < 0 || index >= SkillDisplayBindings.Length)
            {
                return;
            }

            string skillId = SkillDisplayBindings[index].SkillId;
            if (CharacterCreationSessionService.Instance.TrySelectSkill(skillId, BuildCurrentSkillProficiencyIds()))
            {
                RefreshCreationView();
            }
        }

        private bool IsSkillChoiceCandidate(string skillId)
        {
            return CharacterCreationSessionService.Instance.IsSkillChoiceCandidate(skillId, BuildCurrentSkillProficiencyIds());
        }

        private void RemoveToolChoiceStatesBySource(string sourceType)
        {
            CharacterCreationSessionService.Instance.RemoveToolChoiceStatesBySource(sourceType);
        }

        private void RefreshEquipmentToolItems(IReadOnlyList<string> labels)
        {
            RefreshLabelItems(m_equipmentToolItems, m_rectEquipmentToolContent, m_goEquipmentToolTemplate, labels, "m_itemEquipmentTool_", "m_tmpEquipmentToolLabel");
            BindEquipmentToolItemChoices(labels);
        }

        private void BindEquipmentToolItemChoices(IReadOnlyList<string> labels)
        {
            if (labels == null)
            {
                return;
            }

            for (int index = 0; index < m_equipmentToolItems.Count; index++)
            {
                CharacterCreationLabelItemView item = m_equipmentToolItems[index];
                if (item == null)
                {
                    continue;
                }

                if (index >= labels.Count)
                {
                    item.BindClick(null, false);
                    item.SetAlpha(SkillProficientAlpha);
                    continue;
                }

                string label = labels[index];
                CharacterCreationToolChoiceState state = TryCreateToolChoiceState(label);
                if (state == null)
                {
                    item.BindClick(null, false);
                    item.SetAlpha(SkillProficientAlpha);
                }
                else
                {
                    item.BindClick(() => ShowToolSelectionOptions(state), true);
                    item.SetAlpha(SkillNotProficientAlpha);
                }
            }
        }

        private void RefreshInventoryItems(IReadOnlyList<CharacterInventoryDisplayEntry> entries)
        {
            if (m_rectInventoryContent == null || m_goEquipmentToolTemplate == null)
            {
                return;
            }

            int count = entries?.Count ?? 0;
            while (m_inventoryItems.Count < count)
            {
                GameObject itemObject = UnityEngine.Object.Instantiate(m_goEquipmentToolTemplate, m_rectInventoryContent);
                itemObject.name = $"m_itemInventory_{m_inventoryItems.Count + 1}";
                itemObject.SetActive(true);
                m_inventoryItems.Add(CharacterCreationLabelItemView.Bind(itemObject, "m_tmpEquipmentToolLabel"));
            }

            if (m_goEquipmentToolTemplate.activeSelf && m_goEquipmentToolTemplate.transform.IsChildOf(m_rectInventoryContent))
            {
                m_goEquipmentToolTemplate.SetActive(false);
            }

            for (int index = 0; index < m_inventoryItems.Count; index++)
            {
                CharacterCreationLabelItemView itemView = m_inventoryItems[index];
                bool active = index < count;
                itemView.SetActive(active);
                if (!active)
                {
                    itemView.BindClick(null, false);
                    itemView.SetAlpha(SkillProficientAlpha);
                    continue;
                }

                CharacterInventoryDisplayEntry entry = entries[index];
                itemView.SetLabel(entry.Label);
                itemView.SetAlpha(SkillProficientAlpha);
                itemView.BindClick(() => ShowInventoryItemDetail(entry), true);
            }
        }

        private void ShowInventoryItemDetail(CharacterInventoryDisplayEntry entry)
        {
            SetText(m_tmpFeatureDetailTitle, entry.Title);
            SetText(m_tmpFeatureDetailDescription, entry.Description);
            m_visibleInventoryItemInstanceId = entry.ItemInstanceId;
            RefreshInventoryActionButtons(entry.ItemInstanceId);
        }

        private void RefreshInventoryActionButtons(string itemInstanceId)
        {
            CharacterEquipmentItemSaveData item = FindDraftInventoryItem(itemInstanceId);
            if (item == null)
            {
                HideInventoryActionButtons();
                return;
            }

            SetActive(m_rectInventoryActionButtons != null ? m_rectInventoryActionButtons.gameObject : null, true);
            m_visibleInventoryQuickRollContext = BuildInventoryQuickRollContext(item);
            m_visibleInventoryQuickRollResult = null;
            m_visibleInventoryQuickRollPurpose = CharacterInventoryQuickRollPurpose.DisplayOnly;
            m_visibleInventoryQuickRollHistoryEntryId = string.Empty;
            SetActive(m_rectInventoryQuickRollPurposeButtons != null ? m_rectInventoryQuickRollPurposeButtons.gameObject : null, false);
            if (m_tmpInputInventoryUseAmount != null)
            {
                m_tmpInputInventoryUseAmount.text = "1";
            }

            SetInventoryActionButton(m_btnInventoryEquipAction, m_tmpInventoryEquipActionLabel, "装备/卸下");
            SetInventoryActionButton(m_btnInventoryAttuneAction, m_tmpInventoryAttuneActionLabel, "同调/解除同调");
            SetInventoryActionButton(m_btnInventoryUseAction, m_tmpInventoryUseActionLabel, "使用");
            SetInventoryActionButton(m_btnInventoryQuickRollAction, m_tmpInventoryQuickRollActionLabel, "快捷掷骰");
        }

        private void HideInventoryActionButtons()
        {
            m_visibleInventoryItemInstanceId = string.Empty;
            m_visibleInventoryQuickRollContext = null;
            m_visibleInventoryQuickRollResult = null;
            m_visibleInventoryQuickRollPurpose = CharacterInventoryQuickRollPurpose.DisplayOnly;
            m_visibleInventoryQuickRollHistoryEntryId = string.Empty;
            SetActive(m_rectInventoryActionButtons != null ? m_rectInventoryActionButtons.gameObject : null, false);
            SetActive(m_rectInventoryQuickRollPurposeButtons != null ? m_rectInventoryQuickRollPurposeButtons.gameObject : null, false);
        }

        private int GetInventoryUseAmount()
        {
            if (m_tmpInputInventoryUseAmount == null)
            {
                return 1;
            }

            return int.TryParse(m_tmpInputInventoryUseAmount.text, out int amount) && amount > 0
                ? amount
                : 1;
        }

        private static void SetInventoryActionButton(Button button, TMP_Text label, string text)
        {
            SetActive(button != null ? button.gameObject : null, true);
            SetText(label, text);
        }

        private static CharacterInventoryQuickRollContext BuildInventoryQuickRollContext(CharacterEquipmentItemSaveData item)
        {
            if (item?.CustomEffects == null)
            {
                return null;
            }

            for (int index = 0; index < item.CustomEffects.Count; index++)
            {
                CharacterItemEffectSaveData effect = item.CustomEffects[index];
                string diceExpression = effect?.DiceExpression?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(diceExpression))
                {
                    continue;
                }

                return new CharacterInventoryQuickRollContext
                {
                    ItemInstanceId = item.ItemInstanceId ?? string.Empty,
                    ItemName = FirstNonEmpty(item.ItemName, item.ItemId),
                    EffectName = FirstNonEmpty(effect.Name, effect.Description),
                    DiceExpression = diceExpression
                };
            }

            return null;
        }

        private CharacterEquipmentItemSaveData FindDraftInventoryItem(string itemInstanceId)
        {
            CharacterEquipmentSetSaveData equipment = CharacterCreationSessionService.Instance.CurrentState?.Character?.Equipment;
            return FindInventoryItem(equipment?.Armor, itemInstanceId)
                ?? FindInventoryItem(equipment?.Shield, itemInstanceId)
                ?? FindInventoryItem(equipment?.EquippedItems, itemInstanceId)
                ?? FindInventoryItem(equipment?.InventoryItems, itemInstanceId);
        }

        private static CharacterEquipmentItemSaveData FindInventoryItem(CharacterEquipmentItemSaveData item, string itemInstanceId)
        {
            return item != null
                && string.Equals(item.ItemInstanceId, itemInstanceId?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                    ? item
                    : null;
        }

        private static CharacterEquipmentItemSaveData FindInventoryItem(IReadOnlyList<CharacterEquipmentItemSaveData> items, string itemInstanceId)
        {
            if (items == null || string.IsNullOrWhiteSpace(itemInstanceId))
            {
                return null;
            }

            for (int index = 0; index < items.Count; index++)
            {
                CharacterEquipmentItemSaveData item = FindInventoryItem(items[index], itemInstanceId);
                if (item != null)
                {
                    return item;
                }
            }

            return null;
        }

        private static bool IsInventoryItemEquippable(CharacterEquipmentItemSaveData item)
        {
            if (item == null)
            {
                return false;
            }

            if (item.IsEquipped)
            {
                return true;
            }

            if (item.Consumable && item.Charges <= 0)
            {
                return false;
            }

            string itemType = item.ItemType?.Trim() ?? string.Empty;
            string condition = item.EffectApplyCondition?.Trim() ?? string.Empty;
            return item.IsEquipped
                || string.Equals(itemType, "armor", StringComparison.OrdinalIgnoreCase)
                || string.Equals(itemType, "shield", StringComparison.OrdinalIgnoreCase)
                || string.Equals(itemType, "weapon", StringComparison.OrdinalIgnoreCase)
                || string.Equals(itemType, "tool", StringComparison.OrdinalIgnoreCase)
                || string.Equals(condition, "Equipped", StringComparison.OrdinalIgnoreCase)
                || string.Equals(condition, "Worn", StringComparison.OrdinalIgnoreCase)
                || string.Equals(condition, "Wielded", StringComparison.OrdinalIgnoreCase)
                || string.Equals(condition, "EquippedAndAttuned", StringComparison.OrdinalIgnoreCase)
                || !string.Equals(CharacterArmorCategoryIds.Normalize(item.ArmorCategory), CharacterArmorCategoryIds.None, StringComparison.OrdinalIgnoreCase)
                || !string.IsNullOrWhiteSpace(item.WeaponCategory)
                || !string.IsNullOrWhiteSpace(item.ToolCategory)
                || item.ArmorBaseAc > 0
                || item.AcBonus != 0;
        }

        private static bool IsInventoryItemUsable(CharacterEquipmentItemSaveData item)
        {
            if (item == null)
            {
                return false;
            }

            return item.Charges > 0
                || (item.Quantity > 0 && (item.Consumable || item.ConsumeOnUse));
        }

        private partial void OnClickInventoryEquipActionBtn()
        {
            CharacterEquipmentItemSaveData item = FindDraftInventoryItem(m_visibleInventoryItemInstanceId);
            if (item == null)
            {
                HideInventoryActionButtons();
                return;
            }

            if (!IsInventoryItemEquippable(item))
            {
                return;
            }

            CharacterInventoryOperationResult result = item.IsEquipped
                ? CharacterCreationSessionService.Instance.UnequipInventoryItem(item.ItemInstanceId)
                : CharacterCreationSessionService.Instance.EquipInventoryItem(item.ItemInstanceId);
            RefreshAfterInventoryAction(result, item.IsEquipped ? "物品已卸下。" : "物品已装备。");
        }

        private partial void OnClickInventoryAttuneActionBtn()
        {
            CharacterEquipmentItemSaveData item = FindDraftInventoryItem(m_visibleInventoryItemInstanceId);
            if (item == null)
            {
                HideInventoryActionButtons();
                return;
            }

            if (!item.RequiresAttunement)
            {
                return;
            }

            CharacterInventoryOperationResult result = item.IsAttuned
                ? CharacterCreationSessionService.Instance.UnattuneInventoryItem(item.ItemInstanceId)
                : CharacterCreationSessionService.Instance.AttuneInventoryItem(item.ItemInstanceId);
            RefreshAfterInventoryAction(result, item.IsAttuned ? "物品已解除同调。" : "物品已同调。");
        }

        private partial void OnClickInventoryUseActionBtn()
        {
            CharacterEquipmentItemSaveData item = FindDraftInventoryItem(m_visibleInventoryItemInstanceId);
            if (item == null)
            {
                HideInventoryActionButtons();
                return;
            }

            if (!IsInventoryItemUsable(item))
            {
                return;
            }

            CharacterInventoryOperationResult result = CharacterCreationSessionService.Instance.UseInventoryItem(
                m_visibleInventoryItemInstanceId,
                GetInventoryUseAmount());
            RefreshAfterInventoryAction(result, "物品已使用。");
        }

        private void OnClickInventoryQuickRollActionBtn()
        {
            CharacterEquipmentItemSaveData item = FindDraftInventoryItem(m_visibleInventoryItemInstanceId);
            CharacterInventoryQuickRollContext context = BuildInventoryQuickRollContext(item)
                ?? BuildManualInventoryRollContext(item);

            Log.Info($"打开 DiceRollUI：{context.ItemName} / {context.EffectName} / {context.DiceExpression}");
            m_visibleInventoryQuickRollContext = context;
            m_visibleInventoryQuickRollPurpose = CharacterInventoryQuickRollPurpose.DisplayOnly;
            m_visibleInventoryQuickRollResult = null;
            m_visibleInventoryQuickRollHistoryEntryId = string.Empty;
            SetActive(m_rectInventoryQuickRollPurposeButtons != null ? m_rectInventoryQuickRollPurposeButtons.gameObject : null, false);
            RefreshQuickRollApplyResultButtonState();
            SetText(m_tmpFeatureDetailTitle, "快捷掷骰");
            SetText(m_tmpFeatureDetailDescription, BuildInventoryQuickRollPendingText(context));
            GameModule.UI.ShowUIAsync<DiceRollUI>(new DiceRollUIRequest
            {
                SourceType = "inventory_item",
                SourceId = context.ItemInstanceId,
                SourceName = context.ItemName,
                EffectName = context.EffectName,
                DiceExpression = context.DiceExpression,
                OnResult = OnInventoryDiceRollUIResult
            });
        }

        private static CharacterInventoryQuickRollContext BuildManualInventoryRollContext(CharacterEquipmentItemSaveData item)
        {
            return new CharacterInventoryQuickRollContext
            {
                ItemInstanceId = item?.ItemInstanceId ?? string.Empty,
                ItemName = FirstNonEmpty(item?.ItemName, item?.ItemId),
                EffectName = "手动掷骰",
                DiceExpression = "1d20"
            };
        }

        private void OnInventoryDiceRollUIResult(DiceRollUIResult result)
        {
            if (result == null)
            {
                return;
            }

            if (m_visibleInventoryQuickRollContext == null
                || (!string.IsNullOrWhiteSpace(m_visibleInventoryQuickRollContext.ItemInstanceId)
                    && !string.Equals(m_visibleInventoryQuickRollContext.ItemInstanceId, result.SourceId?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            bool isNewRollResult = !ReferenceEquals(m_visibleInventoryQuickRollResult, result.RollResult);
            m_visibleInventoryQuickRollResult = result.RollResult;
            m_visibleInventoryQuickRollPurpose = ConvertDiceRollPurpose(result.Purpose);
            string purposeDisplayName = GetQuickRollPurposeDisplayName(m_visibleInventoryQuickRollPurpose);
            if (isNewRollResult || string.IsNullOrWhiteSpace(m_visibleInventoryQuickRollHistoryEntryId))
            {
                CharacterDiceRollHistoryEntry historyEntry = CharacterCreationSessionService.Instance.AddDiceRollHistoryEntry(
                    m_visibleInventoryQuickRollContext,
                    m_visibleInventoryQuickRollResult,
                    purposeDisplayName);
                m_visibleInventoryQuickRollHistoryEntryId = historyEntry?.EntryId ?? string.Empty;
            }
            else
            {
                CharacterCreationSessionService.Instance.UpdateDiceRollHistoryPurpose(
                    m_visibleInventoryQuickRollHistoryEntryId,
                    purposeDisplayName);
            }

            SetActive(m_rectInventoryQuickRollPurposeButtons != null ? m_rectInventoryQuickRollPurposeButtons.gameObject : null, false);
            RefreshQuickRollApplyResultButtonState();
            SetText(m_tmpFeatureDetailTitle, "快捷掷骰");
            SetText(m_tmpFeatureDetailDescription, BuildInventoryQuickRollResultText(
                m_visibleInventoryQuickRollContext,
                m_visibleInventoryQuickRollResult,
                m_visibleInventoryQuickRollPurpose));
        }

        private void OnClickQuickRollPurposeButton(CharacterInventoryQuickRollPurpose purpose)
        {
            if (m_visibleInventoryQuickRollContext == null)
            {
                return;
            }

            m_visibleInventoryQuickRollPurpose = purpose;
            CharacterCreationSessionService.Instance.UpdateDiceRollHistoryPurpose(
                m_visibleInventoryQuickRollHistoryEntryId,
                GetQuickRollPurposeDisplayName(m_visibleInventoryQuickRollPurpose));
            SetActive(m_rectInventoryQuickRollPurposeButtons != null ? m_rectInventoryQuickRollPurposeButtons.gameObject : null, true);
            RefreshQuickRollApplyResultButtonState();
            SetText(m_tmpFeatureDetailTitle, "快捷掷骰");
            SetText(m_tmpFeatureDetailDescription, BuildInventoryQuickRollResultText(
                m_visibleInventoryQuickRollContext,
                m_visibleInventoryQuickRollResult,
                m_visibleInventoryQuickRollPurpose));
        }

        private void RefreshQuickRollApplyResultButtonState()
        {
            if (m_btnQuickRollApplyResult == null)
            {
                return;
            }

            m_btnQuickRollApplyResult.interactable = m_visibleInventoryQuickRollResult != null
                && m_visibleInventoryQuickRollResult.Success
                && m_visibleInventoryQuickRollPurpose == CharacterInventoryQuickRollPurpose.HealHp;
        }

        private void OnClickQuickRollApplyResultButton()
        {
            if (m_visibleInventoryQuickRollContext == null
                || m_visibleInventoryQuickRollResult == null
                || !m_visibleInventoryQuickRollResult.Success
                || m_visibleInventoryQuickRollPurpose != CharacterInventoryQuickRollPurpose.HealHp)
            {
                return;
            }

            CharacterCreationViewState state = CharacterCreationViewStateService.Instance.BuildCurrentViewState(
                ParseLevel(m_tmpInputLevel != null ? m_tmpInputLevel.text : string.Empty));
            int maxHp = ParsePlainInt(state?.MaxHpText);
            int previousHp = ParsePlainInt(state?.CurrentHpText);
            int healAmount = Math.Max(0, m_visibleInventoryQuickRollResult.Total);
            if (maxHp <= 0 || healAmount <= 0)
            {
                return;
            }

            CharacterEquipmentItemSaveData item = FindDraftInventoryItem(m_visibleInventoryItemInstanceId);
            string consumeMessage = string.Empty;
            if (IsInventoryItemUsable(item))
            {
                CharacterInventoryOperationResult consumeResult = CharacterCreationSessionService.Instance.UseInventoryItem(
                    m_visibleInventoryItemInstanceId,
                    GetInventoryUseAmount());
                if (consumeResult == null || !consumeResult.Success)
                {
                    Log.Warning(consumeResult == null || string.IsNullOrWhiteSpace(consumeResult.Message) ? "物品消耗失败。" : consumeResult.Message);
                    return;
                }

                consumeMessage = BuildQuickRollConsumeMessage(consumeResult);
            }

            if (!CharacterCreationSessionService.Instance.HealCurrentHp(healAmount, maxHp))
            {
                return;
            }

            RefreshCreationView();
            CharacterCreationViewState refreshedState = CharacterCreationViewStateService.Instance.BuildCurrentViewState(
                ParseLevel(m_tmpInputLevel != null ? m_tmpInputLevel.text : string.Empty));
            int currentHp = ParsePlainInt(refreshedState?.CurrentHpText);
            bool itemStillExists = TryFindInventoryDisplayEntry(m_visibleInventoryItemInstanceId, out _);
            string appliedMessage = BuildQuickRollAppliedMessage(previousHp, currentHp, refreshedState?.MaxHpText, consumeMessage);
            CharacterCreationSessionService.Instance.MarkDiceRollHistoryApplied(m_visibleInventoryQuickRollHistoryEntryId, appliedMessage);
            SetText(m_tmpFeatureDetailTitle, "快捷掷骰");
            SetText(m_tmpFeatureDetailDescription, BuildInventoryQuickRollResultText(
                m_visibleInventoryQuickRollContext,
                m_visibleInventoryQuickRollResult,
                m_visibleInventoryQuickRollPurpose,
                appliedMessage));
            m_visibleInventoryQuickRollResult = null;
            RefreshQuickRollApplyResultButtonState();
            if (!itemStillExists)
            {
                HideInventoryActionButtons();
            }
        }

        private static string BuildQuickRollConsumeMessage(CharacterInventoryOperationResult result)
        {
            if (result == null || string.IsNullOrWhiteSpace(result.Message))
            {
                return string.Empty;
            }

            string message = result.Message.Trim();
            if (string.Equals(message, "Item charge consumed.", StringComparison.OrdinalIgnoreCase))
            {
                return "已消耗充能。";
            }

            if (string.Equals(message, "Item quantity consumed.", StringComparison.OrdinalIgnoreCase))
            {
                return "已消耗数量。";
            }

            if (string.Equals(message, "Item consumed.", StringComparison.OrdinalIgnoreCase))
            {
                return "物品已消耗。";
            }

            return message;
        }

        private static string BuildQuickRollAppliedMessage(int previousHp, int currentHp, string maxHpText, string consumeMessage)
        {
            string message = $"已应用恢复生命值：{previousHp} -> {currentHp}/{(string.IsNullOrWhiteSpace(maxHpText) ? "-" : maxHpText.Trim())}";
            if (!string.IsNullOrWhiteSpace(consumeMessage))
            {
                message = $"{message}\n{consumeMessage.Trim()}";
            }

            return message;
        }

        private string BuildInventoryQuickRollResultText(
            CharacterInventoryQuickRollContext context,
            CharacterDiceRollResultData result,
            CharacterInventoryQuickRollPurpose purpose,
            string appliedMessage = "")
        {
            if (context == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            AppendLocalItemDetailLine(builder, "来源物品", context.ItemName);
            AppendLocalItemDetailLine(builder, "词条", context.EffectName);
            AppendLocalItemDetailLine(builder, "骰子表达式", context.DiceExpression);

            if (result == null || !result.Success)
            {
                AppendLocalItemDetailLine(builder, "掷骰失败", result?.Error ?? "未知掷骰错误。");
                return builder.ToString();
            }

            AppendLocalItemDetailLine(builder, "掷骰结果", result.Summary);
            AppendLocalItemDetailLine(builder, "总值", result.Total.ToString());
            AppendInventoryQuickRollPurposeText(builder, purpose, result);
            AppendLocalItemDetailLine(builder, "应用结果", appliedMessage);
            AppendDiceRollHistoryText(builder);
            return builder.ToString();
        }

        private static string BuildInventoryQuickRollPendingText(CharacterInventoryQuickRollContext context)
        {
            if (context == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            AppendLocalItemDetailLine(builder, "来源物品", context.ItemName);
            AppendLocalItemDetailLine(builder, "词条", context.EffectName);
            AppendLocalItemDetailLine(builder, "骰子表达式", context.DiceExpression);
            AppendLocalItemDetailLine(builder, "状态", "已打开掷骰弹窗，请在弹窗中完成掷骰。");
            return builder.ToString();
        }

        private static void AppendDiceRollHistoryText(StringBuilder builder)
        {
            if (builder == null)
            {
                return;
            }

            IReadOnlyList<CharacterDiceRollHistoryEntry> history = CharacterCreationSessionService.Instance.DiceRollHistory;
            if (history == null || history.Count == 0)
            {
                return;
            }

            builder.AppendLine();
            builder.AppendLine(CharacterDiceRollHistoryFormatter.BuildRecentHistoryText(history, 5, true));
        }

        private void AppendInventoryQuickRollPurposeText(
            StringBuilder builder,
            CharacterInventoryQuickRollPurpose purpose,
            CharacterDiceRollResultData result)
        {
            if (builder == null || result == null || !result.Success)
            {
                return;
            }

            CharacterCreationViewState state = CharacterCreationViewStateService.Instance.BuildCurrentViewState(
                ParseLevel(m_tmpInputLevel != null ? m_tmpInputLevel.text : string.Empty));
            CharacterRuntimeSnapshotData snapshot = CharacterDetailCalculationService.Instance.BuildDisplaySnapshot(
                CharacterCreationSessionService.Instance.CurrentState?.Character);

            AppendLocalItemDetailLine(builder, "用途", GetQuickRollPurposeDisplayName(purpose));
            switch (purpose)
            {
                case CharacterInventoryQuickRollPurpose.HealHp:
                    AppendLocalItemDetailLine(builder, "当前生命值", $"{state.CurrentHpText}/{state.MaxHpText}");
                    AppendLocalItemDetailLine(builder, "恢复后预览", BuildHealHpPreviewText(state, result.Total));
                    break;
                case CharacterInventoryQuickRollPurpose.AttackHit:
                    AppendLocalItemDetailLine(builder, "当前通用命中加值", BuildSignedSnapshotValue(snapshot?.AttackBonus ?? 0));
                    AppendLocalItemDetailLine(builder, "当前武器命中加值", BuildSignedSnapshotValue(snapshot?.WeaponAttackBonus ?? 0));
                    AppendLocalItemDetailLine(builder, "通用命中预览", BuildTotalWithBonusPreview(result.Total, snapshot?.AttackBonus ?? 0));
                    AppendLocalItemDetailLine(builder, "物品武器命中预览", BuildTotalWithBonusPreview(result.Total, (snapshot?.AttackBonus ?? 0) + (snapshot?.WeaponAttackBonus ?? 0)));
                    AppendLocalItemDetailLine(builder, "说明", "这里显示物品/装备提供的通用命中加值；具体攻击动作的属性调整值和熟练项仍由玩家选择判断。");
                    break;
                case CharacterInventoryQuickRollPurpose.Damage:
                    AppendLocalItemDetailLine(builder, "当前伤害加值", BuildSignedSnapshotValue(snapshot?.DamageBonus ?? 0));
                    AppendLocalItemDetailLine(builder, "伤害预览", BuildTotalWithBonusPreview(result.Total, snapshot?.DamageBonus ?? 0));
                    break;
                case CharacterInventoryQuickRollPurpose.SkillCheck:
                    AppendLocalItemDetailLine(builder, "技能加值", BuildSkillReferenceText(state));
                    AppendLocalItemDetailLine(builder, "技能结果预览", BuildSkillRollPreviewText(state, result.Total));
                    AppendLocalItemDetailLine(builder, "说明", "请选择本次检定对应的技能，使用对应预览结果。");
                    break;
                case CharacterInventoryQuickRollPurpose.SavingThrow:
                    AppendLocalItemDetailLine(builder, "当前豁免通用加值", BuildSignedSnapshotValue(snapshot?.SavingThrowBonus ?? 0));
                    AppendLocalItemDetailLine(builder, "通用豁免预览", BuildTotalWithBonusPreview(result.Total, snapshot?.SavingThrowBonus ?? 0));
                    AppendLocalItemDetailLine(builder, "说明", "这里显示物品/装备提供的通用豁免加值；具体豁免仍需结合对应属性调整值和豁免熟练项判断。");
                    break;
                case CharacterInventoryQuickRollPurpose.SpellAttack:
                    AppendLocalItemDetailLine(builder, "当前法术攻击加值", state.SpellAttackBonusText);
                    AppendLocalItemDetailLine(builder, "法术攻击预览", BuildTotalWithBonusPreview(result.Total, ParseSignedText(state.SpellAttackBonusText)));
                    break;
                case CharacterInventoryQuickRollPurpose.SpellSaveDc:
                    AppendLocalItemDetailLine(builder, "当前法术豁免DC", state.SpellSaveDcText);
                    AppendLocalItemDetailLine(builder, "掷骰结果", result.Total.ToString());
                    AppendLocalItemDetailLine(builder, "说明", "法术豁免DC通常不与掷骰总值相加，这里仅显示当前最终DC供判断。");
                    break;
                case CharacterInventoryQuickRollPurpose.Custom:
                    AppendLocalItemDetailLine(builder, "说明", "自定义用途由玩家和DM根据物品描述判断。");
                    break;
                default:
                    AppendLocalItemDetailLine(builder, "说明", "仅显示本次掷骰结果，不应用到角色数据。");
                    break;
            }
        }

        private static string GetQuickRollPurposeDisplayName(CharacterInventoryQuickRollPurpose purpose)
        {
            switch (purpose)
            {
                case CharacterInventoryQuickRollPurpose.HealHp:
                    return "恢复生命值";
                case CharacterInventoryQuickRollPurpose.AttackHit:
                    return "攻击命中";
                case CharacterInventoryQuickRollPurpose.Damage:
                    return "伤害数值";
                case CharacterInventoryQuickRollPurpose.SkillCheck:
                    return "技能检定";
                case CharacterInventoryQuickRollPurpose.SavingThrow:
                    return "豁免检定";
                case CharacterInventoryQuickRollPurpose.SpellAttack:
                    return "法术攻击";
                case CharacterInventoryQuickRollPurpose.SpellSaveDc:
                    return "法术豁免DC";
                case CharacterInventoryQuickRollPurpose.Custom:
                    return "自定义/DM判断";
                default:
                    return "仅显示结果";
            }
        }

        private static CharacterInventoryQuickRollPurpose ConvertDiceRollPurpose(string purpose)
        {
            switch (purpose?.Trim())
            {
                case "heal_hp":
                    return CharacterInventoryQuickRollPurpose.HealHp;
                case "attack_hit":
                    return CharacterInventoryQuickRollPurpose.AttackHit;
                case "damage":
                    return CharacterInventoryQuickRollPurpose.Damage;
                case "skill_check":
                    return CharacterInventoryQuickRollPurpose.SkillCheck;
                case "saving_throw":
                    return CharacterInventoryQuickRollPurpose.SavingThrow;
                case "spell_attack":
                    return CharacterInventoryQuickRollPurpose.SpellAttack;
                case "spell_save_dc":
                    return CharacterInventoryQuickRollPurpose.SpellSaveDc;
                case "custom":
                    return CharacterInventoryQuickRollPurpose.Custom;
                default:
                    return CharacterInventoryQuickRollPurpose.DisplayOnly;
            }
        }

        private static string BuildHealHpPreviewText(CharacterCreationViewState state, int healValue)
        {
            int currentHp = ParsePlainInt(state?.CurrentHpText);
            int maxHp = ParsePlainInt(state?.MaxHpText);
            if (maxHp <= 0)
            {
                return $"恢复 {Math.Max(0, healValue)} 点，当前最大生命值未知。";
            }

            int healedHp = Mathf.Clamp(currentHp + Math.Max(0, healValue), 0, maxHp);
            return $"{currentHp} + {Math.Max(0, healValue)} => {healedHp}/{maxHp}";
        }

        private static string BuildTotalWithBonusPreview(int rollTotal, int bonus)
        {
            if (bonus == 0)
            {
                return rollTotal.ToString();
            }

            return $"{rollTotal} {FormatSigned(bonus)} = {rollTotal + bonus}";
        }

        private static string BuildSignedSnapshotValue(int value)
        {
            return value == 0 ? "0" : FormatSigned(value);
        }

        private static string BuildSkillReferenceText(CharacterCreationViewState state)
        {
            if (state?.Skills == null || state.Skills.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < SkillDisplayBindings.Length && index < state.Skills.Count; index++)
            {
                CharacterCreationSkillViewState skill = state.Skills[index];
                if (skill == null)
                {
                    continue;
                }

                AppendSkillPreviewSeparator(builder, index);

                builder.Append(SkillDisplayBindings[index].DisplayName);
                builder.Append(skill.BonusText);
            }

            return builder.ToString();
        }

        private static string BuildSkillRollPreviewText(CharacterCreationViewState state, int rollTotal)
        {
            if (state?.Skills == null || state.Skills.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < SkillDisplayBindings.Length && index < state.Skills.Count; index++)
            {
                CharacterCreationSkillViewState skill = state.Skills[index];
                if (skill == null)
                {
                    continue;
                }

                AppendSkillPreviewSeparator(builder, index);

                builder.Append(SkillDisplayBindings[index].DisplayName);
                builder.Append("：");
                builder.Append(rollTotal + skill.Bonus);
            }

            return builder.ToString();
        }

        private static void AppendSkillPreviewSeparator(StringBuilder builder, int index)
        {
            if (builder == null || builder.Length == 0)
            {
                return;
            }

            builder.Append(index > 0 && index % 6 == 0 ? "\n" : "，");
        }

        private static int ParseSignedText(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Trim() == "-")
            {
                return 0;
            }

            return int.TryParse(value.Trim(), out int result) ? result : 0;
        }

        private static int ParsePlainInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Trim() == "-")
            {
                return 0;
            }

            return int.TryParse(value.Trim(), out int result) ? result : 0;
        }

        private void RefreshAfterInventoryAction(CharacterInventoryOperationResult result, string successMessage)
        {
            if (result == null || !result.Success)
            {
                Log.Warning(result == null || string.IsNullOrWhiteSpace(result.Message) ? "物品操作失败。" : result.Message);
                return;
            }

            string itemInstanceId = result.ItemInstanceId;
            RefreshCreationView();
            if (TryFindInventoryDisplayEntry(itemInstanceId, out CharacterInventoryDisplayEntry entry))
            {
                ShowInventoryItemDetail(entry);
                return;
            }

            SetText(m_tmpFeatureDetailTitle, "物品");
            SetText(m_tmpFeatureDetailDescription, successMessage);
            HideInventoryActionButtons();
        }

        private bool TryFindInventoryDisplayEntry(string itemInstanceId, out CharacterInventoryDisplayEntry entry)
        {
            entry = default;
            CharacterCreationViewState state = CharacterCreationViewStateService.Instance.BuildCurrentViewState(
                ParseLevel(m_tmpInputLevel != null ? m_tmpInputLevel.text : string.Empty));
            IReadOnlyList<CharacterInventoryDisplayEntry> entries = state?.InventoryItems;
            if (entries == null)
            {
                return false;
            }

            for (int index = 0; index < entries.Count; index++)
            {
                CharacterInventoryDisplayEntry candidate = entries[index];
                if (string.Equals(candidate.ItemInstanceId, itemInstanceId?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                {
                    entry = candidate;
                    return true;
                }
            }

            return false;
        }

        private void RefreshClassFeatureItems(IReadOnlyList<CharacterCreationFeatureDisplayEntry> entries)
        {
            RefreshFeatureLabelItems(m_classFeatureItems, m_rectClassFeatureContent, m_goClassFeatureTemplate, entries, "m_itemClassFeature_", "m_tmpClassFeatureTitle");
        }

        private void RefreshRaceFeatureItems(IReadOnlyList<CharacterCreationFeatureDisplayEntry> entries)
        {
            RefreshFeatureLabelItems(m_raceFeatureItems, m_rectRaceFeatureContent, m_goRaceFeatureTemplate, entries, "m_itemRaceFeature_", "m_tmpRaceFeatureTitle");
        }

        private void RefreshOtherFeatureItems(IReadOnlyList<CharacterCreationFeatureDisplayEntry> entries)
        {
            RefreshFeatureLabelItems(m_otherFeatureItems, m_rectOtherFeatureContent, m_goOtherFeatureTemplate, entries, "m_itemOtherFeature_", "m_tmpOtherFeatureTitle");
        }

        private void RefreshStatusEffectItems(IReadOnlyList<CharacterStatusEffectDisplayEntry> entries)
        {
            if (m_rectStatusEffectContent == null || m_goStatusEffectTemplate == null)
            {
                return;
            }

            int count = entries?.Count ?? 0;
            while (m_statusEffectItems.Count < count)
            {
                GameObject itemObject = UnityEngine.Object.Instantiate(m_goStatusEffectTemplate, m_rectStatusEffectContent);
                itemObject.name = $"m_itemStatusEffect_{m_statusEffectItems.Count + 1}";
                itemObject.SetActive(true);
                m_statusEffectItems.Add(CharacterCreationStatusEffectItemView.Bind(itemObject));
            }

            if (m_goStatusEffectTemplate.activeSelf)
            {
                m_goStatusEffectTemplate.SetActive(false);
            }

            SetActive(m_rectStatusEffectContent.gameObject, count > 0);

            for (int index = 0; index < m_statusEffectItems.Count; index++)
            {
                CharacterCreationStatusEffectItemView item = m_statusEffectItems[index];
                bool active = index < count;
                item.SetActive(active);
                if (active)
                {
                    item.Bind(entries[index]);
                }
            }
        }

        private void RefreshLearnedSpellItems(IReadOnlyList<CharacterCreationSpellCardViewState> entries)
        {
            RefreshSpellCardItems(
                m_learnedSpellCards,
                m_rectSpellContent,
                m_goLearnedSpellTemplate,
                entries,
                "m_itemLearnedSpell_",
                OnClickSpellCard);
        }

        private void RefreshSpellCardItems(
            List<CharacterCreationSpellCardView> itemViews,
            RectTransform content,
            GameObject template,
            IReadOnlyList<CharacterCreationSpellCardViewState> entries,
            string generatedNamePrefix,
            Action<string> onClick)
        {
            if (itemViews == null || content == null || template == null)
            {
                return;
            }

            int count = entries?.Count ?? 0;
            while (itemViews.Count < count)
            {
                GameObject itemObject = UnityEngine.Object.Instantiate(template, content);
                itemObject.name = $"{generatedNamePrefix}{itemViews.Count + 1}";
                itemObject.SetActive(true);
                itemViews.Add(CharacterCreationSpellCardView.Bind(itemObject));
            }

            if (template.activeSelf)
            {
                template.SetActive(false);
            }

            for (int index = 0; index < itemViews.Count; index++)
            {
                CharacterCreationSpellCardView itemView = itemViews[index];
                bool active = index < count;
                itemView.SetActive(active);
                if (!active)
                {
                    continue;
                }

                CharacterCreationSpellCardViewState entry = entries[index];
                itemView.Bind(entry, onClick);
            }
        }

        private void OnClickAvailableSpellCard(string spellId)
        {
            if (string.IsNullOrWhiteSpace(spellId))
            {
                return;
            }

            CharacterCreationSessionService.Instance.SetPendingSpellSelection(spellId, m_activeSpellFilterLevel);
            RefreshCreationView();
            ShowSpellOptions(m_activeSpellFilterLevel);
            OnClickSpellCard(spellId);
        }

        private void OnClickSpellCard(string spellId)
        {
            if (string.IsNullOrWhiteSpace(spellId))
            {
                return;
            }

            HideInventoryActionButtons();
            SetText(m_tmpFeatureDetailTitle, CharacterCreationSpellDisplayService.Instance.GetSpellDetailTitle(spellId));
            SetText(m_tmpFeatureDetailDescription, CharacterCreationSpellDisplayService.Instance.GetSpellDetailDescription(spellId));
        }

        private void HideRightPanelTemplates()
        {
            if (m_rectInfoListContent == null)
            {
                return;
            }

            for (int index = 0; index < m_rectInfoListContent.childCount; index++)
            {
                Transform child = m_rectInfoListContent.GetChild(index);
                if (child == null)
                {
                    continue;
                }

                bool isGeneratedClassCard = child.name.StartsWith("m_itemClass_", StringComparison.Ordinal);
                bool isGeneratedRaceCard = child.name.StartsWith("m_itemRace_", StringComparison.Ordinal);
                bool isGeneratedBackgroundCard = child.name.StartsWith("m_itemBackground_", StringComparison.Ordinal);
                bool isGeneratedAlignmentCard = child.name.StartsWith("m_itemAlignment_", StringComparison.Ordinal);
                bool isGeneratedSpellCard = child.name.StartsWith("m_itemSpellOption_", StringComparison.Ordinal);
                bool isGeneratedLocalItemCard = child.name.StartsWith("m_itemLocalInventory_", StringComparison.Ordinal);
                if (!isGeneratedClassCard && !isGeneratedRaceCard && !isGeneratedBackgroundCard && !isGeneratedAlignmentCard && !isGeneratedSpellCard && !isGeneratedLocalItemCard)
                {
                    child.gameObject.SetActive(false);
                }
                else
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        private void SaveDraft()
        {
            CharacterCreationDraftInput input = BuildCreationDraftInput();
            CharacterOperationResult result = CharacterApplicationService.Instance.SaveCreationDraft(input);
            if (!result.Success)
            {
                Log.Warning(string.IsNullOrWhiteSpace(result.Message) ? "角色保存失败。" : result.Message);
                return;
            }

            ReturnToCharacterManagement();
        }

        private CharacterCreationDraftInput BuildCreationDraftInput()
        {
            int level = ParseLevel(m_tmpInputLevel != null ? m_tmpInputLevel.text : string.Empty);
            CharacterCreationFormInput form = new CharacterCreationFormInput
            {
                CharacterName = m_tmpInputCharacterName != null ? m_tmpInputCharacterName.text.Trim() : string.Empty,
                RaceId = SelectedRaceId,
                ClassId = SelectedClassId,
                BackgroundId = SelectedBackgroundId,
                AlignmentId = SelectedAlignmentId,
                Level = level,
                Speed = CharacterCreationViewStateService.Instance.GetSelectedRaceSpeed(),
                BaseAbilityScore = BaseAbilityScore,
                FixedSkillProficiencyIds = CharacterCreationViewStateService.Instance.BuildFixedSkillProficiencyIds(),
                FixedToolProficiencyIds = CharacterCreationViewStateService.Instance.BuildFixedToolProficiencyIds()
            };

            CharacterCreationDraftInput input = CharacterCreationSessionService.Instance.BuildDraftInput(form);
            input.PreviewImagePath = m_previewImagePath?.Trim() ?? string.Empty;
            input.PersonalityTraits = m_tmpInputPersonalityTraits != null ? m_tmpInputPersonalityTraits.text.Trim() : string.Empty;
            input.Ideals = m_tmpInputIdeals != null ? m_tmpInputIdeals.text.Trim() : string.Empty;
            input.Bonds = m_tmpInputBonds != null ? m_tmpInputBonds.text.Trim() : string.Empty;
            input.Flaws = m_tmpInputFlaws != null ? m_tmpInputFlaws.text.Trim() : string.Empty;
            return input;
        }

        private async UniTaskVoid UploadPortraitImageAsync()
        {
            string sourceFilePath = RuntimeImageFileDialog.OpenImageFile("选择角色样貌图片");
            if (string.IsNullOrWhiteSpace(sourceFilePath))
            {
                return;
            }

            if (!File.Exists(sourceFilePath))
            {
                Log.Error($"角色样貌图片文件不存在: {sourceFilePath}");
                return;
            }

            string targetDirectoryPath = CharacterPortraitApplicationService.Instance.GetPortraitDirectoryPath();
            string targetFilePath;
            byte[] imageBytes;
            try
            {
                targetFilePath = await UniTask.RunOnThreadPool(() => CharacterPortraitApplicationService.Instance.StorePortraitImage(sourceFilePath, targetDirectoryPath));
                imageBytes = await UniTask.RunOnThreadPool(() => CharacterPortraitApplicationService.Instance.ReadImageBytes(targetFilePath));
            }
            catch (Exception exception)
            {
                Log.Error($"保存角色样貌图片失败: {exception.Message}");
                return;
            }

            if (string.IsNullOrWhiteSpace(targetFilePath) || imageBytes == null || imageBytes.Length == 0)
            {
                Log.Error("角色样貌图片保存失败。");
                return;
            }

            ApplyPortraitImage(targetFilePath, imageBytes, ++m_portraitLoadVersion, true);
        }

        private void ApplyPortraitImage(string imagePath, byte[] imageBytes, int loadVersion, bool logOnFailure)
        {
            if (loadVersion != m_portraitLoadVersion)
            {
                return;
            }

            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(imageBytes))
            {
                UnityEngine.Object.Destroy(texture);
                if (logOnFailure)
                {
                    Log.Error("角色样貌图片加载失败，文件不是有效图片。");
                }

                return;
            }

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            CleanupPortraitResources();
            m_portraitTexture = texture;
            m_portraitSprite = sprite;
            m_previewImagePath = imagePath?.Trim() ?? string.Empty;
            CharacterCreationSessionService.Instance.SetPreviewImagePath(m_previewImagePath);

            if (m_imgPortraitUpload != null)
            {
                m_imgPortraitUpload.sprite = m_portraitSprite;
                m_imgPortraitUpload.color = Color.white;
                m_imgPortraitUpload.preserveAspect = true;
            }

            SetActive(m_tmpPortraitUploadHint != null ? m_tmpPortraitUploadHint.gameObject : null, false);
        }

        private void CleanupPortraitResources()
        {
            if (m_portraitSprite != null)
            {
                UnityEngine.Object.Destroy(m_portraitSprite);
                m_portraitSprite = null;
            }

            if (m_portraitTexture != null)
            {
                UnityEngine.Object.Destroy(m_portraitTexture);
                m_portraitTexture = null;
            }
        }

        private void ReturnToCharacterManagement()
        {
            GameModule.UI.CloseUI<CharacterCreationUI>();
            GameModule.UI.ShowUIAsync<CharacterCardManagementUI>();
        }

        private T FindChildComponent<T>(string childName) where T : Component
        {
            Transform[] children = gameObject.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < children.Length; index++)
            {
                Transform child = children[index];
                if (child != null && child.name == childName)
                {
                    return child.GetComponent<T>();
                }
            }

            return null;
        }

        private static T FindChildComponentInChildren<T>(Transform parent, string childName) where T : Component
        {
            if (parent == null || string.IsNullOrWhiteSpace(childName))
            {
                return null;
            }

            Transform[] children = parent.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < children.Length; index++)
            {
                Transform child = children[index];
                if (child != null && child.name == childName)
                {
                    return child.GetComponent<T>();
                }
            }

            return null;
        }

        private Button FindChildButton(string childName)
        {
            RectTransform rectTransform = FindChildComponent<RectTransform>(childName);
            if (rectTransform == null)
            {
                return null;
            }

            Button button = rectTransform.GetComponent<Button>();
            if (button == null)
            {
                button = rectTransform.gameObject.AddComponent<Button>();
            }

            button.transition = Selectable.Transition.None;
            button.interactable = true;
            EnsureButtonRaycastTarget(button);
            EnsureMinLayoutHeight(rectTransform, SectionButtonMinHeight);
            return button;
        }

        private Button FindChildButtonInParent(string parentName, string childName)
        {
            RectTransform parent = FindChildComponent<RectTransform>(parentName);
            if (parent == null || string.IsNullOrWhiteSpace(childName))
            {
                return null;
            }

            Transform[] children = parent.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < children.Length; index++)
            {
                Transform child = children[index];
                if (child != null && child.name == childName)
                {
                    Button button = child.GetComponent<Button>();
                    if (button == null)
                    {
                        button = child.gameObject.AddComponent<Button>();
                    }

                    button.transition = Selectable.Transition.None;
                    button.interactable = true;
                    EnsureButtonRaycastTarget(button);
                    return button;
                }
            }

            return null;
        }

        private RectTransform FindScrollContent(string scrollName)
        {
            ScrollRect scrollRect = FindChildComponent<ScrollRect>(scrollName);
            return scrollRect != null ? scrollRect.content : null;
        }

        private static string KeepDigitsOnly(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            char[] buffer = new char[value.Length];
            int length = 0;
            for (int index = 0; index < value.Length; index++)
            {
                char character = value[index];
                if (character >= '0' && character <= '9')
                {
                    buffer[length] = character;
                    length++;
                }
            }

            return new string(buffer, 0, length);
        }

        private static int ParseLevel(string value)
        {
            int level;
            if (!int.TryParse(KeepDigitsOnly(value), out level))
            {
                level = MinCharacterLevel;
            }

            return Mathf.Clamp(level, MinCharacterLevel, MaxCharacterLevel);
        }

        private static void ToggleRectActive(RectTransform target)
        {
            if (target == null)
            {
                return;
            }

            GameObject targetObject = target.gameObject;
            targetObject.SetActive(!targetObject.activeSelf);
            LayoutRebuilder.ForceRebuildLayoutImmediate(target.parent as RectTransform);
        }

        private static void EnsureButtonRaycastTarget(Button button)
        {
            if (button == null)
            {
                return;
            }

            Graphic graphic = button.targetGraphic != null ? button.targetGraphic : button.GetComponent<Graphic>();
            if (graphic == null)
            {
                Image image = button.gameObject.AddComponent<Image>();
                image.color = new Color(1f, 1f, 1f, 0f);
                image.raycastTarget = true;
                button.targetGraphic = image;
                return;
            }

            graphic.raycastTarget = true;
            button.targetGraphic = graphic;
        }

        private static void EnsureMinLayoutHeight(RectTransform rectTransform, float minHeight)
        {
            if (rectTransform == null || rectTransform.rect.height > 0f)
            {
                return;
            }

            LayoutElement layoutElement = rectTransform.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            }

            if (layoutElement.minHeight < minHeight)
            {
                layoutElement.minHeight = minHeight;
            }

            if (layoutElement.preferredHeight < minHeight)
            {
                layoutElement.preferredHeight = minHeight;
            }
        }

        private static string FirstNonEmpty(string first, string second)
        {
            return !string.IsNullOrWhiteSpace(first) ? first.Trim() : second?.Trim() ?? string.Empty;
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value ?? string.Empty;
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

        private static List<LocalCustomItemSaveData> LoadLocalItemOptions()
        {
            LocalCustomItemLibrarySaveData library = LocalCustomItemRepository.Load();
            List<LocalCustomItemSaveData> result = new List<LocalCustomItemSaveData>();
            if (library?.Items == null)
            {
                return result;
            }

            for (int index = 0; index < library.Items.Count; index++)
            {
                LocalCustomItemSaveData item = LocalCustomItemSaveData.Clone(library.Items[index]);
                if (!string.IsNullOrWhiteSpace(item.CustomItemId) && CharacterEquipmentItemSaveData.HasItem(item.Item))
                {
                    result.Add(item);
                }
            }

            return result;
        }

        private LocalCustomItemSaveData FindLocalItemOption(string customItemId)
        {
            if (string.IsNullOrWhiteSpace(customItemId))
            {
                return null;
            }

            string normalizedId = customItemId.Trim();
            for (int index = 0; index < m_localItemOptions.Count; index++)
            {
                LocalCustomItemSaveData item = m_localItemOptions[index];
                if (item != null && string.Equals(item.CustomItemId, normalizedId, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }

            return LocalCustomItemRepository.TryGetItem(normalizedId, out LocalCustomItemSaveData repositoryItem)
                ? repositoryItem
                : null;
        }

        private static string BuildLocalItemOptionLabel(LocalCustomItemSaveData item)
        {
            CharacterEquipmentItemSaveData data = item?.Item;
            string label = FirstNonEmpty(data?.ItemName, data?.ItemId);
            if (string.IsNullOrWhiteSpace(label))
            {
                label = item?.CustomItemId?.Trim() ?? "未命名物品";
            }

            int quantity = Math.Max(1, data?.Quantity ?? 1);
            return quantity > 1 ? $"{label} x{quantity}" : label;
        }

        private static string FormatItemRarity(string rarity)
        {
            string normalized = rarity?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return "无";
            }

            if (string.Equals(normalized, "common", StringComparison.OrdinalIgnoreCase))
            {
                return "普通";
            }

            if (string.Equals(normalized, "uncommon", StringComparison.OrdinalIgnoreCase))
            {
                return "罕见";
            }

            if (string.Equals(normalized, "rare", StringComparison.OrdinalIgnoreCase))
            {
                return "稀有";
            }

            if (string.Equals(normalized, "very_rare", StringComparison.OrdinalIgnoreCase))
            {
                return "非常稀有";
            }

            if (string.Equals(normalized, "legendary", StringComparison.OrdinalIgnoreCase))
            {
                return "传奇";
            }

            if (string.Equals(normalized, "artifact", StringComparison.OrdinalIgnoreCase))
            {
                return "神器";
            }

            return normalized;
        }

        private static string FormatItemType(string itemType)
        {
            string normalized = itemType?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return "未分类";
            }

            if (string.Equals(normalized, "armor", StringComparison.OrdinalIgnoreCase))
            {
                return "护甲";
            }

            if (string.Equals(normalized, "shield", StringComparison.OrdinalIgnoreCase))
            {
                return "盾牌";
            }

            if (string.Equals(normalized, "weapon", StringComparison.OrdinalIgnoreCase))
            {
                return "武器";
            }

            if (string.Equals(normalized, "tool", StringComparison.OrdinalIgnoreCase))
            {
                return "工具";
            }

            if (string.Equals(normalized, "consumable", StringComparison.OrdinalIgnoreCase))
            {
                return "消耗品";
            }

            if (string.Equals(normalized, "wondrous_item", StringComparison.OrdinalIgnoreCase))
            {
                return "奇物";
            }

            return normalized;
        }

        private void ShowLocalItemDetail(LocalCustomItemSaveData item)
        {
            HideInventoryActionButtons();
            CharacterEquipmentItemSaveData data = item?.Item;
            string title = BuildLocalItemOptionLabel(item);
            SetText(m_tmpFeatureDetailTitle, title);
            SetText(m_tmpFeatureDetailDescription, BuildLocalItemDetailDescription(item));
        }

        private static string BuildLocalItemDetailDescription(LocalCustomItemSaveData item)
        {
            CharacterEquipmentItemSaveData data = item?.Item;
            if (data == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            AppendLocalItemDetailLine(builder, "类型", data.ItemType);
            AppendLocalItemDetailLine(builder, "稀有度", data.Rarity);
            AppendLocalItemDetailLine(builder, "数量", Math.Max(1, data.Quantity).ToString());
            AppendLocalItemDetailLine(builder, "护甲类型", data.ArmorCategory);
            if (data.ArmorBaseAc > 0)
            {
                AppendLocalItemDetailLine(builder, "基础AC", data.ArmorBaseAc.ToString());
            }

            if (data.AcBonus != 0)
            {
                AppendLocalItemDetailLine(builder, "AC加值", FormatSigned(data.AcBonus));
            }

            AppendLocalItemDetailLine(builder, "武器类型", data.WeaponCategory);
            AppendLocalItemDetailLine(builder, "伤害", BuildLocalItemDamageText(data));
            AppendLocalItemDetailLine(builder, "工具类型", data.ToolCategory);
            if (data.Charges > 0)
            {
                AppendLocalItemDetailLine(builder, "充能", data.Charges.ToString());
            }

            AppendLocalItemDetailLine(builder, "生效条件", data.EffectApplyCondition);
            AppendLocalItemDetailLine(builder, "描述", data.Description);
            AppendLocalItemDetailLine(builder, "效果", BuildLocalItemEffectsText(data));
            AppendLocalItemDetailLine(builder, "备注", data.Notes);
            return builder.ToString();
        }

        private static string BuildLocalItemDamageText(CharacterEquipmentItemSaveData data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.DamageDice))
            {
                return string.Empty;
            }

            string damage = data.DamageDice.Trim();
            if (!string.IsNullOrWhiteSpace(data.DamageType))
            {
                damage = $"{damage} {data.DamageType.Trim()}";
            }

            if (!string.IsNullOrWhiteSpace(data.TwoHandDamageDice))
            {
                damage = $"{damage} / 双手 {data.TwoHandDamageDice.Trim()}";
            }

            if (data.NormalRange > 0 || data.LongRange > 0)
            {
                damage = $"{damage} ({data.NormalRange}/{data.LongRange})";
            }

            return damage;
        }

        private static string BuildLocalItemEffectsText(CharacterEquipmentItemSaveData data)
        {
            if (data?.CustomEffects == null || data.CustomEffects.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < data.CustomEffects.Count; index++)
            {
                CharacterItemEffectSaveData effect = data.CustomEffects[index];
                if (effect == null)
                {
                    continue;
                }

                string text = FirstNonEmpty(effect.Name, effect.EffectType);
                if (!string.IsNullOrWhiteSpace(effect.Description))
                {
                    text = string.IsNullOrWhiteSpace(text)
                        ? effect.Description.Trim()
                        : $"{text} - {effect.Description.Trim()}";
                }

                if (string.IsNullOrWhiteSpace(text))
                {
                    text = $"{effect.Target} {effect.Value}".Trim();
                }

                string diceExpression = effect.DiceExpression?.Trim() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(diceExpression))
                {
                    text = string.IsNullOrWhiteSpace(text)
                        ? $"快捷掷骰：{diceExpression}"
                        : $"{text}\n快捷掷骰：{diceExpression}";
                }

                if (!string.IsNullOrWhiteSpace(text))
                {
                    if (builder.Length > 0)
                    {
                        builder.AppendLine();
                    }

                    builder.Append(text.Trim());
                }
            }

            return builder.ToString();
        }

        private static void AppendLocalItemDetailLine(StringBuilder builder, string label, string value)
        {
            if (builder == null || string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            builder.Append(label.Trim());
            builder.Append("：");
            builder.AppendLine(value.Trim());
        }

        private bool TryGetSelectedRace(out DndRaceDefineData raceData)
        {
            return CharacterCreationRuleService.Instance.TryGetRace(SelectedRaceId, out raceData);
        }

        private void RebuildRaceAbilityBonuses(DndRaceDefineData raceData)
        {
            CharacterCreationRuleService.Instance.ConfigureRaceAbilityChoice(raceData);
        }

        private bool TryGetSelectedClass(out DndClassDefineData classData)
        {
            classData = null;
            return !string.IsNullOrWhiteSpace(SelectedClassId)
                && CharacterCreationRuleService.Instance.TryGetClass(SelectedClassId, out classData);
        }

        private void ChangeAbilityScore(string abilityId, int delta)
        {
            if (CharacterCreationSessionService.Instance.ChangeAbilityScore(abilityId, delta))
            {
                RefreshCreationView();
                if (m_activeFeatureChoiceState != null && CharacterCreationSessionService.Instance.IsActiveAbilityScoreFeatureChoice())
                {
                    return;
                }
                else if (m_activeFeatureChoiceState != null)
                {
                    ShowFeatureSelectionOptions(m_activeFeatureChoiceState);
                }
            }
        }

        private static string FormatSigned(int value)
        {
            return CharacterCreationCalculationService.Instance.FormatSigned(value);
        }

        private static bool ContainsExactId(IReadOnlyList<string> values, string id)
        {
            return CountExactId(values, id) > 0;
        }

        private static int CountExactId(IReadOnlyList<string> values, string id)
        {
            int count = 0;
            if (values == null || string.IsNullOrWhiteSpace(id))
            {
                return count;
            }

            string normalized = id.Trim();
            for (int index = 0; index < values.Count; index++)
            {
                if (string.Equals(values[index]?.Trim(), normalized, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            return count;
        }

        private static void AppendUniqueExactValue(List<string> target, string value)
        {
            if (target == null || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (!ContainsExactId(target, value))
            {
                target.Add(value.Trim());
            }
        }

        private CharacterCreationFeatureChoiceState TryGetNextPendingFeatureChoiceState(CharacterCreationFeatureDisplayEntry entry)
        {
            if (entry == null)
            {
                return null;
            }

            if (entry.IsChoiceOptionDisplay)
            {
                return null;
            }

            if (entry.ChoiceGroupIds.Count == 0)
            {
                return null;
            }

            CharacterCreationFeatureChoiceState state = CharacterCreationSessionService.Instance.FindOrCreateFeatureChoiceState(
                entry.ChoiceGroupIds,
                entry.SourceType,
                entry.SourceId,
                entry.Level);
            if (state == null)
            {
                return null;
            }

            if (!CharacterCreationFeatureDisplayService.Instance.IsFeatureChoiceCompleted(state))
            {
                return state;
            }

            return CharacterCreationSessionService.Instance.ResumeIncompleteFollowupFeatureChoice(state);
        }

        private string GetSelectedSubclassDisplayName(string classId)
        {
            if (string.IsNullOrWhiteSpace(classId))
            {
                return string.Empty;
            }

            for (int index = 0; index < m_featureChoiceStates.Count; index++)
            {
                CharacterCreationFeatureChoiceState state = m_featureChoiceStates[index];
                if (state == null
                    || !string.Equals(state.SourceType, "Class", StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(state.ClassId, classId, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(state.ChoiceType, "Subclass", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return CharacterCreationFeatureDisplayService.Instance.GetSelectedFeatureChoiceDisplayName(state);
            }

            return string.Empty;
        }

        private void RebuildFeatureChoiceStates()
        {
            DndClassDefineData classData = null;
            DndRaceDefineData raceData = null;
            TryGetSelectedClass(out classData);
            TryGetSelectedRace(out raceData);
            int level = ParseLevel(m_tmpInputLevel != null ? m_tmpInputLevel.text : string.Empty);
            CharacterCreationSessionService.Instance.RebuildFeatureChoiceStatesForCurrentSelection(classData, raceData, level);
        }

        private CharacterCreationFeatureChoiceState TryCreateFeatureChoiceState(CharacterCreationFeatureDisplayEntry entry)
        {
            if (entry == null)
            {
                return null;
            }

            return CharacterCreationSessionService.Instance.FindOrCreateFeatureChoiceState(
                entry.ChoiceGroupIds,
                entry.SourceType,
                entry.SourceId,
                entry.Level);
        }

        private CharacterCreationFeatureChoiceState FindFeatureChoiceState(string choiceGroupId)
        {
            return CharacterCreationSessionService.Instance.FindFeatureChoiceState(choiceGroupId);
        }

        private void RemoveFeatureChoiceStatesBySource(string sourceType)
        {
            CharacterCreationSessionService.Instance.RemoveFeatureChoiceStatesBySource(sourceType);
        }

        private static string NormalizeSkillId(string value)
        {
            string normalized = value?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return string.Empty;
            }

            normalized = normalized.Replace(" ", "_").Replace("-", "_").ToLowerInvariant();
            for (int index = 0; index < SkillDisplayBindings.Length; index++)
            {
                CharacterCreationSkillDisplayBinding binding = SkillDisplayBindings[index];
                if (string.Equals(normalized, binding.SkillId, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(value.Trim(), binding.DisplayName, StringComparison.OrdinalIgnoreCase))
                {
                    return binding.SkillId;
                }
            }

            return normalized;
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

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
            {
                target.SetActive(active);
            }
        }

        private static void RefreshLabelItems(
            List<CharacterCreationLabelItemView> itemViews,
            RectTransform content,
            GameObject template,
            IReadOnlyList<string> labels,
            string generatedNamePrefix,
            string labelChildName)
        {
            if (itemViews == null || content == null || template == null)
            {
                return;
            }

            int count = labels?.Count ?? 0;
            while (itemViews.Count < count)
            {
                GameObject itemObject = UnityEngine.Object.Instantiate(template, content);
                itemObject.name = $"{generatedNamePrefix}{itemViews.Count + 1}";
                itemObject.SetActive(true);
                itemViews.Add(CharacterCreationLabelItemView.Bind(itemObject, labelChildName));
            }

            if (template.activeSelf)
            {
                template.SetActive(false);
            }

            for (int index = 0; index < itemViews.Count; index++)
            {
                bool active = index < count;
                itemViews[index].SetActive(active);
                if (active)
                {
                    itemViews[index].SetLabel(labels[index]);
                }
            }
        }

        private void RefreshFeatureLabelItems(
            List<CharacterCreationLabelItemView> itemViews,
            RectTransform content,
            GameObject template,
            IReadOnlyList<CharacterCreationFeatureDisplayEntry> entries,
            string generatedNamePrefix,
            string labelChildName)
        {
            if (itemViews == null || content == null || template == null)
            {
                return;
            }

            int count = entries?.Count ?? 0;
            while (itemViews.Count < count)
            {
                GameObject itemObject = UnityEngine.Object.Instantiate(template, content);
                itemObject.name = $"{generatedNamePrefix}{itemViews.Count + 1}";
                itemObject.SetActive(true);
                itemViews.Add(CharacterCreationLabelItemView.Bind(itemObject, labelChildName));
            }

            if (template.activeSelf)
            {
                template.SetActive(false);
            }

            for (int index = 0; index < itemViews.Count; index++)
            {
                CharacterCreationLabelItemView itemView = itemViews[index];
                bool active = index < count;
                itemView.SetActive(active);
                if (!active)
                {
                    itemView.BindClick(null, false);
                    itemView.SetAlpha(SkillProficientAlpha);
                    continue;
                }

                CharacterCreationFeatureDisplayEntry entry = entries[index];
                itemView.SetLabel(CharacterCreationFeatureDisplayService.Instance.BuildFeatureEntryDisplayTitle(entry));
                itemView.SetAlpha(SkillProficientAlpha);
                CharacterCreationFeatureDisplayEntry capturedEntry = entry;
                itemView.BindClick(() => OnClickFeatureEntry(capturedEntry), true);
            }
        }

        private void OnClickFeatureEntry(CharacterCreationFeatureDisplayEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            ShowFeatureDetail(entry);
            CharacterCreationFeatureChoiceState state = TryGetNextPendingFeatureChoiceState(entry);
            if (state != null)
            {
                if (CharacterCreationSessionService.Instance.IsAbilityScoreFeatureChoice(state))
                {
                    RefreshCreationView();
                    HideRightPanelTemplates();
                    HideSelectionListOptions();
                }
                else
                {
                    ShowFeatureSelectionOptions(state);
                }
            }
            else
            {
                ClearSelectionList();
            }
        }

        private void ShowFeatureDetail(CharacterCreationFeatureDisplayEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            HideInventoryActionButtons();
            SetText(m_tmpFeatureDetailTitle, CharacterCreationFeatureDisplayService.Instance.BuildFeatureEntryDisplayTitle(entry));
            SetText(m_tmpFeatureDetailDescription, CharacterCreationFeatureDisplayService.Instance.BuildFeatureEntryDisplayDescription(entry));
        }

        private void ShowFeatureChoiceOptionDetail(string choiceGroupId, string optionId)
        {
            HideInventoryActionButtons();
            SetText(m_tmpFeatureDetailTitle, CharacterCreationFeatureDisplayService.Instance.GetChoiceOptionDetailTitle(choiceGroupId, optionId));
            SetText(m_tmpFeatureDetailDescription, CharacterCreationFeatureDisplayService.Instance.GetChoiceOptionDetailDescription(choiceGroupId, optionId));
        }

        private sealed class CharacterCreationStatusEffectItemView
        {
            private readonly GameObject m_root;
            private readonly TMP_Text m_nameText;
            private readonly TMP_Text m_durationText;
            private readonly Image m_iconImage;
            private readonly Sprite m_defaultIcon;

            private CharacterCreationStatusEffectItemView(GameObject root, TMP_Text nameText, TMP_Text durationText, Image iconImage)
            {
                m_root = root;
                m_nameText = nameText;
                m_durationText = durationText;
                m_iconImage = iconImage;
                m_defaultIcon = iconImage != null ? iconImage.sprite : null;
            }

            public static CharacterCreationStatusEffectItemView Bind(GameObject root)
            {
                TMP_Text nameText = null;
                TMP_Text durationText = null;
                Image iconImage = null;
                if (root != null)
                {
                    nameText = root.transform.Find("m_tmpStatusEffectName")?.GetComponent<TMP_Text>();
                    durationText = root.transform.Find("m_imgStatusEffectIcon/m_tmpStatusEffectDuration")?.GetComponent<TMP_Text>();
                    iconImage = root.transform.Find("m_imgStatusEffectIcon")?.GetComponent<Image>();
                }

                return new CharacterCreationStatusEffectItemView(root, nameText, durationText, iconImage);
            }

            public void Bind(CharacterStatusEffectDisplayEntry entry)
            {
                SetText(m_nameText, entry.Name);
                SetText(m_durationText, entry.Duration);
                CharacterCreationUI.SetActive(m_durationText != null ? m_durationText.gameObject : null, !string.IsNullOrWhiteSpace(entry.Duration));

                if (m_iconImage != null && m_iconImage.sprite == null && m_defaultIcon != null)
                {
                    m_iconImage.sprite = m_defaultIcon;
                }
            }

            public void SetActive(bool active)
            {
                CharacterCreationUI.SetActive(m_root, active);
            }
        }

        private sealed class CharacterCreationLabelItemView
        {
            private readonly GameObject m_root;
            private readonly Button m_button;
            private readonly CanvasGroup m_canvasGroup;
            private readonly TMP_Text m_label;

            private CharacterCreationLabelItemView(GameObject root, Button button, CanvasGroup canvasGroup, TMP_Text label)
            {
                m_root = root;
                m_button = button;
                m_canvasGroup = canvasGroup;
                m_label = label;
            }

            public static CharacterCreationLabelItemView Bind(GameObject root, string labelChildName)
            {
                Button button = null;
                CanvasGroup canvasGroup = null;
                TMP_Text label = null;
                if (root != null)
                {
                    button = root.GetComponent<Button>();
                    if (button == null)
                    {
                        button = root.AddComponent<Button>();
                    }

                    button.transition = Selectable.Transition.None;
                    canvasGroup = root.GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                    {
                        canvasGroup = root.AddComponent<CanvasGroup>();
                    }

                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                    Transform labelTransform = root.transform.Find(labelChildName);
                    label = labelTransform != null ? labelTransform.GetComponent<TMP_Text>() : null;
                }

                return new CharacterCreationLabelItemView(root, button, canvasGroup, label);
            }

            public void SetLabel(string value)
            {
                SetText(m_label, value);
            }

            public void BindClick(Action onClick, bool interactable)
            {
                if (m_button == null)
                {
                    return;
                }

                m_button.onClick.RemoveAllListeners();
                m_button.interactable = interactable;
                if (onClick != null)
                {
                    m_button.onClick.AddListener(() => onClick.Invoke());
                }
            }

            public void SetAlpha(float alpha)
            {
                if (m_canvasGroup != null)
                {
                    m_canvasGroup.alpha = Mathf.Clamp01(alpha);
                }
            }

            public void SetActive(bool active)
            {
                if (m_root != null && m_root.activeSelf != active)
                {
                    m_root.SetActive(active);
                }
            }
        }

        private sealed class CharacterCreationSelectionOptionCardView
        {
            private readonly GameObject m_root;
            private readonly Button m_button;
            private readonly GameObject m_selectedMark;
            private readonly TMP_Text m_nameText;

            public GameObject Root => m_root;

            private CharacterCreationSelectionOptionCardView(GameObject root, Button button, GameObject selectedMark, TMP_Text nameText)
            {
                m_root = root;
                m_button = button;
                m_selectedMark = selectedMark;
                m_nameText = nameText;
            }

            public static CharacterCreationSelectionOptionCardView Bind(GameObject root)
            {
                Button button = root != null ? root.GetComponent<Button>() : null;
                if (root != null && button == null)
                {
                    button = root.AddComponent<Button>();
                    button.transition = Selectable.Transition.None;
                    EnsureButtonRaycastTarget(button);
                }

                GameObject selectedMark = null;
                TMP_Text nameText = null;

                if (root != null)
                {
                    Transform[] children = root.GetComponentsInChildren<Transform>(true);
                    for (int index = 0; index < children.Length; index++)
                    {
                        Transform child = children[index];
                        if (child == null)
                        {
                            continue;
                        }

                        if (selectedMark == null
                            && (child.name == "\u9009\u4e2d\u72b6\u6001"
                                || child.name == "m_imgSelectedMark"
                                || child.name.IndexOf("Selected", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            selectedMark = child.gameObject;
                        }

                        if (nameText == null)
                        {
                            nameText = child.GetComponent<TMP_Text>();
                        }
                    }
                }

                return new CharacterCreationSelectionOptionCardView(root, button, selectedMark, nameText);
            }

            public void Bind(string label, bool selected, Action onClick)
            {
                SetText(m_nameText, label);
                CharacterCreationUI.SetActive(m_selectedMark, selected);

                if (m_button != null)
                {
                    m_button.onClick.RemoveAllListeners();
                    if (onClick != null)
                    {
                        m_button.onClick.AddListener(() => onClick.Invoke());
                    }
                }
            }

            public void SetActive(bool active)
            {
                if (m_root != null && m_root.activeSelf != active)
                {
                    m_root.SetActive(active);
                }
            }
        }

        private sealed class CharacterCreationItemInfoCardView
        {
            private readonly GameObject m_root;
            private readonly Button m_button;
            private readonly TMP_Text m_nameText;
            private readonly TMP_Text m_rarityText;
            private readonly TMP_Text m_typeText;
            private readonly Image m_iconImage;
            private readonly Button m_deleteButton;
            private readonly GameObject m_selectedIndicator;
            private readonly Sprite m_defaultIcon;
            private readonly Color m_defaultIconColor;
            private readonly CanvasGroup m_canvasGroup;

            private CharacterCreationItemInfoCardView(
                GameObject root,
                Button button,
                TMP_Text nameText,
                TMP_Text rarityText,
                TMP_Text typeText,
                Image iconImage,
                Button deleteButton,
                GameObject selectedIndicator,
                CanvasGroup canvasGroup)
            {
                m_root = root;
                m_button = button;
                m_nameText = nameText;
                m_rarityText = rarityText;
                m_typeText = typeText;
                m_iconImage = iconImage;
                m_deleteButton = deleteButton;
                m_selectedIndicator = selectedIndicator;
                m_canvasGroup = canvasGroup;
                m_defaultIcon = iconImage != null ? iconImage.sprite : null;
                m_defaultIconColor = iconImage != null ? iconImage.color : Color.white;
            }

            public static CharacterCreationItemInfoCardView Bind(GameObject root)
            {
                Button button = root != null ? root.GetComponent<Button>() : null;
                if (root != null && button == null)
                {
                    button = root.AddComponent<Button>();
                    button.transition = Selectable.Transition.None;
                    EnsureButtonRaycastTarget(button);
                }

                CanvasGroup canvasGroup = root != null ? root.GetComponent<CanvasGroup>() : null;
                if (root != null && canvasGroup == null)
                {
                    canvasGroup = root.AddComponent<CanvasGroup>();
                }

                TMP_Text nameText = root != null
                    ? root.transform.Find("m_panelCardName/m_tmpItemName")?.GetComponent<TMP_Text>()
                    : null;
                TMP_Text rarityText = root != null
                    ? root.transform.Find("m_tmpItemRare")?.GetComponent<TMP_Text>()
                    : null;
                TMP_Text typeText = root != null
                    ? root.transform.Find("m_tmpItemType")?.GetComponent<TMP_Text>()
                    : null;
                Image iconImage = root != null
                    ? root.transform.Find("m_imgItemIcon")?.GetComponent<Image>()
                    : null;
                Button deleteButton = root != null
                    ? root.transform.Find("m_btnDeleteItem")?.GetComponent<Button>()
                    : null;
                GameObject selectedIndicator = root != null
                    ? root.transform.Find("m_imgItemSelected")?.gameObject
                    : null;

                return new CharacterCreationItemInfoCardView(root, button, nameText, rarityText, typeText, iconImage, deleteButton, selectedIndicator, canvasGroup);
            }

            public void Bind(LocalCustomItemSaveData item, bool selected, Action onClick, Action onDelete)
            {
                CharacterEquipmentItemSaveData data = item?.Item;
                SetText(m_nameText, BuildLocalItemOptionLabel(item));
                SetText(m_rarityText, FormatItemRarity(data?.Rarity));
                SetText(m_typeText, FormatItemType(data?.ItemType));

                if (m_iconImage != null)
                {
                    m_iconImage.sprite = m_defaultIcon;
                    m_iconImage.color = m_defaultIcon != null ? m_defaultIconColor : new Color(1f, 1f, 1f, 0.35f);
                }

                if (m_canvasGroup != null)
                {
                    m_canvasGroup.alpha = selected ? 1f : 0.82f;
                }

                if (m_selectedIndicator != null && m_selectedIndicator.activeSelf != selected)
                {
                    m_selectedIndicator.SetActive(selected);
                }

                if (m_button != null)
                {
                    m_button.onClick.RemoveAllListeners();
                    if (onClick != null)
                    {
                        m_button.onClick.AddListener(() => onClick.Invoke());
                    }
                }

                if (m_deleteButton != null)
                {
                    m_deleteButton.onClick.RemoveAllListeners();
                    if (onDelete != null)
                    {
                        m_deleteButton.onClick.AddListener(() => onDelete.Invoke());
                    }
                }
            }

            public void SetActive(bool active)
            {
                if (m_root != null && m_root.activeSelf != active)
                {
                    m_root.SetActive(active);
                }
            }
        }

        private sealed class CharacterCreationSpellCardView
        {
            private readonly GameObject m_root;
            private readonly Button m_button;
            private readonly CanvasGroup m_canvasGroup;
            private readonly GameObject m_selectedMark;
            private readonly TMP_Text m_nameText;
            private readonly TMP_Text m_levelText;
            private readonly TMP_Text m_schoolText;
            private string m_spellId = string.Empty;

            private CharacterCreationSpellCardView(
                GameObject root,
                Button button,
                CanvasGroup canvasGroup,
                GameObject selectedMark,
                TMP_Text nameText,
                TMP_Text levelText,
                TMP_Text schoolText)
            {
                m_root = root;
                m_button = button;
                m_canvasGroup = canvasGroup;
                m_selectedMark = selectedMark;
                m_nameText = nameText;
                m_levelText = levelText;
                m_schoolText = schoolText;
            }

            public static CharacterCreationSpellCardView Bind(GameObject root)
            {
                Button button = root != null ? root.GetComponent<Button>() : null;
                if (root != null && button == null)
                {
                    button = root.AddComponent<Button>();
                    button.transition = Selectable.Transition.None;
                }

                CanvasGroup canvasGroup = root != null ? root.GetComponent<CanvasGroup>() : null;
                if (root != null && canvasGroup == null)
                {
                    canvasGroup = root.AddComponent<CanvasGroup>();
                }

                GameObject selectedMark = null;
                TMP_Text nameText = null;
                TMP_Text levelText = null;
                TMP_Text schoolText = null;
                if (root != null)
                {
                    Transform[] children = root.GetComponentsInChildren<Transform>(true);
                    for (int index = 0; index < children.Length; index++)
                    {
                        Transform child = children[index];
                        if (child == null)
                        {
                            continue;
                        }

                        if (selectedMark == null
                            && (child.name == "m_imgSpellSelected"
                                || child.name.IndexOf("Selected", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            selectedMark = child.gameObject;
                        }

                        TMP_Text text = child.GetComponent<TMP_Text>();
                        if (text == null)
                        {
                            continue;
                        }

                        if (nameText == null && (child.name == "m_tmpSpellName" || child.name == "m_tmpSpellTitle"))
                        {
                            nameText = text;
                        }
                        else if (levelText == null && child.name == "m_tmpSpellLevel")
                        {
                            levelText = text;
                        }
                        else if (schoolText == null && child.name == "m_tmpSpellSchool")
                        {
                            schoolText = text;
                        }
                    }
                }

                return new CharacterCreationSpellCardView(root, button, canvasGroup, selectedMark, nameText, levelText, schoolText);
            }

            public void Bind(CharacterCreationSpellCardViewState state, Action<string> onClick)
            {
                m_spellId = state?.SpellId ?? string.Empty;
                SetText(m_nameText, state?.Name ?? string.Empty);
                SetText(m_levelText, state?.LevelText ?? string.Empty);
                SetText(m_schoolText, state?.SchoolText ?? string.Empty);
                CharacterCreationUI.SetActive(m_selectedMark, state != null && (state.IsSelected || state.IsKnown || state.IsPrepared || state.IsAlwaysPrepared));
                if (m_canvasGroup != null)
                {
                    m_canvasGroup.alpha = state != null && state.IsPendingPreview ? 0.5f : 1f;
                    m_canvasGroup.interactable = true;
                    m_canvasGroup.blocksRaycasts = true;
                }

                if (m_button != null)
                {
                    m_button.onClick.RemoveAllListeners();
                    m_button.onClick.AddListener(() => onClick?.Invoke(m_spellId));
                }
            }

            public void SetActive(bool active)
            {
                if (m_root != null && m_root.activeSelf != active)
                {
                    m_root.SetActive(active);
                }
            }
        }

        private sealed class CharacterCreationClassCardView
        {
            private readonly GameObject m_root;
            private readonly Button m_button;
            private readonly GameObject m_selectedMark;
            private readonly TMP_Text m_classNameText;
            private string m_classId = string.Empty;

            private CharacterCreationClassCardView(GameObject root, Button button, GameObject selectedMark, TMP_Text classNameText)
            {
                m_root = root;
                m_button = button;
                m_selectedMark = selectedMark;
                m_classNameText = classNameText;
            }

            public static CharacterCreationClassCardView Bind(GameObject root)
            {
                Button button = root != null ? root.GetComponent<Button>() : null;
                GameObject selectedMark = root != null ? root.transform.Find("m_imgSelectedMark")?.gameObject : null;
                TMP_Text classNameText = root != null ? root.transform.Find("m_panelCardName/m_tmpClassName")?.GetComponent<TMP_Text>() : null;
                return new CharacterCreationClassCardView(root, button, selectedMark, classNameText);
            }

            public void Bind(string classId, string className, bool selected, Action<string> onClick)
            {
                m_classId = classId ?? string.Empty;
                SetText(m_classNameText, className);
                SetActive(m_selectedMark, selected);

                if (m_button != null)
                {
                    m_button.onClick.RemoveAllListeners();
                    m_button.onClick.AddListener(() => onClick?.Invoke(m_classId));
                }
            }

            public void SetActive(bool active)
            {
                SetActive(m_root, active);
            }

            private static void SetActive(GameObject target, bool active)
            {
                if (target != null && target.activeSelf != active)
                {
                    target.SetActive(active);
                }
            }
        }

        private sealed class CharacterCreationRaceCardView
        {
            private readonly GameObject m_root;
            private readonly Button m_button;
            private readonly GameObject m_selectedMark;
            private readonly TMP_Text m_raceNameText;
            private string m_raceId = string.Empty;

            private CharacterCreationRaceCardView(GameObject root, Button button, GameObject selectedMark, TMP_Text raceNameText)
            {
                m_root = root;
                m_button = button;
                m_selectedMark = selectedMark;
                m_raceNameText = raceNameText;
            }

            public static CharacterCreationRaceCardView Bind(GameObject root)
            {
                Button button = root != null ? root.GetComponent<Button>() : null;
                GameObject selectedMark = root != null ? root.transform.Find("m_imgRaceSelectedMark")?.gameObject : null;
                TMP_Text raceNameText = root != null ? root.transform.Find("m_panelCardName/m_tmpRaceName")?.GetComponent<TMP_Text>() : null;
                return new CharacterCreationRaceCardView(root, button, selectedMark, raceNameText);
            }

            public void Bind(string raceId, string raceName, bool selected, Action<string> onClick)
            {
                m_raceId = raceId ?? string.Empty;
                SetText(m_raceNameText, raceName);
                SetActive(m_selectedMark, selected);

                if (m_button != null)
                {
                    m_button.onClick.RemoveAllListeners();
                    m_button.onClick.AddListener(() => onClick?.Invoke(m_raceId));
                }
            }

            public void SetActive(bool active)
            {
                SetActive(m_root, active);
            }

            private static void SetActive(GameObject target, bool active)
            {
                if (target != null && target.activeSelf != active)
                {
                    target.SetActive(active);
                }
            }
        }

        private sealed class CharacterCreationBackgroundCardView
        {
            private readonly GameObject m_root;
            private readonly Button m_button;
            private readonly GameObject m_selectedMark;
            private readonly TMP_Text m_backgroundNameText;
            private string m_backgroundId = string.Empty;

            private CharacterCreationBackgroundCardView(GameObject root, Button button, GameObject selectedMark, TMP_Text backgroundNameText)
            {
                m_root = root;
                m_button = button;
                m_selectedMark = selectedMark;
                m_backgroundNameText = backgroundNameText;
            }

            public static CharacterCreationBackgroundCardView Bind(GameObject root)
            {
                Button button = root != null ? root.GetComponent<Button>() : null;
                GameObject selectedMark = root != null ? root.transform.Find("m_imgBackgroundSelectedMark")?.gameObject : null;
                TMP_Text backgroundNameText = root != null ? root.transform.Find("m_panelCardName/m_tmpBackgroundName")?.GetComponent<TMP_Text>() : null;
                return new CharacterCreationBackgroundCardView(root, button, selectedMark, backgroundNameText);
            }

            public void Bind(string backgroundId, string backgroundName, bool selected, Action<string> onClick)
            {
                m_backgroundId = backgroundId ?? string.Empty;
                SetText(m_backgroundNameText, backgroundName);
                SetActive(m_selectedMark, selected);

                if (m_button != null)
                {
                    m_button.onClick.RemoveAllListeners();
                    m_button.onClick.AddListener(() => onClick?.Invoke(m_backgroundId));
                }
            }

            public void SetActive(bool active)
            {
                SetActive(m_root, active);
            }

            private static void SetActive(GameObject target, bool active)
            {
                if (target != null && target.activeSelf != active)
                {
                    target.SetActive(active);
                }
            }
        }

        private sealed class CharacterCreationAlignmentCardView
        {
            private readonly GameObject m_root;
            private readonly Button m_button;
            private readonly GameObject m_selectedMark;
            private readonly TMP_Text m_alignmentNameText;
            private string m_alignmentId = string.Empty;

            private CharacterCreationAlignmentCardView(GameObject root, Button button, GameObject selectedMark, TMP_Text alignmentNameText)
            {
                m_root = root;
                m_button = button;
                m_selectedMark = selectedMark;
                m_alignmentNameText = alignmentNameText;
            }

            public static CharacterCreationAlignmentCardView Bind(GameObject root)
            {
                Button button = root != null ? root.GetComponent<Button>() : null;
                GameObject selectedMark = root != null ? root.transform.Find("m_imgAlignmentSelectedMark")?.gameObject : null;
                TMP_Text alignmentNameText = root != null ? root.transform.Find("m_panelCardName/m_tmpAlignmentName")?.GetComponent<TMP_Text>() : null;
                return new CharacterCreationAlignmentCardView(root, button, selectedMark, alignmentNameText);
            }

            public void Bind(string alignmentId, string alignmentName, bool selected, Action<string> onClick)
            {
                m_alignmentId = alignmentId ?? string.Empty;
                SetText(m_alignmentNameText, alignmentName);
                SetActive(m_selectedMark, selected);

                if (m_button != null)
                {
                    m_button.onClick.RemoveAllListeners();
                    m_button.onClick.AddListener(() => onClick?.Invoke(m_alignmentId));
                }
            }

            public void SetActive(bool active)
            {
                SetActive(m_root, active);
            }

            private static void SetActive(GameObject target, bool active)
            {
                if (target != null && target.activeSelf != active)
                {
                    target.SetActive(active);
                }
            }
        }
    }
}
