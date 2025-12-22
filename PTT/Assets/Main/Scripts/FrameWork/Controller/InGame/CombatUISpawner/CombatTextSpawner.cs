using UnityEngine;
using GameBerry.UI;

namespace GameBerry
{
    public sealed class CombatTextSpawner : MonoSingleton<CombatTextSpawner>
    {
        [SerializeField] private UIFloatingCombatTextPool pool;
        [SerializeField] private CombatTextPresetLibraryAsset presets;

        // 오프셋은 상황 맞게 조정
        public Vector3 OFFSET_COMBO = new(0f, 0.8f, 0f);
        public Vector3 OFFSET_MISS = new(0f, 0.5f, 0f);
        public Vector3 OFFSET_CRIT = new(0f, 0.6f, 0f);


        protected override void Init()
        {
            pool.Init();
        }

        public void ShowCombo(Transform player, int comboCount)
        {
            var t = pool.Rent();
            t.PlayInt_Combo(player, OFFSET_COMBO, CombatTextStyle.Combo, comboCount, presets.Combo);
        }

        public void ShowMiss(Transform target)
        {
            var t = pool.Rent();
            t.PlayText(target, OFFSET_MISS, CombatTextStyle.Miss, UIFloatingCombatText.MSG_MISS, presets.Miss);
        }

        public void ShowCritical(Transform target)
        {
            var t = pool.Rent();
            t.PlayText(target, OFFSET_CRIT, CombatTextStyle.Critical, UIFloatingCombatText.MSG_CRITICAL, presets.Critical);
        }

        public void ShowDamage(Transform target, int damage, bool isCritical)
        {
            var t = pool.Rent();
            if (isCritical)
                t.PlayInt(target, OFFSET_CRIT, CombatTextStyle.Critical, damage, presets.Critical);
            else
                t.PlayInt(target, OFFSET_CRIT, CombatTextStyle.Damage, damage, presets.Damage);
        }
    }

}
