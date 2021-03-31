using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NavController : MonoBehaviour
{
    #region PUBLIC
    public bool pursue;
    public float height;
    public float moveSpeed;
    public float rotateSpeed;
    public int resolution;
    public Vector3 target;
    #endregion
    [HideInInspector]
    #region PRIVATE
    new private Rigidbody rigidbody;
    private Vector3[] floorCasts;
    #endregion

    private void Awake()
    {
        floorCasts = new Vector3[resolution * resolution];
        rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
    }

    private void Update()
    {
        Transform();
        if (pursue) rigidbody.AddForce(transform.forward * Time.deltaTime * moveSpeed);
    }

    private void OnDrawGizmos()
    {
        foreach (Vector3 v in floorCasts) Gizmos.DrawSphere(v, .25f);
    }


    public void Move(Vector3 destination)
    {
        Debug.DrawLine(transform.position, destination, Color.black);
        target = destination;
        pursue = true;
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
                if (ChunkManager.Instance.Raycast(transform.position + transform.TransformVector(new Vector3(x - (float)castSize.x * .5f + (castSize.x % 2 == 0 ? .5f : 0), 0, (float)y - castSize.y * .5f + (castSize.y % 2 == 0 ? .5f : 0))), -transform.up, 10f, out chipCastHit))
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

    private void Transform()
    {
        Ray floor = FloorSurvey();
        Quaternion targetRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(target - transform.position, floor.direction), floor.direction);
        float moveSpeed = Mathf.Clamp(50f / Quaternion.Angle(transform.rotation, targetRot), 1f, 3f);
        rigidbody.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeed));

        Vector3 translation = transform.TransformVector(Vector3.up * (height + transform.InverseTransformPoint(floor.origin).y) * Time.deltaTime * .2f);
        if (pursue) translation += transform.forward * Time.deltaTime * moveSpeed;

        rigidbody.MovePosition(transform.position + translation);
    }
}