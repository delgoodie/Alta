using UnityEngine;

public class NavTester : MonoBehaviour, INavigator
{
    public Navigator navigator;
    public GameObject targetGO;

    private void Awake()
    {
        navigator = GetComponent<Navigator>();
        targetGO = GameObject.Find("target");
    }

    private void Start()
    {
        navigator.pursue = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) navigator.Move(targetGO.transform.position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(transform.position, Vector3.one);
    }
}