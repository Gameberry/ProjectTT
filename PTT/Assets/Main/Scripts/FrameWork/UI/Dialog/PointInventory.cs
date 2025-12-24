using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.UI
{
    /// <summary>
    /// PointInventory(지갑) 화면용 스크립트.
    /// - PointTable + PointChart 기반으로 목록 생성/갱신
    /// </summary>
    public class PointInventory : MonoBehaviour
    {
        [SerializeField] private UIPointElement _prefab;
        [SerializeField] private Transform _root;

        private readonly List<UIPointElement> _created = new List<UIPointElement>();

        private void OnEnable()
        {
            Managers.ItemManager.Instance.OnPointChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (Managers.ItemManager.Instance != null)
                Managers.ItemManager.Instance.OnPointChanged -= Refresh;
        }

        public void Refresh()
        {
            var table = Table.UserTable.Get<Table.PointTable>();
            var chart = Chart.GameChart.Get<Chart.PointChart>();
            if (table == null || chart == null || chart.rows == null) return;

            int idx = 0;
            for (int i = 0; i < chart.rows.Length; i++)
            {
                var info = chart.rows[i];
                if (info == null || info.ShowInWallet == false) continue;

                if (idx >= _created.Count)
                {
                    var e = Instantiate(_prefab, _root);
                    _created.Add(e);
                }

                var el = _created[idx];
                el.Set(info.Name, table.GetAmount(info.ItemId));
                el.gameObject.SetActive(true);
                idx++;
            }

            for (int i = idx; i < _created.Count; i++)
                _created[i].gameObject.SetActive(false);
        }
    }
}
