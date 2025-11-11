using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIShopElement_Group : MonoBehaviour
    {
        public int SiblingIdx = 0;

        [SerializeField]
        private LayoutElement myLayoutElement;

        [SerializeField]
        private GridLayoutGroup myGridLayoutGroup;

        [SerializeField]
        private float baseHeight = 218.0f;

        protected int elementCount = 0;

        public void SetSiblingIdx(int idx)
        {
            SiblingIdx = idx;
        }

        public virtual void SetShopElement()
        {
            
        }

        public virtual void SetSibling()
        {
            transform.SetSiblingIndex(SiblingIdx);
        }

        protected void SetLayoutElementSize()
        {
            if (myLayoutElement == null || myGridLayoutGroup == null)
                return;

            if (elementCount == 0)
                return;

            int linecount = elementCount / myGridLayoutGroup.constraintCount;

            if (elementCount % myGridLayoutGroup.constraintCount > 0)
                linecount++;

            myLayoutElement.minHeight = baseHeight
                + (myGridLayoutGroup.cellSize.y * linecount)
                + (myGridLayoutGroup.spacing.y * (linecount - 1));
        }
    }
}