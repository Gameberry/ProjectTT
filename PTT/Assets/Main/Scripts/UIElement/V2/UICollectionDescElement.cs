using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UICollectionDescElement : MonoBehaviour
    {
        [SerializeField]
        private Image m_icon;

        [SerializeField]
        private Image m_gradeBG;

        [SerializeField]
        private Image m_grade_ImageText;

        [SerializeField]
        private TMP_Text m_name;

        [SerializeField]
        private TMP_Text m_desc;

        [SerializeField]
        private Button _detailInfo;

        private int _index = -1;

        //------------------------------------------------------------------------------------
        private void Start()
        {
            if (_detailInfo != null)
                _detailInfo.onClick.AddListener(ShowGoodsInfo);
        }
        //------------------------------------------------------------------------------------
        public void SetCollectionElement(Sprite sprite, V2Enum_Grade v2Enum_Grade, string name, string desc, int index = -1)
        {
            if (m_icon != null)
                m_icon.sprite = sprite;

            if (m_grade_ImageText != null)
            {
                if (v2Enum_Grade == V2Enum_Grade.Max)
                    m_grade_ImageText.gameObject.SetActive(false);
                else
                {
                    m_grade_ImageText.gameObject.SetActive(true);
                    m_grade_ImageText.SetGradeTextImage(v2Enum_Grade);
                }
            }

            if (m_gradeBG != null)
            {
                if (v2Enum_Grade == V2Enum_Grade.Max)
                    m_gradeBG.gameObject.SetActive(false);
                else
                {
                    m_gradeBG.gameObject.SetActive(true);
                    m_gradeBG.color = StaticResource.Instance.GetV2GradeColor(v2Enum_Grade);
                }
            }

            if (m_name != null)
                m_name.SetText(name);

            if (m_desc != null)
                m_desc.SetText(desc);

            _index = index;
        }
        //------------------------------------------------------------------------------------
        public void SetCollectionName_Localize(string localstring, double amount = 0)
        {
            if (m_name != null)
            {
                if (amount >= 2)
                {
                    m_name.SetText(string.Format("{0} x{1:0}", Managers.LocalStringManager.Instance.GetLocalString(localstring), amount));
                }
                else
                {
                    Managers.LocalStringManager.Instance.SetLocalizeText(m_name, localstring);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void ShowGoodsInfo()
        {
            if (_index == -1)
                return;

            Contents.GlobalContent.ShowGoodsDescPopup(V2Enum_Goods.Max, _index);
        }
        //------------------------------------------------------------------------------------
    }
}