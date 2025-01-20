using UnityEngine;

public class MalletRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 360f;
    public float returnSpeed = 5f; // speed at which it returns to upright
    private float xRotation = 0f;
    public bool IsMoving { get; private set; }

    private void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        IsMoving = (moveX != 0 || moveZ != 0);

        if (IsMoving)
        {
            // continue forward rotation while moving
            xRotation = (xRotation + rotationSpeed * Time.deltaTime) % 360f;
            transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        }
        else
        {
            // smoothly return to upright when not moving
            Quaternion targetRotation = Quaternion.Euler(0, 0, 0);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, returnSpeed * Time.deltaTime);

            // update xRotation to match current rotation for smooth transition when moving again
            xRotation = transform.localRotation.eulerAngles.x;
        }
    }
}