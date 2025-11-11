using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UISummonConfirmElement : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _confirmText;

        [SerializeField]
        private TMP_Text _gaugeCount;

        [SerializeField]
        private Image _gaugeBar;

        [SerializeField]
        private Transform _rewardTrans_NotFull;

        [SerializeField]
        private Transform _rewardTrans_Full;

        [SerializeField]
        private Button _playConfirm;

        [SerializeField]
        private Button _viewPercent;

        private V2Enum_SummonType _v2Enum_SummonType = V2Enum_SummonType.Max;

        //------------------------------------------------------------------------------------
        public void Init(V2Enum_SummonType v2Enum_SummonType)
        {
            _v2Enum_SummonType = v2Enum_SummonType;

            if (_viewPercent != null)
                _viewPercent.onClick.AddListener(OnClick_ViewPercent);

            if (_playConfirm != null)
                _playConfirm.onClick.AddListener(OnClick_PlayConfirm);

            if (_confirmText != null)
            {
                string localkey = string.Format("summon/confirm/{0}", v2Enum_SummonType.Enum32ToInt());

                SummonConfirmCountData summonConfirmCountData = Managers.SummonManager.Instance.GetSummonConfirmCountData(_v2Enum_SummonType);
                if (summonConfirmCountData != null)
                    _confirmText.SetText(string.Format(Managers.LocalStringManager.Instance.GetLocalString(localkey), summonConfirmCountData.RequiredCount));
            }
        }
        //------------------------------------------------------------------------------------
        public void RefreshConfirm()
        {
            if (_v2Enum_SummonType == V2Enum_SummonType.Max)
                return;

            bool canConfirm = Managers.SummonManager.Instance.CanConfirm(_v2Enum_SummonType);

            SummonConfirmCountData summonConfirmCountData = Managers.SummonManager.Instance.GetSummonConfirmCountData(_v2Enum_SummonType);

            int curr = Managers.SummonManager.Instance.GetCanCumfirmCount(_v2Enum_SummonType);
            int total = summonConfirmCountData.RequiredCount;

            if (_gaugeCount != null)
                _gaugeCount.SetText("{0}/{1}", curr,
                    total);

            if (_gaugeBar != null)
            {
                float ratio = 0;
                ratio = (float)curr / (float)total;

                _gaugeBar.fillAmount = ratio;
            }

            if (_rewardTrans_NotFull != null)
                _rewardTrans_NotFull.gameObject.SetActive(canConfirm == false);

            if (_rewardTrans_Full != null)
                _rewardTrans_Full.gameObject.SetActive(canConfirm == true);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_PlayConfirm()
        {
            if (Managers.SummonManager.Instance.PlayConfirm(_v2Enum_SummonType) == true)
                RefreshConfirm();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ViewPercent()
        {
            SummonGroupData summonGroupData = Managers.SummonManager.Instance.GetSummonConfirmDrawGroupData(_v2Enum_SummonType);
            Managers.SummonManager.Instance.ShowPercendView(summonGroupData);
        }
        //------------------------------------------------------------------------------------
    }
}