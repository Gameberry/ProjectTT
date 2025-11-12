using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd.Socketio;
using LitJson;
using BackEnd;

namespace GameBerry.Managers
{
    public class ShopPostManager : MonoSingleton<ShopPostManager>
    {
        private Event.SetInGameRewardPopupMsg m_setInGameRewardPopupMsg = new Event.SetInGameRewardPopupMsg();

        private Event.RefreshShopPostListMsg m_refreshShopPostListMsg = new Event.RefreshShopPostListMsg();

        //------------------------------------------------------------------------------------
        public void InitShopPostContent()
        {
            RefreshShopPost();
        }
        //------------------------------------------------------------------------------------
        public Dictionary<string, PostInfo> GetAllPostInfos()
        {
            return ShopPostContainer.m_myShopPostInfo;
        }
        //------------------------------------------------------------------------------------
        private void RefreshShopPost()
        {
            for (int i = 0; i < ShopPostContainer.m_shopPostInfos.Count; ++i)
            {
                ShopPostInfo shopPostInfo = ShopPostContainer.m_shopPostInfos[i];
                if (shopPostInfo == null)
                    continue;

                if (shopPostInfo.IsRecv == false)
                {
                    if (ShopPostContainer.m_myShopPostInfo.ContainsKey(shopPostInfo.InData) == false)
                    {
                        PostInfo postInfo = NewPostInfo(shopPostInfo);
                        if (postInfo == null)
                            continue;

                        ShopPostContainer.m_myShopPostInfo.Add(postInfo.InData, postInfo);

                        Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.PostShop);
                    }
                }
                else
                {
                    if (ShopPostContainer.m_myShopPostInfo.ContainsKey(shopPostInfo.InData) == true)
                        ShopPostContainer.m_myShopPostInfo.Remove(shopPostInfo.InData);
                }
            }

            Message.Send(m_refreshShopPostListMsg);
        }
        //------------------------------------------------------------------------------------
        private PostInfo NewPostInfo(ShopPostInfo shopPostInfo)
        {
            if (ShopPostContainer.m_myShopPostInfo.ContainsKey(shopPostInfo.InData) == true)
                return ShopPostContainer.m_myShopPostInfo[shopPostInfo.InData];

            PostInfo postInfo = PostOperator.GetPostInfoObj();
            postInfo.InData = shopPostInfo.InData;
            postInfo.IsShop = true;
            postInfo.SentDate_TimeStamp = shopPostInfo.Make_TimeStamp;
            postInfo.SentDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            postInfo.SentDate = postInfo.SentDate.AddSeconds(shopPostInfo.Make_TimeStamp);
            postInfo.SentDate = postInfo.SentDate.ToLocalTime();
            postInfo.ExpirationDate = DateTime.MaxValue;
            if (shopPostInfo.rewardDatas.Count > 0 && postInfo.RewardData == null)
                postInfo.RewardData = new List<RewardData>();

            for (int i = 0; i < shopPostInfo.rewardDatas.Count; ++i)
            {
                RewardData rewardData = RewardManager.Instance.GetRewardData();
                rewardData.V2Enum_Goods = shopPostInfo.rewardDatas[i].V2Enum_Goods;
                rewardData.Index = shopPostInfo.rewardDatas[i].Index;
                rewardData.Amount = shopPostInfo.rewardDatas[i].Amount;

                postInfo.RewardData.Add(rewardData);
            }

            ShopDataBase shopDataBase = ShopManager.Instance.GetShopData(shopPostInfo.ShopIndex);
            if (shopDataBase != null)
            {
                postInfo.Title = shopDataBase.MailTitleLocalStringKey;
                postInfo.Content = shopDataBase.MailDescLocalStringKey;
            }
            else
            {
                VipPackageData vipPackageData = VipPackageManager.Instance.GetVipPackageData(shopPostInfo.ShopIndex);
                if (vipPackageData != null)
                {
                    postInfo.Title = vipPackageData.MailTitleLocalStringKey;
                    postInfo.Content = vipPackageData.MailDescLocalStringKey;
                }
            }

            return postInfo;
        }
        //------------------------------------------------------------------------------------
        public PostInfo GetPostInfo(string indata)
        {
            if (ShopPostContainer.m_myShopPostInfo.ContainsKey(indata) == true)
            {
                return ShopPostContainer.m_myShopPostInfo[indata];
            }

            return null;
        }
        //------------------------------------------------------------------------------------
        public ShopPostInfo GetShopPostInfo(string indata)
        {
            return ShopPostContainer.m_shopPostInfos.Find(x => x.InData.GetDecrypted() == indata);
        }
        //------------------------------------------------------------------------------------
        public ShopDataBase GetShopDataBase(string indata)
        {
            ShopPostInfo shopPostInfo = GetShopPostInfo(indata);
            if (shopPostInfo == null)
                return null;

            return ShopManager.Instance.GetShopData(shopPostInfo.ShopIndex.GetDecrypted());
        }
        //------------------------------------------------------------------------------------
        public void AddShopPost(ShopDataBase shopDataBase)
        {
            double currentTime = TimeManager.Instance.Current_TimeStamp;

            ShopPostInfo shopPostInfo = new ShopPostInfo();
            shopPostInfo.InData = string.Format("{0}_{1}", (long)(TimeManager.Instance.Current_TimeStamp), shopDataBase.Index);
            shopPostInfo.ShopIndex = shopDataBase.Index;
            shopPostInfo.Make_TimeStamp = (long)currentTime;
            shopPostInfo.IsRecv = false;

            for (int i = 0; i < shopDataBase.ShopRewardData.Count; ++i)
            {
                RewardData rewardData = new RewardData();
                rewardData.IsPoolData = false;
                rewardData.V2Enum_Goods = shopDataBase.ShopRewardData[i].V2Enum_Goods;
                rewardData.Index = shopDataBase.ShopRewardData[i].Index;
                rewardData.Amount = shopDataBase.ShopRewardData[i].Amount;

                shopPostInfo.rewardDatas.Add(rewardData);
            }

            ShopPostContainer.m_shopPostInfos.Add(shopPostInfo);

            RefreshShopPost();
        }
        //------------------------------------------------------------------------------------
        public void AddShopPost(ShopDataBase shopDataBase, List<RewardData> addReward)
        {
            double currentTime = TimeManager.Instance.Current_TimeStamp;



            ShopPostInfo shopPostInfo = new ShopPostInfo();
            shopPostInfo.InData = string.Format("{0}_{1}", (long)(TimeManager.Instance.Current_TimeStamp), shopDataBase.Index);
            shopPostInfo.ShopIndex = shopDataBase.Index;
            shopPostInfo.Make_TimeStamp = (long)currentTime;
            shopPostInfo.IsRecv = false;

            for (int i = 0; i < shopDataBase.ShopRewardData.Count; ++i)
            {
                RewardData rewardData = new RewardData();
                rewardData.IsPoolData = false;
                rewardData.V2Enum_Goods = shopDataBase.ShopRewardData[i].V2Enum_Goods;
                rewardData.Index = shopDataBase.ShopRewardData[i].Index;
                rewardData.Amount = shopDataBase.ShopRewardData[i].Amount;

                shopPostInfo.rewardDatas.Add(rewardData);
            }

            for (int i = 0; i < addReward.Count; ++i)
            {
                RewardData rewardData = new RewardData();
                rewardData.IsPoolData = false;
                rewardData.V2Enum_Goods = addReward[i].V2Enum_Goods;
                rewardData.Index = addReward[i].Index;
                rewardData.Amount = addReward[i].Amount;

                shopPostInfo.rewardDatas.Add(rewardData);
            }

            ShopPostContainer.m_shopPostInfos.Add(shopPostInfo);

            RefreshShopPost();
        }
        //------------------------------------------------------------------------------------
        public void AddVipPost(VipPackageData vipPackageData)
        {
            if (vipPackageData.ReceiveDiaEveryday <= 0)
                return;

            double currentTime = TimeManager.Instance.Current_TimeStamp;

            ShopPostInfo shopPostInfo = new ShopPostInfo();
            shopPostInfo.InData = string.Format("{0}_{1}", (long)(TimeManager.Instance.Current_TimeStamp), vipPackageData.Index);
            shopPostInfo.ShopIndex = vipPackageData.Index;
            shopPostInfo.Make_TimeStamp = (long)currentTime;
            shopPostInfo.IsRecv = false;

            RewardData rewardData = new RewardData();
            rewardData.IsPoolData = false;
            rewardData.V2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(vipPackageData.ReceiveDiaIndex);
            rewardData.Index = vipPackageData.ReceiveDiaIndex;
            rewardData.Amount = vipPackageData.ReceiveDiaEveryday;

            shopPostInfo.rewardDatas.Add(rewardData);

            ShopPostContainer.m_shopPostInfos.Add(shopPostInfo);

            RefreshShopPost();
        }
        //------------------------------------------------------------------------------------
        public void RecvPostReward(string indata)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            m_setInGameRewardPopupMsg.RewardDatas.Clear();

            if (ShopPostContainer.m_myShopPostInfo.ContainsKey(indata) == false)
                return;

            ShopPostInfo shopPostInfo = ShopPostContainer.m_shopPostInfos.Find(x => x.InData == indata);
            if (shopPostInfo == null)
                return;

            if (shopPostInfo.IsRecv == true)
                return;

            shopPostInfo.IsRecv = true;

            string title = string.Empty;

            PostInfo postInfo = GetPostInfo(indata);
            if (postInfo != null)
                title = postInfo.Title;

            if (ShopPostContainer.m_myShopPostInfo.ContainsKey(shopPostInfo.InData) == true)
            {
                PostOperator.PoolPostInfoObj(ShopPostContainer.m_myShopPostInfo[shopPostInfo.InData]);
                ShopPostContainer.m_myShopPostInfo.Remove(shopPostInfo.InData);
            }

            if (PassManager.Instance.GetPassData(shopPostInfo.ShopIndex) != null)
                PassManager.Instance.SetEnable(shopPostInfo.ShopIndex);

            //if (EventPassManager.Instance.GetEventPassData(shopPostInfo.ShopIndex) != null)
            //    EventPassManager.Instance.SetEnable(shopPostInfo.ShopIndex);

            //if (SevenDayManager.Instance.GetSevenDayPassData(shopPostInfo.ShopIndex) != null)
            //    SevenDayManager.Instance.SetEnable(shopPostInfo.ShopIndex);

            //if (NewYearManager.Instance.GetNewYearPassData(shopPostInfo.ShopIndex) != null)
            //    NewYearManager.Instance.SetEnable(shopPostInfo.ShopIndex);

            //ShopDataBase shopDataBase = Managers.ShopManager.Instance.GetShopData(shopPostInfo.ShopIndex);
            //if (shopDataBase != null)
            //{
            //    if (shopDataBase.MenuType == V2Enum_ShopMenuType.EventDigPass)
            //        EventDigManager.Instance.SetEnablePaidPass(shopDataBase.Index);
            //    else if (shopDataBase.MenuType == V2Enum_ShopMenuType.EventMathRpgPass)
            //        EventMathRpgManager.Instance.SetEnablePaidPass(shopDataBase.Index);

            //    if (Managers.ShopManager.Instance.IsAD(shopDataBase) == false && shopDataBase.PID != "-1")
            //    {
            //        if (ShopContainer.m_freeProductData != null && ShopContainer.m_freeProductData != shopDataBase)
            //            GuildManager.Instance.AddGuildPurchaseCoin();
            //    }
            //}


            List<int> reward_type = new List<int>();
            List<double> before_quan = new List<double>();
            List<double> reward_quan = new List<double>();
            List<double> after_quan = new List<double>();

            for (int rewardidx = 0; rewardidx < shopPostInfo.rewardDatas.Count; ++rewardidx)
            {
                RewardData rewardData = shopPostInfo.rewardDatas[rewardidx];

                reward_type.Add(rewardData.Index);
                before_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));
                reward_quan.Add(rewardData.Amount);

                GoodsManager.Instance.AddGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index, rewardData.Amount);

                after_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));

                m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);
            }

            if (m_setInGameRewardPopupMsg.RewardDatas.Count > 0)
            {
                Message.Send(m_setInGameRewardPopupMsg);
                UI.UIManager.DialogEnter<UI.InGameRewardPopupDialog>();
            }

            //GuideQuestManager.Instance.AddEventCount(V2Enum_EventType.MailGet, 1);

            TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerShopInfoTable);
            TheBackEnd.TheBackEndManager.Instance.SendUpdateWaitData(true);

            ThirdPartyLog.Instance.SendLog_MailEvent(2, title, reward_type, before_quan, reward_quan, after_quan);

            RefreshShopPost();
        }
        //------------------------------------------------------------------------------------
        public void ReceiveAllPostItem()
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            m_setInGameRewardPopupMsg.RewardDatas.Clear();


            List<int> reward_type = new List<int>();
            List<double> before_quan = new List<double>();
            List<double> reward_quan = new List<double>();
            List<double> after_quan = new List<double>();

            string title = string.Empty;

            for (int i = 0; i < ShopPostContainer.m_shopPostInfos.Count; ++i)
            {
                ShopPostInfo shopPostInfo = ShopPostContainer.m_shopPostInfos[i];
                if (shopPostInfo.IsRecv == true)
                    continue;

                shopPostInfo.IsRecv = true;


                PostInfo postInfo = GetPostInfo(shopPostInfo.InData);
                if (postInfo != null)
                {
                    if (string.IsNullOrEmpty(title) == true)
                        title = postInfo.Title;
                    else
                        title = string.Format("{0},{1}", title, postInfo.Title);
                }

                if (ShopPostContainer.m_myShopPostInfo.ContainsKey(shopPostInfo.InData) == true)
                {
                    PostOperator.PoolPostInfoObj(ShopPostContainer.m_myShopPostInfo[shopPostInfo.InData]);
                    ShopPostContainer.m_myShopPostInfo.Remove(shopPostInfo.InData);
                }

                if (PassManager.Instance.GetPassData(shopPostInfo.ShopIndex) != null)
                    PassManager.Instance.SetEnable(shopPostInfo.ShopIndex);

                //if (EventPassManager.Instance.GetEventPassData(shopPostInfo.ShopIndex) != null)
                //    EventPassManager.Instance.SetEnable(shopPostInfo.ShopIndex);

                //if (SevenDayManager.Instance.GetSevenDayPassData(shopPostInfo.ShopIndex) != null)
                //    SevenDayManager.Instance.SetEnable(shopPostInfo.ShopIndex);

                //if (NewYearManager.Instance.GetNewYearPassData(shopPostInfo.ShopIndex) != null)
                //    NewYearManager.Instance.SetEnable(shopPostInfo.ShopIndex);

                //ShopDataBase shopDataBase = Managers.ShopManager.Instance.GetShopData(shopPostInfo.ShopIndex);
                //if (shopDataBase != null)
                //{
                //    if (shopDataBase.MenuType == V2Enum_ShopMenuType.EventDigPass)
                //        EventDigManager.Instance.SetEnablePaidPass(shopDataBase.Index);
                //    else if (shopDataBase.MenuType == V2Enum_ShopMenuType.EventMathRpgPass)
                //        EventMathRpgManager.Instance.SetEnablePaidPass(shopDataBase.Index);

                //    if (Managers.ShopManager.Instance.IsAD(shopDataBase) == false && shopDataBase.PID != "-1")
                //    {
                //        if (ShopContainer.m_freeProductData != null && ShopContainer.m_freeProductData != shopDataBase)
                //            GuildManager.Instance.AddGuildPurchaseCoin();
                //    }
                //}


                for (int rewardidx = 0; rewardidx < shopPostInfo.rewardDatas.Count; ++rewardidx)
                {
                    RewardData rewardData = shopPostInfo.rewardDatas[rewardidx];

                    reward_type.Add(rewardData.Index);
                    before_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));
                    reward_quan.Add(rewardData.Amount);


                    GoodsManager.Instance.AddGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index, rewardData.Amount);

                    after_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));

                    m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);
                }
            }

            if (m_setInGameRewardPopupMsg.RewardDatas.Count > 0)
            {
                Message.Send(m_setInGameRewardPopupMsg);
                UI.UIManager.DialogEnter<UI.InGameRewardPopupDialog>();
            }

            //GuideQuestManager.Instance.AddEventCount(V2Enum_EventType.MailGet, 1);

            TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerShopInfoTable);
            TheBackEnd.TheBackEndManager.Instance.SendUpdateWaitData(true);

            ThirdPartyLog.Instance.SendLog_MailEvent(2, title, reward_type, before_quan, reward_quan, after_quan);

            RefreshShopPost();
        }
        //------------------------------------------------------------------------------------
    }
}