using UnityEngine;
using System.Collections.Generic;
class CreatureManager : MonoBehaviour
{
    public static CreatureManager Instance;

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
}