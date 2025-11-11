using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class ShopSummonPercentageDialog : IDialog
    {
        [Header("------------Element------------")]
        [SerializeField]
        private Transform m_elementRoot;

        [SerializeField]
        private UICollectionTitleElement m_uITitleElement;

        [SerializeField]
        private UICollectionDescElement m_uIDescElement;

        private Dictionary<int, UICollectionTitleElement> m_percentTitle_Dic = new Dictionary<int, UICollectionTitleElement>();
        private Dictionary<int, List<UICollectionDescElement>> m_percentDesc_Dic = new Dictionary<int, List<UICollectionDescElement>>();

        private Queue<UICollectionTitleElement> m_uICollectionTitleElements_Pool = new Queue<UICollectionTitleElement>();
        private Queue<UICollectionDescElement> m_uICollectionDescElements_Pool = new Queue<UICollectionDescElement>();

        private Queue<UICollectionTitleElement> m_uICollectionTitleElements_Use = new Queue<UICollectionTitleElement>();
        private Queue<UICollectionDescElement> m_uICollectionDescElements_Use = new Queue<UICollectionDescElement>();

        private int m_currentLevel;
        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            Message.AddListener<GameBerry.Event.SetShopSummonPercentageDialogMsg>(SetShopSummonPercentageDialog);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.SetShopSummonPercentageDialogMsg>(SetShopSummonPercentageDialog);
        }
        //------------------------------------------------------------------------------------
        private void SetShopSummonPercentageDialog(GameBerry.Event.SetShopSummonPercentageDialogMsg msg)
        {
            SetSummonPercentUI(msg.summonGroupData);
        }
        //------------------------------------------------------------------------------------
        private void SetSummonPercentUI(SummonGroupData v2Enum_SummonType)
        {
            SummonGroupData summonGroupData = v2Enum_SummonType;

            if (summonGroupData == null)
                return;

            //if (m_prevBtn != null)
            //    m_prevBtn.gameObject.SetActive(Managers.SummonManager.Instance.GetSummonGroupData(v2Enum_SummonType, level - 1) != null);

            //if (m_nextBtn != null)
            //    m_nextBtn.gameObject.SetActive(Managers.SummonManager.Instance.GetSummonGroupData(v2Enum_SummonType, level + 1) != null);

            //if (m_gachaLevel != null)
            //    m_gachaLevel.SetText("{0}", m_currentLevel);

            //while (m_uICollectionTitleElements_Use.Count > 0)
            //    PoolTitleElement(m_uICollectionTitleElements_Use.Dequeue());

            while (m_uICollectionDescElements_Use.Count > 0)
                PoolDescElement(m_uICollectionDescElements_Use.Dequeue());

            //m_percentTitle_Dic.Clear();
            m_percentDesc_Dic.Clear();

            int firstindex = -1;

            summonGroupData.SummonElementDatas.Sort((x, y) =>
            {
                V2Enum_Grade xGrade = Managers.GoodsManager.Instance.GetGoodsGrade(x.GoodsIndex);
                V2Enum_Grade yGrade = Managers.GoodsManager.Instance.GetGoodsGrade(y.GoodsIndex);

                if (xGrade > yGrade)
                    return -1;
                else if (xGrade < yGrade)
                    return 1;
                else
                {
                    if (x.Index < y.Index)
                        return -1;
                    else if (x.Index > y.Index)
                        return 1;
                }

                return 0;
            });

            int titleindex = summonGroupData.SummonType.Enum32ToInt();

            //if (m_percentTitle_Dic.ContainsKey(titleindex) == false)
            //{
            //    UICollectionTitleElement uIRuneCollectionTitleElement = GetTitleElement();
            //    uIRuneCollectionTitleElement.ChangeSlotId(titleindex);
            //    uIRuneCollectionTitleElement.SetCollectionTitle_Localize(
            //        null,
            //        string.Format("summon/{0}/name", titleindex));

            //    uIRuneCollectionTitleElement.transform.SetAsLastSibling();
            //    uIRuneCollectionTitleElement.gameObject.SetActive(true);

            //    m_percentTitle_Dic.Add(titleindex, uIRuneCollectionTitleElement);
            //    m_uICollectionTitleElements_Use.Enqueue(uIRuneCollectionTitleElement);

            //    if (m_percentDesc_Dic.ContainsKey(titleindex) == false)
            //        m_percentDesc_Dic.Add(titleindex, new List<UICollectionDescElement>());
            //}

            for (int i = 0; i < summonGroupData.SummonElementDatas.Count; ++i)
            {
                SummonElementData summonElementData = summonGroupData.SummonElementDatas[i];
                

                double weight = summonElementData.GoodsProb;
                double totalweight = summonGroupData.TotalWeight;

                UICollectionDescElement uIRuneCollectionDescElement = GetDescElement();
                uIRuneCollectionDescElement.SetCollectionElement(
                    Managers.GoodsManager.Instance.GetGoodsSprite(summonElementData.GoodsIndex),
                    Managers.GoodsManager.Instance.GetGoodsGrade(summonElementData.GoodsIndex),
                    string.Empty,
                    string.Format("{0:0.0000}%", (weight / totalweight) * 100.0),
                    summonElementData.GoodsIndex
                    );
                uIRuneCollectionDescElement.SetCollectionName_Localize(Managers.GoodsManager.Instance.GetGoodsLocalKey(summonElementData.GoodsIndex), summonElementData.GoodsValue);

                uIRuneCollectionDescElement.transform.SetAsLastSibling();
                uIRuneCollectionDescElement.gameObject.SetActive(true);

                //m_percentDesc_Dic[titleindex].Add(uIRuneCollectionDescElement);

                m_uICollectionDescElements_Use.Enqueue(uIRuneCollectionDescElement);

                //if (firstindex == -1)
                //    firstindex = titleindex;
            }

            //if (firstindex != -1)
            //    OnClick_Title(firstindex, true);
        }
        //-----------------------------------------------------------------------------------
        private UICollectionTitleElement GetTitleElement()
        {
            UICollectionTitleElement uICollectionTitleElement = null;

            if (m_uICollectionTitleElements_Pool.Count <= 0)
            {
                GameObject titleClone = Instantiate(m_uITitleElement.gameObject, m_elementRoot);
                uICollectionTitleElement = titleClone.GetComponent<UICollectionTitleElement>();
                uICollectionTitleElement.Init(-1, OnClick_Title);
            }
            else
                uICollectionTitleElement = m_uICollectionTitleElements_Pool.Dequeue();

            uICollectionTitleElement.gameObject.SetActive(true);

            return uICollectionTitleElement;
        }
        //------------------------------------------------------------------------------------
        private void PoolTitleElement(UICollectionTitleElement uICollectionTitleElement)
        {
            uICollectionTitleElement.gameObject.SetActive(false);
            m_uICollectionTitleElements_Pool.Enqueue(uICollectionTitleElement);
        }
        //------------------------------------------------------------------------------------
        private UICollectionDescElement GetDescElement()
        {
            UICollectionDescElement uICollectionDescElement = null;

            if (m_uICollectionDescElements_Pool.Count <= 0)
            {
                GameObject titleClone = Instantiate(m_uIDescElement.gameObject, m_elementRoot);
                uICollectionDescElement = titleClone.GetComponent<UICollectionDescElement>();
            }
            else
                uICollectionDescElement = m_uICollectionDescElements_Pool.Dequeue();

            uICollectionDescElement.gameObject.SetActive(true);

            return uICollectionDescElement;
        }
        //------------------------------------------------------------------------------------
        private void PoolDescElement(UICollectionDescElement uICollectionDescElement)
        {
            uICollectionDescElement.gameObject.SetActive(false);
            m_uICollectionDescElements_Pool.Enqueue(uICollectionDescElement);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Title(int slotid, bool extensionState)
        {
            foreach (KeyValuePair<int, List<UICollectionDescElement>> pair in m_percentDesc_Dic)
            {
                if (m_percentTitle_Dic.ContainsKey(pair.Key) == true)
                    m_percentTitle_Dic[pair.Key].SetExtensionStateUI(slotid != pair.Key);

                List<UICollectionDescElement> uIRuneCollectionDescElements = pair.Value;

                for (int i = 0; i < uIRuneCollectionDescElements.Count; ++i)
                {
                    uIRuneCollectionDescElements[i].gameObject.SetActive(slotid == pair.Key);
                }
            }
        }
        //------------------------------------------------------------------------------------
    }
}