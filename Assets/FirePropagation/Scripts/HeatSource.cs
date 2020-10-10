using UnityEngine;

public class HeatSource : MonoBehaviour
{
    public float heatRange;

    private void Update()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, heatRange);
        foreach (Collider collider in colliders)
        {
            FireComponent fireComponent = collider.gameObject.GetComponent<FireComponent>();
            if (fireComponent != null && !fireComponent.HasStartedFire)
            {
                fireComponent.StartFire(transform.position);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (isActiveAndEnabled)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, heatRange);
        }
    }
}
