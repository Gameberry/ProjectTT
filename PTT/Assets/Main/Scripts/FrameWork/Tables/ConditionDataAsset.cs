using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Spine;
using Spine.Unity;

namespace GameBerry
{
    [System.Serializable]
    public class ConditionMeta
    {
        public Enum_ConditionType Type;
        public ConditionCategory Category;
        public ConditionStackPolicy StackPolicy;

        public string IconKey;       // UI에서 쓸 키 (Addressable, SpriteAtlas용)
    }

    [System.Serializable]
    public class ConditionData
    {
        public string Desc = string.Empty; // 필요없는건데 인스팩터창에서 잘 보이게하려고 잠시 만듬
        public int Index;
        public Enum_ConditionType Type;

        public float Param1;
        public float Param1_Level;
        public float Param2;
        public float Param2_Level;

        public float Duration;

        public float Rate = 1.0f;

        [HideInInspector]
        public Vector3 EffectPos;
    }

    [CreateAssetMenu(fileName = "ConditionData", menuName = "Table/ConditionData", order = 1)]
    public class ConditionDataAsset : ScriptableObject
    {
        private List<ConditionMeta> _conditionMetas = new List<ConditionMeta>();

        private Dictionary<Enum_ConditionType, ConditionMeta> _table = new Dictionary<Enum_ConditionType, ConditionMeta>();

        [SerializeField]
        [ArrayElementTitle("Desc")]
        private List<ConditionData> conditionDatas = new List<ConditionData>();

        //------------------------------------------------------------------------------------
        void OnEnable()
        {
            _conditionMetas.Clear();
            Add(Enum_ConditionType.Invincible, ConditionCategory.Utility, ConditionStackPolicy.Refresh, "icon_invincible");

            Add(Enum_ConditionType.Stun, ConditionCategory.CrowdControl, ConditionStackPolicy.Refresh, "icon_stun");
            Add(Enum_ConditionType.Snare, ConditionCategory.CrowdControl, ConditionStackPolicy.Refresh, "icon_snare");
            Add(Enum_ConditionType.Slow, ConditionCategory.CrowdControl, ConditionStackPolicy.Refresh, "icon_slow");

            Add(Enum_ConditionType.Knockback, ConditionCategory.CrowdControl, ConditionStackPolicy.MergyValue, "icon_knockback");

            Add(Enum_ConditionType.AttackUp, ConditionCategory.Buff, ConditionStackPolicy.MultipleInstances, "icon_AttackUp");
            Add(Enum_ConditionType.DefenseUp, ConditionCategory.Buff, ConditionStackPolicy.MultipleInstances, "icon_DefenseUp");
            Add(Enum_ConditionType.MoveSpeedUp, ConditionCategory.Buff, ConditionStackPolicy.MultipleInstances, "icon_MoveSpeedUp");
            Add(Enum_ConditionType.AttackSpeedUp, ConditionCategory.Buff, ConditionStackPolicy.MultipleInstances, "icon_AttackSpeedUp");

            Add(Enum_ConditionType.ComboBuff_AttackSpeedUp, ConditionCategory.Buff, ConditionStackPolicy.Refresh, "icon_ComboBuff_1");
            Add(Enum_ConditionType.ComboBuff_AttackUp, ConditionCategory.Buff, ConditionStackPolicy.Refresh, "icon_ComboBuff_2");
            Add(Enum_ConditionType.ComboBuff_CriticalChangeUp, ConditionCategory.Buff, ConditionStackPolicy.Refresh, "icon_ComboBuff_3");

            _table.Clear();

            for (int i = 0; i < _conditionMetas.Count; ++i)
            {
                ConditionMeta conditionMeta = _conditionMetas[i];
                if (_table.ContainsKey(conditionMeta.Type) == false)
                    _table.Add(conditionMeta.Type, conditionMeta);
            }
        }
        //------------------------------------------------------------------------------------
        private void Add(Enum_ConditionType type, ConditionCategory category, ConditionStackPolicy conditionStackPolicy,
            string iconKey)
        {
            _conditionMetas.Add(new ConditionMeta
            {
                Type = type,
                Category = category,
                StackPolicy = conditionStackPolicy,
                IconKey = iconKey
            });
        }
        //------------------------------------------------------------------------------------
        public ConditionMeta GetMeta(Enum_ConditionType type)
            => _conditionMetas.Find(x => x.Type == type);
        //------------------------------------------------------------------------------------
        public ConditionData GetData(int index)
            => conditionDatas.Find(x => x.Index == index);
        //------------------------------------------------------------------------------------
        public string GetConditionDataDesc(int index, int level = 0)
        {
            ConditionData conditionData = GetData(index);

            string desc = string.Empty;

            float finalParam1 = conditionData.Param1 + (conditionData.Param1_Level * level);
            float finalParam2 = conditionData.Param2 + (conditionData.Param2_Level * level);
            float finalDuration = conditionData.Duration;

            switch (conditionData.Type)
            {
                case Enum_ConditionType.Invincible:
                case Enum_ConditionType.Stun:
                case Enum_ConditionType.Snare:
                    {
                        desc = string.Format("{0}({1:0.##}s)", conditionData.Type, conditionData.Duration);
                        break;
                    }
                case Enum_ConditionType.Slow:
                    {
                        if (finalParam1 > 0 && finalParam2 > 0)
                            desc = string.Format("{0}({1:0.##}s), AttackDown : {2:0.##}%, MoveDown : {3:0.##}%", conditionData.Type, conditionData.Duration, finalParam1 * 100, finalParam2 * 100);
                        else if (finalParam1 > 0)
                            desc = string.Format("{0}({1:0.##}s), AttackDown : {2:0.##}%", conditionData.Type, conditionData.Duration, finalParam1 * 100);
                        else if (finalParam2 > 0)
                            desc = string.Format("{0}({1:0.##}s), MoveDown : {2:0.##}%", conditionData.Type, conditionData.Duration, finalParam2 * 100);
                        break;
                    }
                case Enum_ConditionType.Knockback:
                case Enum_ConditionType.Fling:
                    {
                        desc = string.Format("{0}", conditionData.Type);
                        break;
                    }
                case Enum_ConditionType.AttackUp:
                case Enum_ConditionType.HpUp:
                case Enum_ConditionType.DefenseUp:
                case Enum_ConditionType.MoveSpeedUp:
                case Enum_ConditionType.AttackSpeedUp:
                case Enum_ConditionType.ComboBuff_AttackSpeedUp:
                case Enum_ConditionType.ComboBuff_AttackUp:
                case Enum_ConditionType.ComboBuff_CriticalChangeUp:
                    {
                        desc = string.Format("{0:0.##}%{1}({2:0.##}s)", finalParam1 * 100, conditionData.Type, conditionData.Duration);
                        break;
                    }
            }

            return desc;
        }
        //------------------------------------------------------------------------------------
    }
}