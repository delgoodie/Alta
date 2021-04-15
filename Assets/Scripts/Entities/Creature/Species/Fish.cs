using UnityEngine;
using System.Collections.Generic;

public class Fish : MonoBehaviour, ICreature
{
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

            for (int i = 0; i < neighbors.Count; i++)
            {
                if (neighbors[i].Equals(gameObject)) continue;
                cohesion += neighbors[i].transform.position;
                alignment += neighbors[i].transform.forward;

                Vector3 dif = transform.position - neighbors[i].transform.position;
                seperation += dif.normalized * (Mathf.Pow(CreatureManager.Instance.partitionSize, 2) / dif.sqrMagnitude);
            }

            cohesion /= neighbors.Count;
            cohesion -= transform.position;

            cohesion.Normalize();
            alignment.Normalize();
            seperation.Normalize();

            Vector3 direction = (cohesion + alignment + seperation).normalized;

            Quaternion targetRot = Quaternion.LookRotation(direction, Vector3.Cross(direction, transform.right));
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, .3f);
            // rigidbody.MoveRotation(Quaternion.Lerp(transform.rotation, targetRot, .3f));
            // rigidbody.AddForce(transform.forward * Time.deltaTime * 200f);
        }

        transform.Translate(Vector3.forward * Time.deltaTime * 3f, Space.Self);
    }

    private void OnDrawGizmos()
    {
        List<GameObject> neighbors = CreatureManager.Instance.GetPartition(coordinate);
        if (neighbors == null) return;
        foreach (GameObject fish in neighbors)
        {
            // Gizmos.DrawLine(transform.position, fish.transform.position);
        }
    }
}