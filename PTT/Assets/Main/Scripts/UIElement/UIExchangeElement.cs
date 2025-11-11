using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIExchangeElement : MonoBehaviour
    {
        [SerializeField]
        private UIGlobalGoodsRewardIconElement m_materialGoods;

        [SerializeField]
        private TMP_Text m_materialobtain;

        [SerializeField]
        private UIGlobalGoodsRewardIconElement m_returnGoods;

        [SerializeField]
        private TMP_Text m_returnobtain;

        [SerializeField]
        private TMP_Text m_remainCount;

        [SerializeField]
        private Color m_remainCount_Color;

        [SerializeField]
        private Color m_disableRemainCount_Color;


        [SerializeField]
        private Button m_minBtn;

        [SerializeField]
        private Button m_maxBtn;


        [SerializeField]
        private Button m_inCreaseBtn;

        [SerializeField]
        private Button m_deCreaseBtn;

        [SerializeField]
        private Transform m_costGoodsGroup;

        [SerializeField]
        private Image m_costGoodsIcon;

        [SerializeField]
        private TMP_Text m_costGoodsText;

        [SerializeField]
        private TMP_Text m_doExChangeCount;


        [SerializeField]
        private Button m_exChangeBtn;

        [SerializeField]
        private Color m_disableExChange_Btn;

        [SerializeField]
        private Color m_enableExChange_Btn;

        [SerializeField]
        private TMP_Text m_exChangeText;

        [SerializeField]
        private Image m_doExChangeImage;

        private ExchangeData m_currentExchangeData = null;

        private int m_currentCount = 1;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (m_minBtn != null)
                m_minBtn.onClick.AddListener(OnClick_MinBtn);

            if (m_maxBtn != null)
                m_maxBtn.onClick.AddListener(OnClick_MaxBtn);


            if (m_inCreaseBtn != null)
                m_inCreaseBtn.onClick.AddListener(OnClick_InCreaseBtn);

            if (m_deCreaseBtn != null)
                m_deCreaseBtn.onClick.AddListener(OnClick_DeCreaseBtn);


            if (m_exChangeBtn != null)
                m_exChangeBtn.onClick.AddListener(OnClick_ExChangeBtn);
        }
        //------------------------------------------------------------------------------------
        public void SetExchangeElement(ExchangeData exchangeData)
        {
            if (exchangeData == null)
                return;

            m_currentExchangeData = exchangeData;

            if (m_costGoodsGroup != null)
                m_costGoodsGroup.gameObject.SetActive(m_currentExchangeData.CostGoodsParam2 > 0);
        }
        //------------------------------------------------------------------------------------
        public void SetExchangeCount(int count)
        {
            if (m_currentExchangeData == null)
                return;

            m_currentCount = count;

            int canMaxChangeCount = Managers.ExchangeManager.Instance.GetCanMaxChangeCount(m_currentExchangeData);

            if (m_remainCount != null)
            {
                int ToDayExchangeCount = Managers.ExchangeManager.Instance.GetToDayExchangeCount(m_currentExchangeData);

                m_remainCount.SetText("{0}/{1}"
                , ToDayExchangeCount
                , m_currentExchangeData.ExchangeLimit);

                m_remainCount.color = ToDayExchangeCount >= m_currentExchangeData.ExchangeLimit ? m_disableRemainCount_Color : m_remainCount_Color;
            }

            if (m_currentExchangeData.MaterialGoodsParam2 > 0
                && m_materialGoods != null)
            {
                if (m_currentExchangeData.MaterialGoods != null)
                {
                    m_currentExchangeData.MaterialGoods.Amount = m_currentExchangeData.MaterialGoodsParam2 * count;
                    m_materialGoods.SetRewardElement(m_currentExchangeData.MaterialGoods);
                }

                if (m_materialobtain != null)
                    m_materialobtain.SetText(
                        Util.GetAlphabetNumber(Managers.GoodsManager.Instance.GetGoodsAmount(m_currentExchangeData.MaterialGoodsType.Enum32ToInt(), m_currentExchangeData.MaterialGoodsParam1))
                        );
            }

            if (m_currentExchangeData.ReturnGoodsParam2 > 0
                && m_materialGoods != null)
            {
                if (m_currentExchangeData.ReturnGoods != null)
                {
                    m_currentExchangeData.ReturnGoods.Amount = m_currentExchangeData.ReturnGoodsParam2 * count;
                    m_returnGoods.SetRewardElement(m_currentExchangeData.ReturnGoods);
                }

                if (m_returnobtain != null)
                    m_returnobtain.SetText(
                        Util.GetAlphabetNumber(Managers.GoodsManager.Instance.GetGoodsAmount(m_currentExchangeData.ReturnGoodsType.Enum32ToInt(), m_currentExchangeData.ReturnGoodsParam1))
                        );
            }

            if (m_currentExchangeData.CostGoodsParam2 > 0)
            {
                if (m_costGoodsText != null)
                    m_costGoodsText.text = Util.GetAlphabetNumber(m_currentExchangeData.CostGoodsParam2 * count);
            }

            if (m_doExChangeCount != null)
                m_doExChangeCount.SetText("{0}", count);

            if (m_doExChangeImage != null)
                m_doExChangeImage.gameObject.SetActive(canMaxChangeCount >= count);

            if (m_exChangeBtn != null)
            {
                m_exChangeBtn.interactable = canMaxChangeCount >= count;
                m_exChangeBtn.image.color = canMaxChangeCount >= count ? m_enableExChange_Btn: m_disableExChange_Btn;
            }

            if (m_exChangeText != null)
                m_exChangeText.color = canMaxChangeCount >= count ? m_enableExChange_Btn : m_disableExChange_Btn;
        }
        //------------------------------------------------------------------------------------
        public void RefreshExchangeCount()
        {
            SetExchangeCount(m_currentCount);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_MinBtn()
        {
            if (m_currentExchangeData == null)
                return;
            
            SetExchangeCount(1);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_MaxBtn()
        {
            if (m_currentExchangeData == null)
                return;

            int count = Managers.ExchangeManager.Instance.GetCanMaxChangeCount(m_currentExchangeData);

            if (count < 1)
                count = 1;

            SetExchangeCount(count);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_InCreaseBtn()
        {
            if (m_currentExchangeData == null)
                return;

            if (m_currentCount >= m_currentExchangeData.ExchangeLimit)
                return;

            SetExchangeCount(m_currentCount + 1);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_DeCreaseBtn()
        {
            if (m_currentExchangeData == null)
                return;

            if (m_currentCount <= 1)
                return;

            SetExchangeCount(m_currentCount - 1);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ExChangeBtn()
        {
            if (m_currentExchangeData == null)
                return;

            if (m_currentCount < 1)
                return;

            Managers.ExchangeManager.Instance.DoExchange(m_currentExchangeData, m_currentCount);
        }
        //------------------------------------------------------------------------------------
    }
}