using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Common;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class MapManager : MonoSingleton<MapManager>
    {
        private Event.SetInGameRewardPopupMsg m_setInGameRewardPopupMsg = new Event.SetInGameRewardPopupMsg();
        private GameBerry.Event.SetInGameRewardPopup_TitleMsg m_SetInGameRewardPopup_TitleMsg = new GameBerry.Event.SetInGameRewardPopup_TitleMsg();
        private Event.RefreshLobbyMapSelecterMapMsg _refreshLobbyMapSelecterMapMsg = new Event.RefreshLobbyMapSelecterMapMsg();
        private Event.RefreshStageSweepMsg _refreshStageSweepMsg = new Event.RefreshStageSweepMsg();
        private Event.RefreshResultBattleStage_DoubleRewardMsg _refreshResultBattleStage_DoubleRewardMsg = new Event.RefreshResultBattleStage_DoubleRewardMsg();
        

        private MapData[] _disPlayMapDatas = new MapData[Define.DisplayMapDataCount];

        private List<string> _changeInfoUpdate = new List<string>();

        List<string> _changesweepInfoUpdate = new List<string>();

        public int Stage1TryCount = 0;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            _changeInfoUpdate.Add(Define.PlayerMapInfoTable);
            _changeInfoUpdate.Add(Define.PlayerPointTable);
            _changeInfoUpdate.Add(Define.PlayerSynergyInfoTable);
            _changeInfoUpdate.Add(Define.PlayerDescendInfoTable);
            _changeInfoUpdate.Add(Define.PlayerRelicInfoTable);

            _changesweepInfoUpdate.Add(Define.PlayerQuestInfoTable);
            _changesweepInfoUpdate.Add(Define.PlayerPointTable);
            _changesweepInfoUpdate.Add(Define.PlayerMapInfoTable);

            MapOperator.Init();
        }
        //------------------------------------------------------------------------------------
        public void InitMapContent()
        {
            MapContainer.MapLastEnter = MapContainer.MapMaxClear + 1;
            if (MapContainer.MapLastEnter < MapOperator.GetMaxMapNumber())
                MapContainer.MapLastEnter = MapOperator.GetMaxMapNumber();

            if (MapContainer.MapLastEnter > MapOperator.GetMinMapNumber())
                MapContainer.MapLastEnter = MapOperator.GetMinMapNumber();

            if (MapContainer.EventDayInitTime < TimeManager.Instance.Current_TimeStamp)
            {
                MapContainer.EventDayInitTime = TimeManager.Instance.DailyInit_TimeStamp;
                MapContainer.ToDaySweepCount = 0;
                MapContainer.ToDayDoubleRewardCount = 0;
                
            }

            TimeManager.Instance.AddInitEvent(V2Enum_IntervalType.Day, OnInitDailyContent);
        }
        //------------------------------------------------------------------------------------
        public void OnInitDailyContent(double nextinittimestamp)
        {
            MapContainer.EventDayInitTime = nextinittimestamp;
            MapContainer.ToDaySweepCount = 0;
            MapContainer.ToDayDoubleRewardCount = 0;

            Message.Send(_refreshStageSweepMsg);

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(_changeInfoUpdate, null);
        }
        //------------------------------------------------------------------------------------
        public void SetMapLastEnter(ObscuredInt mapLastEnter)
        {
            MapContainer.MapLastEnter = mapLastEnter;
        }
        //------------------------------------------------------------------------------------
        public WaveClearRewardData GetShowWaveClearRewardData()
        {
            List<WaveClearRewardData> clearRewardDatas = MapOperator.GetAllWaveClearRewardData();

            for (int i = 0; i < clearRewardDatas.Count; ++i)
            {
                WaveClearRewardData waveClearRewardData = clearRewardDatas[i];
                StageInfo stageInfo = GetStageInfo(waveClearRewardData.StageNumber);

                if (stageInfo == null)
                    return waveClearRewardData;

                if (stageInfo.RecvClearReward >= waveClearRewardData.WaveNumber)
                { // 이미 받은거
                    continue;
                }

                return waveClearRewardData;
            }

            return null;
        }
        //------------------------------------------------------------------------------------
        public bool IsAlReadyWaveClearReward(WaveClearRewardData waveClearRewardData)
        {
            StageInfo stageInfo = GetStageInfo(waveClearRewardData.StageNumber);

            if (stageInfo == null)
                return false;

            return stageInfo.RecvClearReward >= waveClearRewardData.WaveNumber;
        }
        //------------------------------------------------------------------------------------
        public bool IsReadyWaveClearReward(WaveClearRewardData waveClearRewardData)
        {
            StageInfo stageInfo = GetStageInfo(waveClearRewardData.StageNumber);

            if (stageInfo == null)
                return false;

            if (stageInfo.RecvClearReward >= waveClearRewardData.WaveNumber)
            { // 이미 받은거
                return false;
            }

            if (stageInfo.LastClearWave >= waveClearRewardData.WaveNumber)
                return true;

            return false;
        }
        //------------------------------------------------------------------------------------
        public bool RecvWaveClearReward(WaveClearRewardData waveClearRewardData)
        {
            if (waveClearRewardData == null)
                return false;


            StageInfo stageInfo = GetStageInfo(waveClearRewardData.StageNumber);

            if (stageInfo == null)
                return false;

            if (stageInfo.RecvClearReward >= waveClearRewardData.WaveNumber)
                return false;


            Dictionary<ObscuredInt, WaveClearRewardData> waveClearDatas = GetWaveClearsRewardData(waveClearRewardData.StageNumber);
            if (waveClearDatas == null)
                return false;

            UI.UIManager.Instance.IgnoreEnterDialog = false;

            m_setInGameRewardPopupMsg.RewardDatas.Clear();
            List<int> idx = new List<int>();

            List<int> reward_type = new List<int>();
            List<double> before_quan = new List<double>();
            List<double> reward_quan = new List<double>();
            List<double> after_quan = new List<double>();

            foreach (var pair in waveClearDatas)
            {
                if (pair.Value.WaveNumber > stageInfo.RecvClearReward && pair.Value.WaveNumber <= waveClearRewardData.WaveNumber)
                {
                    WaveClearRewardData reward = pair.Value;

                    idx.Add(reward.Index.GetDecrypted());

                    RewardData rewardData = m_setInGameRewardPopupMsg.RewardDatas.Find(x => x.Index == reward.PerfectClearReward.Index.GetDecrypted());
                    if (rewardData == null)
                    {
                        rewardData = RewardManager.Instance.GetRewardData();
                        rewardData.V2Enum_Goods = GoodsManager.Instance.GetGoodsType(reward.PerfectClearReward.Index);
                        rewardData.Index = reward.PerfectClearReward.Index.GetDecrypted();
                        rewardData.Amount = 0;
                        m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);
                    }

                    rewardData.Amount += reward.PerfectClearReward.Amount.GetDecrypted();

                    if (stageInfo.StageNumber == Define.SynergyTutorialStage
    && reward.WaveNumber == Define.SynergyTutorialWave)
                    {
                        if (Managers.GuideInteractorManager.Instance.PlayedMainTutorial() == true)
                            Managers.GuideInteractorManager.Instance.NeedSynergyTutorial = true;
                        else
                            Message.Send(new Event.PlaySynergyChangeTutorialMsg());
                    }

                    if (stageInfo.StageNumber == Define.SynergyOpenTutorialStage
    && reward.WaveNumber == Define.SynergyOpenTutorialWave)
                    {
                        if (Managers.GuideInteractorManager.Instance.PlayedMainTutorial() == true)
                            Managers.GuideInteractorManager.Instance.NeedSynergyOpenTutorial = true;
                        else
                            Message.Send(new Event.PlaySynergyOpenTutorialMsg());
                    }

                    if (stageInfo.StageNumber == Define.SynergyUnLockTutorialStage
    && reward.WaveNumber == Define.SynergyUnLockTutorialWave)
                    {
                        if (Managers.GuideInteractorManager.Instance.PlayedMainTutorial() == true)
                            Managers.GuideInteractorManager.Instance.NeedSynergyUnLockTutorial = true;
                        else
                            Message.Send(new Event.PlaySynergyUnLockTutorialMsg());
                    }

                    if (stageInfo.StageNumber == Define.GearTutorialStage
    && reward.WaveNumber == Define.GearTutorialWave)
                    {
                        if (Managers.GuideInteractorManager.Instance.PlayedMainTutorial() == true)
                            Managers.GuideInteractorManager.Instance.NeedGearTutorial = true;
                        else
                            Message.Send(new Event.PlayGearTutorialMsg());
                    }

                    if (stageInfo.StageNumber == Define.GearEquipTutorialStage
    && reward.WaveNumber == Define.GearEquipTutorialWave)
                    {
                        if (Managers.GuideInteractorManager.Instance.PlayedMainTutorial() == true)
                            Managers.GuideInteractorManager.Instance.NeedGearEquipTutorial = true;
                        else
                            Message.Send(new Event.PlayGearEquipTutorialMsg { Index = rewardData.Index });
                    }

                    if (stageInfo.StageNumber == Define.ResearchTutorialStage
    && reward.WaveNumber == Define.ResearchTutorialWave)
                    {
                        if (Managers.GuideInteractorManager.Instance.PlayedMainTutorial() == true)
                            Managers.GuideInteractorManager.Instance.NeedResearchTutorial = true;
                        else
                            Message.Send(new Event.PlayResearchChangeTutorialMsg());
                    }

                    if (stageInfo.StageNumber == Define.RelicTutorialStage
    && reward.WaveNumber == Define.RelicTutorialWave)
                    {
                        if (Managers.GuideInteractorManager.Instance.PlayedMainTutorial() == true)
                            Managers.GuideInteractorManager.Instance.NeedRelicTutorial = true;
                        else
                            Message.Send(new Event.PlayRelicTutorialMsg());
                    }

                    if (stageInfo.StageNumber == Define.SynergyBreakTutorialStage
    && reward.WaveNumber == Define.SynergyBreakTutorialWave)
                    {
                        if (Managers.GuideInteractorManager.Instance.PlayedMainTutorial() == true)
                            Managers.GuideInteractorManager.Instance.NeedSynergyBreakTutorial = true;
                        else
                            Message.Send(new Event.PlaySynergyBreakTutorialMsg());
                    }


                    if (stageInfo.StageNumber == Define.RuneTutorialStage
    && reward.WaveNumber == Define.RuneTutorialWave)
                    {
                        if (Managers.GuideInteractorManager.Instance.PlayedMainTutorial() == true)
                            Managers.GuideInteractorManager.Instance.NeedRuneTutorial = true;
                        else
                            Message.Send(new Event.PlayRuneTutorialMsg());
                    }


                    if (stageInfo.StageNumber == Define.DescendTutorialStage
                        && reward.WaveNumber == Define.DescendTutorialWave)
                    {
                        if (Managers.GuideInteractorManager.Instance.PlayedMainTutorial() == true)
                            Managers.GuideInteractorManager.Instance.NeedDescendChangeTutorial = true;
                        else
                            Message.Send(new Event.PlayDescendChangeTutorialMsg());
                    }


                    if (stageInfo.StageNumber == Define.JobTutorialStage
                        && reward.WaveNumber == Define.JobTutorialWave)
                    {
                        if (Managers.GuideInteractorManager.Instance.PlayedMainTutorial() == true)
                            Managers.GuideInteractorManager.Instance.NeedJobChangeTutorial = true;
                        else
                            Message.Send(new Event.PlayJobTutorialMsg());
                    }


                    if (stageInfo.StageNumber == Define.DungeonTutorialStage
                        && reward.WaveNumber == Define.DungeonTutorialWave)
                    {
                        if (Managers.GuideInteractorManager.Instance.PlayedMainTutorial() == true)
                            Managers.GuideInteractorManager.Instance.NeedDungeonTutorial = true;
                        else
                            Message.Send(new Event.PlayDungeonTutorialMsg());
                    }
                }
            }

            stageInfo.RecvClearReward = waveClearRewardData.WaveNumber;

            Managers.ContentOpenConditionManager.Instance.RefreshOpenCondition(V2Enum_OpenConditionType.WaveReward);
            Managers.ShopManager.Instance.RefreshProductContitionType(V2Enum_OpenConditionType.WaveReward);
            Managers.PassManager.Instance.CheckPassType(V2Enum_PassType.MonsterKill);
            
            if (m_setInGameRewardPopupMsg.RewardDatas.Count > 0)
            {
                for (int i = 0; i < m_setInGameRewardPopupMsg.RewardDatas.Count; ++i)
                {
                    RewardData rewardData = m_setInGameRewardPopupMsg.RewardDatas[i];

                    reward_type.Add(rewardData.Index);
                    before_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));
                    reward_quan.Add(rewardData.Amount);

                    GoodsManager.Instance.AddGoodsAmount(rewardData);
                    after_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));
                }

                Message.Send(m_setInGameRewardPopupMsg);
                UI.UIManager.DialogEnter<UI.InGameRewardPopupDialog>();

                ThirdPartyLog.Instance.SendLog_log_progress_reward(idx, reward_type, before_quan, reward_quan, after_quan);
            }

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(_changeInfoUpdate, null);

            Message.Send(_refreshLobbyMapSelecterMapMsg);

            return true;
        }
        //------------------------------------------------------------------------------------
        public void SaveEnterKey(string mapkey)
        {
            MapContainer.MapEnterKey = mapkey;
            TheBackEnd.TheBackEndManager.Instance.UpdatePlayerMapInfoTable();
            MapContainer.MapEnterKey = string.Empty;
        }
        //------------------------------------------------------------------------------------
        public void RemoveEnterKey()
        {
            MapContainer.MapEnterKey = string.Empty;
            TheBackEnd.TheBackEndManager.Instance.UpdatePlayerMapInfoTable();
        }
        //------------------------------------------------------------------------------------
        public bool NeedTutotial1()
        {
            if (MapContainer.MapMaxClear == 0)
            {
                StageInfo stageInfo = GetStageInfo(MapContainer.MapMaxClear);
                if (stageInfo != null)
                    return stageInfo.RecvClearReward != stageInfo.LastClearWave;
            }

            return MapContainer.MapMaxClear == -1;
        }
        //------------------------------------------------------------------------------------
        public bool NeedTutotial2()
        {
            return MapContainer.MapMaxClear == -9;
        }
        //------------------------------------------------------------------------------------
        public StageInfo GetStageInfo(MapData mapData)
        {
            if (mapData == null)
                return null;

            return GetStageInfo(mapData.StageNumber);
        }
        //------------------------------------------------------------------------------------
        public StageInfo GetStageInfo(ObscuredInt Number)
        {
            if (MapContainer.StageInfos.ContainsKey(Number) == false)
                return null;

            return MapContainer.StageInfos[Number];
        }
        //------------------------------------------------------------------------------------
        public StageInfo GetLastChellengeInfo()
        {
            StageInfo stageInfo = GetStageInfo(MapContainer.MapMaxClear + 1);
            if (stageInfo != null)
                return stageInfo;

            return GetStageInfo(MapContainer.MapMaxClear);
        }
        //------------------------------------------------------------------------------------
        public int GetStar(MapData mapData)
        {
            if (mapData == null)
                return 0;

            StageInfo mapInfo = GetStageInfo(mapData);

            if (mapInfo == null)
                return 0;

            //return mapInfo.Star;
            return 0;
        }
        //------------------------------------------------------------------------------------
        public MapData GetMapData(ObscuredInt Number)
        {
            return MapOperator.GetMapData(Number);
        }
        //------------------------------------------------------------------------------------
        public int GetMapMaxClear()
        {
            return MapContainer.MapMaxClear;
        }
        //------------------------------------------------------------------------------------
        public bool IsClearStage(MapData mapData)
        {
            return MapContainer.MapMaxClear >= mapData.StageNumber;
        }
        //------------------------------------------------------------------------------------
        public MapData GetMaxClearMapData()
        {
            MapData mapData = GetMapData(MapContainer.MapMaxClear);
            return mapData;
        }
        //------------------------------------------------------------------------------------
        public MapData GetLobbyDisPlayMapData()
        {
            if (MapContainer.MapMaxClear == -1)
            {
                return GetMapData(MapOperator.GetMinMapNumber());
            }

            MapData mapData = null;

            StageInfo tutorialStageInfo = GetStageInfo(MapContainer.MapMaxClear);
            if (tutorialStageInfo != null)
            {
                Dictionary<ObscuredInt, WaveClearRewardData> clearRewards = GetWaveClearsRewardData(MapContainer.MapMaxClear);
                if (clearRewards != null)
                {
                    int waveReward = 0;

                    foreach (var pair in clearRewards)
                    {
                        if (waveReward < pair.Value.WaveNumber)
                            waveReward = pair.Value.WaveNumber;
                    }

                    if (tutorialStageInfo.RecvClearReward < waveReward)
                    { 
                        return GetMapData(MapContainer.MapMaxClear);
                    }
                }
            }
            else
                return GetMapData(0);

            return GetNextClearMap();
        }
        //------------------------------------------------------------------------------------
        public MapData GetNextClearMap()
        {
            MapData mapData = GetMapData(MapContainer.MapMaxClear + 1);
            if(mapData == null)
                mapData = GetMapData(MapOperator.GetMaxMapNumber());

            return mapData;
        }
        //------------------------------------------------------------------------------------
        public void SetResultStageInfo(WaveRewardData waveRewardData)
        {
            if (waveRewardData == null)
                return;

            MapData mapData = GetMapData(waveRewardData.StageNumber);

            StageInfo mapInfo = GetStageInfo(mapData);

            if (mapInfo == null)
            {
                mapInfo = new StageInfo();
                mapInfo.StageNumber = waveRewardData.StageNumber;
                mapInfo.LastClearWave = 0;
                mapInfo.RecvClearReward = 0;
                MapContainer.StageInfos.Add(mapInfo.StageNumber, mapInfo);
            }

            bool needSaveData = false;

            if (mapInfo.LastClearWave < waveRewardData.WaveNumber)
            {
                mapInfo.LastClearWave = waveRewardData.WaveNumber;

                MapWaveData mapWaveData = mapData.MapWaveDatas.Find(x => x.WaveNumber == waveRewardData.WaveNumber);
                if (mapWaveData != null)
                    MapContainer.MaxWaveClear = mapWaveData.TotalNumber;

                Managers.RankManager.Instance.SetStage(mapWaveData.TotalNumber);

                needSaveData = true;
            }

            if (waveRewardData.WaveNumber >= mapData.MaxWave)
            {
                if (waveRewardData.StageNumber > MapContainer.MapMaxClear)
                {
                    MapContainer.MapMaxClear = waveRewardData.StageNumber;
                    needSaveData = true;
                }
            }



            if (needSaveData == true)
            {
                // 스테이지쪽에서 저장해서 주석
                //    TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(_changeInfoUpdate, null);

                Managers.ContentOpenConditionManager.Instance.RefreshOpenCondition(V2Enum_OpenConditionType.Stage);
                Managers.ContentOpenConditionManager.Instance.RefreshOpenCondition(V2Enum_OpenConditionType.Wave);

                Managers.PassManager.Instance.CheckPassType(V2Enum_PassType.Wave);
            }
        }
        //------------------------------------------------------------------------------------
        public void GetStageSweepReward()
        {
            StageInfo stageInfo = GetStageInfo(MapContainer.MapMaxClear);
            if (stageInfo == null)
                return;

            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Sweep) == false)
            {
                Managers.ContentOpenConditionManager.Instance.ShowOpenConditionNotice(V2Enum_ContentType.Sweep);
                return;
            }

            if (Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), Define.StaminaIndex) < Define.RequiredStamina)
            {
                UI.UIManager.DialogEnter<UI.LobbyStaminaShopDialog>();
                return;
            }

            if (Define.IsAdFree == false)
            {
                if (GetRemainSweepCount() <= 0)
                {
                    Managers.UIQuickLinkManager.Instance.ShowQuickLink(ContentDetailList.ShopVipStore_AD);
                    return;
                }
            }

            WaveRewardData _currentWaveRewardData = GetWaveRewardData(MapContainer.MapMaxClear, stageInfo.LastClearWave);
            if (_currentWaveRewardData == null)
            {
                Contents.GlobalContent.ShowGlobalNotice("Reset MapInfo");
                return;
            }

            UnityPlugins.appLovin.ShowRewardedAd(() =>
            {
                ThirdPartyLog.Instance.SendLog_AD_ViewEvent("stagesweep", MapContainer.MapMaxClear, GameBerry.Define.IsAdFree == true ? 1 : 2);

                if (_currentWaveRewardData != null)
                {
                    bool _adBuff_IncreaseRewardMode = false;
                    double _adBuff_IncreaseRewardValue = 0.0;

                    if (_adBuff_IncreaseRewardMode == false)
                        _adBuff_IncreaseRewardMode = Define.IsAdBuffAlways;

                    if (_adBuff_IncreaseRewardMode == true)
                    {
                        AdBuffActiveData adBuffActiveData = Managers.AdBuffManager.Instance.GetBuffData(V2Enum_BuffEffectType.IncreaseReward);
                        _adBuff_IncreaseRewardValue = adBuffActiveData.BuffValue * Define.PerSkillEffectRecoverValue;
                    }

                    double value = _adBuff_IncreaseRewardValue;

                    for (int i = 0; i < _currentWaveRewardData.WaveRewardRangeDatas.Count; ++i)
                    {
                        WaveRewardRangeData waveRewardRangeData = _currentWaveRewardData.WaveRewardRangeDatas[i];

                        RewardData rewardData = m_setInGameRewardPopupMsg.RewardDatas.Find(x => x.Index == waveRewardRangeData.Index);
                        if (rewardData == null)
                        {
                            rewardData = Managers.RewardManager.Instance.GetRewardData();
                            rewardData.V2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(waveRewardRangeData.Index);
                            rewardData.Index = waveRewardRangeData.Index;
                            rewardData.Amount = 0;

                            m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);
                        }

                        double amount = UnityEngine.Random.Range(waveRewardRangeData.Min, waveRewardRangeData.Max + 1);

                        amount += amount * _adBuff_IncreaseRewardValue;

                        amount += amount * Managers.ResearchManager.Instance.GetResearchValues(V2Enum_ResearchType.StageRewardIncrease);

                        rewardData.Amount += amount;
                    }

                    ThirdPartyLog.Instance.SendLog_StageResult(_currentWaveRewardData.StageNumber, _currentWaveRewardData.WaveNumber, new List<int>());
                    Managers.MapManager.Instance.SetResultStageInfo(_currentWaveRewardData);


                    List<int> reward_type = new List<int>();
                    List<double> before_quan = new List<double>();
                    List<double> reward_quan = new List<double>();
                    List<double> after_quan = new List<double>();

                    for (int i = 0; i < m_setInGameRewardPopupMsg.RewardDatas.Count; ++i)
                    {
                        RewardData rewardData = m_setInGameRewardPopupMsg.RewardDatas[i];
                        rewardData.Amount = Math.Truncate(rewardData.Amount);

                        reward_type.Add(rewardData.Index);
                        before_quan.Add(Managers.GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));
                        reward_quan.Add(rewardData.Amount);

                        Managers.GoodsManager.Instance.AddGoodsAmount(rewardData);

                        after_quan.Add((int)Managers.GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));
                    }

                    MapContainer.ToDaySweepCount += 1;

                    int used_type = 0;
                    double former_quan, used_quan, keep_quan;

                    former_quan = Managers.GoodsManager.Instance.GetGoodsAmount(Define.StaminaIndex);
                    used_quan = Define.RequiredStamina;

                    Managers.GoodsManager.Instance.UseGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), Define.StaminaIndex, Define.RequiredStamina);


                    keep_quan = Managers.GoodsManager.Instance.GetGoodsAmount(Define.StaminaIndex);




                    

                    GameBerry.Managers.QuestManager.Instance.AddMissionCount(GameBerry.V2Enum_QuestGoalType.StageChallenge, 1);

                    TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(_changesweepInfoUpdate, o =>
                    {
                        if (o.IsSuccess())
                        {
                            Message.Send(m_setInGameRewardPopupMsg);
                            UI.UIManager.DialogEnter<UI.InGameRewardPopupDialog>();
                        }
                    });

                    m_SetInGameRewardPopup_TitleMsg.title = Managers.LocalStringManager.Instance.GetLocalString("stage/sweep");
                    Message.Send(m_SetInGameRewardPopup_TitleMsg);

                    Message.Send(_refreshStageSweepMsg);

                    ThirdPartyLog.Instance.SendLog_log_fast_clear(0, MapContainer.MapMaxClear,
                    used_type, former_quan, used_quan, keep_quan,
                    reward_type, before_quan, reward_quan, after_quan,
                    Define.IsAdFree == true ? 1 : 2);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public int GetRemainSweepCount()
        {
            return Define.DailySweepCount - MapContainer.ToDaySweepCount;
        }
        //------------------------------------------------------------------------------------
        public int GetRemainDoubleRewardCount()
        {
            return Define.DailyDoubleRewardCount - MapContainer.ToDayDoubleRewardCount;
        }
        //------------------------------------------------------------------------------------
        public void DoDoubleReward(Dictionary<int, RewardData> coolreward)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            if (GetRemainDoubleRewardCount() <= 0)
                return;

            

            UnityPlugins.appLovin.ShowRewardedAd(() =>
            {
                List<int> reward_type = new List<int>();
                List<double> before_quan = new List<double>();
                List<double> reward_quan = new List<double>();
                List<double> after_quan = new List<double>();

                _refreshResultBattleStage_DoubleRewardMsg.WaveRewardList.Clear();
                _refreshResultBattleStage_DoubleRewardMsg.EnumDungeon = Enum_Dungeon.StageScene;


                foreach (var pair in coolreward)
                {

                    RewardData rewardData = pair.Value;

                    reward_type.Add(rewardData.Index);
                    before_quan.Add(Managers.GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));
                    reward_quan.Add(rewardData.Amount);

                    Managers.GoodsManager.Instance.AddGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index, rewardData.Amount);

                    after_quan.Add(Managers.GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));

                    rewardData.Amount += rewardData.Amount;

                    _refreshResultBattleStage_DoubleRewardMsg.WaveRewardList.Add(rewardData);
                }

                //_refreshResultBattleStage_DoubleRewardMsg.WaveRewardList;

                Message.Send(_refreshResultBattleStage_DoubleRewardMsg);

                MapContainer.ToDayDoubleRewardCount += 1;
                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(_changesweepInfoUpdate);
            });



            
        }
        //------------------------------------------------------------------------------------
        public MapData[] GetDisPlayMapDatas(int selectNumber)
        {
            if (MapOperator.GetMinMapNumber() > selectNumber)
                return null;

            if (MapOperator.GetMaxMapNumber() < selectNumber)
                return null;

            int convertselect = selectNumber - 1;

            int start = convertselect - (convertselect % Define.DisplayMapDataCount); // 5의 범위로 정렬
            start += 1;
            for (int i = 0; i < 5; i++)
            {
                _disPlayMapDatas[i] = MapOperator.GetMapData(start + i);
            }

            return _disPlayMapDatas;
        }
        //------------------------------------------------------------------------------------
        public WaveRewardData GetWaveRewardData(ObscuredInt index)
        {
            return MapOperator.GetWaveRewardData(index);
        }
        //------------------------------------------------------------------------------------
        public WaveRewardData GetWaveRewardData(ObscuredInt stage, ObscuredInt wave)
        {
            return MapOperator.GetWaveRewardData(stage, wave);
        }
        //------------------------------------------------------------------------------------
        public WaveClearRewardData GetWaveClearRewardData(ObscuredInt stage, ObscuredInt wave)
        {
            return MapOperator.GetWaveClearRewardData(stage, wave);
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, WaveClearRewardData> GetWaveClearsRewardData(ObscuredInt stage)
        {
            return MapOperator.GetWaveClearsRewardData(stage);
        }
        //------------------------------------------------------------------------------------
        public MapData GetPrevChellengeData(MapData mapData)
        {
            MapData nextMapData = GetMapData(mapData.StageNumber - 1);

            if (nextMapData != null && nextMapData.StageNumber == 0)
                return null;

            return nextMapData;
        }
        //------------------------------------------------------------------------------------
        public MapData GetNextChellengeData(MapData mapData)
        {
            if (mapData.StageNumber > MapContainer.MapMaxClear)
                return null;

            StageInfo stageInfo = GetStageInfo(mapData);
            if (stageInfo.RecvClearReward < stageInfo.LastClearWave)
                return null;

            MapData nextMapData = GetMapData(mapData.StageNumber + 1);
            return nextMapData;
        }
        //------------------------------------------------------------------------------------
        public WaveClearRewardData GetCanLastClearReward(int stageNumber)
        {
            StageInfo stageInfo = GetStageInfo(stageNumber);
            if (stageInfo == null)
                return null;
            

            if (stageInfo.RecvClearReward == stageInfo.LastClearWave)
                return null;

            Dictionary<ObscuredInt, WaveClearRewardData> waveClearDatas = GetWaveClearsRewardData(stageNumber);
            if (waveClearDatas == null)
                return null;

            for (int i = stageInfo.LastClearWave; i > stageInfo.RecvClearReward; --i)
            {
                if (waveClearDatas.ContainsKey(i) == true)
                    return waveClearDatas[i];
            }

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}