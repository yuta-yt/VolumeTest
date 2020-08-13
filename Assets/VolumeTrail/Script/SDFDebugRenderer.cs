using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDFDebugRenderer : MonoBehaviour
{
    public GameObject Target;
    SDFBaker baker;

    ComputeBuffer _SDFBuffer;

    float cellSize;
    Vector3 cellCount;
    Vector3 gridCenter;

    public Material mat;
    // Start is called before the first frame update
    void Start()
    {
        baker = Target.GetComponent<SDFBaker>();
    }

    // Update is called once per frame
    void Update()
    {
        _SDFBuffer = baker.SDFBuffer;
        
        cellSize = baker.CellSize;
        cellCount = baker.CellCount;
        gridCenter = baker.center;

        mat.SetBuffer("_SDFBuffer", _SDFBuffer);
        mat.SetFloat("_cellSize", cellSize);
        mat.SetVector("_cellCount", cellCount);
        mat.SetVector("_gridCenter", gridCenter);
    }
}
