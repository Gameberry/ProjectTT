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
    public class UILobbySynergyRuneElement : InfiniteScrollItem
    {
        [SerializeField]
        private Image _synergyEffectIcon;

        [SerializeField]
        private List<UnityEngine.UI.Extensions.Gradient> _grageBG = new List<UnityEngine.UI.Extensions.Gradient>();

        [SerializeField]
        private TMP_Text _grage;

        [SerializeField]
        private Image _rarityImage;

        [SerializeField]
        private Image _rarityBGImage;

        [SerializeField]
        private TMP_Text _count;

        [SerializeField]
        private Image _synergyEffectNewdot;

        [SerializeField]
        private Button _synergyEffectClickBtn;

        private SynergyRuneData _currentSynergyEffectData;
        bool isNew = false;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (_synergyEffectClickBtn != null)
                _synergyEffectClickBtn.onClick.AddListener(OnClick);
        }
        //------------------------------------------------------------------------------------
        public override void UpdateData(InfiniteScrollData scrollData)
        {
            SynergyRuneData playerV3AllyInfo = scrollData as SynergyRuneData;

            if (playerV3AllyInfo != null)
                SetSynergyEffectData(playerV3AllyInfo);
        }
        //------------------------------------------------------------------------------------
        public void SetSynergyEffectData(SynergyRuneData synergyEffectData)
        {
            _currentSynergyEffectData = synergyEffectData;

            if (_synergyEffectIcon != null)
            {
                _synergyEffectIcon.sprite = Managers.SynergyRuneManager.Instance.GetDescendIcon(synergyEffectData);
            }

            if (_count != null)
                _count.SetText("{0}", Managers.SynergyRuneManager.Instance.GetDisplayEquipRuneCount(synergyEffectData));

            isNew = Managers.SynergyRuneManager.Instance.IsNewSynergyIcon(synergyEffectData);

            SetGradeColor(synergyEffectData.Grade);

            if (_synergyEffectNewdot != null)
                _synergyEffectNewdot.gameObject.SetActive(isNew);
        }
        //------------------------------------------------------------------------------------
        public void SetGradeColor(V2Enum_Grade v2Enum_Grade)
        {
            V2Enum_Grade myV2Enum_Grade = v2Enum_Grade;

            for (int i = 0; i < _grageBG.Count; ++i)
            {
                _grageBG[i].SetGrade(v2Enum_Grade);
            }

            if (_grage != null)
            {
                //_grage.gameObject.SetActive(true);
                _grage.SetGrade(v2Enum_Grade);
            }

            if (_rarityImage != null)
                _rarityImage.SetGradeTextImage(v2Enum_Grade);

            if (_rarityBGImage != null)
                _rarityBGImage.SetGradeBGImage(v2Enum_Grade);
        }
        //------------------------------------------------------------------------------------
        private void OnClick()
        {
            if (_synergyEffectNewdot != null)
                _synergyEffectNewdot.gameObject.SetActive(false);

            if (_currentSynergyEffectData == null)
                return;

            if (isNew == true)
            {
                Managers.SynergyRuneManager.Instance.RemoveNewIconSynergy(_currentSynergyEffectData);
            }

            OnSelect();


            //Contents.GlobalContent.ShowGoodsDescPopup(V2Enum_Goods.SynergyRune, _currentSynergyEffectData.Index, 0);
        }
        //------------------------------------------------------------------------------------
    }
}