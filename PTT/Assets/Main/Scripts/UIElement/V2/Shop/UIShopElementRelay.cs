using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIShopElementRelay : UIShopElement
    {
        [Header("-----------Reward-----------")]
        [SerializeField]
        private Transform m_rewardElementRoot;

        private Dictionary<RewardData, UIGlobalGoodsRewardIconElement> uIGlobalGoodsRewardIconElements = new Dictionary<RewardData, UIGlobalGoodsRewardIconElement>();

        [SerializeField]
        private List<TMP_Text> m_productPurchaseCondition;

        [SerializeField]
        private Button m_selectReward;

        private ShopPackageRelayData m_currentshopPackageRelayData;

        private bool m_isshow = false;

        private UIGuideInteractor m_freePurchaseGuideActiveInteractor;

        //------------------------------------------------------------------------------------
        public override void Init()
        {
            base.Init();

            if (m_selectReward != null)
                m_selectReward.onClick.AddListener(OnClick_SelectGoods);
        }
        //------------------------------------------------------------------------------------
        public override void SetShopElement(ShopDataBase shopDataBase)
        {
            base.SetShopElement(shopDataBase);

            ShopPackageRelayData shopPackageData = shopDataBase as ShopPackageRelayData;

            m_currentshopPackageRelayData = shopPackageData;

            foreach (var pair in uIGlobalGoodsRewardIconElements)
            {
                UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = pair.Value;
                Managers.RewardManager.Instance.PoolGoodsRewardIcon(uIGlobalGoodsRewardIconElement);
            }

            uIGlobalGoodsRewardIconElements.Clear();

            for (int i = 0; i < shopDataBase.ShopRewardData.Count; ++i)
            {
                UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = Managers.RewardManager.Instance.GetGoodsRewardIcon_NoneParticle();
                if (uIGlobalGoodsRewardIconElement == null)
                    return;

                RewardData rewardData = shopDataBase.ShopRewardData[i];

                uIGlobalGoodsRewardIconElement.gameObject.SetActive(true);
                uIGlobalGoodsRewardIconElement.transform.SetParent(m_rewardElementRoot);
                uIGlobalGoodsRewardIconElement.SetRewardElement(rewardData);
                uIGlobalGoodsRewardIconElement.ShowLightCircle();
                uIGlobalGoodsRewardIconElements.Add(shopDataBase.ShopRewardData[i], uIGlobalGoodsRewardIconElement);
            }

            //if (m_selectReward != null)
            //    m_selectReward.gameObject.SetActive(shopPackageData.SelectRewardData != null);

            if (m_selectReward != null)
                m_selectReward.gameObject.SetActive(false);

            IsShowGroupBtn(m_isshow);
            SetPurchaseConditionUI();

            if (shopDataBase.PID == "-1")
            {
                if (m_freePurchaseGuideActiveInteractor == null)
                {
                    m_freePurchaseGuideActiveInteractor = m_buyBtn.gameObject.AddComponent<UIGuideInteractor>();
                    m_freePurchaseGuideActiveInteractor.MyGuideType = V2Enum_EventType.FreePurchase;
                    m_freePurchaseGuideActiveInteractor.MyStepID = 3;
                    m_freePurchaseGuideActiveInteractor.FocusParent = m_buyBtn.transform;
                    m_freePurchaseGuideActiveInteractor.FocusAngle = 180.0f;
                    m_freePurchaseGuideActiveInteractor.GuideInteratorActionType = Managers.GuideInteratorActionType.End;
                    m_freePurchaseGuideActiveInteractor.IsAutoSetting = false;
                    m_freePurchaseGuideActiveInteractor.ConnectInteractor();
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void IsShowGroupBtn(bool isshow)
        {
            m_isshow = isshow;

            if (m_isshow == false && m_currentshopPackageRelayData != null)
            {
                bool readyBuy = Managers.ShopManager.Instance.IsPossibleBuy_PackageRelayData(m_currentshopPackageRelayData);

                if (m_buyBtn != null)
                    m_buyBtn.interactable = readyBuy;

                if (m_uIShopPrice != null)
                    m_uIShopPrice.color = readyBuy == true ? Color.white : Color.gray;
            }
            else
            {
                if (m_buyBtn != null)
                    m_buyBtn.interactable = true;

                if (m_uIShopPrice != null)
                    m_uIShopPrice.color = Color.white;
            }
        }
        //------------------------------------------------------------------------------------
        protected override void OnRefresh()
        {
            SetPurchaseConditionUI();
            IsShowGroupBtn(m_isshow);
            SetSelectIcon();
        }
        //------------------------------------------------------------------------------------
        protected override void RefreshLocalize()
        {
            base.RefreshLocalize();
            SetPurchaseConditionUI();
        }
        //------------------------------------------------------------------------------------
        private void SetPurchaseConditionUI()
        {
            for (int i = 0; i < m_productPurchaseCondition.Count; ++i)
            {
                if (i == 0 && m_currentshopPackageRelayData.ProductPurchaseConditionType1 != V2Enum_OpenConditionType.None)
                {
                    bool iscontition = Managers.ShopManager.Instance.CheckProductContitionValue(m_currentshopPackageRelayData.ProductPurchaseConditionType1, m_currentshopPackageRelayData.ProductPurchaseConditionParam1);

                    m_productPurchaseCondition[i].text = Managers.ShopManager.Instance.GetPurchaseConditionText(m_currentshopPackageRelayData.ProductPurchaseConditionType1, m_currentshopPackageRelayData.ProductPurchaseConditionParam1);
                    m_productPurchaseCondition[i].fontStyle = iscontition == true ? FontStyles.Strikethrough : FontStyles.Normal;
                    m_productPurchaseCondition[i].color = iscontition == true ? Color.gray : Color.red;
                    m_productPurchaseCondition[i].gameObject.SetActive(true);
                }
                else if (i == 1 && m_currentshopPackageRelayData.ProductPurchaseConditionType2 != V2Enum_OpenConditionType.None)
                {
                    bool iscontition = Managers.ShopManager.Instance.CheckProductContitionValue(m_currentshopPackageRelayData.ProductPurchaseConditionType2, m_currentshopPackageRelayData.ProductPurchaseConditionParam2);

                    m_productPurchaseCondition[i].text = Managers.ShopManager.Instance.GetPurchaseConditionText(m_currentshopPackageRelayData.ProductPurchaseConditionType2, m_currentshopPackageRelayData.ProductPurchaseConditionParam2);
                    m_productPurchaseCondition[i].fontStyle = iscontition == true ? FontStyles.Strikethrough : FontStyles.Normal;
                    m_productPurchaseCondition[i].color = iscontition == true ? Color.gray : Color.red;
                    m_productPurchaseCondition[i].gameObject.SetActive(true);
                }
                else
                    m_productPurchaseCondition[i].gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        protected override void OnClick_BuyBtn()
        {
            if (m_isshow == false)
            { 
                base.OnClick_BuyBtn();
                return;
            }

            if (m_currentshopPackageRelayData == null)
                return;

            Managers.ShopManager.Instance.ShowRelayGroup(m_currentshopPackageRelayData.RelayGroupIndex);
        }
        //------------------------------------------------------------------------------------
        public int GetGroupIndex()
        {
            if (m_currentshopPackageRelayData == null)
                return -1;

            return m_currentshopPackageRelayData.RelayGroupIndex;
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
    }
}