using System;
using System.IO;
using UnityEngine;

namespace GameLogic
{
    internal sealed class CharacterPortraitApplicationService
    {
        private const string SaveDirectoryName = "CharacterPortraits";

        private static readonly Lazy<CharacterPortraitApplicationService> s_instance =
            new Lazy<CharacterPortraitApplicationService>(() => new CharacterPortraitApplicationService());

        private CharacterPortraitApplicationService()
        {
        }

        public static CharacterPortraitApplicationService Instance => s_instance.Value;

        public string GetPortraitDirectoryPath()
        {
            return Path.Combine(Application.persistentDataPath, SaveDirectoryName);
        }

        public string StorePortraitImage(string sourceFilePath, string targetDirectoryPath)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath) || !File.Exists(sourceFilePath))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(targetDirectoryPath))
            {
                return string.Empty;
            }

            Directory.CreateDirectory(targetDirectoryPath);
            string extension = Path.GetExtension(sourceFilePath);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".png";
            }

            string targetFileName = $"character_portrait_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            string targetFilePath = Path.Combine(targetDirectoryPath, targetFileName);
            File.Copy(sourceFilePath, targetFilePath, true);
            return targetFilePath;
        }

        public byte[] ReadImageBytes(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                return Array.Empty<byte>();
            }

            return File.ReadAllBytes(imagePath);
        }
    }
}
