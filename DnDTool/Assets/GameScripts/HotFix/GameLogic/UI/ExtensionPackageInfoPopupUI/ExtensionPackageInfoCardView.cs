using System;
using TMPro;
using TEngine;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    public sealed class ExtensionPackageInfoSelectionData
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Subtitle { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string PreviewImagePath { get; set; } = string.Empty;
    }

    internal sealed class ExtensionPackageInfoOption
    {
        public ExtensionPackageInfoOption(string id, string name, string subtitle, string description, string previewImagePath)
        {
            Id = id;
            Name = name;
            Subtitle = subtitle;
            Description = description;
            PreviewImagePath = previewImagePath;
        }

        public string Id { get; }

        public string Name { get; }

        public string Subtitle { get; }

        public string Description { get; }

        public string PreviewImagePath { get; }

        public ExtensionPackageInfoSelectionData ToSelectionData()
        {
            return new ExtensionPackageInfoSelectionData
            {
                Id = Id,
                Name = Name,
                Subtitle = Subtitle,
                Description = Description,
                PreviewImagePath = PreviewImagePath
            };
        }
    }

    internal sealed class ExtensionPackageInfoCardView
    {
        private static readonly Color NormalCardColor = new Color(0.93f, 0.92f, 0.88f, 1f);
        private static readonly Color SelectedCardColor = new Color(0.83f, 0.90f, 0.98f, 1f);

        private readonly GameObject m_gameObject;
        private readonly RectTransform m_rectTransform;
        private readonly Image m_background;
        private readonly Toggle m_toggleSelect;
        private readonly Image m_imgPreview;
        private readonly TextMeshProUGUI m_tmpPreviewPlaceholder;
        private readonly TextMeshProUGUI m_tmpPackageName;
        private readonly TextMeshProUGUI m_tmpPackageSubtitle;
        private readonly TextMeshProUGUI m_tmpPackageDescription;
        private readonly TextMeshProUGUI m_tmpSelectionState;

        public ExtensionPackageInfoCardView(GameObject gameObject)
        {
            m_gameObject = gameObject;
            m_rectTransform = gameObject.GetComponent<RectTransform>();

            UIBindComponent bindComponent = gameObject.GetComponent<UIBindComponent>();
            m_toggleSelect = bindComponent.GetComponent<Toggle>(0);
            m_imgPreview = bindComponent.GetComponent<Image>(1);
            m_tmpPreviewPlaceholder = bindComponent.GetComponent<TextMeshProUGUI>(2);
            m_tmpPackageName = bindComponent.GetComponent<TextMeshProUGUI>(3);
            m_tmpPackageSubtitle = bindComponent.GetComponent<TextMeshProUGUI>(4);
            m_tmpPackageDescription = bindComponent.GetComponent<TextMeshProUGUI>(5);
            m_tmpSelectionState = bindComponent.GetComponent<TextMeshProUGUI>(6);
            m_background = m_toggleSelect.targetGraphic as Image;
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

        public void Bind(ExtensionPackageInfoOption option, bool isSelected, Action<string, bool> onSelectionChanged)
        {
            m_tmpPackageName.text = option.Name;
            m_tmpPackageSubtitle.text = option.Subtitle;
            m_tmpPackageDescription.text = option.Description;

            if (string.IsNullOrWhiteSpace(option.PreviewImagePath))
            {
                m_imgPreview.sprite = null;
                m_tmpPreviewPlaceholder.gameObject.SetActive(true);
            }
            else
            {
                m_tmpPreviewPlaceholder.gameObject.SetActive(false);
                m_imgPreview.SetSprite(option.PreviewImagePath);
            }

            m_toggleSelect.onValueChanged.RemoveAllListeners();
            m_toggleSelect.isOn = isSelected;
            RefreshSelectionVisual(isSelected);
            m_toggleSelect.onValueChanged.AddListener(selected =>
            {
                RefreshSelectionVisual(selected);
                onSelectionChanged?.Invoke(option.Id, selected);
            });
        }

        public void Dispose()
        {
            if (m_gameObject != null)
            {
                UnityEngine.Object.Destroy(m_gameObject);
            }
        }

        private void RefreshSelectionVisual(bool isSelected)
        {
            if (m_background != null)
            {
                m_background.color = isSelected ? SelectedCardColor : NormalCardColor;
            }

            if (m_tmpSelectionState != null)
            {
                m_tmpSelectionState.text = isSelected ? "已选择" : "点击选择";
                m_tmpSelectionState.color = isSelected
                    ? new Color(0.11f, 0.38f, 0.66f, 1f)
                    : new Color(0.46f, 0.46f, 0.46f, 1f);
            }
        }
    }
}