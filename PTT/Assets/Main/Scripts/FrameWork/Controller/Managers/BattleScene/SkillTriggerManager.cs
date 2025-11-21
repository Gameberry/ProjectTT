using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.Managers
{
    public class SkillTriggerManager : MonoSingleton<SkillTriggerManager>
    {
        private Dictionary<int, Collider2D[]> _recvColliderPools = new Dictionary<int, Collider2D[]>();

        private List<CharacterControllerBase> _skillHitReceivers = new List<CharacterControllerBase>();

        // 광역기에서 한 번에 맞을 수 있는 최대 타겟 수 (성능/연출상 제한)
        private const int MaxAoeTargets = 100;

        public void EffectDamage(AttackData attackData, CharacterControllerBase actortrans, Vector3 attackPos, CharacterControllerBase fixSkillHitReceiver)
        {
            int targetCount = attackData.TargetCount;
            if (targetCount == 0)
                return;

            float range = attackData.HitRange;
            Vector2 pos = attackPos; // 2D 좌표 기준

            // Line 타입 위치 보정 (X축 기준, 필요 없으면 삭제해도 됨)
            if (attackData.TargetAttackType == Enum_AttackRangeType.Line)
            {
                float offset = attackData.HitRange * 0.5f;
                if (actortrans.LookDirection == Enum_LookDirection.Left)
                    pos.x -= offset;
                else
                    pos.x += offset;
            }

            // 부채꼴 쓸지 여부
            bool useSector = attackData.TargetAttackType == Enum_AttackRangeType.Sector;
            float sectorAngle = attackData.HitAngle;

            // 부채꼴 기준점: 발/무기 피벗 우선, 없으면 캐릭터 위치
            Vector2 sectorOrigin = pos;
            Vector2 sectorForward = attackPos - actortrans.transform.position;

            if (fixSkillHitReceiver != null)
                sectorForward = fixSkillHitReceiver.transform.position - attackPos;
            else
                sectorForward = attackPos - actortrans.transform.position;

            // 레이어 마스크
            int searchLayer = LayerMask.NameToLayer(Util.GetEnemyIFFType(actortrans.IFFType).ToString());
            LayerMask layerMask = 1 << searchLayer;

            ContactFilter2D filter = new ContactFilter2D();
            filter.useLayerMask = true;
            filter.layerMask = layerMask;
            filter.useTriggers = true; // 트리거도 포함할지 여부

            Collider2D[] colliders;
            int colliderCount;

            Vector2 overlapCenter = useSector ? sectorOrigin : pos;

            if (targetCount < 0)
            {
                // 광역 스킬: 상한(MaxAoeTargets) 두고 OverlapCircle(ContactFilter2D) 사용
                int bufferSize = MaxAoeTargets;
                if (!_recvColliderPools.TryGetValue(bufferSize, out colliders))
                {
                    colliders = new Collider2D[bufferSize];
                    _recvColliderPools.Add(bufferSize, colliders);
                }

                colliderCount = Physics2D.OverlapCircle(overlapCenter, range, filter, colliders);
            }
            else
            {
                // 타겟 제한 있음 → 그 크기만큼만 버퍼 할당
                if (!_recvColliderPools.TryGetValue(targetCount, out colliders))
                {
                    colliders = new Collider2D[targetCount];
                    _recvColliderPools.Add(targetCount, colliders);
                }

                colliderCount = Physics2D.OverlapCircle(overlapCenter, range, filter, colliders);
            }

            // 디버그용 값 세팅
            if (actortrans.IFFType == IFFType.IFF_Friend)
            {
                debugPos = overlapCenter;
                debugRadius = range;
                debugAngle = useSector ? sectorAngle : 0f;
                debugForward = new Vector3(sectorForward.x, sectorForward.y, 0f);

                if (attackData.TargetAttackType == Enum_AttackRangeType.Line)
                    debugRangeType = DebugRangeType.Line;
                else if (useSector)
                    debugRangeType = DebugRangeType.Sector;
                else
                    debugRangeType = DebugRangeType.Circle;
            }

            _skillHitReceivers.Clear();

            bool needAddRecver = fixSkillHitReceiver != null;

            for (int i = 0; i < colliderCount; i++)
            {
                if (colliders[i] == null)
                    continue;

                CharacterControllerBase skillHitReceiver = colliders[i].GetComponent<CharacterControllerBase>();
                if (skillHitReceiver == null)
                    continue;

                // 부채꼴이면 각도 체크
                if (useSector)
                {
                    Vector2 targetPos = skillHitReceiver.transform.position;
                    if (!IsInSector2D(sectorOrigin, sectorForward, range, sectorAngle, targetPos))
                        continue;
                }

                _skillHitReceivers.Add(skillHitReceiver);

                if (needAddRecver && skillHitReceiver == fixSkillHitReceiver)
                    needAddRecver = false;
            }

            // 거리 / 보스 우선 정렬 + N명 자르기 (이전 SetHitTarget 그대로 사용)
            SetHitTarget(actortrans, attackData, ref _skillHitReceivers);

            // fixSkillHitReceiver 보정 로직 (원래 코드 유지)
            if (fixSkillHitReceiver != null && needAddRecver)
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
                if (_skillHitReceivers[i] != null)
                    _skillHitReceivers[i].Damage(attackData);
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

                //case DebugRangeType.Line:
                //    Gizmos.color = Color.green;
                //    Gizmos.DrawWireCube(debugPos, new Vector3(attackLineLength, 0.01f, attackLineWidth)); // 필요하면 필드로 빼기
                //    break;

                case DebugRangeType.Sector:
                    Gizmos.color = Color.red;
                    DrawSectorGizmo2D(debugPos, debugForward, debugRadius, debugAngle);
                    break;
            }
#endif
        }

        private void DrawSectorGizmo2D(Vector3 origin, Vector3 forward, float radius, float angle)
        {
            Vector3 fwd = forward;
            if (fwd.sqrMagnitude < 0.0001f)
                fwd = Vector3.right;
            fwd.Normalize();

            int segments = 20;
            float halfAngle = angle * 0.5f;
            float step = angle / segments;

            Quaternion baseRot = Quaternion.LookRotation(Vector3.forward, Vector3.up); // 2D에서는 z-forward
            Vector3 baseDir = new Vector3(fwd.x, fwd.y, 0f);

            Vector3 prevDir = Quaternion.AngleAxis(-halfAngle, Vector3.forward) * baseDir;
            Vector3 prevPoint = origin + prevDir * radius;

            for (int i = 1; i <= segments; i++)
            {
                float currentAngle = -halfAngle + step * i;
                Vector3 dir = Quaternion.AngleAxis(currentAngle, Vector3.forward) * baseDir;
                Vector3 point = origin + dir * radius;

                Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;
            }

            Vector3 leftDir = Quaternion.AngleAxis(-halfAngle, Vector3.forward) * baseDir;
            Vector3 rightDir = Quaternion.AngleAxis(halfAngle, Vector3.forward) * baseDir;
            Gizmos.DrawLine(origin, origin + leftDir * radius);
            Gizmos.DrawLine(origin, origin + rightDir * radius);
        }
        //------------------------------------------------------------------------------------
        private bool IsInSector2D(Vector2 origin, Vector2 forward, float radius, float angle, Vector2 targetPos)
        {
            Vector2 toTarget = targetPos - origin;

            float distSqr = toTarget.sqrMagnitude;
            if (distSqr > radius * radius || distSqr <= Mathf.Epsilon)
                return false;

            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector2.right;

            forward.Normalize();
            Vector2 dir = toTarget.normalized;

            float halfRad = angle * 0.5f * Mathf.Deg2Rad;
            float cosHalf = Mathf.Cos(halfRad);

            float dot = Vector2.Dot(forward, dir);

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