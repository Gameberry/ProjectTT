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
    public class LobbySynergyRuneContentDialog : IDialog
    {
        [Header("------------Guide------------")]
        [SerializeField]
        private Transform _guideBG4;

        [SerializeField]
        private Button _guide4CompleteBtn;

        [SerializeField]
        private Transform _guideBG5;

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
        private UILobbySynergyRuneSlotGroupElement _uILobbySynergyRuneSlotGroupElement;


        [Header("-------SynergyRuneCombine-------")]
        [SerializeField]
        private Transform uISynergyCombineGroup;

        [SerializeField]
        private Button _synergyCombine;

        [SerializeField]
        private Button _synergyCombineAutoSetting;

        [SerializeField]
        private List<UILobbySynergyRuneCombineGroupElement> uIRuneCombineGroup = new List<UILobbySynergyRuneCombineGroupElement>();
        

        [Header("-------SynergyRuneEffect-------")]
        [SerializeField]
        private InfiniteScroll _synergyRuneEffect;

        [SerializeField]
        private LobbySynergyRuneDetailPopupDialog _lobbySynergyRuneDetailPopupDialog;


        [Header("------------Filter------------")]
        [SerializeField]
        private List<UICallBackBtnElement> m_elementFilter;
        private List<V2Enum_ARR_SynergyType> m_allyElementFilter = new List<V2Enum_ARR_SynergyType>();

        private Dictionary<V2Enum_Grade, int> _gradeCount = new Dictionary<V2Enum_Grade, int>();

        private ContentDetailList _contentMode = ContentDetailList.None;

        private bool _playedCombineDirection = false;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (_uILobbySynergyRuneSlotGroupElement != null)
                _uILobbySynergyRuneSlotGroupElement.Init(OnClick_Slot, OnClick_unEquipSlot);

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

            for (int i = 0; i < uIRuneCombineGroup.Count; ++i)
            {
                uIRuneCombineGroup[i].SetGroup(OnRemoveMaterial, i);
            }

            SetContentMode(ContentDetailList.LobbySynergyRune_Slot);

            Message.AddListener<GameBerry.Event.ChangeEquipStateSynergyRuneMsg>(ChangeEquipStateSynergyRune);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.ChangeEquipStateSynergyRuneMsg>(ChangeEquipStateSynergyRune);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            SetRuneElement();

            if (_uILobbySynergyRuneSlotGroupElement != null)
                _uILobbySynergyRuneSlotGroupElement.RefreshAllSlot();

            SetContentMode(ContentDetailList.LobbySynergyRune_Slot);
            SetCombineTab_RedDot();

            if (_lobbySynergyRuneDetailPopupDialog != null)
                _lobbySynergyRuneDetailPopupDialog.ElementExit();

            if (Managers.GuideInteractorManager.Instance.PlayRuneTutorial == true)
            {
                //if (_currentSynergyType == V2Enum_ARR_SynergyType.White)
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

            Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.LobbySynergyRune);
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.RuneTutorial)
            {
                if (_guideBG4 != null)
                    _guideBG4.gameObject.SetActive(false);

                if (_guideBG5 != null)
                    _guideBG5.gameObject.SetActive(false);

                if (Managers.GuideInteractorManager.Instance.GetCurrentStep() == 5)
                {


                    Managers.GuideInteractorManager.Instance.EndGuideQuest();
                }
                else if (Managers.GuideInteractorManager.Instance.GetCurrentStep() == 4)
                {
                    Managers.GuideInteractorManager.Instance.SetGuideStep(3);
                }
            }

            Managers.SynergyRuneManager.Instance.RemoveAllNewIconSynergy();
            Managers.RedDotManager.Instance.HideRedDot(ContentDetailList.LobbySynergyRune);
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlayNextWaveDelay()
        {
            await UniTask.NextFrame();
            if (Managers.GuideInteractorManager.Instance.PlayRuneTutorial == true)
            {
                Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/rune3"));

                if (_guideBG4 != null)
                    _guideBG4.gameObject.SetActive(true);

                Managers.GuideInteractorManager.Instance.SetGuideStep(3);

            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Guide4Btn()
        {
            List<SynergyRuneData> playerV3AllyInfos = Managers.SynergyRuneManager.Instance.GetAllSynergyRuneData();
            if (playerV3AllyInfos == null)
                return;

            if (m_allyElementFilter.Count > 0)
            {
                playerV3AllyInfos = playerV3AllyInfos.FindAll(
                    x => Managers.SynergyRuneManager.Instance.GetDisplayEquipRuneCount(x) > 0
                    && m_allyElementFilter.Contains(x.SynergyType) == true);
            }
            else
            {
                playerV3AllyInfos = playerV3AllyInfos.FindAll(
                    x => Managers.SynergyRuneManager.Instance.GetDisplayEquipRuneCount(x) > 0);
            }

            playerV3AllyInfos.Sort(Managers.SynergyRuneManager.Instance.SortRuneData);

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
        //------------------------------------------------------------------------------------
        private void OnClick_ShowSlotMode()
        {
            SetContentMode(ContentDetailList.LobbySynergyRune_Slot);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ShowCombineMode()
        {
            SetContentMode(ContentDetailList.LobbySynergyRune_Combine);
        }
        //------------------------------------------------------------------------------------
        private void SetCombineTab_RedDot()
        {
            if (_synergyCombineRedDot != null)
                _synergyCombineRedDot.gameObject.SetActive(Managers.SynergyRuneManager.Instance.CanCombine());
        }
        //------------------------------------------------------------------------------------
        private void SetContentMode(ContentDetailList contentDetailList)
        {
            if (_contentMode == contentDetailList)
                return;

            if (_synergySlotTab != null)
                _synergySlotTab.SetInteractable(contentDetailList != ContentDetailList.LobbySynergyRune_Slot);

            if (uISynergySlotGroup != null)
                uISynergySlotGroup.gameObject.SetActive(contentDetailList == ContentDetailList.LobbySynergyRune_Slot);


            if (_synergyCombineTab != null)
                _synergyCombineTab.SetInteractable(contentDetailList != ContentDetailList.LobbySynergyRune_Combine);

            if (uISynergyCombineGroup != null)
                uISynergyCombineGroup.gameObject.SetActive(contentDetailList == ContentDetailList.LobbySynergyRune_Combine);

            if (contentDetailList == ContentDetailList.LobbySynergyRune_Slot)
            {
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
        private void ChangeEquipStateSynergyRune(GameBerry.Event.ChangeEquipStateSynergyRuneMsg msg)
        {
            if (_uILobbySynergyRuneSlotGroupElement != null)
                _uILobbySynergyRuneSlotGroupElement.RefreshAllSlot();

            SetRuneElement();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_unEquipSlot(V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType, int slotidx)
        {
            bool isopen = Managers.SynergyRuneManager.Instance.IsOpenDescendSlot(slotidx);
            if (isopen == false)
            {
                Managers.SynergyRuneManager.Instance.ShowNoticeDescendSlotUnLockExp(slotidx);
                return;
            }

            SynergyRuneData synergyRuneData = Managers.SynergyRuneManager.Instance.EquipedRuneData(slotidx);
            if (synergyRuneData == null)
                return;

            Managers.SynergyRuneManager.Instance.UnEquipSkillSlot(
                slotidx);

            //if (_lobbySynergyRuneDetailPopupDialog != null)
            //{
            //    _lobbySynergyRuneDetailPopupDialog.SynergyRuneElement(synergyRuneData);
            //    _lobbySynergyRuneDetailPopupDialog.SetEquipMode(false);
            //    _lobbySynergyRuneDetailPopupDialog.SetSlotState(v2Enum_ARR_SynergyType, slotidx);
            //    _lobbySynergyRuneDetailPopupDialog.ElementEnter();
            //}
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Slot(V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType, int slotidx)
        {
            bool isopen = Managers.SynergyRuneManager.Instance.IsOpenDescendSlot(slotidx);
            if (isopen == false)
            {
                Managers.SynergyRuneManager.Instance.ShowNoticeDescendSlotUnLockExp(slotidx);
                return;
            }

            SynergyRuneData synergyRuneData = Managers.SynergyRuneManager.Instance.EquipedRuneData(slotidx);
            if (synergyRuneData == null)
                return;

            if (_lobbySynergyRuneDetailPopupDialog != null)
            {
                _lobbySynergyRuneDetailPopupDialog.SynergyRuneElement(synergyRuneData);
                _lobbySynergyRuneDetailPopupDialog.SetEquipMode(false);
                _lobbySynergyRuneDetailPopupDialog.SetSlotState(v2Enum_ARR_SynergyType, slotidx);
                _lobbySynergyRuneDetailPopupDialog.ElementEnter();
            }

            if (Managers.GuideInteractorManager.Instance.PlayRuneTutorial == true)
            {
                if (_guideBG5 != null)
                    _guideBG5.gameObject.SetActive(true);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnRemoveMaterial(SynergyRuneData synergyRuneData)
        {
            if (Managers.SynergyRuneManager.Instance.GetCurrentMaterialCount() <= 0)
            {
                SetRuneElement();
                if (_synergyCombineAutoSetting != null)
                    _synergyCombineAutoSetting.gameObject.SetActive(true);
            }
            else
            {
                SetRuneElement(synergyRuneData.Grade);
            }
            

            //if (_synergyCombineAutoSetting != null)
            //{
            //    if (Managers.SynergyRuneManager.Instance.GetCurrentMaterialCount() <= 0)
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

            if (_uILobbySynergyRuneSlotGroupElement != null)
                _uILobbySynergyRuneSlotGroupElement.RefreshAllSlot();
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
            List<SynergyRuneData> playerV3AllyInfos = Managers.SynergyRuneManager.Instance.GetAllSynergyRuneData();
            if (playerV3AllyInfos == null)
                return;

            playerV3AllyInfos = playerV3AllyInfos.FindAll(
                    x => Managers.SynergyRuneManager.Instance.GetDisplayEquipRuneCount(x) > 0);

            _gradeCount.Clear();

            V2Enum_Grade v2Enum_Grade = V2Enum_Grade.Max;

            for (int i = 0; i < playerV3AllyInfos.Count; ++i)
            {
                SynergyRuneData matcount = playerV3AllyInfos[i];

                if (_gradeCount.ContainsKey(matcount.Grade) == false)
                    _gradeCount.Add(matcount.Grade, 0);

                _gradeCount[matcount.Grade] += Managers.SynergyRuneManager.Instance.GetDisplayEquipRuneCount(matcount);
            }

            for (int i = 0; i < V2Enum_Grade.Max.Enum32ToInt(); ++i)
            {
                V2Enum_Grade grade = i.IntToEnum32<V2Enum_Grade>();
                if (_gradeCount.ContainsKey(grade) == false)
                    continue;

                int matcount = _gradeCount[grade];

                SynergyRuneCombineData synergyRuneCombineData = Managers.SynergyRuneManager.Instance.GetSynergyRuneCombineData(grade);
                if (synergyRuneCombineData == null)
                {
                    _gradeCount[grade] = 0;
                    continue;
                }

                _gradeCount[grade] = matcount - (matcount % synergyRuneCombineData.RequiredCount);
            }

            playerV3AllyInfos.Sort(Managers.SynergyRuneManager.Instance.SortRuneData);

            playerV3AllyInfos.Reverse();

            for (int i = 0; i < playerV3AllyInfos.Count; ++i)
            {
                int materialcount = Managers.SynergyRuneManager.Instance.GetDisplayEquipRuneCount(playerV3AllyInfos[i]);

                SynergyRuneData material = playerV3AllyInfos[i];

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

            SetRuneElement();

            _gradeCount.Clear();
        }
        //------------------------------------------------------------------------------------
        private bool AddMaterial(SynergyRuneData synergyRuneData)
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
        private void SetRuneElement(V2Enum_Grade v2Enum_Grade = V2Enum_Grade.Max)
        {
            if (_synergyRuneEffect != null)
            {
                List<SynergyRuneData> playerV3AllyInfos = Managers.SynergyRuneManager.Instance.GetAllSynergyRuneData();
                if (playerV3AllyInfos == null)
                    return;

                if (m_allyElementFilter.Count > 0)
                {
                    playerV3AllyInfos = playerV3AllyInfos.FindAll(
                        x => Managers.SynergyRuneManager.Instance.GetDisplayEquipRuneCount(x) > 0
                        && m_allyElementFilter.Contains(x.SynergyType) == true);
                }
                else
                {
                    playerV3AllyInfos = playerV3AllyInfos.FindAll(
                        x => Managers.SynergyRuneManager.Instance.GetDisplayEquipRuneCount(x) > 0);
                }

                if (v2Enum_Grade != V2Enum_Grade.Max)
                {
                    playerV3AllyInfos = playerV3AllyInfos.FindAll(
                    x => x.Grade == v2Enum_Grade);
                }
                

                playerV3AllyInfos.Sort(Managers.SynergyRuneManager.Instance.SortRuneData);

                _synergyRuneEffect.Clear();

                for (int i = 0; i < playerV3AllyInfos.Count; ++i)
                {
                    _synergyRuneEffect.InsertData(playerV3AllyInfos[i]);
                    _synergyRuneEffect.UpdateAllData();
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
            V2Enum_ARR_SynergyType v2Enum_ElementType = element.IntToEnum32<V2Enum_ARR_SynergyType>();

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
            if (_contentMode == ContentDetailList.LobbySynergyRune_Combine && _playedCombineDirection == true)
                return;

            SynergyRuneData playerCharacterInfo = infiniteScrollData as SynergyRuneData;
            if (playerCharacterInfo == null)
                return;

            Click_Rune(playerCharacterInfo);
        }
        //------------------------------------------------------------------------------------
        private void Click_Rune(SynergyRuneData playerCharacterInfo)
        {
            if (_contentMode == ContentDetailList.LobbySynergyRune_Combine)
            {
                AddMaterial(playerCharacterInfo);
                SetRuneElement(playerCharacterInfo.Grade);

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

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.RuneTutorial)
            {
                if (_guideBG5 != null)
                    _guideBG5.gameObject.SetActive(true);

                Managers.GuideInteractorManager.Instance.SetGuideStep(5);

                if (_guideBG4 != null)
                    _guideBG4.gameObject.SetActive(false);

                Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/rune4"));
            }
        }
        //------------------------------------------------------------------------------------
    }
}