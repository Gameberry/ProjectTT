using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry
{
    public class UIRedDotElement : MonoBehaviour
    {
        public List<ContentDetailList> m_myRecvRedDotType = new List<ContentDetailList>();

        [SerializeField]
        private ContentDetailList m_myRedDotKind = ContentDetailList.None;

        [SerializeField]
        private Image m_myRedDotImage;

        [SerializeField]
        private int m_visibleCount = 0;

        // 인스펙터창에서 셋팅한건 true, 로직에서 셋팅한건 false
        public bool m_isAutoSetting = true;

        public void Awake()
        {
            if (m_isAutoSetting == false)
                return;

            Init();
        }

        public void Init()
        {
            Button button = GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(OnClick_RedDot);

            Managers.RedDotManager.Instance.AddRedDotElement(this);

            for (int i = 0; i < m_myRecvRedDotType.Count; ++i)
            {
                if (Managers.RedDotManager.Instance.GetMyRedDotState(m_myRecvRedDotType[i]) == true)
                {
                    VisibleRedDot(true);
                }
            }
        }

        public void SetRedDotType(ContentDetailList contentDetailList)
        {
            m_myRedDotKind = contentDetailList;
        }

        public void AddRecvRedDotType(ContentDetailList contentDetailList)
        {
            m_myRecvRedDotType.Add(contentDetailList);
        }

        public void VisibleRedDot(bool visible)
        {
            if (visible == true)
            { 
                m_visibleCount++;
                if (m_myRedDotImage != null)
                    m_myRedDotImage.gameObject.SetActive(true);
            }
            else
            { 
                m_visibleCount--;

                if (m_visibleCount < 1)
                {
                    if (m_myRedDotImage != null)
                        m_myRedDotImage.gameObject.SetActive(false);
                }
            }
        }

        private void OnClick_RedDot()
        {
            Managers.RedDotManager.Instance.HideRedDot(m_myRedDotKind);
        }
    }
}