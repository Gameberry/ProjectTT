using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.Managers
{
    public class CharacterStatManager : MonoSingleton<CharacterStatManager>
    {
        private CreatureStatController m_creatureStatController;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            m_creatureStatController = new CreatureStatController();
            //m_creatureStatController.Init(ActorType.Knight);
        }
        //------------------------------------------------------------------------------------
        public CreatureStatController GetCreatureStatController()
        {
            return m_creatureStatController;
        }
        //------------------------------------------------------------------------------------
        public void AddStatElementValue(V2Enum_Stat type, StatElementValue elementvalue)
        {
            m_creatureStatController.AddStatElementValue(type, elementvalue);
        }
        //------------------------------------------------------------------------------------
        public void AddStatElementPercent(V2Enum_Stat type, StatElementValue elementvalue)
        {
            m_creatureStatController.AddStatElementPercent(type, elementvalue);
        }
        //------------------------------------------------------------------------------------
        public void SetOutPutStatValues(List<V2Enum_Stat> v2Enum_Stats)
        {
            m_creatureStatController.SetRefreshValues(v2Enum_Stats);

            PassManager.Instance.CheckPassType(V2Enum_PassType.SkillLevel);
        }
        //------------------------------------------------------------------------------------
        public void SetOutPutStatValue(V2Enum_Stat type)
        {
            m_creatureStatController.SetOutPutStatValue(type);

            PassManager.Instance.CheckPassType(V2Enum_PassType.SkillLevel);
        }
        //------------------------------------------------------------------------------------
        public void SetOutPutStatValue_NotNotice(V2Enum_Stat type)
        {
            m_creatureStatController.SetOutPutStatValue_NotNotice(type);

            PassManager.Instance.CheckPassType(V2Enum_PassType.SkillLevel);
        }
        //------------------------------------------------------------------------------------
        public void SetDefaultStatValue(V2Enum_Stat type, double statvalue)
        { // 기본적으로 테이블에 있는 스탯을 적용한다.
            m_creatureStatController.SetDefaultStatValue(type, statvalue);
        }
        //------------------------------------------------------------------------------------
        public double GetOutPutHP()
        {
            return m_creatureStatController.GetOutPutHP();
        }
        //------------------------------------------------------------------------------------
        public double GetOutPutAddUpgradeCost()
        {
            return 1.0;
        }
        //------------------------------------------------------------------------------------
        public double GetOutPutCoolTimeRatio()
        {
            return m_creatureStatController.GetOutputSkillCoolTimeDecrease();
        }
        //------------------------------------------------------------------------------------
        public float GetOutputAttackRange()
        { 
            return m_creatureStatController.GetOutputAttackRange();
        }
        //------------------------------------------------------------------------------------
        public void SetBattlePower()
        {
            m_creatureStatController.SetBattlePower();
        }
        //------------------------------------------------------------------------------------
        public double GetBattlePower()
        {
            double power = m_creatureStatController.GetBattlePower();

            if (PlayerDataContainer.SaveHightestBattlePower.GetDecrypted() < power)
            { 
                PlayerDataContainer.SaveHightestBattlePower = power;
                PlayerDataContainer.PlayerCombetPower = Util.GetAlphabetNumber(power);
                PassManager.Instance.CheckPassType(V2Enum_PassType.SkillLevel);
            }

            return power;
        }
        //------------------------------------------------------------------------------------
    }
}