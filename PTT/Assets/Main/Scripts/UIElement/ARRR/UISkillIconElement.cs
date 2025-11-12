using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UISkillIconElement : MonoBehaviour
    {
        [SerializeField]
        private Image _gradeColorBG;

        [SerializeField]
        private Image _gradeColorBG2;

        [SerializeField]
        private Image _gradeBGImage;

        [SerializeField]
        private TMP_Text _skillRarityText;

        [SerializeField]
        private Image _rarityImage;

        [SerializeField]
        private Image _skillIcon;

        [SerializeField]
        private TMP_Text _skillTriggerTypeText;

        [SerializeField]
        private Image _skillTriggerTypeBG;

        [SerializeField]
        private Image m_skillKind;

        [SerializeField]
        private TMP_Text _skillIndexText;

        //------------------------------------------------------------------------------------
        System.Action<SkillBaseData> _callBack = null;

        //------------------------------------------------------------------------------------
        public void SetSkillElement(SkillBaseData characterSkillData)
        {
            if (characterSkillData == null)
                return;

            if (_skillIndexText != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_skillIndexText, characterSkillData.NameLocalKey);

            SetGrade(Managers.ARRRSkillManager.Instance.GetARRRSkillGrade(characterSkillData));

            if (_skillIcon != null)
                _skillIcon.sprite = Managers.SkillManager.Instance.GetSkillIcon(characterSkillData);

            Contents.V2SkillTriggerColorData v2SkillTriggerColorData = Contents.GlobalContent.GetV2SkillTriggerColorData(characterSkillData.TriggerType);

            if (_skillTriggerTypeText != null)
            {
                _skillTriggerTypeText.text = characterSkillData.TriggerType == Enum_TriggerType.Active ? "A" : "P";
                if (v2SkillTriggerColorData != null)
                {
                    _skillTriggerTypeText.color = v2SkillTriggerColorData.TextColor;
                    _skillTriggerTypeText.material = v2SkillTriggerColorData.TextMaterial;
                }
            }

            if (_skillTriggerTypeBG != null)
            {
                if (v2SkillTriggerColorData != null)
                {
                    _skillTriggerTypeBG.color = v2SkillTriggerColorData.BGColor;
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void SetSkillElement(MainSkillData gambleSkillData)
        {
            if (gambleSkillData == null)
                return;

            if (_skillIndexText != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_skillIndexText, gambleSkillData.NameLocalKey);

            SetGrade(gambleSkillData.MainSkillGrade);

            if (_skillIcon != null)
                _skillIcon.sprite = Managers.SkillManager.Instance.GetMainSkillIcon(gambleSkillData);

            //Contents.V2SkillTriggerColorData v2SkillTriggerColorData = Contents.GlobalContent.GetV2SkillTriggerColorData(characterSkillData.TriggerType);

            //if (_skillTriggerTypeText != null)
            //{
            //    _skillTriggerTypeText.text = characterSkillData.TriggerType == Enum_TriggerType.Active ? "A" : "P";
            //    if (v2SkillTriggerColorData != null)
            //    {
            //        _skillTriggerTypeText.color = v2SkillTriggerColorData.TextColor;
            //        _skillTriggerTypeText.material = v2SkillTriggerColorData.TextMaterial;
            //    }
            //}

            //if (_skillTriggerTypeBG != null)
            //{
            //    if (v2SkillTriggerColorData != null)
            //    {
            //        _skillTriggerTypeBG.color = v2SkillTriggerColorData.BGColor;
            //    }
            //}
        }
        //------------------------------------------------------------------------------------
        public void SetSkillElement(DescendData gambleSkillData)
        {
            if (gambleSkillData == null)
                return;

            if (_skillIndexText != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_skillIndexText, gambleSkillData.NameLocalKey);

            if (_skillIcon != null)
                _skillIcon.sprite = Managers.DescendManager.Instance.GetDescendIcon(gambleSkillData);

            SetGrade(gambleSkillData.Grade);
        }
        //------------------------------------------------------------------------------------
        public void SetSkillElement(RelicData gambleSkillData)
        {
            if (gambleSkillData == null)
                return;

            if (_skillIndexText != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_skillIndexText, gambleSkillData.NameLocalKey);

            if (_skillIcon != null)
                _skillIcon.sprite = Managers.RelicManager.Instance.GetRelicIcon(gambleSkillData);
        }
        //------------------------------------------------------------------------------------
        public void SetSkillElement(SynergyRuneData gambleSkillData)
        {
            if (gambleSkillData == null)
                return;

            if (_skillIndexText != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_skillIndexText, gambleSkillData.NameLocalKey);

            if (_skillIcon != null)
                _skillIcon.sprite = Managers.SynergyRuneManager.Instance.GetDescendIcon(gambleSkillData);

            SetGrade(gambleSkillData.Grade);
        }
        //------------------------------------------------------------------------------------
        public void SetSkillElement(GearData gambleSkillData)
        {
            if (gambleSkillData == null)
                return;

            if (_skillIndexText != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_skillIndexText, gambleSkillData.NameLocalKey);

            if (_skillIcon != null)
                _skillIcon.sprite = Managers.GearManager.Instance.GetDescendIcon(gambleSkillData);

            SetGrade(gambleSkillData.Grade);
        }
        //------------------------------------------------------------------------------------
        private void SetGrade(V2Enum_Grade v2Enum_Grade)
        {
            if (_gradeColorBG != null)
                _gradeColorBG.SetGradeColor(v2Enum_Grade);

            if (_gradeColorBG2 != null)
                _gradeColorBG2.SetGradeColor(v2Enum_Grade);

            if (_gradeBGImage != null)
                _gradeBGImage.SetGradeBGImage(v2Enum_Grade);

            if (_skillRarityText != null)
            {
                _skillRarityText.text = v2Enum_Grade.ToString();
                _skillRarityText.color = StaticResource.Instance.GetV2GradeTextColor(v2Enum_Grade);

                V2GradeColorData v2GradeColorData = StaticResource.Instance.GetV2GradeColorData(v2Enum_Grade);
                _skillRarityText.enableVertexGradient = v2GradeColorData.UseGradeTextGradation;
                if (v2GradeColorData.UseGradeTextGradation == true)
                {
                    _skillRarityText.colorGradient = v2GradeColorData.GradeTextColorGradient;
                }
            }

            if (_rarityImage != null)
                _rarityImage.SetGradeTextImage(v2Enum_Grade);
        }
        //------------------------------------------------------------------------------------
        public void VisibleGrade(bool enable)
        {
            if (_gradeColorBG != null)
                _gradeColorBG.gameObject.SetActive(enable);
        }
        //------------------------------------------------------------------------------------
    }
}