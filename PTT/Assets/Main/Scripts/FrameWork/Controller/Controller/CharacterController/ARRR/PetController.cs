using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class PetController : CharacterControllerBase
    {
        private CharacterControllerBase _myCharacterControllerBase;

        private PetInfo _petInfo;

        private MoveController_TargetPos _creatureMove_TargetPos;

        private int _index = 0;

        //------------------------------------------------------------------------------------
        public override void Init()
        {
            _creatureMove_TargetPos = new MoveController_TargetPos();
            _creatureMove_TargetPos.SetCharacterController(this);
        }
        //------------------------------------------------------------------------------------
        public MoveController_TargetPos GetMoveController()
        {
            return _creatureMove_TargetPos;
        }
        //------------------------------------------------------------------------------------
        public void SetIndex(int index)
        {
            _index = index;
        }
        //------------------------------------------------------------------------------------
        public void SetCharacterController(CharacterControllerBase characterControllerBase)
        {
            _myCharacterControllerBase = characterControllerBase;
            _characterStatOperator = _myCharacterControllerBase.CharacterStatOperator;
        }
        //------------------------------------------------------------------------------------
        public void SetPetInfo(PetInfo petInfo)
        {
            _petInfo = petInfo;

            PetData petData = Managers.PetManager.Instance.GetPetData(petInfo.Id);

            SpineModelData spineModelData = StaticResource.Instance.GetCreatureSpineModelData(1001);

            SetSpineModelData(spineModelData);
            SetSkin(spineModelData.SkinList[petData.ResourceSkin]);
            PlayAnimation(CharacterState.Idle, true);

            _skillActiveController.ReleaseSkill();
            if (petData.ActiveSkillData != null)
                _skillActiveController.AddSkillData(petData.ActiveSkillData);

            if (petData.PassiveDatas != null)
            {
                for (int i = 0; i < petData.PassiveDatas.Count; ++i)
                {
                    if (_myCharacterControllerBase != null)
                    {
                        _myCharacterControllerBase.SkillActiveController.AddSkillData(petData.PassiveDatas[i]);
                    }
                    //_skillActiveController.AddSkillData(petData.PassiveDatas[i]);
                }
            }

            //SetRandomCharacter();
        }
        //------------------------------------------------------------------------------------
        public void SetPetInfo(PetData petData, SkillInfo skillInfo = null)
        {
            SpineModelData spineModelData = StaticResource.Instance.GetCreatureSpineModelData(1001);

            SetSpineModelData(spineModelData);
            SetSkin(spineModelData.SkinList[petData.ResourceSkin]);
            PlayAnimation(CharacterState.Idle, true);

            _skillActiveController.ReleaseSkill();
            if (petData.ActiveSkillData != null)
                _skillActiveController.AddSkillData(petData.ActiveSkillData);

            if (petData.PassiveDatas != null)
            {
                for (int i = 0; i < petData.PassiveDatas.Count; ++i)
                {
                    if (_myCharacterControllerBase != null)
                    {
                        _myCharacterControllerBase.SkillActiveController.AddSkillData(petData.PassiveDatas[i], false, skillInfo);
                    }
                    //_skillActiveController.AddSkillData(petData.PassiveDatas[i]);
                }
            }

            //SetRandomCharacter();
        }
        //------------------------------------------------------------------------------------
        public void SetRandomCharacter()
        {
            SpineModelAsset spineModelAsset = StaticResource.Instance.GetSpineModelAsset();

            SpineModelData spineModelData = spineModelAsset.SpineModelDatas.Find(x => x.Name.Contains("skull"));

            SetSpineModelData(spineModelData);
            SetSkin(spineModelData.SkinList[_index % spineModelData.SkinList.Count]);
            PlayAnimation(CharacterState.Idle, true);
        }
        //------------------------------------------------------------------------------------
        protected override void Updated()
        {
            if (_myCharacterControllerBase != null)
                ChangeCharacterLookAtDirection_Target(_myCharacterControllerBase.transform);
        }
        //------------------------------------------------------------------------------------
    }
}