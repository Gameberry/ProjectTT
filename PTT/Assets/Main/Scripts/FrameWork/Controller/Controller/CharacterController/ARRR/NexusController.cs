using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class NexusController : CharacterControllerBase
    {
        public event CallCharacterState SendState;
        public event CallMonsterHitState SendHP;

        //------------------------------------------------------------------------------------
        public override void Init()
        {
            _characterStatOperator = new CharacterStatOperator(this);
        }
        //------------------------------------------------------------------------------------
        public void SetNexusData(CreatureData creatureData, int level)
        {
            _characterState = CharacterState.Idle;
            _maxHP  = creatureData.GetBaseStatValue(V2Enum_Stat.HP) + Managers.CreatureManager.Instance.GetCreatureLevelStatAddValue
                    (creatureData, level, V2Enum_Stat.HP);

            SetHP(_maxHP);
            _characterStatOperator.SetDefaultStat(V2Enum_Stat.HP, _maxHP);
            _characterStatOperator.SetDefaultStat(V2Enum_Stat.Defence, creatureData.GetBaseStatValue(V2Enum_Stat.Defence) + Managers.CreatureManager.Instance.GetCreatureLevelStatAddValue
                    (creatureData, level, V2Enum_Stat.Defence));

            _characterStatOperator.RefreshOutputStatValue();

            if (_uiCharacterState != null)
                _uiCharacterState.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------------------
        //public void SetRuneStoneSetHP(double hp)
        //{
        //    _characterState = CharacterState.Idle;
        //    _maxHP = hp;
        //    SetHP(_maxHP);

        //    _characterStatOperator.SetDefaultStat(V2Enum_Stat.HP, hp);
        //    _characterStatOperator.RefreshOutputStatValue();

        //    if (_uiCharacterState != null)
        //        _uiCharacterState.gameObject.SetActive(false);
        //}
        ////------------------------------------------------------------------------------------
        public void ActiveAggro(bool active)
        {
            if (_skillHitReceiver != null)
                _skillHitReceiver.EnableColliders(active);

            if (active == true)
            {
                Managers.AggroManager.Instance.AddIFFCharacterAggro(this);
            }
            else
            {
                Managers.AggroManager.Instance.RemoveIFFCharacterAggro(this);
            }
        }
        //------------------------------------------------------------------------------------
        public override double OnDamage(V2SkillAttackData damage)
        {
            if (_uiCharacterState != null)
                _uiCharacterState.gameObject.SetActive(true);

            return base.OnDamage(damage);
        }
        //------------------------------------------------------------------------------------
        protected override void ChangeState(CharacterState characterState)
        {
            if (characterState == CharacterState.Dead)
            {
                SendState(characterState);
            }
        }
        //------------------------------------------------------------------------------------
    }
}