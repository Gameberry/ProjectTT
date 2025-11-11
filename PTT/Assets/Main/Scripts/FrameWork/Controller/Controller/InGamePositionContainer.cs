using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    [System.Serializable]
    public class MovingDirectSkillContainer
    {
        public int SkillIndex;
        public List<Transform> MovePoint;
    }

    public class InGamePositionContainer : MonoSingleton<InGamePositionContainer>
    {
        [SerializeField]
        private List<BattleScenePositionSlot> _battleFriendPos = new List<BattleScenePositionSlot>();

        [SerializeField]
        private List<BattleScenePositionSlot> _battleFoeSpawnPos = new List<BattleScenePositionSlot>();

        [SerializeField]
        public Transform _arrrStadardPos;









        [SerializeField]
        private Transform m_monsterXPosMinPoint, m_monsterXPosMaxPoint, m_monsterYPosMinPoint, m_monsterYPosMaxPoint, m_bossMonsterPoint, m_traitBossRightPoint, m_traitBossLeftPoint, m_traitBossCenterPoint, m_runeDungeonRunePos, m_guildBossPos;

        [SerializeField]
        private Transform m_monsterLimitLine;

        [SerializeField]
        private Transform m_monsterLimitLine_L;

        [SerializeField]
        private Transform m_creatureLimitLine_Y;

        [SerializeField]
        private Transform m_creatureLimitUnderLine_Y;

        [SerializeField]
        private List<Transform> _monsterPosition_List = new List<Transform>();

        [SerializeField]
        private List<Transform> m_monsterPosition_Left_List = new List<Transform>();

        [SerializeField]
        public Transform m_playerBerserkerModePos;

        [SerializeField]
        private List<Transform> m_allyStadardPos;

        [SerializeField]
        private List<Transform> m_pvpAllyStadardPos;

        [SerializeField]
        private List<MovingDirectSkillContainer> m_movingDirectSkillContainer = new List<MovingDirectSkillContainer>();



        [Header("ForestHunterMovePoint")]
        [SerializeField]
        private List<Transform> m_forestHunterMovePoint;

        [Header("FlowerFairyMovePoint")]
        [SerializeField]
        private List<Transform> m_flowerFairyMovePoint;


        [Header("ShowMovePoint")]
        [SerializeField]
        private List<Transform> m_showMovePoint;

        [SerializeField]
        private bool m_drawLine = false;

        [SerializeField]
        private float m_drawLineZPos = -4.0f; // ∞Óº± «ÿªÛµµ

        [SerializeField]
        private int m_drawLineResolution = 10; // ∞Óº± «ÿªÛµµ

        private LineRenderer lineRenderer; // ∂Û¿Œ ∑ª¥ı∑Ø ∞¥√º

        //------------------------------------------------------------------------------------
        public void Update()
        {
            if (m_drawLine == true)
            {
                if (lineRenderer == null)
                {
                    lineRenderer = gameObject.AddComponent<LineRenderer>();
                }

                lineRenderer.positionCount = m_drawLineResolution + 1;
                lineRenderer.startWidth = 0.1f;
                lineRenderer.endWidth = 0.1f;

                for (int i = 0; i <= m_drawLineResolution; i++)
                {
                    float ti = (float)i / m_drawLineResolution;
                    Vector3 pos = Bezier.NOrderBezierInterp(m_showMovePoint, ti);
                    pos.z = m_drawLineZPos;
                    lineRenderer.SetPosition(i, pos);
                }
            }
        }
        //------------------------------------------------------------------------------------
        public List<BattleScenePositionSlot> GetBattleFriendPosList()
        {
            return _battleFriendPos;
        }
        //------------------------------------------------------------------------------------
        public void VisibleBattleFriendPos(bool visible)
        {
            for (int i = 0; i < _battleFriendPos.Count; ++i)
            {
                _battleFriendPos[i].gameObject.SetActive(visible);
            }
        }
        //------------------------------------------------------------------------------------
        public void UpdatedBattleFriendPos()
        {
            //for (int i = 0; i < _battleFriendPos.Count; ++i)
            //{
            //    _battleFriendPos[i].Updated();
            //}
        }
        //------------------------------------------------------------------------------------
        public BattleScenePositionSlot GetBattleFriendPos(int index)
        {
            int selectIdx = index - 1;

            if (_battleFriendPos.Count <= 0)
                return null;

            if (selectIdx < 0)
                return _battleFriendPos[0];
            else if (selectIdx >= _battleFriendPos.Count)
                return _battleFriendPos[_battleFriendPos.Count - 1];

            return _battleFriendPos[selectIdx];
        }
        //------------------------------------------------------------------------------------
        public int GetBattleFriendPosIndex(BattleScenePositionSlot battleScenePositionSlot)
        {
            return _battleFriendPos.IndexOf(battleScenePositionSlot);
        }
        //------------------------------------------------------------------------------------
        public Transform GetBattleFoeSpawnPos(int index)
        {
            int selectIdx = index - 1;

            BattleScenePositionSlot battleScenePositionSlot = null;

            if (_battleFoeSpawnPos.Count <= 0)
                return transform;

            if (selectIdx < 0)
                battleScenePositionSlot = _battleFoeSpawnPos[0];
            else if (selectIdx >= _battleFoeSpawnPos.Count)
                battleScenePositionSlot = _battleFoeSpawnPos[_battleFoeSpawnPos.Count - 1];
            else
                battleScenePositionSlot = _battleFoeSpawnPos[selectIdx];

            return battleScenePositionSlot.transform;
        }
        //------------------------------------------------------------------------------------
        public float GetMonsterXPosMinPoint()
        {
            return m_monsterXPosMinPoint.position.x;
        }
        //------------------------------------------------------------------------------------
        public float GetMonsterXPosMaxPoint()
        {
            return m_monsterXPosMaxPoint.position.x;
        }
        //------------------------------------------------------------------------------------
        public float GetMonsterYPosMinPoint()
        {
            return m_monsterYPosMinPoint.position.y;
        }
        //------------------------------------------------------------------------------------
        public float GetMonsterYPosMaxPoint()
        {
            return m_monsterYPosMaxPoint.position.y;
        }
        //------------------------------------------------------------------------------------
        public Transform GetStageBossPoint()
        {
            return m_bossMonsterPoint;
        }
        //------------------------------------------------------------------------------------
        public Transform GetTraitBossRightPoint()
        {
            return m_traitBossRightPoint;
        }
        //------------------------------------------------------------------------------------
        public Transform GetTraitBossLeftPoint()
        {
            return m_traitBossLeftPoint;
        }
        //------------------------------------------------------------------------------------
        public Transform GetTraitBossCenterPoint()
        {
            return m_traitBossCenterPoint;
        }
        //------------------------------------------------------------------------------------
        public Transform GetRuneDungeonRunePoint()
        {
            return m_runeDungeonRunePos;
        }
        //------------------------------------------------------------------------------------
        public Transform GetGuildBossPoint()
        {
            return m_guildBossPos;
        }
        //------------------------------------------------------------------------------------
        public Transform GetMonsterLimitLine()
        {
            return m_monsterLimitLine;
        }
        //------------------------------------------------------------------------------------
        public Transform GetMonsterLimitLine_L()
        {
            return m_monsterLimitLine_L;
        }
        //------------------------------------------------------------------------------------
        public Transform GetCreatureLimitLine_Y()
        {
            return m_creatureLimitLine_Y;
        }
        //------------------------------------------------------------------------------------
        public Transform GetCreatureLimitUnderLine_Y()
        {
            return m_creatureLimitUnderLine_Y;
        }
        //------------------------------------------------------------------------------------
        public Transform GetMonsterWavePos(int index)
        {
            int selectIdx = index - 1;

            if (_monsterPosition_List.Count <= 0)
                return m_bossMonsterPoint;

            if (selectIdx < 0)
                return _monsterPosition_List[0];
            else if (selectIdx >= _monsterPosition_List.Count)
                return _monsterPosition_List[_monsterPosition_List.Count - 1];

            return _monsterPosition_List[selectIdx];
        }
        //------------------------------------------------------------------------------------
        public Vector3 GetMonsterWavePos_L(int index)
        {
            int selectIdx = index - 1;

            if (m_monsterPosition_Left_List.Count <= 0)
                return m_bossMonsterPoint.position;

            if (selectIdx < 0)
                return m_monsterPosition_Left_List[0].position;
            else if (selectIdx >= m_monsterPosition_Left_List.Count)
                return m_monsterPosition_Left_List[m_monsterPosition_Left_List.Count - 1].position;

            return m_monsterPosition_Left_List[selectIdx].position;
        }
        //------------------------------------------------------------------------------------
        public Transform GetAllyPos(int idx)
        {
            if (idx < 0)
                return null;

            return m_allyStadardPos[idx - 1];
        }
        //------------------------------------------------------------------------------------
        public int GetAllyPosCount()
        {
            return m_allyStadardPos.Count;
        }
        //------------------------------------------------------------------------------------
        public Transform GetPvPAllyPos(int idx)
        {
            if (idx < 0)
                return null;

            return m_pvpAllyStadardPos[idx - 1];
        }
        //------------------------------------------------------------------------------------
        public Transform GetArrrStadardPos()
        {
            return _arrrStadardPos;
        }
        //------------------------------------------------------------------------------------
        public Transform GetPlayerBerserkerModePos()
        {
            return m_playerBerserkerModePos;
        }
        //------------------------------------------------------------------------------------
        public List<Transform> GetMovingDirectSkillPos(int skillindex)
        {
            MovingDirectSkillContainer movingDirectSkillContainer = m_movingDirectSkillContainer.Find(x => x.SkillIndex == skillindex);
            if (movingDirectSkillContainer == null)
                return new List<Transform>();

            return movingDirectSkillContainer.MovePoint;
        }
        //------------------------------------------------------------------------------------
        public List<Transform> GetFlowerFairyMovePos()
        {
            return m_flowerFairyMovePoint;
        }
        //------------------------------------------------------------------------------------
        public List<Transform> GetForestHunterMovePos()
        {
            return m_forestHunterMovePoint;
        }
        //------------------------------------------------------------------------------------
    }
}