using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class InGameExchangeDialog : IDialog
    {
        public static bool IsEnterState = false;

        [SerializeField]
        private List<Button> m_exitBtn;

        [SerializeField]
        private TMP_Text m_initTime;

        [SerializeField]
        private UIExchangeElement m_uIExchangeElement;

        [SerializeField]
        private Transform m_scrollRectContent;

        private Dictionary<ExchangeData, UIExchangeElement> m_exchangeDatas = new Dictionary<ExchangeData, UIExchangeElement>();

        protected override void OnLoad()
        {
            if (m_exitBtn != null)
            {
                for (int i = 0; i < m_exitBtn.Count; ++i)
                {
                    if (m_exitBtn[i] != null)
                        m_exitBtn[i].onClick.AddListener(() =>
                        {
                            RequestDialogExit<InGameExchangeDialog>();
                        });
                }
            }

            List<ExchangeData> exchangeDatas = Managers.ExchangeManager.Instance.GetExchangeDatas();

            for (int i = 0; i < exchangeDatas.Count; ++i)
            {
                GameObject clone = Instantiate(m_uIExchangeElement.gameObject, m_scrollRectContent);

                UIExchangeElement uIExchangeElement = clone.GetComponent<UIExchangeElement>();
                uIExchangeElement.SetExchangeElement(exchangeDatas[i]);
                uIExchangeElement.SetExchangeCount(1);

                m_exchangeDatas.Add(exchangeDatas[i], uIExchangeElement);
            }

            Managers.TimeManager.Instance.RemainInitDailyContent_Text += SetInitInterval;

            Message.AddListener<GameBerry.Event.RefreshExchangeDataMsg>(RefreshExchangeData);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.RefreshExchangeDataMsg>(RefreshExchangeData);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            foreach (var pair in m_exchangeDatas)
            {
                UIExchangeElement uIExchangeElement = pair.Value;
                uIExchangeElement.RefreshExchangeCount();
            }

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.CheckExchange)
            {
                Managers.GuideQuestManager.Instance.AddEventCount(V2Enum_EventType.CheckExchange, 1);
            }

            IsEnterState = true;
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            IsEnterState = false;
        }
        //------------------------------------------------------------------------------------
        private void RefreshExchangeData(GameBerry.Event.RefreshExchangeDataMsg msg)
        {
            //for (int i = 0; i < msg.exchangeDatas.Count; ++i)
            //{
            //    if (m_exchangeDatas.ContainsKey(msg.exchangeDatas[i]) == true)
            //    {
            //        m_exchangeDatas[msg.exchangeDatas[i]].SetExchangeElement(msg.exchangeDatas[i]);
            //    }
            //}

            foreach (var pair in m_exchangeDatas)
            {
                UIExchangeElement uIExchangeElement = pair.Value;
                uIExchangeElement.RefreshExchangeCount();
            }
        }
        //------------------------------------------------------------------------------------
        private void SetInitInterval(string remaintime)
        {
            m_initTime?.SetText(remaintime);
        }
        //------------------------------------------------------------------------------------
    }
}