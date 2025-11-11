using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gpm.Ui;

namespace GameBerry.UI
{
    public class LobbyGearDetailPopupDialog : IDialog
    {
        [SerializeField]
        private Transform _guideBG4;

        [SerializeField]
        private Transform _guideBG5;

        [SerializeField]
        private Transform _guideBG6;

        [SerializeField]
        private Button _guideBG6Btn;

        [SerializeField]
        private UIGearElement _synergyRuneElement;

        [SerializeField]
        private Button _synergyRuneEquip;


        [SerializeField]
        private TMP_Text m_goodsName;

        [SerializeField]
        private TMP_Text m_goodsDesc;

        [SerializeField]
        private List<UIStatDetailElement> _uIStatDetailElement = new List<UIStatDetailElement>();


        [Header("-------GearKind-------")]
        [SerializeField]
        private Image _gearKindImage;

        [SerializeField]
        private TMP_Text _gearKindText;


        [Header("-------GearSkill-------")]
        [SerializeField]
        private Transform _addSkillEffectDescRoot;

        [SerializeField]
        private UILobbyAddLevelEffectElement _uILobbyAddSkillEffectElement;

        private List<UILobbyAddLevelEffectElement> _uILobbyAddSkillEffectElements = new List<UILobbyAddLevelEffectElement>();



        [SerializeField]
        private TMP_Text _slotLevelText;

        [SerializeField]
        private Image _gearSynergyType;

        [SerializeField]
        private TMP_Text _gearSynergyTypeString;


        [SerializeField]
        private TMP_Text _synergyRuneEquipText;

        [SerializeField]
        private Button _synergyRuneUnEquip;


        [SerializeField]
        private Transform _synergyEffect_Enhance_Max;


        [SerializeField]
        private Button _synergyEffect_Enhance;


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
        private UIItemIconAndAmount _uIItemIconAndAmount;


        [SerializeField]
        private UILobbyCharLvUp _uILobbyCharLvUpEffect;

        [SerializeField]
        private Transform _uILobbyCharLvUpEffect_Root;

        [SerializeField]
        private int _uILobbyCharLvUpEffect_MaxPoolCount = 5;

        private List<UILobbyCharLvUp> _uILobbyCharLvUpEffectPool = new List<UILobbyCharLvUp>();
        [SerializeField]
        private int _uILobbyCharLvUpEffectPoolPlayIndex = 0;

        [SerializeField]
        private float _directionRefreshDelay = 0.5f;


        [SerializeField]
        private Button _gearSlotLevelReset;


        private GearData currentSynergyRuneData;
        private bool isEquipState = false;
        private V2Enum_GearType currentV2Enum_ARR_SynergyType;

        private bool onEquip = false;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (_uIItemIconAndAmount != null)
            {
                _uIItemIconAndAmount.AddCallBack();
            }

            if (_synergyRuneEquip != null)
                _synergyRuneEquip.onClick.AddListener(OnClick_SynergyRuneEquip);

            if (_synergyRuneUnEquip != null)
                _synergyRuneUnEquip.onClick.AddListener(OnClick_SynergyRuneUnEquip);

            if (_synergyEffect_Enhance != null)
                _synergyEffect_Enhance.onClick.AddListener(OnClick_SynergyRuneEnhance);

            if (_gearSlotLevelReset != null)
                _gearSlotLevelReset.onClick.AddListener(OnClick_ResetSlot);

            if (_guideBG6Btn != null)
                _guideBG6Btn.onClick.AddListener(() =>
                {
                    if (_guideBG6 != null)
                        _guideBG6.gameObject.SetActive(false);
                });
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            onEquip = false;

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.GearEquipTutorial)
            {
                if (_guideBG6 != null)
                    _guideBG6.gameObject.SetActive(true);
            }

            for (int i = 0; i < _uILobbyCharLvUpEffectPool.Count; ++i)
            {
                _uILobbyCharLvUpEffectPool[i].gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (_guideBG5 != null)
                _guideBG5.gameObject.SetActive(false);

            if (_guideBG6 != null)
                _guideBG6.gameObject.SetActive(false);

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.GearTutorial)
            {
                if (onEquip == false)
                {
                    if (_guideBG4 != null)
                        _guideBG4.gameObject.SetActive(true);

                    Managers.GuideInteractorManager.Instance.SetGuideStep(4);
                }
                else
                {
                    if (Managers.GuideInteractorManager.Instance.GetCurrentStep() == 5)
                    {
                        Managers.GuideInteractorManager.Instance.EndGuideQuest();
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void SynergyRuneElement(GearData synergyRuneData)
        {
            currentSynergyRuneData = synergyRuneData;

            if (synergyRuneData == null)
                return;

            if (m_goodsName != null)
            {
                string localkey = Managers.GearManager.Instance.GetSynergyLocalKey(synergyRuneData.Index);
                Managers.LocalStringManager.Instance.SetLocalizeText(m_goodsName, localkey);
            }

            //if (m_goodsDesc != null)
            //{
            //    string descKey = string.Format("{0}/desc/{1}", V2Enum_Goods.SynergyRune.ToString().ToCamelCase(), synergyRuneData.Index);

            //    Managers.LocalStringManager.Instance.SetLocalizeText(m_goodsDesc, descKey);
            //}

            GearOptionData gearOptionData = Managers.GearManager.Instance.GetGearOptionData(synergyRuneData.GearType, synergyRuneData.SynergyType);

            int skilleffectcount = 0;

            foreach (var pair in gearOptionData.GearSkills)
            {
                UILobbyAddLevelEffectElement uILobbyAddLevelEffectElement = null;

                if (_uILobbyAddSkillEffectElements.Count > skilleffectcount)
                    uILobbyAddLevelEffectElement = _uILobbyAddSkillEffectElements[skilleffectcount];
                else
                {
                    GameObject clone = Instantiate(_uILobbyAddSkillEffectElement.gameObject, _addSkillEffectDescRoot);

                    uILobbyAddLevelEffectElement = clone.GetComponent<UILobbyAddLevelEffectElement>();

                    _uILobbyAddSkillEffectElements.Add(uILobbyAddLevelEffectElement);
                }


                uILobbyAddLevelEffectElement.SetGearDesc(string.Format("gear/desc/{0}", pair.Value), pair.Key);
                uILobbyAddLevelEffectElement.SetEnableDimmedImage(synergyRuneData.Grade < pair.Key);
                uILobbyAddLevelEffectElement.gameObject.SetActive(true);

                skilleffectcount++;
            }

            for (int i = skilleffectcount; i < _uILobbyAddSkillEffectElements.Count; ++i)
            {
                _uILobbyAddSkillEffectElements[i].gameObject.SetActive(false);
            }

            if (_synergyRuneElement != null)
                _synergyRuneElement.SetSynergyEffectData(synergyRuneData);



            if (_gearSynergyType != null)
            {
                GambleCardSprite gambleCardSprite = StaticResource.Instance.GetGambleCardSpriteData(synergyRuneData.SynergyType);
                _gearSynergyType.sprite = gambleCardSprite.SynergyIcon;
            }

            if (_gearSynergyTypeString != null)
            {
                Managers.LocalStringManager.Instance.SetLocalizeText(_gearSynergyTypeString, string.Format("synergytitle/{0}", synergyRuneData.SynergyType.Enum32ToInt()));
            }


            int costIndex = Managers.GearManager.Instance.GetSynergyEnhanceCostGoodsIndex1(synergyRuneData.GearType);

            if (_uIItemIconAndAmount != null)
                _uIItemIconAndAmount.SetSetting_NotConnectRefreshEvent(costIndex.IntToEnum32<V2Enum_Point>());


            GearResourceData gearResourceData = StaticResource.Instance.GetGearResourceData(synergyRuneData.GearType);
            if (gearResourceData != null)
            {
                if (_gearKindImage != null)
                    _gearKindImage.sprite = gearResourceData.GearSprite;

                if (_gearKindText != null)
                    Managers.LocalStringManager.Instance.SetLocalizeText(_gearKindText, gearResourceData.gearLocalKey);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SynergyRuneEquip()
        {
            Managers.GearManager.Instance.EquipSkill(currentSynergyRuneData);

            onEquip = true;

            ElementExit();
        }
        //------------------------------------------------------------------------------------
        private List<double> _directionValues = new List<double>();
        public System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        private void OnClick_SynergyRuneEnhance()
        {
            SerializeString.Clear();
            _directionValues.Clear();

            for (int i = 0; i < currentSynergyRuneData.OwnEffect.Count; ++i)
            {
                OperatorOverrideStat operatorOverrideStat = currentSynergyRuneData.OwnEffect[i];

                _directionValues.Add(Managers.GearManager.Instance.GetStatValue(currentSynergyRuneData, operatorOverrideStat));
            }

            if (Managers.GearManager.Instance.EnhanceSynergy(currentV2Enum_ARR_SynergyType))
            { 
                RefreshEnhanceBtn();
                //SetBaseStatView();

                for (int i = 0; i < currentSynergyRuneData.OwnEffect.Count; ++i)
                {
                    OperatorOverrideStat operatorOverrideStat = currentSynergyRuneData.OwnEffect[i];

                    double addvalue = 0;
                    if (_directionValues.Count > i)
                        addvalue = Managers.GearManager.Instance.GetStatValue(currentSynergyRuneData, operatorOverrideStat) - _directionValues[i];

                    if(i > 0)
                        SerializeString.Append("\n");

                    SerializeString.Append(string.Format("+{0:0.##}", addvalue));
                }

                PlayLevelUpEffect(SerializeString.ToString());

                PlaySynergyChangeTutorial(Managers.GearManager.Instance.GetSlotLevel(currentV2Enum_ARR_SynergyType)).Forget();

                if (_slotLevelText != null)
                {
                    int level = Managers.GearManager.Instance.GetSlotLevel(currentV2Enum_ARR_SynergyType);
                    if (level <= 0)
                        _slotLevelText.gameObject.SetActive(false);
                    else
                    {
                        _slotLevelText.gameObject.SetActive(true);
                        _slotLevelText.SetText("+{0}", level);
                    }
                }

                int costIndex = Managers.GearManager.Instance.GetSynergyEnhanceCostGoodsIndex1(currentV2Enum_ARR_SynergyType);

                if (_uIItemIconAndAmount != null)
                    _uIItemIconAndAmount.SetSetting_NotConnectRefreshEvent(costIndex.IntToEnum32<V2Enum_Point>());
            }
        }
        //------------------------------------------------------------------------------------
        private void PlayLevelUpEffect(string text)
        {
            if (_uILobbyCharLvUpEffectPoolPlayIndex >= _uILobbyCharLvUpEffect_MaxPoolCount)
                _uILobbyCharLvUpEffectPoolPlayIndex = 0;

            UILobbyCharLvUp uILobbyCharLvUp = null;

            if (_uILobbyCharLvUpEffectPool.Count > _uILobbyCharLvUpEffectPoolPlayIndex)
            {
                uILobbyCharLvUp = _uILobbyCharLvUpEffectPool[_uILobbyCharLvUpEffectPoolPlayIndex];
            }
            else
            {
                GameObject clone = Instantiate(_uILobbyCharLvUpEffect.gameObject, _uILobbyCharLvUpEffect_Root);
                clone.transform.ResetLocal();
                uILobbyCharLvUp = clone.GetComponent<UILobbyCharLvUp>();
                _uILobbyCharLvUpEffectPool.Add(uILobbyCharLvUp);
            }

            uILobbyCharLvUp.gameObject.SetActive(true);
            uILobbyCharLvUp.transform.localPosition = Vector3.zero;
            uILobbyCharLvUp.transform.SetAsLastSibling();
            uILobbyCharLvUp.SetText(text);
            uILobbyCharLvUp.PlayEffect();

            _uILobbyCharLvUpEffectPoolPlayIndex++;
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlaySynergyChangeTutorial(int level)
        {
            await UniTask.WaitForSeconds(_directionRefreshDelay);

            for (int i = 0; i < currentSynergyRuneData.OwnEffect.Count; ++i)
            {
                if (_uIStatDetailElement.Count <= i)
                    break;

                OperatorOverrideStat operatorOverrideStat = currentSynergyRuneData.OwnEffect[i];
                UIStatDetailElement uIStatDetailElement = _uIStatDetailElement[i];
                double statvalue = Managers.GearManager.Instance.GetStatValue(level, operatorOverrideStat);

                double bonusValue = Managers.GearManager.Instance.GetJobBuffValue(currentSynergyRuneData, operatorOverrideStat);

                uIStatDetailElement.SetStatData(Managers.ARRRStatManager.Instance.GetCharacterBaseStatData(operatorOverrideStat.BaseStat));
                uIStatDetailElement.RefrashValue(statvalue);

                if (bonusValue > 0)
                {
                    uIStatDetailElement.EnableBonusValue(true);
                    uIStatDetailElement.ShowBunusValue(bonusValue);
                }
                else
                    uIStatDetailElement.EnableBonusValue(false);

                uIStatDetailElement.gameObject.SetActive(true);
            }

            for (int i = currentSynergyRuneData.OwnEffect.Count; i < _uIStatDetailElement.Count; ++i)
            {
                _uIStatDetailElement[i].gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ResetSlot()
        {
            Contents.GlobalContent.ShowPopup_OkCancel(
                Managers.LocalStringManager.Instance.GetLocalString("common/ui/shortagegoodstitle"),
                Managers.LocalStringManager.Instance.GetLocalString("ui/reset/gear/desc"),
                () =>
                {
                    if (Managers.GearManager.Instance.ResetSlot(currentV2Enum_ARR_SynergyType))
                    {
                        RefreshEnhanceBtn();
                        SetBaseStatView();

                        if (_slotLevelText != null)
                        {
                            int level = Managers.GearManager.Instance.GetSlotLevel(currentV2Enum_ARR_SynergyType);
                            if (level <= 0)
                                _slotLevelText.gameObject.SetActive(false);
                            else
                            {
                                _slotLevelText.gameObject.SetActive(true);
                                _slotLevelText.SetText("+{0}", level);
                            }
                        }

                        int costIndex = Managers.GearManager.Instance.GetSynergyEnhanceCostGoodsIndex1(currentV2Enum_ARR_SynergyType);

                        if (_uIItemIconAndAmount != null)
                            _uIItemIconAndAmount.SetSetting_NotConnectRefreshEvent(costIndex.IntToEnum32<V2Enum_Point>());

                    }
                },
                null);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SynergyRuneUnEquip()
        {
            Managers.GearManager.Instance.UnEquipSkillSlot(
                currentV2Enum_ARR_SynergyType);

            ElementExit();
        }
        //------------------------------------------------------------------------------------
        public void SetSlotState(V2Enum_GearType v2Enum_ARR_SynergyType)
        {
            currentV2Enum_ARR_SynergyType = v2Enum_ARR_SynergyType;

        }
        //------------------------------------------------------------------------------------
        public void SetEquipMode(bool mode)
        {
            isEquipState = mode;

            if (_synergyRuneEquip != null)
                _synergyRuneEquip.gameObject.SetActive(mode);

            if (_gearSlotLevelReset != null)
                _gearSlotLevelReset.gameObject.SetActive(!mode);

            if (_synergyRuneUnEquip != null)
                _synergyRuneUnEquip.gameObject.SetActive(!mode);

            if (currentSynergyRuneData == null)
                return;


            if (isEquipState == true)
            {
                bool Equipfull = Managers.GearManager.Instance.GetCanEquipSkillSlotIdx(currentSynergyRuneData.GearType) == -1;

                if (_synergyRuneEquipText != null)
                {
                    if (Equipfull == true)
                        Managers.LocalStringManager.Instance.SetLocalizeText(_synergyRuneEquipText, "common/ui/slotfull");
                    else
                        Managers.LocalStringManager.Instance.SetLocalizeText(_synergyRuneEquipText, "common/ui/equip");
                }

                if (_synergyRuneEquip != null)
                {
                    _synergyRuneEquip.interactable = Equipfull == false;
                }

            }

            SetBaseStatView();

            if (!mode)
            {
                if (_slotLevelText != null)
                {
                    int level = Managers.GearManager.Instance.GetSlotLevel(currentV2Enum_ARR_SynergyType);
                    if (level <= 0)
                        _slotLevelText.gameObject.SetActive(false);
                    else
                    {
                        _slotLevelText.gameObject.SetActive(true);
                        _slotLevelText.SetText("+{0}", level);
                    }
                }


                RefreshEnhanceBtn();
            }
            else
            {
                if (_synergyEffect_Enhance != null)
                    _synergyEffect_Enhance.gameObject.SetActive(false);

                if (_synergyEffect_Enhance_Max != null)
                    _synergyEffect_Enhance_Max.gameObject.SetActive(false);

                if (_slotLevelText != null)
                    _slotLevelText.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        
        private void SetBaseStatView()
        {

            for (int i = 0; i < currentSynergyRuneData.OwnEffect.Count; ++i)
            {
                if (_uIStatDetailElement.Count <= i)
                    break;

                OperatorOverrideStat operatorOverrideStat = currentSynergyRuneData.OwnEffect[i];
                UIStatDetailElement uIStatDetailElement = _uIStatDetailElement[i];
                double statvalue = isEquipState == false ?
                    Managers.GearManager.Instance.GetStatValue(currentSynergyRuneData, operatorOverrideStat)
                    : Managers.GearManager.Instance.GetStatValue(0, operatorOverrideStat);

                double bonusValue = Managers.GearManager.Instance.GetJobBuffValue(currentSynergyRuneData, operatorOverrideStat);

                uIStatDetailElement.SetStatData(Managers.ARRRStatManager.Instance.GetCharacterBaseStatData(operatorOverrideStat.BaseStat));
                uIStatDetailElement.RefrashValue(statvalue);
                
                if (bonusValue > 0)
                { 
                    uIStatDetailElement.EnableBonusValue(true);
                    uIStatDetailElement.ShowBunusValue(bonusValue);
                }
                else
                    uIStatDetailElement.EnableBonusValue(false);

                uIStatDetailElement.gameObject.SetActive(true);
            }

            for (int i = currentSynergyRuneData.OwnEffect.Count; i < _uIStatDetailElement.Count; ++i)
            {
                _uIStatDetailElement[i].gameObject.SetActive(false);
            }

        }
        //------------------------------------------------------------------------------------
        private void RefreshEnhanceBtn()
        {
            bool isMax = Managers.GearManager.Instance.IsMaxLevelSynergy(currentV2Enum_ARR_SynergyType);

            if (_synergyEffect_Enhance != null)
                _synergyEffect_Enhance.gameObject.SetActive(isMax == false);

            if (_synergyEffect_Enhance_Max != null)
                _synergyEffect_Enhance_Max.gameObject.SetActive(isMax == true);

            if (isMax == true)
            {
                if (_synergyEffect_EnhanceCountText != null)
                {
                    _synergyEffect_EnhanceCountText.color = Color.white;
                    _synergyEffect_EnhanceCountText.SetText("-");
                }

                if (_synergyEffect_EnhanceCountIcon != null)
                    _synergyEffect_EnhanceCountIcon.gameObject.SetActive(false);
            }
            else
            {

                int costIndex = Managers.GearManager.Instance.GetSynergyEnhanceCostGoodsIndex1(currentV2Enum_ARR_SynergyType);

                int currentCount = (int)Managers.GoodsManager.Instance.GetGoodsAmount(costIndex);

                int needCount = Managers.GearManager.Instance.GetSynergyEnhance_NeedCount1(currentV2Enum_ARR_SynergyType);

                bool readyEnhance = currentCount >= needCount;

                Sprite pointsprite = Managers.GoodsManager.Instance.GetGoodsSprite(costIndex);


                //if (_synergyEffect_Enhance != null)
                //    _synergyEffect_Enhance.interactable = Managers.GearManager.Instance.ReadySynergyEnhance(synergyEffectData);

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


                costIndex = Managers.GearManager.Instance.GetSynergyEnhanceCostGoodsIndex2(currentV2Enum_ARR_SynergyType);

                currentCount = (int)Managers.GoodsManager.Instance.GetGoodsAmount(costIndex);

                needCount = Managers.GearManager.Instance.GetSynergyEnhance_NeedCount2(currentV2Enum_ARR_SynergyType);

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
            }
        }
        //------------------------------------------------------------------------------------
    }
}