using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class CheckInRewardData
    {
        public ObscuredInt Index;

        public ObscuredInt CheckInCount;

        public V2Enum_Goods CheckInRewardGoodsType;
        public ObscuredInt CheckInRewardParam1;
        public ObscuredDouble CheckInRewardParam2;

        public V2Enum_CheckInType CheckInType;
    }

    public class CheckInLocalTable : LocalTableBase
    {
        public List<CheckInRewardData> m_checkInReward = new List<CheckInRewardData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("CheckInReward", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                CheckInRewardData checkInRewardData = new CheckInRewardData();
                checkInRewardData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();

                checkInRewardData.CheckInCount = rows[i]["CheckInCount"].ToString().ToInt();

                checkInRewardData.CheckInRewardGoodsType = rows[i]["CheckInRewardGoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                checkInRewardData.CheckInRewardParam1 = rows[i]["CheckInRewardParam1"].ToString().ToInt();
                checkInRewardData.CheckInRewardParam2 = rows[i]["CheckInRewardParam2"].ToString().ToDouble();

                checkInRewardData.CheckInType = rows[i]["CheckInType"].ToString().ToInt().IntToEnum32<V2Enum_CheckInType>();

                m_checkInReward.Add(checkInRewardData);
            }

            //m_checkInReward = await TheBackEnd.TheBackEnd_GameChart.GetListDat_Async<CheckInRewardData>("CheckInReward");
        }
        //------------------------------------------------------------------------------------
        public List<CheckInRewardData> GetCheckInRewardOnceDatas()
        {
            return m_checkInReward.FindAll(x => x.CheckInType == V2Enum_CheckInType.Once);
        }
        //------------------------------------------------------------------------------------
        public List<CheckInRewardData> GetCheckInRewardRepeatDatas()
        {
            return m_checkInReward.FindAll(x => x.CheckInType == V2Enum_CheckInType.Repeat);
        }
        //------------------------------------------------------------------------------------
    }
}