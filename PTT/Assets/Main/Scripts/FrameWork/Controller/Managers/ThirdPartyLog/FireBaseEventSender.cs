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
                    extra.Add(new Parameter("max_stage", MapContainer.MaxWaveClear.ToString()));
                    extra.Add(new Parameter("stage", MapContainer.GetLogStage().ToString()));
                    extra.Add(new Parameter("wave", MapContainer.GetLogWave().ToString()));

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
                    extra.Add(new Parameter("max_stage", MapContainer.MaxWaveClear.ToString()));
                    extra.Add(new Parameter("stage", MapContainer.GetLogStage().ToString()));
                    extra.Add(new Parameter("wave", MapContainer.GetLogWave().ToString()));

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
        public void SendLog_log_levelup(int idx,
            List<int> used_type, List<double> before_quan, List<double> used_quan, List<double> after_quan)
        {
            BigQuery_GrowthLog("log_levelup",
            new Parameter("idx", idx.ToString()),
            new Parameter("used_type", Util.ConvertListToString(used_type)),
            new Parameter("before_quan", Util.ConvertListToString(before_quan)),
            new Parameter("used_quan", Util.ConvertListToString(used_quan)),
            new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_limitup(int idx,
            List<int> used_type, List<double> before_quan, List<double> used_quan, List<double> after_quan)
        {
            BigQuery_GrowthLog("log_limitup",
            new Parameter("idx", idx.ToString()),
            new Parameter("used_type", Util.ConvertListToString(used_type)),
            new Parameter("before_quan", Util.ConvertListToString(before_quan)),
            new Parameter("reward_quan", Util.ConvertListToString(used_quan)),
            new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_tutorial(int type)
        {
            BigQuery_GrowthLog("log_tutorial",
            new Parameter("type", type.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_progress_reward(List<int> idx,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            BigQuery_GrowthLog("log_progress_reward",
            new Parameter("idx", Util.ConvertListToString(idx)),
            new Parameter("reward_type", Util.ConvertListToString(reward_type)),
            new Parameter("before_quan", Util.ConvertListToString(before_quan)),
            new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
            new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------

        public void SendLog_log_dungeon_start(int type, double idx,
            int used_type, double before_quan, double used_quan, double after_quan)
        {
            BigQuery_GrowthLog("log_dungeon_start",
                new Parameter("type", type.ToString()),
                new Parameter("idx", idx.ToString()),
            new Parameter("skills", SynergyRuneContainer.GetLogEquipInfo()),
            new Parameter("devils", DescendContainer.GetLogEquipInfo()),
            new Parameter("used_type", used_type.ToString()),
            new Parameter("before_quan", before_quan.ToString()),
            new Parameter("used_quan", used_quan.ToString()),
            new Parameter("after_quan", after_quan.ToString()));
        }

        //------------------------------------------------------------------------------------
        public void SendLog_log_dungeon_end(int type, int result, double idx,
            int used_type, double used_quan,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan,
            int skip_flag)
        {
            BigQuery_GrowthLog("log_dungeon_end",
                new Parameter("type", type.ToString()),
            new Parameter("result", result.ToString()),
            new Parameter("idx", idx.ToString()),
            new Parameter("used_type", used_type.ToString()),
            new Parameter("used_quan", used_quan.ToString()),
            new Parameter("reward_type", Util.ConvertListToString(reward_type)),
            new Parameter("before_quan", Util.ConvertListToString(before_quan)),
            new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
            new Parameter("after_quan", Util.ConvertListToString(after_quan)),
            new Parameter("skip_flag", skip_flag.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_fast_clear(int type, double idx,
            int used_type, double former_quan, double used_quan, double keep_quan,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan,
            int skip_flag)
        {
            if (skip_flag == 0)
            {
                BigQuery_GrowthLog("log_fast_clear",
            new Parameter("type", type.ToString()),
        new Parameter("idx", idx.ToString()),
        new Parameter("used_type", used_type.ToString()),
        new Parameter("former_quan", former_quan.ToString()),
        new Parameter("used_quan", used_quan.ToString()),
        new Parameter("keep_quan", keep_quan.ToString()),
        new Parameter("reward_type", Util.ConvertListToString(reward_type)),
        new Parameter("before_quan", Util.ConvertListToString(before_quan)),
        new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
        new Parameter("after_quan", Util.ConvertListToString(after_quan)));
            }
            else
            {
                BigQuery_GrowthLog("log_fast_clear",
            new Parameter("type", type.ToString()),
        new Parameter("idx", idx.ToString()),
        new Parameter("used_type", used_type.ToString()),
        new Parameter("former_quan", former_quan.ToString()),
        new Parameter("used_quan", used_quan.ToString()),
        new Parameter("keep_quan", keep_quan.ToString()),
        new Parameter("reward_type", Util.ConvertListToString(reward_type)),
        new Parameter("before_quan", Util.ConvertListToString(before_quan)),
        new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
        new Parameter("after_quan", Util.ConvertListToString(after_quan)),
        new Parameter("skip_flag", skip_flag.ToString()));
            }
            
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_dungeon_end(int result, int idx)
        {
            BigQuery_GrowthLog("log_dungeon_end",
            new Parameter("result", result.ToString()),
            new Parameter("idx", idx.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_job_change(int type, int idx)
        {
            BigQuery_GrowthLog("log_job_change",
            new Parameter("type", type.ToString()),
            new Parameter("idx", idx.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_job_enforce(int type, int idx)
        {
            BigQuery_GrowthLog("log_job_enforce",
            new Parameter("type", type.ToString()),
            new Parameter("idx", idx.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_job_levelup(int idx, int level,
            List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan)
        {
            BigQuery_GrowthLog("log_job_levelup",
            new Parameter("idx", idx.ToString()),
            new Parameter("level", level.ToString()),
            new Parameter("used_type", Util.ConvertListToString(used_type)),
            new Parameter("former_quan", Util.ConvertListToString(former_quan)),
            new Parameter("used_quan", Util.ConvertListToString(used_quan)),
            new Parameter("keep_quan", Util.ConvertListToString(keep_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_dungeon_pearl(int idx)
        {
            BigQuery_GrowthLog("log_dungeon_pearl",
            new Parameter("idx", idx.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_dungeon_joker(int type)
        {
            BigQuery_GrowthLog("log_dungeon_joker",
            new Parameter("type", type.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_descend_enforce(int idx, int level,
            List<int> used_type, List<double> before_quan, List<double> used_quan, List<double> after_quan,
            int exp)
        {
            BigQuery_GrowthLog("log_descend_enforce",
            new Parameter("idx", idx.ToString()),
            new Parameter("level", level.ToString()),
            new Parameter("used_type", Util.ConvertListToString(used_type)),
            new Parameter("former_quan", Util.ConvertListToString(before_quan)),
            new Parameter("used_quan", Util.ConvertListToString(used_quan)),
            new Parameter("keep_quan", Util.ConvertListToString(after_quan)),
            new Parameter("exp", level.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_descend_acquire(int idx,
            int used_type, double former_quan, double used_quan, double keep_quan)
        {
            BigQuery_GrowthLog("log_descend_acquire",
            new Parameter("idx", idx.ToString()),
            new Parameter("used_type", used_type.ToString()),
            new Parameter("former_quan", former_quan.ToString()),
            new Parameter("used_quan", used_quan.ToString()),
            new Parameter("keep_quan", keep_quan.ToString()));
        }
        //------------------------------------------------------------------------------------

        public void SendLog_log_descend_limitup(int idx, int level,
            List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan)
        {
            BigQuery_GrowthLog("log_descend_limitup",
            new Parameter("idx", idx.ToString()),
            new Parameter("level", level.ToString()),
            new Parameter("used_type", Util.ConvertListToString(used_type)),
            new Parameter("former_quan", Util.ConvertListToString(former_quan)),
            new Parameter("used_quan", Util.ConvertListToString(used_quan)),
            new Parameter("keep_quan", Util.ConvertListToString(keep_quan)));
        }
        //------------------------------------------------------------------------------------

        public void SendLog_log_skill_enforce(int idx, int level,
            List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan,
            int exp)
        {
            BigQuery_GrowthLog("log_skill_enforce",
            new Parameter("idx", idx.ToString()),
            new Parameter("level", level.ToString()),
            new Parameter("used_type", Util.ConvertListToString(used_type)),
            new Parameter("former_quan", Util.ConvertListToString(former_quan)),
            new Parameter("used_quan", Util.ConvertListToString(used_quan)),
            new Parameter("keep_quan", Util.ConvertListToString(keep_quan)),
            new Parameter("exp", exp.ToString()));
        }
        //------------------------------------------------------------------------------------


        public void SendLog_log_skill_acquire(int idx,
            int used_type, double former_quan, double used_quan, double keep_quan)
        {
            BigQuery_GrowthLog("log_skill_acquire",
            new Parameter("idx", idx.ToString()),
            new Parameter("used_type", used_type.ToString()),
            new Parameter("former_quan", former_quan.ToString()),
            new Parameter("used_quan", used_quan.ToString()),
            new Parameter("keep_quan", keep_quan.ToString()));
        }
        //------------------------------------------------------------------------------------

        public void SendLog_log_skill_limitup(int idx, int level,
            List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan)
        {
            BigQuery_GrowthLog("log_skill_limitup",
            new Parameter("idx", idx.ToString()),
            new Parameter("level", level.ToString()),
            new Parameter("used_type", Util.ConvertListToString(used_type)),
            new Parameter("former_quan", Util.ConvertListToString(former_quan)),
            new Parameter("used_quan", Util.ConvertListToString(used_quan)),
            new Parameter("keep_quan", Util.ConvertListToString(keep_quan)));
        }
        //------------------------------------------------------------------------------------

        public void SendLog_log_artifact_enforce(int idx, int level)
        {
            BigQuery_GrowthLog("log_artifact_enforce",
            new Parameter("idx", idx.ToString()),
            new Parameter("level", level.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_timeattack_mission(int idx, int type,
    List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            BigQuery_GrowthLog("log_timeattack_mission",
            new Parameter("idx", idx.ToString()),
            new Parameter("type", type.ToString()),
            new Parameter("reward_type", Util.ConvertListToString(reward_type)),
            new Parameter("before_quan", Util.ConvertListToString(before_quan)),
            new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
            new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_stamina_dia(int used_type, double former_quan, double used_quan, double keep_quan)
        {
            BigQuery_GrowthLog("log_stamina_dia",
            new Parameter("used_type", used_type.ToString()),
            new Parameter("former_quan", former_quan.ToString()),
            new Parameter("used_quan", used_quan.ToString()),
            new Parameter("keep_quan", keep_quan.ToString()));
        }

        //------------------------------------------------------------------------------------
        public void SendLog_log_research_levelup(int idx, int level, int used_type, double former_quan, double used_quan, double keep_quan)
        {
            BigQuery_GrowthLog("log_research_levelup",
                new Parameter("idx", idx.ToString()),
                new Parameter("level", level.ToString()),
            new Parameter("used_type", used_type.ToString()),
            new Parameter("former_quan", former_quan.ToString()),
            new Parameter("used_quan", used_quan.ToString()),
            new Parameter("keep_quan", keep_quan.ToString()));
        }

        //------------------------------------------------------------------------------------
        public void SendLog_log_research_speedup(int idx, int level, int used_type, double former_quan, double used_quan, double keep_quan)
        {
            BigQuery_GrowthLog("log_research_speedup",
                new Parameter("idx", idx.ToString()),
                new Parameter("level", level.ToString()),
            new Parameter("used_type", used_type.ToString()),
            new Parameter("former_quan", former_quan.ToString()),
            new Parameter("used_quan", used_quan.ToString()),
            new Parameter("keep_quan", keep_quan.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_fossil_acquire(int type,
            int reward_type, double before_quan, double reward_quan, double after_quan, 
            int used_type, double former_quan, double used_quan, double keep_quan)
        {
            BigQuery_GrowthLog("log_fossil_acquire",
                new Parameter("type", type.ToString()),
            new Parameter("reward_type", reward_type.ToString()),
            new Parameter("before_quan", before_quan.ToString()),
            new Parameter("reward_quan", reward_quan.ToString()),
            new Parameter("after_quan", after_quan.ToString()),
            new Parameter("used_type", used_type.ToString()),
            new Parameter("former_quan", former_quan.ToString()),
            new Parameter("used_quan", used_quan.ToString()),
            new Parameter("keep_quan", keep_quan.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_rune_synthesis(int idx, int level, 
            List<int> used_type, int instance)
        {
            BigQuery_GrowthLog("log_rune_synthesis",
                new Parameter("idx", idx.ToString()),
                new Parameter("level", level.ToString()),
            new Parameter("used_type", Util.ConvertListToString(used_type)));
            //new Parameter("instance", instance.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_equip_gear(int idx)
        {
            BigQuery_GrowthLog("log_equip_gear",
            new Parameter("idx", idx.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_equip_synthesis(int idx, int level,
            List<int> used_type, int instance)
        {
            BigQuery_GrowthLog("log_equip_synthesis",
                new Parameter("idx", idx.ToString()),
                new Parameter("level", level.ToString()),
            new Parameter("used_type", Util.ConvertListToString(used_type)));
            //new Parameter("instance", instance.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_skill_reset(int idx, string level,
            List<int> reward_type, List<double> reward_quan)
        {
            BigQuery_GrowthLog("log_skill_reset",
                new Parameter("idx", idx.ToString()),
                new Parameter("level", level),
            new Parameter("reward_type", Util.ConvertListToString(reward_type)),
            new Parameter("reward_quan", Util.ConvertListToString(reward_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_gearslot_reset(int idx, int level,
            List<int> reward_type, List<double> reward_quan)
        {
            BigQuery_GrowthLog("log_gearslot_reset",
                new Parameter("idx", idx.ToString()),
                new Parameter("level", level.ToString()),
            new Parameter("reward_type", Util.ConvertListToString(reward_type)),
            new Parameter("reward_quan", Util.ConvertListToString(reward_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_gearslot_enforce(int type, int level,
            List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan)
        {
            BigQuery_GrowthLog("log_gearslot_enforce",
            new Parameter("type", type.ToString()),
            new Parameter("level", level.ToString()),
            new Parameter("used_type", Util.ConvertListToString(used_type)),
            new Parameter("former_quan", Util.ConvertListToString(former_quan)),
            new Parameter("used_quan", Util.ConvertListToString(used_quan)),
            new Parameter("keep_quan", Util.ConvertListToString(keep_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_mission(int idx)
        {
            BigQuery_GrowthLog("log_mission",
            new Parameter("idx", idx.ToString()));
        }
        //------------------------------------------------------------------------------------

        public void SendLog_log_playmode(V2Enum_ARR_BattleSpeed type)
        {
            BigQuery_GrowthLog("log_playmode",
            new Parameter("type", type.ToString()));
        }
        //------------------------------------------------------------------------------------





        //------------------------------------------------------------------------------------
        public void SendLog_GuideEvent(int idx, int reward_type, int before_quan, int reward_quan, int after_quan)
        {
            //BigQuery_GrowthLog("log_guide",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("reward_type",reward_type.ToString()),
                //new Parameter("before_quan",before_quan.ToString()),
                //new Parameter("reward_quan",reward_quan.ToString()),
                //new Parameter("after_quan", after_quan.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Map_BossEvent(int idx, int result,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_map_boss",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("result", result.ToString()),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Map_DefeatEvent()
        {
            //BigQuery_GrowthLog("log_map_defeat");
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Dg_ResultEvent(int type,
            int stage, int result, int count, double rec_now, double rec_season,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_dg_result",
                //new Parameter("type", type.ToString()),
                //new Parameter("stage", stage == 0 ? null : stage.ToString()),
                //new Parameter("result", result.ToString()),
                //new Parameter("count", count.ToString()),
                //new Parameter("rec_now", rec_now.ToString()),
                //new Parameter("rec_season", rec_season.ToString()),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Char_UpEvent(double former_quan, double used_quan, double keep_quan)
        {
            //BigQuery_GrowthLog("log_char_up",
                //new Parameter("former_quan", former_quan.ToString()),
                //new Parameter("used_quan", used_quan.ToString()),
                //new Parameter("keep_quan", keep_quan.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Char_AscenEvent(double former_quan, double used_quan, double keep_quan)
        {
            //BigQuery_GrowthLog("log_char_Ascen",
                //new Parameter("former_quan", former_quan.ToString()),
                //new Parameter("used_quan", used_quan.ToString()),
                //new Parameter("keep_quan", keep_quan.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Rune_GetEvent(int reward_type, double former_quan, double used_quan, double keep_quan)
        {
            //BigQuery_GrowthLog("log_rune_get",
                //new Parameter("reward_type", reward_type.ToString()),
                //new Parameter("former_quan", former_quan.ToString()),
                //new Parameter("used_quan", used_quan.ToString()),
                //new Parameter("keep_quan", keep_quan.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Rune_UpEvent(int idx, int after_lv, double former_quan, double used_quan, double keep_quan)
        {
            //BigQuery_GrowthLog("log_rune_up",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("after_lv", after_lv.ToString()),
                //new Parameter("former_quan", former_quan.ToString()),
                //new Parameter("used_quan", used_quan.ToString()),
                //new Parameter("keep_quan", keep_quan.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Rune_AscenEvent(int idx, int grade, double former_quan, double used_quan, double keep_quan)
        {
            //BigQuery_GrowthLog("log_rune_ascen",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("grade", grade.ToString()),
                //new Parameter("former_quan", former_quan.ToString()),
                //new Parameter("used_quan", used_quan.ToString()),
                //new Parameter("keep_quan", keep_quan.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_MasteryEvent(int type, int idx, int after_lv, double former_quan, double used_quan, double keep_quan)
        {
            //BigQuery_GrowthLog("log_mastery",
                //new Parameter("type", type.ToString()),
                //new Parameter("idx", idx.ToString()),
                //new Parameter("after_lv", after_lv.ToString()),
                //new Parameter("former_quan", former_quan.ToString()),
                //new Parameter("used_quan", used_quan.ToString()),
                //new Parameter("keep_quan", keep_quan.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Ally_UpEvent(int idx, int type, int after_lv, 
            List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan)
        {
            //BigQuery_GrowthLog("log_ally_up",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("type", type.ToString()),
                //new Parameter("after_lv", after_lv.ToString()),
                //new Parameter("used_type", Util.ConvertListToString(used_type)),
                //new Parameter("former_quan", Util.ConvertListToString(former_quan)),
                //new Parameter("used_quan", Util.ConvertListToString(used_quan)),
                //new Parameter("keep_quan", Util.ConvertListToString(keep_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Ally_AscenEvent(int idx, int type, List<int> used_type, int grade)
        {
            //BigQuery_GrowthLog("log_ally_ascen",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("type", type.ToString()),
                //new Parameter("used_type", Util.ConvertListToString(used_type)),
                //new Parameter("grade", grade.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Dg_AdEvent(int type, int skip_flag)
        {
            if (skip_flag == 0)
            {
                //BigQuery_GrowthLog("log_dg_ad",
                //new Parameter("type", type.ToString()));
            }
            else
            {
                //BigQuery_GrowthLog("log_dg_ad",
                //new Parameter("type", type.ToString()),
                //new Parameter("skip_flag", skip_flag.ToString()));
            }
        }
        //------------------------------------------------------------------------------------
        public void SendLog_QuestEvent(V2Enum_QuestType type, List<int> idx, 
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            BigQuery_GrowthLog("log_quest",
            new Parameter("type", type.ToString()),
            new Parameter("idx", Util.ConvertListToString(idx)),
            new Parameter("reward_type", Util.ConvertListToString(reward_type)),
            new Parameter("before_quan", Util.ConvertListToString(before_quan)),
            new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
            new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Quest_upperbarEvent(V2Enum_QuestType type, List<int> idx,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            BigQuery_GrowthLog("log_quest_upperbar",
            new Parameter("type", type.ToString()),
            new Parameter("idx", Util.ConvertListToString(idx)),
            new Parameter("reward_type", Util.ConvertListToString(reward_type)),
            new Parameter("before_quan", Util.ConvertListToString(before_quan)),
            new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
            new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_roulette_getEvent(List<int> idx)
        {
            //BigQuery_GrowthLog("log_roulette_get",
                //new Parameter("idx", Util.ConvertListToString(idx)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_AttendEvent(V2Enum_CheckInType type, int count,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan,
            int skip_flag)
        {
            if (skip_flag == 0)
            {
                //BigQuery_GrowthLog("log_attend",
                //new Parameter("type", type.ToString()),
                //new Parameter("count", count.ToString()),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
            }
            else
            {
                //BigQuery_GrowthLog("log_attend",
                //new Parameter("type", type.ToString()),
                //new Parameter("count", count.ToString()),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)),
                //new Parameter("skip_flag", skip_flag.ToString()));
            }
        }
        //------------------------------------------------------------------------------------
        public void SendLog_GachaEvent(int type, 
            int used_type, double former_quan, double used_quan, double keep_quan, int skip_flag)
        {
            if (skip_flag == 0)
            {
                BigQuery_GrowthLog("log_gacha",
            new Parameter("type", type.ToString()),
            new Parameter("used_type", used_type.ToString()),
            new Parameter("former_quan", former_quan.ToString()),
            new Parameter("used_quan", used_quan.ToString()),
            new Parameter("keep_quan", keep_quan.ToString()));
            }
            else
            {
                BigQuery_GrowthLog("log_gacha",
            new Parameter("type", type.ToString()),
            new Parameter("used_type", used_type.ToString()),
            new Parameter("former_quan", former_quan.ToString()),
            new Parameter("used_quan", used_quan.ToString()),
            new Parameter("keep_quan", used_quan.ToString()),
            new Parameter("skip_flag", skip_flag.ToString()));
            }
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_artifact_acquire(int idx,
            int used_type, double former_quan, double used_quan, double keep_quan)
        {
            BigQuery_GrowthLog("log_artifact_acquire",
            new Parameter("idx", idx.ToString()),
            new Parameter("used_type", used_type.ToString()),
            new Parameter("former_quan", former_quan.ToString()),
            new Parameter("used_quan", used_quan.ToString()),
            new Parameter("keep_quan", keep_quan.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Gacha_ResultEvent(int type,
            int instance, List<int> reward_type, List<double> reward_quan)
        {
            BigQuery_GrowthLog("log_gacha_result",
                    new Parameter("type", type.ToString()),
                    new Parameter("instance", instance.ToString()),
                    new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                    new Parameter("reward_quan", Util.ConvertListToString(reward_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Gacha_RewardEvent(int type,
            int idx)
        {
            BigQuery_GrowthLog("log_gacha_reward",
                    new Parameter("type", type.ToString()),
                    new Parameter("idx", idx.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_PassEvent(int idx, int type,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            BigQuery_GrowthLog("log_pass",
                    new Parameter("idx", idx.ToString()),
                    new Parameter("type", type.ToString()),
                    new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                    new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                    new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                    new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_MailEvent(int type, string idx, 
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            BigQuery_GrowthLog("log_mail",
                    new Parameter("idx", idx.ToString()),
                    new Parameter("type", type),
                    new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                    new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                    new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                    new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_OfflineEvent(int count,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_offline",
                //new Parameter("count", count.ToString()),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Offline_AdEvent(int count,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan,
            int skip_flag)
        {
            if (skip_flag == 0)
            {
                //BigQuery_GrowthLog("log_offline_ad",
                    //new Parameter("count", count.ToString()),
                    //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                    //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                    //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                    //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
            }
            else
            {
                //BigQuery_GrowthLog("log_offline_ad",
                    //new Parameter("count", count.ToString()),
                    //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                    //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                    //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                    //new Parameter("after_quan", Util.ConvertListToString(after_quan)),
                    //new Parameter("skip_flag", skip_flag.ToString()));
            }
        }
        //------------------------------------------------------------------------------------
        public void SendLog_NickEvent(string nick_before, string nick_after,
            int reward_type, double before_quan, double reward_quan, double after_quan)
        {
            //BigQuery_GrowthLog("log_mail",
                    //new Parameter("nick_before", nick_before),
                    //new Parameter("nick_after", nick_after),
                    //new Parameter("reward_type", reward_type.ToString()),
                    //new Parameter("before_quan", before_quan.ToString()),
                    //new Parameter("reward_quan", reward_quan.ToString()),
                    //new Parameter("after_quan", after_quan.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Shop_IapEvent(string idx, int shop_id, int shop_price)
        {
            BigQuery_GrowthLog("log_shop_iap",
                new Parameter("idx", idx),
                new Parameter("shop_id", shop_id.ToString()),
                new Parameter("shop_price", shop_price.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_log_shop_noniap(int shop_id,
            int used_type, double former_quan, double used_quan, double keep_quan,
            int reward_type, double before_quan, double reward_quan, double after_quan)
        {
            BigQuery_GrowthLog("log_shop_noniap",
            new Parameter("shop_id", shop_id.ToString()),
            new Parameter("used_type", used_type.ToString()),
            new Parameter("former_quan", former_quan.ToString()),
            new Parameter("used_quan", used_quan.ToString()),
            new Parameter("keep_quan", keep_quan.ToString()),
            new Parameter("reward_type", reward_type.ToString()),
            new Parameter("before_quan", before_quan.ToString()),
            new Parameter("reward_quan", reward_quan.ToString()),
            new Parameter("after_quan", after_quan.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Shop_PopEvent(int shop_id)
        {
            //BigQuery_GrowthLog("log_shop_pop",
                //new Parameter("shop_id", shop_id.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_MarketEvent(int count,
            List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan, 
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_market",
                //new Parameter("count", count.ToString()),
                //new Parameter("used_type", Util.ConvertListToString(used_type)),
                //new Parameter("former_quan", Util.ConvertListToString(former_quan)),
                //new Parameter("used_quan", Util.ConvertListToString(used_quan)),
                //new Parameter("keep_quan", Util.ConvertListToString(keep_quan)),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Clan_UpEvent(int idx,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_clan_up",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_BerserkerEvent(int idx, int before_lv, int after_lv,
    List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan)
        {
            //BigQuery_GrowthLog("log_berserker",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("before_lv", before_lv.ToString()),
                //new Parameter("after_lv", after_lv.ToString()),
                //new Parameter("used_type", Util.ConvertListToString(used_type)),
                //new Parameter("former_quan", Util.ConvertListToString(former_quan)),
                //new Parameter("used_quan", Util.ConvertListToString(used_quan)),
                //new Parameter("keep_quan", Util.ConvertListToString(keep_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_AllyArena_HonorShopEvent(int idx,
            List<int> used_type, List<double> used_quan, List<int> reward_type, List<double> reward_quan)
        {
            //BigQuery_GrowthLog("log_honorshop",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("used_type", Util.ConvertListToString(used_type)),
                //new Parameter("used_quan", Util.ConvertListToString(used_quan)),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_AllyArena_ResultEvent(IFFType result, int count, long rec_season)
        {
            //BigQuery_GrowthLog("log_arenaresult",
                //new Parameter("result", result.ToString()),
                //new Parameter("count", count.ToString()),
                //new Parameter("rec_season", rec_season.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_AD_ViewEvent(string type, int idx, int skip_flag)
        {
            BigQuery_GrowthLog("log_ad_view",
                new Parameter("type", type.ToString()),
                new Parameter("idx", idx.ToString()),
                new Parameter("skip_flag", skip_flag.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_DevilCastleEvent(int idx, int result,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_devilcastle",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("result", result.ToString()),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_God_GachaEvent(List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan,
            List<int> idx, int rec_season)
        {
            //BigQuery_GrowthLog("log_god_gacha",
                //new Parameter("used_type", Util.ConvertListToString(used_type)),
                //new Parameter("former_quan", Util.ConvertListToString(former_quan)),
                //new Parameter("used_quan", Util.ConvertListToString(used_quan)),
                //new Parameter("keep_quan", Util.ConvertListToString(keep_quan)),
                //new Parameter("idx", Util.ConvertListToString(idx)),
                //new Parameter("rec_season", rec_season.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_God_GetEvent(List<int> idx,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_god_get",
                //new Parameter("idx", Util.ConvertListToString(idx)),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_God_Dg_GetEvent(int idx)
        {
            //BigQuery_GrowthLog("log_god_dg_get",
                //new Parameter("idx", idx.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_TrialTowerEvent(int idx, int result,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_trialtower",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("result", result.ToString()),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Ally_ComposeEvent(List<int> reward_type, List<int> used_type, string type)
        {
            //BigQuery_GrowthLog("log_ally_compose",
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("used_type", Util.ConvertListToString(used_type)),
                //new Parameter("type", type));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Jewelry_ComposeEvent(List<int> reward_type, List<int> used_type, string type)
        {
            //BigQuery_GrowthLog("log_jewelry_compose",
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("used_type", Util.ConvertListToString(used_type)),
                //new Parameter("type", type));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Jewelry_AscenEvent(int idx, int type, List<int> used_type, int grade)
        {
            //BigQuery_GrowthLog("log_jewelry_ascen",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("type", type.ToString()),
                //new Parameter("used_type", Util.ConvertListToString(used_type)),
                //new Parameter("grade", grade.ToString()));
        }

        //------------------------------------------------------------------------------------
        public void SendLog_Ally_CollectionEvent(List<int> idx,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_ally_collection",
                //new Parameter("idx", Util.ConvertListToString(idx)),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_LuckyRouletteEvent(int rec_now, List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_luckyroulette",
                //new Parameter("rec_now", rec_now.ToString()),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_BoxEvent(int idx, int count, List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_box",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("count", count.ToString()),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_SummonTicketEvent(int idx, int count, List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_summonticket",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("count", count.ToString()),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_RedBull_GachaEvent(List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan,
            List<int> idx, int rec_season)
        {
            //BigQuery_GrowthLog("log_redbull_gacha",
                //new Parameter("used_type", Util.ConvertListToString(used_type)),
                //new Parameter("former_quan", Util.ConvertListToString(former_quan)),
                //new Parameter("used_quan", Util.ConvertListToString(used_quan)),
                //new Parameter("keep_quan", Util.ConvertListToString(keep_quan)),
                //new Parameter("idx", Util.ConvertListToString(idx)),
                //new Parameter("rec_season", rec_season.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_RedBull_GetEvent(List<int> idx,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_redbull_get",
                //new Parameter("idx", Util.ConvertListToString(idx)),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_RedBull_Dg_GetEvent(int idx)
        {
            //BigQuery_GrowthLog("log_redbull_dg_get",
                //new Parameter("idx", idx.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_RedBull_AddEnterEvent(double used_quan)
        {
            //BigQuery_GrowthLog("log_redbull_addenter",
                //new Parameter("used_quan", used_quan.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Skin_GetEvent(int idx)
        {
            //BigQuery_GrowthLog("log_skin_get",
                //new Parameter("idx", idx.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Skin_UpEvent(int idx, int after_lv, int used_type, double former_quan, double used_quan, double keep_quan)
        {
            //BigQuery_GrowthLog("log_skin_up",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("after_lv", after_lv.ToString()),
                //new Parameter("used_type", used_type.ToString()),
                //new Parameter("former_quan", former_quan.ToString()),
                //new Parameter("used_quan", used_quan.ToString()),
                //new Parameter("keep_quan", keep_quan.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Skin_StarUpEvent(int idx, int grade, int used_quan)
        {
            //BigQuery_GrowthLog("log_skin_starup",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("grade", grade.ToString()),
                //new Parameter("used_quan", used_quan.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Event_PassEvent(int type, List<int> idx, 
    List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_pass",
                //new Parameter("type", type.ToString()),
                //new Parameter("idx", Util.ConvertListToString(idx)),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Rotation_Once_GetEvent(List<int> idx,
    List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_rot_once_get",
                //new Parameter("idx", Util.ConvertListToString(idx)),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Rotation_Repeat_GetEvent(List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_rot_repeat_get",
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Ursula_GachaEvent(List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan,
            List<int> idx, int rec_season)
        {
            //BigQuery_GrowthLog("log_ursula_gacha",
                //new Parameter("used_type", Util.ConvertListToString(used_type)),
                //new Parameter("former_quan", Util.ConvertListToString(former_quan)),
                //new Parameter("used_quan", Util.ConvertListToString(used_quan)),
                //new Parameter("keep_quan", Util.ConvertListToString(keep_quan)),
                //new Parameter("idx", Util.ConvertListToString(idx)),
                //new Parameter("rec_season", rec_season.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Ursula_GetEvent(List<int> idx,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_ursula_get",
                //new Parameter("idx", Util.ConvertListToString(idx)),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Ursula_Dg_GetEvent(int idx)
        {
            //BigQuery_GrowthLog("log_ursula_dg_get",
                //new Parameter("idx", idx.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Ursula_AddEnterEvent(double used_quan)
        {
            //BigQuery_GrowthLog("log_ursula_addenter",
                //new Parameter("used_quan", used_quan.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Dig_FailEvent(List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan,
            List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan,
            int idx, int rec_season)
        {
            //BigQuery_GrowthLog("log_dig_Fail",
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)),
                //new Parameter("used_type", Util.ConvertListToString(used_type)),
                //new Parameter("former_quan", Util.ConvertListToString(former_quan)),
                //new Parameter("used_quan", Util.ConvertListToString(used_quan)),
                //new Parameter("keep_quan", Util.ConvertListToString(keep_quan)),
                //new Parameter("idx", idx.ToString()),
                //new Parameter("rec_season", rec_season.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Dig_findEvent(List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan,
            int count, int rec_season)
        {
            //BigQuery_GrowthLog("log_dig_find",
                //new Parameter("used_type", Util.ConvertListToString(used_type)),
                //new Parameter("former_quan", Util.ConvertListToString(former_quan)),
                //new Parameter("used_quan", Util.ConvertListToString(used_quan)),
                //new Parameter("keep_quan", Util.ConvertListToString(keep_quan)),
                //new Parameter("rec_season", rec_season.ToString()),
                //new Parameter("count", count.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Dig_GetEvent(List<int> idx,
    List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_dig_get",
                //new Parameter("idx", Util.ConvertListToString(idx)),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Dig_AddShovelEvent(double used_quan)
        {
            //BigQuery_GrowthLog("log_dig_addshovel",
                //new Parameter("used_quan", used_quan.ToString()));
        }
        //------------------------------------------------------------------------------------

        public void SendLog_Dig_FreePassEvent(int idx, int rec_now,
List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_dig_freepass",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("rec_now", rec_now.ToString()),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Dig_PaidPassEvent(int idx, int rec_now)
        {
            //BigQuery_GrowthLog("log_dig_paidpass",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("rec_now", rec_now.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_ForgeEvent(int idx, int result, int before_lv, int after_lv, int skip_flag, 
            List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan)
        {
            //BigQuery_GrowthLog("log_forge",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("result", result.ToString()),
                //new Parameter("before_lv", before_lv.ToString()),
                //new Parameter("after_lv", after_lv.ToString()),
                //new Parameter("skip_flag", skip_flag.ToString()),
                //new Parameter("used_type", Util.ConvertListToString(used_type)),
                //new Parameter("former_quan", Util.ConvertListToString(former_quan)),
                //new Parameter("used_quan", Util.ConvertListToString(used_quan)),
                //new Parameter("keep_quan", Util.ConvertListToString(keep_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Forge_CollectionEvent(int idx, int result, int before_lv, int after_lv)
        {
            //BigQuery_GrowthLog("log_forge_collection",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("result", result.ToString()),
                //new Parameter("before_lv", before_lv.ToString()),
                //new Parameter("after_lv", after_lv.ToString()));
        }
        //------------------------------------------------------------------------------------





        public void SendLog_MathRpg_StepUpEvent(int stage, int rec_now, int idx,
List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_mathrpg_stepup",
                //new Parameter("stage", stage.ToString()),
                //new Parameter("rec_now", rec_now.ToString()),
                //new Parameter("idx", idx.ToString()),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_MathRpg_RouletteEvent(int idx, int gacha_lv,
            List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan,
List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_mathrpg_roulette",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("gacha_lv", gacha_lv.ToString()),

                //new Parameter("used_type", Util.ConvertListToString(used_type)),
                //new Parameter("former_quan", Util.ConvertListToString(former_quan)),
                //new Parameter("used_quan", Util.ConvertListToString(used_quan)),
                //new Parameter("keep_quan", Util.ConvertListToString(keep_quan)),

                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_MathRpg_RouletteAccumEvent(int idx,
List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_mathrpg_rouletteaccum",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_MathRpg_FreePassEvent(int idx, int rec_now,
List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_mathrpg_freepass",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("rec_now", rec_now.ToString()),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_MathRpg_PaidPassEvent(int idx, int rec_now)
        {
            //BigQuery_GrowthLog("log_mathrpg_paidpass",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("rec_now", rec_now.ToString()));
        }
        //------------------------------------------------------------------------------------

        public void SendLog_KingSlime_AddEnterEvent(double used_quan)
        {
            //BigQuery_GrowthLog("log_kingslime_addenter",
                //new Parameter("used_quan", used_quan.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_KingSlime_Dg_GetEvent(int idx)
        {
            //BigQuery_GrowthLog("log_kingslime_dg_get",
                //new Parameter("idx", idx.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_KingSlime_GetEvent(List<int> idx,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_kingslime_get",
                //new Parameter("idx", Util.ConvertListToString(idx)),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_KingSlime_GachaEvent(List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan,
            List<int> idx, int rec_season)
        {
            //BigQuery_GrowthLog("log_kingslime_gacha",
                //new Parameter("used_type", Util.ConvertListToString(used_type)),
                //new Parameter("former_quan", Util.ConvertListToString(former_quan)),
                //new Parameter("used_quan", Util.ConvertListToString(used_quan)),
                //new Parameter("keep_quan", Util.ConvertListToString(keep_quan)),
                //new Parameter("idx", Util.ConvertListToString(idx)),
                //new Parameter("rec_season", rec_season.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Guild_CreateEvent(string type, 
            List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan)
        {
            //BigQuery_GrowthLog("log_guild_create",
                //new Parameter("type", type),
                //new Parameter("used_type", Util.ConvertListToString(used_type)),
                //new Parameter("former_quan", Util.ConvertListToString(former_quan)),
                //new Parameter("used_quan", Util.ConvertListToString(used_quan)),
                //new Parameter("keep_quan", Util.ConvertListToString(keep_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Guild_CoinShopEvent(int idx,
            List<int> used_type, List<double> former_quan, List<double> used_quan, List<double> keep_quan,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_guild_coinshop",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("used_type", Util.ConvertListToString(used_type)),
                //new Parameter("former_quan", Util.ConvertListToString(former_quan)),
                //new Parameter("used_quan", Util.ConvertListToString(used_quan)),
                //new Parameter("keep_quan", Util.ConvertListToString(keep_quan)),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        public void SendLog_Guild_CheckInRewardEvent(List<int> idx,
    List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_guild_checkinreward",
                //new Parameter("idx", Util.ConvertListToString(idx)),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Guild_CheckInEvent(int idx, int rec_season,
            List<int> used_type, List<double> used_quan)
        {
            //BigQuery_GrowthLog("log_guild_checkin",
               //new Parameter("idx", idx.ToString()),
               //new Parameter("rec_season", rec_season.ToString()),
               //new Parameter("used_type", Util.ConvertListToString(used_type)),
               //new Parameter("used_quan", Util.ConvertListToString(used_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Guild_PurDonaEvent(int count)
        {
            //BigQuery_GrowthLog("log_guild_purdona",
            //new Parameter("count", count.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Guild_PurrewardEvent(List<int> idx,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_guild_purreward",
            //new Parameter("idx", Util.ConvertListToString(idx)),
            //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
            //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
            //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
            //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Guild_RaidShopEvent(int idx,
            List<int> used_type, List<double> used_quan, List<int> reward_type, List<double> reward_quan)
        {
            //BigQuery_GrowthLog("log_guild_raidshop",
                //new Parameter("idx", idx.ToString()),
                //new Parameter("used_type", Util.ConvertListToString(used_type)),
                //new Parameter("used_quan", Util.ConvertListToString(used_quan)),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Guild_RankDunEvent(V2Enum_Dungeon type, int rec_now)
        {
            //BigQuery_GrowthLog("log_guild_rankDun",
                //new Parameter("type", type.ToString()),
                //new Parameter("rec_now", rec_now.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Guild_NorDunEvent(V2Enum_Dungeon type, int rec_now,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_guild_norDun",
            //new Parameter("type", type.ToString()),
            //new Parameter("rec_now", rec_now.ToString()),
            //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
            //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
            //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
            //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Guild_NorDunSweepEvent(V2Enum_Dungeon type, int rec_now, int count,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_guild_norDunsweep",
            //new Parameter("type", type.ToString()),
            //new Parameter("rec_now", rec_now.ToString()),
            //new Parameter("count", count.ToString()),
            //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
            //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
            //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
            //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Guild_RankAddEnterEvent(double used_quan)
        {
            //BigQuery_GrowthLog("log_guild_rankaddenterr",
                //new Parameter("used_quan", used_quan.ToString()));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_Guild_NorDunGetEvent(List<int> idx,
            List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_guild_nordunget",
                //new Parameter("idx", Util.ConvertListToString(idx)),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------
        public void SendLog_SevenDay_OnceEvent(List<int> idx,
    List<int> reward_type, List<double> before_quan, List<double> reward_quan, List<double> after_quan)
        {
            //BigQuery_GrowthLog("log_sevenday_once",
                //new Parameter("idx", Util.ConvertListToString(idx)),
                //new Parameter("reward_type", Util.ConvertListToString(reward_type)),
                //new Parameter("before_quan", Util.ConvertListToString(before_quan)),
                //new Parameter("reward_quan", Util.ConvertListToString(reward_quan)),
                //new Parameter("after_quan", Util.ConvertListToString(after_quan)));
        }
        //------------------------------------------------------------------------------------

        // Util 쪽으로 이동
        //public string Util.ConvertListToString(List<int> listdata)
        //{
        //    if (listdata == null)
        //        return string.Empty;

        //    string str = string.Empty;

        //    for (int i = 0; i < listdata.Count; ++i)
        //    {
        //        if (i == 0)
        //            str = listdata[i].ToString();
        //        else
        //            str = string.Format("{0}, {1}", str, listdata[i]);
        //    }

        //    return str;
        //}
        ////------------------------------------------------------------------------------------
        //public string Util.ConvertListToString(List<double> listdata)
        //{
        //    if (listdata == null)
        //        return string.Empty;

        //    string str = string.Empty;

        //    for (int i = 0; i < listdata.Count; ++i)
        //    {
        //        if (i == 0)
        //            str = listdata[i].ToString();
        //        else
        //            str = string.Format("{0}, {1}", str, listdata[i]);
        //    }

        //    return str;
        //}
        ////------------------------------------------------------------------------------------
        //public string Util.ConvertListToString(List<string> listdata)
        //{
        //    if (listdata == null)
        //        return string.Empty;

        //    string str = string.Empty;

        //    for (int i = 0; i < listdata.Count; ++i)
        //    {
        //        if (i == 0)
        //            str = listdata[i];
        //        else
        //            str = string.Format("{0}, {1}", str, listdata[i]);
        //    }

        //    return str;
        //}
        ////------------------------------------------------------------------------------------
    }
}