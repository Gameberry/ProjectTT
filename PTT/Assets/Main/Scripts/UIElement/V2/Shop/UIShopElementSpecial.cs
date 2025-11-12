using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIShopElementSpecial : UIShopElement
    {
        [SerializeField]
        private Transform m_elementRoot;

        private Dictionary<RewardData, UIGlobalGoodsRewardIconElement> uIGlobalGoodsRewardIconElements = new Dictionary<RewardData, UIGlobalGoodsRewardIconElement>();

        private ShopPackageSpecialData m_currentshopPackageRelayData;

        [SerializeField]
        private Transform m_remainTimeRoot;

        [SerializeField]
        private TMP_Text m_remainTime;

        [SerializeField]
        private Button m_selectReward;

        private bool m_isOverDayRemainTime = false;

        //------------------------------------------------------------------------------------
        public override void Init()
        {
            base.Init();
            Managers.UnityUpdateManager.Instance.UpdateCoroutineFunc_1Sec += SetRemainTime;

            if (m_selectReward != null)
                m_selectReward.onClick.AddListener(OnClick_SelectGoods);
        }
        //------------------------------------------------------------------------------------
        public override void SetShopElement(ShopDataBase shopDataBase)
        {
            base.SetShopElement(shopDataBase);

            ShopPackageSpecialData shopPackageData = shopDataBase as ShopPackageSpecialData;

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

            //if (m_selectReward != null)
            //    m_selectReward.gameObject.SetActive(shopPackageData.SelectRewardData != null);

            if (m_selectReward != null)
                m_selectReward.gameObject.SetActive(false);
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
            SetSelectIcon();
        }
        //------------------------------------------------------------------------------------
        public int GetSpecialPackageIndex()
        {
            if (m_currentshopPackageRelayData == null)
                return -1;

            return m_currentshopPackageRelayData.Index;
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

                //double dayConvertSecond = Managers.TimeManager.Instance.GetInitAddTime(V2Enum_IntervalType.Day, 1);


                //if (dayConvertSecond > rawSecond)
                //{
                //    if (m_isOverDayRemainTime == true)
                //        Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.ShopSpecial);

                //    m_isOverDayRemainTime = false;
                //}
                //else
                //    m_isOverDayRemainTime = true;

                int remainSecond = rawSecond % 60;

                int rawMinute = rawSecond / 60;

                int remainMinute = rawMinute % 60;

                int remainHour = rawMinute / 60;

                string remainTime = string.Format(" : {0} {1} {2}"
                    , string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.HourLocalKey), remainHour)
                    , string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.MinuteLocalKey), remainMinute)
                    , string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.SecondLocalKey), remainSecond)
                    );

                m_remainTime.text = remainTime;
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SelectGoods()
        {
            if (m_currentshopPackageRelayData == null)
                return;

            if (m_currentshopPackageRelayData.SelectRewardData == null)
                return;

            if (m_currentshopPackageRelayData.SelectiveReturnGoodsParam1_Goods == null)
                return;

            if (m_currentshopPackageRelayData.SelectiveReturnGoodsParam1_Goods.Count <= 1)
                return;

            Contents.GlobalContent.ShowSelectGoodsPopup(
                m_currentshopPackageRelayData.SelectiveReturnGoodsType
                , m_currentshopPackageRelayData.SelectiveReturnGoodsParam1_Goods
                , SelectGoodsCallBack);

            //SelectGoodsCallBack(m_currentshopPackageRelayData.SelectiveReturnGoodsParam1_Goods[1]);
        }
        //------------------------------------------------------------------------------------
        private void SelectGoodsCallBack(int goodsId)
        {
            if (m_currentshopPackageRelayData == null)
                return;

            if (m_currentshopPackageRelayData.SelectRewardData == null)
                return;

            if (uIGlobalGoodsRewardIconElements.ContainsKey(m_currentshopPackageRelayData.SelectRewardData) == false)
                return;

            RewardData rewardData = m_currentshopPackageRelayData.SelectRewardData;

            rewardData.Index = goodsId;

            Message.Send(Managers.ShopManager.Instance.GetRefreshMessageID(m_shopDataBase));
        }
        //------------------------------------------------------------------------------------
        private void SetSelectIcon()
        {
            if (m_currentshopPackageRelayData.SelectRewardData == null)
                return;

            RewardData rewardData = m_currentshopPackageRelayData.SelectRewardData;

            UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = uIGlobalGoodsRewardIconElements[rewardData];

            uIGlobalGoodsRewardIconElement.gameObject.SetActive(true);
            uIGlobalGoodsRewardIconElement.SetRewardElement(rewardData);
        }
        //------------------------------------------------------------------------------------
        protected override void OnClick_BuyBtn()
        {
            Managers.ShopManager.Instance.ShowSpecialPackagePopupDialog(m_currentshopPackageRelayData);
        }
        //------------------------------------------------------------------------------------
    }
}