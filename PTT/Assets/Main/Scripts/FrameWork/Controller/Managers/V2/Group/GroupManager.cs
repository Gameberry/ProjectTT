using System.Collections.Generic;
using UnityEngine;
using GameBerry;
using BackndChat;
using System.Text.RegularExpressions;
using GameBerry.TheBackEnd;
using BackEnd;
using Cysharp.Threading.Tasks;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Linq;

namespace GameBerry.Managers
{
    public class GroupManager : MonoSingleton<GroupManager>
    {
        public void InitGroup(System.Action complete)
        {
            TheBackEnd_Group.GetMyGroup(o =>
            {
                if (o == null)
                {
                    TheBackEnd.TheBackEnd_Group.GetGroupList(o =>
                    {
                        List<BackEnd.Group.GroupItem> list = o.GetGroupList();

                        if (list == null)
                            return;

                        if (list.Count > 0)
                        {
                            BackEnd.Group.GroupItem groupItem = list[0];
                            TheBackEnd_Group.ChangeGroup(groupItem.groupUuid, groupItem.groupName, change =>
                            {
                                if (change.IsSuccess())
                                    complete.Invoke();
                            });
                        }
                    });
                }
                else
                    complete.Invoke();
            });
        }
    }
}
