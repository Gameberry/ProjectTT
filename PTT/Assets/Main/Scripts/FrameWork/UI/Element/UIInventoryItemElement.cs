using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Table;

namespace GameBerry.UI
{
    public class UIInventoryItemElement : MonoBehaviour
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text sub;

        public void Bind(InventoryEntry e)
        {
            if (e == null) return;

            if (title != null)
                title.text = e.IsInstance ? $"{e.itemId}  (+{e.enhanceLevel})" : $"{e.itemId} x{e.count}";

            if (sub != null)
                sub.text = Managers.ItemManager.Instance.GetItemType(e.itemId).ToString();
        }
    }
}