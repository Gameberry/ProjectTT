using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIShopElement_LimitTimeIcon : MonoBehaviour
    {
        public int PackageIndex;

        public Button PackageBtn;
        public Image Icon;
        public TMP_Text PackageRemainTimeBtn;

        private System.Action<int> callback;

        [HideInInspector]
        public bool NeedTimerCheck = false;

        public void Init(System.Action<int> action)
        {
            PackageBtn?.onClick.AddListener(OnClick);
            callback = action;
        }

        public void SetLimitTimeData(ShopPackageEventData shopPackageEventData)
        {
            PackageIndex = shopPackageEventData.Index;

            if (Icon != null)
            {
                Icon.gameObject.SetActive(true);
                Icon.sprite = Managers.ShopManager.Instance.GetPackageIcon(shopPackageEventData.LobbyIconStringKey);
            }
        }

        private void OnClick()
        {
            callback?.Invoke(PackageIndex);
        }
    }
}
