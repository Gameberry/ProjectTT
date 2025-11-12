namespace GameBerry.Event
{
    public class SetNoticeMsg : Message
    {
        public string NoticeStr;
    }

    public class LoginResultMsg : Message
    {
        public bool IsSuccess = false;
        public bool IsTokenLoginError = false;
    }

    public class CreateNickNameResultMsg : Message
    {
        public bool IsSuccess = false;
        public string ErrorMessage = string.Empty; // IsSuccess가 False일 때 초기화 해줌
    }
}