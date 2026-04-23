using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GameLogic
{
    internal sealed class ChapterEditorInputData
    {
        public int SelectedChapterIndex { get; set; } = -1;

        public List<ChapterListItemData> Chapters { get; set; } = new List<ChapterListItemData>();
    }

    internal sealed class ChapterMapGridStateData
    {
        public bool IsMapZoomEnabled { get; set; }

        public bool IsGridZoomEnabled { get; set; }

        public float MapZoomScale { get; set; } = 1f;

        public Vector2 MapPanOffset { get; set; } = Vector2.zero;

        public float GridZoomScale { get; set; } = 1f;

        public Vector2 GridPanOffset { get; set; } = Vector2.zero;

        public bool IsLocked { get; set; }

        public float LockedMapZoomReference { get; set; } = 1f;

        public float LockedGridToMapZoomRatio { get; set; } = 1f;

        public Vector2 LockedGridToMapPanDelta { get; set; } = Vector2.zero;
    }

    internal readonly struct ChapterGridCoordinate : IEquatable<ChapterGridCoordinate>
    {
        public static ChapterGridCoordinate Zero => new ChapterGridCoordinate(0, 0);

        public ChapterGridCoordinate(int cellX, int cellY)
        {
            CellX = cellX;
            CellY = cellY;
        }

        public int CellX { get; }

        public int CellY { get; }

        public bool Equals(ChapterGridCoordinate other)
        {
            return CellX == other.CellX && CellY == other.CellY;
        }

        public override bool Equals(object obj)
        {
            return obj is ChapterGridCoordinate other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (CellX * 397) ^ CellY;
            }
        }

        public override string ToString()
        {
            return $"{CellX}:{CellY}";
        }
    }

    internal enum ChapterGridCellMarkType
    {
        Selected = 1,
        DifficultTerrain = 2,
        ImpassableTerrain = 3,
        Event = 4,
    }

    internal sealed class ChapterGridEventData
    {
        public string EventId { get; set; } = string.Empty;

        public bool IsEnabled { get; set; } = true;

        public bool IsOneShot { get; set; }

        public int EventCategory { get; set; }

        public int EventSubType { get; set; }

        public int TriggerMode { get; set; }

        public int CheckTargetMode { get; set; }

        public int CheckResolutionMode { get; set; }

        public string EventTitle { get; set; } = string.Empty;

        public string TriggerDescription { get; set; } = string.Empty;

        public string SuccessResult { get; set; } = string.Empty;

        public string FailureResult { get; set; } = string.Empty;

        public string DmNote { get; set; } = string.Empty;

        public string DmPrompt { get; set; } = string.Empty;

        public List<ChapterSkillCheckThresholdData> SkillCheckEntries { get; set; } = new List<ChapterSkillCheckThresholdData>();

        public string SkillCheckName { get; set; } = string.Empty;

        public string SkillCheckThreshold { get; set; } = string.Empty;

        public string AbilityStrengthThreshold { get; set; } = string.Empty;

        public string AbilityDexterityThreshold { get; set; } = string.Empty;

        public string AbilityConstitutionThreshold { get; set; } = string.Empty;

        public string AbilityIntelligenceThreshold { get; set; } = string.Empty;

        public string AbilityWisdomThreshold { get; set; } = string.Empty;

        public string AbilityCharismaThreshold { get; set; } = string.Empty;
    }

    internal sealed class ChapterSkillCheckThresholdData
    {
        public string SkillName { get; set; } = string.Empty;

        public string Threshold { get; set; } = string.Empty;
    }

    internal sealed class ChapterGridCellData
    {
        public ChapterGridCoordinate Coordinate { get; set; } = ChapterGridCoordinate.Zero;

        public ChapterGridCellMarkType MarkType { get; set; } = ChapterGridCellMarkType.Selected;

        public ChapterGridEventData EventData { get; set; }
    }

    internal sealed class ChapterEventBindingData
    {
        public string BindingId { get; set; } = string.Empty;

        public string EventId { get; set; } = string.Empty;

        public List<ChapterGridCoordinate> GridCoordinates { get; set; } = new List<ChapterGridCoordinate>();
    }

    internal sealed class ChapterListItemData
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Goal { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string DmNote { get; set; } = string.Empty;

        public string TerrainTag { get; set; } = string.Empty;

        public string TerrainSubTag { get; set; } = string.Empty;

        public string AddMapHint { get; set; } = string.Empty;

        public string CreatureInfo { get; set; } = string.Empty;

        public string MapImagePath { get; set; } = string.Empty;

        public ChapterMapGridStateData MapGridState { get; set; } = new ChapterMapGridStateData();

        public List<ChapterGridCellData> GridCells { get; set; } = new List<ChapterGridCellData>();

        public List<ChapterGridEventData> Events { get; set; } = new List<ChapterGridEventData>();

        public List<ChapterEventBindingData> EventBindings { get; set; } = new List<ChapterEventBindingData>();

        public List<ChapterCreatureData> Creatures { get; set; } = new List<ChapterCreatureData>();
    }

    [Serializable]
    internal sealed class ChapterEditorSaveData
    {
        public int SelectedChapterId = -1;
        public int NextChapterId = 1;
        public List<ChapterItemSaveData> Chapters = new List<ChapterItemSaveData>();
    }

    [Serializable]
    internal sealed class ChapterItemSaveData
    {
        public int Id;
        public string Name = string.Empty;
        public string Goal = string.Empty;
        public string Content = string.Empty;
        public string DmNote = string.Empty;
        public string TerrainTag = string.Empty;
        public string TerrainSubTag = string.Empty;
        public string AddMapHint = string.Empty;
        public string CreatureInfo = string.Empty;
        public string MapImagePath = string.Empty;
        public bool IsMapZoomEnabled;
        public bool IsGridZoomEnabled;
        public float MapZoomScale = 1f;
        public Vector2 MapPanOffset = Vector2.zero;
        public float GridZoomScale = 1f;
        public Vector2 GridPanOffset = Vector2.zero;
        public bool IsMapGridLocked;
        public float LockedMapZoomReference = 1f;
        public float LockedGridToMapZoomRatio = 1f;
        public Vector2 LockedGridToMapPanDelta = Vector2.zero;
        public List<ChapterGridCellSaveData> GridCells = new List<ChapterGridCellSaveData>();
        public List<ChapterGridEventSaveData> Events = new List<ChapterGridEventSaveData>();
        public List<ChapterEventBindingSaveData> EventBindings = new List<ChapterEventBindingSaveData>();
        public List<string> SelectedGridCellKeys = new List<string>();
        public List<ChapterCreatureDataSaveData> Creatures = new List<ChapterCreatureDataSaveData>();
    }

    [Serializable]
    internal sealed class ChapterGridCellSaveData
    {
        public int CellX;
        public int CellY;
        public int MarkType = (int) ChapterGridCellMarkType.DifficultTerrain;
        public ChapterGridEventSaveData EventData;
    }

    [Serializable]
    internal sealed class ChapterGridEventSaveData
    {
        public string EventId = string.Empty;
        public bool IsEnabled = true;
        public bool IsOneShot;
        public int EventType = 2;
        public int EventCategory;
        public int EventSubType;
        public int TriggerMode;
        public int CheckTargetMode;
        public int CheckResolutionMode;
        public string EventTitle = string.Empty;
        public string TriggerDescription = string.Empty;
        public string SuccessResult = string.Empty;
        public string FailureResult = string.Empty;
        public string DmNote = string.Empty;
        public string DmPrompt = string.Empty;
        public List<ChapterSkillCheckThresholdSaveData> SkillCheckEntries = new List<ChapterSkillCheckThresholdSaveData>();
        public string SkillCheckName = string.Empty;
        public string SkillCheckThreshold = string.Empty;
        public string AbilityStrengthThreshold = string.Empty;
        public string AbilityDexterityThreshold = string.Empty;
        public string AbilityConstitutionThreshold = string.Empty;
        public string AbilityIntelligenceThreshold = string.Empty;
        public string AbilityWisdomThreshold = string.Empty;
        public string AbilityCharismaThreshold = string.Empty;
    }

    [Serializable]
    internal sealed class ChapterEventBindingSaveData
    {
        public string BindingId = string.Empty;
        public string EventId = string.Empty;
        public List<ChapterGridCoordinateSaveData> GridCoordinates = new List<ChapterGridCoordinateSaveData>();
    }

    [Serializable]
    internal sealed class ChapterGridCoordinateSaveData
    {
        public int CellX;
        public int CellY;
    }

    [Serializable]
    internal sealed class ChapterSkillCheckThresholdSaveData
    {
        public string SkillName = string.Empty;

        public string Threshold = string.Empty;
    }

    [Serializable]
    internal sealed class ChapterCreatureDataSaveData
    {
        public string Name = string.Empty;
        public string NameEn = string.Empty;
        public string CreatureType = string.Empty;
        public string CreatureSize = string.Empty;
        public string Alignment = string.Empty;
        public string ChallengeRating = string.Empty;
        public string ExperiencePoints = string.Empty;
        public string ArmorClass = string.Empty;
        public string HitPoints = string.Empty;
        public string Speed = string.Empty;
        public string Strength = string.Empty;
        public string Dexterity = string.Empty;
        public string Constitution = string.Empty;
        public string Intelligence = string.Empty;
        public string Wisdom = string.Empty;
        public string Charisma = string.Empty;
        public string SavingThrows = string.Empty;
        public string Skills = string.Empty;
        public string Senses = string.Empty;
        public string Languages = string.Empty;
        public string DamageResistances = string.Empty;
        public string DamageImmunities = string.Empty;
        public string ConditionImmunities = string.Empty;
        public string Traits = string.Empty;
        public string Actions = string.Empty;
        public string BonusActions = string.Empty;
        public string Reactions = string.Empty;
        public string LegendaryActions = string.Empty;
        public string BattleNotes = string.Empty;
        public string PreviewImageFileName = string.Empty;
        public float AccentColorR = 0.45f;
        public float AccentColorG = 0.55f;
        public float AccentColorB = 0.7f;
        public float AccentColorA = 1f;
    }

    internal sealed class ChapterCreatureData
    {
        // 标头
        public string Name { get; set; } = string.Empty;

        public string NameEn { get; set; } = string.Empty;

        public string CreatureType { get; set; } = string.Empty;

        public string CreatureSize { get; set; } = string.Empty;

        public string Alignment { get; set; } = string.Empty;

        // 战斗属性
        public string ChallengeRating { get; set; } = string.Empty;

        public string ExperiencePoints { get; set; } = string.Empty;

        public string ArmorClass { get; set; } = string.Empty;

        public string HitPoints { get; set; } = string.Empty;

        public string Speed { get; set; } = string.Empty;

        // 六维属性（独立字段）
        public string Strength { get; set; } = string.Empty;

        public string Dexterity { get; set; } = string.Empty;

        public string Constitution { get; set; } = string.Empty;

        public string Intelligence { get; set; } = string.Empty;

        public string Wisdom { get; set; } = string.Empty;

        public string Charisma { get; set; } = string.Empty;

        // 豁免 / 技能
        public string SavingThrows { get; set; } = string.Empty;

        public string Skills { get; set; } = string.Empty;

        // 感官 / 语言
        public string Senses { get; set; } = string.Empty;

        public string Languages { get; set; } = string.Empty;

        // 防御（拆分）
        public string DamageResistances { get; set; } = string.Empty;

        public string DamageImmunities { get; set; } = string.Empty;

        public string ConditionImmunities { get; set; } = string.Empty;

        // 特性 / 动作（条目间用 --- 分隔）
        public string Traits { get; set; } = string.Empty;

        public string Actions { get; set; } = string.Empty;

        public string BonusActions { get; set; } = string.Empty;

        public string Reactions { get; set; } = string.Empty;

        public string LegendaryActions { get; set; } = string.Empty;

        // DM 私密
        public string BattleNotes { get; set; } = string.Empty;

        // 预览图
        public string PreviewImageFileName { get; set; } = string.Empty;

        public Color AccentColor { get; set; } = new Color(0.45f, 0.55f, 0.7f, 1f);

        public string GetCreatureTypeDisplay()
        {
            string size = string.IsNullOrWhiteSpace(CreatureSize) ? string.Empty : CreatureSize + " ";
            string type = string.IsNullOrWhiteSpace(CreatureType) ? "未分类生物" : CreatureType;
            string cr = string.IsNullOrWhiteSpace(ChallengeRating) ? "-" : ChallengeRating;
            return $"{size}{type} | CR {cr}";
        }

        public string BuildSummary()
        {
            var b = new StringBuilder(1024);

            // 基础战斗数值
            b.Append("AC: ").Append(Fallback(ArmorClass, "-"));
            b.Append("  HP: ").Append(Fallback(HitPoints, "-"));
            b.Append("  速度: ").AppendLine(Fallback(Speed, "-"));

            // 六维
            b.Append("力量 ").Append(Fallback(Strength, "?")).Append("  ");
            b.Append("敏捷 ").Append(Fallback(Dexterity, "?")).Append("  ");
            b.Append("体质 ").Append(Fallback(Constitution, "?")).Append("  ");
            b.Append("智力 ").Append(Fallback(Intelligence, "?")).Append("  ");
            b.Append("感知 ").Append(Fallback(Wisdom, "?")).Append("  ");
            b.Append("魅力 ").AppendLine(Fallback(Charisma, "?"));

            // 豁免 / 技能
            AppendInline(b, "豁免", SavingThrows);
            AppendInline(b, "技能", Skills);

            // 感官 / 语言
            AppendInline(b, "感官", Senses);
            AppendInline(b, "语言", Languages);

            // 防御
            AppendInline(b, "伤害抗性", DamageResistances);
            AppendInline(b, "伤害免疫", DamageImmunities);
            AppendInline(b, "状态免疫", ConditionImmunities);

            // 特性 / 动作块
            AppendBlock(b, "特性", Traits);
            AppendBlock(b, "动作", Actions);
            AppendBlock(b, "附赠动作", BonusActions);
            AppendBlock(b, "反应", Reactions);
            AppendBlock(b, "传奇动作", LegendaryActions);
            AppendBlock(b, "战斗备注", BattleNotes);

            return b.ToString().TrimEnd();
        }

        private static string Fallback(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static void AppendInline(StringBuilder b, string label, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                b.Append(label).Append(": ").AppendLine(value);
            }
        }

        private static void AppendBlock(StringBuilder b, string title, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return;
            }

            b.AppendLine();
            b.Append('【').Append(title).AppendLine("】");
            b.AppendLine(content);
        }
    }
}
