using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Managers;

namespace GameBerry.UI
{
    public class UIInGameNoticeElement : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text m_postTitle;

        [SerializeField]
        private TMP_Text m_postSubTitle;

        [SerializeField]
        private TMP_Text m_postSendTime;

        [SerializeField]
        private Button m_postOpenButton;

        private NoticeData m_currentPostInfo = null;

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
        public void SetPostElement(NoticeData postInfo)
        {
            if (postInfo == null)
                return;

            m_currentPostInfo = postInfo;

            if (m_postTitle != null)
            {
                Managers.LocalStringManager.Instance.SetLocalizeText(m_postTitle, postInfo.title);

                //if (postInfo.IsShop == true)
                //{
                //    Managers.LocalStringManager.Instance.SetLocalizeText(m_postTitle, postInfo.Title);
                //}
                //else
                //{
                //    Managers.LocalStringManager.Instance.RemoveLocalizeText(m_postTitle);
                //    m_postTitle.text = postInfo.Title;
                //}
            }

            if (m_postSubTitle != null)
            {
                m_postSubTitle.gameObject.SetActive(true);

                string subtitle = Managers.LocalStringManager.Instance.GetLocalString(postInfo.contents);
                if (postInfo.contents.Contains("MaintenanceNoticeContents"))
                {
                    string[] arr = postInfo.contents.Split(',');
                    if (arr.Length >= 3)
                    {
                        System.DateTime startTime = System.DateTime.Parse(arr[1]).ToLocalTime();
                        System.DateTime endTime = System.DateTime.Parse(arr[2]).ToLocalTime();

                        subtitle = string.Format(Managers.LocalStringManager.Instance.GetLocalString(arr[0])
                            , startTime.ToString("yyyy-MM-dd HH:mm:ss")
                            , endTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                }

                if (subtitle.Length > 18)
                {
                    subtitle = subtitle.Substring(0, 17);
                    subtitle += "...";
                }

                m_postSubTitle.text = subtitle;
            }

            if (m_postSendTime != null)
                m_postSendTime.text = postInfo.postingDate.ToString();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_PostBtn()
        {
            if (m_action != null)
                m_action(m_currentPostInfo.inDate);
        }
        //------------------------------------------------------------------------------------
    }
}