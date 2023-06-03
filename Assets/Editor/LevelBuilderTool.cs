using UnityEngine;
using UnityEditor;

public class LevelBuilderTool : EditorWindow
{
    private GameObject cubePrefab;
    private float gridSizeMultiplier = 1f;
    private float planeHeight = 0f; // Desired Y-axis position for the cubes

    private Color cubeColor = Color.white;

    private bool isMouseDown = false;
    private Vector3 previousMousePosition;

    [MenuItem("Tools/Level Builder")]
    public static void ShowWindow()
    {
        LevelBuilderTool window = GetWindow<LevelBuilderTool>();
        window.titleContent = new GUIContent("Level Builder");
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Level Builder Tool", EditorStyles.boldLabel);
        cubePrefab = EditorGUILayout.ObjectField("Cube Prefab", cubePrefab, typeof(GameObject), false) as GameObject;
        gridSizeMultiplier = EditorGUILayout.FloatField("Grid Size Multiplier", gridSizeMultiplier);
        planeHeight = EditorGUILayout.FloatField("Plane Height", planeHeight); // Input for the desired Y-axis position
        cubeColor = EditorGUILayout.ColorField("Cube Color", cubeColor);

        if (GUILayout.Button("Place Cubes"))
        {
            PlaceCubes();
        }

        if (GUILayout.Button("Replace Cubes"))
        {
            ReplaceCubes();
        }
    }

    private void PlaceCubes()
    {
        if (cubePrefab == null)
        {
            Debug.LogError("Cube Prefab is not assigned.");
            return;
        }

        for (int x = 0; x < gridSizeMultiplier; x++)
        {
            for (int z = 0; z < gridSizeMultiplier; z++)
            {
                Vector3 position = GetSnappedPosition(GetMouseWorldPosition()) + new Vector3(x, planeHeight, z); // Set Y-axis position to planeHeight
                GameObject cube = PrefabUtility.InstantiatePrefab(cubePrefab) as GameObject;
                cube.transform.position = position;
                cube.name = "Cube";

                Renderer cubeRenderer = cube.GetComponent<Renderer>();
                if (cubeRenderer != null)
                {
                    cubeRenderer.sharedMaterial.color = cubeColor;
                }

                Undo.RegisterCreatedObjectUndo(cube, "Place Cubes");
            }
        }
    }

    private void ReplaceCubes()
    {
        GameObject[] cubes = GameObject.FindGameObjectsWithTag("Cube");

        foreach (GameObject cube in cubes)
        {
            Renderer cubeRenderer = cube.GetComponent<Renderer>();
            if (cubeRenderer != null)
            {
                cubeRenderer.sharedMaterial.color = cubeColor;
            }
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.point;
        }
        return Vector3.zero;
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (Event.current == null)
        {
            return;
        }

        if (cubePrefab == null)
        {
            return;
        }

        Event currentEvent = Event.current;

        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                if (currentEvent.button == 0)
                {
                    isMouseDown = true;
                    previousMousePosition = GetMouseWorldPosition();
                }
                break;
            case EventType.MouseDrag:
                if (isMouseDown && currentEvent.button == 0)
                {
                    Vector3 currentMousePosition = GetMouseWorldPosition();
                    Vector3 delta = currentMousePosition - previousMousePosition;

                    if (Mathf.Abs(delta.x) >= gridSizeMultiplier || Mathf.Abs(delta.z) >= gridSizeMultiplier)
                    {
                        int deltaX = Mathf.RoundToInt(delta.x / gridSizeMultiplier);
                        int deltaZ = Mathf.RoundToInt(delta.z / gridSizeMultiplier);

                        gridSizeMultiplier = Mathf.Max(1, Mathf.Abs(deltaX)) + 1;

                        if (deltaX < 0)
                        {
                            previousMousePosition.x += gridSizeMultiplier * deltaX;
                        }

                        if (deltaZ < 0)
                        {
                            previousMousePosition.z += gridSizeMultiplier * deltaZ;
                        }

                        previousMousePosition = currentMousePosition;
                        PlaceCubes();
                    }
                }
                break;
            case EventType.MouseUp:
                if (currentEvent.button == 0)
                {
                    isMouseDown = false;
                }
                break;
            case EventType.Repaint: // Add this case to ensure it's only called during repaint event
                if (isMouseDown && currentEvent.button == 0)
                {
                    Vector3 mouseWorldPosition = GetMouseWorldPosition();
                    Handles.color = Color.red;
                    Handles.DrawWireCube(mouseWorldPosition, Vector3.one * gridSizeMultiplier);
                    SceneView.RepaintAll();
                }
                break;
        }
    }

    private Vector3 GetSnappedPosition(Vector3 position)
    {
        Vector3 snappedPosition = position;
        snappedPosition.x = Mathf.Round(snappedPosition.x);
        snappedPosition.z = Mathf.Round(snappedPosition.z);
        return snappedPosition;
    }
}
