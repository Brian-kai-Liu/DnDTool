using System;

namespace GameLogic
{
    internal enum CharacterItemEffectConditionKind
    {
        Automatic,
        Manual,
        Unknown
    }

    internal static class CharacterItemEffectConditionUtility
    {
        public static CharacterItemEffectConditionKind GetKind(string condition)
        {
            string normalized = NormalizeCondition(condition);
            if (string.IsNullOrEmpty(normalized)
                || string.Equals(normalized, "Always", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "Equipped", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "Worn", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "Wielded", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "WearingArmor", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "Attuned", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "EquippedAndAttuned", StringComparison.OrdinalIgnoreCase))
            {
                return CharacterItemEffectConditionKind.Automatic;
            }

            if (string.Equals(normalized, "Manual", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "DMJudgement", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "DMJudgment", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "Situational", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "Activated", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "UseToApply", StringComparison.OrdinalIgnoreCase))
            {
                return CharacterItemEffectConditionKind.Manual;
            }

            return CharacterItemEffectConditionKind.Unknown;
        }

        public static bool IsAutomatic(string condition)
        {
            return GetKind(condition) == CharacterItemEffectConditionKind.Automatic;
        }

        public static bool IsManualOrUnknown(string condition)
        {
            CharacterItemEffectConditionKind kind = GetKind(condition);
            return kind == CharacterItemEffectConditionKind.Manual
                || kind == CharacterItemEffectConditionKind.Unknown;
        }

        public static bool IsMet(
            string condition,
            CharacterRuntimeSnapshotData snapshot,
            CharacterEquipmentItemSaveData item = null,
            bool equippedContext = false)
        {
            string normalized = NormalizeCondition(condition);
            if (string.IsNullOrEmpty(normalized)
                || string.Equals(normalized, "Always", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(normalized, "Equipped", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "Worn", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "Wielded", StringComparison.OrdinalIgnoreCase))
            {
                return equippedContext || item?.IsEquipped == true;
            }

            if (string.Equals(normalized, "WearingArmor", StringComparison.OrdinalIgnoreCase))
            {
                return snapshot != null
                    && !string.Equals(
                        CharacterArmorCategoryIds.Normalize(snapshot.ArmorCategory),
                        CharacterArmorCategoryIds.None,
                        StringComparison.OrdinalIgnoreCase);
            }

            if (string.Equals(normalized, "Attuned", StringComparison.OrdinalIgnoreCase))
            {
                return item?.IsAttuned == true;
            }

            if (string.Equals(normalized, "EquippedAndAttuned", StringComparison.OrdinalIgnoreCase))
            {
                return (equippedContext || item?.IsEquipped == true) && item?.IsAttuned == true;
            }

            return false;
        }

        private static string NormalizeCondition(string condition)
        {
            return condition?.Trim() ?? string.Empty;
        }
    }
}
