using UnityEngine;

namespace GameBerry.TestScene
{
    [CreateAssetMenu(fileName = "SectorHitSkillData", menuName = "GameBerry/Test Scene/Skills/Sector Hit Skill")]
    public class TestSectorHitSkillData : TestSkillData
    {
        public override bool ExecuteHit(TestSkillExecutionContext ctx)
        {
            HitMonstersInSector(ctx, Damage, Range, Angle);
            return true;
        }
    }
}
