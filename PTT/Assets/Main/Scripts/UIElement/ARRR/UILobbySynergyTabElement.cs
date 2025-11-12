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
    public class UILobbySynergyTabElement : MonoBehaviour
    {
        [SerializeField]
        private Image _synergyIcon;

        [SerializeField]
        private Image _synergyTabBG;

        [SerializeField]
        private Transform _synergyLock;


        [SerializeField]
        private RectTransform _synergyTabRect;

        [SerializeField]
        private float _synergyTab_IdleHeight = 120.0f;

        [SerializeField]
        private float _synergyTab_ClickHeight = 160.0f;

        [SerializeField]
        private Button _synergyClick;

        [SerializeField]
        private Image _synergyNewDot;


        [SerializeField]
        private UIRedDotElement _uiRedDotElement;

        [SerializeField]
        private Transform _myTabSprite;

        [SerializeField]
        private Transform _otherTabSprite;


        private Enum_SynergyType _synergyType = Enum_SynergyType.Max;

        private System.Action<Enum_SynergyType> _callBack;


        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (_synergyClick != null)
                _synergyClick.onClick.AddListener(OnClick);
        }
        //------------------------------------------------------------------------------------
        public void SetSynergyListData(Enum_SynergyType Enum_SynergyType, System.Action<Enum_SynergyType> action)
        {
            GambleCardSprite gambleCardSprite = StaticResource.Instance.GetGambleCardSpriteData(Enum_SynergyType);
            if (gambleCardSprite != null)
            {
                if (_synergyIcon != null)
                    _synergyIcon.sprite = gambleCardSprite.SynergyIcon;
            }

            _synergyType = Enum_SynergyType;

            //if (_uiRedDotElement != null)
            //{
            //    _uiRedDotElement.SetRedDotType(Managers.SynergyManager.Instance.ConvertRedDotEnum(_synergyType));
            //    _uiRedDotElement.AddRecvRedDotType(Managers.SynergyManager.Instance.ConvertRedDotEnum(_synergyType));

            //    _uiRedDotElement.Init();
            //}

            _callBack = action;

            SetClickState(false);
        }
        //------------------------------------------------------------------------------------
        public void SetClickState(bool onclick)
        {
            //if (_synergyTabBG != null)
            //    _synergyTabBG.color = onclick == true ? Color.yellow : Color.white;

            //if (_synergyTabRect != null)
            //{
            //    Vector2 size = _synergyTabRect.sizeDelta;
            //    size.y = onclick == true ? _synergyTab_ClickHeight : _synergyTab_IdleHeight;
            //    _synergyTabRect.sizeDelta = size;
            //}

            if (_myTabSprite != null)
                _myTabSprite.gameObject.SetActive(onclick);

            if (_otherTabSprite != null)
                _otherTabSprite.gameObject.SetActive(!onclick);

            if (_synergyType == Enum_SynergyType.Yellow)
            {
                if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.UnlockGoldSynergy) == false)
                {
                    if (_synergyLock != null)
                        _synergyLock.gameObject.SetActive(true);

                    if (_myTabSprite != null)
                        _myTabSprite.gameObject.SetActive(false);

                    if (_otherTabSprite != null)
                        _otherTabSprite.gameObject.SetActive(true);
                }
                else 
                {
                    if (_synergyLock != null)
                        _synergyLock.gameObject.SetActive(false);
                }
            }
            else if (_synergyType == Enum_SynergyType.White)
            {
                if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.UnlockThunderSynergy) == false)
                {
                    if (_synergyLock != null)
                        _synergyLock.gameObject.SetActive(true);

                    if (_myTabSprite != null)
                        _myTabSprite.gameObject.SetActive(false);

                    if (_otherTabSprite != null)
                        _otherTabSprite.gameObject.SetActive(true);
                }
                else
                {
                    if (_synergyLock != null)
                        _synergyLock.gameObject.SetActive(false);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick()
        {
            _callBack?.Invoke(_synergyType);
        }
        //------------------------------------------------------------------------------------
        public void EnableNewDot(bool enable)
        {
            if (_synergyNewDot != null)
                _synergyNewDot.gameObject.SetActive(enable);
        }
        //------------------------------------------------------------------------------------
        public void SetSynergyChangeElement(Transform parent)
        {
            UIGuideInteractor uIGuideInteractor = _synergyClick.GetComponent<UIGuideInteractor>();
            if (uIGuideInteractor == null)
            { 
                uIGuideInteractor = _synergyClick.gameObject.AddComponent<UIGuideInteractor>();
                uIGuideInteractor.MyGuideType = V2Enum_EventType.SynergyChange;
                uIGuideInteractor.MyStepID = 2;
                uIGuideInteractor.FocusAngle = 0;
                uIGuideInteractor.FocusParent = null;
                uIGuideInteractor.GuideInteratorActionType = Managers.GuideInteratorActionType.Next;
                uIGuideInteractor.IsAutoSetting = false;
                uIGuideInteractor.ConnectInteractor();
            }
        }
        //------------------------------------------------------------------------------------
    }
}