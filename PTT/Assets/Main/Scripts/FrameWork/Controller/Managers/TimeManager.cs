using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class TimeManager : MonoSingleton<TimeManager>
    {
        public ObscuredDouble Current_TimeStamp = 0.0;
        public ObscuredDouble DailyInit_TimeStamp = 0.0;
        public ObscuredDouble WeekInit_TimeStamp = 0.0;
        public ObscuredDouble MonthInit_TimeStamp = 0.0;

        public event OnCallBack_Double OnInitDailyContent;
        public event OnCallBack_Double RemainInitDailyContent;
        public string RemainInitDailyContent_String = string.Empty;
        public event OnCallBack_String RemainInitDailyContent_Text;

        public event OnCallBack_Double OnInitWeekContent;
        public event OnCallBack_Double RemainInitWeekContent;
        public string RemainInitWeekContent_String = string.Empty;
        public event OnCallBack_String RemainInitWeekContent_Text;

        public event OnCallBack_Double OnInitMonthContent;
        public event OnCallBack_Double RemainInitMonthContent;
        public string RemainInitMonthContent_String = string.Empty;
        public event OnCallBack_String RemainInitMonthContent_Text;

        private WaitForSecondsRealtime m_addTimeWaitForSecondsRealtime = new WaitForSecondsRealtime(0.1f);
        //private WaitForSeconds m_addTimeWaitForSecondsRealtime = new WaitForSeconds(0.1f);

        //------------------------------------------------------------------------------------
        protected override void Init()
        {

        }
        //------------------------------------------------------------------------------------
        public IEnumerator InitTimeManager()
        {
            bool recvtiem = false;
            TheBackEnd.TheBackEndManager.Instance.GetServerTime(x =>
            {
                recvtiem = true;
                ConvertServerTime(x);
            });

            while (recvtiem == false)
            {
                yield return null;
            }
        }
        //------------------------------------------------------------------------------------
        public void InitTimeContent()
        {
            StartCoroutine(AddTimeValue());

            if (DailyInit_TimeStamp > TimeContainer.DailyInitTimeStamp)
            {
                TimeContainer.AccumLoginCount++;
                TimeContainer.DailyInitTimeStamp = DailyInit_TimeStamp;

                Debug.LogWarning(string.Format("{0}일", TimeContainer.AccumLoginCount.GetDecrypted()));
            }
        }
        //------------------------------------------------------------------------------------
        public void RefreshServerTime()
        {
            TheBackEnd.TheBackEndManager.Instance.GetServerTime(x =>
            {
                DateTime dateTime = x;
                string checktime = dateTime.ToString();
                Debug.LogWarning(string.Format("Current Time : {0}", dateTime.ToString()));

                ObscuredDouble refreshTime = ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
                Debug.Log(string.Format("RefreshTime : {0}  CurrentTime : {1}", refreshTime.GetDecrypted(), Current_TimeStamp.GetDecrypted()));

                //if (Current_TimeStamp > refreshTime + 300)
                //{
                //    TheBackEnd.TheBackEndManager.Instance.OnCheatingDetected();
                //    return;
                //}

                Current_TimeStamp = refreshTime.GetDecrypted();
            });
        }
        //------------------------------------------------------------------------------------
        private void ConvertServerTime(DateTime dateTime)
        {
            string checktime = dateTime.ToString();
            Debug.LogWarning(string.Format("Current Time : {0}", dateTime.ToString()));

            Current_TimeStamp = ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
            SetDaily_Init_TimeStamp(Current_TimeStamp);
            SetWeek_Init_TimeStamp(Current_TimeStamp);
            SetMonth_Init_TimeStamp(Current_TimeStamp);
        }
        //------------------------------------------------------------------------------------
        private void SetDaily_Init_TimeStamp(double serverTimeStamp)
        {
            double timestamp = serverTimeStamp;
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(timestamp);
            DateTime initdt = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0, DateTimeKind.Utc);
            initdt = initdt.AddDays(1.0);
            Debug.LogWarning(string.Format("DailyInit : {0}", initdt.ToString()));

            DailyInit_TimeStamp = ((DateTimeOffset)initdt).ToUnixTimeSeconds();
        }
        //------------------------------------------------------------------------------------
        private void SetWeek_Init_TimeStamp(double serverTimeStamp)
        {
            double timestamp = serverTimeStamp;
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(timestamp);
            DateTime initdt = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0, DateTimeKind.Utc);
            double addday = 7 - (initdt.DayOfWeek.Enum32ToInt() - DayOfWeek.Monday.Enum32ToInt());
            if (addday > 7)
                addday -= 7;

            initdt = initdt.AddDays(addday);
            Debug.LogWarning(string.Format("WeekInit : {0}", initdt.ToString()));

            WeekInit_TimeStamp = ((DateTimeOffset)initdt).ToUnixTimeSeconds();
        }
        //------------------------------------------------------------------------------------
        private void SetMonth_Init_TimeStamp(double serverTimeStamp)
        {
            double timestamp = serverTimeStamp;
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(timestamp);
            DateTime initdt = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0, DateTimeKind.Utc);
            initdt = initdt.AddDays(1 - initdt.Day);
            initdt = initdt.AddMonths(1);
            Debug.LogWarning(string.Format("MonthInit : {0}", initdt.ToString()));

            MonthInit_TimeStamp = ((DateTimeOffset)initdt).ToUnixTimeSeconds();
        }
        //------------------------------------------------------------------------------------
        public ObscuredDouble CheatTime = 1.0;
        private ObscuredDouble AddQeustTimeCheck = 0.0;
        //------------------------------------------------------------------------------------
        private IEnumerator AddTimeValue()
        {
            while (isAlive)
            {
                yield return m_addTimeWaitForSecondsRealtime;

                double AddSecond = 0.1;

#if DEV_DEFINE
                if (Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.Develop
                    || Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.QA)
                {
                    AddSecond = 0.1 * CheatTime;
                }
#endif

                Current_TimeStamp += AddSecond;
                TimeContainer.AccumLoginTime += AddSecond;

                AddQeustTimeCheck += AddSecond;
                if (AddQeustTimeCheck > 60)
                {
                    AddQeustTimeCheck -= 60;
                }

                if (Current_TimeStamp >= DailyInit_TimeStamp)
                { // 하루 초기화 컨텐츠
                    SetDaily_Init_TimeStamp(Current_TimeStamp);

                    if (OnInitDailyContent != null)
                        OnInitDailyContent(DailyInit_TimeStamp);

                    if (DailyInit_TimeStamp > TimeContainer.DailyInitTimeStamp)
                    {
                        TimeContainer.AccumLoginCount++;
                        TimeContainer.DailyInitTimeStamp = DailyInit_TimeStamp;

                        Debug.LogWarning(string.Format("{0}일", TimeContainer.AccumLoginCount.GetDecrypted()));
                    }
                }

                if (Current_TimeStamp >= WeekInit_TimeStamp)
                { // 일주일(월요일) 초기화 컨텐츠
                    SetWeek_Init_TimeStamp(Current_TimeStamp);

                    if (OnInitWeekContent != null)
                        OnInitWeekContent(WeekInit_TimeStamp);
                }

                if (Current_TimeStamp >= MonthInit_TimeStamp)
                { // 한달(월초) 초기화 컨텐츠
                    SetMonth_Init_TimeStamp(Current_TimeStamp);

                    if (OnInitMonthContent != null)
                        OnInitMonthContent(MonthInit_TimeStamp);
                }

                RemainInitDailyContent?.Invoke(DailyInit_TimeStamp - Current_TimeStamp);
                
                RemainInitWeekContent?.Invoke(WeekInit_TimeStamp - Current_TimeStamp);

                RemainInitMonthContent?.Invoke(MonthInit_TimeStamp - Current_TimeStamp);

                {
                    TimeSpan dateTime = new TimeSpan(0, 0, (int)(DailyInit_TimeStamp - Current_TimeStamp));

                    RemainInitDailyContent_String = string.Format("{0} {1} {2}"
                        , string.Format(LocalStringManager.Instance.GetLocalString(Define.HourLocalKey), dateTime.Hours)
                        , string.Format(LocalStringManager.Instance.GetLocalString(Define.MinuteLocalKey), dateTime.Minutes)
                        , string.Format(LocalStringManager.Instance.GetLocalString(Define.SecondLocalKey), dateTime.Seconds)
                        );

                    RemainInitDailyContent_Text?.Invoke(RemainInitDailyContent_String);
                }

                {
                    TimeSpan dateTime = new TimeSpan(0, 0, (int)(WeekInit_TimeStamp - Current_TimeStamp));

                    RemainInitWeekContent_String = string.Format("{0} {1} {2} {3}"
                        , string.Format(LocalStringManager.Instance.GetLocalString(Define.DayLocalKey), dateTime.Days)
                        , string.Format(LocalStringManager.Instance.GetLocalString(Define.HourLocalKey), dateTime.Hours)
                        , string.Format(LocalStringManager.Instance.GetLocalString(Define.MinuteLocalKey), dateTime.Minutes)
                        , string.Format(LocalStringManager.Instance.GetLocalString(Define.SecondLocalKey), dateTime.Seconds)
                        );

                    RemainInitWeekContent_Text?.Invoke(RemainInitWeekContent_String);
                }


                {
                    TimeSpan dateTime = new TimeSpan(0, 0, (int)(MonthInit_TimeStamp - Current_TimeStamp));

                    RemainInitMonthContent_String = string.Format("{0} {1} {2} {3}"
                        , string.Format(LocalStringManager.Instance.GetLocalString(Define.DayLocalKey), dateTime.Days)
                        , string.Format(LocalStringManager.Instance.GetLocalString(Define.HourLocalKey), dateTime.Hours)
                        , string.Format(LocalStringManager.Instance.GetLocalString(Define.MinuteLocalKey), dateTime.Minutes)
                        , string.Format(LocalStringManager.Instance.GetLocalString(Define.SecondLocalKey), dateTime.Seconds)
                        );

                    RemainInitMonthContent_Text?.Invoke(RemainInitMonthContent_String);
                }

            }
        }
        //------------------------------------------------------------------------------------
        public double GetInitTime(V2Enum_IntervalType v2Enum_IntervalType)
        {
            switch (v2Enum_IntervalType)
            {
                case V2Enum_IntervalType.Quarter:
                    {
                        return Current_TimeStamp + (60.0 * 15.0);
                    }
                case V2Enum_IntervalType.Hour:
                    {
                        return Current_TimeStamp + (60.0 * 60.0);
                    }
                case V2Enum_IntervalType.Day:
                    {
                        return DailyInit_TimeStamp;
                    }
                case V2Enum_IntervalType.Week:
                    {
                        return WeekInit_TimeStamp;
                    }
                case V2Enum_IntervalType.Month:
                    {
                        return MonthInit_TimeStamp;
                    }
                default:
                    {
                        return 0.0;
                    }
            }

            return 0.0;
        }
        //------------------------------------------------------------------------------------
        public double GetInitAddTime(V2Enum_IntervalType v2Enum_IntervalType, int cycle = 1)
        {
            switch (v2Enum_IntervalType)
            {
                case V2Enum_IntervalType.Quarter:
                    {
                        return 60.0 * (15.0 * cycle);
                    }
                case V2Enum_IntervalType.Hour:
                    {
                        return 60.0 * (60.0 * cycle);
                    }
                case V2Enum_IntervalType.Day:
                    {
                        return 86400.0 * cycle;
                    }
                case V2Enum_IntervalType.Week:
                    {
                        return 86400.0 * (7.0 * cycle);
                    }
                case V2Enum_IntervalType.Month:
                    {
                        return 86400.0 * 7.0 * (30.0 * cycle);
                    }
                default:
                    {
                        Debug.LogError(string.Format("{0} Is Not Support Type!", v2Enum_IntervalType));
                        return 0.0;
                    }
            }
        }
        //------------------------------------------------------------------------------------
        public void AddInitEvent(V2Enum_IntervalType v2Enum_IntervalType, OnCallBack_Double action)
        {
            switch (v2Enum_IntervalType)
            {
                case V2Enum_IntervalType.Day:
                    {
                        OnInitDailyContent += action;
                        break;
                    }
                case V2Enum_IntervalType.Week:
                    {
                        OnInitWeekContent += action;
                        break;
                    }
                case V2Enum_IntervalType.Month:
                    {
                        OnInitMonthContent += action;
                        break;
                    }
            }
        }
        //------------------------------------------------------------------------------------
        public string GetSecendToDayString_HMS(int rawSecond)
        {
            int remainSecond = rawSecond % 60;

            int rawMinute = rawSecond / 60;

            int remainMinute = rawMinute % 60;

            int remainHour = rawMinute / 60;

            return string.Format("{0} {1} {2}"
                , string.Format(Managers.LocalStringManager.Instance.GetLocalString("time/hour"), remainHour)
                , string.Format(Managers.LocalStringManager.Instance.GetLocalString("time/minute"), remainMinute)
                , string.Format(Managers.LocalStringManager.Instance.GetLocalString("time/second"), remainSecond)
                );
        }
        //------------------------------------------------------------------------------------
        public string GetSecendToDayString_MS(int rawSecond)
        {
            int remainSecond = rawSecond % 60;

            int rawMinute = rawSecond / 60;

            int remainMinute = rawMinute;

            return string.Format("{0} {1}"
                , string.Format(Managers.LocalStringManager.Instance.GetLocalString("time/minute"), remainMinute)
                , string.Format(Managers.LocalStringManager.Instance.GetLocalString("time/second"), remainSecond)
                );
        }
        //------------------------------------------------------------------------------------
        public int GetDayCount()
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(Current_TimeStamp);

            //Debug.Log(string.Format("JoinTime {0}   CurrentTime {1}", TheBackEnd.TheBackEnd_Login.UserJoinDate.GetDecrypted(), dt));

            //TimeSpan timeSpan = dt - TheBackEnd.TheBackEnd_Login.UserJoinDate.GetDecrypted();

            //int dayCount = dt.Day - TheBackEnd.TheBackEnd_Login.UserJoinDate.GetDecrypted().Day;

            //Debug.Log(string.Format("Count : {0}, {1} - {2}", dayCount, dt.Day, TheBackEnd.TheBackEnd_Login.UserJoinDate.GetDecrypted().Day));

            TimeSpan dayCount = dt.Date - TheBackEnd.TheBackEnd_Login.UserJoinDate.GetDecrypted().Date;

            //Debug.Log(string.Format("Count : {0}, {1} - {2}", dayCount.Days, dt.Date, TheBackEnd.TheBackEnd_Login.UserJoinDate.GetDecrypted().Date));

            return dayCount.Days;
        }
        //------------------------------------------------------------------------------------
    }
}