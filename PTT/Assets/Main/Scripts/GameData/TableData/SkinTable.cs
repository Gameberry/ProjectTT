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
        public const string SkinEquipDataKey = "SkinEquip";

        public Dictionary<SkinSlotType, SkinData> SkinEquipData = new Dictionary<SkinSlotType, SkinData>();

        public List<int> tsett = new List<int>();

        public List<SkinData> SkinDataddd = new List<SkinData>();

        public int test = 0;

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
                        else if (key == SkinEquipDataKey)
                        {
                            SkinEquipData = PackUtil.UnpackDict<SkinSlotType, SkinData>(data[i][key].ToString());
                        }
                        else if (key == "tsett")
                        {
                            tsett = PackUtil.UnpackPrimitiveList<int>(data[i][key].ToString());
                        }
                        else if (key == "SkinDataddd")
                        {
                            SkinDataddd = PackUtil.UnpackList<SkinData>(data[i][key].ToString());
                        }
                        else if (key == "test")
                        {
                            test = data[i][key].ToString().ToInt();
                            UnityEngine.Debug.Log("SkinTest " + test);
                        }
                    }
                }
            }
        }

        public override Param GetParam()
        {
            Param param = new Param();
            param.Add(SkinEquipDataKey, PackUtil.PackDict(SkinEquipData));
            param.Add("tsett", PackUtil.PackPrimitiveList(tsett));
            param.Add("SkinDataddd", PackUtil.PackList(SkinDataddd));
            param.Add("test", test);

            return param;
        }
    }

}