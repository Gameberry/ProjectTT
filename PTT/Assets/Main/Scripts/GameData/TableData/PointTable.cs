using LitJson;
using BackEnd;
using System;
using System.Collections.Generic;

namespace GameBerry.Table
{
    public class PointData : ItemData, IPackable
    {
        public string Pack() => $"{itemId},{count}";

        public void Unpack(string str)
        {
            itemId = 0;
            instanceId = 0;
            count = 0;

            if (string.IsNullOrEmpty(str))
                return;

            var sp = str.Split(',');
            if (sp.Length > 0) int.TryParse(sp[0], out itemId);
            if (sp.Length > 1) long.TryParse(sp[1], out count);
        }
    }

    public class PointTable : TableBase
    {
        private const string pointKey = "Point";
        private List<PointData> points = new ();

        

        //------------------------------------------------------------------------------------
        public override void SetData(JsonData data)
        {
            if (data == null || data.Count == 0) return;

            for (int i = 0; i < data.Count; ++i)
            {
                foreach (var key in data[i].Keys)
                {
                    if (key == "inDate") SetInData(data[i][key].ToString());
                    else if (key == pointKey) points = PackUtil.UnpackList<PointData>(data[i][key].ToString());
                }
            }
        }
        //------------------------------------------------------------------------------------
        public override Param GetParam()
        {
            Param p = new Param();
            p.Add(pointKey, PackUtil.PackList(points));
            return p;
        }
        //------------------------------------------------------------------------------------
        public long GetAmount(int pointId)
        {
            PointData pointData = points.Find(x => x.itemId == pointId);
            if (pointData == null)
                return 0;

            return pointData.count;
        }
        //------------------------------------------------------------------------------------
        public void Add(int pointId, long amount)
        {
            if (amount == 0) return;

            PointData pointData = points.Find(x => x.itemId == pointId);

            if (pointData == null)
            {
                PointData newPoint = new PointData { itemId = pointId, count = 0 };
                pointData = newPoint;
                points.Add(newPoint);
            }

            long next = pointData.count + amount;
            if (next < 0) next = 0;

            pointData.count = next;
        }
        //------------------------------------------------------------------------------------
    }
}
