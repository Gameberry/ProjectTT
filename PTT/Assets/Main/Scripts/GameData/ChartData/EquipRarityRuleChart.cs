namespace GameBerry.Chart
{
    public struct EquipRandomRuleInfo
    {
        public Enum_Rarity Rarity;
        public int RandomStatMin;
        public int RandomStatMax;
        public int SalvagePoints;
    }

    public class EquipRarityRuleChart : ChartBase
    {
        public EquipRandomRuleInfo this[int index] => rows[index];
        public EquipRandomRuleInfo[] rows;

        public override bool IsLoaded()
        {
            return rows != null;
        }

        public bool TryGetRandomRule(Enum_Rarity enum_Rarity, out EquipRandomRuleInfo rule)
        {
            for (int i = 0; i < rows.Length; ++i)
            {
                if (rows[i].Rarity == enum_Rarity)
                { 
                    rule = rows[i];
                    return true;
                }
            }

            rule = default;
            return false;
        }
    }
}