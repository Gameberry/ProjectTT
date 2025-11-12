using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class BoxManager : MonoSingleton<BoxManager>
    {
        private Event.SetInGameRewardPopupMsg m_setInGameRewardPopupMsg = new Event.SetInGameRewardPopupMsg();

        private Event.SetRandomBoxPercentageMsg m_setRandomBoxPercentageMsg = new Event.SetRandomBoxPercentageMsg();

        private StackableLocalTable m_boxLocalTable = null;

        private List<int> reward_type = new List<int>();
        private List<double> before_quan = new List<double>();
        private List<double> reward_quan = new List<double>();
        private List<double> after_quan = new List<double>();

        protected override void Init()
        {
            m_boxLocalTable = TableManager.Instance.GetTableClass<StackableLocalTable>();

        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, BoxData> GetAllBoxData()
        {
            return m_boxLocalTable.GetAllBoxData();
        }
        //------------------------------------------------------------------------------------
        public string GetBoxLocalKey(int index)
        {
            BoxData pointData = m_boxLocalTable.GetBoxData(index);
            if (pointData == null)
                return string.Empty;

            return pointData.NameLocalStringKey;
        }
        //------------------------------------------------------------------------------------
        public Sprite GetBoxSprite(int index)
        {
            BoxData pointData = m_boxLocalTable.GetBoxData(index);
            if (pointData == null)
                return null;

            return StaticResource.Instance.GetIcon(pointData.IconStringKey);
        }
        //------------------------------------------------------------------------------------
        public void SetBoxAmount(int index, double amount)
        {
            if (BoxContainer.m_boxAmount.ContainsKey(index) == false)
            {
                BoxContainer.m_boxAmount.Add(index, 0.0);
            }

            BoxContainer.m_boxAmount[index] = amount;

            if (amount > 0)
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.Inventory_Item);
        }
        //------------------------------------------------------------------------------------
        public double GetBoxAmount(int index)
        {
            if (BoxContainer.m_boxAmount.ContainsKey(index) == true)
                return BoxContainer.m_boxAmount[index];

            return 0.0;
        }
        //------------------------------------------------------------------------------------
        public double AddBoxAmount(int index, double amount)
        {
            if (BoxContainer.m_boxAmount.ContainsKey(index) == false)
            {
                BoxContainer.m_boxAmount.Add(index, 0.0);
            }

            double currentAmount = BoxContainer.m_boxAmount[index] + amount;

            BoxContainer.m_boxAmount[index] = currentAmount;

            if (amount > 0)
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.Inventory_Item);

            return currentAmount;
        }
        //------------------------------------------------------------------------------------
        public double UseBoxAmount(int index, double amount)
        {
            if (BoxContainer.m_boxAmount.ContainsKey(index) == true)
            {
                double currentAmount = BoxContainer.m_boxAmount[index] - amount;

                if (currentAmount < 0.0)
                    currentAmount = 0.0;

                BoxContainer.m_boxAmount[index] = currentAmount;

                OpenBox(index, amount);

                return currentAmount;
            }

            return 0.0;
        }
        //------------------------------------------------------------------------------------
        public V2Enum_BoxType GetBoxType(int index)
        {
            BoxData boxData = m_boxLocalTable.GetBoxData(index);

            if (boxData == null)
                return V2Enum_BoxType.Max;

            return boxData.BoxType;
        }
        //------------------------------------------------------------------------------------
        public void OpenBox(int index, double amount)
        {
            int count = (int)amount;

            BoxData boxData = m_boxLocalTable.GetBoxData(index);

            if (boxData == null)
                return;

            m_setInGameRewardPopupMsg.RewardDatas.Clear();

            reward_type.Clear();
            before_quan.Clear();
            reward_quan.Clear();
            after_quan.Clear();

            int opencount = 0;

            while (opencount < count)
            {
                if (boxData.BoxType == V2Enum_BoxType.PackageTypeBox)
                {
                    for (int i = 0; i < boxData.RewardDatas.Count; ++i)
                    {
                        RewardData rewardData = boxData.RewardDatas[i];

                        reward_type.Add(rewardData.Index);
                        before_quan.Add((int)GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));

                        reward_quan.Add((int)rewardData.Amount);

                        GoodsManager.Instance.AddGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index, rewardData.Amount);
                        after_quan.Add((int)GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));

                        m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);
                    }
                }
                else if (boxData.BoxType == V2Enum_BoxType.RandomTypeBox)
                {
                    BoxComponentsData randomBoxRewardData = boxData.weightedRandomPicker_RandomTypeBox.Pick();
                    RewardData rewardData = randomBoxRewardData.rewardData;

                    reward_type.Add(rewardData.Index);
                    before_quan.Add((int)GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));

                    reward_quan.Add((int)rewardData.Amount);

                    GoodsManager.Instance.AddGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index, rewardData.Amount);
                    after_quan.Add((int)GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));

                    m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);
                }

                opencount++;
            }

            if (m_setInGameRewardPopupMsg.RewardDatas.Count > 0)
            {
                Message.Send(m_setInGameRewardPopupMsg);
                UI.UIManager.DialogEnter<UI.InGameRewardPopupDialog>();
            }

            TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerBoxTable);
            TheBackEnd.TheBackEndManager.Instance.SendUpdateWaitData(true);

            ThirdPartyLog.Instance.SendLog_BoxEvent(index, count
                , reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------
        public void ShowRandomBoxPercentage(int index)
        {
            BoxData boxData = m_boxLocalTable.GetBoxData(index);

            if (boxData == null)
                return;

            m_setRandomBoxPercentageMsg.boxData = boxData;
            Message.Send(m_setRandomBoxPercentageMsg);

            UI.UIManager.DialogEnter<UI.InGameBoxPercentageDialog>();
        }
        //------------------------------------------------------------------------------------
    }
}