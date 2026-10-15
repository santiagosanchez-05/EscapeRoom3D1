using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Tooltip("Velocidad de movimiento del jugador")]
    public float speed = 5f;

    [Tooltip("Sensibilidad del mouse para mirar alrededor")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private Transform cameraPivot;

    private CharacterController characterController;
    private float pitch;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        if (cameraPivot == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
                cameraPivot = cam.transform;
        }

        LockCursor(true);
    }

    private void Update()
    {
        GameUI ui = GameUI.Instance;
        if (ui != null && ui.IsBlockingInput)
            return;

        HandleCursor();
        Look();
        Move();
    }

    private void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? speed * 2f : speed;

        Vector3 movement = new Vector3(h, 0f, v);
        if (movement.magnitude > 1f) movement.Normalize();

        if (characterController != null)
        {
            // SimpleMove aplica gravedad y respeta las colisiones con paredes y objetos.
            characterController.SimpleMove(transform.TransformDirection(movement) * currentSpeed);
        }
        else
        {
            transform.Translate(movement * currentSpeed * Time.deltaTime);
        }
    }

    private void Look()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(0f, mouseX, 0f);

        pitch = Mathf.Clamp(pitch - mouseY, -80f, 80f);
        if (cameraPivot != null)
            cameraPivot.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    private void HandleCursor()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            LockCursor(false);
        else if (Input.GetMouseButtonDown(0))
            LockCursor(true);
    }

    public static void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
