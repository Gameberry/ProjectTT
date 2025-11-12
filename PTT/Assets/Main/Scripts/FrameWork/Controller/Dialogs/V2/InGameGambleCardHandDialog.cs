using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.UI
{

    [System.Serializable]
    public class UIGambleCardGradeResult
    {
        public V2Enum_Grade V2Enum_Grade;
        public ParticleSystem Effect;
    }

    public class InGameGambleCardHandDialog : IDialog
    {
        [SerializeField]
        private Image _gambleGrade;

        [SerializeField]
        private List<UIGambleCardGradeResult> _uIGambleGradeEffects;

        [SerializeField]
        private Transform _uIGambleChoiceSkillElementRoot;

        [SerializeField]
        private List<UIGambleChoiceSkillElement> _uIGambleChoiceSkillElements = new List<UIGambleChoiceSkillElement>();

        private Coroutine _gambleSkillDirection = null;

        private GameBerry.Event.AddGambleSynergySlotMsg _addGambleSynergySlotMsg = new GameBerry.Event.AddGambleSynergySlotMsg();

        [SerializeField]
        private Transform _uIGambleJokerChoiceSkillElementRoot;

        [SerializeField]
        private List<UIGambleChoiceSkillElement> _uIGambleJokerChoiceSkillElements = new List<UIGambleChoiceSkillElement>();

        [SerializeField]
        private List<UIGambleChoiceSkillElement> _uIGambleJokerChoiceSkillElements_SynergyLock = new List<UIGambleChoiceSkillElement>();


        [SerializeField]
        private Transform _gasSynergyJokerBalck;

        [SerializeField]
        private TMP_Text _gasSynergyJokerBalck_Text;

        [SerializeField]
        private Transform _needSkillLevelUp;

        [Header("----------AutoPlayDirectionPos----------")]
        [SerializeField]
        private Transform _autoPlayDirectionPos;

        [Header("----------Tutorial----------")]
        [SerializeField]
        private Transform _tutorialBlack;

        [SerializeField]
        private TMP_Text _tutorialText;

        private List<ARR_CardGambleData> _jokerCardGambleData = new List<ARR_CardGambleData>();

        private Queue<ObscuredInt> _minorJokerQueue = new Queue<ObscuredInt>();

        private Enum_SynergyType _gasSynergyTutorialType = Enum_SynergyType.Max;

        private UIGambleChoiceSkillElement forceReleaseElement = null;

        private Coroutine _autoGambleSet = null;


        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            for (int i = 0; i < _uIGambleChoiceSkillElements.Count; ++i)
            {
                _uIGambleChoiceSkillElements[i].Init(OnClick_SkillCard);
            }

            for (int i = 0; i < _uIGambleGradeEffects.Count; ++i)
            {
                if (_uIGambleGradeEffects[i] == null)
                    continue;

                if (_uIGambleGradeEffects[i].Effect != null)
                {
                    _uIGambleGradeEffects[i].Effect.Stop();
                }
            }

            int elementcount = 0;

            foreach (var pair in Managers.SynergyManager.Instance.SynergySortView)
            {
                Enum_SynergyType v2Enum_Stat = pair;

                UIGambleChoiceSkillElement uIGambleChoiceSkillElement = null;

                if (_uIGambleJokerChoiceSkillElements.Count > elementcount)
                {
                    uIGambleChoiceSkillElement = _uIGambleJokerChoiceSkillElements[elementcount];
                    uIGambleChoiceSkillElement.Init(OnClick_SkillCard);

                    ARR_CardGambleData aRR_CardGambleData = new ARR_CardGambleData();
                    aRR_CardGambleData.SynergyStack = Define.JokerCardCount;
                    aRR_CardGambleData.SynergyType = v2Enum_Stat;
                    uIGambleChoiceSkillElement.SetElement(aRR_CardGambleData);

                    _jokerCardGambleData.Add(aRR_CardGambleData);
                }

                elementcount++;
            }

            AllHideGambleUI();

            Message.AddListener<GameBerry.Event.PlayGambleCardMsg>(PlayGambleCard);
            Message.AddListener<GameBerry.Event.PlayGasSynergyMsg>(PlayGasSynergy);
            Message.AddListener<GameBerry.Event.PlayForceEndGambleCardMsg>(PlayForceEndGambleCard);
            Message.AddListener<GameBerry.Event.PlayMinorJokerSynergyMsg>(PlayMinorJokerSynergy);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.PlayGambleCardMsg>(PlayGambleCard);
            Message.RemoveListener<GameBerry.Event.PlayGasSynergyMsg>(PlayGasSynergy);
            Message.RemoveListener<GameBerry.Event.PlayForceEndGambleCardMsg>(PlayForceEndGambleCard);
            Message.RemoveListener<GameBerry.Event.PlayMinorJokerSynergyMsg>(PlayMinorJokerSynergy);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            if (_needSkillLevelUp != null)
            {
                bool needlevel = false;

                for (int i = Enum_SynergyType.Red.Enum32ToInt(); i < Enum_SynergyType.Max.Enum32ToInt(); ++i)
                {
                    Enum_SynergyType v2Enum_Stat = i.IntToEnum32<Enum_SynergyType>();

                    if (v2Enum_Stat == Enum_SynergyType.Yellow)
                    {
                        if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.UnlockGoldSynergy) == false)
                        {
                            continue;
                        }
                    }

                    if (v2Enum_Stat == Enum_SynergyType.White)
                    {
                        if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.UnlockThunderSynergy) == false)
                        {
                            continue;
                        }
                    }

                    int unlocktier = Managers.SynergyManager.Instance.GetInGameSynergyUnlockTier(v2Enum_Stat);
                    int synergyLevel = Managers.SynergyManager.Instance.GetSynergyLevel(v2Enum_Stat);

                    if (synergyLevel < unlocktier)
                    {
                        needlevel = false;
                        break;
                    }

                    if (unlocktier < 5)
                        needlevel = true;
                }

                _needSkillLevelUp.gameObject.SetActive(needlevel);
            }
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (_gambleSkillDirection != null)
            {
                StopCoroutine(_gambleSkillDirection);
            }

            if (_autoGambleSet != null)
            {
                StopCoroutine(_autoGambleSet);
                _autoGambleSet = null;
            }

            AllHideGambleUI();

            if (_minorJokerQueue.Count > 0)
            {
                PlayMinorJoker(_minorJokerQueue.Dequeue());
            }

            if (Managers.GambleManager.Instance.SynergyGambleAuto == true)
                Managers.GambleManager.Instance.FinishAutoGamble();
        }
        //------------------------------------------------------------------------------------
        private void PlayGambleCard(GameBerry.Event.PlayGambleCardMsg msg)
        {
            RequestDialogEnter<InGameGambleCardHandDialog>();
            OnClick_PlayGamble(msg.aRR_GambleResultData);
        }
        //------------------------------------------------------------------------------------
        private void PlayGasSynergy(GameBerry.Event.PlayGasSynergyMsg msg)
        {
            RequestDialogEnter<InGameGambleCardHandDialog>();
            PlayGassSynergy();
        }
        //------------------------------------------------------------------------------------
        private void PlayForceEndGambleCard(GameBerry.Event.PlayForceEndGambleCardMsg msg)
        {
            if (forceReleaseElement != null)
            {
                if (_gambleSkillDirection != null)
                {
                    GambleResultHideDelay().Forget();

                    return;
                }

                OnClick_SkillCard(forceReleaseElement);
            }
        }
        //------------------------------------------------------------------------------------
        public async UniTask GambleResultHideDelay()
        {
            while (_gambleSkillDirection != null)
            {
                Debug.Log("waitJoker");
                await UniTask.NextFrame();
            }

            OnClick_SkillCard(forceReleaseElement);
        }
        //------------------------------------------------------------------------------------
        private void PlayMinorJokerSynergy(GameBerry.Event.PlayMinorJokerSynergyMsg msg)
        {
            if (_isEnter == false)
            {
                PlayMinorJoker(msg.SynergyStack);
            }
            else
                _minorJokerQueue.Enqueue(msg.SynergyStack);
        }
        //------------------------------------------------------------------------------------
        private void PlayMinorJoker(int synergyStack)
        {
            RequestDialogEnter<InGameGambleCardHandDialog>();
            AllHideGambleUI();

            if (_gambleSkillDirection != null)
            {
                StopCoroutine(_gambleSkillDirection);
            }

            _gambleSkillDirection = StartCoroutine(GambleJokerChoiceSkillDirection(synergyStack));
        }
        //------------------------------------------------------------------------------------
        public void AllHideGambleUI()
        {
            if (_gambleGrade != null)
                _gambleGrade.gameObject.SetActive(false);

            _uIGambleChoiceSkillElements.AllSetActive(false);

            if (_uIGambleChoiceSkillElementRoot != null)
                _uIGambleChoiceSkillElementRoot.gameObject.SetActive(false);

            _uIGambleJokerChoiceSkillElements.AllSetActive(false);

            if (_uIGambleJokerChoiceSkillElementRoot != null)
                _uIGambleJokerChoiceSkillElementRoot.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        public void OnClick_PlayGamble(ARR_CardGambleResultData aRR_GambleResultData)
        {
            AllHideGambleUI();

            if (_gambleSkillDirection != null)
            {
                StopCoroutine(_gambleSkillDirection);
            }

            _gambleSkillDirection = StartCoroutine(GambleChoiceSkillDirection(aRR_GambleResultData));
        }
        //------------------------------------------------------------------------------------
        public void PlayGassSynergy()
        {
            AllHideGambleUI();

            if (_gambleSkillDirection != null)
            {
                StopCoroutine(_gambleSkillDirection);
            }

            _gambleSkillDirection = StartCoroutine(GambleJokerChoiceSkillDirection(Define.GasSynergyCount));
        }
        //------------------------------------------------------------------------------------
        public float GambleCardGradeFake = 0.5f;
        public float GambleCardGradeFakeChange = 0.25f;
        public float GambleCardGradeFakeResultView = 0.5f;

        public float GambleCardGradeShow = 0.75f;

        public float GambleCardOpenTime = 0.1f;

        public float GambleSkillTimer2 = 0.25f;
        public float GambleSkillGradeOpenDelay = 0.3f;
        public float GambleSkillDescMovePos = -1.0f;
        public float GambleSkillGradeChangeDelay = 0.5f;
        public float GambleSkillGradeChangeRemainDuration = 1.0f;

        //------------------------------------------------------------------------------------
        private IEnumerator GambleChoiceSkillDirection(ARR_CardGambleResultData aRR_GambleResultData)
        {
            float StartTime = Time.unscaledTime;
            float EndTime = Time.unscaledTime + GambleSkillTimer2;
            Vector3 rot = Vector3.zero;

            for (int i = 0; i < aRR_GambleResultData.FinalSkillData.Length; ++i)
            {
                if (_uIGambleChoiceSkillElements.Count > 0)
                {
                    UIGambleChoiceSkillElement uIGambleChoiceSkillElement = _uIGambleChoiceSkillElements[i];

                    ARR_CardGambleData selectSkill = aRR_GambleResultData.FinalSkillData[i];
                    uIGambleChoiceSkillElement.gameObject.SetActive(true);
                    uIGambleChoiceSkillElement.SetIndex(i);
                    uIGambleChoiceSkillElement.SetElement(selectSkill);
                    uIGambleChoiceSkillElement.ShowCanLevelUp();
                    uIGambleChoiceSkillElement.EnablePickCard(false);

                    forceReleaseElement = uIGambleChoiceSkillElement;
                }
            }

            if (_uIGambleChoiceSkillElementRoot != null)
                _uIGambleChoiceSkillElementRoot.gameObject.SetActive(true);


            if (Managers.GambleManager.Instance.PlayedAutoGamble == false)
                yield return new WaitForSecondsRealtime(GambleSkillGradeOpenDelay);

            for (int i = 0; i < aRR_GambleResultData.FinalSkillData.Length; ++i)
            {
                if (_uIGambleChoiceSkillElements.Count > 0)
                {
                    UIGambleChoiceSkillElement uIGambleChoiceSkillElement = _uIGambleChoiceSkillElements[i];
                    uIGambleChoiceSkillElement.EnablePickBtn(true);
                }
            }


            if (Managers.MapManager.Instance.NeedTutotial1() == true)
            {
                if (Managers.GambleManager.Instance.GetGambleActionCount(Enum_GambleType.Card) == 1)
                {
                    if (_tutorialBlack != null)
                        _tutorialBlack.gameObject.SetActive(true);

                    if (_tutorialText != null)
                        Managers.LocalStringManager.Instance.SetLocalizeText(_tutorialText, "guide/gamblecard2");

                    Managers.BattleSceneManager.Instance.ChangeTimeScale(Enum_BattleSpeed.Pause);
                }

                if (Managers.GuideInteractorManager.Instance.PlayJokerTutorial == true)
                {
                    if (_tutorialBlack != null)
                        _tutorialBlack.gameObject.SetActive(true);

                    if (_tutorialText != null)
                        Managers.LocalStringManager.Instance.SetLocalizeText(_tutorialText, "guide/gamblecardjoker1");

                    Managers.BattleSceneManager.Instance.ChangeTimeScale(Enum_BattleSpeed.Pause);
                }
            }

            _gambleSkillDirection = null;

            PlayAuto(_uIGambleChoiceSkillElements);

            yield break;

            if (Managers.GambleManager.Instance.PlayedAutoGamble == true)
            {
                _autoGambleSet = StartCoroutine(PlayAutoGamble(_uIGambleChoiceSkillElements));
            }
        }
        //------------------------------------------------------------------------------------
        private IEnumerator GambleJokerChoiceSkillDirection(int synergyStack)
        {
            float StartTime = Time.unscaledTime;
            float EndTime = Time.unscaledTime + GambleSkillTimer2;
            Vector3 rot = Vector3.zero;

            _gasSynergyTutorialType = Enum_SynergyType.Max;

            if (Managers.MapManager.Instance.NeedTutotial1() == true)
            {
                if (Managers.GuideInteractorManager.Instance.PlayGasSynergyTutorial == true)
                {
                    if (Managers.SynergyManager.Instance.GetSynergyStack(Enum_SynergyType.Red) > 0)
                        _gasSynergyTutorialType = Enum_SynergyType.Red;
                    else
                        _gasSynergyTutorialType = Enum_SynergyType.Blue;

                    if (_gasSynergyJokerBalck != null)
                        _gasSynergyJokerBalck.gameObject.SetActive(true);
                }
            }


            bool needlocklist = false;

            if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.UnlockGoldSynergy) == false)
            {
                needlocklist = true;
            }

            if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.UnlockThunderSynergy) == false)
            {
                needlocklist = true;
            }

            List<UIGambleChoiceSkillElement> elementList = needlocklist == true ? _uIGambleJokerChoiceSkillElements_SynergyLock : _uIGambleJokerChoiceSkillElements;

            for (int i = 0; i < elementList.Count; ++i)
            {
                UIGambleChoiceSkillElement uIGambleChoiceSkillElement = elementList[i];

                uIGambleChoiceSkillElement.gameObject.SetActive(true);
                uIGambleChoiceSkillElement.SetIndex(i);
                uIGambleChoiceSkillElement.ShowCanLevelUp();

                ARR_CardGambleData aRR_CardGambleData = null;
                if (_jokerCardGambleData.Count > i)
                {
                    aRR_CardGambleData = _jokerCardGambleData[i];
                    aRR_CardGambleData.SynergyStack = synergyStack;
                    uIGambleChoiceSkillElement.SetElement(aRR_CardGambleData);
                    uIGambleChoiceSkillElement.ShowCanLevelUp();
                    if (_gasSynergyTutorialType == aRR_CardGambleData.SynergyType)
                    {
                        uIGambleChoiceSkillElement.SetTutoGambleGasSynergyPick();
                        uIGambleChoiceSkillElement.transform.SetAsLastSibling();
                    }

                    forceReleaseElement = uIGambleChoiceSkillElement;


                    if (aRR_CardGambleData.SynergyType == Enum_SynergyType.Yellow)
                    {
                        if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.UnlockGoldSynergy) == false)
                        {
                            uIGambleChoiceSkillElement.gameObject.SetActive(false);
                            continue;
                        }
                    }
                    else if (aRR_CardGambleData.SynergyType == Enum_SynergyType.White)
                    {
                        if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.UnlockThunderSynergy) == false)
                        {
                            uIGambleChoiceSkillElement.gameObject.SetActive(false);
                            continue;
                        }
                    }
                }

                uIGambleChoiceSkillElement.EnablePickCard(false);
            }

            if (_uIGambleJokerChoiceSkillElementRoot != null)
                _uIGambleJokerChoiceSkillElementRoot.gameObject.SetActive(true);


            if (Managers.GambleManager.Instance.PlayedAutoGamble == false)
                yield return new WaitForSecondsRealtime(GambleSkillGradeOpenDelay);


            for (int i = 0; i < elementList.Count; ++i)
            {
                UIGambleChoiceSkillElement uIGambleChoiceSkillElement = elementList[i];
                uIGambleChoiceSkillElement.EnablePickBtn(true);
            }

            if (Managers.MapManager.Instance.NeedTutotial1() == true)
            {
                if (Managers.GuideInteractorManager.Instance.PlayGasSynergyTutorial == true)
                {
                    //if (_tutorialBlack != null)
                    //    _tutorialBlack.gameObject.SetActive(true);

                    if (_gasSynergyJokerBalck_Text != null)
                        Managers.LocalStringManager.Instance.SetLocalizeText(_gasSynergyJokerBalck_Text, "guide/gassynergy2");

                    Managers.BattleSceneManager.Instance.ChangeTimeScale(Enum_BattleSpeed.Pause);

                    Managers.GuideInteractorManager.Instance.StartGuideInteractor(V2Enum_EventType.TutoGambleGasSynergyPick);
                }

                if (Managers.GuideInteractorManager.Instance.PlayJokerTutorial == true)
                {
                    if (_tutorialBlack != null)
                        _tutorialBlack.gameObject.SetActive(true);

                    if (_tutorialText != null)
                        Managers.LocalStringManager.Instance.SetLocalizeText(_tutorialText, "guide/gamblecardjoker2");

                    Managers.BattleSceneManager.Instance.ChangeTimeScale(Enum_BattleSpeed.Pause);
                }
            }

            _gambleSkillDirection = null;

            PlayAuto(elementList);

            yield break;
            if (Managers.GambleManager.Instance.PlayedAutoGamble == true)
            {
                if (_autoGambleSet != null)
                    StopCoroutine(_autoGambleSet);

                _autoGambleSet = StartCoroutine(PlayAutoGamble(elementList));
            }
        }
        //------------------------------------------------------------------------------------
        private void PlayAuto(List<UIGambleChoiceSkillElement> uIGambleChoiceSkillElements)
        {
            if (uIGambleChoiceSkillElements == null || uIGambleChoiceSkillElements.Count <= 0)
            {
                _autoGambleSet = null;
                return;
            }

            if (Managers.GambleManager.Instance.PlayedAutoGamble == true)
            {
                bool allMax = true;
                for (int i = 0; i < Managers.GambleManager.Instance.AutoGambleOrder.Count; ++i)
                {
                    Enum_SynergyType Enum_SynergyType = Managers.GambleManager.Instance.AutoGambleOrder[i];
                    if (Managers.SynergyManager.Instance.GetInGameSynergyUnlockTier(Enum_SynergyType) <= Managers.SynergyManager.Instance.GetSynergyLevel(Enum_SynergyType))
                        continue;

                    UIGambleChoiceSkillElement uIGambleChoiceSkillElement = uIGambleChoiceSkillElements.Find(x => x.gameObject.activeSelf == true
                    && x.GetGambleSkillData().SynergyType == Enum_SynergyType);
                    if (uIGambleChoiceSkillElement != null)
                    {
                        allMax = false;

                        _autoGambleSet = null;
                        OnClick_SkillCard(uIGambleChoiceSkillElement);
                        break;
                    }
                }

                if (allMax == true)
                {
                    for (int i = 0; i < Managers.GambleManager.Instance.AutoGambleOrder.Count; ++i)
                    {
                        Enum_SynergyType Enum_SynergyType = Managers.GambleManager.Instance.AutoGambleOrder[i];

                        UIGambleChoiceSkillElement uIGambleChoiceSkillElement = uIGambleChoiceSkillElements.Find(x => x.gameObject.activeSelf == true
                        && x.GetGambleSkillData().SynergyType == Enum_SynergyType);
                        if (uIGambleChoiceSkillElement != null)
                        {
                            _autoGambleSet = null;
                            OnClick_SkillCard(uIGambleChoiceSkillElement);
                            break;
                        }
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        private IEnumerator PlayAutoGamble(List<UIGambleChoiceSkillElement> uIGambleChoiceSkillElements)
        {
            if (uIGambleChoiceSkillElements == null || uIGambleChoiceSkillElements.Count <= 0)
            {
                _autoGambleSet = null;
                yield break;
            }

            float time = 0f;
            float duration = 0f;

            time = 0f;
            duration = Managers.GambleManager.Instance.AutoSelectDelay;

            while (time < duration)
            {
                time += Time.deltaTime;
                yield return null;
            }

            if (Managers.GambleManager.Instance.PlayedAutoGamble == true)
            {
                bool allMax = true;
                for (int i = 0; i < Managers.GambleManager.Instance.AutoGambleOrder.Count; ++i)
                {
                    Enum_SynergyType Enum_SynergyType = Managers.GambleManager.Instance.AutoGambleOrder[i];
                    if (Managers.SynergyManager.Instance.GetInGameSynergyUnlockTier(Enum_SynergyType) <= Managers.SynergyManager.Instance.GetSynergyLevel(Enum_SynergyType))
                        continue;

                    UIGambleChoiceSkillElement uIGambleChoiceSkillElement = uIGambleChoiceSkillElements.Find(x => x.gameObject.activeSelf == true
                    && x.GetGambleSkillData().SynergyType == Enum_SynergyType);
                    if (uIGambleChoiceSkillElement != null)
                    {
                        allMax = false;

                        _autoGambleSet = null;
                        OnClick_SkillCard(uIGambleChoiceSkillElement);
                        break;
                    }
                }

                if (allMax == true)
                {
                    for (int i = 0; i < Managers.GambleManager.Instance.AutoGambleOrder.Count; ++i)
                    {
                        Enum_SynergyType Enum_SynergyType = Managers.GambleManager.Instance.AutoGambleOrder[i];

                        UIGambleChoiceSkillElement uIGambleChoiceSkillElement = uIGambleChoiceSkillElements.Find(x => x.gameObject.activeSelf == true
                        && x.GetGambleSkillData().SynergyType == Enum_SynergyType);
                        if (uIGambleChoiceSkillElement != null)
                        {
                            _autoGambleSet = null;
                            OnClick_SkillCard(uIGambleChoiceSkillElement);
                            break;
                        }
                    }
                }
            }

            _autoGambleSet = null;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SkillCard(UIGambleChoiceSkillElement uIGambleChoiceSkillElement)
        {
            if (uIGambleChoiceSkillElement == null)
                return;

            if (_autoGambleSet != null)
            {
                StopCoroutine(_autoGambleSet);
                _autoGambleSet = null;
            }

            if (Managers.MapManager.Instance.NeedTutotial1() == true && Managers.GambleManager.Instance.GetGambleActionCount(Enum_GambleType.Card) == 1)
            {
                if (_tutorialBlack != null)
                    _tutorialBlack.gameObject.SetActive(false);

                UI.IDialog.RequestDialogEnter<UI.InGameGambleSynergyDialog>();

                Managers.BattleSceneManager.Instance.ChangeOriginBattleSpeed();

                ThirdPartyLog.Instance.SendLog_log_tutorial(0);
            }

            if (Managers.MapManager.Instance.NeedTutotial1() == true && Managers.GuideInteractorManager.Instance.PlayJokerTutorial == true)
            {
                if (_tutorialBlack != null)
                    _tutorialBlack.gameObject.SetActive(false);

                Managers.BattleSceneManager.Instance.ChangeOriginBattleSpeed();

                ThirdPartyLog.Instance.SendLog_log_tutorial(1);
            }


            ARR_CardGambleData gambleSkillData = uIGambleChoiceSkillElement.GetGambleSkillData();


            if (Managers.MapManager.Instance.NeedTutotial1() == true && Managers.GuideInteractorManager.Instance.PlayGasSynergyTutorial == true)
            {
                if (_gasSynergyTutorialType != gambleSkillData.SynergyType)
                    return;

                if (_gasSynergyJokerBalck != null)
                    _gasSynergyJokerBalck.gameObject.SetActive(false);

                UI.IDialog.RequestDialogEnter<UI.InGameGambleSynergyDialog>();

                Managers.BattleSceneManager.Instance.ChangeOriginBattleSpeed();

                ThirdPartyLog.Instance.SendLog_log_tutorial(5);
            }


            if (gambleSkillData.SynergyType == Enum_SynergyType.Max)
            {
                AllHideGambleUI();


                if (_gambleSkillDirection != null)
                {
                    StopCoroutine(_gambleSkillDirection);
                }

                _gambleSkillDirection = StartCoroutine(GambleJokerChoiceSkillDirection(Define.JokerCardCount));

                return;
            }

            _addGambleSynergySlotMsg.BeforeData = Managers.SynergyManager.Instance.GetCurrentSynergyEffectData(gambleSkillData.SynergyType);
            _addGambleSynergySlotMsg.BeforeStack = Managers.SynergyManager.Instance.GetSynergyStack(gambleSkillData.SynergyType);

            Managers.GambleManager.Instance.AddGambleSkill(gambleSkillData, ref _addGambleSynergySlotMsg.DescendEnhance);


            if (Managers.GambleManager.Instance.PlayedAutoGamble == true)
            {
                if (_autoPlayDirectionPos != null)
                    _addGambleSynergySlotMsg.UIStartPos = _autoPlayDirectionPos.position;
            }
            else
                _addGambleSynergySlotMsg.UIStartPos = uIGambleChoiceSkillElement.transform.position;

            _addGambleSynergySlotMsg.GambleSkillData = gambleSkillData;
            _addGambleSynergySlotMsg.AfterData = Managers.SynergyManager.Instance.GetCurrentSynergyEffectData(gambleSkillData.SynergyType);
            _addGambleSynergySlotMsg.AfterStack = Managers.SynergyManager.Instance.GetSynergyStack(gambleSkillData.SynergyType);

            Managers.GuideInteractorManager.Instance.PlayJokerTutorial = false;

            Message.Send(_addGambleSynergySlotMsg);

            AllHideGambleUI();

            forceReleaseElement = null;

            RequestDialogExit<InGameGambleCardHandDialog>();
        }
        //------------------------------------------------------------------------------------
    }
}