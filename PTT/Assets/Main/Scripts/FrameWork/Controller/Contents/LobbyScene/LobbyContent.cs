using System.Collections.Generic;
using UnityEngine;
using GameBerry.UI;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections;

using System.Linq;

namespace GameBerry.Contents
{
    public class LobbyContent : IContent
    {
        private GameBerry.Event.SetInGameRewardPopupMsg m_setInGameRewardPopupMsg = new GameBerry.Event.SetInGameRewardPopupMsg();
        private GameBerry.Event.SetInGameRewardPopup_TitleMsg m_SetInGameRewardPopup_TitleMsg = new GameBerry.Event.SetInGameRewardPopup_TitleMsg();

        //------------------------------------------------------------------------------------
        protected override void OnLoadStart()
        {
            UIManager.Instance.Load(nameof(InGameRewardPopupDialog), null);
            UIManager.Instance.Load(nameof(InGameGambleSynergyDetailDialog), null);
            UIManager.Instance.Load(nameof(InGameGambleCardHandDialog), null);

            Managers.BattleSceneManager.Instance.InitBattleScene();
            Managers.HPMPVarianceManager.Instance.InitVariance(StaticResource.Instance.GetVarianceColorList());
            Managers.ARRRStatManager.Instance.SetBattlePower();

            StartCoroutine(CompleteFade());
        }
        //------------------------------------------------------------------------------------
        private IEnumerator CompleteFade()
        {
            //yield return new WaitForSeconds(0.3f);
            GlobalContent.DoFade(false);
            yield return new WaitForSeconds(0.5f);
            Managers.SceneManager.Instance.DeleteAppInitProcess();

            SetLoadComplete();
        }
        //------------------------------------------------------------------------------------
        protected override void OnLoadComplete()
        {
            //GlobalContent.DoFade(true);

            Managers.TimeManager.Instance.PlayCheckStageCoolTimeReward();

            TheBackEnd.TheBackEndManager.Instance.EnableSendRecvUpdateCheck();
            TheBackEnd.TheBackEndManager.Instance.OnAutoSave();
            Managers.GoodsManager.Instance.InitGoodsContent();

            ThirdPartyLog.Instance.SendLog_Game_Connect();

            Managers.PassManager.Instance.SetPassContent();
            Managers.RankManager.Instance.InitRankContent();
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            CheckRewardStage();

            IDialog.RequestDialogEnter<InGameGoodsDropDirectionDialog>();
            IDialog.RequestDialogEnter<BattleSceneDialog>();
            
            Managers.AOSBackBtnManager.Instance.InitManager();

            if (MapContainer.MaxWaveClear == 0)
                Managers.BattleSceneManager.Instance.ChangeBattleScene(Enum_Dungeon.StageScene);
            else
                Managers.BattleSceneManager.Instance.ChangeBattleScene(Enum_Dungeon.LobbyScene);
            Managers.SynergyManager.Instance.RefreshSynergyRedDot();
            

            if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Research) == true)
                Managers.ResearchManager.Instance.RefreshSynergyRedDot();

            Managers.JobManager.Instance.RefreshJobUpgradeReddot();

            DungeonData dungeonTicketData = Managers.DungeonDataManager.Instance.GetDungeonData(Enum_Dungeon.DiamondDungeon);

            if (Managers.GoodsManager.Instance.GetGoodsAmount(dungeonTicketData.EnterCostParam1) > 0)
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.Dungeon);

#if !UNITY_EDITOR
            Managers.ShopManager.Instance.CheckUnPaid();
#endif
        }
        //------------------------------------------------------------------------------------
        private void CheckRewardStage()
        {
            m_setInGameRewardPopupMsg.RewardDatas.Clear();

            if (string.IsNullOrEmpty(MapContainer.MapEnterKey) == false)
            {
                string mapkey = SecurityPlayerPrefs.GetString(MapContainer.MapKey, string.Empty);

                if (MapContainer.MapEnterKey.GetDecrypted() == mapkey)
                {
                    MapContainer.MapEnterKey = string.Empty;

                    int index = SecurityPlayerPrefs.GetInt(MapContainer.Mapidxkey, 0);
                    if (index != 0)
                    {
                        WaveRewardData _currentWaveRewardData = Managers.MapManager.Instance.GetWaveRewardData(index);

                        if (_currentWaveRewardData != null)
                        {

                            for (int i = 0; i < _currentWaveRewardData.WaveRewardRangeDatas.Count; ++i)
                            {
                                WaveRewardRangeData waveRewardRangeData = _currentWaveRewardData.WaveRewardRangeDatas[i];

                                RewardData rewardData = m_setInGameRewardPopupMsg.RewardDatas.Find(x => x.Index == waveRewardRangeData.Index);
                                if(rewardData == null)
                                {
                                    rewardData = Managers.RewardManager.Instance.GetRewardData();
                                    rewardData.V2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(waveRewardRangeData.Index);
                                    rewardData.Index = waveRewardRangeData.Index;
                                    rewardData.Amount = 0;

                                    m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);
                                }

                                rewardData.Amount += Random.Range(waveRewardRangeData.Min, waveRewardRangeData.Max + 1);
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

                                reward_type.Add(rewardData.Index);
                                before_quan.Add(Managers.GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));
                                reward_quan.Add(rewardData.Amount);

                                Managers.GoodsManager.Instance.AddGoodsAmount(rewardData);

                                after_quan.Add((int)Managers.GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));
                            }





                            List<string> _changeInfoUpdate = new List<string>();
                            _changeInfoUpdate.Add(Define.PlayerQuestInfoTable);
                            _changeInfoUpdate.Add(Define.PlayerPointTable);
                            _changeInfoUpdate.Add(Define.PlayerMapInfoTable);

                            GameBerry.Managers.QuestManager.Instance.AddMissionCount(GameBerry.V2Enum_QuestGoalType.StageChallenge, 1);

                            
                            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(_changeInfoUpdate, o =>
                            {
                                if (o.IsSuccess())
                                {
                                    SecurityPlayerPrefs.SetString(MapContainer.MapKey, string.Empty);
                                    SecurityPlayerPrefs.SetInt(MapContainer.Mapidxkey, 0);

                                    Message.Send(m_setInGameRewardPopupMsg);
                                    UI.IDialog.RequestDialogEnter<UI.InGameRewardPopupDialog>();
                                }
                            });


                            m_SetInGameRewardPopup_TitleMsg.title = Managers.LocalStringManager.Instance.GetLocalString("notice/unpaidreward");
                            Message.Send(m_SetInGameRewardPopup_TitleMsg);
                            


                        }
                    }
                }
            }

            MapContainer.MapEnterKey = string.Empty;
        }
        //------------------------------------------------------------------------------------
        private void Update()
        {
#if DEV_DEFINE
            if (Input.GetKeyUp(KeyCode.F3))
            {
                IDialog.RequestDialogEnter<GlobalCheatDialog>();
            }

            if (Input.GetKeyUp(KeyCode.F4))
            {
                IDialog.RequestDialogExit<GlobalCheatDialog>();
            }

            if (Input.GetKeyUp(KeyCode.F5))
            {
                TheBackEnd.TheBackEnd_Rank.UpdateUserScore(
                        V2Enum_RankType.Stage,
                        2,
                        Managers.RankManager.Instance.GetDetailString(),
                        () =>
                        {
                            //TheBackEnd.TheBackEnd_Rank.GetRankList(V2Enum_RankType.Stage, RefreshRankWindow);
                        });
            }

            if (Input.GetKeyUp(KeyCode.F6))
            {
                TheBackEnd.TheBackEnd_Rank.UpdateUserScore(
                        V2Enum_RankType.Power,
                        16,
                        Managers.RankManager.Instance.GetDetailString(),
                        () =>
                        {
                            //TheBackEnd.TheBackEnd_Rank.GetRankList(V2Enum_RankType.Stage, RefreshRankWindow);
                        });
            }
#endif
        }
        //------------------------------------------------------------------------------------
    }
}