using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class BGLayerGroup : MonoBehaviour
    {
        [Header("-1 빠름, 0 속도와 정비례, 1느리게, ")]
        [Range(-1.0f, 1.0f)]
        [SerializeField]
        private float m_moveRatio = 1.0f;

        [Header("true로 설정하면 레이어가 고정된다.")]
        [SerializeField]
        private bool m_DontMoveLayer = false;

        [Header("배경 복제가 화면 정비율을 사용하면 안될 때")]
        [SerializeField]
        private bool m_UseCustomWidth = false;

        [Range(0.0f, 2.0f)]
        [SerializeField]
        private float m_CustomWidthRatio = 1.0f;

        private float m_elementXSize = 0.0f;
        private float m_elementXSizeHalf = 0.0f;

        private Vector3 m_originPos;

        [SerializeField]
        private List<BGLayerElement> bGLayerElements = new List<BGLayerElement>();

        public void Init()
        {
            m_elementXSize = Define.DefaultScreenInGameWidth;

            if (m_UseCustomWidth == true)
                m_elementXSize *= m_CustomWidthRatio;

            m_elementXSizeHalf = m_elementXSize * 0.5f;
            m_originPos = transform.position;

            if (m_DontMoveLayer == false)
            { // 여기서 만들어주기
                for (int i = 0; i < 2; ++i)
                {
                    BGLayerElement element = GetComponentInChildren<BGLayerElement>();
                    if (element != null)
                    {
                        GameObject clone = Instantiate(element.gameObject, transform);
                        if (clone != null)
                        {
                            Vector3 pos = element.transform.localPosition;
                            pos.x = i == 0 ? m_elementXSize : m_elementXSize * -1.0f;
                            clone.transform.localPosition = pos;

                            BGLayerElement cloneelement = clone.GetComponent<BGLayerElement>();
                            if (cloneelement != null)
                                bGLayerElements.Add(cloneelement);
                        }
                    }

                    bGLayerElements.Add(element);
                }
            }
        }

        public void AddPosition(float addPos, Vector3 charpos)
        {
            Vector3 pos = transform.position;

            if (m_DontMoveLayer == true)
                pos.x = charpos.x;
            else
            {
                float addResultPos = addPos * m_moveRatio;

                pos.x += addResultPos;

                if (MathDatas.Abs(charpos.x - transform.position.x) > m_elementXSizeHalf)
                {
                    if (charpos.x > transform.position.x)
                        pos.x += m_elementXSize;
                    else
                        pos.x -= m_elementXSize;
                }
            }

            transform.position = pos;
        }

        public void ResetPos()
        {
            transform.position = m_originPos;
        }

        public void ShowBGIndex(int idx)
        {
            for (int i = 0; i < bGLayerElements.Count; ++i)
            {
                BGLayerElement bGLayerElement = bGLayerElements[i];

                for (int j = 0; j < bGLayerElement.BGImage.Count; ++j) 
                {
                    BGGroup bGGroup = bGLayerElement.BGImage[j];

                    bGGroup.BG.AllSetActive(idx == bGGroup.Index);
                }
            }
        }
    }
}