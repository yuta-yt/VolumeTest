using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class TrailManager : MonoBehaviour
{
    bool Edit = false;

    Vector3 EmitPosition;
    Vector3 TargetPosition;

    public GameObject TrailObj;

    void Update()
    {
        Vector2 p = Input.mousePosition;

        if(Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftControl))
        {
            editTrail(p);
        }

        if(Edit)
        {
            TargetPosition = SceneView.lastActiveSceneView.camera.ScreenToWorldPoint(new Vector3(p.x, p.y, 10f));
            Debug.DrawLine(EmitPosition, TargetPosition, Color.white, .01f, true);
        }
    }

    void editTrail(Vector2 mousePos){
        if(!Edit)
        {
            EmitPosition = SceneView.lastActiveSceneView.camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 2f));
        }
        else
        {
            Vector2 p = Input.mousePosition;
            TargetPosition = SceneView.lastActiveSceneView.camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));

            var instance = Instantiate(TrailObj);
            var trailComponent = instance.GetComponent<Trail>();

            instance.transform.parent = this.transform;
            trailComponent.EmitterPosition = EmitPosition;
            trailComponent.TargetPosition = TargetPosition;

            instance.transform.localPosition = new Vector3(0,0,0);
        }

        Edit = !Edit;
    }

}
