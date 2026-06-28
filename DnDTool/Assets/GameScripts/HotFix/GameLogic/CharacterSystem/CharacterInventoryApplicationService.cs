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
