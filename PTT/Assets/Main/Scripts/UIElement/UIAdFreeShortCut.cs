using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIAdFreeShortCut : MonoBehaviour
    {
        [SerializeField]
        private Button btn;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (btn != null)
                btn.onClick.AddListener(() =>
                {
                    Managers.UIQuickLinkManager.Instance.ShowQuickLink(ContentDetailList.ShopVipStore_AD);
                });

            Managers.VipPackageManager.Instance.changeAdFreeMode += SetGameObjectState;

            SetGameObjectState();
        }
        //------------------------------------------------------------------------------------
        private void SetGameObjectState()
        {
            gameObject.SetActive(Define.IsAdFree == false);
        }
        //------------------------------------------------------------------------------------
    }
}