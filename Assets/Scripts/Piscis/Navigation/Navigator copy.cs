using System.Collections.Generic;
using UnityEngine;

public class Navigator_copy : MonoBehaviour
{
    #region PUBLIC
    public bool pursue;
    public float height;
    public Vector3Int box;
    public float moveSpeed;
    public float rotateSpeed;
    public float heightDampner;
    public float rotationDampner;
    public float minSpeed;
    public float maxSpeed;
    public int resolution; //the number of rays per side length
    public Vector3 target;
    #endregion
    [HideInInspector]
    public List<Vector3> stack; //Keeps track of waypoints. last index will be final target, Navigator will add midpoints before
    #region PRIVATE
    private Vector3[] floorCasts; //The fixed, local directions of the downward raycast which creates a terrain "image"
    [HideInInspector]
    public List<Vector3Int> used;
    [HideInInspector]
    public List<Vector3Int> headPath;
    [HideInInspector]
    public List<Vector3> pathStack;
    #endregion

    private void Awake()
    {
        used = new List<Vector3Int>();
        headPath = new List<Vector3Int>();
        pathStack = new List<Vector3>();
        stack = new List<Vector3>();
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        Ray floor = FloorSurvey();
        Rotation(floor);
        Translation(floor);

        if (stack.Count > 0 && pursue)
        {
            float targetAngle = Vector3.Angle(transform.up, stack[0] - transform.position);

            if ((transform.position - stack[0]).sqrMagnitude < 5f * 5f) stack.RemoveAt(0);
            else if (targetAngle > 10f && targetAngle < 170f) //if current waypoint is not above or below transform
            {
                Plane targetPlane = new Plane(transform.up, transform.position);
                Vector3 projectedTarget = targetPlane.ClosestPointOnPlane(stack[0]);
                transform.Rotate(Vector3.up * Mathf.Clamp(Vector3.SignedAngle(transform.forward, projectedTarget - transform.position, transform.up), -rotateSpeed, rotateSpeed), Space.Self);
            }
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

        foreach (Vector3 v in floorCasts) Gizmos.DrawSphere(v, .25f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, box);
    }

    public void Init()
    {
        floorCasts = new Vector3[resolution * resolution];
    }

    public void Move(Vector3 destination)
    {
        target = destination;
        stack.Clear();
        //NavigationManager.Instance.RequestNavigation(this);
        pursue = true;
    }

    private void Rotation(Ray floor)
    {
        Quaternion targetRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(stack.Count > 0 ? stack[0] - transform.position : transform.forward, floor.direction), floor.direction);
        moveSpeed = Mathf.Clamp(rotationDampner / Quaternion.Angle(transform.rotation, targetRot), minSpeed, maxSpeed);
        TryRotate(Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeed));
    }

    private void Translation(Ray floor)
    {
        Vector3 translation = transform.TransformVector(Vector3.ClampMagnitude(new Vector3(0, height + transform.InverseTransformPoint(floor.origin).y, 0) * Time.deltaTime * heightDampner, 1f));
        if (pursue) translation += transform.forward * Time.deltaTime * moveSpeed;

        TryTranslate(translation);
        Debug.DrawRay(transform.position, translation * 5f, Color.blue);
    }

    public void TryTranslate(Vector3 translation)
    {
        float magnitude = translation.magnitude;
        if (magnitude < .001f) return;
        bool allowed = true;
        for (int x = 0; x < box.x; x++)
        {
            for (int y = 0; y < box.y; y++)
            {
                for (int z = 0; z < box.z; z++)
                {
                    Vector3 worldPos = transform.TransformPoint(new Vector3(x - (float)box.x * .5f + .5f, y - (float)box.y * .5f + .5f, z - (float)box.z * .5f + .5f));
                    if (ChunkManager.Instance.Raycast(worldPos, translation, magnitude))
                    {
                        allowed = false;
                        break;
                    }
                }
                if (!allowed) break;
            }
            if (!allowed) break;
        }

        if (allowed || true) //     ******ALWAYS TRUE******
        {
            transform.Translate(translation, Space.World);
        }
    }

    public void TryRotate(Quaternion rotation)
    {
        //Quaternion oldRot = transform.rotation;
        transform.rotation = rotation;
        //if (!BoxCheck()) transform.rotation = oldRot;

    }

    public bool BoxCheck()
    {
        return ChunkManager.Instance.BoxCheck(transform.position, transform.rotation, box);
    }

    private Ray FloorSurvey()
    {
        Vector2Int castSize = new Vector2Int(resolution, resolution);
        Vector3?[] hits = new Vector3?[castSize.x * castSize.y];
        Vector3 centroid = Vector3.zero;
        int count = 0;
        for (int x = 0; x < castSize.x; x++) for (int y = 0; y < castSize.y; y++)
            {
                ChipCastHit chipCastHit;
                if (ChunkManager.Instance.Raycast(transform.position + transform.TransformVector(new Vector3(x - (float)castSize.x * .5f + (castSize.x % 2 == 0 ? .5f : 0), 0, (float)y - castSize.y * .5f + (castSize.y % 2 == 0 ? .5f : 0))), -transform.up, 20f, out chipCastHit))
                {
                    hits[x * castSize.x + y] = chipCastHit.point;
                    floorCasts[x * castSize.x + y] = chipCastHit.point;
                    centroid += (Vector3)hits[x * castSize.x + y];
                    count++;
                }

            }
        if (count == 0) return new Ray(Vector3.zero, Vector3.zero);
        centroid /= count;
        float xx = 0, yy = 0, zz = 0, xy = 0, xz = 0, yz = 0;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] != null)
            {
                Vector3 r = (Vector3)hits[i] - centroid;
                xx += r.x * r.x;
                xy += r.x * r.y;
                xz += r.x * r.z;
                yy += r.y * r.y;
                yz += r.y * r.z;
                zz += r.z * r.z;
            }
        }

        float detX = yy * zz - yz * yz;
        float detY = xx * zz - xz * xz;
        float detZ = xx * yy - xy * xy;
        float detMax = Mathf.Max(detZ, detX > detY ? detX : detY);

        Vector3 up;
        if (detMax == detX) up = new Vector3(detX, xz * yz - xy * zz, xy * yz - xz * yy);
        else if (detMax == detY) up = new Vector3(xz * yz - xy * zz, detY, xy * xz - yz * xx);
        else up = new Vector3(xy * yz - xz * yy, xy * xz - yz * xx, detZ);
        up.Normalize();
        if (Vector3.Angle(up, transform.position - centroid) > 90f) up = -up;

        Debug.DrawRay(centroid, up, Color.cyan);

        return new Ray(centroid, up);
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