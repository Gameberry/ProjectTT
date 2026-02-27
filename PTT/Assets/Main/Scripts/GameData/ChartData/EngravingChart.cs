using System.Linq;

namespace GameBerry.Chart
{
    // 각인 옵션 정보
    public struct EngravingInfo
    {
        public int Stage;
        public Enum_Rarity SlotTier;
        public Enum_Stat StatType;
        public Enum_Rarity Grade;
        public float MinValue;
        public float MaxValue;
        public float Probability;
    }

    public class EngravingChart : ChartBase
    {
        public EngravingInfo this[int index] => rows[index];
        public EngravingInfo[] rows;

        public override bool IsLoaded()
        {
            return rows != null;
        }

        public EngravingInfo[] GetByStage(int stage)
        {
            return rows.Where(x => x.Stage == stage).ToArray();
        }

        public EngravingInfo[] GetByStageAndTier(int stage, Enum_Rarity tier)
        {
            return rows.Where(x => x.Stage == stage && x.SlotTier == tier).ToArray();
        }
    }

    // 각인 매칭 확률
    public struct EngravingMatchingInfo
    {
        public int Stage;
        public float Probability;
    }

    public class EngravingMatchingChart : ChartBase
    {
        public EngravingMatchingInfo this[int index] => rows[index];
        public EngravingMatchingInfo[] rows;

        public override bool IsLoaded()
        {
            return rows != null;
        }

        public float GetMatchingRate(int stage)
        {
            var info = rows.FirstOrDefault(x => x.Stage == stage);
            return info.Probability;
        }
    }
}