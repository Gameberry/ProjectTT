using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIChatBubbleElement : MonoBehaviour
    {
        [SerializeField]
        private Image rankerMark;

        [SerializeField]
        private Sprite rankerMarkSprite_1;

        [SerializeField]
        private Sprite rankerMarkSprite_2;

        [SerializeField]
        private Sprite rankerMarkSprite_3;

        [SerializeField]
        private Image m_profile;

        [SerializeField]
        private Image m_profile_Frame;

        [SerializeField]
        private Sprite rankerFrameSprite_1;

        [SerializeField]
        private Sprite rankerFrameSprite_2;

        [SerializeField]
        private Sprite rankerFrameSprite_Order;

        [SerializeField]
        private TMP_Text m_nameTitle;

        [SerializeField]
        private TMP_Text m_server;

        [SerializeField]
        private TMP_Text m_rank;

        [SerializeField]
        private TMP_Text m_contentText;

        [SerializeField]
        private Image m_contentBG;

        [SerializeField]
        private Image m_contentBGPoint;

        [SerializeField]
        private RectTransform m_contentRectTrans;

        [SerializeField]
        private TMP_Text m_viewText;

        [SerializeField]
        private ContentSizeFitter contentSizeFitter;

        [SerializeField]
        private float m_maxWidthSize = 809.0f;

        [SerializeField]
        private Button m_showUserInfo;

        [SerializeField]
        private Button m_showReportPopup;

        private string m_myUserUid = string.Empty;
        private int m_myRank = 0;
        private int m_myProfile = 0;
        private BackndChat.MessageInfo myMessageInfo = null;
        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (m_showUserInfo != null)
                m_showUserInfo.onClick.AddListener(OnClick_ShowUserInfoPopup);

            if (m_showReportPopup != null)
                m_showReportPopup.onClick.AddListener(OnClick_ShowReportPopup);
        }
        //------------------------------------------------------------------------------------
        public string GetUserID()
        {
            return m_myUserUid;
        }
        //------------------------------------------------------------------------------------
        public BackndChat.MessageInfo GetMessageInfo()
        {
            return myMessageInfo;
        }
        //------------------------------------------------------------------------------------
        public void OnEnable()
        {
            if (contentSizeFitter != null)
            {
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                contentSizeFitter.SetLayoutHorizontal();

                if (m_contentRectTrans.sizeDelta.x > m_maxWidthSize)
                {
                    contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                    Vector2 sizes = m_contentRectTrans.sizeDelta;
                    sizes.x = m_maxWidthSize;
                    m_contentRectTrans.sizeDelta = sizes;
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ShowUserInfoPopup()
        {
            if (myMessageInfo == null)
                return;

            Managers.ChatManager.Instance.ShowUserInfoPopup(myMessageInfo);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ShowReportPopup()
        {
            if (myMessageInfo == null)
                return;

            Managers.ChatManager.Instance.ShowReportPopup(myMessageInfo);
        }
        //------------------------------------------------------------------------------------
        public void SetBubble(BackndChat.MessageInfo messageInfo, string playerName, Color playerNameColor, string server, int rank, int profile, string contentText, Color contentColor, Color bgColor, string uid = "")
        {
            m_myUserUid = uid;
            m_myRank = rank;
            m_myProfile = profile;
            myMessageInfo = messageInfo;

            if (m_profile != null)
            {
                CharacterProfileData characterProfileData = Managers.PlayerDataManager.Instance.GetProfile(profile);
                if (characterProfileData == null)
                {
                    characterProfileData = Managers.PlayerDataManager.Instance.GetProfile(0);
                    if (characterProfileData == null)
                    {
                        m_profile.sprite = null;

                    }
                    else
                        m_profile.sprite = characterProfileData.sprite;
                }
                else
                    m_profile.sprite = characterProfileData.sprite;
            }

            if (m_nameTitle != null)
            { 
                m_nameTitle.text = string.Format("[{0}]", playerName);
                m_nameTitle.color = playerNameColor;
            }

            if (m_server != null)
                m_server.SetText(server);

            if (m_rank != null)
            {
                if (rank <= 0)
                    m_rank.SetText("Rank -", rank);
                else
                    m_rank.SetText("Rank {0}", rank);
            }

            if (rankerMark != null)
            {
                if (rank == 1)
                {
                    rankerMark.gameObject.SetActive(true);
                    rankerMark.sprite = rankerMarkSprite_1;
                }
                else if (rank == 2)
                {
                    rankerMark.gameObject.SetActive(true);
                    rankerMark.sprite = rankerMarkSprite_2;
                }
                else if (rank == 3)
                {
                    rankerMark.gameObject.SetActive(true);
                    rankerMark.sprite = rankerMarkSprite_3;
                }
                else
                    rankerMark.gameObject.SetActive(false);
            }

            if (m_profile_Frame != null)
            {
                if (rank == 1)
                {
                    m_profile_Frame.sprite = rankerFrameSprite_1;
                }
                else if (rank == 2)
                {
                    m_profile_Frame.sprite = rankerFrameSprite_2;
                }
                else
                    m_profile_Frame.sprite = rankerFrameSprite_Order;
            }

            if (m_contentText != null)
            {
                m_contentText.text = contentText;

                if (contentSizeFitter != null)
                {
                    contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                    contentSizeFitter.SetLayoutHorizontal();

                    if (m_contentRectTrans.sizeDelta.x > m_maxWidthSize)
                    {
                        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                        Vector2 sizes = m_contentRectTrans.sizeDelta;
                        sizes.x = m_maxWidthSize;
                        m_contentRectTrans.sizeDelta = sizes;
                    }
                }
            }
                

            if (m_viewText != null)
            {
                m_viewText.text = contentText;
                m_viewText.color = contentColor;
            }

            if (m_contentBG != null)
                m_contentBG.color = bgColor;

            if (m_contentBGPoint != null)
                m_contentBGPoint.color = bgColor;
        }
        //------------------------------------------------------------------------------------
        public void SetCloneBubble(UIChatBubbleElement uIChatBubbleElement, bool isrecent = false)
        {
            if (uIChatBubbleElement == null)
                return;

            string playerName = string.Empty;
            Color playerNameColor = Color.white;

            string server = string.Empty;

            string contentText = string.Empty;
            Color contentColor = Color.white;
            Color bgColor = Color.white;
            string uid = string.Empty;

            if (m_nameTitle != null)
            {
                playerName = m_nameTitle.text;
                playerNameColor = m_nameTitle.color;
            }

            if (m_server != null)
                server = m_server.text;

            if (m_contentText != null)
            {
                contentText = m_contentText.text;
                contentColor = m_contentText.color;
            }

            if (m_contentBG != null)
                bgColor = m_contentBG.color;

            if (isrecent == false)
                uid = m_myUserUid;
            else
            {
                if (contentText.Length > 20)
                {
                    contentText = contentText.Substring(0, 19);
                    contentText += "...";
                }
            }

            uIChatBubbleElement.SetBubble(myMessageInfo, playerName, playerNameColor, server, m_myRank, m_myProfile, contentText, contentColor, bgColor, uid);
        }
        //------------------------------------------------------------------------------------
        public void SetReset()
        {
            m_myUserUid = string.Empty;
        }
        //------------------------------------------------------------------------------------
    }
}