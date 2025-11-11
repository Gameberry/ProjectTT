#define LOAD_FROM_ASSETBUNDLE

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameBerry.Common;
using DG.Tweening;

namespace GameBerry.Managers
{
    public class SoundManager : MonoSingleton<SoundManager>
    {
        public float volume = 1.0f;
        public float fadeDuration = 1.0f;

        SoundTableAsset _table;

        private const string m_bgmKey = "masterbgm";
        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float m_masterBGMVolume = 1.0f;
        public float MasterBGMVolume { get { return m_masterBGMVolume; }  }

        private const string m_fxKey = "masterfx";
        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float m_masterFXVolume = 1.0f;
        public float MasterFXVolume { get { return m_masterFXVolume; } }

        class ClipCache
        {
            public SoundData data;
            public AudioClip clip;
            public bool instant;
            public bool loop;
        }

        readonly Dictionary<string, ClipCache> _caches = new Dictionary<string, ClipCache>();

        class PlayingAudio
        {
            public ClipCache clipCache;
            public AudioSource audioSource;
        }
        readonly ObjectPool<AudioSource> _audioSourcePool = new ObjectPool<AudioSource>();
        readonly LinkedList<PlayingAudio> _playingAudio = new LinkedList<PlayingAudio>();
        readonly LinkedList<PlayingAudio> _playingBGMAudio = new LinkedList<PlayingAudio>();

        string _lastBGM = string.Empty;

        protected override void Init()
        {
            Message.AddListener<Event.SetBGMVolumeMsg>(SetBGMVolume);
            Message.AddListener<Event.SetFXVolumeMsg>(SetFXVolume);

            SetBGMVolume(PlayerPrefs.GetFloat(m_bgmKey, 0.5f));
            SetFXVolume(PlayerPrefs.GetFloat(m_fxKey, 1.0f));

            GameSettingManager.Instance.AddListener(GameSettingBtn.BGSound, RefreshBGVolum);
            GameSettingManager.Instance.AddListener(GameSettingBtn.FXSound, RefreshFXVolum);
        }

        protected override void Release()
        {
            StopAllSound();
            UnloadAllLoadCaches();

            Message.RemoveListener<Event.SetBGMVolumeMsg>(SetBGMVolume);
            Message.RemoveListener<Event.SetFXVolumeMsg>(SetFXVolume);

            GameSettingManager.Instance.RemoveListener(GameSettingBtn.BGSound, RefreshBGVolum);
            GameSettingManager.Instance.RemoveListener(GameSettingBtn.FXSound, RefreshFXVolum);
        }

        private void RefreshBGVolum()
        {
            if (GameSettingManager.Instance.IsOn(GameSettingBtn.BGSound) == false)
                SetBGMVolume(0.0f);
            else
                SetBGMVolume(1.0f);
        }

        private void RefreshFXVolum()
        {
            if (GameSettingManager.Instance.IsOn(GameSettingBtn.FXSound) == false)
                SetFXVolume(0.0f);
            else
                SetFXVolume(1.0f);
        }

        public void Setup()
        {
            _table = StaticResource.Instance.GetSoundTableAsset();

            for (int i = 0; i < _table.GetSoundDataList().Count; i++)
            {
                SoundData soundData = _table.GetSoundDataList()[i];
                OnPostLoadProcess(soundData, true);
            }
        }

        //private IEnumerator LoadAsync(string id, bool instant = true, bool loop = false)
        //{
        //    if (string.IsNullOrEmpty(id) == true)
        //        yield break;

        //    if (_caches.ContainsKey(id))
        //        yield break;

        //    var data = _table.GetSoundDataList().Find(x => x.FileName == id);
        //    if (data == null)
        //    {
        //        Debug.LogErrorFormat("Could not found 'BT_SoundRow' : {0} of {1}", id, gameObject.name);
        //        yield break;
        //    }

        //    var fullpath = string.Format("{0}{1}", data.FilePath, data.FileName);

        //    yield return StartCoroutine(ResourceLoader.Instance.LoadAsync<AudioClip>(fullpath, 
        //        o => OnPostLoadProcess(o, id, data, instant, loop)));
        //}

        //private void Load(string id, bool instant = true, bool loop = false)
        //{
        //    if (string.IsNullOrEmpty(id) == true)
        //        return;

        //    if (_caches.ContainsKey(id))
        //        return;

        //    var data = _table.GetSoundDataList().Find(x => x.FileName == id);
        //    if (data == null)
        //    {
        //        Debug.LogErrorFormat("Could not found 'BT_SoundRow' : {0} of {1}", id, gameObject.name);
        //        return;
        //    }

        //    var fullpath = string.Format("{0}{1}", data.FilePath, data.FileName);

        //    ResourceLoader.Instance.Load<AudioClip>(fullpath,
        //        o => OnPostLoadProcess(o, id, data, instant, loop));
        //}

        void OnPostLoadProcess(SoundData data, bool instant)
        {
            if (!_caches.ContainsKey(data.FileName))
            {

                var sound = data.Loop ? data.AudioClip : Instantiate(data.AudioClip) as AudioClip;
                _caches.Add(data.FileName, new ClipCache { data = data, clip = sound, instant = instant, loop = data.Loop });
            }
        }

        public void PlayBGM(string id)
        {
            if (_lastBGM == id)
                return;

            StopLastBGM();

            PlaySound(id, true, true);
            _lastBGM = id;
        }

        public void StopLastBGM()
        {
            if (string.IsNullOrEmpty(_lastBGM) == true)
                return;

            StopSound(_lastBGM, true, true);
            _lastBGM = string.Empty;
        }

        public string GetLastBGMID()
        {
            return _lastBGM;
        }

        public void PlaySound(string id, bool fade = false, bool isbgm = false)
        {
            if (string.IsNullOrEmpty(id) == true)
            {
                Debug.LogWarning(string.Format("Empty Name : {0}", id));
                return;
            }

            ClipCache cache;
            if (_caches.TryGetValue(id, out cache))
            {
                float mastervolume = isbgm == true ? m_masterBGMVolume : m_masterFXVolume;

                var source = _audioSourcePool.GetObject();

                if (source == null)
                {
                    if (_playingAudio.Count + _playingBGMAudio.Count >= 3)
                    {
                        var node = _playingAudio.First;
                        if (node != null)
                        {
                            var audio = node.Value;
                            _audioSourcePool.PoolObject(audio.audioSource);
                            _playingAudio.Remove(node);
                        }

                        source = _audioSourcePool.GetObject();
                        if (source == null)
                            return;
                    }
                    else
                    {
                        source = gameObject.AddComponent<AudioSource>();
                        source.dopplerLevel = 0.0f;
                    }
                }

                if (isbgm == false)
                    _playingAudio.AddLast(new PlayingAudio { clipCache = cache, audioSource = source });
                else
                    _playingBGMAudio.AddLast(new PlayingAudio { clipCache = cache, audioSource = source });

                source.clip = cache.clip;
                source.loop = cache.data.Loop;
                source.volume = fade ? 0.0f : cache.data.Volum * mastervolume;
                    
                source.Play();

                if (fade)
                    source.DOFade(cache.data.Volum * mastervolume, fadeDuration);
            }
        }

        public void StopSound(string id, bool fade, bool isbgm = false)
        {
            LinkedListNode<PlayingAudio> node = null;

            if (isbgm == false)
                node = _playingAudio.First;
            else
                node = _playingBGMAudio.First;

            while (node != null)
            {
                var audio = node.Value;
                if (audio.clipCache.data.FileName == id)
                {
                    if (fade)
                    {
                        audio.audioSource.DOFade(0.0f, fadeDuration).OnComplete(
                            () =>
                            {
                                audio.audioSource.Stop();
                            });
                    }
                    else
                    {
                        audio.audioSource.Stop();
                    }

                    break;
                }

                node = node.Next;
            }
        }

        public float GetSoundLength(string id, bool isbgm = false)
        {
            LinkedListNode<PlayingAudio> node = null;

            if (isbgm == false)
                node = _playingAudio.First;
            else
                node = _playingBGMAudio.First;

            float length = 0.0f;

            while (node != null)
            {
                var audio = node.Value;
                if (audio.clipCache.data.FileName == id)
                {
                    length =  audio.audioSource.clip.length;
                    break;
                }

                node = node.Next;
            }

            return length;
        }

        public void SetLastBGMVolum(float volum, bool fade = false, float fadeDuration = 0.0f)
        {
            var node = _playingAudio.First;
            while (node != null)
            {
                var audio = node.Value;
                if (audio.clipCache.data.FileName == GetLastBGMID())
                {
                    if (fade)
                    {
                        audio.audioSource.DOFade(volum, fadeDuration);
                    }
                    else
                    {
                        audio.audioSource.volume = volum;
                    }

                    break;
                }

                node = node.Next;
            }
        }

        public void StopAllSound()
        {
            var node = _playingAudio.First;
            while (node != null)
            {
                var audio = node.Value;
                audio.audioSource.Stop();
                node = node.Next;
            }

            node = _playingBGMAudio.First;
            while (node != null)
            {
                var audio = node.Value;
                audio.audioSource.Stop();
                node = node.Next;
            }
        }

        public bool IsAudioPlaying(string id)
        {
            ClipCache cache;
            _caches.TryGetValue(id, out cache);
            
            var node = _playingAudio.First;
            while (node != null)
            {
                var audio = node.Value;
                if (audio.clipCache.instant && audio.clipCache.clip == cache.clip && audio.audioSource.isPlaying)
                    return audio.audioSource.isPlaying;

                node = node.Next;
            }

            return false;
        }

        void LateUpdate()
        {
            var node = _playingAudio.First;
            while (node != null)
            {
                var audio = node.Value;
                if (!audio.audioSource.isPlaying)
                {
                    audio.audioSource.Stop();
                    //audio.audioSource.clip.UnloadAudioData();
                    audio.audioSource.clip = null;

                    _audioSourcePool.PoolObject(audio.audioSource);
                    _playingAudio.Remove(node);
                }

                node = node.Next;
            }

            node = _playingBGMAudio.First;
            while (node != null)
            {
                var audio = node.Value;
                if (!audio.audioSource.isPlaying)
                {
                    audio.audioSource.Stop();
                    audio.audioSource.clip = null;

                    _audioSourcePool.PoolObject(audio.audioSource);
                    _playingBGMAudio.Remove(node);
                }

                node = node.Next;
            }
        }


        public void UnloadPlayedClip()
        {
            foreach (var cache in _caches)
            {
                if (cache.Value.clip.loadState == AudioDataLoadState.Loaded)
                    cache.Value.clip.UnloadAudioData();
            }
        }

        public void UnloadAllInstantCaches()
        {
            var unloadList = new List<string>();

            foreach (var cache in _caches)
            {
                if (cache.Value.instant)
                {
                    Debug.LogFormat("UnloadAllInstantCaches - {0} - {1} - OK", cache.Value.data.FileName, cache.Value.clip.name);

                    Destroy(cache.Value.clip);
                    cache.Value.clip = null;

                    unloadList.Add(cache.Value.data.FileName);
                }
                else
                    Debug.LogFormat("UnloadAllInstantCaches - {0} - {1} - NO", cache.Value.data.FileName, cache.Value.clip.name);
            }

            for (int i = 0; i < unloadList.Count; ++i)
            {
                _caches.Remove(unloadList[i]);
            }
        }

        public void UnloadAllLoadCaches()
        {
            foreach (var cache in _caches)
            {
                if (!cache.Value.loop)
                    Destroy(cache.Value.clip);

                cache.Value.clip = null;
            }

            _caches.Clear();
        }

        public float GetCliplength(string id)
        {
            return _caches[id].clip.length;
        }

        private void SetBGMVolume(Event.SetBGMVolumeMsg msg)
        {
            SetBGMVolume(msg.volume);
        }

        private void SetFXVolume(Event.SetFXVolumeMsg msg)
        {
            SetFXVolume(msg.volume);
        }

        private void SetBGMVolume(float volume)
        {
            m_masterBGMVolume = Mathf.Clamp(volume, 0.0f, 1.0f);

            var node = _playingBGMAudio.First;
            while (node != null)
            {
                var audio = node.Value;
                if (audio.audioSource.isPlaying)
                    audio.audioSource.volume = audio.clipCache.data.Volum * m_masterBGMVolume;

                node = node.Next;
            }

            PlayerPrefs.SetFloat(m_bgmKey, m_masterBGMVolume);
        }

        private void SetFXVolume(float volume)
        {
            m_masterFXVolume = Mathf.Clamp(volume, 0.0f, 1.0f);

            var node = _playingAudio.First;
            while (node != null)
            {
                var audio = node.Value;
                if (audio.audioSource.isPlaying)
                    audio.audioSource.volume = audio.clipCache.data.Volum * m_masterFXVolume;

                node = node.Next;
            }

            PlayerPrefs.SetFloat(m_fxKey, m_masterFXVolume);
        }
    }
}
