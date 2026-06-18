using System;

namespace GameLogic
{
    internal sealed class CharacterCreationCalculationService
    {
        private const int DefaultAbilityScore = 10;

        private static readonly Lazy<CharacterCreationCalculationService> s_instance =
            new Lazy<CharacterCreationCalculationService>(() => new CharacterCreationCalculationService());

        private CharacterCreationCalculationService()
        {
        }

        public static CharacterCreationCalculationService Instance => s_instance.Value;

        public int CalculateAbilityModifier(int score)
        {
            return (score - 10) >= 0
                ? (score - 10) / 2
                : (score - 11) / 2;
        }

        public int CalculateProficiencyBonus(int level)
        {
            int normalizedLevel = Math.Max(1, level);
            return 2 + (normalizedLevel - 1) / 4;
        }

        public int NormalizeAbilityScore(int score)
        {
            return score > 0 ? score : DefaultAbilityScore;
        }

        public int GetExperienceThreshold(int level)
        {
            return Math.Max(1, level) switch
            {
                1 => 0,
                2 => 300,
                3 => 900,
                4 => 2700,
                5 => 6500,
                6 => 14000,
                7 => 23000,
                8 => 34000,
                9 => 48000,
                10 => 64000,
                11 => 85000,
                12 => 100000,
                13 => 120000,
                14 => 140000,
                15 => 165000,
                16 => 195000,
                17 => 225000,
                18 => 265000,
                19 => 305000,
                20 => 355000,
                _ => 355000
            };
        }

        public int NormalizeCurrentHp(int currentHp, int maxHp)
        {
            int normalizedMaxHp = Math.Max(0, maxHp);
            if (currentHp < 0)
            {
                return normalizedMaxHp;
            }

            int normalizedCurrentHp = Math.Max(0, currentHp);
            return normalizedMaxHp > 0 ? Math.Min(normalizedCurrentHp, normalizedMaxHp) : normalizedCurrentHp;
        }

        public string FormatSigned(int value)
        {
            return value >= 0 ? $"+{value}" : value.ToString();
        }
    }
}
