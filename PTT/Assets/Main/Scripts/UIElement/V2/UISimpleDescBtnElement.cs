using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UISimpleDescBtnElement : MonoBehaviour
    {
        [SerializeField]
        private string title;

        [SerializeField]
        private string desc;

        [SerializeField]
        private Button btn;

        private void Awake()
        {
            btn?.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            Contents.InGameContent.ShowSimpleDescPopup(title, desc);
        }
    }
}