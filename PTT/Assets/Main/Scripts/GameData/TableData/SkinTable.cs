using LitJson;
using BackEnd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace GameBerry.Table
{
    public class SkinData : IPackable
    {
        public int index;
        public bool visible = false;

        public string Pack()
        {
            return $"{index},{(visible ? 1 : 0)}";
        }

        public void Unpack(string str)
        {
            if (string.IsNullOrEmpty(str))
                return;

            var split = str.Split(',');
            if (split.Length > 0)
                int.TryParse(split[0], out index);
            if (split.Length > 1)
                visible = split[1] == "1";
        }
    }

    public class SkinTable : TableBase
    {
        private const string hasSkinListKey = "Skin";
        private List<SkinData> hasSkinList = new List<SkinData>();

        private const string equipSkinKey = "Equip";
        private Dictionary<SkinSlotType, int> equipSkinDict = new Dictionary<SkinSlotType, int>();

        //------------------------------------------------------------------------------------
        public override void SetData(JsonData data)
        {
            if (data.Count == 0)
                return;
            else
            {
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
                            equipSkinDict = PackUtil.UnpackPrimitiveDict<SkinSlotType, int>(data[i][key].ToString());
                        }
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public override Param GetParam()
        {
            Param param = new Param();
            param.Add(hasSkinList, PackUtil.PackList(hasSkinList));
            param.Add(equipSkinKey, PackUtil.PackPrimitiveDict(equipSkinDict));

            return param;
        }
        //------------------------------------------------------------------------------------
        public void CapyEquipSkinDict(ref Dictionary<SkinSlotType, int> data)
        {
            foreach (var pair in equipSkinDict)
            {
                data.Add(pair.Key, pair.Value);
            }
        }
        //------------------------------------------------------------------------------------
        public SkinData GetSkinData(int index)
        {
            return hasSkinList.Find(x => x.index == index);
        }
        //------------------------------------------------------------------------------------
        public SkinData GetSkinEquipData(SkinSlotType skinSlotType)
        {
            if (equipSkinDict.TryGetValue(skinSlotType, out int index))
                return GetSkinData(index);

            return null;
        }
        //------------------------------------------------------------------------------------
        public void UnequipSlotSkin(SkinSlotType slot)
        {
            if (equipSkinDict.ContainsKey(slot) == false)
                return;

            equipSkinDict.Remove(slot);
        }
        //------------------------------------------------------------------------------------
        public void EquipSlotSkin(SkinSlotType slot, int index)
        {
            if (equipSkinDict.ContainsKey(slot) == false)
                return;

            SkinData skinData = GetSkinData(index);

            if (skinData == null)
                return;

            equipSkinDict[slot] = skinData.index;
        }
        //------------------------------------------------------------------------------------
        public SkinData CreateNewSkinData(Chart.SkinInfo skinInfo)
        {
            if (skinInfo == null)
                return null;

            SkinData skinData = new SkinData();
            skinData.index = skinInfo.Index;

            hasSkinList.Add(skinData);

            return skinData;
        }
        //------------------------------------------------------------------------------------
    }

}