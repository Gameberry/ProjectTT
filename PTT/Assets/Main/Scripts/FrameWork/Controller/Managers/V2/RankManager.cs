using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class RankManager : MonoSingleton<RankManager>
    {
        private Event.SetSodialDialogMsg setRankDetailDialogMsg = new Event.SetSodialDialogMsg();

        public bool CompleteFirstCombatRank = false;

        //------------------------------------------------------------------------------------
        public void InitRankContent()
        {
            RefreshMyRankData();

            TheBackEnd.TheBackEndManager.Instance.GetRankTableList(() =>
            {
                //if (Managers.AllyArenaManager.Instance.NeedTestBattle() == false)
                //{
                //    Managers.AllyArenaManager.Instance.UpdateRank();
                //}
                //TheBackEnd.TheBackEnd_Rank.UpdateUserScore(V2Enum_RankType.Power, RankContainer.MyRankInfo.CombatPower.GetDecrypted(), GetDetailString(), () =>
                //{
                //    TheBackEnd.TheBackEnd_Rank.UpdateUserScore(V2Enum_RankType.Stage, RankContainer.MyRankInfo.Stage.GetDecrypted(), GetDetailString());
                //});
            });
        }
        //------------------------------------------------------------------------------------
        public bool NeedUpdateRank(V2Enum_RankType v2Enum_RankType)
        {
            RankTable rankTable = GetRankTable(v2Enum_RankType);
            if (rankTable == null)
                return false;

            if (v2Enum_RankType == V2Enum_RankType.Stage)
                return rankTable.Score < MapContainer.MaxWaveClear;

            else if (v2Enum_RankType == V2Enum_RankType.Power)
                return rankTable.Score != ARRRStatManager.Instance.GetBattlePower();
            return false;
        }
        //------------------------------------------------------------------------------------
        public void SetBattlePower(double battlepower)
        {
            if (RankContainer.MyRankInfo.CombatPower < battlepower)
            {
                RankContainer.MyRankInfo.CombatPower = battlepower;
            }
        }
        //------------------------------------------------------------------------------------
        public void SetStage(int stage)
        {
            if (RankContainer.MyRankInfo.Stage < stage)
            {
                RankContainer.MyRankInfo.Stage = stage;
            }
        }
        //------------------------------------------------------------------------------------
        public void UpdateRank()
        {
            TheBackEnd.TheBackEndManager.Instance.UpdatePlayerRankInfoTable();
        }
        //------------------------------------------------------------------------------------
        public List<RankerData> GetRankers()
        {
            return RankContainer.RankerDatas;
        }
        //------------------------------------------------------------------------------------
        public MyRankInfo GetMyRankData()
        { 
            return RankContainer.MyRankInfo;
        }
        //------------------------------------------------------------------------------------
        public void RefreshMyRankData()
        {
            //RankContainer.MyRankInfo.CombatPower = CharacterStatManager.Instance.GetBattlePower();
            //RankContainer.MyRankInfo.Stage = DungeonDataManager.Instance.GetMaxClearFarmStageStep();
            RankContainer.MyRankInfo.Stage = MapContainer.MaxWaveClear;
            RankContainer.MyRankInfo.CombatPower = ARRRStatManager.Instance.GetBattlePower();
            SetMyDetailInfo();
        }
        //------------------------------------------------------------------------------------
        public RankTable GetRankTable(V2Enum_RankType v2Enum_RankType)
        {
            if (RankContainer.rankTableData.ContainsKey(v2Enum_RankType) == true)
                return RankContainer.rankTableData[v2Enum_RankType];

            return null;
        }
        //------------------------------------------------------------------------------------
        public void SetMyDetailInfo()
        {
            try
            {
                RankContainer.MyRankInfo.Detail.combatpower = string.Format("{0:0,0}", Managers.ARRRStatManager.Instance.GetBattlePower());
                RankContainer.MyRankInfo.Detail.stage = MapOperator.ConvertWaveTotalNumberToUIString(MapContainer.MaxWaveClear);

                //RankContainer.MyRankInfo.Detail.allySlotData = AllyV3Manager.Instance.GetCurrentAllySlot();

                if (RankContainer.MyRankInfo.Detail.allyDetailData == null)
                    RankContainer.MyRankInfo.Detail.allyDetailData = new List<List<ObscuredInt>>();

                RankContainer.MyRankInfo.Detail.allyDetailData.Clear();

                RankContainer.MyRankInfo.Detail.profile = PlayerDataContainer.Profile;
                RankContainer.MyRankInfo.Detail.server = PlayerDataContainer.DisplayServerName;

                RankContainer.MyRankInfo.Detail.needRefreshString = true;

                //TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSocialInfoTable);
            }
            catch
            {
            }

        }
        //------------------------------------------------------------------------------------
        public string GetDetailString()
        {
            return RankContainer.MyRankInfo.Detail.ToString();
        }
        //------------------------------------------------------------------------------------
        public void ShowRankDetailView(RankerData rankerData)
        {
            if (rankerData == null)
                return;

            //SocialManager.Instance.ShowUserDetailInfo(rankerData.nickName, rankerData.detail);
        }
        //------------------------------------------------------------------------------------
    }
}