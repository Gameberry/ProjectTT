using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Managers;
using GameBerry.Event;
using DG.Tweening;

namespace GameBerry.UI
{
    public class UIAdsPackageInteractorElement : MonoBehaviour
    {
        [SerializeField]
        private Button btn;

        private void Awake()
        {
            if (Define.IsAdFree == false)
            { 
                Message.AddListener<GameBerry.Event.ChangeAdFreeStateUIMsg>(ChangeAdFreeStateUI);

                if (btn != null)
                    btn.onClick.AddListener(() =>
                    {
                        Managers.UIQuickLinkManager.Instance.ShowQuickLink(ContentDetailList.ShopPackage);
                    });
            }
            else
                gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Define.IsAdFree == false)
                Message.RemoveListener<GameBerry.Event.ChangeAdFreeStateUIMsg>(ChangeAdFreeStateUI);
        }
        //------------------------------------------------------------------------------------
        private void ChangeAdFreeStateUI(GameBerry.Event.ChangeAdFreeStateUIMsg msg)
        {
            if (Define.IsAdFree == true)
            {
                gameObject.SetActive(false);
                Message.RemoveListener<GameBerry.Event.ChangeAdFreeStateUIMsg>(ChangeAdFreeStateUI);
            }
        }
    }
}