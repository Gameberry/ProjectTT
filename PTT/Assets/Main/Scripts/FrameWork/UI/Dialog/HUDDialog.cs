using System;
using System.Collections.Generic;
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
        private TMP_Text _playerLevel;

        [SerializeField]
        private TMP_Text _playerExp;

        [SerializeField]
        private Image _playerExpBar;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            _comboGroup?.gameObject.SetActive(false);

            PlayerManager.Instance.OnLevelChanged += RefreshLevel;
            RefreshLevel();

            PlayerManager.Instance.OnExpChanged += RefreshExp;
            RefreshExp(PlayerManager.Instance.GetExp());

            Message.AddListener<Event.RefreshComboUIMsg>(ShowComboUI);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<Event.RefreshComboUIMsg>(ShowComboUI);
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
    }
}