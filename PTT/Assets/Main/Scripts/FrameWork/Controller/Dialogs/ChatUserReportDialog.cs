using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class ChatUserReportDialog : IDialog
    {
        [SerializeField]
        private List<Button> m_exitBtn;

        [SerializeField]
        private TMP_Text nickName;

        [SerializeField]
        private TMP_Text uID;

        [SerializeField]
        private Button blockUser;

        [SerializeField]
        private TMP_Dropdown reportType;

        [SerializeField]
        private TMP_InputField reportDetail;

        [SerializeField]
        private Button reportUser;

        private BackndChat.MessageInfo messageInfo = null;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (m_exitBtn != null)
            {
                for (int i = 0; i < m_exitBtn.Count; ++i)
                {
                    if (m_exitBtn[i] != null)
                        m_exitBtn[i].onClick.AddListener(() =>
                        {
                            RequestDialogExit<ChatUserReportDialog>();
                        });
                }
            }

            Message.AddListener<GameBerry.Event.SetReportUserInfoMsg>(SetReportUserInfo);

            if (blockUser != null)
                blockUser.onClick.AddListener(OnClick_Block);

            if (reportUser != null)
                reportUser.onClick.AddListener(OnClick_Report);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.SetReportUserInfoMsg>(SetReportUserInfo);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            if (reportType != null)
            {
                reportType.ClearOptions();
                List<string> optiondatalabel = new List<string>();

                for (int i = (int)V2Enum_ReportType.inappropriate; i < (int)V2Enum_ReportType.Max; ++i)
                {
                    V2Enum_ReportType localizeType = i.IntToEnum32<V2Enum_ReportType>();
                    string localstr = Managers.LocalStringManager.Instance.GetLocalString(string.Format("chat/report/{0}", localizeType.ToString()));

                    optiondatalabel.Add(localstr);
                }

                reportType.AddOptions(optiondatalabel);

                reportType.value = 0;
            }

            if (reportDetail != null)
                reportDetail.text = string.Empty;
        }
        //------------------------------------------------------------------------------------
        private void SetReportUserInfo(GameBerry.Event.SetReportUserInfoMsg msg)
        {
            if (msg == null || msg.messageInfo == null)
                return;

            if (nickName != null)
                nickName.text = msg.messageInfo.GamerName;

            if (uID != null)
                uID.text = msg.messageInfo.Avatar;

            messageInfo = msg.messageInfo;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Block()
        {
            RequestDialogExit<ChatUserReportDialog>();

            if (messageInfo != null)
                Managers.ChatManager.Instance.SendBlockUser(messageInfo);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Report()
        {
            V2Enum_ReportType v2Enum_ReportType = V2Enum_ReportType.inappropriate;
            if (reportType != null)
                v2Enum_ReportType = reportType.value.IntToEnum32<V2Enum_ReportType>();

            if (messageInfo != null)
                Managers.ChatManager.Instance.SendReport(messageInfo, v2Enum_ReportType, reportDetail.text);

            RequestDialogExit<ChatUserReportDialog>();
        }
        //------------------------------------------------------------------------------------
    }
}