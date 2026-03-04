using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using BACKND.Database;
using LitJson;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;
using GameBerry.DB;
using UnityEngine.Purchasing;
using GameBerry.Contents;

namespace GameBerry
{
    [Table("currencytest", TableType.UserTable, ClientAccess = true, ReadPermissions = new[] { TablePermission.SELF }, WritePermissions = new[] { TablePermission.SELF })]
    public class currencytest : BaseModel
    {
        [PrimaryKey]
        [Column("owner", DatabaseType.String, NotNull = true, DefaultValue = "id")]
        public string Owner { get; set; } = "id";

        [Column("dia", DatabaseType.Int32, NotNull = true, DefaultValue = "0")]
        public int Dia { get; set; } = 0;

        [Column("gold", DatabaseType.Int32, NotNull = true, DefaultValue = "0")]
        public int Gold { get; set; } = 0;
    }

    public class ItemDataManager : Singleton<ItemDataManager>
    {
        public currencytest myItemData;

        public async UniTask LoadData()
        {
            var itemdata = await Database.DBClient.From<currencytest>().Where(x => x.Owner == Backend.UserInDate)
            .FirstOrDefault();
            if (itemdata == null)
            {
                Debug.Log("No item data found for the user. Creating new item data.");
                myItemData = new currencytest
                {
                    Dia = 0, // Starting diamonds
                    Gold = 0, // Starting gold
                    Owner = Backend.UserInDate
                };
                await Database.DBClient.From<currencytest>().Insert(myItemData);
            }
            else
            {
                Debug.Log($"Item data found for the user: Dia={itemdata.Dia}, Gold={itemdata.Gold}");
                myItemData = itemdata;
            }
        }
    }
}