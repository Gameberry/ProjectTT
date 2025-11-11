using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class WeaponParticleSortingOrder : MonoBehaviour
    {
        List<ParticleSystemRenderer> particleSystems;

        List<SpriteRenderer> spriteRenderers;

        private int m_currentOrderInLayer = -99999;

        public void Init(Renderer renderer)
        {
            particleSystems = transform.GetComponentsInAllChildren<ParticleSystemRenderer>();

            if (particleSystems != null)
            {
                for (int i = 0; i < particleSystems.Count; ++i)
                {
                    particleSystems[i].gameObject.layer = renderer.gameObject.layer;
                    particleSystems[i].sortingLayerName = renderer.sortingLayerName;
                }
            }

            spriteRenderers = transform.GetComponentsInAllChildren<SpriteRenderer>();
            if (spriteRenderers != null)
            {
                for (int i = 0; i < spriteRenderers.Count; ++i)
                {
                    spriteRenderers[i].gameObject.layer = renderer.gameObject.layer;
                    spriteRenderers[i].sortingLayerName = renderer.sortingLayerName;
                }
            }
        }

        public void SetOrderInLayer(int order)
        {
            if (m_currentOrderInLayer == order)
                return;

            m_currentOrderInLayer = order;


            if (particleSystems != null)
            {
                for (int i = 0; i < particleSystems.Count; ++i)
                {
                    particleSystems[i].sortingOrder = order + 1;
                }
            }

            if (spriteRenderers != null)
            {
                for (int i = 0; i < spriteRenderers.Count; ++i)
                {
                    spriteRenderers[i].sortingOrder = order + 1;
                }
            }
        }
    }
}