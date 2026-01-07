using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.UI
{
    public class PointInventoryDialog : IDialog
    {
        [SerializeField] private UIItemElement _prefab;
        [SerializeField] private Transform _root;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            var chart = Chart.GameChart.Get<Chart.PointChart>();
            if (chart == null || chart.rows == null) return;

            for (int i = 0; i < chart.rows.Length; i++)
            {
                var info = chart.rows[i];
                if (info == null || info.ShowInWallet == false) continue;

                var el = Instantiate(_prefab, _root);
                el.IsDisplay = true;
                el.SetMeta(info.ItemId);
                el.AddEvent();
            }
        }
        //------------------------------------------------------------------------------------
    }
}
