using UnityEngine;

namespace GameBerry.TestScene
{
    internal static class TestSteeringUtils
    {
        public static readonly Vector2[] Directions8 = CreateDirections();
        private static readonly Collider2D[] WallBuffer = new Collider2D[16];

        private static Vector2[] CreateDirections()
        {
            var dirs = new Vector2[8];
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f * Mathf.Deg2Rad;
                dirs[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            }
            return dirs;
        }

        public static Vector3 ResolveWallOverlaps(Vector3 position, float bodyRadius, LayerMask wallLayerMask)
        {
            Vector2 pos2D = (Vector2)position;
            var filter = new ContactFilter2D { layerMask = wallLayerMask, useLayerMask = true, useTriggers = false };
            int count = Physics2D.OverlapCircle(pos2D, bodyRadius, filter, WallBuffer);

            for (int i = 0; i < count; i++)
            {
                Collider2D wall = WallBuffer[i];
                if (wall == null)
                    continue;

                Vector2 closest = wall.ClosestPoint(pos2D);
                Vector2 delta = pos2D - closest;
                float distance = delta.magnitude;

                if (distance >= bodyRadius)
                    continue;

                Vector2 pushDir = distance > 0.0001f ? delta / distance : Vector2.up;
                pos2D += pushDir * (bodyRadius - distance);
            }

            return new Vector3(pos2D.x, pos2D.y, position.z);
        }
    }
}
