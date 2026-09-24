using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SushiTools
{
  public static class SushiComponentFinderSearch
  {
    public static List<SushiComponentFinderResult> Find(
        string searchText,
        bool exact,
        bool includeEnabled,
        bool includeDisabled,
        bool selectedOnly
    )
    {
      List<SushiComponentFinderResult> results = new();

      string query = searchText.Trim();

      // Empty search = List All.
      if (string.IsNullOrEmpty(query))
      {
        return FindAll(
            includeEnabled,
            includeDisabled,
            selectedOnly
        );
      }

      GameObject[] gameObjects =
          GetGameObjects(
              selectedOnly
          );

      foreach (GameObject obj in gameObjects)
      {
        if (!IsValidSceneObject(obj))
        {
          continue;
        }

        if (!IsAllowedActiveState(
                obj,
                includeEnabled,
                includeDisabled))
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

          bool matches;

          if (exact)
          {
            matches =
                componentName == query;
          }
          else
          {
            matches =
                componentName.Contains(
                    query,
                    StringComparison.OrdinalIgnoreCase
                );
          }

          if (!matches)
          {
            continue;
          }

          results.Add(
              CreateResult(
                  obj,
                  component
              )
          );
        }
      }

      return results;
    }

    public static List<SushiComponentFinderResult> FindAll(
        bool includeEnabled,
        bool includeDisabled,
        bool selectedOnly
    )
    {
      List<SushiComponentFinderResult> results = new();

      GameObject[] gameObjects =
          GetGameObjects(
              selectedOnly
          );

      foreach (GameObject obj in gameObjects)
      {
        if (!IsValidSceneObject(obj))
        {
          continue;
        }

        if (!IsAllowedActiveState(
                obj,
                includeEnabled,
                includeDisabled))
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
              CreateResult(
                  obj,
                  component
              )
          );
        }
      }

      return results;
    }

    private static GameObject[] GetGameObjects(
        bool selectedOnly
    )
    {
      if (!selectedOnly)
      {
        return UnityEngine.Object.FindObjectsByType<GameObject>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
      }

      HashSet<GameObject> selectedHierarchy = new();

      foreach (GameObject selected in Selection.gameObjects)
      {
        if (selected == null)
        {
          continue;
        }

        Transform[] children =
            selected.GetComponentsInChildren<Transform>(
                true
            );

        foreach (Transform child in children)
        {
          if (child != null)
          {
            selectedHierarchy.Add(
                child.gameObject
            );
          }
        }
      }

      return new List<GameObject>(
          selectedHierarchy
      ).ToArray();
    }

    private static bool IsAllowedActiveState(
        GameObject obj,
        bool includeEnabled,
        bool includeDisabled
    )
    {
      if (obj.activeInHierarchy)
      {
        return includeEnabled;
      }

      return includeDisabled;
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

    private static SushiComponentFinderResult CreateResult(
        GameObject obj,
        Component component
    )
    {
      return new SushiComponentFinderResult
      {
        gameObject = obj,
        component = component,
        path =
              SushiComponentFinderUtility
                  .GetGameObjectPath(obj)
      };
    }
  }
}
