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
    // ���ֵ����Сֵ
    public RangeInt range;

    public int minNum, maxNum;
    // �������
    public bool allowOverFlow = true;
    public UpdateMode updateMode = UpdateMode.GlobaUpdate;
    public float updateTime;

    // ���ö���
    private List<GameObject> availableObject;
    // �𻵵Ķ���
    private List<GameObject> damageObject;
    // �ѱ�����Ķ���
    private List<GameObject> activeObject;

    private Component[] componentList;

    #region ��̬����

    private static List<ObjectPooling> poolings;

    static ObjectPooling()
    {
        poolings = new List<ObjectPooling>();
    }

    /// <summary>
    /// �������
    /// </summary>
    /// <param name="obj">Ԥ����</param>
    /// <returns></returns>
    public static GameObject Active(GameObject obj)
    {
        if (poolings == null) return null;
        return poolings.First(p => p.prefab == obj).Active();
    }

    /// <summary>
    /// ʧ�����
    /// </summary>
    /// <param name="obj">����</param>
    public static void DeActive(GameObject obj)
    {
        if (poolings == null) return;
        poolings.FirstOrDefault(p => p.IsSubobject(obj))?.Deactive(obj);
    }

    #endregion

    private void Awake()
    {
        // ��ʼ�������
        availableObject = new List<GameObject>();
        damageObject = new List<GameObject>();
        activeObject = new List<GameObject>();
        componentList = prefab.GetComponents<Component>();

        // ��ʼ����������
        for (int i = 0; i < minNum; i++)
        {
            AddObject(false);
        }

        // ��ʼ�����ؼ�ʱ��
        if (updateMode == UpdateMode.LocalUpdate)
        {
            InvokeRepeating("PoolingUpdate",0,updateTime);
        }
    }

    private void OnEnable()
    {
        // ����Լ����б�
        if (prefab != null)
        {
            poolings.Add(this);
        }
        else
        {
            Debug.LogWarning($"ObjectPooling-�����{gameObject.name}��prefabΪ��!");
        }
    }

    private void OnDisable()
    {
        // ���б�ɾ������
        poolings?.Remove(this);
    }

    private void OnDestroy()
    {
        // ���б�ɾ������
        poolings?.Remove(this);
    }

    //void Start()
    //{
        
    //}

    //void Update()
    //{
        
    //}

    // �������
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

    // ʧ�����
    public void Deactive(GameObject obj)
    {
        if(!activeObject.Contains(obj)) return;
        activeObject.Remove(obj);
        // ֱ��ɾ��(����)
        if (activeObject.Count + availableObject.Count > maxNum)
        {
            damageObject.Add(obj);
            return;
        }

        obj.transform.parent = transform;
        obj.SetActive(false);
        if (Reset(obj))
        {
            print("����");
            availableObject.Add(obj);
        }
        else
        {
            print("����");
            damageObject.Add(obj);
        }
    }

    // ��ԭһ������
    public bool Reset(GameObject obj)
    {
        // �������Ƿ��ܹ���ԭ
        var _buf = obj.GetComponents<Component>();
        if (_buf.Length != componentList.Length) return false;
        for (int i = 0; i < componentList.Length; i++)
        {
            if (_buf[i].GetType() != componentList[i].GetType()) return false;
        }

        // ��ʼ��ԭ����
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

    // ���һ������
    public GameObject AddObject(bool active)
    {
        // ˢ�¶���
        if (updateMode == UpdateMode.OnAdd)
        {
            PoolingUpdate();
        }
        // �Ƿ����̼������
        if (active)
        {
            // �Ƿ�δ������(���������)
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

    // �Ƴ�һ������
    public void RemoveObject(GameObject obj)
    {
        // ˢ�¶���
        if (updateMode == UpdateMode.OnRemove)
        {
            PoolingUpdate();
        }

        var _index = availableObject.IndexOf(obj);
        if(_index < 0) return;
        availableObject.RemoveAt(_index);
        damageObject.Add(obj);
    }

    // ˢ�¶����
    public void PoolingUpdate()
    {
        // �Ƴ��ն���
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
        // ɾ�������𻵶���
        foreach (var VARIABLE in damageObject)
        {
            Destroy(VARIABLE);
        }
        damageObject.Clear();

    }

    // ʵ���Ƿ����Դ˶����
    public bool IsSubobject(GameObject obj)
    {
        return activeObject.Contains(obj);
    }
}
