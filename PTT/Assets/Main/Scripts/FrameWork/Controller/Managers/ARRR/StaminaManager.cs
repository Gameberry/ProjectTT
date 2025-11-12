using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class StaminaManager : MonoSingleton<StaminaManager>
    {
        private Event.SetInGameRewardPopupMsg m_setInGameRewardPopupMsg = new Event.SetInGameRewardPopupMsg();

        private Event.RefreshStaminaMsg m_refreshEventDungeonMsg = new Event.RefreshStaminaMsg();

        private List<string> m_changeInfoUpdate = new List<string>();

        public event OnCallBack_Double RechargeTime;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            m_changeInfoUpdate.Add(Define.PlayerStaminaInfoTable);
            m_changeInfoUpdate.Add(Define.PlayerPointTable);
        }
        //------------------------------------------------------------------------------------
        public void InitStaminaRpg()
        {
            UnityUpdateManager.Instance.UpdateCoroutineFunc_HalfSec += ChargeTimer;

            double amount = GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), Define.StaminaIndex);
            double maxamount = GetCoinMaxRechargeAmount();
            if (maxamount <= amount)
            {
                //ShowMathRpgRedDot(ContentDetailList.EventMathRpg_Roulette);
                StaminaContainer.StaminaLastChargeTime = TimeManager.Instance.Current_TimeStamp;
            }
            else
            {
                double timegab = TimeManager.Instance.Current_TimeStamp - StaminaContainer.StaminaLastChargeTime;
                if (Define.StaminaChargeTime <= timegab)
                {
                    int chargecount = (int)(timegab / Define.StaminaChargeTime);

                    if (amount + chargecount > maxamount)
                    {
                        GoodsManager.Instance.SetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), Define.StaminaIndex, maxamount);
                        StaminaContainer.StaminaLastChargeTime = TimeManager.Instance.Current_TimeStamp;
                    }
                    else
                    {
                        GoodsManager.Instance.AddGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), Define.StaminaIndex, chargecount);
                        StaminaContainer.StaminaLastChargeTime = TimeManager.Instance.Current_TimeStamp - (timegab - (Define.StaminaChargeTime * chargecount));
                    }
                }
            }

            if (StaminaContainer.EventDayInitTime < TimeManager.Instance.Current_TimeStamp)
            {
                StaminaContainer.EventDayInitTime = TimeManager.Instance.DailyInit_TimeStamp;
                StaminaContainer.ToDayDigAdCount = 0;
                StaminaContainer.ToDayDigDiaBuyCount = 0;
            }

            TimeManager.Instance.AddInitEvent(V2Enum_IntervalType.Day, OnInitDailyContent);
        }
        //------------------------------------------------------------------------------------
        public void ChargeTimer()
        {
            double amount = GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), Define.StaminaIndex);

            if (GetCoinMaxRechargeAmount() <= amount)
            {
                StaminaContainer.StaminaLastChargeTime = TimeManager.Instance.Current_TimeStamp;
                //ShowMathRpgRedDot(ContentDetailList.EventMathRpg_Roulette);
                RechargeTime?.Invoke(-1);
                return;
            }

            double nextChargeTime = Define.StaminaChargeTime + StaminaContainer.StaminaLastChargeTime;

            if (nextChargeTime <= TimeManager.Instance.Current_TimeStamp)
            {
                {
                    double timegab = TimeManager.Instance.Current_TimeStamp - StaminaContainer.StaminaLastChargeTime;
                    double maxamount = GetCoinMaxRechargeAmount();
                    if (Define.StaminaChargeTime <= timegab)
                    {
                        int chargecount = (int)(timegab / Define.StaminaChargeTime);

                        if (amount + chargecount > maxamount)
                        {
                            GoodsManager.Instance.SetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), Define.StaminaIndex, maxamount);
                            StaminaContainer.StaminaLastChargeTime = TimeManager.Instance.Current_TimeStamp;
                        }
                        else
                        {
                            GoodsManager.Instance.AddGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), Define.StaminaIndex, chargecount);
                            StaminaContainer.StaminaLastChargeTime = TimeManager.Instance.Current_TimeStamp - (timegab - (Define.StaminaChargeTime * chargecount));
                        }
                    }
                }

                //{
                //    GoodsManager.Instance.AddGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), Define.StaminaIndex, 1);

                //    amount = GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), Define.StaminaIndex);

                //    //if (amount >= Define.RequiredStamina)
                //    //    ShowMathRpgRedDot(ContentDetailList.EventMathRpg_Roulette);

                //    StaminaContainer.StaminaLastChargeTime = TimeManager.Instance.Current_TimeStamp;

                    
                //}

                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate, null);
            }

            RechargeTime?.Invoke(nextChargeTime - TimeManager.Instance.Current_TimeStamp);
        }
        //------------------------------------------------------------------------------------
        public ObscuredDouble GetCoinMaxRechargeAmount()
        {
            return Define.MaxStamina;
        }
        //------------------------------------------------------------------------------------
        public void UseStamina()
        {
            Managers.GoodsManager.Instance.UseGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), Define.StaminaIndex, Define.RequiredStamina);
            StaminaContainer.StaminaAccumUse += Define.RequiredStamina;
            ThirdPartyLog.Instance.SendLog_UseStamina(MapContainer.MapLastEnter);
            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate, null);
        }
        //------------------------------------------------------------------------------------
        public void OnInitDailyContent(double nextinittimestamp)
        {
            StaminaContainer.EventDayInitTime = nextinittimestamp;
            StaminaContainer.ToDayDigAdCount = 0;
            StaminaContainer.ToDayDigDiaBuyCount = 0;

            Message.Send(m_refreshEventDungeonMsg);

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate, null);
        }
        //------------------------------------------------------------------------------------
        public bool CanAdView()
        {
            return Define.LimitDailyAdStamina > StaminaContainer.ToDayDigAdCount;
        }
        //------------------------------------------------------------------------------------
        public int RemainAdViewCount()
        {
            return Define.LimitDailyAdStamina - StaminaContainer.ToDayDigAdCount;
        }
        //------------------------------------------------------------------------------------
        public void ChargeAdView()
        {
            if (RemainAdViewCount() <= 0)
                return;

            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            UnityPlugins.appLovin.ShowRewardedAd(() =>
            {
                if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                    return;

                StaminaContainer.ToDayDigAdCount += 1;

                GoodsManager.Instance.AddGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.Stamina.Enum32ToInt(), Define.StaminaChargeCount);

                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate, null);

                RewardData rewardData = RewardManager.Instance.GetRewardData();
                rewardData.V2Enum_Goods = V2Enum_Goods.Point;
                rewardData.Index = V2Enum_Point.Stamina.Enum32ToInt();
                rewardData.Amount = Define.StaminaChargeCount;

                m_setInGameRewardPopupMsg.RewardDatas.Clear();
                m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);

                Message.Send(m_setInGameRewardPopupMsg);
                UI.UIManager.DialogEnter<UI.InGameRewardPopupDialog>();

                Message.Send(m_refreshEventDungeonMsg);

                ThirdPartyLog.Instance.SendLog_ChargeStamina_Ad(MapContainer.MapLastEnter);
                ThirdPartyLog.Instance.SendLog_AD_ViewEvent("stamina", 0, GameBerry.Define.IsAdFree == true ? 1 : 2);
            });
        }
        //------------------------------------------------------------------------------------
        public double GetChargeEnterChancePrice()
        {
            return Define.StaminaPrice * System.Math.Pow(2, StaminaContainer.ToDayDigDiaBuyCount);
        }
        //------------------------------------------------------------------------------------
        public bool ReadyChargeEnter_UseDia()
        {
            double price = GetChargeEnterChancePrice();

            if (price > GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.Dia.Enum32ToInt()))
                return false;

            return true;
        }
        //------------------------------------------------------------------------------------
        public void DoCharge_UseDia()
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            double price = GetChargeEnterChancePrice();

            if (ReadyChargeEnter_UseDia() == false)
                return;

            StaminaContainer.ToDayDigDiaBuyCount++;

            int used_type = V2Enum_Point.Dia.Enum32ToInt();
            double former_quan = GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.Dia.Enum32ToInt());
            double used_quan = price;

            GoodsManager.Instance.UseGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.Dia.Enum32ToInt(), price);

            double keep_quan = GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.Dia.Enum32ToInt());

            GoodsManager.Instance.AddGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), V2Enum_Point.Stamina.Enum32ToInt(), Define.StaminaChargeCount);


            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate, null);

            RewardData rewardData = RewardManager.Instance.GetRewardData();
            rewardData.V2Enum_Goods = V2Enum_Goods.Point;
            rewardData.Index = V2Enum_Point.Stamina.Enum32ToInt();
            rewardData.Amount = Define.StaminaChargeCount;



            m_setInGameRewardPopupMsg.RewardDatas.Clear();
            m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);

            Message.Send(m_setInGameRewardPopupMsg);
            UI.UIManager.DialogEnter<UI.InGameRewardPopupDialog>();

            //ThirdPartyLog.Instance.SendLog_Dig_AddShovelEvent(price);

            ThirdPartyLog.Instance.SendLog_ChargeStamina_Dia(MapContainer.MapLastEnter);
            ThirdPartyLog.Instance.SendLog_log_stamina_dia(used_type, former_quan, used_quan, keep_quan);

            Message.Send(m_refreshEventDungeonMsg);
        }
        //------------------------------------------------------------------------------------
    }
}