using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using CodeStage.AntiCheat.ObscuredTypes;
using GameBerry.Contents;

namespace GameBerry.TheBackEnd
{
    public enum LoginType
    {
        None = 0,
        Google,
        Apple,
        Facebook,
        CustomLogin,
    }

    public static class TheBackEnd_Login
    {
        public static Event.SetNoticeMsg m_setNoticeMsg = new Event.SetNoticeMsg();
        public static Event.CreateNickNameResultMsg m_createNickNameResultMsg = new Event.CreateNickNameResultMsg();
        public static Event.RefreshNickNameMsg m_refreshNickNameMsg = new Event.RefreshNickNameMsg();

        private static bool m_setPushState = false;
        private static bool m_setPushNightState = false;

        public static string UserUID = string.Empty;
        public static string UserInDate = string.Empty;
        public static ObscuredDateTime UserJoinDate;
        public static string UserNickName = string.Empty;

        //------------------------------------------------------------------------------------
        public static LoginType CheckLoginType()
        {
#if CUSTOM_LOGIN
            return LoginType.CustomLogin;
#endif
            return (LoginType)PlayerPrefs.GetInt(Define.LoginTypeKey, 0);
        }
        //------------------------------------------------------------------------------------
        public static void DoCustomSignUp()
        {
            string customID = hardwareID;

#if UNITY_EDITOR
            if (Managers.SceneManager.Instance.AuthOverride == true)
            {
                customID += Managers.SceneManager.Instance.AuthOverrideID;
            }
#endif

            m_setNoticeMsg.NoticeStr = Managers.LocalStringManager.Instance.GetLocalString("title/login");
            Message.Send(m_setNoticeMsg);

            SendQueue.Enqueue(Backend.BMember.CustomSignUp, customID, customID, callback => {
                if (callback.IsSuccess())
                {
                    Debug.Log("회원가입에 성공했습니다");

                    DoCustomLogin();
                }
                else
                {
                    if (callback.GetStatusCode() == "409")
                        DoCustomLogin();
                    else
                    {
                        m_setNoticeMsg.NoticeStr = Managers.LocalStringManager.Instance.GetLocalString("error/login");
                        Message.Send(m_setNoticeMsg);

                        TheBackEndManager.Instance.BackEndErrorCode(callback);
                    }
                }

                
            });
        }
        //------------------------------------------------------------------------------------
        public static void DoCustomLogin()
        {
            Debug.Log("DoCustomLogin");

            string customID = hardwareID;

#if UNITY_EDITOR
            if (Managers.SceneManager.Instance.AuthOverride == true)
            {
                customID += Managers.SceneManager.Instance.AuthOverrideID;
            }
#endif

            m_setNoticeMsg.NoticeStr = Managers.LocalStringManager.Instance.GetLocalString("title/login");
            Message.Send(m_setNoticeMsg);

            SendQueue.Enqueue(Backend.BMember.CustomLogin, customID, customID, callback =>
            {
                if (callback.IsSuccess())
                {
                    Debug.Log("로그인에 성공했습니다");

                    CompleteLogin();
                }
                else
                {
                    m_setNoticeMsg.NoticeStr = Managers.LocalStringManager.Instance.GetLocalString("error/login");
                    Message.Send(m_setNoticeMsg);

                    if (callback.IsMaintenanceError())
                    {
                        CheckLoginMaintenance();
                        return;
                    }

                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void DoGoogleLogin()
        {
            Debug.Log("DoGoogleLogin");

            m_setNoticeMsg.NoticeStr = Managers.LocalStringManager.Instance.GetLocalString("title/login");
            Message.Send(m_setNoticeMsg);

            TheBackend.ToolKit.GoogleLogin.Android.GoogleLogin((isSuccess, errorMessage, token) =>
            {
                if (isSuccess == false)
                {
                    m_setNoticeMsg.NoticeStr = Managers.LocalStringManager.Instance.GetLocalString("error/login");
                    Message.Send(m_setNoticeMsg);

                    if (errorMessage.Contains("Sign in failed"))
                    {
                        TheBackEnd.TheBackEndManager.Instance.mainThreadQueue.Enqueue(ProjectNoticeContent.Instance.ShowGooglePlayServiceUpdate);
                        //ProjectNoticeContent.Instance.ShowGooglePlayServiceUpdate();
                    }

                    Debug.LogError(errorMessage);
                    return;
                }

                SendQueue.Enqueue(Backend.BMember.AuthorizeFederation, token, FederationType.Google, "GPGS로 가입함", callback =>
                {
                    // 페더레이션 인증 이후 처리
                    if (callback.IsSuccess() == false)
                    {
                        m_setNoticeMsg.NoticeStr = Managers.LocalStringManager.Instance.GetLocalString("error/login");
                        Message.Send(m_setNoticeMsg);

                        if (callback.IsMaintenanceError())
                        {
                            CheckLoginMaintenance();
                            return;
                        }

                        TheBackEndManager.Instance.BackEndErrorCode(callback);
                    }
                    else
                    {
                        CompleteLogin();
                    }
                });
            });
        }
        //------------------------------------------------------------------------------------
        public static void DoAppleLogin()
        {
            Debug.Log("DoAppleLogin");

            m_setNoticeMsg.NoticeStr = Managers.LocalStringManager.Instance.GetLocalString("title/login");
            Message.Send(m_setNoticeMsg);

            //UnityPlugins.appleLogin.Request(
            //    (identityToken) =>
            //    {
            //        SendQueue.Enqueue(Backend.BMember.AuthorizeFederation, identityToken, FederationType.Apple, "APPLE로 가입함", callback =>
            //        {
            //            // 페더레이션 인증 이후 처리
            //            if (callback.IsSuccess() == false)
            //            {
            //                m_setNoticeMsg.NoticeStr = Managers.LocalStringManager.Instance.GetLocalString("error/login");
            //                Message.Send(m_setNoticeMsg);

            //                if (callback.IsMaintenanceError())
            //                {
            //                    CheckLoginMaintenance();
            //                    return;
            //                }

            //                TheBackEndManager.Instance.BackEndErrorCode(callback);
            //            }
            //            else
            //            {
            //                CompleteLogin();
            //            }
            //        });
            //    },
            //    error =>
            //    {
            //        m_setNoticeMsg.NoticeStr = Managers.LocalStringManager.Instance.GetLocalString("error/login");
            //        Message.Send(m_setNoticeMsg);

            //        Log.ClientError(error);
            //    }
            //);
        }
        //------------------------------------------------------------------------------------
        private static void CompleteLogin()
        {
            if (Managers.SceneManager.Instance.BuildElement != BuildEnvironmentEnum.Develop)
            {
#if !UNITY_EDITOR
            if (TheBackEndManager.Instance.CheckServerVersion() == false)
                return;
#endif
            }

            

            System.DateTime dateTime = System.DateTime.Parse(Backend.UserInDate);
            dateTime = dateTime.ToUniversalTime();
            UserJoinDate = dateTime;
            UserInDate = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            UserNickName = Backend.UserNickName;

            if (CheckLoginType() == LoginType.None)
                SetPushState(m_setPushState, m_setPushNightState);

            if (string.IsNullOrEmpty(Backend.UserNickName) == true)
            {
                PlayerPrefs.DeleteAll();
                UserNickName = Backend.UID;
                CreateNickName(Backend.UID, null);
            }

            
            
            PlayerPrefs.SetInt(Define.LoginTypeKey, (int)TheBackEndManager.Instance.UserLoginType);
            PlayerPrefs.Save();

            m_setNoticeMsg.NoticeStr = Managers.LocalStringManager.Instance.GetLocalString("common/loginCompleted");
            Message.Send(m_setNoticeMsg);

            

            TheBackEndManager.Instance.InitConnectNotification();

            GetUserData();
        }
        //------------------------------------------------------------------------------------
        public static void GetUserData()
        {
            m_setNoticeMsg.NoticeStr = Managers.LocalStringManager.Instance.GetLocalString("title/user");
            Message.Send(m_setNoticeMsg);

            SendQueue.Enqueue(Backend.BMember.GetUserInfo, callback =>
            {
                if (callback.IsSuccess())
                {
                    UserUID = callback.GetReturnValuetoJSON()["row"]["gamerId"].ToString();

                    Debug.Log("유저아이디 : " + UserUID);
                    ThirdPartyLog.Instance.SetCustomUserID(UserUID);
                    ThirdPartyLog.Instance.SendLoginEvent(TheBackEndManager.Instance.UserLoginType);

                    Message.Send(new Event.LoginResultMsg { IsSuccess = true });
                }
                else
                {

                    m_setNoticeMsg.NoticeStr = Managers.LocalStringManager.Instance.GetLocalString("error/login");
                    Message.Send(m_setNoticeMsg);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void CheckLoginMaintenance()
        {
            var bro = BackEnd.Backend.Utils.GetServerStatus();

            if (bro.IsSuccess() == true)
            {
                string serverStatus = bro.GetReturnValuetoJSON()["serverStatus"].ToString();

                if (serverStatus == "1")
                {
                    ProjectNoticeContent.Instance.ShowCheckDialog(Managers.LocalStringManager.Instance.GetLocalString("LogIn_Network_Desc"));
                    return;
                }
                else if (serverStatus == "2")
                {
                    GlobalContent.ShowMaintenanceError();
                    return;
                }
            }
            else
            {
                ProjectNoticeContent.Instance.ShowCheckDialog(Managers.LocalStringManager.Instance.GetLocalString("LogIn_Network_Desc"));
                return;
            }
        }
        //------------------------------------------------------------------------------------
        public static void Logout()
        {
            SendQueue.Enqueue(Backend.BMember.Logout, callback => {
                // 페더레이션 인증 이후 처리
                if (callback.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
                else
                {
                    PlayerPrefs.SetInt(Define.LoginTypeKey, 0);
                    PlayerPrefs.Save();
                    Managers.SceneManager.Instance.OnApplicationQuit();
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void WithdrawAccount()
        {
            SendQueue.Enqueue(Backend.BMember.WithdrawAccount, callback => {
                // 페더레이션 인증 이후 처리
                if (callback.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
                else
                {
                    PlayerPrefs.SetInt(Define.LoginTypeKey, 0);
                    PlayerPrefs.Save();
                    Managers.SceneManager.Instance.OnApplicationQuit();
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void ChangeCustomToFederation_Google(System.Action<BackendReturnObject> action)
        {
            Debug.Log("ChangeCustomToFederation_Google");

            TheBackend.ToolKit.GoogleLogin.Android.GoogleLogin((isSuccess, errorMessage, token) =>
            {
                if (isSuccess == false)
                {
                    if (errorMessage.Contains("GetCredentialException"))
                    {
                        ProjectNoticeContent.Instance.ShowGooglePlayServiceUpdate();
                    }
                    return;
                }

                SendQueue.Enqueue(Backend.BMember.ChangeCustomToFederation, token, FederationType.Google, callback =>
                {
                    // 페더레이션 인증 이후 처리
                    if (callback.IsSuccess() == false)
                    {
                        TheBackEndManager.Instance.BackEndErrorCode(callback);
                    }
                    else
                    {
                        TheBackEndManager.Instance.UserLoginType = LoginType.Google;
                        PlayerPrefs.SetInt(Define.LoginTypeKey, (int)TheBackEndManager.Instance.UserLoginType);
                        PlayerPrefs.Save();
                    }

                    action?.Invoke(callback);
                });
            }
            );
        }
        //------------------------------------------------------------------------------------
        public static void ChangeCustomToFederation_Apple(System.Action<BackendReturnObject> action)
        {
            Debug.Log("DoAppleLogin");

            //UnityPlugins.appleLogin.Request(
            //    (identityToken) =>
            //    {
            //        SendQueue.Enqueue(Backend.BMember.ChangeCustomToFederation, identityToken, FederationType.Apple, callback =>
            //        {
            //            // 페더레이션 인증 이후 처리
            //            if (callback.IsSuccess() == false)
            //            {
            //                TheBackEndManager.Instance.BackEndErrorCode(callback);
            //            }
            //            else
            //            {
            //                TheBackEndManager.Instance.UserLoginType = LoginType.Apple;
            //                PlayerPrefs.SetInt(Define.LoginTypeKey, (int)TheBackEndManager.Instance.UserLoginType);
            //                PlayerPrefs.Save();
            //            }

            //            action?.Invoke(callback);
            //        });
            //    },
            //    error =>
            //    {
            //        Log.ClientError(error);
            //    }
            //);
        }
        //------------------------------------------------------------------------------------
        public static void SavePushState(bool push, bool pushNight)
        {
            m_setPushState = push;
            m_setPushNightState = pushNight;
        }
        //------------------------------------------------------------------------------------
        public static void SetPushState(bool push, bool pushNight)
        {
#if UNITY_EDITOR
            PlayerPrefs.SetInt(Managers.GameSettingBtn.Push.ToString(), push == true ? 1 : 0);
            PlayerPrefs.SetInt(Managers.GameSettingBtn.PushNight.ToString(), pushNight == true ? 1 : 0);
            PlayerPrefs.Save();
            return;
#endif
            bool pushed = PlayerPrefs.GetInt(Managers.GameSettingBtn.Push.ToString(), 0) == 1;
            bool pushnighted = PlayerPrefs.GetInt(Managers.GameSettingBtn.PushNight.ToString(), 0) == 1;

#if UNITY_IOS
            if (push == true)
            {
                string token = Managers.SceneManager.Instance.IOSDeviceToken;

                SendQueue.Enqueue(Backend.iOS.PutDeviceToken, token, isDevelopment.iosDev, (callback) =>
                {
                    // 이후 처리
                    if (callback.IsSuccess() == true)
                        PlayerPrefs.SetInt(Managers.GameSettingBtn.Push.ToString(), push == true ? 1 : 0);
                    else
                        TheBackEndManager.Instance.BackEndErrorCode(callback);

                        PlayerPrefs.Save();
                });
            }
            else
            {
                SendQueue.Enqueue(Backend.iOS.DeleteDeviceToken, (callback) =>
                {
                    // 이후 처리
                    if (callback.IsSuccess() == true)
                        PlayerPrefs.SetInt(Managers.GameSettingBtn.Push.ToString(), push == true ? 1 : 0);
                    else
                        TheBackEndManager.Instance.BackEndErrorCode(callback);

                        PlayerPrefs.Save();
                });
            }

            SendQueue.Enqueue(Backend.iOS.AgreeNightPushNotification, pushNight, (callback) => {
                // 이후 처리
                if (callback.IsSuccess() == true)
                    PlayerPrefs.SetInt(Managers.GameSettingBtn.PushNight.ToString(), pushNight == true ? 1 : 0);
                else
                    TheBackEndManager.Instance.BackEndErrorCode(callback);

                    PlayerPrefs.Save();
            });
#elif UNITY_ANDROID
            if (push == true)
            {
                SendQueue.Enqueue(Backend.Android.PutDeviceToken, ThirdPartyLog.Instance.GetFirebaseMessagingToken(), (callback) =>
                {
                    // 이후 처리
                    if (callback.IsSuccess() == true)
                        PlayerPrefs.SetInt(Managers.GameSettingBtn.Push.ToString(), push == true ? 1 : 0);
                    else
                        TheBackEndManager.Instance.BackEndErrorCode(callback);

                    PlayerPrefs.Save();
                });
            }
            else
            {
                SendQueue.Enqueue(Backend.Android.DeleteDeviceToken, (callback) =>
                {
                    // 이후 처리
                    if (callback.IsSuccess() == true)
                        PlayerPrefs.SetInt(Managers.GameSettingBtn.Push.ToString(), push == true ? 1 : 0);
                    else
                        TheBackEndManager.Instance.BackEndErrorCode(callback);

                    PlayerPrefs.Save();
                });
            }

            SendQueue.Enqueue(Backend.Android.AgreeNightPushNotification, pushNight, (callback) => {
                // 이후 처리
                if (callback.IsSuccess() == true)
                    PlayerPrefs.SetInt(Managers.GameSettingBtn.PushNight.ToString(), pushNight == true ? 1 : 0);
                else
                    TheBackEndManager.Instance.BackEndErrorCode(callback);

                PlayerPrefs.Save();
            });
#endif

        }
        //------------------------------------------------------------------------------------
        public static void SetPush(bool push)
        {
            Managers.GameSettingManager.Instance.SetPushState(Managers.GameSettingBtn.Push, push);

            bool pushed = PlayerPrefs.GetInt(Managers.GameSettingBtn.Push.ToString(), 0) == 1;

#if UNITY_EDITOR
            if (pushed != push)
            {
                PlayerPrefs.SetInt(Managers.GameSettingBtn.Push.ToString(), push == true ? 1 : 0);

                PlayerPrefs.Save();
            }

            return;
#endif
            

            if (pushed != push)
            {
#if UNITY_IOS
                if (push == true)
                {
                    string token = Managers.SceneManager.Instance.IOSDeviceToken;

                    SendQueue.Enqueue(Backend.iOS.PutDeviceToken, token, isDevelopment.iosDev, (callback) =>
                    {
                        // 이후 처리
                        if (callback.IsSuccess() == true)
                        {
                            string enablePushstr = push == true ? Managers.LocalStringManager.Instance.GetLocalString("TermsConditions_Push_OK") : Managers.LocalStringManager.Instance.GetLocalString("TermsConditions_Push_NO");

                            string noticemsg = string.Format("{0}\n{1}", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), enablePushstr);

                            Contents.GlobalContent.ShowGlobalNotice(noticemsg);
                            PlayerPrefs.SetInt(Managers.GameSettingBtn.Push.ToString(), push == true ? 1 : 0);
                            PlayerPrefs.Save();
                        }
                        else
                            TheBackEndManager.Instance.BackEndErrorCode(callback);
                    });
                }
                else
                {
                    SendQueue.Enqueue(Backend.iOS.DeleteDeviceToken, (callback) =>
                    {
                        if (callback.IsSuccess() == true)
                        {
                            string enablePushstr = push == true ? Managers.LocalStringManager.Instance.GetLocalString("TermsConditions_Push_OK") : Managers.LocalStringManager.Instance.GetLocalString("TermsConditions_Push_NO");

                            string noticemsg = string.Format("{0}\n{1}", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), enablePushstr);

                            Contents.GlobalContent.ShowGlobalNotice(noticemsg);
                            PlayerPrefs.SetInt(Managers.GameSettingBtn.Push.ToString(), push == true ? 1 : 0);
                            PlayerPrefs.Save();
                        }
                        else
                            TheBackEndManager.Instance.BackEndErrorCode(callback);
                    });
                }
#elif UNITY_ANDROID
                if (push == true)
                {
                    SendQueue.Enqueue(Backend.Android.PutDeviceToken, ThirdPartyLog.Instance.GetFirebaseMessagingToken(), (callback) =>
                    {
                        // 이후 처리
                        if (callback.IsSuccess() == true)
                        {
                            string enablePushstr = push == true ? Managers.LocalStringManager.Instance.GetLocalString("TermsConditions_Push_OK") : Managers.LocalStringManager.Instance.GetLocalString("TermsConditions_Push_NO");

                            string noticemsg = string.Format("{0}\n{1}", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), enablePushstr);

                            Contents.GlobalContent.ShowGlobalNotice(noticemsg);
                            PlayerPrefs.SetInt(Managers.GameSettingBtn.Push.ToString(), push == true ? 1 : 0);
                            PlayerPrefs.Save();
                        }
                        else
                            TheBackEndManager.Instance.BackEndErrorCode(callback);
                    });
                }
                else
                {
                    SendQueue.Enqueue(Backend.Android.DeleteDeviceToken, (callback) =>
                    {
                        if (callback.IsSuccess() == true)
                        {
                            string enablePushstr = push == true ? Managers.LocalStringManager.Instance.GetLocalString("TermsConditions_Push_OK") : Managers.LocalStringManager.Instance.GetLocalString("TermsConditions_Push_NO");

                            string noticemsg = string.Format("{0}\n{1}", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), enablePushstr);

                            Contents.GlobalContent.ShowGlobalNotice(noticemsg);
                            PlayerPrefs.SetInt(Managers.GameSettingBtn.Push.ToString(), push == true ? 1 : 0);
                            PlayerPrefs.Save();
                        }
                        else
                            TheBackEndManager.Instance.BackEndErrorCode(callback);
                    });
                }
#endif

            }
        }
        //------------------------------------------------------------------------------------
        public static void SetPushNight(bool pushNight)
        {
            Managers.GameSettingManager.Instance.SetPushState(Managers.GameSettingBtn.PushNight, pushNight);

            bool pushnighted = PlayerPrefs.GetInt(Managers.GameSettingBtn.PushNight.ToString(), 0) == 1;

#if UNITY_EDITOR
            if (pushnighted != pushNight)
            {
                PlayerPrefs.SetInt(Managers.GameSettingBtn.PushNight.ToString(), pushNight == true ? 1 : 0);
                PlayerPrefs.Save();
            }

            return;
#endif

            if (pushnighted != pushNight)
            {
#if UNITY_IOS
                SendQueue.Enqueue(Backend.iOS.AgreeNightPushNotification, pushNight, (callback) => {
                    // 이후 처리
                    if (callback.IsSuccess() == true)
                    {
                        string enablePushstr = pushNight == true ? Managers.LocalStringManager.Instance.GetLocalString("TermsConditions_PushNight_OK") : Managers.LocalStringManager.Instance.GetLocalString("TermsConditions_PushNight_NO");
                        string noticemsg = string.Format("{0}\n{1}", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), enablePushstr);
                        Contents.GlobalContent.ShowGlobalNotice(noticemsg);
                        PlayerPrefs.SetInt(Managers.GameSettingBtn.PushNight.ToString(), pushNight == true ? 1 : 0);
                        PlayerPrefs.Save();
                    }
                    else
                            TheBackEndManager.Instance.BackEndErrorCode(callback);
                });

#elif UNITY_ANDROID
                SendQueue.Enqueue(Backend.Android.AgreeNightPushNotification, pushNight, (callback) => {
                    // 이후 처리
                    if (callback.IsSuccess() == true)
                    {
                        string enablePushstr = pushNight == true ? Managers.LocalStringManager.Instance.GetLocalString("TermsConditions_PushNight_OK") : Managers.LocalStringManager.Instance.GetLocalString("TermsConditions_PushNight_NO");
                        string noticemsg = string.Format("{0}\n{1}", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), enablePushstr);
                        Contents.GlobalContent.ShowGlobalNotice(noticemsg);
                        PlayerPrefs.SetInt(Managers.GameSettingBtn.PushNight.ToString(), pushNight == true ? 1 : 0);
                        PlayerPrefs.Save();
                    }
                    else
                        TheBackEndManager.Instance.BackEndErrorCode(callback);
                });

#endif
            }
        }
        //------------------------------------------------------------------------------------
        private static bool benNickNameChange = false;
        public static void CreateNickName(string nickname, System.Action<BackendReturnObject> action)
        {
            if (benNickNameChange == true)
                return;

            benNickNameChange = true;

            SendQueue.Enqueue(Backend.BMember.CreateNickname, nickname, callback =>
            {
                if (callback.IsSuccess() == true)
                {
                    Debug.Log("NickNameCreate!!");

                    m_createNickNameResultMsg.IsSuccess = callback.IsSuccess();
                    m_createNickNameResultMsg.ErrorMessage = callback.IsSuccess() == true ? string.Empty : callback.GetMessage();

                    Message.Send(m_createNickNameResultMsg);

                    Message.Send(m_refreshNickNameMsg);
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }

                action?.Invoke(callback);

                benNickNameChange = false;
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdateNickname(string nickname, System.Action<BackendReturnObject> action)
        {
            if (benNickNameChange == true)
                return;

            benNickNameChange = true;

            SendQueue.Enqueue(Backend.BMember.UpdateNickname, nickname, callback =>
            {
                if (callback.IsSuccess() == true)
                {
                    Debug.Log("NickNameCreate!!");

                    PlayerDataContainer.PlayerName = Backend.UserNickName;

                    m_createNickNameResultMsg.IsSuccess = callback.IsSuccess();
                    m_createNickNameResultMsg.ErrorMessage = callback.IsSuccess() == true ? string.Empty : callback.GetMessage();

                    Message.Send(m_createNickNameResultMsg);

                    Message.Send(m_refreshNickNameMsg);
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }

                action?.Invoke(callback);

                benNickNameChange = false;
            });
        }
        //------------------------------------------------------------------------------------
        public static string hardwareID
        {
            get
            {
#if UNITY_IOS
            string uuid = FSG.iOSKeychain.Keychain.GetValue("uuid");            
            if(string.IsNullOrEmpty(uuid) == true)
            {
                uuid = SystemInfo.deviceUniqueIdentifier;
                FSG.iOSKeychain.Keychain.SetValue("uuid", uuid);
            }

            return uuid;
#endif
                return SystemInfo.deviceUniqueIdentifier;
            }
        }
    }
}