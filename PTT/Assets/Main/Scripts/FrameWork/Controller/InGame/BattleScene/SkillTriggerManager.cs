using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.Managers
{
    public class SkillTriggerManager : MonoSingleton<SkillTriggerManager>
    {
        private Dictionary<int, Collider[]> _recvColliderPools = new Dictionary<int, Collider[]>();

        private List<CharacterControllerBase> _skillHitReceivers = new List<CharacterControllerBase>();

        // 광역기에서 한 번에 맞을 수 있는 최대 타겟 수 (성능/연출상 제한)
        private const int MaxAoeTargets = 100;

        public bool FixDirec = true;

        public void EffectDamage(AttackStruct attackStruct, CharacterControllerBase actortrans, Vector3 attackPos, CharacterControllerBase fixSkillHitReceiver)
        {
            Chart.SkillInfo attackData = attackStruct.SkillInfo;
            if (attackData == null)
                return;

            int targetCount = attackData.TargetCount;
            if (targetCount == 0)
                return;

            float range = attackData.HitRange;
            Vector3 pos = attackPos; // 3D 좌표 기준 (기본 XZ 평면)

            // Line 타입 위치 보정 (X축 기준, 필요 없으면 삭제해도 됨)
            if (attackData.AttackRangeType == Enum_AttackRangeType.Line)
            {
                float offset = attackData.HitRange * 0.5f;
                if (actortrans.LookDirection == Enum_LookDirection.Left)
                    pos.x -= offset;
                else
                    pos.x += offset;
            }

            // 부채꼴 쓸지 여부
            bool useSector = attackData.AttackRangeType == Enum_AttackRangeType.Sector;
            float sectorAngle = attackData.AttackAngle;

            // 부채꼴 기준점: 발/무기 피벗 우선, 없으면 캐릭터 위치
            Vector3 sectorOrigin = pos;
            Vector3 sectorForward = attackPos - actortrans.transform.position;

            if (FixDirec == true)
            {
                sectorForward = actortrans.LookDirection == Enum_LookDirection.Left ? Vector3.left : Vector3.right;
            }
            else
            {
                if (fixSkillHitReceiver != null)
                    sectorForward = fixSkillHitReceiver.transform.position - attackPos;
                else
                    sectorForward = attackPos - actortrans.transform.position;
            }

            // Y축은 무시하고(XZ 평면) 판정하도록 평면화
            sectorForward.y = 0f;

            // 레이어 마스크
            int searchLayer = LayerMask.NameToLayer(Util.GetEnemyIFFType(actortrans.IFFType).ToString());
            LayerMask layerMask = 1 << searchLayer;
            // 3D Physics: use LayerMask directly (triggers are included if Physics.queriesHitTriggers is true)
            Collider[] colliders;
            int colliderCount;

            Vector3 overlapCenter = useSector ? sectorOrigin : pos;

            if (targetCount < 0)
            {
                // 광역 스킬: 상한(MaxAoeTargets) 두고 OverlapCircle(ContactFilter2D) 사용
                int bufferSize = MaxAoeTargets;
                if (!_recvColliderPools.TryGetValue(bufferSize, out colliders))
                {
                    colliders = new Collider[bufferSize];
                    _recvColliderPools.Add(bufferSize, colliders);
                }

                colliderCount = Physics.OverlapSphereNonAlloc(overlapCenter, range, colliders, layerMask);
            }
            else
            {
                // 타겟 제한 있음 → 그 크기만큼만 버퍼 할당
                if (!_recvColliderPools.TryGetValue(targetCount, out colliders))
                {
                    colliders = new Collider[targetCount];
                    _recvColliderPools.Add(targetCount, colliders);
                }

                colliderCount = Physics.OverlapSphereNonAlloc(overlapCenter, range, colliders, layerMask);
            }

            // 디버그용 값 세팅
            if (actortrans.IFFType == IFFType.IFF_Friend)
            {
                debugPos = overlapCenter;
                debugRadius = range;
                debugAngle = useSector ? sectorAngle : 0f;
                debugForward = sectorForward;

                if (attackData.AttackRangeType == Enum_AttackRangeType.Line)
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
                    Vector3 targetPos = skillHitReceiver.transform.position;
                    if (!IsInSector3D(sectorOrigin, sectorForward, range, sectorAngle, targetPos))
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
                    _skillHitReceivers[i].Damage(attackStruct);
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
                    DrawSectorGizmo3D(debugPos, debugForward, debugRadius, debugAngle);
                    break;
            }
#endif
        }

        private void DrawSectorGizmo3D(Vector3 origin, Vector3 forward, float radius, float angle)
        {
            Vector3 fwd = forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.0001f)
                fwd = Vector3.forward;
            fwd.Normalize();

            int segments = 20;
            float halfAngle = angle * 0.5f;
            float step = angle / segments;

            Vector3 prevDir = Quaternion.Euler(0f, -halfAngle, 0f) * fwd;
            Vector3 prevPoint = origin + prevDir * radius;

            for (int i = 1; i <= segments; i++)
            {
                float currentAngle = -halfAngle + step * i;
                Vector3 dir = Quaternion.Euler(0f, currentAngle, 0f) * fwd;
                Vector3 point = origin + dir * radius;

                Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;
            }

            Vector3 leftDir = Quaternion.Euler(0f, -halfAngle, 0f) * fwd;
            Vector3 rightDir = Quaternion.Euler(0f, halfAngle, 0f) * fwd;
            Gizmos.DrawLine(origin, origin + leftDir * radius);
            Gizmos.DrawLine(origin, origin + rightDir * radius);
        }
        //------------------------------------------------------------------------------------
        private bool IsInSector3D(Vector3 origin, Vector3 forward, float radius, float angle, Vector3 targetPos)
        {
            Vector3 toTarget = targetPos - origin;

            // XZ 평면 기준으로 판정 (Y 무시)
            toTarget.y = 0f;

            float distSqr = toTarget.sqrMagnitude;
            if (distSqr > radius * radius || distSqr <= Mathf.Epsilon)
                return false;

            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.forward;

            forward.y = 0f;
            forward.Normalize();
            Vector3 dir = toTarget.normalized;

            float halfRad = angle * 0.5f * Mathf.Deg2Rad;
            float cosHalf = Mathf.Cos(halfRad);

            float dot = Vector3.Dot(forward, dir);

            return dot >= cosHalf;
        }
        //------------------------------------------------------------------------------------
        private void SetHitTarget(CharacterControllerBase actortrans, Chart.SkillInfo skillData, ref List<CharacterControllerBase> recvlist)
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