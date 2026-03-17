using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameBerry.Chart;
using GameBerry.Common;
using GameBerry.Managers;

namespace GameBerry.UI
{
    public class StageSelectDialog : IDialog
    {
        [Header("Chapter Tabs")]
        [SerializeField] private UIStageChapterTabElement _chapterTabPrefab;
        [SerializeField] private Transform _chapterTabRoot;

        [Header("Stage List")]
        [SerializeField] private UIStageEntryElement _stageEntryPrefab;
        [SerializeField] private Transform _stageEntryRoot;

        [Header("Detail")]
        [SerializeField] private Image _chapterImage;
        [SerializeField] private TMP_Text _stageTitleText;
        [SerializeField] private UIItemElement _rewardGold;
        [SerializeField] private TMP_Text _rewardExpText;
        [SerializeField] private TMP_Text _rewardEquipText;
        [SerializeField] private TMP_Text _rewardEquipDropRateText;
        [SerializeField] private UIItemElement _rewardItemPrefab;
        [SerializeField] private Transform _rewardItemRoot;

        [Header("Buttons")]
        [SerializeField] private Button _enterButton;
        [SerializeField] private TMP_Text _enterButtonText;

        private readonly List<UIStageChapterTabElement> _chapterTabs = new List<UIStageChapterTabElement>();
        private readonly List<UIStageEntryElement> _stageEntries = new List<UIStageEntryElement>();
        private readonly List<UIItemElement> _rewardItems = new List<UIItemElement>();
        private readonly ObjectPool<UIStageChapterTabElement> _chapterTabPool = new ObjectPool<UIStageChapterTabElement>();
        private readonly ObjectPool<UIStageEntryElement> _stageEntryPool = new ObjectPool<UIStageEntryElement>();
        private readonly ObjectPool<UIItemElement> _rewardItemPool = new ObjectPool<UIItemElement>();

        private int _selectedChapter;
        private int _selectedStage;

        protected override void OnLoad()
        {
            if (_enterButton != null)
                _enterButton.onClick.AddListener(OnClickEnter);
        }

        protected override void OnEnter()
        {
            StageManager.Instance.OnDungeonProgressChanged += OnDungeonProgressChanged;
            SyncSelectionToCurrentStage();

            RefreshAll();
        }

        protected override void OnExit()
        {
            if (StageManager.isAlive)
                StageManager.Instance.OnDungeonProgressChanged -= OnDungeonProgressChanged;
        }

        protected override void OnUnload()
        {
            ReleaseRewards();
            ReleaseStages();
            ReleaseChapters();
            _chapterTabPool.ClearAll();
            _stageEntryPool.ClearAll();
            _rewardItemPool.ClearAll();
        }

        private void OnDungeonProgressChanged(Enum_Dungeon dungeonType)
        {
            if (dungeonType != Enum_Dungeon.StageScene)
                return;

            RefreshAll();
        }

        private void RefreshAll()
        {
            EnsureSelection();
            RebuildChapters();
            RebuildStages();
            RefreshDetail();
        }

        private void SyncSelectionToCurrentStage()
        {
            StageManager.Instance.GetCurrentStage(out _selectedChapter, out _selectedStage);
        }

        private void EnsureSelection()
        {
            IReadOnlyList<int> chapters = StageManager.Instance.GetChapters();
            if (chapters == null || chapters.Count <= 0)
            {
                _selectedChapter = 0;
                _selectedStage = 0;
                return;
            }

            if (_selectedChapter > 0 &&
                _selectedStage > 0 &&
                StageManager.Instance.TryGetStageInfo(_selectedChapter, _selectedStage, out _))
                return;

            StageManager.Instance.GetCurrentStage(out _selectedChapter, out _selectedStage);
            if (StageManager.Instance.CanEnterStage(_selectedChapter, _selectedStage))
                return;

            _selectedChapter = chapters[0];
            IReadOnlyList<StageInfo> stages = StageManager.Instance.GetStages(_selectedChapter);
            _selectedStage = stages != null && stages.Count > 0 ? stages[0].Stage : 1;
        }

        private void RebuildChapters()
        {
            ReleaseChapters();

            if (_chapterTabPrefab == null || _chapterTabRoot == null)
                return;

            IReadOnlyList<int> chapters = StageManager.Instance.GetChapters();
            if (chapters == null)
                return;

            for (int i = 0; i < chapters.Count; ++i)
            {
                UIStageChapterTabElement element = GetOrCreateChapterTab();
                if (element == null)
                    continue;

                int chapter = chapters[i];
                bool unlocked = false;
                IReadOnlyList<StageInfo> chapterStages = StageManager.Instance.GetStages(chapter);
                if (chapterStages != null)
                {
                    for (int stageIndex = 0; stageIndex < chapterStages.Count; ++stageIndex)
                    {
                        StageInfo stageInfo = chapterStages[stageIndex];
                        if (stageInfo == null)
                            continue;

                        if (StageManager.Instance.CanEnterStage(stageInfo.Chapter, stageInfo.Stage))
                        {
                            unlocked = true;
                            break;
                        }
                    }
                }

                element.transform.SetParent(_chapterTabRoot, false);
                element.gameObject.SetActive(true);
                element.Bind(chapter, unlocked, chapter == _selectedChapter, OnClickChapter);
                _chapterTabs.Add(element);
            }
        }

        private void RebuildStages()
        {
            ReleaseStages();

            if (_stageEntryPrefab == null || _stageEntryRoot == null || _selectedChapter <= 0)
                return;

            IReadOnlyList<StageInfo> stages = StageManager.Instance.GetStages(_selectedChapter);
            if (stages == null)
                return;

            for (int i = 0; i < stages.Count; ++i)
            {
                StageInfo info = stages[i];
                if (info == null)
                    continue;

                UIStageEntryElement element = GetOrCreateStageEntry();
                if (element == null)
                    continue;

                bool canEnter = StageManager.Instance.CanEnterStage(info.Chapter, info.Stage);
                bool selected = info.Chapter == _selectedChapter && info.Stage == _selectedStage;
                bool isCurrent = StageManager.Instance.IsCurrentStage(info.Chapter, info.Stage);

                element.transform.SetParent(_stageEntryRoot, false);
                element.gameObject.SetActive(true);
                element.Bind(info.Chapter, info.Stage, canEnter, selected, isCurrent, OnClickStage);
                _stageEntries.Add(element);
            }
        }

        private void RefreshDetail()
        {
            ReleaseRewards();

            if (_chapterImage != null)
                _chapterImage.sprite = StageManager.Instance.GetChapterIcon(_selectedChapter);

            if (StageManager.Instance.TryGetStageInfo(_selectedChapter, _selectedStage, out StageInfo info) == false)
            {
                if (_enterButton != null)
                    _enterButton.interactable = false;
                return;
            }

            if (_stageTitleText != null)
                _stageTitleText.SetText($"Stage {_selectedChapter}-{_selectedStage}");

            if (_rewardGold != null)
            {
                int goldItemId = GameChart.Get<PointChart>()?.GetByType(Enum_PointType.Gold).ItemId ?? 0;
                _rewardGold.Bind(ItemHandle.ForMeta(goldItemId, info.Gold));
            }

            if (_rewardExpText != null)
                _rewardExpText.SetText($"{info.Exp:N0}");

            if (_rewardEquipText != null)
                _rewardEquipText.SetText($"Lv.{info.EquipLevelMin}-{info.EquipLevelMax}");

            if(_rewardEquipDropRateText != null)
                _rewardEquipDropRateText.SetText($"Drop {(info.EquipDropRate * 100.0):0.##}%");

            if (_rewardItemPrefab != null && _rewardItemRoot != null && info.EquipList != null)
            {
                for (int i = 0; i < info.EquipList.Length; ++i)
                {
                    if (info.EquipList[i] <= 0)
                        continue;

                    UIItemElement reward = GetOrCreateRewardItem();
                    if (reward == null)
                        continue;

                    reward.transform.SetParent(_rewardItemRoot, false);
                    reward.gameObject.SetActive(true);
                    reward.Bind(ItemHandle.ForMeta(info.EquipList[i], 1));
                    _rewardItems.Add(reward);
                }
            }

            bool canEnter = StageManager.Instance.CanEnterStage(_selectedChapter, _selectedStage);
            if (_enterButton != null)
                _enterButton.interactable = canEnter;
            if (_enterButtonText != null)
                _enterButtonText.SetText(canEnter ? "Enter" : "Locked");
        }

        private void OnClickChapter(int chapter)
        {
            _selectedChapter = chapter;
            IReadOnlyList<StageInfo> stages = StageManager.Instance.GetStages(chapter);
            if (stages != null)
            {
                for (int i = 0; i < stages.Count; ++i)
                {
                    if (StageManager.Instance.CanEnterStage(stages[i].Chapter, stages[i].Stage) == false)
                        continue;

                    _selectedStage = stages[i].Stage;
                    RefreshAll();
                    return;
                }

                if (stages.Count > 0)
                    _selectedStage = stages[0].Stage;
            }

            RefreshAll();
        }

        private void OnClickStage(int chapter, int stage)
        {
            _selectedChapter = chapter;
            _selectedStage = stage;
            RefreshAll();
        }

        private void OnClickEnter()
        {
            if (StageManager.Instance.CanEnterStage(_selectedChapter, _selectedStage) == false)
                return;

            if (StageManager.Instance.PrepareFieldBattle(_selectedChapter, _selectedStage, true) == false)
                return;

            if (BattleSceneManager.isAlive)
            {
                if (BattleSceneManager.Instance.BattleType == Enum_Dungeon.StageScene)
                    BattleSceneManager.Instance.ReloadCurrentBattleScene();
                else
                    BattleSceneManager.Instance.ChangeBattleScene(Enum_Dungeon.StageScene);
            }

            Exit();
        }

        private UIStageChapterTabElement GetOrCreateChapterTab()
        {
            UIStageChapterTabElement element = _chapterTabPool.GetObject();
            if (element != null)
                return element;

            return _chapterTabPrefab != null && _chapterTabRoot != null
                ? Instantiate(_chapterTabPrefab, _chapterTabRoot)
                : null;
        }

        private UIStageEntryElement GetOrCreateStageEntry()
        {
            UIStageEntryElement element = _stageEntryPool.GetObject();
            if (element != null)
                return element;

            return _stageEntryPrefab != null && _stageEntryRoot != null
                ? Instantiate(_stageEntryPrefab, _stageEntryRoot)
                : null;
        }

        private UIItemElement GetOrCreateRewardItem()
        {
            UIItemElement element = _rewardItemPool.GetObject();
            if (element != null)
                return element;

            return _rewardItemPrefab != null && _rewardItemRoot != null
                ? Instantiate(_rewardItemPrefab, _rewardItemRoot)
                : null;
        }

        private void ReleaseChapters()
        {
            for (int i = 0; i < _chapterTabs.Count; ++i)
            {
                UIStageChapterTabElement element = _chapterTabs[i];
                if (element == null)
                    continue;

                element.gameObject.SetActive(false);
                _chapterTabPool.PoolObject(element);
            }

            _chapterTabs.Clear();
        }

        private void ReleaseStages()
        {
            for (int i = 0; i < _stageEntries.Count; ++i)
            {
                UIStageEntryElement element = _stageEntries[i];
                if (element == null)
                    continue;

                element.gameObject.SetActive(false);
                _stageEntryPool.PoolObject(element);
            }

            _stageEntries.Clear();
        }

        private void ReleaseRewards()
        {
            for (int i = 0; i < _rewardItems.Count; ++i)
            {
                UIItemElement element = _rewardItems[i];
                if (element == null)
                    continue;

                element.gameObject.SetActive(false);
                _rewardItemPool.PoolObject(element);
            }

            _rewardItems.Clear();
        }
    }
}
