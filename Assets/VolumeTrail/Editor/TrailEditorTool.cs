using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

[EditorTool("Trail Editor Tool", typeof(Trail))]
public class TrailEditorTool : EditorTool
{
    GUIContent cachedIcon;
    bool cc = false;
    
    // NOTE: as were caching this, unity will serialize it between compiles! so if we want to test out new looks,
    // just return the new GUIContent and bypass the cache until were happy with the icon...
    public override GUIContent toolbarIcon
    {
        get
        {
            if (cachedIcon == null)
                cachedIcon = EditorGUIUtility.IconContent("ViewToolOrbit", "|Trail Editor Tool");
            return cachedIcon; 
        }
    }
    
    public override void OnToolGUI(EditorWindow window)
    {
        foreach (var obj in Selection.gameObjects)
        {
            EditorGUI.BeginChangeCheck();

            var trail = obj.GetComponent<Trail>();
 
            var Epos = trail.EmitterPosition;
            var Tpos = trail.TargetPosition;
 
            Epos = Handles.PositionHandle(Epos, Quaternion.identity);
            Tpos = Handles.PositionHandle(Tpos, Quaternion.identity);

            Quaternion rot = trail.EmitterMatrix.rotation;
            float rad = trail.EmitterRadius*2;
            Handles.color = Color.white;
            float radiusE = Handles.RadiusHandle(rot, Epos, rad, true);
            float radiusT = Handles.RadiusHandle(rot, Tpos, rad, true);

            Handles.CircleHandleCap(1, Epos, rot, rad, EventType.Repaint);
            Handles.color = Color.magenta;
            Handles.CircleHandleCap(1, Tpos, rot, rad, EventType.Repaint);
 
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(obj, "Trail Transform Tool");
                trail.EmitterPosition = Epos;
                trail.TargetPosition = Tpos;

                Matrix4x4 m =  Matrix4x4.LookAt(Epos, Tpos, Vector3.up);
                trail.EmitterMatrix = m;

                if(radiusE != rad) trail.EmitterRadius = radiusE / 2;
                else trail.EmitterRadius = radiusT / 2;
            }
        }
    }
}