using LitJson;
using BackEnd;
using System;
using System.Collections.Generic;

namespace GameBerry.Table
{
    public class SkinData : ItemData, IPackable
    {
        public bool awake = false;

        public string Pack()
        {
            return $"{PackUtil.PackValue(itemId)},{PackUtil.PackValue(awake ? 1 : 0)}";
        }

        public void Unpack(string str)
        {
            if (string.IsNullOrEmpty(str))
                return;

            var split = str.Split(',');
            if (split.Length > 0)
                itemId = PackUtil.UnpackValue<int>(split[0]);
            if (split.Length > 1)
                awake = split[1] == "1";
        }
    }

    public class SkinTable : TableBase
    {
        private const string hasSkinListKey = "Skin";
        private List<SkinData> hasSkinList = new List<SkinData>();

        private const string equipSkinKey = "Equip";
        private Dictionary<Enum_SkinSlotType, int> equipSkinDict = new Dictionary<Enum_SkinSlotType, int>();

        public override void SetData(LitJson.JsonData data)
        {
            if (data == null || data.Count == 0)
                return;

            for (int i = 0; i < data.Count; ++i)
            {
                foreach (var key in data[i].Keys)
                {
                    if (key == "inDate")
                    {
                        SetInData(data[i][key].ToString());
                    }
                    else if (key == hasSkinListKey)
                    {
                        hasSkinList = PackUtil.UnpackList<SkinData>(data[i][key].ToString());
                    }
                    else if (key == equipSkinKey)
                    {
                        equipSkinDict = PackUtil.UnpackPrimitiveDict<Enum_SkinSlotType, int>(data[i][key].ToString());
                    }
                }
            }
        }

        public override BackEnd.Param GetParam()
        {
            BackEnd.Param param = new BackEnd.Param();
            param.Add(hasSkinListKey, PackUtil.PackList(hasSkinList));
            param.Add(equipSkinKey, PackUtil.PackPrimitiveDict(equipSkinDict));
            return param;
        }

        public void CapyEquipSkinDict(ref Dictionary<Enum_SkinSlotType, int> data)
        {
            data.Clear();
            foreach (var pair in equipSkinDict)
                data[pair.Key] = pair.Value;
        }

        public SkinData GetSkinData(int itemId)
        {
            return hasSkinList.Find(x => x.itemId == itemId);
        }

        public bool IsUnlocked(int itemId)
        {
            var d = GetSkinData(itemId);
            return d != null;
        }

        public SkinData GetSkinEquipData(Enum_SkinSlotType skinSlotType)
        {
            if (equipSkinDict.TryGetValue(skinSlotType, out int index))
                return GetSkinData(index);

            return null;
        }

        public void UnequipSlotSkin(Enum_SkinSlotType slot)
        {
            if (equipSkinDict.ContainsKey(slot) == false)
                return;

            equipSkinDict.Remove(slot);
        }

        public bool EquipSlotSkin(Enum_SkinSlotType slot, int itemId)
        {
            // 가드: 해금되지 않은 스킨은 장착 불가
            if (!IsUnlocked(itemId))
                return false;

            equipSkinDict[slot] = itemId;
            return true;
        }

        public bool AddSkin(int itemId)
        {
            var d = GetSkinData(itemId);
            if (d != null)
                return false;

            hasSkinList.Add(new SkinData { itemId = itemId, count = 1, awake = false });
            return true;
        }

        public bool SetAwakeSkin(int itemId)
        {
            var d = GetSkinData(itemId);
            if (d != null)
            {
                if (d.awake) return false;
                d.awake = true;
                return true;
            }

            return false;
        }

        public void EnsureDefaultUnlocked(int itemId)
        {
            AddSkin(itemId);
        }
    }
}
