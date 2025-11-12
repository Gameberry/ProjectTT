using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace GameBerry.UI
{
    [System.Serializable]
    public class UIInGameSynergyDetailKindTab
    {
        public Enum_SynergyType Enum_SynergyType = Enum_SynergyType.Max;

        public Transform SynergyGroup;

        public Button SynergyBtn;

        public Transform SelectedTrans;

        public Transform DisableTrans;

        public System.Action<Enum_SynergyType> _action;

        public void SetCallBack(System.Action<Enum_SynergyType> action)
        {
            if (SynergyBtn != null)
                SynergyBtn.onClick.AddListener(OnClick);

            _action = action;
        }

        private void OnClick()
        {
            _action?.Invoke(Enum_SynergyType);
        }
    }

    public class InGameGambleSynergyDetailDialog : IDialog
    {
        [SerializeField]
        private List<UIInGameSynergyDetailKindTab> _synergyKindTabs = new List<UIInGameSynergyDetailKindTab>();

        [SerializeField]
        private TMP_Text _synergyTitle;

        [SerializeField]
        private Image _synergyIcon;

        [SerializeField]
        private UIGambleSynergyDetailElement _uIGambleSynergyDetailElement;

        [SerializeField]
        private Transform _uIGambleSynergyDetailElementRoot;

        private List<UIGambleSynergyDetailElement> _uIGambleSynergyDetailElementList = new List<UIGambleSynergyDetailElement>();

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            for (int i = 0; i < _synergyKindTabs.Count; ++i)
            {
                _synergyKindTabs[i].SetCallBack(OnClick_SynergyTab);
            }

            for (int i = 0; i < 5; ++i)
            {
                GameObject clone = Instantiate(_uIGambleSynergyDetailElement.gameObject, _uIGambleSynergyDetailElementRoot);
                UIGambleSynergyDetailElement uIGambleSynergyDetailElement = clone.GetComponent<UIGambleSynergyDetailElement>();
                _uIGambleSynergyDetailElementList.Add(uIGambleSynergyDetailElement);
            }

            Message.AddListener<GameBerry.Event.ShowGambleSynergyDetailMsg>(ShowGambleSynergyDetail);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.ShowGambleSynergyDetailMsg>(ShowGambleSynergyDetail);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            if (Managers.MapManager.Instance.NeedTutotial1() == true && Managers.GuideInteractorManager.Instance.PlayCardTutorial == true)
            {
                PlayCardGamblePlayTutorial().Forget();
            }

            if (Managers.MapManager.Instance.NeedTutotial1() == true && Managers.GuideInteractorManager.Instance.PlayGasSynergyTutorial == true)
            {
                PlayCardGamblePlayTutorial().Forget();
            }

            for (int i = 0; i < _synergyKindTabs.Count; ++i)
            {
                UIInGameSynergyDetailKindTab uIInGameSynergyDetailKindTab = _synergyKindTabs[i];
                Enum_SynergyType _synergyType = uIInGameSynergyDetailKindTab.Enum_SynergyType;

                if (_synergyType == Enum_SynergyType.Yellow)
                {
                    if (uIInGameSynergyDetailKindTab.SynergyGroup != null)
                        uIInGameSynergyDetailKindTab.SynergyGroup.gameObject.SetActive(Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.UnlockGoldSynergy));
                }

                if (_synergyType == Enum_SynergyType.White)
                {
                    if (uIInGameSynergyDetailKindTab.SynergyGroup != null)
                        uIInGameSynergyDetailKindTab.SynergyGroup.gameObject.SetActive(Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.UnlockThunderSynergy));
                }
            }
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlayCardGamblePlayTutorial()
        {
                await UniTask.Delay(500, false, PlayerLoopTiming.Update);
            if (_isEnter == true)
            {
                Managers.BattleSceneManager.Instance.ChangeTimeScale(Enum_BattleSpeed.Pause);
            }
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (Managers.MapManager.Instance.NeedTutotial1() == true && Managers.GuideInteractorManager.Instance.PlayCardTutorial == true)
            {
                Managers.BattleSceneManager.Instance.ChangeOriginBattleSpeed();
                Managers.GuideInteractorManager.Instance.PlayCardTutorial = false;
            }

            if (Managers.MapManager.Instance.NeedTutotial1() == true && Managers.GuideInteractorManager.Instance.PlayGasSynergyTutorial == true)
            {
                Managers.BattleSceneManager.Instance.ChangeOriginBattleSpeed();
                Managers.GuideInteractorManager.Instance.PlayGasSynergyTutorial = false;
            }
        }
        //------------------------------------------------------------------------------------
        private void ShowGambleSynergyDetail(GameBerry.Event.ShowGambleSynergyDetailMsg msg)
        {
            RequestDialogEnter<InGameGambleSynergyDetailDialog>();

            if (_synergyTitle != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_synergyTitle, string.Format("synergytitle/{0}", msg.Enum_GambleSynergyType.Enum32ToInt()));


            GambleCardSprite gambleCardSprite = StaticResource.Instance.GetGambleCardSpriteData(msg.Enum_GambleSynergyType);
            if (gambleCardSprite != null)
            {
                if (_synergyIcon != null)
                    _synergyIcon.sprite = gambleCardSprite.SynergyIcon;
            }

            List<SynergyEffectData> gambleSynergyEffectDatas = Managers.SynergyManager.Instance.GetInGameEquipSynergyEffectData(msg.Enum_GambleSynergyType);

            for (int i = 0; i < gambleSynergyEffectDatas.Count; ++i)
            {
                SynergyEffectData gambleSynergyEffectData = gambleSynergyEffectDatas[i];
                UIGambleSynergyDetailElement uIGambleSynergyDetailElement = null;

                if (i < _uIGambleSynergyDetailElementList.Count)
                {
                    uIGambleSynergyDetailElement = _uIGambleSynergyDetailElementList[i];
                }
                else
                {
                    GameObject clone = Instantiate(_uIGambleSynergyDetailElement.gameObject, _uIGambleSynergyDetailElementRoot);
                    uIGambleSynergyDetailElement = clone.GetComponent<UIGambleSynergyDetailElement>();
                    _uIGambleSynergyDetailElementList.Add(uIGambleSynergyDetailElement);
                }

                uIGambleSynergyDetailElement.gameObject.SetActive(true);
                uIGambleSynergyDetailElement.SetGambleSynergyEffectData(gambleSynergyEffectData);
                if (gambleSynergyEffectData == msg.FocusData)
                    uIGambleSynergyDetailElement.PlayPunchAni();
            }

            for (int i = gambleSynergyEffectDatas.Count; i < _uIGambleSynergyDetailElementList.Count; ++i)
            {
                UIGambleSynergyDetailElement uIGambleSynergyDetailElement = _uIGambleSynergyDetailElementList[i];
                uIGambleSynergyDetailElement.gameObject.SetActive(false);
            }

            SetSynergyTabUIState(msg.Enum_GambleSynergyType);
        }
        //------------------------------------------------------------------------------------
        private void SetSynergyTabUIState(Enum_SynergyType Enum_GambleSynergyType)
        {
            for (int i = 0; i < _synergyKindTabs.Count; ++i)
            {
                if (_synergyKindTabs[i].DisableTrans != null)
                    _synergyKindTabs[i].DisableTrans.gameObject.SetActive(_synergyKindTabs[i].Enum_SynergyType != Enum_GambleSynergyType);

                if (_synergyKindTabs[i].SelectedTrans != null)
                    _synergyKindTabs[i].SelectedTrans.gameObject.SetActive(_synergyKindTabs[i].Enum_SynergyType == Enum_GambleSynergyType);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SynergyTab(Enum_SynergyType Enum_GambleSynergyType)
        {
            if (Enum_GambleSynergyType == Enum_SynergyType.Max)
                return;

            Managers.SynergyManager.Instance.ShowSynergyDetailPopup(Enum_GambleSynergyType);
        }
        //------------------------------------------------------------------------------------
    }
}