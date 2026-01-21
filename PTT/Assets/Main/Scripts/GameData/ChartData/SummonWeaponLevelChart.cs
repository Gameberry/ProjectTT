namespace GameBerry.Chart
{
    public struct SummonWeaponLevelInfo
    {
        public int SummonLevel;
        public int Exp;
        public int Reward;
    }

    public class SummonWeaponLevelChart : ChartBase
    {
        public SummonWeaponLevelInfo this[int index] => rows[index];
        public SummonWeaponLevelInfo[] rows;

        public override bool IsLoaded()
        {
            return rows != null;
        }
    }

}