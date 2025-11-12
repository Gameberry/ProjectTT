using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class DungeonData
    {
        public ObscuredInt Index;
        public ObscuredInt ResourceIndex;
        public Enum_Dungeon DungeonType;

        public ObscuredInt MapResource;

        public ObscuredInt EnterCostParam1;
        public ObscuredInt EnterCostParam2;

        public V2Enum_IntervalType EnterCostRechargeIntervalType;
        public ObscuredDouble EnterCostRechargeAmount;
        public ObscuredDouble DailyAdCount;

        public string NameLocalStringKey;
        public string DescLocalStringKey;
        public string BannerIconStringKey;
    }


    public class DungeonLocalTable : LocalTableBase
    {
        public List<DungeonData> dungeonDatas = new List<DungeonData>();
        
        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("Dungeon", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                DungeonData dungeonTicketData = new DungeonData();

                dungeonTicketData.Index = rows[i]["Index"].ToString().ToInt();

                dungeonTicketData.ResourceIndex = rows[i]["ResourceIndex"].ToString().ToInt();

                if (dungeonTicketData.ResourceIndex == -1)
                    continue;

                dungeonTicketData.DungeonType = rows[i]["DungeonType"].ToString().FastStringToInt().IntToEnum32<Enum_Dungeon>();

                dungeonTicketData.MapResource = rows[i]["MapResource"].ToString().ToInt();

                dungeonTicketData.EnterCostParam1 = rows[i]["EnterCostParam1"].ToString().ToInt();
                dungeonTicketData.EnterCostParam2 = rows[i]["EnterCostParam2"].ToString().ToInt();

                dungeonTicketData.EnterCostRechargeIntervalType = rows[i]["EnterCostRechargeIntervalType"].ToString().FastStringToInt().IntToEnum32<V2Enum_IntervalType>();
                dungeonTicketData.EnterCostRechargeAmount = rows[i]["EnterCostRechargeAmount"].ToString().ToDouble();
                dungeonTicketData.DailyAdCount = rows[i]["DailyAdCount"].ToString().ToDouble();

                dungeonTicketData.NameLocalStringKey = string.Format("dungeon/{0}/name", dungeonTicketData.ResourceIndex);
                dungeonTicketData.DescLocalStringKey = string.Format("dungeon/{0}/desc", dungeonTicketData.ResourceIndex);
                dungeonTicketData.BannerIconStringKey = string.Format("dungeon/{0}/dungeonbanner", dungeonTicketData.MapResource);

                dungeonDatas.Add(dungeonTicketData);
            }
        }
        //------------------------------------------------------------------------------------
        public List<DungeonData> GetDungeonAllData()
        {
            return dungeonDatas;
        }
        //------------------------------------------------------------------------------------
        public DungeonData GetData(Enum_Dungeon enumDungeon)
        {
            return dungeonDatas.Find(x => x.DungeonType == enumDungeon);
        }
        //------------------------------------------------------------------------------------
    }
}