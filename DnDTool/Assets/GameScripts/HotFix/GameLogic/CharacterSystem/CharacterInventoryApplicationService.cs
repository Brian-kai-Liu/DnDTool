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

            return AddItem(equipment, CharacterItemSnapshotBuilder.BuildInstanceFromRuleItem(ruleItem, quantity), quantity);
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

            CharacterItemTypeBehaviorUtility.ApplyTypeDefaults(item);
            int amount = Math.Max(1, consumeCount);
            if (!CharacterItemTypeBehaviorUtility.IsInventoryItemUsable(item))
            {
                return Fail("Item cannot be used.", working);
            }

            if (CharacterItemTypeBehaviorUtility.CanConsumeChargeOnUse(item))
            {
                if (item.Charges < amount)
                {
                    return Fail("Item does not have enough remaining charges.", working);
                }

                item.Charges -= amount;
                return Ok("Item charge consumed.", working, item.ItemInstanceId);
            }

            if (CharacterItemTypeBehaviorUtility.CanUseWithoutInventoryConsumption(item))
            {
                return Ok("Item used.", working, item.ItemInstanceId);
            }

            if (!CharacterItemTypeBehaviorUtility.CanConsumeQuantityOnUse(item))
            {
                return Fail("Item cannot be consumed.", working);
            }

            if (item.Quantity <= 0)
            {
                return Fail("Item quantity is empty.", working);
            }

            if (item.Quantity < amount)
            {
                return Fail("Item quantity is not enough.", working);
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

        public CharacterInventoryOperationResult RestoreItemCharges(CharacterEquipmentSetSaveData equipment, string itemInstanceId)
        {
            CharacterEquipmentSetSaveData working = CloneEquipment(equipment);
            CharacterEquipmentItemSaveData item = FindItem(working, itemInstanceId, out _, out _);
            if (item == null)
            {
                return Fail("Item not found.", working);
            }

            CharacterItemTypeBehaviorUtility.ApplyTypeDefaults(item);
            if (item.MaxCharges <= 0)
            {
                return Fail("Item has no charge capacity.", working);
            }

            item.Charges = Math.Max(0, item.MaxCharges);
            return Ok("Item charges restored.", working, item.ItemInstanceId);
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

        public CharacterOperationResult RestoreCharacterItemCharges(string characterId, string itemInstanceId)
        {
            return SaveCharacterInventory(characterId, equipment => RestoreItemCharges(equipment, itemInstanceId));
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
            CharacterItemTypeBehaviorUtility.ApplyTypeDefaults(result);

            if (string.IsNullOrWhiteSpace(result.ItemInstanceId))
            {
                result.ItemInstanceId = CreateItemInstanceId();
            }

            result.IsEquipped = false;
            result.IsAttuned = false;
            return result;
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

            if (equipment.Armor == null)
            {
                equipment.Armor = new CharacterEquipmentItemSaveData();
            }

            if (equipment.Shield == null)
            {
                equipment.Shield = new CharacterEquipmentItemSaveData();
            }

            if (equipment.EquippedItems == null)
            {
                equipment.EquippedItems = new List<CharacterEquipmentItemSaveData>();
            }

            if (equipment.InventoryItems == null)
            {
                equipment.InventoryItems = new List<CharacterEquipmentItemSaveData>();
            }
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
                if (!CharacterItemCategoryUtility.CanStackTogether(candidate, item))
                {
                    continue;
                }

                candidate.Quantity = Math.Max(1, candidate.Quantity) + Math.Max(1, item.Quantity);
                mergedItemInstanceId = candidate.ItemInstanceId;
                return true;
            }

            return false;
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
