using System.Collections.Generic;
using UnityEngine;

class PlantManager : MonoBehaviour
{
    public static PlantManager Instance;
    public Dictionary<string, List<GameObject>> plants;
    public Vector3 target;
    public float radius;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
    }

    private void Update()
    {
    }

    public void PlantUpdate(Vector3 position)
    {
        target = position;
        ServiceScheduler.Instance.Request("plant update", gameObject);
    }
}