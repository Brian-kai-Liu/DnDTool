using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GameLogic
{
    internal sealed class ChapterEditorInputData
    {
        public int SelectedChapterIndex { get; set; } = -1;

        public List<ChapterListItemData> Chapters { get; set; } = new List<ChapterListItemData>();
    }

    internal sealed class ChapterMapGridStateData
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

    internal readonly struct ChapterGridCoordinate : IEquatable<ChapterGridCoordinate>
    {
        public static ChapterGridCoordinate Zero => new ChapterGridCoordinate(0, 0);

        public ChapterGridCoordinate(int cellX, int cellY)
        {
            CellX = cellX;
            CellY = cellY;
        }

        public int CellX { get; }

        public int CellY { get; }

        public bool Equals(ChapterGridCoordinate other)
        {
            return CellX == other.CellX && CellY == other.CellY;
        }

        public override bool Equals(object obj)
        {
            return obj is ChapterGridCoordinate other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (CellX * 397) ^ CellY;
            }
        }

        public override string ToString()
        {
            return $"{CellX}:{CellY}";
        }
    }

    internal enum ChapterGridCellMarkType
    {
        Selected = 1,
        DifficultTerrain = 2,
        ImpassableTerrain = 3,
        Event = 4,
    }

    internal sealed class ChapterGridEventData
    {
        public string EventId { get; set; } = string.Empty;

        public bool IsEnabled { get; set; } = true;

        public bool IsOneShot { get; set; }

        public ChapterEventTriggerData Trigger { get; set; } = new ChapterEventTriggerData();

        public ChapterEventEffectData Effect { get; set; } = new ChapterEventEffectData();

        public string EventTitle { get; set; } = string.Empty;

        public string TriggerDescription { get; set; } = string.Empty;

        public string DmNote { get; set; } = string.Empty;
    }

    internal sealed class ChapterEventTriggerData
    {
        public int TriggerMode { get; set; }

        public int TriggerType { get; set; } = -1;

        public ChapterEventAreaTriggerParamData Area { get; set; } = new ChapterEventAreaTriggerParamData();

        public ChapterEventInteractionTriggerParamData Interaction { get; set; } = new ChapterEventInteractionTriggerParamData();

        public ChapterEventPrerequisiteTriggerParamData Prerequisite { get; set; } = new ChapterEventPrerequisiteTriggerParamData();

        public bool AreaFirstEnterOnly { get; set; }

        public bool AreaShareBinding { get; set; }

        public string InteractionTarget { get; set; } = string.Empty;

        public bool InteractionRequireConfirm { get; set; }

        public string PrerequisiteEventId { get; set; } = string.Empty;

        public string DelayDescription { get; set; } = string.Empty;
    }

    internal sealed class ChapterEventAreaTriggerParamData
    {
        public bool FirstEnterOnly { get; set; }

        public bool ShareBinding { get; set; }
    }

    internal sealed class ChapterEventInteractionTriggerParamData
    {
        public string Target { get; set; } = string.Empty;

        public bool RequireConfirm { get; set; }
    }

    internal sealed class ChapterEventPrerequisiteTriggerParamData
    {
        public string EventId { get; set; } = string.Empty;

        public string DelayDescription { get; set; } = string.Empty;
    }

    internal sealed class ChapterEventEffectData
    {
        public int EffectType { get; set; } = -1;

        public ChapterEventCheckEffectParamData Check { get; set; } = new ChapterEventCheckEffectParamData();

        public ChapterEventNarrativeEffectParamData Narrative { get; set; } = new ChapterEventNarrativeEffectParamData();

        public ChapterEventDialogueEffectParamData Dialogue { get; set; } = new ChapterEventDialogueEffectParamData();

        public ChapterEventCreatureEffectParamData Creature { get; set; } = new ChapterEventCreatureEffectParamData();

        public ChapterEventBattleEffectParamData Battle { get; set; } = new ChapterEventBattleEffectParamData();

        public int CheckTargetMode { get; set; }

        public int CheckResolutionMode { get; set; }

        public string SuccessResult { get; set; } = string.Empty;

        public string FailureResult { get; set; } = string.Empty;

        public string LegacyDmPrompt { get; set; } = string.Empty;

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

        public List<ChapterSkillCheckThresholdData> SkillCheckEntries { get; set; } = new List<ChapterSkillCheckThresholdData>();

        public string SkillCheckName { get; set; } = string.Empty;

        public string SkillCheckThreshold { get; set; } = string.Empty;

        public string AbilityStrengthThreshold { get; set; } = string.Empty;

        public string AbilityDexterityThreshold { get; set; } = string.Empty;

        public string AbilityConstitutionThreshold { get; set; } = string.Empty;

        public string AbilityIntelligenceThreshold { get; set; } = string.Empty;

        public string AbilityWisdomThreshold { get; set; } = string.Empty;

        public string AbilityCharismaThreshold { get; set; } = string.Empty;
    }

    internal sealed class ChapterEventCheckEffectParamData
    {
        public int TargetMode { get; set; }

        public int ResolutionMode { get; set; }

        public string SuccessResult { get; set; } = string.Empty;

        public string FailureResult { get; set; } = string.Empty;

        public List<ChapterSkillCheckThresholdData> SkillCheckEntries { get; set; } = new List<ChapterSkillCheckThresholdData>();

        public string SkillCheckName { get; set; } = string.Empty;

        public string SkillCheckThreshold { get; set; } = string.Empty;

        public string AbilityStrengthThreshold { get; set; } = string.Empty;

        public string AbilityDexterityThreshold { get; set; } = string.Empty;

        public string AbilityConstitutionThreshold { get; set; } = string.Empty;

        public string AbilityIntelligenceThreshold { get; set; } = string.Empty;

        public string AbilityWisdomThreshold { get; set; } = string.Empty;

        public string AbilityCharismaThreshold { get; set; } = string.Empty;
    }

    internal sealed class ChapterEventNarrativeEffectParamData
    {
        public string Text { get; set; } = string.Empty;

        public bool DmOnly { get; set; }
    }

    internal sealed class ChapterEventDialogueEffectParamData
    {
        public string Target { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public string Prompt { get; set; } = string.Empty;
    }

    internal sealed class ChapterEventCreatureEffectParamData
    {
        public string InstanceId { get; set; } = string.Empty;

        public bool Activate { get; set; } = true;

        public int PlacementMode { get; set; }
    }

    internal sealed class ChapterEventBattleEffectParamData
    {
        public string Reference { get; set; } = string.Empty;

        public bool IncludeActiveCreatures { get; set; } = true;

        public string Description { get; set; } = string.Empty;
    }

    internal sealed class ChapterSkillCheckThresholdData
    {
        public string SkillName { get; set; } = string.Empty;

        public string Threshold { get; set; } = string.Empty;
    }

    internal sealed class ChapterGridCellData
    {
        public ChapterGridCoordinate Coordinate { get; set; } = ChapterGridCoordinate.Zero;

        public ChapterGridCellMarkType MarkType { get; set; } = ChapterGridCellMarkType.Selected;

        public ChapterGridEventData EventData { get; set; }
    }

    internal sealed class ChapterEventBindingData
    {
        public string BindingId { get; set; } = string.Empty;

        public string EventId { get; set; } = string.Empty;

        public List<ChapterGridCoordinate> GridCoordinates { get; set; } = new List<ChapterGridCoordinate>();
    }

    internal sealed class ChapterListItemData
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Goal { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string DmNote { get; set; } = string.Empty;

        public string TerrainTag { get; set; } = string.Empty;

        public string TerrainSubTag { get; set; } = string.Empty;

        public string AddMapHint { get; set; } = string.Empty;

        public string CreatureInfo { get; set; } = string.Empty;

        public string MapImagePath { get; set; } = string.Empty;

        public ChapterMapGridStateData MapGridState { get; set; } = new ChapterMapGridStateData();

        public List<ChapterGridCellData> GridCells { get; set; } = new List<ChapterGridCellData>();

        public List<ChapterGridEventData> Events { get; set; } = new List<ChapterGridEventData>();

        public List<ChapterEventBindingData> EventBindings { get; set; } = new List<ChapterEventBindingData>();

        public List<ChapterCreatureData> Creatures { get; set; } = new List<ChapterCreatureData>();

        public List<ChapterCreatureInstanceData> CreatureInstances { get; set; } = new List<ChapterCreatureInstanceData>();
    }

    [Serializable]
    internal sealed class ChapterEditorSaveData
    {
        public int SelectedChapterId = -1;
        public int NextChapterId = 1;
        public List<ChapterItemSaveData> Chapters = new List<ChapterItemSaveData>();
    }

    [Serializable]
    internal sealed class ChapterEditorLegacySaveData
    {
        public int SelectedChapterId = -1;
        public int NextChapterId = 1;
        public List<ChapterItemLegacySaveData> Chapters = new List<ChapterItemLegacySaveData>();
    }

    [Serializable]
    internal sealed class ChapterItemSaveData
    {
        public int Id;
        public string Name = string.Empty;
        public string Goal = string.Empty;
        public string Content = string.Empty;
        public string DmNote = string.Empty;
        public string TerrainTag = string.Empty;
        public string TerrainSubTag = string.Empty;
        public string AddMapHint = string.Empty;
        public string CreatureInfo = string.Empty;
        public string MapImagePath = string.Empty;
        public bool IsMapZoomEnabled;
        public bool IsGridZoomEnabled;
        public float MapZoomScale = 1f;
        public Vector2 MapPanOffset = Vector2.zero;
        public float GridZoomScale = 1f;
        public Vector2 GridPanOffset = Vector2.zero;
        public bool IsMapGridLocked;
        public float LockedMapZoomReference = 1f;
        public float LockedGridToMapZoomRatio = 1f;
        public Vector2 LockedGridToMapPanDelta = Vector2.zero;
        public List<ChapterGridCellSaveData> GridCells = new List<ChapterGridCellSaveData>();
        public List<ChapterGridEventSaveData> Events = new List<ChapterGridEventSaveData>();
        public List<ChapterEventBindingSaveData> EventBindings = new List<ChapterEventBindingSaveData>();
        public List<string> SelectedGridCellKeys = new List<string>();
        public List<ChapterCreatureDataSaveData> Creatures = new List<ChapterCreatureDataSaveData>();
        public List<ChapterCreatureInstanceSaveData> CreatureInstances = new List<ChapterCreatureInstanceSaveData>();
    }

    [Serializable]
    internal sealed class ChapterItemLegacySaveData
    {
        public int Id;
        public string Name = string.Empty;
        public string Goal = string.Empty;
        public string Content = string.Empty;
        public string DmNote = string.Empty;
        public string TerrainTag = string.Empty;
        public string TerrainSubTag = string.Empty;
        public string AddMapHint = string.Empty;
        public string CreatureInfo = string.Empty;
        public string MapImagePath = string.Empty;
        public bool IsMapZoomEnabled;
        public bool IsGridZoomEnabled;
        public float MapZoomScale = 1f;
        public Vector2 MapPanOffset = Vector2.zero;
        public float GridZoomScale = 1f;
        public Vector2 GridPanOffset = Vector2.zero;
        public bool IsMapGridLocked;
        public float LockedMapZoomReference = 1f;
        public float LockedGridToMapZoomRatio = 1f;
        public Vector2 LockedGridToMapPanDelta = Vector2.zero;
        public List<ChapterGridCellLegacySaveData> GridCells = new List<ChapterGridCellLegacySaveData>();
        public List<ChapterGridEventLegacySaveData> Events = new List<ChapterGridEventLegacySaveData>();
        public List<ChapterEventBindingSaveData> EventBindings = new List<ChapterEventBindingSaveData>();
        public List<string> SelectedGridCellKeys = new List<string>();
        public List<ChapterCreatureDataSaveData> Creatures = new List<ChapterCreatureDataSaveData>();
        public List<ChapterCreatureInstanceSaveData> CreatureInstances = new List<ChapterCreatureInstanceSaveData>();
    }

    [Serializable]
    internal sealed class ChapterGridCellSaveData
    {
        public int CellX;
        public int CellY;
        public int MarkType = (int) ChapterGridCellMarkType.DifficultTerrain;
        public ChapterGridEventSaveData EventData;
    }

    [Serializable]
    internal sealed class ChapterGridCellLegacySaveData
    {
        public int CellX;
        public int CellY;
        public int MarkType = (int) ChapterGridCellMarkType.DifficultTerrain;
        public ChapterGridEventLegacySaveData EventData;
    }

    [Serializable]
    internal sealed class ChapterGridEventSaveData
    {
        public string EventId = string.Empty;
        public bool IsEnabled = true;
        public bool IsOneShot;
        public ChapterEventTriggerSaveData Trigger = new ChapterEventTriggerSaveData();
        public ChapterEventEffectSaveData Effect = new ChapterEventEffectSaveData();
        public string EventTitle = string.Empty;
        public string TriggerDescription = string.Empty;
        public string DmNote = string.Empty;
    }

    [Serializable]
    internal sealed class ChapterGridEventLegacySaveData
    {
        public string EventId = string.Empty;
        public bool IsEnabled = true;
        public bool IsOneShot;
        public ChapterEventTriggerSaveData Trigger = new ChapterEventTriggerSaveData();
        public ChapterEventEffectSaveData Effect = new ChapterEventEffectSaveData();
        public int EventType = 2;
        public int EventCategory;
        public int EventSubType;
        public int TriggerMode;
        public int TriggerType = -1;
        public int EffectType = -1;
        public int CheckTargetMode;
        public int CheckResolutionMode;
        public string EventTitle = string.Empty;
        public string TriggerDescription = string.Empty;
        public string SuccessResult = string.Empty;
        public string FailureResult = string.Empty;
        public string DmNote = string.Empty;
        public string DmPrompt = string.Empty;
        public bool TriggerAreaFirstEnterOnly;
        public bool TriggerAreaShareBinding;
        public string TriggerInteractionTarget = string.Empty;
        public bool TriggerInteractionRequireConfirm;
        public string TriggerPrerequisiteEventId = string.Empty;
        public string TriggerDelayDescription = string.Empty;
        public string EffectNarrativeText = string.Empty;
        public bool EffectNarrativeDmOnly;
        public string EffectDialogueTarget = string.Empty;
        public string EffectDialogueSummary = string.Empty;
        public string EffectDialoguePrompt = string.Empty;
        public string EffectCreatureInstanceId = string.Empty;
        public bool EffectCreatureActivate = true;
        public int EffectCreaturePlacementMode;
        public string EffectBattleReference = string.Empty;
        public bool EffectBattleIncludeActiveCreatures = true;
        public string EffectBattleDescription = string.Empty;
        public List<ChapterSkillCheckThresholdSaveData> SkillCheckEntries = new List<ChapterSkillCheckThresholdSaveData>();
        public string SkillCheckName = string.Empty;
        public string SkillCheckThreshold = string.Empty;
        public string AbilityStrengthThreshold = string.Empty;
        public string AbilityDexterityThreshold = string.Empty;
        public string AbilityConstitutionThreshold = string.Empty;
        public string AbilityIntelligenceThreshold = string.Empty;
        public string AbilityWisdomThreshold = string.Empty;
        public string AbilityCharismaThreshold = string.Empty;
    }

    [Serializable]
    internal sealed class ChapterEventTriggerSaveData
    {
        public int TriggerMode;
        public int TriggerType = -1;
        public ChapterEventAreaTriggerParamSaveData Area = new ChapterEventAreaTriggerParamSaveData();
        public ChapterEventInteractionTriggerParamSaveData Interaction = new ChapterEventInteractionTriggerParamSaveData();
        public ChapterEventPrerequisiteTriggerParamSaveData Prerequisite = new ChapterEventPrerequisiteTriggerParamSaveData();
        public bool AreaFirstEnterOnly;
        public bool AreaShareBinding;
        public string InteractionTarget = string.Empty;
        public bool InteractionRequireConfirm;
        public string PrerequisiteEventId = string.Empty;
        public string DelayDescription = string.Empty;
    }

    [Serializable]
    internal sealed class ChapterEventAreaTriggerParamSaveData
    {
        public bool FirstEnterOnly;
        public bool ShareBinding;
    }

    [Serializable]
    internal sealed class ChapterEventInteractionTriggerParamSaveData
    {
        public string Target = string.Empty;
        public bool RequireConfirm;
    }

    [Serializable]
    internal sealed class ChapterEventPrerequisiteTriggerParamSaveData
    {
        public string EventId = string.Empty;
        public string DelayDescription = string.Empty;
    }

    [Serializable]
    internal sealed class ChapterEventEffectSaveData
    {
        public int EffectType = -1;
        public ChapterEventCheckEffectParamSaveData Check = new ChapterEventCheckEffectParamSaveData();
        public ChapterEventNarrativeEffectParamSaveData Narrative = new ChapterEventNarrativeEffectParamSaveData();
        public ChapterEventDialogueEffectParamSaveData Dialogue = new ChapterEventDialogueEffectParamSaveData();
        public ChapterEventCreatureEffectParamSaveData Creature = new ChapterEventCreatureEffectParamSaveData();
        public ChapterEventBattleEffectParamSaveData Battle = new ChapterEventBattleEffectParamSaveData();
        public int CheckTargetMode;
        public int CheckResolutionMode;
        public string SuccessResult = string.Empty;
        public string FailureResult = string.Empty;
        public string LegacyDmPrompt = string.Empty;
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
        public List<ChapterSkillCheckThresholdSaveData> SkillCheckEntries = new List<ChapterSkillCheckThresholdSaveData>();
        public string SkillCheckName = string.Empty;
        public string SkillCheckThreshold = string.Empty;
        public string AbilityStrengthThreshold = string.Empty;
        public string AbilityDexterityThreshold = string.Empty;
        public string AbilityConstitutionThreshold = string.Empty;
        public string AbilityIntelligenceThreshold = string.Empty;
        public string AbilityWisdomThreshold = string.Empty;
        public string AbilityCharismaThreshold = string.Empty;
    }

    [Serializable]
    internal sealed class ChapterEventCheckEffectParamSaveData
    {
        public int TargetMode;
        public int ResolutionMode;
        public string SuccessResult = string.Empty;
        public string FailureResult = string.Empty;
        public List<ChapterSkillCheckThresholdSaveData> SkillCheckEntries = new List<ChapterSkillCheckThresholdSaveData>();
        public string SkillCheckName = string.Empty;
        public string SkillCheckThreshold = string.Empty;
        public string AbilityStrengthThreshold = string.Empty;
        public string AbilityDexterityThreshold = string.Empty;
        public string AbilityConstitutionThreshold = string.Empty;
        public string AbilityIntelligenceThreshold = string.Empty;
        public string AbilityWisdomThreshold = string.Empty;
        public string AbilityCharismaThreshold = string.Empty;
    }

    [Serializable]
    internal sealed class ChapterEventNarrativeEffectParamSaveData
    {
        public string Text = string.Empty;
        public bool DmOnly;
    }

    [Serializable]
    internal sealed class ChapterEventDialogueEffectParamSaveData
    {
        public string Target = string.Empty;
        public string Summary = string.Empty;
        public string Prompt = string.Empty;
    }

    [Serializable]
    internal sealed class ChapterEventCreatureEffectParamSaveData
    {
        public string InstanceId = string.Empty;
        public bool Activate = true;
        public int PlacementMode;
    }

    [Serializable]
    internal sealed class ChapterEventBattleEffectParamSaveData
    {
        public string Reference = string.Empty;
        public bool IncludeActiveCreatures = true;
        public string Description = string.Empty;
    }

    [Serializable]
    internal sealed class ChapterEventBindingSaveData
    {
        public string BindingId = string.Empty;
        public string EventId = string.Empty;
        public List<ChapterGridCoordinateSaveData> GridCoordinates = new List<ChapterGridCoordinateSaveData>();
    }

    [Serializable]
    internal sealed class ChapterGridCoordinateSaveData
    {
        public int CellX;
        public int CellY;
    }

    [Serializable]
    internal sealed class ChapterSkillCheckThresholdSaveData
    {
        public string SkillName = string.Empty;

        public string Threshold = string.Empty;
    }

    [Serializable]
    internal sealed class ChapterCreatureInstancePlacementSaveData
    {
        public int AnchorCellX;
        public int AnchorCellY;
        public float PreviewScale = 1f;
        public bool SnapToGrid = true;
    }

    [Serializable]
    internal sealed class ChapterCreatureInstanceSaveData
    {
        public string InstanceId = string.Empty;
        public string SourceCreatureId = string.Empty;
        public bool IsActive = true;
        public ChapterCreatureInstancePlacementSaveData Placement = new ChapterCreatureInstancePlacementSaveData();
        public ChapterCreatureDataSaveData RuntimeSheet = new ChapterCreatureDataSaveData();
        public string DmNote = string.Empty;
    }

    internal static class ChapterEventDataStructureUtility
    {
        private const int LegacyCheckEventCategory = 0;
        private const int LegacyDmDirectEventCategory = 1;
        private const int LegacyDmEventSubTypeStory = 0;
        private const int LegacyDmEventSubTypeDialogue = 1;
        private const int LegacyDmEventSubTypeChoice = 2;
        private const int LegacyDmEventSubTypeInteraction = 3;
        private const int LegacyDmEventSubTypeCombat = 4;
        private const int LegacyDmEventSubTypeSpecial = 9;
        private const int LegacyTriggerModeAutomatic = 0;
        private const int LegacyTriggerModeDmManual = 1;
        private const int RuntimeTriggerTypeDmManual = 0;
        private const int RuntimeTriggerTypeEnterBindingArea = 1;
        private const int RuntimeTriggerTypeInteractWithSceneObject = 2;
        private const int RuntimeTriggerTypeAfterPrerequisiteEvent = 3;
        private const int RuntimeEffectTypeCheck = 0;
        private const int RuntimeEffectTypeNarrativePrompt = 1;
        private const int RuntimeEffectTypeDialogueInteractionPrompt = 2;
        private const int RuntimeEffectTypeActivateCreatureInstance = 3;
        private const int RuntimeEffectTypeStartBattle = 4;

        public static ChapterGridEventData NormalizeRuntimeEventData(ChapterGridEventData eventData)
        {
            if (eventData == null)
            {
                return null;
            }

            eventData.Trigger ??= new ChapterEventTriggerData();
            eventData.Effect ??= new ChapterEventEffectData();
            eventData.Trigger = NormalizeRuntimeTriggerData(eventData.Trigger);
            eventData.Effect = NormalizeRuntimeEffectData(eventData.Effect);
            eventData.EventId ??= string.Empty;
            eventData.EventTitle ??= string.Empty;
            eventData.TriggerDescription ??= string.Empty;
            eventData.DmNote ??= string.Empty;
            return eventData;
        }

        public static ChapterGridEventSaveData NormalizeSaveEventData(ChapterGridEventSaveData eventData)
        {
            if (eventData == null)
            {
                return null;
            }

            eventData.Trigger ??= new ChapterEventTriggerSaveData();
            eventData.Effect ??= new ChapterEventEffectSaveData();
            eventData.Trigger = NormalizeSaveTriggerData(eventData.Trigger);
            eventData.Effect = NormalizeSaveEffectData(eventData.Effect);
            eventData.EventId ??= string.Empty;
            eventData.EventTitle ??= string.Empty;
            eventData.TriggerDescription ??= string.Empty;
            eventData.DmNote ??= string.Empty;
            return eventData;
        }

        public static ChapterGridEventSaveData NormalizeLegacySaveEventData(ChapterGridEventLegacySaveData eventData)
        {
            if (eventData == null)
            {
                return null;
            }

            ChapterGridEventSaveData normalizedEventData = new ChapterGridEventSaveData
            {
                EventId = eventData.EventId ?? string.Empty,
                IsEnabled = eventData.IsEnabled,
                IsOneShot = eventData.IsOneShot,
                Trigger = IsLegacySaveTriggerStructureMissing(eventData)
                    ? CreateSaveTriggerFromLegacy(eventData)
                    : NormalizeSaveTriggerData(eventData.Trigger),
                Effect = IsLegacySaveEffectStructureMissing(eventData)
                    ? CreateSaveEffectFromLegacy(eventData)
                    : NormalizeSaveEffectData(eventData.Effect),
                EventTitle = eventData.EventTitle ?? string.Empty,
                TriggerDescription = eventData.TriggerDescription ?? string.Empty,
                DmNote = eventData.DmNote ?? string.Empty,
            };

            return NormalizeSaveEventData(normalizedEventData);
        }

        public static ChapterEventTriggerData CloneRuntimeTrigger(ChapterEventTriggerData source)
        {
            if (source == null)
            {
                return new ChapterEventTriggerData();
            }

            return new ChapterEventTriggerData
            {
                TriggerMode = source.TriggerMode,
                TriggerType = source.TriggerType,
                Area = CloneRuntimeAreaTriggerParam(source.Area),
                Interaction = CloneRuntimeInteractionTriggerParam(source.Interaction),
                Prerequisite = CloneRuntimePrerequisiteTriggerParam(source.Prerequisite),
                AreaFirstEnterOnly = source.AreaFirstEnterOnly,
                AreaShareBinding = source.AreaShareBinding,
                InteractionTarget = source.InteractionTarget ?? string.Empty,
                InteractionRequireConfirm = source.InteractionRequireConfirm,
                PrerequisiteEventId = source.PrerequisiteEventId ?? string.Empty,
                DelayDescription = source.DelayDescription ?? string.Empty,
            };
        }

        public static ChapterEventEffectData CloneRuntimeEffect(ChapterEventEffectData source)
        {
            if (source == null)
            {
                return new ChapterEventEffectData();
            }

            return new ChapterEventEffectData
            {
                EffectType = source.EffectType,
                Check = CloneRuntimeCheckEffectParam(source.Check),
                Narrative = CloneRuntimeNarrativeEffectParam(source.Narrative),
                Dialogue = CloneRuntimeDialogueEffectParam(source.Dialogue),
                Creature = CloneRuntimeCreatureEffectParam(source.Creature),
                Battle = CloneRuntimeBattleEffectParam(source.Battle),
                CheckTargetMode = source.CheckTargetMode,
                CheckResolutionMode = source.CheckResolutionMode,
                SuccessResult = source.SuccessResult ?? string.Empty,
                FailureResult = source.FailureResult ?? string.Empty,
                LegacyDmPrompt = source.LegacyDmPrompt ?? string.Empty,
                NarrativeText = source.NarrativeText ?? string.Empty,
                NarrativeDmOnly = source.NarrativeDmOnly,
                DialogueTarget = source.DialogueTarget ?? string.Empty,
                DialogueSummary = source.DialogueSummary ?? string.Empty,
                DialoguePrompt = source.DialoguePrompt ?? string.Empty,
                CreatureInstanceId = source.CreatureInstanceId ?? string.Empty,
                CreatureActivate = source.CreatureActivate,
                CreaturePlacementMode = source.CreaturePlacementMode,
                BattleReference = source.BattleReference ?? string.Empty,
                BattleIncludeActiveCreatures = source.BattleIncludeActiveCreatures,
                BattleDescription = source.BattleDescription ?? string.Empty,
                SkillCheckEntries = CloneRuntimeSkillCheckEntries(source.SkillCheckEntries),
                SkillCheckName = source.SkillCheckName ?? string.Empty,
                SkillCheckThreshold = source.SkillCheckThreshold ?? string.Empty,
                AbilityStrengthThreshold = source.AbilityStrengthThreshold ?? string.Empty,
                AbilityDexterityThreshold = source.AbilityDexterityThreshold ?? string.Empty,
                AbilityConstitutionThreshold = source.AbilityConstitutionThreshold ?? string.Empty,
                AbilityIntelligenceThreshold = source.AbilityIntelligenceThreshold ?? string.Empty,
                AbilityWisdomThreshold = source.AbilityWisdomThreshold ?? string.Empty,
                AbilityCharismaThreshold = source.AbilityCharismaThreshold ?? string.Empty,
            };
        }

        public static ChapterEventTriggerSaveData CloneSaveTrigger(ChapterEventTriggerSaveData source)
        {
            if (source == null)
            {
                return new ChapterEventTriggerSaveData();
            }

            return new ChapterEventTriggerSaveData
            {
                TriggerMode = source.TriggerMode,
                TriggerType = source.TriggerType,
                Area = CloneSaveAreaTriggerParam(source.Area),
                Interaction = CloneSaveInteractionTriggerParam(source.Interaction),
                Prerequisite = CloneSavePrerequisiteTriggerParam(source.Prerequisite),
                AreaFirstEnterOnly = source.AreaFirstEnterOnly,
                AreaShareBinding = source.AreaShareBinding,
                InteractionTarget = source.InteractionTarget ?? string.Empty,
                InteractionRequireConfirm = source.InteractionRequireConfirm,
                PrerequisiteEventId = source.PrerequisiteEventId ?? string.Empty,
                DelayDescription = source.DelayDescription ?? string.Empty,
            };
        }

        public static ChapterEventEffectSaveData CloneSaveEffect(ChapterEventEffectSaveData source)
        {
            if (source == null)
            {
                return new ChapterEventEffectSaveData();
            }

            return new ChapterEventEffectSaveData
            {
                EffectType = source.EffectType,
                Check = CloneSaveCheckEffectParam(source.Check),
                Narrative = CloneSaveNarrativeEffectParam(source.Narrative),
                Dialogue = CloneSaveDialogueEffectParam(source.Dialogue),
                Creature = CloneSaveCreatureEffectParam(source.Creature),
                Battle = CloneSaveBattleEffectParam(source.Battle),
                CheckTargetMode = source.CheckTargetMode,
                CheckResolutionMode = source.CheckResolutionMode,
                SuccessResult = source.SuccessResult ?? string.Empty,
                FailureResult = source.FailureResult ?? string.Empty,
                LegacyDmPrompt = source.LegacyDmPrompt ?? string.Empty,
                NarrativeText = source.NarrativeText ?? string.Empty,
                NarrativeDmOnly = source.NarrativeDmOnly,
                DialogueTarget = source.DialogueTarget ?? string.Empty,
                DialogueSummary = source.DialogueSummary ?? string.Empty,
                DialoguePrompt = source.DialoguePrompt ?? string.Empty,
                CreatureInstanceId = source.CreatureInstanceId ?? string.Empty,
                CreatureActivate = source.CreatureActivate,
                CreaturePlacementMode = source.CreaturePlacementMode,
                BattleReference = source.BattleReference ?? string.Empty,
                BattleIncludeActiveCreatures = source.BattleIncludeActiveCreatures,
                BattleDescription = source.BattleDescription ?? string.Empty,
                SkillCheckEntries = CloneSaveSkillCheckEntries(source.SkillCheckEntries),
                SkillCheckName = source.SkillCheckName ?? string.Empty,
                SkillCheckThreshold = source.SkillCheckThreshold ?? string.Empty,
                AbilityStrengthThreshold = source.AbilityStrengthThreshold ?? string.Empty,
                AbilityDexterityThreshold = source.AbilityDexterityThreshold ?? string.Empty,
                AbilityConstitutionThreshold = source.AbilityConstitutionThreshold ?? string.Empty,
                AbilityIntelligenceThreshold = source.AbilityIntelligenceThreshold ?? string.Empty,
                AbilityWisdomThreshold = source.AbilityWisdomThreshold ?? string.Empty,
                AbilityCharismaThreshold = source.AbilityCharismaThreshold ?? string.Empty,
            };
        }

        public static ChapterEventAreaTriggerParamData CloneRuntimeAreaTriggerParam(ChapterEventAreaTriggerParamData source)
        {
            if (source == null)
            {
                return new ChapterEventAreaTriggerParamData();
            }

            return new ChapterEventAreaTriggerParamData
            {
                FirstEnterOnly = source.FirstEnterOnly,
                ShareBinding = source.ShareBinding,
            };
        }

        public static ChapterEventInteractionTriggerParamData CloneRuntimeInteractionTriggerParam(ChapterEventInteractionTriggerParamData source)
        {
            if (source == null)
            {
                return new ChapterEventInteractionTriggerParamData();
            }

            return new ChapterEventInteractionTriggerParamData
            {
                Target = source.Target ?? string.Empty,
                RequireConfirm = source.RequireConfirm,
            };
        }

        public static ChapterEventPrerequisiteTriggerParamData CloneRuntimePrerequisiteTriggerParam(ChapterEventPrerequisiteTriggerParamData source)
        {
            if (source == null)
            {
                return new ChapterEventPrerequisiteTriggerParamData();
            }

            return new ChapterEventPrerequisiteTriggerParamData
            {
                EventId = source.EventId ?? string.Empty,
                DelayDescription = source.DelayDescription ?? string.Empty,
            };
        }

        public static ChapterEventCheckEffectParamData CloneRuntimeCheckEffectParam(ChapterEventCheckEffectParamData source)
        {
            if (source == null)
            {
                return new ChapterEventCheckEffectParamData();
            }

            return new ChapterEventCheckEffectParamData
            {
                TargetMode = source.TargetMode,
                ResolutionMode = source.ResolutionMode,
                SuccessResult = source.SuccessResult ?? string.Empty,
                FailureResult = source.FailureResult ?? string.Empty,
                SkillCheckEntries = CloneRuntimeSkillCheckEntries(source.SkillCheckEntries),
                SkillCheckName = source.SkillCheckName ?? string.Empty,
                SkillCheckThreshold = source.SkillCheckThreshold ?? string.Empty,
                AbilityStrengthThreshold = source.AbilityStrengthThreshold ?? string.Empty,
                AbilityDexterityThreshold = source.AbilityDexterityThreshold ?? string.Empty,
                AbilityConstitutionThreshold = source.AbilityConstitutionThreshold ?? string.Empty,
                AbilityIntelligenceThreshold = source.AbilityIntelligenceThreshold ?? string.Empty,
                AbilityWisdomThreshold = source.AbilityWisdomThreshold ?? string.Empty,
                AbilityCharismaThreshold = source.AbilityCharismaThreshold ?? string.Empty,
            };
        }

        public static ChapterEventNarrativeEffectParamData CloneRuntimeNarrativeEffectParam(ChapterEventNarrativeEffectParamData source)
        {
            if (source == null)
            {
                return new ChapterEventNarrativeEffectParamData();
            }

            return new ChapterEventNarrativeEffectParamData
            {
                Text = source.Text ?? string.Empty,
                DmOnly = source.DmOnly,
            };
        }

        public static ChapterEventDialogueEffectParamData CloneRuntimeDialogueEffectParam(ChapterEventDialogueEffectParamData source)
        {
            if (source == null)
            {
                return new ChapterEventDialogueEffectParamData();
            }

            return new ChapterEventDialogueEffectParamData
            {
                Target = source.Target ?? string.Empty,
                Summary = source.Summary ?? string.Empty,
                Prompt = source.Prompt ?? string.Empty,
            };
        }

        public static ChapterEventCreatureEffectParamData CloneRuntimeCreatureEffectParam(ChapterEventCreatureEffectParamData source)
        {
            if (source == null)
            {
                return new ChapterEventCreatureEffectParamData();
            }

            return new ChapterEventCreatureEffectParamData
            {
                InstanceId = source.InstanceId ?? string.Empty,
                Activate = source.Activate,
                PlacementMode = source.PlacementMode,
            };
        }

        public static ChapterEventBattleEffectParamData CloneRuntimeBattleEffectParam(ChapterEventBattleEffectParamData source)
        {
            if (source == null)
            {
                return new ChapterEventBattleEffectParamData();
            }

            return new ChapterEventBattleEffectParamData
            {
                Reference = source.Reference ?? string.Empty,
                IncludeActiveCreatures = source.IncludeActiveCreatures,
                Description = source.Description ?? string.Empty,
            };
        }

        public static ChapterEventAreaTriggerParamSaveData CloneSaveAreaTriggerParam(ChapterEventAreaTriggerParamSaveData source)
        {
            if (source == null)
            {
                return new ChapterEventAreaTriggerParamSaveData();
            }

            return new ChapterEventAreaTriggerParamSaveData
            {
                FirstEnterOnly = source.FirstEnterOnly,
                ShareBinding = source.ShareBinding,
            };
        }

        public static ChapterEventInteractionTriggerParamSaveData CloneSaveInteractionTriggerParam(ChapterEventInteractionTriggerParamSaveData source)
        {
            if (source == null)
            {
                return new ChapterEventInteractionTriggerParamSaveData();
            }

            return new ChapterEventInteractionTriggerParamSaveData
            {
                Target = source.Target ?? string.Empty,
                RequireConfirm = source.RequireConfirm,
            };
        }

        public static ChapterEventPrerequisiteTriggerParamSaveData CloneSavePrerequisiteTriggerParam(ChapterEventPrerequisiteTriggerParamSaveData source)
        {
            if (source == null)
            {
                return new ChapterEventPrerequisiteTriggerParamSaveData();
            }

            return new ChapterEventPrerequisiteTriggerParamSaveData
            {
                EventId = source.EventId ?? string.Empty,
                DelayDescription = source.DelayDescription ?? string.Empty,
            };
        }

        public static ChapterEventCheckEffectParamSaveData CloneSaveCheckEffectParam(ChapterEventCheckEffectParamSaveData source)
        {
            if (source == null)
            {
                return new ChapterEventCheckEffectParamSaveData();
            }

            return new ChapterEventCheckEffectParamSaveData
            {
                TargetMode = source.TargetMode,
                ResolutionMode = source.ResolutionMode,
                SuccessResult = source.SuccessResult ?? string.Empty,
                FailureResult = source.FailureResult ?? string.Empty,
                SkillCheckEntries = CloneSaveSkillCheckEntries(source.SkillCheckEntries),
                SkillCheckName = source.SkillCheckName ?? string.Empty,
                SkillCheckThreshold = source.SkillCheckThreshold ?? string.Empty,
                AbilityStrengthThreshold = source.AbilityStrengthThreshold ?? string.Empty,
                AbilityDexterityThreshold = source.AbilityDexterityThreshold ?? string.Empty,
                AbilityConstitutionThreshold = source.AbilityConstitutionThreshold ?? string.Empty,
                AbilityIntelligenceThreshold = source.AbilityIntelligenceThreshold ?? string.Empty,
                AbilityWisdomThreshold = source.AbilityWisdomThreshold ?? string.Empty,
                AbilityCharismaThreshold = source.AbilityCharismaThreshold ?? string.Empty,
            };
        }

        public static ChapterEventNarrativeEffectParamSaveData CloneSaveNarrativeEffectParam(ChapterEventNarrativeEffectParamSaveData source)
        {
            if (source == null)
            {
                return new ChapterEventNarrativeEffectParamSaveData();
            }

            return new ChapterEventNarrativeEffectParamSaveData
            {
                Text = source.Text ?? string.Empty,
                DmOnly = source.DmOnly,
            };
        }

        public static ChapterEventDialogueEffectParamSaveData CloneSaveDialogueEffectParam(ChapterEventDialogueEffectParamSaveData source)
        {
            if (source == null)
            {
                return new ChapterEventDialogueEffectParamSaveData();
            }

            return new ChapterEventDialogueEffectParamSaveData
            {
                Target = source.Target ?? string.Empty,
                Summary = source.Summary ?? string.Empty,
                Prompt = source.Prompt ?? string.Empty,
            };
        }

        public static ChapterEventCreatureEffectParamSaveData CloneSaveCreatureEffectParam(ChapterEventCreatureEffectParamSaveData source)
        {
            if (source == null)
            {
                return new ChapterEventCreatureEffectParamSaveData();
            }

            return new ChapterEventCreatureEffectParamSaveData
            {
                InstanceId = source.InstanceId ?? string.Empty,
                Activate = source.Activate,
                PlacementMode = source.PlacementMode,
            };
        }

        public static ChapterEventBattleEffectParamSaveData CloneSaveBattleEffectParam(ChapterEventBattleEffectParamSaveData source)
        {
            if (source == null)
            {
                return new ChapterEventBattleEffectParamSaveData();
            }

            return new ChapterEventBattleEffectParamSaveData
            {
                Reference = source.Reference ?? string.Empty,
                IncludeActiveCreatures = source.IncludeActiveCreatures,
                Description = source.Description ?? string.Empty,
            };
        }

        private static ChapterEventTriggerSaveData CreateSaveTriggerFromLegacy(ChapterGridEventLegacySaveData eventData)
        {
            return NormalizeSaveTriggerData(new ChapterEventTriggerSaveData
            {
                TriggerMode = eventData.TriggerMode,
                TriggerType = eventData.TriggerType,
                AreaFirstEnterOnly = eventData.TriggerAreaFirstEnterOnly,
                AreaShareBinding = eventData.TriggerAreaShareBinding,
                InteractionTarget = eventData.TriggerInteractionTarget ?? string.Empty,
                InteractionRequireConfirm = eventData.TriggerInteractionRequireConfirm,
                PrerequisiteEventId = eventData.TriggerPrerequisiteEventId ?? string.Empty,
                DelayDescription = eventData.TriggerDelayDescription ?? string.Empty,
            });
        }

        private static ChapterEventEffectSaveData CreateSaveEffectFromLegacy(ChapterGridEventLegacySaveData eventData)
        {
            return NormalizeSaveEffectData(new ChapterEventEffectSaveData
            {
                EffectType = eventData.EffectType,
                CheckTargetMode = eventData.CheckTargetMode,
                CheckResolutionMode = eventData.CheckResolutionMode,
                SuccessResult = eventData.SuccessResult ?? string.Empty,
                FailureResult = eventData.FailureResult ?? string.Empty,
                LegacyDmPrompt = eventData.DmPrompt ?? string.Empty,
                NarrativeText = eventData.EffectNarrativeText ?? string.Empty,
                NarrativeDmOnly = eventData.EffectNarrativeDmOnly,
                DialogueTarget = eventData.EffectDialogueTarget ?? string.Empty,
                DialogueSummary = eventData.EffectDialogueSummary ?? string.Empty,
                DialoguePrompt = eventData.EffectDialoguePrompt ?? string.Empty,
                CreatureInstanceId = eventData.EffectCreatureInstanceId ?? string.Empty,
                CreatureActivate = eventData.EffectCreatureActivate,
                CreaturePlacementMode = eventData.EffectCreaturePlacementMode,
                BattleReference = eventData.EffectBattleReference ?? string.Empty,
                BattleIncludeActiveCreatures = eventData.EffectBattleIncludeActiveCreatures,
                BattleDescription = eventData.EffectBattleDescription ?? string.Empty,
                SkillCheckEntries = CloneSaveSkillCheckEntries(eventData.SkillCheckEntries),
                SkillCheckName = eventData.SkillCheckName ?? string.Empty,
                SkillCheckThreshold = eventData.SkillCheckThreshold ?? string.Empty,
                AbilityStrengthThreshold = eventData.AbilityStrengthThreshold ?? string.Empty,
                AbilityDexterityThreshold = eventData.AbilityDexterityThreshold ?? string.Empty,
                AbilityConstitutionThreshold = eventData.AbilityConstitutionThreshold ?? string.Empty,
                AbilityIntelligenceThreshold = eventData.AbilityIntelligenceThreshold ?? string.Empty,
                AbilityWisdomThreshold = eventData.AbilityWisdomThreshold ?? string.Empty,
                AbilityCharismaThreshold = eventData.AbilityCharismaThreshold ?? string.Empty,
            }, eventData.EffectType, eventData.EventCategory, eventData.EventSubType);
        }

        private static ChapterEventTriggerData NormalizeRuntimeTriggerData(ChapterEventTriggerData source)
        {
            ChapterEventTriggerData triggerData = CloneRuntimeTrigger(source);
            if (!triggerData.Area.FirstEnterOnly && !triggerData.Area.ShareBinding
                && (triggerData.AreaFirstEnterOnly || triggerData.AreaShareBinding))
            {
                triggerData.Area.FirstEnterOnly = triggerData.AreaFirstEnterOnly;
                triggerData.Area.ShareBinding = triggerData.AreaShareBinding;
            }

            if (string.IsNullOrWhiteSpace(triggerData.Interaction.Target)
                && !triggerData.Interaction.RequireConfirm
                && (!string.IsNullOrWhiteSpace(triggerData.InteractionTarget) || triggerData.InteractionRequireConfirm))
            {
                triggerData.Interaction.Target = triggerData.InteractionTarget ?? string.Empty;
                triggerData.Interaction.RequireConfirm = triggerData.InteractionRequireConfirm;
            }

            if (string.IsNullOrWhiteSpace(triggerData.Prerequisite.EventId)
                && string.IsNullOrWhiteSpace(triggerData.Prerequisite.DelayDescription)
                && (!string.IsNullOrWhiteSpace(triggerData.PrerequisiteEventId) || !string.IsNullOrWhiteSpace(triggerData.DelayDescription)))
            {
                triggerData.Prerequisite.EventId = triggerData.PrerequisiteEventId ?? string.Empty;
                triggerData.Prerequisite.DelayDescription = triggerData.DelayDescription ?? string.Empty;
            }

            triggerData.AreaFirstEnterOnly = triggerData.Area.FirstEnterOnly;
            triggerData.AreaShareBinding = triggerData.Area.ShareBinding;
            triggerData.InteractionTarget = triggerData.Interaction.Target ?? string.Empty;
            triggerData.InteractionRequireConfirm = triggerData.Interaction.RequireConfirm;
            triggerData.PrerequisiteEventId = triggerData.Prerequisite.EventId ?? string.Empty;
            triggerData.DelayDescription = triggerData.Prerequisite.DelayDescription ?? string.Empty;
            triggerData.TriggerType = ResolveRuntimeTriggerType(
                triggerData.TriggerType,
                triggerData.TriggerMode,
                triggerData.InteractionTarget,
                triggerData.PrerequisiteEventId);
            triggerData.TriggerMode = ResolveLegacyTriggerMode(triggerData.TriggerType, triggerData.TriggerMode);
            return triggerData;
        }

        private static ChapterEventEffectData NormalizeRuntimeEffectData(ChapterEventEffectData source)
        {
            ChapterEventEffectData effectData = CloneRuntimeEffect(source);
            if (!HasCheckContent(
                    effectData.Check.SkillCheckEntries,
                    effectData.Check.SkillCheckName,
                    effectData.Check.SkillCheckThreshold,
                    effectData.Check.AbilityStrengthThreshold,
                    effectData.Check.AbilityDexterityThreshold,
                    effectData.Check.AbilityConstitutionThreshold,
                    effectData.Check.AbilityIntelligenceThreshold,
                    effectData.Check.AbilityWisdomThreshold,
                    effectData.Check.AbilityCharismaThreshold,
                    effectData.Check.SuccessResult,
                    effectData.Check.FailureResult)
                && HasCheckContent(
                    effectData.SkillCheckEntries,
                    effectData.SkillCheckName,
                    effectData.SkillCheckThreshold,
                    effectData.AbilityStrengthThreshold,
                    effectData.AbilityDexterityThreshold,
                    effectData.AbilityConstitutionThreshold,
                    effectData.AbilityIntelligenceThreshold,
                    effectData.AbilityWisdomThreshold,
                    effectData.AbilityCharismaThreshold,
                    effectData.SuccessResult,
                    effectData.FailureResult))
            {
                effectData.Check.TargetMode = effectData.CheckTargetMode;
                effectData.Check.ResolutionMode = effectData.CheckResolutionMode;
                effectData.Check.SuccessResult = effectData.SuccessResult ?? string.Empty;
                effectData.Check.FailureResult = effectData.FailureResult ?? string.Empty;
                effectData.Check.SkillCheckEntries = CloneRuntimeSkillCheckEntries(effectData.SkillCheckEntries);
                effectData.Check.SkillCheckName = effectData.SkillCheckName ?? string.Empty;
                effectData.Check.SkillCheckThreshold = effectData.SkillCheckThreshold ?? string.Empty;
                effectData.Check.AbilityStrengthThreshold = effectData.AbilityStrengthThreshold ?? string.Empty;
                effectData.Check.AbilityDexterityThreshold = effectData.AbilityDexterityThreshold ?? string.Empty;
                effectData.Check.AbilityConstitutionThreshold = effectData.AbilityConstitutionThreshold ?? string.Empty;
                effectData.Check.AbilityIntelligenceThreshold = effectData.AbilityIntelligenceThreshold ?? string.Empty;
                effectData.Check.AbilityWisdomThreshold = effectData.AbilityWisdomThreshold ?? string.Empty;
                effectData.Check.AbilityCharismaThreshold = effectData.AbilityCharismaThreshold ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(effectData.Narrative.Text)
                && !effectData.Narrative.DmOnly
                && (!string.IsNullOrWhiteSpace(effectData.NarrativeText) || effectData.NarrativeDmOnly))
            {
                effectData.Narrative.Text = effectData.NarrativeText ?? string.Empty;
                effectData.Narrative.DmOnly = effectData.NarrativeDmOnly;
            }

            if (string.IsNullOrWhiteSpace(effectData.Dialogue.Target)
                && string.IsNullOrWhiteSpace(effectData.Dialogue.Summary)
                && string.IsNullOrWhiteSpace(effectData.Dialogue.Prompt)
                && (!string.IsNullOrWhiteSpace(effectData.DialogueTarget)
                    || !string.IsNullOrWhiteSpace(effectData.DialogueSummary)
                    || !string.IsNullOrWhiteSpace(effectData.DialoguePrompt)))
            {
                effectData.Dialogue.Target = effectData.DialogueTarget ?? string.Empty;
                effectData.Dialogue.Summary = effectData.DialogueSummary ?? string.Empty;
                effectData.Dialogue.Prompt = effectData.DialoguePrompt ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(effectData.Creature.InstanceId)
                && effectData.Creature.Activate
                && effectData.Creature.PlacementMode == 0
                && HasCreatureActivationContent(effectData.CreatureInstanceId, effectData.CreatureActivate, effectData.CreaturePlacementMode))
            {
                effectData.Creature.InstanceId = effectData.CreatureInstanceId ?? string.Empty;
                effectData.Creature.Activate = effectData.CreatureActivate;
                effectData.Creature.PlacementMode = effectData.CreaturePlacementMode;
            }

            if (string.IsNullOrWhiteSpace(effectData.Battle.Reference)
                && effectData.Battle.IncludeActiveCreatures
                && string.IsNullOrWhiteSpace(effectData.Battle.Description)
                && HasBattleContent(effectData.BattleReference, effectData.BattleIncludeActiveCreatures, effectData.BattleDescription))
            {
                effectData.Battle.Reference = effectData.BattleReference ?? string.Empty;
                effectData.Battle.IncludeActiveCreatures = effectData.BattleIncludeActiveCreatures;
                effectData.Battle.Description = effectData.BattleDescription ?? string.Empty;
            }

            effectData.CheckTargetMode = effectData.Check.TargetMode;
            effectData.CheckResolutionMode = effectData.Check.ResolutionMode;
            effectData.SuccessResult = effectData.Check.SuccessResult ?? string.Empty;
            effectData.FailureResult = effectData.Check.FailureResult ?? string.Empty;
            effectData.SkillCheckEntries = CloneRuntimeSkillCheckEntries(effectData.Check.SkillCheckEntries);
            effectData.SkillCheckName = effectData.Check.SkillCheckName ?? string.Empty;
            effectData.SkillCheckThreshold = effectData.Check.SkillCheckThreshold ?? string.Empty;
            effectData.AbilityStrengthThreshold = effectData.Check.AbilityStrengthThreshold ?? string.Empty;
            effectData.AbilityDexterityThreshold = effectData.Check.AbilityDexterityThreshold ?? string.Empty;
            effectData.AbilityConstitutionThreshold = effectData.Check.AbilityConstitutionThreshold ?? string.Empty;
            effectData.AbilityIntelligenceThreshold = effectData.Check.AbilityIntelligenceThreshold ?? string.Empty;
            effectData.AbilityWisdomThreshold = effectData.Check.AbilityWisdomThreshold ?? string.Empty;
            effectData.AbilityCharismaThreshold = effectData.Check.AbilityCharismaThreshold ?? string.Empty;
            effectData.NarrativeText = effectData.Narrative.Text ?? string.Empty;
            effectData.NarrativeDmOnly = effectData.Narrative.DmOnly;
            effectData.DialogueTarget = effectData.Dialogue.Target ?? string.Empty;
            effectData.DialogueSummary = effectData.Dialogue.Summary ?? string.Empty;
            effectData.DialoguePrompt = effectData.Dialogue.Prompt ?? string.Empty;
            effectData.CreatureInstanceId = effectData.Creature.InstanceId ?? string.Empty;
            effectData.CreatureActivate = effectData.Creature.Activate;
            effectData.CreaturePlacementMode = effectData.Creature.PlacementMode;
            effectData.BattleReference = effectData.Battle.Reference ?? string.Empty;
            effectData.BattleIncludeActiveCreatures = effectData.Battle.IncludeActiveCreatures;
            effectData.BattleDescription = effectData.Battle.Description ?? string.Empty;
            effectData.EffectType = ResolveRuntimeEffectType(
                effectData.EffectType,
                -1,
                -1,
                0,
                effectData.NarrativeText,
                effectData.DialogueTarget,
                effectData.DialogueSummary,
                effectData.DialoguePrompt,
                effectData.CreatureInstanceId,
                effectData.CreatureActivate,
                effectData.CreaturePlacementMode,
                effectData.BattleReference,
                effectData.BattleIncludeActiveCreatures,
                effectData.BattleDescription,
                effectData.SkillCheckEntries,
                effectData.SkillCheckName,
                effectData.SkillCheckThreshold,
                effectData.AbilityStrengthThreshold,
                effectData.AbilityDexterityThreshold,
                effectData.AbilityConstitutionThreshold,
                effectData.AbilityIntelligenceThreshold,
                effectData.AbilityWisdomThreshold,
                effectData.AbilityCharismaThreshold,
                effectData.SuccessResult,
                effectData.FailureResult,
                effectData.LegacyDmPrompt);
            return effectData;
        }

        private static ChapterEventTriggerSaveData NormalizeSaveTriggerData(ChapterEventTriggerSaveData source)
        {
            ChapterEventTriggerSaveData triggerData = CloneSaveTrigger(source);
            if (!triggerData.Area.FirstEnterOnly && !triggerData.Area.ShareBinding
                && (triggerData.AreaFirstEnterOnly || triggerData.AreaShareBinding))
            {
                triggerData.Area.FirstEnterOnly = triggerData.AreaFirstEnterOnly;
                triggerData.Area.ShareBinding = triggerData.AreaShareBinding;
            }

            if (string.IsNullOrWhiteSpace(triggerData.Interaction.Target)
                && !triggerData.Interaction.RequireConfirm
                && (!string.IsNullOrWhiteSpace(triggerData.InteractionTarget) || triggerData.InteractionRequireConfirm))
            {
                triggerData.Interaction.Target = triggerData.InteractionTarget ?? string.Empty;
                triggerData.Interaction.RequireConfirm = triggerData.InteractionRequireConfirm;
            }

            if (string.IsNullOrWhiteSpace(triggerData.Prerequisite.EventId)
                && string.IsNullOrWhiteSpace(triggerData.Prerequisite.DelayDescription)
                && (!string.IsNullOrWhiteSpace(triggerData.PrerequisiteEventId) || !string.IsNullOrWhiteSpace(triggerData.DelayDescription)))
            {
                triggerData.Prerequisite.EventId = triggerData.PrerequisiteEventId ?? string.Empty;
                triggerData.Prerequisite.DelayDescription = triggerData.DelayDescription ?? string.Empty;
            }

            triggerData.AreaFirstEnterOnly = triggerData.Area.FirstEnterOnly;
            triggerData.AreaShareBinding = triggerData.Area.ShareBinding;
            triggerData.InteractionTarget = triggerData.Interaction.Target ?? string.Empty;
            triggerData.InteractionRequireConfirm = triggerData.Interaction.RequireConfirm;
            triggerData.PrerequisiteEventId = triggerData.Prerequisite.EventId ?? string.Empty;
            triggerData.DelayDescription = triggerData.Prerequisite.DelayDescription ?? string.Empty;
            triggerData.TriggerType = ResolveRuntimeTriggerType(
                triggerData.TriggerType,
                triggerData.TriggerMode,
                triggerData.InteractionTarget,
                triggerData.PrerequisiteEventId);
            triggerData.TriggerMode = ResolveLegacyTriggerMode(triggerData.TriggerType, triggerData.TriggerMode);
            return triggerData;
        }

        private static ChapterEventEffectSaveData NormalizeSaveEffectData(
            ChapterEventEffectSaveData source,
            int legacyEffectType = -1,
            int legacyEventCategory = -1,
            int legacyEventSubType = 0)
        {
            ChapterEventEffectSaveData effectData = CloneSaveEffect(source);
            if (!HasCheckContent(
                    effectData.Check.SkillCheckEntries,
                    effectData.Check.SkillCheckName,
                    effectData.Check.SkillCheckThreshold,
                    effectData.Check.AbilityStrengthThreshold,
                    effectData.Check.AbilityDexterityThreshold,
                    effectData.Check.AbilityConstitutionThreshold,
                    effectData.Check.AbilityIntelligenceThreshold,
                    effectData.Check.AbilityWisdomThreshold,
                    effectData.Check.AbilityCharismaThreshold,
                    effectData.Check.SuccessResult,
                    effectData.Check.FailureResult)
                && HasCheckContent(
                    effectData.SkillCheckEntries,
                    effectData.SkillCheckName,
                    effectData.SkillCheckThreshold,
                    effectData.AbilityStrengthThreshold,
                    effectData.AbilityDexterityThreshold,
                    effectData.AbilityConstitutionThreshold,
                    effectData.AbilityIntelligenceThreshold,
                    effectData.AbilityWisdomThreshold,
                    effectData.AbilityCharismaThreshold,
                    effectData.SuccessResult,
                    effectData.FailureResult))
            {
                effectData.Check.TargetMode = effectData.CheckTargetMode;
                effectData.Check.ResolutionMode = effectData.CheckResolutionMode;
                effectData.Check.SuccessResult = effectData.SuccessResult ?? string.Empty;
                effectData.Check.FailureResult = effectData.FailureResult ?? string.Empty;
                effectData.Check.SkillCheckEntries = CloneSaveSkillCheckEntries(effectData.SkillCheckEntries);
                effectData.Check.SkillCheckName = effectData.SkillCheckName ?? string.Empty;
                effectData.Check.SkillCheckThreshold = effectData.SkillCheckThreshold ?? string.Empty;
                effectData.Check.AbilityStrengthThreshold = effectData.AbilityStrengthThreshold ?? string.Empty;
                effectData.Check.AbilityDexterityThreshold = effectData.AbilityDexterityThreshold ?? string.Empty;
                effectData.Check.AbilityConstitutionThreshold = effectData.AbilityConstitutionThreshold ?? string.Empty;
                effectData.Check.AbilityIntelligenceThreshold = effectData.AbilityIntelligenceThreshold ?? string.Empty;
                effectData.Check.AbilityWisdomThreshold = effectData.AbilityWisdomThreshold ?? string.Empty;
                effectData.Check.AbilityCharismaThreshold = effectData.AbilityCharismaThreshold ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(effectData.Narrative.Text)
                && !effectData.Narrative.DmOnly
                && (!string.IsNullOrWhiteSpace(effectData.NarrativeText) || effectData.NarrativeDmOnly))
            {
                effectData.Narrative.Text = effectData.NarrativeText ?? string.Empty;
                effectData.Narrative.DmOnly = effectData.NarrativeDmOnly;
            }

            if (string.IsNullOrWhiteSpace(effectData.Dialogue.Target)
                && string.IsNullOrWhiteSpace(effectData.Dialogue.Summary)
                && string.IsNullOrWhiteSpace(effectData.Dialogue.Prompt)
                && (!string.IsNullOrWhiteSpace(effectData.DialogueTarget)
                    || !string.IsNullOrWhiteSpace(effectData.DialogueSummary)
                    || !string.IsNullOrWhiteSpace(effectData.DialoguePrompt)))
            {
                effectData.Dialogue.Target = effectData.DialogueTarget ?? string.Empty;
                effectData.Dialogue.Summary = effectData.DialogueSummary ?? string.Empty;
                effectData.Dialogue.Prompt = effectData.DialoguePrompt ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(effectData.Creature.InstanceId)
                && effectData.Creature.Activate
                && effectData.Creature.PlacementMode == 0
                && HasCreatureActivationContent(effectData.CreatureInstanceId, effectData.CreatureActivate, effectData.CreaturePlacementMode))
            {
                effectData.Creature.InstanceId = effectData.CreatureInstanceId ?? string.Empty;
                effectData.Creature.Activate = effectData.CreatureActivate;
                effectData.Creature.PlacementMode = effectData.CreaturePlacementMode;
            }

            if (string.IsNullOrWhiteSpace(effectData.Battle.Reference)
                && effectData.Battle.IncludeActiveCreatures
                && string.IsNullOrWhiteSpace(effectData.Battle.Description)
                && HasBattleContent(effectData.BattleReference, effectData.BattleIncludeActiveCreatures, effectData.BattleDescription))
            {
                effectData.Battle.Reference = effectData.BattleReference ?? string.Empty;
                effectData.Battle.IncludeActiveCreatures = effectData.BattleIncludeActiveCreatures;
                effectData.Battle.Description = effectData.BattleDescription ?? string.Empty;
            }

            effectData.CheckTargetMode = effectData.Check.TargetMode;
            effectData.CheckResolutionMode = effectData.Check.ResolutionMode;
            effectData.SuccessResult = effectData.Check.SuccessResult ?? string.Empty;
            effectData.FailureResult = effectData.Check.FailureResult ?? string.Empty;
            effectData.SkillCheckEntries = CloneSaveSkillCheckEntries(effectData.Check.SkillCheckEntries);
            effectData.SkillCheckName = effectData.Check.SkillCheckName ?? string.Empty;
            effectData.SkillCheckThreshold = effectData.Check.SkillCheckThreshold ?? string.Empty;
            effectData.AbilityStrengthThreshold = effectData.Check.AbilityStrengthThreshold ?? string.Empty;
            effectData.AbilityDexterityThreshold = effectData.Check.AbilityDexterityThreshold ?? string.Empty;
            effectData.AbilityConstitutionThreshold = effectData.Check.AbilityConstitutionThreshold ?? string.Empty;
            effectData.AbilityIntelligenceThreshold = effectData.Check.AbilityIntelligenceThreshold ?? string.Empty;
            effectData.AbilityWisdomThreshold = effectData.Check.AbilityWisdomThreshold ?? string.Empty;
            effectData.AbilityCharismaThreshold = effectData.Check.AbilityCharismaThreshold ?? string.Empty;
            effectData.NarrativeText = effectData.Narrative.Text ?? string.Empty;
            effectData.NarrativeDmOnly = effectData.Narrative.DmOnly;
            effectData.DialogueTarget = effectData.Dialogue.Target ?? string.Empty;
            effectData.DialogueSummary = effectData.Dialogue.Summary ?? string.Empty;
            effectData.DialoguePrompt = effectData.Dialogue.Prompt ?? string.Empty;
            effectData.CreatureInstanceId = effectData.Creature.InstanceId ?? string.Empty;
            effectData.CreatureActivate = effectData.Creature.Activate;
            effectData.CreaturePlacementMode = effectData.Creature.PlacementMode;
            effectData.BattleReference = effectData.Battle.Reference ?? string.Empty;
            effectData.BattleIncludeActiveCreatures = effectData.Battle.IncludeActiveCreatures;
            effectData.BattleDescription = effectData.Battle.Description ?? string.Empty;
            effectData.EffectType = ResolveRuntimeEffectType(
                effectData.EffectType,
                legacyEffectType,
                legacyEventCategory,
                legacyEventSubType,
                effectData.NarrativeText,
                effectData.DialogueTarget,
                effectData.DialogueSummary,
                effectData.DialoguePrompt,
                effectData.CreatureInstanceId,
                effectData.CreatureActivate,
                effectData.CreaturePlacementMode,
                effectData.BattleReference,
                effectData.BattleIncludeActiveCreatures,
                effectData.BattleDescription,
                effectData.SkillCheckEntries,
                effectData.SkillCheckName,
                effectData.SkillCheckThreshold,
                effectData.AbilityStrengthThreshold,
                effectData.AbilityDexterityThreshold,
                effectData.AbilityConstitutionThreshold,
                effectData.AbilityIntelligenceThreshold,
                effectData.AbilityWisdomThreshold,
                effectData.AbilityCharismaThreshold,
                effectData.SuccessResult,
                effectData.FailureResult,
                effectData.LegacyDmPrompt);
            return effectData;
        }

        private static int ResolveRuntimeTriggerType(int structuredTriggerType, int structuredTriggerMode, string interactionTarget, string prerequisiteEventId)
        {
            if (structuredTriggerType >= RuntimeTriggerTypeDmManual && structuredTriggerType <= RuntimeTriggerTypeAfterPrerequisiteEvent)
            {
                return structuredTriggerType;
            }

            if (!string.IsNullOrWhiteSpace(interactionTarget))
            {
                return RuntimeTriggerTypeInteractWithSceneObject;
            }

            if (!string.IsNullOrWhiteSpace(prerequisiteEventId))
            {
                return RuntimeTriggerTypeAfterPrerequisiteEvent;
            }

            return structuredTriggerMode == LegacyTriggerModeDmManual
                ? RuntimeTriggerTypeDmManual
                : RuntimeTriggerTypeEnterBindingArea;
        }

        private static int ResolveLegacyTriggerMode(int triggerType, int structuredTriggerMode)
        {
            if (structuredTriggerMode == LegacyTriggerModeDmManual || structuredTriggerMode == LegacyTriggerModeAutomatic)
            {
                return triggerType == RuntimeTriggerTypeDmManual
                    ? LegacyTriggerModeDmManual
                    : LegacyTriggerModeAutomatic;
            }

            return triggerType == RuntimeTriggerTypeDmManual
                ? LegacyTriggerModeDmManual
                : LegacyTriggerModeAutomatic;
        }

        private static int ResolveRuntimeEffectType(
            int structuredEffectType,
            int legacyEffectType,
            int legacyEventCategory,
            int legacyEventSubType,
            string narrativeText,
            string dialogueTarget,
            string dialogueSummary,
            string dialoguePrompt,
            string creatureInstanceId,
            bool creatureActivate,
            int creaturePlacementMode,
            string battleReference,
            bool battleIncludeActiveCreatures,
            string battleDescription,
            System.Collections.ICollection skillCheckEntries,
            string skillCheckName,
            string skillCheckThreshold,
            string abilityStrengthThreshold,
            string abilityDexterityThreshold,
            string abilityConstitutionThreshold,
            string abilityIntelligenceThreshold,
            string abilityWisdomThreshold,
            string abilityCharismaThreshold,
            string successResult,
            string failureResult,
            string legacyDmPrompt)
        {
            if (structuredEffectType >= RuntimeEffectTypeCheck && structuredEffectType <= RuntimeEffectTypeStartBattle)
            {
                return structuredEffectType;
            }

            if (legacyEffectType >= RuntimeEffectTypeCheck && legacyEffectType <= RuntimeEffectTypeStartBattle)
            {
                return legacyEffectType;
            }

            if (HasCreatureActivationContent(creatureInstanceId, creatureActivate, creaturePlacementMode))
            {
                return RuntimeEffectTypeActivateCreatureInstance;
            }

            if (HasBattleContent(battleReference, battleIncludeActiveCreatures, battleDescription))
            {
                return RuntimeEffectTypeStartBattle;
            }

            if (HasDialogueContent(dialogueTarget, dialogueSummary, dialoguePrompt))
            {
                return RuntimeEffectTypeDialogueInteractionPrompt;
            }

            if (HasCheckContent(
                    skillCheckEntries,
                    skillCheckName,
                    skillCheckThreshold,
                    abilityStrengthThreshold,
                    abilityDexterityThreshold,
                    abilityConstitutionThreshold,
                    abilityIntelligenceThreshold,
                    abilityWisdomThreshold,
                    abilityCharismaThreshold,
                    successResult,
                    failureResult))
            {
                return RuntimeEffectTypeCheck;
            }

            if (legacyEventCategory == LegacyCheckEventCategory)
            {
                return RuntimeEffectTypeCheck;
            }

            if (legacyEventCategory == LegacyDmDirectEventCategory)
            {
                switch (legacyEventSubType)
                {
                    case LegacyDmEventSubTypeDialogue:
                    case LegacyDmEventSubTypeChoice:
                    case LegacyDmEventSubTypeInteraction:
                        return RuntimeEffectTypeDialogueInteractionPrompt;
                    case LegacyDmEventSubTypeCombat:
                        return RuntimeEffectTypeStartBattle;
                    default:
                        return RuntimeEffectTypeNarrativePrompt;
                }
            }

            return !string.IsNullOrWhiteSpace(narrativeText) || !string.IsNullOrWhiteSpace(legacyDmPrompt)
                ? RuntimeEffectTypeNarrativePrompt
                : RuntimeEffectTypeCheck;
        }

        private static bool HasCreatureActivationContent(string creatureInstanceId, bool creatureActivate, int creaturePlacementMode)
        {
            return !string.IsNullOrWhiteSpace(creatureInstanceId)
                || !creatureActivate
                || creaturePlacementMode != 0;
        }

        private static bool HasBattleContent(string battleReference, bool battleIncludeActiveCreatures, string battleDescription)
        {
            return !string.IsNullOrWhiteSpace(battleReference)
                || !battleIncludeActiveCreatures
                || !string.IsNullOrWhiteSpace(battleDescription);
        }

        private static bool HasDialogueContent(string dialogueTarget, string dialogueSummary, string dialoguePrompt)
        {
            return !string.IsNullOrWhiteSpace(dialogueTarget)
                || !string.IsNullOrWhiteSpace(dialogueSummary)
                || !string.IsNullOrWhiteSpace(dialoguePrompt);
        }

        private static bool HasCheckContent(
            System.Collections.ICollection skillCheckEntries,
            string skillCheckName,
            string skillCheckThreshold,
            string abilityStrengthThreshold,
            string abilityDexterityThreshold,
            string abilityConstitutionThreshold,
            string abilityIntelligenceThreshold,
            string abilityWisdomThreshold,
            string abilityCharismaThreshold,
            string successResult,
            string failureResult)
        {
            return (skillCheckEntries != null && skillCheckEntries.Count > 0)
                || !string.IsNullOrWhiteSpace(skillCheckName)
                || !string.IsNullOrWhiteSpace(skillCheckThreshold)
                || !string.IsNullOrWhiteSpace(abilityStrengthThreshold)
                || !string.IsNullOrWhiteSpace(abilityDexterityThreshold)
                || !string.IsNullOrWhiteSpace(abilityConstitutionThreshold)
                || !string.IsNullOrWhiteSpace(abilityIntelligenceThreshold)
                || !string.IsNullOrWhiteSpace(abilityWisdomThreshold)
                || !string.IsNullOrWhiteSpace(abilityCharismaThreshold)
                || !string.IsNullOrWhiteSpace(successResult)
                || !string.IsNullOrWhiteSpace(failureResult);
        }

        private static bool IsLegacySaveTriggerStructureMissing(ChapterGridEventLegacySaveData eventData)
        {
            ChapterEventTriggerSaveData trigger = eventData.Trigger;
            ChapterEventAreaTriggerParamSaveData areaTriggerParam = trigger?.Area;
            ChapterEventInteractionTriggerParamSaveData interactionTriggerParam = trigger?.Interaction;
            ChapterEventPrerequisiteTriggerParamSaveData prerequisiteTriggerParam = trigger?.Prerequisite;
            return trigger == null
                || (trigger.TriggerType < 0
                    && trigger.TriggerMode == 0
                    && !trigger.AreaFirstEnterOnly
                    && !trigger.AreaShareBinding
                    && string.IsNullOrWhiteSpace(trigger.InteractionTarget)
                    && !trigger.InteractionRequireConfirm
                    && string.IsNullOrWhiteSpace(trigger.PrerequisiteEventId)
                    && string.IsNullOrWhiteSpace(trigger.DelayDescription)
                    && (areaTriggerParam == null || (!areaTriggerParam.FirstEnterOnly && !areaTriggerParam.ShareBinding))
                    && (interactionTriggerParam == null || (string.IsNullOrWhiteSpace(interactionTriggerParam.Target) && !interactionTriggerParam.RequireConfirm))
                    && (prerequisiteTriggerParam == null || (string.IsNullOrWhiteSpace(prerequisiteTriggerParam.EventId) && string.IsNullOrWhiteSpace(prerequisiteTriggerParam.DelayDescription))));
        }

        private static bool IsLegacySaveEffectStructureMissing(ChapterGridEventLegacySaveData eventData)
        {
            ChapterEventEffectSaveData effect = eventData.Effect;
            ChapterEventCheckEffectParamSaveData checkEffectParam = effect?.Check;
            ChapterEventNarrativeEffectParamSaveData narrativeEffectParam = effect?.Narrative;
            ChapterEventDialogueEffectParamSaveData dialogueEffectParam = effect?.Dialogue;
            ChapterEventCreatureEffectParamSaveData creatureEffectParam = effect?.Creature;
            ChapterEventBattleEffectParamSaveData battleEffectParam = effect?.Battle;
            return effect == null
                || (effect.EffectType < 0
                    && effect.CheckTargetMode == 0
                    && effect.CheckResolutionMode == 0
                    && string.IsNullOrWhiteSpace(effect.SuccessResult)
                    && string.IsNullOrWhiteSpace(effect.FailureResult)
                    && string.IsNullOrWhiteSpace(effect.LegacyDmPrompt)
                    && string.IsNullOrWhiteSpace(effect.NarrativeText)
                    && !effect.NarrativeDmOnly
                    && string.IsNullOrWhiteSpace(effect.DialogueTarget)
                    && string.IsNullOrWhiteSpace(effect.DialogueSummary)
                    && string.IsNullOrWhiteSpace(effect.DialoguePrompt)
                    && string.IsNullOrWhiteSpace(effect.CreatureInstanceId)
                    && effect.CreatureActivate
                    && effect.CreaturePlacementMode == 0
                    && string.IsNullOrWhiteSpace(effect.BattleReference)
                    && effect.BattleIncludeActiveCreatures
                    && string.IsNullOrWhiteSpace(effect.BattleDescription)
                    && (effect.SkillCheckEntries == null || effect.SkillCheckEntries.Count <= 0)
                    && string.IsNullOrWhiteSpace(effect.SkillCheckName)
                    && string.IsNullOrWhiteSpace(effect.SkillCheckThreshold)
                    && string.IsNullOrWhiteSpace(effect.AbilityStrengthThreshold)
                    && string.IsNullOrWhiteSpace(effect.AbilityDexterityThreshold)
                    && string.IsNullOrWhiteSpace(effect.AbilityConstitutionThreshold)
                    && string.IsNullOrWhiteSpace(effect.AbilityIntelligenceThreshold)
                    && string.IsNullOrWhiteSpace(effect.AbilityWisdomThreshold)
                    && string.IsNullOrWhiteSpace(effect.AbilityCharismaThreshold)
                    && (checkEffectParam == null || !HasCheckContent(
                        checkEffectParam.SkillCheckEntries,
                        checkEffectParam.SkillCheckName,
                        checkEffectParam.SkillCheckThreshold,
                        checkEffectParam.AbilityStrengthThreshold,
                        checkEffectParam.AbilityDexterityThreshold,
                        checkEffectParam.AbilityConstitutionThreshold,
                        checkEffectParam.AbilityIntelligenceThreshold,
                        checkEffectParam.AbilityWisdomThreshold,
                        checkEffectParam.AbilityCharismaThreshold,
                        checkEffectParam.SuccessResult,
                        checkEffectParam.FailureResult))
                    && (narrativeEffectParam == null || (string.IsNullOrWhiteSpace(narrativeEffectParam.Text) && !narrativeEffectParam.DmOnly))
                    && (dialogueEffectParam == null || !HasDialogueContent(dialogueEffectParam.Target, dialogueEffectParam.Summary, dialogueEffectParam.Prompt))
                    && (creatureEffectParam == null || !HasCreatureActivationContent(creatureEffectParam.InstanceId, creatureEffectParam.Activate, creatureEffectParam.PlacementMode))
                    && (battleEffectParam == null || !HasBattleContent(battleEffectParam.Reference, battleEffectParam.IncludeActiveCreatures, battleEffectParam.Description)));
        }

        private static List<ChapterSkillCheckThresholdData> CloneRuntimeSkillCheckEntries(List<ChapterSkillCheckThresholdData> source)
        {
            List<ChapterSkillCheckThresholdData> result = new List<ChapterSkillCheckThresholdData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                ChapterSkillCheckThresholdData entry = source[index];
                if (entry == null)
                {
                    continue;
                }

                result.Add(new ChapterSkillCheckThresholdData
                {
                    SkillName = entry.SkillName ?? string.Empty,
                    Threshold = entry.Threshold ?? string.Empty,
                });
            }

            return result;
        }

        private static List<ChapterSkillCheckThresholdSaveData> CloneSaveSkillCheckEntries(List<ChapterSkillCheckThresholdSaveData> source)
        {
            List<ChapterSkillCheckThresholdSaveData> result = new List<ChapterSkillCheckThresholdSaveData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                ChapterSkillCheckThresholdSaveData entry = source[index];
                if (entry == null)
                {
                    continue;
                }

                result.Add(new ChapterSkillCheckThresholdSaveData
                {
                    SkillName = entry.SkillName ?? string.Empty,
                    Threshold = entry.Threshold ?? string.Empty,
                });
            }

            return result;
        }
    }

    [Serializable]
    internal sealed class ChapterCreatureDataSaveData
    {
        public string CreatureId = string.Empty;
        public string Name = string.Empty;
        public string NameEn = string.Empty;
        public string CreatureType = string.Empty;
        public string CreatureSize = string.Empty;
        public string Alignment = string.Empty;
        public string ChallengeRating = string.Empty;
        public string ExperiencePoints = string.Empty;
        public string ArmorClass = string.Empty;
        public string HitPoints = string.Empty;
        public string Speed = string.Empty;
        public string Strength = string.Empty;
        public string Dexterity = string.Empty;
        public string Constitution = string.Empty;
        public string Intelligence = string.Empty;
        public string Wisdom = string.Empty;
        public string Charisma = string.Empty;
        public string SavingThrows = string.Empty;
        public string Skills = string.Empty;
        public string Senses = string.Empty;
        public string Languages = string.Empty;
        public string DamageResistances = string.Empty;
        public string DamageImmunities = string.Empty;
        public string ConditionImmunities = string.Empty;
        public string Traits = string.Empty;
        public string Actions = string.Empty;
        public string BonusActions = string.Empty;
        public string Reactions = string.Empty;
        public string LegendaryActions = string.Empty;
        public string BattleNotes = string.Empty;
        public string PreviewImageFileName = string.Empty;
        public float AccentColorR = 0.45f;
        public float AccentColorG = 0.55f;
        public float AccentColorB = 0.7f;
        public float AccentColorA = 1f;
    }

    internal sealed class ChapterCreatureData
    {
        public string CreatureId { get; set; } = string.Empty;

        // 标头
        public string Name { get; set; } = string.Empty;

        public string NameEn { get; set; } = string.Empty;

        public string CreatureType { get; set; } = string.Empty;

        public string CreatureSize { get; set; } = string.Empty;

        public string Alignment { get; set; } = string.Empty;

        // 战斗属性
        public string ChallengeRating { get; set; } = string.Empty;

        public string ExperiencePoints { get; set; } = string.Empty;

        public string ArmorClass { get; set; } = string.Empty;

        public string HitPoints { get; set; } = string.Empty;

        public string Speed { get; set; } = string.Empty;

        // 六维属性（独立字段）
        public string Strength { get; set; } = string.Empty;

        public string Dexterity { get; set; } = string.Empty;

        public string Constitution { get; set; } = string.Empty;

        public string Intelligence { get; set; } = string.Empty;

        public string Wisdom { get; set; } = string.Empty;

        public string Charisma { get; set; } = string.Empty;

        // 豁免 / 技能
        public string SavingThrows { get; set; } = string.Empty;

        public string Skills { get; set; } = string.Empty;

        // 感官 / 语言
        public string Senses { get; set; } = string.Empty;

        public string Languages { get; set; } = string.Empty;

        // 防御（拆分）
        public string DamageResistances { get; set; } = string.Empty;

        public string DamageImmunities { get; set; } = string.Empty;

        public string ConditionImmunities { get; set; } = string.Empty;

        // 特性 / 动作（条目间用 --- 分隔）
        public string Traits { get; set; } = string.Empty;

        public string Actions { get; set; } = string.Empty;

        public string BonusActions { get; set; } = string.Empty;

        public string Reactions { get; set; } = string.Empty;

        public string LegendaryActions { get; set; } = string.Empty;

        // DM 私密
        public string BattleNotes { get; set; } = string.Empty;

        // 预览图
        public string PreviewImageFileName { get; set; } = string.Empty;

        public Color AccentColor { get; set; } = new Color(0.45f, 0.55f, 0.7f, 1f);

        public string GetCreatureTypeDisplay()
        {
            string size = string.IsNullOrWhiteSpace(CreatureSize) ? string.Empty : CreatureSize + " ";
            string type = string.IsNullOrWhiteSpace(CreatureType) ? "未分类生物" : CreatureType;
            string cr = string.IsNullOrWhiteSpace(ChallengeRating) ? "-" : ChallengeRating;
            return $"{size}{type} | CR {cr}";
        }

        public string BuildSummary()
        {
            var b = new StringBuilder(1024);

            // 基础战斗数值
            b.Append("AC: ").Append(Fallback(ArmorClass, "-"));
            b.Append("  HP: ").Append(Fallback(HitPoints, "-"));
            b.Append("  速度: ").AppendLine(Fallback(Speed, "-"));

            // 六维
            b.Append("力量 ").Append(Fallback(Strength, "?")).Append("  ");
            b.Append("敏捷 ").Append(Fallback(Dexterity, "?")).Append("  ");
            b.Append("体质 ").Append(Fallback(Constitution, "?")).Append("  ");
            b.Append("智力 ").Append(Fallback(Intelligence, "?")).Append("  ");
            b.Append("感知 ").Append(Fallback(Wisdom, "?")).Append("  ");
            b.Append("魅力 ").AppendLine(Fallback(Charisma, "?"));

            // 豁免 / 技能
            AppendInline(b, "豁免", SavingThrows);
            AppendInline(b, "技能", Skills);

            // 感官 / 语言
            AppendInline(b, "感官", Senses);
            AppendInline(b, "语言", Languages);

            // 防御
            AppendInline(b, "伤害抗性", DamageResistances);
            AppendInline(b, "伤害免疫", DamageImmunities);
            AppendInline(b, "状态免疫", ConditionImmunities);

            // 特性 / 动作块
            AppendBlock(b, "特性", Traits);
            AppendBlock(b, "动作", Actions);
            AppendBlock(b, "附赠动作", BonusActions);
            AppendBlock(b, "反应", Reactions);
            AppendBlock(b, "传奇动作", LegendaryActions);
            AppendBlock(b, "战斗备注", BattleNotes);

            return b.ToString().TrimEnd();
        }

        private static string Fallback(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static void AppendInline(StringBuilder b, string label, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                b.Append(label).Append(": ").AppendLine(value);
            }
        }

        private static void AppendBlock(StringBuilder b, string title, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return;
            }

            b.AppendLine();
            b.Append('【').Append(title).AppendLine("】");
            b.AppendLine(content);
        }
    }

    internal sealed class ChapterCreatureInstancePlacementData
    {
        public ChapterGridCoordinate AnchorCell { get; set; } = ChapterGridCoordinate.Zero;

        public float PreviewScale { get; set; } = 1f;

        public bool SnapToGrid { get; set; } = true;
    }

    internal sealed class ChapterCreatureInstanceData
    {
        public string InstanceId { get; set; } = string.Empty;

        public string SourceCreatureId { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ChapterCreatureInstancePlacementData Placement { get; set; } = new ChapterCreatureInstancePlacementData();

        public ChapterCreatureData RuntimeSheet { get; set; } = new ChapterCreatureData();

        public string DmNote { get; set; } = string.Empty;
    }

    internal static class ChapterCreatureDataStructureUtility
    {
        public static ChapterCreatureData NormalizeCreatureTemplateData(ChapterCreatureData creature)
        {
            if (creature == null)
            {
                return null;
            }

            creature.CreatureId = string.IsNullOrWhiteSpace(creature.CreatureId)
                ? CreateCreatureTemplateId()
                : creature.CreatureId;
            creature.Name ??= string.Empty;
            creature.NameEn ??= string.Empty;
            creature.CreatureType ??= string.Empty;
            creature.CreatureSize ??= string.Empty;
            creature.Alignment ??= string.Empty;
            creature.ChallengeRating ??= string.Empty;
            creature.ExperiencePoints ??= string.Empty;
            creature.ArmorClass ??= string.Empty;
            creature.HitPoints ??= string.Empty;
            creature.Speed ??= string.Empty;
            creature.Strength ??= string.Empty;
            creature.Dexterity ??= string.Empty;
            creature.Constitution ??= string.Empty;
            creature.Intelligence ??= string.Empty;
            creature.Wisdom ??= string.Empty;
            creature.Charisma ??= string.Empty;
            creature.SavingThrows ??= string.Empty;
            creature.Skills ??= string.Empty;
            creature.Senses ??= string.Empty;
            creature.Languages ??= string.Empty;
            creature.DamageResistances ??= string.Empty;
            creature.DamageImmunities ??= string.Empty;
            creature.ConditionImmunities ??= string.Empty;
            creature.Traits ??= string.Empty;
            creature.Actions ??= string.Empty;
            creature.BonusActions ??= string.Empty;
            creature.Reactions ??= string.Empty;
            creature.LegendaryActions ??= string.Empty;
            creature.BattleNotes ??= string.Empty;
            creature.PreviewImageFileName ??= string.Empty;
            return creature;
        }

        public static ChapterCreatureDataSaveData NormalizeCreatureTemplateSaveData(ChapterCreatureDataSaveData creature)
        {
            if (creature == null)
            {
                return null;
            }

            creature.CreatureId = string.IsNullOrWhiteSpace(creature.CreatureId)
                ? CreateCreatureTemplateId()
                : creature.CreatureId;
            creature.Name ??= string.Empty;
            creature.NameEn ??= string.Empty;
            creature.CreatureType ??= string.Empty;
            creature.CreatureSize ??= string.Empty;
            creature.Alignment ??= string.Empty;
            creature.ChallengeRating ??= string.Empty;
            creature.ExperiencePoints ??= string.Empty;
            creature.ArmorClass ??= string.Empty;
            creature.HitPoints ??= string.Empty;
            creature.Speed ??= string.Empty;
            creature.Strength ??= string.Empty;
            creature.Dexterity ??= string.Empty;
            creature.Constitution ??= string.Empty;
            creature.Intelligence ??= string.Empty;
            creature.Wisdom ??= string.Empty;
            creature.Charisma ??= string.Empty;
            creature.SavingThrows ??= string.Empty;
            creature.Skills ??= string.Empty;
            creature.Senses ??= string.Empty;
            creature.Languages ??= string.Empty;
            creature.DamageResistances ??= string.Empty;
            creature.DamageImmunities ??= string.Empty;
            creature.ConditionImmunities ??= string.Empty;
            creature.Traits ??= string.Empty;
            creature.Actions ??= string.Empty;
            creature.BonusActions ??= string.Empty;
            creature.Reactions ??= string.Empty;
            creature.LegendaryActions ??= string.Empty;
            creature.BattleNotes ??= string.Empty;
            creature.PreviewImageFileName ??= string.Empty;
            return creature;
        }

        public static ChapterCreatureInstanceData NormalizeCreatureInstanceData(ChapterCreatureInstanceData creature)
        {
            if (creature == null)
            {
                return null;
            }

            creature.InstanceId = string.IsNullOrWhiteSpace(creature.InstanceId)
                ? CreateCreatureInstanceId()
                : creature.InstanceId;
            creature.Placement ??= new ChapterCreatureInstancePlacementData();
            creature.RuntimeSheet ??= new ChapterCreatureData();
            creature.RuntimeSheet = NormalizeCreatureTemplateData(creature.RuntimeSheet);
            creature.Placement.PreviewScale = creature.Placement.PreviewScale <= 0f ? 1f : creature.Placement.PreviewScale;
            creature.SourceCreatureId = string.IsNullOrWhiteSpace(creature.SourceCreatureId)
                ? creature.RuntimeSheet.CreatureId
                : creature.SourceCreatureId;
            creature.DmNote ??= string.Empty;
            return creature;
        }

        public static ChapterCreatureInstanceSaveData NormalizeCreatureInstanceSaveData(ChapterCreatureInstanceSaveData creature)
        {
            if (creature == null)
            {
                return null;
            }

            creature.InstanceId = string.IsNullOrWhiteSpace(creature.InstanceId)
                ? CreateCreatureInstanceId()
                : creature.InstanceId;
            creature.Placement ??= new ChapterCreatureInstancePlacementSaveData();
            creature.RuntimeSheet ??= new ChapterCreatureDataSaveData();
            creature.RuntimeSheet = NormalizeCreatureTemplateSaveData(creature.RuntimeSheet);
            creature.Placement.PreviewScale = creature.Placement.PreviewScale <= 0f ? 1f : creature.Placement.PreviewScale;
            creature.SourceCreatureId = string.IsNullOrWhiteSpace(creature.SourceCreatureId)
                ? creature.RuntimeSheet.CreatureId
                : creature.SourceCreatureId;
            creature.DmNote ??= string.Empty;
            return creature;
        }

        public static ChapterCreatureData CloneCreatureData(ChapterCreatureData source)
        {
            if (source == null)
            {
                return null;
            }

            return NormalizeCreatureTemplateData(new ChapterCreatureData
            {
                CreatureId = source.CreatureId,
                Name = source.Name,
                NameEn = source.NameEn,
                CreatureType = source.CreatureType,
                CreatureSize = source.CreatureSize,
                Alignment = source.Alignment,
                ChallengeRating = source.ChallengeRating,
                ExperiencePoints = source.ExperiencePoints,
                ArmorClass = source.ArmorClass,
                HitPoints = source.HitPoints,
                Speed = source.Speed,
                Strength = source.Strength,
                Dexterity = source.Dexterity,
                Constitution = source.Constitution,
                Intelligence = source.Intelligence,
                Wisdom = source.Wisdom,
                Charisma = source.Charisma,
                SavingThrows = source.SavingThrows,
                Skills = source.Skills,
                Senses = source.Senses,
                Languages = source.Languages,
                DamageResistances = source.DamageResistances,
                DamageImmunities = source.DamageImmunities,
                ConditionImmunities = source.ConditionImmunities,
                Traits = source.Traits,
                Actions = source.Actions,
                BonusActions = source.BonusActions,
                Reactions = source.Reactions,
                LegendaryActions = source.LegendaryActions,
                BattleNotes = source.BattleNotes,
                PreviewImageFileName = source.PreviewImageFileName,
                AccentColor = source.AccentColor,
            });
        }

        public static ChapterCreatureDataSaveData CloneCreatureSaveData(ChapterCreatureDataSaveData source)
        {
            if (source == null)
            {
                return null;
            }

            return NormalizeCreatureTemplateSaveData(new ChapterCreatureDataSaveData
            {
                CreatureId = source.CreatureId,
                Name = source.Name,
                NameEn = source.NameEn,
                CreatureType = source.CreatureType,
                CreatureSize = source.CreatureSize,
                Alignment = source.Alignment,
                ChallengeRating = source.ChallengeRating,
                ExperiencePoints = source.ExperiencePoints,
                ArmorClass = source.ArmorClass,
                HitPoints = source.HitPoints,
                Speed = source.Speed,
                Strength = source.Strength,
                Dexterity = source.Dexterity,
                Constitution = source.Constitution,
                Intelligence = source.Intelligence,
                Wisdom = source.Wisdom,
                Charisma = source.Charisma,
                SavingThrows = source.SavingThrows,
                Skills = source.Skills,
                Senses = source.Senses,
                Languages = source.Languages,
                DamageResistances = source.DamageResistances,
                DamageImmunities = source.DamageImmunities,
                ConditionImmunities = source.ConditionImmunities,
                Traits = source.Traits,
                Actions = source.Actions,
                BonusActions = source.BonusActions,
                Reactions = source.Reactions,
                LegendaryActions = source.LegendaryActions,
                BattleNotes = source.BattleNotes,
                PreviewImageFileName = source.PreviewImageFileName,
                AccentColorR = source.AccentColorR,
                AccentColorG = source.AccentColorG,
                AccentColorB = source.AccentColorB,
                AccentColorA = source.AccentColorA,
            });
        }

        public static List<ChapterCreatureData> CloneCreatureDataList(List<ChapterCreatureData> source)
        {
            List<ChapterCreatureData> result = new List<ChapterCreatureData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                ChapterCreatureData creature = CloneCreatureData(source[index]);
                if (creature != null)
                {
                    result.Add(creature);
                }
            }

            return result;
        }

        public static List<ChapterCreatureDataSaveData> CloneCreatureSaveDataList(List<ChapterCreatureDataSaveData> source)
        {
            List<ChapterCreatureDataSaveData> result = new List<ChapterCreatureDataSaveData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                ChapterCreatureDataSaveData creature = CloneCreatureSaveData(source[index]);
                if (creature != null)
                {
                    result.Add(creature);
                }
            }

            return result;
        }

        public static ChapterCreatureInstanceData CloneCreatureInstanceData(ChapterCreatureInstanceData source)
        {
            if (source == null)
            {
                return null;
            }

            return NormalizeCreatureInstanceData(new ChapterCreatureInstanceData
            {
                InstanceId = source.InstanceId,
                SourceCreatureId = source.SourceCreatureId,
                IsActive = source.IsActive,
                Placement = source.Placement != null
                    ? new ChapterCreatureInstancePlacementData
                    {
                        AnchorCell = source.Placement.AnchorCell,
                        PreviewScale = source.Placement.PreviewScale,
                        SnapToGrid = source.Placement.SnapToGrid,
                    }
                    : new ChapterCreatureInstancePlacementData(),
                RuntimeSheet = CloneCreatureData(source.RuntimeSheet),
                DmNote = source.DmNote,
            });
        }

        public static ChapterCreatureInstanceSaveData CloneCreatureInstanceSaveData(ChapterCreatureInstanceSaveData source)
        {
            if (source == null)
            {
                return null;
            }

            return NormalizeCreatureInstanceSaveData(new ChapterCreatureInstanceSaveData
            {
                InstanceId = source.InstanceId,
                SourceCreatureId = source.SourceCreatureId,
                IsActive = source.IsActive,
                Placement = source.Placement != null
                    ? new ChapterCreatureInstancePlacementSaveData
                    {
                        AnchorCellX = source.Placement.AnchorCellX,
                        AnchorCellY = source.Placement.AnchorCellY,
                        PreviewScale = source.Placement.PreviewScale,
                        SnapToGrid = source.Placement.SnapToGrid,
                    }
                    : new ChapterCreatureInstancePlacementSaveData(),
                RuntimeSheet = CloneCreatureSaveData(source.RuntimeSheet),
                DmNote = source.DmNote,
            });
        }

        public static List<ChapterCreatureInstanceData> CloneCreatureInstanceDataList(List<ChapterCreatureInstanceData> source)
        {
            List<ChapterCreatureInstanceData> result = new List<ChapterCreatureInstanceData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                ChapterCreatureInstanceData creature = CloneCreatureInstanceData(source[index]);
                if (creature != null)
                {
                    result.Add(creature);
                }
            }

            return result;
        }

        public static List<ChapterCreatureInstanceSaveData> CloneCreatureInstanceSaveDataList(List<ChapterCreatureInstanceSaveData> source)
        {
            List<ChapterCreatureInstanceSaveData> result = new List<ChapterCreatureInstanceSaveData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                ChapterCreatureInstanceSaveData creature = CloneCreatureInstanceSaveData(source[index]);
                if (creature != null)
                {
                    result.Add(creature);
                }
            }

            return result;
        }

        public static int GetCreatureFootprintCellSpan(ChapterCreatureData creature)
        {
            return GetCreatureFootprintCellSpan(creature?.CreatureSize);
        }

        public static int GetCreatureFootprintCellSpan(string creatureSize)
        {
            if (string.IsNullOrWhiteSpace(creatureSize))
            {
                return 1;
            }

            string normalized = creatureSize.Trim();
            if (ContainsIgnoreCase(normalized, "gargantuan")
                || normalized.Contains("超巨", StringComparison.Ordinal)
                || normalized.Contains("巨兽", StringComparison.Ordinal))
            {
                return 4;
            }

            if (ContainsIgnoreCase(normalized, "huge")
                || normalized.Contains("超大型", StringComparison.Ordinal)
                || normalized.Contains("巨型", StringComparison.Ordinal))
            {
                return 3;
            }

            if (ContainsIgnoreCase(normalized, "large")
                || normalized.Contains("大型", StringComparison.Ordinal))
            {
                return 2;
            }

            return 1;
        }

        private static bool ContainsIgnoreCase(string source, string value)
        {
            return !string.IsNullOrWhiteSpace(source)
                && !string.IsNullOrWhiteSpace(value)
                && source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string CreateCreatureTemplateId()
        {
            return $"cre_{Guid.NewGuid():N}";
        }

        private static string CreateCreatureInstanceId()
        {
            return $"ins_{Guid.NewGuid():N}";
        }
    }
}
