using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.TheBackEnd;

namespace GameBerry
{
    public class AppInitialize : MonoBehaviour
    {
        private UI.AppLoadingDialog m_appInitUI;

        private System.Action m_loginCompleteCallBack; // 로그인이 끝나면 호출

        //------------------------------------------------------------------------------------
        public void Init()
        {
            ResourceLoader.Instance.Load<GameObject>("AppInitialize/AppLoadingDialog", o =>
            {
                GameObject clone = Instantiate(o, UI.UIManager.Instance.ProjectLoadingContent) as GameObject;
                if (clone != null)
                    m_appInitUI = clone.GetComponent<UI.AppLoadingDialog>();
            });

            Message.AddListener<Event.LoginResultMsg>(LoginResult);

        }
        //------------------------------------------------------------------------------------
        public void Release()
        {
            Message.RemoveListener<Event.LoginResultMsg>(LoginResult);

            Destroy(m_appInitUI.gameObject);
        }
        //------------------------------------------------------------------------------------
        public void DoLogin(System.Action action)
        {
            m_loginCompleteCallBack = action;

            ShowNoticeText(Managers.LocalStringManager.Instance.GetLocalString("LogIn_Loading_Verification"));

            if (TheBackEndManager.Instance.IsNeedSignUp() == true)
            {
                OnSignUpMenual();
                ShowNoticeText(string.Empty);
            }
            else
            {
                OnSignUpProcess(TheBackEndManager.Instance.CheckLoginState());
            }

            return;
        }
        //------------------------------------------------------------------------------------
        private void OnSignUpMenual()
        {
            if (m_appInitUI != null)
            {
                m_appInitUI.VisibleLoginProcess(true);
                m_appInitUI.SetLoginCallBack(OnSignUpProcess);
                m_appInitUI.VisibleLoginButtonGroup(false);
                m_appInitUI.VisibleTermsConditions(true);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnSignUpProcess(GameBerry.TheBackEnd.LoginType logintype)
        {
            TheBackEndManager.Instance.PlayLogin(logintype);
            m_appInitUI.VisibleLoginProcess(false);
        }
        //------------------------------------------------------------------------------------
        private void LoginResult(Event.LoginResultMsg msg)
        {
            if (msg.IsSuccess == true)
            {
                if (m_appInitUI != null)
                {
                    m_appInitUI.VisibleLoginProcess(false);
                    //m_appInitUI.SetCallBack_TouchToStart(StartClient);
                    //m_appInitUI.VisibleTouchToStart(true);
                    StartClient();
                }
            }
            else
            {
                if (msg.IsTokenLoginError == true)
                {
                    OnSignUpMenual();
                    ShowNoticeText(string.Empty);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void ShowNoticeText(string noticetext)
        {
            if (m_appInitUI != null)
                m_appInitUI.SetNoticeText(noticetext);
        }
        //------------------------------------------------------------------------------------
        private void StartClient()
        {
            if (m_loginCompleteCallBack != null)
                m_loginCompleteCallBack();
        }
        //------------------------------------------------------------------------------------
    }
}