using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIShopElementEvent : UIShopElement
    {
        [SerializeField]
        private Transform m_elementRoot;

        private Dictionary<RewardData, UIGlobalGoodsRewardIconElement> uIGlobalGoodsRewardIconElements = new Dictionary<RewardData, UIGlobalGoodsRewardIconElement>();

        private ShopPackageEventData m_currentshopPackageRelayData;

        [SerializeField]
        private Transform m_remainTimeRoot;

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

            ShopPackageEventData shopPackageData = shopDataBase as ShopPackageEventData;

            m_currentshopPackageRelayData = shopPackageData;

            foreach (var pair in uIGlobalGoodsRewardIconElements)
            {
                UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = pair.Value;
                Managers.RewardManager.Instance.PoolGoodsRewardIcon(uIGlobalGoodsRewardIconElement);
            }

            uIGlobalGoodsRewardIconElements.Clear();

            for (int i = 0; i < shopDataBase.ShopRewardData.Count; ++i)
            {
                UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = Managers.RewardManager.Instance.GetGoodsRewardIcon();
                if (uIGlobalGoodsRewardIconElement == null)
                    return;

                RewardData rewardData = shopDataBase.ShopRewardData[i];

                uIGlobalGoodsRewardIconElement.gameObject.SetActive(true);
                uIGlobalGoodsRewardIconElement.transform.SetParent(m_elementRoot);
                uIGlobalGoodsRewardIconElement.SetRewardElement(rewardData);
                uIGlobalGoodsRewardIconElement.ShowLightCircle();

                uIGlobalGoodsRewardIconElements.Add(shopDataBase.ShopRewardData[i], uIGlobalGoodsRewardIconElement);
            }
        }
        //------------------------------------------------------------------------------------
        public override void ReleaseElement()
        {
            base.ReleaseElement();
            m_currentshopPackageRelayData = null;
        }
        //------------------------------------------------------------------------------------
        private void SetRemainTime()
        {
            if (m_currentshopPackageRelayData == null)
                return;

            bool IsSoldOut = Managers.ShopManager.Instance.IsSoldOut(m_currentshopPackageRelayData);
            if (IsSoldOut == true)
            {
                if (m_remainTimeRoot != null)
                    m_remainTimeRoot.gameObject.SetActive(false);

                return;
            }
            else
            {
                if (m_remainTimeRoot != null)
                    m_remainTimeRoot.gameObject.SetActive(true);
            }

            if (m_remainTime != null)
            {
                PlayerShopInfo playerShopInfo = Managers.ShopManager.Instance.GetPlayerShopInfo(m_currentshopPackageRelayData);
                if (playerShopInfo == null)
                    return;

                //int rawSecond = (int)(playerShopInfo.InitTimeStemp - Managers.TimeManager.Instance.Current_TimeStamp);

                //double dayConvertSecond = Managers.TimeManager.Instance.GetInitAddTime(V2Enum_IntervalType.Day, 1);


                //if (dayConvertSecond > rawSecond)
                //{
                //    if (m_isOverDayRemainTime == true)
                //        Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.ShopSpecial);

                //    m_isOverDayRemainTime = false;
                //}
                //else
                //    m_isOverDayRemainTime = true;

                //int remainSecond = rawSecond % 60;

                //int rawMinute = rawSecond / 60;

                //int remainMinute = rawMinute % 60;

                //int remainHour = rawMinute / 60;

                //string remainTime = string.Format(" : {0} {1} {2}"
                //    , string.Format(Managers.LocalStringManager.Instance.GetLocalString("time/hour"), remainHour)
                //    , string.Format(Managers.LocalStringManager.Instance.GetLocalString("time/minute"), remainMinute)
                //    , string.Format(Managers.LocalStringManager.Instance.GetLocalString("time/second"), remainSecond)
                //    );

                m_remainTime.text = Managers.TimeManager.Instance.GetSecendToDayString_HMS((int)(playerShopInfo.InitTimeStemp - Managers.TimeManager.Instance.Current_TimeStamp));
            }
        }
        //------------------------------------------------------------------------------------
    }
}