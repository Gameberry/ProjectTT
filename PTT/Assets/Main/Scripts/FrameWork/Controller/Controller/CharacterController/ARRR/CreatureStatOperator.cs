using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class CreatureStatOperator : CharacterStatOperator
    {
        public CreatureStatOperator(CharacterControllerBase characterControllerBase) : base(characterControllerBase)
        {
        }

        private CreatureController _myCreatureController = null;

        //------------------------------------------------------------------------------------
        public void SetCreature(CreatureController creatureController)
        {
            _myCreatureController = creatureController;
        }
        //------------------------------------------------------------------------------------
        public override void RefreshDefaultStat()
        {
            if (_myCreatureController == null || _myCreatureController.CreatureData == null)
                return;

            CreatureData creatureData = _myCreatureController.CreatureData;

            for (int i = V2Enum_Stat.Attack.Enum32ToInt(); i < V2Enum_Stat.Max.Enum32ToInt(); ++i)
            {
                V2Enum_Stat v2Enum_Stat = i.IntToEnum32<V2Enum_Stat>();

                double value = 0.0;

                if (creatureData.ContainsStat(v2Enum_Stat) == true)
                {
                    value = creatureData.GetBaseStatValue(v2Enum_Stat);
                }
                else
                {
                    if (v2Enum_Stat == V2Enum_Stat.SkillCoolTimeDecrease)
                        value = 1.0;
                }

                value += Managers.CreatureManager.Instance.GetCreatureLevelStatAddValue
                    (creatureData, _myCreatureController.CreatureLevel, v2Enum_Stat);

                SetDefaultStat(v2Enum_Stat, value);
            }
        }
        //------------------------------------------------------------------------------------
    }
}