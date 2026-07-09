using System;
using System.Collections.Generic;
using System.Text;

namespace GameLogic
{
    internal sealed class ItemEditorApplicationService
    {
        private static readonly Lazy<ItemEditorApplicationService> s_instance =
            new Lazy<ItemEditorApplicationService>(() => new ItemEditorApplicationService());

        private ItemEditorApplicationService()
        {
        }

        public static ItemEditorApplicationService Instance => s_instance.Value;

        public bool TryGetRuleItem(string sourceItemId, out ItemEditorRuleItemViewState state)
        {
            state = null;
            if (string.IsNullOrWhiteSpace(sourceItemId)
                || !DndRuleContentService.Instance.TryGetItem(sourceItemId.Trim(), out DndItemDefineData ruleItem))
            {
                return false;
            }

            state = new ItemEditorRuleItemViewState
            {
                Name = ruleItem.Name ?? string.Empty,
                Description = ruleItem.Description ?? string.Empty,
                ItemType = ruleItem.ItemType ?? string.Empty,
                Rarity = ruleItem.Rarity ?? string.Empty,
                ArmorCategory = ruleItem.ArmorCategory ?? string.Empty,
                ArmorBaseAc = ruleItem.ArmorBaseAc,
                AcBonus = ruleItem.AcBonus,
                MaxDexBonus = ruleItem.MaxDexBonus,
                StrengthRequirement = ruleItem.StrengthRequirement,
                StealthDisadvantage = ruleItem.StealthDisadvantage,
                WeaponCategory = ruleItem.WeaponCategory ?? string.Empty,
                WeaponRangeType = ruleItem.WeaponRangeType ?? string.Empty,
                DamageDice = ruleItem.DamageDice ?? string.Empty,
                DamageType = ruleItem.DamageType ?? string.Empty,
                NormalRange = ruleItem.NormalRange,
                LongRange = ruleItem.LongRange,
                TwoHandDamageDice = ruleItem.TwoHandDamageDice ?? string.Empty,
                ToolCategory = ruleItem.ToolCategory ?? string.Empty,
                Consumable = ruleItem.Consumable,
                Charges = ruleItem.Charges,
                ConsumeOnUse = ruleItem.ConsumeOnUse,
                Weight = ruleItem.Weight > 0f ? ruleItem.Weight.ToString("0.##") : string.Empty,
                EffectApplyCondition = ruleItem.EffectApplyCondition ?? string.Empty
            };
            state.WeaponProperties.AddRange(ruleItem.WeaponProperties);
            state.EffectIds.AddRange(ruleItem.EffectIds);
            return true;
        }

        public CharacterOperationResult SaveCustomItem(string customItemId, CharacterEquipmentItemSaveData item)
        {
            customItemId = customItemId?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(customItemId))
            {
                return CharacterOperationResult.Fail("Custom item id is empty.");
            }

            LocalCustomItemRepository.Upsert(new LocalCustomItemSaveData
            {
                CustomItemId = customItemId,
                Item = CharacterEquipmentItemSaveData.Clone(item)
            });
            return CharacterOperationResult.Ok();
        }

        public List<ItemEditorCharacterPickerEntry> LoadCharacterPickerEntries()
        {
            CharacterLibraryViewState library = CharacterApplicationService.Instance.LoadLibrary();
            List<ItemEditorCharacterPickerEntry> entries = new List<ItemEditorCharacterPickerEntry>();
            if (library?.Characters == null)
            {
                return entries;
            }

            for (int index = 0; index < library.Characters.Count; index++)
            {
                CharacterCardDraftSaveData character = library.Characters[index];
                if (character == null || string.IsNullOrWhiteSpace(character.CharacterId))
                {
                    continue;
                }

                entries.Add(new ItemEditorCharacterPickerEntry
                {
                    CharacterId = character.CharacterId,
                    CharacterName = string.IsNullOrWhiteSpace(character.CharacterName) ? character.CharacterId : character.CharacterName,
                    Summary = BuildCharacterPickerSummary(character)
                });
            }

            return entries;
        }

        public ItemEditorAddItemResult AddCustomItemToCharacter(string characterId, string customItemId, CharacterEquipmentItemSaveData item, int quantity)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return FailAdd("Character id is empty.");
            }

            CharacterOperationResult saveResult = SaveCustomItem(customItemId, item);
            if (!saveResult.Success)
            {
                return FailAdd(saveResult.Message);
            }

            if (!LocalCustomItemRepository.TryGetItem(customItemId, out LocalCustomItemSaveData normalizedItem))
            {
                return FailAdd("Saved custom item cannot be loaded.");
            }

            if (!CharacterApplicationService.Instance.TryGetCharacter(characterId, out CharacterCardDraftSaveData character))
            {
                return FailAdd("Character not found.");
            }

            CharacterEquipmentItemSaveData characterItem = LocalCustomItemRepository.CreateCharacterItemSnapshot(
                normalizedItem,
                Math.Max(1, quantity));

            if (CharacterItemCategoryUtility.IsCurrencyItem(characterItem))
            {
                character.Currency ??= new CharacterCurrencySaveData();
                int amount = CharacterItemCategoryUtility.AddCurrency(character.Currency, characterItem, quantity);
                if (amount <= 0)
                {
                    return FailAdd("Currency item data is invalid.");
                }

                CharacterApplicationService.Instance.Save(character);
                return new ItemEditorAddItemResult
                {
                    Success = true,
                    CharacterName = character.CharacterName ?? string.Empty,
                    ItemName = characterItem.ItemName ?? string.Empty,
                    Message = $"Added {characterItem.ItemName} to {character.CharacterName}."
                };
            }

            CharacterInventoryOperationResult inventoryResult = CharacterInventoryApplicationService.Instance.AddItem(
                character.Equipment,
                characterItem,
                Math.Max(1, quantity));
            if (!inventoryResult.Success)
            {
                return FailAdd(inventoryResult.Message);
            }

            character.Equipment = CharacterEquipmentSetSaveData.Clone(inventoryResult.Equipment);
            CharacterApplicationService.Instance.Save(character);
            return new ItemEditorAddItemResult
            {
                Success = true,
                CharacterName = character.CharacterName ?? string.Empty,
                ItemName = characterItem.ItemName ?? string.Empty,
                Message = $"Added {characterItem.ItemName} to {character.CharacterName}."
            };
        }

        private static ItemEditorAddItemResult FailAdd(string message)
        {
            return new ItemEditorAddItemResult
            {
                Success = false,
                Message = message ?? string.Empty
            };
        }

        private static string BuildCharacterPickerSummary(CharacterCardDraftSaveData character)
        {
            if (character == null)
            {
                return string.Empty;
            }

            string race = FirstNonEmpty(character.RaceId, "未选择种族");
            string classText = BuildCharacterClassSummary(character);
            return $"{race} / {classText} / Lv{Math.Max(1, character.Level)}";
        }

        private static string BuildCharacterClassSummary(CharacterCardDraftSaveData character)
        {
            if (character?.ClassProgresses == null || character.ClassProgresses.Count == 0)
            {
                return FirstNonEmpty(character?.ClassId, "未选择职业");
            }

            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < character.ClassProgresses.Count; index++)
            {
                CharacterClassProgressSaveData progress = character.ClassProgresses[index];
                if (progress == null || string.IsNullOrWhiteSpace(progress.ClassId))
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append(" / ");
                }

                builder.Append(progress.ClassId);
                builder.Append(" Lv");
                builder.Append(Math.Max(1, progress.Level));
            }

            return builder.Length > 0 ? builder.ToString() : FirstNonEmpty(character.ClassId, "未选择职业");
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            for (int index = 0; index < values.Length; index++)
            {
                if (!string.IsNullOrWhiteSpace(values[index]))
                {
                    return values[index].Trim();
                }
            }

            return string.Empty;
        }
    }
}
