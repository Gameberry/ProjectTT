using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry.UI
{
    public class UIContentDetailElement : MonoBehaviour
    {
        [SerializeField]
        private ContentDetailList m_myContentDetailList = ContentDetailList.None;
        public ContentDetailList MyContentDetailList { get { return m_myContentDetailList; } }

        [SerializeField]
        private Button m_myButton;

        private System.Action<ContentDetailList> m_linkCallBack = null;

        //------------------------------------------------------------------------------------
        public void Init(System.Action<ContentDetailList> linkCallBack)
        {
            m_linkCallBack = linkCallBack;

            if (m_myButton != null)
                m_myButton.onClick.AddListener(OnClick);
        }
        //------------------------------------------------------------------------------------
        private void OnClick()
        {
            if (m_linkCallBack != null)
                m_linkCallBack(m_myContentDetailList);
        }
        //------------------------------------------------------------------------------------
    }
}