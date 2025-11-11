using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UICallBackBtnElement : MonoBehaviour
    {
        [SerializeField]
        private Button m_btnElement;

        [SerializeField]
        private Image m_enableImage;

        [SerializeField]
        private ParticleSystem m_enableParticle;

        public List<TMP_Text> m_texts;

        public int m_myID = -1;

        private System.Action<int> m_callback = null;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (m_btnElement != null)
                m_btnElement.onClick.AddListener(OnClick_Element);
        }
        //------------------------------------------------------------------------------------
        public void SetCallBack(System.Action<int> action)
        {
            m_callback = action;
        }
        //------------------------------------------------------------------------------------
        public void OnClick_Element()
        {
            if (m_callback != null)
                m_callback(m_myID);
        }
        //------------------------------------------------------------------------------------
        public void SetEnable(bool enable)
        {
            if (m_enableImage != null)
                m_enableImage.gameObject.SetActive(enable);

            if (m_enableParticle != null)
            { 
                m_enableParticle.gameObject.SetActive(enable);
                if (enable == true)
                    m_enableParticle.Play();
            }
        }
        //------------------------------------------------------------------------------------
        public void SetText(string text)
        {
            for (int i = 0; i < m_texts.Count; ++i)
            {
                m_texts[i].SetText(text);
            }
        }
        //------------------------------------------------------------------------------------
    }
}