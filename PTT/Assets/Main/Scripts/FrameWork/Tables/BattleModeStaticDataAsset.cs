using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Spine;
using Spine.Unity;

namespace GameBerry
{
    [CreateAssetMenu(fileName = "BattleModeStaticData", menuName = "Table/BattleModeStaticData", order = 1)]
    public class BattleModeStaticDataAsset : ScriptableObject
    {
        [Header("-----------TestStage-----------")]
        public List<MonsterController> MonsterPrefab = new List<MonsterController>();
        public int SpawnCount = 30;

        [Header("-----------Stage-----------")]
        public float Stage_StartXPos = 0.0f;
        public double Stage_NexusCreateDistance = 5.0f;

        public float Stage_WaveSpawnTurm = 0.5f;
        public float Stage_WaveSpawnRatio = 0.7f;
        public Vector3 Stage_WaveSpawnPos;
        public float Stage_WaveCreateDistance = 3.0f;

        public float Stage_AddEndLine = 3.0f;

        [Header("-----------Pet-----------")]
        public float PetMoveSpeed = 4.0f;

        public float PetRadius = 1.5f;
        public float PetStartAngle = 125f; // Look 방향의 시작 각도
        public float PetSectorAngle = 115f;
    }
}