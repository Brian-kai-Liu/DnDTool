using System;
using System.Collections.Generic;

namespace GameLogic
{
    internal static class CharacterViewStateBuilder
    {
        public static CharacterListItemViewState BuildListItem(CharacterCardDraftSaveData character)
        {
            character = CharacterCardLocalRepository.Normalize(character);
            CharacterRuntimeSnapshotData snapshot = CharacterRuntimeSnapshotData.Clone(character.RuntimeSnapshot);
            snapshot.CharacterId = character.CharacterId ?? string.Empty;
            snapshot.CharacterName = FormatTextOrDefault(character.CharacterName, "未命名角色");
            snapshot.Level = Math.Max(1, character.Level);
            snapshot.RaceId = character.RaceId ?? string.Empty;
            snapshot.ClassId = character.ClassId ?? string.Empty;
            snapshot.BackgroundId = character.BackgroundId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(snapshot.ClassName))
            {
                snapshot.ClassName = GetClassDisplayName(character.ClassId);
            }

            if (string.IsNullOrWhiteSpace(snapshot.RaceName))
            {
                snapshot.RaceName = GetRaceDisplayName(character.RaceId);
            }

            return new CharacterListItemViewState
            {
                CharacterId = character.CharacterId ?? string.Empty,
                CharacterName = character.CharacterName ?? string.Empty,
                RaceId = character.RaceId ?? string.Empty,
                ClassId = character.ClassId ?? string.Empty,
                BackgroundId = character.BackgroundId ?? string.Empty,
                AlignmentId = character.Alignment ?? string.Empty,
                Level = Math.Max(1, character.Level),
                IsCompleted = character.IsCompleted,
                ClassLine = FormatTextOrDefault(snapshot.ClassName, "未选择职业"),
                StatusLine = FormatTextOrDefault(snapshot.RaceName, "未选择种族"),
                RuntimeSnapshot = snapshot
            };
        }

        public static CharacterDetailViewState BuildDetail(CharacterCardDraftSaveData character)
        {
            character = CharacterCardLocalRepository.Normalize(character);
            CharacterDetailViewState state = new CharacterDetailViewState
            {
                Character = character,
                RuntimeSnapshot = CharacterDetailCalculationService.Instance.BuildDisplaySnapshot(character)
            };

            AppendClassProgresses(state, character);
            AppendChoiceSelections(state, character);
            return state;
        }

        private static void AppendClassProgresses(CharacterDetailViewState state, CharacterCardDraftSaveData character)
        {
            if (state == null || character?.ClassProgresses == null)
            {
                return;
            }

            for (int index = 0; index < character.ClassProgresses.Count; index++)
            {
                CharacterClassProgressSaveData progress = character.ClassProgresses[index];
                if (progress == null || string.IsNullOrWhiteSpace(progress.ClassId))
                {
                    continue;
                }

                state.ClassProgresses.Add(new CharacterClassProgressSaveData
                {
                    ClassId = progress.ClassId.Trim(),
                    SubclassId = progress.SubclassId?.Trim() ?? string.Empty,
                    Level = Math.Max(1, progress.Level)
                });
            }
        }

        private static void AppendChoiceSelections(CharacterDetailViewState state, CharacterCardDraftSaveData character)
        {
            if (state == null || character?.ChoiceSelections == null)
            {
                return;
            }

            for (int index = 0; index < character.ChoiceSelections.Count; index++)
            {
                CharacterChoiceSelectionSaveData selection = character.ChoiceSelections[index];
                if (selection == null || string.IsNullOrWhiteSpace(selection.ChoiceGroupId) || string.IsNullOrWhiteSpace(selection.OptionId))
                {
                    continue;
                }

                state.ChoiceSelections.Add(new CharacterChoiceSelectionSaveData
                {
                    ChoiceGroupId = selection.ChoiceGroupId.Trim(),
                    OptionId = selection.OptionId.Trim(),
                    SourceType = selection.SourceType?.Trim() ?? string.Empty,
                    SourceId = selection.SourceId?.Trim() ?? string.Empty,
                    ClassId = selection.ClassId?.Trim() ?? string.Empty,
                    Level = Math.Max(0, selection.Level)
                });
            }
        }

        private static string FormatTextOrDefault(string value, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        }

        private static string GetClassDisplayName(string classId)
        {
            if (string.IsNullOrWhiteSpace(classId))
            {
                return string.Empty;
            }

            return DndRuleContentService.Instance.TryGetClass(classId.Trim(), out DndClassDefineData classData)
                ? FormatTextOrDefault(classData.Name, classData.ClassId)
                : classId.Trim();
        }

        private static string GetRaceDisplayName(string raceId)
        {
            if (string.IsNullOrWhiteSpace(raceId))
            {
                return string.Empty;
            }

            IReadOnlyList<DndRaceDefineData> races = DndRuleContentService.Instance.Races;
            for (int index = 0; index < races.Count; index++)
            {
                DndRaceDefineData raceData = races[index];
                if (raceData != null && string.Equals(raceData.RaceId, raceId.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return FormatTextOrDefault(raceData.Name, raceData.RaceId);
                }
            }

            return raceId.Trim();
        }
    }
}
