using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UISkinElement : MonoBehaviour
    {
        [SerializeField]
        private Button m_SkinElementBtn;

        [SerializeField]
        private Image m_SkinIcon;

        [SerializeField]
        private Image m_gradeColorBG;

        [SerializeField]
        private TMP_Text m_grade_Text;

        [SerializeField]
        private Image m_canLevelUPIcon;

        [SerializeField]
        private Image m_redDotIcon;

        [SerializeField]
        private TMP_Text m_SkinLevel_Text;


        [SerializeField]
        private Transform m_equipMark;

        [SerializeField]
        private Transform m_selectMark;


        [SerializeField]
        private Transform m_disableImage;

        [SerializeField]
        private ElementFrameResource m_frameElementResource;

        private CharacterSkinData m_currentSkinData = null;

        private System.Action<CharacterSkinData> m_action = null;

        [SerializeField]
        private Transform m_starGaugeGroup;

        [SerializeField]
        private Image m_starGauge;

        [SerializeField]
        private Image m_starGaugeIcon;

        [SerializeField]
        private TMP_Text m_starGaugeText;

        [SerializeField]
        private TMP_Text m_starText;

        //------------------------------------------------------------------------------------
        public void Init(System.Action<CharacterSkinData> action)
        {
            m_action = action;

            if (m_SkinElementBtn != null)
                m_SkinElementBtn.onClick.AddListener(OnClick_Element);
        }
        //------------------------------------------------------------------------------------
        public void SetSkinElement(CharacterSkinData characterSkinData)
        {
            if (characterSkinData == null)
                return;

            m_currentSkinData = characterSkinData;

            PlayerSkinInfo playerSkinInfo = Managers.CharacterSkinManager.Instance.GetPlayerSkinInfo(characterSkinData);

            if (m_gradeColorBG != null)
                m_gradeColorBG.color = StaticResource.Instance.GetV2GradeColor(characterSkinData.SkinGrade);

            if (m_grade_Text != null)
            {
                m_grade_Text.text = characterSkinData.SkinGrade.ToString();
                m_grade_Text.color = StaticResource.Instance.GetV2GradeTextColor(characterSkinData.SkinGrade);

                V2GradeColorData v2GradeColorData = StaticResource.Instance.GetV2GradeColorData(characterSkinData.SkinGrade);
                m_grade_Text.enableVertexGradient = v2GradeColorData.UseGradeTextGradation;
                if (v2GradeColorData.UseGradeTextGradation == true)
                {
                    m_grade_Text.colorGradient = v2GradeColorData.GradeTextColorGradient;
                }
            }

            if (m_frameElementResource != null)
                m_frameElementResource.SetFrame(characterSkinData.SkinGrade);

            if (m_disableImage != null)
                m_disableImage.gameObject.SetActive(playerSkinInfo == null);

            if (m_SkinIcon != null)
                m_SkinIcon.sprite = Managers.CharacterSkinManager.Instance.GetSkinSprite(characterSkinData);

            if (m_SkinLevel_Text != null)
            {
                //if (playerSkinInfo == null)
                //    m_SkinLevel_Text.gameObject.SetActive(false);
                //else
                //{
                //}

                m_SkinLevel_Text.gameObject.SetActive(true);
                m_SkinLevel_Text.text = string.Format("Lv.{0}", Managers.CharacterSkinManager.Instance.GetSkinLevel(characterSkinData));
            }

            if (m_canLevelUPIcon != null)
            {
                if (playerSkinInfo == null)
                    m_canLevelUPIcon.gameObject.SetActive(false);
                else
                {
                    m_canLevelUPIcon.gameObject.SetActive(Managers.CharacterSkinManager.Instance.CheckIsReadyEnhance(characterSkinData));
                }
            }


            if (m_starText != null)
            {
                //if (playerSkinInfo == null)
                //    m_SkinLevel_Text.gameObject.SetActive(false);
                //else
                //{
                //}
                m_starText.gameObject.SetActive(true);
                m_starText.text = string.Format("+{0}", Managers.CharacterSkinManager.Instance.GetSkinStar(characterSkinData));
            }

            if (playerSkinInfo == null)
            {
                if (m_starGaugeGroup != null)
                    m_starGaugeGroup.gameObject.SetActive(false);
            }
            else
            {
                if (m_starGaugeGroup != null)
                    m_starGaugeGroup.gameObject.SetActive(true);

                int currcount = playerSkinInfo.Count.GetDecrypted();
                int needcount = Managers.CharacterSkinManager.Instance.GetNeedStarUpSkinPrice(characterSkinData);

                float ratio = (float)currcount / (float)needcount;

                if (m_starGauge != null)
                    m_starGauge.fillAmount = ratio;

                if (m_starGaugeText != null)
                    m_starGaugeText.text = string.Format("{0}/{1}", currcount, needcount);

                if (m_starGaugeIcon != null)
                    m_starGaugeIcon.sprite = Managers.CharacterSkinManager.Instance.GetSkinSprite(characterSkinData);
            }
        }
        //------------------------------------------------------------------------------------
        public void VisibleRedDot(bool visible)
        {
            if (m_redDotIcon != null)
                m_redDotIcon.gameObject.SetActive(visible);
        }
        //------------------------------------------------------------------------------------
        public void SetEquipState(bool equip)
        {
            if (m_equipMark != null)
                m_equipMark.gameObject.SetActive(equip);
        }
        //------------------------------------------------------------------------------------
        public void SetSelectState(bool equip)
        {
            if (m_selectMark != null)
            {
                m_selectMark.gameObject.SetActive(equip);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Element()
        {
            if (m_action != null)
                m_action(m_currentSkinData);

            VisibleRedDot(false);
        }
        //------------------------------------------------------------------------------------
    }
}