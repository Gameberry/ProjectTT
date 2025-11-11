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
    public class UILobbySynergyBreakthroughtElement : MonoBehaviour
    {
        public Transform _guidebreak3BG;

        [SerializeField]
        private Image _synergyEffectIcon;

        [SerializeField]
        private Image _grageBG;

        [SerializeField]
        private TMP_Text _grage;

        [SerializeField]
        private Image _synergyEffectNewdot;

        [SerializeField]
        private Transform _synergyLock;

        [SerializeField]
        private Transform _synergyLine;

        [SerializeField]
        private Transform _synergyGet;

        [SerializeField]
        private Button _synergyEffectClickBtn;

        private SynergyBreakthroughData _currentSynergyEffectData;
        bool isNew = false;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (_synergyEffectClickBtn != null)
                _synergyEffectClickBtn.onClick.AddListener(OnClick);
        }
        //------------------------------------------------------------------------------------
        public void SetSynergyEffectData(SynergyBreakthroughData synergyEffectData)
        {
            _currentSynergyEffectData = synergyEffectData;

            if (_synergyEffectIcon != null)
            {
                _synergyEffectIcon.sprite = Managers.SynergyManager.Instance.GetSynergyBreakthroughSprite(synergyEffectData);
            }

            bool geted = Managers.SynergyManager.Instance.IsGetedBreakthrough(synergyEffectData);

            //if (_synergyLock != null)
            //    _synergyLock.gameObject.SetActive(geted == false);

            if (_synergyLock != null) // ±×³É ²ô±â
                _synergyLock.gameObject.SetActive(false);

            if (_synergyLine != null)
                _synergyLine.gameObject.SetActive(geted);

            isNew = Managers.SynergyManager.Instance.IsNewSynergyBreakthroughIcon(synergyEffectData);

            SetGradeColor(synergyEffectData.Grade);

            if (_synergyEffectNewdot != null)
                _synergyEffectNewdot.gameObject.SetActive(isNew);
        }
        //------------------------------------------------------------------------------------
        public void SetGradeColor(V2Enum_Grade v2Enum_Grade)
        {
            V2Enum_Grade myV2Enum_Grade = v2Enum_Grade;


            if (_grageBG != null)
                _grageBG.color = StaticResource.Instance.GetV2GradeColor(myV2Enum_Grade);

            if (_grage != null)
            {
                _grage.gameObject.SetActive(true);
                _grage.text = myV2Enum_Grade.ToString();
                _grage.color = StaticResource.Instance.GetV2GradeTextColor(myV2Enum_Grade);
            }
        }
        //------------------------------------------------------------------------------------
        public void PlayGetEffect()
        {
            if (_synergyGet != null)
                _synergyGet.gameObject.SetActive(true);

            RefreshSizeFilter().Forget();
        }
        //------------------------------------------------------------------------------------
        private async UniTask RefreshSizeFilter()
        {
            await UniTask.Delay(750);

            if (_synergyGet != null)
                _synergyGet.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        private void OnClick()
        {
            //if (_synergyEffectReddot != null)
            //    _synergyEffectReddot.gameObject.SetActive(false);

            if (_synergyEffectNewdot != null)
                _synergyEffectNewdot.gameObject.SetActive(false);

            if (_currentSynergyEffectData == null)
                return;

            if (isNew == true)
            {
                Managers.SynergyManager.Instance.RemoveNewIconBreakthroughSynergy(_currentSynergyEffectData);
            }

            if (_guidebreak3BG != null)
                _guidebreak3BG.gameObject.SetActive(false);


            Contents.GlobalContent.ShowGoodsDescPopup(V2Enum_Goods.SynergyBreak, _currentSynergyEffectData.Index, 0);
        }
        //------------------------------------------------------------------------------------
    }
}