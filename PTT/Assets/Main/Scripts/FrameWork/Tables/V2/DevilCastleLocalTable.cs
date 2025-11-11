using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using LitJson;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Cysharp.Threading.Tasks;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class MonsterDropRaward_Weight
    {
        public RewardData RewardData = new RewardData();
        public ObscuredInt Weight;

        public bool Pick()
        {
            return Weight >= UnityEngine.Random.Range(1, 10001);
        }
    }

    public class DevilCastleData
    {
        public int Index;

        public int ResourceIndex;

        public int ChapterNumber;

        public int StageNumber;

        public string MapGroupName;
        public string MapVariationName;

        public V2Enum_ElementType WaveMonsterElementType;

        public int WaveMonsterLevel;
        public List<MonsterWaveData> MonsterWaveDatas = new List<MonsterWaveData>();

        public int BossMonsterLevel;
        public int BossMonsterPosition;
        public int BossMonster;

        public float BossMonsterScale;

        public double BossSubMonsterLevel;
        public MonsterWaveData BossSubMonsterData;


        public float BossTimeLimit = 59.0f;


        public List<MonsterDropRaward_Weight> MonsterDropReward = new List<MonsterDropRaward_Weight>();
        public List<RewardData> ClearRewardList = new List<RewardData>();

        public string ChapterNameLocalKey;

    }

    public class DevilCastleLocalTable : LocalTableBase
    {
        private ConcurrentDictionary<int, DevilCastleData> devilCastleDatas = new ConcurrentDictionary<int, DevilCastleData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("ExtraStageDevilCastle", o =>
            {
                rows = o;
            });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                JsonData jsonData = rows[i];

                DevilCastleData devilCastleData = new DevilCastleData();

                devilCastleData.Index = jsonData["Index"].ToString().Replace(",", "").ToInt();
                devilCastleData.ResourceIndex = jsonData["ResourceIndex"].ToString().ToInt();
                devilCastleData.ChapterNumber = jsonData["ChapterNumber"].ToString().ToInt();

                devilCastleData.StageNumber = jsonData["StageNumber"].ToString().ToInt();

                devilCastleData.MapGroupName = jsonData["MapGroupName"].ToString();
                devilCastleData.MapVariationName = jsonData["MapVariationName"].ToString();

                devilCastleData.WaveMonsterElementType = jsonData["WaveMonsterElementType"].ToString().ToInt().IntToEnum32<V2Enum_ElementType>();

                devilCastleData.WaveMonsterLevel = jsonData["WaveMonsterLevel"].ToString().ToInt();

                for (int j = 1; j <= 9; ++j)
                {
                    try
                    {
                        MonsterWaveData monsterWaveData = new MonsterWaveData();
                        monsterWaveData.WaveMonsterPosition = JsonConvert.DeserializeObject<List<int>>(rows[i][string.Format("Wave{0}MonsterPosition", j)].ToString());
                        monsterWaveData.WaveMonsterGroup = JsonConvert.DeserializeObject<List<int>>(rows[i][string.Format("Wave{0}MonsterGroup", j)].ToString());
                        if (monsterWaveData.WaveMonsterPosition.Count <= 0 || monsterWaveData.WaveMonsterGroup.Count <= 0)
                            break;

                        devilCastleData.MonsterWaveDatas.Add(monsterWaveData);
                    }
                    catch
                    {

                    }
                }

                devilCastleData.BossMonsterLevel = jsonData["BossMonsterLevel"].ToString().ToInt();


                devilCastleData.BossMonsterPosition = jsonData["BossMonsterPosition"].ToString().ToInt();
                devilCastleData.BossMonster = jsonData["BossMonster"].ToString().ToInt();

                devilCastleData.BossMonsterScale = jsonData["BossMonsterScale"].ToString().ToFloat();

                devilCastleData.BossSubMonsterLevel = devilCastleData.WaveMonsterLevel;

                try
                {
                    MonsterWaveData monsterWaveData = new MonsterWaveData();
                    monsterWaveData.WaveMonsterPosition = JsonConvert.DeserializeObject<List<int>>(rows[i]["BossSubMonsterPosition"].ToString());
                    monsterWaveData.WaveMonsterGroup = JsonConvert.DeserializeObject<List<int>>(rows[i]["BossSubMonsterGroup"].ToString());
                    //if (monsterWaveData.WaveMonsterPosition.Count <= 0 || monsterWaveData.WaveMonsterGroup.Count <= 0)
                    //    continue;

                    devilCastleData.BossSubMonsterData = monsterWaveData;
                }
                catch
                {

                }

                devilCastleData.BossTimeLimit = jsonData["BossTimeLimit"].ToString().ToFloat();


                for (int j = 1; j <= 1; ++j)
                {
                    try
                    {
                        MonsterDropRaward_Weight monsterDropRaward_Weight = new MonsterDropRaward_Weight();
                        monsterDropRaward_Weight.RewardData.V2Enum_Goods = jsonData[string.Format("MonsterDropRewardType{0}", j)].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                        monsterDropRaward_Weight.RewardData.Index = jsonData[string.Format("MonsterDropRewardParam{0}1", j)].ToString().ToInt();
                        monsterDropRaward_Weight.RewardData.Amount = jsonData[string.Format("MonsterDropRewardParam{0}2", j)].ToString().ToDouble();
                        monsterDropRaward_Weight.Weight = jsonData[string.Format("MonsterDropRewardParam{0}3", j)].ToString().ToInt();

                        devilCastleData.MonsterDropReward.Add(monsterDropRaward_Weight);
                    }
                    catch
                    {

                    }
                }


                for (int j = 1; j <= 4; ++j)
                {
                    try
                    {
                        int idx = jsonData[string.Format("ClearRewardType{0}", j)].ToString().ToInt();
                        if (idx == -1)
                            continue;

                        RewardData rewardData = new RewardData();
                        rewardData.V2Enum_Goods = idx.IntToEnum32<V2Enum_Goods>();
                        rewardData.Index = jsonData[string.Format("ClearRewardParam{0}1", j)].ToString().ToInt();
                        rewardData.Amount = jsonData[string.Format("ClearRewardParam{0}2", j)].ToString().ToDouble();

                        devilCastleData.ClearRewardList.Add(rewardData);
                    }
                    catch
                    {

                    }
                }


                devilCastleData.ChapterNameLocalKey = string.Format("chapter/{0}/name", devilCastleData.ResourceIndex);

                this.devilCastleDatas.TryAdd(devilCastleData.StageNumber, devilCastleData);

            }
        }
        //------------------------------------------------------------------------------------
        public DevilCastleData GetData(int stage)
        {
            if (devilCastleDatas.ContainsKey(stage) == true)
                return devilCastleDatas[stage];

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}