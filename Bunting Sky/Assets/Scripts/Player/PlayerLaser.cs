using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLaser : MonoBehaviour
{
    [System.NonSerialized] public Control control;
    public Rigidbody rb;
    [System.NonSerialized] public float timeRemainingInLife;
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
            //Make point light visible after awhile
            if (Vector3.Distance(transform.position, playerBody.position) > (MIN_GLOW_DISTANCE + (playerBody.GetComponent<Rigidbody>().velocity.magnitude / 25f)))
            {
                transform.Find("Emissive Model").gameObject.SetActive(true);
                transform.Find("Point Light").gameObject.SetActive(true);
            }
            else
            {
                transform.Find("Emissive Model").gameObject.SetActive(false);
                transform.Find("Point Light").gameObject.SetActive(false);
            }

            //Deactivate self after lifetime expires
            if (timeRemainingInLife <= 0f)
            {
                transform.Find("Emissive Model").gameObject.SetActive(false);
                transform.Find("Point Light").gameObject.SetActive(false);
                gameObject.SetActive(false);
            }

            timeRemainingInLife -= Time.deltaTime;

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