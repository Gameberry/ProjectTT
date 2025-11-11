using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIPostElement : MonoBehaviour
    {
        [SerializeField]
        private Image m_postIcon;

        [SerializeField]
        private TMP_Text m_postTitle;

        [SerializeField]
        private TMP_Text m_postSubTitle;

        [SerializeField]
        private TMP_Text m_postSendTime;

        [SerializeField]
        private Button m_postOpenButton;

        private PostInfo m_currentPostInfo = null;

        private System.Action<string> m_action = null;

        //------------------------------------------------------------------------------------
        public void Init(System.Action<string> action)
        {
            m_action = action;

            if (m_postOpenButton != null)
                m_postOpenButton.onClick.AddListener(OnClick_PostBtn);

            if (Managers.LocalStringManager.isAlive == true)
                Managers.LocalStringManager.Instance.RefreshLocalString += RefreshLocalize;
        }
        //------------------------------------------------------------------------------------
        private void RefreshLocalize()
        {
            SetPostElement(m_currentPostInfo);
        }
        //------------------------------------------------------------------------------------
        public void SetPostElement(PostInfo postInfo)
        {
            if (postInfo == null)
                return;

            m_currentPostInfo = postInfo;

            if (m_postTitle != null)
            {
                Managers.LocalStringManager.Instance.SetLocalizeText(m_postTitle, postInfo.Title);
            }

            if (m_postSubTitle != null)
            {
                if (postInfo.IsShop == true)
                {
                    m_postSubTitle.gameObject.SetActive(true);
                    string subtitle = Managers.LocalStringManager.Instance.GetLocalString(postInfo.Content);
                    if (subtitle.Length > 18)
                    {
                        subtitle = subtitle.Substring(0, 17);
                        subtitle += "...";
                    }

                    m_postSubTitle.text = subtitle;
                }
                else
                {
                    m_postSubTitle.gameObject.SetActive(false);
                    //Managers.LocalStringManager.Instance.RemoveLocalizeText(m_postTitle);
                    //m_postTitle.text = postInfo.Title;
                }
                
            }

            if (m_postSendTime != null)
                m_postSendTime.text = postInfo.SentDate.ToString();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_PostBtn()
        {
            if (m_action != null)
                m_action(m_currentPostInfo.InData);
        }
        //------------------------------------------------------------------------------------
        public void ReleaseData()
        {
            m_currentPostInfo = null;
        }
        //------------------------------------------------------------------------------------
        [ContextMenu("SetLastSibling")]
        private void SetLastSibling()
        {
            transform.SetAsLastSibling();
        }
        //------------------------------------------------------------------------------------
        [ContextMenu("SetFirstSibling")]
        private void SetFirstSibling()
        {
            transform.SetAsFirstSibling();
        }
        //------------------------------------------------------------------------------------
        public PostInfo GetPostInfo()
        {
            return m_currentPostInfo;
        }
        //------------------------------------------------------------------------------------
    }
}