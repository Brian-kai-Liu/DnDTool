using System;
using System.Collections.Generic;

namespace GameLogic
{
    internal sealed class CharacterApplicationService
    {
        private static readonly Lazy<CharacterApplicationService> s_instance =
            new Lazy<CharacterApplicationService>(() => new CharacterApplicationService());

        private CharacterApplicationService()
        {
        }

        public static CharacterApplicationService Instance => s_instance.Value;

        public CharacterDraftState CreateDraft()
        {
            CharacterCardDraftSaveData character = CharacterCardLocalRepository.Normalize(new CharacterCardDraftSaveData());
            return new CharacterDraftState
            {
                Mode = CharacterWorkflowMode.Create,
                Character = character,
                IsDirty = false
            };
        }

        public CharacterDraftState CreateEditDraft(CharacterCardDraftSaveData source)
        {
            CharacterCardDraftSaveData character = CharacterCardLocalRepository.Normalize(source);
            return new CharacterDraftState
            {
                Mode = CharacterWorkflowMode.Edit,
                Character = character,
                IsDirty = false
            };
        }

        public CharacterCardDraftSaveData BuildSaveData(CharacterDraftSaveRequest request)
        {
            request ??= new CharacterDraftSaveRequest();

            CharacterCardDraftSaveData character = new CharacterCardDraftSaveData
            {
                CharacterName = request.CharacterName?.Trim() ?? string.Empty,
                RaceId = request.RaceId?.Trim() ?? string.Empty,
                ClassId = request.ClassId?.Trim() ?? string.Empty,
                BackgroundId = request.BackgroundId?.Trim() ?? string.Empty,
                Alignment = request.AlignmentId?.Trim() ?? string.Empty,
                PreviewImagePath = request.PreviewImagePath?.Trim() ?? string.Empty,
                Level = Math.Max(1, request.Level),
                RuntimeSnapshot = CharacterRuntimeSnapshotData.Clone(request.RuntimeSnapshot),
                Equipment = CharacterEquipmentSetSaveData.Clone(request.Equipment),
                RoleplayProfile = CharacterRoleplayProfileSaveData.Clone(request.RoleplayProfile),
                ClassProgresses = CloneClassProgresses(request.ClassProgresses),
                ChoiceSelections = CloneChoiceSelections(request.ChoiceSelections),
                CustomFeatures = CharacterCustomFeatureSaveData.CloneList(request.CustomFeatures),
                DiceRollHistory = CharacterDiceRollHistorySaveData.CloneList(request.DiceRollHistory)
            };
            character.HpModeId = CharacterHpModeIds.Normalize(character.RuntimeSnapshot.HpModeId);
            character.MaxHp = Math.Max(0, character.RuntimeSnapshot.MaxHp);
            character.CurrentHp = CharacterCreationCalculationService.Instance.NormalizeCurrentHp(character.RuntimeSnapshot.CurrentHp, character.MaxHp);
            character.TemporaryHp = Math.Max(0, character.RuntimeSnapshot.TemporaryHp);

            return CharacterCardLocalRepository.Normalize(character);
        }

        public CharacterCardDraftSaveData BuildSaveData(CharacterCreationDraftInput input)
        {
            input ??= new CharacterCreationDraftInput();
            int level = Math.Max(1, input.Level);
            CharacterDraftSaveRequest request = new CharacterDraftSaveRequest
            {
                CharacterName = input.CharacterName,
                RaceId = input.RaceId,
                ClassId = input.ClassId,
                BackgroundId = input.BackgroundId,
                AlignmentId = input.AlignmentId,
                PreviewImagePath = input.PreviewImagePath,
                Level = level,
                RuntimeSnapshot = BuildCreationRuntimeSnapshot(input),
                Equipment = CharacterEquipmentSetSaveData.Clone(input.Equipment),
                RoleplayProfile = BuildCreationRoleplayProfile(input),
                ClassProgresses = BuildCreationClassProgresses(input, level),
                ChoiceSelections = BuildCreationChoiceSelections(input),
                CustomFeatures = CharacterCustomFeatureSaveData.CloneList(input.CustomFeatures),
                DiceRollHistory = CharacterDiceRollHistorySaveData.CloneList(input.DiceRollHistory)
            };

            CharacterCardDraftSaveData character = BuildSaveData(request);
            character.HpModeId = CharacterHpModeIds.Normalize(input.HpModeId);
            character.MaxHp = Math.Max(0, input.MaxHp);
            character.CurrentHp = CharacterCreationCalculationService.Instance.NormalizeCurrentHp(input.CurrentHp, character.MaxHp);
            character.TemporaryHp = Math.Max(0, input.TemporaryHp);
            character.PreviewImagePath = input.PreviewImagePath?.Trim() ?? string.Empty;
            character.RoleplayProfile = BuildCreationRoleplayProfile(input);
            character.HpRolls = CloneHpRolls(input.HpRolls);
            character.Equipment = CharacterEquipmentSetSaveData.Clone(input.Equipment);
            character.Spellcasting = CharacterSpellcastingSaveData.Clone(input.Spellcasting);
            character.CustomFeatures = CharacterCustomFeatureSaveData.CloneList(input.CustomFeatures);
            character.DiceRollHistory = CharacterDiceRollHistorySaveData.CloneList(input.DiceRollHistory);
            return CharacterCardLocalRepository.Normalize(character);
        }

        public CharacterOperationResult Save(CharacterCardDraftSaveData character)
        {
            if (character == null)
            {
                return CharacterOperationResult.Fail("Character data is empty.");
            }

            CharacterCardLocalRepository.Upsert(character);
            return CharacterOperationResult.Ok();
        }

        public CharacterOperationResult Save(CharacterDraftSaveRequest request)
        {
            return Save(BuildSaveData(request));
        }

        public CharacterOperationResult Save(CharacterCreationDraftInput input)
        {
            return Save(BuildSaveData(input));
        }

        public CharacterOperationResult SaveCreationDraft(CharacterCreationDraftInput input)
        {
            CharacterCreationSessionService.Instance.ApplyCreationInput(input);
            CharacterOperationResult result = Save(BuildSaveData(input));
            if (result.Success)
            {
                CharacterCreationSessionService.Instance.MarkClean();
            }

            return result;
        }

        public void ReloadRuleContent()
        {
            DndRuleContentService.Instance.Reload();
        }

        public CharacterLibraryViewState LoadLibrary()
        {
            CharacterCardLibrarySaveData library = CharacterCardLocalRepository.Load();
            CharacterLibraryViewState state = new CharacterLibraryViewState();
            if (library?.Characters == null)
            {
                return state;
            }

            for (int index = 0; index < library.Characters.Count; index++)
            {
                CharacterCardDraftSaveData character = CharacterCardLocalRepository.Normalize(library.Characters[index]);
                state.Characters.Add(character);
                state.Items.Add(CharacterViewStateBuilder.BuildListItem(character));
            }

            return state;
        }

        public bool TryGetCharacter(string characterId, out CharacterCardDraftSaveData character)
        {
            character = null;
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return false;
            }

            CharacterLibraryViewState library = LoadLibrary();
            for (int index = 0; index < library.Characters.Count; index++)
            {
                CharacterCardDraftSaveData candidate = library.Characters[index];
                if (candidate != null && string.Equals(candidate.CharacterId, characterId, StringComparison.OrdinalIgnoreCase))
                {
                    character = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetCharacterDetail(string characterId, out CharacterDetailViewState detail)
        {
            detail = null;
            if (!TryGetCharacter(characterId, out CharacterCardDraftSaveData character))
            {
                return false;
            }

            detail = CharacterViewStateBuilder.BuildDetail(character);
            return true;
        }

        public CharacterOperationResult Delete(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return CharacterOperationResult.Fail("Character id is empty.");
            }

            CharacterCardLocalRepository.Delete(characterId);
            return CharacterOperationResult.Ok();
        }

        private static List<CharacterClassProgressSaveData> CloneClassProgresses(List<CharacterClassProgressSaveData> source)
        {
            List<CharacterClassProgressSaveData> result = new List<CharacterClassProgressSaveData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterClassProgressSaveData progress = source[index];
                if (progress == null || string.IsNullOrWhiteSpace(progress.ClassId))
                {
                    continue;
                }

                result.Add(new CharacterClassProgressSaveData
                {
                    ClassId = progress.ClassId.Trim(),
                    SubclassId = progress.SubclassId?.Trim() ?? string.Empty,
                    Level = Math.Max(1, progress.Level)
                });
            }

            return result;
        }

        private static CharacterRuntimeSnapshotData BuildCreationRuntimeSnapshot(CharacterCreationDraftInput input)
        {
            return new CharacterRuntimeSnapshotData
            {
                Level = Math.Max(1, input.Level),
                Strength = input.Strength,
                Dexterity = input.Dexterity,
                Constitution = input.Constitution,
                Intelligence = input.Intelligence,
                Wisdom = input.Wisdom,
                Charisma = input.Charisma,
                Speed = Math.Max(0, input.Speed),
                HpModeId = CharacterHpModeIds.Normalize(input.HpModeId),
                MaxHp = Math.Max(0, input.MaxHp),
                CurrentHp = CharacterCreationCalculationService.Instance.NormalizeCurrentHp(input.CurrentHp, input.MaxHp),
                TemporaryHp = Math.Max(0, input.TemporaryHp),
                SkillProficiencyIds = CloneStringList(input.SkillProficiencyIds),
                ToolProficiencyIds = CloneStringList(input.ToolProficiencyIds)
            };
        }

        private static List<CharacterClassProgressSaveData> BuildCreationClassProgresses(CharacterCreationDraftInput input, int level)
        {
            List<CharacterClassProgressSaveData> result = new List<CharacterClassProgressSaveData>();
            if (input == null || string.IsNullOrWhiteSpace(input.ClassId))
            {
                return result;
            }

            result.Add(new CharacterClassProgressSaveData
            {
                ClassId = input.ClassId.Trim(),
                SubclassId = input.SubclassId?.Trim() ?? string.Empty,
                Level = Math.Max(1, level)
            });
            return result;
        }

        private static List<CharacterChoiceSelectionSaveData> BuildCreationChoiceSelections(CharacterCreationDraftInput input)
        {
            List<CharacterChoiceSelectionSaveData> result = CloneChoiceSelections(input?.ChoiceSelections);
            AppendRaceAbilityChoices(result, input?.RaceAbilityChoices);
            AppendSkillChoices(result, input?.SkillChoices);
            AppendToolChoices(result, input?.ToolChoices);
            AppendFeatureChoices(result, input?.FeatureChoices);
            return result;
        }

        private static CharacterRoleplayProfileSaveData BuildCreationRoleplayProfile(CharacterCreationDraftInput input)
        {
            input ??= new CharacterCreationDraftInput();
            return new CharacterRoleplayProfileSaveData
            {
                PersonalityTraits = input.PersonalityTraits?.Trim() ?? string.Empty,
                Ideals = input.Ideals?.Trim() ?? string.Empty,
                Bonds = input.Bonds?.Trim() ?? string.Empty,
                Flaws = input.Flaws?.Trim() ?? string.Empty
            };
        }

        private static void AppendRaceAbilityChoices(List<CharacterChoiceSelectionSaveData> target, List<CharacterCreationRaceAbilityChoiceInput> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterCreationRaceAbilityChoiceInput choice = source[index];
                if (choice == null || string.IsNullOrWhiteSpace(choice.ChoiceGroupId) || string.IsNullOrWhiteSpace(choice.OptionId))
                {
                    continue;
                }

                int count = Math.Max(1, choice.Count);
                for (int repeat = 0; repeat < count; repeat++)
                {
                    target.Add(new CharacterChoiceSelectionSaveData
                    {
                        ChoiceGroupId = choice.ChoiceGroupId.Trim(),
                        OptionId = choice.OptionId.Trim(),
                        SourceType = "Race",
                        SourceId = choice.SourceId?.Trim() ?? string.Empty,
                        Level = 1
                    });
                }
            }
        }

        private static void AppendSkillChoices(List<CharacterChoiceSelectionSaveData> target, List<CharacterCreationSkillChoiceInput> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterCreationSkillChoiceInput choice = source[index];
                if (choice == null || string.IsNullOrWhiteSpace(choice.ChoiceGroupId) || string.IsNullOrWhiteSpace(choice.SkillId))
                {
                    continue;
                }

                target.Add(new CharacterChoiceSelectionSaveData
                {
                    ChoiceGroupId = choice.ChoiceGroupId.Trim(),
                    OptionId = choice.SkillId.Trim(),
                    SourceType = choice.SourceType?.Trim() ?? string.Empty,
                    SourceId = choice.SourceId?.Trim() ?? string.Empty,
                    Level = Math.Max(1, choice.Level)
                });
            }
        }

        private static void AppendToolChoices(List<CharacterChoiceSelectionSaveData> target, List<CharacterCreationToolChoiceInput> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterCreationToolChoiceInput choice = source[index];
                if (choice == null || string.IsNullOrWhiteSpace(choice.ChoiceGroupId) || string.IsNullOrWhiteSpace(choice.OptionId))
                {
                    continue;
                }

                target.Add(new CharacterChoiceSelectionSaveData
                {
                    ChoiceGroupId = choice.ChoiceGroupId.Trim(),
                    OptionId = choice.OptionId.Trim(),
                    SourceType = choice.SourceType?.Trim() ?? string.Empty,
                    SourceId = choice.SourceId?.Trim() ?? string.Empty,
                    ClassId = choice.ClassId?.Trim() ?? string.Empty,
                    Level = Math.Max(1, choice.Level)
                });
            }
        }

        private static void AppendFeatureChoices(List<CharacterChoiceSelectionSaveData> target, List<CharacterCreationFeatureChoiceInput> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterCreationFeatureChoiceInput choice = source[index];
                if (choice == null || string.IsNullOrWhiteSpace(choice.ChoiceGroupId) || string.IsNullOrWhiteSpace(choice.OptionId))
                {
                    continue;
                }

                target.Add(new CharacterChoiceSelectionSaveData
                {
                    ChoiceGroupId = choice.ChoiceGroupId.Trim(),
                    OptionId = choice.OptionId.Trim(),
                    SourceType = choice.SourceType?.Trim() ?? string.Empty,
                    SourceId = choice.SourceId?.Trim() ?? string.Empty,
                    ClassId = choice.ClassId?.Trim() ?? string.Empty,
                    Level = Math.Max(1, choice.Level)
                });
            }
        }

        private static List<CharacterChoiceSelectionSaveData> CloneChoiceSelections(List<CharacterChoiceSelectionSaveData> source)
        {
            List<CharacterChoiceSelectionSaveData> result = new List<CharacterChoiceSelectionSaveData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterChoiceSelectionSaveData selection = source[index];
                if (selection == null || string.IsNullOrWhiteSpace(selection.ChoiceGroupId) || string.IsNullOrWhiteSpace(selection.OptionId))
                {
                    continue;
                }

                result.Add(new CharacterChoiceSelectionSaveData
                {
                    ChoiceGroupId = selection.ChoiceGroupId.Trim(),
                    OptionId = selection.OptionId.Trim(),
                    SourceType = selection.SourceType?.Trim() ?? string.Empty,
                    SourceId = selection.SourceId?.Trim() ?? string.Empty,
                    ClassId = selection.ClassId?.Trim() ?? string.Empty,
                    Level = Math.Max(0, selection.Level)
                });
            }

            return result;
        }

        private static List<string> CloneStringList(List<string> source)
        {
            List<string> result = new List<string>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                string value = source[index];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    result.Add(value.Trim());
                }
            }

            return result;
        }

        private static List<CharacterHpRollSaveData> CloneHpRolls(List<CharacterHpRollSaveData> source)
        {
            List<CharacterHpRollSaveData> result = new List<CharacterHpRollSaveData>();
            if (source == null)
            {
                return result;
            }

            for (int index = 0; index < source.Count; index++)
            {
                CharacterHpRollSaveData roll = source[index];
                if (roll == null)
                {
                    continue;
                }

                result.Add(new CharacterHpRollSaveData
                {
                    Level = Math.Max(1, roll.Level),
                    ClassId = roll.ClassId?.Trim() ?? string.Empty,
                    HitDie = Math.Max(0, roll.HitDie),
                    RollValue = Math.Max(0, roll.RollValue),
                    ConstitutionModifier = roll.ConstitutionModifier,
                    HpGain = Math.Max(0, roll.HpGain)
                });
            }

            return result;
        }
    }
}
