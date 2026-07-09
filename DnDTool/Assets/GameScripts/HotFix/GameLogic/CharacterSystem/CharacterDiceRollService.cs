using System;
using System.Collections.Generic;
using System.Text;

namespace GameLogic
{
    internal sealed class CharacterDiceRollService
    {
        private const int MaxDiceTerms = 32;
        private const int MaxDiceCountPerTerm = 100;
        private const int MaxDieSides = 1000;
        private static readonly Lazy<CharacterDiceRollService> s_instance =
            new Lazy<CharacterDiceRollService>(() => new CharacterDiceRollService());

        private readonly Random m_random = new Random();

        private CharacterDiceRollService()
        {
        }

        public static CharacterDiceRollService Instance => s_instance.Value;

        public bool TryParse(string expression, out CharacterDiceExpressionData data, out string error)
        {
            data = null;
            error = string.Empty;

            string normalized = NormalizeExpression(expression);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                error = "骰子表达式为空。";
                return false;
            }

            CharacterDiceExpressionData result = new CharacterDiceExpressionData
            {
                Expression = normalized
            };

            int index = 0;
            int sign = 1;
            while (index < normalized.Length)
            {
                char current = normalized[index];
                if (current == '+')
                {
                    if (index == normalized.Length - 1)
                    {
                        error = "骰子表达式不能以加号结尾。";
                        return false;
                    }

                    sign = 1;
                    index++;
                    continue;
                }

                if (current == '-')
                {
                    if (index == normalized.Length - 1)
                    {
                        error = "骰子表达式不能以减号结尾。";
                        return false;
                    }

                    sign = -1;
                    index++;
                    continue;
                }

                if (!TryReadNumber(normalized, ref index, out int firstNumber))
                {
                    if (index < normalized.Length && IsDiceSeparator(normalized[index]))
                    {
                        firstNumber = 1;
                    }
                    else
                    {
                        error = $"无法解析骰子表达式第 {index + 1} 个字符。";
                        return false;
                    }
                }

                if (index < normalized.Length && IsDiceSeparator(normalized[index]))
                {
                    index++;
                    if (!TryReadNumber(normalized, ref index, out int dieSides))
                    {
                        error = "骰子面数缺失。";
                        return false;
                    }

                    if (firstNumber <= 0)
                    {
                        error = "骰子数量必须大于 0。";
                        return false;
                    }

                    if (firstNumber > MaxDiceCountPerTerm)
                    {
                        error = $"单个骰子项最多支持 {MaxDiceCountPerTerm} 个骰子。";
                        return false;
                    }

                    if (dieSides <= 0 || dieSides > MaxDieSides)
                    {
                        error = $"骰子面数必须在 1 到 {MaxDieSides} 之间。";
                        return false;
                    }

                    result.Terms.Add(new CharacterDiceTermData
                    {
                        Sign = sign,
                        DiceCount = firstNumber,
                        DieSides = dieSides
                    });
                }
                else
                {
                    result.Terms.Add(new CharacterDiceTermData
                    {
                        Sign = sign,
                        Constant = firstNumber
                    });
                }

                if (result.Terms.Count > MaxDiceTerms)
                {
                    error = $"骰子表达式最多支持 {MaxDiceTerms} 个项。";
                    return false;
                }

                sign = 1;
            }

            if (result.Terms.Count == 0)
            {
                error = "骰子表达式没有有效项。";
                return false;
            }

            data = result;
            return true;
        }

        public CharacterDiceRollResultData Roll(string expression)
        {
            if (!TryParse(expression, out CharacterDiceExpressionData data, out string error))
            {
                return CharacterDiceRollResultData.Fail(expression, error);
            }

            CharacterDiceRollResultData result = new CharacterDiceRollResultData
            {
                Success = true,
                Expression = data.Expression
            };

            for (int termIndex = 0; termIndex < data.Terms.Count; termIndex++)
            {
                CharacterDiceTermData term = data.Terms[termIndex];
                if (!term.IsDice)
                {
                    int value = term.Sign * term.Constant;
                    result.Total += value;
                    result.Terms.Add(new CharacterDiceRollTermResultData
                    {
                        Sign = term.Sign,
                        Constant = term.Constant,
                        Total = value
                    });
                    continue;
                }

                CharacterDiceRollTermResultData termResult = new CharacterDiceRollTermResultData
                {
                    Sign = term.Sign,
                    DiceCount = term.DiceCount,
                    DieSides = term.DieSides
                };

                for (int rollIndex = 0; rollIndex < term.DiceCount; rollIndex++)
                {
                    int roll = m_random.Next(1, term.DieSides + 1);
                    termResult.Rolls.Add(roll);
                    termResult.Total += term.Sign * roll;
                }

                result.Total += termResult.Total;
                result.Terms.Add(termResult);
            }

            result.Summary = BuildSummary(result);
            return result;
        }

        private static string BuildSummary(CharacterDiceRollResultData result)
        {
            if (result == null || !result.Success)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(result.Expression);
            builder.Append(" = ");
            for (int index = 0; index < result.Terms.Count; index++)
            {
                CharacterDiceRollTermResultData term = result.Terms[index];
                if (index > 0)
                {
                    builder.Append(term.Sign >= 0 ? " + " : " - ");
                }
                else if (term.Sign < 0)
                {
                    builder.Append("-");
                }

                if (term.IsDice)
                {
                    builder.Append("[");
                    for (int rollIndex = 0; rollIndex < term.Rolls.Count; rollIndex++)
                    {
                        if (rollIndex > 0)
                        {
                            builder.Append(", ");
                        }

                        builder.Append(term.Rolls[rollIndex]);
                    }

                    builder.Append("]");
                }
                else
                {
                    builder.Append(term.Constant);
                }
            }

            builder.Append(" = ");
            builder.Append(result.Total);
            return builder.ToString();
        }

        private static bool TryReadNumber(string expression, ref int index, out int value)
        {
            value = 0;
            int start = index;
            while (index < expression.Length && char.IsDigit(expression[index]))
            {
                index++;
            }

            if (index == start)
            {
                return false;
            }

            return int.TryParse(expression.Substring(start, index - start), out value);
        }

        private static bool IsDiceSeparator(char value)
        {
            return value == 'd' || value == 'D';
        }

        private static string NormalizeExpression(string expression)
        {
            return (expression ?? string.Empty)
                .Replace(" ", string.Empty)
                .Replace("，", "+")
                .Replace(",", "+")
                .Replace("＋", "+")
                .Replace("－", "-")
                .Trim();
        }
    }

    internal sealed class CharacterDiceExpressionData
    {
        public string Expression { get; set; } = string.Empty;
        public List<CharacterDiceTermData> Terms { get; } = new List<CharacterDiceTermData>();
    }

    internal sealed class CharacterDiceTermData
    {
        public int Sign { get; set; } = 1;
        public int DiceCount { get; set; }
        public int DieSides { get; set; }
        public int Constant { get; set; }
        public bool IsDice => DiceCount > 0 && DieSides > 0;
    }

    internal sealed class CharacterDiceRollResultData
    {
        public bool Success { get; set; }
        public string Expression { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public int Total { get; set; }
        public string Summary { get; set; } = string.Empty;
        public List<CharacterDiceRollTermResultData> Terms { get; } = new List<CharacterDiceRollTermResultData>();

        public static CharacterDiceRollResultData Fail(string expression, string error)
        {
            return new CharacterDiceRollResultData
            {
                Success = false,
                Expression = expression ?? string.Empty,
                Error = error ?? string.Empty
            };
        }
    }

    internal sealed class CharacterDiceRollTermResultData
    {
        public int Sign { get; set; } = 1;
        public int DiceCount { get; set; }
        public int DieSides { get; set; }
        public int Constant { get; set; }
        public int Total { get; set; }
        public List<int> Rolls { get; } = new List<int>();
        public bool IsDice => DiceCount > 0 && DieSides > 0;
    }
}
