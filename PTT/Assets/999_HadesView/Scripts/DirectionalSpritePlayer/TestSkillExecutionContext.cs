using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.TestScene
{
    public class TestSkillExecutionContext
    {
        public TestDirectionalPlayerController PlayerController { get; }
        public TestDirectionalAnimator SpriteAnimator { get; }
        public HashSet<TestDirectionalMonsterController> HitBuffer { get; }

        public TestDirectionalMonsterController LockedTarget { get; set; }
        public Vector3 SkillDirection { get; set; }
        public bool IsTriggered { get; set; }

        // 스킬이 매 프레임 처리할 작업을 등록. true 반환 = 계속 진행, false = 완료.
        public Func<TestSkillExecutionContext, bool> TickAction { get; set; }

        public Vector3 LastGizmoStart { get; private set; }
        public Vector3 LastGizmoEnd { get; private set; }
        public float LastGizmoRadius { get; private set; }
        public float LastGizmoExpireTime { get; private set; }

        public TestSkillExecutionContext(
            TestDirectionalPlayerController playerController,
            TestDirectionalAnimator spriteAnimator,
            HashSet<TestDirectionalMonsterController> hitBuffer)
        {
            PlayerController = playerController;
            SpriteAnimator = spriteAnimator;
            HitBuffer = hitBuffer;
        }

        public void CacheGizmo(Vector3 start, Vector3 end, float radius, float duration)
        {
            LastGizmoStart = start;
            LastGizmoEnd = end;
            LastGizmoRadius = radius;
            LastGizmoExpireTime = Time.time + Mathf.Max(0f, duration);
        }

        public void Reset()
        {
            LockedTarget = null;
            SkillDirection = Vector3.down;
            IsTriggered = false;
            TickAction = null;
            HitBuffer.Clear();
        }
    }
}
