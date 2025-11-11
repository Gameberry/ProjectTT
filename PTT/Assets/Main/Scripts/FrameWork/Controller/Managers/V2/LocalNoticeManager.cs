using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
using Unity.Notifications.Android;
using UnityEngine.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
using UnityEngine.iOS;
#endif
using System;
using System.Linq;

namespace GameBerry.Managers
{
    public enum NoticeType
    {
        MorningRepeat = 0,
        DayRepeat,
        NightRepeat,

        CoolReward,
        ClanMission,
    }

    [System.Serializable]
    public class LocalNoticeData
    {
        public NoticeType noticeType;
        public int Hour;
        public string LocalHead;
        public string LocalBody;
    }
    public class LocalNoticeManager : MonoSingleton<LocalNoticeManager>
    {
        [HideInInspector]
        public bool isReady = false;

        [SerializeField]
        private List<LocalNoticeData> localNoticeDatas = new List<LocalNoticeData>();

        [SerializeField]
        private LocalNoticeData coolTimeRewardNotice = new LocalNoticeData();

        [SerializeField]
        private LocalNoticeData clanMissionNotice = new LocalNoticeData();

        private void SetNotice(string identifier, string title, string text, DateTime fireTime)
        {
            Debug.Log("SimpleNotification   " + title + " | " + text + " | " + fireTime);

#if UNITY_ANDROID
            int notiId = PlayerPrefs.GetInt(identifier);

            NotificationStatus status = AndroidNotificationCenter.CheckScheduledNotificationStatus(notiId);
            if (status == NotificationStatus.Scheduled)
            {
                AndroidNotificationCenter.CancelNotification(notiId);
            }
            
            var notification = new AndroidNotification();
            notification.Title = title;
            notification.Text = text;
            notification.FireTime = fireTime;
            notification.SmallIcon = "icon_0";
            notification.LargeIcon = "icon_1";

            //AndroidNotificationCenter.CheckScheduledNotificationStatus()
            notiId = AndroidNotificationCenter.SendNotification(notification, "channel_id");

            PlayerPrefs.SetInt(identifier, notiId);

#elif UNITY_IOS
        
        iOSNotification[] notis = iOSNotificationCenter.GetScheduledNotifications();
            if (notis.ToList().Exists(x => x.Identifier == identifier))
            {
                iOSNotificationCenter.RemoveScheduledNotification(identifier);
            }

            var notification = new iOSNotification(identifier);
            notification.Title = title;
            notification.Body = text;
            iOSNotificationCalendarTrigger trigger = new iOSNotificationCalendarTrigger();
            trigger.Repeats = true;
            trigger.Hour = fireTime.Hour;
            trigger.Minute = fireTime.Minute;
            trigger.Second = fireTime.Second;
            notification.Trigger = trigger;

            iOSNotificationCenter.ScheduleNotification(notification);
#endif
        }

        public void SetNotice()
        {
            if (isReady == false)
                return;

            SetBaseNotice();
            SetCoolTimeNotice();
            //SetClanMissionNotice();
        }

        public void SetBaseNotice()
        {
            LocalizeType localizeType = LocalStringManager.Instance.GetLocalizeType();

            int pushLocalType = PlayerPrefs.GetInt("NoticeType", -1);

            if (localizeType.Enum32ToInt() != pushLocalType)
            {
                DateTime NowTime = DateTime.Now;

                for (int i = 0; i < localNoticeDatas.Count; ++i)
                {
                    LocalNoticeData localNoticeData = localNoticeDatas[i];
                    DateTime fireTime = new DateTime(NowTime.Year, NowTime.Month, NowTime.Day, localNoticeData.Hour, 0, 0);

                    if (ApplyPush(fireTime) == false)
                        continue;

                    if (fireTime.Ticks < NowTime.Ticks)
                    {
                        fireTime = fireTime.AddDays(1);
                    }

                    SetNotice(localNoticeData.noticeType.ToString(), 
                        LocalStringManager.Instance.GetLocalString(localNoticeData.LocalHead),
                        LocalStringManager.Instance.GetLocalString(localNoticeData.LocalBody), 
                        fireTime);
                }

                //SetNotice(i.ToString(), LocalStringManager.Instance.GetLocalString("common/gameTitle"), fireTime.ToString("yyyy-MM-ddTHH:mm:ssZ"), fireTime);

                PlayerPrefs.SetInt("NoticeType", localizeType.Enum32ToInt());
            }
        }

        public bool ApplyPush(DateTime dateTime)
        {
            if (GameSettingManager.isAlive == false)
                return false;

            if (GameSettingManager.Instance.IsOn(GameSettingBtn.Push) == false)
                return false;

            if (GameSettingManager.Instance.IsOn(GameSettingBtn.PushNight) == false)
            {
                if (dateTime.Hour > 20 || dateTime.Hour < 8)
                    return false;
            }

            return true;
        }

        public void SetCoolTimeNotice()
        {
            if (TimeManager.isAlive == false)
                return;

            int addTime = (int)(Define.StageCooltimeRewardMaxSecond - TimeManager.Instance.GetIdleRewardTime_Second());

            if (addTime < 60)
                addTime = 60;

            DateTime NowTime = DateTime.Now;

            LocalNoticeData localNoticeData = coolTimeRewardNotice;
            DateTime fireTime = NowTime.AddSeconds(addTime);

            if (ApplyPush(fireTime) == false)
                return;

            if (fireTime.Ticks < NowTime.Ticks)
            {
                fireTime = fireTime.AddDays(1);
            }

            SetNotice(localNoticeData.noticeType.ToString(),
                LocalStringManager.Instance.GetLocalString(localNoticeData.LocalHead),
                LocalStringManager.Instance.GetLocalString(localNoticeData.LocalBody),
                fireTime);
        }

        //public void SetClanMissionNotice()
        //{
        //    if (TimeManager.isAlive == false)
        //        return;

        //    int addTime = (int)(ClanMissionContainer.LastFinishMissionTimeStemp - TimeManager.Instance.Current_TimeStamp);

        //    if (addTime < 0)
        //        return;

        //    if (addTime < 60)
        //        addTime = 60;

        //    DateTime NowTime = DateTime.Now;

        //    LocalNoticeData localNoticeData = clanMissionNotice;
        //    DateTime fireTime = NowTime.AddSeconds(addTime);

        //    if (ApplyPush(fireTime) == false)
        //        return;

        //    if (fireTime.Ticks < NowTime.Ticks)
        //    {
        //        fireTime = fireTime.AddDays(1);
        //    }

        //    SetNotice(localNoticeData.noticeType.ToString(),
        //        LocalStringManager.Instance.GetLocalString(localNoticeData.LocalHead),
        //        LocalStringManager.Instance.GetLocalString(localNoticeData.LocalBody),
        //        fireTime);
        //}
    }
}