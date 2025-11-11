using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBerry.Common;

namespace GameBerry
{
    public class FoeWaveData
    {
        public float CurrentPos;
        public MapWaveData MapWaveData;
        public ObscuredInt MonsterSetLevel;
        public ObscuredInt MonsterGold;

        public int WaveStep = 0;

        public FoeWaveData NextMapData = null;
    }

    public class Battle_StageScene : BattleSceneBase
    {
        public List<FoeWaveData> FoeSpawnedList = new List<FoeWaveData>();

        private Event.PlayFreeGambleSlotMsg _playFreeGambleSlotMsg = new Event.PlayFreeGambleSlotMsg();
        private Event.ResultBattleStageMsg _resultBattleStageMsg = new Event.ResultBattleStageMsg();
        private Event.PlayARRRTutorialMsg _playARRRTutorialMsg = new Event.PlayARRRTutorialMsg();
        private Event.PlayGasTutorialMsg _playGasTutorialMsg = new Event.PlayGasTutorialMsg();
        private Event.ChangeCurrentWaveStateMsg _changeCurrentWaveStateMsg = new Event.ChangeCurrentWaveStateMsg();
        private Event.ChangeNewChellengeMapMsg _changeNewChellengeMapMsg = new Event.ChangeNewChellengeMapMsg();

        private ObjectPoolClass<FoeWaveData> _foeWaveDatarPool = new ObjectPoolClass<FoeWaveData>();

        private float _nextSpawnTime = 0.0f;

        private bool _spawnNexus = false;

        private NexusController _nexusController;

        private MonsterSetLocalTable _monsterSetLocalTable = null;

        private MapData _currentMapData;

        private FoeWaveData _currentFoeWaveData = null;
        public FoeWaveData CurrentFoeWaveData { get { return _currentFoeWaveData; } }

        private WaveRewardData _currentWaveRewardData = null;

        private float _arrrMapPosition = 0.0f;
        public float ArrrMapPosition { get { return _arrrMapPosition; } }

        private float _nextInterestTime = 0.0f;
        public float NextInterestTime { get { return _nextInterestTime; } }

        public event OnCallBack_String InterestText;

        private float _playStartTime = 0.0f;

        private int _monsterKillCount = 0;

        private CancellationTokenSource disableCancellation = new CancellationTokenSource(); //비활성화시 취소처리

        Dictionary<int, RewardData> coolreward = new Dictionary<int, RewardData>();

        private List<string> _changeInfoUpdate = new List<string>();

        private bool _adBuff_IncreaseRewardMode = false;
        private double _adBuff_IncreaseRewardValue = 0.0;


        private bool _playingSlotTutorial = false;
        private Vector3 _gasSynergyTempPos;

        private int RewardGasCount = 0;

        //------------------------------------------------------------------------------------
        public override void Init()
        {
            //ResourceLoader.Instance.Load<GameObject>("BattleScene/NexusController", o =>
            //{
            //    GameObject obj = o as GameObject;
            //    GameObject clone = Object.Instantiate(obj, Managers.BattleSceneManager.Instance.transform);

            //    _nexusController = clone.GetComponent<NexusController>();
            //    _nexusController.Init();
            //    _nexusController.SetIFFType(IFFType.IFF_Foe);
            //    clone.gameObject.SetActive(false);
            //});

            _monsterSetLocalTable = Managers.TableManager.Instance.GetTableClass<MonsterSetLocalTable>();

            _changeInfoUpdate.Add(Define.PlayerQuestInfoTable);
            _changeInfoUpdate.Add(Define.PlayerPointTable);
            _changeInfoUpdate.Add(Define.PlayerMapInfoTable);
        }
        //------------------------------------------------------------------------------------
        protected override void OnSetBattleScene()
        {
            //UI.IDialog.RequestDialogEnter<UI.BattleScenePositionSetDialog>();

            Managers.SynergyManager.Instance.SetSynergyNextData();

            _spawnNexus = false;

            _currentMapData = Managers.MapManager.Instance.GetMapData(MapContainer.MapLastEnter);

#if DEV_DEFINE
            if (UI.GlobalCheatDialog.CheatMapStage > 0)
                _currentMapData = Managers.MapManager.Instance.GetMapData(UI.GlobalCheatDialog.CheatMapStage);
#endif


            float startpos = StaticResource.Instance.GetBattleModeStaticData().Stage_StartXPos;

            Managers.GambleManager.Instance.PlayedAutoGamble = false;

            _currentFoeWaveData = null;

            FoeWaveData prevWaveData = null;

            _currentWaveRewardData = null;

            coolreward.Clear();

            for (int i = 0; i < _currentMapData.MapWaveDatas.Count; ++i)
            {
                MapWaveData mapWaveData = _currentMapData.MapWaveDatas[i];
                FoeWaveData foeWaveData = _foeWaveDatarPool.GetObject() ?? new FoeWaveData();
                foeWaveData.CurrentPos = mapWaveData.MonsterSpawnPosition;
                foeWaveData.MapWaveData = mapWaveData;
                foeWaveData.MonsterSetLevel = mapWaveData.MonsterSetLevel;
                foeWaveData.NextMapData = null;

                foeWaveData.WaveStep = i;

                if (prevWaveData != null)
                    prevWaveData.NextMapData = foeWaveData;

                prevWaveData = foeWaveData;
                FoeSpawnedList.Add(foeWaveData);

                if (_currentFoeWaveData == null)
                    _currentFoeWaveData = foeWaveData;
            }

            MapContainer.PlayingStage = _currentMapData.StageNumber;
            MapContainer.PlayingWave = 0;

            if (_currentMapData.StageNumber == 1)
                Managers.MapManager.Instance.Stage1TryCount++;
            else
                Managers.MapManager.Instance.Stage1TryCount = 0;

            _nextSpawnTime = 0;
            RewardGasCount = 0;
            _arrrMapPosition = 0.0f;

            Managers.SoundManager.Instance.PlayBGM("bgm_dungeon03");

            Managers.BattleSceneManager.Instance.SetBGIndex(_currentMapData.BackGround);

            Managers.GoodsManager.Instance.SetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.InGameGold.Enum32ToInt(), _currentMapData.StartGold);
            Managers.GoodsManager.Instance.SetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.InGameGas.Enum32ToInt(), 0);
            Managers.GoodsManager.Instance.SetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.InGameDescendEnforce.Enum32ToInt(), 0);

            Managers.RelicManager.Instance.SetInGameRelicData();
            Managers.SynergyRuneManager.Instance.SetInGameSynergyRuneData();
            Managers.GambleManager.Instance.SetInGameData();
            Managers.SynergyManager.Instance.SetInGameSynergyData();
            Managers.DescendManager.Instance.SetInGameDescendData();
            Managers.GearManager.Instance.SetInGameGearData();
            Managers.JobManager.Instance.SetInGameJobData();

            if (_myARRRControllers != null)
            {
                _myARRRControllers.AllRefreshSynergyStat();
            }

            RefreshInterest();

            //_nexusController.gameObject.SetActive(false);
            //_nexusController.ActiveAggro(false);

            //_nexusController.SendState += NexusState;

            InGamePositionContainer.Instance.VisibleBattleFriendPos(true);

            bool iscanceled = disableCancellation.IsCancellationRequested;
            if (iscanceled == true)
                disableCancellation = new CancellationTokenSource();

            _adBuff_IncreaseRewardMode = Managers.AdBuffManager.Instance.GetActiveBuffKind() == V2Enum_BuffEffectType.IncreaseReward;

            if (_adBuff_IncreaseRewardMode == false)
                _adBuff_IncreaseRewardMode = Define.IsAdBuffAlways;

            if (_adBuff_IncreaseRewardMode == true)
            {
                AdBuffActiveData adBuffActiveData = Managers.AdBuffManager.Instance.GetBuffData(V2Enum_BuffEffectType.IncreaseReward);
                _adBuff_IncreaseRewardValue = adBuffActiveData.BuffValue * Define.PerSkillEffectRecoverValue;
            }

            PlayBattleScene();
        }
        //------------------------------------------------------------------------------------
        protected override void OnPlayBattleScene()
        {
            _monsterKillCount = 0;

            //_nextSpawnTime = Time.time + _currentMapData.MapWaveDatas[_currentMapData.MapWaveDatas.Count - 1].MonsterRespawntimer;
            _nextSpawnTime = Time.time + StaticResource.Instance.GetBattleModeStaticData().Stage_WaveSpawnTurm;
            _nextInterestTime = Time.time + Define.InterestTimer;

            InGamePositionContainer.Instance.VisibleBattleFriendPos(false);

            UI.IDialog.RequestDialogEnter<UI.InGameGambleDialog>();

            if (Managers.MapManager.Instance.NeedTutotial1() == false)
                UI.IDialog.RequestDialogEnter<UI.InGameGambleSynergyDialog>();

            UI.IDialog.RequestDialogEnter<UI.InGameDescendContentDialog>();

            Managers.DescendManager.Instance.EquipDescendContent();

            if (_currentMapData != null)
                ThirdPartyLog.Instance.SendLog_Stage_Try(_currentMapData.StageNumber);

            _playStartTime = Time.time;

            PlayNextWaveDelay(_currentFoeWaveData).Forget();

            if (MapContainer.PlayingStage == Define.TestDefence_Stage)
                Contents.GlobalContent.ShowGlobalNotice(string.Format(Managers.LocalStringManager.Instance.GetLocalString("stage/mapconcept/1"), Define.TestDefence_Wave, Define.TestDefence_HpRatio * 100, Define.TestDefence_Value), 3.0f);
            else if (MapContainer.PlayingStage == Define.TestDamage_Stage)
            { 
                Contents.GlobalContent.ShowGlobalNotice(string.Format(Managers.LocalStringManager.Instance.GetLocalString("stage/mapconcept/2"), Define.TestDamage_SynergyLevel, Define.TestDamage_Value), 3.0f);
                List<SynergyEffectData> synergyEffectData = Managers.SynergyManager.Instance.GetInGameEquipSynergyEffectData(V2Enum_ARR_SynergyType.Blue);
                if (synergyEffectData.Count > Define.TestDamage_SynergyLevel - 1)
                {
                    Managers.SynergyManager.Instance.AddARRRSynergySkill(synergyEffectData[Define.TestDamage_SynergyLevel - 1], false);
                }

            }

            if (_currentMapData != null)
            {
                if (_currentMapData.StageNumber > 0)
                {

                    int used_type = Define.StaminaIndex;
                    double former_quan = Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), Define.StaminaIndex);
                    double used_quan = Define.RequiredStamina;

                    Managers.StaminaManager.Instance.UseStamina();

                    double keep_quan = Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), Define.StaminaIndex);

                    ThirdPartyLog.Instance.SendLog_log_dungeon_start(0, -1, used_type, former_quan, used_quan, keep_quan);

                    string mapkey = SecurityPlayerPrefs.Encrypt(System.DateTime.Now.ToString());
                    Managers.MapManager.Instance.SaveEnterKey(mapkey);
                    SecurityPlayerPrefs.SetString(MapContainer.MapKey, mapkey);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlayNextWaveDelay(FoeWaveData foeWaveData)
        {
            if (_myARRRControllers != null)
                _myARRRControllers.ReadyARRR();

            await UniTask.Delay(1000, false, PlayerLoopTiming.Update, disableCancellation.Token);
            //await UniTask.Delay(500, false, PlayerLoopTiming.Update, disableCancellation.Token);

            if (_myARRRControllers != null)
                _myARRRControllers.DirectionRunARRR();

            if (Managers.MapManager.Instance.NeedTutotial1() == true)
            {
                while (_playingSlotTutorial == true || Managers.GuideInteractorManager.Instance.PlayGasSynergyTutorial == true)
                {
                    await UniTask.Yield(disableCancellation.Token);
                }
            }


            float starttime = Time.time;
            float duration = StaticResource.Instance.GetBattleModeStaticData().Stage_WaveSpawnTurm;
            float endtime = Time.time + duration;

            float startArrrPos = _arrrMapPosition;
            float goalArrrPos = foeWaveData.CurrentPos;
            float gab = goalArrrPos - startArrrPos;

            bool spawnMonster = false;


            while (Time.time < endtime)
            {
                float ratio = (Time.time - starttime) / duration;
                _arrrMapPosition = startArrrPos + (gab * ratio);

                if (spawnMonster == false)
                {
                    if (ratio > StaticResource.Instance.GetBattleModeStaticData().Stage_WaveSpawnRatio)
                    {
                        SpawnFoe(foeWaveData.MapWaveData.MonsterSetIndex, foeWaveData.MonsterSetLevel, foeWaveData.MapWaveData.AddStat);

                        foeWaveData.MonsterGold = (int)(foeWaveData.MapWaveData.MonsterGold / (double)_remainFoeCount);

                        spawnMonster = true;
                    }
                }

                await UniTask.Yield(disableCancellation.Token);
            }

            //_arrrMapPosition = startArrrPos + gab;

            //if (spawnMonster == false)
            //{
            //    //if (ratio > StaticResource.Instance.GetBattleModeStaticData().Stage_WaveSpawnRatio)
            //    {
            //        SpawnFoe(foeWaveData.MapWaveData.MonsterSetIndex, foeWaveData.MonsterSetLevel, foeWaveData.MapWaveData.AddStat);

            //        foeWaveData.MonsterGold = (int)(foeWaveData.MapWaveData.MonsterGold / (double)_remainFoeCount);

            //        spawnMonster = true;
            //    }
            //}

            if (Managers.MapManager.Instance.NeedTutotial1() == true)
            {
                if (foeWaveData.WaveStep == 0)
                    PlayCardGamblePlayTutorial(3000).Forget();
                else if (foeWaveData.WaveStep == 1 && Managers.GambleManager.Instance.GetGambleActionCount(V2Enum_ARR_GambleType.Card) == 1)
                {
                    PlayCardGamblePlayTutorial2().Forget();
                }
                else if (foeWaveData.WaveStep == 4)
                {
                    //Message.Send(new Event.PlaySynergyCombineTutorialMsg());
                    PlaySpeedUpTutorial().Forget();
                }

            }

            //Managers.GambleManager.Instance.PlayAutoGamble();

            _changeCurrentWaveStateMsg.MapWaveData = foeWaveData.MapWaveData;
            _changeCurrentWaveStateMsg.MaxWave = _currentMapData.MaxWave;
            Message.Send(_changeCurrentWaveStateMsg);

            if (_myARRRControllers != null)
            {
                _myARRRControllers.SkillActiveController.AddWaveStartCount(1);
                _myARRRControllers.PlayCreature();
            }

            for (int i = 0; i < _spawnCreature.Count; ++i)
            {
                CreatureController creatureController = _spawnCreature[i];
                if (creatureController.IsDead == true)
                { // 죽어가고 있는 애가 섞여있다
                    continue;
                }
                creatureController.PlayCreature();
            }


            MapContainer.PlayingWave = foeWaveData.WaveStep;
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlayCardGamblePlayTutorial(int delaytime)
        {
            await UniTask.Delay(delaytime, false, PlayerLoopTiming.Update, disableCancellation.Token);

            Managers.GoodsManager.Instance.AddGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.InGameGold.Enum32ToInt(), 30);

            _playARRRTutorialMsg.v2Enum_ARR_GambleType = V2Enum_EventType.TutoGambleCard;
            Message.Send(_playARRRTutorialMsg);
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlayCardGamblePlayTutorial2()
        {
            await UniTask.Delay(1000, false, PlayerLoopTiming.Update, disableCancellation.Token);

            if (Managers.GambleManager.Instance.GetGambleActionCount(V2Enum_ARR_GambleType.Card) == 1)
            {
                //Managers.GoodsManager.Instance.AddGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.InGameGold.Enum32ToInt(), 20);

                _playARRRTutorialMsg.v2Enum_ARR_GambleType = V2Enum_EventType.TutoGambleCard;
                Message.Send(_playARRRTutorialMsg);
            }
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlayGasTutorial()
        {
            Message.Send(new GameBerry.Event.PlayForceEndGambleCardMsg());

            UI.IDialog.RequestDialogExit<UI.InGameGambleSynergyDialog>();
            UI.IDialog.RequestDialogExit<UI.InGameGambleSynergyDetailDialog>();

            _playingSlotTutorial = true;

            Message.Send(new GameBerry.Event.PlaySlotTutorialMsg());

            await UniTask.Delay(6000, false, PlayerLoopTiming.Update, disableCancellation.Token);

            //string localstring = Managers.LocalStringManager.Instance.GetLocalString("ingame/getgas");

            //string noticemsg = string.Format(localstring, 1);

            //Contents.GlobalContent.ShowGlobalNotice(noticemsg);

            Managers.GoodsDropDirectionManager.Instance.ShowDropIn_World(V2Enum_Goods.Point, V2Enum_Point.InGameGas.Enum32ToInt(), _myARRRControllers.transform.position, 1);

            //await UniTask.Delay(1000, false, PlayerLoopTiming.Update, disableCancellation.Token);

            _playingSlotTutorial = false;


            Managers.GuideInteractorManager.Instance.PlayGasSynergyTutorial = true;

            Message.Send(_playGasTutorialMsg);
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlaySpeedUpTutorial()
        {
            await UniTask.Delay(2000, false, PlayerLoopTiming.Update, disableCancellation.Token);

            while (Managers.GuideInteractorManager.Instance.PlayJokerTutorial == true)
            {
                Debug.Log("waitJoker");
                await UniTask.NextFrame(PlayerLoopTiming.Update, disableCancellation.Token);
            }

            if (_isPlay == false)
                return;

            Managers.GuideInteractorManager.Instance.PlayGameSpeedTutorial = true;

            _playARRRTutorialMsg.v2Enum_ARR_GambleType = V2Enum_EventType.SpeedUp;
            Message.Send(_playARRRTutorialMsg);
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlayFreeSlot()
        {
            await UniTask.Delay(1500, false, PlayerLoopTiming.Update, disableCancellation.Token);
            Message.Send(_playFreeGambleSlotMsg);
        }
        //------------------------------------------------------------------------------------
        private void RefreshInterest()
        {
            if (_myARRRControllers != null)
            {
                _myARRRControllers.RefreshInterestAmount(Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.InGameGold.Enum32ToInt()));
            }
        }
        //------------------------------------------------------------------------------------
        private async UniTask GameEnd(bool win)
        {
            if (_isPlay == false)
                return;

            Managers.BattleSceneManager.Instance.ChangeTimeScale(V2Enum_ARR_BattleSpeed.x1);
            _isPlay = false;

            UI.IDialog.RequestDialogExit<UI.InGameGambleDialog>();
            UI.IDialog.RequestDialogExit<UI.InGameGambleSynergyDialog>();
            UI.IDialog.RequestDialogExit<UI.InGameGambleSynergyDetailDialog>();
            UI.IDialog.RequestDialogExit<UI.InGameGambleCardHandDialog>();
            UI.IDialog.RequestDialogExit<UI.InGameDescendContentDialog>();

            if (win == false)
            {
                Managers.SoundManager.Instance.PlaySound("fx_combat_result_lose_1");
                if (_currentFoeWaveData != null && _currentFoeWaveData.MapWaveData != null)
                    MapContainer.LastFailWave = _currentFoeWaveData.MapWaveData.TotalNumber;

                if (_currentMapData != null)
                {
                    if (_currentMapData.StageNumber == 1)
                        Managers.GuideInteractorManager.Instance.ShowNeedGearNotice = true;
                }
            }
            else
            {
                MapContainer.LastFailWave = -1;

                AllForceReleaseFoeCreature();

                Managers.SoundManager.Instance.PlaySound("fx_combat_result_win_4");
                ThirdPartyLog.Instance.SendLog_StageClear(_currentWaveRewardData.StageNumber, _currentWaveRewardData.WaveNumber);
            }

            Contents.GlobalContent.VisibleBufferingUI(true);

            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
            {
                await UniTask.NextFrame();
            }

            _resultBattleStageMsg.v2Enum_Dungeon = V2Enum_Dungeon.StageScene;
            _resultBattleStageMsg.Win = win;
            _resultBattleStageMsg.PlayTime = Time.time - _playStartTime;
            _resultBattleStageMsg.WaveRewardList.Clear();
            _resultBattleStageMsg.ApplyAdIncreaseRewardMode = _adBuff_IncreaseRewardMode;
            _resultBattleStageMsg.ApplyAdIncreaseRewardValue = _adBuff_IncreaseRewardValue;
            _resultBattleStageMsg.currentRecord = 0;
            _resultBattleStageMsg.prevRecord = MapContainer.MaxWaveClear;

            if (_currentWaveRewardData != null)
            {
                _resultBattleStageMsg.currentRecord = (_currentWaveRewardData.StageNumber * 100) + _currentWaveRewardData.WaveNumber;
                int prev = MapContainer.MapMaxClear;

                List<int> descendList = new List<int>();

                foreach (var pair in Managers.DescendManager.Instance.GetEquipSynergyEffect())
                {
                    if (pair.Value != -1)
                        descendList.Add(pair.Value);
                }

                ThirdPartyLog.Instance.SendLog_StageResult(_currentWaveRewardData.StageNumber, _currentWaveRewardData.WaveNumber, descendList);
                Managers.MapManager.Instance.SetResultStageInfo(_currentWaveRewardData);

                if (prev != MapContainer.MapMaxClear)
                    Message.Send(_changeNewChellengeMapMsg);

                double value = _adBuff_IncreaseRewardValue;

                for (int i = 0; i < _currentWaveRewardData.WaveRewardRangeDatas.Count; ++i)
                {
                    WaveRewardRangeData waveRewardRangeData = _currentWaveRewardData.WaveRewardRangeDatas[i];

                    RewardData rewardData = null;
                    if (coolreward.ContainsKey(waveRewardRangeData.Index) == true)
                        rewardData = coolreward[waveRewardRangeData.Index];
                    else
                    {
                        rewardData = Managers.RewardManager.Instance.GetRewardData();
                        rewardData.V2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(waveRewardRangeData.Index);
                        rewardData.Index = waveRewardRangeData.Index;
                        rewardData.Amount = 0;

                        coolreward.Add(rewardData.Index, rewardData);
                    }

                    double amount = Random.Range(waveRewardRangeData.Min, waveRewardRangeData.Max + 1);

                    amount += amount * _adBuff_IncreaseRewardValue;

                    amount += amount * Managers.ResearchManager.Instance.GetResearchValues(V2Enum_ResearchType.StageRewardIncrease);

                    rewardData.Amount += amount;
                }

                List<int> reward_type = new List<int>();
                List<double> before_quan = new List<double>();
                List<double> reward_quan = new List<double>();
                List<double> after_quan = new List<double>();

                foreach (var pair in coolreward)
                {
                    RewardData rewardData = pair.Value;
                    rewardData.Amount = System.Math.Truncate(rewardData.Amount);

                    reward_type.Add(rewardData.Index);
                    before_quan.Add(Managers.GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));
                    reward_quan.Add(rewardData.Amount);

                    Managers.GoodsManager.Instance.AddGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index, rewardData.Amount);

                    after_quan.Add(Managers.GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));

                    _resultBattleStageMsg.WaveRewardList.Add(rewardData);
                }

                if (_currentMapData != null)
                {
                    MapWaveData mapWaveData = _currentMapData.MapWaveDatas.Find(x => x.WaveNumber == _currentWaveRewardData.WaveNumber);
                    if (mapWaveData != null)
                    { 
                        ThirdPartyLog.Instance.SendLog_log_dungeon_end(0, 
                            win == true ? 2 : 1, mapWaveData.TotalNumber,
                            0, 0,
                            reward_type, before_quan, reward_quan, after_quan, Managers.GambleManager.Instance.SynergyGambleAuto == true ? 1 : 0);
                    }
                }
            }

            GameBerry.Managers.QuestManager.Instance.AddMissionCount(GameBerry.V2Enum_QuestGoalType.StageChallenge, 1);
            GameBerry.Managers.QuestManager.Instance.AddMissionCount(GameBerry.V2Enum_QuestGoalType.MonterKillCount, _monsterKillCount);
            GameBerry.Managers.QuestManager.Instance.AddMissionCount(GameBerry.V2Enum_QuestGoalType.CardGambleCount, Managers.GambleManager.Instance.GetGambleActionCount(V2Enum_ARR_GambleType.Card));
            GameBerry.Managers.QuestManager.Instance.AddMissionCount(GameBerry.V2Enum_QuestGoalType.SynergyCombineClear, Managers.SynergyManager.Instance.GetCombineClearCount());

            GameBerry.Managers.PassManager.Instance.AddMonsterKillCount(_monsterKillCount);

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(_changeInfoUpdate, o =>
            {
                if (o.IsSuccess())
                {
                    Contents.GlobalContent.VisibleBufferingUI(false);

                    SecurityPlayerPrefs.SetString(MapContainer.MapKey, string.Empty);
                    SecurityPlayerPrefs.SetInt(MapContainer.Mapidxkey, 0);

                    Message.Send(_resultBattleStageMsg);
                }
                else
                {
                    Contents.GlobalContent.ShowPopup_Ok(
    Managers.LocalStringManager.Instance.GetLocalString("common/popUp/title"),
    Managers.LocalStringManager.Instance.GetLocalString("connecterror/alramdesc"),
    Managers.SceneManager.Instance.OnApplicationQuit);

                }
            });
        }
        //------------------------------------------------------------------------------------
        public void PlayDoubleReward()
        {
            Managers.MapManager.Instance.DoDoubleReward(coolreward);
        }
        //------------------------------------------------------------------------------------
        private void SpawnFoe(int setindex, int level, Dictionary<V2Enum_Stat, ObscuredDouble> addDefaultStat)
        {
            if (_monsterSetLocalTable.MonsterSetDatas.ContainsKey(setindex) == false)
                return;

            MonsterSetData monsterSetData = _monsterSetLocalTable.MonsterSetDatas[setindex];

            for (int i = 0; i < monsterSetData.MonsterSetCreatureDatas.Count; ++i)
            {
                MonsterSetCreatureData monsterSetCreatureData = monsterSetData.MonsterSetCreatureDatas[i];

                Transform pos = InGamePositionContainer.Instance.GetBattleFoeSpawnPos(monsterSetCreatureData.PositionIndex + 1);

                CreatureController creatureController = SpawnCreature(
                    Managers.CreatureManager.Instance.GetCreatureData(monsterSetCreatureData.MonsterIndex),
                    level + monsterSetCreatureData.MonsterCorrectLevel, pos, IFFType.IFF_Foe, Enum_ARR_LookDirection.Left
                    , addDefaultStat);
                creatureController.ReadyCreature();
                //creatureController.PlayCreature();
            }

            _remainFoeCount = monsterSetData.MonsterSetCreatureDatas.Count;
        }
        //------------------------------------------------------------------------------------
        private V2Enum_ARR_GambleType currentGuide = V2Enum_ARR_GambleType.Max;
        private V2Enum_ARR_GambleType completeGuilde = V2Enum_ARR_GambleType.Max;
        //------------------------------------------------------------------------------------
        public override void CallDeadCreature(CreatureController creatureController)
        {
            base.CallDeadCreature(creatureController);

            if (creatureController.IFFType == IFFType.IFF_Foe)
            {
                Managers.GoodsDropDirectionManager.Instance.ShowDropIn_World(V2Enum_Goods.Point, V2Enum_Point.InGameGold.Enum32ToInt(), creatureController.transform.position, _currentFoeWaveData.MonsterGold);
                Managers.GoodsManager.Instance.AddGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.InGameGold.Enum32ToInt(), _currentFoeWaveData.MonsterGold);

                //if (UnityEngine.Random.Range(0.0f, 10000.0f) < Define.MonsterDropItemProb)
                //{
                //    Managers.GoodsDropDirectionManager.Instance.ShowDropIn_World(V2Enum_Goods.Point, Define.FakeDropIndex, creatureController.transform.position, 1);
                //    Managers.GoodsManager.Instance.AddGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), Define.FakeDropIndex, Define.MonsterDropItemValue);
                //}

                _monsterKillCount++;

                RefreshInterest();

                if (_remainFoeCount == 0)
                {
                    if (MyARRRControllers.IsDead == true)
                        return;

                    if (_currentFoeWaveData == null)
                    {
                        GameEnd(true).Forget();
                    }
                    else
                    {
                        V2Enum_ARR_RoomType v2Enum_ARR_RoomType = _currentFoeWaveData.MapWaveData.RoomType;

                        _currentWaveRewardData = Managers.MapManager.Instance.GetWaveRewardData(_currentFoeWaveData.MapWaveData.StageNumber, _currentFoeWaveData.MapWaveData.WaveNumber);

                        if (_currentWaveRewardData != null)
                            SecurityPlayerPrefs.SetInt(MapContainer.Mapidxkey, _currentWaveRewardData.Index.GetDecrypted());

                        double gasAmount = _currentFoeWaveData.MapWaveData.GasReward;
                        int clearWave = _currentFoeWaveData.MapWaveData.WaveNumber;
                        _currentFoeWaveData = _currentFoeWaveData.NextMapData;
                        if (_currentFoeWaveData == null)
                        {
                            GameEnd(true).Forget();
                        }
                        else
                        {
                            Managers.GoodsManager.Instance.AddGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.InGameGold.Enum32ToInt(), _currentFoeWaveData.MapWaveData.WaveClearGold);

                            if (_myARRRControllers != null)
                                _myARRRControllers.SkillActiveController.AddWaveClearCount(1);

                            if (v2Enum_ARR_RoomType == V2Enum_ARR_RoomType.Slot)
                                PlayFreeSlot().Forget();

                            Managers.GoodsManager.Instance.AddGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.InGameGas.Enum32ToInt(), gasAmount);

                            double researchgold = Managers.ResearchManager.Instance.GetResearchValues(V2Enum_ResearchType.WaveRewardIncrease);
                            if (researchgold > 0)
                            {
                                Managers.GoodsManager.Instance.AddGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.InGameGold.Enum32ToInt(), researchgold);

                                Managers.SynergyManager.Instance.ShowInterestText(string.Format("+{0}({1})", researchgold
                                , Managers.ResearchManager.Instance.GetEffectTitle(V2Enum_ResearchType.WaveRewardIncrease)));
                            }
                            
                            if (gasAmount > 0)
                            {
                                if (_myARRRControllers != null)
                                {
                                    _myARRRControllers.SkillActiveController.AddGainGasRewardCount(1);
                                }

                                RewardGasCount++;

                                string localstring = Managers.LocalStringManager.Instance.GetLocalString("ingame/getgas");

                                string noticemsg = string.Format(localstring, gasAmount.ToInt());


                                //if (RewardGasCount == 1 && _currentMapData != null)
                                //{
                                //    if (_currentMapData.StageNumber == 1)
                                //    {
                                //        StageInfo stageInfo = Managers.MapManager.Instance.GetStageInfo(_currentMapData);
                                //        if (stageInfo == null)
                                //            Managers.GuideInteractorManager.Instance.StartGuideInteractor(V2Enum_EventType.GasSynergy);
                                //        else
                                //        {
                                //            if (stageInfo.LastClearWave < clearWave)
                                //                Managers.GuideInteractorManager.Instance.StartGuideInteractor(V2Enum_EventType.GasSynergy);
                                //        }

                                //    }
                                //}




                                if (Managers.MapManager.Instance.NeedTutotial1() == true)
                                {
                                    PlayGasTutorial().Forget();
                                }
                                else
                                { 
                                    Managers.GoodsDropDirectionManager.Instance.ShowDropIn_World(V2Enum_Goods.Point, V2Enum_Point.InGameGas.Enum32ToInt(), creatureController.transform.position, 1);
                                    Contents.GlobalContent.ShowGlobalNotice(noticemsg);
                                }
                            }

                            RefreshInterest();

                            PlayNextWaveDelay(_currentFoeWaveData).Forget();
                        }
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public override void CallDeadARRR(ARRRController creatureController)
        {
            if (_myARRRControllers == creatureController)
            {
                GameEnd(false).Forget();
            }
        }
        //------------------------------------------------------------------------------------
        protected override void OnReleaseBattleScene()
        {
            disableCancellation.Cancel();
            disableCancellation.Dispose();
            Managers.GoodsManager.Instance.SetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.InGameGold.Enum32ToInt(), 0);
            
            for (int i = 0; i < FoeSpawnedList.Count; ++i)
            {
                _foeWaveDatarPool.PoolObject(FoeSpawnedList[i]);
            }

            FoeSpawnedList.Clear();

            //_nexusController.ActiveAggro(false);
            //_nexusController.SendState -= NexusState;
            //_nexusController.gameObject.SetActive(false);
            
            _spawnNexus = false;

            InGamePositionContainer.Instance.VisibleBattleFriendPos(false);

            //UI.IDialog.RequestDialogExit<UI.BattleScenePositionSetDialog>();
            UI.IDialog.RequestDialogExit<UI.InGameGambleDialog>();
            UI.IDialog.RequestDialogExit<UI.InGameGambleSynergyDialog>();
            UI.IDialog.RequestDialogExit<UI.InGameGambleSynergyDetailDialog>();
            UI.IDialog.RequestDialogExit<UI.InGameGambleCardHandDialog>();
            UI.IDialog.RequestDialogExit<UI.InGameGamble_SlotDialog>();
            UI.IDialog.RequestDialogExit<UI.InGameSkillViewDialog>();
            UI.IDialog.RequestDialogExit<UI.InGameDescendContentDialog>();


            Managers.GambleManager.Instance.ResetGambleState();
            Managers.SynergyManager.Instance.ResetSynergyState();
            Managers.DescendManager.Instance.ResetDescendState();
            Managers.SynergyRuneManager.Instance.ResetSynergyRuneState();

            MapContainer.PlayingStage = -1;
            MapContainer.PlayingWave = -1;
        }
        //------------------------------------------------------------------------------------
    }
}