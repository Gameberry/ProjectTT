using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        public const string MSG_MISS = "MISS";
        public const string MSG_CRITICAL = "CRITICAL!";

        [SerializeField] private ScreenSpaceFollower follower;
        [SerializeField] private RectTransform rect;

        [SerializeField] private Image mainTmpIcon;
        [SerializeField] private TMP_Text mainTmp;

        [SerializeField] private TMP_Text comboNumberTmp;
        [SerializeField] private TMP_Text comboLabelTmp;

        private UIFloatingCombatTextPool _pool;

        private CombatTextStyle _style;

        private CombatTextMotionPresetAsset _motion;
        private float _t;
        private float _life;
        private Vector3 _startOffset;
        private Vector3 _endOffset;
        private Vector2 _baseAnchored;

        private float _comboHideTimer;
        private float _comboHideDelay;

        public Transform CurrentTarget { get; private set; }
        private CombatTextSpawner _ownerSpawner;

        public void BindPool(UIFloatingCombatTextPool pool) => _pool = pool;

        void SetupCommon(
            Transform target,
            Vector3 baseWorldOffset,
            Vector2 pixelOffset,
            CombatTextStyle style,
            CombatTextMotionPresetAsset motionPreset,
            CombatTextSpawner ownerSpawner)
        {
            _style = style;
            _motion = motionPreset;
            _ownerSpawner = ownerSpawner;

            _t = 0f;
            if (style == CombatTextStyle.Combo)
                _life = StaticResource.Instance.GetBattleModeStaticData().ComboReleaseTime;
            else
                _life = motionPreset.lifeTime;

            CurrentTarget = target;

            follower.SetTarget(target, baseWorldOffset);
            follower.ScreenPixelOffset = pixelOffset;

            _startOffset = baseWorldOffset;
            _endOffset = baseWorldOffset + Vector3.up * motionPreset.rise;

            rect.localScale = Vector3.one * motionPreset.baseScale;
            _baseAnchored = rect.anchoredPosition;

            _comboHideDelay = motionPreset.comboHideDelay;
            _comboHideTimer = 0f;
        }

        void SetComboVisible(bool comboOn)
        {
            if (mainTmp != null) mainTmp.gameObject.SetActive(!comboOn);
            if (comboNumberTmp != null) comboNumberTmp.gameObject.SetActive(comboOn);
            if (comboLabelTmp != null) comboLabelTmp.gameObject.SetActive(comboOn);
        }

        public void PlayText(
            Transform target,
            Vector3 baseWorldOffset,
            Vector2 pixelOffset,
            CombatTextStyle style,
            string message,
            CombatTextPresetAsset textPreset,
            CombatTextMotionPresetAsset motionPreset,
            Sprite icon = null)
        {
            SetupCommon(target, baseWorldOffset, pixelOffset, style, motionPreset, null);

            SetComboVisible(false);
            textPreset.ApplyTo(mainTmp);
            mainTmp.SetText(message);

            if (icon)
            {
                mainTmpIcon.gameObject.SetActive(true);
                mainTmpIcon.sprite = icon;
            }

            gameObject.SetActive(true);
        }

        public void PlayInt(
            Transform target,
            Vector3 baseWorldOffset,
            Vector2 pixelOffset,
            CombatTextStyle style,
            int value,
            CombatTextPresetAsset textPreset,
            CombatTextMotionPresetAsset motionPreset)
        {
            SetupCommon(target, baseWorldOffset, pixelOffset, style, motionPreset, null);

            SetComboVisible(false);
            textPreset.ApplyTo(mainTmp);
            mainTmp.SetText("{0}", value);

            gameObject.SetActive(true);
        }

        public void PlayDoubleFloorComma(
        Transform target,
        Vector3 baseWorldOffset,
        Vector2 pixelOffset,
        CombatTextStyle style,
        double value,
        CombatTextPresetAsset textPreset,
        CombatTextMotionPresetAsset motionPreset,
        Sprite icon = null)
        {
            SetupCommon(target, baseWorldOffset, pixelOffset, style, motionPreset, null);

            SetComboVisible(false);
            textPreset.ApplyTo(mainTmp);
            Util.SetCommaFromDoubleFloor(mainTmp, value);


            if (icon)
            {
                mainTmpIcon.gameObject.SetActive(true);
                mainTmpIcon.sprite = icon;
            }

            gameObject.SetActive(true);
        }

        public void PlayCombo(
            Transform target,
            Vector3 baseWorldOffset,
            Vector2 pixelOffset,
            long comboValue,
            CombatTextPresetAsset comboNumberPreset,
            CombatTextPresetAsset comboLabelPreset,
            CombatTextMotionPresetAsset motionPreset,
            CombatTextSpawner ownerSpawner)
        {
            SetupCommon(target, baseWorldOffset, pixelOffset, CombatTextStyle.Combo, motionPreset, ownerSpawner);

            SetComboVisible(true);
            comboNumberPreset.ApplyTo(comboNumberTmp);
            comboLabelPreset.ApplyTo(comboLabelTmp);

            Util.SetCommaInteger(comboNumberTmp, comboValue);


            gameObject.SetActive(true);
        }

        public void RefreshCombo(long comboValue)
        {
            Util.SetCommaInteger(comboNumberTmp, comboValue);

            _comboHideTimer = 0f;
            _t = 0f;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;

            if (_style == CombatTextStyle.Combo && _comboHideDelay > 0f)
            {
                _comboHideTimer += dt;
                if (_comboHideTimer >= _comboHideDelay)
                {
                    Despawn();
                    return;
                }
            }

            _t += dt;
            float n = _t / _life;

            if (n >= 1f)
            {
                Despawn();
                return;
            }

            follower.WorldOffset = Vector3.Lerp(_startOffset, _endOffset, n);
            follower.MarkDirty();

            if (_style == CombatTextStyle.Critical)
            {
                float pop = PopCurve(n, _motion.popInNormalized);
                rect.localScale = Vector3.one * (_motion.baseScale + _motion.popAmount * pop);

                float damp = 1f - n;
                float sx = Mathf.Sin(_t * _motion.shakeFrequency) * _motion.shakeAmplitudePx * damp;
                float sy = Mathf.Cos(_t * (_motion.shakeFrequency * 0.9f)) * (_motion.shakeAmplitudePx * 0.6f) * damp;

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
            CurrentTarget = null;
            _ownerSpawner = null;

            mainTmpIcon.gameObject.SetActive(false);
        }

        void Despawn()
        {
            if (_style == CombatTextStyle.Combo && _ownerSpawner != null)
                _ownerSpawner.NotifyComboReturned(this);

            if (_pool != null) _pool.Return(this);
            else gameObject.SetActive(false);
        }
    }
}


