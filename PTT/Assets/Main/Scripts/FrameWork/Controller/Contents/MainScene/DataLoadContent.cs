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
        public static List<System.Action> LoadTable = new List<Action>();
        public static GameBerry.Event.SetNoticeMsg m_setNoticeMsg = new GameBerry.Event.SetNoticeMsg();

        public string serverCheckString = string.Empty;

        //------------------------------------------------------------------------------------
        protected override void OnLoadStart()
        {
            serverCheckString = Managers.LocalStringManager.Instance.GetLocalString("common/serverCheck");
            Message.AddListener<GameBerry.Event.CompleteTableLoadMsg>(CompleteTableLoad);
            StartLoadData();
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.CompleteTableLoadMsg>(CompleteTableLoad);
        }
        //------------------------------------------------------------------------------------
        private void StartLoadData()
        {
            StartCoroutine(LoadLocalClientTable());
        }
        //------------------------------------------------------------------------------------
        private int m_completeTableCount = 0;
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

            //LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerPointTableData);
            //LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerARRRInfoTableData);
            //LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerStaminaInfoTableData);
            //LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerJobInfoTableData);
            //LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerVipPackageInfoTableData);
            //LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerMapInfoTableData);
            //LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerTimeinfoTableData);
            //LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerTimeAttackMissionInfoTableData);
            //LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerQuestInfoTableData);
            //LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerAdBuffinfoTableData);
            //LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerShopInfoTableData);
            //LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerShopRandomStoreInfoTableData);
            //LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerPassInfoTableData);
            //LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerRankTableData);
            //LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerGearInfoTableData);
            //LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerSkillinfoTableData);
            //LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerResearchinfoTableData);
            //LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerSummoninfoTableData);
            //LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerDungeonInfoTableData);

            for (int i = 0; i < LoadTable.Count; ++i)
                LoadTable[i]?.Invoke();

            m_setNoticeMsg.NoticeStr = serverCheckString;
            Message.Send(m_setNoticeMsg);

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            float tableLoadingTime = 0.0f;

            stopwatch.Start();

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



            while (m_completeTableCount < LoadTable.Count)
            {
                m_setNoticeMsg.NoticeStr = string.Format("{0} {1}%", tableLoadLocalString, (int)(((float)m_completeTableCount / (float)LoadTable.Count) * 100.0f));

                Message.Send(m_setNoticeMsg);
                yield return null;
            }

            stopwatch.Stop();
            float dbLoadingTime = ((float)stopwatch.ElapsedMilliseconds) * 0.001f;
            ThirdPartyLog.Instance.SendLog_DBLoadEvent(dbLoadingTime);

            m_setNoticeMsg.NoticeStr = string.Format("{0} {1}%", tableLoadLocalString, (int)(((float)m_completeTableCount / (float)LoadTable.Count) * 100.0f));

            Message.Send(m_setNoticeMsg);

            Managers.TimeManager.Instance.InitTimeContent();

            Managers.NoticeManager.Instance.InitNoticeContent();

            m_setNoticeMsg.NoticeStr = Managers.LocalStringManager.Instance.GetLocalString("common/gameLoading");

            Message.Send(m_setNoticeMsg);

            Chart.SkinChart skinChart = Chart.GameChart.Get<Chart.SkinChart>();


            var bro = Backend.PlayerData.GetTableList();

            if (bro.IsSuccess())
            {
                Debug.LogError(bro.ToString());
            }

            List<TableItem> tableList = new List<TableItem>();
            LitJson.JsonData tableListJson = bro.GetReturnValuetoJSON()["tables"];

            for (int i = 0; i < tableListJson.Count; i++)
            {
                TableItem tableItem = new TableItem();

                tableItem.tableName = tableListJson[i]["tableName"].ToString();
                tableItem.tableExplaination = tableListJson[i]["tableExplaination"].ToString();
                tableItem.isChecked = tableListJson[i]["isChecked"].ToString() == "true" ? true : false;
                tableItem.hasSchema = tableListJson[i]["hasSchema"].ToString() == "true" ? true : false;

                tableList.Add(tableItem);
                Debug.Log(tableItem.ToString());
            }

            while (completeGroup == false)
                yield return null;

            SetLoadComplete();
        }

        public class TableItem
        {
            public string tableName;
            public string tableExplaination;
            public bool isChecked;
            public bool hasSchema;

            public override string ToString()
            {
                return $"tableName : {tableName}\n" +
                $"tableExplaination : {tableExplaination}\n" +
                $"isChecked : {isChecked}\n" +
                $"hasSchema : {hasSchema}\n";
            }
        }

        //------------------------------------------------------------------------------------
        private void CompleteTableLoad(GameBerry.Event.CompleteTableLoadMsg msg)
        {
            m_completeTableCount++;

            //Debug.LogFormat("유저 테이블 로드 성공 {0}/{1}", m_completeTableCount, LoadTable.Count);
        }
        //------------------------------------------------------------------------------------
        private void GetProgress(int totalCount, int remainCount, string fileName)
        {
            Debug.Log("totalCount : " + totalCount + " remainCount : " + remainCount + " fileName : " + fileName);

            m_setNoticeMsg.NoticeStr = string.Format("{0} {1}%", serverCheckString, (int)(((float)(totalCount - remainCount) / (float)totalCount) * 100.0f));

            //m_setNoticeMsg.NoticeStr = serverCheckString;
            Message.Send(m_setNoticeMsg);
        }
    }
}