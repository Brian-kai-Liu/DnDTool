using System;
using System.Collections.Generic;

namespace GameLogic
{
    internal sealed class CharacterEquipmentProficiencyDisplayService
    {
        private static readonly Lazy<CharacterEquipmentProficiencyDisplayService> s_instance =
            new Lazy<CharacterEquipmentProficiencyDisplayService>(() => new CharacterEquipmentProficiencyDisplayService());

        private static readonly Dictionary<string, string> ArmorNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["LightArmor"] = "轻甲",
            ["Light Armor"] = "轻甲",
            ["light_armor"] = "轻甲",
            ["MediumArmor"] = "中甲",
            ["Medium Armor"] = "中甲",
            ["medium_armor"] = "中甲",
            ["HeavyArmor"] = "重甲",
            ["Heavy Armor"] = "重甲",
            ["heavy_armor"] = "重甲",
            ["Shield"] = "盾牌",
            ["Shields"] = "盾牌",
            ["shield"] = "盾牌"
        };

        private static readonly Dictionary<string, string> WeaponNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["SimpleWeapon"] = "\u7b80\u6613\u6b66\u5668",
            ["Simple Weapons"] = "\u7b80\u6613\u6b66\u5668",
            ["simple_weapon"] = "\u7b80\u6613\u6b66\u5668",
            ["MartialWeapon"] = "军用武器",
            ["Martial Weapons"] = "军用武器",
            ["martial_weapon"] = "军用武器",
            ["Club"] = "短棍",
            ["Dagger"] = "匕首",
            ["Greatclub"] = "巨棒",
            ["Handaxe"] = "手斧",
            ["Javelin"] = "标枪",
            ["LightHammer"] = "轻锤",
            ["Mace"] = "\u786c\u5934\u9524",
            ["Quarterstaff"] = "长棍",
            ["Sickle"] = "镰刀",
            ["Spear"] = "\u77db",
            ["CrossbowLight"] = "轻弩",
            ["LightCrossbow"] = "轻弩",
            ["Dart"] = "飞镖",
            ["Shortbow"] = "短弓",
            ["Sling"] = "\u6295\u77f3\u7d22",
            ["Battleaxe"] = "战斧",
            ["Flail"] = "链枷",
            ["Glaive"] = "长柄刀",
            ["Greataxe"] = "巨斧",
            ["Greatsword"] = "巨剑",
            ["Halberd"] = "\u621f",
            ["Lance"] = "骑枪",
            ["Longsword"] = "长剑",
            ["Maul"] = "巨锤",
            ["Morningstar"] = "晨星",
            ["Pike"] = "刺枪",
            ["Rapier"] = "刺剑",
            ["Scimitar"] = "弯刀",
            ["Shortsword"] = "短剑",
            ["Trident"] = "\u4e09\u53c9\u621f",
            ["WarPick"] = "战镐",
            ["Warhammer"] = "战锤",
            ["Whip"] = "长鞭",
            ["Blowgun"] = "\u5439\u7bad\u7b52",
            ["CrossbowHand"] = "手弩",
            ["HandCrossbow"] = "手弩",
            ["CrossbowHeavy"] = "重弩",
            ["HeavyCrossbow"] = "重弩",
            ["Longbow"] = "长弓",
            ["Net"] = "捕网"
        };

        private static readonly Dictionary<string, string> ToolAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["smiths_tools"] = "smith_tools",
            ["brewers_supplies"] = "brewer_supplies",
            ["masons_tools"] = "mason_tools"
        };

        private CharacterEquipmentProficiencyDisplayService()
        {
        }

        public static CharacterEquipmentProficiencyDisplayService Instance => s_instance.Value;

        public string GetArmorDisplayName(string armorId)
        {
            return GetMappedDisplayName(ArmorNames, armorId);
        }

        public string GetWeaponDisplayName(string weaponId)
        {
            return GetMappedDisplayName(WeaponNames, weaponId);
        }

        public string GetToolDisplayName(string toolId)
        {
            if (string.IsNullOrWhiteSpace(toolId))
            {
                return string.Empty;
            }

            string normalized = NormalizeToolId(toolId);
            if (DndRuleContentService.Instance.TryGetTool(normalized, out DndToolDefineData tool))
            {
                return FirstNonEmpty(tool.Name, tool.ToolId);
            }

            return toolId.Trim();
        }

        public string FormatArmorLabel(string armorId)
        {
            return FormatLabel("护甲", GetArmorDisplayName(armorId));
        }

        public string FormatWeaponLabel(string weaponId)
        {
            return FormatLabel("武器", GetWeaponDisplayName(weaponId));
        }

        public string FormatToolLabel(string toolId)
        {
            return FormatLabel("工具", GetToolDisplayName(toolId));
        }

        private static string GetMappedDisplayName(Dictionary<string, string> names, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string trimmed = value.Trim();
            return names.TryGetValue(trimmed, out string displayName) ? displayName : trimmed;
        }

        private static string NormalizeToolId(string toolId)
        {
            string trimmed = toolId.Trim();
            return ToolAliases.TryGetValue(trimmed, out string normalized) ? normalized : trimmed;
        }

        private static string FormatLabel(string prefix, string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return string.Empty;
            }

            string trimmed = displayName.Trim();
            return trimmed.StartsWith($"{prefix}：", StringComparison.Ordinal)
                ? trimmed
                : $"{prefix}：{trimmed}";
        }

        private static string FirstNonEmpty(string first, string second)
        {
            return !string.IsNullOrWhiteSpace(first) ? first.Trim() : second?.Trim() ?? string.Empty;
        }
    }
}
