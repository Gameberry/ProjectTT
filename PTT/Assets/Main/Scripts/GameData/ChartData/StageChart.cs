using System.Collections.Generic;

namespace GameBerry.Chart
{
    public class StageInfo
    {
        public int Chapter;
        public int Stage;
        public int FieldMonsterKey;
        public int[] FieldMonsterModel;
        public int FieldMonsterCount;
        public int ResponTime;
        public int BossSubMonster;
        public int BossSubMonsterModel;
        public int BossSubMonsterCount;
        public int BossMonster;
        public int BossMonsterModel;
        public int BossTime;
        public int Exp;
        public int Gold;
        public int EquipLevelMin;
        public int EquipLevelMax;
        public double EquipDropRate;
        public int[] EquipList;
    }

    public class StageChart : ChartBase
    {
        public StageInfo this[int index] => rows[index];
        public StageInfo[] rows;
        private Dictionary<(int chapter, int stage), StageInfo> _infoByKey;
        private Dictionary<int, int> _maxStageByChapter;
        private Dictionary<int, List<StageInfo>> _infosByChapter;
        private List<int> _chapters;

        public override bool IsLoaded()
        {
            return rows != null;
        }

        public override void LoadComplete()
        {
            _infoByKey = new Dictionary<(int chapter, int stage), StageInfo>();
            _maxStageByChapter = new Dictionary<int, int>();
            _infosByChapter = new Dictionary<int, List<StageInfo>>();
            _chapters = new List<int>();

            if (rows == null)
                return;

            for (int i = 0; i < rows.Length; ++i)
            {
                StageInfo info = rows[i];
                _infoByKey[(info.Chapter, info.Stage)] = info;

                 if (_infosByChapter.TryGetValue(info.Chapter, out List<StageInfo> infos) == false)
                {
                    infos = new List<StageInfo>();
                    _infosByChapter.Add(info.Chapter, infos);
                    _chapters.Add(info.Chapter);
                }

                infos.Add(info);

                if (_maxStageByChapter.TryGetValue(info.Chapter, out int maxStage) == false || info.Stage > maxStage)
                    _maxStageByChapter[info.Chapter] = info.Stage;
            }

            _chapters.Sort();
            foreach (List<StageInfo> infos in _infosByChapter.Values)
            {
                infos.Sort((lhs, rhs) => lhs.Stage.CompareTo(rhs.Stage));
            }
        }

        public bool TryGetInfo(int chapter, int stage, out StageInfo info)
        {
            info = default;
            return _infoByKey != null && _infoByKey.TryGetValue((chapter, stage), out info);
        }

        public int GetMaxStage(int chapter)
        {
            if (_maxStageByChapter != null && _maxStageByChapter.TryGetValue(chapter, out int maxStage))
                return maxStage;

            return 0;
        }

        public bool TryGetNext(int chapter, int stage, out StageInfo nextInfo)
        {
            nextInfo = null;
            if (TryGetInfo(chapter, stage + 1, out nextInfo))
                return true;

            return TryGetInfo(chapter + 1, 1, out nextInfo);
        }

        public IReadOnlyList<int> GetChapters()
        {
            return _chapters;
        }

        public IReadOnlyList<StageInfo> GetStages(int chapter)
        {
            if (_infosByChapter != null && _infosByChapter.TryGetValue(chapter, out List<StageInfo> infos))
                return infos;

            return null;
        }
    }

}
