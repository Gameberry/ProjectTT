using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace GameBerry.UI
{
    public class UITrainingElement : MonoBehaviour
    {
        [SerializeField]
        private Image m_upGradeIcon;

        [SerializeField]
        private ParticleSystem m_upGradeParticle;

        [SerializeField]
        private TMP_Text m_upGradeTitle;

        [SerializeField]
        private TMP_Text m_upGradeMaxLevel;

        [SerializeField]
        private TMP_Text m_upGradeCurrentLevel;

        [SerializeField]
        private TMP_Text m_upGradeState;

        [SerializeField]
        private TMP_Text m_upGradeNextState;

        [SerializeField]
        private Image m_priceImage;

        [SerializeField]
        private TMP_Text m_upGradeBtnTitle;

        [SerializeField]
        private TMP_Text m_upGradePrice;

        [SerializeField]
        private Color m_upGradePrice_EnableColor;

        [SerializeField]
        private Color m_upGradePrice_DisableColor;

        [SerializeField]
        private Transform m_maxLevelText;

        [SerializeField]
        private Image m_buttonEnableLight;

        [SerializeField]
        private Image m_buttonImage;

        [SerializeField]
        private Color m_button_EnableColor;

        [SerializeField]
        private Color m_button_DisableColor;

        [SerializeField]
        private UIPushBtn uIPushBtn;

        [SerializeField]
        private ParticleSystem m_enhanceParticle;

        [SerializeField]
        private UIDoTween_Click m_enhanceDoTween;

        [SerializeField]
        private Image m_lockState;

        [SerializeField]
        private TMP_Text m_lockState_Training;

        [SerializeField]
        private TMP_Text m_lockState_Promo;

        [SerializeField]
        private Color m_isLockTraining;

        [SerializeField]
        private Color m_isUnLockTraining;

        private V2Enum_Stat m_isStatUpGradeType = V2Enum_Stat.Attack;
        private CharacterBaseTrainingData m_myCharacterBaseTrainingData = null;

        private UIGuideInteractor uIGuideInteractor;

        //------------------------------------------------------------------------------------
        public void Init(CharacterBaseTrainingData TrainingData)
        {
            m_myCharacterBaseTrainingData = TrainingData;
            m_isStatUpGradeType = TrainingData.TrainingType;

            CharacterBaseStatData statdata = Managers.ARRRStatManager.Instance.GetCharacterBaseStatData(TrainingData.TrainingType);

            if (m_upGradeIcon != null)
                m_upGradeIcon.sprite = Managers.ARRRStatManager.Instance.GetStatSprite(statdata.BaseStat);

            if (m_upGradeMaxLevel != null)
                m_upGradeMaxLevel.text = string.Format("{0:#,0}", TrainingData.TrainingMaxLevel);

            if (m_upGradeTitle != null)
            {
                Managers.LocalStringManager.Instance.SetLocalizeText(m_upGradeTitle, statdata.NameLocalStringKey);
            }

            if (m_priceImage != null)
                m_priceImage.sprite = Managers.GoodsManager.Instance.GetGoodsSprite(TrainingData.LevelUpCostGoodsType, TrainingData.LevelUpCostGoodsParam1);

            SetCurrentLevel();
            SetUpGradeState();
            SetUpGradePrice();

            if (uIPushBtn != null)
            {
                uIPushBtn.SetOnClick(UpGrade_Click);
                uIPushBtn.SetOnPush(UpGrade_Click);
                //uIPushBtn.SetOnPushEnd(UpGrade_PushEnd);
            }

            RefreshLocalize();

            if (Managers.LocalStringManager.isAlive == true)
                Managers.LocalStringManager.Instance.RefreshLocalString += RefreshLocalize;

            Managers.GoodsManager.Instance.AddGoodsRefreshEvent(TrainingData.LevelUpCostGoodsType, TrainingData.LevelUpCostGoodsParam1, CheckUpGradeGold);

            CheckUpGradeGold(Managers.GoodsManager.Instance.GetGoodsAmount(TrainingData.LevelUpCostGoodsType, TrainingData.LevelUpCostGoodsParam1));
        }
        //------------------------------------------------------------------------------------
        public void OnDestroy()
        {
            if (Managers.LocalStringManager.isAlive == true)
                Managers.LocalStringManager.Instance.RefreshLocalString -= RefreshLocalize;

            if (Managers.GoodsManager.isAlive == true)
                Managers.GoodsManager.Instance.RemoveGoodsRefreshEvent(m_myCharacterBaseTrainingData.LevelUpCostGoodsType, m_myCharacterBaseTrainingData.LevelUpCostGoodsParam1, CheckUpGradeGold);
        }
        //------------------------------------------------------------------------------------
        private void RefreshLocalize()
        {
            if (m_myCharacterBaseTrainingData == null)
                return;

            CharacterBaseTrainingData UnlockTrainingData_Training = Managers.PlayerTrainingDataManager.Instance.GetCharTrainingData(m_myCharacterBaseTrainingData.OpenConditionIndex);
            if (UnlockTrainingData_Training != null)
            {
                if (m_lockState_Training != null)
                { 
                    string trainingName = Managers.LocalStringManager.Instance.GetLocalString(UnlockTrainingData_Training.NameLocalStringKey);

                    m_lockState_Training.text = string.Format(Managers.LocalStringManager.Instance.GetLocalString("character/training/condition"), trainingName, m_myCharacterBaseTrainingData.OpenConditionLevel);

                    //m_lockState_Training.text = string.Format(Managers.LocalStringManager.Instance.GetLocalString("Growth_UI_Locked_Lv"), Managers.LocalStringManager.Instance.GetLocalString(statdata.StatName));
                }
            }

            //CharPromoData Unlock_Promo = Managers.PlayerPromoDataManager.Instance.GetCharPromoData(TrainingData.CharPromoID);
            //if (Unlock_Promo != null)
            //{
            //    if (m_lockState_Promo != null)
            //        m_lockState_Promo.text = string.Format(Managers.LocalStringManager.Instance.GetLocalString("Growth_UI_PromoAblilty_Locked"), Managers.LocalStringManager.Instance.GetLocalString(Unlock_Promo.CharGradeName));
            //}
        }
        //------------------------------------------------------------------------------------
        private void UpGrade_Click()
        {
            if (Managers.PlayerTrainingDataManager.Instance.IsMaxTrainingStat(m_isStatUpGradeType) == true)
            {
                Debug.Log("¿ÃπÃ ∏∏∑¶");
                return;
            }


            if (Managers.PlayerTrainingDataManager.Instance.InCreaseTrainingStatLevel_Click(m_isStatUpGradeType) == true)
            {
                if (uIGuideInteractor != null)
                { 
                    uIGuideInteractor.OnClick_Interactor();
                    Destroy(uIGuideInteractor);
                    uIGuideInteractor = null;
                }

                if (m_enhanceParticle != null)
                {
                    m_enhanceParticle.gameObject.SetActive(true);
                    m_enhanceParticle.Stop();
                    m_enhanceParticle.Play();
                }

                if (m_upGradeParticle != null)
                {
                    m_upGradeParticle.gameObject.SetActive(true);
                    m_upGradeParticle.Stop();
                    m_upGradeParticle.Play();
                }

                if (m_enhanceDoTween != null)
                    m_enhanceDoTween.OnClick_Btn();
            }

            //if (m_enhanceParticle != null)
            //{
            //    m_enhanceParticle.gameObject.SetActive(true);
            //    m_enhanceParticle.Stop();
            //    m_enhanceParticle.Play();
            //}

            //if (m_enhanceDoTween != null)
            //    m_enhanceDoTween.OnClick_Btn();
        }
        //------------------------------------------------------------------------------------
        private void UpGrade_Push()
        {

            //if (Managers.PlayerTrainingDataManager.Instance.IsMaxTrainingStat(m_isStatUpGradeType) == true)
            //    return;

            bool upgradeSuccess = true;//Managers.PlayerTrainingDataManager.Instance.InCreaseTrainingStatLevel_Push(m_isStatUpGradeType);

            if (upgradeSuccess == true)
            {
                SetCurrentLevel();
                SetUpGradeState();
                SetUpGradePrice();

                if (m_enhanceParticle != null)
                {
                    m_enhanceParticle.gameObject.SetActive(true);
                    m_enhanceParticle.Stop();
                    m_enhanceParticle.Play();
                }

                if (m_upGradeParticle != null)
                {
                    m_upGradeParticle.gameObject.SetActive(true);
                    m_upGradeParticle.Stop();
                    m_upGradeParticle.Play();
                }

                if (m_enhanceDoTween != null)
                    m_enhanceDoTween.OnClick_Btn();
            }

            //if (Managers.PlayerTrainingDataManager.Instance.IsMaxTrainingStat(m_isStatUpGradeType) == true)
            //{
            //    if (uIPushBtn != null)
            //        uIPushBtn.gameObject.SetActive(false);

            //    if (m_maxLevelText != null)
            //        m_maxLevelText.gameObject.SetActive(true);
            //}
        }
        ////------------------------------------------------------------------------------------
        //private void UpGrade_PushEnd()
        //{
        //    Managers.PlayerTrainingDataManager.Instance.InCreaseTrainingStatLevel_PushEnd(m_isStatUpGradeType);
        //}
        //------------------------------------------------------------------------------------
        public void RefreshTrainingUI()
        {
            SetCurrentLevel();
            SetUpGradeState();
            SetUpGradePrice();

            if (Managers.PlayerTrainingDataManager.Instance.IsMaxTrainingStat(m_isStatUpGradeType) == true)
            {
                if (uIPushBtn != null)
                    uIPushBtn.gameObject.SetActive(false);

                if (m_maxLevelText != null)
                    m_maxLevelText.gameObject.SetActive(true);
            }
        }
        //------------------------------------------------------------------------------------
        private void SetCurrentLevel()
        {
            if (m_upGradeCurrentLevel != null)
                m_upGradeCurrentLevel.text = string.Format("Lv.{0}", string.Format("{0:#,0}", Managers.PlayerTrainingDataManager.Instance.GetCurrentTrainingStatLevel(m_isStatUpGradeType)));
        }
        //------------------------------------------------------------------------------------
        private void SetUpGradeState()
        {
            bool isMaxLevel = Managers.PlayerTrainingDataManager.Instance.IsMaxTrainingStat(m_isStatUpGradeType);

            if (m_upGradeState != null)
            {
                //m_upGradeState.text = isMaxLevel == true ? Managers.PlayerTrainingDataManager.Instance.GetCurrentTrainingStatValue(m_isStatUpGradeType).ToString() : string.Format("{0}", Managers.PlayerTrainingDataManager.Instance.GetCurrentTrainingStatValue(m_isStatUpGradeType));
                CharacterBaseStatData statdata = Managers.ARRRStatManager.Instance.GetCharacterBaseStatData(m_isStatUpGradeType);
                m_upGradeState.text = Util.GetAddStatPrintValue(Managers.PlayerTrainingDataManager.Instance.GetCurrentTrainingStatValue(m_isStatUpGradeType), statdata);
            }

            //if (m_upGradeNextState != null)
            //{
            //    m_upGradeNextState.text = isMaxLevel == true ? string.Empty : Managers.PlayerTrainingDataManager.Instance.GetNextTrainingStatValue(m_isStatUpGradeType).ToString();
            //}

            bool isLock = Managers.PlayerTrainingDataManager.Instance.CheckLockTraining(m_isStatUpGradeType);

            if (m_lockState != null)
            {
                m_lockState.gameObject.SetActive(isLock);
            }

            if (isLock == true)
            {
                if (m_lockState_Training != null)
                    m_lockState_Training.color = Managers.PlayerTrainingDataManager.Instance.CheckLockTraining_Training(m_isStatUpGradeType) == true ? m_isLockTraining : m_isUnLockTraining;

                //if (m_lockState_Promo != null)
                //    m_lockState_Promo.color = Managers.PlayerTrainingDataManager.Instance.CheckLockTraining_Promo(m_isStatUpGradeType) == true ? m_isLockTraining : m_isUnLockTraining;
            }
        }
        //------------------------------------------------------------------------------------
        private void SetUpGradePrice()
        {
            if (m_upGradePrice != null)
            {
                string textstr = Managers.PlayerTrainingDataManager.Instance.IsMaxTrainingStat(m_isStatUpGradeType) == true ? "MAX" : Util.GetAlphabetNumber(Managers.PlayerTrainingDataManager.Instance.GetTrainingStatPrice(m_isStatUpGradeType));
                m_upGradePrice.text = textstr;
            }
        }
        //------------------------------------------------------------------------------------
        public void CheckUpGradeGold(double gold)
        {
            if (Managers.PlayerTrainingDataManager.Instance.IsMaxTrainingStat(m_isStatUpGradeType) == true)
            {
                if (uIPushBtn != null)
                    uIPushBtn.gameObject.SetActive(false);

                if (m_maxLevelText != null)
                    m_maxLevelText.gameObject.SetActive(true);

                return;
            }

            if (m_maxLevelText != null)
                m_maxLevelText.gameObject.SetActive(false);

            if (uIPushBtn != null)
                uIPushBtn.gameObject.SetActive(true);

            if (Managers.PlayerTrainingDataManager.Instance.GetTrainingStatPrice(m_isStatUpGradeType) <= gold)
            {
                if (m_buttonImage != null)
                    m_buttonImage.color = m_button_EnableColor;

                if (m_upGradePrice != null)
                    m_upGradePrice.color = m_upGradePrice_EnableColor;

                if (m_upGradeBtnTitle != null)
                    m_upGradeBtnTitle.color = m_upGradePrice_EnableColor;

                if (m_buttonEnableLight != null)
                    m_buttonEnableLight.gameObject.SetActive(true);
            }
            else
            {
                if (m_buttonImage != null)
                    m_buttonImage.color = m_button_DisableColor;

                if (m_upGradePrice != null)
                    m_upGradePrice.color = m_upGradePrice_DisableColor;

                if (m_upGradeBtnTitle != null)
                    m_upGradeBtnTitle.color = m_upGradePrice_DisableColor;

                if (m_buttonEnableLight != null)
                    m_buttonEnableLight.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        public void SetTrainingGuideBtn(V2Enum_EventType v2Enum_EventType)
        {
            if (uIPushBtn != null)
            {
                uIGuideInteractor = uIPushBtn.gameObject.AddComponent<UIGuideInteractor>();
                uIGuideInteractor.MyGuideType = v2Enum_EventType;
                uIGuideInteractor.MyStepID = 1;
                uIGuideInteractor.GuideInteratorActionType = Managers.GuideInteratorActionType.End;
                uIGuideInteractor.IsAutoSetting = false;
                uIGuideInteractor.FocusAngle = 180.0f;
                uIGuideInteractor.FocusParent = transform.parent;
                uIGuideInteractor.ConnectInteractor();
            }
        }
        //------------------------------------------------------------------------------------
    }
}