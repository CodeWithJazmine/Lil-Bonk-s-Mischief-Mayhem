using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Movement speed
    public Transform cameraTransform; // Reference to the camera

    private void Update()
    {
        // Get movement input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Calculate movement direction relative to the camera
        Vector3 movement = new Vector3(moveX, 0, moveZ);
        movement = cameraTransform.TransformDirection(movement); // Align movement with camera
        movement.y = 0; // Ignore vertical movement (gravity)

        // Move the player
        transform.Translate(movement.normalized * moveSpeed * Time.deltaTime, Space.World);

        // Rotate the player to face the movement direction
        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // Debug controls
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    GameManager.Instance.HandleBonk(100);
        //    GameManager.Instance.PlayBonkSound();
        //}

    }

}