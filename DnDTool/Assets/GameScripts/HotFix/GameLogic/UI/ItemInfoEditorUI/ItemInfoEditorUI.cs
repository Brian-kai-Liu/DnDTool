using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using TEngine;
using UnityEngine;
using UnityEngine.UI;
using Log = TEngine.Log;

namespace GameLogic
{
    [Window(UILayer.UI, location: "ItemInfoEditorUI", fullScreen: true)]
    internal sealed class ItemInfoEditorUI : UIWindow
    {
        private readonly struct DropdownOption
        {
            public readonly string Value;
            public readonly string Label;

            public DropdownOption(string value, string label)
            {
                Value = value;
                Label = label;
            }
        }

        private static readonly DropdownOption[] SourceTypeOptions =
        {
            new DropdownOption(CharacterItemSourceTypes.Manual, "手动录入"),
            new DropdownOption(CharacterItemSourceTypes.Custom, "自定义物品"),
            new DropdownOption(CharacterItemSourceTypes.RuleTable, "规则表")
        };

        private static readonly DropdownOption[] ArmorCategoryOptions =
        {
            new DropdownOption(CharacterArmorCategoryIds.None, "无"),
            new DropdownOption(CharacterArmorCategoryIds.Light, "轻甲"),
            new DropdownOption(CharacterArmorCategoryIds.Medium, "中甲"),
            new DropdownOption(CharacterArmorCategoryIds.Heavy, "重甲")
        };

        private static readonly DropdownOption[] EffectTypeOptions =
        {
            new DropdownOption("AbilityScoreBonus", "属性加值"),
            new DropdownOption("AbilityScoreSet", "属性设定值"),
            new DropdownOption("ACBonus", "护甲等级加值"),
            new DropdownOption("SpeedBonus", "移动速度加值"),
            new DropdownOption("InitiativeBonus", "先攻加值"),
            new DropdownOption("SkillBonus", "技能加值"),
            new DropdownOption("SavingThrowBonus", "豁免加值"),
            new DropdownOption("SpellAttackBonus", "法术攻击加值"),
            new DropdownOption("SpellSaveDcBonus", "法术豁免DC加值"),
            new DropdownOption("AttackBonus", "攻击加值"),
            new DropdownOption("WeaponAttackBonus", "武器攻击加值"),
            new DropdownOption("DamageBonus", "伤害加值"),
            new DropdownOption("SkillProficiency", "技能熟练"),
            new DropdownOption("ToolProficiency", "工具熟练"),
            new DropdownOption("WeaponProficiency", "武器熟练"),
            new DropdownOption("ArmorProficiency", "护甲熟练"),
            new DropdownOption("SavingThrowProficiency", "豁免熟练"),
            new DropdownOption("Resistance", "伤害抗性"),
            new DropdownOption("Immunity", "伤害免疫"),
            new DropdownOption("Vulnerability", "伤害易伤"),
            new DropdownOption("ManualResolve", "手动处理")
        };

        private static readonly DropdownOption[] WeaponCategoryOptions =
        {
            new DropdownOption(string.Empty, "None"),
            new DropdownOption("simple_melee", "Simple Melee"),
            new DropdownOption("simple_ranged", "Simple Ranged"),
            new DropdownOption("martial_melee", "Martial Melee"),
            new DropdownOption("martial_ranged", "Martial Ranged")
        };

        private static readonly DropdownOption[] WeaponRangeTypeOptions =
        {
            new DropdownOption(string.Empty, "None"),
            new DropdownOption("melee", "Melee"),
            new DropdownOption("ranged", "Ranged"),
            new DropdownOption("thrown", "Thrown")
        };

        private static readonly DropdownOption[] DamageTypeOptions =
        {
            new DropdownOption(string.Empty, "None"),
            new DropdownOption("bludgeoning", "Bludgeoning"),
            new DropdownOption("piercing", "Piercing"),
            new DropdownOption("slashing", "Slashing"),
            new DropdownOption("acid", "Acid"),
            new DropdownOption("cold", "Cold"),
            new DropdownOption("fire", "Fire"),
            new DropdownOption("force", "Force"),
            new DropdownOption("lightning", "Lightning"),
            new DropdownOption("necrotic", "Necrotic"),
            new DropdownOption("poison", "Poison"),
            new DropdownOption("psychic", "Psychic"),
            new DropdownOption("radiant", "Radiant"),
            new DropdownOption("thunder", "Thunder")
        };

        private static readonly DropdownOption[] ToolCategoryOptions =
        {
            new DropdownOption(string.Empty, "None"),
            new DropdownOption("artisan_tools", "Artisan Tools"),
            new DropdownOption("gaming_set", "Gaming Set"),
            new DropdownOption("musical_instrument", "Musical Instrument"),
            new DropdownOption("other", "Other")
        };

        private static readonly DropdownOption[] BoolOptions =
        {
            new DropdownOption("false", "No"),
            new DropdownOption("true", "Yes")
        };

        private static readonly DropdownOption[] EquipmentSlotOptions =
        {
            new DropdownOption(string.Empty, "None"),
            new DropdownOption("armor", "Armor"),
            new DropdownOption("shield", "Shield"),
            new DropdownOption("weapon", "Weapon"),
            new DropdownOption("offhand", "Offhand"),
            new DropdownOption("focus", "Focus"),
            new DropdownOption("tool", "Tool"),
            new DropdownOption("accessory", "Accessory"),
            new DropdownOption("clothing", "Clothing"),
            new DropdownOption("mount_vehicle", "Mount/Vehicle"),
            new DropdownOption("manual", "Manual")
        };

        private static readonly DropdownOption[] AffixEffectTargetOptions =
        {
            new DropdownOption("Manual", "手动处理"),
            new DropdownOption("AC", "护甲等级"),
            new DropdownOption("Attack", "攻击"),
            new DropdownOption("Damage", "伤害"),
            new DropdownOption("Speed", "速度"),
            new DropdownOption("Initiative", "先攻"),
            new DropdownOption("SpellAttack", "法术攻击"),
            new DropdownOption("SpellSaveDC", "法术豁免 DC"),
            new DropdownOption("Strength", "力量"),
            new DropdownOption("Dexterity", "敏捷"),
            new DropdownOption("Constitution", "体质"),
            new DropdownOption("Intelligence", "智力"),
            new DropdownOption("Wisdom", "感知"),
            new DropdownOption("Charisma", "魅力")
        };

        private static readonly DropdownOption[] RarityOptions =
        {
            new DropdownOption(string.Empty, "无"),
            new DropdownOption("common", "普通"),
            new DropdownOption("uncommon", "罕见"),
            new DropdownOption("rare", "稀有"),
            new DropdownOption("very_rare", "非常稀有"),
            new DropdownOption("legendary", "传奇"),
            new DropdownOption("artifact", "神器")
        };

        private static readonly DropdownOption[] AbilityTargetOptions =
        {
            new DropdownOption("strength", "力量"),
            new DropdownOption("dexterity", "敏捷"),
            new DropdownOption("constitution", "体质"),
            new DropdownOption("intelligence", "智力"),
            new DropdownOption("wisdom", "感知"),
            new DropdownOption("charisma", "魅力"),
            new DropdownOption("All", "全部属性")
        };

        private static readonly DropdownOption[] SavingThrowTargetOptions =
        {
            new DropdownOption("strength", "力量豁免"),
            new DropdownOption("dexterity", "敏捷豁免"),
            new DropdownOption("constitution", "体质豁免"),
            new DropdownOption("intelligence", "智力豁免"),
            new DropdownOption("wisdom", "感知豁免"),
            new DropdownOption("charisma", "魅力豁免"),
            new DropdownOption("All", "全部豁免")
        };

        private static readonly DropdownOption[] AcTargetOptions =
        {
            new DropdownOption("AC", "护甲等级")
        };

        private static readonly DropdownOption[] SpeedTargetOptions =
        {
            new DropdownOption("walk", "步行速度")
        };

        private static readonly DropdownOption[] InitiativeTargetOptions =
        {
            new DropdownOption("Initiative", "先攻")
        };

        private static readonly DropdownOption[] SpellAttackTargetOptions =
        {
            new DropdownOption("SpellAttack", "法术攻击")
        };

        private static readonly DropdownOption[] SpellSaveDcTargetOptions =
        {
            new DropdownOption("SpellSaveDC", "法术豁免 DC")
        };

        private static readonly DropdownOption[] AttackTargetOptions =
        {
            new DropdownOption("Attack", "攻击")
        };

        private static readonly DropdownOption[] WeaponAttackTargetOptions =
        {
            new DropdownOption("WeaponAttack", "武器攻击")
        };

        private static readonly DropdownOption[] DamageTargetOptions =
        {
            new DropdownOption("Damage", "伤害")
        };

        private static readonly DropdownOption[] ArmorProficiencyTargetOptions =
        {
            new DropdownOption("light_armor", "轻甲"),
            new DropdownOption("medium_armor", "中甲"),
            new DropdownOption("heavy_armor", "重甲"),
            new DropdownOption("shield", "盾牌")
        };

        private static readonly DropdownOption[] WeaponProficiencyTargetOptions =
        {
            new DropdownOption("simple_weapon", "简易武器"),
            new DropdownOption("martial_weapon", "军用武器")
        };

        private static readonly DropdownOption[] DamageTypeTargetOptions =
        {
            new DropdownOption("acid", "强酸"),
            new DropdownOption("bludgeoning", "钝击"),
            new DropdownOption("cold", "寒冷"),
            new DropdownOption("fire", "火焰"),
            new DropdownOption("force", "力场"),
            new DropdownOption("lightning", "闪电"),
            new DropdownOption("necrotic", "黯蚀"),
            new DropdownOption("piercing", "穿刺"),
            new DropdownOption("poison", "毒素"),
            new DropdownOption("psychic", "心灵"),
            new DropdownOption("radiant", "光耀"),
            new DropdownOption("slashing", "挥砍"),
            new DropdownOption("thunder", "雷鸣")
        };

        private static readonly DropdownOption[] ManualTargetOptions =
        {
            new DropdownOption("Manual", "手动处理")
        };

        private static readonly DropdownOption[] ConditionOptions =
        {
            new DropdownOption("Always", "始终生效"),
            new DropdownOption("Equipped", "装备时生效"),
            new DropdownOption("Worn", "穿戴时生效"),
            new DropdownOption("Wielded", "持握时生效"),
            new DropdownOption("WearingArmor", "穿着护甲时生效"),
            new DropdownOption("Attuned", "同调后生效"),
            new DropdownOption("EquippedAndAttuned", "装备且同调后生效"),
            new DropdownOption("Manual", "手动判断"),
            new DropdownOption("DMJudgement", "DM 裁定"),
            new DropdownOption("Situational", "情境条件"),
            new DropdownOption("Activated", "激活后生效"),
            new DropdownOption("UseToApply", "使用后生效")
        };

        private Button m_btnCancelItem;
        private Button m_btnSaveItem;
        private Button m_btnAddToCharacter;
        private Button m_btnLoadFromRuleTable;
        private Button m_btnAddEffect;
        private Button m_btnRemoveEffect;
        private TMP_InputField m_inputItemName;
        private TMP_InputField m_inputDescription;
        private TMP_InputField m_inputNotes;
        private TMP_InputField m_inputSourceItemId;
        private TMP_InputField m_inputArmorBaseAc;
        private TMP_InputField m_inputAcBonus;
        private TMP_InputField m_inputMaxDexBonus;
        private TMP_InputField m_inputStrengthRequirement;
        private TMP_InputField m_inputDamageDice;
        private TMP_InputField m_inputWeaponProperties;
        private TMP_InputField m_inputNormalRange;
        private TMP_InputField m_inputLongRange;
        private TMP_InputField m_inputTwoHandDamageDice;
        private TMP_InputField m_inputCharges;
        private TMP_InputField m_inputMaxCharges;
        private TMP_InputField m_inputPriceGp;
        private TMP_InputField m_inputWeight;
        private TMP_InputField m_inputEffectTarget;
        private TMP_InputField m_inputEffectValue;
        private TMP_InputField m_inputEffectCondition;
        private TMP_InputField m_inputEffectConditionDescription;
        private TMP_InputField m_inputEffectDescription;
        private TMP_InputField m_inputEffectApplyCondition;
        private TMP_Dropdown m_dropdownEffectTarget;
        private TMP_Dropdown m_dropdownEffectCondition;
        private TMP_Dropdown m_dropdownItemType;
        private TMP_Dropdown m_dropdownRarity;
        private TMP_Dropdown m_dropdownItemSourceType;
        private TMP_Dropdown m_dropdownArmorCategory;
        private TMP_Dropdown m_dropdownStealthDisadvantage;
        private TMP_Dropdown m_dropdownWeaponCategory;
        private TMP_Dropdown m_dropdownWeaponRangeType;
        private TMP_Dropdown m_dropdownDamageType;
        private TMP_Dropdown m_dropdownToolCategory;
        private TMP_Dropdown m_dropdownEquipmentSlot;
        private TMP_Dropdown m_dropdownRequiresAttunement;
        private TMP_Dropdown m_dropdownConsumable;
        private TMP_Dropdown m_dropdownConsumeOnUse;
        private TMP_Dropdown m_dropdownEffectType;
        private TMP_Text m_tmpPreviewItemName;
        private TMP_Text m_tmpPreviewItemType;
        private TMP_Text m_tmpPreviewItemNum;
        private TMP_Text m_tmpPreviewItemRare;
        private TMP_Text m_tmpPreviewSource;
        private TMP_Text m_tmpPreviewAc;
        private TMP_Text m_tmpPreviewEffects;
        private ScrollRect m_scrollItemForm;
        private RectTransform m_rectItemFormContent;
        private RectTransform m_panelItemAffixSection;
        private RectTransform m_sectionItemAffixTitle;
        private RectTransform m_rectAffixListContent;
        private GameObject m_itemAffixTemplate;
        private Button m_btnAddAffix;
        private RectTransform m_panelCharacterPicker;
        private RectTransform m_rectCharacterPickerContent;
        private GameObject m_goCharacterPickerTemplate;
        private Button m_btnCancelCharacterPicker;
        private Button m_btnConfirmAddToCharacter;
        private TMP_Text m_tmpCharacterPickerMessage;

        private readonly List<CharacterItemEffectSaveData> m_customEffects = new List<CharacterItemEffectSaveData>();
        private readonly List<string> m_ruleEffectIds = new List<string>();
        private readonly List<ItemAffixRowBinding> m_affixRows = new List<ItemAffixRowBinding>();
        private readonly List<GameObject> m_characterPickerItems = new List<GameObject>();
        private List<ItemEditorCharacterPickerEntry> m_characterPickerCharacters = new List<ItemEditorCharacterPickerEntry>();
        private List<DropdownOption> m_itemTypeOptions = new List<DropdownOption>();
        private List<DropdownOption> m_currentEffectTargetOptions = new List<DropdownOption>();
        private int m_selectedCharacterPickerIndex = -1;
        private string m_currentCustomItemId = string.Empty;
        private ItemInfoEditorUIRequest m_request;

        private sealed class ItemAffixRowBinding
        {
            public GameObject Root;
            public TMP_InputField NameInput;
            public TMP_Dropdown ConditionDropdown;
            public TMP_Dropdown EffectTargetDropdown;
            public TMP_InputField ValueInput;
            public TMP_InputField DescriptionInput;
            public TMP_InputField DiceExpressionInput;
            public Button RemoveButton;
        }

        protected override void ScriptGenerator()
        {
            m_request = UserData as ItemInfoEditorUIRequest ?? new ItemInfoEditorUIRequest();
            BindControls();
            InitializeDropdowns();
            BindButtons();
            RefreshPreview();
            RefreshEffectPreview();
            RefreshItemFormScrollArea(true);
        }

        protected override void OnUpdate()
        {
            RefreshDropdownRuntimeLayers();
        }

        private void BindControls()
        {
            m_btnCancelItem = FindChildComponent<Button>("m_btnCancelItem");
            m_btnSaveItem = FindChildComponent<Button>("m_btnSaveItem");
            m_btnAddToCharacter = FindChildComponent<Button>("m_btnAddToCharacter");
            m_btnLoadFromRuleTable = FindChildComponent<Button>("m_btnLoadFromRuleTable");
            m_btnAddEffect = FindChildComponent<Button>("m_btnAddEffect");
            m_btnRemoveEffect = FindChildComponent<Button>("m_btnRemoveEffect");
            m_scrollItemForm = FindChildComponent<ScrollRect>("m_scrollItemForm");
            m_rectItemFormContent = FindChildComponent<RectTransform>("m_rectItemFormContent");

            m_inputItemName = FindChildComponent<TMP_InputField>("m_inputItemName");
            m_inputDescription = FindChildComponent<TMP_InputField>("m_inputDescription");
            m_inputNotes = FindChildComponent<TMP_InputField>("m_inputNotes");
            m_inputSourceItemId = FindChildComponent<TMP_InputField>("m_inputSourceItemId");
            m_inputArmorBaseAc = FindChildComponent<TMP_InputField>("m_inputArmorBaseAc");
            m_inputAcBonus = FindChildComponent<TMP_InputField>("m_inputAcBonus");
            m_inputMaxDexBonus = FindChildComponent<TMP_InputField>("m_inputMaxDexBonus");
            m_inputStrengthRequirement = FindChildComponent<TMP_InputField>("m_inputStrengthRequirement");
            m_inputDamageDice = FindChildComponent<TMP_InputField>("m_inputDamageDice");
            m_inputWeaponProperties = FindChildComponent<TMP_InputField>("m_inputWeaponProperties");
            m_inputNormalRange = FindChildComponent<TMP_InputField>("m_inputNormalRange");
            m_inputLongRange = FindChildComponent<TMP_InputField>("m_inputLongRange");
            m_inputTwoHandDamageDice = FindChildComponent<TMP_InputField>("m_inputTwoHandDamageDice");
            m_inputCharges = FindChildComponent<TMP_InputField>("m_inputCharges");
            m_inputMaxCharges = FindChildComponent<TMP_InputField>("m_inputMaxCharges") ?? m_inputCharges;
            m_inputPriceGp = FindChildComponent<TMP_InputField>("m_inputPriceGp");
            m_inputWeight = FindChildComponent<TMP_InputField>("m_inputWeight");
            m_inputEffectTarget = FindChildComponent<TMP_InputField>("m_inputEffectTarget");
            m_inputEffectValue = FindChildComponent<TMP_InputField>("m_inputEffectValue");
            m_inputEffectCondition = FindChildComponent<TMP_InputField>("m_inputEffectCondition");
            m_inputEffectDescription = FindChildComponent<TMP_InputField>("m_inputEffectDescription");
            m_inputEffectApplyCondition = FindChildComponent<TMP_InputField>("m_inputEffectApplyCondition");

            m_dropdownItemType = FindChildComponent<TMP_Dropdown>("m_dropdownItemType");
            m_dropdownItemSourceType = FindChildComponent<TMP_Dropdown>("m_dropdownItemSourceType");
            m_dropdownArmorCategory = FindChildComponent<TMP_Dropdown>("m_dropdownArmorCategory");
            m_dropdownStealthDisadvantage = FindChildComponent<TMP_Dropdown>("m_dropdownStealthDisadvantage");
            m_dropdownWeaponCategory = FindChildComponent<TMP_Dropdown>("m_dropdownWeaponCategory");
            m_dropdownWeaponRangeType = FindChildComponent<TMP_Dropdown>("m_dropdownWeaponRangeType");
            m_dropdownDamageType = FindChildComponent<TMP_Dropdown>("m_dropdownDamageType");
            m_dropdownToolCategory = FindChildComponent<TMP_Dropdown>("m_dropdownToolCategory");
            m_dropdownEquipmentSlot = FindChildComponent<TMP_Dropdown>("m_dropdownEquipmentSlot");
            m_dropdownRequiresAttunement = FindChildComponent<TMP_Dropdown>("m_dropdownRequiresAttunement");
            m_dropdownConsumable = FindChildComponent<TMP_Dropdown>("m_dropdownConsumable");
            m_dropdownConsumeOnUse = FindChildComponent<TMP_Dropdown>("m_dropdownConsumeOnUse");
            m_dropdownEffectType = FindChildComponent<TMP_Dropdown>("m_dropdownEffectType");
            m_inputEffectConditionDescription = FindChildComponent<TMP_InputField>("m_inputEffectConditionDescription")
                ?? CreateEffectConditionDescriptionInput();
            m_dropdownEffectTarget = FindChildComponent<TMP_Dropdown>("m_dropdownEffectTarget")
                ?? CreateDropdownFromInput(m_inputEffectTarget, "m_dropdownEffectTarget");
            m_dropdownEffectCondition = FindChildComponent<TMP_Dropdown>("m_dropdownEffectCondition")
                ?? CreateDropdownFromInput(m_inputEffectCondition, "m_dropdownEffectCondition");

            m_tmpPreviewItemName = FindChildComponent<TMP_Text>("m_tmpPreviewItemName");
            m_tmpPreviewItemType = FindChildComponent<TMP_Text>("m_tmpPreviewItemType");
            m_tmpPreviewItemNum = FindChildComponent<TMP_Text>("m_tmpPreviewItemNum");
            m_tmpPreviewItemRare = FindChildComponent<TMP_Text>("m_tmpPreviewItemRare");
            m_tmpPreviewSource = FindChildComponent<TMP_Text>("m_tmpPreviewSource");
            m_tmpPreviewAc = FindChildComponent<TMP_Text>("m_tmpPreviewAc");
            m_tmpPreviewEffects = FindChildComponent<TMP_Text>("m_tmpPreviewEffects");
            m_dropdownRarity = FindChildComponent<TMP_Dropdown>("m_dropdownRarity")
                ?? CreateDropdownFromInput(FindChildComponent<TMP_InputField>("m_dropdownRarity"), "m_dropdownRarity");
            m_panelItemAffixSection = FindChildComponent<RectTransform>("m_panelItemAffixSection");
            m_sectionItemAffixTitle = FindChildComponent<RectTransform>("m_sectionItemAffixTitle");
            m_rectAffixListContent = FindChildComponent<RectTransform>("m_rectAffixListContent");
            m_itemAffixTemplate = FindChildComponent<RectTransform>("m_itemAffixTemplate")?.gameObject;
            m_btnAddAffix = FindChildComponent<Button>("m_btnAddAffix");
            m_panelCharacterPicker = FindChildComponent<RectTransform>("m_panelCharacterPicker");
            m_rectCharacterPickerContent = FindChildComponent<RectTransform>("m_rectCharacterPickerContent");
            m_goCharacterPickerTemplate = FindChildComponent<RectTransform>("m_itemCharacterPickerTemplate")?.gameObject;
            m_btnCancelCharacterPicker = FindChildComponent<Button>("m_btnCancelCharacterPicker");
            m_btnConfirmAddToCharacter = FindChildComponent<Button>("m_btnConfirmAddToCharacter");
            m_tmpCharacterPickerMessage = FindChildComponent<TMP_Text>("m_tmpCharacterPickerMessage");
        }

        private void InitializeDropdowns()
        {
            m_itemTypeOptions = BuildItemTypeOptions();
            EnsureDropdownTemplates();
            SetupDropdown(m_dropdownItemType, m_itemTypeOptions);
            SetupDropdown(m_dropdownRarity, RarityOptions);
            SetupDropdown(m_dropdownItemSourceType, SourceTypeOptions);
            SetupDropdown(m_dropdownArmorCategory, ArmorCategoryOptions);
            SetupDropdown(m_dropdownStealthDisadvantage, BoolOptions);
            SetupDropdown(m_dropdownWeaponCategory, WeaponCategoryOptions);
            SetupDropdown(m_dropdownWeaponRangeType, WeaponRangeTypeOptions);
            SetupDropdown(m_dropdownDamageType, DamageTypeOptions);
            SetupDropdown(m_dropdownToolCategory, ToolCategoryOptions);
            SetupDropdown(m_dropdownEquipmentSlot, EquipmentSlotOptions);
            SetupDropdown(m_dropdownRequiresAttunement, BoolOptions);
            SetupDropdown(m_dropdownConsumable, BoolOptions);
            SetupDropdown(m_dropdownConsumeOnUse, BoolOptions);
            SetupDropdown(m_dropdownEffectType, EffectTypeOptions);
            SetupDropdown(m_dropdownEffectCondition, ConditionOptions);
            RefreshEffectTargetOptions();
            RefreshEffectValueInputVisibility();
            RefreshCategoryFieldVisibility();
            InitializeAffixRows();
        }

        private void EnsureDropdownTemplates()
        {
            EnsureDropdownTemplate(m_dropdownItemType);
            EnsureDropdownTemplate(m_dropdownRarity);
            EnsureDropdownTemplate(m_dropdownItemSourceType);
            EnsureDropdownTemplate(m_dropdownArmorCategory);
            EnsureDropdownTemplate(m_dropdownStealthDisadvantage);
            EnsureDropdownTemplate(m_dropdownWeaponCategory);
            EnsureDropdownTemplate(m_dropdownWeaponRangeType);
            EnsureDropdownTemplate(m_dropdownDamageType);
            EnsureDropdownTemplate(m_dropdownToolCategory);
            EnsureDropdownTemplate(m_dropdownEquipmentSlot);
            EnsureDropdownTemplate(m_dropdownRequiresAttunement);
            EnsureDropdownTemplate(m_dropdownConsumable);
            EnsureDropdownTemplate(m_dropdownConsumeOnUse);
            EnsureDropdownTemplate(m_dropdownEffectType);
            EnsureDropdownTemplate(m_dropdownEffectTarget);
            EnsureDropdownTemplate(m_dropdownEffectCondition);
        }

        private void BindButtons()
        {
            BindButton(m_btnCancelItem, ReturnHome);
            BindButton(m_btnSaveItem, SaveItem);
            BindButton(m_btnAddToCharacter, AddToCharacter);
            BindButton(m_btnLoadFromRuleTable, LoadFromRuleTable);
            BindButton(m_btnAddEffect, AddEffect);
            BindButton(m_btnRemoveEffect, RemoveEffect);
            BindButton(m_btnAddAffix, AddAffixRow);
            BindButton(m_btnCancelCharacterPicker, HideCharacterPicker);
            BindButton(m_btnConfirmAddToCharacter, ConfirmAddToCharacter);

            BindInput(m_inputItemName, RefreshPreview);
            BindInput(m_inputDescription, RefreshPreview);
            BindInput(m_inputNotes, RefreshPreview);
            BindInput(m_inputSourceItemId, RefreshPreview);
            BindInput(m_inputArmorBaseAc, RefreshPreview);
            BindInput(m_inputAcBonus, RefreshPreview);
            BindInput(m_inputMaxDexBonus, RefreshPreview);
            BindInput(m_inputStrengthRequirement, RefreshPreview);
            BindInput(m_inputDamageDice, RefreshPreview);
            BindInput(m_inputWeaponProperties, RefreshPreview);
            BindInput(m_inputNormalRange, RefreshPreview);
            BindInput(m_inputLongRange, RefreshPreview);
            BindInput(m_inputTwoHandDamageDice, RefreshPreview);
            BindInput(m_inputCharges, RefreshPreview);
            BindInput(m_inputMaxCharges, RefreshPreview);
            BindInput(m_inputPriceGp, RefreshPreview);
            BindInput(m_inputWeight, RefreshPreview);
            BindInput(m_inputEffectTarget, RefreshPreview);
            BindInput(m_inputEffectValue, RefreshPreview);
            BindInput(m_inputEffectCondition, RefreshPreview);
            BindInput(m_inputEffectConditionDescription, RefreshPreview);
            BindInput(m_inputEffectDescription, RefreshPreview);
            BindInput(m_inputEffectApplyCondition, RefreshPreview);

            BindDropdown(m_dropdownItemType, OnItemTypeChanged);
            BindDropdown(m_dropdownRarity, RefreshPreview);
            BindDropdown(m_dropdownItemSourceType, RefreshPreview);
            BindDropdown(m_dropdownArmorCategory, RefreshPreview);
            BindDropdown(m_dropdownStealthDisadvantage, RefreshPreview);
            BindDropdown(m_dropdownWeaponCategory, RefreshPreview);
            BindDropdown(m_dropdownWeaponRangeType, RefreshPreview);
            BindDropdown(m_dropdownDamageType, RefreshPreview);
            BindDropdown(m_dropdownToolCategory, RefreshPreview);
            BindDropdown(m_dropdownEquipmentSlot, RefreshPreview);
            BindDropdown(m_dropdownRequiresAttunement, RefreshPreview);
            BindDropdown(m_dropdownConsumable, RefreshPreview);
            BindDropdown(m_dropdownConsumeOnUse, RefreshPreview);
            BindDropdown(m_dropdownEffectType, OnEffectTypeChanged);
            BindDropdown(m_dropdownEffectTarget, OnEffectTargetChanged);
            BindDropdown(m_dropdownEffectCondition, OnEffectConditionChanged);

        }

        private void LoadFromRuleTable()
        {
            string sourceItemId = GetInputText(m_inputSourceItemId);
            if (string.IsNullOrWhiteSpace(sourceItemId))
            {
                Log.Warning("ItemInfoEditorUI: source item id is empty.");
                return;
            }

            if (!ItemEditorApplicationService.Instance.TryGetRuleItem(sourceItemId, out ItemEditorRuleItemViewState ruleItem))
            {
                Log.Warning($"ItemInfoEditorUI: rule item not found: {sourceItemId}");
                return;
            }

            SetInputText(m_inputItemName, ruleItem.Name);
            SetInputText(m_inputDescription, ruleItem.Description);
            SetInputText(m_inputNotes, string.Empty);
            SetInputText(m_inputArmorBaseAc, ruleItem.ArmorBaseAc.ToString());
            SetInputText(m_inputAcBonus, ruleItem.AcBonus.ToString());
            SetInputText(m_inputMaxDexBonus, ruleItem.MaxDexBonus.ToString());
            SetInputText(m_inputStrengthRequirement, ruleItem.StrengthRequirement.ToString());
            SetInputText(m_inputDamageDice, ruleItem.DamageDice);
            SetInputText(m_inputWeaponProperties, FormatStringList(ruleItem.WeaponProperties));
            SetInputText(m_inputNormalRange, ruleItem.NormalRange.ToString());
            SetInputText(m_inputLongRange, ruleItem.LongRange.ToString());
            SetInputText(m_inputTwoHandDamageDice, ruleItem.TwoHandDamageDice);
            SetInputText(m_inputMaxCharges, ruleItem.Charges.ToString());
            SetInputText(m_inputPriceGp, ruleItem.PriceGp.ToString(CultureInfo.InvariantCulture));
            SetInputText(m_inputWeight, ruleItem.Weight);
            SetInputText(m_inputEffectApplyCondition, ruleItem.EffectApplyCondition);
            m_ruleEffectIds.Clear();
            m_ruleEffectIds.AddRange(ruleItem.EffectIds);
            SelectDropdownValue(m_dropdownItemSourceType, SourceTypeOptions, CharacterItemSourceTypes.RuleTable);
            SelectDropdownValue(m_dropdownItemType, m_itemTypeOptions, ResolveSelectableItemType(ruleItem));
            SelectDropdownValue(m_dropdownRarity, RarityOptions, NormalizeOptionOrFirst(RarityOptions, ruleItem.Rarity));
            SelectDropdownValue(m_dropdownArmorCategory, ArmorCategoryOptions, NormalizeOptionOrFirst(ArmorCategoryOptions, ruleItem.ArmorCategory));
            SelectDropdownValue(m_dropdownStealthDisadvantage, BoolOptions, ToBoolOptionValue(ruleItem.StealthDisadvantage));
            SelectDropdownValue(m_dropdownWeaponCategory, WeaponCategoryOptions, NormalizeOptionOrFirst(WeaponCategoryOptions, ruleItem.WeaponCategory));
            SelectDropdownValue(m_dropdownWeaponRangeType, WeaponRangeTypeOptions, NormalizeOptionOrFirst(WeaponRangeTypeOptions, ruleItem.WeaponRangeType));
            SelectDropdownValue(m_dropdownDamageType, DamageTypeOptions, NormalizeOptionOrFirst(DamageTypeOptions, ruleItem.DamageType));
            SelectDropdownValue(m_dropdownToolCategory, ToolCategoryOptions, NormalizeOptionOrFirst(ToolCategoryOptions, ruleItem.ToolCategory));
            SelectDropdownValue(m_dropdownEquipmentSlot, EquipmentSlotOptions, NormalizeOptionOrFirst(EquipmentSlotOptions, ruleItem.EquipmentSlot));
            SelectDropdownValue(m_dropdownRequiresAttunement, BoolOptions, ToBoolOptionValue(ruleItem.RequiresAttunement));
            SelectDropdownValue(m_dropdownConsumable, BoolOptions, ToBoolOptionValue(ruleItem.Consumable));
            SelectDropdownValue(m_dropdownConsumeOnUse, BoolOptions, ToBoolOptionValue(ruleItem.ConsumeOnUse));
            RefreshCategoryFieldVisibility();
            RefreshPreview();
            RefreshEffectPreview();
        }

        private void AddEffect()
        {
            CharacterItemEffectSaveData effect = new CharacterItemEffectSaveData
            {
                EffectType = GetSelectedDropdownValue(m_dropdownEffectType, EffectTypeOptions),
                Target = ResolveEffectTargetValue(),
                Value = GetInputText(m_inputEffectValue),
                Condition = ResolveEffectConditionValue(),
                ConditionDescription = GetInputText(m_inputEffectConditionDescription),
                Description = GetInputText(m_inputEffectDescription)
            };

            if (string.IsNullOrWhiteSpace(effect.EffectType)
                && string.IsNullOrWhiteSpace(effect.Target)
                && string.IsNullOrWhiteSpace(effect.Value)
                && string.IsNullOrWhiteSpace(effect.Description))
            {
                Log.Warning("ItemInfoEditorUI: effect fields are empty.");
                return;
            }

            m_customEffects.Add(effect);
            RefreshEffectPreview();
            ClearEffectInput();
        }

        private void RemoveEffect()
        {
            if (m_customEffects.Count == 0)
            {
                return;
            }

            m_customEffects.RemoveAt(m_customEffects.Count - 1);
            RefreshEffectPreview();
        }

        private void SaveItem()
        {
            string customItemId = EnsureCustomItemId();
            CharacterOperationResult result = ItemEditorApplicationService.Instance.SaveCustomItem(customItemId, BuildItemData());
            if (!result.Success)
            {
                Log.Warning($"ItemInfoEditorUI: save custom item failed. {result.Message}");
                return;
            }

            Log.Info($"ItemInfoEditorUI: saved custom item {customItemId}");
        }

        private void AddToCharacter()
        {
            SaveItem();
            ShowCharacterPicker();
        }

        private void ShowCharacterPicker()
        {
            if (m_panelCharacterPicker == null)
            {
                Log.Warning("ItemInfoEditorUI: character picker panel is missing.");
                return;
            }

            m_characterPickerCharacters = ItemEditorApplicationService.Instance.LoadCharacterPickerEntries();
            m_selectedCharacterPickerIndex = m_characterPickerCharacters.Count > 0 ? 0 : -1;
            RefreshCharacterPickerItems();
            SetActive(m_panelCharacterPicker.gameObject, true);
        }

        private void HideCharacterPicker()
        {
            SetActive(m_panelCharacterPicker?.gameObject, false);
        }

        private void ConfirmAddToCharacter()
        {
            if (m_selectedCharacterPickerIndex < 0 || m_selectedCharacterPickerIndex >= m_characterPickerCharacters.Count)
            {
                SetText(m_tmpCharacterPickerMessage, "请选择一个角色");
                return;
            }

            ItemEditorCharacterPickerEntry character = m_characterPickerCharacters[m_selectedCharacterPickerIndex];
            ItemEditorAddItemResult result = ItemEditorApplicationService.Instance.AddCustomItemToCharacter(
                character.CharacterId,
                EnsureCustomItemId(),
                BuildItemData(),
                1);
            if (!result.Success)
            {
                Log.Warning($"ItemInfoEditorUI: add item to character failed. {result.Message}");
                SetText(m_tmpCharacterPickerMessage, result.Message);
                return;
            }

            SetText(m_tmpCharacterPickerMessage, $"已加入 {result.CharacterName} 的背包");
            HideCharacterPicker();
            Log.Info($"ItemInfoEditorUI: added item {result.ItemName} to character {result.CharacterName}.");
        }

        private void RefreshCharacterPickerItems()
        {
            EnsureCharacterPickerItemCount(m_characterPickerCharacters.Count);
            for (int index = 0; index < m_characterPickerItems.Count; index++)
            {
                GameObject item = m_characterPickerItems[index];
                bool visible = index < m_characterPickerCharacters.Count;
                SetActive(item, visible);
                if (!visible)
                {
                    continue;
                }

                ItemEditorCharacterPickerEntry character = m_characterPickerCharacters[index];
                SetCharacterPickerItem(item, character, index == m_selectedCharacterPickerIndex);
                BindCharacterPickerItem(item, index);
            }

            SetActive(m_goCharacterPickerTemplate, false);
            SetText(m_tmpCharacterPickerMessage, m_characterPickerCharacters.Count > 0 ? string.Empty : "当前没有本地角色数据");
        }

        private void EnsureCharacterPickerItemCount(int count)
        {
            if (m_rectCharacterPickerContent == null || m_goCharacterPickerTemplate == null)
            {
                return;
            }

            while (m_characterPickerItems.Count < count)
            {
                GameObject itemObject = UnityEngine.Object.Instantiate(m_goCharacterPickerTemplate, m_rectCharacterPickerContent);
                itemObject.name = $"m_itemCharacterPicker_{m_characterPickerItems.Count + 1}";
                m_characterPickerItems.Add(itemObject);
            }
        }

        private void SetCharacterPickerItem(GameObject item, ItemEditorCharacterPickerEntry character, bool selected)
        {
            if (item == null)
            {
                return;
            }

            TMP_Text nameText = item.transform.Find("m_tmpCharacterPickerName")?.GetComponent<TMP_Text>();
            TMP_Text summaryText = item.transform.Find("m_tmpCharacterPickerSummary")?.GetComponent<TMP_Text>();
            TMP_Text markText = item.transform.Find("m_tmpCharacterPickerSelectedMark")?.GetComponent<TMP_Text>();
            Image background = item.GetComponent<Image>();

            SetText(nameText, character.CharacterName);
            SetText(summaryText, character.Summary);
            SetText(markText, selected ? "已选择" : string.Empty);
            if (background != null)
            {
                background.color = selected ? new Color(0.20f, 0.38f, 0.54f, 1f) : new Color(0.17f, 0.18f, 0.21f, 1f);
            }
        }

        private void BindCharacterPickerItem(GameObject item, int index)
        {
            Button button = item != null ? item.GetComponent<Button>() : null;
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                m_selectedCharacterPickerIndex = index;
                RefreshCharacterPickerItems();
            });
        }

        private void InitializeAffixRows()
        {
            m_affixRows.Clear();
            if (m_itemAffixTemplate == null)
            {
                return;
            }

            SetActive(m_itemAffixTemplate, false);
        }

        private void AddAffixRow()
        {
            if (m_itemAffixTemplate == null || m_rectAffixListContent == null)
            {
                return;
            }

            GameObject rowObject = UnityEngine.Object.Instantiate(m_itemAffixTemplate, m_rectAffixListContent);
            rowObject.name = $"m_itemAffixRow_{m_affixRows.Count + 1}";
            SetActive(rowObject, true);

            ItemAffixRowBinding row = BindAffixRow(rowObject);
            m_affixRows.Add(row);
            RefreshAffixRowRemoveButtons();
            RefreshEffectPreview();
        }

        private ItemAffixRowBinding BindAffixRow(GameObject rowObject)
        {
            ItemAffixRowBinding row = new ItemAffixRowBinding
            {
                Root = rowObject,
                NameInput = FindDescendantComponent<TMP_InputField>(rowObject.transform, "m_inputAffixName"),
                ConditionDropdown = FindOrCreateAffixDropdown(rowObject.transform, "m_dropdownAffixCondition"),
                EffectTargetDropdown = FindOrCreateAffixDropdown(rowObject.transform, "m_dropdownAffixEffectTarget"),
                ValueInput = FindDescendantComponent<TMP_InputField>(rowObject.transform, "m_inputAffixEffectValue"),
                DescriptionInput = FindDescendantComponent<TMP_InputField>(rowObject.transform, "m_inputAffixDescription"),
                DiceExpressionInput = FindDescendantComponent<TMP_InputField>(rowObject.transform, "m_inputAffixDiceExpression"),
                RemoveButton = FindDescendantComponent<Button>(rowObject.transform, "m_btnRemoveAffix")
            };

            EnsureAffixDropdownTemplate(row.ConditionDropdown, m_dropdownEffectCondition);
            EnsureAffixDropdownTemplate(row.EffectTargetDropdown, m_dropdownEffectTarget);
            SetupDropdown(row.ConditionDropdown, ConditionOptions);
            SetupDropdown(row.EffectTargetDropdown, AffixEffectTargetOptions);
            BindInput(row.NameInput, OnAffixRowChanged);
            BindDropdown(row.ConditionDropdown, OnAffixRowChanged);
            BindDropdown(row.EffectTargetDropdown, OnAffixRowChanged);
            BindInput(row.ValueInput, OnAffixRowChanged);
            BindInput(row.DescriptionInput, OnAffixRowChanged);
            BindInput(row.DiceExpressionInput, OnAffixRowChanged);
            BindButton(row.RemoveButton, () => RemoveAffixRow(row));
            return row;
        }

        private TMP_Dropdown FindOrCreateAffixDropdown(Transform root, string nodeName)
        {
            TMP_Dropdown dropdown = FindDescendantComponent<TMP_Dropdown>(root, nodeName);
            if (dropdown != null)
            {
                return dropdown;
            }

            TMP_InputField source = FindDescendantComponent<TMP_InputField>(root, nodeName);
            return CreateDropdownFromInput(source, nodeName);
        }

        private void EnsureAffixDropdownTemplate(TMP_Dropdown dropdown, TMP_Dropdown preferredTemplateSource)
        {
            if (dropdown == null)
            {
                return;
            }

            if (dropdown.template == null)
            {
                Log.Error($"ItemInfoEditorUI: {dropdown.name} is missing its own dropdown Template. Please add Template under this dropdown in prefab.");
                return;
            }

            EnsureDropdownTemplate(dropdown);
        }

        private void RemoveAffixRow(ItemAffixRowBinding row)
        {
            if (row == null)
            {
                return;
            }

            m_affixRows.Remove(row);
            if (row.Root != null)
            {
                UnityEngine.Object.Destroy(row.Root);
            }

            RefreshAffixRowRemoveButtons();
            RefreshPreview();
            RefreshEffectPreview();
        }

        private void RefreshAffixRowRemoveButtons()
        {
            for (int index = 0; index < m_affixRows.Count; index++)
            {
                Button button = m_affixRows[index]?.RemoveButton;
                if (button != null)
                {
                    button.interactable = true;
                }
            }
        }

        private void OnAffixRowChanged()
        {
            RefreshPreview();
            RefreshEffectPreview();
        }

        private void SyncCustomEffectsFromAffixRows()
        {
            m_customEffects.Clear();
            if (m_affixRows.Count == 0)
            {
                return;
            }

            for (int index = 0; index < m_affixRows.Count; index++)
            {
                ItemAffixRowBinding row = m_affixRows[index];
                if (row == null)
                {
                    continue;
                }

                CharacterItemEffectSaveData effect = new CharacterItemEffectSaveData
                {
                    Name = GetInputText(row.NameInput),
                    Target = GetSelectedDropdownValue(row.EffectTargetDropdown, AffixEffectTargetOptions),
                    Value = GetInputText(row.ValueInput),
                    Condition = GetSelectedDropdownValue(row.ConditionDropdown, ConditionOptions),
                    Description = GetInputText(row.DescriptionInput),
                    DiceExpression = GetInputText(row.DiceExpressionInput)
                };
                effect.EffectType = ResolveAffixEffectType(effect.Target);
                effect.Target = ResolveAffixEffectTarget(effect.Target);
                effect.EnableQuickRoll = !string.IsNullOrWhiteSpace(effect.DiceExpression);

                if (!IsEmptyAffixEffect(effect))
                {
                    m_customEffects.Add(effect);
                }
            }
        }

        private static bool IsEmptyAffixEffect(CharacterItemEffectSaveData effect)
        {
            if (effect == null)
            {
                return true;
            }

            bool hasValue = !string.IsNullOrWhiteSpace(effect.Value);
            bool hasName = !string.IsNullOrWhiteSpace(effect.Name);
            bool hasDescription = !string.IsNullOrWhiteSpace(effect.Description);
            bool hasDiceExpression = !string.IsNullOrWhiteSpace(effect.DiceExpression);
            if (hasName || hasValue || hasDescription || hasDiceExpression || effect.EnableQuickRoll)
            {
                return false;
            }

            return string.Equals(effect.EffectType, "ManualResolve", StringComparison.OrdinalIgnoreCase)
                && string.Equals(effect.Target, "Manual", StringComparison.OrdinalIgnoreCase)
                && (string.IsNullOrWhiteSpace(effect.Condition)
                    || string.Equals(effect.Condition, "Always", StringComparison.OrdinalIgnoreCase));
        }

        private static string ResolveAffixEffectType(string effectTarget)
        {
            string normalized = effectTarget?.Trim() ?? string.Empty;
            if (string.Equals(normalized, "AC", StringComparison.OrdinalIgnoreCase))
            {
                return "ACBonus";
            }

            if (string.Equals(normalized, "Attack", StringComparison.OrdinalIgnoreCase))
            {
                return "AttackBonus";
            }

            if (string.Equals(normalized, "Damage", StringComparison.OrdinalIgnoreCase))
            {
                return "DamageBonus";
            }

            if (string.Equals(normalized, "Speed", StringComparison.OrdinalIgnoreCase))
            {
                return "SpeedBonus";
            }

            if (string.Equals(normalized, "Initiative", StringComparison.OrdinalIgnoreCase))
            {
                return "InitiativeBonus";
            }

            if (string.Equals(normalized, "SpellAttack", StringComparison.OrdinalIgnoreCase))
            {
                return "SpellAttackBonus";
            }

            if (string.Equals(normalized, "SpellSaveDC", StringComparison.OrdinalIgnoreCase))
            {
                return "SpellSaveDcBonus";
            }

            if (IsAbilityAffixTarget(normalized))
            {
                return "AbilityScoreBonus";
            }

            return "ManualResolve";
        }

        private static string ResolveAffixEffectTarget(string effectTarget)
        {
            string normalized = effectTarget?.Trim() ?? string.Empty;
            if (string.Equals(normalized, "Manual", StringComparison.OrdinalIgnoreCase))
            {
                return "Manual";
            }

            if (string.Equals(normalized, "Speed", StringComparison.OrdinalIgnoreCase))
            {
                return "walk";
            }

            if (IsAbilityAffixTarget(normalized))
            {
                return normalized.ToLowerInvariant();
            }

            return normalized;
        }

        private static bool IsAbilityAffixTarget(string effectTarget)
        {
            return string.Equals(effectTarget, "Strength", StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectTarget, "Dexterity", StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectTarget, "Constitution", StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectTarget, "Intelligence", StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectTarget, "Wisdom", StringComparison.OrdinalIgnoreCase)
                || string.Equals(effectTarget, "Charisma", StringComparison.OrdinalIgnoreCase);
        }

        private static T FindDescendantComponent<T>(Transform root, string name) where T : Component
        {
            if (root == null || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            if (string.Equals(root.name, name, StringComparison.OrdinalIgnoreCase))
            {
                return root.GetComponent<T>();
            }

            for (int index = 0; index < root.childCount; index++)
            {
                T result = FindDescendantComponent<T>(root.GetChild(index), name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private CharacterEquipmentItemSaveData BuildItemData()
        {
            string itemTypeId = GetSelectedDropdownValue(m_dropdownItemType, m_itemTypeOptions);
            DndItemTypeDefineData itemType = FindItemType(itemTypeId);
            bool isEquippable = itemType?.IsEquipmentType ?? false;
            string selectedEquipmentSlot = GetSelectedDropdownValue(m_dropdownEquipmentSlot, EquipmentSlotOptions);
            string equipmentSlot = isEquippable
                ? FirstNonEmpty(selectedEquipmentSlot, itemType?.DefaultEquipmentSlot)
                : string.Empty;
            int maxCharges = itemType != null && itemType.CanHaveCharges
                ? ParseInt(m_inputMaxCharges, 0)
                : 0;
            CharacterEquipmentItemSaveData item = new CharacterEquipmentItemSaveData
            {
                ItemName = GetInputText(m_inputItemName),
                ItemType = itemTypeId,
                Rarity = GetSelectedDropdownValue(m_dropdownRarity, RarityOptions),
                Description = GetInputText(m_inputDescription),
                Notes = GetInputText(m_inputNotes),
                ItemSourceType = GetSelectedDropdownValue(m_dropdownItemSourceType, SourceTypeOptions),
                SourceItemId = GetCustomItemIdValue(),
                ArmorCategory = GetSelectedDropdownValue(m_dropdownArmorCategory, ArmorCategoryOptions),
                ArmorBaseAc = ParseInt(m_inputArmorBaseAc, 0),
                AcBonus = ParseInt(m_inputAcBonus, 0),
                MaxDexBonus = ParseInt(m_inputMaxDexBonus, 0),
                StrengthRequirement = ParseInt(m_inputStrengthRequirement, 0),
                StealthDisadvantage = ParseBoolDropdown(m_dropdownStealthDisadvantage),
                WeaponCategory = GetSelectedDropdownValue(m_dropdownWeaponCategory, WeaponCategoryOptions),
                WeaponRangeType = GetSelectedDropdownValue(m_dropdownWeaponRangeType, WeaponRangeTypeOptions),
                DamageDice = GetInputText(m_inputDamageDice),
                DamageType = GetSelectedDropdownValue(m_dropdownDamageType, DamageTypeOptions),
                WeaponProperties = GetInputText(m_inputWeaponProperties),
                NormalRange = ParseInt(m_inputNormalRange, 0),
                LongRange = ParseInt(m_inputLongRange, 0),
                TwoHandDamageDice = GetInputText(m_inputTwoHandDamageDice),
                ToolCategory = GetSelectedDropdownValue(m_dropdownToolCategory, ToolCategoryOptions),
                Consumable = itemType != null
                    ? itemType.ConsumeQuantityOnUseByDefault
                    : ParseBoolDropdown(m_dropdownConsumable),
                Charges = 0,
                MaxCharges = Math.Max(0, maxCharges),
                ConsumeOnUse = itemType != null
                    ? itemType.ConsumeQuantityOnUseByDefault
                    : ParseBoolDropdown(m_dropdownConsumeOnUse),
                Weight = GetInputText(m_inputWeight),
                PriceGp = ParseInt(m_inputPriceGp, 0),
                Quantity = 1,
                IsEquippable = isEquippable,
                EquipmentSlot = equipmentSlot,
                RequiresAttunement = ParseBoolDropdown(m_dropdownRequiresAttunement),
                EffectApplyCondition = GetInputText(m_inputEffectApplyCondition)
            };
            CharacterItemTypeBehaviorUtility.ApplyTypeDefaults(item);

            item.EffectIds.Clear();
            AppendEffectIds(item.EffectIds, m_ruleEffectIds);
            item.CustomEffects.Clear();
            SyncCustomEffectsFromAffixRows();
            for (int index = 0; index < m_customEffects.Count; index++)
            {
                CharacterItemEffectSaveData effect = m_customEffects[index];
                if (CharacterItemEffectSaveData.HasContent(effect))
                {
                    item.CustomEffects.Add(CharacterItemEffectSaveData.Clone(effect));
                }
            }

            return CharacterItemSnapshotBuilder.BuildTemplateFromCustomItem(item, GetCustomItemIdValue());
        }

        private string EnsureCustomItemId()
        {
            string customItemId = GetCustomItemIdValue();
            if (string.IsNullOrWhiteSpace(customItemId))
            {
                customItemId = $"custom_item_{DateTime.UtcNow:yyyyMMddHHmmssfff}";
            }

            m_currentCustomItemId = customItemId.Trim();
            SetInputText(m_inputSourceItemId, m_currentCustomItemId);
            return m_currentCustomItemId;
        }

        private string GetCustomItemIdValue()
        {
            string inputValue = GetInputText(m_inputSourceItemId);
            if (!string.IsNullOrWhiteSpace(inputValue))
            {
                m_currentCustomItemId = inputValue.Trim();
            }

            return m_currentCustomItemId;
        }

        private void RefreshPreview()
        {
            CharacterEquipmentItemSaveData item = BuildItemData();
            SetText(m_tmpPreviewItemName, FirstNonEmpty(item.ItemName, "未命名物品"));
            SetText(m_tmpPreviewItemType, $"类型：{GetOptionLabel(m_itemTypeOptions, item.ItemType)}");
            SetText(m_tmpPreviewItemNum, "数量：模板不记录数量");
            SetText(m_tmpPreviewItemRare, $"稀有度：{FirstNonEmpty(GetOptionLabel(RarityOptions, item.Rarity), "无")}");
            SetText(m_tmpPreviewSource, $"来源：{GetOptionLabel(SourceTypeOptions, item.ItemSourceType)}");
            SetText(m_tmpPreviewAc, BuildItemFormPreviewDetails(item));
        }

        private string BuildItemFormPreviewDetails(CharacterEquipmentItemSaveData item)
        {
            if (item == null)
            {
                return "-";
            }

            StringBuilder builder = new StringBuilder();
            AppendPreviewLine(builder, "来源ID", item.SourceItemId);
            AppendPreviewLine(builder, "重量", item.Weight);
            AppendPreviewLine(builder, "价格", item.PriceGp > 0 ? $"{item.PriceGp} gp" : string.Empty);
            AppendPreviewLine(builder, "装备栏位", item.EquipmentSlot);
            AppendPreviewLine(builder, "需要同调", item.RequiresAttunement ? "是" : string.Empty);
            AppendPreviewLine(builder, "描述", item.Description);
            AppendPreviewLine(builder, "备注", item.Notes);

            DndItemTypeDefineData itemType = FindItemType(item.ItemType);
            bool isArmor = IsItemTypeOrParent(itemType, "armor") || string.Equals(item.ItemType, "shield", StringComparison.OrdinalIgnoreCase);
            bool isWeapon = IsItemTypeOrParent(itemType, "weapon");
            bool isTool = IsItemTypeOrParent(itemType, "tool");

            if (isWeapon)
            {
                AppendPreviewLine(builder, "武器类别", GetOptionLabel(WeaponCategoryOptions, item.WeaponCategory));
                AppendPreviewLine(builder, "攻击距离", GetOptionLabel(WeaponRangeTypeOptions, item.WeaponRangeType));
                AppendPreviewLine(builder, "伤害骰", item.DamageDice);
                AppendPreviewLine(builder, "伤害类型", GetOptionLabel(DamageTypeOptions, item.DamageType));
                AppendPreviewLine(builder, "武器属性", item.WeaponProperties);
                AppendPreviewLine(builder, "普通射程", item.NormalRange > 0 ? item.NormalRange.ToString() : string.Empty);
                AppendPreviewLine(builder, "长射程", item.LongRange > 0 ? item.LongRange.ToString() : string.Empty);
                AppendPreviewLine(builder, "双手伤害", item.TwoHandDamageDice);
            }
            else if (isTool)
            {
                AppendPreviewLine(builder, "工具类别", GetOptionLabel(ToolCategoryOptions, item.ToolCategory));
            }
            else if (itemType != null
                && (itemType.CanUseByDefault
                    || itemType.StackableByDefault
                    || itemType.ConsumeQuantityOnUseByDefault
                    || itemType.CanHaveCharges
                    || itemType.ConsumeChargeOnUseByDefault))
            {
                AppendPreviewLine(builder, "是否可使用", itemType.CanUseByDefault ? "是" : "否");
                AppendPreviewLine(builder, "是否可堆叠", itemType.StackableByDefault ? "是" : "否");
                AppendPreviewLine(builder, "使用后减数量", itemType.ConsumeQuantityOnUseByDefault ? "是" : "否");
                AppendPreviewLine(builder, "使用后减充能", itemType.ConsumeChargeOnUseByDefault ? "是" : "否");
                AppendPreviewLine(builder, "最大充能", item.MaxCharges > 0 ? item.MaxCharges.ToString() : string.Empty);
            }
            else if (isArmor)
            {
                AppendPreviewLine(builder, "护甲类别", GetOptionLabel(ArmorCategoryOptions, item.ArmorCategory));
                AppendPreviewLine(builder, "基础AC", item.ArmorBaseAc > 0 ? item.ArmorBaseAc.ToString() : string.Empty);
                AppendPreviewLine(builder, "AC加值", item.AcBonus != 0 ? FormatSignedNumber(item.AcBonus) : string.Empty);
                AppendPreviewLine(builder, "敏捷上限", item.MaxDexBonus > 0 ? item.MaxDexBonus.ToString() : string.Empty);
                AppendPreviewLine(builder, "力量需求", item.StrengthRequirement > 0 ? item.StrengthRequirement.ToString() : string.Empty);
                AppendPreviewLine(builder, "隐匿劣势", item.StealthDisadvantage ? "是" : "否");
            }

            AppendPreviewLine(builder, "生效条件", item.EffectApplyCondition);
            return builder.Length > 0 ? builder.ToString() : "-";
        }

        private static void AppendPreviewLine(StringBuilder builder, string label, string value)
        {
            if (builder == null || string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.Append(label);
            builder.Append("：");
            builder.Append(value.Trim());
        }

        private void RefreshEffectPreview()
        {
            SyncCustomEffectsFromAffixRows();
            if (m_tmpPreviewEffects == null)
            {
                RefreshItemFormScrollArea(false);
                return;
            }

            if (m_ruleEffectIds.Count == 0 && m_customEffects.Count == 0)
            {
                m_tmpPreviewEffects.text = string.Empty;
                RefreshItemFormScrollArea(false);
                return;
            }

            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < m_ruleEffectIds.Count; index++)
            {
                string effectId = m_ruleEffectIds[index];
                if (string.IsNullOrWhiteSpace(effectId))
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                if (DndRuleContentService.Instance.TryGetItemEffect(effectId.Trim(), out DndItemEffectData itemEffect))
                {
                    builder.Append(BuildEffectNameDescriptionText(effectId, itemEffect.Description));
                }
                else
                {
                    builder.Append(effectId.Trim());
                }
            }

            for (int index = 0; index < m_customEffects.Count; index++)
            {
                CharacterItemEffectSaveData effect = m_customEffects[index];
                if (effect == null)
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                builder.Append(BuildEffectPreviewText(effect));
            }

            m_tmpPreviewEffects.text = builder.ToString();
            RefreshItemFormScrollArea(false);
        }

        private static string BuildEffectNameDescriptionText(string effectName, string effectDescription)
        {
            string name = effectName?.Trim() ?? string.Empty;
            string description = effectDescription?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(description))
            {
                return $"{name} - {description}";
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            return description;
        }

        private static string BuildEffectPreviewText(CharacterItemEffectSaveData effect)
        {
            if (effect == null)
            {
                return string.Empty;
            }

            string text = BuildEffectNameDescriptionText(effect.Name, effect.Description);
            string diceExpression = effect.DiceExpression?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(diceExpression))
            {
                return text;
            }

            string diceText = $"快捷掷骰：{diceExpression}";
            if (string.IsNullOrWhiteSpace(text))
            {
                return diceText;
            }

            return $"{text}\n{diceText}";
        }

        private static void AppendEffectIds(List<string> target, IReadOnlyList<string> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                string effectId = source[index]?.Trim();
                if (string.IsNullOrWhiteSpace(effectId))
                {
                    continue;
                }

                bool exists = false;
                for (int existingIndex = 0; existingIndex < target.Count; existingIndex++)
                {
                    if (string.Equals(target[existingIndex], effectId, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    target.Add(effectId);
                }
            }
        }

        private static string FormatStringList(IReadOnlyList<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < values.Count; index++)
            {
                string value = values[index]?.Trim();
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append(';');
                }

                builder.Append(value);
            }

            return builder.ToString();
        }

        private string GetEffectTargetLabel(string effectType, string target)
        {
            List<DropdownOption> options = BuildEffectTargetOptions(effectType);
            string label = GetOptionLabel(options, target);
            return string.IsNullOrWhiteSpace(label) ? target?.Trim() ?? string.Empty : label;
        }

        private void RefreshItemFormScrollArea(bool resetPosition)
        {
            if (m_rectItemFormContent == null)
            {
                return;
            }

            float contentHeight = CalculateContentHeight(m_rectItemFormContent);
            RectTransform viewport = m_scrollItemForm != null ? m_scrollItemForm.viewport : null;
            if (viewport != null)
            {
                contentHeight = Mathf.Max(contentHeight, viewport.rect.height);
            }

            m_rectItemFormContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_rectItemFormContent);

            if (m_scrollItemForm != null && resetPosition)
            {
                m_scrollItemForm.StopMovement();
                m_scrollItemForm.verticalNormalizedPosition = 1f;
            }
        }

        private static float CalculateContentHeight(RectTransform content)
        {
            float bottom = 0f;
            Vector3[] corners = new Vector3[4];
            for (int index = 0; index < content.childCount; index++)
            {
                RectTransform child = content.GetChild(index) as RectTransform;
                if (child == null || !child.gameObject.activeSelf)
                {
                    continue;
                }

                child.GetWorldCorners(corners);
                for (int cornerIndex = 0; cornerIndex < corners.Length; cornerIndex++)
                {
                    Vector3 localCorner = content.InverseTransformPoint(corners[cornerIndex]);
                    bottom = Mathf.Min(bottom, localCorner.y);
                }
            }

            return Mathf.Abs(bottom) + 36f;
        }

        private void ClearEffectInput()
        {
            SetInputText(m_inputEffectTarget, string.Empty);
            SetInputText(m_inputEffectValue, string.Empty);
            SetInputText(m_inputEffectCondition, string.Empty);
            SetInputText(m_inputEffectConditionDescription, string.Empty);
            SetInputText(m_inputEffectDescription, string.Empty);
            SelectDropdownValue(m_dropdownEffectCondition, ConditionOptions, "Always");
            RefreshEffectTargetOptions();
        }

        private void OnItemTypeChanged()
        {
            RefreshCategoryFieldVisibility();
            RefreshPreview();
        }

        private void RefreshCategoryFieldVisibility()
        {
            if (m_dropdownArmorCategory == null
                && m_dropdownWeaponCategory == null
                && m_dropdownToolCategory == null
                && m_dropdownConsumable == null
                && m_dropdownEquipmentSlot == null
                && m_inputMaxCharges == null)
            {
                RefreshItemFormScrollArea(false);
                return;
            }

            string itemTypeId = GetSelectedDropdownValue(m_dropdownItemType, m_itemTypeOptions);
            DndItemTypeDefineData itemType = FindItemType(itemTypeId);
            bool isArmor = IsItemTypeOrParent(itemType, "armor")
                || string.Equals(itemTypeId, "shield", StringComparison.OrdinalIgnoreCase);
            bool isWeapon = IsItemTypeOrParent(itemType, "weapon");
            bool isTool = IsItemTypeOrParent(itemType, "tool");
            bool canHaveCharges = itemType?.CanHaveCharges ?? false;
            bool isEquippable = itemType?.IsEquipmentType ?? false;

            SetControlWithLabelActive(m_dropdownArmorCategory, isArmor);
            SetControlWithLabelActive(m_inputArmorBaseAc, isArmor);
            SetControlWithLabelActive(m_inputAcBonus, isArmor);
            SetControlWithLabelActive(m_inputMaxDexBonus, isArmor);
            SetControlWithLabelActive(m_inputStrengthRequirement, isArmor);
            SetControlWithLabelActive(m_dropdownStealthDisadvantage, isArmor);

            SetControlWithLabelActive(m_dropdownWeaponCategory, isWeapon);
            SetControlWithLabelActive(m_dropdownWeaponRangeType, isWeapon);
            SetControlWithLabelActive(m_inputDamageDice, isWeapon);
            SetControlWithLabelActive(m_dropdownDamageType, isWeapon);
            SetControlWithLabelActive(m_inputWeaponProperties, isWeapon);
            SetControlWithLabelActive(m_inputNormalRange, isWeapon);
            SetControlWithLabelActive(m_inputLongRange, isWeapon);
            SetControlWithLabelActive(m_inputTwoHandDamageDice, isWeapon);

            SetControlWithLabelActive(m_dropdownToolCategory, isTool);

            SetControlWithLabelActive(m_dropdownEquipmentSlot, isEquippable);
            SetControlWithLabelActive(m_inputMaxCharges, canHaveCharges);
            SetControlWithLabelActive(m_inputPriceGp, true);
            SetControlWithLabelActive(m_dropdownRequiresAttunement, true);
            SetControlWithLabelActive(m_dropdownConsumable, false);
            SetControlWithLabelActive(m_inputCharges, false);
            SetControlWithLabelActive(m_dropdownConsumeOnUse, false);
            RefreshItemFormScrollArea(false);
        }

        private void SetControlWithLabelActive(Component control, bool active)
        {
            if (control == null)
            {
                return;
            }

            SetActive(control.gameObject, active);
            SetActive(FindSiblingLabel(control.transform), active);
        }

        private GameObject FindSiblingLabel(Transform control)
        {
            if (control == null || control.parent == null)
            {
                return null;
            }

            string labelName = $"m_tmp{control.name}Label";
            Transform label = control.parent.Find(labelName);
            if (label == null && string.Equals(control.name, "m_inputWeight", StringComparison.OrdinalIgnoreCase))
            {
                label = control.parent.Find("m_tmpWeightLabel");
            }

            return label != null ? label.gameObject : null;
        }

        private void OnEffectTypeChanged()
        {
            RefreshEffectTargetOptions();
            RefreshEffectValueInputVisibility();
            RefreshPreview();
        }

        private void OnEffectTargetChanged()
        {
            SetInputText(m_inputEffectTarget, ResolveEffectTargetValue());
            RefreshPreview();
        }

        private void OnEffectConditionChanged()
        {
            SetInputText(m_inputEffectCondition, ResolveEffectConditionValue());
            RefreshPreview();
        }

        private void RefreshEffectTargetOptions()
        {
            string effectType = GetSelectedDropdownValue(m_dropdownEffectType, EffectTypeOptions);
            m_currentEffectTargetOptions = BuildEffectTargetOptions(effectType);
            SetupDropdown(m_dropdownEffectTarget, m_currentEffectTargetOptions);
            RefreshEffectTargetInputVisibility();
            SetInputText(m_inputEffectTarget, ResolveEffectTargetValue());
            SetInputText(m_inputEffectCondition, ResolveEffectConditionValue());
        }

        private void RefreshEffectTargetInputVisibility()
        {
            bool hasStructuredOptions = m_currentEffectTargetOptions != null && m_currentEffectTargetOptions.Count > 0;
            SetActive(m_dropdownEffectTarget?.gameObject, hasStructuredOptions);
            SetActive(m_inputEffectTarget?.gameObject, !hasStructuredOptions);
            RefreshItemFormScrollArea(false);
        }

        private void RefreshEffectValueInputVisibility()
        {
            string effectType = GetSelectedDropdownValue(m_dropdownEffectType, EffectTypeOptions);
            bool visible = RequiresEffectValue(effectType);
            SetActive(m_inputEffectValue?.gameObject, visible);
            if (!visible)
            {
                SetInputText(m_inputEffectValue, string.Empty);
            }
        }

        private static bool RequiresEffectValue(string effectType)
        {
            string normalized = effectType?.Trim() ?? string.Empty;
            return string.Equals(normalized, "AbilityScoreBonus", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "AbilityScoreSet", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "ACBonus", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "SpeedBonus", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "InitiativeBonus", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "SkillBonus", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "SavingThrowBonus", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "SpellAttackBonus", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "SpellSaveDcBonus", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "AttackBonus", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "WeaponAttackBonus", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "DamageBonus", StringComparison.OrdinalIgnoreCase);
        }

        private string ResolveEffectTargetValue()
        {
            string selected = GetSelectedDropdownValue(m_dropdownEffectTarget, m_currentEffectTargetOptions);
            return string.IsNullOrWhiteSpace(selected) ? GetInputText(m_inputEffectTarget) : selected;
        }

        private string ResolveEffectConditionValue()
        {
            string selected = GetSelectedDropdownValue(m_dropdownEffectCondition, ConditionOptions);
            return string.IsNullOrWhiteSpace(selected) ? GetInputText(m_inputEffectCondition) : selected;
        }

        private List<DropdownOption> BuildEffectTargetOptions(string effectType)
        {
            string normalized = effectType?.Trim() ?? string.Empty;
            if (string.Equals(normalized, "AbilityScoreBonus", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "AbilityScoreSet", StringComparison.OrdinalIgnoreCase))
            {
                return ToList(AbilityTargetOptions);
            }

            if (string.Equals(normalized, "SavingThrowBonus", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "SavingThrowProficiency", StringComparison.OrdinalIgnoreCase))
            {
                return ToList(SavingThrowTargetOptions);
            }

            if (string.Equals(normalized, "ACBonus", StringComparison.OrdinalIgnoreCase))
            {
                return ToList(AcTargetOptions);
            }

            if (string.Equals(normalized, "SpeedBonus", StringComparison.OrdinalIgnoreCase))
            {
                return ToList(SpeedTargetOptions);
            }

            if (string.Equals(normalized, "InitiativeBonus", StringComparison.OrdinalIgnoreCase))
            {
                return ToList(InitiativeTargetOptions);
            }

            if (string.Equals(normalized, "SpellAttackBonus", StringComparison.OrdinalIgnoreCase))
            {
                return ToList(SpellAttackTargetOptions);
            }

            if (string.Equals(normalized, "SpellSaveDcBonus", StringComparison.OrdinalIgnoreCase))
            {
                return ToList(SpellSaveDcTargetOptions);
            }

            if (string.Equals(normalized, "AttackBonus", StringComparison.OrdinalIgnoreCase))
            {
                return ToList(AttackTargetOptions);
            }

            if (string.Equals(normalized, "WeaponAttackBonus", StringComparison.OrdinalIgnoreCase))
            {
                return ToList(WeaponAttackTargetOptions);
            }

            if (string.Equals(normalized, "DamageBonus", StringComparison.OrdinalIgnoreCase))
            {
                return ToList(DamageTargetOptions);
            }

            if (string.Equals(normalized, "SkillBonus", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "SkillProficiency", StringComparison.OrdinalIgnoreCase))
            {
                return BuildSkillTargetOptions();
            }

            if (string.Equals(normalized, "ToolProficiency", StringComparison.OrdinalIgnoreCase))
            {
                return BuildToolTargetOptions();
            }

            if (string.Equals(normalized, "WeaponProficiency", StringComparison.OrdinalIgnoreCase))
            {
                return ToList(WeaponProficiencyTargetOptions);
            }

            if (string.Equals(normalized, "ArmorProficiency", StringComparison.OrdinalIgnoreCase))
            {
                return ToList(ArmorProficiencyTargetOptions);
            }

            if (string.Equals(normalized, "Resistance", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "Immunity", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "Vulnerability", StringComparison.OrdinalIgnoreCase))
            {
                return ToList(DamageTypeTargetOptions);
            }

            if (string.Equals(normalized, "ManualResolve", StringComparison.OrdinalIgnoreCase))
            {
                return ToList(ManualTargetOptions);
            }

            return new List<DropdownOption>();
        }

        private static List<DropdownOption> ToList(IReadOnlyList<DropdownOption> options)
        {
            List<DropdownOption> result = new List<DropdownOption>();
            if (options == null)
            {
                return result;
            }

            for (int index = 0; index < options.Count; index++)
            {
                result.Add(options[index]);
            }

            return result;
        }

        private static List<DropdownOption> BuildSkillTargetOptions()
        {
            List<DropdownOption> result = new List<DropdownOption>();
            IReadOnlyList<DndSkillDefineData> skills = DndRuleContentService.Instance.Skills;
            for (int index = 0; index < skills.Count; index++)
            {
                DndSkillDefineData skill = skills[index];
                if (skill != null && !string.IsNullOrWhiteSpace(skill.SkillId))
                {
                    result.Add(new DropdownOption(skill.SkillId.Trim(), FirstNonEmpty(skill.Name, skill.SkillId)));
                }
            }

            if (result.Count == 0)
            {
                Log.Warning("ItemInfoEditorUI: skill target options are empty. Check TbSkillDefine loading.");
            }

            return result;
        }

        private static List<DropdownOption> BuildToolTargetOptions()
        {
            List<DropdownOption> result = new List<DropdownOption>();
            IReadOnlyList<DndToolDefineData> tools = DndRuleContentService.Instance.Tools;
            for (int index = 0; index < tools.Count; index++)
            {
                DndToolDefineData tool = tools[index];
                if (tool != null && !string.IsNullOrWhiteSpace(tool.ToolId))
                {
                    result.Add(new DropdownOption(tool.ToolId.Trim(), FirstNonEmpty(tool.Name, tool.ToolId)));
                }
            }

            if (result.Count == 0)
            {
                Log.Warning("ItemInfoEditorUI: tool target options are empty. Check TbDndToolDefine loading.");
            }

            return result;
        }

        private TMP_Dropdown CreateDropdownFromInput(TMP_InputField source, string name)
        {
            Log.Warning($"ItemInfoEditorUI: {name} is missing. Please add and bind this dropdown in prefab.");
            return null;
        }

        private static void RebindClonedDropdownReferences(TMP_Dropdown dropdown, TMP_InputField source)
        {
            Log.Warning("ItemInfoEditorUI: runtime dropdown cloning is disabled. Please bind dropdown nodes in prefab.");
        }

        private static void EnsureDropdownTemplate(TMP_Dropdown dropdown)
        {
            if (dropdown == null)
            {
                return;
            }

            ApplyDropdownUiLayer(dropdown);

            Image background = dropdown.GetComponent<Image>();
            if (background != null)
            {
                dropdown.targetGraphic = background;
            }

            RectTransform template = EnsureOwnedDropdownTemplate(dropdown);
            if (template != null)
            {
                dropdown.template = template;
            }

            if (dropdown.captionText == null
                || !dropdown.captionText.transform.IsChildOf(dropdown.transform)
                || (dropdown.template != null && dropdown.captionText.transform.IsChildOf(dropdown.template)))
            {
                TMP_Text label = FindDescendantComponent<TMP_Text>(dropdown.transform, "Label")
                    ?? FindDescendantComponent<TMP_Text>(dropdown.transform, "m_tmpPlaceholder")
                    ?? FindDescendantComponent<TMP_Text>(dropdown.transform, "m_tmpInputText");
                if (label != null && (dropdown.template == null || !label.transform.IsChildOf(dropdown.template)))
                {
                    dropdown.captionText = label;
                    label.enableWordWrapping = false;
                    label.overflowMode = TextOverflowModes.Ellipsis;
                }
            }

            TMP_Text itemLabel = dropdown.template != null
                ? FindDescendantComponent<TMP_Text>(dropdown.template, "Item Label")
                : null;
            if (itemLabel != null)
            {
                dropdown.itemText = itemLabel;
            }

            if (dropdown.template != null)
            {
                SetActive(dropdown.template.gameObject, false);
            }
        }

        private void RefreshDropdownRuntimeLayers()
        {
            RefreshDropdownRuntimeLayer(m_dropdownItemType);
            RefreshDropdownRuntimeLayer(m_dropdownRarity);
            RefreshDropdownRuntimeLayer(m_dropdownItemSourceType);
            RefreshDropdownRuntimeLayer(m_dropdownArmorCategory);
            RefreshDropdownRuntimeLayer(m_dropdownStealthDisadvantage);
            RefreshDropdownRuntimeLayer(m_dropdownWeaponCategory);
            RefreshDropdownRuntimeLayer(m_dropdownWeaponRangeType);
            RefreshDropdownRuntimeLayer(m_dropdownDamageType);
            RefreshDropdownRuntimeLayer(m_dropdownToolCategory);
            RefreshDropdownRuntimeLayer(m_dropdownConsumable);
            RefreshDropdownRuntimeLayer(m_dropdownConsumeOnUse);
            RefreshDropdownRuntimeLayer(m_dropdownEquipmentSlot);
            RefreshDropdownRuntimeLayer(m_dropdownRequiresAttunement);
            RefreshDropdownRuntimeLayer(m_dropdownEffectType);
            RefreshDropdownRuntimeLayer(m_dropdownEffectTarget);
            RefreshDropdownRuntimeLayer(m_dropdownEffectCondition);

            for (int index = 0; index < m_affixRows.Count; index++)
            {
                ItemAffixRowBinding row = m_affixRows[index];
                RefreshDropdownRuntimeLayer(row?.ConditionDropdown);
                RefreshDropdownRuntimeLayer(row?.EffectTargetDropdown);
            }
        }

        private static void RefreshDropdownRuntimeLayer(TMP_Dropdown dropdown)
        {
            if (dropdown == null)
            {
                return;
            }

            ApplyDropdownUiLayer(dropdown);

            int layer = ResolveDropdownLayer(dropdown);
            Transform dropdownList = dropdown.transform.Find("Dropdown List");
            if (dropdownList != null)
            {
                SetLayerRecursively(dropdownList.gameObject, layer);
            }

            Canvas canvas = FindRootCanvas(dropdown.transform);
            Transform blocker = canvas != null ? canvas.transform.Find("Blocker") : null;
            if (blocker != null)
            {
                SetLayerRecursively(blocker.gameObject, layer);
            }
        }

        private static RectTransform EnsureOwnedDropdownTemplate(TMP_Dropdown dropdown)
        {
            if (dropdown == null)
            {
                return null;
            }

            RectTransform currentTemplate = dropdown.template
                ?? FindDescendantByName(dropdown.transform, "Template") as RectTransform;
            if (currentTemplate == null)
            {
                return null;
            }

            ApplyDropdownUiLayer(dropdown);
            SetActive(currentTemplate.gameObject, false);
            return currentTemplate;
        }

        private static void ApplyDropdownUiLayer(TMP_Dropdown dropdown)
        {
            if (dropdown == null)
            {
                return;
            }

            int layer = ResolveDropdownLayer(dropdown);
            SetLayerRecursively(dropdown.gameObject, layer);
            if (dropdown.template != null)
            {
                SetLayerRecursively(dropdown.template.gameObject, layer);
            }
        }

        private static int ResolveDropdownLayer(TMP_Dropdown dropdown)
        {
            Canvas canvas = dropdown != null ? dropdown.GetComponentInParent<Canvas>() : null;
            if (canvas != null)
            {
                return canvas.gameObject.layer;
            }

            int uiLayer = LayerMask.NameToLayer("UI");
            return uiLayer >= 0 ? uiLayer : 5;
        }

        private static Canvas FindRootCanvas(Transform transform)
        {
            if (transform == null)
            {
                return null;
            }

            Canvas result = null;
            Transform current = transform;
            while (current != null)
            {
                Canvas canvas = current.GetComponent<Canvas>();
                if (canvas != null)
                {
                    result = canvas;
                }

                current = current.parent;
            }

            return result;
        }

        private static void SetLayerRecursively(GameObject target, int layer)
        {
            if (target == null || layer < 0)
            {
                return;
            }

            target.layer = layer;
            Transform transform = target.transform;
            for (int index = 0; index < transform.childCount; index++)
            {
                SetLayerRecursively(transform.GetChild(index).gameObject, layer);
            }
        }

        private static void SyncDropdownCaptionRect(TMP_Text label, TMP_InputField source)
        {
            Log.Warning("ItemInfoEditorUI: runtime dropdown caption rect syncing is disabled. Please configure caption layout in prefab.");
        }

        private TMP_InputField CreateEffectConditionDescriptionInput()
        {
            Log.Warning("ItemInfoEditorUI: m_inputEffectConditionDescription is missing. Please add and bind this input in prefab.");
            return null;
        }

        private static void SetupDropdown(TMP_Dropdown dropdown, IReadOnlyList<DropdownOption> options)
        {
            if (dropdown == null)
            {
                return;
            }

            dropdown.ClearOptions();
            List<TMP_Dropdown.OptionData> data = new List<TMP_Dropdown.OptionData>();
            for (int index = 0; index < options.Count; index++)
            {
                DropdownOption option = options[index];
                data.Add(new TMP_Dropdown.OptionData(option.Label));
            }

            dropdown.AddOptions(data);
            if (dropdown.options.Count > 0)
            {
                dropdown.SetValueWithoutNotify(0);
            }

            dropdown.RefreshShownValue();
            RefreshDropdownCaptionState(dropdown);
        }

        private static List<DropdownOption> BuildItemTypeOptions()
        {
            List<DropdownOption> result = new List<DropdownOption>();
            IReadOnlyList<DndItemTypeDefineData> itemTypes = DndRuleContentService.Instance.GetSelectableItemTypes();
            for (int index = 0; index < itemTypes.Count; index++)
            {
                DndItemTypeDefineData itemType = itemTypes[index];
                string itemTypeId = itemType?.ItemTypeId?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(itemTypeId))
                {
                    continue;
                }

                result.Add(new DropdownOption(itemTypeId, FirstNonEmpty(itemType.Name, itemTypeId)));
            }

            if (result.Count > 0)
            {
                return result;
            }

            Log.Warning("ItemInfoEditorUI: item type options are empty. Check TbDndItemTypeDefine selectable data.");
            return result;
        }

        private static DndItemTypeDefineData FindItemType(string itemTypeId)
        {
            string normalized = itemTypeId?.Trim() ?? string.Empty;
            return DndRuleContentService.Instance.TryGetItemType(normalized, out DndItemTypeDefineData itemType)
                ? itemType
                : null;
        }

        private static bool IsItemTypeOrParent(DndItemTypeDefineData itemType, string expectedTypeId)
        {
            string expected = expectedTypeId?.Trim() ?? string.Empty;
            if (itemType == null || string.IsNullOrWhiteSpace(expected))
            {
                return false;
            }

            DndItemTypeDefineData current = itemType;
            HashSet<string> visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int depth = 0; current != null && depth < 16; depth++)
            {
                string currentId = current.ItemTypeId?.Trim() ?? string.Empty;
                if (string.Equals(currentId, expected, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (string.IsNullOrWhiteSpace(current.ParentTypeId) || !visited.Add(currentId))
                {
                    return false;
                }

                current = FindItemType(current.ParentTypeId);
            }

            return false;
        }

        private static string ResolveSelectableItemType(ItemEditorRuleItemViewState ruleItem)
        {
            return CharacterItemTypeBehaviorUtility.ResolveRuleItemTypeId(ruleItem);
        }

        private static string NormalizeOptionOrFirst(IReadOnlyList<DropdownOption> options, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                string trimmed = value.Trim();
                for (int index = 0; index < options.Count; index++)
                {
                    if (string.Equals(options[index].Value, trimmed, StringComparison.OrdinalIgnoreCase))
                    {
                        return options[index].Value;
                    }
                }
            }

            return options.Count > 0 ? options[0].Value : string.Empty;
        }

        private static string GetSelectedDropdownValue(TMP_Dropdown dropdown, IReadOnlyList<DropdownOption> options)
        {
            if (dropdown == null || dropdown.options.Count == 0)
            {
                return string.Empty;
            }

            int index = Mathf.Clamp(dropdown.value, 0, dropdown.options.Count - 1);
            return index < options.Count ? options[index].Value : string.Empty;
        }

        private static bool ParseBoolDropdown(TMP_Dropdown dropdown)
        {
            return string.Equals(GetSelectedDropdownValue(dropdown, BoolOptions), "true", StringComparison.OrdinalIgnoreCase);
        }

        private static string ToBoolOptionValue(bool value)
        {
            return value ? "true" : "false";
        }

        private static void SelectDropdownValue(TMP_Dropdown dropdown, IReadOnlyList<DropdownOption> options, string value)
        {
            if (dropdown == null || dropdown.options.Count == 0)
            {
                return;
            }

            for (int index = 0; index < dropdown.options.Count; index++)
            {
                if (index < options.Count && string.Equals(options[index].Value, value, StringComparison.OrdinalIgnoreCase))
                {
                    dropdown.SetValueWithoutNotify(index);
                    dropdown.RefreshShownValue();
                    RefreshDropdownCaptionState(dropdown);
                    return;
                }
            }
        }

        private static string GetOptionLabel(IReadOnlyList<DropdownOption> options, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                for (int index = 0; index < options.Count; index++)
                {
                    if (string.Equals(options[index].Value, value, StringComparison.OrdinalIgnoreCase))
                    {
                        return options[index].Label;
                    }
                }
            }

            return value ?? string.Empty;
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

        private static void BindInput(TMP_InputField input, Action action)
        {
            if (input == null)
            {
                return;
            }

            input.onValueChanged.RemoveAllListeners();
            input.onValueChanged.AddListener(_ => action?.Invoke());
        }

        private static void BindDropdown(TMP_Dropdown dropdown, Action action)
        {
            if (dropdown == null)
            {
                return;
            }

            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.onValueChanged.AddListener(_ =>
            {
                RefreshDropdownCaptionState(dropdown);
                action?.Invoke();
            });
        }

        private static void RefreshDropdownCaptionState(TMP_Dropdown dropdown)
        {
            if (dropdown == null)
            {
                return;
            }

            TMP_Text captionText = dropdown.captionText;
            Transform placeholder = FindDescendantByName(dropdown.transform, "m_tmpPlaceholder");
            bool hasSelection = dropdown.options.Count > 0;
            if (hasSelection && captionText != null)
            {
                int index = Mathf.Clamp(dropdown.value, 0, dropdown.options.Count - 1);
                captionText.text = dropdown.options[index].text ?? string.Empty;
                SetActive(captionText.gameObject, true);
            }

            SetActive(placeholder != null ? placeholder.gameObject : null, !hasSelection);
        }

        private static string GetInputText(TMP_InputField input)
        {
            return input != null ? input.text?.Trim() ?? string.Empty : string.Empty;
        }

        private static void SetInputText(TMP_InputField input, string value)
        {
            if (input != null)
            {
                input.SetTextWithoutNotify(value ?? string.Empty);
            }
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value ?? string.Empty;
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
            {
                target.SetActive(active);
            }
        }

        private static Transform FindDescendantByName(Transform root, string name)
        {
            if (root == null || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            if (root.name == name)
            {
                return root;
            }

            for (int index = 0; index < root.childCount; index++)
            {
                Transform result = FindDescendantByName(root.GetChild(index), name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static string FormatSignedNumber(int value)
        {
            return value >= 0 ? $"+{value}" : value.ToString();
        }

        private static string FirstNonEmpty(string first, string second)
        {
            return !string.IsNullOrWhiteSpace(first) ? first.Trim() : second?.Trim() ?? string.Empty;
        }

        private static int ParseInt(TMP_InputField input, int fallback)
        {
            string text = GetInputText(input);
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : fallback;
        }

        private void ReturnHome()
        {
            GameModule.UI.CloseUI<ItemInfoEditorUI>();
            if (string.Equals(m_request.ReturnUI, nameof(CharacterCreationUI), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            GameModule.UI.ShowUIAsync<HomeUI>();
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
    }
}
