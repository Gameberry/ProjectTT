using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class ShopSpecialPackagePopupDialog : IDialog
    {
        [SerializeField]
        private Button m_exitBtn;

        [SerializeField]
        private UIShopElementSpecial m_element;

        private ShopPackageSpecialData m_shopPackageSpecialData;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (m_exitBtn != null)
                m_exitBtn.onClick.AddListener(OnClick_ExitBtn);

            if (m_element != null)
                m_element.Init();

            Message.AddListener<GameBerry.Event.SetShopSpecialPackagePopupMsg>(SetShopSpecialPackagePopup);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.SetShopSpecialPackagePopupMsg>(SetShopSpecialPackagePopup);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ExitBtn()
        {
            UIManager.DialogExit<ShopSpecialPackagePopupDialog>();
        }
        //------------------------------------------------------------------------------------
        private void SetShopSpecialPackagePopup(GameBerry.Event.SetShopSpecialPackagePopupMsg msg)
        {
            if (m_element != null)
                m_element.SetShopElement(msg.shopPackageSpecialData);
        }
        //------------------------------------------------------------------------------------
        private void RefreshShopSpecial(GameBerry.Event.RefreshShopSpecialMsg msg)
        {
            if (m_shopPackageSpecialData != msg.shopPackageSpecialData)
                return;

            PlayerShopInfo playerShopInfo = Managers.ShopManager.Instance.GetPlayerShopInfo(msg.shopPackageSpecialData);

            if (playerShopInfo == null
                || playerShopInfo.InitTimeStemp < Managers.TimeManager.Instance.Current_TimeStamp)
                OnClick_ExitBtn();
            else
            {
                if (m_element != null)
                    m_element.SetShopElement(msg.shopPackageSpecialData);
            }
        }
        //------------------------------------------------------------------------------------
    }
}