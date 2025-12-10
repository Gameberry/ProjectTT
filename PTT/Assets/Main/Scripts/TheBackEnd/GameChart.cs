using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using LitJson;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;

namespace GameBerry.Chart
{
    public class ChartBase
    {
        public virtual bool IsLoaded()
        {
            return false;
        }

        public virtual void LoadComplete()
        { 

        }

        public virtual void OnReLoadComplete()
        { 

        }
    }

    public static class GameChart
    {
        public static Dictionary<Type, ChartBase> ChartData = new Dictionary<Type, ChartBase>();

        //------------------------------------------------------------------------------------
        public static bool TryGet<T>(out T chart) where T : ChartBase
        {
            if (ChartData.TryGetValue(typeof(T), out var table))
            {
                chart = (T)table;
                return true;
            }

#if UNITY_EDITOR
            Debug.LogError($"{typeof(T).Name} is null");
#endif
            chart = null;
            return false;
        }
        //------------------------------------------------------------------------------------
        public static T Get<T>() where T : ChartBase
        {
            ChartBase table;
            if (ChartData.TryGetValue(typeof(T), out table))
                return (T)table;

            Debug.LogErrorFormat("{0} is null", typeof(T).Name);
            return null;
        }
        //------------------------------------------------------------------------------------
        public static IEnumerator LoadGameChart(BackEnd.Content.ContentProgressDelegate loadingProcess)
        {
            BackEnd.Content.BackendContentTableReturnObject tableCallback = null;

            Backend.CDN.Content.Table.Get(bro =>
            {
                tableCallback = bro;
            });

            yield return new WaitUntil(() => tableCallback != null);

            if (tableCallback.IsSuccess() == false)
            {
                Debug.LogError(tableCallback);
                yield break;
            }


            BackEnd.Content.BackendContentReturnObject callback = null;

            Backend.CDN.Content.Local.Update(tableCallback.GetContentTableItemList(), loadingProcess, bro => { callback = bro; });

            yield return new WaitUntil(() => callback != null);

            if (callback.IsSuccess() == false)
            {
                Debug.LogError("GetContents : Fail : " + callback);
                yield break;
            }

            Dictionary<string, BackEnd.Content.ContentItem> bro = callback.GetContentDictionarySortByChartName();

            int setcount = 0;

            foreach (var pair in bro)
            {
                JsonData data = JsonMapper.ToObject(pair.Value.contentString);

                string className = string.Format("GameBerry.Chart.{0}Chart", pair.Key);

                var type = System.Type.GetType(className);
                if (type == null)
                {
#if DEV_DEFINE
                    Debug.LogError($"Can't convert to {className}");
#endif
                    continue;
                }

                var obj = JsonConvert.DeserializeObject($"{{\"rows\":{pair.Value.contentString}}}", type, new BackendChartValueConverter(false));

                if (obj == null || (obj as Chart.ChartBase).IsLoaded() == false)
                {
                    Debug.LogError($"LoadChart Error {className}: {data}");
                    continue;
                }

                Chart.ChartBase chart = obj as Chart.ChartBase;

                ChartData.Add(Type.GetType(className), chart);

                chart.LoadComplete();

                setcount++;
                if (setcount > 12)
                {
                    setcount = 0;
                    yield return null;
                }
            }
        }
    }
}