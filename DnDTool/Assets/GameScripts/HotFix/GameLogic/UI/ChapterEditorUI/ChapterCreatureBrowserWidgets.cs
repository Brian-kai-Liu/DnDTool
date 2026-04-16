using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GameLogic
{
    internal readonly struct ChapterCreatureStaticCardData
    {
        public ChapterCreatureStaticCardData(string name, string creatureType, string alignment, Color accentColor, string summary)
        {
            Name = name;
            CreatureType = creatureType;
            Alignment = alignment;
            AccentColor = accentColor;
            Summary = summary;
        }

        public string Name { get; }

        public string CreatureType { get; }

        public string Alignment { get; }

        public Color AccentColor { get; }

        public string Summary { get; }
    }

    internal sealed class ChapterCreatureCardWidget
    {
        private static readonly Color DefaultCardColor = new Color(0.18f, 0.21f, 0.25f, 0.94f);
        private static readonly Color SelectedCardColor = new Color(0.27f, 0.31f, 0.37f, 1f);

        private readonly GameObject m_gameObject;
        private readonly Button m_button;
        private readonly Image m_imgBackground;
        private readonly Image m_imgPreview;
        private readonly TMP_Text m_textPreviewInitials;
        private readonly TMP_Text m_textName;
        private readonly TMP_Text m_textType;
        private readonly TMP_Text m_textAlignment;
        private readonly TMP_Text m_textSelectedBadge;

        public ChapterCreatureCardWidget(GameObject gameObject)
        {
            m_gameObject = gameObject;
            m_button = gameObject.GetComponent<Button>();
            m_imgBackground = gameObject.GetComponent<Image>();
            m_imgPreview = FindChildComponent<Image>(gameObject.transform, "m_goPreview");
            m_textPreviewInitials = FindChildComponent<TMP_Text>(gameObject.transform, "m_tmpPreviewInitials");
            m_textName = FindChildComponent<TMP_Text>(gameObject.transform, "m_tmpName");
            m_textType = FindChildComponent<TMP_Text>(gameObject.transform, "m_tmpType");
            m_textAlignment = FindChildComponent<TMP_Text>(gameObject.transform, "m_tmpAlignment");
            m_textSelectedBadge = FindChildComponent<TMP_Text>(gameObject.transform, "m_tmpSelectedBadge");
        }

        public void SetVisible(bool visible)
        {
            m_gameObject.SetActive(visible);
        }

        public void Bind(ChapterCreatureStaticCardData creature, bool selected, Action onClick)
        {
            if (m_button != null)
            {
                m_button.onClick.RemoveAllListeners();
                m_button.onClick.AddListener(() => onClick?.Invoke());
            }

            if (m_imgBackground != null)
            {
                m_imgBackground.color = selected ? SelectedCardColor : DefaultCardColor;
            }

            if (m_imgPreview != null)
            {
                m_imgPreview.color = creature.AccentColor;
            }

            if (m_textPreviewInitials != null)
            {
                m_textPreviewInitials.text = ChapterCreatureWidgetUtility.GetCreatureInitials(creature.Name);
            }

            if (m_textName != null)
            {
                m_textName.text = creature.Name;
            }

            if (m_textType != null)
            {
                m_textType.text = creature.CreatureType;
            }

            if (m_textAlignment != null)
            {
                m_textAlignment.text = creature.Alignment;
            }

            if (m_textSelectedBadge != null)
            {
                m_textSelectedBadge.gameObject.SetActive(false);
            }
        }

        public void Dispose()
        {
            Object.Destroy(m_gameObject);
        }

        private static T FindChildComponent<T>(Transform root, string path) where T : Component
        {
            Transform child = root.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
    }

    internal sealed class ChapterCreatureDetailWidget
    {
        private readonly GameObject m_gameObject;
        private readonly TMP_Text m_textTitle;
        private readonly TMP_Text m_textMeta;
        private readonly Image m_imgPreview;
        private readonly TMP_Text m_textPreviewInitials;
        private readonly TMP_Text m_textBody;

        public ChapterCreatureDetailWidget(GameObject gameObject)
        {
            m_gameObject = gameObject;
            m_textTitle = FindChildComponent<TMP_Text>(gameObject.transform, "m_tmpDetailTitle");
            m_textMeta = FindChildComponent<TMP_Text>(gameObject.transform, "m_tmpDetailMeta");
            m_imgPreview = FindChildComponent<Image>(gameObject.transform, "m_goDetailPreview");
            m_textPreviewInitials = FindChildComponent<TMP_Text>(gameObject.transform, "m_tmpDetailPreviewInitials");
            m_textBody = FindChildComponent<TMP_Text>(gameObject.transform, "m_tmpDetailBody");
        }

        public void SetVisible(bool visible)
        {
            m_gameObject.SetActive(visible);
        }

        public void Bind(ChapterCreatureStaticCardData creature)
        {
            if (m_textTitle != null)
            {
                m_textTitle.text = creature.Name;
            }

            if (m_textMeta != null)
            {
                m_textMeta.text = $"{creature.CreatureType}  |  {creature.Alignment}";
            }

            if (m_imgPreview != null)
            {
                m_imgPreview.color = new Color(creature.AccentColor.r * 0.8f, creature.AccentColor.g * 0.8f, creature.AccentColor.b * 0.8f, 1f);
            }

            if (m_textPreviewInitials != null)
            {
                m_textPreviewInitials.text = ChapterCreatureWidgetUtility.GetCreatureInitials(creature.Name);
            }

            if (m_textBody != null)
            {
                m_textBody.text = creature.Summary + "\n\n当前版本仅用于静态展示卡片布局、预览图占位、怪物类型与阵营信息层次。下一阶段将接入真实生物数据、卡片选择和右侧详情同步刷新。";
            }
        }

        public void Dispose()
        {
            Object.Destroy(m_gameObject);
        }

        private static T FindChildComponent<T>(Transform root, string path) where T : Component
        {
            Transform child = root.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
    }

    internal static class ChapterCreatureWidgetUtility
    {
        public static string GetCreatureInitials(string creatureName)
        {
            if (string.IsNullOrWhiteSpace(creatureName))
            {
                return "DND";
            }

            string[] parts = creatureName.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                string value = parts[0];
                return value.Length <= 3 ? value.ToUpperInvariant() : value.Substring(0, 3).ToUpperInvariant();
            }

            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }
    }
}