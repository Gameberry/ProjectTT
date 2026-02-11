using System.Collections.Generic;

namespace GameBerry.Chart
{
    public struct PlayerLevelInfo
    {
        public int Level;
        public double RequiredExp;      // 해당 레벨 달성에 필요한 누적 경험치
        public double LevelUpExp;       // 다음 레벨까지 필요한 경험치 (RequiredExp[n+1] - RequiredExp[n])
        public string BaseStats;      // 레벨업 시 기본 스탯 증가 (예: "Attack=5|HP=20")
    }

    public class PlayerLevelChart : ChartBase
    {
        public PlayerLevelInfo this[int index] => rows[index];
        public PlayerLevelInfo[] rows;

        private Dictionary<int, PlayerLevelInfo> _levelDict;

        public override bool IsLoaded() => rows != null;

        public override void LoadComplete()
        {
            _levelDict = new Dictionary<int, PlayerLevelInfo>(rows.Length);
            foreach (var r in rows)
            {
                _levelDict[r.Level] = r;
            }
        }

        public bool TryGetLevelInfo(int level, out PlayerLevelInfo info)
        {
            if (_levelDict != null && _levelDict.TryGetValue(level, out info))
                return true;

            info = default;
            return false;
        }

        public int GetMaxLevel()
        {
            if (rows == null || rows.Length == 0)
                return 1;

            return rows[rows.Length - 1].Level;
        }

        /// <summary>
        /// 누적 경험치로 현재 레벨 계산
        /// </summary>
        public int CalculateLevelFromExp(double totalExp)
        {
            if (rows == null || rows.Length == 0)
                return 1;

            int resultLevel = 1;
            for (int i = 0; i < rows.Length; i++)
            {
                if (totalExp >= rows[i].RequiredExp)
                    resultLevel = rows[i].Level;
                else
                    break;
            }

            return resultLevel;
        }
    }
}
