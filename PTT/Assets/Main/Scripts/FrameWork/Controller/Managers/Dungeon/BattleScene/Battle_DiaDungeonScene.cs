using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBerry.Common;

namespace GameBerry
{
    public class Battle_DiaDungeonScene : BattleSceneBase
    {
        public List<FoeWaveData> FoeSpawnedList = new List<FoeWaveData>();

        private Event.PlayFreeGambleSlotMsg _playFreeGambleSlotMsg = new Event.PlayFreeGambleSlotMsg();
        private Event.ResultBattleStageMsg _resultBattleStageMsg = new Event.ResultBattleStageMsg();
        private Event.PlayARRRTutorialMsg _playARRRTutorialMsg = new Event.PlayARRRTutorialMsg();
        private Event.PlayGasTutorialMsg _playGasTutorialMsg = new Event.PlayGasTutorialMsg();
        private Event.ChangeCurrentWaveStateMsg _changeCurrentWaveStateMsg = new Event.ChangeCurrentWaveStateMsg();
        private Event.ChangeNewChellengeMapMsg _changeNewChellengeMapMsg = new Event.ChangeNewChellengeMapMsg();
        private Event.SetHpBarMsg _setHpBarMsg = new Event.SetHpBarMsg();

        

        private ObjectPoolClass<FoeWaveData> _foeWaveDatarPool = new ObjectPoolClass<FoeWaveData>();

        private bool _spawnNexus = false;

        private MonsterSetLocalTable _monsterSetLocalTable = null;

        private V2Enum_Dungeon _v2Enum_Dungeon = V2Enum_Dungeon.None;
        private int _v2Enum_Dungeon_number = 1;

        private DungeonData dungeonData;
        private DungeonModeBase m_currentDungeonData = null;

        private float _playStartTime = 0.0f;

        private int _monsterKillCount = 0;

        private float _arrrMapPosition = 0.0f;
        public float ArrrMapPosition { get { return _arrrMapPosition; } }

        CreatureController bossMonster = null;

        private CancellationTokenSource disableCancellation = new CancellationTokenSource(); //비활성화시 취소처리

        private List<string> _changeInfoUpdate = new List<string>();

        //------------------------------------------------------------------------------------
        public override void Init()
        {
            _monsterSetLocalTable = Managers.TableManager.Instance.GetTableClass<MonsterSetLocalTable>();

            bossMonster = Managers.CreatureManager.Instance.GetCreature();
            bossMonster.SendHP += RefreshBossHp;

            _changeInfoUpdate.Add(Define.PlayerQuestInfoTable);
            _changeInfoUpdate.Add(Define.PlayerPointTable);
            _changeInfoUpdate.Add(Define.PlayerDungeonInfoTable);
        }
        //------------------------------------------------------------------------------------
        protected override void OnSetBattleScene()
        {
            //UI.IDialog.RequestDialogEnter<UI.BattleScenePositionSetDialog>();

            Managers.SynergyManager.Instance.SetSynergyNextData();

            _spawnNexus = false;

            _v2Enum_Dungeon = Managers.BattleSceneManager.Instance.BattleType;

            _v2Enum_Dungeon_number = _v2Enum_Dungeon.Enum32ToInt() - 10;

            m_currentDungeonData = Managers.DungeonDataManager.Instance.GetEnterDungeonData(_v2Enum_Dungeon);
            dungeonData = Managers.DungeonDataManager.Instance.GetDungeonData(_v2Enum_Dungeon);


            float startpos = StaticResource.Instance.GetBattleModeStaticData().Stage_StartXPos;

            _arrrMapPosition = 0.0f;

            Managers.SoundManager.Instance.PlayBGM("bgm_dungeon03");

            Managers.BattleSceneManager.Instance.SetBGIndex(dungeonData.MapResource);


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

            Managers.GoodsManager.Instance.SetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.InGameGas.Enum32ToInt(), 0);
            Managers.GoodsManager.Instance.SetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.InGameDescendEnforce.Enum32ToInt(), 0);
            Managers.GoodsManager.Instance.SetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.InGameGold.Enum32ToInt(), 100);

            //_nexusController.gameObject.SetActive(false);
            //_nexusController.ActiveAggro(false);

            //_nexusController.SendState += NexusState;

            InGamePositionContainer.Instance.VisibleBattleFriendPos(true);

            bool iscanceled = disableCancellation.IsCancellationRequested;
            if (iscanceled == true)
                disableCancellation = new CancellationTokenSource();

            PlayBattleScene();
        }
        //------------------------------------------------------------------------------------
        protected override void OnPlayBattleScene()
        {
            _monsterKillCount = 0;

            //_nextSpawnTime = Time.time + _currentMapData.MapWaveDatas[_currentMapData.MapWaveDatas.Count - 1].MonsterRespawntimer;

            InGamePositionContainer.Instance.VisibleBattleFriendPos(false);

            //UI.IDialog.RequestDialogEnter<UI.InGameGambleDialog>();

            Managers.SynergyManager.Instance.AddSkillSynergy(V2Enum_ARR_SynergyType.Red, Define.SynergyDungeonCharge);
            Managers.SynergyManager.Instance.AddSkillSynergy(V2Enum_ARR_SynergyType.Blue, Define.SynergyDungeonCharge);

            if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.UnlockGoldSynergy) == true)
               Managers.SynergyManager.Instance.AddSkillSynergy(V2Enum_ARR_SynergyType.Yellow, Define.SynergyDungeonCharge);

            if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.UnlockThunderSynergy) == true)
                Managers.SynergyManager.Instance.AddSkillSynergy(V2Enum_ARR_SynergyType.White, Define.SynergyDungeonCharge);

            UI.IDialog.RequestDialogEnter<UI.InGameGambleSynergyDialog>();

            UI.IDialog.RequestDialogEnter<UI.InGameDescendContentDialog>();


            Managers.DescendManager.Instance.EquipDescendContent();

            if (m_currentDungeonData != null)
                ThirdPartyLog.Instance.SendLog_log_dungeon_start(_v2Enum_Dungeon_number, m_currentDungeonData.DungeonNumber, 0, 0, 0, 0);

            _playStartTime = Time.time;

            PlayNextWaveDelay().Forget();
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlayNextWaveDelay()
        {
            if (_myARRRControllers != null)
                _myARRRControllers.ReadyARRR();

            SpawnFoe(m_currentDungeonData.MonsterSetIndex, m_currentDungeonData.BossDungeonOverrideStats);

            await UniTask.Delay(500, false, PlayerLoopTiming.Update, disableCancellation.Token);

            if (_myARRRControllers != null)
            {
                _myARRRControllers.SkillActiveController.AddWaveStartCount(1);
                _myARRRControllers.PlayCreature();
            }

            if (bossMonster != null)
                bossMonster.PlayCreature();

            //for (int i = 0; i < _spawnCreature.Count; ++i)
            //{
            //    CreatureController creatureController = _spawnCreature[i];
            //    if (creatureController.IsDead == true)
            //    { // 죽어가고 있는 애가 섞여있다
            //        continue;
            //    }
            //    creatureController.PlayCreature();
            //}
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
            }
            else
            {
                AllForceReleaseFoeCreature();

                Managers.SoundManager.Instance.PlaySound("fx_combat_result_win_4");
            }

            Contents.GlobalContent.VisibleBufferingUI(true);

            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
            {
                await UniTask.NextFrame();
            }

            _resultBattleStageMsg.v2Enum_Dungeon = _v2Enum_Dungeon;
            _resultBattleStageMsg.Win = win;
            _resultBattleStageMsg.PlayTime = Time.time - _playStartTime;
            _resultBattleStageMsg.WaveRewardList.Clear();
            _resultBattleStageMsg.ApplyAdIncreaseRewardMode = false;
            _resultBattleStageMsg.ApplyAdIncreaseRewardValue = 0;
            _resultBattleStageMsg.currentRecord = 0;
            _resultBattleStageMsg.prevRecord = Managers.DungeonDataManager.Instance.GetDungeonRecord(_v2Enum_Dungeon).ToInt();

            if (m_currentDungeonData != null)
            {
                _resultBattleStageMsg.currentRecord = m_currentDungeonData.DungeonNumber;

                List<int> descendList = new List<int>();

                foreach (var pair in Managers.DescendManager.Instance.GetEquipSynergyEffect())
                {
                    if (pair.Value != -1)
                        descendList.Add(pair.Value);
                }

                if (win == true)
                    Managers.DungeonDataManager.Instance.SetDungeonRecord(_v2Enum_Dungeon, m_currentDungeonData.DungeonNumber);
                //ThirdPartyLog.Instance.SendLog_StageResult(_currentWaveRewardData.StageNumber, _currentWaveRewardData.WaveNumber, descendList);
                //Managers.MapManager.Instance.SetResultStageInfo(_currentWaveRewardData);

                double value = 0;

                if (win == true)
                {

                    {
                        RewardData rewardData = Managers.RewardManager.Instance.GetRewardData();
                        rewardData.V2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(m_currentDungeonData.ClearRewardParam1);
                        rewardData.Index = m_currentDungeonData.ClearRewardParam1;
                        rewardData.Amount = m_currentDungeonData.ClearRewardParam2;


                        List<int> reward_type = new List<int>();
                        List<double> before_quan = new List<double>();
                        List<double> reward_quan = new List<double>();
                        List<double> after_quan = new List<double>();


                        reward_type.Add(rewardData.Index);
                        before_quan.Add(Managers.GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));
                        reward_quan.Add(rewardData.Amount);

                        rewardData.Amount = System.Math.Truncate(rewardData.Amount);

                        Managers.GoodsManager.Instance.AddGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index, rewardData.Amount);

                        after_quan.Add(Managers.GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));

                        _resultBattleStageMsg.WaveRewardList.Add(rewardData);

                        if (dungeonData.EnterCostParam1 != -1)
                            Managers.GoodsManager.Instance.UseGoodsAmount(dungeonData.EnterCostParam1, dungeonData.EnterCostParam2);

                        {
                            ThirdPartyLog.Instance.SendLog_log_dungeon_end(_v2Enum_Dungeon_number,
                                win == true ? 2 : 1,
                                m_currentDungeonData.DungeonNumber, dungeonData.EnterCostParam1, dungeonData.EnterCostParam2, reward_type, before_quan, reward_quan, after_quan, 0);
                        }
                    }
                }
                else
                {
                    ThirdPartyLog.Instance.SendLog_log_dungeon_end(_v2Enum_Dungeon_number,
                                win == true ? 2 : 1,
                                0, 0, 0, null, null, null, null, 0);
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
        private void SpawnFoe(int setindex, List<OperatorOverrideStat> overrideStat)
        {

            MonsterSetData monsterSetData = _monsterSetLocalTable.MonsterSetDatas[setindex];

            for (int i = 0; i < monsterSetData.MonsterSetCreatureDatas.Count; ++i)
            {
                MonsterSetCreatureData monsterSetCreatureData = monsterSetData.MonsterSetCreatureDatas[i];

                Transform pos = InGamePositionContainer.Instance.GetBattleFoeSpawnPos(monsterSetCreatureData.PositionIndex + 1);

                bossMonster.SetIFFType(IFFType.IFF_Foe);
                bossMonster.SetRootTransform(pos);
                bossMonster.SetRootPos();
                bossMonster.gameObject.SetActive(true);
                bossMonster.SetCreatureData(Managers.CreatureManager.Instance.GetCreatureData(monsterSetCreatureData.MonsterIndex), 1);
                bossMonster.SetOverrideStat(overrideStat);
                bossMonster.ChangeCharacterLookAtDirection(Enum_ARR_LookDirection.Left);
                bossMonster.ReadyCreature();
            }

            //_spawnCreature.Add(bossMonster);

            _remainFoeCount = 1;
        }
        //------------------------------------------------------------------------------------
        private void RefreshBossHp(double currDamage, double currHp, double totalHp)
        {
            _setHpBarMsg.currHp = currHp;
            _setHpBarMsg.TotalHp = totalHp;

            Message.Send(_setHpBarMsg);
        }
        //------------------------------------------------------------------------------------
        public override void CallDeadCreature(CreatureController creatureController)
        {
            base.CallDeadCreature(creatureController);

            if (creatureController.IFFType == IFFType.IFF_Foe)
            {
                if (_v2Enum_Dungeon == V2Enum_Dungeon.DiamondDungeon)
                    Managers.GoodsDropDirectionManager.Instance.ShowDropIn_World(V2Enum_Goods.Point, V2Enum_Point.Dia.Enum32ToInt(), creatureController.transform.position, m_currentDungeonData.ClearRewardParam2.ToInt());

                _monsterKillCount++;

                if (_remainFoeCount == 0)
                {
                    GameEnd(true).Forget();
                }
            }
        }
        //------------------------------------------------------------------------------------
        public override void CallReleaseCreature(CreatureController creatureController)
        {
            _spawnCreature.Remove(creatureController);
            creatureController.gameObject.SetActive(false);
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

            if (bossMonster != null)
            { 
                bossMonster.ForceReleaseCreature();
                bossMonster.gameObject.SetActive(false);
            }

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