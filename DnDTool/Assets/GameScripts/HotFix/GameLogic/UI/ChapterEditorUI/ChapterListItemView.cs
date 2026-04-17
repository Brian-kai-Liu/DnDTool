using System;
using TMPro;
using TEngine;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GameLogic
{
    internal sealed class ChapterListItemView
    {
        private static readonly Color NormalColor = new Color(0.95f, 0.94f, 0.9f, 1f);
        private static readonly Color SelectedColor = new Color(0.84f, 0.89f, 0.97f, 1f);
        private static readonly Color DraggingColor = new Color(0.98f, 0.82f, 0.52f, 0.92f);
        private static readonly Color NormalTextColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        private static readonly Color SelectedTextColor = new Color(0.1f, 0.23f, 0.45f, 1f);

        private readonly GameObject m_gameObject;
        private readonly RectTransform m_rectTransform;
        private readonly Button m_button;
        private readonly TMP_InputField m_inputTitle;
        private readonly TMP_Text m_textChapterIndex;
        private readonly Image m_background;
        private readonly ChapterListItemDragHandler m_dragHandler;
        private readonly CanvasGroup m_canvasGroup;
        private readonly Button m_deleteButton;
        private bool m_isSelected;
        private bool m_isDragging;

        public ChapterListItemView(GameObject gameObject)
        {
            m_gameObject = gameObject;
            m_rectTransform = gameObject.GetComponent<RectTransform>();

            m_button = gameObject.GetComponent<Button>();
            m_inputTitle = FindChildComponent<TMP_InputField>(gameObject.transform, "m_tmpInputChapterName");
            m_textChapterIndex = FindChildComponent<TextMeshProUGUI>(gameObject.transform, "Text");
            m_deleteButton = FindChildComponent<Button>(gameObject.transform, "m_btnDeleteChapter");
            m_background = m_button != null ? m_button.targetGraphic as Image : gameObject.GetComponent<Image>();
            m_dragHandler = gameObject.GetComponent<ChapterListItemDragHandler>() ?? gameObject.AddComponent<ChapterListItemDragHandler>();
            m_canvasGroup = gameObject.GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

            if (m_button == null || m_inputTitle == null || m_textChapterIndex == null)
            {
                Log.Error("ChapterListItemView 绑定失败，请检查 ChapterEditorUI 列表项模板节点命名。需要根节点 Button、m_tmpInputChapterName、Text。");
            }
        }

        public void SetVisible(bool visible)
        {
            m_gameObject.SetActive(visible);
        }

        public void SetLayout(int index, float itemHeight, float itemSpacing)
        {
            m_rectTransform.anchorMin = new Vector2(0f, 1f);
            m_rectTransform.anchorMax = new Vector2(1f, 1f);
            m_rectTransform.pivot = new Vector2(0.5f, 1f);
            m_rectTransform.anchoredPosition = new Vector2(0f, -index * (itemHeight + itemSpacing));
            m_rectTransform.sizeDelta = new Vector2(0f, itemHeight);
            m_rectTransform.localScale = Vector3.one;
        }

        public void Bind(ChapterListItemData data, int chapterIndex, bool isSelected, Action onClick, Action onDelete, Action<string> onTitleEditEnd)
        {
            if (m_button == null || m_inputTitle == null || m_textChapterIndex == null)
            {
                return;
            }

            m_textChapterIndex.text = $"第{chapterIndex}章";
            RefreshSelectionVisual(isSelected);

            m_button.onClick.RemoveAllListeners();
            m_button.onClick.AddListener(() => onClick?.Invoke());

            if (m_deleteButton != null)
            {
                m_deleteButton.onClick.RemoveAllListeners();
                m_deleteButton.onClick.AddListener(() => onDelete?.Invoke());
            }

            m_inputTitle.onEndEdit.RemoveAllListeners();
            m_inputTitle.onEndEdit.AddListener(value => onTitleEditEnd?.Invoke(value));

            m_inputTitle.text = data.Name ?? string.Empty;
        }

        public void ConfigureDrag(
            Canvas canvas,
            RectTransform contentRect,
            int itemIndex,
            int itemCount,
            float itemHeight,
            float itemSpacing,
            Action<int, int> onItemDropped,
            Action<int, int> onDragPreviewChanged,
            Action<int, bool> onDragStateChanged)
        {
            m_dragHandler.Configure(
                canvas,
                contentRect,
                itemIndex,
                itemCount,
                itemHeight,
                itemSpacing,
                onItemDropped,
                onDragPreviewChanged,
                onDragStateChanged);
        }

        public void SetDragState(bool isDragging)
        {
            m_isDragging = isDragging;

            if (m_canvasGroup != null)
            {
                m_canvasGroup.alpha = isDragging ? 0.92f : 1f;
                m_canvasGroup.blocksRaycasts = !isDragging;
            }

            ApplyVisualState();

            if (m_deleteButton != null)
            {
                m_deleteButton.interactable = !isDragging;
            }
        }

        public void SetDeleteInteractable(bool interactable)
        {
            if (m_deleteButton != null)
            {
                m_deleteButton.interactable = interactable;
            }
        }

        public void SyncToData(ChapterListItemData data)
        {
            if (data == null || m_inputTitle == null)
            {
                return;
            }

            data.Name = m_inputTitle.text?.Trim() ?? string.Empty;
        }

        public void Dispose()
        {
            if (m_gameObject != null)
            {
                Object.Destroy(m_gameObject);
            }
        }

        private void RefreshSelectionVisual(bool isSelected)
        {
            m_isSelected = isSelected;
            ApplyVisualState();

            if (m_textChapterIndex != null)
            {
                m_textChapterIndex.color = isSelected ? SelectedTextColor : NormalTextColor;
                m_textChapterIndex.fontStyle = isSelected ? FontStyles.Bold : FontStyles.Normal;
            }

            if (m_inputTitle != null && m_inputTitle.textComponent != null)
            {
                m_inputTitle.textComponent.color = isSelected ? SelectedTextColor : NormalTextColor;
                m_inputTitle.textComponent.fontStyle = isSelected ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        private void ApplyVisualState()
        {
            if (m_background == null)
            {
                return;
            }

            if (m_isDragging)
            {
                m_background.color = DraggingColor;
                return;
            }

            m_background.color = m_isSelected ? SelectedColor : NormalColor;
        }

        private static T FindChildComponent<T>(Transform root, string path) where T : Component
        {
            if (root == null || string.IsNullOrEmpty(path))
            {
                return null;
            }

            Transform child = root.Find(path);
            if (child == null)
            {
                return null;
            }

            return child.GetComponent<T>();
        }

    }
}