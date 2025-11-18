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

            // Line 타입일 때 기존 로직 유지
            if (attackData.TargetAttackType == Enum_AttackRangeType.Line)
            {
                if (actortrans.LookDirection == Enum_LookDirection.Left)
                {
                    pos.x -= attackData.HitRange * 0.5f;
                }
                else
                    pos.x += attackData.HitRange * 0.5f;
            }

            // ★ 부채꼴 사용할지 여부
            bool useSector = attackData.TargetAttackType == Enum_AttackRangeType.Sector;
            float sectorAngle = attackData.HitAngle;
            Vector3 sectorForward = damagePos - actortrans.transform.position;
            sectorForward.y = 0f;
            if (sectorForward.sqrMagnitude < 0.0001f)
            {
                sectorForward = (actortrans.LookDirection == Enum_LookDirection.Left)
                    ? Vector3.left
                    : Vector3.right;
            }

            // ★ 부채꼴 기준점 (발/무기 위치로 쓰고 싶으면 여기서 지정)
            // TODO: 실제 발/무기 Transform으로 교체해서 쓰면 됨
            Vector3 sectorOrigin = pos;
            if (useSector)
            {
                sectorOrigin = actortrans.transform.position;
            }

            int searchLayer = 0;
            searchLayer = LayerMask.NameToLayer(Util.GetEnemyIFFType(actortrans.IFFType).ToString());
            searchLayer = 1 << searchLayer;

            Collider[] colliders = null;
            int colliderCount = 0;

            // ★ Overlap 중심도 부채꼴이면 sectorOrigin 기준
            Vector3 overlapCenter = useSector ? sectorOrigin : pos;

            if (targetCount < 0)
            {
                colliders = Physics.OverlapSphere(overlapCenter, range, searchLayer);
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

                colliderCount = Physics.OverlapSphereNonAlloc(overlapCenter, range, colliders, searchLayer);
            }

            if (actortrans.IFFType == IFFType.IFF_Friend)
            {
                debugRadius = range;
                debugPos = useSector ? sectorOrigin : overlapCenter;
                debugAngle = useSector ? sectorAngle : 0f;
                debugForward = useSector ? sectorForward : actortrans.transform.forward;

                if (attackData.TargetAttackType == Enum_AttackRangeType.Line)
                {
                    // 라인 범위 크기(예시는 X 방향 range, Z는 적당히 너비 값)
                    //float width = attackData.HitWidth > 0 ? attackData.HitWidth : range * 0.3f; // HitWidth 필드 있으면 사용
                    debugLineSize = new Vector3(range, 0.1f, range);
                    debugRangeType = DebugRangeType.Line;
                }
                else if (useSector)
                {
                    debugRangeType = DebugRangeType.Sector;
                }
                else
                {
                    debugRangeType = DebugRangeType.Circle;
                }
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
                if (skillHitReceiver == null)
                    continue;

                // ★ 부채꼴이면 XZ 평면 기준 각도 체크
                if (useSector)
                {
                    Vector3 targetPos = skillHitReceiver.transform.position;
                    if (!IsInSectorXZ(sectorOrigin, sectorForward, range, sectorAngle, targetPos))
                        continue;
                }

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
                if (_skillHitReceivers[i] != null)   // <- 여기 null 체크를 리스트 요소로
                {
                    _skillHitReceivers[i].OnDamage(attackData);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public enum DebugRangeType
        {
            None,
            Circle,
            Line,
            Sector
        }

        public DebugRangeType debugRangeType;
        public Vector3 debugPos;
        public float debugRadius;
        public float debugAngle;
        public Vector3 debugForward;
        public Vector3 debugLineSize; // 라인(직사각형) 표현용

        // 기존 필드 재활용 (원형도 같이 찍고 싶으면)
        public Vector3 drawGizmoPos;
        public float drawGizmoRadius;
        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            switch (debugRangeType)
                {
                    case DebugRangeType.Circle:
                        Gizmos.color = Color.blue;
                        Gizmos.DrawWireSphere(debugPos, debugRadius);
                        break;

                    case DebugRangeType.Line:
                        Gizmos.color = Color.green;
                        Gizmos.DrawWireCube(debugPos, debugLineSize);
                        break;

                    case DebugRangeType.Sector:
                        Gizmos.color = Color.red;
                        DrawSectorGizmoXZ(debugPos, debugForward, debugRadius, debugAngle);
                        break;

                    case DebugRangeType.None:
                    default:
                        break;
                }
#endif
        }

        private void DrawSectorGizmoXZ(Vector3 origin, Vector3 forward, float radius, float angle)
        {
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.forward;

            forward.Normalize();

            int segments = 20;
            float halfAngle = angle * 0.5f;
            float step = angle / segments;

            Quaternion baseRot = Quaternion.LookRotation(forward, Vector3.up);

            // 왼쪽 끝 방향
            Vector3 prevDir = baseRot * Quaternion.AngleAxis(-halfAngle, Vector3.up) * Vector3.forward;
            Vector3 prevPoint = origin + prevDir * radius;

            // 부채꼴 테두리
            for (int i = 1; i <= segments; i++)
            {
                float currentAngle = -halfAngle + step * i;
                Vector3 dir = baseRot * Quaternion.AngleAxis(currentAngle, Vector3.up) * Vector3.forward;
                Vector3 point = origin + dir * radius;

                Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;
            }

            // 중심에서 양 끝으로 가는 선 2개
            Vector3 leftDir = baseRot * Quaternion.AngleAxis(-halfAngle, Vector3.up) * Vector3.forward;
            Vector3 rightDir = baseRot * Quaternion.AngleAxis(halfAngle, Vector3.up) * Vector3.forward;
            Gizmos.DrawLine(origin, origin + leftDir * radius);
            Gizmos.DrawLine(origin, origin + rightDir * radius);
        }
        //------------------------------------------------------------------------------------
        private bool IsInSectorXZ(Vector3 origin, Vector3 forward, float radius, float angle, Vector3 targetPos)
        {
            // XZ 평면에서만 거리/각도 계산
            origin.y = 0f;
            targetPos.y = 0f;
            forward.y = 0f;

            Vector3 toTarget = targetPos - origin;
            float distSqr = toTarget.sqrMagnitude;

            if (distSqr > radius * radius || distSqr <= Mathf.Epsilon)
                return false;

            Vector3 dir = toTarget.normalized;
            Vector3 fwd = forward.sqrMagnitude < 0.0001f ? Vector3.forward : forward.normalized;

            float halfRad = angle * 0.5f * Mathf.Deg2Rad;
            float cosHalf = Mathf.Cos(halfRad);

            float dot = Vector3.Dot(fwd, dir);

            return dot >= cosHalf;
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