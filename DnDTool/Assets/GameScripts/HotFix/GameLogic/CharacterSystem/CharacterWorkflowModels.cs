using System;
using System.Collections.Generic;

namespace GameLogic
{
    internal enum AbilityKind
    {
        Strength,
        Dexterity,
        Constitution,
        Intelligence,
        Wisdom,
        Charisma
    }

    internal enum CharacterWorkflowMode
    {
        Create,
        Edit,
        View
    }

    internal sealed class CharacterOperationResult
    {
        public bool Success { get; private set; }
        public string Message { get; private set; } = string.Empty;

        public static CharacterOperationResult Ok(string message = "")
        {
            return new CharacterOperationResult
            {
                Success = true,
                Message = message ?? string.Empty
            };
        }

        public static CharacterOperationResult Fail(string message)
        {
            return new CharacterOperationResult
            {
                Success = false,
                Message = message ?? string.Empty
            };
        }
    }

    internal sealed class CharacterDraftState
    {
        public CharacterWorkflowMode Mode { get; set; } = CharacterWorkflowMode.Create;
        public CharacterCardDraftSaveData Character { get; set; } = new CharacterCardDraftSaveData();
        public bool IsDirty { get; set; }
    }

    internal sealed class CharacterDraftSaveRequest
    {
        public string CharacterName { get; set; } = string.Empty;
        public string RaceId { get; set; } = string.Empty;
        public string ClassId { get; set; } = string.Empty;
        public string BackgroundId { get; set; } = string.Empty;
        public string AlignmentId { get; set; } = string.Empty;
        public string PreviewImagePath { get; set; } = string.Empty;
        public int Level { get; set; } = 1;
        public CharacterRuntimeSnapshotData RuntimeSnapshot { get; set; } = new CharacterRuntimeSnapshotData();
        public CharacterEquipmentSetSaveData Equipment { get; set; } = new CharacterEquipmentSetSaveData();
        public CharacterRoleplayProfileSaveData RoleplayProfile { get; set; } = new CharacterRoleplayProfileSaveData();
        public List<CharacterClassProgressSaveData> ClassProgresses { get; set; } = new List<CharacterClassProgressSaveData>();
        public List<CharacterChoiceSelectionSaveData> ChoiceSelections { get; set; } = new List<CharacterChoiceSelectionSaveData>();
    }

    internal sealed class CharacterCreationDraftInput
    {
        public string CharacterName { get; set; } = string.Empty;
        public string RaceId { get; set; } = string.Empty;
        public string ClassId { get; set; } = string.Empty;
        public string SubclassId { get; set; } = string.Empty;
        public string BackgroundId { get; set; } = string.Empty;
        public string AlignmentId { get; set; } = string.Empty;
        public string PreviewImagePath { get; set; } = string.Empty;
        public int Level { get; set; } = 1;
        public int Speed { get; set; }
        public int Strength { get; set; } = 10;
        public int Dexterity { get; set; } = 10;
        public int Constitution { get; set; } = 10;
        public int Intelligence { get; set; } = 10;
        public int Wisdom { get; set; } = 10;
        public int Charisma { get; set; } = 10;
        public string HpModeId { get; set; } = CharacterHpModeIds.Custom;
        public int MaxHp { get; set; }
        public int CurrentHp { get; set; } = -1;
        public int TemporaryHp { get; set; }
        public string PersonalityTraits { get; set; } = string.Empty;
        public string Ideals { get; set; } = string.Empty;
        public string Bonds { get; set; } = string.Empty;
        public string Flaws { get; set; } = string.Empty;
        public List<CharacterHpRollSaveData> HpRolls { get; set; } = new List<CharacterHpRollSaveData>();
        public List<string> SkillProficiencyIds { get; set; } = new List<string>();
        public List<string> ToolProficiencyIds { get; set; } = new List<string>();
        public List<CharacterChoiceSelectionSaveData> ChoiceSelections { get; set; } = new List<CharacterChoiceSelectionSaveData>();
        public List<CharacterCreationRaceAbilityChoiceInput> RaceAbilityChoices { get; set; } = new List<CharacterCreationRaceAbilityChoiceInput>();
        public List<CharacterCreationSkillChoiceInput> SkillChoices { get; set; } = new List<CharacterCreationSkillChoiceInput>();
        public List<CharacterCreationToolChoiceInput> ToolChoices { get; set; } = new List<CharacterCreationToolChoiceInput>();
        public List<CharacterCreationFeatureChoiceInput> FeatureChoices { get; set; } = new List<CharacterCreationFeatureChoiceInput>();
        public CharacterEquipmentSetSaveData Equipment { get; set; } = new CharacterEquipmentSetSaveData();
        public CharacterSpellcastingSaveData Spellcasting { get; set; } = new CharacterSpellcastingSaveData();
    }

    internal sealed class CharacterCreationFormInput
    {
        public string CharacterName { get; set; } = string.Empty;
        public string RaceId { get; set; } = string.Empty;
        public string ClassId { get; set; } = string.Empty;
        public string BackgroundId { get; set; } = string.Empty;
        public string AlignmentId { get; set; } = string.Empty;
        public int Level { get; set; } = 1;
        public int Speed { get; set; }
        public int BaseAbilityScore { get; set; } = 10;
        public List<string> FixedSkillProficiencyIds { get; set; } = new List<string>();
        public List<string> FixedToolProficiencyIds { get; set; } = new List<string>();
    }

    internal sealed class CharacterCreationRaceAbilityChoiceInput
    {
        public string ChoiceGroupId { get; set; } = string.Empty;
        public string OptionId { get; set; } = string.Empty;
        public string SourceId { get; set; } = string.Empty;
        public int Count { get; set; } = 1;
    }

    internal sealed class CharacterCreationSkillChoiceInput
    {
        public string ChoiceGroupId { get; set; } = string.Empty;
        public string SkillId { get; set; } = string.Empty;
        public string SourceType { get; set; } = string.Empty;
        public string SourceId { get; set; } = string.Empty;
        public int Level { get; set; } = 1;
    }

    internal sealed class CharacterCreationToolChoiceInput
    {
        public string ChoiceGroupId { get; set; } = string.Empty;
        public string OptionId { get; set; } = string.Empty;
        public string SourceType { get; set; } = string.Empty;
        public string SourceId { get; set; } = string.Empty;
        public string ClassId { get; set; } = string.Empty;
        public int Level { get; set; } = 1;
    }

    internal sealed class CharacterCreationFeatureChoiceInput
    {
        public string ChoiceGroupId { get; set; } = string.Empty;
        public string OptionId { get; set; } = string.Empty;
        public string SourceType { get; set; } = string.Empty;
        public string SourceId { get; set; } = string.Empty;
        public string ClassId { get; set; } = string.Empty;
        public int Level { get; set; } = 1;
    }

    internal sealed class CharacterCreationSkillChoiceState
    {
        public string ChoiceGroupId = string.Empty;
        public string SourceType = string.Empty;
        public string SourceId = string.Empty;
        public int MaxSelect;
        public readonly List<string> OptionSkillIds = new List<string>();
        public readonly List<string> CandidateSkillIds = new List<string>();
        public readonly List<string> SelectedSkillIds = new List<string>();
    }

    internal class CharacterCreationToolChoiceState
    {
        public string ChoiceGroupId = string.Empty;
        public string Label = string.Empty;
        public string SourceType = string.Empty;
        public string SourceId = string.Empty;
        public int MaxSelect = 1;
        public readonly List<string> OptionToolIds = new List<string>();
        public readonly List<string> PendingToolIds = new List<string>();
        public readonly List<string> SelectedToolIds = new List<string>();
        public readonly Dictionary<string, string> OptionIdByToolId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public bool IsCompleted => MaxSelect <= 0 ? SelectedToolIds.Count > 0 : SelectedToolIds.Count >= MaxSelect;
    }

    internal sealed class CharacterCreationMixedToolChoiceState : CharacterCreationToolChoiceState
    {
        public CharacterCreationMixedProficiencyChoiceState MixedState;
    }

    internal sealed class CharacterCreationMixedProficiencyChoiceState
    {
        public string ChoiceGroupId = string.Empty;
        public string Label = string.Empty;
        public string SourceType = string.Empty;
        public string SourceId = string.Empty;
        public int MaxSelect = 1;
        public readonly List<string> OptionSkillIds = new List<string>();
        public readonly List<string> CandidateSkillIds = new List<string>();
        public readonly List<string> OptionToolIds = new List<string>();
        public readonly List<string> SelectedSkillIds = new List<string>();
        public readonly List<string> PendingToolIds = new List<string>();
        public readonly List<string> SelectedToolIds = new List<string>();
        public readonly Dictionary<string, string> OptionIdBySkillId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public readonly Dictionary<string, string> OptionIdByToolId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public int SelectedCount => SelectedSkillIds.Count + SelectedToolIds.Count;
        public int PendingCount => SelectedSkillIds.Count + SelectedToolIds.Count + PendingToolIds.Count;
        public bool IsCompleted => MaxSelect <= 0 ? SelectedCount > 0 : SelectedCount >= MaxSelect;
    }

    internal sealed class CharacterCreationFeatureChoiceState
    {
        public string ChoiceGroupId = string.Empty;
        public string ChoiceType = string.Empty;
        public string DisplayLabel = string.Empty;
        public string SourceType = string.Empty;
        public string SourceId = string.Empty;
        public string ClassId = string.Empty;
        public int Level;
        public int MinSelect;
        public int MaxSelect = 1;
        public readonly List<string> OptionIds = new List<string>();
        public readonly List<string> PendingOptionIds = new List<string>();
        public readonly List<string> SelectedOptionIds = new List<string>();
        public readonly Dictionary<string, string> OptionDisplayNameById = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public bool IsConfirmed;
    }

    internal sealed class CharacterCreationRaceAbilityChoiceState
    {
        public readonly Dictionary<string, int> FixedAbilityBonuses = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        public readonly Dictionary<string, int> SelectedAbilityBonuses = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        public readonly Dictionary<string, string> OptionIdByAbility = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public string ChoiceGroupId = string.Empty;
        public string SelectionMode = string.Empty;
        public string SourceRaceId = string.Empty;
        public int MaxSelect;
    }

    internal sealed class CharacterCreationAbilityGenerationState
    {
        public string MethodId = string.Empty;
        public string PendingScoreId = string.Empty;
        public readonly List<CharacterCreationGeneratedAbilityScoreState> Scores = new List<CharacterCreationGeneratedAbilityScoreState>();
        public readonly Dictionary<string, int> PointBuyScores = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        public readonly Dictionary<string, int> ManualScores = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    }

    internal sealed class CharacterCreationSpellSelectionState
    {
        public string PendingSpellId = string.Empty;
        public int FilterLevel = -1;
    }

    internal sealed class CharacterCreationGeneratedAbilityScoreState
    {
        public string ScoreId = string.Empty;
        public int Value;
        public string AssignedAbilityId = string.Empty;
        public bool IsAssigned => !string.IsNullOrWhiteSpace(AssignedAbilityId);
    }

    internal sealed class CharacterCreationAbilityGenerationMethodViewState
    {
        public string MethodId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    internal sealed class CharacterCreationGeneratedAbilityScoreViewState
    {
        public string ScoreId { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
        public bool IsAssigned { get; set; }
    }

    internal sealed class CharacterCreationHitPointGenerationMethodViewState
    {
        public string MethodId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    internal sealed class CharacterCreationSkillChoiceSource
    {
        public string SourceType { get; set; } = string.Empty;
        public string SourceId { get; set; } = string.Empty;
        public List<string> ChoiceGroupIds { get; set; } = new List<string>();
    }

    internal sealed class CharacterCreationFeatureChoiceSource
    {
        public string SourceType { get; set; } = string.Empty;
        public string SourceId { get; set; } = string.Empty;
        public int Level { get; set; }
        public List<string> ChoiceGroupIds { get; set; } = new List<string>();
    }

    internal sealed class CharacterCreationFeatureDisplayEntry
    {
        public readonly string FeatureId;
        public readonly string Title;
        public readonly string Description;
        public readonly string SourceType;
        public readonly string SourceId;
        public readonly int Level;
        public readonly List<string> ChoiceGroupIds = new List<string>();
        public bool IsChoiceOptionDisplay;
        public string ChoiceGroupId = string.Empty;
        public string ChoiceOptionId = string.Empty;

        public CharacterCreationFeatureDisplayEntry(string featureId, string title, string description, string sourceType, string sourceId, int level)
        {
            FeatureId = featureId ?? string.Empty;
            Title = title ?? string.Empty;
            Description = description ?? string.Empty;
            SourceType = sourceType ?? string.Empty;
            SourceId = sourceId ?? string.Empty;
            Level = level;
        }
    }

    internal sealed class CharacterCreationEquipmentToolDisplayState
    {
        public List<string> Labels { get; } = new List<string>();
        public Dictionary<string, string> ChoiceGroupIdByLabel { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> ChoiceSourceTypeByLabel { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> ChoiceSourceIdByLabel { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    internal sealed class CharacterCreationOptionViewState
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    internal sealed class CharacterCreationViewState
    {
        public string ClassSummary { get; set; } = string.Empty;
        public string RaceSummary { get; set; } = string.Empty;
        public string BackgroundSummary { get; set; } = string.Empty;
        public string AlignmentSummary { get; set; } = string.Empty;
        public string HitDiceCountText { get; set; } = string.Empty;
        public string HitDiceDieText { get; set; } = string.Empty;
        public string CurrentHpText { get; set; } = string.Empty;
        public string MaxHpText { get; set; } = string.Empty;
        public string TemporaryHpText { get; set; } = string.Empty;
        public bool ShouldShowHitPointGenerationButton { get; set; } = true;
        public string CopperText { get; set; } = string.Empty;
        public string SilverText { get; set; } = string.Empty;
        public string ElectrumText { get; set; } = string.Empty;
        public string GoldText { get; set; } = string.Empty;
        public string PlatinumText { get; set; } = string.Empty;
        public string SpeedText { get; set; } = string.Empty;
        public string ArmorClassText { get; set; } = string.Empty;
        public string InitiativeText { get; set; } = string.Empty;
        public string ProficiencyBonusText { get; set; } = string.Empty;
        public string PassivePerceptionText { get; set; } = string.Empty;
        public string SpellSaveDcText { get; set; } = string.Empty;
        public string SpellAttackBonusText { get; set; } = string.Empty;
        public string SkillsSummary { get; set; } = string.Empty;
        public string EquipmentToolsSummary { get; set; } = string.Empty;
        public string ClassFeatureClassName { get; set; } = string.Empty;
        public string ClassFeatureSubclassName { get; set; } = string.Empty;
        public string ClassLevelText { get; set; } = string.Empty;
        public string RaceFeatureRaceName { get; set; } = string.Empty;
        public string RaceFeatureSubRaceName { get; set; } = string.Empty;
        public CharacterExperienceDisplayViewState Experience { get; set; } = new CharacterExperienceDisplayViewState();
        public List<CharacterCreationAbilityViewState> Abilities { get; } = new List<CharacterCreationAbilityViewState>();
        public List<CharacterCreationSkillViewState> Skills { get; } = new List<CharacterCreationSkillViewState>();
        public List<CharacterCreationGeneratedAbilityScoreViewState> AbilityScoreOptions { get; } = new List<CharacterCreationGeneratedAbilityScoreViewState>();
        public CharacterCreationEquipmentToolDisplayState EquipmentTools { get; set; } = new CharacterCreationEquipmentToolDisplayState();
        public List<CharacterCreationFeatureDisplayEntry> ClassFeatures { get; } = new List<CharacterCreationFeatureDisplayEntry>();
        public List<CharacterCreationFeatureDisplayEntry> RaceFeatures { get; } = new List<CharacterCreationFeatureDisplayEntry>();
        public List<CharacterCreationSpellCardViewState> LearnedSpells { get; } = new List<CharacterCreationSpellCardViewState>();
        public List<CharacterStatusEffectDisplayEntry> StatusEffects { get; } = new List<CharacterStatusEffectDisplayEntry>();
    }

    internal sealed class CharacterCreationSpellCardViewState
    {
        public string SpellId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LevelText { get; set; } = string.Empty;
        public string SchoolText { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
        public bool IsKnown { get; set; }
        public bool IsPrepared { get; set; }
        public bool IsAlwaysPrepared { get; set; }
    }

    internal sealed class CharacterCreationSpellbookViewState
    {
        public int FilterLevel { get; set; } = -1;
        public int MaxSpellLevel { get; set; }
        public int KnownCantrips { get; set; }
        public int MaxKnownCantrips { get; set; }
        public int KnownSpells { get; set; }
        public int MaxKnownSpells { get; set; }
        public int PreparedSpells { get; set; }
        public int MaxPreparedSpells { get; set; }
        public string SummaryText { get; set; } = string.Empty;
        public List<CharacterCreationSpellCardViewState> AvailableSpells { get; } = new List<CharacterCreationSpellCardViewState>();
        public List<CharacterCreationSpellCardViewState> LearnedSpells { get; } = new List<CharacterCreationSpellCardViewState>();
    }

    internal sealed class CharacterCreationAbilityViewState
    {
        public string AbilityId { get; set; } = string.Empty;
        public int Score { get; set; }
        public string ModifierText { get; set; } = string.Empty;
        public bool CanIncrease { get; set; }
        public bool CanDecrease { get; set; }
        public bool CanManualInput { get; set; }
    }

    internal sealed class CharacterCreationSkillViewState
    {
        public string SkillId { get; set; } = string.Empty;
        public int Bonus { get; set; }
        public string BonusText { get; set; } = string.Empty;
        public bool HasProficiency { get; set; }
        public bool IsChoiceCandidate { get; set; }
    }

    internal sealed class CharacterListItemViewState
    {
        public string CharacterId { get; set; } = string.Empty;
        public string CharacterName { get; set; } = string.Empty;
        public string RaceId { get; set; } = string.Empty;
        public string ClassId { get; set; } = string.Empty;
        public string BackgroundId { get; set; } = string.Empty;
        public string AlignmentId { get; set; } = string.Empty;
        public int Level { get; set; } = 1;
        public bool IsCompleted { get; set; }
        public string ClassLine { get; set; } = string.Empty;
        public string StatusLine { get; set; } = string.Empty;
        public CharacterRuntimeSnapshotData RuntimeSnapshot { get; set; } = new CharacterRuntimeSnapshotData();
    }

    internal sealed class CharacterLibraryViewState
    {
        public List<CharacterCardDraftSaveData> Characters { get; } = new List<CharacterCardDraftSaveData>();
        public List<CharacterListItemViewState> Items { get; } = new List<CharacterListItemViewState>();
    }

    internal sealed class CharacterDetailViewState
    {
        public CharacterCardDraftSaveData Character { get; set; } = new CharacterCardDraftSaveData();
        public CharacterRuntimeSnapshotData RuntimeSnapshot { get; set; } = new CharacterRuntimeSnapshotData();
        public List<CharacterClassProgressSaveData> ClassProgresses { get; } = new List<CharacterClassProgressSaveData>();
        public List<CharacterChoiceSelectionSaveData> ChoiceSelections { get; } = new List<CharacterChoiceSelectionSaveData>();
    }

    internal sealed class CharacterCombatOverviewViewState
    {
        public int ArmorClass { get; set; }
        public int InitiativeBonus { get; set; }
        public int Speed { get; set; }
        public int PassivePerception { get; set; } = 10;
        public bool HasSpellcasting { get; set; }
        public int SpellSaveDc { get; set; }
        public int SpellAttackBonus { get; set; }
    }

    internal sealed class CharacterAbilityDisplayViewState
    {
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Constitution { get; set; }
        public int Intelligence { get; set; }
        public int Wisdom { get; set; }
        public int Charisma { get; set; }
        public string StrengthModifier { get; set; } = string.Empty;
        public string DexterityModifier { get; set; } = string.Empty;
        public string ConstitutionModifier { get; set; } = string.Empty;
        public string IntelligenceModifier { get; set; } = string.Empty;
        public string WisdomModifier { get; set; } = string.Empty;
        public string CharismaModifier { get; set; } = string.Empty;
    }

    internal sealed class CharacterHpDisplayViewState
    {
        public int CurrentHp { get; set; }
        public int MaxHp { get; set; }
        public int TemporaryHp { get; set; }
    }

    internal sealed class CharacterExperienceDisplayViewState
    {
        public int Experience { get; set; }
        public int NextLevelExperience { get; set; }
        public float Progress { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    internal sealed class CharacterProficiencyBonusDisplayViewState
    {
        public int Bonus { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    internal sealed class CharacterSkillBonusViewState
    {
        public int Bonus { get; set; }
        public bool HasProficiency { get; set; }
        public bool HasExpertise { get; set; }
    }

    internal sealed class CharacterInventoryOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public CharacterEquipmentSetSaveData Equipment { get; set; } = new CharacterEquipmentSetSaveData();
        public string ItemInstanceId { get; set; } = string.Empty;
    }

    internal readonly struct CharacterInventoryDisplayEntry
    {
        public readonly string ItemInstanceId;
        public readonly string Label;
        public readonly string Title;
        public readonly string Description;
        public readonly bool IsEquipped;

        public CharacterInventoryDisplayEntry(string itemInstanceId, string label, string title, string description, bool isEquipped)
        {
            ItemInstanceId = itemInstanceId?.Trim() ?? string.Empty;
            Label = label ?? string.Empty;
            Title = title ?? string.Empty;
            Description = description ?? string.Empty;
            IsEquipped = isEquipped;
        }
    }

    internal readonly struct CharacterStatusEffectDisplayEntry
    {
        public readonly string Name;
        public readonly string Duration;
        public readonly string IconPath;

        public CharacterStatusEffectDisplayEntry(string name, string duration, string iconPath = "")
        {
            Name = name ?? string.Empty;
            Duration = duration ?? string.Empty;
            IconPath = iconPath ?? string.Empty;
        }
    }

    internal readonly struct ClassFeatureDisplayEntry
    {
        public readonly string Title;
        public readonly string Description;

        public ClassFeatureDisplayEntry(string title, string description)
        {
            Title = title ?? string.Empty;
            Description = description ?? string.Empty;
        }
    }

    internal sealed class CharacterClassDetailDisplayState
    {
        public string ClassName { get; set; } = string.Empty;
        public string SubclassName { get; set; } = string.Empty;
        public string LevelText { get; set; } = string.Empty;
        public List<ClassFeatureDisplayEntry> Features { get; } = new List<ClassFeatureDisplayEntry>();
    }

    internal sealed class CharacterRaceFeatureHeaderState
    {
        public string MainRaceName { get; set; } = string.Empty;
        public string SubRaceName { get; set; } = string.Empty;
    }

    internal sealed class ItemEditorRuleItemViewState
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public string ArmorCategory { get; set; } = string.Empty;
        public int ArmorBaseAc { get; set; }
        public int AcBonus { get; set; }
        public bool DefaultEquipped { get; set; }
        public bool RequiresAttunement { get; set; }
    }

    internal sealed class ItemEditorCharacterPickerEntry
    {
        public string CharacterId { get; set; } = string.Empty;
        public string CharacterName { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
    }

    internal sealed class ItemEditorAddItemResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string CharacterName { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
    }
}
