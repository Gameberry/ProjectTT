using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace GameBerry
{
    public class UIDoTween_Click : MonoBehaviour
    {
        [SerializeField]
        private List<DOTweenAnimation> m_DOTweenAnimation = new List<DOTweenAnimation>();

        void Awake()
        {
            Button btn = GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(OnClick_Btn);
            }
        }

        public void OnClick_Btn()
        {
            for (int i = 0; i < m_DOTweenAnimation.Count; ++i)
            {
                if (m_DOTweenAnimation[i] != null)
                {
                    m_DOTweenAnimation[i].DORewind();
                    m_DOTweenAnimation[i].DORestart();
                }
                else
                    Debug.LogWarning("Check UIDoTween_Click DOTweenAnimation List");
            }
        }
    }
}