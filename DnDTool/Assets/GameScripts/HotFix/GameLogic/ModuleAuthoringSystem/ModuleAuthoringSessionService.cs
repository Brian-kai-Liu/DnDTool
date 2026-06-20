using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    internal sealed class ModuleAuthoringSessionService
    {
        private static readonly Lazy<ModuleAuthoringSessionService> s_instance =
            new Lazy<ModuleAuthoringSessionService>(() => new ModuleAuthoringSessionService());

        private ModuleAuthoringDraftState m_state = new ModuleAuthoringDraftState();

        private ModuleAuthoringSessionService()
        {
        }

        public static ModuleAuthoringSessionService Instance => s_instance.Value;

        public ModuleAuthoringDraftState CurrentState => m_state;

        public void BeginNewDraft(string draftId = "")
        {
            m_state = CreateEmptyDraft(draftId);
        }

        public void LoadDraft(ModuleAuthoringDraftState state)
        {
            m_state = CloneDraftState(state) ?? CreateEmptyDraft(string.Empty);
            NormalizeDraftState(m_state);
            m_state.IsDirty = false;
        }

        public ModuleAuthoringDraftState ExportDraft()
        {
            ModuleAuthoringDraftState snapshot = CloneDraftState(m_state);
            NormalizeDraftState(snapshot);
            return snapshot;
        }

        public void MarkClean()
        {
            m_state.IsDirty = false;
        }

        public void UpdateBasicInfo(ModuleBasicInfoDraftData basicInfo)
        {
            m_state.BasicInfo = CloneBasicInfo(basicInfo);
            m_state.IsDirty = true;
        }

        public ModuleChapterDraftData AddChapter(string chapterName = "")
        {
            NormalizeDraftState(m_state);
            ModuleChapterDraftData chapter = CreateChapter(m_state.NextChapterId++, chapterName);
            m_state.Chapters.Add(chapter);
            m_state.SelectedChapterId = chapter.ChapterId;
            m_state.IsDirty = true;
            return CloneChapter(chapter);
        }

        public ModuleAuthoringOperationResult SelectChapter(int chapterId)
        {
            if (FindChapterIndex(chapterId) < 0)
            {
                return ModuleAuthoringOperationResult.Fail("Chapter does not exist.");
            }

            m_state.SelectedChapterId = chapterId;
            return ModuleAuthoringOperationResult.Ok();
        }

        public ModuleAuthoringOperationResult DeleteChapter(int chapterId)
        {
            int chapterIndex = FindChapterIndex(chapterId);
            if (chapterIndex < 0)
            {
                return ModuleAuthoringOperationResult.Fail("Chapter does not exist.");
            }

            bool deletingSelectedChapter = m_state.SelectedChapterId == chapterId;
            m_state.Chapters.RemoveAt(chapterIndex);
            if (deletingSelectedChapter)
            {
                int nextSelectedIndex = Mathf.Clamp(chapterIndex, 0, m_state.Chapters.Count - 1);
                m_state.SelectedChapterId = m_state.Chapters.Count > 0
                    ? m_state.Chapters[nextSelectedIndex].ChapterId
                    : -1;
            }

            m_state.IsDirty = true;
            return ModuleAuthoringOperationResult.Ok();
        }

        public ModuleAuthoringOperationResult ReorderChapter(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= m_state.Chapters.Count || toIndex < 0 || toIndex >= m_state.Chapters.Count)
            {
                return ModuleAuthoringOperationResult.Fail("Chapter reorder index is out of range.");
            }

            if (fromIndex == toIndex)
            {
                return ModuleAuthoringOperationResult.Ok();
            }

            ModuleChapterDraftData chapter = m_state.Chapters[fromIndex];
            m_state.Chapters.RemoveAt(fromIndex);
            m_state.Chapters.Insert(toIndex, chapter);
            m_state.IsDirty = true;
            return ModuleAuthoringOperationResult.Ok();
        }

        public bool TryGetSelectedChapter(out ModuleChapterDraftData chapter)
        {
            return TryGetChapter(m_state.SelectedChapterId, out chapter);
        }

        public bool TryGetChapter(int chapterId, out ModuleChapterDraftData chapter)
        {
            int chapterIndex = FindChapterIndex(chapterId);
            if (chapterIndex < 0)
            {
                chapter = null;
                return false;
            }

            chapter = CloneChapter(m_state.Chapters[chapterIndex]);
            return true;
        }

        public ModuleAuthoringOperationResult UpdateChapter(ModuleChapterDraftData chapter)
        {
            if (chapter == null)
            {
                return ModuleAuthoringOperationResult.Fail("Chapter data is empty.");
            }

            int chapterIndex = FindChapterIndex(chapter.ChapterId);
            if (chapterIndex < 0)
            {
                return ModuleAuthoringOperationResult.Fail("Chapter does not exist.");
            }

            m_state.Chapters[chapterIndex] = CloneChapter(chapter);
            NormalizeDraftState(m_state);
            m_state.IsDirty = true;
            return ModuleAuthoringOperationResult.Ok();
        }

        public ModuleAuthoringViewState BuildViewState()
        {
            NormalizeDraftState(m_state);
            ModuleAuthoringViewState viewState = new ModuleAuthoringViewState
            {
                BasicInfo = BuildBasicInfoViewState(m_state.BasicInfo),
                SelectedChapterId = m_state.SelectedChapterId,
            };

            for (int index = 0; index < m_state.Chapters.Count; index++)
            {
                ModuleChapterDraftData chapter = m_state.Chapters[index];
                viewState.ChapterItems.Add(new ModuleChapterListItemViewState
                {
                    ChapterId = chapter.ChapterId,
                    ChapterIndex = index + 1,
                    ChapterName = chapter.ChapterName ?? string.Empty,
                    IsSelected = chapter.ChapterId == m_state.SelectedChapterId,
                    HasMap = !string.IsNullOrWhiteSpace(chapter.MapImagePath),
                    EventCount = chapter.Events?.Count ?? 0,
                    CreatureCount = chapter.CreatureInstances?.Count ?? 0,
                });
            }

            return viewState;
        }

        public bool TryBuildSelectedChapterViewState(out ModuleChapterEditorViewState viewState)
        {
            if (!TryGetSelectedChapter(out ModuleChapterDraftData chapter))
            {
                viewState = null;
                return false;
            }

            viewState = new ModuleChapterEditorViewState
            {
                ChapterId = chapter.ChapterId,
                ChapterName = chapter.ChapterName ?? string.Empty,
                Goal = chapter.Goal ?? string.Empty,
                Content = chapter.Content ?? string.Empty,
                DmNote = chapter.DmNote ?? string.Empty,
                TerrainTag = chapter.TerrainTag ?? string.Empty,
                TerrainSubTag = chapter.TerrainSubTag ?? string.Empty,
                AddMapHint = chapter.AddMapHint ?? string.Empty,
                CreatureInfo = chapter.CreatureInfo ?? string.Empty,
                HasMap = !string.IsNullOrWhiteSpace(chapter.MapImagePath),
                MapImagePath = chapter.MapImagePath ?? string.Empty,
                CanSave = true,
                CanPreview = m_state.Chapters.Count > 0,
            };

            return true;
        }

        private int FindChapterIndex(int chapterId)
        {
            for (int index = 0; index < m_state.Chapters.Count; index++)
            {
                if (m_state.Chapters[index].ChapterId == chapterId)
                {
                    return index;
                }
            }

            return -1;
        }

        private static ModuleAuthoringDraftState CreateEmptyDraft(string draftId)
        {
            return new ModuleAuthoringDraftState
            {
                DraftId = string.IsNullOrWhiteSpace(draftId) ? $"mod_{Guid.NewGuid():N}" : draftId,
                BasicInfo = new ModuleBasicInfoDraftData(),
                SelectedChapterId = -1,
                NextChapterId = 1,
                Chapters = new List<ModuleChapterDraftData>(),
                IsDirty = false,
            };
        }

        private static ModuleChapterDraftData CreateChapter(int chapterId, string chapterName)
        {
            return new ModuleChapterDraftData
            {
                ChapterId = chapterId,
                ChapterName = chapterName ?? string.Empty,
                MapGrid = new ModuleChapterMapGridDraftData(),
                GridCells = new List<ModuleChapterGridCellDraftData>(),
                Events = new List<ModuleChapterEventDraftData>(),
                EventBindings = new List<ModuleChapterEventBindingDraftData>(),
                CreatureTemplates = new List<ModuleChapterCreatureDraftData>(),
                CreatureInstances = new List<ModuleChapterCreatureInstanceDraftData>(),
            };
        }

        private static void NormalizeDraftState(ModuleAuthoringDraftState state)
        {
            if (state == null)
            {
                return;
            }

            state.DraftId = state.DraftId ?? string.Empty;
            state.BasicInfo = CloneBasicInfo(state.BasicInfo);
            state.Chapters ??= new List<ModuleChapterDraftData>();

            int maxChapterId = 0;
            for (int index = 0; index < state.Chapters.Count; index++)
            {
                state.Chapters[index] = CloneChapter(state.Chapters[index]);
                maxChapterId = Mathf.Max(maxChapterId, state.Chapters[index].ChapterId);
            }

            state.NextChapterId = Mathf.Max(state.NextChapterId, maxChapterId + 1, 1);
            if (FindChapterIndex(state.Chapters, state.SelectedChapterId) < 0)
            {
                state.SelectedChapterId = state.Chapters.Count > 0 ? state.Chapters[0].ChapterId : -1;
            }
        }

        private static int FindChapterIndex(List<ModuleChapterDraftData> chapters, int chapterId)
        {
            if (chapters == null)
            {
                return -1;
            }

            for (int index = 0; index < chapters.Count; index++)
            {
                if (chapters[index].ChapterId == chapterId)
                {
                    return index;
                }
            }

            return -1;
        }

        private static ModuleBasicInfoViewState BuildBasicInfoViewState(ModuleBasicInfoDraftData basicInfo)
        {
            ModuleBasicInfoViewState viewState = new ModuleBasicInfoViewState
            {
                ModuleName = basicInfo?.ModuleName ?? string.Empty,
                ModuleIntroduction = basicInfo?.ModuleIntroduction ?? string.Empty,
                RuleVersion = basicInfo?.RuleVersion ?? string.Empty,
                ExtensionPackageSummary = basicInfo?.ExtensionPackageSummary ?? string.Empty,
                RecommendedLevel = basicInfo?.RecommendedLevel ?? string.Empty,
                RecommendedPlayers = basicInfo?.RecommendedPlayers ?? string.Empty,
                EstimatedDuration = basicInfo?.EstimatedDuration ?? string.Empty,
                PreviewImagePath = basicInfo?.PreviewImagePath ?? string.Empty,
                HasPreviewImage = !string.IsNullOrWhiteSpace(basicInfo?.PreviewImagePath),
            };

            if (basicInfo?.AdventureHooks == null)
            {
                return viewState;
            }

            for (int index = 0; index < basicInfo.AdventureHooks.Count; index++)
            {
                ModuleAdventureHookDraftData hook = basicInfo.AdventureHooks[index];
                if (hook == null)
                {
                    continue;
                }

                viewState.AdventureHooks.Add(new ModuleAdventureHookViewState
                {
                    HookId = hook.HookId ?? string.Empty,
                    Target = hook.Target ?? string.Empty,
                    HookContent = hook.HookContent ?? string.Empty,
                    IsCompleted = !string.IsNullOrWhiteSpace(hook.Target) && !string.IsNullOrWhiteSpace(hook.HookContent),
                });
            }

            return viewState;
        }

        internal static ModuleAuthoringDraftState CloneDraftState(ModuleAuthoringDraftState source)
        {
            if (source == null)
            {
                return null;
            }

            ModuleAuthoringDraftState result = new ModuleAuthoringDraftState
            {
                DraftId = source.DraftId ?? string.Empty,
                BasicInfo = CloneBasicInfo(source.BasicInfo),
                SelectedChapterId = source.SelectedChapterId,
                NextChapterId = source.NextChapterId,
                IsDirty = source.IsDirty,
            };

            if (source.Chapters != null)
            {
                for (int index = 0; index < source.Chapters.Count; index++)
                {
                    result.Chapters.Add(CloneChapter(source.Chapters[index]));
                }
            }

            return result;
        }

        internal static ModuleBasicInfoDraftData CloneBasicInfo(ModuleBasicInfoDraftData source)
        {
            ModuleBasicInfoDraftData result = new ModuleBasicInfoDraftData
            {
                ModuleName = source?.ModuleName ?? string.Empty,
                ModuleIntroduction = source?.ModuleIntroduction ?? string.Empty,
                RuleVersion = source?.RuleVersion ?? string.Empty,
                ExtensionPackageSummary = source?.ExtensionPackageSummary ?? string.Empty,
                RecommendedLevel = source?.RecommendedLevel ?? string.Empty,
                RecommendedPlayers = source?.RecommendedPlayers ?? string.Empty,
                EstimatedDuration = source?.EstimatedDuration ?? string.Empty,
                PreviewImagePath = source?.PreviewImagePath ?? string.Empty,
            };

            if (source?.ExtensionPackageIds != null)
            {
                result.ExtensionPackageIds.AddRange(source.ExtensionPackageIds);
            }

            if (source?.AdventureHooks != null)
            {
                for (int index = 0; index < source.AdventureHooks.Count; index++)
                {
                    ModuleAdventureHookDraftData hook = source.AdventureHooks[index];
                    if (hook == null)
                    {
                        continue;
                    }

                    result.AdventureHooks.Add(new ModuleAdventureHookDraftData
                    {
                        HookId = hook.HookId ?? string.Empty,
                        Target = hook.Target ?? string.Empty,
                        HookContent = hook.HookContent ?? string.Empty,
                    });
                }
            }

            return result;
        }

        internal static ModuleChapterDraftData CloneChapter(ModuleChapterDraftData source)
        {
            ModuleChapterDraftData result = new ModuleChapterDraftData
            {
                ChapterId = source?.ChapterId ?? 0,
                ChapterName = source?.ChapterName ?? string.Empty,
                Goal = source?.Goal ?? string.Empty,
                Content = source?.Content ?? string.Empty,
                DmNote = source?.DmNote ?? string.Empty,
                TerrainTag = source?.TerrainTag ?? string.Empty,
                TerrainSubTag = source?.TerrainSubTag ?? string.Empty,
                AddMapHint = source?.AddMapHint ?? string.Empty,
                CreatureInfo = source?.CreatureInfo ?? string.Empty,
                MapImagePath = source?.MapImagePath ?? string.Empty,
                MapGrid = CloneMapGrid(source?.MapGrid),
            };

            if (source?.GridCells != null)
            {
                for (int index = 0; index < source.GridCells.Count; index++)
                {
                    ModuleChapterGridCellDraftData gridCell = source.GridCells[index];
                    if (gridCell == null)
                    {
                        continue;
                    }

                    result.GridCells.Add(new ModuleChapterGridCellDraftData
                    {
                        CellX = gridCell.CellX,
                        CellY = gridCell.CellY,
                        MarkType = gridCell.MarkType,
                    });
                }
            }

            if (source?.Events != null)
            {
                for (int index = 0; index < source.Events.Count; index++)
                {
                    ModuleChapterEventDraftData eventData = CloneEvent(source.Events[index]);
                    if (eventData != null)
                    {
                        result.Events.Add(eventData);
                    }
                }
            }

            if (source?.EventBindings != null)
            {
                for (int index = 0; index < source.EventBindings.Count; index++)
                {
                    ModuleChapterEventBindingDraftData binding = CloneEventBinding(source.EventBindings[index]);
                    if (binding != null)
                    {
                        result.EventBindings.Add(binding);
                    }
                }
            }

            if (source?.CreatureTemplates != null)
            {
                for (int index = 0; index < source.CreatureTemplates.Count; index++)
                {
                    ModuleChapterCreatureDraftData creature = source.CreatureTemplates[index];
                    if (creature == null)
                    {
                        continue;
                    }

                    result.CreatureTemplates.Add(new ModuleChapterCreatureDraftData
                    {
                        CreatureId = creature.CreatureId ?? string.Empty,
                        CreatureName = creature.CreatureName ?? string.Empty,
                        CreatureSize = creature.CreatureSize ?? string.Empty,
                        PreviewImagePath = creature.PreviewImagePath ?? string.Empty,
                    });
                }
            }

            if (source?.CreatureInstances != null)
            {
                for (int index = 0; index < source.CreatureInstances.Count; index++)
                {
                    ModuleChapterCreatureInstanceDraftData creature = source.CreatureInstances[index];
                    if (creature == null)
                    {
                        continue;
                    }

                    result.CreatureInstances.Add(new ModuleChapterCreatureInstanceDraftData
                    {
                        InstanceId = creature.InstanceId ?? string.Empty,
                        CreatureId = creature.CreatureId ?? string.Empty,
                        IsActive = creature.IsActive,
                        Coordinate = CloneCoordinate(creature.Coordinate),
                    });
                }
            }

            return result;
        }

        private static ModuleChapterMapGridDraftData CloneMapGrid(ModuleChapterMapGridDraftData source)
        {
            return new ModuleChapterMapGridDraftData
            {
                IsMapZoomEnabled = source?.IsMapZoomEnabled ?? false,
                IsGridZoomEnabled = source?.IsGridZoomEnabled ?? false,
                MapZoomScale = source?.MapZoomScale ?? 1f,
                MapPanOffset = source?.MapPanOffset ?? Vector2.zero,
                GridZoomScale = source?.GridZoomScale ?? 1f,
                GridPanOffset = source?.GridPanOffset ?? Vector2.zero,
                IsLocked = source?.IsLocked ?? false,
                LockedMapZoomReference = source?.LockedMapZoomReference ?? 1f,
                LockedGridToMapZoomRatio = source?.LockedGridToMapZoomRatio ?? 1f,
                LockedGridToMapPanDelta = source?.LockedGridToMapPanDelta ?? Vector2.zero,
            };
        }

        private static ModuleChapterEventDraftData CloneEvent(ModuleChapterEventDraftData source)
        {
            if (source == null)
            {
                return null;
            }

            return new ModuleChapterEventDraftData
            {
                EventId = source.EventId ?? string.Empty,
                IsEnabled = source.IsEnabled,
                IsOneShot = source.IsOneShot,
                EventTitle = source.EventTitle ?? string.Empty,
                TriggerDescription = source.TriggerDescription ?? string.Empty,
                DmNote = source.DmNote ?? string.Empty,
                Trigger = CloneEventTrigger(source.Trigger),
                Effect = CloneEventEffect(source.Effect),
            };
        }

        private static ModuleChapterEventTriggerDraftData CloneEventTrigger(ModuleChapterEventTriggerDraftData source)
        {
            return new ModuleChapterEventTriggerDraftData
            {
                TriggerMode = source?.TriggerMode ?? 0,
                TriggerType = source?.TriggerType ?? -1,
                AreaFirstEnterOnly = source?.AreaFirstEnterOnly ?? false,
                AreaShareBinding = source?.AreaShareBinding ?? false,
                InteractionTarget = source?.InteractionTarget ?? string.Empty,
                InteractionRequireConfirm = source?.InteractionRequireConfirm ?? false,
                PrerequisiteEventId = source?.PrerequisiteEventId ?? string.Empty,
                DelayDescription = source?.DelayDescription ?? string.Empty,
            };
        }

        private static ModuleChapterEventEffectDraftData CloneEventEffect(ModuleChapterEventEffectDraftData source)
        {
            ModuleChapterEventEffectDraftData result = new ModuleChapterEventEffectDraftData
            {
                EffectType = source?.EffectType ?? -1,
                CheckTargetMode = source?.CheckTargetMode ?? 0,
                CheckResolutionMode = source?.CheckResolutionMode ?? 0,
                SuccessResult = source?.SuccessResult ?? string.Empty,
                FailureResult = source?.FailureResult ?? string.Empty,
                NarrativeText = source?.NarrativeText ?? string.Empty,
                NarrativeDmOnly = source?.NarrativeDmOnly ?? false,
                DialogueTarget = source?.DialogueTarget ?? string.Empty,
                DialogueSummary = source?.DialogueSummary ?? string.Empty,
                DialoguePrompt = source?.DialoguePrompt ?? string.Empty,
                CreatureInstanceId = source?.CreatureInstanceId ?? string.Empty,
                CreatureActivate = source?.CreatureActivate ?? true,
                CreaturePlacementMode = source?.CreaturePlacementMode ?? 0,
                BattleReference = source?.BattleReference ?? string.Empty,
                BattleIncludeActiveCreatures = source?.BattleIncludeActiveCreatures ?? true,
                BattleDescription = source?.BattleDescription ?? string.Empty,
            };

            if (source?.SkillCheckEntries != null)
            {
                for (int index = 0; index < source.SkillCheckEntries.Count; index++)
                {
                    ModuleChapterSkillCheckThresholdDraftData entry = source.SkillCheckEntries[index];
                    if (entry == null)
                    {
                        continue;
                    }

                    result.SkillCheckEntries.Add(new ModuleChapterSkillCheckThresholdDraftData
                    {
                        CheckName = entry.CheckName ?? string.Empty,
                        Threshold = entry.Threshold ?? string.Empty,
                    });
                }
            }

            return result;
        }

        private static ModuleChapterEventBindingDraftData CloneEventBinding(ModuleChapterEventBindingDraftData source)
        {
            if (source == null)
            {
                return null;
            }

            ModuleChapterEventBindingDraftData result = new ModuleChapterEventBindingDraftData
            {
                BindingId = source.BindingId ?? string.Empty,
                EventId = source.EventId ?? string.Empty,
            };

            if (source.GridCoordinates != null)
            {
                for (int index = 0; index < source.GridCoordinates.Count; index++)
                {
                    result.GridCoordinates.Add(CloneCoordinate(source.GridCoordinates[index]));
                }
            }

            return result;
        }

        private static ModuleChapterGridCoordinateDraftData CloneCoordinate(ModuleChapterGridCoordinateDraftData source)
        {
            return new ModuleChapterGridCoordinateDraftData
            {
                CellX = source?.CellX ?? 0,
                CellY = source?.CellY ?? 0,
            };
        }
    }
}
