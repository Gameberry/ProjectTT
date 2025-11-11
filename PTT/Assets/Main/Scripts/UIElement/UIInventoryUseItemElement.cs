using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.UI
{
    public class UIInventoryUseItemElement : IDialog
    {
        [SerializeField]
        private List<Button> m_exitBtn;

        [SerializeField]
        private UIGlobalGoodsRewardIconElement inventory_Icon;

        [SerializeField]
        private Button useItemBtn;

        [SerializeField]
        private Scrollbar moveStageScroll;

        [SerializeField]
        private Image moveStageScrollGauge;

        [SerializeField]
        private UIPushBtn minusBtn;

        [SerializeField]
        private UIPushBtn plusBtn;

        [SerializeField]
        private TMP_Text goodsName;

        [SerializeField]
        private TMP_Text goodsDesc;

        [SerializeField]
        private TMP_Text useAmountCount;


        [SerializeField]
        private Button m_showPercentage;

        private V2Enum_Goods myV2Enum_Goods = V2Enum_Goods.Max;
        private int myIndex = -1;

        private int maxAmount = 0;
        private int useAmount = 1;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (m_exitBtn != null)
            {
                for (int i = 0; i < m_exitBtn.Count; ++i)
                {
                    if (m_exitBtn[i] != null)
                        m_exitBtn[i].onClick.AddListener(ElementExit);
                }
            }

            if (useItemBtn != null)
                useItemBtn.onClick.AddListener(OnClick_UseItem);

            if (minusBtn != null)
            {
                minusBtn.SetOnClick(OnClick_MinusBtn);
                minusBtn.SetOnPush(OnClick_MinusBtn);
            }

            if (plusBtn != null)
            {
                plusBtn.SetOnClick(OnClick_PlusBtn);
                plusBtn.SetOnPush(OnClick_PlusBtn);
            }

            if (moveStageScroll != null)
                moveStageScroll.onValueChanged.AddListener(OnValueChange_ScrollBar);

            if (m_showPercentage != null)
                m_showPercentage.onClick.AddListener(OnClick_ShowPercentage);

            if (Managers.LocalStringManager.isAlive == true)
                Managers.LocalStringManager.Instance.RefreshLocalString += RefreshLocalize;
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            if (Managers.LocalStringManager.isAlive == true)
                Managers.LocalStringManager.Instance.RefreshLocalString -= RefreshLocalize;
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            useAmount = maxAmount;
            SetScrollValue();
            RefreshSelectStage();
        }
        //------------------------------------------------------------------------------------
        public void SetGoods(V2Enum_Goods v2Enum_Goods, int index)
        {
            if (goodsName != null)
            {
                string localkey = Managers.GoodsManager.Instance.GetGoodsLocalKey(v2Enum_Goods.Enum32ToInt(), index);
                Managers.LocalStringManager.Instance.SetLocalizeText(goodsName, localkey);
            }

            if (goodsDesc != null)
            {
                string descKey = string.Format("{0}/{1}{2}/desc", v2Enum_Goods.ToString().ToCamelCase(), v2Enum_Goods.Enum32ToInt(), index);

                Managers.LocalStringManager.Instance.SetLocalizeText(goodsDesc, descKey);
            }

            myV2Enum_Goods = v2Enum_Goods;
            myIndex = index;

            maxAmount = (int)Managers.GoodsManager.Instance.GetGoodsAmount(myV2Enum_Goods.Enum32ToInt(), myIndex);

            if (m_showPercentage != null)
                m_showPercentage.gameObject.SetActive(false);

            if (v2Enum_Goods == V2Enum_Goods.Box)
            {
                V2Enum_BoxType v2Enum_BoxType = Managers.BoxManager.Instance.GetBoxType(myIndex);
                if (v2Enum_BoxType == V2Enum_BoxType.RandomTypeBox)
                {
                    if (m_showPercentage != null)
                        m_showPercentage.gameObject.SetActive(true);
                }
            }

            if (inventory_Icon != null)
            {
                inventory_Icon.SetRewardElement(v2Enum_Goods, index,
                Managers.GoodsManager.Instance.GetGoodsSprite(v2Enum_Goods.Enum32ToInt(), index),
                Managers.GoodsManager.Instance.GetGoodsGrade(v2Enum_Goods.Enum32ToInt(), index),
                0);
                inventory_Icon.HideLightCircle();
                inventory_Icon.HideAmount();
            }
            
        }
        //------------------------------------------------------------------------------------
        private void RefreshLocalize()
        {
            RefreshSelectStage();
        }
        //------------------------------------------------------------------------------------
        private void SetScrollValue()
        {
            if (moveStageScroll != null)
            {
                int max = maxAmount;
                float ratio = (float)useAmount / (float)max;

                moveStageScroll.SetValueWithoutNotify(ratio);

                if (moveStageScrollGauge != null)
                    moveStageScrollGauge.fillAmount = ratio;
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshSelectStage()
        {
            //if (chapterName != null)
            //    chapterName.text = Managers.DungeonDataManager.Instance.GetStageLocalString(m_currentFarmStageData);

            if (useAmountCount != null)
                useAmountCount.SetText("({0}/{1})", useAmount, maxAmount);
        }
        //------------------------------------------------------------------------------------
        private void OnValueChange_ScrollBar(float value)
        {
            if (value <= 0)
            {
                useAmount = 1;
            }
            else if (value >= 1)
            {
                useAmount = maxAmount;
            }
            else
            {
                float max = (float)maxAmount;

                useAmount = (int)(max * value);

                if (useAmount <= 0)
                {
                    useAmount = 1;
                }
                else if (useAmount >= maxAmount)
                {
                    useAmount = maxAmount;
                }
            }

            if (moveStageScrollGauge != null)
                moveStageScrollGauge.fillAmount = value;

            RefreshSelectStage();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_MinusBtn()
        {
            if (useAmount > 1)
            {
                useAmount--;

                SetScrollValue();
                RefreshSelectStage();
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_PlusBtn()
        {
            if (maxAmount > useAmount)
            {
                useAmount++;

                SetScrollValue();
                RefreshSelectStage();
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_UseItem()
        {
            Managers.GoodsManager.Instance.UseGoodsAmount(myV2Enum_Goods.Enum32ToInt(), myIndex, useAmount);
            ElementExit();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ShowPercentage()
        {
            Managers.BoxManager.Instance.ShowRandomBoxPercentage(myIndex);
        }
        //------------------------------------------------------------------------------------
    }
}