using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Table;

namespace GameBerry.UI
{
    public class UIInventoryItemElement : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text itemType;
        [SerializeField] private TMP_Text detail;

        public void Bind(InventoryEntry e)
        {
            if (e == null) return;

            if (icon != null)
                icon.sprite = Managers.ItemManager.Instance.GetIcon(e.itemId);

            if (title != null)
                title.text = Managers.ItemManager.Instance.GetItemRarity(e.itemId).ToString();

            Enum_ItemType enumtype = Managers.ItemManager.Instance.GetItemType(e.itemId);

            if (itemType != null)
                itemType.text = enumtype.ToString();

            if (detail != null)
            {
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