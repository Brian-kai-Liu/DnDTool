using System;

namespace GameLogic
{
    internal static class CharacterItemTypeBehaviorUtility
    {
        public static CharacterEquipmentItemSaveData ApplyTypeDefaults(CharacterEquipmentItemSaveData item)
        {
            if (item == null)
            {
                return null;
            }

            DndItemTypeDefineData itemType = FindItemType(item);
            if (itemType == null)
            {
                NormalizeChargeBounds(item);
                return item;
            }

            if (itemType.IsEquipmentType && string.IsNullOrWhiteSpace(item.EquipmentSlot))
            {
                item.EquipmentSlot = itemType.DefaultEquipmentSlot?.Trim() ?? string.Empty;
            }

            NormalizeChargeBounds(item);
            return item;
        }

        public static string ResolveRuleItemTypeId(DndItemDefineData ruleItem)
        {
            string itemTypeId = ruleItem?.ItemType?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(itemTypeId))
            {
                return string.Empty;
            }

            DndItemTypeDefineData itemType = FindItemType(itemTypeId);
            if (itemType != null && itemType.Selectable)
            {
                return itemType.ItemTypeId;
            }

            string armorCategory = ruleItem?.ArmorCategory?.Trim() ?? string.Empty;
            if (IsSelectableItemType(armorCategory))
            {
                return armorCategory;
            }

            string weaponCategory = ruleItem?.WeaponCategory?.Trim() ?? string.Empty;
            if (IsSelectableItemType(weaponCategory))
            {
                return weaponCategory;
            }

            string toolCategory = ruleItem?.ToolCategory?.Trim() ?? string.Empty;
            if (IsSelectableItemType(toolCategory))
            {
                return toolCategory;
            }

            return itemTypeId;
        }

        public static string ResolveRuleItemTypeId(ItemEditorRuleItemViewState ruleItem)
        {
            string itemTypeId = ruleItem?.ItemType?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(itemTypeId))
            {
                return string.Empty;
            }

            DndItemTypeDefineData itemType = FindItemType(itemTypeId);
            if (itemType != null && itemType.Selectable)
            {
                return itemType.ItemTypeId;
            }

            string armorCategory = ruleItem?.ArmorCategory?.Trim() ?? string.Empty;
            if (IsSelectableItemType(armorCategory))
            {
                return armorCategory;
            }

            string weaponCategory = ruleItem?.WeaponCategory?.Trim() ?? string.Empty;
            if (IsSelectableItemType(weaponCategory))
            {
                return weaponCategory;
            }

            string toolCategory = ruleItem?.ToolCategory?.Trim() ?? string.Empty;
            if (IsSelectableItemType(toolCategory))
            {
                return toolCategory;
            }

            return itemTypeId;
        }

        public static bool IsInventoryItemUsable(CharacterEquipmentItemSaveData item)
        {
            if (item == null)
            {
                return false;
            }

            DndItemTypeDefineData itemType = FindItemType(item);
            if (itemType == null)
            {
                return false;
            }

            if (item.Quantity <= 0)
            {
                return false;
            }

            if (!itemType.CanUseByDefault)
            {
                return false;
            }

            if (CanConsumeChargeOnUse(item, itemType))
            {
                return item.Charges > 0;
            }

            return true;
        }

        public static bool CanRestoreInventoryItemCharges(CharacterEquipmentItemSaveData item)
        {
            if (item == null)
            {
                return false;
            }

            DndItemTypeDefineData itemType = FindItemType(item);
            return itemType != null
                && HasChargeCapacity(item, itemType)
                && item.Charges < item.MaxCharges;
        }

        public static bool ConsumesChargeOnUse(CharacterEquipmentItemSaveData item)
        {
            return CanConsumeChargeOnUse(item, FindItemType(item));
        }

        public static bool ConsumesQuantityOnUse(CharacterEquipmentItemSaveData item)
        {
            return CanConsumeQuantityOnUse(item);
        }

        public static bool CanConsumeQuantityOnUse(CharacterEquipmentItemSaveData item)
        {
            DndItemTypeDefineData itemType = FindItemType(item);
            return itemType != null
                && itemType.CanUseByDefault
                && itemType.ConsumeQuantityOnUseByDefault;
        }

        public static bool CanConsumeChargeOnUse(CharacterEquipmentItemSaveData item)
        {
            return CanConsumeChargeOnUse(item, FindItemType(item));
        }

        public static bool CanUseWithoutInventoryConsumption(CharacterEquipmentItemSaveData item)
        {
            if (item == null)
            {
                return false;
            }

            DndItemTypeDefineData itemType = FindItemType(item);
            return itemType != null
                && itemType.CanUseByDefault
                && !itemType.ConsumeQuantityOnUseByDefault
                && !itemType.ConsumeChargeOnUseByDefault;
        }

        public static bool IsInventoryItemEquippable(CharacterEquipmentItemSaveData item)
        {
            if (item == null)
            {
                return false;
            }

            if (item.IsEquipped)
            {
                return true;
            }

            DndItemTypeDefineData itemType = FindItemType(item);
            return itemType != null && itemType.IsEquipmentType;
        }

        private static bool HasChargeCapacity(CharacterEquipmentItemSaveData item, DndItemTypeDefineData itemType)
        {
            if (item == null)
            {
                return false;
            }

            return itemType != null
                && (itemType.CanHaveCharges || itemType.ConsumeChargeOnUseByDefault)
                && (item.MaxCharges > 0 || item.Charges > 0);
        }

        private static bool CanConsumeChargeOnUse(CharacterEquipmentItemSaveData item, DndItemTypeDefineData itemType)
        {
            if (item == null)
            {
                return false;
            }

            return itemType != null
                && itemType.CanUseByDefault
                && itemType.ConsumeChargeOnUseByDefault
                && HasChargeCapacity(item, itemType);
        }

        private static void NormalizeChargeBounds(CharacterEquipmentItemSaveData item)
        {
            if (item == null)
            {
                return;
            }

            item.Charges = Math.Max(0, item.Charges);
            item.MaxCharges = Math.Max(0, item.MaxCharges);
            if (item.MaxCharges <= 0 && item.Charges > 0)
            {
                item.MaxCharges = item.Charges;
            }

            if (item.MaxCharges > 0 && item.Charges > item.MaxCharges)
            {
                item.Charges = item.MaxCharges;
            }
        }

        private static DndItemTypeDefineData FindItemType(CharacterEquipmentItemSaveData item)
        {
            string itemTypeId = item?.ItemType?.Trim() ?? string.Empty;
            return FindItemType(itemTypeId);
        }

        private static DndItemTypeDefineData FindItemType(string itemTypeId)
        {
            return !string.IsNullOrWhiteSpace(itemTypeId)
                && DndRuleContentService.Instance.TryGetItemType(itemTypeId, out DndItemTypeDefineData itemType)
                    ? itemType
                    : null;
        }

        private static bool IsSelectableItemType(string itemTypeId)
        {
            DndItemTypeDefineData itemType = FindItemType(itemTypeId);
            return itemType != null && itemType.Selectable;
        }
    }
}
