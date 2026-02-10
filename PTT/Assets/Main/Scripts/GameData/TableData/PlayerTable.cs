using LitJson;
using BackEnd;
using System;
using System.Collections.Generic;

namespace GameBerry.Table
{
    public class PlayerTable : TableBase
    {
        // 전직 단계
        private const string jobKey = "Job";
        private int Job = 0;


        //------------------------------------------------------------------------------------
        public override void SetData(JsonData data)
        {
            if (data == null || data.Count == 0) return;

            for (int i = 0; i < data.Count; ++i)
            {
                foreach (var key in data[i].Keys)
                {
                    if (key == "inDate") SetInData(data[i][key].ToString());
                    else if (key == jobKey) Job = PackUtil.UnpackValue<int>(data[i][key].ToString());
                }
            }
        }
        //------------------------------------------------------------------------------------
        public override Param GetParam()
        {
            Param p = new Param();
            p.Add(jobKey, PackUtil.PackValue(Job));
            return p;
        }
        //------------------------------------------------------------------------------------
    }
}