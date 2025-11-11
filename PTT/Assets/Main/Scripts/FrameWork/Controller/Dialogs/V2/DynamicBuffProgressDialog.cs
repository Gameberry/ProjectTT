using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class DynamicBuffProgressDialog : IDialog
    {
        [SerializeField]
        private Transform m_elementRoot;

        [SerializeField]
        private UIDynamicBuffProgressElement m_uIDynamicBuffProgressElement;

        private Queue<UIDynamicBuffProgressElement> m_uIBuffProgressElements = new Queue<UIDynamicBuffProgressElement>();

        private Dictionary<int, UIDynamicBuffProgressElement> m_showUIBuffProgressElement_Dic = new Dictionary<int, UIDynamicBuffProgressElement>();

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            Message.AddListener<GameBerry.Event.RefreshDynamicBuffProgressMsg>(RefreshDynamicBuffProgress);
            Message.AddListener<GameBerry.Event.EndDynamicBuffProgressMsg>(EndDynamicBuffProgressMsg);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.RefreshDynamicBuffProgressMsg>(RefreshDynamicBuffProgress);
            Message.RemoveListener<GameBerry.Event.EndDynamicBuffProgressMsg>(EndDynamicBuffProgressMsg);
        }
        //------------------------------------------------------------------------------------
        private UIDynamicBuffProgressElement GetUIDynamicBuffProgressElement()
        {
            if (m_uIBuffProgressElements.Count > 0)
                return m_uIBuffProgressElements.Dequeue();

            GameObject clone = Instantiate(m_uIDynamicBuffProgressElement.gameObject, m_elementRoot);

            UIDynamicBuffProgressElement uIDynamicBuffProgressElement = clone.GetComponent<UIDynamicBuffProgressElement>();
            uIDynamicBuffProgressElement.gameObject.SetActive(false);

            return uIDynamicBuffProgressElement;
        }
        //------------------------------------------------------------------------------------
        private void PoolUIDynamicBuffProgressElement(UIDynamicBuffProgressElement uIDynamicBuffProgressElement)
        {
            if (uIDynamicBuffProgressElement == null)
                return;

            m_uIBuffProgressElements.Enqueue(uIDynamicBuffProgressElement);
            uIDynamicBuffProgressElement.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        private void RefreshDynamicBuffProgress(GameBerry.Event.RefreshDynamicBuffProgressMsg msg)
        {
            UIDynamicBuffProgressElement uIDynamicBuffProgressElement = null;

            if (m_showUIBuffProgressElement_Dic.ContainsKey(msg.Index) == false)
            {
                uIDynamicBuffProgressElement = GetUIDynamicBuffProgressElement();
                uIDynamicBuffProgressElement.transform.SetAsLastSibling();
                uIDynamicBuffProgressElement.gameObject.SetActive(true);
                m_showUIBuffProgressElement_Dic.Add(msg.Index, uIDynamicBuffProgressElement);
            }
            else
                uIDynamicBuffProgressElement = m_showUIBuffProgressElement_Dic[msg.Index];

            uIDynamicBuffProgressElement.SetBuffProgressElement(msg.Icon, msg.RamainTime, msg.ApplyTime, msg.BuffStack);
        }
        //------------------------------------------------------------------------------------
        private void EndDynamicBuffProgressMsg(GameBerry.Event.EndDynamicBuffProgressMsg msg)
        {
            if (m_showUIBuffProgressElement_Dic.ContainsKey(msg.Index) == false)
                return;

            UIDynamicBuffProgressElement uIDynamicBuffProgressElement = m_showUIBuffProgressElement_Dic[msg.Index];
            m_showUIBuffProgressElement_Dic.Remove(msg.Index);

            PoolUIDynamicBuffProgressElement(uIDynamicBuffProgressElement);
        }
        //------------------------------------------------------------------------------------
    }
}