using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using BackEnd;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace GameBerry.TheBackEnd
{
    public class TheBackEndManager : MonoSingleton<TheBackEndManager>
    {
        public LoginType UserLoginType = LoginType.None;

        public static bool ReadyUpdateData = false;

        private static float updateWaitDataTimer = 0.0f;

        private static float updateWaitDataTimerTurm = 300.0f;

        private Coroutine disConnectGameCoroutine;

        private bool netWorkState = true;
        private bool sendRecvUpdateCheck = false;

        private bool isShowMaintenanceNotice = false;

        public Queue<Action> mainThreadQueue = new Queue<Action>();

        private bool isCheatingUser = false;

        private bool checkServerState = false;

        private bool sendreport = false;

        //------------------------------------------------------------------------------------
        public bool InitBackEnd()
        {
            BackEnd.MultiSettings.MultiProject multiProject = BackEnd.MultiSettings.MultiSettingManager.FindByProjectName(Managers.SceneManager.Instance.BuildElement.ToString());

            if (multiProject == null)
            {
                Debug.LogError("해당 프로젝트를 찾을 수 없습니다.");
            }

            var bro = Backend.InitializeByMultiProject(multiProject);

            //var bro = Backend.Initialize(true);

            if (bro.IsSuccess() == false)
            {
                BackEndErrorCode(bro);
            }
            //else
            //{
            //    InitConnectNotification();
            //}

            if (SendQueue.IsInitialize == false)
                SendQueue.StartSendQueue(true, ExceptionHandler);

            ThirdPartyLog.Instance.InitThirdParty();

            CodeStage.AntiCheat.Detectors.ObscuredCheatingDetector.StartDetection(OnCheatingDetected);

            return bro.IsSuccess();
        }
        //------------------------------------------------------------------------------------
        public string GetGoogleHash()
        {
            return Backend.Utils.GetGoogleHash();
        }
        //------------------------------------------------------------------------------------
        public void InitConnectNotification()
        {
            Backend.Notification.OnAuthorize = (bool Result, string Reason) =>
            {
                Debug.Log("실시간 알림 서버 접속 시도!");

                //접속 이후 처리
                if (Result)
                {
                    Debug.Log("실시간 알림 서버 접속 성공!");

                    Backend.Notification.OnReceivedUserPost = () => {
                        Debug.Log("새 유저 우편이 도착했습니다!");
                    };

                    Backend.Notification.OnServerStatusChanged = (serverStatusType) => {

                        if (serverStatusType == BackEnd.Socketio.ServerStatusType.Maintenance
                        || serverStatusType == BackEnd.Socketio.ServerStatusType.Offline)
                        {
                            mainThreadQueue.Enqueue(() =>
                            {
                                ShowDisConnectGame("system/underMaintenance");
                            });

                            //점검
                        }
                    };

                    Backend.Notification.OnNewPostCreated = (postRepeatType, title, content, author) => {
                        Debug.Log(
        $"[OnNewPostCreated(새로운 우편 생성)]\n" +
        $"| postRepeatType : {postRepeatType}\n" +
        $"| title : {title}\n" +
        $"| content : {content}\n" +
        $"| author : {author}\n"
    );
                        //mainThreadQueue.Enqueue(() =>
                        //{
                        //    Managers.PostManager.Instance.OnNewPostCreated(postRepeatType, title, content, author);
                        //});
                    };

                    Backend.Notification.OnNewNoticeCreated = (string title, string content) => {
                        Debug.Log(
                            $"[OnNewNoticeCreated(새로운 공지사항 생성)]\n" +
                            $"| title : {title}\n" +
                            $"| content : {content}\n"
                        );

                        mainThreadQueue.Enqueue(Managers.NoticeManager.Instance.RefreshNotice);
                    };

                }
                else
                {
                    Debug.Log("실시간 알림 서버 접속 실패 : 이유 : " + Reason);
                }
            };

            //접속 해제 시 반응하는 핸들러를 설정.
            Backend.Notification.OnDisConnect = (string Reason) => {
                Debug.Log("해제 이유 : " + Reason);
                Debug.Log("다시 연결");
                Backend.Notification.Connect();
            };

            // 실시간 알림 서버로 연결
            Backend.Notification.Connect();

            checkServerState = true;
        }
        //------------------------------------------------------------------------------------
        public void EnableSendRecvUpdateCheck()
        {
            sendRecvUpdateCheck = true;
            updateWaitDataTimer = Time.time + updateWaitDataTimerTurm;


            Managers.NetworkChecker.Instance.StartNetworkCheck();
            netWorkState = Managers.NetworkChecker.Instance.IsConnected();

            Managers.NetworkChecker.Instance.OnDisconnected += OnDisconnect;
            Managers.NetworkChecker.Instance.OnReconnected += OnReconnected;
        }
        //------------------------------------------------------------------------------------
        private void OnDisconnect()
        {
            netWorkState = false;
        }
        //------------------------------------------------------------------------------------
        private void OnReconnected()
        {
            netWorkState = true;
            Table.UserTable.AllUpdata();
        }
        //------------------------------------------------------------------------------------
        public bool CheckNetworkState()
        {
            if (netWorkState == false)
            {
                Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("LogIn_Network_Desc"));
                return false;
            }

            return true;
        }
        //------------------------------------------------------------------------------------
        public void OnAutoSave()
        {
            ReadyUpdateData = true;
        }
        //------------------------------------------------------------------------------------
        public void CheckSelfReport()
        {
            if (sendreport == true)
                return;


            //if (ShopContainer.TotalBuyPrice > 0)
            //    return;

            //int daycount = Managers.TimeManager.Instance.GetDayCount();
            //if (daycount <= 3)
            //{
            //    double diacount = PointDataContainer.DiaAmountRecord + PointDataContainer.AccumUseDia.GetDecrypted();
            //    if (diacount > 2000000)
            //    {
            //        SendSelfReport();
            //        return;
            //    }

            //    foreach (var pair in SummonContainer.m_summonInfo)
            //    {
            //        if(pair.Value.Level >= 9)
            //        {
            //            SendSelfReport();
            //            return;
            //        }
            //    }
            //}
        }
        //------------------------------------------------------------------------------------
        public void SendSelfReport()
        {
            sendreport = true;

            //Param viewParam = TheBackEnd_PlayerTable.GetPlayerViewDataParam();

            //Backend.Question.SendQuestion(QuestionType.Report, GetUserID(), viewParam.GetJson(), (callback) =>
            //{

            //});
        }
        //------------------------------------------------------------------------------------
        #region TheBackEnd_Login
        //------------------------------------------------------------------------------------
        public bool IsNeedSignUp()
        {
            return TheBackEnd_Login.CheckLoginType() == LoginType.None;
        }
        //------------------------------------------------------------------------------------
        public LoginType CheckLoginState()
        {
            return TheBackEnd_Login.CheckLoginType();
        }
        //------------------------------------------------------------------------------------
        public void PlayLogin(LoginType logintype)
        {
            UserLoginType = logintype;
            switch (logintype)
            {
                case LoginType.CustomLogin:
                    {
                        TheBackEnd_Login.DoCustomSignUp();
                        break;
                    }
                case LoginType.Google:
                    {
                        TheBackEnd_Login.DoGoogleLogin();
                        break;
                    }
                case LoginType.Apple:
                    {
                        TheBackEnd_Login.DoAppleLogin();
                        break;
                    }
            }
        }
        //------------------------------------------------------------------------------------
        public void ChangeCustomToFederation_Google(Action<BackendReturnObject> action)
        {
            TheBackEnd_Login.ChangeCustomToFederation_Google(action);
        }
        //------------------------------------------------------------------------------------
        public void ChangeCustomToFederation_Apple(Action<BackendReturnObject> action)
        {
            TheBackEnd_Login.ChangeCustomToFederation_Apple(action);
        }
        //------------------------------------------------------------------------------------
        public void SavePushState(bool push, bool pushNight)
        {
            TheBackEnd_Login.SavePushState(push, pushNight);
        }
        //------------------------------------------------------------------------------------
        public void SetPushState(bool push, bool pushNight)
        {
            TheBackEnd_Login.SetPushState(push, pushNight);
        }
        //------------------------------------------------------------------------------------
        public void Logout()
        {
            TheBackEnd_Login.Logout();
        }
        //------------------------------------------------------------------------------------
        public void WithdrawAccount()
        {
            TheBackEnd_Login.WithdrawAccount();
        }
        //------------------------------------------------------------------------------------
        public void CreateNickName(string nickname, System.Action<BackendReturnObject> action)
        {
            TheBackEnd_Login.CreateNickName(nickname, action);
        }
        //------------------------------------------------------------------------------------
        public void UpdateNickName(string nickname, System.Action<BackendReturnObject> action)
        {
            TheBackEnd_Login.UpdateNickname(nickname, action);
        }
        //------------------------------------------------------------------------------------
        public string GetNickPlayerName()
        {
            return Backend.UserNickName;
        }
        //------------------------------------------------------------------------------------
        public string GetFakeNickPlayerName()
        {
            if (Backend.UID == Backend.UserNickName)
                return Managers.LocalStringManager.Instance.GetLocalString("barbar/tempname");

            return Backend.UserNickName;
        }
        //------------------------------------------------------------------------------------
        public string GetUserID()
        {
            return TheBackEnd_Login.UserUID;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_Probability
        //------------------------------------------------------------------------------------
        public void GetProbabilityCardList()
        {
            TheBackEnd_Probability.GetProbabilityCardList();
        }
        //------------------------------------------------------------------------------------
        public void GetProbabilitys(string chartid, int count, System.Action<LitJson.JsonData> onComplete)
        {
            TheBackEnd_Probability.GetProbabilitys(chartid, count, onComplete);
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_Util
        //------------------------------------------------------------------------------------
        public void GetServerTime(Action<DateTime> action)
        {
            TheBackEnd_Utils.GetServerTime(action);
        }
        //------------------------------------------------------------------------------------
        public void GetPostList(PostType postType, Backend.BackendCallback action)
        {
            TheBackEnd_Utils.GetPostList(postType, action);
        }
        //------------------------------------------------------------------------------------
        public void ReceivePostItem(PostType postType, string indata, Backend.BackendCallback action)
        {
            TheBackEnd_Utils.ReceivePostItem(postType, indata, action);
        }
        //------------------------------------------------------------------------------------
        public void ReceivePostItemAll(PostType postType, Backend.BackendCallback action)
        {
            TheBackEnd_Utils.ReceivePostItemAll(postType, action);
        }
        //------------------------------------------------------------------------------------
        public void ReceiveCoupon(string coupon, Backend.BackendCallback action)
        {
            TheBackEnd_Utils.ReceiveCoupon(coupon, action);
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerTable
        //------------------------------------------------------------------------------------
        public Param GetTableParam(string tableName)
        {
            //switch (tableName)
            //{
            //    case Define.PlayerInfoTable:
            //        return TheBackEnd_PlayerTable.GetPlayerInfoParam();

            //    case Define.PlayerARRRInfoTable:
            //        return TheBackEnd_PlayerTable.GetPlayerARRRInfoParam();

            //    case Define.PlayerStaminaInfoTable:
            //        return TheBackEnd_PlayerTable.GetPlayerStaminaInfoParam();

            //    case Define.PlayerJobInfoTable:
            //        return TheBackEnd_PlayerTable.GetPlayerJobInfoParam();

            //    case Define.PlayerVipPackageInfoTable:
            //        return TheBackEnd_PlayerTable.GetPlayerVipPackageInfoParam();

            //    case Define.PlayerDungeonInfoTable:
            //        return TheBackEnd_PlayerTable.GetPlayerDungeonParam();

            //    case Define.PlayerPointTable:
            //        return TheBackEnd_PlayerTable.GetPlayerPointParam();
            //    case Define.PlayerSummonTicketTable:
            //        return TheBackEnd_PlayerTable.GetPlayerSummonTicketParam();
            //    case Define.PlayerBoxTable:
            //        return TheBackEnd_PlayerTable.GetPlayerBoxParam();

            //    case Define.PlayerGearTable:
            //        return TheBackEnd_PlayerTable.GetPlayerGearParam();
            //    case Define.PlayerSkinTable:
            //        return TheBackEnd_PlayerTable.GetPlayerSkinParam();

            //    case Define.PlayerSkillInfoTable:
            //        return TheBackEnd_PlayerTable.GetPlayerSkillParam();

            //    case Define.PlayerResearchInfoTable:
            //        return TheBackEnd_PlayerTable.GetPlayerResearchParam();

            //    case Define.PlayerMapInfoTable:
            //        return TheBackEnd_PlayerTable.GetPlayerMapParam();
                
            //    case Define.PlayerSummonInfoTable:
            //        return TheBackEnd_PlayerTable.GetPlayerSummonParam();

            //    case Define.PlayerTimeInfoTable:
            //        return TheBackEnd_PlayerTable.GetPlayerTimeParam();
            //    case Define.PlayerAdBuffInfoTable:
            //        return TheBackEnd_PlayerTable.GetPlayerAdBuffParam();
            //    case Define.PlayerCheckInInfoTable:
            //        return TheBackEnd_PlayerTable.GetPlayerCheckInParam();
            //    case Define.PlayerShopInfoTable:
            //        return TheBackEnd_PlayerTable.GetPlayerShopParam();
            //    case Define.PlayerShopRandomStoreInfoTable:
            //        return TheBackEnd_PlayerTable.GetPlayerShopRandomStoreParam();
            //    case Define.PlayerPassInfoTable:
            //        return TheBackEnd_PlayerTable.GetPlayerPassParam();
            //    case Define.PlayerQuestInfoTable:
            //        return TheBackEnd_PlayerTable.GetPlayerQuestParam();
            //    case Define.PlayerTimeAttackMissionInfoTable:
            //        return TheBackEnd_PlayerTable.GetPlayerTimeAttackMissionParam();
            //    case Define.PlayerExchangeInfoTable:
            //        return TheBackEnd_PlayerTable.GetPlayerExchangeParam();
            //    case Define.PlayerRankInfoTable:
            //        return TheBackEnd_PlayerTable.GetPlayerRankParam();
            //}

            return null;
        }
        //------------------------------------------------------------------------------------
        //public void GetPlayerInfoTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerInfoTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerInfoTable()
        //{
        //    TheBackEnd_PlayerTable.UpdatePlayerInfoTable();
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerARRRInfoTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerARRRInfoTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerARRRInfoTable()
        //{
        //    if (updateWaitDatas.ContainsKey(Define.PlayerARRRInfoTable) == true)
        //        updateWaitDatas.Remove(Define.PlayerARRRInfoTable);

        //    TheBackEnd_PlayerTable.UpdatePlayerARRRInfoTable();
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerStaminaInfoTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerStaminaInfoTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerStaminaInfoTable()
        //{
        //    if (updateWaitDatas.ContainsKey(Define.PlayerStaminaInfoTable) == true)
        //        updateWaitDatas.Remove(Define.PlayerStaminaInfoTable);

        //    TheBackEnd_PlayerTable.UpdatePlayerStaminaInfoTable();
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerJobInfoTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerJobInfoTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerJobInfoTable()
        //{
        //    TheBackEnd_PlayerTable.UpdatePlayerJobInfoTable();
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerVipPackageInfoTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerVipPackageInfoTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerVipPackageInfoTable()
        //{
        //    if (updateWaitDatas.ContainsKey(Define.PlayerVipPackageInfoTable) == true)
        //        updateWaitDatas.Remove(Define.PlayerVipPackageInfoTable);

        //    TheBackEnd_PlayerTable.UpdatePlayerVipPackageInfoTable();
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerPointTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerPointTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerPointTable()
        //{
        //    if (updateWaitDatas.ContainsKey(Define.PlayerPointTable) == true)
        //        updateWaitDatas.Remove(Define.PlayerPointTable);

        //    TheBackEnd_PlayerTable.UpdatePlayerPointTable();
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerSummonTicketTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerSummonTicketTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerSummonTicketTable()
        //{
        //    if (updateWaitDatas.ContainsKey(Define.PlayerSummonTicketTable) == true)
        //        updateWaitDatas.Remove(Define.PlayerSummonTicketTable);

        //    TheBackEnd_PlayerTable.UpdatePlayerSummonTicketTable();
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerBoxTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerBoxTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerBoxTable()
        //{
        //    if (updateWaitDatas.ContainsKey(Define.PlayerBoxTable) == true)
        //        updateWaitDatas.Remove(Define.PlayerBoxTable);

        //    TheBackEnd_PlayerTable.UpdatePlayerBoxTable();
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerGearInfoTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerGearInfoTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerGearInfoTable()
        //{
        //    TheBackEnd_PlayerTable.UpdatePlayerGearInfoTable();
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerSkinInfoTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerSkinInfoTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerSkinInfoTable()
        //{
        //    TheBackEnd_PlayerTable.UpdatePlayerSkinInfoTable();
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerSkillinfoTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerSkillinfoTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerSkillInfoTable()
        //{
        //    TheBackEnd_PlayerTable.UpdatePlayerSkillInfoTable();
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerResearchinfoTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerResearchinfoTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerResearchInfoTable()
        //{
        //    TheBackEnd_PlayerTable.UpdatePlayerResearchInfoTable();
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerDungeonInfoTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerDungeonInfoTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerDungeonInfoTable()
        //{
        //    TheBackEnd_PlayerTable.UpdatePlayerDungeonInfoTable();
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerSummoninfoTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerSummoninfoTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerSummonInfoTable()
        //{
        //    TheBackEnd_PlayerTable.UpdatePlayerSummonInfoTable();
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerTimeinfoTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerTimeinfoTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerTimeInfoTable()
        //{
        //    TheBackEnd_PlayerTable.UpdatePlayerTimeInfoTable();
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerAdBuffinfoTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerAdBuffinfoTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerAdBuffInfoTable()
        //{
        //    if (updateWaitDatas.ContainsKey(Define.PlayerAdBuffInfoTable) == true)
        //        updateWaitDatas.Remove(Define.PlayerAdBuffInfoTable);

        //    TheBackEnd_PlayerTable.UpdatePlayerAdBuffInfoTable();
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerMapInfoTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerMapInfoTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerMapInfoTable()
        //{
        //    TheBackEnd_PlayerTable.UpdatePlayerMapInfoTable();
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerCheckInInfoTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerCheckInInfoTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerCheckInInfoTable(System.Action onFinish)
        //{
        //    TheBackEnd_PlayerTable.UpdatePlayerCheckInInfoTable(onFinish);
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerShopInfoTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerShopInfoTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerShopInfoTable(System.Action onFinish)
        //{
        //    if (updateWaitDatas.ContainsKey(Define.PlayerShopInfoTable) == true)
        //        updateWaitDatas.Remove(Define.PlayerShopInfoTable);

        //    TheBackEnd_PlayerTable.UpdatePlayerShopInfoTable(onFinish);
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerShopRandomStoreInfoTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerShopRandomStoreInfoTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerShopRandomStoreInfoTable()
        //{
        //    TheBackEnd_PlayerTable.UpdatePlayerShopRandomStoreInfoTable();
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerPassInfoTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerPassInfoTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerPassInfoTable()
        //{
        //    TheBackEnd_PlayerTable.UpdatePlayerPassInfoTable();
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerQuestInfoTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerQuestInfoTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerQuestInfoTable()
        //{
        //    TheBackEnd_PlayerTable.UpdatePlayerQuestInfoTable();
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerTimeAttackMissionInfoTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerTimeAttackMissionInfoTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerTimeAttackMissionInfoTable()
        //{
        //    TheBackEnd_PlayerTable.UpdatePlayerTimeAttackMissionInfoTable();
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerExchangeInfoTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerExchangeInfoTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerExchangeInfoTable()
        //{
        //    TheBackEnd_PlayerTable.UpdatePlayerExchangeInfoTable();
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerRankTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerRankTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerRankInfoTable()
        //{
        //    TheBackEnd_PlayerTable.UpdatePlayerRankInfoTable();
        //}
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        ////------------------------------------------------------------------------------------
        //public void GetPlayerViewDataTableData()
        //{
        //    TheBackEnd_PlayerTable.GetPlayerViewDataTableData();
        //}
        ////------------------------------------------------------------------------------------
        //public void UpdatePlayerViewDataInfoTable()
        //{
        //    TheBackEnd_PlayerTable.UpdatePlayerViewDataInfoTable();
        //}
        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        public void BackEndErrorCode(BackendReturnObject backendReturnObject)
        {
            Contents.GlobalContent.ShowGlobalNotice(backendReturnObject.GetMessage());

            if (backendReturnObject.IsMaintenanceError())
            {
                ShowDisConnectGame("system/underMaintenance");
                return;
            }
            else if (backendReturnObject.IsBadAccessTokenError())
            {
                var bro2 = Backend.BMember.RefreshTheBackendToken();
                if (bro2.GetMessage() == "bad refreshToken")
                {
                    ShowDisConnectGame("system/loggedInAnotherDevice");
                    Debug.Log("다른 기기에서 로그인되었습니다.");
                    return;
                }

                ShowDisConnectGame("system/accessTokenExpired");
                Debug.Log("토큰 만료");
                return;
            }


            switch (backendReturnObject.GetStatusCode())
            {
                case "400":
                    {
                        if (backendReturnObject.GetMessage().Contains("Please Edit the Baceknd Settings"))
                        {
                            //Client App ID 혹은 Signature Key가 null 혹은 string.Empty인 경우
                        }
                        else if (backendReturnObject.GetMessage().Contains("undefined device_unique_id, device_unique_id"))
                        {
                            //디바이스 정보가 null일 경우
                        }
                        break;
                    }
                case "401":
                    {
                        if (backendReturnObject.GetMessage().Contains("bad serverStatus: maintenance"))
                        {
                            //존재하지 않는 아이디의 경우
                        }
                        else if (backendReturnObject.GetMessage().Contains("bad customPassword"))
                        {
                            //비밀번호가 틀린 경우
                        }

                        break;
                    }
                case "403":
                    {
                        if (backendReturnObject.GetMessage().Contains("blocked device"))
                        {
                            ShowDisConnectGame("Notice_BanAccount_Desc");
                            //차단당한 디바이스일 경우
                        }
                        else if (backendReturnObject.GetMessage().Contains("blocked user"))
                        {
                            ShowDisConnectGame("Notice_BanAccount_Desc");
                            //차단당한 유저인 경우
                        }
                        else if (backendReturnObject.GetMessage().Contains("Active User"))
                        {
                            //출시 설정이 테스트인데 AU가 10을 초과한 경우
                        }
                        else if (backendReturnObject.GetMessage().Contains("bad serverStatus: maintenance"))
                        {
                            //프로젝트 상태가 '점검'일 경우(접근 허용 유저 제외)
                        }

                        break;
                    }
                case "404":
                    {
                        if (backendReturnObject.GetMessage().Contains("game not found"))
                        {
                            //Client App ID 혹은 Signature Key가 잘못된 경우
                        }
                        break;
                    }
                case "410":
                    {
                        if (backendReturnObject.GetMessage().Contains("Gone user"))
                        {
                            //탈퇴가 진행중일 경우(WithdrawAccount 함수 호출 이후)
                        }
                        break;
                    }
                case "412":
                    {
                        if (backendReturnObject.GetMessage().Contains("PreconditionFailed"))
                        {
                            //UI.UIManager.DialogExit<UI.GuildListDialog>();
                            //UI.UIManager.DialogExit<UI.GuildDialog>();
                            //UI.UIManager.DialogExit<UI.GuildMemberListDialog>();
                            //UI.UIManager.DialogExit<UI.GuildInfoDialog>();
                            //UI.UIManager.DialogExit<UI.GuildFoundateDialog>();

                            Contents.GlobalContent.ShowGlobalNotice("Empty Guild");
                        }
                        break;
                    }
            }

            Debug.LogError(string.Format("TheBackEndError   StatusCode : {0}   Message : {1}", backendReturnObject.GetStatusCode(), backendReturnObject.GetMessage()));
        }
        //------------------------------------------------------------------------------------
        public void OnCheatingDetected()
        {
            isCheatingUser = true;

            //PlayerDataContainer.PlayerCheatingCount++;
            //UpdatePlayerInfoTable();

            ShowDisConnectGame("common/ui/abuseAlarm");
        }
        //------------------------------------------------------------------------------------
        public void ShowDisConnectGame(string localstring)
        {
            if (isShowMaintenanceNotice == false)
            {
                isShowMaintenanceNotice = true;

                if (disConnectGameCoroutine != null)
                {
                    StopCoroutine(disConnectGameCoroutine);
                    disConnectGameCoroutine = null;
                }
                string check = Managers.LocalStringManager.Instance.GetLocalString(localstring);
                check = string.Format(check, 5);
                ProjectNoticeContent.Instance.ShowCheckDialog(check);

                disConnectGameCoroutine = StartCoroutine(ShowDisConnectGameCoroutine(localstring));

                Managers.AOSBackBtnManager.Instance.QuickExitGame = true;
            }
        }
        //------------------------------------------------------------------------------------
        private IEnumerator ShowDisConnectGameCoroutine(string localstring)
        {
            for (int i = 0; i < 5; ++i)
            {
                ProjectNoticeContent.Instance.ShowCheckDialog(string.Format(Managers.LocalStringManager.Instance.GetLocalString(localstring), (5 - i)));

                yield return new WaitForSeconds(1.0f);
            }

            Managers.SceneManager.Instance.OnApplicationQuit();
        }
        //------------------------------------------------------------------------------------
        public bool CheckServerVersion()
        {
            var bro = BackEnd.Backend.Utils.GetLatestVersion();
            if (bro.IsSuccess() == true)
            {
                string version = bro.GetReturnValuetoJSON()["version"].ToString();
                int forceUpdate = bro.GetReturnValuetoJSON()["type"].ToString().ToInt();
                Debug.LogError(string.Format("InitializeApp serverCheckSuccess {0} forceUpdate {1}", version, forceUpdate));

                string[] clientServerVersionCut = version.Split('.');
                string[] clientLocalVersionCut = Project.version.Split('.');

                // 버전체크 로직 수정 2025.02.19 오진혁
                for (int i = 0; i < clientLocalVersionCut.Length; ++i)
                {
                    if (i == 0 || i == 1)
                    {
                        if (int.Parse(clientServerVersionCut[i]) != int.Parse(clientLocalVersionCut[i]))
                        {
                            ProjectNoticeContent.Instance.ShowNeedUpdateDialog();
                            Managers.AOSBackBtnManager.Instance.QuickExitGame = true;
                            return false;
                        }
                    }
                }

                // 버전체크 로직 수정 2025.02.19 오진혁
                //if (version != Project.version)
                //{
                //    ProjectNoticeContent.Instance.ShowNeedUpdateDialog();
                //    Managers.AOSBackBtnManager.Instance.QuickExitGame = true;
                //    return false;
                //}
            }
            else
            {
                ProjectNoticeContent.Instance.ShowCheckDialog(Managers.LocalStringManager.Instance.GetLocalString("LogIn_Network_Desc"));
                Managers.AOSBackBtnManager.Instance.QuickExitGame = true;
                Debug.LogError(string.Format("InitializeApp serverCheckFail {0}", bro.GetMessage()));

                return false;
            }


            return true;
        }
        //------------------------------------------------------------------------------------
        private void Update()
        {
            if (SendQueue.IsInitialize == false)
                SendQueue.StartSendQueue(true, ExceptionHandler);
            else
                SendQueue.Poll();

            //Backend.AsyncPoll();

            if (mainThreadQueue != null && mainThreadQueue.Count > 0)
            {
                // Dequeue를 통해 행동을 추출 후 호출한다.(메인쓰레드이기 떄문)
                mainThreadQueue.Dequeue().Invoke();
            }

            if (sendRecvUpdateCheck == false)
                return;

            if (isCheatingUser == true)
                return;

            if (netWorkState == false)
            {
                return;
            }

            if (updateWaitDataTimer < Time.time)
            {
                //Managers.SocialManager.Instance.RefreshGuildSupport();
                //Managers.RankManager.Instance.RefreshMyRankData();

                if (Managers.TimeManager.isAlive == true)
                    Managers.TimeManager.Instance.RefreshServerTime(); // 서버시간 동기화

                //UpdatePlayerTimeInfoTable();
                //UpdatePlayerViewDataInfoTable();
                if (Managers.SceneManager.isAlive == true)
                {
                    if (Managers.SceneManager.Instance.BuildElement != BuildEnvironmentEnum.Develop)
                    {
#if !UNITY_EDITOR
                CheckServerVersion();
#endif
                    }
                }

                CheckSelfReport();

                updateWaitDataTimer = Time.time + updateWaitDataTimerTurm;
            }

            if (ReadyUpdateData == true)
                Table.UserTable.Updated();
        }
        //------------------------------------------------------------------------------------
        void ExceptionHandler(Exception e)
        {
            Debug.LogError(e.ToString());
        }
        //------------------------------------------------------------------------------------
        public void SetApplicationPause(bool isPause)
        {
            if (isPause == true)
            {
                // 게임이 Pause 되었을 때

                AllUpdateTable();

                SendQueue.PauseSendQueue();
            }
            else if (isPause == false)
            {
                // 게임이 다시 진행되었을 때
                SendQueue.ResumeSendQueue();

                if (Managers.TimeManager.isAlive == true)
                {
                    Managers.TimeManager.Instance.RefreshServerTime();
                }

                if (checkServerState == true)
                {
                    var bro = BackEnd.Backend.Utils.GetServerStatus();

                    if (bro.IsSuccess() == true)
                    {
                        string serverStatus = bro.GetReturnValuetoJSON()["serverStatus"].ToString();

                        if (serverStatus == "1")
                        {
                            if (ProjectNoticeContent.isAlive == true)
                            {
                                ProjectNoticeContent.Instance.ShowCheckDialog(Managers.LocalStringManager.Instance.GetLocalString("LogIn_Network_Desc"));

                                if (Managers.AOSBackBtnManager.isAlive == true)
                                {
                                    Managers.AOSBackBtnManager.Instance.QuickExitGame = true;
                                }
                            }
                        }
                        else if (serverStatus == "2")
                        {
                            ShowDisConnectGame("system/underMaintenance");
                            if (Managers.AOSBackBtnManager.isAlive == true)
                                Managers.AOSBackBtnManager.Instance.QuickExitGame = true;
                        }
                    }
                    else
                    {
                        if (ProjectNoticeContent.isAlive == true)
                        {
                            ProjectNoticeContent.Instance.ShowCheckDialog(Managers.LocalStringManager.Instance.GetLocalString("LogIn_Network_Desc"));
                            Managers.AOSBackBtnManager.Instance.QuickExitGame = true;
                        }
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void SetApplicationQuit()
        {
            // 큐에 처리되지 않는 요청이 남아있는 경우 대기하고 싶은 경우
            // 큐에 몇 개의 함수가 남아있는지 체크

            AllUpdateTable();

            SendQueue.StopSendQueue();
        }
        //------------------------------------------------------------------------------------
        public void AllUpdateTable()
        {
            if (ReadyUpdateData == true)
                Table.UserTable.AllUpdata();
        }
        //------------------------------------------------------------------------------------
    }
}