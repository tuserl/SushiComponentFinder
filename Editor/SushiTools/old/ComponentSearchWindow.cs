using SushiTools;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ComponentSearchWindow : EditorWindow
{
    private string searchText = "";
    private Vector2 scrollPosition;

    private readonly List<ComponentResult> results = new();

    [MenuItem("Tools/Sushi Tools/(OLD) Component Finder")]
    public static void ShowWindow()
    {
        GetWindow<ComponentSearchWindow>("Component Search");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8);

        // Search field
        EditorGUILayout.BeginHorizontal();

        string newSearch = EditorGUILayout.TextField(
            "Component",
            searchText
        );

        if (newSearch != searchText)
        {
            searchText = newSearch;
            Search();
        }

        if (GUILayout.Button("Search", GUILayout.Width(70)))
        {
            Search();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // Buttons
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Refresh"))
        {
            Search();
        }

  

        GUI.enabled = results.Count > 0;

        if (GUILayout.Button("Select Checked"))
        {
            SelectChecked();
        }

        if (GUILayout.Button("Remove Checked"))
        {
            RemoveChecked();
        }

        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField(
            $"Results: {results.Count}",
            EditorStyles.boldLabel
        );

        EditorGUILayout.Space(3);

        // Results
        scrollPosition = EditorGUILayout.BeginScrollView(
            scrollPosition
        );

        foreach (ComponentResult result in results)
        {
            DrawResult(result);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawResult(ComponentResult result)
    {
        if (result.component == null ||
            result.gameObject == null)
        {
            return;
        }

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();

        // Checkbox
        result.checkedForRemoval =
            EditorGUILayout.Toggle(
                result.checkedForRemoval,
                GUILayout.Width(20)
            );

        // Component name
        if (GUILayout.Button(
            result.component.GetType().Name,
            EditorStyles.boldLabel
        ))
        {
            SelectGameObject(result.gameObject);
        }

        EditorGUILayout.EndHorizontal();

        // GameObject path
        EditorGUILayout.BeginHorizontal();

        GUILayout.Space(25);

        GUIStyle pathStyle = new GUIStyle(
            EditorStyles.miniLabel
        );

        pathStyle.wordWrap = true;

        if (GUILayout.Button(
            result.path,
            pathStyle
        ))
        {
            SelectGameObject(result.gameObject);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void SelectGameObject(GameObject obj)
    {
        Selection.activeGameObject = obj;

        EditorGUIUtility.PingObject(obj);
    }

    private void Search()
    {
        results.Clear();

        string query = searchText.Trim();

        if (string.IsNullOrEmpty(query))
        {
            Repaint();
            return;
        }

        query = query.ToLowerInvariant();

        GameObject[] allGameObjects =
            UnityEngine.Object.FindObjectsByType<GameObject>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (GameObject obj in allGameObjects)
        {
            if (obj == null)
                continue;

            // Only search actual scene objects.
            if (EditorUtility.IsPersistent(obj))
                continue;

            if (!obj.scene.IsValid())
                continue;

            Component[] components =
                obj.GetComponents<Component>();

            foreach (Component component in components)
            {
                if (component == null)
                    continue;

                string componentName =
                    component.GetType().Name;

                if (!componentName.Contains(
                    query,
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                results.Add(new ComponentResult
                {
                    gameObject = obj,
                    component = component,
                    path = GetGameObjectPath(obj),
                    checkedForRemoval = false
                });
            }
        }

        Repaint();
    }

   
    private string GetGameObjectPath(GameObject obj)
    {
        List<string> hierarchy = new();

        Transform current = obj.transform;

        while (current != null)
        {
            hierarchy.Add(current.name);
            current = current.parent;
        }

        hierarchy.Reverse();

        return string.Join(
            " / ",
            hierarchy
        );
    }

    private void SelectChecked()
    {
        List<UnityEngine.Object> objects = new();

        foreach (ComponentResult result in results)
        {
            if (result.checkedForRemoval &&
                result.gameObject != null)
            {
                objects.Add(result.gameObject);
            }
        }

        Selection.objects = objects.ToArray();
    }

    private void RemoveChecked()
    {
        List<ComponentResult> checkedResults =
            results.FindAll(
                r =>
                    r.checkedForRemoval &&
                    r.component != null &&
                    r.gameObject != null
            );

        if (checkedResults.Count == 0)
            return;

        int choice = EditorUtility.DisplayDialogComplex(
            "Remove Components",

            $"You selected {checkedResults.Count} component(s).\n\n" +
            "What do you want to remove?",

            "Remove Components",
            "Cancel",
            "Remove GameObjects"
        );

        // Cancel
        if (choice == 1)
            return;

        int undoGroup = Undo.GetCurrentGroup();

        Undo.SetCurrentGroupName(
            "Component Search Removal"
        );

        if (choice == 0)
        {
            // Remove only the components.
            foreach (ComponentResult result in checkedResults)
            {
                if (result.component != null)
                {
                    Undo.DestroyObjectImmediate(
                        result.component
                    );
                }
            }
        }
        else if (choice == 2)
        {
            // Remove entire GameObjects.
            HashSet<GameObject> gameObjects = new();

            foreach (ComponentResult result in checkedResults)
            {
                if (result.gameObject != null)
                {
                    gameObjects.Add(
                        result.gameObject
                    );
                }
            }

            foreach (GameObject obj in gameObjects)
            {
                Undo.DestroyObjectImmediate(obj);
            }
        }

        Undo.CollapseUndoOperations(undoGroup);

        Search();
    }

    private class ComponentResult
    {
        public GameObject gameObject;
        public Component component;
        public string path;
        public bool checkedForRemoval;
    }
}
