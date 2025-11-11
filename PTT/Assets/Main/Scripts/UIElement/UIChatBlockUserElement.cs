using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;

namespace GameBerry.UI
{
    public class UIChatBlockUserElement : InfiniteScrollItem
    {
        [SerializeField]
        private TMP_Text userInfo;

        [SerializeField]
        private Button unBlock;

        private ChatBlockUser myChatBlockUser;

        private void Awake()
        {
            if (unBlock != null)
                unBlock.onClick.AddListener(OnClick_UnBlock);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_UnBlock()
        {
            if (myChatBlockUser == null)
                return;

            Managers.ChatManager.Instance.SendUnBlockUser(myChatBlockUser.UID);
        }
        //------------------------------------------------------------------------------------
        public override void UpdateData(InfiniteScrollData scrollData)
        {
            myChatBlockUser = scrollData as ChatBlockUser;

            if (userInfo != null)
                userInfo.text = string.Format("{0}\n({1})", myChatBlockUser.NickName, myChatBlockUser.UID);
        }
        //------------------------------------------------------------------------------------
    }
}