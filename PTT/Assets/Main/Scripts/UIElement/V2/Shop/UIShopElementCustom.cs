using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Contents;

namespace GameBerry.UI
{
    public class UIShopElementCustom : MonoBehaviour
    {
        [SerializeField]
        private Image m_elementBG;
        [SerializeField]
        private Color m_elementBG_originColor = Color.white;

        [SerializeField]
        private Image m_elementGraBG;
        [SerializeField]
        private Color m_elementGraBG_originColor = Color.white;

        [SerializeField]
        private Image m_elementFrame1;
        [SerializeField]
        private Color m_elementFrame1_originColor = Color.white;

        [SerializeField]
        private Image m_elementFrame2;
        [SerializeField]
        private Color m_elementFrame2_originColor = Color.white;

        [SerializeField]
        private Image m_elementTitleIcon;

        [SerializeField]
        private Transform m_elementIconRoot;

        private GameObject m_icon;

        [Header("-----------Cheat-----------")]
        [SerializeField]
        private InGameContent InGameContent;

        [SerializeField]
        private ContentDetailList Cheat_ContentDetailList;

        [SerializeField]
        private int Cheat_ResoueceIndex;

        [SerializeField]
        private GameObject Cheat_Icon;

        public void SetCustomElement(ContentDetailList contentDetailList, int resoueceindex)
        {
            ShopElementCustomResource shopElementCustomResource = InGameContent.GetShopElementCustomResource(contentDetailList, resoueceindex);
            if (m_icon != null)
                Destroy(m_icon);

            if (shopElementCustomResource == null)
            {
                if (m_elementBG != null)
                    m_elementBG.color = m_elementBG_originColor;

                if (m_elementGraBG != null)
                    m_elementGraBG.color = m_elementGraBG_originColor;

                if (m_elementFrame1 != null)
                    m_elementFrame1.color = m_elementFrame1_originColor;

                if (m_elementFrame2 != null)
                    m_elementFrame2.color = m_elementFrame2_originColor;

                if (m_elementTitleIcon != null)
                    m_elementTitleIcon.gameObject.SetActive(false);

                return;
            }

            if (m_elementBG != null)
                m_elementBG.color = shopElementCustomResource.PackageBG_Color;

            if (m_elementGraBG != null)
                m_elementGraBG.color = shopElementCustomResource.PackageGraBG_Color;

            if (m_elementFrame1 != null)
                m_elementFrame1.color = shopElementCustomResource.PackageFrame1_Color;

            if (m_elementFrame2 != null)
                m_elementFrame2.color = shopElementCustomResource.PackageFrame2_Color;

            if (m_elementTitleIcon != null)
            {
                if (shopElementCustomResource.PackageTitleIcon_Sprite == null)
                    m_elementTitleIcon.gameObject.SetActive(false);
                else
                {
                    m_elementTitleIcon.gameObject.SetActive(true);
                    m_elementTitleIcon.sprite = shopElementCustomResource.PackageTitleIcon_Sprite;
                }
            }

            if (m_elementIconRoot != null && shopElementCustomResource.PackageIcon != null)
            {
                m_icon = Instantiate(shopElementCustomResource.PackageIcon, m_elementIconRoot);
            }
        }
        //------------------------------------------------------------------------------------
        [ContextMenu("AddCustomResource")]
        public void AddCustomResource()
        {
            ShopElementCustomResource shopElementCustomResource = new ShopElementCustomResource();
            shopElementCustomResource.ContentDetailList = Cheat_ContentDetailList;
            shopElementCustomResource.ResoueceIndex = Cheat_ResoueceIndex;

            if (m_elementBG != null)
                shopElementCustomResource.PackageBG_Color = m_elementBG.color;

            if (m_elementGraBG != null)
                shopElementCustomResource.PackageGraBG_Color = m_elementGraBG.color;

            if (m_elementFrame1 != null)
                shopElementCustomResource.PackageFrame1_Color = m_elementFrame1.color;

            if (m_elementFrame2 != null)
                shopElementCustomResource.PackageFrame2_Color = m_elementFrame2.color;

            if (m_elementTitleIcon != null)
                shopElementCustomResource.PackageTitleIcon_Sprite = m_elementTitleIcon.sprite;

            if (Cheat_Icon != null)
                shopElementCustomResource.PackageIcon = Cheat_Icon;

            InGameContent.AddShopElementCustomResource(shopElementCustomResource);

#if UNITY_EDITOR

            //UnityEditor.PrefabUtility.SaveAsPrefabAsset(InGameContent.gameObject, InGameContent.gameObject.pa); //¿˙¿Â
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
        //------------------------------------------------------------------------------------
    }
}