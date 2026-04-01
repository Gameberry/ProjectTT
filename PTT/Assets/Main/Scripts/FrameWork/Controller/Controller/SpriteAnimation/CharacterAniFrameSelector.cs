using UnityEngine;

namespace GameBerry
{
    public class CharacterAniFrameSelector
    {
        private AnimationTableAsset m_aniTableAsset;
        private AttackAnimationTableAsset m_attackAniTableAsset;
        private CharacterAniController m_charAniController;

        private System.Action<AnimationAction> m_aniAction;

        private CharacterState m_currentAniState = CharacterState.None;
        private SpriteAnimation m_selectAnidata;
        private float m_currentFrameturm;
        private float m_currFrameProcessTime;
        private float m_nextframeChangeTime;
        private int m_nextFrame;
        private string m_currentAnimationId = string.Empty;

        public string CurrentAnimationId => m_currentAnimationId;

        public void Init(CharacterAniController charanicontroller)
        {
            m_currentAniState = CharacterState.Idle;
            m_charAniController = charanicontroller;
            m_aniTableAsset = charanicontroller.AnimationTableAsset;
            m_attackAniTableAsset = charanicontroller.AttackAnimationTableAsset;
        }

        public void ConnectAniActionCallBack(System.Action<AnimationAction> action)
        {
            m_aniAction = action;
        }

        public void PlayStateAnimation(CharacterState characterState, string aniId = "")
        {
            if (m_charAniController == null)
                return;

            m_aniTableAsset = m_charAniController.AnimationTableAsset;
            m_attackAniTableAsset = m_charAniController.AttackAnimationTableAsset;
            m_currentAniState = characterState;

            if (m_aniTableAsset == null || string.IsNullOrEmpty(m_charAniController.AnimationResourceKey))
                return;

            if (string.IsNullOrEmpty(aniId))
            {
                if (characterState == CharacterState.Attack && m_attackAniTableAsset != null)
                {
                    AttackAniData attackAniData = m_attackAniTableAsset.GetAttackAniData(AttackAniType.Attack, AniDirectionPoint.Up);
                    if (attackAniData != null)
                        aniId = attackAniData.AnimationID;
                }

                if (string.IsNullOrEmpty(aniId))
                    m_selectAnidata = m_aniTableAsset.GetRandomStateAniData(m_charAniController.AnimationResourceKey, characterState);
                else
                    m_selectAnidata = m_aniTableAsset.GetAniData(m_charAniController.AnimationResourceKey, aniId);
            }
            else
            {
                m_selectAnidata = m_aniTableAsset.GetAniData(m_charAniController.AnimationResourceKey, aniId);
            }

            if (m_selectAnidata == null)
                return;

            m_currentAnimationId = string.IsNullOrEmpty(m_selectAnidata.AnimationID) ? aniId : m_selectAnidata.AnimationID;
            SetCurrentAniData();
            PlayNextAniFrame();
        }

        public void Updated()
        {
            PlayNextAniFrame();
        }

        private void SetCurrentAniData()
        {
            if (m_selectAnidata == null || m_selectAnidata.ActionDatas == null || m_selectAnidata.ActionDatas.Count <= 0)
            {
                m_currentFrameturm = 0.0f;
                m_nextFrame = 0;
                return;
            }

            m_currentFrameturm = m_selectAnidata.Duration / m_selectAnidata.ActionDatas.Count;
            m_nextFrame = 0;
            m_currFrameProcessTime = Time.time;
            m_nextframeChangeTime = Time.time;
        }

        private void OnFrameCallBack(AnimationAction aniaction)
        {
            if (aniaction == AnimationAction.None)
                return;

            m_aniAction?.Invoke(aniaction);
        }

        private void PlayNextAniFrame()
        {
            if (m_selectAnidata == null || m_selectAnidata.ActionDatas == null)
                return;

            float speed = 1.0f;
            if (m_charAniController != null && m_charAniController.m_characterControllerBase != null)
                speed = m_charAniController.m_characterControllerBase.AniControllerSpeed;

            m_currFrameProcessTime += Time.deltaTime * speed;

            if (m_nextframeChangeTime > m_currFrameProcessTime)
                return;

            if (m_selectAnidata.ActionDatas.Count > m_nextFrame)
            {
                if (m_nextFrame == 0)
                    OnFrameCallBack(AnimationAction.AniStart);

                AnimationActionData renderdata = m_selectAnidata.ActionDatas[m_nextFrame];
                m_charAniController.SetAnimationActionData(renderdata);
                OnFrameCallBack(renderdata.ActionID);

                m_nextframeChangeTime = Time.time + m_currentFrameturm;
                m_currFrameProcessTime = Time.time;
                m_nextFrame++;
                return;
            }

            if (m_selectAnidata.Loop)
                PlayStateAnimation(m_selectAnidata.AnimationGroup, m_selectAnidata.AnimationID);
            else
                OnFrameCallBack(AnimationAction.AniEnd);
        }
    }
}
