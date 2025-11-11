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
    public class LobbySynergyContentDialog : IDialog
    {
        [Header("------------Guide------------")]
        [SerializeField]
        private Transform _guideBG;

        [SerializeField]
        private Button _guide2CompleteBtn;

        [Header("------------GuideBreak------------")]
        [SerializeField]
        private Transform _guidebreak2BG;

        [SerializeField]
        private Transform _guidebreak3BG;


        [Header("------------GuideOpen------------")]
        // 잠긴 아이콘 눌러보기
        [SerializeField]
        private Transform _guideOpen2BG;

        [SerializeField]
        private Button _guideOpen2Btn;

        // 해금 시점 확인하기
        [SerializeField]
        private Transform _guideOpen3BG;

        [SerializeField]
        private Button _guideOpen3Btn;

        //불덩이 발사 다시 누르러 가기
        [SerializeField]
        private Transform _guideOpen4BG;

        [SerializeField]
        private Button _guideOpen4Btn;

        //불덩이 발사 8레벨 달성하기
        [SerializeField]
        private Transform _guideOpen5BG;

        //해금 확인하러 가기
        [SerializeField]
        private Transform _guideOpen6BG;

        [SerializeField]
        private Button _guideOpen6Btn;



        [Header("------------GuideOpen------------")]
        // 잠긴 아이콘 눌러보기
        [SerializeField]
        private Transform _guideUnLock2BG;

        [SerializeField]
        private Button _guideUnLock2Btn;

        [Header("-------SynergyTab-------")]
        [SerializeField]
        private RectTransform _synergyListRoot;

        [SerializeField]
        private HorizontalLayoutGroup _synergyHorizontalLayout;

        [SerializeField]
        private UILobbySynergyTabElement _uIGambleSynergyViewElement;

        private Dictionary<V2Enum_ARR_SynergyType, UILobbySynergyTabElement> _uIGambleSynergyViewElement_dic = new Dictionary<V2Enum_ARR_SynergyType, UILobbySynergyTabElement>();


        [SerializeField]
        private UIItemIconAndAmount _uIItemIconAndAmount;

        [SerializeField]
        private Button _resetSynergyBtn;

        [Header("-------Breakthrough-------")]
        [SerializeField]
        private List<UILobbySynergyBreakthroughtElement> _uISynergyRuneElements = new List<UILobbySynergyBreakthroughtElement>();

        [SerializeField]
        private Image _getBreakthroughPriceIcon;

        [SerializeField]
        private TMP_Text _getBreakthroughtTitle;

        [SerializeField]
        private TMP_Text _getBreakthroughPrice;

        [SerializeField]
        private Button _getBreakthroughBtn;

        [SerializeField]
        private Transform _breakthroughMax;

        [SerializeField]
        private TMP_Text _breakUnLockCount;

        [SerializeField]
        private Transform _breakthroughLock;

        [SerializeField]
        private TMP_Text _breakthroughUnLockNotice;

        [Header("-------SynergyGroup-------")]
        [SerializeField]
        private Transform _synergyTierGroupListRoot;

        [SerializeField]
        private UILobbySynergyTierGroupElement _uIGambleSynergyTierGroupElement;

        private Dictionary<ObscuredInt, UILobbySynergyTierGroupElement> _uIGambleSynergyTierGroupElement_dic = new Dictionary<ObscuredInt, UILobbySynergyTierGroupElement>();

        private List<UILobbySynergyTierGroupElement> _uILobbySynergyTierGroupElements = new List<UILobbySynergyTierGroupElement>();

        [SerializeField]
        private Transform _synergyLockGroup;

        [SerializeField]
        private TMP_Text _synergyUnLockGroupNotice;

        [Header("-------EffectDesc-------")]
        [SerializeField]
        private Transform lockSynergy;

        [SerializeField]
        private List<UILobbySynergyUnLockNoticeElement> _UILobbySynergyUnLockNoticeElementList = new List<UILobbySynergyUnLockNoticeElement>();

        [SerializeField]
        private UIARRRSkillDescGroup uIARRRSkillDescGroup;

        [SerializeField]
        private UIARRRSkillDescGroup uIARRRSkillDescGroup_NextLevel;


        [SerializeField]
        private List<RectTransform> customRefresh;

        [SerializeField]
        private List<ContentSizeFitter> customRefreshSizeFilter = new List<ContentSizeFitter>();

        [SerializeField]
        private Transform _synergyEffect_Enhance_Max;


        [SerializeField]
        private TMP_Text _synergyEffectLevel;

        [SerializeField]
        private Button _synergyEffect_Equip;

        [SerializeField]
        private TMP_Text _synergyEffect_EquipText;

        [SerializeField]
        private Button _synergyEffect_Enhance;

        [SerializeField]
        private TMP_Text _synergyEffect_EnhanceText;

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
        private Image _synergyKindIcon;

        [SerializeField]
        private TMP_Text _synergyNeedStack;

        [Header("-------ReinforceStat-------")]
        [SerializeField]
        private TMP_Text _reinforceStat_Attack;

        [SerializeField]
        private TMP_Text _reinforceStat_Defence;

        [SerializeField]
        private TMP_Text _reinforceStat_HP;


        private SynergyEffectData _currentSynergyEffectData;

        private V2Enum_ARR_SynergyType _currentSynergyType = V2Enum_ARR_SynergyType.Max;

        private int _tutorialIndex;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (_guide2CompleteBtn != null)
                _guide2CompleteBtn.onClick.AddListener(() =>
                {
                    if (_guideBG != null)
                        _guideBG.gameObject.SetActive(false);
                });

            if (_guideOpen2Btn != null)
                _guideOpen2Btn.onClick.AddListener(() =>
                {
                    if (_guideOpen2BG != null)
                        _guideOpen2BG.gameObject.SetActive(false);

                    if (_guideOpen2Btn != null)
                        _guideOpen2Btn.gameObject.SetActive(false);

                    SynergyEffectData synergyEffectData = Managers.SynergyManager.Instance.GetEquipSynergyEffect(V2Enum_ARR_SynergyType.Red, 3);
                    SetSynergyEffectDetail(synergyEffectData);


                    if (_guideOpen3BG != null)
                        _guideOpen3BG.gameObject.SetActive(true);


                    if (_guideOpen3Btn != null)
                        _guideOpen3Btn.gameObject.SetActive(true);

                    Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/synergyopen3"));
                    Managers.GuideInteractorManager.Instance.SetGuideStep(3);
                });

            if (_guideOpen3Btn != null)
                _guideOpen3Btn.onClick.AddListener(() =>
                {
                    if (_guideOpen3BG != null)
                        _guideOpen3BG.gameObject.SetActive(false);

                    if (_guideOpen3Btn != null)
                        _guideOpen3Btn.gameObject.SetActive(false);

                    if (_guideOpen4BG != null)
                        _guideOpen4BG.gameObject.SetActive(true);

                    if (_guideOpen4Btn != null)
                        _guideOpen4Btn.gameObject.SetActive(true);

                    Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/synergyopen4"));
                    Managers.GuideInteractorManager.Instance.SetGuideStep(4);
                });

            if (_guideOpen4Btn != null)
                _guideOpen4Btn.onClick.AddListener(() =>
                {
                    if (_guideOpen4BG != null)
                        _guideOpen4BG.gameObject.SetActive(false);

                    if (_guideOpen4Btn != null)
                        _guideOpen4Btn.gameObject.SetActive(false);

                    if (_guideOpen5BG != null)
                        _guideOpen5BG.gameObject.SetActive(true);

                    SynergyEffectData synergyEffectData = Managers.SynergyManager.Instance.GetEquipSynergyEffect(V2Enum_ARR_SynergyType.Red, 1);
                    SetSynergyEffectDetail(synergyEffectData);


                    Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/synergyopen5"));
                    Managers.GuideInteractorManager.Instance.SetGuideStep(5);
                });

            if (_guideOpen6Btn != null)
                _guideOpen6Btn.onClick.AddListener(() =>
                {
                    if (_guideOpen6BG != null)
                        _guideOpen6BG.gameObject.SetActive(false);

                    if (_guideOpen6Btn != null)
                        _guideOpen6Btn.gameObject.SetActive(false);

                    SynergyEffectData synergyEffectData = Managers.SynergyManager.Instance.GetEquipSynergyEffect(V2Enum_ARR_SynergyType.Red, 3);
                    SetSynergyEffectDetail(synergyEffectData);

                    Managers.GuideInteractorManager.Instance.EndGuideQuest();
                });

            if (_guideUnLock2Btn != null)
                _guideUnLock2Btn.onClick.AddListener(() =>
                {
                    if (_guideUnLock2BG != null)
                        _guideUnLock2BG.gameObject.SetActive(false);

                    if (_guideUnLock2Btn != null)
                        _guideUnLock2Btn.gameObject.SetActive(false);

                    Managers.GuideInteractorManager.Instance.EndGuideQuest();
                });

            foreach (var pair in Managers.SynergyManager.Instance.SynergySortView)
            {
                GameObject clone = Instantiate(_uIGambleSynergyViewElement.gameObject, _synergyListRoot);

                UILobbySynergyTabElement uIGambleSynergyListElement = clone.GetComponent<UILobbySynergyTabElement>();
                uIGambleSynergyListElement.SetSynergyListData(pair, OnClick_SynergyTab);

                _uIGambleSynergyViewElement_dic.Add(pair, uIGambleSynergyListElement);
            }


            if (_uIItemIconAndAmount != null)
            {
                _uIItemIconAndAmount.AddCallBack();
            }

            if (_resetSynergyBtn != null)
                _resetSynergyBtn.onClick.AddListener(OnClick_ResetSynergy);

            if (_synergyEffect_Equip != null)
                _synergyEffect_Equip.onClick.AddListener(OnClick_ChangeEquipSynergy);

            if (_synergyEffect_Enhance != null)
                _synergyEffect_Enhance.onClick.AddListener(OnClick_EnhanceSynergy);

            if (_getBreakthroughBtn != null)
                _getBreakthroughBtn.onClick.AddListener(OnClick_GetBreakthrough);

            Message.AddListener<GameBerry.Event.ChangeEquipSynergyMsg>(ChangeEquipSynergy);
            Message.AddListener<GameBerry.Event.ShowNewSynergyMsg>(ShowNewSynergy);
            Message.AddListener<GameBerry.Event.NoticeNewLobbySynergyElementMsg>(NoticeNewLobbySynergyElement);

            Message.AddListener<GameBerry.Event.PlaySynergyChangeTutorialMsg>(PlaySynergyChangeTutorial);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.ChangeEquipSynergyMsg>(ChangeEquipSynergy);
            Message.RemoveListener<GameBerry.Event.ShowNewSynergyMsg>(ShowNewSynergy);
            Message.RemoveListener<GameBerry.Event.NoticeNewLobbySynergyElementMsg>(NoticeNewLobbySynergyElement);

            Message.RemoveListener<GameBerry.Event.PlaySynergyChangeTutorialMsg>(PlaySynergyChangeTutorial);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            if (_currentSynergyType == V2Enum_ARR_SynergyType.Max)
            {
                OnClick_SynergyTab(V2Enum_ARR_SynergyType.Red);

                SynergyEffectData synergyEffectData = Managers.SynergyManager.Instance.GetEquipSynergyEffect(V2Enum_ARR_SynergyType.Red, 1);
                SetSynergyEffectDetail(synergyEffectData);
            }
            else
            { 
                SetSynergyPage(_currentSynergyType);
                SetSynergyEffectDetail(_currentSynergyEffectData);
            }

            if (Managers.GuideInteractorManager.Instance.PlaySynergyTutorial == true)
            {
                PlaySynergyChangeTutorial().Forget();
            }

            if (Managers.GuideInteractorManager.Instance.PlaySynergyOpenTutorial == true)
            {
                SynergyEffectData synergyEffectData = Managers.SynergyManager.Instance.GetEquipSynergyEffect(V2Enum_ARR_SynergyType.Red, 3);
                if (Managers.SynergyManager.Instance.IsLockSynergy(synergyEffectData) == false)
                {
                    Managers.GuideInteractorManager.Instance.EndGuideQuest();
                    return;
                }

                OnClick_SynergyTab(V2Enum_ARR_SynergyType.Red);

                synergyEffectData = Managers.SynergyManager.Instance.GetEquipSynergyEffect(V2Enum_ARR_SynergyType.Red, 1);
                SetSynergyEffectDetail(synergyEffectData);

                PlaySynergyOpenTutorial().Forget();
            }
            else
            {
                if (_guideOpen2BG != null)
                    _guideOpen2BG.gameObject.SetActive(false);

                if (_guideOpen2Btn != null)
                    _guideOpen2Btn.gameObject.SetActive(false);

                if (_guideOpen3BG != null)
                    _guideOpen3BG.gameObject.SetActive(false);

                if (_guideOpen3Btn != null)
                    _guideOpen3Btn.gameObject.SetActive(false);

                if (_guideOpen4BG != null)
                    _guideOpen4BG.gameObject.SetActive(false);

                if (_guideOpen4Btn != null)
                    _guideOpen4Btn.gameObject.SetActive(false);

                if (_guideOpen5BG != null)
                    _guideOpen5BG.gameObject.SetActive(false);

                if (_guideOpen6BG != null)
                    _guideOpen6BG.gameObject.SetActive(false);

                if (_guideOpen6Btn != null)
                    _guideOpen6Btn.gameObject.SetActive(false);
            }

            if (Managers.GuideInteractorManager.Instance.PlaySynergyUnLockTutorial == true)
            {
                OnClick_SynergyTab(V2Enum_ARR_SynergyType.Red);
                PlaySynergyUnLockTutorial().Forget();
            }

            if (Managers.GuideInteractorManager.Instance.PlaySynergyBreakTutorial == true)
            {
                OnClick_SynergyTab(V2Enum_ARR_SynergyType.Red);

                SynergyEffectData synergyEffectData = Managers.SynergyManager.Instance.GetEquipSynergyEffect(V2Enum_ARR_SynergyType.Red, 1);
                SetSynergyEffectDetail(synergyEffectData);

                PlaySynergyBreakTutorial().Forget();
            }
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlaySynergyChangeTutorial()
        {
            await UniTask.NextFrame();
            //if (_currentSynergyType == V2Enum_ARR_SynergyType.White)
            //    Managers.GuideInteractorManager.Instance.SetGuideStep(3);
            //else
            //    Managers.GuideInteractorManager.Instance.SetGuideStep(2);

            Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/synergyskill3"));
            Managers.GuideInteractorManager.Instance.SetGuideStep(2);
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlaySynergyOpenTutorial()
        {
            await UniTask.NextFrame();
            //if (_currentSynergyType == V2Enum_ARR_SynergyType.White)
            //    Managers.GuideInteractorManager.Instance.SetGuideStep(3);
            //else
            //    Managers.GuideInteractorManager.Instance.SetGuideStep(2);

            if (_guideOpen2BG != null)
                _guideOpen2BG.gameObject.SetActive(true);

            if (_guideOpen2Btn != null)
                _guideOpen2Btn.gameObject.SetActive(true);

            Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/synergyopen2"));
            Managers.GuideInteractorManager.Instance.SetGuideStep(2);
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlaySynergyUnLockTutorial()
        {
            await UniTask.NextFrame();
            if (_guideUnLock2BG != null)
                _guideUnLock2BG.gameObject.SetActive(true);

            if (_guideUnLock2Btn != null)
                _guideUnLock2Btn.gameObject.SetActive(true);

            Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/synergyunlock2"));
            Managers.GuideInteractorManager.Instance.SetGuideStep(2);
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlaySynergyBreakTutorial()
        {
            await UniTask.NextFrame();
            //if (_currentSynergyType == V2Enum_ARR_SynergyType.White)
            //    Managers.GuideInteractorManager.Instance.SetGuideStep(3);
            //else
            //    Managers.GuideInteractorManager.Instance.SetGuideStep(2);

            if (_guidebreak2BG != null)
                _guidebreak2BG.gameObject.SetActive(true);

            Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/skillbreak2"));
            Managers.GuideInteractorManager.Instance.SetGuideStep(2);
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.SynergyChange)
            {
                if (Managers.GuideInteractorManager.Instance.GetCurrentStep() == 3)
                {
                    if (_guideBG != null)
                        _guideBG.gameObject.SetActive(false);

                    Managers.GuideInteractorManager.Instance.EndGuideQuest();

                    Managers.GuideInteractorManager.Instance.SetLastGuidePlayTime(V2Enum_EventType.StageClear, 0);
                }
                else if (Managers.GuideInteractorManager.Instance.GetCurrentStep() == 2)
                    Managers.GuideInteractorManager.Instance.SetGuideStep(1);
            }

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.SynergyUnLock)
            {
                if (_guideUnLock2BG != null)
                    _guideUnLock2BG.gameObject.SetActive(false);

                if (_guideUnLock2Btn != null)
                    _guideUnLock2Btn.gameObject.SetActive(false);

                if (Managers.GuideInteractorManager.Instance.GetCurrentStep() == 2)
                    Managers.GuideInteractorManager.Instance.SetGuideStep(1);
                else
                    Managers.GuideInteractorManager.Instance.EndGuideQuest();
            }

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.SynergyBreak)
            {
                if (_guidebreak2BG != null)
                    _guidebreak2BG.gameObject.SetActive(false);

                if (_guidebreak3BG != null)
                    _guidebreak3BG.gameObject.SetActive(false);


                if (Managers.GuideInteractorManager.Instance.GetCurrentStep() == 3)
                {
                    Managers.GuideInteractorManager.Instance.EndGuideQuest();
                }
                else if (Managers.GuideInteractorManager.Instance.GetCurrentStep() == 2)
                    Managers.GuideInteractorManager.Instance.SetGuideStep(1);
            }

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.SynergyOpen)
            {
                if (_guideOpen2BG != null)
                    _guideOpen2BG.gameObject.SetActive(false);

                if (_guideOpen2Btn != null)
                    _guideOpen2Btn.gameObject.SetActive(false);


                if (_guideOpen3BG != null)
                    _guideOpen3BG.gameObject.SetActive(false);

                if (_guideOpen3Btn != null)
                    _guideOpen3Btn.gameObject.SetActive(false);


                if (_guideOpen4BG != null)
                    _guideOpen4BG.gameObject.SetActive(false);

                if (_guideOpen4Btn != null)
                    _guideOpen4Btn.gameObject.SetActive(false);


                if (_guideOpen5BG != null)
                    _guideOpen5BG.gameObject.SetActive(false);


                if (_guideOpen6BG != null)
                    _guideOpen6BG.gameObject.SetActive(false);

                if (_guideOpen6Btn != null)
                    _guideOpen6Btn.gameObject.SetActive(false);


                if (Managers.GuideInteractorManager.Instance.GetCurrentStep() < 6)
                {
                    Managers.GuideInteractorManager.Instance.SetGuideStep(1);
                }
                else
                    Managers.GuideInteractorManager.Instance.EndGuideQuest();
            }


            Managers.SynergyManager.Instance.RefreshNewSynergyIcon();
            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.SynergyInteraction)
            {
                Managers.GuideInteractorManager.Instance.EndGuideQuest();
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SynergyTab(V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType)
        {
            if (v2Enum_ARR_SynergyType == _currentSynergyType)
                return;

            if (Managers.GuideInteractorManager.Instance.PlaySynergyOpenTutorial == true
                || Managers.GuideInteractorManager.Instance.PlaySynergyBreakTutorial == true)
            {
                if (v2Enum_ARR_SynergyType != V2Enum_ARR_SynergyType.Red)
                    return;
            }

            
            Managers.SynergyManager.Instance.RefreshNewSynergyIcon();

            SetSynergyPage(v2Enum_ARR_SynergyType);

            SynergyEffectData synergyEffectData = Managers.SynergyManager.Instance.GetEquipSynergyEffect(v2Enum_ARR_SynergyType, 1);


            if (Managers.GuideInteractorManager.Instance.PlaySynergyTutorial == true && v2Enum_ARR_SynergyType == V2Enum_ARR_SynergyType.White)
            {
                SynergyEffectData tuto = Managers.SynergyManager.Instance.GetSynergyEffectData(_tutorialIndex);
                if (tuto != null)
                    synergyEffectData = tuto;
            }
            else if (Managers.GuideInteractorManager.Instance.PlaySynergyTutorial == true && v2Enum_ARR_SynergyType != V2Enum_ARR_SynergyType.White)
            {
                Managers.GuideInteractorManager.Instance.SetGuideStep(2);
            }

            SetSynergyEffectDetail(synergyEffectData);
        }
        //------------------------------------------------------------------------------------
        private void SetSynergyPage(V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType)
        {
            _currentSynergyType = v2Enum_ARR_SynergyType;

            if (v2Enum_ARR_SynergyType == V2Enum_ARR_SynergyType.Red)
            {
                if (_uIItemIconAndAmount != null)
                    _uIItemIconAndAmount.SetSetting_NotConnectRefreshEvent(V2Enum_Point.SynergyLimitFire);
            }
            else if (v2Enum_ARR_SynergyType == V2Enum_ARR_SynergyType.Yellow)
            {
                if (_uIItemIconAndAmount != null)
                    _uIItemIconAndAmount.SetSetting_NotConnectRefreshEvent(V2Enum_Point.SynergyLimitGold);
            }
            else if (v2Enum_ARR_SynergyType == V2Enum_ARR_SynergyType.Blue)
            {
                if (_uIItemIconAndAmount != null)
                    _uIItemIconAndAmount.SetSetting_NotConnectRefreshEvent(V2Enum_Point.SynergyLimitWater);
            }
            else if (v2Enum_ARR_SynergyType == V2Enum_ARR_SynergyType.White)
            {
                if (_uIItemIconAndAmount != null)
                    _uIItemIconAndAmount.SetSetting_NotConnectRefreshEvent(V2Enum_Point.SynergyLimitThunder);
            }


            foreach (var pair in _uIGambleSynergyViewElement_dic)
            {
                pair.Value.SetClickState(pair.Key == _currentSynergyType);
            }

            SetSynergyTier(v2Enum_ARR_SynergyType);
        }
        //------------------------------------------------------------------------------------
        private void SetSynergyTier(V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType)
        {
            SynergyData synergyData = Managers.SynergyManager.Instance.GetGambleSynergyData(v2Enum_ARR_SynergyType);

            if (synergyData == null || synergyData.TierDatas == null)
                return;

            int count = 0;

            if (_currentSynergyEffectData != null)
            {
                if (_uIGambleSynergyTierGroupElement_dic.ContainsKey(_currentSynergyEffectData.SynergyTier) == true)
                {
                    UILobbySynergyTierGroupElement uILobbySynergyTierGroupElement = _uIGambleSynergyTierGroupElement_dic[_currentSynergyEffectData.SynergyTier];
                    uILobbySynergyTierGroupElement.EnableSelectEelement(_currentSynergyEffectData, false);
                }
            }

            _uIGambleSynergyTierGroupElement_dic.Clear();

            foreach (var pair in synergyData.TierDatas)
            {
                UILobbySynergyTierGroupElement uILobbySynergyEffectElement = null;

                if (_uILobbySynergyTierGroupElements.Count > count)
                    uILobbySynergyEffectElement = _uILobbySynergyTierGroupElements[count];
                else
                {
                    GameObject clone = Instantiate(_uIGambleSynergyTierGroupElement.gameObject, _synergyTierGroupListRoot);

                    uILobbySynergyEffectElement = clone.GetComponent<UILobbySynergyTierGroupElement>();
                    uILobbySynergyEffectElement.Init(OnClick_SynergyEffect);

                    _uILobbySynergyTierGroupElements.Add(uILobbySynergyEffectElement);
                }

                uILobbySynergyEffectElement.SetSynergyTierGroup(v2Enum_ARR_SynergyType, pair.Key, pair.Value);
                uILobbySynergyEffectElement.gameObject.SetActive(true);

                _uIGambleSynergyTierGroupElement_dic.Add(pair.Key, uILobbySynergyEffectElement);

                count++;
            }

            for (int i = synergyData.TierDatas.Count; i < _uILobbySynergyTierGroupElements.Count; ++i)
            {
                _uILobbySynergyTierGroupElements[i].gameObject.SetActive(false);
            }


            
            if (_synergyLockGroup != null)
            {
                if (v2Enum_ARR_SynergyType == V2Enum_ARR_SynergyType.Yellow)
                {
                    if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.UnlockGoldSynergy) == false)
                    {
                        _synergyLockGroup.gameObject.SetActive(true);

                        if (_synergyUnLockGroupNotice != null)
                            _synergyUnLockGroupNotice.SetText(Managers.ContentOpenConditionManager.Instance.GetOpenContitionLocalString(V2Enum_ContentType.UnlockGoldSynergy));
                    }
                    else
                        _synergyLockGroup.gameObject.SetActive(false);
                }
                else if (v2Enum_ARR_SynergyType == V2Enum_ARR_SynergyType.White)
                {
                    if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.UnlockThunderSynergy) == false)
                    {
                        _synergyLockGroup.gameObject.SetActive(true);

                        if (_synergyUnLockGroupNotice != null)
                            _synergyUnLockGroupNotice.SetText(Managers.ContentOpenConditionManager.Instance.GetOpenContitionLocalString(V2Enum_ContentType.UnlockThunderSynergy));
                    }
                    else
                        _synergyLockGroup.gameObject.SetActive(false);
                }
                else
                    _synergyLockGroup.gameObject.SetActive(false);
            }

        }

        private void SetSynergyRune(SynergyEffectData synergyEffectData)
        {
            int count = 0;
            int unlockcount = 0;
            foreach (var pair in synergyEffectData.SynergyRuneList)
            {
                UILobbySynergyBreakthroughtElement uILobbySynergyEffectElement = null;

                if (_uISynergyRuneElements.Count > count)
                    uILobbySynergyEffectElement = _uISynergyRuneElements[count];
                else
                    break;

                uILobbySynergyEffectElement.SetSynergyEffectData(pair);
                uILobbySynergyEffectElement.gameObject.SetActive(true);

                if (Managers.SynergyManager.Instance.IsGetedBreakthrough(pair) == true)
                    unlockcount++;

                count++;
            }


            if (_breakUnLockCount != null)
                _breakUnLockCount.SetText("{0}/{1}", unlockcount, synergyEffectData.SynergyRuneList.Count);

            for (int i = synergyEffectData.SynergyRuneList.Count; i < _uISynergyRuneElements.Count; ++i)
            {
                _uISynergyRuneElements[i].gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        public void OnClick_SynergyEffect(SynergyEffectData synergyEffectData)
        {
            if (Managers.GuideInteractorManager.Instance.PlaySynergyOpenTutorial == true)
            {
                return;
            }

            SetSynergyEffectDetail(synergyEffectData);
        }
        //------------------------------------------------------------------------------------
        private void SetSynergyEffectDetail(SynergyEffectData synergyEffectData)
        {
            if (synergyEffectData == null || synergyEffectData.SynergySkillData == null)
                return;

            if (Managers.GuideInteractorManager.Instance.PlaySynergyBreakTutorial == true)
            {
                SynergyEffectData tutotarget = Managers.SynergyManager.Instance.GetEquipSynergyEffect(V2Enum_ARR_SynergyType.Red, 1);

                if (synergyEffectData != tutotarget)
                    return;
            }

            if (_currentSynergyEffectData != null)
            {
                if (_uIGambleSynergyTierGroupElement_dic.ContainsKey(_currentSynergyEffectData.SynergyTier) == true)
                {
                    UILobbySynergyTierGroupElement uILobbySynergyTierGroupElement = _uIGambleSynergyTierGroupElement_dic[_currentSynergyEffectData.SynergyTier];
                    uILobbySynergyTierGroupElement.EnableSelectEelement(_currentSynergyEffectData, false);
                }
            }

            _currentSynergyEffectData = synergyEffectData;

            if (_currentSynergyEffectData != null)
            {
                if (_uIGambleSynergyTierGroupElement_dic.ContainsKey(_currentSynergyEffectData.SynergyTier) == true)
                {
                    UILobbySynergyTierGroupElement uILobbySynergyTierGroupElement = _uIGambleSynergyTierGroupElement_dic[_currentSynergyEffectData.SynergyTier];
                    uILobbySynergyTierGroupElement.EnableSelectEelement(_currentSynergyEffectData, true);
                }
            }

            if (_synergyEffect_Enhance2Group != null)
                _synergyEffect_Enhance2Group.gameObject.SetActive(false);


            SkillInfo skillInfo = Managers.SynergyManager.Instance.GetSynergyEffectSkillInfo(synergyEffectData);

            GambleCardSprite gambleCardSprite = StaticResource.Instance.GetGambleCardSpriteData(synergyEffectData.SynergyType);

            if (_synergyKindIcon != null && gambleCardSprite != null)
                _synergyKindIcon.sprite = gambleCardSprite.SynergyIcon;

            if (_synergyNeedStack != null)
                _synergyNeedStack.SetText("{0}", synergyEffectData.SynergyOriginCount);

            if (_breakthroughUnLockNotice != null)
            {
                _breakthroughUnLockNotice.SetText(Managers.LocalStringManager.Instance.GetLocalString("synergy/breakthroughlock"));
            }

            bool lockBreak = Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.SynergyBreakthrough) == false;

            //StageInfo stageInfo = Managers.MapManager.Instance.GetStageInfo(Define.SynergyBreakTutorialStage);

            //if (stageInfo != null)
            //{
            //    lockBreak = stageInfo.RecvClearReward < Define.SynergyBreakTutorialWave;
            //}

            if (_breakthroughLock != null)
                _breakthroughLock.gameObject.SetActive(lockBreak);

            if (skillInfo == null)
            {
                if (uIARRRSkillDescGroup != null)
                    uIARRRSkillDescGroup.SetSkillData(synergyEffectData.SynergySkillData, Define.PlayerSynergyDefaultLevel);

                if (uIARRRSkillDescGroup_NextLevel != null)
                    uIARRRSkillDescGroup_NextLevel.SetSkillData(synergyEffectData.SynergySkillData, Define.PlayerSynergyDefaultLevel + 1);

                if (_synergyEffectLevel != null)
                    _synergyEffectLevel.SetText("Lv.{0}", Define.PlayerSynergyDefaultLevel);

                if (_synergyEffect_Equip != null)
                    _synergyEffect_Equip.gameObject.SetActive(false);

                //if (_synergyEffect_Equip != null)
                //    _synergyEffect_Equip.interactable = false;

                if (_synergyEffect_Enhance != null)
                    _synergyEffect_Enhance.gameObject.SetActive(false);

                if (_synergyEffect_Enhance_Max != null)
                    _synergyEffect_Enhance_Max.gameObject.SetActive(false);

                if (_reinforceStat_Attack != null)
                    _reinforceStat_Attack.SetText("-");
                if (_reinforceStat_Defence != null)
                    _reinforceStat_Defence.SetText("-");
                if (_reinforceStat_HP != null)
                    _reinforceStat_HP.SetText("-");


            }
            else
            {
                if (uIARRRSkillDescGroup != null)
                    uIARRRSkillDescGroup.SetSkillData(synergyEffectData.SynergySkillData, skillInfo.Level);

                if (uIARRRSkillDescGroup_NextLevel != null)
                {
                    if (Managers.SynergyManager.Instance.IsMaxLevelSynergy(synergyEffectData) == false)
                    {
                        uIARRRSkillDescGroup_NextLevel.gameObject.SetActive(true);
                        uIARRRSkillDescGroup_NextLevel.SetSkillData(synergyEffectData.SynergySkillData, skillInfo.Level + 1);
                    }
                    else
                        uIARRRSkillDescGroup_NextLevel.gameObject.SetActive(false);
                }

                if (_synergyEffectLevel != null)
                { 
                    _synergyEffectLevel.SetText("Lv.{0}", skillInfo.Level);
                }


                bool IsEquip = Managers.SynergyManager.Instance.GetEquipSynergyEffect(synergyEffectData.SynergyType, synergyEffectData.SynergyTier) == synergyEffectData;

                if (_synergyEffect_Equip != null)
                {
                    _synergyEffect_Equip.gameObject.SetActive(true);
                    _synergyEffect_Equip.interactable = IsEquip == false;
                }

                if (_synergyEffect_EquipText != null)
                { 
                    _synergyEffect_EquipText.color = IsEquip == false ? _buttonTextEnableColor : _buttonTextDisaEnableColor;
                    _synergyEffect_EquipText.fontMaterial = IsEquip == false ? _buttonTextEnableMaterial : _buttonTextDisableMaterial;

                    Managers.LocalStringManager.Instance.SetLocalizeText(_synergyEffect_EquipText,
                        IsEquip == false ? "common/ui/equip" : "common/ui/equipped");
                }

                if (Managers.SynergyManager.Instance.NeedLimitBreak(synergyEffectData) == true)
                {
                    SynergyBreakthroughCostData synergyLevelUpLimitData = Managers.SynergyManager.Instance.GetSynergyLevelUpLimitData(synergyEffectData.SynergyType, skillInfo.LimitCompleteLevel);

                    int costIndex = synergyLevelUpLimitData.LimitBreakCostGoodsIndex;

                    int currentCount = (int)Managers.GoodsManager.Instance.GetGoodsAmount(costIndex);

                    int needCount = synergyLevelUpLimitData.LimitBreakCostGoodsValue;

                    bool readyEnhance = Managers.SynergyManager.Instance.ReadyCharacterLimitLevelUpCost(synergyLevelUpLimitData);

                    Sprite pointsprite = Managers.GoodsManager.Instance.GetGoodsSprite(costIndex);

                    if (_getBreakthroughBtn != null)
                    {
                        _getBreakthroughBtn.gameObject.SetActive(true);
                    }

                    if (_breakthroughMax != null)
                        _breakthroughMax.gameObject.SetActive(false);

                    if (_getBreakthroughtTitle != null)
                    {
                        _getBreakthroughtTitle.color = readyEnhance == true ? _buttonTextEnableColor : _buttonTextDisaEnableColor;
                        _getBreakthroughtTitle.fontMaterial = readyEnhance == true ? _buttonTextEnableMaterial : _buttonTextDisableMaterial;
                        Managers.LocalStringManager.Instance.SetLocalizeText(_getBreakthroughtTitle, "common/ui/promote");
                    }

                    if (_getBreakthroughPrice != null)
                    {
                        _getBreakthroughPrice.color = readyEnhance == true ? _buttonTextEnableColor : _buttonTextDisaEnableColor;
                        _getBreakthroughPrice.fontMaterial = readyEnhance == true ? _buttonTextEnableMaterial : _buttonTextDisableMaterial;
                        _getBreakthroughPrice.SetText("{0}/{1}", currentCount, needCount);
                    }

                    if (_getBreakthroughPriceIcon != null)
                    {
                        _getBreakthroughPriceIcon.sprite = pointsprite;
                        _getBreakthroughPriceIcon.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (_getBreakthroughBtn != null)
                    {
                        _getBreakthroughBtn.gameObject.SetActive(false);
                    }

                    if (_breakthroughMax != null)
                        _breakthroughMax.gameObject.SetActive(true);
                }

                {
                    bool isMax = Managers.SynergyManager.Instance.IsMaxLevelSynergy(synergyEffectData);

                    if (_synergyEffect_Enhance != null)
                        _synergyEffect_Enhance.gameObject.SetActive(isMax == false);

                    if (_synergyEffect_Enhance_Max != null)
                        _synergyEffect_Enhance_Max.gameObject.SetActive(isMax == true);

                    if (isMax == true)
                    {
                        if (_synergyEffect_EnhanceText != null)
                        {
                            _synergyEffect_EnhanceText.color = _buttonTextEnableColor;
                            _synergyEffect_EnhanceText.fontMaterial = _buttonTextEnableMaterial;
                            Managers.LocalStringManager.Instance.SetLocalizeText(_synergyEffect_EnhanceText, "Max");
                        }

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
                        int costIndex = Managers.SynergyManager.Instance.GetSynergyEnhanceCostGoodsIndex1(synergyEffectData);

                        int currentCount = (int)Managers.GoodsManager.Instance.GetGoodsAmount(costIndex);

                        int needCount = Managers.SynergyManager.Instance.GetSynergyEnhance_NeedCount1(synergyEffectData);

                        bool readyEnhance = currentCount >= needCount;

                        Sprite pointsprite = Managers.GoodsManager.Instance.GetGoodsSprite(costIndex);

                        if (_synergyEffect_EnhanceText != null)
                        {
                            _synergyEffect_EnhanceText.color = readyEnhance == true ? _buttonTextEnableColor : _buttonTextDisaEnableColor;
                            _synergyEffect_EnhanceText.fontMaterial = readyEnhance == true ? _buttonTextEnableMaterial : _buttonTextDisableMaterial;
                            Managers.LocalStringManager.Instance.SetLocalizeText(_synergyEffect_EnhanceText, "ui/reinforce");
                        }

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
                    }
                }


                SynergyReinforceStatData synergyReinforceStatData = Managers.SynergyManager.Instance.GetSynergyReinforceStatData(skillInfo.Level);
                if (synergyReinforceStatData != null)
                {
                    if (synergyReinforceStatData.AccEffectStat.ContainsKey(V2Enum_Stat.Attack) == true)
                    {
                        if (_reinforceStat_Attack != null)
                            _reinforceStat_Attack.SetText(string.Format("{0:0.#}", synergyReinforceStatData.AccEffectStat[V2Enum_Stat.Attack]));
                    }

                    if (synergyReinforceStatData.AccEffectStat.ContainsKey(V2Enum_Stat.Defence) == true)
                    {
                        if (_reinforceStat_Defence != null)
                            _reinforceStat_Defence.SetText(string.Format("{0:0.#}", synergyReinforceStatData.AccEffectStat[V2Enum_Stat.Defence]));
                    }

                    if (synergyReinforceStatData.AccEffectStat.ContainsKey(V2Enum_Stat.HP) == true)
                    {
                        if (_reinforceStat_HP != null)
                            _reinforceStat_HP.SetText(string.Format("{0:0.#}", synergyReinforceStatData.AccEffectStat[V2Enum_Stat.HP]));
                    }
                }
            }

            SetSynergyRune(synergyEffectData);

            SetUnLockDesc(synergyEffectData);

            RefreshSizeFilter().Forget();
        }
        //------------------------------------------------------------------------------------
        private void SetUnLockDesc(SynergyEffectData synergyEffectData)
        {
            bool locksynergy = Managers.SynergyManager.Instance.IsLockSynergy(synergyEffectData);

            if (lockSynergy != null)
                lockSynergy.gameObject.SetActive(locksynergy);

            if (locksynergy == true)
            {
                SynergyConditionData synergyConditionData = synergyEffectData.SynergyConditionData;
                if (synergyConditionData == null)
                    return;

                int index = 0;

                if (index >= _UILobbySynergyUnLockNoticeElementList.Count)
                    return;

                if (Managers.SynergyManager.Instance.IsOverLevel(synergyEffectData.SynergyType, 1, synergyConditionData.RequiredTier1Reinforce) == false)
                {
                    SynergyEffectData target = Managers.SynergyManager.Instance.GetEquipSynergyEffect(synergyEffectData.SynergyType, 1);

                    UILobbySynergyUnLockNoticeElement uILobbySynergyUnLockNoticeElement = _UILobbySynergyUnLockNoticeElementList[index];

                    uILobbySynergyUnLockNoticeElement.SetElement(target, synergyConditionData.RequiredTier1Reinforce);
                    uILobbySynergyUnLockNoticeElement.gameObject.SetActive(true);
                    index++;
                }

                if (index >= _UILobbySynergyUnLockNoticeElementList.Count)
                    return;

                if (Managers.SynergyManager.Instance.IsOverLevel(synergyEffectData.SynergyType, 2, synergyConditionData.RequiredTier2Reinforce) == false)
                {
                    SynergyEffectData target = Managers.SynergyManager.Instance.GetEquipSynergyEffect(synergyEffectData.SynergyType, 2);

                    UILobbySynergyUnLockNoticeElement uILobbySynergyUnLockNoticeElement = _UILobbySynergyUnLockNoticeElementList[index];

                    uILobbySynergyUnLockNoticeElement.SetElement(target, synergyConditionData.RequiredTier2Reinforce);
                    uILobbySynergyUnLockNoticeElement.gameObject.SetActive(true);
                    index++;
                }

                if (index >= _UILobbySynergyUnLockNoticeElementList.Count)
                    return;

                if (Managers.SynergyManager.Instance.IsOverLevel(synergyEffectData.SynergyType, 3, synergyConditionData.RequiredTier3Reinforce) == false)
                {
                    SynergyEffectData target = Managers.SynergyManager.Instance.GetEquipSynergyEffect(synergyEffectData.SynergyType, 3);

                    UILobbySynergyUnLockNoticeElement uILobbySynergyUnLockNoticeElement = _UILobbySynergyUnLockNoticeElementList[index];

                    uILobbySynergyUnLockNoticeElement.SetElement(target, synergyConditionData.RequiredTier3Reinforce);
                    uILobbySynergyUnLockNoticeElement.gameObject.SetActive(true);
                    index++;
                }

                if (index >= _UILobbySynergyUnLockNoticeElementList.Count)
                    return;

                if (Managers.SynergyManager.Instance.IsOverLevel(synergyEffectData.SynergyType, 4, synergyConditionData.RequiredTier4Reinforce) == false)
                {
                    SynergyEffectData target = Managers.SynergyManager.Instance.GetEquipSynergyEffect(synergyEffectData.SynergyType, 4);

                    UILobbySynergyUnLockNoticeElement uILobbySynergyUnLockNoticeElement = _UILobbySynergyUnLockNoticeElementList[index];

                    uILobbySynergyUnLockNoticeElement.SetElement(target, synergyConditionData.RequiredTier4Reinforce);
                    uILobbySynergyUnLockNoticeElement.gameObject.SetActive(true);
                    index++;
                }

                if (index >= _UILobbySynergyUnLockNoticeElementList.Count)
                    return;

                for (int i = index; i < _UILobbySynergyUnLockNoticeElementList.Count; ++i)
                {
                    _UILobbySynergyUnLockNoticeElementList[i].gameObject.SetActive(false);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private async UniTask RefreshSizeFilter()
        {
            await UniTask.NextFrame();
            for (int i = 0; i < customRefreshSizeFilter.Count; ++i)
            {
                customRefreshSizeFilter[i].SetLayoutVertical();
            }

            if (customRefresh != null)
            {
                for (int i = 0; i < customRefresh.Count; ++i)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(customRefresh[i]);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void ChangeEquipSynergy(GameBerry.Event.ChangeEquipSynergyMsg msg)
        {
            if (msg.AfterEquipSynergy == null)
                return;

            SynergyEffectData afterSynergyEffect = msg.AfterEquipSynergy;

            if (afterSynergyEffect.SynergyType == _currentSynergyType)
            {
                if (_uIGambleSynergyTierGroupElement_dic.ContainsKey(afterSynergyEffect.SynergyTier) == true)
                {
                    UILobbySynergyTierGroupElement uILobbySynergyTierGroupElement = _uIGambleSynergyTierGroupElement_dic[afterSynergyEffect.SynergyTier];
                    uILobbySynergyTierGroupElement.RefreshEquipEffectIcon();
                }
            }

            RefreshSynergyEffect(msg.BeforeEquipSynergy);
            RefreshSynergyEffect(msg.AfterEquipSynergy);

            SetSynergyEffectDetail(_currentSynergyEffectData);
        }
        //------------------------------------------------------------------------------------
        private void RefreshSynergyEffect(SynergyEffectData synergyEffectData)
        {
            if (synergyEffectData.SynergyType != _currentSynergyType)
                return;

            if (_uIGambleSynergyTierGroupElement_dic.ContainsKey(synergyEffectData.SynergyTier) == true)
            {
                UILobbySynergyTierGroupElement uILobbySynergyTierGroupElement = _uIGambleSynergyTierGroupElement_dic[synergyEffectData.SynergyTier];
                uILobbySynergyTierGroupElement.RefreshSynergyEffect(synergyEffectData);
            }
        }
        //------------------------------------------------------------------------------------
        private void ShowNewSynergy(GameBerry.Event.ShowNewSynergyMsg msg)
        {
            SynergyEffectData synergyEffectData = null;
            if (msg != null)
                synergyEffectData = msg.NewSynergyEffectData;

            if (synergyEffectData != null)
            {
                UILobbySynergyTabElement uILobbySynergyTabElement = _uIGambleSynergyViewElement_dic[synergyEffectData.SynergyType];

                if (uILobbySynergyTabElement != null)
                    uILobbySynergyTabElement.EnableNewDot(true);
            }
            else
            {
                foreach (var pair in _uIGambleSynergyViewElement_dic)
                {
                    UILobbySynergyTabElement uILobbySynergyTabElement = pair.Value;

                    if (uILobbySynergyTabElement != null)
                        uILobbySynergyTabElement.EnableNewDot(Managers.SynergyManager.Instance.GetNewSynergyIconCount(pair.Key) > 0);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void NoticeNewLobbySynergyElement(GameBerry.Event.NoticeNewLobbySynergyElementMsg msg)
        {
            if (_isEnter == false)
                return;

            SynergyEffectData synergyEffectData = msg.SynergyEffectData;
            if (msg == null)
                return;

            if (_currentSynergyType != synergyEffectData.SynergyType)
                return;

            SetSynergyPage(_currentSynergyType);

            if (_currentSynergyEffectData == synergyEffectData)
                SetSynergyEffectDetail(_currentSynergyEffectData);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ResetSynergy()
        {
            if (_currentSynergyEffectData == null)
                return;

            if (Managers.GuideInteractorManager.Instance.PlaySynergyOpenTutorial == true)
            {
                return;
            }

            Contents.GlobalContent.ShowPopup_OkCancel(
                Managers.LocalStringManager.Instance.GetLocalString("common/ui/shortagegoodstitle"),
                Managers.LocalStringManager.Instance.GetLocalString("ui/reset/synergy/desc"),
                () =>
                {
                    if (Managers.SynergyManager.Instance.DoSynergyLevelReset(_currentSynergyEffectData.SynergyType) == true)
                    {
                        SetSynergyEffectDetail(_currentSynergyEffectData);
                        SetSynergyPage(_currentSynergyEffectData.SynergyType);
                    }
                },
                null);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ChangeEquipSynergy()
        {
            if (_currentSynergyEffectData == null)
                return;

            Managers.SynergyManager.Instance.ChangeEquipSynergy(_currentSynergyEffectData);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_EnhanceSynergy()
        {
            if (_currentSynergyEffectData == null)
                return;

            if (Managers.GuideInteractorManager.Instance.PlaySynergyOpenTutorial == true)
            {
                if (Managers.GuideInteractorManager.Instance.GetCurrentStep() != 5)
                    return;
            }

            {
                if (Managers.SynergyManager.Instance.EnhanceSynergy(_currentSynergyEffectData) == true)
                {
                    SetSynergyEffectDetail(_currentSynergyEffectData);
                    SetSynergyPage(_currentSynergyEffectData.SynergyType);

                    if (_currentSynergyEffectData != null)
                    {
                        if (_uIGambleSynergyTierGroupElement_dic.ContainsKey(_currentSynergyEffectData.SynergyTier) == true)
                        {
                            UILobbySynergyTierGroupElement uILobbySynergyTierGroupElement = _uIGambleSynergyTierGroupElement_dic[_currentSynergyEffectData.SynergyTier];

                            if (uILobbySynergyTierGroupElement != null)
                            {
                                UILobbySynergyEffectElement uILobbySynergyEffectElement = uILobbySynergyTierGroupElement.GetUILobbySynergyEffectElement(_currentSynergyEffectData);
                                if (uILobbySynergyEffectElement != null)
                                {
                                    uILobbySynergyEffectElement.PlayLevelUpEffect();
                                }
                            }
                            
                        }
                    }

                    if (Managers.GuideInteractorManager.Instance.PlaySynergyOpenTutorial == true)
                    {
                        if (Managers.GuideInteractorManager.Instance.GetCurrentStep() == 5)
                        {
                            SynergyEffectData synergyEffectData = Managers.SynergyManager.Instance.GetEquipSynergyEffect(V2Enum_ARR_SynergyType.Red, 3);
                            if (Managers.SynergyManager.Instance.IsLockSynergy(synergyEffectData) == false)
                            {
                                if (_guideOpen5BG != null)
                                    _guideOpen5BG.gameObject.SetActive(false);


                                if (_guideOpen6BG != null)
                                    _guideOpen6BG.gameObject.SetActive(true);

                                if (_guideOpen6Btn != null)
                                    _guideOpen6Btn.gameObject.SetActive(true);

                                Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/synergyopen6"));
                                Managers.GuideInteractorManager.Instance.SetGuideStep(6);
                            }
                            
                        }
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_GetBreakthrough()
        {
            if (_currentSynergyEffectData == null)
                return;

            int targetidx = 0;

            SkillInfo skillInfo = Managers.SynergyManager.Instance.GetSynergyEffectSkillInfo(_currentSynergyEffectData);
            if (skillInfo != null)
            {
                targetidx = skillInfo.LimitCompleteLevel;
            }

            {
                if (Managers.SynergyManager.Instance.DoARRRLimitUp(_currentSynergyEffectData) == true)
                {
                    SetSynergyEffectDetail(_currentSynergyEffectData);
                    SetSynergyPage(_currentSynergyEffectData.SynergyType);


                    if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.SynergyBreak)
                    {
                        Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/skillbreak3"));

                        if (_guidebreak2BG != null)
                            _guidebreak2BG.gameObject.SetActive(false);

                        if (_guidebreak3BG != null)
                            _guidebreak3BG.gameObject.SetActive(true);

                        if (_uISynergyRuneElements.Count > 0)
                        {
                            _uISynergyRuneElements[0]._guidebreak3BG = _guidebreak3BG;
                        }
                    }

                    if (_uISynergyRuneElements.Count > targetidx)
                        _uISynergyRuneElements[targetidx].PlayGetEffect();
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void PlaySynergyChangeTutorial(GameBerry.Event.PlaySynergyChangeTutorialMsg msg)
        {
            //Canvas.ForceUpdateCanvases();


            //if (_synergyListRoot != null)
            //{
            //    LayoutRebuilder.ForceRebuildLayoutImmediate(_synergyListRoot);
            //}

            //if (_synergyHorizontalLayout != null)
            //    _synergyHorizontalLayout.SetLayoutHorizontal();

            Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/synergyskill1"));

            //if (_uIGambleSynergyViewElement_dic.ContainsKey(V2Enum_ARR_SynergyType.White) == true)
            //{
            //    UILobbySynergyTabElement uILobbySynergyTabElement = _uIGambleSynergyViewElement_dic[V2Enum_ARR_SynergyType.White];
            //    uILobbySynergyTabElement.SetSynergyChangeElement(dialogView.transform);
            //}

            //_tutorialIndex = msg.Index;
        }
        //------------------------------------------------------------------------------------
    }
}