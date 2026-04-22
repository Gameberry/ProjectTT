using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.TestScene
{
    public class TestSpatialHash2D<T>
    {
        private readonly float _cellSize;
        private readonly Dictionary<Vector2Int, List<T>> _cells = new Dictionary<Vector2Int, List<T>>();

        public float CellSize => _cellSize;
        public IEnumerable<KeyValuePair<Vector2Int, List<T>>> Cells => _cells;

        public TestSpatialHash2D(float cellSize)
        {
            _cellSize = Mathf.Max(0.01f, cellSize);
        }

        public void Clear()
        {
            _cells.Clear();
        }

        public void Add(Vector2 position, T item)
        {
            Vector2Int cell = WorldToCell(position);
            if (_cells.TryGetValue(cell, out List<T> bucket) == false)
            {
                bucket = new List<T>();
                _cells.Add(cell, bucket);
            }

            bucket.Add(item);
        }

        public void Query(Vector2 position, float radius, List<T> results)
        {
            results.Clear();

            int cellRadius = Mathf.CeilToInt(Mathf.Max(0.0f, radius) / _cellSize);
            Vector2Int center = WorldToCell(position);

            for (int y = -cellRadius; y <= cellRadius; y++)
            {
                for (int x = -cellRadius; x <= cellRadius; x++)
                {
                    Vector2Int cell = new Vector2Int(center.x + x, center.y + y);
                    if (_cells.TryGetValue(cell, out List<T> bucket) == false)
                        continue;

                    for (int i = 0; i < bucket.Count; i++)
                        results.Add(bucket[i]);
                }
            }
        }

        private Vector2Int WorldToCell(Vector2 position)
        {
            return new Vector2Int(
                Mathf.FloorToInt(position.x / _cellSize),
                Mathf.FloorToInt(position.y / _cellSize));
        }

        public Vector2 CellToWorldCenter(Vector2Int cell)
        {
            return new Vector2(
                (cell.x + 0.5f) * _cellSize,
                (cell.y + 0.5f) * _cellSize);
        }
    }
}
