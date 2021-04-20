using UnityEngine;
using System.Collections.Generic;

public class Fish : MonoBehaviour, ICreature, IEntity
{
    public float maxSpeed;
    [HideInInspector]
    public float speed;
    public Vector3Int coordinate { get; set; }
    public string type { get; } = "fish";

    private void Awake()
    {
    }

    private void Update()
    {
        List<GameObject> neighbors = CreatureManager.Instance.GetPartition(coordinate);

        if (neighbors != null && neighbors.Count > 0)
        {
            Vector3 cohesion = Vector3.zero;
            Vector3 alignment = Vector3.zero;
            Vector3 seperation = Vector3.zero;
            Vector3 avoidance = Vector3.zero;
            Vector3 center = Vector3.zero;

            for (int i = 0; i < neighbors.Count; i++)
            {
                if (neighbors[i].Equals(gameObject)) continue;
                cohesion += neighbors[i].transform.position;
                alignment += neighbors[i].transform.forward;

                Vector3 dif = transform.position - neighbors[i].transform.position;
                seperation += dif.normalized * (Mathf.Pow(CreatureManager.Instance.partitionSize, 2) / dif.sqrMagnitude);
            }

            Vector3[] dirs = new Vector3[5] {
                transform.forward,
                Vector3.Slerp(transform.forward, transform.up, .3f),
                Vector3.Slerp(transform.forward, -transform.up, .3f),
                Vector3.Slerp(transform.forward, transform.right, .3f),
                Vector3.Slerp(transform.forward, -transform.right, .3f)
            };


            for (int i = 0; i < dirs.Length; i++)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, dirs[i], out hit, CreatureManager.Instance.partitionSize * .5f))
                    avoidance += -dirs[i] * (CreatureManager.Instance.partitionSize / hit.distance);
            }

            center += CreatureManager.Instance.target - transform.position;

            cohesion /= neighbors.Count;
            cohesion -= transform.position;


            cohesion = cohesion.normalized * CreatureManager.Instance.swarmCohesion;
            alignment = alignment.normalized * CreatureManager.Instance.swarmAlignment;
            seperation *= CreatureManager.Instance.swarmSeperation * .001f;//.002f;
            avoidance *= CreatureManager.Instance.swarmAvoidance * .01f;
            center *= CreatureManager.Instance.swarmCenter * .01f;

            Vector3 direction = (transform.forward + cohesion + alignment + seperation + avoidance + center).normalized;

            speed = Mathf.Lerp(speed, Mathf.Max(0f, Vector3.Dot(direction, transform.forward) * maxSpeed), .1f);



            Quaternion targetRot = Quaternion.LookRotation(direction, Vector3.Cross(direction, transform.right));

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 3);
        }
        transform.Translate(Vector3.forward * Time.deltaTime * speed, Space.Self);
    }

    private void OnDrawGizmos()
    {
        List<GameObject> neighbors = CreatureManager.Instance.GetPartition(coordinate);
        if (neighbors == null) return;
        foreach (GameObject fish in neighbors)
        {
            // Gizmos.DrawLine(transform.position, fish.transform.position);
        }

        Vector3[] dirs = new Vector3[5] {
                transform.forward,
                Vector3.Slerp(transform.forward, transform.up, .3f),
                Vector3.Slerp(transform.forward, -transform.up, .3f),
                Vector3.Slerp(transform.forward, transform.right, .3f),
                Vector3.Slerp(transform.forward, -transform.right, .3f)
            };

        foreach (Vector3 dir in dirs)
        {
            Gizmos.color = Color.green;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, dir, out hit, CreatureManager.Instance.partitionSize))
                Gizmos.color = Color.red;

            Gizmos.DrawLine(transform.position, transform.position + dir * 5f);
        }
    }
}