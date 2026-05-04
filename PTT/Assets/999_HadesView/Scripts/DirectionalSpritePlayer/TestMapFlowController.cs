using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.TestScene
{
    public class TestMapFlowController : MonoBehaviour
    {
        private static TestMapFlowController _instance;

        [SerializeField] private GameObject _lobbyRoomPrefab;
        [SerializeField] private List<TestMapDefinition> _maps = new List<TestMapDefinition>();
        [SerializeField] private TestMapSelectionUI _mapSelectionUI;
        [SerializeField] private TestDungeonResultUI _dungeonResultUI;
        [SerializeField] private Transform _roomRoot;
        [SerializeField] private float _lastRoomClearDelay = 0.5f;
        [SerializeField] private float _returnToLobbyDelayOnDeath = 1.0f;
        [SerializeField] private float _returnToLobbyDelayOnClear = 1.5f;
        [SerializeField] private TestDirectionalCameraFollow _cameraFollow;

        private readonly HashSet<string> _clearedMapIds = new HashSet<string>();
        private static TestMapDefinition s_runtimeMapA;
        private static TestMapDefinition s_runtimeMapB;
        private static GameObject s_runtimeLobbyRoomPrefab;

        private TestDirectionalPlayerController _player;
        private GameObject _currentRoomObject;
        private TestRoomInstance _currentRoomInstance;
        private int _currentRoomIndex = -1;
        private bool _isDungeonClearPending;
        private bool _returningToLobby;

        public static TestMapFlowController Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject controllerObject = new GameObject("TestMapFlowController");
                    _instance = controllerObject.AddComponent<TestMapFlowController>();
                }

                return _instance;
            }
        }

        public event Action<TestMapDefinition> MapSelected;

        public IReadOnlyList<TestMapDefinition> Maps => _maps;
        public TestMapDefinition SelectedMap { get; private set; }

        private void Start()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            EnsureDefaultMaps();
            EnsurePlayer();
            EnsureRoomRoot();
            EnsureUi();
            LoadLobby();
        }

        private void Update()
        {
            if (_currentRoomInstance == null || _currentRoomInstance.IsLobbyRoom)
                return;

            if (_isDungeonClearPending)
                return;

            if (AreAllRoomMonstersCleared() == false)
                return;

            _isDungeonClearPending = true;

            if (IsLastRoom())
            {
                CancelInvoke(nameof(HandleMapCleared));
                Invoke(nameof(HandleMapCleared), Mathf.Max(0.0f, _lastRoomClearDelay));
                return;
            }

            SetCurrentPortalActive(true);
        }

        public void OpenMapSelection()
        {
            if (_returningToLobby)
                return;

            EnsureUi();
            _mapSelectionUI.Show(this);

            if (_cameraFollow != null)
                _cameraFollow.SetMapSpriteRenderer(_currentRoomInstance?._mapSpriteRenderer);
        }

        public void CloseMapSelection()
        {
            if (_mapSelectionUI != null)
                _mapSelectionUI.Hide();
        }

        public bool IsUnlocked(TestMapDefinition mapDefinition)
        {
            if (mapDefinition == null)
                return false;

            TestMapDefinition requiredMap = mapDefinition.RequiredClearMap;
            if (requiredMap == null)
                return true;

            return string.IsNullOrWhiteSpace(requiredMap.MapId) == false && _clearedMapIds.Contains(requiredMap.MapId);
        }

        public bool IsCleared(TestMapDefinition mapDefinition)
        {
            return mapDefinition != null
                && string.IsNullOrWhiteSpace(mapDefinition.MapId) == false
                && _clearedMapIds.Contains(mapDefinition.MapId);
        }

        public void MarkMapCleared(TestMapDefinition mapDefinition)
        {
            if (mapDefinition == null || string.IsNullOrWhiteSpace(mapDefinition.MapId))
                return;

            _clearedMapIds.Add(mapDefinition.MapId);
        }

        public bool TrySelectMap(TestMapDefinition mapDefinition)
        {
            if (IsUnlocked(mapDefinition) == false)
                return false;

            SelectedMap = mapDefinition;
            MapSelected?.Invoke(mapDefinition);
            CloseMapSelection();
            Debug.Log($"[TestMapFlow] Selected map: {mapDefinition.DisplayName} ({mapDefinition.MapId})");
            StartSelectedMap();
            return true;
        }

        public void NotifyPortalEntered(TestRoomPortal enteredPortal)
        {
            if (_returningToLobby)
                return;

            if (_currentRoomInstance == null || enteredPortal == null)
                return;

            if (_currentRoomInstance.RoomPortal != enteredPortal)
                return;

            if (_currentRoomInstance.IsLobbyRoom)
            {
                OpenMapSelection();
                return;
            }

            GoToNextRoomOrLobby();
        }

        private void EnsureUi()
        {
            if (_mapSelectionUI != null)
            {
                EnsureResultUi();
                return;
            }

            _mapSelectionUI = FindObjectOfType<TestMapSelectionUI>();
            if (_mapSelectionUI != null)
            {
                EnsureResultUi();
                return;
            }

            GameObject uiObject = new GameObject("TestMapSelectionUI");
            Canvas parentCanvas = FindObjectOfType<Canvas>();
            if (parentCanvas != null)
                uiObject.transform.SetParent(parentCanvas.transform, false);
            _mapSelectionUI = uiObject.AddComponent<TestMapSelectionUI>();
            EnsureResultUi();
        }

        private void EnsureResultUi()
        {
            if (_dungeonResultUI != null)
                return;

            _dungeonResultUI = FindObjectOfType<TestDungeonResultUI>();
            if (_dungeonResultUI != null)
                return;

            GameObject uiObject = new GameObject("TestDungeonResultUI");
            _dungeonResultUI = uiObject.AddComponent<TestDungeonResultUI>();
        }

        private void EnsureRoomRoot()
        {
            if (_roomRoot != null)
                return;

            Transform existingRoot = transform.Find("RuntimeRoomRoot");
            if (existingRoot != null)
            {
                _roomRoot = existingRoot;
                return;
            }

            GameObject rootObject = new GameObject("RuntimeRoomRoot");
            rootObject.transform.SetParent(transform, false);
            _roomRoot = rootObject.transform;
        }

        private void EnsurePlayer()
        {
            if (_player != null)
                return;

            _player = FindObjectOfType<TestDirectionalPlayerController>();
            if (_player != null)
                _player.Died += HandlePlayerDied;
        }

        private void EnsureDefaultMaps()
        {
            bool hasAnyMap = false;
            for (int i = 0; i < _maps.Count; i++)
            {
                if (_maps[i] == null)
                    continue;

                hasAnyMap = true;
                break;
            }

            if (hasAnyMap)
                return;

            if (s_runtimeMapA == null)
            {
                s_runtimeMapA = ScriptableObject.CreateInstance<TestMapDefinition>();
                s_runtimeMapA.name = "Runtime_Map_A";
                s_runtimeMapA.ConfigureRuntime("map.a", "A Map", null);
            }

            if (s_runtimeMapB == null)
            {
                s_runtimeMapB = ScriptableObject.CreateInstance<TestMapDefinition>();
                s_runtimeMapB.name = "Runtime_Map_B";
                s_runtimeMapB.ConfigureRuntime("map.b", "B Map", s_runtimeMapA);
            }

            _maps.Clear();
            _maps.Add(s_runtimeMapA);
            _maps.Add(s_runtimeMapB);

            if (_lobbyRoomPrefab == null)
                _lobbyRoomPrefab = GetOrCreateRuntimeLobbyPrefab();
        }

        private void StartSelectedMap()
        {
            if (SelectedMap == null || SelectedMap.RoomCount <= 0)
                return;

            _currentRoomIndex = 0;
            LoadRoom(SelectedMap.RoomPrefabs[_currentRoomIndex], false);
        }

        private void LoadLobby()
        {
            CancelInvoke(nameof(HandleMapCleared));
            CancelInvoke(nameof(ReturnToLobbyAfterDeath));
            _returningToLobby = false;
            _isDungeonClearPending = false;
            SelectedMap = null;
            _currentRoomIndex = -1;
            EnsureResultUi();
            _dungeonResultUI?.Hide();
            LoadRoom(_lobbyRoomPrefab, true);
        }

        private void LoadRoom(GameObject roomPrefab, bool isLobby)
        {
            EnsurePlayer();
            EnsureRoomRoot();
            CancelInvoke(nameof(HandleMapCleared));
            DestroyCurrentRoom();

            if (roomPrefab == null)
                return;

            _currentRoomObject = Instantiate(roomPrefab, _roomRoot);
            _currentRoomObject.SetActive(true);
            _currentRoomInstance = _currentRoomObject.GetComponent<TestRoomInstance>();
            if (_currentRoomInstance == null)
                _currentRoomInstance = _currentRoomObject.AddComponent<TestRoomInstance>();

            if (_player != null)
            {
                _player.ClearAutoMoveDestination();
                _player.ResetForSpawn(_currentRoomInstance.PlayerSpawnPoint.position, isLobby);
            }

            SetCurrentPortalActive(isLobby);
            _isDungeonClearPending = isLobby;


            if (_cameraFollow != null)
                _cameraFollow.SetMapSpriteRenderer(_currentRoomInstance?._mapSpriteRenderer);
        }

        private void GoToNextRoomOrLobby()
        {
            if (SelectedMap == null)
            {
                LoadLobby();
                return;
            }

            int nextRoomIndex = _currentRoomIndex + 1;
            if (nextRoomIndex >= SelectedMap.RoomCount)
            {
                HandleMapCleared();
                return;
            }

            _currentRoomIndex = nextRoomIndex;
            LoadRoom(SelectedMap.RoomPrefabs[_currentRoomIndex], false);
        }

        private bool IsLastRoom()
        {
            return SelectedMap != null
                && SelectedMap.RoomCount > 0
                && _currentRoomIndex >= 0
                && _currentRoomIndex == SelectedMap.RoomCount - 1;
        }

        private bool AreAllRoomMonstersCleared()
        {
            if (_currentRoomInstance == null)
                return false;

            TestDirectionalMonsterController[] monsters = _currentRoomInstance.GetComponentsInChildren<TestDirectionalMonsterController>(true);
            for (int i = 0; i < monsters.Length; i++)
            {
                TestDirectionalMonsterController monster = monsters[i];
                if (monster == null)
                    continue;

                if (monster.IsDead == false)
                    return false;
            }

            return true;
        }

        private void SetCurrentPortalActive(bool active)
        {
            if (_currentRoomInstance == null || _currentRoomInstance.RoomPortal == null)
            {
                _player?.ClearAutoMoveDestination();
                return;
            }

            _currentRoomInstance.RoomPortal.SetPortalActive(active);

            if (active && _currentRoomInstance.IsLobbyRoom == false)
                _player?.SetAutoMoveDestination(_currentRoomInstance.RoomPortal.transform);
            else
                _player?.ClearAutoMoveDestination();
        }

        private void DestroyCurrentRoom()
        {
            if (_currentRoomObject != null)
                Destroy(_currentRoomObject);

            _currentRoomObject = null;
            _currentRoomInstance = null;
        }

        private void HandlePlayerDied()
        {
            if (_returningToLobby)
                return;

            if (_currentRoomInstance == null || _currentRoomInstance.IsLobbyRoom)
                return;

            _returningToLobby = true;
            CloseMapSelection();
            SetCurrentPortalActive(false);
            EnsureResultUi();
            _dungeonResultUI?.ShowFail("You were defeated.\nReturning to the lobby...");
            CancelInvoke(nameof(ReturnToLobbyAfterDeath));
            Invoke(nameof(ReturnToLobbyAfterDeath), Mathf.Max(0.0f, _returnToLobbyDelayOnDeath));
        }

        private void HandleMapCleared()
        {
            if (_returningToLobby)
                return;

            _returningToLobby = true;
            MarkMapCleared(SelectedMap);
            CloseMapSelection();
            SetCurrentPortalActive(false);
            EnsureResultUi();

            string mapName = SelectedMap != null && string.IsNullOrWhiteSpace(SelectedMap.DisplayName) == false
                ? SelectedMap.DisplayName
                : "Dungeon";

            _dungeonResultUI?.ShowClear($"{mapName} clear!\nReturning to the lobby...");
            CancelInvoke(nameof(ReturnToLobbyAfterDeath));
            Invoke(nameof(ReturnToLobbyAfterDeath), Mathf.Max(0.0f, _returnToLobbyDelayOnClear));
        }

        private void ReturnToLobbyAfterDeath()
        {
            LoadLobby();
        }

        private static GameObject GetOrCreateRuntimeLobbyPrefab()
        {
            if (s_runtimeLobbyRoomPrefab != null)
                return s_runtimeLobbyRoomPrefab;

            s_runtimeLobbyRoomPrefab = new GameObject("RuntimeLobbyRoomPrefab");
            TestRoomInstance roomInstance = s_runtimeLobbyRoomPrefab.AddComponent<TestRoomInstance>();
            GameObject spawnPoint = new GameObject("PlayerSpawnPoint");
            spawnPoint.transform.SetParent(s_runtimeLobbyRoomPrefab.transform, false);
            spawnPoint.transform.localPosition = Vector3.zero;

            GameObject portalObject = new GameObject("LobbyPortal");
            portalObject.transform.SetParent(s_runtimeLobbyRoomPrefab.transform, false);
            portalObject.transform.localPosition = new Vector3(2.0f, 0.0f, 0.0f);
            TestRoomPortal roomPortal = portalObject.AddComponent<TestRoomPortal>();
            roomInstance.ConfigureRuntime(spawnPoint.transform, roomPortal, true);

            s_runtimeLobbyRoomPrefab.SetActive(false);
            return s_runtimeLobbyRoomPrefab;
        }
    }
}
