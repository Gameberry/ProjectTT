using System.Collections.Generic;
using UnityEngine;
using GameBerry;
using BackndChat;

namespace GameBerry.Managers
{
    public class ChatManager : MonoSingleton<ChatManager>, IChatClientListener
    {
        private Event.ShowGlobalChatMessageMsg m_showGlobalChatMessageMsg = new Event.ShowGlobalChatMessageMsg();
        private Event.ShowGuildChatMessageMsg m_showGuildChatMessageMsg = new Event.ShowGuildChatMessageMsg();
        private Event.ShowSystemMessageMsg m_showSystemMessageMsg = new Event.ShowSystemMessageMsg();

        private Event.OnConnectinghannelGlobalChatMsg m_onConnectinghannelGlobalChatMsg = new Event.OnConnectinghannelGlobalChatMsg();
        private Event.OnJoinChannelGlobalChatMsg m_onJoinChannelGlobalChatMsg = new Event.OnJoinChannelGlobalChatMsg();
        private Event.OnLeaveChannelGlobalChatMsg m_onLeaveChannelGlobalChatMsg = new Event.OnLeaveChannelGlobalChatMsg();

        private Event.SetReportUserInfoMsg m_setReportUserInfoMsg = new Event.SetReportUserInfoMsg();
        private Event.RefreshBlockUserMsg m_refreshBlockUserMsg = new Event.RefreshBlockUserMsg();

        private ChatClient ChatClient = null;
        private string ChatUUId = string.Empty;

        private bool finishChatInit = false;

        private const string globalGroupName_param = "{0}-public";
        private const string globalChanalName_param = "{0}-public-1";

        private string globalGroupName = string.Empty;
        private string globalChanalName = string.Empty;
        private ulong globalChannelNumber = 0;

        public string testmessage = string.Empty;

        private LocalizeType globalChanalLocalType = LocalizeType.English;
        public LocalizeType GlobalChanalLocalType { get { return globalChanalLocalType; } }

        private LocalizeType nextGlobalLocalType = LocalizeType.Max;

        private bool isEnterGlobalChat = false;
        public bool IsEnterGlobalChat { get { return isEnterGlobalChat; } }

        public bool IsFirstError = true;

        private string guildGroupName = string.Empty;
        private string guildChanalName = string.Empty;
        private ulong guildChannelNumber = 0;

        public bool JoinChatGlobal = false;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            switch (SceneManager.Instance.BuildElement)
            {
                case BuildEnvironmentEnum.Develop:
                    {
                        ChatUUId = "d06a7de9-e981-4de0-a4d9-a6f040a1af3c";
                        break;
                    }
                case BuildEnvironmentEnum.QA:
                    {
                        ChatUUId = "7e03b12a-5f0b-4b9c-9486-d3c92fb0a223";
                        break;
                    }
                case BuildEnvironmentEnum.Stage:
                    {
                        ChatUUId = "ac7b00ba-07a1-439b-9f4c-ac8f9db92f0e";
                        break;
                    }
                case BuildEnvironmentEnum.Product:
                    {
                        ChatUUId = "62d8784c-2635-4f0a-bb21-4acfb86fec85";
                        break;
                    }
            }

            int localtype = PlayerPrefs.GetInt(Define.ChatLocalizeKey, -1);

            if (localtype == -1)
            {
                globalChanalLocalType = LocalStringManager.Instance.GetLocalizeType();
            }
            else
                globalChanalLocalType = localtype.IntToEnum32<LocalizeType>();

            string benuser = PlayerPrefs.GetString(Define.ChatBenUserKey, string.Empty);
            ChatContainer.SetDeSerializeString(benuser);
        }
        //------------------------------------------------------------------------------------
        public void ConnectChat()
        {
            if (finishChatInit == true)
                return;

            ChatClient = new ChatClient(this, new ChatClientArguments
            {
                UUID = ChatUUId,
                Avatar = BackEnd.Backend.UID,
            });

            finishChatInit = true;
        }
        //------------------------------------------------------------------------------------
        private void Update()
        {
            if (finishChatInit == true)
                ChatClient?.Update();
        }
        //------------------------------------------------------------------------------------
        public void JoinGuildChannel()
        {
            ChatClient?.SendJoinGuildChannel();
        }
        //------------------------------------------------------------------------------------
        public void LeaveGuildChannel()
        {
            ChatClient?.SendLeaveGuildChannel();

            //if (Managers.DungeonManager.Instance.CurrentDungeonKinds != Enum_Dungeon.AllyArenaDungeon)
            //{
            //    UI.IDialog.RequestDialogEnter<UI.ChatViewDialog>();
            //    UI.IDialog.RequestDialogExit<UI.ChatGuildDialog>();
            //}
        }
        //------------------------------------------------------------------------------------
        public void JoinGlobalChannel()
        {
            JoinGlobalChannel(globalChanalLocalType);
        }
        //------------------------------------------------------------------------------------
        public void JoinGlobalChannel(LocalizeType localizeType)
        {
            ConnectChat();

            if (Managers.RankManager.Instance.CompleteFirstCombatRank == false)
            {
                TheBackEnd.TheBackEnd_Rank.UpdateUserScore(
                    V2Enum_RankType.Power,
                    Managers.RankManager.Instance.GetMyRankData().CombatPower.GetDecrypted(),
                    Managers.RankManager.Instance.GetDetailString(),
                    () =>
                    {
                        TheBackEnd.TheBackEnd_Rank.GetRankList(V2Enum_RankType.Power, null);
                    });

                //SocialManager.Instance.SendFirstSocialData();
                Managers.RankManager.Instance.CompleteFirstCombatRank = true;
            }

            if (isEnterGlobalChat == true)
            {
                nextGlobalLocalType = localizeType;
                ChatClient.SendLeaveChannel(globalGroupName, globalChanalName, globalChannelNumber);
                return;
            }

            globalGroupName = GetGroupName(localizeType);
            globalChanalName = GetChanalName(localizeType);

            Message.Send(m_onConnectinghannelGlobalChatMsg);

            ChatClient.SendJoinOpenChannel(globalGroupName, globalChanalName);

            globalChanalLocalType = localizeType;

            PlayerPrefs.SetInt(Define.ChatLocalizeKey, localizeType.Enum32ToInt());
        }
        //------------------------------------------------------------------------------------
        private string GetGroupName(LocalizeType localizeType)
        {
            return string.Format(globalGroupName_param, ConvertLocalName(localizeType));
        }
        //------------------------------------------------------------------------------------
        private string GetChanalName(LocalizeType localizeType)
        {
            return string.Format(globalChanalName_param, ConvertLocalName(localizeType));
        }
        //------------------------------------------------------------------------------------
        private string ConvertLocalName(LocalizeType localizeType)
        {
            switch (localizeType)
            {
                case LocalizeType.English:
                    {
                        return "en";
                    }
                case LocalizeType.Korean:
                    {
                        return "ko";
                    }
                case LocalizeType.Japanese:
                    {
                        return "ja";
                    }
                case LocalizeType.ChineseTraditional:
                    {
                        return "ch";
                    }
                case LocalizeType.Portuguesa:
                    {
                        return "pt";
                    }
                case LocalizeType.Spanish:
                    {
                        return "sp";
                    }
            }

            return "en";
        }

        //------------------------------------------------------------------------------------
        public void SendChatMessage(string chatMessage)
        {
            if (finishChatInit == false)
            {
                Debug.LogError("finishChatInit is false");
                return;
            }

            if (string.IsNullOrEmpty(chatMessage) == true)
                return;

            RankTable rankTable = Managers.RankManager.Instance.GetRankTable(V2Enum_RankType.Power);

            string sendstr = string.Format("{0}`{1}`{2}`{3}", chatMessage, PlayerDataContainer.DisplayChatServerName, rankTable == null ? 0 : rankTable.MyRank, PlayerDataContainer.Profile);

            ChatClient.SendChatMessage(globalGroupName, globalChanalName, globalChannelNumber, sendstr);

            //Managers.QuestManager.Instance.AddMissionCount(V2Enum_QuestGoalType.Chat, 1);
        }
        //------------------------------------------------------------------------------------
        public void SendGuildChatMessage(string chatMessage)
        {
            if (finishChatInit == false)
            {
                Debug.LogError("finishChatInit is false");
                return;
            }

            if (string.IsNullOrEmpty(chatMessage) == true)
                return;

            RankTable rankTable = Managers.RankManager.Instance.GetRankTable(V2Enum_RankType.Power);

            string sendstr = string.Format("{0}`{1}`{2}`{3}", chatMessage, PlayerDataContainer.DisplayChatServerName, rankTable == null ? 0 : rankTable.MyRank, PlayerDataContainer.Profile);

            ChatClient.SendChatMessage(guildGroupName, guildChanalName, guildChannelNumber, sendstr);

            //Managers.QuestManager.Instance.AddMissionCount(V2Enum_QuestGoalType.Chat, 1);
        }
        //------------------------------------------------------------------------------------
        public void GetDetailInfo(MessageInfo messageInfo, ref string message, ref string server, ref int rank, ref int profile)
        {
            string originmessage = messageInfo.Message;
            string[] arr = originmessage.Split('`');

            try
            {
                if (arr.Length >= 4)
                {
                    message = arr[0];
                    server = arr[1];
                    rank = arr[2].ToInt();
                    profile = arr[3].ToInt();
                }
                else
                {
                    message = string.Empty;
                    server = string.Empty;
                    rank = 0;
                    profile = 0;
                }
            }
            catch
            {
                message = string.Empty;
                server = string.Empty;
                rank = 0;
                profile = 0;
            }
        }
        //------------------------------------------------------------------------------------
        public void ShowUserInfoPopup(MessageInfo messageInfo)
        {
            if (messageInfo == null)
                return;

            if (messageInfo.Avatar == BackEnd.Backend.UID)
                return;

            //Managers.SocialManager.Instance.ShowUserDetailInfo(messageInfo.GamerName);
        }
        //------------------------------------------------------------------------------------
        public void ShowReportPopup(MessageInfo messageInfo)
        {
            if (messageInfo == null)
                return;

            if (messageInfo.Avatar == BackEnd.Backend.UID)
                return;

            m_setReportUserInfoMsg.messageInfo = messageInfo;
            Message.Send(m_setReportUserInfoMsg);

            UI.IDialog.RequestDialogEnter<UI.ChatUserReportDialog>();
        }
        //------------------------------------------------------------------------------------
        public void SendBlockUser(MessageInfo messageInfo)
        {
            if (ChatContainer.BlockUserList.Count >= Define.ChatBenMaxCount)
            {
                Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("chat/blocked/userCountMax"));
                return;
            }

            if (ChatContainer.BlockUserList.ContainsKey(messageInfo.Avatar) == true)
            {
                // 이미 밴된 유저
                Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("chat/block/remind"));
                return;
            }

            ChatBlockUser chatBlockUser = new ChatBlockUser();
            chatBlockUser.UID = messageInfo.Avatar;
            chatBlockUser.NickName = messageInfo.GamerName;

            ChatContainer.BlockUserList.Add(messageInfo.Avatar, chatBlockUser);
            Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("chat/block/complete"));

            Message.Send(m_refreshBlockUserMsg);

            PlayerPrefs.SetString(Define.ChatBenUserKey, ChatContainer.GetSerializeString());
            PlayerPrefs.Save();
        }
        //------------------------------------------------------------------------------------
        public void SendUnBlockUser(string uID)
        {
            if (ChatContainer.BlockUserList.ContainsKey(uID) == false)
            {
                return;
            }

            ChatContainer.BlockUserList.Remove(uID);

            Message.Send(m_refreshBlockUserMsg);

            PlayerPrefs.SetString(Define.ChatBenUserKey, ChatContainer.GetSerializeString());
            PlayerPrefs.Save();
        }
        //------------------------------------------------------------------------------------
        public void SendReport(MessageInfo messageInfo, V2Enum_ReportType v2Enum_ReportType, string detail)
        {
            ChatClient.SendReportChatMessage(messageInfo.Index, messageInfo.Tag, Managers.LocalStringManager.Instance.GetLocalString(string.Format("chat/report/{0}", v2Enum_ReportType.ToString())), detail);
        }
        //------------------------------------------------------------------------------------

        //------------------------------------------------------------------------------------
        public void OnChatMessage(MessageInfo messageInfo)
        {
            Debug.Log(messageInfo.Message);
            if (messageInfo.MessageType == MESSAGE_TYPE.NORMAL_MESSAGE)
            {
                if (ChatContainer.BlockUserList.ContainsKey(messageInfo.Avatar) == true)
                    return;

                if (messageInfo.ChannelGroup == guildGroupName)
                { // 길드에서 온거
                    m_showGuildChatMessageMsg.UID = messageInfo.Avatar;
                    m_showGuildChatMessageMsg.IsMyMessage = messageInfo.Avatar == BackEnd.Backend.UID;
                    m_showGuildChatMessageMsg.NickName = messageInfo.GamerName;

                    GetDetailInfo(messageInfo,
                        ref m_showGuildChatMessageMsg.ChatMessage,
                        ref m_showGuildChatMessageMsg.Server,
                        ref m_showGuildChatMessageMsg.Rank,
                        ref m_showGuildChatMessageMsg.Profile);

                    m_showGuildChatMessageMsg.messageInfo = messageInfo;

                    Message.Send(m_showGuildChatMessageMsg);
                }
                else
                {
                    m_showGlobalChatMessageMsg.UID = messageInfo.Avatar;
                    m_showGlobalChatMessageMsg.IsMyMessage = messageInfo.Avatar == BackEnd.Backend.UID;
                    m_showGlobalChatMessageMsg.NickName = messageInfo.GamerName;

                    GetDetailInfo(messageInfo,
                        ref m_showGlobalChatMessageMsg.ChatMessage,
                        ref m_showGlobalChatMessageMsg.Server,
                        ref m_showGlobalChatMessageMsg.Rank,
                        ref m_showGlobalChatMessageMsg.Profile);

                    m_showGlobalChatMessageMsg.messageInfo = messageInfo;

                    Message.Send(m_showGlobalChatMessageMsg);
                }
            }
        }
        //------------------------------------------------------------------------------------

        //------------------------------------------------------------------------------------
        public void OnDeleteMessage(MessageInfo messageInfo)
        {
            
        }
        //------------------------------------------------------------------------------------
        public void OnError(ERROR_MESSAGE error, object param)
        {
            if (error == ERROR_MESSAGE.DISABLED_CHANNEL)
            {
                if (IsFirstError == true)
                {
                    JoinGlobalChannel();
                }
                else
                {
                    globalChannelNumber = 0;

                    Message.Send(m_onLeaveChannelGlobalChatMsg);

                    Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("chat/serverAccessFail/desc"));
                }
            }
            else if (error == ERROR_MESSAGE.MESSAGE_SPAM)
            { // 채팅 메시지 도배로 인해 메시지가 차단 되었습니다. (10초)
                Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("chat/banned/desc"));
            }
            else if (error == ERROR_MESSAGE.CHAT_BAN)
            { // 채팅 금지 상태 입니다.
                ulong remainsecond = 0;
                try
                {
                    ErrorMessageChatBanParam errorMessageChatBanParam = param as ErrorMessageChatBanParam;
                    remainsecond = errorMessageChatBanParam.RemainSeconds;
                }
                catch
                { 

                }
                string benString = string.Format("{0}({1:#,0}s)", Managers.LocalStringManager.Instance.GetLocalString("chat/banned"), remainsecond);
                Contents.GlobalContent.ShowGlobalNotice(benString);
            }
            else if (error == ERROR_MESSAGE.TOO_MANY_REPORT)
            { // 오늘 하루 신고 가능 횟수를 모두 사용 하였습니다.
                Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("chat/report/dailyLimit"));
            }
            else if (error == ERROR_MESSAGE.MESSAGE_TOO_LONG)
            { // 채팅 메시지 길이가 너무 깁니다.
                Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("chat/textLength/long"));
            }
            else if (error == ERROR_MESSAGE.MESSAGE_TOO_SHORT)
            { // 채팅 메시지 길이가 너무 짧습니다.
                Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("chat/textLength/short"));
            }

            Debug.LogError(string.Format("OnError : {0} {1}", error, param));
        }
        //------------------------------------------------------------------------------------
        public void OnHideMessage(MessageInfo messageInfo)
        {
            
        }

        bool firstguildJoin = false;

        public void OnJoinChannel(ChannelInfo channelInfo)
        {
            if (channelInfo == null)
                return;

            if (channelInfo.ChannelName == "guild")
            {
                guildGroupName = channelInfo.ChannelGroup;
                guildChanalName = channelInfo.ChannelName;
                guildChannelNumber = channelInfo.ChannelNumber;

                if (channelInfo.Messages == null)
                    return;

                for (int i = 0; i < channelInfo.Messages.Count; ++i)
                {
                    OnChatMessage(channelInfo.Messages[i]);
                }

                if (firstguildJoin == false)
                {
                    firstguildJoin = true;
                }
                JoinGlobalChannel();
            }

            if (channelInfo.ChannelName == globalChanalName)
            { 
                globalChannelNumber = channelInfo.ChannelNumber;
                isEnterGlobalChat = true;
                Message.Send(m_onJoinChannelGlobalChatMsg);

                if (channelInfo.Messages == null)
                    return;

                for (int i = 0; i < channelInfo.Messages.Count; ++i)
                {
                    OnChatMessage(channelInfo.Messages[i]);
                }

                nextGlobalLocalType = LocalizeType.Max;

                Debug.LogWarning(string.Format("OnJoinChannel : {0}", channelInfo.ChannelName));
            }
        }

        public void OnJoinChannelPlayer(string channelGroup, string channelName, ulong channelNumber, string gamerName, string avatar)
        {
            
        }

        public void OnLeaveChannel(ChannelInfo channelInfo)
        {
            if (channelInfo.ChannelName == globalChanalName)
            { 
                globalChannelNumber = 0;
                isEnterGlobalChat = false;

                Message.Send(m_onLeaveChannelGlobalChatMsg);

                Debug.LogWarning(string.Format("OnLeaveChannel : {0}", channelInfo.ChannelName));

                if (nextGlobalLocalType != LocalizeType.Max)
                    JoinGlobalChannel(nextGlobalLocalType);
            }
        }

        public void OnLeaveChannelPlayer(string channelGroup, string channelName, ulong channelNumber, string gamerName, string avatar)
        {
            
        }

        public void OnSuccess(SUCCESS_MESSAGE success, object param)
        {
            if (success == SUCCESS_MESSAGE.REPORT)
            { // 신고 처리 완료
                Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("chat/report/complete"));
            }
        }

        public void OnTranslateMessage(List<MessageInfo> messages)
        {
            
        }

        public void OnWhisperMessage(WhisperMessageInfo messageInfo)
        {
            
        }

        //------------------------------------------------------------------------------------
        private void OnApplicationQuit()
        {
            ChatClient?.Dispose();
        }
    }
}