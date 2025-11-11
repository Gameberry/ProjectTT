using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;
using Spine;
using Spine.Unity;

namespace GameBerry.UI
{
    public class PetContentDialog : IDialog
    {
        [Header("------------AllyElementGroup------------")]
        [SerializeField]
        private InfiniteScroll _allyElementInfinityScroll;

        [SerializeField]
        private Transform _allyEmpty;

        [Header("------------Filter------------")]
        [SerializeField]
        private Button _reverseSortBtn;

        [SerializeField]
        private Button _filterExtension;

        [SerializeField]
        private Button _filterOriginSize;

        [SerializeField]
        private List<Transform> _extensionViewElement;

        [SerializeField]
        private List<Transform> _originSizeViewElement;

        [SerializeField]
        private List<UICallBackBtnElement> _gradeFilter;

        private List<V2Enum_Grade> _allyGradeFilter = new List<V2Enum_Grade>();

        [Header("------------DetailView------------")]
        [SerializeField]
        private TMP_Text _petName;

        [SerializeField]
        private TMP_Text _petDesc;

        [SerializeField]
        private TMP_Text _petLevel;

        [SerializeField]
        private SkeletonGraphic _skeletonGraphic;


        [Header("------------Enhance------------")]
        [SerializeField]
        private Button _equipPetBtn;

        [SerializeField]
        private TMP_Text _equipPetText;

        [SerializeField]
        private Button _enhanceBtn;

        [SerializeField]
        private TMP_Text _enhanceText;

        [SerializeField]
        private Button _changeAnimationState;

        private PetData _currentPlayerCharacterInfo;
        private SpineModelData _currentSpineModelData;
        private bool isReverseSort = false;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {

            Message.AddListener<GameBerry.Event.RefreshCharacterInfoListMsg>(RefreshCharacterInfoList);
            Message.AddListener<GameBerry.Event.RefreshCharacterInfo_EnhanceMsg>(RefreshCharacterInfo_Enhance);

            if (_allyElementInfinityScroll != null)
                _allyElementInfinityScroll.AddSelectCallback(OnClick_AllyElement);



            if (_reverseSortBtn != null)
                _reverseSortBtn.onClick.AddListener(OnClick_ReverseSortBtn);

            if (_filterExtension != null)
                _filterExtension.onClick.AddListener(() =>
                {
                    for (int i = 0; i < _originSizeViewElement.Count; ++i)
                    {
                        if (_originSizeViewElement[i] != null)
                            _originSizeViewElement[i].gameObject.SetActive(false);
                    }

                    for (int i = 0; i < _extensionViewElement.Count; ++i)
                    {
                        if (_extensionViewElement[i] != null)
                            _extensionViewElement[i].gameObject.SetActive(true);
                    }
                });

            if (_filterOriginSize != null)
                _filterOriginSize.onClick.AddListener(() =>
                {
                    for (int i = 0; i < _originSizeViewElement.Count; ++i)
                    {
                        if (_originSizeViewElement[i] != null)
                            _originSizeViewElement[i].gameObject.SetActive(true);
                    }

                    for (int i = 0; i < _extensionViewElement.Count; ++i)
                    {
                        if (_extensionViewElement[i] != null)
                            _extensionViewElement[i].gameObject.SetActive(false);
                    }
                });

            for (int i = 0; i < _gradeFilter.Count; ++i)
            {
                _gradeFilter[i].SetCallBack(OnClick_GradeFilter);
                _allyGradeFilter.Add(_gradeFilter[i].m_myID.IntToEnum32<V2Enum_Grade>());
            }

            if (_changeAnimationState != null)
                _changeAnimationState.onClick.AddListener(OnClick_ChangeAllyDetailAnimation);

            if (_equipPetBtn != null)
                _equipPetBtn.onClick.AddListener(OnClick_EquipBtn);

            if (_enhanceBtn != null)
                _enhanceBtn.onClick.AddListener(OnClick_EnhanceBtn);

            SortAllyElement();
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.RefreshCharacterInfoListMsg>(RefreshCharacterInfoList);
            Message.RemoveListener<GameBerry.Event.RefreshCharacterInfo_EnhanceMsg>(RefreshCharacterInfo_Enhance);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            _currentPlayerCharacterInfo = null;

            SortAllyElement();

            if (_allyElementInfinityScroll != null)
            { 
                PetInfo petInfo = Managers.PetManager.Instance.GetEquipPet();
                PetData petData = null;

                if (petInfo != null)
                {
                    petData = Managers.PetManager.Instance.GetPetData(petInfo.Id);
                    _allyElementInfinityScroll.OnSelectItem(petData);
                }
                else
                {
                    List<PetData> playerCharacterInfos = Managers.PetManager.Instance.GetPetAllDatas();
                    if (playerCharacterInfos == null)
                        return;

                    playerCharacterInfos.Sort(Managers.PetManager.Instance.SortPetInfo);


                    if (playerCharacterInfos.Count > 0)
                    {
                        petData = playerCharacterInfos[0];
                        _allyElementInfinityScroll.OnSelectItem(petData);
                    }
                }

                if (petData != null)
                    SetAllyDetailView(petData);
                
                _allyElementInfinityScroll.UpdateAllData();
            }

            
        }
        //------------------------------------------------------------------------------------
        private void SortAllyElement(bool reverse = false)
        {
            if (_allyElementInfinityScroll != null)
            {
                List<PetData> playerV3AllyInfos = Managers.PetManager.Instance.GetPetAllDatas();
                if (playerV3AllyInfos == null)
                    return;

                if (_allyEmpty != null)
                    _allyEmpty.gameObject.SetActive(playerV3AllyInfos.Count <= 0);

                playerV3AllyInfos.Sort(Managers.PetManager.Instance.SortPetInfo);
                if (reverse == true)
                    playerV3AllyInfos.Reverse();

                isReverseSort = reverse;

                _allyElementInfinityScroll.Clear();

                for (int i = 0; i < playerV3AllyInfos.Count; ++i)
                {
                    _allyElementInfinityScroll.InsertData(playerV3AllyInfos[i]);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void SetAllyDetailView(PetData petData)
        {
            if (petData == null)
                return;

            if (_petName != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_petName, petData.NameLocalStringKey);

            if (_petDesc != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_petDesc, petData.DescLocalStringKey);


            if (_petLevel != null)
                _petLevel.SetText("Lv.{0}", Managers.PetManager.Instance.GetPetLevel(petData));

            _currentPlayerCharacterInfo = petData;

            _currentSpineModelData = StaticResource.Instance.GetCreatureSpineModelData(1001);

            if (_currentSpineModelData == null)
                return;

            if (_skeletonGraphic != null)
            {
                _skeletonGraphic.skeletonDataAsset = _currentSpineModelData.SkeletonData;
                _skeletonGraphic.initialSkinName = _currentSpineModelData.SkinList[0];
                _skeletonGraphic.Initialize(true);

                if (_currentSpineModelData.SkinList.Count > petData.ResourceSkin)
                //if (_currentSpineModelData.SkinList.Count > 0)
                {
                    _skeletonGraphic.Skeleton.SetSkin(_currentSpineModelData.SkinList[petData.ResourceSkin]);

                }
                else
                {
                    if (_currentSpineModelData.SkinList.Count > 0)
                    {
                        _skeletonGraphic.Skeleton.SetSkin(_currentSpineModelData.SkinList[0]);
                    }
                }

                _skeletonGraphic.AnimationState.SetAnimation(0, "Idle", true);
            }

            bool isEquipPet = Managers.PetManager.Instance.IsEquipPet(petData);

            if (_equipPetBtn != null)
                _equipPetBtn.interactable = isEquipPet == true ? false : true;

            if (_equipPetText != null)
                _equipPetText.color = isEquipPet == true ? Color.gray : Color.white;

            if (Managers.PetManager.Instance.CanLevelUp(petData) == true)
            {
                if (_enhanceBtn != null)
                    _enhanceBtn.interactable = true;

                if (_enhanceText != null)
                    _enhanceText.color = Color.white;
            }
            else
            { 
                bool isMaxLevel = Managers.PetManager.Instance.IsMaxLevel(petData);

                if (_enhanceBtn != null)
                    _enhanceBtn.interactable = false;

                if (_enhanceText != null)
                {
                    _enhanceText.SetText(isMaxLevel == true ? "MaxLevel" : "LevelUp");
                    _enhanceText.color = isMaxLevel == true ? Color.gray : Color.red;
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void Update()
        {
            if (_isEnter == false)
                return;

            if (Input.GetKey(KeyCode.A))
            {
                if (_currentPlayerCharacterInfo != null)
                { 
                    Managers.PetManager.Instance.AddPetAmount(_currentPlayerCharacterInfo, 1);

                    if (_allyElementInfinityScroll != null)
                        _allyElementInfinityScroll.UpdateAllData();

                    SetAllyDetailView(_currentPlayerCharacterInfo);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ChangeAllyDetailAnimation()
        {
            if (_currentSpineModelData == null)
                return;

            if (_skeletonGraphic != null)
            {
                SpineModelAnimationData spineModelAnimationData = _currentSpineModelData.AnimationList[Random.Range(0, _currentSpineModelData.AnimationList.Count)];

                //string animationName = _currentSpineModelData.AnimationList[Random.Range(0, _currentSpineModelData.AnimationList.Count)];

                if (spineModelAnimationData.stateName == "Idle" || spineModelAnimationData.stateName == "Run")
                {
                    _skeletonGraphic.AnimationState.SetAnimation(0, spineModelAnimationData.animation, true);
                }
                else
                {
                    _skeletonGraphic.AnimationState.SetAnimation(0, spineModelAnimationData.animation, false);
                    _skeletonGraphic.AnimationState.AddAnimation(0, "Idle", true, 0f);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_AllyElement(InfiniteScrollData infiniteScrollData)
        {
            PetData playerCharacterInfo = infiniteScrollData as PetData;

            if (playerCharacterInfo == null)
                return;

            SetAllyDetailView(playerCharacterInfo);
        }
        //------------------------------------------------------------------------------------
        private void RefreshCharacterInfoList(GameBerry.Event.RefreshCharacterInfoListMsg msg)
        {
            if (isEnter == false)
                return;

            if (_allyElementInfinityScroll != null)
            {
                for (int i = 0; i < msg.datas.Count; ++i)
                {
                    _allyElementInfinityScroll.UpdateData(msg.datas[i]);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshCharacterInfo_Enhance(GameBerry.Event.RefreshCharacterInfo_EnhanceMsg msg)
        {
            if (isEnter == false)
                return;

            if (_allyElementInfinityScroll != null)
            {
                _allyElementInfinityScroll.UpdateData(msg.playerV3AllyInfo);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_GradeFilter(int grade)
        {
            return;
            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.AllyEquip
                || Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.AllyLevelUp
                || Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.AllyStarUp)
                return;

            SetGradeFilter(grade);
        }
        //------------------------------------------------------------------------------------
        private void SetGradeFilter(int grade)
        {
            V2Enum_Grade v2Enum_Grade = grade.IntToEnum32<V2Enum_Grade>();

            UICallBackBtnElement uICallBackBtnElement = _gradeFilter.Find(x => x.m_myID == grade);

            if (_allyGradeFilter.Contains(v2Enum_Grade) == true)
            {
                _allyGradeFilter.Remove(v2Enum_Grade);
                if (uICallBackBtnElement != null)
                    uICallBackBtnElement.SetEnable(false);
            }
            else
            {
                _allyGradeFilter.Add(v2Enum_Grade);
                if (uICallBackBtnElement != null)
                    uICallBackBtnElement.SetEnable(true);
            }

            SortAllyElement();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ReverseSortBtn()
        {
            SortAllyElement(!isReverseSort);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_EquipBtn()
        {
            if (_currentPlayerCharacterInfo == null)
                return;

            if (Managers.PetManager.Instance.DoEquipPet(_currentPlayerCharacterInfo))
            {
                if (_allyElementInfinityScroll != null)
                    _allyElementInfinityScroll.UpdateAllData();

                SetAllyDetailView(_currentPlayerCharacterInfo);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_EnhanceBtn()
        {
            if (Managers.PetManager.Instance.DoLevelUp(_currentPlayerCharacterInfo))
            {
                if (_allyElementInfinityScroll != null)
                    _allyElementInfinityScroll.UpdateAllData();

                SetAllyDetailView(_currentPlayerCharacterInfo);
            }
        }
        //------------------------------------------------------------------------------------

    }
}