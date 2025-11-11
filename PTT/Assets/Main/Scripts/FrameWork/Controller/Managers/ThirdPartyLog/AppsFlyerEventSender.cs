using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppsFlyerSDK;

namespace GameBerry
{
    public class AppsFlyerEventSender : MonoBehaviour, IAppsFlyerConversionData
    {
        Dictionary<string, string> events = new Dictionary<string, string>();

        string PurchaseQUANTITY = "1";

        public void Init()
        {
            //AppsFlyer.initSDK("xhF6uWRVHfSnDUPgdVJYbS", "studio.gameberry.idledarkknight", this);
            //AppsFlyer.startSDK();
        }
        //------------------------------------------------------------------------------------
        public void SetCustomUserID(string customid)
        {
            AppsFlyer.setCustomerUserId(customid);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Game_Connect()
        {
            events.Clear();
            AppsFlyer.sendEvent("af_game_connect", events);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Af_purchase(string revenue, string currency, string id, string receipt)
        {
            events.Clear();
            events.Add("af_revenue", revenue);
            events.Add("af_currency", currency);
            events.Add("af_quantity", "1");
            events.Add("af_platform", Application.platform == RuntimePlatform.Android ? "1" : "2");
            events.Add("af_content_id", id);
            events.Add("af_order_id", receipt);
            AppsFlyer.sendEvent("af_purchase", events);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Stage_Try(int stagenum)
        {
            events.Clear();
            AppsFlyer.sendEvent(string.Format("af_stage_try_{0}", stagenum), events);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Card(int stagenum)
        {
            events.Clear();
            events.Add("af_level", stagenum.ToString());
            AppsFlyer.sendEvent("af_card", events);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Slot(int stagenum)
        {
            events.Clear();
            events.Add("af_level", stagenum.ToString());
            AppsFlyer.sendEvent("af_slot", events);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_LuckyUp(int stagenum)
        {
            events.Clear();
            events.Add("af_level", stagenum.ToString());
            AppsFlyer.sendEvent("af_luckup", events);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Defeated(int stagenum)
        {
            events.Clear();
            events.Add("af_level", stagenum.ToString());
            AppsFlyer.sendEvent("af_defeated", events);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_StageResult(int stagenum, int wave, List<int> descend)
        {
            events.Clear();
            events.Add("af_content", Util.ConvertListToString(descend));
            AppsFlyer.sendEvent(string.Format("af_result_{0}_{1}", stagenum, wave), events);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_StageClear(int stagenum, int wave)
        {
            events.Clear();
            AppsFlyer.sendEvent(string.Format("af_clear_{0}_{1}", stagenum, wave), events);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_SynergyChange(int stagenum, int wave)
        {
            events.Clear();
            events.Add("af_class", stagenum.ToString());
            events.Add("af_content", wave.ToString());
            AppsFlyer.sendEvent("af_synergychange", events);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Mission(int index)
        {
            events.Clear();
            events.Add("af_level", index.ToString());
            AppsFlyer.sendEvent(string.Format("af_mission_{0}", index), events);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Character(int level, int stagenum)
        {
            events.Clear();
            events.Add("af_level", stagenum.ToString());
            AppsFlyer.sendEvent(string.Format("af_character_{0}", level), events);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Acc_Reward(int stagenum)
        {
            events.Clear();
            events.Add("af_level", stagenum.ToString());
            AppsFlyer.sendEvent("af_acc_reward", events);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Gacha_Skill(int stagenum)
        {
            events.Clear();
            events.Add("af_level", stagenum.ToString());
            AppsFlyer.sendEvent("af_gacha_skill", events);
        }
        //------------------------------------------------------------------------------------

        public void SendLog_Gacha_Skill_Ad(int stagenum)
        {
            events.Clear();
            events.Add("af_level", stagenum.ToString());
            AppsFlyer.sendEvent("af_gacha_skill_ad", events);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_UseStamina(int stagenum)
        {
            events.Clear();
            events.Add("af_level", stagenum.ToString());
            AppsFlyer.sendEvent("af_usestamina", events);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_ChargeStamina_Ad(int stagenum)
        {
            events.Clear();
            events.Add("af_level", stagenum.ToString());
            AppsFlyer.sendEvent("af_chargestamina_ad", events);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_ChargeStamina_Dia(int stagenum)
        {
            events.Clear();
            events.Add("af_level", stagenum.ToString());
            AppsFlyer.sendEvent("af_chargestamina_dia", events);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Quest(int index)
        {
            events.Clear();
            events.Add("af_content", index.ToString());
            AppsFlyer.sendEvent("af_quest", events);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_QuestGauge(V2Enum_QuestType v2Enum_QuestType, int step)
        {
            events.Clear();
            events.Add("af_class", v2Enum_QuestType.ToString());
            events.Add("af_content", step.ToString());
            AppsFlyer.sendEvent("af_mission", events);
        }
        //------------------------------------------------------------------------------------


        //------------------------------------------------------------------------------------
        public void SendLog_Af_Stage(int stagenum)
        {
            events.Clear();
            //AppsFlyer.sendEvent(string.Format("af_stage_{0:D5}", stagenum), events);
        }
        //------------------------------------------------------------------------------------
        //public void SendPurchaseEvent(NPurchaseInfo purchaseInfo)
        //{
        //    events.Clear();
        //    events.Add(AFInAppEvents.REVENUE, purchaseInfo.price);
        //    events.Add(AFInAppEvents.CURRENCY, purchaseInfo.currency);
        //    events.Add(AFInAppEvents.QUANTITY, PurchaseQUANTITY);
        //    events.Add(AFInAppEvents.CONTENT_ID, purchaseInfo.productId);
        //    events.Add(AFInAppEvents.ORDER_ID, purchaseInfo.orderId);

        //    //AppsFlyer.sendEvent(AFInAppEvents.PURCHASE, events);
        //}
        ////------------------------------------------------------------------------------------
        //public void SendPurchaseEvent(NPurchaseItem nPurchaseItem)
        //{
        //    events.Clear();
        //    events.Add(AFInAppEvents.REVENUE, nPurchaseItem.price);
        //    events.Add(AFInAppEvents.CURRENCY, nPurchaseItem.price_currency_code);
        //    events.Add(AFInAppEvents.QUANTITY, PurchaseQUANTITY);
        //    events.Add(AFInAppEvents.CONTENT_ID, nPurchaseItem.productId);
        //    //events.Add(AFInAppEvents.ORDER_ID, nPurchaseItem.orderId);

        //    //AppsFlyer.sendEvent(AFInAppEvents.PURCHASE, events);
        //}
        ////------------------------------------------------------------------------------------
        public void SendLoginEvent(TheBackEnd.LoginType loginType)
        {
            events.Clear();

            //events.Add(AFInAppEvents.LOGIN, AFInAppEvents.SUCCESS);
            events.Add("LoginType", loginType.ToString().ToLower());

            //AppsFlyer.sendEvent(AFInAppEvents.LOGIN, events);
        }
        ////------------------------------------------------------------------------------------
        //public void SendAdbuffEvent(int buffID)
        //{
        //    events.Clear();
        //    events.Add(AFInAppEvents.AD_BUFF, AFInAppEvents.SUCCESS);
        //    events.Add(AFInAppEvents.Ad_BUFF_TYPE, buffID.ToString());

        //    //AppsFlyer.sendEvent(AFInAppEvents.AD_BUFF, events);
        //}
        ////------------------------------------------------------------------------------------
        //public void SendAdShopEvent()
        //{
        //    events.Clear();
        //    events.Add(AFInAppEvents.AD_SHOP, AFInAppEvents.SUCCESS);

        //    //AppsFlyer.sendEvent(AFInAppEvents.AD_SHOP, events);
        //}
        ////------------------------------------------------------------------------------------
        //public void SendAdGachaEvent()
        //{
        //    events.Clear();
        //    events.Add(AFInAppEvents.AD_GACHA, AFInAppEvents.SUCCESS);

        //    //AppsFlyer.sendEvent(AFInAppEvents.AD_GACHA, events);
        //}
        ////------------------------------------------------------------------------------------
        //public void SendAdAttendanceEvent()
        //{
        //    events.Clear();
        //    events.Add(AFInAppEvents.AD_ATTENDANCE, AFInAppEvents.SUCCESS);

        //    //AppsFlyer.sendEvent(AFInAppEvents.AD_ATTENDANCE, events);
        //}
        ////------------------------------------------------------------------------------------
        //public void SendAdPassEvent()
        //{
        //    events.Clear();
        //    events.Add(AFInAppEvents.AD_PASS, AFInAppEvents.SUCCESS);

        //    //AppsFlyer.sendEvent(AFInAppEvents.AD_PASS, events);
        //}
        ////------------------------------------------------------------------------------------
        //public void SendTutorial_CompletionEvent(int id)
        //{
        //    events.Clear();
        //    string eventid = string.Format(AFInAppEvents.tutorial_completion, id);
        //    events.Add(eventid, AFInAppEvents.SUCCESS);

        //    //AppsFlyer.sendEvent(eventid, events);
        //}
        ////------------------------------------------------------------------------------------
        //public void SendPhantom_AchievedEvent(int step)
        //{
        //    events.Clear();
        //    events.Add(AFInAppEvents.CONTENT_ID, step.ToString());

        //    //AppsFlyer.sendEvent(AFInAppEvents.phantom_achieved, events);
        //}
        ////------------------------------------------------------------------------------------
        //public void SendSavage_AchievedEvent(int step)
        //{
        //    events.Clear();
        //    events.Add(AFInAppEvents.CONTENT_ID, step.ToString());

        //    //AppsFlyer.sendEvent(AFInAppEvents.savage_achieved, events);
        //}
        ////------------------------------------------------------------------------------------
        //public void SendWell_AchievedEvent(int step)
        //{
        //    events.Clear();
        //    events.Add(AFInAppEvents.CONTENT_ID, step.ToString());

        //    //AppsFlyer.sendEvent(AFInAppEvents.well_achieved, events);
        //}
        ////------------------------------------------------------------------------------------
        //public void SendBounty_AchievedEvent(int step)
        //{
        //    events.Clear();
        //    events.Add(AFInAppEvents.CONTENT_ID, step.ToString());

        //    //AppsFlyer.sendEvent(AFInAppEvents.bounty_achieved, events);
        //}
        ////------------------------------------------------------------------------------------
        //public void SendPlayer_LevelEvent(int level)
        //{
        //    if ((level % 10) != 0)
        //        return;

        //    events.Clear();

        //    string eventid = string.Format(AFInAppEvents.player_level, level);
        //    events.Add(eventid, AFInAppEvents.SUCCESS);

        //    //AppsFlyer.sendEvent(eventid, events);
        //}
        ////------------------------------------------------------------------------------------
        //public void SendStage_AchievedEvent(int stage)
        //{
        //    if ((stage % 10) != 0)
        //        return;

        //    events.Clear();

        //    string eventid = string.Format(AFInAppEvents.stage_achieved, stage);
        //    events.Add(eventid, AFInAppEvents.SUCCESS);

        //    //AppsFlyer.sendEvent(eventid, events);
        //}
        ////------------------------------------------------------------------------------------
        public void onAppOpenAttribution(string attributionData)
        {
            //throw new System.NotImplementedException();
        }
        //------------------------------------------------------------------------------------
        public void onAppOpenAttributionFailure(string error)
        {
            //throw new System.NotImplementedException();
        }
        //------------------------------------------------------------------------------------
        public void onConversionDataFail(string error)
        {
            //AppsFlyer.AFLog("onConversionDataFail", conversionData)
        }
        //------------------------------------------------------------------------------------
        public void onConversionDataSuccess(string conversionData)
        {
            //AppsFlyer.AFLog("onConversionDataSuccess", conversionData)
        }
        //------------------------------------------------------------------------------------
    }
}