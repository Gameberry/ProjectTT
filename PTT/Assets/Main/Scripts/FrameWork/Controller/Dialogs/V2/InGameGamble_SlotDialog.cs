using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

namespace GameBerry.UI
{
    [System.Serializable]
    public class SlotStatEffect
    {
        public V2Enum_Stat v2Enum_Stat;
        public ParticleSystem Effect;
    }

    [System.Serializable]
    public class SlotGradeEffect
    {
        public Enum_GambleSlotGrade Enum_GambleSlotGrade;
        public ParticleSystem Effect;
    }

    public class InGameGamble_SlotDialog : IDialog
    {
        [SerializeField]
        private Image _gambleBackBG;

        [SerializeField]
        private Transform _gambleStatSlotRoot;

        [SerializeField]
        private Transform _gambleGradeSlotRoot;

        [SerializeField]
        private UIGambleSlotElement _uIGambleSlotElement;

        private Dictionary<V2Enum_Stat, UIGambleSlotElement> _slotStatElements = new Dictionary<V2Enum_Stat, UIGambleSlotElement>();

        private List<UIGambleSlotElement> _slotStat = new List<UIGambleSlotElement>();

        [SerializeField]
        private TMP_Text _slotStatResult;

        [SerializeField]
        private UIGambleSlotElement _uIGambleSlotElement_Grade;

        private Dictionary<Enum_GambleSlotGrade, UIGambleSlotElement> _slotGradeElements = new Dictionary<Enum_GambleSlotGrade, UIGambleSlotElement>();

        private List<UIGambleSlotElement> _slotGrade = new List<UIGambleSlotElement>();

        [SerializeField]
        private Transform _slotFrame;

        [SerializeField]
        private TMP_Text _slotGradeResult;

        [SerializeField]
        private float _slotStatGridSize = 100.0f;

        [SerializeField]
        private float _slotStatGridSpace = 10.0f;

        [SerializeField]
        private float _minPosY = 0.0f;

        [SerializeField]
        private float _maxPosY = 0.0f;
        private float _maxPosY_Stat = 0.0f;
        private float _maxPosY_Grade = 0.0f;


        [SerializeField]
        private float _minMaxPosGab = 0.0f;
        private float _minMaxPosGab_Stat = 0.0f;
        private float _minMaxPosGab_Grade = 0.0f;

        [SerializeField]
        private AnimationCurve _animationCurve;

        [SerializeField]
        private ParticleSystem _gradeUpParticle;


        [SerializeField]
        private List<SlotStatEffect> _uIGambleStatEffects;

        [SerializeField]
        private List<SlotGradeEffect> _uIGambleGradeEffects;


        [SerializeField]
        private List<SlotGradeEffect> _uIGambleGradeEffectsResult;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            List<V2Enum_Stat> v2Enum_Stats = Managers.GambleManager.Instance.GetDisplayStatList();

            for (int i = 0; i < v2Enum_Stats.Count; ++i)
            {
                V2Enum_Stat v2Enum_Stat = v2Enum_Stats[i];
                GameObject clone = Instantiate(_uIGambleSlotElement.gameObject, _gambleStatSlotRoot);

                UIGambleSlotElement uIGambleSlotElement = clone.GetComponent<UIGambleSlotElement>();
                //uIGambleSlotElement.SetText(v2Enum_Stat.ToString());
                uIGambleSlotElement.SetIcon(Managers.ARRRStatManager.Instance.GetStatSprite(v2Enum_Stat));

                uIGambleSlotElement.transform.localPosition = Vector3.zero;

                _slotStatElements.Add(v2Enum_Stat, uIGambleSlotElement);
                _slotStat.Add(uIGambleSlotElement);
            }

            for (int i = Enum_GambleSlotGrade.One.Enum32ToInt(); i < Enum_GambleSlotGrade.Max.Enum32ToInt(); ++i)
            {
                Enum_GambleSlotGrade v2Enum_Stat = i.IntToEnum32<Enum_GambleSlotGrade>();
                V2Enum_Grade v2Enum_Grade = i.IntToEnum32<V2Enum_Grade>();

                GameObject clone = Instantiate(_uIGambleSlotElement_Grade.gameObject, _gambleGradeSlotRoot);

                UIGambleSlotElement uIGambleSlotElement = clone.GetComponent<UIGambleSlotElement>();
                //uIGambleSlotElement.SetText(v2Enum_Stat.ToString());
                uIGambleSlotElement.SetIcon(StaticResource.Instance.GetV2GradeSprite(v2Enum_Grade));

                uIGambleSlotElement.transform.localPosition = Vector3.zero;

                _slotGradeElements.Add(v2Enum_Stat, uIGambleSlotElement);
                _slotGrade.Add(uIGambleSlotElement);
            }

            _minPosY = (_slotStatGridSize + _slotStatGridSpace) * -2.0f;

            _maxPosY = (_slotStatGridSize + _slotStatGridSpace) * (_slotStat.Count - 2);
            _minMaxPosGab = _maxPosY - _minPosY;


            TotalStatMoveDistance = 10 * _slotStat.Count * (_slotStatGridSize + _slotStatGridSpace);
            TotalGradeMoveDistance = 10 * _slotGrade.Count * (_slotStatGridSize + _slotStatGridSpace);

            _maxPosY_Stat = (_slotStatGridSize + _slotStatGridSpace) * (_slotStat.Count - 2);
            _maxPosY_Grade = (_slotStatGridSize + _slotStatGridSpace) * (_slotGrade.Count - 2);

            _minMaxPosGab_Stat = _maxPosY_Stat - _minPosY;
            _minMaxPosGab_Grade = _maxPosY_Grade - _minPosY;

            Message.AddListener<GameBerry.Event.PlayFreeGambleSlotMsg>(PlayFreeGambleSlot);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.PlayFreeGambleSlotMsg>(PlayFreeGambleSlot);
        }
        //------------------------------------------------------------------------------------
        public void AllHideGambleUI()
        {
            if (_gambleBackBG != null)
                _gambleBackBG.gameObject.SetActive(false);

            if (_gambleStatSlotRoot != null)
                _gambleStatSlotRoot.gameObject.SetActive(false);

            if (_gambleGradeSlotRoot != null)
                _gambleGradeSlotRoot.gameObject.SetActive(false);

            if (_slotStatResult != null)
                _slotStatResult.gameObject.SetActive(false);

            if (_slotFrame != null)
                _slotFrame.gameObject.SetActive(false);

            if (_slotGradeResult != null)
                _slotGradeResult.gameObject.SetActive(false);

            //for (int i = 0; i < _uIGambleStatEffects.Count; ++i)
            //{
            //    SlotStatEffect slotStatEffect = _uIGambleStatEffects[i];
            //    if (slotStatEffect != null && slotStatEffect.Effect != null)
            //        slotStatEffect.Effect.gameObject.SetActive(false);
            //}

            

            //for (int i = 0; i < _uIGambleGradeEffects.Count; ++i)
            //{
            //    SlotGradeEffect uIGambleCardGradeEffect = _uIGambleGradeEffects[i];
            //    if (uIGambleCardGradeEffect != null && uIGambleCardGradeEffect.Effect != null)
            //        uIGambleCardGradeEffect.Effect.gameObject.SetActive(false);
            //}

            //for (int i = 0; i < _uIGambleGradeEffectsResult.Count; ++i)
            //{
            //    SlotGradeEffect uIGambleCardGradeEffect = _uIGambleGradeEffectsResult[i];
            //    if (uIGambleCardGradeEffect != null && uIGambleCardGradeEffect.Effect != null)
            //        uIGambleCardGradeEffect.Effect.gameObject.SetActive(false);
            //}
        }
        //------------------------------------------------------------------------------------
        private void PlayFreeGambleSlot(GameBerry.Event.PlayFreeGambleSlotMsg msg)
        {
            ElementEnter();
            OnClick_PlayGamble();
        }
        //------------------------------------------------------------------------------------
        public void OnClick_PlayGamble()
        {
            //Managers.BattleSceneManager.Instance.ChangeTimeScale(Enum_BattleSpeed.Pause);

            AllHideGambleUI();

            GambleSlotIncreaseValueData gambleSlotIncreaseValueData = Managers.GambleManager.Instance.PlayGambleSlot();
            if (gambleSlotIncreaseValueData == null)
            {
                GambleResultHideDelay().Forget();
                return;
            }


            UIGambleSlotElement statelement = null;
            if (_slotStatElements.ContainsKey(gambleSlotIncreaseValueData.BaseStat) == false)
                return;

            statelement = _slotStatElements[gambleSlotIncreaseValueData.BaseStat];

            

            int statidx = _slotStat.IndexOf(statelement);


            Managers.SoundManager.Instance.PlaySound("SlotRoll");

            StartCoroutine(GambleStatSlot(statidx, gambleSlotIncreaseValueData.BaseStat));
            StartCoroutine(GambleGradeSlotCanvas(gambleSlotIncreaseValueData));
        }
        //------------------------------------------------------------------------------------
        public float StatSlotDuration = 1.0f;
        public int StatSelectIndex = 2;
        public float TotalStatMoveDistance = 3300.0f;


        public float GradeSlotDuration = 1.2f;
        public int GradeSelectIndex = 2;
        public float TotalGradeMoveDistance = 4400.0f;

        public float ResultShowDuration = 0.5f;


        //------------------------------------------------------------------------------------
        private IEnumerator GambleStatSlot(int selectIndex, V2Enum_Stat resultStat)
        {
            //if (_gambleBackBG != null)
            //    _gambleBackBG.gameObject.SetActive(true);

            if (_slotFrame != null)
                _slotFrame.gameObject.SetActive(true);

            if (_gambleStatSlotRoot != null)
                _gambleStatSlotRoot.gameObject.SetActive(true);

            float StartTime = Time.unscaledTime;
            float EndTime = Time.unscaledTime + StatSlotDuration;

            float TotalAddPos = TotalStatMoveDistance + ((_slotStatGridSize + _slotStatGridSpace) * selectIndex);
            TotalAddPos *= -1.0f;

            while (Time.unscaledTime < EndTime)
            {
                float ratio = (Time.unscaledTime - StartTime) / StatSlotDuration;
                float curv = _animationCurve.Evaluate(ratio);

                SetPositionSetting(TotalAddPos * curv, _slotStat, _minMaxPosGab_Stat);

                yield return null;
            }

            SetPositionSetting(TotalAddPos, _slotStat, _minMaxPosGab_Stat);

            if (_slotStatResult != null)
            {
                CharacterBaseStatData characterBaseStatData = Managers.ARRRStatManager.Instance.GetCharacterBaseStatData(resultStat);
                _slotStatResult.gameObject.SetActive(true);
                if (characterBaseStatData != null)
                    Managers.LocalStringManager.Instance.SetLocalizeText(_slotStatResult, characterBaseStatData.NameLocalStringKey);
            }

            for (int i = 0; i < _uIGambleStatEffects.Count; ++i)
            {
                if (_uIGambleStatEffects[i] == null)
                    continue;

                if (_uIGambleStatEffects[i].v2Enum_Stat == resultStat)
                {
                    if (_uIGambleStatEffects[i].Effect != null)
                    {
                        _uIGambleStatEffects[i].Effect.gameObject.SetActive(true);
                        _uIGambleStatEffects[i].Effect.Stop();
                        _uIGambleStatEffects[i].Effect.Play();
                    }

                    break;
                }
            }

            //GambleResultHideDelay().Forget();
        }
        //------------------------------------------------------------------------------------
        private IEnumerator GambleGradeSlotCanvas(GambleSlotIncreaseValueData gambleSlotIncreaseValueData)
        {
            //if (_gambleBackBG != null)
            //    _gambleBackBG.gameObject.SetActive(true);


            if (_slotFrame != null)
                _slotFrame.gameObject.SetActive(true);

            if (_gambleGradeSlotRoot != null)
                _gambleGradeSlotRoot.gameObject.SetActive(true);


            UIGambleSlotElement gradeelement = null;
            if (_slotGradeElements.ContainsKey(gambleSlotIncreaseValueData.GambleSlotGrade) == false)
                yield break;

            gradeelement = _slotGradeElements[gambleSlotIncreaseValueData.GambleSlotGrade];

            

            UIGambleSlotElement fakegradeelement = null;
            if (_slotGradeElements.ContainsKey(gambleSlotIncreaseValueData.FakeData.GambleSlotGrade) == false)
                yield break;

            fakegradeelement = _slotGradeElements[gambleSlotIncreaseValueData.FakeData.GambleSlotGrade];


            int gradeidx = _slotGrade.IndexOf(gradeelement);
            int fackgradeidx = _slotGrade.IndexOf(fakegradeelement);

            Managers.SoundManager.Instance.PlaySound("SlotGrade");

            if (gambleSlotIncreaseValueData.GambleSlotGrade != gambleSlotIncreaseValueData.FakeData.GambleSlotGrade)
            {
                yield return StartCoroutine(GambleGradeSlot(fackgradeidx, gambleSlotIncreaseValueData.FakeData.GambleSlotGrade, gambleSlotIncreaseValueData.FakeData.BaseStatIncreaseValue, GradeSlotDuration));

                yield return new WaitForSecondsRealtime(0.1f);
            }

            if (gambleSlotIncreaseValueData.GambleSlotGrade != gambleSlotIncreaseValueData.FakeData.GambleSlotGrade)
            {
                if (_gradeUpParticle != null)
                {
                    _gradeUpParticle.Stop();
                    _gradeUpParticle.Play();
                }

                Managers.SoundManager.Instance.PlaySound("SlotGradeUp");
            }

            yield return StartCoroutine(GambleGradeSlot(gradeidx, gambleSlotIncreaseValueData.GambleSlotGrade, gambleSlotIncreaseValueData.BaseStatIncreaseValue, GradeSlotDuration));

            for (int i = 0; i < _uIGambleGradeEffects.Count; ++i)
            {
                if (_uIGambleGradeEffects[i] == null)
                    continue;

                if (_uIGambleGradeEffects[i].Enum_GambleSlotGrade == gambleSlotIncreaseValueData.GambleSlotGrade)
                {
                    if (_uIGambleGradeEffects[i].Effect != null)
                    { 
                        _uIGambleGradeEffects[i].Effect.gameObject.SetActive(true);
                        _uIGambleGradeEffects[i].Effect.Stop();
                        _uIGambleGradeEffects[i].Effect.Play();
                    }

                    break;
                }
            }


            for (int i = 0; i < _uIGambleGradeEffectsResult.Count; ++i)
            {
                if (_uIGambleGradeEffectsResult[i] == null)
                    continue;

                if (_uIGambleGradeEffectsResult[i].Enum_GambleSlotGrade == gambleSlotIncreaseValueData.GambleSlotGrade)
                {
                    if (_uIGambleGradeEffectsResult[i].Effect != null)
                    {
                        _uIGambleGradeEffectsResult[i].Effect.gameObject.SetActive(true);
                        _uIGambleGradeEffectsResult[i].Effect.Stop();
                        _uIGambleGradeEffectsResult[i].Effect.Play();
                    }

                    yield return new WaitForSecondsRealtime(0.5f);

                    break;
                }
            }

            yield return new WaitForSecondsRealtime(0.5f);

            GambleResultHideDelay().Forget();
        }
        //------------------------------------------------------------------------------------
        private IEnumerator GambleGradeSlot(int selectIndex, Enum_GambleSlotGrade resultGrade, double value, float duration)
        {
            float StartTime = Time.unscaledTime;
            float EndTime = Time.unscaledTime + duration;

            float TotalAddPos = TotalGradeMoveDistance + ((_slotStatGridSize + _slotStatGridSpace) * selectIndex);
            TotalAddPos *= -1.0f;

            while (Time.unscaledTime < EndTime)
            {
                float ratio = (Time.unscaledTime - StartTime) / duration;
                float curv = _animationCurve.Evaluate(ratio);

                SetPositionSetting(TotalAddPos * curv, _slotGrade, _minMaxPosGab_Grade);

                yield return null;
            }

            SetPositionSetting(TotalAddPos, _slotGrade, _minMaxPosGab_Grade);

            //if (_slotGradeResult != null)
            //{
            //    _slotGradeResult.gameObject.SetActive(true);
            //    _slotGradeResult.SetText(resultGrade.ToString());
            //}

            yield return new WaitForSecondsRealtime(0.5f);

            if (_slotGradeResult != null)
            {
                _slotGradeResult.gameObject.SetActive(true);
                _slotGradeResult.SetText(string.Format("+{0}%", value * 100));
            }

            //if (value > 0)
            //{

                
            //}
            //else
            //    yield return new WaitForSecondsRealtime(0.1f);
        }
        //------------------------------------------------------------------------------------
        private void SetPositionSetting(float addPos, List<UIGambleSlotElement> images, float minMaxPosGab)
        {
            float gradeTurm = (_slotStatGridSize + _slotStatGridSpace);

            for (int i = 0; i < images.Count; ++i)
            {
                float origonYPos = gradeTurm * i;

                float newpos = origonYPos + addPos;

                while (_minPosY > newpos)
                    newpos += minMaxPosGab;

                Vector3 pos = images[i].transform.localPosition;
                pos.y = newpos;
                images[i].transform.localPosition = pos;
            }
        }
        //------------------------------------------------------------------------------------
        public async UniTask GambleResultHideDelay()
        {
            //_slotStatResult //_slotGradeResult
            if (_slotStatResult != null && _slotGradeResult != null)
            {
                Contents.GlobalContent.ShowGlobalNotice(string.Format("{0} {1}",
                    _slotStatResult.text, _slotGradeResult.text));
            }

            int delay = (int)(1000.0f * ResultShowDuration);

            await UniTask.Delay(delay, DelayType.UnscaledDeltaTime);

            AllHideGambleUI();

            //Managers.BattleSceneManager.Instance.ChangeOriginBattleSpeed();
            UIManager.DialogExit<InGameGamble_SlotDialog>();
        }
        //------------------------------------------------------------------------------------
    }
}