using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    internal sealed class ModuleAuthoringApplicationService
    {
        private static readonly Lazy<ModuleAuthoringApplicationService> s_instance =
            new Lazy<ModuleAuthoringApplicationService>(() => new ModuleAuthoringApplicationService());

        private ModuleAuthoringApplicationService()
        {
        }

        public static ModuleAuthoringApplicationService Instance => s_instance.Value;

        public ModuleAuthoringDraftState BeginNewDraft(string draftId = "")
        {
            ModuleAuthoringSessionService.Instance.BeginNewDraft(draftId);
            return ModuleAuthoringSessionService.Instance.ExportDraft();
        }

        public ModuleAuthoringOperationResult LoadDraft(string filePath)
        {
            try
            {
                ModuleAuthoringSaveData saveData = ModuleAuthoringRepository.LoadJson<ModuleAuthoringSaveData>(filePath);
                ModuleAuthoringSessionService.Instance.LoadDraft(ToDraftState(saveData));
                return ModuleAuthoringOperationResult.Ok();
            }
            catch (Exception exception)
            {
                ModuleAuthoringSessionService.Instance.BeginNewDraft();
                return ModuleAuthoringOperationResult.Fail($"Load module draft failed: {exception.Message}");
            }
        }

        public ModuleAuthoringOperationResult SaveCurrentDraft(string filePath)
        {
            return SaveDraft(filePath, ModuleAuthoringSessionService.Instance.ExportDraft());
        }

        public ModuleAuthoringOperationResult SaveDraft(string filePath, ModuleAuthoringDraftState draftState)
        {
            try
            {
                ModuleAuthoringRepository.SaveJson(filePath, ToSaveData(draftState));
                ModuleAuthoringSessionService.Instance.MarkClean();
                return ModuleAuthoringOperationResult.Ok();
            }
            catch (Exception exception)
            {
                return ModuleAuthoringOperationResult.Fail($"Save module draft failed: {exception.Message}");
            }
        }

        public ModuleAuthoringSaveData BuildSaveData(ModuleAuthoringDraftState draftState)
        {
            return ToSaveData(draftState);
        }

        public ModuleAuthoringDraftState BuildDraftState(ModuleAuthoringSaveData saveData)
        {
            return ToDraftState(saveData);
        }

        private static ModuleAuthoringSaveData ToSaveData(ModuleAuthoringDraftState draftState)
        {
            ModuleAuthoringDraftState normalizedDraft = ModuleAuthoringSessionService.CloneDraftState(draftState)
                ?? new ModuleAuthoringDraftState();
            ModuleAuthoringSaveData saveData = new ModuleAuthoringSaveData
            {
                DraftId = normalizedDraft.DraftId ?? string.Empty,
                BasicInfo = ToSaveData(normalizedDraft.BasicInfo),
                SelectedChapterId = normalizedDraft.SelectedChapterId,
                NextChapterId = normalizedDraft.NextChapterId,
            };

            if (normalizedDraft.Chapters != null)
            {
                for (int index = 0; index < normalizedDraft.Chapters.Count; index++)
                {
                    saveData.Chapters.Add(ToSaveData(normalizedDraft.Chapters[index]));
                }
            }

            return saveData;
        }

        private static ModuleAuthoringDraftState ToDraftState(ModuleAuthoringSaveData saveData)
        {
            ModuleAuthoringDraftState draftState = new ModuleAuthoringDraftState
            {
                DraftId = saveData?.DraftId ?? string.Empty,
                BasicInfo = ToDraftData(saveData?.BasicInfo),
                SelectedChapterId = saveData?.SelectedChapterId ?? -1,
                NextChapterId = saveData?.NextChapterId ?? 1,
                IsDirty = false,
            };

            if (saveData?.Chapters != null)
            {
                for (int index = 0; index < saveData.Chapters.Count; index++)
                {
                    ModuleChapterDraftData chapter = ToDraftData(saveData.Chapters[index]);
                    if (chapter != null)
                    {
                        draftState.Chapters.Add(chapter);
                    }
                }
            }

            return draftState;
        }

        private static ModuleBasicInfoSaveData ToSaveData(ModuleBasicInfoDraftData basicInfo)
        {
            ModuleBasicInfoSaveData saveData = new ModuleBasicInfoSaveData
            {
                ModuleName = basicInfo?.ModuleName ?? string.Empty,
                ModuleIntroduction = basicInfo?.ModuleIntroduction ?? string.Empty,
                RuleVersion = basicInfo?.RuleVersion ?? string.Empty,
                ExtensionPackageSummary = basicInfo?.ExtensionPackageSummary ?? string.Empty,
                RecommendedLevel = basicInfo?.RecommendedLevel ?? string.Empty,
                RecommendedPlayers = basicInfo?.RecommendedPlayers ?? string.Empty,
                EstimatedDuration = basicInfo?.EstimatedDuration ?? string.Empty,
                PreviewImagePath = basicInfo?.PreviewImagePath ?? string.Empty,
            };

            if (basicInfo?.ExtensionPackageIds != null)
            {
                saveData.ExtensionPackageIds.AddRange(basicInfo.ExtensionPackageIds);
            }

            if (basicInfo?.AdventureHooks != null)
            {
                for (int index = 0; index < basicInfo.AdventureHooks.Count; index++)
                {
                    ModuleAdventureHookDraftData hook = basicInfo.AdventureHooks[index];
                    if (hook == null)
                    {
                        continue;
                    }

                    saveData.AdventureHooks.Add(new ModuleAdventureHookSaveData
                    {
                        HookId = hook.HookId ?? string.Empty,
                        Target = hook.Target ?? string.Empty,
                        HookContent = hook.HookContent ?? string.Empty,
                    });
                }
            }

            return saveData;
        }

        private static ModuleBasicInfoDraftData ToDraftData(ModuleBasicInfoSaveData basicInfo)
        {
            ModuleBasicInfoDraftData draftData = new ModuleBasicInfoDraftData
            {
                ModuleName = basicInfo?.ModuleName ?? string.Empty,
                ModuleIntroduction = basicInfo?.ModuleIntroduction ?? string.Empty,
                RuleVersion = basicInfo?.RuleVersion ?? string.Empty,
                ExtensionPackageSummary = basicInfo?.ExtensionPackageSummary ?? string.Empty,
                RecommendedLevel = basicInfo?.RecommendedLevel ?? string.Empty,
                RecommendedPlayers = basicInfo?.RecommendedPlayers ?? string.Empty,
                EstimatedDuration = basicInfo?.EstimatedDuration ?? string.Empty,
                PreviewImagePath = basicInfo?.PreviewImagePath ?? string.Empty,
            };

            if (basicInfo?.ExtensionPackageIds != null)
            {
                draftData.ExtensionPackageIds.AddRange(basicInfo.ExtensionPackageIds);
            }

            if (basicInfo?.AdventureHooks != null)
            {
                for (int index = 0; index < basicInfo.AdventureHooks.Count; index++)
                {
                    ModuleAdventureHookSaveData hook = basicInfo.AdventureHooks[index];
                    if (hook == null)
                    {
                        continue;
                    }

                    draftData.AdventureHooks.Add(new ModuleAdventureHookDraftData
                    {
                        HookId = hook.HookId ?? string.Empty,
                        Target = hook.Target ?? string.Empty,
                        HookContent = hook.HookContent ?? string.Empty,
                    });
                }
            }

            return draftData;
        }

        private static ModuleChapterSaveData ToSaveData(ModuleChapterDraftData chapter)
        {
            ModuleChapterSaveData saveData = new ModuleChapterSaveData
            {
                ChapterId = chapter?.ChapterId ?? 0,
                ChapterName = chapter?.ChapterName ?? string.Empty,
                Goal = chapter?.Goal ?? string.Empty,
                Content = chapter?.Content ?? string.Empty,
                DmNote = chapter?.DmNote ?? string.Empty,
                TerrainTag = chapter?.TerrainTag ?? string.Empty,
                TerrainSubTag = chapter?.TerrainSubTag ?? string.Empty,
                AddMapHint = chapter?.AddMapHint ?? string.Empty,
                CreatureInfo = chapter?.CreatureInfo ?? string.Empty,
                MapImagePath = chapter?.MapImagePath ?? string.Empty,
                MapGrid = ToSaveData(chapter?.MapGrid),
            };

            if (chapter?.GridCells != null)
            {
                for (int index = 0; index < chapter.GridCells.Count; index++)
                {
                    ModuleChapterGridCellDraftData gridCell = chapter.GridCells[index];
                    if (gridCell == null)
                    {
                        continue;
                    }

                    saveData.GridCells.Add(new ModuleChapterGridCellSaveData
                    {
                        CellX = gridCell.CellX,
                        CellY = gridCell.CellY,
                        MarkType = gridCell.MarkType,
                    });
                }
            }

            AppendEvents(saveData.Events, chapter?.Events);
            AppendEventBindings(saveData.EventBindings, chapter?.EventBindings);
            AppendCreatureTemplates(saveData.CreatureTemplates, chapter?.CreatureTemplates);
            AppendCreatureInstances(saveData.CreatureInstances, chapter?.CreatureInstances);
            return saveData;
        }

        private static ModuleChapterDraftData ToDraftData(ModuleChapterSaveData chapter)
        {
            if (chapter == null)
            {
                return null;
            }

            ModuleChapterDraftData draftData = new ModuleChapterDraftData
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
                MapImagePath = chapter.MapImagePath ?? string.Empty,
                MapGrid = ToDraftData(chapter.MapGrid),
            };

            if (chapter.GridCells != null)
            {
                for (int index = 0; index < chapter.GridCells.Count; index++)
                {
                    ModuleChapterGridCellSaveData gridCell = chapter.GridCells[index];
                    if (gridCell == null)
                    {
                        continue;
                    }

                    draftData.GridCells.Add(new ModuleChapterGridCellDraftData
                    {
                        CellX = gridCell.CellX,
                        CellY = gridCell.CellY,
                        MarkType = gridCell.MarkType,
                    });
                }
            }

            AppendEvents(draftData.Events, chapter.Events);
            AppendEventBindings(draftData.EventBindings, chapter.EventBindings);
            AppendCreatureTemplates(draftData.CreatureTemplates, chapter.CreatureTemplates);
            AppendCreatureInstances(draftData.CreatureInstances, chapter.CreatureInstances);
            return draftData;
        }

        private static ModuleChapterMapGridSaveData ToSaveData(ModuleChapterMapGridDraftData mapGrid)
        {
            Vector2 mapPanOffset = mapGrid != null ? mapGrid.MapPanOffset : Vector2.zero;
            Vector2 gridPanOffset = mapGrid != null ? mapGrid.GridPanOffset : Vector2.zero;
            Vector2 lockedGridToMapPanDelta = mapGrid != null ? mapGrid.LockedGridToMapPanDelta : Vector2.zero;

            return new ModuleChapterMapGridSaveData
            {
                IsMapZoomEnabled = mapGrid?.IsMapZoomEnabled ?? false,
                IsGridZoomEnabled = mapGrid?.IsGridZoomEnabled ?? false,
                MapZoomScale = mapGrid?.MapZoomScale ?? 1f,
                MapPanOffsetX = mapPanOffset.x,
                MapPanOffsetY = mapPanOffset.y,
                GridZoomScale = mapGrid?.GridZoomScale ?? 1f,
                GridPanOffsetX = gridPanOffset.x,
                GridPanOffsetY = gridPanOffset.y,
                IsLocked = mapGrid?.IsLocked ?? false,
                LockedMapZoomReference = mapGrid?.LockedMapZoomReference ?? 1f,
                LockedGridToMapZoomRatio = mapGrid?.LockedGridToMapZoomRatio ?? 1f,
                LockedGridToMapPanDeltaX = lockedGridToMapPanDelta.x,
                LockedGridToMapPanDeltaY = lockedGridToMapPanDelta.y,
            };
        }

        private static ModuleChapterMapGridDraftData ToDraftData(ModuleChapterMapGridSaveData mapGrid)
        {
            return new ModuleChapterMapGridDraftData
            {
                IsMapZoomEnabled = mapGrid?.IsMapZoomEnabled ?? false,
                IsGridZoomEnabled = mapGrid?.IsGridZoomEnabled ?? false,
                MapZoomScale = mapGrid?.MapZoomScale ?? 1f,
                MapPanOffset = new Vector2(mapGrid?.MapPanOffsetX ?? 0f, mapGrid?.MapPanOffsetY ?? 0f),
                GridZoomScale = mapGrid?.GridZoomScale ?? 1f,
                GridPanOffset = new Vector2(mapGrid?.GridPanOffsetX ?? 0f, mapGrid?.GridPanOffsetY ?? 0f),
                IsLocked = mapGrid?.IsLocked ?? false,
                LockedMapZoomReference = mapGrid?.LockedMapZoomReference ?? 1f,
                LockedGridToMapZoomRatio = mapGrid?.LockedGridToMapZoomRatio ?? 1f,
                LockedGridToMapPanDelta = new Vector2(
                    mapGrid?.LockedGridToMapPanDeltaX ?? 0f,
                    mapGrid?.LockedGridToMapPanDeltaY ?? 0f),
            };
        }

        private static void AppendEvents(List<ModuleChapterEventSaveData> target, List<ModuleChapterEventDraftData> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                ModuleChapterEventDraftData eventData = source[index];
                if (eventData == null)
                {
                    continue;
                }

                ModuleChapterEventSaveData savedEvent = new ModuleChapterEventSaveData
                {
                    EventId = eventData.EventId ?? string.Empty,
                    IsEnabled = eventData.IsEnabled,
                    IsOneShot = eventData.IsOneShot,
                    EventTitle = eventData.EventTitle ?? string.Empty,
                    TriggerDescription = eventData.TriggerDescription ?? string.Empty,
                    DmNote = eventData.DmNote ?? string.Empty,
                    Trigger = ToSaveData(eventData.Trigger),
                    Effect = ToSaveData(eventData.Effect),
                };

                target.Add(savedEvent);
            }
        }

        private static void AppendEvents(List<ModuleChapterEventDraftData> target, List<ModuleChapterEventSaveData> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                ModuleChapterEventSaveData eventData = source[index];
                if (eventData == null)
                {
                    continue;
                }

                target.Add(new ModuleChapterEventDraftData
                {
                    EventId = eventData.EventId ?? string.Empty,
                    IsEnabled = eventData.IsEnabled,
                    IsOneShot = eventData.IsOneShot,
                    EventTitle = eventData.EventTitle ?? string.Empty,
                    TriggerDescription = eventData.TriggerDescription ?? string.Empty,
                    DmNote = eventData.DmNote ?? string.Empty,
                    Trigger = ToDraftData(eventData.Trigger),
                    Effect = ToDraftData(eventData.Effect),
                });
            }
        }

        private static ModuleChapterEventTriggerSaveData ToSaveData(ModuleChapterEventTriggerDraftData trigger)
        {
            return new ModuleChapterEventTriggerSaveData
            {
                TriggerMode = trigger?.TriggerMode ?? 0,
                TriggerType = trigger?.TriggerType ?? -1,
                AreaFirstEnterOnly = trigger?.AreaFirstEnterOnly ?? false,
                AreaShareBinding = trigger?.AreaShareBinding ?? false,
                InteractionTarget = trigger?.InteractionTarget ?? string.Empty,
                InteractionRequireConfirm = trigger?.InteractionRequireConfirm ?? false,
                PrerequisiteEventId = trigger?.PrerequisiteEventId ?? string.Empty,
                DelayDescription = trigger?.DelayDescription ?? string.Empty,
            };
        }

        private static ModuleChapterEventTriggerDraftData ToDraftData(ModuleChapterEventTriggerSaveData trigger)
        {
            return new ModuleChapterEventTriggerDraftData
            {
                TriggerMode = trigger?.TriggerMode ?? 0,
                TriggerType = trigger?.TriggerType ?? -1,
                AreaFirstEnterOnly = trigger?.AreaFirstEnterOnly ?? false,
                AreaShareBinding = trigger?.AreaShareBinding ?? false,
                InteractionTarget = trigger?.InteractionTarget ?? string.Empty,
                InteractionRequireConfirm = trigger?.InteractionRequireConfirm ?? false,
                PrerequisiteEventId = trigger?.PrerequisiteEventId ?? string.Empty,
                DelayDescription = trigger?.DelayDescription ?? string.Empty,
            };
        }

        private static ModuleChapterEventEffectSaveData ToSaveData(ModuleChapterEventEffectDraftData effect)
        {
            ModuleChapterEventEffectSaveData saveData = new ModuleChapterEventEffectSaveData
            {
                EffectType = effect?.EffectType ?? -1,
                CheckTargetMode = effect?.CheckTargetMode ?? 0,
                CheckResolutionMode = effect?.CheckResolutionMode ?? 0,
                SuccessResult = effect?.SuccessResult ?? string.Empty,
                FailureResult = effect?.FailureResult ?? string.Empty,
                NarrativeText = effect?.NarrativeText ?? string.Empty,
                NarrativeDmOnly = effect?.NarrativeDmOnly ?? false,
                DialogueTarget = effect?.DialogueTarget ?? string.Empty,
                DialogueSummary = effect?.DialogueSummary ?? string.Empty,
                DialoguePrompt = effect?.DialoguePrompt ?? string.Empty,
                CreatureInstanceId = effect?.CreatureInstanceId ?? string.Empty,
                CreatureActivate = effect?.CreatureActivate ?? true,
                CreaturePlacementMode = effect?.CreaturePlacementMode ?? 0,
                BattleReference = effect?.BattleReference ?? string.Empty,
                BattleIncludeActiveCreatures = effect?.BattleIncludeActiveCreatures ?? true,
                BattleDescription = effect?.BattleDescription ?? string.Empty,
            };

            if (effect?.SkillCheckEntries != null)
            {
                for (int index = 0; index < effect.SkillCheckEntries.Count; index++)
                {
                    ModuleChapterSkillCheckThresholdDraftData entry = effect.SkillCheckEntries[index];
                    if (entry == null)
                    {
                        continue;
                    }

                    saveData.SkillCheckEntries.Add(new ModuleChapterSkillCheckThresholdSaveData
                    {
                        CheckName = entry.CheckName ?? string.Empty,
                        Threshold = entry.Threshold ?? string.Empty,
                    });
                }
            }

            return saveData;
        }

        private static ModuleChapterEventEffectDraftData ToDraftData(ModuleChapterEventEffectSaveData effect)
        {
            ModuleChapterEventEffectDraftData draftData = new ModuleChapterEventEffectDraftData
            {
                EffectType = effect?.EffectType ?? -1,
                CheckTargetMode = effect?.CheckTargetMode ?? 0,
                CheckResolutionMode = effect?.CheckResolutionMode ?? 0,
                SuccessResult = effect?.SuccessResult ?? string.Empty,
                FailureResult = effect?.FailureResult ?? string.Empty,
                NarrativeText = effect?.NarrativeText ?? string.Empty,
                NarrativeDmOnly = effect?.NarrativeDmOnly ?? false,
                DialogueTarget = effect?.DialogueTarget ?? string.Empty,
                DialogueSummary = effect?.DialogueSummary ?? string.Empty,
                DialoguePrompt = effect?.DialoguePrompt ?? string.Empty,
                CreatureInstanceId = effect?.CreatureInstanceId ?? string.Empty,
                CreatureActivate = effect?.CreatureActivate ?? true,
                CreaturePlacementMode = effect?.CreaturePlacementMode ?? 0,
                BattleReference = effect?.BattleReference ?? string.Empty,
                BattleIncludeActiveCreatures = effect?.BattleIncludeActiveCreatures ?? true,
                BattleDescription = effect?.BattleDescription ?? string.Empty,
            };

            if (effect?.SkillCheckEntries != null)
            {
                for (int index = 0; index < effect.SkillCheckEntries.Count; index++)
                {
                    ModuleChapterSkillCheckThresholdSaveData entry = effect.SkillCheckEntries[index];
                    if (entry == null)
                    {
                        continue;
                    }

                    draftData.SkillCheckEntries.Add(new ModuleChapterSkillCheckThresholdDraftData
                    {
                        CheckName = entry.CheckName ?? string.Empty,
                        Threshold = entry.Threshold ?? string.Empty,
                    });
                }
            }

            return draftData;
        }

        private static void AppendEventBindings(List<ModuleChapterEventBindingSaveData> target, List<ModuleChapterEventBindingDraftData> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                ModuleChapterEventBindingDraftData binding = source[index];
                if (binding == null)
                {
                    continue;
                }

                ModuleChapterEventBindingSaveData saveData = new ModuleChapterEventBindingSaveData
                {
                    BindingId = binding.BindingId ?? string.Empty,
                    EventId = binding.EventId ?? string.Empty,
                };

                if (binding.GridCoordinates != null)
                {
                    for (int coordinateIndex = 0; coordinateIndex < binding.GridCoordinates.Count; coordinateIndex++)
                    {
                        ModuleChapterGridCoordinateDraftData coordinate = binding.GridCoordinates[coordinateIndex];
                        if (coordinate == null)
                        {
                            continue;
                        }

                        saveData.GridCoordinates.Add(new ModuleChapterGridCoordinateSaveData
                        {
                            CellX = coordinate.CellX,
                            CellY = coordinate.CellY,
                        });
                    }
                }

                target.Add(saveData);
            }
        }

        private static void AppendEventBindings(List<ModuleChapterEventBindingDraftData> target, List<ModuleChapterEventBindingSaveData> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                ModuleChapterEventBindingSaveData binding = source[index];
                if (binding == null)
                {
                    continue;
                }

                ModuleChapterEventBindingDraftData draftData = new ModuleChapterEventBindingDraftData
                {
                    BindingId = binding.BindingId ?? string.Empty,
                    EventId = binding.EventId ?? string.Empty,
                };

                if (binding.GridCoordinates != null)
                {
                    for (int coordinateIndex = 0; coordinateIndex < binding.GridCoordinates.Count; coordinateIndex++)
                    {
                        ModuleChapterGridCoordinateSaveData coordinate = binding.GridCoordinates[coordinateIndex];
                        if (coordinate == null)
                        {
                            continue;
                        }

                        draftData.GridCoordinates.Add(new ModuleChapterGridCoordinateDraftData
                        {
                            CellX = coordinate.CellX,
                            CellY = coordinate.CellY,
                        });
                    }
                }

                target.Add(draftData);
            }
        }

        private static void AppendCreatureTemplates(List<ModuleChapterCreatureSaveData> target, List<ModuleChapterCreatureDraftData> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                ModuleChapterCreatureDraftData creature = source[index];
                if (creature == null)
                {
                    continue;
                }

                target.Add(new ModuleChapterCreatureSaveData
                {
                    CreatureId = creature.CreatureId ?? string.Empty,
                    CreatureName = creature.CreatureName ?? string.Empty,
                    CreatureSize = creature.CreatureSize ?? string.Empty,
                    PreviewImagePath = creature.PreviewImagePath ?? string.Empty,
                });
            }
        }

        private static void AppendCreatureTemplates(List<ModuleChapterCreatureDraftData> target, List<ModuleChapterCreatureSaveData> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                ModuleChapterCreatureSaveData creature = source[index];
                if (creature == null)
                {
                    continue;
                }

                target.Add(new ModuleChapterCreatureDraftData
                {
                    CreatureId = creature.CreatureId ?? string.Empty,
                    CreatureName = creature.CreatureName ?? string.Empty,
                    CreatureSize = creature.CreatureSize ?? string.Empty,
                    PreviewImagePath = creature.PreviewImagePath ?? string.Empty,
                });
            }
        }

        private static void AppendCreatureInstances(List<ModuleChapterCreatureInstanceSaveData> target, List<ModuleChapterCreatureInstanceDraftData> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                ModuleChapterCreatureInstanceDraftData creature = source[index];
                if (creature == null)
                {
                    continue;
                }

                target.Add(new ModuleChapterCreatureInstanceSaveData
                {
                    InstanceId = creature.InstanceId ?? string.Empty,
                    CreatureId = creature.CreatureId ?? string.Empty,
                    IsActive = creature.IsActive,
                    Coordinate = ToSaveData(creature.Coordinate),
                });
            }
        }

        private static void AppendCreatureInstances(List<ModuleChapterCreatureInstanceDraftData> target, List<ModuleChapterCreatureInstanceSaveData> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                ModuleChapterCreatureInstanceSaveData creature = source[index];
                if (creature == null)
                {
                    continue;
                }

                target.Add(new ModuleChapterCreatureInstanceDraftData
                {
                    InstanceId = creature.InstanceId ?? string.Empty,
                    CreatureId = creature.CreatureId ?? string.Empty,
                    IsActive = creature.IsActive,
                    Coordinate = ToDraftData(creature.Coordinate),
                });
            }
        }

        private static ModuleChapterGridCoordinateSaveData ToSaveData(ModuleChapterGridCoordinateDraftData coordinate)
        {
            return new ModuleChapterGridCoordinateSaveData
            {
                CellX = coordinate?.CellX ?? 0,
                CellY = coordinate?.CellY ?? 0,
            };
        }

        private static ModuleChapterGridCoordinateDraftData ToDraftData(ModuleChapterGridCoordinateSaveData coordinate)
        {
            return new ModuleChapterGridCoordinateDraftData
            {
                CellX = coordinate?.CellX ?? 0,
                CellY = coordinate?.CellY ?? 0,
            };
        }
    }
}
