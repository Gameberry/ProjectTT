using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.Managers
{
    public class SkillTriggerManager : MonoSingleton<SkillTriggerManager>
    {
        private Dictionary<int, Collider[]> _recvColliderPools = new Dictionary<int, Collider[]>();

        private List<CharacterControllerBase> _skillHitReceivers = new List<CharacterControllerBase>();

        public void EffectDamage(AttackData attackData, CharacterControllerBase actortrans, Vector3 damagePos, CharacterControllerBase fixSkillHitReceiver)
        {
            int targetCount = attackData.TargetCount;
            if (targetCount == 0)
                return;

            float range = attackData.HitRange;
            Vector3 pos = damagePos;

            if (attackData.TargetAttackType == Enum_AttackRangeType.Line)
            {
                if (actortrans.LookDirection == Enum_LookDirection.Left)
                {
                    pos.x -= attackData.HitRange * 0.5f;
                }
                else
                    pos.x += attackData.HitRange * 0.5f;
            }

            int searchLayer = 0;

            searchLayer = LayerMask.NameToLayer(Util.GetEnemyIFFType(actortrans.IFFType).ToString());


            searchLayer = 1 << searchLayer;

            Collider[] colliders = null;

            int colliderCount = 0;

            if (targetCount < 0)
            {
                colliders = Physics.OverlapSphere(pos, range, searchLayer);

                colliderCount = colliders.Length;
            }
            else
            {
                if (_recvColliderPools.ContainsKey(targetCount) == false)
                {
                    colliders = new Collider[targetCount];
                    _recvColliderPools.Add(targetCount, colliders);
                }
                else
                    colliders = _recvColliderPools[targetCount];

                colliderCount = Physics.OverlapSphereNonAlloc(pos, range, colliders, searchLayer);
            }

            if (actortrans.IFFType == IFFType.IFF_Friend)
            {
                drawGizmoPos = pos;
                drawGizmoRadius = range;
            }

            _skillHitReceivers.Clear();

            bool needAddRecver = true;

            if (fixSkillHitReceiver == null)
            {
                needAddRecver = false;
            }

            for (int i = 0; i < colliderCount; i++)
            {
                if (colliders[i] == null)
                    continue;

                CharacterControllerBase skillHitReceiver = colliders[i].gameObject.GetComponent<CharacterControllerBase>();

                _skillHitReceivers.Add(skillHitReceiver);

                if (needAddRecver == true)
                {
                    if (skillHitReceiver == fixSkillHitReceiver)
                        needAddRecver = false;
                }
            }

            SetHitTarget(actortrans, attackData, ref _skillHitReceivers);

            if (needAddRecver == true)
            {
                if (targetCount < 0)
                    _skillHitReceivers.Add(fixSkillHitReceiver);
                else
                {
                    if (_skillHitReceivers.Count == 0)
                        _skillHitReceivers.Add(fixSkillHitReceiver);
                    else if (_skillHitReceivers.Count < targetCount)
                        _skillHitReceivers.Add(fixSkillHitReceiver);
                    else
                        _skillHitReceivers[_skillHitReceivers.Count - 1] = fixSkillHitReceiver;
                }
            }

            for (int i = 0; i < _skillHitReceivers.Count; ++i)
            {
                if (_skillHitReceivers != null)
                {
                    _skillHitReceivers[i].OnDamage(attackData);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public Vector3 drawGizmoPos;
        public float drawGizmoRadius;
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            //Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
            Gizmos.DrawWireSphere(drawGizmoPos, drawGizmoRadius);
        }
        //------------------------------------------------------------------------------------
        private void SetHitTarget(CharacterControllerBase actortrans, AttackData skillData, ref List<CharacterControllerBase> recvlist)
        {
            {
                if (skillData.TargetCount > 0 && recvlist.Count > skillData.TargetCount)
                {
                    recvlist.Sort((x, y) =>
                    {
                        if (MathDatas.GetDistance(actortrans.transform.position, x.transform.position) < MathDatas.GetDistance(actortrans.transform.position, y.transform.position))
                            return -1;
                        else if (MathDatas.GetDistance(actortrans.transform.position, x.transform.position) > MathDatas.GetDistance(actortrans.transform.position, y.transform.position))
                            return 1;

                        return 0;
                    });

                    int selectidx = skillData.TargetCount;

                    recvlist.RemoveRange(selectidx, recvlist.Count - selectidx);
                }
                
            }
        }
        //------------------------------------------------------------------------------------
    }
}