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
using Spine;
using Spine.Unity;

namespace GameBerry.UI
{
    public class LobbyGearContentDialog : IDialog
    {
        [Header("------------Guide------------")]
        [SerializeField]
        private Transform _guideBG4;

        [SerializeField]
        private Button _guide4CompleteBtn;

        [SerializeField]
        private Transform _guideBG5;

        private int _equipTutorialGearIdx = 0;

        [Header("-------Barbar-------")]
        [SerializeField]
        private SkeletonGraphic _skeletonGraphic;

        [Header("-------RuneTab-------")]
        [SerializeField]
        private CButton _synergySlotTab;

        [SerializeField]
        private CButton _synergyCombineTab;

        [SerializeField]
        private Transform _synergyCombineRedDot;

        [Header("------------Slot------------")]
        [SerializeField]
        private Transform uISynergySlotGroup;

        [SerializeField]
        private List<UILobbyGearSlotGroupElement> uIARRRSkillSlotElements = new List<UILobbyGearSlotGroupElement>();

        [SerializeField]
        private Transform _allEquipGear_RedDot;

        [SerializeField]
        private Button _allEquipGear;


        [Header("-------SynergyRuneCombine-------")]
        [SerializeField]
        private Transform uISynergyCombineGroup;

        [SerializeField]
        private Button _synergyCombine;

        [SerializeField]
        private Button _synergyCombineAutoSetting;

        [SerializeField]
        private List<UILobbyGearCombineGroupElement> uIRuneCombineGroup = new List<UILobbyGearCombineGroupElement>();


        [Header("-------SynergyRuneEffect-------")]
        [SerializeField]
        private InfiniteScroll _synergyRuneEffect;

        [SerializeField]
        private LobbyGearDetailPopupDialog _lobbySynergyRuneDetailPopupDialog;


        [Header("------------Filter------------")]
        [SerializeField]
        private List<UICallBackBtnElement> m_elementFilter;
        private List<V2Enum_GearType> m_allyElementFilter = new List<V2Enum_GearType>();

        private Dictionary<V2Enum_Grade, int> _gradeCount = new Dictionary<V2Enum_Grade, int>();

        private ContentDetailList _contentMode = ContentDetailList.None;

        private bool _playedCombineDirection = false;

        Skin myEquipsSkin = new Skin("my new skin");

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            RefreshSkin();

            for (int i = 0; i < uIARRRSkillSlotElements.Count; ++i)
            {
                uIARRRSkillSlotElements[i].Init(OnClick_Slot, OnClick_unEquipSlot);
            }

            if (_guide4CompleteBtn != null)
                _guide4CompleteBtn.onClick.AddListener(OnClick_Guide4Btn);

            if (_synergyRuneEffect != null)
                _synergyRuneEffect.AddSelectCallback(OnClick_RuneElement);


            if (_lobbySynergyRuneDetailPopupDialog != null)
                _lobbySynergyRuneDetailPopupDialog.Load_Element();

            for (int i = 0; i < m_elementFilter.Count; ++i)
            {
                m_elementFilter[i].SetCallBack(OnClick_ElementFilter);
                m_elementFilter[i].SetEnable(false);
            }

            if (_synergySlotTab != null)
                _synergySlotTab.onClick.AddListener(OnClick_ShowSlotMode);

            if (_synergyCombineTab != null)
                _synergyCombineTab.onClick.AddListener(OnClick_ShowCombineMode);

            if (_synergyCombineAutoSetting != null)
                _synergyCombineAutoSetting.onClick.AddListener(OnClick_CombineAutoSetting);

            if (_synergyCombine != null)
                _synergyCombine.onClick.AddListener(OnClick_Combine);

            if (_allEquipGear != null)
                _allEquipGear.onClick.AddListener(OnClick_AllEquipGear);

            for (int i = 0; i < uIRuneCombineGroup.Count; ++i)
            {
                uIRuneCombineGroup[i].SetGroup(OnRemoveMaterial, i);
            }

            SetContentMode(ContentDetailList.LobbyGear_Slot);

            Message.AddListener<GameBerry.Event.ChangeEquipStateGearMsg>(ChangeEquipStateSynergyRune);
            Message.AddListener<GameBerry.Event.RefreshGearAllSlotMsg>(RefreshGearAllSlot);
            Message.AddListener<GameBerry.Event.PlayGearEquipTutorialMsg>(PlayGearEquipTutorial);
            Message.AddListener<GameBerry.Event.RefreshCharacterSkin_StatMsg>(RefreshCharacterSkin_Stat);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.ChangeEquipStateGearMsg>(ChangeEquipStateSynergyRune);
            Message.RemoveListener<GameBerry.Event.RefreshGearAllSlotMsg>(RefreshGearAllSlot);
            Message.RemoveListener<GameBerry.Event.PlayGearEquipTutorialMsg>(PlayGearEquipTutorial);
            Message.RemoveListener<GameBerry.Event.RefreshCharacterSkin_StatMsg>(RefreshCharacterSkin_Stat);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            for (int i = 0; i < m_allyElementFilter.Count; ++i)
            {
                V2Enum_GearType v2Enum_ElementType = m_allyElementFilter[i];
                UICallBackBtnElement uICallBackBtnElement = m_elementFilter.Find(x => x.m_myID == v2Enum_ElementType.Enum32ToInt());
                uICallBackBtnElement.SetEnable(false);
            }

            m_allyElementFilter.Clear();

            SetRuneElement();

            RefreshGearAllSlot(null);

            SetContentMode(ContentDetailList.LobbyGear_Slot);

            SetAllEquipGear_RedDot();
            SetCombineTab_RedDot();

            //if (_lobbySynergyRuneDetailPopupDialog != null)
            //    _lobbySynergyRuneDetailPopupDialog.ElementExit();

            if (Managers.GuideInteractorManager.Instance.PlayGearTutorial == true
                || Managers.GuideInteractorManager.Instance.PlayGearEquipTutorial == true)
            {
                //if (_currentSynergyType == V2Enum_GearType.White)
                //{
                //    SynergyEffectData tuto = Managers.SynergyManager.Instance.GetSynergyEffectData(_tutorialIndex);
                //    if (tuto != null)
                //        SetSynergyEffectDetail(_currentSynergyEffectData);
                //}

                if (_guideBG4 != null)
                    _guideBG4.gameObject.SetActive(false);

                if (_guideBG5 != null)
                    _guideBG5.gameObject.SetActive(false);

                PlayNextWaveDelay().Forget();
            }

            Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.LobbyGear);
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (_guideBG4 != null)
                _guideBG4.gameObject.SetActive(false);

            if (_guideBG5 != null)
                _guideBG5.gameObject.SetActive(false);

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.GearTutorial)
            {
                if (Managers.GuideInteractorManager.Instance.GetCurrentStep() == 5)
                {
                    Managers.GuideInteractorManager.Instance.EndGuideQuest();
                }
                else if (Managers.GuideInteractorManager.Instance.GetCurrentStep() == 4)
                {
                    Managers.GuideInteractorManager.Instance.SetGuideStep(3);
                }
            }

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.GearEquipTutorial)
            {
                if (Managers.GuideInteractorManager.Instance.GetCurrentStep() == 3)
                {
                    Managers.GuideInteractorManager.Instance.EndGuideQuest();
                }
                else if (Managers.GuideInteractorManager.Instance.GetCurrentStep() == 2)
                {
                    Managers.GuideInteractorManager.Instance.SetGuideStep(1);
                }
            }

            Managers.GearManager.Instance.RemoveAllNewIconSynergy();
            Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.LobbyGear);
        }
        //------------------------------------------------------------------------------------
        private void RefreshCharacterSkin_Stat(GameBerry.Event.RefreshCharacterSkin_StatMsg msg)
        {
            RefreshSkin();
        }
        //------------------------------------------------------------------------------------
        private void RefreshSkin()
        {
            SpineModelData _currentSpineModelData = StaticResource.Instance.GetARRRSpineModelData();

            if (_skeletonGraphic != null)
            {
                _skeletonGraphic.skeletonDataAsset = _currentSpineModelData.SkeletonData;
                _skeletonGraphic.initialSkinName = _currentSpineModelData.SkinList[0];
                _skeletonGraphic.Initialize(true);

                Skeleton skeleton = _skeletonGraphic.Skeleton;
                SkeletonData skeletonData = skeleton.Data;

                // 초기 스킨 세팅
                skeleton.SetSkin(_currentSpineModelData.SkinList[0]);

                myEquipsSkin.SetARRRSkin(skeletonData);

                skeleton.SetSkin(myEquipsSkin);
                skeleton.SetSlotsToSetupPose(); // 포즈 적용

                _skeletonGraphic.AnimationState.ClearTracks(); // 스킨 적용 후 초기화
                _skeletonGraphic.AnimationState.SetAnimation(0, "Idle", true); // Idle 적용
            }
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlayNextWaveDelay()
        {
            await UniTask.Delay(500);
            if (Managers.GuideInteractorManager.Instance.PlayGearTutorial == true)
            {
                Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/gear3"));

                if (_guideBG4 != null)
                    _guideBG4.gameObject.SetActive(true);

                Managers.GuideInteractorManager.Instance.SetGuideStep(3);

            }


            if (Managers.GuideInteractorManager.Instance.PlayGearEquipTutorial == true)
            {
                Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/gear5"), 4, 0);

                if (_guideBG4 != null)
                    _guideBG4.gameObject.SetActive(true);

                Managers.GuideInteractorManager.Instance.SetGuideStep(3);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Guide4Btn()
        {
            if (Managers.GuideInteractorManager.Instance.PlayGearTutorial == true)
            {
                List<GearData> playerV3AllyInfos = Managers.GearManager.Instance.GetAllGearData();
                if (playerV3AllyInfos == null)
                    return;

                if (m_allyElementFilter.Count > 0)
                {
                    playerV3AllyInfos = playerV3AllyInfos.FindAll(
                        x => Managers.GearManager.Instance.GetDisplayEquipRuneCount(x) > 0
                        && m_allyElementFilter.Contains(x.GearType) == true);
                }
                else
                {
                    playerV3AllyInfos = playerV3AllyInfos.FindAll(
                        x => Managers.GearManager.Instance.GetDisplayEquipRuneCount(x) > 0);
                }

                playerV3AllyInfos.Sort(Managers.GearManager.Instance.SortRuneData);

                if (playerV3AllyInfos.Count <= 0)
                {
                    Managers.GuideInteractorManager.Instance.EndGuideQuest();
                }
                else
                {
                    Click_Rune(playerV3AllyInfos[0]);

                    if (_guideBG5 != null)
                        _guideBG5.gameObject.SetActive(true);

                    Managers.GuideInteractorManager.Instance.SetGuideStep(5);
                }

                if (_guideBG4 != null)
                    _guideBG4.gameObject.SetActive(false);
            }


            if (Managers.GuideInteractorManager.Instance.PlayGearEquipTutorial == true)
            {
                GearData gearData = Managers.GearManager.Instance.GetSynergyEffectData(_equipTutorialGearIdx);
                if (gearData != null)
                {
                    Click_Rune(gearData);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ShowSlotMode()
        {
            SetContentMode(ContentDetailList.LobbyGear_Slot);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ShowCombineMode()
        {
            SetContentMode(ContentDetailList.LobbyGear_Combine);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_AllEquipGear()
        {
            if (Managers.GuideInteractorManager.Instance.PlayGearTutorial == true
                || Managers.GuideInteractorManager.Instance.PlayGearEquipTutorial == true)
            {
                return;
            }

            if (Managers.GearManager.Instance.AllEquipGear() == true)
            {
                RefreshGearAllSlot(null);
                SetRuneElement();
                if (_allEquipGear_RedDot != null)
                    _allEquipGear_RedDot.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        private void SetAllEquipGear_RedDot()
        {
            if (_allEquipGear_RedDot != null)
                _allEquipGear_RedDot.gameObject.SetActive(Managers.GearManager.Instance.CanGearEquip());
        }
        //------------------------------------------------------------------------------------
        private void SetCombineTab_RedDot()
        {
            if (_synergyCombineRedDot != null)
                _synergyCombineRedDot.gameObject.SetActive(Managers.GearManager.Instance.CanCombine());
        }
        //------------------------------------------------------------------------------------
        
        private void SetContentMode(ContentDetailList contentDetailList)
        {
            if (_contentMode == contentDetailList)
                return;

            if (_synergySlotTab != null)
                _synergySlotTab.SetInteractable(contentDetailList != ContentDetailList.LobbyGear_Slot);

            if (uISynergySlotGroup != null)
                uISynergySlotGroup.gameObject.SetActive(contentDetailList == ContentDetailList.LobbyGear_Slot);


            if (_synergyCombineTab != null)
                _synergyCombineTab.SetInteractable(contentDetailList != ContentDetailList.LobbyGear_Combine);

            if (uISynergyCombineGroup != null)
                uISynergyCombineGroup.gameObject.SetActive(contentDetailList == ContentDetailList.LobbyGear_Combine);

            if (contentDetailList == ContentDetailList.LobbyGear_Slot)
            {
                SetAllEquipGear_RedDot();
                SetCombineTab_RedDot();
            }
            else
            {
                if (_synergyCombineRedDot != null)
                    _synergyCombineRedDot.gameObject.SetActive(false);
            }

            for (int i = 0; i < uIRuneCombineGroup.Count; ++i)
            {
                uIRuneCombineGroup[i].ReleaseMaterial();
            }

            if (_synergyCombineAutoSetting != null)
                _synergyCombineAutoSetting.gameObject.SetActive(true);

            _playedCombineDirection = false;
            _contentMode = contentDetailList;

            SetRuneElement();
        }
        //------------------------------------------------------------------------------------
        private void ChangeEquipStateSynergyRune(GameBerry.Event.ChangeEquipStateGearMsg msg)
        {
            UILobbyGearSlotGroupElement uILobbySynergyRuneSlotGroupElement = uIARRRSkillSlotElements.Find(x => x.SynergyType == msg.v2Enum_ARR_SynergyType);
            if (uILobbySynergyRuneSlotGroupElement != null)
                uILobbySynergyRuneSlotGroupElement.RefreshAllSlot();

            SetRuneElement();

            SetAllEquipGear_RedDot();
        }
        //------------------------------------------------------------------------------------
        private void RefreshGearAllSlot(GameBerry.Event.RefreshGearAllSlotMsg msg)
        {
            for (int i = 0; i < uIARRRSkillSlotElements.Count; ++i)
            {
                uIARRRSkillSlotElements[i].RefreshAllSlot();
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_unEquipSlot(V2Enum_GearType v2Enum_ARR_SynergyType)
        {
            GearData synergyRuneData = Managers.GearManager.Instance.EquipedRuneData(v2Enum_ARR_SynergyType);
            if (synergyRuneData == null)
                return;

            Managers.GearManager.Instance.UnEquipSkillSlot(
                v2Enum_ARR_SynergyType);

            //if (_lobbySynergyRuneDetailPopupDialog != null)
            //{
            //    _lobbySynergyRuneDetailPopupDialog.SynergyRuneElement(synergyRuneData);
            //    _lobbySynergyRuneDetailPopupDialog.SetEquipMode(false);
            //    _lobbySynergyRuneDetailPopupDialog.SetSlotState(v2Enum_ARR_SynergyType, slotidx);
            //    _lobbySynergyRuneDetailPopupDialog.ElementEnter();
            //}

            SetAllEquipGear_RedDot();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Slot(V2Enum_GearType v2Enum_ARR_SynergyType)
        {
            GearData synergyRuneData = Managers.GearManager.Instance.EquipedRuneData(v2Enum_ARR_SynergyType);
            if (synergyRuneData == null)
                return;

            if (Managers.GuideInteractorManager.Instance.PlayGearEquipTutorial == true)
            {
                GearData gearData = Managers.GearManager.Instance.GetSynergyEffectData(_equipTutorialGearIdx);

                Managers.GuideInteractorManager.Instance.SetGuideStep(3);

                if (_guideBG4 != null)
                    _guideBG4.gameObject.SetActive(false);

                Click_Rune(gearData);
                return;
            }

            if (_lobbySynergyRuneDetailPopupDialog != null)
            {
                _lobbySynergyRuneDetailPopupDialog.SynergyRuneElement(synergyRuneData);
                _lobbySynergyRuneDetailPopupDialog.SetSlotState(v2Enum_ARR_SynergyType);
                _lobbySynergyRuneDetailPopupDialog.SetEquipMode(false);
                _lobbySynergyRuneDetailPopupDialog.ElementEnter();
            }

            if (Managers.GuideInteractorManager.Instance.PlayGearTutorial == true)
            {
                if (_guideBG5 != null)
                    _guideBG5.gameObject.SetActive(true);

                Managers.GuideInteractorManager.Instance.SetGuideStep(5);

                if (_guideBG4 != null)
                    _guideBG4.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnRemoveMaterial(GearData synergyRuneData)
        {
            if (Managers.GearManager.Instance.GetCurrentMaterialCount() <= 0)
            {
                SetRuneElement();
                if (_synergyCombineAutoSetting != null)
                    _synergyCombineAutoSetting.gameObject.SetActive(true);
            }
            else
            {
                SetRuneElement(synergyRuneData.Grade, synergyRuneData.GearType);
            }


            //if (_synergyCombineAutoSetting != null)
            //{
            //    if (Managers.GearManager.Instance.GetCurrentMaterialCount() <= 0)
            //        _synergyCombineAutoSetting.gameObject.SetActive(true);
            //}
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Combine()
        {
            if (_playedCombineDirection == true)
                return;

            bool complete = false;

            for (int i = 0; i < uIRuneCombineGroup.Count; ++i)
            {
                bool result = uIRuneCombineGroup[i].DoCombine();
                if (complete == false)
                    complete = result;
            }


            if (complete == true)
            {
                _playedCombineDirection = true;
                PlayCombineDirection().Forget();

                Managers.ShopManager.Instance.RefreshProductContitionType(V2Enum_OpenConditionType.RuneCombineCount);
            }
            else
            {
                if (_synergyCombineAutoSetting != null)
                    _synergyCombineAutoSetting.gameObject.SetActive(true);
            }

            SetRuneElement();

            for (int i = 0; i < uIARRRSkillSlotElements.Count; ++i)
            {
                uIARRRSkillSlotElements[i].RefreshAllSlot();
            }
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlayCombineDirection()
        {
            await UniTask.WaitForSeconds(1);

            for (int i = 0; i < uIRuneCombineGroup.Count; ++i)
            {
                uIRuneCombineGroup[i].ReleaseMaterial();
            }

            if (_synergyCombineAutoSetting != null)
                _synergyCombineAutoSetting.gameObject.SetActive(true);

            _playedCombineDirection = false;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_CombineAutoSetting()
        {
            if (m_allyElementFilter.Count > 0)
            {
                for (int gear = 0; gear < m_allyElementFilter.Count; ++gear)
                {
                    SetAutoMaterial(m_allyElementFilter[gear]);
                }
            }
            else
            {
                for (int gear = V2Enum_GearType.Weapon.Enum32ToInt(); gear < V2Enum_GearType.Max.Enum32ToInt(); ++gear)
                {
                    V2Enum_GearType v2Enum_GearType = gear.IntToEnum32<V2Enum_GearType>();

                    SetAutoMaterial(v2Enum_GearType);
                }
            }

            SetRuneElement();
        }
        //------------------------------------------------------------------------------------
        private void SetAutoMaterial(V2Enum_GearType v2Enum_GearType)
        {
            List<GearData> playerV3AllyInfos = Managers.GearManager.Instance.GetAllGearData(v2Enum_GearType);
            if (playerV3AllyInfos == null)
                return;

            playerV3AllyInfos = playerV3AllyInfos.FindAll(
                    x => Managers.GearManager.Instance.GetDisplayEquipRuneCount(x) > 0);

            _gradeCount.Clear();

            for (int i = 0; i < playerV3AllyInfos.Count; ++i)
            {
                GearData matcount = playerV3AllyInfos[i];

                if (_gradeCount.ContainsKey(matcount.Grade) == false)
                    _gradeCount.Add(matcount.Grade, 0);

                _gradeCount[matcount.Grade] += Managers.GearManager.Instance.GetDisplayEquipRuneCount(matcount);
            }

            for (int i = 0; i < V2Enum_Grade.Max.Enum32ToInt(); ++i)
            {
                V2Enum_Grade grade = i.IntToEnum32<V2Enum_Grade>();
                if (_gradeCount.ContainsKey(grade) == false)
                    continue;

                int matcount = _gradeCount[grade];

                GearCombineData synergyRuneCombineData = Managers.GearManager.Instance.GetGearCombineData(grade);
                if (synergyRuneCombineData == null)
                {
                    _gradeCount[grade] = 0;
                    continue;
                }

                _gradeCount[grade] = matcount - (matcount % synergyRuneCombineData.RequiredCount);
            }

            playerV3AllyInfos.Sort(Managers.GearManager.Instance.SortRuneData);

            playerV3AllyInfos.Reverse();

            for (int i = 0; i < playerV3AllyInfos.Count; ++i)
            {
                int materialcount = Managers.GearManager.Instance.GetDisplayEquipRuneCount(playerV3AllyInfos[i]);

                GearData material = playerV3AllyInfos[i];

                if (_gradeCount.ContainsKey(material.Grade) == false)
                    continue;

                if (_gradeCount[material.Grade] == 0)
                    continue;

                for (int j = 0; j < materialcount; ++j)
                {
                    if (AddMaterial(material) == false)
                        return;

                    _gradeCount[material.Grade]--;

                    if (_gradeCount[material.Grade] == 0)
                        break;
                }
            }

            _gradeCount.Clear();
        }
        //------------------------------------------------------------------------------------
        private bool AddMaterial(GearData synergyRuneData)
        {
            for (int i = 0; i < uIRuneCombineGroup.Count; ++i)
            {
                if (uIRuneCombineGroup[i].AddMaterial(synergyRuneData) == true)
                {
                    return true;
                }
            }

            return false;
        }
        //------------------------------------------------------------------------------------
        private void SetRuneElement(V2Enum_Grade v2Enum_Grade = V2Enum_Grade.Max, V2Enum_GearType v2Enum_GearType = V2Enum_GearType.Max)
        {
            if (_synergyRuneEffect != null)
            {
                List<GearData> playerV3AllyInfos = Managers.GearManager.Instance.GetAllGearData();
                if (playerV3AllyInfos == null)
                    return;

                if (m_allyElementFilter.Count > 0)
                {
                    playerV3AllyInfos = playerV3AllyInfos.FindAll(
                        x => Managers.GearManager.Instance.GetDisplayEquipRuneCount(x) > 0
                        && m_allyElementFilter.Contains(x.GearType) == true);
                }
                else
                {
                    playerV3AllyInfos = playerV3AllyInfos.FindAll(
                        x => Managers.GearManager.Instance.GetDisplayEquipRuneCount(x) > 0);
                }

                if (v2Enum_Grade != V2Enum_Grade.Max)
                {
                    playerV3AllyInfos = playerV3AllyInfos.FindAll(
                    x => x.Grade == v2Enum_Grade);
                }

                if (v2Enum_GearType != V2Enum_GearType.Max)
                {
                    playerV3AllyInfos = playerV3AllyInfos.FindAll(
                    x => x.GearType == v2Enum_GearType);
                }

                playerV3AllyInfos.Sort(Managers.GearManager.Instance.SortRuneData);

                _synergyRuneEffect.Clear();

                for (int i = 0; i < playerV3AllyInfos.Count; ++i)
                {
                    _synergyRuneEffect.InsertData(playerV3AllyInfos[i]);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ElementFilter(int element)
        {
            SetElementFilter(element);
        }
        //------------------------------------------------------------------------------------
        private void SetElementFilter(int element)
        {
            V2Enum_GearType v2Enum_ElementType = element.IntToEnum32<V2Enum_GearType>();

            UICallBackBtnElement uICallBackBtnElement = m_elementFilter.Find(x => x.m_myID == element);

            if (m_allyElementFilter.Contains(v2Enum_ElementType) == true)
            {
                m_allyElementFilter.Remove(v2Enum_ElementType);
                if (uICallBackBtnElement != null)
                    uICallBackBtnElement.SetEnable(false);
            }
            else
            {
                m_allyElementFilter.Add(v2Enum_ElementType);
                if (uICallBackBtnElement != null)
                    uICallBackBtnElement.SetEnable(true);
            }

            SetRuneElement();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_RuneElement(InfiniteScrollData infiniteScrollData)
        {
            if (_contentMode == ContentDetailList.LobbyGear_Combine && _playedCombineDirection == true)
                return;

            GearData playerCharacterInfo = infiniteScrollData as GearData;
            if (playerCharacterInfo == null)
                return;

            if (Managers.GuideInteractorManager.Instance.PlayGearEquipTutorial == true)
            {
                if (playerCharacterInfo.Index != _equipTutorialGearIdx)
                {
                    GearData gearData = Managers.GearManager.Instance.GetSynergyEffectData(_equipTutorialGearIdx);

                    Managers.GuideInteractorManager.Instance.SetGuideStep(3);

                    if (_guideBG4 != null)
                        _guideBG4.gameObject.SetActive(false);

                    Click_Rune(gearData);
                    return;
                }
                
            }


            Click_Rune(playerCharacterInfo);
        }
        //------------------------------------------------------------------------------------
        private void Click_Rune(GearData playerCharacterInfo)
        {
            if (_contentMode == ContentDetailList.LobbyGear_Combine)
            {
                AddMaterial(playerCharacterInfo);
                SetRuneElement(playerCharacterInfo.Grade, playerCharacterInfo.GearType);

                if (_synergyCombineAutoSetting != null)
                    _synergyCombineAutoSetting.gameObject.SetActive(false);

                return;
            }

            if (_lobbySynergyRuneDetailPopupDialog != null)
            {
                _lobbySynergyRuneDetailPopupDialog.SynergyRuneElement(playerCharacterInfo);
                _lobbySynergyRuneDetailPopupDialog.SetEquipMode(true);
                _lobbySynergyRuneDetailPopupDialog.ElementEnter();
            }

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.GearTutorial)
            {
                if (_guideBG5 != null)
                    _guideBG5.gameObject.SetActive(true);

                Managers.GuideInteractorManager.Instance.SetGuideStep(5);

                if (_guideBG4 != null)
                    _guideBG4.gameObject.SetActive(false);
            }

            if (Managers.GuideInteractorManager.Instance.PlayGearEquipTutorial == true)
            {
                Managers.GuideInteractorManager.Instance.SetGuideStep(3);

                if (_guideBG4 != null)
                    _guideBG4.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        private void PlayGearEquipTutorial(GameBerry.Event.PlayGearEquipTutorialMsg msg)
        {
            _equipTutorialGearIdx = msg.Index;
        }
        //------------------------------------------------------------------------------------
    }
}