using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.UI;

namespace GameBerry.Managers
{
    public class GuideQuestManager : MonoSingleton<GuideQuestManager>
    {
        private GuideQuestLocalTable m_guideQuestLocalTable = null;

        private List<GuideQuestData> m_guideQuestDatas = null;
        private GuideQuestData m_currentGuideQuest = null;
        private GuideTutorialData m_currentGuideTutorialData = null;
        private int m_currentGuideValue = 0;

        private Event.SetInGameRewardPopupMsg m_setInGameRewardPopupMsg = new Event.SetInGameRewardPopupMsg();
        private Event.RefreshGuideQuestMsg m_refreshGuideQuestMsg = new Event.RefreshGuideQuestMsg();
        private Event.SetGuideDialogMsg m_setGuideDialogMsg = new Event.SetGuideDialogMsg();

        private List<string> m_changeInfoWeaponUpdate = new List<string>();
        private List<string> m_changeInfoPointUpdate = new List<string>();
        private List<string> m_changeInfoSkillUpdate = new List<string>();
        private List<string> m_changeInfoAllyUpdate = new List<string>();
        private List<string> m_changeInfoJewelryUpdate = new List<string>();


        public int LimitStage { get { return m_limitStage; } }
        private int m_limitStage = -1;

        public int UnLockStageQuestOrder { get { return m_unLockStageQuestOrder; } }
        private int m_unLockStageQuestOrder = -1;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            m_changeInfoWeaponUpdate.Add(Define.PlayerQuestInfoTable);
            m_changeInfoWeaponUpdate.Add(Define.PlayerGearTable);

            m_changeInfoPointUpdate.Add(Define.PlayerQuestInfoTable);
            m_changeInfoPointUpdate.Add(Define.PlayerPointTable);

            m_changeInfoSkillUpdate.Add(Define.PlayerQuestInfoTable);
            m_changeInfoSkillUpdate.Add(Define.PlayerSkillInfoTable);

            m_changeInfoAllyUpdate.Add(Define.PlayerQuestInfoTable);
            m_changeInfoAllyUpdate.Add(Define.PlayerAllyInfoTable);

            m_changeInfoJewelryUpdate.Add(Define.PlayerQuestInfoTable);
            m_changeInfoJewelryUpdate.Add(Define.PlayerAllyJewelryInfoTable);

            m_guideQuestLocalTable = TableManager.Instance.GetTableClass<GuideQuestLocalTable>();
            m_guideQuestDatas = m_guideQuestLocalTable.GetDatas();
        }
        //------------------------------------------------------------------------------------
        public void InitGuideQuestContent()
        {
            SelectCurrentGuide();
        }
        //------------------------------------------------------------------------------------
        private void SelectCurrentGuide()
        {
            if (m_guideQuestDatas == null)
                return;

            m_currentGuideQuest = null;

            for (int i = 0; i < m_guideQuestDatas.Count; ++i)
            {
                if (GuideQuestContainer.ClearGuideQuestOrder < m_guideQuestDatas[i].QuestOrder)
                {
                    m_currentGuideQuest = m_guideQuestDatas[i];
                    break;
                }
            }

            if (m_currentGuideQuest != null)
            {
                GuideQuestContainer.CurrentGuideQuestOrder = m_currentGuideQuest.Index.GetDecrypted().ToString();

                if (m_limitStage != m_currentGuideQuest.MaxApproachStage.GetDecrypted())
                {
                    m_limitStage = m_currentGuideQuest.MaxApproachStage.GetDecrypted();
                    if (m_limitStage != -1)
                    {
                        GuideQuestData guideQuestData = m_guideQuestDatas.Find(x => x.MaxApproachStage.GetDecrypted() > m_limitStage || x.MaxApproachStage.GetDecrypted() == -1);
                        guideQuestData = GetPrevGuide(guideQuestData);
                        if (guideQuestData != null)
                            m_unLockStageQuestOrder = guideQuestData.QuestOrder.GetDecrypted();
                    }
                }
            }

            m_currentGuideValue = 0;
        }
        //------------------------------------------------------------------------------------
        public void PlayGuideQuest()
        {
            if (m_currentGuideQuest == null)
            {
                IDialog.RequestDialogExit<InGameGuideQuestDialog>();
                return;
            }

            m_currentGuideValue = 0;

            m_refreshGuideQuestMsg.guideQuestData = m_currentGuideQuest;
            Message.Send(m_refreshGuideQuestMsg);

            IDialog.RequestDialogEnter<InGameGuideQuestDialog>();

            m_currentGuideTutorialData = m_guideQuestLocalTable.GetTutorialData(m_currentGuideQuest.Index);

            if (m_currentGuideTutorialData != null)
            {
                IDialog.RequestDialogEnter<InGameGuideNPCNoticeDialog>();
                m_setGuideDialogMsg.guideTutorialData = m_currentGuideTutorialData;
                Message.Send(m_setGuideDialogMsg);
            }

            CheckEventType(m_currentGuideQuest.QuestType);

            if (m_currentGuideTutorialData != null
                && m_currentGuideTutorialData.IsFingerGuide == 1
                && IsClearGuideQuest(m_currentGuideQuest) == false)
            {
                GuideInteractorManager.Instance.PlayGuide(m_currentGuideQuest);
            }
        }
        //------------------------------------------------------------------------------------
        public void GoShortCut(GuideQuestData guideQuestData)
        {
            if (m_currentGuideTutorialData != null)
            {
                if (m_currentGuideTutorialData.IsFingerGuide == 1)
                    return;
            }

            switch (guideQuestData.QuestType)
            {
                //case V2Enum_EventType.CharacterAttackTraining:
                //case V2Enum_EventType.CharacterHpTraining:
                //case V2Enum_EventType.AttackSpeedTraining:
                //case V2Enum_EventType.CriticalChanceTraining:

                //case V2Enum_EventType.CriticalDamageTraining:
                //case V2Enum_EventType.AttackIncreaseTraining:
                //case V2Enum_EventType.HpIncreaseTraining:
                //case V2Enum_EventType.FinalAttackIncreaseTraining:


                //case V2Enum_EventType.WeaponEquip:
                //case V2Enum_EventType.WeaponLevelUp:
                //case V2Enum_EventType.AllArmorsEquip:
                //case V2Enum_EventType.AllArmorsLevelUp:
                //case V2Enum_EventType.AccessoryEquip:
                //case V2Enum_EventType.AccessoryLevelUp:
                //case V2Enum_EventType.HelmetEquip:
                //case V2Enum_EventType.ArmorEquip:
                //case V2Enum_EventType.GlovesEquip:
                //case V2Enum_EventType.ShoesEquip:
                //case V2Enum_EventType.HelmetLevelUp:
                //case V2Enum_EventType.ArmorLevelUp:
                //case V2Enum_EventType.GlovesLevelUp:
                //case V2Enum_EventType.ShoesLevelUp:
                //    {
                //        Managers.UIQuickLinkManager.Instance.ShowQuickLink(ContentDetailList.CharacterGear);
                //        break;
                //    }
                //case V2Enum_EventType.ResearchLevelUp:
                //    {
                //        Managers.UIQuickLinkManager.Instance.ShowQuickLink(ContentDetailList.LobbyResearch);
                //        break;
                //    }
            }
        }
        //------------------------------------------------------------------------------------
        public bool CompleteGuideQuest(GuideQuestData guideQuestData)
        {
            if (guideQuestData == null)
                return false;

            if (m_currentGuideQuest != guideQuestData)
                return false;

            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return false;

            GuideQuestContainer.ClearGuideQuestOrder = guideQuestData.QuestOrder.GetDecrypted();

            double beforeAmount = GoodsManager.Instance.GetGoodsAmount(guideQuestData.ClearRewardType.Enum32ToInt(), guideQuestData.ClearRewardParam1.GetDecrypted());

            GoodsManager.Instance.AddGoodsAmount(guideQuestData.ClearRewardType.Enum32ToInt(), guideQuestData.ClearRewardParam1.GetDecrypted(), guideQuestData.ClearRewardParam2.GetDecrypted());

            if (guideQuestData.ClearRewardType == V2Enum_Goods.Point)
            {
                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoPointUpdate, null);
            }
            else if (guideQuestData.ClearRewardType == V2Enum_Goods.Gear)
            {
                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoWeaponUpdate, null);
            }
            else if (guideQuestData.ClearRewardType == V2Enum_Goods.CharacterSkill)
            {
                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoSkillUpdate, null);
            }
            else if (guideQuestData.ClearRewardType == V2Enum_Goods.Ally)
            {
                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoAllyUpdate, null);
            }
            else
            {
                TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerQuestInfoTable);
                TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerPointTable);

                TheBackEnd.TheBackEndManager.Instance.SendUpdateWaitData(true);
            }


            if (guideQuestData.QuestType == V2Enum_EventType.BossChallenge)
            {
                if (GuideQuestContainer.ContainBossChallengeStep.ContainsKey(guideQuestData.QuestParam.GetDecrypted()) == true)
                    GuideQuestContainer.ContainBossChallengeStep[guideQuestData.QuestParam.GetDecrypted()] = true;
            }

            if (m_currentGuideTutorialData != null)
            { 
                GuideInteractorManager.Instance.EndGuideQuest();
                m_currentGuideTutorialData = null;
            }

            int idx = guideQuestData.Index;

            SelectCurrentGuide();

            ThirdPartyLog.Instance.SendGuide_CompletionEvent(idx, guideQuestData.ClearRewardParam1.GetDecrypted(), (int)beforeAmount, (int)guideQuestData.ClearRewardParam2.GetDecrypted(), (int)(guideQuestData.ClearRewardParam2.GetDecrypted() + beforeAmount));

            PlayGuideQuest();

            return true;
        }
        //------------------------------------------------------------------------------------
        public GuideQuestData GetCurrentGuide()
        {
            return m_currentGuideQuest;
        }
        //------------------------------------------------------------------------------------
        public GuideQuestData GetPrevGuide(GuideQuestData guideQuestData)
        {
            int index = m_guideQuestDatas.IndexOf(guideQuestData);

            if (index == -1 || index == 0)
                return null;

            return m_guideQuestDatas[index - 1];
        }
        //------------------------------------------------------------------------------------
        public string GetQuestNameLocalString(GuideQuestData guideQuestData)
        {
            string localstring = string.Empty;

            switch (guideQuestData.QuestType)
            {
                case V2Enum_EventType.StageClear:
                    {
                        //StageData stageData = DungeonDataManager.Instance.GetStageData(guideQuestData.QuestParam.GetDecrypted());

                        //localstring = string.Format(LocalStringManager.Instance.GetLocalString(guideQuestData.NameLocalStringKey), stageData.ChapterNumber, guideQuestData.QuestParam.GetDecrypted() - (50 * (stageData.ChapterNumber - 1)));

                        localstring = "StageClear";

                        break;
                    }
                case V2Enum_EventType.BossChallenge:
                    {
                        //StageData stageData = DungeonDataManager.Instance.GetStageData(guideQuestData.QuestParam.GetDecrypted());
                        //localstring = string.Format("{0}-{1} {2}", stageData.ChapterNumber, guideQuestData.QuestParam.GetDecrypted() - (50 * (stageData.ChapterNumber - 1)), LocalStringManager.Instance.GetLocalString(guideQuestData.NameLocalStringKey));

                        //localstring = string.Format(LocalStringManager.Instance.GetLocalString(guideQuestData.NameLocalStringKey), stageData.ChapterNumber, guideQuestData.QuestParam - (50 * (stageData.ChapterNumber - 1)));

                        localstring = "BossChallenge";

                        break;
                    }
                default:
                    {
                        localstring = string.Format(LocalStringManager.Instance.GetLocalString(guideQuestData.NameLocalStringKey), guideQuestData.QuestParam.GetDecrypted());
                        break;
                    }
            }

            return localstring;
        }
        //------------------------------------------------------------------------------------
        public bool IsVisibleProcessUI(GuideQuestData guideQuestData)
        {
            switch (guideQuestData.QuestType)
            {
                case V2Enum_EventType.StageClear:
                    {
                        return false;
                    }
                case V2Enum_EventType.BossChallenge:
                    {
                        return false;
                    }
                default:
                    {
                        if (guideQuestData.QuestParam.GetDecrypted() <= 1)
                            return false;

                        return true;
                    }
            }

            return false;
        }
        //------------------------------------------------------------------------------------
        public int GetCurrentProcess()
        {
            return m_currentGuideValue;
        }
        //------------------------------------------------------------------------------------
        public bool IsClearGuideQuest(GuideQuestData guideQuestData)
        {
            return guideQuestData.QuestParam.GetDecrypted() <= m_currentGuideValue;
        }
        //------------------------------------------------------------------------------------
        public void CheckEventType(V2Enum_EventType v2Enum_EventType)
        {
            if (m_currentGuideQuest == null)
                return;

            if (m_currentGuideQuest.QuestType != v2Enum_EventType)
                return;

            int eventValue = 0;

            switch (v2Enum_EventType)
            {
                case V2Enum_EventType.AllySummon:
                    {
                        eventValue = (int)SummonManager.Instance.GetAccumCount(V2Enum_SummonType.SummonRelic);
                        break;
                    }

                case V2Enum_EventType.AllyCompose:
                    {
                        
                        break;
                    }

                case V2Enum_EventType.JewelryEquip:
                    {
                        break;
                    }
                case V2Enum_EventType.JewelryCompose:
                    {

                        break;
                    }



                case V2Enum_EventType.ResearchLevelUp:
                    {
                        List<List<ResearchData>> masteryDatas = ResearchManager.Instance.GetResearchDatas();

                        for (int i = 0; i < masteryDatas.Count; ++i)
                        {
                            for (int j = 0; j < masteryDatas[i].Count; ++j)
                            {
                                PlayerResearchInfo playerMasteryInfo = ResearchManager.Instance.GetPlayerResearchInfo(masteryDatas[i][j]);
                                if (playerMasteryInfo == null)
                                    continue;

                                eventValue += playerMasteryInfo.Level;
                            }
                        }

                        break;
                    }


                case V2Enum_EventType.PassFreeRewardClaim:
                    {
                        foreach (var pair in PassManager.Instance.GetPassAllInfo())
                        {
                            eventValue += pair.Value.RecvFreeRewardCount.GetDecrypted().ToInt();
                            eventValue += pair.Value.RecvPaidRewardCount.GetDecrypted().ToInt();
                        }
                        
                        break;
                    }



                case V2Enum_EventType.MonsterKill:
                    {

                        break;
                    }


                //case V2Enum_EventType.StageClear:
                //    {
                //        eventValue = DungeonDataManager.Instance.GetMaxClearFarmStageStep();
                //        break;
                //    }


                //case V2Enum_EventType.DiamondDungeonClear:
                //    {
                //        eventValue = DungeonDataManager.Instance.GetDungeonRecord(V2Enum_Dungeon.DiamondDungeon, V2Enum_DungeonDifficultyType.Normal).ToInt();
                //        break;
                //    }
                //case V2Enum_EventType.MasteryDungeonClear:
                //    {
                //        eventValue = DungeonDataManager.Instance.GetMasteryLastClearPhaseNumber(V2Enum_DungeonDifficultyType.Normal);
                //        break;
                //    }
                //case V2Enum_EventType.GoldDungeonClear:
                //    {
                //        eventValue = DungeonDataManager.Instance.GetDungeonRecord(V2Enum_Dungeon.GoldDungeon, V2Enum_DungeonDifficultyType.Normal).ToInt();
                //        break;
                //    }
                //case V2Enum_EventType.SoulStoneDungeonClear:
                //    {
                //        eventValue = DungeonDataManager.Instance.GetDungeonRecord(V2Enum_Dungeon.SoulStoneDungeon, V2Enum_DungeonDifficultyType.Normal).ToInt();
                //        break;
                //    }
                //case V2Enum_EventType.RuneDungeonClear:
                //    {
                //        eventValue = DungeonDataManager.Instance.GetDungeonRecord(V2Enum_Dungeon.RuneDungeon, V2Enum_DungeonDifficultyType.Normal).ToInt();
                //        break;
                //    }

                case V2Enum_EventType.CheckInRewardGet:
                    {
                        eventValue += CheckInManager.Instance.GetCheckInInfo(V2Enum_CheckInType.Once).NextCheckRewardTime > 0 ? 1 : 0;
                        eventValue += CheckInManager.Instance.GetCheckInInfo(V2Enum_CheckInType.Repeat).NextCheckRewardTime > 0 ? 1 : 0;
                        break;
                    }

                case V2Enum_EventType.DailyMissionRewardGet:
                    {
                        List<QuestData> missionDatas = QuestManager.Instance.GetQuestDatas(V2Enum_QuestType.Daily);
                        for (int i = 0; i < missionDatas.Count; ++i)
                        {
                            QuestInfo missionDailyInfo = QuestManager.Instance.GetMissionInfo(missionDatas[i]);
                            if (missionDailyInfo == null)
                                continue;

                            eventValue += missionDailyInfo.RecvCount;
                        }

                        break;
                    }

                //case V2Enum_EventType.BossChallenge:
                //    {
                //        if (DungeonDataManager.Instance.GetMaxClearFarmStageStep() >= m_currentGuideQuest.QuestParam.GetDecrypted())
                //        {
                //            eventValue = DungeonDataManager.Instance.GetMaxClearFarmStageStep();
                //        }
                //        else
                //        {
                //            if (DungeonDataManager.Instance.GetFarmProgressStage() == m_currentGuideQuest.QuestParam.GetDecrypted())
                //            {
                //                if (DungeonManager.Instance.GetFameState() == FarmState.Boss)
                //                {
                //                    eventValue = m_currentGuideQuest.QuestParam.GetDecrypted();
                //                }
                //            }
                //        }

                //        break;
                //    }

                case V2Enum_EventType.MailGet:
                    {
                        eventValue = ShopPostManager.Instance.GetAllPostInfos().Count <= 0 ? m_currentGuideQuest.QuestParam.GetDecrypted() : 0;
                        break;
                    }
            }

            SetEventCount(v2Enum_EventType, eventValue);
        }
        //------------------------------------------------------------------------------------
        public void AddEventCount(V2Enum_EventType v2Enum_EventType, int addValue)
        {
            if (m_currentGuideQuest == null)
                return;

            if (m_currentGuideQuest.QuestType != v2Enum_EventType)
                return;

            SetEventCount(v2Enum_EventType, m_currentGuideValue + addValue);
        }
        //------------------------------------------------------------------------------------
        public void SetEventCount(V2Enum_EventType v2Enum_EventType, int setvalue)
        {
            if (m_currentGuideQuest == null)
                return;

            if (m_currentGuideQuest.QuestType != v2Enum_EventType)
                return;

            m_currentGuideValue = setvalue;

            if (IsClearGuideQuest(m_currentGuideQuest) == true)
            {
                if (m_currentGuideTutorialData != null)
                {
                    GuideInteractorManager.Instance.EndGuideQuest();
                    m_currentGuideTutorialData = null;
                }
            }

            m_refreshGuideQuestMsg.guideQuestData = m_currentGuideQuest;
            Message.Send(m_refreshGuideQuestMsg);
        }
        //------------------------------------------------------------------------------------
    }
}