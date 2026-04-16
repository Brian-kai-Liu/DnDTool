using System;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using TMPro;
using TEngine;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GameLogic
{
    internal sealed class ChapterCreatureEntryPopupRequest
    {
        public Action<ChapterCreatureStaticCardData> OnConfirm { get; set; } = null!;
    }

    [Window(UILayer.Top, location : "ChapterCreatureEntryPopupUI", fullScreen : true)]
    public sealed partial class ChapterCreatureEntryPopupUI : UIWindow
    {
        private const string DefaultPreviewHintText = "未选择怪物预览图\n支持 PNG / JPG / JPEG";

        private static readonly Color PreviewPlaceholderColor = new Color(0.82f, 0.78f, 0.72f, 1f);

        private ChapterCreatureEntryPopupRequest m_request = null!;
        private Texture2D m_previewTexture = null!;
        private Sprite m_previewSprite = null!;
        private string m_previewImageFileName = string.Empty;

        protected override void OnRefresh()
        {
            m_request = UserData as ChapterCreatureEntryPopupRequest ?? new ChapterCreatureEntryPopupRequest();
            ResetPreviewState();

            ResetInput(m_tmpInputCreatureName, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureType, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureSize, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureAlignment, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureChallengeRating, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureArmorClass, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureHitPoints, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureSpeed, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureAbilityScores, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureSenses, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureLanguages, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureDefenses, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureTraits, string.Empty, TMP_InputField.LineType.MultiLineNewline);
            ResetInput(m_tmpInputCreatureActions, string.Empty, TMP_InputField.LineType.MultiLineNewline);
            ResetInput(m_tmpInputCreatureExtraActions, string.Empty, TMP_InputField.LineType.MultiLineNewline);
            ResetInput(m_tmpInputCreatureBattleNotes, string.Empty, TMP_InputField.LineType.MultiLineNewline);
            ResetScrollPosition();
        }

        protected override void OnDestroy()
        {
            CleanupPreviewResources();
        }

        private void ResetScrollPosition()
        {
            if (m_scrollCreatureFields == null)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            m_scrollCreatureFields.StopMovement();
            m_scrollCreatureFields.horizontalNormalizedPosition = 0f;
            m_scrollCreatureFields.verticalNormalizedPosition = 1f;
        }

        private void OnClickCloseBtn()
        {
            Log.Info("[ChapterCreatureEntryPopupUI] 点击了右上角关闭按钮。");
            Close();
        }

        private void OnClickConfirmBtn()
        {
            ConfirmEntry();
        }

        private void OnClickUploadPreviewBtn()
        {
            UploadPreviewImageAsync().Forget();
        }

        private void ConfirmEntry()
        {
            string creatureName = m_tmpInputCreatureName != null ? (m_tmpInputCreatureName.text?.Trim() ?? string.Empty) : string.Empty;
            if (string.IsNullOrWhiteSpace(creatureName))
            {
                Log.Warning("录入生物时必须填写名称。");
                return;
            }

            ChapterCreatureStaticCardData creature = new ChapterCreatureStaticCardData(
                creatureName,
                BuildCreatureTypeDisplay(
                    GetInputValue(m_tmpInputCreatureType, "未分类生物"),
                    GetInputValue(m_tmpInputCreatureSize, "未设定体型"),
                    GetInputValue(m_tmpInputCreatureChallengeRating, "-")),
                GetInputValue(m_tmpInputCreatureAlignment, "未设定阵营"),
                BuildAccentColor(creatureName),
                BuildCreatureSummary());

            m_request?.OnConfirm?.Invoke(creature);
            Close();
        }

        private static void ResetInput(TMP_InputField inputField, string text, TMP_InputField.LineType lineType)
        {
            if (inputField == null)
            {
                return;
            }

            inputField.lineType = lineType;
            inputField.text = text;
        }

        private static string GetInputValue(TMP_InputField inputField, string fallback)
        {
            string value = inputField != null ? inputField.text?.Trim() ?? string.Empty : string.Empty;
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private async UniTaskVoid UploadPreviewImageAsync()
        {
            string filePath = RuntimeImageFileDialog.OpenImageFile("选择怪物预览图");
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            if (!File.Exists(filePath))
            {
                Log.Error($"怪物预览图文件不存在: {filePath}");
                return;
            }

            byte[] imageBytes;
            try
            {
                imageBytes = await UniTask.RunOnThreadPool(() => File.ReadAllBytes(filePath));
            }
            catch (Exception exception)
            {
                Log.Error($"读取怪物预览图失败: {exception.Message}");
                return;
            }

            Texture2D previewTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!previewTexture.LoadImage(imageBytes))
            {
                Object.Destroy(previewTexture);
                Log.Error("怪物预览图加载失败，文件不是有效图片。");
                return;
            }

            Sprite previewSprite = Sprite.Create(
                previewTexture,
                new Rect(0, 0, previewTexture.width, previewTexture.height),
                new Vector2(0.5f, 0.5f));

            CleanupPreviewResources();

            m_previewTexture = previewTexture;
            m_previewSprite = previewSprite;
            m_previewImageFileName = Path.GetFileName(filePath);

            if (m_imgCreaturePreview != null)
            {
                m_imgCreaturePreview.sprite = m_previewSprite;
                m_imgCreaturePreview.color = Color.white;
            }

            if (m_tmpPreviewHint != null)
            {
                m_tmpPreviewHint.text = $"已选择怪物预览图\n{m_previewImageFileName}";
            }
        }

        private void ResetPreviewState()
        {
            CleanupPreviewResources();
            m_previewImageFileName = string.Empty;

            if (m_imgCreaturePreview != null)
            {
                m_imgCreaturePreview.sprite = null;
                m_imgCreaturePreview.color = PreviewPlaceholderColor;
            }

            if (m_tmpPreviewHint != null)
            {
                m_tmpPreviewHint.text = DefaultPreviewHintText;
            }
        }

        private void CleanupPreviewResources()
        {
            if (m_imgCreaturePreview != null)
            {
                m_imgCreaturePreview.sprite = null;
            }

            if (m_previewSprite != null)
            {
                Object.Destroy(m_previewSprite);
                m_previewSprite = null!;
            }

            if (m_previewTexture != null)
            {
                Object.Destroy(m_previewTexture);
                m_previewTexture = null!;
            }
        }

        private string BuildCreatureSummary()
        {
            StringBuilder builder = new StringBuilder(512);
            builder.AppendLine("【基础信息】");
            builder.AppendLine($"预览图: {(string.IsNullOrWhiteSpace(m_previewImageFileName) ? "未上传" : m_previewImageFileName)}");
            builder.AppendLine($"类型: {GetInputValue(m_tmpInputCreatureType, "未分类生物")}");
            builder.AppendLine($"体型: {GetInputValue(m_tmpInputCreatureSize, "未设定体型")}");
            builder.AppendLine($"阵营: {GetInputValue(m_tmpInputCreatureAlignment, "未设定阵营")}");
            builder.AppendLine($"CR: {GetInputValue(m_tmpInputCreatureChallengeRating, "-")}");
            builder.AppendLine($"AC: {GetInputValue(m_tmpInputCreatureArmorClass, "-")}");
            builder.AppendLine($"HP: {GetInputValue(m_tmpInputCreatureHitPoints, "-")}");
            builder.AppendLine($"速度: {GetInputValue(m_tmpInputCreatureSpeed, "-")}");
            builder.AppendLine($"六维: {GetInputValue(m_tmpInputCreatureAbilityScores, "未填写")}");
            builder.AppendLine($"感官: {GetInputValue(m_tmpInputCreatureSenses, "未填写")}");
            builder.AppendLine($"语言: {GetInputValue(m_tmpInputCreatureLanguages, "未填写")}");
            builder.AppendLine($"抗性/免疫: {GetInputValue(m_tmpInputCreatureDefenses, "未填写")}");
            AppendSection(builder, "特性", GetInputValue(m_tmpInputCreatureTraits, "未填写"));
            AppendSection(builder, "动作", GetInputValue(m_tmpInputCreatureActions, "未填写"));
            AppendSection(builder, "额外动作/反应/传说动作/法术", GetInputValue(m_tmpInputCreatureExtraActions, "未填写"));
            AppendSection(builder, "战斗备注", GetInputValue(m_tmpInputCreatureBattleNotes, "无"));
            return builder.ToString().TrimEnd();
        }

        private static string BuildCreatureTypeDisplay(string creatureType, string creatureSize, string challengeRating)
        {
            return $"{creatureSize} {creatureType} | CR {challengeRating}";
        }

        private static void AppendSection(StringBuilder builder, string title, string content)
        {
            builder.AppendLine();
            builder.Append('【').Append(title).AppendLine("】");
            builder.AppendLine(content);
        }

        private static Color BuildAccentColor(string creatureName)
        {
            int hash = creatureName != null ? creatureName.GetHashCode() : 0;
            float hue = Mathf.Abs(hash % 360) / 360f;
            return Color.HSVToRGB(hue, 0.45f, 0.78f);
        }
    }
}