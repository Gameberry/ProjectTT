using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace GameBerry.UI
{
    public class LobbyEtcMenuDialog : IDialog
    {
        [Header("------------MenuGroup------------")]
        [SerializeField]
        private List<ContentButtonData> m_bottomMenuBtnDatas;

        [SerializeField]
        private Button m_menuBtn;

        [SerializeField]
        private Button m_menuExitBtn;

        [SerializeField]
        private Transform m_extensionMenuBtn;

        private bool m_isExtensionMode = false;

        protected override void OnLoad()
        {
            for (int i = 0; i < m_bottomMenuBtnDatas.Count; ++i)
            {
                if (m_bottomMenuBtnDatas[i] != null)
                {
                    if (m_bottomMenuBtnDatas[i].MenuID == ContentDetailList.CheckIn)
                    {
                        UIGuideInteractor uIGuideInteractor = m_bottomMenuBtnDatas[i].btn.gameObject.AddComponent<UIGuideInteractor>();
                        uIGuideInteractor.MyGuideType = V2Enum_EventType.CheckInRewardGet;
                        uIGuideInteractor.MyStepID = 2;
                        uIGuideInteractor.FocusAngle = 270;
                        uIGuideInteractor.FocusParent = dialogView.transform;
                        uIGuideInteractor.GuideInteratorActionType = Managers.GuideInteratorActionType.Next;
                        uIGuideInteractor.IsAutoSetting = false;
                        uIGuideInteractor.ConnectInteractor();
                    }
                    else if (m_bottomMenuBtnDatas[i].MenuID == ContentDetailList.Quest)
                    {
                        UIGuideInteractor uIGuideInteractor = m_bottomMenuBtnDatas[i].btn.gameObject.AddComponent<UIGuideInteractor>();
                        uIGuideInteractor.MyGuideType = V2Enum_EventType.DailyMissionRewardGet;
                        uIGuideInteractor.MyStepID = 2;
                        uIGuideInteractor.FocusAngle = 270;
                        uIGuideInteractor.FocusParent = dialogView.transform;
                        uIGuideInteractor.GuideInteratorActionType = Managers.GuideInteratorActionType.Next;
                        uIGuideInteractor.IsAutoSetting = false;
                        uIGuideInteractor.ConnectInteractor();
                    }
                    else if (m_bottomMenuBtnDatas[i].MenuID == ContentDetailList.Post)
                    {
                        UIGuideInteractor uIGuideInteractor = m_bottomMenuBtnDatas[i].btn.gameObject.AddComponent<UIGuideInteractor>();
                        uIGuideInteractor.MyGuideType = V2Enum_EventType.MailGet;
                        uIGuideInteractor.MyStepID = 2;
                        uIGuideInteractor.FocusAngle = 270;
                        uIGuideInteractor.FocusParent = dialogView.transform;
                        uIGuideInteractor.GuideInteratorActionType = Managers.GuideInteratorActionType.Next;
                        uIGuideInteractor.IsAutoSetting = false;
                        uIGuideInteractor.ConnectInteractor();
                    }
                    else if (m_bottomMenuBtnDatas[i].MenuID == ContentDetailList.Exchange)
                    {
                        UIGuideInteractor uIGuideInteractor = m_bottomMenuBtnDatas[i].btn.gameObject.AddComponent<UIGuideInteractor>();
                        uIGuideInteractor.MyGuideType = V2Enum_EventType.CheckExchange;
                        uIGuideInteractor.MyStepID = 2;
                        uIGuideInteractor.FocusAngle = 270;
                        uIGuideInteractor.FocusParent = dialogView.transform;
                        uIGuideInteractor.GuideInteratorActionType = Managers.GuideInteratorActionType.Next;
                        uIGuideInteractor.IsAutoSetting = false;
                        uIGuideInteractor.ConnectInteractor();
                    }

                    m_bottomMenuBtnDatas[i].CallBack = OnClick_BottomBtn;
                    m_bottomMenuBtnDatas[i].btn.onClick.AddListener(m_bottomMenuBtnDatas[i].OnClick);
                }
            }


            if (m_menuBtn != null)
                m_menuBtn.onClick.AddListener(OnClick_MenuBtn);

            if (m_menuExitBtn != null)
                m_menuExitBtn.onClick.AddListener(OnClick_MenuExitBtn);

        }
        //------------------------------------------------------------------------------------
        private void OnClick_MenuBtn()
        {
            return;
            if (m_extensionMenuBtn != null)
                m_extensionMenuBtn.gameObject.SetActive(true);

            m_isExtensionMode = true;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_MenuExitBtn()
        {
            return;
            if (m_extensionMenuBtn != null)
                m_extensionMenuBtn.gameObject.SetActive(false);

            m_isExtensionMode = false;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_BottomBtn(ContentDetailList callbackID)
        {
            switch (callbackID)
            {
                case ContentDetailList.Post:
                    {
                        UIManager.DialogEnter<InGamePostPopupDialog>();
                        break;
                    }
                case ContentDetailList.CheckIn:
                    {
                        UIManager.DialogEnter<InGameCheckInDialog>();
                        break;
                    }
                case ContentDetailList.GameOption:
                    {
                        UIManager.DialogEnter<GlobalSettingDialog>();
                        break;
                    }
                case ContentDetailList.Quest:
                    {
                        UIManager.DialogEnter<LobbyQuestContentDialog>();
                        break;
                    }
                case ContentDetailList.Exchange:
                    {
                        UIManager.DialogEnter<InGameExchangeDialog>();
                        break;
                    }
                case ContentDetailList.Rank:
                    {
                        UIManager.DialogEnter<InGameRankDialog>();
                        break;
                    }
                case ContentDetailList.Notice:
                    {
                        UIManager.DialogEnter<InGameNoticeDialog>();
                        break;
                    }
                case ContentDetailList.Inventory:
                    {
                        UIManager.DialogEnter<InGameInventoryDialog>();
                        break;
                    }
            }

            OnClick_MenuBtn();
        }
        //------------------------------------------------------------------------------------
    }
}