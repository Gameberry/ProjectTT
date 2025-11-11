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
    public class InGameGambleSynergyCombineDetailDialog : IDialog
    {
        [Header("----------Skill----------")]
        [SerializeField]
        private Transform _uISkillElement_Root;

        [SerializeField]
        private UISynergyCombineSkillElement _uISynergyCombineSkillElement;

        [SerializeField]
        private ScrollRect _skillScrollRect;

        private Dictionary<SynergyCombineData, UISynergyCombineSkillElement> _uISynergyCombineSkillElements_Dic = new Dictionary<SynergyCombineData, UISynergyCombineSkillElement>();

        [SerializeField]
        private Transform _synergyRewardGroup;

        [SerializeField]
        private TMP_Text _skillName;

        [SerializeField]
        private TMP_Text _skillDesc;

        [Header("----------Stack----------")]
        [SerializeField]
        private Transform _uIStackElement_Root;

        [SerializeField]
        private UISynergyCombineStackElement _uISynergyCombineStackElement;

        private List<UISynergyCombineStackElement> _uIARRRGambleAddSkillElementPool = new List<UISynergyCombineStackElement>();

        [SerializeField]
        private Button _getSynergyCombineBtn;

        [SerializeField]
        private TMP_Text _getSynergyCombineText;


        [Header("----------Tutorial----------")]
        [SerializeField]
        private Transform _tutorialBlack;

        [SerializeField]
        private TMP_Text _tutorialText;



        [SerializeField]
        private Button _tutorial2Btn;

        [SerializeField]
        private UIGuideInteractor _tutorial2Interactor;

        private Transform _uIStackElement_Parent;
        private int _uIStackElement_Sibling;



        [SerializeField]
        private Button _tutorial3Btn;

        [SerializeField]
        private UIGuideInteractor _tutorial3Interactor;

        private Transform _synergyRewardGroup_Parent;
        private int _synergyRewardGroup_Sibling;


        private SynergyCombineData _currentGambleSynergyCombineData = null;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (_getSynergyCombineBtn != null)
                _getSynergyCombineBtn.onClick.AddListener(OnClick_GetSynergyCombineSkill);

            if (_tutorial2Btn != null)
            { 
                _tutorial2Btn.onClick.AddListener(OnClick_Tutorial2Btn);
                _tutorial2Btn.gameObject.SetActive(false);
            }

            if (_tutorial3Btn != null)
            { 
                _tutorial3Btn.onClick.AddListener(OnClick_Tutorial3Btn);
                _tutorial3Btn.gameObject.SetActive(false);
            }

            if (_tutorial2Interactor != null)
                _tutorial2Interactor.ConnectInteractor();


            if (_tutorial3Interactor != null)
                _tutorial3Interactor.ConnectInteractor();

            List<SynergyCombineData> gambleSynergyCombineDatas = Managers.SynergyManager.Instance.GetAllGambleSynergyCombineData();
            
            for (int i = 0; i < gambleSynergyCombineDatas.Count; ++i)
            {
                SynergyCombineData gambleSynergyCombineData = gambleSynergyCombineDatas[i];

                GameObject clone = Instantiate(_uISynergyCombineSkillElement.gameObject, _uISkillElement_Root);
                UISynergyCombineSkillElement uIARRRSkillCoolTimeElement = clone.GetComponent<UISynergyCombineSkillElement>();
                uIARRRSkillCoolTimeElement.Init(OnClick_GambleSynergyCombineElement);
                uIARRRSkillCoolTimeElement.SetSynergyDetailView(gambleSynergyCombineData);
                uIARRRSkillCoolTimeElement.SetClickEnable(false);
                uIARRRSkillCoolTimeElement.RefreshElement();

                _uISynergyCombineSkillElements_Dic.Add(gambleSynergyCombineData, uIARRRSkillCoolTimeElement);
            }
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            RefreshSkillAllIcon();

            if (_skillScrollRect != null)
                _skillScrollRect.normalizedPosition = Vector2.one;

            if (Managers.MapManager.Instance.NeedTutotial2() == true 
                && Managers.GuideInteractorManager.Instance.PlaySynergyCombineTutorial == true)
            {
                if (_tutorialBlack != null)
                { 
                    _tutorialBlack.gameObject.SetActive(true);

                    if (_uIStackElement_Root != null)
                    {
                        _uIStackElement_Parent = _uIStackElement_Root.parent;
                        _uIStackElement_Sibling = _uIStackElement_Root.GetSiblingIndex();

                        _uIStackElement_Root.SetParent(_tutorialBlack.transform);

                        if (_tutorial2Btn != null)
                        {
                            _tutorial2Btn.gameObject.SetActive(true);
                            _tutorial2Btn.transform.position = _uIStackElement_Root.position;
                            _tutorial2Btn.transform.SetAsLastSibling();
                        }
                    }
                }

                if (_tutorialText != null)
                    Managers.LocalStringManager.Instance.SetLocalizeText(_tutorialText, "guide/synergycombine2");
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_GambleSynergyCombineElement(SynergyCombineData gambleSynergyCombineData)
        {
            SetSynergyDetailView(gambleSynergyCombineData);
        }
        //------------------------------------------------------------------------------------
        private void RefreshSkillAllIcon()
        {
            List<SynergyCombineData> gambleSynergyCombineDatas = Managers.SynergyManager.Instance.GetAllGambleSynergyCombineData();

            gambleSynergyCombineDatas.Sort(Managers.SynergyManager.Instance.SortGambleSynergyCombineDatas);

            for (int i = 0; i < gambleSynergyCombineDatas.Count; ++i)
            {
                SynergyCombineData gambleSynergyCombineData = gambleSynergyCombineDatas[i];
                if (_uISynergyCombineSkillElements_Dic.ContainsKey(gambleSynergyCombineData) == false)
                    continue;

                UISynergyCombineSkillElement uIARRRSkillCoolTimeElement = _uISynergyCombineSkillElements_Dic[gambleSynergyCombineData];
                uIARRRSkillCoolTimeElement.RefreshElement();
                uIARRRSkillCoolTimeElement.transform.SetAsLastSibling();
            }

            if (gambleSynergyCombineDatas.Count > 0)
                SetSynergyDetailView(gambleSynergyCombineDatas[0]);
        }
        //------------------------------------------------------------------------------------
        private void SetSynergyDetailView(SynergyCombineData gambleSynergyCombineData)
        {
            if (_currentGambleSynergyCombineData != null)
            {
                if (_uISynergyCombineSkillElements_Dic.ContainsKey(_currentGambleSynergyCombineData) == true)
                    _uISynergyCombineSkillElements_Dic[_currentGambleSynergyCombineData].SetClickEnable(false);
            }

            _currentGambleSynergyCombineData = gambleSynergyCombineData;

            {
                int i = 0;
                foreach (var pair in gambleSynergyCombineData.NeedSynergyCount)
                {
                    UISynergyCombineStackElement uISynergyCombineStackElement = null;
                    if (_uIARRRGambleAddSkillElementPool.Count > i)
                        uISynergyCombineStackElement = _uIARRRGambleAddSkillElementPool[i];
                    else
                    {
                        GameObject clone = Instantiate(_uISynergyCombineStackElement.gameObject, _uIStackElement_Root);
                        uISynergyCombineStackElement = clone.GetComponent<UISynergyCombineStackElement>();
                        _uIARRRGambleAddSkillElementPool.Add(uISynergyCombineStackElement);
                    }

                    uISynergyCombineStackElement.gameObject.SetActive(true);
                    uISynergyCombineStackElement.SetSynergyStack(pair.Key, pair.Value);

                    i++;
                }
            }
            
            for (int i = gambleSynergyCombineData.NeedSynergyCount.Count; i < _uIARRRGambleAddSkillElementPool.Count; ++i)
            {
                UISynergyCombineStackElement uISynergyCombineStackElement = _uIARRRGambleAddSkillElementPool[i];
                uISynergyCombineStackElement.gameObject.SetActive(false);
            }

            bool isReady = true;

            if (_getSynergyCombineBtn != null)
            {
                if (Managers.SynergyManager.Instance.IsAlReadySynergyCombineSkill(gambleSynergyCombineData) == true)
                {
                    isReady = false;
                }
                else
                {
                    if (Managers.SynergyManager.Instance.IsReadySynergyCombineSkill(gambleSynergyCombineData) == true)
                        isReady = true;
                    else
                        isReady = false;
                }

                _getSynergyCombineBtn.interactable = isReady;
            }

            if (_getSynergyCombineText != null)
            { 
                _getSynergyCombineText.color = isReady == true ? Color.white : Color.red;
                if (Managers.SynergyManager.Instance.IsAlReadySynergyCombineSkill(gambleSynergyCombineData) == true)
                    _getSynergyCombineText.SetText(Managers.LocalStringManager.Instance.GetLocalString("synergycombine/getdone"));
                else
                    _getSynergyCombineText.SetText(Managers.LocalStringManager.Instance.GetLocalString("synergycombine/get"));
            }


            if (_skillName != null)
            {
                _skillName.SetText(Managers.LocalStringManager.Instance.GetLocalString(string.Format("missionname/{0}", gambleSynergyCombineData.SynergySkillData.Index)));
            }
            
            if (_skillDesc != null)
            {
                _skillDesc.SetText(Managers.LocalStringManager.Instance.GetLocalString(string.Format("missiondesc/{0}", gambleSynergyCombineData.SynergySkillData.Index)));
            }

            if (_currentGambleSynergyCombineData != null)
            {
                if (_uISynergyCombineSkillElements_Dic.ContainsKey(_currentGambleSynergyCombineData) == true)
                    _uISynergyCombineSkillElements_Dic[_currentGambleSynergyCombineData].SetClickEnable(true);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_GetSynergyCombineSkill()
        {
            if (_currentGambleSynergyCombineData != null)
            {
                if (Managers.SynergyManager.Instance.AddSynergyCombineSkill(_currentGambleSynergyCombineData) == true)
                {
                    RefreshSkillAllIcon();
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Tutorial2Btn()
        {
            if (_tutorialBlack != null)
            {
                _tutorialBlack.gameObject.SetActive(true);

                if (_uIStackElement_Root != null)
                {
                    _uIStackElement_Root.SetParent(_uIStackElement_Parent);
                    _uIStackElement_Root.SetSiblingIndex(_uIStackElement_Sibling);
                }

                if (_tutorial2Btn != null)
                    _tutorial2Btn.gameObject.SetActive(false);

                if (_synergyRewardGroup != null)
                {
                    _synergyRewardGroup_Parent = _synergyRewardGroup.parent;
                    _synergyRewardGroup_Sibling = _synergyRewardGroup.GetSiblingIndex();

                    _synergyRewardGroup.SetParent(_tutorialBlack.transform);

                    if (_tutorial3Btn != null)
                    {
                        _tutorial3Btn.gameObject.SetActive(true);
                        _tutorial3Btn.transform.position = _synergyRewardGroup.position;
                        _tutorial3Btn.transform.SetAsLastSibling();
                    }
                }
            }

            if (_tutorialText != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_tutorialText, "guide/synergycombine3");
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Tutorial3Btn()
        {
            if (_synergyRewardGroup != null)
            {
                _synergyRewardGroup.SetParent(_synergyRewardGroup_Parent);
                _synergyRewardGroup.SetSiblingIndex(_synergyRewardGroup_Sibling);
            }

            if (_tutorial2Btn != null)
                _tutorial2Btn.gameObject.SetActive(false);

            if (_tutorial3Btn != null)
                _tutorial3Btn.gameObject.SetActive(false);

            if (_tutorialBlack != null)
                _tutorialBlack.gameObject.SetActive(false);

            Managers.GuideInteractorManager.Instance.PlaySynergyCombineTutorial = false;

            Managers.BattleSceneManager.Instance.ChangeOriginBattleSpeed();
        }
        //------------------------------------------------------------------------------------
    }
}