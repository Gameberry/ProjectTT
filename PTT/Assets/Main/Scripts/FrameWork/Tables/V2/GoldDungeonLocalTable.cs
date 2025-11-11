using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using LitJson;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class GoldDungeonData : DungeonModeBase
    {
        public List<int> WaveMonsterGroup = new List<int>();
        public List<float> WaveMonsterScale = new List<float>();
        public int MaxWaveCount;

        public List<CreatureBaseStatElement> GoldDungeonOverrideStats = new List<CreatureBaseStatElement>();
    }

    public class GoldDungeonLocalTable : LocalTableBase
    {
        private Dictionary<V2Enum_DungeonDifficultyType, Dictionary<int, GoldDungeonData>> m_goldDungeonDatas_Dic = new Dictionary<V2Enum_DungeonDifficultyType, Dictionary<int, GoldDungeonData>>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            Dictionary<V2Enum_DungeonDifficultyType, List<GoldDungeonData>> goldDungeonDatas_Dic = new Dictionary<V2Enum_DungeonDifficultyType, List<GoldDungeonData>>();

            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("GoldDungeon", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                GoldDungeonData goldDungeonData = new GoldDungeonData();

                goldDungeonData.Index = rows[i]["Index"].ToString().FastStringToInt();
                goldDungeonData.DungeonNumber = rows[i]["DungeonNumber"].ToString().FastStringToInt();


                goldDungeonData.WaveMonsterGroup.Add(106010151);
                goldDungeonData.WaveMonsterScale.Add(1.35f);
                goldDungeonData.MaxWaveCount = 1;


                if (goldDungeonData.WaveMonsterGroup.Count != goldDungeonData.MaxWaveCount)
                {
                    if (goldDungeonData.WaveMonsterGroup.Count < goldDungeonData.MaxWaveCount)
                        goldDungeonData.MaxWaveCount = goldDungeonData.WaveMonsterGroup.Count;
                    else
                    {
                        goldDungeonData.WaveMonsterGroup.RemoveRange(goldDungeonData.MaxWaveCount, goldDungeonData.WaveMonsterGroup.Count - goldDungeonData.MaxWaveCount);
                    }
                }

                for (int j = 1; j <= 5; ++j)
                {
                    try
                    {
                        int overridestat = rows[i][string.Format("OverrideStatType{0}", j)].ToString().ToInt();

                        if (overridestat == -1 || overridestat == 0)
                            continue;

                        CreatureBaseStatElement creatureBaseStatElement = new CreatureBaseStatElement();
                        creatureBaseStatElement.BaseStat = overridestat.IntToEnum32<V2Enum_Stat>();
                        creatureBaseStatElement.BaseValue = rows[i][string.Format("OverrideStatBaseValue{0}", j)].ToString().ToDouble();

                        goldDungeonData.GoldDungeonOverrideStats.Add(creatureBaseStatElement);
                    }
                    catch
                    {

                    }
                }

                goldDungeonData.ClearRewardParam1 = V2Enum_Point.InGameGold.Enum32ToInt();
                goldDungeonData.ClearRewardParam2 = rows[i]["ClearRewardParam2"].ToString().ToDouble();

                try
                {
                    goldDungeonData.DungeonDifficulty = rows[i]["DungeonDifficulty"].ToString().ToInt().IntToEnum32<V2Enum_DungeonDifficultyType>();
                }
                catch
                {
                }

                if (goldDungeonDatas_Dic.ContainsKey(goldDungeonData.DungeonDifficulty) == false)
                    goldDungeonDatas_Dic.Add(goldDungeonData.DungeonDifficulty, new List<GoldDungeonData>());

                goldDungeonDatas_Dic[goldDungeonData.DungeonDifficulty].Add(goldDungeonData);


                if (m_goldDungeonDatas_Dic.ContainsKey(goldDungeonData.DungeonDifficulty) == false)
                    m_goldDungeonDatas_Dic.Add(goldDungeonData.DungeonDifficulty, new Dictionary<int, GoldDungeonData>());

                m_goldDungeonDatas_Dic[goldDungeonData.DungeonDifficulty].Add(goldDungeonData.DungeonNumber, goldDungeonData);
            }

            foreach (var pair in goldDungeonDatas_Dic)
            {
                List<GoldDungeonData> m_goldDungeonDatas = pair.Value;

                for (int i = 0; i < m_goldDungeonDatas.Count; ++i)
                {
                    GoldDungeonData goldDungeonData = m_goldDungeonDatas[i];

                    if (i != 0)
                    {
                        goldDungeonData.PrevData = m_goldDungeonDatas[i - 1];
                    }

                    if (i != m_goldDungeonDatas.Count - 1)
                    {
                        goldDungeonData.NextData = m_goldDungeonDatas[i + 1];
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public GoldDungeonData GetData(V2Enum_DungeonDifficultyType v2Enum_DungonDifficultyType, int dungeonNumber)
        {
            if (m_goldDungeonDatas_Dic.ContainsKey(v2Enum_DungonDifficultyType) == false)
                return null;

            if (m_goldDungeonDatas_Dic[v2Enum_DungonDifficultyType].ContainsKey(dungeonNumber) == true)
                return m_goldDungeonDatas_Dic[v2Enum_DungonDifficultyType][dungeonNumber];

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}
