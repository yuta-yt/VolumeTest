using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class SDFBaker : MonoBehaviour
{
    public ComputeShader CS;
    int initKernel;
    int bakeKernel;
    int finalizeKernel;
    int groupSize;

    MeshFilter mf;

    ComputeBuffer _PositionBuffer;
    ComputeBuffer _IndicesBuffer;

    Vector3 Center;
    public Vector3 center{
        get{ return Center; }
    }
    Vector3 meshSize;
    Vector3 cellCount;
    public Vector3 CellCount{
        get{ return cellCount; }
    }
    float cellSize;
    public float CellSize{
        get{ return cellSize; }
    }

    Vector3 gridSize;

    [SerializeField, Range(.01f, .5f)] float targetCellSize = .05f;
    [SerializeField, Range(1f, 5f)] float marginScale = 1.5f;

    ComputeBuffer _SDFBuffer;
    public ComputeBuffer SDFBuffer{
        get{ return _SDFBuffer; }
    }

    void Start()
    {
        mf = GetComponent<MeshFilter>();
        
        // init some mesh data
        int vertCount = mf.mesh.vertexCount;
        Vector3[] meshPositions = mf.mesh.vertices;
        _PositionBuffer = new ComputeBuffer(vertCount, Marshal.SizeOf(typeof(Vector3)));
        _PositionBuffer.SetData(meshPositions);

        int indexCount = (int)mf.mesh.GetIndexCount(0);
        int[] meshIndices = mf.mesh.GetIndices(0);
        _IndicesBuffer = new ComputeBuffer(indexCount, sizeof(int));
        _IndicesBuffer.SetData(meshIndices);

        // init coumpute kernel
        initKernel = CS.FindKernel("InitSDF");
        bakeKernel = CS.FindKernel("BakeSDF");
        finalizeKernel = CS.FindKernel("FinalizeSDF");

        CS.SetBuffer(bakeKernel, "_PositionBuffer", _PositionBuffer);
        CS.SetBuffer(bakeKernel, "_IndicesBuffer", _IndicesBuffer);

        initGridParam();
        Debug.Log(cellCount);
        Debug.Log(cellSize);
    }

    void Update()
    {
        initGridParam();

        CS.SetBuffer(bakeKernel, "_SDFBuffer", _SDFBuffer);
        CS.Dispatch(bakeKernel, Mathf.CeilToInt((int)mf.mesh.GetIndexCount(0) / 3 / (float)64), 1, 1);

        CS.SetBuffer(finalizeKernel, "_SDFBuffer", _SDFBuffer);
        CS.Dispatch(finalizeKernel, Mathf.CeilToInt(cellCount.x*cellCount.y*cellCount.z / (float)64), 1, 1);
    }

    void OnDestroy(){
        if(_PositionBuffer != null) _PositionBuffer.Release();
        if(_IndicesBuffer != null) _IndicesBuffer.Release();
        if(_SDFBuffer != null) _SDFBuffer.Release();
    }


    void initGridParam(){
        var meshBounds = mf.mesh.bounds;
        Center =  transform.localToWorldMatrix.MultiplyPoint3x4(meshBounds.center);
        meshSize = Vector3.Scale(meshBounds.size * marginScale, transform.lossyScale);

        Center -= meshSize / 2;

        cellCount = new Vector3(Mathf.Ceil(meshSize.x / targetCellSize) + 1,
                                Mathf.Ceil(meshSize.y / targetCellSize) + 1,
                                Mathf.Ceil(meshSize.z / targetCellSize) + 1);

        Vector3 csize = new Vector3(meshSize.x / (cellCount.x - 1),
                                    meshSize.y / (cellCount.y - 1),
                                    meshSize.z / (cellCount.z - 1));

        cellSize = Mathf.Max(csize.x, Mathf.Max(csize.y, csize.z));
        gridSize = new Vector3(cellCount.x*cellSize, cellCount.y*cellSize, cellCount.z*cellSize);

        int numCells = (int)cellCount.x * (int)cellCount.y * (int)cellCount.z;

        _SDFBuffer = new ComputeBuffer(numCells, sizeof(uint));

        CS.SetBuffer(initKernel, "_SDFBuffer", _SDFBuffer);
        CS.SetVector("_gridSize", gridSize);
        CS.SetVector("_gridCenter", Center);
        CS.SetVector("_cellCount", cellCount);
        CS.SetFloat("_cellSize", cellSize);
        CS.SetMatrix("_tW", transform.localToWorldMatrix.transpose);

        groupSize = Mathf.CeilToInt(numCells / (float)64);
        CS.Dispatch(initKernel, groupSize, 1, 1);
    }
}
