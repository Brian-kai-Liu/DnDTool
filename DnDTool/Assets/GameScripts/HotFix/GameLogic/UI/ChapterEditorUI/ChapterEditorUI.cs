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
        private const float MapZoomMinScale = 0.6f;
        private const float MapZoomMaxScale = 2.4f;
        private const float MapZoomScrollStep = 0.12f;
        private const float GridZoomMinScale = 0.1f;
        private const float GridZoomMaxScale = 10f;
        private const float GridZoomScrollStep = 0.02f;
        private const float FineZoomScrollStep = 0.005f;
        private const float GridLineThickness = 4f;
        private const float GridFrameThickness = 4f;
        private const float GridSelectionClickThreshold = 10f;
        private const float GridEventMarkerSize = 18f;
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
        private RectTransform m_btnDifficultTerrainRect = null!;
        private RectTransform m_btnImpassableTerrainRect = null!;
        private RectTransform m_btnClearTerrainRect = null!;
        private Image m_imgMapZoomToggle = null!;
        private Image m_imgGridZoomToggle = null!;
        private Image m_imgSaveChapterState = null!;
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
        private TMP_Text m_textDifficultTerrain = null!;
        private TMP_Text m_textImpassableTerrain = null!;
        private TMP_Text m_textClearTerrain = null!;
        private Texture2D m_mapPreviewTexture = null!;
        private Sprite m_mapPreviewSprite = null!;
        private string m_loadedMapImagePath = string.Empty;
        private int m_mapPreviewLoadVersion;
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
        private readonly List<RaycastResult> m_creatureBoardPointerHits = new List<RaycastResult>();
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

        protected override void OnCreate()
        {
            LoadChapterEditorState();
            m_canvas = gameObject.GetComponent<Canvas>();
            m_addChapterButtonRect = m_btnAddChapter != null ? m_btnAddChapter.GetComponent<RectTransform>() : null!;
            m_addChapterButtonImage = m_btnAddChapter != null ? m_btnAddChapter.targetGraphic as Image : null!;

            SetupChapterDetailPanel();
            SetupChapterList();
        }

        #region 事件

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

            if (m_dragPlaceholder != null)
            {
                Object.Destroy(m_dragPlaceholder.gameObject);
            }

            CleanupMapPreviewResources();
        }

        protected override void OnUpdate()
        {
            if (PopupWindowInputBlocker.ShouldBlockUnderlyingPointerInput())
            {
                m_isDraggingMap = false;
                m_isDraggingGrid = false;
                m_isDraggingLockedMapGrid = false;
                CancelPendingGridCellSelection();
                return;
            }

            HandleCreatureBoardPointerProbe();

            if (m_rectMapPreview == null || !m_rectMapPreview.gameObject.activeInHierarchy)
            {
                m_isDraggingMap = false;
                m_isDraggingGrid = false;
                m_isDraggingLockedMapGrid = false;
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

            HandleGridCellSelection();

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
                Log.Warning("生物区域点击探针: 当前场景不存在 EventSystem。");
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
                Creatures = new List<ChapterCreatureData>(),
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
                    Creatures = chapter.Creatures != null
                        ? new List<ChapterCreatureData>(chapter.Creatures)
                        : new List<ChapterCreatureData>(),
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

            ConfigureChapterTextInput(m_tmpInputChapterGoal, "请输入章节目标", 400);
            ConfigureChapterTextInput(m_tmpInputStoryContent, "请输入章节内容", 0);
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
                    ? "可按名称快速筛选，也可以录入新的生物卡片。"
                    : $"名称筛选: {m_creatureSearchKeyword}";
            }

            RefreshCreatureFilteredCardIndices();
            EnsureSelectedCreatureCardVisible();

            if (m_rectCreatureCardContainer != null && m_rectCreatureCardTemplate != null)
            {
                EnsureCreatureCardViews(m_creatureFilteredCardIndices.Count);
                for (int index = 0; index < m_creatureCardWidgets.Count; index++)
                {
                    int actualCardIndex = m_creatureFilteredCardIndices[index];
                    m_creatureCardWidgets[index].SetVisible(true);
                    m_creatureCardWidgets[index].Bind(m_creatureAllCards[actualCardIndex], actualCardIndex == m_selectedCreatureCardIndex, () => SelectCreatureCard(actualCardIndex));
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
            Log.Info("打开生物信息录入弹窗。");
            GameModule.UI.ShowUIAsync<ChapterCreatureEntryPopupUI>(new ChapterCreatureEntryPopupRequest
            {
                OnConfirm = AppendRuntimeCreature
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
                ShowSaveFeedbackAsync("请先选中至少一个网格后再添加事件", false).Forget();
                return;
            }

            ChapterGridEventData existingEventData = null;
            if (coordinates.Count == 1)
            {
                ChapterGridCellCollectionUtility.TryGetEventData(selectedChapter.GridCells, coordinates[0], out existingEventData);
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
                ShowSaveFeedbackAsync("请先选中至少一个网格后再删除事件", false).Forget();
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

            for (int index = 0; index < coordinates.Count; index++)
            {
                ChapterGridCellCollectionUtility.UpsertEventData(chapter.GridCells, coordinates[index], eventData);
            }

            ChapterGridCellCollectionUtility.ClearSelectedMarks(chapter.GridCells, coordinates);

            if (chapterId == m_selectedChapterId)
            {
                RefreshGridSelectionHighlights();
            }

            SaveChapterEditorState();
            ShowSaveFeedbackAsync(coordinates.Count > 1 ? $"已为 {coordinates.Count} 个网格记录事件" : "事件已记录", true).Forget();
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

            int removedCount = 0;
            for (int index = 0; index < coordinates.Count; index++)
            {
                if (ChapterGridCellCollectionUtility.RemoveEventData(chapter.GridCells, coordinates[index]))
                {
                    removedCount++;
                }
            }

            if (removedCount <= 0)
            {
                ShowSaveFeedbackAsync("目标网格没有事件可删除", false).Forget();
                return;
            }

            ChapterGridCellCollectionUtility.ClearSelectedMarks(chapter.GridCells, coordinates);

            if (chapterId == m_selectedChapterId)
            {
                RefreshGridSelectionHighlights();
            }

            SaveChapterEditorState();
            ShowSaveFeedbackAsync(removedCount > 1 ? $"已移除 {removedCount} 个网格的事件" : "事件已移除", true).Forget();
        }

        private static bool HasGridEventAtAnyCoordinate(List<ChapterGridCellData> gridCells, List<ChapterGridCoordinate> coordinates)
        {
            if (gridCells == null || coordinates == null)
            {
                return false;
            }

            for (int index = 0; index < coordinates.Count; index++)
            {
                if (ChapterGridCellCollectionUtility.HasEventAtCoordinate(gridCells, coordinates[index]))
                {
                    return true;
                }
            }

            return false;
        }

        private void AppendRuntimeCreature(ChapterCreatureData creatureData)
        {
            m_creatureRuntimeCards.Add(new ChapterCreatureStaticCardData(creatureData));
            RebuildCreatureBrowserCards();
            SetCreatureSearchKeyword(string.Empty);
            m_selectedCreatureCardIndex = m_creatureAllCards.Count - 1;
            RedrawCreatureBrowserPreview();
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
                m_selectedCreatureCardIndex = m_creatureFilteredCardIndices[0];
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
                m_creatureCardWidgets[index].Bind(m_creatureAllCards[actualCardIndex], actualCardIndex == m_selectedCreatureCardIndex, () => SelectCreatureCard(actualCardIndex));
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
            detailObject.SetActive(true);
            m_creatureDetailWidget = new ChapterCreatureDetailWidget(detailObject);
        }

        private void RefreshCreatureDetailPanel()
        {
            if (m_creatureDetailWidget == null)
            {
                return;
            }

            bool hasCards = m_creatureAllCards.Count > 0 && m_selectedCreatureCardIndex >= 0 && m_selectedCreatureCardIndex < m_creatureAllCards.Count;
            m_creatureDetailWidget.SetVisible(hasCards);
            if (hasCards)
            {
                m_creatureDetailWidget.Bind(m_creatureAllCards[m_selectedCreatureCardIndex]);
            }
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
                m_selectedCreatureCardIndex = m_creatureAllCards.Count > 0 ? 0 : -1;
                return;
            }

            int selectedIndex = m_creatureAllCards.FindIndex(card => card.Equals(selectedCard.Value));
            m_selectedCreatureCardIndex = selectedIndex >= 0 ? selectedIndex : (m_creatureAllCards.Count > 0 ? 0 : -1);
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
            ShowSaveFeedbackAsync(saved ? "已保存" : "保存失败", saved).Forget();
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

            if (m_tmpMapUploadHint != null)
            {
                m_tmpMapUploadHint.gameObject.SetActive(hasSelectedChapter && !hasMap);
                m_tmpMapUploadHint.text = m_defaultMapUploadHintText;
            }

            UpdateUploadMapButtonLayout(hasMap);
            UpdateMapZoomToggleButtonLayout();
            UpdateGridZoomToggleButtonLayout();
            UpdateSaveChapterStateButtonLayout();
            UpdateMapZoomToggleVisual(hasMap);
            UpdateGridZoomToggleVisual(hasSelectedChapter);
            UpdateSaveChapterStateButtonVisual(hasSelectedChapter);
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
                    Log.Error("章节地图加载失败，文件不是有效图片。");
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
                return;
            }

            UpdateGridEventActionButtons();
            UpdateTerrainToolButtonState();

            ChapterListItemData selectedChapter = GetSelectedChapterData();
            if (selectedChapter == null || selectedChapter.GridCells == null || selectedChapter.GridCells.Count == 0)
            {
                SetGridSelectionHighlightCount(0);
                SetGridEventMarkerCount(0);
                return;
            }

            if (!TryGetCurrentGridMetrics(out ChapterMapGridMetrics metrics))
            {
                SetGridSelectionHighlightCount(0);
                SetGridEventMarkerCount(0);
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
            List<ChapterGridCellData> visibleEventCells = GetVisibleGridCellsByMarkType(selectedChapter.GridCells, ChapterGridCellMarkType.Event, metrics, minX, maxX, minY, maxY);

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

            RefreshGridEventMarkers(visibleEventCells, metrics);
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

        private void RefreshGridEventMarkers(List<ChapterGridCellData> visibleEventCells, ChapterMapGridMetrics metrics)
        {
            SetGridEventMarkerCount(visibleEventCells?.Count ?? 0);
            if (visibleEventCells == null)
            {
                return;
            }

            for (int index = 0; index < visibleEventCells.Count; index++)
            {
                ChapterGridCellData gridCell = visibleEventCells[index];
                Rect cellRect = ChapterMapGridUtility.GetLogicalCellRect(metrics, gridCell.Coordinate);
                Image marker = m_gridEventMarkers[index];
                RectTransform rectTransform = marker.rectTransform;
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = new Vector2(cellRect.center.x, cellRect.center.y);
                rectTransform.sizeDelta = new Vector2(GridEventMarkerSize, GridEventMarkerSize);
                rectTransform.localScale = Vector3.one;
                marker.color = new Color(0.95f, 0.65f, 0.18f, 0.96f);
                marker.gameObject.SetActive(true);
            }
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

            m_btnSaveChapterStateRect.anchorMin = new Vector2(0f, 1f);
            m_btnSaveChapterStateRect.anchorMax = new Vector2(0f, 1f);
            m_btnSaveChapterStateRect.pivot = new Vector2(0f, 1f);
            m_btnSaveChapterStateRect.anchoredPosition = new Vector2(20f, -120f);
            m_btnSaveChapterStateRect.sizeDelta = new Vector2(156f, 42f);
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
                m_textMapZoomToggle.text = m_isMapZoomEnabled ? "地图编辑: 开" : "地图编辑: 关";
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
                m_textGridZoomToggle.text = m_isGridZoomEnabled ? "网格编辑: 开" : "网格编辑: 关";
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
                if (!IsMouseOverMapPreview() || IsMouseOverMapControlButton())
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
                if (!IsMouseOverMapPreview() || IsMouseOverMapControlButton())
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
                if (!IsMouseOverMapPreview() || IsMouseOverMapControlButton())
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
            if (!IsMouseOverMapPreview() || IsMouseOverMapControlButton())
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
            if (!IsMouseOverMapPreview() || IsMouseOverMapControlButton())
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
            ChapterGridCellCollectionUtility.TryGetEventData(selectedChapter.GridCells, m_gridCellEventPopupMouseDownCoordinate, out ChapterGridEventData existingEventData);
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
                || IsMouseOverTerrainToolButtons();
        }

        private void UpdateGridEventActionButtons()
        {
            ChapterListItemData selectedChapter = GetSelectedChapterData();
            bool hasSelectedChapter = selectedChapter != null;
            bool hasSelectedEventCells = false;
            if (hasSelectedChapter && TryGetSelectedGridCoordinates(selectedChapter, out List<ChapterGridCoordinate> coordinates))
            {
                hasSelectedEventCells = HasGridEventAtAnyCoordinate(selectedChapter.GridCells, coordinates);
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
                    return hasSelectedCells ? "标记为困难地形" : "困难地形";
                case TerrainToolButtonType.ImpassableTerrain:
                    return hasSelectedCells ? "标记为不可通过" : "不可通过地形";
                case TerrainToolButtonType.ClearTerrain:
                    return hasSelectedCells ? "清除已选地形" : "清除地形";
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
            if (m_rectMapPreview == null)
            {
                localPoint = Vector2.zero;
                return false;
            }

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(m_rectMapPreview, Input.mousePosition, GetMapPreviewEventCamera(), out localPoint);
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

        public ChapterGridEventData ExistingEventData { get; set; }

        public Action<ChapterGridEventData> OnConfirm { get; set; }
    }

    [Window(UILayer.Top, location : "ChapterEventPopupUI", fullScreen : false)]
    public sealed class ChapterEventPopupUI : UIWindow
    {
        private const string PopupPanelPathPrefix = "Panel/";

        private enum ChapterEventCategory
        {
            Check = 0,
            DmDirect = 1,
        }

        private enum ChapterDmEventSubType
        {
            Story = 0,
            Dialogue = 1,
            Choice = 2,
            Interaction = 3,
            Combat = 4,
            Exploration = 5,
            AreaEnter = 6,
            TimeAdvance = 7,
            Random = 8,
            Special = 9,
        }

        private enum ChapterEventTriggerMode
        {
            Automatic,
            DmManual,
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

        private static readonly string[] EventCategoryLabels =
        {
            "检定类事件",
            "DM 判断类事件",
        };

        private static readonly string[] DmEventSubTypeLabels =
        {
            "剧情事件",
            "对话事件",
            "选择事件",
            "交互事件",
            "战斗事件",
            "探索事件",
            "区域进入事件",
            "时间推进事件",
            "随机事件",
            "特殊事件",
        };

        private static readonly string[] TriggerModeLabels =
        {
            "自动触发",
            "DM 判断",
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
            "运动 Athletics",
            "体操 Acrobatics",
            "巧手 Sleight of Hand",
            "隐匿 Stealth",
            "奥秘 Arcana",
            "历史 History",
            "调查 Investigation",
            "自然 Nature",
            "宗教 Religion",
            "驯兽 Animal Handling",
            "洞悉 Insight",
            "医药 Medicine",
            "察觉 Perception",
            "求生 Survival",
            "欺瞒 Deception",
            "威吓 Intimidation",
            "表演 Performance",
            "说服 Persuasion",
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

        private TextMeshProUGUI m_tmpTitle = null!;
        private Button m_btnClose = null!;
        private TMP_Dropdown m_tmpDropdownEventCategory = null!;
        private TMP_Dropdown m_tmpDropdownDmEventSubType = null!;
        private TMP_Dropdown m_tmpDropdownTriggerMode = null!;
        private TMP_Dropdown m_tmpDropdownCheckTargetMode = null!;
        private TMP_Dropdown m_tmpDropdownCheckResolutionMode = null!;
        private Button m_btnConfirm = null!;
        private RectTransform m_rectAbilityCheckSection = null!;
        private RectTransform m_rectSkillCheckSection = null!;
        private RectTransform m_rectDmDirectSection = null!;
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
        private TMP_InputField m_tmpInputDmPrompt = null!;
        private ChapterEventPopupRequest m_request = null!;
        private ChapterEventCategory m_eventCategory = ChapterEventCategory.Check;
        private ChapterDmEventSubType m_dmEventSubType = ChapterDmEventSubType.Story;
        private ChapterEventTriggerMode m_triggerMode = ChapterEventTriggerMode.Automatic;
        private ChapterCheckTargetMode m_checkTargetMode = ChapterCheckTargetMode.Ability;
        private ChapterCheckResolutionMode m_checkResolutionMode = ChapterCheckResolutionMode.RollDice;
        private readonly Dictionary<string, TMP_InputField> m_skillCheckInputs = new Dictionary<string, TMP_InputField>(StringComparer.Ordinal);

        protected override void OnCreate()
        {
            PopupWindowPresentationHelper.Configure(this);
        }

        protected override void ScriptGenerator()
        {
            m_tmpTitle = BindRequiredComponent<TextMeshProUGUI>("m_tmpTitle");
            m_btnClose = BindRequiredComponent<Button>("m_btnClose");
            m_tmpDropdownEventCategory = BindRequiredComponent<TMP_Dropdown>("m_tmpDropdownEventCategory");
            m_tmpDropdownDmEventSubType = BindRequiredComponent<TMP_Dropdown>("m_rectDmDirectSection/m_tmpDropdownDmEventSubType");
            m_tmpDropdownTriggerMode = BindRequiredComponent<TMP_Dropdown>("m_tmpDropdownTriggerMode");
            m_tmpDropdownCheckTargetMode = BindRequiredComponent<TMP_Dropdown>("m_tmpDropdownCheckTargetMode");
            m_tmpDropdownCheckResolutionMode = BindRequiredComponent<TMP_Dropdown>("m_tmpDropdownCheckResolutionMode");
            m_btnConfirm = BindRequiredComponent<Button>("m_btnConfirm");
            m_rectAbilityCheckSection = BindRequiredComponent<RectTransform>("m_rectAbilityCheckSection");
            m_rectSkillCheckSection = BindRequiredComponent<RectTransform>("m_rectSkillCheckSection");
            m_rectDmDirectSection = BindRequiredComponent<RectTransform>("m_rectDmDirectSection");
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
            m_tmpInputDmPrompt = BindRequiredComponent<TMP_InputField>("m_rectDmDirectSection/m_tmpInputDmPrompt");
            BindSkillCheckInputComponents();

            if (!HasRequiredBindings())
            {
                return;
            }

            m_btnClose.onClick.RemoveAllListeners();
            m_btnClose.onClick.AddListener(OnClickCloseBtn);
            SetupDropdown(m_tmpDropdownEventCategory, EventCategoryLabels, OnEventCategoryDropdownChanged);
            SetupDropdown(m_tmpDropdownDmEventSubType, DmEventSubTypeLabels, OnDmEventSubTypeDropdownChanged);
            SetupDropdown(m_tmpDropdownTriggerMode, TriggerModeLabels, OnTriggerModeDropdownChanged);
            SetupDropdown(m_tmpDropdownCheckTargetMode, CheckTargetModeLabels, OnCheckTargetModeDropdownChanged);
            SetupDropdown(m_tmpDropdownCheckResolutionMode, CheckResolutionModeLabels, OnCheckResolutionModeDropdownChanged);
            m_btnConfirm.onClick.RemoveAllListeners();
            m_btnConfirm.onClick.AddListener(OnClickConfirmBtn);
        }

        protected override void OnRefresh()
        {
            m_request = UserData as ChapterEventPopupRequest ?? new ChapterEventPopupRequest();
            m_eventCategory = ChapterEventCategory.Check;
            m_dmEventSubType = ChapterDmEventSubType.Story;
            m_triggerMode = ChapterEventTriggerMode.Automatic;
            m_checkTargetMode = ChapterCheckTargetMode.Ability;
            m_checkResolutionMode = ChapterCheckResolutionMode.RollDice;
            ResetSkillCheckInputFields();
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
            ResetInputField(m_tmpInputDmPrompt, TMP_InputField.LineType.MultiLineNewline);

            ApplyExistingEventData(m_request.ExistingEventData);

            RefreshView();
        }

        private void OnEventCategoryDropdownChanged(int index)
        {
            m_eventCategory = (ChapterEventCategory) Mathf.Clamp(index, 0, EventCategoryLabels.Length - 1);
            RefreshView();
        }

        private void OnDmEventSubTypeDropdownChanged(int index)
        {
            m_dmEventSubType = (ChapterDmEventSubType) Mathf.Clamp(index, 0, DmEventSubTypeLabels.Length - 1);
        }

        private void OnTriggerModeDropdownChanged(int index)
        {
            m_triggerMode = (ChapterEventTriggerMode) Mathf.Clamp(index, 0, TriggerModeLabels.Length - 1);
            RefreshView();
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

        private void OnClickConfirmBtn()
        {
            ChapterGridEventData eventData = BuildEventData();
            string abilityThresholdSummary = BuildAbilityThresholdSummary();
            int affectedCellCount = m_request.GridCoordinates != null && m_request.GridCoordinates.Count > 0
                ? m_request.GridCoordinates.Count
                : 1;
            Log.Info(
                $"[ChapterEventPopupUI] 已记录格子事件。章节={m_request.ChapterId}, 目标格数={affectedCellCount}, 首格坐标={m_request.GridCoordinate}, 事件类别={EventCategoryLabels[(int) m_eventCategory]}, 标题={GetInputValue(m_tmpInputEventTitle)}{abilityThresholdSummary}");
            m_request.OnConfirm?.Invoke(eventData);
            Close();
        }

        private void RefreshView()
        {
            bool isCheckEvent = m_eventCategory == ChapterEventCategory.Check;
            bool isDmDirectEvent = m_eventCategory == ChapterEventCategory.DmDirect;
            bool isAbilityCheck = isCheckEvent && m_checkTargetMode == ChapterCheckTargetMode.Ability;
            bool isSkillCheck = isCheckEvent && m_checkTargetMode == ChapterCheckTargetMode.Skill;

            if (m_tmpTitle != null)
            {
                bool isBatchEdit = m_request.GridCoordinates != null && m_request.GridCoordinates.Count > 1;
                m_tmpTitle.text = isBatchEdit
                    ? $"批量标记事件 ({m_request.GridCoordinates.Count})"
                    : m_request.ExistingEventData != null ? "编辑事件" : "添加事件";
            }

            SetDropdownValue(m_tmpDropdownEventCategory, (int) m_eventCategory);
            SetDropdownValue(m_tmpDropdownDmEventSubType, (int) m_dmEventSubType);
            SetDropdownValue(m_tmpDropdownTriggerMode, (int) m_triggerMode);
            SetDropdownValue(m_tmpDropdownCheckTargetMode, (int) m_checkTargetMode);
            SetDropdownValue(m_tmpDropdownCheckResolutionMode, (int) m_checkResolutionMode);
            SetButtonLabel(m_btnConfirm, "确定");

            if (m_rectDmDirectSection != null)
            {
                m_rectDmDirectSection.gameObject.SetActive(isDmDirectEvent);
            }

            if (m_tmpDropdownCheckTargetMode != null)
            {
                m_tmpDropdownCheckTargetMode.gameObject.SetActive(isCheckEvent);
            }

            if (m_tmpDropdownCheckResolutionMode != null)
            {
                m_tmpDropdownCheckResolutionMode.gameObject.SetActive(isCheckEvent);
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
                m_tmpInputSuccessResult.gameObject.SetActive(isCheckEvent);
            }

            if (m_tmpInputFailureResult != null)
            {
                m_tmpInputFailureResult.gameObject.SetActive(isCheckEvent);
            }
        }

        private string BuildAbilityThresholdSummary()
        {
            if (m_eventCategory != ChapterEventCategory.Check)
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

                return $", 技能检定=[{string.Join(", ", summarySegments)}]";
            }

            if (m_checkTargetMode != ChapterCheckTargetMode.Ability)
            {
                return string.Empty;
            }

            return $", 属性通过值=[STR:{GetInputValue(m_tmpInputAbilityStrength)}, DEX:{GetInputValue(m_tmpInputAbilityDexterity)}, CON:{GetInputValue(m_tmpInputAbilityConstitution)}, INT:{GetInputValue(m_tmpInputAbilityIntelligence)}, WIS:{GetInputValue(m_tmpInputAbilityWisdom)}, CHA:{GetInputValue(m_tmpInputAbilityCharisma)}]";
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
                && m_tmpDropdownEventCategory != null
                && m_tmpDropdownDmEventSubType != null
                && m_tmpDropdownTriggerMode != null
                && m_tmpDropdownCheckTargetMode != null
                && m_tmpDropdownCheckResolutionMode != null
                && m_btnConfirm != null
                && m_rectAbilityCheckSection != null
                && m_rectSkillCheckSection != null
                && m_rectDmDirectSection != null
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
                && m_tmpInputDmNote != null
                && m_tmpInputDmPrompt != null;
        }

        private void ApplyExistingEventData(ChapterGridEventData eventData)
        {
            if (eventData == null)
            {
                return;
            }

            m_eventCategory = (ChapterEventCategory) Mathf.Clamp(eventData.EventCategory, 0, EventCategoryLabels.Length - 1);
            m_dmEventSubType = (ChapterDmEventSubType) Mathf.Clamp(eventData.EventSubType, 0, DmEventSubTypeLabels.Length - 1);
            m_triggerMode = (ChapterEventTriggerMode) Mathf.Clamp(eventData.TriggerMode, 0, TriggerModeLabels.Length - 1);
            m_checkTargetMode = (ChapterCheckTargetMode) Mathf.Clamp(eventData.CheckTargetMode, 0, CheckTargetModeLabels.Length - 1);
            m_checkResolutionMode = (ChapterCheckResolutionMode) Mathf.Clamp(eventData.CheckResolutionMode, 0, CheckResolutionModeLabels.Length - 1);
            SetInputValue(m_tmpInputEventTitle, eventData.EventTitle);
            SetInputValue(m_tmpInputTriggerDescription, eventData.TriggerDescription);
            SetInputValue(m_tmpInputSuccessResult, eventData.SuccessResult);
            SetInputValue(m_tmpInputFailureResult, eventData.FailureResult);
            SetInputValue(m_tmpInputDmNote, eventData.DmNote);
            SetInputValue(m_tmpInputDmPrompt, eventData.DmPrompt);
            ApplySkillCheckValues(eventData);
            SetInputValue(m_tmpInputAbilityStrength, eventData.AbilityStrengthThreshold);
            SetInputValue(m_tmpInputAbilityDexterity, eventData.AbilityDexterityThreshold);
            SetInputValue(m_tmpInputAbilityConstitution, eventData.AbilityConstitutionThreshold);
            SetInputValue(m_tmpInputAbilityIntelligence, eventData.AbilityIntelligenceThreshold);
            SetInputValue(m_tmpInputAbilityWisdom, eventData.AbilityWisdomThreshold);
            SetInputValue(m_tmpInputAbilityCharisma, eventData.AbilityCharismaThreshold);
        }

        private ChapterGridEventData BuildEventData()
        {
            List<ChapterSkillCheckThresholdData> skillCheckEntries = BuildSkillCheckEntries();
            ChapterSkillCheckThresholdData primarySkillCheckEntry = skillCheckEntries.Count > 0 ? skillCheckEntries[0] : null;

            return new ChapterGridEventData
            {
                EventCategory = (int) m_eventCategory,
                EventSubType = m_eventCategory == ChapterEventCategory.DmDirect ? (int) m_dmEventSubType : 0,
                TriggerMode = (int) m_triggerMode,
                CheckTargetMode = (int) m_checkTargetMode,
                CheckResolutionMode = (int) m_checkResolutionMode,
                EventTitle = GetInputValue(m_tmpInputEventTitle),
                TriggerDescription = GetInputValue(m_tmpInputTriggerDescription),
                SuccessResult = m_eventCategory == ChapterEventCategory.Check ? GetInputValue(m_tmpInputSuccessResult) : string.Empty,
                FailureResult = m_eventCategory == ChapterEventCategory.Check ? GetInputValue(m_tmpInputFailureResult) : string.Empty,
                DmNote = GetInputValue(m_tmpInputDmNote),
                DmPrompt = m_eventCategory == ChapterEventCategory.DmDirect ? GetInputValue(m_tmpInputDmPrompt) : string.Empty,
                SkillCheckEntries = skillCheckEntries,
                SkillCheckName = primarySkillCheckEntry != null ? primarySkillCheckEntry.SkillName : string.Empty,
                SkillCheckThreshold = primarySkillCheckEntry != null ? primarySkillCheckEntry.Threshold : string.Empty,
                AbilityStrengthThreshold = GetInputValue(m_tmpInputAbilityStrength),
                AbilityDexterityThreshold = GetInputValue(m_tmpInputAbilityDexterity),
                AbilityConstitutionThreshold = GetInputValue(m_tmpInputAbilityConstitution),
                AbilityIntelligenceThreshold = GetInputValue(m_tmpInputAbilityIntelligence),
                AbilityWisdomThreshold = GetInputValue(m_tmpInputAbilityWisdom),
                AbilityCharismaThreshold = GetInputValue(m_tmpInputAbilityCharisma),
            };
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
            List<ChapterSkillCheckThresholdData> entries = GetEffectiveSkillCheckEntries(eventData);
            for (int index = 0; index < entries.Count; index++)
            {
                ChapterSkillCheckThresholdData entry = entries[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.SkillName))
                {
                    continue;
                }

                if (m_skillCheckInputs.TryGetValue(entry.SkillName, out TMP_InputField inputField))
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
            List<ChapterSkillCheckThresholdData> entries = new List<ChapterSkillCheckThresholdData>();
            if (eventData == null)
            {
                return entries;
            }

            if (eventData.SkillCheckEntries != null && eventData.SkillCheckEntries.Count > 0)
            {
                for (int index = 0; index < eventData.SkillCheckEntries.Count; index++)
                {
                    ChapterSkillCheckThresholdData entry = eventData.SkillCheckEntries[index];
                    if (entry == null)
                    {
                        continue;
                    }

                    entries.Add(new ChapterSkillCheckThresholdData
                    {
                        SkillName = entry.SkillName ?? string.Empty,
                        Threshold = entry.Threshold ?? string.Empty,
                    });
                }

                return entries;
            }

            if (!string.IsNullOrWhiteSpace(eventData.SkillCheckName) || !string.IsNullOrWhiteSpace(eventData.SkillCheckThreshold))
            {
                entries.Add(new ChapterSkillCheckThresholdData
                {
                    SkillName = eventData.SkillCheckName ?? string.Empty,
                    Threshold = eventData.SkillCheckThreshold ?? string.Empty,
                });
            }

            return entries;
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
        Log.Warning("图片上传当前仅支持 Windows 编辑器或 Windows 打包程序。");
            return string.Empty;
#endif
        }
    }
}