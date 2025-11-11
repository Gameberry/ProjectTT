using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace GameBerry.UI
{
    public class UIGambleSynergyViewElement : MonoBehaviour
    {
        [SerializeField]
        private Image _gambleSynergyIcon;

        [SerializeField]
        private TMP_Text _gambleSynergyLevel;

        [SerializeField]
        private Image _gambleSynergyLevelBG;

        [SerializeField]
        private List<Image> _gambleSynergyLevelStack = new List<Image>();

        [SerializeField]
        private Transform _synergySkillIconGroup;

        [SerializeField]
        private Image _synergySkillIcon;

        [SerializeField]
        private Button _showGambleSynergyDetail;

        [SerializeField]
        private TMP_Text _gambleSynergyStack;

        [SerializeField]
        private Image _gauge_BG;

        [SerializeField]
        private RectTransform _gauge_RectTrans;

        [SerializeField]
        private Image _gauge_Image;

        [SerializeField]
        private Image _gauge_SpotImage;

        [SerializeField]
        private float _gauge_MaxWeight;

        [SerializeField]
        private Image _redDot;

        [SerializeField]
        private Transform _addSkillInteraction;

        [SerializeField]
        private List<DOTweenAnimation> _addSkillInteractionTweens = new List<DOTweenAnimation>();

        [SerializeField]
        private Image _skillIcon;


        [SerializeField]
        private Transform _addDescendStack;

        [SerializeField]
        private List<DOTweenAnimation> _addDescendStackTweens = new List<DOTweenAnimation>();

        [SerializeField]
        private TMP_Text _addDescendStackText;

        private V2Enum_ARR_SynergyType _synergyType = V2Enum_ARR_SynergyType.Max;

        private int _uILevel = 0;
        private float _uIGauge = 0.0f;
        private int _uICurrStack = 0;
        private int _uITargetStack = 0;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (_showGambleSynergyDetail != null)
                _showGambleSynergyDetail.onClick.AddListener(OnClick_ShowSynergyDetail);
        }
        //------------------------------------------------------------------------------------
        public void RefreshSkillIcon()
        {
            return;
            SynergyEffectData refreshiconeffect = Managers.SynergyManager.Instance.GetCurrentSynergyEffectData(_synergyType);
            if (refreshiconeffect != null)
            {
                if (_synergySkillIconGroup != null)
                    _synergySkillIconGroup.gameObject.SetActive(true);

                if (_synergySkillIcon != null)
                    _synergySkillIcon.sprite = Managers.SynergyManager.Instance.GetSynergySprite(refreshiconeffect);
            }
            else
            {
                if (_synergySkillIconGroup != null)
                    _synergySkillIconGroup.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        public void SetSynergyListData(V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType)
        {
            GambleCardSprite gambleCardSprite = StaticResource.Instance.GetGambleCardSpriteData(v2Enum_ARR_SynergyType);
            if (gambleCardSprite != null)
            {
                if (_gambleSynergyIcon != null)
                    _gambleSynergyIcon.sprite = gambleCardSprite.SynergyIcon;

                if (_gambleSynergyLevelBG != null)
                    _gambleSynergyLevelBG.color = gambleCardSprite.CircleBg;

                for (int i = 0; i < _gambleSynergyLevelStack.Count; ++i)
                {
                    Image image = _gambleSynergyLevelStack[i];
                    image.color = gambleCardSprite.Bar;
                }

                if (_gauge_BG != null)
                    _gauge_BG.color = gambleCardSprite.CircleBg;
                
                if (_gauge_Image != null)
                    _gauge_Image.color = gambleCardSprite.Bar;

                if (_gauge_SpotImage != null)
                    _gauge_SpotImage.color = gambleCardSprite.SpotBar;
            }



            _synergyType = v2Enum_ARR_SynergyType;

            RefreshSynergyInfo();
        }
        //------------------------------------------------------------------------------------
        public void HideInteractionUI()
        {
            if (_redDot != null)
                _redDot.gameObject.SetActive(false);

            if (_addSkillInteraction != null)
                _addSkillInteraction.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        public float upDuration = 0.1f;
        public float afterDuration = 0.15f;

        public async UniTask Explosion(
            SynergyEffectData beforeData, int beforeStack
            , SynergyEffectData afterData, int afterStack, int descendEnhance)
        {
            if (beforeData != null && beforeData.NextEffectData == null)
                return;

            int stackGab = afterStack - beforeStack;

            float oneStackDuration = 0.1f;


            float duration = upDuration;
            float startTime = Time.time;
            float endTime = Time.time + duration;

            if (beforeData != afterData)
            { // ·¦¾÷ Çß´Ù
                duration = upDuration;
                startTime = Time.time;
                endTime = Time.time + duration;

                float remainGauge = 1.0f - _uIGauge;
                int remainStack = _uITargetStack - _uICurrStack;

                while (Time.time < endTime)
                {
                    float ratio = (Time.time - startTime) / duration;

                    SetGauge((ratio * remainGauge) + _uIGauge);

                    int desplayCurrStack = (int)(remainStack * ratio) + _uICurrStack;

                    if (_gambleSynergyStack != null)
                        _gambleSynergyStack.SetText("{0:0}/{1}", desplayCurrStack, _uITargetStack);

                    await UniTask.Yield();
                }

                _uILevel = afterData.SynergyTier;

                SetLevel(_uILevel);

                if (_gambleSynergyIcon != null)
                    _gambleSynergyIcon.color = Color.white;

                if (afterData.NextEffectData == null || afterData.NextEffectData.SynergyTier > Managers.SynergyManager.Instance.GetInGameSynergyUnlockTier(_synergyType))
                {
                    if (_gambleSynergyStack != null)
                        _gambleSynergyStack.SetText("Max({0})", _uICurrStack);

                    _uICurrStack = -1;
                    _uITargetStack = -1;
                    _uIGauge = 1.0f;

                    SetGauge(_uIGauge);
                }
                else
                {
                    _uICurrStack = _uITargetStack;
                    _uITargetStack = afterData.NextEffectData.SynergyCount;

                    SetGauge(_uIGauge);

                    _uIGauge = 0.0f;

                    if (_gambleSynergyStack != null)
                        _gambleSynergyStack.SetText("{0}/{1}", _uICurrStack, _uITargetStack);
                }


                if (_redDot != null)
                    _redDot.gameObject.SetActive(true);

                PlayAddSkill(afterData);
            }



            SynergyEffectData gambleSynergyEffectData = afterData;

            int stack = afterStack;

            SynergyEffectData nextEffectData = null;
            if (gambleSynergyEffectData != null)
            {
                nextEffectData = gambleSynergyEffectData.NextEffectData;
            }
            else
            {
                nextEffectData = Managers.SynergyManager.Instance.GetFirstSynergy(_synergyType);
            }

            if (nextEffectData != null && nextEffectData.SynergyTier <= Managers.SynergyManager.Instance.GetInGameSynergyUnlockTier(_synergyType))
            {
                int current = 0;
                int targetstack = 1;
                if (gambleSynergyEffectData != null)
                {
                    current = stack - gambleSynergyEffectData.SynergyCount;
                    targetstack = nextEffectData.SynergyCount - gambleSynergyEffectData.SynergyCount;
                }
                else
                {
                    current = stack;
                    targetstack = nextEffectData.SynergyCount;
                }

                float targetGauge = (float)current / (float)targetstack;
                float targetGaugeGab = targetGauge - _uIGauge;

                int remainStack = afterStack - _uICurrStack;


                duration = afterDuration;
                startTime = Time.time;
                endTime = Time.time + duration;

                while (Time.time < endTime)
                {
                    float ratio = (Time.time - startTime) / duration;

                    SetGauge((ratio * targetGaugeGab) + _uIGauge);

                    int desplayCurrStack = (int)(remainStack * ratio) + _uICurrStack;

                    if (_gambleSynergyStack != null)
                        _gambleSynergyStack.SetText("{0}/{1}", desplayCurrStack, _uITargetStack);

                    await UniTask.Yield();
                }

                _uIGauge = targetGauge;
                _uICurrStack = afterStack;

                _uITargetStack = nextEffectData.SynergyCount;

                if (_gambleSynergyStack != null)
                    _gambleSynergyStack.SetText("{0}/{1}", _uICurrStack, _uITargetStack);

                SetGauge(_uIGauge);
            }
            else
            {
                if (_gambleSynergyStack != null)
                    _gambleSynergyStack.SetText("Max({0})", stack);

                _uIGauge = 1.0f;
                _uICurrStack = -1;
                _uITargetStack = -1;

                SetGauge(1.0f);
            }

            if (descendEnhance > 0)
                PlayAddDescendStack(descendEnhance);


            //SynergyEffectData refreshiconeffect = Managers.SynergyManager.Instance.GetCurrentSynergyEffectData(_synergyType);
            //if (refreshiconeffect != null)
            //{
            //    if (_synergySkillIconGroup != null)
            //        _synergySkillIconGroup.gameObject.SetActive(true);

            //    if (_synergySkillIcon != null)
            //        _synergySkillIcon.sprite = Managers.SynergyManager.Instance.GetSynergySprite(refreshiconeffect);
            //}
            //else
            //{
            //    if (_synergySkillIconGroup != null)
            //        _synergySkillIconGroup.gameObject.SetActive(false);
            //}
        }
        //------------------------------------------------------------------------------------
        public void RefreshSynergyInfo()
        {
            SynergyEffectData gambleSynergyEffectData = Managers.SynergyManager.Instance.GetCurrentSynergyEffectData(_synergyType);
            

            int level = Managers.SynergyManager.Instance.GetSynergyLevel(_synergyType);
            int stack = Managers.SynergyManager.Instance.GetSynergyStack(_synergyType);



            SynergyEffectData nextEffectData = null;
            if (gambleSynergyEffectData != null)
            {
                if (_gambleSynergyIcon != null)
                    _gambleSynergyIcon.color = Color.white;

                nextEffectData = gambleSynergyEffectData.NextEffectData;

                //if (_synergySkillIconGroup != null)
                //    _synergySkillIconGroup.gameObject.SetActive(true);

                //if (_synergySkillIcon != null)
                //    _synergySkillIcon.sprite = Managers.SynergyManager.Instance.GetSynergySprite(gambleSynergyEffectData);
            }
            else
            {
                if (_gambleSynergyIcon != null)
                    _gambleSynergyIcon.color = Color.gray;

                nextEffectData = Managers.SynergyManager.Instance.GetFirstSynergy(_synergyType);

                //if (_synergySkillIconGroup != null)
                //    _synergySkillIconGroup.gameObject.SetActive(false);
            }

            SetLevel(level);

            _uILevel = level;

            if (nextEffectData != null && nextEffectData.SynergyTier <= Managers.SynergyManager.Instance.GetInGameSynergyUnlockTier(_synergyType))
            {
                if (_gambleSynergyStack != null)
                    _gambleSynergyStack.SetText("{0}/{1}", stack, nextEffectData.SynergyCount);

                int current = 0;
                int targetstack = 1;
                if (gambleSynergyEffectData != null)
                {
                    current = stack - gambleSynergyEffectData.SynergyCount;
                    targetstack = nextEffectData.SynergyCount - gambleSynergyEffectData.SynergyCount;
                }
                else
                { 
                    current = stack;
                    targetstack = nextEffectData.SynergyCount;
                }


                _uIGauge = (float)current / (float)targetstack;
                _uICurrStack = stack;
                _uITargetStack = nextEffectData.SynergyCount;

                SetGauge(_uIGauge);
            }
            else
            {
                if (_gambleSynergyStack != null)
                    _gambleSynergyStack.SetText("Max({0})", stack);

                _uIGauge = 1.0f;
                _uICurrStack = -1;
                _uITargetStack = -1;

                SetGauge(1.0f);
            }
        }
        //------------------------------------------------------------------------------------
        private void SetLevel(int level)
        {
            if (_gambleSynergyLevel != null)
                _gambleSynergyLevel.SetText("Lv.{0}", level);

            for (int i = 0; i < _gambleSynergyLevelStack.Count; ++i)
            {
                _gambleSynergyLevelStack[i].gameObject.SetActive(i < level);
            }
        }
        //------------------------------------------------------------------------------------
        private void PlayAddSkill(SynergyEffectData gambleSynergyEffectData)
        {
            if (_addSkillInteraction != null)
                _addSkillInteraction.gameObject.SetActive(true);

            for (int i = 0; i < _addSkillInteractionTweens.Count; ++i)
            {
                if (_addSkillInteractionTweens[i] != null)
                {
                    _addSkillInteractionTweens[i].DORewind();
                    _addSkillInteractionTweens[i].DORestart();
                }
            }

            if (_skillIcon != null)
                _skillIcon.sprite = Managers.SkillManager.Instance.GetMainSkillIcon(gambleSynergyEffectData.SynergySkillData);
        }
        //------------------------------------------------------------------------------------
        public void PlayAddDescendStack(int descendStack)
        {
            if (_addDescendStack != null)
                _addDescendStack.gameObject.SetActive(true);

            for (int i = 0; i < _addDescendStackTweens.Count; ++i)
            {
                if (_addDescendStackTweens[i] != null)
                {
                    _addDescendStackTweens[i].DORewind();
                    _addDescendStackTweens[i].DORestart();
                }
            }

            if (_addDescendStackText != null)
                _addDescendStackText.SetText("+{0}", descendStack);
        }
        //------------------------------------------------------------------------------------
        private void SetGauge(float ratio)
        {
            if (_gauge_RectTrans != null)
            {
                if (ratio > 1.0f)
                    ratio = 1.0f;

                Vector2 gaugeSize = _gauge_RectTrans.sizeDelta;
                gaugeSize.x = _gauge_MaxWeight * ratio;
                _gauge_RectTrans.sizeDelta = gaugeSize;
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ShowSynergyDetail()
        {
            Managers.SynergyManager.Instance.ShowSynergyDetailPopup(_synergyType);

            if (_redDot != null)
                _redDot.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
    }
}