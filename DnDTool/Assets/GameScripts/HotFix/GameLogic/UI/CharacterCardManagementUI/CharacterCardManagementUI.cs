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
    internal enum AbilityKind
    {
        Strength,
        Dexterity,
        Constitution,
        Intelligence,
        Wisdom,
        Charisma
    }

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

    internal readonly struct CharacterInventoryDisplayEntry
    {
        public readonly string Label;
        public readonly string Title;
        public readonly string Description;
        public readonly bool IsEquipped;

        public CharacterInventoryDisplayEntry(string label, string title, string description, bool isEquipped)
        {
            Label = label ?? string.Empty;
            Title = title ?? string.Empty;
            Description = description ?? string.Empty;
            IsEquipped = isEquipped;
        }
    }

    internal readonly struct CharacterStatusEffectDisplayEntry
    {
        public readonly string Name;
        public readonly string Duration;

        public CharacterStatusEffectDisplayEntry(string name, string duration)
        {
            Name = name ?? string.Empty;
            Duration = duration ?? string.Empty;
        }
    }

    [Window(UILayer.UI, location: "CharacterCardManagementUI", fullScreen: true)]
    internal sealed class CharacterCardManagementUI : UIWindow
    {
        private const int DefaultAbilityScore = 10;
        private const int DefaultProficiencyBonus = 2;
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
        private TMP_Text m_tmpDeathSaveSuccesses;
        private TMP_Text m_tmpDeathSaveFailures;
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
        private Button m_btnSectionInventory;
        private RectTransform m_rectInventoryContent;
        private GameObject m_goInventoryItemTemplate;
        private TMP_Text m_tmpInventorySectionTitle;
        private TMP_Text m_tmpCurrentWeight;
        private TMP_Text m_tmpWeightLine;
        private TMP_Text m_tmpMaxWeight;
        private RectTransform m_gridStatusEffects;
        private GameObject m_goStatusEffectTemplate;
        private readonly TMP_Text[] m_tmpSkillBonuses = new TMP_Text[18];
        private readonly Image[] m_imgSkillBackgrounds = new Image[18];
        private readonly Color[] m_defaultSkillBackgroundColors = new Color[18];
        private readonly List<CharacterClassSectionView> m_classSectionViews = new List<CharacterClassSectionView>();
        private readonly List<GameObject> m_equipmentToolItems = new List<GameObject>();
        private readonly List<GameObject> m_raceFeatureItems = new List<GameObject>();
        private readonly List<GameObject> m_otherFeatureItems = new List<GameObject>();
        private readonly List<GameObject> m_inventoryItems = new List<GameObject>();
        private readonly List<GameObject> m_statusEffectItems = new List<GameObject>();

        private readonly List<CharacterCardDraftSaveData> m_characterCards = new List<CharacterCardDraftSaveData>();
        private readonly List<CharacterCardListItemView> m_cardViews = new List<CharacterCardListItemView>();
        private int m_selectedCharacterIndex = -1;
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
            m_tmpDeathSaveSuccesses = FindRightPanelComponent<TextMeshProUGUI>("m_imgDeathSavesBackground/m_tmpDeathSaveSuccesses");
            m_tmpDeathSaveFailures = FindRightPanelComponent<TextMeshProUGUI>("m_imgDeathSavesBackground/m_tmpDeathSaveFailures");
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
            m_tmpProficiencyBonus = GetBoundComponent<TextMeshProUGUI>(23, nameof(m_tmpProficiencyBonus));
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
            m_tmpClassLevel = GetBoundComponent<TextMeshProUGUI>(51, nameof(m_tmpClassLevel));
            m_rectClassFeatureContent = GetBoundComponent<RectTransform>(52, nameof(m_rectClassFeatureContent));
            m_goClassFeatureTemplate = GetBoundComponent<RectTransform>(53, nameof(m_goClassFeatureTemplate))?.gameObject;
            m_tmpClassFeatureClassName = GetBoundComponent<TextMeshProUGUI>(54, nameof(m_tmpClassFeatureClassName));
            m_tmpClassFeatureSubclassName = GetBoundComponent<TextMeshProUGUI>(55, nameof(m_tmpClassFeatureSubclassName));
            m_btnSectionInventory = GetBoundComponent<Button>(62, nameof(m_btnSectionInventory));
            m_rectInventoryContent = GetBoundComponent<RectTransform>(63, nameof(m_rectInventoryContent));
            m_goInventoryItemTemplate = GetBoundComponent<RectTransform>(64, nameof(m_goInventoryItemTemplate))?.gameObject;
            m_tmpInventorySectionTitle = GetBoundComponent<TextMeshProUGUI>(65, nameof(m_tmpInventorySectionTitle));
            m_tmpCurrentWeight = FindRightPanelComponent<TextMeshProUGUI>("m_scrollCharacterDetail/m_viewportCharacterDetail/m_rectCharacterDetailContent/m_sectionInventory/m_panelInventoryHeader/m_tmpCurrentWeight");
            m_tmpWeightLine = FindRightPanelComponent<TextMeshProUGUI>("m_scrollCharacterDetail/m_viewportCharacterDetail/m_rectCharacterDetailContent/m_sectionInventory/m_panelInventoryHeader/m_tmpWeightLine");
            m_tmpMaxWeight = FindRightPanelComponent<TextMeshProUGUI>("m_scrollCharacterDetail/m_viewportCharacterDetail/m_rectCharacterDetailContent/m_sectionInventory/m_panelInventoryHeader/m_tmpMaxWeight");
            m_gridStatusEffects = FindRightPanelComponent<RectTransform>("m_gridStatusEffects");
            m_goStatusEffectTemplate = FindRightPanelComponent<RectTransform>("m_gridStatusEffects/m_itemStatusEffectTemplate")?.gameObject;
            m_tmpAc = FindRightPanelComponent<TextMeshProUGUI>("m_imgAcBackground/m_tmpAc");
            m_tmpInitiative = FindRightPanelComponent<TextMeshProUGUI>("m_imgInitiativeBackground/m_tmpInitiative");
            m_tmpSpeed = FindRightPanelComponent<TextMeshProUGUI>("m_imgSpeedBackground/m_tmpSpeed");
            m_tmpPassivePerception = FindRightPanelComponent<TextMeshProUGUI>("m_img被动感知/m_tmp被动感知");
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
            SetActive(m_goOtherFeatureTemplate, false);
            SetActive(m_goInventoryItemTemplate, false);
            SetActive(m_goStatusEffectTemplate, false);
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

        protected override void OnRefresh()
        {
            LoadRuleContent();
            LoadCharacterCards();
            RefreshCharacterListView();
        }

        private void LoadRuleContent()
        {
            DndRuleContentService.Instance.Reload();
        }

        private void LoadCharacterCards()
        {
            CharacterCardLibrarySaveData library = CharacterCardLocalRepository.Load();
            m_characterCards.Clear();

            if (library?.Characters != null)
            {
                for (int index = 0; index < library.Characters.Count; index++)
                {
                    CharacterCardDraftSaveData character = CharacterCardLocalRepository.Normalize(library.Characters[index]);
                    if (!string.IsNullOrWhiteSpace(character.CharacterId))
                    {
                        m_characterCards.Add(character);
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
                CharacterRuntimeSnapshotData snapshot = BuildDisplaySnapshot(character);
                int capturedIndex = index;
                m_cardViews[index].Bind(
                    character,
                    BuildCharacterClassLine(snapshot),
                    BuildCharacterStatusLine(snapshot, character),
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

            CharacterCardDraftSaveData character = m_characterCards[m_selectedCharacterIndex];
            CharacterRuntimeSnapshotData snapshot = BuildDisplaySnapshot(character);
            SetText(m_tmpCharacterName, FormatTextOrDefault(snapshot.CharacterName, "未选择角色"));
            SetText(m_tmpRace, FormatTextOrDefault(snapshot.RaceName, "未选择种族"));
            SetText(m_tmpClass, BuildClassNameSummary(character, snapshot));
            ApplyPortrait(character.PreviewImagePath);
            RefreshClassSections(character);
            SetAbilityTexts(snapshot);
            SetHpTexts(snapshot);
            SetCombatOverviewTexts(character, snapshot);
            RefreshStatusEffectItems(character);
            SetExperienceProgress(character.Experience, snapshot.Level);
            SetCurrencyTexts(character.Currency);
            SetCarryingWeightTexts();
            SetDeathSaveTexts(character.DeathSaves);
            SetHitDiceTexts(character);
            SetRoleplayTexts(character.RoleplayProfile);
            SetBackgroundText(snapshot);
            SetText(m_tmpProficiencyBonus, CalculateProficiencyBonus(snapshot.Level).ToString());
            SetSkillBonusTexts(character, snapshot);
            SetText(m_tmpEquipmentTools, "装备与工具熟练项");
            RefreshEquipmentToolItems(snapshot);
            RefreshInventoryItems(character.Equipment);
            RefreshRaceFeatureSection(snapshot, character.RaceId);
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
            RefreshStatusEffectItems(null);
            SetExperienceProgress(0, 1);
            SetCurrencyTexts(null);
            SetCarryingWeightTexts();
            SetDeathSaveTexts(null);
            SetHitDiceTexts(null);
            SetRoleplayTexts(null);
            SetBackgroundText(empty);
            SetText(m_tmpProficiencyBonus, DefaultProficiencyBonus.ToString());
            SetSkillBonusTexts(null, empty);
            SetText(m_tmpEquipmentTools, "装备与工具熟练项");
            RefreshEquipmentToolItems(empty);
            RefreshInventoryItems(null);
            RefreshRaceFeatureSection(empty, string.Empty);
            RefreshOtherFeatureSection(null);
            ShowFeatureDetail("特性详情", string.Empty);
        }

        private void SetAbilityTexts(CharacterRuntimeSnapshotData snapshot)
        {
            int strength = NormalizeAbility(snapshot.Strength);
            int dexterity = NormalizeAbility(snapshot.Dexterity);
            int constitution = NormalizeAbility(snapshot.Constitution);
            int intelligence = NormalizeAbility(snapshot.Intelligence);
            int wisdom = NormalizeAbility(snapshot.Wisdom);
            int charisma = NormalizeAbility(snapshot.Charisma);

            SetText(m_tmpStrength, strength.ToString());
            SetText(m_tmpDexterity, dexterity.ToString());
            SetText(m_tmpConstitution, constitution.ToString());
            SetText(m_tmpIntelligence, intelligence.ToString());
            SetText(m_tmpWisdom, wisdom.ToString());
            SetText(m_tmpCharisma, charisma.ToString());
            SetText(m_tmpStrengthModifier, FormatSignedNumber(CalculateAbilityModifier(strength)));
            SetText(m_tmpDexterityModifier, FormatSignedNumber(CalculateAbilityModifier(dexterity)));
            SetText(m_tmpConstitutionModifier, FormatSignedNumber(CalculateAbilityModifier(constitution)));
            SetText(m_tmpIntelligenceModifier, FormatSignedNumber(CalculateAbilityModifier(intelligence)));
            SetText(m_tmpWisdomModifier, FormatSignedNumber(CalculateAbilityModifier(wisdom)));
            SetText(m_tmpCharismaModifier, FormatSignedNumber(CalculateAbilityModifier(charisma)));
        }

        private void SetHpTexts(CharacterRuntimeSnapshotData snapshot)
        {
            int maxHp = Math.Max(0, snapshot.MaxHp);
            int currentHp = NormalizeCurrentHp(snapshot.CurrentHp, maxHp);
            SetText(m_tmpCurrentHp, currentHp.ToString());
            SetText(m_tmpMaxHp, maxHp.ToString());
            SetText(m_tmpTempHp, Math.Max(0, snapshot.TemporaryHp).ToString());
        }

        private void SetCombatOverviewTexts(CharacterCardDraftSaveData character, CharacterRuntimeSnapshotData snapshot)
        {
            if (snapshot == null)
            {
                SetEmptyCombatOverviewTexts();
                return;
            }

            int armorClass = snapshot.ArmorClass > 0 ? snapshot.ArmorClass : CalculateArmorClass(character, snapshot);
            SetText(m_tmpAc, armorClass > 0 ? armorClass.ToString() : string.Empty);
            SetText(m_tmpInitiative, FormatSignedNumber(CalculateInitiativeBonus(character, snapshot)));
            SetText(m_tmpSpeed, snapshot.Speed > 0 ? snapshot.Speed.ToString() : string.Empty);
            SetText(m_tmpPassivePerception, CalculatePassivePerception(character, snapshot).ToString());

            if (TryCalculateSpellcastingNumbers(character, snapshot, out int spellSaveDc, out int spellAttackBonus))
            {
                SetText(m_tmpDc, spellSaveDc.ToString());
                SetText(m_tmpSpellAttackBonus, FormatSignedNumber(spellAttackBonus));
            }
            else
            {
                SetText(m_tmpDc, "-");
                SetText(m_tmpSpellAttackBonus, "-");
            }
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

        internal static int NormalizeCurrentHp(int currentHp, int maxHp)
        {
            int normalizedMaxHp = Math.Max(0, maxHp);
            if (currentHp < 0)
            {
                return normalizedMaxHp;
            }

            int normalizedCurrentHp = Math.Max(0, currentHp);
            return normalizedMaxHp > 0 ? Math.Min(normalizedCurrentHp, normalizedMaxHp) : normalizedCurrentHp;
        }

        private void SetExperienceProgress(int experience, int level)
        {
            int normalizedExperience = Math.Max(0, experience);
            int currentLevel = Math.Max(1, level);
            int currentLevelXp = GetExperienceThreshold(currentLevel);
            int nextLevelXp = GetExperienceThreshold(currentLevel + 1);
            float normalizedValue = nextLevelXp > currentLevelXp
                ? (normalizedExperience - currentLevelXp) / (float)(nextLevelXp - currentLevelXp)
                : 1f;

            if (m_sliderExperience == null)
            {
                SetText(m_tmpExperienceValue, normalizedExperience.ToString());
                return;
            }

            m_sliderExperience.value = Mathf.Clamp01(normalizedValue);
            SetText(m_tmpExperienceValue, nextLevelXp > currentLevelXp
                ? $"{normalizedExperience}/{nextLevelXp}"
                : normalizedExperience.ToString());
        }

        private void SetSkillBonusTexts(CharacterCardDraftSaveData character, CharacterRuntimeSnapshotData snapshot)
        {
            int strength = CalculateAbilityModifier(NormalizeAbility(snapshot.Strength));
            int dexterity = CalculateAbilityModifier(NormalizeAbility(snapshot.Dexterity));
            int intelligence = CalculateAbilityModifier(NormalizeAbility(snapshot.Intelligence));
            int wisdom = CalculateAbilityModifier(NormalizeAbility(snapshot.Wisdom));
            int charisma = CalculateAbilityModifier(NormalizeAbility(snapshot.Charisma));
            int proficiencyBonus = CalculateProficiencyBonus(snapshot.Level);

            for (int index = 0; index < SkillDisplayBindings.Length; index++)
            {
                SkillDisplayBinding binding = SkillDisplayBindings[index];
                int abilityModifier = binding.Ability switch
                {
                    AbilityKind.Strength => strength,
                    AbilityKind.Dexterity => dexterity,
                    AbilityKind.Intelligence => intelligence,
                    AbilityKind.Wisdom => wisdom,
                    AbilityKind.Charisma => charisma,
                    _ => 0
                };

                bool hasExpertise = ContainsId(snapshot.SkillExpertiseIds, binding.SkillId, binding.DisplayName);
                bool hasProficiency = hasExpertise || ContainsId(snapshot.SkillProficiencyIds, binding.SkillId, binding.DisplayName);
                int skillBonus = abilityModifier;
                if (hasExpertise)
                {
                    skillBonus += proficiencyBonus * 2;
                }
                else if (hasProficiency)
                {
                    skillBonus += proficiencyBonus;
                }

                skillBonus += CalculateCharacterAndItemEffectBonus(character, snapshot, "SkillBonus", binding.SkillId, binding.DisplayName);
                SetSkillBonus(index, skillBonus);
                SetSkillBackground(index, hasProficiency, hasExpertise);
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

        private void SetCarryingWeightTexts()
        {
            SetText(m_tmpCurrentWeight, string.Empty);
            SetText(m_tmpWeightLine, string.Empty);
            SetText(m_tmpMaxWeight, string.Empty);
        }

        private void RefreshStatusEffectItems(CharacterCardDraftSaveData character)
        {
            List<CharacterStatusEffectDisplayEntry> entries = BuildStatusEffectEntries(character);
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

        private static List<CharacterStatusEffectDisplayEntry> BuildStatusEffectEntries(CharacterCardDraftSaveData character)
        {
            List<CharacterStatusEffectDisplayEntry> entries = new List<CharacterStatusEffectDisplayEntry>();
            if (character == null)
            {
                return entries;
            }

            AppendConditionStatusEffects(entries, character.Conditions);
            AppendTemporaryStatusEffects(entries, character.TemporaryEffects);
            return entries;
        }

        private static void AppendConditionStatusEffects(
            List<CharacterStatusEffectDisplayEntry> entries,
            IReadOnlyList<CharacterConditionStateSaveData> conditions)
        {
            if (entries == null || conditions == null)
            {
                return;
            }

            for (int index = 0; index < conditions.Count; index++)
            {
                CharacterConditionStateSaveData condition = conditions[index];
                if (condition == null)
                {
                    continue;
                }

                string name = FirstNonEmpty(condition.Name, condition.ConditionId);
                if (condition.ExhaustionLevel > 0)
                {
                    name = string.IsNullOrWhiteSpace(name)
                        ? $"Exhaustion {condition.ExhaustionLevel}"
                        : $"{name} {condition.ExhaustionLevel}";
                }

                if (!string.IsNullOrWhiteSpace(name))
                {
                    entries.Add(new CharacterStatusEffectDisplayEntry(name.Trim(), condition.Duration));
                }
            }
        }

        private static void AppendTemporaryStatusEffects(
            List<CharacterStatusEffectDisplayEntry> entries,
            IReadOnlyList<CharacterTemporaryEffectSaveData> effects)
        {
            if (entries == null || effects == null)
            {
                return;
            }

            for (int index = 0; index < effects.Count; index++)
            {
                CharacterTemporaryEffectSaveData effect = effects[index];
                if (effect == null || !effect.IsActive)
                {
                    continue;
                }

                string name = FirstNonEmpty(effect.Name, effect.EffectId);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    entries.Add(new CharacterStatusEffectDisplayEntry(name.Trim(), effect.Duration));
                }
            }
        }

        private void SetDeathSaveTexts(CharacterDeathSaveData deathSaves)
        {
            CharacterDeathSaveData normalized = CharacterDeathSaveData.Clone(deathSaves);
            SetText(m_tmpDeathSaveSuccesses, $"成功 {normalized.Successes}/3");
            SetText(m_tmpDeathSaveFailures, $"失败 {normalized.Failures}/3");
        }

        private void SetHitDiceTexts(CharacterCardDraftSaveData character)
        {
            List<CharacterHitDicePoolSaveData> pools = BuildDisplayHitDicePools(character);
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

        private List<CharacterHitDicePoolSaveData> BuildDisplayHitDicePools(CharacterCardDraftSaveData character)
        {
            List<CharacterHitDicePoolSaveData> explicitPools = CharacterHitDicePoolSaveData.CloneList(character?.HitDicePools);
            if (explicitPools.Count > 0)
            {
                return explicitPools;
            }

            List<CharacterHitDicePoolSaveData> result = new List<CharacterHitDicePoolSaveData>();
            List<CharacterClassProgressSaveData> progresses = GetCharacterClassProgresses(character);
            for (int index = 0; index < progresses.Count; index++)
            {
                CharacterClassProgressSaveData progress = progresses[index];
                if (progress == null || string.IsNullOrWhiteSpace(progress.ClassId))
                {
                    continue;
                }

                DndClassDefineData classData = FindClass(progress.ClassId);
                int level = Math.Max(1, progress.Level);
                int hitDie = classData != null ? Math.Max(0, classData.HitDie) : 0;
                if (hitDie <= 0)
                {
                    continue;
                }

                result.Add(new CharacterHitDicePoolSaveData
                {
                    ClassId = progress.ClassId,
                    DieSize = hitDie,
                    Total = level,
                    Remaining = level
                });
            }

            return result;
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

        private int CalculatePassivePerception(CharacterCardDraftSaveData character, CharacterRuntimeSnapshotData snapshot)
        {
            if (snapshot == null)
            {
                return 10;
            }

            int wisdomModifier = CalculateAbilityModifier(NormalizeAbility(snapshot.Wisdom));
            int proficiencyBonus = CalculateProficiencyBonus(snapshot.Level);
            bool hasExpertise = ContainsId(snapshot.SkillExpertiseIds, "perception", "察觉");
            bool hasProficiency = hasExpertise || ContainsId(snapshot.SkillProficiencyIds, "perception", "察觉");
            int bonus = wisdomModifier;
            if (hasExpertise)
            {
                bonus += proficiencyBonus * 2;
            }
            else if (hasProficiency)
            {
                bonus += proficiencyBonus;
            }

            bonus += CalculateCharacterAndItemEffectBonus(character, snapshot, "SkillBonus", "perception", "察觉");
            return 10 + bonus;
        }

        private static int GetExperienceThreshold(int level)
        {
            return Math.Max(1, level) switch
            {
                1 => 0,
                2 => 300,
                3 => 900,
                4 => 2700,
                5 => 6500,
                6 => 14000,
                7 => 23000,
                8 => 34000,
                9 => 48000,
                10 => 64000,
                11 => 85000,
                12 => 100000,
                13 => 120000,
                14 => 140000,
                15 => 165000,
                16 => 195000,
                17 => 225000,
                18 => 265000,
                19 => 305000,
                20 => 355000,
                _ => 355000
            };
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

        private CharacterRuntimeSnapshotData BuildDisplaySnapshot(CharacterCardDraftSaveData character)
        {
            character = CharacterCardLocalRepository.Normalize(character);
            CharacterRuntimeSnapshotData snapshot = CharacterRuntimeSnapshotData.Clone(character.RuntimeSnapshot);
            snapshot.CharacterId = character.CharacterId;
            snapshot.CharacterName = FormatTextOrDefault(character.CharacterName, "未命名角色");
            snapshot.Alignment = character.Alignment ?? string.Empty;
            snapshot.Level = Math.Max(1, character.Level);
            snapshot.Experience = Math.Max(0, character.Experience);
            snapshot.RaceId = character.RaceId ?? string.Empty;
            snapshot.ClassId = character.ClassId ?? string.Empty;
            snapshot.BackgroundId = character.BackgroundId ?? string.Empty;
            snapshot.FeatId = character.FeatId ?? string.Empty;
            snapshot.SpellId = character.SpellId ?? string.Empty;
            snapshot.HpModeId = CharacterHpModeIds.Normalize(character.HpModeId);
            snapshot.MaxHp = character.MaxHp;
            snapshot.CurrentHp = NormalizeCurrentHp(character.CurrentHp, character.MaxHp);
            snapshot.TemporaryHp = Math.Max(0, character.TemporaryHp);
            snapshot.DeathSaveSuccesses = character.DeathSaves != null ? Mathf.Clamp(character.DeathSaves.Successes, 0, 3) : 0;
            snapshot.DeathSaveFailures = character.DeathSaves != null ? Mathf.Clamp(character.DeathSaves.Failures, 0, 3) : 0;
            snapshot.ActiveConditions = BuildConditionSummary(character.Conditions);
            snapshot.ActiveResources = BuildResourceSummary(character.Resources);
            ApplyEquippedItemAcData(snapshot, character.Equipment);

            DndClassDefineData classData = FindClass(character.ClassId);
            DndRaceDefineData raceData = FindRace(character.RaceId);
            DndBackgroundDefineData backgroundData = FindBackground(character.BackgroundId);
            AppendUniqueValues(snapshot.SkillProficiencyIds, backgroundData?.SkillProficiencies);
            AppendSkillSummaryValues(snapshot.SkillProficiencyIds, snapshot.Skills);
            AppendEquipmentSummaryValues(snapshot.ArmorProficiencyIds, snapshot.ArmorProficiencies);
            AppendEquipmentSummaryValues(snapshot.WeaponProficiencyIds, snapshot.WeaponProficiencies);
            AppendEquipmentSummaryValues(snapshot.ToolProficiencyIds, snapshot.ToolProficiencies);
            AppendUniqueValues(snapshot.ArmorProficiencyIds, classData?.ArmorProficiencies);
            AppendUniqueValues(snapshot.WeaponProficiencyIds, classData?.WeaponProficiencies);
            AppendUniqueValues(snapshot.ToolProficiencyIds, backgroundData?.ToolProficiencies);
            ApplyFeatureEquipmentProficiencyEffects(snapshot, raceData?.FeatureIds);
            ApplyChoiceEquipmentProficiencyEffects(snapshot, character.ChoiceSelections);
            ApplySubclassEquipmentProficiencyEffects(snapshot, character);

            if (string.IsNullOrWhiteSpace(snapshot.ClassName))
            {
                snapshot.ClassName = classData?.Name ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(snapshot.RaceName))
            {
                snapshot.RaceName = raceData?.Name ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(snapshot.BackgroundName))
            {
                snapshot.BackgroundName = backgroundData?.Name ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(snapshot.Size))
            {
                snapshot.Size = raceData?.Size ?? string.Empty;
            }

            if (snapshot.Speed <= 0)
            {
                snapshot.Speed = raceData?.Speed ?? 0;
            }

            ApplyEquippedItemAttributeEffects(snapshot, character.Equipment);
            snapshot.ArmorClass = CalculateArmorClass(character, snapshot);

            if (string.IsNullOrWhiteSpace(snapshot.SavingThrows))
            {
                snapshot.SavingThrows = FormatList(classData?.SavingThrowProficiencies);
            }

            if (string.IsNullOrWhiteSpace(snapshot.Skills))
            {
                snapshot.Skills = FormatList(backgroundData?.SkillProficiencies);
            }

            if (string.IsNullOrWhiteSpace(snapshot.ArmorProficiencies))
            {
                snapshot.ArmorProficiencies = FormatList(snapshot.ArmorProficiencyIds);
            }

            if (string.IsNullOrWhiteSpace(snapshot.WeaponProficiencies))
            {
                snapshot.WeaponProficiencies = FormatList(snapshot.WeaponProficiencyIds);
            }

            if (string.IsNullOrWhiteSpace(snapshot.ToolProficiencies))
            {
                snapshot.ToolProficiencies = FormatList(snapshot.ToolProficiencyIds);
            }

            if (string.IsNullOrWhiteSpace(snapshot.Languages))
            {
                List<string> languages = new List<string>();
                AppendUniqueValues(languages, raceData?.LanguageIds);
                AppendUniqueValues(languages, backgroundData?.LanguageIds);
                snapshot.Languages = FormatList(languages);
            }

            if (string.IsNullOrWhiteSpace(snapshot.Traits))
            {
                snapshot.Traits = JoinNonEmpty(
                    BuildRaceFeatureSummary(character.RaceId),
                    BuildBackgroundFeatureSummary(character.BackgroundId));
            }

            return snapshot;
        }

        private DndClassDefineData FindClass(string classId)
        {
            return DndRuleContentService.Instance.TryGetClass(classId, out DndClassDefineData classData)
                ? classData
                : null;
        }

        private DndRaceDefineData FindRace(string raceId)
        {
            return FindById(DndRuleContentService.Instance.Races, raceId, data => data.RaceId);
        }

        private DndBackgroundDefineData FindBackground(string backgroundId)
        {
            return FindById(DndRuleContentService.Instance.Backgrounds, backgroundId, data => data.BackgroundId);
        }

        private DndFeatDefineData FindFeat(string featId)
        {
            return FindById(DndRuleContentService.Instance.Feats, featId, data => data.FeatId);
        }

        private string BuildClassLevelSummary(CharacterCardDraftSaveData character)
        {
            List<CharacterClassProgressSaveData> progresses = GetCharacterClassProgresses(character);
            if (progresses.Count == 0)
            {
                return string.Empty;
            }

            List<string> labels = new List<string>();
            for (int index = 0; index < progresses.Count; index++)
            {
                CharacterClassProgressSaveData progress = progresses[index];
                labels.Add($"Lv.{Math.Max(1, progress.Level)}");
            }

            return string.Join(" / ", labels);
        }

        private string BuildClassFeatureClassNameSummary(CharacterCardDraftSaveData character)
        {
            List<CharacterClassProgressSaveData> progresses = GetCharacterClassProgresses(character);
            if (progresses.Count == 0)
            {
                return string.Empty;
            }

            List<string> labels = new List<string>();
            for (int index = 0; index < progresses.Count; index++)
            {
                CharacterClassProgressSaveData progress = progresses[index];
                string className = progress.ClassId;
                DndClassDefineData classData = FindClass(progress.ClassId);
                if (classData != null && !string.IsNullOrWhiteSpace(classData.Name))
                {
                    className = classData.Name.Trim();
                }

                labels.Add(className);
            }

            return string.Join(" / ", labels);
        }

        private static string BuildClassFeatureSubclassNameSummary(CharacterCardDraftSaveData character)
        {
            List<CharacterClassProgressSaveData> progresses = GetCharacterClassProgresses(character);
            if (progresses.Count == 0)
            {
                return string.Empty;
            }

            List<string> labels = new List<string>();
            for (int index = 0; index < progresses.Count; index++)
            {
                CharacterClassProgressSaveData progress = progresses[index];
                string subclassName = ResolveSubclassDisplayName(progress);
                if (!string.IsNullOrWhiteSpace(subclassName))
                {
                    labels.Add(subclassName);
                }
            }

            return string.Join(" / ", labels);
        }

        private string BuildClassNameSummary(CharacterCardDraftSaveData character, CharacterRuntimeSnapshotData snapshot)
        {
            List<CharacterClassProgressSaveData> progresses = GetCharacterClassProgresses(character);
            if (progresses.Count == 0)
            {
                return $"{FormatTextOrDefault(snapshot.ClassName, "未选择职业")}  Lv.{Math.Max(1, snapshot.Level)}";
            }

            List<string> labels = new List<string>();
            for (int index = 0; index < progresses.Count; index++)
            {
                CharacterClassProgressSaveData progress = progresses[index];
                string className = progress.ClassId;
                DndClassDefineData classData = FindClass(progress.ClassId);
                if (classData != null && !string.IsNullOrWhiteSpace(classData.Name))
                {
                    className = classData.Name.Trim();
                }

                string subclassName = ResolveSubclassDisplayName(progress);
                if (!string.IsNullOrWhiteSpace(subclassName))
                {
                    className = $"{className} - {subclassName}";
                }

                labels.Add(className);
            }

            return string.Join(" / ", labels);
        }

        private void RefreshClassSections(CharacterCardDraftSaveData character)
        {
            List<CharacterClassProgressSaveData> progresses = GetCharacterClassProgresses(character);
            EnsureClassSectionViewCount(progresses.Count);

            for (int index = 0; index < m_classSectionViews.Count; index++)
            {
                CharacterClassSectionView view = m_classSectionViews[index];
                bool active = index < progresses.Count;
                view.SetActive(active);
                if (!active)
                {
                    continue;
                }

                CharacterClassProgressSaveData progress = progresses[index];
                view.Bind(
                    GetClassDisplayName(progress),
                    ResolveSubclassDisplayName(progress),
                    $"Lv.{Math.Max(1, progress.Level)}",
                    BuildClassFeatureEntries(character, progress));
            }
        }

        private static string ResolveSubclassDisplayName(CharacterClassProgressSaveData progress)
        {
            if (progress == null || string.IsNullOrWhiteSpace(progress.ClassId) || string.IsNullOrWhiteSpace(progress.SubclassId))
            {
                return string.Empty;
            }

            string choiceGroupId = $"choice_subclass_{progress.ClassId.Trim()}";
            IReadOnlyList<DndChoiceOptionData> options = DndRuleContentService.Instance.GetChoiceOptions(choiceGroupId.Trim());
            for (int index = 0; index < options.Count; index++)
            {
                DndChoiceOptionData option = options[index];
                if (option == null || !string.Equals(option.OptionId, progress.SubclassId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return string.IsNullOrWhiteSpace(option.Name) ? string.Empty : option.Name.Trim();
            }

            return string.Empty;
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

        private string GetClassDisplayName(CharacterClassProgressSaveData progress)
        {
            if (progress == null || string.IsNullOrWhiteSpace(progress.ClassId))
            {
                return string.Empty;
            }

            DndClassDefineData classData = FindClass(progress.ClassId);
            return classData != null && !string.IsNullOrWhiteSpace(classData.Name)
                ? classData.Name.Trim()
                : progress.ClassId.Trim();
        }

        private List<ClassFeatureDisplayEntry> BuildClassFeatureEntries(CharacterCardDraftSaveData character)
        {
            List<ClassFeatureDisplayEntry> entries = new List<ClassFeatureDisplayEntry>();
            List<CharacterClassProgressSaveData> progresses = GetCharacterClassProgresses(character);
            for (int index = 0; index < progresses.Count; index++)
            {
                CharacterClassProgressSaveData progress = progresses[index];
                entries.AddRange(BuildClassFeatureEntries(character, progress));
            }

            return entries;
        }

        private List<ClassFeatureDisplayEntry> BuildClassFeatureEntries(CharacterCardDraftSaveData character, CharacterClassProgressSaveData progress)
        {
            List<ClassFeatureDisplayEntry> entries = new List<ClassFeatureDisplayEntry>();
            if (progress == null || string.IsNullOrWhiteSpace(progress.ClassId))
            {
                return entries;
            }

            int classLevel = Math.Max(1, progress.Level);
            for (int level = 1; level <= classLevel; level++)
            {
                if (!DndRuleContentService.Instance.TryGetClassLevelProgression(progress.ClassId, level, out DndLevelProgressionData progression))
                {
                    continue;
                }

                AppendClassFeatureEntries(entries, progression, progress, level, character?.ChoiceSelections);
            }

            AppendSubclassFeatureEntries(entries, progress, classLevel);
            return entries;
        }

        private static void AppendSubclassFeatureEntries(
            List<ClassFeatureDisplayEntry> entries,
            CharacterClassProgressSaveData progress,
            int classLevel)
        {
            if (entries == null || progress == null || string.IsNullOrWhiteSpace(progress.SubclassId))
            {
                return;
            }

            IReadOnlyList<DndSubclassLevelProgressionData> progressions = DndRuleContentService.Instance.GetSubclassProgressions(progress.SubclassId.Trim());
            for (int index = 0; index < progressions.Count; index++)
            {
                DndSubclassLevelProgressionData progression = progressions[index];
                if (progression == null || progression.Level > classLevel)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(progression.ClassId)
                    && !string.IsNullOrWhiteSpace(progress.ClassId)
                    && !string.Equals(progression.ClassId, progress.ClassId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                AppendFeatureDisplayEntries(entries, progression.FeatureIds);
            }
        }

        private static List<CharacterClassProgressSaveData> GetCharacterClassProgresses(CharacterCardDraftSaveData character)
        {
            List<CharacterClassProgressSaveData> progresses = new List<CharacterClassProgressSaveData>();
            if (character == null)
            {
                return progresses;
            }

            if (character.ClassProgresses != null)
            {
                for (int index = 0; index < character.ClassProgresses.Count; index++)
                {
                    CharacterClassProgressSaveData progress = character.ClassProgresses[index];
                    if (progress == null || string.IsNullOrWhiteSpace(progress.ClassId))
                    {
                        continue;
                    }

                    progresses.Add(new CharacterClassProgressSaveData
                    {
                        ClassId = progress.ClassId.Trim(),
                        SubclassId = progress.SubclassId ?? string.Empty,
                        Level = Math.Max(1, progress.Level)
                    });
                }
            }

            if (progresses.Count == 0 && !string.IsNullOrWhiteSpace(character.ClassId))
            {
                progresses.Add(new CharacterClassProgressSaveData
                {
                    ClassId = character.ClassId.Trim(),
                    Level = Math.Max(1, character.Level)
                });
            }

            return progresses;
        }

        private static void AppendClassFeatureEntries(
            List<ClassFeatureDisplayEntry> entries,
            DndLevelProgressionData progression,
            CharacterClassProgressSaveData progress,
            int level,
            IReadOnlyList<CharacterChoiceSelectionSaveData> choiceSelections)
        {
            IReadOnlyList<string> featureIds = progression?.FeatureIds;
            if (entries == null || featureIds == null)
            {
                return;
            }

            for (int index = 0; index < featureIds.Count; index++)
            {
                string featureId = featureIds[index];
                if (string.IsNullOrWhiteSpace(featureId))
                {
                    continue;
                }

                string title = featureId.Trim();
                string description = string.Empty;
                if (DndRuleContentService.Instance.TryGetFeature(featureId, out DndFeatureDefineData feature))
                {
                    if (ShouldHideSubclassChoiceFeature(featureId, progression))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(feature.Name))
                    {
                        title = feature.Name.Trim();
                    }

                    description = BuildFeatureDescription(feature);
                    if (TryBuildSelectedFeatureChoiceDisplay(feature, progress, level, choiceSelections, out string choiceSuffix, out string choiceDescription))
                    {
                        title = $"{title}-{choiceSuffix}";
                        if (!string.IsNullOrWhiteSpace(choiceDescription))
                        {
                            description = choiceDescription;
                        }
                    }
                }

                entries.Add(new ClassFeatureDisplayEntry(title, description));
            }

            AppendSelectedChoiceGrantedFeatureEntries(entries, progression, progress, level, choiceSelections);
        }

        private static void AppendSelectedChoiceGrantedFeatureEntries(
            List<ClassFeatureDisplayEntry> entries,
            DndLevelProgressionData progression,
            CharacterClassProgressSaveData progress,
            int level,
            IReadOnlyList<CharacterChoiceSelectionSaveData> choiceSelections)
        {
            if (entries == null || progression == null || choiceSelections == null || progression.ChoiceGroupIds == null)
            {
                return;
            }

            for (int index = 0; index < choiceSelections.Count; index++)
            {
                CharacterChoiceSelectionSaveData selection = choiceSelections[index];
                if (!MatchesProgressionChoiceSelection(progression, progress, level, selection))
                {
                    continue;
                }

                DndChoiceOptionData option = FindChoiceOption(selection.ChoiceGroupId, selection.OptionId);
                if (option?.GrantFeatureIds == null)
                {
                    continue;
                }

                AppendFeatureDisplayEntries(entries, option.GrantFeatureIds);
            }

            if (HasSubclassChoiceGroup(progression))
            {
                DndChoiceOptionData subclassOption = FindSelectedSubclassOption(progress);
                if (subclassOption?.GrantFeatureIds != null)
                {
                    AppendFeatureDisplayEntries(entries, subclassOption.GrantFeatureIds);
                }
            }
        }

        private static bool ShouldHideSubclassChoiceFeature(string featureId, DndLevelProgressionData progression)
        {
            if (string.IsNullOrWhiteSpace(featureId) || !HasSubclassChoiceGroup(progression))
            {
                return false;
            }

            string normalizedFeatureId = featureId.Trim().ToLowerInvariant();
            for (int index = 0; index < SubclassChoiceFeatureIdMarkers.Length; index++)
            {
                if (normalizedFeatureId.Contains(SubclassChoiceFeatureIdMarkers[index]))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasSubclassChoiceGroup(DndLevelProgressionData progression)
        {
            if (progression?.ChoiceGroupIds == null)
            {
                return false;
            }

            for (int index = 0; index < progression.ChoiceGroupIds.Count; index++)
            {
                string choiceGroupId = progression.ChoiceGroupIds[index];
                if (!string.IsNullOrWhiteSpace(choiceGroupId)
                    && choiceGroupId.Trim().StartsWith("choice_subclass_", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryBuildSelectedFeatureChoiceDisplay(
            DndFeatureDefineData feature,
            CharacterClassProgressSaveData progress,
            int level,
            IReadOnlyList<CharacterChoiceSelectionSaveData> choiceSelections,
            out string choiceSuffix,
            out string choiceDescription)
        {
            choiceSuffix = string.Empty;
            choiceDescription = string.Empty;
            if (feature == null || feature.ChoiceGroupIds == null || feature.ChoiceGroupIds.Count == 0 || choiceSelections == null)
            {
                return false;
            }

            List<string> suffixes = new List<string>();
            List<string> descriptions = new List<string>();
            for (int index = 0; index < choiceSelections.Count; index++)
            {
                CharacterChoiceSelectionSaveData selection = choiceSelections[index];
                if (!MatchesFeatureChoiceSelection(feature, progress, level, selection))
                {
                    continue;
                }

                DndChoiceOptionData option = FindChoiceOption(selection.ChoiceGroupId, selection.OptionId);
                if (option == null)
                {
                    continue;
                }

                string suffix = BuildChoiceOptionDisplayName(option);
                if (!string.IsNullOrWhiteSpace(suffix))
                {
                    suffixes.Add(suffix);
                }

                string description = BuildChoiceOptionDescription(option);
                if (!string.IsNullOrWhiteSpace(description))
                {
                    descriptions.Add(description);
                }
            }

            if (suffixes.Count == 0)
            {
                return false;
            }

            choiceSuffix = string.Join("/", suffixes);
            choiceDescription = descriptions.Count > 0 ? string.Join("\n", descriptions) : string.Empty;
            return true;
        }

        private static bool MatchesFeatureChoiceSelection(
            DndFeatureDefineData feature,
            CharacterClassProgressSaveData progress,
            int level,
            CharacterChoiceSelectionSaveData selection)
        {
            if (feature == null || selection == null || string.IsNullOrWhiteSpace(selection.ChoiceGroupId))
            {
                return false;
            }

            string selectionChoiceGroupId = selection.ChoiceGroupId.Trim();
            bool groupMatched = false;
            for (int index = 0; index < feature.ChoiceGroupIds.Count; index++)
            {
                string featureChoiceGroupId = feature.ChoiceGroupIds[index];
                if (!string.IsNullOrWhiteSpace(featureChoiceGroupId)
                    && string.Equals(featureChoiceGroupId.Trim(), selectionChoiceGroupId, StringComparison.OrdinalIgnoreCase))
                {
                    groupMatched = true;
                    break;
                }
            }

            if (!groupMatched)
            {
                return false;
            }

            string sourceId = selection.SourceId?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(selection.SourceId)
                && !string.Equals(sourceId, feature.FeatureId?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string selectionClassId = selection.ClassId?.Trim() ?? string.Empty;
            string progressClassId = progress?.ClassId?.Trim() ?? string.Empty;
            if (progress != null
                && !string.IsNullOrWhiteSpace(selectionClassId)
                && !string.Equals(selectionClassId, progressClassId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return selection.Level <= 0 || selection.Level == level;
        }

        private static bool MatchesProgressionChoiceSelection(
            DndLevelProgressionData progression,
            CharacterClassProgressSaveData progress,
            int level,
            CharacterChoiceSelectionSaveData selection)
        {
            if (progression == null || selection == null || string.IsNullOrWhiteSpace(selection.ChoiceGroupId))
            {
                return false;
            }

            string selectionChoiceGroupId = selection.ChoiceGroupId.Trim();
            bool groupMatched = false;
            for (int index = 0; index < progression.ChoiceGroupIds.Count; index++)
            {
                string progressionChoiceGroupId = progression.ChoiceGroupIds[index];
                if (!string.IsNullOrWhiteSpace(progressionChoiceGroupId)
                    && string.Equals(progressionChoiceGroupId.Trim(), selectionChoiceGroupId, StringComparison.OrdinalIgnoreCase))
                {
                    groupMatched = true;
                    break;
                }
            }

            if (!groupMatched)
            {
                return false;
            }

            string selectionClassId = selection.ClassId?.Trim() ?? string.Empty;
            string progressClassId = progress?.ClassId?.Trim() ?? string.Empty;
            if (progress != null
                && !string.IsNullOrWhiteSpace(selectionClassId)
                && !string.Equals(selectionClassId, progressClassId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return selection.Level <= 0 || selection.Level == level;
        }

        private static DndChoiceOptionData FindChoiceOption(string choiceGroupId, string optionId)
        {
            if (string.IsNullOrWhiteSpace(choiceGroupId) || string.IsNullOrWhiteSpace(optionId))
            {
                return null;
            }

            IReadOnlyList<DndChoiceOptionData> options = DndRuleContentService.Instance.GetChoiceOptions(choiceGroupId.Trim());
            string normalizedOptionId = optionId.Trim();
            for (int index = 0; index < options.Count; index++)
            {
                DndChoiceOptionData option = options[index];
                if (option != null
                    && !string.IsNullOrWhiteSpace(option.OptionId)
                    && string.Equals(option.OptionId.Trim(), normalizedOptionId, StringComparison.OrdinalIgnoreCase))
                {
                    return option;
                }
            }

            return null;
        }

        private static DndChoiceOptionData FindSelectedSubclassOption(CharacterClassProgressSaveData progress)
        {
            if (progress == null || string.IsNullOrWhiteSpace(progress.ClassId) || string.IsNullOrWhiteSpace(progress.SubclassId))
            {
                return null;
            }

            return FindChoiceOption($"choice_subclass_{progress.ClassId.Trim()}", progress.SubclassId.Trim());
        }

        private static string BuildChoiceOptionDisplayName(DndChoiceOptionData option)
        {
            if (option == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(option.Name))
            {
                return option.Name.Trim();
            }

            string featureName = GetFirstGrantedFeatureName(option);
            return !string.IsNullOrWhiteSpace(featureName)
                ? featureName
                : string.Empty;
        }

        private static string BuildChoiceOptionDescription(DndChoiceOptionData option)
        {
            if (option == null)
            {
                return string.Empty;
            }

            string effectDescription = GetFirstGrantedEffectDescription(option);
            if (!string.IsNullOrWhiteSpace(effectDescription))
            {
                return effectDescription;
            }

            if (!string.IsNullOrWhiteSpace(option.Description))
            {
                return option.Description.Trim();
            }

            string featureDescription = GetFirstGrantedFeatureDescription(option);
            if (!string.IsNullOrWhiteSpace(featureDescription))
            {
                return featureDescription;
            }

            return string.Empty;
        }

        private static string GetFirstGrantedFeatureName(DndChoiceOptionData option)
        {
            if (option?.GrantFeatureIds == null)
            {
                return string.Empty;
            }

            for (int index = 0; index < option.GrantFeatureIds.Count; index++)
            {
                string featureId = option.GrantFeatureIds[index];
                if (DndRuleContentService.Instance.TryGetFeature(featureId, out DndFeatureDefineData feature)
                    && !string.IsNullOrWhiteSpace(feature.Name))
                {
                    return feature.Name.Trim();
                }
            }

            return string.Empty;
        }

        private static string GetFirstGrantedFeatureDescription(DndChoiceOptionData option)
        {
            if (option?.GrantFeatureIds == null)
            {
                return string.Empty;
            }

            for (int index = 0; index < option.GrantFeatureIds.Count; index++)
            {
                string featureId = option.GrantFeatureIds[index];
                if (DndRuleContentService.Instance.TryGetFeature(featureId, out DndFeatureDefineData feature)
                    && !string.IsNullOrWhiteSpace(feature.Description))
                {
                    return feature.Description.Trim();
                }
            }

            return string.Empty;
        }

        private static string GetFirstGrantedEffectDescription(DndChoiceOptionData option)
        {
            List<string> effectIds = new List<string>();
            if (option?.GrantEffectIds != null)
            {
                effectIds.AddRange(option.GrantEffectIds);
            }

            if (option?.GrantFeatureIds != null)
            {
                for (int index = 0; index < option.GrantFeatureIds.Count; index++)
                {
                    if (DndRuleContentService.Instance.TryGetFeature(option.GrantFeatureIds[index], out DndFeatureDefineData feature)
                        && feature.EffectIds != null)
                    {
                        effectIds.AddRange(feature.EffectIds);
                    }
                }
            }

            for (int index = 0; index < effectIds.Count; index++)
            {
                string effectId = effectIds[index];
                if (DndRuleContentService.Instance.TryGetFeatureEffect(effectId, out DndFeatureEffectData effect)
                    && !string.IsNullOrWhiteSpace(effect.ManualNote))
                {
                    return effect.ManualNote.Trim();
                }
            }

            return string.Empty;
        }

        private string BuildClassFeatureSummary(string classId)
        {
            if (string.IsNullOrWhiteSpace(classId)
                || !DndRuleContentService.Instance.TryGetClassLevelProgression(classId, 1, out DndLevelProgressionData progression))
            {
                return string.Empty;
            }

            return BuildFeatureNameSummary(progression.FeatureIds);
        }

        private string BuildRaceFeatureSummary(string raceId)
        {
            DndRaceDefineData raceData = FindRace(raceId);
            return raceData == null ? string.Empty : BuildFeatureNameSummary(raceData.FeatureIds);
        }

        private string BuildBackgroundFeatureSummary(string backgroundId)
        {
            DndBackgroundDefineData backgroundData = FindBackground(backgroundId);
            return backgroundData == null ? string.Empty : BuildFeatureNameSummary(backgroundData.FeatureIds);
        }

        private static string BuildFeatureNameSummary(IReadOnlyList<string> featureIds)
        {
            if (featureIds == null || featureIds.Count == 0)
            {
                return string.Empty;
            }

            List<string> names = new List<string>();
            for (int index = 0; index < featureIds.Count; index++)
            {
                string featureId = featureIds[index];
                if (string.IsNullOrWhiteSpace(featureId))
                {
                    continue;
                }

                if (DndRuleContentService.Instance.TryGetFeature(featureId, out DndFeatureDefineData feature)
                    && !string.IsNullOrWhiteSpace(feature.Name))
                {
                    names.Add(feature.Name.Trim());
                }
                else
                {
                    names.Add(featureId.Trim());
                }
            }

            return names.Count > 0 ? string.Join("\n", names) : string.Empty;
        }

        private static string BuildFeatureDescription(DndFeatureDefineData feature)
        {
            if (feature == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(feature.Description))
            {
                return feature.Description.Trim();
            }

            if (feature.EffectIds == null)
            {
                return string.Empty;
            }

            for (int index = 0; index < feature.EffectIds.Count; index++)
            {
                string effectId = feature.EffectIds[index];
                if (string.IsNullOrWhiteSpace(effectId))
                {
                    continue;
                }

                if (DndRuleContentService.Instance.TryGetFeatureEffect(effectId, out DndFeatureEffectData effect)
                    && !string.IsNullOrWhiteSpace(effect.ManualNote))
                {
                    return effect.ManualNote.Trim();
                }
            }

            return string.Empty;
        }

        private static void ApplyChoiceEquipmentProficiencyEffects(
            CharacterRuntimeSnapshotData snapshot,
            IReadOnlyList<CharacterChoiceSelectionSaveData> choiceSelections)
        {
            if (snapshot == null || choiceSelections == null)
            {
                return;
            }

            for (int index = 0; index < choiceSelections.Count; index++)
            {
                CharacterChoiceSelectionSaveData selection = choiceSelections[index];
                if (selection == null || string.IsNullOrWhiteSpace(selection.ChoiceGroupId) || string.IsNullOrWhiteSpace(selection.OptionId))
                {
                    continue;
                }

                IReadOnlyList<DndChoiceOptionData> options = DndRuleContentService.Instance.GetChoiceOptions(selection.ChoiceGroupId.Trim());
                for (int optionIndex = 0; optionIndex < options.Count; optionIndex++)
                {
                    DndChoiceOptionData option = options[optionIndex];
                    if (option == null || !string.Equals(option.OptionId, selection.OptionId, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    ApplyFeatureEquipmentProficiencyEffects(snapshot, option.GrantFeatureIds);
                    ApplyEquipmentProficiencyEffects(snapshot, option.GrantEffectIds);
                    break;
                }
            }
        }

        private static void ApplyFeatureEquipmentProficiencyEffects(CharacterRuntimeSnapshotData snapshot, IReadOnlyList<string> featureIds)
        {
            if (snapshot == null || featureIds == null)
            {
                return;
            }

            for (int index = 0; index < featureIds.Count; index++)
            {
                string featureId = featureIds[index];
                if (string.IsNullOrWhiteSpace(featureId))
                {
                    continue;
                }

                if (DndRuleContentService.Instance.TryGetFeature(featureId, out DndFeatureDefineData feature))
                {
                    ApplyEquipmentProficiencyEffects(snapshot, feature.EffectIds);
                }
            }
        }

        private static void ApplySubclassEquipmentProficiencyEffects(CharacterRuntimeSnapshotData snapshot, CharacterCardDraftSaveData character)
        {
            if (snapshot == null || character == null)
            {
                return;
            }

            List<CharacterClassProgressSaveData> progresses = GetCharacterClassProgresses(character);
            for (int progressIndex = 0; progressIndex < progresses.Count; progressIndex++)
            {
                CharacterClassProgressSaveData progress = progresses[progressIndex];
                if (progress == null || string.IsNullOrWhiteSpace(progress.SubclassId))
                {
                    continue;
                }

                int classLevel = Math.Max(1, progress.Level);
                IReadOnlyList<DndSubclassLevelProgressionData> progressions = DndRuleContentService.Instance.GetSubclassProgressions(progress.SubclassId.Trim());
                for (int index = 0; index < progressions.Count; index++)
                {
                    DndSubclassLevelProgressionData progression = progressions[index];
                    if (progression == null || progression.Level > classLevel)
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(progression.ClassId)
                        && !string.IsNullOrWhiteSpace(progress.ClassId)
                        && !string.Equals(progression.ClassId, progress.ClassId, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    ApplyFeatureEquipmentProficiencyEffects(snapshot, progression.FeatureIds);
                }
            }
        }

        private static void ApplyEquipmentProficiencyEffects(CharacterRuntimeSnapshotData snapshot, IReadOnlyList<string> effectIds)
        {
            if (snapshot == null || effectIds == null)
            {
                return;
            }

            for (int index = 0; index < effectIds.Count; index++)
            {
                string effectId = effectIds[index];
                if (string.IsNullOrWhiteSpace(effectId))
                {
                    continue;
                }

                if (DndRuleContentService.Instance.TryGetFeatureEffect(effectId, out DndFeatureEffectData effect))
                {
                    ApplyEquipmentProficiencyEffect(snapshot, effect);
                }
            }
        }

        private static void ApplyEquipmentProficiencyEffect(CharacterRuntimeSnapshotData snapshot, DndFeatureEffectData effect)
        {
            if (snapshot == null || effect == null || string.IsNullOrWhiteSpace(effect.EffectType))
            {
                return;
            }

            if (string.Equals(effect.EffectType, "ArmorProficiency", StringComparison.OrdinalIgnoreCase))
            {
                AppendEquipmentSummaryValues(snapshot.ArmorProficiencyIds, effect.Target);
            }
            else if (string.Equals(effect.EffectType, "WeaponProficiency", StringComparison.OrdinalIgnoreCase))
            {
                AppendEquipmentSummaryValues(snapshot.WeaponProficiencyIds, effect.Target);
            }
            else if (string.Equals(effect.EffectType, "ToolProficiency", StringComparison.OrdinalIgnoreCase))
            {
                AppendEquipmentSummaryValues(snapshot.ToolProficiencyIds, effect.Target);
            }
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

                string normalized = value.Trim();
                if (!target.Exists(item => string.Equals(item, normalized, StringComparison.OrdinalIgnoreCase)))
                {
                    target.Add(normalized);
                }
            }
        }

        private static void AppendSkillSummaryValues(List<string> target, string summary)
        {
            if (target == null || string.IsNullOrWhiteSpace(summary))
            {
                return;
            }

            string[] parts = summary.Split(new[] { ',', '，', ';', '；', '/', '、', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            for (int index = 0; index < parts.Length; index++)
            {
                string normalized = NormalizeSkillId(parts[index]);
                if (!string.IsNullOrWhiteSpace(normalized))
                {
                    AppendUniqueValue(target, normalized);
                }
            }
        }

        private void RefreshEquipmentToolItems(CharacterRuntimeSnapshotData snapshot)
        {
            List<string> entries = BuildEquipmentAndToolEntries(snapshot);
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

        private static List<string> BuildEquipmentAndToolEntries(CharacterRuntimeSnapshotData snapshot)
        {
            List<string> entries = new List<string>();
            if (snapshot == null)
            {
                return entries;
            }

            AppendUniqueValues(entries, snapshot.ArmorProficiencyIds);
            AppendUniqueValues(entries, snapshot.WeaponProficiencyIds);
            AppendUniqueValues(entries, snapshot.ToolProficiencyIds);
            AppendEquipmentSummaryValues(entries, snapshot.ArmorProficiencies);
            AppendEquipmentSummaryValues(entries, snapshot.WeaponProficiencies);
            AppendEquipmentSummaryValues(entries, snapshot.ToolProficiencies);
            return entries;
        }

        private void RefreshInventoryItems(CharacterEquipmentSetSaveData equipment)
        {
            List<CharacterInventoryDisplayEntry> entries = BuildInventoryEntries(equipment);
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
            button.onClick.AddListener(() => ShowFeatureDetail(entry.Title, entry.Description));
        }

        private static List<CharacterInventoryDisplayEntry> BuildInventoryEntries(CharacterEquipmentSetSaveData equipment)
        {
            List<CharacterInventoryDisplayEntry> entries = new List<CharacterInventoryDisplayEntry>();
            if (equipment == null)
            {
                return entries;
            }

            AppendInventoryItemEntry(entries, equipment.Armor, true);
            AppendInventoryItemEntry(entries, equipment.Shield, true);
            AppendInventoryItemEntries(entries, equipment.EquippedItems, true);
            AppendInventoryItemEntries(entries, equipment.InventoryItems, false);
            return entries;
        }

        private static void AppendInventoryItemEntries(List<CharacterInventoryDisplayEntry> entries, IReadOnlyList<CharacterEquipmentItemSaveData> items, bool equipped)
        {
            if (entries == null || items == null)
            {
                return;
            }

            for (int index = 0; index < items.Count; index++)
            {
                AppendInventoryItemEntry(entries, items[index], equipped);
            }
        }

        private static void AppendInventoryItemEntry(List<CharacterInventoryDisplayEntry> entries, CharacterEquipmentItemSaveData item, bool equipped)
        {
            if (entries == null || !CharacterEquipmentItemSaveData.HasItem(item))
            {
                return;
            }

            bool isEquipped = equipped || item.IsEquipped;
            string label = BuildInventoryItemLabel(item);
            if (!string.IsNullOrWhiteSpace(label))
            {
                string title = BuildInventoryItemTitle(item);
                string description = BuildInventoryItemDetailDescription(item, isEquipped);
                entries.Add(new CharacterInventoryDisplayEntry(label, title, description, isEquipped));
            }
        }

        private static string BuildInventoryItemLabel(CharacterEquipmentItemSaveData item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            string name = !string.IsNullOrWhiteSpace(item.ItemName)
                ? item.ItemName.Trim()
                : (item.ItemId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(name);
            if (item.Quantity > 1)
            {
                builder.Append(" x");
                builder.Append(item.Quantity);
            }

            return builder.ToString();
        }

        private static string BuildInventoryItemDetailDescription(CharacterEquipmentItemSaveData item, bool isEquipped)
        {
            if (item == null)
            {
                return string.Empty;
            }

            DndItemDefineData ruleItem = FindDndItemDefinition(item);
            StringBuilder builder = new StringBuilder();
            AppendDetailLine(builder, "类型", FirstNonEmpty(item.ItemType, ruleItem?.ItemType));
            AppendDetailLine(builder, "数量", Math.Max(1, item.Quantity).ToString());
            AppendDetailLine(builder, "状态", isEquipped || item.IsEquipped ? "已装备" : "背包中");
            AppendDetailLine(builder, "来源", BuildInventoryItemSourceText(item, ruleItem));
            AppendDetailLine(builder, "稀有度", ruleItem?.Rarity);
            AppendDetailLine(builder, "装备栏位", ruleItem?.EquipmentSlot);
            AppendDetailLine(builder, "护甲类型", FirstNonEmpty(item.ArmorCategory, ruleItem?.ArmorCategory));

            int armorBaseAc = item.ArmorBaseAc > 0 ? item.ArmorBaseAc : ruleItem?.ArmorBaseAc ?? 0;
            if (armorBaseAc > 0)
            {
                AppendDetailLine(builder, "护甲AC", armorBaseAc.ToString());
            }

            int acBonus = item.AcBonus != 0 ? item.AcBonus : ruleItem?.AcBonus ?? 0;
            if (acBonus != 0)
            {
                AppendDetailLine(builder, "AC加值", FormatSignedNumber(acBonus));
            }

            AppendDetailLine(builder, "伤害", BuildRuleItemDamageText(ruleItem));
            AppendDetailLine(builder, "武器属性", FormatList(ruleItem?.WeaponProperties));
            AppendDetailLine(builder, "重量", ruleItem != null && ruleItem.Weight > 0f ? $"{ruleItem.Weight:g}" : string.Empty);
            AppendDetailLine(builder, "价格", ruleItem != null && ruleItem.PriceGp > 0 ? $"{ruleItem.PriceGp} gp" : string.Empty);

            if (item.RequiresAttunement || ruleItem != null && ruleItem.RequiresAttunement)
            {
                AppendDetailLine(builder, "同调", item.IsAttuned ? "已同调" : "需要同调");
            }

            AppendDetailLine(builder, "描述", FirstNonEmpty(item.Description, ruleItem?.Description));
            AppendDetailLine(builder, "效果", BuildInventoryItemEffectText(item, ruleItem));
            AppendDetailLine(builder, "备注", item.Notes);
            return builder.ToString();
        }

        private static DndItemDefineData FindDndItemDefinition(CharacterEquipmentItemSaveData item)
        {
            if (item == null)
            {
                return null;
            }

            if (DndRuleContentService.Instance.TryGetItem(item.SourceItemId, out DndItemDefineData sourceItem))
            {
                return sourceItem;
            }

            if (DndRuleContentService.Instance.TryGetItem(item.ItemId, out DndItemDefineData itemData))
            {
                return itemData;
            }

            return null;
        }

        private static string BuildInventoryItemTitle(CharacterEquipmentItemSaveData item)
        {
            return BuildInventoryItemName(item);
        }

        private static string BuildInventoryItemName(CharacterEquipmentItemSaveData item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            DndItemDefineData ruleItem = FindDndItemDefinition(item);
            return FirstNonEmpty(item.ItemName, ruleItem?.Name, item.ItemId, item.SourceItemId);
        }

        private static string BuildInventoryItemSourceText(CharacterEquipmentItemSaveData item, DndItemDefineData ruleItem)
        {
            string sourceType = item != null
                ? CharacterItemSourceTypes.Normalize(item.ItemSourceType)
                : string.Empty;
            if (sourceType == CharacterItemSourceTypes.RuleTable)
            {
                return FirstNonEmpty(ruleItem?.SourceBook, "规则表");
            }

            if (sourceType == CharacterItemSourceTypes.Custom)
            {
                return "自定义物品";
            }

            return string.IsNullOrWhiteSpace(sourceType) ? string.Empty : sourceType;
        }

        private static string BuildRuleItemDamageText(DndItemDefineData item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.DamageDice))
            {
                return string.Empty;
            }

            string damage = item.DamageDice.Trim();
            if (!string.IsNullOrWhiteSpace(item.DamageType))
            {
                damage = $"{damage} {item.DamageType.Trim()}";
            }

            if (!string.IsNullOrWhiteSpace(item.TwoHandDamageDice))
            {
                damage = $"{damage} / two-hand {item.TwoHandDamageDice.Trim()}";
            }

            if (item.NormalRange > 0 || item.LongRange > 0)
            {
                damage = $"{damage} ({item.NormalRange}/{item.LongRange})";
            }

            return damage;
        }

        private static string BuildInventoryItemEffectText(CharacterEquipmentItemSaveData item, DndItemDefineData ruleItem)
        {
            List<string> parts = new List<string>();
            AppendFeatureEffectTexts(parts, ruleItem?.EffectIds);
            AppendFeatureEffectTexts(parts, item?.EffectIds);

            if (item?.CustomEffects != null)
            {
                for (int index = 0; index < item.CustomEffects.Count; index++)
                {
                    CharacterItemEffectSaveData effect = item.CustomEffects[index];
                    if (effect == null)
                    {
                        continue;
                    }

                    string text = FirstNonEmpty(effect.Description, BuildInlineEffectText(effect.EffectType, effect.Target, effect.Value, effect.Condition));
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        parts.Add(text);
                    }
                }
            }

            return parts.Count > 0 ? string.Join("\n", parts) : string.Empty;
        }

        private static void AppendFeatureEffectTexts(List<string> target, IReadOnlyList<string> effectIds)
        {
            if (target == null || effectIds == null)
            {
                return;
            }

            for (int index = 0; index < effectIds.Count; index++)
            {
                string effectId = effectIds[index];
                if (string.IsNullOrWhiteSpace(effectId))
                {
                    continue;
                }

                if (DndRuleContentService.Instance.TryGetFeatureEffect(effectId.Trim(), out DndFeatureEffectData effect))
                {
                    string text = FirstNonEmpty(effect.ManualNote, BuildInlineEffectText(effect.EffectType, effect.Target, effect.Value, effect.Condition));
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        target.Add(text);
                    }
                }
                else
                {
                    target.Add(effectId.Trim());
                }
            }
        }

        private static string BuildInlineEffectText(string effectType, string target, string value, string condition)
        {
            string main = JoinNonEmpty(new[] { effectType, target, value }, string.Empty);
            if (string.IsNullOrWhiteSpace(condition))
            {
                return main;
            }

            return string.IsNullOrWhiteSpace(main) ? condition.Trim() : $"{main} ({condition.Trim()})";
        }

        private static void AppendDetailLine(StringBuilder builder, string label, string value)
        {
            if (builder == null || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.Append(label);
            builder.Append(": ");
            builder.Append(value.Trim());
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            for (int index = 0; index < values.Length; index++)
            {
                string value = values[index];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return string.Empty;
        }

        private void RefreshRaceFeatureSection(CharacterRuntimeSnapshotData snapshot, string raceId)
        {
            SetRaceFeatureHeader(snapshot, raceId);

            List<ClassFeatureDisplayEntry> entries = BuildRaceFeatureEntries(raceId);
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
            string mainRaceName = snapshot?.MainRaceName ?? string.Empty;
            string subRaceName = string.Empty;

            if (!string.IsNullOrWhiteSpace(raceId))
            {
                if (DndRuleContentService.Instance.TryGetRaceSub(raceId, out DndRaceSubDefineData subRace))
                {
                    subRaceName = subRace.Name ?? string.Empty;
                    if (DndRuleContentService.Instance.TryGetRaceMain(subRace.MainRaceId, out DndRaceMainDefineData mainRace))
                    {
                        mainRaceName = mainRace.Name ?? mainRaceName;
                    }
                }
                else if (DndRuleContentService.Instance.TryGetRaceMain(raceId, out DndRaceMainDefineData mainRace))
                {
                    mainRaceName = mainRace.Name ?? mainRaceName;
                }
            }

            if (string.IsNullOrWhiteSpace(mainRaceName))
            {
                mainRaceName = snapshot?.RaceName ?? string.Empty;
            }

            SetText(m_tmpRaceFeatureRaceName, FormatTextOrDefault(mainRaceName, "种族"));
            SetText(m_tmpRaceFeatureSubRaceName, subRaceName);
        }

        private List<ClassFeatureDisplayEntry> BuildRaceFeatureEntries(string raceId)
        {
            List<ClassFeatureDisplayEntry> entries = new List<ClassFeatureDisplayEntry>();
            DndRaceDefineData raceData = FindRace(raceId);
            AppendFeatureDisplayEntries(entries, raceData?.FeatureIds);
            return entries;
        }

        private void RefreshOtherFeatureSection(CharacterCardDraftSaveData character)
        {
            SetText(m_tmpOtherFeatureSectionTitle, "其他特性");

            List<ClassFeatureDisplayEntry> entries = BuildOtherFeatureEntries(character);
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

        private List<ClassFeatureDisplayEntry> BuildOtherFeatureEntries(CharacterCardDraftSaveData character)
        {
            List<ClassFeatureDisplayEntry> entries = new List<ClassFeatureDisplayEntry>();
            if (character == null)
            {
                return entries;
            }

            DndFeatDefineData featData = FindFeat(character.FeatId);
            AppendFeatureDisplayEntries(entries, featData?.FeatureIds);
            return entries;
        }

        private static void AppendFeatureDisplayEntries(List<ClassFeatureDisplayEntry> entries, IReadOnlyList<string> featureIds)
        {
            if (entries == null || featureIds == null)
            {
                return;
            }

            for (int index = 0; index < featureIds.Count; index++)
            {
                string featureId = featureIds[index];
                if (string.IsNullOrWhiteSpace(featureId))
                {
                    continue;
                }

                string title = featureId.Trim();
                string description = string.Empty;
                if (DndRuleContentService.Instance.TryGetFeature(featureId, out DndFeatureDefineData feature))
                {
                    if (!string.IsNullOrWhiteSpace(feature.Name))
                    {
                        title = feature.Name.Trim();
                    }

                    description = BuildFeatureDescription(feature);
                }

                entries.Add(new ClassFeatureDisplayEntry(title, description));
            }
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
            SetText(m_tmpFeatureDetailTitle, FormatTextOrDefault(title, "特性详情"));
            SetText(m_tmpFeatureDetailDescription, description ?? string.Empty);
        }

        private static void AppendEquipmentSummaryValues(List<string> target, string summary)
        {
            if (target == null || string.IsNullOrWhiteSpace(summary))
            {
                return;
            }

            string[] parts = summary.Split(new[] { ',', '\uFF0C', ';', '\uFF1B', '/', '\u3001', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            for (int index = 0; index < parts.Length; index++)
            {
                string value = parts[index]?.Trim();
                if (string.IsNullOrWhiteSpace(value) || string.Equals(value, "无", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                AppendUniqueValue(target, value);
            }
        }

        private static void AppendSummaryParts(List<string> target, string summary)
        {
            if (target == null || string.IsNullOrWhiteSpace(summary))
            {
                return;
            }

            string[] parts = summary.Split(new[] { ',', '，', ';', '；', '/', '、', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            for (int index = 0; index < parts.Length; index++)
            {
                string value = parts[index]?.Trim();
                if (string.IsNullOrWhiteSpace(value) || string.Equals(value, "无", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                AppendUniqueValue(target, value);
            }
        }

        private static void AppendUniqueValue(List<string> target, string value)
        {
            if (target == null || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            string normalized = value.Trim();
            if (!target.Exists(item => string.Equals(item, normalized, StringComparison.OrdinalIgnoreCase)))
            {
                target.Add(normalized);
            }
        }

        private static string NormalizeSkillId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string normalized = value.Trim();
            for (int index = 0; index < SkillDisplayBindings.Length; index++)
            {
                SkillDisplayBinding binding = SkillDisplayBindings[index];
                if (string.Equals(normalized, binding.SkillId, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(normalized, binding.DisplayName, StringComparison.OrdinalIgnoreCase))
                {
                    return binding.SkillId;
                }
            }

            return normalized;
        }

        private static bool ContainsId(IReadOnlyList<string> values, string id, string displayName = null)
        {
            if (values == null)
            {
                return false;
            }

            for (int index = 0; index < values.Count; index++)
            {
                string normalized = NormalizeSkillId(values[index]);
                if (string.Equals(normalized, id, StringComparison.OrdinalIgnoreCase)
                    || (!string.IsNullOrWhiteSpace(displayName) && string.Equals(normalized, displayName, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
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

            GameObject target = m_rectSkillProficiencies.gameObject;
            target.SetActive(!target.activeSelf);
        }

        private void OnClickToggleEquipmentTools()
        {
            if (m_rectEquipmentToolContent == null)
            {
                return;
            }

            GameObject target = m_rectEquipmentToolContent.gameObject;
            target.SetActive(!target.activeSelf);
        }

        private void OnClickToggleOtherFeatures()
        {
            if (m_rectOtherFeatureContent == null)
            {
                return;
            }

            GameObject target = m_rectOtherFeatureContent.gameObject;
            target.SetActive(!target.activeSelf);
        }

        private void OnClickToggleRaceFeatures()
        {
            if (m_rectRaceFeatureContent == null)
            {
                return;
            }

            GameObject target = m_rectRaceFeatureContent.gameObject;
            target.SetActive(!target.activeSelf);
        }

        private void OnClickToggleInventory()
        {
            if (m_rectInventoryContent == null)
            {
                return;
            }

            GameObject target = m_rectInventoryContent.gameObject;
            target.SetActive(!target.activeSelf);
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
            CharacterCardLocalRepository.Delete(characterId);
            LoadCharacterCards();
            RefreshCharacterListView();
        }

        private bool HasSelectedCharacter()
        {
            return m_selectedCharacterIndex >= 0 && m_selectedCharacterIndex < m_characterCards.Count;
        }

        private string BuildCharacterClassLine(CharacterRuntimeSnapshotData snapshot)
        {
            return FormatTextOrDefault(snapshot.ClassName, "未选择职业");
        }

        private static string BuildCharacterStatusLine(CharacterRuntimeSnapshotData snapshot, CharacterCardDraftSaveData character)
        {
            return FormatTextOrDefault(snapshot.RaceName, "未选择种族");
        }

        private static string BuildEquipmentAndToolSummary(CharacterRuntimeSnapshotData snapshot)
        {
            List<string> entries = BuildEquipmentAndToolEntries(snapshot);
            if (entries.Count > 0)
            {
                return string.Join(" / ", entries);
            }

            return JoinNonEmpty(new[] { snapshot.ArmorProficiencies, snapshot.WeaponProficiencies, snapshot.ToolProficiencies }, "无");
        }

        private static int CalculateAbilityModifier(int abilityScore)
        {
            return Mathf.FloorToInt((abilityScore - 10) / 2f);
        }

        private static int CalculateProficiencyBonus(int level)
        {
            return 2 + (Math.Max(1, level) - 1) / 4;
        }

        private static void ApplyEquippedItemAcData(CharacterRuntimeSnapshotData snapshot, CharacterEquipmentSetSaveData equipment)
        {
            if (snapshot == null || equipment == null)
            {
                return;
            }

            CharacterEquipmentItemSaveData armor = equipment.Armor;
            if (CharacterEquipmentItemSaveData.HasItem(armor))
            {
                snapshot.ArmorCategory = CharacterArmorCategoryIds.Normalize(armor.ArmorCategory);
                if (armor.ArmorBaseAc > 0)
                {
                    snapshot.ArmorBaseAc = armor.ArmorBaseAc;
                }

                snapshot.EquipmentAcBonus += CalculateItemAcBonus(armor, snapshot);
            }

            CharacterEquipmentItemSaveData shield = equipment.Shield;
            if (CharacterEquipmentItemSaveData.HasItem(shield))
            {
                snapshot.ShieldAcBonus += Math.Max(0, shield.ArmorBaseAc) + CalculateItemAcBonus(shield, snapshot);
            }

            AppendEquippedItemAcBonus(snapshot, equipment.EquippedItems);
        }

        private static void AppendEquippedItemAcBonus(CharacterRuntimeSnapshotData snapshot, IReadOnlyList<CharacterEquipmentItemSaveData> items)
        {
            if (snapshot == null || items == null)
            {
                return;
            }

            for (int index = 0; index < items.Count; index++)
            {
                CharacterEquipmentItemSaveData item = items[index];
                if (CharacterEquipmentItemSaveData.HasItem(item) && item.IsEquipped)
                {
                    snapshot.EquipmentAcBonus += CalculateItemAcBonus(item, snapshot);
                }
            }
        }

        private static void ApplyEquippedItemAttributeEffects(CharacterRuntimeSnapshotData snapshot, CharacterEquipmentSetSaveData equipment)
        {
            if (snapshot == null || equipment == null)
            {
                return;
            }

            ApplyItemAttributeEffects(snapshot, equipment.Armor);
            ApplyItemAttributeEffects(snapshot, equipment.Shield);
            if (equipment.EquippedItems == null)
            {
                return;
            }

            for (int index = 0; index < equipment.EquippedItems.Count; index++)
            {
                CharacterEquipmentItemSaveData item = equipment.EquippedItems[index];
                if (CharacterEquipmentItemSaveData.HasItem(item) && item.IsEquipped)
                {
                    ApplyItemAttributeEffects(snapshot, item);
                }
            }
        }

        private static void ApplyItemAttributeEffects(CharacterRuntimeSnapshotData snapshot, CharacterEquipmentItemSaveData item)
        {
            if (snapshot == null
                || !CharacterEquipmentItemSaveData.HasItem(item)
                || !IsCharacterItemEffectConditionMet(item.EffectApplyCondition, snapshot))
            {
                return;
            }

            ApplyRuleItemAttributeEffects(snapshot, item.EffectIds);
            ApplyCustomItemAttributeEffects(snapshot, item.CustomEffects);
        }

        private static void ApplyRuleItemAttributeEffects(CharacterRuntimeSnapshotData snapshot, IReadOnlyList<string> effectIds)
        {
            if (effectIds == null)
            {
                return;
            }

            for (int index = 0; index < effectIds.Count; index++)
            {
                string effectId = effectIds[index];
                if (string.IsNullOrWhiteSpace(effectId)
                    || !DndRuleContentService.Instance.TryGetFeatureEffect(effectId, out DndFeatureEffectData effect)
                    || !IsStructuredEffectConditionMet(effect, snapshot))
                {
                    continue;
                }

                ApplyItemAttributeEffect(snapshot, effect.EffectType, effect.Target, effect.Value, effect.Condition);
            }
        }

        private static void ApplyCustomItemAttributeEffects(CharacterRuntimeSnapshotData snapshot, IReadOnlyList<CharacterItemEffectSaveData> effects)
        {
            if (effects == null)
            {
                return;
            }

            for (int index = 0; index < effects.Count; index++)
            {
                CharacterItemEffectSaveData effect = effects[index];
                if (effect == null || !IsCharacterItemEffectConditionMet(effect.Condition, snapshot))
                {
                    continue;
                }

                ApplyItemAttributeEffect(snapshot, effect.EffectType, effect.Target, effect.Value, effect.Condition);
            }
        }

        private static void ApplyItemAttributeEffect(
            CharacterRuntimeSnapshotData snapshot,
            string effectType,
            string target,
            string valueText,
            string condition)
        {
            if (snapshot == null
                || string.IsNullOrWhiteSpace(effectType)
                || !IsCharacterItemEffectConditionMet(condition, snapshot)
                || !int.TryParse(valueText, out int value))
            {
                return;
            }

            if (string.Equals(effectType, "AbilityScoreBonus", StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectType, "AbilityBonus", StringComparison.OrdinalIgnoreCase))
            {
                ApplyAbilityScoreBonus(snapshot, target, value);
            }
            else if (string.Equals(effectType, "SpeedBonus", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.Speed = Math.Max(0, snapshot.Speed + value);
            }
            else if (string.Equals(effectType, "InitiativeBonus", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.InitiativeBonus += value;
            }
            else if (string.Equals(effectType, "SpellAttackBonus", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.SpellAttackBonus += value;
            }
            else if (string.Equals(effectType, "SpellSaveDcBonus", StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectType, "SpellSaveDCBonus", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.SpellSaveDcBonus += value;
            }
            else if (string.Equals(effectType, "AttackBonus", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.AttackBonus += value;
            }
            else if (string.Equals(effectType, "WeaponAttackBonus", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.WeaponAttackBonus += value;
            }
            else if (string.Equals(effectType, "DamageBonus", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.DamageBonus += value;
            }
            else if (string.Equals(effectType, "SavingThrowBonus", StringComparison.OrdinalIgnoreCase))
            {
                snapshot.SavingThrowBonus += value;
            }
        }

        private static void ApplyAbilityScoreBonus(CharacterRuntimeSnapshotData snapshot, string target, int value)
        {
            string normalized = target?.Trim() ?? string.Empty;
            if (string.Equals(normalized, "All", StringComparison.OrdinalIgnoreCase)
                || normalized == "全部")
            {
                snapshot.Strength += value;
                snapshot.Dexterity += value;
                snapshot.Constitution += value;
                snapshot.Intelligence += value;
                snapshot.Wisdom += value;
                snapshot.Charisma += value;
                return;
            }

            if (string.Equals(normalized, "Strength", StringComparison.OrdinalIgnoreCase) || normalized == "力量")
            {
                snapshot.Strength += value;
            }
            else if (string.Equals(normalized, "Dexterity", StringComparison.OrdinalIgnoreCase) || normalized == "敏捷")
            {
                snapshot.Dexterity += value;
            }
            else if (string.Equals(normalized, "Constitution", StringComparison.OrdinalIgnoreCase) || normalized == "体质")
            {
                snapshot.Constitution += value;
            }
            else if (string.Equals(normalized, "Intelligence", StringComparison.OrdinalIgnoreCase) || normalized == "智力")
            {
                snapshot.Intelligence += value;
            }
            else if (string.Equals(normalized, "Wisdom", StringComparison.OrdinalIgnoreCase) || normalized == "感知")
            {
                snapshot.Wisdom += value;
            }
            else if (string.Equals(normalized, "Charisma", StringComparison.OrdinalIgnoreCase) || normalized == "魅力")
            {
                snapshot.Charisma += value;
            }
        }

        private static int CalculateItemAcBonus(CharacterEquipmentItemSaveData item, CharacterRuntimeSnapshotData snapshot)
        {
            if (!CharacterEquipmentItemSaveData.HasItem(item))
            {
                return 0;
            }

            if (!IsCharacterItemEffectConditionMet(item.EffectApplyCondition, snapshot))
            {
                return 0;
            }

            int bonus = item.AcBonus;
            bonus += CalculateRuleEffectBonus(item.EffectIds, snapshot, "ACBonus", "AC");
            bonus += CalculateCustomItemEffectBonus(item.CustomEffects, snapshot, "ACBonus", "AC");
            return bonus;
        }

        private static int CalculateRuleEffectBonus(IReadOnlyList<string> effectIds, CharacterRuntimeSnapshotData snapshot, string effectType, string target)
        {
            if (effectIds == null)
            {
                return 0;
            }

            int bonus = 0;
            for (int index = 0; index < effectIds.Count; index++)
            {
                string effectId = effectIds[index];
                if (string.IsNullOrWhiteSpace(effectId)
                    || !DndRuleContentService.Instance.TryGetFeatureEffect(effectId, out DndFeatureEffectData effect)
                    || !IsAcBonusEffect(effect.EffectType, effect.Target, effectType, target)
                    || !IsStructuredEffectConditionMet(effect, snapshot))
                {
                    continue;
                }

                if (int.TryParse(effect.Value, out int value))
                {
                    bonus += value;
                }
            }

            return bonus;
        }

        private static int CalculateCustomItemEffectBonus(
            IReadOnlyList<CharacterItemEffectSaveData> effects,
            CharacterRuntimeSnapshotData snapshot,
            string effectType,
            string target)
        {
            if (effects == null)
            {
                return 0;
            }

            int bonus = 0;
            for (int index = 0; index < effects.Count; index++)
            {
                CharacterItemEffectSaveData effect = effects[index];
                if (effect == null
                    || !IsAcBonusEffect(effect.EffectType, effect.Target, effectType, target)
                    || !IsCharacterItemEffectConditionMet(effect.Condition, snapshot))
                {
                    continue;
                }

                if (int.TryParse(effect.Value, out int value))
                {
                    bonus += value;
                }
            }

            return bonus;
        }

        private static bool IsAcBonusEffect(string actualEffectType, string actualTarget, string expectedEffectType, string expectedTarget)
        {
            return string.Equals(actualEffectType, expectedEffectType, StringComparison.OrdinalIgnoreCase)
                && (string.IsNullOrWhiteSpace(expectedTarget)
                    || string.Equals(actualTarget, expectedTarget, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsCharacterItemEffectConditionMet(string condition, CharacterRuntimeSnapshotData snapshot)
        {
            if (string.IsNullOrWhiteSpace(condition))
            {
                return true;
            }

            if (string.Equals(condition, "WearingArmor", StringComparison.OrdinalIgnoreCase))
            {
                return snapshot != null
                    && !string.Equals(CharacterArmorCategoryIds.Normalize(snapshot.ArmorCategory), CharacterArmorCategoryIds.None, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private int CalculateArmorClass(CharacterCardDraftSaveData character, CharacterRuntimeSnapshotData snapshot)
        {
            if (snapshot == null)
            {
                return 0;
            }

            int dexterityModifier = CalculateAbilityModifier(NormalizeAbility(snapshot.Dexterity));
            int armorBaseAc = snapshot.ArmorBaseAc > 0 ? snapshot.ArmorBaseAc : 10;
            int dexterityAcBonus = CalculateArmorDexterityAcBonus(snapshot.ArmorCategory, dexterityModifier);
            int featureAcBonus = snapshot.FeatureAcBonus + CalculateStructuredEffectBonus(character, snapshot, "ACBonus", "AC");
            return armorBaseAc
                + dexterityAcBonus
                + snapshot.EquipmentAcBonus
                + snapshot.ShieldAcBonus
                + featureAcBonus
                + snapshot.SkillAcBonus;
        }

        private int CalculateInitiativeBonus(CharacterCardDraftSaveData character, CharacterRuntimeSnapshotData snapshot)
        {
            if (snapshot == null)
            {
                return 0;
            }

            int dexterityModifier = CalculateAbilityModifier(NormalizeAbility(snapshot.Dexterity));
            return dexterityModifier
                + snapshot.InitiativeBonus
                + CalculateStructuredEffectBonus(character, snapshot, "InitiativeBonus", "Initiative");
        }

        private static int CalculateArmorDexterityAcBonus(string armorCategory, int dexterityModifier)
        {
            string normalized = CharacterArmorCategoryIds.Normalize(armorCategory);
            if (string.Equals(normalized, CharacterArmorCategoryIds.Heavy, StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            if (string.Equals(normalized, CharacterArmorCategoryIds.Medium, StringComparison.OrdinalIgnoreCase))
            {
                return Math.Min(dexterityModifier, 2);
            }

            return dexterityModifier;
        }

        private bool TryCalculateSpellcastingNumbers(
            CharacterCardDraftSaveData character,
            CharacterRuntimeSnapshotData snapshot,
            out int spellSaveDc,
            out int spellAttackBonus)
        {
            spellSaveDc = 0;
            spellAttackBonus = 0;
            if (snapshot == null)
            {
                return false;
            }

            string abilityId = FindSpellcastingAbilityId(character);
            if (string.IsNullOrWhiteSpace(abilityId))
            {
                return false;
            }

            int abilityScore = GetAbilityScore(snapshot, abilityId);
            if (abilityScore <= 0)
            {
                return false;
            }

            int abilityModifier = CalculateAbilityModifier(NormalizeAbility(abilityScore));
            spellAttackBonus = CalculateProficiencyBonus(snapshot.Level) + abilityModifier + snapshot.AttackBonus + snapshot.SpellAttackBonus;
            spellSaveDc = 8 + spellAttackBonus + snapshot.SpellSaveDcBonus;
            return true;
        }

        private string FindSpellcastingAbilityId(CharacterCardDraftSaveData character)
        {
            List<CharacterClassProgressSaveData> progresses = GetCharacterClassProgresses(character);
            for (int index = 0; index < progresses.Count; index++)
            {
                DndClassDefineData classData = FindClass(progresses[index].ClassId);
                if (classData != null && !string.IsNullOrWhiteSpace(classData.SpellcastingAbility))
                {
                    return classData.SpellcastingAbility.Trim();
                }

                string subclassSpellcastingAbility = FindSubclassSpellcastingAbilityId(progresses[index]);
                if (!string.IsNullOrWhiteSpace(subclassSpellcastingAbility))
                {
                    return subclassSpellcastingAbility;
                }
            }

            return string.Empty;
        }

        private string FindSubclassSpellcastingAbilityId(CharacterClassProgressSaveData progress)
        {
            if (progress != null && !string.IsNullOrWhiteSpace(progress.SubclassId))
            {
                IReadOnlyList<DndSubclassLevelProgressionData> progressions = DndRuleContentService.Instance.GetSubclassProgressions(progress.SubclassId.Trim());
                int classLevel = Math.Max(1, progress.Level);
                for (int index = 0; index < progressions.Count; index++)
                {
                    DndSubclassLevelProgressionData progression = progressions[index];
                    if (progression == null || progression.Level > classLevel)
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(progression.ClassId)
                        && !string.IsNullOrWhiteSpace(progress.ClassId)
                        && !string.Equals(progression.ClassId, progress.ClassId, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string abilityId = FindSpellcastingAbilityId(null, progression.FeatureIds);
                    if (!string.IsNullOrWhiteSpace(abilityId))
                    {
                        return abilityId;
                    }
                }
            }

            DndChoiceOptionData option = FindSelectedSubclassOption(progress);
            if (option == null)
            {
                return string.Empty;
            }

            return FindSpellcastingAbilityId(option.GrantEffectIds, option.GrantFeatureIds);
        }

        private string FindSpellcastingAbilityId(IReadOnlyList<string> effectIds, IReadOnlyList<string> featureIds)
        {
            string abilityId = FindSpellcastingAbilityId(effectIds);
            if (!string.IsNullOrWhiteSpace(abilityId))
            {
                return abilityId;
            }

            if (featureIds == null)
            {
                return string.Empty;
            }

            for (int index = 0; index < featureIds.Count; index++)
            {
                if (DndRuleContentService.Instance.TryGetFeature(featureIds[index], out DndFeatureDefineData feature))
                {
                    abilityId = FindSpellcastingAbilityId(feature.EffectIds);
                    if (!string.IsNullOrWhiteSpace(abilityId))
                    {
                        return abilityId;
                    }
                }
            }

            return string.Empty;
        }

        private string FindSpellcastingAbilityId(IReadOnlyList<string> effectIds)
        {
            if (effectIds == null)
            {
                return string.Empty;
            }

            for (int index = 0; index < effectIds.Count; index++)
            {
                if (DndRuleContentService.Instance.TryGetFeatureEffect(effectIds[index], out DndFeatureEffectData effect)
                    && string.Equals(effect.EffectType, "SpellcastingAbility", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrWhiteSpace(effect.Value))
                    {
                        return effect.Value.Trim();
                    }

                    return effect.Target?.Trim() ?? string.Empty;
                }
            }

            return string.Empty;
        }

        private int CalculateCharacterAndItemEffectBonus(
            CharacterCardDraftSaveData character,
            CharacterRuntimeSnapshotData snapshot,
            string effectType,
            params string[] targets)
        {
            int bonus = CalculateStructuredEffectBonus(character, snapshot, effectType, targets);
            bonus += CalculateEquippedItemEffectBonus(character?.Equipment, snapshot, effectType, targets);
            return bonus;
        }

        private int CalculateStructuredEffectBonus(
            CharacterCardDraftSaveData character,
            CharacterRuntimeSnapshotData snapshot,
            string effectType,
            string target)
        {
            return CalculateStructuredEffectBonus(character, snapshot, effectType, new[] { target });
        }

        private int CalculateStructuredEffectBonus(
            CharacterCardDraftSaveData character,
            CharacterRuntimeSnapshotData snapshot,
            string effectType,
            IReadOnlyList<string> targets)
        {
            if (character == null || string.IsNullOrWhiteSpace(effectType))
            {
                return 0;
            }

            int bonus = 0;
            ApplyCharacterEffects(character, effect =>
            {
                if (effect == null
                    || !IsEffectMatch(effect.EffectType, effect.Target, effectType, targets)
                    || !IsStructuredEffectConditionMet(effect, snapshot))
                {
                    return;
                }

                if (int.TryParse(effect.Value, out int value))
                {
                    bonus += value;
                }
            });

            return bonus;
        }

        private static int CalculateEquippedItemEffectBonus(
            CharacterEquipmentSetSaveData equipment,
            CharacterRuntimeSnapshotData snapshot,
            string effectType,
            IReadOnlyList<string> targets)
        {
            if (equipment == null || string.IsNullOrWhiteSpace(effectType))
            {
                return 0;
            }

            int bonus = 0;
            bonus += CalculateItemEffectBonus(equipment.Armor, snapshot, effectType, targets);
            bonus += CalculateItemEffectBonus(equipment.Shield, snapshot, effectType, targets);

            if (equipment.EquippedItems == null)
            {
                return bonus;
            }

            for (int index = 0; index < equipment.EquippedItems.Count; index++)
            {
                CharacterEquipmentItemSaveData item = equipment.EquippedItems[index];
                if (CharacterEquipmentItemSaveData.HasItem(item) && item.IsEquipped)
                {
                    bonus += CalculateItemEffectBonus(item, snapshot, effectType, targets);
                }
            }

            return bonus;
        }

        private static int CalculateItemEffectBonus(
            CharacterEquipmentItemSaveData item,
            CharacterRuntimeSnapshotData snapshot,
            string effectType,
            IReadOnlyList<string> targets)
        {
            if (!CharacterEquipmentItemSaveData.HasItem(item)
                || !IsCharacterItemEffectConditionMet(item.EffectApplyCondition, snapshot))
            {
                return 0;
            }

            int bonus = 0;
            bonus += CalculateRuleItemEffectBonus(item.EffectIds, snapshot, effectType, targets);
            bonus += CalculateCustomItemEffectBonus(item.CustomEffects, snapshot, effectType, targets);
            return bonus;
        }

        private static int CalculateRuleItemEffectBonus(
            IReadOnlyList<string> effectIds,
            CharacterRuntimeSnapshotData snapshot,
            string effectType,
            IReadOnlyList<string> targets)
        {
            if (effectIds == null)
            {
                return 0;
            }

            int bonus = 0;
            for (int index = 0; index < effectIds.Count; index++)
            {
                string effectId = effectIds[index];
                if (string.IsNullOrWhiteSpace(effectId)
                    || !DndRuleContentService.Instance.TryGetFeatureEffect(effectId, out DndFeatureEffectData effect)
                    || !IsEffectMatch(effect.EffectType, effect.Target, effectType, targets)
                    || !IsStructuredEffectConditionMet(effect, snapshot)
                    || !int.TryParse(effect.Value, out int value))
                {
                    continue;
                }

                bonus += value;
            }

            return bonus;
        }

        private static int CalculateCustomItemEffectBonus(
            IReadOnlyList<CharacterItemEffectSaveData> effects,
            CharacterRuntimeSnapshotData snapshot,
            string effectType,
            IReadOnlyList<string> targets)
        {
            if (effects == null)
            {
                return 0;
            }

            int bonus = 0;
            for (int index = 0; index < effects.Count; index++)
            {
                CharacterItemEffectSaveData effect = effects[index];
                if (effect == null
                    || !IsEffectMatch(effect.EffectType, effect.Target, effectType, targets)
                    || !IsCharacterItemEffectConditionMet(effect.Condition, snapshot)
                    || !int.TryParse(effect.Value, out int value))
                {
                    continue;
                }

                bonus += value;
            }

            return bonus;
        }

        private static bool IsEffectMatch(string actualEffectType, string actualTarget, string expectedEffectType, IReadOnlyList<string> expectedTargets)
        {
            if (!string.Equals(actualEffectType, expectedEffectType, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (expectedTargets == null || expectedTargets.Count == 0)
            {
                return true;
            }

            for (int index = 0; index < expectedTargets.Count; index++)
            {
                string expectedTarget = expectedTargets[index];
                if (string.IsNullOrWhiteSpace(expectedTarget)
                    || string.Equals(actualTarget, expectedTarget, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void ApplyCharacterEffects(CharacterCardDraftSaveData character, Action<DndFeatureEffectData> action)
        {
            if (character == null || action == null)
            {
                return;
            }

            DndRaceDefineData raceData = FindRace(character.RaceId);
            ApplyFeatureEffects(raceData?.FeatureIds, action);

            DndFeatDefineData featData = FindFeat(character.FeatId);
            ApplyEffects(featData?.EffectIds, action);
            ApplyFeatureEffects(featData?.FeatureIds, action);

            List<CharacterClassProgressSaveData> progresses = GetCharacterClassProgresses(character);
            for (int progressIndex = 0; progressIndex < progresses.Count; progressIndex++)
            {
                CharacterClassProgressSaveData progress = progresses[progressIndex];
                int classLevel = Math.Max(1, progress.Level);
                for (int level = 1; level <= classLevel; level++)
                {
                    if (DndRuleContentService.Instance.TryGetClassLevelProgression(progress.ClassId, level, out DndLevelProgressionData progression))
                    {
                        ApplyFeatureEffects(progression.FeatureIds, action);
                    }
                }

                ApplySubclassProgressionFeatureEffects(progress, classLevel, action);
            }

            ApplyChoiceEffects(character.ChoiceSelections, action);
        }

        private static void ApplySubclassProgressionFeatureEffects(
            CharacterClassProgressSaveData progress,
            int classLevel,
            Action<DndFeatureEffectData> action)
        {
            if (progress == null || string.IsNullOrWhiteSpace(progress.SubclassId) || action == null)
            {
                return;
            }

            IReadOnlyList<DndSubclassLevelProgressionData> progressions = DndRuleContentService.Instance.GetSubclassProgressions(progress.SubclassId.Trim());
            for (int index = 0; index < progressions.Count; index++)
            {
                DndSubclassLevelProgressionData progression = progressions[index];
                if (progression == null || progression.Level > classLevel)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(progression.ClassId)
                    && !string.IsNullOrWhiteSpace(progress.ClassId)
                    && !string.Equals(progression.ClassId, progress.ClassId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                ApplyFeatureEffects(progression.FeatureIds, action);
            }
        }

        private static void ApplyFeatureEffects(IReadOnlyList<string> featureIds, Action<DndFeatureEffectData> action)
        {
            if (featureIds == null || action == null)
            {
                return;
            }

            for (int index = 0; index < featureIds.Count; index++)
            {
                if (DndRuleContentService.Instance.TryGetFeature(featureIds[index], out DndFeatureDefineData feature))
                {
                    ApplyEffects(feature.EffectIds, action);
                }
            }
        }

        private static void ApplyEffects(IReadOnlyList<string> effectIds, Action<DndFeatureEffectData> action)
        {
            if (effectIds == null || action == null)
            {
                return;
            }

            for (int index = 0; index < effectIds.Count; index++)
            {
                if (DndRuleContentService.Instance.TryGetFeatureEffect(effectIds[index], out DndFeatureEffectData effect))
                {
                    action(effect);
                }
            }
        }

        private static void ApplyChoiceEffects(
            IReadOnlyList<CharacterChoiceSelectionSaveData> choiceSelections,
            Action<DndFeatureEffectData> action)
        {
            if (choiceSelections == null || action == null)
            {
                return;
            }

            for (int index = 0; index < choiceSelections.Count; index++)
            {
                CharacterChoiceSelectionSaveData selection = choiceSelections[index];
                if (selection == null || string.IsNullOrWhiteSpace(selection.ChoiceGroupId) || string.IsNullOrWhiteSpace(selection.OptionId))
                {
                    continue;
                }

                IReadOnlyList<DndChoiceOptionData> options = DndRuleContentService.Instance.GetChoiceOptions(selection.ChoiceGroupId.Trim());
                for (int optionIndex = 0; optionIndex < options.Count; optionIndex++)
                {
                    DndChoiceOptionData option = options[optionIndex];
                    if (option == null || !string.Equals(option.OptionId, selection.OptionId, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    ApplyEffects(option.GrantEffectIds, action);
                    ApplyFeatureEffects(option.GrantFeatureIds, action);
                    break;
                }
            }
        }

        private static bool IsStructuredEffectConditionMet(DndFeatureEffectData effect, CharacterRuntimeSnapshotData snapshot)
        {
            if (effect == null || string.IsNullOrWhiteSpace(effect.Condition))
            {
                return true;
            }

            if (string.Equals(effect.Condition, "WearingArmor", StringComparison.OrdinalIgnoreCase))
            {
                return snapshot != null
                    && !string.Equals(CharacterArmorCategoryIds.Normalize(snapshot.ArmorCategory), CharacterArmorCategoryIds.None, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static int GetAbilityScore(CharacterRuntimeSnapshotData snapshot, string abilityId)
        {
            if (snapshot == null || string.IsNullOrWhiteSpace(abilityId))
            {
                return 0;
            }

            string normalized = abilityId.Trim();
            if (string.Equals(normalized, "Strength", StringComparison.OrdinalIgnoreCase) || normalized == "力量")
            {
                return snapshot.Strength;
            }

            if (string.Equals(normalized, "Dexterity", StringComparison.OrdinalIgnoreCase) || normalized == "敏捷")
            {
                return snapshot.Dexterity;
            }

            if (string.Equals(normalized, "Constitution", StringComparison.OrdinalIgnoreCase) || normalized == "体质")
            {
                return snapshot.Constitution;
            }

            if (string.Equals(normalized, "Intelligence", StringComparison.OrdinalIgnoreCase) || normalized == "智力")
            {
                return snapshot.Intelligence;
            }

            if (string.Equals(normalized, "Wisdom", StringComparison.OrdinalIgnoreCase) || normalized == "感知")
            {
                return snapshot.Wisdom;
            }

            if (string.Equals(normalized, "Charisma", StringComparison.OrdinalIgnoreCase) || normalized == "魅力")
            {
                return snapshot.Charisma;
            }

            return 0;
        }

        private static int NormalizeAbility(int value)
        {
            return value > 0 ? value : DefaultAbilityScore;
        }

        private static string FormatSignedNumber(int value)
        {
            return value >= 0 ? $"+{value}" : value.ToString();
        }

        private static string FormatTextOrDefault(string value, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        }

        private static string FormatList(IReadOnlyList<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return string.Empty;
            }

            List<string> labels = new List<string>();
            for (int index = 0; index < values.Count; index++)
            {
                if (!string.IsNullOrWhiteSpace(values[index]))
                {
                    labels.Add(values[index].Trim());
                }
            }

            return labels.Count > 0 ? string.Join("、", labels) : string.Empty;
        }

        private static string BuildConditionSummary(IReadOnlyList<CharacterConditionStateSaveData> conditions)
        {
            if (conditions == null || conditions.Count == 0)
            {
                return string.Empty;
            }

            List<string> labels = new List<string>();
            for (int index = 0; index < conditions.Count; index++)
            {
                CharacterConditionStateSaveData condition = conditions[index];
                if (condition == null)
                {
                    continue;
                }

                string label = FirstNonEmpty(condition.Name, condition.ConditionId);
                if (condition.ExhaustionLevel > 0)
                {
                    label = string.IsNullOrWhiteSpace(label)
                        ? $"Exhaustion {condition.ExhaustionLevel}"
                        : $"{label} {condition.ExhaustionLevel}";
                }

                if (!string.IsNullOrWhiteSpace(label))
                {
                    labels.Add(label);
                }
            }

            return labels.Count > 0 ? string.Join(" / ", labels) : string.Empty;
        }

        private static string BuildResourceSummary(IReadOnlyList<CharacterResourceSaveData> resources)
        {
            if (resources == null || resources.Count == 0)
            {
                return string.Empty;
            }

            List<string> labels = new List<string>();
            for (int index = 0; index < resources.Count; index++)
            {
                CharacterResourceSaveData resource = resources[index];
                if (resource == null)
                {
                    continue;
                }

                string label = FirstNonEmpty(resource.Name, resource.ResourceId);
                if (!string.IsNullOrWhiteSpace(label))
                {
                    labels.Add($"{label} {resource.Current}/{resource.Maximum}");
                }
            }

            return labels.Count > 0 ? string.Join(" / ", labels) : string.Empty;
        }

        private static string JoinNonEmpty(params string[] values)
        {
            return JoinNonEmpty(values, string.Empty);
        }

        private static string JoinNonEmpty(IEnumerable<string> values, string emptyText)
        {
            List<string> parts = new List<string>();
            if (values != null)
            {
                foreach (string value in values)
                {
                    if (!string.IsNullOrWhiteSpace(value) && value.Trim() != "无")
                    {
                        parts.Add(value.Trim());
                    }
                }
            }

            return parts.Count > 0 ? string.Join(" / ", parts) : emptyText;
        }

        private static T FindById<T>(IReadOnlyList<T> list, string id, Func<T, string> idGetter)
            where T : class
        {
            if (list == null || string.IsNullOrWhiteSpace(id) || idGetter == null)
            {
                return null;
            }

            for (int index = 0; index < list.Count; index++)
            {
                T item = list[index];
                if (item != null && string.Equals(idGetter(item), id, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }

            return null;
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

    internal readonly struct ClassFeatureDisplayEntry
    {
        public readonly string Title;
        public readonly string Description;

        public ClassFeatureDisplayEntry(string title, string description)
        {
            Title = title ?? string.Empty;
            Description = description ?? string.Empty;
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
            m_button.onClick.AddListener(OnClickToggleContent);
        }

        private void OnClickToggleContent()
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
        public List<CharacterClassProgressSaveData> ClassProgresses = new List<CharacterClassProgressSaveData>();
        public List<CharacterChoiceSelectionSaveData> ChoiceSelections = new List<CharacterChoiceSelectionSaveData>();
        public string BackgroundId = string.Empty;
        public string FeatId = string.Empty;
        public string SpellId = string.Empty;
        public string PreviewImagePath = string.Empty;
        public CharacterIdentityProfileSaveData IdentityProfile = new CharacterIdentityProfileSaveData();
        public CharacterRoleplayProfileSaveData RoleplayProfile = new CharacterRoleplayProfileSaveData();
        public int Level = 1;
        public int Experience;
        public string HpModeId = CharacterHpModeIds.Custom;
        public int MaxHp;
        public int CurrentHp = -1;
        public int TemporaryHp;
        public CharacterDeathSaveData DeathSaves = new CharacterDeathSaveData();
        public int ManualHp;
        public List<CharacterHpRollSaveData> HpRolls = new List<CharacterHpRollSaveData>();
        public List<CharacterHitDicePoolSaveData> HitDicePools = new List<CharacterHitDicePoolSaveData>();
        public CharacterEquipmentSetSaveData Equipment = new CharacterEquipmentSetSaveData();
        public CharacterCurrencySaveData Currency = new CharacterCurrencySaveData();
        public CharacterCarryingCapacitySaveData CarryingCapacity = new CharacterCarryingCapacitySaveData();
        public List<CharacterAttackActionSaveData> AttackActions = new List<CharacterAttackActionSaveData>();
        public CharacterSpellcastingSaveData Spellcasting = new CharacterSpellcastingSaveData();
        public List<CharacterResourceSaveData> Resources = new List<CharacterResourceSaveData>();
        public List<CharacterConditionStateSaveData> Conditions = new List<CharacterConditionStateSaveData>();
        public List<CharacterTemporaryEffectSaveData> TemporaryEffects = new List<CharacterTemporaryEffectSaveData>();
        public bool IsCompleted = false;
        public string CreatedAt = string.Empty;
        public string UpdatedAt = string.Empty;
        public CharacterRuntimeSnapshotData RuntimeSnapshot = new CharacterRuntimeSnapshotData();
    }

    [Serializable]
    internal sealed class CharacterClassProgressSaveData
    {
        public string ClassId = string.Empty;
        public string SubclassId = string.Empty;
        public int Level = 1;
    }

    [Serializable]
    internal sealed class CharacterChoiceSelectionSaveData
    {
        public string ChoiceGroupId = string.Empty;
        public string OptionId = string.Empty;
        public string SourceType = string.Empty;
        public string SourceId = string.Empty;
        public string ClassId = string.Empty;
        public int Level;
    }

    [Serializable]
    internal sealed class CharacterIdentityProfileSaveData
    {
        public string Age = string.Empty;
        public string Height = string.Empty;
        public string Weight = string.Empty;
        public string Eyes = string.Empty;
        public string Skin = string.Empty;
        public string Hair = string.Empty;
        public string Gender = string.Empty;
        public string Appearance = string.Empty;

        public static CharacterIdentityProfileSaveData Clone(CharacterIdentityProfileSaveData source)
        {
            if (source == null)
            {
                return new CharacterIdentityProfileSaveData();
            }

            return new CharacterIdentityProfileSaveData
            {
                Age = source.Age ?? string.Empty,
                Height = source.Height ?? string.Empty,
                Weight = source.Weight ?? string.Empty,
                Eyes = source.Eyes ?? string.Empty,
                Skin = source.Skin ?? string.Empty,
                Hair = source.Hair ?? string.Empty,
                Gender = source.Gender ?? string.Empty,
                Appearance = source.Appearance ?? string.Empty
            };
        }
    }

    [Serializable]
    internal sealed class CharacterRoleplayProfileSaveData
    {
        public string PersonalityTraits = string.Empty;
        public string Ideals = string.Empty;
        public string Bonds = string.Empty;
        public string Flaws = string.Empty;
        public string Backstory = string.Empty;
        public string AlliesAndOrganizations = string.Empty;
        public string Treasure = string.Empty;
        public string AdditionalNotes = string.Empty;

        public static CharacterRoleplayProfileSaveData Clone(CharacterRoleplayProfileSaveData source)
        {
            if (source == null)
            {
                return new CharacterRoleplayProfileSaveData();
            }

            return new CharacterRoleplayProfileSaveData
            {
                PersonalityTraits = source.PersonalityTraits ?? string.Empty,
                Ideals = source.Ideals ?? string.Empty,
                Bonds = source.Bonds ?? string.Empty,
                Flaws = source.Flaws ?? string.Empty,
                Backstory = source.Backstory ?? string.Empty,
                AlliesAndOrganizations = source.AlliesAndOrganizations ?? string.Empty,
                Treasure = source.Treasure ?? string.Empty,
                AdditionalNotes = source.AdditionalNotes ?? string.Empty
            };
        }
    }

    [Serializable]
    internal sealed class CharacterDeathSaveData
    {
        public int Successes;
        public int Failures;

        public static CharacterDeathSaveData Clone(CharacterDeathSaveData source)
        {
            if (source == null)
            {
                return new CharacterDeathSaveData();
            }

            return new CharacterDeathSaveData
            {
                Successes = Mathf.Clamp(source.Successes, 0, 3),
                Failures = Mathf.Clamp(source.Failures, 0, 3)
            };
        }
    }

    [Serializable]
    internal sealed class CharacterHitDicePoolSaveData
    {
        public string ClassId = string.Empty;
        public int DieSize;
        public int Total;
        public int Remaining;

        public static CharacterHitDicePoolSaveData Clone(CharacterHitDicePoolSaveData source)
        {
            if (source == null)
            {
                return new CharacterHitDicePoolSaveData();
            }

            int total = Math.Max(0, source.Total);
            return new CharacterHitDicePoolSaveData
            {
                ClassId = source.ClassId?.Trim() ?? string.Empty,
                DieSize = Math.Max(0, source.DieSize),
                Total = total,
                Remaining = Mathf.Clamp(source.Remaining, 0, total)
            };
        }

        public static List<CharacterHitDicePoolSaveData> CloneList(List<CharacterHitDicePoolSaveData> source)
        {
            List<CharacterHitDicePoolSaveData> result = new List<CharacterHitDicePoolSaveData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterHitDicePoolSaveData pool = Clone(source[index]);
                if (pool.DieSize > 0 || pool.Total > 0 || !string.IsNullOrWhiteSpace(pool.ClassId))
                {
                    result.Add(pool);
                }
            }

            return result;
        }
    }

    [Serializable]
    internal sealed class CharacterEquipmentSetSaveData
    {
        public CharacterEquipmentItemSaveData Armor = new CharacterEquipmentItemSaveData();
        public CharacterEquipmentItemSaveData Shield = new CharacterEquipmentItemSaveData();
        public List<CharacterEquipmentItemSaveData> EquippedItems = new List<CharacterEquipmentItemSaveData>();
        public List<CharacterEquipmentItemSaveData> InventoryItems = new List<CharacterEquipmentItemSaveData>();

        public static CharacterEquipmentSetSaveData Clone(CharacterEquipmentSetSaveData source)
        {
            CharacterEquipmentSetSaveData result = new CharacterEquipmentSetSaveData();
            if (source == null)
            {
                return result;
            }

            result.Armor = CharacterEquipmentItemSaveData.Clone(source.Armor);
            result.Shield = CharacterEquipmentItemSaveData.Clone(source.Shield);
            result.EquippedItems = CloneItemList(source.EquippedItems, true);
            result.InventoryItems = CloneItemList(source.InventoryItems, false);
            return result;
        }

        private static List<CharacterEquipmentItemSaveData> CloneItemList(List<CharacterEquipmentItemSaveData> source, bool equippedOnly)
        {
            List<CharacterEquipmentItemSaveData> result = new List<CharacterEquipmentItemSaveData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterEquipmentItemSaveData item = CharacterEquipmentItemSaveData.Clone(source[index]);
                if (!CharacterEquipmentItemSaveData.HasItem(item))
                {
                    continue;
                }

                if (equippedOnly)
                {
                    item.IsEquipped = true;
                }

                result.Add(item);
            }

            return result;
        }
    }

    [Serializable]
    internal sealed class CharacterCurrencySaveData
    {
        public int Copper;
        public int Silver;
        public int Electrum;
        public int Gold;
        public int Platinum;

        public static CharacterCurrencySaveData Clone(CharacterCurrencySaveData source)
        {
            if (source == null)
            {
                return new CharacterCurrencySaveData();
            }

            return new CharacterCurrencySaveData
            {
                Copper = Math.Max(0, source.Copper),
                Silver = Math.Max(0, source.Silver),
                Electrum = Math.Max(0, source.Electrum),
                Gold = Math.Max(0, source.Gold),
                Platinum = Math.Max(0, source.Platinum)
            };
        }
    }

    [Serializable]
    internal sealed class CharacterCarryingCapacitySaveData
    {
        public float CurrentWeight;
        public float CarryingCapacity;
        public float PushDragLiftCapacity;
        public bool UseVariantEncumbrance;
        public string Notes = string.Empty;

        public static CharacterCarryingCapacitySaveData Clone(CharacterCarryingCapacitySaveData source)
        {
            if (source == null)
            {
                return new CharacterCarryingCapacitySaveData();
            }

            return new CharacterCarryingCapacitySaveData
            {
                CurrentWeight = Math.Max(0f, source.CurrentWeight),
                CarryingCapacity = Math.Max(0f, source.CarryingCapacity),
                PushDragLiftCapacity = Math.Max(0f, source.PushDragLiftCapacity),
                UseVariantEncumbrance = source.UseVariantEncumbrance,
                Notes = source.Notes ?? string.Empty
            };
        }
    }

    [Serializable]
    internal sealed class CharacterAttackActionSaveData
    {
        public string AttackId = string.Empty;
        public string Name = string.Empty;
        public string SourceItemInstanceId = string.Empty;
        public string AbilityId = string.Empty;
        public bool IsProficient;
        public int AttackBonus;
        public string DamageDice = string.Empty;
        public int DamageBonus;
        public string DamageType = string.Empty;
        public string Range = string.Empty;
        public string Properties = string.Empty;
        public string Notes = string.Empty;

        public static CharacterAttackActionSaveData Clone(CharacterAttackActionSaveData source)
        {
            if (source == null)
            {
                return new CharacterAttackActionSaveData();
            }

            return new CharacterAttackActionSaveData
            {
                AttackId = source.AttackId?.Trim() ?? string.Empty,
                Name = source.Name ?? string.Empty,
                SourceItemInstanceId = source.SourceItemInstanceId?.Trim() ?? string.Empty,
                AbilityId = source.AbilityId?.Trim() ?? string.Empty,
                IsProficient = source.IsProficient,
                AttackBonus = source.AttackBonus,
                DamageDice = source.DamageDice?.Trim() ?? string.Empty,
                DamageBonus = source.DamageBonus,
                DamageType = source.DamageType?.Trim() ?? string.Empty,
                Range = source.Range ?? string.Empty,
                Properties = source.Properties ?? string.Empty,
                Notes = source.Notes ?? string.Empty
            };
        }

        public static List<CharacterAttackActionSaveData> CloneList(List<CharacterAttackActionSaveData> source)
        {
            List<CharacterAttackActionSaveData> result = new List<CharacterAttackActionSaveData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterAttackActionSaveData attack = Clone(source[index]);
                if (!string.IsNullOrWhiteSpace(attack.Name) || !string.IsNullOrWhiteSpace(attack.AttackId))
                {
                    result.Add(attack);
                }
            }

            return result;
        }
    }

    [Serializable]
    internal sealed class CharacterEquipmentItemSaveData
    {
        public string ItemInstanceId = string.Empty;
        public string ItemSourceType = CharacterItemSourceTypes.Manual;
        public string SourceItemId = string.Empty;
        public string ItemId = string.Empty;
        public string ItemName = string.Empty;
        public string ItemType = string.Empty;
        public string ArmorCategory = CharacterArmorCategoryIds.None;
        public int ArmorBaseAc;
        public int AcBonus;
        public string Weight = string.Empty;
        public int Quantity = 1;
        public bool IsEquipped;
        public bool RequiresAttunement;
        public bool IsAttuned;
        public string EffectApplyCondition = string.Empty;
        public List<string> EffectIds = new List<string>();
        public List<CharacterItemEffectSaveData> CustomEffects = new List<CharacterItemEffectSaveData>();
        public string Description = string.Empty;
        public string Notes = string.Empty;

        public static CharacterEquipmentItemSaveData Clone(CharacterEquipmentItemSaveData source)
        {
            if (source == null)
            {
                return new CharacterEquipmentItemSaveData();
            }

            return new CharacterEquipmentItemSaveData
            {
                ItemInstanceId = source.ItemInstanceId?.Trim() ?? string.Empty,
                ItemSourceType = CharacterItemSourceTypes.Normalize(source.ItemSourceType),
                SourceItemId = source.SourceItemId?.Trim() ?? string.Empty,
                ItemId = source.ItemId?.Trim() ?? string.Empty,
                ItemName = source.ItemName?.Trim() ?? string.Empty,
                ItemType = source.ItemType?.Trim() ?? string.Empty,
                ArmorCategory = CharacterArmorCategoryIds.Normalize(source.ArmorCategory),
                ArmorBaseAc = Math.Max(0, source.ArmorBaseAc),
                AcBonus = source.AcBonus,
                Weight = source.Weight?.Trim() ?? string.Empty,
                Quantity = Math.Max(1, source.Quantity),
                IsEquipped = source.IsEquipped,
                RequiresAttunement = source.RequiresAttunement,
                IsAttuned = source.IsAttuned,
                EffectApplyCondition = source.EffectApplyCondition?.Trim() ?? string.Empty,
                EffectIds = CloneStringList(source.EffectIds),
                CustomEffects = CharacterItemEffectSaveData.CloneList(source.CustomEffects),
                Description = source.Description ?? string.Empty,
                Notes = source.Notes ?? string.Empty
            };
        }

        public static bool HasItem(CharacterEquipmentItemSaveData item)
        {
            return item != null
                && (!string.IsNullOrWhiteSpace(item.SourceItemId)
                    || !string.IsNullOrWhiteSpace(item.ItemId)
                    || !string.IsNullOrWhiteSpace(item.ItemName)
                    || !string.IsNullOrWhiteSpace(item.Weight)
                    || item.ArmorBaseAc > 0
                    || item.AcBonus != 0
                    || (item.EffectIds != null && item.EffectIds.Count > 0)
                    || (item.CustomEffects != null && item.CustomEffects.Count > 0));
        }

        private static List<string> CloneStringList(List<string> source)
        {
            List<string> result = new List<string>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                string value = source[index];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    result.Add(value.Trim());
                }
            }

            return result;
        }
    }

    [Serializable]
    internal sealed class CharacterItemEffectSaveData
    {
        public string EffectType = string.Empty;
        public string Target = string.Empty;
        public string Value = string.Empty;
        public string Condition = string.Empty;
        public string Description = string.Empty;

        public static CharacterItemEffectSaveData Clone(CharacterItemEffectSaveData source)
        {
            if (source == null)
            {
                return new CharacterItemEffectSaveData();
            }

            return new CharacterItemEffectSaveData
            {
                EffectType = source.EffectType?.Trim() ?? string.Empty,
                Target = source.Target?.Trim() ?? string.Empty,
                Value = source.Value?.Trim() ?? string.Empty,
                Condition = source.Condition?.Trim() ?? string.Empty,
                Description = source.Description ?? string.Empty
            };
        }

        public static List<CharacterItemEffectSaveData> CloneList(List<CharacterItemEffectSaveData> source)
        {
            List<CharacterItemEffectSaveData> result = new List<CharacterItemEffectSaveData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterItemEffectSaveData effect = Clone(source[index]);
                if (!string.IsNullOrWhiteSpace(effect.EffectType))
                {
                    result.Add(effect);
                }
            }

            return result;
        }
    }

    [Serializable]
    internal sealed class CharacterSpellcastingSaveData
    {
        public bool HasSpellcasting;
        public string SpellcastingAbilityId = string.Empty;
        public int SpellSaveDc;
        public int SpellAttackBonus;
        public List<CharacterKnownSpellSaveData> Spells = new List<CharacterKnownSpellSaveData>();
        public List<CharacterSpellSlotLevelSaveData> SpellSlots = new List<CharacterSpellSlotLevelSaveData>();

        public static CharacterSpellcastingSaveData Clone(CharacterSpellcastingSaveData source)
        {
            if (source == null)
            {
                return new CharacterSpellcastingSaveData();
            }

            return new CharacterSpellcastingSaveData
            {
                HasSpellcasting = source.HasSpellcasting,
                SpellcastingAbilityId = source.SpellcastingAbilityId?.Trim() ?? string.Empty,
                SpellSaveDc = Math.Max(0, source.SpellSaveDc),
                SpellAttackBonus = source.SpellAttackBonus,
                Spells = CharacterKnownSpellSaveData.CloneList(source.Spells),
                SpellSlots = CharacterSpellSlotLevelSaveData.CloneList(source.SpellSlots)
            };
        }
    }

    [Serializable]
    internal sealed class CharacterKnownSpellSaveData
    {
        public string SpellId = string.Empty;
        public string SourceClassId = string.Empty;
        public int SpellLevel;
        public bool IsCantrip;
        public bool IsKnown = true;
        public bool IsPrepared;
        public bool IsAlwaysPrepared;
        public bool IsRitual;
        public string Notes = string.Empty;

        public static CharacterKnownSpellSaveData Clone(CharacterKnownSpellSaveData source)
        {
            if (source == null)
            {
                return new CharacterKnownSpellSaveData();
            }

            return new CharacterKnownSpellSaveData
            {
                SpellId = source.SpellId?.Trim() ?? string.Empty,
                SourceClassId = source.SourceClassId?.Trim() ?? string.Empty,
                SpellLevel = Math.Max(0, source.SpellLevel),
                IsCantrip = source.IsCantrip,
                IsKnown = source.IsKnown,
                IsPrepared = source.IsPrepared,
                IsAlwaysPrepared = source.IsAlwaysPrepared,
                IsRitual = source.IsRitual,
                Notes = source.Notes ?? string.Empty
            };
        }

        public static List<CharacterKnownSpellSaveData> CloneList(List<CharacterKnownSpellSaveData> source)
        {
            List<CharacterKnownSpellSaveData> result = new List<CharacterKnownSpellSaveData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterKnownSpellSaveData spell = Clone(source[index]);
                if (!string.IsNullOrWhiteSpace(spell.SpellId))
                {
                    result.Add(spell);
                }
            }

            return result;
        }
    }

    [Serializable]
    internal sealed class CharacterSpellSlotLevelSaveData
    {
        public int SpellLevel;
        public int TotalSlots;
        public int UsedSlots;

        public static CharacterSpellSlotLevelSaveData Clone(CharacterSpellSlotLevelSaveData source)
        {
            if (source == null)
            {
                return new CharacterSpellSlotLevelSaveData();
            }

            int total = Math.Max(0, source.TotalSlots);
            return new CharacterSpellSlotLevelSaveData
            {
                SpellLevel = Math.Max(1, source.SpellLevel),
                TotalSlots = total,
                UsedSlots = Mathf.Clamp(source.UsedSlots, 0, total)
            };
        }

        public static List<CharacterSpellSlotLevelSaveData> CloneList(List<CharacterSpellSlotLevelSaveData> source)
        {
            List<CharacterSpellSlotLevelSaveData> result = new List<CharacterSpellSlotLevelSaveData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterSpellSlotLevelSaveData slot = Clone(source[index]);
                if (slot.TotalSlots > 0)
                {
                    result.Add(slot);
                }
            }

            return result;
        }
    }

    [Serializable]
    internal sealed class CharacterResourceSaveData
    {
        public string ResourceId = string.Empty;
        public string Name = string.Empty;
        public string SourceType = string.Empty;
        public string SourceId = string.Empty;
        public int Maximum;
        public int Current;
        public string RecoveryType = string.Empty;
        public string Notes = string.Empty;

        public static CharacterResourceSaveData Clone(CharacterResourceSaveData source)
        {
            if (source == null)
            {
                return new CharacterResourceSaveData();
            }

            int maximum = Math.Max(0, source.Maximum);
            return new CharacterResourceSaveData
            {
                ResourceId = source.ResourceId?.Trim() ?? string.Empty,
                Name = source.Name ?? string.Empty,
                SourceType = source.SourceType?.Trim() ?? string.Empty,
                SourceId = source.SourceId?.Trim() ?? string.Empty,
                Maximum = maximum,
                Current = Mathf.Clamp(source.Current, 0, maximum),
                RecoveryType = source.RecoveryType?.Trim() ?? string.Empty,
                Notes = source.Notes ?? string.Empty
            };
        }

        public static List<CharacterResourceSaveData> CloneList(List<CharacterResourceSaveData> source)
        {
            List<CharacterResourceSaveData> result = new List<CharacterResourceSaveData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterResourceSaveData resource = Clone(source[index]);
                if (!string.IsNullOrWhiteSpace(resource.Name) || !string.IsNullOrWhiteSpace(resource.ResourceId))
                {
                    result.Add(resource);
                }
            }

            return result;
        }
    }

    [Serializable]
    internal sealed class CharacterConditionStateSaveData
    {
        public string ConditionId = string.Empty;
        public string Name = string.Empty;
        public string Source = string.Empty;
        public int ExhaustionLevel;
        public string Duration = string.Empty;
        public string Notes = string.Empty;

        public static CharacterConditionStateSaveData Clone(CharacterConditionStateSaveData source)
        {
            if (source == null)
            {
                return new CharacterConditionStateSaveData();
            }

            return new CharacterConditionStateSaveData
            {
                ConditionId = source.ConditionId?.Trim() ?? string.Empty,
                Name = source.Name ?? string.Empty,
                Source = source.Source ?? string.Empty,
                ExhaustionLevel = Mathf.Clamp(source.ExhaustionLevel, 0, 6),
                Duration = source.Duration ?? string.Empty,
                Notes = source.Notes ?? string.Empty
            };
        }

        public static List<CharacterConditionStateSaveData> CloneList(List<CharacterConditionStateSaveData> source)
        {
            List<CharacterConditionStateSaveData> result = new List<CharacterConditionStateSaveData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterConditionStateSaveData condition = Clone(source[index]);
                if (!string.IsNullOrWhiteSpace(condition.ConditionId)
                    || !string.IsNullOrWhiteSpace(condition.Name)
                    || condition.ExhaustionLevel > 0)
                {
                    result.Add(condition);
                }
            }

            return result;
        }
    }

    [Serializable]
    internal sealed class CharacterTemporaryEffectSaveData
    {
        public string EffectId = string.Empty;
        public string Name = string.Empty;
        public string Source = string.Empty;
        public string Duration = string.Empty;
        public bool IsActive = true;
        public List<CharacterItemEffectSaveData> Effects = new List<CharacterItemEffectSaveData>();
        public string Notes = string.Empty;

        public static CharacterTemporaryEffectSaveData Clone(CharacterTemporaryEffectSaveData source)
        {
            if (source == null)
            {
                return new CharacterTemporaryEffectSaveData();
            }

            return new CharacterTemporaryEffectSaveData
            {
                EffectId = source.EffectId?.Trim() ?? string.Empty,
                Name = source.Name ?? string.Empty,
                Source = source.Source ?? string.Empty,
                Duration = source.Duration ?? string.Empty,
                IsActive = source.IsActive,
                Effects = CharacterItemEffectSaveData.CloneList(source.Effects),
                Notes = source.Notes ?? string.Empty
            };
        }

        public static List<CharacterTemporaryEffectSaveData> CloneList(List<CharacterTemporaryEffectSaveData> source)
        {
            List<CharacterTemporaryEffectSaveData> result = new List<CharacterTemporaryEffectSaveData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterTemporaryEffectSaveData effect = Clone(source[index]);
                if (!string.IsNullOrWhiteSpace(effect.Name)
                    || !string.IsNullOrWhiteSpace(effect.EffectId)
                    || effect.Effects.Count > 0)
                {
                    result.Add(effect);
                }
            }

            return result;
        }
    }

    internal static class CharacterItemSourceTypes
    {
        public const string RuleTable = "rule_table";
        public const string Custom = "custom";
        public const string Manual = "manual";

        public static string Normalize(string value)
        {
            if (string.Equals(value, RuleTable, StringComparison.OrdinalIgnoreCase))
            {
                return RuleTable;
            }

            if (string.Equals(value, Custom, StringComparison.OrdinalIgnoreCase))
            {
                return Custom;
            }

            return Manual;
        }
    }

    [Serializable]
    internal sealed class LocalCustomItemLibrarySaveData
    {
        public List<LocalCustomItemSaveData> Items = new List<LocalCustomItemSaveData>();
    }

    [Serializable]
    internal sealed class LocalCustomItemSaveData
    {
        public string CustomItemId = string.Empty;
        public CharacterEquipmentItemSaveData Item = new CharacterEquipmentItemSaveData();
        public string CreatedAt = string.Empty;
        public string UpdatedAt = string.Empty;

        public static LocalCustomItemSaveData Clone(LocalCustomItemSaveData source)
        {
            if (source == null)
            {
                return new LocalCustomItemSaveData();
            }

            return new LocalCustomItemSaveData
            {
                CustomItemId = source.CustomItemId?.Trim() ?? string.Empty,
                Item = CharacterEquipmentItemSaveData.Clone(source.Item),
                CreatedAt = source.CreatedAt ?? string.Empty,
                UpdatedAt = source.UpdatedAt ?? string.Empty
            };
        }
    }

    internal static class LocalCustomItemRepository
    {
        private const string SaveDirectoryName = "CustomItems";
        private const string SaveFileName = "custom_items.json";

        public static string GetSaveFilePath()
        {
            return Path.Combine(Application.persistentDataPath, SaveDirectoryName, SaveFileName);
        }

        public static LocalCustomItemLibrarySaveData Load()
        {
            string filePath = GetSaveFilePath();
            if (!File.Exists(filePath))
            {
                return new LocalCustomItemLibrarySaveData();
            }

            try
            {
                string json = File.ReadAllText(filePath, Encoding.UTF8);
                LocalCustomItemLibrarySaveData data = Utility.Json.ToObject<LocalCustomItemLibrarySaveData>(json);
                return NormalizeLibrary(data);
            }
            catch (Exception exception)
            {
                Log.Error($"自定义物品：读取本地物品库失败。{exception.Message}");
                return new LocalCustomItemLibrarySaveData();
            }
        }

        public static void Save(LocalCustomItemLibrarySaveData data)
        {
            string filePath = GetSaveFilePath();
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, Utility.Json.ToJson(NormalizeLibrary(data)), Encoding.UTF8);
        }

        public static void Upsert(LocalCustomItemSaveData item)
        {
            if (item == null)
            {
                return;
            }

            LocalCustomItemSaveData normalized = NormalizeItem(item, true);
            LocalCustomItemLibrarySaveData library = Load();
            int index = -1;
            for (int i = 0; i < library.Items.Count; i++)
            {
                if (string.Equals(library.Items[i].CustomItemId, normalized.CustomItemId, StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
            {
                library.Items[index] = normalized;
            }
            else
            {
                library.Items.Add(normalized);
            }

            Save(library);
        }

        public static bool TryGetItem(string customItemId, out LocalCustomItemSaveData item)
        {
            item = null;
            if (string.IsNullOrWhiteSpace(customItemId))
            {
                return false;
            }

            LocalCustomItemLibrarySaveData library = Load();
            for (int index = 0; index < library.Items.Count; index++)
            {
                LocalCustomItemSaveData candidate = library.Items[index];
                if (string.Equals(candidate.CustomItemId, customItemId, StringComparison.OrdinalIgnoreCase))
                {
                    item = LocalCustomItemSaveData.Clone(candidate);
                    return true;
                }
            }

            return false;
        }

        public static void Delete(string customItemId)
        {
            if (string.IsNullOrWhiteSpace(customItemId))
            {
                return;
            }

            LocalCustomItemLibrarySaveData library = Load();
            for (int index = library.Items.Count - 1; index >= 0; index--)
            {
                if (string.Equals(library.Items[index].CustomItemId, customItemId, StringComparison.OrdinalIgnoreCase))
                {
                    library.Items.RemoveAt(index);
                }
            }

            Save(library);
        }

        public static CharacterEquipmentItemSaveData CreateCharacterItemSnapshot(LocalCustomItemSaveData customItem, int quantity)
        {
            LocalCustomItemSaveData normalized = NormalizeItem(customItem, false);
            CharacterEquipmentItemSaveData snapshot = CharacterEquipmentItemSaveData.Clone(normalized.Item);
            snapshot.ItemInstanceId = $"item_instance_{DateTime.UtcNow:yyyyMMddHHmmssfff}";
            snapshot.ItemSourceType = CharacterItemSourceTypes.Custom;
            snapshot.SourceItemId = normalized.CustomItemId;
            snapshot.ItemId = string.IsNullOrWhiteSpace(snapshot.ItemId) ? normalized.CustomItemId : snapshot.ItemId;
            snapshot.Quantity = Math.Max(1, quantity);
            snapshot.IsEquipped = false;
            return snapshot;
        }

        private static LocalCustomItemLibrarySaveData NormalizeLibrary(LocalCustomItemLibrarySaveData data)
        {
            LocalCustomItemLibrarySaveData result = new LocalCustomItemLibrarySaveData();
            if (data?.Items == null)
            {
                return result;
            }

            for (int index = 0; index < data.Items.Count; index++)
            {
                LocalCustomItemSaveData item = NormalizeItem(data.Items[index], false);
                if (!string.IsNullOrWhiteSpace(item.CustomItemId) && CharacterEquipmentItemSaveData.HasItem(item.Item))
                {
                    result.Items.Add(item);
                }
            }

            return result;
        }

        private static LocalCustomItemSaveData NormalizeItem(LocalCustomItemSaveData source, bool refreshUpdatedAt)
        {
            LocalCustomItemSaveData item = LocalCustomItemSaveData.Clone(source);
            if (string.IsNullOrWhiteSpace(item.CustomItemId))
            {
                item.CustomItemId = $"custom_item_{DateTime.UtcNow:yyyyMMddHHmmssfff}";
            }

            item.Item = CharacterEquipmentItemSaveData.Clone(item.Item);
            item.Item.ItemSourceType = CharacterItemSourceTypes.Custom;
            item.Item.SourceItemId = item.CustomItemId;
            if (string.IsNullOrWhiteSpace(item.Item.ItemId))
            {
                item.Item.ItemId = item.CustomItemId;
            }

            string now = DateTime.UtcNow.ToString("O");
            if (string.IsNullOrWhiteSpace(item.CreatedAt))
            {
                item.CreatedAt = now;
            }

            if (string.IsNullOrWhiteSpace(item.UpdatedAt) || refreshUpdatedAt)
            {
                item.UpdatedAt = now;
            }

            return item;
        }
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
            return normalized.Equals(Custom, StringComparison.OrdinalIgnoreCase)
                || normalized.Equals(Rolled, StringComparison.OrdinalIgnoreCase)
                || normalized.Equals(Average, StringComparison.OrdinalIgnoreCase)
                ? normalized.ToLowerInvariant()
                : Custom;
        }
    }

    internal static class CharacterArmorCategoryIds
    {
        public const string None = "none";
        public const string Light = "light";
        public const string Medium = "medium";
        public const string Heavy = "heavy";

        public static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return None;
            }

            string normalized = value.Trim();
            if (normalized.Equals(Light, StringComparison.OrdinalIgnoreCase) || normalized == "轻甲")
            {
                return Light;
            }

            if (normalized.Equals(Medium, StringComparison.OrdinalIgnoreCase) || normalized == "中甲")
            {
                return Medium;
            }

            if (normalized.Equals(Heavy, StringComparison.OrdinalIgnoreCase) || normalized == "重甲")
            {
                return Heavy;
            }

            if (normalized.Equals(None, StringComparison.OrdinalIgnoreCase) || normalized == "无甲")
            {
                return None;
            }

            return None;
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
        public int Experience;
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
        public int ArmorClass;
        public string ArmorCategory = CharacterArmorCategoryIds.None;
        public int ArmorBaseAc = 10;
        public int EquipmentAcBonus;
        public int ShieldAcBonus;
        public int FeatureAcBonus;
        public int SkillAcBonus;
        public int InitiativeBonus;
        public int AttackBonus;
        public int WeaponAttackBonus;
        public int SpellAttackBonus;
        public int SpellSaveDcBonus;
        public int DamageBonus;
        public int SavingThrowBonus;
        public string HpModeId = CharacterHpModeIds.Custom;
        public int MaxHp;
        public int CurrentHp = -1;
        public int TemporaryHp;
        public int DeathSaveSuccesses;
        public int DeathSaveFailures;
        public int Strength = 10;
        public int Dexterity = 10;
        public int Constitution = 10;
        public int Intelligence = 10;
        public int Wisdom = 10;
        public int Charisma = 10;
        public string SavingThrows = string.Empty;
        public string Skills = string.Empty;
        public List<string> SkillProficiencyIds = new List<string>();
        public List<string> SkillExpertiseIds = new List<string>();
        public List<string> ArmorProficiencyIds = new List<string>();
        public List<string> WeaponProficiencyIds = new List<string>();
        public List<string> ToolProficiencyIds = new List<string>();
        public string ArmorProficiencies = string.Empty;
        public string WeaponProficiencies = string.Empty;
        public string ToolProficiencies = string.Empty;
        public string Senses = string.Empty;
        public string Languages = string.Empty;
        public string DamageResistances = string.Empty;
        public string ActiveConditions = string.Empty;
        public string ActiveResources = string.Empty;
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
                Experience = Math.Max(0, source.Experience),
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
                ArmorClass = Math.Max(0, source.ArmorClass),
                ArmorCategory = CharacterArmorCategoryIds.Normalize(source.ArmorCategory),
                ArmorBaseAc = source.ArmorBaseAc > 0 ? source.ArmorBaseAc : 10,
                EquipmentAcBonus = source.EquipmentAcBonus,
                ShieldAcBonus = source.ShieldAcBonus,
                FeatureAcBonus = source.FeatureAcBonus,
                SkillAcBonus = source.SkillAcBonus,
                InitiativeBonus = source.InitiativeBonus,
                AttackBonus = source.AttackBonus,
                WeaponAttackBonus = source.WeaponAttackBonus,
                SpellAttackBonus = source.SpellAttackBonus,
                SpellSaveDcBonus = source.SpellSaveDcBonus,
                DamageBonus = source.DamageBonus,
                SavingThrowBonus = source.SavingThrowBonus,
                HpModeId = CharacterHpModeIds.Normalize(source.HpModeId),
                MaxHp = Math.Max(0, source.MaxHp),
                CurrentHp = CharacterCardManagementUI.NormalizeCurrentHp(source.CurrentHp, source.MaxHp),
                TemporaryHp = Math.Max(0, source.TemporaryHp),
                DeathSaveSuccesses = Mathf.Clamp(source.DeathSaveSuccesses, 0, 3),
                DeathSaveFailures = Mathf.Clamp(source.DeathSaveFailures, 0, 3),
                Strength = source.Strength,
                Dexterity = source.Dexterity,
                Constitution = source.Constitution,
                Intelligence = source.Intelligence,
                Wisdom = source.Wisdom,
                Charisma = source.Charisma,
                WeaponProficiencies = source.WeaponProficiencies ?? string.Empty,
                SavingThrows = source.SavingThrows ?? string.Empty,
                Skills = source.Skills ?? string.Empty,
                SkillProficiencyIds = CloneStringList(source.SkillProficiencyIds),
                SkillExpertiseIds = CloneStringList(source.SkillExpertiseIds),
                ArmorProficiencyIds = CloneStringList(source.ArmorProficiencyIds),
                WeaponProficiencyIds = CloneStringList(source.WeaponProficiencyIds),
                ToolProficiencyIds = CloneStringList(source.ToolProficiencyIds),
                ArmorProficiencies = source.ArmorProficiencies ?? string.Empty,
                ToolProficiencies = source.ToolProficiencies ?? string.Empty,
                Senses = source.Senses ?? string.Empty,
                Languages = source.Languages ?? string.Empty,
                DamageResistances = source.DamageResistances ?? string.Empty,
                ActiveConditions = source.ActiveConditions ?? string.Empty,
                ActiveResources = source.ActiveResources ?? string.Empty,
                PendingSelections = source.PendingSelections ?? string.Empty,
                ConditionalBenefits = source.ConditionalBenefits ?? string.Empty,
                Traits = source.Traits ?? string.Empty,
                Notes = source.Notes ?? string.Empty
            };
        }

        private static List<string> CloneStringList(List<string> source)
        {
            List<string> result = new List<string>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                string value = source[index];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    result.Add(value.Trim());
                }
            }

            return result;
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
                return new CharacterCardDraftSaveData
                {
                    CharacterId = $"character_{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                    CharacterName = "未命名角色",
                    CreatedAt = DateTime.UtcNow.ToString("O"),
                    UpdatedAt = DateTime.UtcNow.ToString("O")
                };
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
            character.ClassProgresses = NormalizeClassProgresses(character.ClassProgresses, character.ClassId, character.Level);
            character.ChoiceSelections = NormalizeChoiceSelections(character.ChoiceSelections);
            character.BackgroundId ??= string.Empty;
            character.FeatId ??= string.Empty;
            character.SpellId ??= string.Empty;
            character.PreviewImagePath ??= string.Empty;
            character.IdentityProfile = CharacterIdentityProfileSaveData.Clone(character.IdentityProfile);
            character.RoleplayProfile = CharacterRoleplayProfileSaveData.Clone(character.RoleplayProfile);
            character.Level = Math.Max(1, character.Level);
            character.Experience = Math.Max(0, character.Experience);
            character.HpModeId = CharacterHpModeIds.Normalize(character.HpModeId);
            character.MaxHp = Math.Max(0, character.MaxHp);
            character.CurrentHp = CharacterCardManagementUI.NormalizeCurrentHp(character.CurrentHp, character.MaxHp);
            character.TemporaryHp = Math.Max(0, character.TemporaryHp);
            character.DeathSaves = CharacterDeathSaveData.Clone(character.DeathSaves);
            character.ManualHp = Math.Max(0, character.ManualHp);
            character.HpRolls = NormalizeHpRolls(character.HpRolls, character.ClassId);
            character.HitDicePools = CharacterHitDicePoolSaveData.CloneList(character.HitDicePools);
            character.Equipment = CharacterEquipmentSetSaveData.Clone(character.Equipment);
            character.Currency = CharacterCurrencySaveData.Clone(character.Currency);
            character.CarryingCapacity = CharacterCarryingCapacitySaveData.Clone(character.CarryingCapacity);
            character.AttackActions = CharacterAttackActionSaveData.CloneList(character.AttackActions);
            character.Spellcasting = CharacterSpellcastingSaveData.Clone(character.Spellcasting);
            character.Resources = CharacterResourceSaveData.CloneList(character.Resources);
            character.Conditions = CharacterConditionStateSaveData.CloneList(character.Conditions);
            character.TemporaryEffects = CharacterTemporaryEffectSaveData.CloneList(character.TemporaryEffects);
            character.RuntimeSnapshot = CharacterRuntimeSnapshotData.Clone(character.RuntimeSnapshot);

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

        private static List<CharacterClassProgressSaveData> NormalizeClassProgresses(List<CharacterClassProgressSaveData> source, string legacyClassId, int legacyLevel)
        {
            List<CharacterClassProgressSaveData> result = new List<CharacterClassProgressSaveData>();
            if (source != null)
            {
                for (int index = 0; index < source.Count; index++)
                {
                    CharacterClassProgressSaveData progress = source[index];
                    if (progress == null || string.IsNullOrWhiteSpace(progress.ClassId))
                    {
                        continue;
                    }

                    result.Add(new CharacterClassProgressSaveData
                    {
                        ClassId = progress.ClassId.Trim(),
                        SubclassId = progress.SubclassId ?? string.Empty,
                        Level = Math.Max(1, progress.Level)
                    });
                }
            }

            if (result.Count == 0 && !string.IsNullOrWhiteSpace(legacyClassId))
            {
                result.Add(new CharacterClassProgressSaveData
                {
                    ClassId = legacyClassId.Trim(),
                    Level = Math.Max(1, legacyLevel)
                });
            }

            return result;
        }

        private static List<CharacterChoiceSelectionSaveData> NormalizeChoiceSelections(List<CharacterChoiceSelectionSaveData> source)
        {
            List<CharacterChoiceSelectionSaveData> result = new List<CharacterChoiceSelectionSaveData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterChoiceSelectionSaveData selection = source[index];
                if (selection == null || string.IsNullOrWhiteSpace(selection.ChoiceGroupId) || string.IsNullOrWhiteSpace(selection.OptionId))
                {
                    continue;
                }

                result.Add(new CharacterChoiceSelectionSaveData
                {
                    ChoiceGroupId = selection.ChoiceGroupId.Trim(),
                    OptionId = selection.OptionId.Trim(),
                    SourceType = selection.SourceType?.Trim() ?? string.Empty,
                    SourceId = selection.SourceId?.Trim() ?? string.Empty,
                    ClassId = selection.ClassId?.Trim() ?? string.Empty,
                    Level = Math.Max(0, selection.Level)
                });
            }

            return result;
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
