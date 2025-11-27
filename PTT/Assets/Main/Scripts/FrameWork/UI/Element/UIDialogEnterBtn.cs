using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIDialogEnterBtn : MonoBehaviour
    {
        [SerializeField]
        private string dialogName;

        private void Awake()
        {
            if (string.IsNullOrEmpty(dialogName) == true)
                return;

            Button button = GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => UIManager.Instance.DialogEnter(dialogName));
        }
    }
}