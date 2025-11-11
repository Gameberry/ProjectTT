using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace GameBerry.UI
{
    public class UIPromoStarPrinter : MonoBehaviour
    {
        [SerializeField]
        private List<Image> m_star;

        [SerializeField]
        private List<DOTweenAnimation> m_starTween;

        public void SetStar(int star)
        {
            Contents.PromoStarSprite promoStarSprite = Contents.GlobalContent.GePromoStarSprite(star);
            if (promoStarSprite == null)
            {
                for (int i = 0; i < m_star.Count; ++i)
                {
                    m_star[i].gameObject.SetActive(false);
                }

                return;
            }

            for (int i = 0; i < promoStarSprite.StarImage.Count; ++i)
            {
                if (m_star.Count > i)
                { 
                    m_star[i].gameObject.SetActive(true);
                    m_star[i].sprite = promoStarSprite.StarImage[i];
                }
            }

            for (int i = promoStarSprite.StarImage.Count; i < m_star.Count; ++i)
            {
                m_star[i].gameObject.SetActive(false);
            }

            //for (int i = 0; i < m_star.Count; ++i)
            //{
            //    m_star[i].gameObject.SetActive(star > i);
            //}
        }

        public void PlayTweenStar(int star)
        {
            int starindex = star - 1;

            if (starindex < 0)
                return;

            if (m_starTween.Count > starindex)
            {
                if (m_starTween[starindex] != null)
                {
                    if (m_starTween[starindex] != null)
                    {
                        m_starTween[starindex].DORewind();
                        m_starTween[starindex].DORestart();
                    }
                }
            }
        }
    }
}