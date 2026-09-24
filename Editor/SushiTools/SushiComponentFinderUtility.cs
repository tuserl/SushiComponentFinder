using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SushiTools
{
    public static class SushiComponentFinderUtility
    {
        public static string GetGameObjectPath(
            GameObject obj
        )
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

        public static void SelectGameObject(
            GameObject obj
        )
        {
            if (obj == null)
            {
                return;
            }

            Selection.activeGameObject = obj;

            EditorGUIUtility.PingObject(obj);
        }
    }
}