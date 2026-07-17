using System;
using System.Collections.Generic;
using TMPro;
using TEngine;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    [Window(UILayer.Top, location: "DiceRollUI", fullScreen: false)]
    internal sealed class DiceRollUI : UIWindow
    {
        private const float RollAnimationDuration = 0.45f;
        private static readonly int[] QuickDiceSides = { 4, 6, 8, 10, 12, 20, 100 };

        private DiceRollUIRequest m_request;
        private CharacterDiceRollResultData m_currentResult;
        private CharacterDiceRollResultData m_pendingResult;
        private readonly List<GameObject> m_diceResultItems = new List<GameObject>();
        private readonly List<GameObject> m_affixInfoItems = new List<GameObject>();
        private readonly List<DicePurposeButtonBinding> m_purposeButtons = new List<DicePurposeButtonBinding>();
        private string m_currentPurpose = "record";
        private float m_rollAnimationRemaining;
        private bool m_isRolling;

        private TMP_Text m_tmpSource;
        private RectTransform m_rectAffixInfoContent;
        private GameObject m_itemAffixInfoTemplate;
        private TMP_InputField m_inputDiceExpression;
        private RectTransform m_rectQuickDiceButtons;
        private Button m_btnClearExpression;
        private Button m_btnRoll;
        private Button m_btnClose;
        private RectTransform m_rectDiceAnimationArea;
        private RectTransform m_rectDiceResultContent;
        private GameObject m_itemDiceResultTemplate;
        private TMP_Text m_tmpResultSummary;
        private TMP_Text m_tmpResultTotal;
        private RectTransform m_rectPurposeButtons;
        private TMP_Text m_tmpHistory;

        protected override void ScriptGenerator()
        {
            m_tmpSource = FindDescendantComponent<TMP_Text>("m_tmpSource");
            m_rectAffixInfoContent = FindDescendantComponent<RectTransform>("m_rectAffixInfoContent");
            m_itemAffixInfoTemplate = FindDescendant("m_itemAffixInfoTemplate")?.gameObject;
            m_btnClose = FindDescendantComponent<Button>("m_btnClose");
            m_inputDiceExpression = FindDescendantComponent<TMP_InputField>("m_inputDiceExpression");
            m_rectQuickDiceButtons = FindDescendantComponent<RectTransform>("m_rectQuickDiceButtons");
            m_btnClearExpression = FindDescendantComponent<Button>("m_btnClearExpression");
            m_btnRoll = FindDescendantComponent<Button>("m_btnRoll");
            m_rectDiceAnimationArea = FindDescendantComponent<RectTransform>("m_panelDiceAnimation");
            m_rectDiceResultContent = FindDescendantComponent<RectTransform>("m_rectDiceResultContent");
            m_itemDiceResultTemplate = FindDescendant("m_itemDiceResultTemplate")?.gameObject;
            m_tmpResultSummary = FindDescendantComponent<TMP_Text>("m_tmpResultSummary");
            m_tmpResultTotal = FindDescendantComponent<TMP_Text>("m_tmpResultTotal");
            m_rectPurposeButtons = FindDescendantComponent<RectTransform>("m_rectPurposeButtons");
            m_tmpHistory = FindDescendantComponent<TMP_Text>("m_tmpHistory");

            BindButton(m_btnClose, OnClickClose);
            BindButton(m_btnClearExpression, OnClickClearExpression);
            BindButton(m_btnRoll, OnClickRoll);
            BindQuickDiceButtons();
            BindPurposeButtons();
        }

        protected override void OnRefresh()
        {
            m_request = UserData as DiceRollUIRequest ?? new DiceRollUIRequest();
            m_currentResult = null;
            m_pendingResult = null;
            m_isRolling = false;
            m_rollAnimationRemaining = 0f;
            m_currentPurpose = "record";
            SetButtonInteractable(m_btnRoll, true);

            SetText(m_tmpSource, BuildSourceText(m_request));
            RefreshAffixInfo();
            if (m_inputDiceExpression != null)
            {
                m_inputDiceExpression.text = string.IsNullOrWhiteSpace(m_request.DiceExpression)
                    ? "1d20"
                    : m_request.DiceExpression.Trim();
            }

            RefreshPurposeButtons();
            RefreshResult(null);
            RefreshHistory();
        }

        protected override void OnUpdate()
        {
            if (!m_isRolling)
            {
                _hasOverrideUpdate = false;
                return;
            }

            m_rollAnimationRemaining -= Time.deltaTime;
            RefreshRollingPreview();
            if (m_rollAnimationRemaining > 0f)
            {
                return;
            }

            CompleteRollAnimation();
            _hasOverrideUpdate = m_isRolling;
        }

        private void BindQuickDiceButtons()
        {
            if (m_rectQuickDiceButtons == null)
            {
                return;
            }

            for (int index = 0; index < QuickDiceSides.Length; index++)
            {
                int sides = QuickDiceSides[index];
                Button button = FindChildComponent<Button>(m_rectQuickDiceButtons, $"m_btnD{sides}");
                if (button == null)
                {
                    continue;
                }

                BindButton(button, () => AppendDiceTerm(sides));
            }
        }

        private void BindPurposeButtons()
        {
            m_purposeButtons.Clear();
            AddPurposeButton("m_btnPurposeRecord", "record", "仅记录");
            AddPurposeButton("m_btnPurposeHealHp", "heal_hp", "恢复生命");
            AddPurposeButton("m_btnPurposeAttackHit", "attack_hit", "攻击命中");
            AddPurposeButton("m_btnPurposeDamage", "damage", "伤害");
            AddPurposeButton("m_btnPurposeSkillCheck", "skill_check", "技能检定");
            AddPurposeButton("m_btnPurposeSavingThrow", "saving_throw", "豁免检定");
            AddPurposeButton("m_btnPurposeSpellAttack", "spell_attack", "法术攻击");
            AddPurposeButton("m_btnPurposeSpellSaveDc", "spell_save_dc", "法术DC");
            AddPurposeButton("m_btnPurposeCustom", "custom", "自定义");
        }

        private void AddPurposeButton(string buttonName, string purpose, string label)
        {
            if (m_rectPurposeButtons == null)
            {
                return;
            }

            Button button = FindChildComponent<Button>(m_rectPurposeButtons, buttonName);
            if (button == null)
            {
                return;
            }

            TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
            SetText(text, label);
            BindButton(button, () =>
            {
                m_currentPurpose = purpose;
                RefreshPurposeButtons();
                DispatchResult(m_currentResult);
                RefreshHistory();
            });
            m_purposeButtons.Add(new DicePurposeButtonBinding(button, purpose));
        }

        private void AppendDiceTerm(int sides)
        {
            if (m_inputDiceExpression == null)
            {
                return;
            }

            string current = m_inputDiceExpression.text?.Trim() ?? string.Empty;
            string term = $"1d{sides}";
            m_inputDiceExpression.text = string.IsNullOrWhiteSpace(current)
                ? term
                : $"{current}+{term}";
        }

        private void OnClickClearExpression()
        {
            if (m_inputDiceExpression != null)
            {
                m_inputDiceExpression.text = string.Empty;
            }

            m_currentResult = null;
            m_pendingResult = null;
            m_isRolling = false;
            SetButtonInteractable(m_btnRoll, true);
            RefreshResult(null);
            RefreshHistory();
        }

        private void OnClickRoll()
        {
            if (m_isRolling)
            {
                return;
            }

            string expression = m_inputDiceExpression?.text?.Trim() ?? string.Empty;
            CharacterDiceRollResultData result = CharacterDiceRollService.Instance.Roll(expression);
            if (result == null || !result.Success)
            {
                m_currentResult = result;
                RefreshResult(result);
                DispatchResult(result);
                RefreshHistory();
                return;
            }

            m_pendingResult = result;
            m_currentResult = null;
            m_rollAnimationRemaining = RollAnimationDuration;
            m_isRolling = true;
            SetButtonInteractable(m_btnRoll, false);
            SetUpdateDirty();
            RefreshRollingPreview();
        }

        private void DispatchResult(CharacterDiceRollResultData result)
        {
            if (result == null || m_request?.OnResult == null)
            {
                return;
            }

            m_request.OnResult.Invoke(new DiceRollUIResult
            {
                SourceType = m_request.SourceType,
                SourceId = m_request.SourceId,
                SourceName = m_request.SourceName,
                EffectName = m_request.EffectName,
                EffectDescription = m_request.EffectDescription,
                Purpose = m_currentPurpose,
                RollResult = result
            });
        }

        private void RefreshAffixInfo()
        {
            if (m_rectAffixInfoContent == null || m_itemAffixInfoTemplate == null)
            {
                return;
            }

            List<AffixInfoDisplayEntry> entries = BuildAffixInfoEntries(m_request);
            EnsureAffixInfoItemCount(entries.Count);
            for (int index = 0; index < m_affixInfoItems.Count; index++)
            {
                GameObject item = m_affixInfoItems[index];
                bool visible = index < entries.Count;
                SetActive(item, visible);
                if (visible)
                {
                    SetAffixInfoItem(item, entries[index]);
                }
            }

            SetActive(m_itemAffixInfoTemplate, false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_rectAffixInfoContent);
        }

        private void OnClickClose()
        {
            GameModule.UI.CloseUI<DiceRollUI>();
        }

        private void RefreshRollingPreview()
        {
            SetText(m_tmpResultSummary, "掷骰中...");
            SetText(m_tmpResultTotal, "...");
            for (int index = 0; index < m_diceResultItems.Count; index++)
            {
                SetActive(m_diceResultItems[index], false);
            }

            SetActive(m_itemDiceResultTemplate, false);
        }

        private void CompleteRollAnimation()
        {
            m_isRolling = false;
            SetButtonInteractable(m_btnRoll, true);

            CharacterDiceRollResultData result = m_pendingResult;
            m_pendingResult = null;
            m_currentResult = result;
            RefreshResult(result);
            DispatchResult(result);
            RefreshHistory();
        }

        private void RefreshResult(CharacterDiceRollResultData result)
        {
            EnsureDiceResultItemCount(CountDiceResults(result));
            SetText(m_tmpResultSummary, BuildResultSummary(result));
            SetText(m_tmpResultTotal, result != null && result.Success ? result.Total.ToString() : "-");

            int itemIndex = 0;
            if (result != null && result.Success)
            {
                for (int termIndex = 0; termIndex < result.Terms.Count; termIndex++)
                {
                    CharacterDiceRollTermResultData term = result.Terms[termIndex];
                    if (term == null || !term.IsDice)
                    {
                        continue;
                    }

                    for (int rollIndex = 0; rollIndex < term.Rolls.Count; rollIndex++)
                    {
                        SetDiceResultItem(m_diceResultItems[itemIndex], $"d{term.DieSides}", term.Rolls[rollIndex].ToString());
                        itemIndex++;
                    }
                }
            }

            for (int index = 0; index < m_diceResultItems.Count; index++)
            {
                SetActive(m_diceResultItems[index], index < itemIndex);
            }

            SetActive(m_itemDiceResultTemplate, false);
            if (m_rectDiceAnimationArea != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(m_rectDiceAnimationArea);
            }
        }

        private void RefreshHistory()
        {
            if (m_tmpHistory == null)
            {
                return;
            }

            IReadOnlyList<CharacterDiceRollHistoryEntry> history = CharacterCreationSessionService.Instance.DiceRollHistory;
            SetText(m_tmpHistory, CharacterDiceRollHistoryFormatter.BuildRecentHistoryText(history, 6, false));
        }

        private void RefreshPurposeButtons()
        {
            for (int index = 0; index < m_purposeButtons.Count; index++)
            {
                DicePurposeButtonBinding binding = m_purposeButtons[index];
                bool selected = string.Equals(binding.Purpose, m_currentPurpose, StringComparison.OrdinalIgnoreCase);
                Image image = binding.Button != null ? binding.Button.targetGraphic as Image : null;
                if (image != null)
                {
                    image.color = selected
                        ? new Color(0.24f, 0.58f, 0.78f, 1f)
                        : new Color(0.16f, 0.19f, 0.23f, 1f);
                }
            }
        }

        private void EnsureDiceResultItemCount(int count)
        {
            if (m_rectDiceResultContent == null || m_itemDiceResultTemplate == null)
            {
                return;
            }

            while (m_diceResultItems.Count < count)
            {
                GameObject item = UnityEngine.Object.Instantiate(m_itemDiceResultTemplate, m_rectDiceResultContent);
                item.name = $"m_itemDiceResult_{m_diceResultItems.Count + 1}";
                item.SetActive(true);
                m_diceResultItems.Add(item);
            }
        }

        private void EnsureAffixInfoItemCount(int count)
        {
            if (m_rectAffixInfoContent == null || m_itemAffixInfoTemplate == null)
            {
                return;
            }

            while (m_affixInfoItems.Count < count)
            {
                GameObject item = UnityEngine.Object.Instantiate(m_itemAffixInfoTemplate, m_rectAffixInfoContent);
                item.name = $"m_itemAffixInfo_{m_affixInfoItems.Count + 1}";
                item.SetActive(true);
                m_affixInfoItems.Add(item);
            }
        }

        private static int CountDiceResults(CharacterDiceRollResultData result)
        {
            if (result == null || !result.Success)
            {
                return 0;
            }

            int count = 0;
            for (int index = 0; index < result.Terms.Count; index++)
            {
                CharacterDiceRollTermResultData term = result.Terms[index];
                if (term != null && term.IsDice)
                {
                    count += term.Rolls.Count;
                }
            }

            return count;
        }

        private static string BuildResultSummary(CharacterDiceRollResultData result)
        {
            if (result == null)
            {
                return "等待掷骰";
            }

            return result.Success ? result.Summary : $"掷骰失败：{result.Error}";
        }

        private static string BuildSourceText(DiceRollUIRequest request)
        {
            if (request == null)
            {
                return "来源：手动掷骰";
            }

            string source = string.IsNullOrWhiteSpace(request.SourceName) ? "手动掷骰" : request.SourceName.Trim();
            if (!string.IsNullOrWhiteSpace(request.EffectName))
            {
                source = $"{source} - {request.EffectName.Trim()}";
            }

            return $"来源：{source}";
        }

        private static List<AffixInfoDisplayEntry> BuildAffixInfoEntries(DiceRollUIRequest request)
        {
            return new List<AffixInfoDisplayEntry>
            {
                new AffixInfoDisplayEntry(
                    FirstNonEmpty(request?.EffectName, request?.SourceName, "\u624b\u52a8\u63b7\u9ab0"),
                    string.IsNullOrWhiteSpace(request?.DiceExpression) ? "1d20" : request.DiceExpression.Trim(),
                    string.IsNullOrWhiteSpace(request?.EffectDescription) ? "-" : request.EffectDescription.Trim())
            };
        }

        private static void SetAffixInfoItem(GameObject item, AffixInfoDisplayEntry entry)
        {
            if (item == null)
            {
                return;
            }

            TMP_Text nameText = FindDescendantText(item.transform, "m_tmpAffixInfoName");
            TMP_Text diceExpressionText = FindDescendantText(item.transform, "m_tmpAffixDiceExpression");
            TMP_Text descriptionText = FindDescendantText(item.transform, "m_tmpAffixDescription");
            TMP_InputField diceExpressionInput = FindDescendantComponent<TMP_InputField>(item.transform, "m_inputAffixDiceExpression");

            SetText(nameText, entry.Name);
            SetText(diceExpressionText, entry.DiceExpression);
            SetText(descriptionText, entry.Description);
            if (diceExpressionInput != null)
            {
                diceExpressionInput.text = entry.DiceExpression;
            }
        }

        private static void SetDiceResultItem(GameObject item, string diceName, string value)
        {
            if (item == null)
            {
                return;
            }

            TMP_Text diceText = FindChildText(item.transform, "m_tmpDiceName");
            TMP_Text valueText = FindChildText(item.transform, "m_tmpDiceValue");
            SetText(diceText, diceName);
            SetText(valueText, value);
            item.SetActive(true);
        }

        private static TMP_Text FindChildText(Transform root, string name)
        {
            if (root == null)
            {
                return null;
            }

            Transform child = root.Find(name);
            return child != null ? child.GetComponent<TMP_Text>() : null;
        }

        private static TMP_Text FindDescendantText(Transform root, string name)
        {
            Transform target = FindDescendant(root, name);
            return target != null ? target.GetComponent<TMP_Text>() : null;
        }

        private static T FindDescendantComponent<T>(Transform root, string targetName) where T : Component
        {
            Transform target = FindDescendant(root, targetName);
            return target != null ? target.GetComponent<T>() : null;
        }

        private T FindDescendantComponent<T>(string targetName) where T : Component
        {
            Transform target = FindDescendant(targetName);
            return target != null ? target.GetComponent<T>() : null;
        }

        private Transform FindDescendant(string targetName)
        {
            if (rectTransform == null || string.IsNullOrWhiteSpace(targetName))
            {
                return null;
            }

            return FindDescendant(rectTransform, targetName.Trim());
        }

        private static Transform FindDescendant(Transform root, string targetName)
        {
            if (root == null)
            {
                return null;
            }

            if (string.Equals(root.name, targetName, StringComparison.Ordinal))
            {
                return root;
            }

            for (int index = 0; index < root.childCount; index++)
            {
                Transform result = FindDescendant(root.GetChild(index), targetName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
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

        private static void SetButtonInteractable(Button button, bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            for (int index = 0; index < values.Length; index++)
            {
                string value = values[index];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return string.Empty;
        }

        private readonly struct AffixInfoDisplayEntry
        {
            public readonly string Name;
            public readonly string DiceExpression;
            public readonly string Description;

            public AffixInfoDisplayEntry(string name, string diceExpression, string description)
            {
                Name = name ?? string.Empty;
                DiceExpression = diceExpression ?? string.Empty;
                Description = description ?? string.Empty;
            }
        }

        private readonly struct DicePurposeButtonBinding
        {
            public readonly Button Button;
            public readonly string Purpose;

            public DicePurposeButtonBinding(Button button, string purpose)
            {
                Button = button;
                Purpose = purpose ?? string.Empty;
            }
        }
    }
}
