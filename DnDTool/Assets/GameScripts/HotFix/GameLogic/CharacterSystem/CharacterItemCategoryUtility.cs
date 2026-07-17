using System;

namespace GameLogic
{
    internal static class CharacterItemCategoryUtility
    {
        public static bool IsInventoryStackable(CharacterEquipmentItemSaveData item)
        {
            if (item == null)
            {
                return false;
            }

            if (DndRuleContentService.Instance.TryGetItemType(item.ItemType, out DndItemTypeDefineData itemType))
            {
                return itemType.StackableByDefault;
            }

            return false;
        }

        public static bool CanStackTogether(CharacterEquipmentItemSaveData left, CharacterEquipmentItemSaveData right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            if (!IsInventoryStackable(left)
                || !IsInventoryStackable(right))
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

        public static bool IsAmmunitionItem(CharacterEquipmentItemSaveData item)
        {
            if (item == null)
            {
                return false;
            }

            return IsAmmunitionText(item.ItemType)
                || IsAmmunitionText(item.WeaponCategory)
                || ContainsAmmunitionProperty(item.WeaponProperties)
                || IsAmmunitionText(item.ItemId)
                || IsAmmunitionText(item.SourceItemId)
                || IsAmmunitionText(item.ItemName);
        }

        public static bool IsCurrencyItem(CharacterEquipmentItemSaveData item)
        {
            if (item == null)
            {
                return false;
            }

            return TryGetCurrencyKind(item, out _);
        }

        public static bool TryGetCurrencyKind(CharacterEquipmentItemSaveData item, out CharacterCurrencyKind kind)
        {
            kind = CharacterCurrencyKind.None;
            if (item == null)
            {
                return false;
            }

            return TryGetCurrencyKindFromText(item.ItemType, out kind)
                || TryGetCurrencyKindFromText(item.ItemId, out kind)
                || TryGetCurrencyKindFromText(item.SourceItemId, out kind)
                || TryGetCurrencyKindFromText(item.ItemName, out kind);
        }

        public static int AddCurrency(CharacterCurrencySaveData currency, CharacterEquipmentItemSaveData item, int quantity)
        {
            if (currency == null || !TryGetCurrencyKind(item, out CharacterCurrencyKind kind))
            {
                return 0;
            }

            int amount = Math.Max(1, quantity > 0 ? quantity : item?.Quantity ?? 1);
            switch (kind)
            {
                case CharacterCurrencyKind.Copper:
                    currency.Copper = Math.Max(0, currency.Copper + amount);
                    break;
                case CharacterCurrencyKind.Silver:
                    currency.Silver = Math.Max(0, currency.Silver + amount);
                    break;
                case CharacterCurrencyKind.Electrum:
                    currency.Electrum = Math.Max(0, currency.Electrum + amount);
                    break;
                case CharacterCurrencyKind.Gold:
                    currency.Gold = Math.Max(0, currency.Gold + amount);
                    break;
                case CharacterCurrencyKind.Platinum:
                    currency.Platinum = Math.Max(0, currency.Platinum + amount);
                    break;
                default:
                    return 0;
            }

            return amount;
        }

        private static bool ContainsAmmunitionProperty(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string[] parts = value.Split(new[] { ';', ',', '|', '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (int index = 0; index < parts.Length; index++)
            {
                if (IsAmmunitionText(parts[index]))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsAmmunitionText(string value)
        {
            string normalized = NormalizeToken(value);
            return normalized == "ammunition"
                || normalized == "ammo"
                || normalized == "arrow"
                || normalized == "arrows"
                || normalized == "bolt"
                || normalized == "bolts"
                || normalized == "bullet"
                || normalized == "bullets"
                || normalized.Contains("ammunition")
                || normalized.Contains("ammo")
                || normalized.Contains("弹药")
                || normalized.Contains("箭矢")
                || normalized.Contains("弩矢")
                || normalized.Contains("子弹");
        }

        private static bool TryGetCurrencyKindFromText(string value, out CharacterCurrencyKind kind)
        {
            kind = CharacterCurrencyKind.None;
            string normalized = NormalizeToken(value);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return false;
            }

            if (normalized == "cp" || normalized == "copper" || normalized == "copperpiece" || normalized.Contains("铜币"))
            {
                kind = CharacterCurrencyKind.Copper;
                return true;
            }

            if (normalized == "sp" || normalized == "silver" || normalized == "silverpiece" || normalized.Contains("银币"))
            {
                kind = CharacterCurrencyKind.Silver;
                return true;
            }

            if (normalized == "ep" || normalized == "electrum" || normalized == "electrumpiece" || normalized.Contains("琥珀金币"))
            {
                kind = CharacterCurrencyKind.Electrum;
                return true;
            }

            if (normalized == "pp" || normalized == "platinum" || normalized == "platinumpiece" || normalized.Contains("铂金币"))
            {
                kind = CharacterCurrencyKind.Platinum;
                return true;
            }

            if (normalized == "gp" || normalized == "gold" || normalized == "goldpiece" || normalized.Contains("金币"))
            {
                kind = CharacterCurrencyKind.Gold;
                return true;
            }

            return false;
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

        private static string NormalizeToken(string value)
        {
            return (value ?? string.Empty)
                .Trim()
                .Replace("_", string.Empty)
                .Replace("-", string.Empty)
                .Replace(" ", string.Empty)
                .ToLowerInvariant();
        }
    }

    internal enum CharacterCurrencyKind
    {
        None,
        Copper,
        Silver,
        Electrum,
        Gold,
        Platinum
    }
}
