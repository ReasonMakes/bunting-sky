﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponProjectileLaser : MonoBehaviour
{
    [System.NonSerialized] public Control control;
    public Rigidbody rb;

    [System.NonSerialized] public float timeSpentAlive;
    [System.NonSerialized] public float timeAtWhichThisSelfDestructs;
    private readonly float DISTANCE_THRESHOLD_MIN_TO_GLOW = 4f; //2f; //1.0f;
    private readonly float COLLISION_ASTEROID_FORCE = 2e3f; //500f;

    [System.NonSerialized] public bool canDamage = true;

    private void Start()
    {
        //Ignore collisions with player
        Physics.IgnoreCollision(
            transform.Find("Non-Emissive Model").GetComponent<MeshCollider>(),
            control.GetPlayerTransform().Find("Player Collider").GetComponent<MeshCollider>()
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
            TryInteract(hit.transform, hit.point);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        TryInteract(collision.transform, transform.position);
        DeactivateSelf();
    }

    private void TryInteract(Transform transformWeHit, Vector3 hitPoint)
    {
        //transform name
        //Vector3 collision point

        if (transformWeHit.name == control.generation.asteroid.name + "(Clone)")
        {
            Asteroid asteroidScript = transformWeHit.GetComponent<Asteroid>();

            //Break apart asteroid
            if (!asteroidScript.isDestroying)
            {
                if (canDamage)
                {
                    //Calculate the direction from the laser to the asteroid hit point
                    Vector3 direction = (transform.position - hitPoint).normalized;

                    //Add force to the asteroid (negative direction because we should push away, not toward)
                    asteroidScript.rb.AddForce(-direction * COLLISION_ASTEROID_FORCE);
                    asteroidScript.rb.AddTorque(-direction * COLLISION_ASTEROID_FORCE);

                    //Damage the asteroid
                    asteroidScript.Damage(1, direction, hitPoint, true);
                }
            }
        }
        else if (transformWeHit.name == control.generation.enemy.name + "(Clone)")
        {
            if (canDamage)
            {
                //Calculate the direction from the laser to the asteroid hit point
                Vector3 direction = (transform.position - hitPoint).normalized;

                transformWeHit.GetComponent<Enemy>().Damage(1, direction, hitPoint, true, true);
            }
        }

        //Can no longer deal damage
        canDamage = false;
    }

    private void UpdateEmissionAndLuminosity()
    {
        bool glow = Vector3.Distance(transform.position, control.GetPlayerTransform().position) > DISTANCE_THRESHOLD_MIN_TO_GLOW; //todo add SPECIFICALLY forward velocity of player (use dot product?)

        transform.Find("Emissive Model").gameObject.SetActive(glow);
        transform.Find("Point Light").gameObject.SetActive(glow);
    }
}