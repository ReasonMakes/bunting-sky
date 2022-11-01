using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeaponProjectileLaser : MonoBehaviour
{
    [System.NonSerialized] public Control control;
    private Rigidbody rb;

    [System.NonSerialized] public float timeSpentAlive;
    [System.NonSerialized] public float timeAtWhichThisSelfDestructs;

    [System.NonSerialized] public bool canDamage = true;
    [System.NonSerialized] public MeshCollider parentMeshCollider;

    private void Start()
    {
        //Ignore collisions with the enemy who fired it
        Physics.IgnoreCollision(
            transform.Find("Non-Emissive Model").GetComponent<MeshCollider>(),
            parentMeshCollider
        );

        //General collision detection
        rb = GetComponent<Rigidbody>();
        rb.detectCollisions = true;
    }

    private void Update()
    {
        if (!Menu.menuOpenAndGamePaused)
        {
            //Raycast collisions
            UpdateCollisionDetection();

            //Increment lifetime
            timeSpentAlive += Time.deltaTime;

            //Deactivate self after lifetime expires
            if (timeSpentAlive >= timeAtWhichThisSelfDestructs)
            {
                DeactivateSelf();
            }
        }
    }

    private void DeactivateSelf()
    {
        //transform.Find("Emissive Model").gameObject.SetActive(false);
        //transform.Find("Point Light").gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    private void UpdateCollisionDetection()
    {
        /*
         * Unity's collision detection system is great for some things,
         * But for weapon projectiles it often doesn't do a good enough job at detecting them
         * So we use this custom method instead
         * 
         * Here we use raycasting to check the space in front of the projectile for collidables
         * The distance we check ahead increases with the projectile's speed to prevent phasing
         * 
         * We also have to be careful to ignore the trigger colliders since those are used for the waypoint and target system
         * 
         * In the raycast, we cast out from the transform.right direction since the model is rotated
         */

        float minimumRaycastDistance = 20f; //this value must be high enough that the projectile does not phase through objects directly in front of the player
        float raycastDistance = minimumRaycastDistance * rb.velocity.magnitude * Time.deltaTime;

        //if (Physics.Raycast(transform.position, transform.right, out RaycastHit hit, raycastDistance))

        LayerMask someLayerMask = -1;
        if (Physics.Raycast(transform.position, transform.right, out RaycastHit hit, raycastDistance, someLayerMask, QueryTriggerInteraction.Ignore))
        {
            //Debug.Log("Laser hit object: " + hit.transform.name);

            if (hit.transform.name == control.generation.asteroid.name + "(Clone)")
            {
                Asteroid asteroidScript = hit.transform.GetComponent<Asteroid>();

                //Break apart asteroid
                if (!asteroidScript.isDestroying)
                {
                    if (canDamage)
                    {
                        //Calculate the direction from the laser to the asteroid hit point
                        Vector3 direction = (transform.position - hit.point).normalized;

                        //Damage the asteroid
                        asteroidScript.Damage(1, direction, hit.point, true);
                    }
                }
            }
            else if (hit.transform.name == control.generation.playerPrefab.name + "(Clone)")
            {
                if (canDamage)
                {
                    //Calculate the direction from the laser to the asteroid hit point
                    //Vector3 direction = (transform.position - hit.point).normalized;

                    //Damage the player
                    //control.GetPlayerScript().DamagePlayer(control.GetPlayerScript().vitalsHealth - 1.0d, "enemy weapons fire", 1.0f);
                }
            }

            //Can no longer deal damage
            canDamage = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        DeactivateSelf();
    }
}