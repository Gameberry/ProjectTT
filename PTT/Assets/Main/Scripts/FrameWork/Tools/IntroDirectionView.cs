using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry
{
    public class IntroDirectionView : MonoBehaviour
    {
        [SerializeField]
        private Button m_skipBtn;

        System.Action m_skipCallBack = null;

        //------------------------------------------------------------------------------------
        public void SetSkipCallback(System.Action skipCallBack)
        {
            if (m_skipBtn != null)
                m_skipBtn.onClick.AddListener(OnClick_SkipBtn);

            m_skipCallBack = skipCallBack;
        }
        //------------------------------------------------------------------------------------
        public void OnClick_SkipBtn()
        {
            if (m_skipCallBack != null)
                m_skipCallBack();
        }
        //------------------------------------------------------------------------------------
    }
}