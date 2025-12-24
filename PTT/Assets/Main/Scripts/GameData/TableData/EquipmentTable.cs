using LitJson;
using BackEnd;
using System.Collections.Generic;

namespace GameBerry.Table
{
    public class EquipSlotData : IPackable
    {
        public int slot; // (int)Enum_EquipType
        public string instanceId;

        public string Pack() => $"{slot},{instanceId}";
        public void Unpack(string str)
        {
            if (string.IsNullOrEmpty(str)) return;
            var sp = str.Split(',');
            if (sp.Length > 0) int.TryParse(sp[0], out slot);
            if (sp.Length > 1) instanceId = sp[1] ?? string.Empty;
        }
    }

    public class EquipmentTable : TableBase
    {
        private const string equippedKey = "Equipped";
        private List<EquipSlotData> equipped = new List<EquipSlotData>();

        public override void SetData(JsonData data)
        {
            if (data == null || data.Count == 0) return;

            for (int i = 0; i < data.Count; ++i)
            {
                foreach (var key in data[i].Keys)
                {
                    if (key == "inDate") SetInData(data[i][key].ToString());
                    else if (key == equippedKey) equipped = PackUtil.UnpackList<EquipSlotData>(data[i][key].ToString());
                }
            }
        }

        public override Param GetParam()
        {
            Param p = new Param();
            p.Add(equippedKey, PackUtil.PackList(equipped));
            return p;
        }

        public string GetEquippedInstanceId(GameBerry.Enum_EquipType slot)
        {
            int s = (int)slot;
            var d = equipped.Find(x => x.slot == s);
            return d != null ? (d.instanceId ?? string.Empty) : string.Empty;
        }

        public void SetEquipped(GameBerry.Enum_EquipType slot, string instanceId)
        {
            int s = (int)slot;
            var d = equipped.Find(x => x.slot == s);
            if (d == null)
            {
                equipped.Add(new EquipSlotData { slot = s, instanceId = instanceId ?? string.Empty });
                return;
            }
            d.instanceId = instanceId ?? string.Empty;
        }

        public bool IsEquipped(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId)) return false;
            for (int i = 0; i < equipped.Count; i++)
                if (equipped[i] != null && equipped[i].instanceId == instanceId) return true;
            return false;
        }
    }
}
