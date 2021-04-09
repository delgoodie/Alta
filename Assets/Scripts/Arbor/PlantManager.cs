using System.Collections.Generic;
using UnityEngine;

class PlantManager : MonoBehaviour
{
    public static PlantManager Instance;
    [SerializeField]
    private GameObject[] prefabs;
    [Range(0, 1)]
    [SerializeField]
    private float[] frequencies;
    [SerializeField]
    [Min(100)]
    private int PLANT_COUNT;
    private Queue<GameObject>[] pools;


    private void Awake()
    {
        Instance = this;
        pools = new Queue<GameObject>[prefabs.Length];
        float sum = 0f;
        for (int i = 0; i < frequencies.Length; i++) sum += frequencies[i];

        for (int i = 0; i < pools.Length; i++)
        {
            pools[i] = new Queue<GameObject>();
            int count = PLANT_COUNT * (int)(frequencies[i] / (float)sum);
            for (int j = 0; j < count; j++)
            {
                GameObject p = Instantiate(prefabs[i], Vector3.zero, Quaternion.identity, transform);
                p.SetActive(false);
                pools[i].Enqueue(p);
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

    public GameObject GetPlant(Ray site)
    {
        List<int> rank = RandomRank();
        for (int i = 0; i < rank.Count; i++)
        {
            IPlant plantScript = pools[i].Peek().GetComponent<IPlant>();
            if (plantScript == null || plantScript.TestSite(site))
            {
                GameObject p = pools[i].Dequeue();
                p.transform.position = site.origin;
                p.transform.rotation = Quaternion.LookRotation(Vector3.Cross(new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)), site.direction), site.direction);
                p.SetActive(true);
                return p;
            }
        }
        return null;
    }

    public void ReleasePlant(GameObject plant)
    {
        plant.SetActive(false);
        pools[0].Enqueue(plant);
    }
}