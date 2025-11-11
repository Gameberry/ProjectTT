using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameBerry.UI
{
    public class UIResearchLineElement : MonoBehaviour
    {
        [SerializeField]
        private Image m_masteryLineImage;

        [SerializeField]
        private Color m_enableColor;

        [SerializeField]
        private Color m_disableColor;

        [SerializeField]
        private ResearchData _startData;
        [SerializeField]
        private ResearchData _endData;

        //------------------------------------------------------------------------------------
        public void SetEnable(bool isenable)
        {
            if (m_masteryLineImage != null)
                m_masteryLineImage.color = isenable == true ? m_enableColor : m_disableColor;
        }
        //------------------------------------------------------------------------------------
        public void RefreshEnableLine()
        {
            SetEnable(Managers.ResearchManager.Instance.ConnectLine(_startData, _endData));
        }
        //------------------------------------------------------------------------------------
        public void SetStartEndData(ResearchData startData, ResearchData endData)
        {
            _startData = startData;
            _endData = endData;
        }
        //------------------------------------------------------------------------------------
        public void SetLineTrans(UIResearchElement start, UIResearchElement end)
        {
            if (start == null || end == null)
                return;

            Vector3 startpos = start.transform.localPosition;
            Vector3 endpos = end.transform.localPosition;

            Vector3 pos = transform.localPosition;
            pos.x = (startpos.x + endpos.x) * 0.5f;
            pos.y = (startpos.y + endpos.y) * 0.5f;

            transform.localPosition = pos;

            RectTransform rectTransform = GetComponent<RectTransform>();
            Vector2 sizedelta = rectTransform.sizeDelta;
            sizedelta.y = (endpos - startpos).magnitude;
            rectTransform.sizeDelta = sizedelta;

            transform.localRotation = Quaternion.Euler(0, 0, -Mathf.Atan2(endpos.x - startpos.x, endpos.y - startpos.y) * Mathf.Rad2Deg);
        }
        //------------------------------------------------------------------------------------
        public ResearchData GetEndData()
        {
            return _endData;
        }
        //------------------------------------------------------------------------------------
    }
}