using System.Collections.Generic;
using LitJson;
using BackEnd;
using GameBerry.Chart;

namespace GameBerry.Table
{
    public struct EngravingSlot : IPackable
    {
        public Enum_Stat statType;
        public Enum_Rarity grade;
        public float value;

        public bool IsEmpty => statType == default;

        public string Pack() => $"{PackUtil.PackValue(statType.Enum32ToInt())},{PackUtil.PackValue(grade.Enum32ToInt())},{PackUtil.PackValue(value)}";

        public void Unpack(string str)
        {
            statType = default;
            grade = default;
            value = 0f;

            if (string.IsNullOrEmpty(str))
                return;

            var sp = str.Split(',');

            if (sp.Length >= 1)
                statType = PackUtil.UnpackValue<int>(sp[0]).IntToEnum32<Enum_Stat>();

            if (sp.Length >= 2)
                grade = PackUtil.UnpackValue<int>(sp[1]).IntToEnum32<Enum_Rarity>();

            if (sp.Length >= 3)
                value = PackUtil.UnpackValue<float>(sp[2]);
        }

        public void Clear()
        {
            statType = default;
            grade = default;
            value = 0f;
        }

        public void Set(Enum_Stat stat, Enum_Rarity optionGrade, float val)
        {
            statType = stat;
            grade = optionGrade;
            value = val;
        }
    }

    public class EngravingStageData : IPackable
    {
        public const int SlotCount = 3;

        public int stage;
        public bool isUnlocked;
        public List<EngravingSlot> slots = new List<EngravingSlot>();

        public EngravingStageData()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                slots.Add(new EngravingSlot());
            }
        }

        public string Pack() => $"{PackUtil.PackValue(stage)},{PackUtil.PackValue(isUnlocked)}:{PackUtil.PackList(slots, PackSep.L1)}";

        public void Unpack(string str)
        {
            stage = 0;
            isUnlocked = false;
            slots.Clear();

            if (string.IsNullOrEmpty(str))
                return;

            var tsp = str.Split(':');

            if (tsp.Length > 0)
            {
                var sp = tsp[0].Split(',');
                if (sp.Length > 0) stage = PackUtil.UnpackValue<int>(sp[0]);
                if (sp.Length > 1) isUnlocked = PackUtil.UnpackValue<bool>(sp[1]);
            }

            if (tsp.Length > 1 && string.IsNullOrEmpty(tsp[1]) == false)
                slots = PackUtil.UnpackList<EngravingSlot>(tsp[1], PackSep.L1);

            while (slots.Count < SlotCount)
            {
                slots.Add(new EngravingSlot());
            }
        }

        public bool HasMatchingStats()
        {
            Enum_Stat statType = default;

            for (int i = 0; i < slots.Count; ++i)
            {
                if (slots[i].IsEmpty)
                    return false;

                if (i == 0)
                    statType = slots[i].statType;
                else
                {
                    if (statType != slots[i].statType)
                        return false;
                }
            }

            return true;
        }

        public Enum_Rarity GetLowestGrade()
        {
            var lowest = Enum_Rarity.Epic;

            foreach (var slot in slots)
            {
                if (!slot.IsEmpty && (int)slot.grade < (int)lowest)
                {
                    lowest = slot.grade;
                }
            }

            return lowest;
        }

        public void Clear()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                slot.Clear();
                slots[i] = slot;
            }
        }
    }

    public class EngravingTable : TableBase
    {
        public const int MaxStage = 10;

        private const string engravingsKey = "Engravings";
        private Dictionary<int, EngravingStageData> engravingsDict = new Dictionary<int, EngravingStageData>();
        //------------------------------------------------------------------------------------
        public override void SetData(JsonData data)
        {
            if (data == null || data.Count == 0)
            {
                InitializeEngravings();
                return;
            }

            for (int i = 0; i < data.Count; ++i)
            {
                foreach (var key in data[i].Keys)
                {
                    if (key == "inDate")
                        SetInData(data[i][key].ToString());
                    else if (key == engravingsKey)
                        engravingsDict = PackUtil.UnpackDict<int, EngravingStageData>(data[i][key].ToString());
                }
            }

            if (engravingsDict.Count == 0)
                InitializeEngravings();
        }
        //------------------------------------------------------------------------------------
        private void InitializeEngravings()
        {
            engravingsDict.Clear();

            for (int i = 0; i < MaxStage; i++)
            {
                var stageData = new EngravingStageData
                {
                    stage = i + 1,
                    isUnlocked = i == 0
                };
                engravingsDict.Add(i + 1, stageData);
            }
        }
        //------------------------------------------------------------------------------------
        public override Param GetParam()
        {
            Param p = new Param();
            p.Add(engravingsKey, PackUtil.PackDict(engravingsDict));
            return p;
        }
        //------------------------------------------------------------------------------------
        public EngravingStageData GetEngraving(int stageNumber)
        {
            if (engravingsDict.TryGetValue(stageNumber, out var data))
                return data;

            return null;
        }
        //------------------------------------------------------------------------------------
        public bool TryGetEngraving(int stageNumber, out EngravingStageData data)
        {
            return engravingsDict.TryGetValue(stageNumber, out data);
        }
        //------------------------------------------------------------------------------------
        public bool IsUnlocked(int stageNumber)
        {
            if (engravingsDict.TryGetValue(stageNumber, out var data))
                return data.isUnlocked;

            return false;
        }
        //------------------------------------------------------------------------------------
        public bool SetUnlocked(int stageNumber, bool unlocked)
        {
            if (engravingsDict.TryGetValue(stageNumber, out var data) == false)
                return false;

            data.isUnlocked = unlocked;
            return true;
        }
        //------------------------------------------------------------------------------------
        public bool SetSlot(int stageNumber, int slotIndex, Enum_Stat statType, Enum_Rarity grade, float value)
        {
            if (engravingsDict.TryGetValue(stageNumber, out var data) == false)
                return false;

            if (slotIndex < 0 || slotIndex >= data.slots.Count)
                return false;

            var slot = data.slots[slotIndex];
            slot.Set(statType, grade, value);
            data.slots[slotIndex] = slot;

            return true;
        }
        //------------------------------------------------------------------------------------
        public bool ClearStage(int stageNumber)
        {
            if (engravingsDict.TryGetValue(stageNumber, out var data) == false)
                return false;

            data.Clear();
            return true;
        }
        //------------------------------------------------------------------------------------
        public IEnumerable<EngravingStageData> GetAllEngravings()
        {
            return engravingsDict.Values;
        }
        //------------------------------------------------------------------------------------
        public double GetTotalStat(Enum_Stat statType)
        {
            var result = 0D;

            foreach (var engraving in engravingsDict.Values)
            {
                if (!engraving.isUnlocked)
                    continue;

                foreach (var slot in engraving.slots)
                {
                    if (!slot.IsEmpty && slot.statType == statType)
                        result += slot.value;
                }
            }

            return result;
        }
        //------------------------------------------------------------------------------------
    }
}