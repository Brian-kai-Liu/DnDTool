using System;
using System.IO;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    internal static class ModuleAuthoringRepository
    {
        private const string ChapterEditorDirectoryName = "ChapterEditor";
        private const string ChapterCreaturePreviewDirectoryName = "ChapterCreaturePreviews";
        private const string ChapterMapDirectoryName = "ChapterMaps";

        public static string GetChapterEditorSaveFilePath(string fileName)
        {
            return Path.Combine(Application.persistentDataPath, ChapterEditorDirectoryName, fileName);
        }

        public static string GetCreaturePreviewDirectoryPath()
        {
            return Path.Combine(Application.persistentDataPath, ChapterCreaturePreviewDirectoryName);
        }

        public static string GetChapterMapDirectoryPath()
        {
            return Path.Combine(Application.persistentDataPath, ChapterMapDirectoryName);
        }

        public static string ResolveCreaturePreviewPath(string previewImageFileName)
        {
            if (string.IsNullOrWhiteSpace(previewImageFileName))
            {
                return string.Empty;
            }

            if (Path.IsPathRooted(previewImageFileName))
            {
                return File.Exists(previewImageFileName) ? previewImageFileName : string.Empty;
            }

            string resolvedPath = Path.Combine(GetCreaturePreviewDirectoryPath(), previewImageFileName);
            return File.Exists(resolvedPath) ? resolvedPath : string.Empty;
        }

        public static string StoreCreaturePreviewImage(string sourceFilePath)
        {
            return StoreManagedImage(sourceFilePath, GetCreaturePreviewDirectoryPath(), "creature");
        }

        public static string StoreChapterMapImage(string sourceFilePath, int chapterId)
        {
            string storedFileName = StoreManagedImage(sourceFilePath, GetChapterMapDirectoryPath(), $"chapter_{chapterId}");
            return string.IsNullOrWhiteSpace(storedFileName)
                ? string.Empty
                : Path.Combine(GetChapterMapDirectoryPath(), storedFileName);
        }

        public static bool FileExists(string filePath)
        {
            return !string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath);
        }

        public static string GetDisplayFileName(string filePath)
        {
            return string.IsNullOrWhiteSpace(filePath) ? string.Empty : Path.GetFileName(filePath);
        }

        public static byte[] ReadFileBytes(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }

        public static bool TryDeleteManagedChapterMapFile(string filePath)
        {
            return TryDeleteManagedFile(filePath, GetChapterMapDirectoryPath());
        }

        public static T LoadJson<T>(string filePath) where T : class
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return null;
            }

            string json = File.ReadAllText(filePath);
            return Utility.Json.ToObject<T>(json);
        }

        public static void SaveJson<T>(string filePath, T saveData) where T : class
        {
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            File.WriteAllText(filePath, Utility.Json.ToJson(saveData));
        }

        private static string StoreManagedImage(string sourceFilePath, string targetDirectoryPath, string fileNamePrefix)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath) || !File.Exists(sourceFilePath))
            {
                return string.Empty;
            }

            string sourceFullPath = Path.GetFullPath(sourceFilePath);
            Directory.CreateDirectory(targetDirectoryPath);

            string targetDirectoryFullPath = Path.GetFullPath(targetDirectoryPath);
            if (sourceFullPath.StartsWith(targetDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFileName(sourceFullPath);
            }

            string extension = Path.GetExtension(sourceFullPath);
            string targetFileName = $"{fileNamePrefix}_{DateTime.Now:yyyyMMddHHmmssfff}{extension}";
            string targetFilePath = Path.Combine(targetDirectoryPath, targetFileName);
            File.Copy(sourceFullPath, targetFilePath, true);
            return targetFileName;
        }

        private static bool TryDeleteManagedFile(string filePath, string storageDirectoryPath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return false;
            }

            string normalizedFilePath = Path.GetFullPath(filePath);
            string normalizedStorageDirectoryPath = Path.GetFullPath(storageDirectoryPath);
            if (!normalizedFilePath.StartsWith(normalizedStorageDirectoryPath, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            File.Delete(normalizedFilePath);
            return true;
        }
    }
}
