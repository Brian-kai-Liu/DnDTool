using System;
using System.Collections.Generic;
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
    }

    internal sealed class ChapterGridCellData
    {
        public ChapterGridCoordinate Coordinate { get; set; } = ChapterGridCoordinate.Zero;

        public ChapterGridCellMarkType MarkType { get; set; } = ChapterGridCellMarkType.Selected;
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
    }

    [Serializable]
    internal sealed class ChapterEditorSaveData
    {
        public int SelectedChapterId = -1;
        public int NextChapterId = 1;
        public List<ChapterItemSaveData> Chapters = new List<ChapterItemSaveData>();
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
        public List<string> SelectedGridCellKeys = new List<string>();
    }

    [Serializable]
    internal sealed class ChapterGridCellSaveData
    {
        public int CellX;
        public int CellY;
        public int MarkType = (int) ChapterGridCellMarkType.DifficultTerrain;
    }
}