using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class DungeonEnterData
    {
        public ObscuredInt step; 
    }

    public class DungeonDataManager : MonoSingleton<DungeonDataManager>
    {
        private Event.RefreshDungeonAdInfoListMsg m_refreshDungeonAdInfoListMsg = new Event.RefreshDungeonAdInfoListMsg();
        private Event.SetInGameRewardPopupMsg m_setInGameRewardPopupMsg = new Event.SetInGameRewardPopupMsg();
        private Event.SetInGameRewardPopup_TitleMsg m_setInGameRewardPopup_TitleMsg = new Event.SetInGameRewardPopup_TitleMsg();

        private Dictionary<V2Enum_Dungeon, DungeonEnterData> m_selectDungeonEnterStep = new Dictionary<V2Enum_Dungeon, DungeonEnterData>();

        private List<string> m_changeInfoAdTicketUpdate = new List<string>();

        private bool dungeonAutoEnter = false;

        public bool DungeonAutoEnter { get { return dungeonAutoEnter; } }


        protected override void Init()
        {
            m_changeInfoAdTicketUpdate.Add(Define.PlayerDungeonInfoTable);
            m_changeInfoAdTicketUpdate.Add(Define.PlayerPointTable);

            DungeonDataOperator.Init();
        }
        //------------------------------------------------------------------------------------
        public void InitDungeonDataContent()
        {
            List<V2Enum_IntervalType> v2Enum_IntervalTypes = new List<V2Enum_IntervalType>();

            List<DungeonData> dungeonDatas = DungeonDataOperator.GetDungeonAllData();
            for (int i = 0; i < dungeonDatas.Count; ++i)
            {
                if (DungeonDataContainer.m_dungeonInitInfo.ContainsKey(dungeonDatas[i].DungeonType) == true)
                    continue;

                DungeonInitInfo dungeonInitInfo = new DungeonInitInfo();
                dungeonInitInfo.V2Enum_Dungeon = dungeonDatas[i].DungeonType;
                DungeonDataContainer.m_dungeonInitInfo.Add(dungeonInitInfo.V2Enum_Dungeon, dungeonInitInfo);
            }

            foreach (KeyValuePair<V2Enum_Dungeon, DungeonInitInfo> pair in DungeonDataOperator.GetDungeonAdAllInfo())
            {
                DungeonData dungeonData = GetDungeonData(pair.Key);
                DungeonInitInfo dungeonInitInfo = pair.Value;

                if (dungeonData == null)
                    continue;

                double currentTime = TimeManager.Instance.Current_TimeStamp;

                if (dungeonInitInfo.InitTimeStemp < currentTime)
                {
                    dungeonInitInfo.ToDayAdEnterCount = 0;
                    dungeonInitInfo.InitTimeStemp = TimeManager.Instance.GetInitTime(dungeonData.EnterCostRechargeIntervalType);
                    if (dungeonData.EnterCostParam1 == -1)
                        continue;
                    if (dungeonData.EnterCostRechargeAmount > GoodsManager.Instance.GetGoodsAmount(dungeonData.EnterCostParam1))
                        GoodsManager.Instance.SetGoodsAmount(dungeonData.EnterCostParam1, dungeonData.EnterCostRechargeAmount);
                }

                if (v2Enum_IntervalTypes.Contains(dungeonData.EnterCostRechargeIntervalType) == false)
                    v2Enum_IntervalTypes.Add(dungeonData.EnterCostRechargeIntervalType);
            }

            for (int i = 0; i < v2Enum_IntervalTypes.Count; ++i)
            {
                switch (v2Enum_IntervalTypes[i])
                {
                    case V2Enum_IntervalType.Day:
                        {
                            TimeManager.Instance.AddInitEvent(v2Enum_IntervalTypes[i], OnInitDailyContent);
                            break;
                        }
                    case V2Enum_IntervalType.Week:
                        {
                            TimeManager.Instance.AddInitEvent(v2Enum_IntervalTypes[i], OnInitWeekContent);
                            break;
                        }
                    case V2Enum_IntervalType.Month:
                        {
                            TimeManager.Instance.AddInitEvent(v2Enum_IntervalTypes[i], OnInitMonthContent);
                            break;
                        }
                }
            }

        }
        //------------------------------------------------------------------------------------
        public void OnInitDailyContent(double nextinittimestamp)
        {
            List<DungeonData> dungeonDatas = GetDungeonAllData();

            OnInitContent(dungeonDatas.FindAll(x => x.EnterCostRechargeIntervalType == V2Enum_IntervalType.Day), nextinittimestamp);
        }
        //------------------------------------------------------------------------------------
        public void OnInitWeekContent(double nextinittimestamp)
        {
            List<DungeonData> dungeonDatas = GetDungeonAllData();

            OnInitContent(dungeonDatas.FindAll(x => x.EnterCostRechargeIntervalType == V2Enum_IntervalType.Week), nextinittimestamp);
        }
        //------------------------------------------------------------------------------------
        public void OnInitMonthContent(double nextinittimestamp)
        {
            List<DungeonData> dungeonDatas = GetDungeonAllData();

            if (dungeonDatas == null)
                return;

            OnInitContent(dungeonDatas.FindAll(x => x.EnterCostRechargeIntervalType == V2Enum_IntervalType.Month), nextinittimestamp);
        }
        //------------------------------------------------------------------------------------
        public void OnInitContent(List<DungeonData> dungeonDatas, double nextinittimestamp)
        {
            m_refreshDungeonAdInfoListMsg.datas.Clear();

            for (int i = 0; i < dungeonDatas.Count; ++i)
            {
                DungeonData dungeonData = dungeonDatas[i];
                if (dungeonData == null)
                    continue;

                DungeonInitInfo dungeonInitInfo = GetDungeonInitInfo(dungeonData.DungeonType);

                dungeonInitInfo.ToDayAdEnterCount = 0;
                dungeonInitInfo.InitTimeStemp = nextinittimestamp;
                if (dungeonData.EnterCostParam1 == -1)
                    continue;
                if (dungeonData.EnterCostRechargeAmount > GoodsManager.Instance.GetGoodsAmount(dungeonData.EnterCostParam1))
                    GoodsManager.Instance.SetGoodsAmount(dungeonData.EnterCostParam1, dungeonData.EnterCostRechargeAmount);

                m_refreshDungeonAdInfoListMsg.datas.Add(dungeonData.DungeonType);
            }

            Message.Send(m_refreshDungeonAdInfoListMsg);
        }
        //------------------------------------------------------------------------------------
        public void SetAutoEnter(bool auto)
        {
            dungeonAutoEnter = auto;
        }
        //------------------------------------------------------------------------------------
        public List<DungeonData> GetDungeonAllData()
        {
            return DungeonDataOperator.GetDungeonAllData();
        }
        //------------------------------------------------------------------------------------
        public DungeonData GetDungeonData(V2Enum_Dungeon v2Enum_Dungeon)
        {
            return DungeonDataOperator.GetDungeonData(v2Enum_Dungeon);
        }
        //------------------------------------------------------------------------------------
        public DungeonInitInfo GetDungeonInitInfo(V2Enum_Dungeon v2Enum_Dungeon)
        {
            return DungeonDataOperator.GetDungeonInitInfo(v2Enum_Dungeon);
        }
        //------------------------------------------------------------------------------------
        public double GetDungeonTicketAmount(V2Enum_Dungeon v2Enum_Dungeon)
        {
            DungeonData dungeonData = GetDungeonData(v2Enum_Dungeon);

            if (dungeonData == null)
                return 0.0;

            return GoodsManager.Instance.GetGoodsAmount(dungeonData.EnterCostParam1);
        }
        //------------------------------------------------------------------------------------
        public bool SetAdTicket(V2Enum_Dungeon v2Enum_Dungeon, Action complete = null)
        {

            DungeonData dungeonData = GetDungeonData(v2Enum_Dungeon);
            if (dungeonData == null)
                return false;

            DungeonInitInfo dungeonInitInfo = GetDungeonInitInfo(v2Enum_Dungeon);
            if (dungeonInitInfo == null)
                return false;

            if (dungeonData.DailyAdCount <= dungeonInitInfo.ToDayAdEnterCount)
                return false;

            bool viewComplete = false;

            UnityPlugins.appLovin.ShowRewardedAd(() =>
            {
                dungeonInitInfo.ToDayAdEnterCount++;

                GoodsManager.Instance.AddGoodsAmount(dungeonData.EnterCostParam1, 1);

                ThirdPartyLog.Instance.SendLog_Dg_AdEvent(dungeonData.Index, Define.IsAdFree == true ? 1 : 2);

                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoAdTicketUpdate, null);

                ThirdPartyLog.Instance.SendLog_AD_ViewEvent("dungeon", dungeonData.Index, GameBerry.Define.IsAdFree == true ? 1 : 2);

                complete?.Invoke();

                viewComplete = true;

                Debug.Log("ShowRewardedAd");
            });

            Debug.Log(string.Format("viewComplete : {0}", viewComplete));

            return viewComplete;
        }
        //------------------------------------------------------------------------------------
        public void DoUseDungeonTicket(V2Enum_Dungeon v2Enum_Dungeon, int count = 1)
        {
            DungeonData dungeonData = GetDungeonData(v2Enum_Dungeon);

            if (dungeonData == null)
                return;

            GoodsManager.Instance.UseGoodsAmount(dungeonData.EnterCostParam1, dungeonData.EnterCostParam2 * count);
        }
        //------------------------------------------------------------------------------------
        public Sprite GetDungeonRewardSprite(V2Enum_Dungeon v2Enum_Dungeon)
        {
            switch (v2Enum_Dungeon)
            {
                case V2Enum_Dungeon.DiamondDungeon:
                    {
                        DungeonModeBase diamondDungeonData = DungeonDataOperator.GetMaxEnterDiamondDungeonData();
                        if (diamondDungeonData == null)
                            return null;

                        return GoodsManager.Instance.GetGoodsSprite(diamondDungeonData.ClearRewardParam1);
                    }
                case V2Enum_Dungeon.TowerDungeon:
                    {
                        DungeonModeBase diamondDungeonData = DungeonDataOperator.GetMaxEnterTowerDungeonData();
                        if (diamondDungeonData == null)
                            return null;

                        return GoodsManager.Instance.GetGoodsSprite(diamondDungeonData.ClearRewardParam1);
                    }
            }

            return null;
        }
        //------------------------------------------------------------------------------------
        public void GetDungeonRewardInfo(V2Enum_Dungeon v2Enum_Dungeon, out ObscuredInt index)
        {
            index = -1;

            switch (v2Enum_Dungeon)
            {
                case V2Enum_Dungeon.DiamondDungeon:
                    {
                        DungeonModeBase diamondDungeonData = DungeonDataOperator.GetMaxEnterDiamondDungeonData();
                        if (diamondDungeonData == null)
                            return;

                        index = diamondDungeonData.ClearRewardParam1;

                        return;
                    }
                case V2Enum_Dungeon.TowerDungeon:
                    {
                        DungeonModeBase diamondDungeonData = DungeonDataOperator.GetMaxEnterTowerDungeonData();
                        if (diamondDungeonData == null)
                            return;

                        index = diamondDungeonData.ClearRewardParam1;

                        return;
                    }
            }
        }
        //------------------------------------------------------------------------------------
        #region Dungeon
        //------------------------------------------------------------------------------------
        public bool AlreadySweepDungeon(V2Enum_Dungeon v2Enum_Dungeon)
        {
            DungeonData dungeonData = GetDungeonData(v2Enum_Dungeon);
            if (dungeonData == null)
                return false;

            int remainticket = (int)GetDungeonTicketAmount(v2Enum_Dungeon);

            if (remainticket == 0)
                remainticket = GetDungeonRemainAdViewCount(v2Enum_Dungeon);

            if(remainticket <= 0)
                return false;

            return GetDungeonRecord(v2Enum_Dungeon) > 0;
        }
        //------------------------------------------------------------------------------------
        public void DoSweepOnceDungeon(V2Enum_Dungeon v2Enum_Dungeon, Action<bool> action)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            int remainticket = (int)GetDungeonTicketAmount(v2Enum_Dungeon);
            if (remainticket <= 0)
            {
                remainticket = GetDungeonRemainAdViewCount(v2Enum_Dungeon);

                if (remainticket <= 0)
                {
                    action?.Invoke(false);
                    return;
                }

                if (SetAdTicket(v2Enum_Dungeon, () =>
                {
                    DoSweepOnce(v2Enum_Dungeon, true, action);
                }) == false)
                    action?.Invoke(false);

                return;
            }

            DoSweepOnce(v2Enum_Dungeon, false, action);
        }
        //------------------------------------------------------------------------------------
        private void DoSweepOnce(V2Enum_Dungeon v2Enum_Dungeon, bool isad, Action<bool> action)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            DungeonData dungeonData = GetDungeonData(v2Enum_Dungeon);

            m_setInGameRewardPopupMsg.GoodsType = V2Enum_Goods.Max;
            m_setInGameRewardPopupMsg.RewardDatas.Clear();

            int logStage = 0;
            int logcount = 1;
            double logrec_now = 0;
            double logrec_season = 0;

            double record = GetDungeonRecord(v2Enum_Dungeon);

            switch (v2Enum_Dungeon)
            {
                case V2Enum_Dungeon.DiamondDungeon:
                    {
                        m_setInGameRewardPopupMsg.RewardDatas.AddRange(GetDiamondDungeonReward((int)record));
                        logStage = (int)record;
                        break;
                    }
                case V2Enum_Dungeon.TowerDungeon:
                    {
                        m_setInGameRewardPopupMsg.RewardDatas.AddRange(GetTowerDungeonReward((int)record));
                        logStage = (int)record;
                        break;
                    }
            }

            List<int> reward_type = new List<int>();
            List<double> before_quan = new List<double>();
            List<double> reward_quan = new List<double>();
            List<double> after_quan = new List<double>();

            for (int i = 0; i < m_setInGameRewardPopupMsg.RewardDatas.Count; ++i)
            {
                RewardData rewardData = m_setInGameRewardPopupMsg.RewardDatas[i];

                reward_type.Add(rewardData.Index);
                before_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.Index));
                reward_quan.Add(rewardData.Amount);


                GoodsManager.Instance.AddGoodsAmount(rewardData.Index, rewardData.Amount);

                after_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.Index));
            }

            int used_type = 0;
            double former_quan, used_quan, keep_quan;

            former_quan = Managers.GoodsManager.Instance.GetGoodsAmount(dungeonData.EnterCostParam1);
            used_quan = dungeonData.EnterCostParam2;

            DoUseDungeonTicket(v2Enum_Dungeon);

            keep_quan = Managers.GoodsManager.Instance.GetGoodsAmount(dungeonData.EnterCostParam1);

            Message.Send(m_setInGameRewardPopupMsg);
            UI.IDialog.RequestDialogEnter<UI.InGameRewardPopupDialog>();

            m_setInGameRewardPopup_TitleMsg.title = LocalStringManager.Instance.GetLocalString("dungeon/sweepReward");
            Message.Send(m_setInGameRewardPopup_TitleMsg);

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoAdTicketUpdate, null);

            if (dungeonData != null)
            {
                ThirdPartyLog.Instance.SendLog_log_fast_clear(1, record,
                    used_type, former_quan, used_quan, keep_quan,
                    reward_type, before_quan, reward_quan, after_quan,
                    isad == false ? 0 : (Define.IsAdFree == true ? 1 : 2));
            }

            action?.Invoke(true);
        }
        //------------------------------------------------------------------------------------
        public bool DoSweepAllDungeon(V2Enum_Dungeon v2Enum_Dungeon)
        {
            int remainticket = (int)GetDungeonTicketAmount(v2Enum_Dungeon);
            if (remainticket <= 0) // 광고소탕은 여기서 취급 안한다
                return false;

            m_setInGameRewardPopupMsg.GoodsType = V2Enum_Goods.Max;
            m_setInGameRewardPopupMsg.RewardDatas.Clear();

            int logStage = 0;
            int logcount = 1;
            double logrec_now = 0;
            double logrec_season = 0;

            double record = GetDungeonRecord(v2Enum_Dungeon);

            switch (v2Enum_Dungeon)
            {
                case V2Enum_Dungeon.DiamondDungeon:
                    {
                        m_setInGameRewardPopupMsg.RewardDatas.AddRange(GetDiamondDungeonReward((int)record, remainticket));
                        logStage = (int)record;
                        break;
                    }
                case V2Enum_Dungeon.TowerDungeon:
                    {
                        m_setInGameRewardPopupMsg.RewardDatas.AddRange(GetTowerDungeonReward((int)record, remainticket));
                        logStage = (int)record;
                        break;
                    }
            }


            List<int> reward_type = new List<int>();
            List<double> before_quan = new List<double>();
            List<double> reward_quan = new List<double>();
            List<double> after_quan = new List<double>();

            for (int i = 0; i < m_setInGameRewardPopupMsg.RewardDatas.Count; ++i)
            {
                RewardData rewardData = m_setInGameRewardPopupMsg.RewardDatas[i];

                reward_type.Add(rewardData.Index);
                before_quan.Add((int)GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));
                reward_quan.Add(rewardData.Amount);

                GoodsManager.Instance.AddGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index, rewardData.Amount);

                after_quan.Add((int)GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));
            }

            DoUseDungeonTicket(v2Enum_Dungeon, remainticket);

            Message.Send(m_setInGameRewardPopupMsg);
            UI.IDialog.RequestDialogEnter<UI.InGameRewardPopupDialog>();

            m_setInGameRewardPopup_TitleMsg.title = LocalStringManager.Instance.GetLocalString("dungeon/sweepReward");
            Message.Send(m_setInGameRewardPopup_TitleMsg);

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoAdTicketUpdate, null);

            DungeonData dungeonData = GetDungeonData(v2Enum_Dungeon);

            if (dungeonData != null)
            {
                ThirdPartyLog.Instance.SendLog_Dg_ResultEvent(dungeonData.Index,
                    logStage, 1, logcount, logrec_now, logrec_season,
                    reward_type, before_quan, reward_quan, after_quan);
            }

            return true;
        }
        //------------------------------------------------------------------------------------
        public DungeonModeBase GetEnterDungeonData(V2Enum_Dungeon v2Enum_Dungeon)
        {
            switch (v2Enum_Dungeon)
            {
                case V2Enum_Dungeon.DiamondDungeon:
                    {
                        return GetEnterDiamondDungeonData();
                    }
                case V2Enum_Dungeon.TowerDungeon:
                    {
                        return GetEnterTowerDungeonData();
                    }
            }

            return GetEnterDiamondDungeonData();
        }
        //------------------------------------------------------------------------------------
        public DungeonModeBase GetMaxEnterDungeonModeData(V2Enum_Dungeon v2Enum_Dungeon)
        {
            return DungeonDataOperator.GetMaxEnterDungeonModeData(v2Enum_Dungeon);
        }
        //------------------------------------------------------------------------------------
        public DungeonModeBase GetMaxClearDungeonModeData(V2Enum_Dungeon v2Enum_Dungeon)
        {
            return DungeonDataOperator.GetMaxClearDungeonModeData(v2Enum_Dungeon);
        }
        //------------------------------------------------------------------------------------
        public double GetDungeonRecord(V2Enum_Dungeon v2Enum_Dungeon)
        {
            return DungeonDataOperator.GetDungeonRecord(v2Enum_Dungeon);
        }
        //------------------------------------------------------------------------------------
        public void SetDungeonRecord(V2Enum_Dungeon v2Enum_Dungeon, double record)
        {
            bool complete = DungeonDataOperator.SetDungeonRecord(v2Enum_Dungeon, record);

            if (complete == false)
                return;

            //switch (v2Enum_Dungeon)
            //{
            //    case V2Enum_Dungeon.DiamondDungeon:
            //        {
            //            ShopManager.Instance.RefreshProductContitionType(V2Enum_ProductConditionType.DiamondDungeonClear);
            //            ContentOpenConditionManager.Instance.RefreshOpenCondition(V2Enum_OpenConditionType.DiamondDungeonClear);
            //            break;
            //        }
            //}
        }
        //------------------------------------------------------------------------------------
        public int GetDungeonRemainAdViewCount(V2Enum_Dungeon v2Enum_Dungeon)
        {
            DungeonData dungeonData = GetDungeonData(v2Enum_Dungeon);
            if (dungeonData == null)
                return 0;

            DungeonInitInfo dungeonInitInfo = GetDungeonInitInfo(v2Enum_Dungeon);

            int remaincount = (int)dungeonData.DailyAdCount - dungeonInitInfo.ToDayAdEnterCount;
            if (remaincount < 0)
                remaincount = 0;

            return remaincount;
        }
        //------------------------------------------------------------------------------------
        public void SetEnterDungeonStep(V2Enum_Dungeon v2Enum_Dungeon, int step)
        {
            if (m_selectDungeonEnterStep.ContainsKey(v2Enum_Dungeon) == false)
            {
                DungeonEnterData dungeonEnterData = new DungeonEnterData();
                m_selectDungeonEnterStep.Add(v2Enum_Dungeon, dungeonEnterData);
            }

            m_selectDungeonEnterStep[v2Enum_Dungeon].step = step;
        }
        //------------------------------------------------------------------------------------
        public DungeonEnterData GetEnterDungeonStep(V2Enum_Dungeon v2Enum_Dungeon)
        {
            if (m_selectDungeonEnterStep.ContainsKey(v2Enum_Dungeon) == false)
            {
                DungeonEnterData dungeonEnterData = new DungeonEnterData();
                dungeonEnterData.step = 1;
                m_selectDungeonEnterStep.Add(v2Enum_Dungeon, dungeonEnterData);
                return null;
            }

            return m_selectDungeonEnterStep[v2Enum_Dungeon];
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region DiaDungeon
        //------------------------------------------------------------------------------------
        public DungeonModeBase GetEnterDiamondDungeonData()
        {
            if (m_selectDungeonEnterStep.ContainsKey(V2Enum_Dungeon.DiamondDungeon) == false)
                DungeonDataOperator.GetDiamondDungeonData(1);

            return DungeonDataOperator.GetDiamondDungeonData(m_selectDungeonEnterStep[V2Enum_Dungeon.DiamondDungeon].step);
        }
        //------------------------------------------------------------------------------------
        public List<RewardData> GetDiamondDungeonReward(int step, int addCount = 1)
        { // 보상 목록을 주기만 한다.
            List<RewardData> rewardDatas = new List<RewardData>();

            DungeonModeBase diamondDungeonData = DungeonDataOperator.GetDiamondDungeonData(step);
            RewardData rewardData = RewardManager.Instance.GetRewardData();
            rewardData.Index = diamondDungeonData.ClearRewardParam1;
            rewardData.Amount = diamondDungeonData.ClearRewardParam2 * addCount;

            rewardDatas.Add(rewardData);
            return rewardDatas;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TowerDungeon
        //------------------------------------------------------------------------------------
#if DEV_DEFINE
        public int SetTowerStep = 0;
        [ContextMenu("CheatSetTowerStep")]
        private void CheatSetTowerStep()
        {
            DungeonDataContainer.Tower_Dungeon_MaxClear = SetTowerStep;
        }
#endif
        //------------------------------------------------------------------------------------
        public DungeonModeBase GetEnterTowerDungeonData()
        {
            if (m_selectDungeonEnterStep.ContainsKey(V2Enum_Dungeon.TowerDungeon) == false)
                DungeonDataOperator.GetTowerDungeonData(1);

            return DungeonDataOperator.GetTowerDungeonData(m_selectDungeonEnterStep[V2Enum_Dungeon.TowerDungeon].step);
        }
        //------------------------------------------------------------------------------------
        public List<RewardData> GetTowerDungeonReward(int step, int addCount = 1)
        { // 보상 목록을 주기만 한다.
            List<RewardData> rewardDatas = new List<RewardData>();

            DungeonModeBase diamondDungeonData = DungeonDataOperator.GetTowerDungeonData(step);
            RewardData rewardData = RewardManager.Instance.GetRewardData();
            rewardData.Index = diamondDungeonData.ClearRewardParam1;
            rewardData.Amount = diamondDungeonData.ClearRewardParam2 * addCount;

            rewardDatas.Add(rewardData);
            return rewardDatas;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
    }
}