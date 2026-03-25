using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Table;
using GameBerry;
using GameBerry.UI;

namespace GameBerry.UI
{
    public class UIItemElement : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _frame;
        [SerializeField] private TMP_Text _itemName;
        [SerializeField] private TMP_Text _amount;
        [SerializeField] private TMP_Text _level;

        [SerializeField] private Transform _equipMark; // 장비나 포션 혹은 나중에 무언가

        [SerializeField] private Button btn;

        [SerializeField] private int _stackItemId = -1;
        [SerializeField] private bool enableAutoRefresh = false;

        private ItemHandle _handle;
        private bool _isListeningPlayerExp;

        private void Awake()
        {
            if (btn != null)
                btn.onClick.AddListener(OnClick);

            if (enableAutoRefresh == true)
            {
                _handle = ItemHandle.ForStack(_stackItemId);
                Bind(_handle);
                _amount?.gameObject.SetActive(true);
                ItemManager.Instance.AddItemRefreshEvent(_handle.itemId, SetStaticAmount);
                SetStaticAmount(ItemManager.Instance.GetCount(_handle));
            }
        }

        private void OnDestroy()
        {
            if (_handle.itemId > 0)
                ItemManager.Instance.RemoveItemRefreshEvent(_handle.itemId, SetStaticAmount);

            UnregisterPlayerExpEvent();
        }

        private void OnClick()
        {
            ItemManager.Instance.ShowItemDesc(_handle);
        }

        public void AddEvent()
        {
            if (enableAutoRefresh == true)
                return;

            ItemManager.Instance.AddItemRefreshEvent(_handle.itemId, SetStaticAmount);
        }

        public void RemoveEvent()
        {
            if (enableAutoRefresh == true)
                return;

            ItemManager.Instance.RemoveItemRefreshEvent(_handle.itemId, SetStaticAmount);
        }

        private void SetStaticAmount(long amount)
        {
            if (_amount == null)
                return;

            if (IsExpItem() == false)
            {
                Util.SetCommaInteger(_amount, amount);
                return;
            }

            SetExpAmountText(_handle.isMeta ? _handle.metaAmount : amount, _handle.isMeta);
        }

        public void Refresh()
        {
            Bind(_handle);
        }

        public void Bind(ItemHandle e)
        {
            UnregisterPlayerExpEvent();
            _handle = e;

            int itemId = e.itemId;

            Chart.ItemInfo itemInfo = ItemManager.Instance.GetItemMeta(itemId);
            Enum_ItemType itemType = itemInfo != null ? itemInfo.ItemType : Enum_ItemType.Max;
            Enum_Rarity rarity = itemInfo != null ? itemInfo.Rarity : Enum_Rarity.Common;

            if (itemType == Enum_ItemType.Equip && e.isMeta == false)
                rarity = EquipmentManager.Instance.GetEquipmentRarity(e);

            if (_icon != null)
                _icon.sprite = ItemManager.Instance.GetIcon(itemId);

            if (_frame != null)
                _frame.sprite = StaticResource.Instance.GetRarityFrame(rarity);

            if (_itemName != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_itemName, ItemManager.Instance.GetItemNameLocalKey(itemId));

            if (_amount != null)
            {
                long amount = 0;

                if (e.isMeta == true)
                {
                    if (e.metaAmount > 0)
                    {
                        amount = e.metaAmount;
                        _amount.gameObject.SetActive(true);
                        if (IsExpItem())
                            SetExpAmountText(amount, true);
                        else
                            Util.SetCommaInteger(_amount, amount);
                    }
                    else
                        _amount.gameObject.SetActive(false);
                }
                else
                {
                    if (itemInfo.IsStack == true)
                    {
                        amount = ItemManager.Instance.GetItemAmount(itemId);
                        _amount.gameObject.SetActive(true);
                        if (IsExpItem())
                        {
                            SetExpAmountText(amount, false);
                            RegisterPlayerExpEvent();
                        }
                        else
                            Util.SetCommaInteger(_amount, amount);
                    }
                    else
                        _amount.gameObject.SetActive(false);
                }
            }

            Enum_ItemType enumtype = itemType;

            if (_level != null)
            {
                int level = 0;

                if (e.isMeta == true)
                {
                    if (e.metaLevel > 0)
                        level = e.metaLevel;
                }
                else if (enumtype == Enum_ItemType.Equip)
                {
                    EquipmentData equipmentData = Table.UserTable.Get<EquipmentTable>().GetEquipmentData(e.instanceId);
                    if (equipmentData != null)
                        level = equipmentData.level;
                }

                if (level > 0)
                {
                    _level.SetText("Lv.{0}", level);
                    _level.gameObject.SetActive(true);
                }
                else
                    _level.gameObject.SetActive(false);
            }

            if (_equipMark != null)
            {
                if (e.isMeta == true)
                    _equipMark.gameObject.SetActive(false);
                else
                {
                    if (enumtype == Enum_ItemType.Equip)
                    {
                        _equipMark.gameObject.SetActive(EquipmentManager.Instance.IsEquip(e));
                    }
                    else
                        _equipMark.gameObject.SetActive(false);
                }
            }
        }

        private bool IsExpItem()
        {
            return _handle.itemId > 0 && ItemManager.Instance != null && ItemManager.Instance.IsExpItem(_handle.itemId);
        }

        private void RegisterPlayerExpEvent()
        {
            if (_isListeningPlayerExp || PlayerManager.isAlive == false)
                return;

            PlayerManager.Instance.OnExpChanged += OnPlayerExpChanged;
            _isListeningPlayerExp = true;
        }

        private void UnregisterPlayerExpEvent()
        {
            if (_isListeningPlayerExp == false || PlayerManager.isAlive == false)
                return;

            PlayerManager.Instance.OnExpChanged -= OnPlayerExpChanged;
            _isListeningPlayerExp = false;
        }

        private void OnPlayerExpChanged(double _)
        {
            if (_handle.isMeta || _amount == null || IsExpItem() == false)
                return;

            SetExpAmountText(0, false);
        }

        private void SetExpAmountText(long amount, bool isMeta)
        {
            if (_amount == null || PlayerManager.isAlive == false)
                return;

            float percent = isMeta
                ? PlayerManager.Instance.GetExpPercentFromAmount(amount)
                : PlayerManager.Instance.GetCurrentExpPercent();

            _amount.SetText($"{percent:0.###}%");
        }
    }
}
