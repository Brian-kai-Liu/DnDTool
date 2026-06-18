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

        private static readonly DropdownOption[] ItemTypeOptions =
        {
            new DropdownOption("armor", "护甲"),
            new DropdownOption("shield", "盾牌"),
            new DropdownOption("weapon", "武器"),
            new DropdownOption("tool", "工具"),
            new DropdownOption("consumable", "消耗品"),
            new DropdownOption("wondrous_item", "奇物")
        };

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
            new DropdownOption("AddAbility", "属性加值"),
            new DropdownOption("AddAc", "护甲等级加值"),
            new DropdownOption("AddSpeed", "移动速度加值"),
            new DropdownOption("AddInitiative", "先攻加值"),
            new DropdownOption("AddSkillBonus", "技能加值"),
            new DropdownOption("AddSpellAttackBonus", "法术攻击加值"),
            new DropdownOption("AddSpellSaveDc", "法术豁免DC加值"),
            new DropdownOption("AddProficiency", "熟练项"),
            new DropdownOption("ManualResolve", "手动处理")
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
        private TMP_InputField m_inputQuantity;
        private TMP_InputField m_inputArmorBaseAc;
        private TMP_InputField m_inputAcBonus;
        private TMP_InputField m_inputEffectTarget;
        private TMP_InputField m_inputEffectValue;
        private TMP_InputField m_inputEffectCondition;
        private TMP_InputField m_inputEffectDescription;
        private TMP_Dropdown m_dropdownItemType;
        private TMP_Dropdown m_dropdownItemSourceType;
        private TMP_Dropdown m_dropdownArmorCategory;
        private TMP_Dropdown m_dropdownEffectType;
        private Toggle m_toggleIsEquipped;
        private Toggle m_toggleRequiresAttunement;
        private Toggle m_toggleIsAttuned;
        private TMP_Text m_tmpPreviewItemName;
        private TMP_Text m_tmpPreviewItemType;
        private TMP_Text m_tmpPreviewSource;
        private TMP_Text m_tmpPreviewEquipState;
        private TMP_Text m_tmpPreviewAc;
        private TMP_Text m_tmpPreviewEffects;
        private ScrollRect m_scrollItemForm;
        private RectTransform m_rectItemFormContent;
        private RectTransform m_panelCharacterPicker;
        private RectTransform m_rectCharacterPickerContent;
        private GameObject m_goCharacterPickerTemplate;
        private Button m_btnCancelCharacterPicker;
        private Button m_btnConfirmAddToCharacter;
        private TMP_Text m_tmpCharacterPickerMessage;

        private readonly List<CharacterItemEffectSaveData> m_customEffects = new List<CharacterItemEffectSaveData>();
        private readonly List<GameObject> m_characterPickerItems = new List<GameObject>();
        private List<ItemEditorCharacterPickerEntry> m_characterPickerCharacters = new List<ItemEditorCharacterPickerEntry>();
        private int m_selectedCharacterPickerIndex = -1;

        protected override void ScriptGenerator()
        {
            BindControls();
            InitializeDropdowns();
            BindButtons();
            RefreshPreview();
            RefreshEffectPreview();
            SyncToggleVisuals();
            RefreshItemFormScrollArea(true);
        }

        private void BindControls()
        {
            m_btnCancelItem = FindChildComponent<Button>("m_btnCancelItem");
            m_btnSaveItem = FindChildComponent<Button>("m_btnSaveItem");
            m_btnAddToCharacter = FindChildComponent<Button>("m_btnAddToCharacter");
            m_btnLoadFromRuleTable = FindChildComponent<Button>("m_btnLoadFromRuleTable");
            m_btnAddEffect = FindChildComponent<Button>("m_btnAddEffect");
            m_btnRemoveEffect = FindChildComponent<Button>("m_btnRemoveEffect");

            m_inputItemName = FindChildComponent<TMP_InputField>("m_inputItemName");
            m_inputDescription = FindChildComponent<TMP_InputField>("m_inputDescription");
            m_inputNotes = FindChildComponent<TMP_InputField>("m_inputNotes");
            m_inputSourceItemId = FindChildComponent<TMP_InputField>("m_inputSourceItemId");
            m_inputQuantity = FindChildComponent<TMP_InputField>("m_inputQuantity");
            m_inputArmorBaseAc = FindChildComponent<TMP_InputField>("m_inputArmorBaseAc");
            m_inputAcBonus = FindChildComponent<TMP_InputField>("m_inputAcBonus");
            m_inputEffectTarget = FindChildComponent<TMP_InputField>("m_inputEffectTarget");
            m_inputEffectValue = FindChildComponent<TMP_InputField>("m_inputEffectValue");
            m_inputEffectCondition = FindChildComponent<TMP_InputField>("m_inputEffectCondition");
            m_inputEffectDescription = FindChildComponent<TMP_InputField>("m_inputEffectDescription");

            m_dropdownItemType = FindChildComponent<TMP_Dropdown>("m_dropdownItemType");
            m_dropdownItemSourceType = FindChildComponent<TMP_Dropdown>("m_dropdownItemSourceType");
            m_dropdownArmorCategory = FindChildComponent<TMP_Dropdown>("m_dropdownArmorCategory");
            m_dropdownEffectType = FindChildComponent<TMP_Dropdown>("m_dropdownEffectType");

            m_toggleIsEquipped = FindChildComponent<Toggle>("m_toggleIsEquipped");
            m_toggleRequiresAttunement = FindChildComponent<Toggle>("m_toggleRequiresAttunement");
            m_toggleIsAttuned = FindChildComponent<Toggle>("m_toggleIsAttuned");

            m_tmpPreviewItemName = FindChildComponent<TMP_Text>("m_tmpPreviewItemName");
            m_tmpPreviewItemType = FindChildComponent<TMP_Text>("m_tmpPreviewItemType");
            m_tmpPreviewSource = FindChildComponent<TMP_Text>("m_tmpPreviewSource");
            m_tmpPreviewEquipState = FindChildComponent<TMP_Text>("m_tmpPreviewEquipState");
            m_tmpPreviewAc = FindChildComponent<TMP_Text>("m_tmpPreviewAc");
            m_tmpPreviewEffects = FindChildComponent<TMP_Text>("m_tmpPreviewEffects");
            m_scrollItemForm = FindChildComponent<ScrollRect>("m_scrollItemForm");
            m_rectItemFormContent = FindChildComponent<RectTransform>("m_rectItemFormContent");
            m_panelCharacterPicker = FindChildComponent<RectTransform>("m_panelCharacterPicker");
            m_rectCharacterPickerContent = FindChildComponent<RectTransform>("m_rectCharacterPickerContent");
            m_goCharacterPickerTemplate = FindChildComponent<RectTransform>("m_itemCharacterPickerTemplate")?.gameObject;
            m_btnCancelCharacterPicker = FindChildComponent<Button>("m_btnCancelCharacterPicker");
            m_btnConfirmAddToCharacter = FindChildComponent<Button>("m_btnConfirmAddToCharacter");
            m_tmpCharacterPickerMessage = FindChildComponent<TMP_Text>("m_tmpCharacterPickerMessage");
        }

        private void InitializeDropdowns()
        {
            SetupDropdown(m_dropdownItemType, ItemTypeOptions);
            SetupDropdown(m_dropdownItemSourceType, SourceTypeOptions);
            SetupDropdown(m_dropdownArmorCategory, ArmorCategoryOptions);
            SetupDropdown(m_dropdownEffectType, EffectTypeOptions);
        }

        private void BindButtons()
        {
            BindButton(m_btnCancelItem, ReturnHome);
            BindButton(m_btnSaveItem, SaveItem);
            BindButton(m_btnAddToCharacter, AddToCharacter);
            BindButton(m_btnLoadFromRuleTable, LoadFromRuleTable);
            BindButton(m_btnAddEffect, AddEffect);
            BindButton(m_btnRemoveEffect, RemoveEffect);
            BindButton(m_btnCancelCharacterPicker, HideCharacterPicker);
            BindButton(m_btnConfirmAddToCharacter, ConfirmAddToCharacter);

            BindInput(m_inputItemName, RefreshPreview);
            BindInput(m_inputDescription, RefreshPreview);
            BindInput(m_inputNotes, RefreshPreview);
            BindInput(m_inputSourceItemId, RefreshPreview);
            BindInput(m_inputQuantity, RefreshPreview);
            BindInput(m_inputArmorBaseAc, RefreshPreview);
            BindInput(m_inputAcBonus, RefreshPreview);
            BindInput(m_inputEffectTarget, RefreshPreview);
            BindInput(m_inputEffectValue, RefreshPreview);
            BindInput(m_inputEffectCondition, RefreshPreview);
            BindInput(m_inputEffectDescription, RefreshPreview);

            BindDropdown(m_dropdownItemType, RefreshPreview);
            BindDropdown(m_dropdownItemSourceType, RefreshPreview);
            BindDropdown(m_dropdownArmorCategory, RefreshPreview);
            BindDropdown(m_dropdownEffectType, RefreshPreview);

            BindToggle(m_toggleIsEquipped, RefreshPreview);
            BindToggle(m_toggleRequiresAttunement, RefreshPreview);
            BindToggle(m_toggleIsAttuned, RefreshPreview);
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
            SetInputText(m_inputQuantity, "1");
            SetInputText(m_inputArmorBaseAc, ruleItem.ArmorBaseAc.ToString());
            SetInputText(m_inputAcBonus, ruleItem.AcBonus.ToString());
            SelectDropdownValue(m_dropdownItemSourceType, SourceTypeOptions, CharacterItemSourceTypes.RuleTable);
            SelectDropdownValue(m_dropdownItemType, ItemTypeOptions, NormalizeOptionOrFirst(ItemTypeOptions, ruleItem.ItemType));
            SelectDropdownValue(m_dropdownArmorCategory, ArmorCategoryOptions, NormalizeOptionOrFirst(ArmorCategoryOptions, ruleItem.ArmorCategory));
            SetToggle(m_toggleIsEquipped, ruleItem.DefaultEquipped);
            SetToggle(m_toggleRequiresAttunement, ruleItem.RequiresAttunement);
            SetToggle(m_toggleIsAttuned, false);
            RefreshPreview();
        }

        private void AddEffect()
        {
            CharacterItemEffectSaveData effect = new CharacterItemEffectSaveData
            {
                EffectType = GetSelectedDropdownValue(m_dropdownEffectType, EffectTypeOptions),
                Target = GetInputText(m_inputEffectTarget),
                Value = GetInputText(m_inputEffectValue),
                Condition = GetInputText(m_inputEffectCondition),
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
                Math.Max(1, ParseInt(m_inputQuantity, 1)));
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

        private CharacterEquipmentItemSaveData BuildItemData()
        {
            CharacterEquipmentItemSaveData item = new CharacterEquipmentItemSaveData
            {
                ItemName = GetInputText(m_inputItemName),
                ItemType = GetSelectedDropdownValue(m_dropdownItemType, ItemTypeOptions),
                Description = GetInputText(m_inputDescription),
                Notes = GetInputText(m_inputNotes),
                ItemSourceType = GetSelectedDropdownValue(m_dropdownItemSourceType, SourceTypeOptions),
                SourceItemId = GetInputText(m_inputSourceItemId),
                ArmorCategory = GetSelectedDropdownValue(m_dropdownArmorCategory, ArmorCategoryOptions),
                ArmorBaseAc = ParseInt(m_inputArmorBaseAc, 0),
                AcBonus = ParseInt(m_inputAcBonus, 0),
                Quantity = Math.Max(1, ParseInt(m_inputQuantity, 1)),
                IsEquipped = m_toggleIsEquipped != null && m_toggleIsEquipped.isOn,
                RequiresAttunement = m_toggleRequiresAttunement != null && m_toggleRequiresAttunement.isOn,
                IsAttuned = m_toggleIsAttuned != null && m_toggleIsAttuned.isOn
            };

            item.EffectIds.Clear();
            item.CustomEffects.Clear();
            for (int index = 0; index < m_customEffects.Count; index++)
            {
                CharacterItemEffectSaveData effect = m_customEffects[index];
                if (effect != null && !string.IsNullOrWhiteSpace(effect.EffectType))
                {
                    item.CustomEffects.Add(CharacterItemEffectSaveData.Clone(effect));
                }
            }

            return item;
        }

        private string EnsureCustomItemId()
        {
            string customItemId = GetInputText(m_inputSourceItemId);
            if (string.IsNullOrWhiteSpace(customItemId))
            {
                customItemId = $"custom_item_{DateTime.UtcNow:yyyyMMddHHmmssfff}";
                SetInputText(m_inputSourceItemId, customItemId);
            }

            return customItemId;
        }

        private void RefreshPreview()
        {
            CharacterEquipmentItemSaveData item = BuildItemData();
            SetText(m_tmpPreviewItemName, item.ItemName);
            SetText(m_tmpPreviewItemType, GetOptionLabel(ItemTypeOptions, item.ItemType));
            SetText(m_tmpPreviewSource, GetOptionLabel(SourceTypeOptions, item.ItemSourceType));
            SetText(m_tmpPreviewEquipState, item.IsEquipped ? "已装备" : "未装备");
            SetText(m_tmpPreviewAc, item.ArmorBaseAc > 0 ? item.ArmorBaseAc.ToString() : item.AcBonus != 0 ? FormatSignedNumber(item.AcBonus) : "-");
        }

        private void RefreshEffectPreview()
        {
            if (m_tmpPreviewEffects == null)
            {
                RefreshItemFormScrollArea(false);
                return;
            }

            if (m_customEffects.Count == 0)
            {
                m_tmpPreviewEffects.text = string.Empty;
                RefreshItemFormScrollArea(false);
                return;
            }

            StringBuilder builder = new StringBuilder();
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

                builder.Append(GetOptionLabel(EffectTypeOptions, effect.EffectType));
                if (!string.IsNullOrWhiteSpace(effect.Target))
                {
                    builder.Append(" ");
                    builder.Append(effect.Target);
                }
                if (!string.IsNullOrWhiteSpace(effect.Value))
                {
                    builder.Append(" ");
                    builder.Append(effect.Value);
                }
                if (!string.IsNullOrWhiteSpace(effect.Description))
                {
                    builder.Append(" - ");
                    builder.Append(effect.Description);
                }
            }

            m_tmpPreviewEffects.text = builder.ToString();
            RefreshItemFormScrollArea(false);
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
            SetInputText(m_inputEffectDescription, string.Empty);
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
                if (!string.IsNullOrWhiteSpace(option.Value))
                {
                    data.Add(new TMP_Dropdown.OptionData(option.Label));
                }
            }

            dropdown.AddOptions(data);
            if (dropdown.options.Count > 0)
            {
                dropdown.value = 0;
            }
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
                    dropdown.value = index;
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
            dropdown.onValueChanged.AddListener(_ => action?.Invoke());
        }

        private static void BindToggle(Toggle toggle, Action action)
        {
            if (toggle == null)
            {
                return;
            }

            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(_ => action?.Invoke());
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

        private static void SetToggle(Toggle toggle, bool value)
        {
            if (toggle != null)
            {
                toggle.SetIsOnWithoutNotify(value);
                SyncToggleVisual(toggle);
            }
        }

        private void SyncToggleVisuals()
        {
            SyncToggleVisual(m_toggleIsEquipped);
            SyncToggleVisual(m_toggleRequiresAttunement);
            SyncToggleVisual(m_toggleIsAttuned);
        }

        private static void SyncToggleVisual(Toggle toggle)
        {
            if (toggle != null && toggle.graphic != null)
            {
                toggle.graphic.canvasRenderer.SetAlpha(toggle.isOn ? 1f : 0f);
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

        private static string FormatSignedNumber(int value)
        {
            return value >= 0 ? $"+{value}" : value.ToString();
        }

        private static int ParseInt(TMP_InputField input, int fallback)
        {
            string text = GetInputText(input);
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : fallback;
        }

        private void ReturnHome()
        {
            GameModule.UI.CloseUI<ItemInfoEditorUI>();
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
