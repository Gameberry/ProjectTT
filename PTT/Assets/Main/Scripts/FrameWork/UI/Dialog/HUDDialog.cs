using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

            Message.AddListener<Event.RefreshComboUIMsg>(ShowComboUI);
            Message.AddListener<Event.RefreshPlayerHpMsg>(OnRefreshPlayerHp);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.OnLevelChanged -= RefreshLevel;
                PlayerManager.Instance.OnExpChanged -= RefreshExp;
            }

            Message.RemoveListener<Event.RefreshComboUIMsg>(ShowComboUI);
            Message.RemoveListener<Event.RefreshPlayerHpMsg>(OnRefreshPlayerHp);
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
    }
}