using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.Managers
{
    public class AdBuffManager : MonoSingleton<AdBuffManager>
    {
        //private Dictionary<StatType, StatElementValue> m_addStatValue = new Dictionary<StatType, StatElementValue>();

        private Event.RefreshAdBuffStateMsg refreshAdBuffStateMsg = new Event.RefreshAdBuffStateMsg();

        private LinkedList<AdBuffActiveData> m_adEnableActiveBuff = new LinkedList<AdBuffActiveData>();

        private AdBuffActiveData _currentActiveBuff = null;

        private Dictionary<int, Sprite> _skillIcons = new Dictionary<int, Sprite>();

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            AdBuffOperator.Init();

            UnityUpdateManager.Instance.UpdateCoroutineFunc_1Sec += RefreshAdBuffState;
        }
        //------------------------------------------------------------------------------------
        public void InitAdBuffContent()
        {
            double currentServerTime = Managers.TimeManager.Instance.Current_TimeStamp;

            foreach (KeyValuePair<int, AdBuffActiveData> pair in GetAllBuffActivaData())
            {
                if (AdBuffContainer.AdBuffInfo.ContainsKey(pair.Key) == false)
                {
                    PlayerAdBuffInfo playerAdBuffInfo = new PlayerAdBuffInfo();
                    playerAdBuffInfo.Index = pair.Key;
                    playerAdBuffInfo.BuffActiveTime = -1.0;

                    AdBuffContainer.AdBuffInfo.Add(pair.Key, playerAdBuffInfo);
                }

                if (currentServerTime < GetBuffEndTime(pair.Value))
                    _currentActiveBuff = pair.Value;
            }

            TimeManager.Instance.OnInitDailyContent += OnInitDayContent;


            if (AdBuffContainer.DailyInitTimeStemp < currentServerTime)
            {
                OnInitDayContent(TimeManager.Instance.DailyInit_TimeStamp);
            }

        }
        //------------------------------------------------------------------------------------
        private void OnInitDayContent(double nextinittimestamp)
        {
            AdBuffContainer.DailyInitTimeStemp = nextinittimestamp;

            AdBuffContainer.BuffActiveCount = 0;

            TheBackEnd.TheBackEndManager.Instance.UpdatePlayerAdBuffInfoTable();

            Message.Send(refreshAdBuffStateMsg);

            //Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.ClanAllyArena);
            //Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.ClanAllyArenaHonorShop);
        }
        //------------------------------------------------------------------------------------
        public Sprite GetSprite(AdBuffActiveData adbuffstatedata)
        {
            if (adbuffstatedata == null)
                return null;

            return GetSprite(adbuffstatedata.ResourceIndex);
        }
        //------------------------------------------------------------------------------------
        public Sprite GetSprite(int iconIndex)
        {
            Sprite sp = null;

            if (_skillIcons.ContainsKey(iconIndex) == false)
            {
                ResourceLoader.Instance.Load<Sprite>(string.Format(Define.AdBuffIconPath, iconIndex), o =>
                {
                    sp = o as Sprite;
                    _skillIcons.Add(iconIndex, sp);
                });
            }
            else
                sp = _skillIcons[iconIndex];

            return sp;
        }
        //------------------------------------------------------------------------------------
        public Dictionary<int, AdBuffActiveData> GetAllBuffActivaData()
        {
            return AdBuffOperator.GetAllBuffActivaData();
        }
        //------------------------------------------------------------------------------------
        public AdBuffActiveData GetAdBuffActiveData(int index)
        {
            return AdBuffOperator.GetBuffActiveData(index);
        }
        //------------------------------------------------------------------------------------
        public PlayerAdBuffInfo GetPlayerAdBuffInfo(AdBuffActiveData adBuffActiveData)
        {
            if (adBuffActiveData == null)
                return null;

            return AdBuffOperator.GetBuffActiveInfo(adBuffActiveData.Index);
        }
        //------------------------------------------------------------------------------------
        public bool IsEnableActiveBuff(AdBuffActiveData adBuffActiveData)
        {
            if (Define.IsAdBuffAlways == true)
                return true;

            return _currentActiveBuff == adBuffActiveData;
        }
        //------------------------------------------------------------------------------------
        public AdBuffActiveData GetBuffData(V2Enum_BuffEffectType v2Enum_BuffEffectType)
        {
            foreach (KeyValuePair<int, AdBuffActiveData> pair in GetAllBuffActivaData())
            {
                if (pair.Value.BuffEffectType == v2Enum_BuffEffectType)
                    return pair.Value;
            }

            return null;
        }
        //------------------------------------------------------------------------------------
        public AdBuffActiveData GetCurrentActiveBuffData()
        {
            return _currentActiveBuff;
        }
        //------------------------------------------------------------------------------------
        public V2Enum_BuffEffectType GetActiveBuffKind()
        {
            if (_currentActiveBuff == null)
                return V2Enum_BuffEffectType.Max;

            return _currentActiveBuff.BuffEffectType;
        }
        //------------------------------------------------------------------------------------
        public int GetRemainBuffCount()
        {
            int count = GetTotalBuffCount() - AdBuffContainer.BuffActiveCount;
            if (count < 0)
                count = 0;

            return count;
        }
        //------------------------------------------------------------------------------------
        public int GetTotalBuffCount()
        {
            return Define.BuffAdDailyCount + Define.InCreaseAdBuffCount;
        }
        //------------------------------------------------------------------------------------
        public double GetBuffEndTime(AdBuffActiveData adBuffActiveData)
        {
            if (adBuffActiveData == null)
                return -1.0;

            PlayerAdBuffInfo playerAdBuffInfo = AdBuffOperator.GetBuffActiveInfo(adBuffActiveData.Index);
            if (playerAdBuffInfo == null)
                return -1.0;

            return playerAdBuffInfo.BuffActiveTime;
        }
        //------------------------------------------------------------------------------------
        public void OnActiveAdBuff(AdBuffActiveData adBuffActiveData, bool istutorial = false)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            if (adBuffActiveData == null)
                return;

            if (GetRemainBuffCount() <= 0)
                return;

            if (_currentActiveBuff == adBuffActiveData)
                return;

            UnityPlugins.appLovin.ShowRewardedAd(() =>
            {
                if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                    return;

                AdBuffContainer.BuffActiveCount += 1;

                PlayerAdBuffInfo playerAdBuffInfo = GetPlayerAdBuffInfo(adBuffActiveData);
                playerAdBuffInfo.BuffActiveTime = Managers.TimeManager.Instance.Current_TimeStamp + (adBuffActiveData.BuffDuration);

                _currentActiveBuff = adBuffActiveData;

                Message.Send(refreshAdBuffStateMsg);

                TheBackEnd.TheBackEndManager.Instance.UpdatePlayerAdBuffInfoTable();

                ThirdPartyLog.Instance.SendLog_AD_ViewEvent(string.Format("buff_{0}", adBuffActiveData.BuffEffectType.Enum32ToInt() - 10), adBuffActiveData.Index, GameBerry.Define.IsAdFree == true ? 1 : 2);
            });

            
        }
        //------------------------------------------------------------------------------------
        private void RefreshAdBuffState()
        {
            if (_currentActiveBuff != null)
            {
                if (GetBuffEndTime(_currentActiveBuff) < TimeManager.Instance.Current_TimeStamp)
                {
                    AdBuffActiveData endbuff = _currentActiveBuff;
                    _currentActiveBuff = null;

                    Message.Send(refreshAdBuffStateMsg);
                }
            }
        }
        //------------------------------------------------------------------------------------
    }
}