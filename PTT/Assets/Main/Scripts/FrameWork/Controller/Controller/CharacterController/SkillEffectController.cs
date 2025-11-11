using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Managers;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class SkillEffectController : MonoBehaviour
    {
        private CharacterControllerBase _characterControllerBase;

        [SerializeField]
        private CCState_Painter _cCState_Painter;

        private Dictionary<V2Enum_SkillEffectType, CCStateData> _applyCCList = new Dictionary<V2Enum_SkillEffectType, CCStateData>();

        private LinkedList<EffectBuffData> _effectBuffDatas = new LinkedList<EffectBuffData>();

        private Coroutine _knockBackDirectionCoroutine = null;

        private Coroutine _flingDirectionCoroutine = null;

        private float _dustTimming1 = 0.1f;

        private float _dustTimming2 = 0.9f;

        private string _particleBundleTag = "fx_resources";
        private string _particle_Dust = "FX_CharacterDust";
        private string _particle_Dust2 = "FX_CharacterDust2";

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            //if (m_checkCCStateCoroutine != null)
            //    StopCoroutine(m_checkCCStateCoroutine);

            //m_checkCCStateCoroutine = StartCoroutine(CheckCCState());

            CheckCCState().Forget();
        }
        //------------------------------------------------------------------------------------
        public void SetCharacterControllerBase(CharacterControllerBase characterControllerBase)
        {
            _characterControllerBase = characterControllerBase;
        }
        //------------------------------------------------------------------------------------
        private void OnDisable()
        {
            //if (m_checkCCStateCoroutine != null)
            //    StopCoroutine(m_checkCCStateCoroutine);

            if (_knockBackDirectionCoroutine != null)
                StopCoroutine(_knockBackDirectionCoroutine);

            if (_flingDirectionCoroutine != null)
                StopCoroutine(_flingDirectionCoroutine);

            ReleaseAllCC();
        }
        //------------------------------------------------------------------------------------
        public void SetPainterRotation(float selectRatote)
        {
            if (_cCState_Painter != null)
            {
                Vector3 rotate = _cCState_Painter.transform.eulerAngles;
                rotate.y = selectRatote;

                _cCState_Painter.transform.localEulerAngles = rotate;
            }
        }
        //------------------------------------------------------------------------------------
        public void PlayApplyCC(List<V2CCData> v2CCDatas)
        {
            if (v2CCDatas != null)
            {
                for (int i = 0; i < v2CCDatas.Count; ++i)
                {
                    PlayApplyCC(v2CCDatas[i]);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public void PlayApplyCC(V2CCData damageData)
        {
            if (damageData.CCTypeEnum == V2Enum_SkillEffectType.None)
                return;

            if (IsAppliedCC(V2Enum_SkillEffectType.Invincible) == true)
            {
                if (damageData.CCTypeEnum == V2Enum_SkillEffectType.Invincible
                    || damageData.CCTypeEnum == V2Enum_SkillEffectType.Cleansing
                    || damageData.CCTypeEnum == V2Enum_SkillEffectType.HOT
                    || damageData.CCTypeEnum == V2Enum_SkillEffectType.DotHeal)
                {

                }
                else
                    return;
            }

            if (damageData.CCTypeEnum == V2Enum_SkillEffectType.Cleansing)
            {
                PlayCleansing();
                return;
            }
            else if (damageData.CCTypeEnum == V2Enum_SkillEffectType.Knockback)
            {
                if (_knockBackDirectionCoroutine != null)
                    StopCoroutine(_knockBackDirectionCoroutine);

                _knockBackDirectionCoroutine = StartCoroutine(KnockBackDirection(damageData));
            }
            else if (damageData.CCTypeEnum == V2Enum_SkillEffectType.Fling)
            {
                if (_flingDirectionCoroutine != null)
                    StopCoroutine(_flingDirectionCoroutine);

                _flingDirectionCoroutine = StartCoroutine(FlingDirection(damageData));
            }
            else if (damageData.CCTypeEnum == V2Enum_SkillEffectType.IncreaseAtt
                || damageData.CCTypeEnum == V2Enum_SkillEffectType.IncreaseArmor
                || damageData.CCTypeEnum == V2Enum_SkillEffectType.IncreaseMoveSpeed
                || damageData.CCTypeEnum == V2Enum_SkillEffectType.SkillCooltimeReduce
                || damageData.CCTypeEnum == V2Enum_SkillEffectType.IncreaseHp)
            {
                EffectBuffData effectBuffData = Managers.SkillManager.Instance.GetEffectBuffData();

                effectBuffData.CCTypeEnum = damageData.CCTypeEnum;
                effectBuffData.Enum_Stat = ARRRStatOperator.ConvertCrowdControlTypeToStat(damageData.CCTypeEnum);
                effectBuffData.StatValue = damageData.CCValue;
                effectBuffData.BuffEndTime = Time.time + damageData.CCTime;
                effectBuffData.IsDebuff = false;

                _characterControllerBase.InCreaseBuffStat(effectBuffData.Enum_Stat, effectBuffData.StatValue);
                _effectBuffDatas.AddLast(effectBuffData);
                return;
            }
            else if (damageData.CCTypeEnum == V2Enum_SkillEffectType.DecreaseAtt
                || damageData.CCTypeEnum == V2Enum_SkillEffectType.DecreaseArmor)
            {
                EffectBuffData effectBuffData = Managers.SkillManager.Instance.GetEffectBuffData();

                effectBuffData.CCTypeEnum = damageData.CCTypeEnum;
                effectBuffData.Enum_Stat = ARRRStatOperator.ConvertCrowdControlTypeToStat(damageData.CCTypeEnum);
                effectBuffData.StatValue = damageData.CCValue;
                effectBuffData.BuffEndTime = Time.time + damageData.CCTime;
                effectBuffData.IsDebuff = true;

                _characterControllerBase.DeCreaseBuffStat(effectBuffData.Enum_Stat, effectBuffData.StatValue);
                _effectBuffDatas.AddLast(effectBuffData);
                return;
            }
            else if (damageData.CCTypeEnum == V2Enum_SkillEffectType.DamageReduce)
            {
                EffectBuffData effectBuffData = Managers.SkillManager.Instance.GetEffectBuffData();

                effectBuffData.CCTypeEnum = damageData.CCTypeEnum;
                effectBuffData.Enum_Stat = V2Enum_Stat.Max;
                effectBuffData.StatValue = damageData.CCValue;
                effectBuffData.BuffEndTime = Time.time + damageData.CCTime;

                _characterControllerBase.IncreaseDamageReduce(effectBuffData.StatValue);
                _effectBuffDatas.AddLast(effectBuffData);
                return;
            }
            else if (damageData.CCTypeEnum == V2Enum_SkillEffectType.AdditionalDmg)
            {
                return;
            }

            _cCState_Painter.PlayCCPaint(damageData.CCTypeEnum);

            CCStateData cCStateData = null;

            if (_applyCCList.ContainsKey(damageData.CCTypeEnum) == true)
            {
                cCStateData = _applyCCList[damageData.CCTypeEnum];
            }
            else
            {
                cCStateData = SkillManager.Instance.GetCCStateData();
                _applyCCList.Add(damageData.CCTypeEnum, cCStateData);
            }

            cCStateData.CCTypeEnum = damageData.CCTypeEnum;
            cCStateData.CCStartTime = Time.time;
            cCStateData.CCTime = damageData.CCTime;
            cCStateData.CCValue = damageData.CCValue;
            cCStateData.AttackerPos = damageData.AttackerPos;
            cCStateData.LastApplyTime = 0.0f;
        }
        //------------------------------------------------------------------------------------
        public void ReleaseAllCC()
        {
            foreach (KeyValuePair<V2Enum_SkillEffectType, CCStateData> pair in _applyCCList)
            {
                SkillManager.Instance.PoolCCStateData(pair.Value);

                _characterControllerBase.EndCC(pair.Key);
            }

            _cCState_Painter.ReleaseCC();

            _applyCCList.Clear();
        }
        //------------------------------------------------------------------------------------
        public void PlayCleansing()
        {
            foreach (KeyValuePair<V2Enum_SkillEffectType, CCStateData> pair in _applyCCList)
            {
                if (pair.Key == V2Enum_SkillEffectType.Invincible
                    || pair.Key == V2Enum_SkillEffectType.HOT
                    || pair.Key == V2Enum_SkillEffectType.DotHeal)
                    continue;

                SkillManager.Instance.PoolCCStateData(pair.Value);

                _characterControllerBase.EndCC(pair.Key);
                _cCState_Painter.StopCCPaint(pair.Key);
            }

            if (_knockBackDirectionCoroutine != null)
                StopCoroutine(_knockBackDirectionCoroutine);

            if (_flingDirectionCoroutine != null)
                StopCoroutine(_flingDirectionCoroutine);
        }
        //------------------------------------------------------------------------------------
        private async UniTask CheckCCState()
        {
            while (true)
            {
                //m_removeCCType.Clear();

                try
                {
                    //foreach (KeyValuePair<V2Enum_CrowdControlType, CCStateData> pair in m_applyCCList)
                    foreach (var key in _applyCCList.Keys.ToList())
                    {
                        if (_applyCCList.ContainsKey(key) == false)
                            continue;

                        CCStateData cCStateData = _applyCCList[key];
                        if (cCStateData.CCStartTime + cCStateData.CCTime < Time.time)
                        {
                            SkillManager.Instance.PoolCCStateData(cCStateData);

                            _applyCCList.Remove(key);

                            _characterControllerBase.EndCC(key);
                            _cCState_Painter.StopCCPaint(key);
                        }
                        else
                        {
                            if (cCStateData.CCTypeEnum == V2Enum_SkillEffectType.HOT
                                || cCStateData.CCTypeEnum == V2Enum_SkillEffectType.DOT
                                || cCStateData.CCTypeEnum == V2Enum_SkillEffectType.DotHeal
                                || cCStateData.CCTypeEnum == V2Enum_SkillEffectType.BurnDOT)
                            {
                                if (cCStateData.LastApplyTime + 0.5f <= Time.time)
                                {
                                    if (cCStateData.CCTypeEnum == V2Enum_SkillEffectType.HOT)
                                        _characterControllerBase.CCHOT(cCStateData.CCValue);
                                    else if (cCStateData.CCTypeEnum == V2Enum_SkillEffectType.DOT)
                                        _characterControllerBase.CCDOT(cCStateData.CCValue);
                                    else if (cCStateData.CCTypeEnum == V2Enum_SkillEffectType.BurnDOT)
                                        _characterControllerBase.CCBurn(cCStateData.CCValue);
                                    else if (cCStateData.CCTypeEnum == V2Enum_SkillEffectType.DotHeal)
                                        _characterControllerBase.HealPerAttack(cCStateData.CCValue);

                                    cCStateData.LastApplyTime = Time.time;
                                }
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex.ToString());
                }

                var buffNode = _effectBuffDatas.First;

                while (buffNode != null)
                {
                    EffectBuffData effectBuffData = buffNode.Value;
                    if (effectBuffData.BuffEndTime < Time.time)
                    {
                        if (effectBuffData.CCTypeEnum == V2Enum_SkillEffectType.DamageReduce)
                        {
                            _characterControllerBase.DecreaseDamageReduce(effectBuffData.StatValue);
                        }
                        else
                        {
                            if (effectBuffData.IsDebuff == false)
                                _characterControllerBase.DeCreaseBuffStat(effectBuffData.Enum_Stat, effectBuffData.StatValue);
                            else
                                _characterControllerBase.InCreaseBuffStat(effectBuffData.Enum_Stat, effectBuffData.StatValue);
                        }

                        effectBuffData.CCTypeEnum = V2Enum_SkillEffectType.Max;
                        var curr = buffNode;
                        buffNode = curr.Next;
                        Managers.SkillManager.Instance.PoolEffectBuffData(effectBuffData);
                        _effectBuffDatas.Remove(curr);
                    }
                    else
                        buffNode = buffNode.Next;
                }


                //for (int i = 0; i < m_removeCCType.Count; ++i)
                //{
                //    CharacterSkillManager.Instance.PoolCCStateData(m_applyCCList[m_removeCCType[i]]);

                //    m_applyCCList.Remove(m_removeCCType[i]);

                //    m_characterControllerBase.EndCC(m_removeCCType[i]);
                //    m_cCState_Painter.StopCCPaint(m_removeCCType[i]);
                //}

                //yield return m_waitForSeconds;

                await UniTask.Delay(100);
            }
        }
        //------------------------------------------------------------------------------------
        private IEnumerator KnockBackDirection(V2CCData cCApplyData)
        {
            float knockBackDuration = cCApplyData.CCTime;
            float knockBackGoalPos = cCApplyData.AttackerPos.x;

            Vector2 direction = cCApplyData.AttackerPos - transform.position;
            direction.Normalize();

            float knockBackStartTime = Time.time;
            float knockBackStartPos = transform.position.x;

            knockBackGoalPos = knockBackStartPos + (direction.x > 0 ? (float)cCApplyData.CCValue : (float)(cCApplyData.CCValue * -1.0f));

            bool playdustParticle = false;
            bool playdustParticle2 = false;
            ParticlePoolElement dustParticlePoolElement = null;
            ParticlePoolElement dustParticlePoolElement2 = null;
            while (Time.time < knockBackStartTime + knockBackDuration && knockBackDuration > 0.0f)
            {
                float posgab = knockBackStartPos - knockBackGoalPos;

                float ratio = (Time.time - knockBackStartTime) / knockBackDuration;
                float targetpos = (MathDatas.Sin(90.0f * ratio) * posgab) + knockBackStartPos;

                Vector3 pos = transform.position;
                pos.x = targetpos;
                transform.position = pos;

                if (playdustParticle == false && ratio > _dustTimming1)
                {
                    dustParticlePoolElement = PlayDustParticle(_particle_Dust, true);
                    playdustParticle = true;
                }

                if (playdustParticle2 == false && ratio > _dustTimming2)
                {
                    if (dustParticlePoolElement != null)
                        dustParticlePoolElement.StopParticle();

                    dustParticlePoolElement2 = PlayDustParticle(_particle_Dust2, false);
                    playdustParticle2 = true;
                }

                yield return null;
            }

            if (dustParticlePoolElement != null)
                dustParticlePoolElement.StopParticle();

            //if (dustParticlePoolElement2 != null)
            //    dustParticlePoolElement2.StopParticle();
        }
        //------------------------------------------------------------------------------------
        private ParticlePoolElement PlayDustParticle(string particleName, bool setparent)
        {
            //if (m_characterControllerBase.MyActorType == ActorType.Knight)
            //{
            //    ParticlePoolElement particlePoolElement = ParticleManager.Instance.GetParticle(m_particleBundleTag, particleName);
            //    if (setparent == true)
            //    {
            //        particlePoolElement.transform.SetParent(m_characterControllerBase.transform);
            //        particlePoolElement.transform.ResetWorld();
            //        particlePoolElement.transform.ResetLocal();
            //    }
            //    else if (setparent == false)
            //    {
            //        particlePoolElement.transform.SetParent(null);
            //        particlePoolElement.transform.position = m_characterControllerBase.transform.position;

            //        Vector3 rotate = Vector3.zero;
            //        rotate.y = m_characterControllerBase.LookDirection == Enum_LookDirection.Left ? 180.0f : 0.0f;
            //        particlePoolElement.transform.localEulerAngles = rotate;
            //    }

            //    particlePoolElement.gameObject.SetActive(true);
            //    particlePoolElement.PlayParticle();

            //    return particlePoolElement;
            //}

            return null;
        }
        //------------------------------------------------------------------------------------
        private IEnumerator FlingDirection(V2CCData cCApplyData)
        {
            float flingPosX = transform.position.x - cCApplyData.AttackerPos.x;

            flingPosX *= 1.0f - (float)(cCApplyData.CCValue);


            float flingDuration = cCApplyData.CCTime;

            float flingGoalPos = transform.position.x - flingPosX;

            float flingStartTime = Time.time;

            float flingStartPos = transform.position.x;

            float posgab = flingStartPos - flingGoalPos;

            while (Time.time < flingStartTime + flingDuration && flingDuration > 0.0f)
            {
                float ratio = (Time.time - flingStartTime) / flingDuration;
                float targetpos = flingStartPos - (MathDatas.Sin(90.0f * ratio) * posgab);

                Vector3 pos = transform.transform.position;
                pos.x = targetpos;
                transform.transform.position = pos;

                yield return null;
            }
        }
        //------------------------------------------------------------------------------------
        public bool IsAppliedCC(V2Enum_SkillEffectType cCType)
        {
            return _applyCCList.ContainsKey(cCType);
        }
        //------------------------------------------------------------------------------------
        public double GetCCValue(V2Enum_SkillEffectType cCType)
        {
            if (_applyCCList.ContainsKey(cCType) == true)
            {
                return _applyCCList[cCType].CCValue;
            }

            return 0.0;
        }
        //------------------------------------------------------------------------------------
    }
}