using UnityEngine;
using System.Collections.Generic;

namespace GameBerry
{
    public class ParticlePoolElement : MonoBehaviour
    {
        public string ParticleKey { get { return m_particleKey; } }
        private string m_particleKey = string.Empty;

        private float m_defaultSimulationSpeed = 1.0f;
        private float m_lastChangeSimulationSpeed = 1.0f;

        private ParticleSystem _particleSystem;

        private event System.Action<ParticlePoolElement> _particleStop;
        public event System.Action<ParticlePoolElement> ParticleStop
        {
            add { _particleStop += value; }
            remove { _particleStop -= value; }
        }

        public void InitParticle(string key)
        {
            m_particleKey = key;

            _particleSystem = GetComponent<ParticleSystem>();

            if (_particleSystem == null)
                return;

            var main = _particleSystem.main;
            main.stopAction = ParticleSystemStopAction.Callback;
            m_defaultSimulationSpeed = main.simulationSpeed;
        }

        public void SetSimulationSpeed(float speed)
        {
            if (m_lastChangeSimulationSpeed == speed)
                return;

            List<ParticleSystem> particleSystems = _particleSystem.transform.GetComponentsInAllChildren<ParticleSystem>();

            for (int i = 0; i < particleSystems.Count; ++i)
            {
                var main = particleSystems[i].main;
                main.simulationSpeed = speed;
            }

            m_lastChangeSimulationSpeed = speed;
        }

        public void SetDefaultSimulationSpeed()
        {
            SetSimulationSpeed(m_defaultSimulationSpeed);
        }

        public void PlayParticle()
        {
            if (_particleSystem == null)
                ParticleManager.Instance.PoolPaticle(this);
            else
            {
                _particleSystem.Stop();
                _particleSystem.Play();
            }
        }

        public void StopParticle()
        {
            if (_particleSystem != null)
                _particleSystem.Stop();

            OnParticleSystemStopped();
        }

        void OnParticleSystemStopped()
        {
            if (_particleStop != null)
                _particleStop(this);

            ParticleManager.Instance.PoolPaticle(this);
        }
    }
}