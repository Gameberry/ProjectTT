using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using LitJson;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class GuideQuestData
    {
        public ObscuredInt Index;
        public ObscuredInt ResourceIndex;

        public ObscuredInt QuestOrder;
        public V2Enum_EventType QuestType;
        public ObscuredInt QuestParam;

        public V2Enum_Goods ClearRewardType;
        public ObscuredInt ClearRewardParam1;
        public ObscuredDouble ClearRewardParam2;

        public ObscuredInt MaxApproachStage;

        public string NameLocalStringKey;
    }

    public class GuideTutorialData
    {
        public int Index;
        public V2Enum_EventType QuestType;
        public int QuestIndex;
        public string SentenceKR;
        public string SentenceEN;
        public string SentenceJP;
        public string SentenceTW;
        public string SentencePT;
        public string SentenceSP;

        public Dictionary<LocalizeType, string> TutorialStr = new Dictionary<LocalizeType, string>();

        public int IsFingerGuide;
    }

    public class GuideQuestLocalTable : LocalTableBase
    {
        public List<GuideQuestData> m_guideQuestDatas = new List<GuideQuestData>();
        public List<GuideTutorialData> m_guideTutorialDatas = new List<GuideTutorialData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            await UniTask.WhenAll(
                SetGuideQuestData(),
                SetGuideTutorialData());
        }
        //------------------------------------------------------------------------------------
        private async UniTask SetGuideQuestData()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("GuideQuest", o =>
            { 
                rows = o; 
            });

            await UniTask.WaitUntil(() => rows != null);

            if (rows == null)
                UnityEngine.Debug.Log("rows is null");

            Dictionary<V2Enum_EventType, int> guideStack = new Dictionary<V2Enum_EventType, int>();

            for (int i = 0; i < rows.Count; ++i)
            {
                GuideQuestData guideQuestData = new GuideQuestData();

                guideQuestData.Index = rows[i]["Index"].ToString().ToInt();
                guideQuestData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                guideQuestData.QuestOrder = rows[i]["QuestOrder"].ToString().ToInt();
                guideQuestData.QuestType = rows[i]["QuestType"].ToString().ToInt().IntToEnum32<V2Enum_EventType>(); ;
                guideQuestData.QuestParam = rows[i]["QuestParam"].ToString().ToInt();

                guideQuestData.ClearRewardType = rows[i]["ClearRewardType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>(); ;
                guideQuestData.ClearRewardParam1 = rows[i]["ClearRewardParam1"].ToString().ToInt();
                guideQuestData.ClearRewardParam2 = rows[i]["ClearRewardParam2"].ToString().ToDouble();
                guideQuestData.MaxApproachStage = rows[i]["MaxApproachStage"].ToString().ToInt();


                //if (guideQuestData.QuestType == V2Enum_EventType.CharacterAttackTraining
                //    || guideQuestData.QuestType == V2Enum_EventType.CharacterHpTraining

                //    || guideQuestData.QuestType == V2Enum_EventType.AttackSpeedTraining
                //    || guideQuestData.QuestType == V2Enum_EventType.CriticalChanceTraining

                //    || guideQuestData.QuestType == V2Enum_EventType.CriticalDamageTraining
                //    || guideQuestData.QuestType == V2Enum_EventType.AttackIncreaseTraining
                //    || guideQuestData.QuestType == V2Enum_EventType.HpIncreaseTraining
                //    || guideQuestData.QuestType == V2Enum_EventType.FinalAttackIncreaseTraining

                //    || guideQuestData.QuestType == V2Enum_EventType.ElementAttackLightTraining
                //    || guideQuestData.QuestType == V2Enum_EventType.ElementAttackDarknessTraining
                //    || guideQuestData.QuestType == V2Enum_EventType.ElementAttackWaterTraining
                //    || guideQuestData.QuestType == V2Enum_EventType.ElementAttackFireTraining
                //    || guideQuestData.QuestType == V2Enum_EventType.ElementAttackGrassTraining

                //    || guideQuestData.QuestType == V2Enum_EventType.FameLevelUp

                //    || guideQuestData.QuestType == V2Enum_EventType.DiamondDungeonClear
                //    || guideQuestData.QuestType == V2Enum_EventType.MasteryDungeonClear
                //    || guideQuestData.QuestType == V2Enum_EventType.GoldDungeonClear
                //    || guideQuestData.QuestType == V2Enum_EventType.SoulStoneDungeonClear
                //    || guideQuestData.QuestType == V2Enum_EventType.RuneDungeonClear

                //    || guideQuestData.QuestType == V2Enum_EventType.WeaponLevelUp
                //    || guideQuestData.QuestType == V2Enum_EventType.AllArmorsLevelUp
                //    || guideQuestData.QuestType == V2Enum_EventType.AccessoryLevelUp
                //    || guideQuestData.QuestType == V2Enum_EventType.SkillLevelUp
                //    || guideQuestData.QuestType == V2Enum_EventType.AllyLevelUp
                //    || guideQuestData.QuestType == V2Enum_EventType.HelmetLevelUp
                //    || guideQuestData.QuestType == V2Enum_EventType.ArmorLevelUp
                //    || guideQuestData.QuestType == V2Enum_EventType.GlovesLevelUp
                //    || guideQuestData.QuestType == V2Enum_EventType.ShoesLevelUp

                //    || guideQuestData.QuestType == V2Enum_EventType.WeaponSummon
                //    || guideQuestData.QuestType == V2Enum_EventType.ArmorSummon
                //    || guideQuestData.QuestType == V2Enum_EventType.AccessorySummon
                //    || guideQuestData.QuestType == V2Enum_EventType.SkillSummon
                //    || guideQuestData.QuestType == V2Enum_EventType.AllySummon
                //    )
                //{
                //    if (guideStack.ContainsKey(guideQuestData.QuestType) == false)
                //        guideStack.Add(guideQuestData.QuestType, 0);

                //    guideStack[guideQuestData.QuestType] += guideQuestData.QuestParam.GetDecrypted();

                //    guideQuestData.QuestParam = guideStack[guideQuestData.QuestType];
                //}
                //else if (guideQuestData.QuestType == V2Enum_EventType.BossChallenge)
                //    GuideQuestContainer.ContainBossChallengeStep.Add(guideQuestData.QuestParam.GetDecrypted(), false);

                guideQuestData.NameLocalStringKey = string.Format("guideQuest/{0}/name", guideQuestData.QuestType.Enum32ToInt());

                m_guideQuestDatas.Add(guideQuestData);
            }

            m_guideQuestDatas.Sort((x, y) =>
            {
                if (x.QuestOrder.GetDecrypted() > y.QuestOrder.GetDecrypted())
                    return 1;
                else
                    return -1;
            });
        }
        //------------------------------------------------------------------------------------
        private async UniTask SetGuideTutorialData()
        {
            string jsonstring = await ClientLocalChartManager.GetLocalChartData_Task("GuideTutorial.json");

            m_guideTutorialDatas = JsonConvert.DeserializeObject<List<GuideTutorialData>>(jsonstring);

            for (int i = 0; i < m_guideTutorialDatas.Count; ++i)
            {
                GuideTutorialData guideTutorialData = m_guideTutorialDatas[i];
                guideTutorialData.TutorialStr.Add(LocalizeType.Korean, guideTutorialData.SentenceKR);
                guideTutorialData.TutorialStr.Add(LocalizeType.English, guideTutorialData.SentenceEN);
                guideTutorialData.TutorialStr.Add(LocalizeType.Japanese, guideTutorialData.SentenceJP);
                guideTutorialData.TutorialStr.Add(LocalizeType.ChineseTraditional, guideTutorialData.SentenceTW);
                guideTutorialData.TutorialStr.Add(LocalizeType.Portuguesa, guideTutorialData.SentencePT);
                guideTutorialData.TutorialStr.Add(LocalizeType.Spanish, guideTutorialData.SentenceSP);
            }
        }
        //------------------------------------------------------------------------------------
        public List<GuideQuestData> GetDatas()
        {
            return m_guideQuestDatas;
        }
        //------------------------------------------------------------------------------------
        public GuideTutorialData GetTutorialData(ObscuredInt questindex)
        {
            return m_guideTutorialDatas.Find(x => x.QuestIndex == questindex);
        }
        //------------------------------------------------------------------------------------
    }
}