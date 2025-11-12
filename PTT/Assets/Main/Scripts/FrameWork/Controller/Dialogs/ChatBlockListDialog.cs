using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;

namespace GameBerry.UI
{
    public class ChatBlockListDialog : IDialog
    {
        [SerializeField]
        private List<Button> m_exitBtn;

        [SerializeField]
        private TMP_Text m_blockCount;

        [SerializeField]
        private InfiniteScroll m_elementInfinityScroll;

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
                            UIManager.DialogExit<ChatBlockListDialog>();
                        });
                }
            }

            Message.AddListener<GameBerry.Event.RefreshBlockUserMsg>(RefreshBlockUser);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.RefreshBlockUserMsg>(RefreshBlockUser);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            RefreshBlockUser(null);
        }
        //------------------------------------------------------------------------------------
        private void RefreshBlockUser(GameBerry.Event.RefreshBlockUserMsg msg)
        {
            if (m_elementInfinityScroll != null)
            {
                m_elementInfinityScroll.Clear();

                foreach (var pair in ChatContainer.BlockUserList)
                {
                    m_elementInfinityScroll.InsertData(pair.Value);
                }

                m_elementInfinityScroll.MoveToFirstData();
            }

            if (m_blockCount != null)
                m_blockCount.SetText("{0}/{1}", ChatContainer.BlockUserList.Count, Define.ChatBenMaxCount);
        }
        //------------------------------------------------------------------------------------
    }
}