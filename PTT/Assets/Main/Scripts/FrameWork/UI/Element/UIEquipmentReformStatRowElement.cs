using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry.UI
{
    public class UIEquipmentReformStatRowElement : MonoBehaviour
    {
        [SerializeField] private Toggle _lockToggle;
        [SerializeField] private TMP_Text _lockLabel;
        [SerializeField] private TMP_Text _statNameText;
        [SerializeField] private TMP_Text _valueText;
        [SerializeField] private TMP_Text _minMaxText;
        [SerializeField] private Image _minMaxFillImage;

        private Action _onLockChanged;

        public Enum_Stat Stat { get; private set; } = Enum_Stat.Max;
        public bool IsLocked => _lockToggle != null && _lockToggle.isOn;
        
        private float _preffill = 0.5f;
        private float _fillduration = 0.5f;
        private Coroutine _fillRoutine;
        private static readonly Color MinFillColor = new Color(0.95f, 0.3f, 0.3f, 0.9f);
        private static readonly Color MaxFillColor = new Color(0.35f, 0.95f, 0.45f, 0.9f);
        private static readonly Color MaxValueHighlightColor = new Color(0.45f, 1f, 0.5f, 0.95f);

        private void Awake()
        {
            if (_lockToggle != null)
                _lockToggle.onValueChanged.AddListener(_ => _onLockChanged?.Invoke());

            if (_lockLabel != null)
                _lockLabel.SetText("Fix");

            if (_minMaxFillImage != null)
            {
                _minMaxFillImage.fillAmount = Mathf.Clamp01(_preffill);
                _minMaxFillImage.color = EvaluateFillColor(_minMaxFillImage.fillAmount, false);
            }
        }

        private void OnDisable()
        {
            if (_fillRoutine != null)
            {
                StopCoroutine(_fillRoutine);
                _fillRoutine = null;
            }
        }

        public void SetLockChangedCallback(Action callback)
        {
            _onLockChanged = callback;
        }

        public void ResetLock()
        {
            if (_lockToggle != null)
                _lockToggle.SetIsOnWithoutNotify(false);
        }

        public void SetData(Enum_Stat stat, double currentValue, double minValue, double maxValue, bool isMaxValue)
        {
            Stat = stat;

            string statName = StatHelper.GetStatDisplayName(stat);
            string currentText = StatHelper.FormatStatDisplayValue(stat, currentValue);
            string minText = StatHelper.FormatStatDisplayValue(stat, minValue);
            string maxText = StatHelper.FormatStatDisplayValue(stat, maxValue);
            string currentValueText = isMaxValue
                ? $"<color=#FFD54A>{currentText}</color>"
                : currentText;

            if (_statNameText != null)
                _statNameText.SetText(statName);

            if (_valueText != null)
                _valueText.SetText($"{currentValueText}");
            if (_minMaxText != null)
                _minMaxText.SetText($"({minText}/{maxText})");

            if (_minMaxFillImage != null)
            {
                float fillAmount = GetFillAmount(currentValue, minValue, maxValue);

                if (_fillRoutine != null)
                    StopCoroutine(_fillRoutine);

                if (gameObject.activeInHierarchy == false || _fillduration <= 0f)
                {
                    _minMaxFillImage.fillAmount = fillAmount;
                    _minMaxFillImage.color = EvaluateFillColor(fillAmount, isMaxValue);
                }
                else
                {
                    _minMaxFillImage.fillAmount = Mathf.Clamp01(_preffill);
                    _minMaxFillImage.color = EvaluateFillColor(_minMaxFillImage.fillAmount, false);
                    _fillRoutine = StartCoroutine(AnimateFill(fillAmount, isMaxValue));
                }
            }
        }

        private float GetFillAmount(double currentValue, double minValue, double maxValue)
        {
            double range = maxValue - minValue;
            if (range <= 0d)
                return currentValue >= maxValue ? 1f : 0f;

            double normalizedValue = (currentValue - minValue) / range;
            return Mathf.Clamp01((float)normalizedValue);
        }

        private IEnumerator AnimateFill(float targetFill, bool isMaxValue)
        {
            float startFill = Mathf.Clamp01(_preffill);
            Color startColor = EvaluateFillColor(startFill, false);
            Color targetColor = EvaluateFillColor(targetFill, isMaxValue);
            float elapsed = 0f;

            while (elapsed < _fillduration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _fillduration);
                float easedT = 1f - Mathf.Pow(1f - t, 3f);
                float fillamount = Mathf.Lerp(startFill, targetFill, easedT);
                _minMaxFillImage.fillAmount = fillamount;
                _minMaxFillImage.color = Color.Lerp(startColor, targetColor, easedT);
                _preffill = fillamount;
                yield return null;
            }

            _minMaxFillImage.fillAmount = targetFill;
            _minMaxFillImage.color = targetColor;
            _preffill = targetFill;
            _fillRoutine = null;
        }

        private Color EvaluateFillColor(float fillAmount, bool isMaxValue)
        {
            if (isMaxValue)
                return MaxValueHighlightColor;

            return Color.Lerp(MinFillColor, MaxFillColor, Mathf.Clamp01(fillAmount));
        }
    }
}
