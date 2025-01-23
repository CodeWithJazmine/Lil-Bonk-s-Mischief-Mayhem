using UnityEngine;

public class BreakableObject : MonoBehaviour, IBonkable
{
    public Rigidbody[] rbs;
    [SerializeField] float hp = 1.0f;
    [SerializeField] bool broken;
    [SerializeField] Transform explosionPoint;
    [SerializeField] float explosionImpulse = 10f;
    [SerializeField] float explosionUpwardModifier = 1f;
    [SerializeField] float explosionRadius = 3f;
    [SerializeField] bool testBreak = false;
    [SerializeField] float destroyTime = 10f;

    private void Awake()
    {
        rbs = GetComponentsInChildren<Rigidbody>();
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
        hp -= value;
        if(hp <= 0.0f)
        {
            foreach(var rb in rbs)
            {
                rb.transform.SetParent(null);
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.constraints = RigidbodyConstraints.None;
                rb.AddExplosionForce(explosionImpulse, explosionPoint.position, explosionRadius, explosionUpwardModifier, ForceMode.Impulse);
                Destroy(rb.gameObject, destroyTime);
            }

            broken = true;
        }
    }
}
