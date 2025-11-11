using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UICollectionTitleElement : MonoBehaviour
    {
        [SerializeField]
        private Image m_titleIcon;

        [SerializeField]
        private TMP_Text m_titleDesc;

        [SerializeField]
        private Transform m_extensionArrow;

        [SerializeField]
        private bool m_isExtensionState = true;

        [SerializeField]
        private Button m_extensionButton;

        private int m_mySlotid = -1;

        private System.Action<int, bool> m_action;

        //------------------------------------------------------------------------------------
        public void Init(int slotid, System.Action<int, bool> action)
        {
            if (m_extensionButton != null)
                m_extensionButton.onClick.AddListener(OnClick_Extension);

            m_mySlotid = slotid;
            m_action = action;
        }
        //------------------------------------------------------------------------------------
        public void ChangeSlotId(int slotid)
        {
            m_mySlotid = slotid;
        }
        //------------------------------------------------------------------------------------
        public void SetCollectionTitle(Sprite sprite, string desc)
        {
            if (m_titleIcon != null)
                m_titleIcon.sprite = sprite;

            if (m_titleDesc != null)
                m_titleDesc.SetText(desc);
        }
        //------------------------------------------------------------------------------------
        public void SetCollectionTitle_Localize(Sprite sprite, string desc)
        {
            if (m_titleIcon != null)
                m_titleIcon.sprite = sprite;

            if (m_titleDesc != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(m_titleDesc, desc);
        }
        //------------------------------------------------------------------------------------
        private void SetExtensionState(bool extension)
        {
            SetExtensionStateUI(extension);
            m_isExtensionState = extension;

            if (m_action != null)
                m_action(m_mySlotid, m_isExtensionState);
        }
        //------------------------------------------------------------------------------------
        public void SetExtensionStateUI(bool extension)
        {
            if (m_extensionArrow != null)
            {
                Vector3 rotate = Vector3.zero;
                rotate.z = extension == true ? 0.0f : 180.0f;
                m_extensionArrow.localEulerAngles = rotate;
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Extension()
        {
            SetExtensionState(!m_isExtensionState);
        }
        //------------------------------------------------------------------------------------
    }
}