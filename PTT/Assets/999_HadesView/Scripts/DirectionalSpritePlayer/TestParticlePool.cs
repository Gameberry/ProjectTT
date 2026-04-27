using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.TestScene
{
    public class TestParticlePool : MonoBehaviour
    {
        private static TestParticlePool _instance;

        private readonly Dictionary<GameObject, Queue<PooledParticleHandle>> _poolLookup = new Dictionary<GameObject, Queue<PooledParticleHandle>>();
        private readonly List<PooledParticleHandle> _activeParticles = new List<PooledParticleHandle>();

        public static TestParticlePool Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                GameObject poolObject = new GameObject("TestParticlePool");
                _instance = poolObject.AddComponent<TestParticlePool>();
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        private void LateUpdate()
        {
            for (int i = _activeParticles.Count - 1; i >= 0; i--)
            {
                PooledParticleHandle handle = _activeParticles[i];
                if (handle == null)
                {
                    _activeParticles.RemoveAt(i);
                    continue;
                }

                if (handle.IsPlaying())
                    continue;

                ReturnToPool(handle);
                _activeParticles.RemoveAt(i);
            }
        }

        public GameObject Play(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null)
                return null;

            Queue<PooledParticleHandle> pool = GetOrCreatePool(prefab);
            PooledParticleHandle handle = pool.Count > 0 ? pool.Dequeue() : CreateHandle(prefab);
            if (handle == null)
                return null;

            handle.transform.SetPositionAndRotation(position, rotation);
            handle.transform.SetParent(parent, false);
            handle.gameObject.SetActive(true);
            handle.Play();
            _activeParticles.Add(handle);
            return handle.gameObject;
        }

        public GameObject PlayWithSizeY(GameObject prefab, Vector3 position, Quaternion rotation, float startSizeY, Transform parent = null)
        {
            if (prefab == null)
                return null;

            Queue<PooledParticleHandle> pool = GetOrCreatePool(prefab);
            PooledParticleHandle handle = pool.Count > 0 ? pool.Dequeue() : CreateHandle(prefab);
            if (handle == null)
                return null;

            handle.transform.SetPositionAndRotation(position, rotation);
            handle.transform.SetParent(parent, false);
            handle.gameObject.SetActive(true);
            handle.SetStartSizeY(startSizeY);
            handle.Play();
            _activeParticles.Add(handle);
            return handle.gameObject;
        }

        private Queue<PooledParticleHandle> GetOrCreatePool(GameObject prefab)
        {
            if (_poolLookup.TryGetValue(prefab, out Queue<PooledParticleHandle> pool))
                return pool;

            pool = new Queue<PooledParticleHandle>();
            _poolLookup.Add(prefab, pool);
            return pool;
        }

        private PooledParticleHandle CreateHandle(GameObject prefab)
        {
            GameObject instance = Instantiate(prefab, transform);
            instance.name = $"{prefab.name}_Pooled";
            PooledParticleHandle handle = instance.GetComponent<PooledParticleHandle>();
            if (handle == null)
                handle = instance.AddComponent<PooledParticleHandle>();

            handle.Initialize(prefab);
            instance.SetActive(false);
            return handle;
        }

        private void ReturnToPool(PooledParticleHandle handle)
        {
            if (handle == null || handle.SourcePrefab == null)
                return;

            handle.Stop();
            handle.transform.SetParent(transform, false);
            handle.gameObject.SetActive(false);
            GetOrCreatePool(handle.SourcePrefab).Enqueue(handle);
        }

        private sealed class PooledParticleHandle : MonoBehaviour
        {
            private ParticleSystem[] _particleSystems = System.Array.Empty<ParticleSystem>();
            public GameObject SourcePrefab { get; private set; }

            public void Initialize(GameObject sourcePrefab)
            {
                SourcePrefab = sourcePrefab;
                _particleSystems = GetComponentsInChildren<ParticleSystem>(true);
            }

            public void SetStartSizeY(float sizeY)
            {
                for (int i = 0; i < _particleSystems.Length; i++)
                {
                    if (_particleSystems[i] == null)
                        continue;
                    var main = _particleSystems[i].main;
                    main.startSizeY = new ParticleSystem.MinMaxCurve(sizeY);
                }
            }

            public void Play()
            {
                for (int i = 0; i < _particleSystems.Length; i++)
                {
                    ParticleSystem particleSystem = _particleSystems[i];
                    if (particleSystem == null)
                        continue;

                    particleSystem.Clear(true);
                    particleSystem.Play(true);
                }
            }

            public void Stop()
            {
                for (int i = 0; i < _particleSystems.Length; i++)
                {
                    ParticleSystem particleSystem = _particleSystems[i];
                    if (particleSystem == null)
                        continue;

                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }

            public bool IsPlaying()
            {
                if (_particleSystems.Length == 0)
                    return false;

                for (int i = 0; i < _particleSystems.Length; i++)
                {
                    ParticleSystem particleSystem = _particleSystems[i];
                    if (particleSystem != null && particleSystem.IsAlive(true))
                        return true;
                }

                return false;
            }
        }
    }
}
