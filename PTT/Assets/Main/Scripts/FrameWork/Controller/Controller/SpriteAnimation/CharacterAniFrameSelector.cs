using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class CharacterAniFrameSelector
    {
        private AnimationTableAsset m_aniTableAsset = null;
        //private AttackAnimationTableAsset m_attackAniTableAsset = null;

        private CharacterAniController m_charAniController = null;

        private System.Action<AnimationAction> m_aniAction = null;
        
        private CharacterState m_currentAniState = CharacterState.None;

        // 공격 애니메이션 전용 파라미터
        private CharacterState m_prevAniState = CharacterState.None;
        private AttackAniType m_currAttackAniType = AttackAniType.None;
        private AniDirectionPoint m_endDirectionPoint = AniDirectionPoint.Up;
        private AttackAniData m_attackAnidata = null;
        // 공격 애니메이션 전용 파라미터

        // 현재 애니메이션을 돌리기 위한 파라미터
        private SpriteAnimation m_selectAnidata = null;
        private float m_currentFrameturm = 0.0f;

        private float m_currFrameProcessTime = 0.0f;
        private float m_nextframeChangeTime = 0.0f;

        private int m_nextFrame;
        // 현재 애니메이션을 돌리기 위한 파라미터

        //------------------------------------------------------------------------------------
        public void Init(CharacterAniController charanicontroller)
        {
            m_currentAniState = CharacterState.Idle;
            m_aniTableAsset = Managers.TableManager.Instance.GetTableClass<AnimationTableAsset>();
            //m_attackAniTableAsset = Managers.TableManager.Instance.GetTableClass<AttackAnimationTableAsset>();
            m_charAniController = charanicontroller;
        }
        //------------------------------------------------------------------------------------
        public void ConnectAniActionCallBack(System.Action<AnimationAction> action)
        {
            m_aniAction = action;
        }
        //------------------------------------------------------------------------------------
        public void PlayStateAnimation(CharacterState characterState, string aniId = "")
        { // 여기는 현재 애니메이션이 플레이되든 아니든 그냥 호출된다. CharAniFrameSelector는 명령에의해 바로바로 애니가 바껴야한다.
            // 절대 CharAniFrameSelector에 의해 호출되면 안되고 CharAniController에 의해 호출되어야 한다.

            m_currentAniState = characterState;

            if (string.IsNullOrEmpty(aniId) == true)
            {
                //m_selectAnidata = m_aniTableAsset.GetRandomStateAniData(m_charAniController.m_characterControllerBase.GroupIndex, characterState);
            }
            else
            {
                //m_selectAnidata = m_aniTableAsset.GetAniData(m_charAniController.m_characterControllerBase.GroupIndex, aniId);
            }

            m_prevAniState = characterState;

            SetCurrentAniData();

            PlayNextAniFrame();
        }
        //------------------------------------------------------------------------------------
        public void SetDummyCharacterAniFrameSelector(CharacterAniFrameSelector capyCharacterAniFrameSelector)
        {
            if (capyCharacterAniFrameSelector == null)
                return;

            capyCharacterAniFrameSelector.CapyDummyCharacterAniFrameSelector
                (
                ref m_currentAniState,
                ref m_prevAniState,
                ref m_currAttackAniType,
                ref m_endDirectionPoint,
                ref m_attackAnidata,
                ref m_selectAnidata,
                ref m_currentFrameturm,
                ref m_currFrameProcessTime,
                ref m_nextframeChangeTime,
                ref m_nextFrame
                );

            //m_charAniController.SetAnimationActionData(m_selectAnidata.ActionDatas[m_nextFrame]);
        }
        //------------------------------------------------------------------------------------
        public void CapyDummyCharacterAniFrameSelector
            (
            ref CharacterState currentAniState,
            ref CharacterState prevAniState,
            ref AttackAniType currAttackAniType,
            ref AniDirectionPoint endDirectionPoint,
            ref AttackAniData attackAnidata,
            ref SpriteAnimation selectAnidata,
            ref float currentFrameturm,
            ref float currFrameProcessTime,
            ref float nextframeChangeTime,
            ref int nextFrame
            )
        {
            currentAniState = m_currentAniState;
            prevAniState = m_prevAniState;
            currAttackAniType = m_currAttackAniType;
            endDirectionPoint = m_endDirectionPoint;
            attackAnidata = m_attackAnidata;
            selectAnidata = m_selectAnidata;
            currentFrameturm = m_currentFrameturm;
            currFrameProcessTime = m_currFrameProcessTime;
            nextframeChangeTime = m_nextframeChangeTime;
            nextFrame = m_nextFrame;
        }
        //------------------------------------------------------------------------------------
        private void SetAttackAniData()
        {
            //m_attackAnidata = m_attackAniTableAsset.GetAttackAniData(m_currAttackAniType, m_endDirectionPoint);
            
            //if (m_attackAnidata.AniType == AttackAniType.Attack)
            //{
            //    m_endDirectionPoint = m_attackAnidata.EndDirection;
            //}
        }
        //------------------------------------------------------------------------------------
        private void SetCurrentAniData()
        {
            try
            {
                m_currentFrameturm = m_selectAnidata.Duration / m_selectAnidata.ActionDatas.Count;
            }
            catch
            {
            }
            m_nextFrame = 0;
            m_currFrameProcessTime = Time.time;
            m_nextframeChangeTime = Time.time;
        }
        //------------------------------------------------------------------------------------
        public void OnFrameCallBack(AnimationAction aniaction)
        {
            if (aniaction == AnimationAction.None)
                return;

            if (aniaction == AnimationAction.AniEnd)
            {
                if (m_currentAniState == CharacterState.Attack)
                {
                    if (m_currAttackAniType == AttackAniType.Rready || m_currAttackAniType == AttackAniType.AttackTurnReady)
                    {// 준비가 끝났으니 리얼탱 어택 애니메이션으로 돌려준다.
                        m_currAttackAniType = AttackAniType.Attack;

                        SetAttackAniData();

                        //m_selectAnidata = m_aniTableAsset.GetAniData(m_charAniController.m_characterControllerBase.GroupIndex, m_attackAnidata.AnimationID);

                        SetCurrentAniData();
                        return;
                    }
                    else if (m_currAttackAniType == AttackAniType.Attack)
                    {
                        m_currAttackAniType = AttackAniType.None;
                    }
                }
            }

            

            if (m_aniAction != null)
                m_aniAction(aniaction);
        }
        //------------------------------------------------------------------------------------
        public void Updated()
        {
            PlayNextAniFrame();
        }
        //------------------------------------------------------------------------------------
        private void PlayNextAniFrame()
        {
            if (m_selectAnidata == null)
                return;

            //if (m_currAniState == CharacterState.Run)
            //{
            //    m_currFrameProcessTime += Time.deltaTime * m_aniSpeedRatio * (MoveSpeed * MoveSpeedAniRatio);
            //}
            //else
            //{
            //    m_currFrameProcessTime += Time.deltaTime * m_aniSpeedRatio;
            //}

            m_currFrameProcessTime += Time.deltaTime * m_charAniController.m_characterControllerBase.AniControllerSpeed;

            if (m_nextframeChangeTime <= m_currFrameProcessTime)
            {
                if (m_selectAnidata.ActionDatas.Count > m_nextFrame)
                {
                    if(m_nextFrame == 0)
                        OnFrameCallBack(AnimationAction.AniStart);

                    AnimationActionData renderdata = m_selectAnidata.ActionDatas[m_nextFrame];
                    m_charAniController.SetAnimationActionData(renderdata);

                    OnFrameCallBack(renderdata.ActionID);

                    m_nextframeChangeTime = Time.time + m_currentFrameturm;
                    m_currFrameProcessTime = Time.time;

                    m_nextFrame++;
                }
                else
                {
                    

                    // 이젠 어택도 루프를 돌려야해서 수정
                    //if (m_currentAniState == CharacterState.Attack)
                    //{

                    //}
                    //else
                    //{
                    //    if (m_selectAnidata.Loop == true)
                    //    {
                    //        PlayStateAnimation(m_selectAnidata.AnimationGroup);
                    //    }
                    //}

                    if (m_selectAnidata.Loop == true)
                    {
                        PlayStateAnimation(m_selectAnidata.AnimationGroup);
                    }
                    else
                    {
                        OnFrameCallBack(AnimationAction.AniEnd);
                    }
                }
            }
        }
        //------------------------------------------------------------------------------------
    }
}