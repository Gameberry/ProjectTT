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
        public V2Enum_Stat v2Enum_Stat;
        public double value;
    }

    [System.Serializable]
#endif
    public class CharacterStatOperator
    {
        public CharacterStatOperator(CharacterControllerBase characterControllerBase)
        {
            _characterControllerBase = characterControllerBase;
        }

        protected Dictionary<V2Enum_Stat, ObscuredDouble> _defauleStatValue = new Dictionary<V2Enum_Stat, ObscuredDouble>();
        protected Dictionary<V2Enum_Stat, ObscuredDouble> _outputStatValue = new Dictionary<V2Enum_Stat, ObscuredDouble>();

        protected Dictionary<V2Enum_Stat, ObscuredDouble> _buffValue = new Dictionary<V2Enum_Stat, ObscuredDouble>();

#if UNITY_EDITOR
        public List<StatViewer> DefaultViewers = new List<StatViewer>();
        public List<StatViewer> OutputViewers = new List<StatViewer>();
#endif

        protected CharacterControllerBase _characterControllerBase;

        //------------------------------------------------------------------------------------
        public void SetCharacterControllerBaseType(CharacterControllerBase characterControllerBase)
        {
            _characterControllerBase = characterControllerBase;
        }
        //------------------------------------------------------------------------------------
        public void SetDefaultStat(V2Enum_Stat v2Enum_Stat, ObscuredDouble statValue)
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
    public virtual void RefreshDefaultStat()
        {
            
        }
        //------------------------------------------------------------------------------------
        public double GetDefaultValue(V2Enum_Stat v2Enum_Stat)
        {
            if (_defauleStatValue.ContainsKey(v2Enum_Stat) == false)
                return 0;

            return _defauleStatValue[v2Enum_Stat];
        }
        //------------------------------------------------------------------------------------
        public void RefreshOutputStatValue(V2Enum_Stat v2Enum_Stat = V2Enum_Stat.Max)
        {
            if (v2Enum_Stat == V2Enum_Stat.Max)
            {
                for (int i = V2Enum_Stat.Attack.Enum32ToInt(); i < V2Enum_Stat.Max.Enum32ToInt(); ++i)
                {
                    SetOutputStatValue(i.IntToEnum32<V2Enum_Stat>());
                }
            }
            else
                SetOutputStatValue(v2Enum_Stat);
        }
        //------------------------------------------------------------------------------------
        public void SetOutputStatValue(V2Enum_Stat v2Enum_Stat)
        { // 여기서 각 컨텐츠에서 계산되어야 할 값들을 가져와서 다시 셋팅
            double originValue = GetDefaultValue(v2Enum_Stat);

            //double buffvalue = _characterControllerBase.GetMyBuffValue(v2Enum_Stat);
            //buffvalue += Managers.BattleSceneManager.Instance.CurrentBattleScene.GetPassiveBuffValue(v2Enum_Stat, _characterControllerBase.IFFType);

            

            //double applyBuffValue = buffvalue;

            //if (v2Enum_Stat == V2Enum_Stat.Attack)
            //{
            //    if (Define.MaximumDecreaseAtt * Define.PercentageRecoverValue > applyBuffValue)
            //    {
            //        applyBuffValue = Define.MaximumDecreaseAtt * Define.PercentageRecoverValue;
            //    }
            //}
            //else if (v2Enum_Stat == V2Enum_Stat.Defence)
            //{
            //    if (Define.MaximumDecreaseArmor * Define.PercentageRecoverValue > applyBuffValue)
            //    {
            //        applyBuffValue = Define.MaximumDecreaseArmor * Define.PercentageRecoverValue;
            //    }
            //}
            //else
            //{
            //    if (applyBuffValue < -0.99)
            //        applyBuffValue = -0.99;
            //}

            //if (_buffValue.ContainsKey(v2Enum_Stat) == false)
            //    _buffValue.Add(v2Enum_Stat, applyBuffValue);
            //else
            //    _buffValue[v2Enum_Stat] = applyBuffValue;

            //if (v2Enum_Stat == V2Enum_Stat.CritChance)
            //    originValue += originValue + (applyBuffValue * 10000);
            //else
            //    originValue += originValue * applyBuffValue;

            //if (_outputStatValue.ContainsKey(v2Enum_Stat) == false)
            //    _outputStatValue.Add(v2Enum_Stat, 0);

            _outputStatValue[v2Enum_Stat] = originValue;

#if UNITY_EDITOR
            StatViewer statViewer = OutputViewers.Find(x => x.v2Enum_Stat == v2Enum_Stat);
            if (statViewer == null)
            {
                statViewer = new StatViewer();
                statViewer.v2Enum_Stat = v2Enum_Stat;
                OutputViewers.Add(statViewer);
            }

            statViewer.value = originValue;
#endif

        }
        //------------------------------------------------------------------------------------
        public void RefreshAllStatValue()
        {
            RefreshDefaultStat();
            RefreshOutputStatValue();
        }
        //------------------------------------------------------------------------------------
        public double GetOutPutMyStat(V2Enum_Stat v2Enum_Stat)
        {
            if (_outputStatValue.ContainsKey(v2Enum_Stat) == false)
                return 0;

            return _outputStatValue[v2Enum_Stat];
        }
        //------------------------------------------------------------------------------------
        public double GetBuffValue(V2Enum_Stat v2Enum_Stat)
        {
            if (_buffValue.ContainsKey(v2Enum_Stat) == false)
                return 0;

            return _buffValue[v2Enum_Stat];
        }
        //------------------------------------------------------------------------------------
        public void ForceReleaseStat()
        {
            _outputStatValue.Clear();
            _defauleStatValue.Clear();
        }
        //------------------------------------------------------------------------------------
    }
}