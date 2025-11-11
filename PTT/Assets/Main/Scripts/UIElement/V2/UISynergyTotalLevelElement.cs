using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;

namespace GameBerry.UI
{
    public class UISynergyTotalLevelElement : InfiniteScrollItem
    {
        [SerializeField]
        private Button _showBtn;

        [SerializeField]
        private TMP_Text _needExp;

        [SerializeField]
        private TMP_Text _effectDesc;

        [SerializeField]
        private Transform _applyEffect;

        [SerializeField]
        private Image _gauge;

        [SerializeField]
        private Image _typeColorImage;

        [SerializeField]
        private Transform _dimmed;

        public bool _isDescend = false;

        //private SynergyTotalLevelEffectData _currentSynergyTotalLevelEffectData;

        private V2Enum_ARRR_TotalLevelType v2Enum_ARRR_TotalLevelType = V2Enum_ARRR_TotalLevelType.BasicStat;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (_showBtn != null)
                _showBtn.onClick.AddListener(OnClick);
        }
        //------------------------------------------------------------------------------------
        //public override void UpdateData(InfiniteScrollData scrollData)
        //{
        //    SynergyTotalLevelEffectData playerV3AllyInfo = scrollData as SynergyTotalLevelEffectData;

        //    if (playerV3AllyInfo != null)
        //        SetRewardElement(playerV3AllyInfo);
        //}
        ////------------------------------------------------------------------------------------
        //public void SetRewardElement(SynergyTotalLevelEffectData synergyTotalLevelEffectData)
        //{
        //    int myExp = synergyTotalLevelEffectData.SynergyTotalLevelExp;

        //    if (_needExp != null)
        //        _needExp.SetText("{0}", myExp);

        //    v2Enum_ARRR_TotalLevelType = synergyTotalLevelEffectData.TotalLevelEffectType;

        //    _currentSynergyTotalLevelEffectData = synergyTotalLevelEffectData;

        //    if (_effectDesc != null)
        //    {
        //        _effectDesc.SetText(synergyTotalLevelEffectData.Desc);
        //    }

        //    if (_applyEffect == null || _gauge == null)
        //        return;

        //    int exp = _isDescend == true ? DescendContainer.SynergyContentExp : SynergyContainer.SynergyContentExp;

        //    if (myExp <= exp)
        //    {
        //        _applyEffect.gameObject.SetActive(true);
        //        _gauge.fillAmount = 1.0f;

        //        if (_dimmed != null)
        //            _dimmed.gameObject.SetActive(false);
        //    }
        //    else if (synergyTotalLevelEffectData.PrevExp >= exp)
        //    {
        //        _applyEffect.gameObject.SetActive(false);
        //        _gauge.fillAmount = 0.0f;

        //        if (_dimmed != null)
        //            _dimmed.gameObject.SetActive(true);
        //    }
        //    else
        //    {
        //        _applyEffect.gameObject.SetActive(false);

        //        float curr = exp - synergyTotalLevelEffectData.PrevExp;
        //        float total = myExp - synergyTotalLevelEffectData.PrevExp;

        //        _gauge.fillAmount = curr / total;

        //        if (_dimmed != null)
        //            _dimmed.gameObject.SetActive(true);
        //    }

        //    if (_typeColorImage != null)
        //    {
        //        TotalLevelEffectColorData totalLevelEffectColorData = StaticResource.Instance.GetTotalLevelEffectColorData(synergyTotalLevelEffectData.TotalLevelEffectType);
        //        if (totalLevelEffectColorData != null)
        //            _typeColorImage.color = totalLevelEffectColorData.EffectColor;
        //    }


        //}
        ////------------------------------------------------------------------------------------
        private void OnClick()
        {
            if (v2Enum_ARRR_TotalLevelType == V2Enum_ARRR_TotalLevelType.CardGradeLimitBreak)
            {
                //if (_currentSynergyTotalLevelEffectData != null)
                //    Managers.SynergyManager.Instance.ShowTotalExpDetail(ContentDetailList.None, _currentSynergyTotalLevelEffectData);
            }
        }
        //------------------------------------------------------------------------------------
    }
}
