using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponSeismicChargeProjectileExplosion : MonoBehaviour
{
    [System.NonSerialized] public Control control;

    private readonly float EXPLOSION_RADIUS = 3f;
    private readonly float EXPLOSION_STRENGTH = 1f;

    private void Start()
    {
        //Check for colliders in the area
        Collider[] collidersInRadius = Physics.OverlapSphere(transform.position, EXPLOSION_RADIUS);
        foreach (Collider collider in collidersInRadius)
        {
            //Don't bother raycasting unless the collider in the area is an asteroid
            if (collider.gameObject.name == control.generation.cBodyAsteroid.name + "(Clone)")
            {
                //Cast a ray to make sure the asteroid is in LOS
                LayerMask someLayerMask = -1;
                Vector3 rayDirection = (collider.transform.position - transform.position).normalized;
                float rayDistanceMax = (collider.transform.position - transform.position).magnitude;

                if (Physics.Raycast(transform.position, rayDirection, out RaycastHit hit, rayDistanceMax, someLayerMask, QueryTriggerInteraction.Ignore))
                {
                    //Make sure the ray is hitting an asteroid (something else could be in the way blocking LOS)
                    if (hit.transform.name == "CBodyAsteroid(Clone)")
                    {
                        CBodyAsteroid asteroidScript = hit.transform.GetComponent<CBodyAsteroid>();

                        //Don't bother with already destroyed asteroids
                        if (!asteroidScript.destroyed)
                        {
                            //Explosion push force
                            collider.GetComponent<Rigidbody>().AddExplosionForce(EXPLOSION_STRENGTH, transform.position, EXPLOSION_RADIUS);

                            //Explosion damage
                            Vector3 directionHitFrom = (transform.position - hit.point).normalized;
                            asteroidScript.Damage(1, directionHitFrom, hit.point);
                        }
                    }
                }
            }
        }

        //Destroy self quickly after being created
        Destroy(gameObject, 2f);
    }
}