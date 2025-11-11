using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class QuestData
    {
        public ObscuredInt Index;

        public V2Enum_QuestType QuestType;

        public V2Enum_QuestGoalType QuestGoalType;
        public ObscuredInt QuestGoalValue;

        public V2Enum_Goods ClearRewardType = V2Enum_Goods.Max;
        public ObscuredInt ClearRewardIndex;
        public ObscuredDouble ClearRewardValue;

        public string NameLocalStringKey;
    }

    public class QuestGaugeData
    {
        public ObscuredInt Index;

        public V2Enum_QuestType QuestType;

        public ObscuredInt RequiredQuestCount;

        public V2Enum_Goods ClearRewardType;
        public ObscuredInt ClearRewardIndex;
        public ObscuredDouble ClearRewardValue;

        public RewardData RewardData = new RewardData();
    }

    public class QuestLocalTable : LocalTableBase
    {
        private Dictionary<V2Enum_QuestType, List<QuestData>> _questDatas = new Dictionary<V2Enum_QuestType, List<QuestData>>();
        private Dictionary<V2Enum_QuestType, List<QuestGaugeData>> _questGaugeDatas = new Dictionary<V2Enum_QuestType, List<QuestGaugeData>>();
        private Dictionary<V2Enum_QuestType, ObscuredInt> _questGaugeMaxCount = new Dictionary<V2Enum_QuestType, ObscuredInt>();

        public Dictionary<int, QuestData> m_missionDatas = new Dictionary<int, QuestData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            await SetQuestDatas("Quest");
            await SetQuestDatas("Achievement");
            await SetQuestGaugeDatas("QuestGauge");
        }
        //------------------------------------------------------------------------------------
        private async UniTask SetQuestDatas(string json)
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart(json, o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                QuestData questData = new QuestData();
                questData.Index = rows[i]["Index"].ToString().ToInt();

                questData.QuestType = rows[i]["QuestType"].ToString().ToInt().IntToEnum32<V2Enum_QuestType>();

                questData.QuestGoalType = rows[i]["QuestGoalType"].ToString().ToInt().IntToEnum32<V2Enum_QuestGoalType>();
                questData.QuestGoalValue = rows[i]["QuestGoalValue"].ToString().ToInt();

                questData.ClearRewardIndex = rows[i]["ClearRewardIndex"].ToString().ToInt();
                questData.ClearRewardValue = rows[i]["ClearRewardValue"].ToString().ToDouble();

                questData.NameLocalStringKey = string.Format("QuestGoalType/{0}", questData.QuestGoalType.Enum32ToInt());

                m_missionDatas.Add(questData.Index.GetDecrypted(), questData);

                if (_questDatas.ContainsKey(questData.QuestType) == false)
                    _questDatas.Add(questData.QuestType, new List<QuestData>());

                _questDatas[questData.QuestType].Add(questData);
            }
        }
        //------------------------------------------------------------------------------------
        private async UniTask SetQuestGaugeDatas(string json)
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart(json, o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                QuestGaugeData questGaugeData = new QuestGaugeData();
                questGaugeData.Index = rows[i]["Index"].ToString().ToInt();

                questGaugeData.QuestType = rows[i]["QuestType"].ToString().ToInt().IntToEnum32<V2Enum_QuestType>();
                questGaugeData.RequiredQuestCount = rows[i]["RequiredQuestCount"].ToString().ToInt();

                questGaugeData.RewardData.Index = rows[i]["ClearRewardIndex"].ToString().ToInt();
                questGaugeData.RewardData.Amount = rows[i]["ClearRewardValue"].ToString().ToDouble();

                if (_questGaugeDatas.ContainsKey(questGaugeData.QuestType) == false)
                    _questGaugeDatas.Add(questGaugeData.QuestType, new List<QuestGaugeData>());

                _questGaugeDatas[questGaugeData.QuestType].Add(questGaugeData);


                if (_questGaugeMaxCount.ContainsKey(questGaugeData.QuestType) == false)
                    _questGaugeMaxCount.Add(questGaugeData.QuestType, 0);

                if (_questGaugeMaxCount[questGaugeData.QuestType] < questGaugeData.RequiredQuestCount)
                    _questGaugeMaxCount[questGaugeData.QuestType] = questGaugeData.RequiredQuestCount;
            }
        }
        //------------------------------------------------------------------------------------
        public QuestData GetQuestData(int index)
        {
            if (m_missionDatas.ContainsKey(index) == true)
                return m_missionDatas[index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public List<QuestData> GetQuestDatas(V2Enum_QuestType v2Enum_QuestType)
        {
            if (_questDatas.ContainsKey(v2Enum_QuestType) == false)
                return null;

            return _questDatas[v2Enum_QuestType];
        }
        //------------------------------------------------------------------------------------
        public List<QuestGaugeData> GetQuestGaugeDatas(V2Enum_QuestType v2Enum_QuestType)
        {
            if (_questGaugeDatas.ContainsKey(v2Enum_QuestType) == false)
                return null;

            return _questGaugeDatas[v2Enum_QuestType];
        }
        //------------------------------------------------------------------------------------
        public ObscuredInt GetQuestGaugeMaxCount(V2Enum_QuestType v2Enum_QuestType)
        {
            if (_questGaugeMaxCount.ContainsKey(v2Enum_QuestType) == false)
                return -1;

            return _questGaugeMaxCount[v2Enum_QuestType];
        }
        //------------------------------------------------------------------------------------
    }
}