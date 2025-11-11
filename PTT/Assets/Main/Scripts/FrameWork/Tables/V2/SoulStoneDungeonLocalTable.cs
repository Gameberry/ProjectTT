using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using LitJson;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class SoulStoneDungeonData
    {
        public int Index;
        public int PhaseNumber;

        public V2Enum_ElementType WaveMonsterElementType;

        public List<int> WaveMonsterGroup = new List<int>();
        public List<float> WaveMonsterScale = new List<float>();

        public double SoulGauge;
        public double DamageToSoulConvertValue;

        public V2Enum_Goods FullGaugeRewardType;
        public int FullGaugeRewardParam1;
        public double FullGaugeRewardParam2;

        public List<CreatureBaseStatElement> MasteryDungeonOverrideStats = new List<CreatureBaseStatElement>();

        public V2Enum_DungeonDifficultyType DungeonDifficulty;

        public SoulStoneDungeonData PrevData;
        public SoulStoneDungeonData NextData;
    }

    public class SoulStoneDungeonLocalTable : LocalTableBase
    {
        private Dictionary<V2Enum_DungeonDifficultyType, List<SoulStoneDungeonData>> m_soulStoneDungeonDatas_Dic = new Dictionary<V2Enum_DungeonDifficultyType, List<SoulStoneDungeonData>>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("SoulStoneDungeon", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                SoulStoneDungeonData soulStoneDungeonData = new SoulStoneDungeonData();

                soulStoneDungeonData.Index = rows[i]["Index"].ToString().FastStringToInt();
                soulStoneDungeonData.PhaseNumber = rows[i]["PhaseNumber"].ToString().FastStringToInt();

                soulStoneDungeonData.WaveMonsterElementType = rows[i]["WaveMonsterElementType"].ToString().FastStringToInt().IntToEnum32<V2Enum_ElementType>();

                soulStoneDungeonData.WaveMonsterGroup.Add(106010189);
                soulStoneDungeonData.WaveMonsterScale.Add(2.85f);

                soulStoneDungeonData.SoulGauge = rows[i]["SoulGauge"].ToString().ToDouble();
                soulStoneDungeonData.DamageToSoulConvertValue = rows[i]["DamageToSoulConvertValue"].ToString().ToDouble();

                soulStoneDungeonData.FullGaugeRewardType = rows[i]["FullGaugeRewardType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                soulStoneDungeonData.FullGaugeRewardParam1 = rows[i]["FullGaugeRewardParam1"].ToString().ToInt();
                soulStoneDungeonData.FullGaugeRewardParam2 = rows[i]["FullGaugeRewardParam2"].ToString().ToDouble();

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

                        soulStoneDungeonData.MasteryDungeonOverrideStats.Add(creatureBaseStatElement);
                    }
                    catch
                    {

                    }
                }

                try
                {
                    soulStoneDungeonData.DungeonDifficulty = rows[i]["DungeonDifficulty"].ToString().ToInt().IntToEnum32<V2Enum_DungeonDifficultyType>();
                }
                catch
                {
                }

                if (m_soulStoneDungeonDatas_Dic.ContainsKey(soulStoneDungeonData.DungeonDifficulty) == false)
                    m_soulStoneDungeonDatas_Dic.Add(soulStoneDungeonData.DungeonDifficulty, new List<SoulStoneDungeonData>());

                m_soulStoneDungeonDatas_Dic[soulStoneDungeonData.DungeonDifficulty].Add(soulStoneDungeonData);
            }

            foreach (var pair in m_soulStoneDungeonDatas_Dic)
            {
                List<SoulStoneDungeonData> m_soulStoneDungeonDatas = pair.Value;

                for (int i = 0; i < m_soulStoneDungeonDatas.Count; ++i)
                {
                    SoulStoneDungeonData soulStoneDungeonData = m_soulStoneDungeonDatas[i];

                    if (i != 0)
                    {
                        soulStoneDungeonData.PrevData = m_soulStoneDungeonDatas[i - 1];
                    }

                    if (i != m_soulStoneDungeonDatas.Count - 1)
                    {
                        soulStoneDungeonData.NextData = m_soulStoneDungeonDatas[i + 1];
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public List<SoulStoneDungeonData> GetSoulStoneDungeonData(V2Enum_DungeonDifficultyType v2Enum_DungonDifficultyType)
        {
            if (m_soulStoneDungeonDatas_Dic.ContainsKey(v2Enum_DungonDifficultyType) == false)
                return null;

            return m_soulStoneDungeonDatas_Dic[v2Enum_DungonDifficultyType];
        }
        //------------------------------------------------------------------------------------
    }
}