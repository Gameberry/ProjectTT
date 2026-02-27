using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;
using GameBerry.TheBackEnd;
using Cysharp.Threading.Tasks;

namespace GameBerry.Table
{
    public class UpdateDataWaitStruct
    {
        public List<TableBase> tableBases;
        public float SendTime;
        public Action<BackendReturnObject> CallBack;

        public async UniTask PlayTimer()
        {
            while (Time.time < SendTime)
                await UniTask.NextFrame(PlayerLoopTiming.Update);

            UserTable.TransactionUpdate(tableBases, CallBack);
            UserTable.dynamicUpdateData_Wait1Second.Remove(tableBases);
            UserTable.updateDataWaitStruct_Pool.PoolObject(this);
        }
    }

    public abstract class TableBase
    {
        public string TableName = string.Empty;
        public string InData { get; private set; }

        public bool UpdateWaitData = false;

        //------------------------------------------------------------------------------------
        public abstract void SetData(JsonData jsonData);
        //------------------------------------------------------------------------------------
        public abstract Param GetParam();
        //------------------------------------------------------------------------------------
        public virtual void LoadComplete()
        {

        }
        //------------------------------------------------------------------------------------
        public void SetInData(string inData)
        {
            InData = inData;
        }
        //------------------------------------------------------------------------------------
        public void InsertTable()
        {
            UserTable.InsertTable(TableName, GetParam(), o =>
            {
                if (o.IsSuccess() == false)
                    return;

                SetInData(o.GetInDate());
            });
        }
        //------------------------------------------------------------------------------------
        public void UpdateTable(bool immediate = true, System.Action<BackendReturnObject> action = null)
        {
            if (string.IsNullOrEmpty(TableName) == true)
                return;

            if (immediate == false)
            { 
                UpdateWaitData = true;
                return;
            }

            if (string.IsNullOrEmpty(InData) == true)
            {
                InsertTable();
                return;
            }

            SendQueue.Enqueue(Backend.GameData.UpdateV2, TableName, InData, Backend.UserInDate, GetParam(), (callback) =>
            {
                if (callback.IsSuccess() == false)
                {
                    TheBackEndManager.Instance.BackEndErrorCode(callback);
                }
                else
                {
                    action?.Invoke(callback);
                }
            });
        }
        //------------------------------------------------------------------------------------
    }

    public static class UserTable
    {
        public static Dictionary<Type, TableBase> TableData = new Dictionary<Type, TableBase>();

        private static float updateWaitDataTimer = 0.0f;
        private static float updateWaitDataTimerTurm = 300.0f;

        public static Common.ObjectPoolClass<UpdateDataWaitStruct> updateDataWaitStruct_Pool = new Common.ObjectPoolClass<UpdateDataWaitStruct>();

        public static Dictionary<List<TableBase>, UpdateDataWaitStruct> dynamicUpdateData_Wait1Second = new Dictionary<List<TableBase>, UpdateDataWaitStruct>();
        private static Dictionary<List<TableBase>, UpdateDataWaitStruct> reconnectUpdateData = new Dictionary<List<TableBase>, UpdateDataWaitStruct>();

        public static bool isCheatingUser = false;

        //------------------------------------------------------------------------------------
        public static IEnumerator LoadUserTable(Action<int, int> process)
        {
            BackendReturnObject callback = null;

            Backend.GameData.GetTableList(bro => { callback = bro; });

            yield return new WaitUntil(() => callback != null);

            Debug.Log(callback.GetReturnValue());

            if (callback.IsSuccess() == true)
            {
                var data = callback.GetReturnValuetoJSON();
            }
            else
            { 
                TheBackEndManager.Instance.BackEndErrorCode(callback);
                Debug.LogError("GetContents : Fail : " + callback);
                yield break;
            }

            LitJson.JsonData tableListJson = callback.GetReturnValuetoJSON()["tables"];

            int totalTableCount = tableListJson.Count;
            int completeTableCount = 0;

            for (int i = 0; i < tableListJson.Count; i++)
            {
                string tableName = tableListJson[i]["tableName"].ToString();
                string typeName = $"GameBerry.Table.{tableName}Table, Assembly-CSharp";

                System.Type type = System.Type.GetType(typeName, throwOnError: false);
                if (type != null && typeof(TableBase).IsAssignableFrom(type))
                {
                    if (TableData.ContainsKey(type))
                    {
                        Debug.LogWarning($"Duplicate table type detected: {tableName}");
                        continue;
                    }

                    var table = (TableBase)Activator.CreateInstance(type);
                    table.TableName = tableName;

                    TableData.Add(type, table);

                    GetTableData_Task(table, () => 
                    { 
                        completeTableCount++; 
                        process?.Invoke(totalTableCount, completeTableCount); 
                    }).Forget();
                }
                else
                {
                    Debug.LogError($"{typeName} is invalid Table class");
                }
            }
        }
        //------------------------------------------------------------------------------------
        private static async UniTask GetTableData_Task(TableBase tableBase, Action action)
        {
            BackendReturnObject callback = null;

            Backend.PlayerData.GetMyData(tableBase.TableName, bro => { callback = bro; });

            await UniTask.WaitUntil(() => callback != null);

            action?.Invoke();

            if (callback.IsSuccess() == false)
            {
                BackEndErrorCode(callback);
                Debug.Log("데이터 읽기 중에 문제가 발생했습니다 : " + callback.ToString());
            }
            else
            {
                var data = callback.FlattenRows();

                // 불러오기에는 성공했으나 데이터가 존재하지 않는 경우
                if (data.Count <= 0)
                    Debug.Log("데이터가 존재하지 않습니다");

                tableBase.SetData(data);
            }
        }
        //------------------------------------------------------------------------------------
        public static bool TryGet<T>(out T chart) where T : TableBase
        {
            if (TableData.TryGetValue(typeof(T), out var table))
            {
                chart = (T)table;
                return true;
            }

#if UNITY_EDITOR
            Debug.LogError($"{typeof(T).Name} is null");
#endif
            chart = null;
            return false;
        }
        //------------------------------------------------------------------------------------
        public static T Get<T>() where T : TableBase
        {
            TableBase table;
            if (TableData.TryGetValue(typeof(T), out table))
                return (T)table;

            Debug.LogErrorFormat("{0} is null", typeof(T).Name);
            return null;
        }
        //------------------------------------------------------------------------------------
        public static TableBase Get(Type type)
        {
            TableBase table;
            if (TableData.TryGetValue(type, out table))
                return table;

            Debug.LogError($"{type.Name} not found");
            return null;
        }
        //------------------------------------------------------------------------------------
        private static bool CheckNetworkState()
        {
            return GameBerry.TheBackEnd.TheBackEndManager.Instance.CheckNetworkState();
        }
        //------------------------------------------------------------------------------------
        private static void BackEndErrorCode(BackendReturnObject backendReturnObject)
        {
            GameBerry.TheBackEnd.TheBackEndManager.Instance.BackEndErrorCode(backendReturnObject);
        }
        //------------------------------------------------------------------------------------
        #region Table Update
        //------------------------------------------------------------------------------------
        public static void AddUpdateWaitDatas<T>() where T : TableBase
        { // 
            CheckNetworkState();

            TableBase tableBase = Get<T>();

            if (tableBase == null)
                return;

            tableBase.UpdateWaitData = true;

            int waitcount = 0;

            foreach (var pair in TableData)
            {
                if (pair.Value.UpdateWaitData == true)
                    waitcount++;
            }

            if (waitcount >= 10)
            {
                SendUpdateWaitTable();
            }
        }
        //------------------------------------------------------------------------------------
        public static void SendUpdateWaitTable(bool allSend = false)
        {
            if (CheckNetworkState() == false)
                return;

            List<TransactionValue> transactionList = new List<TransactionValue>();

            int waitcount = 0;

            foreach (var pair in TableData)
            {
                if (pair.Value.UpdateWaitData == false)
                    continue;

                transactionList.Add(GetTransactionValue(pair.Value));

                waitcount++;
                pair.Value.UpdateWaitData = false;

                if (waitcount >= 10)
                    break;
            }

            SendTransaction(transactionList, null);

            updateWaitDataTimer = Time.time + updateWaitDataTimerTurm;

            if (allSend == true)
            {
                if (waitcount > 0)
                    SendUpdateWaitTable(true);
            }
        }
        //------------------------------------------------------------------------------------
        public static void TransactionUpdate(List<TableBase> tableBases, Action<BackendReturnObject> action = null)
        {
            if (tableBases == null)
                return;

            if (CheckNetworkState() == false)
            {
                AddReconnectUpdate(tableBases, action);
                return;
            }

            List<TransactionValue> transactionList = GetTransactionValues(tableBases);

            SendTransaction(transactionList, action);
        }
        //------------------------------------------------------------------------------------
        public static void TransactionUpdate_WaitSecond(List<TableBase> tableBases, Action<BackendReturnObject> action = null)
        { // 주의 action 저장은 테이블 첫 콜백에 대한것만 사용하므로, 같은 테이블이름에 다른 함수포인터를 사용하면 런타임에러를 만들 수 있음
            if (tableBases == null)
                return;

            if (dynamicUpdateData_Wait1Second.ContainsKey(tableBases) == true)
            {
                UpdateDataWaitStruct updateDataWaitStruct = dynamicUpdateData_Wait1Second[tableBases];
                updateDataWaitStruct.SendTime = Time.time + 1.0f;
            }
            else
            {
                UpdateDataWaitStruct updateDataWaitStruct = updateDataWaitStruct_Pool.GetObject() ?? new UpdateDataWaitStruct();
                updateDataWaitStruct.tableBases = tableBases;
                updateDataWaitStruct.SendTime = Time.time + 1.0f;
                updateDataWaitStruct.CallBack = action;
                updateDataWaitStruct.PlayTimer().Forget();
                dynamicUpdateData_Wait1Second.Add(tableBases, updateDataWaitStruct);
            }
        }
        //------------------------------------------------------------------------------------
        private static void AddReconnectUpdate(List<TableBase> tableBases, Action<BackendReturnObject> action)
        {
            if (reconnectUpdateData.ContainsKey(tableBases) == true)
                return;

            UpdateDataWaitStruct updateDataWaitStruct = updateDataWaitStruct_Pool.GetObject() ?? new UpdateDataWaitStruct();
            updateDataWaitStruct.tableBases = tableBases;
            updateDataWaitStruct.SendTime = Time.time + 1.0f;
            updateDataWaitStruct.CallBack = action;

            reconnectUpdateData.Add(tableBases, updateDataWaitStruct);
        }
        //------------------------------------------------------------------------------------
        public static void SendReconnectUpdate()
        {
            if (reconnectUpdateData.Count > 0)
            {
                foreach (var pair in reconnectUpdateData)
                {
                    UpdateDataWaitStruct updateDataWaitStruct = pair.Value;

                    TransactionUpdate(updateDataWaitStruct.tableBases, updateDataWaitStruct.CallBack);
                    updateDataWaitStruct_Pool.PoolObject(updateDataWaitStruct);
                }

                reconnectUpdateData.Clear();
            }
        }
        //------------------------------------------------------------------------------------
        private static List<TransactionValue> GetTransactionValues(List<TableBase> tableBases)
        {
            if (tableBases == null)
                return null;

            List<TransactionValue> transactionList = new List<TransactionValue>();

            for (int i = 0; i < tableBases.Count; ++i)
            {
                transactionList.Add(GetTransactionValue(tableBases[i]));
            }

            return transactionList;
        }
        //------------------------------------------------------------------------------------
        private static TransactionValue GetTransactionValue(TableBase tableBase)
        {
            string tableName = tableBase.TableName;
            string InData = tableBase.InData;

            if (string.IsNullOrEmpty(InData) == true)
                return TransactionValue.SetInsert(tableName, tableBase.GetParam());

            return TransactionValue.SetUpdateV2(tableName, InData, Backend.UserInDate, tableBase.GetParam());
        }
        //------------------------------------------------------------------------------------
        private static void SendTransaction(List<TransactionValue> transactionValues, Action<BackendReturnObject> action)
        {
            if (isCheatingUser == true)
                return;

            if (transactionValues == null)
                return;

            if (transactionValues.Count <= 0)
                return;

            SendTransaction_Task(transactionValues, action).Forget();
        }
        //------------------------------------------------------------------------------------
        private static async UniTask SendTransaction_Task(List<TransactionValue> transactionValues, Action<BackendReturnObject> action)
        {
            BackendReturnObject callback = null;

            Backend.GameData.TransactionWriteV2(transactionValues, bro => { callback = bro; });

            await UniTask.WaitUntil(() => callback != null);

            if (callback.IsSuccess() == false)
            {
                BackEndErrorCode(callback);
            }
            else
            {
                action?.Invoke(callback);
                
                for (int i = 0; i < transactionValues.Count; ++i)
                { 
                    TransactionValue transactionValue = transactionValues[i];
                    if (transactionValue.action == TransactionAction.Put)
                    {
                        // Insert한 데이터들 InData셋팅
                        var data = callback.GetReturnValuetoJSON();

                        // 불러오기에는 성공했으나 데이터가 존재하지 않는 경우
                        if (data.Count <= 0)
                        { 
                            Debug.Log("데이터가 존재하지 않습니다");
                            continue;
                        }

                        if (data.ContainsKey("putItem"))
                        {
                            var putItem = data["putItem"];
                            for (int j = 0; j < putItem.Count; ++j)
                            {
                                if (putItem[j]["table"].ToString() == transactionValue.table)
                                {
                                    string chartClass = $"GameBerry.Table.{transactionValue.table}Table, Assembly-CSharp";
                                    System.Type type = System.Type.GetType(chartClass);
                                    if (type == null)
                                    {
                                        Debug.LogError($"{transactionValue.table} is null");
                                    }
                                    else
                                    {
                                        Get(type)?.SetInData(putItem[j]["inDate"].ToString());
                                    }
                                    break;
                                }
                            }
                        }

                        
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
        public static void AllUpdata()
        {
            SendUpdateWaitTable(true);

            if (dynamicUpdateData_Wait1Second.Count > 0)
            {
                foreach (var pair in dynamicUpdateData_Wait1Second)
                {
                    UpdateDataWaitStruct updateDataWaitStruct = pair.Value;

                    TransactionUpdate(updateDataWaitStruct.tableBases, updateDataWaitStruct.CallBack);
                    updateDataWaitStruct_Pool.PoolObject(updateDataWaitStruct);
                }
            }

            dynamicUpdateData_Wait1Second.Clear();

            if (SendQueue.UnprocessedFuncCount > 0)
            {
                SendQueue.Poll();
            }
        }
        //------------------------------------------------------------------------------------
        public static void UpdateTable<T>() where T : TableBase
        {
            Get<T>()?.UpdateTable();
        }
        //------------------------------------------------------------------------------------
        public static void Updated()
        {
            if (updateWaitDataTimer < Time.time)
            {
                SendUpdateWaitTable(true);

                updateWaitDataTimer = Time.time + updateWaitDataTimerTurm;
            }
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Table Insert/Delete
        //------------------------------------------------------------------------------------
        public static void InsertTable(string tableName, Param param, System.Action<BackendReturnObject> action = null)
        {
            SendQueue.Enqueue(Backend.GameData.Insert, tableName, param, (callback) =>
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
            foreach (var pair in TableData)
            {
                SendQueue.Enqueue(Backend.GameData.DeleteV2, pair.Value.TableName, pair.Value.InData, Backend.UserInDate, (bro) =>
                {
                    if (bro.IsSuccess() == false)
                    {
                        TheBackEndManager.Instance.BackEndErrorCode(bro);
                        return;
                    }

                    Debug.LogWarning(string.Format("{0} 테이블 삭제 완료", pair.Value.TableName));
                });
            }

            TableData.Clear();

            PlayerPrefs.DeleteAll();
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region OtherUserTableData
        //------------------------------------------------------------------------------------
        public static void GetOtherUserData(string tableName, string userindata, System.Action<JsonData> action = null)
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
        public static void GetOtherUserData_NickName(string tableName, string nickName, System.Action<JsonData> action = null)
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

                GetOtherUserData(tableName, bro.GetReturnValuetoJSON()["row"]["inDate"].ToString(), action);
            });
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
    }
}