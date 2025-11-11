using System;
using System.Collections.Generic;
using Gpm.Ui;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class PetInfo
    {
        public ObscuredInt Id;
        public ObscuredInt Level = Define.PlayerSkillDefaultLevel;
        public ObscuredInt Count = 0;
    }

    public static class PetContainer
    {
        public static Dictionary<ObscuredInt, PetInfo> petInfos = new Dictionary<ObscuredInt, PetInfo>();
    }

    public static class PetOperator
    {
        //------------------------------------------------------------------------------------
        public static PetInfo AddNewPlayerPetInfo(PetData petData)
        {
            if (PetContainer.petInfos.ContainsKey(petData.Index) == true)
                return PetContainer.petInfos[petData.Index];
            else
            {
                PetInfo playerGearInfo = new PetInfo();
                playerGearInfo.Id = petData.Index;
                playerGearInfo.Count = 0;
                playerGearInfo.Level = Define.PlayerSkillDefaultLevel;

                PetContainer.petInfos.Add(playerGearInfo.Id, playerGearInfo);

                return playerGearInfo;
            }
        }
        //------------------------------------------------------------------------------------
    }
}