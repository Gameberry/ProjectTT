using System;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using LitJson;

namespace GameBerry.TheBackEnd
{
    public class TheBackEnd_Utils
    {
        //------------------------------------------------------------------------------------
        public static void GetServerTime(Action<DateTime> action)
        {
            if (action == null)
                return;

            SendQueue.Enqueue(Backend.Utils.GetServerTime, (callback) =>
            {
                if(callback.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                    return;
                }

                string time = callback.GetReturnValuetoJSON()["utcTime"].ToString();
                DateTime parsedDate = DateTime.Parse(time);
                parsedDate = parsedDate.ToUniversalTime();

                action?.Invoke(parsedDate);
            });
        }
        //------------------------------------------------------------------------------------
        public static void GetPostList(PostType postType, Backend.BackendCallback action)
        {
            SendQueue.Enqueue(Backend.UPost.GetPostList, postType, 10, action);
        }
        //------------------------------------------------------------------------------------
        public static void ReceivePostItem(PostType postType, string indata, Backend.BackendCallback action)
        {
            SendQueue.Enqueue(Backend.UPost.ReceivePostItem, postType, indata, action);
        }
        //------------------------------------------------------------------------------------
        public static void ReceivePostItemAll(PostType postType, Backend.BackendCallback action)
        {
            SendQueue.Enqueue(Backend.UPost.ReceivePostItemAll, postType, action);
        }
        //------------------------------------------------------------------------------------
        public static void ReceiveCoupon(string coupon, Backend.BackendCallback action)
        {
            SendQueue.Enqueue(Backend.Coupon.UseCoupon, coupon, action);
        }
        //------------------------------------------------------------------------------------
    }
}