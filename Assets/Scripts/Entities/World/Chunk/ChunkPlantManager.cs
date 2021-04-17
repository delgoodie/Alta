using UnityEngine;
using System.Collections.Generic;


public class ChunkPlantManager : MonoBehaviour
{
    [SerializeField]
    private int count;
    private List<GameObject> plants;
    private Mesh mesh;
    [SerializeField]
    private string[] plantTypes;
    [SerializeField]
    [Range(0, 1)]
    private float[] plantProbabilities;

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
            float r = Random.Range(0f, 1f), c = 0;
            int j = 0;
            while (c + plantProbabilities[j] < r)
            {
                c += plantProbabilities[j];
                j++;
                if (j >= plantProbabilities.Length) Debug.LogError("Invalid probability");
            }
            GameObject p = EntityManager.Instance.Retrieve(plantTypes[j]);
            if (p != null)
            {
                Ray site = RandomSurfaceRay();
                p.transform.position = site.origin - site.direction * .1f;
                p.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(Random.rotation * Vector3.forward, site.direction), site.direction);
                plants.Add(p);
            }
        }
    }

    public void ReleasePlants()
    {
        for (int i = 0; i < plants.Count; i++)
            EntityManager.Instance.Release(plants[i]);

        plants.Clear();
    }
}