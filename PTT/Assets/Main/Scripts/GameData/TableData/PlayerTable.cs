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
        private int _jobId = 0;

        private const string levelKey = "Lv";
        private int _level = 1;

        private const string expKey = "Exp";
        private double _exp = 0;

        //------------------------------------------------------------------------------------
        public override void SetData(JsonData data)
        {
            if (data == null || data.Count == 0) return;

            for (int i = 0; i < data.Count; ++i)
            {
                foreach (var key in data[i].Keys)
                {
                    if (key == "inDate") SetInData(data[i][key].ToString());
                    else if (key == jobKey) _jobId = PackUtil.UnpackValue<int>(data[i][key].ToString());
                    else if (key == levelKey) _level = PackUtil.UnpackValue<int>(data[i][key].ToString());
                    else if (key == expKey) _exp = PackUtil.UnpackValue<double>(data[i][key].ToString());
                }
            }
        }
        //------------------------------------------------------------------------------------
        public override Param GetParam()
        {
            Param p = new Param();
            p.Add(jobKey, PackUtil.PackValue(_jobId));
            p.Add(levelKey, PackUtil.PackValue(_level));
            p.Add(expKey, PackUtil.PackValue(_exp));
            return p;
        }
        //------------------------------------------------------------------------------------
        public int GetLevel() => _level;
        public double GetExp() => _exp;
        public int GetJobId() => _jobId;
        //------------------------------------------------------------------------------------
        public void SetLevel(int level, bool immediate = true)
        {
            _level = level;
            if (immediate)
                UpdateTable();
        }
        //------------------------------------------------------------------------------------
        public void SetExp(double exp, bool immediate = true)
        {
            _exp = exp;
            if (immediate)
                UpdateTable();
        }
        //------------------------------------------------------------------------------------
        public void SetJobId(int jobId, bool immediate = true)
        {
            _jobId = jobId;
            if (immediate)
                UpdateTable();
        }
        //------------------------------------------------------------------------------------
    }
}