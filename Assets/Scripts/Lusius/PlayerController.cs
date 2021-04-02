using UnityEngine;
public class PlayerController : MonoBehaviour
{
    #region PUBLIC
    public Camera cam;
    public float lookSpeed;
    public float rotationSpeed;
    public float moveSpeed;
    #endregion
    #region PRIVATE
    private Vector3 momentum;
    private Vector2 rotation;
    new private Rigidbody rigidbody;
    #endregion
    private void Awake()
    {
        momentum = Vector3.zero;
        rotation = Vector2.zero;
        rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDrawGizmos()
    {
    }

    private void FixedUpdate()
    {
        MoveHandler();
        // LookHandler();
    }

    private void MoveHandler()
    {
        Vector3 translation = Vector3.zero;
        Vector3 rotation = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) translation += transform.forward * moveSpeed;
        if (Input.GetKey(KeyCode.S)) translation += -transform.forward * moveSpeed;

        if (Input.GetKey(KeyCode.A)) rotation += -transform.up * rotationSpeed;
        if (Input.GetKey(KeyCode.D)) rotation += transform.up * rotationSpeed;

        if (Input.GetKey(KeyCode.LeftAlt))
        {

        }
        else
        {

        }

        if (!translation.Equals(Vector3.zero)) rigidbody.AddForce(translation * Time.deltaTime);
        if (!rotation.Equals(Vector3.zero)) rigidbody.AddTorque(rotation * Time.deltaTime);
    }

    private void LookHandler()
    {
        rotation.y += Input.GetAxis("Mouse X");
        rotation.x += -Input.GetAxis("Mouse Y");
        cam.transform.eulerAngles = (Vector2)rotation * lookSpeed * new Vector2(1, 1);
        transform.eulerAngles = (Vector2)rotation * lookSpeed * new Vector2(0, 1);
    }


    private void InteractHandler()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = new Ray(cam.transform.position, cam.transform.transform.forward);
            RaycastHit hitInfo = new RaycastHit();
            if (Physics.Raycast(ray, out hitInfo, 16f))
            {
                //Chunk c;
                //Plant p;
                //if (hitInfo.transform.TryGetComponent<Chunk>(out c)) c.Subtract(hitInfo.point, 1.2f, 1f);
                //if (hitInfo.transform.TryGetComponent<Plant>(out p)) p.Subtract(hitInfo.point, .3f, 1f);

            }
        }
    }
}
