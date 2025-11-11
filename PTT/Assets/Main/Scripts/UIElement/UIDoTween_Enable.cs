using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace GameBerry
{
    public class UIDoTween_Enable : MonoBehaviour
    {
        [SerializeField]
        private List<DOTweenAnimation> m_DOTweenAnimation = new List<DOTweenAnimation>();

        public bool DisableRewind = false;

        private void OnEnable()
        {
            for (int i = 0; i < m_DOTweenAnimation.Count; ++i)
            {
                if (m_DOTweenAnimation[i] != null)
                {
                    m_DOTweenAnimation[i].DORewind();
                    m_DOTweenAnimation[i].DORestart();
                }
            }
        }

        private void OnDisable()
        {
            if (DisableRewind == true)
            {
                for (int i = 0; i < m_DOTweenAnimation.Count; ++i)
                {
                    if (m_DOTweenAnimation[i] != null)
                    {
                        m_DOTweenAnimation[i].DORewind();
                    }
                }
            }
        }
    }
}