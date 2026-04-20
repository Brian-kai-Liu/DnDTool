using System;
using System.IO;
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
        public Action<ChapterCreatureData> OnConfirm { get; set; } = null!;
    }

    [Window(UILayer.Top, location: "ChapterCreatureEntryPopupUI", fullScreen: false)]
    public partial class ChapterCreatureEntryPopupUI
    {
        private const string DefaultPreviewHintText = "未选择怪物预览图\n支持 PNG / JPG / JPEG";

        private static readonly Color PreviewPlaceholderColor = new Color(0.82f, 0.78f, 0.72f, 1f);

        private ChapterCreatureEntryPopupRequest m_request = null!;
        private Texture2D m_previewTexture = null!;
        private Sprite m_previewSprite = null!;
        private string m_previewImageFileName = string.Empty;

        protected override void OnCreate()
        {
            PopupWindowPresentationHelper.Configure(this);
        }

        protected override void OnRefresh()
        {
            m_request = UserData as ChapterCreatureEntryPopupRequest ?? new ChapterCreatureEntryPopupRequest();
            ResetPreviewState();

            // 标头
            ResetInput(m_tmpInputCreatureName, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureType, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureSize, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureAlignment, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureChallengeRating, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureExperiencePoints, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureArmorClass, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureHitPoints, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureSpeed, string.Empty, TMP_InputField.LineType.SingleLine);

            // 六维属性
            ResetInput(m_tmpInputCreatureStr, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureDex, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureCon, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureInt, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureWis, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureCha, string.Empty, TMP_InputField.LineType.SingleLine);

            // 豁免 / 技能
            ResetInput(m_tmpInputCreatureSavingThrows, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureSkills, string.Empty, TMP_InputField.LineType.SingleLine);

            // 感官 / 语言
            ResetInput(m_tmpInputCreatureSenses, string.Empty, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureLanguages, string.Empty, TMP_InputField.LineType.SingleLine);

            // 防御（合并字段）
            ResetInput(m_tmpInputCreatureDefenses, string.Empty, TMP_InputField.LineType.SingleLine);

            // 特性 / 动作（多行，条目间用 --- 分隔）
            ResetInput(m_tmpInputCreatureTraits, string.Empty, TMP_InputField.LineType.MultiLineNewline);
            ResetInput(m_tmpInputCreatureActions, string.Empty, TMP_InputField.LineType.MultiLineNewline);
            ResetInput(m_tmpInputCreatureExtraActions, string.Empty, TMP_InputField.LineType.MultiLineNewline);
            ResetInput(m_tmpInputCreatureReactions, string.Empty, TMP_InputField.LineType.MultiLineNewline);
            ResetInput(m_tmpInputCreatureLegendaryActions, string.Empty, TMP_InputField.LineType.MultiLineNewline);

            // DM 私密
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

        private partial void OnClickCloseBtn()
        {
            Log.Info("[ChapterCreatureEntryPopupUI] 点击了右上角关闭按钮。");
            Close();
        }

        private partial void OnClickConfirmBtn()
        {
            ConfirmEntry();
        }

        private partial void OnClickUploadPreviewBtn()
        {
            UploadPreviewImageAsync().Forget();
        }

        private void ConfirmEntry()
        {
            string creatureName = m_tmpInputCreatureName != null
                ? (m_tmpInputCreatureName.text?.Trim() ?? string.Empty)
                : string.Empty;

            if (string.IsNullOrWhiteSpace(creatureName))
            {
                Log.Warning("录入生物时必须填写名称。");
                return;
            }

            ChapterCreatureData creatureData = new ChapterCreatureData
            {
                Name = creatureName,
                CreatureType = GetInputValue(m_tmpInputCreatureType, string.Empty),
                CreatureSize = GetInputValue(m_tmpInputCreatureSize, string.Empty),
                Alignment = GetInputValue(m_tmpInputCreatureAlignment, string.Empty),
                ChallengeRating = GetInputValue(m_tmpInputCreatureChallengeRating, string.Empty),
                ExperiencePoints = GetInputValue(m_tmpInputCreatureExperiencePoints, string.Empty),
                ArmorClass = GetInputValue(m_tmpInputCreatureArmorClass, string.Empty),
                HitPoints = GetInputValue(m_tmpInputCreatureHitPoints, string.Empty),
                Speed = GetInputValue(m_tmpInputCreatureSpeed, string.Empty),
                Strength = GetInputValue(m_tmpInputCreatureStr, string.Empty),
                Dexterity = GetInputValue(m_tmpInputCreatureDex, string.Empty),
                Constitution = GetInputValue(m_tmpInputCreatureCon, string.Empty),
                Intelligence = GetInputValue(m_tmpInputCreatureInt, string.Empty),
                Wisdom = GetInputValue(m_tmpInputCreatureWis, string.Empty),
                Charisma = GetInputValue(m_tmpInputCreatureCha, string.Empty),
                SavingThrows = GetInputValue(m_tmpInputCreatureSavingThrows, string.Empty),
                Skills = GetInputValue(m_tmpInputCreatureSkills, string.Empty),
                Senses = GetInputValue(m_tmpInputCreatureSenses, string.Empty),
                Languages = GetInputValue(m_tmpInputCreatureLanguages, string.Empty),
                // Defenses 字段合并映射到 DamageResistances
                DamageResistances = GetInputValue(m_tmpInputCreatureDefenses, string.Empty),
                Traits = GetInputValue(m_tmpInputCreatureTraits, string.Empty),
                Actions = GetInputValue(m_tmpInputCreatureActions, string.Empty),
                // ExtraActions 映射到 BonusActions
                BonusActions = GetInputValue(m_tmpInputCreatureExtraActions, string.Empty),
                Reactions = GetInputValue(m_tmpInputCreatureReactions, string.Empty),
                LegendaryActions = GetInputValue(m_tmpInputCreatureLegendaryActions, string.Empty),
                BattleNotes = GetInputValue(m_tmpInputCreatureBattleNotes, string.Empty),
                PreviewImageFileName = m_previewImageFileName,
                AccentColor = new Color(0.45f, 0.55f, 0.7f, 1f),
            };

            m_request?.OnConfirm?.Invoke(creatureData);
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
                m_tmpPreviewHint.gameObject.SetActive(false);
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
                m_tmpPreviewHint.gameObject.SetActive(true);
            }
        }

        private void CleanupPreviewResources()
        {
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
    }
}