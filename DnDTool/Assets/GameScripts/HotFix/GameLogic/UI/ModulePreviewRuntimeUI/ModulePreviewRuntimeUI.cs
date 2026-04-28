using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using TEngine;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    [Window(UILayer.UI, location: "ModulePreviewRuntimeUI", fullScreen: true)]
    internal sealed class ModulePreviewRuntimeUI : UIWindow
    {
        private ModulePreviewSessionData m_previewSession;
        private TMP_Text m_tmpPreviewMode;
        private TMP_Text m_tmpChapterTitle;
        private TMP_Text m_tmpChapterProgress;
        private TMP_Text m_tmpMapPlaceholder;
        private TMP_Text m_tmpChapterGoal;
        private TMP_Text m_tmpChapterContent;
        private TMP_Text m_tmpDmNote;
        private TMP_Text m_tmpTerrainInfo;
        private TMP_Text m_tmpCreatureInfo;
        private TMP_Text m_tmpRuntimeLog;
        private Canvas m_canvas;
        private RectTransform m_rectRuntimeLogPanel;
        private RectTransform m_rectMapArea;
        private RectTransform m_rectChapterMap;
        private RectTransform m_rectMapOverlay;
        private Image m_imgChapterMap;
        private Image m_imgMapOverlay;
        private Button m_btnReturnEditor;
        private Button m_btnRestartPreview;
        private Button m_btnPrevChapter;
        private Button m_btnNextChapter;
        private ChapterPreviewRuntimeData m_currentChapter;
        private Texture2D m_mapTexture;
        private Sprite m_mapSprite;
        private Texture2D m_overlayTexture;
        private Sprite m_overlaySprite;
        private readonly List<TMP_Text> m_gridCoordinateLabels = new List<TMP_Text>();
        private readonly Dictionary<int, RuntimeTestPlayerTokenData> m_runtimeTestPlayerTokens = new Dictionary<int, RuntimeTestPlayerTokenData>();
        private Vector2 m_runtimeViewPanOffset = Vector2.zero;
        private Vector2 m_lastRuntimeMapDragLocalPoint = Vector2.zero;
        private bool m_isDraggingRuntimeMap;
        private bool m_isDraggingRuntimeTestPlayer;
        private bool m_isDraggingRuntimeTestPlayerFromPlacedToken;
        private bool m_hasRuntimeTestPlayerDragCoordinate;
        private ChapterGridCoordinate m_runtimeTestPlayerDragCoordinate = ChapterGridCoordinate.Zero;
        private float m_runtimeViewZoomScale = 1f;
        private int m_overlayTextureWidth = 1024;
        private int m_overlayTextureHeight = 640;
        private Vector2 m_overlayEditorImageSize = Vector2.one;
        private Vector2 m_overlayEditorMapPanOffset = Vector2.zero;

        private const float MapPreviewPadding = 18f;
        private const float RuntimeMapViewMinScale = 0.5f;
        private const float RuntimeMapViewMaxScale = 3f;
        private const float RuntimeMapViewScrollStep = 0.12f;
        private const float FineZoomScrollStep = 0.005f;
        private const int BaseMapGridColumns = 11;
        private const int BaseMapGridRows = 6;
        private const int OverlayTextureWidth = 1024;
        private const int OverlayTextureHeight = 640;
        private const int OverlayGridLineThickness = 3;
        private static readonly Color PreviewGridLineColor = new Color(0.05f, 0.07f, 0.09f, 0.58f);
        private const float GridCoordinateLabelInset = 3f;
        private const float GridCoordinateLabelHeight = 20f;
        private const float GridCoordinateLabelFontScale = 0.22f;
        private const float GridCoordinateLabelMinFontSize = 10f;
        private const float GridCoordinateLabelMaxFontSize = 18f;
        private const int MaxGridCoordinateLabels = 2048;
        private const string RuntimeTestPlayerName = "测试角色";

        private sealed class RuntimeTestPlayerTokenData
        {
            public string Name { get; set; } = RuntimeTestPlayerName;

            public ChapterGridCoordinate Coordinate { get; set; } = ChapterGridCoordinate.Zero;
        }

        protected override void ScriptGenerator()
        {
            m_tmpPreviewMode = FindChildComponent<TMP_Text>("m_panelTopBar/m_tmpPreviewMode");
            m_tmpChapterTitle = FindChildComponent<TMP_Text>("m_panelTopBar/m_tmpChapterTitle");
            m_tmpChapterProgress = FindChildComponent<TMP_Text>("m_panelTopBar/m_tmpChapterProgress");
            m_btnReturnEditor = FindChildComponent<Button>("m_panelTopBar/m_btnReturnEditor");
            m_btnRestartPreview = FindChildComponent<Button>("m_panelTopBar/m_btnRestartPreview");
            m_btnPrevChapter = FindChildComponent<Button>("m_panelTopBar/m_btnPrevChapter");
            m_btnNextChapter = FindChildComponent<Button>("m_panelTopBar/m_btnNextChapter");
            m_rectMapArea = FindChild("m_panelMapArea") as RectTransform;
            m_imgChapterMap = FindChildComponent<Image>("m_panelMapArea/m_imgChapterMap");
            m_imgMapOverlay = FindChildComponent<Image>("m_panelMapArea/m_imgMapOverlay");
            m_rectChapterMap = m_imgChapterMap != null ? m_imgChapterMap.rectTransform : null;
            m_rectMapOverlay = m_imgMapOverlay != null ? m_imgMapOverlay.rectTransform : null;
            m_tmpMapPlaceholder = FindChildComponent<TMP_Text>("m_panelMapArea/m_tmpMapPlaceholder");
            m_tmpChapterGoal = FindChildComponent<TMP_Text>("m_panelInfo/m_tmpChapterGoal");
            m_tmpChapterContent = FindChildComponent<TMP_Text>("m_panelInfo/m_tmpChapterContent");
            m_tmpDmNote = FindChildComponent<TMP_Text>("m_panelInfo/m_tmpDmNote");
            m_tmpTerrainInfo = FindChildComponent<TMP_Text>("m_panelInfo/m_tmpTerrainInfo");
            m_tmpCreatureInfo = FindChildComponent<TMP_Text>("m_panelInfo/m_tmpCreatureInfo");
            m_rectRuntimeLogPanel = FindChild("m_panelRuntimeLog") as RectTransform;
            m_tmpRuntimeLog = FindChildComponent<TMP_Text>("m_panelRuntimeLog/m_tmpRuntimeLog");

            BindButton(m_btnReturnEditor, OnClickReturnEditor);
            BindButton(m_btnRestartPreview, OnClickRestartPreview);
            BindButton(m_btnPrevChapter, OnClickPrevChapter);
            BindButton(m_btnNextChapter, OnClickNextChapter);
        }

        protected override void OnCreate()
        {
            m_canvas = gameObject.GetComponent<Canvas>();
        }

        protected override void OnRefresh()
        {
            m_previewSession = UserData as ModulePreviewSessionData;
            if (m_previewSession == null)
            {
                Log.Warning("ModulePreviewRuntimeUI opened without preview session data.");
                RefreshEmptyContent();
                return;
            }

            RefreshPreviewContent();
        }

        protected override void OnUpdate()
        {
            if (m_currentChapter == null || m_rectMapArea == null || !m_rectMapArea.gameObject.activeInHierarchy)
            {
                m_isDraggingRuntimeMap = false;
                return;
            }

            if (HandleRuntimeTestPlayerDragging())
            {
                return;
            }

            HandleRuntimeMapDragging();
            HandleRuntimeMapZooming();
        }

        protected override void OnDestroy()
        {
            CleanupMapResources();
            CleanupOverlayResources();
        }

        private void OnClickReturnEditor()
        {
            GameModule.UI.CloseUI<ModulePreviewRuntimeUI>();
            GameModule.UI.ShowUIAsync<ChapterEditorUI>();
        }

        private void OnClickRestartPreview()
        {
            if (m_previewSession == null)
            {
                return;
            }

            m_previewSession.CurrentChapterIndex = m_previewSession.Chapters.Count > 0 ? 0 : -1;
            RefreshPreviewContent();
        }

        private void OnClickPrevChapter()
        {
            if (m_previewSession == null || m_previewSession.Chapters.Count <= 0)
            {
                return;
            }

            m_previewSession.CurrentChapterIndex = Mathf.Max(0, m_previewSession.CurrentChapterIndex - 1);
            RefreshPreviewContent();
        }

        private void OnClickNextChapter()
        {
            if (m_previewSession == null || m_previewSession.Chapters.Count <= 0)
            {
                return;
            }

            m_previewSession.CurrentChapterIndex = Mathf.Min(m_previewSession.Chapters.Count - 1, m_previewSession.CurrentChapterIndex + 1);
            RefreshPreviewContent();
        }

        private void RefreshPreviewContent()
        {
            ChapterPreviewRuntimeData chapter = m_previewSession?.CurrentChapter;
            if (chapter == null)
            {
                RefreshEmptyContent();
                return;
            }

            m_currentChapter = chapter;
            ResetRuntimeMapView();
            int chapterCount = m_previewSession.Chapters?.Count ?? 0;
            SetText(m_tmpPreviewMode, "预览模式");
            SetText(m_tmpChapterTitle, string.IsNullOrWhiteSpace(chapter.ChapterName) ? $"第 {chapter.ChapterIndex + 1} 章" : chapter.ChapterName);
            SetText(m_tmpChapterProgress, $"{chapter.ChapterIndex + 1} / {Mathf.Max(1, chapterCount)}");
            RefreshChapterInfo(chapter);
            SetText(m_tmpRuntimeLog, BuildRuntimeLog(chapter, chapterCount));
            RefreshMap(chapter.MapImagePath);
            ApplyRuntimeMapViewLayout();
            RefreshMapOverlay(chapter);
            ApplyRuntimeMapViewLayout();
            RefreshRuntimeLog();
            RefreshControlButtons();
        }

        private void RefreshChapterInfo(ChapterPreviewRuntimeData chapter)
        {
            if (m_tmpChapterContent == null
                && m_tmpDmNote == null
                && m_tmpTerrainInfo == null
                && m_tmpCreatureInfo == null)
            {
                SetText(m_tmpChapterGoal, BuildCombinedChapterInfo(chapter));
                return;
            }

            SetText(m_tmpChapterGoal, BuildInfoBlock("章节目标", chapter.Goal));
            SetText(m_tmpChapterContent, BuildInfoBlock("章节正文", chapter.Content));
            SetText(m_tmpDmNote, BuildInfoBlock("DM 备注", chapter.DmNote));
            SetText(m_tmpTerrainInfo, BuildTerrainInfo(chapter));
            SetText(m_tmpCreatureInfo, BuildCreatureInfo(chapter));
        }

        private void RefreshEmptyContent()
        {
            m_currentChapter = null;
            ResetRuntimeMapView();
            CleanupMapResources();
            SetText(m_tmpPreviewMode, "预览模式");
            SetText(m_tmpChapterTitle, "暂无章节");
            SetText(m_tmpChapterProgress, "0 / 0");
            SetText(m_tmpMapPlaceholder, "暂无地图");
            SetText(m_tmpChapterGoal, BuildInfoBlock("章节目标", string.Empty));
            SetText(m_tmpChapterContent, BuildInfoBlock("章节正文", string.Empty));
            SetText(m_tmpDmNote, BuildInfoBlock("DM 备注", string.Empty));
            SetText(m_tmpTerrainInfo, BuildInfoBlock("地形说明", string.Empty));
            SetText(m_tmpCreatureInfo, BuildInfoBlock("生物实例", string.Empty));
            SetText(m_tmpRuntimeLog, "预览会话尚未加载。");

            if (m_imgChapterMap != null)
            {
                m_imgChapterMap.gameObject.SetActive(false);
            }

            CleanupOverlayResources();
            if (m_imgMapOverlay != null)
            {
                m_imgMapOverlay.gameObject.SetActive(false);
            }

            ApplyRuntimeMapViewLayout();

            RefreshControlButtons();
        }

        private void RefreshMap(string mapImagePath)
        {
            CleanupMapResources();
            if (m_imgChapterMap == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(mapImagePath) || !File.Exists(mapImagePath))
            {
                m_imgChapterMap.gameObject.SetActive(false);
                SetText(m_tmpMapPlaceholder, "当前章节未配置地图。");
                return;
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(mapImagePath);
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!texture.LoadImage(bytes))
                {
                    UnityEngine.Object.Destroy(texture);
                    m_imgChapterMap.gameObject.SetActive(false);
                    SetText(m_tmpMapPlaceholder, "地图图片读取失败。");
                    return;
                }

                m_mapTexture = texture;
                m_mapSprite = Sprite.Create(
                    m_mapTexture,
                    new Rect(0f, 0f, m_mapTexture.width, m_mapTexture.height),
                    new Vector2(0.5f, 0.5f));
                m_imgChapterMap.sprite = m_mapSprite;
                m_imgChapterMap.preserveAspect = true;
                m_imgChapterMap.gameObject.SetActive(true);
                SetText(m_tmpMapPlaceholder, string.Empty);
            }
            catch (Exception exception)
            {
                Log.Warning($"预览地图加载失败: {exception.Message}");
                m_imgChapterMap.gameObject.SetActive(false);
                SetText(m_tmpMapPlaceholder, "地图图片加载失败。");
            }
        }

        private void ResetRuntimeMapView()
        {
            m_runtimeViewZoomScale = 1f;
            m_runtimeViewPanOffset = Vector2.zero;
            m_lastRuntimeMapDragLocalPoint = Vector2.zero;
            m_isDraggingRuntimeMap = false;
        }

        private void ApplyRuntimeMapViewLayout()
        {
            ApplyChapterMapSurfaceLayout();
            ApplyChapterOverlayLayout();
        }

        private void ApplyChapterMapSurfaceLayout()
        {
            if (m_rectChapterMap == null)
            {
                return;
            }

            if (m_mapTexture == null || m_rectMapArea == null)
            {
                m_rectChapterMap.anchorMin = new Vector2(0.5f, 0.5f);
                m_rectChapterMap.anchorMax = new Vector2(0.5f, 0.5f);
                m_rectChapterMap.pivot = new Vector2(0.5f, 0.5f);
                m_rectChapterMap.anchoredPosition = m_runtimeViewPanOffset;
                m_rectChapterMap.sizeDelta = GetBaseOverlaySize() * m_runtimeViewZoomScale;
                m_rectChapterMap.localScale = Vector3.one;
                return;
            }

            ChapterMapGridStateData mapGridState = m_currentChapter?.MapGridState ?? new ChapterMapGridStateData();
            float mapZoomScale = Mathf.Max(0.0001f, mapGridState.MapZoomScale > 0f ? mapGridState.MapZoomScale : 1f);
            Vector2 baseSize = CalculateFittedMapSize(mapZoomScale);
            Vector2 basePosition = GetScaledSavedMapPosition();

            m_rectChapterMap.anchorMin = new Vector2(0.5f, 0.5f);
            m_rectChapterMap.anchorMax = new Vector2(0.5f, 0.5f);
            m_rectChapterMap.pivot = new Vector2(0.5f, 0.5f);
            m_rectChapterMap.anchoredPosition = basePosition * m_runtimeViewZoomScale + m_runtimeViewPanOffset;
            m_rectChapterMap.sizeDelta = baseSize * m_runtimeViewZoomScale;
            m_rectChapterMap.localScale = Vector3.one;
        }

        private void ApplyChapterOverlayLayout()
        {
            if (m_rectMapOverlay == null)
            {
                return;
            }

            Vector2 baseSize = GetBaseOverlaySize();
            Vector2 basePosition = GetScaledSavedMapPosition();
            m_rectMapOverlay.anchorMin = new Vector2(0.5f, 0.5f);
            m_rectMapOverlay.anchorMax = new Vector2(0.5f, 0.5f);
            m_rectMapOverlay.pivot = new Vector2(0.5f, 0.5f);
            m_rectMapOverlay.anchoredPosition = basePosition * m_runtimeViewZoomScale + m_runtimeViewPanOffset;
            m_rectMapOverlay.sizeDelta = baseSize * m_runtimeViewZoomScale;
            m_rectMapOverlay.localScale = Vector3.one;
        }

        private Vector2 CalculateFittedMapSize(float mapZoomScale)
        {
            Vector2 availableSize = GetBaseMapAreaSize();
            if (m_mapTexture == null)
            {
                return availableSize;
            }

            float textureWidth = Mathf.Max(1f, m_mapTexture.width);
            float textureHeight = Mathf.Max(1f, m_mapTexture.height);
            float scale = Mathf.Min(availableSize.x / textureWidth, availableSize.y / textureHeight);
            return new Vector2(textureWidth * scale * mapZoomScale, textureHeight * scale * mapZoomScale);
        }

        private Vector2 GetBaseOverlaySize()
        {
            if (m_mapTexture != null && m_currentChapter != null)
            {
                ChapterMapGridStateData mapGridState = m_currentChapter.MapGridState ?? new ChapterMapGridStateData();
                float mapZoomScale = Mathf.Max(0.0001f, mapGridState.MapZoomScale > 0f ? mapGridState.MapZoomScale : 1f);
                return CalculateFittedMapSize(mapZoomScale);
            }

            return GetBaseMapAreaSize();
        }

        private Vector2 GetBaseMapAreaSize()
        {
            if (m_rectMapArea == null)
            {
                return new Vector2(OverlayTextureWidth, OverlayTextureHeight);
            }

            return new Vector2(
                Mathf.Max(1f, m_rectMapArea.rect.width - MapPreviewPadding),
                Mathf.Max(1f, m_rectMapArea.rect.height - MapPreviewPadding));
        }

        private Vector2 GetScaledSavedMapPosition()
        {
            ChapterMapGridStateData mapGridState = m_currentChapter?.MapGridState ?? new ChapterMapGridStateData();
            return ScaleSavedMapOffset(mapGridState.MapPanOffset, GetBaseMapAreaSize());
        }

        private Vector2 ScaleSavedMapOffset(Vector2 savedOffset, Vector2 targetSize)
        {
            Vector2 sourceSize = m_currentChapter != null ? m_currentChapter.EditorMapOverlaySize : Vector2.zero;
            return ScaleOffset(savedOffset, sourceSize, targetSize);
        }

        private static Vector2 ScaleOffset(Vector2 savedOffset, Vector2 sourceSize, Vector2 targetSize)
        {
            if (sourceSize.x <= 0f || sourceSize.y <= 0f || targetSize.x <= 0f || targetSize.y <= 0f)
            {
                return savedOffset;
            }

            return new Vector2(
                savedOffset.x * targetSize.x / sourceSize.x,
                savedOffset.y * targetSize.y / sourceSize.y);
        }

        private Vector2 GetEditorOverlaySize(ChapterPreviewRuntimeData chapter)
        {
            if (chapter != null && chapter.EditorMapOverlaySize.x > 0f && chapter.EditorMapOverlaySize.y > 0f)
            {
                return chapter.EditorMapOverlaySize;
            }

            return GetBaseMapAreaSize();
        }

        private Vector2 CalculateEditorFittedMapSize(ChapterMapGridStateData gridState, Vector2 editorOverlaySize)
        {
            if (m_mapTexture == null)
            {
                return editorOverlaySize;
            }

            float textureWidth = Mathf.Max(1f, m_mapTexture.width);
            float textureHeight = Mathf.Max(1f, m_mapTexture.height);
            float scale = Mathf.Min(
                Mathf.Max(1f, editorOverlaySize.x) / textureWidth,
                Mathf.Max(1f, editorOverlaySize.y) / textureHeight);
            float mapZoomScale = Mathf.Max(0.0001f, gridState != null && gridState.MapZoomScale > 0f ? gridState.MapZoomScale : 1f);
            return new Vector2(textureWidth * scale * mapZoomScale, textureHeight * scale * mapZoomScale);
        }

        private void HandleRuntimeMapDragging()
        {
            if (Input.GetMouseButtonUp(0))
            {
                m_isDraggingRuntimeMap = false;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (!IsMouseOverMapArea())
                {
                    return;
                }

                if (TryGetMouseLocalPointInMapArea(out Vector2 localPoint))
                {
                    m_isDraggingRuntimeMap = true;
                    m_lastRuntimeMapDragLocalPoint = localPoint;
                }

                return;
            }

            if (!m_isDraggingRuntimeMap || !Input.GetMouseButton(0))
            {
                return;
            }

            if (!TryGetMouseLocalPointInMapArea(out Vector2 currentLocalPoint))
            {
                return;
            }

            Vector2 dragDelta = currentLocalPoint - m_lastRuntimeMapDragLocalPoint;
            if (dragDelta.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            m_lastRuntimeMapDragLocalPoint = currentLocalPoint;
            m_runtimeViewPanOffset += dragDelta;
            ApplyRuntimeMapViewLayout();
        }

        private void HandleRuntimeMapZooming()
        {
            if (!IsMouseOverMapArea())
            {
                return;
            }

            float scrollDelta = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scrollDelta) < 0.01f)
            {
                return;
            }

            if (!TryGetMouseLocalPointInMapArea(out Vector2 localPoint))
            {
                localPoint = Vector2.zero;
            }

            float previousZoomScale = m_runtimeViewZoomScale;
            float appliedScrollDelta = NormalizeScrollDelta(scrollDelta) * GetZoomScrollStep();
            m_runtimeViewZoomScale = Mathf.Clamp(
                m_runtimeViewZoomScale + appliedScrollDelta,
                RuntimeMapViewMinScale,
                RuntimeMapViewMaxScale);

            if (Mathf.Approximately(previousZoomScale, m_runtimeViewZoomScale))
            {
                return;
            }

            float zoomRatio = m_runtimeViewZoomScale / Mathf.Max(0.0001f, previousZoomScale);
            m_runtimeViewPanOffset = localPoint - (localPoint - m_runtimeViewPanOffset) * zoomRatio;
            ApplyRuntimeMapViewLayout();
        }

        private bool IsMouseOverMapArea()
        {
            return m_rectMapArea != null
                && RectTransformUtility.RectangleContainsScreenPoint(m_rectMapArea, Input.mousePosition, GetMapAreaEventCamera());
        }

        private bool TryGetMouseLocalPointInMapArea(out Vector2 localPoint)
        {
            localPoint = Vector2.zero;
            return m_rectMapArea != null
                && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    m_rectMapArea,
                    Input.mousePosition,
                    GetMapAreaEventCamera(),
                    out localPoint);
        }

        private Camera GetMapAreaEventCamera()
        {
            if (m_canvas != null && m_canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                return m_canvas.worldCamera;
            }

            return null;
        }

        private static float GetZoomScrollStep()
        {
            return IsFineZoomModifierPressed() ? FineZoomScrollStep : RuntimeMapViewScrollStep;
        }

        private static bool IsFineZoomModifierPressed()
        {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }

        private static float NormalizeScrollDelta(float scrollDelta)
        {
            if (scrollDelta > 0f)
            {
                return 1f;
            }

            if (scrollDelta < 0f)
            {
                return -1f;
            }

            return 0f;
        }

        private bool HandleRuntimeTestPlayerDragging()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (IsMouseOverRuntimeTestPlayerSource())
                {
                    BeginRuntimeTestPlayerDrag(false);
                    return true;
                }

                if (IsMouseOverRuntimeTestPlayerToken())
                {
                    BeginRuntimeTestPlayerDrag(true);
                    return true;
                }
            }

            if (!m_isDraggingRuntimeTestPlayer)
            {
                return false;
            }

            m_isDraggingRuntimeMap = false;
            UpdateRuntimeTestPlayerDragCoordinate();

            if (Input.GetMouseButtonUp(0))
            {
                if (m_hasRuntimeTestPlayerDragCoordinate)
                {
                    RuntimeTestPlayerTokenData token = GetOrCreateRuntimeTestPlayerToken();
                    token.Coordinate = m_runtimeTestPlayerDragCoordinate;
                }

                CancelRuntimeTestPlayerDrag();
                RefreshMapOverlay(m_currentChapter);
                ApplyRuntimeMapViewLayout();
                RefreshRuntimeLog();
            }

            return true;
        }

        private void BeginRuntimeTestPlayerDrag(bool fromPlacedToken)
        {
            m_isDraggingRuntimeTestPlayer = true;
            m_isDraggingRuntimeTestPlayerFromPlacedToken = fromPlacedToken;
            m_isDraggingRuntimeMap = false;
            UpdateRuntimeTestPlayerDragCoordinate();
            RefreshMapOverlay(m_currentChapter);
            ApplyRuntimeMapViewLayout();
            RefreshRuntimeLog();
        }

        private void CancelRuntimeTestPlayerDrag()
        {
            m_isDraggingRuntimeTestPlayer = false;
            m_isDraggingRuntimeTestPlayerFromPlacedToken = false;
            m_hasRuntimeTestPlayerDragCoordinate = false;
        }

        private void UpdateRuntimeTestPlayerDragCoordinate()
        {
            bool previousHasCoordinate = m_hasRuntimeTestPlayerDragCoordinate;
            ChapterGridCoordinate previousCoordinate = m_runtimeTestPlayerDragCoordinate;
            m_hasRuntimeTestPlayerDragCoordinate = TryGetRuntimeGridCoordinateFromScreenPoint(Input.mousePosition, out m_runtimeTestPlayerDragCoordinate);
            if (previousHasCoordinate == m_hasRuntimeTestPlayerDragCoordinate
                && (!m_hasRuntimeTestPlayerDragCoordinate || previousCoordinate.Equals(m_runtimeTestPlayerDragCoordinate)))
            {
                return;
            }

            RefreshMapOverlay(m_currentChapter);
            ApplyRuntimeMapViewLayout();
            RefreshRuntimeLog();
        }

        private bool IsMouseOverRuntimeTestPlayerSource()
        {
            return m_rectRuntimeLogPanel != null
                && RectTransformUtility.RectangleContainsScreenPoint(m_rectRuntimeLogPanel, Input.mousePosition, GetMapAreaEventCamera());
        }

        private bool IsMouseOverRuntimeTestPlayerToken()
        {
            return TryGetRuntimeTestPlayerToken(out RuntimeTestPlayerTokenData token)
                && TryGetRuntimeGridCoordinateFromScreenPoint(Input.mousePosition, out ChapterGridCoordinate coordinate)
                && token.Coordinate.Equals(coordinate);
        }

        private bool TryGetRuntimeGridCoordinateFromScreenPoint(Vector2 screenPoint, out ChapterGridCoordinate coordinate)
        {
            coordinate = ChapterGridCoordinate.Zero;
            if (m_rectMapOverlay == null
                || m_overlayTextureWidth <= 0
                || m_overlayTextureHeight <= 0
                || !RectTransformUtility.RectangleContainsScreenPoint(m_rectMapOverlay, screenPoint, GetMapAreaEventCamera())
                || !RectTransformUtility.ScreenPointToLocalPointInRectangle(m_rectMapOverlay, screenPoint, GetMapAreaEventCamera(), out Vector2 overlayLocalPoint)
                || !TryGetCurrentPreviewGridMetrics(out ChapterMapGridMetrics metrics))
            {
                return false;
            }

            Rect overlayRect = m_rectMapOverlay.rect;
            if (overlayRect.width <= 0f || overlayRect.height <= 0f)
            {
                return false;
            }

            float textureX = (overlayLocalPoint.x + overlayRect.width * 0.5f) / overlayRect.width * m_overlayTextureWidth;
            float textureY = (overlayLocalPoint.y + overlayRect.height * 0.5f) / overlayRect.height * m_overlayTextureHeight;
            if (textureX < 0f || textureX > m_overlayTextureWidth || textureY < 0f || textureY > m_overlayTextureHeight)
            {
                return false;
            }

            Vector2 editorLocalPoint = TexturePointToEditorLocalPoint(textureX, textureY);
            return ChapterMapGridUtility.TryGetCellCoordinateFromLocalPoint(editorLocalPoint, metrics, out coordinate);
        }

        private bool TryGetCurrentPreviewGridMetrics(out ChapterMapGridMetrics metrics)
        {
            metrics = default;
            if (m_currentChapter == null)
            {
                return false;
            }

            ChapterMapGridStateData gridState = m_currentChapter.MapGridState ?? new ChapterMapGridStateData();
            Vector2 editorOverlaySize = GetEditorOverlaySize(m_currentChapter);
            if (editorOverlaySize.x <= 0f || editorOverlaySize.y <= 0f)
            {
                return false;
            }

            float gridZoomScale = gridState.GridZoomScale > 0f ? gridState.GridZoomScale : 1f;
            metrics = ChapterMapGridUtility.CreateMetrics(
                new Rect(0f, 0f, editorOverlaySize.x, editorOverlaySize.y),
                BaseMapGridColumns,
                BaseMapGridRows,
                gridZoomScale,
                gridState.GridPanOffset);
            return true;
        }

        private Vector2 TexturePointToEditorLocalPoint(float textureX, float textureY)
        {
            return new Vector2(
                (textureX - m_overlayTextureWidth * 0.5f) / GetEditorToTextureScaleX() + m_overlayEditorMapPanOffset.x,
                (textureY - m_overlayTextureHeight * 0.5f) / GetEditorToTextureScaleY() + m_overlayEditorMapPanOffset.y);
        }

        private RuntimeTestPlayerTokenData GetOrCreateRuntimeTestPlayerToken()
        {
            int chapterKey = GetRuntimeTestPlayerChapterKey();
            if (!m_runtimeTestPlayerTokens.TryGetValue(chapterKey, out RuntimeTestPlayerTokenData token))
            {
                token = new RuntimeTestPlayerTokenData();
                m_runtimeTestPlayerTokens[chapterKey] = token;
            }

            return token;
        }

        private bool TryGetRuntimeTestPlayerToken(out RuntimeTestPlayerTokenData token)
        {
            return m_runtimeTestPlayerTokens.TryGetValue(GetRuntimeTestPlayerChapterKey(), out token);
        }

        private int GetRuntimeTestPlayerChapterKey()
        {
            return m_currentChapter != null ? m_currentChapter.ChapterId : -1;
        }

        private void RefreshMapOverlay(ChapterPreviewRuntimeData chapter)
        {
            CleanupOverlayResources();
            if (m_imgMapOverlay == null || chapter == null)
            {
                return;
            }

            if (m_mapTexture == null)
            {
                m_imgMapOverlay.gameObject.SetActive(false);
                SetGridCoordinateLabelCount(0);
                return;
            }

            Canvas.ForceUpdateCanvases();
            Vector2 overlaySize = GetBaseOverlaySize();
            m_overlayTextureWidth = Mathf.Clamp(Mathf.RoundToInt(overlaySize.x), 1, 4096);
            m_overlayTextureHeight = Mathf.Clamp(Mathf.RoundToInt(overlaySize.y), 1, 4096);
            ApplyChapterOverlayLayout();

            m_overlayTexture = new Texture2D(m_overlayTextureWidth, m_overlayTextureHeight, TextureFormat.RGBA32, false);
            m_overlayTexture.name = "ModulePreviewMapOverlay";
            m_overlayTexture.wrapMode = TextureWrapMode.Clamp;
            m_overlayTexture.filterMode = FilterMode.Bilinear;

            Color32[] clearPixels = new Color32[m_overlayTextureWidth * m_overlayTextureHeight];
            m_overlayTexture.SetPixels32(clearPixels);

            ChapterMapGridStateData gridState = chapter.MapGridState ?? new ChapterMapGridStateData();
            Vector2 editorOverlaySize = GetEditorOverlaySize(chapter);
            m_overlayEditorImageSize = CalculateEditorFittedMapSize(gridState, editorOverlaySize);
            m_overlayEditorMapPanOffset = gridState.MapPanOffset;
            float gridZoomScale = gridState.GridZoomScale > 0f ? gridState.GridZoomScale : 1f;
            ChapterMapGridMetrics metrics = ChapterMapGridUtility.CreateMetrics(
                new Rect(0f, 0f, editorOverlaySize.x, editorOverlaySize.y),
                BaseMapGridColumns,
                BaseMapGridRows,
                gridZoomScale,
                gridState.GridPanOffset);

            DrawTerrainCells(chapter.GridCells, metrics);
            DrawActiveCreatureInstances(chapter.CreatureInstances, metrics);
            DrawRuntimeTestPlayerToken(metrics);
            DrawGridLines(metrics);
            RefreshGridCoordinateLabels(metrics);

            m_overlayTexture.Apply(false, false);
            m_overlaySprite = Sprite.Create(
                m_overlayTexture,
                new Rect(0f, 0f, m_overlayTextureWidth, m_overlayTextureHeight),
                new Vector2(0.5f, 0.5f));
            m_imgMapOverlay.sprite = m_overlaySprite;
            m_imgMapOverlay.preserveAspect = false;
            m_imgMapOverlay.gameObject.SetActive(true);
        }

        private void DrawTerrainCells(List<ChapterGridCellData> gridCells, ChapterMapGridMetrics metrics)
        {
            if (gridCells == null)
            {
                return;
            }

            for (int index = 0; index < gridCells.Count; index++)
            {
                ChapterGridCellData gridCell = gridCells[index];
                if (gridCell == null)
                {
                    continue;
                }

                Color color;
                switch (gridCell.MarkType)
                {
                    case ChapterGridCellMarkType.DifficultTerrain:
                        color = new Color(0.92f, 0.68f, 0.22f, 0.34f);
                        break;
                    case ChapterGridCellMarkType.ImpassableTerrain:
                        color = new Color(0.86f, 0.18f, 0.16f, 0.42f);
                        break;
                    default:
                        continue;
                }

                Rect cellRect = LocalRectToTextureRect(ChapterMapGridUtility.GetLogicalCellRect(metrics, gridCell.Coordinate));
                DrawTextureRect(InsetRect(cellRect, 3f), color);
            }
        }

        private void DrawEventBindings(List<ChapterEventBindingData> eventBindings, ChapterMapGridMetrics metrics)
        {
            List<ChapterGridCoordinate> eventCoordinates = ChapterEventCollectionUtility.CollectBoundCoordinates(eventBindings);
            for (int index = 0; index < eventCoordinates.Count; index++)
            {
                Rect cellRect = LocalRectToTextureRect(ChapterMapGridUtility.GetLogicalCellRect(metrics, eventCoordinates[index]));
                float markerSize = Mathf.Clamp(Mathf.Min(cellRect.width, cellRect.height) * 0.32f, 14f, 34f);
                Rect markerRect = new Rect(
                    cellRect.center.x - markerSize * 0.5f,
                    cellRect.center.y - markerSize * 0.5f,
                    markerSize,
                    markerSize);
                DrawTextureRect(markerRect, new Color(1f, 0.72f, 0.16f, 0.92f));
            }
        }

        private void DrawActiveCreatureInstances(List<ChapterCreatureInstanceData> creatureInstances, ChapterMapGridMetrics metrics)
        {
            if (creatureInstances == null)
            {
                return;
            }

            for (int index = 0; index < creatureInstances.Count; index++)
            {
                ChapterCreatureInstanceData creatureInstance = creatureInstances[index];
                if (creatureInstance == null || !creatureInstance.IsActive)
                {
                    continue;
                }

                Rect tokenRect = LocalRectToTextureRect(GetCreatureTokenRect(metrics, creatureInstance));
                Color accentColor = creatureInstance.RuntimeSheet != null
                    ? creatureInstance.RuntimeSheet.AccentColor
                    : new Color(0.45f, 0.55f, 0.7f, 1f);
                Color frameColor = new Color(
                    Mathf.Clamp01(accentColor.r * 0.82f + 0.08f),
                    Mathf.Clamp01(accentColor.g * 0.82f + 0.08f),
                    Mathf.Clamp01(accentColor.b * 0.82f + 0.08f),
                    0.9f);
                Color fillColor = new Color(
                    Mathf.Clamp01(accentColor.r * 0.86f + 0.06f),
                    Mathf.Clamp01(accentColor.g * 0.86f + 0.06f),
                    Mathf.Clamp01(accentColor.b * 0.86f + 0.06f),
                    0.58f);

                DrawTextureRect(tokenRect, frameColor);
                Rect previewRect = InsetRect(tokenRect, Mathf.Clamp(Mathf.Min(tokenRect.width, tokenRect.height) * 0.08f, 3f, 8f));
                DrawTextureRect(previewRect, fillColor);
                TryDrawCreaturePreviewImage(creatureInstance.RuntimeSheet, previewRect);
            }
        }

        private bool TryDrawCreaturePreviewImage(ChapterCreatureData creature, Rect tokenRect)
        {
            if (creature == null || string.IsNullOrWhiteSpace(creature.PreviewImageFileName))
            {
                return false;
            }

            string previewPath = ChapterEditorPersistenceService.ResolveCreaturePreviewPath(creature.PreviewImageFileName);
            if (string.IsNullOrWhiteSpace(previewPath) || !File.Exists(previewPath))
            {
                return false;
            }

            Texture2D previewTexture = null;
            try
            {
                previewTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!previewTexture.LoadImage(File.ReadAllBytes(previewPath)))
                {
                    return false;
                }

                Rect fittedRect = FitRectPreserveAspect(tokenRect, previewTexture.width, previewTexture.height);
                DrawTextureImage(fittedRect, previewTexture, 0.96f);
                return true;
            }
            catch (Exception exception)
            {
                Log.Warning($"Failed to draw preview creature token image: {exception.Message}");
                return false;
            }
            finally
            {
                if (previewTexture != null)
                {
                    UnityEngine.Object.Destroy(previewTexture);
                }
            }
        }

        private void DrawRuntimeTestPlayerToken(ChapterMapGridMetrics metrics)
        {
            if (TryGetRuntimeTestPlayerToken(out RuntimeTestPlayerTokenData token)
                && (!m_isDraggingRuntimeTestPlayer || !m_isDraggingRuntimeTestPlayerFromPlacedToken))
            {
                DrawRuntimeTestPlayerTokenAtCoordinate(metrics, token.Coordinate, 0.9f);
            }

            if (m_isDraggingRuntimeTestPlayer && m_hasRuntimeTestPlayerDragCoordinate)
            {
                DrawRuntimeTestPlayerTokenAtCoordinate(metrics, m_runtimeTestPlayerDragCoordinate, 0.56f);
            }
        }

        private void DrawRuntimeTestPlayerTokenAtCoordinate(ChapterMapGridMetrics metrics, ChapterGridCoordinate coordinate, float alpha)
        {
            Rect cellRect = ChapterMapGridUtility.GetLogicalCellRect(metrics, coordinate);
            Rect tokenRect = LocalRectToTextureRect(cellRect);
            float inset = Mathf.Clamp(Mathf.Min(tokenRect.width, tokenRect.height) * 0.12f, 4f, 12f);
            Rect fillRect = InsetRect(tokenRect, inset);
            DrawTextureRect(tokenRect, new Color(0.08f, 0.42f, 0.92f, alpha));
            DrawTextureRect(fillRect, new Color(0.25f, 0.78f, 1f, Mathf.Clamp01(alpha + 0.08f)));
        }

        private static Rect FitRectPreserveAspect(Rect targetRect, float sourceWidth, float sourceHeight)
        {
            if (sourceWidth <= 0f || sourceHeight <= 0f || targetRect.width <= 0f || targetRect.height <= 0f)
            {
                return targetRect;
            }

            float sourceAspect = sourceWidth / sourceHeight;
            float targetAspect = targetRect.width / targetRect.height;
            if (sourceAspect > targetAspect)
            {
                float height = targetRect.width / sourceAspect;
                return new Rect(targetRect.xMin, targetRect.center.y - height * 0.5f, targetRect.width, height);
            }

            float width = targetRect.height * sourceAspect;
            return new Rect(targetRect.center.x - width * 0.5f, targetRect.yMin, width, targetRect.height);
        }

        private void DrawGridLines(ChapterMapGridMetrics metrics)
        {
            List<float> verticalLines = ChapterMapGridUtility.BuildCenteredGridLinePositions(
                metrics.OverlayWidth,
                metrics.CellWidth,
                metrics.DisplayOffsetX);
            for (int index = 0; index < verticalLines.Count; index++)
            {
                float editorLocalX = verticalLines[index] - metrics.OverlayWidth * 0.5f;
                float x = EditorLocalXToTextureX(editorLocalX);
                DrawTextureRect(new Rect(x - OverlayGridLineThickness * 0.5f, 0f, OverlayGridLineThickness, m_overlayTextureHeight), PreviewGridLineColor);
            }

            List<float> horizontalLines = ChapterMapGridUtility.BuildCenteredGridLinePositions(
                metrics.OverlayHeight,
                metrics.CellHeight,
                metrics.DisplayOffsetY);
            for (int index = 0; index < horizontalLines.Count; index++)
            {
                float editorLocalY = horizontalLines[index] - metrics.OverlayHeight * 0.5f;
                float y = EditorLocalYToTextureY(editorLocalY);
                DrawTextureRect(new Rect(0f, y - OverlayGridLineThickness * 0.5f, m_overlayTextureWidth, OverlayGridLineThickness), PreviewGridLineColor);
            }
        }

        private void RefreshGridCoordinateLabels(ChapterMapGridMetrics metrics)
        {
            if (m_rectMapOverlay == null)
            {
                SetGridCoordinateLabelCount(0);
                return;
            }

            float minX = m_overlayEditorMapPanOffset.x - m_overlayEditorImageSize.x * 0.5f;
            float maxX = m_overlayEditorMapPanOffset.x + m_overlayEditorImageSize.x * 0.5f;
            float minY = m_overlayEditorMapPanOffset.y - m_overlayEditorImageSize.y * 0.5f;
            float maxY = m_overlayEditorMapPanOffset.y + m_overlayEditorImageSize.y * 0.5f;
            List<ChapterGridCoordinate> visibleCoordinates = BuildVisibleGridCoordinates(metrics, minX, maxX, minY, maxY);

            SetGridCoordinateLabelCount(visibleCoordinates.Count);
            for (int index = 0; index < visibleCoordinates.Count; index++)
            {
                ChapterGridCoordinate coordinate = visibleCoordinates[index];
                Rect cellRect = ChapterMapGridUtility.GetLogicalCellRect(metrics, coordinate);
                float textureX = EditorLocalXToTextureX(cellRect.xMax);
                float textureY = EditorLocalYToTextureY(cellRect.yMin);
                if (textureX < 0f || textureX > m_overlayTextureWidth || textureY < 0f || textureY > m_overlayTextureHeight)
                {
                    m_gridCoordinateLabels[index].gameObject.SetActive(false);
                    continue;
                }

                TMP_Text label = m_gridCoordinateLabels[index];
                RectTransform rectTransform = label.rectTransform;
                rectTransform.anchorMin = new Vector2(textureX / Mathf.Max(1f, m_overlayTextureWidth), textureY / Mathf.Max(1f, m_overlayTextureHeight));
                rectTransform.anchorMax = rectTransform.anchorMin;
                rectTransform.pivot = new Vector2(1f, 0f);
                rectTransform.anchoredPosition = new Vector2(-GridCoordinateLabelInset, GridCoordinateLabelInset);
                Rect textureCellRect = LocalRectToTextureRect(cellRect);
                rectTransform.sizeDelta = new Vector2(Mathf.Max(36f, textureCellRect.width - GridCoordinateLabelInset * 2f), GridCoordinateLabelHeight);
                rectTransform.localScale = Vector3.one;
                label.fontSize = Mathf.Clamp(
                    Mathf.Min(textureCellRect.width, textureCellRect.height) * GridCoordinateLabelFontScale,
                    GridCoordinateLabelMinFontSize,
                    GridCoordinateLabelMaxFontSize);
                label.color = GetPreviewGridCoordinateLabelColor(textureX, textureY);
                label.text = FormatGridCoordinateLabel(coordinate);
                label.gameObject.SetActive(true);
            }
        }

        private void SetGridCoordinateLabelCount(int visibleCount)
        {
            if (m_rectMapOverlay == null)
            {
                return;
            }

            while (m_gridCoordinateLabels.Count < visibleCount)
            {
                GameObject labelObject = new GameObject($"GridCoordinateLabel_{m_gridCoordinateLabels.Count}", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                labelObject.transform.SetParent(m_rectMapOverlay, false);
                TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
                ApplyGridCoordinateLabelStyle(label, m_tmpMapPlaceholder);
                m_gridCoordinateLabels.Add(label);
            }

            for (int index = 0; index < m_gridCoordinateLabels.Count; index++)
            {
                m_gridCoordinateLabels[index].gameObject.SetActive(index < visibleCount);
                if (index < visibleCount)
                {
                    m_gridCoordinateLabels[index].transform.SetAsLastSibling();
                }
            }
        }

        private static List<ChapterGridCoordinate> BuildVisibleGridCoordinates(ChapterMapGridMetrics metrics, float minX, float maxX, float minY, float maxY)
        {
            List<ChapterGridCoordinate> result = new List<ChapterGridCoordinate>();
            if (metrics.CellWidth <= 0f || metrics.CellHeight <= 0f)
            {
                return result;
            }

            int minCellX = Mathf.FloorToInt((minX - metrics.LogicOriginX) / metrics.CellWidth);
            int maxCellX = Mathf.FloorToInt((maxX - metrics.LogicOriginX) / metrics.CellWidth);
            int minCellY = Mathf.FloorToInt((minY - metrics.LogicOriginY) / metrics.CellHeight);
            int maxCellY = Mathf.FloorToInt((maxY - metrics.LogicOriginY) / metrics.CellHeight);

            for (int cellY = minCellY; cellY <= maxCellY; cellY++)
            {
                for (int cellX = minCellX; cellX <= maxCellX; cellX++)
                {
                    if (result.Count >= MaxGridCoordinateLabels)
                    {
                        return result;
                    }

                    ChapterGridCoordinate coordinate = new ChapterGridCoordinate(cellX, cellY);
                    Rect cellRect = ChapterMapGridUtility.GetLogicalCellRect(metrics, coordinate);
                    if (cellRect.xMax <= minX || cellRect.xMin >= maxX || cellRect.yMax <= minY || cellRect.yMin >= maxY)
                    {
                        continue;
                    }

                    result.Add(coordinate);
                }
            }

            return result;
        }

        private static void ApplyGridCoordinateLabelStyle(TextMeshProUGUI label, TMP_Text styleSource)
        {
            if (label == null)
            {
                return;
            }

            if (styleSource != null)
            {
                label.font = styleSource.font;
                label.fontSharedMaterial = styleSource.fontSharedMaterial;
            }

            label.raycastTarget = false;
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Overflow;
            label.alignment = TextAlignmentOptions.BottomRight;
            label.color = new Color(1f, 1f, 1f, 0.88f);
        }

        private static string FormatGridCoordinateLabel(ChapterGridCoordinate coordinate)
        {
            return $"{coordinate.CellX},{coordinate.CellY}";
        }

        private Color GetPreviewGridCoordinateLabelColor(float textureX, float textureY)
        {
            if (m_mapTexture == null || m_overlayTextureWidth <= 0 || m_overlayTextureHeight <= 0)
            {
                return new Color(1f, 1f, 1f, 0.88f);
            }

            float u = Mathf.Clamp01(textureX / m_overlayTextureWidth);
            float v = Mathf.Clamp01(textureY / m_overlayTextureHeight);
            return GetContrastingGridCoordinateLabelColor(m_mapTexture.GetPixelBilinear(u, v));
        }

        private static Color GetContrastingGridCoordinateLabelColor(Color backgroundColor)
        {
            float luminance = backgroundColor.r * 0.2126f + backgroundColor.g * 0.7152f + backgroundColor.b * 0.0722f;
            return luminance > 0.52f
                ? new Color(0f, 0f, 0f, 0.88f)
                : new Color(1f, 1f, 1f, 0.92f);
        }

        private static Rect GetCreatureTokenRect(ChapterMapGridMetrics metrics, ChapterCreatureInstanceData creatureInstance)
        {
            if (creatureInstance == null)
            {
                return Rect.zero;
            }

            ChapterCreatureInstancePlacementData placement = creatureInstance.Placement ?? new ChapterCreatureInstancePlacementData();
            Rect anchorCellRect = ChapterMapGridUtility.GetLogicalCellRect(metrics, placement.AnchorCell);
            int cellSpan = Mathf.Max(1, ChapterCreatureDataStructureUtility.GetCreatureFootprintCellSpan(creatureInstance.RuntimeSheet));
            float previewScale = Mathf.Max(0.4f, placement.PreviewScale);
            Vector2 tokenSize = new Vector2(
                Mathf.Max(18f, metrics.CellWidth * cellSpan * previewScale),
                Mathf.Max(18f, metrics.CellHeight * cellSpan * previewScale));
            return new Rect(anchorCellRect.xMin, anchorCellRect.yMin, tokenSize.x, tokenSize.y);
        }

        private Rect LocalRectToTextureRect(Rect localRect)
        {
            float scaleX = GetEditorToTextureScaleX();
            float scaleY = GetEditorToTextureScaleY();
            return new Rect(
                (localRect.xMin - m_overlayEditorMapPanOffset.x) * scaleX + m_overlayTextureWidth * 0.5f,
                (localRect.yMin - m_overlayEditorMapPanOffset.y) * scaleY + m_overlayTextureHeight * 0.5f,
                localRect.width * scaleX,
                localRect.height * scaleY);
        }

        private float EditorLocalXToTextureX(float editorLocalX)
        {
            return (editorLocalX - m_overlayEditorMapPanOffset.x) * GetEditorToTextureScaleX() + m_overlayTextureWidth * 0.5f;
        }

        private float EditorLocalYToTextureY(float editorLocalY)
        {
            return (editorLocalY - m_overlayEditorMapPanOffset.y) * GetEditorToTextureScaleY() + m_overlayTextureHeight * 0.5f;
        }

        private float GetEditorToTextureScaleX()
        {
            return m_overlayTextureWidth / Mathf.Max(1f, m_overlayEditorImageSize.x);
        }

        private float GetEditorToTextureScaleY()
        {
            return m_overlayTextureHeight / Mathf.Max(1f, m_overlayEditorImageSize.y);
        }

        private static Rect InsetRect(Rect rect, float inset)
        {
            float safeInset = Mathf.Max(0f, Mathf.Min(inset, rect.width * 0.45f, rect.height * 0.45f));
            return new Rect(rect.xMin + safeInset, rect.yMin + safeInset, rect.width - safeInset * 2f, rect.height - safeInset * 2f);
        }

        private void DrawTextureRect(Rect rect, Color color)
        {
            if (m_overlayTexture == null || rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            int xMin = Mathf.Clamp(Mathf.FloorToInt(rect.xMin), 0, m_overlayTextureWidth);
            int xMax = Mathf.Clamp(Mathf.CeilToInt(rect.xMax), 0, m_overlayTextureWidth);
            int yMin = Mathf.Clamp(Mathf.FloorToInt(rect.yMin), 0, m_overlayTextureHeight);
            int yMax = Mathf.Clamp(Mathf.CeilToInt(rect.yMax), 0, m_overlayTextureHeight);
            if (xMax <= xMin || yMax <= yMin)
            {
                return;
            }

            Color32 pixelColor = color;
            for (int y = yMin; y < yMax; y++)
            {
                for (int x = xMin; x < xMax; x++)
                {
                    m_overlayTexture.SetPixel(x, y, pixelColor);
                }
            }
        }

        private void DrawTextureImage(Rect rect, Texture2D sourceTexture, float alpha)
        {
            if (m_overlayTexture == null || sourceTexture == null || rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            int xMin = Mathf.Clamp(Mathf.FloorToInt(rect.xMin), 0, m_overlayTextureWidth);
            int xMax = Mathf.Clamp(Mathf.CeilToInt(rect.xMax), 0, m_overlayTextureWidth);
            int yMin = Mathf.Clamp(Mathf.FloorToInt(rect.yMin), 0, m_overlayTextureHeight);
            int yMax = Mathf.Clamp(Mathf.CeilToInt(rect.yMax), 0, m_overlayTextureHeight);
            if (xMax <= xMin || yMax <= yMin)
            {
                return;
            }

            for (int y = yMin; y < yMax; y++)
            {
                float v = Mathf.Clamp01((y + 0.5f - rect.yMin) / rect.height);
                for (int x = xMin; x < xMax; x++)
                {
                    float u = Mathf.Clamp01((x + 0.5f - rect.xMin) / rect.width);
                    Color sourceColor = sourceTexture.GetPixelBilinear(u, v);
                    sourceColor.a *= alpha;
                    BlendOverlayPixel(x, y, sourceColor);
                }
            }
        }

        private void BlendOverlayPixel(int x, int y, Color sourceColor)
        {
            Color destinationColor = m_overlayTexture.GetPixel(x, y);
            float sourceAlpha = sourceColor.a;
            float destinationAlpha = destinationColor.a;
            float outputAlpha = sourceAlpha + destinationAlpha * (1f - sourceAlpha);
            if (outputAlpha <= 0f)
            {
                m_overlayTexture.SetPixel(x, y, Color.clear);
                return;
            }

            Color outputColor = new Color(
                (sourceColor.r * sourceAlpha + destinationColor.r * destinationAlpha * (1f - sourceAlpha)) / outputAlpha,
                (sourceColor.g * sourceAlpha + destinationColor.g * destinationAlpha * (1f - sourceAlpha)) / outputAlpha,
                (sourceColor.b * sourceAlpha + destinationColor.b * destinationAlpha * (1f - sourceAlpha)) / outputAlpha,
                outputAlpha);
            m_overlayTexture.SetPixel(x, y, outputColor);
        }

        private void RefreshRuntimeLog()
        {
            if (m_currentChapter == null)
            {
                SetText(m_tmpRuntimeLog, string.Empty);
                return;
            }

            int chapterCount = m_previewSession?.Chapters?.Count ?? 0;
            string runtimeLog = BuildRuntimeLog(m_currentChapter, chapterCount);
            List<string> playerLines = new List<string>
            {
                string.Empty,
                "测试角色:",
                "从本日志区域按住拖动到地图网格，用于预览触发与行走测试。"
            };

            if (TryGetRuntimeTestPlayerToken(out RuntimeTestPlayerTokenData token))
            {
                playerLines.Add($"{token.Name}: {FormatGridCoordinateLabel(token.Coordinate)}");
            }
            else
            {
                playerLines.Add("当前尚未放置。");
            }

            if (m_isDraggingRuntimeTestPlayer)
            {
                playerLines.Add(m_hasRuntimeTestPlayerDragCoordinate
                    ? $"拖动目标: {FormatGridCoordinateLabel(m_runtimeTestPlayerDragCoordinate)}"
                    : "拖动目标: 请移动到地图图片范围内。");
            }

            SetText(m_tmpRuntimeLog, runtimeLog + "\n" + string.Join("\n", playerLines));
        }

        private void RefreshControlButtons()
        {
            int chapterCount = m_previewSession?.Chapters?.Count ?? 0;
            int chapterIndex = m_previewSession?.CurrentChapterIndex ?? -1;
            SetButtonInteractable(m_btnRestartPreview, chapterCount > 0);
            SetButtonInteractable(m_btnPrevChapter, chapterIndex > 0);
            SetButtonInteractable(m_btnNextChapter, chapterIndex >= 0 && chapterIndex < chapterCount - 1);
        }

        private static string BuildTerrainInfo(ChapterPreviewRuntimeData chapter)
        {
            List<string> lines = new List<string>();
            if (!string.IsNullOrWhiteSpace(chapter.TerrainTag))
            {
                lines.Add(chapter.TerrainTag);
            }

            if (!string.IsNullOrWhiteSpace(chapter.TerrainSubTag))
            {
                lines.Add(chapter.TerrainSubTag);
            }

            if (!string.IsNullOrWhiteSpace(chapter.AddMapHint))
            {
                lines.Add(chapter.AddMapHint);
            }

            int difficultCount = CountGridCells(chapter.GridCells, ChapterGridCellMarkType.DifficultTerrain);
            int impassableCount = CountGridCells(chapter.GridCells, ChapterGridCellMarkType.ImpassableTerrain);
            if (difficultCount > 0 || impassableCount > 0)
            {
                lines.Add($"困难地形 {difficultCount} 格，不可通行 {impassableCount} 格");
            }

            return BuildInfoBlock("地形说明", lines.Count > 0 ? string.Join("\n", lines) : string.Empty);
        }

        private static string BuildCreatureInfo(ChapterPreviewRuntimeData chapter)
        {
            List<ChapterCreatureInstanceData> activeCreatures = GetActiveCreatureInstances(chapter.CreatureInstances);
            List<string> lines = new List<string>
            {
                $"已激活实例: {activeCreatures.Count}"
            };

            for (int index = 0; index < activeCreatures.Count; index++)
            {
                ChapterCreatureInstanceData creature = activeCreatures[index];
                string creatureName = creature.RuntimeSheet != null && !string.IsNullOrWhiteSpace(creature.RuntimeSheet.Name)
                    ? creature.RuntimeSheet.Name
                    : creature.InstanceId;
                lines.Add($"{index + 1}. {creatureName}");
            }

            if (!string.IsNullOrWhiteSpace(chapter.CreatureInfo))
            {
                lines.Add(string.Empty);
                lines.Add(chapter.CreatureInfo);
            }

            return BuildInfoBlock("生物实例", string.Join("\n", lines));
        }

        private static string BuildRuntimeLog(ChapterPreviewRuntimeData chapter, int chapterCount)
        {
            int activeCreatureCount = GetActiveCreatureInstances(chapter.CreatureInstances).Count;
            int eventCount = chapter.Events?.Count ?? 0;
            int bindingCount = chapter.EventBindings?.Count ?? 0;
            return
                $"已进入预览会话。\n" +
                $"当前章节: 第 {chapter.ChapterIndex + 1} 章 / 共 {chapterCount} 章\n" +
                $"地图: {(string.IsNullOrWhiteSpace(chapter.MapImagePath) ? "未配置" : "已配置")}\n" +
                $"激活生物实例: {activeCreatureCount}\n" +
                $"事件: {eventCount} 个，绑定区域: {bindingCount} 个";
        }

        private static string BuildCombinedChapterInfo(ChapterPreviewRuntimeData chapter)
        {
            return string.Join(
                "\n\n",
                BuildInfoBlock("章节目标", chapter.Goal),
                BuildInfoBlock("章节正文", chapter.Content),
                BuildInfoBlock("DM 备注", chapter.DmNote),
                BuildTerrainInfo(chapter),
                BuildCreatureInfo(chapter));
        }

        private static string BuildInfoBlock(string title, string content)
        {
            return string.IsNullOrWhiteSpace(content)
                ? $"{title}\n-"
                : $"{title}\n{content.Trim()}";
        }

        private static List<ChapterCreatureInstanceData> GetActiveCreatureInstances(List<ChapterCreatureInstanceData> creatureInstances)
        {
            List<ChapterCreatureInstanceData> result = new List<ChapterCreatureInstanceData>();
            if (creatureInstances == null)
            {
                return result;
            }

            for (int index = 0; index < creatureInstances.Count; index++)
            {
                ChapterCreatureInstanceData creature = creatureInstances[index];
                if (creature != null && creature.IsActive)
                {
                    result.Add(creature);
                }
            }

            return result;
        }

        private static int CountGridCells(List<ChapterGridCellData> gridCells, ChapterGridCellMarkType markType)
        {
            if (gridCells == null)
            {
                return 0;
            }

            int count = 0;
            for (int index = 0; index < gridCells.Count; index++)
            {
                if (gridCells[index] != null && gridCells[index].MarkType == markType)
                {
                    count++;
                }
            }

            return count;
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value ?? string.Empty;
            }
        }

        private static void SetButtonInteractable(Button button, bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private void CleanupMapResources()
        {
            if (m_imgChapterMap != null)
            {
                m_imgChapterMap.sprite = null;
            }

            if (m_mapSprite != null)
            {
                UnityEngine.Object.Destroy(m_mapSprite);
                m_mapSprite = null;
            }

            if (m_mapTexture != null)
            {
                UnityEngine.Object.Destroy(m_mapTexture);
                m_mapTexture = null;
            }
        }

        private void CleanupOverlayResources()
        {
            SetGridCoordinateLabelCount(0);

            if (m_imgMapOverlay != null)
            {
                m_imgMapOverlay.sprite = null;
            }

            if (m_overlaySprite != null)
            {
                UnityEngine.Object.Destroy(m_overlaySprite);
                m_overlaySprite = null;
            }

            if (m_overlayTexture != null)
            {
                UnityEngine.Object.Destroy(m_overlayTexture);
                m_overlayTexture = null;
            }
        }
    }
}
