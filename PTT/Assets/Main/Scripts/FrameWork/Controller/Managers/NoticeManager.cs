using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using LitJson;

namespace GameBerry.Managers
{
    public class NoticeData
    {
        public string title;
        public string contents;
        public DateTime postingDate;
        //public string imageKey;
        public string inDate;
        public string uuid;
        //public string linkUrl;
        public bool isPublic;
        //public string linkButtonName;
        public string author;

        public override string ToString()
        {
            return $"title : {title}\n" +
            $"contents : {contents}\n" +
            $"postingDate : {postingDate}\n" +
            //$"imageKey : {imageKey}\n" +
            $"inDate : {inDate}\n" +
            $"uuid : {uuid}\n" +
            //$"linkUrl : {linkUrl}\n" +
            $"isPublic : {isPublic}\n" +
            //$"linkButtonName : {linkButtonName}\n" +
            $"author : {author}\n";
        }
    }

    public class NoticeManager : MonoSingleton<NoticeManager>
    {
        public List<NoticeData> noticeList = new List<NoticeData>();

        //private GameBerry.Event.RefreshNoticeMsg refreshNotice = new Event.RefreshNoticeMsg();

        //------------------------------------------------------------------------------------
        public void InitNoticeContent()
        {
            RefreshNotice();
        }
        //------------------------------------------------------------------------------------
        public void RefreshNotice()
        {
            SendQueue.Enqueue(Backend.Notice.NoticeList, 10, callback =>
            {
                if (callback.IsSuccess() == false)
                {
                    TheBackEnd.TheBackEndManager.Instance.BackEndErrorCode(callback);
                    return;
                }

                noticeList.Clear();

                LitJson.JsonData jsonList = callback.FlattenRows();
                for (int i = 0; i < jsonList.Count; i++)
                {
                    NoticeData notice = new NoticeData();

                    notice.title = jsonList[i]["title"].ToString();
                    notice.contents = jsonList[i]["content"].ToString();

                    string time = jsonList[i]["postingDate"].ToString();

                    notice.postingDate = DateTime.Parse(jsonList[i]["postingDate"].ToString());
                    notice.inDate = jsonList[i]["inDate"].ToString();
                    notice.uuid = jsonList[i]["uuid"].ToString();
                    notice.isPublic = jsonList[i]["isPublic"].ToString() == "y" ? true : false;
                    notice.author = jsonList[i]["author"].ToString();

                    noticeList.Add(notice);
                }

                //if (noticeList.Count > 0)
                //{
                //    string index = PlayerPrefs.GetString("LastNotice", string.Empty);

                //    if (string.IsNullOrEmpty(index))
                //    { 
                //        RedDotManager.Instance.ShowRedDot(ContentDetailList.Notice);
                //        PlayerPrefs.SetString("LastNotice", noticeList[0].inDate);
                //    }
                //    if (index != noticeList[0].inDate)
                //    { 
                //        RedDotManager.Instance.ShowRedDot(ContentDetailList.Notice);
                //        PlayerPrefs.SetString("LastNotice", noticeList[0].inDate);
                //    }
                //}

                //Message.Send(refreshNotice);
            });
        }
        //------------------------------------------------------------------------------------
    }
}