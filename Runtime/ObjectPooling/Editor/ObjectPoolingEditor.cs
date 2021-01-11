using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectPooling))]
public class ObjectPoolingEditor : Editor
{
    private ObjectPooling _target;

    private void OnEnable()
    {
        _target = target as ObjectPooling;
    }


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
