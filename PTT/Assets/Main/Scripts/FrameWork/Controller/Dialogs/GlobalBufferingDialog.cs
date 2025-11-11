using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class GlobalBufferingDialog : IDialog
    {
        [SerializeField]
        private int m_loadingBarAmount = 10;

        [SerializeField]
        private float m_speed = 10.0f;

        [SerializeField]
        private Transform m_loadingCircleGroup = null;

        [SerializeField]
        private TMP_Text m_buffState;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            Message.AddListener<GameBerry.Event.SetBuffStringMsg>(SetBuffString);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.SetBuffStringMsg>(SetBuffString);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            if (m_buffState != null)
                m_buffState.SetText(string.Empty);
        }
        //------------------------------------------------------------------------------------
        private void SetBuffString(GameBerry.Event.SetBuffStringMsg msg)
        {
            if (m_buffState != null)
                m_buffState.SetText(msg.buffstr);
        }
        //------------------------------------------------------------------------------------
    }
}