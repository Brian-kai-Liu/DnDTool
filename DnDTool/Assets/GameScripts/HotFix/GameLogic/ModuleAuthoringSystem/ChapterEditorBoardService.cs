using System;
using System.Collections.Generic;

namespace GameLogic
{
    internal static class ChapterEditorBoardService
    {
        public static int ApplyTerrainMarkToSelectedCells(ChapterListItemData chapter, ChapterGridCellMarkType targetMarkType)
        {
            if (chapter == null)
            {
                return 0;
            }

            chapter.GridCells ??= new List<ChapterGridCellData>();
            return ChapterGridCellCollectionUtility.ApplyMarkTypeToCellsBySourceMark(
                chapter.GridCells,
                ChapterGridCellMarkType.Selected,
                targetMarkType);
        }

        public static int ClearTerrainMarksAtSelectedCells(ChapterListItemData chapter)
        {
            if (chapter == null)
            {
                return 0;
            }

            chapter.GridCells ??= new List<ChapterGridCellData>();
            return ChapterGridCellCollectionUtility.ClearMarksAtSelectedCoordinates(chapter.GridCells);
        }

        public static void ToggleSelectedGridCell(ChapterListItemData chapter, ChapterGridCoordinate coordinate)
        {
            if (chapter == null)
            {
                return;
            }

            chapter.GridCells ??= new List<ChapterGridCellData>();
            ChapterGridCellCollectionUtility.ToggleSelectedCell(chapter.GridCells, coordinate);
        }

        public static bool TryGetSelectedGridCoordinates(ChapterListItemData chapter, out List<ChapterGridCoordinate> coordinates)
        {
            coordinates = new List<ChapterGridCoordinate>();
            List<ChapterGridCellData> selectedCells = ChapterGridCellCollectionUtility.GetCellsByMarkType(chapter?.GridCells, ChapterGridCellMarkType.Selected);
            if (selectedCells.Count <= 0)
            {
                return false;
            }

            for (int index = 0; index < selectedCells.Count; index++)
            {
                if (selectedCells[index] == null)
                {
                    continue;
                }

                coordinates.Add(selectedCells[index].Coordinate);
            }

            return coordinates.Count > 0;
        }

        public static bool TryGetEventData(ChapterListItemData chapter, ChapterGridCoordinate coordinate, out ChapterGridEventData eventData)
        {
            eventData = null;
            return chapter != null
                && ChapterEventCollectionUtility.TryGetEventData(chapter.Events, chapter.EventBindings, coordinate, out eventData);
        }

        public static bool HasGridEventAtAnyCoordinate(List<ChapterEventBindingData> eventBindings, List<ChapterGridCoordinate> coordinates)
        {
            if (eventBindings == null || coordinates == null)
            {
                return false;
            }

            for (int index = 0; index < coordinates.Count; index++)
            {
                if (ChapterEventCollectionUtility.HasEventAtCoordinate(eventBindings, coordinates[index]))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ConfirmGridEvent(ChapterListItemData chapter, List<ChapterGridCoordinate> coordinates, ChapterGridEventData eventData)
        {
            if (chapter == null || eventData == null || coordinates == null || coordinates.Count <= 0)
            {
                return false;
            }

            chapter.GridCells ??= new List<ChapterGridCellData>();
            chapter.Events ??= new List<ChapterGridEventData>();
            chapter.EventBindings ??= new List<ChapterEventBindingData>();

            bool updatedExistingSingleEvent = false;
            if (coordinates.Count == 1
                && ChapterEventCollectionUtility.TryGetEventData(chapter.Events, chapter.EventBindings, coordinates[0], out ChapterGridEventData existingEventData)
                && !string.IsNullOrWhiteSpace(existingEventData?.EventId)
                && string.Equals(existingEventData.EventId, eventData.EventId, StringComparison.Ordinal))
            {
                ChapterEventCollectionUtility.UpsertEventDefinition(chapter.Events, eventData);
                updatedExistingSingleEvent = true;
            }

            if (!updatedExistingSingleEvent)
            {
                ChapterEventCollectionUtility.AssignEventToCoordinates(chapter.Events, chapter.EventBindings, coordinates, eventData);
            }

            ChapterGridCellCollectionUtility.ClearSelectedMarks(chapter.GridCells, coordinates);
            return true;
        }

        public static int DeleteGridEvents(ChapterListItemData chapter, List<ChapterGridCoordinate> coordinates)
        {
            if (chapter == null || coordinates == null || coordinates.Count <= 0)
            {
                return 0;
            }

            chapter.GridCells ??= new List<ChapterGridCellData>();
            chapter.Events ??= new List<ChapterGridEventData>();
            chapter.EventBindings ??= new List<ChapterEventBindingData>();

            int removedCount = ChapterEventCollectionUtility.RemoveEventsAtCoordinates(chapter.Events, chapter.EventBindings, coordinates);
            if (removedCount > 0)
            {
                ChapterGridCellCollectionUtility.ClearSelectedMarks(chapter.GridCells, coordinates);
            }

            return removedCount;
        }

        public static ChapterCreatureInstanceData FindCreatureInstanceById(List<ChapterCreatureInstanceData> sourceInstances, string instanceId)
        {
            if (sourceInstances == null || string.IsNullOrWhiteSpace(instanceId))
            {
                return null;
            }

            for (int index = 0; index < sourceInstances.Count; index++)
            {
                ChapterCreatureInstanceData creatureInstance = sourceInstances[index];
                if (creatureInstance != null && string.Equals(creatureInstance.InstanceId, instanceId, StringComparison.Ordinal))
                {
                    return creatureInstance;
                }
            }

            return null;
        }

        public static ChapterCreatureInstanceData CreateCreatureInstanceFromTemplate(ChapterCreatureData creatureTemplate, ChapterGridCoordinate gridCoordinate)
        {
            ChapterCreatureData normalizedTemplate = ChapterCreatureDataStructureUtility.CloneCreatureData(creatureTemplate);
            return ChapterCreatureDataStructureUtility.NormalizeCreatureInstanceData(new ChapterCreatureInstanceData
            {
                SourceCreatureId = normalizedTemplate?.CreatureId ?? string.Empty,
                IsActive = true,
                Placement = new ChapterCreatureInstancePlacementData
                {
                    AnchorCell = gridCoordinate,
                    PreviewScale = 1f,
                    SnapToGrid = true,
                },
                RuntimeSheet = normalizedTemplate,
                DmNote = string.Empty,
            });
        }

        public static bool DeployCreatureInstance(ChapterListItemData chapter, ChapterCreatureData creatureTemplate, ChapterGridCoordinate gridCoordinate)
        {
            if (chapter == null || creatureTemplate == null)
            {
                return false;
            }

            chapter.CreatureInstances ??= new List<ChapterCreatureInstanceData>();
            chapter.CreatureInstances.Add(CreateCreatureInstanceFromTemplate(creatureTemplate, gridCoordinate));
            return true;
        }

        public static bool MoveCreatureInstance(ChapterListItemData chapter, string instanceId, ChapterGridCoordinate gridCoordinate)
        {
            ChapterCreatureInstanceData creatureInstance = FindCreatureInstanceById(chapter?.CreatureInstances, instanceId);
            if (creatureInstance == null)
            {
                return false;
            }

            creatureInstance.Placement ??= new ChapterCreatureInstancePlacementData();
            creatureInstance.Placement.AnchorCell = gridCoordinate;
            return true;
        }

        public static bool UpdateCreatureInstanceRuntimeSheet(ChapterListItemData chapter, string instanceId, ChapterCreatureData updatedData, out string normalizedInstanceId)
        {
            normalizedInstanceId = string.Empty;
            if (string.IsNullOrWhiteSpace(instanceId) || updatedData == null)
            {
                return false;
            }

            ChapterCreatureInstanceData creatureInstance = FindCreatureInstanceById(chapter?.CreatureInstances, instanceId);
            if (creatureInstance == null)
            {
                return false;
            }

            string existingRuntimeCreatureId = creatureInstance.RuntimeSheet?.CreatureId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(updatedData.CreatureId))
            {
                updatedData.CreatureId = existingRuntimeCreatureId;
            }

            creatureInstance.RuntimeSheet = ChapterCreatureDataStructureUtility.CloneCreatureData(updatedData);
            ChapterCreatureDataStructureUtility.NormalizeCreatureInstanceData(creatureInstance);
            normalizedInstanceId = creatureInstance.InstanceId ?? string.Empty;
            return true;
        }

        public static bool ToggleCreatureInstanceActive(ChapterListItemData chapter, string instanceId, out bool isActive)
        {
            isActive = false;
            ChapterCreatureInstanceData creatureInstance = FindCreatureInstanceById(chapter?.CreatureInstances, instanceId);
            if (creatureInstance == null)
            {
                return false;
            }

            creatureInstance.IsActive = !creatureInstance.IsActive;
            isActive = creatureInstance.IsActive;
            return true;
        }

        public static bool DeleteCreatureInstance(ChapterListItemData chapter, string instanceId)
        {
            if (chapter?.CreatureInstances == null || string.IsNullOrWhiteSpace(instanceId))
            {
                return false;
            }

            for (int index = chapter.CreatureInstances.Count - 1; index >= 0; index--)
            {
                ChapterCreatureInstanceData creatureInstance = chapter.CreatureInstances[index];
                if (creatureInstance == null || !string.Equals(creatureInstance.InstanceId, instanceId, StringComparison.Ordinal))
                {
                    continue;
                }

                chapter.CreatureInstances.RemoveAt(index);
                return true;
            }

            return false;
        }
    }
}
