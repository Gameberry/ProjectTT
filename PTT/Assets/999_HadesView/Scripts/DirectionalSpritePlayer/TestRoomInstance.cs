using UnityEngine;

namespace GameBerry.TestScene
{
    public class TestRoomInstance : MonoBehaviour
    {
        [SerializeField] private Transform _playerSpawnPoint;
        [SerializeField] private TestRoomPortal _roomPortal;
        [SerializeField] private bool _isLobbyRoom;
        public SpriteRenderer _mapSpriteRenderer;

        public Transform PlayerSpawnPoint => _playerSpawnPoint != null ? _playerSpawnPoint : transform;
        public TestRoomPortal RoomPortal => _roomPortal;
        public bool IsLobbyRoom => _isLobbyRoom;

        public void ConfigureRuntime(Transform playerSpawnPoint, TestRoomPortal roomPortal, bool isLobbyRoom)
        {
            _playerSpawnPoint = playerSpawnPoint;
            _roomPortal = roomPortal;
            _isLobbyRoom = isLobbyRoom;
        }

        private void Reset()
        {
            if (_playerSpawnPoint == null)
            {
                Transform spawnChild = transform.Find("PlayerSpawnPoint");
                if (spawnChild != null)
                    _playerSpawnPoint = spawnChild;
            }

            if (_roomPortal == null)
                _roomPortal = GetComponentInChildren<TestRoomPortal>(true);
        }
    }
}
