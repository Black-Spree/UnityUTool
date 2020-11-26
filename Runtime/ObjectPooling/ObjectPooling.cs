using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;

// һ���������

public class ObjectPooling : MonoBehaviour
{
    #region ��̬����
    public static List<ObjectPooling> instances;
    #endregion

    public GameObject prefab;
    public int minNum;

    public bool haveMaxNum;
    public int maxNum;
    public int maxDestoryNum;

    public bool resetProperty;
    public bool reAwake;
    public bool reStart;

    public float UpdateDeltaTime;

    public List<GameObject> objects;
    List<GameObject> waitToAwake;
    
    
    void Start()
    {
        if(instances == null)
            instances = new List<ObjectPooling>();

        instances.Add(this);

        waitToAwake = new List<GameObject>();
        objects = new List<GameObject>();
        for (int i = 0; i < minNum; i++)
        {
            objects.Add(Instantiate(prefab, transform));
            objects.Last().SetActive(false);
        }
    }

    private void Update()
    {
        if(waitToAwake.Count > 0)
        {
            StartCoroutine(StartAwake());
        }
    }

    IEnumerator StartAwake()
    {
        yield return new WaitForFixedUpdate();
        foreach (var item in waitToAwake)
        {
            if (reAwake)
                item.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);
            if (reStart)
                item.SendMessage("Start", SendMessageOptions.DontRequireReceiver);
        }
        waitToAwake.Clear();
    }

    /// <summary>
    /// ����һ������
    /// </summary>
    /// <returns></returns>
    public GameObject Active()
    {
        if(objects.Count > 0)
        {
            var buf = objects.Last();
            buf.transform.parent = null;
            if(reAwake || reStart)
            {
                waitToAwake.Add(buf);
            }
            else
            {
                buf.SetActive(true);
            }
            return buf;
        }
        else
        {
            return Instantiate(prefab);
        }
    }

    /// <summary>
    /// ʹһ������ʧ��
    /// </summary>
    /// <param name="g">����</param>
    public void Deactivation(GameObject g)
    {
        var comparers = g.GetComponents<Component>();
        // �����ö�������
        if(resetProperty)
        {
            foreach (var item in prefab.GetComponents<Component>())
            {
                if (item.GetType() == typeof(MeshRenderer) || item.GetType() == typeof(MeshFilter))
                    continue;
                var buf = comparers.First(c => c.GetType() == item.GetType());
                foreach (var item2 in item.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    try
                    {
                        item2.SetValue(buf, item2.GetValue(item));
                    }
                    catch (System.Exception e)
                    {
                        print(e);
                    }

                }
                foreach (var item2 in item.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    try
                    {
                        if (item2.CanWrite && item2.CanRead)
                            item2.SetValue(buf, item2.GetValue(item));
                    }
                    catch (System.Exception e)
                    {
                        print(e);
                    }
                }
            }
        }
        g.transform.parent = transform;
        g.SetActive(false);
    }
}
