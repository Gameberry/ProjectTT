using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using LitJson;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class HellDungeonData
    {
        public int Index;
        public int PhaseNumber;

        public V2Enum_ElementType PhaseElementType;

        public List<int> WaveMonsterGroup = new List<int>();

        public double HP;

        public List<CreatureBaseStatElement> MasteryDungeonOverrideStats = new List<CreatureBaseStatElement>();

        public V2Enum_DungeonDifficultyType DungeonDifficulty;

        public HellDungeonData PrevData;
        public HellDungeonData NextData;
    }

    public class HellDungeonRewardData
    {
        public int Index;

        public double AccumedDamageMin;
        public double AccumedDamageMax;

        public int ClearRewardDropCount;

        public List<RewardData> PhaseRewardGoodsList = new List<RewardData>();

        public V2Enum_DungeonDifficultyType DungeonDifficulty;
    }

    public class HellDungeonLocalTable : LocalTableBase
    {
        public Dictionary<V2Enum_DungeonDifficultyType, List<HellDungeonData>> hellDungeonDatas_Dic = new Dictionary<V2Enum_DungeonDifficultyType, List<HellDungeonData>>();

        public Dictionary<V2Enum_DungeonDifficultyType, List<HellDungeonRewardData>> hellDungeonRewardDatas_Dic = new Dictionary<V2Enum_DungeonDifficultyType, List<HellDungeonRewardData>>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("HellDungeon", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                HellDungeonData hellDungeonData = new HellDungeonData();

                hellDungeonData.Index = rows[i]["Index"].ToString().ToInt();
                hellDungeonData.PhaseNumber = rows[i]["PhaseNumber"].ToString().ToInt();

                hellDungeonData.PhaseElementType = rows[i]["PhaseElementType"].ToString().FastStringToInt().IntToEnum32<V2Enum_ElementType>();

                hellDungeonData.WaveMonsterGroup.Add(106010243);

                for (int j = 1; j <= 7; ++j)
                {
                    try
                    {
                        int overridestat = rows[i][string.Format("BossOverrideStatType{0}", j)].ToString().ToInt();

                        if (overridestat == -1 || overridestat == 0)
                            continue;

                        CreatureBaseStatElement creatureBaseStatElement = new CreatureBaseStatElement();
                        creatureBaseStatElement.BaseStat = overridestat.IntToEnum32<V2Enum_Stat>();
                        creatureBaseStatElement.BaseValue = rows[i][string.Format("BossOverrideStatValue{0}", j)].ToString().ToDouble();

                        if (creatureBaseStatElement.BaseStat == V2Enum_Stat.HP)
                            hellDungeonData.HP = creatureBaseStatElement.BaseValue;

                        hellDungeonData.MasteryDungeonOverrideStats.Add(creatureBaseStatElement);
                    }
                    catch
                    {

                    }
                }

                try
                {
                    hellDungeonData.DungeonDifficulty = rows[i]["DungeonDifficulty"].ToString().ToInt().IntToEnum32<V2Enum_DungeonDifficultyType>();
                }
                catch
                {
                }

                if (hellDungeonDatas_Dic.ContainsKey(hellDungeonData.DungeonDifficulty) == false)
                    hellDungeonDatas_Dic.Add(hellDungeonData.DungeonDifficulty, new List<HellDungeonData>());

                hellDungeonDatas_Dic[hellDungeonData.DungeonDifficulty].Add(hellDungeonData);
            }


            foreach (var pair in hellDungeonDatas_Dic)
            {
                List<HellDungeonData> hellDungeonDatas = pair.Value;

                for (int i = 0; i < hellDungeonDatas.Count; ++i)
                {
                    HellDungeonData hellDungeonData = hellDungeonDatas[i];

                    if (i != 0)
                    {
                        hellDungeonData.PrevData = hellDungeonDatas[i - 1];
                    }

                    if (i != hellDungeonDatas.Count - 1)
                    {
                        hellDungeonData.NextData = hellDungeonDatas[i + 1];
                    }
                }
            }


            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("HellDungeonReward", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                HellDungeonRewardData hellDungeonRewardData = new HellDungeonRewardData();

                hellDungeonRewardData.Index = rows[i]["Index"].ToString().ToInt();
                hellDungeonRewardData.AccumedDamageMin = rows[i]["AccumedDamageMin"].ToString().ToDouble();
                hellDungeonRewardData.AccumedDamageMax = rows[i]["AccumedDamageMax"].ToString().ToDouble();

                hellDungeonRewardData.ClearRewardDropCount = rows[i]["ClearRewardDropCount"].ToString().ToInt();

                //for (int j = 1; j <= 2; ++j) 이제 반복문이 필요 없어서 이렇게 수정
                {
                    int j = 1;
                    try
                    {
                        int goodstype = rows[i][string.Format("PhaseRewardGoodsType{0}", j)].ToString().ToInt();
                        if (goodstype != -1 && goodstype != 0)
                        {
                            RewardData rewardData = new RewardData();

                            rewardData.V2Enum_Goods = goodstype.IntToEnum32<V2Enum_Goods>();

                            rewardData.Index = rows[i][string.Format("PhaseRewardGoodsParam{0}1", j)].ToString().ToInt();
                            rewardData.Amount = rows[i][string.Format("PhaseRewardGoodsParam{0}2", j)].ToString().ToDouble();

                            hellDungeonRewardData.PhaseRewardGoodsList.Add(rewardData);
                        }
                    }
                    catch
                    {

                    }
                }


                try
                {
                    hellDungeonRewardData.DungeonDifficulty = rows[i]["DungeonDifficulty"].ToString().ToInt().IntToEnum32<V2Enum_DungeonDifficultyType>();
                }
                catch
                {
                }


                if (hellDungeonRewardDatas_Dic.ContainsKey(hellDungeonRewardData.DungeonDifficulty) == false)
                    hellDungeonRewardDatas_Dic.Add(hellDungeonRewardData.DungeonDifficulty, new List<HellDungeonRewardData>());

                hellDungeonRewardDatas_Dic[hellDungeonRewardData.DungeonDifficulty].Add(hellDungeonRewardData);
            }
        }
        //------------------------------------------------------------------------------------
        public List<HellDungeonData> GetHellDungeonData(V2Enum_DungeonDifficultyType v2Enum_DungonDifficultyType)
        {
            if (hellDungeonDatas_Dic.ContainsKey(v2Enum_DungonDifficultyType) == false)
                return null;

            return hellDungeonDatas_Dic[v2Enum_DungonDifficultyType];
        }
        //------------------------------------------------------------------------------------
        public List<HellDungeonRewardData> GetHellDungeonRewardData(V2Enum_DungeonDifficultyType v2Enum_DungonDifficultyType)
        {
            if (hellDungeonRewardDatas_Dic.ContainsKey(v2Enum_DungonDifficultyType) == false)
                return null;

            return hellDungeonRewardDatas_Dic[v2Enum_DungonDifficultyType];
        }
        //------------------------------------------------------------------------------------
    }
}