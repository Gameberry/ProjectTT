using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class InGameSimpleDescPopupDialog : IDialog
    {
        [SerializeField]
        private List<Button> m_exitBtn;

        [SerializeField]
        private ScrollRect scrollrect;

        [SerializeField]
        private Transform titleRoot;

        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private TMP_Text descText;

        private GameBerry.Event.SetSimpleDescPopupMsg setSimpleDescPopupMsg = new GameBerry.Event.SetSimpleDescPopupMsg();

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
                            RequestDialogExit<InGameSimpleDescPopupDialog>();
                        });
                }
            }

            Message.AddListener<GameBerry.Event.SetSimpleDescPopupMsg>(SetSimpleDescPopup);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.SetSimpleDescPopupMsg>(SetSimpleDescPopup);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            if (scrollrect != null)
                scrollrect.normalizedPosition = Vector2.one;
        }
        //------------------------------------------------------------------------------------
        private void SetSimpleDescPopup(GameBerry.Event.SetSimpleDescPopupMsg msg)
        {
            if (string.IsNullOrEmpty(msg.title) == true)
            {
                if (titleRoot != null)
                    titleRoot.gameObject.SetActive(false);
            }
            else
            {
                if (titleRoot != null)
                    titleRoot.gameObject.SetActive(true);

                if (titleText != null)
                    Managers.LocalStringManager.Instance.SetLocalizeText(titleText, msg.title);
            }

            if (descText != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(descText, msg.desc);
        }
        //------------------------------------------------------------------------------------
    }
}