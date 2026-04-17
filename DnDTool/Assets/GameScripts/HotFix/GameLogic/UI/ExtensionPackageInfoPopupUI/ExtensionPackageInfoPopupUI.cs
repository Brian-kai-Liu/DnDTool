using System;
using System.Collections.Generic;
using System.Linq;
using TEngine;
using UnityEngine;
using Object = UnityEngine.Object;
using Log = TEngine.Log;

namespace GameLogic
{
    public sealed class ExtensionPackageInfoPopupResult
    {
        public string Summary { get; set; } = string.Empty;

        public IReadOnlyList<ExtensionPackageInfoSelectionData> SelectedPackages { get; set; } = Array.Empty<ExtensionPackageInfoSelectionData>();
    }

    public sealed class ExtensionPackageInfoPopupRequest
    {
        public string InitialContent { get; set; } = string.Empty;
        public IReadOnlyList<string> InitialSelectedPackageIds { get; set; } = Array.Empty<string>();
        public Action<ExtensionPackageInfoPopupResult> OnConfirm { get; set; } = null!;
    }

    [Window(UILayer.Top, location : "ExtensionPackageInfoPopupUI", fullScreen : false)]
    public partial class ExtensionPackageInfoPopupUI
    {
        private const float CardHeight = 152f;
        private const float CardSpacing = 16f;

        private ExtensionPackageInfoPopupRequest m_request = null!;
        private readonly List<ExtensionPackageInfoOption> m_packageOptions = new List<ExtensionPackageInfoOption>();
        private readonly List<ExtensionPackageInfoCardView> m_packageCardViews = new List<ExtensionPackageInfoCardView>();
        private readonly HashSet<string> m_selectedPackageIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private bool m_hasLoggedConfigFallback = false;

        protected override void OnCreate()
        {
            PopupWindowPresentationHelper.Configure(this);

            if (m_rectPackageCardTemplate != null)
            {
                m_rectPackageCardTemplate.gameObject.SetActive(false);
            }
        }

        protected override void OnRefresh()
        {
            m_request = UserData as ExtensionPackageInfoPopupRequest;
            RefreshPackageOptions();
            ApplyInitialSelection(m_request);
            RebuildPackageCards();
        }

        protected override void OnDestroy()
        {
            for (int index = 0; index < m_packageCardViews.Count; index++)
            {
                m_packageCardViews[index].Dispose();
            }

            m_packageCardViews.Clear();
        }

        private partial void OnClickCloseBtn()
        {
            Close();
        }

        private partial void OnClickCancelBtn()
        {
            Close();
        }

        private partial void OnClickConfirmBtn()
        {
            m_request?.OnConfirm?.Invoke(BuildPopupResult());
            Close();
        }

        private void RefreshPackageOptions()
        {
            m_packageOptions.Clear();
            m_packageOptions.AddRange(LoadExtensionPackageOptions());
        }

        private IEnumerable<ExtensionPackageInfoOption> LoadExtensionPackageOptions()
        {
            if (TryLoadExtensionPackageOptionsFromConfig(out List<ExtensionPackageInfoOption> options) && options.Count > 0)
            {
                return options;
            }

            if (!m_hasLoggedConfigFallback)
            {
                m_hasLoggedConfigFallback = true;
                Log.Warning("当前仓库未找到可用的扩展包配置表与配套资源，扩展包弹窗暂时回退为内置示例数据。后续接入配置表后可直接替换 TryLoadExtensionPackageOptionsFromConfig 的实现。");
            }

            return new[]
            {
                new ExtensionPackageInfoOption(
                    id: "xanathar-guide",
                    name: "赞萨斯的万事指南",
                    subtitle: "Xanathar's Guide to Everything",
                    description: "提供大量子职业、工具规则与 DM 扩展建议，适合补全角色成长与遭遇设计的细节。",
                    previewImagePath: string.Empty),
                new ExtensionPackageInfoOption(
                    id: "tasha-cauldron",
                    name: "塔莎的万物坩埚",
                    subtitle: "Tasha's Cauldron of Everything",
                    description: "收录可选职业特性、队伍协作机制与额外法术内容，适合偏重角色构筑与团队玩法的模组。",
                    previewImagePath: string.Empty),
                new ExtensionPackageInfoOption(
                    id: "monsters-multiverse",
                    name: "怪物的多元宇宙",
                    subtitle: "Monsters of the Multiverse",
                    description: "整合多本资料中的种族与怪物条目，适合需要快速引用多元世界生物资料的冒险。",
                    previewImagePath: string.Empty),
                new ExtensionPackageInfoOption(
                    id: "sword-coast-guide",
                    name: "剑湾冒险者指南",
                    subtitle: "Sword Coast Adventurer's Guide",
                    description: "聚焦被遗忘国度剑湾地区的背景设定与角色资料，适合区域风格明确的战役模组。",
                    previewImagePath: string.Empty),
            };
        }

        private static bool TryLoadExtensionPackageOptionsFromConfig(out List<ExtensionPackageInfoOption> options)
        {
            options = new List<ExtensionPackageInfoOption>();
            return false;
        }

        private void ApplyInitialSelection(ExtensionPackageInfoPopupRequest request)
        {
            m_selectedPackageIds.Clear();

            if (request?.InitialSelectedPackageIds != null && request.InitialSelectedPackageIds.Count > 0)
            {
                for (int index = 0; index < request.InitialSelectedPackageIds.Count; index++)
                {
                    string packageId = request.InitialSelectedPackageIds[index];
                    if (!string.IsNullOrWhiteSpace(packageId) && m_packageOptions.Any(option => string.Equals(option.Id, packageId, StringComparison.OrdinalIgnoreCase)))
                    {
                        m_selectedPackageIds.Add(packageId);
                    }
                }

                return;
            }

            string initialContent = request?.InitialContent ?? string.Empty;
            if (string.IsNullOrWhiteSpace(initialContent))
            {
                return;
            }

            string[] tokens = initialContent
                .Split(new[] { '、', ',', ';', '，', '；', '/', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            for (int index = 0; index < tokens.Length; index++)
            {
                string token = tokens[index].Trim();
                if (string.IsNullOrEmpty(token))
                {
                    continue;
                }

                ExtensionPackageInfoOption option = m_packageOptions.FirstOrDefault(item =>
                    string.Equals(item.Id, token, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(item.Name, token, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(item.Subtitle, token, StringComparison.OrdinalIgnoreCase));

                if (option != null)
                {
                    m_selectedPackageIds.Add(option.Id);
                }
            }
        }

        private void RebuildPackageCards()
        {
            if (m_rectPackageListContent == null || m_rectPackageCardTemplate == null)
            {
                return;
            }

            while (m_packageCardViews.Count < m_packageOptions.Count)
            {
                GameObject cardGo = Object.Instantiate(m_rectPackageCardTemplate.gameObject, m_rectPackageListContent);
                cardGo.SetActive(true);
                m_packageCardViews.Add(new ExtensionPackageInfoCardView(cardGo));
            }

            for (int index = 0; index < m_packageCardViews.Count; index++)
            {
                bool isVisible = index < m_packageOptions.Count;
                ExtensionPackageInfoCardView cardView = m_packageCardViews[index];
                cardView.SetVisible(isVisible);
                if (!isVisible)
                {
                    continue;
                }

                ExtensionPackageInfoOption option = m_packageOptions[index];
                cardView.SetLayout(index, CardHeight, CardSpacing);
                cardView.Bind(option, m_selectedPackageIds.Contains(option.Id), OnPackageSelectionChanged);
            }

            float height = m_packageOptions.Count > 0
                ? (m_packageOptions.Count * (CardHeight + CardSpacing)) - CardSpacing
                : CardHeight;
            m_rectPackageListContent.sizeDelta = new Vector2(m_rectPackageListContent.sizeDelta.x, height);
        }

        private void OnPackageSelectionChanged(string packageId, bool isSelected)
        {
            if (isSelected)
            {
                m_selectedPackageIds.Add(packageId);
            }
            else
            {
                m_selectedPackageIds.Remove(packageId);
            }
        }

        private ExtensionPackageInfoPopupResult BuildPopupResult()
        {
            List<ExtensionPackageInfoSelectionData> selectedPackages = m_packageOptions
                .Where(option => m_selectedPackageIds.Contains(option.Id))
                .Select(option => option.ToSelectionData())
                .ToList();

            return new ExtensionPackageInfoPopupResult
            {
                Summary = selectedPackages.Count == 0 ? string.Empty : string.Join("、", selectedPackages.Select(package => package.Name)),
                SelectedPackages = selectedPackages
            };
        }
    }
}