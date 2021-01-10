using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace BetaTool.StaticUnityEvent
{
    [Serializable]
    public class StaticEvent : ISerializationCallbackReceiver
    {
        //所有调用方法
        protected List<InvokeInfo> invokeInfos;

        //方法参数的约束
        public Type[] constraints;

        public byte[] invokeInfosData;
        public string[] constraintsData;

        public InvokeInfo[] InvokeInfos => invokeInfos.ToArray();
        public List<InvokeInfo> InvokeInfoList => invokeInfos;

        public StaticEvent()
        {
            Update();
        }

        public void Update()
        {
            constraints = GetType().GenericTypeArguments;
        }

        /// <summary>
        /// 筛选符合的方法
        /// </summary>
        /// <param name="methodInfos">要筛选的方法</param>
        /// <returns></returns>
        public MethodInfo[] FilterMethod(MethodInfo[] methodInfos)
        {
            //if(constraints == null)
            //{
            //    constraints = GetType().GenericTypeArguments;
            //}
            
            if (methodInfos == null) return null;
            if (constraints == null || constraints.Length == 0)
            {
                return methodInfos;
            }
            var _buf = new List<MethodInfo>();
            foreach (var method in methodInfos)
            {
                if (CanLoad(method))
                {
                    _buf.Add(method);
                }
            }

            return _buf.ToArray();
        }

        public bool AddInvokeInfos(MethodInfo methodInfo, object[] paramValues)
        {
            if (!CanLoad(methodInfo)) return false;
            invokeInfos.Add(new InvokeInfo(){MethodInfo = methodInfo,Propertys = paramValues});
            return true;
        }

        public void RemoveAs(int index)
        {
            invokeInfos.RemoveAt(index);
        }

        public void InitializeInvokeInfo(int index, MethodInfo methodInfo)
        {
            invokeInfos[index].MethodInfo = methodInfo;
            if (constraints != null)
            {
                invokeInfos[index].Propertys = new object[methodInfo.GetParameters().Length - constraints.Length];
            }
            else
            {
                invokeInfos[index].Propertys = new object[methodInfo.GetParameters().Length];
            }

        }

        public void InitializeList()
        {
            invokeInfos = new List<InvokeInfo>();
        }

        bool CanLoad(MethodInfo methodInfo)
        {
            if (methodInfo == null) return false;
            if (constraints == null || constraints.Length == 0) return true;
            var _param = methodInfo.GetParameters();
            if (_param.Length < constraints.Length) return false;
            for (int i = 0; i < constraints.Length; i++)
            {
                if (_param[i].ParameterType != constraints[i]) return false;
            }

            return true;
        }

        //真实调用
        public void RealInvoke(params object[] paramValues)
        {
            foreach (var VARIABLE in invokeInfos)
            {
                VARIABLE.Invoke(paramValues);
            }
        }

        public void OnBeforeSerialize()
        {
            if (constraints != null)
            {
                constraintsData = new string[constraints.Length];
                for (int i = 0; i < constraints.Length; i++)
                {
                    constraintsData[i] = constraints[i].FullName;
                }
            }

            if (invokeInfos != null)
            {
                foreach (var VARIABLE in invokeInfos)
                {
                    VARIABLE.OnBeforeSerialize();
                }

                var _bina = new BinaryFormatter();
                using var _stream = new MemoryStream();
                _bina.Serialize(_stream, invokeInfos);
                invokeInfosData = _stream.ToArray();
            }

            
        }

        public void OnAfterDeserialize()
        {
            if (constraintsData != null)
            {
                constraints = new Type[constraintsData.Length];
                for (int i = 0; i < constraints.Length; i++)
                {
                    constraints[i] = Type.GetType(constraintsData[i]);
                }
            }

            

            if (invokeInfosData != null && invokeInfosData.Length > 0)
            {
                var _bina = new BinaryFormatter();
                using var _stream = new MemoryStream(invokeInfosData);
                invokeInfos = new List<InvokeInfo>(_bina.Deserialize(_stream) as List<InvokeInfo>);

                foreach (var VARIABLE in invokeInfos)
                {
                    VARIABLE.OnAfterDeserialize();
                    if (IsDirty(VARIABLE))
                    {
                        VARIABLE.MethodInfo = null;
                        VARIABLE.Propertys = null;
                    }
                }


            }
        }

        public bool IsDirty(InvokeInfo invokeInfo)
        {
            if (invokeInfo.MethodInfo == null) return false;
            if (invokeInfo.Propertys.Length + constraints.Length !=
                invokeInfo.MethodInfo.GetParameters().Length) return true;
            var _parames = invokeInfo.MethodInfo.GetParameters();
            for (int i = 0; i < _parames.Length; i++)
            {
                if (i < constraints.Length)
                {
                    if (constraints[i] != _parames[i].ParameterType) return true;
                }
                else
                {
                    if (invokeInfo.parameterInfos[i - constraints.Length] == null)continue;
                    if (invokeInfo.parameterInfos[i - constraints.Length].ParameterType != _parames[i].ParameterType)
                        return true;
                }
            }

            return false;
        }
    }
}
