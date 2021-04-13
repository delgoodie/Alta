using UnityEngine;
using System.Collections.Generic;
class CreatureManager : EntityManager
{
    public static CreatureManager Instance;

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