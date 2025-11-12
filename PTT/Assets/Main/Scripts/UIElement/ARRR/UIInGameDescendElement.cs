using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace GameBerry.UI
{
    public class UIInGameDescendElement : MonoBehaviour
    {
        [SerializeField]
        private Image _synergyEffectIcon;

        [SerializeField]
        private TMP_Text _percent;

        [SerializeField]
        private Button _synergyEffectClickBtn;


        [SerializeField]
        private Transform _skillCoolTimeGroup;

        [SerializeField]
        private Image _skillRemainCoolTimeFilled;

        [SerializeField]
        private TMP_Text _skillRemainCoolTimeText;

        private SkillManageInfo _skillManageInfo;

        [SerializeField]
        private TMP_Text _levelUpCost;

        [SerializeField]
        private Color _synergyCanLevelUp;

        [SerializeField]
        private Color _synergyCannotLevelUp;

        private System.Action<DescendData> _callBack;

        private DescendData _currentDescendData;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (_synergyEffectClickBtn != null)
                _synergyEffectClickBtn.onClick.AddListener(OnClick);
        }
        //------------------------------------------------------------------------------------
        public void Init(System.Action<DescendData> action)
        {
            _callBack = action;
        }
        //------------------------------------------------------------------------------------
        private void OnClick()
        {
            if (Managers.DescendManager.Instance.DoInGameLevelUp(_currentDescendData) == true)
            {
                _callBack?.Invoke(_currentDescendData);
                Refresh();
            }
        }
        //------------------------------------------------------------------------------------
        public void SetDescendData(DescendData synergyEffectData)
        {
            _currentDescendData = synergyEffectData;

            if (_synergyEffectIcon != null)
            {
                _synergyEffectIcon.sprite = Managers.DescendManager.Instance.GetDescendIcon(synergyEffectData);
                //_synergyEffectIcon.color = skillInfo == null ? Color.gray : Color.white;
            }

            if (_skillCoolTimeGroup != null)
                _skillCoolTimeGroup.gameObject.SetActive(false);

            Refresh();
        }
        //------------------------------------------------------------------------------------
        public void Refresh()
        {
            if (_currentDescendData == null)
                return;

            if (_percent != null)
                _percent.SetText(string.Format("{0}%", Managers.DescendManager.Instance.GetInGameDescendDamageRatio(_currentDescendData)));

            bool canLevelUp = Managers.DescendManager.Instance.CanLevelUp(_currentDescendData);


            if (_levelUpCost != null)
            { 
                _levelUpCost.SetText(string.Format("{0}", Managers.DescendManager.Instance.GetInGameDescendEnhanceCost(_currentDescendData)));
                _levelUpCost.color = canLevelUp == true ? _synergyCanLevelUp : _synergyCannotLevelUp;
            }

        }
        //------------------------------------------------------------------------------------
        public void SetSkillManageInfo(SkillManageInfo skillManageInfo)
        {
            if (skillManageInfo == null)
            {
                if (_skillCoolTimeGroup != null)
                    _skillCoolTimeGroup.gameObject.SetActive(false);

                return;
            }

            if (_skillCoolTimeGroup != null)
                _skillCoolTimeGroup.gameObject.SetActive(true);

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
            if (_skillManageInfo == null)
                return;

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

            if (SkillManageInfo.IsCountCoolTime(skillBaseData.CoolTimeType))
            {
                _skillRemainCoolTimeText.SetText("{0}", remainCount);
            }
            else if (skillBaseData.CoolTimeType == Enum_CoolTimeType.HPPercentOver
                || skillBaseData.CoolTimeType == Enum_CoolTimeType.HPPercentBelow)
            {
                _skillRemainCoolTimeText.SetText(skillBaseData.CoolTimeType.ToString());
            }
            else
            {
                _skillRemainCoolTimeText.SetText("{0:0.0}", remainCount);
            }
        }
        //------------------------------------------------------------------------------------
    }
}