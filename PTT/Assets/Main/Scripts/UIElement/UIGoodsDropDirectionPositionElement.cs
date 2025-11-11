using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameBerry.UI
{
    public class UIGoodsDropDirectionPositionElement : MonoBehaviour
    {
        [SerializeField]
        List<V2Enum_Goods> m_myGoodsTarget;


        [SerializeField]
        List<V2Enum_Point> m_myPointTarget;

        [SerializeField]
        private bool m_customPos = false;

        [SerializeField]
        private string m_customKey = string.Empty;

        private void Awake()
        {
            for (int i = 0; i < m_myGoodsTarget.Count; ++i)
            {
                Managers.GoodsDropDirectionManager.Instance.AddGoodsPosition(m_myGoodsTarget[i], transform);
            }

            for (int i = 0; i < m_myPointTarget.Count; ++i)
            {
                Managers.GoodsDropDirectionManager.Instance.AddPointPosition(m_myPointTarget[i], transform);
            }

            if(m_customPos == true)
                Managers.GoodsDropDirectionManager.Instance.AddCustomPosition(m_customKey, transform);
        }
    }
}