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

            LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerPointTableData);
            LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerARRRInfoTableData);
            LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerStaminaInfoTableData);
            LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerJobInfoTableData);
            LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerVipPackageInfoTableData);
            LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerSynergyInfoTableData);
            LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerDescendInfoTableData);
            LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerSynergyRuneInfoTableData);
            LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerRelicInfoTableData);
            LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerMapInfoTableData);
            LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerTimeinfoTableData);
            LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerTimeAttackMissionInfoTableData);
            LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerQuestInfoTableData);
            LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerAdBuffinfoTableData);
            LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerShopInfoTableData);
            LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerShopRandomStoreInfoTableData);
            LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerPassInfoTableData);
            LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerRankTableData);
            LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerGearInfoTableData);
            LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerSkillinfoTableData);
            LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerResearchinfoTableData);
            LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerSummoninfoTableData);
            LoadTable.Add(TheBackEnd.TheBackEndManager.Instance.GetPlayerDungeonInfoTableData);

            for (int i = 0; i < LoadTable.Count; ++i)
                LoadTable[i]?.Invoke();

            //Backend.CDN.Content.Local.Reset();

            m_setNoticeMsg.NoticeStr = serverCheckString;
            Message.Send(m_setNoticeMsg);

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            float tableLoadingTime = 0.0f;

            stopwatch.Start();

            if (Managers.SceneManager.Instance.UseLocalChart == false)
            {
                yield return StartCoroutine(LocalUpdateAsyncIEnumerator());
            }
            
            yield return StartCoroutine(Managers.TableManager.Instance.Load());

            stopwatch.Stop();
            tableLoadingTime = ((float)stopwatch.ElapsedMilliseconds) * 0.001f;
            ThirdPartyLog.Instance.SendLog_TableLoadEvent(tableLoadingTime, TheBackEnd.TheBackEnd_GameChart.needSaveChart.Count);

            UnityEngine.Debug.LogErrorFormat("테이블 로드 완료 : {0:0.###}s", tableLoadingTime);


            //m_setNoticeMsg.NoticeStr = "시간 가져오기 끝";
            //Message.Send(m_setNoticeMsg);

            stopwatch.Start();

            //CharacterBaseStatLocalTable characterBaseStatLocalTable = Managers.TableManager.Instance.GetTableClass<CharacterBaseStatLocalTable>();
            //List<CharacterBaseStatData> m_characterBaseStatDatas = characterBaseStatLocalTable.GetAllData();
            //for (int i = 0; i < m_characterBaseStatDatas.Count; ++i)
            //{
            //    Managers.CharacterStatManager.Instance.SetDefaultStatValue(m_characterBaseStatDatas[i].BaseStat, m_characterBaseStatDatas[i].BaseValue);
            //}

            PlayerDataContainer.PlayerName = TheBackEnd.TheBackEndManager.Instance.GetNickPlayerName();

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

            GameBerry.TheBackEnd.TheBackEnd_GameChart.ChartBROData = null;

            m_setNoticeMsg.NoticeStr = string.Format("{0} {1}%", tableLoadLocalString, (int)(((float)m_completeTableCount / (float)LoadTable.Count) * 100.0f));

            Message.Send(m_setNoticeMsg);

            Managers.TimeManager.Instance.InitTimeContent();

            Managers.ResearchManager.Instance.InitResearchContent();
            Managers.SummonManager.Instance.InitSummonContent();
            Managers.AdBuffManager.Instance.InitAdBuffContent();
            Managers.PostManager.Instance.InitPostContent();
            Managers.ShopManager.Instance.InitShopContent();
            Managers.ShopPostManager.Instance.InitShopPostContent();
            Managers.ShopRandomStoreManager.Instance.InitShopRandomStoreContent();
            Managers.QuestManager.Instance.InitQuestContent();

            Managers.NoticeManager.Instance.InitNoticeContent();

            Managers.DungeonDataManager.Instance.InitDungeonDataContent();

            Managers.CreatureManager.Instance.InitCreatureContent();
            Managers.ARRRManager.Instance.InitCreatureContent();
            Managers.StaminaManager.Instance.InitStaminaRpg();
            Managers.ARRRSkillManager.Instance.InitARRRSkillContent();
            Managers.ARRRStatManager.Instance.InitARRRStatContent();
            Managers.PetManager.Instance.InitCreatureContent();
            Managers.MapManager.Instance.InitMapContent();
            Managers.GambleManager.Instance.InitGambleContent();
            Managers.SynergyManager.Instance.InitSynergyContent();
            Managers.DescendManager.Instance.InitDescendContent();
            Managers.SynergyRuneManager.Instance.InitSynergyRuneContent();
            Managers.GearManager.Instance.InitSynergyRuneContent();
            Managers.RelicManager.Instance.InitRelicContent();
            Managers.TimeAttackMissionManager.Instance.InitTimeAttackMissionContent();
            Managers.VipPackageManager.Instance.InitVipPackageContent();
            Managers.JobManager.Instance.InitJobContent();

            m_setNoticeMsg.NoticeStr = Managers.LocalStringManager.Instance.GetLocalString("common/gameLoading");

            Message.Send(m_setNoticeMsg);

            while (completeGroup == false)
                yield return null;

            SetLoadComplete();
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

        IEnumerator LocalUpdateAsyncIEnumerator()
        {
            BackEnd.Content.BackendContentTableReturnObject tableCallback = null;

            Backend.CDN.Content.Table.Get(bro =>
            {
                tableCallback = bro;
            });

            yield return new WaitUntil(() => tableCallback != null);

            if (tableCallback.IsSuccess() == false)
            {
                Debug.LogError(tableCallback);
                yield break;
            }


            BackEnd.Content.BackendContentReturnObject callback = null;

            Backend.CDN.Content.Local.Update(tableCallback.GetContentTableItemList(), GetProgress, bro => {
                callback = bro;
            });

            yield return new WaitUntil(() => callback != null);

            if (callback.IsSuccess() == false)
            {
                Debug.LogError("GetContents : Fail : " + callback);
                yield break;
            }

            Dictionary<string, BackEnd.Content.ContentItem> bro = callback.GetContentDictionarySortByChartName();

            int setcount = 0;

            foreach (var pair in bro)
            {
                JsonData data = JsonMapper.ToObject(pair.Value.contentString);
                GameBerry.TheBackEnd.TheBackEnd_GameChart.ChartBROData.Add(pair.Key, data);
                setcount++;
                if (setcount > 12)
                {
                    setcount = 0;
                    yield return null;
                }
            }

            //// 확률 파일 이름이 Content일 경우
            //// contentJson은 아래 Success Cases를 참고해주세요
            //if(dic.ContainsKey("Content")) {

            //    LitJson.JsonData json = dic["Content"].contentJson;

            //    foreach(LitJson.JsonData item in json) {
            //        Debug.Log(item["itemID"]);
            //        Debug.Log(item["itemName"]);
            //        Debug.Log(item["hpPower"]);
            //        Debug.Log(item["num"]);
            //    }
            //}
        }
    }
}