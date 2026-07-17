using System;
using System.Collections.Generic;

namespace GameLogic
{
    internal sealed class CharacterSheetViewStateService
    {
        private readonly struct SkillDefinition
        {
            public readonly string SkillId;
            public readonly string DisplayName;
            public readonly AbilityKind Ability;

            public SkillDefinition(string skillId, string displayName, AbilityKind ability)
            {
                SkillId = skillId;
                DisplayName = displayName;
                Ability = ability;
            }
        }

        private static readonly Lazy<CharacterSheetViewStateService> s_instance =
            new Lazy<CharacterSheetViewStateService>(() => new CharacterSheetViewStateService());

        private static readonly SkillDefinition[] SkillDefinitions =
        {
            new SkillDefinition("athletics", "运动", AbilityKind.Strength),
            new SkillDefinition("acrobatics", "体操", AbilityKind.Dexterity),
            new SkillDefinition("sleight_of_hand", "巧手", AbilityKind.Dexterity),
            new SkillDefinition("stealth", "隐匿", AbilityKind.Dexterity),
            new SkillDefinition("arcana", "奥秘", AbilityKind.Intelligence),
            new SkillDefinition("history", "历史", AbilityKind.Intelligence),
            new SkillDefinition("investigation", "调查", AbilityKind.Intelligence),
            new SkillDefinition("nature", "自然", AbilityKind.Intelligence),
            new SkillDefinition("religion", "宗教", AbilityKind.Intelligence),
            new SkillDefinition("animal_handling", "驯兽", AbilityKind.Wisdom),
            new SkillDefinition("insight", "洞悉", AbilityKind.Wisdom),
            new SkillDefinition("medicine", "医药", AbilityKind.Wisdom),
            new SkillDefinition("perception", "察觉", AbilityKind.Wisdom),
            new SkillDefinition("survival", "求生", AbilityKind.Wisdom),
            new SkillDefinition("deception", "欺瞒", AbilityKind.Charisma),
            new SkillDefinition("intimidation", "威吓", AbilityKind.Charisma),
            new SkillDefinition("performance", "表演", AbilityKind.Charisma),
            new SkillDefinition("persuasion", "游说", AbilityKind.Charisma)
        };

        private CharacterSheetViewStateService()
        {
        }

        public static CharacterSheetViewStateService Instance => s_instance.Value;

        public CharacterSheetDisplayViewState Build(CharacterCardDraftSaveData source, CharacterRuntimeSnapshotData snapshotOverride = null)
        {
            CharacterCardDraftSaveData character = CharacterCardLocalRepository.Normalize(source ?? new CharacterCardDraftSaveData());
            CharacterRuntimeSnapshotData snapshot = snapshotOverride != null
                ? CharacterRuntimeSnapshotData.Clone(snapshotOverride)
                : CharacterDetailCalculationService.Instance.BuildDisplaySnapshot(character);

            CharacterSheetDisplayViewState state = new CharacterSheetDisplayViewState
            {
                Character = character,
                RuntimeSnapshot = snapshot,
                CharacterNameText = FormatTextOrDefault(snapshot.CharacterName, "未选择角色"),
                RaceText = FormatTextOrDefault(snapshot.RaceName, "未选择种族"),
                ClassText = CharacterDetailDisplayService.Instance.BuildClassNameSummary(character, snapshot),
                AbilityDisplay = CharacterDetailCalculationService.Instance.BuildAbilityDisplay(snapshot),
                HpDisplay = CharacterDetailCalculationService.Instance.BuildHpDisplay(snapshot),
                CombatOverview = CharacterDetailCalculationService.Instance.BuildCombatOverview(character, snapshot),
                Experience = CharacterDetailCalculationService.Instance.BuildExperienceDisplay(character.Experience, snapshot.Level),
                ProficiencyBonusText = CharacterDetailCalculationService.Instance.BuildProficiencyBonusDisplay(snapshot.Level).Label
            };

            FillHpTexts(state);
            FillCombatTexts(state);
            FillCurrencyTexts(state, character);
            FillDeathSaveTexts(state, snapshot);
            FillHitDiceTexts(state, character);
            FillSkillStates(state, character, snapshot);
            state.EquipmentToolLabels.AddRange(CharacterDetailDisplayService.Instance.BuildEquipmentAndToolEntries(snapshot));
            state.StatusEffects.AddRange(CharacterDetailDisplayService.Instance.BuildStatusEffectEntries(character));
            state.InventoryItems.AddRange(CharacterDetailDisplayService.Instance.BuildInventoryEntries(character.Equipment));
            return state;
        }

        private static void FillHpTexts(CharacterSheetDisplayViewState state)
        {
            CharacterHpDisplayViewState hp = state?.HpDisplay;
            if (state == null || hp == null)
            {
                return;
            }

            state.CurrentHpText = hp.MaxHp > 0 ? hp.CurrentHp.ToString() : "-";
            state.MaxHpText = hp.MaxHp > 0 ? hp.MaxHp.ToString() : "-";
            state.TemporaryHpText = hp.TemporaryHp > 0 ? hp.TemporaryHp.ToString() : "0";
        }

        private static void FillCombatTexts(CharacterSheetDisplayViewState state)
        {
            CharacterCombatOverviewViewState overview = state?.CombatOverview;
            if (state == null || overview == null)
            {
                return;
            }

            state.ArmorClassText = overview.ArmorClass > 0 ? overview.ArmorClass.ToString() : "-";
            state.InitiativeText = CharacterCreationCalculationService.Instance.FormatSigned(overview.InitiativeBonus);
            state.SpeedText = overview.Speed > 0 ? overview.Speed.ToString() : "-";
            state.PassivePerceptionText = overview.PassivePerception.ToString();
            if (overview.HasSpellcasting)
            {
                state.SpellSaveDcText = overview.SpellSaveDc.ToString();
                state.SpellAttackBonusText = CharacterCreationCalculationService.Instance.FormatSigned(overview.SpellAttackBonus);
            }
            else
            {
                state.SpellSaveDcText = "-";
                state.SpellAttackBonusText = "-";
            }
        }

        private static void FillCurrencyTexts(CharacterSheetDisplayViewState state, CharacterCardDraftSaveData character)
        {
            if (state == null)
            {
                return;
            }

            CharacterCurrencySaveData currency = CharacterCurrencySaveData.Clone(character?.Currency);
            CharacterManualOverrideSaveData overrides = character?.ManualOverrides;
            if (overrides != null)
            {
                if (overrides.HasCopper)
                {
                    currency.Copper = overrides.Copper;
                }

                if (overrides.HasSilver)
                {
                    currency.Silver = overrides.Silver;
                }

                if (overrides.HasElectrum)
                {
                    currency.Electrum = overrides.Electrum;
                }

                if (overrides.HasGold)
                {
                    currency.Gold = overrides.Gold;
                }

                if (overrides.HasPlatinum)
                {
                    currency.Platinum = overrides.Platinum;
                }
            }

            state.CopperText = currency.Copper.ToString();
            state.SilverText = currency.Silver.ToString();
            state.ElectrumText = currency.Electrum.ToString();
            state.GoldText = currency.Gold.ToString();
            state.PlatinumText = currency.Platinum.ToString();
        }

        private static void FillDeathSaveTexts(CharacterSheetDisplayViewState state, CharacterRuntimeSnapshotData snapshot)
        {
            if (state == null)
            {
                return;
            }

            int successes = snapshot != null ? Clamp(snapshot.DeathSaveSuccesses, 0, 3) : 0;
            int failures = snapshot != null ? Clamp(snapshot.DeathSaveFailures, 0, 3) : 0;
            state.DeathSaveSuccessesText = FormatDeathSaveText("成功", successes);
            state.DeathSaveFailuresText = FormatDeathSaveText("失败", failures);
        }

        private static void FillHitDiceTexts(CharacterSheetDisplayViewState state, CharacterCardDraftSaveData character)
        {
            if (state == null)
            {
                return;
            }

            List<CharacterHitDicePoolSaveData> pools = CharacterDetailDisplayService.Instance.BuildDisplayHitDicePools(character);
            int remaining = 0;
            List<string> dice = new List<string>();
            if (pools != null)
            {
                for (int index = 0; index < pools.Count; index++)
                {
                    CharacterHitDicePoolSaveData pool = CharacterHitDicePoolSaveData.Clone(pools[index]);
                    if (pool.DieSize <= 0 && pool.Total <= 0)
                    {
                        continue;
                    }

                    remaining += pool.Remaining;
                    if (pool.DieSize > 0)
                    {
                        string dieText = $"d{pool.DieSize}";
                        if (!dice.Contains(dieText))
                        {
                            dice.Add(dieText);
                        }
                    }
                }
            }

            CharacterManualOverrideSaveData overrides = character?.ManualOverrides;
            if (overrides != null && overrides.HasHitDiceRemaining)
            {
                remaining = overrides.HitDiceRemaining;
            }

            if (overrides != null && overrides.HasHitDiceDie)
            {
                dice.Clear();
                if (overrides.HitDiceDie > 0)
                {
                    dice.Add($"d{overrides.HitDiceDie}");
                }
            }

            state.HitDiceCountText = $"x{Math.Max(0, remaining)}";
            state.HitDiceDieText = dice.Count > 0 ? string.Join("/", dice) : "-";
        }

        private static void FillSkillStates(CharacterSheetDisplayViewState state, CharacterCardDraftSaveData character, CharacterRuntimeSnapshotData snapshot)
        {
            if (state == null)
            {
                return;
            }

            for (int index = 0; index < SkillDefinitions.Length; index++)
            {
                SkillDefinition definition = SkillDefinitions[index];
                CharacterSkillBonusViewState skill = CharacterDetailCalculationService.Instance.BuildSkillBonus(
                    character,
                    snapshot,
                    definition.SkillId,
                    definition.DisplayName,
                    definition.Ability);

                state.Skills.Add(new CharacterSheetSkillViewState
                {
                    SkillId = definition.SkillId,
                    DisplayName = definition.DisplayName,
                    Bonus = skill.Bonus,
                    BonusText = CharacterCreationCalculationService.Instance.FormatSigned(skill.Bonus),
                    HasProficiency = skill.HasProficiency,
                    HasExpertise = skill.HasExpertise
                });
            }
        }

        private static string FormatTextOrDefault(string value, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        }

        private static string FormatDeathSaveText(string label, int value)
        {
            return $"{label} {Clamp(value, 0, 3)}/3";
        }

        private static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}
