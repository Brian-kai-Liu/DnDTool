using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    internal sealed class ModulePreviewSessionData
    {
        public string SessionId { get; set; } = string.Empty;

        public int CurrentChapterIndex { get; set; }

        public int EditorSelectedChapterId { get; set; } = -1;

        public int EditorSelectedChapterIndex { get; set; } = -1;

        public List<ChapterPreviewRuntimeData> Chapters { get; set; } = new List<ChapterPreviewRuntimeData>();

        public ChapterPreviewRuntimeData CurrentChapter
        {
            get
            {
                if (Chapters == null || Chapters.Count <= 0)
                {
                    return null;
                }

                int index = Mathf.Clamp(CurrentChapterIndex, 0, Chapters.Count - 1);
                return Chapters[index];
            }
        }
    }

    internal sealed class ChapterPreviewRuntimeData
    {
        public int ChapterId { get; set; }

        public int ChapterIndex { get; set; }

        public string ChapterName { get; set; } = string.Empty;

        public string Goal { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string DmNote { get; set; } = string.Empty;

        public string TerrainTag { get; set; } = string.Empty;

        public string TerrainSubTag { get; set; } = string.Empty;

        public string AddMapHint { get; set; } = string.Empty;

        public string CreatureInfo { get; set; } = string.Empty;

        public string MapImagePath { get; set; } = string.Empty;

        public ChapterMapGridStateData MapGridState { get; set; } = new ChapterMapGridStateData();

        public Vector2 EditorMapOverlaySize { get; set; } = Vector2.zero;

        public List<ChapterGridCellData> GridCells { get; set; } = new List<ChapterGridCellData>();

        public List<ChapterGridEventData> Events { get; set; } = new List<ChapterGridEventData>();

        public List<ChapterEventBindingData> EventBindings { get; set; } = new List<ChapterEventBindingData>();

        public List<ChapterGridCoordinate> PlayerInitialPositionCoordinates { get; set; } = new List<ChapterGridCoordinate>();

        public List<ChapterCreatureData> CreatureTemplates { get; set; } = new List<ChapterCreatureData>();

        public List<ChapterCreatureInstanceData> CreatureInstances { get; set; } = new List<ChapterCreatureInstanceData>();
    }

    internal static class ModulePreviewSessionBuilder
    {
        public static ModulePreviewSessionData Build(
            List<ChapterListItemData> sourceChapters,
            int editorSelectedChapterId,
            int editorSelectedChapterIndex,
            Vector2 editorMapOverlaySize = default)
        {
            ModulePreviewSessionData session = new ModulePreviewSessionData
            {
                SessionId = $"preview_{Guid.NewGuid():N}",
                CurrentChapterIndex = 0,
                EditorSelectedChapterId = editorSelectedChapterId,
                EditorSelectedChapterIndex = editorSelectedChapterIndex,
            };

            if (sourceChapters == null)
            {
                return session;
            }

            for (int index = 0; index < sourceChapters.Count; index++)
            {
                ChapterListItemData source = sourceChapters[index];
                if (source == null)
                {
                    continue;
                }

                session.Chapters.Add(BuildChapterSnapshot(source, index, editorMapOverlaySize));
            }

            session.CurrentChapterIndex = session.Chapters.Count > 0 ? 0 : -1;
            return session;
        }

        private static ChapterPreviewRuntimeData BuildChapterSnapshot(ChapterListItemData source, int chapterIndex, Vector2 editorMapOverlaySize)
        {
            return new ChapterPreviewRuntimeData
            {
                ChapterId = source.Id,
                ChapterIndex = chapterIndex,
                ChapterName = source.Name ?? string.Empty,
                Goal = source.Goal ?? string.Empty,
                Content = source.Content ?? string.Empty,
                DmNote = source.DmNote ?? string.Empty,
                TerrainTag = source.TerrainTag ?? string.Empty,
                TerrainSubTag = source.TerrainSubTag ?? string.Empty,
                AddMapHint = source.AddMapHint ?? string.Empty,
                CreatureInfo = source.CreatureInfo ?? string.Empty,
                MapImagePath = source.MapImagePath ?? string.Empty,
                MapGridState = CloneMapGridState(source.MapGridState),
                EditorMapOverlaySize = editorMapOverlaySize,
                GridCells = ChapterGridCellCollectionUtility.Clone(source.GridCells),
                Events = ChapterEventCollectionUtility.CloneEvents(source.Events),
                EventBindings = ChapterEventCollectionUtility.CloneBindings(source.EventBindings),
                PlayerInitialPositionCoordinates = ChapterEventCollectionUtility.CollectPlayerInitialPositionCoordinates(source.Events, source.EventBindings),
                CreatureTemplates = ChapterCreatureDataStructureUtility.CloneCreatureDataList(source.Creatures),
                CreatureInstances = ChapterCreatureDataStructureUtility.CloneCreatureInstanceDataList(source.CreatureInstances),
            };
        }

        private static ChapterMapGridStateData CloneMapGridState(ChapterMapGridStateData source)
        {
            source ??= new ChapterMapGridStateData();
            return new ChapterMapGridStateData
            {
                IsMapZoomEnabled = source.IsMapZoomEnabled,
                IsGridZoomEnabled = source.IsGridZoomEnabled,
                MapZoomScale = source.MapZoomScale,
                MapPanOffset = source.MapPanOffset,
                GridZoomScale = source.GridZoomScale,
                GridPanOffset = source.GridPanOffset,
                IsLocked = source.IsLocked,
                LockedMapZoomReference = source.LockedMapZoomReference,
                LockedGridToMapZoomRatio = source.LockedGridToMapZoomRatio,
                LockedGridToMapPanDelta = source.LockedGridToMapPanDelta,
            };
        }
    }
}
