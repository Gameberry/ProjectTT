using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class ShopPackageRotatePopupDialog : IDialog
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
        private TMP_Text m_uIShopSubTitle;

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

        //[SerializeField]
        //private Button selectBtn;


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


        [Header("------------allyJewelryIcon------------")]
        [SerializeField]
        private Transform allyJewelryIcon;

        private ShopPackageRotateGroupData currentRelayPackageGroup = null;
        private ShopPackageRotationData currentRelayPackageData = null;

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

            if (buyBtn != null)
                buyBtn.onClick.AddListener(OnClick_BuyBtn);

            Managers.UnityUpdateManager.Instance.UpdateCoroutineFunc_1Sec += SetRemainTime;

            Message.AddListener<GameBerry.Event.RefreshShopRotateMsg>(RefreshShopRotate);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.RefreshShopRotateMsg>(RefreshShopRotate);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            if (currentRelayPackageGroup == null)
                return;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ExitBtn()
        {
            RequestDialogExit<ShopPackageRotatePopupDialog>();
        }
        //------------------------------------------------------------------------------------
        private void RefreshShopRotate(GameBerry.Event.RefreshShopRotateMsg msg)
        {
            if (msg.shopPackageRotateGroupData != null)
            {
                currentRelayPackageGroup = msg.shopPackageRotateGroupData;
            }

            if (currentRelayPackageGroup == null)
                return;


            for (int i = 0; i < relayPackageBtns.Count; ++i)
            {
                if (currentRelayPackageGroup.ShopPackageRelayDatas.Count <= 1)
                {
                    relayPackageBtns[i].SetData(-1);
                    relayPackageBtns[i].BtnGroup.gameObject.SetActive(false);
                    continue;
                }

                if (currentRelayPackageGroup.ShopPackageRelayDatas.Count > i)
                {
                    relayPackageBtns[i].BtnGroup.gameObject.SetActive(true);
                    relayPackageBtns[i].SetData(currentRelayPackageGroup.ShopPackageRelayDatas[i].Index);
                    relayPackageBtns[i].Price.text = Managers.ShopManager.Instance.GetPriceText(currentRelayPackageGroup.ShopPackageRelayDatas[i]);
                }
                else
                {
                    relayPackageBtns[i].SetData(-1);
                    relayPackageBtns[i].BtnGroup.gameObject.SetActive(false);
                }
            }

            PlayerShopInfo playerShopInfo = Managers.ShopManager.Instance.GetPlayerShopInfo(currentRelayPackageGroup.FirstShopPackageData);

            if (playerShopInfo == null)
                SetRelayPackageData(currentRelayPackageGroup.FirstShopPackageData);
            else if(playerShopInfo.AccumCount > 0)
                SetRelayPackageData(currentRelayPackageGroup.ShopPackageRelayDatas.Find(x => x != currentRelayPackageGroup.FirstShopPackageData));
            else
                SetRelayPackageData(currentRelayPackageGroup.FirstShopPackageData);
        }
        //------------------------------------------------------------------------------------
        private void SetRelayPackageData(ShopPackageRotationData shopPackageRelayData)
        {
            if (currentRelayPackageGroup == null)
                return;

            currentRelayPackageData = shopPackageRelayData;

            if (currentRelayPackageData == null)
                return;

            SetShopData(currentRelayPackageData);

            SetPurchaseConditionUI(currentRelayPackageData);

            RefreshRelayPackageBtnState(shopPackageRelayData);

            if (allyJewelryIcon != null)
                allyJewelryIcon.gameObject.SetActive(currentRelayPackageData != currentRelayPackageGroup.FirstShopPackageData);
        }
        //------------------------------------------------------------------------------------
        private void SetShopData(ShopDataBase shopDataBase)
        {
            SetWorth(shopDataBase);
            SetIntervalLimit(shopDataBase);
            VisibleSoldOut(shopDataBase);
            SetRemainTime();

            if (uIShopTitle != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(uIShopTitle, shopDataBase.TitleLocalStringKey);

            if (m_uIShopSubTitle != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(m_uIShopSubTitle, shopDataBase.SubTitleLocalStringKey);

            bool isSoldOut = false;

            PlayerShopInfo playerShopInfo = Managers.ShopManager.Instance.GetPlayerShopInfo(currentRelayPackageData);

            if (playerShopInfo == null)
                isSoldOut = false;
            else if (playerShopInfo.AccumCount > 0)
                isSoldOut = true;
            else
                isSoldOut = false;



            //bool iscontition = Managers.ShopManager.Instance.CheckProductContitionValue(currentRelayPackageData.PurchaseConditionType, currentRelayPackageData.PurchaseConditionParam);

            bool iscontition = true;

            if (currentRelayPackageData.PurchaseConditionParam != -1)
            {

                PlayerShopInfo prevShopInfo = Managers.ShopManager.Instance.GetPlayerShopInfo(currentRelayPackageData.PurchaseConditionParam);
                if (prevShopInfo == null)
                    iscontition = false;
                else if (prevShopInfo.AccumCount > 0)
                    iscontition = true;
                else
                    iscontition = false;
            }


            if (uISoldOut != null)
                uISoldOut.gameObject.SetActive(isSoldOut);

            if (uIShopPrice != null)
            {
                uIShopPrice.text = Managers.ShopManager.Instance.GetPriceText(shopDataBase);

                if (isSoldOut == true)
                {
                    uIShopPrice.color = Color.gray;
                }
                else
                {
                    uIShopPrice.color = iscontition == false ? Color.gray : Color.white;
                }
            }

            if (buyBtn != null)
            {
                buyBtn.interactable = !isSoldOut;

                if (isSoldOut == true)
                {
                    buyBtn.interactable = false;
                }
                else
                {
                    buyBtn.interactable = iscontition;
                }
            }
            

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
        }
        //------------------------------------------------------------------------------------
        private void RefreshRelayPackageBtnState(ShopPackageRotationData focusData)
        {
            for (int i = 0; i < relayPackageBtns.Count; ++i)
            {
                relayPackageBtns[i].ClickBtn.image.sprite = relayPackageBtns[i].GetCurrentIndex() == focusData.Index ? selectBtnSprite : noneBtnSprite;
            }
        }
        //------------------------------------------------------------------------------------
        private void SetPurchaseConditionUI(ShopPackageRotationData shopPackageRelayData)
        {
            for (int i = 0; i < m_productPurchaseCondition.Count; ++i)
            {
                if (i == 0 && shopPackageRelayData.PurchaseConditionType != V2Enum_OpenConditionType.None)
                {
                    //bool iscontition = Managers.ShopManager.Instance.CheckProductContitionValue(shopPackageRelayData.PurchaseConditionType, shopPackageRelayData.PurchaseConditionParam);
                    bool iscontition = true;

                    if (currentRelayPackageData.PurchaseConditionParam != -1)
                    {
                        PlayerShopInfo prevShopInfo = Managers.ShopManager.Instance.GetPlayerShopInfo(shopPackageRelayData.PurchaseConditionParam);
                        if (prevShopInfo == null)
                            iscontition = false;
                        else if (prevShopInfo.AccumCount > 0)
                            iscontition = true;
                        else
                            iscontition = false;
                    }

                    m_productPurchaseCondition[i].text = Managers.ShopManager.Instance.GetPurchaseConditionText(shopPackageRelayData.PurchaseConditionType, shopPackageRelayData.PurchaseConditionParam);
                    m_productPurchaseCondition[i].fontStyle = iscontition == true ? FontStyles.Strikethrough : FontStyles.Normal;
                    m_productPurchaseCondition[i].color = iscontition == true ? Color.gray : Color.red;
                    m_productPurchaseCondition[i].gameObject.SetActive(true);
                }
                else
                    m_productPurchaseCondition[i].gameObject.SetActive(false);
            }
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
            bool IsSoldOut = false;

            PlayerShopInfo playerShopInfo = Managers.ShopManager.Instance.GetPlayerShopInfo(currentRelayPackageData);

            if (playerShopInfo == null)
                IsSoldOut = false;
            else if (playerShopInfo.AccumCount > 0)
                IsSoldOut = true;
            else
                IsSoldOut = false;

            if (soldOutTrans != null)
                soldOutTrans.gameObject.SetActive(IsSoldOut);

            if (elementBuyLight != null)
                elementBuyLight.gameObject.SetActive(IsSoldOut == false);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_RelayPackageBtn(int index)
        {
            ShopDataBase shopDataBase = Managers.ShopManager.Instance.GetShopData(index);
            if (shopDataBase == null)
                return;

            ShopPackageRotationData shopPackageRelayData = shopDataBase as ShopPackageRotationData;
            if (shopPackageRelayData == null)
                return;

            SetRelayPackageData(shopPackageRelayData);
        }
        //------------------------------------------------------------------------------------
        private void SetRemainTime()
        {
            bool IsSoldOut = false;

            PlayerShopInfo playerShopInfo = Managers.ShopManager.Instance.GetPlayerShopInfo(currentRelayPackageData);

            if (playerShopInfo == null)
                IsSoldOut = false;
            else if (playerShopInfo.AccumCount > 0)
                IsSoldOut = true;
            else
                IsSoldOut = false;


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

            int loginCount = Managers.TimeManager.Instance.GetDayCount();
            int dayss = loginCount % 7;
            int RemainDay = 7 - dayss;

            double dayConvertSecond = Managers.TimeManager.Instance.GetInitAddTime(V2Enum_IntervalType.Day, RemainDay - 1);
            double todayRemainTime = Managers.TimeManager.Instance.DailyInit_TimeStamp - Managers.TimeManager.Instance.Current_TimeStamp;

            int rawSecond = (int)(dayConvertSecond + todayRemainTime);

            if (m_remainTime != null)
            {
                m_remainTime.gameObject.SetActive(true);

                int remainSecond = rawSecond % 60;

                int rawMinute = rawSecond / 60;

                int remainMinute = rawMinute % 60;

                int remainHour = rawMinute / 60;

                string remainTime = string.Format("{0} {1} {2}"
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
            if (currentRelayPackageData == null)
                return;

            Managers.ShopManager.Instance.Buy(currentRelayPackageData);
        }
        //------------------------------------------------------------------------------------

    }
}