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
    public class LobbyCharacterContentDialog : IDialog
    {
        [Header("------------GuideJob------------")]
        [SerializeField]
        private Transform _guideJob2BG;

        [Header("-------SwitchTab-------")]
        [SerializeField]
        private CButton _gearTab;

        [SerializeField]
        private CButton _runeTab;

        [SerializeField]
        private CButton _jobTab;

        [Header("-------Dialogs-------")]
        [SerializeField]
        private LobbyGearContentDialog _lobbyGearContentDialog;

        [SerializeField]
        private LobbySynergyRuneContentDialog _lobbySynergyRuneContentDialog;

        [SerializeField]
        private LobbyCharacterJobContentDialog _lobbyCharacterJobContentDialog;

        private ContentDetailList _contentMode = ContentDetailList.None;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (_gearTab != null)
                _gearTab.onClick.AddListener(OnClick_ShowGear);

            if (_runeTab != null)
                _runeTab.onClick.AddListener(OnClick_ShowRune);

            if (_jobTab != null)
                _jobTab.onClick.AddListener(OnClick_ShowJob);

            if (_lobbyGearContentDialog != null)
            { 
                _lobbyGearContentDialog.Load_Element();
                _lobbyGearContentDialog.ElementExit();
            }

            if (_lobbySynergyRuneContentDialog != null)
            {
                _lobbySynergyRuneContentDialog.Load_Element();
                _lobbySynergyRuneContentDialog.ElementExit();
            }

            if (_lobbyCharacterJobContentDialog != null)
            {
                _lobbyCharacterJobContentDialog.Load_Element();
                _lobbyCharacterJobContentDialog.ElementExit();
            }

            _contentMode = ContentDetailList.LobbyGear;
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            if (Managers.GuideInteractorManager.Instance.PlayGearTutorial == true
                || Managers.GuideInteractorManager.Instance.PlayGearEquipTutorial == true)
            {
                _contentMode = ContentDetailList.LobbyGear;
            }

            if (Managers.GuideInteractorManager.Instance.PlayRuneTutorial == true)
            {
                _contentMode = ContentDetailList.LobbySynergyRune;
            }

            if (Managers.GuideInteractorManager.Instance.PlayJobChangeTutorial == true)
            {
                _contentMode = ContentDetailList.LobbyGear;

                if (_guideJob2BG != null)
                    _guideJob2BG.gameObject.SetActive(true);
            }

            SetContentMode(_contentMode);
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (_lobbyGearContentDialog != null)
                _lobbyGearContentDialog.ElementExit();

            if (_lobbySynergyRuneContentDialog != null)
                _lobbySynergyRuneContentDialog.ElementExit();

            if (_lobbyCharacterJobContentDialog != null)
                _lobbyCharacterJobContentDialog.ElementExit();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ShowGear()
        {
            SetContentMode(ContentDetailList.LobbyGear);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ShowRune()
        {
            SetContentMode(ContentDetailList.LobbySynergyRune);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ShowJob()
        {
            SetContentMode(ContentDetailList.LobbyCharacterJob);
        }
        //------------------------------------------------------------------------------------
        private void SetContentMode(ContentDetailList contentDetailList)
        {
            if (ContentDetailList.LobbyGear == contentDetailList)
            {
                if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Gear) == false)
                {
                    Managers.ContentOpenConditionManager.Instance.ShowOpenConditionNotice(V2Enum_ContentType.Gear);
                    return;
                }
            }
            else if (ContentDetailList.LobbySynergyRune == contentDetailList)
            {
                if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Rune) == false)
                {
                    Managers.ContentOpenConditionManager.Instance.ShowOpenConditionNotice(V2Enum_ContentType.Rune);
                    return;
                }
            }
            //else if (ContentDetailList.LobbyCharacterJob == contentDetailList)
            //{
            //    if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Job) == false)
            //    {
            //        Managers.ContentOpenConditionManager.Instance.ShowUnLockContentNotice(V2Enum_ContentType.Job);
            //        return;
            //    }
            //}

            if (Managers.GuideInteractorManager.Instance.PlayGearTutorial == true
                || Managers.GuideInteractorManager.Instance.PlayGearEquipTutorial == true)
            {
                if (contentDetailList != ContentDetailList.LobbyGear)
                    return;
            }

            if (Managers.GuideInteractorManager.Instance.PlayRuneTutorial == true)
            {
                if (contentDetailList != ContentDetailList.LobbySynergyRune)
                    return;
            }

            if (_gearTab != null)
                _gearTab.SetInteractable(contentDetailList != ContentDetailList.LobbyGear);

            if (_runeTab != null)
                _runeTab.SetInteractable(contentDetailList != ContentDetailList.LobbySynergyRune);

            if (_jobTab != null)
                _jobTab.SetInteractable(contentDetailList != ContentDetailList.LobbyCharacterJob);

            if (Managers.GuideInteractorManager.Instance.PlayJobChangeTutorial == true)
            {
                if (contentDetailList != ContentDetailList.LobbyCharacterJob)
                    Managers.GuideInteractorManager.Instance.SetGuideStep(2);
                else
                {
                    Managers.GuideInteractorManager.Instance.SetGuideStep(3);
                    
                    if (_guideJob2BG != null)
                        _guideJob2BG.gameObject.SetActive(false);
                }
            }

            if (contentDetailList == ContentDetailList.LobbyGear)
            {
                if (_lobbyGearContentDialog != null)
                    _lobbyGearContentDialog.ElementEnter();

                if (_lobbySynergyRuneContentDialog != null)
                    _lobbySynergyRuneContentDialog.ElementExit();

                if (_lobbyCharacterJobContentDialog != null)
                    _lobbyCharacterJobContentDialog.ElementExit();
            }
            else if (contentDetailList == ContentDetailList.LobbySynergyRune)
            {
                if (_lobbyGearContentDialog != null)
                    _lobbyGearContentDialog.ElementExit();

                if (_lobbySynergyRuneContentDialog != null)
                    _lobbySynergyRuneContentDialog.ElementEnter();

                if (_lobbyCharacterJobContentDialog != null)
                    _lobbyCharacterJobContentDialog.ElementExit();
            }
            else if (contentDetailList == ContentDetailList.LobbyCharacterJob)
            {
                if (_lobbyGearContentDialog != null)
                    _lobbyGearContentDialog.ElementExit();

                if (_lobbySynergyRuneContentDialog != null)
                    _lobbySynergyRuneContentDialog.ElementExit();

                if (_lobbyCharacterJobContentDialog != null)
                    _lobbyCharacterJobContentDialog.ElementEnter();
            }

            _contentMode = contentDetailList;
        }
        //------------------------------------------------------------------------------------
    }
}