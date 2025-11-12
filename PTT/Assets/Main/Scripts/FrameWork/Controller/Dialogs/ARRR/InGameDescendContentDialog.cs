using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace GameBerry.UI
{
    public class InGameDescendContentDialog : IDialog
    {
        [Header("----------Stack----------")]
        [SerializeField]
        private Transform _uIStackViewPopup;

        [SerializeField]
        private List<Button> _uIStackViewExitBtn;

        [SerializeField]
        private Transform _uIStackElement_Root;

        [SerializeField]
        private UISynergyCombineStackElement _uISynergyCombineStackElement;

        private List<UISynergyCombineStackElement> _uIARRRGambleAddSkillElementPool = new List<UISynergyCombineStackElement>();


        [Header("-------SynergyGroup-------")]
        [SerializeField]
        private Transform _synergyTierGroupListRoot;

        [SerializeField]
        private UIInGameDescendElement _uIGambleSynergyTierGroupElement;

        private List<UIInGameDescendElement> _uIInGameDescendElement_Pool = new List<UIInGameDescendElement>();

        private Dictionary<ObscuredInt, UIInGameDescendElement> _uIInGameDescendElement_dic = new Dictionary<ObscuredInt, UIInGameDescendElement>();

        private LinkedList<UIInGameDescendElement> _coolTimeCheckList = new LinkedList<UIInGameDescendElement>();

        private SkillActiveController _skillActiveController;

        [Header("-------GuideInteractor-------")]
        [SerializeField]
        private Transform _interactor;

        [Header("----------DescendEnhance----------")]
        [SerializeField]
        private Transform _descendEnhanceAmount;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            for (int i = 0; i < _uIStackViewExitBtn.Count; ++i)
            {
                _uIStackViewExitBtn[i].onClick.AddListener(OnClick_ExitSynergyStackView);
            }

            Message.AddListener<GameBerry.Event.RefreshInGameReadyDescendMsg>(RefreshInGameReadyDescend);
            Message.AddListener<GameBerry.Event.ChangeInGameActiveDescendMsg>(ChangeInGameActiveDescend);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.RefreshInGameReadyDescendMsg>(RefreshInGameReadyDescend);
            Message.RemoveListener<GameBerry.Event.ChangeInGameActiveDescendMsg>(ChangeInGameActiveDescend);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            RefreshCurrentEquipDescendElement();

            ARRRController aRRRController = Managers.BattleSceneManager.Instance.CurrentBattleScene.MyARRRControllers;
            if (aRRRController != null)
                _skillActiveController = aRRRController.SkillActiveController;

            if (_skillActiveController != null)
                _skillActiveController.RefreshCoolTimeAction += CoolTimeRefresh;

            if (_descendEnhanceAmount != null)
            {
                bool enableamount = false;

                foreach (var pair in DescendContainer.SynergyEquip_Dic)
                {
                    if (pair.Value != -1)
                        enableamount = true;

                    break;
                }

                _descendEnhanceAmount.gameObject.SetActive(enableamount);
            }
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (_skillActiveController != null)
                _skillActiveController.RefreshCoolTimeAction -= CoolTimeRefresh;
        }
        //------------------------------------------------------------------------------------
        private void CoolTimeRefresh()
        {
            var node = _coolTimeCheckList.First;
            while (node != null)
            {
                node.Value.RefreshSkillCoolTime();
                node = node.Next;
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshInGameReadyDescend(GameBerry.Event.RefreshInGameReadyDescendMsg msg)
        {
            List<DescendData> descendDatas = Managers.DescendManager.Instance.GetInActiveDescendDatas();

            foreach (var pair in Managers.DescendManager.Instance.GetEquipSynergyEffect())
            {
                DescendData descendData = Managers.DescendManager.Instance.GetSynergyEffectData(pair.Value);
                if (descendData == null)
                    continue;

                if (_uIInGameDescendElement_dic.ContainsKey(descendData.Index) == false)
                    continue;

                UIInGameDescendElement uIInGameDescendElement = _uIInGameDescendElement_dic[descendData.Index];
                uIInGameDescendElement.Refresh();
            }

            ShowDescendLevelUpInteractor();
        }
        //------------------------------------------------------------------------------------
        private void ChangeInGameActiveDescend(GameBerry.Event.ChangeInGameActiveDescendMsg msg)
        {
            for (int i = 0; i < msg.ActiveDescend.Count; ++i)
            {
                DescendData descendData = msg.ActiveDescend[i];
                if (descendData == null)
                    continue;

                if (_uIInGameDescendElement_dic.ContainsKey(descendData.Index) == false)
                    continue;

                UIInGameDescendElement uIInGameDescendElement = _uIInGameDescendElement_dic[descendData.Index];
                if (descendData.DescendType == Enum_DescendType.DescendPassive)
                    uIInGameDescendElement.SetSkillManageInfo(null);
                else
                {
                    if (_skillActiveController != null && descendData.SynergySkillData != null)
                    {
                        SkillBaseData skillBaseData = Managers.SkillManager.Instance.GetSkillBaseData(descendData.SynergySkillData.MainSkillTypeParam1);
                        SkillManageInfo skillManageInfo = _skillActiveController.GetSkillManagerInfo(skillBaseData);
                        uIInGameDescendElement.SetSkillManageInfo(skillManageInfo);
                        _coolTimeCheckList.AddLast(uIInGameDescendElement);
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_LevelUpFinish(DescendData synergyEffectData)
        {
            ShowDescendLevelUpInteractor();

            return;

            //if (synergyEffectData == null)
            //    return;

            //SetSynergyStack(synergyEffectData.NeedSynergyCount);

            //if (_uIStackViewPopup != null)
            //    _uIStackViewPopup.gameObject.SetActive(true);
        }
        //------------------------------------------------------------------------------------
        private void ShowDescendLevelUpInteractor()
        {
            if (_interactor == null)
                return;

            UIInGameDescendElement guide = null;

            foreach (var pair in _uIInGameDescendElement_dic)
            {
                DescendData descendData = Managers.DescendManager.Instance.GetSynergyEffectData(pair.Key);

                bool canLevelUp = Managers.DescendManager.Instance.CanLevelUp(descendData);
                if (canLevelUp == true)
                {
                    guide = pair.Value;
                    break;
                }
            }

            if (guide == null)
            {
                _interactor.SetParent(dialogView.transform);
                _interactor.gameObject.SetActive(false);
            }
            else
            {
                _interactor.SetParent(guide.transform);
                _interactor.gameObject.SetActive(true);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ExitSynergyStackView()
        {
            if (_uIStackViewPopup != null)
                _uIStackViewPopup.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        private void RefreshCurrentEquipDescendElement()
        {
            _uIInGameDescendElement_dic.Clear();
            _coolTimeCheckList.Clear();

            int equipcount = 0;

            {
                int i = 0;
                foreach (var pair in Managers.DescendManager.Instance.GetEquipSynergyEffect())
                {
                    DescendData descendData = Managers.DescendManager.Instance.GetSynergyEffectData(pair.Value);

                    if (descendData == null)
                        continue;

                    UIInGameDescendElement uISynergyCombineStackElement = null;
                    if (_uIInGameDescendElement_Pool.Count > i)
                        uISynergyCombineStackElement = _uIInGameDescendElement_Pool[i];
                    else
                    {
                        GameObject clone = Instantiate(_uIGambleSynergyTierGroupElement.gameObject, _synergyTierGroupListRoot);
                        uISynergyCombineStackElement = clone.GetComponent<UIInGameDescendElement>();
                        uISynergyCombineStackElement.Init(OnClick_LevelUpFinish);
                        _uIInGameDescendElement_Pool.Add(uISynergyCombineStackElement);
                    }

                    uISynergyCombineStackElement.gameObject.SetActive(true);
                    uISynergyCombineStackElement.SetDescendData(descendData);

                    _uIInGameDescendElement_dic.Add(pair.Value, uISynergyCombineStackElement);

                    i++;
                    equipcount++;
                }
            }
            

            for (int i = equipcount; i < _uIInGameDescendElement_Pool.Count; ++i)
            {
                UIInGameDescendElement uISynergyCombineStackElement = _uIInGameDescendElement_Pool[i];
                uISynergyCombineStackElement.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
    }
}