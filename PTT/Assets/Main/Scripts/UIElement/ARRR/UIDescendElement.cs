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
    public class UIDescendElement : MonoBehaviour
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
        private Transform _synergyLock;

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

        [SerializeField]
        private TMP_Text _synergyEnhanceGoodsCount;


        [SerializeField]
        private Image _grageBG;

        [SerializeField]
        private TMP_Text _grage;

        [SerializeField]
        private Image _grageSpriteText;

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


        private System.Action<DescendData> _callBack;

        private DescendData _currentDescendData;

        bool isNew = false;


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
        public void SetDescendData(DescendData synergyEffectData)
        {
            _currentDescendData = synergyEffectData;

            SkillInfo skillInfo = Managers.DescendManager.Instance.GetSynergyEffectSkillInfo(synergyEffectData);

            if (_synergyEffectIcon != null)
            {
                _synergyEffectIcon.sprite = Managers.DescendManager.Instance.GetDescendIcon(synergyEffectData);
                //_synergyEffectIcon.color = skillInfo == null ? Color.gray : Color.white;
            }

            if (_synergyLock != null)
                _synergyLock.gameObject.SetActive(skillInfo == null);

            EnableEquipElement(Managers.DescendManager.Instance.IsEquipSkill(synergyEffectData));

            if (_synergyEffect_Enhance2Group != null)
                _synergyEffect_Enhance2Group.gameObject.SetActive(false);


            if (skillInfo == null)
            {
                if (_synergyBar != null)
                    _synergyBar.gameObject.SetActive(false);

                if (_synergyEnhanceGoodsCount != null)
                    _synergyEnhanceGoodsCount.SetText(string.Empty);

                if (_synergyEffectArrow != null)
                    _synergyEffectArrow.gameObject.SetActive(false);

                if (_synergyEffectLevel != null)
                {
                    _synergyEffectLevel.SetText("Lv.{0}", Define.PlayerDescendDefaultLevel);
                }

                DescendOpenCostData synergyConditionData = synergyEffectData.DescendOpenCostData;

                if (synergyConditionData != null)
                {
                    int costIndex = synergyConditionData.OpenCostGoodsIndex;

                    int currentCount = (int)Managers.GoodsManager.Instance.GetGoodsAmount(costIndex);

                    int needCount = (int)synergyConditionData.OpenCostGoodsValue;

                    bool readyEnhance = currentCount >= needCount;

                    Sprite pointsprite = Managers.GoodsManager.Instance.GetGoodsSprite(costIndex);

                    if (_synergyEffect_EnhanceCountIcon != null)
                        _synergyEffect_EnhanceCountIcon.sprite = pointsprite;

                    if (_synergyEffect_EnhanceCountText != null)
                    {
                        _synergyEffect_EnhanceCountText.color = readyEnhance == true ? _buttonTextEnableColor : _buttonTextDisaEnableColor;
                        _synergyEffect_EnhanceCountText.fontMaterial = readyEnhance == true ? _buttonTextEnableMaterial : _buttonTextDisableMaterial;
                        _synergyEffect_EnhanceCountText.SetText("{0}", needCount);
                    }
                }
            }
            else
            {
                if (_synergyEffectLevel != null)
                {
                    _synergyEffectLevel.SetText("Lv.{0}", skillInfo.Level);
                }

                if (Managers.DescendManager.Instance.NeedLimitBreak(synergyEffectData) == true)
                {
                    DescendBreakthroughCostData synergyLevelUpLimitData = Managers.DescendManager.Instance.GetSynergyLevelUpLimitData(skillInfo.LimitCompleteLevel);

                    int costIndex = synergyLevelUpLimitData.LimitBreakCostGoodsIndex;

                    int currentCount = (int)Managers.GoodsManager.Instance.GetGoodsAmount(costIndex);

                    int needCount = synergyLevelUpLimitData.LimitBreakCostGoodsValue;

                    bool readyEnhance = Managers.DescendManager.Instance.ReadyCharacterLimitLevelUpCost(synergyLevelUpLimitData);

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


                    if (_synergyEffectArrow != null)
                        _synergyEffectArrow.gameObject.SetActive(Managers.DescendManager.Instance.ReadyCharacterLimitLevelUpCost(synergyLevelUpLimitData));

                    for (int i = 0; i < _synergyEffectReddotArrow.Count; ++i)
                    {
                        if (_synergyEffectReddotArrow[i] != null)
                            _synergyEffectReddotArrow[i].color = _synergyLimitUp;
                    }
                }
                else
                {
                    if (Managers.DescendManager.Instance.IsMaxLevelSynergy(synergyEffectData) == true)
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
                        int costIndex = Managers.DescendManager.Instance.GetSynergyEnhanceCostGoodsIndex1(synergyEffectData);

                        int currentCount = (int)Managers.GoodsManager.Instance.GetGoodsAmount(costIndex);

                        int needCount = Managers.DescendManager.Instance.GetSynergyEnhance_NeedCount1(synergyEffectData);

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




                        costIndex = Managers.DescendManager.Instance.GetSynergyEnhanceCostGoodsIndex2(synergyEffectData);

                        currentCount = (int)Managers.GoodsManager.Instance.GetGoodsAmount(costIndex);

                        needCount = Managers.DescendManager.Instance.GetSynergyEnhance_NeedCount2(synergyEffectData);

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
                            _synergyEffectArrow.gameObject.SetActive(Managers.DescendManager.Instance.ReadySynergyEnhance(synergyEffectData));

                        for (int i = 0; i < _synergyEffectReddotArrow.Count; ++i)
                        {
                            if (_synergyEffectReddotArrow[i] != null)
                                _synergyEffectReddotArrow[i].color = _synergyCanLevelUp;
                        }
                    }
                }
            }

            isNew = Managers.DescendManager.Instance.IsNewSynergyIcon(synergyEffectData);

            SetGradeColor(synergyEffectData.Grade);

            if (_synergyEffectNewdot != null)
                _synergyEffectNewdot.gameObject.SetActive(isNew);
        }
        //------------------------------------------------------------------------------------
        public void SetGradeColor(V2Enum_Grade v2Enum_Grade)
        {
            V2Enum_Grade myV2Enum_Grade = v2Enum_Grade;


            if (_grageBG != null)
                _grageBG.SetGradeColor(v2Enum_Grade);

            if (_grage != null)
            {
                _grage.gameObject.SetActive(true);
                _grage.SetGrade(myV2Enum_Grade);
            }

            if (_grageSpriteText != null)
                _grageSpriteText.SetGradeTextImage(myV2Enum_Grade);
        }
        //------------------------------------------------------------------------------------
        public void EnableSelectElement(bool enable)
        {
            if (_synergySelect != null)
                _synergySelect.gameObject.SetActive(enable);
        }
        //------------------------------------------------------------------------------------
        public void EnableEquipElement(bool enable)
        {
            if (_synergyEquipMark != null)
                _synergyEquipMark.gameObject.SetActive(enable);

            if (_synergyUnEquipMark != null)
                _synergyUnEquipMark.gameObject.SetActive(!enable);
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
                Managers.DescendManager.Instance.RemoveNewIconSynergy(_currentDescendData);
                Managers.DescendManager.Instance.RefreshNewSynergyIcon();
            }


            _callBack?.Invoke(_currentDescendData);
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