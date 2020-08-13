using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TrailRenderer : MonoBehaviour
{

    [SerializeField] Material mat = null;
    MaterialPropertyBlock prop;

    [SerializeField, Range(.05f, .2f)] float trailRadius = .1f;

    Trail trail;
    // Start is called before the first frame update
    void Start()
    {
        trail = this.GetComponent<Trail>();
    }



    // Update is called once per frame
    void Update()
    {
        if(prop == null)  prop = new MaterialPropertyBlock();

        prop.SetInt("_VerticesCount", trail.VertCount);
        prop.SetInt("_TrailCount", trail.TrailCount);
        prop.SetFloat("_TrailRadius", trailRadius);
        prop.SetBuffer("_VerticesBuffer", trail.VerticesBuffer);

        Graphics.DrawProcedural(
            mat,
            new Bounds(transform.localPosition, transform.lossyScale * 20),
            MeshTopology.Points,
            trail.VertCount * trail.TrailCount, 1,
            null, prop, ShadowCastingMode.TwoSided,
            true, gameObject.layer
        );
        
    }
}
