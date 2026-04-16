using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameLogic
{
    internal sealed class ChapterListItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private RectTransform m_rectTransform = null!;
        private Canvas m_canvas = null!;
        private RectTransform m_contentRect = null!;
        private int m_itemIndex;
        private int m_itemCount;
        private float m_itemHeight;
        private float m_itemSpacing;
        private Vector2 m_originAnchoredPosition;
        private Vector2 m_latestPointerPosition;
        private int m_currentTargetIndex;
        private bool m_isDragging;
        private Camera m_dragEventCamera = null!;

        public Action<int, int>? OnItemDropped;
        public Action<int, int>? OnDragPreviewChanged;
        public Action<int, bool>? OnDragStateChanged;

        private void Awake()
        {
            m_rectTransform = GetComponent<RectTransform>();
        }

        public void Configure(
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
            m_canvas = canvas;
            m_contentRect = contentRect;
            m_itemIndex = itemIndex;
            m_itemCount = itemCount;
            m_itemHeight = itemHeight;
            m_itemSpacing = itemSpacing;
            m_currentTargetIndex = itemIndex;
            OnItemDropped = onItemDropped;
            OnDragPreviewChanged = onDragPreviewChanged;
            OnDragStateChanged = onDragStateChanged;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (m_itemCount <= 1)
            {
                return;
            }

            m_originAnchoredPosition = m_rectTransform.anchoredPosition;
            m_latestPointerPosition = eventData.position;
            m_dragEventCamera = eventData.pressEventCamera;
            m_isDragging = true;
            m_currentTargetIndex = m_itemIndex;
            m_rectTransform.SetAsLastSibling();
            OnDragStateChanged?.Invoke(m_itemIndex, true);
            OnDragPreviewChanged?.Invoke(m_itemIndex, m_itemIndex);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!m_isDragging || m_canvas == null)
            {
                return;
            }

            Vector2 anchoredPosition = m_rectTransform.anchoredPosition;
            anchoredPosition.y += eventData.delta.y / Mathf.Max(m_canvas.scaleFactor, 0.01f);
            m_rectTransform.anchoredPosition = anchoredPosition;

            m_latestPointerPosition = eventData.position;
            m_currentTargetIndex = CalculateTargetIndex(m_latestPointerPosition);
            OnDragPreviewChanged?.Invoke(m_itemIndex, m_currentTargetIndex);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!m_isDragging)
            {
                return;
            }

            m_isDragging = false;

            int targetIndex = m_currentTargetIndex;
            m_rectTransform.anchoredPosition = m_originAnchoredPosition;
            OnDragStateChanged?.Invoke(m_itemIndex, false);
            OnItemDropped?.Invoke(m_itemIndex, targetIndex);
        }

        private int CalculateTargetIndex(Vector2 pointerScreenPosition)
        {
            if (m_contentRect == null)
            {
                return m_itemIndex;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(m_contentRect, pointerScreenPosition, m_dragEventCamera, out Vector2 localPoint))
            {
                return m_itemIndex;
            }

            float itemStep = Mathf.Max(1f, m_itemHeight + m_itemSpacing);
            float distanceFromTop = m_contentRect.rect.yMax - localPoint.y;
            float offsetDistance = distanceFromTop - m_itemHeight * 0.5f;
            int targetIndex = Mathf.RoundToInt(offsetDistance / itemStep);
            return Mathf.Clamp(targetIndex, 0, Mathf.Max(0, m_itemCount - 1));
        }
    }
}