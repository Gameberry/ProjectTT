using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    [System.Serializable]
    public class RewardViewPort
    {
        public V2Enum_Goods V2Enum_Goods;
        public RectTransform ViewPort;
        public RectTransform Content;
        public GridLayoutGroup GridLayoutGroup;
    }

    public class InGameRewardPopupDialog : IDialog
    {
        [SerializeField]
        private Transform m_titleGroup;

        [SerializeField]
        private TMP_Text m_title;

        private Queue<UIGlobalGoodsRewardIconElement> m_uIGlobalGoodsRewardIconElements = new Queue<UIGlobalGoodsRewardIconElement>();

        private Transform m_rewardRoot;

        [SerializeField]
        private ScrollRect m_scrollRect;

        [SerializeField]
        private GridLayoutGroup _gridLayoutGroup;

        [SerializeField]
        private RewardViewPort m_defaultViewPort;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (m_titleGroup != null)
                m_titleGroup.gameObject.SetActive(false);

            Message.AddListener<GameBerry.Event.SetInGameRewardPopupMsg>(SetInGameRewardPopup);
            Message.AddListener<GameBerry.Event.SetInGameRewardPopup_TitleMsg>(SetInGameRewardPopup_Title);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.SetInGameRewardPopupMsg>(SetInGameRewardPopup);
            Message.RemoveListener<GameBerry.Event.SetInGameRewardPopup_TitleMsg>(SetInGameRewardPopup_Title);
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            while (m_uIGlobalGoodsRewardIconElements.Count > 0)
            {
                UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = m_uIGlobalGoodsRewardIconElements.Dequeue();

                if (uIGlobalGoodsRewardIconElement != null)
                {
                    RewardData rewardData = uIGlobalGoodsRewardIconElement.GetRewardData();
                    if (rewardData != null)
                    {
                        double amount = rewardData.Amount.GetDecrypted();
                        if (amount > 10)
                            amount = 10;
                        Managers.GoodsDropDirectionManager.Instance.ShowDropIn(rewardData.V2Enum_Goods, rewardData.Index, uIGlobalGoodsRewardIconElement.transform.position, amount.ToInt());
                    }
                }

                if (Managers.RewardManager.isAlive == true)
                    Managers.RewardManager.Instance.PoolGoodsRewardIcon(uIGlobalGoodsRewardIconElement);
            }

            if (m_titleGroup != null)
                m_titleGroup.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        private void SetInGameRewardPopup(GameBerry.Event.SetInGameRewardPopupMsg msg)
        {
            while (m_uIGlobalGoodsRewardIconElements.Count > 0)
            {
                UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = m_uIGlobalGoodsRewardIconElements.Dequeue();

                Managers.RewardManager.Instance.PoolGoodsRewardIcon(uIGlobalGoodsRewardIconElement);
            }

            RewardViewPort selectviewPort = m_defaultViewPort;

            //int line = (msg.RewardDatas.Count / selectviewPort.GridLayoutGroup.constraintCount) + 1;

            //bool needScroll = false;

            //if (msg.GoodsType == V2Enum_Goods.Ally)
            //    needScroll = line >= 5;
            //else
            //    needScroll = line >= 6;

            if (_gridLayoutGroup != null)
            {
                if (msg.RewardDatas.Count > 15)
                    _gridLayoutGroup.constraintCount = 10;
                else
                    _gridLayoutGroup.constraintCount = 5;
            }

            if (m_scrollRect != null)
            {
                //m_scrollRect.vertical = needScroll;
                m_scrollRect.vertical = true;
                m_scrollRect.viewport = selectviewPort.ViewPort;
                m_scrollRect.content = selectviewPort.Content;
            }

            m_rewardRoot = selectviewPort.Content;

            for (int i = 0; i < msg.RewardDatas.Count; ++i)
            {
                RewardData rewardData = msg.RewardDatas[i];

                if (rewardData == null)
                    continue;

                UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = Managers.RewardManager.Instance.GetGoodsRewardIcon();
                if (uIGlobalGoodsRewardIconElement == null)
                    break;

                uIGlobalGoodsRewardIconElement.transform.SetParent(m_rewardRoot);
                uIGlobalGoodsRewardIconElement.gameObject.SetActive(true);

                if (rewardData.V2Enum_Goods == V2Enum_Goods.TimePoint)
                {
                    uIGlobalGoodsRewardIconElement.SetRewardElement(
                    V2Enum_Goods.Point,
                    rewardData.Index,
                    Managers.GoodsManager.Instance.GetGoodsSprite(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index),
                    Managers.GoodsManager.Instance.GetGoodsGrade(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index),
                    Managers.GoodsManager.Instance.GetTimeGoodsAmount(rewardData.Index, rewardData.Amount));
                }
                else
                { 
                    uIGlobalGoodsRewardIconElement.SetRewardElement(rewardData);
                }

                if (rewardData.IsPoolData == true)
                    Managers.RewardManager.Instance.PoolRewardData(rewardData);

                m_uIGlobalGoodsRewardIconElements.Enqueue(uIGlobalGoodsRewardIconElement);
            }

            msg.RewardDatas.Clear();

            if (m_scrollRect != null)
            {
                //m_scrollRect.normalizedPosition = Vector2.zero;
                m_scrollRect.normalizedPosition = Vector2.one;
            }
        }
        //------------------------------------------------------------------------------------
        private void SetInGameRewardPopup_Title(GameBerry.Event.SetInGameRewardPopup_TitleMsg msg)
        {
            if (m_titleGroup != null)
                m_titleGroup.gameObject.SetActive(true);

            if (m_title != null)
            {
                m_title.SetText(msg.title);
            }
        }
        //------------------------------------------------------------------------------------
    }
}