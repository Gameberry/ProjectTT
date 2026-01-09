using LitJson;
using BackEnd;
using System.Collections.Generic;
using GameBerry.Chart;

namespace GameBerry.Table
{
    public class EquipSlotData : IPackable
    {
        public Enum_EquipType slot; // (int)Enum_EquipType
        public int instanceId;

        public string Pack() => $"{PackUtil.PackValue(slot.Enum32ToInt())},{PackUtil.PackValue(instanceId)}";
        public void Unpack(string str)
        {
            instanceId = 0;

            if (string.IsNullOrEmpty(str))
                return;

            var sp = str.Split(',');

            if (sp.Length >= 1)
                slot = PackUtil.UnpackValue<int>(sp[0]).IntToEnum32<Enum_EquipType>();

            if (sp.Length >= 2)
                instanceId = PackUtil.UnpackValue<int>(sp[1]);
        }
    }

    public struct EquipmentAddStat : IPackable
    {
        public V2Enum_Stat stat;
        public double value;
        public string Pack() => $"{PackUtil.PackValue(stat.Enum32ToInt())},{PackUtil.PackValue(value)}";

        public void Unpack(string str)
        {
            if (string.IsNullOrEmpty(str))
                return;

            var sp = str.Split(',');

            if (sp.Length >= 1)
                stat = PackUtil.UnpackValue<int>(sp[0]).IntToEnum32<V2Enum_Stat>();

            if (sp.Length >= 2)
                value = PackUtil.UnpackValue<double>(sp[1]);
        }

        public static EquipmentAddStat Set(V2Enum_Stat Stat, double Value)
            => new EquipmentAddStat
            {
                stat = Stat,
                value = Value
            };
    }

    public class EquipmentData : IPackable
    {
        public int instanceId;
        public int enhanceLevel;

        public List<EquipmentAddStat> addStatList;

        public string Pack() => $"{PackUtil.PackValue(instanceId)},{PackUtil.PackValue(enhanceLevel)}:{PackUtil.PackList(addStatList)}";
        public void Unpack(string str)
        {
            if (string.IsNullOrEmpty(str)) return;

            var tsp = str.Split(':');

            if (tsp.Length > 0)
            {
                var sp = tsp[0].Split(',');
                if (sp.Length > 0) instanceId = PackUtil.UnpackValue<int>(sp[0]);
                if (sp.Length > 1) enhanceLevel = PackUtil.UnpackValue<int>(sp[1]);
            }

            if (tsp.Length > 1 && string.IsNullOrEmpty(tsp[1]) == false)
                addStatList = PackUtil.UnpackList<EquipmentAddStat>(tsp[1]);
        }
    }

    public class EquipmentTable : TableBase
    {
        private const string equippedKey = "Equipped";
        private List<EquipSlotData> equipped = new List<EquipSlotData>();

        private const string equipmentDataKey = "Equipment";
        private Dictionary<int, EquipmentData> equipmentDataDict = new Dictionary<int, EquipmentData>();

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
        }

        public override Param GetParam()
        {
            Param p = new Param();
            p.Add(equippedKey, PackUtil.PackList(equipped));
            p.Add(equipmentDataKey, PackUtil.PackDict(equipmentDataDict));
            return p;
        }

        public bool AddEquipment(EquipmentData equipmentData)
        {
            if (equipmentDataDict.ContainsKey(equipmentData.instanceId) == true)
                return false;

            equipmentDataDict.Add(equipmentData.instanceId, equipmentData);

            return true;
        }

        public bool RemoveEquipment(int instanceId)
        {
            if (equipmentDataDict.ContainsKey(instanceId) == false)
                return true;

            equipmentDataDict.Remove(instanceId);

            return true;
        }

        public int GetEquippedInstanceId(GameBerry.Enum_EquipType slot)
        {
            var d = equipped.Find(x => x.slot == slot);
            return d != null ? d.instanceId : 0;
        }

        public void SetEquipped(GameBerry.Enum_EquipType slot, int instanceId)
        {
            var d = equipped.Find(x => x.slot == slot);
            if (d == null)
            {
                equipped.Add(new EquipSlotData { slot = slot, instanceId = instanceId });
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

        public int GetLevel(int instanceId)
        {
            if (equipmentDataDict.TryGetValue(instanceId, out var data))
                return data.enhanceLevel;

            return 0;
        }

        public void Enhance(List<int> instanceIds, int addLevel = 1)
        {
            foreach (var id in instanceIds)
            {
                Enhance(id, addLevel, false);
            }

            UpdateTable();
        }

        public bool Enhance(int instanceId, int addLevel = 1, bool immediate = true)
        {
            if (equipmentDataDict.TryGetValue(instanceId, out var data))
            {
                data.enhanceLevel += addLevel;

                if (immediate == true)
                    UpdateTable();

                return true;
            }

            return false;
        }
    }
}
