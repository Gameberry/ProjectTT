using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;

namespace GameBerry.TheBackEnd
{
    public static class TheBackEnd_PlayerTable
    {
        private static Dictionary<string, string> InDatas = new Dictionary<string, string>();


        //------------------------------------------------------------------------------------
        public static void GetTableList()
        {
            SendQueue.Enqueue(Backend.GameData.GetTableList, (callback) =>
            {
                // 이후 처리
                Debug.Log(callback.GetReturnValue());

                if (callback.IsSuccess() == true)
                {
                    var data = callback.GetReturnValuetoJSON();
                }
                else
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
            });
        }
        //------------------------------------------------------------------------------------
        private static void AddInData(string tableName, string InData)
        {
            if (InDatas.ContainsKey(tableName) == false)
                InDatas.Add(tableName, InData);
        }
        //------------------------------------------------------------------------------------
        public static string GetInData(string tableName)
        {
            if (InDatas.ContainsKey(tableName) == false)
                return string.Empty;

            return InDatas[tableName];
        }
        //------------------------------------------------------------------------------------
        public static void AllUpdata()
        {
            
        }
        //------------------------------------------------------------------------------------
        public static void UpdateTable(string tableName, string inDate, string owner_inDate, Param param, System.Action<BackendReturnObject> action = null)
        {
            SendQueue.Enqueue(Backend.GameData.UpdateV2, tableName, inDate, owner_inDate, param, (callback) =>
            {
                if (callback.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
                else
                {
                    //if (ThirdPartyLog.isAlive == true)
                    //    ThirdPartyLog.Instance.SendLog_InGame(tableName, param.GetJson());

                    action?.Invoke(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void DeleteAllTable()
        {
            foreach (var pair in InDatas)
            {
                DeleteTable(pair.Key);
            }

            PlayerPrefs.DeleteAll();
        }
        //------------------------------------------------------------------------------------
        public static void DeleteTable(string tableName)
        {
            SendQueue.Enqueue(Backend.GameData.DeleteV2, tableName, GetInData(tableName), Backend.UserInDate, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                Debug.LogWarning(string.Format("{0} 테이블 삭제 완료", tableName));
            });
        }
        //------------------------------------------------------------------------------------
        public static void GetData(string tableName, string userindata, System.Action<JsonData> action = null)
        {
            Where where = new Where();
            where.Equal("owner_inDate", userindata);

            SendQueue.Enqueue(Backend.GameData.Get, tableName, where, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);

                    action?.Invoke(null);
                    action = null;
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    action?.Invoke(null);
                    action = null;
                }
                else
                {
                    action?.Invoke(data);
                    action = null;
                }
                
            });
        }
        //------------------------------------------------------------------------------------
        public static void GetData_NickName(string tableName, string nickName, System.Action<JsonData> action = null)
        {
            SendQueue.Enqueue(Backend.Social.GetUserInfoByNickName, nickName, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);

                    action?.Invoke(null);
                    action = null;
                    return;
                }

                GetData(tableName, bro.GetReturnValuetoJSON()["row"]["inDate"].ToString(), action);
            });
        }
        //------------------------------------------------------------------------------------
    }
}