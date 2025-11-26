using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UITestShowSkinSelecDialog : MonoBehaviour
    {
        private void Awake()
        {
            Button button = GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => UIManager.DialogEnter<SkinSelectDialog>());
        }
    }
}