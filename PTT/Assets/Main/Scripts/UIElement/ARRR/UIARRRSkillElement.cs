using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIARRRSkillElement : MonoBehaviour
    {
        [SerializeField]
        private Button m_elementbtn;

        [SerializeField]
        private UISkillIconElement m_uISkillIconElement;

        [SerializeField]
        private Image m_canLevelUPIcon;

        [SerializeField]
        private Image m_redDotIcon;

        [SerializeField]
        private TMP_Text m_skillLevelText;

        [SerializeField]
        private Transform m_equipMark;

        [SerializeField]
        private Transform m_selectMark;

        [SerializeField]
        private Transform m_amountGaugeGroup;

        [SerializeField]
        private Image m_amountCountFilled;

        [SerializeField]
        private float m_amountFilledMaxWidth;

        [SerializeField]
        private TMP_Text m_amountCountText;

        [SerializeField]
        private Transform m_disableImage;

        //------------------------------------------------------------------------------------
        private ARRRSkillData m_currentSkillData = null;
        private SkillInfo m_currentSkillInfo = null;

        System.Action<ARRRSkillData, SkillInfo> m_callBack = null;

        //------------------------------------------------------------------------------------
        public void Awake()
        {
            if (m_elementbtn != null)
                m_elementbtn.onClick.AddListener(OnClick_ElementBtn);
        }
        //------------------------------------------------------------------------------------
        public void SetCallBack(System.Action<ARRRSkillData, SkillInfo> callback)
        {
            m_callBack = callback;
        }
        //------------------------------------------------------------------------------------
        public UIGuideInteractor SetSkillInstallGuideBtn()
        {
            if (m_elementbtn != null)
            {
                UIGuideInteractor uIGuideInteractor = m_elementbtn.gameObject.AddComponent<UIGuideInteractor>();
                uIGuideInteractor.MyGuideType = V2Enum_EventType.SkillEquip;
                uIGuideInteractor.MyStepID = 4;
                uIGuideInteractor.GuideInteratorActionType = Managers.GuideInteratorActionType.Next;
                uIGuideInteractor.IsAutoSetting = false;
                uIGuideInteractor.ConnectInteractor();

                return uIGuideInteractor;
            }

            return null;
        }
        //------------------------------------------------------------------------------------
        public void SetSkillEnhanceGuideBtn()
        {
            //UIGuideInteractor uIGuideInteractor = m_elementbtn.gameObject.AddComponent<UIGuideInteractor>();
            //uIGuideInteractor.MySubType = V2Enum_EventType.SkillLevelUp;
            //uIGuideInteractor.MyStepID = 3;
            //uIGuideInteractor.GuideInteratorActionType = Managers.GuideInteratorActionType.Next;
            //uIGuideInteractor.m_isAutoSetting = false;
            //uIGuideInteractor.ConnectInteractor();
            //m_elementbtn
        }
        //------------------------------------------------------------------------------------
        public void SetSkillElement(ARRRSkillData characterSkillData)
        {
            if (characterSkillData == null)
                return;

            SkillBaseData skillBaseData = Managers.ARRRSkillManager.Instance.GetSkillBaseData(characterSkillData);
            if (skillBaseData == null)
                return;

            m_currentSkillData = characterSkillData;

            if (m_uISkillIconElement != null)
                m_uISkillIconElement.SetSkillElement(skillBaseData);

            m_currentSkillInfo = Managers.ARRRSkillManager.Instance.GetARRRSkillInfo(skillBaseData);

            int level = Managers.ARRRSkillManager.Instance.GetSkillLevel(skillBaseData);


            if (m_disableImage != null)
                m_disableImage.gameObject.SetActive(m_currentSkillInfo == null);

            if (m_skillLevelText != null)
            {
                m_skillLevelText.gameObject.SetActive(true);
                m_skillLevelText.text = string.Format("Lv.{0}", level);
            }

            SetEquipElement(Managers.ARRRSkillManager.Instance.IsEquipSkill(characterSkillData));

            if (m_currentSkillInfo == null)
            {
                if (m_amountGaugeGroup != null)
                    m_amountGaugeGroup.gameObject.SetActive(false);

                return;
            }

            int CurrentAmount = m_currentSkillInfo == null ? 0 : m_currentSkillInfo.Count;

            int TargetAmount = Managers.ARRRSkillManager.Instance.IsMaxLevel(m_currentSkillData) == false ? Managers.ARRRSkillManager.Instance.GetLevelUpCost(m_currentSkillData) : -1;

            if (m_amountGaugeGroup != null)
                m_amountGaugeGroup.gameObject.SetActive(true);

            if (m_amountCountFilled != null)
            {
                if (TargetAmount == -1)
                {
                    m_amountCountFilled.fillAmount = 1;

                    if (m_amountCountText != null)
                    {
                        m_amountCountText.gameObject.SetActive(true);
                        //m_amountCountText.SetText("{0}", CurrentAmount);
                        m_amountCountText.SetText("Max");
                    }
                }
                else
                {
                    float ratio = (float)CurrentAmount / (float)TargetAmount;

                    m_amountCountFilled.fillAmount = ratio;

                    if (m_amountCountText != null)
                    {
                        m_amountCountText.gameObject.SetActive(true);
                        m_amountCountText.text = string.Format("{0}/{1}", CurrentAmount, TargetAmount);
                    }
                }
            }


            //if (m_canLevelUPIcon != null)
            //{
            //    if (m_currentSkillInfo == null)
            //        m_canLevelUPIcon.gameObject.SetActive(false);
            //    else
            //    {
            //        m_canLevelUPIcon.gameObject.SetActive(TargetAmount <= CurrentAmount);
            //    }
            //}
        }
        //------------------------------------------------------------------------------------
        public void VisibleRedDot(bool visible)
        {
            if (m_redDotIcon != null)
                m_redDotIcon.gameObject.SetActive(visible);
        }
        //------------------------------------------------------------------------------------
        public void SetEquipElement(bool isequip)
        {
            if (m_equipMark != null)
                m_equipMark.gameObject.SetActive(isequip);
        }
        //------------------------------------------------------------------------------------
        public void SetSelect(bool select)
        {
            if (m_selectMark != null)
                m_selectMark.gameObject.SetActive(select);
        }
        //------------------------------------------------------------------------------------
        public void VisibleAmountGaugeGroup(bool visible)
        {
            if (m_amountGaugeGroup != null)
                m_amountGaugeGroup.gameObject.SetActive(visible);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ElementBtn()
        {
            if (m_callBack != null)
                m_callBack(m_currentSkillData, m_currentSkillInfo);

            VisibleRedDot(false);
        }
        //------------------------------------------------------------------------------------
    }
}

