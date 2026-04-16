using System.Collections.Generic;
using System.IO;
using TEngine;

namespace GameLogic
{
    internal static class ChapterEditorPersistenceService
    {
        public static string GetSaveFilePath(string fileName)
        {
            return Path.Combine(UnityEngine.Application.persistentDataPath, "ChapterEditor", fileName);
        }

        public static ChapterEditorSaveData Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            return Utility.Json.ToObject<ChapterEditorSaveData>(File.ReadAllText(filePath));
        }

        public static void Save(string filePath, ChapterEditorSaveData saveData)
        {
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            File.WriteAllText(filePath, Utility.Json.ToJson(saveData));
        }

        public static ChapterEditorSaveData BuildSaveData(List<ChapterListItemData> chapters, int selectedChapterId, int nextChapterId)
        {
            ChapterEditorSaveData saveData = new ChapterEditorSaveData
            {
                SelectedChapterId = selectedChapterId,
                NextChapterId = nextChapterId,
            };

            if (chapters == null)
            {
                return saveData;
            }

            for (int index = 0; index < chapters.Count; index++)
            {
                saveData.Chapters.Add(ToSaveData(chapters[index]));
            }

            return saveData;
        }

        public static List<ChapterListItemData> BuildRuntimeChapters(List<ChapterItemSaveData> chapters)
        {
            List<ChapterListItemData> result = new List<ChapterListItemData>();
            if (chapters == null)
            {
                return result;
            }

            for (int index = 0; index < chapters.Count; index++)
            {
                ChapterItemSaveData chapter = chapters[index];
                if (chapter == null)
                {
                    continue;
                }

                result.Add(ToRuntimeData(chapter));
            }

            return result;
        }

        private static ChapterItemSaveData ToSaveData(ChapterListItemData chapter)
        {
            ChapterMapGridStateData mapGridState = chapter?.MapGridState ?? new ChapterMapGridStateData();
            ChapterGridCellCollectionUtility.NormalizeExclusiveTerrainMarks(chapter?.GridCells);
            ChapterItemSaveData saveData = new ChapterItemSaveData
            {
                Id = chapter?.Id ?? 0,
                Name = chapter?.Name ?? string.Empty,
                Goal = chapter?.Goal ?? string.Empty,
                Content = chapter?.Content ?? string.Empty,
                DmNote = chapter?.DmNote ?? string.Empty,
                TerrainTag = chapter?.TerrainTag ?? string.Empty,
                TerrainSubTag = chapter?.TerrainSubTag ?? string.Empty,
                AddMapHint = chapter?.AddMapHint ?? string.Empty,
                CreatureInfo = chapter?.CreatureInfo ?? string.Empty,
                MapImagePath = chapter?.MapImagePath ?? string.Empty,
                IsMapZoomEnabled = mapGridState.IsMapZoomEnabled,
                IsGridZoomEnabled = mapGridState.IsGridZoomEnabled,
                MapZoomScale = mapGridState.MapZoomScale,
                MapPanOffset = mapGridState.MapPanOffset,
                GridZoomScale = mapGridState.GridZoomScale,
                GridPanOffset = mapGridState.GridPanOffset,
                IsMapGridLocked = mapGridState.IsLocked,
                LockedMapZoomReference = mapGridState.LockedMapZoomReference,
                LockedGridToMapZoomRatio = mapGridState.LockedGridToMapZoomRatio,
                LockedGridToMapPanDelta = mapGridState.LockedGridToMapPanDelta,
            };

            List<ChapterGridCellData> gridCells = chapter?.GridCells;
            if (gridCells != null)
            {
                for (int index = 0; index < gridCells.Count; index++)
                {
                    ChapterGridCellData gridCell = gridCells[index];
                    if (gridCell == null || gridCell.MarkType == ChapterGridCellMarkType.Selected)
                    {
                        continue;
                    }

                    saveData.GridCells.Add(new ChapterGridCellSaveData
                    {
                        CellX = gridCell.Coordinate.CellX,
                        CellY = gridCell.Coordinate.CellY,
                        MarkType = (int) gridCell.MarkType,
                    });
                }
            }

            return saveData;
        }

        private static ChapterListItemData ToRuntimeData(ChapterItemSaveData chapter)
        {
            List<ChapterGridCellData> gridCells = new List<ChapterGridCellData>();
            if (chapter.GridCells != null && chapter.GridCells.Count > 0)
            {
                for (int index = 0; index < chapter.GridCells.Count; index++)
                {
                    ChapterGridCellSaveData gridCell = chapter.GridCells[index];
                    if (gridCell == null)
                    {
                        continue;
                    }

                    gridCells.Add(new ChapterGridCellData
                    {
                        Coordinate = new ChapterGridCoordinate(gridCell.CellX, gridCell.CellY),
                        MarkType = System.Enum.IsDefined(typeof(ChapterGridCellMarkType), gridCell.MarkType)
                            ? (ChapterGridCellMarkType) gridCell.MarkType
                            : ChapterGridCellMarkType.Selected,
                    });
                }

                ChapterGridCellCollectionUtility.NormalizeExclusiveTerrainMarks(gridCells);
            }

            return new ChapterListItemData
            {
                Id = chapter.Id,
                Name = chapter.Name ?? string.Empty,
                Goal = chapter.Goal ?? string.Empty,
                Content = chapter.Content ?? string.Empty,
                DmNote = chapter.DmNote ?? string.Empty,
                TerrainTag = chapter.TerrainTag ?? string.Empty,
                TerrainSubTag = chapter.TerrainSubTag ?? string.Empty,
                AddMapHint = chapter.AddMapHint ?? string.Empty,
                CreatureInfo = chapter.CreatureInfo ?? string.Empty,
                MapImagePath = chapter.MapImagePath ?? string.Empty,
                MapGridState = new ChapterMapGridStateData
                {
                    IsMapZoomEnabled = chapter.IsMapZoomEnabled,
                    IsGridZoomEnabled = chapter.IsGridZoomEnabled,
                    MapZoomScale = chapter.MapZoomScale,
                    MapPanOffset = chapter.MapPanOffset,
                    GridZoomScale = chapter.GridZoomScale,
                    GridPanOffset = chapter.GridPanOffset,
                    IsLocked = chapter.IsMapGridLocked,
                    LockedMapZoomReference = chapter.LockedMapZoomReference,
                    LockedGridToMapZoomRatio = chapter.LockedGridToMapZoomRatio,
                    LockedGridToMapPanDelta = chapter.LockedGridToMapPanDelta,
                },
                GridCells = gridCells,
            };
        }
    }
}