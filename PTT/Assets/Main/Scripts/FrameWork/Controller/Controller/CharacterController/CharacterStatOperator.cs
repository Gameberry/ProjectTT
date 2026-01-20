using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
#if UNITY_EDITOR

    [System.Serializable]
    public class StatViewer
    {
        public Enum_Stat v2Enum_Stat;
        public double value;
    }

    [System.Serializable]
#endif
    public class CharacterStatOperator
    {
        protected Dictionary<Enum_Stat, ObscuredDouble> _defauleStatValue = new Dictionary<Enum_Stat, ObscuredDouble>();
        protected Dictionary<Enum_Stat, ObscuredDouble> _equipmentValue = new Dictionary<Enum_Stat, ObscuredDouble>();  // 추가
        protected Dictionary<Enum_Stat, ObscuredDouble> _engravingValue = new Dictionary<Enum_Stat, ObscuredDouble>();  // 추가
        protected Dictionary<Enum_Stat, ObscuredDouble> _buffValue = new Dictionary<Enum_Stat, ObscuredDouble>();

        protected Dictionary<Enum_Stat, ObscuredDouble> _outputStatValue = new Dictionary<Enum_Stat, ObscuredDouble>();

#if UNITY_EDITOR
        public List<StatViewer> DefaultViewers = new List<StatViewer>();
        public List<StatViewer> EquipmentViewers = new List<StatViewer>();  // 추가
        public List<StatViewer> EngravingViewers = new List<StatViewer>();  // 추가
        public List<StatViewer> BuffViewers = new List<StatViewer>();
        public List<StatViewer> OutputViewers = new List<StatViewer>();
#endif

        //------------------------------------------------------------------------------------
        public void SetDefaultStat(Enum_Stat v2Enum_Stat, ObscuredDouble statValue)
        {
            if (_defauleStatValue.ContainsKey(v2Enum_Stat) == false)
                _defauleStatValue.Add(v2Enum_Stat, 0);

            _defauleStatValue[v2Enum_Stat] = statValue;

#if UNITY_EDITOR
            StatViewer statViewer = DefaultViewers.Find(x => x.v2Enum_Stat == v2Enum_Stat);
            if (statViewer == null)
            {
                statViewer = new StatViewer();
                statViewer.v2Enum_Stat = v2Enum_Stat;
                DefaultViewers.Add(statViewer);
            }

            statViewer.value = statValue;
#endif
        }
        //------------------------------------------------------------------------------------
        public double GetDefaultValue(Enum_Stat v2Enum_Stat)
        {
            if (_defauleStatValue.ContainsKey(v2Enum_Stat) == false)
                return 0;

            return _defauleStatValue[v2Enum_Stat];
        }
        //------------------------------------------------------------------------------------
        public void SetBuffValue(Enum_Stat v2Enum_Stat, ObscuredDouble statValue)
        {
            if (_buffValue.ContainsKey(v2Enum_Stat) == false)
                _buffValue.Add(v2Enum_Stat, 0);

            _buffValue[v2Enum_Stat] = statValue;

#if UNITY_EDITOR
            StatViewer statViewer = BuffViewers.Find(x => x.v2Enum_Stat == v2Enum_Stat);
            if (statViewer == null)
            {
                statViewer = new StatViewer();
                statViewer.v2Enum_Stat = v2Enum_Stat;
                BuffViewers.Add(statViewer);
            }

            statViewer.value = statValue;
#endif
        }
        //------------------------------------------------------------------------------------
        public double GetBuffValue(Enum_Stat v2Enum_Stat)
        {
            if (_buffValue.ContainsKey(v2Enum_Stat) == false)
                return 0;

            return _buffValue[v2Enum_Stat];
        }
        //------------------------------------------------------------------------------------
        public void SetEquipmentStat(Enum_Stat stat, ObscuredDouble value)
        {
            if (_equipmentValue.ContainsKey(stat) == false)
                _equipmentValue.Add(stat, 0);

            _equipmentValue[stat] = value;

#if UNITY_EDITOR
            StatViewer statViewer = EquipmentViewers.Find(x => x.v2Enum_Stat == stat);
            if (statViewer == null)
            {
                statViewer = new StatViewer();
                statViewer.v2Enum_Stat = stat;
                EquipmentViewers.Add(statViewer);
            }
            statViewer.value = value;
#endif
        }
        //------------------------------------------------------------------------------------
        public double GetEquipmentValue(Enum_Stat stat)
        {
            if (_equipmentValue.ContainsKey(stat) == false)
                return 0;

            return _equipmentValue[stat];
        }
        //------------------------------------------------------------------------------------
        public void ClearEquipmentStats()
        {
            _equipmentValue.Clear();

#if UNITY_EDITOR
            EquipmentViewers.Clear();
#endif
        }
        //------------------------------------------------------------------------------------
        public void SetEngravingStat(Enum_Stat stat, ObscuredDouble value)
        {
            if (_engravingValue.ContainsKey(stat) == false)
                _engravingValue.Add(stat, 0);

            _engravingValue[stat] = value;

#if UNITY_EDITOR
            StatViewer statViewer = EngravingViewers.Find(x => x.v2Enum_Stat == stat);
            if (statViewer == null)
            {
                statViewer = new StatViewer();
                statViewer.v2Enum_Stat = stat;
                EngravingViewers.Add(statViewer);
            }
            statViewer.value = value;
#endif
        }
        //------------------------------------------------------------------------------------
        public double GetEngravingValue(Enum_Stat stat)
        {
            if (_engravingValue.ContainsKey(stat) == false)
                return 0;

            return _engravingValue[stat];
        }
        //------------------------------------------------------------------------------------
        public void ClearEngravingStats()
        {
            _engravingValue.Clear();

#if UNITY_EDITOR
            EngravingViewers.Clear();
#endif
        }
        //------------------------------------------------------------------------------------
        public void RefreshOutputStatValue(Enum_Stat v2Enum_Stat = Enum_Stat.Max)
        {
            if (v2Enum_Stat == Enum_Stat.Max)
            {
                for (int i = Enum_Stat.Attack.Enum32ToInt(); i < Enum_Stat.Max.Enum32ToInt(); ++i)
                {
                    SetOutputStatValue(i.IntToEnum32<Enum_Stat>());
                }
            }
            else
                SetOutputStatValue(v2Enum_Stat);
        }
        //------------------------------------------------------------------------------------
        public void SetOutputStatValue(Enum_Stat v2Enum_Stat)
        { // 여기서 각 컨텐츠에서 계산되어야 할 값들을 가져와서 다시 셋팅
            double originValue = GetDefaultValue(v2Enum_Stat);
            double equipValue = GetEquipmentValue(v2Enum_Stat);  // 추가
            double engravingValue = GetEngravingValue(v2Enum_Stat);  // 추가
            double buffvalue = GetBuffValue(v2Enum_Stat);

            _outputStatValue[v2Enum_Stat] = originValue + equipValue + buffvalue;  // 수정

#if UNITY_EDITOR
            StatViewer statViewer = OutputViewers.Find(x => x.v2Enum_Stat == v2Enum_Stat);
            if (statViewer == null)
            {
                statViewer = new StatViewer();
                statViewer.v2Enum_Stat = v2Enum_Stat;
                OutputViewers.Add(statViewer);
            }
            statViewer.value = originValue + equipValue + engravingValue + buffvalue;  // 수정
#endif
        }
        //------------------------------------------------------------------------------------
        public double GetOutPutMyStat(Enum_Stat v2Enum_Stat)
        {
            if (_outputStatValue.ContainsKey(v2Enum_Stat) == false)
                return 0;

            return _outputStatValue[v2Enum_Stat];
        }

        //------------------------------------------------------------------------------------
        public void ForceReleaseStat()
        {
            _outputStatValue.Clear();
            _defauleStatValue.Clear();
            _equipmentValue.Clear();  // 추가
            _engravingValue.Clear();
        }
        //------------------------------------------------------------------------------------
    }
}