using UnityEngine;
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region PUBLIC
    public Transform feet;
    public Camera cam;
    public float lookSpeed;
    public float moveSpeed;
    public float jumpSpeed;
    #endregion
    #region PRIVATE
    private Vector3 momentum;
    public float airTime;
    private float upVelocity;
    private Vector2 rotation;
    private bool onGround;
    private Vector3 feetBox;
    private CharacterController characterController;
    #endregion
    private void Awake()
    {
        momentum = Vector3.zero;
        airTime = 0f;
        characterController = GetComponent<CharacterController>();
        rotation = Vector2.zero;
        onGround = false;
        feetBox = new Vector3(0.8f, 0.5f, 0.8f);
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(feet.position, feetBox);
    }

    private void Update()
    {
        MoveHandler();
        LookHandler();
    }

    private void MoveHandler()
    {
        Vector3 translation = Vector3.zero;
        if (Physics.CheckBox(feet.position, feetBox, transform.rotation, LayerMask.NameToLayer("Player")))
        {
            if (!onGround) onGround = true;
            if (upVelocity < 0) upVelocity = 0f;
            momentum = (Input.GetAxis("Vertical") * transform.forward + Input.GetAxis("Horizontal") * transform.right) * moveSpeed;
        }
        else
        {
            if (onGround)
            {
                onGround = false;
                airTime = 0f;
            }
            else airTime += Time.deltaTime;
            upVelocity -= 20f * Time.deltaTime;
            momentum += (transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical")) * moveSpeed / ((airTime + 1f) * 20f);
            momentum = Vector3.ClampMagnitude(momentum, moveSpeed);
        }

        translation += momentum * Time.deltaTime;
        translation += Vector3.up * upVelocity * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space) && onGround) upVelocity = jumpSpeed;
        // if (Input.GetKey(KeyCode.C)) translation += Vector3.down * Time.deltaTime * 9f;

        characterController.Move(translation);
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
