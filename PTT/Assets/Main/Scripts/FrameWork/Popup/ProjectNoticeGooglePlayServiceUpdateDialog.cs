using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameBerry.UI;

namespace GameBerry
{
    public class ProjectNoticeGooglePlayServiceUpdateDialog : IDialog
    {
        [SerializeField]
        private Button _goplayservice;

        void Start()
        {
            if (_goplayservice != null)
                _goplayservice.onClick.AddListener(Managers.SceneManager.Instance.GoPlayGooglePlayServiceUpdate);
        }
    }

}
