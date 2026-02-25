using LitJson;
using BackEnd;
using System.Collections.Generic;
using GameBerry.Chart;
using Spine;

namespace GameBerry.Table
{
    /// <summary>
    /// ?§ÌÇ¨ ?¨Î°Ø ?∞Ïù¥??(?°Ìã∞Î∏??§ÌÇ¨ ?•Ï∞©??
    /// </summary>
    public class SkillSlotData : IPackable
    {
        public int slotIndex; // 0~4 (5Í∞??¨Î°Ø)
        public int skillId;   // ?•Ï∞©???§ÌÇ¨ ID (0?¥Î©¥ Îπ??¨Î°Ø)

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
    /// ?†Ï?Í∞Ä Î≥¥Ïú†???§ÌÇ¨ ?∞Ïù¥??    /// </summary>
    public class SkillData : IPackable
    {
        public int skillId;
        public int level; // ?§ÌÇ¨ ?àÎ≤® (Í∏∞Î≥∏ 1)

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

        public const int MaxSlotCount = 5; // ?§ÌÇ¨ ?¨Î°Ø ÏµúÎ? Í∞úÏàò

        //------------------------------------------------------------------------------------
        public override void SetData(JsonData data)
        {
            if (data == null || data.Count == 0) { InitializeSlots(); return; }

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

            // ?¨Î°Ø Ï¥àÍ∏∞??(Îπ??¨Î°Ø?¥Î©¥ ?ùÏÑ±)
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
            if (skillSlots == null)
                skillSlots = new List<SkillSlotData>();

            // ?¨Î°Ø??Î∂ÄÏ°±ÌïòÎ©?Îπ??¨Î°Ø Ï∂îÍ?
            while (skillSlots.Count < MaxSlotCount)
            {
                skillSlots.Add(new SkillSlotData { slotIndex = skillSlots.Count, skillId = 0 });
            }

            // ?¨Î°Ø ?∏Îç±???ïÎ†¨ Î∞?Í≤ÄÏ¶?            skillSlots.Sort((a, b) => a.slotIndex.CompareTo(b.slotIndex));

            for (int i = 0; i < skillSlots.Count; i++)
            {
                skillSlots[i].slotIndex = i;
            }
        }
        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        private void EnsureSlotsInitialized()
        {
            if (skillSlots == null || skillSlots.Count < MaxSlotCount)
                InitializeSlots();
        }
        #region Skill Ownership & Level
        //------------------------------------------------------------------------------------
        /// <summary>
        /// ?§ÌÇ¨ ?¥Í∏à (Î≥¥Ïú†)
        /// </summary>
        public bool UnlockSkill(int skillId)
        {
            if (skillDataDict.ContainsKey(skillId))
                return false; // ?¥Î? Î≥¥Ïú† Ï§?
            skillDataDict.Add(skillId, new SkillData { skillId = skillId, level = 1 });
            return true;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// ?§ÌÇ¨ Î≥¥Ïú† ?¨Î? ?ïÏù∏
        /// </summary>
        public bool HasSkill(int skillId)
        {
            return skillDataDict.ContainsKey(skillId);
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// ?§ÌÇ¨ ?∞Ïù¥??Í∞Ä?∏Ïò§Í∏?        /// </summary>
        public SkillData GetSkillData(int skillId)
        {
            if (skillDataDict.TryGetValue(skillId, out var data))
                return data;

            return null;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// ?§ÌÇ¨ ?àÎ≤® Í∞Ä?∏Ïò§Í∏?        /// </summary>
        public int GetSkillLevel(int skillId)
        {
            if (skillDataDict.TryGetValue(skillId, out var data))
                return data.level;

            return 0;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// ?§ÌÇ¨ ?àÎ≤® ??        /// </summary>
        public bool LevelUpSkill(int skillId)
        {
            if (!skillDataDict.TryGetValue(skillId, out var data))
                return false;

            data.level++;
            return true;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// ?§ÌÇ¨ ?àÎ≤® ?§Ï†ï
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
        /// Î≥¥Ïú†??Î™®Îì† ?§ÌÇ¨ Î™©Î°ù
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
        /// ?¨Î°Ø???§ÌÇ¨ ?•Ï∞©
        /// </summary>
        public bool EquipSkillToSlot(int slotIndex, int skillId)
        {
            EnsureSlotsInitialized();

            if (slotIndex < 0 || slotIndex >= MaxSlotCount)
                return false;

            if (!HasSkill(skillId))
                return false; // Î≥¥Ïú†?òÏ? ?äÏ? ?§ÌÇ¨

            // ?¥Î? ?§Î•∏ ?¨Î°Ø???•Ï∞©?òÏñ¥ ?àÎäîÏßÄ ?ïÏù∏
            for (int i = 0; i < skillSlots.Count; i++)
            {
                if (skillSlots[i].skillId == skillId && i != slotIndex)
                {
                    // Í∏∞Ï°¥ ?¨Î°Ø ?¥Ï†ú
                    skillSlots[i].skillId = 0;
                }
            }

            skillSlots[slotIndex].skillId = skillId;
            return true;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// ?¨Î°Ø?êÏÑú ?§ÌÇ¨ ?¥Ï†ú
        /// </summary>
        public bool UnequipSkillFromSlot(int slotIndex)
        {
            EnsureSlotsInitialized();

            if (slotIndex < 0 || slotIndex >= MaxSlotCount)
                return false;

            skillSlots[slotIndex].skillId = 0;
            return true;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// ?πÏ†ï ?¨Î°Ø???•Ï∞©???§ÌÇ¨ ID Í∞Ä?∏Ïò§Í∏?        /// </summary>
        public int GetEquippedSkillId(int slotIndex)
        {
            EnsureSlotsInitialized();

            if (slotIndex < 0 || slotIndex >= MaxSlotCount)
                return 0;

            SkillSlotData skillSlot = skillSlots.Find(s => s.slotIndex == slotIndex);
            if (skillSlot == null)                
                return 0;


            return skillSlot.skillId;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// ?§ÌÇ¨???•Ï∞©???¨Î°Ø ?∏Îç±??Ï∞æÍ∏∞ (-1?¥Î©¥ ÎØ∏Ïû•Ï∞?
        /// </summary>
        public int FindSlotIndexBySkillId(int skillId)
        {
            EnsureSlotsInitialized();

            for (int i = 0; i < skillSlots.Count; i++)
            {
                if (skillSlots[i].skillId == skillId)
                    return i;
            }

            return -1;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// ?§ÌÇ¨???•Ï∞©?òÏñ¥ ?àÎäîÏßÄ ?ïÏù∏
        /// </summary>
        public bool IsSkillEquipped(int skillId)
        {
            return FindSlotIndexBySkillId(skillId) >= 0;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// Î™®Îì† ?¨Î°Ø ?∞Ïù¥??Í∞Ä?∏Ïò§Í∏?        /// </summary>
        public List<SkillSlotData> GetAllSlots()
        {
            EnsureSlotsInitialized();

            return skillSlots;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
    }
}
