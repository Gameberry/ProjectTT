using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;

namespace GameBerry.UI
{
    public class InGameSelectGoodsPopupDialog : IDialog
    {
        [SerializeField]
        private List<Button> m_exitBtn;

        [SerializeField]
        private Button m_onSelectBtn;

        [SerializeField]
        private Transform m_elementRoot;

        [SerializeField]
        private UISelectGoodsElement m_element;

        [SerializeField]
        private List<UISelectGoodsElement> m_uISelectGoodsElement = new List<UISelectGoodsElement>();

        System.Action<int> m_selectCallBack = null;

        private int m_selectIdx = -1;

        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            if (m_exitBtn != null)
            {
                for (int i = 0; i < m_exitBtn.Count; ++i)
                {
                    if (m_exitBtn[i] != null)
                        m_exitBtn[i].onClick.AddListener(OnClick_ExitBtn);
                }
            }

            if (m_onSelectBtn != null)
                m_onSelectBtn.onClick.AddListener(OnClick_SelectBtn);

            Message.AddListener<GameBerry.Event.SetSelectGoodsPopupMsg>(SetSelectGoodsPopup);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            Message.RemoveListener<GameBerry.Event.SetSelectGoodsPopupMsg>(SetSelectGoodsPopup);
        }
        //------------------------------------------------------------------------------------
        protected override void OnEnter()
        {
            m_selectIdx = -1;
        }
        //------------------------------------------------------------------------------------
        private UISelectGoodsElement CreateElement()
        {
            GameObject clone = Instantiate(m_element.gameObject, m_elementRoot);

            UISelectGoodsElement uIShopElementRelay = clone.GetComponent<UISelectGoodsElement>();
            uIShopElementRelay.Init(OnSelect);
            m_uISelectGoodsElement.Add(uIShopElementRelay);

            return uIShopElementRelay;
        }
        //------------------------------------------------------------------------------------
        private void SetSelectGoodsPopup(GameBerry.Event.SetSelectGoodsPopupMsg msg)
        {
            if (msg.SelectIndexList == null)
                return;

            for (int i = 0; i < msg.SelectIndexList.Count; ++i)
            {
                UISelectGoodsElement uISelectGoodsElement = null;

                if (m_uISelectGoodsElement.Count <= i)
                    uISelectGoodsElement = CreateElement();
                else
                    uISelectGoodsElement = m_uISelectGoodsElement[i];

                uISelectGoodsElement.SetGoodsElement(msg.v2Enum_Goods, msg.SelectIndexList[i]);
                uISelectGoodsElement.SetSelect(false);
                uISelectGoodsElement.gameObject.SetActive(true);
            }

            for (int i = msg.SelectIndexList.Count; i < m_uISelectGoodsElement.Count; ++i)
            {
                UISelectGoodsElement uISelectGoodsElement = null;

                uISelectGoodsElement = m_uISelectGoodsElement[i];

                uISelectGoodsElement.SetGoodsElement(msg.v2Enum_Goods, -1);

                uISelectGoodsElement.gameObject.SetActive(false);
            }

            m_selectCallBack = msg.SelectedCallBack;
        }
        //------------------------------------------------------------------------------------
        private void OnSelect(int idx)
        {
            if (idx == -1)
                return;

            m_selectIdx = idx;

            for (int i = 0; i < m_uISelectGoodsElement.Count; ++i)
            {
                UISelectGoodsElement uISelectGoodsElement = null;

                uISelectGoodsElement = m_uISelectGoodsElement[i];

                uISelectGoodsElement.SetSelect(uISelectGoodsElement.GetMyIndex() == m_selectIdx);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SelectBtn()
        {
            if (m_selectIdx == -1)
                return;

            m_selectCallBack?.Invoke(m_selectIdx);

            OnClick_ExitBtn();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_ExitBtn()
        {
            UIManager.DialogExit<InGameSelectGoodsPopupDialog>();
        }
        //------------------------------------------------------------------------------------

    }
}