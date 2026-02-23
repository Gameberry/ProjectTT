using System.Collections.Generic;

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
        private Dictionary<Enum_LanternSlotType, LanternSlotInfo> _slotTypeToInfo;

        public override bool IsLoaded()
        {
            return rows != null;
        }

        public override void LoadComplete()
        {
            _slotTypeToInfo = new Dictionary<Enum_LanternSlotType, LanternSlotInfo>(rows.Length);
            for (int i = 0; i < rows.Length; ++i)
            {
                LanternSlotInfo slotInfo = rows[i];
                if (slotInfo == null)
                    continue;

                _slotTypeToInfo[slotInfo.SlotType] = slotInfo;
            }
        }

        public LanternSlotInfo Get(Enum_LanternSlotType slotType)
            => _slotTypeToInfo != null && _slotTypeToInfo.TryGetValue(slotType, out var v) ? v : null;

        public IReadOnlyList<LanternSlotInfo> GetAllSlots()
            => rows;
    }

}
