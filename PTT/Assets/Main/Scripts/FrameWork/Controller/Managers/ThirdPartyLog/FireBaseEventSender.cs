using System;
using System.Collections.Generic;
using System.Linq;
using Firebase;
using Firebase.Analytics;
using System.Text;
using UnityEngine;
using Firebase.Messaging;

namespace GameBerry
{
    public class FireBaseEventSender : MonoBehaviour
    {
        FirebaseApp _app;
        private bool UserId = false;
        private string token = string.Empty;


        public void Init()
        {
#if !UNITY_EDITOR
                        if (string.IsNullOrEmpty(TheBackEnd.TheBackEnd_Login.UserUID) == false)
                        {
                            FirebaseAnalytics.SetUserId(TheBackEnd.TheBackEnd_Login.UserUID);
                            UserId = true;
                        }

                        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
                        {
                            if (task.Result == DependencyStatus.Available)
                            {
                                _app = FirebaseApp.DefaultInstance;
                                if (string.IsNullOrEmpty(TheBackEnd.TheBackEnd_Login.UserUID) == false)
                                {
                                    FirebaseAnalytics.SetUserId(TheBackEnd.TheBackEnd_Login.UserUID);
                                    UserId = true;
                                }

                                GetToken();

                            }
                            else
                            {
                                Debug.LogError("Could not resolve all Firebase dependencies" + task.Result);
                            }
                        });
#endif
        }
        //------------------------------------------------------------------------------------
        public string GetFirebaseMessagingToken()
        {
            return token;
        }
        //------------------------------------------------------------------------------------
        void GetToken()
        {
            FirebaseMessaging.TokenReceived += OnTokenReceived;

            FirebaseMessaging.GetTokenAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    token = task.Result;
                    Debug.Log($"FCM Token: {token}");
                }
                else
                {
                    Debug.Log("Failed to get FCM token");
                }
            });
        }
        //------------------------------------------------------------------------------------
        void OnTokenReceived(object sender, TokenReceivedEventArgs token)
        {
            Debug.Log($"Received Registration Token: {token.Token}");
        }
        //------------------------------------------------------------------------------------
        public void BigQuery_GrowthLog_JustParam(string eventName, params Parameter[] parameters)
        {
#if !UNITY_EDITOR
            FirebaseAnalytics.LogEvent(eventName, parameters);
#endif
        }
        //------------------------------------------------------------------------------------
        public void BigQuery_GrowthLog(string eventName, params Parameter[] parameters)
        {
#if !UNITY_EDITOR
            List<Parameter> extra = new List<Parameter>();
                    extra.Add(new Parameter("nickname", TheBackEnd.TheBackEnd_Login.UserNickName));
                    extra.Add(new Parameter("in_date", TheBackEnd.TheBackEnd_Login.UserInDate));
                    extra.Add(new Parameter("user_id", TheBackEnd.TheBackEnd_Login.UserUID));
                    //extra.Add(new Parameter("user_lv", ARRRStatContainer.ARRRLevel.ToString()));
                    //extra.Add(new Parameter("max_stage", MapContainer.MaxWaveClear.ToString()));
                    //extra.Add(new Parameter("stage", MapContainer.GetLogStage().ToString()));
                    //extra.Add(new Parameter("wave", MapContainer.GetLogWave().ToString()));

            parameters = parameters.Concat(extra).ToArray();
            FirebaseAnalytics.LogEvent(eventName, parameters);
#endif


        }
        //------------------------------------------------------------------------------------

        public void BigQuery_GrowthLog(string eventName)
        {
#if !UNITY_EDITOR
                    List<Parameter> extra = new List<Parameter>();
                    extra.Add(new Parameter("nickname", TheBackEnd.TheBackEnd_Login.UserNickName));
                    extra.Add(new Parameter("in_date", TheBackEnd.TheBackEnd_Login.UserInDate));
                    extra.Add(new Parameter("user_id", TheBackEnd.TheBackEnd_Login.UserUID));
                    //extra.Add(new Parameter("user_lv", ARRRStatContainer.ARRRLevel.ToString()));
                    //extra.Add(new Parameter("max_stage", MapContainer.MaxWaveClear.ToString()));
                    //extra.Add(new Parameter("stage", MapContainer.GetLogStage().ToString()));
                    //extra.Add(new Parameter("wave", MapContainer.GetLogWave().ToString()));

            FirebaseAnalytics.LogEvent(eventName, extra.ToArray());  
#endif
        }
        //------------------------------------------------------------------------------------
        public void SetCustomUserID(string customid)
        {
            FirebaseAnalytics.SetUserId(customid);
            UserId = true;
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Game_Connect()
        {
            BigQuery_GrowthLog("log_game_connect");
        }
        //------------------------------------------------------------------------------------
        public void SendLog_TableLoadEvent(float rec_now, int count)
        {
            BigQuery_GrowthLog_JustParam("log_tableload",
                new Parameter("rec_now", rec_now.ToString()),
                new Parameter("count", count.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_DBLoadEvent(float rec_now)
        {
            BigQuery_GrowthLog_JustParam("log_dbload",
                new Parameter("rec_now", rec_now.ToString()));
        }
        //------------------------------------------------------------------------------------
    }
}