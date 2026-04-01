using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class CharacterAniController : MonoBehaviour
    {
        public CharacterControllerBase m_characterControllerBase;

        [SerializeField] private AnimationTableAsset m_animationTableAsset;
        [SerializeField] private AttackAnimationTableAsset m_attackAnimationTableAsset;
        [SerializeField] private string m_animationResourceKey = string.Empty;
        [SerializeField] private List<SpriteAniPart> m_charAniPart = new List<SpriteAniPart>();

        private readonly Dictionary<AnimationPart, SpriteAniPart> m_charAniPart_Dic = new Dictionary<AnimationPart, SpriteAniPart>();
        private readonly List<AnimationPart> m_changedParts = new List<AnimationPart>();
        private readonly List<ParticlePoolElement> m_playingParticles = new List<ParticlePoolElement>();

        private Transform m_charTransform;
        private CharacterAniFrameSelector m_frameSeleter;
        private Color m_color = Color.white;

        public System.Action<string, string> AnimationEvent;

        public CharacterAniFrameSelector FrameSeleter => m_frameSeleter;
        public Dictionary<AnimationPart, SpriteAniPart> CharAniPart_Dic => m_charAniPart_Dic;
        public AnimationTableAsset AnimationTableAsset => m_animationTableAsset;
        public AttackAnimationTableAsset AttackAnimationTableAsset => m_attackAnimationTableAsset;
        public string AnimationResourceKey => m_animationResourceKey;
        public bool CanPlayAnimation => m_animationTableAsset != null && string.IsNullOrEmpty(m_animationResourceKey) == false && m_charAniPart_Dic.Count > 0;

        private void Awake()
        {
            if (m_characterControllerBase == null)
                m_characterControllerBase = GetComponentInParent<CharacterControllerBase>();

            Init(transform);
        }

        private void OnDisable()
        {
            ClearChangedParts();
            ReleasePlayingParticles();
        }

        private void Update()
        {
            m_frameSeleter?.Updated();
        }

        public void Init(Transform charroottrans)
        {
            m_charTransform = charroottrans;

            CacheAniParts();

            if (m_frameSeleter == null)
            {
                m_frameSeleter = new CharacterAniFrameSelector();
                m_frameSeleter.Init(this);
                m_frameSeleter.ConnectAniActionCallBack(AniActionCallBack);
            }
        }

        public void SetAnimationTable(AnimationTableAsset animationTableAsset)
        {
            m_animationTableAsset = animationTableAsset;
        }

        public void SetAttackAnimationTable(AttackAnimationTableAsset attackAnimationTableAsset)
        {
            m_attackAnimationTableAsset = attackAnimationTableAsset;
        }

        public void SetAnimationResourceKey(string animationResourceKey)
        {
            m_animationResourceKey = animationResourceKey ?? string.Empty;
        }

        public bool TryGetRandomAnimationResourceKey(out string animationResourceKey)
        {
            animationResourceKey = string.Empty;

            if (m_animationTableAsset == null || m_animationTableAsset.SpriteAniGroupData_List == null)
                return false;

            List<SpriteAniGroupData> groupDataList = m_animationTableAsset.SpriteAniGroupData_List;
            if (groupDataList.Count == 0)
                return false;

            const int maxRetryCount = 8;
            for (int i = 0; i < maxRetryCount; ++i)
            {
                SpriteAniGroupData groupData = groupDataList[Random.Range(0, groupDataList.Count)];
                if (groupData == null || string.IsNullOrEmpty(groupData.AniResourceKey))
                    continue;

                animationResourceKey = groupData.AniResourceKey;
                return true;
            }

            for (int i = 0; i < groupDataList.Count; ++i)
            {
                SpriteAniGroupData groupData = groupDataList[i];
                if (groupData == null || string.IsNullOrEmpty(groupData.AniResourceKey))
                    continue;

                animationResourceKey = groupData.AniResourceKey;
                return true;
            }

            return false;
        }

        public void SetAnimationSpeed(float speed)
        {
            if (m_characterControllerBase != null)
                m_characterControllerBase.AniControllerSpeed = speed;
        }

        public void SetColor(Color color)
        {
            m_color = color;

            for (int i = 0; i < m_charAniPart.Count; ++i)
            {
                SpriteAniPart part = m_charAniPart[i];
                if (part == null || part.Renderer == null)
                    continue;

                part.Renderer.color = m_color;
            }
        }

        public void PlayAnimation_Once(CharacterState aniplaytype, bool loop)
        {
            PlayAnimation(aniplaytype);
        }

        public void PlayAnimation_Once(string aniId, bool loop)
        {
            CharacterState fallbackState = CharacterState.Idle;
            if (m_characterControllerBase != null)
                fallbackState = m_characterControllerBase.CharacterState;

            PlayAnimation(fallbackState, aniId);
        }

        public void PlayAnimation(CharacterState aniplaytype, string aniId = "")
        {
            if (CanPlayAnimation == false)
                return;

            m_frameSeleter?.PlayStateAnimation(aniplaytype, aniId);
        }

        public void ConnectAniActionState(System.Action<AnimationAction> action)
        {
            if (m_frameSeleter == null)
                return;

            m_frameSeleter.ConnectAniActionCallBack(action);
        }

        public List<SpriteAniPart> GetSpriteAniParts()
        {
            return m_charAniPart;
        }

        public void SetAnimationActionData(AnimationActionData actiondata)
        {
            if (actiondata == null)
                return;

            MoveToCharRoot(actiondata.CharWorldPosition);
            SetAniFrameData(actiondata.FrameDatas);
        }

        public void SetAniFrameData(List<AnimationFrameData> framedata)
        {
            ClearChangedParts();
            ReleasePlayingParticles();

            if (framedata == null)
                return;

            for (int i = 0; i < framedata.Count; ++i)
            {
                AnimationFrameData frameData = framedata[i];
                if (m_charAniPart_Dic.TryGetValue(frameData.PartID, out SpriteAniPart data) == false || data == null || data.Renderer == null)
                    continue;

                data.Renderer.sprite = frameData.Sprite;
                if (data.Renderer.sprite == null)
                    continue;

                int sortingOrder = frameData.OrderInLayer;
                data.Renderer.sortingOrder = sortingOrder;
                data.Renderer.transform.localPosition = frameData.LocalPosition;
                data.Renderer.transform.localEulerAngles = frameData.LocalRotation;
                data.Renderer.transform.localScale = frameData.LocalScale;
                data.Renderer.color = m_color;

                PlayParticle(frameData, data);
                m_changedParts.Add(data.PartID);
            }
        }

        public void HideBodyOrderSprite()
        {
            for (int i = 0; i < m_charAniPart.Count; ++i)
            {
                SpriteAniPart part = m_charAniPart[i];
                if (part == null || part.Renderer == null)
                    continue;

                if (part.PartID != AnimationPart.Body)
                    part.Renderer.sprite = null;
            }
        }

        private void AniActionCallBack(AnimationAction aniaction)
        {
            if (aniaction == AnimationAction.AniStart)
                AnimationEvent?.Invoke(m_frameSeleter.CurrentAnimationId, "Start");
            else if (aniaction == AnimationAction.AniEnd)
                AnimationEvent?.Invoke(m_frameSeleter.CurrentAnimationId, "End");
            else if (aniaction != AnimationAction.None)
                AnimationEvent?.Invoke(m_frameSeleter.CurrentAnimationId, aniaction.ToString());
        }

        private void MoveToCharRoot(Vector3 pos)
        {
            if (m_charTransform == null)
                return;

            Vector3 applypos = m_charTransform.position;
            if (m_characterControllerBase != null && m_characterControllerBase.LookDirection == Enum_LookDirection.Left)
                pos.x *= -1.0f;

            applypos += pos;
            m_charTransform.position = applypos;
        }

        private void PlayParticle(AnimationFrameData frameData, SpriteAniPart data)
        {
            if (frameData == null || string.IsNullOrEmpty(frameData.ParticleName) || ParticleManager.isAlive == false)
                return;

            ParticlePoolElement particlePoolElement = ParticleManager.Instance.GetParticle(frameData.ParticleBundleTag, frameData.ParticleName);
            if (particlePoolElement == null)
                return;

            if (frameData.ParticleWorldView == false)
            {
                particlePoolElement.transform.SetParent(data.Renderer.transform);
                particlePoolElement.transform.localPosition = Vector3.zero;
                particlePoolElement.transform.localEulerAngles = Vector3.zero;
            }
            else
            {
                particlePoolElement.transform.SetParent(null);
                particlePoolElement.transform.position = data.Renderer.transform.position;
                Vector3 rotate = Vector3.zero;
                rotate.y = m_characterControllerBase != null && m_characterControllerBase.LookDirection == Enum_LookDirection.Left ? 180.0f : 0.0f;
                particlePoolElement.transform.localEulerAngles = rotate;
            }

            particlePoolElement.gameObject.SetActive(true);
            particlePoolElement.PlayParticle();
            m_playingParticles.Add(particlePoolElement);
        }

        private void CacheAniParts()
        {
            m_charAniPart_Dic.Clear();

            for (int i = 0; i < m_charAniPart.Count; ++i)
            {
                SpriteAniPart part = m_charAniPart[i];
                if (part == null || part.Renderer == null)
                    continue;

                m_charAniPart_Dic[part.PartID] = part;
            }
        }

        private void ClearChangedParts()
        {
            for (int i = 0; i < m_changedParts.Count; ++i)
            {
                if (m_charAniPart_Dic.TryGetValue(m_changedParts[i], out SpriteAniPart part) == false || part == null || part.Renderer == null)
                    continue;

                part.Renderer.sprite = null;
            }

            m_changedParts.Clear();
        }

        private void ReleasePlayingParticles()
        {
            for (int i = 0; i < m_playingParticles.Count; ++i)
            {
                ParticlePoolElement particle = m_playingParticles[i];
                if (particle != null)
                    particle.StopParticle();
            }

            m_playingParticles.Clear();
        }
    }
}
