using UnityEngine;

namespace GameBerry.Event
{
    public class ShowPopup_OkMsg : Message
    {
        public string titletext;
        public string contenttext;
        public System.Action okAction;

        public System.Action<bool> toDayHide = null;
    }

    public class ShowPopup_OkCancelMsg : Message
    {
        public string titletext;
        public string contenttext;

        public bool useCustomOKBtn;

        public string OKBtnTitle;
        public Sprite OKBtnIcon;
        public string OKBtnContent;

        public System.Action okAction;
        public System.Action cancelAction = null;

        public System.Action<bool> toDayHide = null;
    }

    public class ShowPopup_InputMsg : Message
    {
        public string titletext;
        public string defaultstr;
        public string placeholder;
        public System.Action<string> okAction;
        public System.Action cancelAction = null;

        public System.Action<bool> toDayHide = null;
    }

    public class GlobalNoticeMsg : Message
    {
        public float duration;
        public string NoticeString;
    }

    public class GlobalGuideNoticeMsg : Message
    {
        public float duration;
        public string NoticeString;
        public float height = 0.0f;
    }

    public class GlobalNoticeBattlePowerMsg : Message
    {
        public double battlePower, changeValue;
    }

    public class GlobalNoticeSynergyCountMsg : Message
    {
        public int before, after;
    }


    public class GlobalUnLockContentNoticeMsg : Message
    {
        public string titletext;
    }

    public class SetBGMVolumeMsg : Message
    {
        public float volume;

        public SetBGMVolumeMsg(float volume)
        {
            this.volume = volume;
        }
    }

    public class SetFXVolumeMsg : Message
    {
        public float volume;

        public SetFXVolumeMsg(float volume)
        {
            this.volume = volume;
        }
    }

    public class DoFadeMsg : Message
    {
        public float duration;
        public bool visible; // false 투명 -> 검은색으로
    }

    public class DoDungeonFadeMsg : Message
    {
        public float duration;
        public bool visible; // false 투명 -> 검은색으로
    }

    public class SetBuffStringMsg : Message
    {
        public string buffstr = string.Empty;
    }
}