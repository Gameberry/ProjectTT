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

                    //for (int i = 0; i < data["tables"].Count; ++i)
                    //{
                    //    string returnValue = string.Empty;
                    //    foreach (var key in data["tables"][i].Keys)
                    //    {
                    //        returnValue += string.Format("{0} : {1} / ", key, data["tables"][i][key].ToString());
                    //    }
                    //    Debug.Log(returnValue);
                    //}
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
            UpdatePlayerInfoTable();

            UpdatePlayerARRRInfoTable();
            UpdatePlayerStaminaInfoTable();
            UpdatePlayerJobInfoTable();
            UpdatePlayerVipPackageInfoTable();
            UpdatePlayerSynergyInfoTable();
            UpdatePlayerDescendInfoTable();
            UpdatePlayerSynergyRuneInfoTable();
            UpdatePlayerRelicInfoTable();

            UpdatePlayerPointTable();
            UpdatePlayerSummonTicketTable();
            UpdatePlayerBoxTable();

            UpdatePlayerTrainingTable();
            UpdatePlayerGearInfoTable();
            UpdatePlayerSkinInfoTable();
            UpdatePlayerSkillInfoTable();
            
            UpdatePlayerResearchInfoTable();

            UpdatePlayerSummonInfoTable();
            UpdatePlayerTimeInfoTable();
            UpdatePlayerAdBuffInfoTable();

            UpdatePlayerCheckInInfoTable(null);
            UpdatePlayerShopInfoTable(null);


            UpdatePlayerPassInfoTable();
            UpdatePlayerQuestInfoTable();
            UpdatePlayerExchangeInfoTable();
            UpdatePlayerRankInfoTable();
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
        #region TheBackEnd_PlayerInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerInfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerInfoTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerInfoTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerAdFree)
                            {
                                Define.IsAdFree = data[i][key].ToString().ToInt() == 1;
                            }
                            else if (key == Define.PlayerCheatingCheck)
                            {
                                PlayerDataContainer.PlayerCheatingCount = data[i][key].ToString().ToInt();
                            }
                            else if (key == Define.PlayerDontSearchCheat)
                            {
                                PlayerDataContainer.PlayerDontSearchCheat = data[i][key].ToString().ToInt();
                            }
                            else if (key == Define.PlayerProfile)
                            {
                                PlayerDataContainer.Profile = data[i][key].ToString().ToInt();
                            }

                            else if (key == Define.PlayerRecvLaunchReward)
                            {
                                PlayerDataContainer.PlayerRecvLaunchReward = data[i][key].ToString().ToInt();
                            }
                            else if (key == Define.PlayerRecvPreReward)
                            {
                                PlayerDataContainer.PlayerRecvPreReward = data[i][key].ToString().ToInt();
                            }
                        }
                    }


                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerInfoTable()
        {
            Debug.Log("InsertPlayerInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerInfoTable, GetPlayerInfoParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerInfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerInfoTable()
        {
            UpdateTable(Define.PlayerInfoTable, GetInData(Define.PlayerInfoTable), Backend.UserInDate, GetPlayerInfoParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerInfoParam()
        {
            Param param = new Param();

            param.Add(Define.PlayerAdFree, Define.IsAdFree.GetDecrypted() == true ? 1 : 0);

            param.Add(Define.PlayerCheatingCheck, PlayerDataContainer.PlayerCheatingCount.GetDecrypted());
            param.Add(Define.PlayerDontSearchCheat, PlayerDataContainer.PlayerDontSearchCheat.GetDecrypted());

            param.Add(Define.PlayerProfile, PlayerDataContainer.Profile);

            param.Add(Define.PlayerRecvLaunchReward, PlayerDataContainer.PlayerRecvLaunchReward.GetDecrypted());
            param.Add(Define.PlayerRecvPreReward, PlayerDataContainer.PlayerRecvPreReward.GetDecrypted());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerARRRInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerARRRInfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerARRRInfoTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerARRRInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerARRRInfoTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerARRRLevel)
                            {
                                ARRRContainer.ARRRLevel = data[i][key].ToString().ToInt();
                            }
                            else if (key == Define.PlayerARRRLimitCompleteLevel)
                            {
                                ARRRContainer.ARRRLimitCompleteLevel = data[i][key].ToString().ToInt();
                            }
                            else if (key == Define.PlayerARRRDefaultGoods)
                            {
                                ARRRContainer.RecvDefaultGoods = data[i][key].ToString().ToInt();
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerARRRInfoTable()
        {
            Debug.Log("InsertPlayerARRRInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerARRRInfoTable, GetPlayerARRRInfoParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerARRRInfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerARRRInfoTable()
        {
            UpdateTable(Define.PlayerARRRInfoTable, GetInData(Define.PlayerARRRInfoTable), Backend.UserInDate, GetPlayerARRRInfoParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerARRRInfoParam()
        {
            Param param = new Param();

            param.Add(Define.PlayerARRRLevel, ARRRContainer.ARRRLevel.GetDecrypted());
            param.Add(Define.PlayerARRRLimitCompleteLevel, ARRRContainer.ARRRLimitCompleteLevel.GetDecrypted());
            param.Add(Define.PlayerARRRDefaultGoods, ARRRContainer.RecvDefaultGoods.GetDecrypted());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerDungeonInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerDungeonInfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerDungeonInfoTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerDungeonInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerDungeonInfoTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerDiamondDungeonMaxClear)
                            {
                                DungeonDataContainer.Diamond_Dungeon_MaxClear = data[i][key].ToString().ToInt();
                            }
                            else if (key == Define.PlayerTowerDungeonMaxClear)
                            {
                                DungeonDataContainer.Tower_Dungeon_MaxClear = data[i][key].ToString().ToInt();
                            }
                            else if (key == Define.PlayerDungeonInitInfo)
                            {
                                DungeonDataContainer.SetDungeonInfoDeSerializeString(data[i][key].ToString());
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerDungeonInfoTable()
        {
            Debug.Log("InsertPlayerDungeonInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerDungeonInfoTable, GetPlayerDungeonParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerDungeonInfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerDungeonInfoTable()
        {
            UpdateTable(Define.PlayerDungeonInfoTable, GetInData(Define.PlayerDungeonInfoTable), Backend.UserInDate, GetPlayerDungeonParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerDungeonParam()
        {
            Param param = new Param();
            param.Add(Define.PlayerDiamondDungeonMaxClear, DungeonDataContainer.Diamond_Dungeon_MaxClear.GetDecrypted());
            param.Add(Define.PlayerTowerDungeonMaxClear, DungeonDataContainer.Tower_Dungeon_MaxClear.GetDecrypted());

            param.Add(Define.PlayerDungeonInitInfo, DungeonDataContainer.GetDungeonInfoSerializeString());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerStaminaInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerStaminaInfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerStaminaInfoTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerStaminaInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerStaminaInfoTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerStaminaInitTime)
                            {
                                StaminaContainer.EventDayInitTime = data[i][key].ToString().ToDouble();
                            }
                            else if (key == Define.PlayerStaminaAccumUse)
                            {
                                StaminaContainer.StaminaAccumUse = data[i][key].ToString().ToInt();
                            }
                            else if (key == Define.PlayerStaminaLastChargeTime)
                            {
                                StaminaContainer.StaminaLastChargeTime = data[i][key].ToString().ToDouble();
                            }
                            else if (key == Define.PlayerToDayDigAdCount)
                            {
                                StaminaContainer.ToDayDigAdCount = data[i][key].ToString().ToInt();
                            }
                            else if (key == Define.PlayerToDayDigDiaBuyCount)
                            {
                                StaminaContainer.ToDayDigDiaBuyCount = data[i][key].ToString().ToInt();
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerStaminaInfoTable()
        {
            Debug.Log("InsertPlayerStaminaInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerStaminaInfoTable, GetPlayerStaminaInfoParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerStaminaInfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerStaminaInfoTable()
        {
            UpdateTable(Define.PlayerStaminaInfoTable, GetInData(Define.PlayerStaminaInfoTable), Backend.UserInDate, GetPlayerStaminaInfoParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerStaminaInfoParam()
        {
            Param param = new Param();

            param.Add(Define.PlayerStaminaInitTime, StaminaContainer.EventDayInitTime.GetDecrypted());
            param.Add(Define.PlayerStaminaAccumUse, StaminaContainer.StaminaAccumUse.GetDecrypted());
            param.Add(Define.PlayerStaminaLastChargeTime, StaminaContainer.StaminaLastChargeTime.GetDecrypted());
            param.Add(Define.PlayerToDayDigAdCount, StaminaContainer.ToDayDigAdCount.GetDecrypted());
            param.Add(Define.PlayerToDayDigDiaBuyCount, StaminaContainer.ToDayDigDiaBuyCount.GetDecrypted());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerJobInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerJobInfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerJobInfoTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerJobInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerJobInfoTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerJobInfo)
                            {
                                JobContainer.SetSynergyInfoDeSerializeString(data[i][key].ToString());
                            }
                            else if (key == Define.PlayerJobType)
                            {
                                JobContainer.JobType = data[i][key].ToString().ToInt();
                            }
                            else if (key == Define.PlayerJobTier)
                            {
                                JobContainer.JobTier = data[i][key].ToString().ToInt();
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerJobInfoTable()
        {
            Debug.Log("InsertPlayerJobInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerJobInfoTable, GetPlayerJobInfoParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerJobInfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerJobInfoTable()
        {
            UpdateTable(Define.PlayerJobInfoTable, GetInData(Define.PlayerJobInfoTable), Backend.UserInDate, GetPlayerJobInfoParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerJobInfoParam()
        {
            Param param = new Param();

            param.Add(Define.PlayerJobInfo, JobContainer.GetSynergyInfoSerializeString());
            param.Add(Define.PlayerJobType, JobContainer.JobType.GetDecrypted());
            param.Add(Define.PlayerJobTier, JobContainer.JobTier.GetDecrypted());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerStaminaInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerVipPackageInfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerVipPackageInfoTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerVipPackageInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerVipPackageInfoTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerVipPackageInfo)
                            {
                                string str = data[i][key].ToString();

                                try
                                {
                                    VipPackageContainer.VipPackageInfo.Clear();
                                    VipPackageContainer.SetDeSerializeString(data[i][key].ToString());

                                }
                                catch (System.Exception ex)
                                {
                                    Debug.Log("IsError" + ex.ToString());
                                }
                            }
                            else if (key == Define.PlayerVipPackageShopInfo)
                            {
                                string str = data[i][key].ToString();

                                try
                                {
                                    VipPackageContainer.VipPackageShopInfo.Clear();
                                    VipPackageContainer.SetShopDeSerializeString(data[i][key].ToString());

                                }
                                catch (System.Exception ex)
                                {
                                    Debug.Log("IsError" + ex.ToString());
                                }
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerVipPackageInfoTable()
        {
            Debug.Log("InsertPlayerVipPackageInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerVipPackageInfoTable, GetPlayerVipPackageInfoParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerVipPackageInfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerVipPackageInfoTable()
        {
            UpdateTable(Define.PlayerVipPackageInfoTable, GetInData(Define.PlayerVipPackageInfoTable), Backend.UserInDate, GetPlayerVipPackageInfoParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerVipPackageInfoParam()
        {
            Param param = new Param();

            param.Add(Define.PlayerVipPackageInfo, VipPackageContainer.GetSerializeString());
            param.Add(Define.PlayerVipPackageShopInfo, VipPackageContainer.GetShopSerializeString());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerSynergyInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerSynergyInfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerSynergyInfoTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerSynergyInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerSynergyInfoTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerSynergyAccumLevel)
                            {
                                SynergyContainer.SynergyEffectAccumLevel = data[i][key].ToString().ToInt();
                            }
                            else if (key == Define.PlayerSynergyInfo)
                            {
                                SynergyContainer.SetSynergyInfoDeSerializeString(data[i][key].ToString());
                            }
                            else if (key == Define.PlayerSynergyEquipInfo)
                            {
                                SynergyContainer.SetSynergyInfoEquipSerializeString(data[i][key].ToString());
                            }
                            else if (key == Define.PlayerSynergyExp)
                            {
                                SynergyContainer.SynergyServerRecvExp = data[i][key].ToString().ToInt();
                            }
                            else if (key == Define.PlayerSynergyRune)
                            {
                                SynergyContainer.SetRuneInfoDeSerializeString(data[i][key].ToString());
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerSynergyInfoTable()
        {
            Debug.Log("InsertPlayerSynergyInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerSynergyInfoTable, GetPlayerSynergyInfoParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerSynergyInfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerSynergyInfoTable()
        {
            UpdateTable(Define.PlayerSynergyInfoTable, GetInData(Define.PlayerSynergyInfoTable), Backend.UserInDate, GetPlayerSynergyInfoParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerSynergyInfoParam()
        {
            Param param = new Param();

            param.Add(Define.PlayerSynergyAccumLevel, SynergyContainer.SynergyEffectAccumLevel.GetDecrypted());
            param.Add(Define.PlayerSynergyInfo, SynergyContainer.GetSynergyInfoSerializeString());
            param.Add(Define.PlayerSynergyEquipInfo, SynergyContainer.GetSynergyInfoEquipSerializeString());
            param.Add(Define.PlayerSynergyExp, SynergyContainer.SynergyContentExp.GetDecrypted());
            param.Add(Define.PlayerSynergyRune, SynergyContainer.GetRuneInfoSerializeString());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerDescendInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerDescendInfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerDescendInfoTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerDescendInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerDescendInfoTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerDescendAccumLevel)
                            {
                                DescendContainer.SynergyAccumLevel = data[i][key].ToString().ToInt();
                            }
                            else if (key == Define.PlayerDescendInfo)
                            {
                                DescendContainer.SetSynergyInfoDeSerializeString(data[i][key].ToString());
                            }
                            else if (key == Define.PlayerDescendEquipInfo)
                            {
                                DescendContainer.SetSynergyInfoEquipSerializeString(data[i][key].ToString());
                            }
                            else if (key == Define.PlayerDescendExp)
                            {
                                DescendContainer.SynergyServerRecvExp = data[i][key].ToString().ToInt();
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerDescendInfoTable()
        {
            Debug.Log("InsertPlayerDescendInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerDescendInfoTable, GetPlayerDescendInfoParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerDescendInfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerDescendInfoTable()
        {
            UpdateTable(Define.PlayerDescendInfoTable, GetInData(Define.PlayerDescendInfoTable), Backend.UserInDate, GetPlayerDescendInfoParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerDescendInfoParam()
        {
            Param param = new Param();

            param.Add(Define.PlayerDescendAccumLevel, DescendContainer.SynergyAccumLevel.GetDecrypted());
            param.Add(Define.PlayerDescendInfo, DescendContainer.GetSynergyInfoSerializeString());
            param.Add(Define.PlayerDescendEquipInfo, DescendContainer.GetSynergyInfoEquipSerializeString());
            param.Add(Define.PlayerDescendExp, DescendContainer.SynergyContentExp.GetDecrypted());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerSynergyRuneInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerSynergyRuneInfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerSynergyRuneInfoTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerSynergyRuneInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerSynergyRuneInfoTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerSynergyRuneInfo)
                            {
                                SynergyRuneContainer.SetSynergyInfoDeSerializeString(data[i][key].ToString());
                            }
                            else if (key == Define.PlayerSynergyRuneEquipInfo)
                            {
                                SynergyRuneContainer.SetSynergyInfoEquipSerializeString(data[i][key].ToString());
                            }
                            else if (key == Define.PlayerSynergyRuneAccumCombine)
                            {
                                SynergyRuneContainer.AccumCombineCount = data[i][key].ToString().ToInt();
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerSynergyRuneInfoTable()
        {
            Debug.Log("InsertPlayerSynergyRuneInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerSynergyRuneInfoTable, GetPlayerSynergyRuneInfoParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerSynergyRuneInfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerSynergyRuneInfoTable()
        {
            UpdateTable(Define.PlayerSynergyRuneInfoTable, GetInData(Define.PlayerSynergyRuneInfoTable), Backend.UserInDate, GetPlayerSynergyRuneInfoParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerSynergyRuneInfoParam()
        {
            Param param = new Param();

            param.Add(Define.PlayerSynergyRuneInfo, SynergyRuneContainer.GetSynergyInfoSerializeString());
            param.Add(Define.PlayerSynergyRuneEquipInfo, SynergyRuneContainer.GetSynergyInfoEquipSerializeString());
            param.Add(Define.PlayerSynergyRuneAccumCombine, SynergyRuneContainer.AccumCombineCount.GetDecrypted());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerRelicInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerRelicInfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerRelicInfoTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerRelicInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerRelicInfoTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerRelicInfo)
                            {
                                RelicContainer.SetRelicInfoDeSerializeString(data[i][key].ToString());
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerRelicInfoTable()
        {
            Debug.Log("InsertPlayerRelicInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerRelicInfoTable, GetPlayerRelicInfoParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerRelicInfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerRelicInfoTable()
        {
            UpdateTable(Define.PlayerRelicInfoTable, GetInData(Define.PlayerRelicInfoTable), Backend.UserInDate, GetPlayerRelicInfoParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerRelicInfoParam()
        {
            Param param = new Param();

            param.Add(Define.PlayerRelicInfo, RelicContainer.GetRelicInfoSerializeString());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerPoint
        //------------------------------------------------------------------------------------
        public static void GetPlayerPointTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerPointTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerPointTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerPointTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerPointTable)
                            {
                                PointDataContainer.SetDeSerializeString(data[i][key].ToString());
                            }
                            else if (key == Define.PlayerDiaAmount)
                            {
                                PointDataContainer.DiaAmount = data[i][key].ToString();
                                PointDataContainer.DiaAmountRecord = data[i][key].ToString().ToDouble();
                            }
                            else if (key == Define.PlayerAccumUseDia)
                            {
                                PointDataContainer.AccumUseDia = data[i][key].ToString().ToDouble();
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerPointTable()
        {
            Debug.Log("InsertPlayerPointTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerPointTable, GetPlayerPointParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerPointTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerPointTable()
        {
            UpdateTable(Define.PlayerPointTable, GetInData(Define.PlayerPointTable), Backend.UserInDate, GetPlayerPointParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerPointParam()
        {
            Param param = new Param();

            param.Add(Define.PlayerPointTable, PointDataContainer.GetSerializeString());
            param.Add(Define.PlayerDiaAmount, PointDataContainer.DiaAmountRecord);
            param.Add(Define.PlayerAccumUseDia, PointDataContainer.AccumUseDia.GetDecrypted());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerSummonTicket
        //------------------------------------------------------------------------------------
        public static void GetPlayerSummonTicketTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerSummonTicketTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerSummonTicketTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerSummonTicketTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerSummonTicketTable)
                            {
                                SummonTicketContainer.SetDeSerializeString(data[i][key].ToString());
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerSummonTicketTable()
        {
            Debug.Log("InsertPlayerSummonTicketTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerSummonTicketTable, GetPlayerSummonTicketParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerSummonTicketTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerSummonTicketTable()
        {
            UpdateTable(Define.PlayerSummonTicketTable, GetInData(Define.PlayerSummonTicketTable), Backend.UserInDate, GetPlayerSummonTicketParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerSummonTicketParam()
        {
            Param param = new Param();

            param.Add(Define.PlayerSummonTicketTable, SummonTicketContainer.GetSerializeString());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerBox
        //------------------------------------------------------------------------------------
        public static void GetPlayerBoxTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerBoxTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerBoxTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerBoxTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerBoxTable)
                            {
                                BoxContainer.SetDeSerializeString(data[i][key].ToString());
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerBoxTable()
        {
            Debug.Log("InsertPlayerBoxTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerBoxTable, GetPlayerBoxParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerBoxTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerBoxTable()
        {
            UpdateTable(Define.PlayerBoxTable, GetInData(Define.PlayerBoxTable), Backend.UserInDate, GetPlayerBoxParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerBoxParam()
        {
            Param param = new Param();

            param.Add(Define.PlayerBoxTable, BoxContainer.GetSerializeString());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerTraining
        //------------------------------------------------------------------------------------
        public static void GetPlayerTrainingTable()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerTrainingTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerTrainingTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerTrainingTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerTrainingTable)
                            {
                                string str = data[i][key].ToString();

                                PlayerTrainingDataContainer.m_trainingLevel.Clear();
                                PlayerTrainingDataContainer.SetDeSerializeString(data[i][key].ToString());
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerTrainingTable()
        {
            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerTrainingTable, GetPlayerTrainingParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerTrainingTable();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerTrainingTable()
        {
            UpdateTable(Define.PlayerTrainingTable, GetInData(Define.PlayerTrainingTable), Backend.UserInDate, GetPlayerTrainingParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerTrainingParam()
        {
            Param param = new Param();

            param.Add(Define.PlayerTrainingTable, PlayerTrainingDataContainer.GetSerializeString());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_Gear
        //------------------------------------------------------------------------------------
        public static void GetPlayerGearInfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerGearTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerGearInfo();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerGearTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerGearInfo)
                            {
                                GearContainer.SetSynergyInfoDeSerializeString(data[i][key].ToString());
                            }
                            else if (key == Define.PlayerGearEquipInfo)
                            {
                                GearContainer.SetSynergyInfoEquipSerializeString(data[i][key].ToString());
                            }
                            else if (key == Define.PlayerGearSlotInfo)
                            {
                                GearContainer.SetSynergyInfoSlotSerializeString(data[i][key].ToString());
                            }
                            else if (key == Define.PlayerGearAccumCombine)
                            {
                                GearContainer.AccumCombineCount = data[i][key].ToString().ToInt();
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerGearInfo()
        {
            Debug.Log("InsertCharacterEquipmentInfo()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerGearTable, GetPlayerGearParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerGearInfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerGearInfoTable()
        {
            UpdateTable(Define.PlayerGearTable, GetInData(Define.PlayerGearTable), Backend.UserInDate, GetPlayerGearParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerGearParam()
        {
            Param param = new Param();

            param.Add(Define.PlayerGearInfo, GearContainer.GetSynergyInfoSerializeString());
            param.Add(Define.PlayerGearEquipInfo, GearContainer.GetSynergyInfoEquipSerializeString());
            param.Add(Define.PlayerGearSlotInfo, GearContainer.GetSynergyInfoSlotSerializeString());
            param.Add(Define.PlayerGearAccumCombine, GearContainer.AccumCombineCount.GetDecrypted());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_Skin
        //------------------------------------------------------------------------------------
        public static void GetPlayerSkinInfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerSkinTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerSkinInfo();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerSkinTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerSkinTable)
                            {
                                string str = data[i][key].ToString();

                                try
                                {
                                    CharacterSkinContainer.m_skinInfo.Clear();
                                    CharacterSkinContainer.SetSkinInfoDeSerializeString(data[i][key].ToString());
                                }
                                catch (System.Exception ex)
                                {
                                    Debug.Log("IsError" + ex.ToString());
                                }
                            }
                            else if (key == Define.PlayerSkinEquip)
                            {
                                string str = data[i][key].ToString();

                                try
                                {
                                    CharacterSkinContainer.m_skinEquipId.Clear();
                                    CharacterSkinContainer.SetSkinEquipDeSerializeString(data[i][key].ToString());
                                }
                                catch (System.Exception ex)
                                {
                                    Debug.Log("IsError" + ex.ToString());
                                }
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerSkinInfo()
        {
            Debug.Log("InsertCharacterEquipmentInfo()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerSkinTable, GetPlayerSkinParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerSkinInfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerSkinInfoTable()
        {
            UpdateTable(Define.PlayerSkinTable, GetInData(Define.PlayerSkinTable), Backend.UserInDate, GetPlayerSkinParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerSkinParam()
        {
            Param param = new Param();

            param.Add(Define.PlayerSkinTable, CharacterSkinContainer.GetSkinInfoSerializeString());
            param.Add(Define.PlayerSkinEquip, CharacterSkinContainer.GetSkinEquipSerializeString());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_CharacterSkillInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerSkillinfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerSkillInfoTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerSkillInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerSkillInfoTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerSkillInfo)
                            {
                                string str = data[i][key].ToString();

                                try
                                {

                                    ARRRSkillContainer._skillInfo.Clear();
                                    ARRRSkillContainer.SetSkillInfoDeSerializeString(data[i][key].ToString());

                                }
                                catch (System.Exception ex)
                                {
                                    Debug.Log("IsError" + ex.ToString());
                                }
                            }
                            else if (key == Define.PlayerSkillSlotInfo)
                            {
                                string str = data[i][key].ToString();
                                try
                                {

                                    ARRRSkillContainer._skillTempSlotData.Clear();
                                    ARRRSkillContainer.SetSkillSlotDeSerializeString(data[i][key].ToString());

                                }
                                catch (System.Exception ex)
                                {
                                    Debug.Log("IsError" + ex.ToString());
                                }

                                //LitJson.JsonData chartJson = LitJson.JsonMapper.ToObject(str);
                                //SetSkillSlotData(chartJson);
                            }
                            //else if (key == Define.PlayerSkillSlotPage)
                            //{
                            //    CharacterSkillContainer.SkillSlotPage = data[i][key].ToString().ToInt();
                            //}
                            //else if (key == Define.PlayerSkillOpenSlotCount)
                            //{
                            //    CharacterSkillContainer.CurrentOpenSlotCount = data[i][key].ToString().ToInt();
                            //}
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerSkillInfoTable()
        {
            Debug.Log("InsertCharacterSkillInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerSkillInfoTable, GetPlayerSkillParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerSkillinfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerSkillInfoTable()
        {
            UpdateTable(Define.PlayerSkillInfoTable, GetInData(Define.PlayerSkillInfoTable), Backend.UserInDate, GetPlayerSkillParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerSkillParam()
        {
            Param param = new Param();

            //if (CharacterSkillContainer.m_skillSlotData.Count <= 0)
            //{
            //    CharacterSkillLocalTable m_characterSkillLocalTable = Managers.TableManager.Instance.GetTableClass<CharacterSkillLocalTable>();
            //    List<CharacterSkillSlotData> m_characterSkillSlotDatas = m_characterSkillLocalTable.GetAllSlotData();

            //    Dictionary<int, ObscuredInt> pagevalue = new Dictionary<int, ObscuredInt>();
            //    for (int i = 0; i < m_characterSkillSlotDatas.Count; ++i)
            //    {
            //        pagevalue.Add(m_characterSkillSlotDatas[i].SlotNumber, -1);
            //    }

            //    for (int i = 0; i < Define.SkillSlotTotalPage; ++i)
            //    {
            //        CharacterSkillContainer.m_skillSlotData.Add(i, pagevalue);
            //    }
            //}

            param.Add(Define.PlayerSkillInfo, ARRRSkillContainer.GetSkillInfoSerializeString());
            param.Add(Define.PlayerSkillSlotInfo, ARRRSkillContainer.GetSkillSlotSerializeString());

            //param.Add(Define.PlayerSkillSlotPage, CharacterSkillContainer.SkillSlotPage);
            //param.Add(Define.PlayerSkillOpenSlotCount, CharacterSkillContainer.CurrentOpenSlotCount.GetDecrypted());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerMapInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerMapInfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerMapInfoTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerMapInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerMapInfoTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerMapMaxWaveClear)
                            {
                                MapContainer.MaxWaveClear = data[i][key].ToString().ToInt();
                            }
                            else if (key == Define.PlayerMapMaxClear)
                            {
                                MapContainer.MapMaxClear = data[i][key].ToString().ToInt();
                            }
                            else if (key == Define.PlayerMapMapStageInfo)
                            {
                                MapContainer.SetSkillInfoDeSerializeString(data[i][key].ToString());
                            }
                            else if (key == Define.PlayerMapKey)
                            {
                                MapContainer.MapEnterKey = data[i][key].ToString();
                            }
                            else if (key == Define.PlayerEventDayInitTime)
                            {
                                MapContainer.EventDayInitTime = data[i][key].ToString().ToDouble();
                            }
                            else if (key == Define.PlayerToDaySweepCount)
                            {
                                MapContainer.ToDaySweepCount = data[i][key].ToString().ToInt();
                            }
                            else if (key == Define.PlayerToDayDoubleRewardCount)
                            {
                                MapContainer.ToDayDoubleRewardCount = data[i][key].ToString().ToInt();
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerMapInfoTable()
        {
            Debug.Log("InsertPlayerMapInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerMapInfoTable, GetPlayerMapParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerMapInfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerMapInfoTable()
        {
            UpdateTable(Define.PlayerMapInfoTable, GetInData(Define.PlayerMapInfoTable), Backend.UserInDate, GetPlayerMapParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerMapParam()
        {
            Param param = new Param();
            param.Add(Define.PlayerMapMaxWaveClear, MapContainer.MaxWaveClear.GetDecrypted());
            param.Add(Define.PlayerMapMaxClear, MapContainer.MapMaxClear.GetDecrypted());
            param.Add(Define.PlayerMapMapStageInfo, MapContainer.GetSkillInfoSerializeString());
            param.Add(Define.PlayerMapKey, MapContainer.MapEnterKey.GetDecrypted());

            param.Add(Define.PlayerEventDayInitTime, MapContainer.EventDayInitTime.GetDecrypted());
            param.Add(Define.PlayerToDaySweepCount, MapContainer.ToDaySweepCount.GetDecrypted());
            param.Add(Define.PlayerToDayDoubleRewardCount, MapContainer.ToDayDoubleRewardCount.GetDecrypted());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerResearchInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerResearchinfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerResearchInfoTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerResearchInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerResearchInfoTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerResearchInfo)
                            {
                                string str = data[i][key].ToString();
                                //Debug.Log(str);

                                ResearchContainer.ResearchInfo.Clear();
                                ResearchContainer.SetDeSerializeString(data[i][key].ToString());
                            }
                            else if (key == Define.PlayerResearchSlot)
                            {
                                string str = data[i][key].ToString();
                                //Debug.Log(str);

                                ResearchContainer.ResearchSlot.Clear();
                                ResearchContainer.SetSlotDeSerializeString(data[i][key].ToString());
                            }
                            else if (key == Define.PlayerResearchAdCount)
                            {
                                ResearchContainer.TodayAdViewCount = data[i][key].ToString().ToInt();
                            }
                            else if (key == Define.PlayerResearchDailyInitTime)
                            {
                                ResearchContainer.DailyInitTime = data[i][key].ToString().ToDouble();
                            }
                            else if (key == Define.PlayerResearchChargeCount)
                            {
                                ResearchContainer.ChargeResearchCount = data[i][key].ToString().ToDouble();
                            }
                            else if (key == Define.PlayerResearchLastCharge)
                            {
                                ResearchContainer.ReserchLastChargeTime = data[i][key].ToString().ToDouble();
                            }
                            else if (key == Define.PlayerResearchViewQueue)
                            {
                                string str = data[i][key].ToString();
                                //Debug.Log(str);

                                ResearchContainer.ResearchEndViewQueue.Clear();
                                ResearchContainer.SetViewQueueDeSerializeString(data[i][key].ToString());
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerResearchInfoTable()
        {
            Param param = new Param();
            param.Add(Define.PlayerResearchInfo, LitJson.JsonMapper.ToJson(ResearchContainer.ResearchInfo));

            Debug.Log("InsertPlayerResearchInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerResearchInfoTable, GetPlayerResearchParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerResearchinfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerResearchInfoTable()
        {
            UpdateTable(Define.PlayerResearchInfoTable, GetInData(Define.PlayerResearchInfoTable), Backend.UserInDate, GetPlayerResearchParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerResearchParam()
        {
            Param param = new Param();

            param.Add(Define.PlayerResearchInfo, ResearchContainer.GetSerializeString());
            param.Add(Define.PlayerResearchSlot, ResearchContainer.GetSlotSerializeString());
            param.Add(Define.PlayerResearchAdCount, ResearchContainer.TodayAdViewCount.GetDecrypted());
            param.Add(Define.PlayerResearchDailyInitTime, ResearchContainer.DailyInitTime.GetDecrypted());
            param.Add(Define.PlayerResearchChargeCount, ResearchContainer.ChargeResearchCount.GetDecrypted());
            param.Add(Define.PlayerResearchLastCharge, ResearchContainer.ReserchLastChargeTime.GetDecrypted());
            param.Add(Define.PlayerResearchViewQueue, ResearchContainer.GetViewQueueSerializeString());
            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerSummonInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerSummoninfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerSummonInfoTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerSummonInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerSummonInfoTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerSummonInfo)
                            {
                                string str = data[i][key].ToString();
                                //Debug.Log(str);

                                try
                                {
                                    SummonContainer.m_summonInfo.Clear();
                                    SummonContainer.SetDeSerializeString(data[i][key].ToString());
                                }
                                catch (System.Exception ex)
                                {
                                    Debug.Log("IsError" + ex.ToString());
                                }
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerSummonInfoTable()
        {
            Debug.Log("InsertPlayerSummonInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerSummonInfoTable, GetPlayerSummonParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerSummoninfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerSummonInfoTable()
        {
            UpdateTable(Define.PlayerSummonInfoTable, GetInData(Define.PlayerSummonInfoTable), Backend.UserInDate, GetPlayerSummonParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerSummonParam()
        {
            Param param = new Param();
            //if (SummonContainer.m_summonInfo.Count <= 0)
            //{
            //    SummonLocalTable summonLocalTable = Managers.TableManager.Instance.GetTableClass<SummonLocalTable>();
            //    Dictionary<V2Enum_SummonType, SummonData> summonDatas = summonLocalTable.GetSummonDatas();

            //    foreach (KeyValuePair<V2Enum_SummonType, SummonData> pair in summonDatas)
            //    {
            //        SummonData summonData = pair.Value;
            //        SummonInfo summonInfo = new SummonInfo();
            //        summonInfo.Id = summonData.Index;

            //        SummonContainer.m_summonInfo.Add(pair.Key, summonInfo);
            //    }
            //}


            param.Add(Define.PlayerSummonInfo, SummonContainer.GetSerializeString());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerTimeInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerTimeinfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerTimeInfoTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerTimeInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerTimeInfoTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerLastRecvStageCoolTimeReward)
                            {
                                TimeContainer.LastRecvStageCoolTimeReward = data[i][key].ToString().ToDouble();
                            }
                            else if (key == Define.PlayerAccumLoginTime)
                            {
                                TimeContainer.AccumLoginTime = data[i][key].ToString().ToDouble();
                            }
                            else if (key == Define.PlayerDailyInitTimeStamp)
                            {
                                TimeContainer.DailyInitTimeStamp = data[i][key].ToString().ToDouble();
                            }
                            else if (key == Define.PlayerAccumLoginCount)
                            {
                                TimeContainer.AccumLoginCount = data[i][key].ToString().ToInt();
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerTimeInfoTable()
        {
            Debug.Log("InsertPlayerTimeInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerTimeInfoTable, GetPlayerTimeParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerTimeinfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerTimeInfoTable()
        {
            UpdateTable(Define.PlayerTimeInfoTable, GetInData(Define.PlayerTimeInfoTable), Backend.UserInDate, GetPlayerTimeParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerTimeParam()
        {
            Param param = new Param();
            param.Add(Define.PlayerLastRecvStageCoolTimeReward, (long)TimeContainer.LastRecvStageCoolTimeReward.GetDecrypted());
            param.Add(Define.PlayerAccumLoginTime, (long)TimeContainer.AccumLoginTime.GetDecrypted());
            param.Add(Define.PlayerDailyInitTimeStamp, TimeContainer.DailyInitTimeStamp.GetDecrypted());
            param.Add(Define.PlayerAccumLoginCount, TimeContainer.AccumLoginCount.GetDecrypted());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerAdBuffInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerAdBuffinfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerAdBuffInfoTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerAdBuffInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerAdBuffInfoTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerAdBuffInfo)
                            {
                                string str = data[i][key].ToString();

                                try
                                {
                                    AdBuffContainer.AdBuffInfo.Clear();
                                    AdBuffContainer.SetDeSerializeString(data[i][key].ToString());

                                }
                                catch (System.Exception ex)
                                {
                                    Debug.Log("IsError" + ex.ToString());
                                }
                            }
                            else if (key == Define.PlayerAdBuffInitTime)
                            {
                                AdBuffContainer.DailyInitTimeStemp = data[i][key].ToString().ToDouble();
                            }
                            else if (key == Define.PlayerAdBuffTodayActiveCount)
                            {
                                AdBuffContainer.BuffActiveCount = data[i][key].ToString().ToInt();
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerAdBuffInfoTable()
        {
            Debug.Log("InsertPlayerAdBuffInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerAdBuffInfoTable, GetPlayerAdBuffParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerAdBuffinfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerAdBuffInfoTable()
        {
            UpdateTable(Define.PlayerAdBuffInfoTable, GetInData(Define.PlayerAdBuffInfoTable), Backend.UserInDate, GetPlayerAdBuffParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerAdBuffParam()
        {
            Param param = new Param();
            param.Add(Define.PlayerAdBuffInfo, AdBuffContainer.GetSerializeString());

            param.Add(Define.PlayerAdBuffInitTime, AdBuffContainer.DailyInitTimeStemp.GetDecrypted());
            param.Add(Define.PlayerAdBuffTodayActiveCount, AdBuffContainer.BuffActiveCount.GetDecrypted());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerCheckInInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerCheckInInfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerCheckInInfoTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerCheckInInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerCheckInInfoTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerCheckInRewardInfo)
                            {
                                string str = data[i][key].ToString();
                                //Debug.Log(str);

                                try
                                {
                                    CheckInContainer.m_checkInInfo.Clear();
                                    CheckInContainer.SetDeSerializeString(data[i][key].ToString());
                                }
                                catch (System.Exception ex)
                                {
                                    Debug.Log("IsError" + ex.ToString());
                                }
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerCheckInInfoTable()
        {
            Debug.Log("InsertPlayerCheckInInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerCheckInInfoTable, GetPlayerCheckInParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerCheckInInfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerCheckInInfoTable(System.Action onFinish)
        {
            UpdateTable(Define.PlayerCheckInInfoTable, GetInData(Define.PlayerCheckInInfoTable), Backend.UserInDate, GetPlayerCheckInParam(), o =>
            {
                if (onFinish != null)
                    onFinish();
            });
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerCheckInParam()
        {
            Param param = new Param();

            param.Add(Define.PlayerCheckInRewardInfo, CheckInContainer.GetSerializeString());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerShopInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerShopInfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerShopInfoTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerShopInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerShopInfoTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerShopInfoTable)
                            {
                                string str = data[i][key].ToString();
                                //Debug.Log(str);

                                try
                                {
                                    ShopContainer.m_shopInfo.Clear();
                                    ShopContainer.SetDeSerializeString(data[i][key].ToString());
                                }
                                catch (System.Exception ex)
                                {
                                    Debug.Log("IsError" + ex.ToString());
                                }
                            }
                            else if (key == Define.PlayerShopPostInfo)
                            {
                                string str = data[i][key].ToString();
                                //Debug.Log(str);

                                try
                                {

                                    ShopPostContainer.m_shopPostInfos.Clear();
                                    ShopPostContainer.SetDeSerializeString(data[i][key].ToString());

                                }
                                catch (System.Exception ex)
                                {
                                    Debug.Log("IsError" + ex.ToString());
                                }
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerShopInfoTable()
        {
            Debug.Log("InsertPlayerShopInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerShopInfoTable, GetPlayerShopParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerShopInfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerShopInfoTable(System.Action onFinish)
        {
            UpdateTable(Define.PlayerShopInfoTable, GetInData(Define.PlayerShopInfoTable), Backend.UserInDate, GetPlayerShopParam(), o =>
            {
                if (onFinish != null)
                    onFinish();
            });
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerShopParam()
        {
            Param param = new Param();

            param.Add(Define.PlayerShopInfoTable, ShopContainer.GetSerializeString());
            param.Add(Define.PlayerShopPostInfo, ShopPostContainer.GetSerializeString());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerShopRandomStoreInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerShopRandomStoreInfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerShopRandomStoreInfoTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerShopRandomStoreInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerShopRandomStoreInfoTable, data[i][key].ToString());
                            }

                            else if (key == Define.PlayerShopRandomStoreInitTime)
                            {
                                ShopRandomStoreContainer.DailyInitTimeStemp = data[i][key].ToString().ToDouble();
                            }

                            else if (key == Define.PlayerShopRandomStoreBuyInfo)
                            {
                                string str = data[i][key].ToString();
                                //Debug.Log(str);

                                try
                                {
                                    ShopRandomStoreContainer.BuyRandomStoreInfo.Clear();
                                    ShopRandomStoreContainer.SetBuyHonorShopInfoDeSerializeString(data[i][key].ToString());
                                }
                                catch (System.Exception ex)
                                {
                                    Debug.Log("IsError" + ex.ToString());
                                }
                            }
                            else if (key == Define.PlayerShopRandomStoreDisplay)
                            {
                                string str = data[i][key].ToString();
                                //Debug.Log(str);

                                try
                                {
                                    ShopRandomStoreContainer.StoreDisPlayList.Clear();
                                    ShopRandomStoreContainer.SetHonorShopDisPlayListDeSerializeString(data[i][key].ToString());
                                }
                                catch (System.Exception ex)
                                {
                                    Debug.Log("IsError" + ex.ToString());
                                }
                            }

                            else if (key == Define.PlayerShopRandomStoreResetAdView)
                            {
                                ShopRandomStoreContainer.StoreResetAdViewCount = data[i][key].ToString().ToInt();
                            }
                            else if (key == Define.PlayerShopRandomStoreResetDia)
                            {
                                ShopRandomStoreContainer.StoreResetDiaCount = data[i][key].ToString().ToInt();
                            }

                            else if (key == Define.PlayerShopRandomStoreDiaFree)
                            {
                                string str = data[i][key].ToString();
                                //Debug.Log(str);

                                try
                                {
                                    ShopRandomStoreContainer.ShopFreeGoodsInfos.Clear();
                                    ShopRandomStoreContainer.SetSynergyInfoDeSerializeString(data[i][key].ToString());
                                }
                                catch (System.Exception ex)
                                {
                                    Debug.Log("IsError" + ex.ToString());
                                }
                            }

                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerShopRandomStoreInfoTable()
        {
            Debug.Log("InsertPlayerShopRandomStoreInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerShopRandomStoreInfoTable, GetPlayerShopRandomStoreParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerShopRandomStoreInfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerShopRandomStoreInfoTable()
        {
            UpdateTable(Define.PlayerShopRandomStoreInfoTable, GetInData(Define.PlayerShopRandomStoreInfoTable), Backend.UserInDate, GetPlayerShopRandomStoreParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerShopRandomStoreParam()
        {
            Param param = new Param();

            param.Add(Define.PlayerShopRandomStoreInitTime, ShopRandomStoreContainer.DailyInitTimeStemp.GetDecrypted());

            param.Add(Define.PlayerShopRandomStoreBuyInfo, ShopRandomStoreContainer.GetBuyHonorShopInfoSerializeString());
            param.Add(Define.PlayerShopRandomStoreDisplay, ShopRandomStoreContainer.GetHonorShopDisPlayListSerializeString());

            param.Add(Define.PlayerShopRandomStoreResetAdView, ShopRandomStoreContainer.StoreResetAdViewCount.GetDecrypted());
            param.Add(Define.PlayerShopRandomStoreResetDia, ShopRandomStoreContainer.StoreResetDiaCount.GetDecrypted());

            param.Add(Define.PlayerShopRandomStoreDiaFree, ShopRandomStoreContainer.GetSynergyInfoSerializeString());




            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerPassInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerPassInfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerPassInfoTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerPassInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerPassInfoTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerPassMonsterKillInfo)
                            {
                                PassContainer.AccumMonster = data[i][key].ToString().ToInt();
                            }
                            else if (key == Define.PlayerPassInfo)
                            {
                                string str = data[i][key].ToString();
                                //Debug.Log(str);

                                try
                                {
                                    PassContainer.m_passInfos.Clear();
                                    PassContainer.SetDeSerializeString(data[i][key].ToString());
                                }
                                catch (System.Exception ex)
                                {
                                    Debug.Log("IsError" + ex.ToString());
                                }
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerPassInfoTable()
        {
            Debug.Log("InsertPlayerPassInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerPassInfoTable, GetPlayerPassParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerPassInfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerPassInfoTable()
        {
            UpdateTable(Define.PlayerPassInfoTable, GetInData(Define.PlayerPassInfoTable), Backend.UserInDate, GetPlayerPassParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerPassParam()
        {
            Param param = new Param();

            param.Add(Define.PlayerPassMonsterKillInfo, PassContainer.AccumMonster.GetDecrypted());
            param.Add(Define.PlayerPassInfo, PassContainer.GetSerializeString());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerQuestInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerQuestInfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerQuestInfoTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerQuestInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerQuestInfoTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerClearGuideQuestOrderInfo)
                            {
                                int value = data[i][key].ToString().ToInt();
                                GuideQuestContainer.ClearGuideQuestOrder = value;
                            }
                            else if (key == Define.PlayerQuestInfo)
                            {
                                string str = data[i][key].ToString();
                                //Debug.Log(str);

                                try
                                {
                                    QuestContainer.QuestInfos.Clear();
                                    QuestContainer.SetQuestInfosDeSerializeString(data[i][key].ToString());
                                }
                                catch (System.Exception ex)
                                {
                                    Debug.Log("IsError" + ex.ToString());
                                }
                            }
                            else if (key == Define.PlayerQuestGaugeInfo)
                            {
                                string str = data[i][key].ToString();
                                //Debug.Log(str);

                                try
                                {
                                    QuestContainer.QuestGaugeInfos.Clear();
                                    QuestContainer.SetQuestGaugeInfosDeSerializeString(data[i][key].ToString());
                                }
                                catch (System.Exception ex)
                                {
                                    Debug.Log("IsError" + ex.ToString());
                                }
                            }
                            else if (key == Define.PlayerQuestDailyInit)
                            {
                                QuestContainer.DaliyInitTimeStemp = data[i][key].ToString().ToDouble();
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerQuestInfoTable()
        {
            Debug.Log("InsertPlayerQuestInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerQuestInfoTable, GetPlayerQuestParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerQuestInfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerQuestInfoTable()
        {
            UpdateTable(Define.PlayerQuestInfoTable, GetInData(Define.PlayerQuestInfoTable), Backend.UserInDate, GetPlayerQuestParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerQuestParam()
        {
            Param param = new Param();
            param.Add(Define.PlayerClearGuideQuestOrderInfo, GuideQuestContainer.ClearGuideQuestOrder.GetDecrypted());


            param.Add(Define.PlayerQuestInfo, QuestContainer.GetQuestInfosSerializeString());
            param.Add(Define.PlayerQuestGaugeInfo, QuestContainer.GetQuestGaugeInfosSerializeString());
            param.Add(Define.PlayerQuestDailyInit, QuestContainer.DaliyInitTimeStemp.GetDecrypted());
            

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerTimeAttackMissionInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerTimeAttackMissionInfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerTimeAttackMissionInfoTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerTimeAttackMissionInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerTimeAttackMissionInfoTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerTimeAttackMissionInfo)
                            {
                                string str = data[i][key].ToString();
                                //Debug.Log(str);

                                try
                                {
                                    TimeAttackMissionContainer.TimeAttackMissionInfos.Clear();
                                    TimeAttackMissionContainer.SetTimeAttackMissionInfosDeSerializeString(data[i][key].ToString());
                                }
                                catch (System.Exception ex)
                                {
                                    Debug.Log("IsError" + ex.ToString());
                                }
                            }
                            else if (key == Define.PlayerFocusMissionInfo)
                            {
                                int value = data[i][key].ToString().ToInt();
                                TimeAttackMissionContainer.FocusMission = value;
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerTimeAttackMissionInfoTable()
        {
            Debug.Log("InsertPlayerTimeAttackMissionInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerTimeAttackMissionInfoTable, GetPlayerTimeAttackMissionParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerTimeAttackMissionInfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerTimeAttackMissionInfoTable()
        {
            UpdateTable(Define.PlayerTimeAttackMissionInfoTable, GetInData(Define.PlayerTimeAttackMissionInfoTable), Backend.UserInDate, GetPlayerTimeAttackMissionParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerTimeAttackMissionParam()
        {
            Param param = new Param();
            param.Add(Define.PlayerTimeAttackMissionInfo, TimeAttackMissionContainer.GetTimeAttackMissionInfosSerializeString());
            param.Add(Define.PlayerFocusMissionInfo, TimeAttackMissionContainer.FocusMission.GetDecrypted());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerExchangeInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerExchangeInfoTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerExchangeInfoTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerExchangeInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerExchangeInfoTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerExchangeInfo)
                            {
                                string str = data[i][key].ToString();
                                //Debug.Log(str);

                                try
                                {
                                    ExchangeContainer.ExchangeInfos.Clear();
                                    ExchangeContainer.SetDeSerializeString(data[i][key].ToString());
                                }
                                catch (System.Exception ex)
                                {
                                    Debug.Log("IsError" + ex.ToString());
                                }
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerExchangeInfoTable()
        {
            Debug.Log("InsertPlayerExchangeInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerExchangeInfoTable, GetPlayerExchangeParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerExchangeInfoTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerExchangeInfoTable()
        {
            UpdateTable(Define.PlayerExchangeInfoTable, GetInData(Define.PlayerExchangeInfoTable), Backend.UserInDate, GetPlayerExchangeParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerExchangeParam()
        {
            Param param = new Param();

            param.Add(Define.PlayerExchangeInfo, ExchangeContainer.GetSerializeString());

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_PlayerRankInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerRankTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerRankInfoTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerRankInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerRankInfoTable, data[i][key].ToString());
                            }
                            else if (key == Define.PlayerRankCombatPower)
                            {
                                RankContainer.MyRankInfo.CombatPower = data[i][key].ToString().ToDouble();
                            }
                            else if (key == Define.PlayerStage)
                            {
                                RankContainer.MyRankInfo.Stage = data[i][key].ToString().ToInt();
                            }
                            //else if (key == Define.PlayerDetail)
                            //{
                            //    string str = data[i][key].ToString();

                            //    try
                            //    {
                            //        RankContainer.MyRankInfo.Detail = JsonConvert.DeserializeObject<RankDetailInfo>(str);
                            //    }
                            //    catch (System.Exception ex)
                            //    {
                            //        Debug.Log("IsError" + ex.ToString());
                            //    }
                            //}
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerRankInfoTable()
        {
            Debug.Log("InsertPlayerRankInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerRankInfoTable, GetPlayerRankParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerRankTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerRankInfoTable()
        {
            UpdateTable(Define.PlayerRankInfoTable, GetInData(Define.PlayerRankInfoTable), Backend.UserInDate, GetPlayerRankParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerRankParam()
        {
            Param param = new Param();
            param.Add(Define.PlayerRankCombatPower, RankContainer.MyRankInfo.CombatPower.GetDecrypted());
            param.Add(Define.PlayerStage, RankContainer.MyRankInfo.Stage.GetDecrypted());

            //param.Add(Define.PlayerDetail, LitJson.JsonMapper.ToJson(RankContainer.MyRankInfo.Detail));

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region TheBackEnd_ViewDataInfo
        //------------------------------------------------------------------------------------
        public static void GetPlayerViewDataTableData()
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, Define.PlayerViewDataTable, new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(bro);
                    return;
                }

                var data = bro.FlattenRows();

                if (data.Count == 0)
                {
                    InsertPlayerViewDataInfoTable();
                }
                else
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        foreach (var key in data[i].Keys)
                        {
                            if (key == "inDate")
                            {
                                AddInData(Define.PlayerViewDataTable, data[i][key].ToString());
                                break;
                            }
                        }
                    }

                    Message.Send(new Event.CompleteTableLoadMsg());
                }
            });
        }
        //------------------------------------------------------------------------------------
        private static void InsertPlayerViewDataInfoTable()
        {
            Debug.Log("InsertPlayerViewDataInfoTable()");

            SendQueue.Enqueue(Backend.PlayerData.InsertData, Define.PlayerViewDataTable, GetPlayerViewDataParam(), (callback) =>
            {
                if (callback.IsSuccess() == true)
                {
                    GetPlayerViewDataTableData();
                }
                else
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
        public static void UpdatePlayerViewDataInfoTable()
        {
            if (string.IsNullOrEmpty(GetInData(Define.PlayerViewDataTable)))
                return;

            UpdateTable(Define.PlayerViewDataTable, GetInData(Define.PlayerViewDataTable), Backend.UserInDate, GetPlayerViewDataParam());
        }
        //------------------------------------------------------------------------------------
        public static Param GetPlayerViewDataParam()
        {
            Param param = new Param();

            param.Add(Define.PlayerDiaAmount, PointDataContainer.DiaAmountRecord);
            param.Add(Define.PlayerAccumUseDia, PointDataContainer.AccumUseDia.GetDecrypted());

            param.Add(Define.PlayerViewDataBuyPrice, ShopContainer.TotalBuyPrice);

            return param;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
    }
}