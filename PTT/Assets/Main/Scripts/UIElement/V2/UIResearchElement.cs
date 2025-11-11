using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    [System.Serializable]
    public class UIMastery_StateColor
    {
        public Image Image;
        public Color DisableColor;
        public Color EnableColor;
        public Color MaxColor;
    }

    public class UIResearchElement : MonoBehaviour
    {
        [SerializeField]
        private Button m_masteryElementBtn;

        [SerializeField]
        private Image m_masteryIcon;

        [SerializeField]
        private Color m_masteryIconDisableColor;

        [SerializeField]
        private Color m_masteryIconEnableColor;

        [SerializeField]
        private List<UIMastery_StateColor> m_mastery_StateColors = new List<UIMastery_StateColor>();

        [SerializeField]
        private TMP_Text m_masteryLevel_Text;

        [SerializeField]
        private Transform m_selectMark;

        [SerializeField]
        private Image m_infinityMark;

        [SerializeField]
        private TMP_Text m_maxMark;

        [SerializeField]
        private Transform m_disableImage;

        [SerializeField]
        private Transform _doingResearch;

        private ResearchData m_currentMasteryData = null;

        private System.Action<ResearchData> m_action = null;

        //------------------------------------------------------------------------------------
        public void Init(System.Action<ResearchData> action)
        {
            m_action = action;

            if (m_masteryElementBtn != null)
                m_masteryElementBtn.onClick.AddListener(OnClick_Element);
        }
        //------------------------------------------------------------------------------------
        public void SetResearchElement(ResearchData charactermasteryData)
        {
            if (charactermasteryData == null)
                return;

            m_currentMasteryData = charactermasteryData;

            PlayerResearchInfo playermasteryInfo = Managers.ResearchManager.Instance.GetPlayerResearchInfo(charactermasteryData);
            
            bool isMaxLevel = Managers.ResearchManager.Instance.IsMaxLevel(charactermasteryData);

            bool isOpen = Managers.ResearchManager.Instance.IsOpenCondition(charactermasteryData);

            for (int i = 0; i < m_mastery_StateColors.Count; ++i)
            {
                Color selectColor = Color.white;
                if (isOpen == false)
                    selectColor = m_mastery_StateColors[i].DisableColor;
                else
                {
                    if (isMaxLevel == true)
                        selectColor = m_mastery_StateColors[i].MaxColor;
                    else
                        selectColor = m_mastery_StateColors[i].EnableColor;
                }

                if (m_mastery_StateColors[i].Image != null)
                    m_mastery_StateColors[i].Image.color = selectColor;
            }

            if (m_masteryIcon != null)
            {
                m_masteryIcon.sprite = Managers.ResearchManager.Instance.GetResearchSprite(charactermasteryData);

                if (isMaxLevel == true)
                {
                    m_masteryIcon.color = Color.white;
                }
                else
                {
                    m_masteryIcon.color = isOpen == false ? m_masteryIconDisableColor : m_masteryIconEnableColor;
                }
            }

            int currentLevel = playermasteryInfo == null ? Define.PlayerJewelryDefaultLevel : playermasteryInfo.Level;

            if (charactermasteryData.ResearchMaxLevel == -1)
            {
                if (m_masteryLevel_Text != null)
                    m_masteryLevel_Text.SetText("{0}", currentLevel);
            }
            else
            {
                if (m_masteryLevel_Text != null)
                    m_masteryLevel_Text.SetText("{0}/{1}", currentLevel, charactermasteryData.ResearchMaxLevel);
            }

            if (m_infinityMark != null)
            { 
                m_infinityMark.gameObject.SetActive(charactermasteryData.ResearchMaxLevel == -1);
                m_infinityMark.color = isOpen == false ? m_masteryIconDisableColor : m_masteryIconEnableColor;
            }

            if (m_maxMark != null)
                m_maxMark.gameObject.SetActive(isMaxLevel);

            if (_doingResearch != null)
                _doingResearch.gameObject.SetActive(Managers.ResearchManager.Instance.IsDoingResearch(charactermasteryData));
            //if (m_disableImage != null)
            //    m_disableImage.gameObject.SetActive(playermasteryInfo == null);
        }
        //------------------------------------------------------------------------------------
        public void SetSelectState(bool equip)
        {
            if (m_selectMark != null)
            {
                m_selectMark.gameObject.SetActive(equip);
                //if (equip == true)
                //{
                //    m_selectMark.Stop();
                //    m_selectMark.Play();
                //}
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Element()
        {
            if (m_action != null)
                m_action(m_currentMasteryData);
        }
        //------------------------------------------------------------------------------------
        public UIGuideInteractor SetGuideInteractor()
        {
            UIGuideInteractor uIGuideInteractor = m_masteryElementBtn.gameObject.AddComponent<UIGuideInteractor>();
            uIGuideInteractor.MyGuideType = V2Enum_EventType.ResearchTutorial;
            uIGuideInteractor.MyStepID = 5;
            uIGuideInteractor.FocusAngle = 0;
            uIGuideInteractor.FocusParent = transform;
            uIGuideInteractor.GuideInteratorActionType = Managers.GuideInteratorActionType.Next;
            uIGuideInteractor.IsAutoSetting = false;
            uIGuideInteractor.ConnectInteractor();

            return uIGuideInteractor;
        }
    }
}