using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.Managers
{
    public class ItemRefreshEvent
    {
        public event System.Action<double> RefrashItemDoubleEvent;

        public void RefreshEvent(double amount)
        {
            RefrashItemDoubleEvent?.Invoke(amount);
            //if (RefrashItemDoubleEvent != null)
            //    RefrashItemDoubleEvent(amount);
        }
    }

    public class GoodsManager : MonoSingleton<GoodsManager>
    {
        private Dictionary<V2Enum_Goods, Dictionary<int, ItemRefreshEvent>> m_goodsRefreshEvent = new Dictionary<V2Enum_Goods, Dictionary<int, ItemRefreshEvent>>();

        private BackLightLocalTable backLightLocalTable = null;
        private StackableLocalTable _stackableLocalTable = null;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            //backLightLocalTable = TableManager.Instance.GetTableClass<BackLightLocalTable>();
            _stackableLocalTable = TableManager.Instance.GetTableClass<StackableLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public void InitGoodsContent()
        {
            if (ARRRContainer.RecvDefaultGoods != 1)
            {
                foreach (var pair in ARRRContainer.DefaultGoods)
                    AddGoodsAmount(pair.Key, pair.Value);

                ARRRContainer.RecvDefaultGoods = 1;

                TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerARRRInfoTable);
                TheBackEnd.TheBackEndManager.Instance.AllUpdateTable();
            }

            ARRRContainer.DefaultGoods = null;
        }
        //------------------------------------------------------------------------------------
        public V2Enum_Grade GetBackLightGrade(int targetIdx)
        {
            if (backLightLocalTable == null)
                return V2Enum_Grade.D;

            return backLightLocalTable.GetGrade(targetIdx);
        }
        //------------------------------------------------------------------------------------
        public V2Enum_Goods GetGoodsType(int index)
        { 
            return _stackableLocalTable.GetGoodsType(index);
        }
        //------------------------------------------------------------------------------------
        public string GetGoodsLocalKey(int index)
        {
            V2Enum_Goods type = _stackableLocalTable.GetGoodsType(index);

            return GetGoodsLocalKey(type.Enum32ToInt(), index);
        }
        //------------------------------------------------------------------------------------
        public string GetGoodsLocalKey(int goods, int index)
        {
            V2Enum_Goods type = goods.IntToEnum32<V2Enum_Goods>();

            switch (type)
            {
                case V2Enum_Goods.Gear:
                        return GearManager.Instance.GetSynergyLocalKey(index);
                case V2Enum_Goods.Point:
                case V2Enum_Goods.TimePoint:
                    {
                        return PointDataManager.Instance.GetPointLocalKey(index);
                    }
                case V2Enum_Goods.SummonTicket:
                    {
                        return SummonTicketManager.Instance.GetSummonTicketLocalKey(index);
                    }
                case V2Enum_Goods.Box:
                    {
                        return BoxManager.Instance.GetBoxLocalKey(index);
                    }
                case V2Enum_Goods.Skin:
                    {
                        return CharacterSkinManager.Instance.GetSkinLocalKey(index);
                    }
                case V2Enum_Goods.CharacterSkill:
                    {
                        return ARRRSkillManager.Instance.GetARRRSkillLocalKey(index);
                    }
                case V2Enum_Goods.Synergy:
                    {
                        return SynergyManager.Instance.GetSynergyLocalKey(index);
                    }
                case V2Enum_Goods.Descend:
                    {
                        return DescendManager.Instance.GetSynergyLocalKey(index);
                    }
                case V2Enum_Goods.Relic:
                    {
                        return RelicManager.Instance.GetSynergyLocalKey(index);
                    }
                case V2Enum_Goods.VipPackage:
                    {
                        return VipPackageManager.Instance.GetSynergyLocalKey(index);
                    }
                case V2Enum_Goods.SynergyBreak:
                    {
                        return SynergyManager.Instance.GetSynergyBreakthroughLocalKey(index);
                    }
                case V2Enum_Goods.SynergyRune:
                    {
                        return SynergyRuneManager.Instance.GetSynergyLocalKey(index);
                    }
                case V2Enum_Goods.DescendBreak:
                    {
                        return DescendManager.Instance.GetSynergyBreakthroughLocalKey(index);
                    }
            }

            return string.Empty;
        }
        //------------------------------------------------------------------------------------
        public Sprite GetGoodsSprite(int index)
        {
            V2Enum_Goods type = _stackableLocalTable.GetGoodsType(index);

            return GetGoodsSprite(type, index);
        }
        //------------------------------------------------------------------------------------
        public Sprite GetGoodsSprite(int goods, int index)
        {
            V2Enum_Goods type = goods.IntToEnum32<V2Enum_Goods>();

            if (type == V2Enum_Goods.Max)
                type = _stackableLocalTable.GetGoodsType(index);

            return GetGoodsSprite(type, index);
        }
        //------------------------------------------------------------------------------------
        public Sprite GetGoodsSprite(V2Enum_Goods goods, int index)
        {
            V2Enum_Goods type = goods;

            switch (type)
            {
                case V2Enum_Goods.Gear:
                    {
                        return GearManager.Instance.GetDescendIcon(index);
                    }
                case V2Enum_Goods.Point:
                case V2Enum_Goods.TimePoint:
                    {
                        return PointDataManager.Instance.GetPointSprite(index);
                    }
                case V2Enum_Goods.SummonTicket:
                    {
                        return SummonTicketManager.Instance.GetSummonTicketSprite(index);
                    }
                case V2Enum_Goods.Box:
                    {
                        return BoxManager.Instance.GetBoxSprite(index);
                    }
                case V2Enum_Goods.Skin:
                    {
                        return CharacterSkinManager.Instance.GetSkinSprite(index);
                    }
                case V2Enum_Goods.CharacterSkill:
                    {
                        return ARRRSkillManager.Instance.GetARRRSKillSprite(index);
                    }
                case V2Enum_Goods.Synergy:
                    {
                        return SynergyManager.Instance.GetSynergySprite(index);
                    }
                case V2Enum_Goods.Descend:
                    {
                        return DescendManager.Instance.GetDescendIcon(index);
                    }
                case V2Enum_Goods.Relic:
                    {
                        return RelicManager.Instance.GetRelicIcon(index);
                    }
                case V2Enum_Goods.VipPackage:
                    {
                        return VipPackageManager.Instance.GetRelicIcon(index);
                    }
                case V2Enum_Goods.SynergyBreak:
                    {
                        return SynergyManager.Instance.GetSynergyBreakthroughSprite(index);
                    }
                case V2Enum_Goods.SynergyRune:
                    {
                        return SynergyRuneManager.Instance.GetDescendIcon(index);
                    }
                case V2Enum_Goods.DescendBreak:
                    {
                        return DescendManager.Instance.GetSynergyBreakthroughSprite(index);
                    }
            }

            return null;
        }
        //------------------------------------------------------------------------------------
        public void SetGoodsAmount(int index, double amount)
        {
            V2Enum_Goods type = _stackableLocalTable.GetGoodsType(index);

            SetGoodsAmount(type, index, amount);
        }
        //------------------------------------------------------------------------------------
        public void SetGoodsAmount(int goods, int index, double amount)
        {
            V2Enum_Goods type = goods.IntToEnum32<V2Enum_Goods>();

            if (type == V2Enum_Goods.Max)
                type = _stackableLocalTable.GetGoodsType(index);

            SetGoodsAmount(type, index, amount);
        }
        //------------------------------------------------------------------------------------
        public void SetGoodsAmount(V2Enum_Goods goods, int index, double amount)
        {
            V2Enum_Goods type = goods;

            switch (type)
            {
                case V2Enum_Goods.Gear:
                    {
                        GearManager.Instance.SetSynergyAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerGearTable);
                        break;
                    }
                case V2Enum_Goods.TimePoint:
                    {

                        break;
                    }
                case V2Enum_Goods.Point:
                    {
                        PointDataManager.Instance.SetPointAmount(index, amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerPointTable);
                        break;
                    }
                case V2Enum_Goods.SummonTicket:
                    {
                        SummonTicketManager.Instance.SetSummonTicketAmount(index, amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSummonTicketTable);
                        break;
                    }
                case V2Enum_Goods.Box:
                    {
                        BoxManager.Instance.SetBoxAmount(index, amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerBoxTable);
                        break;
                    }
                case V2Enum_Goods.Skin:
                    {
                        CharacterSkinManager.Instance.SetSkinAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSkinTable);
                        break;
                    }
                case V2Enum_Goods.CharacterSkill:
                    {
                        ARRRSkillManager.Instance.SetSkillAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSkillInfoTable);
                        break;
                    }
                case V2Enum_Goods.Synergy:
                    {
                        SynergyManager.Instance.SetSynergyAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSynergyInfoTable);
                        break;
                    }
                case V2Enum_Goods.Descend:
                    {
                        DescendManager.Instance.SetSynergyAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerDescendInfoTable);
                        break;
                    }
                case V2Enum_Goods.Relic:
                    {
                        RelicManager.Instance.SetSynergyAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerRelicInfoTable);
                        break;
                    }
                case V2Enum_Goods.VipPackage:
                    {
                        VipPackageManager.Instance.SetSynergyAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerVipPackageInfoTable);
                        break;
                    }
                case V2Enum_Goods.SynergyBreak:
                    {
                        SynergyManager.Instance.SetSynergyBreakthroughAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSynergyInfoTable);
                        break;
                    }
                case V2Enum_Goods.SynergyRune:
                    {
                        SynergyRuneManager.Instance.SetSynergyAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSynergyRuneInfoTable);
                        break;
                    }
                case V2Enum_Goods.DescendBreak:
                    {
                        DescendManager.Instance.SetSynergyBreakthroughAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerDescendInfoTable);
                        break;
                    }
            }

            RefreshItemAmountUI(goods, index, amount);
        }
        //------------------------------------------------------------------------------------
        public double GetGoodsAmount(int index)
        {
            V2Enum_Goods type = _stackableLocalTable.GetGoodsType(index);

            return GetGoodsAmount(type, index);
        }
        //------------------------------------------------------------------------------------
        public double GetGoodsAmount(int goods, int index)
        {
            V2Enum_Goods type = goods.IntToEnum32<V2Enum_Goods>();

            return GetGoodsAmount(type, index);
        }
        //------------------------------------------------------------------------------------
        public double GetGoodsAmount(V2Enum_Goods goods, int index)
        {
            V2Enum_Goods type = goods;

            switch (type)
            {
                case V2Enum_Goods.Gear:
                    {
                        return GearManager.Instance.GetSynergyAmount(index);
                    }
                case V2Enum_Goods.Point:
                case V2Enum_Goods.TimePoint:
                    {
                        return PointDataManager.Instance.GetPointAmount(index);
                    }
                case V2Enum_Goods.SummonTicket:
                    {
                        return SummonTicketManager.Instance.GetSummonTicketAmount(index);
                    }
                case V2Enum_Goods.Box:
                    {
                        return BoxManager.Instance.GetBoxAmount(index);
                    }
                case V2Enum_Goods.Skin:
                    {
                        return CharacterSkinManager.Instance.GetSkinAmount(index);
                    }
                case V2Enum_Goods.CharacterSkill:
                    {
                        return ARRRSkillManager.Instance.GetSkillAmount(index);
                    }
                case V2Enum_Goods.Synergy:
                    {
                        return SynergyManager.Instance.GetSynergyAmount(index);
                    }
                case V2Enum_Goods.Descend:
                    {
                        return DescendManager.Instance.GetSynergyAmount(index);
                    }
                case V2Enum_Goods.Relic:
                    {
                        return RelicManager.Instance.GetSynergyAmount(index);
                    }
                case V2Enum_Goods.VipPackage:
                    {
                        return VipPackageManager.Instance.GetSynergyAmount(index);
                    }
                case V2Enum_Goods.SynergyBreak:
                    {
                        return SynergyManager.Instance.GetSynergyBreakthroughAmount(index);
                    }
                case V2Enum_Goods.SynergyRune:
                    {
                        return SynergyRuneManager.Instance.GetSynergyAmount(index);
                    }
                case V2Enum_Goods.DescendBreak:
                    {
                        return DescendManager.Instance.GetSynergyBreakthroughAmount(index);
                    }
            }

            return 0.0;
        }
        //------------------------------------------------------------------------------------
        public void AddGoodsAmount(int index, double amount)
        {
            V2Enum_Goods type = _stackableLocalTable.GetGoodsType(index);

            AddGoodsAmount(type, index, amount);
        }
        //------------------------------------------------------------------------------------
        public void AddGoodsAmount(int goods, int index, double amount)
        {
            V2Enum_Goods type = goods.IntToEnum32<V2Enum_Goods>();

            if (type == V2Enum_Goods.Max)
                type = _stackableLocalTable.GetGoodsType(index);

            AddGoodsAmount(type, index, amount);
        }
        //------------------------------------------------------------------------------------
        public void AddGoodsAmount(V2Enum_Goods goods, int index, double amount)
        {
            V2Enum_Goods type = goods;

            double currentAmount = 0.0;

            switch (type)
            {
                case V2Enum_Goods.Gear:
                    {
                        currentAmount = GearManager.Instance.AddSynergyAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerGearTable);
                        break;
                    }
                case V2Enum_Goods.TimePoint:
                    {
                        currentAmount = PointDataManager.Instance.AddPointAmount(index, GetTimeGoodsAmount(index, amount));
                        goods = V2Enum_Goods.Point;
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerPointTable);
                        break;
                    }
                case V2Enum_Goods.Point:
                    {
                        currentAmount = PointDataManager.Instance.AddPointAmount(index, amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerPointTable);
                        break;
                    }
                case V2Enum_Goods.SummonTicket:
                    {
                        currentAmount = SummonTicketManager.Instance.AddSummonTicketAmount(index, amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSummonTicketTable);
                        break;
                    }
                case V2Enum_Goods.Box:
                    {
                        currentAmount = BoxManager.Instance.AddBoxAmount(index, amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerBoxTable);
                        break;
                    }
                case V2Enum_Goods.Skin:
                    {
                        currentAmount = CharacterSkinManager.Instance.AddSkinAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSkinTable);
                        break;
                    }
                case V2Enum_Goods.CharacterSkill:
                    {
                        currentAmount = ARRRSkillManager.Instance.AddSkillAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSkillInfoTable);
                        break;
                    }
                case V2Enum_Goods.Synergy:
                    {
                        currentAmount = SynergyManager.Instance.AddSynergyAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSynergyInfoTable);
                        break;
                    }
                case V2Enum_Goods.Descend:
                    {
                        currentAmount = DescendManager.Instance.AddSynergyAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerDescendInfoTable);
                        break;
                    }
                case V2Enum_Goods.Relic:
                    {
                        currentAmount = RelicManager.Instance.AddSynergyAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerRelicInfoTable);
                        break;
                    }
                case V2Enum_Goods.VipPackage:
                    {
                        currentAmount = VipPackageManager.Instance.AddSynergyAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerVipPackageInfoTable);
                        break;
                    }
                case V2Enum_Goods.SynergyBreak:
                    {
                        currentAmount = SynergyManager.Instance.AddSynergyBreakthroughAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSynergyInfoTable);
                        break;
                    }
                case V2Enum_Goods.SynergyRune:
                    {
                        currentAmount = SynergyRuneManager.Instance.AddSynergyAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSynergyRuneInfoTable);
                        break;
                    }
                case V2Enum_Goods.DescendBreak:
                    {
                        currentAmount = DescendManager.Instance.AddSynergyBreakthroughAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerDescendInfoTable);
                        break;
                    }
            }

            RefreshItemAmountUI(goods, index, currentAmount);
        }
        //------------------------------------------------------------------------------------
        public void AddGoodsAmount(RewardData rewardData)
        {
            if (rewardData == null)
                return;

            V2Enum_Goods type = rewardData.V2Enum_Goods;
            if (type == V2Enum_Goods.Max)
                type = _stackableLocalTable.GetGoodsType(rewardData.Index);

            double currentAmount = 0.0;

            switch (type)
            {
                case V2Enum_Goods.Synergy:
                    {
                        currentAmount = SynergyManager.Instance.AddSynergyAmount(rewardData);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSynergyInfoTable);
                        break;
                    }
                case V2Enum_Goods.Descend:
                    {
                        currentAmount = DescendManager.Instance.AddSynergyAmount(rewardData);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerDescendInfoTable);
                        break;
                    }
                case V2Enum_Goods.SynergyBreak:
                    {
                        currentAmount = SynergyManager.Instance.AddSynergyBreakthroughAmount(rewardData);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSynergyInfoTable);
                        break;
                    }
                case V2Enum_Goods.DescendBreak:
                    {
                        currentAmount = DescendManager.Instance.AddSynergyBreakthroughAmount(rewardData);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerDescendInfoTable);
                        break;
                    }
                default:
                    {
                        AddGoodsAmount(rewardData.V2Enum_Goods, rewardData.Index, rewardData.Amount);
                        return;
                    }
            }

            RefreshItemAmountUI(type, rewardData.Index, currentAmount);
        }
        //------------------------------------------------------------------------------------
        public void UseGoodsAmount(int index, double amount)
        {
            V2Enum_Goods type = _stackableLocalTable.GetGoodsType(index);

            UseGoodsAmount(type, index, amount);
        }
        //------------------------------------------------------------------------------------
        public void UseGoodsAmount(int goods, int index, double amount)
        {
            V2Enum_Goods type = goods.IntToEnum32<V2Enum_Goods>();

            if (type == V2Enum_Goods.Max)
                type = _stackableLocalTable.GetGoodsType(index);

            UseGoodsAmount(type, index, amount);
        }
        //------------------------------------------------------------------------------------
        public void UseGoodsAmount(V2Enum_Goods goods, int index, double amount)
        {
            V2Enum_Goods type = goods;

            double currentAmount = 0.0;

            switch (type)
            {
                case V2Enum_Goods.Gear:
                    {
                        currentAmount = GearManager.Instance.UseSynergyAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerGearTable);
                        break;
                    }
                case V2Enum_Goods.Point:
                    {
                        currentAmount = PointDataManager.Instance.UsePointAmount(index, amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerPointTable);
                        break;
                    }
                case V2Enum_Goods.SummonTicket:
                    {
                        currentAmount = SummonTicketManager.Instance.UseSummonTicketAmount(index, amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSummonTicketTable);
                        break;
                    }
                case V2Enum_Goods.Box:
                    {
                        currentAmount = BoxManager.Instance.UseBoxAmount(index, amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerBoxTable);
                        break;
                    }
                case V2Enum_Goods.Skin:
                    {
                        currentAmount = CharacterSkinManager.Instance.UseSkinAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSkinTable);
                        break;
                    }
                case V2Enum_Goods.CharacterSkill:
                    {
                        currentAmount = ARRRSkillManager.Instance.UseSkillAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSkillInfoTable);
                        break;
                    }
                case V2Enum_Goods.Synergy:
                    {
                        currentAmount = SynergyManager.Instance.UseSynergyAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSynergyInfoTable);
                        break;
                    }
                case V2Enum_Goods.Descend:
                    {
                        currentAmount = DescendManager.Instance.UseSynergyAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerDescendInfoTable);
                        break;
                    }
                case V2Enum_Goods.Relic:
                    {
                        currentAmount = RelicManager.Instance.UseSynergyAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerRelicInfoTable);
                        break;
                    }
                case V2Enum_Goods.SynergyBreak:
                    {
                        currentAmount = SynergyManager.Instance.UseSynergyBreakthroughAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSynergyInfoTable);
                        break;
                    }
                case V2Enum_Goods.SynergyRune:
                    {
                        currentAmount = SynergyRuneManager.Instance.UseSynergyAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSynergyRuneInfoTable);
                        break;
                    }
                case V2Enum_Goods.DescendBreak:
                    {
                        currentAmount = DescendManager.Instance.UseSynergyBreakthroughAmount(index, (int)amount);
                        TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerDescendInfoTable);
                        break;
                    }
            }

            RefreshItemAmountUI(goods, index, currentAmount);
        }
        //------------------------------------------------------------------------------------
        public V2Enum_Grade GetGoodsGrade(int index)
        {
            V2Enum_Goods type = _stackableLocalTable.GetGoodsType(index);

            return GetGoodsGrade(type, index);
        }
        //------------------------------------------------------------------------------------
        public V2Enum_Grade GetGoodsGrade(int goods, int index)
        {
            V2Enum_Goods type = goods.IntToEnum32<V2Enum_Goods>();

            return GetGoodsGrade(type, index);
        }
        //------------------------------------------------------------------------------------
        public V2Enum_Grade GetGoodsGrade(V2Enum_Goods goods, int index)
        {
            V2Enum_Goods type = goods;

            switch (type)
            {
                case V2Enum_Goods.Gear:
                    {
                        return GearManager.Instance.GetSynergyGrade(index);
                    }
                case V2Enum_Goods.SummonTicket:
                    {
                        return SummonTicketManager.Instance.GetSummonTicketGrade(index);
                    }
                case V2Enum_Goods.Skin:
                    {
                        return CharacterSkinManager.Instance.GetSkinGrade(index);
                    }
                case V2Enum_Goods.CharacterSkill:
                    {
                        return ARRRSkillManager.Instance.GetARRRSkillGrade(index);
                    }
                case V2Enum_Goods.Synergy:
                    {
                        return SynergyManager.Instance.GetSynergyGrade(index);
                    }
                case V2Enum_Goods.Descend:
                    {
                        return DescendManager.Instance.GetSynergyGrade(index);
                    }
                case V2Enum_Goods.SynergyBreak:
                    {
                        return SynergyManager.Instance.GetSynergyBreakthroughGrade(index);
                    }
                case V2Enum_Goods.SynergyRune:
                    {
                        return SynergyRuneManager.Instance.GetSynergyGrade(index);
                    }
            }

            return V2Enum_Grade.Max;
        }
        //------------------------------------------------------------------------------------
        public double GetTimeGoodsAmount(int index, double time)
        {
            //switch (index.IntToEnum32<V2Enum_Point>())
            //{
            //    case V2Enum_Point.Gold:
            //        {
            //            int currentstage = DungeonDataManager.Instance.GetMaxClearFarmStageStep();

            //            StageCooltimeRewardData stageCooltimeRewardData = DungeonDataOperator.GetStageCooltimeRewardData(currentstage);
            //            if (stageCooltimeRewardData == null)
            //                return 0.0;

            //            StageCooltimeRewardElementData stageCooltimeRewardElementData = stageCooltimeRewardData.StageCooltimeRewardElementDatas.Find(x => x.GoodsType == V2Enum_Goods.Point && x.GoodsIndex == index);
            //            if (stageCooltimeRewardElementData == null)
            //                return 0.0;

            //            double rewardamount = stageCooltimeRewardElementData.Amount;
            //            return rewardamount * time * Define.StageCoolTimeRewardTimeGab;
            //        }
            //}

            return 0.0;
        }
        //------------------------------------------------------------------------------------
        public void AddGoodsRefreshEvent(int index, System.Action<double> action)
        {
            V2Enum_Goods type = GetGoodsType(index);
            AddGoodsRefreshEvent(type, index, action);
        }
        //------------------------------------------------------------------------------------
        public void AddGoodsRefreshEvent(int goods, int index, System.Action<double> action)
        {
            V2Enum_Goods type = goods.IntToEnum32<V2Enum_Goods>();
            AddGoodsRefreshEvent(type, index, action);
        }
        //------------------------------------------------------------------------------------
        public void AddGoodsRefreshEvent(V2Enum_Goods goods, int index, System.Action<double> action)
        {
            if (m_goodsRefreshEvent.ContainsKey(goods) == false)
                m_goodsRefreshEvent.Add(goods, new Dictionary<int, ItemRefreshEvent>());

            if (m_goodsRefreshEvent[goods].ContainsKey(index) == false)
                m_goodsRefreshEvent[goods].Add(index, new ItemRefreshEvent());

            m_goodsRefreshEvent[goods][index].RefrashItemDoubleEvent += action;
        }
        //------------------------------------------------------------------------------------
        public void RemoveGoodsRefreshEvent(int index, System.Action<double> action)
        {
            V2Enum_Goods type = GetGoodsType(index);
            RemoveGoodsRefreshEvent(type, index, action);
        }
        //------------------------------------------------------------------------------------
        public void RemoveGoodsRefreshEvent(int goods, int index, System.Action<double> action)
        {
            V2Enum_Goods type = goods.IntToEnum32<V2Enum_Goods>();
            RemoveGoodsRefreshEvent(type, index, action);
        }
        //------------------------------------------------------------------------------------
        public void RemoveGoodsRefreshEvent(V2Enum_Goods goods, int index, System.Action<double> action)
        {
            if (m_goodsRefreshEvent.ContainsKey(goods) == false)
                return;

            if (m_goodsRefreshEvent[goods].ContainsKey(index) == false)
                return;

            m_goodsRefreshEvent[goods][index].RefrashItemDoubleEvent -= action;
        }
        //------------------------------------------------------------------------------------
        public void RefreshItemAmountUI(V2Enum_Goods goods, int index, double amount)
        {
            if (m_goodsRefreshEvent.ContainsKey(goods) == false)
                m_goodsRefreshEvent.Add(goods, new Dictionary<int, ItemRefreshEvent>());

            if (m_goodsRefreshEvent[goods].ContainsKey(index) == false)
                m_goodsRefreshEvent[goods].Add(index, new ItemRefreshEvent());

            m_goodsRefreshEvent[goods][index].RefreshEvent(amount);
        }
        //------------------------------------------------------------------------------------
    }
}