using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameBerry.Table;
using TMPro;

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
        [SerializeField] private TMP_Dropdown _bulkSalvageDropdown;
        [SerializeField] private Toggle _autoSalvageToggle;
        [SerializeField] private Button _batchSalvageButton;

        private Enum_InventorySort sort = Enum_InventorySort.AcquireSort;
        private readonly List<UIItemElement> _spawned = new();
        private static readonly Enum_Rarity[] SalvageRarityOptions =
        {
            Enum_Rarity.Max,
            Enum_Rarity.Common,
            Enum_Rarity.Uncommon,
            Enum_Rarity.Rare,
            Enum_Rarity.Epic,
            Enum_Rarity.Legendary,
            Enum_Rarity.Mythic,
            Enum_Rarity.Special,
        };

        protected override void OnLoad()
        {
            if (acquireTab != null) acquireTab.onClick.AddListener(() => SetSort(Enum_InventorySort.AcquireSort));
            if (typeTab != null) typeTab.onClick.AddListener(() => SetSort(Enum_InventorySort.TypeSort));
            if (rarityTab != null) rarityTab.onClick.AddListener(() => SetSort(Enum_InventorySort.RaritySort));

            if (_bulkSalvageDropdown != null)
            {
                _bulkSalvageDropdown.ClearOptions();
                _bulkSalvageDropdown.AddOptions(BuildDropdownOptions());
                _bulkSalvageDropdown.onValueChanged.AddListener(OnBulkSalvageDropdownChanged);
            }

            if (_autoSalvageToggle != null)
                _autoSalvageToggle.onValueChanged.AddListener(OnAutoSalvageToggleChanged);

            if (_batchSalvageButton != null)
                _batchSalvageButton.onClick.AddListener(OnClickBatchSalvage);

            SyncBulkSalvageControls();

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
            SyncBulkSalvageControls();
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
            RefreshBulkSalvageControlsState();
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

        private void SyncBulkSalvageControls()
        {
            if (_bulkSalvageDropdown == null || _autoSalvageToggle == null)
                return;

            EquipmentManager manager = EquipmentManager.Instance;
            if (manager == null)
                return;

            Enum_Rarity threshold = manager.GetAutoSalvageThreshold();
            int dropdownIndex = 0;
            for (int i = 0; i < SalvageRarityOptions.Length; ++i)
            {
                if (SalvageRarityOptions[i] == threshold)
                {
                    dropdownIndex = i;
                    break;
                }
            }

            _bulkSalvageDropdown.SetValueWithoutNotify(dropdownIndex);
            _autoSalvageToggle.SetIsOnWithoutNotify(manager.GetAutoSalvageEnabled());
            RefreshBulkSalvageControlsState();
        }

        private void RefreshBulkSalvageControlsState()
        {
            if (_batchSalvageButton == null || _bulkSalvageDropdown == null)
                return;

            Enum_Rarity selected = GetSelectedSalvageThreshold();
            _batchSalvageButton.interactable = selected != Enum_Rarity.Max;
        }

        private void OnBulkSalvageDropdownChanged(int index)
        {
            EquipmentManager.Instance.SetAutoSalvageThreshold(GetThresholdByIndex(index));
            RefreshBulkSalvageControlsState();
        }

        private void OnAutoSalvageToggleChanged(bool enabled)
        {
            EquipmentManager.Instance.SetAutoSalvageEnabled(enabled);
        }

        private void OnClickBatchSalvage()
        {
            Enum_Rarity threshold = GetSelectedSalvageThreshold();
            if (threshold == Enum_Rarity.Max)
                return;

            int count = EquipmentManager.Instance.SalvageAllAtOrBelow(threshold, true);
            if (count <= 0)
                return;

            Refresh();
            RefreshEquipSlot();
        }

        private Enum_Rarity GetSelectedSalvageThreshold()
        {
            if (_bulkSalvageDropdown == null)
                return Enum_Rarity.Max;

            return GetThresholdByIndex(_bulkSalvageDropdown.value);
        }

        private Enum_Rarity GetThresholdByIndex(int index)
        {
            if (index < 0 || index >= SalvageRarityOptions.Length)
                return Enum_Rarity.Max;

            return SalvageRarityOptions[index];
        }

        private List<TMP_Dropdown.OptionData> BuildDropdownOptions()
        {
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>(SalvageRarityOptions.Length);
            for (int i = 0; i < SalvageRarityOptions.Length; ++i)
                options.Add(new TMP_Dropdown.OptionData(GetRarityOptionLabel(SalvageRarityOptions[i])));

            return options;
        }

        private string GetRarityOptionLabel(Enum_Rarity rarity)
        {
            if (rarity == Enum_Rarity.Max)
                return "Do Not Salvage";

            return $"{rarity} Or Lower";
        }
    }
}
