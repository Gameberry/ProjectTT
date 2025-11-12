using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    [System.Serializable]
    public class UIShopLimitBanner
    {
        public int Index;
        public Transform Banner;
    }

    public class UIShopElement_LimitTime : UIShopElement
    {
        [SerializeField]
        private List<UIShopLimitBanner> _banners = new List<UIShopLimitBanner>();

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
                uIGlobalGoodsRewardIconElement.transform.localScale = Vector3.one;

                uIGlobalGoodsRewardIconElements.Add(shopDataBase.ShopRewardData[i], uIGlobalGoodsRewardIconElement);
            }

            for (int i = 0; i < _banners.Count; ++i)
            {
                UIShopLimitBanner uIShopLimitBanner = _banners[i];
                if (uIShopLimitBanner != null && uIShopLimitBanner.Banner != null)
                    uIShopLimitBanner.Banner.gameObject.SetActive(uIShopLimitBanner.Index == shopPackageData.ResourceIndex);
            }

            OnRefresh();
        }
        //------------------------------------------------------------------------------------
        public override void ReleaseElement()
        {
            base.ReleaseElement();
            m_currentshopPackageRelayData = null;
        }
        //------------------------------------------------------------------------------------
        protected override void OnRefresh()
        {
            if (_tagGroup != null)
                _tagGroup.gameObject.SetActive(Managers.ShopManager.Instance.IsSoldOut(m_shopDataBase) == false);
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

                int rawSecond = (int)(playerShopInfo.InitTimeStemp - Managers.TimeManager.Instance.Current_TimeStamp);

                //int remainSecond = rawSecond % 60;

                //int rawMinute = rawSecond / 60;

                //int remainMinute = rawMinute % 60;

                //int remainHour = rawMinute / 60;

                //string remainTime = string.Format("{0} {1} {2}"
                //    , string.Format(Managers.LocalStringManager.Instance.GetLocalString("time/hour"), remainHour)
                //    , string.Format(Managers.LocalStringManager.Instance.GetLocalString("time/minute"), remainMinute)
                //    , string.Format(Managers.LocalStringManager.Instance.GetLocalString("time/second"), remainSecond)
                //    );

                m_remainTime.text = Managers.TimeManager.Instance.GetSecendToDayString_HMS(rawSecond);
            }
        }
        //------------------------------------------------------------------------------------
    }
}
