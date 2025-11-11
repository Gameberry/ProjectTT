using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class ShopSummonResultPopupDialog : IDialog
    {
        [SerializeField]
        private Toggle m_allyShowDirSkipToggle;

        [SerializeField]
        private UIShopSummonElement m_summonBtnElement;

        private List<UIGlobalGoodsRewardIconElement> m_uIGlobalGoodsRewardIconElements = new List<UIGlobalGoodsRewardIconElement>();

        private Transform m_rewardRoot;

        [SerializeField]
        private ScrollRect m_scrollRect;

        [SerializeField]
        private GridLayoutGroup _gridLayoutGroup;

        [SerializeField]
        private RewardViewPort m_defaultViewPort;

        [SerializeField]
        private TMP_Text _relicDesc;

        public float runeStartDay = 0.5f;
        public float runeEndDay = 0.5f;

        private WaitForSeconds _gachaNormalDelay = new WaitForSeconds(0.01f);

        private WaitForSeconds _runeStartDay = new WaitForSeconds(0.3f);
        private WaitForSeconds _runeEndDay = new WaitForSeconds(0.3f);

        private Coroutine m_summonShowDirection = null;


        V2Enum_SummonType _v2Enum_SummonType = V2Enum_SummonType.Max;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (m_summonBtnElement != null)
                m_summonBtnElement.Init(V2Enum_SummonType.SummonGear);

            if (m_summonBtnElement != null)
                m_summonBtnElement.gameObject.SetActive(false);

            Message.AddListener<GameBerry.Event.SetSummonPopupMsg>(SetSummonPopup);
            Message.AddListener<GameBerry.Event.SetSummonPopup_SummonBtnMsg>(SetSummonPopup_SummonBtn);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.SetSummonPopupMsg>(SetSummonPopup);
            Message.RemoveListener<GameBerry.Event.SetSummonPopup_SummonBtnMsg>(SetSummonPopup_SummonBtn);
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (m_allyShowDirSkipToggle != null)
                m_allyShowDirSkipToggle.isOn = false;

            while (m_uIGlobalGoodsRewardIconElements.Count > 0)
            {
                UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = m_uIGlobalGoodsRewardIconElements[0];
                m_uIGlobalGoodsRewardIconElements.RemoveAt(0);

                if (Managers.RewardManager.isAlive == true)
                    Managers.RewardManager.Instance.PoolGoodsRewardIcon(uIGlobalGoodsRewardIconElement);
            }

            if (m_summonBtnElement != null)
                m_summonBtnElement.gameObject.SetActive(false);

            if (m_summonShowDirection != null)
            {
                StopCoroutine(m_summonShowDirection);
                m_summonShowDirection = null;

                for (int i = 0; i < m_uIGlobalGoodsRewardIconElements.Count; ++i)
                {
                    m_uIGlobalGoodsRewardIconElements[i].gameObject.SetActive(true);
                    m_uIGlobalGoodsRewardIconElements[i].ShowOpenGacha();
                }

                if (m_summonBtnElement != null)
                    m_summonBtnElement.gameObject.SetActive(true);

                return;
            }
        }
        //------------------------------------------------------------------------------------
        private void SetSummonPopup(GameBerry.Event.SetSummonPopupMsg msg)
        {
            while (m_uIGlobalGoodsRewardIconElements.Count > 0)
            {
                UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = m_uIGlobalGoodsRewardIconElements[0];
                m_uIGlobalGoodsRewardIconElements.RemoveAt(0);
                Managers.RewardManager.Instance.PoolGoodsRewardIcon(uIGlobalGoodsRewardIconElement);
            }

            if (m_summonBtnElement != null)
                m_summonBtnElement.gameObject.SetActive(false);

            RewardViewPort selectviewPort = m_defaultViewPort;

            int line = (msg.RewardDatas.Count / selectviewPort.GridLayoutGroup.constraintCount) + 1;

            bool needScroll = true;

            //if (msg.GoodsType == V2Enum_Goods.Ally)
            //    needScroll = line >= 5;
            //else
            //    needScroll = line >= 9;

            if (_gridLayoutGroup != null)
            {
                if (msg.RewardDatas.Count > 15)
                    _gridLayoutGroup.constraintCount = 10;
                else
                    _gridLayoutGroup.constraintCount = 5;
            }

            if (m_scrollRect != null)
            {
                m_scrollRect.vertical = needScroll;
                m_scrollRect.viewport = selectviewPort.ViewPort;
                m_scrollRect.content = selectviewPort.Content;
            }

            m_rewardRoot = selectviewPort.Content;

            if (_relicDesc != null)
            {
                _relicDesc.gameObject.SetActive(msg.GoodsType == V2Enum_Goods.Relic);
                _relicDesc.SetText(string.Empty);
            }


            for (int i = 0; i < msg.RewardDatas.Count; ++i)
            {
                RewardData rewardData = msg.RewardDatas[i];

                if (rewardData == null)
                    continue;

                UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = Managers.RewardManager.Instance.GetGoodsRewardIcon();
                if (uIGlobalGoodsRewardIconElement == null)
                    break;

                uIGlobalGoodsRewardIconElement.transform.SetParent(m_rewardRoot);
                uIGlobalGoodsRewardIconElement.gameObject.SetActive(false);

                uIGlobalGoodsRewardIconElement.SetRewardElement(rewardData);
                uIGlobalGoodsRewardIconElement.ShowOpenGacha();

                if (rewardData.IsPoolData == true)
                    Managers.RewardManager.Instance.PoolRewardData(rewardData);

                if (rewardData.V2Enum_Goods == V2Enum_Goods.Relic)
                {
                    RelicData synergyEffectData = Managers.RelicManager.Instance.GetRelicData(rewardData.Index);
                    SkillInfo skillInfo = Managers.RelicManager.Instance.GetSynergyEffectSkillInfo(synergyEffectData);

                    int level = skillInfo == null ? 0 : skillInfo.Level;


                    SkillBaseData skillBaseData = Managers.SkillManager.Instance.GetSkillBaseData(synergyEffectData.SynergySkillData.MainSkillTypeParam1);

                    if (skillBaseData != null)
                    {
                        double factor = 0;
                        double addfactor = 0;

                        SkillDamageData skillDamageData = skillBaseData.SkillDamageIndex;
                        if (skillDamageData != null)
                        {
                            addfactor = (skillDamageData.DamageFactorPerLevel * level);

                            factor = skillDamageData.DamageFactorBase + addfactor;
                        }

                        addfactor *= Define.PerStatPrintRecoverValueTemp;
                        factor *= Define.PerStatPrintRecoverValueTemp;

                        string desc = string.Format(Managers.LocalStringManager.Instance.GetLocalString(synergyEffectData.DescLocalKey), factor, addfactor);

                        _relicDesc.SetText(desc);
                    }
                }

                m_uIGlobalGoodsRewardIconElements.Add(uIGlobalGoodsRewardIconElement);
            }

            msg.RewardDatas.Clear();

            if (m_scrollRect != null)
            {
                m_scrollRect.normalizedPosition = Vector2.zero;
            }

            if (m_summonShowDirection != null)
                StopCoroutine(m_summonShowDirection);

            m_summonShowDirection = StartCoroutine(SummonResultShowDirection());

            //if (m_summonBtnElement != null)
            //    m_summonBtnElement.gameObject.SetActive(true);
        }
        //------------------------------------------------------------------------------------
        private IEnumerator SummonResultShowDirection()
        {
            for (int i = 0; i < m_uIGlobalGoodsRewardIconElements.Count; ++i)
            {
                UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = m_uIGlobalGoodsRewardIconElements[i];

                V2Enum_Grade v2Enum_Grade = uIGlobalGoodsRewardIconElement.GetGrade();

                if (v2Enum_Grade != V2Enum_Grade.Max && v2Enum_Grade >= V2Enum_Grade.B)
                {
                    yield return _runeStartDay;
                }

                m_uIGlobalGoodsRewardIconElements[i].gameObject.SetActive(true);
                m_uIGlobalGoodsRewardIconElements[i].ShowOpenGacha();


                if (v2Enum_Grade != V2Enum_Grade.Max && v2Enum_Grade >= V2Enum_Grade.B)
                {
                    m_uIGlobalGoodsRewardIconElements[i].EnableSpotLight(true);
                    yield return _runeEndDay;
                }
                else
                    yield return _gachaNormalDelay;
            }


            if (_v2Enum_SummonType != V2Enum_SummonType.Max)
            {
                if (m_summonBtnElement != null)
                    m_summonBtnElement.gameObject.SetActive(true);
            }

            m_summonShowDirection = null;
        }
        //------------------------------------------------------------------------------------
        private void SetSummonPopup_SummonBtn(GameBerry.Event.SetSummonPopup_SummonBtnMsg msg)
        {
            if (m_summonBtnElement != null)
            {
                if (msg.v2Enum_SummonType == V2Enum_SummonType.Max)
                    m_summonBtnElement.gameObject.SetActive(false);
                else 
                {
                    m_summonBtnElement.gameObject.SetActive(true);
                    m_summonBtnElement.SetSummonElement(msg.v2Enum_SummonType);
                }
                
            }

            _v2Enum_SummonType = msg.v2Enum_SummonType;
        }
        //------------------------------------------------------------------------------------
    }
}
