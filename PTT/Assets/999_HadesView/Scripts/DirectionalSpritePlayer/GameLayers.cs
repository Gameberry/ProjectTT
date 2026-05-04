using UnityEngine;

namespace GameBerry.TestScene
{
    // Unity Project Settings > Tags and Layers 에 레이어 이름을 등록해야 동작함.
    public static class GameLayers
    {
        private static int _mapBoundary = -1;
        private static int _obstacle = -1;

        // 절대 못 넘는 외벽
        public static LayerMask MapBoundary
        {
            get
            {
                if (_mapBoundary == -1)
                    _mapBoundary = LayerMask.GetMask("MapBoundary");
                return _mapBoundary;
            }
        }

        // 일반적으로는 못 넘지만 일부 스킬은 통과 가능
        public static LayerMask Obstacle
        {
            get
            {
                if (_obstacle == -1)
                    _obstacle = LayerMask.GetMask("Obstacle");
                return _obstacle;
            }
        }

        // 일반 이동/대부분의 스킬이 사용하는 전체 벽 마스크
        public static LayerMask Wall => MapBoundary | Obstacle;
    }
}
