using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace BetaTool.StaticUnityEvent
{
    [Serializable]
    /// <summary>
    /// 保存了调用静态方法的信息
    /// </summary>
    public class InvokeInfo
    {
        //方法信息
        [NonSerialized]
        public MethodInfo MethodInfo;
        //参数内容
        [NonSerialized]
        public object[] Propertys;

        public string className;
        public string methodName;
        public byte[] propertysData;

        public ParameterInfo[] parameterInfos
        {
            get
            {
                if (Propertys == null || MethodInfo == null) return null;
                var _buf = new ParameterInfo[Propertys.Length];
                var _pro = MethodInfo.GetParameters();
                Array.Copy(_pro, _pro.Length - Propertys.Length, _buf, 0, Propertys.Length);
                return _buf;
            }
        }

        public void Invoke(params object[] paramValues)
        {
            if(MethodInfo == null) return;
            if (Propertys != null)
            {
                var _buf = new object[paramValues.Length + Propertys.Length];
                Array.Copy(paramValues, _buf, paramValues.Length);
                Array.Copy(Propertys, 0, _buf, paramValues.Length, Propertys.Length);
                MethodInfo.Invoke(null, _buf);
            }
            else
            {
                MethodInfo.Invoke(null, paramValues);
            }
        }

        public void OnAfterDeserialize()
        {
            if(className != null)
                MethodInfo = Type.GetType(className)?.GetMethod(methodName);
            if (propertysData != null && propertysData.Length > 0)
            {
                var _bin = new BinaryFormatter();
                using var _stream = new MemoryStream(propertysData);
                Propertys = (object[])_bin.Deserialize(_stream);
            }
            else
            {
                MethodInfo = null;
            }
        }

        public void OnBeforeSerialize()
        {
            if (MethodInfo != null)
            {
                className = MethodInfo.DeclaringType.FullName;
                methodName = MethodInfo.Name;
            }
            if (Propertys != null)
            {
                var _bin = new BinaryFormatter();
                using var _stream = new MemoryStream();
                _bin.Serialize(_stream,Propertys);
                propertysData = _stream.ToArray();
            }
        }
    }
}
