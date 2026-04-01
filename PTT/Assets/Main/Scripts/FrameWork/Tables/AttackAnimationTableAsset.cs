using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public enum AniDirectionPoint : byte
    { 
        Up = 0,
        Down,
    }

    public enum AttackAniType : byte
    { 
        None = 0,
        Attack,
        Rready,
        AttackTurnReady,
    }

    [System.Serializable]
    public class AttackAniData
    {
        public AttackAniType AniType = AttackAniType.Attack;

        public AniDirectionPoint StartDirection = AniDirectionPoint.Up;
        public AniDirectionPoint EndDirection = AniDirectionPoint.Down;

        public string AnimationID;
    }

    [System.Serializable]
    public class AttackTypeData
    {
        public AttackAniType AniType = AttackAniType.Attack;

        public Dictionary<AniDirectionPoint, List<AttackAniData>> AttackAniData_Dic = new Dictionary<AniDirectionPoint, List<AttackAniData>>();
    }

    [CreateAssetMenu(fileName = "AttackAnimationTable", menuName = "Table/AttackAnimationTable", order = 1)]
    public class AttackAnimationTableAsset : ScriptableObject
    {
        [ArrayElementTitle("AniType")]
        public List<AttackAniData> AttackAniData_List = new List<AttackAniData>();

        public Dictionary<AttackAniType, AttackTypeData> AttackTypeData_Dic = new Dictionary<AttackAniType, AttackTypeData>();
        public Dictionary<string, AttackAniData> AttackAniData_Dic = new Dictionary<string, AttackAniData>();

        //------------------------------------------------------------------------------------
        public void OnEnable()
        {
            AttackTypeData_Dic.Clear();
            AttackAniData_Dic.Clear();

            for (int i = 0; i < AttackAniData_List.Count; ++i)
            {
                AttackAniData data = AttackAniData_List[i];

                if (AttackTypeData_Dic.ContainsKey(data.AniType) == false)
                {
                    AttackTypeData attacktypedata = new AttackTypeData();
                    attacktypedata.AniType = data.AniType;
                    AttackTypeData_Dic.Add(data.AniType, attacktypedata);
                }

                if(AttackTypeData_Dic[data.AniType].AttackAniData_Dic.ContainsKey(data.StartDirection) == false)
                {
                    AttackTypeData_Dic[data.AniType].AttackAniData_Dic.Add(data.StartDirection, new List<AttackAniData>());
                }

                AttackTypeData_Dic[data.AniType].AttackAniData_Dic[data.StartDirection].Add(data);

                AttackAniData_Dic.Add(data.AnimationID, data);
            }
        }
        //------------------------------------------------------------------------------------
        public AttackAniData GetAttackAniData(AttackAniType attackanitype, AniDirectionPoint startdirection)
        {
            AttackTypeData typedata = null;
            AttackTypeData_Dic.TryGetValue(attackanitype, out typedata);
            if (typedata == null)
                return null;

            List<AttackAniData> anidatalist = null;
            typedata.AttackAniData_Dic.TryGetValue(startdirection, out anidatalist);
            if (anidatalist == null)
                return null;

            return anidatalist[Random.Range(0, anidatalist.Count)];
        }
        //------------------------------------------------------------------------------------
        public AttackAniData GetAttackAniData(string animationid)
        {
            AttackAniData data = null;
            AttackAniData_Dic.TryGetValue(animationid, out data);

            return data;
        }
        //------------------------------------------------------------------------------------
        [ContextMenu("SortingSpriteAnimationList")]
        public void SortingSpriteAnimationList()
        {
            AttackAniData_List.Sort((x, y) =>
            {
                return x.AnimationID.CompareTo(y.AnimationID);
            });
        }
        //------------------------------------------------------------------------------------
    }
}