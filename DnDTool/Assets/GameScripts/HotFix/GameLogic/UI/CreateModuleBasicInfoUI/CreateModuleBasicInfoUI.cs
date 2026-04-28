using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using TEngine;
using UnityEngine;
using UnityEngine.UI;
using Log = TEngine.Log;
using Object = UnityEngine.Object;

namespace GameLogic
{
    internal sealed class CreateModuleBasicInfoInputData
    {
        public string ModuleName { get; set; } = string.Empty;

        public string ModuleIntroduction { get; set; } = string.Empty;

        public string RuleVersion { get; set; } = string.Empty;

        public string ExtensionPackageInfo { get; set; } = string.Empty;

        public string RecommendedLevel { get; set; } = string.Empty;

        public string RecommendedPlayers { get; set; } = string.Empty;

        public string EstimatedDuration { get; set; } = string.Empty;

        public bool HasPreviewImage { get; set; }

        public List<AdventureHookCardData> AdventureHookCards { get; set; } = new List<AdventureHookCardData>();
    }

    [Window(UILayer.UI, location : "CreateModuleBasicInfoUI", fullScreen : true)]
    public partial class CreateModuleBasicInfoUI
    {
        private const float AdventureHookCardHeight = 264f;
        private const float AdventureHookCardSpacing = 14f;

        private Texture2D m_previewTexture = null!;
        private Sprite m_previewSprite = null!;
        private readonly List<string> m_ruleVersionOptions = new List<string>();
        private readonly List<ExtensionPackageInfoSelectionData> m_selectedExtensionPackages = new List<ExtensionPackageInfoSelectionData>();
        private readonly List<AdventureHookCardData> m_adventureHookCards = new List<AdventureHookCardData>();
        private readonly List<AdventureHookCardView> m_adventureHookCardViews = new List<AdventureHookCardView>();
        private string m_extensionPackageInfo = string.Empty;
        private bool m_hasLoggedRuleVersionConfigFallback = false;

        protected override void OnCreate()
        {
            ResetPreviewState();
            SetupCenterPanel();
            SetupRightPanel();
        }

        #region 事件

        private partial void OnClickBackBtn()
        {
            GameModule.UI.CloseUI<CreateModuleBasicInfoUI>();
            GameModule.UI.ShowUIAsync<HomeUI>();
        }

        private partial void OnClickNextBtn()
        {
            CreateModuleBasicInfoInputData inputData = CollectModuleBasicInfoInputData();
            int adventureHookCardCount = inputData.AdventureHookCards.Count;
            int completedAdventureHookCardCount = 0;
            List<string> adventureHookTargets = new List<string>();

            for (int index = 0; index < inputData.AdventureHookCards.Count; index++)
            {
                AdventureHookCardData card = inputData.AdventureHookCards[index];
                bool hasTarget = !string.IsNullOrWhiteSpace(card.Target);
                bool hasContent = !string.IsNullOrWhiteSpace(card.HookContent);
                if (hasTarget && hasContent)
                {
                    completedAdventureHookCardCount++;
                }

                if (hasTarget)
                {
                    adventureHookTargets.Add(card.Target);
                }
            }

            Log.Info($"创建模组第一页输入状态：模组名称={inputData.ModuleName}，已上传预览图={inputData.HasPreviewImage}，简介长度={inputData.ModuleIntroduction.Length}，规则版本={inputData.RuleVersion}，扩展包信息长度={inputData.ExtensionPackageInfo.Length}，适用等级={inputData.RecommendedLevel}，推荐人数={inputData.RecommendedPlayers}，预计时长={inputData.EstimatedDuration}，冒险引子卡数量={adventureHookCardCount}，已完成引子卡={completedAdventureHookCardCount}，引子针对项={string.Join("、", adventureHookTargets)}。");

            GameModule.UI.CloseUI<CreateModuleBasicInfoUI>();
            GameModule.UI.ShowUIAsync<ChapterEditorUI>();
        }

        private partial void OnClickUploadPreviewBtn()
        {
            UploadPreviewImageAsync().Forget();
        }

        private partial void OnDropdownRuleVersionChange(int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= m_ruleVersionOptions.Count)
            {
                return;
            }

            Log.Info($"当前规则版本选择：{m_ruleVersionOptions[selectedIndex]}");
        }

        private partial void OnClickEditExtensionPackageBtn()
        {
            GameModule.UI.ShowUIAsync<ExtensionPackageInfoPopupUI>(new ExtensionPackageInfoPopupRequest
            {
                InitialContent = m_extensionPackageInfo,
                InitialSelectedPackageIds = m_selectedExtensionPackages.ConvertAll(package => package.Id),
                OnConfirm = OnExtensionPackageInfoConfirmed
            });
        }

        private partial void OnClickAddAdventureHookCardBtn()
        {
            SyncAdventureHookInputsToData();
            m_adventureHookCards.Add(new AdventureHookCardData());
            RefreshAdventureHookCards();
        }

        #endregion

        protected override void OnDestroy()
        {
            SyncAdventureHookInputsToData();

            for (int index = 0; index < m_adventureHookCardViews.Count; index++)
            {
                m_adventureHookCardViews[index].Dispose();
            }

            m_adventureHookCardViews.Clear();

            CleanupPreviewResources();
        }

        private void ResetPreviewState()
        {
            if (m_imgPreview != null)
            {
                m_imgPreview.sprite = null;
            }

            if (m_tmpPreviewPlaceholder != null)
            {
                m_tmpPreviewPlaceholder.gameObject.SetActive(true);
            }

            if (m_tmpPreviewFileName != null)
            {
                m_tmpPreviewFileName.text = string.Empty;
                m_tmpPreviewFileName.gameObject.SetActive(false);
            }
        }

        private void SetupCenterPanel()
        {
            PopulateRuleVersionDropdown();
            RefreshExtensionPackageSummary();
        }

        private void SetupRightPanel()
        {
            if (m_rectAdventureHookCardTemplate != null)
            {
                m_rectAdventureHookCardTemplate.gameObject.SetActive(false);
            }

            if (m_adventureHookCards.Count == 0)
            {
                m_adventureHookCards.Add(new AdventureHookCardData());
            }

            RefreshAdventureHookCards();
        }

        private void PopulateRuleVersionDropdown()
        {
            m_ruleVersionOptions.Clear();
            m_ruleVersionOptions.AddRange(LoadRuleVersionOptions());

            if (m_dropdownRuleVersion == null)
            {
                return;
            }

            m_dropdownRuleVersion.ClearOptions();
            m_dropdownRuleVersion.AddOptions(m_ruleVersionOptions);
            m_dropdownRuleVersion.value = 0;
            m_dropdownRuleVersion.RefreshShownValue();
        }

        private IEnumerable<string> LoadRuleVersionOptions()
        {
            if (TryLoadRuleVersionOptionsFromConfig(out List<string> options) && options.Count > 0)
            {
                return options;
            }

            if (!m_hasLoggedRuleVersionConfigFallback)
            {
                m_hasLoggedRuleVersionConfigFallback = true;
                Log.Warning("当前仓库未找到可用的配置表代码与配置资源，规则版本下拉框暂时回退为默认选项。后续接入 Luban 配置后可直接替换 TryLoadRuleVersionOptionsFromConfig 的实现。");
            }

            return new[]
            {
                "DND 5E 2014",
                "DND 5E 2024"
            };
        }

        private static bool TryLoadRuleVersionOptionsFromConfig(out List<string> options)
        {
            options = DndRuleContentService.Instance.GetRulePackageOptionLabels();
            return options.Count > 0;
        }

        private string GetSelectedRuleVersion()
        {
            if (m_dropdownRuleVersion == null || m_ruleVersionOptions.Count == 0)
            {
                return string.Empty;
            }

            int selectedIndex = Mathf.Clamp(m_dropdownRuleVersion.value, 0, m_ruleVersionOptions.Count - 1);
            return m_ruleVersionOptions[selectedIndex];
        }

        private void OnExtensionPackageInfoConfirmed(ExtensionPackageInfoPopupResult result)
        {
            m_selectedExtensionPackages.Clear();
            if (result?.SelectedPackages != null)
            {
                m_selectedExtensionPackages.AddRange(result.SelectedPackages);
            }

            m_extensionPackageInfo = result?.Summary?.Trim() ?? string.Empty;
            RefreshExtensionPackageSummary();
        }

        private void RefreshExtensionPackageSummary()
        {
            if (m_tmpExtensionPackageSummary == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(m_extensionPackageInfo))
            {
                m_tmpExtensionPackageSummary.text = "未填写扩展包信息，点击下方按钮进行编辑";
                return;
            }

            if (m_selectedExtensionPackages.Count > 0)
            {
                m_tmpExtensionPackageSummary.text = $"已选择 {m_selectedExtensionPackages.Count} 个扩展包：{m_extensionPackageInfo}";
                return;
            }

            string summary = m_extensionPackageInfo.Replace("\r", " ").Replace("\n", " / ");
            if (summary.Length > 56)
            {
                summary = summary.Substring(0, 56) + "...";
            }

            m_tmpExtensionPackageSummary.text = summary;
        }

        private void RefreshAdventureHookCards()
        {
            if (m_rectAdventureHookCardContent == null || m_rectAdventureHookCardTemplate == null)
            {
                return;
            }

            SyncAdventureHookInputsToData();

            while (m_adventureHookCardViews.Count < m_adventureHookCards.Count)
            {
                GameObject cardObject = Object.Instantiate(m_rectAdventureHookCardTemplate.gameObject, m_rectAdventureHookCardContent, false);
                cardObject.SetActive(true);
                m_adventureHookCardViews.Add(new AdventureHookCardView(cardObject));
            }

            while (m_adventureHookCardViews.Count > m_adventureHookCards.Count)
            {
                int lastIndex = m_adventureHookCardViews.Count - 1;
                m_adventureHookCardViews[lastIndex].Dispose();
                m_adventureHookCardViews.RemoveAt(lastIndex);
            }

            for (int index = 0; index < m_adventureHookCards.Count; index++)
            {
                m_adventureHookCardViews[index].SetVisible(true);
                m_adventureHookCardViews[index].SetLayout(index, AdventureHookCardHeight, AdventureHookCardSpacing);
                m_adventureHookCardViews[index].Bind(m_adventureHookCards[index], RemoveAdventureHookCard);
            }

            float contentHeight = m_adventureHookCards.Count == 0
                ? 0f
                : m_adventureHookCards.Count * AdventureHookCardHeight + (m_adventureHookCards.Count - 1) * AdventureHookCardSpacing;
            m_rectAdventureHookCardContent.sizeDelta = new Vector2(0f, contentHeight);

            RefreshAdventureHookSummary();
        }

        private void RemoveAdventureHookCard(AdventureHookCardData card)
        {
            SyncAdventureHookInputsToData();

            if (!m_adventureHookCards.Remove(card))
            {
                return;
            }

            RefreshAdventureHookCards();
        }

        private void RefreshAdventureHookSummary()
        {
            SyncAdventureHookInputsToData();
        }

        private CreateModuleBasicInfoInputData CollectModuleBasicInfoInputData()
        {
            SyncAdventureHookInputsToData();

            return new CreateModuleBasicInfoInputData
            {
                ModuleName = m_tmpInputModuleName != null ? m_tmpInputModuleName.text.Trim() : string.Empty,
                ModuleIntroduction = m_tmpInputModuleIntroduction != null ? m_tmpInputModuleIntroduction.text.Trim() : string.Empty,
                RuleVersion = GetSelectedRuleVersion(),
                ExtensionPackageInfo = m_extensionPackageInfo,
                RecommendedLevel = m_tmpInputRecommendedLevel != null ? m_tmpInputRecommendedLevel.text.Trim() : string.Empty,
                RecommendedPlayers = m_tmpInputRecommendedPlayers != null ? m_tmpInputRecommendedPlayers.text.Trim() : string.Empty,
                EstimatedDuration = m_tmpInputEstimatedDuration != null ? m_tmpInputEstimatedDuration.text.Trim() : string.Empty,
                HasPreviewImage = m_previewSprite != null,
                AdventureHookCards = CollectAdventureHookCardData()
            };
        }

        private List<AdventureHookCardData> CollectAdventureHookCardData()
        {
            SyncAdventureHookInputsToData();

            List<AdventureHookCardData> result = new List<AdventureHookCardData>(m_adventureHookCards.Count);
            for (int index = 0; index < m_adventureHookCards.Count; index++)
            {
                AdventureHookCardData card = m_adventureHookCards[index];
                result.Add(new AdventureHookCardData
                {
                    Target = card.Target,
                    HookContent = card.HookContent
                });
            }

            return result;
        }

        private void SyncAdventureHookInputsToData()
        {
            int syncCount = Mathf.Min(m_adventureHookCards.Count, m_adventureHookCardViews.Count);
            for (int index = 0; index < syncCount; index++)
            {
                m_adventureHookCardViews[index].SyncToData(m_adventureHookCards[index]);
            }
        }

        private async UniTaskVoid UploadPreviewImageAsync()
        {
            string filePath = OpenLocalImageFile();
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            if (!File.Exists(filePath))
            {
                Log.Error($"预览图文件不存在: {filePath}");
                return;
            }

            byte[] imageBytes;
            try
            {
                imageBytes = await UniTask.RunOnThreadPool(() => File.ReadAllBytes(filePath));
            }
            catch (System.Exception e)
            {
                Log.Error($"读取预览图失败: {e.Message}");
                return;
            }

            CleanupPreviewResources();

            m_previewTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!m_previewTexture.LoadImage(imageBytes))
            {
                CleanupPreviewResources();
                Log.Error("预览图加载失败，文件不是有效图片。");
                return;
            }

            m_previewSprite = Sprite.Create(
                m_previewTexture,
                new Rect(0, 0, m_previewTexture.width, m_previewTexture.height),
                new Vector2(0.5f, 0.5f));

            m_imgPreview.sprite = m_previewSprite;

            if (m_tmpPreviewPlaceholder != null)
            {
                m_tmpPreviewPlaceholder.gameObject.SetActive(false);
            }

            if (m_tmpPreviewFileName != null)
            {
                m_tmpPreviewFileName.text = Path.GetFileName(filePath);
                m_tmpPreviewFileName.gameObject.SetActive(true);
            }
        }

        private string OpenLocalImageFile()
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.OpenFilePanel("选择模组预览图", string.Empty, "png,jpg,jpeg");
#else
            Log.Warning("预览图上传当前仅在 Unity 编辑器中接入，本地文件选择运行时暂未实现。");
            return string.Empty;
#endif
        }

        private void CleanupPreviewResources()
        {
            if (m_imgPreview != null)
            {
                m_imgPreview.sprite = null;
            }

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
    }
}
