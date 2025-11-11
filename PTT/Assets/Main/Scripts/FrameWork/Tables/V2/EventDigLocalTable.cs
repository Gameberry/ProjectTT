using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class EventDigRewardFailData
    {
        public ObscuredInt Index;

        public ObscuredInt FailRewardNumber;

        public RewardData RewardData = new RewardData();
    }

    public class EventDigRewardSuccessData
    {
        public ObscuredInt Index;

        public ObscuredInt DigSuccessAccumedCount;

        public RewardData RewardData = new RewardData();
    }

    public class EventDigPassData
    {
        public ObscuredInt Index;

        public ObscuredInt DisplayOrder;
        public ObscuredInt DigLevel;

        public List<RewardData> rewardDatas = new List<RewardData>();

        public ObscuredBool IsPaidProduct;
        public ObscuredInt PaidProductIndex;

        public EventDigPassData PrevData;
        public EventDigPassData NextData;
    }

    public class EventDigPassLevelData
    {
        public ObscuredInt Index;

        public ObscuredInt TargetLevel;
        public ObscuredInt RequiredDigCount;
    }

    public class EventDigLocalTable : LocalTableBase
    {
        public List<EventDigRewardFailData> eventDigRewardFailDatas = new List<EventDigRewardFailData>();
        public List<EventDigRewardSuccessData> eventDigRewardSuccessDatas = new List<EventDigRewardSuccessData>();
        public List<EventDigPassData> eventDigPassDatas = new List<EventDigPassData>();
        public List<EventDigPassLevelData> eventDigPassLevelDatas = new List<EventDigPassLevelData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("EventDigRewardFail", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                EventDigRewardFailData eventDigRewardFailData = new EventDigRewardFailData();

                eventDigRewardFailData.Index = rows[i]["Index"].ToString().ToInt();

                eventDigRewardFailData.FailRewardNumber = rows[i]["FailRewardNumber"].ToString().ToInt();

                eventDigRewardFailData.RewardData.V2Enum_Goods = rows[i]["FailRewardType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                eventDigRewardFailData.RewardData.Index = rows[i]["FailRewardParam1"].ToString().ToInt();
                eventDigRewardFailData.RewardData.Amount = rows[i]["FailRewardParam2"].ToString().ToDouble();

                eventDigRewardFailDatas.Add(eventDigRewardFailData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("EventDigRewardSuccess", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                EventDigRewardSuccessData eventDigRewardSuccessData = new EventDigRewardSuccessData();

                eventDigRewardSuccessData.Index = rows[i]["Index"].ToString().ToInt();

                eventDigRewardSuccessData.DigSuccessAccumedCount = rows[i]["DigSuccessAccumedCount"].ToString().ToInt();

                eventDigRewardSuccessData.RewardData.V2Enum_Goods = rows[i]["SuccessRewardType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                eventDigRewardSuccessData.RewardData.Index = rows[i]["SuccessRewardParam1"].ToString().ToInt();
                eventDigRewardSuccessData.RewardData.Amount = rows[i]["SuccessRewardParam2"].ToString().ToDouble();

                eventDigRewardSuccessDatas.Add(eventDigRewardSuccessData);
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("EventDigPass", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                EventDigPassData eventDigPassData = new EventDigPassData();

                eventDigPassData.Index = rows[i]["Index"].ToString().ToInt();

                eventDigPassData.DisplayOrder = rows[i]["DisplayOrder"].ToString().ToInt();
                eventDigPassData.DigLevel = rows[i]["DigLevel"].ToString().ToInt();

                {
                    RewardData rewardData = new RewardData();
                    rewardData.V2Enum_Goods = rows[i]["FreeRewardType1"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                    rewardData.Index = rows[i]["FreeRewardParam11"].ToString().ToInt();
                    rewardData.Amount = rows[i]["FreeRewardParam12"].ToString().ToDouble();

                    eventDigPassData.rewardDatas.Add(rewardData);
                }

                {
                    RewardData rewardData = new RewardData();
                    rewardData.V2Enum_Goods = rows[i]["FreeRewardType2"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                    rewardData.Index = rows[i]["FreeRewardParam21"].ToString().ToInt();
                    rewardData.Amount = rows[i]["FreeRewardParam22"].ToString().ToDouble();

                    eventDigPassData.rewardDatas.Add(rewardData);
                }

                eventDigPassData.IsPaidProduct = rows[i]["IsPaidProduct"].ToString().ToInt() == 1;
                eventDigPassData.PaidProductIndex = rows[i]["PaidProductIndex"].ToString().ToInt();

                eventDigPassDatas.Add(eventDigPassData);
            }


            for (int i = 0; i < eventDigPassDatas.Count; ++i)
            {
                EventDigPassData eventDigPassData = eventDigPassDatas[i];

                if (i != 0)
                {
                    eventDigPassData.PrevData = eventDigPassDatas[i - 1];
                }

                if (i != eventDigPassDatas.Count - 1)
                {
                    eventDigPassData.NextData = eventDigPassDatas[i + 1];
                }
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("EventDigPassLevel", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                EventDigPassLevelData eventDigPassLevelData = new EventDigPassLevelData();

                eventDigPassLevelData.Index = rows[i]["Index"].ToString().ToInt();

                eventDigPassLevelData.TargetLevel = rows[i]["TargetLevel"].ToString().ToInt();
                eventDigPassLevelData.RequiredDigCount = rows[i]["RequiredDigCount"].ToString().ToInt();

                eventDigPassLevelDatas.Add(eventDigPassLevelData);
            }
        }
    }
}