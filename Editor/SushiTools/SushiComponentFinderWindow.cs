using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SushiTools
{
    public class SushiComponentFinderWindow : EditorWindow
    {
        private string searchText = "";
        private Vector2 scrollPosition;
        private bool potatoMode;

        // Components currently visible in the window.
        private readonly List<SushiComponentFinderResult> results = new();

        // Components checked by the user.
        // This survives Search(), ListAll(), and Refresh().
        private readonly HashSet<Component> checkedComponents = new();

        [MenuItem("Tools/Sushi Tools/Component Finder")]
        public static void ShowWindow()
        {
            GetWindow<SushiComponentFinderWindow>(
                "Sushi Component Finder"
            );
        }

        private void OnGUI()
        {
            DrawSearchBar();
            DrawToolbar();
            DrawResults();
        }

        private void DrawSearchBar()
        {
            EditorGUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();

            string newSearch = EditorGUILayout.TextField(
                "Component",
                searchText
            );

            if (newSearch != searchText)
            {
                searchText = newSearch;

                if (!potatoMode)
                {
                    Search();
                }
            }

            if (GUILayout.Button(
                "Search",
                GUILayout.Width(70)
            ))
            {
                Search();
            }

            if (GUILayout.Button(
                "Exact",
                GUILayout.Width(70)
            ))
            {
                SearchExact();
            }

            potatoMode = EditorGUILayout.ToggleLeft(
                 "Wasabi Mode",
                 potatoMode,
                 GUILayout.Width(100)
             );


            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("List All"))
            {
                ListAll();
            }

            GUI.enabled = checkedComponents.Count > 0;

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
                $"Results: {results.Count} | " +
                $"Checked: {checkedComponents.Count}",
                EditorStyles.boldLabel
            );

            EditorGUILayout.Space(3);
        }

        private void DrawResults()
        {
            scrollPosition =
                EditorGUILayout.BeginScrollView(
                    scrollPosition
                );

            foreach (SushiComponentFinderResult result in results)
            {
                DrawResult(result);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawResult(
      SushiComponentFinderResult result
  )
        {
            if (result.component == null ||
                result.gameObject == null)
            {
                return;
            }

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();

            // Read the check state from the global HashSet.
            bool isChecked =
                checkedComponents.Contains(
                    result.component
                );

            bool newChecked =
                EditorGUILayout.Toggle(
                    isChecked,
                    GUILayout.Width(20)
                );

            // Update the global selection.
            if (newChecked != isChecked)
            {
                if (newChecked)
                {
                    checkedComponents.Add(
                        result.component
                    );
                }
                else
                {
                    checkedComponents.Remove(
                        result.component
                    );
                }
            }

            // Component icon
            Texture icon =
                AssetPreview.GetMiniTypeThumbnail(
                    result.component.GetType()
                );

            if (icon != null)
            {
                GUILayout.Label(
                    icon,
                    GUILayout.Width(16),
                    GUILayout.Height(16)
                );
            }

            // Component name
            // if u dont want to drag-select
            //if (GUILayout.Button(
            //    result.component.GetType().Name,
            //    EditorStyles.boldLabel
            //))
            //{
            //    SushiComponentFinderUtility.SelectGameObject(
            //        result.gameObject
            //    );
            //}
            // make the component name selectable with the mouse so you can drag-select and copy the text.
            EditorGUILayout.SelectableLabel(
                result.component.GetType().Name,
                EditorStyles.boldLabel,
                GUILayout.Height(EditorGUIUtility.singleLineHeight)
            );

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            // Align path with component name.
            GUILayout.Space(20 + 16 + 4);

            GUIStyle pathStyle = new(
                EditorStyles.miniLabel
            )
            {
                wordWrap = true
            };

            if (GUILayout.Button(
                result.path,
                pathStyle
            ))
            {
                SushiComponentFinderUtility.SelectGameObject(
                    result.gameObject
                );
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void SearchComponents(bool exact)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                ListAll();
                return;
            }

            results.Clear();

            if (exact)
            {
                results.AddRange(
                    SushiComponentFinderSearch.FindExact(
                        searchText
                    )
                );
            }
            else
            {
                results.AddRange(
                    SushiComponentFinderSearch.Find(
                        searchText
                    )
                );
            }

            Repaint();
        }

        private void Search()
        {
            SearchComponents(exact: false);
        }

        private void SearchExact()
        {

            SearchComponents(exact: true);
        }

        private void ListAll()
        {
            // Only replace what is visible.
            // checkedComponents is NOT touched.
            results.Clear();

            results.AddRange(
                SushiComponentFinderSearch.FindAll()
            );

            Repaint();
        }

        private void SelectChecked()
        {
            HashSet<GameObject> gameObjects = new();

            foreach (Component component in checkedComponents)
            {
                if (component == null)
                {
                    continue;
                }

                GameObject obj = component.gameObject;

                if (obj != null)
                {
                    gameObjects.Add(obj);
                }
            }

            Selection.objects =
                new List<GameObject>(
                    gameObjects
                ).ToArray();
        }

        private void RemoveChecked()
        {
            // Clean destroyed components out of the selection.
            checkedComponents.RemoveWhere(
                component => component == null
            );

            if (checkedComponents.Count == 0)
            {
                return;
            }

            int choice = EditorUtility.DisplayDialogComplex(
                "Remove Checked",

                BuildCheckedListMessage(),

                "Remove Components",
                "Cancel",
                "Remove GameObjects"
            );

            // Cancel
            if (choice == 1)
            {
                return;
            }

            int undoGroup = Undo.GetCurrentGroup();

            Undo.SetCurrentGroupName(
                "Sushi Component Finder Removal"
            );

            if (choice == 0)
            {
                RemoveCheckedComponents();
            }
            else if (choice == 2)
            {
                RemoveCheckedGameObjects();
            }

            Undo.CollapseUndoOperations(
                undoGroup
            );

            // The selected components no longer exist.
            checkedComponents.Clear();

            Search();
        }

        private string BuildCheckedListMessage()
        {
            List<string> lines = new();

            foreach (Component component in checkedComponents)
            {
                if (component == null)
                {
                    continue;
                }

                GameObject obj = component.gameObject;

                if (obj == null)
                {
                    continue;
                }

                string path =
                    SushiComponentFinderUtility
                        .GetGameObjectPath(obj);

                lines.Add(
                    $"{component.GetType().Name}\n" +
                    $"  {path}"
                );
            }

            return
                $"You have checked " +
                $"{lines.Count} component(s):\n\n" +
                $"{string.Join("\n\n", lines)}\n\n" +
                "What do you want to remove?";
        }

        private void RemoveCheckedComponents()
        {
            foreach (Component component in checkedComponents)
            {
                if (component == null)
                {
                    continue;
                }

                Undo.DestroyObjectImmediate(
                    component
                );
            }
        }

        private void RemoveCheckedGameObjects()
        {
            HashSet<GameObject> gameObjects = new();

            foreach (Component component in checkedComponents)
            {
                if (component == null)
                {
                    continue;
                }

                GameObject obj = component.gameObject;

                if (obj != null)
                {
                    gameObjects.Add(obj);
                }
            }

            foreach (GameObject obj in gameObjects)
            {
                Undo.DestroyObjectImmediate(
                    obj
                );
            }
        }
    }
}