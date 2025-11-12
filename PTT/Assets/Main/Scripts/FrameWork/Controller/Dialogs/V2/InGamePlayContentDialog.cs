using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

namespace GameBerry.UI
{
    [System.Serializable]
    public class EventPackageUI
    {
        public int PackageIndex;
        public Button PackageBtn;
        public TMP_Text PackageRemainTimeBtn;

        private System.Action<int> callback;

        [HideInInspector]
        public bool NeedTimerCheck = false;

        public void Init(System.Action<int> action)
        {
            PackageBtn?.onClick.AddListener(OnClick);
            callback = action;
        }

        private void OnClick()
        {
            callback?.Invoke(PackageIndex);
        }
    }

    [System.Serializable]
    public class ContentButtonData
    {
        public ContentDetailList MenuID = ContentDetailList.None;

        public Button btn;

        public TMP_Text text;

        public List<Transform> contents = new List<Transform>();

        public string localstring;

        public System.Action<ContentDetailList> CallBack;
        public void OnClick()
        {
            if (CallBack != null)
                CallBack(MenuID);
        }
    }

    public class InGamePlayContentDialog : IDialog
    {
        [Header("------------Stage1HideUI------------")]
        [SerializeField]
        private Transform _stage1HideUI;

        [Header("------------ReadyIdleReward------------")]
        [SerializeField]
        private Button m_readyIdleRewardBtn;

        [SerializeField]
        private TMP_Text m_idleRewardText;

        [SerializeField]
        private Transform m_idleRewardFull;

        [Header("------------Button------------")]
        [SerializeField]
        private List<ContentButtonData> m_contentButtonDatas = new List<ContentButtonData>();

        [Header("------------Stage------------")]
        [SerializeField]
        private Transform _stageGroup;

        [SerializeField]
        private Button _prevStage;

        [SerializeField]
        private List<Button> _nextStage;

        [SerializeField]
        private Button _stageEnter_Btn;

        [SerializeField]
        private Image _stageEnterStaminaIcon;

        [SerializeField]
        private TMP_Text _stageEnterStaminaPrice;

        [SerializeField]
        private TMP_Text _stageEnterNumber_Btn;

        private MapData _currentMapData;

        [Header("---------Sweep---------")]
        [SerializeField]
        private Button _stageSweep_Btn;

        [SerializeField]
        private Transform _adRemoveStageSweepComponent;

        [SerializeField]
        private TMP_Text _stageSweepCount;

        [SerializeField]
        private TMP_Text _sweepEnterStaminaPrice;

        [Header("---------Stamina---------")]
        [SerializeField]
        private TMP_Text _chargeStaminaRemainTime;

        [SerializeField]
        private TMP_Text _staminaAmount;

        [SerializeField]
        private Button _showStaminaShop;

        [Header("---------Synergy---------")]
        [SerializeField]
        private Button _synergyBtn;

        [SerializeField]
        private UIGuideInteractor _synergyChangeInteractor;

        [SerializeField]
        private Image _newSynergy;

        [SerializeField]
        private UIFarmingDirectionController _farmingDirectionController;

        [Header("---------Descend---------")]
        [SerializeField]
        private Button _descendBtn;

        [SerializeField]
        private UIGuideInteractor _descendChangeInteractor;

        [SerializeField]
        private Image _newDescend;


        [Header("---------Relic---------")]
        [SerializeField]
        private Button _relicBtn;

        [SerializeField]
        private UIGuideInteractor _relicChangeInteractor;

        [Header("---------Pass---------")]
        [SerializeField]
        private Button _passBtn;

        [Header("---------Shop---------")]
        [SerializeField]
        private Button _shopBtn;

        [Header("---------AdBuff---------")]
        [SerializeField]
        private Button _adBuffBtn;

        [Header("---------Quset---------")]
        [SerializeField]
        private Button _questBtn;

        [Header("---------TimeAttackMission---------")]
        [SerializeField]
        private Button _timeAttackMissionBtn;

        [SerializeField]
        private TMP_Text _timeAttackMission_RemainTime;

        [SerializeField]
        private Transform _timeAttackMission_RecvReady;

        [SerializeField]
        private Color _timeAttackMission_RemainTime_1HUnder;

        [SerializeField]
        private Color _timeAttackMission_RemainTime_1HOver;

        [Header("---------ShopPackageLimitTime---------")]
        [SerializeField]
        private Transform _uIShopElement_LimitTimeIcon_Root;

        [SerializeField]
        private List<UIShopLimitBanner> _customLimiticons = new List<UIShopLimitBanner>();

        [SerializeField]
        private UIShopElement_LimitTimeIcon _uIShopElement_LimitTimeIcon;
        private List<UIShopElement_LimitTimeIcon> eventPackageUIs = new List<UIShopElement_LimitTimeIcon>();

        [SerializeField]
        private Color remainTimeColor = Color.white;

        [SerializeField]
        private Color remainTimeColor_Noti = Color.white;

        private GameBerry.Event.SetWaveRewardDialogMsg _setWaveRewardDialogMsg = new GameBerry.Event.SetWaveRewardDialogMsg();

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            for (int i = 0; i < m_contentButtonDatas.Count; ++i)
            {
                if (m_contentButtonDatas[i] != null)
                {
                    m_contentButtonDatas[i].CallBack = OnClick_BottomBtn;
                    m_contentButtonDatas[i].btn.onClick.AddListener(m_contentButtonDatas[i].OnClick);
                }
            }

            if (m_readyIdleRewardBtn != null)
            {
                m_readyIdleRewardBtn.onClick.AddListener(() =>
                {
                    m_readyIdleRewardBtn.gameObject.SetActive(false);
                    Managers.TimeManager.Instance.DoIdleReward();

                    List<RewardData> rewards = Managers.TimeManager.Instance.GetStageCoolTimeRewardDatas();

                    for (int i = 0; i < rewards.Count; ++i)
                    {
                        Managers.GoodsDropDirectionManager.Instance.ShowDropIn(rewards[i].V2Enum_Goods, rewards[i].Index, m_readyIdleRewardBtn.transform.position, rewards[i].Amount.GetDecrypted().ToInt());
                    }
                });

                m_readyIdleRewardBtn.gameObject.SetActive(false);
            }

            if (_stageEnter_Btn != null)
            {
                _stageEnter_Btn.onClick.AddListener(OnClick_StageEnter);
            }

            if (_stageSweep_Btn != null)
                _stageSweep_Btn.onClick.AddListener(Managers.MapManager.Instance.GetStageSweepReward);

            if (_prevStage != null)
                _prevStage.onClick.AddListener(OnClick_PrevMap);


            if (_nextStage != null)
            {
                for (int i = 0; i < _nextStage.Count; ++i)
                    _nextStage[i].onClick.AddListener(OnClick_NextMap);
            }

            if (_stageEnterStaminaIcon != null)
                _stageEnterStaminaIcon.sprite = Managers.GoodsManager.Instance.GetGoodsSprite(V2Enum_Goods.Point.Enum32ToInt(), Define.StaminaIndex);

            if (_stageEnterStaminaPrice != null)
                _stageEnterStaminaPrice.SetText("-{0}", Define.RequiredStamina);

            if (_sweepEnterStaminaPrice != null)
                _sweepEnterStaminaPrice.SetText("-{0}", Define.RequiredStamina);

            if (_showStaminaShop != null)
                _showStaminaShop.onClick.AddListener(() =>
                {
                    RequestDialogEnter<LobbyStaminaShopDialog>();
                });




            List<ShopPackageEventData> ShopPackageSpecialDatas = Managers.ShopManager.Instance.GetPackageEventDatas();

            for (int i = 0; i < ShopPackageSpecialDatas.Count; ++i)
            {
                ShopPackageEventData shopPackageSpecialData = ShopPackageSpecialDatas[i];
                if (shopPackageSpecialData == null)
                    continue;

                GameObject clone = null;

                UIShopLimitBanner clonetrans = _customLimiticons.Find(x => x.Index == shopPackageSpecialData.IconResourceIndex.GetDecrypted());
                if (clonetrans != null && clonetrans.Banner != null)
                    clone = Instantiate(clonetrans.Banner.gameObject, _uIShopElement_LimitTimeIcon_Root);
                else
                    clone = Instantiate(_uIShopElement_LimitTimeIcon.gameObject, _uIShopElement_LimitTimeIcon_Root);

                if (clone == null)
                    continue;

                clone.gameObject.SetActive(false);

                UIShopElement_LimitTimeIcon uIShopElement_LimitTimeIcon = clone.GetComponent<UIShopElement_LimitTimeIcon>();
                if (uIShopElement_LimitTimeIcon == null)
                    continue;

                uIShopElement_LimitTimeIcon.Init(OnClick_EventPackageBtn);
                uIShopElement_LimitTimeIcon.SetLimitTimeData(shopPackageSpecialData);

                eventPackageUIs.Add(uIShopElement_LimitTimeIcon);


                PlayerShopInfo playerShopInfo = Managers.ShopManager.Instance.GetPlayerShopInfo(shopPackageSpecialData);
                if (playerShopInfo == null)
                    continue;

                
                UIShopElement_LimitTimeIcon eventPackageUI = uIShopElement_LimitTimeIcon;
                if (eventPackageUI == null)
                    continue;

                if (Managers.ShopManager.Instance.IsSoldOut(shopPackageSpecialData) == false)
                {
                    eventPackageUI.NeedTimerCheck = true;
                    SetEventPackageUIEventTime(eventPackageUI);
                }
                else
                {
                    eventPackageUI.PackageBtn.gameObject.SetActive(false);
                    eventPackageUI.NeedTimerCheck = false;
                }
            }

            Message.AddListener<GameBerry.Event.RefreshTimeAttackMissionMsg>(RefreshTimeAttackMission);
            Message.AddListener<GameBerry.Event.HideTimeAttackMissionIconMsg>(HideTimeAttackMissionIcon);
            Message.AddListener<GameBerry.Event.ChangeNewChellengeMapMsg>(ChangeNewChellengeMap);
            Message.AddListener<GameBerry.Event.RefreshLobbyMapSelecterMapMsg>(RefreshLobbyMapSelecterMap);

            Message.AddListener<GameBerry.Event.ShowNewSynergyMsg>(ShowNewSynergy);
            Message.AddListener<GameBerry.Event.NoticeNewLobbySynergyElementMsg>(NoticeNewLobbySynergyElement);
            Message.AddListener<GameBerry.Event.PlaySynergyChangeTutorialMsg>(PlaySynergyChangeTutorial);

            Message.AddListener<GameBerry.Event.ShowNewRelicMsg>(ShowNewRelic);

            Message.AddListener<GameBerry.Event.ShowNewDescendMsg>(ShowNewDescend);

            Message.AddListener<GameBerry.Event.PlaySynergyOpenTutorialMsg>(PlaySynergyOpenTutorial);
            Message.AddListener<GameBerry.Event.PlaySynergyUnLockTutorialMsg>(PlaySynergyUnLockTutorial);
            Message.AddListener<GameBerry.Event.PlayResearchChangeTutorialMsg>(PlayResearchChangeTutorial);
            Message.AddListener<GameBerry.Event.PlayRelicTutorialMsg>(PlayRelicTutorial);
            Message.AddListener<GameBerry.Event.PlayGearTutorialMsg>(PlayGearTutorial);
            Message.AddListener<GameBerry.Event.PlayGearEquipTutorialMsg>(PlayGearEquipTutorial);
            Message.AddListener<GameBerry.Event.PlaySynergyBreakTutorialMsg>(PlaySynergyBreakTutorial);
            Message.AddListener<GameBerry.Event.PlayRuneTutorialMsg>(PlayRuneTutorial);
            Message.AddListener<GameBerry.Event.PlayDescendChangeTutorialMsg>(PlayDescendChangeTutorial);
            Message.AddListener<GameBerry.Event.PlayJobTutorialMsg>(PlayJobTutorial);
            Message.AddListener<GameBerry.Event.PlayDungeonTutorialMsg>(PlayDungeonTutorial);

            Message.AddListener<GameBerry.Event.RefreshShopEventMsg>(RefreshShopEvent);
            Message.AddListener<GameBerry.Event.RefreshStageSweepMsg>(RefreshStageSweep);

            

            Managers.StaminaManager.Instance.RechargeTime += SetStaminaChargeRemainTime;
            Managers.GoodsManager.Instance.AddGoodsRefreshEvent(V2Enum_Goods.Point, Define.StaminaIndex, RefreshStamina);

            Managers.TimeAttackMissionManager.Instance.OnEndEventTime += RefreshTimeAttackMissionRemainTime;

            RefreshStageSweep(null);


            if (Define.IsAdFree == false)
            {
                Managers.VipPackageManager.Instance.changeAdFreeMode += () =>
                {
                    RefreshStageSweep(null);
                };
            }

            Managers.UnityUpdateManager.Instance.UpdateCoroutineFunc_1Sec += Update1Sec;
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.RefreshTimeAttackMissionMsg>(RefreshTimeAttackMission);
            Message.RemoveListener<GameBerry.Event.HideTimeAttackMissionIconMsg>(HideTimeAttackMissionIcon);
            Message.RemoveListener<GameBerry.Event.ChangeNewChellengeMapMsg>(ChangeNewChellengeMap);
            Message.RemoveListener<GameBerry.Event.RefreshLobbyMapSelecterMapMsg>(RefreshLobbyMapSelecterMap);

            Message.RemoveListener<GameBerry.Event.ShowNewSynergyMsg>(ShowNewSynergy);
            Message.RemoveListener<GameBerry.Event.PlaySynergyOpenTutorialMsg>(PlaySynergyOpenTutorial);
            Message.RemoveListener<GameBerry.Event.PlaySynergyUnLockTutorialMsg>(PlaySynergyUnLockTutorial);
            Message.RemoveListener<GameBerry.Event.PlayResearchChangeTutorialMsg>(PlayResearchChangeTutorial);
            Message.RemoveListener<GameBerry.Event.NoticeNewLobbySynergyElementMsg>(NoticeNewLobbySynergyElement);
            Message.RemoveListener<GameBerry.Event.PlaySynergyChangeTutorialMsg>(PlaySynergyChangeTutorial);

            Message.RemoveListener<GameBerry.Event.ShowNewRelicMsg>(ShowNewRelic);

            Message.RemoveListener<GameBerry.Event.ShowNewDescendMsg>(ShowNewDescend);
            Message.RemoveListener<GameBerry.Event.PlayRelicTutorialMsg>(PlayRelicTutorial);
            Message.RemoveListener<GameBerry.Event.PlayGearTutorialMsg>(PlayGearTutorial);
            Message.RemoveListener<GameBerry.Event.PlayGearEquipTutorialMsg>(PlayGearEquipTutorial);
            Message.RemoveListener<GameBerry.Event.PlaySynergyBreakTutorialMsg>(PlaySynergyBreakTutorial);
            Message.RemoveListener<GameBerry.Event.PlayRuneTutorialMsg>(PlayRuneTutorial);
            Message.RemoveListener<GameBerry.Event.PlayDescendChangeTutorialMsg>(PlayDescendChangeTutorial);
            Message.RemoveListener<GameBerry.Event.PlayJobTutorialMsg>(PlayJobTutorial);
            Message.RemoveListener<GameBerry.Event.PlayDungeonTutorialMsg>(PlayDungeonTutorial);

            Message.RemoveListener<GameBerry.Event.RefreshShopEventMsg>(RefreshShopEvent);
            Message.RemoveListener<GameBerry.Event.RefreshStageSweepMsg>(RefreshStageSweep);

            Managers.GoodsManager.Instance.RemoveGoodsRefreshEvent(V2Enum_Goods.Point, Define.StaminaIndex, RefreshStamina);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            if (_currentMapData == null)
                _currentMapData = Managers.MapManager.Instance.GetLobbyDisPlayMapData();

            SetMapUI(_currentMapData);

            //if (_waveRewardGroup != null)
            //    _waveRewardGroup.gameObject.SetActive(Managers.MapManager.Instance.NeedTutotial1() == false);

            //StageInfo stageInfo = Managers.MapManager.Instance.GetStageInfo(Define.SynergyTutorialStage);
            //bool finishedSynergyTutorial = false;
            //if (stageInfo != null)
            //{
            //    if (stageInfo.RecvClearReward >= Define.SynergyTutorialWave)
            //        finishedSynergyTutorial = true;
            //}

            //if (_synergyBtn != null)
            //    _synergyBtn.gameObject.SetActive(finishedSynergyTutorial);



            //stageInfo = Managers.MapManager.Instance.GetStageInfo(Define.RelicTutorialStage);
            //bool finishedRelicTutorial = false;
            //if (stageInfo != null)
            //{
            //    if (stageInfo.RecvClearReward >= Define.RelicTutorialWave)
            //        finishedRelicTutorial = true;
            //}

            //if (_relicBtn != null)
            //    _relicBtn.gameObject.SetActive(finishedRelicTutorial);

            //if (_shopBtn != null)
            //    _shopBtn.gameObject.SetActive(finishedRelicTutorial);

            //if (_passBtn != null)
            //    _passBtn.gameObject.SetActive(finishedRelicTutorial);



            //stageInfo = Managers.MapManager.Instance.GetStageInfo(Define.DescendTutorialStage);
            //bool finishedDescendTutorial = false;
            //if (stageInfo != null)
            //{
            //    if (stageInfo.RecvClearReward >= Define.DescendTutorialWave)
            //        finishedDescendTutorial = true;
            //}

            //if (_descendBtn != null)
            //    _descendBtn.gameObject.SetActive(finishedDescendTutorial);

            //if (_adBuffBtn != null)
            //    _adBuffBtn.gameObject.SetActive(finishedDescendTutorial);

            //if (_questBtn != null)
            //    _questBtn.gameObject.SetActive(finishedDescendTutorial);




            if (_timeAttackMissionBtn != null)
            {
                Managers.TimeAttackMissionManager.Instance.RefreshFocusTimeMission();
                TimeAttackMissionData timeAttackMissionData = Managers.TimeAttackMissionManager.Instance.GetFocusTimeMission();
                _timeAttackMissionBtn.gameObject.SetActive(timeAttackMissionData != null);

                if (timeAttackMissionData != null)
                {
                    RefreshTimeAttackMission(null);
                    RefreshTimeAttackMissionRemainTime(Managers.TimeAttackMissionManager.Instance.GetRemainTime());
                }
            }
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (_farmingDirectionController != null)
                _farmingDirectionController.ForceRelease();
        }
        //------------------------------------------------------------------------------------
        private void Update1Sec()
        {
            for (int i = 0; i < eventPackageUIs.Count; ++i)
            {
                if (eventPackageUIs[i].NeedTimerCheck == true)
                {
                    SetEventPackageUIEventTime(eventPackageUIs[i]);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshTimeAttackMission(GameBerry.Event.RefreshTimeAttackMissionMsg msg)
        {
            TimeAttackMissionData timeAttackMissionData = Managers.TimeAttackMissionManager.Instance.GetFocusTimeMission();
            if (timeAttackMissionData == null)
                return;

            if (_timeAttackMission_RecvReady != null)
                _timeAttackMission_RecvReady.gameObject.SetActive(Managers.TimeAttackMissionManager.Instance.IsRecvReady(timeAttackMissionData));
        }
        //------------------------------------------------------------------------------------
        private void HideTimeAttackMissionIcon(GameBerry.Event.HideTimeAttackMissionIconMsg msg)
        {
            if (_timeAttackMissionBtn != null)
                _timeAttackMissionBtn.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        private void RefreshTimeAttackMissionRemainTime(double timestamp)
        {
            if (timestamp <= 0)
            {
                if (_timeAttackMissionBtn != null)
                    _timeAttackMissionBtn.gameObject.SetActive(false);

                return;
            }

            if (_timeAttackMissionBtn != null)
                _timeAttackMissionBtn.gameObject.SetActive(true);

            if (_timeAttackMission_RemainTime != null)
            {

                System.TimeSpan dateTime = new System.TimeSpan(0, 0, (int)timestamp);

                if (dateTime.Days > 0)
                {
                    string day = string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.DayLocalKey), dateTime.Days);
                    string hour = string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.HourLocalKey), dateTime.Hours);

                    _timeAttackMission_RemainTime.text = string.Format("{0} {1}", day, hour);
                    _timeAttackMission_RemainTime.color = _timeAttackMission_RemainTime_1HOver;
                }
                else if (dateTime.Hours > 0)
                {
                    string hour = string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.HourLocalKey), dateTime.Hours);
                    string minute = string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.MinuteLocalKey), dateTime.Minutes);

                    _timeAttackMission_RemainTime.text = string.Format("{0} {1}", hour, minute);
                    _timeAttackMission_RemainTime.color = _timeAttackMission_RemainTime_1HOver;
                }
                else
                {
                    string minute = string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.MinuteLocalKey), dateTime.Minutes);
                    string second = string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.SecondLocalKey), dateTime.Seconds);

                    _timeAttackMission_RemainTime.text = string.Format("{0} {1}", minute, second);
                    _timeAttackMission_RemainTime.color = _timeAttackMission_RemainTime_1HUnder;
                }
            }
            
        }
        //------------------------------------------------------------------------------------
        private void RefreshLobbyMapSelecterMap(GameBerry.Event.RefreshLobbyMapSelecterMapMsg msg)
        {
            SetMapUI(_currentMapData);
        }
        //------------------------------------------------------------------------------------
        private void ChangeNewChellengeMap(GameBerry.Event.ChangeNewChellengeMapMsg msg)
        {
            //_currentMapData = Managers.MapManager.Instance.GetNextClearMap();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_StageEnter()
        {
            if (Managers.GuideInteractorManager.Instance.PlayedMainTutorial() == true)
            {
                Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("notice/needcompletetutorial"));
                return;
            }

            if (Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), Define.StaminaIndex) < Define.RequiredStamina)
            {
                RequestDialogEnter<LobbyStaminaShopDialog>();
                return;
            }


            if (_currentMapData == null)
            {
                _currentMapData = Managers.MapManager.Instance.GetNextClearMap();
                return;
            }

            if (Managers.MapManager.Instance.GetCanLastClearReward(_currentMapData.StageNumber) != null)
            {
                Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("notice/recvwavereward"));
                return;
            }


            if (_currentMapData.StageNumber == MapContainer.MapMaxClear)
            {
                if (_currentMapData.StageNumber > 0)
                {
                    StageInfo stageInfo = Managers.MapManager.Instance.GetLastChellengeInfo();

                    if (stageInfo != null)
                    {
                        if (stageInfo.StageNumber == MapContainer.MapMaxClear)
                            return;
                    }
                }
                
            }

            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            Managers.MapManager.Instance.SetMapLastEnter(_currentMapData.StageNumber);
            Managers.BattleSceneManager.Instance.ChangeBattleScene(Enum_Dungeon.StageScene);
        }
        //------------------------------------------------------------------------------------
        private void ShowNewSynergy(GameBerry.Event.ShowNewSynergyMsg msg)
        {
            if (_newSynergy != null)
                _newSynergy.gameObject.SetActive(Managers.SynergyManager.Instance.GetNewSynergyIconCount() > 0);
        }
        //------------------------------------------------------------------------------------
        private void ShowNewRelic(GameBerry.Event.ShowNewRelicMsg msg)
        {
            if (Managers.ContentOpenConditionManager.isAlive == true
                && Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Relic) == false)
                return;

            //if (_newRelic != null)
            //    _newRelic.gameObject.SetActive(Managers.RelicManager.Instance.GetNewSynergyIconCount() > 0);

            if (msg.NewSynergyEffectData != null)
            {
                if (_farmingDirectionController != null)
                {
                    _farmingDirectionController.ShowFarmingReward(Managers.RelicManager.Instance.GetRelicIcon(msg.NewSynergyEffectData), Managers.LocalStringManager.Instance.GetLocalString("ui/newsynergynotice"));
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void ShowNewDescend(GameBerry.Event.ShowNewDescendMsg msg)
        {
            if (_newDescend != null)
                _newDescend.gameObject.SetActive(Managers.DescendManager.Instance.GetNewSynergyIconCount() > 0);

            if (msg.NewSynergyEffectData != null)
            {
                if (_farmingDirectionController != null)
                {
                    _farmingDirectionController.ShowFarmingReward(Managers.DescendManager.Instance.GetDescendIcon(msg.NewSynergyEffectData), Managers.LocalStringManager.Instance.GetLocalString("ui/newsynergynotice"));
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void NoticeNewLobbySynergyElement(GameBerry.Event.NoticeNewLobbySynergyElementMsg msg)
        {
            if (msg.SynergyEffectData == null)
                return;

            if (_farmingDirectionController != null)
            {
                _farmingDirectionController.ShowFarmingReward(Managers.SynergyManager.Instance.GetSynergySprite(msg.SynergyEffectData), Managers.LocalStringManager.Instance.GetLocalString("ui/newsynergynotice"));
            }
        }
        //------------------------------------------------------------------------------------
        private void PlaySynergyChangeTutorial(GameBerry.Event.PlaySynergyChangeTutorialMsg msg)
        {
            if (_synergyChangeInteractor != null)
                _synergyChangeInteractor.ConnectInteractor();

            Managers.GuideInteractorManager.Instance.PlaySynergyTutorial = true;
            Managers.GuideInteractorManager.Instance.StartGuideInteractor(V2Enum_EventType.SynergyChange);
        }
        //------------------------------------------------------------------------------------
        private void PlaySynergyOpenTutorial(GameBerry.Event.PlaySynergyOpenTutorialMsg msg)
        {
            Managers.GuideInteractorManager.Instance.PlaySynergyOpenTutorial = true;
            Managers.GuideInteractorManager.Instance.StartGuideInteractor(V2Enum_EventType.SynergyOpen);
            Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/synergyopen1"));
        }
        //------------------------------------------------------------------------------------
        private void PlaySynergyUnLockTutorial(GameBerry.Event.PlaySynergyUnLockTutorialMsg msg)
        {
            Managers.GuideInteractorManager.Instance.PlaySynergyUnLockTutorial = true;
            Managers.GuideInteractorManager.Instance.StartGuideInteractor(V2Enum_EventType.SynergyUnLock);
            Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/synergyunlock1"));
        }
        //------------------------------------------------------------------------------------
        private void PlayResearchChangeTutorial(GameBerry.Event.PlayResearchChangeTutorialMsg msg)
        {
            Managers.GuideInteractorManager.Instance.PlayResearchTutorial = true;
            Managers.GuideInteractorManager.Instance.StartGuideInteractor(V2Enum_EventType.ResearchTutorial);
            Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/research1"));
        }
        //------------------------------------------------------------------------------------
        private void PlayRelicTutorial(GameBerry.Event.PlayRelicTutorialMsg msg)
        {
            Managers.GuideInteractorManager.Instance.PlayRelicTutorial = true;
            Managers.GuideInteractorManager.Instance.StartGuideInteractor(V2Enum_EventType.RelicTutorial);
            Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/relic1"));
        }
        //------------------------------------------------------------------------------------
        private void PlayGearTutorial(GameBerry.Event.PlayGearTutorialMsg msg)
        {
            Managers.GuideInteractorManager.Instance.PlayGearTutorial = true;
            Managers.GuideInteractorManager.Instance.StartGuideInteractor(V2Enum_EventType.GearTutorial);
            Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/gear1"));
        }
        //------------------------------------------------------------------------------------
        private void PlayGearEquipTutorial(GameBerry.Event.PlayGearEquipTutorialMsg msg)
        {
            Managers.GuideInteractorManager.Instance.PlayGearEquipTutorial = true;
            Managers.GuideInteractorManager.Instance.StartGuideInteractor(V2Enum_EventType.GearEquipTutorial);
            Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/gear4"));
        }
        //------------------------------------------------------------------------------------
        private void PlaySynergyBreakTutorial(GameBerry.Event.PlaySynergyBreakTutorialMsg msg)
        {
            Managers.GuideInteractorManager.Instance.PlaySynergyBreakTutorial = true;
            Managers.GuideInteractorManager.Instance.StartGuideInteractor(V2Enum_EventType.SynergyBreak);
            Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/skillbreak1"));
        }
        //------------------------------------------------------------------------------------
        private void PlayRuneTutorial(GameBerry.Event.PlayRuneTutorialMsg msg)
        {
            Managers.GuideInteractorManager.Instance.PlayRuneTutorial = true;
            Managers.GuideInteractorManager.Instance.StartGuideInteractor(V2Enum_EventType.RuneTutorial);
            Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/rune1"));
        }
        //------------------------------------------------------------------------------------
        private void PlayDescendChangeTutorial(GameBerry.Event.PlayDescendChangeTutorialMsg msg)
        {

            if (_descendChangeInteractor != null)
                _descendChangeInteractor.ConnectInteractor();

            Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/descendskill1"));

            Managers.GuideInteractorManager.Instance.PlayDescendChangeTutorial = true;
            Managers.GuideInteractorManager.Instance.StartGuideInteractor(V2Enum_EventType.DescendChange);
        }
        //------------------------------------------------------------------------------------
        private void PlayJobTutorial(GameBerry.Event.PlayJobTutorialMsg msg)
        {
            Managers.GuideInteractorManager.Instance.PlayJobChangeTutorial = true;
            Managers.GuideInteractorManager.Instance.StartGuideInteractor(V2Enum_EventType.Job);
            Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/job1"));
        }
        //------------------------------------------------------------------------------------
        private void PlayDungeonTutorial(GameBerry.Event.PlayDungeonTutorialMsg msg)
        {
            Managers.GuideInteractorManager.Instance.PlayDungeonTutorial = true;
            Managers.GuideInteractorManager.Instance.StartGuideInteractor(V2Enum_EventType.Dungeon);
            Contents.GlobalContent.ShowGlobalNotice_Guide(Managers.LocalStringManager.Instance.GetLocalString("guide/dungeon1"));
        }
        //------------------------------------------------------------------------------------
        private void SetMapUI(MapData mapData)
        {
            if (mapData == null)
                return;

            if (_currentMapData != null && _currentMapData.StageNumber == 0 && mapData.StageNumber != 0)
            {
                RequestDialogExit<CharacterInfoDialog>();
                RequestDialogEnter<CharacterInfoDialog>();
            }

            _currentMapData = mapData;

            Managers.BattleSceneManager.Instance.SetBGIndex(_currentMapData.BackGround);

            Managers.GuideInteractorManager.Instance.CurrentMapData = _currentMapData;

            if (_synergyBtn != null)
                _synergyBtn.gameObject.SetActive(mapData.StageNumber > 0);

            if (_relicBtn != null)
                _relicBtn.gameObject.SetActive(mapData.StageNumber > 0);

            if (_descendBtn != null)
                _descendBtn.gameObject.SetActive(mapData.StageNumber > 0);

            //if (_shopBtn != null)
            //    _shopBtn.gameObject.SetActive(mapData.StageNumber > 0);

            //if (_passBtn != null)
            //    _passBtn.gameObject.SetActive(mapData.StageNumber > 0);

            //if (_adBuffBtn != null)
            //    _adBuffBtn.gameObject.SetActive(mapData.StageNumber > 0);

            //if (_questBtn != null)
            //    _questBtn.gameObject.SetActive(mapData.StageNumber > 0);


            if (_stageEnterNumber_Btn != null)
            {
                if (mapData.StageNumber == 0)
                    _stageEnterNumber_Btn.text = Managers.LocalStringManager.Instance.GetLocalString("stage/wavetutorialreward");
                else
                    _stageEnterNumber_Btn.text = string.Format(Managers.LocalStringManager.Instance.GetLocalString("dungeon/step"), mapData.StageNumber);
            }

            if (_prevStage != null)
                _prevStage.gameObject.SetActive(Managers.MapManager.Instance.GetPrevChellengeData(mapData) != null);

            bool canNextStage = Managers.MapManager.Instance.GetNextChellengeData(mapData) != null;

            if (_nextStage != null)
            {
                for (int i = 0; i < _nextStage.Count; ++i)
                    _nextStage[i].gameObject.SetActive(canNextStage);
            }

            if (_currentMapData != null)
            {
                _setWaveRewardDialogMsg.StageNumber = _currentMapData.StageNumber;
                Message.Send(_setWaveRewardDialogMsg);
            }


            if (mapData.StageNumber != 0)
            {
                WaveClearRewardData waveClearRewardData = Managers.MapManager.Instance.GetCanLastClearReward(mapData.StageNumber);

                Managers.GuideInteractorManager.Instance.CurrentWaveClearRewardData = waveClearRewardData;

                if (mapData.StageNumber == 1 && Managers.GuideInteractorManager.Instance.NeedResearchTutorial == true)
                {
                    PlayTutorial().Forget();
                }
                if (mapData.StageNumber == 2 && Managers.GuideInteractorManager.Instance.NeedSynergyOpenTutorial == true)
                {
                    PlaySynergyOpenTutorial().Forget();
                }
                if (mapData.StageNumber == 3 && Managers.GuideInteractorManager.Instance.NeedSynergyUnLockTutorial == true)
                {
                    PlaySynergyUnLockTutorial().Forget();
                }
                else if (waveClearRewardData != null)
                {
                    if (Managers.GuideInteractorManager.Instance.PlayedMainTutorial() == false)
                    {
                        Managers.GuideInteractorManager.Instance.RemoveLastGuidePlayTime(V2Enum_EventType.WaveReward);
                        Managers.GuideInteractorManager.Instance.EndGuideQuest();
                    }
                }
            }

            if (_stageSweep_Btn != null)
                _stageSweep_Btn.gameObject.SetActive(MapContainer.MapMaxClear >= 2);


            if (_stageGroup != null)
            {
                if (mapData.StageNumber == 0)
                {
                    if (MapContainer.MapMaxClear == 0)
                    {
                        _stageGroup.gameObject.SetActive(false);

                        StageInfo stageInfo = Managers.MapManager.Instance.GetStageInfo(MapContainer.MapMaxClear);
                        if (stageInfo.RecvClearReward != stageInfo.LastClearWave)
                        {
                            Managers.GuideInteractorManager.Instance.StartGuideInteractor(V2Enum_EventType.WaveReward);
                        }
                    }
                    else
                        _stageGroup.gameObject.SetActive(true);
                }
                else
                    _stageGroup.gameObject.SetActive(true);
            }


            if (_stage1HideUI != null)
                _stage1HideUI.gameObject.SetActive(mapData.StageNumber != 0);

            RefreshStamina(Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), Define.StaminaIndex));
        }
        //------------------------------------------------------------------------------------
#if DEV_DEFINE
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.F6))
            {
                foreach (var pair in ResearchContainer.ResearchInfo)
                {
                    pair.Value.Level = Define.PlayerJewelryDefaultLevel;
                }
                PlayTutorial().Forget();
            }

            if (Input.GetKeyUp(KeyCode.F7))
            {
                RequestDialogEnter<InGameRankDialog>();
            }
        }
#endif
        //------------------------------------------------------------------------------------
        private async UniTask PlayTutorial()
        {
            await UniTask.WaitForSeconds(0.5f);
            Managers.GuideInteractorManager.Instance.NeedResearchTutorial = false;
            Message.Send(new GameBerry.Event.PlayResearchChangeTutorialMsg());
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlaySynergyOpenTutorial()
        {
            await UniTask.WaitForSeconds(0.5f);
            Managers.GuideInteractorManager.Instance.NeedSynergyOpenTutorial = false;
            Message.Send(new GameBerry.Event.PlaySynergyOpenTutorialMsg());
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlaySynergyUnLockTutorial()
        {
            await UniTask.WaitForSeconds(0.5f);
            Managers.GuideInteractorManager.Instance.NeedSynergyUnLockTutorial = false;
            Message.Send(new GameBerry.Event.PlaySynergyUnLockTutorialMsg());
        }
        //------------------------------------------------------------------------------------
        private void OnClick_PrevMap()
        {
            if (Managers.GuideInteractorManager.Instance.PlayedMainTutorial() == true)
            {
                Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("notice/needcompletetutorial"));
                return;
            }

            if (_currentMapData == null)
                return;

            SetMapUI(Managers.MapManager.Instance.GetPrevChellengeData(_currentMapData));

            Managers.GuideInteractorManager.Instance.StartGuideInteractor(V2Enum_EventType.NextStage);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_NextMap()
        {
            if (Managers.GuideInteractorManager.Instance.PlayedMainTutorial() == true)
            {
                Contents.GlobalContent.ShowGlobalNotice(Managers.LocalStringManager.Instance.GetLocalString("notice/needcompletetutorial"));
                return;
            }

            if (_currentMapData == null)
                return;

            if (_currentMapData.StageNumber == 0)
            {
                ThirdPartyLog.Instance.SendLog_log_tutorial(4);
                if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.NextStage)
                {
                    Managers.GuideInteractorManager.Instance.EndGuideQuest();
                }

                // 0-7 튜토리얼 하이드
                Managers.GuideInteractorManager.Instance.NeedResearchTutorial = true;

                Managers.GuideInteractorManager.Instance.SetLastGuidePlayTime(V2Enum_EventType.StageClear, 0);
            }
            else if (_currentMapData.StageNumber == 1 && Managers.MapManager.Instance.GetStageInfo(2) == null)
            {
                ThirdPartyLog.Instance.SendLog_log_tutorial(6);
                if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.NextStage)
                {
                    Managers.GuideInteractorManager.Instance.EndGuideQuest();
                }

                // 0-7 튜토리얼 하이드
                Managers.GuideInteractorManager.Instance.NeedSynergyOpenTutorial = true;

                Managers.GuideInteractorManager.Instance.SetLastGuidePlayTime(V2Enum_EventType.StageClear, 0);
            }
            else if (_currentMapData.StageNumber == 2 && Managers.MapManager.Instance.GetStageInfo(3) == null)
            {
                ThirdPartyLog.Instance.SendLog_log_tutorial(7);
                if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.NextStage)
                {
                    Managers.GuideInteractorManager.Instance.EndGuideQuest();
                }

                // 0-7 튜토리얼 하이드
                Managers.GuideInteractorManager.Instance.NeedSynergyUnLockTutorial = true;

                Managers.GuideInteractorManager.Instance.SetLastGuidePlayTime(V2Enum_EventType.StageClear, 0);
            }

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.NextStage)
            {
                Managers.GuideInteractorManager.Instance.EndGuideQuest();
            }

            SetMapUI(Managers.MapManager.Instance.GetNextChellengeData(_currentMapData));
        }
        //------------------------------------------------------------------------------------
        private void RefreshStamina(double amount)
        {
            bool canEnter = amount >= Define.RequiredStamina;

            if (_staminaAmount != null)
                _staminaAmount.text = string.Format("{0}/{1}",
                    Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), Define.StaminaIndex),
                    Define.MaxStamina);


            if (_stageEnterStaminaPrice != null)
            { 
                _stageEnterStaminaPrice.color = canEnter ? Color.white : Color.red;
                if (Managers.MapManager.Instance.NeedTutotial1() == true)
                    _stageEnterStaminaPrice.SetText("{0}", 0);
                else
                {
                    if (canEnter == true)
                        _stageEnterStaminaPrice.SetText("-{0}", Define.RequiredStamina);
                    else
                        _stageEnterStaminaPrice.SetText(Managers.LocalStringManager.Instance.GetLocalString("title/stamaniabuy"));
                }
            }

            if (_sweepEnterStaminaPrice != null)
            {
                if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Sweep) == false)
                    _sweepEnterStaminaPrice.color = Color.gray;
                else
                    _sweepEnterStaminaPrice.color = canEnter ? Color.white : Color.red;
            }
        }
        //------------------------------------------------------------------------------------
        private void SetStaminaChargeRemainTime(double timestamp)
        {
            if (_chargeStaminaRemainTime != null)
            {
                if (timestamp <= -1)
                {
                    _chargeStaminaRemainTime.gameObject.SetActive(false);
                    return;
                }

                _chargeStaminaRemainTime.gameObject.SetActive(true);

                System.TimeSpan dateTime = new System.TimeSpan(0, 0, (int)timestamp);
                _chargeStaminaRemainTime.text = string.Format("{0}:{1:00}", dateTime.Minutes, dateTime.Seconds);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_EventPackageBtn(int index)
        {
            ShopDataBase shopDataBase = Managers.ShopManager.Instance.GetShopData(index);

            Managers.ShopManager.Instance.ShowEventPackagePopupDialog(shopDataBase as ShopPackageEventData);
        }
        //------------------------------------------------------------------------------------
        private void RefreshShopEvent(GameBerry.Event.RefreshShopEventMsg msg)
        {
            UIShopElement_LimitTimeIcon eventPackageUI = eventPackageUIs.Find(x => x.PackageIndex == msg.shopPackageEventData.Index);

            if (eventPackageUI == null)
                return;


            if (Managers.ShopManager.Instance.IsSoldOut(msg.shopPackageEventData) == false)
            {
                eventPackageUI.NeedTimerCheck = true;
                SetEventPackageUIEventTime(eventPackageUI);
            }
            else
            {
                eventPackageUI.PackageBtn.gameObject.SetActive(false);
                eventPackageUI.NeedTimerCheck = false;
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshStageSweep(GameBerry.Event.RefreshStageSweepMsg msg)
        {

            if (_adRemoveStageSweepComponent != null)
            {
                if (Define.IsAdFree == true)
                {
                    _adRemoveStageSweepComponent.gameObject.SetActive(false);
                }
                else
                {
                    _adRemoveStageSweepComponent.gameObject.SetActive(true);

                    if (_stageSweepCount != null)
                        _stageSweepCount.SetText("({0}/{1})", Managers.MapManager.Instance.GetRemainSweepCount(), Define.DailySweepCount);
                }
            }

        }
        //------------------------------------------------------------------------------------
        private void SetEventPackageUIEventTime(UIShopElement_LimitTimeIcon eventPackageUI)
        {
            PlayerShopInfo playerShopInfo = Managers.ShopManager.Instance.GetPlayerShopInfo(eventPackageUI.PackageIndex);
            if (playerShopInfo == null)
            {
                eventPackageUI.PackageBtn.gameObject.SetActive(false);
                return;
            }

            int rawSecond = (int)(playerShopInfo.InitTimeStemp - Managers.TimeManager.Instance.Current_TimeStamp);

            if (rawSecond < 0)
            {
                eventPackageUI.PackageBtn.gameObject.SetActive(false);
                eventPackageUI.NeedTimerCheck = false;

                return;
            }

            eventPackageUI.PackageBtn.gameObject.SetActive(true);

            if (eventPackageUI.PackageRemainTimeBtn != null)
            {
                int remainSecond = rawSecond % 60;

                int rawMinute = rawSecond / 60;

                int remainMinute = rawMinute % 60;

                int remainHour = rawMinute / 60;

                string remainTime = string.Format("{0}:{1}:{2}", remainHour, remainMinute, remainSecond);

                eventPackageUI.PackageRemainTimeBtn.text = remainTime;
                eventPackageUI.PackageRemainTimeBtn.color = remainHour > 0 ? remainTimeColor : remainTimeColor_Noti;
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_BottomBtn(ContentDetailList callbackID)
        {
            ShowBottomDialog(callbackID);
        }
        //------------------------------------------------------------------------------------
        private void ShowBottomDialog(ContentDetailList callbackID)
        {
            if (callbackID == ContentDetailList.None)
                return;

            switch (callbackID)
            {
                case ContentDetailList.LobbyGear:
                    {
                        RequestDialogEnter<LobbyCharacterContentDialog>();
                        break;
                    }
                case ContentDetailList.LobbyRelic:
                    {
                        RequestDialogEnter<LobbyRelicContentDialog>();
                        break;
                    }
                case ContentDetailList.TimeAttackMission:
                    {
                        RequestDialogEnter<LobbyTimeAttackMissionDialog>();
                        break;
                    }
                case ContentDetailList.LobbyDescend:
                    {
                        RequestDialogEnter<LobbyDescendContentDialog>();
                        break;
                    }
                case ContentDetailList.LobbySynergy:
                    {
                        RequestDialogEnter<LobbySynergyContentDialog>();
                        break;
                    }
                case ContentDetailList.LobbySynergyRune:
                    {
                        RequestDialogEnter<LobbySynergyRuneContentDialog>();
                        break;
                    }
                case ContentDetailList.Shop:
                    {
                        RequestDialogEnter<ShopGeneralDialog>();
                        break;
                    }
                case ContentDetailList.StageMap:
                    {
                        OnClick_StageEnter();
                        break;
                    }
                case ContentDetailList.Quest:
                    {
                        RequestDialogEnter<LobbyQuestContentDialog>();
                        break;
                    }
                case ContentDetailList.Pass:
                    {
                        RequestDialogEnter<LobbyPassDialog>();
                        break;
                    }
                case ContentDetailList.AdBuff:
                    {
                        RequestDialogEnter<AdBuffDialog>();
                        break;
                    }
                case ContentDetailList.LobbyResearch:
                    {
                        RequestDialogEnter<LobbyResearchDialog>();
                        break;
                    }
                case ContentDetailList.Rank:
                    {
                        RequestDialogEnter<InGameRankDialog>();
                        break;
                    }
                case ContentDetailList.Dungeon:
                    {
                        RequestDialogEnter<DungeonContentDialog>();
                        break;
                    }
            }
        }
        //------------------------------------------------------------------------------------
    }
}