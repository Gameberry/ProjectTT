using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.Managers
{
    public class BuffManager : MonoSingleton<BuffManager>
    {
        private BuffLocalTable m_buffLocalTable = null;

        private Dictionary<V2Enum_TargetType, Dictionary<V2Enum_Stat, double>> m_targerBuffStat = new Dictionary<V2Enum_TargetType, Dictionary<V2Enum_Stat, double>>();

        private Dictionary<IFFType, List<CreatureStatController>> m_actorController = new Dictionary<IFFType, List<CreatureStatController>>();

        // 현재 켜져있는 버프
        private Dictionary<BuffData, double> m_activeBuff = new Dictionary<BuffData, double>();
        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            m_buffLocalTable = TableManager.Instance.GetTableClass<BuffLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public void AddActorStatController(CreatureStatController creatureStatController)
        {
            if (m_actorController.ContainsKey(creatureStatController.MyActorType) == false)
            {
                m_actorController.Add(creatureStatController.MyActorType, new List<CreatureStatController>());
            }

            m_actorController[creatureStatController.MyActorType].Add(creatureStatController);
        }
        //------------------------------------------------------------------------------------
        public BuffData GetBuffData(int index)
        {
            return m_buffLocalTable.GetData(index);
        }
        //------------------------------------------------------------------------------------
        public void ActiveBuff(int index)
        {
            ActiveBuff(GetBuffData(index));
        }
        //------------------------------------------------------------------------------------
        public void ActiveBuff(BuffData buffData)
        {
            if (m_activeBuff.ContainsKey(buffData) == true)
                return;

            if (m_targerBuffStat.ContainsKey(buffData.BuffTargetType) == false)
                m_targerBuffStat.Add(buffData.BuffTargetType, new Dictionary<V2Enum_Stat, double>());

            if (m_targerBuffStat[buffData.BuffTargetType].ContainsKey(buffData.BuffEffectType) == false)
                m_targerBuffStat[buffData.BuffTargetType].Add(buffData.BuffEffectType, 0.0);

            m_targerBuffStat[buffData.BuffTargetType][buffData.BuffEffectType] += buffData.BuffEffectValue;

            m_activeBuff.Add(buffData, buffData.BuffEffectValue);

            RefreshBuffValue(buffData.BuffTargetType, buffData.BuffEffectType);
        }
        //------------------------------------------------------------------------------------
        public void InActiveBuff(int index)
        {
            InActiveBuff(GetBuffData(index));
        }
        //------------------------------------------------------------------------------------
        public void InActiveBuff(BuffData buffData)
        {
            if (m_activeBuff.ContainsKey(buffData) == false)
                return;

            if (m_targerBuffStat.ContainsKey(buffData.BuffTargetType) == false)
                m_targerBuffStat.Add(buffData.BuffTargetType, new Dictionary<V2Enum_Stat, double>());

            if (m_targerBuffStat[buffData.BuffTargetType].ContainsKey(buffData.BuffEffectType) == false)
                m_targerBuffStat[buffData.BuffTargetType].Add(buffData.BuffEffectType, 0.0);

            m_targerBuffStat[buffData.BuffTargetType][buffData.BuffEffectType] -= buffData.BuffEffectValue;
            if (m_targerBuffStat[buffData.BuffTargetType][buffData.BuffEffectType] < 0.0)
                m_targerBuffStat[buffData.BuffTargetType][buffData.BuffEffectType] = 0.0;

            m_activeBuff.Remove(buffData);

            RefreshBuffValue(buffData.BuffTargetType, buffData.BuffEffectType);
        }
        //------------------------------------------------------------------------------------
        private void RefreshBuffValue(V2Enum_TargetType v2Enum_TargetType, V2Enum_Stat v2Enum_Stat)
        {
            if (v2Enum_TargetType == V2Enum_TargetType.Ally)
            {
                RefreshBuffValue(IFFType.IFF_Friend, v2Enum_Stat);
            }
            else if (v2Enum_TargetType == V2Enum_TargetType.Monster)
            {
                RefreshBuffValue(IFFType.IFF_Foe, v2Enum_Stat);
            }
            else if (v2Enum_TargetType == V2Enum_TargetType.CharacterAndAlly)
            {
                //RefreshBuffValue(ActorType.Knight, v2Enum_Stat);
                RefreshBuffValue(IFFType.IFF_Friend, v2Enum_Stat);
            }
        }
        //------------------------------------------------------------------------------------
        private void RefreshBuffValue(IFFType actorType, V2Enum_Stat v2Enum_Stat)
        {
            if (m_actorController.ContainsKey(actorType) == true)
            {
                List<CreatureStatController> creatureStatControllers = m_actorController[actorType];

                for (int i = 0; i < creatureStatControllers.Count; ++i)
                {
                    creatureStatControllers[i].SetOutPutStatValue(v2Enum_Stat);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public double GetBuffValue(IFFType actorType, V2Enum_Stat v2Enum_Stat)
        {
            double bufffvalue = 0.0;

            if (actorType == IFFType.IFF_Friend)
            {
                bufffvalue += GetBuffValue(V2Enum_TargetType.Ally, v2Enum_Stat);
                bufffvalue += GetBuffValue(V2Enum_TargetType.CharacterAndAlly, v2Enum_Stat);
            }
            else if (actorType == IFFType.IFF_Foe)
            {
                bufffvalue += GetBuffValue(V2Enum_TargetType.Monster, v2Enum_Stat);
            }

            return bufffvalue;
        }
        //------------------------------------------------------------------------------------
        public double GetBuffValue(V2Enum_TargetType v2Enum_TargetType, V2Enum_Stat v2Enum_Stat)
        {
            if (m_targerBuffStat.ContainsKey(v2Enum_TargetType) == true)
            {
                if (m_targerBuffStat[v2Enum_TargetType].ContainsKey(v2Enum_Stat) == true)
                    return m_targerBuffStat[v2Enum_TargetType][v2Enum_Stat];
            }

            return 0.0;
        }
        //------------------------------------------------------------------------------------
    }
}