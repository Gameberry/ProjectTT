using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using Spine;
using Spine.Unity;
using DG.Tweening;

namespace GameBerry.UI
{
    [System.Serializable]
    public class BattleSceneUI
    {
        public Enum_Dungeon Enum_BattleType;

        public List<Transform> CustomUIs = new List<Transform>();

        public IDialog SceneUIDialog;
    }

    public class BattleSceneDialog : IDialog
    {
        [SerializeField] 
        private List<BattleSceneUI> _battleSceneUIs = new List<BattleSceneUI>();

        [SerializeField]
        private Button _showCheat;

        [SerializeField]
        private Button _battleSpeed_Btn;

        [SerializeField]
        private TMP_Text _battleSpeed_Text;

        [SerializeField]
        private Transform _battleSpeed_x1;

        [SerializeField]
        private Transform _battleSpeed_xOther;

        [SerializeField]
        private Transform _battleSpeed_AdRemoveHooking;

        [SerializeField]
        private Button _playBtn;

        [SerializeField]
        private Button _goLobbyBtn;

        private BattleSceneUI _currentBattleSceneUI = null;


        [Header("----------ARRRState----------")]
        [SerializeField]
        private TMP_Text _attackBuffPer;

        [SerializeField]
        private TMP_Text _defenceBuffPer;

        [SerializeField]
        private TMP_Text _hpBuffPer;

        [SerializeField]
        private TMP_Text _currentHP_Text;

        [SerializeField]
        private TMP_Text _maxHP_Text;

        [SerializeField]
        private RectTransform _hpGauge_RectTrans;

        [SerializeField]
        private float _hpGauge_MaxWeight;

        [Header("----------BossHp----------")]
        [SerializeField]
        private TMP_Text _bossCurrentHP_Text;

        [SerializeField]
        private TMP_Text _bossMaxHP_Text;

        [SerializeField]
        private RectTransform _bossHpGauge_RectTrans;

        [SerializeField]
        private float _bossHpGauge_MaxWeight;

        [Header("------------Result_Win------------")]
        [SerializeField]
        private Transform _winPopup;

        [Header("------------Result_lose------------")]
        [SerializeField]
        private Transform _losePopup;

        [Header("------------Result------------")]
        [SerializeField]
        private Transform _playTimeGroup;

        [SerializeField]
        private TMP_Text _playTime;

        [SerializeField]
        private Transform _applyAdBuff;

        [SerializeField]
        private TMP_Text _applyAdBuffValue;

        [SerializeField]
        private Transform _farmingItemGroup;

        [SerializeField]
        private UIGlobalGoodsRewardIconElement _farmingItem;

        [SerializeField]
        private TMP_Text _currentRecord;

        [SerializeField]
        private TMP_Text _prevRecord;

        [SerializeField]
        private Transform _clear;

        [SerializeField]
        private Transform _doubleRewardComplete;

        [SerializeField]
        private Button _doubleReward;

        [SerializeField]
        private TMP_Text _doubleReward_Count;


        [SerializeField]
        private SkeletonGraphic _skeletonGraphic;

        Skin myEquipsSkin = new Skin("my new skin");

        [SerializeField]
        private Transform _loseNotiGroup;

        [SerializeField]
        private Transform _stageLoseGroup;

        [SerializeField]
        private TMP_Text _stageLose_Text;

        [SerializeField]
        private float _stageLose_TextAniDuration = 0.05f;

        [SerializeField]
        private float _stageLose_TextAniDelay = 1.0f;

        [SerializeField]
        private Transform _dungeonLoseGroup;

        [Header("----------Tutorial----------")]
        [SerializeField]
        private Transform _tutorialBlack;

        [SerializeField]
        private TMP_Text _tutorialText;

        [SerializeField]
        private Button _resultLobbyBtn;

        private List<UIGlobalGoodsRewardIconElement> m_uIGlobalGoodsRewardIconElements = new List<UIGlobalGoodsRewardIconElement>();

        private BattleSceneBase _battle_StageScene;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            RefreshSkin();

            for (int i = 0; i < _battleSceneUIs.Count; ++i)
            {
                if (_battleSceneUIs[i].SceneUIDialog != null)
                { 
                    _battleSceneUIs[i].SceneUIDialog.Load_Element();
                }

                _battleSceneUIs[i].CustomUIs.AllSetActive(false);
            }

            if (_playBtn != null)
                _playBtn.onClick.AddListener(OnClick_PlayBtn);


            if (_battleSpeed_Btn != null)
                _battleSpeed_Btn.onClick.AddListener(OnClick_ChangeBattleSpeed);

            if (_goLobbyBtn != null)
                _goLobbyBtn.onClick.AddListener(OnClick_GoLobbyBtn);

            if (_resultLobbyBtn != null)
                _resultLobbyBtn.onClick.AddListener(OnClick_ResultLobbyBtn);

            if (_doubleReward != null)
                _doubleReward.onClick.AddListener(OnClick_DoubleReward);

#if DEV_DEFINE
            if (_showCheat != null)
                _showCheat.onClick.AddListener(() =>
                {
                    RequestDialogEnter<GlobalCheatDialog>();
                });
#endif

            Message.AddListener<GameBerry.Event.RefreshBattleSceneUIMsg>(RefreshBattleSceneUI);
            Message.AddListener<GameBerry.Event.RefreshBattleSpeedMsg>(RefreshBattleSpeed);
            Message.AddListener<GameBerry.Event.ResultBattleStageMsg>(ResultBattleStage);
            Message.AddListener<GameBerry.Event.RefreshResultBattleStage_DoubleRewardMsg>(RefreshResultBattleStage_DoubleReward);
            Message.AddListener<GameBerry.Event.PlayARRRTutorialMsg>(PlayARRRTutorial);
            Message.AddListener<GameBerry.Event.RefreshCharacterSkin_StatMsg>(RefreshCharacterSkin_Stat);
            Message.AddListener<GameBerry.Event.RefreshAddBuffMsg>(RefreshAddBuff);
            Message.AddListener<GameBerry.Event.SetHpBarMsg>(SetHpBar);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.RefreshBattleSceneUIMsg>(RefreshBattleSceneUI);
            Message.RemoveListener<GameBerry.Event.RefreshBattleSpeedMsg>(RefreshBattleSpeed);
            Message.RemoveListener<GameBerry.Event.ResultBattleStageMsg>(ResultBattleStage);
            Message.RemoveListener<GameBerry.Event.RefreshResultBattleStage_DoubleRewardMsg>(RefreshResultBattleStage_DoubleReward);
            Message.RemoveListener<GameBerry.Event.PlayARRRTutorialMsg>(PlayARRRTutorial);
            Message.RemoveListener<GameBerry.Event.RefreshCharacterSkin_StatMsg>(RefreshCharacterSkin_Stat);
            Message.RemoveListener<GameBerry.Event.RefreshAddBuffMsg>(RefreshAddBuff);
            Message.RemoveListener<GameBerry.Event.SetHpBarMsg>(SetHpBar);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            AllHideResultUI();


        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            AllHideResultUI();
        }
        //------------------------------------------------------------------------------------
        private void AllHideResultUI()
        {
            if (_winPopup != null)
                _winPopup.gameObject.SetActive(false);

            if (_losePopup != null)
                _losePopup.gameObject.SetActive(false);

            if (_playTimeGroup != null)
                _playTimeGroup.gameObject.SetActive(false);

            if (_farmingItemGroup != null)
                _farmingItemGroup.gameObject.SetActive(false);

            if (_applyAdBuff != null)
                _applyAdBuff.gameObject.SetActive(false);

            if (_resultLobbyBtn != null)
                _resultLobbyBtn.gameObject.SetActive(false);

            while (m_uIGlobalGoodsRewardIconElements.Count > 0)
            {
                UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = m_uIGlobalGoodsRewardIconElements[0];

                if (Managers.RewardManager.isAlive == true)
                    Managers.RewardManager.Instance.PoolGoodsRewardIcon(uIGlobalGoodsRewardIconElement);
                m_uIGlobalGoodsRewardIconElements.Remove(uIGlobalGoodsRewardIconElement);
            }
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
        private void RefreshStatBuffUI(V2Enum_Stat v2Enum_Stat)
        {
            if (_battle_StageScene == null)
                return;

            if (_battle_StageScene.MyARRRControllers == null)
                return;

            ARRRController aRRRController = _battle_StageScene.MyARRRControllers;
            if (aRRRController != null)
            {
                double buffValue = aRRRController.CharacterStatOperator.GetBuffValue(v2Enum_Stat);

                if (v2Enum_Stat == V2Enum_Stat.Attack)
                {
                    if (_attackBuffPer != null)
                        _attackBuffPer.text = string.Format("+{0:0.##}%", buffValue * 100);
                }
                else if (v2Enum_Stat == V2Enum_Stat.Defence)
                {
                    if (_defenceBuffPer != null)
                        _defenceBuffPer.text = string.Format("+{0:0.##}%", buffValue * 100);
                }
                else if (v2Enum_Stat == V2Enum_Stat.HP)
                {
                    if (_hpBuffPer != null)
                        _hpBuffPer.text = string.Format("+{0:0.##}%", buffValue * 100);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshAddBuff(GameBerry.Event.RefreshAddBuffMsg msg)
        {
            if (msg.v2Enum_Stat == V2Enum_Stat.Max)
            {
                RefreshStatBuffUI(V2Enum_Stat.Attack);
                RefreshStatBuffUI(V2Enum_Stat.Defence);
                RefreshStatBuffUI(V2Enum_Stat.HP);
            }
            else
                RefreshStatBuffUI(msg.v2Enum_Stat);
        }
        //------------------------------------------------------------------------------------
        private void Update()
        {
            if (isEnter == false)
                return;

            if (_battle_StageScene == null)
                return;

            if (Managers.BattleSceneManager.Instance.BattleType == Enum_Dungeon.LobbyScene)
                return;

            if (_battle_StageScene.MyARRRControllers != null)
            {
                double currhp = _battle_StageScene.MyARRRControllers.CurrentHP;
                double maxhp = _battle_StageScene.MyARRRControllers.MaxHP;

                double ratio = 0;

                if (maxhp > 0)
                {
                    ratio = currhp / maxhp;

                    if (ratio > 1)
                        ratio = 1;
                }
                else
                {
                    currhp = 0;
                    maxhp = 0;
                }

                if (_currentHP_Text != null)
                    _currentHP_Text.text = string.Format("{0:0,0}", currhp);

                if (_maxHP_Text != null)
                    _maxHP_Text.text = string.Format("{0:0,0}", maxhp);

                if (_hpGauge_RectTrans != null)
                {
                    Vector2 gaugeSize = _hpGauge_RectTrans.sizeDelta;
                    gaugeSize.x = _hpGauge_MaxWeight * (float)ratio;
                    _hpGauge_RectTrans.sizeDelta = gaugeSize;
                }
            }

            
        }
        //------------------------------------------------------------------------------------
        private void SetHpBar(GameBerry.Event.SetHpBarMsg msg)
        {
            double currhp = msg.currHp;
            double maxhp = msg.TotalHp;

            double ratio = 0;

            if (maxhp > 0)
            {
                ratio = currhp / maxhp;

                if (ratio > 1)
                    ratio = 1;
            }
            else
            {
                currhp = 0;
                maxhp = 0;
            }

            if (_bossCurrentHP_Text != null)
                _bossCurrentHP_Text.text = string.Format("{0:0,0}", currhp);

            if (_bossMaxHP_Text != null)
                _bossMaxHP_Text.text = string.Format("{0:0,0}", maxhp);

            if (_bossHpGauge_RectTrans != null)
            {
                Vector2 gaugeSize = _bossHpGauge_RectTrans.sizeDelta;
                gaugeSize.x = _bossHpGauge_MaxWeight * (float)ratio;
                _bossHpGauge_RectTrans.sizeDelta = gaugeSize;
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshBattleSceneUI(GameBerry.Event.RefreshBattleSceneUIMsg msg)
        {
            if (_currentBattleSceneUI != null)
            {
                for (int i = 0; i < _currentBattleSceneUI.CustomUIs.Count; ++i)
                {
                    _currentBattleSceneUI.CustomUIs[i].gameObject.SetActive(false);
                }

                if (_currentBattleSceneUI.SceneUIDialog != null)
                    _currentBattleSceneUI.SceneUIDialog.ElementExit();
            }

            AllHideResultUI();

            _currentBattleSceneUI = _battleSceneUIs.Find(x => x.Enum_BattleType == Managers.BattleSceneManager.Instance.BattleType);
            if (_currentBattleSceneUI == null)
                return;

            for (int i = 0; i < _currentBattleSceneUI.CustomUIs.Count; ++i)
            {
                _currentBattleSceneUI.CustomUIs[i].gameObject.SetActive(true);
            }

            if (_currentBattleSceneUI.SceneUIDialog != null)
                _currentBattleSceneUI.SceneUIDialog.ElementEnter();

            if (Managers.BattleSceneManager.Instance.BattleType == Enum_Dungeon.StageScene)
            {
                if (_battleSpeed_Btn != null)
                {
                    if (Managers.MapManager.Instance.NeedTutotial1() == true)
                        _battleSpeed_Btn.gameObject.SetActive(false);
                    else
                        _battleSpeed_Btn.gameObject.SetActive(true);
                }

                if (_goLobbyBtn != null)
                {
                    if (Managers.MapManager.Instance.NeedTutotial1() == true)
                        _goLobbyBtn.gameObject.SetActive(false);
                    else
                        _goLobbyBtn.gameObject.SetActive(true);
                }
            }

            if (_battleSpeed_AdRemoveHooking != null)
                _battleSpeed_AdRemoveHooking.gameObject.SetActive(Define.IsAdFree == false);

            _battle_StageScene = Managers.BattleSceneManager.Instance.CurrentBattleScene;

            RefreshStatBuffUI(V2Enum_Stat.Attack);
            RefreshStatBuffUI(V2Enum_Stat.Defence);
            RefreshStatBuffUI(V2Enum_Stat.HP);

            RefreshBattleSpeed(null);
        }
        //------------------------------------------------------------------------------------
        private void RefreshBattleSpeed(GameBerry.Event.RefreshBattleSpeedMsg msg)
        {
            Enum_BattleSpeed Enum_BattleSpeed = Managers.BattleSceneManager.Instance.CurrentBattleSpeed();

            if (_battleSpeed_Text != null)
            {
                if (Enum_BattleSpeed == Enum_BattleSpeed.x1Dot5)
                    _battleSpeed_Text.text = "x1.5";
                else
                    _battleSpeed_Text.text = Enum_BattleSpeed.ToString();
            }

            if (_battleSpeed_x1 != null)
                _battleSpeed_x1.gameObject.SetActive(Enum_BattleSpeed == Enum_BattleSpeed.x1);

            if (_battleSpeed_xOther != null)
                _battleSpeed_xOther.gameObject.SetActive(Enum_BattleSpeed != Enum_BattleSpeed.x1);
        }
        //------------------------------------------------------------------------------------
        private void ResultBattleStage(GameBerry.Event.ResultBattleStageMsg msg)
        {
            if (msg.Win == true)
            {
                ShowWin();

                if (_clear != null)
                    _clear.gameObject.SetActive(true);

                if (_currentRecord != null)
                    _currentRecord.gameObject.SetActive(false);

                if (_prevRecord != null)
                    _prevRecord.gameObject.SetActive(false);

                if (_loseNotiGroup != null)
                    _loseNotiGroup.gameObject.SetActive(false);

                if (_stageLoseGroup != null)
                    _stageLoseGroup.gameObject.SetActive(false);

                if (_dungeonLoseGroup != null)
                    _dungeonLoseGroup.gameObject.SetActive(false);
            }
            else
            { 
                ShowLose();

                if (_clear != null)
                    _clear.gameObject.SetActive(false);

                if (_currentRecord != null)
                {
                    _currentRecord.gameObject.SetActive(true);
                    if (msg.EnumDungeon == Enum_Dungeon.StageScene)
                    {
                        int stage = msg.currentRecord / 100;
                        int wave = msg.currentRecord % 100;
                        _currentRecord.gameObject.SetActive(true);
                        //_currentRecord.SetText(string.Format("{0} : {1}", Managers.LocalStringManager.Instance.GetLocalString("result/ui/lastwave"),
                        //    string.Format("{0}-{1}", stage, wave)));
                        _currentRecord.SetText(string.Format("{0}", string.Format("{0}-{1}", stage, wave)));
                    }
                    else
                        _currentRecord.SetText(msg.currentRecord.ToString());
                }

                if (_prevRecord != null)
                { 
                    _prevRecord.gameObject.SetActive(false);

                    if (msg.EnumDungeon == Enum_Dungeon.StageScene)
                    {
                        int stage = msg.prevRecord / 100;
                        int wave = msg.prevRecord % 100;
                        _prevRecord.gameObject.SetActive(true);
                        _prevRecord.SetText(string.Format("{0} : {1}", Managers.LocalStringManager.Instance.GetLocalString("result/ui/bestrecord"),
                            string.Format("{0}-{1}", stage, wave)));
                    }
                    else
                        _prevRecord.SetText(string.Format("{0} : {1}", Managers.LocalStringManager.Instance.GetLocalString("result/ui/bestrecord"),
                            msg.prevRecord.ToString()));
                }

                if (_loseNotiGroup != null)
                    _loseNotiGroup.gameObject.SetActive(true);

                if (msg.EnumDungeon == Enum_Dungeon.StageScene)
                {
                    if (_stageLoseGroup != null)
                        _stageLoseGroup.gameObject.SetActive(true);

                    if (_dungeonLoseGroup != null)
                        _dungeonLoseGroup.gameObject.SetActive(false);

                    MapData mapData = Managers.MapManager.Instance.GetMapData(MapContainer.MapLastEnter);
                    string localstring = string.Empty;

                    if (mapData != null && mapData.ResultLocalKey != null && mapData.ResultLocalKey.Length > 0)
                        localstring = mapData.ResultLocalKey[UnityEngine.Random.Range(0, mapData.ResultLocalKey.Length)];

                    string localstr = Managers.LocalStringManager.Instance.GetLocalString(localstring);

                    if (_stageLose_Text != null)
                    {


                        _stageLose_Text.DOKill();
                        _stageLose_Text.text = "";
                        _stageLose_Text.DOText(localstr, localstr.Length * _stageLose_TextAniDuration).SetDelay(_stageLose_TextAniDelay).SetEase(Ease.Linear);
                    }
                }
                else
                {
                    if (_stageLoseGroup != null)
                        _stageLoseGroup.gameObject.SetActive(false);

                    if (_dungeonLoseGroup != null)
                        _dungeonLoseGroup.gameObject.SetActive(true);
                }
            }

            if (_farmingItemGroup != null)
            { 
                _farmingItemGroup.gameObject.SetActive(true);

                List<RewardData> rewardDatas = msg.WaveRewardList;

                for (int i = 0; i < rewardDatas.Count; ++i)
                {
                    RewardData rewardData = rewardDatas[i];
                    UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = Managers.RewardManager.Instance.GetGoodsRewardIcon_NoneParticle();
                    if (uIGlobalGoodsRewardIconElement == null)
                        break;

                    uIGlobalGoodsRewardIconElement.transform.SetParent(_farmingItemGroup);
                    uIGlobalGoodsRewardIconElement.gameObject.SetActive(true);
                    uIGlobalGoodsRewardIconElement.transform.localScale = Vector3.one;

                    uIGlobalGoodsRewardIconElement.SetRewardElement(rewardData);

                    m_uIGlobalGoodsRewardIconElements.Add(uIGlobalGoodsRewardIconElement);
                }
            }

            if (_applyAdBuff != null)
            {
                if (msg.ApplyAdIncreaseRewardMode == true)
                {
                    _applyAdBuff.gameObject.SetActive(true);

                    if (_applyAdBuffValue != null)
                        _applyAdBuffValue.text = string.Format("+{0:0.##}%", msg.ApplyAdIncreaseRewardValue * 100);
                }
                else
                {
                    _applyAdBuff.gameObject.SetActive(false);
                }
            }


            //if (_farmingItem != null)
            //{
            //    if (msg.FarmingItem != null)
            //    {
            //        _farmingItem.gameObject.SetActive(true);
            //        _farmingItem.SetRewardElement(msg.FarmingItem);
            //    }
            //}

            if (_playTimeGroup != null)
                _playTimeGroup.gameObject.SetActive(true);

            if (_resultLobbyBtn != null)
                _resultLobbyBtn.gameObject.SetActive(true);

            if (_playTime != null)
            { 
                _playTime.gameObject.SetActive(true);

                System.TimeSpan dateTime = new System.TimeSpan(0, 0, (int)(msg.PlayTime));

                string RemainInitMonthContent_String = string.Format("{0} {1}"
                    , string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.MinuteLocalKey), dateTime.Minutes)
                    , string.Format(Managers.LocalStringManager.Instance.GetLocalString(Define.SecondLocalKey), dateTime.Seconds)
                    );

                _playTime.SetText(RemainInitMonthContent_String);
            }

            RefreshDoubleRewardBtn();
        }
        //------------------------------------------------------------------------------------
        private void RefreshResultBattleStage_DoubleReward(GameBerry.Event.RefreshResultBattleStage_DoubleRewardMsg msg)
        {
            while (m_uIGlobalGoodsRewardIconElements.Count > 0)
            {
                UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = m_uIGlobalGoodsRewardIconElements[0];

                if (Managers.RewardManager.isAlive == true)
                    Managers.RewardManager.Instance.PoolGoodsRewardIcon(uIGlobalGoodsRewardIconElement);
                m_uIGlobalGoodsRewardIconElements.Remove(uIGlobalGoodsRewardIconElement);
            }

            if (_farmingItemGroup != null)
            {
                _farmingItemGroup.gameObject.SetActive(true);

                List<RewardData> rewardDatas = msg.WaveRewardList;

                for (int i = 0; i < rewardDatas.Count; ++i)
                {
                    RewardData rewardData = rewardDatas[i];
                    UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = Managers.RewardManager.Instance.GetGoodsRewardIcon_NoneParticle();
                    if (uIGlobalGoodsRewardIconElement == null)
                        break;

                    uIGlobalGoodsRewardIconElement.transform.SetParent(_farmingItemGroup);
                    uIGlobalGoodsRewardIconElement.gameObject.SetActive(true);
                    uIGlobalGoodsRewardIconElement.transform.localScale = Vector3.one;

                    uIGlobalGoodsRewardIconElement.SetRewardElement(rewardData);

                    m_uIGlobalGoodsRewardIconElements.Add(uIGlobalGoodsRewardIconElement);
                }
            }

            if (_doubleRewardComplete != null)
                _doubleRewardComplete.gameObject.SetActive(true);

            if (_doubleReward != null)
                _doubleReward.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        private void RefreshDoubleRewardBtn()
        {
            if (Managers.BattleSceneManager.Instance.BattleType == Enum_Dungeon.StageScene)
            {
                if (Managers.MapManager.Instance.GetRemainDoubleRewardCount() <= 0)
                {
                    if (_doubleReward != null)
                        _doubleReward.gameObject.SetActive(false);

                    if (_doubleRewardComplete != null)
                        _doubleRewardComplete.gameObject.SetActive(false);
                }
                else
                {
                    if (_doubleRewardComplete != null)
                        _doubleRewardComplete.gameObject.SetActive(false);

                    if (_doubleReward != null)
                        _doubleReward.gameObject.SetActive(true);

                    if (_doubleReward_Count != null)
                        _doubleReward_Count.SetText("({0}/{1})", Managers.MapManager.Instance.GetRemainDoubleRewardCount(), Define.DailyDoubleRewardCount);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void ShowWin()
        {
            if (_winPopup != null)
                _winPopup.gameObject.SetActive(true);
        }
        //------------------------------------------------------------------------------------
        private void ShowLose()
        {
            if (_losePopup != null)
                _losePopup.gameObject.SetActive(true);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ChangeBattleSpeed()
        {
            if (Managers.GuideInteractorManager.Instance.PlayGameSpeedTutorial == true)
            { 
                Managers.BattleSceneManager.Instance.ChangeTimeScale(Enum_BattleSpeed.Pause);

                if (_tutorialBlack != null)
                {
                    _tutorialBlack.gameObject.SetActive(false);
                }

                ThirdPartyLog.Instance.SendLog_log_tutorial(2);
            }
            Managers.BattleSceneManager.Instance.ChangeBattleSpeed_UI();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_PlayBtn()
        {
            if (Managers.BattleSceneManager.Instance.CurrentBattleScene.IsReadyBattle() == true)
                Managers.BattleSceneManager.Instance.CurrentBattleScene.PlayBattleScene();

            if (_playBtn != null)
                _playBtn.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_GoLobbyBtn()
        {
            Managers.BattleSceneManager.Instance.VisibleDungeonExitPopup(true);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ResultLobbyBtn()
        {
            Managers.BattleSceneManager.Instance.ChangeBattleScene(Enum_Dungeon.LobbyScene);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_DoubleReward()
        {
            if (_battle_StageScene != null)
            {
                Battle_StageScene battle_StageScene = _battle_StageScene as Battle_StageScene;
                if (battle_StageScene != null)
                {
                    battle_StageScene.PlayDoubleReward();
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void PlayARRRTutorial(GameBerry.Event.PlayARRRTutorialMsg msg)
        {
            if (msg.Enum_GambleType != V2Enum_EventType.SpeedUp)
                return;

            if (_battleSpeed_Btn != null)
                _battleSpeed_Btn.gameObject.SetActive(true);

            Managers.GuideInteractorManager.Instance.StartGuideInteractor(V2Enum_EventType.SpeedUp);

            if (_tutorialText != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_tutorialText, "guide/speedup");

            if (_tutorialBlack != null)
            {
                _tutorialBlack.gameObject.SetActive(true);
            }

            Managers.BattleSceneManager.Instance.ChangeTimeScale(Enum_BattleSpeed.Pause);
        }
        //------------------------------------------------------------------------------------
    }
}