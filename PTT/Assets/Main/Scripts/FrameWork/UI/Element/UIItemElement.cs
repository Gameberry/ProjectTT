using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Table;

namespace GameBerry.UI
{
    public class UIItemElement : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text rarity;
        [SerializeField] private TMP_Text itemType;
        [SerializeField] private TMP_Text detail;

        public void SetMeta(int itemId)
        {
            Chart.ItemInfo itemInfo = ItemManager.Instance.GetItemMeta(itemId);

            if (icon != null)
                icon.sprite = ItemManager.Instance.GetIcon(itemId);

            if (rarity != null)
                rarity.text = itemInfo.Rarity.ToString();

            if (itemType != null)
                itemType.text = itemInfo.ItemType.ToString();

            if (detail != null)
            {
                if (itemInfo.IsStack == false)
                    detail.SetText(string.Empty);
                else
                    detail.SetText("{0}", ItemManager.Instance.GetCount(itemId));
            }
        }

        public void Bind(ItemData e)
        {
            if (e == null) return;

            SetMeta(e.itemId);

            if (detail != null)
            {
                Enum_ItemType enumtype = ItemManager.Instance.GetItemType(e.itemId);

                if (enumtype == Enum_ItemType.Equip)
                {
                    detail.SetText("+{0}", Table.UserTable.Get<EquipmentTable>().GetLevel(e.instanceId));
                }
                else
                {
                    if (e.IsInstance == true)
                        detail.SetText(string.Empty);
                    else
                        detail.SetText("{0}", e.count);
                }
            }
        }
    }
}