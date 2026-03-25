using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameBerry.Chart;
using GameBerry.Common;
using GameBerry.Managers;

namespace GameBerry.UI
{
    public class DungeonSelectDialog : IDialog
    {
        [Serializable]
        private class DungeonButtonBinding
        {
            public Enum_Dungeon DungeonType;
            public Button Button;
            public Image Background;
            public TMP_Text TitleText;
            public TMP_Text TicketText;
        }

        [Header("Dungeon Buttons")]
        [SerializeField] private List<DungeonButtonBinding> _dungeonButtonBindings = new List<DungeonButtonBinding>();

        [Header("Detail")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private Button _prevStageButton;
        [SerializeField] private TMP_Text _stageText;
        [SerializeField] private Button _nextStageButton;
        [SerializeField] private TMP_Text _unlockText;
        [SerializeField] private TMP_Text _ticketText;
        [SerializeField] private TMP_Text _rewardTitleText;
        [SerializeField] private UIItemElement _rewardItemPrefab;
        [SerializeField] private Transform _rewardItemRoot;
        [SerializeField] private TMP_Text _statusText;

        [Header("Enter")]
        [SerializeField] private Button _enterButton;
        [SerializeField] private TMP_Text _enterButtonText;

        private Enum_Dungeon _selectedDungeon = Enum_Dungeon.GrowthWeapon;
        private int _selectedStage = 1;
        private readonly List<UIItemElement> _rewardItems = new List<UIItemElement>();
        private readonly ObjectPool<UIItemElement> _rewardItemPool = new ObjectPool<UIItemElement>();

        protected override void OnLoad()
        {
            if (_prevStageButton != null)
                _prevStageButton.onClick.AddListener(OnClickPrevStage);
            if (_nextStageButton != null)
                _nextStageButton.onClick.AddListener(OnClickNextStage);
            if (_enterButton != null)
                _enterButton.onClick.AddListener(OnClickEnter);

            for (int i = 0; i < _dungeonButtonBindings.Count; ++i)
            {
                DungeonButtonBinding binding = _dungeonButtonBindings[i];
                if (binding?.Button == null)
                    continue;

                Enum_Dungeon dungeonType = binding.DungeonType;
                binding.Button.onClick.RemoveAllListeners();
                binding.Button.onClick.AddListener(() => OnClickDungeon(dungeonType));
            }
        }

        protected override void OnEnter()
        {
            GrowthDungeonManager.Instance.OnGrowthDungeonProgressChanged += OnGrowthDungeonProgressChanged;
            ItemManager.Instance.OnPointChanged += OnPointChanged;

            IReadOnlyList<Enum_Dungeon> dungeonTypes = GrowthDungeonManager.Instance.GetDungeonTypes();
            if (dungeonTypes != null && dungeonTypes.Count > 0 && GrowthDungeonManager.IsGrowthDungeon(_selectedDungeon) == false)
                _selectedDungeon = dungeonTypes[0];

            SyncSelectedStageToProgress();
            RefreshAll();
        }

        protected override void OnExit()
        {
            if (GrowthDungeonManager.isAlive)
                GrowthDungeonManager.Instance.OnGrowthDungeonProgressChanged -= OnGrowthDungeonProgressChanged;
            if (ItemManager.isAlive)
                ItemManager.Instance.OnPointChanged -= OnPointChanged;

            if (_statusText != null)
                _statusText.SetText(string.Empty);
        }

        protected override void OnUnload()
        {
            ReleaseRewards();
            _rewardItemPool.ClearAll();
        }

        private void OnGrowthDungeonProgressChanged(Enum_Dungeon dungeonType)
        {
            if (dungeonType != _selectedDungeon)
                return;

            ClampSelectedStage();
            RefreshAll();
        }

        private void OnPointChanged()
        {
            RefreshAll();
        }

        private void OnClickDungeon(Enum_Dungeon dungeonType)
        {
            _selectedDungeon = dungeonType;
            SyncSelectedStageToProgress();

            if (_statusText != null)
                _statusText.SetText(string.Empty);

            RefreshAll();
        }

        private void OnClickPrevStage()
        {
            int maxUnlocked = GrowthDungeonManager.Instance.GetMaxUnlockedStage(_selectedDungeon);
            if (maxUnlocked <= 1)
                return;

            _selectedStage = Mathf.Clamp(_selectedStage - 1, 1, maxUnlocked);
            RefreshAll();
        }

        private void OnClickNextStage()
        {
            int maxUnlocked = GrowthDungeonManager.Instance.GetMaxUnlockedStage(_selectedDungeon);
            if (maxUnlocked <= 1)
                return;

            _selectedStage = Mathf.Clamp(_selectedStage + 1, 1, maxUnlocked);
            RefreshAll();
        }

        private void OnClickEnter()
        {
            if (GrowthDungeonManager.Instance.TryEnterDungeon(_selectedDungeon, _selectedStage, true) == false)
            {
                if (_statusText != null)
                    _statusText.SetText(BuildFailReason());

                RefreshAll();
                return;
            }

            if (BattleSceneManager.isAlive)
            {
                if (BattleSceneManager.Instance.BattleType == _selectedDungeon)
                    BattleSceneManager.Instance.ReloadCurrentBattleScene();
                else
                    BattleSceneManager.Instance.ChangeBattleScene(_selectedDungeon);
            }

            Exit();
        }

        private string BuildFailReason()
        {
            if (GrowthDungeonManager.Instance.CanEnter(_selectedDungeon, _selectedStage) == false)
                return "This stage is locked.";

            if (GrowthDungeonManager.Instance.GetEntryTicketItemId(_selectedDungeon) <= 0)
                return "Ticket point is not configured.";

            return "Not enough tickets.";
        }

        private void SyncSelectedStageToProgress()
        {
            _selectedStage = GrowthDungeonManager.Instance.GetCurrentStage(_selectedDungeon);
            ClampSelectedStage();
        }

        private void ClampSelectedStage()
        {
            int maxUnlocked = Mathf.Max(1, GrowthDungeonManager.Instance.GetMaxUnlockedStage(_selectedDungeon));
            _selectedStage = Mathf.Clamp(_selectedStage, 1, maxUnlocked);
        }

        private void RefreshAll()
        {
            if (_rewardTitleText != null)
                _rewardTitleText.SetText("Clear Reward");

            RefreshDungeonButtons();
            RefreshDetail();
        }

        private void RefreshDungeonButtons()
        {
            for (int i = 0; i < _dungeonButtonBindings.Count; ++i)
            {
                DungeonButtonBinding binding = _dungeonButtonBindings[i];
                if (binding == null)
                    continue;

                bool selected = binding.DungeonType == _selectedDungeon;
                long ticketCount = GrowthDungeonManager.Instance.GetEntryTicketCount(binding.DungeonType);

                if (binding.Background != null)
                    binding.Background.color = selected
                        ? new Color(0.22f, 0.54f, 0.86f, 1f)
                        : new Color(0.16f, 0.18f, 0.22f, 1f);

                if (binding.TitleText != null)
                    binding.TitleText.SetText(GrowthDungeonManager.Instance.GetDungeonDisplayName(binding.DungeonType));

                if (binding.TicketText != null)
                {
                    Enum_PointType ticketPointType = GrowthDungeonManager.Instance.GetEntryTicketPointType(binding.DungeonType);
                    string ticketName = GrowthDungeonManager.Instance.GetPointDisplayName(ticketPointType);
                    binding.TicketText.SetText($"{ticketName}  {ticketCount:N0}");
                }
            }
        }

        private void RefreshDetail()
        {
            ClampSelectedStage();
            ReleaseRewards();

            if (_titleText != null)
                _titleText.SetText(GrowthDungeonManager.Instance.GetDungeonDisplayName(_selectedDungeon));

            if (_descriptionText != null)
                _descriptionText.SetText(GrowthDungeonManager.Instance.GetDungeonShortDescription(_selectedDungeon));

            int maxUnlocked = GrowthDungeonManager.Instance.GetMaxUnlockedStage(_selectedDungeon);
            int maxConfigured = GrowthDungeonManager.Instance.GetMaxConfiguredStage(_selectedDungeon);
            int ticketCost = GrowthDungeonManager.Instance.GetEntryTicketCost(_selectedDungeon, _selectedStage);
            long ticketCount = GrowthDungeonManager.Instance.GetEntryTicketCount(_selectedDungeon);

            if (_stageText != null)
                _stageText.SetText($"Stage {_selectedStage}");

            if (_unlockText != null)
                _unlockText.SetText($"Unlocked {maxUnlocked} / {maxConfigured}");

            if (_ticketText != null)
            {
                Enum_PointType ticketPointType = GrowthDungeonManager.Instance.GetEntryTicketPointType(_selectedDungeon);
                string ticketName = GrowthDungeonManager.Instance.GetPointDisplayName(ticketPointType);
                _ticketText.SetText($"Ticket {ticketName}  {ticketCount:N0} / Cost {ticketCost}");
            }

            if (_prevStageButton != null)
                _prevStageButton.interactable = _selectedStage > 1;
            if (_nextStageButton != null)
                _nextStageButton.interactable = _selectedStage < maxUnlocked;

            RefreshRewards();

            bool canEnter = GrowthDungeonManager.Instance.CanEnter(_selectedDungeon, _selectedStage) &&
                            GrowthDungeonManager.Instance.CanAffordEntryTicket(_selectedDungeon, _selectedStage);

            if (_enterButton != null)
                _enterButton.interactable = canEnter;
            if (_enterButtonText != null)
                _enterButtonText.SetText(canEnter ? "Enter" : "Locked");
        }

        private void RefreshRewards()
        {
            if (GrowthDungeonManager.Instance.TryGetInfo(_selectedDungeon, _selectedStage, out DungeonRuntimeInfo info) == false || info == null)
            {
                return;
            }

            if (_rewardItemPrefab == null || _rewardItemRoot == null)
                return;

            IReadOnlyList<DungeonRewardPointInfo> rewardPoints = info.GetRewardPoints();
            if (rewardPoints == null)
                return;

            for (int i = 0; i < rewardPoints.Count; ++i)
            {
                DungeonRewardPointInfo rewardInfo = rewardPoints[i];
                if (rewardInfo == null || rewardInfo.PointType == Enum_PointType.Max || rewardInfo.Amount <= 0)
                    continue;

                int rewardItemId = GameChart.Get<PointChart>()?.GetByType(rewardInfo.PointType)?.ItemId ?? 0;
                if (rewardItemId <= 0)
                    continue;

                UIItemElement rewardItem = GetOrCreateRewardItem();
                if (rewardItem == null)
                    continue;

                rewardItem.transform.SetParent(_rewardItemRoot, false);
                rewardItem.gameObject.SetActive(true);
                rewardItem.Bind(ItemHandle.ForMeta(rewardItemId, rewardInfo.Amount));
                _rewardItems.Add(rewardItem);
            }
        }

        private UIItemElement GetOrCreateRewardItem()
        {
            UIItemElement item = _rewardItemPool.GetObject();
            if (item != null)
                return item;

            return _rewardItemPrefab != null && _rewardItemRoot != null
                ? Instantiate(_rewardItemPrefab, _rewardItemRoot)
                : null;
        }

        private void ReleaseRewards()
        {
            for (int i = 0; i < _rewardItems.Count; ++i)
            {
                UIItemElement item = _rewardItems[i];
                if (item == null)
                    continue;

                item.gameObject.SetActive(false);
                _rewardItemPool.PoolObject(item);
            }

            _rewardItems.Clear();
        }
    }
}
