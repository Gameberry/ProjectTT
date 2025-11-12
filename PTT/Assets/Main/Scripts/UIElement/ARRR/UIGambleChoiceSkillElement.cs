using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;

namespace GameBerry.UI
{
    [System.Serializable]
    public class UIGambleGradePositionData
    {
        public int Stack;
        public Transform Root;
        public List<Image> Images = new List<Image>();
    }

    public class UIGambleChoiceSkillElement : MonoBehaviour
    {
        [SerializeField]
        private Image _gambleSynergyIcon;

        [SerializeField]
        private Image _gambleCardIcon_s;


        [SerializeField]
        private Image _gambleCardGrade;

        [SerializeField]
        private TMP_Text _gambleCardGrade_Text;

        [SerializeField]
        private Image _gambleCardTypeImage;

        [SerializeField]
        private Image _gambleCardSymbolImage;

        [SerializeField]
        private TMP_Text _gambleCardBackGrade_Text;

        [SerializeField]
        private ParticleSystem _gambleCardBackGrade_ChangeEffect;

        [SerializeField]
        private ParticleSystem _gambleCardBackGrade_ConfirmEffect;

        [SerializeField]
        private Image _gambleCardTypeFrontImage;

        [SerializeField]
        private Transform _gambleCardSkillNameGroup;

        [SerializeField]
        private TMP_Text _gambleCardSkillName;

        [SerializeField]
        private Transform _gambleCardSkillIcon_Group;

        [SerializeField]
        private Image _gambleCardSkillIcon;

        [SerializeField]
        private Transform _gambleCardSkillDesc_Group;

        [SerializeField]
        private TMP_Text _gambleCardSkillDesc;

        [SerializeField]
        private Image _gambleCardPickOutLine;

        [SerializeField]
        private Button _gambleCardSelectBtn;

        private ARR_CardGambleData _currentGambleSkillData = null;

        [SerializeField]
        private List<UIGambleCardGradeEffect> _uIGambleCardGradeEffects;

        [SerializeField]
        private List<UIGambleGradePositionData> _uIGambleGradePositionData = new List<UIGambleGradePositionData>();

        [SerializeField]
        private Transform _joker;

        [SerializeField]
        private Transform _canLevelUpGroup;

        [SerializeField]
        private Image _getskillIcon;

        [SerializeField]
        private Button _canLevelUpButton;

        private int _index = -1;

        private System.Action<UIGambleChoiceSkillElement> _callback;

        private Vector3 _descOriginPosition;

        //------------------------------------------------------------------------------------
        public void Init(System.Action<UIGambleChoiceSkillElement> action)
        {
            if (_gambleCardSelectBtn != null)
                _gambleCardSelectBtn.onClick.AddListener(Click_Skill);

            if (_canLevelUpButton != null)
                _canLevelUpButton.onClick.AddListener(OnClick_CanLevelUpBtn);

            _callback = action;

            //if (_gambleCardSkillDesc_Group != null)
            //    _descOriginPosition = _gambleCardSkillDesc_Group.transform.localPosition;
        }
        //------------------------------------------------------------------------------------
        public void SetIndex(int index)
        {
            _index = index;
        }
        //------------------------------------------------------------------------------------
        public void SetElement(Enum_SynergyType Enum_Card, V2Enum_Grade V2Enum_Grade, MainSkillData gambleSkillData)
        {
            GambleCardSprite gambleCardSprite = StaticResource.Instance.GetGambleCardSpriteData(Enum_Card);
            //if (_gambleCardTypeImage != null)
            //    _gambleCardTypeImage.sprite = gambleCardSprite.CardSprite;

            //if (_gambleCardTypeFrontImage != null)
            //    _gambleCardTypeFrontImage.sprite = gambleCardSprite.CardFrontSprite;

            //if (_gambleCardSymbolImage != null)
            //    _gambleCardSymbolImage.sprite = gambleCardSprite.CardSymbol;

            //if (_gambleSynergyIcon != null)
            //    _gambleSynergyIcon.sprite = gambleCardSprite.SynergyIcon;

            //string cardSkill = gambleSkillData.Index.GetDecrypted().ToString();

            //if (_gambleCardSkillName != null)
            //    Managers.LocalStringManager.Instance.SetLocalizeText(_gambleCardSkillName, gambleSkillData.NameLocalKey);

            //if(_gambleCardSkillDesc != null)
            //    Managers.LocalStringManager.Instance.SetLocalizeText(_gambleCardSkillDesc, gambleSkillData.DescLocalKey);

            //if (_gambleCardSkillIcon != null)
            //    _gambleCardSkillIcon.sprite = Managers.GambleManager.Instance.GetGambleSkillIcon(gambleSkillData);

            //SetGrade(V2Enum_Grade);

            //EnablePickBtn(false);

            //SetStack(gambleSkillData.SynergyStack, gambleCardSprite.SynergyIcon);

            //_currentGambleSkillData = gambleSkillData;
        }
        //------------------------------------------------------------------------------------
        public void SetElement(ARR_CardGambleData gambleSkillData)
        {
            GambleCardSprite gambleCardSprite = StaticResource.Instance.GetGambleCardSpriteData(gambleSkillData.SynergyType);
            EnablePickBtn(false);

            _currentGambleSkillData = gambleSkillData;

            if (gambleSkillData.SynergyType == Enum_SynergyType.Max)
            {
                if (_gambleCardTypeImage != null)
                    _gambleCardTypeImage.color = Color.yellow;

                EnableJoker(true);
                SetStack(0, null);
            }
            else
            {
                if (_gambleCardTypeImage != null)
                    _gambleCardTypeImage.color = Color.white;

                EnableJoker(false);
                SetStack(gambleSkillData.SynergyStack, gambleCardSprite.SynergyIcon);
            }
        }
        //------------------------------------------------------------------------------------
        public void ShowCanLevelUp()
        {
            if (Managers.MapManager.Instance.NeedTutotial1() == true && Managers.GambleManager.Instance.GetGambleActionCount(Enum_GambleType.Card) == 1)
            {
                if (_canLevelUpGroup != null)
                    _canLevelUpGroup.gameObject.SetActive(false);
                return;
            }

            if (_currentGambleSkillData != null)
            {
                if (_currentGambleSkillData.SynergyType == Enum_SynergyType.Max)
                {
                    if (_canLevelUpGroup != null)
                        _canLevelUpGroup.gameObject.SetActive(false);
                    return;
                }
            }

            SynergyEffectData synergyEffectData = Managers.SynergyManager.Instance.CanSynergyLevelUp(_currentGambleSkillData);

            if (_canLevelUpGroup != null)
                _canLevelUpGroup.gameObject.SetActive(synergyEffectData != null);

            if (synergyEffectData != null)
            {
                if (_getskillIcon != null)
                    _getskillIcon.sprite = Managers.SynergyManager.Instance.GetSynergySprite(synergyEffectData);
            }
            
        }
        //------------------------------------------------------------------------------------
        public void SetGrade(V2Enum_Grade V2Enum_Grade)
        {
            GambleGradeBGSprite gambleGradeBGSprite = StaticResource.Instance.GetGambleGradeBGSpriteData(V2Enum_Grade);
            if (_gambleCardGrade != null)
                _gambleCardGrade.sprite = gambleGradeBGSprite.GradeSprite;

            if (_gambleCardGrade_Text != null)
                _gambleCardGrade_Text.SetText(V2Enum_Grade.ToString());

            if (_gambleCardBackGrade_Text != null)
                _gambleCardBackGrade_Text.SetText(V2Enum_Grade.ToString());

            if (_gambleCardIcon_s != null)
                _gambleCardIcon_s.sprite = gambleGradeBGSprite.GradeSprite;

            EnableGradeParticle(V2Enum_Grade);
        }
        //------------------------------------------------------------------------------------
        public void EnableCardGrade(bool enable)
        {
            if (_gambleCardGrade != null)
                _gambleCardGrade.gameObject.SetActive(enable);
        }
        //------------------------------------------------------------------------------------
        public Transform GetCardGrade()
        {
            if (_gambleCardGrade != null)
                return _gambleCardGrade.transform;

            return null;
        }
        //------------------------------------------------------------------------------------
        public void EnableCardType(bool enable)
        {
            if (_gambleCardTypeImage != null)
                _gambleCardTypeImage.gameObject.SetActive(enable);
        }
        //------------------------------------------------------------------------------------
        public void EnableCardSymbol(bool enable)
        {
            if (_gambleCardSymbolImage != null)
                _gambleCardSymbolImage.gameObject.SetActive(enable);
        }
        //------------------------------------------------------------------------------------
        public Transform GetCardSymbolTrans()
        {
            if (_gambleCardSymbolImage != null)
                return _gambleCardSymbolImage.transform;

            return null;
        }
        //------------------------------------------------------------------------------------
        public void PlayChangeGradeParticle()
        {
            if (_gambleCardBackGrade_ChangeEffect != null)
            { 
                _gambleCardBackGrade_ChangeEffect.gameObject.SetActive(true);
                _gambleCardBackGrade_ChangeEffect.Stop();
                _gambleCardBackGrade_ChangeEffect.Play();
            }
        }
        //------------------------------------------------------------------------------------
        public void PlayConfirmGradeParticle()
        {
            if (_gambleCardBackGrade_ConfirmEffect != null)
            {
                _gambleCardBackGrade_ConfirmEffect.gameObject.SetActive(true);
                _gambleCardBackGrade_ConfirmEffect.Stop();
                _gambleCardBackGrade_ConfirmEffect.Play();
            }
        }
        //------------------------------------------------------------------------------------
        public void EnableSkillCard(bool enable)
        {
            //if (_gambleCardSkillNameGroup != null)
            //    _gambleCardSkillNameGroup.gameObject.SetActive(enable);

            //if (_gambleCardSkillIcon_Group != null)
            //    _gambleCardSkillIcon_Group.gameObject.SetActive(enable);

            //if (_gambleCardSkillIcon != null)
            //    _gambleCardSkillIcon.gameObject.SetActive(enable);

            ////if (_gambleCardTypeFrontImage != null)
            ////    _gambleCardTypeFrontImage.gameObject.SetActive(enable);

            //if (_gambleCardSkillDesc_Group != null)
            //    _gambleCardSkillDesc_Group.gameObject.SetActive(enable);
        }
        //------------------------------------------------------------------------------------
        public Vector3 GetOriginDescGroupPosition()
        {
            return _descOriginPosition;
        }
        //------------------------------------------------------------------------------------
        public Transform GetDescGroupTrans()
        {
            if (_gambleCardSkillDesc_Group != null)
                return _gambleCardSkillDesc_Group.transform;

            return null;
        }
        //------------------------------------------------------------------------------------
        public void EnablePickBtn(bool enable)
        {
            if (_gambleCardSelectBtn != null)
                _gambleCardSelectBtn.gameObject.SetActive(enable);
        }
        //------------------------------------------------------------------------------------
        public void EnablePickCard(bool enable)
        {
            if (_gambleCardPickOutLine != null)
                _gambleCardPickOutLine.gameObject.SetActive(enable);
        }
        //------------------------------------------------------------------------------------
        public void Click_Skill()
        {
            _callback?.Invoke(this);
        }
        //------------------------------------------------------------------------------------
        public ARR_CardGambleData GetGambleSkillData()
        {
            return _currentGambleSkillData;
        }
        //------------------------------------------------------------------------------------
        public void EnableGradeParticle(V2Enum_Grade V2Enum_Grade)
        {
            for (int i = 0; i < _uIGambleCardGradeEffects.Count; ++i)
            {
                if (_uIGambleCardGradeEffects[i] == null)
                    continue;

                if (_uIGambleCardGradeEffects[i].Effects != null)
                    _uIGambleCardGradeEffects[i].Effects.AllSetActive(_uIGambleCardGradeEffects[i].V2Enum_Grade == V2Enum_Grade);
            }
        }
        //------------------------------------------------------------------------------------
        public void SetStack(int stack, Sprite sprite)
        {
            for (int i = 0; i < _uIGambleGradePositionData.Count; ++i)
            {
                UIGambleGradePositionData uIGambleGradePositionData = _uIGambleGradePositionData[i];

                if (uIGambleGradePositionData.Root != null)
                    uIGambleGradePositionData.Root.gameObject.SetActive(uIGambleGradePositionData.Stack == stack);

                if (uIGambleGradePositionData.Stack == stack)
                {
                    for (int j = 0; j < uIGambleGradePositionData.Images.Count; ++j)
                    {
                        uIGambleGradePositionData.Images[j].sprite = sprite;
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void EnableJoker(bool enable)
        {
            if (_joker != null)
                _joker.gameObject.SetActive(enable);
        }
        //------------------------------------------------------------------------------------
        public void OnClick_CanLevelUpBtn()
        {
            if (_currentGambleSkillData != null)
            {
                SynergyEffectData gambleSynergyEffectData = Managers.SynergyManager.Instance.GetNextSynergyData(_currentGambleSkillData.SynergyType);
                Managers.SynergyManager.Instance.ShowSynergyDetailPopup(_currentGambleSkillData.SynergyType, gambleSynergyEffectData);
            }
        }
        //------------------------------------------------------------------------------------
        public void SetTutoGambleGasSynergyPick()
        {
            UIGuideInteractor uIGuideInteractor = _gambleCardSelectBtn.gameObject.AddComponent<UIGuideInteractor>();
            uIGuideInteractor.MyGuideType = V2Enum_EventType.TutoGambleGasSynergyPick;
            uIGuideInteractor.MyStepID = 1;
            uIGuideInteractor.FocusAngle = 0;
            uIGuideInteractor.FocusParent = transform.parent.parent.parent;
            uIGuideInteractor.GuideInteratorActionType = Managers.GuideInteratorActionType.End;
            uIGuideInteractor.IsAutoSetting = false;
            uIGuideInteractor.ConnectInteractor();
        }
        //------------------------------------------------------------------------------------
    }
}