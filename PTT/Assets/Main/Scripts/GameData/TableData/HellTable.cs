using LitJson;
using BackEnd;

namespace GameBerry.Table
{
    public class HellStateData : IPackable
    {
        public int level = 1;
        public int exp = 0;
        public long levelUpEndTimeSec = 0;

        public string Pack() => $"{PackUtil.PackValue(level)},{PackUtil.PackValue(exp)},{PackUtil.PackValue(levelUpEndTimeSec)}";

        public void Unpack(string str)
        {
            level = 1;
            exp = 0;
            levelUpEndTimeSec = 0;

            if (string.IsNullOrEmpty(str))
                return;

            string[] sp = str.Split(',');
            if (sp.Length > 0)
                level = PackUtil.UnpackValue<int>(sp[0]);
            if (sp.Length > 1)
                exp = PackUtil.UnpackValue<int>(sp[1]);
            if (sp.Length > 2)
                levelUpEndTimeSec = PackUtil.UnpackValue<long>(sp[2]);
        }
    }

    public class HellTable : TableBase
    {
        private const string hellStateKey = "HellState";
        private HellStateData _state = new HellStateData();

        public override void SetData(JsonData data)
        {
            if (data == null || data.Count == 0)
                return;

            for (int i = 0; i < data.Count; ++i)
            {
                foreach (var key in data[i].Keys)
                {
                    if (key == "inDate")
                        SetInData(data[i][key].ToString());
                    else if (key == hellStateKey)
                    {
                        _state = new HellStateData();
                        _state.Unpack(data[i][key].ToString());
                    }
                }
            }

            EnsureState();
        }

        public override Param GetParam()
        {
            Param p = new Param();
            p.Add(hellStateKey, _state?.Pack() ?? string.Empty);
            return p;
        }

        public int GetLevel()
        {
            EnsureState();
            return _state.level < 1 ? 1 : _state.level;
        }

        public int GetExp()
        {
            EnsureState();
            return _state.exp < 0 ? 0 : _state.exp;
        }

        public void SetState(int level, int exp)
        {
            EnsureState();
            _state.level = level < 1 ? 1 : level;
            _state.exp = exp < 0 ? 0 : exp;
        }

        public long GetLevelUpEndTimeSec()
        {
            EnsureState();
            return _state.levelUpEndTimeSec < 0 ? 0 : _state.levelUpEndTimeSec;
        }

        public void SetState(int level, int exp, long levelUpEndTimeSec)
        {
            EnsureState();
            _state.level = level < 1 ? 1 : level;
            _state.exp = exp < 0 ? 0 : exp;
            _state.levelUpEndTimeSec = levelUpEndTimeSec < 0 ? 0 : levelUpEndTimeSec;
        }

        private void EnsureState()
        {
            if (_state == null)
                _state = new HellStateData();
        }
    }
}
