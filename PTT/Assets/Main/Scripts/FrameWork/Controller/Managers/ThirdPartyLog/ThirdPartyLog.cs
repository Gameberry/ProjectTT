using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppsFlyerSDK;

namespace GameBerry
{
    public class ThirdPartyLog : MonoSingleton<ThirdPartyLog>
    {
        private AppsFlyerEventSender appsFlyerEventSender;
        private FireBaseEventSender fireBaseEventSender;
        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            appsFlyerEventSender = gameObject.AddComponent<AppsFlyerEventSender>();
            appsFlyerEventSender.Init();

            fireBaseEventSender = gameObject.AddComponent<FireBaseEventSender>();
            fireBaseEventSender.Init();
        }
        //------------------------------------------------------------------------------------
        public void InitThirdParty()
        {

        }
        //------------------------------------------------------------------------------------
        public string GetFirebaseMessagingToken()
        {
            return fireBaseEventSender.GetFirebaseMessagingToken();
        }
        //------------------------------------------------------------------------------------
        public void SetCustomUserID(string customid)
        {
            appsFlyerEventSender.SetCustomUserID(customid);
            fireBaseEventSender.SetCustomUserID(customid);
            //UnityPlugins.appLovin.SetUserId(customid);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Game_Connect()
        {
            appsFlyerEventSender.SendLog_Game_Connect();
            fireBaseEventSender.SendLog_Game_Connect();
        }
        //------------------------------------------------------------------------------------
        public void SendLog_StageResult(int stagenum, int wave, List<int> descend)
        {
            appsFlyerEventSender.SendLog_StageResult(stagenum, wave, descend);
        }
        //------------------------------------------------------------------------------------
        public void SendLoginEvent(TheBackEnd.LoginType loginType)
        {
            appsFlyerEventSender.SendLoginEvent(loginType);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_TableLoadEvent(float rec_now, int count)
        {
            fireBaseEventSender.SendLog_TableLoadEvent(rec_now, count);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_DBLoadEvent(float rec_now)
        {
            fireBaseEventSender.SendLog_DBLoadEvent(rec_now);
        }
        //------------------------------------------------------------------------------------
    }
}