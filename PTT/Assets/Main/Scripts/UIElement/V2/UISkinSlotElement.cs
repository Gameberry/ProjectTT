using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UISkinSlotElement : MonoBehaviour
    {
        [SerializeField]
        private V2Enum_Skin m_mySkinType = V2Enum_Skin.Max;

        [SerializeField]
        private Image m_gearIcon;

        [SerializeField]
        private Image m_gradeColorBG;

        [SerializeField]
        private Image m_noneSlotImage;

        [SerializeField]
        private Image m_grade_Text_BG;

        [SerializeField]
        private TMP_Text m_grade_Text;

        [SerializeField]
        private TMP_Text m_gearLevel_Text;

        [SerializeField]
        private Button m_gearSlotBtn;

        [SerializeField]
        private ElementFrameResource m_frameElementResource;

        private System.Action<V2Enum_Skin> m_action = null;

        private int m_myIndex = -1;

        //------------------------------------------------------------------------------------
        public void Init(System.Action<V2Enum_Skin> action)
        {
            m_action = action;

            if (m_gearSlotBtn != null)
                m_gearSlotBtn.onClick.AddListener(OnClick_Slot);

            SetSkinData(null);
        }
        //------------------------------------------------------------------------------------
        public V2Enum_Skin GetSkinType()
        {
            return m_mySkinType;
        }
        //------------------------------------------------------------------------------------
        public void SetSkinData(CharacterSkinData characterGearData)
        {
            if (characterGearData == null)
            {
                if (m_gearIcon != null)
                    m_gearIcon.gameObject.SetActive(false);

                if (m_gradeColorBG != null)
                    m_gradeColorBG.color = Color.black;

                if (m_grade_Text_BG != null)
                    m_grade_Text_BG.gameObject.SetActive(false);

                if (m_grade_Text != null)
                    m_grade_Text.gameObject.SetActive(false);

                if (m_gearLevel_Text != null)
                    m_gearLevel_Text.gameObject.SetActive(false);

                if (m_noneSlotImage != null)
                    m_noneSlotImage.gameObject.SetActive(true);

                if (m_frameElementResource != null)
                    m_frameElementResource.SetFrame(V2Enum_Grade.Max);

                m_myIndex = -1;

                return;
            }

            if (m_noneSlotImage != null)
                m_noneSlotImage.gameObject.SetActive(false);

            if (m_gearIcon != null)
            {
                m_gearIcon.sprite = Managers.CharacterSkinManager.Instance.GetSkinSprite(characterGearData);
                m_gearIcon.gameObject.SetActive(true);
            }

            if (m_gradeColorBG != null)
                m_gradeColorBG.color = StaticResource.Instance.GetV2GradeColor(characterGearData.SkinGrade);

            if (m_grade_Text_BG != null)
                m_grade_Text_BG.gameObject.SetActive(true);

            if (m_grade_Text != null)
            {
                m_grade_Text.SetText(characterGearData.SkinGrade.ToString());
                m_grade_Text.gameObject.SetActive(true);

                m_grade_Text.color = StaticResource.Instance.GetV2GradeTextColor(characterGearData.SkinGrade);

                V2GradeColorData v2GradeColorData = StaticResource.Instance.GetV2GradeColorData(characterGearData.SkinGrade);
                m_grade_Text.enableVertexGradient = v2GradeColorData.UseGradeTextGradation;
                if (v2GradeColorData.UseGradeTextGradation == true)
                {
                    m_grade_Text.colorGradient = v2GradeColorData.GradeTextColorGradient;
                }
            }

            if (m_frameElementResource != null)
                m_frameElementResource.SetFrame(characterGearData.SkinGrade);

            PlayerSkinInfo playerGearInfo = Managers.CharacterSkinManager.Instance.GetPlayerSkinInfo(characterGearData);

            if (playerGearInfo == null)
            {
                if (m_gearLevel_Text != null)
                    m_gearLevel_Text.gameObject.SetActive(false);
            }
            else
            {
                if (m_gearLevel_Text != null)
                {
                    m_gearLevel_Text.SetText("Lv {0}", playerGearInfo.Level);
                    m_gearLevel_Text.gameObject.SetActive(true);
                }
            }

            m_myIndex = characterGearData.Index;
        }
        //------------------------------------------------------------------------------------
        public void HideLeve()
        {
            if (m_gearLevel_Text != null)
                m_gearLevel_Text.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Slot()
        {
            if (m_action != null)
                m_action(m_mySkinType);
        }
        //------------------------------------------------------------------------------------
        public int GetGearIndex()
        {
            return m_myIndex;
        }
        //------------------------------------------------------------------------------------
    }
}