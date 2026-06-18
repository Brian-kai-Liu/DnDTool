using System;
using System.Collections.Generic;

namespace GameLogic
{
    internal enum DndPrimaryAbilityMode
    {
        Fixed,
        AnyOne,
        All,
        Manual
    }

    internal sealed class DndRuleContentLibraryData
    {
        public List<DndRulePackageData> RulePackages { get; } = new List<DndRulePackageData>();

        public List<DndClassDefineData> Classes { get; } = new List<DndClassDefineData>();

        public List<DndLevelProgressionData> LevelProgressions { get; } = new List<DndLevelProgressionData>();

        public List<DndSubclassLevelProgressionData> SubclassLevelProgressions { get; } = new List<DndSubclassLevelProgressionData>();

        public List<DndFeatureDefineData> Features { get; } = new List<DndFeatureDefineData>();

        public List<DndFeatureEffectData> FeatureEffects { get; } = new List<DndFeatureEffectData>();

        public List<DndFeatureEffectConditionData> FeatureEffectConditions { get; } = new List<DndFeatureEffectConditionData>();

        public List<DndChoiceGroupData> ChoiceGroups { get; } = new List<DndChoiceGroupData>();

        public List<DndChoiceOptionData> ChoiceOptions { get; } = new List<DndChoiceOptionData>();

        public List<DndSkillDefineData> Skills { get; } = new List<DndSkillDefineData>();

        public List<DndRaceMainDefineData> RaceMains { get; } = new List<DndRaceMainDefineData>();

        public List<DndRaceSubDefineData> RaceSubs { get; } = new List<DndRaceSubDefineData>();

        public List<DndRaceDefineData> Races { get; } = new List<DndRaceDefineData>();

        public List<DndBackgroundDefineData> Backgrounds { get; } = new List<DndBackgroundDefineData>();

        public List<DndFeatDefineData> Feats { get; } = new List<DndFeatDefineData>();

        public List<DndSpellDefineData> Spells { get; } = new List<DndSpellDefineData>();

        public List<DndClassSpellListData> ClassSpellLists { get; } = new List<DndClassSpellListData>();

        public List<DndItemDefineData> Items { get; } = new List<DndItemDefineData>();

        public List<DndToolDefineData> Tools { get; } = new List<DndToolDefineData>();

        public List<DndLanguageDefineData> Languages { get; } = new List<DndLanguageDefineData>();

        public List<DndEnumListData> EnumLists { get; } = new List<DndEnumListData>();

        public List<DndAlignmentData> Alignments { get; } = new List<DndAlignmentData>();
    }

    internal sealed class DndRulePackageData
    {
        public string PackageId { get; set; } = string.Empty;

        public string PackageName { get; set; } = string.Empty;

        public string Version { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public string License { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string GetDisplayLabel()
        {
            if (string.IsNullOrWhiteSpace(Version))
            {
                return string.IsNullOrWhiteSpace(PackageName) ? PackageId : PackageName;
            }

            return $"{(string.IsNullOrWhiteSpace(PackageName) ? PackageId : PackageName)} ({Version})";
        }
    }

    internal sealed class DndClassDefineData
    {
        public string ClassId { get; set; } = string.Empty;

        public string PackageId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int HitDie { get; set; }

        public List<string> PrimaryAbilityIds { get; } = new List<string>();

        public DndPrimaryAbilityMode PrimaryAbilityMode { get; set; } = DndPrimaryAbilityMode.Fixed;

        public List<string> SavingThrowProficiencies { get; } = new List<string>();

        public List<string> ArmorProficiencies { get; } = new List<string>();

        public List<string> WeaponProficiencies { get; } = new List<string>();

        public List<string> ToolProficiencies { get; } = new List<string>();

        public string SpellcastingAbility { get; set; } = string.Empty;

        public string SpellSlotProgressionId { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    internal sealed class DndLevelProgressionData
    {
        public string ProgressionId { get; set; } = string.Empty;

        public string OwnerType { get; set; } = string.Empty;

        public string OwnerId { get; set; } = string.Empty;

        public string ClassId { get; set; } = string.Empty;

        public string SubclassId { get; set; } = string.Empty;

        public int Level { get; set; }

        public int ProficiencyBonus { get; set; }

        public int FixedHpGain { get; set; }

        public List<string> FeatureIds { get; } = new List<string>();

        public List<string> ChoiceGroupIds { get; } = new List<string>();

        public List<string> ResourceGrantIds { get; } = new List<string>();

        public int? SpellSlotProgressionLevel { get; set; }

        public int? CantripKnown { get; set; }

        public int? SpellKnown { get; set; }

        public string PreparedSpellFormula { get; set; } = string.Empty;

        public bool AsiAvailable { get; set; }

        public string AsiRuleId { get; set; } = string.Empty;

        public bool SubclassFeature { get; set; }

        public string Note { get; set; } = string.Empty;
    }

    internal sealed class DndSubclassLevelProgressionData
    {
        public string SubclassId { get; set; } = string.Empty;

        public string ClassId { get; set; } = string.Empty;

        public int Level { get; set; }

        public List<string> FeatureIds { get; } = new List<string>();

        public List<string> ChoiceGroupIds { get; } = new List<string>();

        public List<string> ResourceGrantIds { get; } = new List<string>();

        public int? SpellSlotProgressionLevel { get; set; }

        public int? CantripKnown { get; set; }

        public int? SpellKnown { get; set; }

        public string PreparedSpellFormula { get; set; } = string.Empty;

        public string Note { get; set; } = string.Empty;
    }

    internal sealed class DndFeatureDefineData
    {
        public string FeatureId { get; set; } = string.Empty;

        public string PackageId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string FeatureType { get; set; } = string.Empty;

        public string SourceRef { get; set; } = string.Empty;

        public List<string> PrerequisiteIds { get; } = new List<string>();

        public List<string> EffectIds { get; } = new List<string>();

        public List<string> ChoiceGroupIds { get; } = new List<string>();

        public string Description { get; set; } = string.Empty;
    }

    internal sealed class DndFeatureEffectData
    {
        public string EffectId { get; set; } = string.Empty;

        public string EffectType { get; set; } = string.Empty;

        public string Target { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public string Condition { get; set; } = string.Empty;

        public List<string> ConditionIds { get; } = new List<string>();

        public string StackingRule { get; set; } = string.Empty;

        public string ManualNote { get; set; } = string.Empty;
    }

    internal sealed class DndFeatureEffectConditionData
    {
        public string ConditionId { get; set; } = string.Empty;

        public string PackageId { get; set; } = string.Empty;

        public string ConditionType { get; set; } = string.Empty;

        public string Target { get; set; } = string.Empty;

        public string Operator { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    internal sealed class DndChoiceGroupData
    {
        public string ChoiceGroupId { get; set; } = string.Empty;

        public string PackageId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string ChoiceType { get; set; } = string.Empty;

        public int MinSelect { get; set; }

        public int MaxSelect { get; set; }

        public string OptionFilter { get; set; } = string.Empty;

        public string SelectionMode { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    internal sealed class DndChoiceOptionData
    {
        public string ChoiceGroupId { get; set; } = string.Empty;

        public string OptionId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public List<string> GrantFeatureIds { get; } = new List<string>();

        public List<string> GrantEffectIds { get; } = new List<string>();

        public string Description { get; set; } = string.Empty;
    }

    internal sealed class DndSkillDefineData
    {
        public string SkillId { get; set; } = string.Empty;

        public string PackageId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string AbilityId { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    internal sealed class DndRaceDefineData
    {
        public string RaceId { get; set; } = string.Empty;

        public string PackageId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Size { get; set; } = string.Empty;

        public int Speed { get; set; }

        public List<string> LanguageIds { get; } = new List<string>();

        public List<string> FeatureIds { get; } = new List<string>();

        public List<string> ChoiceGroupIds { get; } = new List<string>();

        public string Description { get; set; } = string.Empty;
    }

    internal sealed class DndRaceMainDefineData
    {
        public string MainRaceId { get; set; } = string.Empty;

        public string PackageId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Size { get; set; } = string.Empty;

        public int Speed { get; set; }

        public List<string> LanguageIds { get; } = new List<string>();

        public List<string> MainFeatureIds { get; } = new List<string>();

        public List<string> ChoiceGroupIds { get; } = new List<string>();

        public string Description { get; set; } = string.Empty;
    }

    internal sealed class DndRaceSubDefineData
    {
        public string SubRaceId { get; set; } = string.Empty;

        public string PackageId { get; set; } = string.Empty;

        public string MainRaceId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Size { get; set; } = string.Empty;

        public int Speed { get; set; }

        public List<string> FeatureIds { get; } = new List<string>();

        public List<string> ChoiceGroupIds { get; } = new List<string>();

        public string Description { get; set; } = string.Empty;
    }

    internal sealed class DndBackgroundDefineData
    {
        public string BackgroundId { get; set; } = string.Empty;

        public string PackageId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public List<string> SkillProficiencies { get; } = new List<string>();

        public List<string> ToolProficiencies { get; } = new List<string>();

        public List<string> LanguageIds { get; } = new List<string>();

        public List<string> FeatureIds { get; } = new List<string>();

        public List<string> EquipmentGrantIds { get; } = new List<string>();

        public string Description { get; set; } = string.Empty;
    }

    internal sealed class DndFeatDefineData
    {
        public string FeatId { get; set; } = string.Empty;

        public string PackageId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public List<string> PrerequisiteIds { get; } = new List<string>();

        public List<string> FeatureIds { get; } = new List<string>();

        public List<string> EffectIds { get; } = new List<string>();

        public List<string> ChoiceGroupIds { get; } = new List<string>();

        public string Description { get; set; } = string.Empty;
    }

    internal sealed class DndSpellDefineData
    {
        public string SpellId { get; set; } = string.Empty;

        public string PackageId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int Level { get; set; }

        public string School { get; set; } = string.Empty;

        public string CastingTime { get; set; } = string.Empty;

        public string Range { get; set; } = string.Empty;

        public string Components { get; set; } = string.Empty;

        public string Duration { get; set; } = string.Empty;

        public bool Concentration { get; set; }

        public bool Ritual { get; set; }

        public string AttackType { get; set; } = string.Empty;

        public string SaveAbility { get; set; } = string.Empty;

        public string DamageFormula { get; set; } = string.Empty;

        public string DamageType { get; set; } = string.Empty;

        public List<string> EffectTags { get; } = new List<string>();

        public string Description { get; set; } = string.Empty;

        public string HigherLevelDescription { get; set; } = string.Empty;
    }

    internal sealed class DndClassSpellListData
    {
        public string ClassId { get; set; } = string.Empty;

        public string SpellId { get; set; } = string.Empty;

        public int MinClassLevel { get; set; }

        public bool AlwaysPrepared { get; set; }

        public string SourceFeatureId { get; set; } = string.Empty;

        public string Note { get; set; } = string.Empty;
    }

    internal sealed class DndItemDefineData
    {
        public string ItemId { get; set; } = string.Empty;

        public string PackageId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string ItemType { get; set; } = string.Empty;

        public string Rarity { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string SourceBook { get; set; } = string.Empty;

        public string SourcePage { get; set; } = string.Empty;

        public bool Stackable { get; set; }

        public int MaxStack { get; set; }

        public float Weight { get; set; }

        public int PriceGp { get; set; }

        public bool IsEquippable { get; set; }

        public int DefaultQuantity { get; set; }

        public string EquipmentSlot { get; set; } = string.Empty;

        public bool RequiresAttunement { get; set; }

        public bool DefaultEquipped { get; set; }

        public string ArmorCategory { get; set; } = string.Empty;

        public int ArmorBaseAc { get; set; }

        public int AcBonus { get; set; }

        public int MaxDexBonus { get; set; }

        public int StrengthRequirement { get; set; }

        public bool StealthDisadvantage { get; set; }

        public string WeaponCategory { get; set; } = string.Empty;

        public string WeaponRangeType { get; set; } = string.Empty;

        public string DamageDice { get; set; } = string.Empty;

        public string DamageType { get; set; } = string.Empty;

        public List<string> WeaponProperties { get; } = new List<string>();

        public int NormalRange { get; set; }

        public int LongRange { get; set; }

        public string TwoHandDamageDice { get; set; } = string.Empty;

        public string ToolCategory { get; set; } = string.Empty;

        public bool Consumable { get; set; }

        public int Charges { get; set; }

        public bool ConsumeOnUse { get; set; }

        public List<string> EffectIds { get; } = new List<string>();

        public string EffectApplyCondition { get; set; } = string.Empty;
    }

    internal sealed class DndToolDefineData
    {
        public string ToolId { get; set; } = string.Empty;

        public string PackageId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string ToolCategory { get; set; } = string.Empty;

        public string EnglishName { get; set; } = string.Empty;

        public float PriceGp { get; set; }

        public float Weight { get; set; }

        public string SourceBook { get; set; } = string.Empty;

        public string SourcePage { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    internal sealed class DndLanguageDefineData
    {
        public string LanguageId { get; set; } = string.Empty;

        public string PackageId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string EnglishName { get; set; } = string.Empty;

        public string LanguageCategory { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    internal sealed class DndEnumListData
    {
        public string EnumType { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    internal sealed class DndAlignmentData
    {
        public string AlignmentId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
