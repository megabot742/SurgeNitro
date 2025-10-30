using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class AutoFindSetup
{
    public static void FindChildrenByRegex(GameObject gameObject, string pattern, List<GameObject> matches)
        {
            for (var i = 0; i < gameObject.transform.childCount; i++)
            {
                var child = gameObject.transform.GetChild(i);

                if (Regex.IsMatch(child.name, pattern))
                {
                    matches.Add(child.gameObject);
                }

                FindChildrenByRegex(child.gameObject, pattern, matches);
            }
        }
}
