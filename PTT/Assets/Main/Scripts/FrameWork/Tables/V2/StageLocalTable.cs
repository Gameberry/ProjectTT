using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using LitJson;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class NormalWaveMonsterData
    {
        public int MonsterID;
        public int MonsterRatio;
    }

    public class MonsterWaveData
    {
        public List<int> WaveMonsterPosition = new List<int>();
        public List<int> WaveMonsterGroup = new List<int>();
    }

    public class StageData
    {
        public int Index;

        public int ResourceIndex;

        public int ChapterNumber;

        public int StageNumberMax;

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


        public V2Enum_Goods MonsterDropRewardType;
        public int MonsterDropRewardParam1;
        public double MonsterDropRewardParam2;

        public string ChapterNameLocalKey;

        public StageClearRewardData stageClearRewardData;

        public StageCooltimeRewardData stageCooltimeRewardData;

        public void CapyData(StageData stageData)
        {
            stageData.MapGroupName = MapGroupName;
            stageData.MapVariationName = MapVariationName;
            stageData.WaveMonsterElementType = WaveMonsterElementType;
            
            stageData.MonsterWaveDatas = MonsterWaveDatas;

            stageData.BossMonsterPosition = BossMonsterPosition;
            stageData.BossMonster = BossMonster;
            stageData.BossMonsterScale = BossMonsterScale;
            stageData.BossSubMonsterData = BossSubMonsterData;

            stageData.BossTimeLimit = BossTimeLimit;

            stageData.ChapterNameLocalKey = ChapterNameLocalKey;
        }
    }

    public class StageClearRewardData
    {
        public List<RewardData> CommonRewardList = new List<RewardData>();

        public RewardData SpecialReward;

        public void CapyStageClearRewardData(StageClearRewardData stageClearRewardData)
        {
            for (int i = 0; i < CommonRewardList.Count; ++i)
            {
                RewardData rewardData = new RewardData();
                rewardData.V2Enum_Goods = CommonRewardList[i].V2Enum_Goods;
                rewardData.Index = CommonRewardList[i].Index;
                rewardData.Amount = CommonRewardList[i].Amount;

                stageClearRewardData.CommonRewardList.Add(rewardData);
            }
        }
    }

    public class StageCooltimeRewardElementData
    {
        public V2Enum_Goods GoodsType;
        public int GoodsIndex;
        public double Amount;
        public double Chance;
    }

    public class StageCooltimeRewardData
    {
        public List<StageCooltimeRewardElementData> StageCooltimeRewardElementDatas = new List<StageCooltimeRewardElementData>();

        public void CopyStageCooltimeRewardData(StageCooltimeRewardData stageCooltimeRewardData)
        {
            for (int i = 0; i < StageCooltimeRewardElementDatas.Count; ++i)
            {
                StageCooltimeRewardElementData rewardData = new StageCooltimeRewardElementData();
                rewardData.GoodsType = StageCooltimeRewardElementDatas[i].GoodsType;
                rewardData.GoodsIndex = StageCooltimeRewardElementDatas[i].GoodsIndex;
                rewardData.Amount = StageCooltimeRewardElementDatas[i].Amount;
                rewardData.Chance = StageCooltimeRewardElementDatas[i].Chance;

                stageCooltimeRewardData.StageCooltimeRewardElementDatas.Add(rewardData);
            }
        }
    }

    public class StageLocalTable : LocalTableBase
    {
        private ConcurrentDictionary<int, StageData> stageDatas = new ConcurrentDictionary<int, StageData>();

        private int originLastStage = 0;
        private int lastStage = 0;
    }
}


