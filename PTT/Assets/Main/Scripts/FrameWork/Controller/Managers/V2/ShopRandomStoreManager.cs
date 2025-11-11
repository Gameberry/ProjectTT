using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BackEnd;
using CodeStage.AntiCheat.Storage;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class ShopRandomStoreManager : MonoSingleton<ShopRandomStoreManager>
    {
        private Event.RefreshShopRandomStoreMsg _refreshShopRandomStoreMsg = new Event.RefreshShopRandomStoreMsg();

        private Event.SetInGameRewardPopupMsg _setInGameRewardPopupMsg = new Event.SetInGameRewardPopupMsg();

        private WeightedRandomPicker<ShopRandomStoreData> _shopRandomStorePicker = new WeightedRandomPicker<ShopRandomStoreData>();

        private List<string> m_changeInfoUpdate = new List<string>();

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            ShopRandomStoreOperator.Init();

            m_changeInfoUpdate.Add(Define.PlayerShopRandomStoreInfoTable);
            m_changeInfoUpdate.Add(Define.PlayerPointTable);
        }
        //------------------------------------------------------------------------------------
        public void InitShopRandomStoreContent()
        {
            TimeManager.Instance.OnInitDailyContent += OnInitDayContent;

            if (ShopRandomStoreContainer.DailyInitTimeStemp < TimeManager.Instance.Current_TimeStamp)
            {
                OnInitDayContent(TimeManager.Instance.DailyInit_TimeStamp);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnInitDayContent(double nextinittimestamp)
        {
            ShopRandomStoreContainer.DailyInitTimeStemp = nextinittimestamp;

            SetDisPlayList();

            ShopRandomStoreContainer.StoreResetAdViewCount = 0;
            ShopRandomStoreContainer.StoreResetDiaCount = 0;

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate, null);

            Message.Send(_refreshShopRandomStoreMsg);

            foreach (var pair in ShopRandomStoreContainer.ShopFreeGoodsInfos)
            {
                ShopFreeGoodsInfo shopFreeGoodsInfo = pair.Value;
                shopFreeGoodsInfo.DiaFreeRecvCount = 0;
                shopFreeGoodsInfo.DiaAdViewRecvCount = 0;

                ShopFreeGoodsData shopFreeGoodsData = GetShopFreeGoodsData(shopFreeGoodsInfo.Index);
                //if (shopFreeGoodsData != null)
                //{ 
                //    shopFreeGoodsInfo.InitTime = Managers.TimeManager.Instance.DailyInit_TimeStamp + Managers.TimeManager.Instance.GetInitAddTime(shopFreeGoodsData.IntervalType, shopFreeGoodsData.IntervalParam);
                //}

                shopFreeGoodsInfo.InitTime = Managers.TimeManager.Instance.DailyInit_TimeStamp;

                Message.Send(ShopManager.Instance.GetRefreshMessageID(shopFreeGoodsData.Index));
            }

            Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.ShopRandomStore);
        }
        //------------------------------------------------------------------------------------
        public List<ShopRandomStoreData> GetShopRandomStoreDatas()
        {
            return ShopRandomStoreOperator.GetShopRandomStoreDatas();
        }
        //------------------------------------------------------------------------------------
        public ShopRandomStoreData GetShopRandomStoreData(ObscuredInt index)
        {
            return ShopRandomStoreOperator.GetShopRandomStoreDatas().Find(x => x.Index == index);
        }
        //------------------------------------------------------------------------------------
        public List<ShopFreeGoodsData> GetShopFreeGoodsDatas()
        {
            return ShopRandomStoreOperator.GetShopFreeGoodsDatas();
        }
        //------------------------------------------------------------------------------------
        public ShopFreeGoodsData GetShopFreeGoodsData(ObscuredInt index)
        {
            List<ShopFreeGoodsData> datas = GetShopFreeGoodsDatas();

            if (datas == null)
                return null;

            return datas.Find(x => x.Index == index);
        }
        //------------------------------------------------------------------------------------
        public ShopFreeGoodsData GetShopFreeGoodsData(V2Enum_ShopMenuType v2Enum_ShopMenuType)
        {
            List<ShopFreeGoodsData> datas = GetShopFreeGoodsDatas();

            if (datas == null)
                return null;

            return datas.Find(x => x.MenuType == v2Enum_ShopMenuType);
        }
        //------------------------------------------------------------------------------------
        private void SetDisPlayList()
        {
            bool isinit = ShopRandomStoreContainer.StoreDisPlayList.Count == 0;

            ShopRandomStoreContainer.BuyRandomStoreInfo.Clear();
            ShopRandomStoreContainer.StoreDisPlayList.Clear();

            List<ShopRandomStoreData> shopRandomStoreDatas = GetShopRandomStoreDatas();

            _shopRandomStorePicker.Clear();

            int actioncount = 0;

            for (int i = 0; i < shopRandomStoreDatas.Count; ++i)
            {
                ShopRandomStoreData shopRandomStoreData = shopRandomStoreDatas[i];

                if (isinit == true)
                {
                    if (shopRandomStoreData.Index == Define.BaseSellingItemOnce)
                    { 
                        ShopRandomStoreContainer.StoreDisPlayList.Add(Define.BaseSellingItemOnce);
                        actioncount += 1;
                        isinit = false;
                        continue;
                    }
                }

                _shopRandomStorePicker.Add(shopRandomStoreData, shopRandomStoreData.Prob);
            }

            while (actioncount < Define.RandomShopSlot && _shopRandomStorePicker.Count > 0)
            {
                ShopRandomStoreData shopRandomStoreData = _shopRandomStorePicker.Pick();
                ShopRandomStoreContainer.StoreDisPlayList.Add(shopRandomStoreData.Index);
                _shopRandomStorePicker.Remove(shopRandomStoreData, shopRandomStoreData.Prob);

                actioncount++;
            }
        }
        //------------------------------------------------------------------------------------
        public List<ObscuredInt> GetDisPlayIndexs()
        {
            return ShopRandomStoreContainer.StoreDisPlayList;
        }
        //------------------------------------------------------------------------------------
        public bool IsSoldOut(ShopRandomStoreData shopRandomStoreData)
        { 
            return ShopRandomStoreContainer.BuyRandomStoreInfo.Contains(shopRandomStoreData.Index);
        }
        //------------------------------------------------------------------------------------
        public int RemainResetStoreCount_AdView()
        {
            return Define.RandomShopAdRefreshCount - ShopRandomStoreContainer.StoreResetAdViewCount;
        }
        //------------------------------------------------------------------------------------
        public void ResetStore_AdView()
        {
            if (RemainResetStoreCount_AdView() <= 0)
                return;

            UnityPlugins.appLovin.ShowRewardedAd(() =>
            {
                ShopRandomStoreContainer.StoreResetAdViewCount++;

                SetDisPlayList();

                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate, null);

                Message.Send(_refreshShopRandomStoreMsg);

                ThirdPartyLog.Instance.SendLog_AD_ViewEvent("randomstorereset", 0, GameBerry.Define.IsAdFree == true ? 1 : 2);
            });
        }
        //------------------------------------------------------------------------------------
        public int RemainResetStoreCount_Dia()
        {
            return Define.RandomShopRefreshCount - ShopRandomStoreContainer.StoreResetDiaCount;
        }
        //------------------------------------------------------------------------------------
        public void ResetStore_Dia()
        {
            if (RemainResetStoreCount_Dia() <= 0)
                return;

            double dia = Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point, V2Enum_Point.Dia.Enum32ToInt());

            if (dia < Define.RandomShopRefreshValue)
                return;

            ShopRandomStoreContainer.StoreResetDiaCount++;

            SetDisPlayList();

            Managers.GoodsManager.Instance.UseGoodsAmount(V2Enum_Goods.Point, V2Enum_Point.Dia.Enum32ToInt(), Define.RandomShopRefreshValue);

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate, null);

            Message.Send(_refreshShopRandomStoreMsg);
        }
        //------------------------------------------------------------------------------------
        public bool CanRecvFreeDia(ShopFreeGoodsData shopFreeGoodsData)
        {
            if (ShopRandomStoreContainer.ShopFreeGoodsInfos.ContainsKey(shopFreeGoodsData.Index) == false)
                return true;

            ShopFreeGoodsInfo shopFreeGoodsInfo = ShopRandomStoreContainer.ShopFreeGoodsInfos[shopFreeGoodsData.Index];

            return shopFreeGoodsInfo.DiaFreeRecvCount < 1;
        }
        //------------------------------------------------------------------------------------
        public int RemainFreeDiaCount_AdView(ShopFreeGoodsData shopFreeGoodsData)
        {
            if (ShopRandomStoreContainer.ShopFreeGoodsInfos.ContainsKey(shopFreeGoodsData.Index) == false)
                return Define.RandomShopFreeDiaAD;

            ShopFreeGoodsInfo shopFreeGoodsInfo = ShopRandomStoreContainer.ShopFreeGoodsInfos[shopFreeGoodsData.Index];


            return Define.RandomShopFreeDiaAD - shopFreeGoodsInfo.DiaAdViewRecvCount;
        }
        //------------------------------------------------------------------------------------
        public void RecvFreeDia(ShopFreeGoodsData shopFreeGoodsData)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            if (CanRecvFreeDia(shopFreeGoodsData) == true)
            {
                ShopFreeGoodsInfo shopFreeGoodsInfo = null;

                if (ShopRandomStoreContainer.ShopFreeGoodsInfos.ContainsKey(shopFreeGoodsData.Index) == true)
                {
                    shopFreeGoodsInfo = ShopRandomStoreContainer.ShopFreeGoodsInfos[shopFreeGoodsData.Index];
                }
                else
                {
                    shopFreeGoodsInfo = new ShopFreeGoodsInfo();
                    shopFreeGoodsInfo.Index = shopFreeGoodsData.Index;
                    shopFreeGoodsInfo.InitTime = Managers.TimeManager.Instance.DailyInit_TimeStamp;

                    ShopRandomStoreContainer.ShopFreeGoodsInfos.Add(shopFreeGoodsInfo.Index, shopFreeGoodsInfo);
                }

                shopFreeGoodsInfo.DiaFreeRecvCount += 1;

                _setInGameRewardPopupMsg.RewardDatas.Clear();

                int reward_type = shopFreeGoodsData.ReturnGoods.Index;
                double before_quan = GoodsManager.Instance.GetGoodsAmount(shopFreeGoodsData.ReturnGoods.Index);
                double reward_quan = shopFreeGoodsData.ReturnGoods.Amount;

                Managers.GoodsManager.Instance.AddGoodsAmount(shopFreeGoodsData.ReturnGoods.Index, shopFreeGoodsData.ReturnGoods.Amount);

                double after_quan = GoodsManager.Instance.GetGoodsAmount(shopFreeGoodsData.ReturnGoods.Index);

                _setInGameRewardPopupMsg.RewardDatas.Add(shopFreeGoodsData.ReturnGoods);

                Message.Send(_setInGameRewardPopupMsg);
                UI.IDialog.RequestDialogEnter<UI.InGameRewardPopupDialog>();

                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate, null);

                Message.Send(ShopManager.Instance.GetRefreshMessageID(shopFreeGoodsData.Index));


                ThirdPartyLog.Instance.SendLog_log_shop_noniap(shopFreeGoodsData.Index,
                    0, 0, 0, 0,
                    reward_type, before_quan, reward_quan, after_quan);

                if (shopFreeGoodsData.MenuType == V2Enum_ShopMenuType.Research)
                {
                    ThirdPartyLog.Instance.SendLog_log_fossil_acquire(4,
    reward_type, before_quan, reward_quan, after_quan,
    0, 0, 0, 0);
                }


                //Message.Send(_refreshShopRandomStoreMsg);
            }
            else if (RemainFreeDiaCount_AdView(shopFreeGoodsData) > 0)
            {

                UnityPlugins.appLovin.ShowRewardedAd(() =>
                {
                    if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                        return;

                    ShopFreeGoodsInfo shopFreeGoodsInfo = null;

                    if (ShopRandomStoreContainer.ShopFreeGoodsInfos.ContainsKey(shopFreeGoodsData.Index) == true)
                    {
                        shopFreeGoodsInfo = ShopRandomStoreContainer.ShopFreeGoodsInfos[shopFreeGoodsData.Index];
                    }
                    else
                    {
                        shopFreeGoodsInfo = new ShopFreeGoodsInfo();
                        shopFreeGoodsInfo.Index = shopFreeGoodsData.Index;
                        shopFreeGoodsInfo.InitTime = Managers.TimeManager.Instance.DailyInit_TimeStamp;

                        ShopRandomStoreContainer.ShopFreeGoodsInfos.Add(shopFreeGoodsInfo.Index, shopFreeGoodsInfo);
                    }

                    shopFreeGoodsInfo.DiaAdViewRecvCount += 1;

                    _setInGameRewardPopupMsg.RewardDatas.Clear();

                    int reward_type = shopFreeGoodsData.ReturnGoods.Index;
                    double before_quan = GoodsManager.Instance.GetGoodsAmount(shopFreeGoodsData.ReturnGoods.Index);
                    double reward_quan = shopFreeGoodsData.ReturnGoods.Amount;

                    Managers.GoodsManager.Instance.AddGoodsAmount(shopFreeGoodsData.ReturnGoods.Index, shopFreeGoodsData.ReturnGoods.Amount);

                    double after_quan = GoodsManager.Instance.GetGoodsAmount(shopFreeGoodsData.ReturnGoods.Index);

                    _setInGameRewardPopupMsg.RewardDatas.Add(shopFreeGoodsData.ReturnGoods);

                    Message.Send(_setInGameRewardPopupMsg);
                    UI.IDialog.RequestDialogEnter<UI.InGameRewardPopupDialog>();

                    TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate, null);

                    Message.Send(ShopManager.Instance.GetRefreshMessageID(shopFreeGoodsData.Index));
                    //Message.Send(_refreshShopRandomStoreMsg);

                    ThirdPartyLog.Instance.SendLog_AD_ViewEvent("randomstoredia", shopFreeGoodsData.Index, GameBerry.Define.IsAdFree == true ? 1 : 2);

                    if (shopFreeGoodsData.MenuType == V2Enum_ShopMenuType.Research)
                    {
                        ThirdPartyLog.Instance.SendLog_log_fossil_acquire(2,
        reward_type, before_quan, reward_quan, after_quan,
        0, 0, 0, 0);
                    }
                });
            }
        }
        //------------------------------------------------------------------------------------
        public bool RecvRandomStore(ShopRandomStoreData shopRandomStoreData)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return false;

            if (shopRandomStoreData == null)
                return false;

            if (IsSoldOut(shopRandomStoreData) == true)
                return false;

            RewardData cost = shopRandomStoreData.CostGoods;

            if (Managers.GoodsManager.Instance.GetGoodsAmount(cost.Index) < cost.Amount)
                return false;

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

            reward_type = shopRandomStoreData.ReturnGoods.Index;
            before_quan = GoodsManager.Instance.GetGoodsAmount(shopRandomStoreData.ReturnGoods.Index);
            reward_quan = shopRandomStoreData.ReturnGoods.Amount;


            Managers.GoodsManager.Instance.AddGoodsAmount(shopRandomStoreData.ReturnGoods.Index, shopRandomStoreData.ReturnGoods.Amount);

            after_quan = GoodsManager.Instance.GetGoodsAmount(shopRandomStoreData.ReturnGoods.Index);

            ShopRandomStoreContainer.BuyRandomStoreInfo.Add(shopRandomStoreData.Index);

            _setInGameRewardPopupMsg.RewardDatas.Clear();

            _setInGameRewardPopupMsg.RewardDatas.Add(shopRandomStoreData.ReturnGoods);

            Message.Send(_setInGameRewardPopupMsg);
            UI.IDialog.RequestDialogEnter<UI.InGameRewardPopupDialog>();

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate, null);

            ThirdPartyLog.Instance.SendLog_log_shop_noniap(shopRandomStoreData.Index,
                used_type, former_quan, used_quan, keep_quan,
                reward_type, before_quan, reward_quan, after_quan);

            Message.Send(_refreshShopRandomStoreMsg);

            return true;
        }
        //------------------------------------------------------------------------------------
    }
}