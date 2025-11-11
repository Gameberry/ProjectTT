using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;

namespace GameBerry.UI
{
    public class LobbyStaminaShopDialog : IDialog
    {
        [Header("------------AdCharge------------")]
        [SerializeField]
        private TMP_Text m_remainAd;

        [SerializeField]
        private UIGlobalGoodsRewardIconElement m_adCharge;

        [SerializeField]
        private CButton m_chargeAd;

        [SerializeField]
        private Image m_cannotAd;

        [Header("------------DiaCharge------------")]
        [SerializeField]
        private TMP_Text m_diaPrice;

        [SerializeField]
        private Color m_diaPriceIdleColor;

        [SerializeField]
        private UIGlobalGoodsRewardIconElement m_diaCharge;

        [SerializeField]
        private CButton m_chargeDia;

        protected override void OnLoad()
        {
            if (m_adCharge != null)
            {
                RewardData rewardData = new RewardData();
                rewardData.V2Enum_Goods = V2Enum_Goods.Point;
                rewardData.Index = V2Enum_Point.Stamina.Enum32ToInt();
                rewardData.Amount = Define.StaminaChargeCount;

                m_adCharge.SetRewardElement(rewardData);
            }

            if (m_chargeAd != null)
                m_chargeAd.onClick.AddListener(Managers.StaminaManager.Instance.ChargeAdView);

            if (m_diaCharge != null)
            {
                RewardData rewardData = new RewardData();
                rewardData.V2Enum_Goods = V2Enum_Goods.Point;
                rewardData.Index = V2Enum_Point.Stamina.Enum32ToInt();
                rewardData.Amount = Define.StaminaChargeCount;

                m_diaCharge.SetRewardElement(rewardData);
            }

            if (m_chargeDia != null)
                m_chargeDia.onClick.AddListener(Managers.StaminaManager.Instance.DoCharge_UseDia);

            RefreshStamina(null);

            Message.AddListener<GameBerry.Event.RefreshStaminaMsg>(RefreshStamina);

            Managers.GoodsManager.Instance.AddGoodsRefreshEvent(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.Dia.Enum32ToInt(), RefreshDiaCharge);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.RefreshStaminaMsg>(RefreshStamina);

            if (Managers.GoodsManager.isAlive == true)
                Managers.GoodsManager.Instance.RemoveGoodsRefreshEvent(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.Dia.Enum32ToInt(), RefreshDiaCharge);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.EventDig_Shop);
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (Managers.RedDotManager.isAlive == false)
                return;

            Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.EventDig_Shop);
        }
        //------------------------------------------------------------------------------------
        private void RefreshStamina(GameBerry.Event.RefreshStaminaMsg msg)
        {
            RefreshAdCharge();
            RefreshDiaCharge(0.0);
        }
        //------------------------------------------------------------------------------------
        private void RefreshAdCharge()
        {
            if (m_chargeAd != null)
                m_chargeAd.SetInteractable(Managers.StaminaManager.Instance.RemainAdViewCount() > 0);

            if (m_remainAd != null)
                m_remainAd.SetText("({0}/{1})", Managers.StaminaManager.Instance.RemainAdViewCount(), Define.LimitDailyAdStamina);

            if (m_cannotAd != null)
                m_cannotAd.gameObject.SetActive(Managers.StaminaManager.Instance.CanAdView() == false);
        }
        //------------------------------------------------------------------------------------
        private void RefreshDiaCharge(double amount)
        {
            if (m_chargeDia != null)
                m_chargeDia.SetInteractable(Managers.StaminaManager.Instance.ReadyChargeEnter_UseDia());

            if (m_diaPrice != null)
            {
                m_diaPrice.text = string.Format("{0:#,0}", Managers.StaminaManager.Instance.GetChargeEnterChancePrice());
            }
        }
        //------------------------------------------------------------------------------------
    }
}