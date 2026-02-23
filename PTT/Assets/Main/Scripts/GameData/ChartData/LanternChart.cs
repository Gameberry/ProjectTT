namespace GameBerry.Chart
{
    public class LanternInfo
    {
        public int ItemId;
        public string EquipStat;
        public string OwnStat;
        public int Skill;
    }

    public class LanternChart : ChartBase
    {
        public LanternInfo this[int index] => rows[index];
        public LanternInfo[] rows;

        public override bool IsLoaded()
        {
            return rows != null;
        }
    }

}