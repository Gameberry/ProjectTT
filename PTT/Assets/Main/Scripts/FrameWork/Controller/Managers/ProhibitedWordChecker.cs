using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

namespace GameBerry
{
    public class ProhibitedWordChecker : MonoSingleton<ProhibitedWordChecker>
    {
        List<string> ProhibitedWordList = new List<string>();
        protected override void Init()
        {
            string jsonstring = ClientLocalChartManager.GetLocalChartData_V2("ProhibitedWord.json");

            JsonData chartJson = JsonMapper.ToObject(jsonstring);

            SetLocalData_Json(chartJson);
        }
        //------------------------------------------------------------------------------------
        private void SetLocalData_Json(JsonData chartJson)
        {
            for (int i = 0; i < chartJson.Count; ++i)
            {
                try
                {
                    ProhibitedWordList.Add(chartJson[i]["ForbiddenWord"].ToString().ToLower());
                }
                catch
                {
                    Debug.LogError(string.Format("LocalChart 터짐 범인 : {0} 번째", i));
                }
            }
        }
        //------------------------------------------------------------------------------------
        public bool CheckProhibitedWord(string checkstring, bool shownotice = true)
        {
            string lowerstr = checkstring.ToLower();
            for (int i = 0; i < ProhibitedWordList.Count; ++i)
            {
                if (lowerstr.Contains(ProhibitedWordList[i]))
                {
                    Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("PlayerNick_Message_ProhibitedWords"));
                    return false;
                }
            }

            return true;
        }
        //------------------------------------------------------------------------------------
    }
}