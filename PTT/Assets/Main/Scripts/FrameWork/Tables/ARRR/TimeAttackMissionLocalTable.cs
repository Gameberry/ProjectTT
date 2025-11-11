using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;
using LitJson;

namespace GameBerry
{
    public class TimeAttackMissionData
    {
        public ObscuredInt Index;

        public V2Enum_OpenConditionType OpenConditionType;
        public ObscuredInt OpenConditionParam;


        public V2Enum_OpenConditionType ClearConditionType;
        public ObscuredInt ClearConditionParam;

        public V2Enum_IntervalType DurationType;
        public ObscuredInt DurationParam;

        public List<RewardData> ReturnGoods = new List<RewardData>();
    }

    public class TimeAttackMissionLocalTable : LocalTableBase
    {
        private Dictionary<ObscuredInt, TimeAttackMissionData> _timeAttackMissionData_Dic = new Dictionary<ObscuredInt, TimeAttackMissionData>();

        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("TimeAttackMission", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                TimeAttackMissionData timeAttackMissionData = new TimeAttackMissionData();

                timeAttackMissionData.Index = rows[i]["Index"].ToString().ToInt();

                timeAttackMissionData.OpenConditionType = rows[i]["OpenConditionType"].ToString().ToInt().IntToEnum32<V2Enum_OpenConditionType>();
                timeAttackMissionData.OpenConditionParam = rows[i]["OpenConditionParam"].ToString().ToInt();

                timeAttackMissionData.ClearConditionType = rows[i]["ClearConditionType"].ToString().ToInt().IntToEnum32<V2Enum_OpenConditionType>();
                timeAttackMissionData.ClearConditionParam = rows[i]["ClearConditionParam"].ToString().ToInt();

                timeAttackMissionData.DurationType = rows[i]["DurationType"].ToString().ToInt().IntToEnum32<V2Enum_IntervalType>();
                timeAttackMissionData.DurationParam = rows[i]["DurationParam"].ToString().ToInt();

                for (int j = 1; j <= 4; ++j)
                {
                    try
                    {
                        string GoodsIndex = string.Format("ReturnGoodsParam{0}1", j);
                        int GoodsIndexParam = rows[i][GoodsIndex].ToString().ToInt();
                        if (GoodsIndexParam == -1 || GoodsIndexParam == 0)
                            continue;

                        string GoodsAmount = string.Format("ReturnGoodsParam{0}2", j);
                        int GoodsAmountParam = rows[i][GoodsAmount].ToString().ToInt();
                        if (GoodsAmountParam == -1 || GoodsAmountParam == 0)
                            continue;

                        RewardData rewardData = new RewardData();
                        rewardData.Index = GoodsIndexParam;
                        rewardData.Amount = GoodsAmountParam;

                        timeAttackMissionData.ReturnGoods.Add(rewardData);
                    }
                    catch
                    {

                    }
                }

                if (_timeAttackMissionData_Dic.ContainsKey(timeAttackMissionData.Index) == false)
                    _timeAttackMissionData_Dic.Add(timeAttackMissionData.Index, timeAttackMissionData);
            }
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, TimeAttackMissionData> GetAllTimeAttackMissionData()
        {
            return _timeAttackMissionData_Dic;
        }
        //------------------------------------------------------------------------------------
        public TimeAttackMissionData GetTimeAttackMissionData(ObscuredInt index)
        {
            if (_timeAttackMissionData_Dic.ContainsKey(index) == true)
                return _timeAttackMissionData_Dic[index];

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}