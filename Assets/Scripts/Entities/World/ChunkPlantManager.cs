using UnityEngine;
using System.Collections.Generic;


public class ChunkPlantManager : MonoBehaviour
{
    [SerializeField]
    private int count;
    private List<GameObject> plants;
    private Mesh mesh;

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        plants = new List<GameObject>();
    }

    private void Update()
    {

    }

    private Ray RandomSurfaceRay()
    {
        int tri = Random.Range(0, mesh.triangles.Length / 3);
        Vector3 a = mesh.vertices[mesh.triangles[tri * 3]], b = mesh.vertices[mesh.triangles[tri * 3 + 1]], c = mesh.vertices[mesh.triangles[tri * 3 + 2]];
        return new Ray(transform.position + Vector3.Lerp(a, Vector3.Lerp(b, c, Random.Range(0.0f, 1.0f)), Random.Range(0.0f, 1.0f)), Vector3.Cross(a - b, a - c));
    }

    public void CreatePlants()
    {
        ReleasePlants();
        for (int i = 0; i < count; i++)
        {
            GameObject k = EntityManager.Instance.Retrieve("kelp");
            if (k != null)
            {
                Ray site = RandomSurfaceRay();
                k.transform.position = site.origin - site.direction * .1f;
                k.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(Random.rotation * Vector3.forward, site.direction), site.direction);
                plants.Add(k);
            }
        }
    }

    public void ReleasePlants()
    {
        for (int i = 0; i < plants.Count; i++)
        {
            EntityManager.Instance.Release("kelp", plants[i]);
        }
        plants.Clear();
    }
}