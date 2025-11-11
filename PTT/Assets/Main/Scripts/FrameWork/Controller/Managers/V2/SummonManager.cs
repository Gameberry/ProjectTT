using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.Managers
{
    public class SummonManager : MonoSingleton<SummonManager>
    {

        private Event.RefreshSummonInfoListMsg m_refreshSummonInfoListMsg = new Event.RefreshSummonInfoListMsg();
        private Event.SetSummonPopupMsg m_setSummonPopupMsg = new Event.SetSummonPopupMsg();
        private Event.SetSummonPopup_SummonBtnMsg m_setSummonPopup_SummonBtnMsg = new Event.SetSummonPopup_SummonBtnMsg();
        private GameBerry.Event.SetShopSummonPercentageDialogMsg m_setShopSummonPercentageDialogMsg = new GameBerry.Event.SetShopSummonPercentageDialogMsg();

        private List<string> m_changeSynergyInfoUpdate = new List<string>();

        private List<string> m_changeRelicInfoUpdate = new List<string>();

        private List<string> m_changeGearInfoUpdate = new List<string>();

        private bool m_waitSummon = false;
        private int m_waitStateClickCount = 0;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            m_changeSynergyInfoUpdate.Add(Define.PlayerSynergyInfoTable);
            m_changeSynergyInfoUpdate.Add(Define.PlayerDescendInfoTable);
            m_changeSynergyInfoUpdate.Add(Define.PlayerPointTable);
            m_changeSynergyInfoUpdate.Add(Define.PlayerSummonInfoTable);
            m_changeSynergyInfoUpdate.Add(Define.PlayerSynergyRuneInfoTable);

            m_changeRelicInfoUpdate.Add(Define.PlayerPointTable);
            m_changeRelicInfoUpdate.Add(Define.PlayerRelicInfoTable);
            m_changeRelicInfoUpdate.Add(Define.PlayerSummonInfoTable);

            m_changeGearInfoUpdate.Add(Define.PlayerPointTable);
            m_changeGearInfoUpdate.Add(Define.PlayerGearTable);
            m_changeGearInfoUpdate.Add(Define.PlayerSummonInfoTable);

            SummonOperator.Init();
        }
        //------------------------------------------------------------------------------------
        public void InitSummonContent()
        {
            for (int i = V2Enum_SummonType.SummonGear.Enum32ToInt(); i < V2Enum_SummonType.Max.Enum32ToInt(); ++i)
            {
                V2Enum_SummonType v2Enum_SummonType = i.IntToEnum32<V2Enum_SummonType>();
                //V2Enum_SummonType v2Enum_SummonType = V2Enum_SummonType.SummonNormal;

                if (SummonOperator.GetAllSummonInfo().ContainsKey(v2Enum_SummonType) == false)
                {
                    SummonData summonData = GetSummonData(v2Enum_SummonType);
                    SummonInfo summonInfo = new SummonInfo();
                    summonInfo.Id = summonData.Index;

                    SummonContainer.m_summonInfo.Add(v2Enum_SummonType, summonInfo);
                }
            }

            double currentTime = TimeManager.Instance.Current_TimeStamp;

            foreach (KeyValuePair<V2Enum_SummonType, SummonInfo> pair in SummonOperator.GetAllSummonInfo())
            {
                SummonInfo summonInfo = pair.Value;

                if (summonInfo.InitTimeStemp < currentTime)
                {
                    summonInfo.ToDayAdSummonCount = 0;
                    summonInfo.InitTimeStemp = TimeManager.Instance.DailyInit_TimeStamp;
                }

                //while (IsMaxLevel(pair.Key) == false && GetNeedExp(pair.Key) <= 0)
                //{
                //    SummonData summonData = GetSummonData(pair.Key);
                //    summonInfo.Count -= summonData.SummonLevelRequiredExp[summonInfo.Level + 1];
                //    summonInfo.Level++;

                //    TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSummonInfoTable);
                //}
            }

            SummonOperator.SetLogSummonData();

            TimeManager.Instance.OnInitDailyContent += OnInitDailyContent;
        }
        //------------------------------------------------------------------------------------
        public void OnInitDailyContent(double nextinittimestamp)
        {
            m_refreshSummonInfoListMsg.datas.Clear();

            foreach (KeyValuePair<V2Enum_SummonType, SummonInfo> pair in SummonOperator.GetAllSummonInfo())
            {
                SummonInfo summonInfo = pair.Value;

                summonInfo.ToDayAdSummonCount = 0;
                summonInfo.InitTimeStemp = nextinittimestamp;
                m_refreshSummonInfoListMsg.datas.Add(pair.Key);
            }

            Message.Send(m_refreshSummonInfoListMsg);
        }
        //------------------------------------------------------------------------------------
        public Dictionary<V2Enum_SummonType, SummonData> GetSummonDatas()
        {
            return SummonOperator.GetSummonDatas();
        }
        //------------------------------------------------------------------------------------
        public SummonData GetSummonData(V2Enum_SummonType v2Enum_SummonType)
        {
            return SummonOperator.GetSummonData(v2Enum_SummonType);
        }
        //------------------------------------------------------------------------------------
        public SummonInfo GetSummonInfo(V2Enum_SummonType v2Enum_SummonType)
        {
            return SummonOperator.GetSummonInfo(v2Enum_SummonType);
        }
        //------------------------------------------------------------------------------------
        public SummonGroupData GetSummonGroupData(V2Enum_SummonType v2Enum_SummonType)
        {
            return SummonOperator.GetSummonGroupData(v2Enum_SummonType);
        }
        //------------------------------------------------------------------------------------
        public SummonConfirmCountData GetSummonConfirmCountData(V2Enum_SummonType v2Enum_SummonType)
        {
            return SummonOperator.GetSummonConfirmCountData(v2Enum_SummonType);
        }
        //------------------------------------------------------------------------------------
        public SummonGroupData GetSummonConfirmDrawGroupData(V2Enum_SummonType v2Enum_SummonType)
        {
            return SummonOperator.GetSummonConfirmDrawGroupData(v2Enum_SummonType);
        }
        //------------------------------------------------------------------------------------
        public Sprite GetSummonTypeSprite(V2Enum_SummonType v2Enum_SummonType)
        {
            SummonData summonData = GetSummonData(v2Enum_SummonType);
            if (summonData == null)
                return null;

            return StaticResource.Instance.GetIcon(summonData.IconStringKey);
        }
        //------------------------------------------------------------------------------------
        public long GetAccumCount(V2Enum_SummonType v2Enum_SummonType)
        {
            return SummonOperator.GetAccumCount(v2Enum_SummonType);
        }
        //------------------------------------------------------------------------------------
        public long GetUseConfirmCount(V2Enum_SummonType v2Enum_SummonType)
        {
            return SummonOperator.GetUseConfirmCount(v2Enum_SummonType);
        }
        //------------------------------------------------------------------------------------
        public int GetAdSummonElementCount(V2Enum_SummonType v2Enum_SummonType)
        {
            return SummonOperator.GetAdSummonElementCount(v2Enum_SummonType);
        }
        //------------------------------------------------------------------------------------
        public int GetAdSummonRemainCount(V2Enum_SummonType v2Enum_SummonType)
        {
            return SummonOperator.GetAdSummonRemainCount(v2Enum_SummonType);
        }
        //------------------------------------------------------------------------------------
        public bool IsReadyAdSummon(V2Enum_SummonType v2Enum_SummonType)
        {
            return SummonOperator.IsReadyAdSummon(v2Enum_SummonType);
        }
        //------------------------------------------------------------------------------------
        public void DoAdSummon(V2Enum_SummonType v2Enum_SummonType)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            if (m_waitSummon == true)
                return;

            UnityPlugins.appLovin.ShowRewardedAd(() =>
            {
                SummonInfo summonInfo = GetSummonInfo(v2Enum_SummonType);
                int adsummoncount = GetAdSummonElementCount(v2Enum_SummonType);

                //if (v2Enum_SummonType == V2Enum_SummonType.SummonAlly)
                //{
                //    if (AllyV3Manager.Instance.GetAllyMaxStorage() < AllyV3Manager.Instance.GetAllyCurrentCount() + adsummoncount)
                //    {
                //        Contents.GlobalContent.ShowGlobalNotice(LocalStringManager.Instance.GetLocalString("summon/overflow/ally"));
                //        return;
                //    }
                //}
                //else if (v2Enum_SummonType == V2Enum_SummonType.SummonAllyJewelry)
                //{
                //    if (AllyJewelryManager.Instance.GetJewelryMaxStorage() < AllyJewelryManager.Instance.GetJewelryCurrentCount() + adsummoncount)
                //    {
                //        Contents.GlobalContent.ShowGlobalNotice(LocalStringManager.Instance.GetLocalString("summon/overflow/jewelry"));
                //        return;
                //    }
                //}

                List<SummonElementData> summonElementDatas = SummonOperator.DoGachaSummon(v2Enum_SummonType, adsummoncount);

                if (summonElementDatas == null || summonElementDatas.Count <= 0)
                    return;

                summonInfo.AdSummonCount++;

                summonInfo.ToDayAdSummonCount++;
                summonInfo.InitTimeStemp = TimeManager.Instance.DailyInit_TimeStamp;

                m_refreshSummonInfoListMsg.datas.Clear();
                m_refreshSummonInfoListMsg.datas.Add(v2Enum_SummonType);
                Message.Send(m_refreshSummonInfoListMsg);

                SummonData summonData = GetSummonData(v2Enum_SummonType);
                if (summonData != null && summonInfo != null)
                {
                    ThirdPartyLog.Instance.SendLog_GachaEvent(summonData.Index,
                                0, 0, 0, 0,
                                GameBerry.Define.IsAdFree == true ? 1 : 2);

                    ThirdPartyLog.Instance.SendLog_AD_ViewEvent("summon", summonData.Index, GameBerry.Define.IsAdFree == true ? 1 : 2);
                }

                ThirdPartyLog.Instance.SendLog_Gacha_Skill_Ad(MapContainer.MapLastEnter);

                if (v2Enum_SummonType == V2Enum_SummonType.SummonRelic)
                {
                    for (int i = 0; i < summonElementDatas.Count; ++i)
                    {
                        ThirdPartyLog.Instance.SendLog_log_artifact_acquire(summonElementDatas[i].GoodsIndex,
                0, 0, 0, 0);
                    }
                }

                SetSummonResult(v2Enum_SummonType, summonElementDatas);
            });
        }
        //------------------------------------------------------------------------------------
        public bool CanTicketUse(SummonCostData summonCostData)
        {
            V2Enum_Point v2Enum_Point = GetSummonTicketType(summonCostData.SummonType);
            if (v2Enum_Point == V2Enum_Point.Max)
                return false;

            SummonData summonData = GetSummonData(summonCostData.SummonType);

            double needTicketValue = summonCostData.summonCount * summonData.SummonCostParam14;

            return Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), v2Enum_Point.Enum32ToInt()) >= needTicketValue;
        }
        //------------------------------------------------------------------------------------
        public V2Enum_Point GetSummonTicketType(V2Enum_SummonType v2Enum_SummonType)
        {
            SummonData summonData = GetSummonData(v2Enum_SummonType);

            if (summonData.SummonCostParam13 == -1)
            {
                return V2Enum_Point.Max;
            }

            return summonData.SummonCostParam13.GetDecrypted().IntToEnum32<V2Enum_Point>();
        }
        //------------------------------------------------------------------------------------
        public bool IsReadySummon(SummonCostData summonCostData)
        {
            bool canTicketUse = CanTicketUse(summonCostData);

            SummonData summonData = GetSummonData(summonCostData.SummonType);
            if (summonData == null)
                return false;

            double costprice = 0.0;

            int summonCount = summonCostData.summonCount;

            if (canTicketUse == true)
                costprice = summonData.SummonCostParam14 * summonCount;
            else
                costprice = summonData.SummonCostParam12 * summonCount;

            int pointindex = canTicketUse == true ? summonData.SummonCostParam13 : summonData.SummonCostParam11;

            V2Enum_Goods v2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(pointindex);

            if (costprice > GoodsManager.Instance.GetGoodsAmount(v2Enum_Goods.Enum32ToInt(), pointindex))
                return false;

            return true;
        }
        //------------------------------------------------------------------------------------
        public void DoSummon(V2Enum_SummonType v2Enum_SummonType, SummonCostData summonCostData)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            if (m_waitSummon == true)
            {
                m_waitStateClickCount++;
                if(m_waitStateClickCount > 5)
                    Contents.GlobalContent.ShowGlobalNotice("Please Wait");
                return;
            }

            bool canTicketUse = CanTicketUse(summonCostData);

            if (canTicketUse == false)
            {
                if (IsReadySummon(summonCostData) == false)
                    return;
            }

            int summonCount = summonCostData.summonCount;

            List<SummonElementData> summonElementDatas = SummonOperator.DoGachaSummon(v2Enum_SummonType, summonCount);

            if (summonElementDatas == null || summonElementDatas.Count <= 0)
                return;

            SummonData summonData = GetSummonData(v2Enum_SummonType);
            if (summonData == null)
                return;

            double costprice = 0.0;


            if (canTicketUse == true)
                costprice = summonData.SummonCostParam14 * summonCount;
            else
                costprice = summonData.SummonCostParam12 * summonCount;

            int pointindex = canTicketUse == true ? summonData.SummonCostParam13 : summonData.SummonCostParam11;

            V2Enum_Goods v2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(pointindex);

            int used_type = pointindex;
            double former_quan = GoodsManager.Instance.GetGoodsAmount(v2Enum_Goods.Enum32ToInt(), pointindex);
            double used_quan = costprice;

            GoodsManager.Instance.UseGoodsAmount(v2Enum_Goods.Enum32ToInt(), pointindex, costprice);

            double keep_quan = GoodsManager.Instance.GetGoodsAmount(v2Enum_Goods.Enum32ToInt(), pointindex);

            m_refreshSummonInfoListMsg.datas.Clear();
            m_refreshSummonInfoListMsg.datas.Add(v2Enum_SummonType);
            Message.Send(m_refreshSummonInfoListMsg);

            SummonInfo summonInfo = GetSummonInfo(v2Enum_SummonType);
            if (summonData != null && summonInfo != null)
            {
                ThirdPartyLog.Instance.SendLog_Gacha_Skill(MapContainer.MapLastEnter);
                ThirdPartyLog.Instance.SendLog_GachaEvent(summonData.Index, 
                            used_type, former_quan, used_quan, keep_quan,
                            0);
            }


            if (v2Enum_SummonType == V2Enum_SummonType.SummonRelic)
            {
                for (int i = 0; i < summonElementDatas.Count; ++i)
                {
                    ThirdPartyLog.Instance.SendLog_log_artifact_acquire(summonElementDatas[i].GoodsIndex,
            used_type, former_quan, used_quan, keep_quan);
                }
            }

            SetSummonResult(v2Enum_SummonType, summonElementDatas);

            
        }
        //------------------------------------------------------------------------------------
        private void SetSummonResult(V2Enum_SummonType v2Enum_SummonType, List<SummonElementData> summonElementDatas)
        {
            if (summonElementDatas == null || summonElementDatas.Count <= 0)
                return;

            m_setSummonPopupMsg.RewardDatas.Clear();

            int tickcount = System.Environment.TickCount;

            SummonData summonData = GetSummonData(v2Enum_SummonType);
            SummonInfo summonInfo = GetSummonInfo(v2Enum_SummonType);

            List<int> reward_type = new List<int>();
            List<double> reward_quan = new List<double>();

            for (int i = 0; i < summonElementDatas.Count; ++i)
            {
                SummonElementData summonElementData = summonElementDatas[i];
                RewardData rewardData = RewardManager.Instance.GetRewardData();
                rewardData.V2Enum_Goods = GoodsManager.Instance.GetGoodsType(summonElementData.GoodsIndex);
                rewardData.Index = summonElementData.GoodsIndex;
                rewardData.Amount = summonElementData.GoodsValue;

                m_setSummonPopupMsg.RewardDatas.Add(rewardData);

                reward_type.Add(summonElementData.GoodsIndex);
                reward_quan.Add(summonElementData.GoodsValue);

                if (reward_type.Count >= 3)
                {
                    if (summonData != null && summonInfo != null)
                    {
                        ThirdPartyLog.Instance.SendLog_Gacha_ResultEvent(summonData.Index, tickcount, reward_type, reward_quan);
                        reward_type.Clear();
                    }
                }
            }

            for (int i = 0; i < m_setSummonPopupMsg.RewardDatas.Count; ++i)
            {
                GoodsManager.Instance.AddGoodsAmount(m_setSummonPopupMsg.RewardDatas[i]);
            }

            if (reward_type.Count > 0)
            {
                if (summonData != null && summonInfo != null)
                {
                    ThirdPartyLog.Instance.SendLog_Gacha_ResultEvent(summonData.Index, tickcount, reward_type, reward_quan);
                    reward_type.Clear();
                }
            }

            m_setSummonPopupMsg.GoodsType = V2Enum_Goods.Max;

            if (v2Enum_SummonType == V2Enum_SummonType.SummonRelic)
            { 
                m_setSummonPopupMsg.GoodsType = V2Enum_Goods.Relic;
                Managers.ShopManager.Instance.RefreshProductContitionType(V2Enum_OpenConditionType.RelicSummonCount);
            }
            else if (v2Enum_SummonType == V2Enum_SummonType.SummonNormal)
            {
                Managers.ShopManager.Instance.RefreshProductContitionType(V2Enum_OpenConditionType.SummonCount);
            }
            else if (v2Enum_SummonType == V2Enum_SummonType.SummonGear)
            {
                Managers.ShopManager.Instance.RefreshProductContitionType(V2Enum_OpenConditionType.GearSummonCount);
            }
            else if (v2Enum_SummonType == V2Enum_SummonType.SummonRune)
            {
                Managers.ShopManager.Instance.RefreshProductContitionType(V2Enum_OpenConditionType.RuneSummonCount);
            }

            m_setSummonPopup_SummonBtnMsg.v2Enum_SummonType = v2Enum_SummonType;

            m_waitSummon = true;

            if (v2Enum_SummonType == V2Enum_SummonType.SummonNormal
                || v2Enum_SummonType == V2Enum_SummonType.SummonRune)
            {
                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeSynergyInfoUpdate, ChangeReadySummon);
            }
            else if (v2Enum_SummonType == V2Enum_SummonType.SummonRelic)
            {
                //GuideQuestManager.Instance.CheckEventType(V2Enum_EventType.SkillSummon);
                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeRelicInfoUpdate, ChangeReadySummon);
            }
            else if (v2Enum_SummonType == V2Enum_SummonType.SummonGear)
            {
                //GuideQuestManager.Instance.CheckEventType(V2Enum_EventType.SkillSummon);
                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeGearInfoUpdate, ChangeReadySummon);
            }

            //MissionManager.Instance.AddMissionCount(V2Enum_MissionType.SummonCount, summonElementDatas.Count);
        }
        //------------------------------------------------------------------------------------
        private void ChangeReadySummon(BackEnd.BackendReturnObject backendReturnObject)
        {
            m_waitSummon = false;
            m_waitStateClickCount = 0;
            UI.IDialog.RequestDialogEnter<UI.ShopSummonResultPopupDialog>();

            Message.Send(m_setSummonPopupMsg);

            Message.Send(m_setSummonPopup_SummonBtnMsg);
        }
        //------------------------------------------------------------------------------------
        public void ShowPercendView(SummonGroupData summonGroupData)
        {
            if (summonGroupData == null)
                return;

            m_setShopSummonPercentageDialogMsg.summonGroupData = summonGroupData;
            Message.Send(m_setShopSummonPercentageDialogMsg);
            UI.IDialog.RequestDialogEnter<UI.ShopSummonPercentageDialog>();
        }
        //------------------------------------------------------------------------------------
        public int GetCanCumfirmCount(V2Enum_SummonType v2Enum_SummonType)
        {
            return (int)(GetAccumCount(v2Enum_SummonType) - GetUseConfirmCount(v2Enum_SummonType));
        }
        //------------------------------------------------------------------------------------
        public bool CanConfirm(V2Enum_SummonType v2Enum_SummonType)
        {
            SummonConfirmCountData summonConfirmCountData = GetSummonConfirmCountData(v2Enum_SummonType);

            return GetCanCumfirmCount(v2Enum_SummonType) >= summonConfirmCountData.RequiredCount;
        }
        //------------------------------------------------------------------------------------
        public bool PlayConfirm(V2Enum_SummonType v2Enum_SummonType)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return false;

            if (CanConfirm(v2Enum_SummonType) == false)
                return false;

            SummonConfirmCountData summonConfirmCountData = GetSummonConfirmCountData(v2Enum_SummonType);
            if (summonConfirmCountData == null)
                return false;

            SummonGroupData summonGroupData = GetSummonConfirmDrawGroupData(v2Enum_SummonType);

            if (summonGroupData == null)
                return false;


            SummonElementData summonElementData = summonGroupData.WeightedRandomPicker.Pick();


            m_setSummonPopupMsg.RewardDatas.Clear();

            RewardData rewardData = RewardManager.Instance.GetRewardData();
            rewardData.V2Enum_Goods = GoodsManager.Instance.GetGoodsType(summonElementData.GoodsIndex);
            rewardData.Index = summonElementData.GoodsIndex;
            rewardData.Amount = summonElementData.GoodsValue;

            m_setSummonPopupMsg.RewardDatas.Add(rewardData);


            for (int i = 0; i < m_setSummonPopupMsg.RewardDatas.Count; ++i)
            {
                GoodsManager.Instance.AddGoodsAmount(m_setSummonPopupMsg.RewardDatas[i]);
            }

            m_setSummonPopup_SummonBtnMsg.v2Enum_SummonType = V2Enum_SummonType.Max;

            m_setSummonPopupMsg.GoodsType = V2Enum_Goods.Max;

            SummonInfo summonInfo = GetSummonInfo(v2Enum_SummonType);

            summonInfo.UseConfirmCount += summonConfirmCountData.RequiredCount;

            if (v2Enum_SummonType == V2Enum_SummonType.SummonNormal
                || v2Enum_SummonType == V2Enum_SummonType.SummonRune)
            {
                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeSynergyInfoUpdate, ChangeReadySummon);
            }
            else if (v2Enum_SummonType == V2Enum_SummonType.SummonRelic)
            {
                //GuideQuestManager.Instance.CheckEventType(V2Enum_EventType.SkillSummon);
                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeRelicInfoUpdate, ChangeReadySummon);
            }
            else if (v2Enum_SummonType == V2Enum_SummonType.SummonGear)
            {
                //GuideQuestManager.Instance.CheckEventType(V2Enum_EventType.SkillSummon);
                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeGearInfoUpdate, ChangeReadySummon);
            }

            ThirdPartyLog.Instance.SendLog_Gacha_RewardEvent(v2Enum_SummonType.Enum32ToInt(), summonElementData.GoodsIndex);

            return true;
        }
        //------------------------------------------------------------------------------------
    }
}