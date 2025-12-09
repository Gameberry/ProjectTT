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
        private StringLocalChart _stringLocalChart = null;
        private LocalizeType _localizeType = LocalizeType.Korean;
        private Dictionary<TMP_Text, string> _localizeUIs = new Dictionary<TMP_Text, string>();

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
            _stringLocalChart = LocalTableManager.Instance.GetTableClass<StringLocalChart>();

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

            _localizeType = localizeType;

            ChangeLocalize(localizeType);
        }
        //------------------------------------------------------------------------------------
        public void SetLocalizeText(TMP_Text textmesh, string id)
        {
            if (_localizeUIs.ContainsKey(textmesh) == true)
                _localizeUIs[textmesh] = id;
            else
                _localizeUIs.Add(textmesh, id);

#if DEV_DEFINE
            StringLocalData data = _stringLocalChart.GetLocalString(id);

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
            if (_localizeUIs.ContainsKey(textmesh) == true)
                _localizeUIs.Remove(textmesh);
        }
        //------------------------------------------------------------------------------------
        public string GetLocalString(string id)
        {
            if (_stringLocalChart == null)
                return id;

            StringLocalData data = _stringLocalChart.GetLocalString(id);

            if (data == null)
                return id;

            if (data.LocalizeString.ContainsKey(_localizeType) == true)
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
                return data.LocalizeString[_localizeType];
            }
            else
            {
                if (_localizeType != LocalizeType.English
                    && _localizeType != LocalizeType.Korean
                    && _localizeType != LocalizeType.Japanese
                    && _localizeType != LocalizeType.ChineseTraditional
                    && _localizeType != LocalizeType.Portuguesa
                    && _localizeType != LocalizeType.Spanish)
                {
                    ChangeLocalize(LocalizeType.English);
                    if (data.LocalizeString.ContainsKey(_localizeType) == true)
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
                        return data.LocalizeString[_localizeType];
                    }
                }

                return id;
            }
        }
        //------------------------------------------------------------------------------------
        public void ChangeLocalize(LocalizeType type)
        {
            _stringLocalChart.AddLanguage(type, () =>
            {
                _localizeType = type;

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
            foreach (KeyValuePair<TMP_Text, string> pair in _localizeUIs)
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
            return _localizeType;
        }
        //------------------------------------------------------------------------------------
    }
}