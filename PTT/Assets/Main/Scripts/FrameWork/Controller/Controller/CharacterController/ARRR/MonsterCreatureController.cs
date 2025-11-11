using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class MonsterCreatureController : CreatureController
    {
        //protected SkillBaseData _attackData;
        //protected CreatureSkillActionBase _attackActionScript;

        //protected SkillBaseData _skillData;
        //protected CreatureSkillActionBase _skillActionScript;


        //private bool _deadDirection_Swing = false;
        //private Vector3 _swingDirVec = Vector3.zero;

        //private float _playSkillTime = 0.0f;


        ////------------------------------------------------------------------------------------
        //public void SetLevel(int level)
        //{
        //    CreatureLevel = level;

        //    _characterStatOperator.RefreshAllStatValue();

        //    RefreshStat();
        //}
        ////------------------------------------------------------------------------------------
        //public override void SetCreatureSizeControll(Vector3 size)
        //{
        //    base.SetCreatureSizeControll(size);

        //    if (_attackActionScript != null)
        //        _attackActionScript.transform.localScale = size;

        //    if (_skillActionScript != null)
        //        _skillActionScript.transform.localScale = size;
        //}
        ////------------------------------------------------------------------------------------
        //public void SetCreatureData(CreatureData creatureData, int level)
        //{
        //    _deadDirection_Swing = false;

        //    Vector3 rotate = transform.localEulerAngles;
        //    rotate.z = 0.0f;
        //    transform.localEulerAngles = rotate;

        //    _uiCharacterState.transform.localEulerAngles = rotate;

        //    CreatureLevel = level;

        //    _creatureData = creatureData;

        //    _selectPlaySkillData = null;

        //    _currentSpineModelData = StaticResource.Instance.GetCreatureSpineModelData(creatureData.ResourceIndex);

        //    _attackData = creatureData.BasicAttackData;
        //    SetCreatureSkillActor(_attackData, _currentSpineModelData.BaseSkill, ref _attackActionScript);

        //    if (_attackData != null)
        //        SetSelectSkillType(_attackData);

        //    _skillData = creatureData.ActiveSkillData;
        //    SetCreatureSkillActor(_skillData, _currentSpineModelData.ActiveSkill, ref _skillActionScript);

        //    if (_selectPlaySkillData == null)
        //    {
        //        if (_skillData != null)
        //            SetSelectSkillType(_skillData);
        //    }

        //    SetCreatureSizeControll(creatureData.Scale.ToVector3());

        //    _uiCharacterState.EnableCoolTimeBar(_skillData != null);

        //    _characterStatOperator.RefreshDefaultStat();

        //    RefreshStat(true);

        //    if (_currentSpineModelData != null)
        //    {
        //        SetSpineModelData(_currentSpineModelData);

        //        if (_currentSpineModelData.SkinList.Count > creatureData.ResourceSkin)
        //            SetSkin(_currentSpineModelData.SkinList[creatureData.ResourceSkin]);
        //        else
        //        {
        //            if (_currentSpineModelData.SkinList.Count > 0)
        //                SetSkin(_currentSpineModelData.SkinList[0]);
        //        }
        //    }


        //    ChangeState(CharacterState.Idle);

        //}
        ////------------------------------------------------------------------------------------
        //protected override void CreatureSkillRelease()
        //{
        //    if (_attackActionScript != null)
        //        _attackActionScript.ReleaseSkill();

        //    if (_skillActionScript != null)
        //        _skillActionScript.ReleaseSkill();
        //}
        ////------------------------------------------------------------------------------------
        //protected override void CreatureSkillRemove()
        //{
        //    SpineModelSkillData BaseSkillActor = _currentSpineModelData.BaseSkill;

        //    if (_currentSpineModelData.BaseSkill != null && _currentSpineModelData.BaseSkill.SkillActor != null)
        //    {
        //        Managers.CreatureSkillActionManager.Instance.PoolCreatureSkillActor(_currentSpineModelData.BaseSkill.SkillActor, _attackActionScript);
        //        _attackActionScript = null;
        //    }

        //    if (_currentSpineModelData.ActiveSkill != null && _currentSpineModelData.ActiveSkill.SkillActor != null)
        //    {
        //        Managers.CreatureSkillActionManager.Instance.PoolCreatureSkillActor(_currentSpineModelData.ActiveSkill.SkillActor, _skillActionScript);
        //        _attackActionScript = null;
        //    }
        //}
        ////------------------------------------------------------------------------------------
        //protected override void SetSelectPlaySkillData()
        //{
        //    if (_skillData != null && _playSkillTime <= Time.time)
        //    {
        //        if (_skillActionScript == null)
        //            SetSelectSkillType(_skillData);
        //        else if (_skillActionScript != null && _skillActionScript.IsReady() == true)
        //        {
        //            SetSelectSkillType(_skillData);
        //        }
        //        else
        //            SetSelectSkillType(_attackData);
        //    }
        //    else
        //        SetSelectSkillType(_attackData);
        //}
        ////------------------------------------------------------------------------------------
        //private void SetSelectSkillType(SkillBaseData skillBaseData)
        //{
        //    _selectPlaySkillData = skillBaseData;

        //    ChangeSearchType();
        //}
        ////------------------------------------------------------------------------------------

    }
}