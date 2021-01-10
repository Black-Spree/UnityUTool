using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace BetaTool.Utility
{
    public static class UtilityAdditional
    {
        public static object GetTraget(this SerializedProperty serializedProperty)
        {
            object target = serializedProperty.serializedObject.targetObject;
            var path = serializedProperty.propertyPath;
            path.Replace(".Array.data[", "[");
            var elements = path.Split('.');
            foreach (var VARIABLE in elements)
            {
                if (VARIABLE.Contains("["))
                {
                    var elementName = VARIABLE.Substring(0, VARIABLE.IndexOf("["));
                    var index = int.Parse(VARIABLE.Substring(VARIABLE.IndexOf("[") + 1).Replace("]", string.Empty));
                    target = GetValue(target, elementName, index);
                }
                else
                {
                    target = GetValue(target, VARIABLE);
                }
            }

            return target;
        }
        private static object GetValue(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f == null)
            {
                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p == null)
                    return null;
                return p.GetValue(source, null);
            }
            return f.GetValue(source);
        }

        private static object GetValue(object source, string name, int index)
        {
            var enumerable = GetValue(source, name) as IEnumerable;
            var enm = enumerable.GetEnumerator();
            while (index-- >= 0)
                enm.MoveNext();
            return enm.Current;
        }
    }
}
