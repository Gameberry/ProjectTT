using LitJson;
using BackEnd;
using System.Collections.Generic;
using GameBerry.Chart;

namespace GameBerry.Table
{
    /// <summary>
    /// 스킬 슬롯 데이터 (액티브 스킬 장착용)
    /// </summary>
    public class SkillSlotData : IPackable
    {
        public int slotIndex; // 0~4 (5개 슬롯)
        public int skillId;   // 장착된 스킬 ID (0이면 빈 슬롯)

        public string Pack() => $"{PackUtil.PackValue(slotIndex)},{PackUtil.PackValue(skillId)}";

        public void Unpack(string str)
        {
            slotIndex = 0;
            skillId = 0;

            if (string.IsNullOrEmpty(str))
                return;

            var sp = str.Split(',');

            if (sp.Length >= 1)
                slotIndex = PackUtil.UnpackValue<int>(sp[0]);

            if (sp.Length >= 2)
                skillId = PackUtil.UnpackValue<int>(sp[1]);
        }
    }

    /// <summary>
    /// 유저가 보유한 스킬 데이터
    /// </summary>
    public class SkillData : IPackable
    {
        public int skillId;
        public int level; // 스킬 레벨 (기본 1)

        public string Pack() => $"{PackUtil.PackValue(skillId)},{PackUtil.PackValue(level)}";

        public void Unpack(string str)
        {
            skillId = 0;
            level = 1;

            if (string.IsNullOrEmpty(str))
                return;

            var sp = str.Split(',');

            if (sp.Length >= 1)
                skillId = PackUtil.UnpackValue<int>(sp[0]);

            if (sp.Length >= 2)
                level = PackUtil.UnpackValue<int>(sp[1]);
        }
    }

    public class SkillTable : TableBase
    {
        private const string skillDataKey = "Skills";
        private Dictionary<int, SkillData> skillDataDict = new Dictionary<int, SkillData>();

        private const string skillSlotsKey = "SkillSlots";
        private List<SkillSlotData> skillSlots = new List<SkillSlotData>();

        public const int MaxSlotCount = 5; // 스킬 슬롯 최대 개수

        //------------------------------------------------------------------------------------
        public override void SetData(JsonData data)
        {
            if (data == null || data.Count == 0) return;

            for (int i = 0; i < data.Count; ++i)
            {
                foreach (var key in data[i].Keys)
                {
                    if (key == "inDate")
                        SetInData(data[i][key].ToString());
                    else if (key == skillDataKey)
                        skillDataDict = PackUtil.UnpackDict<int, SkillData>(data[i][key].ToString());
                    else if (key == skillSlotsKey)
                        skillSlots = PackUtil.UnpackList<SkillSlotData>(data[i][key].ToString());
                }
            }

            // 슬롯 초기화 (빈 슬롯이면 생성)
            InitializeSlots();
        }
        //------------------------------------------------------------------------------------
        public override Param GetParam()
        {
            Param p = new Param();
            p.Add(skillDataKey, PackUtil.PackDict(skillDataDict));
            p.Add(skillSlotsKey, PackUtil.PackList(skillSlots));
            return p;
        }
        //------------------------------------------------------------------------------------
        private void InitializeSlots()
        {
            // 슬롯이 부족하면 빈 슬롯 추가
            while (skillSlots.Count < MaxSlotCount)
            {
                skillSlots.Add(new SkillSlotData { slotIndex = skillSlots.Count, skillId = 0 });
            }

            // 슬롯 인덱스 정렬 및 검증
            skillSlots.Sort((a, b) => a.slotIndex.CompareTo(b.slotIndex));

            for (int i = 0; i < skillSlots.Count; i++)
            {
                skillSlots[i].slotIndex = i;
            }
        }
        //------------------------------------------------------------------------------------
        #region Skill Ownership & Level
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬 해금 (보유)
        /// </summary>
        public bool UnlockSkill(int skillId)
        {
            if (skillDataDict.ContainsKey(skillId))
                return false; // 이미 보유 중

            skillDataDict.Add(skillId, new SkillData { skillId = skillId, level = 1 });
            return true;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬 보유 여부 확인
        /// </summary>
        public bool HasSkill(int skillId)
        {
            return skillDataDict.ContainsKey(skillId);
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬 데이터 가져오기
        /// </summary>
        public SkillData GetSkillData(int skillId)
        {
            if (skillDataDict.TryGetValue(skillId, out var data))
                return data;

            return null;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬 레벨 가져오기
        /// </summary>
        public int GetSkillLevel(int skillId)
        {
            if (skillDataDict.TryGetValue(skillId, out var data))
                return data.level;

            return 0;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬 레벨 업
        /// </summary>
        public bool LevelUpSkill(int skillId)
        {
            if (!skillDataDict.TryGetValue(skillId, out var data))
                return false;

            data.level++;
            return true;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬 레벨 설정
        /// </summary>
        public bool SetSkillLevel(int skillId, int level)
        {
            if (!skillDataDict.TryGetValue(skillId, out var data))
                return false;

            if (level < 1)
                level = 1;

            data.level = level;
            return true;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 보유한 모든 스킬 목록
        /// </summary>
        public Dictionary<int, SkillData> GetAllSkills()
        {
            return skillDataDict;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Skill Slots (Active Skills Only)
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 슬롯에 스킬 장착
        /// </summary>
        public bool EquipSkillToSlot(int slotIndex, int skillId)
        {
            if (slotIndex < 0 || slotIndex >= MaxSlotCount)
                return false;

            if (!HasSkill(skillId))
                return false; // 보유하지 않은 스킬

            // 이미 다른 슬롯에 장착되어 있는지 확인
            for (int i = 0; i < skillSlots.Count; i++)
            {
                if (skillSlots[i].skillId == skillId && i != slotIndex)
                {
                    // 기존 슬롯 해제
                    skillSlots[i].skillId = 0;
                }
            }

            skillSlots[slotIndex].skillId = skillId;
            return true;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 슬롯에서 스킬 해제
        /// </summary>
        public bool UnequipSkillFromSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxSlotCount)
                return false;

            skillSlots[slotIndex].skillId = 0;
            return true;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 특정 슬롯에 장착된 스킬 ID 가져오기
        /// </summary>
        public int GetEquippedSkillId(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxSlotCount)
                return 0;

            return skillSlots[slotIndex].skillId;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬이 장착된 슬롯 인덱스 찾기 (-1이면 미장착)
        /// </summary>
        public int FindSlotIndexBySkillId(int skillId)
        {
            for (int i = 0; i < skillSlots.Count; i++)
            {
                if (skillSlots[i].skillId == skillId)
                    return i;
            }

            return -1;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 스킬이 장착되어 있는지 확인
        /// </summary>
        public bool IsSkillEquipped(int skillId)
        {
            return FindSlotIndexBySkillId(skillId) >= 0;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 모든 슬롯 데이터 가져오기
        /// </summary>
        public List<SkillSlotData> GetAllSlots()
        {
            return skillSlots;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
    }
}
