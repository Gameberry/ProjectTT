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
        public void SendLog_Stage_Try(int stagenum)
        {
            appsFlyerEventSender.SendLog_Stage_Try(stagenum);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Card(int stagenum)
        {
            appsFlyerEventSender.SendLog_Card(stagenum);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Slot(int stagenum)
        {
            appsFlyerEventSender.SendLog_Slot(stagenum);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_LuckyUp(int stagenum)
        {
            appsFlyerEventSender.SendLog_LuckyUp(stagenum);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Defeated(int stagenum)
        {
            appsFlyerEventSender.SendLog_Defeated(stagenum);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_StageResult(int stagenum, int wave, List<int> descend)
        {
            appsFlyerEventSender.SendLog_StageResult(stagenum, wave, descend);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_StageClear(int stagenum, int wave)
        {
            appsFlyerEventSender.SendLog_StageClear(stagenum, wave);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_SynergyChange(int stagenum, int wave)
        {
            appsFlyerEventSender.SendLog_SynergyChange(stagenum, wave);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Mission(int index)
        {
            appsFlyerEventSender.SendLog_Mission(index);
            fireBaseEventSender.SendLog_log_mission(index);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Character(int level, int stagenum)
        {
            appsFlyerEventSender.SendLog_Character(level, stagenum);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Acc_Reward(int stagenum)
        {
            appsFlyerEventSender.SendLog_Acc_Reward(stagenum);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Gacha_Skill(int stagenum)
        {
            appsFlyerEventSender.SendLog_Gacha_Skill(stagenum);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Gacha_Skill_Ad(int stagenum)
        {
            appsFlyerEventSender.SendLog_Gacha_Skill_Ad(stagenum);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_UseStamina(int stagenum)
        {
            appsFlyerEventSender.SendLog_UseStamina(stagenum);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_ChargeStamina_Ad(int stagenum)
        {
            appsFlyerEventSender.SendLog_ChargeStamina_Ad(stagenum);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_ChargeStamina_Dia(int stagenum)
        {
            appsFlyerEventSender.SendLog_ChargeStamina_Dia(stagenum);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_stamina_dia(int used_type, double former_quan, double used_quan, double keep_quan)
        {
            fireBaseEventSender.SendLog_log_stamina_dia(used_type, former_quan, used_quan, keep_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_research_levelup(int idx, int level, int used_type, double former_quan, double used_quan, double keep_quan)
        {
            fireBaseEventSender.SendLog_log_research_levelup(idx, level, used_type, former_quan, used_quan, keep_quan);
        }
        //------------------------------------------------------------------------------------

        public void SendLog_log_research_speedup(int idx, int level, int used_type, double former_quan, double used_quan, double keep_quan)
        {
            fireBaseEventSender.SendLog_log_research_speedup(idx, level, used_type, former_quan, used_quan, keep_quan);
        }
        //------------------------------------------------------------------------------------

        public void SendLog_log_fossil_acquire(int type,
            int reward_type, double before_quan, double reward_quan, double after_quan,
            int used_type, double former_quan, double used_quan, double keep_quan)
        {
            fireBaseEventSender.SendLog_log_fossil_acquire(type,
                reward_type, before_quan, reward_quan, after_quan,
                used_type, former_quan, used_quan, keep_quan);
        }
        //------------------------------------------------------------------------------------

        public void SendLog_log_rune_synthesis(int idx, int level,
            List<int> used_type, int instance)
        {
            fireBaseEventSender.SendLog_log_rune_synthesis(idx, level,
                used_type, instance);
        }
        //------------------------------------------------------------------------------------

        public void SendLog_log_equip_gear(int idx)
        {
            fireBaseEventSender.SendLog_log_equip_gear(idx);
        }
        //------------------------------------------------------------------------------------

        public void SendLog_log_equip_synthesis(int idx, int level,
            List<int> used_type, int instance)
        {
            fireBaseEventSender.SendLog_log_equip_synthesis(idx, level,
                used_type, instance);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_skill_reset(int idx, string level,
            List<int> reward_type, List<double> reward_quan)
        {
            fireBaseEventSender.SendLog_log_skill_reset(idx, level,
                reward_type, reward_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_gearslot_reset(int idx, int level,
            List<int> reward_type, List<double> reward_quan)
        {
            fireBaseEventSender.SendLog_log_gearslot_reset(idx, level,
                reward_type, reward_quan);
        }
        //------------------------------------------------------------------------------------

        //------------------------------------------------------------------------------------
        public void SendLog_log_gearslot_enforce(int type, int level,
            List<int> used_type, List<double> before_quan, List<double> used_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_log_gearslot_enforce(
                type, level,
                used_type, before_quan, used_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Quest(int index)
        {
            appsFlyerEventSender.SendLog_Quest(index);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_QuestGauge(V2Enum_QuestType v2Enum_QuestType, int step)
        {
            appsFlyerEventSender.SendLog_QuestGauge(v2Enum_QuestType, step);
        }
        //------------------------------------------------------------------------------------









        //------------------------------------------------------------------------------------
        public void SendLog_log_levelup(int idx,
            List<int> used_type, List<double> before_quan, List<double> used_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_log_levelup(
                idx,
                used_type, before_quan, used_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_limitup(int idx,
            List<int> used_type, List<double> before_quan, List<double> used_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_log_limitup(
                idx,
                used_type, before_quan, used_quan, after_quan);
        }
        //------------------------------------------------------------------------------------

        public void SendLog_log_tutorial(int type)
        {
            fireBaseEventSender.SendLog_log_tutorial(type);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_progress_reward(List<int> idx,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_log_progress_reward(idx,
            reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        
        public void SendLog_log_dungeon_start(int type, double idx, 
            int used_type, double before_quan, double used_quan, double after_quan)
        {
            fireBaseEventSender.SendLog_log_dungeon_start(type, idx,
            used_type, before_quan, used_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_dungeon_end(int type, int result, double idx,
            int used_type, double used_quan,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan,
            int skip_flag)
        {
            fireBaseEventSender.SendLog_log_dungeon_end(type, result, idx,
                used_type, used_quan,
            reward_type, before_quan, reward_quan, after_quan,
            skip_flag);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_fast_clear(int type, double idx,
            int used_type, double former_quan, double used_quan, double keep_quan,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan,
            int skip_flag)
        {
            fireBaseEventSender.SendLog_log_fast_clear(type, idx,
                used_type, former_quan, used_quan, keep_quan,
            reward_type, before_quan, reward_quan, after_quan,
            skip_flag);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_dungeon_end(int result, int idx)
        {
            fireBaseEventSender.SendLog_log_dungeon_end(result, idx);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_job_change(int type, int idx)
        {
            fireBaseEventSender.SendLog_log_job_change(type, idx);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_job_enforce(int type, int idx)
        {
            fireBaseEventSender.SendLog_log_job_enforce(type, idx);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_job_levelup(int idx, int level,
            List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan)
        {
            fireBaseEventSender.SendLog_log_job_levelup(
                idx, level,
                used_type, former_quan, used_quan, keep_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_dungeon_pearl(int idx)
        {
            fireBaseEventSender.SendLog_log_dungeon_pearl(
                idx);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_dungeon_joker(int type)
        {
            fireBaseEventSender.SendLog_log_dungeon_joker(type);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_descend_enforce(int idx, int level,
            List<int> used_type, List<double> before_quan, List<double> used_quan, List<double> after_quan,
            int exp)
        {
            fireBaseEventSender.SendLog_log_descend_enforce(
                idx, level,
                used_type, before_quan, used_quan, after_quan,
                exp);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_descend_acquire(int idx,
            int used_type, double before_quan, double used_quan, double after_quan)
        {
            fireBaseEventSender.SendLog_log_descend_acquire(
                idx,
                used_type, before_quan, used_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_descend_limitup(int idx, int level,
    List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan)
        {
            fireBaseEventSender.SendLog_log_descend_limitup(
                idx, level,
                used_type, former_quan, used_quan, keep_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_skill_enforce(int idx, int level,
            List<int> used_type, List<double> before_quan, List<double> used_quan, List<double> after_quan,
            int exp)
        {
            fireBaseEventSender.SendLog_log_skill_enforce(
                idx, level,
                used_type, before_quan, used_quan, after_quan,
                exp);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_skill_acquire(int idx,
            int used_type, double before_quan, double used_quan, double after_quan)
        {
            fireBaseEventSender.SendLog_log_skill_acquire(
                idx,
                used_type, before_quan, used_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_skill_limitup(int idx, int level,
    List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan)
        {
            fireBaseEventSender.SendLog_log_skill_limitup(
                idx, level,
                used_type, former_quan, used_quan, keep_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_artifact_enforce(int idx, int level)
        {
            fireBaseEventSender.SendLog_log_artifact_enforce(
                idx, level);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_timeattack_mission(int idx, int type,
    List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_log_timeattack_mission(
                idx, type,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------


        public void SendLog_log_playmode(V2Enum_ARR_BattleSpeed type)
        {
            fireBaseEventSender.SendLog_log_playmode(type);
        }
        //------------------------------------------------------------------------------------




        public void SendLog_IAP(string revenue, string currency, string id, string receipt)
        {
            appsFlyerEventSender.SendLog_Af_purchase(revenue, currency, id, receipt);
        }
        //------------------------------------------------------------------------------------
        //public void SendPurchaseEvent(NPurchaseInfo purchaseInfo)
        //{
        //    appsFlyerEventSender.SendPurchaseEvent(purchaseInfo);
        //}
        ////------------------------------------------------------------------------------------
        //public void SendPurchaseEvent(NPurchaseItem nPurchaseItem)
        //{
        //    appsFlyerEventSender.SendPurchaseEvent(nPurchaseItem);
        //}
        //------------------------------------------------------------------------------------
        public void SendLoginEvent(TheBackEnd.LoginType loginType)
        {
            appsFlyerEventSender.SendLoginEvent(loginType);
        }
        //------------------------------------------------------------------------------------
        public void SendGuide_CompletionEvent(int id, int reward_type, int before_quan, int reward_quan, int after_quan)
        {
            fireBaseEventSender.SendLog_GuideEvent(id, reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Map_BossEvent(int idx, int result, 
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_Map_BossEvent(
                idx, result,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Map_DefeatEvent()
        {
            fireBaseEventSender.SendLog_Map_DefeatEvent();
        }
        //------------------------------------------------------------------------------------
        public void SendLogSetClearMaxStageEvent(int stagenum)
        {
            appsFlyerEventSender.SendLog_Af_Stage(stagenum);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Dg_ResultEvent(int type,
            int stage, int result, int count, double rec_now, double rec_season,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_Dg_ResultEvent(
                type, 
                stage, result, count, rec_now, rec_season,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Char_UpEvent(double former_quan, double used_quan, double keep_quan)
        {
            fireBaseEventSender.SendLog_Char_UpEvent(former_quan, used_quan, keep_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Char_AscenEvent(double former_quan, double used_quan, double keep_quan)
        {
            fireBaseEventSender.SendLog_Char_AscenEvent(former_quan, used_quan, keep_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Rune_GetEvent(int reward_type, double former_quan, double used_quan, double keep_quan)
        {
            fireBaseEventSender.SendLog_Rune_GetEvent(reward_type, former_quan, used_quan, keep_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Rune_UpEvent(int idx, int after_lv, double former_quan, double used_quan, double keep_quan)
        {
            fireBaseEventSender.SendLog_Rune_UpEvent(idx, after_lv, former_quan, used_quan, keep_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Rune_AscenEvent(int idx, int grade, double former_quan, double used_quan, double keep_quan)
        {
            fireBaseEventSender.SendLog_Rune_AscenEvent(idx, grade, former_quan, used_quan, keep_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_ResearchEvent(int type, int idx, int after_lv, double former_quan, double used_quan, double keep_quan)
        {
            fireBaseEventSender.SendLog_MasteryEvent(type, idx, after_lv, former_quan, used_quan, keep_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Ally_UpEvent(int idx, int type, int after_lv,
            List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan)
        {
            fireBaseEventSender.SendLog_Ally_UpEvent(idx, type, after_lv, used_type, former_quan, used_quan, keep_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Ally_AscenEvent(int idx, int type, List<int> used_type, int grade)
        {
            fireBaseEventSender.SendLog_Ally_AscenEvent(idx, type, used_type, grade);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Gacha_ResultEvent(int type,
            int instance, List<int> reward_type, List<double> reward_quan)
        {
            fireBaseEventSender.SendLog_Gacha_ResultEvent(type, instance, reward_type, reward_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Gacha_RewardEvent(int type, int idx)
        {
            fireBaseEventSender.SendLog_Gacha_RewardEvent(type, idx);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_PassEvent(int idx, int type,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_PassEvent(idx, type, reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_MailEvent(int type, string idx, 
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_MailEvent(type, idx, reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Dg_AdEvent(int idx, int skip_flag)
        {
            fireBaseEventSender.SendLog_Dg_AdEvent(idx, skip_flag);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_QuestEvent(V2Enum_QuestType type, List<int> idx, 
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_QuestEvent(type, idx, 
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Quest_upperbarEvent(V2Enum_QuestType type, List<int> idx,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_Quest_upperbarEvent(type, idx,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_roulette_getEvent(List<int> idx)
        {
            fireBaseEventSender.SendLog_roulette_getEvent(idx);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_AttendEvent(V2Enum_CheckInType type, int count,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan,
            int skip_flag)
        {
            fireBaseEventSender.SendLog_AttendEvent(type, count,
                reward_type, before_quan, reward_quan, after_quan,
                skip_flag);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_GachaEvent(int type, 
            int used_type, double former_quan, double used_quan, double keep_quan
            , int skip_flag)
        {
            fireBaseEventSender.SendLog_GachaEvent(type, 
                used_type, former_quan, used_quan, keep_quan,
                skip_flag);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_artifact_acquire(int idx,
            int used_type, double former_quan, double used_quan, double keep_quan)
        {
            fireBaseEventSender.SendLog_log_artifact_acquire(idx,
                used_type, former_quan, used_quan, keep_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_OfflineEvent(int count,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_OfflineEvent(count,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Offline_AdEvent(int count,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan,
            int skip_flag)
        {
            fireBaseEventSender.SendLog_Offline_AdEvent(count,
                reward_type, before_quan, reward_quan, after_quan,
                skip_flag);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_NickEvent(string nick_before, string nick_after,
            int reward_type, double before_quan, double reward_quan, double after_quan)
        {
            fireBaseEventSender.SendLog_NickEvent(nick_before, nick_after, reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Shop_IapEvent(string idx, int shop_id, int shop_price)
        {
            fireBaseEventSender.SendLog_Shop_IapEvent(idx, shop_id, shop_price);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_shop_noniap(int shop_id,
            int used_type, double former_quan, double used_quan, double keep_quan,
            int reward_type, double before_quan, double reward_quan, double after_quan)
        {
            fireBaseEventSender.SendLog_log_shop_noniap(shop_id,
                used_type, former_quan, used_quan, keep_quan,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Shop_PopEvent(int shop_id)
        {
            fireBaseEventSender.SendLog_Shop_PopEvent(shop_id);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_MarketEvent(int count,
            List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_MarketEvent(count,
                used_type, former_quan, used_quan, keep_quan,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Clan_UpEvent(int idx,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_Clan_UpEvent(idx,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_BerserkerEvent(int idx, int before_lv, int after_lv,
    List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan)
        {
            fireBaseEventSender.SendLog_BerserkerEvent(idx, before_lv, after_lv,
                used_type, former_quan, used_quan, keep_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_AllyArena_HonorShopEvent(int idx,
            List<int> used_type, List<double> used_quan, List<int> reward_type, List<double> reward_quan)
        {
            fireBaseEventSender.SendLog_AllyArena_HonorShopEvent(idx,
                used_type, used_quan, reward_type, reward_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_AllyArena_ResultEvent(IFFType result, int count, long rec_season)
        {
            fireBaseEventSender.SendLog_AllyArena_ResultEvent(result, count, rec_season);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_AD_ViewEvent(string type, int idx, int skip_flag)
        {
            fireBaseEventSender.SendLog_AD_ViewEvent(type, idx, skip_flag);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_DevilCastleEvent(int idx, int result,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_DevilCastleEvent(
                idx, result,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_God_GachaEvent(List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan,
            List<int> idx, int rec_season)
        {
            fireBaseEventSender.SendLog_God_GachaEvent(
                used_type, former_quan, used_quan, keep_quan,
                idx, rec_season);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_God_GetEvent(List<int> idx,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_God_GetEvent(idx,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_God_Dg_GetEvent(int idx)
        {
            fireBaseEventSender.SendLog_God_Dg_GetEvent(idx);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_TrialTowerEvent(int idx, int result,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_TrialTowerEvent(
                idx, result,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Ally_ComposeEvent(List<int> reward_type, List<int> used_type, string type)
        {
            fireBaseEventSender.SendLog_Ally_ComposeEvent(reward_type, used_type, type);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Jewelry_ComposeEvent(List<int> reward_type, List<int> used_type, string type)
        {
            fireBaseEventSender.SendLog_Jewelry_ComposeEvent(reward_type, used_type, type);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Jewelry_AscenEvent(int idx, int type, List<int> used_type, int grade)
        {
            fireBaseEventSender.SendLog_Jewelry_AscenEvent(idx, type, used_type, grade);
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
        public void SendLog_Ally_CollectionEvent(List<int> idx,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_Ally_CollectionEvent(
                idx, reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_LuckyRouletteEvent(int rec_now, List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_LuckyRouletteEvent(rec_now, 
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_BoxEvent(int idx, int count, List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_BoxEvent(idx, count,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_SummonTicketEvent(int idx, int count, List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_SummonTicketEvent(idx, count,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_RedBull_GachaEvent(List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan,
    List<int> idx, int rec_season)
        {
            fireBaseEventSender.SendLog_RedBull_GachaEvent(
                used_type, former_quan, used_quan, keep_quan,
                idx, rec_season);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_RedBull_GetEvent(List<int> idx,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_RedBull_GetEvent(idx,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_RedBull_Dg_GetEvent(int idx)
        {
            fireBaseEventSender.SendLog_RedBull_Dg_GetEvent(idx);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_RedBull_AddEnterEvent(double used_quan)
        {
            fireBaseEventSender.SendLog_RedBull_AddEnterEvent(used_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Skin_GetEvent(int idx)
        {
            fireBaseEventSender.SendLog_Skin_GetEvent(idx);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Skin_UpEvent(int idx, int after_lv, int used_type, double former_quan, double used_quan, double keep_quan)
        {
            fireBaseEventSender.SendLog_Skin_UpEvent(idx, after_lv, used_type, former_quan, used_quan, keep_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Skin_StarUpEvent(int idx, int grade, int used_quan)
        {
            fireBaseEventSender.SendLog_Skin_StarUpEvent(idx, grade, used_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Event_PassEvent(int type, List<int> idx,
    List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_Event_PassEvent(type, idx,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Rotation_Once_GetEvent(List<int> idx,
    List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_Rotation_Once_GetEvent(idx,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Rotation_Repeat_GetEvent(List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_Rotation_Repeat_GetEvent(reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Ursula_GachaEvent(List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan,
List<int> idx, int rec_season)
        {
            fireBaseEventSender.SendLog_Ursula_GachaEvent(
                used_type, former_quan, used_quan, keep_quan,
                idx, rec_season);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Ursula_GetEvent(List<int> idx,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_Ursula_GetEvent(idx,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Ursula_Dg_GetEvent(int idx)
        {
            fireBaseEventSender.SendLog_Ursula_Dg_GetEvent(idx);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Ursula_AddEnterEvent(double used_quan)
        {
            fireBaseEventSender.SendLog_Ursula_AddEnterEvent(used_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Dig_FailEvent(List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan,
            List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan,
            int idx, int rec_season)
        {
            fireBaseEventSender.SendLog_Dig_FailEvent(reward_type, before_quan, reward_quan, after_quan,
                used_type, former_quan, used_quan, keep_quan,
                idx, rec_season);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Dig_FindEvent(List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan,
            int count, int rec_season)
        {
            fireBaseEventSender.SendLog_Dig_findEvent(used_type, former_quan, used_quan, keep_quan,
                count, rec_season);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Dig_GetEvent(List<int> idx,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_Dig_GetEvent(idx,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Dig_AddShovelEvent(double used_quan)
        {
            fireBaseEventSender.SendLog_Dig_AddShovelEvent(used_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Dig_FreePassEvent(int idx, int rec_now,
List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_Dig_FreePassEvent(idx, rec_now,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Dig_PaidPassEvent(int idx, int rec_now)
        {
            fireBaseEventSender.SendLog_Dig_PaidPassEvent(idx, rec_now);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_ForgeEvent(int idx, int result, int before_lv, int after_lv, int skip_flag,
    List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan)
        {
            fireBaseEventSender.SendLog_ForgeEvent(idx, result, before_lv, after_lv, skip_flag,
                used_type, former_quan, used_quan, keep_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Forge_CollectionEvent(int idx, int result, int before_lv, int after_lv)
        {
            fireBaseEventSender.SendLog_Forge_CollectionEvent(idx, result, before_lv, after_lv);
        }
        //------------------------------------------------------------------------------------







        public void SendLog_MathRpg_StepUpEvent(int stage, int rec_now, int idx,
List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_MathRpg_StepUpEvent(stage, rec_now, idx,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_MathRpg_RouletteEvent(int idx, int gacha_lv,
            List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan,
List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_MathRpg_RouletteEvent(idx, gacha_lv,
                used_type, former_quan, used_quan, keep_quan,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_MathRpg_RouletteAccumEvent(int idx,
List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_MathRpg_RouletteAccumEvent(idx,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_MathRpg_FreePassEvent(int idx, int rec_now,
List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_MathRpg_FreePassEvent(idx, rec_now,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_MathRpg_PaidPassEvent(int idx, int rec_now)
        {
            fireBaseEventSender.SendLog_MathRpg_PaidPassEvent(idx, rec_now);
        }
        //------------------------------------------------------------------------------------


        public void SendLog_KingSlime_AddEnterEvent(double used_quan)
        {
            fireBaseEventSender.SendLog_KingSlime_AddEnterEvent(used_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_KingSlime_Dg_GetEvent(int idx)
        {
            fireBaseEventSender.SendLog_KingSlime_Dg_GetEvent(idx);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_KingSlime_GetEvent(List<int> idx,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_KingSlime_GetEvent(idx,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_KingSlime_GachaEvent(List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan,
            List<int> idx, int rec_season)
        {
            fireBaseEventSender.SendLog_KingSlime_GachaEvent(
                used_type, former_quan, used_quan, keep_quan,
                idx, rec_season);
        }
        //------------------------------------------------------------------------------------

        public void SendLog_Guild_CreateEvent(string type,
    List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan)
        {
            fireBaseEventSender.SendLog_Guild_CreateEvent(type, 
                used_type, former_quan, used_quan, keep_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Guild_CoinShopEvent(int idx,
            List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_Guild_CoinShopEvent(idx,
                used_type, former_quan, used_quan, keep_quan,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Guild_CheckInRewardEvent(List<int> idx,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_Guild_CheckInRewardEvent(idx,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Guild_CheckInEvent(int idx, int rec_season,
            List<int> used_type, List<double> used_quan)
        {
            fireBaseEventSender.SendLog_Guild_CheckInEvent(idx, rec_season,
                used_type, used_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Guild_PurDonaEvent(int count)
        {
            fireBaseEventSender.SendLog_Guild_PurDonaEvent(count);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Guild_PurrewardEvent(List<int> idx,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_Guild_PurrewardEvent(idx, 
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Guild_RaidShopEvent(int idx,
            List<int> used_type, List<double> used_quan, List<int> reward_type, List<double> reward_quan)
        {
            fireBaseEventSender.SendLog_Guild_RaidShopEvent(idx,
                used_type, used_quan, reward_type, reward_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Guild_RankDunEvent(V2Enum_Dungeon type, int rec_now)
        {
            fireBaseEventSender.SendLog_Guild_RankDunEvent(type, rec_now);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Guild_NorDunEvent(V2Enum_Dungeon type, int rec_now,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_Guild_NorDunEvent(type, rec_now,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Guild_NorDunSweepEvent(V2Enum_Dungeon type, int rec_now, int count,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_Guild_NorDunSweepEvent(type, rec_now, count,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Guild_RankAddEnterEvent(double used_quan)
        {
            fireBaseEventSender.SendLog_Guild_RankAddEnterEvent(used_quan);
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Guild_NorDunGetEvent(List<int> idx,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_Guild_NorDunGetEvent(idx,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------

        public void SendLog_SevenDay_OnceEvent(List<int> idx,
    List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            fireBaseEventSender.SendLog_SevenDay_OnceEvent(idx,
                reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
    }
}