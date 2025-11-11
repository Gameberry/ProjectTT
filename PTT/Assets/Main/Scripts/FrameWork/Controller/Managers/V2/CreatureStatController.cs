using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class StatElementValue
    {
        public ObscuredDouble StatValue = 0.0;
    }

    public class CurrnetStat
    {
        public ObscuredDouble CurrStat;
        public event System.Action<double> RefrashStatEvent;

        public void SetStat(double stat)
        {
            CurrStat = stat;
            if (RefrashStatEvent != null)
                RefrashStatEvent(CurrStat.GetDecrypted());
        }
    }

    public class CreatureBaseStatElement
    {
        public V2Enum_Stat BaseStat;
        public double BaseValue;
    }

    public class OperatorOverrideStat : CreatureBaseStatElement
    {
        public double OverrideStatBaseValue;
        public double OverrideStatAddValue;
    }

    public class CreatureStatController
    {
        public Dictionary<V2Enum_Stat, List<StatElementValue>> m_addStatValues = new Dictionary<V2Enum_Stat, List<StatElementValue>>();
        public Dictionary<V2Enum_Stat, List<StatElementValue>> m_addStatPercents = new Dictionary<V2Enum_Stat, List<StatElementValue>>();

        public Dictionary<V2Enum_Stat, ObscuredDouble> m_defaultStatValues = new Dictionary<V2Enum_Stat, ObscuredDouble>();

        public Dictionary<V2Enum_Stat, CurrnetStat> m_statCurrentValues = new Dictionary<V2Enum_Stat, CurrnetStat>();

        private List<V2Enum_Stat> m_criticalKindHelper = new List<V2Enum_Stat>();

        public delegate void OnRefreshBattlePower(double before, double after);
        public event OnRefreshBattlePower RefreshBattlePowerEvent;

        private ObscuredDouble m_battlePower;

        private IFFType m_myActorType = IFFType.IFF_None;
        public IFFType MyActorType { get { return m_myActorType; } }

        //------------------------------------------------------------------------------------
        public void Init(IFFType actorType)
        {
            m_myActorType = actorType;

            if (actorType != IFFType.IFF_Friend
                && actorType != IFFType.IFF_Foe)
                Managers.BuffManager.Instance.AddActorStatController(this);

            m_criticalKindHelper.Add(V2Enum_Stat.CritChance);
        }
        //------------------------------------------------------------------------------------
        public void AddStatElementValue(V2Enum_Stat type, StatElementValue elementvalue)
        {
            if (m_addStatValues.ContainsKey(type) == false)
                m_addStatValues.Add(type, new List<StatElementValue>());

            m_addStatValues[type].Add(elementvalue);
        }
        //------------------------------------------------------------------------------------
        public void AddStatElementPercent(V2Enum_Stat type, StatElementValue elementvalue)
        {
            if (m_addStatPercents.ContainsKey(type) == false)
                m_addStatPercents.Add(type, new List<StatElementValue>());

            m_addStatPercents[type].Add(elementvalue);
        }
        //------------------------------------------------------------------------------------
        public void SetBattlePower()
        {
            m_battlePower = 0;

            double battle = 0.0;

            foreach (KeyValuePair<V2Enum_Stat, CurrnetStat> pair in m_statCurrentValues)
            {
                CharacterBaseStatData statdata = Managers.ARRRStatManager.Instance.GetCharacterBaseStatData(pair.Key);
                //battle += pair.Value.CurrStat * statdata.BattlePowerConvertValue;
            }

            m_battlePower = System.Math.Round(battle);
        }
        //------------------------------------------------------------------------------------
        public double GetBattlePower()
        {
            return m_battlePower;
        }
        //------------------------------------------------------------------------------------
        public void RefreshBattlePower(bool notice = true)
        {
            if (notice == true)
            {
                if (m_myActorType == IFFType.IFF_Friend)
                    Contents.InGameContent.CheckNoticeBattlePowerChange(this);
            }

            double before = m_battlePower;

            SetBattlePower();

            if (RefreshBattlePowerEvent != null)
                RefreshBattlePowerEvent(before, m_battlePower);
        }
        //------------------------------------------------------------------------------------
        public double GetFinalAttack()
        {
            double statvalue = GetOutPutATK() * GetOutPutFinalAttackValue();

            return statvalue;
        }
        //------------------------------------------------------------------------------------
        public double GetFinalHP()
        {
            double statvalue = GetOutPutHP();

            return statvalue;
        }
        //------------------------------------------------------------------------------------
        public double GetFinalDefence()
        {
            double statvalue = GetOutPutDefence();

            return statvalue;
        }
        //------------------------------------------------------------------------------------
        public double GetFinalCriticalDamage()
        {
            double statvalue = GetFinalAttack() * (1.0 + (GetCriticalDamageValue(V2Enum_Stat.CritChance) * Define.PerStatRecoverValue));

            return statvalue;
        }
        //------------------------------------------------------------------------------------
        public void SetRefreshBattlePowerEvent(OnRefreshBattlePower action)
        {
            RefreshBattlePowerEvent += action;
        }
        //------------------------------------------------------------------------------------
        public void SetDefaultStatValue(V2Enum_Stat type, double statvalue)
        { // 기본적으로 테이블에 있는 스탯을 적용한다.
            if (m_defaultStatValues.ContainsKey(type) == false)
                m_defaultStatValues.Add(type, 0.0d);

            m_defaultStatValues[type] = statvalue;

            SetOutPutStatValue(type);
        }
        //------------------------------------------------------------------------------------
        public double GetDefaultValue(V2Enum_Stat type)
        {
            if (m_defaultStatValues.ContainsKey(type) == false)
                return 0.0;

            return m_defaultStatValues[type];
        }
        //------------------------------------------------------------------------------------
        private double GetTotalAddStatValue(V2Enum_Stat type)
        {
            double AddStat = GetDefaultValue(type);

            if (m_addStatValues.ContainsKey(type) == true)
            {
                List<StatElementValue> AddStatList = m_addStatValues[type];

                for (int i = 0; i < AddStatList.Count; ++i)
                {
                    AddStat += AddStatList[i].StatValue;
                }
            }

            if (m_addStatPercents.ContainsKey(type) == true)
            {
                List<StatElementValue> AddBuffStatList = m_addStatPercents[type];

                double addpercent = 0.0;

                for (int i = 0; i < AddBuffStatList.Count; ++i)
                {
                    addpercent += AddBuffStatList[i].StatValue;
                }

                addpercent *= Define.PerStatRecoverValue;

                AddStat = AddStat + (AddStat * addpercent);
            }

            double buffValue = Managers.BuffManager.Instance.GetBuffValue(m_myActorType, type);

            AddStat *= (1.0 + (buffValue * 0.0001));

            return AddStat;
        }
        //------------------------------------------------------------------------------------
        public void SetRefreshValues(List<V2Enum_Stat> v2Enum_Stats)
        {
            if (v2Enum_Stats == null)
                return;

            for (int i = 0; i < v2Enum_Stats.Count; ++i)
            {
                if (m_statCurrentValues.ContainsKey(v2Enum_Stats[i]) == false)
                    m_statCurrentValues.Add(v2Enum_Stats[i], new CurrnetStat());

                double value = GetTotalAddStatValue(v2Enum_Stats[i]);

                m_statCurrentValues[v2Enum_Stats[i]].SetStat(value);
            }

            if (m_myActorType == IFFType.IFF_Friend)
                RefreshBattlePower();
        }
        //------------------------------------------------------------------------------------
        public void SetOutPutStatValue(V2Enum_Stat type)
        {
            if (m_statCurrentValues.ContainsKey(type) == false)
                m_statCurrentValues.Add(type, new CurrnetStat());

            double value = GetTotalAddStatValue(type);

            m_statCurrentValues[type].SetStat(value);

            if (m_myActorType == IFFType.IFF_Friend)
                RefreshBattlePower();
        }
        //------------------------------------------------------------------------------------
        public void SetOutPutStatValue_NotNotice(V2Enum_Stat type)
        {
            if (m_statCurrentValues.ContainsKey(type) == false)
                m_statCurrentValues.Add(type, new CurrnetStat());

            double value = GetTotalAddStatValue(type);

            m_statCurrentValues[type].SetStat(value);

            if (m_myActorType == IFFType.IFF_Friend)
                RefreshBattlePower(false);
        }
        //------------------------------------------------------------------------------------
        public void AddStatRefrashEvent(V2Enum_Stat type, System.Action<double> action)
        {
            if (m_statCurrentValues.ContainsKey(type) == false)
                m_statCurrentValues.Add(type, new CurrnetStat());

            m_statCurrentValues[type].RefrashStatEvent += action;
        }
        //------------------------------------------------------------------------------------
        public void RemoveStatRefrashEvent(V2Enum_Stat type, System.Action<double> action)
        {
            if (m_statCurrentValues.ContainsKey(type) == true)
            {
                m_statCurrentValues[type].RefrashStatEvent -= action;
            }
        }
        //------------------------------------------------------------------------------------
        public double GetOutPutStatValue(V2Enum_Stat type)
        {
            if (m_statCurrentValues.ContainsKey(type) == false)
            {
                //SetOutPutStatValue(type);
                return 0.0;
            }

            return m_statCurrentValues[type].CurrStat;
        }
        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        public double GetOutPutATK()
        {
            double orivalue = GetOutPutStatValue(V2Enum_Stat.Attack);

            double returnvalue = orivalue;

            return returnvalue;
        }
        //------------------------------------------------------------------------------------
        public double GetOutPutFinalAttackValue()
        {
            return (1.0 + (Define.PerStatRecoverValue));
        }
        //------------------------------------------------------------------------------------
        public V2Enum_Stat GetCriticalKind()
        {
            bool ApplyCritical = Random.Range(0.0f, 10000.0f) <= GetOutPutStatValue(V2Enum_Stat.CritChance);

            if (ApplyCritical == true)
                return V2Enum_Stat.CritChance;

            return V2Enum_Stat.Attack;
        }
        //------------------------------------------------------------------------------------
        public double GetCriticalDamageValue(V2Enum_Stat criticaltype)
        {
            if (criticaltype == V2Enum_Stat.Attack)
                return 1.0;

            return Define.CriticalDamageValue;
        }
        //------------------------------------------------------------------------------------
        public double GetRandomAddDamage(double damage)
        {
            //double pervalue = GetOutPutStatValue(V2Enum_Stat.RandomDamage) * Define.PerStatRecoverValue;
            //float randomDamage = Random.Range(1.0f, (float)pervalue);

            //return damage * (double)randomDamage;

            return damage;
        }
        //------------------------------------------------------------------------------------
        public double GetOutPutHP()
        {
            double orivalue = GetOutPutStatValue(V2Enum_Stat.HP);

            return orivalue;
        }
        //------------------------------------------------------------------------------------
        public double GetOutPutDefence()
        {
            double orivalue = GetOutPutStatValue(V2Enum_Stat.Defence);

            return orivalue;
        }
        //------------------------------------------------------------------------------------
        public float GetOutputMoveSpeed()
        {
            return (float)(GetOutPutStatValue(V2Enum_Stat.MoveSpeed) * Define.PerStatRecoverValue);
        }
        //------------------------------------------------------------------------------------
        public float GetOutputAttackRange()
        {
            return 1.0f;
        }
        //------------------------------------------------------------------------------------
        public float GetOutputSkillCoolTimeDecrease()
        {
            //return (float)(GetOutPutStatValue(V2Enum_Stat.SkillCoolTimeDecrease) * Define.PerStatRecoverValue) - 1.0f;
            return 0.0f;
        }
        //------------------------------------------------------------------------------------
    }
}