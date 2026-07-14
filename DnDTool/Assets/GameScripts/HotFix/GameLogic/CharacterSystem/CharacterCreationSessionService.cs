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
        private const int MaxDiceRollHistoryCount = 20;

        private static readonly Lazy<CharacterCreationSessionService> s_instance =
            new Lazy<CharacterCreationSessionService>(() => new CharacterCreationSessionService());

        private CharacterDraftState m_state = new CharacterDraftState();
        private CharacterCreationToolChoiceState m_activeToolChoiceState;
        private CharacterCreationFeatureChoiceState m_activeFeatureChoiceState;
        private readonly System.Random m_random = new System.Random();
        private readonly List<CharacterDiceRollHistoryEntry> m_diceRollHistory = new List<CharacterDiceRollHistoryEntry>();

        private CharacterCreationSessionService()
        {
        }

        public static CharacterCreationSessionService Instance => s_instance.Value;

        public CharacterDraftState CurrentState => m_state;

        public IReadOnlyList<CharacterDiceRollHistoryEntry> DiceRollHistory => m_diceRollHistory;

        public List<CharacterCreationSkillChoiceState> SkillChoiceStates { get; } = new List<CharacterCreationSkillChoiceState>();

        public List<CharacterCreationToolChoiceState> ToolChoiceStates { get; } = new List<CharacterCreationToolChoiceState>();

        public List<CharacterCreationMixedProficiencyChoiceState> MixedProficiencyChoiceStates { get; } = new List<CharacterCreationMixedProficiencyChoiceState>();

        public List<CharacterCreationFeatureChoiceState> FeatureChoiceStates { get; } = new List<CharacterCreationFeatureChoiceState>();

        public CharacterCreationToolChoiceState ActiveToolChoiceState => m_activeToolChoiceState;

        public CharacterCreationFeatureChoiceState ActiveFeatureChoiceState => m_activeFeatureChoiceState;

        public CharacterCreationRaceAbilityChoiceState RaceAbilityChoiceState { get; } = new CharacterCreationRaceAbilityChoiceState();

        public CharacterCreationAbilityGenerationState AbilityGenerationState { get; } = new CharacterCreationAbilityGenerationState();

        public CharacterCreationSpellSelectionState SpellSelectionState { get; } = new CharacterCreationSpellSelectionState();

        public void BeginNewDraft()
        {
            m_state = new CharacterDraftState
            {
                Mode = CharacterWorkflowMode.Create,
                Character = CharacterCardLocalRepository.Normalize(new CharacterCardDraftSaveData()),
                IsDirty = false
            };
            LoadDiceRollHistoryFromCharacter(m_state.Character);
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
            m_state.Character.PreviewImagePath = input.PreviewImagePath?.Trim() ?? string.Empty;
            m_state.Character.Spellcasting = CharacterSpellcastingSaveData.Clone(input.Spellcasting);
            m_state.Character.CustomFeatures = CharacterCustomFeatureSaveData.CloneList(input.CustomFeatures);
            m_state.Character.DiceRollHistory = CharacterDiceRollHistorySaveData.CloneList(input.DiceRollHistory);
            m_state.Character.ManualOverrides = CharacterManualOverrideSaveData.Clone(input.ManualOverrides);
            LoadDiceRollHistoryFromCharacter(m_state.Character);
            m_state.Character.Level = Math.Max(1, input.Level);
            m_state.Character.Equipment = CharacterEquipmentSetSaveData.Clone(input.Equipment);
            m_state.Character.RoleplayProfile = new CharacterRoleplayProfileSaveData
            {
                PersonalityTraits = input.PersonalityTraits?.Trim() ?? string.Empty,
                Ideals = input.Ideals?.Trim() ?? string.Empty,
                Bonds = input.Bonds?.Trim() ?? string.Empty,
                Flaws = input.Flaws?.Trim() ?? string.Empty
            };
            m_state.IsDirty = true;
        }

        public bool AddCustomFeature(string featureName, string description)
        {
            EnsureState();
            string normalizedName = featureName?.Trim() ?? string.Empty;
            string normalizedDescription = description?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedName) && string.IsNullOrWhiteSpace(normalizedDescription))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                normalizedName = "自定义特性";
            }

            m_state.Character.CustomFeatures ??= new List<CharacterCustomFeatureSaveData>();
            m_state.Character.CustomFeatures.Add(new CharacterCustomFeatureSaveData
            {
                Name = normalizedName,
                Description = normalizedDescription
            });
            m_state.IsDirty = true;
            return true;
        }

        public void SetPreviewImagePath(string previewImagePath)
        {
            EnsureState();
            m_state.Character.PreviewImagePath = previewImagePath?.Trim() ?? string.Empty;
            m_state.IsDirty = true;
        }

        public CharacterInventoryOperationResult AddInventoryItem(CharacterEquipmentItemSaveData item, int quantity = 1)
        {
            EnsureState();
            if (CharacterItemCategoryUtility.IsCurrencyItem(item))
            {
                m_state.Character.Currency ??= new CharacterCurrencySaveData();
                int amount = CharacterItemCategoryUtility.AddCurrency(m_state.Character.Currency, item, quantity);
                if (amount <= 0)
                {
                    return new CharacterInventoryOperationResult
                    {
                        Success = false,
                        Message = "Currency item data is invalid.",
                        Equipment = CharacterEquipmentSetSaveData.Clone(m_state.Character?.Equipment)
                    };
                }

                m_state.IsDirty = true;
                return new CharacterInventoryOperationResult
                {
                    Success = true,
                    Message = "Currency added.",
                    Equipment = CharacterEquipmentSetSaveData.Clone(m_state.Character.Equipment)
                };
            }

            return ApplyInventoryOperation(equipment => CharacterInventoryApplicationService.Instance.AddItem(equipment, item, quantity));
        }

        public CharacterInventoryOperationResult AddRuleInventoryItem(string sourceItemId, int quantity = 1)
        {
            if (!string.IsNullOrWhiteSpace(sourceItemId)
                && DndRuleContentService.Instance.TryGetItem(sourceItemId.Trim(), out DndItemDefineData ruleItem)
                && ruleItem != null)
            {
                CharacterEquipmentItemSaveData item = new CharacterEquipmentItemSaveData
                {
                    ItemSourceType = CharacterItemSourceTypes.RuleTable,
                    SourceItemId = ruleItem.ItemId?.Trim() ?? string.Empty,
                    ItemId = ruleItem.ItemId?.Trim() ?? string.Empty,
                    ItemName = ruleItem.Name?.Trim() ?? string.Empty,
                    ItemType = ruleItem.ItemType?.Trim() ?? string.Empty,
                    Quantity = Math.Max(1, quantity > 0 ? quantity : ruleItem.DefaultQuantity),
                    Consumable = ruleItem.Consumable
                };
                if (CharacterItemCategoryUtility.IsCurrencyItem(item))
                {
                    return AddInventoryItem(item, item.Quantity);
                }
            }

            return ApplyInventoryOperation(equipment => CharacterInventoryApplicationService.Instance.AddRuleItem(equipment, sourceItemId, quantity));
        }

        public CharacterInventoryOperationResult RemoveInventoryItem(string itemInstanceId, int quantity = 1)
        {
            return ApplyInventoryOperation(equipment => CharacterInventoryApplicationService.Instance.RemoveItem(equipment, itemInstanceId, quantity));
        }

        public CharacterInventoryOperationResult DeleteInventoryItem(string itemInstanceId)
        {
            return ApplyInventoryOperation(equipment => CharacterInventoryApplicationService.Instance.DeleteItem(equipment, itemInstanceId));
        }

        public CharacterInventoryOperationResult EquipInventoryItem(string itemInstanceId)
        {
            return ApplyInventoryOperation(equipment => CharacterInventoryApplicationService.Instance.EquipItem(equipment, itemInstanceId));
        }

        public CharacterInventoryOperationResult UnequipInventoryItem(string itemInstanceId)
        {
            return ApplyInventoryOperation(equipment => CharacterInventoryApplicationService.Instance.UnequipItem(equipment, itemInstanceId));
        }

        public CharacterInventoryOperationResult AttuneInventoryItem(string itemInstanceId)
        {
            return ApplyInventoryOperation(equipment => CharacterInventoryApplicationService.Instance.AttuneItem(equipment, itemInstanceId));
        }

        public CharacterInventoryOperationResult UnattuneInventoryItem(string itemInstanceId)
        {
            return ApplyInventoryOperation(equipment => CharacterInventoryApplicationService.Instance.UnattuneItem(equipment, itemInstanceId));
        }

        public CharacterInventoryOperationResult UseInventoryItem(string itemInstanceId, int consumeCount = 1)
        {
            return ApplyInventoryOperation(equipment => CharacterInventoryApplicationService.Instance.UseItem(equipment, itemInstanceId, consumeCount));
        }

        public bool HealCurrentHp(int healAmount, int finalMaxHp)
        {
            EnsureState();
            CharacterCardDraftSaveData character = m_state.Character;
            if (character == null || healAmount <= 0)
            {
                return false;
            }

            int maxHp = Math.Max(0, finalMaxHp);
            if (maxHp <= 0)
            {
                maxHp = Math.Max(0, character.MaxHp);
            }

            if (maxHp <= 0)
            {
                return false;
            }

            int currentHp = CharacterCreationCalculationService.Instance.NormalizeCurrentHp(character.CurrentHp, maxHp);
            character.CurrentHp = Math.Min(maxHp, currentHp + healAmount);
            m_state.IsDirty = true;
            return true;
        }

        public bool ChangeCurrentHp(int delta, int finalMaxHp)
        {
            EnsureState();
            CharacterCardDraftSaveData character = m_state.Character;
            if (character == null || delta == 0)
            {
                return false;
            }

            int maxHp = Math.Max(0, finalMaxHp);
            if (maxHp <= 0)
            {
                maxHp = Math.Max(0, character.MaxHp);
            }

            if (maxHp <= 0)
            {
                return false;
            }

            int currentHp = CharacterCreationCalculationService.Instance.NormalizeCurrentHp(character.CurrentHp, maxHp);
            int nextHp = Math.Max(0, Math.Min(maxHp, currentHp + delta));
            if (nextHp == currentHp)
            {
                return false;
            }

            character.CurrentHp = nextHp;
            m_state.IsDirty = true;
            return true;
        }

        public CharacterDiceRollHistoryEntry AddDiceRollHistoryEntry(
            CharacterInventoryQuickRollContext context,
            CharacterDiceRollResultData result,
            string purpose)
        {
            CharacterDiceRollHistoryEntry entry = new CharacterDiceRollHistoryEntry
            {
                EntryId = Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.Now,
                SourceItemInstanceId = context?.ItemInstanceId ?? string.Empty,
                SourceItemName = context?.ItemName ?? string.Empty,
                SourceEffectName = context?.EffectName ?? string.Empty,
                DiceExpression = context?.DiceExpression ?? result?.Expression ?? string.Empty,
                Purpose = purpose?.Trim() ?? string.Empty,
                Summary = result?.Summary ?? string.Empty,
                Total = result?.Total ?? 0,
                Success = result != null && result.Success,
                Error = result?.Error ?? string.Empty
            };

            m_diceRollHistory.Insert(0, entry);
            while (m_diceRollHistory.Count > MaxDiceRollHistoryCount)
            {
                m_diceRollHistory.RemoveAt(m_diceRollHistory.Count - 1);
            }

            SyncDiceRollHistoryToCharacter(true);
            return entry;
        }

        public void UpdateDiceRollHistoryPurpose(string entryId, string purpose)
        {
            CharacterDiceRollHistoryEntry entry = FindDiceRollHistoryEntry(entryId);
            if (entry != null)
            {
                entry.Purpose = purpose?.Trim() ?? string.Empty;
                SyncDiceRollHistoryToCharacter(true);
            }
        }

        public void MarkDiceRollHistoryApplied(string entryId, string appliedMessage)
        {
            CharacterDiceRollHistoryEntry entry = FindDiceRollHistoryEntry(entryId);
            if (entry != null)
            {
                entry.Applied = true;
                entry.AppliedMessage = appliedMessage?.Trim() ?? string.Empty;
                SyncDiceRollHistoryToCharacter(true);
            }
        }

        private void LoadDiceRollHistoryFromCharacter(CharacterCardDraftSaveData character)
        {
            m_diceRollHistory.Clear();
            if (character?.DiceRollHistory == null)
            {
                return;
            }

            for (int index = 0; index < character.DiceRollHistory.Count && m_diceRollHistory.Count < MaxDiceRollHistoryCount; index++)
            {
                CharacterDiceRollHistoryEntry entry = CharacterDiceRollHistoryFormatter.FromSaveData(character.DiceRollHistory[index]);
                if (entry != null)
                {
                    m_diceRollHistory.Add(entry);
                }
            }
        }

        private void SyncDiceRollHistoryToCharacter(bool markDirty)
        {
            EnsureState();
            if (m_state.Character == null)
            {
                return;
            }

            List<CharacterDiceRollHistorySaveData> saveData = new List<CharacterDiceRollHistorySaveData>();
            for (int index = 0; index < m_diceRollHistory.Count && saveData.Count < MaxDiceRollHistoryCount; index++)
            {
                CharacterDiceRollHistorySaveData entry = CharacterDiceRollHistoryFormatter.ToSaveData(m_diceRollHistory[index]);
                if (entry != null)
                {
                    saveData.Add(entry);
                }
            }

            m_state.Character.DiceRollHistory = CharacterDiceRollHistorySaveData.CloneList(saveData, MaxDiceRollHistoryCount);
            if (markDirty)
            {
                m_state.IsDirty = true;
            }
        }

        private CharacterDiceRollHistoryEntry FindDiceRollHistoryEntry(string entryId)
        {
            if (string.IsNullOrWhiteSpace(entryId))
            {
                return null;
            }

            string normalized = entryId.Trim();
            for (int index = 0; index < m_diceRollHistory.Count; index++)
            {
                CharacterDiceRollHistoryEntry entry = m_diceRollHistory[index];
                if (entry != null && string.Equals(entry.EntryId, normalized, StringComparison.OrdinalIgnoreCase))
                {
                    return entry;
                }
            }

            return null;
        }

        public CharacterCreationToolChoiceState FindToolChoiceState(string choiceGroupId)
        {
            return FindChoiceState(ToolChoiceStates, choiceGroupId);
        }

        public CharacterCreationMixedProficiencyChoiceState FindMixedProficiencyChoiceState(string choiceGroupId)
        {
            return FindChoiceState(MixedProficiencyChoiceStates, choiceGroupId);
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

        public void SetPendingSpellSelection(string spellId, int filterLevel)
        {
            EnsureState();
            SpellSelectionState.PendingSpellId = spellId?.Trim() ?? string.Empty;
            SpellSelectionState.FilterLevel = filterLevel;
        }

        public void ClearPendingSpellSelection()
        {
            SpellSelectionState.PendingSpellId = string.Empty;
            SpellSelectionState.FilterLevel = -1;
        }

        public bool ConfirmPendingSpellSelection()
        {
            EnsureState();
            string spellId = SpellSelectionState.PendingSpellId?.Trim() ?? string.Empty;
            int level = Math.Max(1, m_state?.Character?.Level ?? 1);
            CharacterCreationSpellbookViewState spellbook = CharacterCreationSpellDisplayService.Instance.BuildSpellbook(m_state.Character, level, -1);
            if (string.IsNullOrWhiteSpace(spellId)
                || !DndRuleContentService.Instance.TryGetSpell(spellId, out DndSpellDefineData spell)
                || CharacterCreationSpellDisplayService.Instance.IsSpellKnown(m_state.Character, spellId)
                || !CanLearnSpell(spell, spellbook))
            {
                return false;
            }

            bool shouldPrepare = spell.Level > 0
                && spellbook.MaxKnownSpells <= 0
                && spellbook.MaxPreparedSpells > 0;
            if (m_state.Character.Spellcasting == null)
            {
                m_state.Character.Spellcasting = new CharacterSpellcastingSaveData();
            }

            m_state.Character.Spellcasting.HasSpellcasting = true;
            m_state.Character.Spellcasting.Spells.Add(new CharacterKnownSpellSaveData
            {
                SpellId = spell.SpellId,
                SourceClassId = m_state.Character.ClassId?.Trim() ?? string.Empty,
                SpellLevel = Math.Max(0, spell.Level),
                IsCantrip = spell.Level <= 0,
                IsKnown = true,
                IsPrepared = shouldPrepare,
                IsAlwaysPrepared = false,
                IsRitual = spell.Ritual
            });
            SpellSelectionState.PendingSpellId = string.Empty;
            m_state.IsDirty = true;
            return true;
        }

        private static bool CanLearnSpell(DndSpellDefineData spell, CharacterCreationSpellbookViewState spellbook)
        {
            if (spell == null || spellbook == null)
            {
                return false;
            }

            if (spell.Level > spellbook.MaxSpellLevel)
            {
                return false;
            }

            if (spell.Level <= 0)
            {
                return spellbook.MaxKnownCantrips > 0 && spellbook.KnownCantrips < spellbook.MaxKnownCantrips;
            }

            if (spellbook.MaxKnownSpells > 0)
            {
                return spellbook.KnownSpells < spellbook.MaxKnownSpells;
            }

            return spellbook.MaxPreparedSpells > 0 && spellbook.PreparedSpells < spellbook.MaxPreparedSpells;
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
            string normalized = NormalizeAbilityId(abilityId);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return false;
            }

            if (IsManualAbilityMode())
            {
                AbilityGenerationState.ManualScores[normalized] = Math.Max(ManualMinScore, Math.Min(ManualMaxScore, score));
            }

            return SetManualNumericOverride(normalized, score);
        }

        public bool CanManualInputAbilityScore(string abilityId)
        {
            return !string.IsNullOrWhiteSpace(NormalizeAbilityId(abilityId));
        }

        public bool SetManualNumericOverride(string fieldId, int value)
        {
            EnsureState();
            CharacterManualOverrideSaveData overrides = EnsureManualOverrides();
            if (!overrides.TrySetValue(fieldId, value))
            {
                return false;
            }

            m_state.IsDirty = true;
            if (IsAbilityOverrideField(fieldId))
            {
                RefreshDerivedStatsAfterAbilityScoresChanged();
            }

            return true;
        }

        public bool TryGetManualNumericOverride(string fieldId, out int value)
        {
            value = 0;
            CharacterManualOverrideSaveData overrides = m_state?.Character?.ManualOverrides;
            return overrides != null && overrides.TryGetValue(fieldId, out value);
        }

        public bool ClearManualOverrides()
        {
            EnsureState();
            CharacterManualOverrideSaveData overrides = EnsureManualOverrides();
            if (!overrides.HasAny())
            {
                return false;
            }

            overrides.Clear();
            m_state.IsDirty = true;
            RefreshDerivedStatsAfterAbilityScoresChanged();
            return true;
        }

        public bool IsActiveAbilityScoreFeatureChoice()
        {
            return IsRepeatableAbilityScoreChoice(m_activeFeatureChoiceState);
        }

        public bool IsAbilityScoreFeatureChoice(CharacterCreationFeatureChoiceState state)
        {
            return IsRepeatableAbilityScoreChoice(state);
        }

        public bool IsFeatFeatureChoice(CharacterCreationFeatureChoiceState state)
        {
            return state != null
                && DndRuleContentService.Instance.TryGetChoiceGroup(state.ChoiceGroupId, out DndChoiceGroupData choiceGroup)
                && IsFeatChoiceGroup(choiceGroup);
        }

        public bool TogglePendingToolChoice(string toolId)
        {
            CharacterCreationToolChoiceState state = m_activeToolChoiceState;
            if (state == null || string.IsNullOrWhiteSpace(toolId))
            {
                return false;
            }

            if (state is CharacterCreationMixedToolChoiceState mixedToolState)
            {
                CharacterCreationMixedProficiencyChoiceState mixedState = mixedToolState.MixedState;
                if (mixedState == null)
                {
                    return false;
                }

                TogglePendingMixedToolChoice(mixedState, toolId);
                m_state.IsDirty = true;
                return true;
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

        public bool SelectFeatFeatureChoice(string optionId)
        {
            return SelectFeatFeatureChoice(m_activeFeatureChoiceState, optionId);
        }

        public bool SelectFeatFeatureChoice(CharacterCreationFeatureChoiceState state, string optionId)
        {
            if (!IsFeatFeatureChoice(state) || string.IsNullOrWhiteSpace(optionId))
            {
                return false;
            }

            state.PendingOptionIds.Clear();
            AppendUniqueExactValue(state.PendingOptionIds, optionId);
            state.IsConfirmed = false;
            RemoveFollowupFeatureChoicesForSource("Feat", state);
            RemoveToolChoiceStatesBySource("Feat");
            m_state.IsDirty = true;
            RefreshDerivedStatsAfterAbilityScoresChanged();
            return true;
        }

        public bool ConfirmActiveToolChoice()
        {
            CharacterCreationToolChoiceState state = m_activeToolChoiceState;
            if (state == null)
            {
                return false;
            }

            if (state is CharacterCreationMixedToolChoiceState mixedToolState)
            {
                CharacterCreationMixedProficiencyChoiceState mixedState = mixedToolState.MixedState;
                if (mixedState == null)
                {
                    return false;
                }

                AppendUniqueValues(mixedState.SelectedToolIds, mixedState.PendingToolIds);
                mixedState.PendingToolIds.Clear();
                ValidateMixedToolSelections(mixedState);
                SyncMixedToolChoiceState(mixedToolState);
                m_state.IsDirty = true;
                return true;
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

            state.IsConfirmed = true;
            m_state.IsDirty = true;
            RefreshDerivedStatsAfterAbilityScoresChanged();
            return true;
        }

        public CharacterCreationFeatureChoiceState StartFollowupFeatureChoice(CharacterCreationFeatureChoiceState completedState)
        {
            if (completedState == null
                || GetCommittedOrPendingOptionIds(completedState).Count == 0
                || !DndRuleContentService.Instance.TryGetChoiceGroup(completedState.ChoiceGroupId, out DndChoiceGroupData parentGroup)
                || parentGroup == null)
            {
                ClearActiveFeatureChoiceIfMatches(completedState);
                return null;
            }

            CharacterCreationFeatureChoiceState followupState = null;
            if (IsAdvancementOptionChoiceType(parentGroup.ChoiceType))
            {
                IReadOnlyList<string> optionIds = GetCommittedOrPendingOptionIds(completedState);
                string followupChoiceGroupId = ResolveAdvancementFollowupChoiceGroupId(parentGroup, optionIds[0]);
                followupState = StartFirstIncompleteFeatureChoiceGroup(
                    new[] { followupChoiceGroupId },
                    completedState.SourceType,
                    completedState.SourceId,
                    completedState.Level,
                    completedState.ClassId);
            }
            else if (TryResolveSelectedFeat(parentGroup, completedState, out DndFeatDefineData selectedFeat))
            {
                followupState = StartFirstIncompleteFeatureChoiceGroup(
                    BuildFeatChoiceGroupIds(selectedFeat),
                    "Feat",
                    selectedFeat.FeatId,
                    completedState.Level,
                    completedState.ClassId);
            }
            else if (TryResolveSourceFeat(completedState, out DndFeatDefineData sourceFeat))
            {
                followupState = StartFirstIncompleteFeatureChoiceGroup(
                    BuildFeatChoiceGroupIds(sourceFeat),
                    "Feat",
                    sourceFeat.FeatId,
                    completedState.Level,
                    completedState.ClassId);
            }

            if (followupState == null)
            {
                ClearActiveFeatureChoiceIfMatches(completedState);
            }

            return followupState;
        }

        public CharacterCreationFeatureChoiceState ResumeIncompleteFollowupFeatureChoice(CharacterCreationFeatureChoiceState completedState)
        {
            if (completedState == null
                || GetCommittedOrPendingOptionIds(completedState).Count == 0
                || !DndRuleContentService.Instance.TryGetChoiceGroup(completedState.ChoiceGroupId, out DndChoiceGroupData parentGroup)
                || parentGroup == null)
            {
                return null;
            }

            if (IsAdvancementOptionChoiceType(parentGroup.ChoiceType))
            {
                IReadOnlyList<string> optionIds = GetCommittedOrPendingOptionIds(completedState);
                string followupChoiceGroupId = ResolveAdvancementFollowupChoiceGroupId(parentGroup, optionIds[0]);
                return ResumeFirstIncompleteFeatureChoiceGroup(
                    new[] { followupChoiceGroupId },
                    completedState.SourceType,
                    completedState.SourceId,
                    completedState.Level,
                    completedState.ClassId);
            }

            if (TryResolveSelectedFeat(parentGroup, completedState, out DndFeatDefineData selectedFeat))
            {
                return ResumeFirstIncompleteFeatureChoiceGroup(
                    BuildFeatChoiceGroupIds(selectedFeat),
                    "Feat",
                    selectedFeat.FeatId,
                    completedState.Level,
                    completedState.ClassId);
            }

            if (TryResolveSourceFeat(completedState, out DndFeatDefineData sourceFeat))
            {
                return ResumeFirstIncompleteFeatureChoiceGroup(
                    BuildFeatChoiceGroupIds(sourceFeat),
                    "Feat",
                    sourceFeat.FeatId,
                    completedState.Level,
                    completedState.ClassId);
            }

            return null;
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

        public CharacterCreationToolChoiceState CreateOrRefreshMixedToolChoiceState(string choiceGroupId, string label)
        {
            CharacterCreationMixedProficiencyChoiceState mixedState = FindMixedProficiencyChoiceState(choiceGroupId);
            if (mixedState == null || mixedState.OptionToolIds.Count == 0)
            {
                return null;
            }

            ValidateMixedToolSelections(mixedState);
            CharacterCreationMixedToolChoiceState state = FindMixedToolChoiceState(choiceGroupId);
            if (state == null)
            {
                state = new CharacterCreationMixedToolChoiceState
                {
                    ChoiceGroupId = mixedState.ChoiceGroupId
                };
                ToolChoiceStates.Add(state);
            }

            state.MixedState = mixedState;
            state.Label = FirstNonEmpty(label, mixedState.Label);
            state.SourceType = mixedState.SourceType ?? string.Empty;
            state.SourceId = mixedState.SourceId ?? string.Empty;
            state.MaxSelect = GetMixedToolRemainingSelect(mixedState);
            state.OptionToolIds.Clear();
            state.PendingToolIds.Clear();
            state.SelectedToolIds.Clear();
            state.OptionIdByToolId.Clear();

            AppendUniqueExactValues(state.OptionToolIds, mixedState.OptionToolIds);
            AppendUniqueExactValues(state.PendingToolIds, mixedState.PendingToolIds);
            AppendUniqueExactValues(state.SelectedToolIds, mixedState.SelectedToolIds);
            foreach (KeyValuePair<string, string> pair in mixedState.OptionIdByToolId)
            {
                state.OptionIdByToolId[pair.Key] = pair.Value;
            }

            return ValidateToolChoiceState(state);
        }

        public void RebuildSkillChoiceStates(IReadOnlyList<CharacterCreationSkillChoiceSource> sources, IReadOnlyList<string> fixedSkillIds)
        {
            List<CharacterCreationSkillChoiceState> previousStates = new List<CharacterCreationSkillChoiceState>(SkillChoiceStates);
            List<CharacterCreationMixedProficiencyChoiceState> previousMixedStates = new List<CharacterCreationMixedProficiencyChoiceState>(MixedProficiencyChoiceStates);
            SkillChoiceStates.Clear();
            MixedProficiencyChoiceStates.Clear();
            RemoveMixedToolChoiceStates();

            if (sources == null)
            {
                return;
            }

            for (int sourceIndex = 0; sourceIndex < sources.Count; sourceIndex++)
            {
                CharacterCreationSkillChoiceSource source = sources[sourceIndex];
                AppendSkillChoiceStates(previousStates, source.ChoiceGroupIds, source.SourceType, source.SourceId, fixedSkillIds);
                AppendMixedProficiencyChoiceStates(previousMixedStates, source.ChoiceGroupIds, source.SourceType, source.SourceId, fixedSkillIds);
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

            AppendSelectedFeatSkillChoiceSources(sources);
            RebuildSkillChoiceStates(sources, CharacterCreationRuleService.Instance.BuildFixedSkillProficiencyIds(raceData, backgroundData));
        }

        private void AppendSelectedFeatSkillChoiceSources(List<CharacterCreationSkillChoiceSource> sources)
        {
            if (sources == null)
            {
                return;
            }

            for (int stateIndex = 0; stateIndex < FeatureChoiceStates.Count; stateIndex++)
            {
                CharacterCreationFeatureChoiceState state = FeatureChoiceStates[stateIndex];
                if (!TryGetSelectedFeat(state, out DndFeatDefineData feat))
                {
                    continue;
                }

                sources.Add(new CharacterCreationSkillChoiceSource
                {
                    SourceType = "Feat",
                    SourceId = feat.FeatId,
                    ChoiceGroupIds = BuildFeatChoiceGroupIds(feat)
                });
            }
        }

        public bool TrySelectSkill(string skillId, IReadOnlyList<string> currentProficiencyIds)
        {
            if (string.IsNullOrWhiteSpace(skillId))
            {
                return false;
            }

            for (int stateIndex = 0; stateIndex < MixedProficiencyChoiceStates.Count; stateIndex++)
            {
                CharacterCreationMixedProficiencyChoiceState state = MixedProficiencyChoiceStates[stateIndex];
                if (state != null && RemoveNormalizedSkillId(state.SelectedSkillIds, skillId))
                {
                    ValidateMixedToolSelections(state);
                    m_state.IsDirty = true;
                    return true;
                }
            }

            for (int stateIndex = 0; stateIndex < SkillChoiceStates.Count; stateIndex++)
            {
                CharacterCreationSkillChoiceState state = SkillChoiceStates[stateIndex];
                if (state != null && RemoveNormalizedSkillId(state.SelectedSkillIds, skillId))
                {
                    m_state.IsDirty = true;
                    return true;
                }
            }

            if (ContainsNormalizedSkillId(currentProficiencyIds, skillId))
            {
                return false;
            }

            for (int stateIndex = 0; stateIndex < MixedProficiencyChoiceStates.Count; stateIndex++)
            {
                CharacterCreationMixedProficiencyChoiceState state = MixedProficiencyChoiceStates[stateIndex];
                if (state == null
                    || state.PendingCount >= state.MaxSelect
                    || !ContainsNormalizedSkillId(state.CandidateSkillIds, skillId)
                    || ContainsNormalizedSkillId(state.SelectedSkillIds, skillId))
                {
                    continue;
                }

                AppendUniqueNormalizedSkillId(state.SelectedSkillIds, skillId);
                m_state.IsDirty = true;
                return true;
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

            for (int index = 0; index < MixedProficiencyChoiceStates.Count; index++)
            {
                CharacterCreationMixedProficiencyChoiceState state = MixedProficiencyChoiceStates[index];
                if (state != null
                    && state.PendingCount < state.MaxSelect
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

            for (int index = 0; index < MixedProficiencyChoiceStates.Count; index++)
            {
                CharacterCreationMixedProficiencyChoiceState state = MixedProficiencyChoiceStates[index];
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

            for (int stateIndex = 0; stateIndex < MixedProficiencyChoiceStates.Count; stateIndex++)
            {
                CharacterCreationMixedProficiencyChoiceState state = MixedProficiencyChoiceStates[stateIndex];
                if (state == null || string.IsNullOrWhiteSpace(state.ChoiceGroupId))
                {
                    continue;
                }

                for (int skillIndex = 0; skillIndex < state.SelectedSkillIds.Count; skillIndex++)
                {
                    string skillId = NormalizeSkillId(state.SelectedSkillIds[skillIndex]);
                    if (string.IsNullOrWhiteSpace(skillId))
                    {
                        continue;
                    }

                    string optionId = state.OptionIdBySkillId.TryGetValue(skillId, out string mappedOptionId)
                        ? mappedOptionId
                        : $"skill:{skillId}";

                    result.Add(new CharacterCreationSkillChoiceInput
                    {
                        ChoiceGroupId = state.ChoiceGroupId,
                        SkillId = optionId,
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
                if (state == null || state is CharacterCreationMixedToolChoiceState || string.IsNullOrWhiteSpace(state.ChoiceGroupId))
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

            for (int stateIndex = 0; stateIndex < MixedProficiencyChoiceStates.Count; stateIndex++)
            {
                CharacterCreationMixedProficiencyChoiceState state = MixedProficiencyChoiceStates[stateIndex];
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
                        : $"tool:{normalizedToolId}";

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
                if (state == null || state is CharacterCreationMixedToolChoiceState)
                {
                    continue;
                }

                for (int toolIndex = 0; toolIndex < state.SelectedToolIds.Count; toolIndex++)
                {
                    AppendUniqueExactValue(result, state.SelectedToolIds[toolIndex]);
                }
            }

            for (int stateIndex = 0; stateIndex < MixedProficiencyChoiceStates.Count; stateIndex++)
            {
                CharacterCreationMixedProficiencyChoiceState state = MixedProficiencyChoiceStates[stateIndex];
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
            return BuildFeatureChoiceInputs(false);
        }

        public List<CharacterCreationFeatureChoiceInput> BuildPreviewFeatureChoiceInputs()
        {
            return BuildFeatureChoiceInputs(true);
        }

        private List<CharacterCreationFeatureChoiceInput> BuildFeatureChoiceInputs(bool includeActivePending)
        {
            List<CharacterCreationFeatureChoiceInput> result = new List<CharacterCreationFeatureChoiceInput>();

            for (int stateIndex = 0; stateIndex < FeatureChoiceStates.Count; stateIndex++)
            {
                CharacterCreationFeatureChoiceState state = FeatureChoiceStates[stateIndex];
                if (state == null || string.IsNullOrWhiteSpace(state.ChoiceGroupId))
                {
                    continue;
                }

                IReadOnlyList<string> optionIds = includeActivePending
                    && (ReferenceEquals(state, m_activeFeatureChoiceState) || !state.IsConfirmed)
                    && state.PendingOptionIds.Count > 0
                    ? state.PendingOptionIds
                    : state.SelectedOptionIds;

                for (int optionIndex = 0; optionIndex < optionIds.Count; optionIndex++)
                {
                    string optionId = optionIds[optionIndex];
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
                Strength = GetBaseAbilityScore("Strength", baseAbilityScore),
                Dexterity = GetBaseAbilityScore("Dexterity", baseAbilityScore),
                Constitution = GetBaseAbilityScore("Constitution", baseAbilityScore),
                Intelligence = GetBaseAbilityScore("Intelligence", baseAbilityScore),
                Wisdom = GetBaseAbilityScore("Wisdom", baseAbilityScore),
                Charisma = GetBaseAbilityScore("Charisma", baseAbilityScore),
                HpModeId = CharacterHpModeIds.Normalize(character.HpModeId),
                MaxHp = Math.Max(0, character.MaxHp),
                CurrentHp = CharacterCreationCalculationService.Instance.NormalizeCurrentHp(character.CurrentHp, character.MaxHp),
                TemporaryHp = Math.Max(0, character.TemporaryHp),
                PersonalityTraits = character.RoleplayProfile?.PersonalityTraits?.Trim() ?? string.Empty,
                Ideals = character.RoleplayProfile?.Ideals?.Trim() ?? string.Empty,
                Bonds = character.RoleplayProfile?.Bonds?.Trim() ?? string.Empty,
                Flaws = character.RoleplayProfile?.Flaws?.Trim() ?? string.Empty,
                HpRolls = CloneHpRolls(character.HpRolls),
                SkillProficiencyIds = BuildCurrentSkillProficiencyIds(form.FixedSkillProficiencyIds),
                ToolProficiencyIds = BuildCurrentToolProficiencyIds(form.FixedToolProficiencyIds),
                RaceAbilityChoices = BuildRaceAbilityChoiceInputs(),
                SkillChoices = BuildSkillChoiceInputs(),
                ToolChoices = BuildToolChoiceInputs(classId),
                FeatureChoices = BuildFeatureChoiceInputs(),
                PreviewImagePath = character.PreviewImagePath?.Trim() ?? string.Empty,
                Equipment = CharacterEquipmentSetSaveData.Clone(character.Equipment),
                Spellcasting = CharacterSpellcastingSaveData.Clone(character.Spellcasting),
                CustomFeatures = CharacterCustomFeatureSaveData.CloneList(character.CustomFeatures),
                DiceRollHistory = CharacterDiceRollHistorySaveData.CloneList(character.DiceRollHistory),
                ManualOverrides = CharacterManualOverrideSaveData.Clone(character.ManualOverrides)
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
                    || !state.IsConfirmed
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

        private void AppendMixedProficiencyChoiceStates(
            IReadOnlyList<CharacterCreationMixedProficiencyChoiceState> previousStates,
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
                    || !string.Equals(choiceGroup.ChoiceType, "SkillOrTool", StringComparison.OrdinalIgnoreCase)
                    || FindMixedProficiencyChoiceState(choiceGroup.ChoiceGroupId) != null)
                {
                    continue;
                }

                CharacterCreationMixedProficiencyChoiceState state = new CharacterCreationMixedProficiencyChoiceState
                {
                    ChoiceGroupId = choiceGroup.ChoiceGroupId,
                    Label = FirstNonEmpty(choiceGroup.Name, choiceGroup.ChoiceGroupId),
                    SourceType = sourceType ?? string.Empty,
                    SourceId = sourceId ?? string.Empty,
                    MaxSelect = Math.Max(choiceGroup.MinSelect, choiceGroup.MaxSelect)
                };

                IReadOnlyList<DndChoiceOptionData> options = DndRuleContentService.Instance.GetChoiceOptions(choiceGroup.ChoiceGroupId);
                for (int optionIndex = 0; optionIndex < options.Count; optionIndex++)
                {
                    DndChoiceOptionData option = options[optionIndex];
                    string optionId = option?.OptionId?.Trim() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(optionId))
                    {
                        continue;
                    }

                    if (TryResolveSkillOrToolOption(option, out string skillId, out string toolId))
                    {
                        if (!string.IsNullOrWhiteSpace(skillId))
                        {
                            AppendUniqueNormalizedSkillId(state.OptionSkillIds, skillId);
                            string normalizedSkillId = NormalizeSkillId(skillId);
                            if (!state.OptionIdBySkillId.ContainsKey(normalizedSkillId))
                            {
                                state.OptionIdBySkillId[normalizedSkillId] = optionId;
                            }

                            if (!ContainsNormalizedSkillId(fixedSkillIds, normalizedSkillId))
                            {
                                AppendUniqueNormalizedSkillId(state.CandidateSkillIds, normalizedSkillId);
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(toolId))
                        {
                            AppendUniqueExactValue(state.OptionToolIds, toolId);
                            if (!state.OptionIdByToolId.ContainsKey(toolId))
                            {
                                state.OptionIdByToolId[toolId] = optionId;
                            }
                        }
                    }
                }

                RestoreMixedProficiencyChoiceSelections(previousStates, state);
                if (state.MaxSelect > 0 && (state.OptionSkillIds.Count > 0 || state.OptionToolIds.Count > 0))
                {
                    MixedProficiencyChoiceStates.Add(state);
                }
            }
        }

        private static void RestoreMixedProficiencyChoiceSelections(
            IReadOnlyList<CharacterCreationMixedProficiencyChoiceState> previousStates,
            CharacterCreationMixedProficiencyChoiceState state)
        {
            if (previousStates == null || state == null)
            {
                return;
            }

            for (int index = 0; index < previousStates.Count; index++)
            {
                CharacterCreationMixedProficiencyChoiceState previous = previousStates[index];
                if (previous == null || !string.Equals(previous.ChoiceGroupId, state.ChoiceGroupId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                for (int skillIndex = 0; skillIndex < previous.SelectedSkillIds.Count; skillIndex++)
                {
                    string skillId = previous.SelectedSkillIds[skillIndex];
                    if (ContainsNormalizedSkillId(state.OptionSkillIds, skillId) && state.SelectedCount < state.MaxSelect)
                    {
                        AppendUniqueNormalizedSkillId(state.SelectedSkillIds, skillId);
                    }
                }

                for (int toolIndex = 0; toolIndex < previous.SelectedToolIds.Count; toolIndex++)
                {
                    string toolId = previous.SelectedToolIds[toolIndex];
                    if (ContainsExactValue(state.OptionToolIds, toolId) && state.SelectedCount < state.MaxSelect)
                    {
                        AppendUniqueExactValue(state.SelectedToolIds, toolId);
                    }
                }

                for (int toolIndex = 0; toolIndex < previous.PendingToolIds.Count; toolIndex++)
                {
                    string toolId = previous.PendingToolIds[toolIndex];
                    if (ContainsExactValue(state.OptionToolIds, toolId) && state.PendingCount < state.MaxSelect)
                    {
                        AppendUniqueExactValue(state.PendingToolIds, toolId);
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

                        state.IsConfirmed = previous.IsConfirmed;
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

        private CharacterCreationFeatureChoiceState StartFirstIncompleteFeatureChoiceGroup(
            IReadOnlyList<string> choiceGroupIds,
            string sourceType,
            string sourceId,
            int level,
            string classId)
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
                if (state != null)
                {
                    if (IsFeatureChoiceSelectionComplete(state) || state.OptionIds.Count == 0)
                    {
                        continue;
                    }

                    SetActiveFeatureChoice(state);
                    return state;
                }

                state = BuildFeatureChoiceState(choiceGroup, sourceType, sourceId, level);
                state.ClassId = classId?.Trim() ?? string.Empty;
                if (state.OptionIds.Count == 0)
                {
                    continue;
                }

                FeatureChoiceStates.Add(state);
                SetActiveFeatureChoice(state);
                return state;
            }

            return null;
        }

        private CharacterCreationFeatureChoiceState ResumeFirstIncompleteFeatureChoiceGroup(
            IReadOnlyList<string> choiceGroupIds,
            string sourceType,
            string sourceId,
            int level,
            string classId)
        {
            return StartFirstIncompleteFeatureChoiceGroup(choiceGroupIds, sourceType, sourceId, level, classId);
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

        private static bool IsFeatChoiceGroup(DndChoiceGroupData choiceGroup)
        {
            return choiceGroup != null
                && (string.Equals(choiceGroup.ChoiceType, "Feat", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(choiceGroup.ChoiceGroupId, "choice_feat", StringComparison.OrdinalIgnoreCase));
        }

        private static IReadOnlyList<string> GetCommittedOrPendingOptionIds(CharacterCreationFeatureChoiceState state)
        {
            if (state == null)
            {
                return Array.Empty<string>();
            }

            return state.IsConfirmed && state.SelectedOptionIds.Count > 0
                ? state.SelectedOptionIds
                : state.PendingOptionIds;
        }

        private static bool TryResolveSelectedFeat(
            DndChoiceGroupData parentGroup,
            CharacterCreationFeatureChoiceState state,
            out DndFeatDefineData feat)
        {
            feat = null;
            IReadOnlyList<string> optionIds = GetCommittedOrPendingOptionIds(state);
            if (!IsFeatChoiceGroup(parentGroup) || state == null || optionIds.Count == 0)
            {
                return false;
            }

            for (int index = 0; index < optionIds.Count; index++)
            {
                string optionId = optionIds[index]?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(optionId))
                {
                    continue;
                }

                if (DndRuleContentService.Instance.TryGetFeat(optionId, out feat))
                {
                    return true;
                }

                if (TryResolveFeatFromOption(state.ChoiceGroupId, optionId, out feat))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetSelectedFeat(CharacterCreationFeatureChoiceState state, out DndFeatDefineData feat)
        {
            feat = null;
            if (state == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(state.ChoiceGroupId)
                && DndRuleContentService.Instance.TryGetChoiceGroup(state.ChoiceGroupId, out DndChoiceGroupData choiceGroup)
                && TryResolveSelectedFeat(choiceGroup, state, out feat))
            {
                return true;
            }

            return TryResolveSourceFeat(state, out feat);
        }

        private static List<string> BuildFeatChoiceGroupIds(DndFeatDefineData feat)
        {
            List<string> choiceGroupIds = new List<string>();
            if (feat == null)
            {
                return choiceGroupIds;
            }

            AppendUniqueValues(choiceGroupIds, feat.ChoiceGroupIds);
            if (feat.FeatureIds == null)
            {
                return choiceGroupIds;
            }

            for (int index = 0; index < feat.FeatureIds.Count; index++)
            {
                string featureId = feat.FeatureIds[index];
                if (DndRuleContentService.Instance.TryGetFeature(featureId, out DndFeatureDefineData feature))
                {
                    AppendUniqueValues(choiceGroupIds, feature.ChoiceGroupIds);
                }
            }

            return choiceGroupIds;
        }

        private static bool TryResolveSourceFeat(CharacterCreationFeatureChoiceState state, out DndFeatDefineData feat)
        {
            feat = null;
            return state != null
                && string.Equals(state.SourceType, "Feat", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(state.SourceId)
                && DndRuleContentService.Instance.TryGetFeat(state.SourceId.Trim(), out feat);
        }

        private static DndChoiceOptionData FindChoiceOption(string choiceGroupId, string optionId)
        {
            if (string.IsNullOrWhiteSpace(choiceGroupId) || string.IsNullOrWhiteSpace(optionId))
            {
                return null;
            }

            string normalizedOptionId = optionId.Trim();
            IReadOnlyList<DndChoiceOptionData> options = DndRuleContentService.Instance.GetChoiceOptions(choiceGroupId.Trim());
            for (int index = 0; index < options.Count; index++)
            {
                DndChoiceOptionData option = options[index];
                if (option != null && string.Equals(option.OptionId, normalizedOptionId, StringComparison.OrdinalIgnoreCase))
                {
                    return option;
                }
            }

            return null;
        }

        private void ClearActiveFeatureChoiceIfMatches(CharacterCreationFeatureChoiceState state)
        {
            if (ReferenceEquals(m_activeFeatureChoiceState, state))
            {
                SetActiveFeatureChoice(null);
            }
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
                return "choice_feat";
            }

            return string.Empty;
        }

        private static bool IsAdvancementOptionChoiceType(string choiceType)
        {
            return string.Equals(choiceType, "AdvancementOption", StringComparison.OrdinalIgnoreCase)
                || string.Equals(choiceType, "FeatOrAbilityScore", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsFeatureChoiceSelectionComplete(CharacterCreationFeatureChoiceState state)
        {
            if (state == null || !state.IsConfirmed || state.SelectedOptionIds.Count == 0)
            {
                return false;
            }

            int requiredCount = state.MinSelect > 0 ? state.MinSelect : Math.Max(1, state.MaxSelect);
            return state.MaxSelect <= 0
                ? state.SelectedOptionIds.Count > 0
                : state.SelectedOptionIds.Count >= requiredCount;
        }

        public void RemoveToolChoiceStatesBySource(string sourceType)
        {
            RemoveChoiceStatesBySource(ToolChoiceStates, sourceType);
            RemoveChoiceStatesBySource(MixedProficiencyChoiceStates, sourceType);
        }

        public void RemoveFeatureChoiceStatesBySource(string sourceType)
        {
            RemoveChoiceStatesBySource(FeatureChoiceStates, sourceType);
        }

        private void RemoveFollowupFeatureChoicesForSource(string sourceType, CharacterCreationFeatureChoiceState stateToKeep = null)
        {
            if (string.IsNullOrWhiteSpace(sourceType))
            {
                return;
            }

            for (int index = FeatureChoiceStates.Count - 1; index >= 0; index--)
            {
                CharacterCreationFeatureChoiceState state = FeatureChoiceStates[index];
                if (state == null || ReferenceEquals(state, stateToKeep))
                {
                    continue;
                }

                if (string.Equals(state.SourceType, sourceType, StringComparison.OrdinalIgnoreCase))
                {
                    if (ReferenceEquals(state, m_activeFeatureChoiceState))
                    {
                        SetActiveFeatureChoice(null);
                    }

                    FeatureChoiceStates.RemoveAt(index);
                }
            }
        }

        public void ClearChoiceState()
        {
            SkillChoiceStates.Clear();
            ToolChoiceStates.Clear();
            MixedProficiencyChoiceStates.Clear();
            FeatureChoiceStates.Clear();
            m_diceRollHistory.Clear();
            ClearPendingSpellSelection();
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
            return GetCurrentBaseAbilityScore(normalized, baseScore)
                + GetFeatureAbilityScoreIncrease(normalized);
        }

        public int GetCurrentEffectiveAbilityScore(string abilityId, int baseScore)
        {
            string normalized = NormalizeAbilityId(abilityId);
            if (TryGetManualNumericOverride(normalized, out int overrideScore))
            {
                return overrideScore;
            }

            return GetCurrentAbilityScore(normalized, baseScore);
        }

        public int GetCurrentBaseAbilityScore(string abilityId, int baseScore)
        {
            return GetBaseAbilityScore(abilityId, baseScore);
        }

        private int GetBaseAbilityScore(string abilityId, int baseScore)
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

        private int GetFeatureAbilityScoreIncrease(string abilityId)
        {
            string normalized = NormalizeAbilityId(abilityId);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return 0;
            }

            int total = 0;
            for (int index = 0; index < FeatureChoiceStates.Count; index++)
            {
                CharacterCreationFeatureChoiceState state = FeatureChoiceStates[index];
                if (!IsRepeatableAbilityScoreChoice(state))
                {
                    continue;
                }

                IReadOnlyList<string> values = state == m_activeFeatureChoiceState
                    ? state.PendingOptionIds
                    : state.SelectedOptionIds;
                total += CountExactValues(values, normalized);
            }

            total += GetPendingFeatAbilityScoreBonus(normalized);
            return total;
        }

        private int GetPendingFeatAbilityScoreBonus(string abilityId)
        {
            int total = 0;
            for (int stateIndex = 0; stateIndex < FeatureChoiceStates.Count; stateIndex++)
            {
                CharacterCreationFeatureChoiceState state = FeatureChoiceStates[stateIndex];
                if (state == null
                    || !DndRuleContentService.Instance.TryGetChoiceGroup(state.ChoiceGroupId, out DndChoiceGroupData choiceGroup)
                    || !IsFeatChoiceGroup(choiceGroup))
                {
                    continue;
                }

                IReadOnlyList<string> optionIds = GetCommittedOrPendingOptionIds(state);
                for (int optionIndex = 0; optionIndex < optionIds.Count; optionIndex++)
                {
                    string optionId = optionIds[optionIndex];
                    if (TryResolveFeatFromOption(state.ChoiceGroupId, optionId, out DndFeatDefineData feat))
                    {
                        total += SumFeatAbilityScoreBonus(feat, abilityId);
                    }

                    DndChoiceOptionData option = FindChoiceOption(state.ChoiceGroupId, optionId);
                    total += SumAbilityScoreBonusFromEffects(option?.GrantEffectIds, abilityId);
                    total += SumAbilityScoreBonusFromFeatures(option?.GrantFeatureIds, abilityId);
                }
            }

            return total;
        }

        private static bool TryResolveFeatFromOption(string choiceGroupId, string optionId, out DndFeatDefineData feat)
        {
            feat = null;
            string normalizedOptionId = optionId?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedOptionId))
            {
                return false;
            }

            if (DndRuleContentService.Instance.TryGetFeat(normalizedOptionId, out feat))
            {
                return true;
            }

            DndChoiceOptionData option = FindChoiceOption(choiceGroupId, normalizedOptionId);
            if (option?.GrantFeatureIds == null || option.GrantFeatureIds.Count == 0)
            {
                return false;
            }

            IReadOnlyList<DndFeatDefineData> feats = DndRuleContentService.Instance.Feats;
            for (int featIndex = 0; featIndex < feats.Count; featIndex++)
            {
                DndFeatDefineData candidate = feats[featIndex];
                if (candidate?.FeatureIds == null)
                {
                    continue;
                }

                for (int featureIndex = 0; featureIndex < option.GrantFeatureIds.Count; featureIndex++)
                {
                    if (ContainsExactString(candidate.FeatureIds, option.GrantFeatureIds[featureIndex]))
                    {
                        feat = candidate;
                        return true;
                    }
                }
            }

            return false;
        }

        private static int SumFeatAbilityScoreBonus(DndFeatDefineData feat, string abilityId)
        {
            if (feat?.FeatureIds == null || string.IsNullOrWhiteSpace(abilityId))
            {
                return 0;
            }

            int total = 0;
            total += SumAbilityScoreBonusFromFeatures(feat.FeatureIds, abilityId);
            return total;
        }

        private static int SumAbilityScoreBonusFromFeatures(IReadOnlyList<string> featureIds, string abilityId)
        {
            if (featureIds == null || string.IsNullOrWhiteSpace(abilityId))
            {
                return 0;
            }

            int total = 0;
            for (int featureIndex = 0; featureIndex < featureIds.Count; featureIndex++)
            {
                if (!DndRuleContentService.Instance.TryGetFeature(featureIds[featureIndex], out DndFeatureDefineData feature)
                    || feature?.EffectIds == null)
                {
                    continue;
                }

                total += SumAbilityScoreBonusFromEffects(feature.EffectIds, abilityId);
            }

            return total;
        }

        private static int SumAbilityScoreBonusFromEffects(IReadOnlyList<string> effectIds, string abilityId)
        {
            if (effectIds == null || string.IsNullOrWhiteSpace(abilityId))
            {
                return 0;
            }

            int total = 0;
            for (int effectIndex = 0; effectIndex < effectIds.Count; effectIndex++)
            {
                if (!DndRuleContentService.Instance.TryGetFeatureEffect(effectIds[effectIndex], out DndFeatureEffectData effect)
                    || effect == null
                    || !string.Equals(effect.EffectType, "AbilityScoreBonus", StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(NormalizeAbilityId(effect.Target), abilityId, StringComparison.OrdinalIgnoreCase)
                    || !int.TryParse(effect.Value, out int value))
                {
                    continue;
                }

                total += value;
            }

            return total;
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

                if (state is CharacterCreationMixedProficiencyChoiceState mixedState
                    && string.Equals(mixedState.ChoiceGroupId, choiceGroupId, StringComparison.OrdinalIgnoreCase))
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

                if (state is CharacterCreationMixedProficiencyChoiceState mixedState
                    && string.Equals(mixedState.SourceType, sourceType, StringComparison.OrdinalIgnoreCase))
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

        private CharacterCreationMixedToolChoiceState FindMixedToolChoiceState(string choiceGroupId)
        {
            if (string.IsNullOrWhiteSpace(choiceGroupId))
            {
                return null;
            }

            for (int index = 0; index < ToolChoiceStates.Count; index++)
            {
                if (ToolChoiceStates[index] is CharacterCreationMixedToolChoiceState state
                    && string.Equals(state.ChoiceGroupId, choiceGroupId, StringComparison.OrdinalIgnoreCase))
                {
                    return state;
                }
            }

            return null;
        }

        private void RemoveMixedToolChoiceStates()
        {
            for (int index = ToolChoiceStates.Count - 1; index >= 0; index--)
            {
                if (ToolChoiceStates[index] is CharacterCreationMixedToolChoiceState)
                {
                    ToolChoiceStates.RemoveAt(index);
                }
            }
        }

        private static int GetMixedToolRemainingSelect(CharacterCreationMixedProficiencyChoiceState state)
        {
            if (state == null || state.MaxSelect <= 0)
            {
                return 0;
            }

            return Math.Max(0, state.MaxSelect - state.SelectedSkillIds.Count);
        }

        private static int GetMixedPendingToolRemainingSelect(CharacterCreationMixedProficiencyChoiceState state)
        {
            if (state == null || state.MaxSelect <= 0)
            {
                return 0;
            }

            return Math.Max(0, state.MaxSelect - state.SelectedSkillIds.Count - state.SelectedToolIds.Count);
        }

        private static void TogglePendingMixedToolChoice(CharacterCreationMixedProficiencyChoiceState state, string toolId)
        {
            if (state == null || string.IsNullOrWhiteSpace(toolId) || !ContainsExactValue(state.OptionToolIds, toolId))
            {
                return;
            }

            int maxToolSelect = GetMixedPendingToolRemainingSelect(state);
            if (maxToolSelect <= 0 && !ContainsExactValue(state.PendingToolIds, toolId))
            {
                return;
            }

            TogglePendingValue(state.PendingToolIds, toolId, maxToolSelect);
        }

        private static void SyncMixedToolChoiceState(CharacterCreationMixedToolChoiceState toolState)
        {
            if (toolState?.MixedState == null)
            {
                return;
            }

            CharacterCreationMixedProficiencyChoiceState mixedState = toolState.MixedState;
            toolState.MaxSelect = GetMixedToolRemainingSelect(mixedState);
            toolState.PendingToolIds.Clear();
            toolState.SelectedToolIds.Clear();
            AppendUniqueExactValues(toolState.PendingToolIds, mixedState.PendingToolIds);
            AppendUniqueExactValues(toolState.SelectedToolIds, mixedState.SelectedToolIds);
        }

        private static void ValidateMixedToolSelections(CharacterCreationMixedProficiencyChoiceState state)
        {
            if (state == null)
            {
                return;
            }

            RemoveMissingValues(state.PendingToolIds, state.OptionToolIds);
            RemoveMissingValues(state.SelectedToolIds, state.OptionToolIds);

            int maxPendingToolSelect = GetMixedPendingToolRemainingSelect(state);
            while (state.PendingToolIds.Count > maxPendingToolSelect)
            {
                state.PendingToolIds.RemoveAt(state.PendingToolIds.Count - 1);
            }

            int maxToolSelect = GetMixedToolRemainingSelect(state);
            while (state.SelectedToolIds.Count > maxToolSelect)
            {
                state.SelectedToolIds.RemoveAt(state.SelectedToolIds.Count - 1);
            }
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

            return string.Equals(choiceGroup.ChoiceType, "AbilityScore", StringComparison.OrdinalIgnoreCase);
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

            if (targetCap > 0 && Instance.GetCurrentAbilityScore(normalized, 10) >= targetCap)
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

            return targetCap <= 0 || GetCurrentAbilityScore(normalized, 10) < targetCap;
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

        private static bool ContainsExactString(IReadOnlyList<string> values, string value)
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

        private static bool RemoveNormalizedSkillId(List<string> target, string value)
        {
            if (target == null || string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string normalized = NormalizeSkillId(value);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return false;
            }

            for (int index = target.Count - 1; index >= 0; index--)
            {
                if (string.Equals(NormalizeSkillId(target[index]), normalized, StringComparison.OrdinalIgnoreCase))
                {
                    target.RemoveAt(index);
                    return true;
                }
            }

            return false;
        }

        private static bool TryResolveSkillOrToolOption(DndChoiceOptionData option, out string skillId, out string toolId)
        {
            skillId = string.Empty;
            toolId = string.Empty;
            string optionId = option?.OptionId?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(optionId))
            {
                return false;
            }

            if (optionId.StartsWith("skill:", StringComparison.OrdinalIgnoreCase))
            {
                skillId = NormalizeSkillId(optionId.Substring("skill:".Length));
                return !string.IsNullOrWhiteSpace(skillId);
            }

            if (optionId.StartsWith("tool:", StringComparison.OrdinalIgnoreCase))
            {
                toolId = optionId.Substring("tool:".Length).Trim();
                return !string.IsNullOrWhiteSpace(toolId) && DndRuleContentService.Instance.TryGetTool(toolId, out _);
            }

            string normalizedSkillId = NormalizeSkillId(optionId);
            if (!string.IsNullOrWhiteSpace(normalizedSkillId)
                && DndRuleContentService.Instance.TryGetSkill(normalizedSkillId, out _))
            {
                skillId = normalizedSkillId;
                return true;
            }

            if (DndRuleContentService.Instance.TryGetTool(optionId, out DndToolDefineData tool))
            {
                toolId = tool.ToolId;
                return true;
            }

            return false;
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

        private CharacterManualOverrideSaveData EnsureManualOverrides()
        {
            EnsureState();
            m_state.Character.ManualOverrides = CharacterManualOverrideSaveData.Clone(m_state.Character.ManualOverrides);
            return m_state.Character.ManualOverrides;
        }

        private static bool IsAbilityOverrideField(string fieldId)
        {
            string normalized = NormalizeAbilityId(fieldId);
            return !string.IsNullOrWhiteSpace(normalized);
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
            int constitutionModifier = CharacterCreationCalculationService.Instance.CalculateAbilityModifier(GetCurrentEffectiveAbilityScore("Constitution", 10));
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
            snapshot.Strength = GetCurrentEffectiveAbilityScore("Strength", 10);
            snapshot.Dexterity = GetCurrentEffectiveAbilityScore("Dexterity", 10);
            snapshot.Constitution = GetCurrentEffectiveAbilityScore("Constitution", 10);
            snapshot.Intelligence = GetCurrentEffectiveAbilityScore("Intelligence", 10);
            snapshot.Wisdom = GetCurrentEffectiveAbilityScore("Wisdom", 10);
            snapshot.Charisma = GetCurrentEffectiveAbilityScore("Charisma", 10);
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

        private CharacterInventoryOperationResult ApplyInventoryOperation(
            Func<CharacterEquipmentSetSaveData, CharacterInventoryOperationResult> operation)
        {
            EnsureState();
            if (operation == null)
            {
                return new CharacterInventoryOperationResult
                {
                    Success = false,
                    Message = "Inventory operation is empty.",
                    Equipment = CharacterEquipmentSetSaveData.Clone(m_state.Character?.Equipment)
                };
            }

            CharacterInventoryOperationResult result = operation(m_state.Character.Equipment);
            if (result.Success)
            {
                m_state.Character.Equipment = CharacterEquipmentSetSaveData.Clone(result.Equipment);
                m_state.IsDirty = true;
                RefreshDerivedStatsAfterAbilityScoresChanged();
            }

            return result;
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
