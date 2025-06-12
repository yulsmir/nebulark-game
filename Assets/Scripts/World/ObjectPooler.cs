using System.Collections.Generic;
using UnityEngine;
public class ObjectPooler : MonoBehaviour
{
    public GameObject prefab;
    public int initialSize = 100;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject Get(Vector3 position, Quaternion rotation, Transform parent = null)
    {
        GameObject obj = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab);
        obj.transform.SetParent(parent);
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}