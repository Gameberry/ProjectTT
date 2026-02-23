using LitJson;
using BackEnd;
using System.Collections.Generic;

namespace GameBerry.Table
{
    public class LanternData : ItemData, IPackable
    {
        public int level = 1;

        public string Pack() => $"{PackUtil.PackValue(itemId)},{PackUtil.PackValue(count)},{PackUtil.PackValue(level)}";

        public void Unpack(string str)
        {
            itemId = 0;
            instanceId = 0;
            count = 0;
            level = 1;

            if (string.IsNullOrEmpty(str))
                return;

            var sp = str.Split(',');
            if (sp.Length > 0) itemId = PackUtil.UnpackValue<int>(sp[0]);
            if (sp.Length > 1) count = PackUtil.UnpackValue<long>(sp[1]);
            if (sp.Length > 2) level = PackUtil.UnpackValue<int>(sp[2]);
        }
    }

    public class LanternEquipSlotData : IPackable
    {
        public Enum_LanternSlotType slotType;
        public int itemId;

        public string Pack() => $"{PackUtil.PackValue((int)slotType)},{PackUtil.PackValue(itemId)}";

        public void Unpack(string str)
        {
            slotType = Enum_LanternSlotType.Main;
            itemId = 0;

            if (string.IsNullOrEmpty(str))
                return;

            var sp = str.Split(',');
            if (sp.Length > 0) slotType = (Enum_LanternSlotType)PackUtil.UnpackValue<int>(sp[0]);
            if (sp.Length > 1) itemId = PackUtil.UnpackValue<int>(sp[1]);
        }
    }

    public class LanternTable : TableBase
    {
        private const string lanternKey = "Lantern";
        private List<LanternData> lanterns = new();

        private const string equipSlotKey = "LanternEquip";
        private List<LanternEquipSlotData> equipSlots = new();

        public override void SetData(JsonData data)
        {
            if (data == null || data.Count == 0) return;

            for (int i = 0; i < data.Count; ++i)
            {
                foreach (var key in data[i].Keys)
                {
                    if (key == "inDate") SetInData(data[i][key].ToString());
                    else if (key == lanternKey) lanterns = PackUtil.UnpackList<LanternData>(data[i][key].ToString());
                    else if (key == equipSlotKey) equipSlots = PackUtil.UnpackList<LanternEquipSlotData>(data[i][key].ToString());
                }
            }

            EnsureEquipSlots();
        }

        public override Param GetParam()
        {
            Param p = new Param();
            p.Add(lanternKey, PackUtil.PackList(lanterns));
            p.Add(equipSlotKey, PackUtil.PackList(equipSlots));
            return p;
        }

        public long GetAmount(int itemId)
        {
            LanternData lanternData = lanterns.Find(x => x.itemId == itemId);
            return lanternData?.count ?? 0;
        }

        public void Add(int itemId, long amount)
        {
            if (amount == 0) return;

            LanternData lanternData = lanterns.Find(x => x.itemId == itemId);
            if (lanternData == null)
            {
                lanternData = new LanternData { itemId = itemId, count = 0, level = 1 };
                lanterns.Add(lanternData);
            }

            long next = lanternData.count + amount;
            if (next < 0) next = 0;
            lanternData.count = next;
        }

        public LanternData GetLanternData(int itemId)
            => lanterns.Find(x => x.itemId == itemId);

        public List<LanternData> GetAllLanterns()
            => lanterns;

        public int GetEquippedLanternId(Enum_LanternSlotType slotType)
        {
            LanternEquipSlotData slotData = equipSlots.Find(x => x.slotType == slotType);
            return slotData?.itemId ?? 0;
        }

        public void SetEquipped(Enum_LanternSlotType slotType, int itemId)
        {
            EnsureEquipSlots();
            LanternEquipSlotData slotData = equipSlots.Find(x => x.slotType == slotType);
            if (slotData == null)
            {
                slotData = new LanternEquipSlotData { slotType = slotType, itemId = 0 };
                equipSlots.Add(slotData);
            }

            slotData.itemId = itemId;
        }

        public void ClearEquip(Enum_LanternSlotType slotType)
        {
            SetEquipped(slotType, 0);
        }

        public bool IsEquipped(int itemId)
        {
            for (int i = 0; i < equipSlots.Count; ++i)
            {
                if (equipSlots[i].itemId == itemId)
                    return true;
            }
            return false;
        }

        public Enum_LanternSlotType FindSlotTypeByItemId(int itemId)
        {
            for (int i = 0; i < equipSlots.Count; ++i)
            {
                if (equipSlots[i].itemId == itemId)
                    return equipSlots[i].slotType;
            }
            return Enum_LanternSlotType.Max;
        }

        public List<LanternEquipSlotData> GetAllEquipSlots()
            => equipSlots;

        private void EnsureEquipSlots()
        {
            for (Enum_LanternSlotType t = Enum_LanternSlotType.Main; t < Enum_LanternSlotType.Max; ++t)
            {
                if (equipSlots.Exists(x => x.slotType == t))
                    continue;

                equipSlots.Add(new LanternEquipSlotData { slotType = t, itemId = 0 });
            }
        }
    }
}
