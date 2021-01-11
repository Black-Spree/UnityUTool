using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BetaTool.Utility;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace BetaTool.StaticUnityEvent
{
    [CustomPropertyDrawer(typeof(StaticEvent),true)]
    class StaticEventDrawer : PropertyDrawer
    {
        private ReorderableList reorderable;
        private StaticEvent target;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 初始化
            if (reorderable == null)
            {
                target = property.GetTraget() as StaticEvent;
                reorderable = new ReorderableList(target.InvokeInfoList, typeof(InvokeInfo), true, true, true,
                    true);
                reorderable.drawElementCallback = DrawElementCallbackRect;
                reorderable.onAddDropdownCallback = (rect, list) => target.InvokeInfoList.Add(new InvokeInfo());
                reorderable.elementHeightCallback = ElementHeightCallback;
                reorderable.drawHeaderCallback = rect => GUI.Label(rect,
                    $"StaticEvent-{property.name}({string.Join(" ", Array.ConvertAll(target.constraints, c => c.Name))})");
            }

            if (target != null)
            {
                target.Update();
                if (target.InvokeInfoList == null)
                    target.InitializeList();
                if(reorderable.list == null)
                    reorderable.list = target.InvokeInfoList;
                foreach (var VARIABLE in target.InvokeInfoList)
                {
                    if (target.IsDirty(VARIABLE))
                    {
                        VARIABLE.MethodInfo = null;
                        VARIABLE.Propertys = null;
                    }
                }
                reorderable.DoList(position);
            }
        }

        void DrawElementCallbackRect(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.height = 20f;
            // 防止出现空对象
            if (target.InvokeInfoList[index] == null)
                target.InvokeInfoList[index] = new InvokeInfo();
            
            // 获取方法所在脚本对象
            var _element = target.InvokeInfoList[index];
            MonoScript _script = null;
            if (_element.MethodInfo != null)
            {
                _script = (MonoScript)EditorGUI.ObjectField(rect, "Script",
                    UtilityAssets.AllScript.First(s => s.GetClass() == _element.MethodInfo.DeclaringType),
                    typeof(MonoScript), null);
            }
            else
            {
                _script = (MonoScript)EditorGUI.ObjectField(rect, "Script", null, typeof(MonoScript), null);
            }

            // 若找到了脚本对象
            if (_script != null)
            {
                
                // 查询脚本所含有的所有方法
                var _methods = target.FilterMethod(_script.GetClass().GetMethods(BindingFlags.Static | BindingFlags.Public));

                // 刷新调用信息
                if (_methods != null && _methods.Length > 0)
                {
                    bool _dirty = false;
                    int _methodIndex = -1;
                    // 查询调用信息在数组中的下标
                    if (_element.MethodInfo == null)
                    {
                        _dirty = true;
                        _methodIndex = 0;
                    }
                    else
                    {
                        _methodIndex = Array.IndexOf(_methods, _element.MethodInfo);
                        if (_methodIndex < 0)
                        {
                            _dirty = true;
                            _methodIndex = 0;
                        }
                    }

                    rect.y += 25;
                    var _buf = EditorGUI.Popup(rect, _methodIndex, Array.ConvertAll(_methods, m => m.Name));

                    if (_buf != _methodIndex)
                    {
                        _dirty = true;
                        _methodIndex = _buf;
                    }

                    if (_dirty)
                    {
                        target.InitializeInvokeInfo(index, _methods[_methodIndex]);
                    }
                }

            }

            if (_element.MethodInfo != null)
            {
                var _parms = _element.parameterInfos;
                if (_parms != null)
                {
                    rect.y += 25f;
                    for (int i = 0; i < _parms.Length; i++)
                    {
                        _element.Propertys[i] = DrawParameter(rect, _parms[i], _element.Propertys[i]);
                        rect.y += 25f;
                    }
                }
            }
        }

        float ElementHeightCallback(int index)
        {
            if (target == null)
                return 50;
            return 50 + (target.InvokeInfoList[index].parameterInfos?.Length ?? 0) * 25;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (target == null)
            {
                target = property.GetTraget() as StaticEvent;
            }

            float _height = 70;
            if (target.InvokeInfoList != null)
            {
                foreach (var VARIABLE in target.InvokeInfoList)
                {
                    _height += 50;
                    _height += (VARIABLE.parameterInfos?.Length ?? 0) * 25;
                }
            }

            return _height;
        }

        // 用于绘制属性
        object DrawParameter(Rect rect, ParameterInfo info, object value)
        {
            if (info.ParameterType == typeof(int))
            {
                return EditorGUI.IntField(rect, info.Name, (int) (value ?? 0));
            }
            if (info.ParameterType == typeof(float))
            {
                return EditorGUI.FloatField(rect, info.Name, (float)(value ?? 0f));
            }
            if (info.ParameterType == typeof(string))
            {
                return EditorGUI.TextField(rect, info.Name, (string)(value ?? string.Empty));
            }
            if (info.ParameterType == typeof(float))
            {
                return EditorGUI.FloatField(rect, info.Name, (float)(value ?? 0f));
            }
            if (info.ParameterType == typeof(float))
            {
                return EditorGUI.FloatField(rect, info.Name, (float)(value ?? 0f));
            }
            return null;
        }
    }
}
