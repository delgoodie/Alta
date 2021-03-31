using System.Collections.Generic;
using UnityEngine;

public class Omni : MonoBehaviour
{
    private GameObject navTester;
    private List<GameObject> navTesters;
    private void Start()
    {
        navTester = Resources.Load("Prefabs/Monsters/Spider/Spider") as GameObject;
        navTesters = new List<GameObject>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 30f)) navTesters.Add(Instantiate(navTester, hit.point + hit.normal * 3f, Quaternion.LookRotation(Vector3.Cross(hit.normal, Vector3.up), hit.normal)));
        }
        else if (Input.GetMouseButtonDown(1))
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 30f)) foreach (GameObject s in navTesters) s.GetComponent<Navigator>().Move(hit.point);
    }

    void OnCollisionEnter(Collision collision)
    {

    }
}
