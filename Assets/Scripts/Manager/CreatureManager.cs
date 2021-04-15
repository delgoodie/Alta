using UnityEngine;
using System.Collections.Generic;
class CreatureManager : MonoBehaviour
{
    public static CreatureManager Instance;
    public int partitionSize;
    public int partCount;
    public float radius;
    [HideInInspector]
    public Dictionary<string, List<GameObject>> creatures;
    [HideInInspector]
    public Dictionary<Vector3Int, List<GameObject>> partitions;
    [HideInInspector]
    public Vector3 target;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
    }

    private void Update()
    {
        if (GameManager.Instance.tick3)
        {
            Partition();
            SpawnHandler();
            // ServiceScheduler.Instance.Request("creature partition", gameObject);
        }
    }

    public Vector3Int WorldToCoord(Vector3 world)
    {
        if (world.x < 0) world.x -= partitionSize;
        if (world.y < 0) world.y -= partitionSize;
        if (world.z < 0) world.z -= partitionSize;

        return new Vector3Int((int)(world.x / (partitionSize)), (int)(world.y / (partitionSize)), (int)(world.z / (partitionSize)));
    }

    public Vector3 CoordToWorld(Vector3Int coord)
    {
        return new Vector3(coord.x * partitionSize, coord.y * partitionSize, coord.z * partitionSize);
    }

    private void Partition()
    {
        Vector3Int coord;
        partitions.Clear();
        foreach (KeyValuePair<string, List<GameObject>> pair in creatures)
        {
            for (int i = 0; i < pair.Value.Count; i++)
            {
                coord = WorldToCoord(pair.Value[i].transform.position);

                if (!partitions.ContainsKey(coord))
                    partitions.Add(coord, new List<GameObject>());

                partitions[coord].Add(pair.Value[i]);
            }
        }
    }


    private void SpawnHandler()
    {
        foreach (KeyValuePair<Vector3Int, List<GameObject>> pair in partitions)
        {
            float dist = (target - CoordToWorld(pair.Key)).magnitude / radius;
            if (dist > 1)
            {
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    EntityManager.Instance.Release("fish", pair.Value[i]);
                    pair.Value.Clear();
                    partitions.Remove(pair.Key);
                }
            }
            else
            {
                float need = (float)pair.Value.Count / (float)(partCount);
                if (need > 1)
                {
                    for (int i = 0; i < (int)(need * 3f); i++)
                    {
                        EntityManager.Instance.Release("fish", pair.Value[0]);
                        pair.Value.RemoveAt(0);
                    }
                }
                else
                {
                    for (int i = 0; i < (int)(dist * need * 3f); i++)
                    {
                        GameObject f = EntityManager.Instance.Retrieve("fish");
                        if (f != null)
                        {
                            f.transform.position = new Vector3(pair.Key.x + Random.Range(0f, partitionSize), pair.Key.y + Random.Range(0f, partitionSize), pair.Key.z + Random.Range(0f, partitionSize));
                        }
                    }
                }
            }
        }
    }

    public List<GameObject> GetPartition(Vector3Int coord)
    {
        if (partitions.ContainsKey(coord)) return partitions[coord];
        else return null;
    }
}