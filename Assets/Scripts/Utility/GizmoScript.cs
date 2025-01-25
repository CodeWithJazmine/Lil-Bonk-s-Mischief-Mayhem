using UnityEngine;

public class GizmoScript : MonoBehaviour
{
    public float size = 1f;
    public Color color = Color.white;
    public bool solid = true;

    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        if(solid)
            Gizmos.DrawSphere(transform.position, size);
        else
            Gizmos.DrawWireSphere(transform.position, size);
    }
}
