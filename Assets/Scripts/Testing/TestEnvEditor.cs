using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestEnvScript))]
[CanEditMultipleObjects]
public class TestEnvEditor : Editor
{
    private ChunkManager cm;
    private bool editEnabled;
    private bool editMode;
    private bool buttonDown;
    private float size;
    private float strength;
    void OnEnable()
    {
        cm = target as ChunkManager;
        editEnabled = false;
        editMode = false;
        size = 5f;
        strength = 1f;
    }


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Save Chunks")) cm.SaveMemory();
        if (GUILayout.Button("Delete Chunks")) cm.DeleteMemory();
    }
    void OnSceneGUI()
    {
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Q) buttonDown = true;
        if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Q) buttonDown = false;
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 20, 150, 200), "Chunk Editor");
        if (GUILayout.Button("Edit Chunks", GUILayout.Width(100))) editEnabled = !editEnabled;
        if (editEnabled && GUILayout.Button(editMode ? "Add" : "Subtract", GUILayout.Width(100))) editMode = !editMode;
        if (editEnabled) size = GUILayout.HorizontalSlider(size, 1f, 10f);
        if (editEnabled) strength = GUILayout.HorizontalSlider(strength, 1f, 10f);
        GUILayout.EndArea();
        Handles.EndGUI();
        if (editEnabled && Camera.current != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out hit, 100f))
            {
                Handles.color = editMode ? Color.blue : Color.red;
                if (buttonDown) Handles.SphereHandleCap(0, hit.point, Quaternion.identity, size * 2f, EventType.Repaint);
                else Handles.DrawWireCube(hit.point, Vector3.one * size * 2f);

                if (buttonDown)
                    if (editMode) cm.Add(hit.point, size, strength * .2f);
                    else cm.Subtract(hit.point, size, strength * .2f);
            }
        }
    }
}