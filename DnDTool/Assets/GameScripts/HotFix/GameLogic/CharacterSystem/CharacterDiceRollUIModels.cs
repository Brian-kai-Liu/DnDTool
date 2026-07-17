using System;

namespace GameLogic
{
    internal sealed class DiceRollUIRequest
    {
        public string SourceType { get; set; } = string.Empty;
        public string SourceId { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public string EffectName { get; set; } = string.Empty;
        public string EffectDescription { get; set; } = string.Empty;
        public string DiceExpression { get; set; } = string.Empty;
        public Action<DiceRollUIResult> OnResult { get; set; }
    }

    internal sealed class DiceRollUIResult
    {
        public string SourceType { get; set; } = string.Empty;
        public string SourceId { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public string EffectName { get; set; } = string.Empty;
        public string EffectDescription { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public CharacterDiceRollResultData RollResult { get; set; }
    }
}
