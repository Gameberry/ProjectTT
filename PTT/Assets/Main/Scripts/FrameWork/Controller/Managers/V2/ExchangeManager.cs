using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.UI;

namespace GameBerry.Managers
{
    public class ExchangeManager : MonoSingleton<ExchangeManager>
    {
        private ExchangeLocalTable m_exchangeLocalTable = null;

        private Event.SetInGameRewardPopupMsg m_setInGameRewardPopupMsg = new Event.SetInGameRewardPopupMsg();
        private Event.RefreshExchangeDataMsg m_refreshExchangeDataMsg = new Event.RefreshExchangeDataMsg();

        private List<string> m_changeInfoUpdate = new List<string>();

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            m_changeInfoUpdate.Add(Define.PlayerExchangeInfoTable);
            m_changeInfoUpdate.Add(Define.PlayerPointTable);

            m_exchangeLocalTable = TableManager.Instance.GetTableClass<ExchangeLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public void InitExchangeContent()
        {
            double currentTime = TimeManager.Instance.Current_TimeStamp;

            foreach (var pair in ExchangeContainer.ExchangeInfos)
            {
                ExchangeInfo exchangeInfo = pair.Value;

                if (exchangeInfo.InitTimeStemp < currentTime)
                {
                    exchangeInfo.ToDayExchangeCount = 0;
                    exchangeInfo.InitTimeStemp = TimeManager.Instance.DailyInit_TimeStamp;
                }
            }

            if (Define.ExchangeInterval == V2Enum_IntervalType.Day)
                TimeManager.Instance.OnInitDailyContent += OnInitContent;
            else if (Define.ExchangeInterval == V2Enum_IntervalType.Week)
                TimeManager.Instance.OnInitWeekContent += OnInitContent;
            else if (Define.ExchangeInterval == V2Enum_IntervalType.Month)
                TimeManager.Instance.OnInitMonthContent += OnInitContent;
            else
                TimeManager.Instance.OnInitDailyContent += OnInitContent;
        }
        //------------------------------------------------------------------------------------
        public void OnInitContent(double nextinittimestamp)
        {
            m_refreshExchangeDataMsg.exchangeDatas.Clear();

            foreach (var pair in ExchangeContainer.ExchangeInfos)
            {
                ExchangeInfo exchangeInfo = pair.Value;

                exchangeInfo.ToDayExchangeCount = 0;
                exchangeInfo.InitTimeStemp = TimeManager.Instance.DailyInit_TimeStamp;

                ExchangeData exchangeData = GetExchangeData(exchangeInfo.Index);

                if (exchangeData == null)
                    continue;

                m_refreshExchangeDataMsg.exchangeDatas.Add(exchangeData);
            }

            Message.Send(m_refreshExchangeDataMsg);
        }
        //------------------------------------------------------------------------------------
        public double GetInitTime()
        {
            if (Define.ExchangeInterval == V2Enum_IntervalType.Day)
                return TimeManager.Instance.DailyInit_TimeStamp;
            else if (Define.ExchangeInterval == V2Enum_IntervalType.Week)
                return TimeManager.Instance.WeekInit_TimeStamp;
            else if (Define.ExchangeInterval == V2Enum_IntervalType.Month)
                return TimeManager.Instance.MonthInit_TimeStamp;

            return TimeManager.Instance.DailyInit_TimeStamp;
        }
        //------------------------------------------------------------------------------------
        public List<ExchangeData> GetExchangeDatas()
        {
            return m_exchangeLocalTable.GetDatas();
        }
        //------------------------------------------------------------------------------------
        public ExchangeData GetExchangeData(int index)
        {
            List<ExchangeData> exchangeDatas = GetExchangeDatas();
            if (exchangeDatas == null)
                return null;

            return exchangeDatas.Find(x => x.Index == index);
        }
        //------------------------------------------------------------------------------------
        public ExchangeInfo GetExchangeInfo(ExchangeData exchangeData)
        {
            if (exchangeData == null)
                return null;

            if (ExchangeContainer.ExchangeInfos.ContainsKey(exchangeData.Index) == true)
                return ExchangeContainer.ExchangeInfos[exchangeData.Index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public ExchangeInfo AddNewExchangeInfo(ExchangeData exchangeData)
        {
            if (exchangeData == null)
                return null;

            ExchangeInfo checkExchangeInfo = GetExchangeInfo(exchangeData);
            if (checkExchangeInfo != null)
                return null;

            ExchangeInfo exchangeInfo = new ExchangeInfo();
            exchangeInfo.Index = exchangeData.Index;
            exchangeInfo.ToDayExchangeCount = 0;
            exchangeInfo.InitTimeStemp = TimeManager.Instance.DailyInit_TimeStamp;

            ExchangeContainer.ExchangeInfos.Add(exchangeInfo.Index, exchangeInfo);

            return exchangeInfo;
        }
        //------------------------------------------------------------------------------------
        public int GetToDayExchangeCount(ExchangeData exchangeData)
        {
            if (exchangeData == null)
                return 0;

            ExchangeInfo exchangeInfo = GetExchangeInfo(exchangeData);
            if (exchangeInfo == null)
                return 0;

            return exchangeInfo.ToDayExchangeCount;
        }
        //------------------------------------------------------------------------------------
        public int GetCanExchangeLimitCount(ExchangeData exchangeData)
        {
            if (exchangeData == null)
                return 0;

            ExchangeInfo exchangeInfo = GetExchangeInfo(exchangeData);
            if (exchangeInfo == null)
                return exchangeData.ExchangeLimit;

            return exchangeData.ExchangeLimit - exchangeInfo.ToDayExchangeCount;
        }
        //------------------------------------------------------------------------------------
        public int GetCanMaxChangeCount(ExchangeData exchangeData)
        {
            int canLimitCount = GetCanExchangeLimitCount(exchangeData);

            if (canLimitCount <= 0)
                return 0;

            int materialMaxCount = -1;
            int CostMaxCount = -1;

            if (exchangeData.MaterialGoodsParam2 >= 1)
            {
                double materialRemain = Managers.GoodsManager.Instance.GetGoodsAmount(exchangeData.MaterialGoodsType.Enum32ToInt(), exchangeData.MaterialGoodsParam1);
                materialMaxCount = (materialRemain / exchangeData.MaterialGoodsParam2).ToInt();

                materialMaxCount = materialMaxCount > canLimitCount ? canLimitCount : materialMaxCount;
            }

            if (exchangeData.CostGoodsParam2 >= 1)
            {
                double materialRemain = Managers.GoodsManager.Instance.GetGoodsAmount(exchangeData.CostGoodsType.Enum32ToInt(), exchangeData.CostGoodsParam1);
                CostMaxCount = (materialRemain / exchangeData.CostGoodsParam2).ToInt();

                CostMaxCount = CostMaxCount > canLimitCount ? canLimitCount : CostMaxCount;
            }

            if (materialMaxCount == -1 && CostMaxCount == -1)
                return 0;
            else if (materialMaxCount != -1 && CostMaxCount != -1)
                return materialMaxCount > CostMaxCount ? CostMaxCount : materialMaxCount;
            
            return materialMaxCount < CostMaxCount ? CostMaxCount : materialMaxCount;
        }
        //------------------------------------------------------------------------------------
        public void DoExchange(ExchangeData exchangeData, int count)
        {
            if (exchangeData == null)
                return;

            int canMaxChangeCount = GetCanMaxChangeCount(exchangeData);

            if (canMaxChangeCount < 1)
                return;

            int changeCount = canMaxChangeCount < count ? canMaxChangeCount : count;

            List<int> used_type = new List<int>();
            List<double> former_quan = new List<double>();
            List< double > used_quan = new List<double>();
            List<double> keep_quan = new List<double>();
            

            List<int> reward_type = new List<int>();
            List<double> before_quan = new List<double>();
            List<double> reward_quan = new List<double>();
            List<double> after_quan = new List<double>();

            if (exchangeData.MaterialGoodsParam2 >= 1 && exchangeData.CostGoodsParam2 >= 1)
            {
                double materialNeedCount = exchangeData.MaterialGoodsParam2 * changeCount;
                double costNeedCount = exchangeData.CostGoodsParam2 * changeCount;

                if (materialNeedCount > GoodsManager.Instance.GetGoodsAmount(exchangeData.MaterialGoodsType.Enum32ToInt(), exchangeData.MaterialGoodsParam1)
                    || costNeedCount > GoodsManager.Instance.GetGoodsAmount(exchangeData.CostGoodsType.Enum32ToInt(), exchangeData.CostGoodsParam1))
                {
                    return;
                }

                used_type.Add(exchangeData.MaterialGoodsParam1);
                used_type.Add(exchangeData.CostGoodsParam1);

                former_quan.Add(GoodsManager.Instance.GetGoodsAmount(exchangeData.MaterialGoodsType.Enum32ToInt(), exchangeData.MaterialGoodsParam1));
                former_quan.Add(GoodsManager.Instance.GetGoodsAmount(exchangeData.CostGoodsType.Enum32ToInt(), exchangeData.CostGoodsParam1));

                used_quan.Add(materialNeedCount);
                used_quan.Add(costNeedCount);

                GoodsManager.Instance.UseGoodsAmount(exchangeData.MaterialGoodsType.Enum32ToInt(), exchangeData.MaterialGoodsParam1, materialNeedCount);
                GoodsManager.Instance.UseGoodsAmount(exchangeData.CostGoodsType.Enum32ToInt(), exchangeData.CostGoodsParam1, costNeedCount);

                keep_quan.Add(GoodsManager.Instance.GetGoodsAmount(exchangeData.MaterialGoodsType.Enum32ToInt(), exchangeData.MaterialGoodsParam1));
                keep_quan.Add(GoodsManager.Instance.GetGoodsAmount(exchangeData.CostGoodsType.Enum32ToInt(), exchangeData.CostGoodsParam1));
            }
            else
            {
                if (exchangeData.MaterialGoodsParam2 >= 1)
                {
                    double materialNeedCount = exchangeData.MaterialGoodsParam2 * changeCount;
                    if (materialNeedCount > GoodsManager.Instance.GetGoodsAmount(exchangeData.MaterialGoodsType.Enum32ToInt(), exchangeData.MaterialGoodsParam1))
                        return;

                    used_type.Add(exchangeData.MaterialGoodsParam1);
                    former_quan.Add(GoodsManager.Instance.GetGoodsAmount(exchangeData.MaterialGoodsType.Enum32ToInt(), exchangeData.MaterialGoodsParam1));
                    used_quan.Add(materialNeedCount);

                    GoodsManager.Instance.UseGoodsAmount(exchangeData.MaterialGoodsType.Enum32ToInt(), exchangeData.MaterialGoodsParam1, materialNeedCount);

                    keep_quan.Add(GoodsManager.Instance.GetGoodsAmount(exchangeData.MaterialGoodsType.Enum32ToInt(), exchangeData.MaterialGoodsParam1));
                }

                if (exchangeData.CostGoodsParam2 >= 1)
                {
                    double costNeedCount = exchangeData.CostGoodsParam2 * changeCount;
                    if (costNeedCount > GoodsManager.Instance.GetGoodsAmount(exchangeData.CostGoodsType.Enum32ToInt(), exchangeData.CostGoodsParam1))
                        return;

                    used_type.Add(exchangeData.CostGoodsParam1);
                    former_quan.Add(GoodsManager.Instance.GetGoodsAmount(exchangeData.CostGoodsType.Enum32ToInt(), exchangeData.CostGoodsParam1));
                    used_quan.Add(costNeedCount);

                    GoodsManager.Instance.UseGoodsAmount(exchangeData.CostGoodsType.Enum32ToInt(), exchangeData.CostGoodsParam1, costNeedCount);

                    keep_quan.Add(GoodsManager.Instance.GetGoodsAmount(exchangeData.CostGoodsType.Enum32ToInt(), exchangeData.CostGoodsParam1));
                }
            }

            ExchangeInfo exchangeInfo = GetExchangeInfo(exchangeData) ?? AddNewExchangeInfo(exchangeData);
            exchangeInfo.ToDayExchangeCount += count;
            exchangeInfo.InitTimeStemp = GetInitTime();

            reward_type.Add(exchangeData.ReturnGoodsParam1);
            before_quan.Add(GoodsManager.Instance.GetGoodsAmount(exchangeData.ReturnGoodsType.Enum32ToInt(), exchangeData.ReturnGoodsParam1));
            reward_quan.Add(exchangeData.ReturnGoodsParam2 * changeCount);

            GoodsManager.Instance.AddGoodsAmount(exchangeData.ReturnGoodsType.Enum32ToInt(), exchangeData.ReturnGoodsParam1, exchangeData.ReturnGoodsParam2 * changeCount);

            RewardData rewardData = RewardManager.Instance.GetRewardData();
            rewardData.V2Enum_Goods = exchangeData.ReturnGoodsType;
            rewardData.Index = exchangeData.ReturnGoodsParam1;
            rewardData.Amount = exchangeData.ReturnGoodsParam2 * changeCount;

            after_quan.Add(GoodsManager.Instance.GetGoodsAmount(exchangeData.ReturnGoodsType.Enum32ToInt(), exchangeData.ReturnGoodsParam1));

            m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);

            Message.Send(m_setInGameRewardPopupMsg);
            UI.IDialog.RequestDialogEnter<UI.InGameRewardPopupDialog>();

            m_refreshExchangeDataMsg.exchangeDatas.Clear();
            m_refreshExchangeDataMsg.exchangeDatas.Add(exchangeData);
            Message.Send(m_refreshExchangeDataMsg);

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate, null);

            ThirdPartyLog.Instance.SendLog_MarketEvent(count,
                            used_type, former_quan, used_quan, keep_quan,
                            reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
    }
}