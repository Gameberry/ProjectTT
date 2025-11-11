using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry.UI
{
    public class UIBottomUISizeControllerBtn : MonoBehaviour
    {
        [SerializeField]
        private BottomStandardAnchors m_anchors;

        [SerializeField]
        private Button m_controllerBtn;

        [SerializeField]
        private Image m_ditectionUI;

        void Start()
        {
            if (m_controllerBtn != null)
                m_controllerBtn.onClick.AddListener(OnClick_Controller);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Controller()
        {
            if (m_anchors == null)
                return;

            if (m_anchors.CurrnetState == TargetPos.Expand)
            {
                Vector3 rotate = m_ditectionUI.transform.localEulerAngles;
                rotate.z = 0.0f;
                m_ditectionUI.transform.localEulerAngles = rotate;

                m_anchors.SetBottomStadardAnchors(TargetPos.Stadard);
            }
            else if (m_anchors.CurrnetState == TargetPos.Stadard)
            {
                Vector3 rotate = m_ditectionUI.transform.localEulerAngles;
                rotate.z = 180.0f;
                m_ditectionUI.transform.localEulerAngles = rotate;

                m_anchors.SetBottomStadardAnchors(TargetPos.Expand);
            }
        }
        //------------------------------------------------------------------------------------
    }
}

