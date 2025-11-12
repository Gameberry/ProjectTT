using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.UI;

namespace GameBerry.Managers
{
    public enum GuideInteratorActionType
    { 
        None = 0,
        Prev,
        Next,
        End,
        GoToFirst,
        GoToJump,
    }

    public class GuideInteractorManager : MonoSingleton<GuideInteractorManager>
    {
        // GuideInteraction
        private Dictionary<V2Enum_EventType, List<UIGuideInteractor>> m_guideInteractors = new Dictionary<V2Enum_EventType, List<UIGuideInteractor>>();
        private UIGuideInteractor m_currentFocusGuideInteractor = null;
        private Transform m_guideFocusInteration = null;
        private UIGuideQuestClicker m_uIGuideQuestClicker = null;
        // GuideInteraction

        [SerializeField]
        private V2Enum_EventType _currentGuideEvent = V2Enum_EventType.Max;

        public Dictionary<V2Enum_EventType, float> _lastGuidePlayTime = new Dictionary<V2Enum_EventType, float>();

        private Event.SetGuideInteractorDialogMsg m_setGuideInteractorDialogMsg = new Event.SetGuideInteractorDialogMsg();
        private Event.VisibleGuideInteractorFocusMsg m_visibleGuideInteractorFocusMsg = new Event.VisibleGuideInteractorFocusMsg();

        public bool ShowNeedGearNotice = false;


        public bool PlaySynergyCombineTutorial = true;

        public bool NeedSynergyTutorial = false;
        public bool NeedSynergyOpenTutorial = false;
        public bool NeedSynergyUnLockTutorial = false;
        public bool NeedResearchTutorial = false;
        public bool NeedGearTutorial = false;
        public bool NeedGearEquipTutorial = false;
        public bool NeedRelicTutorial = false;
        public bool NeedSynergyBreakTutorial = false;
        public bool NeedRuneTutorial = false;
        public bool NeedDescendChangeTutorial = false;
        public bool NeedJobChangeTutorial = false;
        public bool NeedDungeonTutorial = false;


        public bool PlaySynergyTutorial = false;
        public bool PlaySynergyOpenTutorial = false;
        public bool PlaySynergyUnLockTutorial = false;
        public bool PlayResearchTutorial = false;
        public bool PlayGearTutorial = false;
        public bool PlayGearEquipTutorial = false;
        public bool PlayRelicTutorial = false;
        public bool PlaySynergyBreakTutorial = false;
        public bool PlayRuneTutorial = false;
        public bool PlayDescendChangeTutorial = false;
        public bool PlayJobChangeTutorial = false;
        public bool PlayDungeonTutorial = false;


        public bool PlayJokerTutorial = false;

        public bool PlayCardTutorial = false;
        public bool PlayGasSynergyTutorial = false;

        public bool PlayGameSpeedTutorial = false;

        public MapData CurrentMapData;

        public WaveClearRewardData CurrentWaveClearRewardData = null;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            //guide/Guide_hand00/sprite
            //guide/Guide_hand_E1_00/sprite
            ResourceLoader.Instance.Load<GameObject>("ContentResources/LobbyContent/UI/Objects/UI_QuestClick", o =>
            {
                GameObject obj = o as GameObject;

                GameObject clone = Instantiate(obj, transform);

                if (clone != null)
                {
                    m_guideFocusInteration = clone.transform;
                    m_uIGuideQuestClicker = clone.GetComponent<UIGuideQuestClicker>();
   
                    clone.SetActive(false);
                }
            });

            UnityUpdateManager.Instance.UpdateCoroutineFunc_1Sec += UpdateCoroutineFunc_1Sec;
        }
        //------------------------------------------------------------------------------------
        public void ConnecctGuideInteractor(UIGuideInteractor uIGuideInteractor)
        {
            if (uIGuideInteractor.MyGuideType == V2Enum_EventType.Max)
                return;

            if (uIGuideInteractor.MyStepID == -1)
                return;

            if (uIGuideInteractor.FocusOn == false)
                return;

            if (m_guideInteractors.ContainsKey(uIGuideInteractor.MyGuideType) == false)
                m_guideInteractors.Add(uIGuideInteractor.MyGuideType, new List<UIGuideInteractor>());

            List<UIGuideInteractor> uIGuideInteractors = m_guideInteractors[uIGuideInteractor.MyGuideType];

            uIGuideInteractors.Add(uIGuideInteractor);

            uIGuideInteractors.Sort((x, y) =>
            {
                if (x.MyStepID < y.MyStepID)
                {
                    return -1;
                }
                else if (x.MyStepID > y.MyStepID)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            });
        }
        //------------------------------------------------------------------------------------
        public void DisConnectGuideInteractor(UIGuideInteractor uIGuideInteractor)
        {
            if (uIGuideInteractor.MyGuideType == V2Enum_EventType.Max)
                return;

            if (uIGuideInteractor.MyStepID == -1)
                return;

            if (uIGuideInteractor.FocusOn == false)
                return;

            if (m_guideInteractors.ContainsKey(uIGuideInteractor.MyGuideType) == false)
                return;

            List<UIGuideInteractor> uIGuideInteractors = m_guideInteractors[uIGuideInteractor.MyGuideType];
            uIGuideInteractors.Remove(uIGuideInteractor);
        }
        //------------------------------------------------------------------------------------
        public V2Enum_EventType GetCurrentGuide()
        {
            if (m_currentFocusGuideInteractor == null)
                return V2Enum_EventType.Max;

            return m_currentFocusGuideInteractor.MyGuideType;
        }
        //------------------------------------------------------------------------------------
        public int GetCurrentStep()
        {
            if (m_currentFocusGuideInteractor == null)
                return -1;

            return m_currentFocusGuideInteractor.MyStepID;
        }
        //------------------------------------------------------------------------------------
        public void EndGuideQuest()
        {
            m_currentFocusGuideInteractor = null;
            HideFocus();
        }
        //------------------------------------------------------------------------------------
        public void PlayGuide(GuideQuestData guideQuestData)
        {
            if (guideQuestData == null)
                return;

            m_setGuideInteractorDialogMsg.guideQuestData = guideQuestData;
            Message.Send(m_setGuideInteractorDialogMsg);

            StartGuideInteractor(guideQuestData.QuestType);
        }
        //------------------------------------------------------------------------------------
        public void StartGuideInteractor(V2Enum_EventType V2Enum_EventType)
        {
            if (m_guideInteractors.ContainsKey(V2Enum_EventType) == false)
                return;

            List<UIGuideInteractor> uIGuideInteractors = m_guideInteractors[V2Enum_EventType];
            if (uIGuideInteractors.Count <= 0)
                return;

            m_currentFocusGuideInteractor = uIGuideInteractors[0];

            if (m_currentFocusGuideInteractor.IsEnableShow == true)
            {
                if (m_currentFocusGuideInteractor.gameObject.activeSelf == false
                    && uIGuideInteractors.Count > 1)
                {
                    OnClick_GuideInteractor(m_currentFocusGuideInteractor);
                    return;
                }
            }

            if (_lastGuidePlayTime.ContainsKey(V2Enum_EventType) == false)
                _lastGuidePlayTime.Add(V2Enum_EventType, 0.0f);

            _lastGuidePlayTime[V2Enum_EventType] = Time.time;

            _currentGuideEvent = V2Enum_EventType;

            SetGuideFocus();
        }
        //------------------------------------------------------------------------------------
        public void OnClick_GuideInteractor(UIGuideInteractor uIGuideInteractor)
        {
            if (uIGuideInteractor.MyGuideType != GetCurrentGuide())
                return;

            if (uIGuideInteractor.GuideInteratorActionType == GuideInteratorActionType.None)
                return;

            if (uIGuideInteractor.GuideInteratorActionType == GuideInteratorActionType.End)
            {
                m_currentFocusGuideInteractor = null;
                HideFocus();
            }

            if (m_guideInteractors.ContainsKey(uIGuideInteractor.MyGuideType) == false)
                return;

            List<UIGuideInteractor> uIGuideInteractors = m_guideInteractors[uIGuideInteractor.MyGuideType];
            if (uIGuideInteractors.Count <= 0)
                return;

            if (uIGuideInteractor.GuideInteratorActionType == GuideInteratorActionType.GoToFirst)
            {
                StartGuideInteractor(uIGuideInteractor.MyGuideType);
                return;
            }
            else if (uIGuideInteractor.GuideInteratorActionType == GuideInteratorActionType.GoToJump)
            {
                if (uIGuideInteractor.JumpIdx < uIGuideInteractors.Count)
                {
                    UIGuideInteractor jumpinteractor = uIGuideInteractors.Find(x => x.MyStepID == uIGuideInteractor.JumpIdx);
                    if (jumpinteractor != null)
                    {
                        m_currentFocusGuideInteractor = jumpinteractor;
                        SetGuideFocus();
                    }
                }
                return;
            }

            int idx = uIGuideInteractors.IndexOf(uIGuideInteractor);

            if (idx == -1)
                return;

            if (uIGuideInteractor.GuideInteratorActionType == GuideInteratorActionType.Prev)
            {
                idx--;

                if (idx < 0)
                    return;

                m_currentFocusGuideInteractor = uIGuideInteractors[idx];
                SetGuideFocus();
            }
            else if (uIGuideInteractor.GuideInteratorActionType == GuideInteratorActionType.Next)
            {
                idx++;
                if (idx >= uIGuideInteractors.Count)
                    return;

                m_currentFocusGuideInteractor = uIGuideInteractors[idx];
                SetGuideFocus();
            }
        }
        //------------------------------------------------------------------------------------
        public void SetNextGuideInteractor()
        {
            if (m_currentFocusGuideInteractor == null)
                return;

            List<UIGuideInteractor> uIGuideInteractors = m_guideInteractors[m_currentFocusGuideInteractor.MyGuideType];
            if (uIGuideInteractors.Count <= 0)
                return;

            int idx = uIGuideInteractors.IndexOf(m_currentFocusGuideInteractor);

            if (idx == -1)
                return;

            idx++;
            if (idx >= uIGuideInteractors.Count)
                return;

            m_currentFocusGuideInteractor = uIGuideInteractors[idx];
            SetGuideFocus();
        }
        //------------------------------------------------------------------------------------
        public void SetPrevGuideInteractor()
        {
            if (m_currentFocusGuideInteractor == null)
                return;

            List<UIGuideInteractor> uIGuideInteractors = m_guideInteractors[m_currentFocusGuideInteractor.MyGuideType];
            if (uIGuideInteractors.Count <= 0)
                return;

            int idx = uIGuideInteractors.IndexOf(m_currentFocusGuideInteractor);

            if (idx == -1)
                return;

            if (idx <= 0)
                return;

            idx--;

            m_currentFocusGuideInteractor = uIGuideInteractors[idx];
            SetGuideFocus();
        }
        //------------------------------------------------------------------------------------
        public void SetFirstGuideInteractor()
        {
            if (m_currentFocusGuideInteractor == null)
                return;
        }
        //------------------------------------------------------------------------------------
        public void SetGuideStep(UIGuideInteractor uIGuideInteractor)
        {
            if (uIGuideInteractor == null)
                return;

            if (GetCurrentGuide() != uIGuideInteractor.MyGuideType)
                return;

            m_currentFocusGuideInteractor = uIGuideInteractor;
            SetGuideFocus();
        }
        //------------------------------------------------------------------------------------
        public void SetGuideStep(int step)
        {
            if (GetCurrentGuide() == V2Enum_EventType.Max)
                return;

            List<UIGuideInteractor> uIGuideInteractors = m_guideInteractors[m_currentFocusGuideInteractor.MyGuideType];
            if (uIGuideInteractors.Count <= 0)
                return;

            UIGuideInteractor select = uIGuideInteractors.Find(x => x.MyStepID == step);

            if (select == null)
                return;

            m_currentFocusGuideInteractor = select;
            SetGuideFocus();
        }
        //------------------------------------------------------------------------------------
        private void SetGuideFocus()
        {
            if (m_currentFocusGuideInteractor != null && m_guideFocusInteration != null)
            {
                m_guideFocusInteration.gameObject.SetActive(true);
                m_guideFocusInteration.SetParent(m_currentFocusGuideInteractor.transform);
                m_guideFocusInteration.ResetLocal();

                if (m_currentFocusGuideInteractor.FocusParent != null)
                    m_guideFocusInteration.SetParent(m_currentFocusGuideInteractor.FocusParent);
                
                if (m_uIGuideQuestClicker != null)
                    m_uIGuideQuestClicker.SetHandAngle(m_currentFocusGuideInteractor.FocusAngle);
            }

            if (m_currentFocusGuideInteractor != null)
            {
                m_visibleGuideInteractorFocusMsg.visible = true;
                m_visibleGuideInteractorFocusMsg.uIGuideInteractor = m_currentFocusGuideInteractor;
                Message.Send(m_visibleGuideInteractorFocusMsg);
            }
        }
        //------------------------------------------------------------------------------------
        public void HideFocus()
        {
            m_visibleGuideInteractorFocusMsg.visible = false;
            m_visibleGuideInteractorFocusMsg.uIGuideInteractor = null;
            Message.Send(m_visibleGuideInteractorFocusMsg);

            if (m_guideFocusInteration != null)
                m_guideFocusInteration.gameObject.SetActive(false);

            if (_currentGuideEvent == V2Enum_EventType.SynergyChange)
            { 
                PlaySynergyTutorial = false;
                SetLastGuidePlayTime(V2Enum_EventType.StageClear, 0);
            }

            if (_currentGuideEvent == V2Enum_EventType.SynergyOpen)
                PlaySynergyOpenTutorial = false;

            if (_currentGuideEvent == V2Enum_EventType.SynergyUnLock)
                PlaySynergyUnLockTutorial = false;

            if (_currentGuideEvent == V2Enum_EventType.DescendChange)
                PlayDescendChangeTutorial = false;

            if (_currentGuideEvent == V2Enum_EventType.ResearchTutorial)
                PlayResearchTutorial = false;

            if (_currentGuideEvent == V2Enum_EventType.GearTutorial)
                PlayGearTutorial = false;

            if (_currentGuideEvent == V2Enum_EventType.GearEquipTutorial)
                PlayGearEquipTutorial = false;

            if (_currentGuideEvent == V2Enum_EventType.RelicTutorial)
                PlayRelicTutorial = false;

            if (_currentGuideEvent == V2Enum_EventType.SynergyBreak)
                PlaySynergyBreakTutorial = false;

            if (_currentGuideEvent == V2Enum_EventType.RuneTutorial)
                PlayRuneTutorial = false;

            if (_currentGuideEvent == V2Enum_EventType.Job)
                PlayJobChangeTutorial = false;

            if (_currentGuideEvent == V2Enum_EventType.Dungeon)
                PlayDungeonTutorial = false;

            _currentGuideEvent = V2Enum_EventType.Max;
        }
        //------------------------------------------------------------------------------------
        public void ShowCompleteFocus(UIGuideInteractor uIGuideInteractor)
        {
            m_visibleGuideInteractorFocusMsg.visible = true;
            m_visibleGuideInteractorFocusMsg.uIGuideInteractor = uIGuideInteractor;
            Message.Send(m_visibleGuideInteractorFocusMsg);
        }
        //------------------------------------------------------------------------------------
        public void ChangeBattleScene()
        { 

        }
        //------------------------------------------------------------------------------------
        public bool PlayedMainTutorial()
        {
            if (PlaySynergyTutorial == true
                || PlaySynergyOpenTutorial == true
                || PlaySynergyUnLockTutorial == true
                || PlayResearchTutorial == true
                || PlayRelicTutorial == true
                || PlaySynergyBreakTutorial == true
                || PlayRuneTutorial == true
                || PlayDescendChangeTutorial == true
                || PlayGearTutorial == true
                || PlayGearEquipTutorial == true
                || PlayJobChangeTutorial == true
                || PlayDungeonTutorial == true
                )
            {
                return true;
            }

            return false;
        }
        //------------------------------------------------------------------------------------
        public float lobbyinterctordelay = 10.0f;
        //------------------------------------------------------------------------------------
        private void UpdateCoroutineFunc_1Sec()
        {
            if (Managers.BattleSceneManager.Instance.BattleType == Enum_Dungeon.LobbyScene)
            {
                if (PlaySynergyTutorial == true)
                    return;

                if (PlaySynergyOpenTutorial == true)
                    return;

                if (PlaySynergyUnLockTutorial == true)
                    return;

                if (PlayResearchTutorial == true)
                    return;

                if (PlayRelicTutorial == true)
                    return;

                if (PlayGearTutorial == true)
                    return;

                if (PlayGearEquipTutorial == true)
                    return;

                if (PlaySynergyBreakTutorial == true)
                    return;

                if (PlayRuneTutorial == true)
                    return;

                if (PlayDescendChangeTutorial == true)
                    return;

                if (PlayJobChangeTutorial == true)
                    return;

                if (PlayDungeonTutorial == true)
                    return;

                if (NeedSynergyTutorial == true)
                {
                    NeedSynergyTutorial = false;
                    Message.Send(new Event.PlaySynergyChangeTutorialMsg());
                    return;
                }

                if (NeedSynergyOpenTutorial == true)
                {
                    NeedSynergyOpenTutorial = false;
                    Message.Send(new Event.PlaySynergyOpenTutorialMsg());
                    return;
                }

                if (NeedSynergyUnLockTutorial == true)
                {
                    NeedSynergyUnLockTutorial = false;
                    Message.Send(new Event.PlaySynergyUnLockTutorialMsg());
                    return;
                }

                if (NeedResearchTutorial == true)
                {
                    NeedResearchTutorial = false;
                    Message.Send(new Event.PlayResearchChangeTutorialMsg());
                    return;
                }

                if (NeedRelicTutorial == true)
                {
                    NeedRelicTutorial = false;
                    Message.Send(new Event.PlayRelicTutorialMsg());
                    return;
                }

                if (NeedGearTutorial == true)
                {
                    NeedGearTutorial = false;
                    Message.Send(new Event.PlayGearTutorialMsg());
                    return;
                }

                if (NeedGearEquipTutorial == true)
                {
                    NeedGearEquipTutorial = false;
                    Message.Send(new Event.PlayGearEquipTutorialMsg());
                    return;
                }
                

                if (NeedSynergyBreakTutorial == true)
                {
                    NeedSynergyBreakTutorial = false;
                    Message.Send(new Event.PlaySynergyBreakTutorialMsg());
                    return;
                }

                if (NeedRuneTutorial == true)
                {
                    NeedRuneTutorial = false;
                    Message.Send(new Event.PlayRuneTutorialMsg());
                    return;
                }

                if (NeedDescendChangeTutorial == true)
                {
                    NeedDescendChangeTutorial = false;
                    Message.Send(new Event.PlayDescendChangeTutorialMsg());
                    return;
                }

                if (NeedJobChangeTutorial == true)
                {
                    NeedJobChangeTutorial = false;
                    Message.Send(new Event.PlayJobTutorialMsg());
                    return;
                }

                if (NeedDungeonTutorial == true)
                {
                    NeedDungeonTutorial = false;
                    Message.Send(new Event.PlayDungeonTutorialMsg());
                    return;
                }

                if (_currentGuideEvent == V2Enum_EventType.Max)
                {
                    if (Managers.MapManager.Instance.NeedTutotial1() == true)
                    {
                        if (MapContainer.MapMaxClear == 0)
                            return;
                    }

                    //if (CanGuide(V2Enum_EventType.WaveReward) == true)
                    //{

                    //    WaveClearRewardData waveRewardData = Managers.MapManager.Instance.GetShowWaveClearRewardData();
                    //    if (waveRewardData != null)
                    //    {
                    //        bool ready = Managers.MapManager.Instance.IsReadyWaveClearReward(waveRewardData);
                    //        if (ready == true)
                    //        {
                    //            StartGuideInteractor(V2Enum_EventType.WaveReward);
                    //            return;
                    //        }
                    //    }
                    //}


                    //if (CanGuide(V2Enum_EventType.CharacterLevelUp) == true)
                    //{

                    //    int level = Managers.ARRRStatManager.Instance.GetCharacterLevel();

                    //    if (Managers.ARRRStatManager.Instance.NeedLimitBreak(level) == false)
                    //    {
                    //        if (Managers.ARRRStatManager.Instance.IsMaxLevel(level) == false)
                    //        {
                    //            CharacterLevelUpCostData characterLevelUpCostData = Managers.ARRRStatManager.Instance.GetCharacterLevelUpCostData(level);
                    //            if (characterLevelUpCostData != null)
                    //            {
                    //                bool readyCost = true;

                    //                for (int i = 0; i < characterLevelUpCostData.LevelUpCostGoods.Count; ++i)
                    //                {

                    //                    CharacterLevelUpCost characterLevelUpCost = characterLevelUpCostData.LevelUpCostGoods[i];
                    //                    if (Managers.ARRRStatManager.Instance.ReadyCharacterLevelUpCost(characterLevelUpCost, level) == false)
                    //                        readyCost = false;
                    //                }

                    //                if (readyCost == true)
                    //                {
                    //                    StartGuideInteractor(V2Enum_EventType.CharacterLevelUp);
                    //                    return;
                    //                }
                    //            }
                    //        }
                    //    }
                    //}

                    //if (MapManager.Instance.NeedTutotial1() == false)
                    //{
                    //    if (CanGuide(V2Enum_EventType.AllyEquip) == true)
                    //    {
                    //        if (PetManager.Instance.GetEquipPet() == null)
                    //        {
                    //            StartGuideInteractor(V2Enum_EventType.AllyEquip);
                    //            return;
                    //        }
                    //    }
                    //}

                    if (lobbyinterctordelay >= Time.time)
                        return;

                    if (CanGuide(V2Enum_EventType.WaveReward) == true)
                    {
                        if (CurrentWaveClearRewardData != null)
                        {
                            StartGuideInteractor(V2Enum_EventType.WaveReward);
                            return;
                        }
                    }

                    if (CanGuide(V2Enum_EventType.NextStage) == true)
                    {
                        if (CurrentMapData != null)
                        {
                            bool canNextStage = Managers.MapManager.Instance.GetNextChellengeData(CurrentMapData) != null;
                            if (canNextStage == true)
                            {
                                StartGuideInteractor(V2Enum_EventType.NextStage);
                                return;
                            }
                        }
                    }

                    if (CanGuide(V2Enum_EventType.SynergyInteraction) == true)
                    {
                        if (MapContainer.MapMaxClear >= 0 && MapContainer.MapMaxClear <= 2)
                        {
                            if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Synergy))
                            {
                                if (Managers.SynergyManager.Instance.RefreshSynergyRedDot() == true)
                                {
                                    StartGuideInteractor(V2Enum_EventType.SynergyInteraction);
                                    return;
                                }
                            }
                        }
                    }

                    if (CanGuide(V2Enum_EventType.StageClear) == true)
                    {
                        if (Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), Define.StaminaIndex) >= Define.RequiredStamina)
                            StartGuideInteractor(V2Enum_EventType.StageClear);
                    }
                }
                
            }
        }
        //------------------------------------------------------------------------------------
        private bool CanGuide(V2Enum_EventType v2Enum_EventType)
        {
            if (_lastGuidePlayTime.ContainsKey(v2Enum_EventType) == false)
                return true;

            if (_lastGuidePlayTime[v2Enum_EventType] + 30.0f < Time.time)
                return true;

            return false;
        }
        //------------------------------------------------------------------------------------
        public void SetLastGuidePlayTime(V2Enum_EventType v2Enum_EventType, float time)
        {
            if (_lastGuidePlayTime.ContainsKey(v2Enum_EventType) == false)
                _lastGuidePlayTime.Add(v2Enum_EventType, time);

            _lastGuidePlayTime[v2Enum_EventType] = time;
        }
        //------------------------------------------------------------------------------------
        public void RemoveLastGuidePlayTime(V2Enum_EventType v2Enum_EventType)
        {
            if (_lastGuidePlayTime.ContainsKey(v2Enum_EventType) == false)
                return;

            _lastGuidePlayTime.Remove(v2Enum_EventType);
        }
        //------------------------------------------------------------------------------------
    }
}