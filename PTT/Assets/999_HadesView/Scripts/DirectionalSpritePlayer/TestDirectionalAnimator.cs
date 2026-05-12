using System;
using UnityEngine;

namespace GameBerry.TestScene
{
    public enum EightDirection
    {
        NorthEast = 1,
        NorthWest = 2,
        SouthEast = 3,
        SouthWest = 4,
    }

    [Serializable]
    public class FrameParticleEvent
    {
        public EightDirection Direction = EightDirection.SouthEast;
        public GameObject ParticleObject;
        public Vector3 LocalOffset = Vector3.zero;
        public Vector3 RotationOffset = Vector3.zero;
    }

    [Serializable]
    public class AnimationFrameEventData
    {
        public int FrameIndex;
        public bool TriggerHit;
        public AudioClip Sound;
        //[ArrayElementTitle("Direction")]
        public System.Collections.Generic.List<FrameParticleEvent> Particles = new System.Collections.Generic.List<FrameParticleEvent>();

        public Transform Root;
    }

    public readonly struct AnimationFrameEventTrigger
    {
        public readonly CharacterState State;
        public readonly string AnimationKey;
        public readonly int FrameIndex;
        public readonly AnimationFrameEventData EventData;

        public AnimationFrameEventTrigger(CharacterState state, string animationKey, int frameIndex, AnimationFrameEventData eventData)
        {
            State = state;
            AnimationKey = animationKey;
            FrameIndex = frameIndex;
            EventData = eventData;
        }
    }

    public readonly struct AnimationPlaybackKey : IEquatable<AnimationPlaybackKey>
    {
        public readonly CharacterState State;
        public readonly string AnimationKey;

        public AnimationPlaybackKey(CharacterState state, string animationKey)
        {
            State = state;
            AnimationKey = NormalizeAnimationKey(animationKey);
        }

        public bool Equals(AnimationPlaybackKey other)
        {
            return State == other.State && AnimationKey == other.AnimationKey;
        }

        public override bool Equals(object obj)
        {
            return obj is AnimationPlaybackKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)State * 397) ^ (AnimationKey != null ? AnimationKey.GetHashCode() : 0);
            }
        }

        public static string NormalizeAnimationKey(string animationKey)
        {
            return string.IsNullOrWhiteSpace(animationKey) ? "Default" : animationKey.Trim();
        }
    }

    public abstract class TestDirectionalAnimator : MonoBehaviour
    {
        public event Action<CharacterState> StatePlaybackCompleted;
        public event Action<CharacterState, int> StateFrameTriggered;
        public event Action<AnimationFrameEventTrigger> FrameEventTriggered;

        public abstract CharacterState CurrentState { get; }
        public abstract string CurrentAnimationKey { get; }
        public abstract EightDirection CurrentDirection { get; }
        public abstract float CurrentPlaybackDuration { get; }
        public abstract bool AutoReturnToIdleOnAttackComplete { get; set; }

        public abstract void Initialize();
        public abstract void Play(CharacterState state, Vector3 moveDirection, bool forceRestart = false);
        public abstract void Play(CharacterState state, string animationKey, Vector3 moveDirection, bool forceRestart = false);
        public abstract void SetDirection(Vector3 moveDirection, bool forceRefresh = false);

        protected void RaiseStatePlaybackCompleted(CharacterState completedState)
        {
            StatePlaybackCompleted?.Invoke(completedState);
        }

        protected void RaiseStateFrameTriggered(CharacterState state, int frameIndex)
        {
            StateFrameTriggered?.Invoke(state, frameIndex);
        }

        protected void RaiseFrameEventTriggered(AnimationFrameEventTrigger frameEventTrigger)
        {
            FrameEventTriggered?.Invoke(frameEventTrigger);
        }

        public static EightDirection ResolveDirection(Vector3 moveDirection, EightDirection fallback)
        {
            Vector2 planar = new Vector2(moveDirection.x, moveDirection.y);
            if (planar.sqrMagnitude <= 0.0001f)
                return fallback;

            Vector2 fallbackVector = DirectionToVector(fallback);
            float x = Mathf.Abs(planar.x) <= 0.0001f ? fallbackVector.x : planar.x;
            float y = Mathf.Abs(planar.y) <= 0.0001f ? fallbackVector.y : planar.y;

            if (x >= 0.0f)
                return y >= 0.0f ? EightDirection.NorthEast : EightDirection.SouthEast;

            return y >= 0.0f ? EightDirection.NorthWest : EightDirection.SouthWest;
        }

        public static Vector2 DirectionToVector(EightDirection direction)
        {
            switch (direction)
            {
                case EightDirection.NorthEast:
                    return new Vector2(1.0f, 1.0f).normalized;
                case EightDirection.SouthEast:
                    return new Vector2(1.0f, -1.0f).normalized;
                case EightDirection.SouthWest:
                    return new Vector2(-1.0f, -1.0f).normalized;
                default:
                    return new Vector2(-1.0f, 1.0f).normalized;
            }
        }

        public static Vector3 DirectionToMoveVector(EightDirection direction)
        {
            Vector2 planarDirection = DirectionToVector(direction);
            return new Vector3(planarDirection.x, planarDirection.y, 0.0f);
        }

        protected static bool IsLeftDirection(EightDirection direction)
        {
            return direction == EightDirection.NorthWest
                || direction == EightDirection.SouthWest;
        }

        protected static EightDirection GetMirroredDirection(EightDirection direction)
        {
            switch (direction)
            {
                case EightDirection.NorthWest:
                    return EightDirection.NorthEast;
                case EightDirection.SouthWest:
                    return EightDirection.SouthEast;
                default:
                    return direction;
            }
        }
    }
}
