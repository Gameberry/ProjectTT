using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

namespace GameBerry.Managers
{
    public class NetworkChecker : MonoSingleton<NetworkChecker>
    {
        public event Action OnDisconnected;
        public event Action OnReconnected;

        [Header("Settings")]
        [SerializeField] private float checkInterval = 5f;
        [SerializeField] private string healthCheckUrl = "https://www.google.com/generate_204";
        [SerializeField] private int timeoutSeconds = 4;

        [Header("UI")]
        [SerializeField] private GameObject disconnectedPopup; // 연결 끊김 메시지 UI 프리팹 or 오브젝트

        private bool _wasConnected = true;
        private bool _running = false;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {

        }
        //------------------------------------------------------------------------------------
        public void StartNetworkCheck()
        {
            if (!_running)
            {
                _running = true;
                CheckLoop().Forget();
            }

            if (disconnectedPopup != null)
                disconnectedPopup.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        private async UniTaskVoid CheckLoop()
        {
            var token = this.GetCancellationTokenOnDestroy();

            while (_running)
            {
                await CheckConnection(token);
                await UniTask.Delay(TimeSpan.FromSeconds(checkInterval), cancellationToken: token);
            }
        }
        //------------------------------------------------------------------------------------
        private async UniTask CheckConnection(CancellationToken token)
        {
            using var request = UnityWebRequest.Get(healthCheckUrl);
            request.timeout = timeoutSeconds;

            try
            {
                await request.SendWebRequest().ToUniTask(cancellationToken: token);

                bool isConnected = !(request.result == UnityWebRequest.Result.ConnectionError ||
                                     request.result == UnityWebRequest.Result.ProtocolError);

                if (isConnected && !_wasConnected)
                {
                    _wasConnected = true;
                    Debug.Log("인터넷 연결 복구됨");
                    disconnectedPopup?.SetActive(false);
                    OnReconnected?.Invoke();
                }
                else if (!isConnected && _wasConnected)
                {
                    _wasConnected = false;
                    Debug.LogWarning("인터넷 연결 끊김");
                    disconnectedPopup?.SetActive(true);
                    OnDisconnected?.Invoke();
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Network check cancelled");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Network check exception: {e.Message}");
                if (_wasConnected)
                {
                    _wasConnected = false;
                    disconnectedPopup?.SetActive(true);
                    OnDisconnected?.Invoke();
                }
            }
        }
        //------------------------------------------------------------------------------------
        public bool IsConnected() => _wasConnected;
        //------------------------------------------------------------------------------------
        public void VisibleNetworkDelay()
        { 

        }
        //------------------------------------------------------------------------------------
        private void OnDestroy()
        {
            _running = false;
        }
        //------------------------------------------------------------------------------------
    }
}