using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd.Socketio;
using LitJson;
using BackEnd;

namespace GameBerry.Managers
{
    public class PostManager : MonoSingleton<PostManager>
    {
        public bool m_needRefreshPostList = false;
        private Event.RefreshPostListMsg m_refreshPostListMsg = new Event.RefreshPostListMsg();
        private Event.SetInGameRewardPopupMsg m_setInGameRewardPopupMsg = new Event.SetInGameRewardPopupMsg();

        private bool m_waitAdminPostRefresh = false;
        private bool m_waitRankPostRefresh = false;

        //------------------------------------------------------------------------------------
        public void InitPostContent()
        {
            RefreshAdminPost();
        }
        //------------------------------------------------------------------------------------
        public void OnNewPostCreated(PostRepeatType postRepeatType, string title, string content, string author)
        {
            m_needRefreshPostList = true;
            RedDotManager.Instance.ShowRedDot(ContentDetailList.Post);
            RedDotManager.Instance.ShowRedDot(ContentDetailList.PostGeneral);
            Debug.Log(
                            $"[OnNewPostCreated(새로운 우편 생성)]\n" +
                            $"| postRepeatType : {postRepeatType}\n" +
                            $"| title : {title}\n" +
                            $"| content : {content}\n" +
                            $"| author : {author}\n"
                        );
        }
        //------------------------------------------------------------------------------------
        public Dictionary<string, PostInfo> GetAllPostInfos()
        {
            return PostContainer.m_myPostInfo;
        }
        //------------------------------------------------------------------------------------
        public PostInfo GetPostInfo(string indata)
        {
            if (PostContainer.m_myPostInfo.ContainsKey(indata) == true)
            {
                return PostContainer.m_myPostInfo[indata];
            }

            return null;
        }
        //------------------------------------------------------------------------------------
        public bool NeedRefreshPost()
        {
            return m_needRefreshPostList;
        }
        //------------------------------------------------------------------------------------
        public void RefreshAdminPost()
        {
            TheBackEnd.TheBackEndManager.Instance.GetPostList(BackEnd.PostType.Admin, RefreshAdminPostInfo);
            m_waitAdminPostRefresh = true;

            TheBackEnd.TheBackEndManager.Instance.GetPostList(BackEnd.PostType.Rank, RefreshRankPostInfo);
            m_waitRankPostRefresh = true;
        }
        //------------------------------------------------------------------------------------
        public void OnRefreshComplete()
        {
            if (m_waitAdminPostRefresh == false && m_waitRankPostRefresh == false)
            {
                m_needRefreshPostList = false;
                Message.Send(m_refreshPostListMsg);
            }
        }
        //------------------------------------------------------------------------------------
        public void RefreshAdminPostInfo(BackendReturnObject backendReturnObject)
        {
            RefreshPostInfo(backendReturnObject, PostType.Admin);
            m_waitAdminPostRefresh = false;

            OnRefreshComplete();
        }
        //------------------------------------------------------------------------------------
        public void RefreshRankPostInfo(BackendReturnObject backendReturnObject)
        {
            RefreshPostInfo(backendReturnObject, PostType.Rank);
            m_waitRankPostRefresh = false;

            OnRefreshComplete();
        }
        //------------------------------------------------------------------------------------
        public void RefreshPostInfo(BackendReturnObject backendReturnObject, PostType postType)
        {
            JsonData json = backendReturnObject.GetReturnValuetoJSON()["postList"];

            for (int i = 0; i < json.Count; i++)
            {
                try
                {
                    string indata = json[i]["inDate"].ToString();

                    if (PostContainer.m_myPostInfo.ContainsKey(indata) == true)
                        continue;

                    PostInfo postInfo = PostOperator.GetPostInfoObj();

                    postInfo.InData = indata;

                    postInfo.PostType = postType;
                    //common/ui/eventReward

                    if (postType == PostType.Rank)
                    { // 여기서 랭킹 보상 로컬 적용하기 json[i]["title"].ToString() AllyArena_GO_105 qweewq (2 - 2)
                        string originTitle = json[i]["title"].ToString();

                        if (originTitle.Contains(V2Enum_RankType.Stage.ToString()))
                        {
                            postInfo.Title = "rankReward/stage/title";
                            postInfo.Content = "rankReward/stage/desc";
                        }
                        else if (originTitle.Contains(V2Enum_RankType.Power.ToString()))
                        {
                            postInfo.Title = "rankReward/combatPower/title";
                            postInfo.Content = "rankReward/combatPower/desc";
                        }
                        else
                        {
                            postInfo.Title = "common/ui/eventReward";
                            postInfo.Content = "common/ui/eventReward";
                        }
                    }
                    else
                    {
                        postInfo.Title = json[i]["title"].ToString();
                        postInfo.Content = json[i]["content"].ToString();
                    }
                    
                    //postInfo.Author = json[i]["author"].ToString();

                    postInfo.SentDate = DateTime.Parse(json[i]["sentDate"].ToString());
                    postInfo.SentDate_TimeStamp = ((DateTimeOffset)postInfo.SentDate).ToUnixTimeSeconds();
                    postInfo.ExpirationDate = DateTime.Parse(json[i]["expirationDate"].ToString());

                    if (json[i].ContainsKey("items") == true)
                    {
                        if (json[i]["items"].Count > 0)
                        {
                            if (postInfo.RewardData == null)
                                postInfo.RewardData = new List<RewardData>();

                            for (int j = 0; j < json[i]["items"].Count; ++j)
                            {
                                RewardData rewardData = RewardManager.Instance.GetRewardData();
                                rewardData.Amount = json[i]["items"][j]["itemCount"].ToString().ToDouble();
                                rewardData.V2Enum_Goods = json[i]["items"][j]["item"]["GoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                                rewardData.Index = json[i]["items"][j]["item"]["GoodsIndex"].ToString().ToInt();

                                postInfo.RewardData.Add(rewardData);
                            }
                        }
                    }

                    RedDotManager.Instance.ShowRedDot(ContentDetailList.Post);
                    RedDotManager.Instance.ShowRedDot(ContentDetailList.PostGeneral);

                    PostContainer.m_myPostInfo.Add(indata, postInfo);
                }
                catch(Exception e)
                {
                    Debug.LogError(e);
                }
            }

        }
        //------------------------------------------------------------------------------------
        private void OnRewardComplete()
        {
            if (m_setInGameRewardPopupMsg.RewardDatas.Count > 0)
            {
                Message.Send(m_setInGameRewardPopupMsg);
                UI.UIManager.DialogEnter<UI.InGameRewardPopupDialog>();
            }

            TheBackEnd.TheBackEndManager.Instance.SendUpdateWaitData(true);

            OnRefreshComplete();
        }
        //------------------------------------------------------------------------------------
        public void ReceivePostItem(string indata)
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            PostInfo postInfo = GetPostInfo(indata);

            if (postInfo == null)
                return;

            m_setInGameRewardPopupMsg.RewardDatas.Clear();

            TheBackEnd.TheBackEndManager.Instance.ReceivePostItem(postInfo.PostType, postInfo.InData, RecvPostReward);

            PostContainer.m_myPostInfo.Remove(postInfo.InData);
            PostOperator.PoolPostInfoObj(postInfo);
        }
        //------------------------------------------------------------------------------------
        public void RecvPostReward(BackendReturnObject backendReturnObject)
        {
            if (backendReturnObject.IsSuccess() == false || backendReturnObject.GetReturnValuetoJSON().ContainsKey("postItems") == false)
                return;

            JsonData json = backendReturnObject.GetReturnValuetoJSON()["postItems"];

            List<int> reward_type = new List<int>();
            List<double> before_quan = new List<double>();
            List<double> reward_quan = new List<double>();
            List<double> after_quan = new List<double>();

            for (int i = 0; i < json.Count; i++)
            {
                try
                {
                    RewardData rewardData = RewardManager.Instance.GetRewardData();
                    rewardData.Amount = json[i]["itemCount"].ToString().ToDouble();
                    rewardData.V2Enum_Goods = json[i]["item"]["GoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                    rewardData.Index = json[i]["item"]["GoodsIndex"].ToString().ToInt();

                    reward_type.Add(rewardData.Index);
                    before_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));
                    reward_quan.Add(rewardData.Amount);

                    GoodsManager.Instance.AddGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index, rewardData.Amount);

                    after_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));

                    m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            ThirdPartyLog.Instance.SendLog_MailEvent(1, string.Empty, reward_type, before_quan, reward_quan, after_quan);

            OnRewardComplete();
        }
        //------------------------------------------------------------------------------------
        public void ReceiveAllPostItem()
        {
            if (TheBackEnd.TheBackEndManager.Instance.CheckNetworkState() == false)
                return;

            m_setInGameRewardPopupMsg.RewardDatas.Clear();

            StartCoroutine(ReceiveAllPostItemCoroutine());

            foreach (KeyValuePair<string, PostInfo> pair in PostContainer.m_myPostInfo)
            {
                PostOperator.PoolPostInfoObj(pair.Value);
            }

            PostContainer.m_myPostInfo.Clear();
        }
        //------------------------------------------------------------------------------------
        public IEnumerator ReceiveAllPostItemCoroutine()
        {
            bool recvAdminReward = false;
            bool recvRankReward = false;

            m_setInGameRewardPopupMsg.RewardDatas.Clear();

            TheBackEnd.TheBackEndManager.Instance.ReceivePostItemAll(BackEnd.PostType.Admin, o =>
            {
                recvAdminReward = true;
                RecvAllPostReward(o);
            });

            TheBackEnd.TheBackEndManager.Instance.ReceivePostItemAll(BackEnd.PostType.Rank, o =>
            {
                recvRankReward = true;
                RecvAllPostReward(o);
            });

            while (recvAdminReward == false || recvRankReward == false)
            {
                yield return null;
            }

            OnRewardComplete();
        }
        //------------------------------------------------------------------------------------
        public void RecvAllPostReward(BackendReturnObject backendReturnObject)
        {
            if (backendReturnObject.IsSuccess() == false || backendReturnObject.GetReturnValuetoJSON().ContainsKey("postItems") == false)
                return;

            JsonData json = backendReturnObject.GetReturnValuetoJSON()["postItems"];

            List<int> reward_type = new List<int>();
            List<double> before_quan = new List<double>();
            List<double> reward_quan = new List<double>();
            List<double> after_quan = new List<double>();

            for (int i = 0; i < json.Count; i++)
            {
                try
                {
                    for (int j = 0; j < json[i].Count; ++j)
                    {
                        RewardData rewardData = RewardManager.Instance.GetRewardData();
                        rewardData.Amount = json[i][j]["itemCount"].ToString().ToDouble();
                        rewardData.V2Enum_Goods = json[i][j]["item"]["GoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                        rewardData.Index = json[i][j]["item"]["GoodsIndex"].ToString().ToInt();

                        reward_type.Add(rewardData.Index);
                        before_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));
                        reward_quan.Add(rewardData.Amount);

                        GoodsManager.Instance.AddGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index, rewardData.Amount);

                        after_quan.Add(GoodsManager.Instance.GetGoodsAmount(rewardData.V2Enum_Goods.Enum32ToInt(), rewardData.Index));

                        m_setInGameRewardPopupMsg.RewardDatas.Add(rewardData);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            ThirdPartyLog.Instance.SendLog_MailEvent(1, string.Empty, reward_type, before_quan, reward_quan, after_quan);
        }
        //------------------------------------------------------------------------------------

    }
}