using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTrail : MonoBehaviour
{
    public float activeTime = 0.2f;

    public float meshRefreshRate = 0.1f;
    public float meshDestroyDelay = 0.2f;
    public Transform positionToSpawn;

    public Material mat;

    bool isTrailActive = false;

    MeshRenderer skeletonMeshRenderer;
    MeshFilter skeletonMeshFilter;

    private void Start()
    {
        skeletonMeshRenderer = GetComponent<MeshRenderer>();
        skeletonMeshFilter = GetComponent<MeshFilter>();
    }

    private void Update()
    {
        isTrailActive = true;
        StartCoroutine(ActiveTrail(activeTime));
    }

    IEnumerator ActiveTrail(float timeActive)
    {
        while (timeActive > 0)
        {
            timeActive -= meshRefreshRate;

            GameObject temp = new GameObject();

            temp.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

            temp.transform.localScale = transform.localScale;

            MeshRenderer mr = temp.AddComponent<MeshRenderer>();

            temp.layer = skeletonMeshRenderer.gameObject.layer;

            mr.sortingLayerID = skeletonMeshRenderer.sortingLayerID;
            mr.sortingOrder = skeletonMeshRenderer.sortingOrder - 1;
            MeshFilter mf = temp.AddComponent<MeshFilter>();


            //temp.AddComponent<shadow_>

            mr.material = mat;

            Mesh mesh = new Mesh();
            mesh = skeletonMeshFilter.mesh;
            mf.mesh = mesh;

            Destroy(temp, meshDestroyDelay);

            yield return new WaitForSeconds(meshRefreshRate);
        }
    }
}
