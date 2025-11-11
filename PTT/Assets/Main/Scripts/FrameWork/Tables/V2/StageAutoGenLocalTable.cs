using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class StageAutoGenInfoData
    {
        public int Index;
        public int GroupIndex;
        public int Step;
        public int IncreaseFactor1;
        public int IncreaseFactor2;
    }

    public class StageAutoGenConfigData
    {
        public int Index;
        public int ResourceIndex;
        public int StageRangeMin;
        public int StageRangeMax;
        public int GroupIndex;

        public string StageHeadLocalString;
    }

    public class StageAutoGenLocalTable : LocalTableBase
    {
        public Dictionary<int, List<StageAutoGenInfoData>> stageAutoGenInfoGroupData_Dic = new Dictionary<int, List<StageAutoGenInfoData>>();

        public List<StageAutoGenConfigData> stageAutoGenConfigDatas = new List<StageAutoGenConfigData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("StageAutoGenInfo", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                StageAutoGenInfoData stageAutoGenInfoData = new StageAutoGenInfoData();
                stageAutoGenInfoData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                stageAutoGenInfoData.GroupIndex = rows[i]["GroupIndex"].ToString().ToInt();
                stageAutoGenInfoData.Step = rows[i]["Step"].ToString().ToInt();
                stageAutoGenInfoData.IncreaseFactor1 = rows[i]["IncreaseFactor1"].ToString().ToInt();
                stageAutoGenInfoData.IncreaseFactor2 = rows[i]["IncreaseFactor2"].ToString().ToInt();

                if (stageAutoGenInfoGroupData_Dic.ContainsKey(stageAutoGenInfoData.GroupIndex) == false)
                    stageAutoGenInfoGroupData_Dic.Add(stageAutoGenInfoData.GroupIndex, new List<StageAutoGenInfoData>());

                stageAutoGenInfoGroupData_Dic[stageAutoGenInfoData.GroupIndex].Add(stageAutoGenInfoData);
            }

            stageAutoGenConfigDatas = await TheBackEnd.TheBackEnd_GameChart.GetListDat_Async<StageAutoGenConfigData>("StageAutoGenConfig");

            for (int i = 0; i < stageAutoGenConfigDatas.Count; ++i)
            {
                StageAutoGenConfigData stageAutoGenConfigData = stageAutoGenConfigDatas[i];
                stageAutoGenConfigData.StageHeadLocalString = string.Format("stageAutoGenConfig/{0}/name", stageAutoGenConfigData.ResourceIndex);
            }
        }
        //------------------------------------------------------------------------------------
        public StageAutoGenInfoData GetStageAutoGenInfoData(int stage)
        {
            StageAutoGenConfigData stageAutoGenConfigData = GetStageAutoGenConfigData(stage);

            if (stageAutoGenConfigData == null)
                return null;

            if (stageAutoGenInfoGroupData_Dic.ContainsKey(stageAutoGenConfigData.GroupIndex) == false)
                return null;

            List<StageAutoGenInfoData> stageAutoGenInfoDatas = stageAutoGenInfoGroupData_Dic[stageAutoGenConfigData.GroupIndex];

            int step = stage % stageAutoGenInfoDatas.Count;
            if(step == 0)
                step = stageAutoGenInfoDatas.Count;

            return stageAutoGenInfoDatas.Find(x => x.Step == step);
        }
        //------------------------------------------------------------------------------------
        public StageAutoGenConfigData GetStageAutoGenConfigData(int stage)
        {
            StageAutoGenConfigData stageAutoGenConfigData = null;

            for (int i = 0; i < stageAutoGenConfigDatas.Count; ++i)
            {
                if (stageAutoGenConfigDatas[i].StageRangeMin <= stage
                    && stageAutoGenConfigDatas[i].StageRangeMax >= stage)
                    stageAutoGenConfigData = stageAutoGenConfigDatas[i];
            }

            //if (stageAutoGenConfigData == null)
            //    stageAutoGenConfigData = stageAutoGenConfigDatas[stageAutoGenConfigDatas.Count - 1];

            return stageAutoGenConfigData;
        }
        //------------------------------------------------------------------------------------
    }
}