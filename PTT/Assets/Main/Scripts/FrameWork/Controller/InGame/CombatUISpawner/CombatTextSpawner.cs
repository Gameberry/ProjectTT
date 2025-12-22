using UnityEngine;
using GameBerry.UI;
using System.Collections.Generic;

namespace GameBerry
{
    public sealed class CombatTextSpawner : MonoSingleton<CombatTextSpawner>
    {
        [SerializeField] private UIFloatingCombatTextPool pool;

        [SerializeField] private CombatTextPresetLibraryAsset textPresets;
        [SerializeField] private CombatTextMotionLibraryAsset motionPresets;

        private static readonly Vector3 BASE_WORLD_COMBO = new(0f, 1.2f, 0f);
        private static readonly Vector3 BASE_WORLD_MISS = new(0f, 1.2f, 0f);
        private static readonly Vector3 BASE_WORLD_CRIT = new(0f, 1.2f, 0f);

        [SerializeField] private float stackResetTime = 0.25f;
        [SerializeField] private float stackStepPixels = 18f;
        [SerializeField] private int stackMax = 4;

        struct TargetStackState
        {
            public float lastSpawnTime;
            public int index;
        }

        private readonly Dictionary<Transform, TargetStackState> _stackByTarget = new(256);
        private readonly Dictionary<Transform, UIFloatingCombatText> _comboByTarget = new(64);

        [SerializeField]
        private bool _showDamageNumber = false;

        Vector2 GetStackedPixelOffset(Transform target)
        {
            float now = Time.unscaledTime;

            _stackByTarget.TryGetValue(target, out var s);

            if (now - s.lastSpawnTime > stackResetTime)
                s.index = 0;
            else
                s.index = (s.index + 1) % Mathf.Max(1, stackMax);

            s.lastSpawnTime = now;
            _stackByTarget[target] = s;

            return new Vector2(0f, s.index * stackStepPixels);
        }

        public void ShowCombo(Transform target, long comboCount)
        {
            if (target == null) return;

            if (_comboByTarget.TryGetValue(target, out var existing) && existing != null && existing.gameObject.activeSelf)
            {
                existing.RefreshCombo(comboCount);
                return;
            }

            var t = pool.Rent();
            _comboByTarget[target] = t;

            t.PlayCombo(
                target,
                BASE_WORLD_COMBO,
                Vector2.zero,
                comboCount,
                textPresets.comboNumber,
                textPresets.comboLabel,
                motionPresets.combo,
                this);
        }

        public void NotifyComboReturned(UIFloatingCombatText comboText)
        {
            if (comboText == null) return;

            Transform target = comboText.CurrentTarget;
            if (target != null && _comboByTarget.TryGetValue(target, out var mapped) && mapped == comboText)
                _comboByTarget.Remove(target);
        }

        public void ShowMiss(Transform target)
        {
            if (target == null) return;

            var t = pool.Rent();
            Vector2 px = GetStackedPixelOffset(target);

            t.PlayText(
                target,
                BASE_WORLD_MISS,
                px,
                CombatTextStyle.Miss,
                UIFloatingCombatText.MSG_MISS,
                textPresets.miss,
                motionPresets.miss);
        }

        public void ShowCritical(Transform target)
        {
            if (target == null) return;

            var t = pool.Rent();
            Vector2 px = GetStackedPixelOffset(target);

            t.PlayText(
                target,
                BASE_WORLD_CRIT,
                px,
                CombatTextStyle.Critical,
                UIFloatingCombatText.MSG_CRITICAL,
                textPresets.critical,
                motionPresets.critical);
        }

        public void ShowDamage(Transform target, double damage, bool isCritical)
        {
            if (target == null) return;

            if (_showDamageNumber == false)
            {
                if (isCritical == true)
                    ShowCritical(target);

                return;
            }

            var t = pool.Rent();
            Vector2 px = GetStackedPixelOffset(target);

            if (isCritical)
            {
                t.PlayDoubleFloorComma(
                    target,
                    BASE_WORLD_CRIT,
                    px,
                    CombatTextStyle.Critical,
                    damage,
                    textPresets.critical,
                    motionPresets.critical);
            }
            else
            {
                t.PlayDoubleFloorComma(
                    target,
                    BASE_WORLD_MISS,
                    px,
                    CombatTextStyle.Damage,
                    damage,
                    textPresets.damage,
                    motionPresets.damage);
            }
        }
    }
}
