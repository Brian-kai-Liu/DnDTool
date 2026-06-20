using System.Collections.Generic;

namespace GameLogic
{
    internal sealed class ModuleAuthoringViewState
    {
        public ModuleBasicInfoViewState BasicInfo { get; set; } = new ModuleBasicInfoViewState();

        public int SelectedChapterId { get; set; } = -1;

        public List<ModuleChapterListItemViewState> ChapterItems { get; set; } = new List<ModuleChapterListItemViewState>();
    }

    internal sealed class ModuleBasicInfoViewState
    {
        public string ModuleName { get; set; } = string.Empty;

        public string ModuleIntroduction { get; set; } = string.Empty;

        public string RuleVersion { get; set; } = string.Empty;

        public string ExtensionPackageSummary { get; set; } = string.Empty;

        public string RecommendedLevel { get; set; } = string.Empty;

        public string RecommendedPlayers { get; set; } = string.Empty;

        public string EstimatedDuration { get; set; } = string.Empty;

        public string PreviewImagePath { get; set; } = string.Empty;

        public bool HasPreviewImage { get; set; }

        public List<ModuleAdventureHookViewState> AdventureHooks { get; set; } = new List<ModuleAdventureHookViewState>();
    }

    internal sealed class ModuleAdventureHookViewState
    {
        public string HookId { get; set; } = string.Empty;

        public string Target { get; set; } = string.Empty;

        public string HookContent { get; set; } = string.Empty;

        public bool IsCompleted { get; set; }
    }

    internal sealed class ModuleChapterListItemViewState
    {
        public int ChapterId { get; set; }

        public int ChapterIndex { get; set; }

        public string ChapterName { get; set; } = string.Empty;

        public bool IsSelected { get; set; }

        public bool HasMap { get; set; }

        public int EventCount { get; set; }

        public int CreatureCount { get; set; }
    }

    internal sealed class ModuleChapterEditorViewState
    {
        public int ChapterId { get; set; }

        public string ChapterName { get; set; } = string.Empty;

        public string Goal { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string DmNote { get; set; } = string.Empty;

        public string TerrainTag { get; set; } = string.Empty;

        public string TerrainSubTag { get; set; } = string.Empty;

        public string AddMapHint { get; set; } = string.Empty;

        public string CreatureInfo { get; set; } = string.Empty;

        public bool HasMap { get; set; }

        public string MapImagePath { get; set; } = string.Empty;

        public bool CanSave { get; set; }

        public bool CanPreview { get; set; }
    }
}
