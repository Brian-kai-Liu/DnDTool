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
    internal readonly struct SkillDisplayBinding
    {
        public readonly string SkillId;
        public readonly string DisplayName;
        public readonly AbilityKind Ability;

        public SkillDisplayBinding(string skillId, string displayName, AbilityKind ability)
        {
            SkillId = skillId;
            DisplayName = displayName;
            Ability = ability;
        }
    }

    internal enum CharacterCardPopupMode
    {
        None,
        Info,
        OptionList,
        Form
    }

    [Window(UILayer.UI, location: "CharacterCardManagementUI", fullScreen: true)]
    internal sealed class CharacterCardManagementUI : UIWindow
    {
        private static readonly Color SkillProficiencyBackgroundColor = new Color(0.28f, 0.46f, 0.74f, 0.85f);
        private static readonly Color SkillExpertiseBackgroundColor = new Color(0.55f, 0.42f, 0.18f, 0.9f);
        private static readonly SkillDisplayBinding[] SkillDisplayBindings =
        {
            new SkillDisplayBinding("athletics", "运动", AbilityKind.Strength),
            new SkillDisplayBinding("acrobatics", "体操", AbilityKind.Dexterity),
            new SkillDisplayBinding("sleight_of_hand", "巧手", AbilityKind.Dexterity),
            new SkillDisplayBinding("stealth", "隐匿", AbilityKind.Dexterity),
            new SkillDisplayBinding("arcana", "奥秘", AbilityKind.Intelligence),
            new SkillDisplayBinding("history", "历史", AbilityKind.Intelligence),
            new SkillDisplayBinding("investigation", "调查", AbilityKind.Intelligence),
            new SkillDisplayBinding("nature", "自然", AbilityKind.Intelligence),
            new SkillDisplayBinding("religion", "宗教", AbilityKind.Intelligence),
            new SkillDisplayBinding("animal_handling", "驯兽", AbilityKind.Wisdom),
            new SkillDisplayBinding("insight", "洞悉", AbilityKind.Wisdom),
            new SkillDisplayBinding("medicine", "医药", AbilityKind.Wisdom),
            new SkillDisplayBinding("perception", "察觉", AbilityKind.Wisdom),
            new SkillDisplayBinding("survival", "求生", AbilityKind.Wisdom),
            new SkillDisplayBinding("deception", "欺瞒", AbilityKind.Charisma),
            new SkillDisplayBinding("intimidation", "威吓", AbilityKind.Charisma),
            new SkillDisplayBinding("performance", "表演", AbilityKind.Charisma),
            new SkillDisplayBinding("persuasion", "游说", AbilityKind.Charisma)
        };

        private static readonly string[] SubclassChoiceFeatureIdMarkers =
        {
            "primal_path",
            "bard_college",
            "divine_domain",
            "druid_circle",
            "martial_archetype",
            "monastic_tradition",
            "sacred_oath",
            "ranger_archetype",
            "roguish_archetype",
            "sorcerous_origin",
            "otherworldly_patron",
            "arcane_tradition"
        };

        private UIBindComponent m_bindComponent;
        private Button m_btnBack;
        private Button m_btnCreateCharacter;
        private Button m_btnDeleteSelected;
        private TMP_Text m_tmpTitle;
        private TMP_Text m_tmpCreateCharacterText;
        private TMP_Text m_tmpDeleteSelectedText;
        private RectTransform m_rectCardContent;
        private GameObject m_goCharacterCardTemplate;

        private TMP_Text m_tmpCharacterName;
        private TMP_Text m_tmpRace;
        private TMP_Text m_tmpClass;
        private TMP_Text m_tmpCurrentHp;
        private TMP_Text m_tmpMaxHp;
        private TMP_Text m_tmpTempHp;
        private TMP_Text m_tmpDeathSaveSuccessesCount;
        private TMP_Text m_tmpDeathSaveFailuresCount;
        private TMP_Text m_tmpAc;
        private TMP_Text m_tmpInitiative;
        private TMP_Text m_tmpSpeed;
        private TMP_Text m_tmpPassivePerception;
        private TMP_Text m_tmpDc;
        private TMP_Text m_tmpSpellAttackBonus;
        private Slider m_sliderExperience;
        private TMP_Text m_tmpExperienceValue;
        private Image m_imgCharacterPortrait;
        private TMP_Text m_tmpCopper;
        private TMP_Text m_tmpSilver;
        private TMP_Text m_tmpElectrum;
        private TMP_Text m_tmpGold;
        private TMP_Text m_tmpPlatinum;
        private TMP_Text m_tmpHitDiceDie;
        private TMP_Text m_tmpHitDiceRemaining;
        private TMP_Text m_tmpPersonalityTraits;
        private TMP_Text m_tmpIdeals;
        private TMP_Text m_tmpFlaws;
        private TMP_Text m_tmpBackgroundFeatureBackgroundName;
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
        private TMP_Text m_tmpProficiencyBonus;
        private TMP_Text m_tmpEquipmentTools;
        private TMP_Text m_tmpClassLevel;
        private TMP_Text m_tmpClassFeatureClassName;
        private TMP_Text m_tmpClassFeatureSubclassName;
        private RectTransform m_rectClassFeatureContent;
        private GameObject m_goClassFeatureTemplate;
        private GameObject m_goClassSectionTemplate;
        private Transform m_transformClassSectionParent;
        private Button m_btnSectionSkills;
        private RectTransform m_rectSkillProficiencies;
        private Button m_btnSectionEquipmentTools;
        private RectTransform m_rectEquipmentToolContent;
        private GameObject m_goEquipmentToolTemplate;
        private Button m_btnSectionOtherFeatures;
        private RectTransform m_rectOtherFeatureContent;
        private GameObject m_goOtherFeatureTemplate;
        private TMP_Text m_tmpOtherFeatureSectionTitle;
        private TMP_Text m_tmpFeatureDetailTitle;
        private TMP_Text m_tmpFeatureDetailDescription;
        private Button m_btnSectionRace;
        private RectTransform m_rectRaceFeatureContent;
        private TMP_Text m_tmpRaceFeatureRaceName;
        private TMP_Text m_tmpRaceFeatureSubRaceName;
        private GameObject m_goRaceFeatureTemplate;
        private Button m_btnSectionSpells;
        private RectTransform m_rectSpellContent;
        private GameObject m_goSpellTemplate;
        private Button m_btnSectionInventory;
        private Button m_btnAddInventoryItem;
        private RectTransform m_rectInventoryContent;
        private GameObject m_goInventoryItemTemplate;
        private TMP_Text m_tmpInventorySectionTitle;
        private TMP_Text m_tmpCurrentWeight;
        private TMP_Text m_tmpWeightLine;
        private TMP_Text m_tmpMaxWeight;
        private GameObject m_panelInventoryActionButtons;
        private GameObject m_goInventoryUseAmount;
        private TMP_Text m_tmpInventoryUseAmountLabel;
        private TMP_InputField m_tmpInputInventoryUseAmount;
        private Button m_btnInventoryUseAction;
        private TMP_Text m_tmpInventoryUseActionLabel;
        private Button m_btnInventoryRestoreChargesAction;
        private TMP_Text m_tmpInventoryRestoreChargesActionLabel;
        private Button m_btnInventoryEquipAction;
        private TMP_Text m_tmpInventoryEquipActionLabel;
        private Button m_btnInventoryAttuneAction;
        private TMP_Text m_tmpInventoryAttuneActionLabel;
        private Button m_btnInventoryRemoveAction;
        private TMP_Text m_tmpInventoryRemoveActionLabel;
        private Button m_btnDiceHistory;
        private RectTransform m_gridStatusEffects;
        private GameObject m_goStatusEffectTemplate;
        private GameObject m_panelCharacterPopup;
        private TMP_Text m_tmpCharacterPopupTitle;
        private TMP_Text m_tmpCharacterPopupDescription;
        private Button m_btnCharacterPopupClose;
        private Button m_btnCharacterPopupConfirm;
        private TMP_Text m_tmpCharacterPopupConfirm;
        private GameObject m_goCharacterPopupList;
        private RectTransform m_rectCharacterPopupContent;
        private RectTransform m_rectCharacterPopupOptionContentRoot;
        private RectTransform m_rectCharacterPopupSelectedContent;
        private RectTransform m_rectCharacterPopupSelectedContentRoot;
        private GameObject m_goCharacterPopupOptionTemplate;
        private GameObject m_goCharacterPopupSelectedTemplate;
        private GameObject m_panelCharacterPopupForm;
        private TMP_InputField m_inputCharacterPopupName;
        private TMP_InputField m_inputCharacterPopupDescription;
        private readonly TMP_Text[] m_tmpSkillBonuses = new TMP_Text[18];
        private readonly Image[] m_imgSkillBackgrounds = new Image[18];
        private readonly Color[] m_defaultSkillBackgroundColors = new Color[18];
        private readonly List<CharacterClassSectionView> m_classSectionViews = new List<CharacterClassSectionView>();
        private readonly List<GameObject> m_equipmentToolItems = new List<GameObject>();
        private readonly List<GameObject> m_raceFeatureItems = new List<GameObject>();
        private readonly List<GameObject> m_spellItems = new List<GameObject>();
        private readonly List<GameObject> m_otherFeatureItems = new List<GameObject>();
        private readonly List<GameObject> m_inventoryItems = new List<GameObject>();
        private readonly List<GameObject> m_characterPopupOptionItems = new List<GameObject>();
        private readonly List<GameObject> m_characterPopupSelectedItems = new List<GameObject>();
        private readonly List<GameObject> m_statusEffectItems = new List<GameObject>();

        private readonly List<CharacterCardDraftSaveData> m_characterCards = new List<CharacterCardDraftSaveData>();
        private readonly List<CharacterListItemViewState> m_characterListItems = new List<CharacterListItemViewState>();
        private readonly List<CharacterCardListItemView> m_cardViews = new List<CharacterCardListItemView>();
        private List<LocalCustomItemSaveData> m_localItemOptions = new List<LocalCustomItemSaveData>();
        private readonly List<string> m_pendingLocalItemOrder = new List<string>();
        private readonly Dictionary<string, int> m_pendingLocalItemCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private int m_selectedCharacterIndex = -1;
        private string m_pendingLocalItemId = string.Empty;
        private bool m_showingLocalItemOptions;
        private CharacterCardPopupMode m_popupMode = CharacterCardPopupMode.None;
        private CharacterInventoryQuickRollContext m_visibleInventoryQuickRollContext;
        private CharacterDiceRollResultData m_visibleInventoryQuickRollResult;
        private string m_visibleInventoryQuickRollHistoryEntryId = string.Empty;
        private string m_visibleInventoryQuickRollCharacterId = string.Empty;
        private string m_visibleInventoryItemInstanceId = string.Empty;
        private Texture2D m_loadedPortraitTexture;
        private Sprite m_loadedPortraitSprite;
        private string m_loadedPortraitPath = string.Empty;

        protected override void ScriptGenerator()
        {
            m_bindComponent = gameObject.GetComponent<UIBindComponent>();
            if (m_bindComponent == null)
            {
                Log.Error($"根物体: {gameObject.name} 缺少组件 UIBindComponent, 请检查 CharacterCardManagementUI.prefab。");
                return;
            }

            m_tmpTitle = GetBoundComponent<TextMeshProUGUI>(0, nameof(m_tmpTitle));
            m_btnBack = GetBoundComponent<Button>(1, nameof(m_btnBack));
            m_rectCardContent = GetBoundComponent<RectTransform>(2, nameof(m_rectCardContent));
            m_goCharacterCardTemplate = GetBoundComponent<RectTransform>(3, nameof(m_goCharacterCardTemplate))?.gameObject;
            m_btnCreateCharacter = GetBoundComponent<Button>(4, nameof(m_btnCreateCharacter));
            m_btnDeleteSelected = GetBoundComponent<Button>(5, nameof(m_btnDeleteSelected));
            m_tmpCreateCharacterText = GetBoundComponent<TextMeshProUGUI>(6, nameof(m_tmpCreateCharacterText));
            m_tmpDeleteSelectedText = GetBoundComponent<TextMeshProUGUI>(7, nameof(m_tmpDeleteSelectedText));
            m_tmpCharacterName = GetBoundComponent<TextMeshProUGUI>(8, nameof(m_tmpCharacterName));
            m_tmpRace = GetBoundComponent<TextMeshProUGUI>(9, nameof(m_tmpRace));
            m_tmpClass = GetBoundComponent<TextMeshProUGUI>(10, nameof(m_tmpClass));
            m_tmpCurrentHp = FindRightPanelComponent<TextMeshProUGUI>("m_imgHpBackground/m_tmpCurrentHp");
            m_tmpMaxHp = FindRightPanelComponent<TextMeshProUGUI>("m_imgHpBackground/m_tmpMaxHp");
            m_tmpTempHp = FindRightPanelComponent<TextMeshProUGUI>("m_imgHpBackground/m_tmpTempHp");
            m_tmpDeathSaveSuccessesCount = FindRightPanelComponent<TextMeshProUGUI>("m_imgDeathSavesBackground/m_tmpDeathSaveSuccessesCount");
            m_tmpDeathSaveFailuresCount = FindRightPanelComponent<TextMeshProUGUI>("m_imgDeathSavesBackground/m_tmpDeathSaveFailuresCount");
            m_sliderExperience = FindRightPanelComponent<Slider>("m_sliderExperience");
            m_tmpExperienceValue = FindRightPanelComponent<TextMeshProUGUI>("m_sliderExperience/m_tmpExperienceValue");
            m_imgCharacterPortrait = FindRightPanelComponent<Image>("m_imgCharacterPortrait");
            m_tmpStrength = GetBoundComponent<TextMeshProUGUI>(11, nameof(m_tmpStrength));
            m_tmpDexterity = GetBoundComponent<TextMeshProUGUI>(12, nameof(m_tmpDexterity));
            m_tmpConstitution = GetBoundComponent<TextMeshProUGUI>(13, nameof(m_tmpConstitution));
            m_tmpIntelligence = GetBoundComponent<TextMeshProUGUI>(14, nameof(m_tmpIntelligence));
            m_tmpWisdom = GetBoundComponent<TextMeshProUGUI>(15, nameof(m_tmpWisdom));
            m_tmpCharisma = GetBoundComponent<TextMeshProUGUI>(16, nameof(m_tmpCharisma));
            m_tmpStrengthModifier = GetBoundComponent<TextMeshProUGUI>(17, nameof(m_tmpStrengthModifier));
            m_tmpDexterityModifier = GetBoundComponent<TextMeshProUGUI>(18, nameof(m_tmpDexterityModifier));
            m_tmpConstitutionModifier = GetBoundComponent<TextMeshProUGUI>(19, nameof(m_tmpConstitutionModifier));
            m_tmpIntelligenceModifier = GetBoundComponent<TextMeshProUGUI>(20, nameof(m_tmpIntelligenceModifier));
            m_tmpWisdomModifier = GetBoundComponent<TextMeshProUGUI>(21, nameof(m_tmpWisdomModifier));
            m_tmpCharismaModifier = GetBoundComponent<TextMeshProUGUI>(22, nameof(m_tmpCharismaModifier));
            m_tmpProficiencyBonus = FindRightPanelComponent<TextMeshProUGUI>("m_imgProficiencyBonusBackground/m_tmpProficiencyBonus");
            m_tmpEquipmentTools = GetBoundComponent<TextMeshProUGUI>(24, nameof(m_tmpEquipmentTools));
            m_btnSectionSkills = GetBoundComponent<Button>(28, nameof(m_btnSectionSkills));
            m_rectSkillProficiencies = GetBoundComponent<RectTransform>(29, nameof(m_rectSkillProficiencies));
            for (int index = 0; index < m_tmpSkillBonuses.Length; index++)
            {
                m_tmpSkillBonuses[index] = GetBoundComponent<TextMeshProUGUI>(30 + index, $"{nameof(m_tmpSkillBonuses)}[{index}]");
                m_imgSkillBackgrounds[index] = m_tmpSkillBonuses[index] != null
                    ? m_tmpSkillBonuses[index].GetComponentInParent<Image>()
                    : null;
                m_defaultSkillBackgroundColors[index] = m_imgSkillBackgrounds[index] != null
                    ? m_imgSkillBackgrounds[index].color
                    : Color.white;
            }
            m_btnSectionEquipmentTools = GetBoundComponent<Button>(48, nameof(m_btnSectionEquipmentTools));
            m_rectEquipmentToolContent = GetBoundComponent<RectTransform>(49, nameof(m_rectEquipmentToolContent));
            m_goEquipmentToolTemplate = GetBoundComponent<RectTransform>(50, nameof(m_goEquipmentToolTemplate))?.gameObject;
            m_btnSectionOtherFeatures = GetBoundComponent<Button>(56, nameof(m_btnSectionOtherFeatures));
            m_rectOtherFeatureContent = GetBoundComponent<RectTransform>(57, nameof(m_rectOtherFeatureContent));
            m_goOtherFeatureTemplate = GetBoundComponent<RectTransform>(58, nameof(m_goOtherFeatureTemplate))?.gameObject;
            m_tmpOtherFeatureSectionTitle = GetBoundComponent<TextMeshProUGUI>(59, nameof(m_tmpOtherFeatureSectionTitle));
            m_tmpFeatureDetailTitle = GetBoundComponent<TextMeshProUGUI>(60, nameof(m_tmpFeatureDetailTitle));
            m_tmpFeatureDetailDescription = GetBoundComponent<TextMeshProUGUI>(61, nameof(m_tmpFeatureDetailDescription));
            m_btnSectionRace = FindRightPanelComponent<Button>("m_scrollCharacterDetail/m_viewportCharacterDetail/m_rectCharacterDetailContent/m_sectionRace");
            m_rectRaceFeatureContent = FindRightPanelComponent<RectTransform>("m_scrollCharacterDetail/m_viewportCharacterDetail/m_rectCharacterDetailContent/m_rectRaceFeatureContent");
            m_tmpRaceFeatureRaceName = FindRightPanelComponent<TextMeshProUGUI>("m_scrollCharacterDetail/m_viewportCharacterDetail/m_rectCharacterDetailContent/m_sectionRace/m_panelRaceFeatureHeader/m_tmpRaceFeatureRaceName");
            m_tmpRaceFeatureSubRaceName = FindRightPanelComponent<TextMeshProUGUI>("m_scrollCharacterDetail/m_viewportCharacterDetail/m_rectCharacterDetailContent/m_sectionRace/m_panelRaceFeatureHeader/m_tmpRaceFeatureSubRaceName");
            m_goRaceFeatureTemplate = FindRightPanelComponent<RectTransform>("m_scrollCharacterDetail/m_viewportCharacterDetail/m_rectCharacterDetailContent/m_rectRaceFeatureContent/m_itemRaceFeatureTemplate")?.gameObject;
            m_btnSectionSpells = FindRightPanelComponent<Button>("m_scrollCharacterDetail/m_viewportCharacterDetail/m_rectCharacterDetailContent/m_btnSectionSpells");
            m_rectSpellContent = FindRightPanelComponent<RectTransform>("m_scrollCharacterDetail/m_viewportCharacterDetail/m_rectCharacterDetailContent/m_rectSpellContent");
            m_goSpellTemplate = FindRightPanelComponent<RectTransform>("m_scrollCharacterDetail/m_viewportCharacterDetail/m_rectCharacterDetailContent/m_rectSpellContent/m_itemSpellTemplate")?.gameObject;
            m_tmpClassLevel = GetBoundComponent<TextMeshProUGUI>(51, nameof(m_tmpClassLevel));
            m_rectClassFeatureContent = GetBoundComponent<RectTransform>(52, nameof(m_rectClassFeatureContent));
            m_goClassFeatureTemplate = GetBoundComponent<RectTransform>(53, nameof(m_goClassFeatureTemplate))?.gameObject;
            m_tmpClassFeatureClassName = GetBoundComponent<TextMeshProUGUI>(54, nameof(m_tmpClassFeatureClassName));
            m_tmpClassFeatureSubclassName = GetBoundComponent<TextMeshProUGUI>(55, nameof(m_tmpClassFeatureSubclassName));
            m_btnSectionInventory = GetBoundComponent<Button>(62, nameof(m_btnSectionInventory));
            m_btnAddInventoryItem = FindRightPanelComponent<Button>("m_scrollCharacterDetail/m_viewportCharacterDetail/m_rectCharacterDetailContent/m_sectionInventory/m_panelInventoryHeader/m_btnAddInventoryItem");
            m_rectInventoryContent = GetBoundComponent<RectTransform>(63, nameof(m_rectInventoryContent));
            m_goInventoryItemTemplate = GetBoundComponent<RectTransform>(64, nameof(m_goInventoryItemTemplate))?.gameObject;
            m_tmpInventorySectionTitle = GetBoundComponent<TextMeshProUGUI>(65, nameof(m_tmpInventorySectionTitle));
            m_tmpCurrentWeight = FindRightPanelComponent<TextMeshProUGUI>("m_scrollCharacterDetail/m_viewportCharacterDetail/m_rectCharacterDetailContent/m_sectionInventory/m_panelInventoryHeader/m_tmpCurrentWeight");
            m_tmpWeightLine = FindRightPanelComponent<TextMeshProUGUI>("m_scrollCharacterDetail/m_viewportCharacterDetail/m_rectCharacterDetailContent/m_sectionInventory/m_panelInventoryHeader/m_tmpWeightLine");
            m_tmpMaxWeight = FindRightPanelComponent<TextMeshProUGUI>("m_scrollCharacterDetail/m_viewportCharacterDetail/m_rectCharacterDetailContent/m_sectionInventory/m_panelInventoryHeader/m_tmpMaxWeight");
            m_panelInventoryActionButtons = FindRightPanelComponent<RectTransform>("m_scrollDetailInfoPanel/m_panelFeatureDetailDescription/m_panelInventoryActionButtons")?.gameObject;
            m_goInventoryUseAmount = FindRightPanelComponent<RectTransform>("m_scrollDetailInfoPanel/m_panelFeatureDetailDescription/m_panelInventoryActionButtons/InventoryUseAmount")?.gameObject;
            m_tmpInventoryUseAmountLabel = FindRightPanelComponent<TextMeshProUGUI>("m_scrollDetailInfoPanel/m_panelFeatureDetailDescription/m_panelInventoryActionButtons/InventoryUseAmount/m_tmpInventoryUseAmountLabel");
            m_tmpInputInventoryUseAmount = FindRightPanelComponent<TMP_InputField>("m_scrollDetailInfoPanel/m_panelFeatureDetailDescription/m_panelInventoryActionButtons/InventoryUseAmount/m_tmpInputInventoryUseAmount");
            m_btnInventoryUseAction = FindRightPanelComponent<Button>("m_scrollDetailInfoPanel/m_panelFeatureDetailDescription/m_panelInventoryActionButtons/m_btnInventoryUseAction");
            m_tmpInventoryUseActionLabel = FindRightPanelComponent<TextMeshProUGUI>("m_scrollDetailInfoPanel/m_panelFeatureDetailDescription/m_panelInventoryActionButtons/m_btnInventoryUseAction/m_tmpInventoryUseActionLabel");
            m_btnInventoryRestoreChargesAction = FindRightPanelComponent<Button>("m_scrollDetailInfoPanel/m_panelFeatureDetailDescription/m_panelInventoryActionButtons/m_btnInventoryRestoreChargesAction");
            m_tmpInventoryRestoreChargesActionLabel = FindRightPanelComponent<TextMeshProUGUI>("m_scrollDetailInfoPanel/m_panelFeatureDetailDescription/m_panelInventoryActionButtons/m_btnInventoryRestoreChargesAction/m_tmpInventoryRestoreChargesActionLabel");
            m_btnInventoryEquipAction = FindRightPanelComponent<Button>("m_scrollDetailInfoPanel/m_panelFeatureDetailDescription/m_panelInventoryActionButtons/m_btnInventoryEquipAction");
            m_tmpInventoryEquipActionLabel = FindRightPanelComponent<TextMeshProUGUI>("m_scrollDetailInfoPanel/m_panelFeatureDetailDescription/m_panelInventoryActionButtons/m_btnInventoryEquipAction/m_tmpInventoryEquipActionLabel");
            m_btnInventoryAttuneAction = FindRightPanelComponent<Button>("m_scrollDetailInfoPanel/m_panelFeatureDetailDescription/m_panelInventoryActionButtons/m_btnInventoryAttuneAction");
            m_tmpInventoryAttuneActionLabel = FindRightPanelComponent<TextMeshProUGUI>("m_scrollDetailInfoPanel/m_panelFeatureDetailDescription/m_panelInventoryActionButtons/m_btnInventoryAttuneAction/m_tmpInventoryAttuneActionLabel");
            m_btnInventoryRemoveAction = FindRightPanelComponent<Button>("m_scrollDetailInfoPanel/m_panelFeatureDetailDescription/m_panelInventoryActionButtons/m_btnInventoryRemoveAction");
            m_tmpInventoryRemoveActionLabel = FindRightPanelComponent<TextMeshProUGUI>("m_scrollDetailInfoPanel/m_panelFeatureDetailDescription/m_panelInventoryActionButtons/m_btnInventoryRemoveAction/m_tmpInventoryRemoveActionLabel");
            m_btnDiceHistory = FindRightPanelComponent<Button>("m_scrollDetailInfoPanel/m_panelFeatureDetailDescription/m_panelInventoryActionButtons/m_btnDiceHistory");
            m_gridStatusEffects = FindRightPanelComponent<RectTransform>("m_gridStatusEffects");
            m_goStatusEffectTemplate = FindRightPanelComponent<RectTransform>("m_gridStatusEffects/m_itemStatusEffectTemplate")?.gameObject;
            m_tmpAc = FindRightPanelComponent<TextMeshProUGUI>("m_imgAcBackground/m_tmpAc");
            m_tmpInitiative = FindRightPanelComponent<TextMeshProUGUI>("m_imgInitiativeBackground/m_tmpInitiative");
            m_tmpSpeed = FindRightPanelComponent<TextMeshProUGUI>("m_imgSpeedBackground/m_tmpSpeed");
            m_tmpPassivePerception = FindRightPanelComponent<TextMeshProUGUI>("m_imgPassivePerceptionBackground/m_tmpPassivePerception");
            m_tmpDc = FindRightPanelComponent<TextMeshProUGUI>("m_imgDcBackground/m_tmpDc");
            m_tmpSpellAttackBonus = FindRightPanelComponent<TextMeshProUGUI>("m_imgSpellAttackBonusBackground/m_tmpSpellAttackBonus");
            m_tmpCopper = FindRightPanelComponent<TextMeshProUGUI>("m_imgCopperBackground/m_tmpCopper");
            m_tmpSilver = FindRightPanelComponent<TextMeshProUGUI>("m_imgSilverBackground/m_tmpSilver");
            m_tmpElectrum = FindRightPanelComponent<TextMeshProUGUI>("m_imgElectrumBackground/m_tmpElectrum");
            m_tmpGold = FindRightPanelComponent<TextMeshProUGUI>("m_imgGoldBackground/m_tmpGold");
            m_tmpPlatinum = FindRightPanelComponent<TextMeshProUGUI>("m_imgPlatinumBackground/m_tmpPlatinum");
            m_tmpHitDiceDie = FindRightPanelComponent<TextMeshProUGUI>("m_imgHitDiceBackground/m_tmpHitDiceDie");
            m_tmpHitDiceRemaining = FindRightPanelComponent<TextMeshProUGUI>("m_imgHitDiceBackground/m_tmpHitDiceRemaining");
            m_tmpPersonalityTraits = FindRightPanelComponent<TextMeshProUGUI>("m_imgPersonalityTraitsBackground/m_tmpPersonalityTraits");
            m_tmpIdeals = FindRightPanelComponent<TextMeshProUGUI>("m_imgIdealsBackground/m_tmpIdeals");
            m_tmpFlaws = FindRightPanelComponent<TextMeshProUGUI>("m_imgFlawsBackground/m_tmpFlaws");
            m_tmpBackgroundFeatureBackgroundName = FindRightPanelComponent<TextMeshProUGUI>("m_scrollCharacterDetail/m_viewportCharacterDetail/m_rectCharacterDetailContent/m_sectionBackground/m_panelBackgroundFeatureHeader/m_tmpBackgroundFeatureBackgroundName");
            EnsureCharacterPopupBindings();
            m_goClassSectionTemplate = m_tmpClassFeatureClassName != null
                ? m_tmpClassFeatureClassName.transform.parent?.parent?.gameObject
                : null;
            m_transformClassSectionParent = m_goClassSectionTemplate != null
                ? m_goClassSectionTemplate.transform.parent
                : null;
            m_classSectionViews.Clear();
            if (m_goClassSectionTemplate != null && m_rectClassFeatureContent != null)
            {
                m_classSectionViews.Add(CharacterClassSectionView.Bind(m_goClassSectionTemplate, m_rectClassFeatureContent.gameObject, ShowFeatureDetail));
            }

            BindButton(m_btnBack, CloseToHome);
            BindButton(m_btnCreateCharacter, OnClickCreateCharacter);
            BindButton(m_btnDeleteSelected, OnClickDeleteSelectedCharacter);
            BindButton(m_btnSectionSkills, OnClickToggleSkillProficiencies);
            BindButton(m_btnSectionEquipmentTools, OnClickToggleEquipmentTools);
            BindButton(m_btnSectionOtherFeatures, OnClickToggleOtherFeatures);
            BindButton(m_btnSectionRace, OnClickToggleRaceFeatures);
            BindButton(m_btnSectionInventory, OnClickToggleInventory);
            BindButton(m_btnAddInventoryItem, OnClickAddInventoryItem);
            BindButton(m_btnSectionSpells, OnClickToggleSpells);
            BindButton(m_btnInventoryUseAction, OnClickInventoryUseAction);
            BindButton(m_btnInventoryRestoreChargesAction, OnClickInventoryRestoreChargesAction);
            BindButton(m_btnInventoryEquipAction, OnClickInventoryEquipAction);
            BindButton(m_btnInventoryAttuneAction, OnClickInventoryAttuneAction);
            BindButton(m_btnInventoryRemoveAction, OnClickInventoryRemoveAction);
            BindButton(m_btnDiceHistory, OnClickDiceHistory);
            BindButton(m_btnCharacterPopupClose, CloseCharacterPopup);
            BindButton(m_btnCharacterPopupConfirm, OnClickCharacterPopupConfirm);
            if (m_btnAddInventoryItem == null)
            {
                Log.Warning("角色卡管理：m_btnAddInventoryItem 未绑定，请确认按钮位于 m_sectionInventory/m_panelInventoryHeader/m_btnAddInventoryItem。");
            }
            SetText(m_tmpTitle, "角色卡管理");
            SetText(m_tmpCreateCharacterText, "新建角色");
            SetText(m_tmpDeleteSelectedText, "删除所选");
            SetText(m_tmpOtherFeatureSectionTitle, "其他特性");
            SetText(m_tmpInventorySectionTitle, "物品");
            SetText(m_tmpWeightLine, string.Empty);
            SetActive(m_goCharacterCardTemplate, false);
            SetActive(m_goEquipmentToolTemplate, false);
            SetActive(m_goClassFeatureTemplate, false);
            SetActive(m_goRaceFeatureTemplate, false);
            SetActive(m_goSpellTemplate, false);
            SetActive(m_goOtherFeatureTemplate, false);
            SetActive(m_goInventoryItemTemplate, false);
            SetActive(m_goStatusEffectTemplate, false);
            HideInventoryActionButtons();
            SetActive(m_goCharacterPopupOptionTemplate, false);
            SetActive(m_goCharacterPopupSelectedTemplate, false);
            CloseCharacterPopup();
            ShowFeatureDetail("特性详情", string.Empty);
        }

        private T GetBoundComponent<T>(int index, string fieldName) where T : Component
        {
            T component = m_bindComponent.GetComponent<T>(index);
            if (component == null)
            {
                Log.Warning($"角色卡管理：UIBindComponent 绑定缺失，字段 {fieldName}，索引 {index}。");
            }

            return component;
        }

        private T FindRightPanelComponent<T>(string relativePath) where T : Component
        {
            Transform target = transform.Find($"m_panelCharacterMgr/m_panelRight/{relativePath}");
            if (target == null)
            {
                Log.Warning($"角色卡管理：右侧面板节点缺失 {relativePath}。");
                return null;
            }

            T component = target.GetComponent<T>();
            if (component == null)
            {
                Log.Warning($"角色卡管理：右侧面板节点 {relativePath} 缺少组件 {typeof(T).Name}。");
            }

            return component;
        }

        private void EnsureCharacterPopupBindings()
        {
            Transform popup = transform.Find("m_panelCharacterMgr/m_panelCharacterPopup")
                ?? transform.Find("m_panelCharacterMgr/m_panelRight/m_panelCharacterPopup");
            if (popup == null)
            {
                Log.Warning("角色卡管理：通用弹窗节点 m_panelCharacterPopup 缺失，请先在 prefab 中创建该弹窗。");
                return;
            }

            m_panelCharacterPopup = popup.gameObject;
            m_tmpCharacterPopupTitle = FirstComponent<TMP_Text>(popup,
                "m_tmpCharacterPopupTitle",
                "m_panelCharacterPopupHeader/m_tmpCharacterPopupTitle");
            m_tmpCharacterPopupDescription = FirstComponent<TMP_Text>(popup,
                "m_tmpCharacterPopupDescription",
                "m_panelCharacterPopupDescription/m_tmpCharacterPopupDescription");
            m_btnCharacterPopupClose = FirstComponent<Button>(popup,
                "m_btnCharacterPopupClose",
                "m_panelCharacterPopupFooter/m_btnCharacterPopupClose");
            m_btnCharacterPopupConfirm = FirstComponent<Button>(popup,
                "m_btnCharacterPopupConfirm",
                "m_panelCharacterPopupFooter/m_btnCharacterPopupConfirm");
            m_tmpCharacterPopupConfirm = FirstComponent<TMP_Text>(popup,
                "m_btnCharacterPopupConfirm/m_tmpCharacterPopupConfirm",
                "m_panelCharacterPopupFooter/m_btnCharacterPopupConfirm/m_tmpCharacterPopupConfirm");
            m_goCharacterPopupList = FirstGameObject(popup,
                "m_scrollCharacterPopupList",
                "m_panelCharacterPopupList");
            m_rectCharacterPopupContent = FirstComponent<RectTransform>(popup,
                "m_scrollCharacterPopupList/m_viewportCharacterPopupList/m_rectCharacterPopupContent",
                "m_panelCharacterPopupList/m_rectCharacterPopupContent");
            m_rectCharacterPopupOptionContentRoot = ResolveScrollContentRoot(m_rectCharacterPopupContent,
                "m_rectCharacterPopupOptionContent",
                "m_contentCharacterPopupOptions",
                "m_rectContent",
                "Content",
                "Viewport/Content",
                "m_viewport/Content");
            m_rectCharacterPopupSelectedContent = FirstComponent<RectTransform>(popup,
                "m_panelCharacterPopupList/m_rectCharacterPopupSelectedContent");
            m_rectCharacterPopupSelectedContentRoot = ResolveScrollContentRoot(m_rectCharacterPopupSelectedContent,
                "m_rectCharacterPopupSelectedItemContent",
                "m_contentCharacterPopupSelectedItems",
                "m_rectContent",
                "Content",
                "Viewport/Content",
                "m_viewport/Content");
            m_goCharacterPopupOptionTemplate = m_rectCharacterPopupOptionContentRoot != null
                ? FirstGameObject(m_rectCharacterPopupOptionContentRoot.transform,
                    "m_itemCharacterPopupOptionTemplate")
                : null;
            m_goCharacterPopupSelectedTemplate = m_rectCharacterPopupSelectedContentRoot != null
                ? FirstGameObject(m_rectCharacterPopupSelectedContentRoot.transform,
                    "m_itemCharacterPopupSelectedTemplate")
                : null;
            m_panelCharacterPopupForm = FirstGameObject(popup, "m_panelCharacterPopupForm");
            m_inputCharacterPopupName = FirstComponent<TMP_InputField>(popup, "m_panelCharacterPopupForm/m_inputCharacterPopupName");
            m_inputCharacterPopupDescription = FirstComponent<TMP_InputField>(popup, "m_panelCharacterPopupForm/m_inputCharacterPopupDescription");
        }

        protected override void OnRefresh()
        {
            LoadRuleContent();
            LoadCharacterCards();
            RefreshCharacterListView();
        }

        private void LoadRuleContent()
        {
            CharacterApplicationService.Instance.ReloadRuleContent();
        }

        private void LoadCharacterCards()
        {
            CharacterLibraryViewState library = CharacterApplicationService.Instance.LoadLibrary();
            m_characterCards.Clear();
            m_characterListItems.Clear();

            if (library?.Characters != null)
            {
                for (int index = 0; index < library.Characters.Count; index++)
                {
                    CharacterCardDraftSaveData character = library.Characters[index];
                    if (!string.IsNullOrWhiteSpace(character.CharacterId))
                    {
                        m_characterCards.Add(character);
                        if (library.Items != null && index < library.Items.Count)
                        {
                            m_characterListItems.Add(library.Items[index]);
                        }
                        else
                        {
                            m_characterListItems.Add(CharacterViewStateBuilder.BuildListItem(character));
                        }
                    }
                }
            }

            if (m_characterCards.Count == 0)
            {
                m_selectedCharacterIndex = -1;
            }
            else if (m_selectedCharacterIndex < 0 || m_selectedCharacterIndex >= m_characterCards.Count)
            {
                m_selectedCharacterIndex = 0;
            }
        }

        private void RestoreSelectedCharacter(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return;
            }

            for (int index = 0; index < m_characterCards.Count; index++)
            {
                CharacterCardDraftSaveData character = m_characterCards[index];
                if (character != null && string.Equals(character.CharacterId, characterId, StringComparison.OrdinalIgnoreCase))
                {
                    m_selectedCharacterIndex = index;
                    return;
                }
            }
        }

        private void RefreshCharacterListView()
        {
            EnsureCardViewCount(m_characterCards.Count);
            for (int index = 0; index < m_cardViews.Count; index++)
            {
                if (index >= m_characterCards.Count)
                {
                    m_cardViews[index].SetActive(false);
                    continue;
                }

                CharacterCardDraftSaveData character = m_characterCards[index];
                CharacterListItemViewState itemState = index < m_characterListItems.Count
                    ? m_characterListItems[index]
                    : CharacterViewStateBuilder.BuildListItem(character);
                int capturedIndex = index;
                m_cardViews[index].Bind(
                    character,
                    itemState.ClassLine,
                    itemState.StatusLine,
                    index == m_selectedCharacterIndex,
                    () => OnClickSelectCharacterCard(capturedIndex),
                    () => OnClickEditCharacterCard(capturedIndex));
            }

            SetButtonInteractable(m_btnDeleteSelected, HasSelectedCharacter());
            RefreshSelectedCharacterDetail();
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

        private void RefreshSelectedCharacterDetail()
        {
            if (!HasSelectedCharacter())
            {
                SetEmptyDetail();
                return;
            }

            CharacterCardDraftSaveData selectedCharacter = m_characterCards[m_selectedCharacterIndex];
            CharacterCardDraftSaveData character = selectedCharacter;
            CharacterRuntimeSnapshotData snapshot = CharacterDetailCalculationService.Instance.BuildDisplaySnapshot(character);
            if (CharacterApplicationService.Instance.TryGetCharacterDetail(selectedCharacter.CharacterId, out CharacterDetailViewState detail))
            {
                character = detail.Character;
                snapshot = detail.RuntimeSnapshot;
            }

            CharacterSheetDisplayViewState sheetState = CharacterSheetViewStateService.Instance.Build(character, snapshot);
            SetText(m_tmpCharacterName, sheetState.CharacterNameText);
            SetText(m_tmpRace, sheetState.RaceText);
            SetText(m_tmpClass, sheetState.ClassText);
            ApplyPortrait(character.PreviewImagePath);
            RefreshClassSections(character);
            SetAbilityTexts(sheetState.AbilityDisplay);
            SetHpTexts(sheetState);
            SetCombatOverviewTexts(sheetState);
            RefreshStatusEffectItems(sheetState.StatusEffects);
            SetExperienceProgress(sheetState.Experience);
            SetCurrencyTexts(sheetState);
            SetCarryingWeightTexts();
            SetDeathSaveTexts(sheetState);
            SetHitDiceTexts(sheetState);
            SetRoleplayTexts(character.RoleplayProfile);
            SetBackgroundText(snapshot);
            SetText(m_tmpProficiencyBonus, sheetState.ProficiencyBonusText);
            SetSkillBonusTexts(sheetState.Skills);
            SetText(m_tmpEquipmentTools, "装备与工具熟练项");
            RefreshEquipmentToolItems(sheetState.EquipmentToolLabels);
            RefreshInventoryItems(sheetState.InventoryItems);
            RefreshRaceFeatureSection(character, snapshot, character.RaceId);
            RefreshSpellItems(character);
            RefreshOtherFeatureSection(character);
            ShowFeatureDetail("特性详情", string.Empty);
        }

        private void SetEmptyDetail()
        {
            SetText(m_tmpCharacterName, "未选择角色");
            SetText(m_tmpRace, "种族");
            SetText(m_tmpClass, "职业");
            ApplyPortrait(string.Empty);
            RefreshClassSections(null);
            CharacterRuntimeSnapshotData empty = new CharacterRuntimeSnapshotData();
            SetAbilityTexts(empty);
            SetHpTexts(empty);
            SetEmptyCombatOverviewTexts();
            RefreshStatusEffectItems((CharacterCardDraftSaveData)null);
            SetExperienceProgress(0, 1);
            SetCurrencyTexts((CharacterCurrencySaveData)null);
            SetCarryingWeightTexts();
            SetDeathSaveTexts((CharacterDeathSaveData)null);
            SetHitDiceTexts((CharacterCardDraftSaveData)null);
            SetRoleplayTexts(null);
            SetBackgroundText(empty);
            SetText(m_tmpProficiencyBonus, CharacterDetailCalculationService.Instance.BuildProficiencyBonusDisplay(1).Label);
            SetSkillBonusTexts(null, empty);
            SetText(m_tmpEquipmentTools, "装备与工具熟练项");
            RefreshEquipmentToolItems(empty);
            RefreshInventoryItems((CharacterEquipmentSetSaveData)null);
            RefreshRaceFeatureSection(null, empty, string.Empty);
            RefreshSpellItems(null);
            RefreshOtherFeatureSection(null);
            ShowFeatureDetail("特性详情", string.Empty);
        }

        private void SetAbilityTexts(CharacterRuntimeSnapshotData snapshot)
        {
            CharacterAbilityDisplayViewState state = CharacterDetailCalculationService.Instance.BuildAbilityDisplay(snapshot);
            SetAbilityTexts(state);
        }

        private void SetAbilityTexts(CharacterAbilityDisplayViewState state)
        {
            if (state == null)
            {
                state = new CharacterAbilityDisplayViewState();
            }
            SetText(m_tmpStrength, state.Strength.ToString());
            SetText(m_tmpDexterity, state.Dexterity.ToString());
            SetText(m_tmpConstitution, state.Constitution.ToString());
            SetText(m_tmpIntelligence, state.Intelligence.ToString());
            SetText(m_tmpWisdom, state.Wisdom.ToString());
            SetText(m_tmpCharisma, state.Charisma.ToString());
            SetText(m_tmpStrengthModifier, state.StrengthModifier);
            SetText(m_tmpDexterityModifier, state.DexterityModifier);
            SetText(m_tmpConstitutionModifier, state.ConstitutionModifier);
            SetText(m_tmpIntelligenceModifier, state.IntelligenceModifier);
            SetText(m_tmpWisdomModifier, state.WisdomModifier);
            SetText(m_tmpCharismaModifier, state.CharismaModifier);
        }

        private void SetHpTexts(CharacterRuntimeSnapshotData snapshot)
        {
            CharacterHpDisplayViewState state = CharacterDetailCalculationService.Instance.BuildHpDisplay(snapshot);
            SetText(m_tmpCurrentHp, state.CurrentHp.ToString());
            SetText(m_tmpMaxHp, state.MaxHp.ToString());
            SetText(m_tmpTempHp, state.TemporaryHp.ToString());
        }

        private void SetHpTexts(CharacterSheetDisplayViewState state)
        {
            SetText(m_tmpCurrentHp, state?.CurrentHpText ?? string.Empty);
            SetText(m_tmpMaxHp, state?.MaxHpText ?? string.Empty);
            SetText(m_tmpTempHp, state?.TemporaryHpText ?? string.Empty);
        }

        private void SetCombatOverviewTexts(CharacterCardDraftSaveData character, CharacterRuntimeSnapshotData snapshot)
        {
            if (snapshot == null)
            {
                SetEmptyCombatOverviewTexts();
                return;
            }

            CharacterCombatOverviewViewState overview = CharacterDetailCalculationService.Instance.BuildCombatOverview(character, snapshot);
            SetText(m_tmpAc, overview.ArmorClass > 0 ? overview.ArmorClass.ToString() : string.Empty);
            SetText(m_tmpInitiative, FormatSignedNumber(overview.InitiativeBonus));
            SetText(m_tmpSpeed, overview.Speed > 0 ? overview.Speed.ToString() : string.Empty);
            SetText(m_tmpPassivePerception, overview.PassivePerception.ToString());

            if (overview.HasSpellcasting)
            {
                SetText(m_tmpDc, overview.SpellSaveDc.ToString());
                SetText(m_tmpSpellAttackBonus, FormatSignedNumber(overview.SpellAttackBonus));
            }
            else
            {
                SetText(m_tmpDc, "-");
                SetText(m_tmpSpellAttackBonus, "-");
            }
        }

        private void SetCombatOverviewTexts(CharacterSheetDisplayViewState state)
        {
            if (state == null)
            {
                SetEmptyCombatOverviewTexts();
                return;
            }

            SetText(m_tmpAc, state.ArmorClassText);
            SetText(m_tmpInitiative, state.InitiativeText);
            SetText(m_tmpSpeed, state.SpeedText);
            SetText(m_tmpPassivePerception, state.PassivePerceptionText);
            SetText(m_tmpDc, state.SpellSaveDcText);
            SetText(m_tmpSpellAttackBonus, state.SpellAttackBonusText);
        }

        private void SetEmptyCombatOverviewTexts()
        {
            SetText(m_tmpAc, string.Empty);
            SetText(m_tmpInitiative, string.Empty);
            SetText(m_tmpSpeed, string.Empty);
            SetText(m_tmpPassivePerception, string.Empty);
            SetText(m_tmpDc, string.Empty);
            SetText(m_tmpSpellAttackBonus, string.Empty);
        }

        private void SetExperienceProgress(int experience, int level)
        {
            CharacterExperienceDisplayViewState state = CharacterDetailCalculationService.Instance.BuildExperienceDisplay(experience, level);
            SetExperienceProgress(state);
        }

        private void SetExperienceProgress(CharacterExperienceDisplayViewState state)
        {
            if (state == null)
            {
                state = new CharacterExperienceDisplayViewState();
            }

            if (m_sliderExperience == null)
            {
                SetText(m_tmpExperienceValue, state.Label);
                return;
            }

            m_sliderExperience.value = Mathf.Clamp01(state.Progress);
            SetText(m_tmpExperienceValue, state.Label);
        }

        private void SetSkillBonusTexts(CharacterCardDraftSaveData character, CharacterRuntimeSnapshotData snapshot)
        {
            for (int index = 0; index < SkillDisplayBindings.Length; index++)
            {
                SkillDisplayBinding binding = SkillDisplayBindings[index];
                CharacterSkillBonusViewState state = CharacterDetailCalculationService.Instance.BuildSkillBonus(
                    character,
                    snapshot,
                    binding.SkillId,
                    binding.DisplayName,
                    binding.Ability);
                SetSkillBonus(index, state.Bonus);
                SetSkillBackground(index, state.HasProficiency, state.HasExpertise);
            }
        }

        private void SetSkillBonusTexts(IReadOnlyList<CharacterSheetSkillViewState> skills)
        {
            for (int index = 0; index < SkillDisplayBindings.Length; index++)
            {
                CharacterSheetSkillViewState state = skills != null && index < skills.Count
                    ? skills[index]
                    : null;
                SetSkillBonus(index, state != null ? state.Bonus : 0);
                SetSkillBackground(index, state != null && state.HasProficiency, state != null && state.HasExpertise);
            }
        }

        private void SetCurrencyTexts(CharacterCurrencySaveData currency)
        {
            CharacterCurrencySaveData normalized = CharacterCurrencySaveData.Clone(currency);
            SetText(m_tmpCopper, normalized.Copper.ToString());
            SetText(m_tmpSilver, normalized.Silver.ToString());
            SetText(m_tmpElectrum, normalized.Electrum.ToString());
            SetText(m_tmpGold, normalized.Gold.ToString());
            SetText(m_tmpPlatinum, normalized.Platinum.ToString());
        }

        private void SetCurrencyTexts(CharacterSheetDisplayViewState state)
        {
            SetText(m_tmpCopper, state?.CopperText ?? string.Empty);
            SetText(m_tmpSilver, state?.SilverText ?? string.Empty);
            SetText(m_tmpElectrum, state?.ElectrumText ?? string.Empty);
            SetText(m_tmpGold, state?.GoldText ?? string.Empty);
            SetText(m_tmpPlatinum, state?.PlatinumText ?? string.Empty);
        }

        private void SetCarryingWeightTexts()
        {
            SetText(m_tmpCurrentWeight, string.Empty);
            SetText(m_tmpWeightLine, string.Empty);
            SetText(m_tmpMaxWeight, string.Empty);
        }

        private void RefreshStatusEffectItems(CharacterCardDraftSaveData character)
        {
            List<CharacterStatusEffectDisplayEntry> entries = CharacterDetailDisplayService.Instance.BuildStatusEffectEntries(character);
            RefreshStatusEffectItems(entries);
        }

        private void RefreshStatusEffectItems(IReadOnlyList<CharacterStatusEffectDisplayEntry> entries)
        {
            if (entries == null)
            {
                entries = Array.Empty<CharacterStatusEffectDisplayEntry>();
            }
            EnsureStatusEffectItemCount(entries.Count);

            for (int index = 0; index < m_statusEffectItems.Count; index++)
            {
                GameObject item = m_statusEffectItems[index];
                bool active = index < entries.Count;
                SetActive(item, active);
                if (active)
                {
                    SetStatusEffectItem(item, entries[index]);
                }
            }

            SetActive(m_goStatusEffectTemplate, false);
            SetActive(m_gridStatusEffects != null ? m_gridStatusEffects.gameObject : null, entries.Count > 0);
        }

        private void EnsureStatusEffectItemCount(int count)
        {
            if (m_gridStatusEffects == null || m_goStatusEffectTemplate == null)
            {
                return;
            }

            while (m_statusEffectItems.Count < count)
            {
                GameObject itemObject = UnityEngine.Object.Instantiate(m_goStatusEffectTemplate, m_gridStatusEffects);
                itemObject.name = $"m_itemStatusEffect_{m_statusEffectItems.Count + 1}";
                SetActive(itemObject, true);
                m_statusEffectItems.Add(itemObject);
            }
        }

        private static void SetStatusEffectItem(GameObject item, CharacterStatusEffectDisplayEntry entry)
        {
            if (item == null)
            {
                return;
            }

            TMP_Text nameText = item.transform.Find("m_tmpStatusEffectName")?.GetComponent<TMP_Text>();
            TMP_Text durationText = item.transform.Find("m_imgStatusEffectIcon/m_tmpStatusEffectDuration")?.GetComponent<TMP_Text>();
            SetText(nameText, entry.Name);
            SetText(durationText, entry.Duration);
        }

        private void SetDeathSaveTexts(CharacterDeathSaveData deathSaves)
        {
            CharacterDeathSaveData normalized = CharacterDeathSaveData.Clone(deathSaves);
            SetText(m_tmpDeathSaveSuccessesCount, FormatDeathSaveCountText(normalized.Successes));
            SetText(m_tmpDeathSaveFailuresCount, FormatDeathSaveCountText(normalized.Failures));
        }

        private void SetDeathSaveTexts(CharacterSheetDisplayViewState state)
        {
            SetText(m_tmpDeathSaveSuccessesCount, ExtractDeathSaveCountText(state?.DeathSaveSuccessesText));
            SetText(m_tmpDeathSaveFailuresCount, ExtractDeathSaveCountText(state?.DeathSaveFailuresText));
        }

        private static string FormatDeathSaveCountText(int value)
        {
            return $"{Math.Max(0, Math.Min(3, value))}/3";
        }

        private static string ExtractDeathSaveCountText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string trimmed = value.Trim();
            int separatorIndex = trimmed.LastIndexOf(' ');
            return separatorIndex >= 0 && separatorIndex < trimmed.Length - 1
                ? trimmed.Substring(separatorIndex + 1)
                : trimmed;
        }

        private void SetHitDiceTexts(CharacterCardDraftSaveData character)
        {
            List<CharacterHitDicePoolSaveData> pools = CharacterDetailDisplayService.Instance.BuildDisplayHitDicePools(character);
            if (pools == null || pools.Count == 0)
            {
                SetText(m_tmpHitDiceDie, "x0");
                SetText(m_tmpHitDiceRemaining, "-");
                return;
            }

            int remaining = 0;
            List<string> dice = new List<string>();
            for (int index = 0; index < pools.Count; index++)
            {
                CharacterHitDicePoolSaveData pool = CharacterHitDicePoolSaveData.Clone(pools[index]);
                if (pool.DieSize <= 0 && pool.Total <= 0)
                {
                    continue;
                }

                remaining += pool.Remaining;
                if (pool.DieSize > 0)
                {
                    string dieText = $"d{pool.DieSize}";
                    if (!dice.Contains(dieText))
                    {
                        dice.Add(dieText);
                    }
                }
            }

            SetText(m_tmpHitDiceDie, $"x{Math.Max(0, remaining)}");
            SetText(m_tmpHitDiceRemaining, dice.Count > 0 ? string.Join("/", dice) : "-");
        }

        private void SetHitDiceTexts(CharacterSheetDisplayViewState state)
        {
            SetText(m_tmpHitDiceDie, state?.HitDiceCountText ?? string.Empty);
            SetText(m_tmpHitDiceRemaining, state?.HitDiceDieText ?? string.Empty);
        }

        private void SetRoleplayTexts(CharacterRoleplayProfileSaveData profile)
        {
            CharacterRoleplayProfileSaveData normalized = CharacterRoleplayProfileSaveData.Clone(profile);
            SetText(m_tmpPersonalityTraits, normalized.PersonalityTraits);
            SetText(m_tmpIdeals, normalized.Ideals);
            SetText(m_tmpFlaws, normalized.Flaws);
        }

        private void SetBackgroundText(CharacterRuntimeSnapshotData snapshot)
        {
            SetText(m_tmpBackgroundFeatureBackgroundName, FormatTextOrDefault(snapshot?.BackgroundName, "背景"));
        }

        private void ApplyPortrait(string previewPath)
        {
            bool hasPreview = TryLoadPortrait(previewPath);
            if (m_imgCharacterPortrait != null)
            {
                m_imgCharacterPortrait.color = hasPreview ? Color.white : new Color(0.05f, 0.06f, 0.08f, 1f);
            }
        }

        private bool TryLoadPortrait(string previewPath)
        {
            if (string.IsNullOrWhiteSpace(previewPath) || !File.Exists(previewPath))
            {
                ClearLoadedPortrait();
                return false;
            }

            if (string.Equals(m_loadedPortraitPath, previewPath, StringComparison.OrdinalIgnoreCase)
                && m_loadedPortraitSprite != null)
            {
                if (m_imgCharacterPortrait != null)
                {
                    m_imgCharacterPortrait.sprite = m_loadedPortraitSprite;
                }

                return true;
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(previewPath);
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!texture.LoadImage(bytes))
                {
                    UnityEngine.Object.Destroy(texture);
                    ClearLoadedPortrait();
                    return false;
                }

                ClearLoadedPortrait();
                m_loadedPortraitTexture = texture;
                m_loadedPortraitSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                m_loadedPortraitPath = previewPath;
                if (m_imgCharacterPortrait != null)
                {
                    m_imgCharacterPortrait.sprite = m_loadedPortraitSprite;
                    m_imgCharacterPortrait.preserveAspect = true;
                }

                return true;
            }
            catch (Exception exception)
            {
                Log.Warning($"CharacterCardManagementUI: failed to load character portrait. {exception.Message}");
                ClearLoadedPortrait();
                return false;
            }
        }

        private void ClearLoadedPortrait()
        {
            if (m_imgCharacterPortrait != null)
            {
                m_imgCharacterPortrait.sprite = null;
            }

            if (m_loadedPortraitSprite != null)
            {
                UnityEngine.Object.Destroy(m_loadedPortraitSprite);
            }

            if (m_loadedPortraitTexture != null)
            {
                UnityEngine.Object.Destroy(m_loadedPortraitTexture);
            }

            m_loadedPortraitSprite = null;
            m_loadedPortraitTexture = null;
            m_loadedPortraitPath = string.Empty;
        }

        private void SetSkillBonus(int index, int bonus)
        {
            if (index < 0 || index >= m_tmpSkillBonuses.Length)
            {
                return;
            }

            SetText(m_tmpSkillBonuses[index], FormatSignedNumber(bonus));
        }

        private void SetSkillBackground(int index, bool hasProficiency, bool hasExpertise)
        {
            if (index < 0 || index >= m_imgSkillBackgrounds.Length)
            {
                return;
            }

            Image image = m_imgSkillBackgrounds[index];
            if (image == null)
            {
                return;
            }

            if (hasExpertise)
            {
                image.color = SkillExpertiseBackgroundColor;
            }
            else if (hasProficiency)
            {
                image.color = SkillProficiencyBackgroundColor;
            }
            else
            {
                image.color = m_defaultSkillBackgroundColors[index];
            }
        }

        private void RefreshClassSections(CharacterCardDraftSaveData character)
        {
            List<CharacterClassDetailDisplayState> sections = CharacterDetailDisplayService.Instance.BuildClassDetailSections(character);
            EnsureClassSectionViewCount(sections.Count);

            for (int index = 0; index < m_classSectionViews.Count; index++)
            {
                CharacterClassSectionView view = m_classSectionViews[index];
                bool active = index < sections.Count;
                view.SetActive(active);
                if (!active)
                {
                    continue;
                }

                CharacterClassDetailDisplayState section = sections[index];
                view.Bind(
                    section.ClassName,
                    section.SubclassName,
                    section.LevelText,
                    section.Features);
            }
        }

        private void EnsureClassSectionViewCount(int count)
        {
            if (m_classSectionViews.Count == 0 || m_goClassSectionTemplate == null || m_rectClassFeatureContent == null || m_transformClassSectionParent == null)
            {
                return;
            }

            while (m_classSectionViews.Count < count)
            {
                CharacterClassSectionView previous = m_classSectionViews[m_classSectionViews.Count - 1];
                int insertIndex = previous.ContentTransform != null
                    ? previous.ContentTransform.GetSiblingIndex() + 1
                    : m_transformClassSectionParent.childCount;

                GameObject sectionObject = UnityEngine.Object.Instantiate(m_goClassSectionTemplate, m_transformClassSectionParent);
                sectionObject.name = $"m_sectionClass_{m_classSectionViews.Count + 1}";
                sectionObject.transform.SetSiblingIndex(insertIndex);

                GameObject contentObject = UnityEngine.Object.Instantiate(m_rectClassFeatureContent.gameObject, m_transformClassSectionParent);
                contentObject.name = $"m_rectClassFeatureContent_{m_classSectionViews.Count + 1}";
                contentObject.transform.SetSiblingIndex(insertIndex + 1);
                RemoveGeneratedClassFeatureItems(contentObject.transform);

                m_classSectionViews.Add(CharacterClassSectionView.Bind(sectionObject, contentObject, ShowFeatureDetail));
            }
        }

        private static void RemoveGeneratedClassFeatureItems(Transform content)
        {
            if (content == null)
            {
                return;
            }

            for (int index = content.childCount - 1; index >= 0; index--)
            {
                Transform child = content.GetChild(index);
                if (child.name == "m_itemClassFeatureTemplate")
                {
                    continue;
                }

                child.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }

        private void RefreshEquipmentToolItems(CharacterRuntimeSnapshotData snapshot)
        {
            List<string> entries = CharacterDetailDisplayService.Instance.BuildEquipmentAndToolEntries(snapshot);
            RefreshEquipmentToolItems(entries);
        }

        private void RefreshEquipmentToolItems(IReadOnlyList<string> entries)
        {
            if (entries == null)
            {
                entries = Array.Empty<string>();
            }
            EnsureEquipmentToolItemCount(entries.Count);

            for (int index = 0; index < m_equipmentToolItems.Count; index++)
            {
                GameObject item = m_equipmentToolItems[index];
                bool active = index < entries.Count;
                SetActive(item, active);
                if (active)
                {
                    SetEquipmentToolItemLabel(item, entries[index]);
                }
            }

            SetActive(m_goEquipmentToolTemplate, false);
        }

        private void EnsureEquipmentToolItemCount(int count)
        {
            if (m_rectEquipmentToolContent == null || m_goEquipmentToolTemplate == null)
            {
                return;
            }

            while (m_equipmentToolItems.Count < count)
            {
                GameObject itemObject = UnityEngine.Object.Instantiate(m_goEquipmentToolTemplate, m_rectEquipmentToolContent);
                itemObject.name = $"m_itemEquipmentTool_{m_equipmentToolItems.Count + 1}";
                SetActive(itemObject, true);
                m_equipmentToolItems.Add(itemObject);
            }
        }

        private static void SetEquipmentToolItemLabel(GameObject item, string value)
        {
            if (item == null)
            {
                return;
            }

            TMP_Text label = item.transform.Find("m_tmpEquipmentToolLabel")?.GetComponent<TMP_Text>();

            if (label != null)
            {
                label.text = value ?? string.Empty;
            }
        }

        private void RefreshInventoryItems(CharacterEquipmentSetSaveData equipment)
        {
            List<CharacterInventoryDisplayEntry> entries = CharacterDetailDisplayService.Instance.BuildInventoryEntries(equipment);
            RefreshInventoryItems(entries);
        }

        private void RefreshInventoryItems(IReadOnlyList<CharacterInventoryDisplayEntry> entries)
        {
            ExitInventoryAddMode(false);
            if (entries == null)
            {
                entries = Array.Empty<CharacterInventoryDisplayEntry>();
            }
            EnsureInventoryItemCount(entries.Count);

            for (int index = 0; index < m_inventoryItems.Count; index++)
            {
                GameObject item = m_inventoryItems[index];
                bool active = index < entries.Count;
                SetActive(item, active);
                if (active)
                {
                    SetInventoryItem(item, entries[index]);
                    BindInventoryItemDetailButton(item, entries[index]);
                }
            }

            SetActive(m_goInventoryItemTemplate, false);
        }

        private void EnsureInventoryItemCount(int count)
        {
            if (m_rectInventoryContent == null || m_goInventoryItemTemplate == null)
            {
                return;
            }

            while (m_inventoryItems.Count < count)
            {
                GameObject itemObject = UnityEngine.Object.Instantiate(m_goInventoryItemTemplate, m_rectInventoryContent);
                itemObject.name = $"m_itemInventory_{m_inventoryItems.Count + 1}";
                SetActive(itemObject, true);
                m_inventoryItems.Add(itemObject);
            }
        }

        private static void SetInventoryItem(GameObject item, CharacterInventoryDisplayEntry entry)
        {
            if (item == null)
            {
                return;
            }

            TMP_Text label = item.transform.Find("m_tmpInventoryItemTitle")?.GetComponent<TMP_Text>();
            if (label != null)
            {
                label.text = entry.Label;
            }

            TMP_Text quantity = item.transform.Find("m_tmpInventoryNum")?.GetComponent<TMP_Text>();
            SetText(quantity, $"X{Math.Max(1, entry.Quantity)}");

            Transform equippedMark = item.transform.Find("m_tmpInventoryEquippedMark");
            SetActive(equippedMark != null ? equippedMark.gameObject : null, entry.IsEquipped);
        }

        private void BindInventoryItemDetailButton(GameObject item, CharacterInventoryDisplayEntry entry)
        {
            if (item == null)
            {
                return;
            }

            Button button = item.GetComponent<Button>();
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnClickInventoryItem(entry));
        }

        private void OnClickInventoryItem(CharacterInventoryDisplayEntry entry)
        {
            if (!HasSelectedCharacter())
            {
                ShowFeatureDetail(entry.Title, entry.Description);
                return;
            }

            CharacterCardDraftSaveData character = m_characterCards[m_selectedCharacterIndex];
            CharacterEquipmentItemSaveData item = FindInventoryItem(character?.Equipment, entry.ItemInstanceId);
            ShowInventoryItemDetail(entry, item);
        }

        private void ShowInventoryItemDetail(CharacterInventoryDisplayEntry entry, CharacterEquipmentItemSaveData item)
        {
            SetText(m_tmpFeatureDetailTitle, entry.Title);
            SetText(m_tmpFeatureDetailDescription, entry.Description);
            m_visibleInventoryItemInstanceId = entry.ItemInstanceId;
            m_visibleInventoryQuickRollContext = null;
            m_visibleInventoryQuickRollResult = null;
            m_visibleInventoryQuickRollHistoryEntryId = string.Empty;
            m_visibleInventoryQuickRollCharacterId = string.Empty;
            RefreshInventoryActionButtons(item);
        }

        private void RefreshInventoryActionButtons(CharacterEquipmentItemSaveData item)
        {
            if (item == null)
            {
                HideInventoryActionButtons();
                return;
            }

            SetActive(m_panelInventoryActionButtons, true);
            if (m_tmpInputInventoryUseAmount != null)
            {
                m_tmpInputInventoryUseAmount.SetTextWithoutNotify("1");
            }

            bool equippable = IsInventoryItemEquippable(item);
            bool requiresAttunement = item.RequiresAttunement || item.IsAttuned;
            bool usable = IsInventoryItemUsable(item);
            bool canRestoreCharges = CanRestoreInventoryItemCharges(item);

            SetActive(m_goInventoryUseAmount, false);
            SetActive(m_tmpInventoryUseAmountLabel != null ? m_tmpInventoryUseAmountLabel.gameObject : null, false);
            SetActive(m_tmpInputInventoryUseAmount != null ? m_tmpInputInventoryUseAmount.gameObject : null, false);
            SetInventoryActionButton(m_btnInventoryUseAction, m_tmpInventoryUseActionLabel, "使用", usable);
            SetInventoryActionButton(m_btnInventoryRestoreChargesAction, m_tmpInventoryRestoreChargesActionLabel, "恢复充能", canRestoreCharges);
            SetInventoryActionButton(m_btnInventoryEquipAction, m_tmpInventoryEquipActionLabel, item.IsEquipped ? "卸下" : "装备", equippable);
            SetInventoryActionButton(m_btnInventoryAttuneAction, m_tmpInventoryAttuneActionLabel, item.IsAttuned ? "解除同调" : "同调", requiresAttunement);
            SetInventoryActionButton(m_btnInventoryRemoveAction, m_tmpInventoryRemoveActionLabel, "移除1个", true);
            SetActive(m_btnDiceHistory != null ? m_btnDiceHistory.gameObject : null, HasSelectedCharacter());
        }

        private void HideInventoryActionButtons()
        {
            m_visibleInventoryItemInstanceId = string.Empty;
            bool showHistoryButton = HasSelectedCharacter() && m_btnDiceHistory != null;
            SetActive(m_panelInventoryActionButtons, showHistoryButton);
            if (m_tmpInputInventoryUseAmount != null)
            {
                m_tmpInputInventoryUseAmount.SetTextWithoutNotify("1");
            }
            SetActive(m_goInventoryUseAmount, false);
            SetActive(m_tmpInventoryUseAmountLabel != null ? m_tmpInventoryUseAmountLabel.gameObject : null, false);
            SetActive(m_tmpInputInventoryUseAmount != null ? m_tmpInputInventoryUseAmount.gameObject : null, false);
            SetActive(m_btnInventoryUseAction != null ? m_btnInventoryUseAction.gameObject : null, false);
            SetActive(m_btnInventoryRestoreChargesAction != null ? m_btnInventoryRestoreChargesAction.gameObject : null, false);
            SetActive(m_btnInventoryEquipAction != null ? m_btnInventoryEquipAction.gameObject : null, false);
            SetActive(m_btnInventoryAttuneAction != null ? m_btnInventoryAttuneAction.gameObject : null, false);
            SetActive(m_btnInventoryRemoveAction != null ? m_btnInventoryRemoveAction.gameObject : null, false);
            SetActive(m_btnDiceHistory != null ? m_btnDiceHistory.gameObject : null, showHistoryButton);
        }

        private static void SetInventoryActionButton(Button button, TMP_Text label, string text, bool active)
        {
            SetActive(button != null ? button.gameObject : null, active);
            SetText(label, text);
        }

        private static bool IsInventoryItemEquippable(CharacterEquipmentItemSaveData item)
        {
            return CharacterItemTypeBehaviorUtility.IsInventoryItemEquippable(item);
        }

        private static bool IsInventoryItemUsable(CharacterEquipmentItemSaveData item)
        {
            return CharacterItemTypeBehaviorUtility.IsInventoryItemUsable(item);
        }

        private static bool CanRestoreInventoryItemCharges(CharacterEquipmentItemSaveData item)
        {
            return CharacterItemTypeBehaviorUtility.CanRestoreInventoryItemCharges(item);
        }

        private void OnClickInventoryUseAction()
        {
            if (!TryGetVisibleInventoryItem(out CharacterCardDraftSaveData character, out CharacterEquipmentItemSaveData item))
            {
                return;
            }

            if (!IsInventoryItemUsable(item))
            {
                SetInventoryActionFeedback("物品操作", "该物品当前不可使用。");
                return;
            }

            OpenInventoryUseDiceRollUI(character, item);
        }

        private void OnClickInventoryRestoreChargesAction()
        {
            if (!TryGetVisibleInventoryItem(out CharacterCardDraftSaveData character, out CharacterEquipmentItemSaveData item))
            {
                return;
            }

            if (!CanRestoreInventoryItemCharges(item))
            {
                SetInventoryActionFeedback("物品操作", "该物品当前不需要恢复充能。");
                return;
            }

            CharacterOperationResult result = CharacterInventoryApplicationService.Instance.RestoreCharacterItemCharges(
                character.CharacterId,
                item.ItemInstanceId);
            RefreshAfterCharacterInventoryAction(character.CharacterId, item.ItemInstanceId, result, "物品充能已恢复。");
        }

        private void OpenInventoryUseDiceRollUI(CharacterCardDraftSaveData character, CharacterEquipmentItemSaveData item)
        {
            if (character == null || item == null)
            {
                return;
            }

            CharacterInventoryQuickRollContext context = BuildInventoryQuickRollContext(item)
                ?? BuildManualInventoryRollContext(item);
            string currentDescription = m_tmpFeatureDetailDescription != null
                ? m_tmpFeatureDetailDescription.text
                : string.Empty;

            Log.Info($"打开 DiceRollUI：{context.ItemName} / {context.EffectName} / {context.DiceExpression}");
            m_visibleInventoryQuickRollContext = context;
            m_visibleInventoryQuickRollResult = null;
            m_visibleInventoryQuickRollHistoryEntryId = string.Empty;
            m_visibleInventoryQuickRollCharacterId = character.CharacterId ?? string.Empty;

            SetText(m_tmpFeatureDetailTitle, "使用物品");
            SetText(m_tmpFeatureDetailDescription, BuildInventoryQuickRollPendingText(context, currentDescription));
            GameModule.UI.ShowUIAsync<DiceRollUI>(new DiceRollUIRequest
            {
                SourceType = "inventory_item",
                SourceId = context.ItemInstanceId,
                SourceName = context.ItemName,
                EffectName = context.EffectName,
                EffectDescription = context.EffectDescription,
                DiceExpression = context.DiceExpression,
                OnResult = OnInventoryDiceRollUIResult
            });
        }

        private void OnClickInventoryEquipAction()
        {
            if (!TryGetVisibleInventoryItem(out CharacterCardDraftSaveData character, out CharacterEquipmentItemSaveData item))
            {
                return;
            }

            if (!IsInventoryItemEquippable(item))
            {
                SetInventoryActionFeedback("物品操作", "该物品当前不可装备。");
                return;
            }

            bool wasEquipped = item.IsEquipped;
            CharacterOperationResult result = wasEquipped
                ? CharacterInventoryApplicationService.Instance.UnequipCharacterItem(character.CharacterId, item.ItemInstanceId)
                : CharacterInventoryApplicationService.Instance.EquipCharacterItem(character.CharacterId, item.ItemInstanceId);
            RefreshAfterCharacterInventoryAction(character.CharacterId, item.ItemInstanceId, result, wasEquipped ? "物品已卸下。" : "物品已装备。");
        }

        private void OnClickInventoryAttuneAction()
        {
            if (!TryGetVisibleInventoryItem(out CharacterCardDraftSaveData character, out CharacterEquipmentItemSaveData item))
            {
                return;
            }

            if (!item.RequiresAttunement && !item.IsAttuned)
            {
                SetInventoryActionFeedback("物品操作", "该物品不需要同调。");
                return;
            }

            bool wasAttuned = item.IsAttuned;
            CharacterOperationResult result = wasAttuned
                ? CharacterInventoryApplicationService.Instance.UnattuneCharacterItem(character.CharacterId, item.ItemInstanceId)
                : CharacterInventoryApplicationService.Instance.AttuneCharacterItem(character.CharacterId, item.ItemInstanceId);
            RefreshAfterCharacterInventoryAction(character.CharacterId, item.ItemInstanceId, result, wasAttuned ? "物品已解除同调。" : "物品已同调。");
        }

        private void OnClickInventoryRemoveAction()
        {
            if (!TryGetVisibleInventoryItem(out CharacterCardDraftSaveData character, out CharacterEquipmentItemSaveData item))
            {
                return;
            }

            CharacterOperationResult result = CharacterInventoryApplicationService.Instance.RemoveItemFromCharacter(
                character.CharacterId,
                item.ItemInstanceId,
                1);
            RefreshAfterCharacterInventoryAction(character.CharacterId, item.ItemInstanceId, result, "已移除 1 个物品。");
        }

        private void OnClickDiceHistory()
        {
            if (!HasSelectedCharacter())
            {
                HideInventoryActionButtons();
                SetText(m_tmpFeatureDetailTitle, "掷骰记录");
                SetText(m_tmpFeatureDetailDescription, "请先选择一个角色。");
                return;
            }

            CharacterCardDraftSaveData character = m_characterCards[m_selectedCharacterIndex];
            SetText(m_tmpFeatureDetailTitle, "掷骰记录");
            SetText(m_tmpFeatureDetailDescription, BuildCharacterDiceHistorySummary(character));
            HideInventorySpecificActionButtons();
        }

        private void HideInventorySpecificActionButtons()
        {
            SetActive(m_panelInventoryActionButtons, m_btnDiceHistory != null);
            SetActive(m_goInventoryUseAmount, false);
            SetActive(m_tmpInventoryUseAmountLabel != null ? m_tmpInventoryUseAmountLabel.gameObject : null, false);
            SetActive(m_tmpInputInventoryUseAmount != null ? m_tmpInputInventoryUseAmount.gameObject : null, false);
            SetActive(m_btnInventoryUseAction != null ? m_btnInventoryUseAction.gameObject : null, false);
            SetActive(m_btnInventoryRestoreChargesAction != null ? m_btnInventoryRestoreChargesAction.gameObject : null, false);
            SetActive(m_btnInventoryEquipAction != null ? m_btnInventoryEquipAction.gameObject : null, false);
            SetActive(m_btnInventoryAttuneAction != null ? m_btnInventoryAttuneAction.gameObject : null, false);
            SetActive(m_btnInventoryRemoveAction != null ? m_btnInventoryRemoveAction.gameObject : null, false);
            SetActive(m_btnDiceHistory != null ? m_btnDiceHistory.gameObject : null, m_btnDiceHistory != null);
        }

        private bool TryGetVisibleInventoryItem(out CharacterCardDraftSaveData character, out CharacterEquipmentItemSaveData item)
        {
            character = null;
            item = null;
            if (!HasSelectedCharacter() || string.IsNullOrWhiteSpace(m_visibleInventoryItemInstanceId))
            {
                HideInventoryActionButtons();
                return false;
            }

            character = m_characterCards[m_selectedCharacterIndex];
            item = FindInventoryItem(character?.Equipment, m_visibleInventoryItemInstanceId);
            if (item != null)
            {
                return true;
            }

            HideInventoryActionButtons();
            SetText(m_tmpFeatureDetailTitle, "物品操作");
            SetText(m_tmpFeatureDetailDescription, "当前物品不存在，可能已经被移除。");
            return false;
        }

        private void RefreshAfterCharacterInventoryAction(string characterId, string itemInstanceId, CharacterOperationResult result, string successMessage)
        {
            if (result == null || !result.Success)
            {
                SetInventoryActionFeedback("物品操作失败", result == null || string.IsNullOrWhiteSpace(result.Message) ? "物品操作失败。" : result.Message);
                return;
            }

            RefreshCharacterRuntimeSnapshot(characterId);
            LoadCharacterCards();
            RestoreSelectedCharacter(characterId);
            RefreshCharacterListView();
            if (TryFindInventoryDisplayEntry(itemInstanceId, out CharacterInventoryDisplayEntry entry))
            {
                CharacterCardDraftSaveData character = HasSelectedCharacter() ? m_characterCards[m_selectedCharacterIndex] : null;
                CharacterEquipmentItemSaveData updatedItem = FindInventoryItem(character?.Equipment, itemInstanceId);
                ShowInventoryItemDetail(entry, updatedItem);
                if (!string.IsNullOrWhiteSpace(successMessage))
                {
                    SetText(m_tmpFeatureDetailDescription, $"{entry.Description}\n\n{successMessage}");
                }
                return;
            }

            HideInventoryActionButtons();
            SetText(m_tmpFeatureDetailTitle, "物品操作");
            SetText(m_tmpFeatureDetailDescription, successMessage);
        }

        private static void RefreshCharacterRuntimeSnapshot(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId)
                || !CharacterApplicationService.Instance.TryGetCharacter(characterId, out CharacterCardDraftSaveData character)
                || character == null)
            {
                return;
            }

            character.RuntimeSnapshot = CharacterDetailCalculationService.Instance.BuildDisplaySnapshot(character);
            CharacterApplicationService.Instance.Save(character);
        }

        private bool TryFindInventoryDisplayEntry(string itemInstanceId, out CharacterInventoryDisplayEntry entry)
        {
            entry = default;
            if (!HasSelectedCharacter())
            {
                return false;
            }

            CharacterCardDraftSaveData character = m_characterCards[m_selectedCharacterIndex];
            List<CharacterInventoryDisplayEntry> entries = CharacterDetailDisplayService.Instance.BuildInventoryEntries(character?.Equipment);
            string normalizedId = itemInstanceId?.Trim() ?? string.Empty;
            for (int index = 0; index < entries.Count; index++)
            {
                CharacterInventoryDisplayEntry candidate = entries[index];
                if (string.Equals(candidate.ItemInstanceId, normalizedId, StringComparison.OrdinalIgnoreCase))
                {
                    entry = candidate;
                    return true;
                }
            }

            return false;
        }

        private void SetInventoryActionFeedback(string title, string description)
        {
            SetText(m_tmpFeatureDetailTitle, FormatTextOrDefault(title, "物品操作"));
            SetText(m_tmpFeatureDetailDescription, description ?? string.Empty);
        }

        private void OnClickAddInventoryItem()
        {
            if (!HasSelectedCharacter())
            {
                OpenCharacterPopup("添加物品", "请先选择一个角色。", "关闭", false, false);
                return;
            }

            ShowLocalInventoryItemOptions();
        }

        private void ShowLocalInventoryItemOptions()
        {
            if (m_panelCharacterPopup == null
                || m_rectCharacterPopupOptionContentRoot == null
                || m_goCharacterPopupOptionTemplate == null
                || m_rectCharacterPopupSelectedContentRoot == null
                || m_goCharacterPopupSelectedTemplate == null)
            {
                Log.Warning("角色卡管理：通用弹窗缺失，无法显示本地物品库。");
                ShowFeatureDetail("添加物品", "通用弹窗节点缺失，无法打开本地物品库。");
                return;
            }

            m_showingLocalItemOptions = true;
            m_popupMode = CharacterCardPopupMode.OptionList;
            m_pendingLocalItemId = string.Empty;
            ClearPendingLocalItemSelection();
            m_localItemOptions = LoadLocalItemOptions();
            OpenCharacterPopup("添加物品", "点击物品卡片加入预览区，最后点击确认加入当前角色背包。", "确认", true, false);
            RefreshLocalItemOptionCards();
            RefreshLocalItemSelectedPreviewCards();

            if (m_localItemOptions.Count == 0)
            {
                SetCharacterPopupDescription("当前没有已保存的本地物品。请先在物品编辑界面保存物品。");
                return;
            }
        }

        private void RefreshLocalItemOptionCards()
        {
            if (!m_showingLocalItemOptions)
            {
                SetCharacterPopupOptionItemsActive(false);
                return;
            }

            EnsureLocalItemOptionItemCount(m_localItemOptions.Count);
            for (int index = 0; index < m_characterPopupOptionItems.Count; index++)
            {
                GameObject item = m_characterPopupOptionItems[index];
                bool active = index < m_localItemOptions.Count;
                SetActive(item, active);
                if (!active)
                {
                    continue;
                }

                LocalCustomItemSaveData option = m_localItemOptions[index];
                string customItemId = option?.CustomItemId?.Trim() ?? string.Empty;
                bool selected = GetPendingLocalItemCount(customItemId) > 0;
                SetLocalItemOption(item, option, selected);
                BindLocalItemOptionButton(item, customItemId);
            }

            SetActive(m_goCharacterPopupOptionTemplate, false);
        }

        private void EnsureLocalItemOptionItemCount(int count)
        {
            if (m_rectCharacterPopupOptionContentRoot == null || m_goCharacterPopupOptionTemplate == null)
            {
                return;
            }

            while (m_characterPopupOptionItems.Count < count)
            {
                GameObject itemObject = UnityEngine.Object.Instantiate(m_goCharacterPopupOptionTemplate, m_rectCharacterPopupOptionContentRoot);
                itemObject.name = $"m_itemCharacterPopupOption_{m_characterPopupOptionItems.Count + 1}";
                SetActive(itemObject, true);
                m_characterPopupOptionItems.Add(itemObject);
            }
        }

        private void RefreshLocalItemSelectedPreviewCards()
        {
            EnsureLocalItemSelectedPreviewItemCount(m_pendingLocalItemOrder.Count);
            for (int index = 0; index < m_characterPopupSelectedItems.Count; index++)
            {
                GameObject item = m_characterPopupSelectedItems[index];
                bool active = index < m_pendingLocalItemOrder.Count;
                SetActive(item, active);
                if (!active)
                {
                    continue;
                }

                string customItemId = m_pendingLocalItemOrder[index];
                LocalCustomItemSaveData option = FindLocalItemOption(customItemId);
                SetLocalItemSelectedPreviewItem(item, option, GetPendingLocalItemCount(customItemId));
                BindLocalItemSelectedPreviewButton(item, customItemId);
            }

            SetActive(m_goCharacterPopupSelectedTemplate, false);
        }

        private void EnsureLocalItemSelectedPreviewItemCount(int count)
        {
            if (m_rectCharacterPopupSelectedContentRoot == null || m_goCharacterPopupSelectedTemplate == null)
            {
                return;
            }

            while (m_characterPopupSelectedItems.Count < count)
            {
                GameObject itemObject = UnityEngine.Object.Instantiate(m_goCharacterPopupSelectedTemplate, m_rectCharacterPopupSelectedContentRoot);
                itemObject.name = $"m_itemCharacterPopupSelected_{m_characterPopupSelectedItems.Count + 1}";
                SetActive(itemObject, true);
                m_characterPopupSelectedItems.Add(itemObject);
            }
        }

        private void SetLocalItemSelectedPreviewItem(GameObject item, LocalCustomItemSaveData option, int count)
        {
            if (item == null)
            {
                return;
            }

            TMP_Text label = FirstComponent<TMP_Text>(item.transform, "m_tmpPopupSelectedItemTitle");
            TMP_Text quantity = FirstComponent<TMP_Text>(item.transform, "m_tmpPopupSelectedItemCount");
            SetText(label, BuildLocalItemOptionLabel(option));
            SetText(quantity, $"X{Math.Max(1, count)}");
        }

        private void BindLocalItemSelectedPreviewButton(GameObject item, string customItemId)
        {
            if (item == null)
            {
                return;
            }

            Button button = item.GetComponent<Button>();
            if (button == null)
            {
                Log.Warning("角色卡管理：弹窗已选物品模板缺少 Button 组件，无法点击减少物品数量。");
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnClickLocalItemSelectedPreview(customItemId));
        }

        private void OnClickLocalItemSelectedPreview(string customItemId)
        {
            string normalizedId = customItemId?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedId))
            {
                SetCharacterPopupDescription("请选择一个有效的已选物品。");
                return;
            }

            RemovePendingLocalItem(normalizedId);
            LocalCustomItemSaveData item = FindLocalItemOption(normalizedId);
            int remainingCount = GetPendingLocalItemCount(normalizedId);
            if (item != null && remainingCount > 0)
            {
                SetCharacterPopupDescription($"{BuildLocalItemDetailDescription(item)}\n\n待添加数量：X{remainingCount}");
            }
            else
            {
                SetCharacterPopupDescription("已从预览区减少 1 个物品。点击左侧物品卡片可继续添加。");
            }

            RefreshLocalItemOptionCards();
            RefreshLocalItemSelectedPreviewCards();
        }

        private void SetLocalItemOption(GameObject item, LocalCustomItemSaveData option, bool selected)
        {
            if (item == null)
            {
                return;
            }

            CharacterEquipmentItemSaveData data = option?.Item;
            TMP_Text label = FirstComponent<TMP_Text>(item.transform,
                "m_tmpPopupOptionTitle",
                "m_tmpInventoryItemTitle");
            TMP_Text subtitle = FirstComponent<TMP_Text>(item.transform,
                "m_tmpPopupOptionSubtitle");
            TMP_Text quantity = FirstComponent<TMP_Text>(item.transform,
                "m_tmpPopupOptionCount",
                "m_tmpInventoryNum");
            Transform selectedMark = FirstTransform(item.transform,
                "m_imgPopupOptionSelected",
                "m_tmpInventoryEquippedMark");
            SetText(label, BuildLocalItemOptionLabel(option));
            SetText(subtitle, FirstNonEmpty(data?.ItemType, data?.Rarity));
            SetText(quantity, $"X{Math.Max(1, data?.Quantity ?? 1)}");
            SetActive(selectedMark != null ? selectedMark.gameObject : null, selected);
        }

        private void BindLocalItemOptionButton(GameObject item, string customItemId)
        {
            if (item == null)
            {
                return;
            }

            Button button = item.GetComponent<Button>();
            if (button == null)
            {
                Log.Warning("角色卡管理：弹窗物品模板缺少 Button 组件，无法点击选择物品。");
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnClickLocalItemOption(customItemId));
        }

        private void OnClickLocalItemOption(string customItemId)
        {
            string normalizedId = customItemId?.Trim() ?? string.Empty;
            LocalCustomItemSaveData item = FindLocalItemOption(normalizedId);
            if (item != null)
            {
                AddPendingLocalItem(normalizedId);
                m_pendingLocalItemId = normalizedId;
                ShowLocalItemDetail(item);
            }
            else
            {
                SetCharacterPopupDescription("请选择一个有效的本地物品。");
            }

            RefreshLocalItemOptionCards();
            RefreshLocalItemSelectedPreviewCards();
        }

        private void ConfirmPendingLocalItemSelection()
        {
            if (!HasSelectedCharacter())
            {
                SetCharacterPopupDescription("请先选择一个角色。");
                return;
            }

            if (m_pendingLocalItemOrder.Count == 0)
            {
                SetCharacterPopupDescription("请先点击物品卡片，将物品加入预览区。");
                return;
            }

            CharacterCardDraftSaveData character = m_characterCards[m_selectedCharacterIndex];
            string selectedCharacterId = character?.CharacterId?.Trim() ?? string.Empty;
            int addedItemKinds = 0;
            int addedItemCount = 0;
            for (int index = 0; index < m_pendingLocalItemOrder.Count; index++)
            {
                string customItemId = m_pendingLocalItemOrder[index];
                int quantity = GetPendingLocalItemCount(customItemId);
                if (quantity <= 0)
                {
                    continue;
                }

                LocalCustomItemSaveData item = FindLocalItemOption(customItemId);
                if (item == null)
                {
                    SetCharacterPopupDescription("待添加物品不存在，请重新选择。");
                    return;
                }

                CharacterEquipmentItemSaveData snapshot = LocalCustomItemRepository.CreateCharacterItemSnapshot(item, quantity);
                CharacterInventoryOperationResult inventoryResult = AddInventoryItemToCharacter(character, snapshot, snapshot.Quantity);
                if (!inventoryResult.Success)
                {
                    SetCharacterPopupDescription(string.IsNullOrWhiteSpace(inventoryResult.Message) ? "物品加入角色背包失败。" : inventoryResult.Message);
                    return;
                }

                addedItemKinds++;
                addedItemCount += quantity;
            }

            character.RuntimeSnapshot = CharacterDetailCalculationService.Instance.BuildDisplaySnapshot(character);
            CharacterOperationResult saveResult = CharacterApplicationService.Instance.Save(character);
            if (!saveResult.Success)
            {
                SetCharacterPopupDescription(string.IsNullOrWhiteSpace(saveResult.Message) ? "角色保存失败。" : saveResult.Message);
                return;
            }

            ExitInventoryAddMode(false);
            CloseCharacterPopup();
            LoadCharacterCards();
            RestoreSelectedCharacter(selectedCharacterId);
            RefreshCharacterListView();
            ShowFeatureDetail("添加物品", $"已将 {addedItemKinds} 种、共 {addedItemCount} 个物品加入当前角色背包。");
        }

        private static CharacterInventoryOperationResult AddInventoryItemToCharacter(
            CharacterCardDraftSaveData character,
            CharacterEquipmentItemSaveData item,
            int quantity)
        {
            if (character == null)
            {
                return new CharacterInventoryOperationResult
                {
                    Success = false,
                    Message = "Character data is empty."
                };
            }

            if (CharacterItemCategoryUtility.IsCurrencyItem(item))
            {
                if (character.Currency == null)
                {
                    character.Currency = new CharacterCurrencySaveData();
                }
                int amount = CharacterItemCategoryUtility.AddCurrency(character.Currency, item, quantity);
                return new CharacterInventoryOperationResult
                {
                    Success = amount > 0,
                    Message = amount > 0 ? "Currency added." : "Currency item data is invalid.",
                    Equipment = CharacterEquipmentSetSaveData.Clone(character.Equipment)
                };
            }

            if (character.Equipment == null)
            {
                character.Equipment = new CharacterEquipmentSetSaveData();
            }
            CharacterInventoryOperationResult result = CharacterInventoryApplicationService.Instance.AddItem(
                character.Equipment,
                item,
                Math.Max(1, quantity));
            if (result.Success)
            {
                character.Equipment = CharacterEquipmentSetSaveData.Clone(result.Equipment);
            }

            return result;
        }

        private void ExitInventoryAddMode(bool restoreInventory)
        {
            m_showingLocalItemOptions = false;
            m_pendingLocalItemId = string.Empty;
            ClearPendingLocalItemSelection();
            SetCharacterPopupOptionItemsActive(false);
            SetCharacterPopupSelectedItemsActive(false);
            if (restoreInventory && HasSelectedCharacter())
            {
                CharacterCardDraftSaveData character = m_characterCards[m_selectedCharacterIndex];
                RefreshInventoryItems(character?.Equipment);
            }
        }

        private void SetInventoryItemsActive(bool active)
        {
            for (int index = 0; index < m_inventoryItems.Count; index++)
            {
                SetActive(m_inventoryItems[index], active);
            }
        }

        private void SetCharacterPopupOptionItemsActive(bool active)
        {
            for (int index = 0; index < m_characterPopupOptionItems.Count; index++)
            {
                SetActive(m_characterPopupOptionItems[index], active);
            }
        }

        private void SetCharacterPopupSelectedItemsActive(bool active)
        {
            for (int index = 0; index < m_characterPopupSelectedItems.Count; index++)
            {
                SetActive(m_characterPopupSelectedItems[index], active);
            }
        }

        private void AddPendingLocalItem(string customItemId)
        {
            string normalizedId = customItemId?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedId))
            {
                return;
            }

            if (!m_pendingLocalItemCounts.ContainsKey(normalizedId))
            {
                m_pendingLocalItemOrder.Add(normalizedId);
                m_pendingLocalItemCounts[normalizedId] = 0;
            }

            m_pendingLocalItemCounts[normalizedId]++;
        }

        private void RemovePendingLocalItem(string customItemId)
        {
            string normalizedId = customItemId?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedId)
                || !m_pendingLocalItemCounts.TryGetValue(normalizedId, out int count))
            {
                return;
            }

            count--;
            if (count > 0)
            {
                m_pendingLocalItemCounts[normalizedId] = count;
                return;
            }

            m_pendingLocalItemCounts.Remove(normalizedId);
            for (int index = m_pendingLocalItemOrder.Count - 1; index >= 0; index--)
            {
                if (string.Equals(m_pendingLocalItemOrder[index], normalizedId, StringComparison.OrdinalIgnoreCase))
                {
                    m_pendingLocalItemOrder.RemoveAt(index);
                    break;
                }
            }

            if (string.Equals(m_pendingLocalItemId, normalizedId, StringComparison.OrdinalIgnoreCase))
            {
                m_pendingLocalItemId = m_pendingLocalItemOrder.Count > 0
                    ? m_pendingLocalItemOrder[m_pendingLocalItemOrder.Count - 1]
                    : string.Empty;
            }
        }

        private int GetPendingLocalItemCount(string customItemId)
        {
            string normalizedId = customItemId?.Trim() ?? string.Empty;
            return !string.IsNullOrWhiteSpace(normalizedId) && m_pendingLocalItemCounts.TryGetValue(normalizedId, out int count)
                ? Math.Max(0, count)
                : 0;
        }

        private void ClearPendingLocalItemSelection()
        {
            m_pendingLocalItemOrder.Clear();
            m_pendingLocalItemCounts.Clear();
            SetCharacterPopupSelectedItemsActive(false);
        }

        private void OpenCharacterPopup(string title, string description, string confirmText, bool showList, bool showForm)
        {
            SetActive(m_panelCharacterPopup, true);
            if (m_panelCharacterPopup != null)
            {
                m_panelCharacterPopup.transform.SetAsLastSibling();
            }

            SetCharacterPopupTitle(title);
            SetCharacterPopupDescription(description);
            SetText(m_tmpCharacterPopupConfirm, confirmText);
            SetActive(m_goCharacterPopupList, showList);
            SetActive(m_panelCharacterPopupForm, showForm);
            SetActive(m_btnCharacterPopupConfirm != null ? m_btnCharacterPopupConfirm.gameObject : null, !string.IsNullOrWhiteSpace(confirmText));
            SetActive(m_goCharacterPopupOptionTemplate, false);
            SetActive(m_goCharacterPopupSelectedTemplate, false);
            ResetPopupScrollPositions();
        }

        private void CloseCharacterPopup()
        {
            m_popupMode = CharacterCardPopupMode.None;
            m_showingLocalItemOptions = false;
            m_pendingLocalItemId = string.Empty;
            ClearPendingLocalItemSelection();
            SetCharacterPopupOptionItemsActive(false);
            SetCharacterPopupSelectedItemsActive(false);
            SetActive(m_panelCharacterPopup, false);
        }

        private void SetCharacterPopupTitle(string value)
        {
            SetText(m_tmpCharacterPopupTitle, FormatTextOrDefault(value, "信息"));
        }

        private void SetCharacterPopupDescription(string value)
        {
            SetText(m_tmpCharacterPopupDescription, value ?? string.Empty);
        }

        private void OnClickCharacterPopupConfirm()
        {
            switch (m_popupMode)
            {
                case CharacterCardPopupMode.OptionList:
                    ConfirmPendingLocalItemSelection();
                    break;
                case CharacterCardPopupMode.Form:
                    ConfirmCustomFeaturePopup();
                    break;
                default:
                    CloseCharacterPopup();
                    break;
            }
        }

        private void OpenCustomFeaturePopup()
        {
            if (!HasSelectedCharacter())
            {
                ShowFeatureDetail("添加自定义特性", "请先选择一个角色。");
                return;
            }

            m_popupMode = CharacterCardPopupMode.Form;
            OpenCharacterPopup("添加特性", "输入特性名称与描述，确认后会保存到当前角色。", "添加", false, true);
            if (m_inputCharacterPopupName != null)
            {
                m_inputCharacterPopupName.SetTextWithoutNotify(string.Empty);
                m_inputCharacterPopupName.Select();
                m_inputCharacterPopupName.ActivateInputField();
            }

            if (m_inputCharacterPopupDescription != null)
            {
                m_inputCharacterPopupDescription.SetTextWithoutNotify(string.Empty);
            }
        }

        private void ConfirmCustomFeaturePopup()
        {
            if (!HasSelectedCharacter())
            {
                SetCharacterPopupDescription("请先选择一个角色。");
                return;
            }

            string featureName = m_inputCharacterPopupName != null ? m_inputCharacterPopupName.text?.Trim() ?? string.Empty : string.Empty;
            string description = m_inputCharacterPopupDescription != null ? m_inputCharacterPopupDescription.text?.Trim() ?? string.Empty : string.Empty;
            if (string.IsNullOrWhiteSpace(featureName) && string.IsNullOrWhiteSpace(description))
            {
                SetCharacterPopupDescription("自定义特性的名称和描述不能同时为空。");
                return;
            }

            CharacterCardDraftSaveData character = m_characterCards[m_selectedCharacterIndex];
            string selectedCharacterId = character?.CharacterId?.Trim() ?? string.Empty;
            if (character.CustomFeatures == null)
            {
                character.CustomFeatures = new List<CharacterCustomFeatureSaveData>();
            }
            character.CustomFeatures.Add(new CharacterCustomFeatureSaveData
            {
                Name = featureName,
                Description = description
            });
            character.RuntimeSnapshot = CharacterDetailCalculationService.Instance.BuildDisplaySnapshot(character);
            CharacterOperationResult saveResult = CharacterApplicationService.Instance.Save(character);
            if (!saveResult.Success)
            {
                SetCharacterPopupDescription(string.IsNullOrWhiteSpace(saveResult.Message) ? "角色保存失败。" : saveResult.Message);
                return;
            }

            CloseCharacterPopup();
            LoadCharacterCards();
            RestoreSelectedCharacter(selectedCharacterId);
            RefreshCharacterListView();
            ShowFeatureDetail(FormatTextOrDefault(featureName, "自定义特性"), description);
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

        private void ShowLocalItemDetail(LocalCustomItemSaveData item)
        {
            SetCharacterPopupDescription(BuildLocalItemDetailDescription(item));
        }

        private static string BuildLocalItemOptionLabel(LocalCustomItemSaveData item)
        {
            CharacterEquipmentItemSaveData data = item?.Item;
            if (!string.IsNullOrWhiteSpace(data?.ItemName))
            {
                return data.ItemName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(data?.ItemId))
            {
                return data.ItemId.Trim();
            }

            return !string.IsNullOrWhiteSpace(item?.CustomItemId) ? item.CustomItemId.Trim() : "未命名物品";
        }

        private static string BuildLocalItemDetailDescription(LocalCustomItemSaveData item)
        {
            CharacterEquipmentItemSaveData data = item?.Item;
            if (data == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            AppendDetailLine(builder, "类型", data.ItemType);
            AppendDetailLine(builder, "稀有度", data.Rarity);
            AppendDetailLine(builder, "价格", data.PriceGp > 0 ? $"{data.PriceGp} gp" : string.Empty);
            AppendDetailLine(builder, "装备栏位", data.EquipmentSlot);
            AppendDetailLine(builder, "需要同调", data.RequiresAttunement ? "是" : string.Empty);
            AppendDetailLine(builder, "护甲类型", data.ArmorCategory);
            if (data.ArmorBaseAc > 0)
            {
                AppendDetailLine(builder, "基础AC", data.ArmorBaseAc.ToString());
            }

            if (data.AcBonus != 0)
            {
                AppendDetailLine(builder, "AC加值", FormatSignedNumber(data.AcBonus));
            }

            AppendDetailLine(builder, "武器类型", data.WeaponCategory);
            AppendDetailLine(builder, "伤害", BuildLocalItemDamageText(data));
            AppendDetailLine(builder, "工具类型", data.ToolCategory);
            if (data.MaxCharges > 0)
            {
                AppendDetailLine(builder, "最大充能", data.MaxCharges.ToString());
            }

            AppendDetailLine(builder, "生效条件", data.EffectApplyCondition);
            AppendDetailLine(builder, "描述", data.Description);
            AppendDetailLine(builder, "效果", BuildLocalItemEffectsText(data));
            AppendDetailLine(builder, "备注", data.Notes);
            return builder.ToString().Trim();
        }

        private static string BuildLocalItemDamageText(CharacterEquipmentItemSaveData data)
        {
            if (data == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(data.DamageDice) && string.IsNullOrWhiteSpace(data.DamageType))
            {
                return string.Empty;
            }

            return string.IsNullOrWhiteSpace(data.DamageType)
                ? data.DamageDice.Trim()
                : $"{data.DamageDice?.Trim()} {data.DamageType.Trim()}".Trim();
        }

        private static string BuildLocalItemEffectsText(CharacterEquipmentItemSaveData data)
        {
            if (data == null)
            {
                return string.Empty;
            }

            List<string> entries = new List<string>();
            if (data.EffectIds != null)
            {
                for (int index = 0; index < data.EffectIds.Count; index++)
                {
                    string effectId = data.EffectIds[index];
                    if (!string.IsNullOrWhiteSpace(effectId))
                    {
                        entries.Add(effectId.Trim());
                    }
                }
            }

            if (data.CustomEffects != null)
            {
                for (int index = 0; index < data.CustomEffects.Count; index++)
                {
                    CharacterItemEffectSaveData effect = data.CustomEffects[index];
                    if (CharacterItemEffectSaveData.HasContent(effect))
                    {
                        entries.Add(FirstNonEmpty(effect.Name, effect.EffectType));
                    }
                }
            }

            return entries.Count > 0 ? string.Join("、", entries) : string.Empty;
        }

        private void OnInventoryDiceRollUIResult(DiceRollUIResult result)
        {
            if (result == null
                || m_visibleInventoryQuickRollContext == null
                || string.IsNullOrWhiteSpace(m_visibleInventoryQuickRollCharacterId)
                || (!string.IsNullOrWhiteSpace(m_visibleInventoryQuickRollContext.ItemInstanceId)
                    && !string.Equals(m_visibleInventoryQuickRollContext.ItemInstanceId, result.SourceId?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            bool isNewRollResult = !ReferenceEquals(m_visibleInventoryQuickRollResult, result.RollResult);
            m_visibleInventoryQuickRollResult = result.RollResult;
            CharacterInventoryQuickRollPurpose purpose = ConvertDiceRollPurpose(result.Purpose);
            string purposeDisplayName = GetQuickRollPurposeDisplayName(purpose);
            if (isNewRollResult || string.IsNullOrWhiteSpace(m_visibleInventoryQuickRollHistoryEntryId))
            {
                CharacterDiceRollHistoryEntry historyEntry = CharacterApplicationService.Instance.AddDiceRollHistoryEntry(
                    m_visibleInventoryQuickRollCharacterId,
                    m_visibleInventoryQuickRollContext,
                    m_visibleInventoryQuickRollResult,
                    purposeDisplayName);
                m_visibleInventoryQuickRollHistoryEntryId = historyEntry?.EntryId ?? string.Empty;
            }
            else
            {
                CharacterApplicationService.Instance.UpdateDiceRollHistoryPurpose(
                    m_visibleInventoryQuickRollCharacterId,
                    m_visibleInventoryQuickRollHistoryEntryId,
                    purposeDisplayName);
            }

            RefreshSelectedCharacterFromRepository();
            string resultText = BuildInventoryQuickRollResultText(
                m_visibleInventoryQuickRollContext,
                m_visibleInventoryQuickRollResult,
                purpose);

            SetText(m_tmpFeatureDetailTitle, "使用物品");
            SetText(m_tmpFeatureDetailDescription, resultText);
        }

        private void RefreshSelectedCharacterFromRepository()
        {
            if (!HasSelectedCharacter())
            {
                return;
            }

            string characterId = m_characterCards[m_selectedCharacterIndex].CharacterId;
            if (!CharacterApplicationService.Instance.TryGetCharacter(characterId, out CharacterCardDraftSaveData updatedCharacter))
            {
                return;
            }

            m_characterCards[m_selectedCharacterIndex] = updatedCharacter;
            if (m_selectedCharacterIndex < m_characterListItems.Count)
            {
                m_characterListItems[m_selectedCharacterIndex] = CharacterViewStateBuilder.BuildListItem(updatedCharacter);
            }

            RefreshOtherFeatureSection(updatedCharacter);
        }

        private static CharacterEquipmentItemSaveData FindInventoryItem(CharacterEquipmentSetSaveData equipment, string itemInstanceId)
        {
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

            string normalized = itemInstanceId.Trim();
            for (int index = 0; index < items.Count; index++)
            {
                CharacterEquipmentItemSaveData item = items[index];
                if (item != null && string.Equals(item.ItemInstanceId, normalized, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }

            return null;
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
                    EffectDescription = BuildInventoryEffectDescription(effect),
                    DiceExpression = diceExpression
                };
            }

            return null;
        }

        private static CharacterInventoryQuickRollContext BuildManualInventoryRollContext(CharacterEquipmentItemSaveData item)
        {
            return new CharacterInventoryQuickRollContext
            {
                ItemInstanceId = item?.ItemInstanceId ?? string.Empty,
                ItemName = FirstNonEmpty(item?.ItemName, item?.ItemId),
                EffectName = "手动掷骰",
                EffectDescription = item?.Description ?? string.Empty,
                DiceExpression = "1d20"
            };
        }

        private static string BuildInventoryEffectDescription(CharacterItemEffectSaveData effect)
        {
            return effect?.Description?.Trim() ?? string.Empty;
        }

        private string BuildInventoryQuickRollPendingText(CharacterInventoryQuickRollContext context, string itemDescription)
        {
            if (context == null)
            {
                return itemDescription ?? string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            AppendDetailLine(builder, "来源物品", context.ItemName);
            AppendDetailLine(builder, "词条", context.EffectName);
            AppendDetailLine(builder, "词条说明", context.EffectDescription);
            AppendDetailLine(builder, "骰子表达式", context.DiceExpression);
            AppendDetailLine(builder, "状态", "已打开掷骰弹窗，请在弹窗中完成掷骰。");
            if (!string.IsNullOrWhiteSpace(itemDescription))
            {
                builder.AppendLine();
                builder.Append(itemDescription.Trim());
            }

            return builder.ToString();
        }

        private string BuildInventoryQuickRollResultText(
            CharacterInventoryQuickRollContext context,
            CharacterDiceRollResultData result,
            CharacterInventoryQuickRollPurpose purpose)
        {
            if (context == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            AppendDetailLine(builder, "来源物品", context.ItemName);
            AppendDetailLine(builder, "词条", context.EffectName);
            AppendDetailLine(builder, "词条说明", context.EffectDescription);
            AppendDetailLine(builder, "骰子表达式", context.DiceExpression);
            if (result == null || !result.Success)
            {
                AppendDetailLine(builder, "掷骰失败", result?.Error ?? "未知掷骰错误。");
                AppendCharacterDiceRollHistory(builder);
                return builder.ToString();
            }

            AppendDetailLine(builder, "掷骰结果", result.Summary);
            AppendDetailLine(builder, "总值", result.Total.ToString());
            AppendInventoryQuickRollPurposeText(builder, purpose, result);
            AppendCharacterDiceRollHistory(builder);
            return builder.ToString();
        }

        private void AppendInventoryQuickRollPurposeText(
            StringBuilder builder,
            CharacterInventoryQuickRollPurpose purpose,
            CharacterDiceRollResultData result)
        {
            if (builder == null || result == null || !result.Success || !HasSelectedCharacter())
            {
                return;
            }

            CharacterCardDraftSaveData character = m_characterCards[m_selectedCharacterIndex];
            CharacterRuntimeSnapshotData snapshot = CharacterDetailCalculationService.Instance.BuildDisplaySnapshot(character);
            CharacterCombatOverviewViewState overview = CharacterDetailCalculationService.Instance.BuildCombatOverview(character, snapshot);

            AppendDetailLine(builder, "用途", GetQuickRollPurposeDisplayName(purpose));
            switch (purpose)
            {
                case CharacterInventoryQuickRollPurpose.HealHp:
                    AppendDetailLine(builder, "当前生命值", $"{Math.Max(0, snapshot.CurrentHp)}/{Math.Max(0, snapshot.MaxHp)}");
                    AppendDetailLine(builder, "恢复后预览", $"{Math.Max(0, snapshot.CurrentHp)} + {result.Total} = {Math.Min(Math.Max(0, snapshot.MaxHp), Math.Max(0, snapshot.CurrentHp) + result.Total)}");
                    AppendDetailLine(builder, "说明", "角色卡界面暂不自动应用恢复，结果已记录到掷骰历史。");
                    break;
                case CharacterInventoryQuickRollPurpose.AttackHit:
                    AppendDetailLine(builder, "当前通用命中加值", FormatSignedNumber(snapshot.AttackBonus));
                    AppendDetailLine(builder, "当前武器命中加值", FormatSignedNumber(snapshot.WeaponAttackBonus));
                    AppendDetailLine(builder, "通用命中预览", BuildTotalWithBonusPreview(result.Total, snapshot.AttackBonus));
                    AppendDetailLine(builder, "物品武器命中预览", BuildTotalWithBonusPreview(result.Total, snapshot.AttackBonus + snapshot.WeaponAttackBonus));
                    break;
                case CharacterInventoryQuickRollPurpose.Damage:
                    AppendDetailLine(builder, "当前伤害加值", FormatSignedNumber(snapshot.DamageBonus));
                    AppendDetailLine(builder, "伤害预览", BuildTotalWithBonusPreview(result.Total, snapshot.DamageBonus));
                    break;
                case CharacterInventoryQuickRollPurpose.SkillCheck:
                    AppendDetailLine(builder, "技能结果预览", BuildSkillRollPreviewText(character, snapshot, result.Total));
                    break;
                case CharacterInventoryQuickRollPurpose.SavingThrow:
                    AppendDetailLine(builder, "当前豁免通用加值", FormatSignedNumber(snapshot.SavingThrowBonus));
                    AppendDetailLine(builder, "通用豁免预览", BuildTotalWithBonusPreview(result.Total, snapshot.SavingThrowBonus));
                    break;
                case CharacterInventoryQuickRollPurpose.SpellAttack:
                    AppendDetailLine(builder, "当前法术攻击加值", FormatSignedNumber(overview.SpellAttackBonus));
                    AppendDetailLine(builder, "法术攻击预览", BuildTotalWithBonusPreview(result.Total, overview.SpellAttackBonus));
                    break;
                case CharacterInventoryQuickRollPurpose.SpellSaveDc:
                    AppendDetailLine(builder, "当前法术豁免DC", overview.SpellSaveDc > 0 ? overview.SpellSaveDc.ToString() : "-");
                    AppendDetailLine(builder, "说明", "法术豁免DC通常不与掷骰总值相加，这里仅显示当前最终DC供判断。");
                    break;
                case CharacterInventoryQuickRollPurpose.Custom:
                    AppendDetailLine(builder, "说明", "自定义用途由玩家和DM根据物品描述判断。");
                    break;
                default:
                    AppendDetailLine(builder, "说明", "仅显示本次掷骰结果，不应用到角色数据。");
                    break;
            }
        }

        private void AppendCharacterDiceRollHistory(StringBuilder builder)
        {
            if (builder == null || !HasSelectedCharacter())
            {
                return;
            }

            CharacterCardDraftSaveData character = m_characterCards[m_selectedCharacterIndex];
            builder.AppendLine();
            builder.AppendLine(CharacterDiceRollHistoryFormatter.BuildRecentHistoryText(character?.DiceRollHistory, 5, true));
        }

        private static string BuildCharacterDiceHistorySummary(CharacterCardDraftSaveData character)
        {
            if (character?.DiceRollHistory == null || character.DiceRollHistory.Count == 0)
            {
                return "暂无掷骰记录。";
            }

            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < character.DiceRollHistory.Count; index++)
            {
                CharacterDiceRollHistorySaveData entry = character.DiceRollHistory[index];
                if (entry == null)
                {
                    continue;
                }

                string itemName = FirstNonEmpty(entry.SourceItemName, entry.SourceEffectName);
                if (string.IsNullOrWhiteSpace(itemName))
                {
                    itemName = "手动掷骰";
                }

                string diceExpression = string.IsNullOrWhiteSpace(entry.DiceExpression) ? "-" : entry.DiceExpression.Trim();
                string finalValue = entry.Success
                    ? entry.Total.ToString()
                    : FormatTextOrDefault(entry.Error, "失败");

                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                builder.Append(itemName);
                builder.Append("-");
                builder.Append(diceExpression);
                builder.Append("-");
                builder.Append(finalValue);
            }

            return builder.Length > 0 ? builder.ToString() : "暂无掷骰记录。";
        }

        private void RefreshRaceFeatureSection(CharacterCardDraftSaveData character, CharacterRuntimeSnapshotData snapshot, string raceId)
        {
            SetRaceFeatureHeader(snapshot, raceId);

            List<ClassFeatureDisplayEntry> entries = CharacterDetailDisplayService.Instance.BuildRaceFeatureEntries(character, raceId);
            EnsureRaceFeatureItemCount(entries.Count);

            for (int index = 0; index < m_raceFeatureItems.Count; index++)
            {
                GameObject item = m_raceFeatureItems[index];
                bool active = index < entries.Count;
                SetActive(item, active);
                if (active)
                {
                    SetRaceFeatureItem(item, entries[index]);
                }
            }

            SetActive(m_goRaceFeatureTemplate, false);
        }

        private void SetRaceFeatureHeader(CharacterRuntimeSnapshotData snapshot, string raceId)
        {
            CharacterRaceFeatureHeaderState state = CharacterDetailDisplayService.Instance.BuildRaceFeatureHeader(snapshot, raceId);
            SetText(m_tmpRaceFeatureRaceName, FormatTextOrDefault(state.MainRaceName, "种族"));
            SetText(m_tmpRaceFeatureSubRaceName, state.SubRaceName);
        }

        private void RefreshOtherFeatureSection(CharacterCardDraftSaveData character)
        {
            SetText(m_tmpOtherFeatureSectionTitle, "其他特性");

            List<ClassFeatureDisplayEntry> entries = CharacterDetailDisplayService.Instance.BuildOtherFeatureEntries(character);
            AppendDiceRollHistoryEntry(entries, character);
            EnsureOtherFeatureItemCount(entries.Count);

            for (int index = 0; index < m_otherFeatureItems.Count; index++)
            {
                GameObject item = m_otherFeatureItems[index];
                bool active = index < entries.Count;
                SetActive(item, active);
                if (active)
                {
                    SetOtherFeatureItem(item, entries[index]);
                }
            }

            SetActive(m_goOtherFeatureTemplate, false);
        }

        private static void AppendDiceRollHistoryEntry(List<ClassFeatureDisplayEntry> entries, CharacterCardDraftSaveData character)
        {
            if (entries == null)
            {
                return;
            }

            string historyText = CharacterDiceRollHistoryFormatter.BuildRecentHistoryText(
                character?.DiceRollHistory,
                8,
                true);
            entries.Add(new ClassFeatureDisplayEntry("最近掷骰", historyText));
        }

        private void EnsureRaceFeatureItemCount(int count)
        {
            if (m_rectRaceFeatureContent == null || m_goRaceFeatureTemplate == null)
            {
                return;
            }

            while (m_raceFeatureItems.Count < count)
            {
                GameObject itemObject = UnityEngine.Object.Instantiate(m_goRaceFeatureTemplate, m_rectRaceFeatureContent);
                itemObject.name = $"m_itemRaceFeature_{m_raceFeatureItems.Count + 1}";
                SetActive(itemObject, true);
                m_raceFeatureItems.Add(itemObject);
            }
        }

        private void RefreshSpellItems(CharacterCardDraftSaveData character)
        {
            List<CharacterCreationSpellCardViewState> spells = character != null
                ? CharacterCreationSpellDisplayService.Instance.BuildLearnedSpellCards(character)
                : new List<CharacterCreationSpellCardViewState>();
            EnsureSpellItemCount(spells.Count);

            for (int index = 0; index < m_spellItems.Count; index++)
            {
                GameObject item = m_spellItems[index];
                bool active = index < spells.Count;
                SetActive(item, active);
                if (active)
                {
                    SetSpellItem(item, spells[index]);
                    BindSpellDetailButton(item, spells[index]);
                }
            }

            SetActive(m_goSpellTemplate, false);
        }

        private void EnsureSpellItemCount(int count)
        {
            if (m_rectSpellContent == null || m_goSpellTemplate == null)
            {
                return;
            }

            while (m_spellItems.Count < count)
            {
                GameObject itemObject = UnityEngine.Object.Instantiate(m_goSpellTemplate, m_rectSpellContent);
                itemObject.name = $"m_itemSpell_{m_spellItems.Count + 1}";
                SetActive(itemObject, true);
                m_spellItems.Add(itemObject);
            }
        }

        private void EnsureOtherFeatureItemCount(int count)
        {
            if (m_rectOtherFeatureContent == null || m_goOtherFeatureTemplate == null)
            {
                return;
            }

            while (m_otherFeatureItems.Count < count)
            {
                GameObject itemObject = UnityEngine.Object.Instantiate(m_goOtherFeatureTemplate, m_rectOtherFeatureContent);
                itemObject.name = $"m_itemOtherFeature_{m_otherFeatureItems.Count + 1}";
                SetActive(itemObject, true);
                m_otherFeatureItems.Add(itemObject);
            }
        }

        private void SetRaceFeatureItem(GameObject item, ClassFeatureDisplayEntry entry)
        {
            TMP_Text title = item.transform.Find("m_tmpRaceFeatureTitle")?.GetComponent<TMP_Text>();
            TMP_Text description = item.transform.Find("m_tmpRaceFeatureDescription")?.GetComponent<TMP_Text>();
            Transform oldText = item.transform.Find("m_tmpRaceFeatures");
            SetActive(oldText != null ? oldText.gameObject : null, false);
            SetText(title, entry.Title);
            SetText(description, entry.Description);
            BindFeatureDetailButton(item, entry);
        }

        private void SetSpellItem(GameObject item, CharacterCreationSpellCardViewState spell)
        {
            if (item == null)
            {
                return;
            }

            TMP_Text title = item.transform.Find("m_tmpSpellTitle")?.GetComponent<TMP_Text>()
                ?? item.transform.Find("m_tmpSpellName")?.GetComponent<TMP_Text>();
            TMP_Text level = item.transform.Find("m_tmpSpellLevel")?.GetComponent<TMP_Text>();
            TMP_Text school = item.transform.Find("m_tmpSpellSchool")?.GetComponent<TMP_Text>();
            SetText(title, spell?.Name ?? string.Empty);
            SetText(level, spell?.LevelText ?? string.Empty);
            SetText(school, spell?.SchoolText ?? string.Empty);
        }

        private void BindSpellDetailButton(GameObject item, CharacterCreationSpellCardViewState spell)
        {
            if (item == null)
            {
                return;
            }

            Button button = item.GetComponent<Button>();
            if (button == null)
            {
                return;
            }

            string spellId = spell?.SpellId ?? string.Empty;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnClickSpellCard(spellId));
        }

        private void OnClickSpellCard(string spellId)
        {
            if (string.IsNullOrWhiteSpace(spellId))
            {
                ShowFeatureDetail("法术详情", string.Empty);
                return;
            }

            ShowFeatureDetail(
                CharacterCreationSpellDisplayService.Instance.GetSpellDetailTitle(spellId),
                CharacterCreationSpellDisplayService.Instance.GetSpellDetailDescription(spellId));
        }

        private void SetOtherFeatureItem(GameObject item, ClassFeatureDisplayEntry entry)
        {
            TMP_Text title = item.transform.Find("m_tmpOtherFeatureTitle")?.GetComponent<TMP_Text>();
            SetText(title, entry.Title);
            BindFeatureDetailButton(item, entry);
        }

        private void BindFeatureDetailButton(GameObject item, ClassFeatureDisplayEntry entry)
        {
            if (item == null)
            {
                return;
            }

            Button button = item.GetComponent<Button>();
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ShowFeatureDetail(entry));
        }

        private void ShowFeatureDetail(ClassFeatureDisplayEntry entry)
        {
            ShowFeatureDetail(entry.Title, entry.Description);
        }

        private void ShowFeatureDetail(string title, string description)
        {
            HideInventoryActionButtons();
            SetText(m_tmpFeatureDetailTitle, FormatTextOrDefault(title, "特性详情"));
            SetText(m_tmpFeatureDetailDescription, description ?? string.Empty);
        }

        private void OnClickSelectCharacterCard(int characterIndex)
        {
            if (characterIndex < 0 || characterIndex >= m_characterCards.Count)
            {
                return;
            }

            m_selectedCharacterIndex = characterIndex;
            RefreshCharacterListView();
        }

        private void OnClickCreateCharacter()
        {
            GameModule.UI.CloseUI<CharacterCardManagementUI>();
            GameModule.UI.ShowUIAsync<CharacterCreationUI>();
        }

        private void OnClickToggleSkillProficiencies()
        {
            if (m_rectSkillProficiencies == null)
            {
                return;
            }

            GameObject targetObject = m_rectSkillProficiencies.gameObject;
            targetObject.SetActive(!targetObject.activeSelf);
        }

        private void OnClickToggleEquipmentTools()
        {
            if (m_rectEquipmentToolContent == null)
            {
                return;
            }

            GameObject targetObject = m_rectEquipmentToolContent.gameObject;
            targetObject.SetActive(!targetObject.activeSelf);
        }

        private void OnClickToggleOtherFeatures()
        {
            if (m_rectOtherFeatureContent == null)
            {
                return;
            }

            GameObject targetObject = m_rectOtherFeatureContent.gameObject;
            targetObject.SetActive(!targetObject.activeSelf);
        }

        private void OnClickToggleRaceFeatures()
        {
            if (m_rectRaceFeatureContent == null)
            {
                return;
            }

            GameObject targetObject = m_rectRaceFeatureContent.gameObject;
            targetObject.SetActive(!targetObject.activeSelf);
        }

        private void OnClickToggleSpells()
        {
            if (m_rectSpellContent == null)
            {
                return;
            }

            GameObject targetObject = m_rectSpellContent.gameObject;
            targetObject.SetActive(!targetObject.activeSelf);
        }

        private void OnClickToggleInventory()
        {
            if (m_rectInventoryContent == null)
            {
                return;
            }

            GameObject targetObject = m_rectInventoryContent.gameObject;
            targetObject.SetActive(!targetObject.activeSelf);
        }

        private void OnClickEditCharacterCard(int characterIndex)
        {
            if (characterIndex < 0 || characterIndex >= m_characterCards.Count)
            {
                return;
            }

            m_selectedCharacterIndex = characterIndex;
            RefreshCharacterListView();
            Log.Info("角色卡管理界面：角色编辑功能已拆分，等待重新开发独立编辑界面。");
        }

        private void OnClickDeleteSelectedCharacter()
        {
            if (!HasSelectedCharacter())
            {
                return;
            }

            string characterId = m_characterCards[m_selectedCharacterIndex].CharacterId;
            CharacterApplicationService.Instance.Delete(characterId);
            LoadCharacterCards();
            RefreshCharacterListView();
        }

        private bool HasSelectedCharacter()
        {
            return m_selectedCharacterIndex >= 0 && m_selectedCharacterIndex < m_characterCards.Count;
        }

        private static string FormatSignedNumber(int value)
        {
            return value >= 0 ? $"+{value}" : value.ToString();
        }

        private static string BuildTotalWithBonusPreview(int rollTotal, int bonus)
        {
            int total = rollTotal + bonus;
            return bonus == 0
                ? total.ToString()
                : $"{rollTotal} {(bonus >= 0 ? "+" : "-")} {Math.Abs(bonus)} = {total}";
        }

        private static string BuildSkillRollPreviewText(CharacterCardDraftSaveData character, CharacterRuntimeSnapshotData snapshot, int rollTotal)
        {
            if (snapshot == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < SkillDisplayBindings.Length; index++)
            {
                SkillDisplayBinding binding = SkillDisplayBindings[index];
                CharacterSkillBonusViewState state = CharacterDetailCalculationService.Instance.BuildSkillBonus(
                    character,
                    snapshot,
                    binding.SkillId,
                    binding.DisplayName,
                    binding.Ability);
                if (builder.Length > 0)
                {
                    builder.Append(index % 6 == 0 ? "\n" : "；");
                }

                builder.Append(binding.DisplayName);
                builder.Append(" ");
                builder.Append(BuildTotalWithBonusPreview(rollTotal, state.Bonus));
            }

            return builder.ToString();
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

        private static string FirstNonEmpty(string first, string second)
        {
            return string.IsNullOrWhiteSpace(first) ? second?.Trim() ?? string.Empty : first.Trim();
        }

        private static void AppendDetailLine(StringBuilder builder, string label, string value)
        {
            if (builder == null || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            builder.Append(label);
            builder.Append("：");
            builder.AppendLine(value.Trim());
        }

        private static string FormatTextOrDefault(string value, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
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

        private static void SetButtonInteractable(Button button, bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        private void CloseToHome()
        {
            GameModule.UI.CloseUI<CharacterCardManagementUI>();
            GameModule.UI.ShowUIAsync<HomeUI>();
        }

        private static T FirstComponent<T>(Transform root, params string[] paths) where T : Component
        {
            Transform target = FirstTransform(root, paths);
            return target != null ? target.GetComponent<T>() : null;
        }

        private static GameObject FirstGameObject(Transform root, params string[] paths)
        {
            Transform target = FirstTransform(root, paths);
            return target != null ? target.gameObject : null;
        }

        private static Transform FirstTransform(Transform root, params string[] paths)
        {
            if (root == null || paths == null)
            {
                return null;
            }

            for (int index = 0; index < paths.Length; index++)
            {
                string path = paths[index];
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                Transform target = root.Find(path);
                if (target != null)
                {
                    return target;
                }
            }

            return null;
        }

        private static RectTransform ResolveScrollContentRoot(RectTransform scrollArea, params string[] contentPaths)
        {
            if (scrollArea == null)
            {
                return null;
            }

            ScrollRect scrollRect = scrollArea.GetComponent<ScrollRect>();
            if (scrollRect != null && scrollRect.content != null)
            {
                return scrollRect.content;
            }

            RectTransform namedContent = FirstComponent<RectTransform>(scrollArea.transform, contentPaths);
            return namedContent != null ? namedContent : scrollArea;
        }

        private void ResetPopupScrollPositions()
        {
            ResetScrollPosition(m_rectCharacterPopupContent, true);
            ResetScrollPosition(m_rectCharacterPopupSelectedContent, false);
        }

        private static void ResetScrollPosition(RectTransform scrollArea, bool vertical)
        {
            ScrollRect scrollRect = scrollArea != null ? scrollArea.GetComponent<ScrollRect>() : null;
            if (scrollRect == null)
            {
                return;
            }

            if (vertical)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
            else
            {
                scrollRect.horizontalNormalizedPosition = 0f;
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
            {
                target.SetActive(active);
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

    internal sealed class CharacterClassSectionView
    {
        private readonly GameObject m_root;
        private readonly GameObject m_contentRoot;
        private readonly RectTransform m_contentTransform;
        private readonly GameObject m_featureTemplate;
        private readonly TMP_Text m_classNameText;
        private readonly TMP_Text m_subclassNameText;
        private readonly TMP_Text m_levelText;
        private readonly Button m_button;
        private readonly Action<ClassFeatureDisplayEntry> m_onClickFeatureDetail;
        private readonly List<GameObject> m_featureItems = new List<GameObject>();

        public Transform ContentTransform => m_contentTransform;

        private CharacterClassSectionView(
            GameObject root,
            GameObject contentRoot,
            RectTransform contentTransform,
            GameObject featureTemplate,
            TMP_Text classNameText,
            TMP_Text subclassNameText,
            TMP_Text levelText,
            Button button,
            Action<ClassFeatureDisplayEntry> onClickFeatureDetail)
        {
            m_root = root;
            m_contentRoot = contentRoot;
            m_contentTransform = contentTransform;
            m_featureTemplate = featureTemplate;
            m_classNameText = classNameText;
            m_subclassNameText = subclassNameText;
            m_levelText = levelText;
            m_button = button;
            m_onClickFeatureDetail = onClickFeatureDetail;
            SetActive(m_featureTemplate, false);
            BindToggleButton();
        }

        public static CharacterClassSectionView Bind(GameObject root, GameObject contentRoot, Action<ClassFeatureDisplayEntry> onClickFeatureDetail)
        {
            TMP_Text classNameText = root.transform.Find("m_panelClassFeatureHeader/m_tmpClassFeatureClassName")?.GetComponent<TMP_Text>();
            TMP_Text subclassNameText = root.transform.Find("m_panelClassFeatureHeader/m_tmpClassFeatureSubclassName")?.GetComponent<TMP_Text>();
            TMP_Text levelText = root.transform.Find("m_panelClassFeatureHeader/m_tmpClassLevel")?.GetComponent<TMP_Text>();
            Button button = root.GetComponent<Button>();
            RectTransform contentTransform = contentRoot != null ? contentRoot.GetComponent<RectTransform>() : null;
            GameObject featureTemplate = contentRoot != null
                ? contentRoot.transform.Find("m_itemClassFeatureTemplate")?.gameObject
                : null;

            return new CharacterClassSectionView(root, contentRoot, contentTransform, featureTemplate, classNameText, subclassNameText, levelText, button, onClickFeatureDetail);
        }

        public void Bind(string className, string subclassName, string level, IReadOnlyList<ClassFeatureDisplayEntry> features)
        {
            SetActive(true);
            SetText(m_classNameText, className);
            SetText(m_subclassNameText, subclassName);
            SetText(m_levelText, level);

            int count = features != null ? features.Count : 0;
            EnsureFeatureItemCount(count);
            for (int index = 0; index < m_featureItems.Count; index++)
            {
                GameObject item = m_featureItems[index];
                bool active = index < count;
                SetActive(item, active);
                if (active)
                {
                    SetFeatureItem(item, features[index]);
                }
            }

            SetActive(m_featureTemplate, false);
        }

        public void SetActive(bool active)
        {
            SetActive(m_root, active);
            SetActive(m_contentRoot, active);
        }

        private void BindToggleButton()
        {
            if (m_button == null)
            {
                return;
            }

            m_button.onClick.RemoveAllListeners();
            m_button.onClick.AddListener(ToggleContent);
        }

        private void ToggleContent()
        {
            if (m_contentRoot == null)
            {
                return;
            }

            m_contentRoot.SetActive(!m_contentRoot.activeSelf);
        }

        private void EnsureFeatureItemCount(int count)
        {
            if (m_featureTemplate == null || m_contentTransform == null)
            {
                return;
            }

            while (m_featureItems.Count < count)
            {
                GameObject itemObject = UnityEngine.Object.Instantiate(m_featureTemplate, m_contentTransform);
                itemObject.name = $"m_itemClassFeature_{m_featureItems.Count + 1}";
                SetActive(itemObject, true);
                m_featureItems.Add(itemObject);
            }
        }

        private void SetFeatureItem(GameObject item, ClassFeatureDisplayEntry entry)
        {
            TMP_Text title = item.transform.Find("m_tmpClassFeatureTitle")?.GetComponent<TMP_Text>();
            SetText(title, entry.Title);
            BindFeatureDetailButton(item, entry);
        }

        private void BindFeatureDetailButton(GameObject item, ClassFeatureDisplayEntry entry)
        {
            if (item == null || m_onClickFeatureDetail == null)
            {
                return;
            }

            Button button = item.GetComponent<Button>();
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => m_onClickFeatureDetail(entry));
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
            {
                target.SetActive(active);
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

    internal sealed class CharacterCardListItemView
    {
        private readonly GameObject m_root;
        private readonly Image m_background;
        private readonly Image m_previewImage;
        private readonly TMP_Text m_previewPlaceholder;
        private readonly TMP_Text m_nameText;
        private readonly TMP_Text m_classText;
        private readonly TMP_Text m_statusText;
        private readonly TMP_Text m_levelText;
        private readonly TMP_Text m_subClassText;
        private readonly TMP_Text m_classLevelText;
        private readonly TMP_Text m_noteText;
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
            TMP_Text levelText,
            TMP_Text subClassText,
            TMP_Text classLevelText,
            TMP_Text noteText,
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
            m_levelText = levelText;
            m_subClassText = subClassText;
            m_classLevelText = classLevelText;
            m_noteText = noteText;
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
            TMP_Text nameText = root.transform.Find("m_panelCardName/m_tmpCharacterName")?.GetComponent<TMP_Text>();
            TMP_Text statusText = root.transform.Find("m_tmpCharacterStatus")?.GetComponent<TMP_Text>();
            TMP_Text levelText = root.transform.Find("m_tmpCardLevel")?.GetComponent<TMP_Text>();
            Transform classItem = root.transform.Find("m_layoutCardClass/m_itemCardClassTemplate");
            TMP_Text classText = classItem?.Find("m_tmpCardClassName")?.GetComponent<TMP_Text>();
            TMP_Text subClassText = classItem?.Find("m_tmpCardSubclassName")?.GetComponent<TMP_Text>();
            TMP_Text classLevelText = classItem?.Find("m_tmpCardClassLevel")?.GetComponent<TMP_Text>();
            TMP_Text noteText = root.transform.Find("m_tmpCardNotes")?.GetComponent<TMP_Text>();
            if (statusText != null)
            {
                statusText.alignment = TextAlignmentOptions.Center;
            }

            return new CharacterCardListItemView(root, background, previewImage, placeholder, nameText, classText, statusText, levelText, subClassText, classLevelText, noteText, button, btnEdit);
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
            SetText(m_levelText, Math.Max(1, character.Level).ToString());
            SetText(m_subClassText, string.Empty);
            SetText(m_classLevelText, $"Lv.{Math.Max(1, character.Level)}");
            SetText(m_noteText, character.IsCompleted ? "已完成" : "未完成");
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
            if (string.IsNullOrWhiteSpace(previewPath) || !File.Exists(previewPath))
            {
                ClearLoadedPreview();
                return false;
            }

            if (string.Equals(m_loadedPreviewPath, previewPath, StringComparison.OrdinalIgnoreCase)
                && m_loadedPreviewSprite != null)
            {
                if (m_previewImage != null)
                {
                    m_previewImage.sprite = m_loadedPreviewSprite;
                }

                return true;
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(previewPath);
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!texture.LoadImage(bytes))
                {
                    UnityEngine.Object.Destroy(texture);
                    ClearLoadedPreview();
                    return false;
                }

                ClearLoadedPreview();
                m_loadedPreviewTexture = texture;
                m_loadedPreviewSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                m_loadedPreviewPath = previewPath;
                if (m_previewImage != null)
                {
                    m_previewImage.sprite = m_loadedPreviewSprite;
                    m_previewImage.preserveAspect = true;
                }

                return true;
            }
            catch (Exception exception)
            {
                Log.Warning($"角色卡管理：读取角色预览图失败。{exception.Message}");
                ClearLoadedPreview();
                return false;
            }
        }

        private void ClearLoadedPreview()
        {
            if (m_previewImage != null)
            {
                m_previewImage.sprite = null;
            }

            if (m_loadedPreviewSprite != null)
            {
                UnityEngine.Object.Destroy(m_loadedPreviewSprite);
            }

            if (m_loadedPreviewTexture != null)
            {
                UnityEngine.Object.Destroy(m_loadedPreviewTexture);
            }

            m_loadedPreviewSprite = null;
            m_loadedPreviewTexture = null;
            m_loadedPreviewPath = string.Empty;
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
