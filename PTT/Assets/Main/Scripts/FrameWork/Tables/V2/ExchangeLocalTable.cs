using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using LitJson;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class ExchangeData
    {
        public int Index;
        public int ResourceIndex;

        public V2Enum_Goods ReturnGoodsType;
        public int ReturnGoodsParam1;
        public double ReturnGoodsParam2;

        public RewardData ReturnGoods = new RewardData();

        public V2Enum_Goods MaterialGoodsType;
        public int MaterialGoodsParam1;
        public double MaterialGoodsParam2;

        public RewardData MaterialGoods = new RewardData();

        public V2Enum_Goods CostGoodsType;
        public int CostGoodsParam1;
        public double CostGoodsParam2;

        public int ExchangeLimit;

        public string NameLocalStringKey;
    }

    public class ExchangeLocalTable : LocalTableBase
    {
        public List<ExchangeData> m_exchangeDatas = new List<ExchangeData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            m_exchangeDatas = await TheBackEnd.TheBackEnd_GameChart.GetListDat_Async<ExchangeData>("Exchange");

            for (int i = 0; i < m_exchangeDatas.Count; ++i)
            {
                ExchangeData exchangeData = m_exchangeDatas[i];

                exchangeData.ReturnGoods.V2Enum_Goods = exchangeData.ReturnGoodsType;
                exchangeData.ReturnGoods.Index = exchangeData.ReturnGoodsParam1;
                exchangeData.ReturnGoods.Amount = exchangeData.ReturnGoodsParam2;

                exchangeData.MaterialGoods.V2Enum_Goods = exchangeData.MaterialGoodsType;
                exchangeData.MaterialGoods.Index = exchangeData.MaterialGoodsParam1;
                exchangeData.MaterialGoods.Amount = exchangeData.MaterialGoodsParam2;

                exchangeData.NameLocalStringKey = string.Format("exchange/{0}/name", exchangeData.ResourceIndex);
            }
        }
        //------------------------------------------------------------------------------------
        public List<ExchangeData> GetDatas()
        {
            return m_exchangeDatas;
        }
        //------------------------------------------------------------------------------------
    }
}