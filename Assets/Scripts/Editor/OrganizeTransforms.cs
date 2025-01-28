using UnityEngine;
using UnityEditor;

public class GridOrganizer : EditorWindow
{
    private Transform parentTransform;
    private float width = 1.0f;

    [MenuItem("Tools/Grid Organizer")]
    public static void ShowWindow()
    {
        GetWindow<GridOrganizer>("Grid Organizer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Grid Organizer", EditorStyles.boldLabel);

        parentTransform = (Transform)EditorGUILayout.ObjectField("Parent Transform", parentTransform, typeof(Transform), true);
        width = EditorGUILayout.FloatField("Width", width);

        if (GUILayout.Button("Organize in Grid"))
        {
            if (parentTransform == null)
            {
                Debug.LogError("Please assign a parent transform.");
                return;
            }

            OrganizeGrid();
        }
    }

    private void OrganizeGrid()
    {
        int childCount = parentTransform.childCount;

        if (childCount == 0)
        {
            Debug.LogWarning("No children to organize.");
            return;
        }

        int rows = Mathf.CeilToInt(Mathf.Sqrt(childCount));
        int columns = Mathf.CeilToInt((float)childCount / rows);

        for (int i = 0; i < childCount; i++)
        {
            Transform child = parentTransform.GetChild(i);

            int row = i / columns;
            int column = i % columns;

            Vector3 newPosition = new Vector3(column * width, 0, row * width);
            child.localPosition = newPosition;
        }

        Debug.Log("Transforms organized in a grid pattern.");
    }
}
