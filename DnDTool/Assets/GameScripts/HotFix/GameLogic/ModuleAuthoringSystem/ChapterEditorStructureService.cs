using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    internal static class ChapterEditorStructureService
    {
        public static ChapterListItemData CreateChapter(int chapterId)
        {
            return new ChapterListItemData
            {
                Id = chapterId,
                Name = string.Empty,
                Goal = string.Empty,
                Content = string.Empty,
                DmNote = string.Empty,
                TerrainTag = string.Empty,
                TerrainSubTag = string.Empty,
                AddMapHint = string.Empty,
                CreatureInfo = string.Empty,
                MapGridState = new ChapterMapGridStateData(),
                GridCells = new List<ChapterGridCellData>(),
                Events = new List<ChapterGridEventData>(),
                EventBindings = new List<ChapterEventBindingData>(),
                Creatures = new List<ChapterCreatureData>(),
                CreatureInstances = new List<ChapterCreatureInstanceData>(),
            };
        }

        public static int FindChapterIndexById(List<ChapterListItemData> chapters, int chapterId)
        {
            if (chapters == null || chapterId < 0)
            {
                return -1;
            }

            for (int index = 0; index < chapters.Count; index++)
            {
                if (chapters[index]?.Id == chapterId)
                {
                    return index;
                }
            }

            return -1;
        }

        public static int ResolveSelectedChapterId(List<ChapterListItemData> chapters, int selectedChapterId)
        {
            if (chapters == null || chapters.Count <= 0)
            {
                return -1;
            }

            return FindChapterIndexById(chapters, selectedChapterId) >= 0
                ? selectedChapterId
                : chapters[0].Id;
        }

        public static int ResolveNextChapterId(List<ChapterListItemData> chapters, int requestedNextChapterId)
        {
            int maxChapterId = 0;
            if (chapters != null)
            {
                for (int index = 0; index < chapters.Count; index++)
                {
                    maxChapterId = Mathf.Max(maxChapterId, chapters[index]?.Id ?? 0);
                }
            }

            return Mathf.Max(requestedNextChapterId, maxChapterId + 1, 1);
        }

        public static ChapterListItemData AddChapter(List<ChapterListItemData> chapters, ref int nextChapterId)
        {
            if (chapters == null)
            {
                return null;
            }

            ChapterListItemData chapter = CreateChapter(nextChapterId++);
            chapters.Add(chapter);
            return chapter;
        }

        public static bool MoveChapter(List<ChapterListItemData> chapters, int fromIndex, int toIndex)
        {
            if (chapters == null
                || fromIndex < 0
                || fromIndex >= chapters.Count
                || toIndex < 0
                || toIndex >= chapters.Count
                || fromIndex == toIndex)
            {
                return false;
            }

            ChapterListItemData movedChapter = chapters[fromIndex];
            chapters.RemoveAt(fromIndex);
            chapters.Insert(toIndex, movedChapter);
            return true;
        }

        public static bool DeleteChapter(List<ChapterListItemData> chapters, int chapterId, ref int selectedChapterId)
        {
            int chapterIndex = FindChapterIndexById(chapters, chapterId);
            if (chapters == null || chapterIndex < 0 || chapterIndex >= chapters.Count)
            {
                return false;
            }

            bool isDeletedChapterSelected = selectedChapterId == chapterId;
            chapters.RemoveAt(chapterIndex);
            if (chapters.Count <= 0)
            {
                selectedChapterId = -1;
                return true;
            }

            if (isDeletedChapterSelected)
            {
                int nextSelectedIndex = Mathf.Clamp(chapterIndex, 0, chapters.Count - 1);
                selectedChapterId = chapters[nextSelectedIndex].Id;
            }

            return true;
        }
    }
}
