using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace GameBerry.UI
{
    public class UIShopElementSpecial_Group : UIShopElement_Group
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
        private UIShopElementSpecial m_uIShopElement;

        [SerializeField]
        private Transform m_emptyElement;

        private Dictionary<ShopPackageSpecialData, UIShopElementSpecial> m_uIShopElementRelays = new Dictionary<ShopPackageSpecialData, UIShopElementSpecial>();

        private Queue<UIShopElementSpecial> m_uIShopPool = new Queue<UIShopElementSpecial>();

        private GameBerry.Event.ShowHudSpecialPackageRemainTimeMsg showHudSpecialPackageRemainTimeMsg = new GameBerry.Event.ShowHudSpecialPackageRemainTimeMsg();

        //------------------------------------------------------------------------------------
        public override void SetShopElement()
        {
            List<ShopPackageSpecialData> ShopPackageSpecialDatas = Managers.ShopManager.Instance.GetPackageSpecialDatas();

            for (int i = 0; i < ShopPackageSpecialDatas.Count; ++i)
            {
                ShopPackageSpecialData shopPackageSpecialData = ShopPackageSpecialDatas[i];
                if (shopPackageSpecialData == null)
                    continue;

                PlayerShopInfo playerShopInfo = Managers.ShopManager.Instance.GetPlayerShopInfo(shopPackageSpecialData.Index);
                if (playerShopInfo == null)
                    continue;

                if (Managers.ShopManager.Instance.IsSoldOut(playerShopInfo) == true)
                {
                    CreateGroupElement(shopPackageSpecialData);
                }
                else
                {
                    if (playerShopInfo.InitTimeStemp > Managers.TimeManager.Instance.Current_TimeStamp)
                        CreateGroupElement(shopPackageSpecialData);
                }
            }

            Message.AddListener<GameBerry.Event.RefreshShopSpecialMsg>(RefreshShopSpecial);

            //gameObject.SetActive(m_uIShopElementRelays.Count > 0);

            SortElement();
        }
        //------------------------------------------------------------------------------------
        private void OnDestroy()
        {
            Message.RemoveListener<GameBerry.Event.RefreshShopSpecialMsg>(RefreshShopSpecial);
        }
        //------------------------------------------------------------------------------------
        private void SortElement()
        {
            if (m_uIShopElementRelays.Count <= 0)
            {
                if (m_emptyElement != null)
                    m_emptyElement.gameObject.SetActive(true);
            }
            else
            {
                if (m_emptyElement != null)
                    m_emptyElement.gameObject.SetActive(false);
            }

            List<UIShopElementSpecial> lists = m_uIShopElementRelays.Values.ToList();

            if (lists == null)
                return;

            lists.Sort((x, y) =>
            {
                PlayerShopInfo xGroup = Managers.ShopManager.Instance.GetPlayerShopInfo(x.GetSpecialPackageIndex());
                PlayerShopInfo yGroup = Managers.ShopManager.Instance.GetPlayerShopInfo(y.GetSpecialPackageIndex());

                int xGroupOrder = (Managers.ShopManager.Instance.IsSoldOut(xGroup) == true ? 10000 : 0);
                int yGroupOrder = (Managers.ShopManager.Instance.IsSoldOut(yGroup) == true ? 10000 : 0);

                if (xGroupOrder > yGroupOrder)
                    return 1;
                else if (xGroupOrder < yGroupOrder)
                    return -1;

                if (xGroup.InitTimeStemp < yGroup.InitTimeStemp)
                    return -1;
                else if (xGroup.InitTimeStemp > yGroup.InitTimeStemp)
                    return 1;

                return 0;
            });

            for (int i = 0; i < lists.Count; ++i)
            {
                lists[i].transform.SetAsLastSibling();
            }

            showHudSpecialPackageRemainTimeMsg.index = -1;

            if (lists.Count > 0)
            {
                PlayerShopInfo xGroup = Managers.ShopManager.Instance.GetPlayerShopInfo(lists[0].GetSpecialPackageIndex());

                if (Managers.ShopManager.Instance.IsSoldOut(xGroup) == false)
                    showHudSpecialPackageRemainTimeMsg.index = xGroup.Id;
            }

            Message.Send(showHudSpecialPackageRemainTimeMsg);
        }
        //------------------------------------------------------------------------------------
        private UIShopElementSpecial CreateGroupElement(ShopPackageSpecialData shopPackageSpecialData)
        {
            UIShopElementSpecial uIShopElementRelay = null;

            if (m_uIShopPool.Count > 0)
            {
                uIShopElementRelay = m_uIShopPool.Dequeue();
                uIShopElementRelay.gameObject.SetActive(true);
                uIShopElementRelay.transform.SetAsLastSibling();
            }
            else
            {
                GameObject clone = Instantiate(m_uIShopElement.gameObject, m_elementRoot);

                uIShopElementRelay = clone.GetComponent<UIShopElementSpecial>();
                uIShopElementRelay.Init();
            }
            
            uIShopElementRelay.SetShopElement(shopPackageSpecialData);

            m_uIShopElementRelays.Add(shopPackageSpecialData, uIShopElementRelay);

            SetlayoutElementHeight();

            return uIShopElementRelay;
        }
        //------------------------------------------------------------------------------------
        private void SetlayoutElementHeight()
        {
            int elementCount = m_uIShopElementRelays.Count;
            if (elementCount <= 0)
                elementCount = 1;
            m_layoutElement.minHeight = (m_gridLayoutGroup.cellSize.y + m_gridLayoutGroup.spacing.y) * elementCount + m_spareHeight;
        }
        //------------------------------------------------------------------------------------
        private void RefreshShopSpecial(GameBerry.Event.RefreshShopSpecialMsg msg)
        {
            if (msg.shopPackageSpecialData == null)
                return;

            UIShopElementSpecial uIShopElementSpecial = null;

            PlayerShopInfo playerShopInfo = Managers.ShopManager.Instance.GetPlayerShopInfo(msg.shopPackageSpecialData);

            bool isShow = false;

            if (playerShopInfo == null)
                isShow = false;
            else
            {
                if (Managers.ShopManager.Instance.IsSoldOut(playerShopInfo) == true)
                    isShow = true;
                else
                    isShow = playerShopInfo.InitTimeStemp > Managers.TimeManager.Instance.Current_TimeStamp;
            }

            if (m_uIShopElementRelays.ContainsKey(msg.shopPackageSpecialData) == false)
            {
                if (isShow == false)
                    return;

                uIShopElementSpecial = CreateGroupElement(msg.shopPackageSpecialData);
            }
            else
            {
                uIShopElementSpecial = m_uIShopElementRelays[msg.shopPackageSpecialData];

                if (isShow == false)
                {
                    m_uIShopElementRelays.Remove(msg.shopPackageSpecialData);

                    m_uIShopPool.Enqueue(uIShopElementSpecial);
                    uIShopElementSpecial.ReleaseElement();
                    uIShopElementSpecial.gameObject.SetActive(false);
                    SetlayoutElementHeight();
                }
                else
                {
                    uIShopElementSpecial.SetShopElement(msg.shopPackageSpecialData);
                }
            }

            //gameObject.SetActive(m_uIShopElementRelays.Count > 0);

            SortElement();
        }
        //------------------------------------------------------------------------------------
    }
}