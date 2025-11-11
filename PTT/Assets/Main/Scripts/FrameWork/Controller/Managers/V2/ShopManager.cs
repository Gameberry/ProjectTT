using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BackEnd;
using CodeStage.AntiCheat.Storage;
using UnityEngine.Purchasing;
using System;

namespace GameBerry.Managers
{
    public class ShopManager : MonoSingleton<ShopManager>
    {
        private string _refreshID = "shopRefresh{0}";

        private string _refreshRelayGroupID = "shopRefreshRelayGroup{0}";

        private WaitForSeconds _checkIntervalContent = new WaitForSeconds(1.0f);

        private Event.RefreshRelayGroupMsg _refreshRelayGroupMsg = new Event.RefreshRelayGroupMsg();
        private Event.SetRelayPackageGroupPopupMsg _setRelayPackageGroupPopupMsg = new Event.SetRelayPackageGroupPopupMsg();

        private Event.RefreshShopSpecialMsg _refreshShopSpecialMsg = new Event.RefreshShopSpecialMsg();
        private Event.SetShopSpecialPackagePopupMsg _setShopSpecialPackagePopupMsg = new Event.SetShopSpecialPackagePopupMsg();

        private Event.RefreshShopEventMsg _refreshShopEventMsg = new Event.RefreshShopEventMsg();
        private Event.SetShopEventPackagePopupMsg _setShopEventPackagePopupMsg = new Event.SetShopEventPackagePopupMsg();

        private Event.RefreshShopRotateMsg _refreshShopRotateMsg = new Event.RefreshShopRotateMsg();

        public bool ShowedRotateGroupData = false;
        public ShopPackageRotateGroupData TodayRotateGroupData = null;

        private Event.SetInGameRewardPopupMsg _setInGameRewardPopupMsg = new Event.SetInGameRewardPopupMsg();

        private Dictionary<string, Sprite> _packageIcons = new Dictionary<string, Sprite>();

        private List<string> m_changeInGameShopInfoUpdate = new List<string>();

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            m_changeInGameShopInfoUpdate.Add(Define.PlayerPointTable);
            m_changeInGameShopInfoUpdate.Add(Define.PlayerShopInfoTable);


            ShopOperator.Init();
        }
        //------------------------------------------------------------------------------------
        public void InitShopContent()
        {
            TimeManager.Instance.OnInitDailyContent += OnInitDailyContent;
            TimeManager.Instance.OnInitWeekContent += OnInitWeekContent;
            TimeManager.Instance.OnInitMonthContent += OnInitMonthContent;

            double dayConvertSecond = TimeManager.Instance.GetInitAddTime(V2Enum_IntervalType.Day, 1);

            double currentTime = TimeManager.Instance.Current_TimeStamp;

            foreach (var pair in ShopContainer.m_allShopDatas)
            {
                ShopDataBase shopDataBase = pair.Value;
                for (int i = 0; i < shopDataBase.ShopRewardData.Count; ++i)
                {
                    RewardData rewardData = shopDataBase.ShopRewardData[i];
                    rewardData.V2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(rewardData.Index);
                }
            }

            List<ShopDiamondChargeData> shopDiamondChargeDatas = GetShopDiamondChargeDatas();

            foreach (var pair in shopDiamondChargeDatas)
            {
                ShopDiamondChargeData shopDataBase = pair;
                for (int i = 0; i < shopDataBase.BonusGoods.Count; ++i)
                {
                    RewardData rewardData = shopDataBase.BonusGoods[i];
                    rewardData.V2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(rewardData.Index);
                }
            }

            foreach (var pair in ShopContainer.m_shopInfo)
            {
                ShopDataBase shopDataBase = GetShopData(pair.Value.Id);

                if (shopDataBase == null)
                {
                    Debug.LogError(string.Format("IsNull {0}", pair.Value.Id));
                    continue;
                }

                ShopContainer.TotalBuyPrice += shopDataBase.PriceKR * pair.Value.AccumCount;

                if (shopDataBase.IntervalType == V2Enum_IntervalType.Account
                    || shopDataBase.IntervalType == V2Enum_IntervalType.None)
                {
                    if (shopDataBase is ShopPackageSpecialData)
                    {
                        if (pair.Value.InitTimeStemp > currentTime)
                        { 
                            ShopOperator.AddStoreSpecialPackageDurationCheckData(shopDataBase as ShopPackageSpecialData);

                            //double remaintime = pair.Value.InitTimeStemp - currentTime;

                            //if (dayConvertSecond > remaintime)
                            //    RedDotManager.Instance.ShowRedDot(ContentDetailList.ShopSpecial);
                        }
                    }

                    continue;
                }

                if (pair.Value.InitTimeStemp < currentTime)
                {
                    if (shopDataBase is ShopPackageRotationData == true)
                    {

                    }
                    else
                    {
                        pair.Value.Count = 0;
                    }

                    if (shopDataBase is ShopPackageSpecialData)
                    {
                        pair.Value.InitTimeStemp = 0.0;
                    }
                    else if (shopDataBase is ShopPackageRotationData)
                    {
                        
                    }
                    else
                    {
                        if (shopDataBase.IntervalType == V2Enum_IntervalType.Day
                        || shopDataBase.IntervalType == V2Enum_IntervalType.Week
                        || shopDataBase.IntervalType == V2Enum_IntervalType.Month)
                        {
                            pair.Value.InitTimeStemp = TimeManager.Instance.GetInitTime(shopDataBase.IntervalType);
                        }
                        else
                            pair.Value.InitTimeStemp = 0.0;
                    }
                }
                else
                {
                    if (shopDataBase is ShopPackageSpecialData)
                    {
                        //double remaintime = pair.Value.InitTimeStemp - currentTime;
                        
                        //if (dayConvertSecond > remaintime)
                        //    RedDotManager.Instance.ShowRedDot(ContentDetailList.ShopSpecial);


                        ShopOperator.AddStoreSpecialPackageDurationCheckData(shopDataBase as ShopPackageSpecialData);
                    }
                }
            }

            foreach (var pair in ShopOperator.GetPackageRelayDatas())
            {
                SetRelayGroupData(pair.Value);
            }


            //TodayRotateGroupData = GetToDayBuyRotateGroupData();
            if (TodayRotateGroupData == null)
                ShowedRotateGroupData = true;
            else
                ShowedRotateGroupData = CheckProductContitionValue(TodayRotateGroupData.OpenConditionType, TodayRotateGroupData.OpenConditionParam);

            StartCoroutine(OtherIntervalContentChecker());
        }
        //------------------------------------------------------------------------------------
        public void CheckUnPaid()
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

#if !UNITY_EDITOR
            string receipt = ObscuredPrefs.Get("receipt", string.Empty);
            Debug.Log(string.Format("미지급 receipt : {0}", receipt));

            string pid = ObscuredPrefs.Get("pid", string.Empty);
            Debug.Log(string.Format("미지급 pid : {0}", pid));


            if (!string.IsNullOrEmpty(receipt) && !string.IsNullOrEmpty(pid))
            {
                BackendReturnObject callback = null;

                Debug.Log("구매성공");

                ShopDataBase shopDataBase = null;

                foreach (var pair in ShopContainer.m_allShopDatas)
                {
                    if (pair.Value.PID == pid)
                    {
                        shopDataBase = pair.Value;
                    }
                }

                string transactionID = ObscuredPrefs.Get("transactionID", string.Empty);
                decimal iapPrice = ObscuredPrefs.Get("iapPrice", 0);
                string iapCurrency = ObscuredPrefs.Get("iapCurrency", string.Empty);
                string orderid = ObscuredPrefs.Get("orderid", string.Empty);

                if (Application.platform == RuntimePlatform.Android)
                    callback = Backend.Receipt.IsValidateGooglePurchase(receipt, shopDataBase == null ? "receiptDescription" : shopDataBase.Description, iapPrice, iapCurrency);
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                    callback = Backend.Receipt.IsValidateApplePurchase(receipt, shopDataBase == null ? "receiptDescription" : shopDataBase.Description, iapPrice, iapCurrency);

                ObscuredPrefs.Set("receipt", string.Empty);
                ObscuredPrefs.Set("pid", string.Empty);
                ObscuredPrefs.Save();

                if (callback.IsSuccess())
                {
                    if (shopDataBase == null)
                        return;

                    Debug.Log("영수증 검증성공");

                    if (shopDataBase is PassData)
                        PassManager.Instance.IAPComplete(shopDataBase);
                    else if (shopDataBase is VipPackageShopData)
                        VipPackageManager.Instance.IAPComplete(shopDataBase);
                    //else if (shopDataBase is EventPassData)
                    //    EventPassManager.Instance.IAPComplete(shopDataBase);
                    //else if (shopDataBase is SevenDayPassData)
                    //{
                    //    if (SevenDayManager.Instance.GetSevenDayPassData(shopDataBase.Index) != null)
                    //        SevenDayManager.Instance.IAPComplete(shopDataBase);
                    //    else if (NewYearManager.Instance.GetNewYearPassData(shopDataBase.Index) != null)
                    //        NewYearManager.Instance.IAPComplete(shopDataBase);
                    //}
                    //else
                    //{
                    //    if (shopDataBase.MenuType == V2Enum_ShopMenuType.EventDigPass)
                    //        EventDigManager.Instance.IAPComplete(shopDataBase);
                    //    else if (shopDataBase.MenuType == V2Enum_ShopMenuType.EventMathRpgPass)
                    //        EventMathRpgManager.Instance.IAPComplete(shopDataBase);
                    //    else
                    //        IAPComplete(shopDataBase);
                    //}

                    if (SceneManager.Instance.BuildElement == BuildEnvironmentEnum.Product)
                    {
                        UnityIAP.Item item = UnityPlugins.iap.GetItem(shopDataBase.PID);
                        ThirdPartyLog.Instance.SendLog_IAP(item.localizedPrice.ToString(), item.isoCurrencyCode, shopDataBase.Index.ToString(), receipt);
                        ThirdPartyLog.Instance.SendLog_Shop_IapEvent(orderid, shopDataBase.Index, shopDataBase.PriceKR);
                    }
                }
                else
                {
                    Debug.Log("영수증 실패");
                    TheBackEnd.TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            }

#endif
        }
        //------------------------------------------------------------------------------------
        public void RefreshPrice()
        {
            foreach (var pair in ShopContainer.m_allShopDatas)
            {
                Message.Send(GetRefreshMessageID(pair.Value));
            }
        }
        //------------------------------------------------------------------------------------
        public void OnInitDailyContent(double nextinittimestamp)
        {
            InitShopIntervalType(V2Enum_IntervalType.Day, nextinittimestamp);

            //if (ShowedRotateGroupData == true)
            //{

            //    TodayRotateGroupData = GetToDayBuyRotateGroupData();

            //    _refreshShopRotateMsg.shopPackageRotateGroupData = TodayRotateGroupData;
            //    Message.Send(_refreshShopRotateMsg);
            //}
        }
        //------------------------------------------------------------------------------------
        public void OnInitWeekContent(double nextinittimestamp)
        {
            InitShopIntervalType(V2Enum_IntervalType.Week, nextinittimestamp);
        }
        //------------------------------------------------------------------------------------
        public void OnInitMonthContent(double nextinittimestamp)
        {
            InitShopIntervalType(V2Enum_IntervalType.Month, nextinittimestamp);
        }
        //------------------------------------------------------------------------------------
        private void InitShopIntervalType(V2Enum_IntervalType v2Enum_IntervalType, double nextinittimestamp)
        {
            if (ShopContainer.m_shopIntervalDatas.ContainsKey(v2Enum_IntervalType) == true)
            {
                List<ShopDataBase> shopDataBases = ShopContainer.m_shopIntervalDatas[v2Enum_IntervalType];

                for (int i = 0; i < shopDataBases.Count; ++i)
                {
                    ShopDataBase shopDataBase = shopDataBases[i];
                    PlayerShopInfo playerShopInfo = GetPlayerShopInfo(shopDataBase);
                    if (playerShopInfo == null)
                        continue;

                    playerShopInfo.Count = 0;
                    playerShopInfo.InitTimeStemp = nextinittimestamp;
                    Message.Send(GetRefreshMessageID(shopDataBase));
                }
            }

            if (ShopContainer.m_shopSpecialPackageIntervalDatas.ContainsKey(v2Enum_IntervalType) == true)
            {
                List<ShopPackageSpecialData> shopDataBases = ShopContainer.m_shopSpecialPackageIntervalDatas[v2Enum_IntervalType];

                for (int i = 0; i < shopDataBases.Count; ++i)
                {
                    ShopPackageSpecialData shopDataBase = shopDataBases[i];
                    PlayerShopInfo playerShopInfo = GetPlayerShopInfo(shopDataBase);
                    if (playerShopInfo == null)
                        continue;

                    playerShopInfo.Count = 0;
                    playerShopInfo.InitTimeStemp = 0;

                    _refreshShopSpecialMsg.shopPackageSpecialData = shopDataBase;
                    Message.Send(_refreshShopSpecialMsg);
                }
            }

            if (ShopContainer.m_shopEventPackageIntervalDatas.ContainsKey(v2Enum_IntervalType) == true)
            {
                List<ShopPackageEventData> shopDataBases = ShopContainer.m_shopEventPackageIntervalDatas[v2Enum_IntervalType];

                for (int i = 0; i < shopDataBases.Count; ++i)
                {
                    ShopPackageEventData shopDataBase = shopDataBases[i];
                    PlayerShopInfo playerShopInfo = GetPlayerShopInfo(shopDataBase);
                    if (playerShopInfo == null)
                        continue;

                    playerShopInfo.Count = 0;
                    playerShopInfo.InitTimeStemp = 0;

                    _refreshShopEventMsg.shopPackageEventData = shopDataBase;
                    Message.Send(_refreshShopEventMsg);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private IEnumerator OtherIntervalContentChecker()
        { // Quarter, Hour 체크 전용
            while (isAlive == true)
            {
                InitOtherIntervalContent(V2Enum_IntervalType.Quarter);
                InitOtherIntervalContent(V2Enum_IntervalType.Hour);

                yield return _checkIntervalContent;
            }
        }
        //------------------------------------------------------------------------------------
        private void InitOtherIntervalContent(V2Enum_IntervalType v2Enum_IntervalType)
        {
            double currentTime = TimeManager.Instance.Current_TimeStamp;

            if (ShopContainer.m_shopIntervalDatas.ContainsKey(v2Enum_IntervalType) == true)
            {
                List<ShopDataBase> shopDataBases = ShopContainer.m_shopIntervalDatas[v2Enum_IntervalType];

                for (int i = 0; i < shopDataBases.Count; ++i)
                {
                    ShopDataBase shopDataBase = shopDataBases[i];
                    PlayerShopInfo playerShopInfo = GetPlayerShopInfo(shopDataBase);
                    if (playerShopInfo == null)
                        continue;

                    if (playerShopInfo.InitTimeStemp < currentTime || playerShopInfo.Count > 0)
                    {
                        playerShopInfo.Count = 0;

                        playerShopInfo.InitTimeStemp = 0.0;

                        Message.Send(GetRefreshMessageID(shopDataBase));
                    }
                }
            }



            if (ShopContainer.m_shopSpecialPackageIntervalDatas.ContainsKey(v2Enum_IntervalType) == true)
            {
                List<ShopPackageSpecialData> shopDataBases = ShopContainer.m_shopSpecialPackageIntervalDatas[v2Enum_IntervalType];

                for (int i = 0; i < shopDataBases.Count; ++i)
                {
                    ShopPackageSpecialData shopDataBase = shopDataBases[i];
                    PlayerShopInfo playerShopInfo = GetPlayerShopInfo(shopDataBase);
                    if (playerShopInfo == null)
                        continue;

                    if (playerShopInfo.InitTimeStemp > 0
                        && playerShopInfo.InitTimeStemp < currentTime)
                    {
                        playerShopInfo.Count = 0;
                        playerShopInfo.InitTimeStemp = 0.0;

                        _refreshShopSpecialMsg.shopPackageSpecialData = shopDataBase;
                        Message.Send(_refreshShopSpecialMsg);
                    }
                }
            }

            if (ShopContainer.m_shopEventPackageIntervalDatas.ContainsKey(v2Enum_IntervalType) == true)
            {
                List<ShopPackageEventData> shopDataBases = ShopContainer.m_shopEventPackageIntervalDatas[v2Enum_IntervalType];

                for (int i = 0; i < shopDataBases.Count; ++i)
                {
                    ShopPackageEventData shopDataBase = shopDataBases[i];
                    PlayerShopInfo playerShopInfo = GetPlayerShopInfo(shopDataBase);
                    if (playerShopInfo == null)
                        continue;

                    if (playerShopInfo.InitTimeStemp > 0
                        && playerShopInfo.InitTimeStemp < currentTime)
                    {
                        playerShopInfo.Count = 0;
                        playerShopInfo.InitTimeStemp = 0.0;

                        _refreshShopEventMsg.shopPackageEventData = shopDataBase;
                        Message.Send(_refreshShopEventMsg);
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public Sprite GetPackageIcon(string path)
        {
            Sprite sp = null;

            if (_packageIcons.ContainsKey(path) == false)
            {
                ResourceLoader.Instance.Load<Sprite>(path, o =>
                {
                    sp = o as Sprite;
                    _packageIcons.Add(path, sp);
                });
            }
            else
                sp = _packageIcons[path];

            return sp;
        }
        //------------------------------------------------------------------------------------
        public List<ShopDiamondChargeData> GetShopDiamondChargeDatas()
        {
            return ShopOperator.GetShopDiamondChargeDatas();
        }
        //------------------------------------------------------------------------------------
        public List<ShopPackageData> GetPackageDatas()
        {
            return ShopOperator.GetPackageDatas();
        }
        //------------------------------------------------------------------------------------
        public Dictionary<int, ShopPackageRelayGroupData> GetPackageRelayDatas()
        {
            return ShopOperator.GetPackageRelayDatas();
        }
        //------------------------------------------------------------------------------------
        public ShopPackageRelayGroupData GetPackageRelayGroupData(int index)
        {
            return ShopOperator.GetPackageRelayGroupData(index);
        }
        //------------------------------------------------------------------------------------
        public List<ShopPackageSpecialData> GetPackageSpecialDatas()
        { 
            return ShopOperator.GetPackageSpecialDatas();
        }
        //------------------------------------------------------------------------------------
        public List<ShopPackageEventData> GetPackageEventDatas()
        {
            return ShopOperator.GetPackageEventDatas();
        }
        //------------------------------------------------------------------------------------
        public List<ShopIngameStoreData> GetShopIngameStoreDatas()
        {
            return ShopOperator.GetShopIngameStoreDatas();
        }
        //------------------------------------------------------------------------------------
        public static Dictionary<int, ShopPackageRotateGroupData> GetPackageRotateDatas()
        {
            return ShopOperator.GetPackageRotateDatas();
        }
        //------------------------------------------------------------------------------------
        public static ShopPackageRotateGroupData GetPackageRotateGroupData(int index)
        {
            return ShopOperator.GetPackageRotateGroupData(index);
        }
        //------------------------------------------------------------------------------------
        public ShopDataBase GetShopData(int index)
        {
            return ShopOperator.GetShopData(index);
        }
        //------------------------------------------------------------------------------------
        public PlayerShopInfo GetPlayerShopInfo(int index)
        {
            return GetPlayerShopInfo(GetShopData(index));
        }
        //------------------------------------------------------------------------------------
        public PlayerShopInfo GetPlayerShopInfo(ShopDataBase shopDataBase)
        {
            return ShopOperator.GetPlayerShopInfo(shopDataBase);
        }
        //------------------------------------------------------------------------------------
        public int GetBuyCount(int index)
        {
            return GetBuyCount(GetShopData(index));
        }
        //------------------------------------------------------------------------------------
        public int GetBuyCount(ShopDataBase shopDataBase)
        {
            PlayerShopInfo playerShopInfo = GetPlayerShopInfo(shopDataBase);

            if (playerShopInfo == null)
                return 0;

            return playerShopInfo.Count;
        }
        //------------------------------------------------------------------------------------
        public string GetPurchaseConditionText(V2Enum_OpenConditionType v2Enum_ProductConditionType, int ConditionParam)
        {
            return Managers.ContentOpenConditionManager.Instance.GetOpenContitionLocalString(v2Enum_ProductConditionType, ConditionParam);
        }
        //------------------------------------------------------------------------------------
        public void RefreshProductContitionType(V2Enum_OpenConditionType v2Enum_ProductConditionType)
        {
            if (ShopContainer.m_activeCheckDatas.ContainsKey(v2Enum_ProductConditionType) == true)
            {
                List<ShopPackageRelayGroupData> shopPackageRelayGroupDatas = ShopContainer.m_activeCheckDatas[v2Enum_ProductConditionType];

                for (int i = 0; i < shopPackageRelayGroupDatas.Count; ++i)
                {
                    ShopPackageRelayGroupData shopPackageRelayGroupData = shopPackageRelayGroupDatas[i];

                    if (CheckProductContitionValue(v2Enum_ProductConditionType, shopPackageRelayGroupData.FocusShopPackageData.ProductActivateConditionParam) == true)
                    {
                        shopPackageRelayGroupDatas.Remove(shopPackageRelayGroupData);
                        SetRelayGroupData(shopPackageRelayGroupData);

                        _refreshRelayGroupMsg.RefreshData = shopPackageRelayGroupData;
                        Message.Send(_refreshRelayGroupMsg);
                        
                        if (shopPackageRelayGroupData.FocusShopPackageData.DisplayType == V2Enum_DisplayType.Dynamic)
                        {
                            // 돌발 팝업 띄어주기
                            ShowRelayGroup(shopPackageRelayGroupData);

                            ThirdPartyLog.Instance.SendLog_Shop_PopEvent(shopPackageRelayGroupData.RelayGroupIndex);
                        }

                        i -= 1;
                    }
                }
            }

            if (ShopContainer.m_productConditionDatas.ContainsKey(v2Enum_ProductConditionType) == true)
            {
                List<ShopPackageRelayData> shopPackageRelayDatas = ShopContainer.m_productConditionDatas[v2Enum_ProductConditionType];

                for (int i = 0; i < shopPackageRelayDatas.Count; ++i)
                {
                    ShopPackageRelayData shopPackageRelayData = shopPackageRelayDatas[i];

                    if (v2Enum_ProductConditionType == shopPackageRelayData.ProductPurchaseConditionType1)
                    {
                        if (CheckProductContitionValue(v2Enum_ProductConditionType, shopPackageRelayData.ProductPurchaseConditionParam1) == true)
                        {
                            shopPackageRelayDatas.Remove(shopPackageRelayData);
                            Message.Send(GetRefreshMessageID(shopPackageRelayData));

                            i -= 1;
                        }
                    }
                    else if(v2Enum_ProductConditionType == shopPackageRelayData.ProductPurchaseConditionType2)
                    {
                        if (CheckProductContitionValue(v2Enum_ProductConditionType, shopPackageRelayData.ProductPurchaseConditionParam2) == true)
                        {
                            shopPackageRelayDatas.Remove(shopPackageRelayData);
                            Message.Send(GetRefreshMessageID(shopPackageRelayData));

                            i -= 1;
                        }
                    }
                    else
                    {
                        shopPackageRelayDatas.Remove(shopPackageRelayData);
                        i -= 1;
                    }
                }
            }


            //if (ShopContainer.m_productConditionSpcialDatas.ContainsKey(v2Enum_ProductConditionType) == true)
            //{
            //    List<ShopPackageSpecialData> shopPackageSpecialDatas = ShopContainer.m_productConditionSpcialDatas[v2Enum_ProductConditionType];

            //    for (int i = 0; i < shopPackageSpecialDatas.Count; ++i)
            //    {
            //        ShopPackageSpecialData shopPackageSpecialData = shopPackageSpecialDatas[i];

            //        PlayerShopInfo playerShopInfo = GetPlayerShopInfo(shopPackageSpecialData.Index);
            //        if (playerShopInfo != null)
            //        {
            //            if (shopPackageSpecialData.OpenConditionType == V2Enum_OpenConditionType.DiamondDungeonTicketRemainder
            //        || shopPackageSpecialData.OpenConditionType == V2Enum_OpenConditionType.MasteryDungonTicketRemainder
            //        || shopPackageSpecialData.OpenConditionType == V2Enum_OpenConditionType.GoldDungeonTicketRemainder
            //        || shopPackageSpecialData.OpenConditionType == V2Enum_OpenConditionType.SoulStoneDungeonTicketRemainder
            //        || shopPackageSpecialData.OpenConditionType == V2Enum_OpenConditionType.RuneDungeonTicketRemainder
            //        || shopPackageSpecialData.OpenConditionType == V2Enum_OpenConditionType.HellDungeonTicketRemainder)
            //            {
            //            }
            //            else
            //                continue;
            //        }

            //        if (CheckProductContitionValue(v2Enum_ProductConditionType, shopPackageSpecialData.OpenConditionParam) == true)
            //        {
            //            ShowSpecialPackage(shopPackageSpecialData);
            //            ThirdPartyLog.Instance.SendLog_Shop_PopEvent(shopPackageSpecialData.Index);

            //            if (shopPackageSpecialData.OpenConditionType == V2Enum_OpenConditionType.DiamondDungeonTicketRemainder
            //        || shopPackageSpecialData.OpenConditionType == V2Enum_OpenConditionType.MasteryDungonTicketRemainder
            //        || shopPackageSpecialData.OpenConditionType == V2Enum_OpenConditionType.GoldDungeonTicketRemainder
            //        || shopPackageSpecialData.OpenConditionType == V2Enum_OpenConditionType.SoulStoneDungeonTicketRemainder
            //        || shopPackageSpecialData.OpenConditionType == V2Enum_OpenConditionType.RuneDungeonTicketRemainder
            //        || shopPackageSpecialData.OpenConditionType == V2Enum_OpenConditionType.HellDungeonTicketRemainder)
            //            {
            //            }
            //            else
            //            { 
            //                shopPackageSpecialDatas.Remove(shopPackageSpecialData);
            //                i -= 1;
            //            }
            //            //Message.Send(GetRefreshMessageID(shopPackageSpecialData));

                        
            //        }
            //    }
            //}


            if (ShopContainer.m_productConditionEventDatas.ContainsKey(v2Enum_ProductConditionType) == true)
            {
                List<ShopPackageEventData> shopPackageEventDatas = ShopContainer.m_productConditionEventDatas[v2Enum_ProductConditionType];

                for (int i = 0; i < shopPackageEventDatas.Count; ++i)
                {
                    ShopPackageEventData shopPackageEventData = shopPackageEventDatas[i];

                    PlayerShopInfo playerShopInfo = GetPlayerShopInfo(shopPackageEventData.Index);
                    if (playerShopInfo != null)
                        continue;

                    if (CheckProductContitionValue(v2Enum_ProductConditionType, shopPackageEventData.OpenConditionParam) == true)
                    {
                        ShowEventPackage(shopPackageEventData);
                        ThirdPartyLog.Instance.SendLog_Shop_PopEvent(shopPackageEventData.Index);

                        shopPackageEventDatas.Remove(shopPackageEventData);
                        i -= 1;
                    }
                }
            }


            if (TodayRotateGroupData != null && ShowedRotateGroupData == false)
            {
                if (TodayRotateGroupData.OpenConditionType == v2Enum_ProductConditionType)
                {
                    if (CheckProductContitionValue(v2Enum_ProductConditionType, TodayRotateGroupData.OpenConditionParam) == true)
                    {
                        ShowedRotateGroupData = true;

                        _refreshShopRotateMsg.shopPackageRotateGroupData = TodayRotateGroupData;
                        Message.Send(_refreshShopRotateMsg);

                        ThirdPartyLog.Instance.SendLog_Shop_PopEvent(TodayRotateGroupData.FirstShopPackageData.RelayGroupIndex);

                        ShowToDayBuyRotateGroupDialog();
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public bool CheckProductContitionValue(V2Enum_OpenConditionType v2Enum_ProductConditionType, int value)
        {
            if (v2Enum_ProductConditionType == V2Enum_OpenConditionType.None)
                return true;

            return ContentOpenConditionManager.Instance.IsOpen(v2Enum_ProductConditionType, value);
        }
        //------------------------------------------------------------------------------------
        private void SetRelayGroupData(ShopPackageRelayGroupData shopPackageRelayGroupData)
        {
            if (shopPackageRelayGroupData == null)
                return;

            for (int i = 0; i < shopPackageRelayGroupData.ShopPackageRelayDatas.Count; ++i)
            {
                ShopPackageRelayData shopPackageRelayData = shopPackageRelayGroupData.ShopPackageRelayDatas[i];

                if (shopPackageRelayGroupData.IsActive == false)
                {
                    shopPackageRelayGroupData.IsActive = IsActiveRelayData(shopPackageRelayData);
                }

                shopPackageRelayGroupData.FocusShopPackageData = shopPackageRelayData;

                shopPackageRelayGroupData.IsSoldOut = IsSoldOut(shopPackageRelayData);

                if (shopPackageRelayGroupData.IsSoldOut == false)
                    break;
            }

            if (shopPackageRelayGroupData.IsActive == false)
            {
                ShopOperator.AddRelayGroupDataActiveCheck(shopPackageRelayGroupData);
            }
            else if (shopPackageRelayGroupData.IsActive == true
                && shopPackageRelayGroupData.IsSoldOut == false)
            {
                for (int i = 0; i < shopPackageRelayGroupData.ShopPackageRelayDatas.Count; ++i)
                {
                    ShopPackageRelayData shopPackageRelayData = shopPackageRelayGroupData.ShopPackageRelayDatas[i];

                    if (CheckProductContitionValue(shopPackageRelayData.ProductPurchaseConditionType1, shopPackageRelayData.ProductPurchaseConditionParam1) == false)
                    {
                        ShopOperator.AddRelayDataProductPurchaseCheck(shopPackageRelayData.ProductPurchaseConditionType1, shopPackageRelayData);
                    }

                    if(CheckProductContitionValue(shopPackageRelayData.ProductPurchaseConditionType2, shopPackageRelayData.ProductPurchaseConditionParam2) == false)
                    {
                        ShopOperator.AddRelayDataProductPurchaseCheck(shopPackageRelayData.ProductPurchaseConditionType2, shopPackageRelayData);
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void ShowRelayGroup(int index)
        {
            ShowRelayGroup(GetPackageRelayGroupData(index));
        }
        //------------------------------------------------------------------------------------
        public void ShowRelayGroup(ShopPackageRelayGroupData shopPackageRelayGroupData)
        {
            if (shopPackageRelayGroupData == null)
                return;

            _setRelayPackageGroupPopupMsg.RefreshData = shopPackageRelayGroupData;


            Message.Send(_setRelayPackageGroupPopupMsg);
            UI.IDialog.RequestDialogEnter<UI.ShopPackagePopupDialog>();
        }
        //------------------------------------------------------------------------------------
        public void ShowSpecialPackage(ShopPackageSpecialData shopPackageSpecialData)
        {
            //PlayerShopInfo playerShopInfo = GetPlayerShopInfo(shopPackageSpecialData);
            //if (playerShopInfo != null)
            //{
            //    if (shopPackageSpecialData.OpenConditionType == V2Enum_OpenConditionType.DiamondDungeonTicketRemainder
            //        || shopPackageSpecialData.OpenConditionType == V2Enum_OpenConditionType.MasteryDungonTicketRemainder
            //        || shopPackageSpecialData.OpenConditionType == V2Enum_OpenConditionType.GoldDungeonTicketRemainder
            //        || shopPackageSpecialData.OpenConditionType == V2Enum_OpenConditionType.SoulStoneDungeonTicketRemainder
            //        || shopPackageSpecialData.OpenConditionType == V2Enum_OpenConditionType.RuneDungeonTicketRemainder
            //        || shopPackageSpecialData.OpenConditionType == V2Enum_OpenConditionType.HellDungeonTicketRemainder)
            //    {
            //        if (playerShopInfo.InitTimeStemp > TimeManager.Instance.Current_TimeStamp)
            //            return;

            //        if (IsSoldOut(playerShopInfo) == true)
            //            return;
            //    }
            //    else
            //        return;
            //}
            //else
            //{
            //    playerShopInfo = ShopOperator.CreatePlayerShopInfo(shopPackageSpecialData);
            //}
            
            //playerShopInfo.InitTimeStemp = TimeManager.Instance.Current_TimeStamp
            //    + TimeManager.Instance.GetInitAddTime(shopPackageSpecialData.DurationType, shopPackageSpecialData.DurationParam);

            //_refreshShopSpecialMsg.shopPackageSpecialData = shopPackageSpecialData;
            //Message.Send(_refreshShopSpecialMsg);
            //ShopOperator.AddStoreSpecialPackageDurationCheckData(shopPackageSpecialData);

            //if (shopPackageSpecialData.DisplayType == V2Enum_DisplayType.Dynamic)
            //{
            //    ShowSpecialPackagePopupDialog(shopPackageSpecialData);
            //}


            //TheBackEnd.TheBackEndManager.Instance.UpdatePlayerShopInfoTable(null);
        }
        //------------------------------------------------------------------------------------
        public void ShowSpecialPackagePopupDialog(ShopPackageSpecialData shopPackageSpecialData)
        {
            _setShopSpecialPackagePopupMsg.shopPackageSpecialData = shopPackageSpecialData;
            Message.Send(_setShopSpecialPackagePopupMsg);

            UI.IDialog.RequestDialogEnter<UI.ShopPackagePopupDialog>();
        }
        //------------------------------------------------------------------------------------
        public void ShowEventPackage(ShopPackageEventData shopPackageEventData)
        {
            PlayerShopInfo playerShopInfo = GetPlayerShopInfo(shopPackageEventData);
            if (playerShopInfo != null)
            {
                return;
            }
            else
            {
                playerShopInfo = ShopOperator.CreatePlayerShopInfo(shopPackageEventData);
            }

            playerShopInfo.InitTimeStemp = TimeManager.Instance.Current_TimeStamp
                + TimeManager.Instance.GetInitAddTime(shopPackageEventData.DurationType, shopPackageEventData.DurationParam);

            _refreshShopEventMsg.shopPackageEventData = shopPackageEventData;
            Message.Send(_refreshShopEventMsg);
            ShopOperator.AddStoreEventPackageDurationCheckData(shopPackageEventData);

            if (shopPackageEventData.DisplayType == V2Enum_DisplayType.Dynamic)
            {
                ShowEventPackagePopupDialog(shopPackageEventData);
            }

            TheBackEnd.TheBackEndManager.Instance.UpdatePlayerShopInfoTable(null);
        }
        //------------------------------------------------------------------------------------
        public void ShowEventPackagePopupDialog(ShopPackageEventData shopPackageEventData)
        {
            if (shopPackageEventData == null)
                return;

            _setShopEventPackagePopupMsg.shopPackageEventData = shopPackageEventData;
            Message.Send(_setShopEventPackagePopupMsg);

            UI.IDialog.RequestDialogEnter<UI.ShopPackageLimitTimePopupDialog>();
        }
        //------------------------------------------------------------------------------------
        public ShopPackageRotateGroupData GetToDayBuyRotateGroupData()
        {
            int loginCount = TimeManager.Instance.GetDayCount();

            int dayss = loginCount % 7;

            Dictionary<int, ShopPackageRotateGroupData> rotateDatas = ShopOperator.GetPackageRotateDatas();

            List<ShopPackageRotateGroupData> copyrotateDatas = new List<ShopPackageRotateGroupData>();

            foreach (var pair in rotateDatas)
            {
                PlayerShopInfo playerShopInfo = GetPlayerShopInfo(pair.Value.FirstShopPackageData);
                if (playerShopInfo == null)
                {
                    copyrotateDatas.Add(pair.Value);
                }
                else
                {
                    if (playerShopInfo.AccumCount < 1)
                    {
                        copyrotateDatas.Add(pair.Value);
                    }
                }
            }


            ShopPackageRotateGroupData shopPackageRotateGroupData = null;


            bool toWeekSoldOut = false;

            foreach (var pair in rotateDatas)
            {
                ShopPackageRotateGroupData selectdata = pair.Value;
                PlayerShopInfo playerShopInfo = GetPlayerShopInfo(selectdata.FirstShopPackageData);
                if (playerShopInfo == null)
                {
                    continue;
                }
                else
                {
                    if (playerShopInfo.AccumCount > 0)
                    {
                        DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        dt = dt.AddSeconds(playerShopInfo.InitTimeStemp);

                        DateTime ct = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        ct = ct.AddSeconds(TimeManager.Instance.Current_TimeStamp);

                        TimeSpan dayCount = ct.Date - dt.Date;

                        int daycount = dayCount.Days;

                        if (daycount <= dayss)
                        {

                            bool needBuy = false;

                            for(int i = 0; i < selectdata.ShopPackageRelayDatas.Count; ++i)
                            {
                                if (selectdata.ShopPackageRelayDatas[i] == selectdata.FirstShopPackageData)
                                    continue;

                                ShopPackageRotationData otherData = selectdata.ShopPackageRelayDatas[i];
                                PlayerShopInfo otherShopInfo = GetPlayerShopInfo(otherData);

                                if (otherShopInfo == null)
                                {
                                    needBuy = true;
                                    break;
                                }
                                else if (otherShopInfo.AccumCount < 1)
                                {
                                    needBuy = true;
                                    break;
                                }
                            }


                            if (needBuy == true)
                            {
                                shopPackageRotateGroupData = selectdata;
                                break;
                            }
                            else
                            {
                                toWeekSoldOut = true;
                                break;
                            }
                        }
                    }
                }
            }

            if (toWeekSoldOut == true)
            {
                // 이번주 못산다.
                return null;
            }

            if (shopPackageRotateGroupData != null)
            {
                //이번주에 사긴 했는데, 아직 보석 남았다.
                return shopPackageRotateGroupData;
            }

            int selectGroup = loginCount / 7;

            shopPackageRotateGroupData = copyrotateDatas[selectGroup % copyrotateDatas.Count];

            return shopPackageRotateGroupData;
        }
        //------------------------------------------------------------------------------------
        public void ShowToDayBuyRotateGroupDialog()
        {
            if (ShowedRotateGroupData == true && TodayRotateGroupData != null)
            {
                _refreshShopRotateMsg.shopPackageRotateGroupData = TodayRotateGroupData;
                Message.Send(_refreshShopRotateMsg);

                UI.IDialog.RequestDialogEnter<UI.ShopPackageRotatePopupDialog>();
            }
        }
        //------------------------------------------------------------------------------------
        private bool IsActiveRelayData(ShopPackageRelayData shopPackageRelayData)
        {
            return CheckProductContitionValue(shopPackageRelayData.ProductActivateConditionType, shopPackageRelayData.ProductActivateConditionParam);
        }
        //------------------------------------------------------------------------------------
        public string GetPriceText(ShopDataBase shopDataBase)
        {
            if (shopDataBase == null)
                return "-";

            if (shopDataBase.PID == "-1")
                return LocalStringManager.Instance.GetLocalString("common/ui/free");

            if (IsAD(shopDataBase) == true)
            {
                return LocalStringManager.Instance.GetLocalString("common/ui/adView");
            }

            if (UnityPlugins.iap.IsInitialized() == false)
            {
                return "-";
            }

            UnityIAP.Item item = UnityPlugins.iap.GetItem(shopDataBase.PID);

            if (item != null)
            {
                return item.localizedPriceString.Replace(SceneManager.Instance.FranceEmptyStr, "");
            }

            return string.Format("{0:n0}", shopDataBase.PriceKR);
        }
        //------------------------------------------------------------------------------------
        public bool IsAD(ShopDataBase shopDataBase)
        {
            return ShopOperator.IsAD(shopDataBase);
        }
        //------------------------------------------------------------------------------------
        public bool IsReadyFixedTermAD()
        {
            for (int i = 0; i < ShopContainer.m_adProductDatas.Count; ++i)
            {
                if (ShopContainer.m_adProductDatas[i] is ShopPackageData)
                {
                    if (IsSoldOut(ShopContainer.m_adProductDatas[i]) == false)
                        return true;
                }
            }
            return false;
        }
        //------------------------------------------------------------------------------------
        public string GetRefreshMessageID(ShopDataBase shopDataBase)
        {
            if (shopDataBase == null)
                return string.Empty;

            return string.Format(_refreshID, shopDataBase.Index);
        }
        //------------------------------------------------------------------------------------
        public string GetRefreshMessageID(int index)
        {
            return string.Format(_refreshID, index);
        }
        //------------------------------------------------------------------------------------
        public string GetRefreshRelayGroupIDMessageID(ShopPackageRelayGroupData shopPackageRelayGroupData)
        {
            if (shopPackageRelayGroupData == null)
                return string.Empty;

            return string.Format(_refreshRelayGroupID, shopPackageRelayGroupData.RelayGroupIndex);
        }
        //------------------------------------------------------------------------------------
        public bool IsPossibleBuy_PackageRelayData(ShopPackageRelayData shopPackageRelayData)
        {
            bool readyBuy = (CheckProductContitionValue(shopPackageRelayData.ProductPurchaseConditionType1, shopPackageRelayData.ProductPurchaseConditionParam1)
                && CheckProductContitionValue(shopPackageRelayData.ProductPurchaseConditionType2, shopPackageRelayData.ProductPurchaseConditionParam2));

            return readyBuy;
        }
        //------------------------------------------------------------------------------------
        public bool IsSoldOut(ShopDataBase shopDataBase)
        {
            if (shopDataBase == null)
                return true;

            if (shopDataBase.IntervalParam <= 0)
                return false;

            return GetBuyCount(shopDataBase) >= shopDataBase.IntervalParam;
        }
        //------------------------------------------------------------------------------------
        public bool IsSoldOut(PlayerShopInfo playerShopInfo)
        {
            if (playerShopInfo == null)
                return true;

            ShopDataBase shopDataBase = GetShopData(playerShopInfo.Id);

            return IsSoldOut(shopDataBase);
        }
        //------------------------------------------------------------------------------------
        public void SetShopIngameStore(ShopIngameStoreData shopIngameStoreData)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            if (shopIngameStoreData == null)
                return;

            RewardData cost = shopIngameStoreData.CostGoods;

            if (Managers.GoodsManager.Instance.GetGoodsAmount(cost.Index) < cost.Amount)
            {
                return;
            }

            int used_type = 0;
            double former_quan = 0;
            double used_quan = 0;
            double keep_quan = 0;

            used_type = cost.Index;
            former_quan = GoodsManager.Instance.GetGoodsAmount(cost.Index);
            used_quan = cost.Amount;

            Managers.GoodsManager.Instance.UseGoodsAmount(cost.Index, cost.Amount);

            keep_quan = GoodsManager.Instance.GetGoodsAmount(cost.Index);




            int reward_type = 0;
            double before_quan = 0;
            double reward_quan = 0;
            double after_quan = 0;


            reward_type = shopIngameStoreData.ReturnGoods.Index;
            before_quan = GoodsManager.Instance.GetGoodsAmount(shopIngameStoreData.ReturnGoods.Index);
            reward_quan = shopIngameStoreData.ReturnGoods.Amount;

            PlayerShopInfo playerShopInfo = GetPlayerShopInfo(shopIngameStoreData) ?? ShopOperator.CreatePlayerShopInfo(shopIngameStoreData);

            playerShopInfo.Count++;
            playerShopInfo.AccumCount++;

            playerShopInfo.InitTimeStemp = TimeManager.Instance.GetInitTime(shopIngameStoreData.IntervalType);

            Managers.GoodsManager.Instance.AddGoodsAmount(shopIngameStoreData.ReturnGoods.Index, shopIngameStoreData.ReturnGoods.Amount);

            after_quan = GoodsManager.Instance.GetGoodsAmount(shopIngameStoreData.ReturnGoods.Index);

            _setInGameRewardPopupMsg.RewardDatas.Clear();

            _setInGameRewardPopupMsg.RewardDatas.Add(shopIngameStoreData.ReturnGoods);

            Message.Send(_setInGameRewardPopupMsg);
            UI.IDialog.RequestDialogEnter<UI.InGameRewardPopupDialog>();

            TheBackEnd.TheBackEndManager.Instance.UpdatePlayerPointTable();

            Message.Send(GetRefreshMessageID(shopIngameStoreData));

            ThirdPartyLog.Instance.SendLog_log_shop_noniap(shopIngameStoreData.Index,
                            used_type, former_quan, used_quan, keep_quan,
                            reward_type, before_quan, reward_quan, after_quan);

            if (shopIngameStoreData.MenuType == V2Enum_ShopMenuType.Research)
            {
                ThirdPartyLog.Instance.SendLog_log_fossil_acquire(3,
      reward_type, before_quan, reward_quan, after_quan,
      used_type, former_quan, used_quan, keep_quan);
            }

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInGameShopInfoUpdate, null);
            
        }
        //------------------------------------------------------------------------------------
        public bool CanBonus(ShopDiamondChargeData shopDiamondChargeData)
        {
            if (shopDiamondChargeData.BonusIntervalType == V2Enum_IntervalType.None)
                return false;

            PlayerShopInfo playerShopInfo = GetPlayerShopInfo(shopDiamondChargeData);

            if (playerShopInfo == null)
                return true;

            return playerShopInfo.InitTimeStemp < TimeManager.Instance.Current_TimeStamp;
        }
        //------------------------------------------------------------------------------------
        public void Buy(ShopDataBase shopDataBase)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            if (UnityPlugins.iap.IsInitialized() == false)
            {
                Contents.GlobalContent.ShowGlobalNotice("Please Wait");
                return;
            }

            if (shopDataBase is ShopPackageRelayData)
            {
                ShopPackageRelayData shopPackageRelayData = shopDataBase as ShopPackageRelayData;

                if (IsPossibleBuy_PackageRelayData(shopPackageRelayData) == false)
                    return;
            }

            ProcessBilling(shopDataBase, IAPComplete);
        }
        //------------------------------------------------------------------------------------
        private Coroutine ProcessingBillingCoroutine = null;
        //------------------------------------------------------------------------------------
        public void ProcessBilling(ShopDataBase shopDataBase, Action<ShopDataBase> callBack)
        {
            if (ProcessingBillingCoroutine != null)
                return;

            ProcessingBillingCoroutine = StartCoroutine(ProcessingBilling(shopDataBase, callBack));
        }
        //------------------------------------------------------------------------------------
        public IEnumerator ProcessingBilling(ShopDataBase shopDataBase, Action<ShopDataBase> callBack)
        {
            Contents.GlobalContent.VisibleBufferingUI(true);

            Debug.Log("ProcessingBilling함수시작");

            if (ShopContainer.m_freeProductData.Contains(shopDataBase))
            {
                // 임시로 바로 콜백 바로 리턴. PID 다 등록되면 원래 아래로직으로 해야함
                callBack?.Invoke(shopDataBase);

                Contents.GlobalContent.VisibleBufferingUI(false);
                ProcessingBillingCoroutine = null;
                yield break;
                // 임시로 바로 콜백 바로 리턴. PID 다 등록되면 원래 아래로직으로 해야함
            }
            else if (IsAD(shopDataBase) == true)
            {
                UnityPlugins.appLovin.ShowRewardedAd(() =>
                {
                    callBack?.Invoke(shopDataBase);
                });

                Contents.GlobalContent.VisibleBufferingUI(false);
                ProcessingBillingCoroutine = null;

                ThirdPartyLog.Instance.SendLog_AD_ViewEvent("shop", shopDataBase.Index, GameBerry.Define.IsAdFree == true ? 1 : 2);
                yield break;
            }


#if UNITY_EDITOR
            callBack?.Invoke(shopDataBase);
#else
            bool resultReward = false;

            UnityPlugins.iap.Purchase(shopDataBase.PID,
                Product =>
                {
                    BackendReturnObject callback = null;

                    Debug.Log("구매성공");

                    decimal iapPrice = Product.metadata.localizedPrice;
                    string iapCurrency = Product.metadata.isoCurrencyCode;
                    string orderid = UnityPlugins.iap.GetOrderId(Product);

                    if (Application.platform == RuntimePlatform.Android)
                        callback = Backend.Receipt.IsValidateGooglePurchase(Product.receipt, shopDataBase == null ? "receiptDescription" : shopDataBase.Description, iapPrice, iapCurrency);
                    else if (Application.platform == RuntimePlatform.IPhonePlayer)
                        callback = Backend.Receipt.IsValidateApplePurchase(Product.receipt, shopDataBase == null ? "receiptDescription" : shopDataBase.Description, iapPrice, iapCurrency);

                    ObscuredPrefs.Set("receipt", string.Empty);
                    ObscuredPrefs.Set("pid", string.Empty);
                    ObscuredPrefs.Set("orderid", string.Empty);
                    ObscuredPrefs.Save();

                    Debug.Log("영수증 검증완료");

                    Debug.Log(Product.receipt + "\n" + Product.definition.id + "\n" + shopDataBase.PID + "\n" + orderid);

                    Debug.Log("영수증 검증완료");

                    if (callback.IsSuccess())
                    {

                        Debug.Log("영수증 검증성공");
                        callBack?.Invoke(shopDataBase);

                        if (SceneManager.Instance.BuildElement == BuildEnvironmentEnum.Product)
                        {
                            UnityIAP.Item item = UnityPlugins.iap.GetItem(shopDataBase.PID);
                            ThirdPartyLog.Instance.SendLog_IAP(item.localizedPrice.ToString(), item.isoCurrencyCode, shopDataBase.Index.ToString(), Product.receipt);

                            ShopContainer.TotalBuyPrice += shopDataBase.PriceKR;
                            ThirdPartyLog.Instance.SendLog_Shop_IapEvent(orderid, shopDataBase.Index, shopDataBase.PriceKR);
                        }
                    }
                    else
                    {
                        Debug.Log("영수증 실패");
                        TheBackEnd.TheBackEndManager.Instance.BackEndErrorCode(callback);
                    }

                    resultReward = true;
                },
                (Product, failStr) =>
                {
                    Contents.GlobalContent.ShowPopup_Ok(
    LocalStringManager.Instance.GetLocalString("common/ui/purchaseFail"),
    failStr);

                    resultReward = true;
                }
                );

            while (resultReward == false)
                yield return null;
#endif

            ProcessingBillingCoroutine = null;

            Debug.Log("ProcessingBilling함수 끝");

            Contents.GlobalContent.VisibleBufferingUI(false);
        }
        //------------------------------------------------------------------------------------
        private void IAPComplete(ShopDataBase shopDataBase)
        {
            PlayerShopInfo playerShopInfo = GetPlayerShopInfo(shopDataBase) ?? ShopOperator.CreatePlayerShopInfo(shopDataBase);

            playerShopInfo.Count++;
            playerShopInfo.AccumCount++;

            if (shopDataBase is ShopPackageSpecialData)
            {
                playerShopInfo.InitTimeStemp = 0;

                _refreshShopSpecialMsg.shopPackageSpecialData = shopDataBase as ShopPackageSpecialData;
                Message.Send(_refreshShopSpecialMsg);
            }
            else if (shopDataBase is ShopPackageRotationData)
            {
                playerShopInfo.InitTimeStemp = TimeManager.Instance.Current_TimeStamp;

                TodayRotateGroupData = GetToDayBuyRotateGroupData();

                _refreshShopRotateMsg.shopPackageRotateGroupData = TodayRotateGroupData;
                Message.Send(_refreshShopRotateMsg);
            }
            else if (shopDataBase is ShopDiamondChargeData)
            { 

            }
            else
            {
                if (playerShopInfo.InitTimeStemp < TimeManager.Instance.Current_TimeStamp)
                    playerShopInfo.InitTimeStemp = TimeManager.Instance.GetInitTime(shopDataBase.IntervalType);
            }

            if (shopDataBase is ShopDiamondChargeData)
            {
                ShopDiamondChargeData shopDiamondChargeData = shopDataBase as ShopDiamondChargeData;

                if (CanBonus(shopDiamondChargeData) == true)
                    ShopPostManager.Instance.AddShopPost(shopDataBase, shopDiamondChargeData.BonusGoods);
                else
                    ShopPostManager.Instance.AddShopPost(shopDataBase);

                playerShopInfo.InitTimeStemp = TimeManager.Instance.GetInitTime(shopDiamondChargeData.BonusIntervalType);
            }
            else
                ShopPostManager.Instance.AddShopPost(shopDataBase);

            Message.Send(GetRefreshMessageID(shopDataBase));

            Contents.GlobalContent.ShowPopup_Ok(
                LocalStringManager.Instance.GetLocalString("common/popUp/title"),
                string.Format("{0}\n{1}", LocalStringManager.Instance.GetLocalString(shopDataBase.TitleLocalStringKey),
                LocalStringManager.Instance.GetLocalString("common/ui/purchaseSuccess")));

            //Contents.GlobalContent.ShowGlobalNotice(LocalStringManager.Instance.GetLocalString("common/ui/purchaseSuccess"));

            if (shopDataBase is ShopPackageRelayData)
            {
                ShopPackageRelayData shopPackageRelayData = shopDataBase as ShopPackageRelayData;

                ShopPackageRelayGroupData shopPackageRelayGroupData = GetPackageRelayGroupData(shopPackageRelayData.RelayGroupIndex);

                SetRelayGroupData(shopPackageRelayGroupData);

                _refreshRelayGroupMsg.RefreshData = shopPackageRelayGroupData;
                Message.Send(_refreshRelayGroupMsg);
            }

            if (shopDataBase is ShopPackageEventData)
            {
                _refreshShopEventMsg.shopPackageEventData = shopDataBase as ShopPackageEventData;
                Message.Send(_refreshShopEventMsg);
            }

            //RefreshProductContitionType(V2Enum_OpenConditionType.PrevProductPurchase);

            //if (shopDataBase.PID == "-1")
            //    GuideQuestManager.Instance.CheckEventType(V2Enum_EventType.FreePurchase);


            TheBackEnd.TheBackEndManager.Instance.UpdatePlayerShopInfoTable(null);
        }
        //------------------------------------------------------------------------------------
        
    }
}