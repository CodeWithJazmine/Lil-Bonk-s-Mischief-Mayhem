using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    SphereCollider sphereCollider;
    public bool playerIsDetected = false;
    public float playerDetectionCooldown = 5f;
    float currentCooldown = 0;
    Transform player;

    private void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.enabled = false;
    }

    private void Update()
    {
        // If we have spotted the player, the sphere collider is turned on, then we start a cooldown.
        // After the cooldown if the player isn't around, set playerIsDetected to false, deactivate
        // the sphere collider and wait until the player re-enters the sight cone. 
        if(playerIsDetected)
        {
            currentCooldown += Time.deltaTime;
            if(currentCooldown > playerDetectionCooldown )
            {
                Collider[] col = Physics.OverlapSphere(transform.position, sphereCollider.radius);
                foreach(Collider c in col )
                {
                    if (c.CompareTag("Player"))
                    {
                        currentCooldown = 0;
                        return;
                    }
                }

                playerIsDetected = false;
                sphereCollider.enabled = false;
            }
        }
    }
    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player") && !playerIsDetected)
        {
            player = col.transform;
            currentCooldown = 0;
            playerIsDetected = true;
            sphereCollider.enabled = true;
        }
    }
}
