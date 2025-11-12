using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry.Managers
{
    public class CharacterSkinManager : MonoSingleton<CharacterSkinManager>
    {
        private Dictionary<V2Enum_Stat, StatElementValue> m_addOwnStatValue = new Dictionary<V2Enum_Stat, StatElementValue>();

        private Event.RefreshCharacterSkinInfoListMsg m_refreshCharacterSkinInfoListMsg = new Event.RefreshCharacterSkinInfoListMsg();
        private Event.ChangeCharacterSkinInfoMsg m_changeCharacterSkinInfoMsg = new Event.ChangeCharacterSkinInfoMsg();
        

        private GearData tempCharacterGearData = new GearData(); // ±øÅë Gear


        private List<string> m_changeInfoUpdate = new List<string>();

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            m_changeInfoUpdate.Add(Define.PlayerSkinTable);
            m_changeInfoUpdate.Add(Define.PlayerPointTable);

            CharacterSkinOperator.Init();
        }
        //------------------------------------------------------------------------------------
        public void InitSkinContent()
        {
            foreach (KeyValuePair<int, PlayerSkinInfo> pair in CharacterSkinContainer.m_skinInfo)
            {
                CharacterSkinData characterSkinData = GetCharacterSkinData(pair.Value.Id);
                double statvalue = GetSkinOwnEffectValue(characterSkinData);

                if (statvalue <= 0.0)
                    continue;

                InCreaseOwnStat(characterSkinData.OwnEffectType, statvalue);
            }

            foreach (var pair in CharacterSkinContainer.m_skinEquipId)
            {
                if (pair.Key == V2Enum_Skin.SkinWeapon)
                {
                    CharacterSkinData characterSkinData = GetCharacterSkinData(pair.Value);
                    if (characterSkinData == null)
                        continue;

                    PlayerSkinInfo playerSkinInfo = GetPlayerSkinInfo(characterSkinData);
                    if (playerSkinInfo == null)
                        continue;


                    CharacterSkinContainer.m_currentSkinWeaponName = string.Format(Define.WeaponResourceNameKey, characterSkinData.ResourceIndex);
                    //Managers.CharacterGearManager.Instance.ChangeEquipWeaponEffect(characterSkinData.MyFakeGearData);
                }
                else if (pair.Key == V2Enum_Skin.SkinBody)
                {
                    CharacterSkinData characterSkinData = GetCharacterSkinData(pair.Value);
                    if (characterSkinData == null)
                        continue;

                    PlayerSkinInfo playerSkinInfo = GetPlayerSkinInfo(characterSkinData);
                    if (playerSkinInfo == null)
                        continue;


                    CharacterSkinContainer.m_currentSkinBodyNumber = characterSkinData.ResourceIndex;
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void InCreaseOwnStat(V2Enum_Stat v2Enum_Stat, double statValue, bool dontRefreshStat = false)
        {
            if (m_addOwnStatValue.ContainsKey(v2Enum_Stat) == false)
            {
                m_addOwnStatValue.Add(v2Enum_Stat, new StatElementValue());
                CharacterStatManager.Instance.AddStatElementValue(v2Enum_Stat, m_addOwnStatValue[v2Enum_Stat]);
            }

            m_addOwnStatValue[v2Enum_Stat].StatValue += statValue;
            if (dontRefreshStat == false)
                CharacterStatManager.Instance.SetOutPutStatValue(v2Enum_Stat);
        }
        //------------------------------------------------------------------------------------
        public void DeCreaseOwnStat(V2Enum_Stat v2Enum_Stat, double statValue, bool dontRefreshStat = false)
        {
            if (m_addOwnStatValue.ContainsKey(v2Enum_Stat) == false)
            {
                return;
            }

            m_addOwnStatValue[v2Enum_Stat].StatValue -= statValue;
            if (dontRefreshStat == false)
                CharacterStatManager.Instance.SetOutPutStatValue(v2Enum_Stat);
        }
        //------------------------------------------------------------------------------------
        public List<CharacterSkinData> GetCharacterSkinAllData(V2Enum_Skin v2Enum_Goods)
        {
            return CharacterSkinOperator.GetCharacterSkinAllData(v2Enum_Goods);
        }
        //------------------------------------------------------------------------------------
        public CharacterSkinData GetCharacterSkinData(int index)
        {
            return CharacterSkinOperator.GetCharacterSkinData(index);
        }
        //------------------------------------------------------------------------------------
        public double GetSkinOwnEffectValue(CharacterSkinData characterSkinData)
        {
            return CharacterSkinOperator.GetSkinOwnEffectValue(characterSkinData);
        }
        //------------------------------------------------------------------------------------
        private PlayerSkinInfo AddNewPlayerSkinInfo(CharacterSkinData characterSkinData)
        {
            PlayerSkinInfo playerSkinInfo = CharacterSkinOperator.AddNewPlayerSkinInfo(characterSkinData);

            if (playerSkinInfo == null)
                return null;

            double statvalue = GetSkinOwnEffectValue(characterSkinData);
            InCreaseOwnStat(characterSkinData.OwnEffectType, statvalue);

            Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.CharacterSkin);

            if(characterSkinData.SkinType == V2Enum_Skin.SkinWeapon)
                RedDotManager.Instance.ShowRedDot(ContentDetailList.CharacterSkin_Weapon);
            else if (characterSkinData.SkinType == V2Enum_Skin.SkinBody)
                RedDotManager.Instance.ShowRedDot(ContentDetailList.CharacterSkin_Body);

            ThirdPartyLog.Instance.SendLog_Skin_GetEvent(characterSkinData.Index);

            return playerSkinInfo;
        }
        //------------------------------------------------------------------------------------
        public PlayerSkinInfo GetPlayerSkinInfo(int index)
        {
            CharacterSkinData characterSkinData = GetCharacterSkinData(index);
            if (characterSkinData == null)
                return null;

            return CharacterSkinOperator.GetPlayerSkinInfo(characterSkinData);
        }
        //------------------------------------------------------------------------------------
        public PlayerSkinInfo GetPlayerSkinInfo(CharacterSkinData characterSkinData)
        {
            return CharacterSkinOperator.GetPlayerSkinInfo(characterSkinData);
        }
        //------------------------------------------------------------------------------------
        public int GetSkinLevel(CharacterSkinData characterSkinData)
        {
            PlayerSkinInfo playerSkinInfo = GetPlayerSkinInfo(characterSkinData);
            if (playerSkinInfo == null)
                return Define.PlayerSkillDefaultLevel;

            return playerSkinInfo.Level;
        }
        //------------------------------------------------------------------------------------
        public int GetSkinStar(CharacterSkinData characterSkinData)
        {
            PlayerSkinInfo playerSkinInfo = GetPlayerSkinInfo(characterSkinData);
            if (playerSkinInfo == null)
                return 0;

            return playerSkinInfo.Star;
        }
        //------------------------------------------------------------------------------------
        public string GetSkinWeaponName()
        {
            if(string.IsNullOrEmpty(CharacterSkinContainer.m_fakeSkinWeaponName) == false)
                return CharacterSkinContainer.m_fakeSkinWeaponName;

            return CharacterSkinContainer.m_currentSkinWeaponName;
        }
        //------------------------------------------------------------------------------------
        public int GetSkinBodyNumber()
        {
            if (CharacterSkinContainer.m_fakeSkinBodyNumber != -1)
                return CharacterSkinContainer.m_fakeSkinBodyNumber;

            if (CharacterSkinContainer.m_currentSkinBodyNumber != -1)
                return CharacterSkinContainer.m_currentSkinBodyNumber;

            return Define.PlayerSpriteVariationNumber;
        }
        //------------------------------------------------------------------------------------
        private void SetPlayerSkinInfo_LevelUp(CharacterSkinData characterSkinData, bool dontRefreshStat = false)
        {
            PlayerSkinInfo playerSkinInfo = GetPlayerSkinInfo(characterSkinData);

            if (playerSkinInfo == null)
                return;

            double statvalue = GetSkinOwnEffectValue(characterSkinData);
            DeCreaseOwnStat(characterSkinData.OwnEffectType, statvalue, dontRefreshStat);

            playerSkinInfo.Level++;

            statvalue = GetSkinOwnEffectValue(characterSkinData);
            InCreaseOwnStat(characterSkinData.OwnEffectType, statvalue, dontRefreshStat);
        }
        //------------------------------------------------------------------------------------
        private void SetPlayerSkinInfo_StarUp(CharacterSkinData characterSkinData, bool dontRefreshStat = false)
        {
            PlayerSkinInfo playerSkinInfo = GetPlayerSkinInfo(characterSkinData);

            if (playerSkinInfo == null)
                return;

            double statvalue = GetSkinOwnEffectValue(characterSkinData);
            DeCreaseOwnStat(characterSkinData.OwnEffectType, statvalue, dontRefreshStat);

            playerSkinInfo.Star++;

            statvalue = GetSkinOwnEffectValue(characterSkinData);
            InCreaseOwnStat(characterSkinData.OwnEffectType, statvalue, dontRefreshStat);
        }
        //------------------------------------------------------------------------------------
        public string GetSkinLocalKey(int index)
        {
            CharacterSkinData characterSkinData = GetCharacterSkinData(index);

            if (characterSkinData == null)
                return string.Empty;

            return characterSkinData.NameLocalStringKey;
        }
        //------------------------------------------------------------------------------------
        public Sprite GetSkinSprite(int index)
        {
            CharacterSkinData characterSkinData = GetCharacterSkinData(index);

            if (characterSkinData == null)
                return null;

            return GetSkinSprite(characterSkinData);
        }
        //------------------------------------------------------------------------------------
        public Sprite GetSkinSprite(CharacterSkinData characterSkinData)
        {
            if (characterSkinData == null)
                return null;

            return StaticResource.Instance.GetIcon(characterSkinData.IconStringKey);
        }
        //------------------------------------------------------------------------------------
        public void SetSkinAmount(int index, int amount)
        {
            SetSkinAmount(GetCharacterSkinData(index), amount);
        }
        //------------------------------------------------------------------------------------
        public void SetSkinAmount(CharacterSkinData characterSkinData, int amount)
        {
            if (characterSkinData == null)
                return;

            PlayerSkinInfo playerSkinInfo = GetPlayerSkinInfo(characterSkinData);
            if (playerSkinInfo == null)
                playerSkinInfo = AddNewPlayerSkinInfo(characterSkinData);

            playerSkinInfo.Count = amount;

            if (CheckIsReadyStarUp(characterSkinData) == true)
            {
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.CharacterSkin);

                if (characterSkinData.SkinType == V2Enum_Skin.SkinWeapon)
                    RedDotManager.Instance.ShowRedDot(ContentDetailList.CharacterSkin_Weapon);
                else if (characterSkinData.SkinType == V2Enum_Skin.SkinBody)
                    RedDotManager.Instance.ShowRedDot(ContentDetailList.CharacterSkin_Body);
            }

            m_refreshCharacterSkinInfoListMsg.datas.Clear();
            m_refreshCharacterSkinInfoListMsg.datas.Add(characterSkinData);
            Message.Send(m_refreshCharacterSkinInfoListMsg);

            //ShowSkinRedDot(characterSkinData.SkinType);
        }
        //------------------------------------------------------------------------------------
        public int GetSkinAmount(int index)
        {
            return GetSkinAmount(GetCharacterSkinData(index));
        }
        //------------------------------------------------------------------------------------
        public int GetSkinAmount(CharacterSkinData characterSkinData)
        {
            if (characterSkinData == null)
                return 0;

            PlayerSkinInfo playerSkinInfo = GetPlayerSkinInfo(characterSkinData);
            if (playerSkinInfo == null)
                return 0;
            else
                return playerSkinInfo.Count;
        }
        //------------------------------------------------------------------------------------
        public int AddSkinAmount(int index, int amount)
        {
            return AddSkinAmount(GetCharacterSkinData(index), amount);
        }
        //------------------------------------------------------------------------------------

        public int AddSkinAmount(CharacterSkinData characterSkinData, int amount)
        {
            if (characterSkinData == null)
                return 0;

            PlayerSkinInfo playerSkinInfo = GetPlayerSkinInfo(characterSkinData);
            if (playerSkinInfo == null)
                playerSkinInfo = AddNewPlayerSkinInfo(characterSkinData);

            playerSkinInfo.Count += amount;

            if (CheckIsReadyStarUp(characterSkinData) == true)
            {
                Managers.RedDotManager.Instance.ShowRedDot(ContentDetailList.CharacterSkin);

                if (characterSkinData.SkinType == V2Enum_Skin.SkinWeapon)
                    RedDotManager.Instance.ShowRedDot(ContentDetailList.CharacterSkin_Weapon);
                else if (characterSkinData.SkinType == V2Enum_Skin.SkinBody)
                    RedDotManager.Instance.ShowRedDot(ContentDetailList.CharacterSkin_Body);
            }

            m_refreshCharacterSkinInfoListMsg.datas.Clear();
            m_refreshCharacterSkinInfoListMsg.datas.Add(characterSkinData);
            Message.Send(m_refreshCharacterSkinInfoListMsg);

            //ShowSkinRedDot(characterSkinData.SkinType);

            return playerSkinInfo.Count;
        }
        //------------------------------------------------------------------------------------
        public int UseSkinAmount(int index, int amount)
        {
            return UseSkinAmount(GetCharacterSkinData(index), amount);
        }
        //------------------------------------------------------------------------------------
        public int UseSkinAmount(CharacterSkinData characterSkinData, int amount)
        {
            if (characterSkinData == null)
                return 0;

            PlayerSkinInfo playerSkinInfo = GetPlayerSkinInfo(characterSkinData);
            if (playerSkinInfo == null)
                return 0;

            playerSkinInfo.Count -= amount;

            if (playerSkinInfo.Count < 0)
                playerSkinInfo.Count = 0;

            m_refreshCharacterSkinInfoListMsg.datas.Clear();
            m_refreshCharacterSkinInfoListMsg.datas.Add(characterSkinData);
            Message.Send(m_refreshCharacterSkinInfoListMsg);

            return playerSkinInfo.Count;
        }
        //------------------------------------------------------------------------------------
        public V2Enum_Grade GetSkinGrade(int index)
        {
            CharacterSkinData characterSkinData = GetCharacterSkinData(index);
            if (characterSkinData == null)
                return V2Enum_Grade.Max;

            return characterSkinData.SkinGrade;
        }
        //------------------------------------------------------------------------------------
        public double GetNeedEnhanceSkinPrice(CharacterSkinData characterSkinData)
        {
            return CharacterSkinOperator.GetNeedEnhanceSkinPrice(characterSkinData);
        }
        //------------------------------------------------------------------------------------
        public int GetNeedStarUpSkinPrice(CharacterSkinData characterSkinData)
        {
            return CharacterSkinOperator.GetNeedStarUpSkinPrice(characterSkinData);
        }
        //------------------------------------------------------------------------------------
        public bool IsMaxLevel(CharacterSkinData characterSkinData)
        {
            if (characterSkinData == null)
                return false;

            PlayerSkinInfo playerSkinInfo = GetPlayerSkinInfo(characterSkinData);
            if (playerSkinInfo == null)
                return false;

            return characterSkinData.MaxLevel <= playerSkinInfo.Level;
        }
        //------------------------------------------------------------------------------------
        public bool CheckIsReadyEnhance(CharacterSkinData characterSkinData)
        {
            if (IsMaxLevel(characterSkinData) == true)
                return false;

            PlayerSkinInfo playerSkinInfo = GetPlayerSkinInfo(characterSkinData);

            if (playerSkinInfo == null)
                return false;

            double needSkin = GetNeedEnhanceSkinPrice(characterSkinData);

            return GoodsManager.Instance.GetGoodsAmount(characterSkinData.LevelUpCostGoodsType.Enum32ToInt(), characterSkinData.LevelUpCostGoodsParam1) >= needSkin;
        }
        //------------------------------------------------------------------------------------
        public bool DoSkinEnhance(CharacterSkinData characterSkinData)
        {
            if (characterSkinData == null)
                return false;

            if (CheckIsReadyEnhance(characterSkinData) == true)
            {
                PlayerSkinInfo playerSkinInfo = GetPlayerSkinInfo(characterSkinData);

                double needSkin = GetNeedEnhanceSkinPrice(characterSkinData);

                int used_type = characterSkinData.LevelUpCostGoodsParam1;
                double former_quan = GoodsManager.Instance.GetGoodsAmount(characterSkinData.LevelUpCostGoodsType.Enum32ToInt(), characterSkinData.LevelUpCostGoodsParam1);
                double used_quan = needSkin;

                GoodsManager.Instance.UseGoodsAmount(characterSkinData.LevelUpCostGoodsType.Enum32ToInt(), characterSkinData.LevelUpCostGoodsParam1, needSkin);

                double keep_quan = GoodsManager.Instance.GetGoodsAmount(characterSkinData.LevelUpCostGoodsType.Enum32ToInt(), characterSkinData.LevelUpCostGoodsParam1);

                SetPlayerSkinInfo_LevelUp(characterSkinData);

                m_refreshCharacterSkinInfoListMsg.datas.Clear();
                m_refreshCharacterSkinInfoListMsg.datas.Add(characterSkinData);
                Message.Send(m_refreshCharacterSkinInfoListMsg);

                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate, null);

                ThirdPartyLog.Instance.SendLog_Skin_UpEvent(characterSkinData.Index, playerSkinInfo.Level,
                    used_type, former_quan, used_quan, keep_quan);

                return true;
            }

            return false;
        }
        //------------------------------------------------------------------------------------
        public bool CheckIsReadyStarUp(CharacterSkinData characterSkinData)
        {
            PlayerSkinInfo playerSkinInfo = GetPlayerSkinInfo(characterSkinData);

            if (playerSkinInfo == null)
                return false;

            double needSkin = GetNeedStarUpSkinPrice(characterSkinData);

            return playerSkinInfo.Count >= needSkin;
        }
        //------------------------------------------------------------------------------------
        public bool DoSkinStarUp(CharacterSkinData characterSkinData)
        {
            if (characterSkinData == null)
                return false;

            if (CheckIsReadyStarUp(characterSkinData) == true)
            {
                PlayerSkinInfo playerSkinInfo = GetPlayerSkinInfo(characterSkinData);

                int needSkin = GetNeedStarUpSkinPrice(characterSkinData);

                playerSkinInfo.Count -= needSkin;
                if (playerSkinInfo.Count < 0)
                    playerSkinInfo.Count = 0;

                SetPlayerSkinInfo_StarUp(characterSkinData);

                m_refreshCharacterSkinInfoListMsg.datas.Clear();
                m_refreshCharacterSkinInfoListMsg.datas.Add(characterSkinData);
                Message.Send(m_refreshCharacterSkinInfoListMsg);

                TheBackEnd.TheBackEndManager.Instance.DynamicUpdateData_WaitSecond(m_changeInfoUpdate, null);

                ThirdPartyLog.Instance.SendLog_Skin_StarUpEvent(characterSkinData.Index, playerSkinInfo.Star, needSkin);

                return true;
            }

            return false;
        }
        //------------------------------------------------------------------------------------
        public CharacterSkinData GetCurrentSlotSkin(V2Enum_Skin v2Enum_Skin)
        {
            return CharacterSkinOperator.GetEquipSkin(v2Enum_Skin);
        }
        //------------------------------------------------------------------------------------
        public bool IsEquipSkin(CharacterSkinData characterSkinData)
        {
            return CharacterSkinOperator.IsEquipSkin(characterSkinData);
        }
        //------------------------------------------------------------------------------------
        public void SetFakeWeaponSkin(CharacterSkinData characterSkinData)
        {
            if (characterSkinData.SkinType == V2Enum_Skin.SkinWeapon)
            {
                CharacterSkinContainer.m_fakeSkinWeaponName = string.Format(Define.WeaponResourceNameKey, characterSkinData.ResourceIndex);
                //Managers.CharacterGearManager.Instance.ChangeEquipWeaponEffect(characterSkinData.MyFakeGearData);
            }
            else if (characterSkinData.SkinType == V2Enum_Skin.SkinBody)
            {
                CharacterSkinContainer.m_fakeSkinBodyNumber = characterSkinData.ResourceIndex;
            }
        }
        //------------------------------------------------------------------------------------
        public void ReleaseFakeWeaponSkin()
        {
            //if (string.IsNullOrEmpty(CharacterSkinContainer.m_fakeSkinWeaponName) == false)
            //{
            //    CharacterSkinContainer.m_fakeSkinWeaponName = string.Empty;

            //    if (string.IsNullOrEmpty(CharacterSkinContainer.m_currentSkinWeaponName) == false)
            //    {
            //        CharacterSkinData current = GetCurrentSlotSkin(V2Enum_Skin.SkinWeapon);
            //        if (current != null)
            //            Managers.CharacterGearManager.Instance.ChangeEquipWeaponEffect(current.MyFakeGearData);
            //        else
            //            Managers.CharacterGearManager.Instance.ChangeEquipWeaponEffect(tempCharacterGearData);
            //    }
            //    else
            //    {
            //        Managers.CharacterGearManager.Instance.ChangeEquipWeaponEffect(CharacterGearManager.Instance.GetCurrentSlotGear(V2Enum_Goods.Gear));
            //    }
            //}

            CharacterSkinContainer.m_fakeSkinBodyNumber = -1;
        }
        //------------------------------------------------------------------------------------
        public bool DoEquipSkin(CharacterSkinData characterSkinData)
        {
            if (IsEquipSkin(characterSkinData) == true)
                return false;

            PlayerSkinInfo playerSkinInfo = GetPlayerSkinInfo(characterSkinData);
            if (playerSkinInfo == null)
                return false;

            CharacterSkinData beforeCharacterSkinData = GetCurrentSlotSkin(characterSkinData.SkinType);
            if (CharacterSkinOperator.DoEquipSkin(characterSkinData) == false)
                return false;

            if (characterSkinData.SkinType == V2Enum_Skin.SkinWeapon)
            {
                CharacterSkinContainer.m_currentSkinWeaponName = string.Format(Define.WeaponResourceNameKey, characterSkinData.ResourceIndex);
                //Managers.CharacterGearManager.Instance.ChangeEquipWeaponEffect(characterSkinData.MyFakeGearData);
            }
            else if (characterSkinData.SkinType == V2Enum_Skin.SkinBody)
            { 
                CharacterSkinContainer.m_currentSkinBodyNumber = characterSkinData.ResourceIndex;
            }

            m_changeCharacterSkinInfoMsg.V2Enum_Skin = characterSkinData.SkinType;
            m_changeCharacterSkinInfoMsg.BeforeGear = beforeCharacterSkinData;
            m_changeCharacterSkinInfoMsg.AfterGear = characterSkinData;
            Message.Send(m_changeCharacterSkinInfoMsg);


            TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSkinTable);

            return true;
        }
        //------------------------------------------------------------------------------------
        public void DoUnEquipSkin(V2Enum_Skin SkinType)
        {
            CharacterSkinData beforeCharacterSkinData = GetCurrentSlotSkin(SkinType);

            CharacterSkinOperator.UhEquipSkin(SkinType);

            if (SkinType == V2Enum_Skin.SkinWeapon)
            {
                ReleaseFakeWeaponSkin();

                //Managers.CharacterGearManager.Instance.ChangeEquipWeaponEffect(CharacterGearManager.Instance.GetCurrentSlotGear(V2Enum_Goods.Gear));
                CharacterSkinContainer.m_currentSkinWeaponName = string.Empty;
            }
            else if (SkinType == V2Enum_Skin.SkinBody)
            {
                ReleaseFakeWeaponSkin();

                CharacterSkinContainer.m_currentSkinBodyNumber = -1;
            }

            m_changeCharacterSkinInfoMsg.V2Enum_Skin = SkinType;
            m_changeCharacterSkinInfoMsg.BeforeGear = beforeCharacterSkinData;
            m_changeCharacterSkinInfoMsg.AfterGear = null;
            Message.Send(m_changeCharacterSkinInfoMsg);


            TheBackEnd.TheBackEndManager.Instance.AddUpdateWaitDatas(Define.PlayerSkinTable);
        }
        //------------------------------------------------------------------------------------
    }
}