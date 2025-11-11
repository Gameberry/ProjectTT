using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIInventoryElement : MonoBehaviour
    {
        [SerializeField]
        private UIGlobalGoodsRewardIconElement inventory_Icon;

        [SerializeField]
        private Button inventoryElementBtn;

        private RewardData myRewardData = new RewardData();

        private V2Enum_Goods myV2Enum_Goods = V2Enum_Goods.Max;
        private int myIndex = -1;

        private System.Action<V2Enum_Goods, int> onClick_Item;
        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (inventoryElementBtn != null)
                inventoryElementBtn.onClick.AddListener(OnClick_Btn);
        }
        //------------------------------------------------------------------------------------
        public void Init(System.Action<V2Enum_Goods, int> action)
        {
            onClick_Item = action;
        }
        //------------------------------------------------------------------------------------
        public void SetInvenToryIcon(V2Enum_Goods v2Enum_Goods, int index)
        {
            if (v2Enum_Goods == V2Enum_Goods.Max)
            {
                RemoveAmountEvent();
                myV2Enum_Goods = v2Enum_Goods;
                myIndex = index;

                gameObject.SetActive(false);
                return;
            }

            if (myV2Enum_Goods == v2Enum_Goods
                && myIndex == index)
            {

            }
            else
            {
                RemoveAmountEvent();

                myV2Enum_Goods = v2Enum_Goods;
                myIndex = index;

                myRewardData.V2Enum_Goods = myV2Enum_Goods;
                myRewardData.Index = myIndex;

                AddAmountEvent();
            }

            double amount = Managers.GoodsManager.Instance.GetGoodsAmount(v2Enum_Goods.Enum32ToInt(), index);

            RefreshAmount(amount);
        }
        //------------------------------------------------------------------------------------
        private void AddAmountEvent()
        {
            if (myV2Enum_Goods == V2Enum_Goods.Max)
                return;

            Managers.GoodsManager.Instance.AddGoodsRefreshEvent(myV2Enum_Goods, myIndex, RefreshAmount);
        }
        //------------------------------------------------------------------------------------
        private void RemoveAmountEvent()
        {
            if (myV2Enum_Goods == V2Enum_Goods.Max)
                return;

            Managers.GoodsManager.Instance.RemoveGoodsRefreshEvent(myV2Enum_Goods, myIndex, RefreshAmount);
        }
        //------------------------------------------------------------------------------------
        private void RefreshAmount(double amount)
        {
            if (myV2Enum_Goods == V2Enum_Goods.Point)
            {
                gameObject.SetActive(true);
            }
            else
            {
                if (amount <= 0)
                { 
                    gameObject.SetActive(false);
                    return;
                }
                else
                    gameObject.SetActive(true);
            }

            myRewardData.Amount = amount;

            if (inventory_Icon != null)
            { 
                inventory_Icon.SetRewardElement(myRewardData);
                inventory_Icon.ForceShowAmount();
                inventory_Icon.HideLightCircle();
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Btn()
        {
            if (myV2Enum_Goods == V2Enum_Goods.Point)
            {
                if (myIndex != -1)
                {
                    Contents.GlobalContent.ShowGoodsDescPopup(myV2Enum_Goods, myIndex, myRewardData.Amount);
                }
            }
            else
            {
                onClick_Item?.Invoke(myV2Enum_Goods, myIndex);
            }
        }
        //------------------------------------------------------------------------------------
    }
}