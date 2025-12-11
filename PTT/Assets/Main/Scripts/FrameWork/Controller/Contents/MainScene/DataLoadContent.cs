using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;
using BackEnd;
using LitJson;
using Newtonsoft.Json;

namespace GameBerry.Contents
{
    public class DataLoadContent : IContent
    {
        public static GameBerry.Event.SetNoticeMsg m_setNoticeMsg = new GameBerry.Event.SetNoticeMsg();

        public string serverCheckString = string.Empty;

        //------------------------------------------------------------------------------------
        protected override void OnLoadStart()
        {
            serverCheckString = Managers.LocalStringManager.Instance.GetLocalString("common/serverCheck");
            StartLoadData();
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
        }
        //------------------------------------------------------------------------------------
        private void StartLoadData()
        {
            StartCoroutine(LoadLocalClientTable());
        }
        //------------------------------------------------------------------------------------
        private IEnumerator LoadLocalClientTable()
        {
            Debug.Log("시간 가져오기 시작");

            m_setNoticeMsg.NoticeStr = serverCheckString;
            Message.Send(m_setNoticeMsg);

            yield return StartCoroutine(Managers.TimeManager.Instance.InitTimeManager());
            Debug.Log("시간 가져오기 끝");

            Debug.Log("테이블 로드 시작");

            bool completeGroup = false;

            Managers.GroupManager.Instance.InitGroup(() => { completeGroup = true; });

            // 로딩 카운트 버그 방지를 위해 LoadTableList로 변경

            m_setNoticeMsg.NoticeStr = serverCheckString;
            Message.Send(m_setNoticeMsg);

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            float tableLoadingTime = 0.0f;

            stopwatch.Start();


            int totalTableCount = 0;
            int completeTableCount = 0;

            // 유저 데이터 로드
            StartCoroutine(Table.UserTable.LoadUserTable((x, y) =>
            {
                totalTableCount = x;
                completeTableCount = y;
            }));

            // 디바이스에 있는 테이블들 로드
            yield return StartCoroutine(Managers.LocalTableManager.Instance.Load());

            // 뒤끝 차트 로드
            yield return StartCoroutine(Chart.GameChart.LoadGameChart(GetProgress));

            stopwatch.Stop();
            tableLoadingTime = ((float)stopwatch.ElapsedMilliseconds) * 0.001f;
            ThirdPartyLog.Instance.SendLog_TableLoadEvent(tableLoadingTime, Chart.GameChart.ChartData.Count);

            UnityEngine.Debug.LogErrorFormat("테이블 로드 완료 : {0:0.###}s", tableLoadingTime);

            stopwatch.Start();

            string tableLoadLocalString = Managers.LocalStringManager.Instance.GetLocalString("title/user");


            while (completeTableCount < totalTableCount)
            {
                m_setNoticeMsg.NoticeStr = string.Format("{0} {1}%", tableLoadLocalString, (int)(((float)completeTableCount / (float)totalTableCount) * 100.0f));

                Message.Send(m_setNoticeMsg);
                yield return null;
            }

            stopwatch.Stop();
            float dbLoadingTime = ((float)stopwatch.ElapsedMilliseconds) * 0.001f;
            ThirdPartyLog.Instance.SendLog_DBLoadEvent(dbLoadingTime);

            m_setNoticeMsg.NoticeStr = string.Format("{0} {1}%", tableLoadLocalString, (int)(((float)completeTableCount / (float)totalTableCount) * 100.0f));

            Message.Send(m_setNoticeMsg);

            Managers.TimeManager.Instance.InitTimeContent();

            Managers.NoticeManager.Instance.InitNoticeContent();

            m_setNoticeMsg.NoticeStr = Managers.LocalStringManager.Instance.GetLocalString("common/gameLoading");

            Message.Send(m_setNoticeMsg);

            while (completeGroup == false)
                yield return null;

            SetLoadComplete();
        }
        //------------------------------------------------------------------------------------
        private void GetProgress(int totalCount, int remainCount, string fileName)
        {
            Debug.Log("totalCount : " + totalCount + " remainCount : " + remainCount + " fileName : " + fileName);

            m_setNoticeMsg.NoticeStr = string.Format("{0} {1}%", serverCheckString, (int)(((float)(totalCount - remainCount) / (float)totalCount) * 100.0f));

            //m_setNoticeMsg.NoticeStr = serverCheckString;
            Message.Send(m_setNoticeMsg);
        }
        //------------------------------------------------------------------------------------
    }
}