using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.UI
{
    public enum TargetPos
    { 
        Stadard = 0,
        Expand,
    }

    public class BottomStandardAnchors : MonoBehaviour
    {
        private RectTransform m_myRecttransform;
        [HideInInspector]
        public TargetPos CurrnetState = TargetPos.Stadard;

        //------------------------------------------------------------------------------------
        void Start()
        {
            m_myRecttransform = transform.GetComponent<RectTransform>();

            SetBottomStadardAnchors(TargetPos.Stadard);
        }
        //------------------------------------------------------------------------------------
#if UNITY_EDITOR
        void Update()
        {
            if (Input.GetKeyUp(KeyCode.M))
            {
                SetBottomStadardAnchors(TargetPos.Stadard);
            }
        }
#endif
        //------------------------------------------------------------------------------------
        [ContextMenu("ShowTestView")]
        private void ShowTestView()
        {
            SetBottomStadardAnchors(TargetPos.Stadard);
        }
        //------------------------------------------------------------------------------------
        public void SetBottomStadardAnchors(TargetPos targetpos)
        {
            if (m_myRecttransform == null)
                return;

            //if (CurrnetState == targetpos)
            //    return;

            CurrnetState = targetpos;

            float goalpos = 0.0f;

            if (targetpos == TargetPos.Expand)
                goalpos = UIManager.Instance.BottomUIExpandPos.localPosition.y;
            else
                goalpos = UIManager.Instance.BottomUIStandardPos.localPosition.y;

            float targetHeight = 0.0f;

            float scaleheight = ((float)Screen.width / Screen.height) / ((float)9 / 16);
            if (scaleheight < 1)
            {
                targetHeight = ((Define.DefaultScreenWidth / Screen.width) * ((float)Screen.height * 0.5f)) + goalpos;
            }
            else
            {
                float gab = Screen.height / 16.0f;
                targetHeight = ((Define.DefaultScreenWidth / (gab * 9.0f)) * ((float)Screen.height * 0.5f)) + goalpos;
            }

            Vector2 targetSize = m_myRecttransform.sizeDelta;
            targetSize.y = targetHeight;

            m_myRecttransform.sizeDelta = targetSize;
        }
        //------------------------------------------------------------------------------------
    }
}