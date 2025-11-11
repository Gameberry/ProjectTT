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
    [System.Serializable]
    public class UILobbySynergyRunDot
    {
        public V2Enum_Grade Grade;
        public Transform Dot;
    }

    public class UILobbySynergyEffectElement : MonoBehaviour
    {
        [SerializeField]
        private Image _synergyEffectIcon;

        [SerializeField]
        private Image _synergySelect;

        [SerializeField]
        private Transform _synergyEquipMark;

        [SerializeField]
        private Transform _synergyUnEquipMark;

        [SerializeField]
        private Button _synergyEffectClickBtn;

        [SerializeField]
        private Image _synergyKindIcon;

        [SerializeField]
        private Transform _synergyLock;

        [SerializeField]
        private Transform _synergyUnLockEffect;

        [SerializeField]
        private TMP_Text _synergyNeedStack;

        [SerializeField]
        private TMP_Text _synergyEffectLevel;

        [SerializeField]
        private Transform _synergyEffectArrow;

        [SerializeField]
        private List<Image> _synergyEffectReddotArrow = new List<Image>();

        [SerializeField]
        private Image _synergyEffectNewdot;

        [SerializeField]
        private Image _synergyBar;

        [SerializeField]
        private Color _synergyLimitUp;

        [SerializeField]
        private Color _synergyCanLevelUp;

        [SerializeField]
        private Color _synergyCannotLevelUp;




        [Header("-------Enhance-------")]
        [SerializeField]
        private Image _synergyEffect_EnhanceCountIcon;

        [SerializeField]
        private TMP_Text _synergyEffect_EnhanceCountText;


        [SerializeField]
        private Transform _synergyEffect_Enhance2Group;

        [SerializeField]
        private Image _synergyEffect_Enhance2CountIcon;

        [SerializeField]
        private TMP_Text _synergyEffect_Enhance2CountText;


        [SerializeField]
        private Color _buttonTextEnableColor;

        [SerializeField]
        private Material _buttonTextEnableMaterial;

        [SerializeField]
        private Color _buttonTextDisaEnableColor;

        [SerializeField]
        private Material _buttonTextDisableMaterial;



        [SerializeField]
        private UILobbyCharLvUp _uILobbyCharLvUp;

        [Header("-------Rune-------")]
        [SerializeField]
        private List<UILobbySynergyRunDot> _runDot = new List<UILobbySynergyRunDot>();

        private System.Action<SynergyEffectData> _callBack;

        private SynergyEffectData _currentSynergyEffectData;
        private bool _locked = false;

        bool isNew = false;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (_synergyEffectClickBtn != null)
                _synergyEffectClickBtn.onClick.AddListener(OnClick);
        }
        //------------------------------------------------------------------------------------
        public void Init(System.Action<SynergyEffectData> action)
        {
            _callBack = action;
        }
        //------------------------------------------------------------------------------------
        public void SetSynergyEffectData(SynergyEffectData synergyEffectData)
        {
            SynergyEffectData beforeEffect = _currentSynergyEffectData;

            _currentSynergyEffectData = synergyEffectData;

            SkillInfo skillInfo = Managers.SynergyManager.Instance.GetSynergyEffectSkillInfo(synergyEffectData);

            if (_synergyEffectIcon != null)
            {
                _synergyEffectIcon.sprite = Managers.SynergyManager.Instance.GetSynergySprite(synergyEffectData);
                //_synergyEffectIcon.color = skillInfo == null ? Color.gray : Color.white;
            }

            bool isLock = Managers.SynergyManager.Instance.IsLockSynergy(synergyEffectData);

            if (_synergyLock != null)
                _synergyLock.gameObject.SetActive(isLock);



            if (beforeEffect == _currentSynergyEffectData)
            {
                if (isLock == false && _locked == true)
                {
                    if (_synergyUnLockEffect != null)
                        _synergyUnLockEffect.gameObject.SetActive(true);
                }
            }
            else
            {
                if (_synergyUnLockEffect != null)
                    _synergyUnLockEffect.gameObject.SetActive(false);
            }

            _locked = isLock;

            if (_synergyUnEquipMark != null)
                _synergyUnEquipMark.gameObject.SetActive(Managers.SynergyManager.Instance.GetEquipSynergyEffect(synergyEffectData.SynergyType, synergyEffectData.SynergyTier) != synergyEffectData);

            if (_synergyEquipMark != null)
                _synergyEquipMark.gameObject.SetActive(Managers.SynergyManager.Instance.GetEquipSynergyEffect(synergyEffectData.SynergyType, synergyEffectData.SynergyTier) == synergyEffectData);

            GambleCardSprite gambleCardSprite = StaticResource.Instance.GetGambleCardSpriteData(synergyEffectData.SynergyType);

            if (_synergyKindIcon != null && gambleCardSprite != null)
                _synergyKindIcon.sprite = gambleCardSprite.SynergyIcon;

            if (_synergyNeedStack != null)
                _synergyNeedStack.SetText("{0}", synergyEffectData.SynergyOriginCount);

            if (_synergyEffect_Enhance2Group != null)
                _synergyEffect_Enhance2Group.gameObject.SetActive(false);

            if (_synergyEffectLevel != null)
            {
                _synergyEffectLevel.SetText("Lv.{0}", skillInfo == null ? Define.PlayerSynergyDefaultLevel :  skillInfo.Level);
            }

            if (skillInfo == null)
            {
                if (_synergyBar != null)
                    _synergyBar.gameObject.SetActive(false);

                if (_synergyEffectArrow != null)
                    _synergyEffectArrow.gameObject.SetActive(false);
            }
            else
            {


                
                {
                    if (Managers.SynergyManager.Instance.IsMaxLevelSynergy(synergyEffectData) == true)
                    {
                        if (_synergyEffect_EnhanceCountText != null)
                        {
                            _synergyEffect_EnhanceCountText.color = Color.white;
                            _synergyEffect_EnhanceCountText.SetText("MAX");
                        }

                        if (_synergyEffect_EnhanceCountIcon != null)
                            _synergyEffect_EnhanceCountIcon.gameObject.SetActive(false);

                        if (_synergyEffectArrow != null)
                            _synergyEffectArrow.gameObject.SetActive(false);
                    }
                    else
                    {
                        int costIndex = Managers.SynergyManager.Instance.GetSynergyEnhanceCostGoodsIndex1(synergyEffectData);

                        int currentCount = (int)Managers.GoodsManager.Instance.GetGoodsAmount(costIndex);

                        int needCount = Managers.SynergyManager.Instance.GetSynergyEnhance_NeedCount1(synergyEffectData);

                        bool readyEnhance = currentCount >= needCount;

                        Sprite pointsprite = Managers.GoodsManager.Instance.GetGoodsSprite(costIndex);



                        if (_synergyEffect_EnhanceCountText != null)
                        {
                            _synergyEffect_EnhanceCountText.color = readyEnhance == true ? _buttonTextEnableColor : _buttonTextDisaEnableColor;
                            _synergyEffect_EnhanceCountText.fontMaterial = readyEnhance == true ? _buttonTextEnableMaterial : _buttonTextDisableMaterial;
                            _synergyEffect_EnhanceCountText.SetText("{0}", needCount);
                        }

                        if (_synergyEffect_EnhanceCountIcon != null)
                        {
                            _synergyEffect_EnhanceCountIcon.sprite = pointsprite;
                            _synergyEffect_EnhanceCountIcon.gameObject.SetActive(true);
                        }




                        costIndex = Managers.SynergyManager.Instance.GetSynergyEnhanceCostGoodsIndex2(synergyEffectData);

                        currentCount = (int)Managers.GoodsManager.Instance.GetGoodsAmount(costIndex);

                        needCount = Managers.SynergyManager.Instance.GetSynergyEnhance_NeedCount2(synergyEffectData);

                        readyEnhance = currentCount >= needCount;

                        pointsprite = Managers.GoodsManager.Instance.GetGoodsSprite(costIndex);


                        if (costIndex != -1)
                        {
                            if (_synergyEffect_Enhance2Group != null)
                                _synergyEffect_Enhance2Group.gameObject.SetActive(true);

                            if (_synergyEffect_Enhance2CountText != null)
                            {
                                _synergyEffect_Enhance2CountText.color = readyEnhance == true ? _buttonTextEnableColor : _buttonTextDisaEnableColor;
                                _synergyEffect_Enhance2CountText.fontMaterial = readyEnhance == true ? _buttonTextEnableMaterial : _buttonTextDisableMaterial;
                                _synergyEffect_Enhance2CountText.SetText("{0}", needCount);
                            }

                            if (_synergyEffect_Enhance2CountIcon != null)
                            {
                                _synergyEffect_Enhance2CountIcon.sprite = pointsprite;
                                _synergyEffect_Enhance2CountIcon.gameObject.SetActive(true);
                            }
                        }


                        if (_synergyEffectArrow != null)
                            _synergyEffectArrow.gameObject.SetActive(Managers.SynergyManager.Instance.ReadySynergyEnhance(synergyEffectData));

                        for (int i = 0; i < _synergyEffectReddotArrow.Count; ++i)
                        {
                            if (_synergyEffectReddotArrow[i] != null)
                                _synergyEffectReddotArrow[i].color = _synergyCanLevelUp;
                        }
                    }
                }
            }

            isNew = Managers.SynergyManager.Instance.IsNewSynergyIcon(synergyEffectData);

            if (_synergyEffectNewdot != null)
                _synergyEffectNewdot.gameObject.SetActive(isNew);

            if (_locked == true)
            {
                if (_synergyEffectArrow != null)
                    _synergyEffectArrow.gameObject.SetActive(false);

            }

            for (int i = 0; i < synergyEffectData.SynergyRuneList.Count; ++i)
            {
                SynergyBreakthroughData synergyRuneData = synergyEffectData.SynergyRuneList[i];

                UILobbySynergyRunDot uILobbySynergyRunDot = _runDot.Find(x => x.Grade == synergyRuneData.Grade);
                if (uILobbySynergyRunDot != null)
                {
                    bool isGeted = Managers.SynergyManager.Instance.IsGetedBreakthrough(synergyRuneData);
                    uILobbySynergyRunDot.Dot.gameObject.SetActive(isGeted);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void EnableSelectElement(bool enable)
        {
            if (_synergySelect != null)
                _synergySelect.gameObject.SetActive(enable);
        }
        //------------------------------------------------------------------------------------
        public void EnableLockImage(bool enable)
        {
            if (_synergyLock != null)
                _synergyLock.gameObject.SetActive(enable);
        }
        //------------------------------------------------------------------------------------
        private void OnClick()
        {
            //if (_synergyEffectReddot != null)
            //    _synergyEffectReddot.gameObject.SetActive(false);

            if (_synergyEffectNewdot != null)
                _synergyEffectNewdot.gameObject.SetActive(false);

            if (isNew == true)
            { 
                Managers.SynergyManager.Instance.RemoveNewIconSynergy(_currentSynergyEffectData);
                Managers.SynergyManager.Instance.RefreshNewSynergyIcon();
            }

            //if (_locked == true)
            //    Managers.SynergyManager.Instance.ShowNoticeSynergyUnLock(_currentSynergyEffectData);

            _callBack?.Invoke(_currentSynergyEffectData);
        }
        //------------------------------------------------------------------------------------
        private float _hideEffectTime = 0.0f;
        //------------------------------------------------------------------------------------
        public void PlayLevelUpEffect()
        {
            if (_uILobbyCharLvUp != null)
            {
                _uILobbyCharLvUp.gameObject.SetActive(true);
                _uILobbyCharLvUp.PlayEffect();
                if (_hideEffectTime > Time.time)
                {
                    _hideEffectTime = Time.time + 1.0f;
                }
                else
                {
                    _hideEffectTime = Time.time + 1.0f;
                    AutoHideEffect().Forget();
                }
            }
        }
        //------------------------------------------------------------------------------------
        private async UniTask AutoHideEffect()
        {
            while (_hideEffectTime > Time.time)
            {
                await UniTask.NextFrame();
            }

            if (_uILobbyCharLvUp != null)
            {
                _uILobbyCharLvUp.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
    }
}
