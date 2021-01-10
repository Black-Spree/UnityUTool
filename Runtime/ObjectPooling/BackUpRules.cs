using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ObjectPooling/Create Rules")]
public class BackUpRules : ScriptableObject
{
    public BackUpRules parent;
    public string[] classNames;

    string[] rules;
    bool isDirty;

    public string[] GetRules()
    {
        if(isDirty)
        {
            isDirty = false;
            rules = new string[classNames.Length + parent?.GetRules()?.Length ?? 0];
            classNames.CopyTo(rules, 0);
            if(parent?.GetRules() != null)
                parent.GetRules().CopyTo(rules, classNames.Length);
        }
        return rules;
    }
}