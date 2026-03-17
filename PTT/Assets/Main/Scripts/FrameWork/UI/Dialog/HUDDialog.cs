using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Chart;
using GameBerry.Managers;

namespace GameBerry.UI
{
    public class HUDDialog : IDialog
    {
        [Header("Combo")]
        [SerializeField]
        private Transform _comboGroup;

        [SerializeField]
        private TMP_Text _comboText;

        [SerializeField]
        private TMP_Text _playerCurrentHp;

        [SerializeField]
        private TMP_Text _playerMaxHp;

        [SerializeField]
        private Image _playerHpbar;

        [SerializeField]
        private TMP_Text _playerLevel;

        [SerializeField]
        private TMP_Text _playerExp;

        [SerializeField]
        private Image _playerExpBar;

        [Header("Stage")]
        [SerializeField]
        private List<Button> _stageSelectButtons = new List<Button>();

        [SerializeField]
        private Button _challengeButton;

        [SerializeField]
        private TMP_Text _stageNameText;

        [SerializeField]
        private TMP_Text _challengeButtonText;

        [SerializeField]
        private GameObject _bossTimerRoot;

        [SerializeField]
        private TMP_Text _bossTimerText;

        [Header("Hell")]
        [SerializeField]
        private Button _hellInfoButton;

        [SerializeField]
        private TMP_Text _hellLevelText;

        private long _cachedCurrentHp = -1;
        private long _cachedMaxHp = -1;
        private float _cachedHpRatio = -1f;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            _comboGroup?.gameObject.SetActive(false);

            PlayerManager.Instance.OnLevelChanged += RefreshLevel;
            RefreshLevel();

            PlayerManager.Instance.OnExpChanged += RefreshExp;
            RefreshExp(PlayerManager.Instance.GetExp());

            RegisterStageButtons();
            RegisterHellButton();

            Message.AddListener<Event.RefreshComboUIMsg>(ShowComboUI);
            Message.AddListener<Event.RefreshPlayerHpMsg>(OnRefreshPlayerHp);
            Message.AddListener<Event.RefreshBattleSceneUIMsg>(OnRefreshBattleSceneUI);

            StageManager.Instance.OnDungeonProgressChanged += OnDungeonProgressChanged;
            HellManager.Instance.OnHellStateChanged += RefreshHellLevel;

            RefreshStageInfo();
            RefreshHellLevel();
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.OnLevelChanged -= RefreshLevel;
                PlayerManager.Instance.OnExpChanged -= RefreshExp;
            }

            if (StageManager.isAlive)
                StageManager.Instance.OnDungeonProgressChanged -= OnDungeonProgressChanged;

            UnregisterStageButtons();
            UnregisterHellButton();

            if (HellManager.isAlive)
                HellManager.Instance.OnHellStateChanged -= RefreshHellLevel;

            Message.RemoveListener<Event.RefreshComboUIMsg>(ShowComboUI);
            Message.RemoveListener<Event.RefreshPlayerHpMsg>(OnRefreshPlayerHp);
            Message.RemoveListener<Event.RefreshBattleSceneUIMsg>(OnRefreshBattleSceneUI);
        }
        //------------------------------------------------------------------------------------
        private void Update()
        {
            RefreshBossTimer();
        }
        //------------------------------------------------------------------------------------
        private void ShowComboUI(Event.RefreshComboUIMsg msg)
        {
            if (msg == null)
                return;

            if (msg.Combo <= 1)
                _comboGroup?.gameObject.SetActive(false);
            else
            {
                _comboGroup?.gameObject.SetActive(true);
                _comboText?.SetText("{0:#,###}", msg.Combo);
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshLevel()
        {
            if (_playerLevel != null)
                _playerLevel.SetText("Lv. {0}", PlayerManager.Instance.GetLevel());
        }
        //------------------------------------------------------------------------------------
        private void RefreshExp(double totalExp)
        {
            float expProgress = PlayerManager.Instance.GetExpProgress();

            if (_playerExp != null)
                _playerExp.SetText(string.Format("{0:0.###}%", expProgress * 100f));

            if (_playerExpBar != null)
                _playerExpBar.fillAmount = expProgress;
        }
        //------------------------------------------------------------------------------------
        private void OnRefreshPlayerHp(Event.RefreshPlayerHpMsg msg)
        {
            if (msg == null)
                return;

            RefreshHp(msg.CurrentHp, msg.MaxHp);
        }
        //------------------------------------------------------------------------------------
        private void RefreshHp(double currentHp, double maxHp)
        {
            float hpRatio = 0f;

            if (maxHp > 0)
                hpRatio = Mathf.Clamp01((float)(currentHp / maxHp));

            long currentHpInt = (long)Math.Floor(Math.Max(0, currentHp));
            long maxHpInt = (long)Math.Floor(Math.Max(0, maxHp));

            if (_cachedCurrentHp == currentHpInt && _cachedMaxHp == maxHpInt && Mathf.Approximately(_cachedHpRatio, hpRatio))
                return;

            _cachedCurrentHp = currentHpInt;
            _cachedMaxHp = maxHpInt;
            _cachedHpRatio = hpRatio;

            if (_playerCurrentHp != null)
                _playerCurrentHp.SetText(string.Format("{0:#,###}", currentHpInt));

            if (_playerMaxHp != null)
                _playerMaxHp.SetText(string.Format("{0:#,###}", maxHpInt));

            if (_playerHpbar != null)
                _playerHpbar.fillAmount = hpRatio;
        }
        //------------------------------------------------------------------------------------
        private void RegisterStageButtons()
        {
            for (int i = 0; i < _stageSelectButtons.Count; ++i)
            {
                Button button = _stageSelectButtons[i];
                if (button != null)
                    button.onClick.AddListener(OnClickStageSelect);
            }

            if (_challengeButton != null)
                _challengeButton.onClick.AddListener(OnClickChallenge);
        }
        //------------------------------------------------------------------------------------
        private void RegisterHellButton()
        {
            if (_hellInfoButton != null)
                _hellInfoButton.onClick.AddListener(OnClickHellInfo);
        }
        //------------------------------------------------------------------------------------
        private void UnregisterStageButtons()
        {
            for (int i = 0; i < _stageSelectButtons.Count; ++i)
            {
                Button button = _stageSelectButtons[i];
                if (button != null)
                    button.onClick.RemoveListener(OnClickStageSelect);
            }

            if (_challengeButton != null)
                _challengeButton.onClick.RemoveListener(OnClickChallenge);
        }
        //------------------------------------------------------------------------------------
        private void UnregisterHellButton()
        {
            if (_hellInfoButton != null)
                _hellInfoButton.onClick.RemoveListener(OnClickHellInfo);
        }
        //------------------------------------------------------------------------------------
        private void OnClickStageSelect()
        {
            UIManager.Instance.DialogEnter<StageSelectDialog>();
        }
        //------------------------------------------------------------------------------------
        private void OnClickHellInfo()
        {
            UIManager.Instance.DialogEnter<HellInfoDialog>();
        }
        //------------------------------------------------------------------------------------
        private void OnClickChallenge()
        {
            StageManager.Instance.GetCurrentStage(out int chapter, out int stage);

            if (StageManager.Instance.IsStageBossBattle)
            {
                if (StageManager.Instance.PrepareFieldBattle(chapter, stage, true) == false)
                    return;

                if (BattleSceneManager.Instance.BattleType == Enum_Dungeon.StageScene)
                    BattleSceneManager.Instance.ReloadCurrentBattleScene();
                else
                    BattleSceneManager.Instance.ChangeBattleScene(Enum_Dungeon.StageScene);

                return;
            }

            if (StageManager.Instance.CanEnterStage(chapter, stage) == false)
                return;

            if (StageManager.Instance.TryGetCurrentStageInfo(out StageInfo info) == false)
                return;

            if (info.BossMonster <= 0)
                return;

            if (StageManager.Instance.PrepareBossBattle(chapter, stage, true) == false)
                return;

            if (BattleSceneManager.Instance.BattleType == Enum_Dungeon.StageScene)
                BattleSceneManager.Instance.ReloadCurrentBattleScene();
            else
                BattleSceneManager.Instance.ChangeBattleScene(Enum_Dungeon.StageScene);
        }
        //------------------------------------------------------------------------------------
        private void OnDungeonProgressChanged(Enum_Dungeon dungeonType)
        {
            if (dungeonType != Enum_Dungeon.StageScene)
                return;

            RefreshStageInfo();
        }
        //------------------------------------------------------------------------------------
        private void OnRefreshBattleSceneUI(Event.RefreshBattleSceneUIMsg msg)
        {
            RefreshStageInfo();
        }
        //------------------------------------------------------------------------------------
        private void RefreshStageInfo()
        {
            StageManager.Instance.GetCurrentStage(out int chapter, out int stage);
            bool isBossBattle = StageManager.Instance.IsStageBossBattle;
            bool canChallenge = false;

            if (StageManager.Instance.TryGetCurrentStageInfo(out StageInfo info) && info != null)
                canChallenge = info.BossMonster > 0;

            if (_stageNameText != null)
                _stageNameText.SetText($"Stage {chapter}-{stage}");

            for (int i = 0; i < _stageSelectButtons.Count; ++i)
            {
                if (_stageSelectButtons[i] != null)
                    _stageSelectButtons[i].gameObject.SetActive(isBossBattle == false);
            }

            if (_challengeButton != null)
                _challengeButton.gameObject.SetActive(isBossBattle || canChallenge);

            if (_challengeButtonText != null)
                _challengeButtonText.SetText(isBossBattle ? "나가기" : "도전");

            if (_bossTimerRoot != null)
                _bossTimerRoot.SetActive(isBossBattle);

            RefreshBossTimer();
        }
        //------------------------------------------------------------------------------------
        private void RefreshBossTimer()
        {
            bool isBossBattle = StageManager.isAlive && StageManager.Instance.IsStageBossBattle;

            if (_bossTimerRoot != null && _bossTimerRoot.activeSelf != isBossBattle)
                _bossTimerRoot.SetActive(isBossBattle);

            if (_bossTimerText == null || isBossBattle == false)
                return;

            float remainingTime = StageManager.Instance.IsBossBattleTimerRunning
                ? StageManager.Instance.BossBattleRemainingTime
                : 0f;

            int totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, remainingTime));
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            _bossTimerText.SetText("{0:00}:{1:00}", minutes, seconds);
        }
        //------------------------------------------------------------------------------------
        private void RefreshHellLevel()
        {
            if (_hellLevelText != null)
            {
                int hellLevel = HellManager.Instance.GetHellLevel();
                _hellLevelText.SetText($"Hell Lv.{hellLevel}");
            }
        }
        //------------------------------------------------------------------------------------
    }
}
