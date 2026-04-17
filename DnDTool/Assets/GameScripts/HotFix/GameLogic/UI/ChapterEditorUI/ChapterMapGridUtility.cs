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
            if (source == null)
            {
                return null;
            }

            List<ChapterSkillCheckThresholdData> skillCheckEntries = new List<ChapterSkillCheckThresholdData>();
            if (source.SkillCheckEntries != null)
            {
                for (int index = 0; index < source.SkillCheckEntries.Count; index++)
                {
                    ChapterSkillCheckThresholdData entry = source.SkillCheckEntries[index];
                    if (entry == null)
                    {
                        continue;
                    }

                    skillCheckEntries.Add(new ChapterSkillCheckThresholdData
                    {
                        SkillName = entry.SkillName ?? string.Empty,
                        Threshold = entry.Threshold ?? string.Empty,
                    });
                }
            }
            else if (!string.IsNullOrWhiteSpace(source.SkillCheckName)
                || !string.IsNullOrWhiteSpace(source.SkillCheckThreshold))
            {
                skillCheckEntries.Add(new ChapterSkillCheckThresholdData
                {
                    SkillName = source.SkillCheckName ?? string.Empty,
                    Threshold = source.SkillCheckThreshold ?? string.Empty,
                });
            }

            return new ChapterGridEventData
            {
                EventType = source.EventType,
                TriggerMode = source.TriggerMode,
                CheckTargetMode = source.CheckTargetMode,
                CheckResolutionMode = source.CheckResolutionMode,
                EventTitle = source.EventTitle ?? string.Empty,
                TriggerDescription = source.TriggerDescription ?? string.Empty,
                SuccessResult = source.SuccessResult ?? string.Empty,
                FailureResult = source.FailureResult ?? string.Empty,
                DmNote = source.DmNote ?? string.Empty,
                SkillCheckEntries = skillCheckEntries,
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
}