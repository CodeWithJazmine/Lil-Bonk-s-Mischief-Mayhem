using UnityEngine;

public class BreakableObject : MonoBehaviour, IBonkable
{
    DinoFracture.FractureGeometry fracture;
    [SerializeField] float hp = 1.0f;
    [SerializeField] bool broken = false;
    [SerializeField] Transform explosionPoint;
    [SerializeField] float explosionImpulse = 10f;
    [SerializeField] float explosionUpwardModifier = 1f;
    [SerializeField] float explosionRadius = 3f;
    [SerializeField] bool testBreak = false;
    [SerializeField] float destroyTime = 10f;
    [SerializeField] LayerMask mask;

    private void Awake()
    {
        fracture = GetComponent<DinoFracture.FractureGeometry>();
    }

    void Start()
    {
        if (explosionPoint == null) explosionPoint = transform;
    }

    void Update()
    {
        if(testBreak)
        {
            OnBonked(hp);
            testBreak = false;
        }
    }

    public void OnBonked(float value)
    {
        if (broken) return; // Already broken, don't respond to bonking
        Debug.Log("Bonked with value: " + value.ToString());
        hp = hp - value;

        if(hp <= 0.0f)
        {
            if(fracture != null)
                fracture.Fracture();

            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, mask);

            foreach(var col in colliders)
            {
                Rigidbody rb;
                col.TryGetComponent<Rigidbody>(out rb);
                if(rb)
                    rb.AddExplosionForce(explosionImpulse, explosionPoint.position, explosionRadius, explosionUpwardModifier, ForceMode.Impulse);
            }

            broken = true;
        }
    }
}
