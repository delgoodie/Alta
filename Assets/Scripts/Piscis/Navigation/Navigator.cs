using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(INavigator))]
[RequireComponent(typeof(NavController))]
public class Navigator : MonoBehaviour
{
    #region PUBLIC
    public bool pursue;
    public Vector3Int box;
    public Vector3 target;
    #endregion
    [HideInInspector]
    public List<Vector3> stack; //Keeps track of waypoints. last index will be final target, Navigator will add midpoints before
    [HideInInspector]
    public List<Vector3Int> used;
    [HideInInspector]
    public List<Vector3Int> headPath;
    [HideInInspector]
    public List<Vector3> pathStack;
    private NavController navController;

    private void Awake()
    {
        used = new List<Vector3Int>();
        headPath = new List<Vector3Int>();
        pathStack = new List<Vector3>();
        stack = new List<Vector3>();
        navController = GetComponent<NavController>();
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        //if (stack.Count > 0) rigidbody.AddTorque(transform.up * .1f * Vector3.SignedAngle(transform.forward, (stack[0] - transform.position).normalized, transform.up));

        //if (stack.Count > 0 && rigidbody.velocity.sqrMagnitude < 1f && ChunkManager.Instance.Raycast(transform.position, transform.forward, radius))
        //    if (ChunkManager.Instance.Raycast(transform.position, -transform.up, radius))
        //        rigidbody.AddForce((transform.up * 2f + transform.forward) * Time.deltaTime * 400f, ForceMode.Impulse);   
        if (stack.Count > 0 && pursue)
        {
            navController.Move(stack[0]);
            float targetAngle = Vector3.Angle(transform.up, stack[0] - transform.position);

            if ((transform.position - stack[0]).sqrMagnitude < 3f * 3f) stack.RemoveAt(0);
            else if (targetAngle > 10f && targetAngle < 170f) //if current waypoint is not above or below transform
            { }
            //else stack.Insert(0, intermediate()); // add intermediate waypoint to avoid algorithm locking
        }
    }

    private void OnDrawGizmos()
    {
        if (NavigationManager.Instance.gizmosEnabled)
        {
            Gizmos.color = Color.white;
            if (stack.Count > 0)
            {
                foreach (Vector3 v in stack) Gizmos.DrawCube(v, Vector3.one * 2f);
                Gizmos.DrawLine(transform.position, stack[0]);
                Gizmos.DrawLine(stack[0], transform.position + transform.up * 10f);
                Gizmos.DrawLine(transform.position, transform.position + transform.up * 10f);
            }
            foreach (Vector3Int v in used) Gizmos.DrawWireCube(v, Vector3.one);
            Gizmos.color = Color.red;
            foreach (Vector3Int v in headPath) Gizmos.DrawCube(v, Vector3.one);
            Gizmos.color = Color.blue;
            foreach (Vector3 v in pathStack) Gizmos.DrawSphere(v, 1f);
            Gizmos.color = Color.white;
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, box);
    }

    public void Init()
    {
    }

    public void Move(Vector3 destination)
    {
        target = destination;
        stack.Clear();
        NavigationManager.Instance.RequestNavigation(this);
        pursue = true;
        navController.Move(destination);
    }

    private Vector3 Intermediate()
    {
        if (Physics.Linecast(transform.position, stack[0], ~(1 << LayerMask.NameToLayer("Monster"))))
        {
            float aRes = 20f;
            float distRes = 5f;
            float maxDist = 200f;
            for (float a = 0f; a < 360f; a += aRes)
            {
                Vector3 dir = Quaternion.AngleAxis(a, transform.up) * transform.forward;
                for (float d = 0f; d < maxDist; d += distRes)
                {
                    Vector3 pos = transform.position + dir * d;
                    if (Physics.OverlapSphere(pos, 20f).Length < 0) break;

                    if (!Physics.Linecast(pos, stack[0], ~(1 << LayerMask.NameToLayer("Monster")))) return pos + dir * 2f;
                }
            }
            stack.RemoveAt(0);
            return transform.position;
        }
        else
        {
            Vector3 min = Vector3.zero;
            float aRes = 20f;
            for (float a = 0f; a < 360f; a += aRes)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Quaternion.AngleAxis(a, transform.up) * transform.forward, out hit, 200f, ~(1 << LayerMask.NameToLayer("Monster"))))
                    if ((hit.point - stack[0]).sqrMagnitude < (min - stack[0]).sqrMagnitude || min == Vector3.zero) min = hit.point;
            }
            if (min == Vector3.zero)
            {
                stack.RemoveAt(0);
                return transform.position;
            }
            return min + (min - transform.position).normalized * 10f;
        }
    }
}