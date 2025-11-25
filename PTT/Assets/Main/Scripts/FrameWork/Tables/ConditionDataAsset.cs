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
        public float DefaultDuration;

        public string IconKey;       // UI에서 쓸 키 (Addressable, SpriteAtlas용)
    }

    [System.Serializable]
    public class ConditionData
    {
        public int Index;
        public Enum_ConditionType Type;
        public float Param1;
        public float Param2;
        public float Duration;
        public float Rate = 1.0f;
        [HideInInspector]
        public Vector2 EffectPos;
    }

    [CreateAssetMenu(fileName = "ConditionData", menuName = "Table/ConditionData", order = 1)]
    public class ConditionDataAsset : ScriptableObject
    {
        public List<ConditionMeta> conditionMetas = new List<ConditionMeta>();

        private Dictionary<Enum_ConditionType, ConditionMeta> _table = new Dictionary<Enum_ConditionType, ConditionMeta>();

        public List<ConditionData> conditionDatas = new List<ConditionData>();

        //------------------------------------------------------------------------------------
        void OnEnable()
        {
            if (conditionMetas.Count <= 0)
            {
                Add(Enum_ConditionType.Invincible, ConditionCategory.Utility, ConditionStackPolicy.RefreshDuration, 1.5f, "icon_invincible");

                Add(Enum_ConditionType.Stun, ConditionCategory.CrowdControl, ConditionStackPolicy.RefreshDuration, 2f, "icon_stun");
                Add(Enum_ConditionType.Snare, ConditionCategory.CrowdControl, ConditionStackPolicy.RefreshDuration, 3f, "icon_snare");
                Add(Enum_ConditionType.Slow, ConditionCategory.CrowdControl, ConditionStackPolicy.RefreshDuration, 4f, "icon_slow");

                Add(Enum_ConditionType.Knockback, ConditionCategory.CrowdControl, ConditionStackPolicy.MergyValue, 0.1f, "icon_knockback");

                Add(Enum_ConditionType.AttackUp, ConditionCategory.Buff, ConditionStackPolicy.MultipleInstances, 1f, "icon_AttackUp");
                Add(Enum_ConditionType.DefenseUp, ConditionCategory.Buff, ConditionStackPolicy.MultipleInstances, 1f, "icon_DefenseUp");
                Add(Enum_ConditionType.MoveSpeedUp, ConditionCategory.Buff, ConditionStackPolicy.MultipleInstances, 1f, "icon_MoveSpeedUp");
                Add(Enum_ConditionType.AttackSpeedUp, ConditionCategory.Buff, ConditionStackPolicy.MultipleInstances, 1f, "icon_AttackSpeedUp");

            }

            _table.Clear();

            for (int i = 0; i < conditionMetas.Count; ++i)
            {
                ConditionMeta conditionMeta = conditionMetas[i];
                if (_table.ContainsKey(conditionMeta.Type) == false)
                    _table.Add(conditionMeta.Type, conditionMeta);
            }
        }
        //------------------------------------------------------------------------------------
        private void Add(Enum_ConditionType type, ConditionCategory category, ConditionStackPolicy conditionStackPolicy,
            float duration, string iconKey)
        {
            conditionMetas.Add(new ConditionMeta
            {
                Type = type,
                Category = category,
                StackPolicy = conditionStackPolicy,
                DefaultDuration = duration,
                IconKey = iconKey
            });
        }
        //------------------------------------------------------------------------------------
        public ConditionMeta GetMeta(Enum_ConditionType type)
            => _table.TryGetValue(type, out var meta) ? meta : null;
        //------------------------------------------------------------------------------------
        public ConditionData GetData(int index)
            => conditionDatas.Find(x => x.Index == index);
        //------------------------------------------------------------------------------------
    }
}