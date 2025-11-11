using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry.UI
{
    public class InGameGoodsDescPopupDialog : IDialog
    {
        [SerializeField]
        private Button m_exitBtn;

        [SerializeField]
        private TMP_Text m_goodsName;

        [SerializeField]
        private TMP_Text m_goodsDesc;

        [SerializeField]
        private TMP_Text m_timeGoodsAmount;

        [SerializeField]
        private UIGlobalGoodsRewardIconElement m_goodsIcon;

        [SerializeField]
        private Button m_showPercentage;

        private RewardData rewardData = new RewardData();

        [SerializeField]
        private Transform _gearDetailViewGroup;

        [SerializeField]
        private Transform _addSkillEffectDescRoot;

        [SerializeField]
        private UILobbyAddLevelEffectElement _uILobbyAddSkillEffectElement;

        private List<UILobbyAddLevelEffectElement> _uILobbyAddSkillEffectElements = new List<UILobbyAddLevelEffectElement>();


        [SerializeField]
        private Transform _uIStatDetailElementGroup;

        [SerializeField]
        private List<UIStatDetailElement> _uIStatDetailElement = new List<UIStatDetailElement>();




        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (m_showPercentage != null)
                m_showPercentage.onClick.AddListener(OnClick_ShowPercentage);

            Message.AddListener<GameBerry.Event.SetGoodsDescPopupMsg>(SetGoodsDescPopup);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.SetGoodsDescPopupMsg>(SetGoodsDescPopup);
        }
        //------------------------------------------------------------------------------------
        private void SetGoodsDescPopup(GameBerry.Event.SetGoodsDescPopupMsg msg)
        {
            if (msg.v2Enum_Goods == V2Enum_Goods.TimePoint)
            {
                if (m_timeGoodsAmount != null)
                {
                    double amount = Managers.GoodsManager.Instance.GetTimeGoodsAmount(msg.index, msg.timeGoodsTime);
                    m_timeGoodsAmount.SetText(Util.GetAlphabetNumber(amount));
                    m_timeGoodsAmount.gameObject.SetActive(true);
                }
                msg.v2Enum_Goods = V2Enum_Goods.Point;
            }
            else
            {
                if (m_timeGoodsAmount != null)
                    m_timeGoodsAmount.gameObject.SetActive(false);
            }

            rewardData.V2Enum_Goods = msg.v2Enum_Goods;
            rewardData.Index = msg.index;
            rewardData.Amount = 0.0;

            m_goodsIcon.SetRewardElement(rewardData);

            if (m_goodsName != null)
            {
                string localkey = Managers.GoodsManager.Instance.GetGoodsLocalKey(msg.v2Enum_Goods.Enum32ToInt(), msg.index);
                Managers.LocalStringManager.Instance.SetLocalizeText(m_goodsName, localkey);
            }

            if (msg.v2Enum_Goods == V2Enum_Goods.Gear)
            {
                if (_gearDetailViewGroup != null)
                    _gearDetailViewGroup.gameObject.SetActive(true);

                if (_uIStatDetailElementGroup != null)
                    _uIStatDetailElementGroup.gameObject.SetActive(true);

                if (m_goodsDesc != null)
                    m_goodsDesc.gameObject.SetActive(false);

                GearData gearData = Managers.GearManager.Instance.GetSynergyEffectData(msg.index);
                SetGearDesc(gearData);
            }
            else
            {
                if (_gearDetailViewGroup != null)
                    _gearDetailViewGroup.gameObject.SetActive(false);

                if (_uIStatDetailElementGroup != null)
                    _uIStatDetailElementGroup.gameObject.SetActive(false);

                if (m_goodsDesc != null)
                {
                    m_goodsDesc.gameObject.SetActive(true);
                    string descKey = string.Format("{0}/desc/{1}", msg.v2Enum_Goods.ToString().ToCamelCase(), msg.index);

                    Managers.LocalStringManager.Instance.SetLocalizeText(m_goodsDesc, descKey);
                }
            }



            if (m_showPercentage != null)
                m_showPercentage.gameObject.SetActive(false);

            if (msg.v2Enum_Goods == V2Enum_Goods.Box)
            {
                V2Enum_BoxType v2Enum_BoxType = Managers.BoxManager.Instance.GetBoxType(msg.index);
                if (v2Enum_BoxType == V2Enum_BoxType.RandomTypeBox)
                {
                    if (m_showPercentage != null)
                        m_showPercentage.gameObject.SetActive(true);
                }
            }
        }
        //------------------------------------------------------------------------------------
        private void SetGearDesc(GearData gearData)
        {
            if (gearData == null)
                return;

            GearOptionData gearOptionData = Managers.GearManager.Instance.GetGearOptionData(gearData.GearType, gearData.SynergyType);

            int skilleffectcount = 0;

            foreach (var pair in gearOptionData.GearSkills)
            {
                UILobbyAddLevelEffectElement uILobbyAddLevelEffectElement = null;

                if (_uILobbyAddSkillEffectElements.Count > skilleffectcount)
                    uILobbyAddLevelEffectElement = _uILobbyAddSkillEffectElements[skilleffectcount];
                else
                {
                    GameObject clone = Instantiate(_uILobbyAddSkillEffectElement.gameObject, _addSkillEffectDescRoot);

                    uILobbyAddLevelEffectElement = clone.GetComponent<UILobbyAddLevelEffectElement>();

                    _uILobbyAddSkillEffectElements.Add(uILobbyAddLevelEffectElement);
                }


                uILobbyAddLevelEffectElement.SetGearDesc(string.Format("gear/desc/{0}", pair.Value), pair.Key);
                uILobbyAddLevelEffectElement.SetEnableDimmedImage(gearData.Grade < pair.Key);
                uILobbyAddLevelEffectElement.gameObject.SetActive(true);

                skilleffectcount++;
            }

            for (int i = skilleffectcount; i < _uILobbyAddSkillEffectElements.Count; ++i)
            {
                _uILobbyAddSkillEffectElements[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < gearData.OwnEffect.Count; ++i)
            {
                if (_uIStatDetailElement.Count <= i)
                    break;

                OperatorOverrideStat operatorOverrideStat = gearData.OwnEffect[i];
                UIStatDetailElement uIStatDetailElement = _uIStatDetailElement[i];
                double statvalue = Managers.GearManager.Instance.GetStatValue(0, operatorOverrideStat);

                uIStatDetailElement.SetStatData(Managers.ARRRStatManager.Instance.GetCharacterBaseStatData(operatorOverrideStat.BaseStat));
                uIStatDetailElement.RefrashValue(statvalue);
                uIStatDetailElement.gameObject.SetActive(true);
            }

            for (int i = gearData.OwnEffect.Count; i < _uIStatDetailElement.Count; ++i)
            {
                _uIStatDetailElement[i].gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ShowPercentage()
        {
            Managers.BoxManager.Instance.ShowRandomBoxPercentage(rewardData.Index);
        }
        //------------------------------------------------------------------------------------
    }
}