using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TEngine;
using UnityEngine;
using Log = TEngine.Log;

namespace GameLogic
{
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
        public List<CharacterDiceRollHistorySaveData> DiceRollHistory = new List<CharacterDiceRollHistorySaveData>();
        public List<CharacterCustomFeatureSaveData> CustomFeatures = new List<CharacterCustomFeatureSaveData>();
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
        public string Gender = string.Empty;

        public static CharacterIdentityProfileSaveData Clone(CharacterIdentityProfileSaveData source)
        {
            if (source == null)
            {
                return new CharacterIdentityProfileSaveData();
            }

            return new CharacterIdentityProfileSaveData
            {
                Gender = source.Gender ?? string.Empty
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
    internal sealed class CharacterCustomFeatureSaveData
    {
        public string Name = string.Empty;
        public string Description = string.Empty;

        public static CharacterCustomFeatureSaveData Clone(CharacterCustomFeatureSaveData source)
        {
            if (source == null)
            {
                return new CharacterCustomFeatureSaveData();
            }

            return new CharacterCustomFeatureSaveData
            {
                Name = source.Name?.Trim() ?? string.Empty,
                Description = source.Description?.Trim() ?? string.Empty
            };
        }

        public static List<CharacterCustomFeatureSaveData> CloneList(List<CharacterCustomFeatureSaveData> source)
        {
            List<CharacterCustomFeatureSaveData> result = new List<CharacterCustomFeatureSaveData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterCustomFeatureSaveData feature = Clone(source[index]);
                if (string.IsNullOrWhiteSpace(feature.Name) && string.IsNullOrWhiteSpace(feature.Description))
                {
                    continue;
                }

                result.Add(feature);
            }

            return result;
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
        public string Rarity = string.Empty;
        public string ArmorCategory = CharacterArmorCategoryIds.None;
        public int ArmorBaseAc;
        public int AcBonus;
        public int MaxDexBonus;
        public int StrengthRequirement;
        public bool StealthDisadvantage;
        public string WeaponCategory = string.Empty;
        public string WeaponRangeType = string.Empty;
        public string DamageDice = string.Empty;
        public string DamageType = string.Empty;
        public string WeaponProperties = string.Empty;
        public int NormalRange;
        public int LongRange;
        public string TwoHandDamageDice = string.Empty;
        public string ToolCategory = string.Empty;
        public bool Consumable;
        public int Charges;
        public bool ConsumeOnUse;
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
                Rarity = source.Rarity?.Trim() ?? string.Empty,
                ArmorCategory = CharacterArmorCategoryIds.Normalize(source.ArmorCategory),
                ArmorBaseAc = Math.Max(0, source.ArmorBaseAc),
                AcBonus = source.AcBonus,
                MaxDexBonus = Math.Max(0, source.MaxDexBonus),
                StrengthRequirement = Math.Max(0, source.StrengthRequirement),
                StealthDisadvantage = source.StealthDisadvantage,
                WeaponCategory = source.WeaponCategory?.Trim() ?? string.Empty,
                WeaponRangeType = source.WeaponRangeType?.Trim() ?? string.Empty,
                DamageDice = source.DamageDice?.Trim() ?? string.Empty,
                DamageType = source.DamageType?.Trim() ?? string.Empty,
                WeaponProperties = source.WeaponProperties?.Trim() ?? string.Empty,
                NormalRange = Math.Max(0, source.NormalRange),
                LongRange = Math.Max(0, source.LongRange),
                TwoHandDamageDice = source.TwoHandDamageDice?.Trim() ?? string.Empty,
                ToolCategory = source.ToolCategory?.Trim() ?? string.Empty,
                Consumable = source.Consumable,
                Charges = Math.Max(0, source.Charges),
                ConsumeOnUse = source.ConsumeOnUse,
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
                    || !string.IsNullOrWhiteSpace(item.Rarity)
                    || !string.IsNullOrWhiteSpace(item.Weight)
                    || item.ArmorBaseAc > 0
                    || item.AcBonus != 0
                    || item.MaxDexBonus > 0
                    || item.StrengthRequirement > 0
                    || item.StealthDisadvantage
                    || !string.IsNullOrWhiteSpace(item.WeaponCategory)
                    || !string.IsNullOrWhiteSpace(item.WeaponRangeType)
                    || !string.IsNullOrWhiteSpace(item.DamageDice)
                    || !string.IsNullOrWhiteSpace(item.DamageType)
                    || !string.IsNullOrWhiteSpace(item.WeaponProperties)
                    || item.NormalRange > 0
                    || item.LongRange > 0
                    || !string.IsNullOrWhiteSpace(item.TwoHandDamageDice)
                    || !string.IsNullOrWhiteSpace(item.ToolCategory)
                    || item.Consumable
                    || item.Charges > 0
                    || item.ConsumeOnUse
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
        public string Name = string.Empty;
        public string EffectType = string.Empty;
        public string Target = string.Empty;
        public string Value = string.Empty;
        public string Condition = string.Empty;
        public string ConditionDescription = string.Empty;
        public string Description = string.Empty;
        public bool EnableQuickRoll;
        public string DiceExpression = string.Empty;

        public static CharacterItemEffectSaveData Clone(CharacterItemEffectSaveData source)
        {
            if (source == null)
            {
                return new CharacterItemEffectSaveData();
            }

            return new CharacterItemEffectSaveData
            {
                Name = source.Name ?? string.Empty,
                EffectType = source.EffectType?.Trim() ?? string.Empty,
                Target = source.Target?.Trim() ?? string.Empty,
                Value = source.Value?.Trim() ?? string.Empty,
                Condition = source.Condition?.Trim() ?? string.Empty,
                ConditionDescription = source.ConditionDescription ?? string.Empty,
                Description = source.Description ?? string.Empty,
                EnableQuickRoll = source.EnableQuickRoll,
                DiceExpression = source.DiceExpression?.Trim() ?? string.Empty
            };
        }

        public static bool HasContent(CharacterItemEffectSaveData effect)
        {
            return effect != null
                && (!string.IsNullOrWhiteSpace(effect.Name)
                    || !string.IsNullOrWhiteSpace(effect.EffectType)
                    || !string.IsNullOrWhiteSpace(effect.Target)
                    || !string.IsNullOrWhiteSpace(effect.Value)
                    || !string.IsNullOrWhiteSpace(effect.Condition)
                    || !string.IsNullOrWhiteSpace(effect.ConditionDescription)
                    || !string.IsNullOrWhiteSpace(effect.Description)
                    || effect.EnableQuickRoll
                    || !string.IsNullOrWhiteSpace(effect.DiceExpression));
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
                if (HasContent(effect))
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

    [Serializable]
    internal sealed class CharacterDiceRollHistorySaveData
    {
        public string EntryId = string.Empty;
        public string CreatedAt = string.Empty;
        public string SourceItemInstanceId = string.Empty;
        public string SourceItemName = string.Empty;
        public string SourceEffectName = string.Empty;
        public string DiceExpression = string.Empty;
        public string Purpose = string.Empty;
        public string Summary = string.Empty;
        public int Total;
        public bool Success;
        public string Error = string.Empty;
        public bool Applied;
        public string AppliedMessage = string.Empty;

        public static CharacterDiceRollHistorySaveData Clone(CharacterDiceRollHistorySaveData source)
        {
            if (source == null)
            {
                return new CharacterDiceRollHistorySaveData();
            }

            return new CharacterDiceRollHistorySaveData
            {
                EntryId = source.EntryId?.Trim() ?? string.Empty,
                CreatedAt = source.CreatedAt?.Trim() ?? string.Empty,
                SourceItemInstanceId = source.SourceItemInstanceId?.Trim() ?? string.Empty,
                SourceItemName = source.SourceItemName?.Trim() ?? string.Empty,
                SourceEffectName = source.SourceEffectName?.Trim() ?? string.Empty,
                DiceExpression = source.DiceExpression?.Trim() ?? string.Empty,
                Purpose = source.Purpose?.Trim() ?? string.Empty,
                Summary = source.Summary ?? string.Empty,
                Total = source.Total,
                Success = source.Success,
                Error = source.Error ?? string.Empty,
                Applied = source.Applied,
                AppliedMessage = source.AppliedMessage ?? string.Empty
            };
        }

        public static List<CharacterDiceRollHistorySaveData> CloneList(List<CharacterDiceRollHistorySaveData> source, int maxCount = 20)
        {
            List<CharacterDiceRollHistorySaveData> result = new List<CharacterDiceRollHistorySaveData>();
            if (source == null || maxCount <= 0)
            {
                return result;
            }

            for (int index = 0; index < source.Count && result.Count < maxCount; index++)
            {
                CharacterDiceRollHistorySaveData entry = Clone(source[index]);
                if (HasContent(entry))
                {
                    result.Add(entry);
                }
            }

            return result;
        }

        private static bool HasContent(CharacterDiceRollHistorySaveData entry)
        {
            return entry != null
                && (!string.IsNullOrWhiteSpace(entry.EntryId)
                    || !string.IsNullOrWhiteSpace(entry.SourceItemName)
                    || !string.IsNullOrWhiteSpace(entry.SourceEffectName)
                    || !string.IsNullOrWhiteSpace(entry.DiceExpression)
                    || !string.IsNullOrWhiteSpace(entry.Summary)
                    || !string.IsNullOrWhiteSpace(entry.Error));
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
                CurrentHp = CharacterCreationCalculationService.Instance.NormalizeCurrentHp(source.CurrentHp, source.MaxHp),
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
            character.CurrentHp = CharacterCreationCalculationService.Instance.NormalizeCurrentHp(character.CurrentHp, character.MaxHp);
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
            character.DiceRollHistory = CharacterDiceRollHistorySaveData.CloneList(character.DiceRollHistory);
            character.CustomFeatures = CharacterCustomFeatureSaveData.CloneList(character.CustomFeatures);
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
}
