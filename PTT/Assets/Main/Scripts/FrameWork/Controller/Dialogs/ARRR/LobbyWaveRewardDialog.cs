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
    public class LobbyWaveRewardDialog : IDialog
    {
        [Header("------------BG------------")]
        [SerializeField]
        private Image _bgImage;

        [SerializeField]
        private List<Sprite> _bgSpriteList = new List<Sprite>();

        [Header("------------RecordBubble------------")]
        [SerializeField]
        private Transform _bubbleGroup;

        [SerializeField]
        private TMP_Text _stageRecord;

        [Header("------------RewardScroll------------")]
        [SerializeField]
        private LayoutGroup _rewardLayoutGroup;

        [SerializeField]
        private ContentSizeFitter _rewardContentSizeFiltter;

        [SerializeField]
        private ScrollRect _rewardScroll;

        [SerializeField]
        private Transform _rewardRoot;

        [SerializeField]
        private Transform _startPos;

        [SerializeField]
        private UIWaveRewardElement _uIWaveRewardElement;

        [SerializeField]
        private Transform _characterRoot;

        [SerializeField]
        private SkeletonGraphic _skeletonGraphic;

        [SerializeField]
        private float _characterRollStartDelay = 0.3f;

        [SerializeField]
        private string _characterMoveAnimationName = "Roll_2";

        [SerializeField]
        private float _characterMoveDuration = 0.3f;

        [SerializeField]
        private float _characterRollEndDelay = 0.3f;

        [SerializeField]
        private Button _uIGuideInteractorBtn;


        [Header("------------RewardScroll------------")]
        [SerializeField]
        private Transform _job;

        Skin myEquipsSkin = new Skin("my new skin");

        private Dictionary<WaveClearRewardData, UIWaveRewardElement> _uIGambleSynergyDetailElement_Dic = new Dictionary<WaveClearRewardData, UIWaveRewardElement>();
        private List<UIWaveRewardElement> _uIGambleSynergyDetailElementList = new List<UIWaveRewardElement>();

        private StageInfo _currentStageInfo = null;

        private Coroutine _characterMoveDirectionCoroutine = null;

        private WaveClearRewardData _currentWaveClearRewardData = null;

        private UIWaveRewardElement _lastWaveReward = null;

        private int _currentStage = 0;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            RefreshSkin();

            if (_uIGuideInteractorBtn != null)
                _uIGuideInteractorBtn.onClick.AddListener(OnClick_GuideInteractor);

            Message.AddListener<GameBerry.Event.SetWaveRewardDialogMsg>(SetWaveRewardDialog);
            Message.AddListener<GameBerry.Event.RefreshCharacterSkin_StatMsg>(RefreshCharacterSkin_Stat);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.SetWaveRewardDialogMsg>(SetWaveRewardDialog);
            Message.RemoveListener<GameBerry.Event.RefreshCharacterSkin_StatMsg>(RefreshCharacterSkin_Stat);
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
        private void OnClick_GuideInteractor()
        {
            if (_currentWaveClearRewardData != null)
                OnClick_WaveClearReward(_currentWaveClearRewardData);
        }
        //------------------------------------------------------------------------------------
        private void SetWaveRewardDialog(GameBerry.Event.SetWaveRewardDialogMsg msg)
        {
            Dictionary<ObscuredInt, WaveClearRewardData> waveClearDatas = Managers.MapManager.Instance.GetWaveClearsRewardData(msg.StageNumber);

            if (waveClearDatas == null)
                return;

            _currentStage = msg.StageNumber;

            if (_bgImage != null)
            { 
                MapData mapData = Managers.MapManager.Instance.GetMapData(msg.StageNumber);
                if (mapData != null)
                {
                    if (_bgSpriteList.Count > mapData.BackGround)
                        _bgImage.sprite = _bgSpriteList[mapData.BackGround];
                }
            }

            if (_characterMoveDirectionCoroutine != null)
                StopCoroutine(_characterMoveDirectionCoroutine);

            _uIGambleSynergyDetailElement_Dic.Clear();

            _currentWaveClearRewardData = Managers.MapManager.Instance.GetCanLastClearReward(msg.StageNumber);

            {
                int i = 0;

                foreach (var pair in waveClearDatas)
                {
                    WaveClearRewardData waveClearRewardData = pair.Value;

                    UIWaveRewardElement uIGambleSynergyDetailElement = null;

                    if (i < _uIGambleSynergyDetailElementList.Count)
                    {
                        uIGambleSynergyDetailElement = _uIGambleSynergyDetailElementList[i];
                    }
                    else
                    {
                        GameObject clone = Instantiate(_uIWaveRewardElement.gameObject, _rewardRoot);
                        uIGambleSynergyDetailElement = clone.GetComponent<UIWaveRewardElement>();
                        uIGambleSynergyDetailElement.Init(OnClick_WaveClearReward);
                        _uIGambleSynergyDetailElementList.Add(uIGambleSynergyDetailElement);
                    }

                    uIGambleSynergyDetailElement.gameObject.SetActive(true);
                    uIGambleSynergyDetailElement.SetQuestGaugeElement(waveClearRewardData);

                    uIGambleSynergyDetailElement.transform.SetAsLastSibling();

                    _lastWaveReward = uIGambleSynergyDetailElement;

                    _uIGambleSynergyDetailElement_Dic.Add(waveClearRewardData, uIGambleSynergyDetailElement);

                    i++;
                }
            }

            for (int i = waveClearDatas.Count; i < _uIGambleSynergyDetailElementList.Count; ++i)
            {
                UIWaveRewardElement uIGambleSynergyDetailElement = _uIGambleSynergyDetailElementList[i];
                uIGambleSynergyDetailElement.gameObject.SetActive(false);
            }

            if (_rewardLayoutGroup != null)
                _rewardLayoutGroup.enabled = true;

            if (_rewardContentSizeFiltter != null)
                _rewardContentSizeFiltter.enabled = true;

            _currentStageInfo = Managers.MapManager.Instance.GetStageInfo(msg.StageNumber);

            FrameDelay().Forget();


            if (_stageRecord != null)
            {
                string recordstr = MapOperator.ConvertWaveTotalNumberToUIString(MapContainer.MaxWaveClear);
                _stageRecord.SetText(
                    string.Format(Managers.LocalStringManager.Instance.GetLocalString("ui/stagerecord/dialogue"),
                    recordstr));
            }

            RefreshSkin();

            ShowBubble();
        }
        //------------------------------------------------------------------------------------
        private async UniTask FrameDelay()
        {
            await UniTask.NextFrame();

            if (_rewardLayoutGroup != null)
                _rewardLayoutGroup.enabled = false;

            if (_rewardContentSizeFiltter != null)
                _rewardContentSizeFiltter.enabled = false;

            if (_uIGuideInteractorBtn != null)
            { 
                _uIGuideInteractorBtn.gameObject.SetActive(_currentWaveClearRewardData != null);

                if (_currentWaveClearRewardData != null)
                {
                    if (_uIGambleSynergyDetailElement_Dic.ContainsKey(_currentWaveClearRewardData) == true)
                    {
                        UIWaveRewardElement uIWaveRewardElement = _uIGambleSynergyDetailElement_Dic[_currentWaveClearRewardData];
                        _uIGuideInteractorBtn.transform.position = uIWaveRewardElement.GetButtonPos();
                        _uIGuideInteractorBtn.transform.SetAsLastSibling();
                    }
                }
            }

            SetUIPos();

            if (_job != null)
                _job.gameObject.SetActive(false);

            foreach (var pair in Managers.JobManager.Instance.GetAllJobTierUpgradeConditionData())
            {
                JobTierUpgradeConditionData jobTierUpgradeConditionData = pair.Value;
                if (jobTierUpgradeConditionData.OpenConditionValue == _currentStage)
                {
                    if (_job != null)
                    {
                        if (_lastWaveReward != null)
                        { 
                            _job.gameObject.SetActive(true);
                            _job.position = _lastWaveReward.transform.position;
                        }
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshAllRewardElement()
        {
            foreach (var pair in _uIGambleSynergyDetailElement_Dic)
            {
                pair.Value.SetQuestGaugeElement(pair.Key);
            }
        }
        //------------------------------------------------------------------------------------
        private void SetUIPos()
        {
            if (_currentStageInfo == null && _startPos != null)
            {
                SetCharacterPos(_startPos.localPosition);

                if (_rewardScroll != null)
                    _rewardScroll.normalizedPosition = _rewardScroll.GetNormalizedPositionToCenter(_startPos);
            }
            else
            {
                StageInfo stageInfo = _currentStageInfo;


                if (stageInfo.RecvClearReward == 0)
                {
                    SetCharacterPos(_startPos.localPosition);

                    if (_rewardScroll != null)
                        _rewardScroll.normalizedPosition = _rewardScroll.GetNormalizedPositionToCenter(_startPos);
                }
                else
                {
                    WaveClearRewardData waveClearRewardData = Managers.MapManager.Instance.GetWaveClearRewardData(stageInfo.StageNumber, stageInfo.RecvClearReward);


                    if (_uIGambleSynergyDetailElement_Dic.ContainsKey(waveClearRewardData) == true)
                    {
                        UIWaveRewardElement uIWaveRewardElement = _uIGambleSynergyDetailElement_Dic[waveClearRewardData];

                        if (uIWaveRewardElement != null)
                        {
                            SetCharacterPos(uIWaveRewardElement.transform.localPosition);

                            if (_rewardScroll != null)
                                _rewardScroll.SetNormalizedPositionToCenter(uIWaveRewardElement.transform);
                        }
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public IEnumerator SmoothScrollToCenter(float rollDuration, WaveClearRewardData waveClearRewardData)
        {
            if (_characterRoot == null || _rewardScroll == null || waveClearRewardData == null)
            {
                SetUIPos();
                yield break;
            }

            UIManager.Instance.IgnoreEnterDialog = true;

            Vector3 start = _characterRoot.localPosition;
            Vector3 end = start;

            if (_uIGambleSynergyDetailElement_Dic.TryGetValue(waveClearRewardData, out var uIWaveRewardElement) && uIWaveRewardElement != null)
            {
                end = uIWaveRewardElement.transform.localPosition;
            }

            if (_rewardScroll != null)
                _rewardScroll.enabled = false;

            _rewardScroll.SetNormalizedPositionToCenter(_characterRoot);

            if (_bubbleGroup != null)
                _bubbleGroup.gameObject.SetActive(false);

            float time = 0f;
            float duration = 0f;

            if (_skeletonGraphic != null)
            {
                _skeletonGraphic.AnimationState.ClearTrack(0); // 기존 애니 제거
                _skeletonGraphic.AnimationState.SetAnimation(0, "Roll_Start", false);
            }

            time = 0f;
            duration = _characterRollStartDelay;

            while (time < duration)
            {
                time += Time.deltaTime;
                yield return null;
            }

            if (_skeletonGraphic != null)
            {
                _skeletonGraphic.AnimationState.ClearTrack(0);
                _skeletonGraphic.AnimationState.SetAnimation(0, _characterMoveAnimationName, true);
                //_skeletonGraphic.AnimationState.SetAnimation(0, "Run", true);
            }

            time = 0f;
            duration = rollDuration;

            while (time < duration)
            {
                time += Time.deltaTime;
                _characterRoot.localPosition = Vector3.Lerp(start, end, time / duration);
                _rewardScroll.SetNormalizedPositionToCenter(_characterRoot);
                yield return null;
            }

            if (_skeletonGraphic != null)
            {
                _skeletonGraphic.AnimationState.ClearTrack(0);
                _skeletonGraphic.AnimationState.SetAnimation(0, "Roll_End", false);
            }

            time = 0f;
            duration = _characterRollEndDelay;

            while (time < duration)
            {
                time += Time.deltaTime;
                yield return null;
            }

            if (_rewardScroll != null)
                _rewardScroll.enabled = true;

            UIManager.Instance.IgnoreEnterDialog = false;

            if (_skeletonGraphic != null)
            {
                _skeletonGraphic.AnimationState.ClearTrack(0);
                _skeletonGraphic.AnimationState.SetAnimation(0, "Idle", true);

                // 스킨 꼬임 방지용 포즈 재설정 (중요!)
                _skeletonGraphic.Skeleton.SetSlotsToSetupPose();
            }

            RefreshSkin();

            ShowBubble();

            if (Managers.MapManager.Instance.RecvWaveClearReward(waveClearRewardData))
            {
                RefreshAllRewardElement();

                if (waveClearRewardData.StageNumber == 0)
                    Managers.GuideInteractorManager.Instance.StartGuideInteractor(V2Enum_EventType.NextStage);
            }

            SetUIPos();
            _characterMoveDirectionCoroutine = null;
        }
        //------------------------------------------------------------------------------------
        private void ShowBubble()
        {
            if (MapContainer.MaxWaveClear <= 100)
            {
                if (_bubbleGroup != null)
                    _bubbleGroup.gameObject.SetActive(false);
                return;
            }

            if (_bubbleGroup != null)
                _bubbleGroup.gameObject.SetActive(true);

            
        }
        //------------------------------------------------------------------------------------
        private void SetCharacterPos(Vector3 localpos)
        {
            if (_characterRoot != null)
                _characterRoot.transform.localPosition = localpos;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_WaveClearReward(WaveClearRewardData waveClearRewardData)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.WaveReward
                && Managers.MapManager.Instance.NeedTutotial1() == true)
                ThirdPartyLog.Instance.SendLog_log_tutorial(3);

            if (_characterMoveDirectionCoroutine != null)
                return;

            Vector3 start = _characterRoot.localPosition;
            Vector3 end = start;

            if (_uIGambleSynergyDetailElement_Dic.TryGetValue(waveClearRewardData, out var uIWaveRewardElement) && uIWaveRewardElement != null)
            {
                end = uIWaveRewardElement.transform.localPosition;
            }

            float roll = _characterMoveDuration * MathDatas.GetDistance(start, end);

            _characterMoveDirectionCoroutine = StartCoroutine(SmoothScrollToCenter(roll, waveClearRewardData));
            SaftyUnLockUIIgnore(roll + _characterRollStartDelay + _characterRollEndDelay).Forget();
        }
        //------------------------------------------------------------------------------------
        private async UniTask SaftyUnLockUIIgnore(float unlocktime)
        {
            await UniTask.WaitForSeconds(unlocktime);

            UIManager.Instance.IgnoreEnterDialog = false;
        }
        //------------------------------------------------------------------------------------
    }
}