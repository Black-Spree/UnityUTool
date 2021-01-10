using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;

public class ObjectPooling : MonoBehaviour
{
    public BackUpRules rules;

    public GameObject perfab;

    public bool reAwake, reStart;

    public bool haveMaxNum;
    public int startNum, maxNum;

    public int onceInstanceNum = 1;

    public int minDestoryNum = 1;

    public float costomUpdateTime;


    System.Type[] perfabComponentTypes;
    Component[] perfabComponents;


    public bool IsMax
        => haveMaxNum && instanceObjects.Count >= maxNum;

    [SerializeField]
    List<InstanceObject> instanceObjects;
    // Start is called before the first frame update
    private void Awake()
    {
        instanceObjects = new List<InstanceObject>();
    }

    void Start()
    {
        perfabComponents = perfab.GetComponents<Component>();
        perfabComponentTypes = System.Array.ConvertAll(perfabComponents, c => c.GetType());

        for (int i = 0; i < startNum; i++)
        {
            var buf = Instantiate(perfab, transform);
            buf.transform.parent = transform;
            buf.SetActive(false);
            instanceObjects.Add(new InstanceObject()
            {
                instance = buf,
                isActive = false
            });
        }

        InvokeRepeating("DestoryMoreObjects", 0, costomUpdateTime);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 用于删除多余的对象
    void DestoryMoreObjects()
    {
        if(instanceObjects.Count > startNum)
        {
            var _buf = instanceObjects.Where(i => !i.isActive).ToArray();
            if(_buf.Length / minDestoryNum >= 1)
            {
                foreach (var item in _buf)
                {
                    instanceObjects.Remove(item);
                    Destroy(item.instance);
                }
            }
        }
    }

    // 实例化新的对象到对象池
    bool InstantiateNew()
    {
        if(!IsMax)
        {
            var _count = instanceObjects.Count;
            for (int i = 0; i < Mathf.Min(onceInstanceNum, haveMaxNum ? maxNum - _count : Mathf.Infinity); i++)
            {
                var buf = Instantiate(perfab, transform);
                buf.transform.parent = transform;
                buf.SetActive(false);
                instanceObjects.Add(new InstanceObject()
                {
                    instance = buf,
                    isActive = false
                });
            }
            return true;
        }
        return false;
    }

    // 复原对象
    void ResetObject(GameObject g)
    {
        var _components = g.GetComponents<Component>();
        for (int i = 0; i < perfabComponents.Length; i++)
        {
            if(rules)
            {
                if(rules.GetRules() != null)
                {
                    if (System.Array.IndexOf(rules.GetRules(), perfabComponentTypes[i].Name) < 0)
                        continue;
                }
            }
            // 还原属性
            foreach (var properties in perfabComponentTypes[i].GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                try
                {
                    properties.SetValue(_components[i], properties.GetValue(perfabComponents[i]));
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Reset <b>Properties</b> Error:Component.field:{perfabComponentTypes[i].Name}.{properties.Name}\nMessage:{ex.Message}");
                }
            }
            // 还原字段
            foreach (var field in perfabComponentTypes[i].GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                try
                {
                    field.SetValue(_components[i], field.GetValue(perfabComponents[i]));
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Reset <b>Fields</b> Error:Component.field:{perfabComponentTypes[i].Name}.{field.Name}\nMessage:{ex.Message}");
                }
            }
        }
    }

    // 检查是否损坏
    void CheckBreak(InstanceObject instanceObj)
    {
        var _components = System.Array.ConvertAll(instanceObj.instance.GetComponents<Component>(), c => c.GetType());
        if (_components.Length != perfabComponentTypes.Length)
        {
            instanceObj.isBreak = true;
            return;
        }
        for (int i = 0; i < perfabComponentTypes.Length; i++)
        {
            if(_components[i] != perfabComponentTypes[i])
            {
                instanceObj.isBreak = true;
                break;
            }
        }
    }

    /// <summary>
    /// 激活对象
    /// </summary>
    /// <returns>激活的实例</returns>
    public GameObject ActiveObject()
    {
        GameObject _returnValue = null;
        foreach (var item in instanceObjects)
        {
            if(!item.isActive)
            {
                if (item.isBreak)
                    continue;
                if(item.isDirty)
                    ResetObject(item.instance);

                _returnValue = item.Active();
                break;
            }
        }
        if(!_returnValue && InstantiateNew())
        {
            _returnValue = instanceObjects.Last().Active();
        }
        return _returnValue;
    }

    public void TTT()
    {
        ActiveObject();
    }

    /// <summary>
    /// 失活对象
    /// </summary>
    /// <param name="g"></param>
    public void DesActiveObject(GameObject g)
    {
        var _buf = instanceObjects.FirstOrDefault(i => i.instance == g);
        if(_buf != null)
        {
            _buf.instance.SetActive(false);
            _buf.instance.transform.parent = transform;
            _buf.isActive = false;
            CheckBreak(_buf);
        }
    }
}

[System.Serializable]
public class InstanceObject
{
    public GameObject instance;
    public bool isActive;
    public bool isDirty;
    public bool isBreak;

    public GameObject Active()
    {
        isActive = true;
        instance.transform.parent = null;
        instance.SetActive(true);
        return instance;
    }
}
