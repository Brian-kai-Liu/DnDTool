using System;
using System.Collections.Generic;
using TEngine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GameLogic
{
    internal sealed class AdventureHookCardData
    {
        public string Target { get; set; } = string.Empty;

        public string HookContent { get; set; } = string.Empty;
    }

    internal sealed class AdventureHookCardView
    {
        private readonly GameObject m_gameObject;
        private readonly RectTransform m_rectTransform;
        private readonly TMP_InputField m_tmpInputTarget;
        private readonly TMP_InputField m_tmpInputHookContent;
        private readonly Button m_btnRemove;

        public AdventureHookCardView(GameObject gameObject)
        {
            m_gameObject = gameObject;
            m_rectTransform = gameObject.GetComponent<RectTransform>();

            UIBindComponent bindComponent = gameObject.GetComponent<UIBindComponent>();
            m_tmpInputTarget = GetRequiredComponent(
                bindComponent != null ? bindComponent.GetComponent<TMP_InputField>(0) : null,
                gameObject.transform,
                "m_tmpInputTarget",
                "m_inputTarget");
            m_tmpInputHookContent = GetRequiredComponent(
                FindChildComponentByName<TMP_InputField>(gameObject.transform, "m_tmpInputHookContent")
                ?? FindChildComponentByName<TMP_InputField>(gameObject.transform, "m_tmpInputHookContentLegacy")
                ?? FindChildComponentByName<TMP_InputField>(gameObject.transform, "m_inputHookContent"),
                gameObject.transform,
                "m_tmpInputHookContent",
                "m_tmpInputHookContentLegacy",
                "m_inputHookContent");
            m_btnRemove = GetRequiredComponent(bindComponent != null ? bindComponent.GetComponent<Button>(2) : null, gameObject.transform, "m_btnRemoveAdventureHookCard");
        }

        public void SetVisible(bool visible)
        {
            m_gameObject.SetActive(visible);
        }

        public void SetLayout(int index, float cardHeight, float cardSpacing)
        {
            m_rectTransform.anchorMin = new Vector2(0f, 1f);
            m_rectTransform.anchorMax = new Vector2(1f, 1f);
            m_rectTransform.pivot = new Vector2(0.5f, 1f);
            m_rectTransform.anchoredPosition = new Vector2(0f, -index * (cardHeight + cardSpacing));
            m_rectTransform.sizeDelta = new Vector2(0f, cardHeight);
            m_rectTransform.localScale = Vector3.one;
        }

        public void Bind(AdventureHookCardData data, Action<AdventureHookCardData> onRemove)
        {
            m_btnRemove.onClick.RemoveAllListeners();

            m_tmpInputTarget.text = data.Target ?? string.Empty;
            m_tmpInputHookContent.text = data.HookContent ?? string.Empty;

            m_btnRemove.onClick.AddListener(() => onRemove?.Invoke(data));
        }

        public void SyncToData(AdventureHookCardData data)
        {
            if (data == null)
            {
                return;
            }

            data.Target = m_tmpInputTarget != null ? m_tmpInputTarget.text?.Trim() ?? string.Empty : string.Empty;
            data.HookContent = m_tmpInputHookContent != null ? m_tmpInputHookContent.text?.Trim() ?? string.Empty : string.Empty;
        }

        public void Dispose()
        {
            if (m_gameObject != null)
            {
                Object.Destroy(m_gameObject);
            }
        }

        private static T GetRequiredComponent<T>(T component, Transform root, params string[] childNames) where T : Component
        {
            if (component != null)
            {
                return component;
            }

            for (int index = 0; index < childNames.Length; index++)
            {
                T fallbackComponent = FindChildComponentByName<T>(root, childNames[index]);
                if (fallbackComponent != null)
                {
                    return fallbackComponent;
                }
            }

            throw new MissingReferenceException($"在冒险引子卡模板中未找到组件: {string.Join(" / ", childNames)} ({typeof(T).Name})");
        }

        private static T FindChildComponentByName<T>(Transform root, string childName) where T : Component
        {
            if (root == null)
            {
                return null;
            }

            Queue<Transform> transforms = new Queue<Transform>();
            transforms.Enqueue(root);
            while (transforms.Count > 0)
            {
                Transform current = transforms.Dequeue();
                if (current.name == childName)
                {
                    return current.GetComponent<T>();
                }

                for (int index = 0; index < current.childCount; index++)
                {
                    transforms.Enqueue(current.GetChild(index));
                }
            }

            return null;
        }
    }
}