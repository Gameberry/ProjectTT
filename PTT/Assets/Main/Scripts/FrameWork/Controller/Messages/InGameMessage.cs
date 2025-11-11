namespace GameBerry.Event
{
    public class ShowGlobalChatMessageMsg : Message
    {
        public string UID = string.Empty;
        public bool IsMyMessage = false;
        public string NickName = string.Empty;
        public string ChatMessage = string.Empty;
        public string Server = string.Empty;
        public int Rank = 0;
        public int Profile = 0;
        public BackndChat.MessageInfo messageInfo = null;
    }

    public class ShowGuildChatMessageMsg : Message
    {
        public string UID = string.Empty;
        public bool IsMyMessage = false;
        public string NickName = string.Empty;
        public string ChatMessage = string.Empty;
        public string Server = string.Empty;
        public int Rank = 0;
        public int Profile = 0;
        public BackndChat.MessageInfo messageInfo = null;
    }

    public class SetGlobalChatStateMsg : Message
    {
        public bool Extension = false;
    }

    public class SetGuildChatStateMsg : Message
    {
        public bool Extension = false;
    }

    public class ShowSystemMessageMsg : Message
    {
        public string NickName = string.Empty;
        public string ChatMessage = string.Empty;
    }

    public class OnConnectinghannelGlobalChatMsg : Message
    {
    }

    public class OnJoinChannelGlobalChatMsg : Message
    {
    }

    public class OnLeaveChannelGlobalChatMsg : Message
    {
    }

    public class SetReportUserInfoMsg : Message
    {
        public BackndChat.MessageInfo messageInfo = null;
    }

    public class RefreshNoticeMsg : Message
    {

    }
}