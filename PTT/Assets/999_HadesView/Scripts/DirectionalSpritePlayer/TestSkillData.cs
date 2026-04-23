using UnityEngine;

namespace GameBerry.TestScene
{
    public enum TestSkillExecutionType
    {
        SectorHit,
        DashSlash,
    }

    [CreateAssetMenu(fileName = "TestSkillData", menuName = "GameBerry/Test Scene/Test Skill Data")]
    public class TestSkillData : ScriptableObject
    {
        public const string ByungRyeokIlSeomSkillId = "skill.byungryeokilseom";
        public const string ByungRyeokIlSeomAnimationKey = "ByungRyeokIlSeom";

        [SerializeField] private string _skillId = ByungRyeokIlSeomSkillId;
        [SerializeField] private string _animationKey = ByungRyeokIlSeomAnimationKey;
        [SerializeField] private CharacterState _playbackState = CharacterState.Skill;
        [SerializeField] private TestSkillExecutionType _executionType = TestSkillExecutionType.DashSlash;
        [SerializeField] private int _damage = 24;
        [SerializeField] private float _range = 2.5f;
        [SerializeField] private float _angle = 120.0f;
        [SerializeField] private float _dashDistance = 2.6f;
        [SerializeField] private float _dashDuration = 0.16f;
        [SerializeField] private float _dashHitRadius = 0.6f;
        [SerializeField] private bool _lockMovementDuringPlayback = true;

        public string SkillId => _skillId;
        public string AnimationKey => AnimationPlaybackKey.NormalizeAnimationKey(_animationKey);
        public CharacterState PlaybackState => _playbackState;
        public TestSkillExecutionType ExecutionType => _executionType;
        public int Damage => _damage;
        public float Range => _range;
        public float Angle => _angle;
        public float DashDistance => _dashDistance;
        public float DashDuration => _dashDuration;
        public float DashHitRadius => _dashHitRadius;
        public bool LockMovementDuringPlayback => _lockMovementDuringPlayback;
    }
}
