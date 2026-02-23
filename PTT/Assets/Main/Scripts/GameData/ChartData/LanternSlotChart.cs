namespace GameBerry.Chart
{
    public class LanternSlotInfo
    {
        public Enum_LanternSlotType SlotType;
        public int UnLockSummonLevel;
    }

    public class LanternSlotChart : ChartBase
    {
        public LanternSlotInfo this[int index] => rows[index];
        public LanternSlotInfo[] rows;

        public override bool IsLoaded()
        {
            return rows != null;
        }
    }

}