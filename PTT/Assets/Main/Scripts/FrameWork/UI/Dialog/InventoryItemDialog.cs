using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameBerry.Managers;
using GameBerry.Table;

namespace GameBerry.UI
{
    public class InventoryItemDialog : IDialog
    {
        [SerializeField] private Button acquireTab;
        [SerializeField] private Button typeTab;
        [SerializeField] private Button rarityTab;

        [SerializeField] private Transform contentRoot;
        [SerializeField] private UIItemElement itemCellPrefab;

        private Enum_InventorySort sort = Enum_InventorySort.AcquireSort;
        private readonly List<UIItemElement> _spawned = new();

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (acquireTab != null) acquireTab.onClick.AddListener(() => SetSort(Enum_InventorySort.AcquireSort));
            if (typeTab != null) typeTab.onClick.AddListener(() => SetSort(Enum_InventorySort.TypeSort));
            if (rarityTab != null) rarityTab.onClick.AddListener(() => SetSort(Enum_InventorySort.RaritySort));
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            ItemManager.Instance.OnInventoryChanged += Refresh;
            Refresh();
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (ItemManager.Instance != null)
                ItemManager.Instance.OnInventoryChanged -= Refresh;
        }
        //------------------------------------------------------------------------------------
        private void SetSort(Enum_InventorySort s)
        {
            sort = s;
            Refresh();
        }
        //------------------------------------------------------------------------------------
        private void Refresh()
        {
            var inv = UserTable.Get<InventoryTable>();
            if (inv == null) return;

            var view = inv.BuildView(sort);
            Rebuild(view);
        }
        //------------------------------------------------------------------------------------
        private void Rebuild(List<InventoryEntry> view)
        {
            for (int i = 0; i < _spawned.Count; i++)
                if (_spawned[i] != null) Destroy(_spawned[i].gameObject);
            _spawned.Clear();

            if (itemCellPrefab == null || contentRoot == null) return;

            for (int i = 0; i < view.Count; i++)
            {
                var cell = Instantiate(itemCellPrefab, contentRoot);
                cell.Bind(view[i]);
                _spawned.Add(cell);
            }
        }
        //------------------------------------------------------------------------------------
    }
}
