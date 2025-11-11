using UnityEngine;
using UnityEngine.UI;

namespace GameBerry.UI
{
    public class UIQuickLinkElement : MonoBehaviour
    {
        [SerializeField]
        private ContentDetailList contentDetailList = ContentDetailList.None;

        private void Awake()
        {
            Button button = GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            Managers.UIQuickLinkManager.Instance.ShowQuickLink(contentDetailList);
        }
    }
}