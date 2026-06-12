using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TMPro;
using TEngine;
using UnityEngine;
using UnityEngine.UI;
using Log = TEngine.Log;

namespace GameLogic
{
    [Window(UILayer.UI, location: "CharacterCreationUI", fullScreen: true)]
    internal sealed class CharacterCreationUI : UIWindow
    {
        private readonly struct DropdownOption
        {
            public readonly string Value;
            public readonly string Label;

            public DropdownOption(string value, string label)
            {
                Value = value ?? string.Empty;
                Label = label ?? string.Empty;
            }
        }

        private static readonly DropdownOption[] HpModeOptions =
        {
            new DropdownOption(CharacterHpModeIds.Custom, "自定义"),
            new DropdownOption(CharacterHpModeIds.Average, "均值"),
            new DropdownOption(CharacterHpModeIds.Rolled, "掷骰")
        };

        private Button m_btnBack;
        private Button m_btnSaveDraft;
        private Button m_btnImportPortrait;
        private TMP_InputField m_inputCharacterName;
        private TMP_InputField m_inputAge;
        private TMP_InputField m_inputHeight;
        private TMP_InputField m_inputGender;
        private TMP_InputField m_inputExperience;
        private TMP_InputField m_inputLevel;
        private TMP_InputField m_inputStrength;
        private TMP_InputField m_inputDexterity;
        private TMP_InputField m_inputConstitution;
        private TMP_InputField m_inputIntelligence;
        private TMP_InputField m_inputWisdom;
        private TMP_InputField m_inputCharisma;
        private TMP_InputField m_inputMaxHp;
        private TMP_InputField m_inputCurrentHp;
        private TMP_InputField m_inputTemporaryHp;
        private TMP_Dropdown m_dropdownAlignment;
        private TMP_Dropdown m_dropdownRace;
        private TMP_Dropdown m_dropdownBackground;
        private TMP_Dropdown m_dropdownClass;
        private TMP_Dropdown m_dropdownHpMode;
        private Image m_imgPreviewPortrait;
        private GameObject m_goPreviewPortraitPlaceholder;
        private TMP_Text m_tmpPreviewName;
        private TMP_Text m_tmpPreviewRace;
        private TMP_Text m_tmpPreviewClass;
        private TMP_Text m_tmpPreviewLevel;
        private TMP_Text m_tmpPreviewHp;
        private TMP_Text m_tmpPreviewAbilities;
        private TMP_Text m_tmpCreationMessage;

        private readonly List<DropdownOption> m_alignmentOptions = new List<DropdownOption>();
        private readonly List<DropdownOption> m_raceOptions = new List<DropdownOption>();
        private readonly List<DropdownOption> m_backgroundOptions = new List<DropdownOption>();
        private readonly List<DropdownOption> m_classOptions = new List<DropdownOption>();
        private string m_previewImagePath = string.Empty;
        private Texture2D m_loadedPortraitTexture;
        private Sprite m_loadedPortraitSprite;

        protected override void ScriptGenerator()
        {
            BindControls();
            InitializeDropdowns();
            BindEvents();
            InitializeDefaults();
            RefreshPreview();
        }

        private void BindControls()
        {
            m_btnBack = FindChildComponent<Button>("m_btnBack");
            m_btnSaveDraft = FindChildComponent<Button>("m_btnSaveDraft");
            m_btnImportPortrait = FindChildComponent<Button>("m_btnImportPortrait");
            m_inputCharacterName = FindChildComponent<TMP_InputField>("m_inputCharacterName");
            m_inputAge = FindChildComponent<TMP_InputField>("m_inputAge");
            m_inputHeight = FindChildComponent<TMP_InputField>("m_inputHeight");
            m_inputGender = FindChildComponent<TMP_InputField>("m_inputGender");
            m_inputExperience = FindChildComponent<TMP_InputField>("m_inputExperience");
            m_inputLevel = FindChildComponent<TMP_InputField>("m_inputLevel");
            m_inputStrength = FindChildComponent<TMP_InputField>("m_inputStrength");
            m_inputDexterity = FindChildComponent<TMP_InputField>("m_inputDexterity");
            m_inputConstitution = FindChildComponent<TMP_InputField>("m_inputConstitution");
            m_inputIntelligence = FindChildComponent<TMP_InputField>("m_inputIntelligence");
            m_inputWisdom = FindChildComponent<TMP_InputField>("m_inputWisdom");
            m_inputCharisma = FindChildComponent<TMP_InputField>("m_inputCharisma");
            m_inputMaxHp = FindChildComponent<TMP_InputField>("m_inputMaxHp");
            m_inputCurrentHp = FindChildComponent<TMP_InputField>("m_inputCurrentHp");
            m_inputTemporaryHp = FindChildComponent<TMP_InputField>("m_inputTemporaryHp");
            m_dropdownAlignment = FindChildComponent<TMP_Dropdown>("m_dropdownAlignment");
            m_dropdownRace = FindChildComponent<TMP_Dropdown>("m_dropdownRace");
            m_dropdownBackground = FindChildComponent<TMP_Dropdown>("m_dropdownBackground");
            m_dropdownClass = FindChildComponent<TMP_Dropdown>("m_dropdownClass");
            m_dropdownHpMode = FindChildComponent<TMP_Dropdown>("m_dropdownHpMode");
            m_imgPreviewPortrait = FindChildComponent<Image>("m_imgPreviewPortrait");
            m_goPreviewPortraitPlaceholder = FindChildComponent<RectTransform>("m_tmpPreviewPortraitPlaceholder")?.gameObject;
            m_tmpPreviewName = FindChildComponent<TMP_Text>("m_tmpPreviewName");
            m_tmpPreviewRace = FindChildComponent<TMP_Text>("m_tmpPreviewRace");
            m_tmpPreviewClass = FindChildComponent<TMP_Text>("m_tmpPreviewClass");
            m_tmpPreviewLevel = FindChildComponent<TMP_Text>("m_tmpPreviewLevel");
            m_tmpPreviewHp = FindChildComponent<TMP_Text>("m_tmpPreviewHp");
            m_tmpPreviewAbilities = FindChildComponent<TMP_Text>("m_tmpPreviewAbilities");
            m_tmpCreationMessage = FindChildComponent<TMP_Text>("m_tmpCreationMessage");
        }

        private void InitializeDropdowns()
        {
            m_alignmentOptions.Clear();
            m_raceOptions.Clear();
            m_backgroundOptions.Clear();
            m_classOptions.Clear();

            m_alignmentOptions.Add(new DropdownOption(string.Empty, "请选择阵营"));
            IReadOnlyList<DndAlignmentData> alignments = DndRuleContentService.Instance.Alignments;
            for (int index = 0; index < alignments.Count; index++)
            {
                DndAlignmentData data = alignments[index];
                m_alignmentOptions.Add(new DropdownOption(data.AlignmentId, FirstNonEmpty(data.Name, data.AlignmentId)));
            }

            m_raceOptions.Add(new DropdownOption(string.Empty, "请选择种族"));
            IReadOnlyList<DndRaceDefineData> races = DndRuleContentService.Instance.Races;
            for (int index = 0; index < races.Count; index++)
            {
                DndRaceDefineData data = races[index];
                m_raceOptions.Add(new DropdownOption(data.RaceId, FirstNonEmpty(data.Name, data.RaceId)));
            }

            m_backgroundOptions.Add(new DropdownOption(string.Empty, "请选择背景"));
            IReadOnlyList<DndBackgroundDefineData> backgrounds = DndRuleContentService.Instance.Backgrounds;
            for (int index = 0; index < backgrounds.Count; index++)
            {
                DndBackgroundDefineData data = backgrounds[index];
                m_backgroundOptions.Add(new DropdownOption(data.BackgroundId, FirstNonEmpty(data.Name, data.BackgroundId)));
            }

            m_classOptions.Add(new DropdownOption(string.Empty, "请选择职业"));
            IReadOnlyList<DndClassDefineData> classes = DndRuleContentService.Instance.Classes;
            for (int index = 0; index < classes.Count; index++)
            {
                DndClassDefineData data = classes[index];
                m_classOptions.Add(new DropdownOption(data.ClassId, FirstNonEmpty(data.Name, data.ClassId)));
            }

            SetupDropdown(m_dropdownAlignment, m_alignmentOptions);
            SetupDropdown(m_dropdownRace, m_raceOptions);
            SetupDropdown(m_dropdownBackground, m_backgroundOptions);
            SetupDropdown(m_dropdownClass, m_classOptions);
            SetupDropdown(m_dropdownHpMode, HpModeOptions);
        }

        private void BindEvents()
        {
            BindButton(m_btnBack, ReturnToCharacterManagement);
            BindButton(m_btnSaveDraft, SaveDraft);
            BindButton(m_btnImportPortrait, ImportPortrait);
            BindInput(m_inputCharacterName, RefreshPreview);
            BindInput(m_inputExperience, RefreshPreview);
            BindInput(m_inputLevel, RefreshPreview);
            BindInput(m_inputStrength, RefreshPreview);
            BindInput(m_inputDexterity, RefreshPreview);
            BindInput(m_inputConstitution, RefreshPreview);
            BindInput(m_inputIntelligence, RefreshPreview);
            BindInput(m_inputWisdom, RefreshPreview);
            BindInput(m_inputCharisma, RefreshPreview);
            BindInput(m_inputMaxHp, RefreshPreview);
            BindInput(m_inputCurrentHp, RefreshPreview);
            BindInput(m_inputTemporaryHp, RefreshPreview);
            BindDropdown(m_dropdownRace, RefreshPreview);
            BindDropdown(m_dropdownBackground, RefreshPreview);
            BindDropdown(m_dropdownClass, RefreshPreview);
            BindDropdown(m_dropdownAlignment, RefreshPreview);
            BindDropdown(m_dropdownHpMode, RefreshPreview);
        }

        private void InitializeDefaults()
        {
            SetInputText(m_inputLevel, "1");
            SetInputText(m_inputExperience, "0");
            SetInputText(m_inputStrength, "10");
            SetInputText(m_inputDexterity, "10");
            SetInputText(m_inputConstitution, "10");
            SetInputText(m_inputIntelligence, "10");
            SetInputText(m_inputWisdom, "10");
            SetInputText(m_inputCharisma, "10");
            SetInputText(m_inputMaxHp, "0");
            SetInputText(m_inputCurrentHp, "0");
            SetInputText(m_inputTemporaryHp, "0");
            SetText(m_tmpCreationMessage, string.Empty);
        }

        private void RefreshPreview()
        {
            string characterName = GetInputText(m_inputCharacterName);
            int level = Math.Max(1, ParseInt(m_inputLevel, 1));
            int maxHp = Math.Max(0, ParseInt(m_inputMaxHp, 0));
            int currentHp = CharacterCardManagementUI.NormalizeCurrentHp(ParseInt(m_inputCurrentHp, maxHp), maxHp);
            int tempHp = Math.Max(0, ParseInt(m_inputTemporaryHp, 0));

            SetText(m_tmpPreviewName, string.IsNullOrWhiteSpace(characterName) ? "未命名角色" : characterName);
            SetText(m_tmpPreviewRace, $"种族：{GetSelectedLabel(m_raceOptions, m_dropdownRace, "-")}");
            SetText(m_tmpPreviewClass, $"职业：{GetSelectedLabel(m_classOptions, m_dropdownClass, "-")}");
            SetText(m_tmpPreviewLevel, $"等级：{level}");
            SetText(m_tmpPreviewHp, tempHp > 0 ? $"HP：{currentHp}/{maxHp} +{tempHp}" : $"HP：{currentHp}/{maxHp}");
            SetText(
                m_tmpPreviewAbilities,
                $"属性：{ParseInt(m_inputStrength, 10)} / {ParseInt(m_inputDexterity, 10)} / {ParseInt(m_inputConstitution, 10)} / {ParseInt(m_inputIntelligence, 10)} / {ParseInt(m_inputWisdom, 10)} / {ParseInt(m_inputCharisma, 10)}");
        }

        private void SaveDraft()
        {
            CharacterCardDraftSaveData character = new CharacterCardDraftSaveData
            {
                CharacterName = GetInputText(m_inputCharacterName),
                Alignment = GetSelectedValue(m_alignmentOptions, m_dropdownAlignment),
                RaceId = GetSelectedValue(m_raceOptions, m_dropdownRace),
                ClassId = GetSelectedValue(m_classOptions, m_dropdownClass),
                BackgroundId = GetSelectedValue(m_backgroundOptions, m_dropdownBackground),
                PreviewImagePath = m_previewImagePath,
                Level = Math.Max(1, ParseInt(m_inputLevel, 1)),
                Experience = Math.Max(0, ParseInt(m_inputExperience, 0)),
                HpModeId = GetSelectedValue(HpModeOptions, m_dropdownHpMode),
                MaxHp = Math.Max(0, ParseInt(m_inputMaxHp, 0)),
                CurrentHp = ParseInt(m_inputCurrentHp, -1),
                TemporaryHp = Math.Max(0, ParseInt(m_inputTemporaryHp, 0)),
                IsCompleted = false
            };

            character.IdentityProfile.Age = GetInputText(m_inputAge);
            character.IdentityProfile.Height = GetInputText(m_inputHeight);
            character.IdentityProfile.Gender = GetInputText(m_inputGender);
            character.RuntimeSnapshot.Strength = ParseInt(m_inputStrength, 10);
            character.RuntimeSnapshot.Dexterity = ParseInt(m_inputDexterity, 10);
            character.RuntimeSnapshot.Constitution = ParseInt(m_inputConstitution, 10);
            character.RuntimeSnapshot.Intelligence = ParseInt(m_inputIntelligence, 10);
            character.RuntimeSnapshot.Wisdom = ParseInt(m_inputWisdom, 10);
            character.RuntimeSnapshot.Charisma = ParseInt(m_inputCharisma, 10);

            if (!string.IsNullOrWhiteSpace(character.ClassId))
            {
                character.ClassProgresses.Add(new CharacterClassProgressSaveData
                {
                    ClassId = character.ClassId,
                    Level = character.Level
                });
            }

            CharacterCardLocalRepository.Upsert(character);
            SetText(m_tmpCreationMessage, "角色草稿已保存");
            ReturnToCharacterManagement();
        }

        private void ImportPortrait()
        {
#if UNITY_EDITOR
            string path = UnityEditor.EditorUtility.OpenFilePanel("选择角色形象", string.Empty, "png,jpg,jpeg");
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            m_previewImagePath = path;
            ApplyPortrait(path);
            RefreshPreview();
#else
            Log.Warning("CharacterCreationUI: runtime image import requires a platform file picker.");
            SetText(m_tmpCreationMessage, "当前运行环境暂未接入图片选择器");
#endif
        }

        private void ApplyPortrait(string path)
        {
            if (m_imgPreviewPortrait == null || string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return;
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!texture.LoadImage(bytes))
                {
                    UnityEngine.Object.Destroy(texture);
                    return;
                }

                if (m_loadedPortraitSprite != null)
                {
                    UnityEngine.Object.Destroy(m_loadedPortraitSprite);
                    m_loadedPortraitSprite = null;
                }

                if (m_loadedPortraitTexture != null)
                {
                    UnityEngine.Object.Destroy(m_loadedPortraitTexture);
                }

                m_loadedPortraitTexture = texture;
                m_loadedPortraitSprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
                m_imgPreviewPortrait.sprite = m_loadedPortraitSprite;
                m_imgPreviewPortrait.preserveAspect = true;
                SetActive(m_goPreviewPortraitPlaceholder, false);
            }
            catch (Exception exception)
            {
                Log.Warning($"CharacterCreationUI: failed to load portrait. {exception.Message}");
            }
        }

        private void ReturnToCharacterManagement()
        {
            GameModule.UI.CloseUI<CharacterCreationUI>();
            GameModule.UI.ShowUIAsync<CharacterCardManagementUI>();
        }

        private static void SetupDropdown(TMP_Dropdown dropdown, IReadOnlyList<DropdownOption> options)
        {
            if (dropdown == null)
            {
                return;
            }

            dropdown.ClearOptions();
            List<TMP_Dropdown.OptionData> optionData = new List<TMP_Dropdown.OptionData>();
            if (options != null)
            {
                for (int index = 0; index < options.Count; index++)
                {
                    optionData.Add(new TMP_Dropdown.OptionData(options[index].Label));
                }
            }

            dropdown.AddOptions(optionData);
            dropdown.SetValueWithoutNotify(0);
            dropdown.RefreshShownValue();
        }

        private static void BindButton(Button button, Action action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action?.Invoke());
        }

        private static void BindInput(TMP_InputField input, Action action)
        {
            if (input == null)
            {
                return;
            }

            input.onValueChanged.RemoveAllListeners();
            input.onValueChanged.AddListener(_ => action?.Invoke());
        }

        private static void BindDropdown(TMP_Dropdown dropdown, Action action)
        {
            if (dropdown == null)
            {
                return;
            }

            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.onValueChanged.AddListener(_ => action?.Invoke());
        }

        private T FindChildComponent<T>(string childName) where T : Component
        {
            Transform[] children = gameObject.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < children.Length; index++)
            {
                Transform child = children[index];
                if (child != null && child.name == childName)
                {
                    return child.GetComponent<T>();
                }
            }

            return null;
        }

        private static string GetInputText(TMP_InputField input)
        {
            return input != null ? input.text?.Trim() ?? string.Empty : string.Empty;
        }

        private static void SetInputText(TMP_InputField input, string value)
        {
            input?.SetTextWithoutNotify(value ?? string.Empty);
        }

        private static int ParseInt(TMP_InputField input, int fallback)
        {
            string text = GetInputText(input);
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : fallback;
        }

        private static string GetSelectedValue(IReadOnlyList<DropdownOption> options, TMP_Dropdown dropdown)
        {
            if (options == null || dropdown == null || dropdown.value < 0 || dropdown.value >= options.Count)
            {
                return string.Empty;
            }

            return options[dropdown.value].Value;
        }

        private static string GetSelectedLabel(IReadOnlyList<DropdownOption> options, TMP_Dropdown dropdown, string emptyText)
        {
            if (options == null || dropdown == null || dropdown.value <= 0 || dropdown.value >= options.Count)
            {
                return emptyText ?? string.Empty;
            }

            return options[dropdown.value].Label;
        }

        private static string FirstNonEmpty(string first, string second)
        {
            return !string.IsNullOrWhiteSpace(first) ? first.Trim() : second?.Trim() ?? string.Empty;
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value ?? string.Empty;
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
            {
                target.SetActive(active);
            }
        }
    }
}
