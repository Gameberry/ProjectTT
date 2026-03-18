namespace GameBerry.Chart
{
    public class EquipStatRangeInfo
    {
        public string StatType;
        public double Min;
        public double Max;
        public double LevelMultiple;
        public double Common;
        public double Uncommon;
        public double Rare;
        public double Epic;
        public double Legendary;
        public double Mythic;
        public double Special;
        public Enum_StatMode ValueMode;
    }

    public class EquipStatRangeChart : ChartBase
    {
        public EquipStatRangeInfo this[int index] => rows[index];
        public EquipStatRangeInfo[] rows;

        public override bool IsLoaded()
        {
            return rows != null;
        }
    }

}