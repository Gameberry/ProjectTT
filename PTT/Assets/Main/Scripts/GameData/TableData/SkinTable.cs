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
        public const string SkinEquipDataKey = "SkinEquip";

        public Dictionary<SkinSlotType, SkinData> SkinEquipData = new Dictionary<SkinSlotType, SkinData>();

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
                        //else if (key == SkinEquipDataKey)
                        //{
                        //    //SkinEquipData = data[i][key].ToString().ToInt(); 데이터 셋팅
                        //}
                        else if (key == "test")
                        {
                            test = data[i][key].ToString().ToInt();
                            UnityEngine.Debug.Log("SkinTest " + test);
                        }
                    }
                }
            }

            test++;
        }

        public override Param GetParam()
        {
            Param param = new Param();
            //param.Add(SkinEquipDataKey, SkinEquipData.ToArray().ToString());
            param.Add("test", test);

            return param;
        }
    }

}