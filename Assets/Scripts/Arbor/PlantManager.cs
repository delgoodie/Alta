using System.Collections.Generic;
using UnityEngine;

class PlantManager : EntityManager
{
    public static PlantManager Instance;
    private void Awake()
    {
        Instance = this;
        base.Init();
    }

    private void Start()
    {
    }

    private void Update()
    {
    }
}