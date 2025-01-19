using UnityEngine;

public class MalletRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 360f;
    public float returnSpeed = 5f; // Speed at which it returns to upright
    private float xRotation = 0f;
    private bool isMoving = false;

    private void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        isMoving = (moveX != 0 || moveZ != 0);

        if (isMoving)
        {
            // Continue forward rotation while moving
            xRotation = (xRotation + rotationSpeed * Time.deltaTime) % 360f;
            transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        }
        else
        {
            // Smoothly return to upright when not moving
            Quaternion targetRotation = Quaternion.Euler(0, 0, 0);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, returnSpeed * Time.deltaTime);

            // Update xRotation to match current rotation for smooth transition when moving again
            xRotation = transform.localRotation.eulerAngles.x;
        }
    }
}