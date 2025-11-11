using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UINumberCallBackBtn : MonoBehaviour
    {
        [SerializeField]
        private Button m_btn;

        [SerializeField]
        private Color m_btnBG_Enable;

        [SerializeField]
        private Color m_btnBG_Disable;

        [SerializeField]
        private TMP_Text m_btnNumber;

        [SerializeField]
        private Color m_btnNumber_Enable;

        [SerializeField]
        private Color m_btnNumber_Disable;

        [SerializeField]
        private Transform m_enableEffect;

        private int m_myNumber = 0;
        private System.Action<int> m_btnClickCallback = null;
        //------------------------------------------------------------------------------------
        public void Init(System.Action<int> callback)
        {
            m_btnClickCallback = callback;

            if (m_btn != null)
                m_btn.onClick.AddListener(OnClick);
        }
        //------------------------------------------------------------------------------------
        public void SetNumber(int number)
        {
            m_myNumber = number;
        }
        //------------------------------------------------------------------------------------
        public void SetViewNumber(int number)
        {
            if (m_btnNumber != null)
                m_btnNumber.text = number.ToString();
        }
        //------------------------------------------------------------------------------------
        public void SetEnable(bool enable)
        {
            if (m_enableEffect != null)
                m_enableEffect.gameObject.SetActive(enable);

            if (m_btn != null)
                m_btn.image.color = enable == true ? m_btnBG_Enable : m_btnBG_Disable;

            if (m_btnNumber != null)
                m_btnNumber.color = enable == true ? m_btnNumber_Enable : m_btnNumber_Disable;
        }
        //------------------------------------------------------------------------------------
        private void OnClick()
        {
            if (m_btnClickCallback != null)
                m_btnClickCallback(m_myNumber);
        }
        //------------------------------------------------------------------------------------
    }
}