using LitJson;
using BackEnd;
using System;
using System.Collections.Generic;

namespace GameBerry.Table
{
    public class PointTable : TableBase
    {
        private const string pointKey = "Point";
        private Dictionary<int, long> points = new Dictionary<int, long>();

        public override void SetData(JsonData data)
        {
            if (data == null || data.Count == 0) return;

            for (int i = 0; i < data.Count; ++i)
            {
                foreach (var key in data[i].Keys)
                {
                    if (key == "inDate") SetInData(data[i][key].ToString());
                    else if (key == pointKey) points = PackUtil.UnpackPrimitiveDict<int, long>(data[i][key].ToString());
                }
            }
        }

        public override Param GetParam()
        {
            Param p = new Param();
            p.Add(pointKey, PackUtil.PackPrimitiveDict(points));
            return p;
        }

        public long GetAmount(int pointId) => points.TryGetValue(pointId, out var v) ? v : 0;

        public void Add(int pointId, long amount)
        {
            if (amount == 0) return;
            long next = GetAmount(pointId) + amount;
            if (next < 0) next = 0;
            points[pointId] = next;
        }
    }
}
