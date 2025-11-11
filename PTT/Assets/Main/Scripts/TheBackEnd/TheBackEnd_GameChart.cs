using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using LitJson;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;

namespace GameBerry.TheBackEnd
{
    public static class TheBackEnd_GameChart
    {
        public static Dictionary<string, string> TableChartFileld = new Dictionary<string, string>();
        public static Dictionary<string, string> TableChartUUID = new Dictionary<string, string>();

        public static Dictionary<string, JsonData> ChartBROData = new Dictionary<string, JsonData>();

        public static Queue<string> needSaveChart = new Queue<string>();

        //------------------------------------------------------------------------------------
        public static void GetBackEndChart(string fileidkey, System.Action<JsonData> action)
        {
            if (Managers.SceneManager.Instance.UseLocalChart == false)
            {
                if (ChartBROData.ContainsKey(fileidkey) == false)
                {
                    Debug.LogError(string.Format("{0} is null", fileidkey));
                    return;
                }

                action?.Invoke(ChartBROData[fileidkey]);
            }
            else
            {
                GetBackEndChart_LocalJson(fileidkey, action);
            }
        }
        //------------------------------------------------------------------------------------
        public static void GetBackEndChart_LocalJson(string jsonstr, System.Action<JsonData> action)
        {
            string jsonstring = ClientLocalChartManager.GetLocalChartData_V2(string.Format("{0}.json", jsonstr));

            JsonData jsonData = JsonMapper.ToObject(jsonstring);

            action?.Invoke(jsonData);
        }
        //------------------------------------------------------------------------------------
        public static async UniTask<List<T>> GetListDat_Async<T>(string fileidkey)
        {
           

            if (Managers.SceneManager.Instance.UseLocalChart == false)
            {
                if (ChartBROData.ContainsKey(fileidkey) == false)
                {
                    Debug.LogError(string.Format("{0} is null", fileidkey));
                    return null;
                }

                List<T> ts = null;

                await Task.Run(() =>
                {
                    //string jsonstr = ChartBROData[fileidkey].contentJson.ToJson();
                    ts = JsonConvert.DeserializeObject<List<T>>(ChartBROData[fileidkey].ToJson());
                });

                return ts;
            }
            else
            {
                List<T> ts = null;

                string jsonstr = fileidkey;

                string jsonstring = ClientLocalChartManager.GetLocalChartData_V2(string.Format("{0}.json", jsonstr));

                ts = JsonConvert.DeserializeObject<List<T>>(jsonstring);

                return ts;
            }
            
        }
        //------------------------------------------------------------------------------------
    }
}