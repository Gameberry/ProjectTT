using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Managers;

namespace GameBerry.UI
{
    public class InGameNoticeDialog : IDialog
    {
        [Header("------------ElementGroup------------")]
        [SerializeField]
        private Image m_elementGroupFrame;

        [SerializeField]
        private UIInGameNoticeElement m_postElement;

        [SerializeField]
        private RectTransform m_scrollRectContent;

        private List<UIInGameNoticeElement> m_uIPostElements = new List<UIInGameNoticeElement>();

        [Header("------------PostDetail------------")]
        [SerializeField]
        private UIInGameNoticeDetailElement m_uIPostDetailElement;
        protected override void OnLoad()
        {
            Message.AddListener<GameBerry.Event.RefreshNoticeMsg>(RefreshNotice);

            RefreshNotice(null);
        }
        //------------------------------------------------------------------------------------

        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.RefreshNoticeMsg>(RefreshNotice);
        }
        //------------------------------------------------------------------------------------
        private void RefreshNotice(GameBerry.Event.RefreshNoticeMsg msg)
        {
            List<NoticeData> noticeDatas = NoticeManager.Instance.noticeList;

            for (int i = 0; i < noticeDatas.Count; ++i)
            {
                UIInGameNoticeElement uIInGameNoticeElement = null;
                if (m_uIPostElements.Count > i)
                {
                    uIInGameNoticeElement = m_uIPostElements[i];
                }
                else
                {
                    GameObject clone = Instantiate(m_postElement.gameObject, m_scrollRectContent);
                    uIInGameNoticeElement = clone.GetComponent<UIInGameNoticeElement>();
                    if (uIInGameNoticeElement != null)
                    {
                        uIInGameNoticeElement.Init(OnClick_PostDetail);
                    }
                    m_uIPostElements.Add(uIInGameNoticeElement);
                }

                uIInGameNoticeElement.gameObject.SetActive(true);
                uIInGameNoticeElement.SetPostElement(noticeDatas[i]);
            }

            for (int i = noticeDatas.Count; i < m_uIPostElements.Count; ++i)
            {
                m_uIPostElements[i].gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_PostDetail(string indata)
        {
            NoticeData noticeData = NoticeManager.Instance.noticeList.Find(x => x.inDate == indata);

            if (noticeData == null)
                return;

            if (m_uIPostDetailElement != null)
            {
                m_uIPostDetailElement.gameObject.SetActive(true);

                m_uIPostDetailElement.SetPostView(noticeData);
            }
        }
        //------------------------------------------------------------------------------------
    }
}