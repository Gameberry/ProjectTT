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
    public class UIRelicElement : MonoBehaviour
    {
        [SerializeField]
        private Image _synergyEffectIcon;

        [SerializeField]
        private Image _synergySelect;

        [SerializeField]
        private Button _synergyEffectClickBtn;

        [SerializeField]
        private Transform _synergyLock;

        [SerializeField]
        private TMP_Text _synergyEffectLevel;

        [SerializeField]
        private Transform _synergyEffectReddot;

        [SerializeField]
        private List<Image> _synergyEffectReddotArrow = new List<Image>();

        [SerializeField]
        private Image _synergyEffectNewdot;

        [SerializeField]
        private Image _synergyBar;

        [SerializeField]
        private Color _synergyLimitUp;

        [SerializeField]
        private Color _synergyCanLevelUp;

        [SerializeField]
        private Color _synergyCannotLevelUp;

        [SerializeField]
        private TMP_Text _synergyEnhanceGoodsCount;

        [SerializeField]
        private UILobbyCharLvUp _uILobbyCharLvUp;


        private System.Action<RelicData> _callBack;

        private RelicData _currentDescendData;

        bool isNew = false;


        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (_synergyEffectClickBtn != null)
                _synergyEffectClickBtn.onClick.AddListener(OnClick);
        }
        //------------------------------------------------------------------------------------
        public void Init(System.Action<RelicData> action)
        {
            _callBack = action;
        }
        //------------------------------------------------------------------------------------
        public void SetDescendData(RelicData synergyEffectData)
        {
            _currentDescendData = synergyEffectData;

            SkillInfo skillInfo = Managers.RelicManager.Instance.GetSynergyEffectSkillInfo(synergyEffectData);

            if (_synergyEffectIcon != null)
            {
                _synergyEffectIcon.sprite = Managers.RelicManager.Instance.GetRelicIcon(synergyEffectData);
                //_synergyEffectIcon.color = skillInfo == null ? Color.gray : Color.white;
            }

            if (_synergyLock != null)
                _synergyLock.gameObject.SetActive(skillInfo == null);

            if (skillInfo == null)
            {
                if (_synergyEffectLevel != null)
                    _synergyEffectLevel.gameObject.SetActive(false);

                if (_synergyBar != null)
                    _synergyBar.gameObject.SetActive(false);

                if (_synergyEnhanceGoodsCount != null)
                    _synergyEnhanceGoodsCount.SetText(string.Empty);

                if (_synergyEffectReddot != null)
                    _synergyEffectReddot.gameObject.SetActive(false);
            }
            else
            {
                if (_synergyEffectLevel != null)
                {
                    _synergyEffectLevel.gameObject.SetActive(skillInfo.Level > 0);
                    _synergyEffectLevel.SetText("+{0}", skillInfo.Level);
                }

                if (Managers.RelicManager.Instance.IsMaxLevelSynergy(synergyEffectData) == true)
                {
                    if (_synergyBar != null)
                    {
                        _synergyBar.gameObject.SetActive(true);
                        _synergyBar.fillAmount = 1.0f;
                        _synergyBar.color = _synergyCannotLevelUp;
                    }

                    if (_synergyEnhanceGoodsCount != null)
                        _synergyEnhanceGoodsCount.SetText("MAX");

                    if (_synergyEffectReddot != null)
                        _synergyEffectReddot.gameObject.SetActive(false);
                }
                else
                {
                    float ratio = 0;

                    int curr = skillInfo.Count;
                    int goal = Managers.RelicManager.Instance.GetSynergyEnhance_NeedCount(synergyEffectData);

                    if (goal != 0)
                        ratio = (float)curr / (float)goal;

                    if (_synergyBar != null)
                    {
                        _synergyBar.gameObject.SetActive(true);
                        _synergyBar.fillAmount = ratio;
                        _synergyBar.color = curr >= goal ? _synergyCanLevelUp : _synergyCannotLevelUp;
                    }

                    if (_synergyEnhanceGoodsCount != null)
                        _synergyEnhanceGoodsCount.SetText("{0}/{1}", curr, goal);

                    if (_synergyEffectReddot != null)
                        _synergyEffectReddot.gameObject.SetActive(Managers.RelicManager.Instance.ReadySynergyEnhance(synergyEffectData));


                    for (int i = 0; i < _synergyEffectReddotArrow.Count; ++i)
                    {
                        if (_synergyEffectReddotArrow[i] != null)
                            _synergyEffectReddotArrow[i].color = _synergyCanLevelUp;
                    }
                }
            }

            isNew = Managers.RelicManager.Instance.IsNewSynergyIcon(synergyEffectData);

            if (_synergyEffectNewdot != null)
                _synergyEffectNewdot.gameObject.SetActive(isNew);
        }
        //------------------------------------------------------------------------------------
        public void EnableSelectElement(bool enable)
        {
            if (_synergySelect != null)
                _synergySelect.gameObject.SetActive(enable);
        }
        //------------------------------------------------------------------------------------
        private void OnClick()
        {
            //if (_synergyEffectReddot != null)
            //    _synergyEffectReddot.gameObject.SetActive(false);

            if (_synergyEffectNewdot != null)
                _synergyEffectNewdot.gameObject.SetActive(false);

            if (isNew == true)
            {
                Managers.RelicManager.Instance.RemoveNewIconSynergy(_currentDescendData);
                Managers.RelicManager.Instance.RefreshNewSynergyIcon();
            }

            _callBack?.Invoke(_currentDescendData);
        }
        //------------------------------------------------------------------------------------
        private float _hideEffectTime = 0.0f;
        //------------------------------------------------------------------------------------
        public void PlayLevelUpEffect()
        {
            if (_uILobbyCharLvUp != null)
            {
                _uILobbyCharLvUp.gameObject.SetActive(true);
                _uILobbyCharLvUp.PlayEffect();
                if (_hideEffectTime > Time.time)
                {
                    _hideEffectTime = Time.time + 1.0f;
                }
                else
                {
                    _hideEffectTime = Time.time + 1.0f;
                    AutoHideEffect().Forget();
                }
            }
        }
        //------------------------------------------------------------------------------------
        private async UniTask AutoHideEffect()
        {
            while (_hideEffectTime > Time.time)
            {
                await UniTask.NextFrame();
            }

            if (_uILobbyCharLvUp != null)
            {
                _uILobbyCharLvUp.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
    }
}