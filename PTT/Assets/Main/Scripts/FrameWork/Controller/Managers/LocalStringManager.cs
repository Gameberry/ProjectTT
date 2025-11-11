using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;

namespace GameBerry.Managers
{
    [System.Serializable]
    public class Showlocallog
    {
        public string key;

        public Dictionary<TMP_Text, TMP_Text> checkDic = new Dictionary<TMP_Text, TMP_Text>();
        public List<TMP_Text> objName = new List<TMP_Text>();
    }

    public class LocalStringManager : MonoSingleton<LocalStringManager>
    {
        private StringLocalChart m_stringLocalChart = null;
        private LocalizeType m_localizeType = LocalizeType.Korean;
        private Dictionary<TMP_Text, string> m_localizeUIs = new Dictionary<TMP_Text, string>();

#if DEV_DEFINE
        public Dictionary<string, Showlocallog> OldString = new Dictionary<string, Showlocallog>();
        public List<string> OldLocalKey = new List<string>();
        public List<Showlocallog> OldLocalLog = new List<Showlocallog>();
#endif

        private event System.Action _refreshLocalString;
        public event System.Action RefreshLocalString
        {
            add { _refreshLocalString += value; }
            remove { _refreshLocalString -= value; }
        }

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            m_stringLocalChart = TableManager.Instance.GetTableClass<StringLocalChart>();

            int localtype = PlayerPrefs.GetInt(Define.DeviceLocalizeKey, -1);

            LocalizeType localizeType = LocalizeType.Korean;

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

            m_localizeType = localizeType;

            ChangeLocalize(localizeType);
        }
        //------------------------------------------------------------------------------------
        public void SetLocalizeText(TMP_Text textmesh, string id)
        {
            if (m_localizeUIs.ContainsKey(textmesh) == true)
                m_localizeUIs[textmesh] = id;
            else
                m_localizeUIs.Add(textmesh, id);

#if DEV_DEFINE
            StringLocalData data = m_stringLocalChart.GetLocalString(id);

            if (data != null)
            {
                if (data.isOldString == true)
                {

                    if (OldString.ContainsKey(id) == false)
                    {
                        Showlocallog showlocallog = new Showlocallog();
                        showlocallog.key = id;
                        showlocallog.checkDic.Add(textmesh, textmesh);
                        showlocallog.objName.Add(textmesh);

                        OldString.Add(id, showlocallog);
                        OldLocalKey.Add(id);
                        OldLocalLog.Add(showlocallog);
                    }
                    else
                    {
                        if (OldString[id].checkDic.ContainsKey(textmesh) == false)
                        { 
                            OldString[id].checkDic.Add(textmesh, textmesh);
                            OldString[id].objName.Add(textmesh);
                        }
                    }

                }
            }
#endif
            textmesh.SetText(GetLocalString(id));
        }
        //------------------------------------------------------------------------------------
        public void RemoveLocalizeText(TMP_Text textmesh)
        {
            if (m_localizeUIs.ContainsKey(textmesh) == true)
                m_localizeUIs.Remove(textmesh);
        }
        //------------------------------------------------------------------------------------
        public string GetLocalString(string id)
        {
            if (m_stringLocalChart == null)
                return id;

            StringLocalData data = m_stringLocalChart.GetLocalString(id);

            if (data == null)
                return id;

            if (data.LocalizeString.ContainsKey(m_localizeType) == true)
            {
#if DEV_DEFINE
                if (data.isOldString == true)
                {
                    if (OldString.ContainsKey(id) == false)
                    {
                        Showlocallog showlocallog = new Showlocallog();
                        showlocallog.key = id;

                        OldString.Add(id, showlocallog);
                        OldLocalKey.Add(id);
                        OldLocalLog.Add(showlocallog);
                    }
                }
#endif
                return data.LocalizeString[m_localizeType];
            }
            else
            {
                if (m_localizeType != LocalizeType.English
                    && m_localizeType != LocalizeType.Korean
                    && m_localizeType != LocalizeType.Japanese
                    && m_localizeType != LocalizeType.ChineseTraditional
                    && m_localizeType != LocalizeType.Portuguesa
                    && m_localizeType != LocalizeType.Spanish)
                {
                    ChangeLocalize(LocalizeType.English);
                    if (data.LocalizeString.ContainsKey(m_localizeType) == true)
                    {
#if DEV_DEFINE
                        if (data.isOldString == true)
                        {
                            if (OldString.ContainsKey(id) == false)
                            {
                                Showlocallog showlocallog = new Showlocallog();
                                showlocallog.key = id;

                                OldString.Add(id, showlocallog);

                                OldLocalKey.Add(id);
                                OldLocalLog.Add(showlocallog);
                            }
                        }
#endif
                        return data.LocalizeString[m_localizeType];
                    }
                }

                return id;
            }
        }
        //------------------------------------------------------------------------------------
        public void ChangeLocalize(LocalizeType type)
        {
            m_stringLocalChart.AddLanguage(type, () =>
            {
                m_localizeType = type;

                ApplyAllLocalizeText();

                if (_refreshLocalString != null)
                    _refreshLocalString();

                PlayerPrefs.SetInt(Define.DeviceLocalizeKey, (int)type);
                PlayerPrefs.Save();
            }).Forget();
        }
        //------------------------------------------------------------------------------------
        private void ApplyAllLocalizeText()
        {
            foreach (KeyValuePair<TMP_Text, string> pair in m_localizeUIs)
            {
                if (pair.Key != null)
                    pair.Key.text = GetLocalString(pair.Value);
            }
        }
        //------------------------------------------------------------------------------------
#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Keypad1))
            {
                ChangeLocalize(LocalizeType.Korean);
            }

            if (Input.GetKeyUp(KeyCode.Keypad2))
            {
                ChangeLocalize(LocalizeType.English);
            }

            if (Input.GetKeyUp(KeyCode.Keypad3))
            {
                ChangeLocalize(LocalizeType.Japanese);
            }

            if (Input.GetKeyUp(KeyCode.Keypad4))
            {
                ChangeLocalize(LocalizeType.ChineseTraditional);
            }

            if (Input.GetKeyUp(KeyCode.Keypad5))
            {
                ChangeLocalize(LocalizeType.Portuguesa);
            }

            if (Input.GetKeyUp(KeyCode.Keypad6))
            {
                ChangeLocalize(LocalizeType.Spanish);
            }

#if DEV_DEFINE
            if (Input.GetKeyUp(KeyCode.Keypad7))
            {
                string str = string.Empty;
                for (int i = 0; i < OldLocalKey.Count; ++i)
                {
                    str += OldLocalKey[i];
                    if (i != OldLocalKey.Count - 1)
                        str += ",";
                }

                Debug.Log(str);
            }
#endif
        }
        //------------------------------------------------------------------------------------
#endif
        public LocalizeType GetLocalizeType()
        {
            return m_localizeType;
        }
        //------------------------------------------------------------------------------------
    }
}