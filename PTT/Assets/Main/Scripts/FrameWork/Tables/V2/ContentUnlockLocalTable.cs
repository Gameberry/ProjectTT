using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class ContentUnlockData
    {
        public int Index;
        //public int ResourceIndex;
        public V2Enum_ContentType ContentsType;
        public V2Enum_OpenConditionType UnlockConditionType;
        public int UnlockConditionParam;
        public int IsUnlockNotice;
        public int IsDisplay;
    }

    public class ContentsUnlockRangeData
    {
        public int Index;
        public int Param1;
        public int Param2;
    }

    public class ContentUnlockLocalTable : LocalTableBase
    {
        public Dictionary<V2Enum_ContentType, ContentUnlockData> m_contentUnlockDatas = new Dictionary<V2Enum_ContentType, ContentUnlockData>();

        public Dictionary<int, ContentsUnlockRangeData> _contentsUnlockRangeDatas = new Dictionary<int, ContentsUnlockRangeData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            List<ContentUnlockData> contentUnlockDatas = await TheBackEnd.TheBackEnd_GameChart.GetListDat_Async<ContentUnlockData>("ContentsUnlock");

            for (int i = 0; i < contentUnlockDatas.Count; ++i)
            {
                ContentUnlockData contentUnlockData = contentUnlockDatas[i];
                m_contentUnlockDatas.Add(contentUnlockData.ContentsType, contentUnlockData);
            }

            List<ContentsUnlockRangeData> contentsUnlockRangeDatas = await TheBackEnd.TheBackEnd_GameChart.GetListDat_Async<ContentsUnlockRangeData>("ContentsUnlockRange");

            for (int i = 0; i < contentsUnlockRangeDatas.Count; ++i)
            {
                ContentsUnlockRangeData contentUnlockData = contentsUnlockRangeDatas[i];
                _contentsUnlockRangeDatas.Add(contentUnlockData.Index, contentUnlockData);
            }
        }
        //------------------------------------------------------------------------------------
        public ContentUnlockData GetContentUnlockData(V2Enum_ContentType contentType)
        {
            if (m_contentUnlockDatas.ContainsKey(contentType) == true)
                return m_contentUnlockDatas[contentType];

            return null;
        }
        //------------------------------------------------------------------------------------
        public ContentsUnlockRangeData GetContentsUnlockRangeData(int index)
        {
            if (_contentsUnlockRangeDatas.ContainsKey(index) == true)
                return _contentsUnlockRangeDatas[index];

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}