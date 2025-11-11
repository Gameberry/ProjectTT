using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    [System.Serializable]
    public class SkillParticleData
    {
        public int Index;
        public int Order;
        public ParticleSystem Particle;

        public bool SetParent = false;

        private Queue<ParticleSystem> _particlePool = new Queue<ParticleSystem>();

        public ParticleSystem GetParticle()
        {
            if (_particlePool.Count > 0)
                return _particlePool.Dequeue();

            GameObject clone = Object.Instantiate(Particle.gameObject, Particle.transform.parent);
            clone.transform.localPosition = Particle.gameObject.transform.localPosition;
            ParticleStopListener particleStopListener = clone.AddComponent<ParticleStopListener>();
            ParticleSystem _particle = clone.GetComponent<ParticleSystem>();

            particleStopListener.SetParticle(_particle);
            particleStopListener.AddStopCallback(PoolParticle);

            return _particle;
        }

        private void PoolParticle(ParticleStopListener particleStopListener)
        {
            particleStopListener.gameObject.SetActive(false);

            if (SetParent == true)
            {
                if (Particle != null)
                    particleStopListener.gameObject.transform.SetParent(Particle.transform.parent);
            }
            
            _particlePool.Enqueue(particleStopListener.GetParticle());
        }
    }

    public class SkillParticlePlayer : MonoBehaviour
    {
        [SerializeField]
        private List<SkillParticleData> _skillParticleDatas = new List<SkillParticleData>();

        private Dictionary<int, SkillParticleData> _skillParticleDatas_Dic = new Dictionary<int, SkillParticleData>();

        public void PlayParticle(int index, int order, CharacterControllerBase target)
        {
            SkillParticleData skillParticleData = _skillParticleDatas.Find(x => x.Index == index && x.Order == order);

            if (skillParticleData == null)
                return;

            //ParticleSystem particleSystem = skillParticleData.Particle;
            ParticleSystem particleSystem = skillParticleData.GetParticle();
            if (particleSystem != null)
            {
                particleSystem.gameObject.SetActive(true);
                if (target != null)
                    particleSystem.transform.position = target.transform.position;

                if (skillParticleData.SetParent == true)
                    particleSystem.transform.SetParent(null);

                particleSystem.Stop();
                particleSystem.Play();
            }
        }
    }
}