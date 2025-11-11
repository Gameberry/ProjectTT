using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Managers;

namespace GameBerry.UI
{
    public class UIInGameNoticeDetailElement : MonoBehaviour
    {
        [Header("------------PostDetail------------")]
        [SerializeField]
        private TMP_Text m_postTitle;

        [SerializeField]
        private TMP_Text m_postContent;

        [SerializeField]
        private List<Button> m_exitBtn;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (m_exitBtn != null)
            {
                for (int i = 0; i < m_exitBtn.Count; ++i)
                {
                    if (m_exitBtn[i] != null)
                        m_exitBtn[i].onClick.AddListener(OnClick_ExitBtn);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void SetPostView(NoticeData postInfo)
        {
            if (postInfo == null)
                return;

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

            if (m_postContent != null)
            {
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

                m_postContent.text = subtitle;

                //if (postInfo.IsShop == true)
                //{
                //    Managers.LocalStringManager.Instance.SetLocalizeText(m_postContent, postInfo.Content);
                //}
                //else
                //{
                //    Managers.LocalStringManager.Instance.RemoveLocalizeText(m_postContent);
                //    m_postContent.text = postInfo.Content;
                //}
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ExitBtn()
        {
            gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
    }
}