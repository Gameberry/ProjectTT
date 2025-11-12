using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

namespace GameBerry.UI
{
    public class InGameGambleDialog : IDialog
    {
        [SerializeField]
        private Transform _gambleBtnRoot;

        [Header("----------Card----------")]
        [SerializeField]
        private TMP_Text _gambleCardCost;

        [SerializeField]
        private TMP_Text _gambleCardCost_ColorChange;

        [SerializeField]
        private Button _playGambleCard;

        [SerializeField]
        private Image _playGambleDisableImage;

        [SerializeField]
        private UIFarmingDirectionController _uIFarmingDirectionController;

        [Header("----------AutoTrigger----------")]
        [SerializeField]
        private Button _gambleAutoToggle;

        [SerializeField]
        private Image _gambleAutoToggle_Check;

        [Header("----------GasAmount----------")]
        [SerializeField]
        private Transform _gasAmount;

        [Header("----------GasHp----------")]
        [SerializeField]
        private TMP_Text _gasHpCost;

        [SerializeField]
        private TMP_Text _gasHpCost_ColorChange;

        [SerializeField]
        private TMP_Text _gasHpRecovery;

        [SerializeField]
        private Button _playGasHp;

        [SerializeField]
        private Image _playGasHpDisableImage;


        [Header("----------GasSynergy----------")]
        [SerializeField]
        private TMP_Text _gasSynergyCost;

        [SerializeField]
        private TMP_Text _gasSynergyCost_ColorChange;

        [SerializeField]
        private TMP_Text _gasSynergyPercent;

        [SerializeField]
        private TMP_Text _gasSynergyFail;

        [SerializeField]
        private TMP_Text _gasSynergySuccess;

        [SerializeField]
        private Button _playGasSynergy;

        [SerializeField]
        private Image _playGasSynergyDisableImage;

        [SerializeField]
        private Animator _playGasSynergyAnimator;

        [SerializeField]
        private AnimatorListener _playGasSynergyAnimatorListener;


        [Header("----------DescendEnhance----------")]
        [SerializeField]
        private Transform _descendEnhanceAmount;

        [Header("----------Tutorial----------")]
        [SerializeField]
        private Transform _tutorialBlack;

        [SerializeField]
        private TMP_Text _tutorialText;

        private V2Enum_EventType _tutorialMode = V2Enum_EventType.Max;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (_gambleCardCost != null)
                _gambleCardCost.text = string.Format("{0}", Managers.GambleManager.Instance.GetCost(Enum_GambleType.Card));

            if (_playGambleCard != null)
                _playGambleCard.onClick.AddListener(OnClick_PlayGambleCard);

            if (_playGasHp != null)
                _playGasHp.onClick.AddListener(OnClick_PlayGasHp);

            if (_playGasSynergy != null)
                _playGasSynergy.onClick.AddListener(OnClick_PlayGasSynergy);

            if (_playGasSynergyAnimatorListener != null)
                _playGasSynergyAnimatorListener.OnCallBack += ShowGasSynergyResult;

            if (_gambleAutoToggle != null)
                _gambleAutoToggle.onClick.AddListener(OnValueChange_AutoGamble);

            Managers.GoodsManager.Instance.AddGoodsRefreshEvent(V2Enum_Goods.Point, V2Enum_Point.InGameGold.Enum32ToInt(), RefreshGambleBtn);
            Managers.GoodsManager.Instance.AddGoodsRefreshEvent(V2Enum_Goods.Point, V2Enum_Point.InGameGas.Enum32ToInt(), RefreshGasBtn);

            //Message.AddListener<GameBerry.Event.PlayFreeGambleSlotMsg>(PlayFreeGambleSlot);
            Message.AddListener<GameBerry.Event.PlayARRRTutorialMsg>(PlayARRRTutorial);
            Message.AddListener<GameBerry.Event.ShowInterestTextMsg>(ShowInterestText);
            Message.AddListener<GameBerry.Event.PlaySlotTutorialMsg>(PlaySlotTutorial);
            Message.AddListener<GameBerry.Event.PlayGasTutorialMsg>(PlayGasTutorial);
            Message.AddListener<GameBerry.Event.RefreshGambleAutoTriggerMsg>(RefreshGambleAutoTrigger);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Managers.GoodsManager.Instance.RemoveGoodsRefreshEvent(V2Enum_Goods.Point, V2Enum_Point.InGameGold.Enum32ToInt(), RefreshGambleBtn);
            Managers.GoodsManager.Instance.RemoveGoodsRefreshEvent(V2Enum_Goods.Point, V2Enum_Point.InGameGas.Enum32ToInt(), RefreshGasBtn);

            //Message.RemoveListener<GameBerry.Event.PlayFreeGambleSlotMsg>(PlayFreeGambleSlot);
            Message.RemoveListener<GameBerry.Event.PlayARRRTutorialMsg>(PlayARRRTutorial);
            Message.RemoveListener<GameBerry.Event.ShowInterestTextMsg>(ShowInterestText);
            Message.RemoveListener<GameBerry.Event.PlaySlotTutorialMsg>(PlaySlotTutorial);
            Message.RemoveListener<GameBerry.Event.PlayGasTutorialMsg>(PlayGasTutorial);
            Message.RemoveListener<GameBerry.Event.RefreshGambleAutoTriggerMsg>(RefreshGambleAutoTrigger);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            RefreshButtonState();
            RefreshGasState();
            SetAutoGambleToggle();

            //if (_playGasSynergyAnimator != null)
            //{
            //    _playGasSynergyAnimator.Rebind();
            //    _playGasSynergyAnimator.SetBool("play", false);
            //}

            _playedGasSynergy = false;

            if (_gasHpRecovery != null)
                _gasHpRecovery.text = string.Format("{0:0.#}%", Managers.GambleManager.Instance.GetGasHpRecoveryRatio());

            if (_gasSynergyPercent != null)
                _gasSynergyPercent.text = string.Format("{0:0.#}%", Managers.GambleManager.Instance.GetGasSynergyPercent());

            if (_playGambleCard != null)
            {
                if (Managers.MapManager.Instance.NeedTutotial1() == true)
                    _playGambleCard.gameObject.SetActive(false);
                else
                    _playGambleCard.gameObject.SetActive(true);
            }

            if (_gasAmount != null)
                _gasAmount.gameObject.SetActive(Managers.MapManager.Instance.NeedTutotial1() == false);

            if (_playGasHp != null)
                _playGasHp.gameObject.SetActive(Managers.MapManager.Instance.NeedTutotial1() == false);

            if (_playGasSynergy != null)
                _playGasSynergy.gameObject.SetActive(Managers.MapManager.Instance.NeedTutotial1() == false);

            if (_descendEnhanceAmount != null)
                _descendEnhanceAmount.gameObject.SetActive(Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Descend));

            if (_gambleAutoToggle != null)
                _gambleAutoToggle.gameObject.SetActive(MapContainer.MapMaxClear >= 2);
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (_uIFarmingDirectionController != null)
                _uIFarmingDirectionController.ForceRelease();

            if (_playGasSynergyAnimator != null)
            {
                _playGasSynergyAnimator.SetBool("play", false);
                //_playGasSynergyAnimator.runtimeAnimatorController = null;
            }

            UIManager.DialogExit<InGameGambleAutoDialog>();
        }
        //------------------------------------------------------------------------------------
        #region Gamble
        //------------------------------------------------------------------------------------
        private void RefreshGambleAutoTrigger(GameBerry.Event.RefreshGambleAutoTriggerMsg msg)
        {
            SetAutoGambleToggle();
        }
        //------------------------------------------------------------------------------------
        private void SetAutoGambleToggle()
        {
            bool setauto = Managers.GambleManager.Instance.SynergyGambleAuto;

            if (_gambleAutoToggle_Check != null)
                _gambleAutoToggle_Check.gameObject.SetActive(setauto);

            //if(setauto == true)
            //    Managers.GambleManager.Instance.PlayAutoGamble();
        }
        //------------------------------------------------------------------------------------
        private void OnValueChange_AutoGamble()
        {
            if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.AutoPlay) == false)
            {
                Managers.ContentOpenConditionManager.Instance.ShowOpenConditionNotice(V2Enum_ContentType.AutoPlay);
                return;
            }

            if (Managers.GambleManager.Instance.SynergyGambleAuto == true)
            {
                Managers.GambleManager.Instance.SetAutoGamble(false);
            }
            else
            {
                UIManager.DialogEnter<InGameGambleAutoDialog>();
            }
            
            //Managers.GambleManager.Instance.SetAutoGamble(value);
        }
        //------------------------------------------------------------------------------------
        private void RefreshGambleBtn(double amount)
        {
            RefreshButtonState();
        }
        //------------------------------------------------------------------------------------
        private void RefreshButtonState()
        {
            double currentGold = Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point, V2Enum_Point.InGameGold.Enum32ToInt());

            double needcost = Managers.GambleManager.Instance.GetCost(Enum_GambleType.Card);

            if (_playGambleDisableImage != null)
            {
                _playGambleDisableImage.gameObject.SetActive(needcost > currentGold);
            }

            if (_gambleCardCost != null)
            {
                _gambleCardCost.text = string.Format("{0}", needcost);
            }

            if (_gambleCardCost_ColorChange != null)
            {
                _gambleCardCost_ColorChange.color = needcost > currentGold ? Color.red : Color.white;
            }

            //Managers.GambleManager.Instance.PlayAutoGamble();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_PlayGambleCard()
        {
            if (_playedGasSynergy == true)
                return;

            if (Managers.GuideInteractorManager.Instance.PlayCardTutorial == true)
                return;

            double currentGold = Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point, V2Enum_Point.InGameGold.Enum32ToInt());
            double needcost = Managers.GambleManager.Instance.GetCost(Enum_GambleType.Card);

            if (needcost > currentGold)
                return;

            if (_tutorialMode == V2Enum_EventType.TutoGambleCard)
            {
                if (_playGambleCard != null)
                    _playGambleCard.transform.SetParent(_gambleBtnRoot);

                if (_tutorialBlack != null)
                    _tutorialBlack.gameObject.SetActive(false);

                Managers.BattleSceneManager.Instance.ChangeOriginBattleSpeed();
            }

            Managers.GambleManager.Instance.SetAutoGamble(false);

            Managers.GambleManager.Instance.PlayCardGamble();

            RefreshButtonState();
        }
        //------------------------------------------------------------------------------------
        private void PlayARRRTutorial(GameBerry.Event.PlayARRRTutorialMsg msg)
        {
            if (msg.Enum_GambleType != V2Enum_EventType.TutoGambleCard)
                return;

            _tutorialMode = msg.Enum_GambleType;

            Managers.GuideInteractorManager.Instance.StartGuideInteractor(V2Enum_EventType.TutoGambleCard);

            if (_tutorialText != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_tutorialText, "guide/gamblecard");

            if (_playGambleCard == null)
                return;

            if (_tutorialBlack != null)
            { 
                _tutorialBlack.gameObject.SetActive(true);
                _playGambleCard.transform.SetParent(_tutorialBlack);
                _playGambleCard.gameObject.SetActive(true);
            }

            Managers.BattleSceneManager.Instance.ChangeTimeScale(Enum_BattleSpeed.Pause);
        }
        //------------------------------------------------------------------------------------
        private void PlaySlotTutorial(GameBerry.Event.PlaySlotTutorialMsg msg)
        {
            if (_playGambleCard != null)
                _playGambleCard.gameObject.SetActive(false);

            if (_tutorialText != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_tutorialText, "guide/slot1");

            if (_tutorialBlack != null)
            {
                _tutorialBlack.gameObject.SetActive(true);
            }

            //Managers.BattleSceneManager.Instance.ChangeTimeScale(Enum_BattleSpeed.Pause);

            PlayGasTutorial().Forget();
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlayGasTutorial()
        {
            await UniTask.Delay(5000, false, PlayerLoopTiming.Update);

            if (_tutorialBlack != null)
            {
                _tutorialBlack.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        private void PlayGasTutorial(GameBerry.Event.PlayGasTutorialMsg msg)
        {
            if (_gasAmount != null)
                _gasAmount.gameObject.SetActive(true);

            UIManager.DialogExit<UI.InGameGambleSynergyDialog>();

            if (_playGasSynergy != null)
            { 
                _playGasSynergy.gameObject.SetActive(true);
                _playGasSynergy.transform.SetParent(_tutorialBlack);
            }

            Managers.GuideInteractorManager.Instance.StartGuideInteractor(V2Enum_EventType.TutoGambleGas);

            if (_tutorialText != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_tutorialText, "guide/gassynergy1");

            if (_tutorialBlack != null)
            {
                _tutorialBlack.gameObject.SetActive(true);
            }

            //Managers.BattleSceneManager.Instance.ChangeTimeScale(Enum_BattleSpeed.Pause);
        }
        //------------------------------------------------------------------------------------
        private void ShowInterestText(GameBerry.Event.ShowInterestTextMsg msg)
        {
            Sprite sprite = Managers.GoodsManager.Instance.GetGoodsSprite(V2Enum_Goods.Point, V2Enum_Point.InGameGold.Enum32ToInt());
            if (_uIFarmingDirectionController != null)
                _uIFarmingDirectionController.ShowFarmingReward(sprite, msg.text);
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Gas
        //------------------------------------------------------------------------------------
        private void RefreshGasBtn(double amount)
        {
            RefreshGasState();
        }
        //------------------------------------------------------------------------------------
        private void RefreshGasState()
        {
            double currentGold = Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point, V2Enum_Point.InGameGas.Enum32ToInt());

            {
                double needcost = 1;

                if (_playGasHpDisableImage != null)
                {
                    _playGasHpDisableImage.gameObject.SetActive(needcost > currentGold);
                }

                if (_gasHpCost != null)
                {
                    _gasHpCost.text = string.Format("{0}", needcost);
                }

                if (_gasHpCost_ColorChange != null)
                {
                    _gasHpCost_ColorChange.color = needcost > currentGold ? Color.red : Color.white;
                }
            }

            {
                double needcost = 1;

                if (_playGasSynergyDisableImage != null)
                {
                    _playGasSynergyDisableImage.gameObject.SetActive(needcost > currentGold);
                }

                if (_gasSynergyCost != null)
                {
                    _gasSynergyCost.text = string.Format("{0}", needcost);
                }

                if (_gasSynergyCost_ColorChange != null)
                {
                    _gasSynergyCost_ColorChange.color = needcost > currentGold ? Color.red : Color.white;
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_PlayGasHp()
        {
            double currentGold = Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point, V2Enum_Point.InGameGas.Enum32ToInt());

            double needcost = 1;
            
            if (needcost > currentGold)
                return;

            Managers.GambleManager.Instance.PlayGasHp();

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.GasSynergy)
                Managers.GuideInteractorManager.Instance.EndGuideQuest();

            RefreshGasState();
        }
        //------------------------------------------------------------------------------------
        bool _canGasSynergy = false;
        bool _playedGasSynergy = false;
        private void OnClick_PlayGasSynergy()
        {
            if (_playedGasSynergy == true)
                return;

            double currentGold = Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point, V2Enum_Point.InGameGas.Enum32ToInt());

            double needcost = 1;

            if (needcost > currentGold)
                return;

            _canGasSynergy = Managers.GambleManager.Instance.CanPlayGasSynergy();

            if (Managers.MapManager.Instance.NeedTutotial1() == true && Managers.GuideInteractorManager.Instance.PlayGasSynergyTutorial == true)
            {
                _canGasSynergy = true;

                Managers.BattleSceneManager.Instance.ChangeOriginBattleSpeed();

                if (_playGasSynergy != null)
                {
                    _playGasSynergy.transform.SetParent(_gambleBtnRoot);
                }

                if (_tutorialBlack != null)
                    _tutorialBlack.gameObject.SetActive(false);
            }

            Managers.GoodsManager.Instance.UseGoodsAmount(V2Enum_Goods.Point, V2Enum_Point.InGameGas.Enum32ToInt(), 1);

            Debug.Log(string.Format("가스 시너지 결과 : {0}", _canGasSynergy));


            if (_gasSynergyFail != null)
                _gasSynergyFail.gameObject.SetActive(_canGasSynergy == false);

            if (_gasSynergySuccess != null)
                _gasSynergySuccess.gameObject.SetActive(_canGasSynergy == true);

            _playedGasSynergy = true;

            if (_canGasSynergy == true)
                Managers.GambleManager.Instance.SetAutoGamble(false);

            if (_playGasSynergyAnimator != null)
            {
                _playGasSynergyAnimator.SetBool("play", true);
            }

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.GasSynergy)
                Managers.GuideInteractorManager.Instance.EndGuideQuest();

            RefreshGasState();
        }
        //------------------------------------------------------------------------------------
        private void ShowGasSynergyResult()
        {
            if (_playGasSynergyAnimator != null)
            {
                _playGasSynergyAnimator.SetBool("play", false);
            }

            if (Managers.MapManager.Instance.NeedTutotial1() == true && Managers.GuideInteractorManager.Instance.PlayGasSynergyTutorial == true)
            {
                if (_playGambleCard != null)
                    _playGambleCard.gameObject.SetActive(true);
            }




            _playedGasSynergy = false;

            if (_isEnter == false)
                return;

            if (_canGasSynergy == false)
            {
                Managers.BattleSceneManager.Instance.CurrentBattleScene.MyARRRControllers.SkillActiveController.AddFailGasGambleCount(1);
                return;
            }

            Managers.GambleManager.Instance.PlayGasSynergy();
            UI.UIManager.DialogEnter<UI.InGameGambleSynergyDialog>();
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
    }
}