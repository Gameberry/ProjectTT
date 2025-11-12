using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class StageCooltimeRewardDialog : IDialog
    {
        [SerializeField]
        private TMP_Text m_idleMinute;

        [SerializeField]
        private Button m_adRewardBtn;

        [SerializeField]
        private List<Button> m_exitBtn;

        [SerializeField]
        private Button m_okBtn;

        [SerializeField]
        private TMP_Text m_goldAmount;

        [SerializeField]
        private Transform m_rewardRoot;

        private List<UIGlobalGoodsRewardIconElement> m_uIGlobalGoodsRewardIconElements = new List<UIGlobalGoodsRewardIconElement>();

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (m_adRewardBtn != null)
                m_adRewardBtn.onClick.AddListener(OnClick_AdRewardBtn);

            if (m_exitBtn != null)
            {
                for (int i = 0; i < m_exitBtn.Count; ++i)
                {
                    if (m_exitBtn[i] != null)
                        m_exitBtn[i].onClick.AddListener(OnClick_ExitBtn);
                }
            }
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            if (m_idleMinute != null)
                m_idleMinute.SetText(Managers.LocalStringManager.Instance.GetLocalString(Define.MinuteLocalKey)
                    , Managers.TimeManager.Instance.GetStageCoolTimeRewardMinute());

            List<RewardData> rewardDatas = Managers.TimeManager.Instance.GetStageCoolTimeRewardDatas();

            for (int i = 0; i < rewardDatas.Count; ++i)
            {
                RewardData rewardData = rewardDatas[i];
                UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = Managers.RewardManager.Instance.GetGoodsRewardIcon();
                if (uIGlobalGoodsRewardIconElement == null)
                    break;

                uIGlobalGoodsRewardIconElement.transform.SetParent(m_rewardRoot);
                uIGlobalGoodsRewardIconElement.gameObject.SetActive(true);

                uIGlobalGoodsRewardIconElement.SetRewardElement(rewardData);

                m_uIGlobalGoodsRewardIconElements.Add(uIGlobalGoodsRewardIconElement);
            }

            Managers.SoundManager.Instance.PlaySound("AccReward");
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            while (m_uIGlobalGoodsRewardIconElements.Count > 0)
            {
                UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = m_uIGlobalGoodsRewardIconElements[0];

                if (Managers.RewardManager.isAlive == true)
                    Managers.RewardManager.Instance.PoolGoodsRewardIcon(uIGlobalGoodsRewardIconElement);
                m_uIGlobalGoodsRewardIconElements.Remove(uIGlobalGoodsRewardIconElement);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_AdRewardBtn()
        {
            //UnityPlugins.appLovin.ShowRewardedAd(() =>
            //{
            //    List<RewardData> rewards = Managers.TimeManager.Instance.GetStageCoolTimeRewardDatas();

            //    for (int i = 0; i < rewards.Count; ++i)
            //    {
            //        Managers.GoodsDropDirectionManager.Instance.ShowDropIn(rewards[i].V2Enum_Goods, rewards[i].Index, m_adRewardBtn.transform.position, rewards[i].Amount.GetDecrypted().ToInt());
            //    }

            //    Managers.TimeManager.Instance.DoAdStageCoolTimeReward();
            //    UIManager.DialogExit<StageCooltimeRewardDialog>();

            //    ThirdPartyLog.Instance.SendLog_AD_ViewEvent("coolrewrad", 0, GameBerry.Define.IsAdFree == true ? 1 : 2);
            //});
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ExitBtn()
        {
            List<RewardData> rewards = Managers.TimeManager.Instance.GetStageCoolTimeRewardDatas();

            for (int i = 0; i < rewards.Count; ++i)
            {
                Managers.GoodsDropDirectionManager.Instance.ShowDropIn(rewards[i].V2Enum_Goods, rewards[i].Index, m_okBtn.transform.position, rewards[i].Amount.GetDecrypted().ToInt());
            }

            Managers.TimeManager.Instance.ReleaseStageCoolTimeReward();
            UIManager.DialogExit<StageCooltimeRewardDialog>();
        }
        //------------------------------------------------------------------------------------
    }
}