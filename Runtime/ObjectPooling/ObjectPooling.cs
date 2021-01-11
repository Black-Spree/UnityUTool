using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    public enum UpdateMode
    {
        GlobaUpdate,LocalUpdate,OnChange,OnAdd,OnRemove,Custom
    }
    
    public GameObject prefab;
    // 最大值与最小值
    public RangeInt range;

    public int minNum, maxNum;
    // 允许溢出
    public bool allowOverFlow = true;
    public UpdateMode updateMode = UpdateMode.GlobaUpdate;
    public float updateTime;

    // 可用对象
    private List<GameObject> availableObject;
    // 损坏的对象
    private List<GameObject> damageObject;
    // 已被激活的对象
    private List<GameObject> activeObject;

    private Component[] componentList;

    #region 静态部分

    private static List<ObjectPooling> poolings;

    static ObjectPooling()
    {
        poolings = new List<ObjectPooling>();
    }

    /// <summary>
    /// 激活对象
    /// </summary>
    /// <param name="obj">预制体</param>
    /// <returns></returns>
    public static GameObject Active(GameObject obj)
    {
        if (poolings == null) return null;
        return poolings.First(p => p.prefab == obj).Active();
    }

    /// <summary>
    /// 失活对象
    /// </summary>
    /// <param name="obj">对象</param>
    public static void DeActive(GameObject obj)
    {
        if (poolings == null) return;
        poolings.FirstOrDefault(p => p.IsSubobject(obj))?.Deactive(obj);
    }

    #endregion

    private void Awake()
    {
        // 初始化对象池
        availableObject = new List<GameObject>();
        damageObject = new List<GameObject>();
        activeObject = new List<GameObject>();
        componentList = prefab.GetComponents<Component>();

        // 初始化基础数量
        for (int i = 0; i < minNum; i++)
        {
            AddObject(false);
        }

        // 初始化本地计时器
        if (updateMode == UpdateMode.LocalUpdate)
        {
            InvokeRepeating("PoolingUpdate",0,updateTime);
        }
    }

    private void OnEnable()
    {
        // 添加自己到列表
        if (prefab != null)
        {
            poolings.Add(this);
        }
        else
        {
            Debug.LogWarning($"ObjectPooling-对象池{gameObject.name}的prefab为空!");
        }
    }

    private void OnDisable()
    {
        // 从列表删除自身
        poolings?.Remove(this);
    }

    private void OnDestroy()
    {
        // 从列表删除自身
        poolings?.Remove(this);
    }

    //void Start()
    //{
        
    //}

    //void Update()
    //{
        
    //}

    // 激活对象
    public GameObject Active()
    {
        if (availableObject.Count > 0)
        {
            activeObject.Add(availableObject.Last());
            availableObject.RemoveAt(availableObject.Count - 1);
            activeObject.Last().SetActive(true);
            activeObject.Last().transform.parent = null;
            return activeObject.Last();
        }

        return AddObject(true);
    }

    // 失活对象
    public void Deactive(GameObject obj)
    {
        if(!activeObject.Contains(obj)) return;
        activeObject.Remove(obj);
        // 直接删除(过多)
        if (activeObject.Count + availableObject.Count > maxNum)
        {
            damageObject.Add(obj);
            return;
        }

        obj.transform.parent = transform;
        obj.SetActive(false);
        if (Reset(obj))
        {
            print("回收");
            availableObject.Add(obj);
        }
        else
        {
            print("销毁");
            damageObject.Add(obj);
        }
    }

    // 还原一个对象
    public bool Reset(GameObject obj)
    {
        // 检测对象是否还能够还原
        var _buf = obj.GetComponents<Component>();
        if (_buf.Length != componentList.Length) return false;
        for (int i = 0; i < componentList.Length; i++)
        {
            if (_buf[i].GetType() != componentList[i].GetType()) return false;
        }

        // 开始还原对象
        for (int i = 0; i < componentList.Length; i++)
        {
            var _fields = componentList.GetType().GetFields();
            foreach (var field in _fields)
            {
                field.SetValue(_buf[i], field.GetValue(componentList[i]));
            }
        }

        return true;
    }

    // 添加一个对象
    public GameObject AddObject(bool active)
    {
        // 刷新对象
        if (updateMode == UpdateMode.OnAdd)
        {
            PoolingUpdate();
        }
        // 是否立刻激活对象
        if (active)
        {
            // 是否未到上限(或允许溢出)
            if ((activeObject.Count + availableObject.Count + 1 <= maxNum) || allowOverFlow)
            {
                activeObject.Add(Instantiate(prefab));
                return activeObject.Last();
            }

            return null;
        }
        else
        {
            availableObject.Add(Instantiate(prefab,transform));
            availableObject.Last().SetActive(false);
            return availableObject.Last();
        }
    }

    // 移除一个对象
    public void RemoveObject(GameObject obj)
    {
        // 刷新对象
        if (updateMode == UpdateMode.OnRemove)
        {
            PoolingUpdate();
        }

        var _index = availableObject.IndexOf(obj);
        if(_index < 0) return;
        availableObject.RemoveAt(_index);
        damageObject.Add(obj);
    }

    // 刷新对象池
    public void PoolingUpdate()
    {
        // 移除空对象
        for (int i = 0; i < activeObject.Count; i++)
        {
            if(activeObject[i] != null)continue;
            activeObject.RemoveAt(i--);
        }
        for (int i = 0; i < availableObject.Count; i++)
        {
            if (availableObject[i] != null) continue;
            availableObject.RemoveAt(i--);
        }
        // 删除所有损坏对象
        foreach (var VARIABLE in damageObject)
        {
            Destroy(VARIABLE);
        }
        damageObject.Clear();

    }

    // 实例是否来自此对象池
    public bool IsSubobject(GameObject obj)
    {
        return activeObject.Contains(obj);
    }
}
