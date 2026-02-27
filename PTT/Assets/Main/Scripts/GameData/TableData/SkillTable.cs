using LitJson;
using BackEnd;
using System.Collections.Generic;
using GameBerry.Chart;
using Spine;

namespace GameBerry.Table
{
    /// <summary>
    /// ?ㅽ궗 ?щ’ ?곗씠??(?≫떚釉??ㅽ궗 ?μ갑??
    /// </summary>
    public class SkillSlotData : IPackable
    {
        public int slotIndex; // 0~4 (5媛??щ’)
        public int skillId;   // ?μ갑???ㅽ궗 ID (0?대㈃ 鍮??щ’)

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
    /// ?좎?媛 蹂댁쑀???ㅽ궗 ?곗씠??    /// </summary>
    public class SkillData : IPackable
    {
        public int skillId;
        public int level; // ?ㅽ궗 ?덈꺼 (湲곕낯 1)

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

        public const int MaxSlotCount = 5; // ?ㅽ궗 ?щ’ 理쒕? 媛쒖닔

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

            // ?щ’ 珥덇린??(鍮??щ’?대㈃ ?앹꽦)
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

            // ?щ’??遺議깊븯硫?鍮??щ’ 異붽?
            while (skillSlots.Count < MaxSlotCount)
            {
                skillSlots.Add(new SkillSlotData { slotIndex = skillSlots.Count, skillId = 0 });
            }

            // ?щ’ ?몃뜳???뺣젹 諛?寃利?            skillSlots.Sort((a, b) => a.slotIndex.CompareTo(b.slotIndex));

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
        /// ?ㅽ궗 ?닿툑 (蹂댁쑀)
        /// </summary>
        public bool UnlockSkill(int skillId)
        {
            if (skillDataDict.ContainsKey(skillId))
                return false; // ?대? 蹂댁쑀 以?
            skillDataDict.Add(skillId, new SkillData { skillId = skillId, level = 1 });
            return true;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// ?ㅽ궗 蹂댁쑀 ?щ? ?뺤씤
        /// </summary>
        public bool HasSkill(int skillId)
        {
            return skillDataDict.ContainsKey(skillId);
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// ?ㅽ궗 ?곗씠??媛?몄삤湲?        /// </summary>
        public SkillData GetSkillData(int skillId)
        {
            if (skillDataDict.TryGetValue(skillId, out var data))
                return data;

            return null;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// ?ㅽ궗 ?덈꺼 媛?몄삤湲?        /// </summary>
        public int GetSkillLevel(int skillId)
        {
            if (skillDataDict.TryGetValue(skillId, out var data))
                return data.level;

            return 0;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// ?ㅽ궗 ?덈꺼 ??        /// </summary>
        public bool LevelUpSkill(int skillId)
        {
            if (!skillDataDict.TryGetValue(skillId, out var data))
                return false;

            data.level++;
            return true;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// ?ㅽ궗 ?덈꺼 ?ㅼ젙
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
        /// 蹂댁쑀??紐⑤뱺 ?ㅽ궗 紐⑸줉
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
        /// ?щ’???ㅽ궗 ?μ갑
        /// </summary>
        public bool EquipSkillToSlot(int slotIndex, int skillId)
        {
            EnsureSlotsInitialized();

            if (slotIndex < 0 || slotIndex >= MaxSlotCount)
                return false;

            if (!HasSkill(skillId))
                return false; // 蹂댁쑀?섏? ?딆? ?ㅽ궗

            // ?대? ?ㅻⅨ ?щ’???μ갑?섏뼱 ?덈뒗吏 ?뺤씤
            for (int i = 0; i < skillSlots.Count; i++)
            {
                if (skillSlots[i].skillId == skillId && i != slotIndex)
                {
                    // 湲곗〈 ?щ’ ?댁젣
                    skillSlots[i].skillId = 0;
                }
            }

            skillSlots[slotIndex].skillId = skillId;
            return true;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// ?щ’?먯꽌 ?ㅽ궗 ?댁젣
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
        /// ?뱀젙 ?щ’???μ갑???ㅽ궗 ID 媛?몄삤湲?        /// </summary>
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
        /// ?ㅽ궗???μ갑???щ’ ?몃뜳??李얘린 (-1?대㈃ 誘몄옣李?
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
        /// ?ㅽ궗???μ갑?섏뼱 ?덈뒗吏 ?뺤씤
        /// </summary>
        public bool IsSkillEquipped(int skillId)
        {
            return FindSlotIndexBySkillId(skillId) >= 0;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 紐⑤뱺 ?щ’ ?곗씠??媛?몄삤湲?        /// </summary>
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
