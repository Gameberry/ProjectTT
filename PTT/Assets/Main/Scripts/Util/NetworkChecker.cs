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
        [SerializeField] private GameObject disconnectedPopup; // ¿¬°á ²÷±è ¸Þ½ÃÁö UI ÇÁ¸®ÆÕ or ¿ÀºêÁ§Æ®

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
                    Debug.Log("ÀÎÅÍ³Ý ¿¬°á º¹±¸µÊ");
                    disconnectedPopup?.SetActive(false);
                    OnReconnected?.Invoke();
                }
                else if (!isConnected && _wasConnected)
                {
                    _wasConnected = false;
                    Debug.LogWarning("ÀÎÅÍ³Ý ¿¬°á ²÷±è");
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