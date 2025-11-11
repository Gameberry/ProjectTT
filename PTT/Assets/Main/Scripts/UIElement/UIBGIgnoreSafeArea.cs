using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.UI
{
    public class UIBGIgnoreSafeArea : MonoBehaviour
    {
        public bool X = true;
        public bool Y = true;

        private void Awake()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();

            Vector2 changeAnchorMin = new Vector2();
            changeAnchorMin.x = X == true ? 0.5f : rectTransform.anchorMin.x;
            changeAnchorMin.y = Y == true ? 0.5f : rectTransform.anchorMin.y;


            Vector2 changeAnchorMax = new Vector2();
            changeAnchorMax.x = X == true ? 0.5f : rectTransform.anchorMax.x;
            changeAnchorMax.y = Y == true ? 0.5f : rectTransform.anchorMax.y;

            rectTransform.anchorMin = changeAnchorMin;
            rectTransform.anchorMax = changeAnchorMax;

            Vector3 pos = transform.localPosition;
            if (X == true)
                pos.x = UIManager.Instance.screenCanvasContent.localPosition.x * -1.0f;

            if (Y == true)
                pos.y = UIManager.Instance.screenCanvasContent.localPosition.y * -1.0f;

            transform.localPosition = pos;

            Vector2 sizedelta = rectTransform.sizeDelta;

            if (X == true)
                sizedelta.x = UIManager.Instance.screenCanvasRect.sizeDelta.x;

            if (Y == true)
                sizedelta.y = UIManager.Instance.screenCanvasRect.sizeDelta.y;

            rectTransform.sizeDelta = sizedelta;
        }
    }
}