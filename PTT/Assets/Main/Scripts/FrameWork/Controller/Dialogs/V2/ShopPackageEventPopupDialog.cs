using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class ShopPackageEventPopupDialog : IDialog
    {
        [SerializeField]
        private List<Button> exitBtn;

        [SerializeField]
        private List<UIShopElementEvent> uIShopElementEvents;

        //------------------------------------------------------------------------------------
        //protected override void OnLoad()
        //{
        //    if (exitBtn != null)
        //    {
        //        for (int i = 0; i < exitBtn.Count; ++i)
        //        {
        //            if (exitBtn[i] != null)
        //                exitBtn[i].onClick.AddListener(OnClick_ExitBtn);
        //        }
        //    }

        //    List<ShopPackageEventData> ShopPackageSpecialDatas = Managers.ShopManager.Instance.GetPackageEventDatas();

        //    for (int i = 0; i < ShopPackageSpecialDatas.Count; ++i)
        //    {
        //        ShopPackageEventData shopPackageSpecialData = ShopPackageSpecialDatas[i];
        //        if (shopPackageSpecialData == null)
        //            continue;

        //        if (Managers.ShopManager.Instance.IsSoldOut(shopPackageSpecialData) == false)
        //        {
        //            UIShopElementEvent uIShopElementEvent = uIShopElementEvents.Find(x => x.MyIndex == shopPackageSpecialData.Index);
        //            if (uIShopElementEvent == null)
        //                continue;
        //            uIShopElementEvent.Init();
        //            uIShopElementEvent.SetShopElement(shopPackageSpecialData);
        //        }
                
        //    }

        //    Message.AddListener<GameBerry.Event.SetShopEventPackagePopupMsg>(SetShopEventPackagePopup);
        //}
        ////------------------------------------------------------------------------------------
        //protected override void OnUnload()
        //{
        //    Message.RemoveListener<GameBerry.Event.SetShopEventPackagePopupMsg>(SetShopEventPackagePopup);
        //}
        ////------------------------------------------------------------------------------------
        //private void SetShopEventPackagePopup(GameBerry.Event.SetShopEventPackagePopupMsg msg)
        //{
        //    for (int i = 0; i < uIShopElementEvents.Count; ++i)
        //    {
        //        uIShopElementEvents[i].gameObject.SetActive(msg.shopPackageEventData.Index == uIShopElementEvents[i].MyIndex);

        //        if (msg.shopPackageEventData.Index == uIShopElementEvents[i].MyIndex)
        //        {
        //            uIShopElementEvents[i].SetShopElement(msg.shopPackageEventData);
        //        }
        //    }
        //}
        ////------------------------------------------------------------------------------------
        //private void OnClick_ExitBtn()
        //{
        //    UIManager.DialogExit<ShopPackageEventPopupDialog>();
        //}
        //------------------------------------------------------------------------------------
    }
}