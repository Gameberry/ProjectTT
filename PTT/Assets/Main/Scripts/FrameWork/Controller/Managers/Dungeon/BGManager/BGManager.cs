using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class BGManager : MonoBehaviour
    {
        private BGLayerGroup[] m_bgLayerGroupList;// = new List<BGLayerGroup>();

        private Transform m_focusTransform;

        private float m_prevCharPos;

        private int currentBGIdx = 0;

        private void Awake()
        {
            m_bgLayerGroupList = GetComponentsInChildren<BGLayerGroup>();
        }

        public void ResetAllGroupPos()
        {
            for (int i = 0; i < m_bgLayerGroupList.Length; ++i)
            {
                m_bgLayerGroupList[i].ResetPos();
            }

            if (m_focusTransform != null)
                m_prevCharPos = m_focusTransform.position.x;
        }

        public void SetFocusTransform(Transform focustrans)
        {
            m_focusTransform = focustrans;
            m_prevCharPos = m_focusTransform.position.x;

            for (int i = 0; i < m_bgLayerGroupList.Length; ++i)
            {
                m_bgLayerGroupList[i].Init();
            }
        }

        private void LateUpdate()
        {
            float addpos = m_focusTransform.position.x - m_prevCharPos;

            for (int i = 0; i < m_bgLayerGroupList.Length; ++i)
            {
                m_bgLayerGroupList[i].AddPosition(addpos, m_focusTransform.position);
            }

            m_prevCharPos = m_focusTransform.position.x;

#if DEV_DEFINE
            if (Input.GetKeyUp(KeyCode.F7))
            {
                currentBGIdx--;
                if (currentBGIdx < 0)
                    currentBGIdx = 0;

                ShowBGIndex(currentBGIdx);
            }

            if (Input.GetKeyUp(KeyCode.F8))
            {
                currentBGIdx++;
                ShowBGIndex(currentBGIdx);
            }
#endif
        }

        public void ShowBGIndex(int idx)
        {
            for (int i = 0; i < m_bgLayerGroupList.Length; ++i)
            {
                BGLayerGroup bGLayerGroup = m_bgLayerGroupList[i];
                bGLayerGroup.ShowBGIndex(idx);
            }

            currentBGIdx = idx;
        }
    }
}