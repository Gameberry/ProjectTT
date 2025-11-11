using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;

namespace GameBerry.UI
{
    public class UISelectGoodsElement : MonoBehaviour
    {
        [SerializeField]
        private UIGlobalGoodsRewardIconElement uIGlobalGoodsRewardIconElement;

        [SerializeField]
        private Image m_isSelect;

        [SerializeField]
        private Transform m_isSelectParitcle;

        [SerializeField]
        private Button m_selectBtn;

        System.Action<int> m_selectCallBack = null;

        private int m_myIndex = -1;

        //------------------------------------------------------------------------------------
        public void Init(System.Action<int> callback)
        {
            if (m_selectBtn != null)
                m_selectBtn.onClick.AddListener(OnClick_SelectBtn);

            m_selectCallBack = callback;
        }
        //------------------------------------------------------------------------------------
        public void SetGoodsElement(V2Enum_Goods v2Enum_Goods, int index)
        {
            uIGlobalGoodsRewardIconElement.SetRewardElement(
                    v2Enum_Goods,
                    index,
                    Managers.GoodsManager.Instance.GetGoodsSprite(v2Enum_Goods.Enum32ToInt(), index),
                    Managers.GoodsManager.Instance.GetGoodsGrade(v2Enum_Goods.Enum32ToInt(), index),
                    0);

            m_myIndex = index;
        }
        //------------------------------------------------------------------------------------
        public void SetSelect(bool select)
        {
            if (m_isSelect != null)
                m_isSelect.gameObject.SetActive(select);

            if (m_isSelectParitcle != null)
            {
                m_isSelectParitcle.gameObject.SetActive(select);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SelectBtn()
        {
            if (m_myIndex == -1)
                return;

            m_selectCallBack?.Invoke(m_myIndex);
        }
        //------------------------------------------------------------------------------------
        public int GetMyIndex()
        {
            return m_myIndex;
        }
        //------------------------------------------------------------------------------------
    }
}