using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;

public class Trail : MonoBehaviour
{
    public Vector3 EmitterPosition;
    public Vector3 TargetPosition;

    public GameObject BakerObj;
    SDFBaker baker;
    ComputeBuffer _SDFBuffer;
    float cellSize;
    Vector3 cellCount;
    Vector3 gridCenter;

    [SerializeField, Range(.01f, 1f)] float Radius = .2f;
    [SerializeField, Range(30, 300)] int Count = 60; 

    [SerializeField, Range(60, 300)] int Iterate = 100;
    [SerializeField, Range(.5f, 10f)] float Length = 5f;
    [SerializeField, Range(0.2f, 3f)] float mixRange = .1f;

    [SerializeField, Range(.01f, 3f)] float noiseScale = 1f;
    [SerializeField] Vector3 noiseOffset;

    Vector3[] emitPts;
    Matrix4x4 emitterMat;

    ComputeBuffer _EmitterBuffer;
    ComputeBuffer _VerticesBuffer;

    [SerializeField] VisualEffect _VFX;
    RenderTexture _PositionMap;

    public ComputeShader cstrail;
    int kernelMain;

    public int TrailCount{
        get{ return Count; }
    }

    public int VertCount{
        get{ return Iterate; }
    }

    public float EmitterRadius{
        get{ return Radius; }
        set{ Radius = value; }
    }

    public ComputeBuffer VerticesBuffer{
        get{ return _VerticesBuffer; }
    }

    public Matrix4x4 EmitterMatrix{
        get{ return emitterMat; }
        set{ emitterMat = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        emitPts = new Vector3[Count];

        for(int i = 0; i < Count; i++){
            Vector2 dir = getND();
            emitPts[i] = new Vector3(dir.x, dir.y, 0) * Random.Range(0f, 1f);
        }

        emitterMat = Matrix4x4.LookAt(EmitterPosition, TargetPosition, Vector3.up);

        _EmitterBuffer = new ComputeBuffer(Count, Marshal.SizeOf(typeof(Vector3)) );
        _VerticesBuffer = new ComputeBuffer(Count * Iterate, Marshal.SizeOf(typeof(Vector3)));

        kernelMain = cstrail.FindKernel("CSTrailVert");

        baker = BakerObj.GetComponent<SDFBaker>();

        _PositionMap = new RenderTexture(Iterate, Count, 0, RenderTextureFormat.ARGBFloat);
        _PositionMap.enableRandomWrite = true;
        _PositionMap.Create();

        cstrail.SetTexture(kernelMain, "_PositionMap", _PositionMap);

        _VFX.SetTexture("PositionMap", _PositionMap);
        _VFX.SetUInt("SegmentCount", (uint)Iterate);
        _VFX.SetUInt("TrailCount", (uint)Count);
    }

    // Update is called once per frame
    void Update()
    {
        _SDFBuffer = baker.SDFBuffer;
        cellSize = baker.CellSize;
        cellCount = baker.CellCount;
        gridCenter = baker.center;

        Vector3[] pts = new Vector3[Count];

        for(int i = 0; i < Count; i++){
            pts[i] =  emitterMat.MultiplyPoint3x4(emitPts[i] * Radius);
        }

        _EmitterBuffer.SetData(pts);

        cstrail.SetInt("_Segment", Iterate);
        cstrail.SetInt("_TrailCount", Count);
        cstrail.SetFloat("_Step", Length / Iterate);
        cstrail.SetVector("_EmitDir", Vector3.Normalize(TargetPosition - EmitterPosition));
        cstrail.SetVector("_TargetPos", TargetPosition);
        cstrail.SetFloat("_Range", mixRange);

        cstrail.SetFloat("_noiseScale", noiseScale);
        cstrail.SetVector("_noiseOffset", noiseOffset);

        cstrail.SetFloat("_cellSize", cellSize);
        cstrail.SetVector("_cellCount", cellCount);
        cstrail.SetVector("_gridCenter", gridCenter);

        cstrail.SetBuffer(kernelMain, "_SDFBuffer", _SDFBuffer);

        cstrail.SetBuffer(kernelMain, "_EmitterBuffer", _EmitterBuffer);
        cstrail.SetBuffer(kernelMain, "_VerticesBuffer", _VerticesBuffer);

        cstrail.Dispatch(kernelMain, Mathf.CeilToInt(Count / 8f), 1, 1);
        _VFX.SetTexture("PositionMap", _PositionMap);
    }

    Vector2 getND(){
        float x = Random.Range(-1f, 1f);
        float y = Random.Range(-1f, 1f);
 
        float Z1 = Mathf.Sqrt(-2.0f * Mathf.Log(x)) * Mathf.Cos(2.0f * Mathf.PI * y);
        float Z2 = Mathf.Sqrt(-2.0f * Mathf.Log(x)) * Mathf.Sin(2.0f * Mathf.PI * y);

        return new Vector2(Z1, Z2);
    }
}
