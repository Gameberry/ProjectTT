using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace GameBerry.UI
{
    public class UIGlobalNoticeElement : MonoBehaviour
    {
        protected Coroutine HideDirectionCoroutine;

        [SerializeField]
        protected CanvasGroup m_messageCanvasGroup;

        [SerializeField]
        protected TMP_Text m_noticeMessage;

        private WaitForSeconds m_noticeTurm = new WaitForSeconds(1.5f);

        private WaitForSeconds m_noticeAdminTurm = new WaitForSeconds(3.0f);

        private float m_playAlphaDuration = 0.5f;

        private System.Action<UIGlobalNoticeElement> m_endCallBack = null;

        //------------------------------------------------------------------------------------
        public void Init(System.Action<UIGlobalNoticeElement> callBack)
        {
            m_endCallBack = callBack;
        }
        //------------------------------------------------------------------------------------
        public void ShowNoticeElement(string notice, float duration)
        {
            if (m_noticeMessage != null)
                m_noticeMessage.text = notice;

            if (m_messageCanvasGroup != null)
                m_messageCanvasGroup.alpha = 1.0f;

            gameObject.SetActive(true);

            StartCoroutine(HideDirection(duration));
        }
        //------------------------------------------------------------------------------------
        public void ShowNoticeElement_Guide(string notice, float duration)
        {
            if (m_noticeMessage != null)
                m_noticeMessage.text = notice;

            if (m_messageCanvasGroup != null)
                m_messageCanvasGroup.alpha = 1.0f;

            gameObject.SetActive(true);

            if (HideDirectionCoroutine != null)
                StopCoroutine(HideDirectionCoroutine);

            HideDirectionCoroutine = StartCoroutine(HideDirection(duration));
        }
        //------------------------------------------------------------------------------------
        protected IEnumerator HideDirection(float duration)
        {
            float starttime = Time.time;
            float endtime = Time.time + duration;

            while (endtime >= Time.time)
            {
                yield return null;
            }

            starttime = Time.time;
            endtime = Time.time + m_playAlphaDuration;

            while (endtime >= Time.time)
            {
                float ratio = (endtime - Time.time) / m_playAlphaDuration;

                if (m_messageCanvasGroup != null)
                    m_messageCanvasGroup.alpha = ratio;

                yield return null;
            }

            ReleaseElement();
        }
        //------------------------------------------------------------------------------------
        protected IEnumerator HideDirection(WaitForSeconds turm)
        {
            yield return turm;

            float starttime = Time.time;
            float endtime = Time.time + m_playAlphaDuration;

            while (endtime >= Time.time)
            {
                float ratio = (endtime - Time.time) / m_playAlphaDuration;

                if (m_messageCanvasGroup != null)
                    m_messageCanvasGroup.alpha = ratio;

                yield return null;
            }

            ReleaseElement();
        }
        //------------------------------------------------------------------------------------
        public void ForceReleaseHide()
        {
            if (HideDirectionCoroutine != null)
                StopCoroutine(HideDirectionCoroutine);
        }
        //------------------------------------------------------------------------------------
        private void ReleaseElement()
        {
            if (m_endCallBack != null)
                m_endCallBack(this);
        }
        //------------------------------------------------------------------------------------
    }
}