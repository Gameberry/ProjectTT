using System.Collections.Generic;
using UnityEngine;
using GameBerry.Chart;
using GameBerry.Table;

namespace GameBerry.UI
{
    public class LanternAppliedEffectDialog : IDialog
    {
        [SerializeField] private UILanternAppliedEffectElement _rowPrefab;
        [SerializeField] private Transform _contentRoot;

        private readonly List<UILanternAppliedEffectElement> _rows = new List<UILanternAppliedEffectElement>();

        public void RefreshRows()
        {
            if (_rowPrefab == null || _contentRoot == null)
                return;

            EnsureRowCount(0);

            List<Enum_LanternSlotType> unlockedSlots = LanternManager.Instance.GetUnlockedSlots();
            int activeCount = 0;

            for (int i = 0; i < unlockedSlots.Count; ++i)
            {
                int itemId = LanternManager.Instance.GetEquippedLanternId(unlockedSlots[i]);
                if (itemId <= 0)
                    continue;

                LanternInfo info = LanternManager.Instance.GetLanternInfo(itemId);
                LanternData data = LanternManager.Instance.GetLanternData(itemId);
                if (info == null || data == null)
                    continue;

                IReadOnlyDictionary<Enum_Stat, double> equipStats = info.GetEquipStats();
                if (equipStats == null || equipStats.Count <= 0)
                    continue;

                double multiplier = 1.0 + (Mathf.Max(1, data.level) - 1) * 0.1;
                foreach (var kvp in equipStats)
                {
                    EnsureRowCount(activeCount + 1);
                    UILanternAppliedEffectElement row = _rows[activeCount];
                    row.gameObject.SetActive(true);
                    row.Bind(itemId, StatHelper.GetStatDisplayName(kvp.Key), StatHelper.FormatStatDisplayValue(kvp.Key, kvp.Value * multiplier));
                    activeCount++;
                }
            }

            for (int i = activeCount; i < _rows.Count; ++i)
                _rows[i].gameObject.SetActive(false);
        }

        private void EnsureRowCount(int count)
        {
            if (_rowPrefab == null || _contentRoot == null)
                return;

            while (_rows.Count < count)
            {
                UILanternAppliedEffectElement row = Instantiate(_rowPrefab, _contentRoot);
                _rows.Add(row);
            }
        }
    }
}
