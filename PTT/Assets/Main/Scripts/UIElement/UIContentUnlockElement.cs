using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameBerry.Managers;
using TMPro;

namespace GameBerry.UI
{
    public class UIContentUnlockElement : MonoBehaviour
    {
        [SerializeField]
        private Image m_contentImageLock;

        [SerializeField]
        private Color m_contentImageColor = Color.white;

        [SerializeField]
        private List<Graphic> m_contentImages;


        [SerializeField]
        private List<Transform> _hideList = new List<Transform>();

        [SerializeField]
        private TMP_Text m_contentTextLock;

        [SerializeField]
        private Color m_contentTextColor = Color.white;

        [SerializeField]
        private Button m_contentBlockBtn;

        public V2Enum_ContentType m_v2Enum_ContentType;

        public bool IsAutoSetting = true;


        private ContentUnlockData m_myContentData;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (IsAutoSetting == false)
                return;

            Init();
        }
        //------------------------------------------------------------------------------------
        public void Init()
        {
            m_myContentData = Managers.ContentOpenConditionManager.Instance.GetContentUnlockData(m_v2Enum_ContentType);
            if (m_myContentData == null)
            {
                if (m_contentBlockBtn != null)
                    m_contentBlockBtn.gameObject.SetActive(false);

                return;
            }

            if (m_contentImageLock != null)
            {
                m_contentImageColor = m_contentImageLock.color;
            }

            if (m_contentTextLock != null)
            {
                m_contentTextColor = m_contentTextLock.color;
            }

            if (ContentOpenConditionManager.Instance.IsOpen(m_v2Enum_ContentType) == false)
            {
                if (m_contentBlockBtn != null)
                    m_contentBlockBtn.onClick.AddListener(OnClick_Block);

                ContentOpenConditionManager.Instance.AddOpenConditionEvent(m_myContentData.UnlockConditionType, RefreshOpenCondition);
                SetLockState(true);
            }
            else
                SetLockState(false);
        }
        //------------------------------------------------------------------------------------
        private void RefreshOpenCondition(V2Enum_OpenConditionType v2Enum_OpenConditionType, int conditionValue)
        {
            bool isopen = ContentOpenConditionManager.Instance.IsOpen(m_v2Enum_ContentType);

            if (isopen == true)
            {
                ContentOpenConditionManager.Instance.RemoveOpenConditionEvent(v2Enum_OpenConditionType, RefreshOpenCondition);
                //Managers.ContentOpenConditionManager.Instance.ShowOpenConditionNotice(m_v2Enum_ContentType);
                SetLockState(false);
            }
        }
        //------------------------------------------------------------------------------------
        private void SetLockState(bool lockstate)
        {
            if (lockstate == true)
            {
                if (m_contentImageLock != null)
                {
                    m_contentImageColor = m_contentImageLock.color;
                    m_contentImageLock.color = Color.gray;
                }

                if (m_contentTextLock != null)
                {
                    m_contentTextColor = m_contentTextLock.color;
                    m_contentTextLock.color = Color.gray;
                }

                for (int i = 0; i < m_contentImages.Count; ++i)
                {
                    if (m_contentImages[i] != null)
                        m_contentImages[i].color = Color.gray;
                }
            }
            else
            {
                if (m_contentImageLock != null)
                {
                    m_contentImageLock.color = m_contentImageColor;
                }

                if (m_contentTextLock != null)
                {
                    m_contentTextLock.color = m_contentTextColor;
                }

                for (int i = 0; i < m_contentImages.Count; ++i)
                {
                    if (m_contentImages[i] != null)
                        m_contentImages[i].color = Color.white;
                }

            }

            if (m_myContentData.IsDisplay == 0)
            {
                if (_hideList != null)
                    _hideList.AllSetActive(lockstate == false);
            }

            if (m_contentBlockBtn != null)
                m_contentBlockBtn.gameObject.SetActive(lockstate);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_Block()
        {
            if (m_myContentData == null)
                return;

            Managers.ContentOpenConditionManager.Instance.ShowOpenConditionNotice(m_myContentData.UnlockConditionType, m_myContentData.UnlockConditionParam);
        }
        //------------------------------------------------------------------------------------
    }
}