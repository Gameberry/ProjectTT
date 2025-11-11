using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class ChatViewDialog : IDialog
    {
        [Header("---------ChatColor---------")]
        [SerializeField]
        private Color m_myNameColor;

        [SerializeField]
        private Color m_myContentColor;

        [SerializeField]
        private Color m_myBGColor;

        [SerializeField]
        private Color m_orderNameColor;

        [SerializeField]
        private Color m_orderContentColor;

        [SerializeField]
        private Color m_orderBGColor;

        [SerializeField]
        private Color m_systemNameColor;

        [SerializeField]
        private Color m_systemContentColor;

        [SerializeField]
        private Color m_systemBGColor;

        [Header("---------MiniMum---------")]
        [SerializeField]
        private Transform m_miniMumGroup;

        [SerializeField]
        private Button m_showMaximumContnetn;

        [SerializeField]
        private UIChatBubbleElement m_recentChatBubble;

        [SerializeField]
        private Button m_chatJoin;

        [SerializeField]
        private TMP_Text m_chatJoinState;

        [Header("---------MaxiMum---------")]
        [SerializeField]
        private Transform m_maxiMumGroup;

        [SerializeField]
        private Button m_showMinimumContnetn;

        [SerializeField]
        private UIChatBubbleElement m_chatBubbleElement;

        [SerializeField]
        private Transform m_uIChatBubbleElementRoot;

        [SerializeField]
        private int m_limitChar = 50;

        [SerializeField]
        private int m_limitChatCount = 50;
        private List<UIChatBubbleElement> m_uIChatBubbleElementPool = new List<UIChatBubbleElement>();

        [SerializeField]
        private TMP_InputField m_InputChatString;

        [SerializeField]
        private Button m_showBlockListBtn;

        [SerializeField]
        private Button m_sendChat;

        [SerializeField]
        private Button m_guildChat;

        [SerializeField]
        private TMP_Dropdown m_languageSelect;

        private GameBerry.Event.SetGuildChatStateMsg setGuildChatStateMsg = new GameBerry.Event.SetGuildChatStateMsg();

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            Message.AddListener<GameBerry.Event.ShowGlobalChatMessageMsg>(ShowGlobalChatMessage);
            Message.AddListener<GameBerry.Event.ShowSystemMessageMsg>(ShowSystemMessage);
            Message.AddListener<GameBerry.Event.RefreshBlockUserMsg>(RefreshBlockUser);
            Message.AddListener<GameBerry.Event.SetGlobalChatStateMsg>(SetGlobalChatState);

            Message.AddListener<GameBerry.Event.OnConnectinghannelGlobalChatMsg>(OnConnectinghannelGlobalChat);
            Message.AddListener<GameBerry.Event.OnJoinChannelGlobalChatMsg>(OnJoinChannelGlobalChat);
            Message.AddListener<GameBerry.Event.OnLeaveChannelGlobalChatMsg>(OnLeaveChannelGlobalChat);

            if (m_showMinimumContnetn != null)
                m_showMinimumContnetn.onClick.AddListener(OnClick_ShowMinimumContent);

            if (m_showMaximumContnetn != null)
                m_showMaximumContnetn.onClick.AddListener(OnClick_ShowMaximumContent);

            if (m_InputChatString != null)
            { 
                m_InputChatString.characterLimit = m_limitChar;
                m_InputChatString.onSubmit.AddListener(SendChat);
            }

            if (m_showBlockListBtn != null)
                m_showBlockListBtn.onClick.AddListener(OnClick_m_showBlockListBtn);

            if (m_sendChat != null)
                m_sendChat.onClick.AddListener(SendChat);

            if (m_chatJoin != null)
                m_chatJoin.onClick.AddListener(OnClick_ChatJoin);

            if (m_guildChat != null)
                m_guildChat.onClick.AddListener(() =>
                {
                    //if (GuildContainer.JoinedGuild == true)
                    //{
                    //    RequestDialogEnter<ChatGuildDialog>();
                    //    RequestDialogExit<ChatViewDialog>();
                    //    OnClick_ShowMinimumContent();

                    //    Message.Send(setGuildChatStateMsg);
                    //}
                    //else
                    //    Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("guild/search/fail"));
                });

            if (m_chatJoinState != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(m_chatJoinState, "chat/serverAccess");

            if (m_languageSelect != null)
            {
                m_languageSelect.ClearOptions();
                List<string> optiondatalabel = new List<string>();

                for (int i = (int)LocalizeType.English; i < (int)LocalizeType.Max; ++i)
                {
                    LocalizeType localizeType = i.IntToEnum32<LocalizeType>();
                    string localstr = string.Empty;
                    if (localizeType == LocalizeType.English)
                        localstr = Managers.LocalStringManager.Instance.GetLocalString("Setting_UI_Game_En");
                    else if (localizeType == LocalizeType.Korean)
                        localstr = Managers.LocalStringManager.Instance.GetLocalString("Setting_UI_Game_Ko");
                    if (localizeType == LocalizeType.Japanese)
                        localstr = Managers.LocalStringManager.Instance.GetLocalString("Setting_UI_Game_Ja");
                    if (localizeType == LocalizeType.ChineseTraditional)
                        localstr = Managers.LocalStringManager.Instance.GetLocalString("Setting_UI_Game_Zhcht");
                    if (localizeType == LocalizeType.Portuguesa)
                        localstr = Managers.LocalStringManager.Instance.GetLocalString("Setting_UI_Game_Pt");
                    if (localizeType == LocalizeType.Spanish)
                        localstr = Managers.LocalStringManager.Instance.GetLocalString("Setting_UI_Game_Sp");

                    optiondatalabel.Add(localstr);
                }

                m_languageSelect.AddOptions(optiondatalabel);
                m_languageSelect.onValueChanged.AddListener(OnValueChange_ChatLocalType);
            }
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.ShowGlobalChatMessageMsg>(ShowGlobalChatMessage);
            Message.RemoveListener<GameBerry.Event.ShowSystemMessageMsg>(ShowSystemMessage);
            Message.RemoveListener<GameBerry.Event.RefreshBlockUserMsg>(RefreshBlockUser);
            Message.RemoveListener<GameBerry.Event.SetGlobalChatStateMsg>(SetGlobalChatState);

            Message.RemoveListener<GameBerry.Event.OnConnectinghannelGlobalChatMsg>(OnConnectinghannelGlobalChat);
            Message.RemoveListener<GameBerry.Event.OnJoinChannelGlobalChatMsg>(OnJoinChannelGlobalChat);
            Message.RemoveListener<GameBerry.Event.OnLeaveChannelGlobalChatMsg>(OnLeaveChannelGlobalChat);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ChatJoin()
        {
            if (m_recentChatBubble != null)
            {
                m_recentChatBubble.SetReset();
                m_recentChatBubble.gameObject.SetActive(false);
            }

            if (m_chatJoin != null)
            { 
                m_chatJoin.interactable = false;
            }

            Managers.ChatManager.Instance.JoinGlobalChannel();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ShowMinimumContent()
        {
            if (m_miniMumGroup != null)
                m_miniMumGroup.gameObject.SetActive(true);

            if (m_maxiMumGroup != null)
                m_maxiMumGroup.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ShowMaximumContent()
        {
            if (Managers.ChatManager.Instance.IsEnterGlobalChat == false)
                return;

            if (m_miniMumGroup != null)
                m_miniMumGroup.gameObject.SetActive(false);

            if (m_maxiMumGroup != null)
                m_maxiMumGroup.gameObject.SetActive(true);

            if (m_languageSelect != null)
                m_languageSelect.value = Managers.ChatManager.Instance.GlobalChanalLocalType.Enum32ToInt();
        }
        //------------------------------------------------------------------------------------
        private void OnValueChange_ChatLocalType(int value)
        {
            LocalizeType selectValue = value.IntToEnum32<LocalizeType>();

            if (Managers.ChatManager.Instance.GlobalChanalLocalType == selectValue)
                return;

            OnClick_ShowMinimumContent();

            if (m_recentChatBubble != null)
            {
                m_recentChatBubble.SetReset();
                m_recentChatBubble.gameObject.SetActive(false);
            }

            if (m_chatJoin != null)
            {
                m_chatJoin.interactable = false;
            }

            if (m_chatJoinState != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(m_chatJoinState, "chat/serverAccess/desc");

            Managers.ChatManager.Instance.JoinGlobalChannel(selectValue);
        }
        //------------------------------------------------------------------------------------
        private void SendChat()
        {
            if (m_InputChatString != null)
            {
                if (string.IsNullOrEmpty(m_InputChatString.text) == false)
                {
                    if (m_InputChatString.text.Contains("`") == true
                        || m_InputChatString.text.Contains('`') == true)
                    {
                        string localstring = Managers.LocalStringManager.Instance.GetLocalString("chat/bannedText");
                        string outputstr = string.Format("{0}\n({1})", localstring, "`");

                        Contents.GlobalContent.ShowGlobalNotice(outputstr);
                        return;
                    }

                    Managers.ChatManager.Instance.SendChatMessage(m_InputChatString.text);
                    m_InputChatString.text = string.Empty;
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void SendChat(string message)
        {
            if (string.IsNullOrEmpty(message) == true)
                return;

            if (message.Contains("`") == true
                || m_InputChatString.text.Contains('`') == true)
            {
                string localstring = Managers.LocalStringManager.Instance.GetLocalString("chat/bannedText");
                string outputstr = string.Format("{0}\n({1})", localstring, "`");

                Contents.GlobalContent.ShowGlobalNotice(outputstr);
                return;
            }

            Managers.ChatManager.Instance.SendChatMessage(message);

            if (m_InputChatString != null)
            { 
                m_InputChatString.text = string.Empty;
            }
        }
        //------------------------------------------------------------------------------------
        private void ShowGlobalChatMessage(GameBerry.Event.ShowGlobalChatMessageMsg msg)
        {
            UIChatBubbleElement element = GetUIChatBubbleElement();

            element.SetBubble(
                msg.messageInfo,
                msg.NickName, 
                msg.IsMyMessage == true ? m_myNameColor : m_orderNameColor,
                msg.Server,
                msg.Rank,
                msg.Profile,
                msg.ChatMessage,
                msg.IsMyMessage == true ? m_myContentColor : m_orderContentColor,
                msg.IsMyMessage == true ? m_myBGColor : m_orderBGColor,
                msg.UID
                );

            if (m_recentChatBubble != null)
            {
                string chatmsg = msg.ChatMessage;
                if (chatmsg.Length > 20)
                {
                    chatmsg = chatmsg.Substring(0, 19);
                    chatmsg += "...";
                }

                m_recentChatBubble.SetBubble(
                msg.messageInfo,
                msg.NickName,
                msg.IsMyMessage == true ? m_myNameColor : m_orderNameColor,
                msg.Server,
                msg.Rank,
                msg.Profile,
                chatmsg,
                msg.IsMyMessage == true ? m_myContentColor : m_orderContentColor,
                msg.IsMyMessage == true ? m_myBGColor : m_orderBGColor
                );

                m_recentChatBubble.gameObject.SetActive(true);
            }
            
        }
        //------------------------------------------------------------------------------------
        private void ShowSystemMessage(GameBerry.Event.ShowSystemMessageMsg msg)
        {
            UIChatBubbleElement element = GetUIChatBubbleElement();
            element.SetBubble(
                null,
                Define.SystemChatName, 
                m_systemNameColor, 
                string.Empty,
                0,
                0,
                string.Format("{0} {1}", msg.NickName, msg.ChatMessage), 
                m_systemContentColor,
                m_systemBGColor
                );

            if (m_recentChatBubble != null)
            { 
                m_recentChatBubble.SetBubble(
                    null,
                    Define.SystemChatName,
                m_systemNameColor,
                string.Empty,
                0,
                0,
                string.Format("{0} {1}", msg.NickName, msg.ChatMessage),
                m_systemContentColor,
                m_systemBGColor
                );

                m_recentChatBubble.gameObject.SetActive(true);
            }
        }
        //------------------------------------------------------------------------------------
        private UIChatBubbleElement GetUIChatBubbleElement()
        {
            UIChatBubbleElement element = null;

            if (m_uIChatBubbleElementPool.Count >= m_limitChatCount)
            {
                element = m_uIChatBubbleElementPool[0];
                m_uIChatBubbleElementPool.Remove(element);
            }
            else
            {
                GameObject clone = Instantiate(m_chatBubbleElement.gameObject, m_uIChatBubbleElementRoot);

                element = clone.GetComponent<UIChatBubbleElement>();
            }

            element.gameObject.SetActive(true);

            element.transform.SetAsLastSibling();

            m_uIChatBubbleElementPool.Add(element);

            return element;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_m_showBlockListBtn()
        {
            RequestDialogEnter<ChatBlockListDialog>();
        }
        //------------------------------------------------------------------------------------
        private void RefreshBlockUser(GameBerry.Event.RefreshBlockUserMsg msg)
        {
            if (m_uIChatBubbleElementPool.Count <= 0)
                return;

            foreach (var pair in ChatContainer.BlockUserList)
            {
                List<UIChatBubbleElement> uIChatBubbleElements = m_uIChatBubbleElementPool.FindAll(x => x.GetUserID() == pair.Key);
                m_uIChatBubbleElementPool.RemoveAll(x => x.GetUserID() == pair.Key);

                for (int j = 0; j < uIChatBubbleElements.Count; ++j)
                {
                    UIChatBubbleElement element = uIChatBubbleElements[j];
                    element.SetReset();
                    element.gameObject.SetActive(false);
                }

                m_uIChatBubbleElementPool.InsertRange(0, uIChatBubbleElements);
            }

            UIChatBubbleElement recntelement = m_uIChatBubbleElementPool[m_uIChatBubbleElementPool.Count - 1];

            if (m_recentChatBubble != null)
            {
                if (string.IsNullOrEmpty(recntelement.GetUserID()))
                {
                    m_recentChatBubble.SetBubble(
                        null,
                        string.Empty,
                        m_orderNameColor,
                        string.Empty,
                        0,
                        0,
                        string.Empty,
                        m_orderContentColor,
                        m_orderBGColor
                        );

                    m_recentChatBubble.gameObject.SetActive(true);
                }
                else
                {
                    recntelement.SetCloneBubble(m_recentChatBubble, true);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnConnectinghannelGlobalChat(GameBerry.Event.OnConnectinghannelGlobalChatMsg msg)
        {
            if (m_recentChatBubble != null)
            {
                m_recentChatBubble.SetReset();
                m_recentChatBubble.gameObject.SetActive(false);
            }

            if (m_chatJoin != null)
            {
                m_chatJoin.interactable = false;
                m_chatJoin.gameObject.SetActive(true);
            }

            if (m_chatJoinState != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(m_chatJoinState, "chat/serverAccess/desc");

            OnClick_ShowMinimumContent();
        }
        //------------------------------------------------------------------------------------
        private void OnJoinChannelGlobalChat(GameBerry.Event.OnJoinChannelGlobalChatMsg msg)
        {
            if (m_recentChatBubble != null)
            {
                m_recentChatBubble.SetReset();
                m_recentChatBubble.gameObject.SetActive(false);
            }

            if (m_chatJoin != null)
            {
                m_chatJoin.interactable = true;
                m_chatJoin.gameObject.SetActive(false);
            }

            for (int i = 0; i < m_uIChatBubbleElementPool.Count; ++i)
            {
                UIChatBubbleElement element = m_uIChatBubbleElementPool[i];
                element.SetReset();
                element.gameObject.SetActive(false);
            }

            OnClick_ShowMinimumContent();
        }
        //------------------------------------------------------------------------------------
        private void OnLeaveChannelGlobalChat(GameBerry.Event.OnLeaveChannelGlobalChatMsg msg)
        {
            if (m_recentChatBubble != null)
            { 
                m_recentChatBubble.SetReset();
                m_recentChatBubble.gameObject.SetActive(false);
            }

            for (int i = 0; i < m_uIChatBubbleElementPool.Count; ++i)
            {
                UIChatBubbleElement element = m_uIChatBubbleElementPool[i];
                element.SetReset();
                element.gameObject.SetActive(false);
            }

            if (m_chatJoin != null)
            { 
                m_chatJoin.interactable = true;
                m_chatJoin.gameObject.SetActive(true);
            }

            if (m_chatJoinState != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(m_chatJoinState, "chat/serverAccess");
        }
        //------------------------------------------------------------------------------------
        private void SetGlobalChatState(GameBerry.Event.SetGlobalChatStateMsg msg)
        {
            OnClick_ShowMaximumContent();
        }
        //------------------------------------------------------------------------------------
    }
}