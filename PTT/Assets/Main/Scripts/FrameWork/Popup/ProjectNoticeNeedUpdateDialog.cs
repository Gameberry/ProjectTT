using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry
{
    public class ProjectNoticeNeedUpdateDialog : MonoBehaviour
    {
        [SerializeField]
        private Button m_gameExit;

        [SerializeField]
        private Button m_goStore;

        void Start()
        {
            if (m_gameExit != null)
                m_gameExit.onClick.AddListener(Managers.SceneManager.Instance.OnApplicationQuit);

            if (m_goStore != null)
                m_goStore.onClick.AddListener(Managers.SceneManager.Instance.GoStore);
        }
    }
}