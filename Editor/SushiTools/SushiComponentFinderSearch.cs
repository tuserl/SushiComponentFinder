using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SushiTools
{
    public static class SushiComponentFinderSearch
    {
        public static List<SushiComponentFinderResult> FindExact(
    string searchText
)
        {
            List<SushiComponentFinderResult> results = new();

            string query = searchText.Trim();

            if (string.IsNullOrEmpty(query))
            {
                return results;
            }

            GameObject[] allGameObjects =
                UnityEngine.Object.FindObjectsByType<GameObject>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            foreach (GameObject obj in allGameObjects)
            {
                if (!IsValidSceneObject(obj))
                {
                    continue;
                }

                Component[] components =
                    obj.GetComponents<Component>();

                foreach (Component component in components)
                {
                    if (component == null)
                    {
                        continue;
                    }

                    string componentName =
                        component.GetType().Name;

                    if (componentName != query)
                    {
                        continue;
                    }

                    results.Add(
                        new SushiComponentFinderResult
                        {
                            gameObject = obj,
                            component = component,
                            path =
                                SushiComponentFinderUtility
                                    .GetGameObjectPath(obj)
                        }
                    );
                }
            }

            return results;
        }
        public static List<SushiComponentFinderResult> Find(
            string searchText
        )
        {
            List<SushiComponentFinderResult> results = new();

            string query = searchText.Trim();

            if (string.IsNullOrEmpty(query))
            {
                return results;
            }

            GameObject[] allGameObjects =
                UnityEngine.Object.FindObjectsByType<GameObject>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            foreach (GameObject obj in allGameObjects)
            {
                if (!IsValidSceneObject(obj))
                {
                    continue;
                }

                Component[] components =
                    obj.GetComponents<Component>();

                foreach (Component component in components)
                {
                    if (component == null)
                    {
                        continue;
                    }

                    string componentName =
                        component.GetType().Name;

                    if (!componentName.Contains(
                        query,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    results.Add(
                        new SushiComponentFinderResult
                        {
                            gameObject = obj,
                            component = component,
                            path =
                                SushiComponentFinderUtility
                                    .GetGameObjectPath(obj),
                        }
                    );
                }
            }

            return results;
        }

        public static List<SushiComponentFinderResult> FindAll()
        {
            List<SushiComponentFinderResult> results = new();

            GameObject[] allGameObjects =
                UnityEngine.Object.FindObjectsByType<GameObject>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            foreach (GameObject obj in allGameObjects)
            {
                if (!IsValidSceneObject(obj))
                {
                    continue;
                }

                Component[] components =
                    obj.GetComponents<Component>();

                foreach (Component component in components)
                {
                    if (component == null)
                    {
                        continue;
                    }

                    results.Add(
                        new SushiComponentFinderResult
                        {
                            gameObject = obj,
                            component = component,
                            path =
                                SushiComponentFinderUtility
                                    .GetGameObjectPath(obj),
                        }
                    );
                }
            }

            return results;
        }

        private static bool IsValidSceneObject(
            GameObject obj
        )
        {
            if (obj == null)
            {
                return false;
            }

            if (EditorUtility.IsPersistent(obj))
            {
                return false;
            }

            if (!obj.scene.IsValid())
            {
                return false;
            }

            return true;
        }
    }
}