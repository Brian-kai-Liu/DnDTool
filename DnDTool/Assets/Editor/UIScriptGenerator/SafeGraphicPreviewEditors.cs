#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    [CustomEditor(typeof(Image), true)]
    [CanEditMultipleObjects]
    internal sealed class SafeImagePreviewEditor : ImageEditor
    {
        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            if (!IsValidPreviewRect(rect))
            {
                return;
            }

            Image image = target as Image;
            if (image == null || image.sprite == null)
            {
                return;
            }

            base.OnPreviewGUI(rect, background);
        }

        private static bool IsValidPreviewRect(Rect rect)
        {
            return rect.width > 1f && rect.height > 1f && !float.IsNaN(rect.width) && !float.IsNaN(rect.height)
                && !float.IsInfinity(rect.width) && !float.IsInfinity(rect.height);
        }
    }

    [CustomEditor(typeof(RawImage), true)]
    [CanEditMultipleObjects]
    internal sealed class SafeRawImagePreviewEditor : RawImageEditor
    {
        public override bool HasPreviewGUI()
        {
            RawImage rawImage = target as RawImage;
            if (rawImage == null || rawImage.rectTransform == null)
            {
                return false;
            }

            Rect targetRect = rawImage.rectTransform.rect;
            if (targetRect.width <= 0f || targetRect.height <= 0f)
            {
                return false;
            }

            return base.HasPreviewGUI();
        }

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            if (!SafeImagePreviewEditorReflection.IsValidPreviewRect(rect))
            {
                return;
            }

            base.OnPreviewGUI(rect, background);
        }
    }

    internal static class SafeImagePreviewEditorReflection
    {
        public static bool IsValidPreviewRect(Rect rect)
        {
            return rect.width > 1f && rect.height > 1f && !float.IsNaN(rect.width) && !float.IsNaN(rect.height)
                && !float.IsInfinity(rect.width) && !float.IsInfinity(rect.height);
        }
    }
}

#endif