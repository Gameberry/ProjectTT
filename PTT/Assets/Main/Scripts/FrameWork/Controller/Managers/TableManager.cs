//#define LOAD_FROM_ASSETBUNDLE

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;

namespace GameBerry.Managers
{
    public class TableManager : MonoSingleton<TableManager>
    {
        bool _alreadyLoading = false;
        bool _loadComplete = false;
        string basePath = "Tables/";

        readonly Dictionary<System.Type, object> _tables = new Dictionary<System.Type, object>();

        ConcurrentQueue<LocalTableBase> localTableBases = new ConcurrentQueue<LocalTableBase>();
        List<UniTask> loadingTasks = new List<UniTask>();

        Event.SetNoticeMsg m_setNoticeMsg = new Event.SetNoticeMsg();
        string noticestring = string.Empty;

        public T GetTableClass<T>() where T : class
        {
            object table;
            if (_tables.TryGetValue(typeof(T), out table))
                return (T)table;

            Debug.LogErrorFormat("{0} is null", typeof(T).Name);
            return null;
        }

        int totalCount = 0;
        int completeCount = 0;
        int currentCompleteCount = 0;

        public IEnumerator Load()
        {
            //_tables.Clear();

            if (_loadComplete)
                yield break;

            if (_alreadyLoading)
            {
                while (!_loadComplete)
                    yield return null;

                yield break;
            }

            noticestring = Managers.LocalStringManager.Instance.GetLocalString("title/data");



            AddTable(new CharacterLocalTable());

            AddTable(new DefineLocalTable());
            AddTable(new GambleLocalTable());
            AddTable(new SynergyLocalTable());
            AddTable(new DescendLocalTable());
            AddTable(new RelicLocalTable());

            AddTable(new MapLocalTable());
            AddTable(new CreatureLocalTable());
            AddTable(new MonsterSetLocalTable());
            AddTable(new PetLocalTable());
            AddTable(new SkillLocalTable());
            AddTable(new StackableLocalTable());

            AddTable(new SummonLocalTable());

            AddTable(new QuestLocalTable());
            AddTable(new TimeAttackMissionLocalTable());

            AddTable(new ContentUnlockLocalTable());

            AddTable(new PassLocalTable());
            AddTable(new ShopLocalTable());
            AddTable(new ShopRandomStoreLocalTable());

            AddTable(new AdBuffLocalTable());
            AddTable(new VipPackageLocalTable());

            AddTable(new SynergyRuneLocalTable());
            AddTable(new ResearchLocalTable());
            AddTable(new GearLocalTable());
            AddTable(new JobLocalTable());

            //AddTable(new GuideQuestLocalTable());
            //AddTable(new StageAutoGenLocalTable());
            //AddTable(new CharacterSkillLocalTable());

            //AddTable(new MonsterSkillLocalTable());
            //AddTable(new CharacterGearLocalTable());
            //AddTable(new CharacterSkinLocalTable());
            //AddTable(new CharacterBaseTrainingLocalTable());

            //AddTable(new ExchangeLocalTable());
            //AddTable(new DevilCastleLocalTable());
            //AddTable(new TrialTowerLocalTable());

            //AddTable(new TraitLocalTable());
            AddTable(new DungeonLocalTable());


            AddTable(new DiamondDungeonLocalTable());
            //AddTable(new MasteryDungeonLocalTable());
            //AddTable(new GoldDungeonLocalTable());
            //AddTable(new SoulStoneDungeonLocalTable());
            //AddTable(new HellDungeonLocalTable());
            //AddTable(new FameLocalTable());
            //AddTable(new BuffLocalTable());

            //AddTable(new CheckInLocalTable());


            //AddTable(new StageLocalTable());

            //AddTable(new EventSchedulerLocalTable());

            //AddTable(new RotationEventLocalTable());

            //AddTable(new EventRouletteLocalTable());
            //AddTable(new EventCrackDungeonLocalTable());
            //AddTable(new EventGoddessDungeonLocalTable());
            //AddTable(new EventRedBullLocalTable());

            //AddTable(new EventPassLocalTable());
            //AddTable(new LocalOverrideLocalTable());

            //AddTable(new SevenDayLocalTable());
            //AddTable(new BackLightLocalTable());

            totalCount = localTableBases.Count + loadingTasks.Count;
            completeCount = 0;


            ShowTableLoadState(completeCount, totalCount);





            yield return new WaitForSeconds(0.3f);

            int halfCount = localTableBases.Count / 2;


            //LoadAllTable().Forget();
            LoadAllTableTable().Forget();

            while (completeCount < totalCount)
                yield return null;

            ShowTableLoadState(completeCount, totalCount);

            _alreadyLoading = false;
            _loadComplete = true;
        }

        private async UniTask LoadAllTableTable()
        {
            while (localTableBases.Count > 0)
            {
                LocalTableBase localTableBase = null;

                if (localTableBases.TryDequeue(out localTableBase) == false)
                    continue;

                localTableBase.InitData_Async().ContinueWith(() =>
                {
                    completeCount++;

                    currentCompleteCount--;

                    //UnityEngine.Debug.LogError("UniComplete");
                    ShowTableLoadState(completeCount, totalCount);
                }).Forget();

                //Debug.Log(string.Format("로드 남은 카운트 : {0}", localTableBases.Count));

                await UniTask.Yield();
            }

            ShowTableLoadState(completeCount, totalCount);
        }

        private async UniTask LoadAllTable()
        {
            while (localTableBases.Count > 0)
            {
                LocalTableBase localTableBase = null;

                if (localTableBases.TryDequeue(out localTableBase) == false)
                    continue;

                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                await localTableBase.InitData_Async();
                completeCount++;
                ShowTableLoadState(completeCount, totalCount);

                stopwatch.Stop();

                if (stopwatch.ElapsedMilliseconds > 500)
                    UnityEngine.Debug.LogError("time : " + stopwatch.ElapsedMilliseconds + "ms  " + localTableBase.GetType());
            }

            ShowTableLoadState(completeCount, totalCount);
        }

        private async UniTask StartLoading(int startidx, int endidx, System.Action oncomplete)
        {
            //await UniTask.WhenAll(loadingTasks);

            while (localTableBases.Count > 0)
            {
                LocalTableBase localTableBase = null;

                if (localTableBases.TryDequeue(out localTableBase) == false)
                    continue;

                //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                //stopwatch.Start();

                await localTableBase.InitData_Async();
                completeCount++;
                ShowTableLoadState(completeCount, totalCount);

                //stopwatch.Stop();
                //UnityEngine.Debug.LogError("time : " + stopwatch.ElapsedMilliseconds + "ms  " + localTableBase.GetType());
            }

            //for (int i = startidx; i < endidx; ++i)
            //{
            //    await localTableBases[i].InitData_Async();
            //    completeCount++;
            //    ShowTableLoadState(completeCount, totalCount);
            //}

            //foreach (var task in localTableBases)
            //{
            //    await task.InitData_Async();
            //    completeCount++;
            //    ShowTableLoadState(completeCount, totalCount);
            //}

            //foreach (var task in loadingTasks)
            //{
            //    await task;
            //}

            Debug.Log(string.Format("Complete {0}~{1}", startidx, endidx));

            ShowTableLoadState(completeCount, totalCount);
            //UnityEngine.Debug.LogError("UniALLLLComplete");
            oncomplete?.Invoke();
        }

        private void AddUniTaskLoadTable(LocalTableBase localTableBase)
        {
            loadingTasks.Add(localTableBase.InitData_Async().ContinueWith(() =>
            {
                completeCount++;

                currentCompleteCount--;

                //UnityEngine.Debug.LogError("UniComplete");
                ShowTableLoadState(completeCount, totalCount);
            }));

            //string tableName = localTableBase.GetType().ToString();
            //_tables.Add(Type.GetType(tableName), localTableBase);
        }

        public void AddTable(LocalTableBase localTableBase)
        {
            //AddUniTaskLoadTable(localTableBase);
            localTableBases.Enqueue(localTableBase);

            AddTableClass(localTableBase);
        }

        public void AddTableClass(LocalTableBase localTableBase)
        {
            string tableName = localTableBase.GetType().ToString();
            _tables.Add(Type.GetType(tableName), localTableBase);
        }

        private void ShowTableLoadState(int completeCount, int totalCount, string addstr = "")
        {
            if (addstr == "")
            {
                m_setNoticeMsg.NoticeStr = string.Format("{0} {1:0}%({2}/{3})", noticestring, ((float)completeCount / (float)totalCount) * 100.0f, completeCount, totalCount);
            }
            else
            {
                m_setNoticeMsg.NoticeStr = string.Format("{0}\n{1} {2:0}%", addstr, noticestring, ((float)completeCount / (float)totalCount) * 100.0f);
            }
            Message.Send(m_setNoticeMsg);
        }

        public void Clear()
        {
            _tables.Clear();
            Debug.Log("Clear Tables. - " + GetInstanceID());
        }

        public async UniTask LoadDeviceLocalString(System.Action complete)
        {
            StringLocalChart stringLocalChart = new StringLocalChart();
            _tables.Add(Type.GetType("GameBerry.StringLocalChart"), stringLocalChart);
            await stringLocalChart.InitData_Async();

            complete?.Invoke();
        }


        protected override void Release()
        {
            Clear();
        }
    }
}
