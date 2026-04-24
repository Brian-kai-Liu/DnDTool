using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using TMPro;
using TEngine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GameLogic
{
    internal readonly struct ChapterCreatureStaticCardData
    {
        // 用于静态种子数据（无结构化来源）
        public ChapterCreatureStaticCardData(string name, string creatureType, string alignment, Color accentColor, string summary)
        {
            Source = null;
            Name = name;
            NameEn = string.Empty;
            CreatureType = creatureType;
            Alignment = alignment;
            AccentColor = accentColor;
            Summary = summary;
            PreviewImageFileName = string.Empty;
        }

        // 用于从结构化数据构建的运行时卡片
        public ChapterCreatureStaticCardData(ChapterCreatureData source)
        {
            Source = source;
            Name = source.Name;
            NameEn = source.NameEn;
            CreatureType = source.GetCreatureTypeDisplay();
            Alignment = source.Alignment;
            AccentColor = source.AccentColor;
            Summary = source.BuildSummary();
            PreviewImageFileName = source.PreviewImageFileName;
        }

        public ChapterCreatureData Source { get; }

        public string Name { get; }

        public string NameEn { get; }

        public string CreatureType { get; }

        public string Alignment { get; }

        public Color AccentColor { get; }

        public string Summary { get; }

        public string PreviewImageFileName { get; }
    }

    internal sealed class ChapterCreatureCardDragProxy : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public bool IsDragEnabled { get; set; }

        public Action<Vector2> OnBeginDragAction { get; set; }

        public Action<Vector2> OnDragAction { get; set; }

        public Action<Vector2> OnEndDragAction { get; set; }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsDragEnabled)
            {
                return;
            }

            OnBeginDragAction?.Invoke(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsDragEnabled)
            {
                return;
            }

            OnDragAction?.Invoke(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!IsDragEnabled)
            {
                return;
            }

            OnEndDragAction?.Invoke(eventData.position);
        }
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
        private readonly Button m_btnEdit;
        private readonly ChapterCreatureCardDragProxy m_dragProxy;
        private Texture2D m_previewTexture;
        private Sprite m_previewSprite;
        private string m_loadedPreviewPath = string.Empty;
        private int m_previewLoadVersion;

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
            m_btnEdit = FindChildComponent<Button>(gameObject.transform, "m_btnEditCreature");
            m_dragProxy = gameObject.GetComponent<ChapterCreatureCardDragProxy>() ?? gameObject.AddComponent<ChapterCreatureCardDragProxy>();
        }

        public void SetVisible(bool visible)
        {
            m_gameObject.SetActive(visible);
        }

        public void Bind(ChapterCreatureStaticCardData creature, bool selected, Action onClick, Action onEdit, Action<Vector2> onBeginDrag, Action<Vector2> onDrag, Action<Vector2> onEndDrag)
        {
            if (m_button != null)
            {
                m_button.onClick.RemoveAllListeners();
                m_button.onClick.AddListener(() => onClick?.Invoke());
            }

            if (m_btnEdit != null)
            {
                bool canEdit = creature.Source != null && onEdit != null;
                m_btnEdit.gameObject.SetActive(canEdit);
                m_btnEdit.onClick.RemoveAllListeners();
                if (canEdit)
                {
                    m_btnEdit.onClick.AddListener(() => onEdit.Invoke());
                }
            }

            if (m_dragProxy != null)
            {
                bool canDrag = creature.Source != null && onBeginDrag != null && onEndDrag != null;
                m_dragProxy.IsDragEnabled = canDrag;
                m_dragProxy.OnBeginDragAction = canDrag ? onBeginDrag : null;
                m_dragProxy.OnDragAction = canDrag ? onDrag : null;
                m_dragProxy.OnEndDragAction = canDrag ? onEndDrag : null;
            }

            if (m_imgBackground != null)
            {
                m_imgBackground.color = selected ? SelectedCardColor : DefaultCardColor;
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

            RefreshPreview(creature);
        }

        public void Dispose()
        {
            CleanupPreviewResources();
            Object.Destroy(m_gameObject);
        }

        private void RefreshPreview(ChapterCreatureStaticCardData creature)
        {
            string previewPath = ChapterEditorPersistenceService.ResolveCreaturePreviewPath(creature.PreviewImageFileName);
            if (!string.IsNullOrWhiteSpace(previewPath)
                && string.Equals(m_loadedPreviewPath, previewPath, StringComparison.OrdinalIgnoreCase)
                && m_previewSprite != null)
            {
                SetPreviewSprite(m_previewSprite);
                return;
            }

            ResetPreview(creature);
            if (string.IsNullOrWhiteSpace(previewPath))
            {
                return;
            }

            int loadVersion = ++m_previewLoadVersion;
            LoadPreviewFromPathAsync(previewPath, loadVersion).Forget();
        }

        private void ResetPreview(ChapterCreatureStaticCardData creature)
        {
            ++m_previewLoadVersion;
            CleanupPreviewResources();

            if (m_imgPreview != null)
            {
                m_imgPreview.sprite = null;
                m_imgPreview.preserveAspect = true;
                m_imgPreview.color = creature.AccentColor;
            }

            if (m_textPreviewInitials != null)
            {
                m_textPreviewInitials.gameObject.SetActive(true);
                m_textPreviewInitials.text = ChapterCreatureWidgetUtility.GetCreatureInitials(creature.Name);
            }
        }

        public void SetPreviewSprite(Sprite sprite)
        {
            if (m_imgPreview != null)
            {
                m_imgPreview.sprite = sprite;
                m_imgPreview.preserveAspect = true;
                m_imgPreview.color = sprite != null ? Color.white : m_imgPreview.color;
            }

            if (m_textPreviewInitials != null)
            {
                m_textPreviewInitials.gameObject.SetActive(sprite == null);
            }
        }

        private async UniTaskVoid LoadPreviewFromPathAsync(string previewPath, int loadVersion)
        {
            byte[] imageBytes;
            try
            {
                imageBytes = await UniTask.RunOnThreadPool(() => File.ReadAllBytes(previewPath));
            }
            catch (Exception exception)
            {
                Log.Warning($"读取生物卡片预览图失败: {exception.Message}");
                return;
            }

            if (loadVersion != m_previewLoadVersion)
            {
                return;
            }

            Texture2D previewTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!previewTexture.LoadImage(imageBytes))
            {
                Object.Destroy(previewTexture);
                Log.Warning("生物卡片预览图加载失败，文件不是有效图片。");
                return;
            }

            Sprite previewSprite = Sprite.Create(
                previewTexture,
                new Rect(0, 0, previewTexture.width, previewTexture.height),
                new Vector2(0.5f, 0.5f));

            if (loadVersion != m_previewLoadVersion)
            {
                Object.Destroy(previewSprite);
                Object.Destroy(previewTexture);
                return;
            }

            CleanupPreviewResources();
            m_previewTexture = previewTexture;
            m_previewSprite = previewSprite;
            m_loadedPreviewPath = previewPath;
            SetPreviewSprite(previewSprite);
        }

        private void CleanupPreviewResources()
        {
            m_loadedPreviewPath = string.Empty;

            if (m_previewSprite != null)
            {
                Object.Destroy(m_previewSprite);
                m_previewSprite = null;
            }

            if (m_previewTexture != null)
            {
                Object.Destroy(m_previewTexture);
                m_previewTexture = null;
            }
        }

        private static T FindChildComponent<T>(Transform root, string path) where T : Component
        {
            Transform child = root.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
    }

    internal sealed class ChapterCreatureMapTokenWidget
    {
        private readonly GameObject m_gameObject;
        private readonly RectTransform m_rectTransform;
        private readonly Image m_imgFrame;
        private readonly Image m_imgPreview;
        private readonly TMP_Text m_textLabel;
        private Texture2D m_previewTexture;
        private Sprite m_previewSprite;
        private string m_loadedPreviewPath = string.Empty;
        private int m_previewLoadVersion;

        public ChapterCreatureMapTokenWidget(GameObject gameObject, TMP_Text styleSource = null)
        {
            m_gameObject = gameObject;
            m_rectTransform = gameObject.GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            m_imgFrame = gameObject.GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            m_imgPreview = EnsurePreviewImage(gameObject.transform);
            m_textLabel = EnsureLabel(gameObject.transform, styleSource);
            m_imgFrame.raycastTarget = false;
        }

        public void SetVisible(bool visible)
        {
            m_gameObject.SetActive(visible);
        }

        public void Dispose()
        {
            ++m_previewLoadVersion;
            CleanupPreviewResources();

            if (m_gameObject != null)
            {
                Object.Destroy(m_gameObject);
            }
        }

        public void Bind(ChapterCreatureData creature, Vector2 anchoredPosition, Vector2 size, bool ghosted, bool selected)
        {
            creature = ChapterCreatureDataStructureUtility.NormalizeCreatureTemplateData(creature);
            if (creature == null)
            {
                SetVisible(false);
                return;
            }

            Color accentColor = creature.AccentColor;
            float alpha = ghosted ? 0.36f : 0.92f;
            float selectedBoost = selected ? 0.12f : 0f;
            m_rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            m_rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            m_rectTransform.pivot = new Vector2(0.5f, 0.5f);
            m_rectTransform.anchoredPosition = anchoredPosition;
            m_rectTransform.sizeDelta = size + new Vector2(selected ? 6f : 0f, selected ? 6f : 0f);
            m_rectTransform.localScale = Vector3.one;

            float inset = Mathf.Clamp(Mathf.Min(size.x, size.y) * 0.06f, 2f, 6f);
            if (m_imgPreview != null)
            {
                RectTransform previewRect = m_imgPreview.rectTransform;
                previewRect.anchorMin = Vector2.zero;
                previewRect.anchorMax = Vector2.one;
                previewRect.offsetMin = new Vector2(inset, inset);
                previewRect.offsetMax = new Vector2(-inset, -inset);
            }

            m_imgFrame.color = new Color(
                Mathf.Clamp01(accentColor.r * 0.82f + 0.08f + selectedBoost),
                Mathf.Clamp01(accentColor.g * 0.82f + 0.08f + selectedBoost),
                Mathf.Clamp01(accentColor.b * 0.82f + 0.08f + selectedBoost),
                alpha);

            RefreshPreview(creature, ghosted);
            m_textLabel.text = ChapterCreatureWidgetUtility.GetCreatureInitials(creature.Name);
            m_textLabel.color = new Color(1f, 1f, 1f, ghosted ? 0.78f : 0.96f);
            m_textLabel.fontSize = Mathf.Clamp(Mathf.Min(size.x, size.y) * (selected ? 0.28f : 0.24f), 14f, 30f);
            SetVisible(true);
        }

        private void RefreshPreview(ChapterCreatureData creature, bool ghosted)
        {
            string previewPath = ChapterEditorPersistenceService.ResolveCreaturePreviewPath(creature.PreviewImageFileName);
            if (!string.IsNullOrWhiteSpace(previewPath)
                && string.Equals(m_loadedPreviewPath, previewPath, StringComparison.OrdinalIgnoreCase)
                && m_previewSprite != null)
            {
                SetPreviewSprite(m_previewSprite, ghosted);
                return;
            }

            ResetPreview(creature, ghosted);
            if (string.IsNullOrWhiteSpace(previewPath))
            {
                return;
            }

            int loadVersion = ++m_previewLoadVersion;
            LoadPreviewFromPathAsync(previewPath, loadVersion, ghosted).Forget();
        }

        private void ResetPreview(ChapterCreatureData creature, bool ghosted)
        {
            ++m_previewLoadVersion;
            CleanupPreviewResources();

            if (m_imgPreview != null)
            {
                Color accentColor = creature.AccentColor;
                m_imgPreview.sprite = null;
                m_imgPreview.preserveAspect = true;
                m_imgPreview.color = new Color(
                    Mathf.Clamp01(accentColor.r * 0.86f + 0.06f),
                    Mathf.Clamp01(accentColor.g * 0.86f + 0.06f),
                    Mathf.Clamp01(accentColor.b * 0.86f + 0.06f),
                    ghosted ? 0.32f : 0.76f);
            }

            if (m_textLabel != null)
            {
                m_textLabel.gameObject.SetActive(true);
            }
        }

        private void SetPreviewSprite(Sprite sprite, bool ghosted)
        {
            if (m_imgPreview != null)
            {
                m_imgPreview.sprite = sprite;
                m_imgPreview.preserveAspect = true;
                m_imgPreview.color = new Color(1f, 1f, 1f, ghosted ? 0.46f : 0.98f);
            }

            if (m_textLabel != null)
            {
                m_textLabel.gameObject.SetActive(sprite == null);
            }
        }

        private async UniTaskVoid LoadPreviewFromPathAsync(string previewPath, int loadVersion, bool ghosted)
        {
            byte[] imageBytes;
            try
            {
                imageBytes = await UniTask.RunOnThreadPool(() => File.ReadAllBytes(previewPath));
            }
            catch (Exception exception)
            {
                Log.Warning($"Failed to read creature token preview: {exception.Message}");
                return;
            }

            if (loadVersion != m_previewLoadVersion)
            {
                return;
            }

            Texture2D previewTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!previewTexture.LoadImage(imageBytes))
            {
                Object.Destroy(previewTexture);
                Log.Warning("Failed to decode creature token preview image.");
                return;
            }

            Sprite previewSprite = Sprite.Create(
                previewTexture,
                new Rect(0, 0, previewTexture.width, previewTexture.height),
                new Vector2(0.5f, 0.5f));

            if (loadVersion != m_previewLoadVersion)
            {
                Object.Destroy(previewSprite);
                Object.Destroy(previewTexture);
                return;
            }

            CleanupPreviewResources();
            m_previewTexture = previewTexture;
            m_previewSprite = previewSprite;
            m_loadedPreviewPath = previewPath;
            SetPreviewSprite(previewSprite, ghosted);
        }

        private void CleanupPreviewResources()
        {
            m_loadedPreviewPath = string.Empty;

            if (m_previewSprite != null)
            {
                Object.Destroy(m_previewSprite);
                m_previewSprite = null;
            }

            if (m_previewTexture != null)
            {
                Object.Destroy(m_previewTexture);
                m_previewTexture = null;
            }
        }

        private static Image EnsurePreviewImage(Transform root)
        {
            Transform existing = root.Find("m_imgTokenPreview");
            if (existing != null)
            {
                Image image = existing.GetComponent<Image>();
                if (image != null)
                {
                    image.raycastTarget = false;
                    return image;
                }
            }

            GameObject previewObject = new GameObject("m_imgTokenPreview", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            previewObject.transform.SetParent(root, false);
            Image previewImage = previewObject.GetComponent<Image>();
            previewImage.raycastTarget = false;
            previewImage.preserveAspect = true;
            return previewImage;
        }

        private static TMP_Text EnsureLabel(Transform root, TMP_Text styleSource)
        {
            Transform existing = root.Find("m_tmpTokenLabel");
            if (existing != null)
            {
                TMP_Text text = existing.GetComponent<TMP_Text>();
                if (text != null)
                {
                    return text;
                }
            }

            GameObject labelObject = new GameObject("m_tmpTokenLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(root, false);
            RectTransform rectTransform = labelObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            TextMeshProUGUI textComponent = labelObject.GetComponent<TextMeshProUGUI>();
            if (styleSource != null)
            {
                textComponent.font = styleSource.font;
                textComponent.fontSharedMaterial = styleSource.fontSharedMaterial;
            }
            else if (TMP_Settings.defaultFontAsset != null)
            {
                textComponent.font = TMP_Settings.defaultFontAsset;
            }

            textComponent.text = string.Empty;
            textComponent.fontSize = 18f;
            textComponent.fontStyle = FontStyles.Bold;
            textComponent.enableWordWrapping = false;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.raycastTarget = false;
            return textComponent;
        }
    }

    internal sealed class ChapterCreatureDetailWidget
    {
        private readonly GameObject m_gameObject;
        private readonly RectTransform m_infoPanelRect;
        private readonly RectTransform m_infoContentRect;
        private readonly Image m_imgInfoPanelBackground;
        private readonly ScrollRect m_scrollRect;
        private readonly TMP_Text m_textTitle;
        private readonly TMP_Text m_textMeta;
        private readonly Image m_imgPreview;
        private readonly TMP_Text m_textPreviewInitials;
        private readonly DetailFieldSectionCard m_sectionCombat;
        private readonly DetailFieldSectionCard m_sectionAbility;
        private readonly DetailFieldSectionCard m_sectionSense;
        private readonly DetailFieldSectionCard m_sectionDefense;
        private readonly DetailBlockSectionCard m_sectionTraits;
        private readonly DetailBlockSectionCard m_sectionActions;
        private readonly DetailFieldSectionCard m_sectionExtraActions;
        private readonly DetailBlockSectionCard m_sectionBattleNotes;
        private readonly List<IDetailSectionCard> m_sectionCards;

        public ChapterCreatureDetailWidget(GameObject gameObject)
        {
            m_gameObject = gameObject;
            m_infoPanelRect = FindChildComponent<RectTransform>(gameObject.transform, "m_goDetailInfoPanel");
            m_infoContentRect = FindChildComponent<RectTransform>(gameObject.transform, "m_rectDetailInfoContent") ?? m_infoPanelRect;
            m_imgInfoPanelBackground = FindChildComponent<Image>(gameObject.transform, "m_goDetailInfoPanel");
            m_scrollRect = FindChildComponent<ScrollRect>(gameObject.transform, "m_goDetailInfoPanel");
            m_textTitle = FindChildComponent<TMP_Text>(gameObject.transform, "m_tmpDetailTitle")
                ?? FindChildComponent<TMP_Text>(gameObject.transform, "m_tmpDetailPreviewTitle");
            m_textMeta = FindChildComponent<TMP_Text>(gameObject.transform, "m_tmpDetailMeta")
                ?? EnsureInfoText("m_tmpDetailMeta", new Vector2(18f, -20f), new Vector2(-18f, -74f), 18f, FontStyles.Bold, new Color(0.94f, 0.95f, 0.98f, 0.92f));
            m_imgPreview = FindChildComponent<Image>(gameObject.transform, "m_goDetailPreview");
            m_textPreviewInitials = FindChildComponent<TMP_Text>(gameObject.transform, "m_tmpDetailPreviewInitials");

            m_sectionCombat = new DetailFieldSectionCard(
                gameObject.transform,
                "m_goSectionCombat",
                "m_tmpArmorClass",
                "m_tmpHitPoints",
                "m_tmpSpeed",
                "m_tmpExperiencePoints");
            m_sectionAbility = new DetailFieldSectionCard(
                gameObject.transform,
                "m_goSectionAbility",
                "m_tmpStrength",
                "m_tmpDexterity",
                "m_tmpConstitution",
                "m_tmpIntelligence",
                "m_tmpWisdom",
                "m_tmpCharisma");
            m_sectionSense = new DetailFieldSectionCard(
                gameObject.transform,
                "m_goSectionSense",
                "m_tmpSavingThrows",
                "m_tmpSkills",
                "m_tmpSenses",
                "m_tmpLanguages");
            m_sectionDefense = new DetailFieldSectionCard(
                gameObject.transform,
                "m_goSectionDefense",
                "m_tmpDamageResistances",
                "m_tmpDamageImmunities",
                "m_tmpConditionImmunities");
            m_sectionTraits = new DetailBlockSectionCard(gameObject.transform, "m_goSectionTraits", "m_tmpTraitsContent");
            m_sectionActions = new DetailBlockSectionCard(gameObject.transform, "m_goSectionActions", "m_tmpActionsContent");
            m_sectionExtraActions = new DetailFieldSectionCard(
                gameObject.transform,
                "m_goSectionExtraActions",
                "m_tmpBonusActions",
                "m_tmpReactions",
                "m_tmpLegendaryActions");
            m_sectionBattleNotes = new DetailBlockSectionCard(gameObject.transform, "m_goSectionBattleNotes", "m_tmpBattleNotesContent");
            m_sectionCards = new List<IDetailSectionCard>
            {
                m_sectionCombat,
                m_sectionAbility,
                m_sectionSense,
                m_sectionDefense,
                m_sectionTraits,
                m_sectionActions,
                m_sectionExtraActions,
                m_sectionBattleNotes
            };
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
                m_textMeta.text = string.IsNullOrWhiteSpace(creature.NameEn)
                    ? $"{creature.CreatureType}  |  {creature.Alignment}"
                    : $"{creature.NameEn}\n{creature.CreatureType}  |  {creature.Alignment}";
            }

            ResetPreview(creature);

            if (creature.Source != null)
            {
                BindStructuredSections(creature.Source);
            }
            else
            {
                BindLegacySummary(creature.Summary);
            }

            RefreshInfoScrollContent();
        }

        public void ResetPreview(ChapterCreatureStaticCardData creature)
        {
            if (m_imgPreview != null)
            {
                m_imgPreview.sprite = null;
                m_imgPreview.preserveAspect = true;
                m_imgPreview.color = new Color(creature.AccentColor.r * 0.8f, creature.AccentColor.g * 0.8f, creature.AccentColor.b * 0.8f, 0.72f);
            }

            if (m_textPreviewInitials != null)
            {
                m_textPreviewInitials.gameObject.SetActive(true);
                m_textPreviewInitials.text = ChapterCreatureWidgetUtility.GetCreatureInitials(creature.Name);
            }
        }

        public void SetPreviewSprite(Sprite sprite)
        {
            if (m_imgPreview != null)
            {
                m_imgPreview.sprite = sprite;
                m_imgPreview.preserveAspect = true;
                m_imgPreview.color = sprite != null ? Color.white : m_imgPreview.color;
            }

            if (m_textPreviewInitials != null)
            {
                m_textPreviewInitials.gameObject.SetActive(sprite == null);
            }
        }

        public void Dispose()
        {
            Object.Destroy(m_gameObject);
        }

        private TMP_Text EnsureInfoText(string objectName, Vector2 offsetMin, Vector2 offsetMax, float fontSize, FontStyles fontStyle, Color color)
        {
            if (m_infoContentRect == null)
            {
                return null;
            }

            Transform existing = m_infoContentRect.Find(objectName);
            if (existing != null)
            {
                return existing.GetComponent<TMP_Text>();
            }

            GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.layer = m_infoContentRect.gameObject.layer;
            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.SetParent(m_infoContentRect, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
            rectTransform.pivot = new Vector2(0.5f, 1f);

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            TMP_Text styleSource = m_textTitle ?? m_textPreviewInitials;
            if (styleSource != null)
            {
                text.font = styleSource.font;
                text.fontSharedMaterial = styleSource.fontSharedMaterial;
            }

            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.color = color;
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Overflow;
            text.horizontalAlignment = HorizontalAlignmentOptions.Left;
            text.verticalAlignment = VerticalAlignmentOptions.Top;
            text.raycastTarget = false;
            text.text = string.Empty;
            return text;
        }

        private void RefreshInfoScrollContent()
        {
            if (m_infoContentRect == null)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_infoContentRect);
            if (m_scrollRect != null)
            {
                m_scrollRect.StopMovement();
                m_scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        private void BindStructuredSections(ChapterCreatureData source)
        {
            int visibleSectionCount = 0;
            visibleSectionCount += m_sectionCombat.Bind(
                "战斗数值",
                ("护甲等级", source.ArmorClass),
                ("生命值", source.HitPoints),
                ("速度", source.Speed),
                ("经验值", source.ExperiencePoints)) ? 1 : 0;
            visibleSectionCount += m_sectionAbility.Bind(
                "属性数值",
                ("力量", source.Strength),
                ("敏捷", source.Dexterity),
                ("体质", source.Constitution),
                ("智力", source.Intelligence),
                ("感知", source.Wisdom),
                ("魅力", source.Charisma)) ? 1 : 0;
            visibleSectionCount += m_sectionSense.Bind(
                "感知与语言",
                ("豁免", source.SavingThrows),
                ("技能", source.Skills),
                ("感官", source.Senses),
                ("语言", source.Languages)) ? 1 : 0;
            visibleSectionCount += m_sectionDefense.Bind(
                "防御信息",
                ("伤害抗性", source.DamageResistances),
                ("伤害免疫", source.DamageImmunities),
                ("状态免疫", source.ConditionImmunities)) ? 1 : 0;
            visibleSectionCount += m_sectionTraits.Bind("特性", NormalizeBlock(source.Traits)) ? 1 : 0;
            visibleSectionCount += m_sectionActions.Bind("动作", NormalizeBlock(source.Actions)) ? 1 : 0;
            visibleSectionCount += m_sectionExtraActions.Bind(
                "额外动作",
                ("附赠动作", source.BonusActions),
                ("反应", source.Reactions),
                ("传奇动作", source.LegendaryActions)) ? 1 : 0;
            visibleSectionCount += m_sectionBattleNotes.Bind("战斗备注", NormalizeBlock(source.BattleNotes)) ? 1 : 0;

            if (visibleSectionCount == 0)
            {
                BindLegacySummary(source.BuildSummary());
            }
        }

        private void BindLegacySummary(string summary)
        {
            foreach (IDetailSectionCard sectionCard in m_sectionCards)
            {
                sectionCard.SetVisible(false);
            }

            string content = string.IsNullOrWhiteSpace(summary) ? "暂无详细信息。" : summary.Trim();
            m_sectionTraits.Bind("详情说明", content);
        }

        private static string NormalizeBlock(string content)
        {
            return string.IsNullOrWhiteSpace(content) ? string.Empty : content.Trim();
        }

        private interface IDetailSectionCard
        {
            void SetVisible(bool visible);
        }

        private sealed class DetailFieldSectionCard : IDetailSectionCard
        {
            private sealed class FieldRowLayout
            {
                public FieldRowLayout(int[] fieldIndices, float topGap)
                {
                    FieldIndices = fieldIndices;
                    TopGap = topGap;
                }

                public int[] FieldIndices { get; }

                public float TopGap { get; }
            }

            private readonly GameObject m_root;
            private readonly RectTransform m_rootRect;
            private readonly LayoutElement m_layoutElement;
            private readonly TMP_Text m_title;
            private readonly RectTransform m_titleRect;
            private readonly float m_titleMinHeight;
            private readonly float m_titleTopOffset;
            private readonly TMP_Text[] m_fields;
            private readonly RectTransform[] m_fieldRects;
            private readonly float[] m_fieldMinHeights;
            private readonly FieldRowLayout[] m_rows;
            private readonly float m_bottomPadding;

            public DetailFieldSectionCard(Transform owner, string rootName, params string[] fieldNames)
            {
                Transform root = owner.Find(rootName) ?? FindDescendant(owner, rootName);
                if (root == null)
                {
                    m_fields = Array.Empty<TMP_Text>();
                    m_fieldRects = Array.Empty<RectTransform>();
                    m_fieldMinHeights = Array.Empty<float>();
                    m_rows = Array.Empty<FieldRowLayout>();
                    return;
                }

                m_root = root.gameObject;
                m_rootRect = root as RectTransform;
                m_layoutElement = root.GetComponent<LayoutElement>();
                m_title = FindChildComponent<TMP_Text>(root, "m_tmpSectionTitle");
                m_titleRect = m_title != null ? m_title.rectTransform : null;
                m_titleMinHeight = m_titleRect != null ? GetCurrentHeight(m_titleRect) : 0f;
                m_titleTopOffset = GetTop(m_titleRect);
                m_fields = new TMP_Text[fieldNames.Length];
                m_fieldRects = new RectTransform[fieldNames.Length];
                m_fieldMinHeights = new float[fieldNames.Length];
                for (int index = 0; index < fieldNames.Length; index++)
                {
                    m_fields[index] = FindChildComponent<TMP_Text>(root, fieldNames[index]);
                    m_fieldRects[index] = m_fields[index] != null ? m_fields[index].rectTransform : null;
                    m_fieldMinHeights[index] = m_fieldRects[index] != null ? GetCurrentHeight(m_fieldRects[index]) : 0f;
                }

                m_rows = BuildRows(m_fieldRects, m_titleRect, m_fieldMinHeights);
                m_bottomPadding = CalculateBottomPadding(m_rootRect, CombineRects(m_titleRect, m_fieldRects));
            }

            public bool IsValid => m_root != null && m_title != null && m_fields.Length > 0;

            public void SetVisible(bool visible)
            {
                if (m_root != null)
                {
                    m_root.SetActive(visible);
                }
            }

            public bool Bind(string title, params (string Label, string Value)[] items)
            {
                if (!IsValid)
                {
                    return false;
                }

                if (m_title != null)
                {
                    m_title.text = title;
                }

                bool hasVisibleField = false;
                for (int index = 0; index < m_fields.Length; index++)
                {
                    TMP_Text fieldText = m_fields[index];
                    if (fieldText == null)
                    {
                        continue;
                    }

                    bool hasData = index < items.Length && !string.IsNullOrWhiteSpace(items[index].Value);
                    fieldText.gameObject.SetActive(hasData);
                    if (!hasData)
                    {
                        continue;
                    }

                    fieldText.text = $"{items[index].Label}：{items[index].Value.Trim()}";
                    hasVisibleField = true;
                }

                if (hasVisibleField)
                {
                    RefreshDynamicHeights();
                }

                SetVisible(hasVisibleField);
                return hasVisibleField;
            }

            private void RefreshDynamicHeights()
            {
                if (m_rootRect == null)
                {
                    return;
                }

                SetRectTop(m_titleRect, m_titleTopOffset);
                UpdateTextHeight(m_title, m_titleRect, m_titleMinHeight);

                float maxBottom = GetBottom(m_titleRect);
                for (int rowIndex = 0; rowIndex < m_rows.Length; rowIndex++)
                {
                    FieldRowLayout row = m_rows[rowIndex];
                    bool rowVisible = false;
                    float rowHeight = 0f;
                    for (int fieldIndexIndex = 0; fieldIndexIndex < row.FieldIndices.Length; fieldIndexIndex++)
                    {
                        int fieldIndex = row.FieldIndices[fieldIndexIndex];
                        TMP_Text field = m_fields[fieldIndex];
                        RectTransform fieldRect = m_fieldRects[fieldIndex];
                        if (field == null || fieldRect == null || !field.gameObject.activeSelf)
                        {
                            continue;
                        }

                        UpdateTextHeight(field, fieldRect, m_fieldMinHeights[fieldIndex]);
                        rowHeight = Mathf.Max(rowHeight, GetCurrentHeight(fieldRect));
                        rowVisible = true;
                    }

                    if (!rowVisible)
                    {
                        continue;
                    }

                    float rowTop = maxBottom + row.TopGap;
                    for (int fieldIndexIndex = 0; fieldIndexIndex < row.FieldIndices.Length; fieldIndexIndex++)
                    {
                        int fieldIndex = row.FieldIndices[fieldIndexIndex];
                        TMP_Text field = m_fields[fieldIndex];
                        RectTransform fieldRect = m_fieldRects[fieldIndex];
                        if (field == null || fieldRect == null || !field.gameObject.activeSelf)
                        {
                            continue;
                        }

                        SetRectTop(fieldRect, rowTop);
                    }

                    maxBottom = rowTop + rowHeight;
                }

                float targetHeight = maxBottom + m_bottomPadding;
                SetRectHeight(m_rootRect, targetHeight);
                if (m_layoutElement != null)
                {
                    m_layoutElement.minHeight = targetHeight;
                    m_layoutElement.preferredHeight = targetHeight;
                }
            }

            private static FieldRowLayout[] BuildRows(RectTransform[] fieldRects, RectTransform titleRect, float[] fieldMinHeights)
            {
                List<(float top, List<int> indices)> groupedRows = new List<(float top, List<int> indices)>();
                for (int index = 0; index < fieldRects.Length; index++)
                {
                    RectTransform fieldRect = fieldRects[index];
                    if (fieldRect == null)
                    {
                        continue;
                    }

                    float top = GetTop(fieldRect);
                    bool grouped = false;
                    for (int rowIndex = 0; rowIndex < groupedRows.Count; rowIndex++)
                    {
                        if (Mathf.Abs(groupedRows[rowIndex].top - top) > 0.5f)
                        {
                            continue;
                        }

                        groupedRows[rowIndex].indices.Add(index);
                        grouped = true;
                        break;
                    }

                    if (!grouped)
                    {
                        groupedRows.Add((top, new List<int> {index}));
                    }
                }

                groupedRows.Sort((left, right) => left.top.CompareTo(right.top));

                List<FieldRowLayout> rows = new List<FieldRowLayout>(groupedRows.Count);
                float previousBottom = GetBottom(titleRect);
                for (int rowIndex = 0; rowIndex < groupedRows.Count; rowIndex++)
                {
                    List<int> indices = groupedRows[rowIndex].indices;
                    indices.Sort((left, right) =>
                    {
                        RectTransform leftRect = fieldRects[left];
                        RectTransform rightRect = fieldRects[right];
                        float leftX = leftRect != null ? leftRect.anchoredPosition.x : 0f;
                        float rightX = rightRect != null ? rightRect.anchoredPosition.x : 0f;
                        return leftX.CompareTo(rightX);
                    });

                    float rowTop = groupedRows[rowIndex].top;
                    float rowBottom = rowTop;
                    for (int index = 0; index < indices.Count; index++)
                    {
                        int fieldIndex = indices[index];
                        rowBottom = Mathf.Max(rowBottom, rowTop + fieldMinHeights[fieldIndex]);
                    }

                    float topGap = Mathf.Max(0f, rowTop - previousBottom);
                    rows.Add(new FieldRowLayout(indices.ToArray(), topGap));
                    previousBottom = rowBottom;
                }

                return rows.ToArray();
            }
        }

        private sealed class DetailBlockSectionCard : IDetailSectionCard
        {
            private readonly GameObject m_root;
            private readonly RectTransform m_rootRect;
            private readonly LayoutElement m_layoutElement;
            private readonly TMP_Text m_title;
            private readonly RectTransform m_titleRect;
            private readonly float m_titleMinHeight;
            private readonly float m_titleTopOffset;
            private readonly TMP_Text m_content;
            private readonly RectTransform m_contentRect;
            private readonly float m_contentMinHeight;
            private readonly float m_contentTopGap;
            private readonly float m_bottomPadding;

            public DetailBlockSectionCard(Transform owner, string rootName, string contentName)
            {
                Transform root = owner.Find(rootName) ?? FindDescendant(owner, rootName);
                if (root == null)
                {
                    return;
                }

                m_root = root.gameObject;
                m_rootRect = root as RectTransform;
                m_layoutElement = root.GetComponent<LayoutElement>();
                m_title = FindChildComponent<TMP_Text>(root, "m_tmpSectionTitle");
                m_titleRect = m_title != null ? m_title.rectTransform : null;
                m_titleMinHeight = m_titleRect != null ? GetCurrentHeight(m_titleRect) : 0f;
                m_titleTopOffset = GetTop(m_titleRect);
                m_content = FindChildComponent<TMP_Text>(root, contentName)
                    ?? FindChildComponent<TMP_Text>(root, "m_tmpSectionContent");
                m_contentRect = m_content != null ? m_content.rectTransform : null;
                m_contentMinHeight = m_contentRect != null ? GetCurrentHeight(m_contentRect) : 0f;
                m_contentTopGap = Mathf.Max(0f, GetTop(m_contentRect) - GetBottom(m_titleRect));
                m_bottomPadding = CalculateBottomPadding(m_rootRect, m_titleRect, m_contentRect);
            }

            public bool IsValid => m_root != null && m_title != null && m_content != null;

            public void SetVisible(bool visible)
            {
                if (m_root != null)
                {
                    m_root.SetActive(visible);
                }
            }

            public bool Bind(string title, string content)
            {
                if (!IsValid)
                {
                    return false;
                }

                bool visible = !string.IsNullOrWhiteSpace(content);
                if (visible)
                {
                    m_title.text = title;
                    m_content.text = content.Trim();
                    RefreshDynamicHeights();
                }

                SetVisible(visible);
                return visible;
            }

            private void RefreshDynamicHeights()
            {
                if (m_rootRect == null)
                {
                    return;
                }

                SetRectTop(m_titleRect, m_titleTopOffset);
                UpdateTextHeight(m_title, m_titleRect, m_titleMinHeight);
                SetRectTop(m_contentRect, GetBottom(m_titleRect) + m_contentTopGap);
                UpdateTextHeight(m_content, m_contentRect, m_contentMinHeight);

                float targetHeight = Mathf.Max(GetBottom(m_titleRect), GetBottom(m_contentRect)) + m_bottomPadding;
                SetRectHeight(m_rootRect, targetHeight);
                if (m_layoutElement != null)
                {
                    m_layoutElement.minHeight = targetHeight;
                    m_layoutElement.preferredHeight = targetHeight;
                }
            }
        }

        private static float CalculateBottomPadding(RectTransform rootRect, params RectTransform[] childRects)
        {
            if (rootRect == null)
            {
                return 0f;
            }

            float rootHeight = GetCurrentHeight(rootRect);
            float maxBottom = 0f;
            for (int index = 0; index < childRects.Length; index++)
            {
                maxBottom = Mathf.Max(maxBottom, GetBottom(childRects[index]));
            }

            return Mathf.Max(0f, rootHeight - maxBottom);
        }

        private static float GetTop(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return 0f;
            }

            return -rectTransform.anchoredPosition.y;
        }

        private static RectTransform[] CombineRects(RectTransform first, RectTransform[] others)
        {
            int otherCount = others != null ? others.Length : 0;
            RectTransform[] combined = new RectTransform[otherCount + 1];
            combined[0] = first;
            for (int index = 0; index < otherCount; index++)
            {
                combined[index + 1] = others[index];
            }

            return combined;
        }

        private static void UpdateTextHeight(TMP_Text text, RectTransform rectTransform, float minimumHeight)
        {
            if (text == null || rectTransform == null)
            {
                return;
            }

            float width = GetPreferredWidth(rectTransform);
            if (width <= 0f)
            {
                width = rectTransform.rect.width;
            }

            text.ForceMeshUpdate();
            Vector2 preferred = text.GetPreferredValues(text.text, width, 0f);
            float targetHeight = Mathf.Max(minimumHeight, preferred.y);
            SetRectHeight(rectTransform, targetHeight);
        }

        private static float GetPreferredWidth(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return 0f;
            }

            RectTransform parentRect = rectTransform.parent as RectTransform;
            if (parentRect != null && !Mathf.Approximately(rectTransform.anchorMin.x, rectTransform.anchorMax.x))
            {
                return parentRect.rect.width * (rectTransform.anchorMax.x - rectTransform.anchorMin.x) + rectTransform.sizeDelta.x;
            }

            return rectTransform.rect.width > 0f ? rectTransform.rect.width : rectTransform.sizeDelta.x;
        }

        private static float GetBottom(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return 0f;
            }

            return -rectTransform.anchoredPosition.y + GetCurrentHeight(rectTransform);
        }

        private static float GetCurrentHeight(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return 0f;
            }

            return rectTransform.rect.height > 0f ? rectTransform.rect.height : rectTransform.sizeDelta.y;
        }

        private static void SetRectHeight(RectTransform rectTransform, float height)
        {
            if (rectTransform == null)
            {
                return;
            }

            Vector2 sizeDelta = rectTransform.sizeDelta;
            sizeDelta.y = height;
            rectTransform.sizeDelta = sizeDelta;
        }

        private static void SetRectTop(RectTransform rectTransform, float top)
        {
            if (rectTransform == null)
            {
                return;
            }

            Vector2 anchoredPosition = rectTransform.anchoredPosition;
            anchoredPosition.y = -top;
            rectTransform.anchoredPosition = anchoredPosition;
        }

        private static T FindChildComponent<T>(Transform root, string path) where T : Component
        {
            Transform child = root.Find(path) ?? FindDescendant(root, path);
            return child != null ? child.GetComponent<T>() : null;
        }

        private static Transform FindDescendant(Transform root, string targetName)
        {
            for (int index = 0; index < root.childCount; index++)
            {
                Transform child = root.GetChild(index);
                if (child.name == targetName)
                {
                    return child;
                }

                Transform descendant = FindDescendant(child, targetName);
                if (descendant != null)
                {
                    return descendant;
                }
            }

            return null;
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
