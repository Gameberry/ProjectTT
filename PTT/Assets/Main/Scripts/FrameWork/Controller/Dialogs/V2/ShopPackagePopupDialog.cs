using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    [System.Serializable]
    public class RelayPackageBtn
    {
        public Transform BtnGroup;
        public Button ClickBtn;
        public TMP_Text Price;

        private int myIndex;

        private System.Action<int> callback;

        public void Init(System.Action<int> action)
        {
            callback = action;

            ClickBtn?.onClick.AddListener(OnClick);
        }

        public void SetData(int index)
        {
            myIndex = index;
        }

        public int GetCurrentIndex()
        {
            return myIndex;
        }

        private void OnClick()
        {
            callback?.Invoke(myIndex);
        }
    }

    public class ShopPackagePopupDialog : IDialog
    {
        [SerializeField]
        private List<Button> exitBtn;

        [Header("------------ElementGroup------------")]
        [SerializeField]
        private Transform worthGroup;

        [SerializeField]
        private TMP_Text worthText;

        [SerializeField]
        private TMP_Text uIShopTitle;

        [SerializeField]
        protected TMP_Text uIShopPrice;

        [SerializeField]
        protected TMP_Text uISoldOut;

        [SerializeField]
        protected Button buyBtn;

        [SerializeField]
        private Transform soldOutTrans;

        [SerializeField]
        private Transform elementBuyLight;

        [SerializeField]
        private TMP_Text intervalLimit;

        [SerializeField]
        private Transform m_remainTimeRoot;

        [SerializeField]
        private TMP_Text m_remainTime;

        [SerializeField]
        private Button selectBtn;


        [SerializeField]
        private List<TMP_Text> m_productPurchaseCondition;


        [SerializeField]
        private Sprite selectBtnSprite;

        [SerializeField]
        private Sprite noneBtnSprite;

        [Header("-----------Reward-----------")]
        [SerializeField]
        private Transform m_rewardElementRoot;

        private Dictionary<RewardData, UIGlobalGoodsRewardIconElement> uIGlobalGoodsRewardIconElements = new Dictionary<RewardData, UIGlobalGoodsRewardIconElement>();


        [Header("------------RelayBtn------------")]
        [SerializeField]
        private List<RelayPackageBtn> relayPackageBtns;

        private Queue<ShopPackageSpecialData> showReadySpecialDataBases = new Queue<ShopPackageSpecialData>();
        
        private Queue<ShopPackageRelayGroupData> shopReadyRelayGroup = new Queue<ShopPackageRelayGroupData>();

        private ShopPackageSpecialData currentSpecialPackageData = null;
        
        private ShopPackageRelayGroupData currentRelayPackageGroup = null;
        private ShopPackageRelayData currentRelayPackageData = null;

        private ShopDataBase myShopDataBase = null;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (exitBtn != null)
            {
                for (int i = 0; i < exitBtn.Count; ++i)
                {
                    if (exitBtn[i] != null)
                        exitBtn[i].onClick.AddListener(OnClick_ExitBtn);
                }
            }

            for (int i = 0; i < relayPackageBtns.Count; ++i)
            {
                relayPackageBtns[i].Init(OnClick_RelayPackageBtn);
            }

            if (selectBtn != null)
                selectBtn.onClick.AddListener(OnClick_SelectGoods);

            if (buyBtn != null)
                buyBtn.onClick.AddListener(OnClick_BuyBtn);

            Managers.UnityUpdateManager.Instance.UpdateCoroutineFunc_1Sec += SetRemainTime;

            Message.AddListener<GameBerry.Event.SetShopSpecialPackagePopupMsg>(SetShopSpecialPackagePopup);
            Message.AddListener<GameBerry.Event.SetRelayPackageGroupPopupMsg>(SetRelayPackageGroupPopup);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.SetShopSpecialPackagePopupMsg>(SetShopSpecialPackagePopup);
            Message.RemoveListener<GameBerry.Event.SetRelayPackageGroupPopupMsg>(SetRelayPackageGroupPopup);
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (myShopDataBase != null)
            {
                if (Managers.ShopManager.isAlive == true)
                    Message.RemoveListener(Managers.ShopManager.Instance.GetRefreshMessageID(myShopDataBase), Refresh);
            }

            myShopDataBase = null;

            currentSpecialPackageData = null;

            currentRelayPackageGroup = null;
            currentRelayPackageData = null;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ExitBtn()
        {
            if (showReadySpecialDataBases.Count > 0)
            {
                currentRelayPackageGroup = null;
                currentRelayPackageData = null;

                SetSpcialPackage(showReadySpecialDataBases.Dequeue());

                return;
            }

            if (shopReadyRelayGroup.Count > 0)
            {
                currentSpecialPackageData = null;

                SetRelayPackageGroup(shopReadyRelayGroup.Dequeue());

                return;
            }

            UIManager.DialogExit<ShopPackagePopupDialog>();
        }
        //------------------------------------------------------------------------------------
        private void SetShopSpecialPackagePopup(GameBerry.Event.SetShopSpecialPackagePopupMsg msg)
        {
            if (currentSpecialPackageData != null || currentRelayPackageGroup != null)
            {
                showReadySpecialDataBases.Enqueue(msg.shopPackageSpecialData);
                return;
            }

            SetSpcialPackage(msg.shopPackageSpecialData);
        }
        //------------------------------------------------------------------------------------
        private void SetRelayPackageGroupPopup(GameBerry.Event.SetRelayPackageGroupPopupMsg msg)
        {
            if (currentSpecialPackageData != null || currentRelayPackageGroup != null)
            {
                shopReadyRelayGroup.Enqueue(msg.RefreshData);
                return;
            }

            SetRelayPackageGroup(msg.RefreshData);
        }
        //------------------------------------------------------------------------------------
        private void SetSpcialPackage(ShopPackageSpecialData shopPackageSpecialData)
        {
            currentSpecialPackageData = shopPackageSpecialData;

            SetShopData(currentSpecialPackageData);

            for (int i = 0; i < relayPackageBtns.Count; ++i)
            {
                relayPackageBtns[i].BtnGroup.gameObject.SetActive(false);
            }

            for (int i = 0; i < m_productPurchaseCondition.Count; ++i)
            {
                m_productPurchaseCondition[i].gameObject.SetActive(false);
            }

            if (selectBtn != null)
            {
                selectBtn.gameObject.SetActive(shopPackageSpecialData.SelectRewardData != null);

                //if (shopPackageSpecialData.SelectRewardData != null)
                //{
                //    if (uIGlobalGoodsRewardIconElements.ContainsKey(shopPackageSpecialData.SelectRewardData) == true)
                //    {
                //        selectBtn.transform.localPosition = uIGlobalGoodsRewardIconElements[shopPackageSpecialData.SelectRewardData].transform.localPosition;
                //        selectBtn.transform.SetAsLastSibling();
                //    }
                //}
            }
        }
        //------------------------------------------------------------------------------------
        private void SetRelayPackageGroup(ShopPackageRelayGroupData shopPackageRelayGroupData)
        {
            currentRelayPackageGroup = shopPackageRelayGroupData;

            for (int i = 0; i < relayPackageBtns.Count; ++i)
            {
                if (shopPackageRelayGroupData.ShopPackageRelayDatas.Count <= 1)
                {
                    relayPackageBtns[i].SetData(-1);
                    relayPackageBtns[i].BtnGroup.gameObject.SetActive(false);
                    continue;
                }

                if (shopPackageRelayGroupData.ShopPackageRelayDatas.Count > i)
                {
                    relayPackageBtns[i].BtnGroup.gameObject.SetActive(true);
                    relayPackageBtns[i].SetData(shopPackageRelayGroupData.ShopPackageRelayDatas[i].Index);
                    relayPackageBtns[i].Price.text = Managers.ShopManager.Instance.GetPriceText(shopPackageRelayGroupData.ShopPackageRelayDatas[i]);
                }
                else
                {
                    relayPackageBtns[i].SetData(-1);
                    relayPackageBtns[i].BtnGroup.gameObject.SetActive(false);
                }
            }

            SetRelayPackageData(currentRelayPackageGroup.FocusShopPackageData);
        }
        //------------------------------------------------------------------------------------
        private void SetRelayPackageData(ShopPackageRelayData shopPackageRelayData)
        {
            currentRelayPackageData = shopPackageRelayData;

            SetShopData(currentRelayPackageData);

            SetPurchaseConditionUI(currentRelayPackageData);

            RefreshRelayPackageBtnState(shopPackageRelayData);

            if (selectBtn != null)
            {
                selectBtn.gameObject.SetActive(shopPackageRelayData.SelectRewardData != null);

                //if (shopPackageRelayData.SelectRewardData != null)
                //{
                //    if (uIGlobalGoodsRewardIconElements.ContainsKey(shopPackageRelayData.SelectRewardData) == true)
                //    {
                //        selectBtn.transform.localPosition = uIGlobalGoodsRewardIconElements[shopPackageRelayData.SelectRewardData].transform.localPosition;
                //        selectBtn.transform.SetAsLastSibling();
                //    }
                //}
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshRelayPackageBtnState(ShopPackageRelayData focusData)
        {
            for (int i = 0; i < relayPackageBtns.Count; ++i)
            {
                relayPackageBtns[i].ClickBtn.image.sprite = relayPackageBtns[i].GetCurrentIndex() == focusData.Index ? selectBtnSprite : noneBtnSprite;
            }
        }
        //------------------------------------------------------------------------------------
        private void SetShopData(ShopDataBase shopDataBase)
        {
            myShopDataBase = shopDataBase;

            SetWorth(shopDataBase);
            SetIntervalLimit(shopDataBase);
            VisibleSoldOut(shopDataBase);
            SetRemainTime();

            if (uIShopTitle != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(uIShopTitle, shopDataBase.TitleLocalStringKey);

            bool isSoldOut = Managers.ShopManager.Instance.IsSoldOut(shopDataBase);

            if (uIShopPrice != null)
            {
                uIShopPrice.text = Managers.ShopManager.Instance.GetPriceText(shopDataBase);
                uIShopPrice.color = isSoldOut == true ? Color.gray : Color.white;
            }

            if (uISoldOut != null)
                uISoldOut.gameObject.SetActive(isSoldOut);

            if (buyBtn != null)
                buyBtn.interactable = !isSoldOut;

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

            if (shopDataBase != null)
                Message.RemoveListener(Managers.ShopManager.Instance.GetRefreshMessageID(shopDataBase), Refresh);

            Message.AddListener(Managers.ShopManager.Instance.GetRefreshMessageID(shopDataBase), Refresh);
        }
        //------------------------------------------------------------------------------------
        private void Refresh()
        {
            SetWorth(myShopDataBase);
            SetIntervalLimit(myShopDataBase);
            VisibleSoldOut(myShopDataBase);

            SetSelectIcon();
        }
        //------------------------------------------------------------------------------------
        protected void SetWorth(ShopDataBase shopDataBase)
        {
            if (shopDataBase == null)
                return;

            if (worthGroup != null && worthText != null)
            {
                worthGroup.gameObject.SetActive(true);
                Managers.LocalStringManager.Instance.SetLocalizeText(worthText, shopDataBase.TagString);
            }
        }
        //------------------------------------------------------------------------------------
        protected void SetIntervalLimit(ShopDataBase shopDataBase)
        {
            if (shopDataBase == null)
                return;

            if (intervalLimit != null)
            {
                if (shopDataBase.IntervalParam > 0)
                {
                    intervalLimit.gameObject.SetActive(true);
                    intervalLimit.text = string.Format("({0}/{1})", Managers.ShopManager.Instance.GetBuyCount(shopDataBase), shopDataBase.IntervalParam);
                }
                else
                    intervalLimit.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        protected void VisibleSoldOut(ShopDataBase shopDataBase)
        {
            bool visible = Managers.ShopManager.Instance.IsSoldOut(shopDataBase);

            if (soldOutTrans != null)
                soldOutTrans.gameObject.SetActive(visible);

            if (elementBuyLight != null)
                elementBuyLight.gameObject.SetActive(visible == false);
        }
        //------------------------------------------------------------------------------------
        private void SetPurchaseConditionUI(ShopPackageRelayData shopPackageRelayData)
        {
            for (int i = 0; i < m_productPurchaseCondition.Count; ++i)
            {
                if (i == 0 && shopPackageRelayData.ProductPurchaseConditionType1 != V2Enum_OpenConditionType.None)
                {
                    bool iscontition = Managers.ShopManager.Instance.CheckProductContitionValue(shopPackageRelayData.ProductPurchaseConditionType1, shopPackageRelayData.ProductPurchaseConditionParam1);

                    m_productPurchaseCondition[i].text = Managers.ShopManager.Instance.GetPurchaseConditionText(shopPackageRelayData.ProductPurchaseConditionType1, shopPackageRelayData.ProductPurchaseConditionParam1);
                    m_productPurchaseCondition[i].fontStyle = iscontition == true ? FontStyles.Strikethrough : FontStyles.Normal;
                    m_productPurchaseCondition[i].color = iscontition == true ? Color.gray : Color.red;
                    m_productPurchaseCondition[i].gameObject.SetActive(true);
                }
                else if (i == 1 && shopPackageRelayData.ProductPurchaseConditionType2 != V2Enum_OpenConditionType.None)
                {
                    bool iscontition = Managers.ShopManager.Instance.CheckProductContitionValue(shopPackageRelayData.ProductPurchaseConditionType2, shopPackageRelayData.ProductPurchaseConditionParam2);

                    m_productPurchaseCondition[i].text = Managers.ShopManager.Instance.GetPurchaseConditionText(shopPackageRelayData.ProductPurchaseConditionType2, shopPackageRelayData.ProductPurchaseConditionParam2);
                    m_productPurchaseCondition[i].fontStyle = iscontition == true ? FontStyles.Strikethrough : FontStyles.Normal;
                    m_productPurchaseCondition[i].color = iscontition == true ? Color.gray : Color.red;
                    m_productPurchaseCondition[i].gameObject.SetActive(true);
                }
                else
                    m_productPurchaseCondition[i].gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_RelayPackageBtn(int index)
        {
            ShopDataBase shopDataBase = Managers.ShopManager.Instance.GetShopData(index);
            if (shopDataBase == null)
                return;

            ShopPackageRelayData shopPackageRelayData = shopDataBase as ShopPackageRelayData;
            if (shopPackageRelayData == null)
                return;

            SetRelayPackageData(shopPackageRelayData);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SelectGoods()
        {
            if (myShopDataBase is ShopPackageSpecialData)
            {
                if (currentSpecialPackageData == null)
                    return;

                if (currentSpecialPackageData.SelectRewardData == null)
                    return;

                if (currentSpecialPackageData.SelectiveReturnGoodsParam1_Goods == null)
                    return;

                if (currentSpecialPackageData.SelectiveReturnGoodsParam1_Goods.Count <= 1)
                    return;

                Contents.GlobalContent.ShowSelectGoodsPopup(
                    currentSpecialPackageData.SelectiveReturnGoodsType
                    , currentSpecialPackageData.SelectiveReturnGoodsParam1_Goods
                    , SelectGoodsCallBack);
            }
            else if (myShopDataBase is ShopPackageRelayData)
            {
                if (currentRelayPackageData == null)
                    return;

                if (currentRelayPackageData.SelectRewardData == null)
                    return;

                if (currentRelayPackageData.SelectiveReturnGoodsParam1_Goods == null)
                    return;

                if (currentRelayPackageData.SelectiveReturnGoodsParam1_Goods.Count <= 1)
                    return;

                Contents.GlobalContent.ShowSelectGoodsPopup(
                    currentRelayPackageData.SelectiveReturnGoodsType
                    , currentRelayPackageData.SelectiveReturnGoodsParam1_Goods
                    , SelectGoodsCallBack);
            }
        }
        //------------------------------------------------------------------------------------
        private void SelectGoodsCallBack(int goodsId)
        {
            if (myShopDataBase is ShopPackageSpecialData)
            {
                if (currentSpecialPackageData == null)
                    return;

                if (currentSpecialPackageData.SelectRewardData == null)
                    return;

                if (uIGlobalGoodsRewardIconElements.ContainsKey(currentSpecialPackageData.SelectRewardData) == false)
                    return;

                RewardData rewardData = currentSpecialPackageData.SelectRewardData;

                rewardData.Index = goodsId;

                Message.Send(Managers.ShopManager.Instance.GetRefreshMessageID(currentSpecialPackageData));
            }
            else if (myShopDataBase is ShopPackageRelayData)
            {
                if (currentRelayPackageData == null)
                    return;

                if (currentRelayPackageData.SelectRewardData == null)
                    return;

                if (uIGlobalGoodsRewardIconElements.ContainsKey(currentRelayPackageData.SelectRewardData) == false)
                    return;

                RewardData rewardData = currentRelayPackageData.SelectRewardData;

                rewardData.Index = goodsId;

                Message.Send(Managers.ShopManager.Instance.GetRefreshMessageID(currentRelayPackageData));
            }
        }
        //------------------------------------------------------------------------------------
        private void SetSelectIcon()
        {
            if (myShopDataBase is ShopPackageSpecialData)
            {
                if (currentSpecialPackageData.SelectRewardData == null)
                    return;

                RewardData rewardData = currentSpecialPackageData.SelectRewardData;

                UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = uIGlobalGoodsRewardIconElements[rewardData];

                uIGlobalGoodsRewardIconElement.gameObject.SetActive(true);
                uIGlobalGoodsRewardIconElement.SetRewardElement(rewardData);
            }
            else if (myShopDataBase is ShopPackageRelayData)
            {
                if (currentRelayPackageData.SelectRewardData == null)
                    return;

                RewardData rewardData = currentRelayPackageData.SelectRewardData;

                UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = uIGlobalGoodsRewardIconElements[rewardData];

                uIGlobalGoodsRewardIconElement.gameObject.SetActive(true);
                uIGlobalGoodsRewardIconElement.SetRewardElement(rewardData);
            }

            
        }
        //------------------------------------------------------------------------------------
        private void SetRemainTime()
        {
            if (myShopDataBase == null)
                return;

            if (myShopDataBase is ShopPackageRelayData)
            {
                if (m_remainTimeRoot != null)
                    m_remainTimeRoot.gameObject.SetActive(false);

                return;
            }

            bool IsSoldOut = Managers.ShopManager.Instance.IsSoldOut(myShopDataBase);
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
                PlayerShopInfo playerShopInfo = Managers.ShopManager.Instance.GetPlayerShopInfo(myShopDataBase);
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
        private void OnClick_BuyBtn()
        {
            if (myShopDataBase == null)
                return;

            Managers.ShopManager.Instance.Buy(myShopDataBase);
        }
        //------------------------------------------------------------------------------------
    }
}