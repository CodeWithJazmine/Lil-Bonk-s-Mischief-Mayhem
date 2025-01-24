using UnityEngine;

public class ExplodeOnCommand : MonoBehaviour
{
    public float radius;
    public float force = 1000f;
    public bool explode = false;

    void Update()
    {
        if(explode)
        {
            var hits = Physics.OverlapSphere(transform.position, radius);
            foreach(var hit in hits)
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if(rb != null )
                {
                    rb.AddExplosionForce(force, transform.position, radius);
                }
            }

            explode = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
