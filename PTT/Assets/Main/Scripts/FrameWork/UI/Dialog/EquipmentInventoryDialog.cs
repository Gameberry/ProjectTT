using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameBerry.Table;

namespace GameBerry.UI
{
    public class EquipmentInventoryDialog : IDialog
    {
        [SerializeField] private Button acquireTab;
        [SerializeField] private Button typeTab;
        [SerializeField] private Button rarityTab;

        [SerializeField] private Transform contentRoot;
        [SerializeField] private UIItemElement itemCellPrefab;

        [SerializeField]
        private List<UIEquipmentSlotElement> _equipSlotElements = new List<UIEquipmentSlotElement>();

        private Enum_InventorySort sort = Enum_InventorySort.AcquireSort;
        private readonly List<UIItemElement> _spawned = new();

        protected override void OnLoad()
        {
            if (acquireTab != null) acquireTab.onClick.AddListener(() => SetSort(Enum_InventorySort.AcquireSort));
            if (typeTab != null) typeTab.onClick.AddListener(() => SetSort(Enum_InventorySort.TypeSort));
            if (rarityTab != null) rarityTab.onClick.AddListener(() => SetSort(Enum_InventorySort.RaritySort));

            for (int i = 0; i < _equipSlotElements.Count; ++i)
            {
                _equipSlotElements[i]?.Init();
                _equipSlotElements[i]?.RefreshSlot();
            }
        }

        protected override void OnEnter()
        {
            if (ItemManager.Instance != null)
                ItemManager.Instance.OnEquipmentStorageChanged += Refresh;

            if (EquipmentManager.Instance != null)
                EquipmentManager.Instance.OnEquipSlotChanged += RefreshEquipSlot;

            Refresh();
            RefreshEquipSlot();
        }

        protected override void OnExit()
        {
            if (ItemManager.Instance != null)
                ItemManager.Instance.OnEquipmentStorageChanged -= Refresh;

            if (EquipmentManager.Instance != null)
                EquipmentManager.Instance.OnEquipSlotChanged -= RefreshEquipSlot;
        }

        private void SetSort(Enum_InventorySort s)
        {
            sort = s;
            Refresh();
        }

        private void Refresh()
        {
            var eq = UserTable.Get<EquipmentTable>();
            if (eq == null) return;

            var view = eq.BuildView(sort);
            Rebuild(view);
        }

        private void RefreshEquipSlot()
        {
            for (int i = 0; i < _equipSlotElements.Count; ++i)
            {
                _equipSlotElements[i]?.RefreshSlot();
            }

            for (int i = 0; i < _spawned.Count; i++)
            {
                if (_spawned[i] != null)
                    _spawned[i].Refresh();
            }
        }

        private void Rebuild(List<EquipmentData> view)
        {
            for (int i = 0; i < _spawned.Count; i++)
            {
                if (_spawned[i] != null)
                    Destroy(_spawned[i].gameObject);
            }
            _spawned.Clear();

            if (itemCellPrefab == null || contentRoot == null) return;

            for (int i = 0; i < view.Count; i++)
            {
                var cell = Instantiate(itemCellPrefab, contentRoot);
                cell.Bind(ItemHandle.ForInstance(view[i].itemId, view[i].instanceId));
                _spawned.Add(cell);
            }
        }
    }
}
