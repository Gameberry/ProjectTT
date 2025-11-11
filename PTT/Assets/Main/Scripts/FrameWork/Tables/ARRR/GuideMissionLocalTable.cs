using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;
using LitJson;

namespace GameBerry
{
    public class GuideMissionData
    {
        public ObscuredInt Index;

        public ObscuredInt SeqGap;

        public V2Enum_QuestGoalType MissionType;
        public ObscuredInt MissionParam1;

        public ObscuredInt MissionRewardIndex;
        public ObscuredDouble MissionRewardValue;

        public ObscuredInt Uishortcut;
        public ObscuredInt IsRepeat;
    }

    public class GuideMissionLocalTable : LocalTableBase
    {
        private List<GuideMissionData> _guideMissionDatas = new List<GuideMissionData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("GuideMission", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                GuideMissionData guideMissionData = new GuideMissionData();
                guideMissionData.Index = rows[i]["Index"].ToString().ToInt();
                guideMissionData.SeqGap = rows[i]["SeqGap"].ToString().ToInt();

                guideMissionData.MissionType = rows[i]["QuestType"].ToString().ToInt().IntToEnum32<V2Enum_QuestGoalType>();
                guideMissionData.MissionParam1 = rows[i]["MissionParam1"].ToString().ToInt();

                guideMissionData.MissionRewardIndex = rows[i]["MissionRewardIndex"].ToString().ToInt();
                guideMissionData.MissionRewardValue = rows[i]["MissionRewardValue"].ToString().ToDouble();

                guideMissionData.Uishortcut = rows[i]["Uishortcut"].ToString().ToInt();
                guideMissionData.IsRepeat = rows[i]["IsRepeat"].ToString().ToInt();

                _guideMissionDatas.Add(guideMissionData);
            }
        }
        //------------------------------------------------------------------------------------
        public List<GuideMissionData> GetGuideMissionDatas()
        {
            return _guideMissionDatas;
        }
        //------------------------------------------------------------------------------------
    }
}