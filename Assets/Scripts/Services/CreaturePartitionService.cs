using UnityEngine;
using System.Collections.Generic;


public class CreaturePartitionService : MonoBehaviour, IService
{
    private void Awake()
    {
    }

    public void Execute(GameObject gameObject)
    {
        CreatureManager cm = gameObject.GetComponent<CreatureManager>();
        if (cm == null) return;
    }
}