using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float moveSpeed = 5f;
    public float mouseSensitivity = 100f;
    public Transform cameraTransform;
    


   


    // Update is called once per frame
    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        
        //Movement Vector
        Vector3 movement = new Vector3(moveX, 0, moveZ);

        // Calculate movement direction relative to the camera
        movement = cameraTransform.TransformDirection(movement); // Align movement with camera
        movement.y = 0; // Ignore vertical movement (gravity)


      
       

        transform.Translate(movement.normalized * moveSpeed * Time.deltaTime, Space.World);





    }
}
