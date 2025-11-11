using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    [System.Serializable]
    public class SummonBtn
    {
        public Button summonBtn;
        public TMP_Text summonCount, summonPrice;
        public Image summonIcon;

        private System.Action<SummonCostData> m_action;

        public SummonCostData summonCostData;

        public void Init(System.Action<SummonCostData> action)
        {
            if (summonBtn != null)
                summonBtn.onClick.AddListener(OnClick_Btn);

            m_action = action;
        }

        private void OnClick_Btn()
        {
            if (m_action != null)
                m_action(summonCostData);
        }
    }

    public class UIShopSummonElement : MonoBehaviour
    {
        [SerializeField]
        private Image m_titleIcon;

        [SerializeField]
        private TMP_Text m_title;

        [SerializeField]
        private Image m_bigIcon;

        [SerializeField]
        private Button m_viewPercent;

        [Header("------------SummonBtn------------")]
        [SerializeField]
        private Button m_summonAdBtn;

        [SerializeField]
        private Image m_summonAdLight;

        [SerializeField]
        private List<SummonBtn> m_summonBtn_List;

        [SerializeField]
        private Color m_summonBtn_Disable;

        [SerializeField]
        private Color m_summonBtn_Enable;

        [SerializeField]
        private Color m_summonADBtn_Enable;


        [SerializeField]
        private TMP_Text m_summonAdTitle, m_summonAdValue;

        [SerializeField]
        private Color m_summonText_Disable;

        [SerializeField]
        private Color m_summonText_Enable;

        [SerializeField]
        private UISummonConfirmElement _uISummonConfirmElement;

        private SummonData m_currentSummonData = null;

        private GameBerry.Event.SetShopSummonPercentageDialogMsg m_setShopSummonPercentageDialogMsg = new GameBerry.Event.SetShopSummonPercentageDialogMsg();

        //------------------------------------------------------------------------------------
        public void Init(V2Enum_SummonType v2Enum_SummonType)
        {
            if (m_viewPercent != null)
                m_viewPercent.onClick.AddListener(OnClick_ViewPercent);

            if (m_titleIcon != null)
                m_titleIcon.sprite = Managers.SummonManager.Instance.GetSummonTypeSprite(v2Enum_SummonType);

            if (m_bigIcon != null)
                m_bigIcon.sprite = Managers.SummonManager.Instance.GetSummonTypeSprite(v2Enum_SummonType);

            if (_uISummonConfirmElement != null)
            { 
                _uISummonConfirmElement.Init(v2Enum_SummonType);
                _uISummonConfirmElement.RefreshConfirm();
            }

            SummonData summonData = Managers.SummonManager.Instance.GetSummonData(v2Enum_SummonType);
            if (m_title != null)
            {
                m_title.gameObject.SetActive(summonData != null);
                if (summonData != null)
                {
                    Managers.LocalStringManager.Instance.SetLocalizeText(m_title, summonData.NameLocalStringKey);
                }
            }

            if (m_summonAdBtn != null)
                m_summonAdBtn.onClick.AddListener(OnClick_AdSummon);

            for (int i = 0; i < m_summonBtn_List.Count; ++i)
            {
                m_summonBtn_List[i].Init(OnClick_Summon);

                //if (i == 0 && m_summonBtn_List[i].summonBtn != null)
                //{
                //    UIGuideInteractor uIGuideInteractor = m_summonBtn_List[i].summonBtn.gameObject.AddComponent<UIGuideInteractor>();
                //    switch (v2Enum_SummonType)
                //    {
                //        case V2Enum_SummonType.SummonCharacterGear:
                //            {
                //                uIGuideInteractor.MyGuideType = V2Enum_EventType.WeaponSummon;
                //                break;
                //            }
                //        case V2Enum_SummonType.SummonNormal:
                //            {
                //                uIGuideInteractor.MyGuideType = V2Enum_EventType.SkillSummon;
                //                break;
                //            }

                //        case V2Enum_SummonType.SummonRelic:
                //            {
                //                uIGuideInteractor.MyGuideType = V2Enum_EventType.AllySummon;
                //                break;
                //            }
                //    }

                //    uIGuideInteractor.MyStepID = 3;
                //    uIGuideInteractor.FocusAngle = 180.0f;
                //    uIGuideInteractor.FocusParent = m_summonBtn_List[i].summonBtn.transform;
                //    uIGuideInteractor.GuideInteratorActionType = Managers.GuideInteratorActionType.End;
                //    uIGuideInteractor.IsAutoSetting = false;
                //    uIGuideInteractor.ConnectInteractor();
                //}
            }

            m_currentSummonData = summonData;
        }
        //------------------------------------------------------------------------------------
        public void SetSummonElement(V2Enum_SummonType v2Enum_SummonType)
        {
            SummonData summonData = Managers.SummonManager.Instance.GetSummonData(v2Enum_SummonType);
            SummonInfo summonInfo = Managers.SummonManager.Instance.GetSummonInfo(v2Enum_SummonType);

            m_currentSummonData = summonData;
            if (summonInfo == null)
                return;

            SetAdSummonBtn();
            SetSummonBtn();
        }
        //------------------------------------------------------------------------------------
        public void SetAdSummonBtn()
        {
            if (m_currentSummonData == null)
                return;

            bool IsAlready = Managers.SummonManager.Instance.IsReadyAdSummon(m_currentSummonData.SummonType);

            if (m_summonAdBtn != null)
            {
                m_summonAdBtn.interactable = IsAlready;
                m_summonAdBtn.image.color = IsAlready == true ? m_summonADBtn_Enable : m_summonBtn_Disable;
            }

            if (m_summonAdLight != null)
                m_summonAdLight.gameObject.SetActive(IsAlready);

            if (m_summonAdTitle != null)
            { 
                m_summonAdTitle.SetText("x{0}", Managers.SummonManager.Instance.GetAdSummonElementCount(m_currentSummonData.SummonType));
                m_summonAdTitle.color = IsAlready == true ? m_summonText_Enable : m_summonText_Disable;
            }

            if (m_summonAdValue != null)
            { 
                m_summonAdValue.SetText("({0}/{1})", Managers.SummonManager.Instance.GetAdSummonRemainCount(m_currentSummonData.SummonType), m_currentSummonData.MaxAdViewCountPerDate);
                m_summonAdValue.color = IsAlready == true ? m_summonText_Enable : m_summonText_Disable;
            }
        }
        //------------------------------------------------------------------------------------
        public void SetSummonBtn()
        {
            if (m_currentSummonData == null)
                return;

            for (int i = 0; i < m_summonBtn_List.Count; ++i)
            {
                SummonBtn summonBtn = m_summonBtn_List[i];

                if (m_currentSummonData.SummonCostDatas.Count <= i)
                {
                    if (summonBtn.summonBtn != null)
                        summonBtn.summonBtn.gameObject.SetActive(false);

                    if (summonBtn.summonCount != null)
                        summonBtn.summonCount.gameObject.SetActive(false);

                    if (summonBtn.summonPrice != null)
                        summonBtn.summonPrice.gameObject.SetActive(false);

                    if (summonBtn.summonIcon != null)
                        summonBtn.summonIcon.gameObject.SetActive(false);
                    continue;
                }

                SummonCostData summonCostData = m_currentSummonData.SummonCostDatas[i];

                bool canTicketUse = Managers.SummonManager.Instance.CanTicketUse(summonCostData);

                int summonCount = summonCostData.summonCount;

                bool IsReady = false;

                if (canTicketUse == true)
                    IsReady = true;
                else
                    IsReady = Managers.SummonManager.Instance.IsReadySummon(summonCostData);

                if (summonBtn.summonBtn != null)
                {
                    summonBtn.summonBtn.gameObject.SetActive(true);
                    summonBtn.summonBtn.interactable = IsReady;
                    summonBtn.summonBtn.image.color = IsReady == true ? m_summonBtn_Enable : m_summonBtn_Disable;
                }

                if (summonBtn.summonCount != null)
                {
                    summonBtn.summonCount.gameObject.SetActive(true);
                    summonBtn.summonCount.SetText("x{0}", summonCount);
                    summonBtn.summonCount.color = IsReady == true ? m_summonText_Enable : m_summonText_Disable;
                }

                if (summonBtn.summonPrice != null)
                {
                    double costprice = 0.0;
                    if (canTicketUse == true)
                        costprice = m_currentSummonData.SummonCostParam14 * summonCount;
                    else
                        costprice = m_currentSummonData.SummonCostParam12 * summonCount;

                    summonBtn.summonPrice.gameObject.SetActive(true);
                    summonBtn.summonPrice.SetText(Util.GetAlphabetNumber(costprice));
                    summonBtn.summonPrice.color = IsReady == true ? m_summonText_Enable : m_summonText_Disable;
                }

                if (summonBtn.summonIcon != null)
                {
                    summonBtn.summonIcon.gameObject.SetActive(true);

                    int pointindex = canTicketUse == true ? m_currentSummonData.SummonCostParam13 : m_currentSummonData.SummonCostParam11;
                    summonBtn.summonIcon.sprite = Managers.GoodsManager.Instance.GetGoodsSprite(V2Enum_Goods.Point, pointindex);
                }

                summonBtn.summonCostData = summonCostData;
            }



            if (_uISummonConfirmElement != null)
            {
                _uISummonConfirmElement.RefreshConfirm();
            }
        }
        //------------------------------------------------------------------------------------
        public void RefreshElement(double amount)
        {
            SetSummonBtn();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_AdSummon()
        {
            if (m_currentSummonData == null)
                return;

            Managers.SummonManager.Instance.DoAdSummon(m_currentSummonData.SummonType);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Summon(SummonCostData summonCostData)
        {
            if (m_currentSummonData == null)
                return;

            if (summonCostData == null)
                return;

            Managers.SummonManager.Instance.DoSummon(m_currentSummonData.SummonType, summonCostData);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ViewPercent()
        {
            SummonGroupData summonGroupData = Managers.SummonManager.Instance.GetSummonGroupData(m_currentSummonData.SummonType);
            Managers.SummonManager.Instance.ShowPercendView(summonGroupData);
        }
        //------------------------------------------------------------------------------------
    }
}