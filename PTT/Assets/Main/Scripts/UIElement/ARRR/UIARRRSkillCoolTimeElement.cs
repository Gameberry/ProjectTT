using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIARRRSkillCoolTimeElement : MonoBehaviour
    {

        [SerializeField]
        private UISkillIconElement _uISkillIconElement;

        [SerializeField]
        private Image _skillRemainCoolTimeFilled;

        [SerializeField]
        private TMP_Text _skillRemainCoolTimeText;

        private SkillManageInfo _skillManageInfo;

        //------------------------------------------------------------------------------------
        public void SetSkillManageInfo(SkillManageInfo skillManageInfo)
        {
            if (skillManageInfo == null)
                return;

            if (_uISkillIconElement != null)
                _uISkillIconElement.SetSkillElement(skillManageInfo.SkillBaseData);

            _skillManageInfo = skillManageInfo;

            SkillBaseData skillBaseData = skillManageInfo.SkillBaseData;

            if (_skillRemainCoolTimeFilled != null)
                _skillRemainCoolTimeFilled.gameObject.SetActive(skillBaseData.IsUseCoolTime == true);

            if (_skillRemainCoolTimeText != null)
                _skillRemainCoolTimeText.gameObject.SetActive(skillBaseData.IsUseCoolTime == true);
        }
        //------------------------------------------------------------------------------------
        public void RefreshSkillCoolTime()
        {
            if (_skillRemainCoolTimeFilled == null || _skillRemainCoolTimeText == null)
                return;

            SkillBaseData skillBaseData = _skillManageInfo.SkillBaseData;

            if (skillBaseData.IsUseCoolTime == false)
                return;

            if (_skillManageInfo.IsReady == true)
            {
                _skillRemainCoolTimeFilled.gameObject.SetActive(false);
                _skillRemainCoolTimeText.gameObject.SetActive(false);

                return;
            }

            float ratio = _skillManageInfo.GetRemainCoolRatio();
            float remainCount = _skillManageInfo.GetRemainCoolTime();



            _skillRemainCoolTimeFilled.gameObject.SetActive(true);
            _skillRemainCoolTimeFilled.fillAmount = ratio;

            _skillRemainCoolTimeText.gameObject.SetActive(true);

            if (skillBaseData.CoolTimeType == V2Enum_ARR_CoolTimeType.AttackCount
                    || skillBaseData.CoolTimeType == V2Enum_ARR_CoolTimeType.KillingCount
                    || skillBaseData.CoolTimeType == V2Enum_ARR_CoolTimeType.HitCount)
            {
                _skillRemainCoolTimeText.SetText("{0}", remainCount);
            }
            else if (skillBaseData.CoolTimeType == V2Enum_ARR_CoolTimeType.HPPercentOver
                || skillBaseData.CoolTimeType == V2Enum_ARR_CoolTimeType.HPPercentBelow)
            {
                _skillRemainCoolTimeText.SetText(skillBaseData.CoolTimeType.ToString());
            }
            else
            {
                _skillRemainCoolTimeText.SetText("{0 : 0.0}", remainCount);
            }
        }
        //------------------------------------------------------------------------------------
    }
}