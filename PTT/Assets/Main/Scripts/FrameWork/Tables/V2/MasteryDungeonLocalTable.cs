using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using LitJson;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class MasteryDungeonData
    {
        public int Index;
        public int PhaseNumber;

        public V2Enum_ElementType WaveMonsterElementType;

        public List<int> WaveMonsterGroup = new List<int>();
        public List<float> WaveMonsterScale = new List<float>();

        public double HP;

        public List<CreatureBaseStatElement> MasteryDungeonOverrideStats = new List<CreatureBaseStatElement>();

        public V2Enum_DungeonDifficultyType DungeonDifficulty;

        public MasteryDungeonData PrevData;
        public MasteryDungeonData NextData;
    }

    public class MasteryDungeonRewardData
    {
        public int Index;

        public double AccumedDamageMin;
        public double AccumedDamageMax;

        public V2Enum_Goods PointRewardType;
        public int PointRewardParam1;
        public double PointRewardParam2;

        public V2Enum_DungeonDifficultyType DungeonDifficulty;

        public WeightedRandomPicker<V2Enum_Goods> GoodsTypeRandomPicker = new WeightedRandomPicker<V2Enum_Goods>();

        public int ItemRewardSelectCount;

        public WeightedRandomPicker<V2Enum_Grade> GradeTypeRandomPicker = new WeightedRandomPicker<V2Enum_Grade>();
    }

    public class MasteryDungeonLocalTable : LocalTableBase
    {
        private Dictionary<V2Enum_DungeonDifficultyType, List<MasteryDungeonData>> m_masteryDungeonDatas_Dic = new Dictionary<V2Enum_DungeonDifficultyType, List<MasteryDungeonData>>();

        private Dictionary<V2Enum_DungeonDifficultyType, List<MasteryDungeonRewardData>> m_masteryDungeonRewardDatas_Dic = new Dictionary<V2Enum_DungeonDifficultyType, List<MasteryDungeonRewardData>>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("MasteryDungeon", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                MasteryDungeonData masteryDungeonData = new MasteryDungeonData();

                masteryDungeonData.Index = rows[i]["Index"].ToString().FastStringToInt();
                masteryDungeonData.PhaseNumber = rows[i]["PhaseNumber"].ToString().FastStringToInt();

                masteryDungeonData.WaveMonsterElementType = rows[i]["WaveMonsterElementType"].ToString().FastStringToInt().IntToEnum32<V2Enum_ElementType>();

                masteryDungeonData.WaveMonsterGroup.Add(106010187);
                masteryDungeonData.WaveMonsterScale.Add(2.22f);

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

                        if (creatureBaseStatElement.BaseStat == V2Enum_Stat.HP)
                            masteryDungeonData.HP = creatureBaseStatElement.BaseValue;

                        masteryDungeonData.MasteryDungeonOverrideStats.Add(creatureBaseStatElement);
                    }
                    catch
                    {

                    }
                }

                try
                {
                    masteryDungeonData.DungeonDifficulty = rows[i]["DungeonDifficulty"].ToString().ToInt().IntToEnum32<V2Enum_DungeonDifficultyType>();
                }
                catch
                {
                }

                if (m_masteryDungeonDatas_Dic.ContainsKey(masteryDungeonData.DungeonDifficulty) == false)
                    m_masteryDungeonDatas_Dic.Add(masteryDungeonData.DungeonDifficulty, new List<MasteryDungeonData>());

                m_masteryDungeonDatas_Dic[masteryDungeonData.DungeonDifficulty].Add(masteryDungeonData);
            }

            foreach (var pair in m_masteryDungeonDatas_Dic)
            {
                List<MasteryDungeonData> m_masteryDungeonDatas = pair.Value;

                for (int i = 0; i < m_masteryDungeonDatas.Count; ++i)
                {
                    MasteryDungeonData masteryDungeonData = m_masteryDungeonDatas[i];

                    if (i != 0)
                    {
                        masteryDungeonData.PrevData = m_masteryDungeonDatas[i - 1];
                    }

                    if (i != m_masteryDungeonDatas.Count - 1)
                    {
                        masteryDungeonData.NextData = m_masteryDungeonDatas[i + 1];
                    }
                }
            }

            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("MasteryDungeonReward", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                MasteryDungeonRewardData masteryDungeonRewardData = new MasteryDungeonRewardData();

                masteryDungeonRewardData.Index = rows[i]["Index"].ToString().FastStringToInt();
                masteryDungeonRewardData.AccumedDamageMin = rows[i]["AccumedDamageMin"].ToString().ToDouble();
                masteryDungeonRewardData.AccumedDamageMax = rows[i]["AccumedDamageMax"].ToString().ToDouble();

                masteryDungeonRewardData.PointRewardType = rows[i]["PointRewardType"].ToString().FastStringToInt().IntToEnum32<V2Enum_Goods>();
                masteryDungeonRewardData.PointRewardParam1 = rows[i]["PointRewardParam1"].ToString().FastStringToInt();
                masteryDungeonRewardData.PointRewardParam2 = rows[i]["PointRewardParam2"].ToString().ToDouble();

                masteryDungeonRewardData.ItemRewardSelectCount = rows[i]["ItemRewardSelectCount"].ToString().FastStringToInt();

                for (int j = 1; j <= 5; ++j)
                {
                    try
                    {
                        int goodstype = rows[i][string.Format("ItemRewardType{0}", j)].ToString().ToInt();
                        if (goodstype != -1 && goodstype != 0)
                        {
                            V2Enum_Goods ItemRewardType = goodstype.IntToEnum32<V2Enum_Goods>();
                            masteryDungeonRewardData.GoodsTypeRandomPicker.Add(ItemRewardType, 100.0);
                        }
                    }
                    catch
                    {

                    }

                    try
                    {
                        int gradetype = rows[i][string.Format("ItemRewardGrade{0}", j)].ToString().ToInt();
                        if (gradetype != -1 && gradetype != 0)
                        {
                            V2Enum_Grade ItemRewardGrade = gradetype.IntToEnum32<V2Enum_Grade>();
                            double ItemRewardWeight = rows[i][string.Format("ItemRewardWeight{0}", j)].ToString().ToDouble();
                            masteryDungeonRewardData.GradeTypeRandomPicker.Add(ItemRewardGrade, ItemRewardWeight);
                        }
                    }
                    catch
                    {

                    }
                }

                try
                {
                    masteryDungeonRewardData.DungeonDifficulty = rows[i]["DungeonDifficulty"].ToString().ToInt().IntToEnum32<V2Enum_DungeonDifficultyType>();
                }
                catch
                {
                }

                if (m_masteryDungeonRewardDatas_Dic.ContainsKey(masteryDungeonRewardData.DungeonDifficulty) == false)
                    m_masteryDungeonRewardDatas_Dic.Add(masteryDungeonRewardData.DungeonDifficulty, new List<MasteryDungeonRewardData>());

                m_masteryDungeonRewardDatas_Dic[masteryDungeonRewardData.DungeonDifficulty].Add(masteryDungeonRewardData);
            }
        }
        //------------------------------------------------------------------------------------
        public List<MasteryDungeonData> GetMasteryDungeonData(V2Enum_DungeonDifficultyType v2Enum_DungonDifficultyType)
        {
            if (m_masteryDungeonDatas_Dic.ContainsKey(v2Enum_DungonDifficultyType) == false)
                return null;

            return m_masteryDungeonDatas_Dic[v2Enum_DungonDifficultyType];
        }
        //------------------------------------------------------------------------------------
        public List<MasteryDungeonRewardData> GetMasteryDungeonRewardData(V2Enum_DungeonDifficultyType v2Enum_DungonDifficultyType)
        {
            if (m_masteryDungeonRewardDatas_Dic.ContainsKey(v2Enum_DungonDifficultyType) == false)
                return null;

            return m_masteryDungeonRewardDatas_Dic[v2Enum_DungonDifficultyType];
        }
        //------------------------------------------------------------------------------------
    }
}