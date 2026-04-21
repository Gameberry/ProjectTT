using UnityEngine;

namespace GameBerry.TestScene
{
    public class TestDirectionalPlayerController : MonoBehaviour
    {
        [SerializeField] private TestDirectionalSpriteAnimator _spriteAnimator;
        [SerializeField] private float _moveSpeed = 3.5f;
        [SerializeField] private bool _supportWASD = true;
        [SerializeField] private bool _enablePreviewHotKeys = true;

        private Vector3 _lastMoveDirection = Vector3.back;
        private CharacterState _previewState = CharacterState.None;

        private void Reset()
        {
            EnsureDependencies();
        }

        private void Awake()
        {
            EnsureDependencies();
            _spriteAnimator.StatePlaybackCompleted += HandleStatePlaybackCompleted;
            _spriteAnimator.Play(CharacterState.Idle, _lastMoveDirection, true);
        }

        private void OnDestroy()
        {
            if (_spriteAnimator != null)
                _spriteAnimator.StatePlaybackCompleted -= HandleStatePlaybackCompleted;
        }

        private void Update()
        {
            UpdatePreviewState();

            if (IsPreviewLockedState(_previewState))
            {
                _spriteAnimator.Play(_previewState, _lastMoveDirection);
                return;
            }

            Vector3 moveDirection = ReadMoveInput();
            if (moveDirection.sqrMagnitude > 1.0f)
                moveDirection.Normalize();

            if (moveDirection.sqrMagnitude > 0.0001f)
            {
                transform.position += moveDirection * (_moveSpeed * Time.deltaTime);
                _lastMoveDirection = moveDirection.normalized;
                _spriteAnimator.Play(CharacterState.Run, _lastMoveDirection);
            }
            else
            {
                CharacterState idleLikeState = _previewState == CharacterState.None ? CharacterState.Idle : _previewState;
                _spriteAnimator.Play(idleLikeState, _lastMoveDirection);
            }
        }

        private void EnsureDependencies()
        {
            if (_spriteAnimator == null)
                _spriteAnimator = GetComponent<TestDirectionalSpriteAnimator>();

            if (_spriteAnimator == null)
                _spriteAnimator = gameObject.AddComponent<TestDirectionalSpriteAnimator>();
        }

        private void HandleStatePlaybackCompleted(CharacterState completedState)
        {
            if (completedState == CharacterState.Attack)
                _previewState = CharacterState.None;
        }

        private Vector3 ReadMoveInput()
        {
            float horizontal = 0.0f;
            float vertical = 0.0f;

            if (Input.GetKey(KeyCode.LeftArrow) || (_supportWASD && Input.GetKey(KeyCode.A)))
                horizontal -= 1.0f;

            if (Input.GetKey(KeyCode.RightArrow) || (_supportWASD && Input.GetKey(KeyCode.D)))
                horizontal += 1.0f;

            if (Input.GetKey(KeyCode.UpArrow) || (_supportWASD && Input.GetKey(KeyCode.W)))
                vertical += 1.0f;

            if (Input.GetKey(KeyCode.DownArrow) || (_supportWASD && Input.GetKey(KeyCode.S)))
                vertical -= 1.0f;

            return new Vector3(horizontal, 0.0f, vertical);
        }

        private void UpdatePreviewState()
        {
            if (_enablePreviewHotKeys == false)
                return;

            if (Input.GetKeyDown(KeyCode.Alpha0))
                _previewState = CharacterState.None;
            else if (Input.GetKeyDown(KeyCode.Alpha1))
                _previewState = CharacterState.Attack;
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                _previewState = CharacterState.Hit;
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                _previewState = CharacterState.Skill;
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                _previewState = CharacterState.Tran;
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                _previewState = CharacterState.Dead;
        }

        private static bool IsPreviewLockedState(CharacterState state)
        {
            return state == CharacterState.Attack
                || state == CharacterState.Hit
                || state == CharacterState.Skill
                || state == CharacterState.Tran
                || state == CharacterState.Dead;
        }
    }
}
