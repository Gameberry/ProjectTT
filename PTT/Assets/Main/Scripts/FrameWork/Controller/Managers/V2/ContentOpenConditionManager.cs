using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.Managers
{
    public class OpenDeleGateContainer
    {
        public RefreshOpenCondition RefreshOpenCondition;
    }

    public class ContentOpenConditionManager : MonoSingleton<ContentOpenConditionManager>
    {
        private Dictionary<V2Enum_OpenConditionType, OpenDeleGateContainer> m_openConditionEvent = new Dictionary<V2Enum_OpenConditionType, OpenDeleGateContainer>();

        private ContentUnlockLocalTable m_contentUnlockLocalTable;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            m_contentUnlockLocalTable = TableManager.Instance.GetTableClass<ContentUnlockLocalTable>();
        }
        //------------------------------------------------------------------------------------
        public void InitManager()
        {
        }
        //------------------------------------------------------------------------------------
        public ContentUnlockData GetContentUnlockData(V2Enum_ContentType v2Enum_ContentType)
        {
            return m_contentUnlockLocalTable.GetContentUnlockData(v2Enum_ContentType);
        }
        //------------------------------------------------------------------------------------
        public ContentsUnlockRangeData GetContentsUnlockRangeData(int index)
        {
            return m_contentUnlockLocalTable.GetContentsUnlockRangeData(index);
        }
        //------------------------------------------------------------------------------------
        public bool IsOpen(V2Enum_ContentType v2Enum_ContentType)
        {
            if (SceneManager.Instance.BuildElement == BuildEnvironmentEnum.Stage)
                return true;

            ContentUnlockData contentUnlockData = GetContentUnlockData(v2Enum_ContentType);

            if (contentUnlockData == null)
                return true;

            return IsOpen(contentUnlockData.UnlockConditionType, contentUnlockData.UnlockConditionParam);
        }
        //------------------------------------------------------------------------------------
        public bool IsOpen(V2Enum_OpenConditionType v2Enum_OpenConditionType, int openLevel)
        {
            switch (v2Enum_OpenConditionType)
            {
                case V2Enum_OpenConditionType.Stage:
                    {
                        return MapManager.Instance.GetMapMaxClear() >= openLevel;
                    }
                case V2Enum_OpenConditionType.CharacterLevel:
                    {
                        return ARRRStatManager.Instance.GetCharacterLevel() >= openLevel;
                    }
                case V2Enum_OpenConditionType.StackLogin:
                    {
                        return TimeContainer.AccumLoginCount >= openLevel;
                    }
                case V2Enum_OpenConditionType.StackSkillCount:
                    {
                        return SynergyContainer.SynergyInfo.Count >= openLevel;
                    }
                case V2Enum_OpenConditionType.GetGear:
                    {
                        return true;
                    }
                case V2Enum_OpenConditionType.BreakthroughCount:
                    {
                        return ARRRStatManager.Instance.GetCharacterARRRLimitCompleteLevel() >= openLevel;
                    }
                case V2Enum_OpenConditionType.Wave:
                    {
                        int stage = openLevel / 100;
                        int wave = openLevel % 100;
                        if (stage > MapManager.Instance.GetMapMaxClear())
                            return false;
                        else if (stage < MapManager.Instance.GetMapMaxClear())
                            return true;

                        StageInfo stageInfo = MapManager.Instance.GetStageInfo(stage);
                        if (stageInfo == null)
                            return false;

                        return stageInfo.LastClearWave >= wave;
                    }
                case V2Enum_OpenConditionType.StaminaStack:
                    {
                        return openLevel >= GetOpenConditionCurrentValue(v2Enum_OpenConditionType);
                    }
                case V2Enum_OpenConditionType.WaveDeath:
                    {
                        ContentsUnlockRangeData waveDeathRangeData = GetContentsUnlockRangeData(openLevel);
                        if (waveDeathRangeData == null)
                            return false;


                        return waveDeathRangeData.Param1 <= MapContainer.LastFailWave && waveDeathRangeData.Param2 >= MapContainer.LastFailWave;
                    }
                case V2Enum_OpenConditionType.DungeonDiffClear:
                    {
                        ContentsUnlockRangeData waveDeathRangeData = GetContentsUnlockRangeData(openLevel);
                        if (waveDeathRangeData == null)
                            return false;

                        Enum_Dungeon enumDungeon = waveDeathRangeData.Param1.IntToEnum32<Enum_Dungeon>();

                        return waveDeathRangeData.Param2 <= Managers.DungeonDataManager.Instance.GetDungeonRecord(enumDungeon);
                    }
                case V2Enum_OpenConditionType.StaminaCount:
                case V2Enum_OpenConditionType.RelicSummonCount:
                case V2Enum_OpenConditionType.SummonCount:
                case V2Enum_OpenConditionType.GearSummonCount:
                case V2Enum_OpenConditionType.RelicLevelStack:
                case V2Enum_OpenConditionType.SynergySkillLevelStack:
                case V2Enum_OpenConditionType.DescendSkillLevelStack:
                case V2Enum_OpenConditionType.RuneSummonCount:
                case V2Enum_OpenConditionType.RuneCombineCount:


                case V2Enum_OpenConditionType.FireSynergyLevelStack:
                case V2Enum_OpenConditionType.GoldSynergyLevelStack:
                case V2Enum_OpenConditionType.WaterSynergyLevelStack:
                case V2Enum_OpenConditionType.ThunderSynergyLevelStack:

                case V2Enum_OpenConditionType.FireSynergyBreakStack:
                case V2Enum_OpenConditionType.GoldSynergyBreakStack:
                case V2Enum_OpenConditionType.WaterSynergyBreakStack:
                case V2Enum_OpenConditionType.ThunderSynergyBreakStack:

                case V2Enum_OpenConditionType.Synergy4GradeSkillCount:
                    {
                        return GetOpenConditionCurrentValue(v2Enum_OpenConditionType) >= openLevel;
                    }
                case V2Enum_OpenConditionType.GetSynergySkill:
                    {
                        SkillInfo skillInfo = Managers.SynergyManager.Instance.GetSynergyEffectSkillInfo(openLevel);
                        
                        return skillInfo != null;
                    }
                case V2Enum_OpenConditionType.GetDescendSkill:
                    {
                        SkillInfo skillInfo = Managers.DescendManager.Instance.GetSynergyEffectSkillInfo(openLevel);

                        return skillInfo != null;
                    }
                case V2Enum_OpenConditionType.NoAdBuff:
                    {
                        ContentsUnlockRangeData waveDeathRangeData = GetContentsUnlockRangeData(openLevel);
                        if (waveDeathRangeData == null)
                            return false;

                        VipPackageShopData vipPackageShopData = VipPackageManager.Instance.GetVipPackageShopData(waveDeathRangeData.Param1);
                        if (vipPackageShopData == null)
                            return false;

                        VipPackageShopInfo vipPackageShopInfo = VipPackageManager.Instance.GetVipPackageShopInfo(vipPackageShopData);
                        if (vipPackageShopInfo != null)
                            return false;

                        return TimeContainer.AccumLoginTime > TimeManager.Instance.GetInitAddTime(V2Enum_IntervalType.Day, waveDeathRangeData.Param2);
                    }
                case V2Enum_OpenConditionType.WaveReward:
                    {
                        int stage = openLevel / 100;
                        int wave = openLevel % 100;
                        StageInfo stageInfo = MapManager.Instance.GetStageInfo(stage);
                        if (stageInfo == null)
                            return false;

                        return stageInfo.RecvClearReward >= wave;
                    }
            }

            return true;
        }
        //------------------------------------------------------------------------------------
        public int GetOpenConditionCurrentValue(V2Enum_OpenConditionType v2Enum_OpenConditionType)
        {
            int sendValue = 0;

            switch (v2Enum_OpenConditionType)
            {
                case V2Enum_OpenConditionType.Stage:
                    {
                        sendValue = MapManager.Instance.GetMapMaxClear();
                        break;
                    }
                case V2Enum_OpenConditionType.CharacterLevel:
                    {
                        sendValue = ARRRStatManager.Instance.GetCharacterLevel();
                        break;
                    }
                case V2Enum_OpenConditionType.StackLogin:
                    {
                        sendValue = TimeContainer.AccumLoginCount;
                        break;
                    }
                case V2Enum_OpenConditionType.StackSkillCount:
                    {
                        sendValue = SynergyContainer.SynergyInfo.Count;
                        break;
                    }
                case V2Enum_OpenConditionType.GetGear:
                    {
                        break;
                    }
                case V2Enum_OpenConditionType.BreakthroughCount:
                    {
                        sendValue = ARRRStatManager.Instance.GetCharacterARRRLimitCompleteLevel();
                        break;
                    }
                case V2Enum_OpenConditionType.Wave:
                    {
                        sendValue = MapContainer.MaxWaveClear;

                        break;
                    }
                case V2Enum_OpenConditionType.StaminaStack:
                    {
                        sendValue = (int)Managers.GoodsManager.Instance.GetGoodsAmount(V2Enum_Goods.Point.Enum32ToInt(), Define.StaminaIndex);

                        break;
                    }
                case V2Enum_OpenConditionType.StaminaCount:
                    {
                        sendValue = StaminaContainer.StaminaAccumUse;

                        break;
                    }
                case V2Enum_OpenConditionType.RelicSummonCount:
                    {
                        sendValue = Managers.SummonManager.Instance.GetAccumCount(V2Enum_SummonType.SummonRelic).ToInt();

                        break;
                    }
                case V2Enum_OpenConditionType.SummonCount:
                    {
                        sendValue = Managers.SummonManager.Instance.GetAccumCount(V2Enum_SummonType.SummonNormal).ToInt();

                        break;
                    }
                case V2Enum_OpenConditionType.GearSummonCount:
                    {
                        sendValue = Managers.SummonManager.Instance.GetAccumCount(V2Enum_SummonType.SummonGear).ToInt();

                        break;
                    }
                case V2Enum_OpenConditionType.RelicLevelStack:
                    {
                        sendValue = RelicContainer.SynergyAccumLevel;

                        break;
                    }
                case V2Enum_OpenConditionType.SynergySkillLevelStack:
                    {
                        sendValue = SynergyContainer.SynergyEffectAccumLevel;

                        break;
                    }
                case V2Enum_OpenConditionType.DescendSkillLevelStack:
                    {
                        sendValue = DescendContainer.SynergyAccumLevel;

                        break;
                    }
                case V2Enum_OpenConditionType.RuneSummonCount:
                    {
                        sendValue = Managers.SummonManager.Instance.GetAccumCount(V2Enum_SummonType.SummonRune).ToInt();

                        break;
                    }
                case V2Enum_OpenConditionType.RuneCombineCount:
                    {
                        sendValue = SynergyRuneContainer.AccumCombineCount;

                        break;
                    }



                case V2Enum_OpenConditionType.FireSynergyLevelStack:
                    {
                        sendValue = Managers.SynergyManager.Instance.GetSynergyAccumLevel(Enum_SynergyType.Red);

                        break;
                    }
                case V2Enum_OpenConditionType.GoldSynergyLevelStack:
                    {
                        sendValue = Managers.SynergyManager.Instance.GetSynergyAccumLevel(Enum_SynergyType.Yellow);

                        break;
                    }
                case V2Enum_OpenConditionType.WaterSynergyLevelStack:
                    {
                        sendValue = Managers.SynergyManager.Instance.GetSynergyAccumLevel(Enum_SynergyType.Blue);

                        break;
                    }
                case V2Enum_OpenConditionType.ThunderSynergyLevelStack:
                    {
                        sendValue = Managers.SynergyManager.Instance.GetSynergyAccumLevel(Enum_SynergyType.White);

                        break;
                    }


                case V2Enum_OpenConditionType.FireSynergyBreakStack:
                    {
                        sendValue = Managers.SynergyManager.Instance.GetSynergyAccumBreakLevel(Enum_SynergyType.Red);

                        break;
                    }
                case V2Enum_OpenConditionType.GoldSynergyBreakStack:
                    {
                        sendValue = Managers.SynergyManager.Instance.GetSynergyAccumBreakLevel(Enum_SynergyType.Yellow);

                        break;
                    }
                case V2Enum_OpenConditionType.WaterSynergyBreakStack:
                    {
                        sendValue = Managers.SynergyManager.Instance.GetSynergyAccumBreakLevel(Enum_SynergyType.Blue);

                        break;
                    }
                case V2Enum_OpenConditionType.ThunderSynergyBreakStack:
                    {
                        sendValue = Managers.SynergyManager.Instance.GetSynergyAccumBreakLevel(Enum_SynergyType.White);

                        break;
                    }
                case V2Enum_OpenConditionType.Synergy4GradeSkillCount:
                    {
                        for (int i = Enum_SynergyType.Red.Enum32ToInt(); i < Enum_SynergyType.Max.Enum32ToInt(); ++i)
                        {
                            Enum_SynergyType v2Enum_Stat = i.IntToEnum32<Enum_SynergyType>();

                            SynergyEffectData synergyEffectData = Managers.SynergyManager.Instance.GetEquipSynergyEffect(v2Enum_Stat, 4);

                            if (Managers.SynergyManager.Instance.IsLockSynergy(synergyEffectData) == false)
                                sendValue++;
                        }
                        break;
                    }
            }

            return sendValue;
        }
        //------------------------------------------------------------------------------------
        public void RefreshOpenCondition(V2Enum_OpenConditionType v2Enum_OpenConditionType)
        {
            if (m_openConditionEvent.ContainsKey(v2Enum_OpenConditionType) == false)
                return;

            int sendValue = GetOpenConditionCurrentValue(v2Enum_OpenConditionType);

            if (m_openConditionEvent.ContainsKey(v2Enum_OpenConditionType) == true)
            {
                OpenDeleGateContainer openDeleGateContainer = m_openConditionEvent[v2Enum_OpenConditionType];
                openDeleGateContainer.RefreshOpenCondition?.Invoke(v2Enum_OpenConditionType, sendValue);
            }
        }
        //------------------------------------------------------------------------------------
        public void AddOpenConditionEvent(V2Enum_OpenConditionType v2Enum_OpenConditionType, RefreshOpenCondition action)
        {
            if (m_openConditionEvent.ContainsKey(v2Enum_OpenConditionType) == false)
                m_openConditionEvent.Add(v2Enum_OpenConditionType, new OpenDeleGateContainer());

            m_openConditionEvent[v2Enum_OpenConditionType].RefreshOpenCondition += action;
        }
        //------------------------------------------------------------------------------------
        public void RemoveOpenConditionEvent(V2Enum_OpenConditionType v2Enum_OpenConditionType, RefreshOpenCondition action)
        {
            if (m_openConditionEvent.ContainsKey(v2Enum_OpenConditionType) == false)
                return;
            
            m_openConditionEvent[v2Enum_OpenConditionType].RefreshOpenCondition -= action;
        }
        //------------------------------------------------------------------------------------
        public void ShowOpenConditionNotice(V2Enum_OpenConditionType v2Enum_OpenConditionType, int conditionValue)
        {
            Contents.GlobalContent.ShowGlobalNotice(GetOpenContitionLocalString(v2Enum_OpenConditionType, conditionValue));
        }
        //------------------------------------------------------------------------------------
        public void ShowOpenConditionNotice(V2Enum_ContentType v2Enum_ContentType)
        {
            ContentUnlockData contentUnlockData = GetContentUnlockData(v2Enum_ContentType);
            if (contentUnlockData == null)
                return;

            ShowOpenConditionNotice(contentUnlockData.UnlockConditionType, contentUnlockData.UnlockConditionParam);
        }
        //------------------------------------------------------------------------------------
        [System.Obsolete]
        public void ShowUnLockContentNotice(V2Enum_ContentType v2Enum_ContentType)
        {
            ContentUnlockData contentUnlockData = GetContentUnlockData(v2Enum_ContentType);
            if (contentUnlockData == null)
                return;

            ShowOpenConditionNotice(contentUnlockData.UnlockConditionType, contentUnlockData.UnlockConditionParam);

            //ShowUnLockContentNotice(contentUnlockData);
        }
        //------------------------------------------------------------------------------------
        public void ShowUnLockContentNotice(ContentUnlockData contentUnlockData)
        {
            if (contentUnlockData == null)
                return;

            if (contentUnlockData.IsUnlockNotice == 0)
                return;

            //Contents.GlobalContent.ShowUnLockContentNotice(LocalStringManager.Instance.GetLocalString(string.Format("contentUnclock/{0}/name", contentUnlockData.ResourceIndex)));
        }
        //------------------------------------------------------------------------------------
        public string GetOpenContitionLocalString(V2Enum_ContentType v2Enum_OpenConditionType)
        {
            ContentUnlockData contentUnlockData = GetContentUnlockData(v2Enum_OpenConditionType);
            if (contentUnlockData == null)
                return string.Empty;

            return GetOpenContitionLocalString(contentUnlockData.UnlockConditionType, contentUnlockData.UnlockConditionParam);
        }
        //------------------------------------------------------------------------------------
        public string GetOpenContitionLocalString(V2Enum_OpenConditionType v2Enum_OpenConditionType, int conditionValue)
        {
            string localstring = string.Empty;

            string localkey = string.Format("opencondition/{0}", v2Enum_OpenConditionType.Enum32ToInt());

            switch (v2Enum_OpenConditionType)
            {
                case V2Enum_OpenConditionType.Wave:
                case V2Enum_OpenConditionType.WaveReward:
                    {
                        string stagewave = MapOperator.ConvertWaveTotalNumberToUIString(conditionValue);

                        localstring = string.Format(LocalStringManager.Instance.GetLocalString(localkey), stagewave);
                        break;
                    }
                case V2Enum_OpenConditionType.GetSynergySkill:
                    {
                        string stagewave = Managers.GoodsManager.Instance.GetGoodsLocalKey(conditionValue);

                        localstring = string.Format(LocalStringManager.Instance.GetLocalString(localkey), LocalStringManager.Instance.GetLocalString(stagewave));
                        break;
                    }
                case V2Enum_OpenConditionType.StaminaStack:
                case V2Enum_OpenConditionType.RelicSummonCount:
                case V2Enum_OpenConditionType.SummonCount:
                case V2Enum_OpenConditionType.DescendSkillLevelStack:
                    
                case V2Enum_OpenConditionType.GearSummonCount:
                case V2Enum_OpenConditionType.RuneSummonCount:
                case V2Enum_OpenConditionType.RuneCombineCount:

                case V2Enum_OpenConditionType.FireSynergyLevelStack:
                case V2Enum_OpenConditionType.GoldSynergyLevelStack:
                case V2Enum_OpenConditionType.WaterSynergyLevelStack:
                case V2Enum_OpenConditionType.ThunderSynergyLevelStack:

                case V2Enum_OpenConditionType.FireSynergyBreakStack:
                case V2Enum_OpenConditionType.GoldSynergyBreakStack:
                case V2Enum_OpenConditionType.WaterSynergyBreakStack:
                case V2Enum_OpenConditionType.ThunderSynergyBreakStack:

                    {
                        localstring = string.Format(LocalStringManager.Instance.GetLocalString(localkey), conditionValue);
                        localstring += string.Format("\n{0} : {1}", LocalStringManager.Instance.GetLocalString("ui/remaincount"),
                            conditionValue - GetOpenConditionCurrentValue(v2Enum_OpenConditionType));

                        break;
                    }
                default:
                    {
                        localstring = string.Format(LocalStringManager.Instance.GetLocalString(localkey), conditionValue);
                        break;
                    }
            }

            return localstring;
        }
        //------------------------------------------------------------------------------------
    }
}