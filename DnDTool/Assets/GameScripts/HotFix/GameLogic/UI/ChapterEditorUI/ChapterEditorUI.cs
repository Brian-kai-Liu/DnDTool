using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using TMPro;
using TEngine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GameLogic
{
    [Window(UILayer.UI, location : "ChapterEditorUI", fullScreen : true)]
    public partial class ChapterEditorUI
    {
        private enum TerrainToolButtonType
        {
            DifficultTerrain,
            ImpassableTerrain,
            ClearTerrain,
        }

        private const float ChapterItemHeight = 60f;
        private const float ChapterItemSpacing = 10f;
        private const float MapPreviewPadding = 18f;
        private const float CreatureBrowserPadding = 14f;
        private const float CreatureCardHeight = 92f;
        private const float CreatureCardSpacing = 10f;
        private const int BaseMapGridColumns = 11;
        private const int BaseMapGridRows = 6;
        private const float MapZoomMinScale = 0.25f;
        private const float MapZoomMaxScale = 5f;
        private const float MapZoomScrollStep = 0.12f;
        private const float GridZoomMinScale = 0.05f;
        private const float GridZoomMaxScale = 20f;
        private const float GridZoomScrollStep = 0.02f;
        private const float FineZoomScrollStep = 0.005f;
        private const float GridLineThickness = 4f;
        private const float GridFrameThickness = 4f;
        private const float GridSelectionClickThreshold = 10f;
        private const float GridEventMarkerSize = 18f;
        private const float GridCoordinateLabelInset = 3f;
        private const float GridCoordinateLabelHeight = 20f;
        private const float GridCoordinateLabelFontScale = 0.22f;
        private const float GridCoordinateLabelMinFontSize = 10f;
        private const float GridCoordinateLabelMaxFontSize = 18f;
        private const int MaxGridCoordinateLabels = 360;
        private const string ChapterEditorSaveFileName = "chapter_editor_state.json";

        private readonly List<ChapterListItemData> m_chapterItems = new List<ChapterListItemData>();
        private readonly List<ChapterListItemView> m_chapterItemViews = new List<ChapterListItemView>();
        private TMP_Text m_textCreatureInfoBoard = null!;
        private TMP_Text m_textTerrainTag = null!;
        private TMP_Text m_textTerrainSubTag = null!;
        private TMP_Text m_textCreatureBrowserSubtitle = null!;
        private int m_selectedChapterId = -1;
        private int m_nextChapterId = 1;
        private Canvas m_canvas = null!;
        private RectTransform m_addChapterButtonRect = null!;
        private Image m_addChapterButtonImage = null!;
        private RectTransform m_rectChapterListItemTemplate = null!;
        private TMP_Text m_textAddMapHint = null!;
        private TMP_Text m_textUploadMapButton = null!;
        private RectTransform m_btnOpenCheckPopupRect = null!;
        private RectTransform m_btnDeleteGridEventRect = null!;
        private RectTransform m_btnUploadMapRect = null!;
        private RectTransform m_btnMapZoomToggleRect = null!;
        private RectTransform m_btnGridZoomToggleRect = null!;
        private RectTransform m_btnSaveChapterStateRect = null!;
        private RectTransform m_btnPreviewModuleRect = null!;
        private RectTransform m_btnDifficultTerrainRect = null!;
        private RectTransform m_btnImpassableTerrainRect = null!;
        private RectTransform m_btnClearTerrainRect = null!;
        private Image m_imgMapZoomToggle = null!;
        private Image m_imgGridZoomToggle = null!;
        private Image m_imgSaveChapterState = null!;
        private Image m_imgPreviewModule = null!;
        private Image m_imgDeleteGridEvent = null!;
        private Image m_imgDifficultTerrain = null!;
        private Image m_imgImpassableTerrain = null!;
        private Image m_imgClearTerrain = null!;
        private RectTransform m_rectMapSurface = null!;
        private GameObject m_goCreatureBrowserRoot = null!;
        private Image m_imgCreatureBrowserRootBackground = null!;
        private Image m_imgCreatureListPanelBackground = null!;
        private RectTransform m_rectCreatureCardContainer = null!;
        private RectTransform m_rectCreatureCardTemplate = null!;
        private Image m_imgCreatureDetailPanelBackground = null!;
        private RectTransform m_rectCreatureDetailTemplate = null!;
        private CanvasGroup m_saveFeedbackCanvasGroup = null!;
        private readonly List<ChapterCreatureCardWidget> m_creatureCardWidgets = new List<ChapterCreatureCardWidget>();
        private readonly List<ChapterCreatureStaticCardData> m_creatureSeedCards = new List<ChapterCreatureStaticCardData>();
        private readonly List<ChapterCreatureStaticCardData> m_creatureRuntimeCards = new List<ChapterCreatureStaticCardData>();
        private readonly List<ChapterCreatureStaticCardData> m_creatureAllCards = new List<ChapterCreatureStaticCardData>();
        private readonly List<int> m_creatureFilteredCardIndices = new List<int>();
        private ChapterCreatureDetailWidget m_creatureDetailWidget;
        private int m_selectedCreatureCardIndex = -1;
        private string m_creatureSearchKeyword = string.Empty;
        private bool m_ignoreCreatureSearchValueChanged;
        private string m_defaultTerrainTagText = string.Empty;
        private string m_defaultTerrainSubTagText = string.Empty;
        private string m_defaultAddMapHintText = string.Empty;
        private string m_defaultCreatureInfoText = string.Empty;
        private string m_defaultMapUploadHintText = string.Empty;
        private TMP_Text m_textMapZoomToggle = null!;
        private TMP_Text m_textGridZoomToggle = null!;
        private TMP_Text m_textSaveChapterState = null!;
        private TMP_Text m_textPreviewModule = null!;
        private TMP_Text m_textDifficultTerrain = null!;
        private TMP_Text m_textImpassableTerrain = null!;
        private TMP_Text m_textClearTerrain = null!;
        private Texture2D m_mapPreviewTexture = null!;
        private Sprite m_mapPreviewSprite = null!;
        private Texture2D m_creatureDetailPreviewTexture = null!;
        private Sprite m_creatureDetailPreviewSprite = null!;
        private string m_loadedMapImagePath = string.Empty;
        private string m_loadedCreaturePreviewPath = string.Empty;
        private int m_mapPreviewLoadVersion;
        private int m_creatureDetailPreviewLoadVersion;
        private int m_saveFeedbackVersion;
        private bool m_isMapPreviewLoading;
        private float m_mapPreviewZoomScale = 1f;
        private Vector2 m_mapPreviewPanOffset = Vector2.zero;
        private float m_mapGridZoomScale = 1f;
        private Vector2 m_mapGridPanOffset = Vector2.zero;
        private readonly List<RectTransform> m_verticalGridLines = new List<RectTransform>();
        private readonly List<RectTransform> m_horizontalGridLines = new List<RectTransform>();
        private readonly List<Image> m_gridSelectionHighlights = new List<Image>();
        private readonly List<Image> m_gridEventMarkers = new List<Image>();
        private readonly List<TMP_Text> m_gridCoordinateLabels = new List<TMP_Text>();
        private readonly List<ChapterCreatureMapTokenWidget> m_creatureInstanceTokenWidgets = new List<ChapterCreatureMapTokenWidget>();
        private readonly List<RaycastResult> m_creatureBoardPointerHits = new List<RaycastResult>();
        private RectTransform m_rectCreaturePlacementPreview = null!;
        private Image m_imgCreaturePlacementPreview = null!;
        private RectTransform m_verticalGridLineTemplate = null!;
        private RectTransform m_horizontalGridLineTemplate = null!;
        private float m_lockedMapZoomReference = 1f;
        private float m_lockedGridToMapZoomRatio = 1f;
        private Vector2 m_lockedGridToMapPanDelta = Vector2.zero;
        private RectTransform m_dragPlaceholder = null!;
        private int m_draggingChapterIndex = -1;
        private int m_dragPreviewIndex = -1;
        private bool m_isDraggingChapter;
        private bool m_isApplyingStructureChange;
        private bool m_isMapZoomEnabled;
        private bool m_isGridZoomEnabled;
        private bool m_isMapGridLocked;
        private bool m_isDraggingMap;
        private bool m_isDraggingGrid;
        private bool m_isDraggingLockedMapGrid;
        private bool m_isPendingGridCellSelection;
        private bool m_isPendingGridCellEventPopup;
        private Vector2 m_lastMapDragLocalPoint;
        private Vector2 m_lastGridDragLocalPoint;
        private Vector2 m_lastLockedMapGridDragLocalPoint;
        private Vector2 m_gridCellSelectionMouseDownLocalPoint;
        private Vector2 m_gridCellEventPopupMouseDownLocalPoint;
        private ChapterGridCoordinate m_gridCellSelectionMouseDownCoordinate = ChapterGridCoordinate.Zero;
        private ChapterGridCoordinate m_gridCellEventPopupMouseDownCoordinate = ChapterGridCoordinate.Zero;
        private bool m_isDraggingCreatureDeployment;
        private ChapterCreatureData m_draggingCreatureTemplate = null!;
        private ChapterCreatureMapTokenWidget m_creatureDragPreviewWidget;
        private string m_selectedCreatureInstanceId = string.Empty;
        private bool m_isPendingCreatureInstanceSelection;
        private bool m_isDraggingCreatureInstance;
        private string m_pendingCreatureInstanceId = string.Empty;
        private string m_draggingCreatureInstanceId = string.Empty;
        private Vector2 m_creatureInstanceMouseDownLocalPoint;
        private RectTransform m_rectCreatureInstanceActionPanel = null!;
        private RectTransform m_btnCreatureInstanceEditRect = null!;
        private RectTransform m_btnCreatureInstanceToggleActiveRect = null!;
        private RectTransform m_btnCreatureInstanceDeleteRect = null!;
        private Button m_btnCreatureInstanceEdit = null!;
        private Button m_btnCreatureInstanceToggleActive = null!;
        private Button m_btnCreatureInstanceDelete = null!;
        private Button m_btnPreviewModule = null!;
        private TMP_Text m_textCreatureInstanceToggleActive = null!;

        protected override void OnCreate()
        {
            LoadChapterEditorState();
            m_canvas = gameObject.GetComponent<Canvas>();
            m_addChapterButtonRect = m_btnAddChapter != null ? m_btnAddChapter.GetComponent<RectTransform>() : null!;
            m_addChapterButtonImage = m_btnAddChapter != null ? m_btnAddChapter.targetGraphic as Image : null!;

            SetupChapterDetailPanel();
            SetupChapterList();
        }

        #region 浜嬩欢

        private partial void OnClickAddChapterBtn()
        {
            if (m_isDraggingChapter)
            {
                return;
            }

            ApplyChapterStructureChange(() =>
            {
                ChapterListItemData chapter = CreateChapterItemData();
                int insertIndex = Mathf.Clamp(m_chapterItems.Count, 0, m_chapterItems.Count);
                m_chapterItems.Insert(insertIndex, chapter);
            });
        }

        private partial void OnClickBackBtn()
        {
            GameModule.UI.CloseUI<ChapterEditorUI>();
            GameModule.UI.ShowUIAsync<CreateModuleBasicInfoUI>();
        }

        private partial void OnClickUploadMapBtn()
        {
            OnClickUploadMap();
        }

        private partial void OnClickMapZoomToggleBtn()
        {
            OnClickMapZoomToggle();
        }

        private partial void OnClickGridZoomToggleBtn()
        {
            OnClickGridZoomToggle();
        }

        private partial void OnClickSaveChapterStateBtn()
        {
            OnClickSaveChapterState();
        }

        private void OnClickPreviewModuleBtn()
        {
            OnClickPreviewModule();
        }

        private partial void OnClickDifficultTerrainBtn()
        {
            OnClickDifficultTerrain();
        }

        private partial void OnClickImpassableTerrainBtn()
        {
            OnClickImpassableTerrain();
        }

        private partial void OnClickClearTerrainBtn()
        {
            OnClickClearTerrain();
        }

        private partial void OnClickDeleteGridEventBtn()
        {
            OnClickDeleteGridEvent();
        }

        private partial void OnClickOpenCheckPopupBtn()
        {
            OpenChapterEventPopup();
        }

        private partial void OnClickOpenCreatureEntryPopupBtn()
        {
            OpenCreatureEntryPopup();
        }

        #endregion

        protected override void OnDestroy()
        {
            SyncChapterInputsToData();
            SaveChapterEditorState();

            for (int index = 0; index < m_chapterItemViews.Count; index++)
            {
                m_chapterItemViews[index].Dispose();
            }

            m_chapterItemViews.Clear();
            DisposeCreatureBrowserViews();
            DisposeCreatureTokenViews();

            if (m_dragPlaceholder != null)
            {
                Object.Destroy(m_dragPlaceholder.gameObject);
            }

            CleanupMapPreviewResources();
            CleanupCreatureDetailPreviewResources();
        }

        protected override void OnUpdate()
        {
            if (PopupWindowInputBlocker.ShouldBlockUnderlyingPointerInput())
            {
                m_isDraggingMap = false;
                m_isDraggingGrid = false;
                m_isDraggingLockedMapGrid = false;
                CancelPendingGridCellSelection();
                HideCreaturePlacementPreview();
                return;
            }

            HandleCreatureBoardPointerProbe();

            if (m_rectMapPreview == null || !m_rectMapPreview.gameObject.activeInHierarchy)
            {
                m_isDraggingMap = false;
                m_isDraggingGrid = false;
                m_isDraggingLockedMapGrid = false;
                HideCreaturePlacementPreview();
                return;
            }

            if (m_isMapGridLocked)
            {
                HandleLockedMapGridDragging();
            }
            else
            {
                HandleMapDragging();
                HandleGridDragging();
            }

            HandleCreatureInstanceInteraction();
            HandleGridCellSelection();
            UpdateDraggingCreaturePlacementPreview();

            if (!IsMouseOverMapPreview())
            {
                return;
            }

            float scrollDelta = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scrollDelta) < 0.01f)
            {
                return;
            }

            if (m_isMapGridLocked)
            {
                ApplyLockedMapGridScroll(scrollDelta);
                return;
            }

            if (m_isGridZoomEnabled)
            {
                float gridScrollDelta = GetAppliedZoomScrollDelta(scrollDelta, GridZoomScrollStep);
                m_mapGridZoomScale = Mathf.Clamp(m_mapGridZoomScale + gridScrollDelta, GridZoomMinScale, GridZoomMaxScale);
                ApplyMapGridLayout();
                SaveChapterEditorState();
            }

            if (!m_isMapZoomEnabled || m_mapPreviewTexture == null)
            {
                return;
            }

            float mapScrollDelta = GetAppliedZoomScrollDelta(scrollDelta, MapZoomScrollStep);
            m_mapPreviewZoomScale = Mathf.Clamp(m_mapPreviewZoomScale + mapScrollDelta, MapZoomMinScale, MapZoomMaxScale);
            UpdateMapSurfaceLayoutToFit();
            SaveChapterEditorState();
        }

        private void HandleCreatureBoardPointerProbe()
        {
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            RectTransform creatureRect = m_rectCreatureBrowserRoot;
            if (creatureRect == null && m_textCreatureInfoBoard != null)
            {
                creatureRect = m_textCreatureInfoBoard.GetComponent<RectTransform>();
            }

            if (creatureRect == null)
            {
                return;
            }

            Camera eventCamera = m_canvas != null ? m_canvas.worldCamera : null;
            if (!RectTransformUtility.RectangleContainsScreenPoint(creatureRect, Input.mousePosition, eventCamera))
            {
                return;
            }

            if (EventSystem.current == null)
            {
                Log.Warning("Creature board pointer probe: current scene has no EventSystem.");
                return;
            }

            PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            m_creatureBoardPointerHits.Clear();
            EventSystem.current.RaycastAll(pointerEventData, m_creatureBoardPointerHits);
            if (m_creatureBoardPointerHits.Count <= 0)
            {
                Log.Warning($"生物区域点击探针: 未命中任何 UI。鼠标位置 {Input.mousePosition}");
                return;
            }

            int count = Mathf.Min(8, m_creatureBoardPointerHits.Count);
            string[] hitNames = new string[count];
            for (int index = 0; index < count; index++)
            {
                RaycastResult hit = m_creatureBoardPointerHits[index];
                hitNames[index] = $"{index + 1}.{hit.gameObject.name}";
            }
        }

        private void SetupChapterList()
        {
            m_rectChapterListItemTemplate = m_itemChapterListItemTemplate != null
                ? m_itemChapterListItemTemplate.GetComponent<RectTransform>()
                : null!;

            if (m_rectChapterListItemTemplate != null)
            {
                m_rectChapterListItemTemplate.gameObject.SetActive(false);
            }

            SetupListAddButton();

            if (m_chapterItems.Count == 0)
            {
                m_selectedChapterId = -1;
                RefreshChapterList(false);
            }
            else
            {
                RefreshChapterList(false);
            }
        }

        private ChapterListItemData CreateChapterItemData()
        {
            return new ChapterListItemData
            {
                Id = m_nextChapterId++,
                Name = string.Empty,
                Goal = string.Empty,
                Content = string.Empty,
                DmNote = string.Empty,
                TerrainTag = string.Empty,
                TerrainSubTag = string.Empty,
                AddMapHint = string.Empty,
                CreatureInfo = string.Empty,
                MapGridState = new ChapterMapGridStateData(),
                GridCells = new List<ChapterGridCellData>(),
                Events = new List<ChapterGridEventData>(),
                EventBindings = new List<ChapterEventBindingData>(),
                Creatures = new List<ChapterCreatureData>(),
                CreatureInstances = new List<ChapterCreatureInstanceData>(),
            };
        }

        private void RefreshChapterList(bool syncInputsBeforeRefresh = true)
        {
            if (m_rectChapterListContent == null || m_rectChapterListItemTemplate == null)
            {
                return;
            }

            EnsureDragPlaceholder();
            if (syncInputsBeforeRefresh && !m_isApplyingStructureChange)
            {
                SyncChapterInputsToData();
            }

            while (m_chapterItemViews.Count < m_chapterItems.Count)
            {
                GameObject itemObject = Object.Instantiate(m_rectChapterListItemTemplate.gameObject, m_rectChapterListContent, false);
                itemObject.SetActive(true);
                m_chapterItemViews.Add(new ChapterListItemView(itemObject));
            }

            while (m_chapterItemViews.Count > m_chapterItems.Count)
            {
                int lastIndex = m_chapterItemViews.Count - 1;
                m_chapterItemViews[lastIndex].Dispose();
                m_chapterItemViews.RemoveAt(lastIndex);
            }

            for (int index = 0; index < m_chapterItems.Count; index++)
            {
                ChapterListItemData chapter = m_chapterItems[index];
                m_chapterItemViews[index].SetVisible(true);
                m_chapterItemViews[index].SetLayout(index, ChapterItemHeight, ChapterItemSpacing);
                m_chapterItemViews[index].Bind(
                    chapter,
                    index + 1,
                    chapter.Id == m_selectedChapterId,
                    () => OnSelectChapter(chapter.Id),
                    () => OnDeleteChapter(chapter.Id),
                    value => OnChapterTitleEditEnd(chapter.Id, value));
                m_chapterItemViews[index].ConfigureDrag(
                    m_canvas,
                    m_rectChapterListContent,
                    index,
                    m_chapterItems.Count,
                    ChapterItemHeight,
                    ChapterItemSpacing,
                    OnReorderChapterItem,
                    OnChapterItemDragPreviewChanged,
                    OnChapterItemDragStateChanged);
                m_chapterItemViews[index].SetDragState(false);
                m_chapterItemViews[index].SetDeleteInteractable(!m_isDraggingChapter);
            }

            float contentHeight = m_chapterItems.Count > 0
                ? (m_chapterItems.Count + 1) * ChapterItemHeight + m_chapterItems.Count * ChapterItemSpacing
                : ChapterItemHeight;
            m_rectChapterListContent.sizeDelta = new Vector2(0f, contentHeight);

            RefreshAddChapterButtonLayout();
            RefreshChapterDetailPanel();

            UpdateChapterActionButtons();
            HideDragPreview();
        }

        private void OnSelectChapter(int chapterId)
        {
            if (m_isDraggingChapter)
            {
                return;
            }

            int chapterIndex = GetChapterIndexById(chapterId);
            if (chapterIndex < 0 || chapterIndex >= m_chapterItems.Count)
            {
                return;
            }

            SyncChapterInputsToData();
            m_selectedChapterId = chapterId;
            RefreshChapterList(false);
        }

        private void SyncChapterInputsToData()
        {
            int syncCount = Mathf.Min(m_chapterItems.Count, m_chapterItemViews.Count);
            for (int index = 0; index < syncCount; index++)
            {
                m_chapterItemViews[index].SyncToData(m_chapterItems[index]);
            }

            SyncSelectedChapterDetailToData();
        }

        private void OnReorderChapterItem(int fromIndex, int toIndex)
        {
            m_draggingChapterIndex = -1;
            m_dragPreviewIndex = -1;
            m_isDraggingChapter = false;

            if (fromIndex < 0 || fromIndex >= m_chapterItems.Count || toIndex < 0 || toIndex >= m_chapterItems.Count)
            {
                RefreshChapterList();
                return;
            }

            if (fromIndex == toIndex)
            {
                RefreshChapterList();
                return;
            }

            ApplyChapterStructureChange(() => MoveChapterItem(fromIndex, toIndex));
        }

        private void OnChapterItemDragPreviewChanged(int fromIndex, int toIndex)
        {
            m_draggingChapterIndex = fromIndex;
            m_dragPreviewIndex = Mathf.Clamp(toIndex, 0, Mathf.Max(0, m_chapterItems.Count - 1));
            UpdateDragPreviewLayout();
        }

        private void OnChapterItemDragStateChanged(int chapterIndex, bool isDragging)
        {
            m_isDraggingChapter = isDragging;
            UpdateChapterActionButtons();

            if (isDragging)
            {
                m_draggingChapterIndex = chapterIndex;
                if (m_dragPreviewIndex < 0)
                {
                    m_dragPreviewIndex = chapterIndex;
                }

                UpdateDragPreviewLayout();
                return;
            }

            HideDragPreview();
        }

        private void UpdateDragPreviewLayout()
        {
            if (m_draggingChapterIndex < 0 || m_dragPreviewIndex < 0 || m_draggingChapterIndex >= m_chapterItems.Count)
            {
                HideDragPreview();
                return;
            }

            EnsureDragPlaceholder();

            for (int index = 0; index < m_chapterItemViews.Count; index++)
            {
                ChapterListItemView view = m_chapterItemViews[index];
                if (index == m_draggingChapterIndex)
                {
                    view.SetDragState(true);
                    continue;
                }

                view.SetDragState(false);
                view.SetLayout(GetPreviewDisplayIndex(index), ChapterItemHeight, ChapterItemSpacing);
            }

            m_dragPlaceholder.gameObject.SetActive(true);
            SetPreviewLayout(m_dragPlaceholder, m_dragPreviewIndex, ChapterItemHeight, ChapterItemSpacing);
            m_dragPlaceholder.SetAsLastSibling();
        }

        private int GetPreviewDisplayIndex(int itemIndex)
        {
            if (m_draggingChapterIndex < 0 || m_dragPreviewIndex < 0 || itemIndex == m_draggingChapterIndex)
            {
                return itemIndex;
            }

            if (m_draggingChapterIndex < m_dragPreviewIndex)
            {
                if (itemIndex > m_draggingChapterIndex && itemIndex <= m_dragPreviewIndex)
                {
                    return itemIndex - 1;
                }

                return itemIndex;
            }

            if (itemIndex >= m_dragPreviewIndex && itemIndex < m_draggingChapterIndex)
            {
                return itemIndex + 1;
            }

            return itemIndex;
        }

        private void HideDragPreview()
        {
            if (m_dragPlaceholder != null)
            {
                m_dragPlaceholder.gameObject.SetActive(false);
            }

            for (int index = 0; index < m_chapterItemViews.Count; index++)
            {
                m_chapterItemViews[index].SetDragState(false);
            }

            RefreshAddChapterButtonLayout();
        }

        private void ApplyChapterStructureChange(System.Action changeAction)
        {
            if (changeAction == null || m_isApplyingStructureChange)
            {
                return;
            }

            m_isApplyingStructureChange = true;
            try
            {
                SyncChapterInputsToData();
                changeAction.Invoke();
            }
            finally
            {
                m_isApplyingStructureChange = false;
            }

            RefreshChapterList(false);
            SaveChapterEditorState();
        }

        private void MoveChapterItem(int fromIndex, int toIndex)
        {
            ChapterListItemData movedChapter = m_chapterItems[fromIndex];
            m_chapterItems.RemoveAt(fromIndex);
            m_chapterItems.Insert(toIndex, movedChapter);
        }

        private int GetSelectedChapterIndex()
        {
            return GetChapterIndexById(m_selectedChapterId);
        }

        private int GetChapterIndexById(int chapterId)
        {
            if (chapterId < 0)
            {
                return -1;
            }

            for (int index = 0; index < m_chapterItems.Count; index++)
            {
                if (m_chapterItems[index].Id == chapterId)
                {
                    return index;
                }
            }

            return -1;
        }

        private void UpdateChapterActionButtons()
        {
            if (m_btnAddChapter != null)
            {
                m_btnAddChapter.interactable = !m_isDraggingChapter;
            }

            for (int index = 0; index < m_chapterItemViews.Count; index++)
            {
                m_chapterItemViews[index].SetDeleteInteractable(!m_isDraggingChapter);
            }
        }

        private void OnDeleteChapter(int chapterId)
        {
            if (m_isDraggingChapter)
            {
                return;
            }

            int chapterIndex = GetChapterIndexById(chapterId);
            if (chapterIndex < 0 || chapterIndex >= m_chapterItems.Count)
            {
                return;
            }

            ApplyChapterStructureChange(() =>
            {
                bool isDeletedChapterSelected = m_selectedChapterId == chapterId;
                m_chapterItems.RemoveAt(chapterIndex);
                if (m_chapterItems.Count == 0)
                {
                    m_selectedChapterId = -1;
                    return;
                }

                if (isDeletedChapterSelected)
                {
                    int nextSelectedIndex = Mathf.Clamp(chapterIndex, 0, m_chapterItems.Count - 1);
                    m_selectedChapterId = m_chapterItems[nextSelectedIndex].Id;
                }
            });
        }

        private void OnChapterTitleEditEnd(int chapterId, string title)
        {
            if (m_selectedChapterId == chapterId && m_tmpStoryTitle != null)
            {
                string trimmedTitle = title?.Trim() ?? string.Empty;
                m_tmpStoryTitle.text = string.IsNullOrWhiteSpace(trimmedTitle)
                    ? string.Empty
                    : trimmedTitle;
            }
        }

        private void SetupListAddButton()
        {
            if (m_btnAddChapter == null || m_addChapterButtonRect == null || m_rectChapterListContent == null)
            {
                return;
            }

            m_addChapterButtonRect.SetParent(m_rectChapterListContent, false);
            m_addChapterButtonRect.localScale = Vector3.one;

            if (m_addChapterButtonImage != null)
            {
                m_addChapterButtonImage.color = new Color(0.8f, 0.87f, 0.92f, 1f);
            }

            RefreshAddChapterButtonLayout();
        }

        private void RefreshAddChapterButtonLayout()
        {
            if (m_addChapterButtonRect == null)
            {
                return;
            }

            SetPreviewLayout(m_addChapterButtonRect, m_chapterItems.Count, ChapterItemHeight, ChapterItemSpacing);
            m_addChapterButtonRect.SetAsLastSibling();
        }

        private void EnsureDragPlaceholder()
        {
            if (m_dragPlaceholder != null || m_rectChapterListContent == null)
            {
                return;
            }

            GameObject placeholderObject = new GameObject("ChapterDragPlaceholder", typeof(RectTransform), typeof(Image));
            placeholderObject.transform.SetParent(m_rectChapterListContent, false);
            placeholderObject.SetActive(false);

            m_dragPlaceholder = placeholderObject.GetComponent<RectTransform>();
            Image placeholderImage = placeholderObject.GetComponent<Image>();
            placeholderImage.color = new Color(0.33f, 0.62f, 0.94f, 0.18f);
            placeholderImage.raycastTarget = false;

            SetPreviewLayout(m_dragPlaceholder, 0, ChapterItemHeight, ChapterItemSpacing);
        }

        private static void SetPreviewLayout(RectTransform rectTransform, int index, float itemHeight, float itemSpacing)
        {
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(0f, -index * (itemHeight + itemSpacing));
            rectTransform.sizeDelta = new Vector2(0f, itemHeight);
            rectTransform.localScale = Vector3.one;
        }

        private ChapterEditorInputData CollectChapterEditorInputData()
        {
            return new ChapterEditorInputData
            {
                SelectedChapterIndex = GetSelectedChapterIndex(),
                Chapters = CollectChapterListData()
            };
        }

        private List<ChapterListItemData> CollectChapterListData()
        {
            SyncChapterInputsToData();

            List<ChapterListItemData> result = new List<ChapterListItemData>(m_chapterItems.Count);
            for (int index = 0; index < m_chapterItems.Count; index++)
            {
                ChapterListItemData chapter = m_chapterItems[index];
                result.Add(new ChapterListItemData
                {
                    Id = chapter.Id,
                    Name = chapter.Name,
                    Goal = chapter.Goal,
                    Content = chapter.Content,
                    DmNote = chapter.DmNote,
                    TerrainTag = chapter.TerrainTag,
                    TerrainSubTag = chapter.TerrainSubTag,
                    AddMapHint = chapter.AddMapHint,
                    CreatureInfo = chapter.CreatureInfo,
                    MapImagePath = chapter.MapImagePath,
                    MapGridState = new ChapterMapGridStateData
                    {
                        MapZoomScale = chapter.MapGridState?.MapZoomScale ?? 1f,
                        MapPanOffset = chapter.MapGridState?.MapPanOffset ?? Vector2.zero,
                        GridZoomScale = chapter.MapGridState?.GridZoomScale ?? 1f,
                        GridPanOffset = chapter.MapGridState?.GridPanOffset ?? Vector2.zero,
                        IsLocked = chapter.MapGridState?.IsLocked ?? false,
                        LockedMapZoomReference = chapter.MapGridState?.LockedMapZoomReference ?? 1f,
                        LockedGridToMapZoomRatio = chapter.MapGridState?.LockedGridToMapZoomRatio ?? 1f,
                        LockedGridToMapPanDelta = chapter.MapGridState?.LockedGridToMapPanDelta ?? Vector2.zero,
                    },
                    GridCells = ChapterGridCellCollectionUtility.Clone(chapter.GridCells),
                    Events = ChapterEventCollectionUtility.CloneEvents(chapter.Events),
                    EventBindings = ChapterEventCollectionUtility.CloneBindings(chapter.EventBindings),
                    Creatures = ChapterCreatureDataStructureUtility.CloneCreatureDataList(chapter.Creatures),
                    CreatureInstances = ChapterCreatureDataStructureUtility.CloneCreatureInstanceDataList(chapter.CreatureInstances),
                });
            }

            return result;
        }

        private void SetupChapterDetailPanel()
        {
            m_rectMapSurface = m_imgMapSurface != null ? m_imgMapSurface.rectTransform : null!;
            m_goCreatureBrowserRoot = m_rectCreatureBrowserRoot != null ? m_rectCreatureBrowserRoot.gameObject : null!;
            m_rectCreatureCardContainer = m_gridCreatureCardContainer != null
                ? m_gridCreatureCardContainer.GetComponent<RectTransform>()
                : null!;
            m_rectCreatureCardTemplate = m_itemCreatureCardTemplate != null
                ? m_itemCreatureCardTemplate.GetComponent<RectTransform>()
                : null!;
            m_rectCreatureDetailTemplate = m_itemCreatureDetailTemplate != null
                ? m_itemCreatureDetailTemplate.GetComponent<RectTransform>()
                : null!;
            m_imgCreatureBrowserRootBackground = m_rectCreatureBrowserRoot != null
                ? m_rectCreatureBrowserRoot.GetComponent<Image>()
                : null!;
            m_imgCreatureListPanelBackground = m_rectCreatureListPanel != null
                ? m_rectCreatureListPanel.GetComponent<Image>()
                : null!;
            m_imgCreatureDetailPanelBackground = m_rectCreatureDetailPanel != null
                ? m_rectCreatureDetailPanel.GetComponent<Image>()
                : null!;
            m_saveFeedbackCanvasGroup = m_tmpSaveFeedback != null
                ? m_tmpSaveFeedback.GetComponent<CanvasGroup>()
                : null!;

            EnsureMapGridSelectionOverlay();
            CacheMapGridLines();

            m_defaultTerrainTagText = m_textTerrainTag != null ? m_textTerrainTag.text : string.Empty;
            m_defaultTerrainSubTagText = m_textTerrainSubTag != null ? m_textTerrainSubTag.text : string.Empty;
            m_defaultAddMapHintText = m_textAddMapHint != null ? m_textAddMapHint.text : string.Empty;
            m_defaultCreatureInfoText = m_textCreatureInfoBoard != null ? m_textCreatureInfoBoard.text : string.Empty;
            m_defaultMapUploadHintText = m_tmpMapUploadHint != null ? m_tmpMapUploadHint.text : string.Empty;

            EnsureStaticCreatureBrowserWidget();
            ApplyStoryAndCreatureBoardLayout();

            if (m_btnUploadMap != null)
            {
                m_btnUploadMapRect = m_btnUploadMap.GetComponent<RectTransform>();
                m_textUploadMapButton = m_btnUploadMap.GetComponentInChildren<TMP_Text>(true);
            }

            if (m_btnOpenCheckPopup != null)
            {
                m_btnOpenCheckPopupRect = m_btnOpenCheckPopup.GetComponent<RectTransform>();
            }

            if (m_btnDeleteGridEvent != null)
            {
                m_btnDeleteGridEventRect = m_btnDeleteGridEvent.GetComponent<RectTransform>();
                m_imgDeleteGridEvent = m_btnDeleteGridEvent.GetComponent<Image>();
            }

            EnsureMapZoomToggleButton();
            EnsureGridZoomToggleButton();
            EnsureSaveChapterStateButton();
            EnsureSaveFeedbackLabel();
            EnsureTerrainToolButtons();

            if (m_imgMapSurface != null)
            {
                m_imgMapSurface.type = Image.Type.Simple;
                m_imgMapSurface.preserveAspect = true;
                m_imgMapSurface.sprite = null;
            }

            ResetMapSurfaceLayout();
            ApplyMapGridLayout();
            RefreshGridSelectionHighlights();

            if (m_tmpInputStoryContent == null)
            {
                RefreshChapterMapPreview(null!);
                return;
            }

            ConfigureChapterTextInput(m_tmpInputChapterGoal, "Please enter the chapter goal", 400);
            ConfigureChapterTextInput(m_tmpInputStoryContent, "Please enter the chapter content", 0);
            ConfigureChapterTextInput(m_tmpInputDmNote, "请输入 DM 备注", 1200);

            RefreshChapterMapPreview(null!);
        }

        private void RefreshChapterDetailPanel()
        {
            ChapterListItemData selectedChapter = GetSelectedChapterData();
            bool hasSelectedChapter = selectedChapter != null;

            if (m_tmpStoryTitle != null)
            {
                m_tmpStoryTitle.gameObject.SetActive(hasSelectedChapter);
            }

            if (m_tmpStoryTitle != null)
            {
                m_tmpStoryTitle.text = hasSelectedChapter && !string.IsNullOrWhiteSpace(selectedChapter.Name)
                    ? selectedChapter.Name
                    : string.Empty;
            }

            if (m_tmpInputStoryContent != null)
            {
                m_tmpInputStoryContent.gameObject.SetActive(hasSelectedChapter);
                m_tmpInputStoryContent.interactable = hasSelectedChapter;
                m_tmpInputStoryContent.text = hasSelectedChapter ? selectedChapter.Content ?? string.Empty : string.Empty;
            }

            if (m_tmpInputChapterGoal != null)
            {
                m_tmpInputChapterGoal.gameObject.SetActive(hasSelectedChapter);
                m_tmpInputChapterGoal.interactable = hasSelectedChapter;
                m_tmpInputChapterGoal.text = hasSelectedChapter ? selectedChapter.Goal ?? string.Empty : string.Empty;
            }

            if (m_tmpInputDmNote != null)
            {
                m_tmpInputDmNote.gameObject.SetActive(hasSelectedChapter);
                m_tmpInputDmNote.interactable = hasSelectedChapter;
                m_tmpInputDmNote.text = hasSelectedChapter ? selectedChapter.DmNote ?? string.Empty : string.Empty;
            }

            UpdateGridEventActionButtons();

            if (m_textTerrainTag != null)
            {
                m_textTerrainTag.text = hasSelectedChapter && !string.IsNullOrWhiteSpace(selectedChapter.TerrainTag)
                    ? selectedChapter.TerrainTag
                    : m_defaultTerrainTagText;
            }

            if (m_textTerrainSubTag != null)
            {
                m_textTerrainSubTag.text = hasSelectedChapter && !string.IsNullOrWhiteSpace(selectedChapter.TerrainSubTag)
                    ? selectedChapter.TerrainSubTag
                    : m_defaultTerrainSubTagText;
            }

            if (m_textAddMapHint != null)
            {
                m_textAddMapHint.text = hasSelectedChapter && !string.IsNullOrWhiteSpace(selectedChapter.AddMapHint)
                    ? selectedChapter.AddMapHint
                    : m_defaultAddMapHintText;
            }

            if (m_textCreatureInfoBoard != null)
            {
                m_textCreatureInfoBoard.gameObject.SetActive(m_rectCreatureBrowserRoot == null);
                m_textCreatureInfoBoard.text = hasSelectedChapter && !string.IsNullOrWhiteSpace(selectedChapter.CreatureInfo)
                    ? selectedChapter.CreatureInfo
                    : m_defaultCreatureInfoText;
            }

            m_creatureRuntimeCards.Clear();
            if (hasSelectedChapter && selectedChapter.Creatures != null)
            {
                for (int index = 0; index < selectedChapter.Creatures.Count; index++)
                {
                    if (selectedChapter.Creatures[index] != null)
                    {
                        m_creatureRuntimeCards.Add(new ChapterCreatureStaticCardData(selectedChapter.Creatures[index]));
                    }
                }
            }

            RefreshCreatureBrowserPreview(null);

            ApplyStoryAndCreatureBoardLayout();

            ApplyChapterMapGridState(selectedChapter);
            RefreshChapterMapPreview(selectedChapter);
        }

        private void EnsureStaticCreatureBrowserWidget()
        {
            if (m_rectCreatureBrowserRoot != null)
            {
                if (m_goCreatureBrowserRoot == null)
                {
                    BindCreatureBrowserNodes();
                    InitializeCreatureBrowserView();
                }

                RefreshCreatureBrowserPreview(null);
                if (m_textCreatureInfoBoard != null)
                {
                    m_textCreatureInfoBoard.gameObject.SetActive(false);
                }
                return;
            }

            if (m_textCreatureInfoBoard == null)
            {
                return;
            }
        }

        private void BindCreatureBrowserNodes()
        {
            m_goCreatureBrowserRoot = m_rectCreatureBrowserRoot != null ? m_rectCreatureBrowserRoot.gameObject : null!;
        }

        private void InitializeCreatureBrowserView()
        {
            if (m_rectCreatureCardTemplate != null)
            {
                m_rectCreatureCardTemplate.gameObject.SetActive(false);
            }

            if (m_rectCreatureDetailTemplate != null)
            {
                m_rectCreatureDetailTemplate.gameObject.SetActive(false);
            }

            if (m_imgCreatureBrowserRootBackground != null)
            {
                m_imgCreatureBrowserRootBackground.raycastTarget = false;
            }

            if (m_imgCreatureListPanelBackground != null)
            {
                m_imgCreatureListPanelBackground.raycastTarget = false;
            }

            if (m_imgCreatureDetailPanelBackground != null)
            {
                m_imgCreatureDetailPanelBackground.raycastTarget = false;
            }
        }

        private void ApplyStoryAndCreatureBoardLayout()
        {
            RectTransform storyContentRect = m_tmpInputStoryContent != null ? m_tmpInputStoryContent.GetComponent<RectTransform>() : null;

            if (storyContentRect != null)
            {
                if (m_tmpInputStoryContent.placeholder is Graphic placeholderGraphic)
                {
                    placeholderGraphic.raycastTarget = false;
                }
            }
        }

        private void RefreshCreatureBrowserPreview(IReadOnlyList<ChapterCreatureStaticCardData> cards)
        {
            if (m_rectCreatureBrowserRoot == null)
            {
                return;
            }

            ChapterCreatureStaticCardData? selectedCard = GetSelectedCreatureCard();

            m_creatureSeedCards.Clear();
            if (cards != null)
            {
                for (int index = 0; index < cards.Count; index++)
                {
                    m_creatureSeedCards.Add(cards[index]);
                }
            }

            RebuildCreatureBrowserCards();
            RestoreSelectedCreatureCard(selectedCard);
            RedrawCreatureBrowserPreview();
        }

        private void RedrawCreatureBrowserPreview()
        {
            if (m_rectCreatureBrowserRoot == null)
            {
                return;
            }

            if (m_tmpCreatureBrowserTitle != null)
            {
                m_tmpCreatureBrowserTitle.text = "怪物卡片";
            }

            if (m_textCreatureBrowserSubtitle != null)
            {
                m_textCreatureBrowserSubtitle.text = string.IsNullOrWhiteSpace(m_creatureSearchKeyword)
                    ? "Search by name or add a new creature card."
                    : $"名称筛选: {m_creatureSearchKeyword}";
            }

            RefreshCreatureFilteredCardIndices();
            EnsureSelectedCreatureCardVisible();

            if (m_rectCreatureCardContainer != null && m_rectCreatureCardTemplate != null)
            {
                EnsureCreatureCardViews(m_creatureFilteredCardIndices.Count);
                for (int index = 0; index < m_creatureFilteredCardIndices.Count; index++)
                {
                    int actualCardIndex = m_creatureFilteredCardIndices[index];
                    m_creatureCardWidgets[index].SetVisible(true);
                    int runtimeCreatureIndex = GetRuntimeCreatureIndex(actualCardIndex);
                    m_creatureCardWidgets[index].Bind(
                        m_creatureAllCards[actualCardIndex],
                        actualCardIndex == m_selectedCreatureCardIndex,
                        () => SelectCreatureCard(actualCardIndex),
                        runtimeCreatureIndex >= 0 ? () => OpenCreatureEntryPopup(runtimeCreatureIndex) : null,
                        screenPoint => BeginCreatureDeploymentDrag(actualCardIndex, screenPoint),
                        UpdateCreatureDeploymentDrag,
                        EndCreatureDeploymentDrag);
                }
            }

            EnsureCreatureDetailWidget();
            RefreshCreatureDetailPanel();
        }

        private void OnCreatureSearchValueChanged(string keyword)
        {
            if (m_ignoreCreatureSearchValueChanged)
            {
                return;
            }

            m_creatureSearchKeyword = keyword?.Trim() ?? string.Empty;
            RedrawCreatureBrowserPreview();
        }

        private void OpenCreatureEntryPopup()
        {
            OpenCreatureEntryPopup(-1);
        }

        private void OpenCreatureEntryPopup(int runtimeCreatureIndex)
        {
            bool isEditMode = runtimeCreatureIndex >= 0 && runtimeCreatureIndex < m_creatureRuntimeCards.Count;
            ChapterCreatureData initialData = isEditMode ? CloneCreatureData(m_creatureRuntimeCards[runtimeCreatureIndex].Source) : null;

            Log.Info(isEditMode ? "Open creature edit popup." : "Open creature entry popup.");
            GameModule.UI.ShowUIAsync<ChapterCreatureEntryPopupUI>(new ChapterCreatureEntryPopupRequest
            {
                InitialData = initialData,
                OnConfirm = isEditMode
                    ? updatedData => UpdateRuntimeCreature(runtimeCreatureIndex, updatedData)
                    : AppendRuntimeCreature,
                OnDelete = isEditMode
                    ? () => DeleteRuntimeCreature(runtimeCreatureIndex)
                    : null,
            });
        }

        private void OpenChapterEventPopup()
        {
            if (m_isDraggingChapter)
            {
                return;
            }

            ChapterListItemData selectedChapter = GetSelectedChapterData();
            if (selectedChapter == null)
            {
                return;
            }

            if (!TryGetSelectedGridCoordinates(selectedChapter, out List<ChapterGridCoordinate> coordinates))
            {
                ShowSaveFeedbackAsync("Select at least one grid cell before adding an event.", false).Forget();
                return;
            }

            ChapterGridEventData existingEventData = null;
            if (coordinates.Count == 1)
            {
                ChapterEventCollectionUtility.TryGetEventData(selectedChapter.Events, selectedChapter.EventBindings, coordinates[0], out existingEventData);
            }

            OpenChapterEventPopup(selectedChapter, coordinates, existingEventData);
        }

        private void OpenChapterEventPopup(ChapterListItemData chapter, List<ChapterGridCoordinate> coordinates, ChapterGridEventData existingEventData)
        {
            if (chapter == null || coordinates == null || coordinates.Count <= 0)
            {
                return;
            }

            GameModule.UI.ShowUIAsync<ChapterEventPopupUI>(new ChapterEventPopupRequest
            {
                ChapterId = chapter.Id,
                ChapterName = chapter.Name ?? string.Empty,
                GridCoordinate = coordinates[0],
                GridCoordinates = new List<ChapterGridCoordinate>(coordinates),
                CreatureInstances = ChapterCreatureDataStructureUtility.CloneCreatureInstanceDataList(chapter.CreatureInstances),
                ExistingEventData = ChapterGridCellCollectionUtility.CloneEventData(existingEventData),
                OnConfirm = eventData => OnChapterGridEventConfirmed(chapter.Id, coordinates, eventData),
            });
        }

        private void OnClickDeleteGridEvent()
        {
            if (m_isDraggingChapter)
            {
                return;
            }

            ChapterListItemData selectedChapter = GetSelectedChapterData();
            if (selectedChapter == null)
            {
                return;
            }

            if (!TryGetSelectedGridCoordinates(selectedChapter, out List<ChapterGridCoordinate> coordinates))
            {
                ShowSaveFeedbackAsync("Select at least one grid cell before deleting an event.", false).Forget();
                return;
            }

            OnChapterGridEventDeleted(selectedChapter.Id, coordinates);
        }

        private void OnChapterGridEventConfirmed(int chapterId, List<ChapterGridCoordinate> coordinates, ChapterGridEventData eventData)
        {
            if (eventData == null || coordinates == null || coordinates.Count <= 0)
            {
                return;
            }

            int chapterIndex = GetChapterIndexById(chapterId);
            if (chapterIndex < 0 || chapterIndex >= m_chapterItems.Count)
            {
                return;
            }

            ChapterListItemData chapter = m_chapterItems[chapterIndex];
            chapter.GridCells ??= new List<ChapterGridCellData>();
            chapter.Events ??= new List<ChapterGridEventData>();
            chapter.EventBindings ??= new List<ChapterEventBindingData>();

            bool updatedExistingSingleEvent = false;
            if (coordinates.Count == 1
                && ChapterEventCollectionUtility.TryGetEventData(chapter.Events, chapter.EventBindings, coordinates[0], out ChapterGridEventData existingEventData)
                && !string.IsNullOrWhiteSpace(existingEventData?.EventId)
                && string.Equals(existingEventData.EventId, eventData.EventId, StringComparison.Ordinal))
            {
                ChapterEventCollectionUtility.UpsertEventDefinition(chapter.Events, eventData);
                updatedExistingSingleEvent = true;
            }

            if (!updatedExistingSingleEvent)
            {
                ChapterEventCollectionUtility.AssignEventToCoordinates(chapter.Events, chapter.EventBindings, coordinates, eventData);
            }

            ChapterGridCellCollectionUtility.ClearSelectedMarks(chapter.GridCells, coordinates);

            if (chapterId == m_selectedChapterId)
            {
                RefreshGridSelectionHighlights();
            }

            SaveChapterEditorState();
            ShowSaveFeedbackAsync(coordinates.Count > 1 ? $"Recorded events for {coordinates.Count} grid cells." : "Event recorded.", true).Forget();
        }

        private void OnChapterGridEventDeleted(int chapterId, List<ChapterGridCoordinate> coordinates)
        {
            if (coordinates == null || coordinates.Count <= 0)
            {
                return;
            }

            int chapterIndex = GetChapterIndexById(chapterId);
            if (chapterIndex < 0 || chapterIndex >= m_chapterItems.Count)
            {
                return;
            }

            ChapterListItemData chapter = m_chapterItems[chapterIndex];
            chapter.GridCells ??= new List<ChapterGridCellData>();
            chapter.Events ??= new List<ChapterGridEventData>();
            chapter.EventBindings ??= new List<ChapterEventBindingData>();

            int removedCount = ChapterEventCollectionUtility.RemoveEventsAtCoordinates(chapter.Events, chapter.EventBindings, coordinates);

            if (removedCount <= 0)
            {
                ShowSaveFeedbackAsync("No event was found on the selected grid cells.", false).Forget();
                return;
            }

            ChapterGridCellCollectionUtility.ClearSelectedMarks(chapter.GridCells, coordinates);

            if (chapterId == m_selectedChapterId)
            {
                RefreshGridSelectionHighlights();
            }

            SaveChapterEditorState();
            ShowSaveFeedbackAsync(removedCount > 1 ? $"Removed events from {removedCount} grid cells." : "Event removed.", true).Forget();
        }

        private static bool HasGridEventAtAnyCoordinate(List<ChapterEventBindingData> eventBindings, List<ChapterGridCoordinate> coordinates)
        {
            if (eventBindings == null || coordinates == null)
            {
                return false;
            }

            for (int index = 0; index < coordinates.Count; index++)
            {
                if (ChapterEventCollectionUtility.HasEventAtCoordinate(eventBindings, coordinates[index]))
                {
                    return true;
                }
            }

            return false;
        }

        private void AppendRuntimeCreature(ChapterCreatureData creatureData)
        {
            if (creatureData == null)
            {
                return;
            }

            creatureData = ChapterCreatureDataStructureUtility.CloneCreatureData(creatureData);
            m_creatureRuntimeCards.Add(new ChapterCreatureStaticCardData(creatureData));
            RebuildCreatureBrowserCards();
            SetCreatureSearchKeyword(string.Empty);
            m_selectedCreatureCardIndex = m_creatureAllCards.Count - 1;
            RedrawCreatureBrowserPreview();

            bool saved = SaveChapterEditorState();
            ShowSaveFeedbackAsync(saved ? "Creature saved." : "Failed to save creature.", saved).Forget();
        }

        private void UpdateRuntimeCreature(int runtimeCreatureIndex, ChapterCreatureData creatureData)
        {
            if (creatureData == null || runtimeCreatureIndex < 0 || runtimeCreatureIndex >= m_creatureRuntimeCards.Count)
            {
                return;
            }

            string existingCreatureId = m_creatureRuntimeCards[runtimeCreatureIndex].Source?.CreatureId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(creatureData.CreatureId))
            {
                creatureData.CreatureId = existingCreatureId;
            }

            creatureData = ChapterCreatureDataStructureUtility.CloneCreatureData(creatureData);
            m_creatureRuntimeCards[runtimeCreatureIndex] = new ChapterCreatureStaticCardData(creatureData);
            RebuildCreatureBrowserCards();
            m_selectedCreatureCardIndex = m_creatureSeedCards.Count + runtimeCreatureIndex;
            RedrawCreatureBrowserPreview();

            bool saved = SaveChapterEditorState();
            ShowSaveFeedbackAsync(saved ? "Creature updated." : "Failed to update creature.", saved).Forget();
        }

        private void DeleteRuntimeCreature(int runtimeCreatureIndex)
        {
            if (runtimeCreatureIndex < 0 || runtimeCreatureIndex >= m_creatureRuntimeCards.Count)
            {
                return;
            }

            m_creatureRuntimeCards.RemoveAt(runtimeCreatureIndex);
            RebuildCreatureBrowserCards();

            int nextSelectedIndex = m_creatureSeedCards.Count + runtimeCreatureIndex;
            if (m_creatureAllCards.Count <= 0)
            {
                m_selectedCreatureCardIndex = -1;
            }
            else
            {
                m_selectedCreatureCardIndex = Mathf.Clamp(nextSelectedIndex, 0, m_creatureAllCards.Count - 1);
            }

            RedrawCreatureBrowserPreview();

            bool saved = SaveChapterEditorState();
            ShowSaveFeedbackAsync(saved ? "Creature deleted." : "Failed to delete creature.", saved).Forget();
        }

        private void RebuildCreatureBrowserCards()
        {
            m_creatureAllCards.Clear();
            m_creatureAllCards.AddRange(m_creatureSeedCards);
            m_creatureAllCards.AddRange(m_creatureRuntimeCards);
        }

        private void RefreshCreatureFilteredCardIndices()
        {
            m_creatureFilteredCardIndices.Clear();

            bool hasKeyword = !string.IsNullOrWhiteSpace(m_creatureSearchKeyword);
            for (int index = 0; index < m_creatureAllCards.Count; index++)
            {
                if (!hasKeyword || ContainsCreatureKeyword(m_creatureAllCards[index].Name, m_creatureSearchKeyword))
                {
                    m_creatureFilteredCardIndices.Add(index);
                }
            }
        }

        private void EnsureSelectedCreatureCardVisible()
        {
            if (m_creatureFilteredCardIndices.Count <= 0)
            {
                m_selectedCreatureCardIndex = -1;
                return;
            }

            if (m_selectedCreatureCardIndex < 0 || m_selectedCreatureCardIndex >= m_creatureAllCards.Count || !m_creatureFilteredCardIndices.Contains(m_selectedCreatureCardIndex))
            {
                m_selectedCreatureCardIndex = -1;
            }
        }

        private void SelectCreatureCard(int cardIndex)
        {
            if (cardIndex < 0 || cardIndex >= m_creatureAllCards.Count)
            {
                return;
            }

            if (m_selectedCreatureCardIndex == cardIndex)
            {
                RefreshCreatureDetailPanel();
                return;
            }

            m_selectedCreatureCardIndex = cardIndex;

            for (int index = 0; index < m_creatureCardWidgets.Count && index < m_creatureFilteredCardIndices.Count; index++)
            {
                int actualCardIndex = m_creatureFilteredCardIndices[index];
                int runtimeCreatureIndex = GetRuntimeCreatureIndex(actualCardIndex);
                m_creatureCardWidgets[index].Bind(
                    m_creatureAllCards[actualCardIndex],
                    actualCardIndex == m_selectedCreatureCardIndex,
                    () => SelectCreatureCard(actualCardIndex),
                    runtimeCreatureIndex >= 0 ? () => OpenCreatureEntryPopup(runtimeCreatureIndex) : null,
                    screenPoint => BeginCreatureDeploymentDrag(actualCardIndex, screenPoint),
                    UpdateCreatureDeploymentDrag,
                    EndCreatureDeploymentDrag);
            }

            RefreshCreatureDetailPanel();
        }

        private void EnsureCreatureDetailWidget()
        {
            if (m_creatureDetailWidget != null || m_rectCreatureDetailTemplate == null)
            {
                return;
            }

            GameObject detailObject = Object.Instantiate(m_rectCreatureDetailTemplate.gameObject, m_rectCreatureDetailTemplate.parent, false);
            detailObject.name = m_rectCreatureDetailTemplate.gameObject.name;
            detailObject.SetActive(false);
            m_creatureDetailWidget = new ChapterCreatureDetailWidget(detailObject);
        }

        private void RefreshCreatureDetailPanel()
        {
            if (m_creatureDetailWidget == null)
            {
                return;
            }

            bool hasSelectedCard = m_selectedCreatureCardIndex >= 0 && m_selectedCreatureCardIndex < m_creatureAllCards.Count;
            m_creatureDetailWidget.SetVisible(hasSelectedCard);
            if (!hasSelectedCard)
            {
                CleanupCreatureDetailPreviewResources();
                return;
            }

            ChapterCreatureStaticCardData creature = m_creatureAllCards[m_selectedCreatureCardIndex];
            m_creatureDetailWidget.Bind(creature);
            RefreshCreatureDetailPreview(creature);
        }

        private void EnsureCreatureCardViews(int count)
        {
            if (m_rectCreatureCardContainer == null || m_rectCreatureCardTemplate == null)
            {
                return;
            }

            while (m_creatureCardWidgets.Count < count)
            {
                GameObject cardObject = Object.Instantiate(m_rectCreatureCardTemplate.gameObject, m_rectCreatureCardContainer, false);
                cardObject.name = m_rectCreatureCardTemplate.gameObject.name;
                cardObject.SetActive(true);
                m_creatureCardWidgets.Add(new ChapterCreatureCardWidget(cardObject));
            }

            for (int index = count; index < m_creatureCardWidgets.Count; index++)
            {
                m_creatureCardWidgets[index].SetVisible(false);
            }
        }

        private void DisposeCreatureBrowserViews()
        {
            for (int index = 0; index < m_creatureCardWidgets.Count; index++)
            {
                m_creatureCardWidgets[index].Dispose();
            }

            m_creatureCardWidgets.Clear();

            if (m_creatureDetailWidget != null)
            {
                m_creatureDetailWidget.Dispose();
                m_creatureDetailWidget = null;
            }
        }

        private ChapterCreatureStaticCardData? GetSelectedCreatureCard()
        {
            if (m_selectedCreatureCardIndex < 0 || m_selectedCreatureCardIndex >= m_creatureAllCards.Count)
            {
                return null;
            }

            return m_creatureAllCards[m_selectedCreatureCardIndex];
        }

        private void RestoreSelectedCreatureCard(ChapterCreatureStaticCardData? selectedCard)
        {
            if (!selectedCard.HasValue)
            {
                m_selectedCreatureCardIndex = -1;
                return;
            }

            int selectedIndex = m_creatureAllCards.FindIndex(card => card.Equals(selectedCard.Value));
            m_selectedCreatureCardIndex = selectedIndex >= 0 ? selectedIndex : -1;
        }

        private int GetRuntimeCreatureIndex(int actualCardIndex)
        {
            int runtimeCreatureIndex = actualCardIndex - m_creatureSeedCards.Count;
            return runtimeCreatureIndex >= 0 && runtimeCreatureIndex < m_creatureRuntimeCards.Count
                ? runtimeCreatureIndex
                : -1;
        }

        private static ChapterCreatureData CloneCreatureData(ChapterCreatureData source)
        {
            return ChapterCreatureDataStructureUtility.CloneCreatureData(source);
        }

        private void RefreshCreatureDetailPreview(ChapterCreatureStaticCardData creature)
        {
            if (m_creatureDetailWidget == null)
            {
                return;
            }

            m_creatureDetailWidget.ResetPreview(creature);

            string previewPath = ChapterEditorPersistenceService.ResolveCreaturePreviewPath(creature.PreviewImageFileName);
            if (string.IsNullOrWhiteSpace(previewPath))
            {
                CleanupCreatureDetailPreviewResources();
                return;
            }

            if (string.Equals(m_loadedCreaturePreviewPath, previewPath, StringComparison.OrdinalIgnoreCase) && m_creatureDetailPreviewSprite != null)
            {
                m_creatureDetailWidget.SetPreviewSprite(m_creatureDetailPreviewSprite);
                return;
            }

            CleanupCreatureDetailPreviewResources();
            int loadVersion = ++m_creatureDetailPreviewLoadVersion;
            LoadCreatureDetailPreviewAsync(previewPath, loadVersion).Forget();
        }

        private async UniTaskVoid LoadCreatureDetailPreviewAsync(string previewPath, int loadVersion)
        {
            byte[] imageBytes;
            try
            {
                imageBytes = await UniTask.RunOnThreadPool(() => File.ReadAllBytes(previewPath));
            }
            catch (Exception exception)
            {
                Log.Warning($"读取生物详情预览图失败: {exception.Message}");
                return;
            }

            if (loadVersion != m_creatureDetailPreviewLoadVersion || m_creatureDetailWidget == null)
            {
                return;
            }

            Texture2D previewTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!previewTexture.LoadImage(imageBytes))
            {
                Object.Destroy(previewTexture);
                Log.Warning("Creature preview loading failed because the file is not a valid image.");
                return;
            }

            Sprite previewSprite = Sprite.Create(
                previewTexture,
                new Rect(0, 0, previewTexture.width, previewTexture.height),
                new Vector2(0.5f, 0.5f));

            CleanupCreatureDetailPreviewResources();
            m_creatureDetailPreviewTexture = previewTexture;
            m_creatureDetailPreviewSprite = previewSprite;
            m_loadedCreaturePreviewPath = previewPath;
            m_creatureDetailWidget.SetPreviewSprite(previewSprite);
        }

        private void CleanupCreatureDetailPreviewResources()
        {
            m_loadedCreaturePreviewPath = string.Empty;

            if (m_creatureDetailPreviewSprite != null)
            {
                Object.Destroy(m_creatureDetailPreviewSprite);
                m_creatureDetailPreviewSprite = null!;
            }

            if (m_creatureDetailPreviewTexture != null)
            {
                Object.Destroy(m_creatureDetailPreviewTexture);
                m_creatureDetailPreviewTexture = null!;
            }
        }

        private void SetCreatureSearchKeyword(string keyword)
        {
            m_creatureSearchKeyword = keyword ?? string.Empty;

            if (m_tmpInputCreatureSearch == null)
            {
                return;
            }

            m_ignoreCreatureSearchValueChanged = true;
            m_tmpInputCreatureSearch.text = m_creatureSearchKeyword;
            m_ignoreCreatureSearchValueChanged = false;
        }

        private static bool ContainsCreatureKeyword(string source, string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return true;
            }

            return !string.IsNullOrEmpty(source) && source.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void SyncSelectedChapterDetailToData()
        {
            if (m_tmpInputStoryContent == null)
            {
                return;
            }

            ChapterListItemData selectedChapter = GetSelectedChapterData();
            if (selectedChapter == null)
            {
                return;
            }

            selectedChapter.Goal = m_tmpInputChapterGoal != null ? m_tmpInputChapterGoal.text ?? string.Empty : string.Empty;
            selectedChapter.Content = m_tmpInputStoryContent.text ?? string.Empty;
            selectedChapter.DmNote = m_tmpInputDmNote != null ? m_tmpInputDmNote.text ?? string.Empty : string.Empty;

            selectedChapter.Creatures.Clear();
            for (int index = 0; index < m_creatureRuntimeCards.Count; index++)
            {
                ChapterCreatureData source = m_creatureRuntimeCards[index].Source;
                if (source != null)
                {
                    selectedChapter.Creatures.Add(source);
                }
            }

            SyncChapterMapGridStateToData(selectedChapter);
        }

        private static void ConfigureChapterTextInput(TMP_InputField inputField, string placeholderText, int characterLimit)
        {
            if (inputField == null)
            {
                return;
            }

            inputField.lineType = TMP_InputField.LineType.MultiLineNewline;
            inputField.contentType = TMP_InputField.ContentType.Standard;
            inputField.characterLimit = characterLimit;
            inputField.text = string.Empty;

            if (inputField.textComponent != null)
            {
                inputField.textComponent.alignment = TextAlignmentOptions.TopLeft;
                inputField.textComponent.enableWordWrapping = true;
                inputField.textComponent.overflowMode = TextOverflowModes.Overflow;
                inputField.textComponent.fontSize = 16;
                inputField.textComponent.lineSpacing = 1.08f;
            }

            if (inputField.placeholder is TMP_Text placeholder)
            {
                placeholder.text = placeholderText;
                placeholder.alignment = TextAlignmentOptions.TopLeft;
                placeholder.fontStyle = FontStyles.Italic;
                placeholder.enableWordWrapping = true;
                placeholder.overflowMode = TextOverflowModes.Overflow;
            }
        }

        private void OnClickUploadMap()
        {
            if (m_isDraggingChapter)
            {
                return;
            }

            ChapterListItemData selectedChapter = GetSelectedChapterData();
            if (selectedChapter == null)
            {
                return;
            }

            UploadChapterMapAsync(selectedChapter).Forget();
        }

        private void OnClickMapZoomToggle()
        {
            if (m_mapPreviewTexture == null)
            {
                return;
            }

            m_isMapZoomEnabled = !m_isMapZoomEnabled;
            if (!m_isMapZoomEnabled)
            {
                m_isDraggingMap = false;
            }

            UpdateAutoMapGridLockState(true, true);
            UpdateMapZoomToggleVisual(true);
        }

        private void OnClickGridZoomToggle()
        {
            if (m_rectMapGridOverlay == null || !m_rectMapGridOverlay.gameObject.activeInHierarchy)
            {
                return;
            }

            m_isGridZoomEnabled = !m_isGridZoomEnabled;

            UpdateAutoMapGridLockState(true, true);
            UpdateGridZoomToggleVisual(true);
        }

        private void OnClickSaveChapterState()
        {
            if (GetSelectedChapterData() == null)
            {
                return;
            }

            bool saved = SaveChapterEditorState();
            ShowSaveFeedbackAsync(saved ? "Saved." : "Save failed.", saved).Forget();
        }

        private void OnClickPreviewModule()
        {
            if (m_chapterItems.Count <= 0)
            {
                return;
            }

            bool saved = SaveChapterEditorState();
            if (!saved)
            {
                ShowSaveFeedbackAsync("Save failed.", false).Forget();
                return;
            }

            ModulePreviewSessionData previewSession = BuildModulePreviewSession();
            if (previewSession == null || previewSession.Chapters.Count <= 0)
            {
                ShowSaveFeedbackAsync("Preview data is empty.", false).Forget();
                return;
            }

            GameModule.UI.CloseUI<ChapterEditorUI>();
            GameModule.UI.ShowUIAsync<ModulePreviewRuntimeUI>(previewSession);
        }

        private ModulePreviewSessionData BuildModulePreviewSession()
        {
            return ModulePreviewSessionBuilder.Build(
                CollectChapterListData(),
                m_selectedChapterId,
                GetSelectedChapterIndex(),
                m_rectMapGridOverlay != null ? m_rectMapGridOverlay.rect.size : Vector2.zero);
        }

        private void OnClickDifficultTerrain()
        {
            if (m_isDraggingChapter)
            {
                return;
            }

            ChapterListItemData selectedChapter = GetSelectedChapterData();
            if (selectedChapter == null)
            {
                return;
            }

            selectedChapter.GridCells ??= new List<ChapterGridCellData>();
            int markedCellCount = ChapterGridCellCollectionUtility.ApplyMarkTypeToCellsBySourceMark(
                selectedChapter.GridCells,
                ChapterGridCellMarkType.Selected,
                ChapterGridCellMarkType.DifficultTerrain);
            if (markedCellCount <= 0)
            {
                return;
            }

            RefreshGridSelectionHighlights();
            SaveChapterEditorState();
        }

        private void OnClickImpassableTerrain()
        {
            if (m_isDraggingChapter)
            {
                return;
            }

            ChapterListItemData selectedChapter = GetSelectedChapterData();
            if (selectedChapter == null)
            {
                return;
            }

            selectedChapter.GridCells ??= new List<ChapterGridCellData>();
            int markedCellCount = ChapterGridCellCollectionUtility.ApplyMarkTypeToCellsBySourceMark(
                selectedChapter.GridCells,
                ChapterGridCellMarkType.Selected,
                ChapterGridCellMarkType.ImpassableTerrain);
            if (markedCellCount <= 0)
            {
                return;
            }

            RefreshGridSelectionHighlights();
            SaveChapterEditorState();
        }

        private void OnClickClearTerrain()
        {
            if (m_isDraggingChapter)
            {
                return;
            }

            ChapterListItemData selectedChapter = GetSelectedChapterData();
            if (selectedChapter == null)
            {
                return;
            }

            selectedChapter.GridCells ??= new List<ChapterGridCellData>();
            int removedCount = ChapterGridCellCollectionUtility.ClearMarksAtSelectedCoordinates(selectedChapter.GridCells);
            if (removedCount <= 0)
            {
                return;
            }

            RefreshGridSelectionHighlights();
            SaveChapterEditorState();
        }

        private async UniTaskVoid UploadChapterMapAsync(ChapterListItemData chapter)
        {
            string sourceFilePath = RuntimeImageFileDialog.OpenImageFile("选择章节地图");
            if (string.IsNullOrEmpty(sourceFilePath))
            {
                return;
            }

            if (!File.Exists(sourceFilePath))
            {
                Log.Error($"章节地图文件不存在: {sourceFilePath}");
                return;
            }

            string oldPath = chapter.MapImagePath;
            string extension = Path.GetExtension(sourceFilePath);
            string targetDirectory = GetChapterMapStorageDirectory();
            string targetFileName = $"chapter_{chapter.Id}_{DateTime.Now:yyyyMMddHHmmssfff}{extension}";
            string targetFilePath = Path.Combine(targetDirectory, targetFileName);

            try
            {
                await UniTask.RunOnThreadPool(() =>
                {
                    Directory.CreateDirectory(targetDirectory);
                    File.Copy(sourceFilePath, targetFilePath, true);
                });
            }
            catch (Exception exception)
            {
                Log.Error($"复制章节地图失败: {exception.Message}");
                return;
            }

            chapter.MapImagePath = targetFilePath;
            if (!string.IsNullOrEmpty(oldPath) && !string.Equals(oldPath, targetFilePath, StringComparison.OrdinalIgnoreCase))
            {
                TryDeleteManagedMapFile(oldPath);
            }

            RefreshChapterMapPreview(chapter, forceReload : true);
            SaveChapterEditorState();
        }

        private void RefreshChapterMapPreview(ChapterListItemData chapter, bool forceReload = false)
        {
            bool hasSelectedChapter = chapter != null;
            bool hasMap = hasSelectedChapter && !string.IsNullOrWhiteSpace(chapter.MapImagePath) && File.Exists(chapter.MapImagePath);

            if (m_rectMapPreview != null)
            {
                m_rectMapPreview.gameObject.SetActive(hasSelectedChapter);
            }

            if (m_rectMapGridOverlay != null)
            {
                m_rectMapGridOverlay.gameObject.SetActive(hasSelectedChapter);
            }

            if (m_btnUploadMap != null)
            {
                m_btnUploadMap.gameObject.SetActive(hasSelectedChapter);
                m_btnUploadMap.interactable = hasSelectedChapter;
            }

            if (m_btnMapZoomToggle != null)
            {
                m_btnMapZoomToggle.gameObject.SetActive(hasSelectedChapter);
                m_btnMapZoomToggle.interactable = hasMap;
            }

            if (m_btnGridZoomToggle != null)
            {
                m_btnGridZoomToggle.gameObject.SetActive(hasSelectedChapter);
                m_btnGridZoomToggle.interactable = hasSelectedChapter;
            }

            if (m_btnSaveChapterState != null)
            {
                m_btnSaveChapterState.gameObject.SetActive(hasSelectedChapter);
                m_btnSaveChapterState.interactable = hasSelectedChapter;
            }

            if (m_btnPreviewModule != null)
            {
                bool canPreview = m_chapterItems.Count > 0;
                m_btnPreviewModule.gameObject.SetActive(hasSelectedChapter);
                m_btnPreviewModule.interactable = canPreview;
            }

            if (m_tmpMapUploadHint != null)
            {
                m_tmpMapUploadHint.gameObject.SetActive(hasSelectedChapter && !hasMap);
                m_tmpMapUploadHint.text = m_defaultMapUploadHintText;
            }

            UpdateUploadMapButtonLayout(hasMap);
            UpdateMapZoomToggleButtonLayout();
            UpdateGridZoomToggleButtonLayout();
            UpdateSaveChapterStateButtonLayout();
            UpdatePreviewModuleButtonLayout();
            UpdateMapZoomToggleVisual(hasMap);
            UpdateGridZoomToggleVisual(hasSelectedChapter);
            UpdateSaveChapterStateButtonVisual(hasSelectedChapter);
            UpdatePreviewModuleButtonVisual(m_chapterItems.Count > 0);
            UpdateTerrainToolButtons(hasSelectedChapter, hasMap, HasSelectedCellsForTerrainMarking());
            ApplyMapGridLayout();
            RefreshGridSelectionHighlights();

            if (m_textUploadMapButton != null)
            {
                m_textUploadMapButton.text = hasMap ? "重新上传地图" : "上传章节地图";
            }

            if (m_textAddMapHint != null)
            {
                m_textAddMapHint.text = hasMap
                    ? Path.GetFileName(chapter.MapImagePath)
                    : m_defaultAddMapHintText;
            }

            if (!hasSelectedChapter)
            {
                m_mapPreviewLoadVersion++;
                m_isMapPreviewLoading = false;
                ResetMapZoomState();
                ResetGridZoomState();
                ResetMapGridLockState();
                CleanupMapPreviewResources();
                ApplyEmptyMapPreviewVisual();
                RefreshGridSelectionHighlights();
                return;
            }

            if (!hasMap)
            {
                m_mapPreviewLoadVersion++;
                m_isMapPreviewLoading = false;
                ResetMapZoomState();
                CleanupMapPreviewResources();
                ApplyEmptyMapPreviewVisual();
                ResetMapGridLockState();
                RefreshGridSelectionHighlights();
                return;
            }

            if (!forceReload && string.Equals(m_loadedMapImagePath, chapter.MapImagePath, StringComparison.OrdinalIgnoreCase) && m_mapPreviewSprite != null)
            {
                m_isMapPreviewLoading = false;
                ApplyLoadedMapPreviewVisual();
                return;
            }

            m_isMapPreviewLoading = true;
            int loadVersion = ++m_mapPreviewLoadVersion;
            LoadMapPreviewAsync(chapter.MapImagePath, loadVersion).Forget();
        }

        private async UniTaskVoid LoadMapPreviewAsync(string mapImagePath, int loadVersion)
        {
            byte[] imageBytes;
            try
            {
                imageBytes = await UniTask.RunOnThreadPool(() => File.ReadAllBytes(mapImagePath));
            }
            catch (Exception exception)
            {
                if (loadVersion == m_mapPreviewLoadVersion)
                {
                    m_isMapPreviewLoading = false;
                    CleanupMapPreviewResources();
                    ApplyEmptyMapPreviewVisual();
                    Log.Error($"读取章节地图失败: {exception.Message}");
                }

                return;
            }

            if (loadVersion != m_mapPreviewLoadVersion)
            {
                return;
            }

            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(imageBytes))
            {
                Object.Destroy(texture);
                if (loadVersion == m_mapPreviewLoadVersion)
                {
                    m_isMapPreviewLoading = false;
                    CleanupMapPreviewResources();
                    ApplyEmptyMapPreviewVisual();
                    Log.Error("Chapter map loading failed because the file is not a valid image.");
                }

                return;
            }

            Sprite previewSprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));

            if (loadVersion != m_mapPreviewLoadVersion)
            {
                Object.Destroy(previewSprite);
                Object.Destroy(texture);
                return;
            }

            CleanupMapPreviewResources();

            m_mapPreviewTexture = texture;
            m_mapPreviewSprite = previewSprite;
            m_loadedMapImagePath = mapImagePath;
            m_isMapPreviewLoading = false;

            if (m_imgMapSurface != null)
            {
                m_imgMapSurface.sprite = m_mapPreviewSprite;
            }

            ApplyLoadedMapPreviewVisual();
        }

        private void ApplyLoadedMapPreviewVisual()
        {
            if (m_imgMapSurface != null)
            {
                m_imgMapSurface.type = Image.Type.Simple;
                m_imgMapSurface.preserveAspect = true;
                m_imgMapSurface.color = Color.white;
            }

            ApplyChapterMapGridState(GetSelectedChapterData());
            UpdateAutoMapGridLockState(true, false);
            UpdateSaveChapterStateButtonVisual(GetSelectedChapterData() != null);
            UpdateMapSurfaceLayoutToFit();
            ApplyMapGridLayout();

            if (m_tmpMapUploadHint != null)
            {
                m_tmpMapUploadHint.gameObject.SetActive(false);
            }
        }

        private void ApplyEmptyMapPreviewVisual()
        {
            if (m_imgMapSurface != null)
            {
                m_imgMapSurface.sprite = null;
                m_imgMapSurface.type = Image.Type.Simple;
                m_imgMapSurface.preserveAspect = true;
                m_imgMapSurface.color = new Color(0.95f, 0.93f, 0.88f, 1f);
            }

            ResetMapSurfaceLayout();
        }

        private void CleanupMapPreviewResources()
        {
            if (m_imgMapSurface != null)
            {
                m_imgMapSurface.sprite = null;
            }

            if (m_mapPreviewSprite != null)
            {
                Object.Destroy(m_mapPreviewSprite);
                m_mapPreviewSprite = null;
            }

            if (m_mapPreviewTexture != null)
            {
                Object.Destroy(m_mapPreviewTexture);
                m_mapPreviewTexture = null;
            }

            m_loadedMapImagePath = string.Empty;
        }

        private void EnsureMapZoomToggleButton()
        {
            if (m_btnMapZoomToggle == null)
            {
                return;
            }

            m_btnMapZoomToggleRect = m_btnMapZoomToggle.GetComponent<RectTransform>();
            m_imgMapZoomToggle = m_btnMapZoomToggle.targetGraphic as Image;
            m_textMapZoomToggle = m_btnMapZoomToggle.GetComponentInChildren<TMP_Text>(true);
            UpdateMapZoomToggleButtonLayout();
            UpdateMapZoomToggleVisual(m_mapPreviewTexture != null);
        }

        private void EnsureGridZoomToggleButton()
        {
            if (m_btnGridZoomToggle == null)
            {
                return;
            }

            m_btnGridZoomToggleRect = m_btnGridZoomToggle.GetComponent<RectTransform>();
            m_imgGridZoomToggle = m_btnGridZoomToggle.targetGraphic as Image;
            m_textGridZoomToggle = m_btnGridZoomToggle.GetComponentInChildren<TMP_Text>(true);
            UpdateGridZoomToggleButtonLayout();
            UpdateGridZoomToggleVisual(m_rectMapPreview != null && m_rectMapPreview.gameObject.activeInHierarchy);
        }

        private void EnsureSaveChapterStateButton()
        {
            if (m_btnSaveChapterState == null)
            {
                return;
            }

            m_btnSaveChapterStateRect = m_btnSaveChapterState.GetComponent<RectTransform>();
            m_imgSaveChapterState = m_btnSaveChapterState.targetGraphic as Image;
            m_textSaveChapterState = m_btnSaveChapterState.GetComponentInChildren<TMP_Text>(true);
            UpdateSaveChapterStateButtonLayout();
            UpdateSaveChapterStateButtonVisual(GetSelectedChapterData() != null);
            EnsurePreviewModuleButton();
        }

        private void EnsurePreviewModuleButton()
        {
            if (m_btnPreviewModule != null)
            {
                return;
            }

            Transform previewTransform = gameObject.transform.Find("m_btnPreviewModule");
            if (previewTransform == null)
            {
                return;
            }

            GameObject previewButtonObject = previewTransform.gameObject;
            m_btnPreviewModule = previewButtonObject.GetComponent<Button>();
            m_btnPreviewModuleRect = previewButtonObject.GetComponent<RectTransform>();
            m_imgPreviewModule = m_btnPreviewModule != null ? m_btnPreviewModule.targetGraphic as Image : null;
            m_textPreviewModule = previewButtonObject.GetComponentInChildren<TMP_Text>(true);

            if (m_btnPreviewModule != null)
            {
                m_btnPreviewModule.onClick.RemoveAllListeners();
                m_btnPreviewModule.onClick.AddListener(OnClickPreviewModuleBtn);
            }

            UpdatePreviewModuleButtonLayout();
            UpdatePreviewModuleButtonVisual(m_chapterItems.Count > 0);
            previewButtonObject.SetActive(GetSelectedChapterData() != null);
        }

        private void EnsureSaveFeedbackLabel()
        {
            if (m_tmpSaveFeedback == null)
            {
                return;
            }

            if (m_saveFeedbackCanvasGroup != null)
            {
                m_saveFeedbackCanvasGroup.alpha = 0f;
                m_saveFeedbackCanvasGroup.blocksRaycasts = false;
                m_saveFeedbackCanvasGroup.interactable = false;
            }

            m_tmpSaveFeedback.raycastTarget = false;
            m_tmpSaveFeedback.text = string.Empty;
        }

        private async UniTaskVoid ShowSaveFeedbackAsync(string message, bool isSuccess)
        {
            if (m_tmpSaveFeedback == null || m_saveFeedbackCanvasGroup == null)
            {
                return;
            }

            int feedbackVersion = ++m_saveFeedbackVersion;
            m_tmpSaveFeedback.text = message;
            m_tmpSaveFeedback.color = isSuccess
                ? new Color(0.79f, 0.92f, 0.81f, 1f)
                : new Color(0.95f, 0.72f, 0.72f, 1f);
            m_saveFeedbackCanvasGroup.alpha = 1f;

            await UniTask.Delay(900, DelayType.UnscaledDeltaTime);
            if (feedbackVersion != m_saveFeedbackVersion || m_saveFeedbackCanvasGroup == null)
            {
                return;
            }

            const int fadeSteps = 10;
            for (int index = 0; index < fadeSteps; index++)
            {
                if (feedbackVersion != m_saveFeedbackVersion || m_saveFeedbackCanvasGroup == null)
                {
                    return;
                }

                m_saveFeedbackCanvasGroup.alpha = 1f - (index + 1) / (float) fadeSteps;
                await UniTask.Delay(40, DelayType.UnscaledDeltaTime);
            }

            if (feedbackVersion != m_saveFeedbackVersion || m_saveFeedbackCanvasGroup == null || m_tmpSaveFeedback == null)
            {
                return;
            }

            m_saveFeedbackCanvasGroup.alpha = 0f;
            m_tmpSaveFeedback.text = string.Empty;
        }

        private void EnsureTerrainToolButtons()
        {
            EnsureTerrainToolButton(TerrainToolButtonType.DifficultTerrain);
            EnsureTerrainToolButton(TerrainToolButtonType.ImpassableTerrain);
            EnsureTerrainToolButton(TerrainToolButtonType.ClearTerrain);
        }

        private void EnsureTerrainToolButton(TerrainToolButtonType buttonType)
        {
            Button button = GetTerrainToolButton(buttonType);

            if (button == null)
            {
                return;
            }

            CacheTerrainToolButtonComponents(buttonType, button);
            UpdateTerrainToolButtonLayout(buttonType);
            UpdateTerrainToolButtonVisual(buttonType, m_mapPreviewTexture != null, HasSelectedCellsForTerrainMarking());
        }

        private void CacheTerrainToolButtonComponents(TerrainToolButtonType buttonType, Button button)
        {
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            Image image = button.targetGraphic as Image;
            TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);

            switch (buttonType)
            {
                case TerrainToolButtonType.DifficultTerrain:
                    m_btnDifficultTerrain = button;
                    m_btnDifficultTerrainRect = rectTransform;
                    m_imgDifficultTerrain = image;
                    m_textDifficultTerrain = text;
                    break;
                case TerrainToolButtonType.ImpassableTerrain:
                    m_btnImpassableTerrain = button;
                    m_btnImpassableTerrainRect = rectTransform;
                    m_imgImpassableTerrain = image;
                    m_textImpassableTerrain = text;
                    break;
                case TerrainToolButtonType.ClearTerrain:
                    m_btnClearTerrain = button;
                    m_btnClearTerrainRect = rectTransform;
                    m_imgClearTerrain = image;
                    m_textClearTerrain = text;
                    break;
            }
        }

        private void BindTerrainToolButtonAction(TerrainToolButtonType buttonType, Button button)
        {
            switch (buttonType)
            {
                case TerrainToolButtonType.DifficultTerrain:
                    button.onClick.AddListener(OnClickDifficultTerrain);
                    break;
                case TerrainToolButtonType.ImpassableTerrain:
                    button.onClick.AddListener(OnClickImpassableTerrain);
                    break;
                case TerrainToolButtonType.ClearTerrain:
                    button.onClick.AddListener(OnClickClearTerrain);
                    break;
            }
        }

        private static string GetTerrainToolButtonName(TerrainToolButtonType buttonType)
        {
            switch (buttonType)
            {
                case TerrainToolButtonType.DifficultTerrain:
                    return "m_btnDifficultTerrain";
                case TerrainToolButtonType.ImpassableTerrain:
                    return "m_btnImpassableTerrain";
                case TerrainToolButtonType.ClearTerrain:
                    return "m_btnClearTerrain";
                default:
                    return string.Empty;
            }
        }

        private void EnsureMapGridSelectionOverlay()
        {
            if (m_rectMapPreview == null || m_rectMapGridSelectionOverlay == null)
            {
                return;
            }

            m_rectMapGridSelectionOverlay.anchorMin = Vector2.zero;
            m_rectMapGridSelectionOverlay.anchorMax = Vector2.one;
            m_rectMapGridSelectionOverlay.pivot = new Vector2(0.5f, 0.5f);
            m_rectMapGridSelectionOverlay.anchoredPosition = Vector2.zero;
            m_rectMapGridSelectionOverlay.sizeDelta = new Vector2(-MapPreviewPadding, -MapPreviewPadding);
            m_rectMapGridSelectionOverlay.localScale = Vector3.one;

            if (m_rectMapGridOverlay != null)
            {
                m_rectMapGridSelectionOverlay.SetSiblingIndex(m_rectMapGridOverlay.GetSiblingIndex());
            }
        }

        private void CacheMapGridLines()
        {
            m_verticalGridLines.Clear();
            m_horizontalGridLines.Clear();
            m_verticalGridLineTemplate = null;
            m_horizontalGridLineTemplate = null;

            if (m_rectMapGridOverlay == null)
            {
                return;
            }

            for (int index = 0; index < m_rectMapGridOverlay.childCount; index++)
            {
                RectTransform child = m_rectMapGridOverlay.GetChild(index) as RectTransform;
                if (child == null)
                {
                    continue;
                }

                if (child.name.StartsWith("m_imgGridLineV", StringComparison.Ordinal))
                {
                    m_verticalGridLines.Add(child);
                    continue;
                }

                if (child.name.StartsWith("m_imgGridLineH", StringComparison.Ordinal))
                {
                    m_horizontalGridLines.Add(child);
                }
            }

            m_verticalGridLines.Sort(CompareGridLineByName);
            m_horizontalGridLines.Sort(CompareGridLineByName);
            m_verticalGridLineTemplate = m_verticalGridLines.Count > 0 ? m_verticalGridLines[0] : null;
            m_horizontalGridLineTemplate = m_horizontalGridLines.Count > 0 ? m_horizontalGridLines[0] : null;
            ApplyMapGridVisualStyle();
        }

        private void ApplyMapGridVisualStyle()
        {
            if (m_rectMapGridOverlay == null)
            {
                return;
            }

            Color lineColor = new Color(0.22f, 0.27f, 0.31f, 0.52f);
            Color frameColor = new Color(0.18f, 0.23f, 0.27f, 0.68f);

            for (int index = 0; index < m_rectMapGridOverlay.childCount; index++)
            {
                RectTransform child = m_rectMapGridOverlay.GetChild(index) as RectTransform;
                if (child == null)
                {
                    continue;
                }

                Image image = child.GetComponent<Image>();
                if (image == null)
                {
                    continue;
                }

                if (child.name.StartsWith("m_imgGridLineV", StringComparison.Ordinal))
                {
                    image.color = lineColor;
                    child.sizeDelta = new Vector2(GridLineThickness, 0f);
                    continue;
                }

                if (child.name.StartsWith("m_imgGridLineH", StringComparison.Ordinal))
                {
                    image.color = lineColor;
                    child.sizeDelta = new Vector2(0f, GridLineThickness);
                    continue;
                }

                if (child.name.Equals("m_imgGridFrameTop", StringComparison.Ordinal) || child.name.Equals("m_imgGridFrameBottom", StringComparison.Ordinal))
                {
                    image.color = frameColor;
                    child.sizeDelta = new Vector2(0f, GridFrameThickness);
                    continue;
                }

                if (child.name.Equals("m_imgGridFrameLeft", StringComparison.Ordinal) || child.name.Equals("m_imgGridFrameRight", StringComparison.Ordinal))
                {
                    image.color = frameColor;
                    child.sizeDelta = new Vector2(GridFrameThickness, 0f);
                }
            }
        }

        private void ApplyMapGridLayout()
        {
            if (m_rectMapGridOverlay == null)
            {
                return;
            }

            ChapterMapGridMetrics metrics = ChapterMapGridUtility.CreateMetrics(m_rectMapGridOverlay.rect, BaseMapGridColumns, BaseMapGridRows, m_mapGridZoomScale, m_mapGridPanOffset);
            List<float> verticalPositions = ChapterMapGridUtility.BuildCenteredGridLinePositions(metrics.OverlayWidth, metrics.CellWidth, metrics.DisplayOffsetX);
            List<float> horizontalPositions = ChapterMapGridUtility.BuildCenteredGridLinePositions(metrics.OverlayHeight, metrics.CellHeight, metrics.DisplayOffsetY);
            UpdateGridLineCollection(m_verticalGridLines, m_verticalGridLineTemplate, verticalPositions, true, metrics.OverlayWidth, metrics.OverlayHeight);
            UpdateGridLineCollection(m_horizontalGridLines, m_horizontalGridLineTemplate, horizontalPositions, false, metrics.OverlayWidth, metrics.OverlayHeight);
            RefreshGridSelectionHighlights();
        }

        private void RefreshGridSelectionHighlights()
        {
            if (m_rectMapGridSelectionOverlay == null)
            {
                HideCreatureInstanceActionPanel();
                HideCreaturePlacementPreview();
                SetGridCoordinateLabelCount(0);
                return;
            }

            UpdateGridEventActionButtons();
            UpdateTerrainToolButtonState();

            ChapterListItemData selectedChapter = GetSelectedChapterData();
            if (selectedChapter == null)
            {
                SetGridSelectionHighlightCount(0);
                SetGridEventMarkerCount(0);
                SetGridCoordinateLabelCount(0);
                HideCreatureInstanceActionPanel();
                HideCreaturePlacementPreview();
                return;
            }

            if (!TryGetCurrentGridMetrics(out ChapterMapGridMetrics metrics))
            {
                SetGridSelectionHighlightCount(0);
                SetGridEventMarkerCount(0);
                SetGridCoordinateLabelCount(0);
                HideCreatureInstanceActionPanel();
                HideCreaturePlacementPreview();
                return;
            }

            List<ChapterGridCellData> visibleGridCells = new List<ChapterGridCellData>();
            float minX = -metrics.OverlayWidth * 0.5f;
            float maxX = metrics.OverlayWidth * 0.5f;
            float minY = -metrics.OverlayHeight * 0.5f;
            float maxY = metrics.OverlayHeight * 0.5f;

            AppendVisibleGridCellsByMarkType(visibleGridCells, selectedChapter.GridCells, ChapterGridCellMarkType.DifficultTerrain, metrics, minX, maxX, minY, maxY);
            AppendVisibleGridCellsByMarkType(visibleGridCells, selectedChapter.GridCells, ChapterGridCellMarkType.ImpassableTerrain, metrics, minX, maxX, minY, maxY);
            AppendVisibleGridCellsByMarkType(visibleGridCells, selectedChapter.GridCells, ChapterGridCellMarkType.Selected, metrics, minX, maxX, minY, maxY);
            List<ChapterGridCoordinate> visibleEventCoordinates = GetVisibleEventCoordinates(selectedChapter.EventBindings, metrics, minX, maxX, minY, maxY);

            SetGridSelectionHighlightCount(visibleGridCells.Count);
            for (int index = 0; index < visibleGridCells.Count; index++)
            {
                ChapterGridCellData gridCell = visibleGridCells[index];
                Rect cellRect = ChapterMapGridUtility.GetLogicalCellRect(metrics, gridCell.Coordinate);
                float cellCenterX = cellRect.center.x;
                float cellCenterY = cellRect.center.y;
                Image highlight = m_gridSelectionHighlights[index];
                RectTransform rectTransform = highlight.rectTransform;
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = new Vector2(cellCenterX, cellCenterY);
                rectTransform.sizeDelta = new Vector2(metrics.CellWidth, metrics.CellHeight);
                rectTransform.localScale = Vector3.one;
                highlight.color = GetGridCellHighlightColor(gridCell.MarkType);
                highlight.gameObject.SetActive(true);
            }

            RefreshCreatureInstanceTokens(selectedChapter.CreatureInstances, metrics, minX, maxX, minY, maxY);
            RefreshGridCoordinateLabels(metrics, minX, maxX, minY, maxY);
            RefreshGridEventMarkers(visibleEventCoordinates, metrics, selectedChapter);
            BringGridEventMarkersToFront();
        }

        private void AppendVisibleGridCellsByMarkType(List<ChapterGridCellData> visibleGridCells, List<ChapterGridCellData> sourceGridCells, ChapterGridCellMarkType markType, ChapterMapGridMetrics metrics, float minX, float maxX, float minY, float maxY)
        {
            List<ChapterGridCellData> gridCells = ChapterGridCellCollectionUtility.GetCellsByMarkType(sourceGridCells, markType);
            for (int index = 0; index < gridCells.Count; index++)
            {
                ChapterGridCellData gridCell = gridCells[index];
                Rect cellRect = ChapterMapGridUtility.GetLogicalCellRect(metrics, gridCell.Coordinate);
                if (cellRect.xMax <= minX || cellRect.xMin >= maxX || cellRect.yMax <= minY || cellRect.yMin >= maxY)
                {
                    continue;
                }

                visibleGridCells.Add(gridCell);
            }
        }

        private List<ChapterGridCellData> GetVisibleGridCellsByMarkType(List<ChapterGridCellData> sourceGridCells, ChapterGridCellMarkType markType, ChapterMapGridMetrics metrics, float minX, float maxX, float minY, float maxY)
        {
            List<ChapterGridCellData> visibleGridCells = new List<ChapterGridCellData>();
            AppendVisibleGridCellsByMarkType(visibleGridCells, sourceGridCells, markType, metrics, minX, maxX, minY, maxY);
            return visibleGridCells;
        }

        private static Color GetGridCellHighlightColor(ChapterGridCellMarkType markType)
        {
            switch (markType)
            {
                case ChapterGridCellMarkType.DifficultTerrain:
                    return new Color(0.78f, 0.39f, 0.08f, 0.34f);
                case ChapterGridCellMarkType.ImpassableTerrain:
                    return new Color(0.45f, 0.45f, 0.45f, 0.3f);
                case ChapterGridCellMarkType.Selected:
                default:
                    return new Color(0.12f, 0.55f, 0.92f, 0.3f);
            }
        }

        private void SetGridSelectionHighlightCount(int visibleCount)
        {
            if (m_rectMapGridSelectionOverlay == null)
            {
                return;
            }

            while (m_gridSelectionHighlights.Count < visibleCount)
            {
                GameObject highlightObject = new GameObject($"GridSelectionHighlight_{m_gridSelectionHighlights.Count}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                RectTransform rectTransform = highlightObject.GetComponent<RectTransform>();
                rectTransform.SetParent(m_rectMapGridSelectionOverlay, false);
                Image image = highlightObject.GetComponent<Image>();
                image.raycastTarget = false;
                image.color = new Color(0.12f, 0.55f, 0.92f, 0.3f);
                m_gridSelectionHighlights.Add(image);
            }

            for (int index = 0; index < m_gridSelectionHighlights.Count; index++)
            {
                m_gridSelectionHighlights[index].gameObject.SetActive(index < visibleCount);
            }
        }

        private List<ChapterGridCoordinate> GetVisibleEventCoordinates(List<ChapterEventBindingData> sourceBindings, ChapterMapGridMetrics metrics, float minX, float maxX, float minY, float maxY)
        {
            List<ChapterGridCoordinate> visibleCoordinates = new List<ChapterGridCoordinate>();
            List<ChapterGridCoordinate> boundCoordinates = ChapterEventCollectionUtility.CollectBoundCoordinates(sourceBindings);
            for (int index = 0; index < boundCoordinates.Count; index++)
            {
                ChapterGridCoordinate coordinate = boundCoordinates[index];
                Rect cellRect = ChapterMapGridUtility.GetLogicalCellRect(metrics, coordinate);
                if (cellRect.xMax <= minX || cellRect.xMin >= maxX || cellRect.yMax <= minY || cellRect.yMin >= maxY)
                {
                    continue;
                }

                visibleCoordinates.Add(coordinate);
            }

            return visibleCoordinates;
        }

        private void RefreshGridEventMarkers(List<ChapterGridCoordinate> visibleEventCoordinates, ChapterMapGridMetrics metrics, ChapterListItemData selectedChapter)
        {
            SetGridEventMarkerCount(visibleEventCoordinates?.Count ?? 0);
            if (visibleEventCoordinates == null)
            {
                return;
            }

            for (int index = 0; index < visibleEventCoordinates.Count; index++)
            {
                Rect cellRect = ChapterMapGridUtility.GetLogicalCellRect(metrics, visibleEventCoordinates[index]);
                Image marker = m_gridEventMarkers[index];
                RectTransform rectTransform = marker.rectTransform;
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = new Vector2(cellRect.center.x, cellRect.center.y);
                rectTransform.sizeDelta = new Vector2(GridEventMarkerSize, GridEventMarkerSize);
                rectTransform.localScale = Vector3.one;
                ChapterEventCollectionUtility.TryGetEventData(selectedChapter?.Events, selectedChapter?.EventBindings, visibleEventCoordinates[index], out ChapterGridEventData eventData);
                marker.color = GetGridEventMarkerColor(eventData);
                marker.gameObject.SetActive(true);
            }
        }

        private static Color GetGridEventMarkerColor(ChapterGridEventData eventData)
        {
            return ChapterEventDataStructureUtility.IsPlayerInitialPositionEvent(eventData)
                ? new Color(0.18f, 0.82f, 0.58f, 0.96f)
                : new Color(0.95f, 0.65f, 0.18f, 0.96f);
        }

        private void SetGridEventMarkerCount(int visibleCount)
        {
            if (m_rectMapGridSelectionOverlay == null)
            {
                return;
            }

            while (m_gridEventMarkers.Count < visibleCount)
            {
                GameObject markerObject = new GameObject($"GridEventMarker_{m_gridEventMarkers.Count}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                markerObject.transform.SetParent(m_rectMapGridSelectionOverlay, false);
                Image image = markerObject.GetComponent<Image>();
                image.raycastTarget = false;
                m_gridEventMarkers.Add(image);
            }

            for (int index = 0; index < m_gridEventMarkers.Count; index++)
            {
                m_gridEventMarkers[index].gameObject.SetActive(index < visibleCount);
            }
        }

        private void RefreshGridCoordinateLabels(ChapterMapGridMetrics metrics, float minX, float maxX, float minY, float maxY)
        {
            if (m_rectMapGridSelectionOverlay == null)
            {
                SetGridCoordinateLabelCount(0);
                return;
            }

            List<ChapterGridCoordinate> visibleCoordinates = BuildVisibleGridCoordinates(metrics, minX, maxX, minY, maxY);
            SetGridCoordinateLabelCount(visibleCoordinates.Count);
            for (int index = 0; index < visibleCoordinates.Count; index++)
            {
                ChapterGridCoordinate coordinate = visibleCoordinates[index];
                Rect cellRect = ChapterMapGridUtility.GetLogicalCellRect(metrics, coordinate);
                TMP_Text label = m_gridCoordinateLabels[index];
                RectTransform rectTransform = label.rectTransform;
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(1f, 0f);
                rectTransform.anchoredPosition = new Vector2(cellRect.xMax - GridCoordinateLabelInset, cellRect.yMin + GridCoordinateLabelInset);
                rectTransform.sizeDelta = new Vector2(Mathf.Max(36f, cellRect.width - GridCoordinateLabelInset * 2f), GridCoordinateLabelHeight);
                rectTransform.localScale = Vector3.one;
                label.fontSize = Mathf.Clamp(
                    Mathf.Min(metrics.CellWidth, metrics.CellHeight) * GridCoordinateLabelFontScale,
                    GridCoordinateLabelMinFontSize,
                    GridCoordinateLabelMaxFontSize);
                label.color = GetEditorGridCoordinateLabelColor(new Vector2(cellRect.xMax - GridCoordinateLabelInset, cellRect.yMin + GridCoordinateLabelInset));
                label.text = FormatGridCoordinateLabel(coordinate);
                label.gameObject.SetActive(true);
            }
        }

        private void SetGridCoordinateLabelCount(int visibleCount)
        {
            if (m_rectMapGridSelectionOverlay == null)
            {
                return;
            }

            while (m_gridCoordinateLabels.Count < visibleCount)
            {
                GameObject labelObject = new GameObject($"GridCoordinateLabel_{m_gridCoordinateLabels.Count}", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                labelObject.transform.SetParent(m_rectMapGridSelectionOverlay, false);
                TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
                ApplyGridCoordinateLabelStyle(label, m_tmpCreatureBrowserTitle);
                m_gridCoordinateLabels.Add(label);
            }

            for (int index = 0; index < m_gridCoordinateLabels.Count; index++)
            {
                m_gridCoordinateLabels[index].gameObject.SetActive(index < visibleCount);
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

        private Color GetEditorGridCoordinateLabelColor(Vector2 localPoint)
        {
            return TryGetMapPreviewColorAtLocalPoint(localPoint, out Color backgroundColor)
                ? GetContrastingGridCoordinateLabelColor(backgroundColor)
                : new Color(1f, 1f, 1f, 0.88f);
        }

        private bool TryGetMapPreviewColorAtLocalPoint(Vector2 localPoint, out Color color)
        {
            color = Color.clear;
            if (m_mapPreviewTexture == null || m_rectMapSurface == null)
            {
                return false;
            }

            Vector2 mapSize = m_rectMapSurface.sizeDelta;
            if (mapSize.x <= 0f || mapSize.y <= 0f)
            {
                return false;
            }

            Vector2 mapMin = m_mapPreviewPanOffset - mapSize * 0.5f;
            float u = (localPoint.x - mapMin.x) / mapSize.x;
            float v = (localPoint.y - mapMin.y) / mapSize.y;
            if (u < 0f || u > 1f || v < 0f || v > 1f)
            {
                return false;
            }

            color = m_mapPreviewTexture.GetPixelBilinear(u, v);
            return true;
        }

        private static Color GetContrastingGridCoordinateLabelColor(Color backgroundColor)
        {
            float luminance = backgroundColor.r * 0.2126f + backgroundColor.g * 0.7152f + backgroundColor.b * 0.0722f;
            return luminance > 0.52f
                ? new Color(0f, 0f, 0f, 0.88f)
                : new Color(1f, 1f, 1f, 0.92f);
        }

        private void RefreshCreatureInstanceTokens(List<ChapterCreatureInstanceData> sourceInstances, ChapterMapGridMetrics metrics, float minX, float maxX, float minY, float maxY)
        {
            if (m_rectMapGridSelectionOverlay == null)
            {
                HideCreatureInstanceActionPanel();
                return;
            }

            if (sourceInstances == null || sourceInstances.Count <= 0)
            {
                SetCreatureInstanceTokenCount(0);
                HideCreatureInstanceActionPanel();
                return;
            }

            List<ChapterCreatureInstanceData> visibleInstances = new List<ChapterCreatureInstanceData>();
            for (int index = 0; index < sourceInstances.Count; index++)
            {
                ChapterCreatureInstanceData creatureInstance = ChapterCreatureDataStructureUtility.NormalizeCreatureInstanceData(sourceInstances[index]);
                if (creatureInstance == null)
                {
                    continue;
                }

                Rect tokenRect = GetCreatureTokenRect(metrics, creatureInstance);
                if (tokenRect.xMax <= minX || tokenRect.xMin >= maxX || tokenRect.yMax <= minY || tokenRect.yMin >= maxY)
                {
                    continue;
                }

                visibleInstances.Add(creatureInstance);
            }

            SetCreatureInstanceTokenCount(visibleInstances.Count);
            for (int index = 0; index < visibleInstances.Count; index++)
            {
                ChapterCreatureInstanceData creatureInstance = visibleInstances[index];
                Rect tokenRect = GetCreatureTokenRect(metrics, creatureInstance);
                m_creatureInstanceTokenWidgets[index].Bind(
                    creatureInstance.RuntimeSheet,
                    tokenRect.center,
                    tokenRect.size,
                    !creatureInstance.IsActive,
                    string.Equals(creatureInstance.InstanceId, m_selectedCreatureInstanceId, StringComparison.Ordinal));
            }

            RefreshCreatureInstanceActionPanel(selectedChapter: GetSelectedChapterData(), metrics);
        }

        private void HideCreatureInstanceActionPanel()
        {
            if (m_rectCreatureInstanceActionPanel != null)
            {
                m_rectCreatureInstanceActionPanel.gameObject.SetActive(false);
            }
        }

        private void SetCreatureInstanceTokenCount(int visibleCount)
        {
            if (m_rectMapGridSelectionOverlay == null)
            {
                return;
            }

            while (m_creatureInstanceTokenWidgets.Count < visibleCount)
            {
                GameObject tokenObject = new GameObject($"CreatureInstanceToken_{m_creatureInstanceTokenWidgets.Count}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                tokenObject.transform.SetParent(m_rectMapGridSelectionOverlay, false);
                ChapterCreatureMapTokenWidget widget = new ChapterCreatureMapTokenWidget(tokenObject, m_tmpCreatureBrowserTitle);
                widget.SetVisible(false);
                m_creatureInstanceTokenWidgets.Add(widget);
            }

            for (int index = 0; index < m_creatureInstanceTokenWidgets.Count; index++)
            {
                m_creatureInstanceTokenWidgets[index].SetVisible(index < visibleCount);
            }
        }

        private void DisposeCreatureTokenViews()
        {
            for (int index = 0; index < m_creatureInstanceTokenWidgets.Count; index++)
            {
                m_creatureInstanceTokenWidgets[index].Dispose();
            }

            m_creatureInstanceTokenWidgets.Clear();

            if (m_creatureDragPreviewWidget != null)
            {
                m_creatureDragPreviewWidget.Dispose();
                m_creatureDragPreviewWidget = null;
            }

            if (m_rectCreaturePlacementPreview != null)
            {
                Object.Destroy(m_rectCreaturePlacementPreview.gameObject);
                m_rectCreaturePlacementPreview = null;
                m_imgCreaturePlacementPreview = null;
            }
        }

        private void RefreshCreatureInstanceActionPanel(ChapterListItemData selectedChapter, ChapterMapGridMetrics metrics)
        {
            EnsureCreatureInstanceActionPanel();
            if (m_rectCreatureInstanceActionPanel == null)
            {
                return;
            }

            ChapterCreatureInstanceData creatureInstance = FindCreatureInstanceById(selectedChapter?.CreatureInstances, m_selectedCreatureInstanceId);
            if (creatureInstance == null)
            {
                HideCreatureInstanceActionPanel();
                return;
            }

            Rect cellRect = GetCreatureTokenRect(metrics, creatureInstance);
            m_rectCreatureInstanceActionPanel.anchorMin = new Vector2(0.5f, 0.5f);
            m_rectCreatureInstanceActionPanel.anchorMax = new Vector2(0.5f, 0.5f);
            m_rectCreatureInstanceActionPanel.pivot = new Vector2(0.5f, 0f);
            m_rectCreatureInstanceActionPanel.anchoredPosition = new Vector2(cellRect.center.x, cellRect.yMax + 8f);
            m_rectCreatureInstanceActionPanel.gameObject.SetActive(true);
            m_rectCreatureInstanceActionPanel.SetAsLastSibling();

            if (m_textCreatureInstanceToggleActive != null)
            {
                m_textCreatureInstanceToggleActive.text = creatureInstance.IsActive ? "隐藏" : "显示";
            }
        }

        private void EnsureCreatureInstanceActionPanel()
        {
            if (m_rectCreatureInstanceActionPanel != null || m_rectMapGridSelectionOverlay == null)
            {
                return;
            }

            GameObject panelObject = new GameObject("CreatureInstanceActionPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            panelObject.transform.SetParent(m_rectMapGridSelectionOverlay, false);
            m_rectCreatureInstanceActionPanel = panelObject.GetComponent<RectTransform>();
            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(0.12f, 0.14f, 0.17f, 0.9f);
            HorizontalLayoutGroup layoutGroup = panelObject.GetComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 6f;
            layoutGroup.padding = new RectOffset(8, 8, 6, 6);
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = false;
            ContentSizeFitter fitter = panelObject.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            (m_btnCreatureInstanceEdit, m_btnCreatureInstanceEditRect, _) =
                CreateCreatureInstanceActionButton("CreatureInstanceEditBtn", "编辑", OnClickEditSelectedCreatureInstance);
            (m_btnCreatureInstanceToggleActive, m_btnCreatureInstanceToggleActiveRect, m_textCreatureInstanceToggleActive) =
                CreateCreatureInstanceActionButton("CreatureInstanceToggleActiveBtn", "隐藏", OnClickToggleSelectedCreatureInstanceActive);
            (m_btnCreatureInstanceDelete, m_btnCreatureInstanceDeleteRect, _) =
                CreateCreatureInstanceActionButton("CreatureInstanceDeleteBtn", "移除", OnClickDeleteSelectedCreatureInstance);

            if (m_btnCreatureInstanceEditRect != null)
            {
                m_btnCreatureInstanceEditRect.SetParent(m_rectCreatureInstanceActionPanel, false);
            }

            if (m_btnCreatureInstanceToggleActiveRect != null)
            {
                m_btnCreatureInstanceToggleActiveRect.SetParent(m_rectCreatureInstanceActionPanel, false);
            }

            if (m_btnCreatureInstanceDeleteRect != null)
            {
                m_btnCreatureInstanceDeleteRect.SetParent(m_rectCreatureInstanceActionPanel, false);
            }

            m_rectCreatureInstanceActionPanel.gameObject.SetActive(false);
        }

        private (Button button, RectTransform rect, TMP_Text label) CreateCreatureInstanceActionButton(string objectName, string labelText, UnityAction onClick)
        {
            GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.27f, 0.31f, 0.36f, 0.96f);
            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(onClick);
            LayoutElement layoutElement = buttonObject.GetComponent<LayoutElement>();
            layoutElement.minWidth = 56f;
            layoutElement.minHeight = 28f;

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonObject.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(6f, 3f);
            labelRect.offsetMax = new Vector2(-6f, -3f);

            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            if (m_tmpCreatureBrowserTitle != null)
            {
                label.font = m_tmpCreatureBrowserTitle.font;
                label.fontSharedMaterial = m_tmpCreatureBrowserTitle.fontSharedMaterial;
            }

            label.text = labelText;
            label.fontSize = 15f;
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = false;
            label.raycastTarget = false;
            label.color = new Color(0.95f, 0.96f, 0.98f, 0.96f);
            return (button, rectTransform, label);
        }

        private void OnClickEditSelectedCreatureInstance()
        {
            ChapterListItemData selectedChapter = GetSelectedChapterData();
            ChapterCreatureInstanceData creatureInstance = FindCreatureInstanceById(selectedChapter?.CreatureInstances, m_selectedCreatureInstanceId);
            if (creatureInstance == null)
            {
                return;
            }

            creatureInstance = ChapterCreatureDataStructureUtility.NormalizeCreatureInstanceData(creatureInstance);
            string instanceId = creatureInstance.InstanceId ?? string.Empty;
            ChapterCreatureData initialData = ChapterCreatureDataStructureUtility.CloneCreatureData(creatureInstance.RuntimeSheet);
            GameModule.UI.ShowUIAsync<ChapterCreatureEntryPopupUI>(new ChapterCreatureEntryPopupRequest
            {
                InitialData = initialData,
                OnConfirm = updatedData => UpdateCreatureInstanceRuntimeSheet(instanceId, updatedData),
            });
        }

        private void UpdateCreatureInstanceRuntimeSheet(string instanceId, ChapterCreatureData updatedData)
        {
            if (string.IsNullOrWhiteSpace(instanceId) || updatedData == null)
            {
                return;
            }

            ChapterListItemData selectedChapter = GetSelectedChapterData();
            ChapterCreatureInstanceData creatureInstance = FindCreatureInstanceById(selectedChapter?.CreatureInstances, instanceId);
            if (creatureInstance == null)
            {
                return;
            }

            string existingRuntimeCreatureId = creatureInstance.RuntimeSheet?.CreatureId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(updatedData.CreatureId))
            {
                updatedData.CreatureId = existingRuntimeCreatureId;
            }

            creatureInstance.RuntimeSheet = ChapterCreatureDataStructureUtility.CloneCreatureData(updatedData);
            ChapterCreatureDataStructureUtility.NormalizeCreatureInstanceData(creatureInstance);
            m_selectedCreatureInstanceId = creatureInstance.InstanceId ?? string.Empty;
            RefreshGridSelectionHighlights();
            bool saved = SaveChapterEditorState();
            ShowSaveFeedbackAsync(saved ? "Creature instance updated." : "Failed to update creature instance.", saved).Forget();
        }

        private void OnClickToggleSelectedCreatureInstanceActive()
        {
            ChapterListItemData selectedChapter = GetSelectedChapterData();
            ChapterCreatureInstanceData creatureInstance = FindCreatureInstanceById(selectedChapter?.CreatureInstances, m_selectedCreatureInstanceId);
            if (creatureInstance == null)
            {
                return;
            }

            creatureInstance.IsActive = !creatureInstance.IsActive;
            RefreshGridSelectionHighlights();
            bool saved = SaveChapterEditorState();
            ShowSaveFeedbackAsync(saved
                ? (creatureInstance.IsActive ? "Creature shown." : "Creature hidden.")
                : "Failed to save creature visibility.", saved).Forget();
        }

        private void OnClickDeleteSelectedCreatureInstance()
        {
            ChapterListItemData selectedChapter = GetSelectedChapterData();
            if (selectedChapter?.CreatureInstances == null)
            {
                return;
            }

            for (int index = selectedChapter.CreatureInstances.Count - 1; index >= 0; index--)
            {
                ChapterCreatureInstanceData creatureInstance = selectedChapter.CreatureInstances[index];
                if (creatureInstance == null || !string.Equals(creatureInstance.InstanceId, m_selectedCreatureInstanceId, StringComparison.Ordinal))
                {
                    continue;
                }

                selectedChapter.CreatureInstances.RemoveAt(index);
                m_selectedCreatureInstanceId = string.Empty;
                RefreshGridSelectionHighlights();
                bool saved = SaveChapterEditorState();
                ShowSaveFeedbackAsync(saved ? "Creature removed." : "Failed to remove creature.", saved).Forget();
                return;
            }
        }

        private bool TryGetCreatureInstanceAtLocalPoint(Vector2 localPoint, bool includeInactive, out ChapterCreatureInstanceData creatureInstance)
        {
            creatureInstance = null;
            ChapterListItemData selectedChapter = GetSelectedChapterData();
            if (selectedChapter?.CreatureInstances == null || !TryGetCurrentGridMetrics(out ChapterMapGridMetrics metrics))
            {
                return false;
            }

            for (int index = selectedChapter.CreatureInstances.Count - 1; index >= 0; index--)
            {
                ChapterCreatureInstanceData candidate = ChapterCreatureDataStructureUtility.NormalizeCreatureInstanceData(selectedChapter.CreatureInstances[index]);
                if (candidate == null || (!includeInactive && !candidate.IsActive))
                {
                    continue;
                }

                Rect tokenRect = GetCreatureTokenRect(metrics, candidate);
                if (!tokenRect.Contains(localPoint))
                {
                    continue;
                }

                creatureInstance = candidate;
                return true;
            }

            return false;
        }

        private ChapterCreatureInstanceData FindCreatureInstanceById(List<ChapterCreatureInstanceData> sourceInstances, string instanceId)
        {
            if (sourceInstances == null || string.IsNullOrWhiteSpace(instanceId))
            {
                return null;
            }

            for (int index = 0; index < sourceInstances.Count; index++)
            {
                ChapterCreatureInstanceData creatureInstance = sourceInstances[index];
                if (creatureInstance != null && string.Equals(creatureInstance.InstanceId, instanceId, StringComparison.Ordinal))
                {
                    return creatureInstance;
                }
            }

            return null;
        }

        private void BringGridEventMarkersToFront()
        {
            for (int index = 0; index < m_gridEventMarkers.Count; index++)
            {
                if (m_gridEventMarkers[index] != null && m_gridEventMarkers[index].gameObject.activeInHierarchy)
                {
                    m_gridEventMarkers[index].transform.SetAsLastSibling();
                }
            }
        }

        private bool TryGetCurrentGridMetrics(out ChapterMapGridMetrics metrics)
        {
            metrics = default;

            if (m_rectMapGridOverlay == null)
            {
                return false;
            }

            metrics = ChapterMapGridUtility.CreateMetrics(m_rectMapGridOverlay.rect, BaseMapGridColumns, BaseMapGridRows, m_mapGridZoomScale, m_mapGridPanOffset);
            return true;
        }

        private bool TryGetGridCellCoordinateFromLocalPoint(Vector2 localPoint, out ChapterGridCoordinate gridCoordinate)
        {
            gridCoordinate = ChapterGridCoordinate.Zero;
            if (!TryGetCurrentGridMetrics(out ChapterMapGridMetrics metrics))
            {
                return false;
            }

            return ChapterMapGridUtility.TryGetCellCoordinateFromLocalPoint(localPoint, metrics, out gridCoordinate);
        }

        private void UpdateGridLineCollection(List<RectTransform> gridLines, RectTransform template, List<float> positions, bool isVertical, float overlayWidth, float overlayHeight)
        {
            if (gridLines == null || template == null || m_rectMapGridOverlay == null)
            {
                return;
            }

            int visibleCount = positions != null ? positions.Count : 0;
            while (gridLines.Count < visibleCount)
            {
                RectTransform clonedLine = Object.Instantiate(template.gameObject, m_rectMapGridOverlay, false).GetComponent<RectTransform>();
                clonedLine.name = isVertical
                    ? $"m_imgGridLineV{gridLines.Count + 1}"
                    : $"m_imgGridLineH{gridLines.Count + 1}";
                clonedLine.SetSiblingIndex(0);
                gridLines.Add(clonedLine);
            }

            for (int index = 0; index < gridLines.Count; index++)
            {
                RectTransform gridLine = gridLines[index];
                bool shouldShow = index < visibleCount;
                if (gridLine.gameObject.activeSelf != shouldShow)
                {
                    gridLine.gameObject.SetActive(shouldShow);
                }

                if (!shouldShow)
                {
                    continue;
                }

                if (isVertical)
                {
                    float anchor = positions[index] / Mathf.Max(1f, overlayWidth);
                    gridLine.anchorMin = new Vector2(anchor, 0f);
                    gridLine.anchorMax = new Vector2(anchor, 1f);
                    gridLine.pivot = new Vector2(0.5f, 0.5f);
                    gridLine.anchoredPosition = Vector2.zero;
                    gridLine.sizeDelta = new Vector2(GridLineThickness, 0f);
                }
                else
                {
                    float anchor = positions[index] / Mathf.Max(1f, overlayHeight);
                    gridLine.anchorMin = new Vector2(0f, anchor);
                    gridLine.anchorMax = new Vector2(1f, anchor);
                    gridLine.pivot = new Vector2(0.5f, 0.5f);
                    gridLine.anchoredPosition = Vector2.zero;
                    gridLine.sizeDelta = new Vector2(0f, GridLineThickness);
                }
            }
        }

        private static int CompareGridLineByName(RectTransform left, RectTransform right)
        {
            return ExtractTrailingNumber(left != null ? left.name : string.Empty).CompareTo(ExtractTrailingNumber(right != null ? right.name : string.Empty));
        }

        private static int ExtractTrailingNumber(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            int index = value.Length - 1;
            while (index >= 0 && char.IsDigit(value[index]))
            {
                index--;
            }

            string numericSuffix = value.Substring(index + 1);
            return int.TryParse(numericSuffix, out int number) ? number : 0;
        }

        private void UpdateMapSurfaceLayoutToFit()
        {
            if (m_rectMapSurface == null || m_rectMapPreview == null || m_mapPreviewTexture == null)
            {
                return;
            }

            float availableWidth = Mathf.Max(0f, m_rectMapPreview.rect.width - MapPreviewPadding);
            float availableHeight = Mathf.Max(0f, m_rectMapPreview.rect.height - MapPreviewPadding);
            if (availableWidth <= 0f || availableHeight <= 0f)
            {
                return;
            }

            float textureWidth = Mathf.Max(1f, m_mapPreviewTexture.width);
            float textureHeight = Mathf.Max(1f, m_mapPreviewTexture.height);
            float scale = Mathf.Min(availableWidth / textureWidth, availableHeight / textureHeight);
            float fittedWidth = textureWidth * scale * m_mapPreviewZoomScale;
            float fittedHeight = textureHeight * scale * m_mapPreviewZoomScale;

            m_rectMapSurface.anchorMin = new Vector2(0.5f, 0.5f);
            m_rectMapSurface.anchorMax = new Vector2(0.5f, 0.5f);
            m_rectMapSurface.pivot = new Vector2(0.5f, 0.5f);
            m_rectMapSurface.anchoredPosition = m_mapPreviewPanOffset;
            m_rectMapSurface.sizeDelta = new Vector2(fittedWidth, fittedHeight);
        }

        private void ResetMapSurfaceLayout()
        {
            if (m_rectMapSurface == null)
            {
                return;
            }

            m_rectMapSurface.anchorMin = Vector2.zero;
            m_rectMapSurface.anchorMax = Vector2.one;
            m_rectMapSurface.pivot = new Vector2(0.5f, 0.5f);
            m_rectMapSurface.anchoredPosition = Vector2.zero;
            m_rectMapSurface.sizeDelta = new Vector2(-MapPreviewPadding, -MapPreviewPadding);
        }

        private void UpdateMapZoomToggleButtonLayout()
        {
            if (m_btnMapZoomToggleRect == null)
            {
                return;
            }

            m_btnMapZoomToggleRect.anchorMin = new Vector2(0f, 1f);
            m_btnMapZoomToggleRect.anchorMax = new Vector2(0f, 1f);
            m_btnMapZoomToggleRect.pivot = new Vector2(0f, 1f);
            m_btnMapZoomToggleRect.anchoredPosition = new Vector2(20f, -20f);
            m_btnMapZoomToggleRect.sizeDelta = new Vector2(156f, 42f);
        }

        private void UpdateGridZoomToggleButtonLayout()
        {
            if (m_btnGridZoomToggleRect == null)
            {
                return;
            }

            m_btnGridZoomToggleRect.anchorMin = new Vector2(0f, 1f);
            m_btnGridZoomToggleRect.anchorMax = new Vector2(0f, 1f);
            m_btnGridZoomToggleRect.pivot = new Vector2(0f, 1f);
            m_btnGridZoomToggleRect.anchoredPosition = new Vector2(20f, -70f);
            m_btnGridZoomToggleRect.sizeDelta = new Vector2(156f, 42f);
        }

        private void UpdateSaveChapterStateButtonLayout()
        {
            if (m_btnSaveChapterStateRect == null)
            {
                return;
            }
        }

        private void UpdatePreviewModuleButtonLayout()
        {
            if (m_btnPreviewModuleRect == null || m_btnSaveChapterStateRect == null)
            {
                return;
            }

            const float buttonSpacing = 16f;
            m_btnPreviewModuleRect.anchorMin = m_btnSaveChapterStateRect.anchorMin;
            m_btnPreviewModuleRect.anchorMax = m_btnSaveChapterStateRect.anchorMax;
            m_btnPreviewModuleRect.pivot = m_btnSaveChapterStateRect.pivot;
            m_btnPreviewModuleRect.sizeDelta = m_btnSaveChapterStateRect.sizeDelta;
            m_btnPreviewModuleRect.anchoredPosition = m_btnSaveChapterStateRect.anchoredPosition
                + new Vector2(m_btnSaveChapterStateRect.sizeDelta.x + buttonSpacing, 0f);
        }

        private void UpdateTerrainToolButtons(bool hasSelectedChapter, bool hasMap, bool hasSelectedCells)
        {
            UpdateTerrainToolButton(TerrainToolButtonType.DifficultTerrain, hasSelectedChapter, hasMap, hasSelectedCells);
            UpdateTerrainToolButton(TerrainToolButtonType.ImpassableTerrain, hasSelectedChapter, hasMap, hasSelectedCells);
            UpdateTerrainToolButton(TerrainToolButtonType.ClearTerrain, hasSelectedChapter, hasMap, hasSelectedCells);
        }

        private void UpdateTerrainToolButton(TerrainToolButtonType buttonType, bool hasSelectedChapter, bool hasMap, bool hasSelectedCells)
        {
            Button button = GetTerrainToolButton(buttonType);
            if (button != null)
            {
                button.gameObject.SetActive(hasSelectedChapter);
                button.interactable = hasMap && hasSelectedCells;
            }

            UpdateTerrainToolButtonLayout(buttonType);
            UpdateTerrainToolButtonVisual(buttonType, hasMap, hasSelectedCells);
        }

        private void UpdateTerrainToolButtonLayout(TerrainToolButtonType buttonType)
        {
            RectTransform rectTransform = GetTerrainToolButtonRect(buttonType);
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = new Vector2(1f, 0f);
            rectTransform.anchorMax = new Vector2(1f, 0f);
            rectTransform.pivot = new Vector2(1f, 0f);
            rectTransform.anchoredPosition = new Vector2(-20f, GetTerrainToolButtonBottomOffset(buttonType));
            rectTransform.sizeDelta = new Vector2(180f, 42f);
        }

        private void UpdateMapZoomToggleVisual(bool hasMap)
        {
            if (m_textMapZoomToggle != null)
            {
                m_textMapZoomToggle.text = m_isMapZoomEnabled ? "Map Edit: On" : "Map Edit: Off";
            }

            if (m_imgMapZoomToggle != null)
            {
                if (!hasMap)
                {
                    m_imgMapZoomToggle.color = new Color(0.55f, 0.58f, 0.61f, 0.72f);
                    return;
                }

                m_imgMapZoomToggle.color = m_isMapZoomEnabled
                    ? new Color(0.22f, 0.55f, 0.38f, 0.95f)
                    : new Color(0.37f, 0.43f, 0.49f, 0.92f);
            }
        }

        private void UpdateGridZoomToggleVisual(bool hasSelectedChapter)
        {
            if (m_textGridZoomToggle != null)
            {
                m_textGridZoomToggle.text = m_isGridZoomEnabled ? "Grid Edit: On" : "Grid Edit: Off";
            }

            if (m_imgGridZoomToggle != null)
            {
                if (!hasSelectedChapter)
                {
                    m_imgGridZoomToggle.color = new Color(0.55f, 0.58f, 0.61f, 0.72f);
                    return;
                }

                m_imgGridZoomToggle.color = m_isGridZoomEnabled
                    ? new Color(0.64f, 0.45f, 0.18f, 0.95f)
                    : new Color(0.37f, 0.43f, 0.49f, 0.92f);
            }
        }

        private void UpdateSaveChapterStateButtonVisual(bool hasSelectedChapter)
        {
            if (m_textSaveChapterState != null)
            {
                m_textSaveChapterState.text = "手动保存";
            }

            if (m_imgSaveChapterState != null)
            {
                if (!hasSelectedChapter)
                {
                    m_imgSaveChapterState.color = new Color(0.55f, 0.58f, 0.61f, 0.72f);
                    return;
                }

                m_imgSaveChapterState.color = new Color(0.29f, 0.47f, 0.31f, 0.94f);
            }
        }

        private void UpdatePreviewModuleButtonVisual(bool canPreview)
        {
            if (m_textPreviewModule != null)
            {
                m_textPreviewModule.text = "预览";
            }

            if (m_imgPreviewModule != null)
            {
                if (!canPreview)
                {
                    m_imgPreviewModule.color = new Color(0.55f, 0.58f, 0.61f, 0.72f);
                    return;
                }

                m_imgPreviewModule.color = new Color(0.22f, 0.41f, 0.64f, 0.94f);
            }
        }

        private void UpdateTerrainToolButtonVisual(TerrainToolButtonType buttonType, bool hasMap, bool hasSelectedCells)
        {
            TMP_Text text = GetTerrainToolButtonText(buttonType);
            if (text != null)
            {
                text.text = GetTerrainToolButtonLabel(buttonType, hasSelectedCells);
            }

            Image image = GetTerrainToolButtonImage(buttonType);
            if (image == null)
            {
                return;
            }

            if (!hasMap)
            {
                image.color = new Color(0.55f, 0.58f, 0.61f, 0.72f);
                return;
            }

            image.color = hasSelectedCells
                ? GetTerrainToolButtonEnabledColor(buttonType)
                : GetTerrainToolButtonDisabledColor(buttonType);
        }

        private void ResetMapZoomState()
        {
            m_isMapZoomEnabled = false;
            m_isDraggingMap = false;
            m_mapPreviewZoomScale = 1f;
            m_mapPreviewPanOffset = Vector2.zero;
        }

        private void ResetGridZoomState()
        {
            m_isGridZoomEnabled = false;
            m_isDraggingGrid = false;
            m_mapGridZoomScale = 1f;
            m_mapGridPanOffset = Vector2.zero;
            ApplyMapGridLayout();
        }

        private void ResetMapGridLockState()
        {
            m_isMapGridLocked = false;
            m_isDraggingLockedMapGrid = false;
            m_lockedMapZoomReference = 1f;
            m_lockedGridToMapZoomRatio = 1f;
            m_lockedGridToMapPanDelta = Vector2.zero;
        }

        private void UpdateAutoMapGridLockState(bool captureRelationWhenLocked, bool saveState)
        {
            bool shouldLock = m_isMapZoomEnabled && m_isGridZoomEnabled && m_mapPreviewTexture != null;
            bool lockStateChanged = m_isMapGridLocked != shouldLock;

            m_isMapGridLocked = shouldLock;
            if (!m_isMapGridLocked)
            {
                m_isDraggingLockedMapGrid = false;
            }
            else if (lockStateChanged || captureRelationWhenLocked)
            {
                CaptureMapGridLockRelation();
            }

            if (saveState && lockStateChanged)
            {
                SaveChapterEditorState();
            }
        }

        private void CaptureMapGridLockRelation()
        {
            m_lockedMapZoomReference = Mathf.Max(0.0001f, m_mapPreviewZoomScale);
            m_lockedGridToMapZoomRatio = m_mapGridZoomScale / Mathf.Max(0.0001f, m_mapPreviewZoomScale);
            m_lockedGridToMapPanDelta = m_mapGridPanOffset - m_mapPreviewPanOffset;
        }

        private void ApplyLockedMapGridScroll(float scrollDelta)
        {
            bool canEditMap = m_isMapZoomEnabled && m_mapPreviewTexture != null;
            bool canEditGrid = m_isGridZoomEnabled;
            if (!canEditMap && !canEditGrid)
            {
                return;
            }

            float normalScrollStep = canEditMap && !canEditGrid ? MapZoomScrollStep : GridZoomScrollStep;
            float appliedScrollDelta = GetAppliedZoomScrollDelta(scrollDelta, normalScrollStep);
            float lockedMapMin = MapZoomMinScale;
            float lockedMapMax = MapZoomMaxScale;
            if (lockedMapMax <= lockedMapMin)
            {
                return;
            }

            m_mapPreviewZoomScale = Mathf.Clamp(m_mapPreviewZoomScale + appliedScrollDelta, lockedMapMin, lockedMapMax);
            SyncLockedGridTransformWithMap();
            UpdateMapSurfaceLayoutToFit();
            ApplyMapGridLayout();
        }

        private void SyncLockedGridTransformWithMap()
        {
            float zoomRatio = Mathf.Max(0.0001f, m_lockedGridToMapZoomRatio);
            float mapZoomReference = Mathf.Max(0.0001f, m_lockedMapZoomReference);
            float panScale = m_mapPreviewZoomScale / mapZoomReference;

            m_mapGridZoomScale = m_mapPreviewZoomScale * zoomRatio;
            m_mapGridPanOffset = m_mapPreviewPanOffset + m_lockedGridToMapPanDelta * panScale;
        }

        private bool IsMouseOverMapPreview()
        {
            if (m_rectMapPreview == null)
            {
                return false;
            }

            return RectTransformUtility.RectangleContainsScreenPoint(m_rectMapPreview, Input.mousePosition, GetMapPreviewEventCamera());
        }

        private void HandleMapDragging()
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (m_isDraggingMap)
                {
                    SaveChapterEditorState();
                }

                m_isDraggingMap = false;
            }

            if (!m_isMapZoomEnabled || m_mapPreviewTexture == null || m_rectMapSurface == null)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (!IsMouseOverMapPreview() || IsMouseOverMapControlButton() || IsMouseOverCreatureInstanceToken())
                {
                    return;
                }

                if (TryGetMouseLocalPointInMapPreview(out Vector2 localPoint))
                {
                    CancelPendingGridCellSelection();
                    m_isDraggingMap = true;
                    m_lastMapDragLocalPoint = localPoint;
                }

                return;
            }

            if (!m_isDraggingMap || !Input.GetMouseButton(0))
            {
                return;
            }

            if (!TryGetMouseLocalPointInMapPreview(out Vector2 currentLocalPoint))
            {
                return;
            }

            Vector2 dragDelta = currentLocalPoint - m_lastMapDragLocalPoint;
            if (dragDelta.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            m_lastMapDragLocalPoint = currentLocalPoint;
            m_mapPreviewPanOffset += dragDelta;
            UpdateMapSurfaceLayoutToFit();
        }

        private void HandleGridDragging()
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (m_isDraggingGrid)
                {
                    SaveChapterEditorState();
                }

                m_isDraggingGrid = false;
            }

            if (!m_isGridZoomEnabled || m_rectMapGridOverlay == null || !m_rectMapGridOverlay.gameObject.activeInHierarchy)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (!IsMouseOverMapPreview() || IsMouseOverMapControlButton() || IsMouseOverCreatureInstanceToken())
                {
                    return;
                }

                if (TryGetMouseLocalPointInMapPreview(out Vector2 localPoint))
                {
                    CancelPendingGridCellSelection();
                    m_isDraggingGrid = true;
                    m_lastGridDragLocalPoint = localPoint;
                }

                return;
            }

            if (!m_isDraggingGrid || !Input.GetMouseButton(0))
            {
                return;
            }

            if (!TryGetMouseLocalPointInMapPreview(out Vector2 currentLocalPoint))
            {
                return;
            }

            Vector2 dragDelta = currentLocalPoint - m_lastGridDragLocalPoint;
            if (dragDelta.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            m_lastGridDragLocalPoint = currentLocalPoint;
            m_mapGridPanOffset += dragDelta;
            ApplyMapGridLayout();
        }

        private void HandleLockedMapGridDragging()
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (m_isDraggingLockedMapGrid)
                {
                    SaveChapterEditorState();
                }

                m_isDraggingLockedMapGrid = false;
            }

            if (!m_isMapGridLocked || m_mapPreviewTexture == null)
            {
                return;
            }

            bool canEditMap = m_isMapZoomEnabled;
            bool canEditGrid = m_isGridZoomEnabled;
            if (!canEditMap && !canEditGrid)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (!IsMouseOverMapPreview() || IsMouseOverMapControlButton() || IsMouseOverCreatureInstanceToken())
                {
                    return;
                }

                if (TryGetMouseLocalPointInMapPreview(out Vector2 localPoint))
                {
                    CancelPendingGridCellSelection();
                    m_isDraggingLockedMapGrid = true;
                    m_lastLockedMapGridDragLocalPoint = localPoint;
                }

                return;
            }

            if (!m_isDraggingLockedMapGrid || !Input.GetMouseButton(0))
            {
                return;
            }

            if (!TryGetMouseLocalPointInMapPreview(out Vector2 currentLocalPoint))
            {
                return;
            }

            Vector2 dragDelta = currentLocalPoint - m_lastLockedMapGridDragLocalPoint;
            if (dragDelta.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            m_lastLockedMapGridDragLocalPoint = currentLocalPoint;
            m_mapPreviewPanOffset += dragDelta;
            SyncLockedGridTransformWithMap();
            UpdateMapSurfaceLayoutToFit();
            ApplyMapGridLayout();
        }

        private void HandleCreatureInstanceInteraction()
        {
            if (!m_isMapGridLocked)
            {
                CancelPendingCreatureInstanceSelection();
                if (!m_isDraggingCreatureInstance)
                {
                    return;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                TryBeginCreatureInstanceSelection();
            }

            if (m_isPendingCreatureInstanceSelection && Input.GetMouseButton(0))
            {
                TryBeginCreatureInstanceDragging();
            }

            if (Input.GetMouseButtonUp(0))
            {
                TryCommitCreatureInstanceInteraction();
            }
        }

        private void TryBeginCreatureInstanceSelection()
        {
            CancelPendingCreatureInstanceSelection();
            if (!IsMouseOverMapPreview() || IsMouseOverMapControlButton())
            {
                return;
            }

            if (!TryGetMouseLocalPointInMapPreview(out Vector2 localPoint))
            {
                return;
            }

            if (!TryGetCreatureInstanceAtLocalPoint(localPoint, includeInactive: true, out ChapterCreatureInstanceData creatureInstance))
            {
                if (!string.IsNullOrWhiteSpace(m_selectedCreatureInstanceId))
                {
                    m_selectedCreatureInstanceId = string.Empty;
                    RefreshGridSelectionHighlights();
                }

                return;
            }

            CancelPendingGridCellSelection();
            m_isPendingCreatureInstanceSelection = true;
            m_pendingCreatureInstanceId = creatureInstance.InstanceId ?? string.Empty;
            m_creatureInstanceMouseDownLocalPoint = localPoint;
        }

        private void TryBeginCreatureInstanceDragging()
        {
            if (!m_isPendingCreatureInstanceSelection || string.IsNullOrWhiteSpace(m_pendingCreatureInstanceId))
            {
                return;
            }

            if (!TryGetMouseLocalPointInMapPreview(out Vector2 currentLocalPoint))
            {
                return;
            }

            if ((currentLocalPoint - m_creatureInstanceMouseDownLocalPoint).sqrMagnitude <= GridSelectionClickThreshold * GridSelectionClickThreshold)
            {
                return;
            }

            m_selectedCreatureInstanceId = m_pendingCreatureInstanceId;
            m_draggingCreatureInstanceId = m_pendingCreatureInstanceId;
            m_isDraggingCreatureInstance = true;
            m_isPendingCreatureInstanceSelection = false;
            m_pendingCreatureInstanceId = string.Empty;
            RefreshGridSelectionHighlights();
        }

        private void TryCommitCreatureInstanceInteraction()
        {
            if (m_isDraggingCreatureInstance)
            {
                TryCommitCreatureInstanceDragging();
                return;
            }

            if (!m_isPendingCreatureInstanceSelection)
            {
                return;
            }

            m_selectedCreatureInstanceId = m_pendingCreatureInstanceId ?? string.Empty;
            CancelPendingCreatureInstanceSelection();
            RefreshGridSelectionHighlights();
        }

        private void TryCommitCreatureInstanceDragging()
        {
            string draggingInstanceId = m_draggingCreatureInstanceId;
            m_isDraggingCreatureInstance = false;
            m_draggingCreatureInstanceId = string.Empty;
            HideCreaturePlacementPreview();
            if (string.IsNullOrWhiteSpace(draggingInstanceId))
            {
                return;
            }

            ChapterListItemData selectedChapter = GetSelectedChapterData();
            if (selectedChapter == null)
            {
                RefreshGridSelectionHighlights();
                return;
            }

            ChapterCreatureInstanceData creatureInstance = FindCreatureInstanceById(selectedChapter.CreatureInstances, draggingInstanceId);
            if (creatureInstance != null
                && TryGetMouseLocalPointInMapPreview(out Vector2 localPoint)
                && TryGetGridCellCoordinateFromLocalPoint(localPoint, out ChapterGridCoordinate gridCoordinate))
            {
                creatureInstance.Placement ??= new ChapterCreatureInstancePlacementData();
                creatureInstance.Placement.AnchorCell = gridCoordinate;
                bool saved = SaveChapterEditorState();
                ShowSaveFeedbackAsync(saved ? "Creature repositioned." : "Creature moved, but save failed.", saved).Forget();
            }

            RefreshGridSelectionHighlights();
        }

        private void CancelPendingCreatureInstanceSelection()
        {
            m_isPendingCreatureInstanceSelection = false;
            m_pendingCreatureInstanceId = string.Empty;
        }

        private void HandleGridCellSelection()
        {
            if (!CanHandleGridCellSelection())
            {
                CancelPendingGridCellSelection();
                return;
            }

            if (Input.GetMouseButtonUp(0))
            {
                TryCommitGridCellSelection();
            }

            if (Input.GetMouseButtonUp(1))
            {
                TryOpenGridCellEventPopup();
            }

            if (Input.GetMouseButtonDown(0))
            {
                TryBeginGridCellSelection();
            }

            if (Input.GetMouseButtonDown(1))
            {
                TryBeginGridCellEventPopup();
            }
        }

        private void TryBeginGridCellSelection()
        {
            m_isPendingGridCellSelection = false;
            if (!IsMouseOverMapPreview() || IsMouseOverMapControlButton() || IsMouseOverCreatureInstanceToken())
            {
                return;
            }

            if (!TryGetMouseLocalPointInMapPreview(out Vector2 localPoint))
            {
                return;
            }

            if (!TryGetGridCellCoordinateFromLocalPoint(localPoint, out ChapterGridCoordinate gridCoordinate))
            {
                return;
            }

            m_isPendingGridCellSelection = true;
            m_gridCellSelectionMouseDownLocalPoint = localPoint;
            m_gridCellSelectionMouseDownCoordinate = gridCoordinate;
        }

        private void TryBeginGridCellEventPopup()
        {
            m_isPendingGridCellEventPopup = false;
            if (!IsMouseOverMapPreview() || IsMouseOverMapControlButton() || IsMouseOverCreatureInstanceToken())
            {
                return;
            }

            if (!TryGetMouseLocalPointInMapPreview(out Vector2 localPoint))
            {
                return;
            }

            if (!TryGetGridCellCoordinateFromLocalPoint(localPoint, out ChapterGridCoordinate gridCoordinate))
            {
                return;
            }

            m_isPendingGridCellEventPopup = true;
            m_gridCellEventPopupMouseDownLocalPoint = localPoint;
            m_gridCellEventPopupMouseDownCoordinate = gridCoordinate;
        }

        private void TryCommitGridCellSelection()
        {
            if (!m_isPendingGridCellSelection)
            {
                return;
            }

            m_isPendingGridCellSelection = false;
            if (!TryGetMouseLocalPointInMapPreview(out Vector2 localPoint))
            {
                return;
            }

            if ((localPoint - m_gridCellSelectionMouseDownLocalPoint).sqrMagnitude > GridSelectionClickThreshold * GridSelectionClickThreshold)
            {
                return;
            }

            ChapterListItemData selectedChapter = GetSelectedChapterData();
            if (selectedChapter == null)
            {
                return;
            }

            selectedChapter.GridCells ??= new List<ChapterGridCellData>();

            ChapterGridCellCollectionUtility.ToggleSelectedCell(selectedChapter.GridCells, m_gridCellSelectionMouseDownCoordinate);

            RefreshGridSelectionHighlights();
            SaveChapterEditorState();
        }

        private void TryOpenGridCellEventPopup()
        {
            if (!m_isPendingGridCellEventPopup)
            {
                return;
            }

            m_isPendingGridCellEventPopup = false;
            if (!TryGetMouseLocalPointInMapPreview(out Vector2 localPoint))
            {
                return;
            }

            if ((localPoint - m_gridCellEventPopupMouseDownLocalPoint).sqrMagnitude > GridSelectionClickThreshold * GridSelectionClickThreshold)
            {
                return;
            }

            ChapterListItemData selectedChapter = GetSelectedChapterData();
            if (selectedChapter == null)
            {
                return;
            }

            selectedChapter.GridCells ??= new List<ChapterGridCellData>();
            ChapterEventCollectionUtility.TryGetEventData(selectedChapter.Events, selectedChapter.EventBindings, m_gridCellEventPopupMouseDownCoordinate, out ChapterGridEventData existingEventData);
            OpenChapterEventPopup(selectedChapter, new List<ChapterGridCoordinate> { m_gridCellEventPopupMouseDownCoordinate }, existingEventData);
        }

        private static bool TryGetSelectedGridCoordinates(ChapterListItemData chapter, out List<ChapterGridCoordinate> coordinates)
        {
            coordinates = new List<ChapterGridCoordinate>();
            List<ChapterGridCellData> selectedCells = ChapterGridCellCollectionUtility.GetCellsByMarkType(chapter?.GridCells, ChapterGridCellMarkType.Selected);
            if (selectedCells.Count <= 0)
            {
                return false;
            }

            for (int index = 0; index < selectedCells.Count; index++)
            {
                if (selectedCells[index] == null)
                {
                    continue;
                }

                coordinates.Add(selectedCells[index].Coordinate);
            }

            return coordinates.Count > 0;
        }

        private bool CanHandleGridCellSelection()
        {
            if (m_rectMapPreview == null || !m_rectMapPreview.gameObject.activeInHierarchy)
            {
                return false;
            }

            return m_isMapGridLocked;
        }

        private void CancelPendingGridCellSelection()
        {
            m_isPendingGridCellSelection = false;
            m_isPendingGridCellEventPopup = false;
            m_gridCellSelectionMouseDownCoordinate = ChapterGridCoordinate.Zero;
            m_gridCellEventPopupMouseDownCoordinate = ChapterGridCoordinate.Zero;
        }

        private bool IsMouseOverMapControlButton()
        {
            return IsMouseOverRectTransform(m_btnOpenCheckPopupRect)
                || IsMouseOverRectTransform(m_btnDeleteGridEventRect)
                || IsMouseOverRectTransform(m_btnUploadMapRect)
                || IsMouseOverRectTransform(m_btnMapZoomToggleRect)
                || IsMouseOverRectTransform(m_btnGridZoomToggleRect)
                || IsMouseOverRectTransform(m_btnSaveChapterStateRect)
                || IsMouseOverRectTransform(m_btnPreviewModuleRect)
                || IsMouseOverTerrainToolButtons()
                || IsMouseOverCreatureInstanceActionButtons();
        }

        private void UpdateGridEventActionButtons()
        {
            ChapterListItemData selectedChapter = GetSelectedChapterData();
            bool hasSelectedChapter = selectedChapter != null;
            bool hasSelectedEventCells = false;
            if (hasSelectedChapter && TryGetSelectedGridCoordinates(selectedChapter, out List<ChapterGridCoordinate> coordinates))
            {
                hasSelectedEventCells = HasGridEventAtAnyCoordinate(selectedChapter.EventBindings, coordinates);
            }

            if (m_btnOpenCheckPopup != null)
            {
                m_btnOpenCheckPopup.gameObject.SetActive(hasSelectedChapter);
                m_btnOpenCheckPopup.interactable = hasSelectedChapter && !m_isDraggingChapter;
            }

            if (m_btnDeleteGridEvent != null)
            {
                m_btnDeleteGridEvent.gameObject.SetActive(hasSelectedChapter);
                m_btnDeleteGridEvent.interactable = hasSelectedChapter && !m_isDraggingChapter && hasSelectedEventCells;
            }

            if (m_imgDeleteGridEvent != null)
            {
                m_imgDeleteGridEvent.color = hasSelectedEventCells
                    ? new Color(0.72f, 0.29f, 0.24f, 0.98f)
                    : new Color(0.55f, 0.58f, 0.61f, 0.72f);
            }
        }

        private bool HasSelectedCellsForTerrainMarking()
        {
            ChapterListItemData selectedChapter = GetSelectedChapterData();
            return selectedChapter != null
                && ChapterGridCellCollectionUtility.HasCellsByMarkType(selectedChapter.GridCells, ChapterGridCellMarkType.Selected);
        }

        private bool IsMouseOverTerrainToolButtons()
        {
            return IsMouseOverRectTransform(m_btnDifficultTerrainRect)
                || IsMouseOverRectTransform(m_btnImpassableTerrainRect)
                || IsMouseOverRectTransform(m_btnClearTerrainRect);
        }

        private bool IsMouseOverCreatureInstanceActionButtons()
        {
            return IsMouseOverRectTransform(m_btnCreatureInstanceEditRect)
                || IsMouseOverRectTransform(m_btnCreatureInstanceToggleActiveRect)
                || IsMouseOverRectTransform(m_btnCreatureInstanceDeleteRect);
        }

        private bool IsMouseOverCreatureInstanceToken()
        {
            return TryGetMouseLocalPointInMapPreview(out Vector2 localPoint)
                && TryGetCreatureInstanceAtLocalPoint(localPoint, includeInactive: true, out _);
        }

        private void UpdateTerrainToolButtonState()
        {
            bool hasMap = m_mapPreviewTexture != null;
            bool hasSelectedCells = HasSelectedCellsForTerrainMarking();

            UpdateTerrainToolButtons(GetSelectedChapterData() != null, hasMap, hasSelectedCells);
        }

        private Button GetTerrainToolButton(TerrainToolButtonType buttonType)
        {
            switch (buttonType)
            {
                case TerrainToolButtonType.DifficultTerrain:
                    return m_btnDifficultTerrain;
                case TerrainToolButtonType.ImpassableTerrain:
                    return m_btnImpassableTerrain;
                case TerrainToolButtonType.ClearTerrain:
                    return m_btnClearTerrain;
                default:
                    return null!;
            }
        }

        private RectTransform GetTerrainToolButtonRect(TerrainToolButtonType buttonType)
        {
            switch (buttonType)
            {
                case TerrainToolButtonType.DifficultTerrain:
                    return m_btnDifficultTerrainRect;
                case TerrainToolButtonType.ImpassableTerrain:
                    return m_btnImpassableTerrainRect;
                case TerrainToolButtonType.ClearTerrain:
                    return m_btnClearTerrainRect;
                default:
                    return null!;
            }
        }

        private Image GetTerrainToolButtonImage(TerrainToolButtonType buttonType)
        {
            switch (buttonType)
            {
                case TerrainToolButtonType.DifficultTerrain:
                    return m_imgDifficultTerrain;
                case TerrainToolButtonType.ImpassableTerrain:
                    return m_imgImpassableTerrain;
                case TerrainToolButtonType.ClearTerrain:
                    return m_imgClearTerrain;
                default:
                    return null!;
            }
        }

        private TMP_Text GetTerrainToolButtonText(TerrainToolButtonType buttonType)
        {
            switch (buttonType)
            {
                case TerrainToolButtonType.DifficultTerrain:
                    return m_textDifficultTerrain;
                case TerrainToolButtonType.ImpassableTerrain:
                    return m_textImpassableTerrain;
                case TerrainToolButtonType.ClearTerrain:
                    return m_textClearTerrain;
                default:
                    return null!;
            }
        }

        private static float GetTerrainToolButtonBottomOffset(TerrainToolButtonType buttonType)
        {
            switch (buttonType)
            {
                case TerrainToolButtonType.DifficultTerrain:
                    return 20f;
                case TerrainToolButtonType.ImpassableTerrain:
                    return 70f;
                case TerrainToolButtonType.ClearTerrain:
                    return 120f;
                default:
                    return 20f;
            }
        }

        private static string GetTerrainToolButtonLabel(TerrainToolButtonType buttonType, bool hasSelectedCells)
        {
            switch (buttonType)
            {
                case TerrainToolButtonType.DifficultTerrain:
                    return hasSelectedCells ? "Mark As Difficult Terrain" : "Difficult Terrain";
                case TerrainToolButtonType.ImpassableTerrain:
                    return hasSelectedCells ? "标记为不可通过" : "不可通过地形";
                case TerrainToolButtonType.ClearTerrain:
                    return hasSelectedCells ? "Clear Selected Terrain" : "Clear Terrain";
                default:
                    return string.Empty;
            }
        }

        private static Color GetTerrainToolButtonEnabledColor(TerrainToolButtonType buttonType)
        {
            switch (buttonType)
            {
                case TerrainToolButtonType.DifficultTerrain:
                    return new Color(0.69f, 0.42f, 0.16f, 0.96f);
                case TerrainToolButtonType.ImpassableTerrain:
                    return new Color(0.38f, 0.41f, 0.44f, 0.96f);
                case TerrainToolButtonType.ClearTerrain:
                    return new Color(0.46f, 0.22f, 0.22f, 0.96f);
                default:
                    return Color.white;
            }
        }

        private static Color GetTerrainToolButtonDisabledColor(TerrainToolButtonType buttonType)
        {
            switch (buttonType)
            {
                case TerrainToolButtonType.DifficultTerrain:
                    return new Color(0.45f, 0.38f, 0.3f, 0.88f);
                case TerrainToolButtonType.ImpassableTerrain:
                    return new Color(0.33f, 0.35f, 0.38f, 0.88f);
                case TerrainToolButtonType.ClearTerrain:
                    return new Color(0.38f, 0.32f, 0.32f, 0.88f);
                default:
                    return Color.white;
            }
        }

        private bool IsMouseOverRectTransform(RectTransform rectTransform)
        {
            if (rectTransform == null || !rectTransform.gameObject.activeInHierarchy)
            {
                return false;
            }

            return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, GetMapPreviewEventCamera());
        }

        private bool TryGetMouseLocalPointInMapPreview(out Vector2 localPoint)
        {
            return TryGetLocalPointInMapPreview(Input.mousePosition, out localPoint);
        }

        private bool TryGetLocalPointInMapPreview(Vector2 screenPoint, out Vector2 localPoint)
        {
            if (m_rectMapPreview == null)
            {
                localPoint = Vector2.zero;
                return false;
            }

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(m_rectMapPreview, screenPoint, GetMapPreviewEventCamera(), out localPoint);
        }

        private bool TryGetLocalPointInCanvas(Vector2 screenPoint, out Vector2 localPoint)
        {
            RectTransform canvasRect = m_canvas != null ? m_canvas.transform as RectTransform : null;
            if (canvasRect == null)
            {
                localPoint = Vector2.zero;
                return false;
            }

            Camera eventCamera = m_canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : m_canvas.worldCamera;
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, eventCamera, out localPoint);
        }

        private void BeginCreatureDeploymentDrag(int actualCardIndex, Vector2 screenPoint)
        {
            if (actualCardIndex < 0 || actualCardIndex >= m_creatureAllCards.Count)
            {
                return;
            }

            ChapterListItemData selectedChapter = GetSelectedChapterData();
            ChapterCreatureData creatureTemplate = m_creatureAllCards[actualCardIndex].Source;
            if (selectedChapter == null || creatureTemplate == null)
            {
                return;
            }

            m_isDraggingCreatureDeployment = true;
            m_draggingCreatureTemplate = ChapterCreatureDataStructureUtility.CloneCreatureData(creatureTemplate);
            CancelPendingGridCellSelection();
            EnsureCreatureDragPreviewWidget();
            UpdateCreatureDeploymentDrag(screenPoint);
        }

        private void UpdateCreatureDeploymentDrag(Vector2 screenPoint)
        {
            if (!m_isDraggingCreatureDeployment || m_draggingCreatureTemplate == null)
            {
                HideCreaturePlacementPreview();
                return;
            }

            EnsureCreatureDragPreviewWidget();
            if (m_creatureDragPreviewWidget == null || !TryGetLocalPointInCanvas(screenPoint, out Vector2 localPoint))
            {
                HideCreaturePlacementPreview();
                return;
            }

            Vector2 previewSize = GetCreatureDragPreviewSize(screenPoint);
            m_creatureDragPreviewWidget.Bind(m_draggingCreatureTemplate, localPoint, previewSize, true, false);
            UpdateCreaturePlacementPreview(screenPoint, m_draggingCreatureTemplate, 1f);
        }

        private void EndCreatureDeploymentDrag(Vector2 screenPoint)
        {
            if (!m_isDraggingCreatureDeployment)
            {
                return;
            }

            bool deployed = TryDeployDraggedCreatureToMap(screenPoint);
            CancelCreatureDeploymentDrag();
            if (deployed)
            {
                bool saved = SaveChapterEditorState();
                ShowSaveFeedbackAsync(saved ? "Creature deployed." : "Creature deployed, but save failed.", saved).Forget();
            }
        }

        private bool TryDeployDraggedCreatureToMap(Vector2 screenPoint)
        {
            ChapterListItemData selectedChapter = GetSelectedChapterData();
            if (selectedChapter == null || m_draggingCreatureTemplate == null)
            {
                return false;
            }

            if (IsScreenPointOverMapControlButton(screenPoint))
            {
                return false;
            }

            if (!TryGetLocalPointInMapPreview(screenPoint, out Vector2 localPoint))
            {
                return false;
            }

            if (!TryGetGridCellCoordinateFromLocalPoint(localPoint, out ChapterGridCoordinate gridCoordinate))
            {
                return false;
            }

            selectedChapter.CreatureInstances ??= new List<ChapterCreatureInstanceData>();
            selectedChapter.CreatureInstances.Add(CreateCreatureInstanceFromTemplate(m_draggingCreatureTemplate, gridCoordinate));
            RefreshGridSelectionHighlights();
            return true;
        }

        private void CancelCreatureDeploymentDrag()
        {
            m_isDraggingCreatureDeployment = false;
            m_draggingCreatureTemplate = null;
            HideCreaturePlacementPreview();
            if (m_creatureDragPreviewWidget != null)
            {
                m_creatureDragPreviewWidget.SetVisible(false);
            }
        }

        private void EnsureCreatureDragPreviewWidget()
        {
            if (m_creatureDragPreviewWidget != null || m_canvas == null)
            {
                return;
            }

            GameObject previewObject = new GameObject("CreatureDeploymentDragPreview", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            previewObject.transform.SetParent(m_canvas.transform, false);
            m_creatureDragPreviewWidget = new ChapterCreatureMapTokenWidget(previewObject, m_tmpCreatureBrowserTitle);
            m_creatureDragPreviewWidget.SetVisible(false);
        }

        private bool IsScreenPointOverMapControlButton(Vector2 screenPoint)
        {
            return IsScreenPointOverRectTransform(m_btnOpenCheckPopupRect, screenPoint)
                || IsScreenPointOverRectTransform(m_btnDeleteGridEventRect, screenPoint)
                || IsScreenPointOverRectTransform(m_btnUploadMapRect, screenPoint)
                || IsScreenPointOverRectTransform(m_btnMapZoomToggleRect, screenPoint)
                || IsScreenPointOverRectTransform(m_btnGridZoomToggleRect, screenPoint)
                || IsScreenPointOverRectTransform(m_btnSaveChapterStateRect, screenPoint)
                || IsScreenPointOverRectTransform(m_btnPreviewModuleRect, screenPoint)
                || IsScreenPointOverRectTransform(m_btnDifficultTerrainRect, screenPoint)
                || IsScreenPointOverRectTransform(m_btnImpassableTerrainRect, screenPoint)
                || IsScreenPointOverRectTransform(m_btnClearTerrainRect, screenPoint)
                || IsScreenPointOverRectTransform(m_btnCreatureInstanceEditRect, screenPoint)
                || IsScreenPointOverRectTransform(m_btnCreatureInstanceToggleActiveRect, screenPoint)
                || IsScreenPointOverRectTransform(m_btnCreatureInstanceDeleteRect, screenPoint)
                || IsScreenPointOverCreatureInstanceToken(screenPoint);
        }

        private bool IsScreenPointOverRectTransform(RectTransform rectTransform, Vector2 screenPoint)
        {
            if (rectTransform == null || !rectTransform.gameObject.activeInHierarchy)
            {
                return false;
            }

            return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPoint, GetMapPreviewEventCamera());
        }

        private bool IsScreenPointOverCreatureInstanceToken(Vector2 screenPoint)
        {
            return TryGetLocalPointInMapPreview(screenPoint, out Vector2 localPoint)
                && TryGetCreatureInstanceAtLocalPoint(localPoint, includeInactive: true, out _);
        }

        private static ChapterCreatureInstanceData CreateCreatureInstanceFromTemplate(ChapterCreatureData creatureTemplate, ChapterGridCoordinate gridCoordinate)
        {
            ChapterCreatureData normalizedTemplate = ChapterCreatureDataStructureUtility.CloneCreatureData(creatureTemplate);
            return ChapterCreatureDataStructureUtility.NormalizeCreatureInstanceData(new ChapterCreatureInstanceData
            {
                SourceCreatureId = normalizedTemplate?.CreatureId ?? string.Empty,
                IsActive = true,
                Placement = new ChapterCreatureInstancePlacementData
                {
                    AnchorCell = gridCoordinate,
                    PreviewScale = 1f,
                    SnapToGrid = true,
                },
                RuntimeSheet = normalizedTemplate,
                DmNote = string.Empty,
            });
        }

        private static Rect GetCreatureTokenRect(ChapterMapGridMetrics metrics, ChapterCreatureInstanceData creatureInstance)
        {
            if (creatureInstance == null)
            {
                return Rect.zero;
            }

            ChapterCreatureInstancePlacementData placement = creatureInstance.Placement ?? new ChapterCreatureInstancePlacementData();
            return GetCreatureTokenRect(metrics, placement.AnchorCell, creatureInstance.RuntimeSheet, placement.PreviewScale);
        }

        private static Rect GetCreatureTokenRect(ChapterMapGridMetrics metrics, ChapterGridCoordinate anchorCell, ChapterCreatureData creature, float previewScale)
        {
            Rect anchorCellRect = ChapterMapGridUtility.GetLogicalCellRect(metrics, anchorCell);
            Vector2 tokenSize = GetCreatureTokenSize(metrics, creature, previewScale);
            return new Rect(anchorCellRect.xMin, anchorCellRect.yMin, tokenSize.x, tokenSize.y);
        }

        private static Vector2 GetCreatureTokenSize(ChapterMapGridMetrics metrics, ChapterCreatureData creature, float previewScale)
        {
            int cellSpan = Mathf.Max(1, ChapterCreatureDataStructureUtility.GetCreatureFootprintCellSpan(creature));
            float scale = Mathf.Max(0.4f, previewScale);
            return new Vector2(
                Mathf.Max(18f, metrics.CellWidth * cellSpan * scale),
                Mathf.Max(18f, metrics.CellHeight * cellSpan * scale));
        }

        private Vector2 GetCreatureDragPreviewSize(Vector2 screenPoint)
        {
            if (TryGetLocalPointInMapPreview(screenPoint, out _)
                && TryGetCurrentGridMetrics(out ChapterMapGridMetrics metrics))
            {
                return GetCreatureTokenSize(metrics, m_draggingCreatureTemplate, 1f);
            }

            int cellSpan = Mathf.Max(1, ChapterCreatureDataStructureUtility.GetCreatureFootprintCellSpan(m_draggingCreatureTemplate));
            float baseSize = 56f * Mathf.Clamp(cellSpan, 1, 4);
            return new Vector2(baseSize, baseSize);
        }

        private void UpdateDraggingCreaturePlacementPreview()
        {
            if (m_isDraggingCreatureDeployment)
            {
                UpdateCreaturePlacementPreview(Input.mousePosition, m_draggingCreatureTemplate, 1f);
                return;
            }

            if (!m_isDraggingCreatureInstance)
            {
                HideCreaturePlacementPreview();
                return;
            }

            ChapterListItemData selectedChapter = GetSelectedChapterData();
            ChapterCreatureInstanceData creatureInstance = FindCreatureInstanceById(selectedChapter?.CreatureInstances, m_draggingCreatureInstanceId);
            UpdateCreaturePlacementPreview(Input.mousePosition, creatureInstance?.RuntimeSheet, creatureInstance?.Placement?.PreviewScale ?? 1f);
        }

        private void UpdateCreaturePlacementPreview(Vector2 screenPoint, ChapterCreatureData creature, float previewScale)
        {
            if (creature == null || IsScreenPointOverMapControlButton(screenPoint))
            {
                HideCreaturePlacementPreview();
                return;
            }

            if (!TryGetLocalPointInMapPreview(screenPoint, out Vector2 localPoint)
                || !TryGetGridCellCoordinateFromLocalPoint(localPoint, out ChapterGridCoordinate gridCoordinate))
            {
                HideCreaturePlacementPreview();
                return;
            }

            if (!TryGetCurrentGridMetrics(out ChapterMapGridMetrics metrics))
            {
                HideCreaturePlacementPreview();
                return;
            }

            ShowCreaturePlacementPreview(metrics, gridCoordinate, creature, previewScale);
        }

        private void ShowCreaturePlacementPreview(ChapterMapGridMetrics metrics, ChapterGridCoordinate gridCoordinate, ChapterCreatureData creature, float previewScale)
        {
            EnsureCreaturePlacementPreview();
            if (m_rectCreaturePlacementPreview == null || m_imgCreaturePlacementPreview == null)
            {
                return;
            }

            Rect previewRect = GetCreatureTokenRect(metrics, gridCoordinate, creature, previewScale);
            m_rectCreaturePlacementPreview.anchorMin = new Vector2(0.5f, 0.5f);
            m_rectCreaturePlacementPreview.anchorMax = new Vector2(0.5f, 0.5f);
            m_rectCreaturePlacementPreview.pivot = new Vector2(0.5f, 0.5f);
            m_rectCreaturePlacementPreview.anchoredPosition = previewRect.center;
            m_rectCreaturePlacementPreview.sizeDelta = previewRect.size;
            m_rectCreaturePlacementPreview.localScale = Vector3.one;

            Color accentColor = creature != null ? creature.AccentColor : new Color(0.12f, 0.55f, 0.92f, 1f);
            m_imgCreaturePlacementPreview.color = new Color(
                Mathf.Clamp01(accentColor.r * 0.9f + 0.08f),
                Mathf.Clamp01(accentColor.g * 0.9f + 0.08f),
                Mathf.Clamp01(accentColor.b * 0.9f + 0.08f),
                0.28f);

            m_rectCreaturePlacementPreview.gameObject.SetActive(true);
            m_rectCreaturePlacementPreview.SetAsLastSibling();
        }

        private void HideCreaturePlacementPreview()
        {
            if (m_rectCreaturePlacementPreview != null)
            {
                m_rectCreaturePlacementPreview.gameObject.SetActive(false);
            }
        }

        private void EnsureCreaturePlacementPreview()
        {
            if (m_rectCreaturePlacementPreview != null || m_rectMapGridSelectionOverlay == null)
            {
                return;
            }

            GameObject previewObject = new GameObject("CreaturePlacementPreview", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            previewObject.transform.SetParent(m_rectMapGridSelectionOverlay, false);
            m_rectCreaturePlacementPreview = previewObject.GetComponent<RectTransform>();
            m_imgCreaturePlacementPreview = previewObject.GetComponent<Image>();
            m_imgCreaturePlacementPreview.raycastTarget = false;
            m_imgCreaturePlacementPreview.color = new Color(0.12f, 0.55f, 0.92f, 0.28f);
            m_rectCreaturePlacementPreview.gameObject.SetActive(false);
        }

        private static float GetZoomScrollStep(float normalStep)
        {
            return IsFineZoomModifierPressed() ? FineZoomScrollStep : normalStep;
        }

        private static float GetAppliedZoomScrollDelta(float scrollDelta, float normalStep)
        {
            return NormalizeScrollDelta(scrollDelta) * GetZoomScrollStep(normalStep);
        }

        private static float NormalizeScrollDelta(float scrollDelta)
        {
            return Mathf.Clamp(scrollDelta, -1f, 1f);
        }

        private static bool IsFineZoomModifierPressed()
        {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }

        private Camera GetMapPreviewEventCamera()
        {
            if (m_canvas != null && m_canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                return m_canvas.worldCamera;
            }

            return null;
        }

        private void UpdateUploadMapButtonLayout(bool hasMap)
        {
            if (m_btnUploadMapRect == null)
            {
                return;
            }

            if (hasMap)
            {
                m_btnUploadMapRect.anchorMin = new Vector2(1f, 1f);
                m_btnUploadMapRect.anchorMax = new Vector2(1f, 1f);
                m_btnUploadMapRect.pivot = new Vector2(1f, 1f);
                m_btnUploadMapRect.anchoredPosition = new Vector2(-20f, -20f);
                m_btnUploadMapRect.sizeDelta = new Vector2(156f, 42f);
                return;
            }

            m_btnUploadMapRect.anchorMin = new Vector2(0.5f, 0.5f);
            m_btnUploadMapRect.anchorMax = new Vector2(0.5f, 0.5f);
            m_btnUploadMapRect.pivot = new Vector2(0.5f, 0.5f);
            m_btnUploadMapRect.anchoredPosition = new Vector2(0f, 24f);
            m_btnUploadMapRect.sizeDelta = new Vector2(196f, 52f);
        }

        private static string GetChapterMapStorageDirectory()
        {
            return Path.Combine(Application.persistentDataPath, "ChapterMaps");
        }

        private static string GetChapterEditorSaveFilePath()
        {
            return ChapterEditorPersistenceService.GetSaveFilePath(ChapterEditorSaveFileName);
        }

        private void SyncChapterMapGridStateToData(ChapterListItemData chapter)
        {
            if (chapter == null)
            {
                return;
            }

            if (m_isMapPreviewLoading)
            {
                return;
            }

            chapter.MapGridState ??= new ChapterMapGridStateData();
            chapter.MapGridState.IsMapZoomEnabled = m_isMapZoomEnabled;
            chapter.MapGridState.IsGridZoomEnabled = m_isGridZoomEnabled;
            chapter.MapGridState.MapZoomScale = Mathf.Clamp(m_mapPreviewZoomScale, MapZoomMinScale, MapZoomMaxScale);
            chapter.MapGridState.MapPanOffset = m_mapPreviewPanOffset;
            chapter.MapGridState.GridZoomScale = Mathf.Clamp(m_mapGridZoomScale, GridZoomMinScale, GridZoomMaxScale);
            chapter.MapGridState.GridPanOffset = m_mapGridPanOffset;
            chapter.MapGridState.IsLocked = m_isMapGridLocked;
            chapter.MapGridState.LockedMapZoomReference = Mathf.Max(0.0001f, m_lockedMapZoomReference);
            chapter.MapGridState.LockedGridToMapZoomRatio = Mathf.Max(0.0001f, m_lockedGridToMapZoomRatio);
            chapter.MapGridState.LockedGridToMapPanDelta = m_lockedGridToMapPanDelta;
        }

        private void ApplyChapterMapGridState(ChapterListItemData chapter)
        {
            if (chapter == null)
            {
                ResetMapZoomState();
                ResetGridZoomState();
                ResetMapGridLockState();
                return;
            }

            ChapterMapGridStateData mapGridState = chapter.MapGridState ?? new ChapterMapGridStateData();
            m_isMapZoomEnabled = mapGridState.IsMapZoomEnabled;
            m_isGridZoomEnabled = mapGridState.IsGridZoomEnabled;
            m_mapPreviewZoomScale = Mathf.Clamp(mapGridState.MapZoomScale > 0f ? mapGridState.MapZoomScale : 1f, MapZoomMinScale, MapZoomMaxScale);
            m_mapPreviewPanOffset = mapGridState.MapPanOffset;
            m_mapGridZoomScale = Mathf.Clamp(mapGridState.GridZoomScale > 0f ? mapGridState.GridZoomScale : 1f, GridZoomMinScale, GridZoomMaxScale);
            m_mapGridPanOffset = mapGridState.GridPanOffset;
            m_isMapGridLocked = false;
            m_lockedMapZoomReference = Mathf.Max(0.0001f, mapGridState.LockedMapZoomReference > 0f ? mapGridState.LockedMapZoomReference : m_mapPreviewZoomScale);
            m_lockedGridToMapZoomRatio = Mathf.Max(0.0001f, mapGridState.LockedGridToMapZoomRatio > 0f ? mapGridState.LockedGridToMapZoomRatio : 1f);
            m_lockedGridToMapPanDelta = mapGridState.LockedGridToMapPanDelta;
        }

        private bool SaveChapterEditorState()
        {
            try
            {
                string filePath = GetChapterEditorSaveFilePath();
                ChapterEditorSaveData saveData = BuildChapterEditorSaveData();
                ChapterEditorPersistenceService.Save(filePath, saveData);
                return true;
            }
            catch (Exception exception)
            {
                Log.Error($"保存章节编辑数据失败: {exception.Message}");
                return false;
            }
        }

        private void LoadChapterEditorState()
        {
            string filePath = GetChapterEditorSaveFilePath();
            if (!File.Exists(filePath))
            {
                return;
            }

            try
            {
                ChapterEditorSaveData saveData = ChapterEditorPersistenceService.Load(filePath);
                if (saveData == null)
                {
                    return;
                }

                m_chapterItems.Clear();
                m_chapterItems.AddRange(ChapterEditorPersistenceService.BuildRuntimeChapters(saveData.Chapters));

                m_selectedChapterId = saveData.SelectedChapterId;
                int maxChapterId = 0;
                for (int index = 0; index < m_chapterItems.Count; index++)
                {
                    maxChapterId = Mathf.Max(maxChapterId, m_chapterItems[index].Id);
                }

                m_nextChapterId = Mathf.Max(saveData.NextChapterId, maxChapterId + 1, 1);
                if (GetChapterIndexById(m_selectedChapterId) < 0)
                {
                    m_selectedChapterId = m_chapterItems.Count > 0 ? m_chapterItems[0].Id : -1;
                }
            }
            catch (Exception exception)
            {
                m_chapterItems.Clear();
                m_selectedChapterId = -1;
                m_nextChapterId = 1;
                Log.Error($"加载章节编辑数据失败: {exception.Message}");
            }
        }

        private ChapterEditorSaveData BuildChapterEditorSaveData()
        {
            SyncChapterInputsToData();
            return ChapterEditorPersistenceService.BuildSaveData(m_chapterItems, m_selectedChapterId, m_nextChapterId);
        }

        private static void TryDeleteManagedMapFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return;
            }

            string storageDirectory = GetChapterMapStorageDirectory();
            string normalizedFilePath = Path.GetFullPath(filePath);
            string normalizedStorageDirectory = Path.GetFullPath(storageDirectory);
            if (!normalizedFilePath.StartsWith(normalizedStorageDirectory, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                File.Delete(normalizedFilePath);
            }
            catch (Exception exception)
            {
                Log.Warning($"删除旧章节地图失败: {exception.Message}");
            }
        }

        private ChapterListItemData GetSelectedChapterData()
        {
            int selectedChapterIndex = GetSelectedChapterIndex();
            if (selectedChapterIndex < 0 || selectedChapterIndex >= m_chapterItems.Count)
            {
                return null!;
            }

            return m_chapterItems[selectedChapterIndex];
        }

    }

    internal sealed class ChapterEventPopupRequest
    {
        public int ChapterId { get; set; }

        public string ChapterName { get; set; } = string.Empty;

        public ChapterGridCoordinate GridCoordinate { get; set; } = ChapterGridCoordinate.Zero;

        public List<ChapterGridCoordinate> GridCoordinates { get; set; } = new List<ChapterGridCoordinate>();

        public List<ChapterCreatureInstanceData> CreatureInstances { get; set; } = new List<ChapterCreatureInstanceData>();

        public ChapterGridEventData ExistingEventData { get; set; }

        public Action<ChapterGridEventData> OnConfirm { get; set; }
    }

    [Window(UILayer.Top, location : "ChapterEventPopupUI", fullScreen : false)]
    public sealed class ChapterEventPopupUI : UIWindow
    {
        private const string PopupPanelPathPrefix = "Panel/";

        private enum ChapterEventTriggerMode
        {
            Automatic,
            DmManual,
        }

        private enum ChapterEventTriggerType
        {
            DmManual = 0,
            EnterBindingArea = 1,
            InteractWithSceneObject = 2,
            AfterPrerequisiteEvent = 3,
            ChapterEnter = 4,
        }

        private enum ChapterEventEffectType
        {
            Check = 0,
            NarrativePrompt = 1,
            DialogueInteractionPrompt = 2,
            ActivateCreatureInstance = 3,
            StartBattle = 4,
            PlayerInitialPosition = 5,
        }

        private enum ChapterEffectCreaturePlacementMode
        {
            UseSavedInstancePosition = 0,
            UseBindingCoordinate = 1,
            ManualOverride = 2,
        }

        private enum ChapterCheckTargetMode
        {
            Ability,
            Skill,
            Tool,
            Contested,
            Passive,
            SavingThrow,
        }

        private enum ChapterCheckResolutionMode
        {
            RollDice,
            DmDirect,
        }

        private static readonly string[] TriggerTypeLabels =
        {
            "\u7531 DM \u624B\u52A8\u89E6\u53D1",
            "\u8FDB\u5165\u7ED1\u5B9A\u533A\u57DF\u89E6\u53D1",
            "\u4E0E\u573A\u666F\u5BF9\u8C61\u4EA4\u4E92\u89E6\u53D1",
            "\u524D\u7F6E\u4E8B\u4EF6\u5B8C\u6210\u540E\u89E6\u53D1",
            "进入章节时触发",
        };

        private static readonly string[] EffectTypeLabels =
        {
            "\u68C0\u5B9A",
            "\u5267\u60C5\u63D0\u793A",
            "\u5BF9\u8BDD/\u4EA4\u4E92\u63D0\u793A",
            "\u6FC0\u6D3B\u751F\u7269\u5B9E\u4F8B",
            "\u5F00\u59CB\u6218\u6597",
            "玩家初始位置",
        };

        private static readonly string[] EffectCreaturePlacementModeLabels =
        {
            "\u4F7F\u7528\u5B9E\u4F8B\u5DF2\u4FDD\u5B58\u4F4D\u7F6E",
            "\u4F7F\u7528\u5F53\u524D\u7ED1\u5B9A\u683C\u5B50",
            "\u624B\u52A8\u6307\u5B9A",
        };

        private static readonly string[] CheckTargetModeLabels =
        {
            "属性检定",
            "技能检定",
            "工具检定",
            "对抗检定",
            "被动检定",
            "豁免检定",
        };

        private static readonly string[] CheckResolutionModeLabels =
        {
            "掷骰判定",
            "DM 直接判定",
        };

        private static readonly string[] SkillCheckLabels =
        {
            "运动",
            "体操",
            "巧手",
            "隐匿",
            "奥秘",
            "历史",
            "调查",
            "自然",
            "宗教",
            "驯兽",
            "洞悉",
            "医药",
            "察觉",
            "求生",
            "欺瞒",
            "威吓",
            "表演",
            "说服",
        };

        private static readonly string[] SkillCheckInputNodeNames =
        {
            "m_rectSkillCheckSection/m_rectSkillCheckScrollView/Viewport/Content/m_tmpInputSkillCheckAthletics",
            "m_rectSkillCheckSection/m_rectSkillCheckScrollView/Viewport/Content/m_tmpInputSkillCheckAcrobatics",
            "m_rectSkillCheckSection/m_rectSkillCheckScrollView/Viewport/Content/m_tmpInputSkillCheckSleightOfHand",
            "m_rectSkillCheckSection/m_rectSkillCheckScrollView/Viewport/Content/m_tmpInputSkillCheckStealth",
            "m_rectSkillCheckSection/m_rectSkillCheckScrollView/Viewport/Content/m_tmpInputSkillCheckArcana",
            "m_rectSkillCheckSection/m_rectSkillCheckScrollView/Viewport/Content/m_tmpInputSkillCheckHistory",
            "m_rectSkillCheckSection/m_rectSkillCheckScrollView/Viewport/Content/m_tmpInputSkillCheckInvestigation",
            "m_rectSkillCheckSection/m_rectSkillCheckScrollView/Viewport/Content/m_tmpInputSkillCheckNature",
            "m_rectSkillCheckSection/m_rectSkillCheckScrollView/Viewport/Content/m_tmpInputSkillCheckReligion",
            "m_rectSkillCheckSection/m_rectSkillCheckScrollView/Viewport/Content/m_tmpInputSkillCheckAnimalHandling",
            "m_rectSkillCheckSection/m_rectSkillCheckScrollView/Viewport/Content/m_tmpInputSkillCheckInsight",
            "m_rectSkillCheckSection/m_rectSkillCheckScrollView/Viewport/Content/m_tmpInputSkillCheckMedicine",
            "m_rectSkillCheckSection/m_rectSkillCheckScrollView/Viewport/Content/m_tmpInputSkillCheckPerception",
            "m_rectSkillCheckSection/m_rectSkillCheckScrollView/Viewport/Content/m_tmpInputSkillCheckSurvival",
            "m_rectSkillCheckSection/m_rectSkillCheckScrollView/Viewport/Content/m_tmpInputSkillCheckDeception",
            "m_rectSkillCheckSection/m_rectSkillCheckScrollView/Viewport/Content/m_tmpInputSkillCheckIntimidation",
            "m_rectSkillCheckSection/m_rectSkillCheckScrollView/Viewport/Content/m_tmpInputSkillCheckPerformance",
            "m_rectSkillCheckSection/m_rectSkillCheckScrollView/Viewport/Content/m_tmpInputSkillCheckPersuasion",
        };

        private const float EventPopupConfirmSpacing = 24f;
        private const float EventPopupEventIdLayoutShift = 70f;
        private const float EventPopupEventIdLabelTop = 186f;
        private const float EventPopupEventIdInputTop = 210f;
        private const float EventPopupEventIdInputHeight = 38f;

        private TextMeshProUGUI m_tmpTitle = null!;
        private Button m_btnClose = null!;
        private TMP_Dropdown m_tmpDropdownEffectType = null!;
        private TMP_Dropdown m_tmpDropdownTriggerType = null!;
        private TMP_Dropdown m_tmpDropdownEffectCreaturePlacementMode = null!;
        private TMP_Dropdown m_tmpDropdownCheckTargetMode = null!;
        private TMP_Dropdown m_tmpDropdownCheckResolutionMode = null!;
        private Button m_btnConfirm = null!;
        private RectTransform m_rectPanel = null!;
        private TMP_Text m_tmpBindingSummary = null!;
        private TMP_Text m_tmpEventIdLabel = null!;
        private TMP_InputField m_tmpInputEventId = null!;
        private Toggle m_toggleEventEnabled = null!;
        private Toggle m_toggleEventOneShot = null!;
        private RectTransform m_rectTriggerManualSection = null!;
        private RectTransform m_rectTriggerAreaSection = null!;
        private RectTransform m_rectTriggerInteractionSection = null!;
        private RectTransform m_rectTriggerPrerequisiteSection = null!;
        private RectTransform m_rectEffectNarrativeSection = null!;
        private RectTransform m_rectEffectDialogueSection = null!;
        private RectTransform m_rectEffectCombatantActivationSection = null!;
        private RectTransform m_rectEffectBattleSection = null!;
        private RectTransform m_rectAbilityCheckSection = null!;
        private RectTransform m_rectSkillCheckSection = null!;
        private Toggle m_toggleTriggerAreaFirstEnterOnly = null!;
        private Toggle m_toggleTriggerAreaShareBinding = null!;
        private TMP_InputField m_tmpInputTriggerInteractionTarget = null!;
        private Toggle m_toggleTriggerInteractionRequireConfirm = null!;
        private TMP_InputField m_tmpInputTriggerPrerequisiteEventId = null!;
        private TMP_InputField m_tmpInputTriggerDelayDescription = null!;
        private Toggle m_toggleEffectNarrativeDmOnly = null!;
        private TMP_InputField m_tmpInputEffectNarrativeText = null!;
        private TMP_InputField m_tmpInputEffectDialogueTarget = null!;
        private TMP_InputField m_tmpInputEffectDialogueSummary = null!;
        private TMP_InputField m_tmpInputEffectDialoguePrompt = null!;
        private TMP_Dropdown m_tmpDropdownEffectCreatureInstanceId = null!;
        private Toggle m_toggleEffectCreatureActivate = null!;
        private TMP_InputField m_tmpInputEffectBattleReference = null!;
        private Toggle m_toggleEffectBattleIncludeActiveCreatures = null!;
        private TMP_InputField m_tmpInputEffectBattleDescription = null!;
        private TMP_InputField m_tmpInputAbilityStrength = null!;
        private TMP_InputField m_tmpInputAbilityDexterity = null!;
        private TMP_InputField m_tmpInputAbilityConstitution = null!;
        private TMP_InputField m_tmpInputAbilityIntelligence = null!;
        private TMP_InputField m_tmpInputAbilityWisdom = null!;
        private TMP_InputField m_tmpInputAbilityCharisma = null!;
        private TMP_InputField m_tmpInputEventTitle = null!;
        private TMP_InputField m_tmpInputTriggerDescription = null!;
        private TMP_InputField m_tmpInputSuccessResult = null!;
        private TMP_InputField m_tmpInputFailureResult = null!;
        private TMP_InputField m_tmpInputDmNote = null!;
        private ChapterEventPopupRequest m_request = null!;
        private string m_existingEventId = string.Empty;
        private ChapterEventTriggerType m_triggerType = ChapterEventTriggerType.DmManual;
        private ChapterEventEffectType m_effectType = ChapterEventEffectType.Check;
        private ChapterEffectCreaturePlacementMode m_effectCreaturePlacementMode = ChapterEffectCreaturePlacementMode.UseSavedInstancePosition;
        private ChapterCheckTargetMode m_checkTargetMode = ChapterCheckTargetMode.Ability;
        private ChapterCheckResolutionMode m_checkResolutionMode = ChapterCheckResolutionMode.RollDice;
        private readonly Dictionary<string, TMP_InputField> m_skillCheckInputs = new Dictionary<string, TMP_InputField>(StringComparer.Ordinal);
        private readonly List<string> m_effectCreatureInstanceOptionIds = new List<string>();
        private bool m_isEventPopupLayoutInitialized;
        private float m_eventPopupSectionTop;
        private float m_eventPopupGapAfterTriggerSection;
        private float m_eventPopupGapAfterCheckControls;
        private float m_eventPopupGapAfterEffectSection;
        private float m_eventPopupGapBetweenInputFields;
        private float m_eventPopupConfirmTopInset;
        private float m_eventPopupMinimumPanelHeight;
        private float m_triggerDelayDescriptionMinimumHeight;
        private float m_triggerPrerequisiteSectionMinimumHeight;
        private float m_triggerPrerequisiteSectionBottomPadding;

        protected override void OnCreate()
        {
            PopupWindowPresentationHelper.Configure(this);
        }

        protected override void ScriptGenerator()
        {
            m_tmpTitle = BindRequiredComponent<TextMeshProUGUI>("m_tmpTitle");
            m_btnClose = BindRequiredComponent<Button>("m_btnClose");
            m_tmpDropdownEffectType = BindRequiredComponent<TMP_Dropdown>("m_tmpDropdownEffectType");
            m_tmpDropdownTriggerType = BindRequiredComponent<TMP_Dropdown>("m_tmpDropdownTriggerType");
            m_tmpDropdownEffectCreaturePlacementMode = BindRequiredComponent<TMP_Dropdown>("m_rectEffectCombatantActivationSection/m_tmpDropdownEffectCreaturePlacementMode");
            m_tmpDropdownCheckTargetMode = BindRequiredComponent<TMP_Dropdown>("m_tmpDropdownCheckTargetMode");
            m_tmpDropdownCheckResolutionMode = BindRequiredComponent<TMP_Dropdown>("m_tmpDropdownCheckResolutionMode");
            m_btnConfirm = BindRequiredComponent<Button>("m_btnConfirm");
            m_rectPanel = BindRequiredComponent<RectTransform>("Panel");
            m_tmpBindingSummary = BindRequiredComponent<TMP_Text>("m_tmpBindingSummary");
            m_toggleEventEnabled = BindRequiredComponent<Toggle>("m_toggleEventEnabled");
            m_toggleEventOneShot = BindRequiredComponent<Toggle>("m_toggleEventOneShot");
            m_rectTriggerManualSection = BindRequiredComponent<RectTransform>("m_rectTriggerManualSection");
            m_rectTriggerAreaSection = BindRequiredComponent<RectTransform>("m_rectTriggerAreaSection");
            m_rectTriggerInteractionSection = BindRequiredComponent<RectTransform>("m_rectTriggerInteractionSection");
            m_rectTriggerPrerequisiteSection = BindRequiredComponent<RectTransform>("m_rectTriggerPrerequisiteSection");
            m_rectEffectNarrativeSection = BindRequiredComponent<RectTransform>("m_rectEffectNarrativeSection");
            m_rectEffectDialogueSection = BindRequiredComponent<RectTransform>("m_rectEffectDialogueSection");
            m_rectEffectCombatantActivationSection = BindRequiredComponent<RectTransform>("m_rectEffectCombatantActivationSection");
            m_rectEffectBattleSection = BindRequiredComponent<RectTransform>("m_rectEffectBattleSection");
            m_rectAbilityCheckSection = BindRequiredComponent<RectTransform>("m_rectAbilityCheckSection");
            m_rectSkillCheckSection = BindRequiredComponent<RectTransform>("m_rectSkillCheckSection");
            m_toggleTriggerAreaFirstEnterOnly = BindRequiredComponent<Toggle>("m_rectTriggerAreaSection/m_toggleTriggerAreaFirstEnterOnly");
            m_toggleTriggerAreaShareBinding = BindRequiredComponent<Toggle>("m_rectTriggerAreaSection/m_toggleTriggerAreaShareBinding");
            m_tmpInputTriggerInteractionTarget = BindRequiredComponent<TMP_InputField>("m_rectTriggerInteractionSection/m_tmpInputTriggerInteractionTarget");
            m_toggleTriggerInteractionRequireConfirm = BindRequiredComponent<Toggle>("m_rectTriggerInteractionSection/m_toggleTriggerInteractionRequireConfirm");
            m_tmpInputTriggerPrerequisiteEventId = BindRequiredComponent<TMP_InputField>("m_rectTriggerPrerequisiteSection/m_tmpInputTriggerPrerequisiteEventId");
            m_tmpInputTriggerDelayDescription = BindRequiredComponent<TMP_InputField>("m_rectTriggerPrerequisiteSection/m_tmpInputTriggerDelayDescription");
            m_toggleEffectNarrativeDmOnly = BindRequiredComponent<Toggle>("m_rectEffectNarrativeSection/m_toggleEffectNarrativeDmOnly");
            m_tmpInputEffectNarrativeText = BindRequiredComponent<TMP_InputField>("m_rectEffectNarrativeSection/m_tmpInputEffectNarrativeText");
            m_tmpInputEffectDialogueTarget = BindRequiredComponent<TMP_InputField>("m_rectEffectDialogueSection/m_tmpInputEffectDialogueTarget");
            m_tmpInputEffectDialogueSummary = BindRequiredComponent<TMP_InputField>("m_rectEffectDialogueSection/m_tmpInputEffectDialogueSummary");
            m_tmpInputEffectDialoguePrompt = BindRequiredComponent<TMP_InputField>("m_rectEffectDialogueSection/m_tmpInputEffectDialoguePrompt");
            m_tmpDropdownEffectCreatureInstanceId = BindRequiredComponent<TMP_Dropdown>("m_rectEffectCombatantActivationSection/m_tmpDropdownEffectCreatureInstanceId");
            m_toggleEffectCreatureActivate = BindRequiredComponent<Toggle>("m_rectEffectCombatantActivationSection/m_toggleEffectCreatureActivate");
            m_tmpInputEffectBattleReference = BindRequiredComponent<TMP_InputField>("m_rectEffectBattleSection/m_tmpInputEffectBattleReference");
            m_toggleEffectBattleIncludeActiveCreatures = BindRequiredComponent<Toggle>("m_rectEffectBattleSection/m_toggleEffectBattleIncludeActiveCreatures");
            m_tmpInputEffectBattleDescription = BindRequiredComponent<TMP_InputField>("m_rectEffectBattleSection/m_tmpInputEffectBattleDescription");
            m_tmpInputAbilityStrength = BindRequiredComponent<TMP_InputField>("m_rectAbilityCheckSection/m_tmpInputAbilityStrength");
            m_tmpInputAbilityDexterity = BindRequiredComponent<TMP_InputField>("m_rectAbilityCheckSection/m_tmpInputAbilityDexterity");
            m_tmpInputAbilityConstitution = BindRequiredComponent<TMP_InputField>("m_rectAbilityCheckSection/m_tmpInputAbilityConstitution");
            m_tmpInputAbilityIntelligence = BindRequiredComponent<TMP_InputField>("m_rectAbilityCheckSection/m_tmpInputAbilityIntelligence");
            m_tmpInputAbilityWisdom = BindRequiredComponent<TMP_InputField>("m_rectAbilityCheckSection/m_tmpInputAbilityWisdom");
            m_tmpInputAbilityCharisma = BindRequiredComponent<TMP_InputField>("m_rectAbilityCheckSection/m_tmpInputAbilityCharisma");
            m_tmpInputEventTitle = BindRequiredComponent<TMP_InputField>("m_tmpInputEventTitle");
            m_tmpInputTriggerDescription = BindRequiredComponent<TMP_InputField>("m_tmpInputTriggerDescription");
            m_tmpInputSuccessResult = BindRequiredComponent<TMP_InputField>("m_tmpInputSuccessResult");
            m_tmpInputFailureResult = BindRequiredComponent<TMP_InputField>("m_tmpInputFailureResult");
            m_tmpInputDmNote = BindRequiredComponent<TMP_InputField>("m_tmpInputDmNote");
            BindSkillCheckInputComponents();

            if (!HasRequiredBindings())
            {
                return;
            }

            EnsureEventIdFieldCreated();
            m_btnClose.onClick.RemoveAllListeners();
            m_btnClose.onClick.AddListener(OnClickCloseBtn);
            SetupDropdown(m_tmpDropdownEffectType, EffectTypeLabels, OnEffectTypeDropdownChanged);
            SetupDropdown(m_tmpDropdownTriggerType, TriggerTypeLabels, OnTriggerTypeDropdownChanged);
            SetupDropdown(m_tmpDropdownEffectCreaturePlacementMode, EffectCreaturePlacementModeLabels, OnEffectCreaturePlacementModeDropdownChanged);
            m_tmpDropdownEffectCreatureInstanceId.onValueChanged.RemoveAllListeners();
            SetupDropdown(m_tmpDropdownCheckTargetMode, CheckTargetModeLabels, OnCheckTargetModeDropdownChanged);
            SetupDropdown(m_tmpDropdownCheckResolutionMode, CheckResolutionModeLabels, OnCheckResolutionModeDropdownChanged);
            m_btnConfirm.onClick.RemoveAllListeners();
            m_btnConfirm.onClick.AddListener(OnClickConfirmBtn);
            m_tmpInputTriggerDelayDescription.onValueChanged.AddListener(OnTriggerLayoutInputChanged);
        }

        protected override void OnRefresh()
        {
            m_request = UserData as ChapterEventPopupRequest ?? new ChapterEventPopupRequest();
            m_triggerType = ChapterEventTriggerType.DmManual;
            m_effectType = ChapterEventEffectType.Check;
            m_effectCreaturePlacementMode = ChapterEffectCreaturePlacementMode.UseSavedInstancePosition;
            m_checkTargetMode = ChapterCheckTargetMode.Ability;
            m_checkResolutionMode = ChapterCheckResolutionMode.RollDice;
            m_existingEventId = string.Empty;
            ResetSkillCheckInputFields();
            SetToggleValue(m_toggleTriggerAreaFirstEnterOnly, false);
            SetToggleValue(m_toggleTriggerAreaShareBinding, false);
            SetToggleValue(m_toggleTriggerInteractionRequireConfirm, false);
            SetToggleValue(m_toggleEffectNarrativeDmOnly, false);
            SetToggleValue(m_toggleEffectCreatureActivate, true);
            SetToggleValue(m_toggleEffectBattleIncludeActiveCreatures, true);
            ResetInputField(m_tmpInputTriggerInteractionTarget, TMP_InputField.LineType.SingleLine);
            ResetInputField(m_tmpInputTriggerPrerequisiteEventId, TMP_InputField.LineType.SingleLine);
            ResetInputField(m_tmpInputTriggerDelayDescription, TMP_InputField.LineType.MultiLineNewline);
            ResetInputField(m_tmpInputEffectNarrativeText, TMP_InputField.LineType.MultiLineNewline);
            ResetInputField(m_tmpInputEffectDialogueTarget, TMP_InputField.LineType.SingleLine);
            ResetInputField(m_tmpInputEffectDialogueSummary, TMP_InputField.LineType.MultiLineNewline);
            ResetInputField(m_tmpInputEffectDialoguePrompt, TMP_InputField.LineType.MultiLineNewline);
            RefreshCreatureInstanceDropdown(string.Empty);
            ResetInputField(m_tmpInputEffectBattleReference, TMP_InputField.LineType.SingleLine);
            ResetInputField(m_tmpInputEffectBattleDescription, TMP_InputField.LineType.MultiLineNewline);
            ResetInputField(m_tmpInputAbilityStrength, TMP_InputField.LineType.SingleLine);
            ResetInputField(m_tmpInputAbilityDexterity, TMP_InputField.LineType.SingleLine);
            ResetInputField(m_tmpInputAbilityConstitution, TMP_InputField.LineType.SingleLine);
            ResetInputField(m_tmpInputAbilityIntelligence, TMP_InputField.LineType.SingleLine);
            ResetInputField(m_tmpInputAbilityWisdom, TMP_InputField.LineType.SingleLine);
            ResetInputField(m_tmpInputAbilityCharisma, TMP_InputField.LineType.SingleLine);
            ResetInputField(m_tmpInputEventTitle, TMP_InputField.LineType.SingleLine);
            ResetInputField(m_tmpInputTriggerDescription, TMP_InputField.LineType.MultiLineNewline);
            ResetInputField(m_tmpInputSuccessResult, TMP_InputField.LineType.MultiLineNewline);
            ResetInputField(m_tmpInputFailureResult, TMP_InputField.LineType.MultiLineNewline);
            ResetInputField(m_tmpInputDmNote, TMP_InputField.LineType.MultiLineNewline);
            SetToggleValue(m_toggleEventEnabled, true);
            SetToggleValue(m_toggleEventOneShot, false);

            ApplyExistingEventData(m_request.ExistingEventData);

            RefreshView();
        }

        private void OnEffectTypeDropdownChanged(int index)
        {
            m_effectType = (ChapterEventEffectType) Mathf.Clamp(index, 0, EffectTypeLabels.Length - 1);
            if (m_effectType == ChapterEventEffectType.PlayerInitialPosition)
            {
                m_triggerType = ChapterEventTriggerType.ChapterEnter;
            }

            RefreshView();
        }

        private void OnTriggerTypeDropdownChanged(int index)
        {
            m_triggerType = (ChapterEventTriggerType) Mathf.Clamp(index, 0, TriggerTypeLabels.Length - 1);
            if (m_effectType == ChapterEventEffectType.PlayerInitialPosition)
            {
                m_triggerType = ChapterEventTriggerType.ChapterEnter;
            }

            RefreshView();
        }

        private void OnEffectCreaturePlacementModeDropdownChanged(int index)
        {
            m_effectCreaturePlacementMode = (ChapterEffectCreaturePlacementMode) Mathf.Clamp(index, 0, EffectCreaturePlacementModeLabels.Length - 1);
        }

        private void OnCheckTargetModeDropdownChanged(int index)
        {
            m_checkTargetMode = (ChapterCheckTargetMode) Mathf.Clamp(index, 0, CheckTargetModeLabels.Length - 1);
            RefreshView();
        }

        private void OnCheckResolutionModeDropdownChanged(int index)
        {
            m_checkResolutionMode = (ChapterCheckResolutionMode) Mathf.Clamp(index, 0, CheckResolutionModeLabels.Length - 1);
            RefreshView();
        }

        private void OnTriggerLayoutInputChanged(string _)
        {
            RefreshDynamicLayout(m_effectType == ChapterEventEffectType.Check);
        }

        private void RefreshEventIdDisplay()
        {
            if (m_tmpInputEventId == null)
            {
                return;
            }

            SetInputValue(m_tmpInputEventId, m_existingEventId);
        }

        private void RefreshCreatureInstanceDropdown(string selectedInstanceId)
        {
            m_effectCreatureInstanceOptionIds.Clear();
            if (m_tmpDropdownEffectCreatureInstanceId == null)
            {
                return;
            }

            m_tmpDropdownEffectCreatureInstanceId.onValueChanged.RemoveAllListeners();
            m_tmpDropdownEffectCreatureInstanceId.ClearOptions();

            List<string> optionLabels = new List<string> { "请选择生物实例" };
            m_effectCreatureInstanceOptionIds.Add(string.Empty);

            bool hasSelectedInstance = string.IsNullOrWhiteSpace(selectedInstanceId);
            List<ChapterCreatureInstanceData> creatureInstances = m_request?.CreatureInstances;
            if (creatureInstances != null)
            {
                for (int index = 0; index < creatureInstances.Count; index++)
                {
                    ChapterCreatureInstanceData creatureInstance = ChapterCreatureDataStructureUtility.NormalizeCreatureInstanceData(creatureInstances[index]);
                    if (creatureInstance == null || string.IsNullOrWhiteSpace(creatureInstance.InstanceId))
                    {
                        continue;
                    }

                    optionLabels.Add(BuildCreatureInstanceOptionLabel(creatureInstance));
                    m_effectCreatureInstanceOptionIds.Add(creatureInstance.InstanceId);
                    if (string.Equals(creatureInstance.InstanceId, selectedInstanceId, StringComparison.Ordinal))
                    {
                        hasSelectedInstance = true;
                    }
                }
            }

            if (!hasSelectedInstance && !string.IsNullOrWhiteSpace(selectedInstanceId))
            {
                optionLabels.Add($"未找到实例 | {selectedInstanceId}");
                m_effectCreatureInstanceOptionIds.Add(selectedInstanceId);
            }

            m_tmpDropdownEffectCreatureInstanceId.AddOptions(optionLabels);
            m_tmpDropdownEffectCreatureInstanceId.interactable = optionLabels.Count > 1;
            SetCreatureInstanceDropdownValue(selectedInstanceId);
        }

        private void SetCreatureInstanceDropdownValue(string instanceId)
        {
            if (m_tmpDropdownEffectCreatureInstanceId == null || m_effectCreatureInstanceOptionIds.Count <= 0)
            {
                return;
            }

            int targetIndex = 0;
            for (int index = 0; index < m_effectCreatureInstanceOptionIds.Count; index++)
            {
                if (!string.Equals(m_effectCreatureInstanceOptionIds[index], instanceId, StringComparison.Ordinal))
                {
                    continue;
                }

                targetIndex = index;
                break;
            }

            SetDropdownValue(m_tmpDropdownEffectCreatureInstanceId, targetIndex);
        }

        private string GetSelectedEffectCreatureInstanceId()
        {
            if (m_tmpDropdownEffectCreatureInstanceId == null || m_effectCreatureInstanceOptionIds.Count <= 0)
            {
                return string.Empty;
            }

            int selectedIndex = Mathf.Clamp(m_tmpDropdownEffectCreatureInstanceId.value, 0, m_effectCreatureInstanceOptionIds.Count - 1);
            return m_effectCreatureInstanceOptionIds[selectedIndex] ?? string.Empty;
        }

        private static string BuildCreatureInstanceOptionLabel(ChapterCreatureInstanceData creatureInstance)
        {
            string creatureName = creatureInstance.RuntimeSheet != null && !string.IsNullOrWhiteSpace(creatureInstance.RuntimeSheet.Name)
                ? creatureInstance.RuntimeSheet.Name.Trim()
                : "未命名生物";
            ChapterCreatureInstancePlacementData placement = creatureInstance.Placement ?? new ChapterCreatureInstancePlacementData();
            string activeState = creatureInstance.IsActive ? "已显示" : "隐藏";
            return $"{creatureName} | 格子({placement.AnchorCell.CellX},{placement.AnchorCell.CellY}) | {activeState}";
        }

        private void OnClickConfirmBtn()
        {
            ChapterGridEventData eventData = BuildEventData();
            string abilityThresholdSummary = BuildAbilityThresholdSummary();
            int affectedCellCount = m_request.GridCoordinates != null && m_request.GridCoordinates.Count > 0
                ? m_request.GridCoordinates.Count
                : 1;
            Log.Info(
                $"[ChapterEventPopupUI] 已记录格子事件。章节={m_request.ChapterId}, 目标格数={affectedCellCount}, 首格坐标={m_request.GridCoordinate}, 效果类型={EffectTypeLabels[(int) m_effectType]}, 标题={GetInputValue(m_tmpInputEventTitle)}{abilityThresholdSummary}");
            m_request.OnConfirm?.Invoke(eventData);
            Close();
        }

        private void RefreshView()
        {
            bool isCheckEffect = m_effectType == ChapterEventEffectType.Check;
            bool isNarrativeEffect = m_effectType == ChapterEventEffectType.NarrativePrompt;
            bool isDialogueEffect = m_effectType == ChapterEventEffectType.DialogueInteractionPrompt;
            bool isCombatantActivationEffect = m_effectType == ChapterEventEffectType.ActivateCreatureInstance;
            bool isBattleEffect = m_effectType == ChapterEventEffectType.StartBattle;
            bool isPlayerInitialPositionEffect = m_effectType == ChapterEventEffectType.PlayerInitialPosition;
            bool isAbilityCheck = isCheckEffect && m_checkTargetMode == ChapterCheckTargetMode.Ability;
            bool isSkillCheck = isCheckEffect && m_checkTargetMode == ChapterCheckTargetMode.Skill;
            if (isPlayerInitialPositionEffect)
            {
                m_triggerType = ChapterEventTriggerType.ChapterEnter;
                SetToggleValue(m_toggleEventOneShot, false);
            }

            if (m_tmpTitle != null)
            {
                bool isBatchEdit = m_request.GridCoordinates != null && m_request.GridCoordinates.Count > 1;
                m_tmpTitle.text = isBatchEdit
                    ? $"批量标记事件 ({m_request.GridCoordinates.Count})"
                    : m_request.ExistingEventData != null ? "编辑事件" : "添加事件";
            }

            SetDropdownValue(m_tmpDropdownEffectType, (int) m_effectType);
            SetDropdownValue(m_tmpDropdownTriggerType, (int) m_triggerType);
            SetDropdownValue(m_tmpDropdownEffectCreaturePlacementMode, (int) m_effectCreaturePlacementMode);
            SetDropdownValue(m_tmpDropdownCheckTargetMode, (int) m_checkTargetMode);
            SetDropdownValue(m_tmpDropdownCheckResolutionMode, (int) m_checkResolutionMode);
            SetButtonLabel(m_btnConfirm, "确定");
            RefreshEventIdDisplay();

            RefreshBindingSummary();

            if (m_tmpDropdownTriggerType != null)
            {
                m_tmpDropdownTriggerType.interactable = !isPlayerInitialPositionEffect;
            }

            if (m_toggleEventOneShot != null)
            {
                m_toggleEventOneShot.interactable = !isPlayerInitialPositionEffect;
            }

            if (m_rectTriggerManualSection != null)
            {
                m_rectTriggerManualSection.gameObject.SetActive(m_triggerType == ChapterEventTriggerType.DmManual && !isPlayerInitialPositionEffect);
            }

            if (m_rectTriggerAreaSection != null)
            {
                m_rectTriggerAreaSection.gameObject.SetActive(m_triggerType == ChapterEventTriggerType.EnterBindingArea && !isPlayerInitialPositionEffect);
            }

            if (m_rectTriggerInteractionSection != null)
            {
                m_rectTriggerInteractionSection.gameObject.SetActive(m_triggerType == ChapterEventTriggerType.InteractWithSceneObject && !isPlayerInitialPositionEffect);
            }

            if (m_rectTriggerPrerequisiteSection != null)
            {
                m_rectTriggerPrerequisiteSection.gameObject.SetActive(m_triggerType == ChapterEventTriggerType.AfterPrerequisiteEvent && !isPlayerInitialPositionEffect);
            }

            if (m_rectEffectNarrativeSection != null)
            {
                m_rectEffectNarrativeSection.gameObject.SetActive(isNarrativeEffect);
            }

            if (m_rectEffectDialogueSection != null)
            {
                m_rectEffectDialogueSection.gameObject.SetActive(isDialogueEffect);
            }

            if (m_rectEffectCombatantActivationSection != null)
            {
                m_rectEffectCombatantActivationSection.gameObject.SetActive(isCombatantActivationEffect);
            }

            if (m_rectEffectBattleSection != null)
            {
                m_rectEffectBattleSection.gameObject.SetActive(isBattleEffect);
            }

            if (m_tmpDropdownCheckTargetMode != null)
            {
                m_tmpDropdownCheckTargetMode.gameObject.SetActive(isCheckEffect);
            }

            if (m_tmpDropdownCheckResolutionMode != null)
            {
                m_tmpDropdownCheckResolutionMode.gameObject.SetActive(isCheckEffect);
            }

            if (m_rectAbilityCheckSection != null)
            {
                m_rectAbilityCheckSection.gameObject.SetActive(isAbilityCheck);
            }

            if (m_rectSkillCheckSection != null)
            {
                m_rectSkillCheckSection.gameObject.SetActive(isSkillCheck);
            }

            if (m_tmpInputSuccessResult != null)
            {
                m_tmpInputSuccessResult.gameObject.SetActive(isCheckEffect);
            }

            if (m_tmpInputFailureResult != null)
            {
                m_tmpInputFailureResult.gameObject.SetActive(isCheckEffect);
            }

            RefreshDynamicLayout(isCheckEffect);
        }

        private void RefreshDynamicLayout(bool isCheckEffect)
        {
            if (!HasRequiredBindings())
            {
                return;
            }

            InitializeEventPopupLayout();
            Canvas.ForceUpdateCanvases();

            float currentTop = m_eventPopupSectionTop;
            RectTransform triggerSection = GetActiveTriggerSectionRect();
            RectTransform effectSection = GetActiveEffectSectionRect(isCheckEffect);
            RectTransform checkTargetRect = GetRectTransform(m_tmpDropdownCheckTargetMode);
            RectTransform checkResolutionRect = GetRectTransform(m_tmpDropdownCheckResolutionMode);
            RectTransform triggerDescriptionRect = GetRectTransform(m_tmpInputTriggerDescription);
            RectTransform successResultRect = GetRectTransform(m_tmpInputSuccessResult);
            RectTransform failureResultRect = GetRectTransform(m_tmpInputFailureResult);
            RectTransform dmNoteRect = GetRectTransform(m_tmpInputDmNote);

            RefreshTriggerSectionLayout(triggerSection);

            if (triggerSection != null && triggerSection.gameObject.activeSelf)
            {
                SetRectTop(triggerSection, currentTop);
                currentTop = GetBottom(triggerSection) + m_eventPopupGapAfterTriggerSection;
            }

            if (isCheckEffect)
            {
                SetRectTop(checkTargetRect, currentTop);
                SetRectTop(checkResolutionRect, currentTop);
                currentTop = Mathf.Max(GetBottom(checkTargetRect), GetBottom(checkResolutionRect)) + m_eventPopupGapAfterCheckControls;
            }

            if (effectSection != null && effectSection.gameObject.activeSelf)
            {
                SetRectTop(effectSection, currentTop);
                currentTop = GetBottom(effectSection) + m_eventPopupGapAfterEffectSection;
            }

            SetRectTop(triggerDescriptionRect, currentTop);
            currentTop = GetBottom(triggerDescriptionRect) + m_eventPopupGapBetweenInputFields;

            if (isCheckEffect)
            {
                SetRectTop(successResultRect, currentTop);
                currentTop = GetBottom(successResultRect) + m_eventPopupGapBetweenInputFields;

                SetRectTop(failureResultRect, currentTop);
                currentTop = GetBottom(failureResultRect) + m_eventPopupGapBetweenInputFields;
            }

            SetRectTop(dmNoteRect, currentTop);
            float contentBottom = GetBottom(dmNoteRect);
            float targetPanelHeight = Mathf.Max(
                m_eventPopupMinimumPanelHeight,
                contentBottom + EventPopupConfirmSpacing + m_eventPopupConfirmTopInset);
            SetRectHeight(m_rectPanel, targetPanelHeight);
            Canvas.ForceUpdateCanvases();
        }

        private void InitializeEventPopupLayout()
        {
            if (m_isEventPopupLayoutInitialized)
            {
                return;
            }

            RectTransform checkTargetRect = GetRectTransform(m_tmpDropdownCheckTargetMode);
            RectTransform triggerDescriptionRect = GetRectTransform(m_tmpInputTriggerDescription);
            RectTransform successResultRect = GetRectTransform(m_tmpInputSuccessResult);
            RectTransform dmNoteRect = GetRectTransform(m_tmpInputDmNote);
            RectTransform confirmRect = GetRectTransform(m_btnConfirm);

            m_eventPopupSectionTop = GetRectTop(m_rectTriggerManualSection);
            m_eventPopupGapAfterTriggerSection = Mathf.Max(8f, GetRectTop(checkTargetRect) - GetBottom(m_rectTriggerManualSection));
            m_eventPopupGapAfterCheckControls = Mathf.Max(8f, GetRectTop(m_rectAbilityCheckSection) - GetBottom(checkTargetRect));
            m_eventPopupGapAfterEffectSection = Mathf.Max(8f, GetRectTop(triggerDescriptionRect) - GetBottom(m_rectAbilityCheckSection));
            m_eventPopupGapBetweenInputFields = Mathf.Max(8f, GetRectTop(successResultRect) - GetBottom(triggerDescriptionRect));
            m_eventPopupConfirmTopInset = GetBottomAnchoredTopInset(confirmRect);
            m_triggerDelayDescriptionMinimumHeight = GetCurrentHeight(GetRectTransform(m_tmpInputTriggerDelayDescription));
            m_triggerPrerequisiteSectionMinimumHeight = GetCurrentHeight(m_rectTriggerPrerequisiteSection);
            m_triggerPrerequisiteSectionBottomPadding = Mathf.Max(
                0f,
                m_triggerPrerequisiteSectionMinimumHeight - GetBottom(GetRectTransform(m_tmpInputTriggerDelayDescription)));
            m_eventPopupMinimumPanelHeight = m_eventPopupSectionTop
                + GetMinimumRectHeight(m_rectTriggerManualSection, m_rectTriggerAreaSection, m_rectTriggerInteractionSection, m_rectTriggerPrerequisiteSection)
                + m_eventPopupGapAfterTriggerSection
                + GetMinimumRectHeight(
                    m_rectEffectNarrativeSection,
                    m_rectEffectDialogueSection,
                    m_rectEffectCombatantActivationSection,
                    m_rectEffectBattleSection,
                    m_rectAbilityCheckSection,
                    m_rectSkillCheckSection)
                + m_eventPopupGapAfterEffectSection
                + GetCurrentHeight(triggerDescriptionRect)
                + m_eventPopupGapBetweenInputFields
                + GetCurrentHeight(dmNoteRect)
                + EventPopupConfirmSpacing
                + m_eventPopupConfirmTopInset;
            m_isEventPopupLayoutInitialized = true;
        }

        private void EnsureEventIdFieldCreated()
        {
            if (m_tmpEventIdLabel != null && m_tmpInputEventId != null)
            {
                return;
            }

            m_tmpEventIdLabel = Object.Instantiate(m_tmpBindingSummary, m_rectPanel);
            m_tmpEventIdLabel.name = "m_tmpEventIdLabel";
            m_tmpEventIdLabel.text = "事件 ID";
            m_tmpEventIdLabel.raycastTarget = false;
            if (m_tmpEventIdLabel is TextMeshProUGUI eventIdLabel)
            {
                eventIdLabel.alignment = TextAlignmentOptions.Left;
            }

            RectTransform labelRect = GetRectTransform(m_tmpEventIdLabel);
            if (labelRect != null)
            {
                SetRectTop(labelRect, EventPopupEventIdLabelTop);
            }

            m_tmpInputEventId = Object.Instantiate(m_tmpInputEventTitle, m_rectPanel);
            m_tmpInputEventId.name = "m_tmpInputEventId";
            m_tmpInputEventId.readOnly = true;
            m_tmpInputEventId.lineType = TMP_InputField.LineType.SingleLine;
            m_tmpInputEventId.onValueChanged.RemoveAllListeners();
            m_tmpInputEventId.onEndEdit.RemoveAllListeners();
            m_tmpInputEventId.onSubmit.RemoveAllListeners();
            m_tmpInputEventId.onSelect.RemoveAllListeners();
            m_tmpInputEventId.onDeselect.RemoveAllListeners();
            m_tmpInputEventId.onTextSelection.RemoveAllListeners();
            m_tmpInputEventId.onEndTextSelection.RemoveAllListeners();
            m_tmpInputEventId.onTouchScreenKeyboardStatusChanged.RemoveAllListeners();
            if (m_tmpInputEventId.textComponent != null)
            {
                m_tmpInputEventId.textComponent.enableWordWrapping = false;
            }

            if (m_tmpInputEventId.placeholder is TMP_Text placeholder)
            {
                placeholder.text = "保存后自动生成，可复制";
            }

            RectTransform inputRect = GetRectTransform(m_tmpInputEventId);
            if (inputRect != null)
            {
                SetRectTop(inputRect, EventPopupEventIdInputTop);
                SetRectHeight(inputRect, EventPopupEventIdInputHeight);
            }

            SetSiblingIndexAfter(m_tmpEventIdLabel.transform, m_tmpBindingSummary.transform);
            SetSiblingIndexAfter(m_tmpInputEventId.transform, m_tmpEventIdLabel.transform);

            ShiftEventPopupContentForEventId();
        }

        private void ShiftEventPopupContentForEventId()
        {
            ShiftRectDown(GetRectTransform(m_toggleEventEnabled), EventPopupEventIdLayoutShift);
            ShiftRectDown(GetRectTransform(m_toggleEventOneShot), EventPopupEventIdLayoutShift);
            ShiftRectDown(GetRectTransform(m_tmpDropdownEffectType), EventPopupEventIdLayoutShift);
            ShiftRectDown(GetRectTransform(m_tmpDropdownTriggerType), EventPopupEventIdLayoutShift);
            ShiftRectDown(m_rectTriggerManualSection, EventPopupEventIdLayoutShift);
            ShiftRectDown(m_rectTriggerAreaSection, EventPopupEventIdLayoutShift);
            ShiftRectDown(m_rectTriggerInteractionSection, EventPopupEventIdLayoutShift);
            ShiftRectDown(m_rectTriggerPrerequisiteSection, EventPopupEventIdLayoutShift);
            ShiftRectDown(GetRectTransform(m_tmpDropdownCheckTargetMode), EventPopupEventIdLayoutShift);
            ShiftRectDown(GetRectTransform(m_tmpDropdownCheckResolutionMode), EventPopupEventIdLayoutShift);
            ShiftRectDown(m_rectEffectNarrativeSection, EventPopupEventIdLayoutShift);
            ShiftRectDown(m_rectEffectDialogueSection, EventPopupEventIdLayoutShift);
            ShiftRectDown(m_rectEffectCombatantActivationSection, EventPopupEventIdLayoutShift);
            ShiftRectDown(m_rectEffectBattleSection, EventPopupEventIdLayoutShift);
            ShiftRectDown(m_rectAbilityCheckSection, EventPopupEventIdLayoutShift);
            ShiftRectDown(m_rectSkillCheckSection, EventPopupEventIdLayoutShift);
            ShiftRectDown(GetRectTransform(m_tmpInputTriggerDescription), EventPopupEventIdLayoutShift);
            ShiftRectDown(GetRectTransform(m_tmpInputSuccessResult), EventPopupEventIdLayoutShift);
            ShiftRectDown(GetRectTransform(m_tmpInputFailureResult), EventPopupEventIdLayoutShift);
            ShiftRectDown(GetRectTransform(m_tmpInputDmNote), EventPopupEventIdLayoutShift);
        }

        private void RefreshTriggerSectionLayout(RectTransform triggerSection)
        {
            if (triggerSection == null)
            {
                return;
            }

            if (triggerSection == m_rectTriggerPrerequisiteSection)
            {
                RectTransform delayDescriptionRect = GetRectTransform(m_tmpInputTriggerDelayDescription);
                RefreshInputFieldHeight(m_tmpInputTriggerDelayDescription, m_triggerDelayDescriptionMinimumHeight);

                float targetSectionHeight = Mathf.Max(
                    m_triggerPrerequisiteSectionMinimumHeight,
                    GetBottom(delayDescriptionRect) + m_triggerPrerequisiteSectionBottomPadding);
                SetRectHeight(m_rectTriggerPrerequisiteSection, targetSectionHeight);
                return;
            }

            if (triggerSection == m_rectTriggerManualSection)
            {
                SetRectHeight(m_rectTriggerManualSection, GetMinimumRectHeight(m_rectTriggerManualSection));
                return;
            }

            if (triggerSection == m_rectTriggerAreaSection)
            {
                SetRectHeight(m_rectTriggerAreaSection, GetMinimumRectHeight(m_rectTriggerAreaSection));
                return;
            }

            if (triggerSection == m_rectTriggerInteractionSection)
            {
                SetRectHeight(m_rectTriggerInteractionSection, GetMinimumRectHeight(m_rectTriggerInteractionSection));
            }
        }

        private RectTransform GetActiveTriggerSectionRect()
        {
            if (m_rectTriggerManualSection.gameObject.activeSelf)
            {
                return m_rectTriggerManualSection;
            }

            if (m_rectTriggerAreaSection.gameObject.activeSelf)
            {
                return m_rectTriggerAreaSection;
            }

            if (m_rectTriggerInteractionSection.gameObject.activeSelf)
            {
                return m_rectTriggerInteractionSection;
            }

            if (m_rectTriggerPrerequisiteSection.gameObject.activeSelf)
            {
                return m_rectTriggerPrerequisiteSection;
            }

            return null;
        }

        private RectTransform GetActiveEffectSectionRect(bool isCheckEffect)
        {
            if (isCheckEffect)
            {
                if (m_rectSkillCheckSection.gameObject.activeSelf)
                {
                    return m_rectSkillCheckSection;
                }

                return m_rectAbilityCheckSection;
            }

            if (m_rectEffectNarrativeSection.gameObject.activeSelf)
            {
                return m_rectEffectNarrativeSection;
            }

            if (m_rectEffectDialogueSection.gameObject.activeSelf)
            {
                return m_rectEffectDialogueSection;
            }

            if (m_rectEffectCombatantActivationSection.gameObject.activeSelf)
            {
                return m_rectEffectCombatantActivationSection;
            }

            if (m_rectEffectBattleSection.gameObject.activeSelf)
            {
                return m_rectEffectBattleSection;
            }

            return null;
        }

        private string BuildAbilityThresholdSummary()
        {
            if (m_effectType != ChapterEventEffectType.Check)
            {
                return string.Empty;
            }

            if (m_checkTargetMode == ChapterCheckTargetMode.Skill)
            {
                List<ChapterSkillCheckThresholdData> skillCheckEntries = BuildSkillCheckEntries();
                if (skillCheckEntries.Count == 0)
                {
                    return string.Empty;
                }

                List<string> summarySegments = new List<string>(skillCheckEntries.Count);
                for (int index = 0; index < skillCheckEntries.Count; index++)
                {
                    ChapterSkillCheckThresholdData entry = skillCheckEntries[index];
                    summarySegments.Add($"{entry.SkillName}:{entry.Threshold}");
                }

                return $", 技能检定[{string.Join(", ", summarySegments)}]";
            }

            if (m_checkTargetMode != ChapterCheckTargetMode.Ability)
            {
                return string.Empty;
            }

            return $", 属性通过值[力量:{GetInputValue(m_tmpInputAbilityStrength)}, 敏捷:{GetInputValue(m_tmpInputAbilityDexterity)}, 体质:{GetInputValue(m_tmpInputAbilityConstitution)}, 智力:{GetInputValue(m_tmpInputAbilityIntelligence)}, 感知:{GetInputValue(m_tmpInputAbilityWisdom)}, 魅力:{GetInputValue(m_tmpInputAbilityCharisma)}]";
        }

        private void OnClickCloseBtn()
        {
            Close();
        }

        private static void SetButtonLabel(Button button, string text)
        {
            if (button == null)
            {
                return;
            }

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.text = text;
            }
        }

        private static void SetupDropdown(TMP_Dropdown dropdown, string[] options, UnityAction<int> onValueChanged)
        {
            if (dropdown == null)
            {
                return;
            }

            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.ClearOptions();
            dropdown.AddOptions(new List<string>(options));
            dropdown.onValueChanged.AddListener(onValueChanged);
            dropdown.SetValueWithoutNotify(0);
            dropdown.RefreshShownValue();
        }

        private static void SetDropdownValue(TMP_Dropdown dropdown, int value)
        {
            if (dropdown == null || dropdown.options == null || dropdown.options.Count == 0)
            {
                return;
            }

            int clampedValue = Mathf.Clamp(value, 0, dropdown.options.Count - 1);
            dropdown.SetValueWithoutNotify(clampedValue);
            dropdown.RefreshShownValue();
        }

        private static void SetToggleValue(Toggle toggle, bool value)
        {
            if (toggle == null)
            {
                return;
            }

            toggle.SetIsOnWithoutNotify(value);
        }

        private static bool GetToggleValue(Toggle toggle)
        {
            return toggle != null && toggle.isOn;
        }

        private static void ResetInputField(TMP_InputField inputField, TMP_InputField.LineType lineType)
        {
            if (inputField == null)
            {
                return;
            }

            inputField.lineType = lineType;
            inputField.text = string.Empty;
        }

        private T BindRequiredComponent<T>(string path) where T : Component
        {
            T component = ResolvePopupComponent<T>(path);
            if (component == null)
            {
                Log.Error($"[ChapterEventPopupUI] 找不到节点: {path}");
            }

            return component;
        }

        private T ResolvePopupComponent<T>(string path) where T : Component
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            T component = FindChildComponent<T>(path);
            if (component != null)
            {
                return component;
            }

            if (path.StartsWith(PopupPanelPathPrefix, StringComparison.Ordinal))
            {
                return null;
            }

            return FindChildComponent<T>($"{PopupPanelPathPrefix}{path}");
        }

        private bool HasRequiredBindings()
        {
            return m_tmpTitle != null
                && m_btnClose != null
                && m_tmpDropdownEffectType != null
                && m_tmpDropdownTriggerType != null
                && m_tmpDropdownEffectCreaturePlacementMode != null
                && m_tmpDropdownCheckTargetMode != null
                && m_tmpDropdownCheckResolutionMode != null
                && m_btnConfirm != null
                && m_rectPanel != null
                && m_tmpBindingSummary != null
                && m_toggleEventEnabled != null
                && m_toggleEventOneShot != null
                && m_rectTriggerManualSection != null
                && m_rectTriggerAreaSection != null
                && m_rectTriggerInteractionSection != null
                && m_rectTriggerPrerequisiteSection != null
                && m_rectEffectNarrativeSection != null
                && m_rectEffectDialogueSection != null
                && m_rectEffectCombatantActivationSection != null
                && m_rectEffectBattleSection != null
                && m_rectAbilityCheckSection != null
                && m_rectSkillCheckSection != null
                && m_toggleTriggerAreaFirstEnterOnly != null
                && m_toggleTriggerAreaShareBinding != null
                && m_tmpInputTriggerInteractionTarget != null
                && m_toggleTriggerInteractionRequireConfirm != null
                && m_tmpInputTriggerPrerequisiteEventId != null
                && m_tmpInputTriggerDelayDescription != null
                && m_toggleEffectNarrativeDmOnly != null
                && m_tmpInputEffectNarrativeText != null
                && m_tmpInputEffectDialogueTarget != null
                && m_tmpInputEffectDialogueSummary != null
                && m_tmpInputEffectDialoguePrompt != null
                && m_tmpDropdownEffectCreatureInstanceId != null
                && m_toggleEffectCreatureActivate != null
                && m_tmpInputEffectBattleReference != null
                && m_toggleEffectBattleIncludeActiveCreatures != null
                && m_tmpInputEffectBattleDescription != null
                && m_skillCheckInputs.Count == SkillCheckLabels.Length
                && m_tmpInputAbilityStrength != null
                && m_tmpInputAbilityDexterity != null
                && m_tmpInputAbilityConstitution != null
                && m_tmpInputAbilityIntelligence != null
                && m_tmpInputAbilityWisdom != null
                && m_tmpInputAbilityCharisma != null
                && m_tmpInputEventTitle != null
                && m_tmpInputTriggerDescription != null
                && m_tmpInputSuccessResult != null
                && m_tmpInputFailureResult != null
                && m_tmpInputDmNote != null;
        }

        private void ApplyExistingEventData(ChapterGridEventData eventData)
        {
            eventData = ChapterEventDataStructureUtility.NormalizeRuntimeEventData(eventData);
            if (eventData == null)
            {
                return;
            }

            ChapterEventTriggerData triggerData = eventData.Trigger ?? new ChapterEventTriggerData();
            ChapterEventEffectData effectData = eventData.Effect ?? new ChapterEventEffectData();
            ChapterEventAreaTriggerParamData areaTriggerParam = triggerData.Area ?? new ChapterEventAreaTriggerParamData();
            ChapterEventInteractionTriggerParamData interactionTriggerParam = triggerData.Interaction ?? new ChapterEventInteractionTriggerParamData();
            ChapterEventPrerequisiteTriggerParamData prerequisiteTriggerParam = triggerData.Prerequisite ?? new ChapterEventPrerequisiteTriggerParamData();
            ChapterEventCheckEffectParamData checkEffectParam = effectData.Check ?? new ChapterEventCheckEffectParamData();
            ChapterEventNarrativeEffectParamData narrativeEffectParam = effectData.Narrative ?? new ChapterEventNarrativeEffectParamData();
            ChapterEventDialogueEffectParamData dialogueEffectParam = effectData.Dialogue ?? new ChapterEventDialogueEffectParamData();
            ChapterEventCreatureEffectParamData creatureEffectParam = effectData.Creature ?? new ChapterEventCreatureEffectParamData();
            ChapterEventBattleEffectParamData battleEffectParam = effectData.Battle ?? new ChapterEventBattleEffectParamData();

            m_existingEventId = eventData.EventId ?? string.Empty;
            m_triggerType = ResolveTriggerType(eventData);
            m_effectType = ResolveEffectType(eventData);
            m_effectCreaturePlacementMode = ResolveEffectCreaturePlacementMode(eventData);
            m_checkTargetMode = (ChapterCheckTargetMode) Mathf.Clamp(checkEffectParam.TargetMode, 0, CheckTargetModeLabels.Length - 1);
            m_checkResolutionMode = (ChapterCheckResolutionMode) Mathf.Clamp(checkEffectParam.ResolutionMode, 0, CheckResolutionModeLabels.Length - 1);
            SetToggleValue(m_toggleEventEnabled, eventData.IsEnabled);
            SetToggleValue(m_toggleEventOneShot, eventData.IsOneShot);
            SetToggleValue(m_toggleTriggerAreaFirstEnterOnly, areaTriggerParam.FirstEnterOnly);
            SetToggleValue(m_toggleTriggerAreaShareBinding, areaTriggerParam.ShareBinding);
            SetToggleValue(m_toggleTriggerInteractionRequireConfirm, interactionTriggerParam.RequireConfirm);
            SetToggleValue(m_toggleEffectNarrativeDmOnly, narrativeEffectParam.DmOnly);
            SetToggleValue(m_toggleEffectCreatureActivate, creatureEffectParam.Activate);
            SetToggleValue(m_toggleEffectBattleIncludeActiveCreatures, battleEffectParam.IncludeActiveCreatures);
            SetInputValue(m_tmpInputEventTitle, eventData.EventTitle);
            SetInputValue(m_tmpInputTriggerDescription, eventData.TriggerDescription);
            SetInputValue(m_tmpInputTriggerInteractionTarget, interactionTriggerParam.Target);
            SetInputValue(m_tmpInputTriggerPrerequisiteEventId, prerequisiteTriggerParam.EventId);
            SetInputValue(m_tmpInputTriggerDelayDescription, prerequisiteTriggerParam.DelayDescription);
            SetInputValue(m_tmpInputEffectNarrativeText, GetEffectiveNarrativeText(eventData));
            SetInputValue(m_tmpInputEffectDialogueTarget, dialogueEffectParam.Target);
            SetInputValue(m_tmpInputEffectDialogueSummary, dialogueEffectParam.Summary);
            SetInputValue(m_tmpInputEffectDialoguePrompt, GetEffectiveDialoguePrompt(eventData));
            RefreshCreatureInstanceDropdown(creatureEffectParam.InstanceId);
            SetInputValue(m_tmpInputEffectBattleReference, battleEffectParam.Reference);
            SetInputValue(m_tmpInputEffectBattleDescription, GetEffectiveBattleDescription(eventData));
            SetInputValue(m_tmpInputSuccessResult, checkEffectParam.SuccessResult);
            SetInputValue(m_tmpInputFailureResult, checkEffectParam.FailureResult);
            SetInputValue(m_tmpInputDmNote, eventData.DmNote);
            ApplySkillCheckValues(eventData);
            SetInputValue(m_tmpInputAbilityStrength, checkEffectParam.AbilityStrengthThreshold);
            SetInputValue(m_tmpInputAbilityDexterity, checkEffectParam.AbilityDexterityThreshold);
            SetInputValue(m_tmpInputAbilityConstitution, checkEffectParam.AbilityConstitutionThreshold);
            SetInputValue(m_tmpInputAbilityIntelligence, checkEffectParam.AbilityIntelligenceThreshold);
            SetInputValue(m_tmpInputAbilityWisdom, checkEffectParam.AbilityWisdomThreshold);
            SetInputValue(m_tmpInputAbilityCharisma, checkEffectParam.AbilityCharismaThreshold);
        }

        private ChapterGridEventData BuildEventData()
        {
            bool isPlayerInitialPositionEffect = m_effectType == ChapterEventEffectType.PlayerInitialPosition;
            ChapterEventTriggerType effectiveTriggerType = isPlayerInitialPositionEffect
                ? ChapterEventTriggerType.ChapterEnter
                : m_triggerType;
            List<ChapterSkillCheckThresholdData> skillCheckEntries = BuildSkillCheckEntries();
            ChapterSkillCheckThresholdData primarySkillCheckEntry = skillCheckEntries.Count > 0 ? skillCheckEntries[0] : null;
            ChapterEventTriggerData triggerData = new ChapterEventTriggerData
            {
                TriggerMode = effectiveTriggerType == ChapterEventTriggerType.DmManual
                    ? (int) ChapterEventTriggerMode.DmManual
                    : (int) ChapterEventTriggerMode.Automatic,
                TriggerType = (int) effectiveTriggerType,
                Area = new ChapterEventAreaTriggerParamData
                {
                    FirstEnterOnly = !isPlayerInitialPositionEffect && GetToggleValue(m_toggleTriggerAreaFirstEnterOnly),
                    ShareBinding = !isPlayerInitialPositionEffect && GetToggleValue(m_toggleTriggerAreaShareBinding),
                },
                Interaction = new ChapterEventInteractionTriggerParamData
                {
                    Target = isPlayerInitialPositionEffect ? string.Empty : GetInputValue(m_tmpInputTriggerInteractionTarget),
                    RequireConfirm = !isPlayerInitialPositionEffect && GetToggleValue(m_toggleTriggerInteractionRequireConfirm),
                },
                Prerequisite = new ChapterEventPrerequisiteTriggerParamData
                {
                    EventId = isPlayerInitialPositionEffect ? string.Empty : GetInputValue(m_tmpInputTriggerPrerequisiteEventId),
                    DelayDescription = isPlayerInitialPositionEffect ? string.Empty : GetInputValue(m_tmpInputTriggerDelayDescription),
                },
            };
            ChapterEventEffectData effectData = new ChapterEventEffectData
            {
                EffectType = (int) m_effectType,
                Check = new ChapterEventCheckEffectParamData
                {
                    TargetMode = (int) m_checkTargetMode,
                    ResolutionMode = (int) m_checkResolutionMode,
                    SuccessResult = m_effectType == ChapterEventEffectType.Check ? GetInputValue(m_tmpInputSuccessResult) : string.Empty,
                    FailureResult = m_effectType == ChapterEventEffectType.Check ? GetInputValue(m_tmpInputFailureResult) : string.Empty,
                    SkillCheckEntries = skillCheckEntries,
                    SkillCheckName = primarySkillCheckEntry != null ? primarySkillCheckEntry.SkillName : string.Empty,
                    SkillCheckThreshold = primarySkillCheckEntry != null ? primarySkillCheckEntry.Threshold : string.Empty,
                    AbilityStrengthThreshold = GetInputValue(m_tmpInputAbilityStrength),
                    AbilityDexterityThreshold = GetInputValue(m_tmpInputAbilityDexterity),
                    AbilityConstitutionThreshold = GetInputValue(m_tmpInputAbilityConstitution),
                    AbilityIntelligenceThreshold = GetInputValue(m_tmpInputAbilityIntelligence),
                    AbilityWisdomThreshold = GetInputValue(m_tmpInputAbilityWisdom),
                    AbilityCharismaThreshold = GetInputValue(m_tmpInputAbilityCharisma),
                },
                LegacyDmPrompt = BuildLegacyDmPrompt(),
                Narrative = new ChapterEventNarrativeEffectParamData
                {
                    Text = GetInputValue(m_tmpInputEffectNarrativeText),
                    DmOnly = GetToggleValue(m_toggleEffectNarrativeDmOnly),
                },
                Dialogue = new ChapterEventDialogueEffectParamData
                {
                    Target = GetInputValue(m_tmpInputEffectDialogueTarget),
                    Summary = GetInputValue(m_tmpInputEffectDialogueSummary),
                    Prompt = GetInputValue(m_tmpInputEffectDialoguePrompt),
                },
                Creature = new ChapterEventCreatureEffectParamData
                {
                    InstanceId = GetSelectedEffectCreatureInstanceId(),
                    Activate = GetToggleValue(m_toggleEffectCreatureActivate),
                    PlacementMode = (int) m_effectCreaturePlacementMode,
                },
                Battle = new ChapterEventBattleEffectParamData
                {
                    Reference = GetInputValue(m_tmpInputEffectBattleReference),
                    IncludeActiveCreatures = GetToggleValue(m_toggleEffectBattleIncludeActiveCreatures),
                    Description = GetInputValue(m_tmpInputEffectBattleDescription),
                },
            };
            ChapterGridEventData eventData = new ChapterGridEventData
            {
                EventId = m_existingEventId,
                IsEnabled = GetToggleValue(m_toggleEventEnabled),
                IsOneShot = !isPlayerInitialPositionEffect && GetToggleValue(m_toggleEventOneShot),
                Trigger = triggerData,
                Effect = effectData,
                EventTitle = GetInputValue(m_tmpInputEventTitle, isPlayerInitialPositionEffect ? "玩家初始位置" : string.Empty),
                TriggerDescription = GetInputValue(m_tmpInputTriggerDescription, isPlayerInitialPositionEffect ? "章节进入时，将玩家角色实例放置到当前绑定格子。" : string.Empty),
                DmNote = GetInputValue(m_tmpInputDmNote),
            };

            return ChapterEventDataStructureUtility.NormalizeRuntimeEventData(eventData);
        }

        private static ChapterEventTriggerType ResolveTriggerType(ChapterGridEventData eventData)
        {
            eventData = ChapterEventDataStructureUtility.NormalizeRuntimeEventData(eventData);
            ChapterEventTriggerData triggerData = eventData?.Trigger;
            if (triggerData != null && triggerData.TriggerType >= 0 && triggerData.TriggerType < TriggerTypeLabels.Length)
            {
                return (ChapterEventTriggerType) triggerData.TriggerType;
            }

            if (triggerData != null && triggerData.TriggerMode == (int) ChapterEventTriggerMode.DmManual)
            {
                return ChapterEventTriggerType.DmManual;
            }

            return ChapterEventTriggerType.EnterBindingArea;
        }

        private static ChapterEventEffectType ResolveEffectType(ChapterGridEventData eventData)
        {
            eventData = ChapterEventDataStructureUtility.NormalizeRuntimeEventData(eventData);
            ChapterEventEffectData effectData = eventData?.Effect;
            if (effectData != null && effectData.EffectType >= 0 && effectData.EffectType < EffectTypeLabels.Length)
            {
                return (ChapterEventEffectType) effectData.EffectType;
            }

            return ChapterEventEffectType.Check;
        }

        private static ChapterEffectCreaturePlacementMode ResolveEffectCreaturePlacementMode(ChapterGridEventData eventData)
        {
            eventData = ChapterEventDataStructureUtility.NormalizeRuntimeEventData(eventData);
            ChapterEventCreatureEffectParamData creatureEffectParam = eventData?.Effect?.Creature;
            if (creatureEffectParam != null && creatureEffectParam.PlacementMode >= 0 && creatureEffectParam.PlacementMode < EffectCreaturePlacementModeLabels.Length)
            {
                return (ChapterEffectCreaturePlacementMode) creatureEffectParam.PlacementMode;
            }

            return ChapterEffectCreaturePlacementMode.UseSavedInstancePosition;
        }

        private string BuildLegacyDmPrompt()
        {
            switch (m_effectType)
            {
                case ChapterEventEffectType.NarrativePrompt:
                    return GetInputValue(m_tmpInputEffectNarrativeText);
                case ChapterEventEffectType.DialogueInteractionPrompt:
                    return GetInputValue(m_tmpInputEffectDialoguePrompt);
                case ChapterEventEffectType.StartBattle:
                    return GetInputValue(m_tmpInputEffectBattleDescription);
                default:
                    return string.Empty;
            }
        }

        private static string GetEffectiveNarrativeText(ChapterGridEventData eventData)
        {
            eventData = ChapterEventDataStructureUtility.NormalizeRuntimeEventData(eventData);
            if (!string.IsNullOrWhiteSpace(eventData?.Effect?.Narrative?.Text))
            {
                return eventData.Effect.Narrative.Text;
            }

            return ResolveEffectType(eventData) == ChapterEventEffectType.NarrativePrompt
                ? eventData?.Effect?.LegacyDmPrompt ?? string.Empty
                : string.Empty;
        }

        private static string GetEffectiveDialoguePrompt(ChapterGridEventData eventData)
        {
            eventData = ChapterEventDataStructureUtility.NormalizeRuntimeEventData(eventData);
            if (!string.IsNullOrWhiteSpace(eventData?.Effect?.Dialogue?.Prompt))
            {
                return eventData.Effect.Dialogue.Prompt;
            }

            return ResolveEffectType(eventData) == ChapterEventEffectType.DialogueInteractionPrompt
                ? eventData?.Effect?.LegacyDmPrompt ?? string.Empty
                : string.Empty;
        }

        private static string GetEffectiveBattleDescription(ChapterGridEventData eventData)
        {
            eventData = ChapterEventDataStructureUtility.NormalizeRuntimeEventData(eventData);
            if (!string.IsNullOrWhiteSpace(eventData?.Effect?.Battle?.Description))
            {
                return eventData.Effect.Battle.Description;
            }

            return ResolveEffectType(eventData) == ChapterEventEffectType.StartBattle
                ? eventData?.Effect?.LegacyDmPrompt ?? string.Empty
                : string.Empty;
        }

        private void RefreshBindingSummary()
        {
            if (m_tmpBindingSummary == null)
            {
                return;
            }

            List<ChapterGridCoordinate> coordinates = m_request.GridCoordinates;
            if (coordinates != null && coordinates.Count > 1)
            {
                m_tmpBindingSummary.text = $"\u7ED1\u5B9A\u8303\u56F4\uFF1A\u5DF2\u9009\u4E2D {coordinates.Count} \u4E2A\u7F51\u683C";
                return;
            }

            ChapterGridCoordinate coordinate = coordinates != null && coordinates.Count == 1
                ? coordinates[0]
                : m_request.GridCoordinate;
            m_tmpBindingSummary.text = $"\u7ED1\u5B9A\u8303\u56F4\uFF1A\u5F53\u524D\u7F51\u683C ({coordinate.CellX}, {coordinate.CellY})";
        }

        private void BindSkillCheckInputComponents()
        {
            if (m_rectSkillCheckSection == null)
            {
                return;
            }

            m_skillCheckInputs.Clear();

            for (int index = 0; index < SkillCheckLabels.Length; index++)
            {
                TMP_InputField skillInput = BindRequiredComponent<TMP_InputField>(SkillCheckInputNodeNames[index]);
                if (skillInput != null)
                {
                    m_skillCheckInputs[SkillCheckLabels[index]] = skillInput;
                }
            }
        }

        private void ResetSkillCheckInputFields()
        {
            foreach (TMP_InputField inputField in m_skillCheckInputs.Values)
            {
                ResetInputField(inputField, TMP_InputField.LineType.SingleLine);
            }
        }

        private void ApplySkillCheckValues(ChapterGridEventData eventData)
        {
            eventData = ChapterEventDataStructureUtility.NormalizeRuntimeEventData(eventData);
            List<ChapterSkillCheckThresholdData> entries = GetEffectiveSkillCheckEntries(eventData);
            for (int index = 0; index < entries.Count; index++)
            {
                ChapterSkillCheckThresholdData entry = entries[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.SkillName))
                {
                    continue;
                }

                string normalizedSkillName = NormalizeSkillCheckName(entry.SkillName);
                if (m_skillCheckInputs.TryGetValue(normalizedSkillName, out TMP_InputField inputField))
                {
                    SetInputValue(inputField, entry.Threshold);
                }
            }
        }

        private List<ChapterSkillCheckThresholdData> BuildSkillCheckEntries()
        {
            List<ChapterSkillCheckThresholdData> entries = new List<ChapterSkillCheckThresholdData>();
            for (int index = 0; index < SkillCheckLabels.Length; index++)
            {
                string skillLabel = SkillCheckLabels[index];
                if (!m_skillCheckInputs.TryGetValue(skillLabel, out TMP_InputField inputField))
                {
                    continue;
                }

                string threshold = GetInputValue(inputField);
                if (string.IsNullOrWhiteSpace(threshold))
                {
                    continue;
                }

                entries.Add(new ChapterSkillCheckThresholdData
                {
                    SkillName = skillLabel,
                    Threshold = threshold,
                });
            }

            return entries;
        }

        private static List<ChapterSkillCheckThresholdData> GetEffectiveSkillCheckEntries(ChapterGridEventData eventData)
        {
            eventData = ChapterEventDataStructureUtility.NormalizeRuntimeEventData(eventData);
            List<ChapterSkillCheckThresholdData> entries = new List<ChapterSkillCheckThresholdData>();
            if (eventData == null)
            {
                return entries;
            }

            ChapterEventEffectData effectData = eventData.Effect ?? new ChapterEventEffectData();
            ChapterEventCheckEffectParamData checkEffectParam = effectData.Check ?? new ChapterEventCheckEffectParamData();
            if (checkEffectParam.SkillCheckEntries != null && checkEffectParam.SkillCheckEntries.Count > 0)
            {
                for (int index = 0; index < checkEffectParam.SkillCheckEntries.Count; index++)
                {
                    ChapterSkillCheckThresholdData entry = checkEffectParam.SkillCheckEntries[index];
                    if (entry == null)
                    {
                        continue;
                    }

                    entries.Add(new ChapterSkillCheckThresholdData
                    {
                        SkillName = NormalizeSkillCheckName(entry.SkillName),
                        Threshold = entry.Threshold ?? string.Empty,
                    });
                }

                return entries;
            }

            if (!string.IsNullOrWhiteSpace(checkEffectParam.SkillCheckName) || !string.IsNullOrWhiteSpace(checkEffectParam.SkillCheckThreshold))
            {
                entries.Add(new ChapterSkillCheckThresholdData
                {
                    SkillName = NormalizeSkillCheckName(checkEffectParam.SkillCheckName),
                    Threshold = checkEffectParam.SkillCheckThreshold ?? string.Empty,
                });
            }

            return entries;
        }

        private static string NormalizeSkillCheckName(string skillName)
        {
            switch ((skillName ?? string.Empty).Trim())
            {
                case "运动 Athletics":
                case "Athletics":
                case "运动":
                    return "运动";
                case "体操 Acrobatics":
                case "Acrobatics":
                case "体操":
                    return "体操";
                case "巧手 Sleight of Hand":
                case "Sleight of Hand":
                case "巧手":
                    return "巧手";
                case "隐匿 Stealth":
                case "Stealth":
                case "隐匿":
                    return "隐匿";
                case "奥秘 Arcana":
                case "Arcana":
                case "奥秘":
                case "奥秘 Acrana":
                case "Acrana":
                    return "奥秘";
                case "历史 History":
                case "History":
                case "历史":
                    return "历史";
                case "调查 Investigation":
                case "Investigation":
                case "调查":
                    return "调查";
                case "自然 Nature":
                case "Nature":
                case "自然":
                    return "自然";
                case "宗教 Religion":
                case "Religion":
                case "宗教":
                    return "宗教";
                case "驯兽 Animal Handling":
                case "Animal Handling":
                case "驯兽":
                    return "驯兽";
                case "洞悉 Insight":
                case "Insight":
                case "洞悉":
                    return "洞悉";
                case "医药 Medicine":
                case "Medicine":
                case "医药":
                    return "医药";
                case "察觉 Perception":
                case "Perception":
                case "察觉":
                    return "察觉";
                case "求生 Survival":
                case "Survival":
                case "求生":
                    return "求生";
                case "欺瞒 Deception":
                case "Deception":
                case "欺瞒":
                case "期满 Deception":
                case "期满":
                    return "欺瞒";
                case "威吓 Intimidation":
                case "Intimidation":
                case "威吓":
                    return "威吓";
                case "表演 Performance":
                case "Performance":
                case "表演":
                    return "表演";
                case "说服 Persuasion":
                case "Persuasion":
                case "说服":
                    return "说服";
                default:
                    return (skillName ?? string.Empty).Trim();
            }
        }

        private static RectTransform GetRectTransform(Component component)
        {
            return component != null ? component.transform as RectTransform : null;
        }

        private static float GetRectTop(RectTransform rectTransform)
        {
            return rectTransform != null ? -rectTransform.anchoredPosition.y : 0f;
        }

        private static float GetBottom(RectTransform rectTransform)
        {
            return GetRectTop(rectTransform) + GetCurrentHeight(rectTransform);
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

        private static void ShiftRectDown(RectTransform rectTransform, float offset)
        {
            if (rectTransform == null || Mathf.Approximately(offset, 0f))
            {
                return;
            }

            SetRectTop(rectTransform, GetRectTop(rectTransform) + offset);
        }

        private static void SetSiblingIndexAfter(Transform target, Transform sibling)
        {
            if (target == null || sibling == null || target.parent != sibling.parent)
            {
                return;
            }

            target.SetSiblingIndex(sibling.GetSiblingIndex() + 1);
        }

        private static float GetBottomAnchoredTopInset(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return 0f;
            }

            return rectTransform.anchoredPosition.y + (1f - rectTransform.pivot.y) * GetCurrentHeight(rectTransform);
        }

        private static void RefreshInputFieldHeight(TMP_InputField inputField, float minimumHeight)
        {
            if (inputField == null)
            {
                return;
            }

            RectTransform inputRect = inputField.transform as RectTransform;
            TMP_Text textComponent = inputField.textComponent;
            RectTransform textRect = textComponent != null ? textComponent.rectTransform : null;
            if (inputRect == null || textComponent == null || textRect == null)
            {
                return;
            }

            float textWidth = textRect.rect.width;
            if (textWidth <= 0f)
            {
                textWidth = Mathf.Max(0f, inputRect.rect.width > 0f ? inputRect.rect.width - 20f : inputRect.sizeDelta.x - 20f);
            }

            string displayText = string.IsNullOrEmpty(inputField.text)
                ? "\u200B"
                : inputField.text;
            textComponent.ForceMeshUpdate();
            Vector2 preferred = textComponent.GetPreferredValues(displayText, textWidth, 0f);
            float verticalPadding = Mathf.Max(0f, GetCurrentHeight(inputRect) - GetCurrentHeight(textRect));
            float targetHeight = Mathf.Max(minimumHeight, preferred.y + verticalPadding);
            SetRectHeight(inputRect, targetHeight);
        }

        private static float GetMinimumRectHeight(params RectTransform[] rectTransforms)
        {
            float minimumHeight = float.MaxValue;
            for (int index = 0; index < rectTransforms.Length; index++)
            {
                RectTransform rectTransform = rectTransforms[index];
                if (rectTransform == null)
                {
                    continue;
                }

                minimumHeight = Mathf.Min(minimumHeight, GetCurrentHeight(rectTransform));
            }

            return minimumHeight < float.MaxValue ? minimumHeight : 0f;
        }

        private static void SetInputValue(TMP_InputField inputField, string value)
        {
            if (inputField == null)
            {
                return;
            }

            inputField.text = value ?? string.Empty;
        }

        private static string GetInputValue(TMP_InputField inputField)
        {
            return inputField != null ? inputField.text?.Trim() ?? string.Empty : string.Empty;
        }

        private static string GetInputValue(TMP_InputField inputField, string fallback)
        {
            string value = GetInputValue(inputField);
            return string.IsNullOrWhiteSpace(value) ? fallback ?? string.Empty : value;
        }
    }

    internal static class RuntimeImageFileDialog
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        private const int HResultCancelled = unchecked((int)0x800704C7);

        [Flags]
        private enum FileOpenOptions : uint
        {
            FileMustExist = 0x00001000,
            PathMustExist = 0x00000800,
            ForceFileSystem = 0x00000040,
            NoChangeDir = 0x00000008
        }

        private enum Sigdn : uint
        {
            FileSystemPath = 0x80058000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct ComdlgFilterSpec
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszName;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszSpec;
        }

        [ComImport]
        [Guid("42f85136-db7e-439c-85f1-e4075d135fc8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IFileDialog
        {
            [PreserveSig]
            int Show(IntPtr parent);

            void SetFileTypes(uint cFileTypes, [MarshalAs(UnmanagedType.LPArray)] ComdlgFilterSpec[] rgFilterSpec);
            void SetFileTypeIndex(uint iFileType);
            void GetFileTypeIndex(out uint piFileType);
            void Advise(IntPtr pfde, out uint pdwCookie);
            void Unadvise(uint dwCookie);
            void SetOptions(FileOpenOptions fos);
            void GetOptions(out FileOpenOptions pfos);
            void SetDefaultFolder(IShellItem psi);
            void SetFolder(IShellItem psi);
            void GetFolder(out IShellItem ppsi);
            void GetCurrentSelection(out IShellItem ppsi);
            void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);
            void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
            void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);
            void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
            void GetResult(out IShellItem ppsi);
            void AddPlace(IShellItem psi, int fdap);
            void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
            void Close(int hr);
            void SetClientGuid(ref Guid guid);
            void ClearClientData();
            void SetFilter(IntPtr pFilter);
        }

        [ComImport]
        [Guid("d57c7288-d4ad-4768-be02-9d969532d960")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IFileOpenDialog : IFileDialog
        {
            [PreserveSig]
            new int Show(IntPtr parent);

            new void SetFileTypes(uint cFileTypes, [MarshalAs(UnmanagedType.LPArray)] ComdlgFilterSpec[] rgFilterSpec);
            new void SetFileTypeIndex(uint iFileType);
            new void GetFileTypeIndex(out uint piFileType);
            new void Advise(IntPtr pfde, out uint pdwCookie);
            new void Unadvise(uint dwCookie);
            new void SetOptions(FileOpenOptions fos);
            new void GetOptions(out FileOpenOptions pfos);
            new void SetDefaultFolder(IShellItem psi);
            new void SetFolder(IShellItem psi);
            new void GetFolder(out IShellItem ppsi);
            new void GetCurrentSelection(out IShellItem ppsi);
            new void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            new void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);
            new void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
            new void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);
            new void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
            new void GetResult(out IShellItem ppsi);
            new void AddPlace(IShellItem psi, int fdap);
            new void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
            new void Close(int hr);
            new void SetClientGuid(ref Guid guid);
            new void ClearClientData();
            new void SetFilter(IntPtr pFilter);
            void GetResults(out IntPtr ppenum);
            void GetSelectedItems(out IntPtr ppsai);
        }

        [ComImport]
        [Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItem
        {
            void BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, out IntPtr ppv);
            void GetParent(out IShellItem ppsi);
            void GetDisplayName(Sigdn sigdnName, out IntPtr ppszName);
            void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);
            void Compare(IShellItem psi, uint hint, out int piOrder);
        }

        [ComImport]
        [Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7")]
        private sealed class FileOpenDialogComObject
        {
        }
#endif

        public static string OpenImageFile(string dialogTitle = "选择图片文件")
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            IFileOpenDialog dialog = null!;
            IShellItem shellItem = null!;
            IntPtr displayNamePtr = IntPtr.Zero;
            try
            {
                Type dialogType = Type.GetTypeFromCLSID(new Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7"));
                object dialogObject = Activator.CreateInstance(dialogType);
                dialog = (IFileOpenDialog)dialogObject;
                ComdlgFilterSpec[] filters =
                {
                    new ComdlgFilterSpec { pszName = "图片文件", pszSpec = "*.png;*.jpg;*.jpeg" },
                    new ComdlgFilterSpec { pszName = "PNG 文件", pszSpec = "*.png" },
                    new ComdlgFilterSpec { pszName = "JPEG 文件", pszSpec = "*.jpg;*.jpeg" }
                };

                dialog.SetFileTypes((uint)filters.Length, filters);
                dialog.SetFileTypeIndex(1);
                dialog.SetDefaultExtension("png");
                dialog.SetTitle(dialogTitle);
                dialog.SetOptions(FileOpenOptions.ForceFileSystem | FileOpenOptions.FileMustExist | FileOpenOptions.PathMustExist | FileOpenOptions.NoChangeDir);

                int showResult = dialog.Show(IntPtr.Zero);
                if (showResult == HResultCancelled)
                {
                    return string.Empty;
                }

                Marshal.ThrowExceptionForHR(showResult);

                dialog.GetResult(out shellItem);
                shellItem.GetDisplayName(Sigdn.FileSystemPath, out displayNamePtr);
                return displayNamePtr != IntPtr.Zero ? Marshal.PtrToStringUni(displayNamePtr) ?? string.Empty : string.Empty;
            }
            catch (COMException exception)
            {
                Log.Error($"打开图片文件选择器失败: {exception.Message}");
                return string.Empty;
            }
            finally
            {
                if (displayNamePtr != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(displayNamePtr);
                }

                if (shellItem != null)
                {
                    Marshal.ReleaseComObject(shellItem);
                }

                if (dialog != null)
                {
                    Marshal.ReleaseComObject(dialog);
                }
            }
#else
        Log.Warning("Image upload is currently supported only in the Windows editor or Windows player.");
            return string.Empty;
#endif
        }
    }
}
