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
        public ChapterCreatureData InitialData { get; set; }

        public Action<ChapterCreatureData> OnConfirm { get; set; } = null!;

        public Action OnDelete { get; set; }
    }

    [Window(UILayer.Top, location: "ChapterCreatureEntryPopupUI", fullScreen: false)]
    public partial class ChapterCreatureEntryPopupUI
    {
        private const string DefaultPreviewHintText = "未选择怪物预览图\n支持 PNG / JPG / JPEG";

        private static readonly Color PreviewPlaceholderColor = new Color(0.82f, 0.78f, 0.72f, 1f);

        private ChapterCreatureEntryPopupRequest m_request = null!;
        private Button m_btnDelete = null!;
        private Texture2D m_previewTexture = null!;
        private Sprite m_previewSprite = null!;
        private string m_previewImageSourcePath = string.Empty;
        private string m_previewImageFileName = string.Empty;
        private int m_previewLoadVersion;

        protected override void OnCreate()
        {
            PopupWindowPresentationHelper.Configure(this);
            m_btnDelete = FindChildComponent<Button>(transform, "m_btnDelete");
            if (m_btnDelete != null)
            {
                m_btnDelete.onClick.RemoveAllListeners();
                m_btnDelete.onClick.AddListener(OnClickDeleteBtn);
            }
        }

        protected override void OnRefresh()
        {
            m_request = UserData as ChapterCreatureEntryPopupRequest ?? new ChapterCreatureEntryPopupRequest();
            ApplyRequestState();
            PopulateFields(m_request.InitialData);
            ResetScrollPosition();
        }

        protected override void OnDestroy()
        {
            CleanupPreviewResources();
        }

        private void ApplyRequestState()
        {
            if (m_tmpTitle != null)
            {
                m_tmpTitle.text = m_request.InitialData == null ? "生物信息录入" : "编辑生物信息";
            }

            if (m_btnDelete != null)
            {
                m_btnDelete.gameObject.SetActive(m_request.OnDelete != null);
            }
        }

        private void PopulateFields(ChapterCreatureData creatureData)
        {
            ResetPreviewState();

            if (creatureData == null)
            {
                ResetEditableFields();
                return;
            }

            ResetEditableFields(
                creatureData.Name,
                creatureData.CreatureType,
                creatureData.CreatureSize,
                creatureData.Alignment,
                creatureData.ChallengeRating,
                creatureData.ExperiencePoints,
                creatureData.ArmorClass,
                creatureData.HitPoints,
                creatureData.Speed,
                creatureData.Strength,
                creatureData.Dexterity,
                creatureData.Constitution,
                creatureData.Intelligence,
                creatureData.Wisdom,
                creatureData.Charisma,
                creatureData.SavingThrows,
                creatureData.Skills,
                creatureData.Senses,
                creatureData.Languages,
                creatureData.DamageResistances,
                creatureData.Traits,
                creatureData.Actions,
                creatureData.BonusActions,
                creatureData.Reactions,
                creatureData.LegendaryActions,
                creatureData.BattleNotes);

            string previewPath = ChapterEditorPersistenceService.ResolveCreaturePreviewPath(creatureData.PreviewImageFileName);
            if (!string.IsNullOrWhiteSpace(previewPath))
            {
                int loadVersion = ++m_previewLoadVersion;
                LoadPreviewFromPathAsync(previewPath, loadVersion).Forget();
            }
        }

        private void ResetEditableFields()
        {
            ResetEditableFields(
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty);
        }

        private void ResetEditableFields(
            string name,
            string creatureType,
            string creatureSize,
            string alignment,
            string challengeRating,
            string experiencePoints,
            string armorClass,
            string hitPoints,
            string speed,
            string strength,
            string dexterity,
            string constitution,
            string intelligence,
            string wisdom,
            string charisma,
            string savingThrows,
            string skills,
            string senses,
            string languages,
            string defenses,
            string traits,
            string actions,
            string extraActions,
            string reactions,
            string legendaryActions,
            string battleNotes)
        {
            // 标头
            ResetInput(m_tmpInputCreatureName, name, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureType, creatureType, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureSize, creatureSize, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureAlignment, alignment, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureChallengeRating, challengeRating, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureExperiencePoints, experiencePoints, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureArmorClass, armorClass, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureHitPoints, hitPoints, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureSpeed, speed, TMP_InputField.LineType.SingleLine);

            // 六维属性
            ResetInput(m_tmpInputCreatureStr, strength, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureDex, dexterity, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureCon, constitution, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureInt, intelligence, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureWis, wisdom, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureCha, charisma, TMP_InputField.LineType.SingleLine);

            // 豁免 / 技能
            ResetInput(m_tmpInputCreatureSavingThrows, savingThrows, TMP_InputField.LineType.MultiLineNewline);
            ResetInput(m_tmpInputCreatureSkills, skills, TMP_InputField.LineType.MultiLineNewline);

            // 感官 / 语言
            ResetInput(m_tmpInputCreatureSenses, senses, TMP_InputField.LineType.SingleLine);
            ResetInput(m_tmpInputCreatureLanguages, languages, TMP_InputField.LineType.SingleLine);

            // 防御
            ResetInput(m_tmpInputCreatureDefenses, defenses, TMP_InputField.LineType.SingleLine);

            // 特性 / 动作
            ResetInput(m_tmpInputCreatureTraits, traits, TMP_InputField.LineType.MultiLineNewline);
            ResetInput(m_tmpInputCreatureActions, actions, TMP_InputField.LineType.MultiLineNewline);
            ResetInput(m_tmpInputCreatureExtraActions, extraActions, TMP_InputField.LineType.MultiLineNewline);
            ResetInput(m_tmpInputCreatureReactions, reactions, TMP_InputField.LineType.MultiLineNewline);
            ResetInput(m_tmpInputCreatureLegendaryActions, legendaryActions, TMP_InputField.LineType.MultiLineNewline);

            // DM 私密
            ResetInput(m_tmpInputCreatureBattleNotes, battleNotes, TMP_InputField.LineType.MultiLineNewline);
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

        private void OnClickDeleteBtn()
        {
            Action onDelete = m_request?.OnDelete;
            Close();
            onDelete?.Invoke();
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

            string previewImageFileName = m_request?.InitialData?.PreviewImageFileName ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(m_previewImageSourcePath))
            {
                try
                {
                    previewImageFileName = ChapterEditorPersistenceService.StoreCreaturePreviewImage(m_previewImageSourcePath);
                }
                catch (Exception exception)
                {
                    Log.Warning($"缓存生物预览图失败: {exception.Message}");
                }
            }

            ChapterCreatureData creatureData = CloneCreatureData(m_request?.InitialData) ?? new ChapterCreatureData();
            creatureData.Name = creatureName;
            creatureData.CreatureType = GetInputValue(m_tmpInputCreatureType, string.Empty);
            creatureData.CreatureSize = GetInputValue(m_tmpInputCreatureSize, string.Empty);
            creatureData.Alignment = GetInputValue(m_tmpInputCreatureAlignment, string.Empty);
            creatureData.ChallengeRating = GetInputValue(m_tmpInputCreatureChallengeRating, string.Empty);
            creatureData.ExperiencePoints = GetInputValue(m_tmpInputCreatureExperiencePoints, string.Empty);
            creatureData.ArmorClass = GetInputValue(m_tmpInputCreatureArmorClass, string.Empty);
            creatureData.HitPoints = GetInputValue(m_tmpInputCreatureHitPoints, string.Empty);
            creatureData.Speed = GetInputValue(m_tmpInputCreatureSpeed, string.Empty);
            creatureData.Strength = GetInputValue(m_tmpInputCreatureStr, string.Empty);
            creatureData.Dexterity = GetInputValue(m_tmpInputCreatureDex, string.Empty);
            creatureData.Constitution = GetInputValue(m_tmpInputCreatureCon, string.Empty);
            creatureData.Intelligence = GetInputValue(m_tmpInputCreatureInt, string.Empty);
            creatureData.Wisdom = GetInputValue(m_tmpInputCreatureWis, string.Empty);
            creatureData.Charisma = GetInputValue(m_tmpInputCreatureCha, string.Empty);
            creatureData.SavingThrows = GetInputValue(m_tmpInputCreatureSavingThrows, string.Empty);
            creatureData.Skills = GetInputValue(m_tmpInputCreatureSkills, string.Empty);
            creatureData.Senses = GetInputValue(m_tmpInputCreatureSenses, string.Empty);
            creatureData.Languages = GetInputValue(m_tmpInputCreatureLanguages, string.Empty);
            creatureData.DamageResistances = GetInputValue(m_tmpInputCreatureDefenses, string.Empty);
            creatureData.Traits = GetInputValue(m_tmpInputCreatureTraits, string.Empty);
            creatureData.Actions = GetInputValue(m_tmpInputCreatureActions, string.Empty);
            creatureData.BonusActions = GetInputValue(m_tmpInputCreatureExtraActions, string.Empty);
            creatureData.Reactions = GetInputValue(m_tmpInputCreatureReactions, string.Empty);
            creatureData.LegendaryActions = GetInputValue(m_tmpInputCreatureLegendaryActions, string.Empty);
            creatureData.BattleNotes = GetInputValue(m_tmpInputCreatureBattleNotes, string.Empty);
            creatureData.PreviewImageFileName = previewImageFileName;
            if (creatureData.AccentColor.a <= 0f)
            {
                creatureData.AccentColor = new Color(0.45f, 0.55f, 0.7f, 1f);
            }

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

            int loadVersion = ++m_previewLoadVersion;
            ApplyLoadedPreview(filePath, imageBytes, loadVersion, true);
        }

        private async UniTaskVoid LoadPreviewFromPathAsync(string filePath, int loadVersion)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return;
            }

            byte[] imageBytes;
            try
            {
                imageBytes = await UniTask.RunOnThreadPool(() => File.ReadAllBytes(filePath));
            }
            catch (Exception exception)
            {
                Log.Warning($"读取已有生物预览图失败: {exception.Message}");
                return;
            }

            ApplyLoadedPreview(filePath, imageBytes, loadVersion, false);
        }

        private void ApplyLoadedPreview(string filePath, byte[] imageBytes, int loadVersion, bool logOnFailure)
        {
            if (loadVersion != m_previewLoadVersion)
            {
                return;
            }

            Texture2D previewTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!previewTexture.LoadImage(imageBytes))
            {
                Object.Destroy(previewTexture);
                if (logOnFailure)
                {
                    Log.Error("怪物预览图加载失败，文件不是有效图片。");
                }
                return;
            }

            Sprite previewSprite = Sprite.Create(
                previewTexture,
                new Rect(0, 0, previewTexture.width, previewTexture.height),
                new Vector2(0.5f, 0.5f));

            CleanupPreviewResources();

            m_previewTexture = previewTexture;
            m_previewSprite = previewSprite;
            m_previewImageSourcePath = filePath;
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
            ++m_previewLoadVersion;
            m_previewImageSourcePath = string.Empty;
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

        private static ChapterCreatureData CloneCreatureData(ChapterCreatureData source)
        {
            if (source == null)
            {
                return null;
            }

            return new ChapterCreatureData
            {
                CreatureId = source.CreatureId,
                Name = source.Name,
                NameEn = source.NameEn,
                CreatureType = source.CreatureType,
                CreatureSize = source.CreatureSize,
                Alignment = source.Alignment,
                ChallengeRating = source.ChallengeRating,
                ExperiencePoints = source.ExperiencePoints,
                ArmorClass = source.ArmorClass,
                HitPoints = source.HitPoints,
                Speed = source.Speed,
                Strength = source.Strength,
                Dexterity = source.Dexterity,
                Constitution = source.Constitution,
                Intelligence = source.Intelligence,
                Wisdom = source.Wisdom,
                Charisma = source.Charisma,
                SavingThrows = source.SavingThrows,
                Skills = source.Skills,
                Senses = source.Senses,
                Languages = source.Languages,
                DamageResistances = source.DamageResistances,
                DamageImmunities = source.DamageImmunities,
                ConditionImmunities = source.ConditionImmunities,
                Traits = source.Traits,
                Actions = source.Actions,
                BonusActions = source.BonusActions,
                Reactions = source.Reactions,
                LegendaryActions = source.LegendaryActions,
                BattleNotes = source.BattleNotes,
                PreviewImageFileName = source.PreviewImageFileName,
                AccentColor = source.AccentColor,
            };
        }

        private static T FindChildComponent<T>(Transform root, string targetName) where T : Component
        {
            if (root == null)
            {
                return null;
            }

            for (int index = 0; index < root.childCount; index++)
            {
                Transform child = root.GetChild(index);
                if (child.name == targetName)
                {
                    return child.GetComponent<T>();
                }

                T component = FindChildComponent<T>(child, targetName);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }
    }
}
