using System;
using System.Collections.Generic;
using GameBerry.Chart;
using GameBerry.Table;
using UnityEngine;

namespace GameBerry
{
    public enum StageBattleMode
    {
        Field = 0,
        Boss = 1,
    }

    public class StageManager : Singleton<StageManager>
    {
        public event Action<Enum_Dungeon> OnDungeonProgressChanged;

        private StageChart _stageChart;
        private DungeonProgressTable _dungeonProgressTable;
        private readonly Dictionary<int, Sprite> _chapterIcons = new Dictionary<int, Sprite>();
        private StageBattleMode _stageBattleMode = StageBattleMode.Field;
        private float _bossBattleRemainingTime = 0f;
        private bool _isBossBattleTimerRunning = false;

        private const string _chapterIconPath = "Icon/chapter/{0}";

        public StageBattleMode CurrentStageBattleMode => _stageBattleMode;
        public bool IsStageBossBattle => _stageBattleMode == StageBattleMode.Boss;
        public float BossBattleRemainingTime => _bossBattleRemainingTime;
        public bool IsBossBattleTimerRunning => _isBossBattleTimerRunning;

        protected override void Init()
        {
            _stageChart = GameChart.Get<StageChart>();
            _dungeonProgressTable = UserTable.Get<DungeonProgressTable>();
        }

        public bool TryGetStageInfo(int chapter, int stage, out StageInfo info)
        {
            info = null;
            return _stageChart != null && _stageChart.TryGetInfo(chapter, stage, out info);
        }

        public IReadOnlyList<int> GetChapters()
        {
            return _stageChart?.GetChapters();
        }

        public IReadOnlyList<StageInfo> GetStages(int chapter)
        {
            return _stageChart?.GetStages(chapter);
        }

        public Sprite GetChapterIcon(int chapter)
        {
            Sprite icon = null;

            if (_chapterIcons.ContainsKey(chapter) == false)
            {
                ResourceLoader.Instance.Load<Sprite>(string.Format(_chapterIconPath, chapter), o =>
                {
                    icon = o as Sprite;
                    _chapterIcons[chapter] = icon;
                });
            }
            else
            {
                icon = _chapterIcons[chapter];
            }

            return icon;
        }

        public DungeonProgressData GetProgress(Enum_Dungeon dungeonType)
        {
            if (_dungeonProgressTable == null)
                return new DungeonProgressData { dungeonType = dungeonType };

            return _dungeonProgressTable.GetOrCreate(dungeonType);
        }

        public DungeonProgressData GetStageProgress()
        {
            return GetProgress(Enum_Dungeon.StageScene);
        }

        public void GetCurrentStage(out int chapter, out int stage)
        {
            DungeonProgressData data = GetStageProgress();
            chapter = data.currentChapter;
            stage = data.currentStage;
        }

        public void GetMaxStage(out int chapter, out int stage)
        {
            DungeonProgressData data = GetStageProgress();
            chapter = data.maxChapter;
            stage = data.maxStage;
        }

        public bool TryGetCurrentStageInfo(out StageInfo info)
        {
            GetCurrentStage(out int chapter, out int stage);
            return TryGetStageInfo(chapter, stage, out info);
        }

        public bool CanEnterStage(int chapter, int stage)
        {
            if (IsValidStage(chapter, stage) == false)
                return false;

            GetMaxStage(out int maxChapter, out int maxStage);
            return CompareStage(chapter, stage, maxChapter, maxStage) <= 0;
        }

        public bool IsCurrentStage(int chapter, int stage)
        {
            GetCurrentStage(out int currentChapter, out int currentStage);
            return currentChapter == chapter && currentStage == stage;
        }

        public bool IsHighestStage(int chapter, int stage)
        {
            GetMaxStage(out int maxChapter, out int maxStage);
            return maxChapter == chapter && maxStage == stage;
        }

        public bool SetCurrentStage(int chapter, int stage, bool immediate = true)
        {
            return SetCurrentProgress(Enum_Dungeon.StageScene, chapter, stage, immediate);
        }

        public void SetStageBattleMode(StageBattleMode battleMode)
        {
            _stageBattleMode = battleMode;
        }

        public void StartBossBattleTimer(float durationSeconds)
        {
            _bossBattleRemainingTime = Mathf.Max(0f, durationSeconds);
            _isBossBattleTimerRunning = true;
        }

        public void UpdateBossBattleTimer(float remainingTimeSeconds)
        {
            _bossBattleRemainingTime = Mathf.Max(0f, remainingTimeSeconds);
        }

        public void StopBossBattleTimer()
        {
            _bossBattleRemainingTime = 0f;
            _isBossBattleTimerRunning = false;
        }

        public bool PrepareFieldBattle(int chapter, int stage, bool immediate = true)
        {
            if (SetCurrentStage(chapter, stage, immediate) == false)
                return false;

            SetStageBattleMode(StageBattleMode.Field);
            return true;
        }

        public bool PrepareBossBattle(int chapter, int stage, bool immediate = true)
        {
            if (SetCurrentStage(chapter, stage, immediate) == false)
                return false;

            SetStageBattleMode(StageBattleMode.Boss);
            return true;
        }

        public bool SetMaxStage(int chapter, int stage, bool immediate = true)
        {
            return SetMaxProgress(Enum_Dungeon.StageScene, chapter, stage, immediate);
        }

        public bool SetCurrentProgress(Enum_Dungeon dungeonType, int chapter, int stage, bool immediate = true)
        {
            if (_dungeonProgressTable == null || IsValidStage(chapter, stage) == false)
                return false;

            _dungeonProgressTable.SetCurrent(dungeonType, chapter, stage);
            _dungeonProgressTable.UpdateTable(immediate);
            OnDungeonProgressChanged?.Invoke(dungeonType);
            return true;
        }

        public bool SetMaxProgress(Enum_Dungeon dungeonType, int chapter, int stage, bool immediate = true)
        {
            if (_dungeonProgressTable == null || IsValidStage(chapter, stage) == false)
                return false;

            DungeonProgressData data = GetProgress(dungeonType);
            if (CompareStage(chapter, stage, data.maxChapter, data.maxStage) < 0)
                return false;

            _dungeonProgressTable.SetMax(dungeonType, chapter, stage);
            _dungeonProgressTable.UpdateTable(immediate);
            OnDungeonProgressChanged?.Invoke(dungeonType);
            return true;
        }

        public bool TryAdvanceToNextStage(bool immediate = true)
        {
            if (_dungeonProgressTable == null)
                return false;

            DungeonProgressData data = GetStageProgress();
            if (_stageChart == null || _stageChart.TryGetNext(data.currentChapter, data.currentStage, out StageInfo nextInfo) == false)
                return false;

            _dungeonProgressTable.SetCurrent(Enum_Dungeon.StageScene, nextInfo.Chapter, nextInfo.Stage);

            if (CompareStage(nextInfo.Chapter, nextInfo.Stage, data.maxChapter, data.maxStage) > 0)
                _dungeonProgressTable.SetMax(Enum_Dungeon.StageScene, nextInfo.Chapter, nextInfo.Stage);

            _dungeonProgressTable.UpdateTable(immediate);
            OnDungeonProgressChanged?.Invoke(Enum_Dungeon.StageScene);
            return true;
        }

        private bool IsValidStage(int chapter, int stage)
        {
            return _stageChart != null && _stageChart.TryGetInfo(chapter, stage, out _);
        }

        private int CompareStage(int lhsChapter, int lhsStage, int rhsChapter, int rhsStage)
        {
            if (lhsChapter != rhsChapter)
                return lhsChapter.CompareTo(rhsChapter);

            return lhsStage.CompareTo(rhsStage);
        }
    }
}
