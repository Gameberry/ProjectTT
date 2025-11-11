using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using LitJson;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class LocalOverrideLocalTable : LocalTableBase
    {
        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("LocalOverride", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            StringLocalChart stringLocalChart = Managers.TableManager.Instance.GetTableClass<StringLocalChart>();

            for (int i = 0; i < rows.Count; ++i)
            {
                try
                {
                    string id = rows[i]["TextKey"].ToString();

                    StringLocalData stringlocaldata = stringLocalChart.GetLocalString(id);
                    if (stringlocaldata == null)
                    {
                        stringlocaldata = new StringLocalData();
                        stringlocaldata.LocalStringID = id;
                        stringLocalChart.AddLocalString(stringlocaldata);
                    }

                    if (stringlocaldata.LocalizeString.ContainsKey(LocalizeType.Korean) == false)
                        stringlocaldata.LocalizeString.Add(LocalizeType.Korean, string.Empty);

                    if (stringlocaldata.LocalizeString.ContainsKey(LocalizeType.English) == false)
                        stringlocaldata.LocalizeString.Add(LocalizeType.English, string.Empty);

                    if (stringlocaldata.LocalizeString.ContainsKey(LocalizeType.Japanese) == false)
                        stringlocaldata.LocalizeString.Add(LocalizeType.Japanese, string.Empty);

                    if (stringlocaldata.LocalizeString.ContainsKey(LocalizeType.ChineseTraditional) == false)
                        stringlocaldata.LocalizeString.Add(LocalizeType.ChineseTraditional, string.Empty);

                    if (stringlocaldata.LocalizeString.ContainsKey(LocalizeType.Portuguesa) == false)
                        stringlocaldata.LocalizeString.Add(LocalizeType.Portuguesa, string.Empty);

                    if (stringlocaldata.LocalizeString.ContainsKey(LocalizeType.Spanish) == false)
                        stringlocaldata.LocalizeString.Add(LocalizeType.Spanish, string.Empty);

                    stringlocaldata.LocalizeString[LocalizeType.Korean] = rows[i]["TextKR"].ToString().Replace("\\n", "\n");
                    stringlocaldata.LocalizeString[LocalizeType.English] = rows[i]["TextEN"].ToString().Replace("\\n", "\n");
                    stringlocaldata.LocalizeString[LocalizeType.Japanese] = rows[i]["TextJP"].ToString().Replace("\\n", "\n");
                    stringlocaldata.LocalizeString[LocalizeType.ChineseTraditional] = rows[i]["TextTW"].ToString().Replace("\\n", "\n");
                    stringlocaldata.LocalizeString[LocalizeType.Portuguesa] = rows[i]["TextPT"].ToString().Replace("\\n", "\n");
                    stringlocaldata.LocalizeString[LocalizeType.Spanish] = rows[i]["TextSP"].ToString().Replace("\\n", "\n");
                }
                catch
                {
                    Debug.LogError(string.Format("LocalOverrideChart 터짐 범인 : {0} 번째", i));
                }
            }
        }
    }
}