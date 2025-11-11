using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class ShopPackageLimitTimePopupDialog : IDialog
    {
        [SerializeField]
        private UIShopElement_LimitTime uIShopElementEvent;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (uIShopElementEvent != null)
                uIShopElementEvent.Init();

            Message.AddListener<GameBerry.Event.SetShopEventPackagePopupMsg>(SetShopEventPackagePopup);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.SetShopEventPackagePopupMsg>(SetShopEventPackagePopup);
        }
        //------------------------------------------------------------------------------------
        private void SetShopEventPackagePopup(GameBerry.Event.SetShopEventPackagePopupMsg msg)
        {
            if (uIShopElementEvent != null)
                uIShopElementEvent.SetShopElement(msg.shopPackageEventData);
        }
        //------------------------------------------------------------------------------------
    }
}