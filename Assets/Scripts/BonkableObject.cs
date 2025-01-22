using NUnit.Framework.Internal.Commands;
using UnityEngine;

public class BonkableObject : MonoBehaviour, IBonkable
{
    public void OnBonked(float value)
    {
       
        
           //Apply a color changed to bonked objects
            GetComponent<Renderer>().material.color = Color.red;

          //Apply a force if the object has a Rigidbody
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 bonkForce = (transform.position - Camera.main.transform.position).normalized * 5f;
                rb.AddForce(bonkForce, ForceMode.Impulse);
            }

           //Destroy the object after a delay
            Destroy(gameObject, 2f);
       
    }
}


    
