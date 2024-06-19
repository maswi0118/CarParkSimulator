using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float movementSpeed = 10.0f;
    public float mouseSensitivity = 100.0f;
    public float maxPitch = 80.0f;
    public float minPitch = -80.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    private bool followMouse = true; // Flag to determine if camera should follow mouse

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            followMouse = !followMouse; // Toggle followMouse flag
            Cursor.lockState = followMouse ? CursorLockMode.Locked : CursorLockMode.None; // Lock or unlock cursor based on followMouse
        }

        if (followMouse)
        {
            // Mouse rotation
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            yaw += mouseX;
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
    }

    void LateUpdate()
    {
        if (followMouse)
        {
            // Apply rotation in LateUpdate for smoother results
            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }
    }

    void FixedUpdate()
    {
        // Keyboard movement in FixedUpdate for smoother physics-based movement
        float moveForward = Input.GetAxis("Vertical") * movementSpeed * Time.fixedDeltaTime;
        float moveRight = Input.GetAxis("Horizontal") * movementSpeed * Time.fixedDeltaTime;

        Vector3 move = transform.right * moveRight + transform.forward * moveForward;
        transform.position += move;

        // Move up and down
        if (Input.GetKey(KeyCode.Q))
        {
            transform.position += transform.up * movementSpeed * Time.fixedDeltaTime;
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.position -= transform.up * movementSpeed * Time.fixedDeltaTime;
        }
    }
}