using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIItemIconAndAmount : MonoBehaviour
    {
        [SerializeField]
        private V2Enum_Point m_v2Enum_Point = V2Enum_Point.InGameGold;

        [SerializeField]
        private Image m_pointIcon;

        [SerializeField]
        private TMP_Text m_pointAmount;

        [SerializeField]
        private bool IsAutoSetting = true;

        private void Start()
        {
            if (IsAutoSetting == false)
                return;

            Button btn = gameObject.AddComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(OnClick_ItemIcon);

            SetSetting(m_v2Enum_Point);
        }
        //------------------------------------------------------------------------------------
        public void SetSetting(int id)
        {
            m_v2Enum_Point = id.IntToEnum32<V2Enum_Point>();

            SetSetting(m_v2Enum_Point);
        }
        //------------------------------------------------------------------------------------
        public void SetSetting(V2Enum_Point type)
        {
            if (m_pointIcon != null)
                m_pointIcon.sprite = Managers.GoodsManager.Instance.GetGoodsSprite(V2Enum_Goods.Point.Enum32ToInt(), m_v2Enum_Point.Enum32ToInt());


            if (m_pointAmount != null)
            {
                Managers.GoodsManager.Instance.AddGoodsRefreshEvent(V2Enum_Goods.Point, m_v2Enum_Point.Enum32ToInt(), RefreshAmount);
                RefreshAmount(Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), m_v2Enum_Point.Enum32ToInt()));
            }
        }
        //------------------------------------------------------------------------------------
        public void SetSetting_NotConnectRefreshEvent(V2Enum_Point type)
        {
            if (m_pointIcon != null)
                m_pointIcon.sprite = Managers.GoodsManager.Instance.GetGoodsSprite(V2Enum_Goods.Point.Enum32ToInt(), type.Enum32ToInt());


            if (m_pointAmount != null)
            {
                RefreshAmount(Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), type.Enum32ToInt()));
            }

            m_v2Enum_Point = type;
        }
        //------------------------------------------------------------------------------------
        public void AddCallBack()
        {
            if (IsAutoSetting == true)
                return;

            if (gameObject.GetComponent<Button>() != null)
                return;

            Button btn = gameObject.AddComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(OnClick_ItemIcon);
        }
        //------------------------------------------------------------------------------------
        private void RefreshAmount(double amount)
        {
            if (m_pointAmount != null)
            {
                if (m_v2Enum_Point == V2Enum_Point.LobbyGold)
                    m_pointAmount.text = string.Format("{0:#,0.##}", amount);
                else
                    m_pointAmount.text = Util.GetAlphabetNumber(amount);
                //if (itemSortTypeName == ItemSortTypeName.Gold
                //    || itemSortTypeName == ItemSortTypeName.StoneEquip)
                //    m_itemAmount.text = Util.GetAlphabetNumber(amount);
                //else
                //    m_itemAmount.text = string.Format("{0:#,0}", amount); 
            }
        }
        //------------------------------------------------------------------------------------
        private void OnDestroy()
        {
            if (m_pointAmount != null)
            {
                if (Managers.GoodsManager.isAlive == true)
                    Managers.GoodsManager.Instance.RemoveGoodsRefreshEvent(V2Enum_Goods.Point.Enum32ToInt(), m_v2Enum_Point.Enum32ToInt(), RefreshAmount);
            }
            
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ItemIcon()
        {
            Contents.GlobalContent.ShowGoodsDescPopup(V2Enum_Goods.Point, m_v2Enum_Point.Enum32ToInt());
        }
        //------------------------------------------------------------------------------------
    }
}