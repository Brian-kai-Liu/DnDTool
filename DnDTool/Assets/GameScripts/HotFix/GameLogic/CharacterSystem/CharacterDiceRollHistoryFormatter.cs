using System;
using System.Collections.Generic;
using System.Text;

namespace GameLogic
{
    internal static class CharacterDiceRollHistoryFormatter
    {
        public static CharacterDiceRollHistoryEntry FromSaveData(CharacterDiceRollHistorySaveData source)
        {
            if (source == null)
            {
                return null;
            }

            DateTime createdAt = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(source.CreatedAt)
                && DateTime.TryParse(source.CreatedAt.Trim(), out DateTime parsedCreatedAt))
            {
                createdAt = parsedCreatedAt;
            }

            return new CharacterDiceRollHistoryEntry
            {
                EntryId = source.EntryId?.Trim() ?? string.Empty,
                CreatedAt = createdAt,
                SourceItemInstanceId = source.SourceItemInstanceId?.Trim() ?? string.Empty,
                SourceItemName = source.SourceItemName?.Trim() ?? string.Empty,
                SourceEffectName = source.SourceEffectName?.Trim() ?? string.Empty,
                DiceExpression = source.DiceExpression?.Trim() ?? string.Empty,
                Purpose = source.Purpose?.Trim() ?? string.Empty,
                Summary = source.Summary ?? string.Empty,
                Total = source.Total,
                Success = source.Success,
                Error = source.Error ?? string.Empty,
                Applied = source.Applied,
                AppliedMessage = source.AppliedMessage ?? string.Empty
            };
        }

        public static CharacterDiceRollHistorySaveData ToSaveData(CharacterDiceRollHistoryEntry source)
        {
            if (source == null)
            {
                return null;
            }

            return new CharacterDiceRollHistorySaveData
            {
                EntryId = source.EntryId?.Trim() ?? string.Empty,
                CreatedAt = source.CreatedAt.ToString("O"),
                SourceItemInstanceId = source.SourceItemInstanceId?.Trim() ?? string.Empty,
                SourceItemName = source.SourceItemName?.Trim() ?? string.Empty,
                SourceEffectName = source.SourceEffectName?.Trim() ?? string.Empty,
                DiceExpression = source.DiceExpression?.Trim() ?? string.Empty,
                Purpose = source.Purpose?.Trim() ?? string.Empty,
                Summary = source.Summary ?? string.Empty,
                Total = source.Total,
                Success = source.Success,
                Error = source.Error ?? string.Empty,
                Applied = source.Applied,
                AppliedMessage = source.AppliedMessage ?? string.Empty
            };
        }

        public static string BuildRecentHistoryText(
            IReadOnlyList<CharacterDiceRollHistoryEntry> history,
            int maxCount,
            bool includeTitle)
        {
            if (history == null || history.Count == 0 || maxCount <= 0)
            {
                return includeTitle ? "最近掷骰：\n暂无掷骰记录" : "暂无掷骰记录";
            }

            StringBuilder builder = new StringBuilder();
            if (includeTitle)
            {
                builder.AppendLine("最近掷骰：");
            }

            int count = Math.Min(maxCount, history.Count);
            for (int index = 0; index < count; index++)
            {
                CharacterDiceRollHistoryEntry entry = history[index];
                if (entry == null)
                {
                    continue;
                }

                if (!includeTitle && builder.Length > 0)
                {
                    builder.AppendLine();
                }

                AppendHistoryEntry(builder, entry, index + 1);
            }

            return builder.ToString().TrimEnd();
        }

        public static string BuildRecentHistoryText(
            IReadOnlyList<CharacterDiceRollHistorySaveData> history,
            int maxCount,
            bool includeTitle)
        {
            List<CharacterDiceRollHistoryEntry> entries = new List<CharacterDiceRollHistoryEntry>();
            if (history != null)
            {
                for (int index = 0; index < history.Count && entries.Count < Math.Max(0, maxCount); index++)
                {
                    CharacterDiceRollHistoryEntry entry = FromSaveData(history[index]);
                    if (entry != null)
                    {
                        entries.Add(entry);
                    }
                }
            }

            return BuildRecentHistoryText(entries, maxCount, includeTitle);
        }

        private static void AppendHistoryEntry(StringBuilder builder, CharacterDiceRollHistoryEntry entry, int displayIndex)
        {
            if (builder == null || entry == null)
            {
                return;
            }

            string source = BuildSourceText(entry);
            string purpose = string.IsNullOrWhiteSpace(entry.Purpose) ? "未指定用途" : entry.Purpose.Trim();
            string expression = string.IsNullOrWhiteSpace(entry.DiceExpression) ? "-" : entry.DiceExpression.Trim();

            builder.Append(displayIndex);
            builder.Append(". ");
            builder.Append(entry.CreatedAt.ToString("HH:mm:ss"));
            builder.Append(" ");
            builder.Append(source);
            builder.Append(" / ");
            builder.Append(purpose);
            builder.Append(" / ");
            builder.Append(expression);
            builder.AppendLine();

            builder.Append("   ");
            builder.Append(entry.Success ? BuildSuccessText(entry) : BuildFailureText(entry));

            if (entry.Applied)
            {
                builder.AppendLine();
                builder.Append("   已应用");
                if (!string.IsNullOrWhiteSpace(entry.AppliedMessage))
                {
                    builder.Append("：");
                    builder.Append(entry.AppliedMessage.Trim().Replace("\r\n", "；").Replace("\n", "；"));
                }
            }

            builder.AppendLine();
        }

        private static string BuildSourceText(CharacterDiceRollHistoryEntry entry)
        {
            string itemName = entry?.SourceItemName?.Trim() ?? string.Empty;
            string effectName = entry?.SourceEffectName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(itemName) && string.IsNullOrWhiteSpace(effectName))
            {
                return "手动掷骰";
            }

            if (string.IsNullOrWhiteSpace(effectName))
            {
                return itemName;
            }

            if (string.IsNullOrWhiteSpace(itemName))
            {
                return effectName;
            }

            return $"{itemName} - {effectName}";
        }

        private static string BuildSuccessText(CharacterDiceRollHistoryEntry entry)
        {
            string summary = entry?.Summary?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(summary))
            {
                return summary;
            }

            return $"总值：{entry?.Total ?? 0}";
        }

        private static string BuildFailureText(CharacterDiceRollHistoryEntry entry)
        {
            string error = entry?.Error?.Trim() ?? string.Empty;
            return string.IsNullOrWhiteSpace(error) ? "失败：未知掷骰错误。" : $"失败：{error}";
        }
    }
}
