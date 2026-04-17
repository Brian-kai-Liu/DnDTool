using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

namespace GameLogic
{
    internal static class PopupWindowPresentationHelper
    {
        private const string DefaultPanelPath = "Panel";
        private const string DefaultTitlePath = "Panel/m_tmpTitle";

        public static void Configure(UIWindow window, string panelPath = DefaultPanelPath, string dragHandlePath = DefaultTitlePath)
        {
            if (window?.gameObject == null)
            {
                return;
            }

            DisableRootMask(window.gameObject);

            RectTransform panelRect = window.FindChildComponent<RectTransform>(panelPath);
            if (panelRect == null)
            {
                return;
            }

            if (panelRect.GetComponent<PopupWindowInputBlocker>() == null)
            {
                panelRect.gameObject.AddComponent<PopupWindowInputBlocker>();
            }

            RectTransform dragHandleRect = window.FindChildComponent<RectTransform>(dragHandlePath);
            if (dragHandleRect == null)
            {
                dragHandleRect = panelRect;
            }

            EnsureDragHandleGraphic(dragHandleRect);

            PopupWindowDragHandler dragHandler = dragHandleRect.GetComponent<PopupWindowDragHandler>();
            if (dragHandler == null)
            {
                dragHandler = dragHandleRect.gameObject.AddComponent<PopupWindowDragHandler>();
            }

            dragHandler.Configure(panelRect, window.rectTransform);
        }

        private static void DisableRootMask(GameObject windowObject)
        {
            Image maskImage = windowObject.GetComponent<Image>();
            if (maskImage == null)
            {
                return;
            }

            maskImage.raycastTarget = false;
            maskImage.enabled = false;
        }

        private static void EnsureDragHandleGraphic(RectTransform dragHandleRect)
        {
            if (dragHandleRect == null)
            {
                return;
            }

            Graphic graphic = dragHandleRect.GetComponent<Graphic>();
            if (graphic != null)
            {
                graphic.raycastTarget = true;
                return;
            }

            Image image = dragHandleRect.gameObject.AddComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = true;
        }
    }

    internal sealed class PopupWindowInputBlocker : MonoBehaviour
    {
        private static readonly List<RaycastResult> RaycastResults = new List<RaycastResult>(16);

        private static bool s_isPointerCapturedByPopup;

        public static bool ShouldBlockUnderlyingPointerInput()
        {
            UpdatePointerCaptureState();
            return s_isPointerCapturedByPopup || IsPointerOverPopupContent();
        }

        public static void CapturePointer()
        {
            s_isPointerCapturedByPopup = true;
        }

        public static void ReleasePointer()
        {
            s_isPointerCapturedByPopup = false;
        }

        private static void UpdatePointerCaptureState()
        {
            if (Input.GetMouseButtonUp(0))
            {
                s_isPointerCapturedByPopup = false;
                return;
            }

            if (Input.GetMouseButtonDown(0) && IsPointerOverPopupContent())
            {
                s_isPointerCapturedByPopup = true;
                return;
            }

            if (!Input.GetMouseButton(0) && !Input.GetMouseButtonDown(0))
            {
                s_isPointerCapturedByPopup = false;
            }
        }

        private static bool IsPointerOverPopupContent()
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            RaycastResults.Clear();
            EventSystem.current.RaycastAll(pointerEventData, RaycastResults);
            for (int index = 0; index < RaycastResults.Count; index++)
            {
                RaycastResult raycastResult = RaycastResults[index];
                if (raycastResult.gameObject == null)
                {
                    continue;
                }

                if (raycastResult.gameObject.GetComponentInParent<PopupWindowInputBlocker>() != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnDisable()
        {
            s_isPointerCapturedByPopup = false;
        }
    }

    internal sealed class PopupWindowDragHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {

        private RectTransform m_targetRect = null!;
        private RectTransform m_boundsRect = null!;
        private Camera m_dragCamera = null!;
        private Vector2 m_dragOffset;

        public void Configure(RectTransform targetRect, RectTransform boundsRect)
        {
            m_targetRect = targetRect;
            m_boundsRect = boundsRect;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            PopupWindowInputBlocker.CapturePointer();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            PopupWindowInputBlocker.ReleasePointer();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (m_targetRect == null)
            {
                return;
            }

            PopupWindowInputBlocker.CapturePointer();
            m_dragCamera = eventData.pressEventCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                m_targetRect,
                eventData.position,
                m_dragCamera,
                out m_dragOffset);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (m_targetRect == null || m_boundsRect == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    m_boundsRect,
                    eventData.position,
                    m_dragCamera,
                    out Vector2 localPoint))
            {
                return;
            }

            Vector2 nextAnchoredPosition = localPoint - m_dragOffset;
            m_targetRect.anchoredPosition = ClampToBounds(nextAnchoredPosition);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            PopupWindowInputBlocker.ReleasePointer();
        }

        private void OnDisable()
        {
            PopupWindowInputBlocker.ReleasePointer();
        }

        private Vector2 ClampToBounds(Vector2 anchoredPosition)
        {
            Rect boundsRect = m_boundsRect.rect;
            Rect targetRect = m_targetRect.rect;
            Vector2 pivot = m_targetRect.pivot;

            float minX = boundsRect.xMin + targetRect.width * pivot.x;
            float maxX = boundsRect.xMax - targetRect.width * (1f - pivot.x);
            float minY = boundsRect.yMin + targetRect.height * pivot.y;
            float maxY = boundsRect.yMax - targetRect.height * (1f - pivot.y);

            if (minX > maxX)
            {
                float centerX = (minX + maxX) * 0.5f;
                minX = centerX;
                maxX = centerX;
            }

            if (minY > maxY)
            {
                float centerY = (minY + maxY) * 0.5f;
                minY = centerY;
                maxY = centerY;
            }

            anchoredPosition.x = Mathf.Clamp(anchoredPosition.x, minX, maxX);
            anchoredPosition.y = Mathf.Clamp(anchoredPosition.y, minY, maxY);
            return anchoredPosition;
        }
    }
}