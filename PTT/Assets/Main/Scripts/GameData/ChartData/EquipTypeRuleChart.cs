namespace GameBerry.Chart
{
    public class EquipTypeRuleInfo
    {
        public Enum_EquipType EquipType;
        public Enum_Stat[] FixedStatType;
        public Enum_Stat[] RandomStatType;
    }

    public class EquipTypeRuleChart : ChartBase
    {
        public EquipTypeRuleInfo this[int index] => rows[index];
        public EquipTypeRuleInfo[] rows;
        private System.Collections.Generic.Dictionary<Enum_EquipType, EquipTypeRuleInfo> _ruleByEquipType;

        public override bool IsLoaded()
        {
            return rows != null;
        }

        public override void LoadComplete()
        {
            _ruleByEquipType = new System.Collections.Generic.Dictionary<Enum_EquipType, EquipTypeRuleInfo>();

            if (rows == null)
                return;

            for (int i = 0; i < rows.Length; ++i)
            {
                EquipTypeRuleInfo info = rows[i];
                if (info == null)
                    continue;

                _ruleByEquipType[info.EquipType] = info;
            }
        }

        public bool TryGetRule(Enum_EquipType equipType, out EquipTypeRuleInfo info)
        {
            info = null;
            return _ruleByEquipType != null && _ruleByEquipType.TryGetValue(equipType, out info);
        }
    }

}
