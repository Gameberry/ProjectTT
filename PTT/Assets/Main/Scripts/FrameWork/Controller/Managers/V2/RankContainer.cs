using System.Collections;
using System.Collections.Generic;
using Gpm.Ui;
using System.Text;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class MyRankInfo
    {
        public ObscuredDouble CombatPower = 0.0;
        public ObscuredInt Stage = 0;

        public RankDetailInfo Detail = new RankDetailInfo();
    }

    public struct RankDetailInfo
    {
        public string combatpower;
        public string stage;

        public int famestep;
        public int famelv;

        public Dictionary<V2Enum_GearType, ObscuredInt> gearEquipId;
        public Dictionary<int, ObscuredInt> ringEquipId;

        public List<List<ObscuredInt>> allyDetailData;

        public ObscuredInt clanlevel;

        public ObscuredInt profile;

        public string server;

        public string detailst;
        public StringBuilder detailStr;

        public bool needRefreshString;

        public void SetDetailInfo(string info)
        {
            //if (string.IsNullOrEmpty(info))
            //    return;

            //string[] infos = info.Split('/');

            //needRefreshString = true;

            //try
            //{
            //    gearEquipId = new Dictionary<V2Enum_Goods, ObscuredInt>();
            //    ringEquipId = new Dictionary<int, ObscuredInt>();
            //    allyDetailData = new List<List<ObscuredInt>>();

            //    combatpower = infos[0];
            //    stage = infos[1].ToInt();

            //    string[] famearr = infos[2].Split(',');

            //    famestep = famearr[0].ToInt();
            //    famelv = famearr[1].ToInt();

            //    if (gearEquipId != null)
            //        gearEquipId.Clear();
            //    else
            //        gearEquipId = new Dictionary<V2Enum_Goods, ObscuredInt>();

            //    int gearstartidx = V2Enum_Goods.Gear.Enum32ToInt();
            //    string[] geararr = infos[3].Split(',');

            //    for (int kind = V2Enum_Goods.Gear.Enum32ToInt(); kind <= (int)V2Enum_Goods.Belt; ++kind)
            //    {
            //        V2Enum_Goods v2Enum_Goods = kind.IntToEnum32<V2Enum_Goods>();
            //        int idx = kind - gearstartidx;

            //        int index = geararr[idx].ToInt();

            //        if (index == -1 || index == 0)
            //            gearEquipId.Add(v2Enum_Goods, index);
            //        else
            //        {
            //            gearEquipId.Add(v2Enum_Goods, index + 107010000);
            //        }
            //    }

            //    if (allyDetailData != null)
            //        allyDetailData.Clear();
            //    else
            //        allyDetailData = new List<List<ObscuredInt>>();


            //    string[] allyarr = infos[5].Split(';');

            //    for (int i = 0; i < allyarr.Length; ++i)
            //    {
            //        string[] allystat = allyarr[i].Split(',');

            //        if (allystat.Length >= 3)
            //        {
            //            List<ObscuredInt> allydetails = new List<ObscuredInt>();
            //            allydetails.Add(allystat[0].ToInt() + 104010000);
            //            allydetails.Add(allystat[1].ToInt());
            //            allydetails.Add(allystat[2].ToInt());

            //            for (int j = 0; j < allystat.Length; ++j)
            //            {
            //                allydetails.Add(allystat[j].ToInt());
            //            }

            //            allyDetailData.Add(allydetails);
            //        }
            //    }

            //    if (infos.Length >= 8)
            //    {
            //        clanlevel = infos[6].ToInt();
            //    }
            //    else
            //        clanlevel = 0;
                
            //    if (infos.Length >= 9)
            //    {
            //        profile = infos[7].ToInt();
            //    }
            //    else
            //        profile = 0;

            //    if (infos.Length >= 10)
            //    {
            //        server = infos[8];
            //    }
            //    else
            //        server = "-";
                
            //}
            //catch
            //{

            //}
        }

        public void SetDetailString()
        {
        //    if (detailStr == null)
        //        detailStr = new StringBuilder();

        //    detailStr.Clear();
        //    detailStr.Append(combatpower);
        //    detailStr.Append('/');
        //    detailStr.Append(stage);
        //    detailStr.Append('/');
        //    detailStr.Append(famestep);
        //    detailStr.Append(',');
        //    detailStr.Append(famelv);
        //    detailStr.Append('/');

        //    for (int kind = V2Enum_Goods.Gear.Enum32ToInt(); kind <= (int)V2Enum_Goods.Belt; ++kind)
        //    {
        //        V2Enum_Goods v2Enum_Goods = kind.IntToEnum32<V2Enum_Goods>();

        //        if (gearEquipId.ContainsKey(v2Enum_Goods) == false)
        //            detailStr.Append(-1);
        //        else
        //        {
        //            int index = gearEquipId[v2Enum_Goods].GetDecrypted();
        //            if (index == -1 || index == 0)
        //                detailStr.Append(index);
        //            else
        //            { 
        //                detailStr.Append(index - 107010000);
        //            }
        //        }

        //        if(v2Enum_Goods != V2Enum_Goods.Belt)
        //            detailStr.Append(',');
        //    }

        //    detailStr.Append('/');


        //    detailStr.Append('/');

        //    detailStr.Append('/');

        //    detailStr.Append(clanlevel.GetDecrypted());

        //    detailStr.Append('/');
        //    detailStr.Append(profile.GetDecrypted());

        //    detailStr.Append('/');
        //    detailStr.Append(PlayerDataContainer.DisplayServerName);

        //    needRefreshString = false;
        }

        public override string ToString()
        {
            if (needRefreshString == true)
                SetDetailString();

            if (detailStr == null)
                detailStr = new StringBuilder();

            return detailStr.ToString();
        }
    }

    public class RankTable
    {
        public string uuid;
        public string title;
        public string table;
        public string column;

        public int MyRank;
        public double Score = -1;
        public long TotalCount;
    }

    public class RankerData : InfiniteScrollData
    {
        public V2Enum_RankType v2Enum_RankType;
        public string nickName;
        public double score;
        public int rank;
        public string detail;
        public string gamerInData;
    }

    public static class RankContainer
    {
        public static MyRankInfo MyRankInfo = new MyRankInfo();

        public static Dictionary<V2Enum_RankType, RankTable> rankTableData = new Dictionary<V2Enum_RankType, RankTable>();

        public static List<RankerData> RankerDatas = new List<RankerData>();

        public static bool IsContainServer(int myServer, int orderServer)
        {
            if (myServer >= 107)
                return myServer == orderServer;

            return orderServer != 107;
        }
    }
}