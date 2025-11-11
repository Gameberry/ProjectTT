using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameBerry.UI;

namespace GameBerry
{
    public class ProjectNoticeExitGameDialog : IDialog
    {
        [SerializeField]
        private Button m_gameExit;

        [SerializeField]
        private Button m_closePopup;

        void Start()
        {
            if (m_gameExit != null)
                m_gameExit.onClick.AddListener(Managers.SceneManager.Instance.OnApplicationQuit);

            if (m_closePopup != null)
                m_closePopup.onClick.AddListener(() =>
                {
                    ElementExit();
                    //gameObject.SetActive(false);
                });
        }
    }
}