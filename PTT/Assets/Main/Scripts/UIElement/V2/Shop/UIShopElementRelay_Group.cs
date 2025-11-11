using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace GameBerry.UI
{
    public class UIShopElementRelay_Group : UIShopElement_Group
    {
        [SerializeField]
        private LayoutElement m_layoutElement;

        [SerializeField]
        private GridLayoutGroup m_gridLayoutGroup;

        [SerializeField]
        private float m_spareHeight = 150.0f;

        [SerializeField]
        private Transform m_elementRoot;

        [SerializeField]
        private UIShopElementRelay m_uIShopElement;

        private Dictionary<ShopPackageRelayGroupData, UIShopElementRelay> m_uIShopElementRelays = new Dictionary<ShopPackageRelayGroupData, UIShopElementRelay>();

        //------------------------------------------------------------------------------------
        public override void SetShopElement()
        {
            foreach (var pair in Managers.ShopManager.Instance.GetPackageRelayDatas())
            {
                if (pair.Value.IsActive == true)
                {
                    UIShopElementRelay uIShopElementRelay = CreateGroupElement(pair.Value);

                    int addsibling = pair.Value.IsSoldOut == true ? 10000 : 0;

                    uIShopElementRelay.transform.SetSiblingIndex(pair.Value.RelayGroupIndex + addsibling);

                    gameObject.SetActive(m_uIShopElementRelays.Count > 0);
                }
            }

            Message.AddListener<GameBerry.Event.RefreshRelayGroupMsg>(RefreshRelayGroup);

            gameObject.SetActive(m_uIShopElementRelays.Count > 0);

            SortElement();
        }
        //------------------------------------------------------------------------------------
        private void OnDestroy()
        {
            Message.RemoveListener<GameBerry.Event.RefreshRelayGroupMsg>(RefreshRelayGroup);
        }
        //------------------------------------------------------------------------------------
        private void SortElement()
        {
            List<UIShopElementRelay> lists = m_uIShopElementRelays.Values.ToList();

            if (lists == null)
                return;

            lists.Sort((x, y) =>
            {
                ShopPackageRelayGroupData xGroup = Managers.ShopManager.Instance.GetPackageRelayGroupData(x.GetGroupIndex());
                ShopPackageRelayGroupData yGroup = Managers.ShopManager.Instance.GetPackageRelayGroupData(y.GetGroupIndex());

                int xGroupOrder = xGroup.RelayGroupIndex + (xGroup.IsSoldOut == true ? 10000 : 0);
                int yGroupOrder = yGroup.RelayGroupIndex + (yGroup.IsSoldOut == true ? 10000 : 0);

                if (xGroupOrder > yGroupOrder)
                    return 1;
                else if (xGroupOrder < yGroupOrder)
                    return -1;

                return 0;
            });

            for (int i = 0; i < lists.Count; ++i)
            {
                lists[i].transform.SetAsLastSibling();
            }
        }
        //------------------------------------------------------------------------------------
        private UIShopElementRelay CreateGroupElement(ShopPackageRelayGroupData shopPackageRelayGroupData)
        {
            GameObject clone = Instantiate(m_uIShopElement.gameObject, m_elementRoot);

            UIShopElementRelay uIShopElementRelay = clone.GetComponent<UIShopElementRelay>();
            uIShopElementRelay.Init();
            uIShopElementRelay.SetShopElement(shopPackageRelayGroupData.FocusShopPackageData);
            uIShopElementRelay.IsShowGroupBtn(true);

            m_uIShopElementRelays.Add(shopPackageRelayGroupData, uIShopElementRelay);

            m_layoutElement.minHeight = (m_gridLayoutGroup.cellSize.y + m_gridLayoutGroup.spacing.y) * m_uIShopElementRelays.Count + m_spareHeight;

            return uIShopElementRelay;
        }
        //------------------------------------------------------------------------------------
        private void RefreshRelayGroup(GameBerry.Event.RefreshRelayGroupMsg msg)
        {
            UIShopElementRelay uIShopElementRelay = null;

            if (m_uIShopElementRelays.ContainsKey(msg.RefreshData) == false)
            {
                uIShopElementRelay = CreateGroupElement(msg.RefreshData);
            }
            else
            {
                uIShopElementRelay = m_uIShopElementRelays[msg.RefreshData];
                uIShopElementRelay.SetShopElement(msg.RefreshData.FocusShopPackageData);
                uIShopElementRelay.IsShowGroupBtn(true);
            }

            int addsibling = msg.RefreshData.IsSoldOut == true ? 10000 : 0;

            uIShopElementRelay.transform.SetSiblingIndex(msg.RefreshData.RelayGroupIndex + addsibling);

            gameObject.SetActive(m_uIShopElementRelays.Count > 0);

            SortElement();
        }
        //------------------------------------------------------------------------------------

    }
}