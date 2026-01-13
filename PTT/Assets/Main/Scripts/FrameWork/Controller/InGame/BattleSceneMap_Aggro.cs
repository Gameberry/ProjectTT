using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class BattleSceneMap_Aggro : MonoBehaviour
    {
        private List<MonsterController> _monsters = new List<MonsterController>();

        private PlayerController _playerController = null;

        private void OnTriggerEnter(Collider other)
        {
            _playerController = other.GetComponent<PlayerController>();

            SetAggro();
        }

        private void OnTriggerExit(Collider other)
        {
            _playerController = other.GetComponent<PlayerController>();

            if (_playerController != null)
            {
                _playerController = null;
            }

            if (StaticResource.Instance.GetBattleModeStaticData().PlayerOutReturnMonster == true)
                SetAggro();
        }

        private void SetAggro()
        {
            for (int i = 0; i < _monsters.Count; ++i)
            {
                _monsters[i].SetAggro(_playerController);
            }
        }

        public void SpawnMonster(int count)
        {
            if (_monsters.Count >= count)
                return;

            int spawnCount = count - _monsters.Count;

            Vector3 rootpos = transform.position;

            List<Vector3> posList = PointSpawnPlacer2D.GeneratePositionsGuaranteed(rootpos, spawnCount,
                StaticResource.Instance.GetBattleModeStaticData().SpawnrouteRadius,
                StaticResource.Instance.GetBattleModeStaticData().MonsterMinSeparation);

            for (int i = 0; i < posList.Count; ++i)
            {
                Vector3 pos = posList[i];
                MonsterController monsterController = Managers.MonsterManager.Instance.GetMonster();
                monsterController.gameObject.SetActive(true);
                monsterController.transform.position = pos;
                monsterController.SetMonster(this, pos, StaticResource.Instance.GetBattleModeStaticData().MonsterModelIdxs.GetRandom());
                monsterController.SetAggro(_playerController);
                monsterController.Play();

                _monsters.Add(monsterController);
            }
        }

        public void OnDeadMonster(MonsterController monsterController)
        {
            _monsters.Remove(monsterController);
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            Gizmos.DrawWireSphere(transform.position, StaticResource.Instance.GetBattleModeStaticData().SpawnrouteRadius);
#endif
        }
    }

    public static class PointSpawnPlacer2D
    {
        public static List<Vector3> GeneratePositionsGuaranteed(
            Vector3 center,
            int desiredCount,
            float spawnRadius,      // 중심점 기준 반경 (기존 n 개념)
            float minSeparation,    // 몬스터 간 최소거리 (기존 d)
            int triesPerCandidate = 30,
            int maxTotalAttempts = 200_000,
            int seed = -1,

            // A: 완화 단계
            float[] relaxFactors = null,

            // B: 약한 밀어내기
            int separationIterations = 4,
            float separationStrength = 0.35f,

            // C: 강제 채움
            int forceFillAttemptsPerPoint = 2000,
            float forceFillMinSeparationFactor = 0.35f, // 강제 단계에서 d의 몇 %까지 허용할지
            float fallbackRingJitterFactor = 0.25f      // 최후 링 배치 시 지터 (minSeparation * 이 값)
        )
        {
            if (desiredCount <= 0) return new List<Vector3>();

            spawnRadius = Mathf.Max(0f, spawnRadius);
            minSeparation = Mathf.Max(0.001f, minSeparation);

            relaxFactors ??= new float[] { 1.0f, 0.95f, 0.90f, 0.85f, 0.80f, 0.75f, 0.70f, 0.65f, 0.60f, 0.55f, 0.50f };

            var rng = (seed >= 0) ? new System.Random(seed) : new System.Random();
            var positions = new List<Vector3>(desiredCount);

            // =========================
            // A: 엄격 -> 완화로 채우기
            // =========================
            int globalAttempts = 0;

            for (int fi = 0; fi < relaxFactors.Length && positions.Count < desiredCount; fi++)
            {
                float currentSeparation = minSeparation * relaxFactors[fi];
                var grid = new SpatialHash(currentSeparation);

                for (int i = 0; i < positions.Count; i++)
                    grid.Add(positions[i]);

                while (positions.Count < desiredCount && globalAttempts < maxTotalAttempts)
                {
                    globalAttempts++;

                    bool placed = false;
                    for (int t = 0; t < triesPerCandidate; t++)
                    {
                        Vector3 candidate = SampleInsideCircle(rng, center, spawnRadius);

                        if (grid.IsFarEnough(candidate, currentSeparation))
                        {
                            positions.Add(candidate);
                            grid.Add(candidate);
                            placed = true;
                            break;
                        }
                    }
                }
            }

            // =========================
            // B: 약한 밀어내기 정리
            // =========================
            if (positions.Count > 1 && separationIterations > 0 && separationStrength > 0f)
            {
                ResolveOverlapsWeak(
                    positions,
                    center,
                    spawnRadius,
                    minSeparation,
                    separationIterations,
                    Mathf.Clamp01(separationStrength)
                );
            }

            // =========================
            // C: 무조건 desiredCount 채우기
            // =========================
            if (positions.Count < desiredCount)
            {
                ForceFillToCount(
                    positions,
                    center,
                    spawnRadius,
                    desiredCount,
                    rng,
                    minSeparation,
                    forceFillAttemptsPerPoint,
                    Mathf.Clamp01(forceFillMinSeparationFactor),
                    Mathf.Clamp01(fallbackRingJitterFactor)
                );

                // 강제 채움 후 한 번 더 아주 약하게 정리
                if (positions.Count > 1 && separationIterations > 0 && separationStrength > 0f)
                {
                    ResolveOverlapsWeak(
                        positions,
                        center,
                        spawnRadius,
                        minSeparation,
                        Mathf.Max(2, separationIterations),
                        Mathf.Clamp01(separationStrength * 0.75f)
                    );
                }
            }

            // 최종 개수 보장
            if (positions.Count > desiredCount)
                positions.RemoveRange(desiredCount, positions.Count - desiredCount);

            return positions;
        }

        // =========================
        // Sampling
        // =========================
        private static Vector3 SampleInsideCircle(System.Random rng, Vector3 center, float radius)
        {
            if (radius <= 0f) return center;

            // 균일 분포: r = sqrt(u) * R
            double u = rng.NextDouble();
            double v = rng.NextDouble();
            double angle = v * Math.PI * 2.0;
            float r = (float)(Math.Sqrt(u) * radius);

            return center + new Vector3((float)Math.Cos(angle), 0, (float)Math.Sin(angle)) * r;
        }

        private static Vector3 RandomUnit2D(System.Random rng)
        {
            double angle = rng.NextDouble() * Math.PI * 2.0;
            return new Vector3((float)Math.Cos(angle), 0, (float)Math.Sin(angle));
        }

        private static Vector3 ClampToCircle(Vector3 p, Vector3 center, float radius)
        {
            if (radius <= 0f) return center;

            Vector3 d = p - center;
            float len = d.magnitude;
            if (len <= radius) return p;
            if (len < 1e-6f) return center;

            return center + d / len * radius;
        }

        // =========================
        // B: 약한 밀어내기
        // =========================
        private static void ResolveOverlapsWeak(
            List<Vector3> points,
            Vector3 center,
            float spawnRadius,
            float targetSeparation,
            int iterations,
            float strength
        )
        {
            float sep = Mathf.Max(0.001f, targetSeparation);
            float sepSqr = sep * sep;

            for (int iter = 0; iter < iterations; iter++)
            {
                var grid = new SpatialHash(sep);
                for (int i = 0; i < points.Count; i++)
                    grid.Add(points[i]);

                for (int i = 0; i < points.Count; i++)
                {
                    Vector3 p = points[i];
                    var neighbors = grid.QueryNeighbors(p);

                    Vector3 push = Vector3.zero;
                    int pushCount = 0;

                    for (int k = 0; k < neighbors.Count; k++)
                    {
                        Vector3 q = neighbors[k];
                        Vector3 d = p - q;
                        float distSqr = d.sqrMagnitude;
                        if (distSqr < 1e-12f) continue;

                        if (distSqr < sepSqr)
                        {
                            float dist = Mathf.Sqrt(distSqr);
                            float overlap = (sep - dist);
                            Vector3 dir = d / dist;
                            push += dir * overlap;
                            pushCount++;
                        }
                    }

                    if (pushCount > 0)
                    {
                        p += (push / pushCount) * strength;
                        p = ClampToCircle(p, center, spawnRadius);
                        points[i] = p;
                    }
                    else
                    {
                        points[i] = ClampToCircle(p, center, spawnRadius);
                    }
                }
            }
        }

        // =========================
        // C: Force Fill
        // =========================
        private static void ForceFillToCount(
            List<Vector3> points,
            Vector3 center,
            float spawnRadius,
            int desiredCount,
            System.Random rng,
            float minSeparation,
            int attemptsPerPoint,
            float minSepFactor,
            float ringJitterFactor
        )
        {
            float relaxedSep = Mathf.Max(0.001f, minSeparation * minSepFactor);

            // 1) 랜덤 배치 + 완화된 간격으로 최대한 넣기
            var grid = new SpatialHash(relaxedSep);
            for (int i = 0; i < points.Count; i++)
                grid.Add(points[i]);

            while (points.Count < desiredCount)
            {
                bool placed = false;

                for (int t = 0; t < attemptsPerPoint; t++)
                {
                    Vector3 candidate = SampleInsideCircle(rng, center, spawnRadius);
                    if (grid.IsFarEnough(candidate, relaxedSep))
                    {
                        points.Add(candidate);
                        grid.Add(candidate);
                        placed = true;
                        break;
                    }
                }

                if (placed) continue;

                // 2) 최후 보루: 원 둘레에 균등 배치 + 지터
                int idx = points.Count;
                float angle = ((idx + 0.5f) / desiredCount) * Mathf.PI * 2f;

                Vector3 onRing = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * spawnRadius;
                Vector3 jitter = RandomUnit2D(rng) * (minSeparation * ringJitterFactor);

                Vector3 forced = ClampToCircle(onRing + jitter, center, spawnRadius);
                points.Add(forced);
                grid.Add(forced);
            }
        }

        // =========================
        // Spatial Hash
        // =========================
        private sealed class SpatialHash
        {
            private readonly float _cellSize;
            private readonly Dictionary<Vector3Int, List<Vector3>> _cells = new Dictionary<Vector3Int, List<Vector3>>(1024);

            public SpatialHash(float cellSize)
            {
                _cellSize = Mathf.Max(0.001f, cellSize);
            }

            public void Add(Vector3 p)
            {
                Vector3Int c = ToCell(p);
                if (!_cells.TryGetValue(c, out var list))
                {
                    list = new List<Vector3>(8);
                    _cells[c] = list;
                }
                list.Add(p);
            }

            public bool IsFarEnough(Vector3 p, float minDist)
            {
                float minDistSqr = minDist * minDist;
                Vector3Int c = ToCell(p);

                for (int y = -1; y <= 1; y++)
                    for (int x = -1; x <= 1; x++)
                    {
                        Vector3Int nc = new Vector3Int(c.x + x, c.y + y);
                        if (!_cells.TryGetValue(nc, out var list)) continue;

                        for (int i = 0; i < list.Count; i++)
                        {
                            if ((list[i] - p).sqrMagnitude < minDistSqr)
                                return false;
                        }
                    }

                return true;
            }

            public List<Vector3> QueryNeighbors(Vector3 p)
            {
                Vector3Int c = ToCell(p);
                var neighbors = new List<Vector3>(32);

                for (int y = -1; y <= 1; y++)
                    for (int x = -1; x <= 1; x++)
                    {
                        Vector3Int nc = new Vector3Int(c.x + x, c.y + y);
                        if (_cells.TryGetValue(nc, out var list))
                            neighbors.AddRange(list);
                    }

                return neighbors;
            }

            private Vector3Int ToCell(Vector3 p)
            {
                int cx = Mathf.FloorToInt(p.x / _cellSize);
                int cy = Mathf.FloorToInt(p.y / _cellSize);
                return new Vector3Int(cx, 0, cy);
            }
        }
    }
}