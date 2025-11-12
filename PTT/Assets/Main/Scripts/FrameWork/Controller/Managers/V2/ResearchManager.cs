using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Common;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class ResearchManager : MonoSingleton<ResearchManager>
    {
        // totalLevel
        private Dictionary<V2Enum_Stat, ObscuredDouble> _arrrSynergyTotalStatValues = new Dictionary<V2Enum_Stat, ObscuredDouble>();
        public Dictionary<V2Enum_Stat, ObscuredDouble> ArrrSynergyTotalStatValues { get { return _arrrSynergyTotalStatValues; } }

        private Dictionary<int, Sprite> _skillIcons = new Dictionary<int, Sprite>();

        private Event.SetInGameRewardPopupMsg m_setInGameRewardPopupMsg = new Event.SetInGameRewardPopupMsg();

        private Event.RefreshResearchInfoListMsg m_refreshResearchInfoListMsg = new Event.RefreshResearchInfoListMsg();
        private Event.DrawNextResearchLineMsg m_drawNextResearchLineMsg = new Event.DrawNextResearchLineMsg();
        private Event.RefreshResearchSlotMsg m_refreshResearchSlotMsg = new Event.RefreshResearchSlotMsg();
        private Event.NoticeResearchCompleteMsg m_noticeResearchCompleteMsg = new Event.NoticeResearchCompleteMsg();
        private GameBerry.Event.RefreshCharacterInfo_StatMsg _refreshCharacterInfo_StatMsg = new Event.RefreshCharacterInfo_StatMsg();

        private Dictionary<Enum_SynergyType, ObscuredInt> _synergyGambleLevel = new Dictionary<Enum_SynergyType, ObscuredInt>();

        private Dictionary<V2Enum_ResearchType, ObscuredDouble> _researchValues = new Dictionary<V2Enum_ResearchType, ObscuredDouble>();

        private List<string> m_changeInfoUpdate = new List<string>();

        public event OnCallBack_String RechargeTime;

        private bool _showCanSlotReddot = false;
        private bool _showShopReddot = false;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            m_changeInfoUpdate.Add(Define.PlayerResearchInfoTable);
            m_changeInfoUpdate.Add(Define.PlayerPointTable);
            ResearchOperator.Init();
        }
        //------------------------------------------------------------------------------------
        public void InitResearchContent()
        {
            UnityUpdateManager.Instance.UpdateCoroutineFunc_HalfSec += CheckResearchTimer;


            if (_synergyGambleLevel.Count == 0)
            {
                for (int i = Enum_SynergyType.Red.Enum32ToInt(); i < Enum_SynergyType.Max.Enum32ToInt(); ++i)
                {
                    Enum_SynergyType Enum_SynergyType = i.IntToEnum32<Enum_SynergyType>();
                    _synergyGambleLevel.Add(Enum_SynergyType, 1);
                }
            }


            //List<ResearchData> researchNoneRootDatas = ResearchOperator.GetNoneRootData();

            //for (int i = 0; i < researchNoneRootDatas.Count; ++i)
            //{
            //    PlayerResearchInfo playerResearchInfo = GetPlayerResearchInfo(researchNoneRootDatas[i]);
            //    if (playerResearchInfo == null)
            //    {
            //        AddNewPlayerResearchInfo(researchNoneRootDatas[i]);
            //    }
            //}

            

            if (ResearchContainer.ResearchSlot.Count == 0)
            {
                for (int slot = 1; slot <= 3; ++slot)
                {
                    PlayerResearchSlotInfo playerResearchSlotInfo = new PlayerResearchSlotInfo();
                    playerResearchSlotInfo.Id = slot;
                    ResearchContainer.ResearchSlot.Add(slot, playerResearchSlotInfo);
                }
            }

            List<ResearchData> needNewInfo = null;

            double currentTime = TimeManager.Instance.Current_TimeStamp;

            bool needSave = false;

            foreach (KeyValuePair<int, PlayerResearchInfo> pair in ResearchContainer.ResearchInfo)
            {
                ResearchData researchData = GetResearchData(pair.Value.Id);

                if (researchData == null)
                    continue;

                PlayerResearchInfo playerResearchInfo = pair.Value;

                if (playerResearchInfo.CompleteTime > 1)
                {
                    if (playerResearchInfo.CompleteTime <= currentTime)
                    {
                        SetCompleteResearch(playerResearchInfo);

                        needSave = true;
                    }
                }

                double statvalue = GetOwnEffectValue(researchData);
                InCreaseResearchValue(researchData.ResearchEffectType, statvalue, false);

                if (IsMaxLevel(researchData) == true)
                {
                    for (int i = 0; i < researchData.NextResearchIndex.Count; ++i)
                    {
                        ResearchData nextdata = GetResearchData(researchData.NextResearchIndex[i]);
                        if (nextdata == null)
                            continue;

                        PlayerResearchInfo playerNextResearchInfo = GetPlayerResearchInfo(nextdata);
                        if (playerNextResearchInfo == null)
                        {
                            if (needNewInfo == null)
                                needNewInfo = new List<ResearchData>();

                            needNewInfo.Add(nextdata);
                        }
                    }
                }
            }

            if (ResearchContainer.DailyInitTime < TimeManager.Instance.Current_TimeStamp)
            {
                ResearchContainer.TodayAdViewCount = 0;
                ResearchContainer.DailyInitTime = TimeManager.Instance.DailyInit_TimeStamp;

                _showShopReddot = true;

                needSave = true;
            }

            InitResearch();

            TheBackEnd.TheBackEndManager.Instance.UpdatePlayerResearchInfoTable();

            if (needNewInfo != null)
            {
                for (int i = 0; i < needNewInfo.Count; ++i)
                {
                    AddNewPlayerResearchInfo(needNewInfo[i]);
                }
            }
            

            TimeManager.Instance.OnInitDailyContent += OnInitDailyContent;
        }
        //------------------------------------------------------------------------------------
        private void InitResearch()
        {
            double amount = ResearchContainer.ChargeResearchCount;
            double maxamount = GetMaxResearchCharge();
            if (maxamount <= amount)
            {
                //ShowMathRpgRedDot(ContentDetailList.EventMathRpg_Roulette);
                ResearchContainer.ReserchLastChargeTime = TimeManager.Instance.Current_TimeStamp;
            }
            else
            {
                double timegab = TimeManager.Instance.Current_TimeStamp - ResearchContainer.ReserchLastChargeTime;

                double interval = GeResearchChargeInterval();

                if (interval <= timegab)
                {
                    int cyclecount = (timegab / interval).ToInt();

                    double chargecount = GeResearchChargeCount() * cyclecount;

                    chargecount = Math.Truncate(chargecount);
                    
                    if (amount + chargecount > maxamount)
                    {
                        ResearchContainer.ChargeResearchCount = maxamount;
                        ResearchContainer.ReserchLastChargeTime = TimeManager.Instance.Current_TimeStamp;
                    }
                    else
                    {
                        ResearchContainer.ChargeResearchCount += chargecount;
                        ResearchContainer.ReserchLastChargeTime = TimeManager.Instance.Current_TimeStamp - (timegab - (interval * cyclecount));
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void RefreshSynergyRedDot()
        {
            if (_showShopReddot == true)
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbyResearch_Shop);

            if (_showCanSlotReddot == true)
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbyResearch);
        }
        //------------------------------------------------------------------------------------
        public void OnInitDailyContent(double nextinittimestamp)
        {
            ResearchContainer.TodayAdViewCount = 0;
            ResearchContainer.DailyInitTime = nextinittimestamp;

            TheBackEnd.TheBackEndManager.Instance.UpdatePlayerResearchInfoTable();

            Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbyResearch_Shop);
        }
        //------------------------------------------------------------------------------------
        private void CheckResearchTimer()
        {
            bool needSave = false;

            {

                double amount = ResearchContainer.ChargeResearchCount;

                if (GetMaxResearchCharge() <= amount)
                {
                    ResearchContainer.ReserchLastChargeTime = TimeManager.Instance.Current_TimeStamp;
                    //ShowMathRpgRedDot(ContentDetailList.EventMathRpg_Roulette);
                    RechargeTime?.Invoke("MAX");

                    if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Research) == true)
                        Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbyResearch_Charge);
                }
                else 
                {
                    double interval = GeResearchChargeInterval();

                    double nextChargeTime = interval + ResearchContainer.ReserchLastChargeTime;

                    if (nextChargeTime <= TimeManager.Instance.Current_TimeStamp)
                    {
                        {
                            double timegab = TimeManager.Instance.Current_TimeStamp - ResearchContainer.ReserchLastChargeTime;
                            double maxamount = GetMaxResearchCharge();
                            if (interval <= timegab)
                            {
                                int cyclecount = (timegab / interval).ToInt();

                                double chargecount = GeResearchChargeCount() * cyclecount;

                                if (amount + chargecount > maxamount)
                                {
                                    ResearchContainer.ChargeResearchCount = maxamount;
                                    ResearchContainer.ReserchLastChargeTime = TimeManager.Instance.Current_TimeStamp;
                                }
                                else
                                {
                                    ResearchContainer.ChargeResearchCount += chargecount;
                                    ResearchContainer.ReserchLastChargeTime = TimeManager.Instance.Current_TimeStamp - (timegab - (interval * cyclecount));
                                }
                            }
                        }

                        TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate, null);
                    }

                    int remainSecond = (nextChargeTime - TimeManager.Instance.Current_TimeStamp).ToInt();

                    RechargeTime?.Invoke(TimeManager.Instance.GetSecendToDayString_MS(remainSecond));
                }
                
            }

            if (Managers.GuideInteractorManager.Instance.PlayResearchTutorial == true)
                return;

            {
                double currentTime = TimeManager.Instance.Current_TimeStamp;

                for (int slot = 1; slot <= 3; ++slot)
                {
                    if (ResearchContainer.ResearchSlot.ContainsKey(slot) == true)
                    {
                        ResearchData researchData = GetSlotResearchData(slot);
                        if (researchData == null)
                            continue;

                        PlayerResearchInfo playerResearchInfo = GetPlayerResearchInfo(researchData);
                        if (playerResearchInfo == null)
                            continue;

                        if (IsDoingResearch(researchData) == true)
                        {
                            if (playerResearchInfo.CompleteTime <= currentTime)
                            {
                                SetCompleteResearch(playerResearchInfo);

                                double statvalue = researchData.ResearchEffectTypeLevelUpValue;
                                InCreaseResearchValue(researchData.ResearchEffectType, statvalue);

                                m_refreshResearchInfoListMsg.datas.Clear();
                                m_refreshResearchInfoListMsg.datas.Add(researchData);

                                m_drawNextResearchLineMsg.researchData = researchData;
                                Message.Send(m_drawNextResearchLineMsg);

                                for (int i = 0; i < researchData.NextResearchIndex.Count; ++i)
                                {
                                    ResearchData nextdata = GetResearchData(researchData.NextResearchIndex[i]);
                                    if (nextdata == null)
                                        continue;

                                    if (IsOpenCondition(nextdata) == true)
                                        m_refreshResearchInfoListMsg.datas.Add(nextdata);
                                }

                                Message.Send(m_refreshResearchInfoListMsg);

                                needSave = true;
                            }
                        }
                    }
                }
            }
            

            if (needSave == true)
                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate, null);
        }
        //------------------------------------------------------------------------------------
        private void SetCompleteResearch(PlayerResearchInfo playerResearchInfo)
        {
            double currentTime = TimeManager.Instance.Current_TimeStamp;

            if (playerResearchInfo.CompleteTime <= currentTime)
            {
                playerResearchInfo.Level++;
                playerResearchInfo.CompleteTime = 0;

                ResearchContainer.ResearchEndViewQueue.Add(playerResearchInfo.Id);

                for (int slot = 1; slot <= 3; ++slot)
                {
                    PlayerResearchSlotInfo playerResearchSlotInfo = GetPlayResearchSlotInfo(slot);
                    if (playerResearchSlotInfo == null)
                        continue;

                    if (playerResearchSlotInfo.ResearchTarget == playerResearchInfo.Id)
                    {
                        playerResearchSlotInfo.SetResearchTarget(-1);

                        m_refreshResearchSlotMsg.SlotIdx = slot;
                        Message.Send(m_refreshResearchSlotMsg);
                    }
                }

                Message.Send(m_noticeResearchCompleteMsg);

                _showCanSlotReddot = true;

                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.LobbyResearch);
            }
        }
        //------------------------------------------------------------------------------------
        public int GetResearchLevel(ResearchData researchData)
        {
            if (researchData == null)
                return 0;

            PlayerResearchInfo playerResearchInfo = GetPlayerResearchInfo(researchData);
            if (playerResearchInfo == null)
                return 0;

            return playerResearchInfo.Level;
        }
        //------------------------------------------------------------------------------------
        public Enum_SynergyType ConvertResearchTypeToSynergyType(V2Enum_ResearchType v2Enum_ResearchType)
        {
            switch (v2Enum_ResearchType)
            {
                case V2Enum_ResearchType.FireGambleLevel:
                    {
                        return Enum_SynergyType.Red;
                    }
                case V2Enum_ResearchType.GoldGambleLevel:
                    {
                        return Enum_SynergyType.Yellow;
                    }
                case V2Enum_ResearchType.WaterGambleLevel:
                    {
                        return Enum_SynergyType.Blue;
                    }
                case V2Enum_ResearchType.ThunderGambleLevel:
                    {
                        return Enum_SynergyType.White;
                    }
            }

            return Enum_SynergyType.Max;
        }
        //------------------------------------------------------------------------------------
        public void InCreaseResearchValue(V2Enum_ResearchType v2Enum_ResearchType, double effectValue, bool noticestat = true)
        {
            if (_researchValues.ContainsKey(v2Enum_ResearchType) == false)
                _researchValues.Add(v2Enum_ResearchType, 0);

            _researchValues[v2Enum_ResearchType] += effectValue;

            bool needRefreshStat = false;

            switch (v2Enum_ResearchType)
            {
                case V2Enum_ResearchType.FireGambleLevel:
                case V2Enum_ResearchType.GoldGambleLevel:
                case V2Enum_ResearchType.WaterGambleLevel:
                case V2Enum_ResearchType.ThunderGambleLevel:
                    {
                        Enum_SynergyType Enum_SynergyType = ConvertResearchTypeToSynergyType(v2Enum_ResearchType);
                        int setlevel = effectValue.ToInt();
                        if (_synergyGambleLevel[Enum_SynergyType] <= setlevel)
                            _synergyGambleLevel[Enum_SynergyType] = setlevel;

                        break;
                    }
                case V2Enum_ResearchType.Attack:
                    {
                        if (_arrrSynergyTotalStatValues.ContainsKey(V2Enum_Stat.Attack) == false)
                            _arrrSynergyTotalStatValues.Add(V2Enum_Stat.Attack, 0);

                        _arrrSynergyTotalStatValues[V2Enum_Stat.Attack] += effectValue;

                        needRefreshStat = true;
                        break;
                    }
                case V2Enum_ResearchType.Hp:
                    {
                        if (_arrrSynergyTotalStatValues.ContainsKey(V2Enum_Stat.HP) == false)
                            _arrrSynergyTotalStatValues.Add(V2Enum_Stat.HP, 0);

                        _arrrSynergyTotalStatValues[V2Enum_Stat.HP] += effectValue;

                        needRefreshStat = true;
                        break;
                    }
                case V2Enum_ResearchType.Defence:
                    {
                        if (_arrrSynergyTotalStatValues.ContainsKey(V2Enum_Stat.Defence) == false)
                            _arrrSynergyTotalStatValues.Add(V2Enum_Stat.Defence, 0);

                        _arrrSynergyTotalStatValues[V2Enum_Stat.Defence] += effectValue;

                        needRefreshStat = true;
                        break;
                    }
                case V2Enum_ResearchType.SkillCoolTimeReduce:
                    {
                        if (_arrrSynergyTotalStatValues.ContainsKey(V2Enum_Stat.SkillCoolTimeDecrease) == false)
                            _arrrSynergyTotalStatValues.Add(V2Enum_Stat.SkillCoolTimeDecrease, 0);

                        _arrrSynergyTotalStatValues[V2Enum_Stat.SkillCoolTimeDecrease] += effectValue;

                        needRefreshStat = true;
                        break;
                    }
            }

            if (noticestat == true && needRefreshStat == true)
            {
                BattleSceneManager.Instance.RefreshMyARRRStat();
                Message.Send(_refreshCharacterInfo_StatMsg);
                ARRRStatManager.Instance.RefreshBattlePower();
            }
        }
        //------------------------------------------------------------------------------------
        public double GetResearchValues(V2Enum_ResearchType v2Enum_ResearchType)
        {
            if (_researchValues.ContainsKey(v2Enum_ResearchType) == false)
                return 0.0;

            switch (v2Enum_ResearchType)
            {
                case V2Enum_ResearchType.FireGambleLevel:
                case V2Enum_ResearchType.GoldGambleLevel:
                case V2Enum_ResearchType.WaterGambleLevel:
                case V2Enum_ResearchType.ThunderGambleLevel:
                case V2Enum_ResearchType.Attack:
                case V2Enum_ResearchType.Hp:
                case V2Enum_ResearchType.Defence:
                case V2Enum_ResearchType.WaveRewardIncrease:
                case V2Enum_ResearchType.MissionRewardIncrease:
                case V2Enum_ResearchType.FreeGoodsCountIncrease:
                case V2Enum_ResearchType.FreeGoodsTimeDecrease:
                case V2Enum_ResearchType.FreeGoodsSaveIncrease:
                    {
                        return _researchValues[v2Enum_ResearchType];
                    }
                case V2Enum_ResearchType.SkillCoolTimeReduce:
                    {
                        return _researchValues[v2Enum_ResearchType] * 10000;
                    }
                default:
                    {
                        return _researchValues[v2Enum_ResearchType] * 0.01;
                    }
            }

            return _researchValues[v2Enum_ResearchType];
        }
        //------------------------------------------------------------------------------------
        public int GetSynergyGambleLevel(Enum_SynergyType Enum_SynergyType)
        {
            if (_synergyGambleLevel.ContainsKey(Enum_SynergyType) == false)
                return 1;

            return _synergyGambleLevel[Enum_SynergyType];
        }
        //------------------------------------------------------------------------------------
        public List<List<ResearchData>> GetResearchDatas()
        {
            return ResearchOperator.GetNormalAllData();
        }
        //------------------------------------------------------------------------------------
        public ResearchData GetResearchData(int index)
        {
            return ResearchOperator.GetResearchData(index);
        }
        //------------------------------------------------------------------------------------
        public ResearchOpenConditionData GetResearchOpenConditionData(int index)
        {
            return ResearchOperator.GetResearchOpenConditionData(index);
        }
        //------------------------------------------------------------------------------------
        public string GetResearchName(ResearchData researchData)
        {
            return LocalStringManager.Instance.GetLocalString(string.Format("researchtypename/{0}", researchData.Index));
        }
        //------------------------------------------------------------------------------------
        public string GetEffectTitle(V2Enum_ResearchType v2Enum_Stat)
        {
            return string.Format("researchtype/{0}", v2Enum_Stat);
        }
        //------------------------------------------------------------------------------------
        public double GetResearchTime(ResearchData researchData)
        {
            if (researchData == null)
                return double.MaxValue;

            int level = GetResearchLevel(researchData);

            double time = researchData.BaseResearchTimeValue + (researchData.IncreaseResearchTimeValue * level);

            time += time * (GetResearchValues(V2Enum_ResearchType.ResearchTimeReduce));

            return time;
        }
        //------------------------------------------------------------------------------------
        public double GetOwnEffectValue(ResearchData researchData)
        {
            return ResearchOperator.GetOwnEffectValue(researchData);
        }
        //------------------------------------------------------------------------------------
        public double GetOwnEffectValue_NextLevel(ResearchData researchData)
        {
            return ResearchOperator.GetOwnEffectValue_NextLevel(researchData);
        }
        //------------------------------------------------------------------------------------
        public bool IsSynergyResearchData(ResearchData researchData)
        {
            switch (researchData.ResearchEffectType)
            {
                case V2Enum_ResearchType.FireGambleLevel:
                case V2Enum_ResearchType.GoldGambleLevel:
                case V2Enum_ResearchType.WaterGambleLevel:
                case V2Enum_ResearchType.ThunderGambleLevel:
                    {
                        return true;
                    }
            }

            return false;
        }
        //------------------------------------------------------------------------------------
        public string GetDisplayCurrentValue(ResearchData researchData)
        {
            if (researchData == null)
                return string.Empty;

            if (IsSynergyResearchData(researchData) == true)
            {
                int level = GetSynergyGambleLevel(ConvertResearchTypeToSynergyType(researchData.ResearchEffectType));

                return ConvertDisplayString(researchData.ResearchEffectType, level);
            }

            double value = GetOwnEffectValue(researchData);

            return ConvertDisplayString(researchData.ResearchEffectType, value);
        }
        //------------------------------------------------------------------------------------
        public string GetDisplayNextValue(ResearchData researchData)
        {
            if (researchData == null)
                return string.Empty;

            if (IsSynergyResearchData(researchData) == true)
            {
                int sytype = researchData.ResearchEffectType.Enum32ToInt();
                int level = GetSynergyGambleLevel(sytype.IntToEnum32<Enum_SynergyType>()) + 1;

                return ConvertDisplayString(researchData.ResearchEffectType, level);
            }

            double value = GetOwnEffectValue_NextLevel(researchData);

            return ConvertDisplayString(researchData.ResearchEffectType, value);
        }
        //------------------------------------------------------------------------------------
        public string ConvertDisplayString(V2Enum_ResearchType v2Enum_ResearchType, double effectValue)
        {
            string desc = string.Empty;

            switch (v2Enum_ResearchType)
            {
                case V2Enum_ResearchType.FireGambleLevel:
                case V2Enum_ResearchType.GoldGambleLevel:
                case V2Enum_ResearchType.WaterGambleLevel:
                case V2Enum_ResearchType.ThunderGambleLevel:
                    {
                        int level = effectValue.ToInt();

                        desc = SynergyOperator.GetCardGradeLimitBreakLocalString(level);

                        break;
                    }
                case V2Enum_ResearchType.Attack:
                case V2Enum_ResearchType.Hp:
                case V2Enum_ResearchType.Defence:
                case V2Enum_ResearchType.WaveRewardIncrease:
                case V2Enum_ResearchType.MissionRewardIncrease:
                case V2Enum_ResearchType.FreeGoodsCountIncrease:
                case V2Enum_ResearchType.FreeGoodsTimeDecrease:
                case V2Enum_ResearchType.FreeGoodsSaveIncrease:
                    {
                        if (effectValue <= 0)
                            desc = string.Format("{0}", effectValue);
                        else
                            desc = string.Format("+{0}", effectValue);
                        break;
                    }
                default:
                    {
                        if (effectValue <= 0)
                            desc = string.Format("{0}%", effectValue);
                        else
                            desc = string.Format("+{0}%", effectValue);
                        break;
                    }
            }

            return desc;
        }
        //------------------------------------------------------------------------------------
        public PlayerResearchInfo AddNewPlayerResearchInfo(ResearchData researchData)
        {
            PlayerResearchInfo playerResearchInfo = ResearchOperator.AddNewPlayerResearchInfo(researchData);

            return playerResearchInfo;
        }
        //------------------------------------------------------------------------------------
        public PlayerResearchInfo GetPlayerResearchInfo(ResearchData researchData)
        {
            if (researchData == null)
                return null;

            return ResearchOperator.GetPlayerResearchInfo(researchData);
        }
        //------------------------------------------------------------------------------------
        public PlayerResearchSlotInfo GetPlayResearchSlotInfo(int idx)
        {
            if (ResearchContainer.ResearchSlot.ContainsKey(idx) == true)
                return ResearchContainer.ResearchSlot[idx];

            return null;
        }
        //------------------------------------------------------------------------------------
        public Sprite GetResearchSprite(ResearchData researchData)
        {
            Sprite sp = null;

            if (_skillIcons.ContainsKey(researchData.ResourceIndex) == false)
            {
                ResourceLoader.Instance.Load<Sprite>(string.Format(Define.ResearchPath, researchData.ResourceIndex), o =>
                {
                    sp = o as Sprite;
                    _skillIcons.Add(researchData.ResourceIndex, sp);
                });
            }
            else
                sp = _skillIcons[researchData.ResourceIndex];

            return sp;
        }
        //------------------------------------------------------------------------------------
        public double GetNeedLevelUPSpPoint(ResearchData researchData)
        {
            return ResearchOperator.GetNeedLevelUPSpPoint(researchData);
        }
        //------------------------------------------------------------------------------------
        public Sprite GetMastaryLevelUPGoodsSprite(ResearchData researchData)
        {
            return GoodsManager.Instance.GetGoodsSprite(researchData.LevelUpCostGoodsParam1);
        }
        //------------------------------------------------------------------------------------
        public bool IsMaxLevel(ResearchData researchData)
        {
            if (researchData == null)
                return false;

            PlayerResearchInfo playerResearchInfo = GetPlayerResearchInfo(researchData);
            if (playerResearchInfo == null)
                return false;

            if (researchData.ResearchMaxLevel == -1)
                return false;

            return researchData.ResearchMaxLevel <= playerResearchInfo.Level;
        }
        //------------------------------------------------------------------------------------
        public bool CheckReadyLevelUp(ResearchData researchData)
        {
            if (researchData == null)
                return false;

            if (IsMaxLevel(researchData) == true)
                return false;

            PlayerResearchInfo playerResearchInfo = GetPlayerResearchInfo(researchData);

            int level = playerResearchInfo == null ? 0 : playerResearchInfo.Level;

            double needgoods = GetNeedLevelUPSpPoint(researchData);
            double currentgoods = GoodsManager.Instance.GetGoodsAmount(researchData.LevelUpCostGoodsParam1);

            if (needgoods > currentgoods)
                return false;

            return true;
        }
        //------------------------------------------------------------------------------------
        public bool IsOpenCondition(ResearchData researchData)
        {
            if (researchData == null)
                return false;

            ResearchOpenConditionData researchOpenConditionData = GetResearchOpenConditionData(researchData.Index);
            if (researchOpenConditionData == null)
                return false;

            if (researchOpenConditionData.Preceeds.Count == 0)
                return true;

            foreach (var pair in researchOpenConditionData.Preceeds)
            {
                ResearchData check = GetResearchData(pair.Key);
                int currlevel = GetResearchLevel(check);
                if (currlevel < pair.Value)
                    return false;
            }

            return true;
        }
        //------------------------------------------------------------------------------------
        public bool ConnectLine(ResearchData start, ResearchData end)
        {
            if (start == null || end == null)
                return false;

            ResearchOpenConditionData researchOpenConditionData = GetResearchOpenConditionData(end.Index);
            if (researchOpenConditionData == null)
                return false;

            if (researchOpenConditionData.Preceeds.Count == 0)
                return true;

            if (researchOpenConditionData.Preceeds.ContainsKey(start.Index) == false)
                return false;

            int currlevel = GetResearchLevel(start);
            if (currlevel < researchOpenConditionData.Preceeds[start.Index])
                return false;

            return true;
        }
        //------------------------------------------------------------------------------------
        public void ShowNoticeOpenCondition(ResearchData researchData)
        {
            Contents.GlobalContent.ShowGlobalNotice(GetOpenContitionString(researchData));
        }
        //------------------------------------------------------------------------------------
        public string GetOpenContitionString(ResearchData researchData)
        {
            if (researchData == null)
                return string.Empty;

            ResearchOpenConditionData researchOpenConditionData = GetResearchOpenConditionData(researchData.Index);
            if (researchOpenConditionData == null)
                return string.Empty;

            string str = string.Empty;
            int i = 0;

            str = LocalStringManager.Instance.GetLocalString("synergy/breakthroughlock_skill");

            foreach (var pair in researchOpenConditionData.Preceeds)
            {
                ResearchData check = GetResearchData(pair.Key);

                string name = GetResearchName(check);

                //if (i == 0)
                //    str += string.Format("{0} : Lv.{1}", name, pair.Value);
                //else
                    str += string.Format("\n{0} : Lv.{1}", name, pair.Value);

                i++;
            }

            return str;
        }
        //------------------------------------------------------------------------------------
        public bool IsDoingResearch(ResearchData researchData)
        {
            if (researchData == null)
                return false;

            PlayerResearchInfo playerResearchInfo = GetPlayerResearchInfo(researchData);
            if (playerResearchInfo == null)
                return false;

            return playerResearchInfo.CompleteTime > 1;
        }
        //------------------------------------------------------------------------------------
        public int GetCanResearchSlot()
        {
            foreach (var pair in ResearchContainer.ResearchSlot)
            {
                PlayerResearchSlotInfo playerResearchSlotInfo = pair.Value;
                if (playerResearchSlotInfo.ResearchTarget == -1)
                {
                    if (IsLockSlot(pair.Key) == false)
                        return pair.Key;
                }
            }

            return -1;
        }
        //------------------------------------------------------------------------------------
        public bool IsLockSlot(int slot)
        {
            if (slot == 1)
                return false;
            else if (slot == 2)
            {
                return GoodsManager.Instance.GetGoodsAmount(V2Enum_Point.ResearchSlotTwo.Enum32ToInt()) <= 0;
            }
            else if (slot == 3)
            {
                if (Define.OpenResearchSlot == true)
                    return false;
                else
                {
                    return GetSlotResearchData(slot) == null;
                }
            }

            return true;
        }
        //------------------------------------------------------------------------------------
        public int GetRemainAdResearchAccelCount(int slot)
        {
            return Define.ResearchADTimeCount - ResearchContainer.TodayAdViewCount;
        }
        //------------------------------------------------------------------------------------
        public ResearchData GetSlotResearchData(int slot)
        {
            PlayerResearchSlotInfo playerResearchSlotInfo = GetPlayResearchSlotInfo(slot);
            if (playerResearchSlotInfo == null)
                return null;

            return GetResearchData(playerResearchSlotInfo.ResearchTarget);
        }
        //------------------------------------------------------------------------------------
        public bool DoAdResearchAccel(int slot)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return false;

            if (GetRemainAdResearchAccelCount(slot) <= 0)
                return false;

            PlayerResearchSlotInfo playerResearchSlotInfo = GetPlayResearchSlotInfo(slot);
            if (playerResearchSlotInfo == null)
                return false;

            ResearchData researchData = GetSlotResearchData(slot);
            if (researchData == null)
                return false;

            PlayerResearchInfo playerResearchInfo = GetPlayerResearchInfo(researchData);
            if (playerResearchInfo == null)
                return false;

            UnityPlugins.appLovin.ShowRewardedAd(() =>
            {
                if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                    return;

                playerResearchInfo.CompleteTime -= Define.ResearchADTime;

                ResearchContainer.TodayAdViewCount += 1;

                m_refreshResearchSlotMsg.SlotIdx = slot;
                Message.Send(m_refreshResearchSlotMsg);

                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate, null);

                ThirdPartyLog.Instance.SendLog_AD_ViewEvent("researchaccel", playerResearchSlotInfo.Id, GameBerry.Define.IsAdFree == true ? 1 : 2);
            });

            return true;
        }
        //------------------------------------------------------------------------------------
        public bool DoTicketResearchAccel(int slot, int count)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return false;

            PlayerResearchSlotInfo playerResearchSlotInfo = GetPlayResearchSlotInfo(slot);
            if (playerResearchSlotInfo == null)
                return false;

            ResearchData researchData = GetSlotResearchData(slot);
            if (researchData == null)
                return false;

            PlayerResearchInfo playerResearchInfo = GetPlayerResearchInfo(researchData);
            if (playerResearchInfo == null)
                return false;

            if (Managers.GoodsManager.Instance.GetGoodsAmount(Define.ResearchAccelTicketIndex) < count)
                return false;

            double former_quan = 0, keep_quan = 0;

            double ticketDuration = Define.ResearchAccelTime * count;

            former_quan = GoodsManager.Instance.GetGoodsAmount(Define.ResearchAccelTicketIndex);

            Managers.GoodsManager.Instance.UseGoodsAmount(Define.ResearchAccelTicketIndex, count);

            keep_quan = GoodsManager.Instance.GetGoodsAmount(Define.ResearchAccelTicketIndex);

            playerResearchInfo.CompleteTime -= ticketDuration;

            ThirdPartyLog.Instance.SendLog_log_research_speedup(researchData.Index, playerResearchInfo.Level, Define.ResearchAccelTicketIndex, former_quan, count, keep_quan);

            m_refreshResearchSlotMsg.SlotIdx = slot;
            Message.Send(m_refreshResearchSlotMsg);

            

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate, null);

            return true;
        }
        //------------------------------------------------------------------------------------
        public bool SetPlayerResearchInfo_LevelUp(ResearchData researchData)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return false;

            if (CheckReadyLevelUp(researchData) == false)
            {
                UI.UIManager.DialogEnter<UI.LobbyResearchTicketShopDialog>();
                return false;
            }

            if (IsOpenCondition(researchData) == false)
            {
                ShowNoticeOpenCondition(researchData);
                return false;
            }

            if (IsDoingResearch(researchData) == true)
            {
                Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("research/already"));
                return false;
            }

            int slot = GetCanResearchSlot();

            if (slot == -1)
            {
                Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("research/noavailableslots"));
                return false;
            }

            PlayerResearchInfo playerResearchInfo = GetPlayerResearchInfo(researchData);

            if (playerResearchInfo == null)
            {
                playerResearchInfo = AddNewPlayerResearchInfo(researchData);
            }

            double needgoods = GetNeedLevelUPSpPoint(researchData);

            double former_quan = 0, keep_quan = 0;

            former_quan = GoodsManager.Instance.GetGoodsAmount(researchData.LevelUpCostGoodsParam1);

            GoodsManager.Instance.UseGoodsAmount(researchData.LevelUpCostGoodsParam1, needgoods);

            keep_quan = GoodsManager.Instance.GetGoodsAmount(researchData.LevelUpCostGoodsParam1);

            double time = GetResearchTime(researchData);

            playerResearchInfo.CompleteTime = time + TimeManager.Instance.Current_TimeStamp;

            PlayerResearchSlotInfo playerResearchSlotInfo = GetPlayResearchSlotInfo(slot);
            playerResearchSlotInfo.SetResearchTarget(researchData.Index);

            ThirdPartyLog.Instance.SendLog_log_research_levelup(researchData.Index, playerResearchInfo.Level, researchData.LevelUpCostGoodsParam1, former_quan, needgoods, keep_quan);

            if (time <= 0)
            {
                SetCompleteResearch(playerResearchInfo);

                double statvalue = researchData.ResearchEffectTypeLevelUpValue;
                InCreaseResearchValue(researchData.ResearchEffectType, statvalue);

                m_drawNextResearchLineMsg.researchData = researchData;
                Message.Send(m_drawNextResearchLineMsg);

                for (int i = 0; i < researchData.NextResearchIndex.Count; ++i)
                {
                    ResearchData nextdata = GetResearchData(researchData.NextResearchIndex[i]);
                    if (nextdata == null)
                        continue;

                    if (IsOpenCondition(nextdata) == true)
                        m_refreshResearchInfoListMsg.datas.Add(nextdata);
                }

                Message.Send(m_refreshResearchInfoListMsg);
            }

            m_refreshResearchInfoListMsg.datas.Clear();
            m_refreshResearchInfoListMsg.datas.Add(researchData);

            Message.Send(m_refreshResearchInfoListMsg);

            m_refreshResearchSlotMsg.SlotIdx = slot;
            Message.Send(m_refreshResearchSlotMsg);



            



            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate, null);



            return true;
        }
        //------------------------------------------------------------------------------------
        #region ResearchCharge
        //------------------------------------------------------------------------------------
        public double GetMaxResearchCharge()
        {
            double returnvalue = Define.ResearchGoodsLimitCount + GetResearchValues(V2Enum_ResearchType.FreeGoodsSaveIncrease);
            return Math.Truncate(returnvalue);
        }
        //------------------------------------------------------------------------------------
        public bool IsMaxResearchCharge()
        {
            double amount = ResearchContainer.ChargeResearchCount;

            return GetMaxResearchCharge() <= amount;
        }
        //------------------------------------------------------------------------------------
        public double GeResearchChargeInterval()
        {
            double returnvalue = Define.ResearchChargingTime + GetResearchValues(V2Enum_ResearchType.FreeGoodsTimeDecrease);
            return Math.Truncate(returnvalue);
        }
        //------------------------------------------------------------------------------------
        public double GeResearchChargeCount()
        {
            double returnvalue = Define.ResearchGoodsEarn + GetResearchValues(V2Enum_ResearchType.FreeGoodsCountIncrease);
            return Math.Truncate(returnvalue);
        }
        //------------------------------------------------------------------------------------
        public bool GetResearchChargeAmount()
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return false;

            if (ResearchContainer.ChargeResearchCount <= 0)
                return false;

            double amount = ResearchContainer.ChargeResearchCount;

            int reward_type = Define.ResearchGoodsIndex;
            double before_quan = GoodsManager.Instance.GetGoodsAmount(Define.ResearchGoodsIndex);
            double reward_quan = amount;

            Managers.GoodsManager.Instance.AddGoodsAmount(Define.ResearchGoodsIndex, amount);

            double after_quan = GoodsManager.Instance.GetGoodsAmount(Define.ResearchGoodsIndex);

            ResearchContainer.ChargeResearchCount = 0;

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate, null);

            RewardData rewardData = RewardManager.Instance.GetRewardData();
            rewardData.V2Enum_Goods = V2Enum_Goods.Point;
            rewardData.Index = V2Enum_Point.ResearchTicket.Enum32ToInt();
            rewardData.Amount = amount;

            ThirdPartyLog.Instance.SendLog_log_fossil_acquire(1,
                reward_type, before_quan, reward_quan, after_quan,
                0, 0, 0, 0);

            m_setInGameRewardPopupMsg.RewardDatas.Clear();
            m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);

            Message.Send(m_setInGameRewardPopupMsg);
            UI.UIManager.DialogEnter<UI.InGameRewardPopupDialog>();

            return true;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
    }
}