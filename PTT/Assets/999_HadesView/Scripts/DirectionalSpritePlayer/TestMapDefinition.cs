using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.TestScene
{
    [CreateAssetMenu(fileName = "TestMapDefinition", menuName = "GameBerry/Test Scene/Test Map Definition")]
    public class TestMapDefinition : ScriptableObject
    {
        [SerializeField] private string _mapId = "map.a";
        [SerializeField] private string _displayName = "A Map";
        [SerializeField] private TestMapDefinition _requiredClearMap;
        [SerializeField] private List<GameObject> _roomPrefabs = new List<GameObject>();

        public string MapId => _mapId;
        public string DisplayName => string.IsNullOrWhiteSpace(_displayName) ? _mapId : _displayName;
        public TestMapDefinition RequiredClearMap => _requiredClearMap;
        public IReadOnlyList<GameObject> RoomPrefabs => _roomPrefabs;
        public int RoomCount => _roomPrefabs != null ? _roomPrefabs.Count : 0;

        public void ConfigureRuntime(string mapId, string displayName, TestMapDefinition requiredClearMap)
        {
            _mapId = mapId;
            _displayName = displayName;
            _requiredClearMap = requiredClearMap;
        }
    }
}
