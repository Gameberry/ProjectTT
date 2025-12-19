using System;
using System.Collections.Generic;
using UnityEngine;
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

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            _comboGroup?.gameObject.SetActive(false);

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
    }
}