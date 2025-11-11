using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;

using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

using System.Collections.Concurrent;
using CodeStage.AntiCheat.ObscuredTypes;

namespace GameBerry
{
    public class SkillBase
    {
        public int Index;
    }

    public class CharacterSkillData : SkillBaseData
    {
        public V2Enum_ElementType ElementType;

        public int MaxLevel;

        public int LevelUpCostAmountBase;
        public int LevelUpCostAmountMod1;
        public int LevelUpCostAmountMod2;

        public V2Enum_Stat OwnEffectType;
        public double OwnEffectBaseValue;
        public double OwnEffectLevelUpValue;

        public string NameLocalStringKey;
        public string DescLocalStringKey;
        public string IconStringKey;
        public string AniStringKey;
    }


    /// <summary>
    /// 캐릭터스킬슬롯
    /// </summary>
    public class CharacterSkillSlotData
    {
        public int Index;
        public int SlotNumber;
        public V2Enum_OpenConditionType OpenConditionType;
        public int OpenConditionParam;
    }

    public class CharacterSkillLocalTable : LocalTableBase
    {
        private CharacterSkillData m_characterSkillData_BasicAttack = null;
        private List<CharacterSkillData> m_characterSkillDatas = new List<CharacterSkillData>();
        private ConcurrentDictionary<int, CharacterSkillData> m_characterTotalSkillDatas_Dic = new ConcurrentDictionary<int, CharacterSkillData>();

        private List<CharacterSkillSlotData> m_characterSkillSlotDatas = new List<CharacterSkillSlotData>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            //await SetListToDic_Async<SkillDamageData>("CharacterSkillDamage");
            //await SetListToDic_Async<SkillEffectData>("CharacterSkillCrowdControl");

            string jsonstring = ClientLocalChartManager.GetLocalChartData_V2("CharacterSkillSlot.json");
            m_characterSkillSlotDatas = JsonConvert.DeserializeObject<List<CharacterSkillSlotData>>(jsonstring);

            m_characterSkillDatas = await TheBackEnd.TheBackEnd_GameChart.GetListDat_Async<CharacterSkillData>("CharacterSkill");

            for (int i = 0; i < m_characterSkillDatas.Count; ++i)
            {
                m_characterSkillDatas[i].NameLocalStringKey = string.Format("charSkill/{0}/name", m_characterSkillDatas[i].ResourceIndex);
                m_characterSkillDatas[i].DescLocalStringKey = string.Format("charSkill/{0}/desc", m_characterSkillDatas[i].ResourceIndex);
                m_characterSkillDatas[i].IconStringKey = string.Format("charSkill/{0}/icon", m_characterSkillDatas[i].ResourceIndex);
                m_characterSkillDatas[i].AniStringKey = string.Format("{0}", m_characterSkillDatas[i].ResourceIndex);
            }

            m_characterSkillData_BasicAttack = m_characterSkillDatas.Find(x => x.TriggerType == V2Enum_ARR_TriggerType.Default);
            m_characterSkillDatas.Remove(m_characterSkillData_BasicAttack);

            //SetListToDic(m_characterSkillDatas);


            //Contents.DataLoadContent.LoadTable.Add(null);
            //TheBackEnd.TheBackEndManager.Instance.GetPlayerSkillinfoTableData();
        }
        //------------------------------------------------------------------------------------
        private async UniTask SetListToDic_Async<T>(string jsonStr) where T : SkillBase
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart(jsonStr, o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            List<T> m_characterSkillDamageDatas = JsonConvert.DeserializeObject<List<T>>(rows.ToJson());

            for (int i = 0; i < m_characterSkillDamageDatas.Count; ++i)
            {
                //m_characterTotalSkillDatas_Dic.TryAdd(m_characterSkillDamageDatas[i].Index, m_characterSkillDamageDatas[i]);
            }
        }
        //------------------------------------------------------------------------------------
        private void SetListToDic<T>(List<T> tlist) where T : SkillBase
        {
            for (int i = 0; i < tlist.Count; ++i)
            {
                //m_characterTotalSkillDatas_Dic.TryAdd(tlist[i].Index, tlist[i]);
            }
        }
        //------------------------------------------------------------------------------------
        public CharacterSkillData GetBasicAttackData()
        {
            return m_characterSkillData_BasicAttack;
        }
        //------------------------------------------------------------------------------------
        public List<CharacterSkillData> GetAllData()
        {
            return m_characterSkillDatas;
        }
        //------------------------------------------------------------------------------------
        public T GetData<T>(int index) where T : CharacterSkillData
        {
            if (m_characterTotalSkillDatas_Dic.ContainsKey(index) == true)
                return m_characterTotalSkillDatas_Dic[index] as T;

            return null;
        }
        //------------------------------------------------------------------------------------
        public List<CharacterSkillSlotData> GetAllSlotData()
        {
            return m_characterSkillSlotDatas;
        }
        //------------------------------------------------------------------------------------
    }
}