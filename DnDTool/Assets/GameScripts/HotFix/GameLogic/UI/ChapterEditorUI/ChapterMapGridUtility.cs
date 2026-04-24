using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    internal readonly struct ChapterMapGridMetrics
    {
        public ChapterMapGridMetrics(float overlayWidth, float overlayHeight, float cellWidth, float cellHeight, float displayOffsetX, float displayOffsetY, float logicOriginX, float logicOriginY)
        {
            OverlayWidth = overlayWidth;
            OverlayHeight = overlayHeight;
            CellWidth = cellWidth;
            CellHeight = cellHeight;
            DisplayOffsetX = displayOffsetX;
            DisplayOffsetY = displayOffsetY;
            LogicOriginX = logicOriginX;
            LogicOriginY = logicOriginY;
        }

        public float OverlayWidth { get; }

        public float OverlayHeight { get; }

        public float CellWidth { get; }

        public float CellHeight { get; }

        public float DisplayOffsetX { get; }

        public float DisplayOffsetY { get; }

        public float LogicOriginX { get; }

        public float LogicOriginY { get; }
    }

    internal static class ChapterMapGridUtility
    {
        public static ChapterMapGridMetrics CreateMetrics(Rect overlayRect, int baseColumns, int baseRows, float gridZoomScale, Vector2 gridPanOffset)
        {
            float overlayWidth = Mathf.Max(1f, overlayRect.width);
            float overlayHeight = Mathf.Max(1f, overlayRect.height);
            float baseCellWidth = overlayWidth / baseColumns;
            float baseCellHeight = overlayHeight / baseRows;
            float cellWidth = Mathf.Max(8f, baseCellWidth * gridZoomScale);
            float cellHeight = Mathf.Max(8f, baseCellHeight * gridZoomScale);
            float logicOriginX = gridPanOffset.x;
            float logicOriginY = gridPanOffset.y;
            float displayOffsetX = NormalizeDisplayOffset(gridPanOffset.x, cellWidth);
            float displayOffsetY = NormalizeDisplayOffset(gridPanOffset.y, cellHeight);

            return new ChapterMapGridMetrics(overlayWidth, overlayHeight, cellWidth, cellHeight, displayOffsetX, displayOffsetY, logicOriginX, logicOriginY);
        }

        public static bool TryGetCellCoordinateFromLocalPoint(Vector2 localPoint, ChapterMapGridMetrics metrics, out ChapterGridCoordinate coordinate)
        {
            if (metrics.CellWidth <= 0f || metrics.CellHeight <= 0f)
            {
                coordinate = ChapterGridCoordinate.Zero;
                return false;
            }

            int cellX = Mathf.FloorToInt((localPoint.x - metrics.LogicOriginX) / metrics.CellWidth);
            int cellY = Mathf.FloorToInt((localPoint.y - metrics.LogicOriginY) / metrics.CellHeight);
            coordinate = new ChapterGridCoordinate(cellX, cellY);
            return true;
        }

        public static Rect GetLogicalCellRect(ChapterMapGridMetrics metrics, ChapterGridCoordinate coordinate)
        {
            float cellLeft = metrics.LogicOriginX + coordinate.CellX * metrics.CellWidth;
            float cellBottom = metrics.LogicOriginY + coordinate.CellY * metrics.CellHeight;
            return new Rect(cellLeft, cellBottom, metrics.CellWidth, metrics.CellHeight);
        }

        public static float NormalizeDisplayOffset(float offset, float cellLength)
        {
            if (cellLength <= 0f)
            {
                return 0f;
            }

            return Mathf.Repeat(offset + cellLength * 0.5f, cellLength) - cellLength * 0.5f;
        }

        public static List<float> BuildCenteredGridLinePositions(float totalLength, float cellLength, float offset)
        {
            List<float> positions = new List<float>();
            if (totalLength <= 0f || cellLength <= 0f)
            {
                return positions;
            }

            float centerPosition = totalLength * 0.5f + offset;

            for (float position = centerPosition; position > 0f; position -= cellLength)
            {
                if (position > 0f && position < totalLength)
                {
                    positions.Add(position);
                }
            }

            for (float position = centerPosition + cellLength; position < totalLength; position += cellLength)
            {
                if (position > 0f && position < totalLength)
                {
                    positions.Add(position);
                }
            }

            positions.Sort();
            return positions;
        }
    }

    internal static class ChapterGridCellCollectionUtility
    {
        public static List<ChapterGridCellData> Clone(List<ChapterGridCellData> source)
        {
            List<ChapterGridCellData> result = new List<ChapterGridCellData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                ChapterGridCellData gridCell = source[index];
                if (gridCell == null)
                {
                    continue;
                }

                result.Add(new ChapterGridCellData
                {
                    Coordinate = gridCell.Coordinate,
                    MarkType = gridCell.MarkType,
                    EventData = CloneEventData(gridCell.EventData),
                });
            }

            return result;
        }

        public static void ToggleSelectedCell(List<ChapterGridCellData> gridCells, ChapterGridCoordinate coordinate)
        {
            if (gridCells == null)
            {
                return;
            }

            int existingIndex = FindIndex(gridCells, coordinate, ChapterGridCellMarkType.Selected);
            if (existingIndex >= 0)
            {
                gridCells.RemoveAt(existingIndex);
                return;
            }

            gridCells.Add(new ChapterGridCellData
            {
                Coordinate = coordinate,
                MarkType = ChapterGridCellMarkType.Selected,
            });
        }

        public static List<ChapterGridCellData> GetCellsByMarkType(List<ChapterGridCellData> gridCells, ChapterGridCellMarkType markType)
        {
            List<ChapterGridCellData> result = new List<ChapterGridCellData>();
            if (gridCells == null)
            {
                return result;
            }

            for (int index = 0; index < gridCells.Count; index++)
            {
                ChapterGridCellData gridCell = gridCells[index];
                if (gridCell != null && gridCell.MarkType == markType)
                {
                    result.Add(gridCell);
                }
            }

            return result;
        }

        public static bool HasCellsByMarkType(List<ChapterGridCellData> gridCells, ChapterGridCellMarkType markType)
        {
            if (gridCells == null)
            {
                return false;
            }

            for (int index = 0; index < gridCells.Count; index++)
            {
                ChapterGridCellData gridCell = gridCells[index];
                if (gridCell != null && gridCell.MarkType == markType)
                {
                    return true;
                }
            }

            return false;
        }

        public static int ApplyMarkTypeToCellsBySourceMark(List<ChapterGridCellData> gridCells, ChapterGridCellMarkType sourceMarkType, ChapterGridCellMarkType targetMarkType)
        {
            if (gridCells == null)
            {
                return 0;
            }

            List<ChapterGridCoordinate> coordinatesToApply = new List<ChapterGridCoordinate>();
            for (int index = gridCells.Count - 1; index >= 0; index--)
            {
                ChapterGridCellData gridCell = gridCells[index];
                if (gridCell == null || gridCell.MarkType != sourceMarkType)
                {
                    continue;
                }

                coordinatesToApply.Add(gridCell.Coordinate);
                gridCells.RemoveAt(index);
            }

            for (int index = 0; index < coordinatesToApply.Count; index++)
            {
                UpsertMarkType(gridCells, coordinatesToApply[index], targetMarkType);
            }

            return coordinatesToApply.Count;
        }

        public static int ClearMarksAtSelectedCoordinates(List<ChapterGridCellData> gridCells)
        {
            if (gridCells == null)
            {
                return 0;
            }

            HashSet<ChapterGridCoordinate> selectedCoordinates = new HashSet<ChapterGridCoordinate>();
            for (int index = 0; index < gridCells.Count; index++)
            {
                ChapterGridCellData gridCell = gridCells[index];
                if (gridCell != null && gridCell.MarkType == ChapterGridCellMarkType.Selected)
                {
                    selectedCoordinates.Add(gridCell.Coordinate);
                }
            }

            if (selectedCoordinates.Count == 0)
            {
                return 0;
            }

            int removedCount = 0;
            for (int index = gridCells.Count - 1; index >= 0; index--)
            {
                ChapterGridCellData gridCell = gridCells[index];
                if (gridCell == null
                    || !selectedCoordinates.Contains(gridCell.Coordinate)
                    || gridCell.MarkType == ChapterGridCellMarkType.Event)
                {
                    continue;
                }

                gridCells.RemoveAt(index);
                removedCount++;
            }

            return removedCount;
        }

        public static void NormalizeExclusiveTerrainMarks(List<ChapterGridCellData> gridCells)
        {
            if (gridCells == null || gridCells.Count <= 1)
            {
                return;
            }

            HashSet<ChapterGridCoordinate> terrainCoordinates = new HashSet<ChapterGridCoordinate>();
            for (int index = gridCells.Count - 1; index >= 0; index--)
            {
                ChapterGridCellData gridCell = gridCells[index];
                if (gridCell == null || !IsExclusiveTerrainMarkType(gridCell.MarkType))
                {
                    continue;
                }

                if (terrainCoordinates.Add(gridCell.Coordinate))
                {
                    continue;
                }

                gridCells.RemoveAt(index);
            }
        }

        public static void UpsertMarkType(List<ChapterGridCellData> gridCells, ChapterGridCoordinate coordinate, ChapterGridCellMarkType markType)
        {
            if (gridCells == null)
            {
                return;
            }

            if (IsExclusiveTerrainMarkType(markType))
            {
                RemoveExclusiveTerrainMarksAtCoordinate(gridCells, coordinate, markType);
            }

            int existingIndex = FindIndex(gridCells, coordinate, markType);
            if (existingIndex >= 0)
            {
                return;
            }

            gridCells.Add(new ChapterGridCellData
            {
                Coordinate = coordinate,
                MarkType = markType,
            });
        }

        public static bool TryGetEventData(List<ChapterGridCellData> gridCells, ChapterGridCoordinate coordinate, out ChapterGridEventData eventData)
        {
            eventData = null;
            if (gridCells == null)
            {
                return false;
            }

            int existingIndex = FindIndex(gridCells, coordinate, ChapterGridCellMarkType.Event);
            if (existingIndex < 0)
            {
                return false;
            }

            eventData = CloneEventData(gridCells[existingIndex]?.EventData);
            return eventData != null;
        }

        public static bool HasEventAtCoordinate(List<ChapterGridCellData> gridCells, ChapterGridCoordinate coordinate)
        {
            return FindIndex(gridCells, coordinate, ChapterGridCellMarkType.Event) >= 0;
        }

        public static void UpsertEventData(List<ChapterGridCellData> gridCells, ChapterGridCoordinate coordinate, ChapterGridEventData eventData)
        {
            if (gridCells == null || eventData == null)
            {
                return;
            }

            int existingIndex = FindIndex(gridCells, coordinate, ChapterGridCellMarkType.Event);
            if (existingIndex >= 0)
            {
                gridCells[existingIndex].EventData = CloneEventData(eventData);
                return;
            }

            gridCells.Add(new ChapterGridCellData
            {
                Coordinate = coordinate,
                MarkType = ChapterGridCellMarkType.Event,
                EventData = CloneEventData(eventData),
            });
        }

        public static bool RemoveEventData(List<ChapterGridCellData> gridCells, ChapterGridCoordinate coordinate)
        {
            if (gridCells == null)
            {
                return false;
            }

            int existingIndex = FindIndex(gridCells, coordinate, ChapterGridCellMarkType.Event);
            if (existingIndex < 0)
            {
                return false;
            }

            gridCells.RemoveAt(existingIndex);
            return true;
        }

        public static int ClearSelectedMarks(List<ChapterGridCellData> gridCells, IEnumerable<ChapterGridCoordinate> coordinates)
        {
            if (gridCells == null || coordinates == null)
            {
                return 0;
            }

            HashSet<ChapterGridCoordinate> coordinateSet = new HashSet<ChapterGridCoordinate>(coordinates);
            if (coordinateSet.Count <= 0)
            {
                return 0;
            }

            int removedCount = 0;
            for (int index = gridCells.Count - 1; index >= 0; index--)
            {
                ChapterGridCellData gridCell = gridCells[index];
                if (gridCell == null
                    || gridCell.MarkType != ChapterGridCellMarkType.Selected
                    || !coordinateSet.Contains(gridCell.Coordinate))
                {
                    continue;
                }

                gridCells.RemoveAt(index);
                removedCount++;
            }

            return removedCount;
        }

        public static List<ChapterGridCellData> FromLegacyKeys(List<string> gridCellKeys)
        {
            List<ChapterGridCellData> result = new List<ChapterGridCellData>();
            if (gridCellKeys == null)
            {
                return result;
            }

            for (int index = 0; index < gridCellKeys.Count; index++)
            {
                if (!TryParseLegacyKey(gridCellKeys[index], out ChapterGridCoordinate coordinate))
                {
                    continue;
                }

                result.Add(new ChapterGridCellData
                {
                    Coordinate = coordinate,
                    MarkType = ChapterGridCellMarkType.Selected,
                });
            }

            return result;
        }

        public static bool TryParseLegacyKey(string gridCellKey, out ChapterGridCoordinate coordinate)
        {
            coordinate = ChapterGridCoordinate.Zero;
            if (string.IsNullOrWhiteSpace(gridCellKey))
            {
                return false;
            }

            string[] parts = gridCellKey.Split(':');
            if (parts.Length != 2)
            {
                return false;
            }

            if (!int.TryParse(parts[0], out int cellX) || !int.TryParse(parts[1], out int cellY))
            {
                return false;
            }

            coordinate = new ChapterGridCoordinate(cellX, cellY);
            return true;
        }

        public static ChapterGridEventData CloneEventData(ChapterGridEventData source)
        {
            source = ChapterEventDataStructureUtility.NormalizeRuntimeEventData(source);
            if (source == null)
            {
                return null;
            }

            return ChapterEventDataStructureUtility.NormalizeRuntimeEventData(new ChapterGridEventData
            {
                EventId = source.EventId ?? string.Empty,
                IsEnabled = source.IsEnabled,
                IsOneShot = source.IsOneShot,
                Trigger = ChapterEventDataStructureUtility.CloneRuntimeTrigger(source.Trigger),
                Effect = ChapterEventDataStructureUtility.CloneRuntimeEffect(source.Effect),
                EventTitle = source.EventTitle ?? string.Empty,
                TriggerDescription = source.TriggerDescription ?? string.Empty,
                DmNote = source.DmNote ?? string.Empty,
            });
        }

        private static int FindIndex(List<ChapterGridCellData> gridCells, ChapterGridCoordinate coordinate, ChapterGridCellMarkType markType)
        {
            for (int index = 0; index < gridCells.Count; index++)
            {
                ChapterGridCellData gridCell = gridCells[index];
                if (gridCell != null && gridCell.MarkType == markType && gridCell.Coordinate.Equals(coordinate))
                {
                    return index;
                }
            }

            return -1;
        }

        private static void RemoveExclusiveTerrainMarksAtCoordinate(List<ChapterGridCellData> gridCells, ChapterGridCoordinate coordinate, ChapterGridCellMarkType preservedMarkType)
        {
            for (int index = gridCells.Count - 1; index >= 0; index--)
            {
                ChapterGridCellData gridCell = gridCells[index];
                if (gridCell == null
                    || !gridCell.Coordinate.Equals(coordinate)
                    || !IsExclusiveTerrainMarkType(gridCell.MarkType)
                    || gridCell.MarkType == preservedMarkType)
                {
                    continue;
                }

                gridCells.RemoveAt(index);
            }
        }

        private static bool IsExclusiveTerrainMarkType(ChapterGridCellMarkType markType)
        {
            return markType == ChapterGridCellMarkType.DifficultTerrain
                || markType == ChapterGridCellMarkType.ImpassableTerrain;
        }
    }

    internal static class ChapterEventCollectionUtility
    {
        public static List<ChapterGridEventData> CloneEvents(List<ChapterGridEventData> source)
        {
            List<ChapterGridEventData> result = new List<ChapterGridEventData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                ChapterGridEventData eventData = ChapterGridCellCollectionUtility.CloneEventData(source[index]);
                if (eventData != null)
                {
                    result.Add(eventData);
                }
            }

            return result;
        }

        public static List<ChapterEventBindingData> CloneBindings(List<ChapterEventBindingData> source)
        {
            List<ChapterEventBindingData> result = new List<ChapterEventBindingData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                ChapterEventBindingData binding = source[index];
                if (binding == null)
                {
                    continue;
                }

                result.Add(new ChapterEventBindingData
                {
                    BindingId = binding.BindingId ?? string.Empty,
                    EventId = binding.EventId ?? string.Empty,
                    GridCoordinates = CloneCoordinates(binding.GridCoordinates),
                });
            }

            return result;
        }

        public static void UpsertEventDefinition(List<ChapterGridEventData> events, ChapterGridEventData eventData)
        {
            if (events == null || eventData == null)
            {
                return;
            }

            ChapterGridEventData normalizedEvent = ChapterGridCellCollectionUtility.CloneEventData(eventData);
            normalizedEvent.EventId = EnsureEventId(normalizedEvent.EventId);

            int existingIndex = FindEventIndex(events, normalizedEvent.EventId);
            if (existingIndex >= 0)
            {
                events[existingIndex] = normalizedEvent;
                return;
            }

            events.Add(normalizedEvent);
        }

        public static int AssignEventToCoordinates(List<ChapterGridEventData> events, List<ChapterEventBindingData> bindings, IEnumerable<ChapterGridCoordinate> coordinates, ChapterGridEventData eventData)
        {
            if (events == null || bindings == null || eventData == null)
            {
                return 0;
            }

            List<ChapterGridCoordinate> uniqueCoordinates = BuildUniqueCoordinateList(coordinates);
            if (uniqueCoordinates.Count <= 0)
            {
                return 0;
            }

            ChapterGridEventData normalizedEvent = ChapterGridCellCollectionUtility.CloneEventData(eventData);
            normalizedEvent.EventId = EnsureEventId(normalizedEvent.EventId);
            UpsertEventDefinition(events, normalizedEvent);

            RemoveCoordinatesFromBindings(bindings, uniqueCoordinates);
            bindings.Add(new ChapterEventBindingData
            {
                BindingId = CreateBindingId(),
                EventId = normalizedEvent.EventId,
                GridCoordinates = uniqueCoordinates,
            });

            RemoveOrphanEvents(events, bindings);
            return uniqueCoordinates.Count;
        }

        public static int RemoveEventsAtCoordinates(List<ChapterGridEventData> events, List<ChapterEventBindingData> bindings, IEnumerable<ChapterGridCoordinate> coordinates)
        {
            if (bindings == null)
            {
                return 0;
            }

            List<ChapterGridCoordinate> uniqueCoordinates = BuildUniqueCoordinateList(coordinates);
            if (uniqueCoordinates.Count <= 0)
            {
                return 0;
            }

            int removedCount = RemoveCoordinatesFromBindings(bindings, uniqueCoordinates);
            RemoveOrphanEvents(events, bindings);
            return removedCount;
        }

        public static bool TryGetEventData(List<ChapterGridEventData> events, List<ChapterEventBindingData> bindings, ChapterGridCoordinate coordinate, out ChapterGridEventData eventData)
        {
            eventData = null;
            if (!TryGetBinding(bindings, coordinate, out ChapterEventBindingData binding))
            {
                return false;
            }

            int eventIndex = FindEventIndex(events, binding.EventId);
            if (eventIndex < 0)
            {
                return false;
            }

            eventData = ChapterGridCellCollectionUtility.CloneEventData(events[eventIndex]);
            return eventData != null;
        }

        public static bool HasEventAtCoordinate(List<ChapterEventBindingData> bindings, ChapterGridCoordinate coordinate)
        {
            return TryGetBinding(bindings, coordinate, out _);
        }

        public static List<ChapterGridCoordinate> CollectBoundCoordinates(List<ChapterEventBindingData> bindings)
        {
            List<ChapterGridCoordinate> result = new List<ChapterGridCoordinate>();
            if (bindings == null)
            {
                return result;
            }

            HashSet<ChapterGridCoordinate> coordinateSet = new HashSet<ChapterGridCoordinate>();
            for (int bindingIndex = 0; bindingIndex < bindings.Count; bindingIndex++)
            {
                ChapterEventBindingData binding = bindings[bindingIndex];
                if (binding?.GridCoordinates == null)
                {
                    continue;
                }

                for (int coordinateIndex = 0; coordinateIndex < binding.GridCoordinates.Count; coordinateIndex++)
                {
                    ChapterGridCoordinate coordinate = binding.GridCoordinates[coordinateIndex];
                    if (coordinateSet.Add(coordinate))
                    {
                        result.Add(coordinate);
                    }
                }
            }

            return result;
        }

        private static bool TryGetBinding(List<ChapterEventBindingData> bindings, ChapterGridCoordinate coordinate, out ChapterEventBindingData binding)
        {
            binding = null;
            if (bindings == null)
            {
                return false;
            }

            for (int bindingIndex = 0; bindingIndex < bindings.Count; bindingIndex++)
            {
                ChapterEventBindingData candidate = bindings[bindingIndex];
                if (candidate?.GridCoordinates == null)
                {
                    continue;
                }

                for (int coordinateIndex = 0; coordinateIndex < candidate.GridCoordinates.Count; coordinateIndex++)
                {
                    if (!candidate.GridCoordinates[coordinateIndex].Equals(coordinate))
                    {
                        continue;
                    }

                    binding = candidate;
                    return true;
                }
            }

            return false;
        }

        private static int FindEventIndex(List<ChapterGridEventData> events, string eventId)
        {
            if (events == null || string.IsNullOrWhiteSpace(eventId))
            {
                return -1;
            }

            for (int index = 0; index < events.Count; index++)
            {
                if (events[index] != null && string.Equals(events[index].EventId, eventId, StringComparison.Ordinal))
                {
                    return index;
                }
            }

            return -1;
        }

        private static int RemoveCoordinatesFromBindings(List<ChapterEventBindingData> bindings, List<ChapterGridCoordinate> coordinates)
        {
            if (bindings == null || coordinates == null || coordinates.Count <= 0)
            {
                return 0;
            }

            HashSet<ChapterGridCoordinate> coordinateSet = new HashSet<ChapterGridCoordinate>(coordinates);
            int removedCount = 0;
            for (int bindingIndex = bindings.Count - 1; bindingIndex >= 0; bindingIndex--)
            {
                ChapterEventBindingData binding = bindings[bindingIndex];
                if (binding?.GridCoordinates == null)
                {
                    bindings.RemoveAt(bindingIndex);
                    continue;
                }

                for (int coordinateIndex = binding.GridCoordinates.Count - 1; coordinateIndex >= 0; coordinateIndex--)
                {
                    if (!coordinateSet.Contains(binding.GridCoordinates[coordinateIndex]))
                    {
                        continue;
                    }

                    binding.GridCoordinates.RemoveAt(coordinateIndex);
                    removedCount++;
                }

                if (binding.GridCoordinates.Count <= 0)
                {
                    bindings.RemoveAt(bindingIndex);
                }
            }

            return removedCount;
        }

        private static void RemoveOrphanEvents(List<ChapterGridEventData> events, List<ChapterEventBindingData> bindings)
        {
            if (events == null)
            {
                return;
            }

            HashSet<string> referencedEventIds = new HashSet<string>(StringComparer.Ordinal);
            if (bindings != null)
            {
                for (int index = 0; index < bindings.Count; index++)
                {
                    string eventId = bindings[index]?.EventId;
                    if (!string.IsNullOrWhiteSpace(eventId))
                    {
                        referencedEventIds.Add(eventId);
                    }
                }
            }

            for (int index = events.Count - 1; index >= 0; index--)
            {
                ChapterGridEventData eventData = events[index];
                if (eventData == null || !referencedEventIds.Contains(eventData.EventId))
                {
                    events.RemoveAt(index);
                }
            }
        }

        private static string EnsureEventId(string eventId)
        {
            return string.IsNullOrWhiteSpace(eventId) ? $"evt_{Guid.NewGuid():N}" : eventId;
        }

        private static string CreateBindingId()
        {
            return $"bind_{Guid.NewGuid():N}";
        }

        private static List<ChapterGridCoordinate> BuildUniqueCoordinateList(IEnumerable<ChapterGridCoordinate> coordinates)
        {
            List<ChapterGridCoordinate> result = new List<ChapterGridCoordinate>();
            if (coordinates == null)
            {
                return result;
            }

            HashSet<ChapterGridCoordinate> coordinateSet = new HashSet<ChapterGridCoordinate>();
            foreach (ChapterGridCoordinate coordinate in coordinates)
            {
                if (coordinateSet.Add(coordinate))
                {
                    result.Add(coordinate);
                }
            }

            return result;
        }

        private static List<ChapterGridCoordinate> CloneCoordinates(List<ChapterGridCoordinate> source)
        {
            List<ChapterGridCoordinate> result = new List<ChapterGridCoordinate>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                result.Add(source[index]);
            }

            return result;
        }
    }
}
