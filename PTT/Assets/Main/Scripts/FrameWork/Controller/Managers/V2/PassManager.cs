using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class PassManager : MonoSingleton<PassManager>
    {
        public Dictionary<V2Enum_PassType, int> m_currentPassCount = new Dictionary<V2Enum_PassType, int>();

        private Event.SetInGameRewardPopupMsg m_setInGameRewardPopupMsg = new Event.SetInGameRewardPopupMsg();
        private Event.RefreshPassListMsg m_refreshPassListMsg = new Event.RefreshPassListMsg();
        private Event.RefreshPassUIMsg m_refreshPassUIMsg = new Event.RefreshPassUIMsg();

        private GameBerry.Event.SetPassRewardDialogMsg m_setPassRewardDialogMsg = new GameBerry.Event.SetPassRewardDialogMsg();

        private List<string> m_changeInfoUpdate = new List<string>();
        private List<string> m_changeInfoUpdate_BuyPass = new List<string>();

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            m_changeInfoUpdate.Add(Define.PlayerPassInfoTable);
            m_changeInfoUpdate.Add(Define.PlayerPointTable);

            m_changeInfoUpdate_BuyPass.Add(Define.PlayerPassInfoTable);
            m_changeInfoUpdate_BuyPass.Add(Define.PlayerShopInfoTable);
            
            PassOperator.Init();
        }
        //------------------------------------------------------------------------------------
        public void InitPassContent()
        { 

        }
        //------------------------------------------------------------------------------------
        public void SetPassContent()
        {
            CheckPassType(V2Enum_PassType.Wave);
            CheckPassType(V2Enum_PassType.CharacterLevel);
            CheckPassType(V2Enum_PassType.SkillLevel);
            CheckPassType(V2Enum_PassType.DescendLevel);
            CheckPassType(V2Enum_PassType.MonsterKill);
        }
        //------------------------------------------------------------------------------------
        public void AddMonsterKillCount(int count)
        {
            PassContainer.AccumMonster += count;

            CheckPassType(V2Enum_PassType.MonsterKill);

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate, null);
        }
        //------------------------------------------------------------------------------------
        public ContentDetailList ConvertPassTypeToContentDetail(V2Enum_PassType v2Enum_PassType)
        {
            if (v2Enum_PassType == V2Enum_PassType.Wave)
            {
                return ContentDetailList.PassWave;
            }
            else if (v2Enum_PassType == V2Enum_PassType.CharacterLevel)
            {
                return ContentDetailList.PassCharacterLevel;
            }
            else if (v2Enum_PassType == V2Enum_PassType.SkillLevel)
            {
                return ContentDetailList.PassSkillLevel;
            }
            else if (v2Enum_PassType == V2Enum_PassType.DescendLevel)
            {
                return ContentDetailList.PassDescendLevel;
            }
            else if (v2Enum_PassType == V2Enum_PassType.MonsterKill)
            {
                return ContentDetailList.PassMonsterKill;
            }


            return ContentDetailList.Pass;
        }
        //------------------------------------------------------------------------------------
        public void CheckPassType(V2Enum_PassType v2Enum_PassType)
        {
            int passCount = 0;

            switch (v2Enum_PassType)
            {
                case V2Enum_PassType.Wave:
                    {
                        passCount = MapContainer.MaxWaveClear.GetDecrypted();
                        SetPassCount(v2Enum_PassType, passCount);
                        break;
                    }
                case V2Enum_PassType.CharacterLevel:
                    {
                        passCount = ARRRStatManager.Instance.GetCharacterLevel();
                        SetPassCount(v2Enum_PassType, passCount);
                        break;
                    }
                case V2Enum_PassType.SkillLevel:
                    {
                        passCount = SynergyContainer.SynergyEffectAccumLevel.GetDecrypted();
                        SetPassCount(v2Enum_PassType, passCount);

                        break;
                    }
                case V2Enum_PassType.DescendLevel:
                    {
                        passCount = DescendContainer.SynergyAccumLevel.GetDecrypted();
                        SetPassCount(v2Enum_PassType, passCount);

                        break;
                    }
                case V2Enum_PassType.MonsterKill:
                    {
                        passCount = PassContainer.AccumMonster.GetDecrypted();
                        SetPassCount(v2Enum_PassType, passCount);

                        break;
                    }
            }
        }
        //------------------------------------------------------------------------------------
        public void SetPassCount(V2Enum_PassType v2Enum_PassType, int count)
        {
            if (v2Enum_PassType == V2Enum_PassType.CharacterLevel)
                return;

            if (m_currentPassCount.ContainsKey(v2Enum_PassType) == false)
                m_currentPassCount.Add(v2Enum_PassType, 0);

            m_currentPassCount[v2Enum_PassType] = count;

            List<PassData> passDatas = GetPassDatas(v2Enum_PassType);

            m_refreshPassListMsg.passDatas.Clear();

            for (int i = 0; i < passDatas.Count; ++i)
            {
                PassData passData = passDatas[i];
                if (passData.IsMinClearParam <= count)
                    m_refreshPassListMsg.passDatas.Add(passData);

                if (IsReadyReward(passData) == true)
                {
                    RedDotManager.Instance.ShowRedDot(ConvertPassTypeToContentDetail(passData.PassType));
                }
            }

            Message.Send(m_refreshPassListMsg);
        }
        //------------------------------------------------------------------------------------
        public Sprite GetPassBanner(V2Enum_PassType v2Enum_PassType)
        {
            return StaticResource.Instance.GetIcon(string.Format("pass/{0}/banner", v2Enum_PassType.Enum32ToInt()));
        }
        //------------------------------------------------------------------------------------
        public PassData GetPassData(int index)
        {
            return PassOperator.GetPassData(index);
        }
        //------------------------------------------------------------------------------------
        public PassData GetPassData_RewardIndex(int rewardIndex)
        {
            return PassOperator.GetPassData_RewardIndex(rewardIndex);
        }
        //------------------------------------------------------------------------------------
        public List<PassData> GetPassDatas(V2Enum_PassType v2Enum_PassType)
        {
            return PassOperator.GetPassDatas(v2Enum_PassType);
        }
        //------------------------------------------------------------------------------------
        public string GetPriceText(PassData passData)
        {
            return ShopManager.Instance.GetPriceText(passData);
        }
        //------------------------------------------------------------------------------------
        public PlayerPassInfo GetPassInfo(PassData passData)
        {
            if (PassContainer.m_passInfos.ContainsKey(passData.Index) == true)
                return PassContainer.m_passInfos[passData.Index];

            return null;
        }
        //------------------------------------------------------------------------------------
        public Dictionary<int, PlayerPassInfo> GetPassAllInfo()
        {
            return PassContainer.m_passInfos;
        }
        //------------------------------------------------------------------------------------
        public void ShowPassDialog(V2Enum_PassType v2Enum_PassType)
        {
            m_setPassRewardDialogMsg.v2Enum_PassType = v2Enum_PassType;
            Message.Send(m_setPassRewardDialogMsg);

            UI.UIManager.DialogEnter<UI.LobbyPassConditionDialog>();
        }
        //------------------------------------------------------------------------------------
        public string GetConvertPassCountText(V2Enum_PassType v2Enum_PassType, int count)
        {
            string localstring = string.Empty;

            switch (v2Enum_PassType)
            {
                case V2Enum_PassType.Wave:
                    {
                        localstring = MapOperator.ConvertWaveTotalNumberToUIString(count);

                        break;
                    }
                case V2Enum_PassType.CharacterLevel:
                    {
                        localstring = string.Format("Lv.{0}", count);

                        break;
                    }
                case V2Enum_PassType.SkillLevel:
                case V2Enum_PassType.DescendLevel:
                case V2Enum_PassType.MonsterKill:
                    {
                        localstring = count.ToString();

                        break;
                    }
            }

            return localstring;
        }
        //------------------------------------------------------------------------------------
        public PassData GetPassFocusData(V2Enum_PassType v2Enum_PassType)
        {
            PassData m_focusData = null;

            List<PassData> passDatas = GetPassDatas(v2Enum_PassType);

            if (passDatas == null || passDatas.Count <= 0)
                return null;

            //double currentcount = PassCurrentCount(v2Enum_PassType);

            for (int i = 0; i < passDatas.Count; ++i)
            {
                if (IsReadyReward(passDatas[i]) == true)
                { 
                    m_focusData = passDatas[i];
                    break;
                }
            }

            if (m_focusData == null)
            {
                int currentcount = PassCurrentCount(v2Enum_PassType);

                for (int i = 0; i < passDatas.Count; ++i)
                {
                    PassData passData = passDatas[i];

                    if (passData.IsMaxClearParam >= currentcount)
                    {
                        m_focusData = passDatas[i];
                        break;
                    }

                    //if (passData.) == true)
                    //{
                    //    m_focusData = passDatas[i];
                    //    break;
                    //}
                }

                //m_focusData = passDatas[passDatas.Count - 1];
            }

            if (m_focusData == null)
            {
                m_focusData = passDatas[passDatas.Count - 1];
            }

            return m_focusData;
        }
        //------------------------------------------------------------------------------------
        public PassData GetPrevLevelPass(PassData passData)
        {
            List<PassData> passDatas = GetPassDatas(passData.PassType);
            if (passDatas == null)
                return null;

            return passDatas.Find(x => x.PassStep.GetDecrypted() == passData.PassStep.GetDecrypted() - 1);
        }
        //------------------------------------------------------------------------------------
        public PassData GetNextLevelPass(PassData passData)
        {
            List<PassData> passDatas = GetPassDatas(passData.PassType);
            if (passDatas == null)
                return null;

            return passDatas.Find(x => x.PassStep.GetDecrypted() == passData.PassStep.GetDecrypted() + 1);
        }
        //------------------------------------------------------------------------------------
        public double GetFreeRewardStep(PassData passData)
        {
            if (passData == null)
                return 0;

            PlayerPassInfo playerPassInfo = GetPassInfo(passData);
            if (playerPassInfo == null)
                return 0;

            return playerPassInfo.RecvFreeRewardCount;
        }
        //------------------------------------------------------------------------------------
        public double GetPaidRewardStep(PassData passData)
        {
            if (passData == null)
                return 0;

            PlayerPassInfo playerPassInfo = GetPassInfo(passData);
            if (playerPassInfo == null)
                return 0;

            return playerPassInfo.RecvPaidRewardCount;
        }
        //------------------------------------------------------------------------------------
        public double PassCurrentCount(PassData passData)
        {
            return PassCurrentCount(passData.PassType);
        }
        //------------------------------------------------------------------------------------
        public int PassCurrentCount(V2Enum_PassType v2Enum_PassType)
        {
            if (m_currentPassCount.ContainsKey(v2Enum_PassType) == true)
                return m_currentPassCount[v2Enum_PassType];

            return 0;
        }
        //------------------------------------------------------------------------------------
        public void RecvPassReward(PassData passData)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            if (IsReadyReward(passData) == false)
                return;

            bool isbuy = IsEnable(passData);

            m_setInGameRewardPopupMsg.RewardDatas.Clear();

            double recvCount = 0;


            bool recvFree = false;
            bool recvBuy = false;

            for (int i = 0; i < passData.passConditionRewardDatas.Count; ++i)
            {
                PassConditionRewardData passConditionRewardData = passData.passConditionRewardDatas[i];

                double currentcount = PassCurrentCount(passConditionRewardData.PassType);

                if (passConditionRewardData.ConditionClearParam.GetDecrypted() > currentcount)
                    break;

                if (passConditionRewardData.ConditionClearParam.GetDecrypted() > GetFreeRewardStep(passData))
                {
                    recvCount = passConditionRewardData.ConditionClearParam.GetDecrypted();

                    RewardData rewardData = m_setInGameRewardPopupMsg.RewardDatas.Find(x => x.Index == passConditionRewardData.FreeRewardGoodsParam1);
                    if (rewardData == null)
                    {
                        rewardData = RewardManager.Instance.GetRewardData();

                        rewardData.V2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(passConditionRewardData.FreeRewardGoodsParam1);
                        rewardData.Index = passConditionRewardData.FreeRewardGoodsParam1;
                        rewardData.Amount = 0;

                        m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);
                    }

                    rewardData.Amount += passConditionRewardData.FreeRewardGoodsParam2;
                    recvFree = true;
                }

                if (isbuy == true)
                {
                    if (passConditionRewardData.ConditionClearParam.GetDecrypted() > GetPaidRewardStep(passData))
                    {
                        recvCount = passConditionRewardData.ConditionClearParam.GetDecrypted();

                        RewardData rewardData = m_setInGameRewardPopupMsg.RewardDatas.Find(x => x.Index == passConditionRewardData.PaidRewardGoodsParam1);
                        if (rewardData == null)
                        {
                            rewardData = RewardManager.Instance.GetRewardData();

                            rewardData.V2Enum_Goods = Managers.GoodsManager.Instance.GetGoodsType(passConditionRewardData.PaidRewardGoodsParam1);
                            rewardData.Index = passConditionRewardData.PaidRewardGoodsParam1;
                            rewardData.Amount = 0;

                            m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);
                        }

                        rewardData.Amount += passConditionRewardData.PaidRewardGoodsParam2;

                        recvBuy = true;
                    }
                }
            }

            if (recvCount <= 0)
                return;

            List<int> reward_type = new List<int>();
            List<double> before_quan = new List<double>();
            List<double> reward_quan = new List<double>();
            List<double> after_quan = new List<double>();

            int logType = 1;

            for (int i = 0; i < m_setInGameRewardPopupMsg.RewardDatas.Count; ++i)
            {
                RewardData rewardData = m_setInGameRewardPopupMsg.RewardDatas[i];

                reward_type.Add(rewardData.Index);
                before_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));
                reward_quan.Add(rewardData.Amount);

                GoodsManager.Instance.AddGoodsAmount(rewardData);

                after_quan.Add((int)GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));
            }

            if (recvFree == true && recvBuy == true)
                logType = 2;
            else if (recvBuy == true)
                logType = 3;

            ThirdPartyLog.Instance.SendLog_PassEvent(passData.Index, logType, reward_type, before_quan, reward_quan, after_quan);

            if (m_setInGameRewardPopupMsg.RewardDatas.Count > 0)
            {
                Message.Send(m_setInGameRewardPopupMsg);
                UI.UIManager.DialogEnter<UI.InGameRewardPopupDialog>();
            }

            PlayerPassInfo playerShopInfo = GetPassInfo(passData) ?? PassOperator.CreatePlayerShopInfo(passData);
            playerShopInfo.RecvFreeRewardCount = recvCount;
            if (isbuy == true)
                playerShopInfo.RecvPaidRewardCount = recvCount;

            m_refreshPassListMsg.passDatas.Clear();
            m_refreshPassListMsg.passDatas.Add(passData);
            Message.Send(m_refreshPassListMsg);

            Message.Send(m_refreshPassUIMsg);

            //GuideQuestManager.Instance.AddEventCount(V2Enum_EventType.PassFreeRewardClaim, 1);

            RedDotManager.Instance.HideRedDot(ConvertPassTypeToContentDetail(passData.PassType));

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate, null);
            TheBackEnd.TheBackEndManager.Instance.SendUpdateWaitData(true);
        }
        //------------------------------------------------------------------------------------
        public bool IsReadyReward(PassData passData)
        {
            if (passData == null)
                return false;

            bool isbuy = IsEnable(passData);

            for (int i = 0; i < passData.passConditionRewardDatas.Count; ++i)
            {
                PassConditionRewardData passConditionRewardData = passData.passConditionRewardDatas[i];

                int currentcount = PassCurrentCount(passConditionRewardData.PassType);

                if (passConditionRewardData.ConditionClearParam.GetDecrypted() > currentcount)
                    break;

                if (passConditionRewardData.ConditionClearParam.GetDecrypted() > GetFreeRewardStep(passData))
                {
                    return true;
                }

                if (isbuy == true)
                {
                    if (passConditionRewardData.ConditionClearParam.GetDecrypted() > GetPaidRewardStep(passData))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        //------------------------------------------------------------------------------------
        public bool IsBuy(PassData passData)
        {
            if (passData == null)
                return false;

            PlayerPassInfo playerPassInfo = GetPassInfo(passData);
            if (playerPassInfo == null)
                return false;

            return playerPassInfo.IsBuy != 0;
        }
        //------------------------------------------------------------------------------------
        public bool IsEnable(PassData passData)
        {
            if (passData == null)
                return false;

            PlayerPassInfo playerPassInfo = GetPassInfo(passData);
            if (playerPassInfo == null)
                return false;

            return playerPassInfo.IsEnable != 0;
        }
        //------------------------------------------------------------------------------------
        public void SetEnable(int index)
        {
            PassData passData = GetPassData(index);

            if (passData == null)
                return;

            PlayerPassInfo playerPassInfo = GetPassInfo(passData);
            if (playerPassInfo == null)
                return;

            playerPassInfo.IsEnable = 1;

            m_refreshPassListMsg.passDatas.Clear();
            m_refreshPassListMsg.passDatas.Add(passData);
            Message.Send(m_refreshPassListMsg);

            Message.Send(m_refreshPassUIMsg);

            TheBackEnd.TheBackEndManager.Instance.UpdatePlayerPassInfoTable();
        }
        //------------------------------------------------------------------------------------
        public void Buy(PassData passData)
        {
            if (IsBuy(passData) == true)
                return;

            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            ShopManager.Instance.ProcessBilling(passData, IAPComplete);
        }
        //------------------------------------------------------------------------------------
        public void IAPComplete(ShopDataBase shopDataBase)
        {
            if (shopDataBase == null)
                return;

            PassData passData = shopDataBase as PassData;

            PlayerPassInfo playerShopInfo = GetPassInfo(passData) ?? PassOperator.CreatePlayerShopInfo(passData);

            playerShopInfo.IsBuy = 1;

            ShopPostManager.Instance.AddShopPost(passData);

            Contents.GlobalContent.ShowPopup_Ok(
                LocalStringManager.Instance.GetLocalString("common/popUp/title"),
                string.Format("{0}\n{1}", LocalStringManager.Instance.GetLocalString(passData.TitleLocalStringKey),
                LocalStringManager.Instance.GetLocalString("common/ui/purchaseSuccess")));

            //Contents.GlobalContent.ShowGlobalNotice(LocalStringManager.Instance.GetLocalString("common/ui/purchaseSuccess"));

            m_refreshPassListMsg.passDatas.Clear();
            m_refreshPassListMsg.passDatas.Add(passData);
            Message.Send(m_refreshPassListMsg);

            Message.Send(m_refreshPassUIMsg);

            TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData(m_changeInfoUpdate_BuyPass, null);
        }
        //------------------------------------------------------------------------------------
    }
}