using UnityEngine;
using System.Collections.Generic;

public class ChunkCreatureManager : MonoBehaviour
{
    [SerializeField]
    private int count;
    private List<GameObject> creatures;
    private Marcher marcher;

    private void Awake()
    {
        marcher = GetComponent<Marcher>();
        creatures = new List<GameObject>();
    }

    private void Update()
    {

    }

    private Ray RandomSpawnPoint()
    {
        return new Ray(Vector3.zero, Vector3.zero);
    }

    public void CreateCreatures()
    {
        ReleaseCreatures();
        for (int i = 0; i < count; i++)
        {
            GameObject k = EntityManager.Instance.Retrieve("fish");
            if (k != null)
            {
                Ray site = RandomSpawnPoint();
                k.transform.position = site.origin - site.direction * .1f;
                k.transform.rotation = Quaternion.LookRotation(Vector3.Cross(site.direction, Vector3.forward), site.direction);
                creatures.Add(k);
            }
        }
    }

    public void ReleaseCreatures()
    {
        for (int i = 0; i < creatures.Count; i++)
        {
            EntityManager.Instance.Release("fish", creatures[i]);
        }
        creatures.Clear();
    }
}