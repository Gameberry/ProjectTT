using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class SummonTicketManager : MonoSingleton<SummonTicketManager>
    {
        private Event.SetInGameRewardPopupMsg m_setInGameRewardPopupMsg = new Event.SetInGameRewardPopupMsg();

        private StackableLocalTable m_summonTicketLocalTable = null;

        private List<int> reward_type = new List<int>();
        private List<double> before_quan = new List<double>();
        private List<double> reward_quan = new List<double>();
        private List<double> after_quan = new List<double>();

        protected override void Init()
        {
            m_summonTicketLocalTable = TableManager.Instance.GetTableClass<StackableLocalTable>();

        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, SummonTicketData> GetAllBoxData()
        {
            return m_summonTicketLocalTable.GetAllSummonTicketData();
        }
        //------------------------------------------------------------------------------------
        public string GetSummonTicketLocalKey(int index)
        {
            SummonTicketData pointData = m_summonTicketLocalTable.GetSummonTicketData(index);
            if (pointData == null)
                return string.Empty;

            return pointData.NameLocalStringKey;
        }
        //------------------------------------------------------------------------------------
        public Sprite GetSummonTicketSprite(int index)
        {
            SummonTicketData pointData = m_summonTicketLocalTable.GetSummonTicketData(index);
            if (pointData == null)
                return null;

            switch (pointData.SummonType)
            {
                case V2Enum_SummonType.SummonGear:
                    {
                        return PointDataManager.Instance.GetPointSprite(V2Enum_Point.BuyDescend.Enum32ToInt());
                    }
                case V2Enum_SummonType.SummonNormal:
                    {
                        return PointDataManager.Instance.GetPointSprite(V2Enum_Point.SkillSummonTicket.Enum32ToInt());
                    }
                case V2Enum_SummonType.SummonRelic:
                    {
                        return PointDataManager.Instance.GetPointSprite(V2Enum_Point.AllySummonTicket.Enum32ToInt());
                    }
            }

            return null;
        }
        //------------------------------------------------------------------------------------
        public V2Enum_Grade GetSummonTicketGrade(int index)
        {
            SummonTicketData pointData = m_summonTicketLocalTable.GetSummonTicketData(index);
            if (pointData == null)
                return V2Enum_Grade.Max;

            return pointData.ReturnGrade;
        }
        //------------------------------------------------------------------------------------
        public void SetSummonTicketAmount(int index, double amount)
        {
            if (SummonTicketContainer.m_summonTicketAmount.ContainsKey(index) == false)
            {
                SummonTicketContainer.m_summonTicketAmount.Add(index, 0.0);
            }

            SummonTicketContainer.m_summonTicketAmount[index] = amount;

            if (amount > 0)
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.Inventory_Item);
        }
        //------------------------------------------------------------------------------------
        public double GetSummonTicketAmount(int index)
        {
            if (SummonTicketContainer.m_summonTicketAmount.ContainsKey(index) == true)
                return SummonTicketContainer.m_summonTicketAmount[index];

            return 0.0;
        }
        //------------------------------------------------------------------------------------
        public double AddSummonTicketAmount(int index, double amount)
        {
            if (SummonTicketContainer.m_summonTicketAmount.ContainsKey(index) == false)
            {
                SummonTicketContainer.m_summonTicketAmount.Add(index, 0.0);
            }

            double currentAmount = SummonTicketContainer.m_summonTicketAmount[index] + amount;

            SummonTicketContainer.m_summonTicketAmount[index] = currentAmount;

            if (amount > 0)
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.Inventory_Item);

            return currentAmount;
        }
        //------------------------------------------------------------------------------------
        public double UseSummonTicketAmount(int index, double amount)
        {
            if (SummonTicketContainer.m_summonTicketAmount.ContainsKey(index) == true)
            {
                double currentAmount = SummonTicketContainer.m_summonTicketAmount[index] - amount;

                if (currentAmount < 0.0)
                    currentAmount = 0.0;

                SummonTicketContainer.m_summonTicketAmount[index] = currentAmount;

                OpenBox(index, amount);

                return currentAmount;
            }

            return 0.0;
        }
        //------------------------------------------------------------------------------------
        public void OpenBox(int index, double amount)
        {
            int count = (int)amount;

            SummonTicketData boxData = m_summonTicketLocalTable.GetSummonTicketData(index);

            if (boxData == null)
                return;

            m_setInGameRewardPopupMsg.RewardDatas.Clear();

            int opencount = 0;

            reward_type.Clear();
            before_quan.Clear();
            reward_quan.Clear();
            after_quan.Clear();

            switch (boxData.SummonType)
            {
                case V2Enum_SummonType.SummonGear:
                    {
                        List<GearData> gearLists = Managers.GearManager.Instance.GetAllGearData(V2Enum_GearType.Weapon);
                        gearLists = gearLists.FindAll(x => x.Grade == boxData.ReturnGrade);

                        while (opencount < count)
                        {
                            int pickidx = gearLists[Random.Range(0, gearLists.Count)].Index;

                            RewardData rewardData = m_setInGameRewardPopupMsg.RewardDatas.Find(x => x.Index == pickidx);
                            if (rewardData == null)
                            {
                                rewardData = RewardManager.Instance.GetRewardData();
                                rewardData.V2Enum_Goods = V2Enum_Goods.Gear;
                                rewardData.Index = pickidx;
                                m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);
                            }

                            rewardData.Amount++;


                            opencount++;
                        }

                        break;
                    }
            }

            if (m_setInGameRewardPopupMsg.RewardDatas.Count > 0)
            {
                for (int i = 0; i < m_setInGameRewardPopupMsg.RewardDatas.Count; ++i)
                {
                    RewardData rewardData = m_setInGameRewardPopupMsg.RewardDatas[i];

                    reward_type.Add(rewardData.Index);
                    before_quan.Add((int)GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));

                    reward_quan.Add((int)rewardData.Amount);

                    GoodsManager.Instance.AddGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index, rewardData.Amount);

                    after_quan.Add((int)GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));
                }

                Message.Send(m_setInGameRewardPopupMsg);
                UI.IDialog.RequestDialogEnter<UI.InGameRewardPopupDialog>();
            }

            TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSummonTicketTable);
            TheBackEnd.TheBackEndManager.Instance.SendUpdateWaitData(true);

            ThirdPartyLog.Instance.SendLog_SummonTicketEvent(index, count
    , reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
    }
}