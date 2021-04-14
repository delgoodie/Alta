using System.Collections.Generic;
using UnityEngine;

public class PlantUpdateService : MonoBehaviour, IService
{

    private void Awake()
    {
    }

    public void Execute(GameObject gameObject)
    {
        PlantManager pm = gameObject.GetComponent<PlantManager>();
        if (pm == null) return;

        foreach (KeyValuePair<string, List<GameObject>> pair in pm.plants)
        {
            for (int i = 0; i < pair.Value.Count; i++)
            {
                if ((pair.Value[i].transform.position - pm.target).sqrMagnitude > pm.radius * pm.radius)
                {
                    EntityManager.Instance.Release(pair.Key, pair.Value[i]);
                    pair.Value.RemoveAt(i);
                    i--;
                }
            }
        }
        //TODO: Add in new plants, raycast randomly in radius 
    }
}
