using UnityEngine;
using System.Collections.Generic;

public class Fish : MonoBehaviour, ICreature
{
    public Vector3Int coordinate { get; set; }

    private void Awake()
    {
    }

    private void Update()
    {
        Vector3Int coord = ChunkManager.Instance.WorldToCoord(transform.position);
        List<GameObject> neighbors = new List<GameObject>();
        // foreach (KeyValuePair<Vector3Int, Chunk> c in ChunkManager.Instance.ChunkDict)
        //     neighbors.AddRange(c.Value.creatures);
        if (neighbors == null) return;
        Vector3 seperation = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        int sepcount = 0;

        for (int i = 0; i < neighbors.Count; i++)
        {
            cohesion += neighbors[i].transform.position;
            alignment += neighbors[i].transform.forward;
            if ((transform.position - neighbors[i].transform.position).sqrMagnitude < 10f * 10f)
            {
                seperation += (transform.position - neighbors[i].transform.position).normalized * (10f * 10f) / (transform.position - neighbors[i].transform.position).sqrMagnitude;
                sepcount++;
            }
        }
        cohesion = (cohesion / neighbors.Count - transform.position).normalized;
        alignment = alignment / neighbors.Count;
        seperation = seperation / sepcount;

        Vector3 direction = (cohesion + alignment * 2f + seperation);

        if (direction.sqrMagnitude > 1f * 1f)
        {
            direction.Normalize();
            Quaternion targetRot = Quaternion.LookRotation(direction, Vector3.Cross(direction, transform.right));
            // rigidbody.MoveRotation(Quaternion.Lerp(transform.rotation, targetRot, .3f));
            // rigidbody.AddForce(transform.forward * Time.deltaTime * 200f);
        }
    }
}