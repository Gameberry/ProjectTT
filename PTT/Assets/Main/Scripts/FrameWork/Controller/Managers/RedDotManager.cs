using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.Managers
{
    public class RedDotManager : MonoSingleton<RedDotManager>
    {
        public Dictionary<ContentDetailList, string> m_redDotKey = new Dictionary<ContentDetailList, string>();
        public Dictionary<ContentDetailList, List<UIRedDotElement>> m_uiRedDotElement = new Dictionary<ContentDetailList, List<UIRedDotElement>>();

        public Dictionary<ContentDetailList, bool> m_redDotState = new Dictionary<ContentDetailList, bool>();

        //------------------------------------------------------------------------------------
        public void AddRedDotElement(UIRedDotElement uIRedDotElement)
        {
            if (uIRedDotElement == null)
                return;

            for (int i = 0; i < uIRedDotElement.m_myRecvRedDotType.Count; ++i)
            {
                if (uIRedDotElement.m_myRecvRedDotType[i] == ContentDetailList.None)
                    continue;

                List<UIRedDotElement> elemenlist = GetUIRedDotElements(uIRedDotElement.m_myRecvRedDotType[i]);
                elemenlist.Add(uIRedDotElement);
            }
        }
        //------------------------------------------------------------------------------------
        public void RemoveRedDotElement(UIRedDotElement uIRedDotElement)
        {
            if (uIRedDotElement == null)
                return;

            for (int i = 0; i < uIRedDotElement.m_myRecvRedDotType.Count; ++i)
            {
                if (uIRedDotElement.m_myRecvRedDotType[i] == ContentDetailList.None)
                    continue;

                List<UIRedDotElement> elemenlist = GetUIRedDotElements(uIRedDotElement.m_myRecvRedDotType[i]);
                elemenlist.Remove(uIRedDotElement);
            }
        }
        //------------------------------------------------------------------------------------
        private List<UIRedDotElement> GetUIRedDotElements(ContentDetailList contentDetailList)
        {
            if (m_uiRedDotElement.ContainsKey(contentDetailList) == false)
                m_uiRedDotElement.Add(contentDetailList, new List<UIRedDotElement>());

            List<UIRedDotElement> elemenlist = m_uiRedDotElement[contentDetailList];

            return elemenlist;
        }
        //------------------------------------------------------------------------------------
        public void SetMyRedDotState(ContentDetailList contentDetailList, bool state)
        {
            if (m_redDotState.ContainsKey(contentDetailList) == false)
                m_redDotState.Add(contentDetailList, state);
            else
                m_redDotState[contentDetailList] = state;
        }
        //------------------------------------------------------------------------------------
        public bool GetMyRedDotState(ContentDetailList contentDetailList)
        {
            if (m_redDotState.ContainsKey(contentDetailList) == false)
                return false;

            return m_redDotState[contentDetailList];

            //return PlayerPrefs.GetInt(GetRedDotSaveKey(contentDetailList), 0) == 1;
        }
        //------------------------------------------------------------------------------------
        public void ShowRedDot(ContentDetailList contentDetailList)
        {
            if (contentDetailList == ContentDetailList.None)
                return;

            if (GetMyRedDotState(contentDetailList) == true)
                return;

            //if (contentDetailList == ContentDetailList.Quest
            //    || contentDetailList == ContentDetailList.Quest_Daily
            //    || contentDetailList == ContentDetailList.Quest_Weekly)
            //{
            //    if (ContentOpenConditionManager.isAlive == true
            //        && ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Mission) == false)
            //        return;
            //}

            if (contentDetailList == ContentDetailList.PassDescendLevel)
            {
                if (ContentOpenConditionManager.isAlive == true
                    && ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.DescendPass) == false)
                    return;
            }
            else if(contentDetailList == ContentDetailList.PassMonsterKill)
            {
                if (ContentOpenConditionManager.isAlive == true
                    && ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.HuntingPass) == false)
                    return;
            }
            else if (contentDetailList == ContentDetailList.LobbyRelic)
            {
                if (ContentOpenConditionManager.isAlive == true
                    && ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Relic) == false)
                    return;
            }
            else if (contentDetailList == ContentDetailList.LobbyDescend)
            {
                if (ContentOpenConditionManager.isAlive == true
                    && ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Descend) == false)
                    return;
            }
            else if (contentDetailList == ContentDetailList.LobbySynergyRune)
            {
                if (ContentOpenConditionManager.isAlive == true
                    && ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Rune) == false)
                    return;
            }
            else if (contentDetailList == ContentDetailList.Quest)
            {
                if (ContentOpenConditionManager.isAlive == true
                    && ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Quest) == false)
                    return;
            }

            SetMyRedDotState(contentDetailList, true);
            //PlayerPrefs.SetInt(GetRedDotSaveKey(contentDetailList), 1);

            List<UIRedDotElement> elemenlist = GetUIRedDotElements(contentDetailList);

            for (int i = 0; i < elemenlist.Count; ++i)
            {
                elemenlist[i].VisibleRedDot(true);
            }
        }
        //------------------------------------------------------------------------------------
        public void HideRedDot(ContentDetailList contentDetailList)
        {
            if (contentDetailList == ContentDetailList.None)
                return;

            if (GetMyRedDotState(contentDetailList) == false)
                return;

            SetMyRedDotState(contentDetailList, false);
            //PlayerPrefs.SetInt(GetRedDotSaveKey(contentDetailList), 0);

            List<UIRedDotElement> elemenlist = GetUIRedDotElements(contentDetailList);

            for (int i = 0; i < elemenlist.Count; ++i)
            {
                elemenlist[i].VisibleRedDot(false);
            }
        }
        //------------------------------------------------------------------------------------
        public string GetRedDotSaveKey(ContentDetailList contentDetailList)
        {
            if (m_redDotKey.ContainsKey(contentDetailList) == false)
                m_redDotKey.Add(contentDetailList, string.Format("{0}{1}", Define.RedDotSaveKey, contentDetailList));

            return m_redDotKey[contentDetailList];
        }
        //------------------------------------------------------------------------------------
    }
}