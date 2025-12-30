using LitJson;
using BackEnd;
using System.Collections.Generic;

namespace GameBerry.Table
{
    public class EquipSlotData : IPackable
    {
        public int slot; // (int)Enum_EquipType
        public int instanceId;

        public string Pack() => $"{slot},{instanceId}";
        public void Unpack(string str)
        {
            instanceId = 0;

            if (string.IsNullOrEmpty(str)) return;
            var sp = str.Split(',');
            if (sp.Length > 0) int.TryParse(sp[0], out slot);
            if (sp.Length > 1) int.TryParse(sp[1], out instanceId);
        }
    }

    public class EquipmentData : IPackable
    {
        public int instanceId;
        public int enhanceLevel;

        public Dictionary<V2Enum_Stat, double> addStat;

        public string Pack() => $"{instanceId},{enhanceLevel}:{PackUtil.PackPrimitiveDict(addStat)}";
        public void Unpack(string str)
        {
            if (string.IsNullOrEmpty(str)) return;

            var tsp = str.Split(':');

            if (tsp.Length > 0)
            {
                var sp = tsp[0].Split(',');
                if (sp.Length > 0) int.TryParse(sp[0], out instanceId);
                if (sp.Length > 1) int.TryParse(sp[1], out enhanceLevel);
            }

            if (tsp.Length > 1 && string.IsNullOrEmpty(tsp[1]) == false)
                addStat = PackUtil.UnpackPrimitiveDict<V2Enum_Stat, double>(tsp[1]);
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

        public bool AddEquipment(int instanceId)
        {
            if (equipmentDataDict.ContainsKey(instanceId) == true)
                return false;

            equipmentDataDict.Add(instanceId, new EquipmentData { instanceId = instanceId });

            return true;
        }

        public int GetEquippedInstanceId(GameBerry.Enum_EquipType slot)
        {
            int s = (int)slot;
            var d = equipped.Find(x => x.slot == s);
            return d != null ? d.instanceId : 0;
        }

        public void SetEquipped(GameBerry.Enum_EquipType slot, int instanceId)
        {
            int s = (int)slot;
            var d = equipped.Find(x => x.slot == s);
            if (d == null)
            {
                equipped.Add(new EquipSlotData { slot = s, instanceId = instanceId });
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

                if(immediate == true) 
                    UpdateTable();

                return true;
            }
            
            return false;
        }
    }
}
