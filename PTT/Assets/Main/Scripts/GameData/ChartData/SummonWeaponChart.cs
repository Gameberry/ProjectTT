namespace GameBerry.Chart
{
    public struct SummonWeaponInfo
    {
        public int SummonLevel;
        public int Item;
        public double Prob;
    }

    public class SummonWeaponChart : ChartBase
    {
        public SummonWeaponInfo this[int index] => rows[index];
        public SummonWeaponInfo[] rows;

        public override bool IsLoaded()
        {
            return rows != null;
        }
    }

}