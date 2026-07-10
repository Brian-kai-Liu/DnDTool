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
        private readonly List<CharacterListItemViewState> m_characterListItems = new List<CharacterListItemViewState>();
        private readonly List<CharacterCardListItemView> m_cardViews = new List<CharacterCardListItemView>();
        private int m_selectedCharacterIndex = -1;
        private CharacterInventoryQuickRollContext m_visibleInventoryQuickRollContext;
        private CharacterDiceRollResultData m_visibleInventoryQuickRollResult;
        private string m_visibleInventoryQuickRollHistoryEntryId = string.Empty;
        private string m_visibleInventoryQuickRollCharacterId = string.Empty;
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

            SetText(m_tmpCharacterName, FormatTextOrDefault(snapshot.CharacterName, "未选择角色"));
            SetText(m_tmpRace, FormatTextOrDefault(snapshot.RaceName, "未选择种族"));
            SetText(m_tmpClass, CharacterDetailDisplayService.Instance.BuildClassNameSummary(character, snapshot));
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
            SetText(m_tmpProficiencyBonus, CharacterDetailCalculationService.Instance.BuildProficiencyBonusDisplay(snapshot.Level).Label);
            SetSkillBonusTexts(character, snapshot);
            SetText(m_tmpEquipmentTools, "装备与工具熟练项");
            RefreshEquipmentToolItems(snapshot);
            RefreshInventoryItems(character.Equipment);
            RefreshRaceFeatureSection(character, snapshot, character.RaceId);
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
            SetText(m_tmpProficiencyBonus, CharacterDetailCalculationService.Instance.BuildProficiencyBonusDisplay(1).Label);
            SetSkillBonusTexts(null, empty);
            SetText(m_tmpEquipmentTools, "装备与工具熟练项");
            RefreshEquipmentToolItems(empty);
            RefreshInventoryItems(null);
            RefreshRaceFeatureSection(null, empty, string.Empty);
            RefreshOtherFeatureSection(null);
            ShowFeatureDetail("特性详情", string.Empty);
        }

        private void SetAbilityTexts(CharacterRuntimeSnapshotData snapshot)
        {
            CharacterAbilityDisplayViewState state = CharacterDetailCalculationService.Instance.BuildAbilityDisplay(snapshot);
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
            List<CharacterStatusEffectDisplayEntry> entries = CharacterDetailDisplayService.Instance.BuildStatusEffectEntries(character);
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
            SetText(m_tmpDeathSaveSuccesses, $"成功 {normalized.Successes}/3");
            SetText(m_tmpDeathSaveFailures, $"失败 {normalized.Failures}/3");
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
            button.onClick.AddListener(() => OnClickInventoryItem(entry));
        }

        private void OnClickInventoryItem(CharacterInventoryDisplayEntry entry)
        {
            ShowFeatureDetail(entry.Title, entry.Description);
            if (!HasSelectedCharacter())
            {
                return;
            }

            CharacterCardDraftSaveData character = m_characterCards[m_selectedCharacterIndex];
            CharacterEquipmentItemSaveData item = FindInventoryItem(character?.Equipment, entry.ItemInstanceId);
            CharacterInventoryQuickRollContext context = BuildInventoryQuickRollContext(item);
            if (context == null)
            {
                return;
            }

            m_visibleInventoryQuickRollContext = context;
            m_visibleInventoryQuickRollResult = null;
            m_visibleInventoryQuickRollHistoryEntryId = string.Empty;
            m_visibleInventoryQuickRollCharacterId = character?.CharacterId?.Trim() ?? string.Empty;

            SetText(m_tmpFeatureDetailTitle, "快捷掷骰");
            SetText(m_tmpFeatureDetailDescription, BuildInventoryQuickRollPendingText(context, entry.Description));
            GameModule.UI.ShowUIAsync<DiceRollUI>(new DiceRollUIRequest
            {
                SourceType = "character_inventory_item",
                SourceId = context.ItemInstanceId,
                SourceName = context.ItemName,
                EffectName = context.EffectName,
                DiceExpression = context.DiceExpression,
                OnResult = OnInventoryDiceRollUIResult
            });
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
            SetText(m_tmpFeatureDetailTitle, "快捷掷骰");
            SetText(m_tmpFeatureDetailDescription, BuildInventoryQuickRollResultText(
                m_visibleInventoryQuickRollContext,
                m_visibleInventoryQuickRollResult,
                purpose));
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
                    DiceExpression = diceExpression
                };
            }

            return null;
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
