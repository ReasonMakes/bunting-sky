using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponProjectileLaser : MonoBehaviour
{
    [System.NonSerialized] public Control control;
    private Transform playerBody;
    public Rigidbody rb;

    public GameObject asteroid;

    [System.NonSerialized] public float timeSpentAlive;
    [System.NonSerialized] public float timeAtWhichThisSelfDestructs;
    private readonly float MIN_GLOW_DISTANCE = 1.0f;
    //private readonly float COLLISION_ASTEROID_FORCE = 2.0f;

    [System.NonSerialized] public bool canDamage = true;

    private void Start()
    {
        //Player reference
        playerBody = control.generation.instancePlayer.transform.Find("Body");

        //Ignore collisions with player
        Physics.IgnoreCollision(
            transform.Find("Non-Emissive Model").GetComponent<MeshCollider>(),
            playerBody.GetComponent<MeshCollider>()
        );

        //General collision detection
        rb.detectCollisions = true;
    }

    private void Update()
    {
        if (!Menu.menuOpenAndGamePaused)
        {
            //Make point light visible after awhile and invisible just before self-destruction
            UpdateEmissionAndLuminosity();
            
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
        transform.Find("Emissive Model").gameObject.SetActive(false);
        transform.Find("Point Light").gameObject.SetActive(false);
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

        //Debug.Log(raycastDistance);
        //Debug.DrawRay(transform.position, transform.right * raycastDistance, Color.red);

        //if (Physics.Raycast(transform.position, transform.right, out RaycastHit hit, raycastDistance))

        LayerMask someLayerMask = -1;
        if (Physics.Raycast(transform.position, transform.right, out RaycastHit hit, raycastDistance, someLayerMask, QueryTriggerInteraction.Ignore))
        {
            //Debug.Log("Laser hit object: " + hit.transform.name);

            if (hit.transform.name == asteroid.name + "(Clone)")
            {
                Asteroid asteroidScript = hit.transform.GetComponent<Asteroid>();

                //Break apart asteroid
                if (!asteroidScript.destroying)
                {
                    if (canDamage)
                    {
                        //Calculate the direction from the laser to the asteroid hit point
                        Vector3 direction = (transform.position - hit.point).normalized;

                        //Add force to the asteroid (negative direction because we should push away, not toward)
                        //asteroidScript.rb.AddForce(-direction * COLLISION_ASTEROID_FORCE);
                        //asteroidScript.rb.AddTorque(-direction * COLLISION_ASTEROID_FORCE);

                        //Damage the asteroid
                        asteroidScript.Damage(1, direction, hit.point, true);
                    }

                    //Reset tooltip certainty
                    control.ui.tipAimCertainty = 0f;
                }
            }

            //Can no longer deal damage
            canDamage = false;

            //Deactivate self
            //DeactivateSelf();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Collision!");

        DeactivateSelf();

        //if (collision.gameObject.name == control.generation.cBodyAsteroid.name + "(Clone)")
        //{
        //    DeactivateSelf();
        //}
    }

    private void UpdateEmissionAndLuminosity()
    {
        bool glow = Vector3.Distance(transform.position, playerBody.position) > MIN_GLOW_DISTANCE + playerBody.GetComponent<Rigidbody>().velocity.magnitude;

        transform.Find("Emissive Model").gameObject.SetActive(glow);
        transform.Find("Point Light").gameObject.SetActive(glow);
    }
}