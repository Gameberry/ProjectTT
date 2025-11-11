using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using LitJson;

namespace GameBerry.TheBackEnd
{
    public static class TheBackEnd_Rank
    {
        //------------------------------------------------------------------------------------
        public static void GetRankTableList(System.Action action)
        {
            Backend.Leaderboard.User.GetLeaderboards(callback => {

                if (callback.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                    return;
                }

                Dictionary<string, V2Enum_RankType> ranktitles = new Dictionary<string, V2Enum_RankType>();
                ranktitles.Add(GetRankTitleName(V2Enum_RankType.Stage), V2Enum_RankType.Stage);
                ranktitles.Add(GetRankTitleName(V2Enum_RankType.Power), V2Enum_RankType.Power);

                foreach (BackEnd.Leaderboard.LeaderboardTableItem item in callback.GetLeaderboardTableList())
                {
                    foreach (var rankname in ranktitles)
                    {
                        if (item.title.Contains(rankname.Key))
                        {
                            if (RankContainer.rankTableData.ContainsKey(rankname.Value) == false)
                            {
                                RankTable rankTable = new RankTable();
                                rankTable.uuid = item.uuid;
                                rankTable.title = item.title;
                                rankTable.table = item.table;
                                rankTable.column = item.column;

                                RankContainer.rankTableData.Add(rankname.Value, rankTable);
                            }

                            break;
                        }
                    }
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static string GetRankTitleName(V2Enum_RankType v2Enum_RankType)
        {
            //string RankName = string.Empty;

            ////if (v2Enum_RankType == V2Enum_RankType.Event)
            ////    return "EventRou";

            //if (v2Enum_RankType == V2Enum_RankType.EventGod)
            //    RankName = string.Format("{0}_{1}_{2}", v2Enum_RankType.ToString(), PlayerDataContainer.PlayerServerKind, PlayerDataContainer.PlayerServerNum);
            //else
            //{
            //    if (PlayerDataContainer.PlayerServerNum >= 107)
            //        RankName = string.Format("{0}_{1}_{2}", v2Enum_RankType.ToString(), PlayerDataContainer.PlayerServerKind, PlayerDataContainer.PlayerServerNum);
            //    else
            //        RankName = string.Format("{0}_GT1", v2Enum_RankType.ToString());

            //    //switch (Managers.SceneManager.Instance.BuildElement)
            //    //{
            //    //    case BuildEnvironmentEnum.Product:
            //    //        {
            //    //            RankName = string.Format("{0}_{1}_{2}", v2Enum_RankType.ToString(), PlayerDataContainer.PlayerServerKind, PlayerDataContainer.PlayerServerNum);
            //    //            break;
            //    //        }
            //    //    default:
            //    //        {
            //    //            RankName = string.Format("{0}_{1}_{2}", v2Enum_RankType.ToString(), PlayerDataContainer.PlayerServerKind, PlayerDataContainer.PlayerServerNum);
            //    //            break;
            //    //        }
            //    //}
            //}
            

            return v2Enum_RankType.ToString();
        }
        //------------------------------------------------------------------------------------
        public static void GetMyRank(V2Enum_RankType v2Enum_RankType, System.Action action = null)
        {
            if (RankContainer.rankTableData.ContainsKey(v2Enum_RankType) == false)
                return;

            RankTable rankTable = RankContainer.rankTableData[v2Enum_RankType];

            Backend.Leaderboard.User.GetMyLeaderboard(rankTable.uuid, callback => 
            {
                if (callback.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                    return;
                }

                Debug.Log("리더보드 총 유저 등록 수 : " + callback.GetTotalCount());

                rankTable.TotalCount = callback.GetTotalCount();

                foreach (BackEnd.Leaderboard.UserLeaderboardItem item in callback.GetUserLeaderboardList())
                {
                    Debug.Log($"{item.rank}위({item.score}) : {item.nickname}");
                    Debug.Log(item.ToString());

                    rankTable.MyRank = item.rank.ToInt();
                    rankTable.Score = item.score.ToDouble();


                    if (v2Enum_RankType == V2Enum_RankType.Power)
                        RankContainer.MyRankInfo.CombatPower = rankTable.Score;
                    else if (v2Enum_RankType == V2Enum_RankType.Stage)
                        RankContainer.MyRankInfo.Stage = rankTable.Score.ToInt();
                }

                action?.Invoke();
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdateUserScore(V2Enum_RankType v2Enum_RankType, double score, string detail, System.Action action = null)
        {
            if (RankContainer.rankTableData.ContainsKey(v2Enum_RankType) == false)
            {
                action?.Invoke();
                return;
            }

            RankTable rankTable = RankContainer.rankTableData[v2Enum_RankType];

            Param param = new Param();
            param.Add(rankTable.column, score);
            param.Add("Detail", detail);

            Backend.Leaderboard.User.UpdateMyDataAndRefreshLeaderboard(rankTable.uuid, rankTable.table, TheBackEnd_PlayerTable.GetInData(Define.PlayerRankInfoTable), param, callback =>
            {
                if (callback.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                    return;
                }

                action?.Invoke();
            });

            //Backend.URank.User.UpdateUserScore(rankTable.uuid, rankTable.table, TheBackEnd_PlayerTable.GetInData(Define.PlayerRankInfoTable), param, callback =>
            //{
            //    if (callback.IsSuccess() == false)
            //    {
            //        TheBackEndManager.Instance.BackEndErrorCode(callback);
            //        return;
            //    }

            //    action?.Invoke();
            //});
        }
        //------------------------------------------------------------------------------------
        public static void GetRankList(V2Enum_RankType v2Enum_RankType, System.Action action = null)
        {
            if (RankContainer.rankTableData.ContainsKey(v2Enum_RankType) == false)
                return;

            RankTable rankTable = RankContainer.rankTableData[v2Enum_RankType];

            Backend.Leaderboard.User.GetLeaderboard(rankTable.uuid, 100, callback =>
            {
                if (callback.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                    return;
                }

                bool refreshedUserInfo = false;

                int rankcount = 0;

                foreach (BackEnd.Leaderboard.UserLeaderboardItem item in callback.GetUserLeaderboardList())
                {
                    RankerData rankData = null;

                    if (RankContainer.RankerDatas.Count <= rankcount)
                    {
                        rankData = new RankerData();
                        RankContainer.RankerDatas.Add(rankData);
                    }
                    else
                    {
                        rankData = RankContainer.RankerDatas[rankcount];
                    }

                    rankData.v2Enum_RankType = v2Enum_RankType;
                    rankData.nickName = item.nickname;
                    rankData.score = item.score.ToDouble();
                    rankData.rank = item.rank.ToInt();
                    //rankData.detail = rows[i]["Detail"]["S"].ToString();

                    if (TheBackEndManager.Instance.GetNickPlayerName() == rankData.nickName)
                    {
                        refreshedUserInfo = true;
                        rankTable.MyRank = rankData.rank;
                        rankTable.Score = rankData.score;

                        if (v2Enum_RankType == V2Enum_RankType.Power)
                            RankContainer.MyRankInfo.CombatPower = rankTable.Score;
                        else if (v2Enum_RankType == V2Enum_RankType.Stage)
                            RankContainer.MyRankInfo.Stage = rankTable.Score.ToInt();
                    }

                    rankcount++;
                }

                for (int i = callback.GetUserLeaderboardList().Count; i < RankContainer.RankerDatas.Count; ++i)
                {
                    RankerData rankData = RankContainer.RankerDatas[i];
                    rankData.rank = -1;
                }

                if (refreshedUserInfo == false)
                    GetMyRank(v2Enum_RankType, action);
                else
                    action?.Invoke();
            });
        }
        //------------------------------------------------------------------------------------
        public static void GetRankList(V2Enum_RankType v2Enum_RankType, int limit, int offset, System.Action<List<BackEnd.Leaderboard.UserLeaderboardItem>> action = null)
        {
            if (RankContainer.rankTableData.ContainsKey(v2Enum_RankType) == false)
                return;

            RankTable rankTable = RankContainer.rankTableData[v2Enum_RankType];

            Backend.Leaderboard.User.GetLeaderboard(rankTable.uuid, limit, offset, callback =>
            {
                if (callback.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);

                    action?.Invoke(null);
                    action = null;
                    return;
                }

                action?.Invoke(callback.GetUserLeaderboardList());
            });
        }
        //------------------------------------------------------------------------------------
    }
}