using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public GameObject target;
    public Vector3 offset;

    [Header("Distance Settings")]
    [Tooltip("How far the camera stays from the player")]
    [Range(2f, 10f)]
    public float distanceFromPlayer = 5f;

    [Header("Rotation Settings")]
    [Tooltip("Mouse look sensitivity")]
    [Range(50f, 200f)]
    public float mouseSensitivity = 100f;

    [Header("Angle Constraints")]
    [Tooltip("Maximum upward angle in degrees")]
    [Range(0f, 90f)]
    public float maxUpwardAngle = 50f;

    [Tooltip("Maximum downward angle in degrees")]
    [Range(-30f, 0f)]
    public float maxDownwardAngle = 0f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        // validate the initial position
        Vector3 initialDirection = transform.position - target.transform.position;
        float initialVerticalAngle = Vector3.SignedAngle(Vector3.forward, Vector3.ProjectOnPlane(initialDirection, Vector3.right), Vector3.right);
        rotationX = Mathf.Clamp(initialVerticalAngle, -maxDownwardAngle, maxUpwardAngle);
    }

    void Update()
    {
        if (target == null) return;

        // get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // update rotation values
        rotationY += mouseX; // Horizontal rotation is unlimited
        rotationX = Mathf.Clamp(rotationX - mouseY, -maxDownwardAngle, maxUpwardAngle); // vertical rotation is clamped

        // calculate camera position
        Vector3 direction = new Vector3(0, 0, -distanceFromPlayer);
        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);

        // set position and look at target
        transform.position = target.transform.position + offset + rotation * direction;
        transform.LookAt(target.transform.position + offset);
    }

    // useful for visualizing the camera bounds in the editor
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;

        Gizmos.color = Color.yellow;
        Vector3 position = target.transform.position + offset;

        // draw lines showing the up and down limits
        Quaternion upRotation = Quaternion.Euler(-maxUpwardAngle, rotationY, 0);
        Quaternion downRotation = Quaternion.Euler(maxDownwardAngle, rotationY, 0);

        Vector3 upDirection = upRotation * Vector3.forward * distanceFromPlayer;
        Vector3 downDirection = downRotation * Vector3.forward * distanceFromPlayer;

        Gizmos.DrawLine(position, position + upDirection);
        Gizmos.DrawLine(position, position + downDirection);
    }
}