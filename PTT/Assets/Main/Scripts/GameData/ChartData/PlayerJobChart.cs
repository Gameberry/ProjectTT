using System.Collections.Generic;

namespace GameBerry.Chart
{
    public struct PlayerJobInfo
    {
        public int JobId;                 // 전직 ID (1차=1, 2차=2, ...)
        public int RequiredLevel;         // 전직 가능 레벨
        public string Name;               // 전직 명칭 (예: "초보자" → "전사")
        public string BonusStats;         // 전직 보너스 스탯 (예: "Attack=50|HP=200")
        public int UnlockSkillGroupId;    // 해금되는 스킬 그룹 (확장용)
    }

    public class PlayerJobChart : ChartBase
    {
        public PlayerJobInfo this[int index] => rows[index];
        public PlayerJobInfo[] rows;

        private Dictionary<int, PlayerJobInfo> _jobDict;
        private Dictionary<int, PlayerJobInfo> _levelToJobDict;

        public override bool IsLoaded() => rows != null;

        public override void LoadComplete()
        {
            _jobDict = new Dictionary<int, PlayerJobInfo>(rows.Length);
            _levelToJobDict = new Dictionary<int, PlayerJobInfo>(rows.Length);

            foreach (var r in rows)
            {
                _jobDict[r.JobId] = r;
                _levelToJobDict[r.RequiredLevel] = r;
            }
        }

        public bool TryGetJobInfo(int jobId, out PlayerJobInfo info)
        {
            if (_jobDict != null && _jobDict.TryGetValue(jobId, out info))
                return true;

            info = default;
            return false;
        }

        public bool TryGetJobByLevel(int level, out PlayerJobInfo info)
        {
            if (_levelToJobDict != null && _levelToJobDict.TryGetValue(level, out info))
                return true;

            info = default;
            return false;
        }

        /// <summary>
        /// 현재 레벨에서 달성 가능한 최고 전직 단계
        /// </summary>
        public int GetCurrentJobId(int currentLevel)
        {
            if (rows == null || rows.Length == 0)
                return 0;

            int resultId = 0;
            for (int i = 0; i < rows.Length; i++)
            {
                if (currentLevel >= rows[i].RequiredLevel)
                    resultId = rows[i].JobId;
                else
                    break;
            }

            return resultId;
        }

        /// <summary>
        /// 다음 전직 정보 (없으면 false)
        /// </summary>
        public bool TryGetNextJob(int currentJobId, out PlayerJobInfo info)
        {
            int nextId = currentJobId + 1;
            return TryGetJobInfo(nextId, out info);
        }

        public int GetMaxJobId()
        {
            if (rows == null || rows.Length == 0)
                return 0;

            return rows[rows.Length - 1].JobId;
        }
    }
}
