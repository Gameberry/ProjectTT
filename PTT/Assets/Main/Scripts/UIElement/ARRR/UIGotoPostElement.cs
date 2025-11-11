using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIGotoPostElement : MonoBehaviour
    {
        [SerializeField]
        private Button _showPost;

        void Start()
        {
            if (_showPost != null)
                _showPost.onClick.AddListener(OnClick);

        }

        private void OnClick()
        {
            UI.IDialog.RequestDialogEnter<UI.InGamePostPopupDialog>();
        }
    }
}