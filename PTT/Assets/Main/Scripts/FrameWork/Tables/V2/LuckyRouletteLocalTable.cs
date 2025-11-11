using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class LuckyRouletteData
    {
        public ObscuredInt Index;
        public ObscuredBool IsRare;

        public ObscuredDouble SelectionWeight;

        public RewardData RewardData = new RewardData();
    }

    public class LuckyRouletteLocalTable : LocalTableBase
    {
        public List<LuckyRouletteData> luckytRouletteDatas = new List<LuckyRouletteData>();
        public WeightedRandomPicker<LuckyRouletteData> luckytRouletteDataPicker = new WeightedRandomPicker<LuckyRouletteData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("LuckyRoulette", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                LuckyRouletteData luckytRouletteData = new LuckyRouletteData();
                luckytRouletteData.Index = rows[i]["Index"].ToString().Replace(",", "").ToInt();
                luckytRouletteData.IsRare = rows[i]["IsRare"].ToString().ToInt() == 1;

                luckytRouletteData.SelectionWeight = rows[i]["SelectionWeight"].ToString().ToDouble();

                luckytRouletteData.RewardData.V2Enum_Goods = rows[i]["RewardGoodsType"].ToString().ToInt().IntToEnum32<V2Enum_Goods>();
                luckytRouletteData.RewardData.Index = rows[i]["RewardGoodsParam1"].ToString().ToInt();
                luckytRouletteData.RewardData.Amount = rows[i]["RewardGoodsParam2"].ToString().ToDouble();

                luckytRouletteDataPicker.Add(luckytRouletteData, luckytRouletteData.SelectionWeight);
                luckytRouletteDatas.Add(luckytRouletteData);
            }
        }
    }
}