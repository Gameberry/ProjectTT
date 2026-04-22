using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.TestScene
{
    public class TestDirectionalMonsterManager : MonoBehaviour
    {
        private static TestDirectionalMonsterManager _instance;

        [SerializeField] private float _collisionQueryCellSize = 1.0f;
        [SerializeField] private bool _drawGridGizmos = true;
        [SerializeField] private bool _drawOnlyOccupiedCells = false;
        [SerializeField] private int _debugGridRadiusInCells = 12;
        [SerializeField] private Color _emptyCellLineColor = new Color(0.3f, 0.8f, 1.0f, 0.25f);
        [SerializeField] private Color _occupiedCellFillColor = new Color(1.0f, 0.5f, 0.15f, 0.22f);
        [SerializeField] private Color _occupiedCellLineColor = new Color(1.0f, 0.7f, 0.2f, 0.8f);

        private readonly List<TestDirectionalMonsterController> _monsters = new List<TestDirectionalMonsterController>();
        private TestSpatialHash2D<TestDirectionalMonsterController> _collisionGrid;
        private int _lastRebuildFrame = -1;

        public static TestDirectionalMonsterManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject managerObject = new GameObject("TestDirectionalMonsterManager");
                    _instance = managerObject.AddComponent<TestDirectionalMonsterManager>();
                }

                return _instance;
            }
        }

        public static bool HasInstance => _instance != null;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            EnsureGrid();
        }

        public void Register(TestDirectionalMonsterController monster)
        {
            if (monster == null || _monsters.Contains(monster))
                return;

            _monsters.Add(monster);
            EnsureGrid();
            _lastRebuildFrame = -1;
        }

        public void Unregister(TestDirectionalMonsterController monster)
        {
            _monsters.Remove(monster);
            _lastRebuildFrame = -1;
        }

        public void QueryMonsters(Vector2 position, float radius, List<TestDirectionalMonsterController> results)
        {
            EnsureGrid();
            EnsureGridUpdated();
            _collisionGrid.Query(position, radius, results);
        }

        private void EnsureGrid()
        {
            if (_collisionGrid == null)
                _collisionGrid = new TestSpatialHash2D<TestDirectionalMonsterController>(_collisionQueryCellSize);
        }

        private void EnsureGridUpdated()
        {
            if (_lastRebuildFrame == Time.frameCount)
                return;

            RebuildSpatialHash();
            _lastRebuildFrame = Time.frameCount;
        }

        private void RebuildSpatialHash()
        {
            EnsureGrid();
            _collisionGrid.Clear();

            for (int i = _monsters.Count - 1; i >= 0; i--)
            {
                TestDirectionalMonsterController monster = _monsters[i];
                if (monster == null)
                {
                    _monsters.RemoveAt(i);
                    continue;
                }

                Vector3 worldPosition = monster.transform.position;
                _collisionGrid.Add(new Vector2(worldPosition.x, worldPosition.y), monster);
            }
        }

        private void OnDrawGizmos()
        {
            if (_drawGridGizmos == false)
                return;

            EnsureGrid();
            if (Application.isPlaying == false || _lastRebuildFrame != Time.frameCount)
                RebuildSpatialHash();

            if (_drawOnlyOccupiedCells)
            {
                DrawOccupiedCells();
                return;
            }

            DrawGridAroundMonsters();
            DrawOccupiedCells();
        }

        private void DrawGridAroundMonsters()
        {
            float cellSize = _collisionGrid.CellSize;
            int radius = Mathf.Max(0, _debugGridRadiusInCells);
            Vector2Int centerCell = GetGridCenterCell();

            Gizmos.color = _emptyCellLineColor;
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    Vector2Int cell = new Vector2Int(centerCell.x + x, centerCell.y + y);
                    DrawCellWire(cell, cellSize, _emptyCellLineColor);
                }
            }
        }

        private void DrawOccupiedCells()
        {
            float cellSize = _collisionGrid.CellSize;
            foreach (KeyValuePair<Vector2Int, List<TestDirectionalMonsterController>> pair in _collisionGrid.Cells)
            {
                if (pair.Value == null || pair.Value.Count == 0)
                    continue;

                DrawCellSolid(pair.Key, cellSize, _occupiedCellFillColor);
                DrawCellWire(pair.Key, cellSize, _occupiedCellLineColor);
            }
        }

        private Vector2Int GetGridCenterCell()
        {
            if (_monsters.Count > 0)
            {
                for (int i = 0; i < _monsters.Count; i++)
                {
                    if (_monsters[i] == null)
                        continue;

                    Vector3 position = _monsters[i].transform.position;
                    return new Vector2Int(
                        Mathf.FloorToInt(position.x / _collisionGrid.CellSize),
                        Mathf.FloorToInt(position.y / _collisionGrid.CellSize));
                }
            }

            return Vector2Int.zero;
        }

        private static void DrawCellWire(Vector2Int cell, float cellSize, Color color)
        {
            Gizmos.color = color;
            Vector3 center = new Vector3((cell.x + 0.5f) * cellSize, (cell.y + 0.5f) * cellSize, 0.0f);
            Vector3 size = new Vector3(cellSize, cellSize, 0.0f);
            Gizmos.DrawWireCube(center, size);
        }

        private static void DrawCellSolid(Vector2Int cell, float cellSize, Color color)
        {
            Gizmos.color = color;
            Vector3 center = new Vector3((cell.x + 0.5f) * cellSize, (cell.y + 0.5f) * cellSize, 0.0f);
            Vector3 size = new Vector3(cellSize, cellSize, 0.0f);
            Gizmos.DrawCube(center, size);
        }
    }
}
