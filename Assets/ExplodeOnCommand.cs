using UnityEngine;

public class ExplodeOnCommand : MonoBehaviour
{
    public float radius;
    public float force = 1000f;
    public bool explode = false;
    float delay = 1f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            explode = true;

        if(explode)
        {
            delay -= Time.deltaTime;
            if(delay < 0f)
            {
                var hits = Physics.OverlapSphere(transform.position, radius);
                foreach (var hit in hits)
                {
                    Rigidbody rb = hit.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.AddExplosionForce(force, transform.position, radius);
                    }
                }

                delay = 1f;
                explode = false;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
