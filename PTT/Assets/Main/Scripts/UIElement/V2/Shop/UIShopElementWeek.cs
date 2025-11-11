using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIShopElementWeek : UIShopElement
    {
        [SerializeField]
        private Transform m_elementRoot;

        [SerializeField]
        private Image m_ad;

        private List<UIGlobalGoodsRewardIconElement> uIGlobalGoodsRewardIconElements = new List<UIGlobalGoodsRewardIconElement>();

        public override void SetShopElement(ShopDataBase shopDataBase)
        {
            base.SetShopElement(shopDataBase);

            ShopPackageData shopPackageData = shopDataBase as ShopPackageData;  

            for (int i = 0; i < shopDataBase.ShopRewardData.Count; ++i)
            {
                UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = Managers.RewardManager.Instance.GetGoodsRewardIcon_NoneParticle();
                if (uIGlobalGoodsRewardIconElement == null)
                    return;

                RewardData rewardData = shopDataBase.ShopRewardData[i];

                uIGlobalGoodsRewardIconElement.gameObject.SetActive(true);
                uIGlobalGoodsRewardIconElement.transform.SetParent(m_elementRoot);
                uIGlobalGoodsRewardIconElement.SetRewardElement(rewardData);
                uIGlobalGoodsRewardIconElement.transform.localScale = Vector3.one;
                //uIGlobalGoodsRewardIconElement.ShowLightCircle();

            }

            if (m_ad != null)
            {
                m_ad.gameObject.SetActive(Managers.ShopManager.Instance.IsAD(shopDataBase));
            }
        }
    }
}