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


    public class InGameGambleSynergyDialog : IDialog
    {
        [SerializeField]
        private Transform _synergyListRoot;

        [SerializeField]
        private VerticalLayoutGroup _synergyVerticalLayout;

        [SerializeField]
        private UIGambleSynergyViewElement _uIGambleSynergyViewElement;

        private Dictionary<V2Enum_ARR_SynergyType, UIGambleSynergyViewElement> _uIGambleSynergyViewElement_dic = new Dictionary<V2Enum_ARR_SynergyType, UIGambleSynergyViewElement>();



        [SerializeField]
        private Transform uIBerserkerGaugeMoveElementRoot;

        [SerializeField]
        private UICardMoveSynergyElement uIGoodsDropDirectionElementObj;

        [SerializeField]
        private Ease ToEase = Ease.InCubic;

        [SerializeField]
        private float ToDuration = 0.25f;

        private Queue<UICardMoveSynergyElement> m_uIGoodsDropDirectionElements = new Queue<UICardMoveSynergyElement>();

        private CancellationTokenSource disableCancellation = new CancellationTokenSource(); //비활성화시 취소처리

        [Header("----------SynergySkill----------")]
        [SerializeField]
        private Button _showSynergySkill;

        [SerializeField]
        private Transform _readyGambleSynergySkillGroup;

        [SerializeField]
        private TMP_Text _readyGambleSynergySkill;

        [SerializeField]
        private Transform _missionComplete;

        [SerializeField]
        private Transform _missionGoldViewPoint;

        [Header("----------Tutorial----------")]
        [SerializeField]
        private Transform _tutorialBlack;

        [SerializeField]
        private TMP_Text _tutorialText;

        [SerializeField]
        private Button _synergyViewToturialBtn;


        private V2Enum_ARR_SynergyType _tutorialSynergyType;
        private UIGambleSynergyViewElement _tutorialUIGambleSynergyViewElement;

        public Dictionary<V2Enum_ARR_SynergyType, Queue<SynergyViewDirectionOrderData>> _synergyAddData = new Dictionary<V2Enum_ARR_SynergyType, Queue<SynergyViewDirectionOrderData>>();

        public Dictionary<V2Enum_ARR_SynergyType, bool> _synergyDoing = new Dictionary<V2Enum_ARR_SynergyType, bool>();

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            foreach (var pair in Managers.SynergyManager.Instance.SynergySortView)
            {
                GameObject clone = Instantiate(_uIGambleSynergyViewElement.gameObject, _synergyListRoot);

                UIGambleSynergyViewElement uIGambleSynergyListElement = clone.GetComponent<UIGambleSynergyViewElement>();
                uIGambleSynergyListElement.SetSynergyListData(pair);

                _uIGambleSynergyViewElement_dic.Add(pair, uIGambleSynergyListElement);
            }

            if (_showSynergySkill != null)
                _showSynergySkill.onClick.AddListener(() =>
                {
                    if (Managers.GuideInteractorManager.Instance.PlaySynergyCombineTutorial == true
                    && _tutorialBlack != null)
                        _tutorialBlack.gameObject.SetActive(false);

                    RequestDialogEnter<InGameGambleSynergyCombineDetailDialog>();
                });

            Message.AddListener<GameBerry.Event.AddGambleSynergySlotMsg>(AddGambleSynergySlot);
            Message.AddListener<GameBerry.Event.AddSkillSynergyMsg>(AddSkillSynergy);
            Message.AddListener<GameBerry.Event.RefreshGambleSynergyMsg>(RefreshGambleSynergy);
            Message.AddListener<GameBerry.Event.RefreshReadyGambleSynergyCombineSkillMsg>(RefreshReadyGambleSynergyCombineSkill);

            Message.AddListener<GameBerry.Event.PlaySynergyCombineTutorialMsg>(PlaySynergyCombineTutorial);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.AddGambleSynergySlotMsg>(AddGambleSynergySlot);
            Message.RemoveListener<GameBerry.Event.AddSkillSynergyMsg>(AddSkillSynergy);
            Message.RemoveListener<GameBerry.Event.RefreshGambleSynergyMsg>(RefreshGambleSynergy);
            Message.RemoveListener<GameBerry.Event.RefreshReadyGambleSynergyCombineSkillMsg>(RefreshReadyGambleSynergyCombineSkill);

            Message.RemoveListener<GameBerry.Event.PlaySynergyCombineTutorialMsg>(PlaySynergyCombineTutorial);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            if (_synergyVerticalLayout != null)
                _synergyVerticalLayout.enabled = true;


            foreach (var pair in Managers.SynergyManager.Instance.SynergySortView)
            {
                if (_uIGambleSynergyViewElement_dic.ContainsKey(pair) == false)
                    continue;


                if (pair == V2Enum_ARR_SynergyType.Yellow)
                {
                    if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.UnlockGoldSynergy) == false)
                    {
                        _uIGambleSynergyViewElement_dic[pair].gameObject.SetActive(false);
                        continue;
                    }
                }
                else if (pair == V2Enum_ARR_SynergyType.White)
                {
                    if (Managers.ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.UnlockThunderSynergy) == false)
                    {
                        _uIGambleSynergyViewElement_dic[pair].gameObject.SetActive(false);
                        continue;
                    }
                }

                _uIGambleSynergyViewElement_dic[pair].gameObject.SetActive(true);
                _uIGambleSynergyViewElement_dic[pair].transform.SetAsLastSibling();
            }

            ARRRController aRRRController = Managers.BattleSceneManager.Instance.CurrentBattleScene.MyARRRControllers;
            foreach (var pair in _uIGambleSynergyViewElement_dic)
            {
                pair.Value.RefreshSynergyInfo();
                pair.Value.HideInteractionUI();
            }

            bool istutorial = Managers.MapManager.Instance.NeedTutotial1();

            if (istutorial == true)
                istutorial = Managers.GuideInteractorManager.Instance.PlaySynergyCombineTutorial;

            if (_showSynergySkill != null)
            {
                if (istutorial == true)
                    _showSynergySkill.gameObject.SetActive(false);
                else
                {
                    if (Managers.BattleSceneManager.Instance.BattleType == V2Enum_Dungeon.DiamondDungeon
                        || Managers.BattleSceneManager.Instance.BattleType == V2Enum_Dungeon.TowerDungeon)
                        _showSynergySkill.gameObject.SetActive(false);
                    else
                        _showSynergySkill.gameObject.SetActive(true);
                }
            }

            

            if (_missionComplete != null)
                _missionComplete.gameObject.SetActive(false);

            RefreshReadyGambleSynergyCombineSkill(null);

            disableCancellation = new CancellationTokenSource();
        }
        //------------------------------------------------------------------------------------
        private void AddSynergyDirectionData(V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType, SynergyViewDirectionOrderData synergyViewDirectionOrderData)
        {
            if (_synergyAddData.ContainsKey(v2Enum_ARR_SynergyType) == false)
                _synergyAddData.Add(v2Enum_ARR_SynergyType, new Queue<SynergyViewDirectionOrderData>());

            if (_synergyDoing.ContainsKey(v2Enum_ARR_SynergyType) == false)
                _synergyDoing.Add(v2Enum_ARR_SynergyType, false);

            

            _synergyAddData[v2Enum_ARR_SynergyType].Enqueue(synergyViewDirectionOrderData);

            if (_synergyDoing[v2Enum_ARR_SynergyType] == false)
                PlaySynergyDirection(v2Enum_ARR_SynergyType);
        }
        //------------------------------------------------------------------------------------
        private async void PlaySynergyDirection(V2Enum_ARR_SynergyType v2Enum_ARR_SynergyType)
        {
            if (_synergyAddData.ContainsKey(v2Enum_ARR_SynergyType) == false)
                return;

            _synergyDoing[v2Enum_ARR_SynergyType] = true;

            Queue<SynergyViewDirectionOrderData> synergyQueue = _synergyAddData[v2Enum_ARR_SynergyType];

            while (synergyQueue.Count > 0)
            {
                Debug.Log(string.Format("synergyQueue.Count : {0}", synergyQueue.Count));
                await DirectionSynergy(synergyQueue.Dequeue());
            }

            _synergyDoing[v2Enum_ARR_SynergyType] = false;
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (disableCancellation.IsCancellationRequested == false) 
            {
                disableCancellation.Cancel();
                disableCancellation.Dispose();
            }
        }
        //------------------------------------------------------------------------------------
        private void AddGambleSynergySlot(GameBerry.Event.AddGambleSynergySlotMsg msg)
        {
            ARR_CardGambleData gambleSkillData = msg.GambleSkillData;
            if (_uIGambleSynergyViewElement_dic.ContainsKey(gambleSkillData.SynergyType) == false)
                return;


            SynergyViewDirectionOrderData synergyViewDirectionOrderData = new SynergyViewDirectionOrderData();
            synergyViewDirectionOrderData.v2Enum_ARR_SynergyType = gambleSkillData.SynergyType;
            synergyViewDirectionOrderData.startpos = msg.UIStartPos;
            synergyViewDirectionOrderData.gambleSkillData = gambleSkillData;

            synergyViewDirectionOrderData.beforeData = msg.BeforeData;
            synergyViewDirectionOrderData.beforeStack = msg.BeforeStack;
            synergyViewDirectionOrderData.afterData = msg.AfterData;
            synergyViewDirectionOrderData.afterStack = msg.AfterStack;

            synergyViewDirectionOrderData.DescendEnhance = msg.DescendEnhance;

            Debug.Log(string.Format("AddGambleSynergySlot AfterSynergy : {0}", msg.AfterStack));

            AddSynergyDirectionData(gambleSkillData.SynergyType, synergyViewDirectionOrderData);


            //DirectionSynergy(synergyViewDirectionOrderData).Forget();
        }
        //------------------------------------------------------------------------------------
        private void AddSkillSynergy(GameBerry.Event.AddSkillSynergyMsg msg)
        {
            if (_uIGambleSynergyViewElement_dic.ContainsKey(msg.V2Enum_ARR_SynergyType) == false)
                return;

            SynergyViewDirectionOrderData synergyViewDirectionOrderData = new SynergyViewDirectionOrderData();
            synergyViewDirectionOrderData.v2Enum_ARR_SynergyType = msg.V2Enum_ARR_SynergyType;

            synergyViewDirectionOrderData.beforeData = msg.BeforeData;
            synergyViewDirectionOrderData.beforeStack = msg.BeforeStack;
            synergyViewDirectionOrderData.afterData = msg.AfterData;
            synergyViewDirectionOrderData.afterStack = msg.AfterStack;

            synergyViewDirectionOrderData.DescendEnhance = msg.DescendEnhance;

            Debug.Log(string.Format("AddSkillSynergy AfterSynergy : {0}", msg.AfterStack));

            AddSynergyDirectionData(msg.V2Enum_ARR_SynergyType, synergyViewDirectionOrderData);

            //UIGambleSynergyViewElement uIGambleSynergyViewElement = _uIGambleSynergyViewElement_dic[msg.V2Enum_ARR_SynergyType];
            //uIGambleSynergyViewElement.AddSynergyViewDirectionOrderData(synergyViewDirectionOrderData);
        }
        //------------------------------------------------------------------------------------
        private void RefreshGambleSynergy(GameBerry.Event.RefreshGambleSynergyMsg msg)
        {
            if (_uIGambleSynergyViewElement_dic.ContainsKey(msg.v2Enum_ARR_GambleSynergyType) == false)
                return;

            UIGambleSynergyViewElement uIGambleSynergyViewElement = _uIGambleSynergyViewElement_dic[msg.v2Enum_ARR_GambleSynergyType];
            uIGambleSynergyViewElement.RefreshSynergyInfo();
        }
        //------------------------------------------------------------------------------------


        public async UniTask DirectionSynergy(SynergyViewDirectionOrderData viewdata)
        {
            if (disableCancellation.IsCancellationRequested == true)
            {
                return;
            }

            if (Managers.MapManager.Instance.NeedTutotial1() == true && Managers.GambleManager.Instance.GetGambleActionCount(V2Enum_ARR_GambleType.Card) == 1)
                await UniTask.Delay(100, false, PlayerLoopTiming.Update, disableCancellation.Token);

            UIGambleSynergyViewElement uIGambleSynergyViewElement = _uIGambleSynergyViewElement_dic[viewdata.v2Enum_ARR_SynergyType];

            if (viewdata.gambleSkillData != null)
            {
                UICardMoveSynergyElement uIGoodsDropDirectionElement = GetUIGoodsDropDirectionElement();
                if (uIGoodsDropDirectionElement == null)
                {
                    await uIGambleSynergyViewElement.Explosion(viewdata.beforeData, viewdata.beforeStack,
                    viewdata.afterData, viewdata.afterStack, viewdata.DescendEnhance);
                    //uIGambleSynergyViewElement.AddSynergyViewDirectionOrderData(viewdata);
                    return;
                }

                Vector3 fromPos = viewdata.startpos;

                uIGoodsDropDirectionElement.gameObject.SetActive(true);

                uIGoodsDropDirectionElement.SetCardData(viewdata.gambleSkillData);

                await uIGoodsDropDirectionElement.Explosion(fromPos, uIGambleSynergyViewElement.transform.position, ToEase, ToDuration);

            }

            if (disableCancellation.IsCancellationRequested == true)
            {
                
                uIGambleSynergyViewElement.RefreshSynergyInfo();
                
                if (viewdata.DescendEnhance > 0)
                    uIGambleSynergyViewElement.PlayAddDescendStack(viewdata.DescendEnhance);

                return;
            }

            if (viewdata.beforeData != null && viewdata.beforeData.NextEffectData == null)
            {
                uIGambleSynergyViewElement.RefreshSynergyInfo();

                if (viewdata.DescendEnhance > 0)
                    uIGambleSynergyViewElement.PlayAddDescendStack(viewdata.DescendEnhance);

                return;
            }

            await uIGambleSynergyViewElement.Explosion(viewdata.beforeData, viewdata.beforeStack,
                    viewdata.afterData, viewdata.afterStack, viewdata.DescendEnhance);

            Debug.Log(string.Format("DirectionEndAfterSynergy : {0}", viewdata.afterStack));

            if (Managers.MapManager.Instance.NeedTutotial1() == true 
                && (Managers.GuideInteractorManager.Instance.PlayCardTutorial == true
                || Managers.GuideInteractorManager.Instance.PlayGasSynergyTutorial == true))
            {
                await UniTask.Delay(500, false, PlayerLoopTiming.Update, disableCancellation.Token);

                if (_synergyVerticalLayout != null)
                    _synergyVerticalLayout.enabled = false;

                Managers.BattleSceneManager.Instance.ChangeTimeScale(V2Enum_ARR_BattleSpeed.Pause);
                _tutorialSynergyType = viewdata.v2Enum_ARR_SynergyType;
                if (_synergyViewToturialBtn != null)
                { 
                    _synergyViewToturialBtn.gameObject.SetActive(true);
                    _synergyViewToturialBtn.onClick.AddListener(OnClick_SynergyViewBtn);
                    _synergyViewToturialBtn.transform.position = uIGambleSynergyViewElement.transform.position;
                }

                if (_tutorialBlack != null)
                { 
                    _tutorialBlack.gameObject.SetActive(true);
                    uIGambleSynergyViewElement.transform.SetParent(_tutorialBlack.transform);
                    _tutorialUIGambleSynergyViewElement = uIGambleSynergyViewElement;
                }

                if (Managers.GuideInteractorManager.Instance.PlayCardTutorial == true)
                {
                    if (_tutorialText != null)
                        Managers.LocalStringManager.Instance.SetLocalizeText(_tutorialText, "guide/synergy");

                    Managers.GuideInteractorManager.Instance.StartGuideInteractor(V2Enum_EventType.TutoGambleSynergy);
                }
                else if (Managers.GuideInteractorManager.Instance.PlayGasSynergyTutorial == true)
                {
                    if (_tutorialText != null)
                        Managers.LocalStringManager.Instance.SetLocalizeText(_tutorialText, "guide/gassynergy3");

                    Managers.GuideInteractorManager.Instance.StartGuideInteractor(V2Enum_EventType.TutoGambleGasSynergy);
                }

            }

        }
        //------------------------------------------------------------------------------------
        private UICardMoveSynergyElement GetUIGoodsDropDirectionElement()
        {
            if (m_uIGoodsDropDirectionElements.Count > 0)
                return m_uIGoodsDropDirectionElements.Dequeue();

            GameObject clone = Instantiate(uIGoodsDropDirectionElementObj.gameObject, uIBerserkerGaugeMoveElementRoot);
            if (clone == null)
                return null;

            UICardMoveSynergyElement uIGoodsDropDirectionElement = clone.GetComponent<UICardMoveSynergyElement>();
            uIGoodsDropDirectionElement.Init(PoolElement);

            return uIGoodsDropDirectionElement;
        }
        //------------------------------------------------------------------------------------

        private void PoolElement(UICardMoveSynergyElement uIGoodsDropDirectionElement)
        {
            m_uIGoodsDropDirectionElements.Enqueue(uIGoodsDropDirectionElement);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SynergyViewBtn()
        {
            if (_synergyViewToturialBtn != null)
                _synergyViewToturialBtn.gameObject.SetActive(false);
            Managers.SynergyManager.Instance.ShowSynergyDetailPopup(_tutorialSynergyType);

            Managers.BattleSceneManager.Instance.ChangeOriginBattleSpeed();

            if (_tutorialBlack != null)
                _tutorialBlack.gameObject.SetActive(false);

            if (_tutorialUIGambleSynergyViewElement != null)
            {
                _tutorialUIGambleSynergyViewElement.transform.SetParent(_synergyListRoot.transform);
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshReadyGambleSynergyCombineSkill(GameBerry.Event.RefreshReadyGambleSynergyCombineSkillMsg msg)
        {
            if (msg == null)
                return;

            if (msg.ReadySynergyCombineSkillList == null)
                return;


            if (msg.ReadySynergyCombineSkillList.Count == 0)
                return;

            for (int i = 0; i < msg.ReadySynergyCombineSkillList.Count; ++i)
            {
                Managers.SynergyManager.Instance.AddSynergyCombineSkill(msg.ReadySynergyCombineSkillList[i]);
            }

            if (_missionGoldViewPoint != null)
                Managers.GoodsDropDirectionManager.Instance.ShowDropIn(V2Enum_Goods.Point, V2Enum_Point.InGameGold.Enum32ToInt(), _missionGoldViewPoint.transform.position, 10);

            if (_missionComplete != null)
            {
                _missionComplete.gameObject.SetActive(true);
            }

            PlayFreeSlot().Forget();

            //if (_readyGambleSynergySkill == null || _readyGambleSynergySkillGroup == null)
            //    return;

            //if (msg == null)
            //    _readyGambleSynergySkillGroup.gameObject.SetActive(false);
            //else if (msg.ReadySynergyCombineSkillList.Count <= 0)
            //    _readyGambleSynergySkillGroup.gameObject.SetActive(false);
            //else
            //{
            //    _readyGambleSynergySkillGroup.gameObject.SetActive(true);
            //    _readyGambleSynergySkill.SetText("{0}", msg.ReadySynergyCombineSkillList.Count);
            //}
        }
        //------------------------------------------------------------------------------------
        private async UniTask PlayFreeSlot()
        {
            await UniTask.Delay(2000, false, PlayerLoopTiming.Update, disableCancellation.Token);
            if (_missionComplete != null)
                _missionComplete.gameObject.SetActive(false);
        }

        //------------------------------------------------------------------------------------
        private void PlaySynergyCombineTutorial(GameBerry.Event.PlaySynergyCombineTutorialMsg msg)
        {
            Managers.BattleSceneManager.Instance.ChangeTimeScale(V2Enum_ARR_BattleSpeed.Pause);

            if (_tutorialBlack != null)
                _tutorialBlack.gameObject.SetActive(true);

            if (_tutorialText != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_tutorialText, "guide/synergycombine1");

            if (_showSynergySkill != null)
                _showSynergySkill.gameObject.SetActive(true);

            Managers.GuideInteractorManager.Instance.StartGuideInteractor(V2Enum_EventType.SynergyCombine);
        }
        //------------------------------------------------------------------------------------
    }
}