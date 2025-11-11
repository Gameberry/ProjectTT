using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class ShopSummonDialog : IDialog
    {
        [Header("------------Element------------")]
        [SerializeField]
        private List<ShopSummonElementResource> m_shopSummonElementResources;

        [SerializeField]
        private ScrollRect m_elementScrollRect;

        [SerializeField]
        private RectTransform m_elementRoot;

        [SerializeField]
        private float m_snapOffSet = -10.0f;

        private Dictionary<V2Enum_SummonType, UIShopSummonElement> m_uIShopSummonElements_Dic = new Dictionary<V2Enum_SummonType, UIShopSummonElement>();

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            Dictionary<V2Enum_SummonType, SummonData> summonData_dic = Managers.SummonManager.Instance.GetSummonDatas();

            for (int i = 0; i < m_shopSummonElementResources.Count; ++i)
            {
                ShopSummonElementResource shopSummonElementResource = m_shopSummonElementResources[i];

                if (summonData_dic.ContainsKey(shopSummonElementResource.v2Enum_SummonType) == false)
                    continue;

                SummonData summonData = summonData_dic[shopSummonElementResource.v2Enum_SummonType];

                UIShopSummonElement uIShopSummonElement = shopSummonElementResource.uIShopSummonElement;
                uIShopSummonElement.Init(shopSummonElementResource.v2Enum_SummonType);
                uIShopSummonElement.SetSummonElement(shopSummonElementResource.v2Enum_SummonType);

                if (m_uIShopSummonElements_Dic.ContainsKey(shopSummonElementResource.v2Enum_SummonType) == false)
                {
                    m_uIShopSummonElements_Dic.Add(shopSummonElementResource.v2Enum_SummonType, uIShopSummonElement);
                }

                if (summonData.SummonCostParam11 > 0)
                {
                    V2Enum_Goods v2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(summonData.SummonCostParam11);

                    Managers.GoodsManager.Instance.AddGoodsRefreshEvent(v2Enum_Goods, summonData.SummonCostParam11, uIShopSummonElement.RefreshElement);
                }

                if (summonData.SummonCostParam13 > 0)
                {
                    V2Enum_Goods v2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(summonData.SummonCostParam13);

                    Managers.GoodsManager.Instance.AddGoodsRefreshEvent(v2Enum_Goods, summonData.SummonCostParam13, uIShopSummonElement.RefreshElement);
                }
            }

            Managers.GoodsManager.Instance.AddGoodsRefreshEvent(V2Enum_Goods.Point, V2Enum_Point.Dia.Enum32ToInt(), RefreshDia);

            Message.AddListener<GameBerry.Event.RefreshSummonInfoListMsg>(RefreshSummonInfoList);
            Message.AddListener<GameBerry.Event.SetShopSummonDialogStateMsg>(SetShopSummonDialogState);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            if (Managers.GoodsManager.isAlive == true)
                Managers.GoodsManager.Instance.RemoveGoodsRefreshEvent(V2Enum_Goods.Point, V2Enum_Point.LobbyGold.Enum32ToInt(), RefreshDia);

            Message.RemoveListener<GameBerry.Event.RefreshSummonInfoListMsg>(RefreshSummonInfoList);
            Message.RemoveListener<GameBerry.Event.SetShopSummonDialogStateMsg>(SetShopSummonDialogState);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            
        }
        //------------------------------------------------------------------------------------
        private void SetShopSummonDialogState(GameBerry.Event.SetShopSummonDialogStateMsg msg)
        {
            V2Enum_SummonType v2Enum_SummonType = V2Enum_SummonType.Max;

            switch (msg.ContentDetailList)
            {
                case ContentDetailList.ShopRandomStore:
                    {
                        v2Enum_SummonType = V2Enum_SummonType.SummonGear;
                        break;
                    }
                case ContentDetailList.ShopSummon_Normal:
                    {
                        v2Enum_SummonType = V2Enum_SummonType.SummonNormal;
                        break;
                    }
                case ContentDetailList.ShopSummon_Relic:
                    {
                        v2Enum_SummonType = V2Enum_SummonType.SummonRelic;
                        break;
                    }
                case ContentDetailList.ShopSummon_Rune:
                    {
                        v2Enum_SummonType = V2Enum_SummonType.SummonRune;
                        break;
                    }
                case ContentDetailList.ShopSummon_Gear:
                    {
                        v2Enum_SummonType = V2Enum_SummonType.SummonGear;
                        break;
                    }
            }

            if (m_uIShopSummonElements_Dic.ContainsKey(v2Enum_SummonType) == true)
            {
                UIShopSummonElement uIShopSummonElement = m_uIShopSummonElements_Dic[v2Enum_SummonType];

                RectTransform rectTransform = null;
                if (uIShopSummonElement.TryGetComponent(out rectTransform))
                {
                    Vector2 offset = Vector2.zero;
                    offset.y = m_snapOffSet;

                    Util.ScrollViewSnapToItem(m_elementScrollRect, m_elementRoot, rectTransform, offset);
                }

            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshDia(double amount)
        {
            foreach (KeyValuePair<V2Enum_SummonType, UIShopSummonElement> pair in m_uIShopSummonElements_Dic)
            {
                pair.Value.SetSummonBtn();
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshSummonInfoList(GameBerry.Event.RefreshSummonInfoListMsg msg)
        {
            for (int i = 0; i < msg.datas.Count; ++i)
            {
                if (m_uIShopSummonElements_Dic.ContainsKey(msg.datas[i]) == true)
                    m_uIShopSummonElements_Dic[msg.datas[i]].SetSummonElement(msg.datas[i]);
            }
        }
        //------------------------------------------------------------------------------------
    }
}