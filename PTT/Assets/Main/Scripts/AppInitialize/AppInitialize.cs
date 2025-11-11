using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.TheBackEnd;

namespace GameBerry
{
    public class AppInitialize : MonoBehaviour
    {
        private AppInitializeUI m_appInitUI;

        private System.Action m_loginCompleteCallBack; // 로그인이 끝나면 호출

        //------------------------------------------------------------------------------------
        public void Init()
        {
            ResourceLoader.Instance.Load<GameObject>("AppInitialize/AppInitializeUI", o =>
            {
                GameObject clone = Instantiate(o, UI.UIManager.Instance.ProjectLoadingContent) as GameObject;
                if (clone != null)
                    m_appInitUI = clone.GetComponent<AppInitializeUI>();

                if (m_appInitUI != null)
                    m_appInitUI.Init();
            });

            Message.AddListener<Event.CreateNickNameMsg>(CreateNickName);
            Message.AddListener<Event.LoginResultMsg>(LoginResult);

        }
        //------------------------------------------------------------------------------------
        public void Release()
        {
            Message.RemoveListener<Event.CreateNickNameMsg>(CreateNickName);
            Message.RemoveListener<Event.LoginResultMsg>(LoginResult);

            if (m_appInitUI != null)
                m_appInitUI.Release();

            Destroy(m_appInitUI.gameObject);
        }
        //------------------------------------------------------------------------------------
        public bool InitBackEnd()
        {
            return GameBerry.TheBackEnd.TheBackEndManager.Instance.InitBackEnd();
        }
        //------------------------------------------------------------------------------------
        public void PlayFlashParticle()
        {
            if (m_appInitUI != null)
                m_appInitUI.PlayFlashParticle();
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

//#if UNITY_EDITOR
//            if (m_appInitUI != null)
//            {
//                m_appInitUI.SetLoginCallBack(OnSignUpProcess);
//                m_appInitUI.VisibleLoginButtonGroup(true);
//                GamePotManager.Instance.PlayCustomLogin();
//            }
//#else
//            if (GamePotManager.Instance.CheckNeedLogin() == true)
//            {
//                if (m_appInitUI != null)
//                {
//                    m_appInitUI.VisibleLoginProcess(true);
//                    m_appInitUI.SetLoginCallBack(OnSignUpProcess);
//                    m_appInitUI.VisibleLoginButtonGroup(false);
//                    m_appInitUI.VisibleTermsConditions(true);

//                    ShowNoticeText(string.Empty);
//                }
//            }
//            else
//            {
//                OnSignUpProcess(GamePotManager.Instance.GetLastLoginType());
//            }
//#endif

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
        private void CreateNickName(Event.CreateNickNameMsg msg)
        {
            Start_CreateNickName();
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
        private void Start_CreateNickName()
        {
            if (m_appInitUI != null)
            {
                m_appInitUI.VisibleLoginProcess(true);
                m_appInitUI.VisibleLoginProcess(true);
                m_appInitUI.VisibleLoginButtonGroup(false);
                m_appInitUI.VisibleCreateNickName(true);
                m_appInitUI.SetNickNameCallBack(OnNickNameProcess);
            }
        }
        //------------------------------------------------------------------------------------
        private bool m_benSetNickName = false;
        private string prevName = string.Empty;
        private void OnNickNameProcess(string nickname)
        {
            if (m_benSetNickName == true)
                return;

            if (prevName == nickname)
                return;

            m_benSetNickName = false;

            prevName = nickname;

            //DKServerManager.Instance.API_Player_Set_NameRequest(nickname, API_Player_Set_NameResponse);
        }
        //------------------------------------------------------------------------------------
        //private void API_Player_Set_NameResponse(PlayerSetNameResponse playerSetNameResponse)
        //{
        //    if (playerSetNameResponse.error_code != 0)
        //    {
        //        m_benSetNickName = false;

        //        m_appInitUI.SetNickNameError(Managers.LocalStringManager.Instance.GetLocalString("PlayerNick_UI_Overlap"));
        //        Debug.LogError(playerSetNameResponse.error_message);
        //        return;
        //    }

        //    ThirdPartyLog.Instance.SendLog_NickeEvent(null, playerSetNameResponse.player.player_name, null, null, null, null);

        //    //DKServerManager.Instance.CompletePlayerNickName(playerSetNameResponse);

        //    if (m_appInitUI != null)
        //    {
        //        m_appInitUI.VisibleLoginProcess(false);
        //    }
        //}
        //------------------------------------------------------------------------------------
#if DEV_DEFINE
        private bool isTouchBegin = false;
        private float touchTime = 0f;

        private void Update()
        {
            if (Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.Develop
                || Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.QA)
            {
                if (Input.GetMouseButton(0))
                {
                    if (isTouchBegin == false)
                    {
                        isTouchBegin = true;
                        touchTime = Time.unscaledTime;
                    }
                    else
                    {
                        if (Time.unscaledTime - touchTime >= 3.0f)
                        {
                            isTouchBegin = false;
                            touchTime = 0;
                            Gpm.LogViewer.GpmLogViewer.Instance.Show();
                        }
                    }
                }
                else
                {
                    isTouchBegin = false;
                    touchTime = 0;
                }
            }
        }
#endif
        //------------------------------------------------------------------------------------
    }
}