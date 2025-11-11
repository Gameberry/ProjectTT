using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.Managers
{
    public class SkillTriggerManager : MonoSingleton<SkillTriggerManager>
    {
        private Dictionary<int, Collider[]> _recvColliderPools = new Dictionary<int, Collider[]>();

        private List<SkillHitReceiver> _skillHitReceivers = new List<SkillHitReceiver>();

        public void RecvDamageDate(SkillManageInfo skillManageInfo, V2SkillAttackData damage, CharacterControllerBase actortrans, Vector3 damagePos, SkillHitReceiver fixSkillHitReceiver)
        {
            SkillBaseData skilldata = skillManageInfo.SkillBaseData;

            int targetCount = skillManageInfo.GetTargetCount();
            if (targetCount == 0)
                return;

            float range = skillManageInfo.GetAttackRange() * actortrans.GetOutputAttackRange();
            Vector3 pos = damagePos;

            if (skilldata.TargetAttackType == V2Enum_ARR_TargetAttackType.Line)
            {
                if (actortrans.LookDirection == Enum_ARR_LookDirection.Left)
                {
                    pos.x -= skilldata.AttackRange * 0.5f * actortrans.GetOutputAttackRange();
                }
                else
                    pos.x += skilldata.AttackRange * 0.5f * actortrans.GetOutputAttackRange();
            }

            int searchLayer = 0;

            if (skilldata.TargetCheckType == Enum_ARR_TargetCheckType.Friendly)
                searchLayer = LayerMask.NameToLayer(actortrans.IFFType.ToString());
            else
            {
                searchLayer = LayerMask.NameToLayer(SkillManager.Instance.GetEnemyIFFType(actortrans.IFFType).ToString());
            }

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

                SkillHitReceiver skillHitReceiver = colliders[i].gameObject.GetComponent<SkillHitReceiver>();

                _skillHitReceivers.Add(skillHitReceiver);

                if (needAddRecver == true)
                {
                    if (skillHitReceiver == fixSkillHitReceiver)
                        needAddRecver = false;
                }
            }

            SetHitTarget(actortrans, skilldata, ref _skillHitReceivers);

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
                    _skillHitReceivers[i].RecvHitData(damage);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void RecvDamageDate(SkillManageInfo skillManageInfo, V2SkillAttackData damage, CharacterControllerBase actortrans, SkillHitReceiver fixSkillHitReceiver)
        {
            RecvDamageDate(skillManageInfo, damage, actortrans, actortrans.transform.position, fixSkillHitReceiver);
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
        private void SetHitTarget(CharacterControllerBase actortrans, SkillBaseData skillData, ref List<SkillHitReceiver> recvlist)
        {
            if (skillData.TargetCondition == Enum_ARR_TargetConditionType.Range)
            {
                if (skillData.TargetCount > 0 && recvlist.Count > skillData.TargetCount)
                {
                    while (recvlist.Count > skillData.TargetCount)
                    {
                        int deleteIdx = Random.Range(0, recvlist.Count);
                        recvlist.RemoveAt(deleteIdx);
                    }
                }
            }
            else
            {
                if (skillData.TargetCount > 0 && recvlist.Count > skillData.TargetCount)
                {
                    if (skillData.TargetCondition == Enum_ARR_TargetConditionType.HpHigh || skillData.TargetCondition == Enum_ARR_TargetConditionType.HpLow)
                    {
                        if (skillData.TargetCount > 0 && recvlist.Count > skillData.TargetCount)
                        {
                            if (skillData.TargetCondition == Enum_ARR_TargetConditionType.HpHigh)
                            {
                                recvlist.Sort((x, y) =>
                                {
                                    if (x.CharacterController.CurrentHP > y.CharacterController.CurrentHP)
                                        return -1;
                                    else if (x.CharacterController.CurrentHP < y.CharacterController.CurrentHP)
                                        return 1;

                                    return 0;
                                });
                            }
                            else
                            {
                                recvlist.Sort((x, y) =>
                                {
                                    if (x.CharacterController.CurrentHP < y.CharacterController.CurrentHP)
                                        return -1;
                                    else if (x.CharacterController.CurrentHP > y.CharacterController.CurrentHP)
                                        return 1;

                                    return 0;
                                });
                            }
                        }
                    }
                    else if (skillData.TargetCondition == Enum_ARR_TargetConditionType.AtkHigh)
                    {
                        recvlist.Sort((x, y) =>
                        {
                            if (x.CharacterController.MyDamage > y.CharacterController.MyDamage)
                                return -1;
                            else if (x.CharacterController.MyDamage < y.CharacterController.MyDamage)
                                return 1;

                            return 0;
                        });
                    }
                    else if (skillData.TargetCondition == Enum_ARR_TargetConditionType.Far || skillData.TargetCondition == Enum_ARR_TargetConditionType.Near)
                    {
                        if (skillData.TargetCondition == Enum_ARR_TargetConditionType.Far)
                        {
                            recvlist.Sort((x, y) =>
                            {
                                if (MathDatas.GetDistance(actortrans.transform.position, x.CharacterController.transform.position) > MathDatas.GetDistance(actortrans.transform.position, y.CharacterController.transform.position))
                                    return -1;
                                else if (MathDatas.GetDistance(actortrans.transform.position, x.CharacterController.transform.position) < MathDatas.GetDistance(actortrans.transform.position, y.CharacterController.transform.position))
                                    return 1;

                                return 0;
                            });
                        }
                        else if (skillData.TargetCondition == Enum_ARR_TargetConditionType.Near)
                        {
                            recvlist.Sort((x, y) =>
                            {
                                if (MathDatas.GetDistance(actortrans.transform.position, x.CharacterController.transform.position) < MathDatas.GetDistance(actortrans.transform.position, y.CharacterController.transform.position))
                                    return -1;
                                else if (MathDatas.GetDistance(actortrans.transform.position, x.CharacterController.transform.position) > MathDatas.GetDistance(actortrans.transform.position, y.CharacterController.transform.position))
                                    return 1;

                                return 0;
                            });
                        }
                    }

                    int selectidx = skillData.TargetCount;

                    recvlist.RemoveRange(selectidx, recvlist.Count - selectidx);
                }
                
            }
        }
        //------------------------------------------------------------------------------------
    }
}