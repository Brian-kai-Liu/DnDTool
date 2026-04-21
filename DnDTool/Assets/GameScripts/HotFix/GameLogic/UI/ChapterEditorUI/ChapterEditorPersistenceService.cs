using System.Collections.Generic;
using System.IO;
using System;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    internal static class ChapterEditorPersistenceService
    {
        public static string GetSaveFilePath(string fileName)
        {
            return Path.Combine(UnityEngine.Application.persistentDataPath, "ChapterEditor", fileName);
        }

        public static string GetCreaturePreviewDirectoryPath()
        {
            return Path.Combine(UnityEngine.Application.persistentDataPath, "ChapterCreaturePreviews");
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
            if (string.IsNullOrWhiteSpace(sourceFilePath) || !File.Exists(sourceFilePath))
            {
                return string.Empty;
            }

            string sourceFullPath = Path.GetFullPath(sourceFilePath);
            string targetDirectoryPath = GetCreaturePreviewDirectoryPath();
            Directory.CreateDirectory(targetDirectoryPath);

            string targetDirectoryFullPath = Path.GetFullPath(targetDirectoryPath);
            if (sourceFullPath.StartsWith(targetDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFileName(sourceFullPath);
            }

            string extension = Path.GetExtension(sourceFullPath);
            string targetFileName = $"creature_{DateTime.Now:yyyyMMddHHmmssfff}{extension}";
            string targetFilePath = Path.Combine(targetDirectoryPath, targetFileName);
            File.Copy(sourceFullPath, targetFilePath, true);
            return targetFileName;
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
                        EventData = gridCell.MarkType == ChapterGridCellMarkType.Event
                            ? ToSaveData(gridCell.EventData)
                            : null,
                    });
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
                        EventData = ToRuntimeData(gridCell.EventData),
                    });
                }

                ChapterGridCellCollectionUtility.NormalizeExclusiveTerrainMarks(gridCells);
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

            return result;
        }

        private static ChapterCreatureDataSaveData ToSaveData(ChapterCreatureData creature)
        {
            Color c = creature.AccentColor;
            return new ChapterCreatureDataSaveData
            {
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
            return new ChapterCreatureData
            {
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
            };
        }

        private static ChapterGridEventSaveData ToSaveData(ChapterGridEventData eventData)
        {
            if (eventData == null)
            {
                return null;
            }

            List<ChapterSkillCheckThresholdSaveData> skillCheckEntries = new List<ChapterSkillCheckThresholdSaveData>();
            if (eventData.SkillCheckEntries != null)
            {
                for (int index = 0; index < eventData.SkillCheckEntries.Count; index++)
                {
                    ChapterSkillCheckThresholdData entry = eventData.SkillCheckEntries[index];
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
                EventType = MigrateEventCategoryToOldType(eventData.EventCategory, eventData.EventSubType),
                EventCategory = eventData.EventCategory,
                EventSubType = eventData.EventSubType,
                TriggerMode = eventData.TriggerMode,
                CheckTargetMode = eventData.CheckTargetMode,
                CheckResolutionMode = eventData.CheckResolutionMode,
                EventTitle = eventData.EventTitle ?? string.Empty,
                TriggerDescription = eventData.TriggerDescription ?? string.Empty,
                SuccessResult = eventData.SuccessResult ?? string.Empty,
                FailureResult = eventData.FailureResult ?? string.Empty,
                DmNote = eventData.DmNote ?? string.Empty,
                DmPrompt = eventData.DmPrompt ?? string.Empty,
                SkillCheckEntries = skillCheckEntries,
                SkillCheckName = eventData.SkillCheckName ?? string.Empty,
                SkillCheckThreshold = eventData.SkillCheckThreshold ?? string.Empty,
                AbilityStrengthThreshold = eventData.AbilityStrengthThreshold ?? string.Empty,
                AbilityDexterityThreshold = eventData.AbilityDexterityThreshold ?? string.Empty,
                AbilityConstitutionThreshold = eventData.AbilityConstitutionThreshold ?? string.Empty,
                AbilityIntelligenceThreshold = eventData.AbilityIntelligenceThreshold ?? string.Empty,
                AbilityWisdomThreshold = eventData.AbilityWisdomThreshold ?? string.Empty,
                AbilityCharismaThreshold = eventData.AbilityCharismaThreshold ?? string.Empty,
            };
        }

        private static ChapterGridEventData ToRuntimeData(ChapterGridEventSaveData eventData)
        {
            if (eventData == null)
            {
                return null;
            }

            List<ChapterSkillCheckThresholdData> skillCheckEntries = new List<ChapterSkillCheckThresholdData>();
            if (eventData.SkillCheckEntries != null && eventData.SkillCheckEntries.Count > 0)
            {
                for (int index = 0; index < eventData.SkillCheckEntries.Count; index++)
                {
                    ChapterSkillCheckThresholdSaveData entry = eventData.SkillCheckEntries[index];
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
            else if (!string.IsNullOrWhiteSpace(eventData.SkillCheckName)
                || !string.IsNullOrWhiteSpace(eventData.SkillCheckThreshold))
            {
                skillCheckEntries.Add(new ChapterSkillCheckThresholdData
                {
                    SkillName = eventData.SkillCheckName ?? string.Empty,
                    Threshold = eventData.SkillCheckThreshold ?? string.Empty,
                });
            }

            return new ChapterGridEventData
            {
                EventCategory = ResolveEventCategory(eventData.EventCategory, eventData.EventType),
                EventSubType = ResolveEventSubType(eventData.EventCategory, eventData.EventSubType, eventData.EventType),
                TriggerMode = eventData.TriggerMode,
                CheckTargetMode = eventData.CheckTargetMode,
                CheckResolutionMode = eventData.CheckResolutionMode,
                EventTitle = eventData.EventTitle ?? string.Empty,
                TriggerDescription = eventData.TriggerDescription ?? string.Empty,
                SuccessResult = eventData.SuccessResult ?? string.Empty,
                FailureResult = eventData.FailureResult ?? string.Empty,
                DmNote = eventData.DmNote ?? string.Empty,
                DmPrompt = eventData.DmPrompt ?? string.Empty,
                SkillCheckEntries = skillCheckEntries,
                SkillCheckName = eventData.SkillCheckName ?? string.Empty,
                SkillCheckThreshold = eventData.SkillCheckThreshold ?? string.Empty,
                AbilityStrengthThreshold = eventData.AbilityStrengthThreshold ?? string.Empty,
                AbilityDexterityThreshold = eventData.AbilityDexterityThreshold ?? string.Empty,
                AbilityConstitutionThreshold = eventData.AbilityConstitutionThreshold ?? string.Empty,
                AbilityIntelligenceThreshold = eventData.AbilityIntelligenceThreshold ?? string.Empty,
                AbilityWisdomThreshold = eventData.AbilityWisdomThreshold ?? string.Empty,
                AbilityCharismaThreshold = eventData.AbilityCharismaThreshold ?? string.Empty,
            };
        }
        private static int ResolveEventCategory(int savedCategory, int oldEventType)
        {
            if (savedCategory == 1)
            {
                return 1;
            }

            // Migrate from old format: EventType != 2 (Check) means it was a DM-direct event
            if (savedCategory == 0 && oldEventType != 2)
            {
                return 1;
            }

            return 0;
        }

        private static int ResolveEventSubType(int savedCategory, int savedSubType, int oldEventType)
        {
            if (savedCategory == 1)
            {
                return savedSubType;
            }

            // Migrate from old format: map old EventType to new DM subtype index
            // Old: Story=0, Dialogue=1, [Check=2 skipped], Choice=3, Interaction=4,
            //      Combat=5, Exploration=6, AreaEnter=7, TimeAdvance=8, Random=9, Special=10
            // New DM subtypes: Story=0, Dialogue=1, Choice=2, Interaction=3,
            //                  Combat=4, Exploration=5, AreaEnter=6, TimeAdvance=7, Random=8, Special=9
            if (savedCategory == 0 && oldEventType != 2 && oldEventType >= 0 && oldEventType <= 10)
            {
                return oldEventType < 2 ? oldEventType : oldEventType - 1;
            }

            return 0;
        }

        private static int MigrateEventCategoryToOldType(int eventCategory, int eventSubType)
        {
            if (eventCategory == 0)
            {
                return 2; // Check
            }

            // DM subtype → old EventType: Story=0, Dialogue=1, Choice=2→3, Interaction=3→4,
            // Combat=4→5, Exploration=5→6, AreaEnter=6→7, TimeAdvance=7→8, Random=8→9, Special=9→10
            return eventSubType < 2 ? eventSubType : eventSubType + 1;
        }
    }
}