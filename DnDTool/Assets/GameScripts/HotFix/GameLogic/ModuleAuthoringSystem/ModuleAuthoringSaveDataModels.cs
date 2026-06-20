using System;
using System.Collections.Generic;

namespace GameLogic
{
    [Serializable]
    internal sealed class ModuleAuthoringSaveData
    {
        public int Version = 1;

        public string DraftId = string.Empty;

        public ModuleBasicInfoSaveData BasicInfo = new ModuleBasicInfoSaveData();

        public int SelectedChapterId = -1;

        public int NextChapterId = 1;

        public List<ModuleChapterSaveData> Chapters = new List<ModuleChapterSaveData>();
    }

    [Serializable]
    internal sealed class ModuleBasicInfoSaveData
    {
        public string ModuleName = string.Empty;

        public string ModuleIntroduction = string.Empty;

        public string RuleVersion = string.Empty;

        public List<string> ExtensionPackageIds = new List<string>();

        public string ExtensionPackageSummary = string.Empty;

        public string RecommendedLevel = string.Empty;

        public string RecommendedPlayers = string.Empty;

        public string EstimatedDuration = string.Empty;

        public string PreviewImagePath = string.Empty;

        public List<ModuleAdventureHookSaveData> AdventureHooks = new List<ModuleAdventureHookSaveData>();
    }

    [Serializable]
    internal sealed class ModuleAdventureHookSaveData
    {
        public string HookId = string.Empty;

        public string Target = string.Empty;

        public string HookContent = string.Empty;
    }

    [Serializable]
    internal sealed class ModuleChapterSaveData
    {
        public int ChapterId;

        public string ChapterName = string.Empty;

        public string Goal = string.Empty;

        public string Content = string.Empty;

        public string DmNote = string.Empty;

        public string TerrainTag = string.Empty;

        public string TerrainSubTag = string.Empty;

        public string AddMapHint = string.Empty;

        public string CreatureInfo = string.Empty;

        public string MapImagePath = string.Empty;

        public ModuleChapterMapGridSaveData MapGrid = new ModuleChapterMapGridSaveData();

        public List<ModuleChapterGridCellSaveData> GridCells = new List<ModuleChapterGridCellSaveData>();

        public List<ModuleChapterEventSaveData> Events = new List<ModuleChapterEventSaveData>();

        public List<ModuleChapterEventBindingSaveData> EventBindings = new List<ModuleChapterEventBindingSaveData>();

        public List<ModuleChapterCreatureSaveData> CreatureTemplates = new List<ModuleChapterCreatureSaveData>();

        public List<ModuleChapterCreatureInstanceSaveData> CreatureInstances = new List<ModuleChapterCreatureInstanceSaveData>();
    }

    [Serializable]
    internal sealed class ModuleChapterMapGridSaveData
    {
        public bool IsMapZoomEnabled;

        public bool IsGridZoomEnabled;

        public float MapZoomScale = 1f;

        public float MapPanOffsetX;

        public float MapPanOffsetY;

        public float GridZoomScale = 1f;

        public float GridPanOffsetX;

        public float GridPanOffsetY;

        public bool IsLocked;

        public float LockedMapZoomReference = 1f;

        public float LockedGridToMapZoomRatio = 1f;

        public float LockedGridToMapPanDeltaX;

        public float LockedGridToMapPanDeltaY;
    }

    [Serializable]
    internal sealed class ModuleChapterGridCellSaveData
    {
        public int CellX;

        public int CellY;

        public int MarkType;
    }

    [Serializable]
    internal sealed class ModuleChapterEventSaveData
    {
        public string EventId = string.Empty;

        public bool IsEnabled = true;

        public bool IsOneShot;

        public string EventTitle = string.Empty;

        public string TriggerDescription = string.Empty;

        public string DmNote = string.Empty;

        public ModuleChapterEventTriggerSaveData Trigger = new ModuleChapterEventTriggerSaveData();

        public ModuleChapterEventEffectSaveData Effect = new ModuleChapterEventEffectSaveData();
    }

    [Serializable]
    internal sealed class ModuleChapterEventTriggerSaveData
    {
        public int TriggerMode;

        public int TriggerType = -1;

        public bool AreaFirstEnterOnly;

        public bool AreaShareBinding;

        public string InteractionTarget = string.Empty;

        public bool InteractionRequireConfirm;

        public string PrerequisiteEventId = string.Empty;

        public string DelayDescription = string.Empty;
    }

    [Serializable]
    internal sealed class ModuleChapterEventEffectSaveData
    {
        public int EffectType = -1;

        public int CheckTargetMode;

        public int CheckResolutionMode;

        public string SuccessResult = string.Empty;

        public string FailureResult = string.Empty;

        public string NarrativeText = string.Empty;

        public bool NarrativeDmOnly;

        public string DialogueTarget = string.Empty;

        public string DialogueSummary = string.Empty;

        public string DialoguePrompt = string.Empty;

        public string CreatureInstanceId = string.Empty;

        public bool CreatureActivate = true;

        public int CreaturePlacementMode;

        public string BattleReference = string.Empty;

        public bool BattleIncludeActiveCreatures = true;

        public string BattleDescription = string.Empty;

        public List<ModuleChapterSkillCheckThresholdSaveData> SkillCheckEntries =
            new List<ModuleChapterSkillCheckThresholdSaveData>();
    }

    [Serializable]
    internal sealed class ModuleChapterSkillCheckThresholdSaveData
    {
        public string CheckName = string.Empty;

        public string Threshold = string.Empty;
    }

    [Serializable]
    internal sealed class ModuleChapterEventBindingSaveData
    {
        public string BindingId = string.Empty;

        public string EventId = string.Empty;

        public List<ModuleChapterGridCoordinateSaveData> GridCoordinates =
            new List<ModuleChapterGridCoordinateSaveData>();
    }

    [Serializable]
    internal sealed class ModuleChapterGridCoordinateSaveData
    {
        public int CellX;

        public int CellY;
    }

    [Serializable]
    internal sealed class ModuleChapterCreatureSaveData
    {
        public string CreatureId = string.Empty;

        public string CreatureName = string.Empty;

        public string CreatureSize = string.Empty;

        public string PreviewImagePath = string.Empty;
    }

    [Serializable]
    internal sealed class ModuleChapterCreatureInstanceSaveData
    {
        public string InstanceId = string.Empty;

        public string CreatureId = string.Empty;

        public bool IsActive = true;

        public ModuleChapterGridCoordinateSaveData Coordinate = new ModuleChapterGridCoordinateSaveData();
    }
}
