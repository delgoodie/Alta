using UnityEngine;
using System.Collections.Generic;
class EntityManager : MonoBehaviour
{
    public static EntityManager Instance;
    [SerializeField]
    public string[] keys;
    public GameObject[] prefabs;
    [SerializeField]
    public int[] counts;
    [SerializeField]
    private Dictionary<string, Queue<GameObject>> pools;

    private void Awake()
    {
        Instance = this;

        if (prefabs.Length != counts.Length || counts.Length != keys.Length) Debug.LogError("Keys - Prefabs - Counts not same Length");
        int n = prefabs.Length;

        pools = new Dictionary<string, Queue<GameObject>>(n);
        for (int i = 0; i < n; i++)
        {
            Transform pool_t = new GameObject(keys[i]).transform;
            pool_t.parent = transform;
            Queue<GameObject> temp = new Queue<GameObject>(counts[i]);
            for (int j = 0; j < counts[i]; j++)
            {
                GameObject e = Instantiate(prefabs[i], Vector3.zero, Quaternion.identity, pool_t.transform);
                e.SetActive(false);
                temp.Enqueue(e);
            }
            pools.Add(keys[i], temp);
        }
    }

    public GameObject Retrieve(string key)
    {
        if (pools[key].Count > 0)
        {
            pools[key].Peek().SetActive(true);
            return pools[key].Dequeue();
        }
        else return null;
    }

    public void Release(string key, GameObject entity)
    {
        entity.SetActive(false);
        pools[key].Enqueue(entity);
    }
}