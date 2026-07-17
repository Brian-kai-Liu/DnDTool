using System;

namespace GameLogic
{
    internal static class CharacterItemSnapshotBuilder
    {
        public static CharacterEquipmentItemSaveData BuildTemplateFromCustomItem(CharacterEquipmentItemSaveData source, string customItemId)
        {
            CharacterEquipmentItemSaveData item = CharacterEquipmentItemSaveData.Clone(source);
            item.ItemSourceType = CharacterItemSourceTypes.Custom;
            item.SourceItemId = customItemId?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(item.ItemId))
            {
                item.ItemId = item.SourceItemId;
            }

            CharacterItemTypeBehaviorUtility.ApplyTypeDefaults(item);
            ClearInstanceState(item);
            return item;
        }

        public static CharacterEquipmentItemSaveData BuildInstanceFromCustomItem(LocalCustomItemSaveData customItem, int quantity)
        {
            LocalCustomItemSaveData normalized = LocalCustomItemSaveData.Clone(customItem);
            CharacterEquipmentItemSaveData item = BuildTemplateFromCustomItem(normalized.Item, normalized.CustomItemId);
            item.ItemInstanceId = CreateItemInstanceId();
            item.Quantity = Math.Max(1, quantity);
            InitializeInstanceState(item);
            return item;
        }

        public static CharacterEquipmentItemSaveData BuildInstanceFromRuleItem(DndItemDefineData ruleItem, int quantity)
        {
            if (ruleItem == null)
            {
                return new CharacterEquipmentItemSaveData();
            }

            CharacterEquipmentItemSaveData item = new CharacterEquipmentItemSaveData
            {
                ItemInstanceId = CreateItemInstanceId(),
                ItemSourceType = CharacterItemSourceTypes.RuleTable,
                SourceItemId = ruleItem.ItemId?.Trim() ?? string.Empty,
                ItemId = ruleItem.ItemId?.Trim() ?? string.Empty,
                ItemName = ruleItem.Name?.Trim() ?? string.Empty,
                ItemType = CharacterItemTypeBehaviorUtility.ResolveRuleItemTypeId(ruleItem),
                Rarity = ruleItem.Rarity?.Trim() ?? string.Empty,
                ArmorCategory = CharacterArmorCategoryIds.Normalize(ruleItem.ArmorCategory),
                ArmorBaseAc = Math.Max(0, ruleItem.ArmorBaseAc),
                AcBonus = ruleItem.AcBonus,
                MaxDexBonus = Math.Max(0, ruleItem.MaxDexBonus),
                StrengthRequirement = Math.Max(0, ruleItem.StrengthRequirement),
                StealthDisadvantage = ruleItem.StealthDisadvantage,
                WeaponCategory = ruleItem.WeaponCategory?.Trim() ?? string.Empty,
                WeaponRangeType = ruleItem.WeaponRangeType?.Trim() ?? string.Empty,
                DamageDice = ruleItem.DamageDice?.Trim() ?? string.Empty,
                DamageType = ruleItem.DamageType?.Trim() ?? string.Empty,
                WeaponProperties = FormatStringList(ruleItem.WeaponProperties),
                NormalRange = Math.Max(0, ruleItem.NormalRange),
                LongRange = Math.Max(0, ruleItem.LongRange),
                TwoHandDamageDice = ruleItem.TwoHandDamageDice?.Trim() ?? string.Empty,
                ToolCategory = ruleItem.ToolCategory?.Trim() ?? string.Empty,
                Consumable = ruleItem.Consumable,
                MaxCharges = Math.Max(0, ruleItem.Charges),
                ConsumeOnUse = ruleItem.ConsumeOnUse,
                Weight = ruleItem.Weight > 0f ? ruleItem.Weight.ToString("0.##") : string.Empty,
                PriceGp = Math.Max(0, ruleItem.PriceGp),
                Quantity = Math.Max(1, quantity > 0 ? quantity : ruleItem.DefaultQuantity),
                IsEquippable = ruleItem.IsEquippable,
                EquipmentSlot = ruleItem.EquipmentSlot?.Trim() ?? string.Empty,
                RequiresAttunement = ruleItem.RequiresAttunement,
                EffectApplyCondition = ruleItem.EffectApplyCondition?.Trim() ?? string.Empty,
                EffectIds = CloneStringList(ruleItem.EffectIds),
                Description = ruleItem.Description ?? string.Empty
            };

            CharacterItemTypeBehaviorUtility.ApplyTypeDefaults(item);
            InitializeInstanceState(item);
            return item;
        }

        public static void ClearInstanceState(CharacterEquipmentItemSaveData item)
        {
            if (item == null)
            {
                return;
            }

            item.ItemInstanceId = string.Empty;
            item.Quantity = 1;
            item.Charges = 0;
            item.IsEquipped = false;
            item.IsAttuned = false;
        }

        public static void InitializeInstanceState(CharacterEquipmentItemSaveData item)
        {
            if (item == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(item.ItemInstanceId))
            {
                item.ItemInstanceId = CreateItemInstanceId();
            }

            item.Quantity = Math.Max(1, item.Quantity);
            item.Charges = Math.Max(0, item.MaxCharges);
            item.IsEquipped = false;
            item.IsAttuned = false;
        }

        private static string CreateItemInstanceId()
        {
            return $"item_instance_{Guid.NewGuid():N}";
        }

        private static string FormatStringList(System.Collections.Generic.IReadOnlyList<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return string.Empty;
            }

            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            for (int index = 0; index < values.Count; index++)
            {
                string value = values[index]?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append("; ");
                }

                builder.Append(value);
            }

            return builder.ToString();
        }

        private static System.Collections.Generic.List<string> CloneStringList(System.Collections.Generic.IReadOnlyList<string> source)
        {
            System.Collections.Generic.List<string> result = new System.Collections.Generic.List<string>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                string value = source[index]?.Trim() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    result.Add(value);
                }
            }

            return result;
        }
    }
}
