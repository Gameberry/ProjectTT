using System.Collections.Generic;
using LitJson;
using BackEnd;
using System.Text;
namespace GameBerry.Table
{
    public class SummonStateData : IPackable
    {
        public int summonType;
        public int level = 1;
        public int exp = 0;
        public int dailyAdViewCount = 0;
        public HashSet<int> claimedRewardLevels = new HashSet<int>();

        public string Pack()
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (int rewardLevel in claimedRewardLevels)
            {
                if (first == false)
                    sb.Append('|');
                first = false;
                sb.Append(PackUtil.PackValue(rewardLevel));
            }

            return $"{PackUtil.PackValue(summonType)},{PackUtil.PackValue(level)},{PackUtil.PackValue(exp)},{PackUtil.PackValue(dailyAdViewCount)},{sb}";
        }

        public void Unpack(string str)
        {
            summonType = 0;
            level = 1;
            exp = 0;
            dailyAdViewCount = 0;
            claimedRewardLevels.Clear();

            if (string.IsNullOrEmpty(str))
                return;

            var sp = str.Split(',');
            if (sp.Length > 0) summonType = PackUtil.UnpackValue<int>(sp[0]);
            if (sp.Length > 1) level = PackUtil.UnpackValue<int>(sp[1]);
            if (sp.Length > 2) exp = PackUtil.UnpackValue<int>(sp[2]);
            if (sp.Length > 3 && sp.Length <= 4)
            {
                if (string.IsNullOrEmpty(sp[3]) == false)
                {
                    string[] claimedSplit = sp[3].Split('|');
                    for (int i = 0; i < claimedSplit.Length; ++i)
                    {
                        if (string.IsNullOrEmpty(claimedSplit[i]))
                            continue;

                        claimedRewardLevels.Add(PackUtil.UnpackValue<int>(claimedSplit[i]));
                    }
                }
            }
            else if (sp.Length > 3)
            {
                dailyAdViewCount = PackUtil.UnpackValue<int>(sp[3]);
            }

            if (sp.Length > 4 && string.IsNullOrEmpty(sp[4]) == false)
            {
                string[] claimedSplit = sp[4].Split('|');
                for (int i = 0; i < claimedSplit.Length; ++i)
                {
                    if (string.IsNullOrEmpty(claimedSplit[i]))
                        continue;

                    claimedRewardLevels.Add(PackUtil.UnpackValue<int>(claimedSplit[i]));
                }
            }
        }
    }

    public class SummonTable : TableBase
    {
        private const string summonStateKey = "SummonState";
        private const string dailyResetTimestampKey = "SummonDailyResetTimestamp";
        private List<SummonStateData> _states = new List<SummonStateData>();
        private double _dailyResetTimestamp = 0;
        private bool _dirtyAfterLoad = false;

        public override void SetData(JsonData data)
        {
            if (data == null || data.Count == 0)
                return;

            for (int i = 0; i < data.Count; ++i)
            {
                foreach (var key in data[i].Keys)
                {
                    if (key == "inDate") SetInData(data[i][key].ToString());
                    else if (key == summonStateKey) _states = PackUtil.UnpackList<SummonStateData>(data[i][key].ToString());
                    else if (key == dailyResetTimestampKey) _dailyResetTimestamp = PackUtil.UnpackValue<double>(data[i][key].ToString());
                }
            }

            if (Managers.TimeManager.isAlive)
            {
                if (EnsureDailyReset(Managers.TimeManager.Instance.Current_TimeStamp, Managers.TimeManager.Instance.DailyInit_TimeStamp))
                    _dirtyAfterLoad = true;
            }
        }

        public override Param GetParam()
        {
            Param p = new Param();
            p.Add(summonStateKey, PackUtil.PackList(_states));
            p.Add(dailyResetTimestampKey, PackUtil.PackValue(_dailyResetTimestamp));
            return p;
        }

        public int GetLevel(Enum_SummonType summonType)
        {
            SummonStateData state = _states.Find(x => x.summonType == (int)summonType);
            if (state == null)
                return 1;

            return state.level < 1 ? 1 : state.level;
        }

        public int GetExp(Enum_SummonType summonType)
        {
            SummonStateData state = _states.Find(x => x.summonType == (int)summonType);
            if (state == null)
                return 0;

            return state.exp < 0 ? 0 : state.exp;
        }

        public void SetState(Enum_SummonType summonType, int level, int exp)
        {
            SummonStateData state = _states.Find(x => x.summonType == (int)summonType);
            if (state == null)
            {
                state = new SummonStateData { summonType = (int)summonType, level = 1, exp = 0 };
                _states.Add(state);
            }

            state.level = level < 1 ? 1 : level;
            state.exp = exp < 0 ? 0 : exp;
        }

        public int GetDailyAdViewCount(Enum_SummonType summonType)
        {
            SummonStateData state = _states.Find(x => x.summonType == (int)summonType);
            if (state == null)
                return 0;

            return state.dailyAdViewCount < 0 ? 0 : state.dailyAdViewCount;
        }

        public void SetDailyAdViewCount(Enum_SummonType summonType, int count)
        {
            SummonStateData state = _states.Find(x => x.summonType == (int)summonType);
            if (state == null)
            {
                state = new SummonStateData { summonType = (int)summonType, level = 1, exp = 0 };
                _states.Add(state);
            }

            state.dailyAdViewCount = count < 0 ? 0 : count;
        }

        public bool TryConsumeDailyAdView(Enum_SummonType summonType, int dailyLimit)
        {
            if (dailyLimit <= 0)
                return false;

            SummonStateData state = _states.Find(x => x.summonType == (int)summonType);
            if (state == null)
            {
                state = new SummonStateData { summonType = (int)summonType, level = 1, exp = 0 };
                _states.Add(state);
            }

            if (state.dailyAdViewCount >= dailyLimit)
                return false;

            state.dailyAdViewCount += 1;
            return true;
        }

        public bool EnsureDailyReset(double currentTimestamp, double nextDailyResetTimestamp)
        {
            bool changed = false;
            if (_dailyResetTimestamp <= 0)
            {
                _dailyResetTimestamp = nextDailyResetTimestamp;
                changed = true;
            }

            if (_dailyResetTimestamp > 0 && currentTimestamp >= _dailyResetTimestamp)
            {
                for (int i = 0; i < _states.Count; ++i)
                {
                    _states[i].dailyAdViewCount = 0;
                }

                _dailyResetTimestamp = nextDailyResetTimestamp;
                changed = true;
            }

            return changed;
        }

        public void OnDailyReset(double nextDailyResetTimestamp)
        {
            for (int i = 0; i < _states.Count; ++i)
            {
                _states[i].dailyAdViewCount = 0;
            }

            _dailyResetTimestamp = nextDailyResetTimestamp;
        }

        public bool ConsumeDirtyAfterLoad()
        {
            bool dirty = _dirtyAfterLoad;
            _dirtyAfterLoad = false;
            return dirty;
        }

        public bool IsRewardClaimed(Enum_SummonType summonType, int rewardLevel)
        {
            SummonStateData state = _states.Find(x => x.summonType == (int)summonType);
            if (state == null)
                return false;

            return state.claimedRewardLevels.Contains(rewardLevel);
        }

        public bool ClaimReward(Enum_SummonType summonType, int rewardLevel)
        {
            SummonStateData state = _states.Find(x => x.summonType == (int)summonType);
            if (state == null)
            {
                state = new SummonStateData { summonType = (int)summonType, level = 1, exp = 0 };
                _states.Add(state);
            }

            if (state.claimedRewardLevels.Contains(rewardLevel))
                return false;

            state.claimedRewardLevels.Add(rewardLevel);
            return true;
        }
    }
}
