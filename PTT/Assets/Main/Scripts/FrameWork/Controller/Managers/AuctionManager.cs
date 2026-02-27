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
    [Table("auction", TableType.FlexibleTable)]
    public class Auction : BaseModel
    {
        [PrimaryKey(AutoIncrement = true)]
        [Column("id", DatabaseType.Int32, NotNull = true)]
        public int Id { get; set; }

        [Column("itemid", DatabaseType.Int32, NotNull = true, DefaultValue = "0")]
        public int Itemid { get; set; } = 0;

        [Column("endtime", DatabaseType.DateTime, NotNull = true, DefaultValue = "NOW()")]
        public DateTime Endtime { get; set; } = DateTime.Now;

        [Column("amount", DatabaseType.Int32, NotNull = true, DefaultValue = "0")]
        public int Amount { get; set; } = 0;

        [Column("price", DatabaseType.Int32, NotNull = true, DefaultValue = "0")]
        public int Price { get; set; } = 0;

        [Column("issold", DatabaseType.Bool, NotNull = true, DefaultValue = "false")]
        public bool Issold { get; set; } = false;

        [Column("owner", DatabaseType.String, NotNull = true, DefaultValue = "id")]
        public string Owner { get; set; } = "id";
    }

    public class AuctionManager : Singleton<AuctionManager>
    {
        private static int diaId = 1001; // ?ˆì‹œë¡??¤ì´?„ë°¡???„ì´??IDë¥??¤ì •

        private void ShowInfo(string message)
        {
            GlobalContent.ShowPopup_Ok("Auction", message);
        }

        private void ShowError(string message)
        {
            GlobalContent.ShowPopup_Ok("Auction Error", message);
        }

        public async UniTask AddAuction(int itemid, int price, int amount)
        {

            if (amount > ItemManager.Instance.GetItemAmount(itemid))
            {
                ShowError("Not enough items to put up for auction.");
                return;
            }

            var auction = new Auction
            {
                Itemid = itemid,
                Amount = amount,
                Price = price,
                Issold = false,
                Owner = Backend.UserInDate
            };

            var result = await Database.DBClient.From<Auction>().Insert(auction);
            ItemManager.Instance.ConsumeItem(auction.Itemid, auction.Amount);
            ShowInfo(result.Message);
        }

        public async UniTask<List<Auction>> GetAllMyAuctionItems()
        {
            return await Database.DBClient.From<Auction>()
                .Where(x => x.Owner == Backend.UserInDate)
                .ToList();
        }

        public async UniTask<List<Auction>> GetAuctionItems(int itemId)
        {
            return await Database.DBClient.From<Auction>()
                .Where(x => x.Itemid == itemId)
                .ToList();
        }

        public async UniTask BuyAuctionItem(Auction selectedAuction)
        {
            if (selectedAuction == null || selectedAuction.Issold)
            {
                ShowError("Selected auction item is either null or already sold.");
                return;
            }

            if (selectedAuction.Owner == Backend.UserInDate)
            {
                ShowError("Cannot buy your own auction item.");
                return;
            }

            if (selectedAuction.Price > ItemManager.Instance.GetItemAmount(diaId))
            {
                ShowError("Not enough currency to buy this item.");
                return;
            }

            var auction = await Database.DBClient.From<Auction>()
                .Where(x => x.Id == selectedAuction.Id)
                .First();

            if (auction != null && !auction.Issold)
            {
                auction.Issold = true;
                await Database.DBClient.From<Auction>().Update(auction);

                ItemManager.Instance.AddItem(auction.Itemid, auction.Amount);
                ItemManager.Instance.ConsumeItem(diaId, auction.Price);

                ShowInfo($"Bought item: {auction.Itemid}, Amount: {auction.Amount}, Price: {auction.Price}");
            }
            else
            {
                ShowError("Auction item not found or already sold.");
            }
        }

        public async UniTask CancelAuctionItem(Auction selectedAuction)
        {
            if (selectedAuction == null || selectedAuction.Issold)
            {
                ShowError("Selected auction item is either null or already sold.");
                return;
            }

            if (selectedAuction.Owner != Backend.UserInDate)
            {
                ShowError("Cannot cancel an auction item that you do not own.");
                return;
            }

            var auction = await Database.DBClient.From<Auction>()
                .Where(x => x.Id == selectedAuction.Id)
                .First();

            if (auction != null && !auction.Issold)
            {
                await Database.DBClient.From<Auction>().Where(x => x.Id == auction.Id).Delete();
                ItemManager.Instance.AddItem(auction.Itemid, auction.Amount);

                ShowInfo($"Cancelled auction item: {auction.Itemid}, Amount: {auction.Amount}");
            }
            else
            {
                ShowError("Auction item not found or already sold.");
            }
        }

        public async UniTask RecvMySoldAuctionItems()
        {
            var soldItems = await Database.DBClient.From<Auction>()
                .Where(x => x.Owner == Backend.UserInDate)
                .Where(x => x.Issold == true)
                .ToList();

            TransactionBuilder transaction = Database.DBClient.Transaction();

            int AddAmount = 0;
            int soldCount = soldItems.Count;

            foreach (var item in soldItems)
            {
                AddAmount += item.Price;

                transaction.From<Auction>()
                    .Where(x => x.Id == item.Id)
                    .Delete();
            }

            ItemManager.Instance.AddItem(diaId, AddAmount);

            var result = await transaction.Commit();

            if (result.Success)
            {
                ShowInfo($"Sold auction items processed successfully. Count: {soldCount}, Amount: {AddAmount}");
            }
            else
            {
                ShowError($"Failed to process sold auction items: {result}");
            }
        }
    }
}
