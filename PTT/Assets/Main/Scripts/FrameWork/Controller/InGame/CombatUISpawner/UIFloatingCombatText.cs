using TMPro;
using UnityEngine;

public enum CombatTextStyle : byte
{
    Miss = 0,
    Critical = 1,
    Combo = 2,
    Damage = 3
}

namespace GameBerry.UI
{
    public sealed class UIFloatingCombatText : MonoBehaviour
    {
        public const string MSG_MISS = "Miss";
        public const string MSG_CRITICAL = "Critical";

        [Header("Refs")]
        [SerializeField] private ScreenSpaceFollower follower;
        [SerializeField] private TMP_Text tmp;
        [SerializeField] private RectTransform rect;

        private UIFloatingCombatTextPool _pool;
        
        private CombatTextPresetAsset _preset;
        private CombatTextStyle _style;

        private float _t;
        private float _life;

        private Vector3 _startOffset;
        private Vector3 _endOffset;

        private Vector2 _baseAnchored;

        public void BindPool(UIFloatingCombatTextPool pool) => _pool = pool;

        public void PlayText(Transform target, Vector3 baseOffset, CombatTextStyle style, string message, CombatTextPresetAsset preset)
        {
            _style = style;
            _preset = preset;

            _t = 0f;
            _life = preset.lifeTime;

            follower.SetTarget(target, baseOffset);
            _startOffset = baseOffset;
            _endOffset = baseOffset + Vector3.up * preset.rise;

            ApplyPresetToTMP(preset);
            tmp.SetText(message);

            rect.localScale = Vector3.one * preset.baseScale;
            _baseAnchored = rect.anchoredPosition;

            gameObject.SetActive(true);
        }

        public void PlayInt_Combo(Transform target, Vector3 baseOffset, CombatTextStyle style, int value, CombatTextPresetAsset preset)
        {
            _style = style;
            _preset = preset;

            _t = 0f;
            //_life = preset.lifeTime;
            _life = StaticResource.Instance.GetBattleModeStaticData().ComboReleaseTime;

            follower.SetTarget(target, baseOffset);
            _startOffset = baseOffset;
            _endOffset = baseOffset + Vector3.up * preset.rise;

            ApplyPresetToTMP(preset);
            tmp.SetText("<size=32>{0:#,###}</size> Combo", value);

            rect.localScale = Vector3.one * preset.baseScale;
            _baseAnchored = rect.anchoredPosition;

            gameObject.SetActive(true);
        }

        public void PlayInt(Transform target, Vector3 baseOffset, CombatTextStyle style, int value, CombatTextPresetAsset preset)
        {
            _style = style;
            _preset = preset;

            _t = 0f;
            _life = preset.lifeTime;

            follower.SetTarget(target, baseOffset);
            _startOffset = baseOffset;
            _endOffset = baseOffset + Vector3.up * preset.rise;

            ApplyPresetToTMP(preset);
            tmp.SetText("{0:#,###}", value);

            rect.localScale = Vector3.one * preset.baseScale;
            _baseAnchored = rect.anchoredPosition;

            gameObject.SetActive(true);
        }

        void ApplyPresetToTMP(CombatTextPresetAsset p)
        {
            if (p.fontMaterial != null) tmp.fontMaterial = p.fontMaterial;

            tmp.fontSize = p.fontSize;
            tmp.fontStyle = p.fontStyle;
            tmp.alignment = p.alignment;
            tmp.color = p.color;

            tmp.enableAutoSizing = p.autoSize;
            tmp.enableWordWrapping = p.wordWrap;
            tmp.richText = p.richText;
            tmp.raycastTarget = p.raycastTarget;
        }

        void Update()
        {
            _t += Time.deltaTime;
            float n = _t / _life;

            if (n >= 1f)
            {
                Despawn();
                return;
            }

            follower.WorldOffset = Vector3.Lerp(_startOffset, _endOffset, n);

            if (_style == CombatTextStyle.Critical)
            {
                float pop = PopCurve(n, _preset.popInNormalized);
                rect.localScale = Vector3.one * (_preset.baseScale + _preset.popAmount * pop);

                float damp = 1f - n;
                float sx = Mathf.Sin(_t * _preset.shakeFrequency) * _preset.shakeAmplitudePx * damp;
                float sy = Mathf.Cos(_t * (_preset.shakeFrequency * 0.9f)) * (_preset.shakeAmplitudePx * 0.6f) * damp;

                rect.anchoredPosition = _baseAnchored + new Vector2(sx, sy);
            }
            else
            {
                rect.anchoredPosition = _baseAnchored;
            }
        }

        static float PopCurve(float n, float inN)
        {
            inN = Mathf.Clamp(inN, 0.01f, 0.5f);

            if (n <= inN)
            {
                float t = n / inN;
                return Smooth01(t);
            }
            else
            {
                float t = (n - inN) / (1f - inN);
                return 1f - Smooth01(t);
            }
        }

        static float Smooth01(float x) => x * x * (3f - 2f * x);

        void OnDisable()
        {
            follower.ClearTarget();
            rect.anchoredPosition = _baseAnchored;
        }

        void Despawn()
        {
            if (_pool != null) _pool.Return(this);
            else gameObject.SetActive(false);
        }
    }
}


