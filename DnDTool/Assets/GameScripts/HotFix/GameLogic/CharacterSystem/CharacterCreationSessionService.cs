using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GameLogic
{
    internal sealed class CharacterCreationSessionService
    {
        private const string AbilityGenerationMethodPointBuy = "point_buy";
        private const string AbilityGenerationMethodManual = "manual";
        private const int PointBuyBudget = 27;
        private const int PointBuyMinScore = 8;
        private const int PointBuyMaxScore = 15;
        private const int ManualMinScore = 1;
        private const int ManualMaxScore = 30;

        private static readonly Lazy<CharacterCreationSessionService> s_instance =
            new Lazy<CharacterCreationSessionService>(() => new CharacterCreationSessionService());

        private CharacterDraftState m_state = new CharacterDraftState();
        private CharacterCreationToolChoiceState m_activeToolChoiceState;
        private CharacterCreationFeatureChoiceState m_activeFeatureChoiceState;
        private readonly System.Random m_random = new System.Random();

        private CharacterCreationSessionService()
        {
        }

        public static CharacterCreationSessionService Instance => s_instance.Value;

        public CharacterDraftState CurrentState => m_state;

        public List<CharacterCreationSkillChoiceState> SkillChoiceStates { get; } = new List<CharacterCreationSkillChoiceState>();

        public List<CharacterCreationToolChoiceState> ToolChoiceStates { get; } = new List<CharacterCreationToolChoiceState>();

        public List<CharacterCreationFeatureChoiceState> FeatureChoiceStates { get; } = new List<CharacterCreationFeatureChoiceState>();

        public CharacterCreationToolChoiceState ActiveToolChoiceState => m_activeToolChoiceState;

        public CharacterCreationFeatureChoiceState ActiveFeatureChoiceState => m_activeFeatureChoiceState;

        public CharacterCreationRaceAbilityChoiceState RaceAbilityChoiceState { get; } = new CharacterCreationRaceAbilityChoiceState();

        public CharacterCreationAbilityGenerationState AbilityGenerationState { get; } = new CharacterCreationAbilityGenerationState();

        public void BeginNewDraft()
        {
            m_state = new CharacterDraftState
            {
                Mode = CharacterWorkflowMode.Create,
                Character = CharacterCardLocalRepository.Normalize(new CharacterCardDraftSaveData()),
                IsDirty = false
            };
            ClearChoiceState();
        }

        public void SetSelectedClass(string classId)
        {
            EnsureState();
            m_state.Character.ClassId = classId?.Trim() ?? string.Empty;
            RefreshDerivedStatsAfterAbilityScoresChanged();
            m_state.IsDirty = true;
        }

        public string ToggleSelectedClass(string classId)
        {
            EnsureState();
            string normalized = classId?.Trim() ?? string.Empty;
            SetSelectedClass(string.Equals(m_state.Character.ClassId, normalized, StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : normalized);
            return m_state.Character.ClassId;
        }

        public void SetSelectedRace(string raceId)
        {
            EnsureState();
            m_state.Character.RaceId = raceId?.Trim() ?? string.Empty;
            m_state.IsDirty = true;
        }

        public string ToggleSelectedRace(string raceId)
        {
            EnsureState();
            string normalized = raceId?.Trim() ?? string.Empty;
            SetSelectedRace(string.Equals(m_state.Character.RaceId, normalized, StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : normalized);
            return m_state.Character.RaceId;
        }

        public void SetSelectedBackground(string backgroundId)
        {
            EnsureState();
            m_state.Character.BackgroundId = backgroundId?.Trim() ?? string.Empty;
            m_state.IsDirty = true;
        }

        public string ToggleSelectedBackground(string backgroundId)
        {
            EnsureState();
            string normalized = backgroundId?.Trim() ?? string.Empty;
            SetSelectedBackground(string.Equals(m_state.Character.BackgroundId, normalized, StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : normalized);
            return m_state.Character.BackgroundId;
        }

        public void SetSelectedAlignment(string alignmentId)
        {
            EnsureState();
            m_state.Character.Alignment = alignmentId?.Trim() ?? string.Empty;
            m_state.IsDirty = true;
        }

        public string ToggleSelectedAlignment(string alignmentId)
        {
            EnsureState();
            string normalized = alignmentId?.Trim() ?? string.Empty;
            SetSelectedAlignment(string.Equals(m_state.Character.Alignment, normalized, StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : normalized);
            return m_state.Character.Alignment;
        }

        public void SetLevel(int level)
        {
            EnsureState();
            m_state.Character.Level = Math.Max(1, level);
            RefreshDerivedStatsAfterAbilityScoresChanged();
            m_state.IsDirty = true;
        }

        public void SetCharacterName(string characterName)
        {
            EnsureState();
            m_state.Character.CharacterName = characterName?.Trim() ?? string.Empty;
            m_state.IsDirty = true;
        }

        public void ApplyCreationInput(CharacterCreationDraftInput input)
        {
            EnsureState();
            input ??= new CharacterCreationDraftInput();
            m_state.Character.CharacterName = input.CharacterName?.Trim() ?? string.Empty;
            m_state.Character.RaceId = input.RaceId?.Trim() ?? string.Empty;
            m_state.Character.ClassId = input.ClassId?.Trim() ?? string.Empty;
            m_state.Character.BackgroundId = input.BackgroundId?.Trim() ?? string.Empty;
            m_state.Character.Alignment = input.AlignmentId?.Trim() ?? string.Empty;
            m_state.Character.Level = Math.Max(1, input.Level);
            m_state.IsDirty = true;
        }

        public CharacterCreationToolChoiceState FindToolChoiceState(string choiceGroupId)
        {
            return FindChoiceState(ToolChoiceStates, choiceGroupId);
        }

        public CharacterCreationFeatureChoiceState FindFeatureChoiceState(string choiceGroupId)
        {
            return FindChoiceState(FeatureChoiceStates, choiceGroupId);
        }

        public CharacterCreationFeatureChoiceState GetActiveFeatureChoiceState()
        {
            return m_activeFeatureChoiceState;
        }

        public void SetActiveToolChoice(CharacterCreationToolChoiceState state)
        {
            m_activeFeatureChoiceState = null;
            m_activeToolChoiceState = state;
        }

        public void SetActiveFeatureChoice(CharacterCreationFeatureChoiceState state)
        {
            m_activeToolChoiceState = null;
            m_activeFeatureChoiceState = state;
        }

        public void ClearActiveChoice()
        {
            m_activeToolChoiceState = null;
            m_activeFeatureChoiceState = null;
        }

        public List<CharacterCreationAbilityGenerationMethodViewState> GetAbilityGenerationMethods()
        {
            return new List<CharacterCreationAbilityGenerationMethodViewState>
            {
                new CharacterCreationAbilityGenerationMethodViewState { MethodId = "standard_array", Name = "标准数组" },
                new CharacterCreationAbilityGenerationMethodViewState { MethodId = "roll_4d6_drop_lowest", Name = "掷骰" },
                new CharacterCreationAbilityGenerationMethodViewState { MethodId = "point_buy", Name = "点数购买" },
                new CharacterCreationAbilityGenerationMethodViewState { MethodId = "manual", Name = "自定义" }
            };
        }

        public List<CharacterCreationHitPointGenerationMethodViewState> GetHitPointGenerationMethods()
        {
            return new List<CharacterCreationHitPointGenerationMethodViewState>
            {
                new CharacterCreationHitPointGenerationMethodViewState { MethodId = CharacterHpModeIds.Rolled, Name = "生命值掷骰" },
                new CharacterCreationHitPointGenerationMethodViewState { MethodId = CharacterHpModeIds.Average, Name = "生命值均值" }
            };
        }

        public bool GenerateHitPoints(string hpModeId, int level)
        {
            EnsureState();
            CharacterCardDraftSaveData character = m_state.Character;
            string normalizedMode = CharacterHpModeIds.Normalize(hpModeId);
            if (string.Equals(normalizedMode, CharacterHpModeIds.Custom, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            character.HpModeId = normalizedMode;
            character.Level = Math.Max(1, level);
            character.ManualHp = 0;
            return SyncHitPointsForCurrentLevel();
        }

        public bool StartAbilityGeneration(string methodId)
        {
            string normalized = methodId?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return false;
            }

            ClearAbilityGeneration();
            AbilityGenerationState.MethodId = normalized;
            if (string.Equals(normalized, "standard_array", StringComparison.OrdinalIgnoreCase))
            {
                AddGeneratedAbilityScores(new[] { 15, 14, 13, 12, 10, 8 });
            }
            else if (string.Equals(normalized, "roll_4d6_drop_lowest", StringComparison.OrdinalIgnoreCase))
            {
                AddGeneratedAbilityScores(RollBestAbilityScoreSet());
            }
            else if (string.Equals(normalized, AbilityGenerationMethodPointBuy, StringComparison.OrdinalIgnoreCase))
            {
                InitializePointBuyScores();
            }
            else if (string.Equals(normalized, AbilityGenerationMethodManual, StringComparison.OrdinalIgnoreCase))
            {
                InitializeManualScores();
            }
            else
            {
                AbilityGenerationState.MethodId = normalized;
            }

            m_state.IsDirty = true;
            return true;
        }

        public void ClearAbilityGeneration()
        {
            AbilityGenerationState.MethodId = string.Empty;
            AbilityGenerationState.PendingScoreId = string.Empty;
            AbilityGenerationState.Scores.Clear();
            AbilityGenerationState.PointBuyScores.Clear();
            AbilityGenerationState.ManualScores.Clear();
        }

        public List<CharacterCreationGeneratedAbilityScoreViewState> BuildGeneratedAbilityScoreOptions()
        {
            List<CharacterCreationGeneratedAbilityScoreViewState> result = new List<CharacterCreationGeneratedAbilityScoreViewState>();
            for (int index = 0; index < AbilityGenerationState.Scores.Count; index++)
            {
                CharacterCreationGeneratedAbilityScoreState score = AbilityGenerationState.Scores[index];
                if (score == null)
                {
                    continue;
                }

                result.Add(new CharacterCreationGeneratedAbilityScoreViewState
                {
                    ScoreId = score.ScoreId,
                    Label = score.Value.ToString(),
                    IsSelected = string.Equals(score.ScoreId, AbilityGenerationState.PendingScoreId, StringComparison.OrdinalIgnoreCase),
                    IsAssigned = score.IsAssigned
                });
            }

            return result;
        }

        public bool SelectGeneratedAbilityScore(string scoreId)
        {
            CharacterCreationGeneratedAbilityScoreState score = FindGeneratedAbilityScore(scoreId);
            if (score == null)
            {
                return false;
            }

            if (score.IsAssigned)
            {
                score.AssignedAbilityId = string.Empty;
                if (string.Equals(AbilityGenerationState.PendingScoreId, score.ScoreId, StringComparison.OrdinalIgnoreCase))
                {
                    AbilityGenerationState.PendingScoreId = string.Empty;
                }

                m_state.IsDirty = true;
                RefreshDerivedStatsAfterAbilityScoresChanged();
                return true;
            }

            AbilityGenerationState.PendingScoreId = string.Equals(AbilityGenerationState.PendingScoreId, score.ScoreId, StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : score.ScoreId;
            m_state.IsDirty = true;
            return true;
        }

        public bool AssignPendingGeneratedAbilityScore(string abilityId)
        {
            string normalizedAbilityId = NormalizeAbilityId(abilityId);
            CharacterCreationGeneratedAbilityScoreState pendingScore = FindGeneratedAbilityScore(AbilityGenerationState.PendingScoreId);
            if (string.IsNullOrWhiteSpace(normalizedAbilityId) || pendingScore == null || pendingScore.IsAssigned)
            {
                return false;
            }

            for (int index = 0; index < AbilityGenerationState.Scores.Count; index++)
            {
                CharacterCreationGeneratedAbilityScoreState score = AbilityGenerationState.Scores[index];
                if (score != null && string.Equals(score.AssignedAbilityId, normalizedAbilityId, StringComparison.OrdinalIgnoreCase))
                {
                    score.AssignedAbilityId = string.Empty;
                }
            }

            pendingScore.AssignedAbilityId = normalizedAbilityId;
            AbilityGenerationState.PendingScoreId = string.Empty;
            m_state.IsDirty = true;
            RefreshDerivedStatsAfterAbilityScoresChanged();
            return true;
        }

        public bool IsAbilityGenerationAssignmentComplete()
        {
            if (IsPointBuyMode())
            {
                return true;
            }

            if (AbilityGenerationState.Scores.Count == 0)
            {
                return false;
            }

            for (int index = 0; index < AbilityGenerationState.Scores.Count; index++)
            {
                CharacterCreationGeneratedAbilityScoreState score = AbilityGenerationState.Scores[index];
                if (score == null || !score.IsAssigned)
                {
                    return false;
                }
            }

            return true;
        }

        public bool ChangeAbilityScore(string abilityId, int delta)
        {
            if (TryChangeActiveAbilityScoreFeatureChoice(abilityId, delta))
            {
                return true;
            }

            if (IsPointBuyMode())
            {
                return ChangePointBuyAbilityScore(abilityId, delta);
            }

            return ChangeRaceSelectedAbilityBonus(abilityId, delta);
        }

        public bool SetManualAbilityScore(string abilityId, int score)
        {
            if (!IsManualAbilityMode())
            {
                return false;
            }

            string normalized = NormalizeAbilityId(abilityId);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return false;
            }

            AbilityGenerationState.ManualScores[normalized] = Math.Max(ManualMinScore, Math.Min(ManualMaxScore, score));
            m_state.IsDirty = true;
            RefreshDerivedStatsAfterAbilityScoresChanged();
            return true;
        }

        public bool CanManualInputAbilityScore(string abilityId)
        {
            return IsManualAbilityMode() && !string.IsNullOrWhiteSpace(NormalizeAbilityId(abilityId));
        }

        public bool TogglePendingToolChoice(string toolId)
        {
            CharacterCreationToolChoiceState state = m_activeToolChoiceState;
            if (state == null || string.IsNullOrWhiteSpace(toolId))
            {
                return false;
            }

            TogglePendingValue(state.PendingToolIds, toolId, state.MaxSelect);
            m_state.IsDirty = true;
            return true;
        }

        public bool TogglePendingFeatureChoice(string optionId)
        {
            CharacterCreationFeatureChoiceState state = m_activeFeatureChoiceState;
            if (state == null || string.IsNullOrWhiteSpace(optionId))
            {
                return false;
            }

            if (IsRepeatableAbilityScoreChoice(state))
            {
                ToggleRepeatableAbilityScoreChoice(state, optionId);
            }
            else
            {
                TogglePendingValue(state.PendingOptionIds, optionId, state.MaxSelect);
            }

            m_state.IsDirty = true;
            return true;
        }

        public bool ConfirmActiveToolChoice()
        {
            CharacterCreationToolChoiceState state = m_activeToolChoiceState;
            if (state == null)
            {
                return false;
            }

            state.SelectedToolIds.Clear();
            AppendUniqueValues(state.SelectedToolIds, state.PendingToolIds);
            m_state.IsDirty = true;
            return true;
        }

        public bool ConfirmFeatureChoice(CharacterCreationFeatureChoiceState state)
        {
            if (state == null)
            {
                return false;
            }

            if (state.MinSelect > 0 && state.PendingOptionIds.Count < state.MinSelect)
            {
                return false;
            }

            state.SelectedOptionIds.Clear();
            if (IsRepeatableAbilityScoreChoice(state))
            {
                AppendValues(state.SelectedOptionIds, state.PendingOptionIds);
            }
            else
            {
                AppendUniqueValues(state.SelectedOptionIds, state.PendingOptionIds);
            }

            m_state.IsDirty = true;
            RefreshDerivedStatsAfterAbilityScoresChanged();
            return true;
        }

        public CharacterCreationFeatureChoiceState StartFollowupFeatureChoice(CharacterCreationFeatureChoiceState completedState)
        {
            if (completedState == null
                || completedState.SelectedOptionIds.Count == 0
                || !DndRuleContentService.Instance.TryGetChoiceGroup(completedState.ChoiceGroupId, out DndChoiceGroupData parentGroup)
                || parentGroup == null
                || !IsAdvancementOptionChoiceType(parentGroup.ChoiceType))
            {
                return null;
            }

            string followupChoiceGroupId = ResolveAdvancementFollowupChoiceGroupId(parentGroup, completedState.SelectedOptionIds[0]);
            if (string.IsNullOrWhiteSpace(followupChoiceGroupId)
                || !TryGetSelectableFeatureChoiceGroup(followupChoiceGroupId, out DndChoiceGroupData followupGroup))
            {
                return null;
            }

            CharacterCreationFeatureChoiceState followupState = FindFeatureChoiceState(followupChoiceGroupId);
            if (followupState == null)
            {
                followupState = BuildFeatureChoiceState(
                    followupGroup,
                    completedState.SourceType,
                    completedState.SourceId,
                    completedState.Level);
                followupState.ClassId = completedState.ClassId;
                if (followupState.OptionIds.Count > 0)
                {
                    FeatureChoiceStates.Add(followupState);
                }
            }

            SetActiveFeatureChoice(followupState);
            return followupState != null && followupState.OptionIds.Count > 0 ? followupState : null;
        }

        public CharacterCreationToolChoiceState CreateOrRefreshToolChoiceState(
            DndChoiceGroupData choiceGroup,
            string label,
            string sourceType,
            string sourceId,
            Func<DndChoiceOptionData, string> resolveToolId)
        {
            if (choiceGroup == null || string.IsNullOrWhiteSpace(choiceGroup.ChoiceGroupId))
            {
                return null;
            }

            CharacterCreationToolChoiceState state = FindToolChoiceState(choiceGroup.ChoiceGroupId);
            if (state == null)
            {
                state = new CharacterCreationToolChoiceState
                {
                    ChoiceGroupId = choiceGroup.ChoiceGroupId,
                    SourceType = sourceType ?? string.Empty,
                    SourceId = sourceId ?? string.Empty
                };
                ToolChoiceStates.Add(state);
            }

            state.Label = FirstNonEmpty(label, choiceGroup.Name);
            state.SourceType = sourceType ?? string.Empty;
            state.SourceId = sourceId ?? string.Empty;
            state.MaxSelect = Math.Max(choiceGroup.MinSelect, choiceGroup.MaxSelect);
            state.OptionToolIds.Clear();
            state.OptionIdByToolId.Clear();

            IReadOnlyList<DndChoiceOptionData> options = DndRuleContentService.Instance.GetChoiceOptions(choiceGroup.ChoiceGroupId);
            for (int optionIndex = 0; optionIndex < options.Count; optionIndex++)
            {
                DndChoiceOptionData option = options[optionIndex];
                string toolId = resolveToolId != null ? resolveToolId(option) : string.Empty;
                if (!string.IsNullOrWhiteSpace(toolId))
                {
                    AppendUniqueExactValue(state.OptionToolIds, toolId);
                    if (!state.OptionIdByToolId.ContainsKey(toolId))
                    {
                        state.OptionIdByToolId[toolId] = FirstNonEmpty(option?.OptionId, toolId);
                    }
                }
            }

            return ValidateToolChoiceState(state);
        }

        public void RebuildSkillChoiceStates(IReadOnlyList<CharacterCreationSkillChoiceSource> sources, IReadOnlyList<string> fixedSkillIds)
        {
            List<CharacterCreationSkillChoiceState> previousStates = new List<CharacterCreationSkillChoiceState>(SkillChoiceStates);
            SkillChoiceStates.Clear();

            if (sources == null)
            {
                return;
            }

            for (int sourceIndex = 0; sourceIndex < sources.Count; sourceIndex++)
            {
                CharacterCreationSkillChoiceSource source = sources[sourceIndex];
                AppendSkillChoiceStates(previousStates, source.ChoiceGroupIds, source.SourceType, source.SourceId, fixedSkillIds);
            }

            m_state.IsDirty = true;
        }

        public void RebuildSkillChoiceStatesForCurrentSelection(
            DndClassDefineData classData,
            DndRaceDefineData raceData,
            DndBackgroundDefineData backgroundData)
        {
            List<CharacterCreationSkillChoiceSource> sources = new List<CharacterCreationSkillChoiceSource>();

            if (classData != null
                && DndRuleContentService.Instance.TryGetClassLevelProgression(classData.ClassId, 1, out DndLevelProgressionData progression))
            {
                sources.Add(new CharacterCreationSkillChoiceSource
                {
                    SourceType = "Class",
                    SourceId = classData.ClassId,
                    ChoiceGroupIds = new List<string>(progression.ChoiceGroupIds)
                });
            }

            if (raceData != null)
            {
                sources.Add(new CharacterCreationSkillChoiceSource
                {
                    SourceType = "Race",
                    SourceId = raceData.RaceId,
                    ChoiceGroupIds = CharacterCreationRuleService.Instance.BuildRaceChoiceGroupIds(raceData)
                });
            }

            RebuildSkillChoiceStates(sources, CharacterCreationRuleService.Instance.BuildFixedSkillProficiencyIds(raceData, backgroundData));
        }

        public bool TrySelectSkill(string skillId, IReadOnlyList<string> currentProficiencyIds)
        {
            if (string.IsNullOrWhiteSpace(skillId) || ContainsNormalizedSkillId(currentProficiencyIds, skillId))
            {
                return false;
            }

            for (int stateIndex = 0; stateIndex < SkillChoiceStates.Count; stateIndex++)
            {
                CharacterCreationSkillChoiceState state = SkillChoiceStates[stateIndex];
                if (state == null
                    || state.SelectedSkillIds.Count >= state.MaxSelect
                    || !ContainsNormalizedSkillId(state.CandidateSkillIds, skillId)
                    || ContainsNormalizedSkillId(state.SelectedSkillIds, skillId))
                {
                    continue;
                }

                AppendUniqueNormalizedSkillId(state.SelectedSkillIds, skillId);
                m_state.IsDirty = true;
                return true;
            }

            return false;
        }

        public bool IsSkillChoiceCandidate(string skillId, IReadOnlyList<string> currentProficiencyIds)
        {
            if (string.IsNullOrWhiteSpace(skillId) || ContainsNormalizedSkillId(currentProficiencyIds, skillId))
            {
                return false;
            }

            for (int index = 0; index < SkillChoiceStates.Count; index++)
            {
                CharacterCreationSkillChoiceState state = SkillChoiceStates[index];
                if (state != null
                    && state.SelectedSkillIds.Count < state.MaxSelect
                    && ContainsNormalizedSkillId(state.CandidateSkillIds, skillId)
                    && !ContainsNormalizedSkillId(state.SelectedSkillIds, skillId))
                {
                    return true;
                }
            }

            return false;
        }

        public List<string> BuildCurrentSkillProficiencyIds(IReadOnlyList<string> fixedSkillIds)
        {
            List<string> result = new List<string>();
            AppendUniqueNormalizedSkillIds(result, fixedSkillIds);

            for (int index = 0; index < SkillChoiceStates.Count; index++)
            {
                CharacterCreationSkillChoiceState state = SkillChoiceStates[index];
                if (state != null)
                {
                    AppendUniqueNormalizedSkillIds(result, state.SelectedSkillIds);
                }
            }

            return result;
        }

        public List<CharacterCreationSkillChoiceInput> BuildSkillChoiceInputs()
        {
            List<CharacterCreationSkillChoiceInput> result = new List<CharacterCreationSkillChoiceInput>();

            for (int stateIndex = 0; stateIndex < SkillChoiceStates.Count; stateIndex++)
            {
                CharacterCreationSkillChoiceState state = SkillChoiceStates[stateIndex];
                if (state == null || string.IsNullOrWhiteSpace(state.ChoiceGroupId))
                {
                    continue;
                }

                for (int skillIndex = 0; skillIndex < state.SelectedSkillIds.Count; skillIndex++)
                {
                    string skillId = state.SelectedSkillIds[skillIndex];
                    if (string.IsNullOrWhiteSpace(skillId))
                    {
                        continue;
                    }

                    result.Add(new CharacterCreationSkillChoiceInput
                    {
                        ChoiceGroupId = state.ChoiceGroupId,
                        SkillId = skillId.Trim(),
                        SourceType = state.SourceType?.Trim() ?? string.Empty,
                        SourceId = state.SourceId?.Trim() ?? string.Empty,
                        Level = 1
                    });
                }
            }

            return result;
        }

        public List<CharacterCreationToolChoiceInput> BuildToolChoiceInputs(string selectedClassId)
        {
            List<CharacterCreationToolChoiceInput> result = new List<CharacterCreationToolChoiceInput>();

            for (int stateIndex = 0; stateIndex < ToolChoiceStates.Count; stateIndex++)
            {
                CharacterCreationToolChoiceState state = ToolChoiceStates[stateIndex];
                if (state == null || string.IsNullOrWhiteSpace(state.ChoiceGroupId))
                {
                    continue;
                }

                for (int toolIndex = 0; toolIndex < state.SelectedToolIds.Count; toolIndex++)
                {
                    string toolId = state.SelectedToolIds[toolIndex];
                    if (string.IsNullOrWhiteSpace(toolId))
                    {
                        continue;
                    }

                    string normalizedToolId = toolId.Trim();
                    string optionId = state.OptionIdByToolId.TryGetValue(normalizedToolId, out string mappedOptionId)
                        ? mappedOptionId
                        : normalizedToolId;

                    result.Add(new CharacterCreationToolChoiceInput
                    {
                        ChoiceGroupId = state.ChoiceGroupId,
                        OptionId = optionId?.Trim() ?? string.Empty,
                        SourceType = state.SourceType?.Trim() ?? string.Empty,
                        SourceId = state.SourceId?.Trim() ?? string.Empty,
                        ClassId = string.Equals(state.SourceType, "Class", StringComparison.OrdinalIgnoreCase) ? selectedClassId?.Trim() ?? string.Empty : string.Empty,
                        Level = 1
                    });
                }
            }

            return result;
        }

        public List<string> BuildCurrentToolProficiencyIds(IReadOnlyList<string> fixedToolIds)
        {
            List<string> result = new List<string>();
            AppendUniqueExactValues(result, fixedToolIds);

            for (int stateIndex = 0; stateIndex < ToolChoiceStates.Count; stateIndex++)
            {
                CharacterCreationToolChoiceState state = ToolChoiceStates[stateIndex];
                if (state == null)
                {
                    continue;
                }

                for (int toolIndex = 0; toolIndex < state.SelectedToolIds.Count; toolIndex++)
                {
                    AppendUniqueExactValue(result, state.SelectedToolIds[toolIndex]);
                }
            }

            return result;
        }

        public List<CharacterCreationFeatureChoiceInput> BuildFeatureChoiceInputs()
        {
            List<CharacterCreationFeatureChoiceInput> result = new List<CharacterCreationFeatureChoiceInput>();

            for (int stateIndex = 0; stateIndex < FeatureChoiceStates.Count; stateIndex++)
            {
                CharacterCreationFeatureChoiceState state = FeatureChoiceStates[stateIndex];
                if (state == null || string.IsNullOrWhiteSpace(state.ChoiceGroupId))
                {
                    continue;
                }

                for (int optionIndex = 0; optionIndex < state.SelectedOptionIds.Count; optionIndex++)
                {
                    string optionId = state.SelectedOptionIds[optionIndex];
                    if (string.IsNullOrWhiteSpace(optionId))
                    {
                        continue;
                    }

                    result.Add(new CharacterCreationFeatureChoiceInput
                    {
                        ChoiceGroupId = state.ChoiceGroupId,
                        OptionId = optionId.Trim(),
                        SourceType = state.SourceType?.Trim() ?? string.Empty,
                        SourceId = state.SourceId?.Trim() ?? string.Empty,
                        ClassId = state.ClassId?.Trim() ?? string.Empty,
                        Level = state.Level > 0 ? state.Level : 1
                    });
                }
            }

            return result;
        }

        public CharacterCreationDraftInput BuildDraftInput(CharacterCreationFormInput form)
        {
            form ??= new CharacterCreationFormInput();
            int baseAbilityScore = form.BaseAbilityScore > 0 ? form.BaseAbilityScore : 10;
            string classId = form.ClassId?.Trim() ?? string.Empty;
            CharacterCardDraftSaveData character = m_state?.Character ?? new CharacterCardDraftSaveData();

            return new CharacterCreationDraftInput
            {
                CharacterName = form.CharacterName?.Trim() ?? string.Empty,
                RaceId = form.RaceId?.Trim() ?? string.Empty,
                ClassId = classId,
                SubclassId = GetSelectedSubclassId(classId),
                BackgroundId = form.BackgroundId?.Trim() ?? string.Empty,
                AlignmentId = form.AlignmentId?.Trim() ?? string.Empty,
                Level = Math.Max(1, form.Level),
                Speed = Math.Max(0, form.Speed),
                Strength = GetCurrentAbilityScore("Strength", baseAbilityScore),
                Dexterity = GetCurrentAbilityScore("Dexterity", baseAbilityScore),
                Constitution = GetCurrentAbilityScore("Constitution", baseAbilityScore),
                Intelligence = GetCurrentAbilityScore("Intelligence", baseAbilityScore),
                Wisdom = GetCurrentAbilityScore("Wisdom", baseAbilityScore),
                Charisma = GetCurrentAbilityScore("Charisma", baseAbilityScore),
                HpModeId = CharacterHpModeIds.Normalize(character.HpModeId),
                MaxHp = Math.Max(0, character.MaxHp),
                CurrentHp = CharacterCreationCalculationService.Instance.NormalizeCurrentHp(character.CurrentHp, character.MaxHp),
                TemporaryHp = Math.Max(0, character.TemporaryHp),
                HpRolls = CloneHpRolls(character.HpRolls),
                SkillProficiencyIds = BuildCurrentSkillProficiencyIds(form.FixedSkillProficiencyIds),
                ToolProficiencyIds = BuildCurrentToolProficiencyIds(form.FixedToolProficiencyIds),
                RaceAbilityChoices = BuildRaceAbilityChoiceInputs(),
                SkillChoices = BuildSkillChoiceInputs(),
                ToolChoices = BuildToolChoiceInputs(classId),
                FeatureChoices = BuildFeatureChoiceInputs()
            };
        }

        public string GetSelectedSubclassId(string classId)
        {
            if (string.IsNullOrWhiteSpace(classId))
            {
                return string.Empty;
            }

            for (int index = 0; index < FeatureChoiceStates.Count; index++)
            {
                CharacterCreationFeatureChoiceState state = FeatureChoiceStates[index];
                if (state == null
                    || !string.Equals(state.SourceType, "Class", StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(state.ClassId, classId, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(state.ChoiceType, "Subclass", StringComparison.OrdinalIgnoreCase)
                    || state.SelectedOptionIds.Count == 0)
                {
                    continue;
                }

                return state.SelectedOptionIds[0]?.Trim() ?? string.Empty;
            }

            return string.Empty;
        }

        public void RebuildFeatureChoiceStates(IReadOnlyList<CharacterCreationFeatureChoiceSource> sources)
        {
            List<CharacterCreationFeatureChoiceState> previousStates = new List<CharacterCreationFeatureChoiceState>(FeatureChoiceStates);
            FeatureChoiceStates.Clear();

            if (sources == null)
            {
                return;
            }

            for (int sourceIndex = 0; sourceIndex < sources.Count; sourceIndex++)
            {
                CharacterCreationFeatureChoiceSource source = sources[sourceIndex];
                AppendFeatureChoiceStates(previousStates, source.ChoiceGroupIds, source.SourceType, source.SourceId, source.Level);
            }

            m_state.IsDirty = true;
        }

        public void RebuildFeatureChoiceStatesForCurrentSelection(DndClassDefineData classData, DndRaceDefineData raceData, int level)
        {
            List<CharacterCreationFeatureChoiceSource> sources = new List<CharacterCreationFeatureChoiceSource>();

            if (classData != null)
            {
                int maxLevel = Math.Max(1, level);
                IReadOnlyList<DndLevelProgressionData> progressions = DndRuleContentService.Instance.GetClassProgressions(classData.ClassId);
                for (int index = 0; index < progressions.Count; index++)
                {
                    DndLevelProgressionData progression = progressions[index];
                    if (progression == null || progression.Level < 1 || progression.Level > maxLevel)
                    {
                        continue;
                    }

                    sources.Add(new CharacterCreationFeatureChoiceSource
                    {
                        SourceType = "Class",
                        SourceId = classData.ClassId,
                        Level = progression.Level,
                        ChoiceGroupIds = new List<string>(progression.ChoiceGroupIds)
                    });

                    for (int featureIndex = 0; featureIndex < progression.FeatureIds.Count; featureIndex++)
                    {
                        string featureId = progression.FeatureIds[featureIndex];
                        if (!string.IsNullOrWhiteSpace(featureId)
                            && DndRuleContentService.Instance.TryGetFeature(featureId, out DndFeatureDefineData feature))
                        {
                            sources.Add(new CharacterCreationFeatureChoiceSource
                            {
                                SourceType = "Class",
                                SourceId = classData.ClassId,
                                Level = progression.Level,
                                ChoiceGroupIds = new List<string>(feature.ChoiceGroupIds)
                            });
                        }
                    }
                }

                AppendSelectedSubclassFeatureChoiceSources(sources, classData.ClassId, maxLevel);
            }

            if (raceData != null)
            {
                sources.Add(new CharacterCreationFeatureChoiceSource
                {
                    SourceType = "Race",
                    SourceId = raceData.RaceId,
                    Level = 0,
                    ChoiceGroupIds = CharacterCreationRuleService.Instance.BuildRaceChoiceGroupIds(raceData)
                });
            }

            RebuildFeatureChoiceStates(sources);
        }

        private void AppendSelectedSubclassFeatureChoiceSources(
            List<CharacterCreationFeatureChoiceSource> sources,
            string classId,
            int maxLevel)
        {
            string subclassId = GetSelectedSubclassId(classId);
            if (sources == null || string.IsNullOrWhiteSpace(classId) || string.IsNullOrWhiteSpace(subclassId))
            {
                return;
            }

            IReadOnlyList<DndSubclassLevelProgressionData> progressions = DndRuleContentService.Instance.GetSubclassProgressions(subclassId.Trim());
            for (int index = 0; index < progressions.Count; index++)
            {
                DndSubclassLevelProgressionData progression = progressions[index];
                if (progression == null || progression.Level < 1 || progression.Level > maxLevel)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(progression.ClassId)
                    && !string.Equals(progression.ClassId.Trim(), classId.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                sources.Add(new CharacterCreationFeatureChoiceSource
                {
                    SourceType = "Subclass",
                    SourceId = progression.SubclassId,
                    Level = progression.Level,
                    ChoiceGroupIds = new List<string>(progression.ChoiceGroupIds)
                });

                for (int featureIndex = 0; featureIndex < progression.FeatureIds.Count; featureIndex++)
                {
                    string featureId = progression.FeatureIds[featureIndex];
                    if (!string.IsNullOrWhiteSpace(featureId)
                        && DndRuleContentService.Instance.TryGetFeature(featureId, out DndFeatureDefineData feature))
                    {
                        sources.Add(new CharacterCreationFeatureChoiceSource
                        {
                            SourceType = "Subclass",
                            SourceId = progression.SubclassId,
                            Level = progression.Level,
                            ChoiceGroupIds = new List<string>(feature.ChoiceGroupIds)
                        });
                    }
                }
            }
        }

        public CharacterCreationFeatureChoiceState FindOrCreateFeatureChoiceState(
            IReadOnlyList<string> choiceGroupIds,
            string sourceType,
            string sourceId,
            int level)
        {
            if (choiceGroupIds == null)
            {
                return null;
            }

            for (int index = 0; index < choiceGroupIds.Count; index++)
            {
                string choiceGroupId = choiceGroupIds[index];
                if (!TryGetSelectableFeatureChoiceGroup(choiceGroupId, out DndChoiceGroupData choiceGroup))
                {
                    continue;
                }

                CharacterCreationFeatureChoiceState state = FindFeatureChoiceState(choiceGroup.ChoiceGroupId);
                if (state == null)
                {
                    state = BuildFeatureChoiceState(choiceGroup, sourceType, sourceId, level);
                    if (state.OptionIds.Count > 0)
                    {
                        FeatureChoiceStates.Add(state);
                    }
                }

                return state != null && state.OptionIds.Count > 0 ? state : null;
            }

            return null;
        }

        private void AppendSkillChoiceStates(
            IReadOnlyList<CharacterCreationSkillChoiceState> previousStates,
            IReadOnlyList<string> choiceGroupIds,
            string sourceType,
            string sourceId,
            IReadOnlyList<string> fixedSkillIds)
        {
            if (choiceGroupIds == null)
            {
                return;
            }

            for (int index = 0; index < choiceGroupIds.Count; index++)
            {
                string choiceGroupId = choiceGroupIds[index];
                if (string.IsNullOrWhiteSpace(choiceGroupId)
                    || !DndRuleContentService.Instance.TryGetChoiceGroup(choiceGroupId, out DndChoiceGroupData choiceGroup)
                    || !string.Equals(choiceGroup.ChoiceType, "Skill", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                CharacterCreationSkillChoiceState state = new CharacterCreationSkillChoiceState
                {
                    ChoiceGroupId = choiceGroup.ChoiceGroupId,
                    SourceType = sourceType ?? string.Empty,
                    SourceId = sourceId ?? string.Empty,
                    MaxSelect = Math.Max(choiceGroup.MinSelect, choiceGroup.MaxSelect)
                };

                IReadOnlyList<DndChoiceOptionData> options = DndRuleContentService.Instance.GetChoiceOptions(choiceGroup.ChoiceGroupId);
                for (int optionIndex = 0; optionIndex < options.Count; optionIndex++)
                {
                    string skillId = NormalizeSkillId(FirstNonEmpty(options[optionIndex]?.OptionId, options[optionIndex]?.Name));
                    if (string.IsNullOrWhiteSpace(skillId))
                    {
                        continue;
                    }

                    AppendUniqueNormalizedSkillId(state.OptionSkillIds, skillId);
                    if (!ContainsNormalizedSkillId(fixedSkillIds, skillId))
                    {
                        AppendUniqueNormalizedSkillId(state.CandidateSkillIds, skillId);
                    }
                }

                RestoreSkillChoiceSelections(previousStates, state);
                if (state.OptionSkillIds.Count > 0 && state.MaxSelect > 0)
                {
                    SkillChoiceStates.Add(state);
                }
            }
        }

        private static void RestoreSkillChoiceSelections(IReadOnlyList<CharacterCreationSkillChoiceState> previousStates, CharacterCreationSkillChoiceState state)
        {
            if (previousStates == null || state == null)
            {
                return;
            }

            for (int index = 0; index < previousStates.Count; index++)
            {
                CharacterCreationSkillChoiceState previous = previousStates[index];
                if (previous == null || !string.Equals(previous.ChoiceGroupId, state.ChoiceGroupId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                for (int skillIndex = 0; skillIndex < previous.SelectedSkillIds.Count; skillIndex++)
                {
                    string skillId = previous.SelectedSkillIds[skillIndex];
                    if (ContainsNormalizedSkillId(state.OptionSkillIds, skillId)
                        && state.SelectedSkillIds.Count < state.MaxSelect)
                    {
                        AppendUniqueNormalizedSkillId(state.SelectedSkillIds, skillId);
                    }
                }

                return;
            }
        }

        private void AppendFeatureChoiceStates(
            IReadOnlyList<CharacterCreationFeatureChoiceState> previousStates,
            IReadOnlyList<string> choiceGroupIds,
            string sourceType,
            string sourceId,
            int level)
        {
            if (choiceGroupIds == null)
            {
                return;
            }

            for (int index = 0; index < choiceGroupIds.Count; index++)
            {
                string choiceGroupId = choiceGroupIds[index];
                if (!TryGetSelectableFeatureChoiceGroup(choiceGroupId, out DndChoiceGroupData choiceGroup)
                    || FindFeatureChoiceState(choiceGroupId) != null)
                {
                    continue;
                }

                CharacterCreationFeatureChoiceState state = BuildFeatureChoiceState(choiceGroup, sourceType, sourceId, level);
                RestoreFeatureChoiceSelections(previousStates, state);
                if (state.OptionIds.Count > 0)
                {
                    FeatureChoiceStates.Add(state);
                }
            }
        }

        private static CharacterCreationFeatureChoiceState BuildFeatureChoiceState(DndChoiceGroupData choiceGroup, string sourceType, string sourceId, int level)
        {
            CharacterCreationFeatureChoiceState state = new CharacterCreationFeatureChoiceState
            {
                ChoiceGroupId = choiceGroup.ChoiceGroupId,
                ChoiceType = choiceGroup.ChoiceType,
                SourceType = sourceType ?? string.Empty,
                SourceId = sourceId ?? string.Empty,
                ClassId = string.Equals(sourceType, "Class", StringComparison.OrdinalIgnoreCase) ? sourceId ?? string.Empty : string.Empty,
                Level = level,
                MinSelect = Math.Max(0, choiceGroup.MinSelect),
                MaxSelect = Math.Max(choiceGroup.MinSelect, choiceGroup.MaxSelect),
                DisplayLabel = FirstNonEmpty(choiceGroup.Name, choiceGroup.ChoiceGroupId)
            };

            AppendFeatureChoiceStateOptions(state, choiceGroup.ChoiceGroupId);
            return state;
        }

        private static void RestoreFeatureChoiceSelections(IReadOnlyList<CharacterCreationFeatureChoiceState> previousStates, CharacterCreationFeatureChoiceState state)
        {
            if (previousStates == null || state == null)
            {
                return;
            }

            for (int index = 0; index < previousStates.Count; index++)
            {
                CharacterCreationFeatureChoiceState previous = previousStates[index];
                if (previous == null || !string.Equals(previous.ChoiceGroupId, state.ChoiceGroupId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                for (int optionIndex = 0; optionIndex < previous.SelectedOptionIds.Count; optionIndex++)
                {
                    string optionId = previous.SelectedOptionIds[optionIndex];
                    if (ContainsExactValue(state.OptionIds, optionId) && (state.MaxSelect <= 0 || state.SelectedOptionIds.Count < state.MaxSelect))
                    {
                        if (IsRepeatableAbilityScoreChoice(state))
                        {
                            state.SelectedOptionIds.Add(optionId.Trim());
                            state.PendingOptionIds.Add(optionId.Trim());
                        }
                        else
                        {
                            AppendUniqueExactValue(state.SelectedOptionIds, optionId);
                            AppendUniqueExactValue(state.PendingOptionIds, optionId);
                        }
                    }
                }

                return;
            }
        }

        private static void AppendFeatureChoiceStateOptions(CharacterCreationFeatureChoiceState state, string choiceGroupId)
        {
            if (state == null || string.IsNullOrWhiteSpace(choiceGroupId))
            {
                return;
            }

            IReadOnlyList<DndChoiceOptionData> options = DndRuleContentService.Instance.GetChoiceOptions(choiceGroupId);
            for (int index = 0; index < options.Count; index++)
            {
                DndChoiceOptionData option = options[index];
                if (option == null || string.IsNullOrWhiteSpace(option.OptionId))
                {
                    continue;
                }

                AppendUniqueExactValue(state.OptionIds, option.OptionId);
                if (!state.OptionDisplayNameById.ContainsKey(option.OptionId))
                {
                    state.OptionDisplayNameById[option.OptionId] = FirstNonEmpty(option.Name, option.OptionId);
                }
            }
        }

        private static bool TryGetSelectableFeatureChoiceGroup(string choiceGroupId, out DndChoiceGroupData choiceGroup)
        {
            choiceGroup = null;
            if (string.IsNullOrWhiteSpace(choiceGroupId)
                || !DndRuleContentService.Instance.TryGetChoiceGroup(choiceGroupId, out choiceGroup)
                || string.Equals(choiceGroup.ChoiceType, "Skill", StringComparison.OrdinalIgnoreCase)
                || string.Equals(choiceGroup.ChoiceType, "Tool", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        private static string ResolveAdvancementFollowupChoiceGroupId(DndChoiceGroupData parentGroup, string optionId)
        {
            string normalized = optionId?.Trim() ?? string.Empty;
            if (parentGroup?.NextChoiceGroupIds != null && parentGroup.NextChoiceGroupIds.Count > 0)
            {
                if (string.Equals(normalized, "option_asi", StringComparison.OrdinalIgnoreCase)
                    && parentGroup.NextChoiceGroupIds.Count > 0)
                {
                    return parentGroup.NextChoiceGroupIds[0]?.Trim() ?? string.Empty;
                }

                if (string.Equals(normalized, "option_feat", StringComparison.OrdinalIgnoreCase)
                    && parentGroup.NextChoiceGroupIds.Count > 1)
                {
                    return parentGroup.NextChoiceGroupIds[1]?.Trim() ?? string.Empty;
                }
            }

            if (string.Equals(normalized, "option_asi", StringComparison.OrdinalIgnoreCase))
            {
                return "choice_asi_attributes";
            }

            if (string.Equals(normalized, "option_feat", StringComparison.OrdinalIgnoreCase))
            {
                return "choice_feat_any";
            }

            return string.Empty;
        }

        private static bool IsAdvancementOptionChoiceType(string choiceType)
        {
            return string.Equals(choiceType, "AdvancementOption", StringComparison.OrdinalIgnoreCase)
                || string.Equals(choiceType, "FeatOrAbilityScore", StringComparison.OrdinalIgnoreCase);
        }

        public void RemoveToolChoiceStatesBySource(string sourceType)
        {
            RemoveChoiceStatesBySource(ToolChoiceStates, sourceType);
        }

        public void RemoveFeatureChoiceStatesBySource(string sourceType)
        {
            RemoveChoiceStatesBySource(FeatureChoiceStates, sourceType);
        }

        public void ClearChoiceState()
        {
            SkillChoiceStates.Clear();
            ToolChoiceStates.Clear();
            FeatureChoiceStates.Clear();
            ClearRaceAbilityChoiceState();
            ClearActiveChoice();
        }

        public void ClearRaceAbilityChoiceState()
        {
            RaceAbilityChoiceState.FixedAbilityBonuses.Clear();
            RaceAbilityChoiceState.SelectedAbilityBonuses.Clear();
            RaceAbilityChoiceState.OptionIdByAbility.Clear();
            RaceAbilityChoiceState.ChoiceGroupId = string.Empty;
            RaceAbilityChoiceState.SelectionMode = string.Empty;
            RaceAbilityChoiceState.SourceRaceId = string.Empty;
            RaceAbilityChoiceState.MaxSelect = 0;
            RefreshDerivedStatsAfterAbilityScoresChanged();
        }

        public void ConfigureRaceAbilityChoice(
            string sourceRaceId,
            Dictionary<string, int> fixedAbilityBonuses,
            string choiceGroupId,
            string selectionMode,
            int maxSelect,
            Dictionary<string, string> optionIdByAbility)
        {
            ClearRaceAbilityChoiceState();
            RaceAbilityChoiceState.SourceRaceId = sourceRaceId?.Trim() ?? string.Empty;
            RaceAbilityChoiceState.ChoiceGroupId = choiceGroupId?.Trim() ?? string.Empty;
            RaceAbilityChoiceState.SelectionMode = selectionMode?.Trim() ?? string.Empty;
            RaceAbilityChoiceState.MaxSelect = Math.Max(0, maxSelect);
            CopyAbilityBonuses(RaceAbilityChoiceState.FixedAbilityBonuses, fixedAbilityBonuses);
            CopyAbilityOptions(RaceAbilityChoiceState.OptionIdByAbility, optionIdByAbility);
            m_state.IsDirty = true;
            RefreshDerivedStatsAfterAbilityScoresChanged();
        }

        public bool ChangeRaceSelectedAbilityBonus(string abilityId, int delta)
        {
            string normalized = NormalizeAbilityId(abilityId);
            if (string.IsNullOrWhiteSpace(normalized)
                || string.IsNullOrWhiteSpace(RaceAbilityChoiceState.ChoiceGroupId)
                || !RaceAbilityChoiceState.OptionIdByAbility.ContainsKey(normalized))
            {
                return false;
            }

            int current = GetDictionaryValue(RaceAbilityChoiceState.SelectedAbilityBonuses, normalized);
            if (delta > 0)
            {
                if (CountSelectedRaceAbilityBonuses() >= RaceAbilityChoiceState.MaxSelect
                    || current >= GetRaceAbilityChoiceOptionMax())
                {
                    return false;
                }

                RaceAbilityChoiceState.SelectedAbilityBonuses[normalized] = current + 1;
                m_state.IsDirty = true;
                RefreshDerivedStatsAfterAbilityScoresChanged();
                return true;
            }

            if (delta < 0)
            {
                if (current <= 0)
                {
                    return false;
                }

                if (current == 1)
                {
                    RaceAbilityChoiceState.SelectedAbilityBonuses.Remove(normalized);
                }
                else
                {
                    RaceAbilityChoiceState.SelectedAbilityBonuses[normalized] = current - 1;
                }

                m_state.IsDirty = true;
                RefreshDerivedStatsAfterAbilityScoresChanged();
                return true;
            }

            return false;
        }

        public int GetCurrentAbilityScore(string abilityId, int baseScore)
        {
            string normalized = NormalizeAbilityId(abilityId);
            return GetGeneratedOrDefaultAbilityScore(normalized, baseScore)
                + GetDictionaryValue(RaceAbilityChoiceState.FixedAbilityBonuses, normalized)
                + GetDictionaryValue(RaceAbilityChoiceState.SelectedAbilityBonuses, normalized);
        }

        public bool CanIncreaseRaceAbility(string abilityId)
        {
            if (CanIncreaseActiveAbilityScoreFeatureChoice(abilityId))
            {
                return true;
            }

            if (IsPointBuyMode())
            {
                return CanIncreasePointBuyAbility(abilityId);
            }

            string normalized = NormalizeAbilityId(abilityId);
            bool available = !string.IsNullOrWhiteSpace(RaceAbilityChoiceState.ChoiceGroupId)
                && RaceAbilityChoiceState.OptionIdByAbility.ContainsKey(normalized);
            int current = GetDictionaryValue(RaceAbilityChoiceState.SelectedAbilityBonuses, normalized);
            return available
                && CountSelectedRaceAbilityBonuses() < RaceAbilityChoiceState.MaxSelect
                && current < GetRaceAbilityChoiceOptionMax();
        }

        public bool CanDecreaseRaceAbility(string abilityId)
        {
            if (CanDecreaseActiveAbilityScoreFeatureChoice(abilityId))
            {
                return true;
            }

            if (IsPointBuyMode())
            {
                return CanDecreasePointBuyAbility(abilityId);
            }

            string normalized = NormalizeAbilityId(abilityId);
            bool available = !string.IsNullOrWhiteSpace(RaceAbilityChoiceState.ChoiceGroupId)
                && RaceAbilityChoiceState.OptionIdByAbility.ContainsKey(normalized);
            return available && GetDictionaryValue(RaceAbilityChoiceState.SelectedAbilityBonuses, normalized) > 0;
        }

        public List<CharacterCreationRaceAbilityChoiceInput> BuildRaceAbilityChoiceInputs()
        {
            List<CharacterCreationRaceAbilityChoiceInput> result = new List<CharacterCreationRaceAbilityChoiceInput>();
            if (string.IsNullOrWhiteSpace(RaceAbilityChoiceState.ChoiceGroupId))
            {
                return result;
            }

            foreach (KeyValuePair<string, int> pair in RaceAbilityChoiceState.SelectedAbilityBonuses)
            {
                if (pair.Value <= 0
                    || !RaceAbilityChoiceState.OptionIdByAbility.TryGetValue(pair.Key, out string optionId)
                    || string.IsNullOrWhiteSpace(optionId))
                {
                    continue;
                }

                result.Add(new CharacterCreationRaceAbilityChoiceInput
                {
                    ChoiceGroupId = RaceAbilityChoiceState.ChoiceGroupId,
                    OptionId = optionId,
                    SourceId = RaceAbilityChoiceState.SourceRaceId,
                    Count = pair.Value
                });
            }

            return result;
        }

        public void MarkClean()
        {
            EnsureState();
            m_state.IsDirty = false;
        }

        private static T FindChoiceState<T>(List<T> states, string choiceGroupId) where T : class
        {
            if (states == null || string.IsNullOrWhiteSpace(choiceGroupId))
            {
                return null;
            }

            for (int index = 0; index < states.Count; index++)
            {
                T state = states[index];
                if (state is CharacterCreationToolChoiceState toolState
                    && string.Equals(toolState.ChoiceGroupId, choiceGroupId, StringComparison.OrdinalIgnoreCase))
                {
                    return state;
                }

                if (state is CharacterCreationFeatureChoiceState featureState
                    && string.Equals(featureState.ChoiceGroupId, choiceGroupId, StringComparison.OrdinalIgnoreCase))
                {
                    return state;
                }
            }

            return null;
        }

        private static void RemoveChoiceStatesBySource<T>(List<T> states, string sourceType)
        {
            if (states == null || string.IsNullOrWhiteSpace(sourceType))
            {
                return;
            }

            for (int index = states.Count - 1; index >= 0; index--)
            {
                T state = states[index];
                if (state is CharacterCreationToolChoiceState toolState
                    && string.Equals(toolState.SourceType, sourceType, StringComparison.OrdinalIgnoreCase))
                {
                    states.RemoveAt(index);
                    continue;
                }

                if (state is CharacterCreationFeatureChoiceState featureState
                    && string.Equals(featureState.SourceType, sourceType, StringComparison.OrdinalIgnoreCase))
                {
                    states.RemoveAt(index);
                }
            }
        }

        private static CharacterCreationToolChoiceState ValidateToolChoiceState(CharacterCreationToolChoiceState state)
        {
            if (state == null || state.OptionToolIds.Count == 0 || state.MaxSelect <= 0)
            {
                return null;
            }

            RemoveMissingValues(state.PendingToolIds, state.OptionToolIds);
            RemoveMissingValues(state.SelectedToolIds, state.OptionToolIds);

            while (state.PendingToolIds.Count > state.MaxSelect)
            {
                state.PendingToolIds.RemoveAt(state.PendingToolIds.Count - 1);
            }

            while (state.SelectedToolIds.Count > state.MaxSelect)
            {
                state.SelectedToolIds.RemoveAt(state.SelectedToolIds.Count - 1);
            }

            return state;
        }

        private static void TogglePendingValue(List<string> values, string value, int maxSelect)
        {
            if (values == null || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            string normalized = value.Trim();
            for (int index = values.Count - 1; index >= 0; index--)
            {
                if (string.Equals(values[index], normalized, StringComparison.OrdinalIgnoreCase))
                {
                    values.RemoveAt(index);
                    return;
                }
            }

            if (maxSelect > 0 && values.Count >= maxSelect)
            {
                values.RemoveAt(0);
            }

            values.Add(normalized);
        }

        private static bool IsRepeatableAbilityScoreChoice(CharacterCreationFeatureChoiceState state)
        {
            if (state == null
                || !string.Equals(state.ChoiceType, "AbilityScore", StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(state.ChoiceGroupId)
                || !DndRuleContentService.Instance.TryGetChoiceGroup(state.ChoiceGroupId, out DndChoiceGroupData choiceGroup)
                || choiceGroup == null)
            {
                return false;
            }

            return string.Equals(choiceGroup.SelectionMode, "Repeatable", StringComparison.OrdinalIgnoreCase);
        }

        private static void ToggleRepeatableAbilityScoreChoice(CharacterCreationFeatureChoiceState state, string optionId)
        {
            if (state == null || string.IsNullOrWhiteSpace(optionId))
            {
                return;
            }

            string normalized = optionId.Trim();
            int existingCount = CountExactValues(state.PendingOptionIds, normalized);
            if (existingCount > 0 && (state.MaxSelect <= 0 || state.PendingOptionIds.Count >= state.MaxSelect))
            {
                RemoveLastExactValue(state.PendingOptionIds, normalized);
                return;
            }

            if (state.MaxSelect > 0 && state.PendingOptionIds.Count >= state.MaxSelect)
            {
                return;
            }

            int maxPerOption = 0;
            int targetCap = 0;
            if (DndRuleContentService.Instance.TryGetChoiceGroup(state.ChoiceGroupId, out DndChoiceGroupData choiceGroup)
                && choiceGroup != null)
            {
                maxPerOption = choiceGroup.MaxValuePerOption;
                targetCap = choiceGroup.TargetValueCap;
            }

            if (maxPerOption > 0 && existingCount >= maxPerOption)
            {
                RemoveLastExactValue(state.PendingOptionIds, normalized);
                return;
            }

            if (targetCap > 0 && Instance.GetCurrentAbilityScore(normalized, 10) + existingCount >= targetCap)
            {
                return;
            }

            state.PendingOptionIds.Add(normalized);
        }

        private bool TryChangeActiveAbilityScoreFeatureChoice(string abilityId, int delta)
        {
            CharacterCreationFeatureChoiceState state = m_activeFeatureChoiceState;
            if (!IsRepeatableAbilityScoreChoice(state))
            {
                return false;
            }

            string normalized = NormalizeAbilityId(abilityId);
            if (string.IsNullOrWhiteSpace(normalized) || !ContainsExactValue(state.OptionIds, normalized))
            {
                return false;
            }

            if (delta > 0)
            {
                if (!CanIncreaseActiveAbilityScoreFeatureChoice(normalized))
                {
                    return false;
                }

                state.PendingOptionIds.Add(normalized);
                m_state.IsDirty = true;
                RefreshDerivedStatsAfterAbilityScoresChanged();
                return true;
            }

            if (delta < 0)
            {
                if (!CanDecreaseActiveAbilityScoreFeatureChoice(normalized))
                {
                    return false;
                }

                RemoveLastExactValue(state.PendingOptionIds, normalized);
                m_state.IsDirty = true;
                RefreshDerivedStatsAfterAbilityScoresChanged();
                return true;
            }

            return false;
        }

        private bool CanIncreaseActiveAbilityScoreFeatureChoice(string abilityId)
        {
            CharacterCreationFeatureChoiceState state = m_activeFeatureChoiceState;
            if (!IsRepeatableAbilityScoreChoice(state))
            {
                return false;
            }

            string normalized = NormalizeAbilityId(abilityId);
            if (string.IsNullOrWhiteSpace(normalized)
                || !ContainsExactValue(state.OptionIds, normalized)
                || (state.MaxSelect > 0 && state.PendingOptionIds.Count >= state.MaxSelect))
            {
                return false;
            }

            int existingCount = CountExactValues(state.PendingOptionIds, normalized);
            int maxPerOption = 0;
            int targetCap = 0;
            if (DndRuleContentService.Instance.TryGetChoiceGroup(state.ChoiceGroupId, out DndChoiceGroupData choiceGroup)
                && choiceGroup != null)
            {
                maxPerOption = choiceGroup.MaxValuePerOption;
                targetCap = choiceGroup.TargetValueCap;
            }

            if (maxPerOption > 0 && existingCount >= maxPerOption)
            {
                return false;
            }

            return targetCap <= 0 || GetCurrentAbilityScore(normalized, 10) + existingCount < targetCap;
        }

        private bool CanDecreaseActiveAbilityScoreFeatureChoice(string abilityId)
        {
            CharacterCreationFeatureChoiceState state = m_activeFeatureChoiceState;
            string normalized = NormalizeAbilityId(abilityId);
            return IsRepeatableAbilityScoreChoice(state)
                && !string.IsNullOrWhiteSpace(normalized)
                && CountExactValues(state.PendingOptionIds, normalized) > 0;
        }

        private static int CountExactValues(IReadOnlyList<string> values, string value)
        {
            int count = 0;
            if (values == null || string.IsNullOrWhiteSpace(value))
            {
                return count;
            }

            for (int index = 0; index < values.Count; index++)
            {
                if (string.Equals(values[index], value, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            return count;
        }

        private static void RemoveLastExactValue(List<string> values, string value)
        {
            if (values == null || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            for (int index = values.Count - 1; index >= 0; index--)
            {
                if (string.Equals(values[index], value, StringComparison.OrdinalIgnoreCase))
                {
                    values.RemoveAt(index);
                    return;
                }
            }
        }

        private static void AppendValues(List<string> target, List<string> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                string value = source[index]?.Trim() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    target.Add(value);
                }
            }
        }

        private static void AppendUniqueValues(List<string> target, List<string> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                string value = source[index]?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                bool exists = false;
                for (int targetIndex = 0; targetIndex < target.Count; targetIndex++)
                {
                    if (string.Equals(target[targetIndex], value, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    target.Add(value);
                }
            }
        }

        private static void RemoveMissingValues(List<string> values, IReadOnlyList<string> validValues)
        {
            if (values == null)
            {
                return;
            }

            for (int index = values.Count - 1; index >= 0; index--)
            {
                if (!ContainsExactValue(validValues, values[index]))
                {
                    values.RemoveAt(index);
                }
            }
        }

        private static bool ContainsExactValue(IReadOnlyList<string> values, string value)
        {
            if (values == null || string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string normalized = value.Trim();
            for (int index = 0; index < values.Count; index++)
            {
                if (string.Equals(values[index]?.Trim(), normalized, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static void AppendUniqueExactValue(List<string> target, string value)
        {
            if (target == null || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (!ContainsExactValue(target, value))
            {
                target.Add(value.Trim());
            }
        }

        private static void AppendUniqueExactValues(List<string> target, IReadOnlyList<string> values)
        {
            if (target == null || values == null)
            {
                return;
            }

            for (int index = 0; index < values.Count; index++)
            {
                AppendUniqueExactValue(target, values[index]);
            }
        }

        private static bool ContainsNormalizedSkillId(IReadOnlyList<string> values, string id)
        {
            if (values == null || string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            string normalizedId = NormalizeSkillId(id);
            for (int index = 0; index < values.Count; index++)
            {
                if (string.Equals(NormalizeSkillId(values[index]), normalizedId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static void AppendUniqueNormalizedSkillId(List<string> target, string value)
        {
            if (target == null || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            string normalized = NormalizeSkillId(value);
            if (!string.IsNullOrWhiteSpace(normalized) && !ContainsNormalizedSkillId(target, normalized))
            {
                target.Add(normalized);
            }
        }

        private static void AppendUniqueNormalizedSkillIds(List<string> target, IReadOnlyList<string> values)
        {
            if (target == null || values == null)
            {
                return;
            }

            for (int index = 0; index < values.Count; index++)
            {
                AppendUniqueNormalizedSkillId(target, values[index]);
            }
        }

        private static string NormalizeSkillId(string value)
        {
            string normalized = value?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return string.Empty;
            }

            normalized = normalized.Replace("技能：", string.Empty).Replace("技能:", string.Empty).Trim();

            if (DndRuleContentService.Instance.TryGetSkill(normalized, out DndSkillDefineData directSkill))
            {
                return directSkill.SkillId;
            }

            IReadOnlyList<DndSkillDefineData> skills = DndRuleContentService.Instance.Skills;
            for (int index = 0; index < skills.Count; index++)
            {
                DndSkillDefineData skill = skills[index];
                if (skill == null)
                {
                    continue;
                }

                if (string.Equals(skill.SkillId, normalized, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(skill.Name, normalized, StringComparison.OrdinalIgnoreCase))
                {
                    return skill.SkillId;
                }
            }

            return normalized;
        }

        private static string FirstNonEmpty(string first, string second)
        {
            return !string.IsNullOrWhiteSpace(first) ? first.Trim() : second?.Trim() ?? string.Empty;
        }

        private static int GetDictionaryValue(Dictionary<string, int> values, string key)
        {
            return values != null && !string.IsNullOrWhiteSpace(key) && values.TryGetValue(key, out int value) ? value : 0;
        }

        private int GetRaceAbilityChoiceOptionMax()
        {
            if (string.Equals(RaceAbilityChoiceState.SelectionMode, "Repeatable", StringComparison.OrdinalIgnoreCase))
            {
                return Math.Max(1, RaceAbilityChoiceState.MaxSelect);
            }

            return 1;
        }

        private int CountSelectedRaceAbilityBonuses()
        {
            int count = 0;
            foreach (KeyValuePair<string, int> pair in RaceAbilityChoiceState.SelectedAbilityBonuses)
            {
                count += Math.Max(0, pair.Value);
            }

            return count;
        }

        private void AddGeneratedAbilityScores(IReadOnlyList<int> values)
        {
            if (values == null)
            {
                return;
            }

            for (int index = 0; index < values.Count; index++)
            {
                AbilityGenerationState.Scores.Add(new CharacterCreationGeneratedAbilityScoreState
                {
                    ScoreId = $"score_{index + 1}",
                    Value = Math.Max(1, values[index])
                });
            }
        }

        private void InitializePointBuyScores()
        {
            AbilityGenerationState.PointBuyScores.Clear();
            AbilityGenerationState.PointBuyScores["Strength"] = PointBuyMinScore;
            AbilityGenerationState.PointBuyScores["Dexterity"] = PointBuyMinScore;
            AbilityGenerationState.PointBuyScores["Constitution"] = PointBuyMinScore;
            AbilityGenerationState.PointBuyScores["Intelligence"] = PointBuyMinScore;
            AbilityGenerationState.PointBuyScores["Wisdom"] = PointBuyMinScore;
            AbilityGenerationState.PointBuyScores["Charisma"] = PointBuyMinScore;
        }

        private void InitializeManualScores()
        {
            AbilityGenerationState.ManualScores.Clear();
            AbilityGenerationState.ManualScores["Strength"] = 10;
            AbilityGenerationState.ManualScores["Dexterity"] = 10;
            AbilityGenerationState.ManualScores["Constitution"] = 10;
            AbilityGenerationState.ManualScores["Intelligence"] = 10;
            AbilityGenerationState.ManualScores["Wisdom"] = 10;
            AbilityGenerationState.ManualScores["Charisma"] = 10;
        }

        private bool ChangePointBuyAbilityScore(string abilityId, int delta)
        {
            string normalized = NormalizeAbilityId(abilityId);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return false;
            }

            int current = GetPointBuyAbilityScore(normalized);
            int next = current + delta;
            if (next < PointBuyMinScore || next > PointBuyMaxScore)
            {
                return false;
            }

            int currentTotalCost = CalculatePointBuyTotalCost();
            int nextTotalCost = currentTotalCost - GetPointBuyCost(current) + GetPointBuyCost(next);
            if (nextTotalCost > PointBuyBudget)
            {
                return false;
            }

            AbilityGenerationState.PointBuyScores[normalized] = next;
            m_state.IsDirty = true;
            RefreshDerivedStatsAfterAbilityScoresChanged();
            return true;
        }

        private bool CanIncreasePointBuyAbility(string abilityId)
        {
            string normalized = NormalizeAbilityId(abilityId);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return false;
            }

            int current = GetPointBuyAbilityScore(normalized);
            if (current >= PointBuyMaxScore)
            {
                return false;
            }

            int nextTotalCost = CalculatePointBuyTotalCost() - GetPointBuyCost(current) + GetPointBuyCost(current + 1);
            return nextTotalCost <= PointBuyBudget;
        }

        private bool CanDecreasePointBuyAbility(string abilityId)
        {
            string normalized = NormalizeAbilityId(abilityId);
            return !string.IsNullOrWhiteSpace(normalized) && GetPointBuyAbilityScore(normalized) > PointBuyMinScore;
        }

        private int GetPointBuyAbilityScore(string abilityId)
        {
            string normalized = NormalizeAbilityId(abilityId);
            return AbilityGenerationState.PointBuyScores.TryGetValue(normalized, out int score)
                ? score
                : PointBuyMinScore;
        }

        private int CalculatePointBuyTotalCost()
        {
            int total = 0;
            foreach (KeyValuePair<string, int> pair in AbilityGenerationState.PointBuyScores)
            {
                total += GetPointBuyCost(pair.Value);
            }

            return total;
        }

        private static int GetPointBuyCost(int score)
        {
            return score switch
            {
                <= 8 => 0,
                9 => 1,
                10 => 2,
                11 => 3,
                12 => 4,
                13 => 5,
                14 => 7,
                _ => 9
            };
        }

        private bool IsPointBuyMode()
        {
            return string.Equals(AbilityGenerationState.MethodId, AbilityGenerationMethodPointBuy, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsManualAbilityMode()
        {
            return string.Equals(AbilityGenerationState.MethodId, AbilityGenerationMethodManual, StringComparison.OrdinalIgnoreCase);
        }

        private int RollFourD6DropLowest()
        {
            int total = 0;
            int lowest = 7;
            for (int index = 0; index < 4; index++)
            {
                int roll = m_random.Next(1, 7);
                total += roll;
                if (roll < lowest)
                {
                    lowest = roll;
                }
            }

            return total - lowest;
        }

        private List<int> RollBestAbilityScoreSet()
        {
            List<int> bestScores = null;
            int bestTotal = -1;
            for (int setIndex = 0; setIndex < 3; setIndex++)
            {
                List<int> scores = RollAbilityScoreSet();
                int total = SumValues(scores);
                if (bestScores == null || total > bestTotal)
                {
                    bestScores = scores;
                    bestTotal = total;
                }
            }

            return bestScores ?? new List<int>();
        }

        private List<int> RollAbilityScoreSet()
        {
            List<int> scores = new List<int>();
            for (int index = 0; index < 6; index++)
            {
                scores.Add(RollFourD6DropLowest());
            }

            return scores;
        }

        private static int SumValues(IReadOnlyList<int> values)
        {
            int total = 0;
            if (values == null)
            {
                return total;
            }

            for (int index = 0; index < values.Count; index++)
            {
                total += values[index];
            }

            return total;
        }

        private static int CalculateAverageHitDieGain(int hitDie)
        {
            return hitDie > 0 ? hitDie / 2 + 1 : 1;
        }

        private void RefreshDerivedStatsAfterAbilityScoresChanged()
        {
            SyncHitPointsForCurrentLevel();
        }

        private bool SyncHitPointsForCurrentLevel()
        {
            EnsureState();
            CharacterCardDraftSaveData character = m_state.Character;
            if (character == null || string.IsNullOrWhiteSpace(character.ClassId))
            {
                return false;
            }

            string hpModeId = CharacterHpModeIds.Normalize(character.HpModeId);
            if (string.Equals(hpModeId, CharacterHpModeIds.Custom, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!DndRuleContentService.Instance.TryGetClass(character.ClassId, out DndClassDefineData classData)
                || classData == null
                || classData.HitDie <= 0)
            {
                return false;
            }

            int level = Math.Max(1, character.Level);
            int hitDie = Math.Max(1, classData.HitDie);
            int constitutionModifier = CharacterCreationCalculationService.Instance.CalculateAbilityModifier(GetCurrentAbilityScore("Constitution", 10));
            List<CharacterHpRollSaveData> rolls = BuildHitPointRollsForLevel(
                character.HpRolls,
                character.ClassId,
                hpModeId,
                level,
                hitDie,
                constitutionModifier);

            int maxHp = 0;
            for (int index = 0; index < rolls.Count; index++)
            {
                maxHp += Math.Max(0, rolls[index].HpGain);
            }

            int manualHpBonus = Math.Max(0, character.ManualHp);
            CharacterRuntimeSnapshotData hpBonusSnapshot = BuildHitPointBonusSnapshot(character);
            int maxHpBonus = CharacterDetailCalculationService.Instance.CalculateCharacterAndItemEffectBonus(character, hpBonusSnapshot, "MaxHpBonus", "MaxHp", "HP", "HitPoints");
            int hitPointBonus = CharacterDetailCalculationService.Instance.CalculateCharacterAndItemEffectBonus(character, hpBonusSnapshot, "HitPointBonus", "MaxHp", "HP", "HitPoints");
            int hpBonus = CharacterDetailCalculationService.Instance.CalculateCharacterAndItemEffectBonus(character, hpBonusSnapshot, "HPBonus", "MaxHp", "HP", "HitPoints");
            int totalBonus = manualHpBonus + maxHpBonus + hitPointBonus + hpBonus;
            maxHp = Math.Max(1, maxHp + totalBonus);
            character.HpModeId = hpModeId;
            character.HpRolls = rolls;
            character.MaxHp = maxHp;
            character.CurrentHp = maxHp;
            character.TemporaryHp = Math.Max(0, character.TemporaryHp);
            m_state.IsDirty = true;
            LogHitPointBreakdown(character, classData, rolls, manualHpBonus, maxHpBonus, hitPointBonus, hpBonus, maxHp);
            return true;
        }

        private List<CharacterHpRollSaveData> BuildHitPointRollsForLevel(
            List<CharacterHpRollSaveData> existingRolls,
            string classId,
            string hpModeId,
            int level,
            int hitDie,
            int constitutionModifier)
        {
            List<CharacterHpRollSaveData> result = new List<CharacterHpRollSaveData>();
            string normalizedClassId = classId?.Trim() ?? string.Empty;
            for (int currentLevel = 1; currentLevel <= level; currentLevel++)
            {
                CharacterHpRollSaveData existingRoll = FindHpRoll(existingRolls, currentLevel);
                int rollValue = ResolveHitPointRollValue(existingRoll, hpModeId, currentLevel, hitDie);
                int hpGain = Math.Max(1, rollValue + constitutionModifier);
                result.Add(new CharacterHpRollSaveData
                {
                    Level = currentLevel,
                    ClassId = normalizedClassId,
                    HitDie = hitDie,
                    RollValue = rollValue,
                    ConstitutionModifier = constitutionModifier,
                    HpGain = hpGain
                });
            }

            return result;
        }

        private int ResolveHitPointRollValue(CharacterHpRollSaveData existingRoll, string hpModeId, int level, int hitDie)
        {
            if (level <= 1)
            {
                return hitDie;
            }

            if (string.Equals(hpModeId, CharacterHpModeIds.Average, StringComparison.OrdinalIgnoreCase))
            {
                return CalculateAverageHitDieGain(hitDie);
            }

            if (existingRoll != null && existingRoll.RollValue > 0)
            {
                return Math.Max(1, Math.Min(hitDie, existingRoll.RollValue));
            }

            return m_random.Next(1, hitDie + 1);
        }

        private static CharacterHpRollSaveData FindHpRoll(List<CharacterHpRollSaveData> rolls, int level)
        {
            if (rolls == null)
            {
                return null;
            }

            for (int index = 0; index < rolls.Count; index++)
            {
                CharacterHpRollSaveData roll = rolls[index];
                if (roll != null && roll.Level == level)
                {
                    return roll;
                }
            }

            return null;
        }

        private int CalculateHitPointBonus(CharacterCardDraftSaveData character)
        {
            if (character == null)
            {
                return 0;
            }

            CharacterRuntimeSnapshotData snapshot = BuildHitPointBonusSnapshot(character);

            int bonus = Math.Max(0, character.ManualHp);
            bonus += CharacterDetailCalculationService.Instance.CalculateCharacterAndItemEffectBonus(character, snapshot, "MaxHpBonus", "MaxHp", "HP", "HitPoints");
            bonus += CharacterDetailCalculationService.Instance.CalculateCharacterAndItemEffectBonus(character, snapshot, "HitPointBonus", "MaxHp", "HP", "HitPoints");
            bonus += CharacterDetailCalculationService.Instance.CalculateCharacterAndItemEffectBonus(character, snapshot, "HPBonus", "MaxHp", "HP", "HitPoints");
            return bonus;
        }

        private CharacterRuntimeSnapshotData BuildHitPointBonusSnapshot(CharacterCardDraftSaveData character)
        {
            CharacterRuntimeSnapshotData snapshot = CharacterRuntimeSnapshotData.Clone(character?.RuntimeSnapshot);
            snapshot.Level = Math.Max(1, character?.Level ?? 1);
            snapshot.Strength = GetCurrentAbilityScore("Strength", 10);
            snapshot.Dexterity = GetCurrentAbilityScore("Dexterity", 10);
            snapshot.Constitution = GetCurrentAbilityScore("Constitution", 10);
            snapshot.Intelligence = GetCurrentAbilityScore("Intelligence", 10);
            snapshot.Wisdom = GetCurrentAbilityScore("Wisdom", 10);
            snapshot.Charisma = GetCurrentAbilityScore("Charisma", 10);
            return snapshot;
        }

        private void LogHitPointBreakdown(
            CharacterCardDraftSaveData character,
            DndClassDefineData classData,
            IReadOnlyList<CharacterHpRollSaveData> rolls,
            int manualHpBonus,
            int maxHpBonus,
            int hitPointBonus,
            int hpBonus,
            int finalMaxHp)
        {
            int rollTotal = 0;
            StringBuilder builder = new StringBuilder();
            builder.Append("[CharacterCreationUI] 生命值变化 ");
            builder.Append("角色=");
            builder.Append(string.IsNullOrWhiteSpace(character?.CharacterName) ? "(未命名)" : character.CharacterName.Trim());
            builder.Append(", 职业=");
            builder.Append(classData != null && !string.IsNullOrWhiteSpace(classData.Name) ? classData.Name.Trim() : character?.ClassId ?? string.Empty);
            builder.Append(", 等级=");
            builder.Append(Math.Max(1, character?.Level ?? 1));
            builder.Append(", 模式=");
            builder.Append(CharacterHpModeIds.Normalize(character?.HpModeId));
            builder.AppendLine();

            if (rolls != null)
            {
                for (int index = 0; index < rolls.Count; index++)
                {
                    CharacterHpRollSaveData roll = rolls[index];
                    if (roll == null)
                    {
                        continue;
                    }

                    rollTotal += Math.Max(0, roll.HpGain);
                    builder.Append("  Lv");
                    builder.Append(Math.Max(1, roll.Level));
                    builder.Append(": d");
                    builder.Append(Math.Max(0, roll.HitDie));
                    builder.Append("=");
                    builder.Append(Math.Max(0, roll.RollValue));
                    builder.Append(" + 体质调整值=");
                    builder.Append(roll.ConstitutionModifier);
                    builder.Append(" => ");
                    builder.Append(Math.Max(0, roll.HpGain));
                    builder.AppendLine();
                }
            }

            builder.Append("  等级生命小计=");
            builder.Append(rollTotal);
            builder.Append(", 手动加值=");
            builder.Append(manualHpBonus);
            builder.Append(", MaxHpBonus=");
            builder.Append(maxHpBonus);
            builder.Append(", HitPointBonus=");
            builder.Append(hitPointBonus);
            builder.Append(", HPBonus=");
            builder.Append(hpBonus);
            builder.Append(", 最终MaxHp=");
            builder.Append(finalMaxHp);
            Debug.Log(builder.ToString());
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

        private CharacterCreationGeneratedAbilityScoreState FindGeneratedAbilityScore(string scoreId)
        {
            if (string.IsNullOrWhiteSpace(scoreId))
            {
                return null;
            }

            for (int index = 0; index < AbilityGenerationState.Scores.Count; index++)
            {
                CharacterCreationGeneratedAbilityScoreState score = AbilityGenerationState.Scores[index];
                if (score != null && string.Equals(score.ScoreId, scoreId.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return score;
                }
            }

            return null;
        }

        private int GetGeneratedOrDefaultAbilityScore(string abilityId, int defaultScore)
        {
            string normalized = NormalizeAbilityId(abilityId);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return defaultScore;
            }

            if (IsPointBuyMode())
            {
                return AbilityGenerationState.PointBuyScores.TryGetValue(normalized, out int pointBuyScore)
                    ? pointBuyScore
                    : PointBuyMinScore;
            }

            if (IsManualAbilityMode())
            {
                return AbilityGenerationState.ManualScores.TryGetValue(normalized, out int manualScore)
                    ? manualScore
                    : 10;
            }

            if (AbilityGenerationState.Scores.Count == 0)
            {
                return defaultScore;
            }

            for (int index = 0; index < AbilityGenerationState.Scores.Count; index++)
            {
                CharacterCreationGeneratedAbilityScoreState score = AbilityGenerationState.Scores[index];
                if (score != null && string.Equals(score.AssignedAbilityId, normalized, StringComparison.OrdinalIgnoreCase))
                {
                    return score.Value;
                }
            }

            return 0;
        }

        private static void CopyAbilityBonuses(Dictionary<string, int> target, Dictionary<string, int> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            foreach (KeyValuePair<string, int> pair in source)
            {
                string normalized = NormalizeAbilityId(pair.Key);
                if (!string.IsNullOrWhiteSpace(normalized) && pair.Value != 0)
                {
                    target[normalized] = pair.Value;
                }
            }
        }

        private static void CopyAbilityOptions(Dictionary<string, string> target, Dictionary<string, string> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            foreach (KeyValuePair<string, string> pair in source)
            {
                string normalized = NormalizeAbilityId(pair.Key);
                if (!string.IsNullOrWhiteSpace(normalized) && !string.IsNullOrWhiteSpace(pair.Value))
                {
                    target[normalized] = pair.Value.Trim();
                }
            }
        }

        private static string NormalizeAbilityId(string value)
        {
            string normalized = value?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return string.Empty;
            }

            switch (normalized.ToLowerInvariant())
            {
                case "str":
                case "strength":
                case "力量":
                    return "Strength";
                case "dex":
                case "dexterity":
                case "敏捷":
                    return "Dexterity";
                case "con":
                case "constitution":
                case "体质":
                    return "Constitution";
                case "int":
                case "intelligence":
                case "智力":
                    return "Intelligence";
                case "wis":
                case "wisdom":
                case "感知":
                    return "Wisdom";
                case "cha":
                case "charisma":
                case "魅力":
                    return "Charisma";
                case "all":
                case "全部":
                    return "All";
                default:
                    return normalized;
            }
        }

        private void EnsureState()
        {
            if (m_state == null)
            {
                BeginNewDraft();
            }
        }
    }
}
