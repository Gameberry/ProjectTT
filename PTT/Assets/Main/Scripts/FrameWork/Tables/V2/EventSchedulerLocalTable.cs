using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class EventSchedulerData
    {
        public ObscuredInt Index;

        public ObscuredString ServerKind;
        public ObscuredInt ServerNumber;

        public V2Enum_EventKindType EventKind;

        public ObscuredInt EventVersion;

        public ObscuredString EventStartTime;
        public ObscuredString EventEndTime;

        public ObscuredLong DisplayDelaytime;
    }

    public class EventSchedulerLocalTable : LocalTableBase
    {
        public JsonData eventSchedulerDatas = null;

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("EventScheduler", o =>
            { eventSchedulerDatas = o; });

            await UniTask.WaitUntil(() => eventSchedulerDatas != null);

            await SetEventScheduler();
        }
        //------------------------------------------------------------------------------------
        public async UniTask SetEventScheduler()
        {
            if (eventSchedulerDatas == null)
                return;

            for (int i = 0; i < eventSchedulerDatas.Count; ++i)
            {
                string ServerKind = eventSchedulerDatas[i]["ServerKind"].ToString();
                if (PlayerDataContainer.PlayerServerKind != ServerKind)
                    continue;

                int ServerNumber = eventSchedulerDatas[i]["ServerNumber"].ToString().ToInt();
                if (PlayerDataContainer.PlayerServerNum != ServerNumber)
                    continue;

                V2Enum_EventKindType EventKind = eventSchedulerDatas[i]["EventKind"].ToString().ToInt().IntToEnum32<V2Enum_EventKindType>();

                int EventVersion = eventSchedulerDatas[i]["EventVersion"].ToString().ToInt();

                string EventStartTime = eventSchedulerDatas[i]["EventStartTime"].ToString();
                string EventEndTime = eventSchedulerDatas[i]["EventEndTime"].ToString();

                long DisplayDelaytime = eventSchedulerDatas[i]["DisplayDelaytime"].ToString().ToLong();

                if (EventKind == V2Enum_EventKindType.EventRoulette)
                {
                    Define.EventNumber = EventVersion;

                    {
                        System.DateTime dateTime = System.DateTime.Parse(EventStartTime).ToUniversalTime();
                        UnityEngine.Debug.Log("EventStart : " + EventStartTime + dateTime.ToString());
                        Define.EventRouletteStartTime = ((System.DateTimeOffset)dateTime).ToUnixTimeSeconds();
                    }

                    {
                        System.DateTime dateTime = System.DateTime.Parse(EventEndTime).ToUniversalTime();
                        UnityEngine.Debug.Log("EventEnd : " + EventEndTime + dateTime.ToString());
                        Define.EventRouletteEndTime = ((System.DateTimeOffset)dateTime).ToUnixTimeSeconds();
                        Define.EventRouletteDisPlaySecond = DisplayDelaytime;
                        Define.EventRouletteDisPlayTime = Define.EventRouletteEndTime + Define.EventRouletteDisPlaySecond;

                        if (Define.EventRouletteDisPlayTime > Managers.TimeManager.Instance.Current_TimeStamp)
                        {
                            EventRouletteLocalTable eventRouletteLocalTable = new EventRouletteLocalTable();
                            await eventRouletteLocalTable.InitData_Async();

                            Managers.TableManager.Instance.AddTableClass(eventRouletteLocalTable);
                        }
                    }
                }
                else if (EventKind == V2Enum_EventKindType.EventDungeon)
                {
                    Define.EventDungeonNumber = EventVersion;

                    {
                        System.DateTime dateTime = System.DateTime.Parse(EventStartTime).ToUniversalTime();
                        UnityEngine.Debug.Log("EventDungeonStart : " + EventStartTime + dateTime.ToString());
                        Define.EventDungeonStartTime = ((System.DateTimeOffset)dateTime).ToUnixTimeSeconds();
                    }

                    {
                        System.DateTime dateTime = System.DateTime.Parse(EventEndTime).ToUniversalTime();
                        UnityEngine.Debug.Log("EventDungeonEnd : " + EventEndTime + dateTime.ToString());
                        Define.EventDungeonEndTime = ((System.DateTimeOffset)dateTime).ToUnixTimeSeconds();
                        Define.EventDungeonDisPlaySecond = DisplayDelaytime;
                        Define.EventDungeonDisPlayTime = Define.EventDungeonEndTime + Define.EventDungeonDisPlaySecond;

                        if (Define.EventDungeonDisPlayTime > Managers.TimeManager.Instance.Current_TimeStamp)
                        {
                            EventCrackDungeonLocalTable eventCrackDungeonLocalTable = new EventCrackDungeonLocalTable();
                            await eventCrackDungeonLocalTable.InitData_Async();

                            Managers.TableManager.Instance.AddTableClass(eventCrackDungeonLocalTable);
                        }
                    }
                }
                else if (EventKind == V2Enum_EventKindType.EventHealDungeon)
                {
                    Define.EventHealDungeonNumber = EventVersion;

                    {
                        System.DateTime dateTime = System.DateTime.Parse(EventStartTime).ToUniversalTime();
                        UnityEngine.Debug.Log("EventHealDungeonStart : " + EventStartTime + dateTime.ToString());
                        Define.EventHealDungeonStartTime = ((System.DateTimeOffset)dateTime).ToUnixTimeSeconds();
                    }

                    {
                        System.DateTime dateTime = System.DateTime.Parse(EventEndTime).ToUniversalTime();
                        UnityEngine.Debug.Log("EventHealDungeonEnd : " + EventEndTime + dateTime.ToString());
                        Define.EventHealDungeonEndTime = ((System.DateTimeOffset)dateTime).ToUnixTimeSeconds();
                        Define.EventHealDungeonDisPlaySecond = DisplayDelaytime;
                        Define.EventHealDungeonDisPlayTime = Define.EventHealDungeonEndTime + Define.EventHealDungeonDisPlaySecond;

                        if (Define.EventHealDungeonDisPlayTime > Managers.TimeManager.Instance.Current_TimeStamp)
                        {
                            EventGoddessDungeonLocalTable eventGoddessDungeonLocalTable = new EventGoddessDungeonLocalTable();
                            await eventGoddessDungeonLocalTable.InitData_Async();

                            Managers.TableManager.Instance.AddTableClass(eventGoddessDungeonLocalTable);
                        }
                    }
                }
                else if (EventKind == V2Enum_EventKindType.EventRedBullDungeon)
                {
                    Define.EventRedBullNumber = EventVersion;

                    {
                        System.DateTime dateTime = System.DateTime.Parse(EventStartTime).ToUniversalTime();
                        UnityEngine.Debug.Log("EventRedBullStart : " + EventStartTime + dateTime.ToString());
                        Define.EventRedBullStartTime = ((System.DateTimeOffset)dateTime).ToUnixTimeSeconds();
                    }

                    {
                        System.DateTime dateTime = System.DateTime.Parse(EventEndTime).ToUniversalTime();
                        UnityEngine.Debug.Log("EventRedBullEnd : " + EventEndTime + dateTime.ToString());
                        Define.EventRedBullEndTime = ((System.DateTimeOffset)dateTime).ToUnixTimeSeconds();
                        Define.EventRedBullDisPlaySecond = DisplayDelaytime;
                        Define.EventRedBullDisPlayTime = Define.EventRedBullEndTime + Define.EventRedBullDisPlaySecond;

                        if (Define.EventRedBullDisPlayTime > Managers.TimeManager.Instance.Current_TimeStamp)
                        {
                            EventRedBullLocalTable eventRedBullLocalTable = new EventRedBullLocalTable();
                            await eventRedBullLocalTable.InitData_Async();

                            Managers.TableManager.Instance.AddTableClass(eventRedBullLocalTable);
                        }
                    }
                }
                else if (EventKind == V2Enum_EventKindType.EventUrsulaDungeon)
                {
                    Define.EventUrsulaNumber = EventVersion;

                    {
                        System.DateTime dateTime = System.DateTime.Parse(EventStartTime).ToUniversalTime();
                        UnityEngine.Debug.Log("EventUrsulaStart : " + EventStartTime + dateTime.ToString());
                        Define.EventUrsulaStartTime = ((System.DateTimeOffset)dateTime).ToUnixTimeSeconds();
                    }

                    {
                        System.DateTime dateTime = System.DateTime.Parse(EventEndTime).ToUniversalTime();
                        UnityEngine.Debug.Log("EventUrsulaEnd : " + EventEndTime + dateTime.ToString());
                        Define.EventUrsulaEndTime = ((System.DateTimeOffset)dateTime).ToUnixTimeSeconds();
                        Define.EventUrsulaDisPlaySecond = DisplayDelaytime;
                        Define.EventUrsulaDisPlayTime = Define.EventUrsulaEndTime + Define.EventUrsulaDisPlaySecond;

                        if (Define.EventUrsulaDisPlayTime > Managers.TimeManager.Instance.Current_TimeStamp)
                        {
                            EventUrsulaLocalTable eventUrsulaLocalTable = new EventUrsulaLocalTable();
                            await eventUrsulaLocalTable.InitData_Async();

                            Managers.TableManager.Instance.AddTableClass(eventUrsulaLocalTable);
                        }
                    }
                }
                else if (EventKind == V2Enum_EventKindType.EventDig)
                {
                    Define.EventDigNumber = EventVersion;

                    {
                        System.DateTime dateTime = System.DateTime.Parse(EventStartTime).ToUniversalTime();
                        UnityEngine.Debug.Log("EventDigStart : " + EventStartTime + dateTime.ToString());
                        Define.EventDigStartTime = ((System.DateTimeOffset)dateTime).ToUnixTimeSeconds();
                    }

                    {
                        System.DateTime dateTime = System.DateTime.Parse(EventEndTime).ToUniversalTime();
                        UnityEngine.Debug.Log("EventDigEnd : " + EventEndTime + dateTime.ToString());
                        Define.EventDigEndTime = ((System.DateTimeOffset)dateTime).ToUnixTimeSeconds();
                        Define.EventDigDisPlaySecond = DisplayDelaytime;
                        Define.EventDigDisPlayTime = Define.EventDigEndTime + Define.EventDigDisPlaySecond;

                        if (Define.EventDigDisPlayTime > Managers.TimeManager.Instance.Current_TimeStamp)
                        {
                            EventDigLocalTable eventDigLocalTable = new EventDigLocalTable();
                            await eventDigLocalTable.InitData_Async();

                            Managers.TableManager.Instance.AddTableClass(eventDigLocalTable);
                        }
                    }
                }
                else if (EventKind == V2Enum_EventKindType.EventMathRpg)
                {
                    Define.EventMathRpgNumber = EventVersion;

                    {
                        System.DateTime dateTime = System.DateTime.Parse(EventStartTime).ToUniversalTime();
                        UnityEngine.Debug.Log("EventMathRpgStart : " + EventStartTime + dateTime.ToString());
                        Define.EventMathRpgStartTime = ((System.DateTimeOffset)dateTime).ToUnixTimeSeconds();
                    }

                    {
                        System.DateTime dateTime = System.DateTime.Parse(EventEndTime).ToUniversalTime();
                        UnityEngine.Debug.Log("EventMathRpgEnd : " + EventEndTime + dateTime.ToString());
                        Define.EventMathRpgEndTime = ((System.DateTimeOffset)dateTime).ToUnixTimeSeconds();
                        Define.EventMathRpgDisPlaySecond = DisplayDelaytime;
                        Define.EventMathRpgDisPlayTime = Define.EventMathRpgEndTime + Define.EventMathRpgDisPlaySecond;

                        if (Define.EventMathRpgDisPlayTime > Managers.TimeManager.Instance.Current_TimeStamp)
                        {
                            EventMathRpgLocalTable eventMathRpgLocalTable = new EventMathRpgLocalTable();
                            await eventMathRpgLocalTable.InitData_Async();

                            Managers.TableManager.Instance.AddTableClass(eventMathRpgLocalTable);
                        }
                    }
                }
                else if (EventKind == V2Enum_EventKindType.EventKingSlime)
                {
                    Define.EventKingSlimeDungeonNumber = EventVersion;

                    {
                        System.DateTime dateTime = System.DateTime.Parse(EventStartTime).ToUniversalTime();
                        UnityEngine.Debug.Log("EventKingSlimeDungeonStart : " + EventStartTime + dateTime.ToString());
                        Define.EventKingSlimeDungeonStartTime = ((System.DateTimeOffset)dateTime).ToUnixTimeSeconds();
                    }

                    {
                        System.DateTime dateTime = System.DateTime.Parse(EventEndTime).ToUniversalTime();
                        UnityEngine.Debug.Log("EventKingSlimeDungeonEnd : " + EventEndTime + dateTime.ToString());
                        Define.EventKingSlimeDungeonEndTime = ((System.DateTimeOffset)dateTime).ToUnixTimeSeconds();
                        Define.EventKingSlimeDungeonDisPlaySecond = DisplayDelaytime;
                        Define.EventKingSlimeDungeonDisPlayTime = Define.EventKingSlimeDungeonEndTime + Define.EventKingSlimeDungeonDisPlaySecond;

                        if (Define.EventKingSlimeDungeonDisPlayTime > Managers.TimeManager.Instance.Current_TimeStamp)
                        {
                            EventKingSlimeLocalTable eventKingSlimeDungeonLocalTable = new EventKingSlimeLocalTable();
                            await eventKingSlimeDungeonLocalTable.InitData_Async();

                            Managers.TableManager.Instance.AddTableClass(eventKingSlimeDungeonLocalTable);
                        }
                    }
                }

            }
        }
        //------------------------------------------------------------------------------------
    }
}