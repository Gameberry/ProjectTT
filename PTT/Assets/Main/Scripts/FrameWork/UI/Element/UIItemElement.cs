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
        [SerializeField] private TMP_Text rarity;
        [SerializeField] private TMP_Text itemName;
        [SerializeField] private TMP_Text _amount;
        [SerializeField] private Button btn;

        [SerializeField] private int _itemId = -1;

        private ItemHandle _handle;

        [SerializeField] private bool _isStatic = false;

        public bool IsDisplay = false;

        private void Awake()
        {
            if (btn != null)
                btn.onClick.AddListener(OnClick);

            if (_isStatic == true)
            {
                IsDisplay = true;
                SetMeta(_itemId);
                AddEvent();
            }
        }

        private void OnClick()
        {
            ItemManager.Instance.ShowItemDesc(_handle, IsDisplay);
        }

        public void AddEvent()
        {
            if (_isStatic == true)
                return;

            ItemManager.Instance.AddItemRefreshEvent(_itemId, SetStaticAmount);
        }

        public void SetStaticAmount(long amount)
        {
            Util.SetCommaInteger(_amount, amount);
        }

        public void SetMeta(int itemId)
        {
            _itemId = itemId;
            _handle = ItemHandle.Stack(itemId);
            if (itemId <= 0)
                return;

            Chart.ItemInfo itemInfo = ItemManager.Instance.GetItemMeta(itemId);

            if (_icon != null)
                _icon.sprite = ItemManager.Instance.GetIcon(itemId);

            if (rarity != null)
                rarity.text = itemInfo.Rarity.ToString();

            if (itemName != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(itemName, ItemManager.Instance.GetItemNameLocalKey(itemId));

            if (_amount != null)
            {
                if (itemInfo.IsStack == false)
                    _amount.SetText(string.Empty);
                else
                    Util.SetCommaInteger(_amount, ItemManager.Instance.GetItemAmount(itemId));
            }

            _itemId = itemId;
        }

        public void Bind(ItemData e)
        {
            if (e == null)
            {
                _itemId = -1;
                return;
            }

            SetMeta(e.itemId);

            if (_amount != null)
            {
                Enum_ItemType enumtype = ItemManager.Instance.GetItemType(e.itemId);

                if (enumtype == Enum_ItemType.Equip)
                {
                    _amount.SetText("+{0}", Table.UserTable.Get<EquipmentTable>().GetLevel(e.instanceId));
                }
                else
                {
                    if (e.IsInstance == true)
                        _amount.SetText(string.Empty);
                    else
                        Util.SetCommaInteger(_amount, ItemManager.Instance.GetItemAmount(e.itemId));
                }
            }
        }
    }
}