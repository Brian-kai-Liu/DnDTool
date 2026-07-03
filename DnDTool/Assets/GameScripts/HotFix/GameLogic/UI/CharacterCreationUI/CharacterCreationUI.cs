using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using TMPro;
using TEngine;
using UnityEngine;
using UnityEngine.UI;
using Log = TEngine.Log;

namespace GameLogic
{
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
    internal sealed class CharacterCreationUI : UIWindow
    {
        private Button m_btnBack;
        private Button m_btnConfirmDraft;
        private Button m_btnSave;
        private Button m_btnPanelClass;
        private Button m_btnPanelRace;
        private Button m_btnPanelBackground;
        private Button m_btnPanelAlignment;
        private Button m_btnAbilityGeneration;
        private Button m_btnGenerateHitPoints;
        private Button m_btnSectionInventory;
        private Button m_btnSectionSkills;
        private Button m_btnSectionEquipmentTools;
        private Button m_btnSectionClass;
        private Button m_btnSectionRace;
        private Button m_btnSectionOtherFeatures;
        private Button m_btnAddInventoryItem;
        private Button m_btnAddCustomFeature;
        private Button m_btnCloseCustomFeature;
        private Button m_btnCancelCustomFeature;
        private Button m_btnConfirmCustomFeature;
        private Button m_btnSectionSpells;
        private readonly Button[] m_btnSpellFilters = new Button[11];
        private Button m_btnStrengthIncrease;
        private Button m_btnStrengthDecrease;
        private Button m_btnDexterityIncrease;
        private Button m_btnDexterityDecrease;
        private Button m_btnConstitutionIncrease;
        private Button m_btnConstitutionDecrease;
        private Button m_btnIntelligenceIncrease;
        private Button m_btnIntelligenceDecrease;
        private Button m_btnWisdomIncrease;
        private Button m_btnWisdomDecrease;
        private Button m_btnCharismaIncrease;
        private Button m_btnCharismaDecrease;
        private Button m_btnPortraitUpload;
        private Image m_imgPortraitUpload;
        private TMP_Text m_tmpPortraitUploadHint;
        private RectTransform m_rectInfoListContent;
        private RectTransform m_rectSelectionListContent;
        private GameObject m_goClassTemplate;
        private GameObject m_goRaceTemplate;
        private GameObject m_goBackgroundTemplate;
        private GameObject m_goAlignmentTemplate;
        private RectTransform m_rectEquipmentToolContent;
        private GameObject m_goEquipmentToolTemplate;
        private RectTransform m_rectClassFeatureContent;
        private GameObject m_goClassFeatureTemplate;
        private RectTransform m_rectRaceFeatureContent;
        private GameObject m_goRaceFeatureTemplate;
        private RectTransform m_rectSkillProficiencies;
        private RectTransform m_rectInventoryContent;
        private RectTransform m_rectOtherFeatureContent;
        private GameObject m_goOtherFeatureTemplate;
        private RectTransform m_rectStatusEffectContent;
        private RectTransform m_rectSpellContent;
        private GameObject m_goStatusEffectTemplate;
        private GameObject m_goSpellTemplate;
        private GameObject m_goSpellInfoTemplate;
        private GameObject m_goLearnedSpellTemplate;
        private TMP_Text m_tmpHitDiceDie;
        private TMP_Text m_tmpHitDiceRemaining;
        private TMP_Text m_tmpCurrentHp;
        private TMP_Text m_tmpMaxHp;
        private TMP_Text m_tmpTempHp;
        private TMP_Text m_tmpCopper;
        private TMP_Text m_tmpSilver;
        private TMP_Text m_tmpElectrum;
        private TMP_Text m_tmpGold;
        private TMP_Text m_tmpPlatinum;
        private Slider m_sliderExperience;
        private TMP_Text m_tmpExperienceValue;
        private TMP_Text m_tmpSpeed;
        private TMP_Text m_tmpAc;
        private TMP_Text m_tmpInitiative;
        private TMP_Text m_tmpProficiencyBonus;
        private TMP_Text m_tmpPassivePerception;
        private TMP_Text m_tmpDc;
        private TMP_Text m_tmpSpellAttackBonus;
        private TMP_Text m_tmpDetailClass;
        private TMP_Text m_tmpDetailRace;
        private TMP_Text m_tmpDetailBackground;
        private TMP_Text m_tmpDetailAlignment;
        private TMP_Text m_tmpSkillsLabel;
        private TMP_Text m_tmpEquipmentToolsLabel;
        private TMP_Text m_tmpClassFeatureClassName;
        private TMP_Text m_tmpClassFeatureSubclassName;
        private TMP_Text m_tmpClassLevel;
        private TMP_Text m_tmpRaceFeatureRaceName;
        private TMP_Text m_tmpRaceFeatureSubRaceName;
        private TMP_Text m_tmpFeatureDetailTitle;
        private TMP_Text m_tmpFeatureDetailDescription;
        private TMP_Text m_tmpStrength;
        private TMP_Text m_tmpDexterity;
        private TMP_Text m_tmpConstitution;
        private TMP_Text m_tmpIntelligence;
        private TMP_Text m_tmpWisdom;
        private TMP_Text m_tmpCharisma;
        private TMP_Text m_tmpStrengthModifier;
        private TMP_Text m_tmpDexterityModifier;
        private TMP_Text m_tmpConstitutionModifier;
        private TMP_Text m_tmpIntelligenceModifier;
        private TMP_Text m_tmpWisdomModifier;
        private TMP_Text m_tmpCharismaModifier;
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
        private TMP_InputField m_inputCharacterName;
        private TMP_InputField m_inputLevel;
        private TMP_InputField m_inputPersonalityTraits;
        private TMP_InputField m_inputIdeals;
        private TMP_InputField m_inputBonds;
        private TMP_InputField m_inputFlaws;
        private GameObject m_goCustomFeaturePopup;
        private TMP_InputField m_inputCustomFeatureName;
        private TMP_InputField m_inputCustomFeatureDescription;
        private bool m_isUpdatingLevelInput;
        private string m_pendingAbilityGenerationMethodId = string.Empty;
        private string m_pendingHitPointGenerationMethodId = string.Empty;
        private Texture2D m_portraitTexture;
        private Sprite m_portraitSprite;
        private string m_previewImagePath = string.Empty;
        private int m_portraitLoadVersion;
        private CharacterCreationFeatureChoiceState m_visibleFeatureChoiceState;

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
        private readonly List<CharacterCreationLabelItemView> m_classFeatureItems = new List<CharacterCreationLabelItemView>();
        private readonly List<CharacterCreationLabelItemView> m_raceFeatureItems = new List<CharacterCreationLabelItemView>();
        private readonly List<CharacterCreationLabelItemView> m_otherFeatureItems = new List<CharacterCreationLabelItemView>();
        private readonly List<CharacterCreationStatusEffectItemView> m_statusEffectItems = new List<CharacterCreationStatusEffectItemView>();
        private readonly List<CharacterCreationSpellCardView> m_availableSpellCards = new List<CharacterCreationSpellCardView>();
        private readonly List<CharacterCreationSpellCardView> m_learnedSpellCards = new List<CharacterCreationSpellCardView>();
        private readonly Dictionary<string, string> m_toolChoiceGroupIdByLabel = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> m_toolChoiceSourceTypeByLabel = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> m_toolChoiceSourceIdByLabel = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private List<CharacterCreationSkillChoiceState> m_skillChoiceStates => CharacterCreationSessionService.Instance.SkillChoiceStates;
        private List<CharacterCreationToolChoiceState> m_toolChoiceStates => CharacterCreationSessionService.Instance.ToolChoiceStates;
        private List<CharacterCreationFeatureChoiceState> m_featureChoiceStates => CharacterCreationSessionService.Instance.FeatureChoiceStates;
        private int m_activeSpellFilterLevel = -1;
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

        protected override void ScriptGenerator()
        {
            CharacterCreationSessionService.Instance.BeginNewDraft();
            BindControls();
            BindButtons();
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

        private void BindControls()
        {
            m_btnBack = FindChildComponent<Button>("m_btnBack");
            m_btnConfirmDraft = FindChildComponent<Button>("m_btnComfirmDraft");
            m_btnSave = FindChildComponent<Button>("m_btnSave");
            m_btnPanelClass = FindChildComponent<Button>("m_panelClass");
            m_btnPanelRace = FindChildComponent<Button>("m_panelRace");
            m_btnPanelBackground = FindChildComponent<Button>("m_panelBackground");
            m_btnPanelAlignment = FindChildComponent<Button>("m_panelAlignment");
            m_btnAbilityGeneration = FindChildComponent<Button>("m_btnAbilityGeneration");
            m_btnGenerateHitPoints = FindChildComponent<Button>("m_btnGenerateHitPoints");
            m_btnSectionInventory = FindChildButton("m_sectionInventory");
            m_btnSectionSkills = FindChildButton("m_sectionSkills");
            m_btnSectionEquipmentTools = FindChildButton("m_sectionEquipmentTools");
            m_btnSectionClass = FindChildButton("m_sectionClass");
            m_btnSectionRace = FindChildButton("m_sectionRace");
            m_btnSectionOtherFeatures = FindChildButton("m_sectionOtherFeatures");
            m_btnAddInventoryItem = FindChildButtonInParent("m_panelInventoryHeader", "Button");
            m_btnAddCustomFeature = FindChildButtonInParent("m_panelOtherFeatureHeader", "Button");
            m_btnCloseCustomFeature = FindChildComponent<Button>("m_btnCloseCustomFeature");
            m_btnCancelCustomFeature = FindChildComponent<Button>("m_btnCancelCustomFeature");
            m_btnConfirmCustomFeature = FindChildComponent<Button>("m_btnConfirmCustomFeature");
            m_btnSectionSpells = FindChildButton("m_sectionSpells");
            BindSpellFilterControls();
            m_btnStrengthIncrease = FindChildComponent<Button>("m_btnStrengthIncrease");
            m_btnStrengthDecrease = FindChildComponent<Button>("m_btnStrengthDecrease");
            m_btnDexterityIncrease = FindChildComponent<Button>("m_btnDexterityIncrease");
            m_btnDexterityDecrease = FindChildComponent<Button>("m_btnDexterityDecrease");
            m_btnConstitutionIncrease = FindChildComponent<Button>("m_btnConstitutionIncrease");
            m_btnConstitutionDecrease = FindChildComponent<Button>("m_btnConstitutionDecrease");
            m_btnIntelligenceIncrease = FindChildComponent<Button>("m_btnIntelligenceIncrease");
            m_btnIntelligenceDecrease = FindChildComponent<Button>("m_btnIntelligenceDecrease");
            m_btnWisdomIncrease = FindChildComponent<Button>("m_btnWisdomIncrease");
            m_btnWisdomDecrease = FindChildComponent<Button>("m_btnWisdomDecrease");
            m_btnCharismaIncrease = FindChildComponent<Button>("m_btnCharismaIncrease");
            m_btnCharismaDecrease = FindChildComponent<Button>("m_btnCharismaDecrease");
            m_btnPortraitUpload = FindChildComponent<Button>("m_imgCharacterPortraitUploadArea");
            m_imgPortraitUpload = FindChildComponent<Image>("m_imgCharacterPortraitUploadArea");
            m_tmpPortraitUploadHint = FindChildComponent<TMP_Text>("m_tmpCharacterPortraitUploadHint");
            if (m_imgPortraitUpload != null)
            {
                m_imgPortraitUpload.preserveAspect = true;
            }

            m_rectInfoListContent = FindScrollContent("m_scrollInfoList");
            m_rectSelectionListContent = FindScrollContent("m_scrollSelectionList");
            m_goClassTemplate = FindChildComponent<RectTransform>("m_itemClassTemplate")?.gameObject;
            m_goRaceTemplate = FindChildComponent<RectTransform>("m_itemRaceTemplate")?.gameObject;
            m_goBackgroundTemplate = FindChildComponent<RectTransform>("m_itemBackgroundTemplate")?.gameObject;
            m_goAlignmentTemplate = FindChildComponent<RectTransform>("m_itemAlignmentTemplate")?.gameObject;
            m_rectEquipmentToolContent = FindChildComponent<RectTransform>("m_rectEquipmentToolContent");
            m_goEquipmentToolTemplate = FindChildComponent<RectTransform>("m_itemEquipmentToolTemplate")?.gameObject;
            m_rectClassFeatureContent = FindChildComponent<RectTransform>("m_rectClassFeatureContent");
            m_goClassFeatureTemplate = FindChildComponent<RectTransform>("m_itemClassFeatureTemplate")?.gameObject;
            m_rectRaceFeatureContent = FindChildComponent<RectTransform>("m_rectRaceFeatureContent");
            m_goRaceFeatureTemplate = FindChildComponent<RectTransform>("m_itemRaceFeatureTemplate")?.gameObject;
            m_rectSkillProficiencies = FindChildComponent<RectTransform>("m_gridSkillProficiencies");
            m_rectInventoryContent = FindChildComponent<RectTransform>("m_rectInventoryContent");
            m_rectOtherFeatureContent = FindChildComponent<RectTransform>("m_rectOtherFeatureContent");
            m_goOtherFeatureTemplate = FindChildComponent<RectTransform>("m_itemOtherFeatureTemplate")?.gameObject;
            m_rectStatusEffectContent = FindChildComponent<RectTransform>("m_gridStatusEffects");
            m_rectSpellContent = FindChildComponent<RectTransform>("m_rectSpellContent");
            m_goStatusEffectTemplate = FindChildComponent<RectTransform>("m_itemStatusEffectTemplate")?.gameObject;
            m_goSpellTemplate = FindChildComponent<RectTransform>("m_itemSpellTemplate")?.gameObject;
            m_goSpellInfoTemplate = FindChildComponentInChildren<RectTransform>(m_rectInfoListContent, "m_itemSpellInfoTemplate")?.gameObject
                ?? FindChildComponent<RectTransform>("m_itemSpellInfoTemplate")?.gameObject;
            m_goLearnedSpellTemplate = FindChildComponentInChildren<RectTransform>(m_rectSpellContent, "m_itemSpellInfoTemplate")?.gameObject
                ?? m_goSpellTemplate;
            BindSkillItems();
            BindAbilityItems();
            m_tmpHitDiceDie = FindChildComponent<TMP_Text>("m_tmpHitDiceDie");
            m_tmpHitDiceRemaining = FindChildComponent<TMP_Text>("m_tmpHitDiceRemaining");
            m_tmpCurrentHp = FindChildComponent<TMP_Text>("m_tmpCurrentHp");
            m_tmpMaxHp = FindChildComponent<TMP_Text>("m_tmpMaxHp");
            m_tmpTempHp = FindChildComponent<TMP_Text>("m_tmpTempHp");
            m_tmpCopper = FindChildComponent<TMP_Text>("m_tmpCopper");
            m_tmpSilver = FindChildComponent<TMP_Text>("m_tmpSilver");
            m_tmpElectrum = FindChildComponent<TMP_Text>("m_tmpElectrum");
            m_tmpGold = FindChildComponent<TMP_Text>("m_tmpGold");
            m_tmpPlatinum = FindChildComponent<TMP_Text>("m_tmpPlatinum");
            m_sliderExperience = FindChildComponent<Slider>("m_sliderExperience");
            m_tmpExperienceValue = FindChildComponent<TMP_Text>("m_tmpExperienceValue");
            m_tmpSpeed = FindChildComponent<TMP_Text>("m_tmpSpeed");
            m_tmpAc = FindChildComponent<TMP_Text>("m_tmpAc");
            m_tmpInitiative = FindChildComponent<TMP_Text>("m_tmpInitiative");
            m_tmpProficiencyBonus = FindChildComponent<TMP_Text>("m_tmpProficiencyBonus");
            m_tmpPassivePerception = FindChildComponent<TMP_Text>("m_tmpPassivePerception");
            m_tmpDc = FindChildComponent<TMP_Text>("m_tmpDc");
            m_tmpSpellAttackBonus = FindChildComponent<TMP_Text>("m_tmpSpellAttackBonus");
            m_tmpDetailClass = FindChildComponent<TMP_Text>("m_tmpDetailClass");
            m_tmpDetailRace = FindChildComponent<TMP_Text>("m_tmpDetailRace");
            m_tmpDetailBackground = FindChildComponent<TMP_Text>("m_tmpDetailBackground");
            m_tmpDetailAlignment = FindChildComponent<TMP_Text>("m_tmpDetailAlignment");
            m_tmpSkillsLabel = FindChildComponent<TMP_Text>("m_tmpSkillsLabel");
            m_tmpEquipmentToolsLabel = FindChildComponent<TMP_Text>("m_tmpEquipmentToolsLabel");
            m_tmpClassFeatureClassName = FindChildComponent<TMP_Text>("m_tmpClassFeatureClassName");
            m_tmpClassFeatureSubclassName = FindChildComponent<TMP_Text>("m_tmpClassFeatureSubclassName");
            m_tmpClassLevel = FindChildComponent<TMP_Text>("m_tmpClassLevel");
            m_tmpRaceFeatureRaceName = FindChildComponent<TMP_Text>("m_tmpRaceFeatureRaceName");
            m_tmpRaceFeatureSubRaceName = FindChildComponent<TMP_Text>("m_tmpRaceFeatureSubRaceName");
            m_tmpFeatureDetailTitle = FindChildComponent<TMP_Text>("m_tmpFeatureDetailTitle");
            m_tmpFeatureDetailDescription = FindChildComponent<TMP_Text>("m_tmpFeatureDetailDescription");
            m_tmpStrength = FindChildComponent<TMP_Text>("m_tmpStrength");
            m_tmpDexterity = FindChildComponent<TMP_Text>("m_tmpDexterity");
            m_tmpConstitution = FindChildComponent<TMP_Text>("m_tmpConstitution");
            m_tmpIntelligence = FindChildComponent<TMP_Text>("m_tmpIntelligence");
            m_tmpWisdom = FindChildComponent<TMP_Text>("m_tmpWisdom");
            m_tmpCharisma = FindChildComponent<TMP_Text>("m_tmpCharisma");
            m_tmpStrengthModifier = FindChildComponent<TMP_Text>("m_tmpStrengthModifier");
            m_tmpDexterityModifier = FindChildComponent<TMP_Text>("m_tmpDexterityModifier");
            m_tmpConstitutionModifier = FindChildComponent<TMP_Text>("m_tmpConstitutionModifier");
            m_tmpIntelligenceModifier = FindChildComponent<TMP_Text>("m_tmpIntelligenceModifier");
            m_tmpWisdomModifier = FindChildComponent<TMP_Text>("m_tmpWisdomModifier");
            m_tmpCharismaModifier = FindChildComponent<TMP_Text>("m_tmpCharismaModifier");
            BindAbilityScoreInputs();
            m_inputCharacterName = FindChildComponent<TMP_InputField>("m_inputCharacterName");
            m_inputLevel = FindChildComponent<TMP_InputField>("m_tmpLevel");
            m_inputPersonalityTraits = FindChildComponent<TMP_InputField>("m_tmpPersonalityTraits");
            m_inputIdeals = FindChildComponent<TMP_InputField>("m_tmpIdeals");
            m_inputBonds = FindChildComponent<TMP_InputField>("m_tmpBonds");
            m_inputFlaws = FindChildComponent<TMP_InputField>("m_tmpFlaws");
            m_goCustomFeaturePopup = FindChildComponent<RectTransform>("m_popupCustomFeature")?.gameObject;
            m_inputCustomFeatureName = FindChildComponent<TMP_InputField>("m_inputCustomFeatureName");
            m_inputCustomFeatureDescription = FindChildComponent<TMP_InputField>("m_inputCustomFeatureDescription");
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

        private void BindButtons()
        {
            BindButton(m_btnBack, ReturnToCharacterManagement);
            BindButton(m_btnSave, SaveDraft);
            BindButton(m_btnConfirmDraft, ConfirmCurrentRightPanelSelection);
            BindButton(m_btnPanelClass, ShowClassOptions);
            BindButton(m_btnPanelRace, ShowRaceOptions);
            BindButton(m_btnPanelBackground, ShowBackgroundOptions);
            BindButton(m_btnPanelAlignment, ShowAlignmentOptions);
            BindButton(m_btnAbilityGeneration, OnClickAbilityGenerationButton);
            BindButton(m_btnGenerateHitPoints, OnClickHitPointGenerationButton);
            BindButton(m_btnPortraitUpload, () => UploadPortraitImageAsync().Forget());
            BindButton(m_btnSectionInventory, () => ToggleRectActive(m_rectInventoryContent));
            BindButton(m_btnSectionSkills, () => ToggleRectActive(m_rectSkillProficiencies));
            BindButton(m_btnSectionEquipmentTools, () => ToggleRectActive(m_rectEquipmentToolContent));
            BindButton(m_btnSectionClass, () => ToggleRectActive(m_rectClassFeatureContent));
            BindButton(m_btnSectionRace, () => ToggleRectActive(m_rectRaceFeatureContent));
            BindButton(m_btnSectionOtherFeatures, () => ToggleRectActive(m_rectOtherFeatureContent));
            BindButton(m_btnAddInventoryItem, OpenItemInfoEditor);
            BindButton(m_btnAddCustomFeature, OpenCustomFeaturePopup);
            BindButton(m_btnCloseCustomFeature, CloseCustomFeaturePopup);
            BindButton(m_btnCancelCustomFeature, CloseCustomFeaturePopup);
            BindButton(m_btnConfirmCustomFeature, ConfirmCustomFeaturePopup);
            BindButton(m_btnSectionSpells, OnClickSpellSection);
            BindButton(m_btnStrengthIncrease, () => ChangeAbilityScore("Strength", 1));
            BindButton(m_btnStrengthDecrease, () => ChangeAbilityScore("Strength", -1));
            BindButton(m_btnDexterityIncrease, () => ChangeAbilityScore("Dexterity", 1));
            BindButton(m_btnDexterityDecrease, () => ChangeAbilityScore("Dexterity", -1));
            BindButton(m_btnConstitutionIncrease, () => ChangeAbilityScore("Constitution", 1));
            BindButton(m_btnConstitutionDecrease, () => ChangeAbilityScore("Constitution", -1));
            BindButton(m_btnIntelligenceIncrease, () => ChangeAbilityScore("Intelligence", 1));
            BindButton(m_btnIntelligenceDecrease, () => ChangeAbilityScore("Intelligence", -1));
            BindButton(m_btnWisdomIncrease, () => ChangeAbilityScore("Wisdom", 1));
            BindButton(m_btnWisdomDecrease, () => ChangeAbilityScore("Wisdom", -1));
            BindButton(m_btnCharismaIncrease, () => ChangeAbilityScore("Charisma", 1));
            BindButton(m_btnCharismaDecrease, () => ChangeAbilityScore("Charisma", -1));
            BindSpellFilterButtons();
        }

        private void BindSpellFilterButtons()
        {
            BindButton(m_btnSpellFilters[0], () => ShowSpellOptions(-1));
            BindButton(m_btnSpellFilters[1], () => ShowSpellOptions(0));
            for (int level = 1; level <= 9; level++)
            {
                int capturedLevel = level;
                BindButton(m_btnSpellFilters[level + 1], () => ShowSpellOptions(capturedLevel));
            }
        }

        private void OpenCustomFeaturePopup()
        {
            if (m_inputCustomFeatureName != null)
            {
                m_inputCustomFeatureName.SetTextWithoutNotify(string.Empty);
            }

            if (m_inputCustomFeatureDescription != null)
            {
                m_inputCustomFeatureDescription.SetTextWithoutNotify(string.Empty);
            }

            SetActive(m_goCustomFeaturePopup, true);
            if (m_inputCustomFeatureName != null)
            {
                m_inputCustomFeatureName.Select();
                m_inputCustomFeatureName.ActivateInputField();
            }
        }

        private void CloseCustomFeaturePopup()
        {
            SetActive(m_goCustomFeaturePopup, false);
        }

        private void ConfirmCustomFeaturePopup()
        {
            string featureName = m_inputCustomFeatureName != null ? m_inputCustomFeatureName.text : string.Empty;
            string description = m_inputCustomFeatureDescription != null ? m_inputCustomFeatureDescription.text : string.Empty;
            if (!CharacterCreationSessionService.Instance.AddCustomFeature(featureName, description))
            {
                Log.Warning("自定义特性名称与描述不能同时为空。");
                return;
            }

            CloseCustomFeaturePopup();
            SetActive(m_rectOtherFeatureContent != null ? m_rectOtherFeatureContent.gameObject : null, true);
            RefreshCreationView();
        }

        private void OpenItemInfoEditor()
        {
            GameModule.UI.ShowUIAsync<ItemInfoEditorUI>(ItemInfoEditorUIRequest.FromCharacterCreation());
        }

        private void BindLevelInput()
        {
            if (m_inputLevel == null)
            {
                return;
            }

            m_inputLevel.contentType = TMP_InputField.ContentType.IntegerNumber;
            m_inputLevel.lineType = TMP_InputField.LineType.SingleLine;
            m_inputLevel.characterLimit = 2;
            if (string.IsNullOrWhiteSpace(m_inputLevel.text))
            {
                m_inputLevel.SetTextWithoutNotify(MinCharacterLevel.ToString());
            }

            m_inputLevel.onValueChanged.RemoveAllListeners();
            m_inputLevel.onEndEdit.RemoveAllListeners();
            m_inputLevel.onValueChanged.AddListener(OnLevelInputChanged);
            m_inputLevel.onEndEdit.AddListener(OnLevelInputEndEdit);
        }

        private void BindRoleplayInputs()
        {
            ConfigureRoleplayInput(m_inputPersonalityTraits);
            ConfigureRoleplayInput(m_inputIdeals);
            ConfigureRoleplayInput(m_inputBonds);
            ConfigureRoleplayInput(m_inputFlaws);
        }

        private void BindCustomFeatureInputs()
        {
            ConfigureRoleplayInput(m_inputCustomFeatureName);
            ConfigureRoleplayInput(m_inputCustomFeatureDescription);
            if (m_inputCustomFeatureName != null)
            {
                m_inputCustomFeatureName.lineType = TMP_InputField.LineType.SingleLine;
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
            if (m_isUpdatingLevelInput || m_inputLevel == null)
            {
                return;
            }

            string digits = KeepDigitsOnly(value);
            if (digits == value)
            {
                return;
            }

            m_isUpdatingLevelInput = true;
            m_inputLevel.SetTextWithoutNotify(digits);
            m_isUpdatingLevelInput = false;
        }

        private void OnLevelInputEndEdit(string value)
        {
            if (m_inputLevel == null)
            {
                return;
            }

            int level = ParseLevel(value);
            m_inputLevel.SetTextWithoutNotify(level.ToString());
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
            int level = ParseLevel(m_inputLevel != null ? m_inputLevel.text : string.Empty);
            CharacterCreationSpellbookViewState spellbook = CharacterCreationSpellDisplayService.Instance.BuildSpellbook(character, level, filterLevel);
            RefreshSpellCardItems(
                m_availableSpellCards,
                m_rectInfoListContent,
                m_goSpellInfoTemplate,
                spellbook.AvailableSpells,
                "m_itemSpellOption_",
                OnClickAvailableSpellCard);
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

        private void ConfirmCurrentRightPanelSelection()
        {
            if (!string.IsNullOrWhiteSpace(m_pendingHitPointGenerationMethodId))
            {
                int level = ParseLevel(m_inputLevel != null ? m_inputLevel.text : string.Empty);
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
            int level = ParseLevel(m_inputLevel != null ? m_inputLevel.text : string.Empty);
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
            CharacterCreationViewState state = CharacterCreationViewStateService.Instance.BuildCurrentViewState(ParseLevel(m_inputLevel != null ? m_inputLevel.text : string.Empty));
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
                if (!isGeneratedClassCard && !isGeneratedRaceCard && !isGeneratedBackgroundCard && !isGeneratedAlignmentCard && !isGeneratedSpellCard)
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
            int level = ParseLevel(m_inputLevel != null ? m_inputLevel.text : string.Empty);
            CharacterCreationFormInput form = new CharacterCreationFormInput
            {
                CharacterName = m_inputCharacterName != null ? m_inputCharacterName.text.Trim() : string.Empty,
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
            input.PersonalityTraits = m_inputPersonalityTraits != null ? m_inputPersonalityTraits.text.Trim() : string.Empty;
            input.Ideals = m_inputIdeals != null ? m_inputIdeals.text.Trim() : string.Empty;
            input.Bonds = m_inputBonds != null ? m_inputBonds.text.Trim() : string.Empty;
            input.Flaws = m_inputFlaws != null ? m_inputFlaws.text.Trim() : string.Empty;
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

        private static void BindButton(Button button, Action action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action?.Invoke());
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
            int level = ParseLevel(m_inputLevel != null ? m_inputLevel.text : string.Empty);
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

            SetText(m_tmpFeatureDetailTitle, CharacterCreationFeatureDisplayService.Instance.BuildFeatureEntryDisplayTitle(entry));
            SetText(m_tmpFeatureDetailDescription, CharacterCreationFeatureDisplayService.Instance.BuildFeatureEntryDisplayDescription(entry));
        }

        private void ShowFeatureChoiceOptionDetail(string choiceGroupId, string optionId)
        {
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
