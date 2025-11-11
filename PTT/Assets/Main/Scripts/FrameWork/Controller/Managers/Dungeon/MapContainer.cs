using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class StageInfo
    {
        public ObscuredInt StageNumber;
        public ObscuredInt LastClearWave;
        public ObscuredInt RecvClearReward;
    }

    public static class MapContainer
    {
        public static Dictionary<ObscuredInt, StageInfo> StageInfos = new Dictionary<ObscuredInt, StageInfo>();

        public static ObscuredInt LastFailWave = -1; // 서버 저장 아님. 상품 이벤트 트리거용

        public static ObscuredInt MaxWaveClear = 0;
        public static ObscuredInt MapMaxClear = -1;
        public static ObscuredInt MapLastEnter = -1; // 서버에 저장하는 데이터는 아니다. 유저 편의용 데이터

        public static ObscuredString MapEnterKey = string.Empty;

        public static ObscuredDouble EventDayInitTime = 0; // 일일초기화
        public static ObscuredInt ToDaySweepCount = 0;
        public static ObscuredInt ToDayDoubleRewardCount = 0;

        public static System.Text.StringBuilder SerializeString = new System.Text.StringBuilder();

        // MapKey
        public static ObscuredString MapKey = "mapkey";
        public static ObscuredString Mapidxkey = "mapidxkey";

        public static ObscuredInt PlayingStage = -1;
        public static ObscuredInt PlayingWave = -1;

        public static int GetLogStage()
        {
            if (PlayingStage == -1)
                return MaxWaveClear / 100;

            return PlayingStage;
        }

        public static int GetLogWave()
        {
            if (PlayingStage == -1)
                return MaxWaveClear % 100;

            return PlayingWave;
        }

        public static string GetSkillInfoSerializeString()
        {
            SerializeString.Clear();

            foreach (var pair in StageInfos)
            {
                SerializeString.Append(pair.Value.StageNumber);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.LastClearWave);
                SerializeString.Append(',');
                SerializeString.Append(pair.Value.RecvClearReward);
                SerializeString.Append('/');
            }

            if (SerializeString.Length > 0)
                SerializeString.Remove(SerializeString.Length - 1, 1);

            return SerializeString.ToString();
        }

        public static void SetSkillInfoDeSerializeString(string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;

            string[] arr = data.Split('/');

            for (int i = 0; i < arr.Length; ++i)
            {
                string[] arrcontent = arr[i].Split(',');

                StageInfo stageInfo = new StageInfo();
                stageInfo.StageNumber = arrcontent[0].ToInt();
                stageInfo.LastClearWave = arrcontent[1].ToInt();
                stageInfo.RecvClearReward = arrcontent[2].ToInt();

                StageInfos.Add(stageInfo.StageNumber, stageInfo);
            }
        }
    }

    public static class MapOperator
    {
        private static MapLocalTable _mapLocalTable = null;

        //------------------------------------------------------------------------------------
        public static void Init()
        {
            _mapLocalTable = Managers.TableManager.Instance.GetTableClass<MapLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public static MapData GetMapData(ObscuredInt number)
        {
            return _mapLocalTable.GetMapData(number);
        }
        //------------------------------------------------------------------------------------
        public static ObscuredInt GetMinMapNumber()
        {
            return _mapLocalTable.GetMinMapNumber();
        }
        //------------------------------------------------------------------------------------
        public static ObscuredInt GetMaxMapNumber()
        {
            return _mapLocalTable.GetMaxMapNumber();
        }
        //------------------------------------------------------------------------------------
        public static WaveRewardData GetWaveRewardData(ObscuredInt index)
        {
            return _mapLocalTable.GetWaveRewardData(index);
        }
        //------------------------------------------------------------------------------------
        public static WaveRewardData GetWaveRewardData(ObscuredInt stage, ObscuredInt wave)
        {
            return _mapLocalTable.GetWaveRewardData(stage, wave);
        }
        //------------------------------------------------------------------------------------
        public static WaveClearRewardData GetWaveClearRewardData(ObscuredInt stage, ObscuredInt wave)
        {
            return _mapLocalTable.GetWaveClearRewardData(stage, wave);
        }
        //------------------------------------------------------------------------------------
        public static Dictionary<ObscuredInt, WaveClearRewardData> GetWaveClearsRewardData(ObscuredInt stage)
        {
            return _mapLocalTable.GetWaveClearsRewardData(stage);
        }
        //------------------------------------------------------------------------------------
        public static List<WaveClearRewardData> GetAllWaveClearRewardData()
        {
            return _mapLocalTable.GetAllWaveClearRewardData();
        }
        //------------------------------------------------------------------------------------
        public static string ConvertWaveTotalNumberToUIString(int totalnumber)
        {
            int stage = totalnumber / 100;
            int wave = totalnumber % 100;

            return string.Format("{0}-{1}", stage, wave);
        }
        //------------------------------------------------------------------------------------
    }
}