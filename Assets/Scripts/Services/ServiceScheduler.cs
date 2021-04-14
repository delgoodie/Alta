using UnityEngine;
using System.Collections.Generic;
using System;
public class ServiceScheduler : MonoBehaviour
{
    public static ServiceScheduler Instance;
    public bool servicing;
    [SerializeField]
    private string[] keys;
    [SerializeField]
    private GameObject[] serviceGameObjects;
    private IService[] services;
    private Queue<Tuple<string, GameObject>> servees;
    private void Awake()
    {
        if (serviceGameObjects.Length != keys.Length) Debug.LogError("services length does not match keys length");
        services = new IService[keys.Length];
        Instance = this;
        for (int i = 0; i < serviceGameObjects.Length; i++)
            services[i] = serviceGameObjects[i].GetComponent<IService>();
        servicing = true;
        servees = new Queue<Tuple<string, GameObject>>();
    }

    private void Update()
    {
        servicing = servees.Count > 0;
        if (servicing)
        {
            Tuple<string, GameObject> s = servees.Dequeue();
            for (int i = 0; i < keys.Length; i++)
                if (s.Item1.Equals(keys[i]))
                {
                    services[i].Execute(s.Item2);
                    break;
                }
        }
    }

    public void Request(string service, GameObject gameObject)
    {
        Tuple<string, GameObject> entry = new Tuple<string, GameObject>(service, gameObject);
        if (!servees.Contains(entry)) servees.Enqueue(entry);
    }
}