using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.UI;

namespace GameBerry.Managers
{
    public class RewardManager : MonoSingleton<RewardManager>
    {
        private Queue<RewardData> m_rewardPool = new Queue<RewardData>();
        
        private Queue<UIGlobalGoodsRewardIconElement> m_uIGlobalGoodsRewardIconElements = new Queue<UIGlobalGoodsRewardIconElement>();
        private UIGlobalGoodsRewardIconElement m_uIGlobalGoodsRewardIconElement = null;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            ResourceLoader.Instance.Load<GameObject>("ContentResources/UIObjects/UIGlobalGoodsRewardIconElement", o =>
            {
                GameObject obj = o as GameObject;

                if (obj != null)
                    m_uIGlobalGoodsRewardIconElement = obj.GetComponent<UIGlobalGoodsRewardIconElement>();
            });
        }
        //------------------------------------------------------------------------------------
        public RewardData GetRewardData()
        {
            RewardData rewardData = null;

            if (m_rewardPool.Count <= 0)
            {
                rewardData = new RewardData();
                rewardData.IsPoolData = true;
            }
            else
            {
                rewardData = m_rewardPool.Dequeue();

                rewardData.V2Enum_Goods = V2Enum_Goods.Max;
                rewardData.Index = -1;
                rewardData.Amount = 0.0;

                rewardData.DupliIndex = -1;
                rewardData.DupliAmount = 0.0;
            }

            return rewardData;
        }
        //------------------------------------------------------------------------------------
        public void PoolRewardData(RewardData rewardData)
        {
            if (rewardData.IsPoolData == false)
                return;

            m_rewardPool.Enqueue(rewardData);
        }
        //------------------------------------------------------------------------------------
        public UIGlobalGoodsRewardIconElement GetGoodsRewardIcon()
        {
            if (m_uIGlobalGoodsRewardIconElements.Count <= 0)
            {
                if (m_uIGlobalGoodsRewardIconElement != null)
                {
                    GameObject clone = Instantiate(m_uIGlobalGoodsRewardIconElement.gameObject, UIManager.Instance.screenCanvas.transform);
                    if (clone != null)
                    {
                        UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement = clone.GetComponent<UIGlobalGoodsRewardIconElement>();
                        clone.SetActive(false);
                        return uIGlobalGoodsRewardIconElement;
                    }
                }
            }

            return m_uIGlobalGoodsRewardIconElements.Dequeue();
        }
        //------------------------------------------------------------------------------------
        public void PoolGoodsRewardIcon(UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement)
        {
            if (uIGlobalGoodsRewardIconElement == null)
                return;

            uIGlobalGoodsRewardIconElement.gameObject.SetActive(false);
            uIGlobalGoodsRewardIconElement.transform.SetParent(UIManager.Instance.screenCanvas.transform);
            uIGlobalGoodsRewardIconElement.AddClickListener(null);
            
            m_uIGlobalGoodsRewardIconElements.Enqueue(uIGlobalGoodsRewardIconElement);
        }
        //------------------------------------------------------------------------------------
    }
}