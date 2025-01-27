using UnityEngine;

public class DetatchChild : MonoBehaviour
{
    void Start()
    {
        transform.GetChild(0).SetParent(null);
    }
}
