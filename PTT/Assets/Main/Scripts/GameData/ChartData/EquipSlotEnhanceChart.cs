using System.Collections.Generic;

namespace GameBerry.Chart
{
    public struct EquipSlotEnhanceInfo
    {
        public int Step;
        public double MainStatPer;
        public double SubStatPer;
        public float Success;
        public float Stay;
        public float Down;
        public float Destroy;
        public int MainPriceKey;
        public long MainPrice;
        public int SubPriceKey;
        public long SubPrice;
    }

    public class EquipSlotEnhanceChart : ChartBase
    {
        public EquipSlotEnhanceInfo this[int index] => rows[index];
        public EquipSlotEnhanceInfo[] rows;

        Dictionary<int, EquipSlotEnhanceInfo> enhance_Dict = new Dictionary<int, EquipSlotEnhanceInfo>();

        public override bool IsLoaded()
        {
            return rows != null;
        }

        public bool TryGetEquipSlotEnhanceInfo(int step, out EquipSlotEnhanceInfo enhanceInfo)
        {
            if (enhance_Dict.TryGetValue(step, out enhanceInfo))
                return true;

            for (int i = 0; i < rows.Length; ++i)
            {
                if (rows[i].Step == step)
                {
                    enhance_Dict.Add(step, rows[i]);
                    enhanceInfo = rows[i];
                    return true;
                }
            }

            enhanceInfo = default;

            return false;
        }
    }

}