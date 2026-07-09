using System;
using System.Collections.Generic;

namespace GameLogic
{
    internal sealed class CharacterInventoryApplicationService
    {
        private static readonly Lazy<CharacterInventoryApplicationService> s_instance =
            new Lazy<CharacterInventoryApplicationService>(() => new CharacterInventoryApplicationService());

        private CharacterInventoryApplicationService()
        {
        }

        public static CharacterInventoryApplicationService Instance => s_instance.Value;

        public CharacterInventoryOperationResult AddItem(
            CharacterEquipmentSetSaveData equipment,
            CharacterEquipmentItemSaveData item,
            int quantity = 1)
        {
            CharacterEquipmentSetSaveData working = CloneEquipment(equipment);
            CharacterEquipmentItemSaveData normalizedItem = NormalizeInventoryItem(item, quantity);
            if (!CharacterEquipmentItemSaveData.HasItem(normalizedItem))
            {
                return Fail("Item data is empty.", working);
            }

            EnsureInventoryLists(working);
            if (CharacterItemCategoryUtility.IsInventoryStackable(normalizedItem)
                && TryMergeStackableInventoryItem(working.InventoryItems, normalizedItem, out string mergedItemInstanceId))
            {
                return Ok("Item quantity stacked.", working, mergedItemInstanceId);
            }

            if (!CharacterItemCategoryUtility.IsInventoryStackable(normalizedItem) && normalizedItem.Quantity > 1)
            {
                string firstItemInstanceId = normalizedItem.ItemInstanceId;
                int count = Math.Max(1, normalizedItem.Quantity);
                for (int index = 0; index < count; index++)
                {
                    CharacterEquipmentItemSaveData itemCopy = CharacterEquipmentItemSaveData.Clone(normalizedItem);
                    itemCopy.Quantity = 1;
                    itemCopy.ItemInstanceId = index == 0
                        ? firstItemInstanceId
                        : CreateItemInstanceId();
                    working.InventoryItems.Add(itemCopy);
                }

                return Ok("Items added.", working, firstItemInstanceId);
            }

            working.InventoryItems.Add(normalizedItem);
            return Ok("Item added.", working, normalizedItem.ItemInstanceId);
        }

        public CharacterInventoryOperationResult AddRuleItem(
            CharacterEquipmentSetSaveData equipment,
            string sourceItemId,
            int quantity = 1)
        {
            if (string.IsNullOrWhiteSpace(sourceItemId)
                || !DndRuleContentService.Instance.TryGetItem(sourceItemId.Trim(), out DndItemDefineData ruleItem)
                || ruleItem == null)
            {
                return Fail("Rule item not found.", CloneEquipment(equipment));
            }

            return AddItem(equipment, CreateItemFromRule(ruleItem, quantity), quantity);
        }

        public CharacterInventoryOperationResult RemoveItem(
            CharacterEquipmentSetSaveData equipment,
            string itemInstanceId,
            int quantity = 1)
        {
            CharacterEquipmentSetSaveData working = CloneEquipment(equipment);
            CharacterEquipmentItemSaveData item = FindItem(working, itemInstanceId, out InventoryItemLocation location, out int index);
            if (item == null)
            {
                return Fail("Item not found.", working);
            }

            int removeQuantity = Math.Max(1, quantity);
            if (item.Quantity > removeQuantity)
            {
                item.Quantity -= removeQuantity;
                return Ok("Item quantity removed.", working, item.ItemInstanceId);
            }

            string removedItemId = item.ItemInstanceId;
            ClearItemAt(working, location, index);
            return Ok("Item removed.", working, removedItemId);
        }

        public CharacterInventoryOperationResult DeleteItem(CharacterEquipmentSetSaveData equipment, string itemInstanceId)
        {
            return RemoveItem(equipment, itemInstanceId, int.MaxValue);
        }

        public CharacterInventoryOperationResult EquipItem(CharacterEquipmentSetSaveData equipment, string itemInstanceId)
        {
            CharacterEquipmentSetSaveData working = CloneEquipment(equipment);
            CharacterEquipmentItemSaveData item = FindItem(working, itemInstanceId, out _, out _);
            if (item == null)
            {
                return Fail("Item not found.", working);
            }

            item.IsEquipped = true;
            return Ok("Item equipped.", working, item.ItemInstanceId);
        }

        public CharacterInventoryOperationResult UnequipItem(CharacterEquipmentSetSaveData equipment, string itemInstanceId)
        {
            CharacterEquipmentSetSaveData working = CloneEquipment(equipment);
            CharacterEquipmentItemSaveData item = FindItem(working, itemInstanceId, out _, out _);
            if (item == null)
            {
                return Fail("Item not found.", working);
            }

            item.IsEquipped = false;
            return Ok("Item unequipped.", working, item.ItemInstanceId);
        }

        public CharacterInventoryOperationResult AttuneItem(CharacterEquipmentSetSaveData equipment, string itemInstanceId)
        {
            CharacterEquipmentSetSaveData working = CloneEquipment(equipment);
            CharacterEquipmentItemSaveData item = FindItem(working, itemInstanceId, out _, out _);
            if (item == null)
            {
                return Fail("Item not found.", working);
            }

            if (!item.RequiresAttunement)
            {
                return Fail("Item does not require attunement.", working);
            }

            item.IsAttuned = true;
            return Ok("Item attuned.", working, item.ItemInstanceId);
        }

        public CharacterInventoryOperationResult UnattuneItem(CharacterEquipmentSetSaveData equipment, string itemInstanceId)
        {
            CharacterEquipmentSetSaveData working = CloneEquipment(equipment);
            CharacterEquipmentItemSaveData item = FindItem(working, itemInstanceId, out _, out _);
            if (item == null)
            {
                return Fail("Item not found.", working);
            }

            item.IsAttuned = false;
            return Ok("Item unattuned.", working, item.ItemInstanceId);
        }

        public CharacterInventoryOperationResult UseItem(CharacterEquipmentSetSaveData equipment, string itemInstanceId, int consumeCount = 1)
        {
            CharacterEquipmentSetSaveData working = CloneEquipment(equipment);
            CharacterEquipmentItemSaveData item = FindItem(working, itemInstanceId, out InventoryItemLocation location, out int index);
            if (item == null)
            {
                return Fail("Item not found.", working);
            }

            int amount = Math.Max(1, consumeCount);
            if (item.Charges > 0)
            {
                item.Charges = Math.Max(0, item.Charges - amount);
                return Ok("Item charge consumed.", working, item.ItemInstanceId);
            }

            if (!item.Consumable && !item.ConsumeOnUse)
            {
                return Fail("Item cannot be consumed.", working);
            }

            string usedItemId = item.ItemInstanceId;
            if (item.Quantity > amount)
            {
                item.Quantity -= amount;
                return Ok("Item quantity consumed.", working, usedItemId);
            }

            ClearItemAt(working, location, index);
            return Ok("Item consumed.", working, usedItemId);
        }

        public CharacterOperationResult AddItemToCharacter(
            string characterId,
            CharacterEquipmentItemSaveData item,
            int quantity = 1)
        {
            return SaveCharacterInventory(characterId, equipment => AddItem(equipment, item, quantity));
        }

        public CharacterOperationResult AddRuleItemToCharacter(string characterId, string sourceItemId, int quantity = 1)
        {
            return SaveCharacterInventory(characterId, equipment => AddRuleItem(equipment, sourceItemId, quantity));
        }

        public CharacterOperationResult RemoveItemFromCharacter(string characterId, string itemInstanceId, int quantity = 1)
        {
            return SaveCharacterInventory(characterId, equipment => RemoveItem(equipment, itemInstanceId, quantity));
        }

        public CharacterOperationResult DeleteItemFromCharacter(string characterId, string itemInstanceId)
        {
            return SaveCharacterInventory(characterId, equipment => DeleteItem(equipment, itemInstanceId));
        }

        public CharacterOperationResult EquipCharacterItem(string characterId, string itemInstanceId)
        {
            return SaveCharacterInventory(characterId, equipment => EquipItem(equipment, itemInstanceId));
        }

        public CharacterOperationResult UnequipCharacterItem(string characterId, string itemInstanceId)
        {
            return SaveCharacterInventory(characterId, equipment => UnequipItem(equipment, itemInstanceId));
        }

        public CharacterOperationResult AttuneCharacterItem(string characterId, string itemInstanceId)
        {
            return SaveCharacterInventory(characterId, equipment => AttuneItem(equipment, itemInstanceId));
        }

        public CharacterOperationResult UnattuneCharacterItem(string characterId, string itemInstanceId)
        {
            return SaveCharacterInventory(characterId, equipment => UnattuneItem(equipment, itemInstanceId));
        }

        public CharacterOperationResult UseCharacterItem(string characterId, string itemInstanceId, int consumeCount = 1)
        {
            return SaveCharacterInventory(characterId, equipment => UseItem(equipment, itemInstanceId, consumeCount));
        }

        private CharacterOperationResult SaveCharacterInventory(
            string characterId,
            Func<CharacterEquipmentSetSaveData, CharacterInventoryOperationResult> operation)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return CharacterOperationResult.Fail("Character id is empty.");
            }

            if (operation == null)
            {
                return CharacterOperationResult.Fail("Inventory operation is empty.");
            }

            if (!CharacterApplicationService.Instance.TryGetCharacter(characterId, out CharacterCardDraftSaveData character))
            {
                return CharacterOperationResult.Fail("Character not found.");
            }

            CharacterInventoryOperationResult result = operation(character.Equipment);
            if (!result.Success)
            {
                return CharacterOperationResult.Fail(result.Message);
            }

            character.Equipment = CharacterEquipmentSetSaveData.Clone(result.Equipment);
            return CharacterApplicationService.Instance.Save(character);
        }

        private static CharacterEquipmentSetSaveData CloneEquipment(CharacterEquipmentSetSaveData equipment)
        {
            CharacterEquipmentSetSaveData result = CharacterEquipmentSetSaveData.Clone(equipment);
            EnsureInventoryLists(result);
            return result;
        }

        private static CharacterEquipmentItemSaveData NormalizeInventoryItem(CharacterEquipmentItemSaveData item, int quantity)
        {
            CharacterEquipmentItemSaveData result = CharacterEquipmentItemSaveData.Clone(item);
            result.Quantity = Math.Max(1, quantity > 0 ? quantity : result.Quantity);
            if (string.IsNullOrWhiteSpace(result.ItemInstanceId))
            {
                result.ItemInstanceId = CreateItemInstanceId();
            }

            result.IsEquipped = false;
            return result;
        }

        private static CharacterEquipmentItemSaveData CreateItemFromRule(DndItemDefineData ruleItem, int quantity)
        {
            return new CharacterEquipmentItemSaveData
            {
                ItemInstanceId = CreateItemInstanceId(),
                ItemSourceType = CharacterItemSourceTypes.RuleTable,
                SourceItemId = ruleItem.ItemId?.Trim() ?? string.Empty,
                ItemId = ruleItem.ItemId?.Trim() ?? string.Empty,
                ItemName = ruleItem.Name?.Trim() ?? string.Empty,
                ItemType = ruleItem.ItemType?.Trim() ?? string.Empty,
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
                Charges = Math.Max(0, ruleItem.Charges),
                ConsumeOnUse = ruleItem.ConsumeOnUse,
                Weight = ruleItem.Weight > 0f ? ruleItem.Weight.ToString("0.##") : string.Empty,
                Quantity = Math.Max(1, quantity > 0 ? quantity : ruleItem.DefaultQuantity),
                RequiresAttunement = ruleItem.RequiresAttunement,
                EffectApplyCondition = ruleItem.EffectApplyCondition?.Trim() ?? string.Empty,
                EffectIds = CloneStringList(ruleItem.EffectIds),
                Description = ruleItem.Description ?? string.Empty
            };
        }

        private static CharacterEquipmentItemSaveData FindItem(
            CharacterEquipmentSetSaveData equipment,
            string itemInstanceId,
            out InventoryItemLocation location,
            out int index)
        {
            location = InventoryItemLocation.None;
            index = -1;
            string normalizedId = itemInstanceId?.Trim() ?? string.Empty;
            if (equipment == null || string.IsNullOrWhiteSpace(normalizedId))
            {
                return null;
            }

            if (IsItemId(equipment.Armor, normalizedId))
            {
                location = InventoryItemLocation.Armor;
                return equipment.Armor;
            }

            if (IsItemId(equipment.Shield, normalizedId))
            {
                location = InventoryItemLocation.Shield;
                return equipment.Shield;
            }

            CharacterEquipmentItemSaveData item = FindListItem(equipment.EquippedItems, normalizedId, out index);
            if (item != null)
            {
                location = InventoryItemLocation.EquippedList;
                return item;
            }

            item = FindListItem(equipment.InventoryItems, normalizedId, out index);
            if (item != null)
            {
                location = InventoryItemLocation.Inventory;
                return item;
            }

            return null;
        }

        private static CharacterEquipmentItemSaveData FindListItem(
            List<CharacterEquipmentItemSaveData> items,
            string itemInstanceId,
            out int index)
        {
            index = -1;
            if (items == null)
            {
                return null;
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (IsItemId(items[i], itemInstanceId))
                {
                    index = i;
                    return items[i];
                }
            }

            return null;
        }

        private static bool IsItemId(CharacterEquipmentItemSaveData item, string itemInstanceId)
        {
            return item != null
                && string.Equals(item.ItemInstanceId, itemInstanceId, StringComparison.OrdinalIgnoreCase);
        }

        private static void ClearItemAt(CharacterEquipmentSetSaveData equipment, InventoryItemLocation location, int index)
        {
            switch (location)
            {
                case InventoryItemLocation.Armor:
                    equipment.Armor = new CharacterEquipmentItemSaveData();
                    break;
                case InventoryItemLocation.Shield:
                    equipment.Shield = new CharacterEquipmentItemSaveData();
                    break;
                case InventoryItemLocation.EquippedList:
                    RemoveListItemAt(equipment.EquippedItems, index);
                    break;
                case InventoryItemLocation.Inventory:
                    RemoveListItemAt(equipment.InventoryItems, index);
                    break;
            }
        }

        private static void RemoveListItemAt(List<CharacterEquipmentItemSaveData> items, int index)
        {
            if (items != null && index >= 0 && index < items.Count)
            {
                items.RemoveAt(index);
            }
        }

        private static void EnsureInventoryLists(CharacterEquipmentSetSaveData equipment)
        {
            if (equipment == null)
            {
                return;
            }

            equipment.Armor ??= new CharacterEquipmentItemSaveData();
            equipment.Shield ??= new CharacterEquipmentItemSaveData();
            equipment.EquippedItems ??= new List<CharacterEquipmentItemSaveData>();
            equipment.InventoryItems ??= new List<CharacterEquipmentItemSaveData>();
        }

        private static bool TryMergeStackableInventoryItem(
            List<CharacterEquipmentItemSaveData> inventoryItems,
            CharacterEquipmentItemSaveData item,
            out string mergedItemInstanceId)
        {
            mergedItemInstanceId = string.Empty;
            if (inventoryItems == null || item == null)
            {
                return false;
            }

            for (int index = 0; index < inventoryItems.Count; index++)
            {
                CharacterEquipmentItemSaveData candidate = inventoryItems[index];
                if (!CanStackTogether(candidate, item))
                {
                    continue;
                }

                candidate.Quantity = Math.Max(1, candidate.Quantity) + Math.Max(1, item.Quantity);
                mergedItemInstanceId = candidate.ItemInstanceId;
                return true;
            }

            return false;
        }

        private static bool CanStackTogether(CharacterEquipmentItemSaveData left, CharacterEquipmentItemSaveData right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            if (!CharacterItemCategoryUtility.IsInventoryStackable(left)
                || !CharacterItemCategoryUtility.IsInventoryStackable(right))
            {
                return false;
            }

            if (left.IsEquipped || left.IsAttuned || right.IsEquipped || right.IsAttuned)
            {
                return false;
            }

            return string.Equals(left.ItemSourceType, right.ItemSourceType, StringComparison.OrdinalIgnoreCase)
                && string.Equals(GetStackIdentity(left), GetStackIdentity(right), StringComparison.OrdinalIgnoreCase)
                && string.Equals(left.ItemType?.Trim() ?? string.Empty, right.ItemType?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                && string.Equals(left.Rarity?.Trim() ?? string.Empty, right.Rarity?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                && string.Equals(left.Description ?? string.Empty, right.Description ?? string.Empty, StringComparison.Ordinal);
        }

        private static string GetStackIdentity(CharacterEquipmentItemSaveData item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(item.SourceItemId))
            {
                return item.SourceItemId.Trim();
            }

            if (!string.IsNullOrWhiteSpace(item.ItemId))
            {
                return item.ItemId.Trim();
            }

            return item.ItemName?.Trim() ?? string.Empty;
        }

        private static string FormatStringList(IReadOnlyList<string> source)
        {
            List<string> result = CloneStringList(source);
            return result.Count > 0 ? string.Join(";", result) : string.Empty;
        }

        private static List<string> CloneStringList(IReadOnlyList<string> source)
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

        private static CharacterInventoryOperationResult Ok(
            string message,
            CharacterEquipmentSetSaveData equipment,
            string itemInstanceId)
        {
            return new CharacterInventoryOperationResult
            {
                Success = true,
                Message = message ?? string.Empty,
                Equipment = CharacterEquipmentSetSaveData.Clone(equipment),
                ItemInstanceId = itemInstanceId?.Trim() ?? string.Empty
            };
        }

        private static CharacterInventoryOperationResult Fail(string message, CharacterEquipmentSetSaveData equipment)
        {
            return new CharacterInventoryOperationResult
            {
                Success = false,
                Message = message ?? string.Empty,
                Equipment = CharacterEquipmentSetSaveData.Clone(equipment)
            };
        }

        private static string CreateItemInstanceId()
        {
            return $"item_{Guid.NewGuid():N}";
        }

        private enum InventoryItemLocation
        {
            None,
            Armor,
            Shield,
            EquippedList,
            Inventory
        }
    }
}
