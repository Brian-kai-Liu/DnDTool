using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    internal static class ChapterEditorPersistenceService
    {
        public static string GetSaveFilePath(string fileName)
        {
            return ModuleAuthoringRepository.GetChapterEditorSaveFilePath(fileName);
        }

        public static string GetCreaturePreviewDirectoryPath()
        {
            return ModuleAuthoringRepository.GetCreaturePreviewDirectoryPath();
        }

        public static string GetChapterMapDirectoryPath()
        {
            return ModuleAuthoringRepository.GetChapterMapDirectoryPath();
        }

        public static string ResolveCreaturePreviewPath(string previewImageFileName)
        {
            return ModuleAuthoringRepository.ResolveCreaturePreviewPath(previewImageFileName);
        }

        public static string StoreCreaturePreviewImage(string sourceFilePath)
        {
            return ModuleAuthoringRepository.StoreCreaturePreviewImage(sourceFilePath);
        }

        public static string StoreChapterMapImage(string sourceFilePath, int chapterId)
        {
            return ModuleAuthoringRepository.StoreChapterMapImage(sourceFilePath, chapterId);
        }

        public static bool FileExists(string filePath)
        {
            return ModuleAuthoringRepository.FileExists(filePath);
        }

        public static string GetDisplayFileName(string filePath)
        {
            return ModuleAuthoringRepository.GetDisplayFileName(filePath);
        }

        public static byte[] ReadFileBytes(string filePath)
        {
            return ModuleAuthoringRepository.ReadFileBytes(filePath);
        }

        public static bool TryDeleteManagedChapterMapFile(string filePath)
        {
            return ModuleAuthoringRepository.TryDeleteManagedChapterMapFile(filePath);
        }

        public static ChapterEditorSaveData Load(string filePath)
        {
            ChapterEditorLegacySaveData legacySaveData = ModuleAuthoringRepository.LoadJson<ChapterEditorLegacySaveData>(filePath);
            return ToCurrentSaveData(legacySaveData);
        }

        public static void Save(string filePath, ChapterEditorSaveData saveData)
        {
            ModuleAuthoringRepository.SaveJson(filePath, saveData);
        }

        private static ChapterEditorSaveData ToCurrentSaveData(ChapterEditorLegacySaveData saveData)
        {
            if (saveData == null)
            {
                return null;
            }

            ChapterEditorSaveData currentSaveData = new ChapterEditorSaveData
            {
                SelectedChapterId = saveData.SelectedChapterId,
                NextChapterId = saveData.NextChapterId,
            };

            if (saveData.Chapters == null)
            {
                return currentSaveData;
            }

            for (int index = 0; index < saveData.Chapters.Count; index++)
            {
                ChapterItemSaveData chapter = ToCurrentSaveData(saveData.Chapters[index]);
                if (chapter != null)
                {
                    currentSaveData.Chapters.Add(chapter);
                }
            }

            return currentSaveData;
        }

        private static ChapterItemSaveData ToCurrentSaveData(ChapterItemLegacySaveData chapter)
        {
            if (chapter == null)
            {
                return null;
            }

            ChapterItemSaveData currentChapter = new ChapterItemSaveData
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
                IsMapZoomEnabled = chapter.IsMapZoomEnabled,
                IsGridZoomEnabled = chapter.IsGridZoomEnabled,
                MapZoomScale = chapter.MapZoomScale,
                MapPanOffset = chapter.MapPanOffset,
                GridZoomScale = chapter.GridZoomScale,
                GridPanOffset = chapter.GridPanOffset,
                IsMapGridLocked = chapter.IsMapGridLocked,
                LockedMapZoomReference = chapter.LockedMapZoomReference,
                LockedGridToMapZoomRatio = chapter.LockedGridToMapZoomRatio,
                LockedGridToMapPanDelta = chapter.LockedGridToMapPanDelta,
                SelectedGridCellKeys = CloneSelectedGridCellKeys(chapter.SelectedGridCellKeys),
                Creatures = ChapterCreatureDataStructureUtility.CloneCreatureSaveDataList(chapter.Creatures),
                CreatureInstances = ChapterCreatureDataStructureUtility.CloneCreatureInstanceSaveDataList(chapter.CreatureInstances),
                EventBindings = CloneEventBindingSaveDataList(chapter.EventBindings),
            };

            if (chapter.GridCells != null)
            {
                for (int index = 0; index < chapter.GridCells.Count; index++)
                {
                    ChapterGridCellLegacySaveData gridCell = chapter.GridCells[index];
                    if (gridCell == null)
                    {
                        continue;
                    }

                    currentChapter.GridCells.Add(new ChapterGridCellSaveData
                    {
                        CellX = gridCell.CellX,
                        CellY = gridCell.CellY,
                        MarkType = gridCell.MarkType,
                        EventData = gridCell.EventData != null
                            ? ChapterEventDataStructureUtility.NormalizeLegacySaveEventData(gridCell.EventData)
                            : null,
                    });
                }
            }

            if (chapter.Events != null)
            {
                for (int index = 0; index < chapter.Events.Count; index++)
                {
                    ChapterGridEventSaveData eventData = ChapterEventDataStructureUtility.NormalizeLegacySaveEventData(chapter.Events[index]);
                    if (eventData != null)
                    {
                        currentChapter.Events.Add(eventData);
                    }
                }
            }

            return currentChapter;
        }

        private static List<string> CloneSelectedGridCellKeys(List<string> keys)
        {
            List<string> result = new List<string>();
            if (keys == null)
            {
                return result;
            }

            for (int index = 0; index < keys.Count; index++)
            {
                result.Add(keys[index] ?? string.Empty);
            }

            return result;
        }

        private static List<ChapterEventBindingSaveData> CloneEventBindingSaveDataList(List<ChapterEventBindingSaveData> bindings)
        {
            List<ChapterEventBindingSaveData> result = new List<ChapterEventBindingSaveData>();
            if (bindings == null)
            {
                return result;
            }

            for (int index = 0; index < bindings.Count; index++)
            {
                ChapterEventBindingSaveData binding = CloneEventBindingSaveData(bindings[index]);
                if (binding != null)
                {
                    result.Add(binding);
                }
            }

            return result;
        }

        private static ChapterEventBindingSaveData CloneEventBindingSaveData(ChapterEventBindingSaveData binding)
        {
            if (binding == null)
            {
                return null;
            }

            ChapterEventBindingSaveData result = new ChapterEventBindingSaveData
            {
                BindingId = binding.BindingId ?? string.Empty,
                EventId = binding.EventId ?? string.Empty,
            };

            if (binding.GridCoordinates != null)
            {
                for (int index = 0; index < binding.GridCoordinates.Count; index++)
                {
                    ChapterGridCoordinateSaveData coordinate = binding.GridCoordinates[index];
                    if (coordinate == null)
                    {
                        continue;
                    }

                    result.GridCoordinates.Add(new ChapterGridCoordinateSaveData
                    {
                        CellX = coordinate.CellX,
                        CellY = coordinate.CellY,
                    });
                }
            }

            return result;
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
                    if (gridCell == null
                        || gridCell.MarkType == ChapterGridCellMarkType.Selected
                        || gridCell.MarkType == ChapterGridCellMarkType.Event)
                    {
                        continue;
                    }

                    saveData.GridCells.Add(new ChapterGridCellSaveData
                    {
                        CellX = gridCell.Coordinate.CellX,
                        CellY = gridCell.Coordinate.CellY,
                        MarkType = (int) gridCell.MarkType,
                        EventData = gridCell.MarkType == ChapterGridCellMarkType.Event
                            ? ToSaveData(gridCell.EventData)
                            : null,
                    });
                }
            }

            List<ChapterGridEventData> events = chapter?.Events;
            if (events != null)
            {
                for (int index = 0; index < events.Count; index++)
                {
                    ChapterGridEventSaveData savedEvent = ToSaveData(events[index]);
                    if (savedEvent != null)
                    {
                        saveData.Events.Add(savedEvent);
                    }
                }
            }

            List<ChapterEventBindingData> eventBindings = chapter?.EventBindings;
            if (eventBindings != null)
            {
                for (int index = 0; index < eventBindings.Count; index++)
                {
                    ChapterEventBindingSaveData savedBinding = ToSaveData(eventBindings[index]);
                    if (savedBinding != null)
                    {
                        saveData.EventBindings.Add(savedBinding);
                    }
                }
            }

            List<ChapterCreatureData> creatures = chapter?.Creatures;
            if (creatures != null)
            {
                for (int index = 0; index < creatures.Count; index++)
                {
                    if (creatures[index] != null)
                    {
                        saveData.Creatures.Add(ToSaveData(creatures[index]));
                    }
                }
            }

            List<ChapterCreatureInstanceData> creatureInstances = chapter?.CreatureInstances;
            if (creatureInstances != null)
            {
                for (int index = 0; index < creatureInstances.Count; index++)
                {
                    ChapterCreatureInstanceSaveData savedInstance = ToSaveData(creatureInstances[index]);
                    if (savedInstance != null)
                    {
                        saveData.CreatureInstances.Add(savedInstance);
                    }
                }
            }

            return saveData;
        }

        private static ChapterListItemData ToRuntimeData(ChapterItemSaveData chapter)
        {
            List<ChapterGridCellData> gridCells = new List<ChapterGridCellData>();
            List<ChapterGridEventData> events = new List<ChapterGridEventData>();
            List<ChapterEventBindingData> eventBindings = new List<ChapterEventBindingData>();
            if (chapter.GridCells != null && chapter.GridCells.Count > 0)
            {
                for (int index = 0; index < chapter.GridCells.Count; index++)
                {
                    ChapterGridCellSaveData gridCell = chapter.GridCells[index];
                    if (gridCell == null)
                    {
                        continue;
                    }

                    if (gridCell.MarkType == (int) ChapterGridCellMarkType.Event)
                    {
                        ChapterGridEventData legacyEvent = ToRuntimeData(gridCell.EventData);
                        if (legacyEvent != null)
                        {
                            if (string.IsNullOrWhiteSpace(legacyEvent.EventId))
                            {
                                legacyEvent.EventId = $"evt_{Guid.NewGuid():N}";
                            }

                            events.Add(legacyEvent);
                            eventBindings.Add(new ChapterEventBindingData
                            {
                                BindingId = $"bind_{Guid.NewGuid():N}",
                                EventId = legacyEvent.EventId,
                                GridCoordinates = new List<ChapterGridCoordinate>
                                {
                                    new ChapterGridCoordinate(gridCell.CellX, gridCell.CellY),
                                },
                            });
                        }

                        continue;
                    }

                    gridCells.Add(new ChapterGridCellData
                    {
                        Coordinate = new ChapterGridCoordinate(gridCell.CellX, gridCell.CellY),
                        MarkType = System.Enum.IsDefined(typeof(ChapterGridCellMarkType), gridCell.MarkType)
                            ? (ChapterGridCellMarkType) gridCell.MarkType
                            : ChapterGridCellMarkType.Selected,
                        EventData = null,
                    });
                }

                ChapterGridCellCollectionUtility.NormalizeExclusiveTerrainMarks(gridCells);
            }

            if (chapter.Events != null && chapter.Events.Count > 0)
            {
                for (int index = 0; index < chapter.Events.Count; index++)
                {
                    ChapterGridEventData eventData = ToRuntimeData(chapter.Events[index]);
                    if (eventData != null)
                    {
                        if (string.IsNullOrWhiteSpace(eventData.EventId))
                        {
                            eventData.EventId = $"evt_{Guid.NewGuid():N}";
                        }

                        events.Add(eventData);
                    }
                }
            }

            if (chapter.EventBindings != null && chapter.EventBindings.Count > 0)
            {
                for (int index = 0; index < chapter.EventBindings.Count; index++)
                {
                    ChapterEventBindingData binding = ToRuntimeData(chapter.EventBindings[index]);
                    if (binding != null)
                    {
                        if (string.IsNullOrWhiteSpace(binding.BindingId))
                        {
                            binding.BindingId = $"bind_{Guid.NewGuid():N}";
                        }

                        eventBindings.Add(binding);
                    }
                }
            }

            ChapterListItemData result = new ChapterListItemData
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
                Events = events,
                EventBindings = eventBindings,
            };

            if (chapter.Creatures != null && chapter.Creatures.Count > 0)
            {
                List<ChapterCreatureData> runtimeCreatures = new List<ChapterCreatureData>(chapter.Creatures.Count);
                for (int index = 0; index < chapter.Creatures.Count; index++)
                {
                    if (chapter.Creatures[index] != null)
                    {
                        runtimeCreatures.Add(ToRuntimeData(chapter.Creatures[index]));
                    }
                }

                result.Creatures = runtimeCreatures;
            }

            if (chapter.CreatureInstances != null && chapter.CreatureInstances.Count > 0)
            {
                List<ChapterCreatureInstanceData> runtimeCreatureInstances = new List<ChapterCreatureInstanceData>(chapter.CreatureInstances.Count);
                for (int index = 0; index < chapter.CreatureInstances.Count; index++)
                {
                    ChapterCreatureInstanceSaveData creatureInstance = chapter.CreatureInstances[index];
                    if (creatureInstance != null)
                    {
                        runtimeCreatureInstances.Add(ToRuntimeData(creatureInstance));
                    }
                }

                result.CreatureInstances = runtimeCreatureInstances;
            }

            return result;
        }

        private static ChapterCreatureDataSaveData ToSaveData(ChapterCreatureData creature)
        {
            creature = ChapterCreatureDataStructureUtility.NormalizeCreatureTemplateData(creature);
            if (creature == null)
            {
                return null;
            }

            Color c = creature.AccentColor;
            return new ChapterCreatureDataSaveData
            {
                CreatureId = creature.CreatureId ?? string.Empty,
                Name = creature.Name ?? string.Empty,
                NameEn = creature.NameEn ?? string.Empty,
                CreatureType = creature.CreatureType ?? string.Empty,
                CreatureSize = creature.CreatureSize ?? string.Empty,
                Alignment = creature.Alignment ?? string.Empty,
                ChallengeRating = creature.ChallengeRating ?? string.Empty,
                ExperiencePoints = creature.ExperiencePoints ?? string.Empty,
                ArmorClass = creature.ArmorClass ?? string.Empty,
                HitPoints = creature.HitPoints ?? string.Empty,
                Speed = creature.Speed ?? string.Empty,
                Strength = creature.Strength ?? string.Empty,
                Dexterity = creature.Dexterity ?? string.Empty,
                Constitution = creature.Constitution ?? string.Empty,
                Intelligence = creature.Intelligence ?? string.Empty,
                Wisdom = creature.Wisdom ?? string.Empty,
                Charisma = creature.Charisma ?? string.Empty,
                SavingThrows = creature.SavingThrows ?? string.Empty,
                Skills = creature.Skills ?? string.Empty,
                Senses = creature.Senses ?? string.Empty,
                Languages = creature.Languages ?? string.Empty,
                DamageResistances = creature.DamageResistances ?? string.Empty,
                DamageImmunities = creature.DamageImmunities ?? string.Empty,
                ConditionImmunities = creature.ConditionImmunities ?? string.Empty,
                Traits = creature.Traits ?? string.Empty,
                Actions = creature.Actions ?? string.Empty,
                BonusActions = creature.BonusActions ?? string.Empty,
                Reactions = creature.Reactions ?? string.Empty,
                LegendaryActions = creature.LegendaryActions ?? string.Empty,
                BattleNotes = creature.BattleNotes ?? string.Empty,
                PreviewImageFileName = creature.PreviewImageFileName ?? string.Empty,
                AccentColorR = c.r,
                AccentColorG = c.g,
                AccentColorB = c.b,
                AccentColorA = c.a,
            };
        }

        private static ChapterCreatureData ToRuntimeData(ChapterCreatureDataSaveData saved)
        {
            if (saved == null)
            {
                return null;
            }

            return ChapterCreatureDataStructureUtility.NormalizeCreatureTemplateData(new ChapterCreatureData
            {
                CreatureId = saved.CreatureId ?? string.Empty,
                Name = saved.Name ?? string.Empty,
                NameEn = saved.NameEn ?? string.Empty,
                CreatureType = saved.CreatureType ?? string.Empty,
                CreatureSize = saved.CreatureSize ?? string.Empty,
                Alignment = saved.Alignment ?? string.Empty,
                ChallengeRating = saved.ChallengeRating ?? string.Empty,
                ExperiencePoints = saved.ExperiencePoints ?? string.Empty,
                ArmorClass = saved.ArmorClass ?? string.Empty,
                HitPoints = saved.HitPoints ?? string.Empty,
                Speed = saved.Speed ?? string.Empty,
                Strength = saved.Strength ?? string.Empty,
                Dexterity = saved.Dexterity ?? string.Empty,
                Constitution = saved.Constitution ?? string.Empty,
                Intelligence = saved.Intelligence ?? string.Empty,
                Wisdom = saved.Wisdom ?? string.Empty,
                Charisma = saved.Charisma ?? string.Empty,
                SavingThrows = saved.SavingThrows ?? string.Empty,
                Skills = saved.Skills ?? string.Empty,
                Senses = saved.Senses ?? string.Empty,
                Languages = saved.Languages ?? string.Empty,
                DamageResistances = saved.DamageResistances ?? string.Empty,
                DamageImmunities = saved.DamageImmunities ?? string.Empty,
                ConditionImmunities = saved.ConditionImmunities ?? string.Empty,
                Traits = saved.Traits ?? string.Empty,
                Actions = saved.Actions ?? string.Empty,
                BonusActions = saved.BonusActions ?? string.Empty,
                Reactions = saved.Reactions ?? string.Empty,
                LegendaryActions = saved.LegendaryActions ?? string.Empty,
                BattleNotes = saved.BattleNotes ?? string.Empty,
                PreviewImageFileName = saved.PreviewImageFileName ?? string.Empty,
                AccentColor = new UnityEngine.Color(saved.AccentColorR, saved.AccentColorG, saved.AccentColorB, saved.AccentColorA),
            });
        }

        private static ChapterCreatureInstanceSaveData ToSaveData(ChapterCreatureInstanceData creature)
        {
            creature = ChapterCreatureDataStructureUtility.NormalizeCreatureInstanceData(creature);
            if (creature == null)
            {
                return null;
            }

            ChapterCreatureInstancePlacementData placement = creature.Placement ?? new ChapterCreatureInstancePlacementData();
            return new ChapterCreatureInstanceSaveData
            {
                InstanceId = creature.InstanceId ?? string.Empty,
                SourceCreatureId = creature.SourceCreatureId ?? string.Empty,
                IsActive = creature.IsActive,
                Placement = new ChapterCreatureInstancePlacementSaveData
                {
                    AnchorCellX = placement.AnchorCell.CellX,
                    AnchorCellY = placement.AnchorCell.CellY,
                    PreviewScale = placement.PreviewScale,
                    SnapToGrid = placement.SnapToGrid,
                },
                RuntimeSheet = ToSaveData(creature.RuntimeSheet),
                DmNote = creature.DmNote ?? string.Empty,
            };
        }

        private static ChapterCreatureInstanceData ToRuntimeData(ChapterCreatureInstanceSaveData saved)
        {
            saved = ChapterCreatureDataStructureUtility.NormalizeCreatureInstanceSaveData(saved);
            if (saved == null)
            {
                return null;
            }

            ChapterCreatureInstancePlacementSaveData placement = saved.Placement ?? new ChapterCreatureInstancePlacementSaveData();
            return ChapterCreatureDataStructureUtility.NormalizeCreatureInstanceData(new ChapterCreatureInstanceData
            {
                InstanceId = saved.InstanceId ?? string.Empty,
                SourceCreatureId = saved.SourceCreatureId ?? string.Empty,
                IsActive = saved.IsActive,
                Placement = new ChapterCreatureInstancePlacementData
                {
                    AnchorCell = new ChapterGridCoordinate(placement.AnchorCellX, placement.AnchorCellY),
                    PreviewScale = placement.PreviewScale,
                    SnapToGrid = placement.SnapToGrid,
                },
                RuntimeSheet = ToRuntimeData(saved.RuntimeSheet),
                DmNote = saved.DmNote ?? string.Empty,
            });
        }

        private static ChapterGridEventSaveData ToSaveData(ChapterGridEventData eventData)
        {
            eventData = ChapterEventDataStructureUtility.NormalizeRuntimeEventData(eventData);
            if (eventData == null)
            {
                return null;
            }

            ChapterEventTriggerData triggerData = eventData.Trigger ?? new ChapterEventTriggerData();
            ChapterEventEffectData effectData = eventData.Effect ?? new ChapterEventEffectData();
            ChapterEventAreaTriggerParamData areaTriggerParam = triggerData.Area ?? new ChapterEventAreaTriggerParamData();
            ChapterEventInteractionTriggerParamData interactionTriggerParam = triggerData.Interaction ?? new ChapterEventInteractionTriggerParamData();
            ChapterEventPrerequisiteTriggerParamData prerequisiteTriggerParam = triggerData.Prerequisite ?? new ChapterEventPrerequisiteTriggerParamData();
            ChapterEventCheckEffectParamData checkEffectParam = effectData.Check ?? new ChapterEventCheckEffectParamData();
            ChapterEventNarrativeEffectParamData narrativeEffectParam = effectData.Narrative ?? new ChapterEventNarrativeEffectParamData();
            ChapterEventDialogueEffectParamData dialogueEffectParam = effectData.Dialogue ?? new ChapterEventDialogueEffectParamData();
            ChapterEventCreatureEffectParamData creatureEffectParam = effectData.Creature ?? new ChapterEventCreatureEffectParamData();
            ChapterEventBattleEffectParamData battleEffectParam = effectData.Battle ?? new ChapterEventBattleEffectParamData();
            List<ChapterSkillCheckThresholdSaveData> skillCheckEntries = new List<ChapterSkillCheckThresholdSaveData>();
            if (checkEffectParam.SkillCheckEntries != null)
            {
                for (int index = 0; index < checkEffectParam.SkillCheckEntries.Count; index++)
                {
                    ChapterSkillCheckThresholdData entry = checkEffectParam.SkillCheckEntries[index];
                    if (entry == null)
                    {
                        continue;
                    }

                    skillCheckEntries.Add(new ChapterSkillCheckThresholdSaveData
                    {
                        SkillName = entry.SkillName ?? string.Empty,
                        Threshold = entry.Threshold ?? string.Empty,
                    });
                }
            }

            return new ChapterGridEventSaveData
            {
                EventId = eventData.EventId ?? string.Empty,
                IsEnabled = eventData.IsEnabled,
                IsOneShot = eventData.IsOneShot,
                Trigger = ChapterEventDataStructureUtility.CloneSaveTrigger(new ChapterEventTriggerSaveData
                {
                    TriggerMode = triggerData.TriggerMode,
                    TriggerType = triggerData.TriggerType,
                    Area = ChapterEventDataStructureUtility.CloneSaveAreaTriggerParam(new ChapterEventAreaTriggerParamSaveData
                    {
                        FirstEnterOnly = areaTriggerParam.FirstEnterOnly,
                        ShareBinding = areaTriggerParam.ShareBinding,
                    }),
                    Interaction = ChapterEventDataStructureUtility.CloneSaveInteractionTriggerParam(new ChapterEventInteractionTriggerParamSaveData
                    {
                        Target = interactionTriggerParam.Target ?? string.Empty,
                        RequireConfirm = interactionTriggerParam.RequireConfirm,
                    }),
                    Prerequisite = ChapterEventDataStructureUtility.CloneSavePrerequisiteTriggerParam(new ChapterEventPrerequisiteTriggerParamSaveData
                    {
                        EventId = prerequisiteTriggerParam.EventId ?? string.Empty,
                        DelayDescription = prerequisiteTriggerParam.DelayDescription ?? string.Empty,
                    }),
                    AreaFirstEnterOnly = triggerData.AreaFirstEnterOnly,
                    AreaShareBinding = triggerData.AreaShareBinding,
                    InteractionTarget = triggerData.InteractionTarget ?? string.Empty,
                    InteractionRequireConfirm = triggerData.InteractionRequireConfirm,
                    PrerequisiteEventId = triggerData.PrerequisiteEventId ?? string.Empty,
                    DelayDescription = triggerData.DelayDescription ?? string.Empty,
                }),
                Effect = ChapterEventDataStructureUtility.CloneSaveEffect(new ChapterEventEffectSaveData
                {
                    EffectType = effectData.EffectType,
                    Check = ChapterEventDataStructureUtility.CloneSaveCheckEffectParam(new ChapterEventCheckEffectParamSaveData
                    {
                        TargetMode = checkEffectParam.TargetMode,
                        ResolutionMode = checkEffectParam.ResolutionMode,
                        SuccessResult = checkEffectParam.SuccessResult ?? string.Empty,
                        FailureResult = checkEffectParam.FailureResult ?? string.Empty,
                        SkillCheckEntries = skillCheckEntries,
                        SkillCheckName = checkEffectParam.SkillCheckName ?? string.Empty,
                        SkillCheckThreshold = checkEffectParam.SkillCheckThreshold ?? string.Empty,
                        AbilityStrengthThreshold = checkEffectParam.AbilityStrengthThreshold ?? string.Empty,
                        AbilityDexterityThreshold = checkEffectParam.AbilityDexterityThreshold ?? string.Empty,
                        AbilityConstitutionThreshold = checkEffectParam.AbilityConstitutionThreshold ?? string.Empty,
                        AbilityIntelligenceThreshold = checkEffectParam.AbilityIntelligenceThreshold ?? string.Empty,
                        AbilityWisdomThreshold = checkEffectParam.AbilityWisdomThreshold ?? string.Empty,
                        AbilityCharismaThreshold = checkEffectParam.AbilityCharismaThreshold ?? string.Empty,
                    }),
                    Narrative = ChapterEventDataStructureUtility.CloneSaveNarrativeEffectParam(new ChapterEventNarrativeEffectParamSaveData
                    {
                        Text = narrativeEffectParam.Text ?? string.Empty,
                        DmOnly = narrativeEffectParam.DmOnly,
                    }),
                    Dialogue = ChapterEventDataStructureUtility.CloneSaveDialogueEffectParam(new ChapterEventDialogueEffectParamSaveData
                    {
                        Target = dialogueEffectParam.Target ?? string.Empty,
                        Summary = dialogueEffectParam.Summary ?? string.Empty,
                        Prompt = dialogueEffectParam.Prompt ?? string.Empty,
                    }),
                    Creature = ChapterEventDataStructureUtility.CloneSaveCreatureEffectParam(new ChapterEventCreatureEffectParamSaveData
                    {
                        InstanceId = creatureEffectParam.InstanceId ?? string.Empty,
                        Activate = creatureEffectParam.Activate,
                        PlacementMode = creatureEffectParam.PlacementMode,
                    }),
                    Battle = ChapterEventDataStructureUtility.CloneSaveBattleEffectParam(new ChapterEventBattleEffectParamSaveData
                    {
                        Reference = battleEffectParam.Reference ?? string.Empty,
                        IncludeActiveCreatures = battleEffectParam.IncludeActiveCreatures,
                        Description = battleEffectParam.Description ?? string.Empty,
                    }),
                    CheckTargetMode = effectData.CheckTargetMode,
                    CheckResolutionMode = effectData.CheckResolutionMode,
                    SuccessResult = effectData.SuccessResult ?? string.Empty,
                    FailureResult = effectData.FailureResult ?? string.Empty,
                    LegacyDmPrompt = effectData.LegacyDmPrompt ?? string.Empty,
                    NarrativeText = effectData.NarrativeText ?? string.Empty,
                    NarrativeDmOnly = effectData.NarrativeDmOnly,
                    DialogueTarget = effectData.DialogueTarget ?? string.Empty,
                    DialogueSummary = effectData.DialogueSummary ?? string.Empty,
                    DialoguePrompt = effectData.DialoguePrompt ?? string.Empty,
                    CreatureInstanceId = effectData.CreatureInstanceId ?? string.Empty,
                    CreatureActivate = effectData.CreatureActivate,
                    CreaturePlacementMode = effectData.CreaturePlacementMode,
                    BattleReference = effectData.BattleReference ?? string.Empty,
                    BattleIncludeActiveCreatures = effectData.BattleIncludeActiveCreatures,
                    BattleDescription = effectData.BattleDescription ?? string.Empty,
                    SkillCheckEntries = skillCheckEntries,
                    SkillCheckName = effectData.SkillCheckName ?? string.Empty,
                    SkillCheckThreshold = effectData.SkillCheckThreshold ?? string.Empty,
                    AbilityStrengthThreshold = effectData.AbilityStrengthThreshold ?? string.Empty,
                    AbilityDexterityThreshold = effectData.AbilityDexterityThreshold ?? string.Empty,
                    AbilityConstitutionThreshold = effectData.AbilityConstitutionThreshold ?? string.Empty,
                    AbilityIntelligenceThreshold = effectData.AbilityIntelligenceThreshold ?? string.Empty,
                    AbilityWisdomThreshold = effectData.AbilityWisdomThreshold ?? string.Empty,
                    AbilityCharismaThreshold = effectData.AbilityCharismaThreshold ?? string.Empty,
                }),
                EventTitle = eventData.EventTitle ?? string.Empty,
                TriggerDescription = eventData.TriggerDescription ?? string.Empty,
                DmNote = eventData.DmNote ?? string.Empty,
            };
        }

        private static ChapterGridEventData ToRuntimeData(ChapterGridEventSaveData eventData)
        {
            eventData = ChapterEventDataStructureUtility.NormalizeSaveEventData(eventData);
            if (eventData == null)
            {
                return null;
            }

            ChapterEventTriggerSaveData triggerData = eventData.Trigger ?? new ChapterEventTriggerSaveData();
            ChapterEventEffectSaveData effectData = eventData.Effect ?? new ChapterEventEffectSaveData();
            ChapterEventAreaTriggerParamSaveData areaTriggerParam = triggerData.Area ?? new ChapterEventAreaTriggerParamSaveData();
            ChapterEventInteractionTriggerParamSaveData interactionTriggerParam = triggerData.Interaction ?? new ChapterEventInteractionTriggerParamSaveData();
            ChapterEventPrerequisiteTriggerParamSaveData prerequisiteTriggerParam = triggerData.Prerequisite ?? new ChapterEventPrerequisiteTriggerParamSaveData();
            ChapterEventCheckEffectParamSaveData checkEffectParam = effectData.Check ?? new ChapterEventCheckEffectParamSaveData();
            ChapterEventNarrativeEffectParamSaveData narrativeEffectParam = effectData.Narrative ?? new ChapterEventNarrativeEffectParamSaveData();
            ChapterEventDialogueEffectParamSaveData dialogueEffectParam = effectData.Dialogue ?? new ChapterEventDialogueEffectParamSaveData();
            ChapterEventCreatureEffectParamSaveData creatureEffectParam = effectData.Creature ?? new ChapterEventCreatureEffectParamSaveData();
            ChapterEventBattleEffectParamSaveData battleEffectParam = effectData.Battle ?? new ChapterEventBattleEffectParamSaveData();
            List<ChapterSkillCheckThresholdData> skillCheckEntries = new List<ChapterSkillCheckThresholdData>();
            if (checkEffectParam.SkillCheckEntries != null && checkEffectParam.SkillCheckEntries.Count > 0)
            {
                for (int index = 0; index < checkEffectParam.SkillCheckEntries.Count; index++)
                {
                    ChapterSkillCheckThresholdSaveData entry = checkEffectParam.SkillCheckEntries[index];
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
            else if (!string.IsNullOrWhiteSpace(checkEffectParam.SkillCheckName)
                || !string.IsNullOrWhiteSpace(checkEffectParam.SkillCheckThreshold))
            {
                skillCheckEntries.Add(new ChapterSkillCheckThresholdData
                {
                    SkillName = checkEffectParam.SkillCheckName ?? string.Empty,
                    Threshold = checkEffectParam.SkillCheckThreshold ?? string.Empty,
                });
            }

            ChapterGridEventData runtimeEventData = new ChapterGridEventData
            {
                EventId = eventData.EventId ?? string.Empty,
                IsEnabled = eventData.IsEnabled,
                IsOneShot = eventData.IsOneShot,
                Trigger = ChapterEventDataStructureUtility.CloneRuntimeTrigger(new ChapterEventTriggerData
                {
                    TriggerMode = triggerData.TriggerMode,
                    TriggerType = triggerData.TriggerType,
                    Area = ChapterEventDataStructureUtility.CloneRuntimeAreaTriggerParam(new ChapterEventAreaTriggerParamData
                    {
                        FirstEnterOnly = areaTriggerParam.FirstEnterOnly,
                        ShareBinding = areaTriggerParam.ShareBinding,
                    }),
                    Interaction = ChapterEventDataStructureUtility.CloneRuntimeInteractionTriggerParam(new ChapterEventInteractionTriggerParamData
                    {
                        Target = interactionTriggerParam.Target ?? string.Empty,
                        RequireConfirm = interactionTriggerParam.RequireConfirm,
                    }),
                    Prerequisite = ChapterEventDataStructureUtility.CloneRuntimePrerequisiteTriggerParam(new ChapterEventPrerequisiteTriggerParamData
                    {
                        EventId = prerequisiteTriggerParam.EventId ?? string.Empty,
                        DelayDescription = prerequisiteTriggerParam.DelayDescription ?? string.Empty,
                    }),
                    AreaFirstEnterOnly = triggerData.AreaFirstEnterOnly,
                    AreaShareBinding = triggerData.AreaShareBinding,
                    InteractionTarget = triggerData.InteractionTarget ?? string.Empty,
                    InteractionRequireConfirm = triggerData.InteractionRequireConfirm,
                    PrerequisiteEventId = triggerData.PrerequisiteEventId ?? string.Empty,
                    DelayDescription = triggerData.DelayDescription ?? string.Empty,
                }),
                Effect = ChapterEventDataStructureUtility.CloneRuntimeEffect(new ChapterEventEffectData
                {
                    EffectType = effectData.EffectType,
                    Check = ChapterEventDataStructureUtility.CloneRuntimeCheckEffectParam(new ChapterEventCheckEffectParamData
                    {
                        TargetMode = checkEffectParam.TargetMode,
                        ResolutionMode = checkEffectParam.ResolutionMode,
                        SuccessResult = checkEffectParam.SuccessResult ?? string.Empty,
                        FailureResult = checkEffectParam.FailureResult ?? string.Empty,
                        SkillCheckEntries = skillCheckEntries,
                        SkillCheckName = checkEffectParam.SkillCheckName ?? string.Empty,
                        SkillCheckThreshold = checkEffectParam.SkillCheckThreshold ?? string.Empty,
                        AbilityStrengthThreshold = checkEffectParam.AbilityStrengthThreshold ?? string.Empty,
                        AbilityDexterityThreshold = checkEffectParam.AbilityDexterityThreshold ?? string.Empty,
                        AbilityConstitutionThreshold = checkEffectParam.AbilityConstitutionThreshold ?? string.Empty,
                        AbilityIntelligenceThreshold = checkEffectParam.AbilityIntelligenceThreshold ?? string.Empty,
                        AbilityWisdomThreshold = checkEffectParam.AbilityWisdomThreshold ?? string.Empty,
                        AbilityCharismaThreshold = checkEffectParam.AbilityCharismaThreshold ?? string.Empty,
                    }),
                    Narrative = ChapterEventDataStructureUtility.CloneRuntimeNarrativeEffectParam(new ChapterEventNarrativeEffectParamData
                    {
                        Text = narrativeEffectParam.Text ?? string.Empty,
                        DmOnly = narrativeEffectParam.DmOnly,
                    }),
                    Dialogue = ChapterEventDataStructureUtility.CloneRuntimeDialogueEffectParam(new ChapterEventDialogueEffectParamData
                    {
                        Target = dialogueEffectParam.Target ?? string.Empty,
                        Summary = dialogueEffectParam.Summary ?? string.Empty,
                        Prompt = dialogueEffectParam.Prompt ?? string.Empty,
                    }),
                    Creature = ChapterEventDataStructureUtility.CloneRuntimeCreatureEffectParam(new ChapterEventCreatureEffectParamData
                    {
                        InstanceId = creatureEffectParam.InstanceId ?? string.Empty,
                        Activate = creatureEffectParam.Activate,
                        PlacementMode = creatureEffectParam.PlacementMode,
                    }),
                    Battle = ChapterEventDataStructureUtility.CloneRuntimeBattleEffectParam(new ChapterEventBattleEffectParamData
                    {
                        Reference = battleEffectParam.Reference ?? string.Empty,
                        IncludeActiveCreatures = battleEffectParam.IncludeActiveCreatures,
                        Description = battleEffectParam.Description ?? string.Empty,
                    }),
                    CheckTargetMode = effectData.CheckTargetMode,
                    CheckResolutionMode = effectData.CheckResolutionMode,
                    SuccessResult = effectData.SuccessResult ?? string.Empty,
                    FailureResult = effectData.FailureResult ?? string.Empty,
                    LegacyDmPrompt = effectData.LegacyDmPrompt ?? string.Empty,
                    NarrativeText = effectData.NarrativeText ?? string.Empty,
                    NarrativeDmOnly = effectData.NarrativeDmOnly,
                    DialogueTarget = effectData.DialogueTarget ?? string.Empty,
                    DialogueSummary = effectData.DialogueSummary ?? string.Empty,
                    DialoguePrompt = effectData.DialoguePrompt ?? string.Empty,
                    CreatureInstanceId = effectData.CreatureInstanceId ?? string.Empty,
                    CreatureActivate = effectData.CreatureActivate,
                    CreaturePlacementMode = effectData.CreaturePlacementMode,
                    BattleReference = effectData.BattleReference ?? string.Empty,
                    BattleIncludeActiveCreatures = effectData.BattleIncludeActiveCreatures,
                    BattleDescription = effectData.BattleDescription ?? string.Empty,
                    SkillCheckEntries = skillCheckEntries,
                    SkillCheckName = effectData.SkillCheckName ?? string.Empty,
                    SkillCheckThreshold = effectData.SkillCheckThreshold ?? string.Empty,
                    AbilityStrengthThreshold = effectData.AbilityStrengthThreshold ?? string.Empty,
                    AbilityDexterityThreshold = effectData.AbilityDexterityThreshold ?? string.Empty,
                    AbilityConstitutionThreshold = effectData.AbilityConstitutionThreshold ?? string.Empty,
                    AbilityIntelligenceThreshold = effectData.AbilityIntelligenceThreshold ?? string.Empty,
                    AbilityWisdomThreshold = effectData.AbilityWisdomThreshold ?? string.Empty,
                    AbilityCharismaThreshold = effectData.AbilityCharismaThreshold ?? string.Empty,
                }),
                EventTitle = eventData.EventTitle ?? string.Empty,
                TriggerDescription = eventData.TriggerDescription ?? string.Empty,
                DmNote = eventData.DmNote ?? string.Empty,
            };

            return ChapterEventDataStructureUtility.NormalizeRuntimeEventData(runtimeEventData);
        }

        private static ChapterEventBindingSaveData ToSaveData(ChapterEventBindingData binding)
        {
            if (binding == null || string.IsNullOrWhiteSpace(binding.EventId) || binding.GridCoordinates == null || binding.GridCoordinates.Count <= 0)
            {
                return null;
            }

            ChapterEventBindingSaveData saveData = new ChapterEventBindingSaveData
            {
                BindingId = binding.BindingId ?? string.Empty,
                EventId = binding.EventId ?? string.Empty,
            };

            for (int index = 0; index < binding.GridCoordinates.Count; index++)
            {
                ChapterGridCoordinate coordinate = binding.GridCoordinates[index];
                saveData.GridCoordinates.Add(new ChapterGridCoordinateSaveData
                {
                    CellX = coordinate.CellX,
                    CellY = coordinate.CellY,
                });
            }

            return saveData;
        }

        private static ChapterEventBindingData ToRuntimeData(ChapterEventBindingSaveData binding)
        {
            if (binding == null || string.IsNullOrWhiteSpace(binding.EventId))
            {
                return null;
            }

            ChapterEventBindingData result = new ChapterEventBindingData
            {
                BindingId = binding.BindingId ?? string.Empty,
                EventId = binding.EventId ?? string.Empty,
            };

            if (binding.GridCoordinates != null)
            {
                for (int index = 0; index < binding.GridCoordinates.Count; index++)
                {
                    ChapterGridCoordinateSaveData coordinate = binding.GridCoordinates[index];
                    if (coordinate == null)
                    {
                        continue;
                    }

                    result.GridCoordinates.Add(new ChapterGridCoordinate(coordinate.CellX, coordinate.CellY));
                }
            }

            return result.GridCoordinates.Count > 0 ? result : null;
        }
    }
}
