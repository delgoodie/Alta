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
    private Vector3 camRotation;
    new private Rigidbody rigidbody;
    #endregion
    private void Awake()
    {
        momentum = Vector3.zero;
        camRotation = Vector3.zero;
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

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            camRotation.y = Mathf.Clamp(camRotation.y + Input.GetAxis("Mouse X"), -90f, 90f);
            camRotation.x = Mathf.Clamp(camRotation.x - Input.GetAxis("Mouse Y"), -90f, 90f);
        }
        else
            camRotation *= .75f;
        cam.transform.localEulerAngles = camRotation;
    }

    private void FixedUpdate()
    {
        MoveHandler();
    }

    private void MoveHandler()
    {
        Vector3 translation = Vector3.zero;
        Vector3 rotation = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) translation += transform.forward * moveSpeed;
        if (Input.GetKey(KeyCode.S)) translation += -transform.forward * moveSpeed;

        if (Input.GetKey(KeyCode.LeftShift)) translation *= 5f;

        if (Input.GetKey(KeyCode.A)) rotation += transform.forward * rotationSpeed;
        if (Input.GetKey(KeyCode.D)) rotation += -transform.forward * rotationSpeed;


        if (!Input.GetKey(KeyCode.LeftAlt))
        {
            rotation += transform.up * Input.GetAxis("Mouse X") * lookSpeed * .4f;
            rotation += -transform.right * Input.GetAxis("Mouse Y") * lookSpeed;
        }

        if (!translation.Equals(Vector3.zero)) rigidbody.AddForce(translation * Time.deltaTime);
        if (!rotation.Equals(Vector3.zero)) rigidbody.AddTorque(rotation * Time.deltaTime);
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
