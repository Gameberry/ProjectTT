using LitJson;
using BackEnd;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameBerry.Table
{
    public class SkinData
    {
        public int index;
        public bool visible = false;
    }

    public class SkinTable : TableBase
    {
        private readonly Type[] SaveTargets = new Type[]
{
        typeof(SkinTable),
// 필요한 table을 여기에 추가하면 끝
};

        public const string SkinEquipDataKey = "SkinEquip";

        public Dictionary<SkinSlotType, SkinData> SkinEquipData = new Dictionary<SkinSlotType, SkinData>();

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
                            //SkinEquipData = data[i][key].ToString().ToInt(); 데이터 셋팅
                        }
                    }
                }
            }
        }

        public override Param GetParam()
        {
            Param param = new Param();
            param.Add(SkinEquipDataKey, SkinEquipData.ToArray().ToString());

            return param;
        }
    }

}