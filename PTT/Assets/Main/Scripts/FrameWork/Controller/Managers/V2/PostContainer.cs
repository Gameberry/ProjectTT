using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using BackEnd;

namespace GameBerry
{
    public class PostInfo
    {
        public string InData; // key°¡ µÈ´Ù.

        public PostType PostType;

        public bool IsShop = false;

        public string Title;
        public string Content;
        public string Author;
        public DateTime SentDate;
        public double SentDate_TimeStamp;
        public DateTime ExpirationDate;
        public List<RewardData> RewardData;
    }

    public static class PostContainer
    {
        public static Dictionary<string, PostInfo> m_myPostInfo = new Dictionary<string, PostInfo>();
    }

    public static class PostOperator
    {
        public static Queue<PostInfo> m_postInfoPool = new Queue<PostInfo>();
        
        //------------------------------------------------------------------------------------
        public static PostInfo GetPostInfoObj()
        {
            if (m_postInfoPool.Count > 0)
                return m_postInfoPool.Dequeue();

            return new PostInfo();
        }
        //------------------------------------------------------------------------------------
        public static void PoolPostInfoObj(PostInfo postInfo)
        {
            if (postInfo.RewardData != null && postInfo.RewardData.Count > 0)
            {
                for (int i = 0; i < postInfo.RewardData.Count; ++i)
                {
                    RewardData rewardData = postInfo.RewardData[i];

                    Managers.RewardManager.Instance.PoolRewardData(rewardData);
                }

                postInfo.RewardData.Clear();
            }

            m_postInfoPool.Enqueue(postInfo);
        }
        //------------------------------------------------------------------------------------
    }
}