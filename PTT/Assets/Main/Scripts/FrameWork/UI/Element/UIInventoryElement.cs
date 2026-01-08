using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Table;
using GameBerry;
using GameBerry.UI;

namespace GameBerry.UI
{
    public class UIInventoryElement : MonoBehaviour
    {
        [SerializeField] private UIItemElement _uIItemElement;
        [SerializeField] private Button _btn;

        private ItemHandle _handle;
        private Action<ItemHandle> _action;

        public void Init(Action<ItemHandle> action)
        {
            _action = action;
        }

        public void SetItem(ItemData e)
        {
            _handle = ItemHandle.FromData(e);


        }
    }
}