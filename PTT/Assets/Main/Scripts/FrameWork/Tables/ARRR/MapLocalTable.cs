using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class MapWaveData
    {
        public ObscuredInt Index;
        public ObscuredInt StageNumber;
        public ObscuredInt WaveNumber;

        public ObscuredInt TotalNumber;

        public ObscuredInt MonsterSetIndex;
        public ObscuredInt MonsterSetLevel;
        public ObscuredInt MonsterSetIcon;
        public ObscuredDouble WaveClearGold;
        public ObscuredDouble MonsterGold;
        public V2Enum_ARR_RoomType RoomType;
        public ObscuredDouble GasReward;

        public Dictionary<V2Enum_Stat, ObscuredDouble> AddStat = new Dictionary<V2Enum_Stat, ObscuredDouble>();

        public ObscuredFloat MonsterSpawnPosition; // 처음 셋팅 시 사용되는 포지션
    }

    public class MapData
    {
        public ObscuredInt Index;
        public ObscuredInt StageNumber;
        public ObscuredDouble StartGold;

        public ObscuredInt MaxWave;

        public ObscuredInt BackGround;

        public string[] ResultLocalKey;

        public List<MapWaveData> MapWaveDatas = new List<MapWaveData>();
    }

    public class WaveRewardRangeData
    {
        public ObscuredInt Index;
        public ObscuredInt Min;
        public ObscuredInt Max;
    }

    public class WaveRewardData
    {
        public ObscuredInt Index;

        public ObscuredInt StageNumber;
        public ObscuredInt WaveNumber;

        public List<WaveRewardRangeData> WaveRewardRangeDatas = new List<WaveRewardRangeData>();
    }

    public class WaveClearRewardData
    {
        public ObscuredInt Index;

        public ObscuredInt StageNumber;
        public ObscuredInt WaveNumber;

        public RewardData PerfectClearReward = new RewardData();
    }


    public class MapLocalTable : LocalTableBase
    {
        public List<MapData> MapDatas = new List<MapData>();

        public Dictionary<ObscuredInt, MapData> MapDatas_Dic = new Dictionary<ObscuredInt, MapData>();

        public ObscuredInt MinMapNumber = -1;
        public ObscuredInt MaxMapNumber = -1;

        public Dictionary<ObscuredInt, WaveRewardData> AllWaveRewardDatas_Dic = new Dictionary<ObscuredInt, WaveRewardData>();
        public Dictionary<ObscuredInt, Dictionary<ObscuredInt, WaveRewardData>> WaveRewardDatas_Dic = new Dictionary<ObscuredInt, Dictionary<ObscuredInt, WaveRewardData>>();

        public Dictionary<ObscuredInt, Dictionary<ObscuredInt, WaveClearRewardData>> WaveClearRewardDatas_Dic = new Dictionary<ObscuredInt, Dictionary<ObscuredInt, WaveClearRewardData>>();
        public List<WaveClearRewardData> WaveClearRewardDatas = new List<WaveClearRewardData>();


        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("MapTable", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                int stageNumber = rows[i]["StageNumber"].ToString().ToInt();
                MapData mapData = null;
                if (MapDatas_Dic.ContainsKey(stageNumber) == false)
                { 
                    mapData = new MapData();
                    mapData.StageNumber = stageNumber;
                    MapDatas.Add(mapData);
                    MapDatas_Dic.Add(stageNumber, mapData);
                }
                else
                    mapData = MapDatas_Dic[stageNumber];


                MapWaveData mapWaveData = new MapWaveData();
                mapWaveData.Index = rows[i]["Index"].ToString().ToInt();
                mapWaveData.StageNumber = stageNumber;
                mapWaveData.WaveNumber = rows[i]["WaveNumber"].ToString().ToInt();
                mapWaveData.TotalNumber = rows[i]["TotalNumber"].ToString().ToInt();

                if (mapData.MaxWave < mapWaveData.WaveNumber)
                    mapData.MaxWave = mapWaveData.WaveNumber;

                string MonsterSetIndex = string.Format("MonsterSetIndex");
                int monsterSetIndex = rows[i][MonsterSetIndex].ToString().ToInt();
                if (monsterSetIndex == -1 || monsterSetIndex == 0)
                    continue;

                string MonsterSetLevel = string.Format("MonsterSetLevel");
                string MonsterSetIcon = string.Format("MonsterSetIcon");
                string WaveClearGold = string.Format("WaveClearGold");
                string MonsterGold = string.Format("MonsterGold");
                string RoomType = string.Format("RoomType");
                string GasReward = string.Format("GasReward");


                string AddAtt = string.Format("AddAtt");
                string AddHP = string.Format("AddHP");
                string AddDef = string.Format("AddDef");

                mapWaveData.MonsterSetIndex = monsterSetIndex;
                mapWaveData.MonsterSetLevel = rows[i][MonsterSetLevel].ToString().ToInt();
                mapWaveData.MonsterSetIcon = rows[i][MonsterSetIcon].ToString().ToInt();
                mapWaveData.WaveClearGold = rows[i][WaveClearGold].ToString().ToDouble();
                mapWaveData.MonsterGold = rows[i][MonsterGold].ToString().ToDouble();
                mapWaveData.RoomType = rows[i][RoomType].ToString().ToInt().IntToEnum32<V2Enum_ARR_RoomType>();
                mapWaveData.GasReward = rows[i][GasReward].ToString().ToDouble();

                mapWaveData.AddStat.Add(V2Enum_Stat.Attack, rows[i][AddAtt].ToString().ToDouble());
                mapWaveData.AddStat.Add(V2Enum_Stat.HP, rows[i][AddHP].ToString().ToDouble());
                mapWaveData.AddStat.Add(V2Enum_Stat.Defence, rows[i][AddDef].ToString().ToDouble());

                mapData.MapWaveDatas.Add(mapWaveData);

                if (MinMapNumber == -1)
                    MinMapNumber = mapData.StageNumber;
                else
                {
                    if (MinMapNumber > mapData.StageNumber)
                        MinMapNumber = mapData.StageNumber;
                }

                if (MaxMapNumber == -1)
                    MaxMapNumber = mapData.StageNumber;
                else
                {
                    if (MaxMapNumber < mapData.StageNumber)
                        MaxMapNumber = mapData.StageNumber;
                }

            }

            for (int i = 0; i < MapDatas.Count; ++i)
            {
                
                MapData mapData = MapDatas[i];

                mapData.MapWaveDatas.Sort((x, y) =>
                {
                    if (x.WaveNumber < y.WaveNumber)
                        return -1;
                    else if (x.WaveNumber > y.WaveNumber)
                        return 1;

                    return 0;
                });

                for (int mapidx = 0; mapidx < mapData.MapWaveDatas.Count; ++mapidx)
                {
                    MapWaveData mapWaveData = mapData.MapWaveDatas[mapidx];

                    mapWaveData.MonsterSpawnPosition = (float)(mapidx + 1) / (float)mapData.MapWaveDatas.Count;
                }
            }



            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("WaveRewardTable", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                WaveRewardData waveRewardData = new WaveRewardData();

                waveRewardData.Index = rows[i]["Index"].ToString().ToInt();

                waveRewardData.StageNumber = rows[i]["StageNumber"].ToString().ToInt();
                waveRewardData.WaveNumber = rows[i]["WaveNumber"].ToString().ToInt();


                for (int j = 1; j <= 5; ++j)
                {
                    try
                    {
                        string TimeClearRewardIndex = string.Format("WaveClearRewardIndex{0}", j);
                        int monsterSetIndex = rows[i][TimeClearRewardIndex].ToString().ToInt();
                        if (monsterSetIndex == -1 || monsterSetIndex == 0)
                            continue;

                        string WaveClearRewardMin = string.Format("WaveClearRewardMin{0}", j);
                        string WaveClearRewardMax = string.Format("WaveClearRewardMax{0}", j);

                        WaveRewardRangeData mapWaveData = new WaveRewardRangeData();
                        mapWaveData.Index = monsterSetIndex;

                        mapWaveData.Min = rows[i][WaveClearRewardMin].ToString(). ToInt();
                        mapWaveData.Max = rows[i][WaveClearRewardMax].ToString().ToInt();

                        waveRewardData.WaveRewardRangeDatas.Add(mapWaveData);
                    }
                    catch
                    {

                    }
                }

                if (WaveRewardDatas_Dic.ContainsKey(waveRewardData.StageNumber) == false)
                    WaveRewardDatas_Dic.Add(waveRewardData.StageNumber, new Dictionary<ObscuredInt, WaveRewardData>());

                if (WaveRewardDatas_Dic[waveRewardData.StageNumber].ContainsKey(waveRewardData.WaveNumber) == false)
                    WaveRewardDatas_Dic[waveRewardData.StageNumber].Add(waveRewardData.WaveNumber, waveRewardData);

                if (AllWaveRewardDatas_Dic.ContainsKey(waveRewardData.Index) == false)
                    AllWaveRewardDatas_Dic.Add(waveRewardData.Index, waveRewardData);
            }



            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("WaveClearRewardTable", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                WaveClearRewardData waveClearRewardData = new WaveClearRewardData();

                waveClearRewardData.Index = rows[i]["Index"].ToString().ToInt();

                waveClearRewardData.StageNumber = rows[i]["StageNumber"].ToString().ToInt();
                waveClearRewardData.WaveNumber = rows[i]["WaveNumber"].ToString().ToInt();


                waveClearRewardData.PerfectClearReward.Index = rows[i]["RewardIndex"].ToString().ToInt();
                waveClearRewardData.PerfectClearReward.Amount = rows[i]["RewardValue"].ToString().ToInt();


                if (WaveClearRewardDatas_Dic.ContainsKey(waveClearRewardData.StageNumber) == false)
                    WaveClearRewardDatas_Dic.Add(waveClearRewardData.StageNumber, new Dictionary<ObscuredInt, WaveClearRewardData>());

                if (WaveClearRewardDatas_Dic[waveClearRewardData.StageNumber].ContainsKey(waveClearRewardData.WaveNumber) == false)
                    WaveClearRewardDatas_Dic[waveClearRewardData.StageNumber].Add(waveClearRewardData.WaveNumber, waveClearRewardData);

                WaveClearRewardDatas.Add(waveClearRewardData);
            }


            WaveClearRewardDatas.Sort((x, y) =>
            {
                if (x.StageNumber < y.StageNumber)
                    return -1;
                else if (x.StageNumber > y.StageNumber)
                    return 1;

                if (x.WaveNumber < y.WaveNumber)
                    return -1;
                else if (x.WaveNumber > y.WaveNumber)
                    return 1;

                return 0;
            });




            rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("StageTable", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            for (int i = 0; i < rows.Count; ++i)
            {
                int StageNumber = rows[i]["StageNumber"].ToString().ToInt();

                if (MapDatas_Dic.ContainsKey(StageNumber) == false)
                    continue;

                MapData mapData = MapDatas_Dic[StageNumber];
                mapData.StartGold = rows[i]["StartGold"].ToString().ToDouble();
                mapData.BackGround = rows[i]["BackGround"].ToString().ToInt();

                string mapdesckey = rows[i]["ResultLocalKey"].ToString();

                mapData.ResultLocalKey = mapdesckey.Split(",");
            }
        }
        //------------------------------------------------------------------------------------
        public MapData GetMapData(ObscuredInt number)
        {
            if (MapDatas_Dic.ContainsKey(number) == true)
                return MapDatas_Dic[number];

            return null;
        }
        //------------------------------------------------------------------------------------
        public ObscuredInt GetMinMapNumber()
        {
            return MinMapNumber;
        }
        //------------------------------------------------------------------------------------
        public ObscuredInt GetMaxMapNumber()
        {
            return MaxMapNumber;
        }
        //------------------------------------------------------------------------------------
        public WaveRewardData GetWaveRewardData(ObscuredInt index)
        {
            if (AllWaveRewardDatas_Dic.ContainsKey(index) == true)
            {
                return AllWaveRewardDatas_Dic[index];
            }

            return null;
        }
        //------------------------------------------------------------------------------------
        public WaveRewardData GetWaveRewardData(ObscuredInt stage, ObscuredInt wave)
        {
            if (WaveRewardDatas_Dic.ContainsKey(stage) == true)
            {
                if (WaveRewardDatas_Dic[stage].ContainsKey(wave) == true)
                {
                    return WaveRewardDatas_Dic[stage][wave];
                }
            }

            return null;
        }
        //------------------------------------------------------------------------------------
        public WaveClearRewardData GetWaveClearRewardData(ObscuredInt stage, ObscuredInt wave)
        {
            if (WaveClearRewardDatas_Dic.ContainsKey(stage) == true)
            {
                if (WaveClearRewardDatas_Dic[stage].ContainsKey(wave) == true)
                {
                    return WaveClearRewardDatas_Dic[stage][wave];
                }
            }

            return null;
        }
        //------------------------------------------------------------------------------------
        public Dictionary<ObscuredInt, WaveClearRewardData> GetWaveClearsRewardData(ObscuredInt stage)
        {
            if (WaveClearRewardDatas_Dic.ContainsKey(stage) == true)
            {
                return WaveClearRewardDatas_Dic[stage];
            }

            return null;
        }
        //------------------------------------------------------------------------------------
        public List<WaveClearRewardData> GetAllWaveClearRewardData()
        {
            return WaveClearRewardDatas;
        }
        //------------------------------------------------------------------------------------
    }
}