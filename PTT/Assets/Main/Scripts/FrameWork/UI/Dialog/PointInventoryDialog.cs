using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.UI
{
    public class PointInventoryDialog : IDialog
    {
        [SerializeField] private UIItemElement _prefab;
        [SerializeField] private Transform _root;

        private readonly List<UIItemElement> _created = new List<UIItemElement>();

        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            ItemManager.Instance.OnPointChanged += Refresh;
            Refresh();
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (ItemManager.Instance != null)
                ItemManager.Instance.OnPointChanged -= Refresh;
        }
        //------------------------------------------------------------------------------------
        public void Refresh()
        {
            var chart = Chart.GameChart.Get<Chart.PointChart>();
            if (chart == null || chart.rows == null) return;

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
                el.SetMeta(info.ItemId);
                el.gameObject.SetActive(true);
                idx++;
            }

            for (int i = idx; i < _created.Count; i++)
                _created[i].gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
    }
}
