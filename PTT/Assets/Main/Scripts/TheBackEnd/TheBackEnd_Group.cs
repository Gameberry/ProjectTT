using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using LitJson;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.TheBackEnd
{
    public static class TheBackEnd_Group
    {
        public static void GetGroupList(System.Action<BackEnd.Group.BackendGroupReturnObject> action)
        {
            Backend.Group.Table.Get(callback => {
                if (callback.IsSuccess())
                    action?.Invoke(callback);
                else
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
            });
        }

        public static void GetMyGroup(System.Action<BackEnd.Group.BackendMyGroupReturnObject> action)
        {
            Backend.Group.Get(callback => {
                if (callback.IsSuccess())
                    action?.Invoke(callback);
                else
                {
                    action?.Invoke(null);
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }

        public static void ChangeGroup(string groupUid, string groupName, System.Action<BackendReturnObject> action)
        {
            Backend.Group.Update(groupUid, groupName, callback => {
                action?.Invoke(callback);
            });
        }
    }
}