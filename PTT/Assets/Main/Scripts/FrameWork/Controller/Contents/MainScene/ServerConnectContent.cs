using System.Collections.Generic;
using UnityEngine;
using GameBerry.UI;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections;
using GameBerry.Managers;
using GameBerry.TheBackEnd;
using GameBerry.Event;
using System.Linq;

namespace GameBerry.Contents
{
    public class ServerConnectContent : IContent
    { // 서버 붙이기만 하긴 뭐해서 서버 접속, 로그인까지 한다.
        private AppLoadingDialog _appLoadingDialog;

        //------------------------------------------------------------------------------------
        protected override void OnLoadStart()
        {
            Message.AddListener<LoginResultMsg>(LoginResult);

            _appLoadingDialog = UIManager.Get<AppLoadingDialog>() as AppLoadingDialog;

            ConnectServer();
        }
        //------------------------------------------------------------------------------------
        public void ShowNoticeText(string noticetext)
        {
            if (_appLoadingDialog != null)
                _appLoadingDialog.SetNoticeText(noticetext);
        }
        //------------------------------------------------------------------------------------
        private void ConnectServer()
        {
            ShowNoticeText(LocalStringManager.Instance.GetLocalString("LogIn_Loading_Initialization"));
            if (TheBackEnd.TheBackEndManager.Instance.InitBackEnd() == false)
                return;

            BuildEnvironmentEnum BuildElement = SceneManager.Instance.BuildElement;

            if (BuildElement != BuildEnvironmentEnum.Develop)
            { // 서버 버전 확인
#if !UNITY_EDITOR
        var bro = BackEnd.Backend.Utils.GetLatestVersion();
            if (bro.IsSuccess() == true)
            {
                string version = bro.GetReturnValuetoJSON()["version"].ToString();
                int forceUpdate = bro.GetReturnValuetoJSON()["type"].ToString().ToInt();

                string[] clientServerVersionCut = version.Split('.');
                string[] clientLocalVersionCut = Project.version.Split('.');

                for (int i = 0; i < clientLocalVersionCut.Length; ++i)
                {
                    if(i < 2)
                    {
                        if (int.Parse(clientServerVersionCut[i]) < int.Parse(clientLocalVersionCut[i]))
                        {
                            // 검수서버다
                            if (BuildElement == BuildEnvironmentEnum.Product)
                            {
                                // SceneManager에도 접속 서버 바꿔주기
                                SceneManager.Instance.BuildElement = BuildEnvironmentEnum.Stage;

                                Debug.Log("검수서버다");

                                if (TheBackEnd.TheBackEndManager.Instance.InitBackEnd() == false)
                                    return;
                            }
                        }
                    }
                
                }

                Debug.LogError(string.Format("InitializeApp serverCheckSuccess {0} forceUpdate {1}", version, forceUpdate));
            }
            else
            {
                ProjectNoticeContent.Instance.ShowCheckDialog(Managers.LocalStringManager.Instance.GetLocalString("LogIn_Network_Desc"));
                Debug.LogError(string.Format("InitializeApp serverCheckFail {0}", bro.GetMessage()));
                AOSBackBtnManager.Instance.QuickExitGame = true;
                return;
            }
#endif
            }

            DoLogin();
        }
        //------------------------------------------------------------------------------------
        public void DoLogin()
        {
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
        }
        //------------------------------------------------------------------------------------
        private void OnSignUpMenual()
        {
            if (_appLoadingDialog != null)
            {
                _appLoadingDialog.VisibleLoginProcess(true);
                _appLoadingDialog.SetLoginCallBack(OnSignUpProcess);
                _appLoadingDialog.VisibleLoginButtonGroup(false);
                _appLoadingDialog.VisibleTermsConditions(true);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnSignUpProcess(GameBerry.TheBackEnd.LoginType logintype)
        {
            TheBackEndManager.Instance.PlayLogin(logintype);
            _appLoadingDialog.VisibleLoginProcess(false);
        }
        //------------------------------------------------------------------------------------
        private void LoginResult(LoginResultMsg msg)
        {
            if (msg.IsSuccess == true)
            {
                if (_appLoadingDialog != null)
                {
                    _appLoadingDialog.VisibleLoginProcess(false);
                    FinishServerConnect();
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
        private void FinishServerConnect()
        {
            Message.RemoveListener<LoginResultMsg>(LoginResult);
            _appLoadingDialog = null;
            SetLoadComplete();
        }
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