using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLaser : MonoBehaviour
{
    [System.NonSerialized] public Control control;
    public Rigidbody rb;
    [System.NonSerialized] public float timeSpentAlive;
    [System.NonSerialized] public float timeAtWhichThisSelfDestructs;
    private readonly float MIN_GLOW_DISTANCE = 2f;
    private Transform playerBody;

    private void Start()
    {
        rb.detectCollisions = false;
        playerBody = control.instancePlayer.transform.Find("Body");
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
         * Unit's collision detection system is great for some things,
         * But for weapon projectiles it often doesn't do a good enough job at detecting them
         * So we use this custom method instead
         * 
         * Here we use raycasting to check the space in front of the projectile for collideables
         * The distance we check ahead increases with the projectile's speed to prevent phasing
         * 
         * We also have to be careful to ignore the trigger colliders since those are used for the waypoint and target system
         * 
         * In the raycast, we cast out from the transform.right direction since the model is rotated
         */ 

        float raycastDistance = 2f * rb.velocity.magnitude * Time.deltaTime;

        //Debug.DrawRay(transform.position, transform.right * raycastDistance, Color.red);
        //if (Physics.Raycast(transform.position, transform.right, out RaycastHit hit, raycastDistance))

        LayerMask someLayerMask = -1;
        if (Physics.Raycast(transform.position, transform.right, out RaycastHit hit, raycastDistance, someLayerMask, QueryTriggerInteraction.Ignore))
        {
            //Debug.Log("Laser hit object: " + hit.transform.name);

            if (hit.transform.name == "CBodyAsteroid(Clone)")
            {
                CBodyAsteroid asteroidScript = hit.transform.GetComponent<CBodyAsteroid>();

                //Break apart asteroid
                if (!asteroidScript.destroyed)
                {
                    Vector3 direction = (transform.position - hit.point).normalized;
                    asteroidScript.Damage(1, direction, hit.point);
                }
            }

            //Deactivate self
            DeactivateSelf();
        }
    }

    private void UpdateEmissionAndLuminosity()
    {
        bool glow = timeSpentAlive < timeAtWhichThisSelfDestructs - 1f
            && Vector3.Distance(transform.position, playerBody.position) > MIN_GLOW_DISTANCE + (playerBody.GetComponent<Rigidbody>().velocity.magnitude / 25f);

        transform.Find("Emissive Model").gameObject.SetActive(glow);
        transform.Find("Point Light").gameObject.SetActive(glow);
    }
}