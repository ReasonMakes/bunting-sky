using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLaser : MonoBehaviour
{
    public Rigidbody rb;
    public float lifetime;

    private void Start()
    {
        rb.detectCollisions = false;
    }

    private void Update()
    {
        if (!Menu.menuOpenAndGamePaused)
        { 
            //Deactivate self after lifetime expires
            if (lifetime <= 0f)
            {
                gameObject.SetActive(false);
            }

            lifetime -= Time.deltaTime;

            //Raycast collisions
            //Check farther ahead when moving faster so it's impossible to move through a collider without hitting
            float raycastDistance = 2f * rb.velocity.magnitude * Time.deltaTime;

            //Debug.DrawRay(transform.position, transform.right * raycastDistance, Color.red);
            //if (Physics.Raycast(transform.position, transform.right, out RaycastHit hit, raycastDistance))

            //We use transform.right instead of transform.forward since the model is rotated
            //We use QueryTriggerInteraction.Ignore to ignore trigger colliders since those are used for the waypoint system
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
                gameObject.SetActive(false);
            }
        }
    }
}