using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    

    
    public Vector3 offSet;
    [Header("References")]
    public GameObject target;
    public float mouseSensitivity = 100f; // Sensitivity for mouse movement
    public float distanceFromPlayer = 5f; // Distance from the player
    public float verticalAngleLimit = 80f; // Limit for vertical camera rotation

    private float rotationX = 0f;
    private float rotationY = 0f;

    

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {
        transform.position = target.transform.position + offSet;
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        rotationY += mouseX;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -verticalAngleLimit, verticalAngleLimit);

        Vector3 direction = new Vector3(0, 0, -distanceFromPlayer);
        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);
        transform.position = target.transform.position + rotation * direction;
        transform.LookAt(target.transform); // Keep the camera facing the player

    }
}
