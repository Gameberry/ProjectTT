using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class ParticleManager : MonoSingleton<ParticleManager>
    {
        private Dictionary<string, GameObject> m_particleKey = new Dictionary<string, GameObject>();
        private Dictionary<string, Queue<ParticlePoolElement>> m_particleQueue = new Dictionary<string, Queue<ParticlePoolElement>>();
        private readonly string BossAuraBundleTag = "FX_Resources";
        private readonly string BossAuraName = "FX_BossAura_{0}";

        //------------------------------------------------------------------------------------
        public ParticlePoolElement PlayParticle_JustPlayAniTool(ParticleSystem particleSystem)
        { // 사실상 애니툴용이라 그냥 맘대로 처리~

            GameObject clone = Instantiate(particleSystem.gameObject, transform);
            ParticlePoolElement particlePoolElement = clone.AddComponent<ParticlePoolElement>();
            particlePoolElement.InitParticle(string.Empty);
            particlePoolElement.PlayParticle();

            return particlePoolElement;
        }
        //------------------------------------------------------------------------------------
        public ParticlePoolElement GetParticle(string bunblename, string objname)
        {
            ParticlePoolElement particlePoolElement = GetParticleElement(bunblename, objname);
            return particlePoolElement;
        }
        //------------------------------------------------------------------------------------
        private ParticlePoolElement GetParticleElement(string bunblename, string objname)
        {
            string makeKey = string.Format("{0},{1}", bunblename, objname);

            if (m_particleQueue.ContainsKey(makeKey) == false)
            {
                m_particleQueue.Add(makeKey, new Queue<ParticlePoolElement>());
            }

            if (m_particleQueue[makeKey].Count <= 0)
            {
                if (m_particleKey.ContainsKey(makeKey) == false)
                {
                    AssetBundleLoader.Instance.Load<GameObject>(bunblename, objname, o =>
                    {
                        GameObject obj = o as GameObject;
                        if (obj != null)
                        {
                            m_particleKey.Add(makeKey, obj);
                        }
                    });
                }

                if (m_particleKey.ContainsKey(makeKey) == false)
                    return null;

                GameObject obj = m_particleKey[makeKey];

                GameObject clone = Instantiate(obj, transform);
                ParticlePoolElement particlePoolElement = clone.GetComponent<ParticlePoolElement>() ?? clone.AddComponent<ParticlePoolElement>();
                particlePoolElement.InitParticle(makeKey);
                m_particleQueue[makeKey].Enqueue(particlePoolElement);

                particlePoolElement.gameObject.SetActive(false);
            }

            if (m_particleQueue[makeKey].Count <= 0)
                return null;
            else
                return m_particleQueue[makeKey].Dequeue();
        }
        //------------------------------------------------------------------------------------
        public void PoolPaticle(ParticlePoolElement particlePoolElement)
        {
            if (m_particleQueue.ContainsKey(particlePoolElement.ParticleKey) == true)
            {
                m_particleQueue[particlePoolElement.ParticleKey].Enqueue(particlePoolElement);
                particlePoolElement.transform.SetParent(transform);
                particlePoolElement.gameObject.SetActive(false);
            }
            else
            {
                Destroy(particlePoolElement.gameObject);
            }
        }
        //------------------------------------------------------------------------------------
    }
}
