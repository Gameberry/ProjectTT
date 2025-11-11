using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIShopElementVipPackage : UIShopElement
    {
        [SerializeField]
        private UIGlobalGoodsRewardIconElement m_uIGlobalGoodsRewardIconElement;

        [SerializeField]
        private Image m_packageImage;

        private VipPackageShopData _vipPackageShopData;

        [SerializeField]
        private Transform m_remainTimeGroup;

        [SerializeField]
        private TMP_Text m_remainTime;

        //------------------------------------------------------------------------------------
        public override void Init()
        {
            base.Init();
            Managers.UnityUpdateManager.Instance.UpdateCoroutineFunc_1Sec += SetRemainTime;
        }
        //------------------------------------------------------------------------------------
        public override void SetShopElement(ShopDataBase shopDataBase)
        {
            base.SetShopElement(shopDataBase);

            VipPackageShopData shopDiamondChargeData = shopDataBase as VipPackageShopData;

            _vipPackageShopData = shopDiamondChargeData;

            if (m_packageImage != null)
            {
                m_packageImage.gameObject.SetActive(true);
                m_packageImage.sprite = Managers.VipPackageManager.Instance.GetPackageIcon(shopDiamondChargeData.PackageIconStringKey);
            }

            if (shopDiamondChargeData.ShopRewardData != null
                && shopDiamondChargeData.ShopRewardData.Count > 0)
            {
                V2Enum_Goods v2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(shopDiamondChargeData.ShopRewardData[0].Index);

                if (m_uIGlobalGoodsRewardIconElement != null)
                    m_uIGlobalGoodsRewardIconElement.SetRewardElement(
                        v2Enum_Goods,
                        shopDiamondChargeData.ShopRewardData[0].Index,
                        Managers.GoodsManager.Instance.GetGoodsSprite(v2Enum_Goods.Enum32ToInt(), shopDiamondChargeData.ShopRewardData[0].Index),
                        Managers.GoodsManager.Instance.GetGoodsGrade(v2Enum_Goods.Enum32ToInt(), shopDiamondChargeData.ShopRewardData[0].Index),
                        0.0);
            }

            OnRefresh();
        }
        //------------------------------------------------------------------------------------
        protected override void OnRefresh()
        {
            SetRemainTime();
        }
        //------------------------------------------------------------------------------------
        private void SetRemainTime()
        {
            if (_vipPackageShopData == null)
                return;

            bool canBuy = Managers.VipPackageManager.Instance.CanBuy(_vipPackageShopData);

            if (canBuy == true)
            {
                VisibleSoldOut(false);
            }
            else
            {
                VisibleSoldOut(true);

                //if (m_remainTime != null)
                //{
                //    VipPackageShopInfo playerShopInfo = Managers.VipPackageManager.Instance.GetVipPackageShopInfo(_vipPackageShopData);
                //    if (playerShopInfo == null)
                //        return;

                //    int rawSecond = (int)(playerShopInfo.NextBuyTime - Managers.TimeManager.Instance.Current_TimeStamp);

                //    int remainSecond = rawSecond % 60;

                //    int rawMinute = rawSecond / 60;

                //    int remainMinute = rawMinute % 60;

                //    int remainHour = rawMinute / 60;

                //    string remainTime = string.Format("{0} {1} {2}"
                //        , string.Format(Managers.LocalStringManager.Instance.GetLocalString("time/hour"), remainHour)
                //        , string.Format(Managers.LocalStringManager.Instance.GetLocalString("time/minute"), remainMinute)
                //        , string.Format(Managers.LocalStringManager.Instance.GetLocalString("time/second"), remainSecond)
                //        );

                //    m_remainTime.text = remainTime;
                //}



                if (m_remainTimeGroup != null)
                {
                    m_remainTimeGroup.gameObject.SetActive(_vipPackageShopData.IntervalType != V2Enum_IntervalType.Account);
                }


                if (m_remainTime != null)
                {
                    if (_vipPackageShopData.IntervalType == V2Enum_IntervalType.Account)
                        m_remainTime.gameObject.SetActive(false);
                    else
                    {
                        m_remainTime.gameObject.SetActive(true);

                        VipPackageShopInfo playerShopInfo = Managers.VipPackageManager.Instance.GetVipPackageShopInfo(_vipPackageShopData);
                        if (playerShopInfo == null)
                            return;

                        int remainSecond = (int)(playerShopInfo.NextBuyTime - Managers.TimeManager.Instance.Current_TimeStamp);

                        int remainMinute = remainSecond / 60;

                        if (remainMinute < 1)
                            m_remainTime.text = string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.SecondLocalKey), remainSecond);
                        else
                        {
                            int remainHour = remainMinute / 60;
                            if (remainHour < 1)
                                m_remainTime.text = string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.MinuteLocalKey), remainMinute);
                            else
                            {
                                int remainDay = remainHour / 24;
                                if (remainDay < 1)
                                    m_remainTime.text = string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.HourLocalKey), remainHour);
                                else
                                    m_remainTime.text = string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.DayLocalKey), remainDay);
                            }
                        }
                    }
                }
            }

            
        }
        //------------------------------------------------------------------------------------
        protected override void OnClick_BuyBtn()
        {
            if (_vipPackageShopData == null)
                return;

            Managers.VipPackageManager.Instance.Buy(_vipPackageShopData);
        }
        //------------------------------------------------------------------------------------

    }
}
