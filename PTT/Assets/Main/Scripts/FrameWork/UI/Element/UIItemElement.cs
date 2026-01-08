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
        [SerializeField] private Button btn;

        [SerializeField] private int _stackItemId = -1;
        [SerializeField] private bool enableAutoRefresh = false;

        private ItemHandle _handle;

        private void Awake()
        {
            if (btn != null)
                btn.onClick.AddListener(OnClick);

            if (enableAutoRefresh == true)
            {
                _handle = ItemHandle.ForStack(_stackItemId);
                Bind(_handle);
                AddEvent();
            }
        }

        private void OnDestroy()
        {
            if (_handle.itemId > 0)
                ItemManager.Instance.RemoveItemRefreshEvent(_handle.itemId, SetStaticAmount);
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

        private void SetStaticAmount(long amount)
        {
            Util.SetCommaInteger(_amount, amount);
        }

        public void Bind(ItemHandle e)
        {
            int itemId = e.itemId;

            Chart.ItemInfo itemInfo = ItemManager.Instance.GetItemMeta(itemId);

            if (_icon != null)
                _icon.sprite = ItemManager.Instance.GetIcon(itemId);

            if (_frame != null)
                _frame.sprite = StaticResource.Instance.GetRarityFrame(itemInfo.Rarity);

            if (_itemName != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_itemName, ItemManager.Instance.GetItemNameLocalKey(itemId));

            if (_amount != null)
            {
                long amount = 0;

                if (e.isMeta == true)
                {
                    if (e.metaAmount > 0)
                        amount = e.metaAmount;
                }
                else
                {
                    if (itemInfo.IsStack == true)
                        amount = ItemManager.Instance.GetItemAmount(itemId);
                }

                if (amount > 0)
                {
                    Util.SetCommaInteger(_amount, amount);
                    _amount.gameObject.SetActive(true);
                }
                else
                    _amount.gameObject.SetActive(false);
            }


            if (_level != null)
            {
                int level = 0;

                if (e.isMeta == true)
                {
                    if (e.metaLevel > 0)
                        level = e.metaLevel;
                }
                else
                {
                    Enum_ItemType enumtype = ItemManager.Instance.GetItemType(e.itemId);

                    if (enumtype == Enum_ItemType.Equip)
                        level = Table.UserTable.Get<EquipmentTable>().GetLevel(e.instanceId);
                }

                if (level > 0)
                {
                    _level.SetText("+{0}", level);
                    _level.gameObject.SetActive(true);
                }
                else
                    _level.gameObject.SetActive(false);
            }
        }
    }
}