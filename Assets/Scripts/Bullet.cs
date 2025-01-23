using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float bulletSpeed = 10f; // Impulse
    public float bulletForce = 20f; // Impulse
    public GameObject hitEffect;
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        rb.AddForce(transform.forward * bulletSpeed, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("Player"))
        {
            collision.collider.GetComponent<Rigidbody>().AddForce(transform.forward * bulletForce, ForceMode.Impulse);
        }

        else if(collision.collider.CompareTag("Enemy"))
        {
            collision.collider.GetComponent<EnemyStateMachine>().OnBonked(1f);
        }

        Instantiate(hitEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
