using System.Collections.Generic;

namespace GameBerry.Chart
{
    public struct EquipRandomPoolInfo
    {
        public Enum_EquipType EquipType;
        public Enum_Stat Stat;
        public double Weight;
        public float MinValue;
        public float MaxValue;
        public Enum_StatMode ValueMode;

        public double GetRandomStatValue()
        {
            double statvalue = 0;
            if (ValueMode == Enum_StatMode.Int)
                statvalue = UnityEngine.Random.Range((int)MinValue, (int)MaxValue + 1);
            else
                statvalue = System.Math.Round(UnityEngine.Random.Range(MinValue, MaxValue), 2);

            return statvalue;
        }
    }

    public class EquipRandomPoolChart : ChartBase
    {
        public EquipRandomPoolInfo this[int index] => rows[index];
        public EquipRandomPoolInfo[] rows;

        public Dictionary<Enum_EquipType, WeightedRandomPicker<EquipRandomPoolInfo>> _picker = new Dictionary<Enum_EquipType, WeightedRandomPicker<EquipRandomPoolInfo>>();

        public Dictionary<Enum_EquipType, List<EquipRandomPoolInfo>> _pool= new Dictionary<Enum_EquipType, List<EquipRandomPoolInfo>>();
        public override bool IsLoaded()
        {
            return rows != null;
        }

        public List<EquipRandomPoolInfo> GetRandomPool(Enum_EquipType enum_EquipType)
        {
            List<EquipRandomPoolInfo> pool = null;

            if (_pool.TryGetValue(enum_EquipType, out pool) == false)
            {
                pool = new List<EquipRandomPoolInfo>();

                for (int i = 0; i < rows.Length; ++i)
                {
                    if (rows[i].EquipType == enum_EquipType)
                        pool.Add(rows[i]);
                }
            }

            return pool;
        }
    }
}