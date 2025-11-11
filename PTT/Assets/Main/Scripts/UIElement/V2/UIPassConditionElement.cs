using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;

namespace GameBerry.UI
{
    [System.Serializable]
    public class RewardIconElement
    {
        public UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement;

        public Transform AlreadyCheck;

        public Transform Disable;

        public Transform RecvReady;

        public Button m_recvBtn;

        public void SetRewardElement(bool isready
            , double myrewardcount
            , double rrecvrewardcount)
        {
            if (isready == true)
            {
                if (Disable != null)
                    Disable.gameObject.SetActive(false);

                if (AlreadyCheck != null)
                    AlreadyCheck.gameObject.SetActive(myrewardcount <= rrecvrewardcount);

                if (RecvReady != null)
                {
                    if (myrewardcount > rrecvrewardcount)
                    {
                        RecvReady.gameObject.SetActive(true);
                    }
                    else
                        RecvReady.gameObject.SetActive(false);
                }

                if (m_recvBtn != null)
                    m_recvBtn.gameObject.SetActive(myrewardcount > rrecvrewardcount);
            }
            else
            {
                if (AlreadyCheck != null)
                    AlreadyCheck.gameObject.SetActive(false);

                if (Disable != null)
                    Disable.gameObject.SetActive(true);

                if (RecvReady != null)
                    RecvReady.gameObject.SetActive(false);

                if (m_recvBtn != null)
                    m_recvBtn.gameObject.SetActive(false);
            }
        }
    }

    public class UIPassConditionElement : InfiniteScrollItem
    {
        [SerializeField]
        private RewardIconElement m_freeReward;

        [SerializeField]
        private RewardIconElement m_paidReward;

        [SerializeField]
        private TMP_Text m_currentStep;

        [SerializeField]
        private Transform _readytrans;

        private PassData m_currentPassData = null;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (m_freeReward != null && m_freeReward.m_recvBtn != null)
                m_freeReward.m_recvBtn.onClick.AddListener(OnClick_RecvReward);

            if (m_paidReward != null && m_paidReward.m_recvBtn != null)
                m_paidReward.m_recvBtn.onClick.AddListener(OnClick_RecvReward);
        }
        //------------------------------------------------------------------------------------
        public override void UpdateData(InfiniteScrollData scrollData)
        {
            PassConditionRewardData playerV3AllyInfo = scrollData as PassConditionRewardData;
            if (playerV3AllyInfo == null)
            {
                m_currentPassData = null;
                return;
            }

            SetRewardElement(playerV3AllyInfo);
        }
        //------------------------------------------------------------------------------------
        public void SetRewardElement(PassConditionRewardData passConditionRewardData)
        {
            if (passConditionRewardData == null)
            {
                m_currentPassData = null;
                return;
            }

            PassData passData = Managers.PassManager.Instance.GetPassData_RewardIndex(passConditionRewardData.ConditionRewardGroupIndex);
            if (passData == null)
            {
                m_currentPassData = null;
                return;
            }

            m_currentPassData = passData;

            double currentStep = Managers.PassManager.Instance.PassCurrentCount(passConditionRewardData.PassType);

            bool isReady = passConditionRewardData.ConditionClearParam.GetDecrypted() <= currentStep;

            if (_readytrans != null)
                _readytrans.gameObject.SetActive(isReady);

            if (m_currentStep != null)
            {
                m_currentStep.gameObject.SetActive(true);
                m_currentStep.text = Managers.PassManager.Instance.GetConvertPassCountText(passConditionRewardData.PassType, passConditionRewardData.ConditionClearParam.GetDecrypted());
            }

            if (m_freeReward != null)
            {
                if (m_freeReward.uIGlobalGoodsRewardIconElement != null)
                {
                    V2Enum_Goods v2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(passConditionRewardData.FreeRewardGoodsParam1);

                    m_freeReward.uIGlobalGoodsRewardIconElement.SetRewardElement(
                        v2Enum_Goods,
                        passConditionRewardData.FreeRewardGoodsParam1,
                        Managers.GoodsManager.Instance.GetGoodsSprite(v2Enum_Goods, passConditionRewardData.FreeRewardGoodsParam1),
                        Managers.GoodsManager.Instance.GetGoodsGrade(v2Enum_Goods, passConditionRewardData.FreeRewardGoodsParam1),
                        passConditionRewardData.FreeRewardGoodsParam2);
                }

                m_freeReward.SetRewardElement(isReady, passConditionRewardData.ConditionClearParam.GetDecrypted(), Managers.PassManager.Instance.GetFreeRewardStep(passData));
            }

            if (m_paidReward != null)
            {
                if(m_paidReward.uIGlobalGoodsRewardIconElement != null)
                {
                    V2Enum_Goods v2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(passConditionRewardData.PaidRewardGoodsParam1);

                    m_paidReward.uIGlobalGoodsRewardIconElement.SetRewardElement(
                        v2Enum_Goods,
                        passConditionRewardData.PaidRewardGoodsParam1,
                        Managers.GoodsManager.Instance.GetGoodsSprite(v2Enum_Goods, passConditionRewardData.PaidRewardGoodsParam1),
                        Managers.GoodsManager.Instance.GetGoodsGrade(v2Enum_Goods, passConditionRewardData.PaidRewardGoodsParam1),
                        passConditionRewardData.PaidRewardGoodsParam2);
                }

                if (isReady == true)
                    isReady = Managers.PassManager.Instance.IsEnable(passData);

                m_paidReward.SetRewardElement(isReady, passConditionRewardData.ConditionClearParam.GetDecrypted(), Managers.PassManager.Instance.GetPaidRewardStep(passData));
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_RecvReward()
        {
            if (m_currentPassData == null)
                return;

            Managers.PassManager.Instance.RecvPassReward(m_currentPassData);
        }
        //------------------------------------------------------------------------------------
        public RewardIconElement GetFreeRewardIconElement()
        {
            return m_freeReward;
        }
        //------------------------------------------------------------------------------------
        public UIGuideInteractor SetPassGuideBtn()
        {
            if (m_freeReward != null && m_freeReward.m_recvBtn != null)
            {
                UIGuideInteractor uIGuideInteractor = m_freeReward.m_recvBtn.gameObject.AddComponent<UIGuideInteractor>();
                uIGuideInteractor.MyGuideType = V2Enum_EventType.PassFreeRewardClaim;
                uIGuideInteractor.MyStepID = 3;
                uIGuideInteractor.GuideInteratorActionType = Managers.GuideInteratorActionType.End;
                uIGuideInteractor.IsAutoSetting = false;
                uIGuideInteractor.ConnectInteractor();

                return uIGuideInteractor;
            }

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}