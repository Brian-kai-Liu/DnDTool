using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    internal sealed class ModuleAuthoringDraftState
    {
        public string DraftId { get; set; } = string.Empty;

        public ModuleBasicInfoDraftData BasicInfo { get; set; } = new ModuleBasicInfoDraftData();

        public int SelectedChapterId { get; set; } = -1;

        public int NextChapterId { get; set; } = 1;

        public List<ModuleChapterDraftData> Chapters { get; set; } = new List<ModuleChapterDraftData>();

        public bool IsDirty { get; set; }
    }

    internal sealed class ModuleBasicInfoDraftData
    {
        public string ModuleName { get; set; } = string.Empty;

        public string ModuleIntroduction { get; set; } = string.Empty;

        public string RuleVersion { get; set; } = string.Empty;

        public List<string> ExtensionPackageIds { get; set; } = new List<string>();

        public string ExtensionPackageSummary { get; set; } = string.Empty;

        public string RecommendedLevel { get; set; } = string.Empty;

        public string RecommendedPlayers { get; set; } = string.Empty;

        public string EstimatedDuration { get; set; } = string.Empty;

        public string PreviewImagePath { get; set; } = string.Empty;

        public List<ModuleAdventureHookDraftData> AdventureHooks { get; set; } = new List<ModuleAdventureHookDraftData>();
    }

    internal sealed class ModuleAdventureHookDraftData
    {
        public string HookId { get; set; } = string.Empty;

        public string Target { get; set; } = string.Empty;

        public string HookContent { get; set; } = string.Empty;
    }

    internal sealed class ModuleChapterDraftData
    {
        public int ChapterId { get; set; }

        public string ChapterName { get; set; } = string.Empty;

        public string Goal { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string DmNote { get; set; } = string.Empty;

        public string TerrainTag { get; set; } = string.Empty;

        public string TerrainSubTag { get; set; } = string.Empty;

        public string AddMapHint { get; set; } = string.Empty;

        public string CreatureInfo { get; set; } = string.Empty;

        public string MapImagePath { get; set; } = string.Empty;

        public ModuleChapterMapGridDraftData MapGrid { get; set; } = new ModuleChapterMapGridDraftData();

        public List<ModuleChapterGridCellDraftData> GridCells { get; set; } = new List<ModuleChapterGridCellDraftData>();

        public List<ModuleChapterEventDraftData> Events { get; set; } = new List<ModuleChapterEventDraftData>();

        public List<ModuleChapterEventBindingDraftData> EventBindings { get; set; } = new List<ModuleChapterEventBindingDraftData>();

        public List<ModuleChapterCreatureDraftData> CreatureTemplates { get; set; } = new List<ModuleChapterCreatureDraftData>();

        public List<ModuleChapterCreatureInstanceDraftData> CreatureInstances { get; set; } = new List<ModuleChapterCreatureInstanceDraftData>();
    }

    internal sealed class ModuleChapterMapGridDraftData
    {
        public bool IsMapZoomEnabled { get; set; }

        public bool IsGridZoomEnabled { get; set; }

        public float MapZoomScale { get; set; } = 1f;

        public Vector2 MapPanOffset { get; set; } = Vector2.zero;

        public float GridZoomScale { get; set; } = 1f;

        public Vector2 GridPanOffset { get; set; } = Vector2.zero;

        public bool IsLocked { get; set; }

        public float LockedMapZoomReference { get; set; } = 1f;

        public float LockedGridToMapZoomRatio { get; set; } = 1f;

        public Vector2 LockedGridToMapPanDelta { get; set; } = Vector2.zero;
    }

    internal sealed class ModuleChapterGridCellDraftData
    {
        public int CellX { get; set; }

        public int CellY { get; set; }

        public int MarkType { get; set; }
    }

    internal sealed class ModuleChapterEventDraftData
    {
        public string EventId { get; set; } = string.Empty;

        public bool IsEnabled { get; set; } = true;

        public bool IsOneShot { get; set; }

        public string EventTitle { get; set; } = string.Empty;

        public string TriggerDescription { get; set; } = string.Empty;

        public string DmNote { get; set; } = string.Empty;

        public ModuleChapterEventTriggerDraftData Trigger { get; set; } = new ModuleChapterEventTriggerDraftData();

        public ModuleChapterEventEffectDraftData Effect { get; set; } = new ModuleChapterEventEffectDraftData();
    }

    internal sealed class ModuleChapterEventTriggerDraftData
    {
        public int TriggerMode { get; set; }

        public int TriggerType { get; set; } = -1;

        public bool AreaFirstEnterOnly { get; set; }

        public bool AreaShareBinding { get; set; }

        public string InteractionTarget { get; set; } = string.Empty;

        public bool InteractionRequireConfirm { get; set; }

        public string PrerequisiteEventId { get; set; } = string.Empty;

        public string DelayDescription { get; set; } = string.Empty;
    }

    internal sealed class ModuleChapterEventEffectDraftData
    {
        public int EffectType { get; set; } = -1;

        public int CheckTargetMode { get; set; }

        public int CheckResolutionMode { get; set; }

        public string SuccessResult { get; set; } = string.Empty;

        public string FailureResult { get; set; } = string.Empty;

        public string NarrativeText { get; set; } = string.Empty;

        public bool NarrativeDmOnly { get; set; }

        public string DialogueTarget { get; set; } = string.Empty;

        public string DialogueSummary { get; set; } = string.Empty;

        public string DialoguePrompt { get; set; } = string.Empty;

        public string CreatureInstanceId { get; set; } = string.Empty;

        public bool CreatureActivate { get; set; } = true;

        public int CreaturePlacementMode { get; set; }

        public string BattleReference { get; set; } = string.Empty;

        public bool BattleIncludeActiveCreatures { get; set; } = true;

        public string BattleDescription { get; set; } = string.Empty;

        public List<ModuleChapterSkillCheckThresholdDraftData> SkillCheckEntries { get; set; } =
            new List<ModuleChapterSkillCheckThresholdDraftData>();
    }

    internal sealed class ModuleChapterSkillCheckThresholdDraftData
    {
        public string CheckName { get; set; } = string.Empty;

        public string Threshold { get; set; } = string.Empty;
    }

    internal sealed class ModuleChapterEventBindingDraftData
    {
        public string BindingId { get; set; } = string.Empty;

        public string EventId { get; set; } = string.Empty;

        public List<ModuleChapterGridCoordinateDraftData> GridCoordinates { get; set; } =
            new List<ModuleChapterGridCoordinateDraftData>();
    }

    internal sealed class ModuleChapterGridCoordinateDraftData
    {
        public int CellX { get; set; }

        public int CellY { get; set; }
    }

    internal sealed class ModuleChapterCreatureDraftData
    {
        public string CreatureId { get; set; } = string.Empty;

        public string CreatureName { get; set; } = string.Empty;

        public string CreatureSize { get; set; } = string.Empty;

        public string PreviewImagePath { get; set; } = string.Empty;
    }

    internal sealed class ModuleChapterCreatureInstanceDraftData
    {
        public string InstanceId { get; set; } = string.Empty;

        public string CreatureId { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ModuleChapterGridCoordinateDraftData Coordinate { get; set; } = new ModuleChapterGridCoordinateDraftData();
    }
}
