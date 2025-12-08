namespace GameBerry.Chart
{
    public struct SkinInfo
    {
        public int Index;
        public string SkinType;
        public string SkinName;
    }

    public class SkinChart : ChartBase
    {
        public SkinInfo this[int index] => rows[index];
        public SkinInfo[] rows;

        public override bool IsLoaded()
        {
            return rows != null;
        }
    }

}