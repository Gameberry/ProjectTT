using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Common;

namespace GameBerry.Managers
{
    public class CreatureManager : MonoSingleton<CreatureManager>
    {
        private CreatureController _creatureControllerObject;

        private ObjectPool<CreatureController> _creatureControllerPool = new ObjectPool<CreatureController>();

        private CreatureLocalTable _creatureLocalTable;
        
        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            _creatureLocalTable = TableManager.Instance.GetTableClass<CreatureLocalTable>();

        }
        //------------------------------------------------------------------------------------
        public void InitCreatureContent()
        {
            ResourceLoader.Instance.Load<GameObject>("BattleScene/CreatureController", o =>
            {
                GameObject obj = o as GameObject;
                _creatureControllerObject = obj.GetComponent<CreatureController>();
            });


            foreach (var pair in _creatureLocalTable.GetAllCreatureData())
            {
                CreatureData creatureData = pair.Value;
                creatureData.BasicAttackData = SkillManager.Instance.GetSkillBaseData(creatureData.BasicAttack);
                creatureData.ActiveSkillData = SkillManager.Instance.GetSkillBaseData(creatureData.ActiveSkill1);

                SetPassiveSkillData(creatureData, SkillManager.Instance.GetSkillBaseData(creatureData.PassiveSkill1));
                SetPassiveSkillData(creatureData, SkillManager.Instance.GetSkillBaseData(creatureData.PassiveSkill2));
                SetPassiveSkillData(creatureData, SkillManager.Instance.GetSkillBaseData(creatureData.PassiveSkill3));
            }

            for (int i = 0; i < 10; ++i)
            {
                CreateCreature();
            }
        }
        //------------------------------------------------------------------------------------
        private void SetPassiveSkillData(CreatureData creatureData, SkillBaseData skillBaseData)
        {
            if (skillBaseData == null)
                return;

            if (creatureData.PassiveDatas == null)
                creatureData.PassiveDatas = new List<SkillBaseData>();

            creatureData.PassiveDatas.Add(skillBaseData);
        }
        //------------------------------------------------------------------------------------
        public CreatureController GetCreature()
        {
            CreatureController creatureController = _creatureControllerPool.GetObject() ?? CreateCreature();

            return creatureController;
        }
        //------------------------------------------------------------------------------------
        public void PoolCreature(CreatureController creatureController)
        {
            creatureController.gameObject.SetActive(false);
            _creatureControllerPool.PoolObject(creatureController);
        }
        //------------------------------------------------------------------------------------
        private CreatureController CreateCreature()
        {
            GameObject clone = Instantiate(_creatureControllerObject.gameObject, transform);

            CreatureController creatureController = clone.GetComponent<CreatureController>();
            creatureController.Init();
            creatureController.gameObject.SetActive(false);

            return creatureController;
        }
        //------------------------------------------------------------------------------------
        public CreatureData GetCreatureData(int creatureIndex)
        {
            return _creatureLocalTable.GetCreatureData(creatureIndex);
        }
        //------------------------------------------------------------------------------------
        public double GetCreatureLevelStatAddValue(CreatureData creatureData, int level, V2Enum_Stat v2Enum_Stat)
        {
            List<CreatureLevelUpStatData> creatureLevelUpStatDatas = _creatureLocalTable.GetCreatureLevelUpStatDatas(creatureData.MonsterRoleType);

            double statValue = 0.0;

            int checkLastDataMaxLevel = 0;

            for (int i = 0; i < creatureLevelUpStatDatas.Count; ++i)
            {
                CreatureLevelUpStatData creatureLevelUpStatData = creatureLevelUpStatDatas[i];

                if (creatureLevelUpStatData.StatValue.ContainsKey(v2Enum_Stat) == false)
                {
                    checkLastDataMaxLevel = creatureLevelUpStatData.MaximumLevel;
                    continue;
                }

                int operLevel = 0;

                if (creatureLevelUpStatData.MaximumLevel < level)
                {
                    
                    operLevel = creatureLevelUpStatData.MaximumLevel - checkLastDataMaxLevel;

                    statValue += creatureLevelUpStatData.StatValue[v2Enum_Stat].BaseValue * operLevel;

                    checkLastDataMaxLevel = creatureLevelUpStatData.MaximumLevel;

                    continue;
                }

                operLevel = level - checkLastDataMaxLevel;
                statValue += creatureLevelUpStatData.StatValue[v2Enum_Stat].BaseValue * operLevel;

                break;
            }

            return statValue;
        }
        //------------------------------------------------------------------------------------
    }
}