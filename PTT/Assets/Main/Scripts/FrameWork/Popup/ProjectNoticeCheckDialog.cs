using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry
{
    public class ProjectNoticeCheckDialog : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text m_checkTime;

        [SerializeField]
        private Button m_gameExit;

        [SerializeField]
        private Button m_goDiscode;

        void Start()
        {
            if (m_gameExit != null)
                m_gameExit.onClick.AddListener(Managers.SceneManager.Instance.OnApplicationQuit);

            if (m_goDiscode != null)
                m_goDiscode.onClick.AddListener(Managers.SceneManager.Instance.GoCommunity);
        }

        public void SetCheckTime(string checktime)
        {
            if (m_checkTime != null)
                m_checkTime.text = checktime;
        }
    }
}