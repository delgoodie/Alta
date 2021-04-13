using UnityEngine;
using System.Collections.Generic;
class EntityManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] prefabs;
    [Range(0, 1)]
    [SerializeField]
    private float[] frequencies;
    [SerializeField]
    [Min(100)]
    private int COUNT;
    private Queue<GameObject>[] pools;

    public void Init()
    {
        pools = new Queue<GameObject>[prefabs.Length];
        float sum = 0f;
        for (int i = 0; i < frequencies.Length; i++) sum += frequencies[i];

        for (int i = 0; i < pools.Length; i++)
        {
            pools[i] = new Queue<GameObject>();
            int count = COUNT * (int)(frequencies[i] / (float)sum);
            for (int j = 0; j < count; j++)
            {
                GameObject e = Instantiate(prefabs[i], Vector3.zero, Quaternion.identity, transform);
                e.SetActive(false);
                pools[i].Enqueue(e);
            }
        }
    }

    private void Start()
    {
    }

    private void Update()
    {

    }

    private List<int> RandomRank()
    {
        List<int> index = new List<int>();
        List<float> rank = new List<float>();
        for (int i = 0; i < frequencies.Length; i++)
        {
            if (pools[i].Count > 0)
            {
                index.Add(i);
                rank.Add(frequencies[i] * Random.Range(0f, 1f));
            }
        }
        bool sorted = false;
        while (!sorted)
        {
            sorted = true;
            for (int i = 1; i < index.Count; i++)
            {
                if (rank[index[i]] > rank[index[i - 1]])
                {
                    float rtemp = rank[index[i]];
                    rank[index[i]] = rank[index[i - 1]];
                    rank[index[i - 1]] = rtemp;
                    int itemp = index[i];
                    index[i] = index[i - 1];
                    index[i - 1] = index[i];
                    sorted = false;
                }
            }
        }
        return index;
    }

    public GameObject Retrieve(Ray site)
    {
        List<int> rank = RandomRank();
        for (int i = 0; i < rank.Count; i++)
        {
            IEntity e_script = pools[rank[i]].Peek().GetComponent<IEntity>();
            if (e_script == null || e_script.TestSite(site))
            {
                GameObject e = pools[rank[i]].Dequeue();
                e.transform.position = site.origin;
                e.transform.rotation = Quaternion.LookRotation(Vector3.Cross(new Vector3(Random.Range(1f, 2f), Random.Range(1f, 2f), Random.Range(1f, 2f)), site.direction.normalized), site.direction);
                e.SetActive(true);
                return e;
            }
        }
        return null;
    }

    public void Release(GameObject entity)
    {
        entity.SetActive(false);
        pools[0].Enqueue(entity);
    }
}