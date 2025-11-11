using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;

namespace GameBerry
{
    public class BackLightData
    {
        public int LightTargetIndex;
        public V2Enum_Grade LightType;
    }

    public class BackLightLocalTable : LocalTableBase
    {
        private Dictionary<int, BackLightData> backLightGradeData_Dic = new Dictionary<int, BackLightData>();
        public override async UniTask InitData_Async()
        {
            string jsonstring = ClientLocalChartManager.GetLocalChartData_V2("BackLight.json");
            List<BackLightData> BackLightDatas = JsonConvert.DeserializeObject<List<BackLightData>>(jsonstring);

            for (int i = 0; i < BackLightDatas.Count; ++i)
            {
                backLightGradeData_Dic.Add(BackLightDatas[i].LightTargetIndex, BackLightDatas[i]);
            }
        }
        //------------------------------------------------------------------------------------
        public V2Enum_Grade GetGrade(int targetIdx)
        {
            if (backLightGradeData_Dic.ContainsKey(targetIdx) == false)
                return V2Enum_Grade.D;

            return backLightGradeData_Dic[targetIdx].LightType;
        }
        //------------------------------------------------------------------------------------
    }
}