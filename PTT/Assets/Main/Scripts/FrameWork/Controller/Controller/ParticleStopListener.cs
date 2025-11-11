using UnityEngine;
using System.Collections.Generic;

namespace GameBerry
{
    public class ParticleStopListener : MonoBehaviour
    {
        private ParticleSystem _particleSystem;

        public System.Action<ParticleStopListener> _stopCallBack;

        public void SetParticle(ParticleSystem particleSystem)
        {
            _particleSystem = particleSystem;

            if (_particleSystem == null)
                return;

            var main = _particleSystem.main;
            main.stopAction = ParticleSystemStopAction.Callback;
        }

        public void AddStopCallback(System.Action<ParticleStopListener> action)
        {
            _stopCallBack = action;
        }

        public ParticleSystem GetParticle()
        {
            return _particleSystem;
        }

        void OnParticleSystemStopped()
        {
            _stopCallBack?.Invoke(this);
        }
    }
}