using DinoFracture;
using UnityEngine;

public class BreakableObject : MonoBehaviour, IBonkable
{
    public Rigidbody[] rbs;
    [SerializeField] float hp = 1.0f;
    [SerializeField] bool broken = false;
    [SerializeField] Transform debugExplosionPoint;
    [SerializeField] float explosionImpulse = 10f;
    [SerializeField] float explosionUpwardModifier = 1f;
    [SerializeField] float explosionRadius = 3f;
    [SerializeField] bool testBreak = false;
    [SerializeField] float destroyTime = 10f;
    [SerializeField] ParticleSystem dustParticle = null;
    [SerializeField] GameObject breakEffect = null, dustEffect;

    private void Awake()
    {
        // Grab all of the Rigidbodies in the child object
        // Then detatch the first child which should be the Fractured Prefab
        rbs = GetComponentsInChildren<Rigidbody>(true);
        transform.GetChild(0).SetParent(null);
    }

    void Start()
    {
        dustParticle = Instantiate(dustEffect.gameObject, transform.position, transform.rotation).GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if(testBreak)
        {
            OnBonked(hp, debugExplosionPoint.position);
            testBreak = false;
        }
    }

    public void OnBonked(float value, Vector3 position)
    {
        if (broken) return; // Already broken, don't respond to bonking
        Debug.Log("Bonked with value: " + value.ToString());
        hp -= value;

        if(hp <= 0.0f)
        {
            GetComponent<FractureGeometry>().Fracture();
            if (dustParticle != null) dustParticle.Play();
            if(breakEffect != null) Instantiate(breakEffect, position, Quaternion.identity);

            foreach(var rb in rbs)
            {
                rb.transform.SetParent(null);
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.constraints = RigidbodyConstraints.None;
                rb.AddExplosionForce(explosionImpulse, position, explosionRadius, explosionUpwardModifier, ForceMode.Impulse);
                //Destroy(rb.gameObject, destroyTime);
            }

            broken = true;
        }
    }
}
