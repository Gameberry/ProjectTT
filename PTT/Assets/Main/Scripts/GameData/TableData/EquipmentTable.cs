using LitJson;
using BackEnd;
using System.Collections.Generic;
using GameBerry.Chart;

namespace GameBerry.Table
{
    public class EquipSlotData : IPackable
    {
        public Enum_EquipType slot;
        public int instanceId;
        public int level;

        public string Pack() => $"{PackUtil.PackValue(slot.Enum32ToInt())},{PackUtil.PackValue(instanceId)},{PackUtil.PackValue(level)}";
        public void Unpack(string str)
        {
            instanceId = 0;
            level = 0;

            if (string.IsNullOrEmpty(str))
                return;

            var sp = str.Split(',');

            if (sp.Length >= 1)
                slot = PackUtil.UnpackValue<int>(sp[0]).IntToEnum32<Enum_EquipType>();

            if (sp.Length >= 2)
                instanceId = PackUtil.UnpackValue<int>(sp[1]);

            if (sp.Length >= 3)
                level = PackUtil.UnpackValue<int>(sp[2]);
        }
    }

    public struct EquipmentAddStat : IPackable
    {
        public Enum_Stat stat;
        public double value;
        public string Pack() => $"{PackUtil.PackValue(stat.Enum32ToInt())},{PackUtil.PackValue(value)}";

        public void Unpack(string str)
        {
            if (string.IsNullOrEmpty(str))
                return;

            var sp = str.Split(',');

            if (sp.Length >= 1)
                stat = PackUtil.UnpackValue<int>(sp[0]).IntToEnum32<Enum_Stat>();

            if (sp.Length >= 2)
                value = PackUtil.UnpackValue<double>(sp[1]);
        }

        public static EquipmentAddStat Set(Enum_Stat Stat, double Value)
            => new EquipmentAddStat
            {
                stat = Stat,
                value = Value
            };
    }

    public class EquipmentData : IPackable
    {
        public int instanceId;
        public int itemId;
        public List<EquipmentAddStat> addStatList;

        // Backward compatible:
        // - New: instanceId,itemId:addStat...
        // - Old: instanceId:addStat...
        public string Pack() => $"{PackUtil.PackValue(instanceId)},{PackUtil.PackValue(itemId)}:{PackUtil.PackList(addStatList, PackSep.L1)}";

        public void Unpack(string str)
        {
            instanceId = 0;
            itemId = 0;
            addStatList = new List<EquipmentAddStat>();

            if (string.IsNullOrEmpty(str)) return;

            var tsp = str.Split(':');

            if (tsp.Length > 0)
            {
                var sp = tsp[0].Split(',');
                if (sp.Length > 0) instanceId = PackUtil.UnpackValue<int>(sp[0]);
                if (sp.Length > 1) itemId = PackUtil.UnpackValue<int>(sp[1]);
            }

            if (tsp.Length > 1 && string.IsNullOrEmpty(tsp[1]) == false)
                addStatList = PackUtil.UnpackList<EquipmentAddStat>(tsp[1], PackSep.L1);
        }
    }

    public class EquipmentTable : TableBase
    {
        private const string equippedKey = "Equipped";
        private List<EquipSlotData> equipped = new List<EquipSlotData>();

        private const string equipmentDataKey = "Equipment";
        private Dictionary<int, EquipmentData> equipmentDataDict = new Dictionary<int, EquipmentData>();

        private int nextInstanceId = 1;

        public override void SetData(JsonData data)
        {
            if (data == null || data.Count == 0) return;

            for (int i = 0; i < data.Count; ++i)
            {
                foreach (var key in data[i].Keys)
                {
                    if (key == "inDate")
                        SetInData(data[i][key].ToString());
                    else if (key == equippedKey)
                        equipped = PackUtil.UnpackList<EquipSlotData>(data[i][key].ToString());
                    else if (key == equipmentDataKey)
                        equipmentDataDict = PackUtil.UnpackDict<int, EquipmentData>(data[i][key].ToString());
                }
            }

            RebuildNextInstanceId();
        }

        public override Param GetParam()
        {
            Param p = new Param();
            p.Add(equippedKey, PackUtil.PackList(equipped));
            p.Add(equipmentDataKey, PackUtil.PackDict(equipmentDataDict));
            return p;
        }

        private void RebuildNextInstanceId()
        {
            nextInstanceId = 1;
            if (equipmentDataDict == null || equipmentDataDict.Count == 0)
                return;

            foreach (var kv in equipmentDataDict)
            {
                if (kv.Key >= nextInstanceId)
                    nextInstanceId = kv.Key + 1;
            }
        }

        private int GetNewInstanceId()
        {
            if (nextInstanceId <= 0)
                nextInstanceId = 1;

            while (equipmentDataDict.ContainsKey(nextInstanceId))
                nextInstanceId++;

            return nextInstanceId++;
        }

        public ItemHandle AddEquipment(int itemId, List<EquipmentAddStat> addStats = null)
        {
            int instanceId = GetNewInstanceId();
            EquipmentData equipmentData = new EquipmentData
            {
                instanceId = instanceId,
                itemId = itemId,
                addStatList = addStats ?? new List<EquipmentAddStat>()
            };

            equipmentDataDict[instanceId] = equipmentData;
            return ItemHandle.ForInstance(itemId, instanceId);
        }

        public EquipmentData GetEquipmentData(int instanceId)
        {
            if (equipmentDataDict.TryGetValue(instanceId, out var data))
                return data;

            return null;
        }

        public bool RemoveEquipment(int instanceId)
        {
            if (IsEquipped(instanceId))
                return false;

            if (equipmentDataDict.ContainsKey(instanceId) == false)
                return false;

            equipmentDataDict.Remove(instanceId);
            return true;
        }

        public int GetEquippedInstanceId(GameBerry.Enum_EquipType slot)
        {
            var d = equipped.Find(x => x.slot == slot);
            return d != null ? d.instanceId : 0;
        }

        public int GetStarforceLevel(GameBerry.Enum_EquipType slot)
        {
            var d = equipped.Find(x => x.slot == slot);
            if (d == null)
                return 0;

            if (d.level == -1)
                return 0;

            return d.level;
        }

        public bool IsDestroyStarforce(GameBerry.Enum_EquipType slot)
        {
            var d = equipped.Find(x => x.slot == slot);
            if (d == null)
                return false;

            return d.level == -1;
        }

        public bool EnhanceSlot(Enum_EquipType slot, Enum_StarforceResult enum_StarforceResult, bool immediate = true)
        {
            if (enum_StarforceResult == Enum_StarforceResult.Stay)
                return true;

            var d = equipped.Find(x => x.slot == slot);

            if (d == null)
                return false;

            if (enum_StarforceResult == Enum_StarforceResult.Success)
                d.level += 1;
            else if (enum_StarforceResult == Enum_StarforceResult.Down)
                d.level -= 1;
            else if (enum_StarforceResult == Enum_StarforceResult.Destroy)
                d.level = -1;

            if (immediate == true)
                UpdateTable();

            return true;
        }

        public bool DoSlotRestoration(GameBerry.Enum_EquipType slot, bool immediate = true)
        {
            var d = equipped.Find(x => x.slot == slot);
            if (d == null)
                return false;

            if (d.level != -1)
                return false;

            d.level = 12;

            if (immediate == true)
                UpdateTable();

            return true;
        }

        public void SetEquipped(GameBerry.Enum_EquipType slot, int instanceId)
        {
            var d = equipped.Find(x => x.slot == slot);
            if (d == null)
            {
                equipped.Add(new EquipSlotData { slot = slot, instanceId = instanceId, level = 0 });
                return;
            }
            d.instanceId = instanceId;
        }

        public bool IsEquipped(int instanceId)
        {
            for (int i = 0; i < equipped.Count; i++)
            {
                if (equipped[i] != null && equipped[i].instanceId == instanceId)
                    return true;
            }

            return false;
        }

        public bool TryGetData(int instanceId, out EquipmentData data)
        {
            return equipmentDataDict.TryGetValue(instanceId, out data);
        }

        public bool HasEquipment(int instanceId)
        {
            return equipmentDataDict.ContainsKey(instanceId);
        }

        public bool TryGetHandleByInstanceId(int instanceId, out ItemHandle handle)
        {
            handle = default;
            if (!equipmentDataDict.TryGetValue(instanceId, out var data))
                return false;

            handle = ItemHandle.ForInstance(data.itemId, data.instanceId);
            return true;
        }

        public long CountByItemId(int itemId)
        {
            long count = 0;
            foreach (var kv in equipmentDataDict)
            {
                if (kv.Value == null) continue;
                if (kv.Value.itemId != itemId) continue;
                count++;
            }
            return count;
        }

        public bool HasInstance(int instanceId)
        {
            return equipmentDataDict.ContainsKey(instanceId);
        }

        public List<EquipmentData> BuildView(Enum_InventorySort sort)
        {
            var itemChart = GameChart.Get<ItemChart>();
            var equipChart = GameChart.Get<EquipChart>();

            int RarityKey(EquipmentData e) => (int)(itemChart?.Get(e.itemId)?.Rarity ?? 0);
            int EquipKey(EquipmentData e) => (int)(equipChart?.Get(e.itemId)?.EquipType ?? Enum_EquipType.Max);

            List<EquipmentData> list = new List<EquipmentData>(equipmentDataDict.Values);

            switch (sort)
            {
                case Enum_InventorySort.TypeSort:
                    list.Sort((x, y) =>
                    {
                        if (EquipKey(x) < EquipKey(y)) return -1;
                        if (EquipKey(x) > EquipKey(y)) return 1;

                        if (RarityKey(x) < RarityKey(y)) return 1;
                        if (RarityKey(x) > RarityKey(y)) return -1;

                        if (x.itemId < y.itemId) return -1;
                        if (x.itemId > y.itemId) return 1;

                        return x.instanceId.CompareTo(y.instanceId);
                    });
                    break;

                case Enum_InventorySort.RaritySort:
                    list.Sort((x, y) =>
                    {
                        if (RarityKey(x) < RarityKey(y)) return 1;
                        if (RarityKey(x) > RarityKey(y)) return -1;

                        if (EquipKey(x) < EquipKey(y)) return -1;
                        if (EquipKey(x) > EquipKey(y)) return 1;

                        if (x.itemId < y.itemId) return -1;
                        if (x.itemId > y.itemId) return 1;

                        return x.instanceId.CompareTo(y.instanceId);
                    });
                    break;

                default:
                    list.Sort((x, y) => x.instanceId.CompareTo(y.instanceId));
                    break;
            }

            return list;
        }
    }
}
