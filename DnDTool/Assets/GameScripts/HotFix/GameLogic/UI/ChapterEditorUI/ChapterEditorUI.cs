using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using TMPro;
using TEngine;
using UnityEngine;
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
        private const string ChapterEditorSaveFileName = "chapter_editor_state.json";

        private static readonly ChapterCreatureStaticCardData[] StaticCreatureCards =
        {
            new ChapterCreatureStaticCardData("Goblin Scout", "Humanoid (Goblinoid)", "Neutral Evil", new Color(0.35f, 0.57f, 0.28f, 1f), "擅长伏击与游击的小型生物，适合用来填充低等级遭遇。首版静态版中用于展示基础卡片布局与详情区域排版。"),
            new ChapterCreatureStaticCardData("Orc Raider", "Humanoid (Orc)", "Chaotic Evil", new Color(0.52f, 0.34f, 0.22f, 1f), "近战压迫感强，适合作为正面推进敌人。可用于测试卡片在较长名称与多行说明下的视觉稳定性。"),
            new ChapterCreatureStaticCardData("Skeleton Archer", "Undead", "Lawful Evil", new Color(0.56f, 0.56f, 0.62f, 1f), "远程持续输出型生物，适合验证类型、阵营与详情摘要在窄宽度下的可读性。"),
            new ChapterCreatureStaticCardData("Owlbear", "Monstrosity", "Unaligned", new Color(0.66f, 0.48f, 0.24f, 1f), "高辨识度中型威胁单位。这里使用较强的色块预览，方便后续替换成真实怪物立绘资源。"),
            new ChapterCreatureStaticCardData("Young Red Dragon", "Dragon", "Chaotic Evil", new Color(0.7f, 0.24f, 0.18f, 1f), "高威胁首领型怪物，适合检验详情面板在标题较长时的排版与层次。"),
            new ChapterCreatureStaticCardData("Gelatinous Cube", "Ooze", "Unaligned", new Color(0.24f, 0.56f, 0.54f, 1f), "特殊机制型怪物，用于验证怪物类型、阵营与摘要在静态原型中的通用展示效果。"),
        };

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
        private TMP_Text m_textAddMapHint = null!;
        private TMP_Text m_textUploadMapButton = null!;
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
        private Image m_imgDifficultTerrain = null!;
        private Image m_imgImpassableTerrain = null!;
        private Image m_imgClearTerrain = null!;
        private RectTransform m_rectMapSurface = null!;
        private GameObject m_goCreatureBrowserRoot = null!;
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
        private Vector2 m_lastMapDragLocalPoint;
        private Vector2 m_lastGridDragLocalPoint;
        private Vector2 m_lastLockedMapGridDragLocalPoint;
        private Vector2 m_gridCellSelectionMouseDownLocalPoint;
        private ChapterGridCoordinate m_gridCellSelectionMouseDownCoordinate = ChapterGridCoordinate.Zero;

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
                GridCells = new List<ChapterGridCellData>()
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
                    GridCells = ChapterGridCellCollectionUtility.Clone(chapter.GridCells)
                });
            }

            return result;
        }

        private void SetupChapterDetailPanel()
        {
            m_rectMapSurface = m_imgMapSurface != null ? m_imgMapSurface.rectTransform : null!;
            m_goCreatureBrowserRoot = m_rectCreatureBrowserRoot != null ? m_rectCreatureBrowserRoot.gameObject : null!;

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

            if (m_btnOpenCheckPopup != null)
            {
                m_btnOpenCheckPopup.gameObject.SetActive(hasSelectedChapter);
                m_btnOpenCheckPopup.interactable = hasSelectedChapter && !m_isDraggingChapter;
            }

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

            RefreshCreatureBrowserPreview(StaticCreatureCards);

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

                RefreshCreatureBrowserPreview(StaticCreatureCards);
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

            GameModule.UI.ShowUIAsync<ChapterEventPopupUI>(new ChapterEventPopupRequest
            {
                ChapterId = selectedChapter.Id,
                ChapterName = selectedChapter.Name ?? string.Empty
            });
        }

        private void AppendRuntimeCreature(ChapterCreatureStaticCardData creature)
        {
            m_creatureRuntimeCards.Add(creature);
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

            UpdateTerrainToolButtonState();

            ChapterListItemData selectedChapter = GetSelectedChapterData();
            if (selectedChapter == null || selectedChapter.GridCells == null || selectedChapter.GridCells.Count == 0)
            {
                SetGridSelectionHighlightCount(0);
                return;
            }

            if (!TryGetCurrentGridMetrics(out ChapterMapGridMetrics metrics))
            {
                SetGridSelectionHighlightCount(0);
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
                return;
            }

            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

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
            m_gridCellSelectionMouseDownCoordinate = ChapterGridCoordinate.Zero;
        }

        private bool IsMouseOverMapControlButton()
        {
            return IsMouseOverRectTransform(m_btnUploadMapRect)
                || IsMouseOverRectTransform(m_btnMapZoomToggleRect)
                || IsMouseOverRectTransform(m_btnGridZoomToggleRect)
                || IsMouseOverRectTransform(m_btnSaveChapterStateRect)
                || IsMouseOverTerrainToolButtons();
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

        private static T FindChildComponentByName<T>(Transform root, string childName) where T : Component
        {
            Transform targetTransform = FindChildTransformByName(root, childName);
            return targetTransform != null ? targetTransform.GetComponent<T>() : null!;
        }

        private static Transform FindChildTransformByName(Transform root, string childName)
        {
            if (root == null)
            {
                return null!;
            }

            Queue<Transform> transforms = new Queue<Transform>();
            transforms.Enqueue(root);
            while (transforms.Count > 0)
            {
                Transform current = transforms.Dequeue();
                if (current.name == childName)
                {
                    return current;
                }

                for (int index = 0; index < current.childCount; index++)
                {
                    transforms.Enqueue(current.GetChild(index));
                }
            }

            return null!;
        }
    }

    internal sealed class ChapterEventPopupRequest
    {
        public int ChapterId { get; set; }

        public string ChapterName { get; set; } = string.Empty;
    }

    [Window(UILayer.Top, location : "ChapterEventPopupUI", fullScreen : true)]
    public sealed class ChapterEventPopupUI : UIWindow
    {
        private enum ChapterEventType
        {
            Story,
            Dialogue,
            Check,
            Choice,
            Interaction,
            Combat,
            Exploration,
            AreaEnter,
            TimeAdvance,
            Random,
            Special,
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
        }

        private enum ChapterCheckResolutionMode
        {
            RollDice,
            DmDirect,
        }

        private static readonly string[] EventTypeLabels =
        {
            "剧情事件",
            "对话事件",
            "检定事件",
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
        };

        private static readonly string[] CheckResolutionModeLabels =
        {
            "掷骰判定",
            "DM 直接判定",
        };

        private UIBindComponent m_bindComponent = null!;
        private TextMeshProUGUI m_tmpTitle = null!;
        private Button m_btnClose = null!;
        private TextMeshProUGUI m_tmpChapterInfo = null!;
        private Button m_btnEventType = null!;
        private Button m_btnTriggerMode = null!;
        private Button m_btnCheckTargetMode = null!;
        private Button m_btnCheckResolutionMode = null!;
        private TextMeshProUGUI m_tmpFormPreview = null!;
        private Button m_btnConfirm = null!;
        private ChapterEventPopupRequest m_request = null!;
        private ChapterEventType m_eventType = ChapterEventType.Check;
        private ChapterEventTriggerMode m_triggerMode = ChapterEventTriggerMode.Automatic;
        private ChapterCheckTargetMode m_checkTargetMode = ChapterCheckTargetMode.Ability;
        private ChapterCheckResolutionMode m_checkResolutionMode = ChapterCheckResolutionMode.RollDice;

        protected override void ScriptGenerator()
        {
            m_bindComponent = gameObject.GetComponent<UIBindComponent>();
            if (m_bindComponent == null)
            {
                Log.Error($"根物体: {gameObject.name} 缺少组件 UIBindComponent, 请检查！！！");
                return;
            }

            m_tmpTitle = m_bindComponent.GetComponent<TextMeshProUGUI>(0);
            m_btnClose = m_bindComponent.GetComponent<Button>(1);
            m_tmpChapterInfo = m_bindComponent.GetComponent<TextMeshProUGUI>(2);
            m_btnEventType = m_bindComponent.GetComponent<Button>(3);
            m_btnTriggerMode = m_bindComponent.GetComponent<Button>(4);
            m_btnCheckTargetMode = m_bindComponent.GetComponent<Button>(5);
            m_btnCheckResolutionMode = m_bindComponent.GetComponent<Button>(6);
            m_tmpFormPreview = m_bindComponent.GetComponent<TextMeshProUGUI>(7);
            m_btnConfirm = m_bindComponent.GetComponent<Button>(8);

            m_btnClose.onClick.RemoveAllListeners();
            m_btnClose.onClick.AddListener(OnClickCloseBtn);
            m_btnEventType.onClick.RemoveAllListeners();
            m_btnEventType.onClick.AddListener(OnClickCycleEventType);
            m_btnTriggerMode.onClick.RemoveAllListeners();
            m_btnTriggerMode.onClick.AddListener(OnClickCycleTriggerMode);
            m_btnCheckTargetMode.onClick.RemoveAllListeners();
            m_btnCheckTargetMode.onClick.AddListener(OnClickCycleCheckTargetMode);
            m_btnCheckResolutionMode.onClick.RemoveAllListeners();
            m_btnCheckResolutionMode.onClick.AddListener(OnClickCycleCheckResolutionMode);
            m_btnConfirm.onClick.RemoveAllListeners();
            m_btnConfirm.onClick.AddListener(OnClickConfirmBtn);
        }

        protected override void OnRefresh()
        {
            m_request = UserData as ChapterEventPopupRequest ?? new ChapterEventPopupRequest();
            m_eventType = ChapterEventType.Check;
            m_triggerMode = ChapterEventTriggerMode.Automatic;
            m_checkTargetMode = ChapterCheckTargetMode.Ability;
            m_checkResolutionMode = ChapterCheckResolutionMode.RollDice;

            RefreshView();
        }

        private void OnClickCycleEventType()
        {
            int nextValue = ((int) m_eventType + 1) % EventTypeLabels.Length;
            m_eventType = (ChapterEventType) nextValue;
            RefreshView();
        }

        private void OnClickCycleTriggerMode()
        {
            int nextValue = ((int) m_triggerMode + 1) % TriggerModeLabels.Length;
            m_triggerMode = (ChapterEventTriggerMode) nextValue;
            RefreshView();
        }

        private void OnClickCycleCheckTargetMode()
        {
            int nextValue = ((int) m_checkTargetMode + 1) % CheckTargetModeLabels.Length;
            m_checkTargetMode = (ChapterCheckTargetMode) nextValue;
            RefreshView();
        }

        private void OnClickCycleCheckResolutionMode()
        {
            int nextValue = ((int) m_checkResolutionMode + 1) % CheckResolutionModeLabels.Length;
            m_checkResolutionMode = (ChapterCheckResolutionMode) nextValue;
            RefreshView();
        }

        private void OnClickConfirmBtn()
        {
            Log.Info($"[ChapterEventPopupUI] 当前为 prefab 固定结构阶段。章节={m_request.ChapterId}, 事件类型={EventTypeLabels[(int) m_eventType]}");
            Close();
        }

        private void RefreshView()
        {
            bool isCheckEvent = m_eventType == ChapterEventType.Check;

            if (m_tmpTitle != null)
            {
                m_tmpTitle.text = "添加事件";
            }

            if (m_tmpChapterInfo != null)
            {
                string chapterName = string.IsNullOrWhiteSpace(m_request.ChapterName)
                    ? $"章节 #{m_request.ChapterId}"
                    : m_request.ChapterName.Trim();
                m_tmpChapterInfo.text = $"当前章节: {chapterName}\n当前页面结构以 prefab 为准，代码仅负责状态绑定。";
            }

            SetButtonLabel(m_btnEventType, $"事件类型: {EventTypeLabels[(int) m_eventType]}");
            SetButtonLabel(m_btnTriggerMode, $"触发方式: {TriggerModeLabels[(int) m_triggerMode]}");
            SetButtonLabel(m_btnCheckTargetMode, $"检定对象: {CheckTargetModeLabels[(int) m_checkTargetMode]}");
            SetButtonLabel(m_btnCheckResolutionMode, $"判定方式: {CheckResolutionModeLabels[(int) m_checkResolutionMode]}");
            SetButtonLabel(m_btnConfirm, "确定");

            if (m_btnCheckTargetMode != null)
            {
                m_btnCheckTargetMode.gameObject.SetActive(isCheckEvent);
            }

            if (m_btnCheckResolutionMode != null)
            {
                m_btnCheckResolutionMode.gameObject.SetActive(isCheckEvent);
            }

            if (m_tmpFormPreview != null)
            {
                m_tmpFormPreview.text = isCheckEvent
                    ? "检定事件专属字段区域\n- 检定对象\n- 具体检定项\n- 判定方式\n- DC 难度\n- 成功结果\n- 失败结果"
                    : "事件扩展区域\n后续按不同事件类型在 prefab 中补齐固定输入项。";
            }
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