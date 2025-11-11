using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class MonsterSetCreatureData
    {
        public ObscuredInt PositionIndex;
        public ObscuredInt MonsterIndex;
        public ObscuredInt MonsterCorrectLevel;
    }

    public class MonsterSetData
    {
        public ObscuredInt Index;

        public List<MonsterSetCreatureData> MonsterSetCreatureDatas = new List<MonsterSetCreatureData>();
    }

    public class MonsterSetLocalTable : LocalTableBase
    {
        public Dictionary<ObscuredInt, MonsterSetData> MonsterSetDatas = new Dictionary<ObscuredInt, MonsterSetData>();

        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("MonsterSetTable", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                MonsterSetData monsterSetData = new MonsterSetData();

                monsterSetData.Index = rows[i]["Index"].ToString().ToInt();

                for (int j = 1; j <= 18; ++j)
                {
                    try
                    {
                        string MonsterIndex = string.Format("MonsterIndex{0}", j);
                        int monsterIndex = rows[i][MonsterIndex].ToString().ToInt();
                        if (monsterIndex == -1 || monsterIndex == 0)
                            continue;

                        string MonsterCorrectLevel = string.Format("MonsterCorrectLevel{0}", j);

                        MonsterSetCreatureData monsterSetCreatureData = new MonsterSetCreatureData();
                        monsterSetCreatureData.PositionIndex = j;
                        monsterSetCreatureData.MonsterIndex = monsterIndex;
                        monsterSetCreatureData.MonsterCorrectLevel = rows[i][MonsterCorrectLevel].ToString().ToInt();

                        monsterSetData.MonsterSetCreatureDatas.Add(monsterSetCreatureData);
                    }
                    catch
                    {

                    }
                }

                MonsterSetDatas.Add(monsterSetData.Index, monsterSetData);
            }
        }
    }
}