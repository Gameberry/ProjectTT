using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;

namespace GameBerry
{
    public enum LocalizeType
    {
        English = 0,
        Korean,
        Japanese,
        ChineseTraditional,
        Portuguesa,
        Spanish,

        Max,
    }

    public class V2LocalData
    {
        public string key;
        public string msg;
    }

    public class StringLocalData
    {
        public string LocalStringID;

        public Dictionary<LocalizeType, string> LocalizeString = new Dictionary<LocalizeType, string>();

        public bool isOldString = false;
    }

    public class StringLocalChart : LocalTableBase
    {
        private Dictionary<string, StringLocalData> m_stringLocalDatas_Dic = new Dictionary<string, StringLocalData>();
        private List<LocalizeType> loadCompleteLanguage = new List<LocalizeType>();

        public override async UniTask InitData_Async()
        {
            int localtype = PlayerPrefs.GetInt(Define.DeviceLocalizeKey, -1);

            LocalizeType localizeType = LocalizeType.English;

            if (localtype == -1)
            {
                switch (Application.systemLanguage)
                {
                    case SystemLanguage.Korean:
                        {
                            localizeType = LocalizeType.Korean;
                            break;
                        }
                    case SystemLanguage.Japanese:
                        {
                            localizeType = LocalizeType.Japanese;
                            break;
                        }
                    case SystemLanguage.Chinese:
                    case SystemLanguage.ChineseSimplified:
                    case SystemLanguage.ChineseTraditional:
                        {
                            localizeType = LocalizeType.ChineseTraditional;
                            break;
                        }
                    case SystemLanguage.Portuguese:
                        {
                            localizeType = LocalizeType.Portuguesa;
                            break;
                        }
                    case SystemLanguage.Spanish:
                        {
                            localizeType = LocalizeType.Spanish;
                            break;
                        }
                    default:
                        {
                            localizeType = LocalizeType.English;
                            break;
                        }
                }

                PlayerPrefs.SetInt(Define.DeviceLocalizeKey, (int)localizeType);
                PlayerPrefs.Save();
            }
            else
            {
                localizeType = (LocalizeType)localtype;

                if (localizeType != LocalizeType.English
                    && localizeType != LocalizeType.Korean
                    && localizeType != LocalizeType.Japanese
                    && localizeType != LocalizeType.ChineseTraditional
                    && localizeType != LocalizeType.Portuguesa
                    && localizeType != LocalizeType.Spanish)
                {
                    localizeType = LocalizeType.English;
                }
            }

            await AddLanguage(localizeType, null);


            //await SetLocalData_Json_Async("Localstring.json", true);
            //await SetLocalData_Json_Async("DeviceLocalString.json", true);

            //await UniTask.WhenAll(
            //    SetLocalData_Json_Async("Localstring.json"),
            //    SetLocalData_Json_Async("DeviceLocalString.json"),

            //    SetLocalData_V2_Async("Local_Korean.json", LocalizeType.Korean),
            //    SetLocalData_V2_Async("Local_English.json", LocalizeType.English),
            //    SetLocalData_V2_Async("Local_Japanese.json", LocalizeType.Japanese),
            //    SetLocalData_V2_Async("Local_ChineseSimple.json", LocalizeType.ChineseTraditional),

            //    SetLocalData_V2_Async("Language_Korean.json", LocalizeType.Korean),
            //    SetLocalData_V2_Async("Language_English.json", LocalizeType.English),
            //    SetLocalData_V2_Async("Language_Japanese.json", LocalizeType.Japanese),
            //    SetLocalData_V2_Async("Language_ChineseSimple.json", LocalizeType.ChineseTraditional));
        }
        //------------------------------------------------------------------------------------
        public async UniTask AddLanguage(LocalizeType localizeType, System.Action onComplete)
        {
            if (loadCompleteLanguage.Contains(localizeType) == true)
            {
                onComplete?.Invoke();
                return;
            }

            switch (localizeType)
            {
                case LocalizeType.English:
                    {
                        await SetLocalData_V2_Async("Local_English.json", LocalizeType.English);
                        await SetLocalData_V2_Async("Language_English.json", LocalizeType.English);

                        break;
                    }
                case LocalizeType.Korean:
                    {
                        await SetLocalData_V2_Async("Local_Korean.json", LocalizeType.Korean);
                        await SetLocalData_V2_Async("Language_Korean.json", LocalizeType.Korean);

                        break;
                    }
                case LocalizeType.Japanese:
                    {
                        await SetLocalData_V2_Async("Local_Japanese.json", LocalizeType.Japanese);
                        await SetLocalData_V2_Async("Language_Japanese.json", LocalizeType.Japanese);

                        break;
                    }
                case LocalizeType.ChineseTraditional:
                    {
                        await SetLocalData_V2_Async("Local_ChineseSimple.json", LocalizeType.ChineseTraditional);
                        await SetLocalData_V2_Async("Language_ChineseSimple.json", LocalizeType.ChineseTraditional);

                        break;
                    }
                case LocalizeType.Portuguesa:
                    {
                        await SetLocalData_V2_Async("Local_Portuguese.json", LocalizeType.Portuguesa);
                        await SetLocalData_V2_Async("Language_Portuguese.json", LocalizeType.Portuguesa);

                        break;
                    }
                case LocalizeType.Spanish:
                    {
                        await SetLocalData_V2_Async("Local_Spanish.json", LocalizeType.Spanish);
                        await SetLocalData_V2_Async("Language_Spanish.json", LocalizeType.Spanish);

                        break;
                    }
            }

            loadCompleteLanguage.Add(localizeType);
            onComplete?.Invoke();
        }
        //------------------------------------------------------------------------------------
        private async UniTask SetLocalData_Json_Async(string json, bool isOldString = false)
        {
            string jsonstring = await ClientLocalChartManager.GetLocalChartData_Task(json);
            await UniTask.Yield();
            JsonData rows = JsonMapper.ToObject(jsonstring);
            await UniTask.Yield();
            for (int i = 0; i < rows.Count; ++i)
            {
                StringLocalData stringlocaldata = new StringLocalData();

                try
                {
                    stringlocaldata.LocalStringID = rows[i]["LocalstringID"].ToString();
                    stringlocaldata.LocalizeString.Add(LocalizeType.Korean, rows[i]["ko"].ToString().Replace("\\n", "\n"));
                    stringlocaldata.LocalizeString.Add(LocalizeType.English, rows[i]["en"].ToString().Replace("\\n", "\n"));
                    stringlocaldata.LocalizeString.Add(LocalizeType.Japanese, rows[i]["ja"].ToString().Replace("\\n", "\n"));
                    stringlocaldata.LocalizeString.Add(LocalizeType.ChineseTraditional, rows[i]["zhcht"].ToString().Replace("\\n", "\n"));
                    stringlocaldata.LocalizeString.Add(LocalizeType.Portuguesa, rows[i]["pt"].ToString().Replace("\\n", "\n"));
                    stringlocaldata.LocalizeString.Add(LocalizeType.Spanish, rows[i]["en"].ToString().Replace("\\n", "\n"));
                    stringlocaldata.isOldString = isOldString;
                }
                catch
                {
                    Debug.LogError(string.Format("LocalChart ÅÍÁü ¹üÀÎ : {0} ¹øÂ° LocalstringID : {1}", i, stringlocaldata.LocalStringID));
                }


                if (m_stringLocalDatas_Dic.ContainsKey(stringlocaldata.LocalStringID) == true)
                {
                    Debug.Log("ÀÌ»õ³¢´Ù" + stringlocaldata.LocalStringID);
                }
                else
                    m_stringLocalDatas_Dic.Add(stringlocaldata.LocalStringID, stringlocaldata);
            }
        }
        //------------------------------------------------------------------------------------
        private async UniTask SetLocalData_V2_Async(string json, LocalizeType localizeType)
        {
            string jsonstring = await ClientLocalChartManager.GetLocalChartData_Task(json);
            await UniTask.Yield();
            List<V2LocalData> v2LocalDatas = JsonConvert.DeserializeObject<List<V2LocalData>>(jsonstring);
            await UniTask.Yield();
            for (int i = 0; i < v2LocalDatas.Count; ++i)
            {
                if (m_stringLocalDatas_Dic.ContainsKey(v2LocalDatas[i].key) == false)
                    m_stringLocalDatas_Dic.Add(v2LocalDatas[i].key, new StringLocalData());

                StringLocalData stringLocalData = m_stringLocalDatas_Dic[v2LocalDatas[i].key];
                if (stringLocalData.LocalizeString.ContainsKey(localizeType) == false)
                {
                    stringLocalData.LocalStringID = v2LocalDatas[i].key;
                    stringLocalData.LocalizeString.Add(localizeType, v2LocalDatas[i].msg);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public StringLocalData GetLocalString(string id)
        {
            StringLocalData data = null;
            
            if (m_stringLocalDatas_Dic.ContainsKey(id) == true)
            {
                m_stringLocalDatas_Dic.TryGetValue(id, out data);
            }

            return data;
        }
        //------------------------------------------------------------------------------------
        public void AddLocalString(StringLocalData stringLocalData)
        {
            if (m_stringLocalDatas_Dic.ContainsKey(stringLocalData.LocalStringID) == true)
            {
            }
            else
                m_stringLocalDatas_Dic.Add(stringLocalData.LocalStringID, stringLocalData);
        }
        //------------------------------------------------------------------------------------
    }
}