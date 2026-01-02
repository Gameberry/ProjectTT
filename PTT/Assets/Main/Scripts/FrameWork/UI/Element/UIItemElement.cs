using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Table;

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

        [SerializeField] private bool _isStatic = false;

        private void Awake()
        {
            if (btn != null)
                btn.onClick.AddListener(() => ItemManager.Instance.ShowItemDesc(_itemId));

            if (_isStatic == true)
            {
                ItemManager.Instance.AddItemRefreshEvent(_itemId, SetStaticAmount);

                SetMeta(_itemId);
            }
        }

        public void SetStaticAmount(long amount)
        {
            Util.SetCommaInteger(_amount, amount);
        }

        public void SetMeta(int itemId)
        {
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
                    _amount.SetText("{0}", ItemManager.Instance.GetItemAmount(itemId));
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
                        _amount.SetText("{0}", e.count);
                }
            }
        }
    }
}