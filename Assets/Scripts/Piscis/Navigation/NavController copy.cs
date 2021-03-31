using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NavController_copy : MonoBehaviour
{
    #region PUBLIC
    public bool pursue;
    public float radius;
    public float moveSpeed;
    public float rotateSpeed;
    public int resolution;
    public float pull;
    public float push;
    public Vector3 target;
    #endregion
    [HideInInspector]
    #region PRIVATE
    private Ray[] spherePoints;
    new private Rigidbody rigidbody;
    private int mindex = 0;
    private Vector3[] floorCasts;
    #endregion

    private void Awake()
    {
        floorCasts = new Vector3[6 * 6];
        rigidbody = GetComponent<Rigidbody>();
        spherePoints = new Ray[resolution];
        float goldenRatio = 2f / (1f + 5f * Mathf.Sqrt(.5f));
        for (int i = 0; i < resolution; i++)
        {
            float phi = Mathf.Acos(1f - 2f * ((float)i + .5f) / resolution);
            float theta = 2f * Mathf.PI * ((float)i + .5f) * goldenRatio;
            spherePoints[i] = new Ray(Vector3.zero, new Vector3(Mathf.Cos(theta) * Mathf.Sin(phi), Mathf.Sin(theta) * Mathf.Sin(phi), Mathf.Cos(phi)));
        }
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        //Repel();
        Old();
        if (pursue)
        {
            rigidbody.AddForce(transform.forward * Time.deltaTime * moveSpeed);
            // rigidbody.AddForce((target - transform.position).normalized * Time.deltaTime * moveSpeed);
            Debug.DrawRay(transform.position, transform.forward, Color.blue);
            Debug.DrawRay(transform.position, transform.up, Color.green);
            Debug.DrawRay(transform.position, transform.right, Color.red);

            //rigidbody.AddForce(-transform.up * Time.deltaTime * 400);
            //Debug.DrawRay(transform.position, -transform.up * Time.deltaTime * 400, Color.black);


            //if (stack.Count > 0) rigidbody.AddTorque(transform.up * .1f * Vector3.SignedAngle(transform.forward, (stack[0] - transform.position).normalized, transform.up));

            //if (stack.Count > 0 && rigidbody.velocity.sqrMagnitude < 1f && ChunkManager.Instance.Raycast(transform.position, transform.forward, radius))
            //    if (ChunkManager.Instance.Raycast(transform.position, -transform.up, radius))
            //        rigidbody.AddForce((transform.up * 2f + transform.forward) * Time.deltaTime * 400f, ForceMode.Impulse);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        foreach (Ray r in spherePoints) Gizmos.DrawSphere(r.origin, .1f);

        Gizmos.color = Color.gray;
        if (mindex != -1) Gizmos.DrawSphere(spherePoints[mindex].origin, .5f);

        foreach (Vector3 v in floorCasts) Gizmos.DrawSphere(v, .25f);
    }

    public void Init()
    {
    }

    public void Move(Vector3 destination)
    {
        Debug.Log(destination);
        Debug.DrawLine(transform.position, destination, Color.black);
        target = destination;
        pursue = true;
    }

    private void Repel()
    {
        ChipCastHit chip;
        mindex = -1;
        Vector3 targetDirection = (target - transform.position).normalized;
        for (int i = 0; i < spherePoints.Length; i++)
        {
            if (ChunkManager.Instance.Raycast(transform.position, transform.TransformDirection(spherePoints[i].direction), radius * 2f, out chip))
            {
                spherePoints[i].origin = chip.point;
                if (mindex == -1 || (spherePoints[i].origin - transform.position).sqrMagnitude < (spherePoints[mindex].origin - transform.position).sqrMagnitude) mindex = i;
                float scalar = (chip.distance - radius < 0 ? push : pull) * (chip.distance - radius);
                rigidbody.AddForce((spherePoints[i].origin - transform.position).normalized * scalar);
            }
            else
            {
                spherePoints[i].origin = transform.position + transform.TransformDirection(spherePoints[i].direction) * radius;
            }
        }



        if (mindex == -1) return;

        Quaternion targetRot = Quaternion.LookRotation(targetDirection, -spherePoints[mindex].direction);
        rigidbody.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeed));
    }

    private Ray FloorSurvey()
    {
        int resolution = 6;
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


    private void Old()
    {
        Ray floor = FloorSurvey();
        Quaternion targetRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(target - transform.position, floor.direction), floor.direction);
        float moveSpeed = Mathf.Clamp(50f / Quaternion.Angle(transform.rotation, targetRot), 1f, 3f);
        rigidbody.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeed));

        Vector3 translation = transform.TransformVector(Vector3.ClampMagnitude(new Vector3(0, radius + transform.InverseTransformPoint(floor.origin).y, 0) * Time.deltaTime * .2f, 1f));
        if (pursue) translation += transform.forward * Time.deltaTime * moveSpeed;

        rigidbody.MovePosition(transform.position + translation);
        Debug.DrawRay(transform.position, translation * 5f, Color.blue);
    }
}