using UnityEngine;
using System.Collections.Generic;
class CreatureManager : MonoBehaviour
{
    public static CreatureManager Instance;
    public int partitionSize;
    public int count;
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
        creatures = new Dictionary<string, List<GameObject>>();
        partitions = new Dictionary<Vector3Int, List<GameObject>>();
        creatures.Add("fish", new List<GameObject>());
    }

    private void Start()
    {
    }

    private void Update()
    {
        if (GameManager.Instance.tick3)
        {
            // ServiceScheduler.Instance.Request("creature partition", gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        foreach (KeyValuePair<Vector3Int, List<GameObject>> pair in partitions)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(pair.Key * partitionSize + Vector3.one * (partitionSize / 2f), Vector3.one * partitionSize * 0.95f);
        }
    }
    public void CreatureUpdate(Vector3 _target)
    {
        target = _target;
        Partition();
        SpawnHandler();
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

                pair.Value[i].GetComponent<ICreature>().coordinate = coord;
                partitions[coord].Add(pair.Value[i]);
            }
        }
    }

    private void SpawnHandler()
    {
        // * Remove Fish
        // int c1 = 0;
        // foreach (KeyValuePair<Vector3Int, List<GameObject>> pair in partitions)
        //     c1 += pair.Value.Count;
        // int c2 = 0;
        // foreach (KeyValuePair<string, List<GameObject>> pair in creatures)
        //     c2 += pair.Value.Count;
        // Debug.Log($"Creature count {c2}, part count {c1}, ");


        List<Vector3Int> removes = new List<Vector3Int>();
        foreach (KeyValuePair<Vector3Int, List<GameObject>> pair in partitions)
        {
            if ((target - CoordToWorld(pair.Key)).sqrMagnitude > radius * radius)
            {
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    EntityManager.Instance.Release(pair.Value[i].GetComponent<ICreature>().type, pair.Value[i]);
                    creatures[pair.Value[i].GetComponent<ICreature>().type].Remove(pair.Value[i]);
                }
                removes.Add(pair.Key);
                pair.Value.Clear();
            }
        }
        foreach (Vector3Int k in removes) partitions.Remove(k);

        // * Count Fish
        int current = 0;
        foreach (KeyValuePair<Vector3Int, List<GameObject>> pair in partitions)
            current += pair.Value.Count;

        // * Add Fish
        for (int i = current; i < count; i++)
        {
            GameObject f = EntityManager.Instance.Retrieve("fish");
            if (f != null)
            {
                float dist = Random.Range(.7f, 1f);
                dist *= radius;

                Vector3 pos = target + Random.rotation * Vector3.forward * dist;
                Vector3Int coord = WorldToCoord(pos);
                f.transform.position = pos;
                f.transform.rotation = Random.rotation;
                f.GetComponent<ICreature>().coordinate = coord;

                if (!partitions.ContainsKey(coord))
                    partitions.Add(coord, new List<GameObject>());
                partitions[coord].Add(f);
                creatures[f.GetComponent<ICreature>().type].Add(f);
            }
        }
    }

    public List<GameObject> GetPartition(Vector3Int coord)
    {
        if (partitions.ContainsKey(coord)) return partitions[coord];
        else return null;
    }
}