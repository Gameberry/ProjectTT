using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gpm.Ui;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace GameBerry.UI
{
    public class UIWaveRewardElement : MonoBehaviour
    {
        [SerializeField]
        private UIGlobalGoodsRewardIconElement _waveRewardElement;

        [SerializeField]
        private Transform alReadyReward;

        [SerializeField]
        private Transform _readyReward;

        [SerializeField]
        private TMP_Text _waveNumber;

        [SerializeField]
        private Button _recvWaveReward;

        private WaveClearRewardData _currentWaveRewardData;

        private System.Action<WaveClearRewardData> _action;

        //------------------------------------------------------------------------------------
        public void Init(System.Action<WaveClearRewardData> action)
        {
            if (_recvWaveReward != null)
                _recvWaveReward.onClick.AddListener(OnClick_RecvWaveReward);
            
            _action = action;
        }
        //------------------------------------------------------------------------------------
        public Vector3 GetButtonPos()
        {
            if (_recvWaveReward == null)
                return transform.position;

            return _recvWaveReward.transform.position;
        }
        //------------------------------------------------------------------------------------
        public void SetQuestGaugeElement(WaveClearRewardData waveRewardData)
        {
            _currentWaveRewardData = waveRewardData;

            if (_currentWaveRewardData != null)
            {
                if (_waveNumber != null)
                {
                    _waveNumber.gameObject.SetActive(true);
                    if (_currentWaveRewardData.StageNumber == 0)
                    { 
                        _waveNumber.text = Managers.LocalStringManager.Instance.GetLocalString("stage/wavetutorialreward");
                        ConnectGuide();
                    }
                    else
                        _waveNumber.SetText("{0}-{1}", _currentWaveRewardData.StageNumber, _currentWaveRewardData.WaveNumber);
                }

                if (_waveRewardElement != null)
                {
                    _waveRewardElement.gameObject.SetActive(true);
                    _waveRewardElement.SetRewardElement(_currentWaveRewardData.PerfectClearReward);
                }

                if (Managers.MapManager.Instance.IsAlReadyWaveClearReward(_currentWaveRewardData) == true)
                {
                    if (alReadyReward != null)
                        alReadyReward.gameObject.SetActive(true);

                    if (_readyReward != null)
                        _readyReward.gameObject.SetActive(false);

                    if (_recvWaveReward != null)
                    {
                        _recvWaveReward.gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (alReadyReward != null)
                        alReadyReward.gameObject.SetActive(false);

                    bool ready = Managers.MapManager.Instance.IsReadyWaveClearReward(_currentWaveRewardData);

                    if (_readyReward != null)
                        _readyReward.gameObject.SetActive(ready);

                    if (_recvWaveReward != null)
                    {
                        _recvWaveReward.gameObject.SetActive(ready);
                    }
                }
            }
            else
            {
                if (alReadyReward != null)
                    alReadyReward.gameObject.SetActive(false);

                if (_readyReward != null)
                    _readyReward.gameObject.SetActive(false);

                if (_recvWaveReward != null)
                    _recvWaveReward.gameObject.SetActive(false);

                if (_waveNumber != null)
                    _waveNumber.gameObject.SetActive(false);

                if (_waveRewardElement != null)
                    _waveRewardElement.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        private void OnClick_RecvWaveReward()
        {
            _action?.Invoke(_currentWaveRewardData);
        }
        //------------------------------------------------------------------------------------
        public void ConnectGuide()
        {
            return;
            if (_recvWaveReward == null)
                return;

            UIGuideInteractor uIGuideInteractor = _recvWaveReward.GetComponent<UIGuideInteractor>();
            if (uIGuideInteractor == null)
            {
                uIGuideInteractor = _recvWaveReward.gameObject.AddComponent<UIGuideInteractor>();
                uIGuideInteractor.MyGuideType = V2Enum_EventType.WaveReward;
                uIGuideInteractor.MyStepID = 1;
                uIGuideInteractor.FocusAngle = 0;
                uIGuideInteractor.FocusParent = null;
                uIGuideInteractor.GuideInteratorActionType = Managers.GuideInteratorActionType.End;
                uIGuideInteractor.IsAutoSetting = false;
                uIGuideInteractor.ConnectInteractor();
            }
        }
        //------------------------------------------------------------------------------------
    }
}