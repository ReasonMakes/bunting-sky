using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponProjectileSeismicCharge : MonoBehaviour
{
    [System.NonSerialized] public Control control;
    private Transform playerBody;
    public Rigidbody rb;
    public GameObject explosionPrefab;

    [System.NonSerialized] public Vector3 startVelocity;
    private readonly float DRAG = 3f;

    private readonly float TIME_FROM_START_SUCK = 2.31f; //3f;
    private readonly float TIME_FROM_START_EXPLODE = 3.028f; //3f;
    [System.NonSerialized] public float timePoolSpawned;
    private readonly float MIN_GLOW_DISTANCE = 1f;

    public GameObject explosion;
    private readonly float EXPLOSION_RADIUS = 30f;
    private readonly float EXPLOSION_PUSH_STRENGTH = 5000f; //1f;
    private readonly float EXPLOSION_SUCK_STRENGTH = 5000f; //1f;
    private readonly float EXPLOSION_DURATION = 0.6f; //0.3f; //animation duration in seconds
    [System.NonSerialized] public bool sucked = false;
    [System.NonSerialized] public bool exploded = false;

    private void Start()
    {
        //Ignore collisions with player
        Physics.IgnoreCollision(
            transform.Find("Visible").Find("Model").Find("Non-Emissive Model").GetComponent<MeshCollider>(),
            control.GetPlayerTransform().Find("Player Collider").GetComponent<MeshCollider>()
        );
    }

    public void ResetPoolState(Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        transform.position = position;
        transform.rotation = rotation;
        rb.rotation = rotation;
        rb.angularVelocity = Vector3.zero;
        rb.velocity = velocity;
        rb.AddTorque(100 * new Vector3(Random.value, Random.value, Random.value));
        timePoolSpawned = Time.time;
        startVelocity = control.generation.playerPrefab.GetComponentInChildren<Rigidbody>().velocity;
        sucked = false;
    }

    private void Update()
    {
        if (!Menu.menuOpenAndGamePaused)
        {
            //Make point light visible once far enough from player
            EmissionAndLuminosityOffToOn();
            
            //Drag relative to player velocity at instantiation time
            //rb.velocity = Control.GetVelocityDraggedRelative(rb.velocity, startVelocity, DRAG);

            //Deactivate self after lifetime expires
            if (Time.time >= timePoolSpawned + TIME_FROM_START_SUCK)
            {
                Suck();
            }
            if (Time.time >= timePoolSpawned + TIME_FROM_START_EXPLODE)
            {
                Explode();
            }

            //Explosion animation
            if (sucked)
            {
                float explosionModelRadius = 1.72142f; //from Blender
                //explosion.transform.localScale += (Vector3.one * EXPLOSION_RADIUS * 2f * (1f / transform.localScale.magnitude) * Time.deltaTime) / EXPLOSION_DURATION;
                //explosion.transform.localScale = Vector3.one * (EXPLOSION_RADIUS / explosionModelRadius);
                explosion.transform.localScale += Vector3.one * ((EXPLOSION_RADIUS * Time.deltaTime) / (explosionModelRadius * EXPLOSION_DURATION));
            }
        }
    }

    /*
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.name != "Player (Clone)")
        {
            DeactivateSelf();
        }
    }
    */

    private void Suck()
    {
        if (!sucked)
        {
            //Turn off regular model excluding explosion shader
            transform.Find("Visible").Find("Model").gameObject.SetActive(false);

            //Suck
            RaycastWeaponInteraction(false);

            //Only run once
            sucked = true;
        }
    }

    private void Explode()
    {
        if (!exploded)
        {
            //Explode
            DeactivateSelf();
            //Invoke("DeactivateSelf", EXPLOSION_DURATION);

            //Only run once
            exploded = true;
        }
    }

    private void DeactivateSelf() //invoked
    {
        /*
        GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        explosion.transform.parent = transform.parent;
        explosion.GetComponent<PlayerWeaponSeismicChargeProjectileExplosion>().control = control;
        */

        RaycastWeaponInteraction(true);

        explosion.transform.localScale = Vector3.zero;
        explosion.SetActive(false);
        transform.Find("Visible").gameObject.SetActive(false);
    }

    private void RaycastWeaponInteraction(bool explosion)
    {
        //Check for colliders in the area
        Collider[] collidersInRadius = Physics.OverlapSphere(transform.position, EXPLOSION_RADIUS);
        foreach (Collider collider in collidersInRadius)
        {
            //Don't bother raycasting unless the collider in the area is "damageable" (asteroid, enemy, player, etc.)
            if (IsADamageableCollider(collider))
            {
                //Cast a ray to make sure the collider is in LOS
                LayerMask someLayerMask = -1;
                Vector3 rayDirection = (collider.transform.position - transform.position).normalized;
                float rayDistanceMax = (collider.transform.position - transform.position).magnitude;

                if (Physics.Raycast(transform.position, rayDirection, out RaycastHit hit, rayDistanceMax, someLayerMask, QueryTriggerInteraction.Ignore))
                {
                    //Make sure the ray is hitting an asteroid (something else could be in the way blocking LOS) that is within range
                    float distanceBetweenHitAndEpicentre = (transform.position - hit.point).magnitude;

                    if (IsADamageableRaycastHit(hit) && distanceBetweenHitAndEpicentre < EXPLOSION_RADIUS)
                    {
                        //Direction and force magnitude
                        Vector3 directionFromEpicentreToHit = (hit.point - transform.position).normalized;
                        float forceMag = EXPLOSION_PUSH_STRENGTH;
                        if (!explosion)
                        {
                            //Suck
                            directionFromEpicentreToHit = -directionFromEpicentreToHit;
                            forceMag = EXPLOSION_SUCK_STRENGTH;
                        }
                        
                        //Asteroid
                        if (hit.transform.name == control.generation.asteroid.name + "(Clone)")
                        {
                            Asteroid asteroidScript = hit.transform.GetComponent<Asteroid>();

                            //Don't bother with already destroyed asteroids
                            if (!asteroidScript.isDestroying)
                            {
                                //THIS RUNS FOUR TIMES BECAUSE IT IS HITTING THE TRIGGER COLLIDERS

                                //Force
                                Vector3 finalForceVector = directionFromEpicentreToHit * EXPLOSION_PUSH_STRENGTH * (1f - (distanceBetweenHitAndEpicentre / EXPLOSION_RADIUS));
                                asteroidScript.rb.AddForce(finalForceVector);

                                //Damage
                                if (explosion)
                                {
                                    Vector3 directionHitFrom = (transform.position - hit.point).normalized;
                                    asteroidScript.Damage((byte)(1 + GetDamageAmount(hit.transform.position)), directionHitFrom, hit.point, true);
                                }
                            }
                        }
                        else if (hit.transform.name == control.generation.enemy.name + "(Clone)")
                        {
                            Enemy enemyScript = hit.transform.GetComponent<Enemy>();

                            //Force
                            Vector3 finalForceVector = directionFromEpicentreToHit * EXPLOSION_PUSH_STRENGTH * (1f - (distanceBetweenHitAndEpicentre / EXPLOSION_RADIUS));
                            enemyScript.rb.AddForce(finalForceVector);

                            //Damage
                            if (explosion)
                            {
                                enemyScript.Damage((byte)GetDamageAmount(hit.transform.position), -directionFromEpicentreToHit, hit.point, true, true);
                            }
                        }
                        else if (hit.transform.name == "Body")
                        {
                            //Force
                            Vector3 finalForceVector = directionFromEpicentreToHit * EXPLOSION_PUSH_STRENGTH * (1f - (distanceBetweenHitAndEpicentre / EXPLOSION_RADIUS));
                            control.GetPlayerScript().rb.AddForce(finalForceVector);

                            //Damage
                            if (explosion)
                            {
                                control.GetPlayerScript().DamagePlayer(
                                    control.GetPlayerScript().vitalsHealth - GetDamageAmount(control.GetPlayerTransform().position),
                                    "seismic charge explosion",
                                    1.0f,
                                    (transform.position - control.GetPlayerTransform().position).normalized,
                                    true
                                );
                            }
                        }
                    }
                }
            }
        }
    }

    private double GetDamageAmount(Vector3 positionOfVictim)
    {
        double distanceToVictim = Vector3.Distance(transform.position, positionOfVictim);

        double baseDamage = 1.0d;
        double splashFactor = (EXPLOSION_RADIUS - distanceToVictim) / EXPLOSION_RADIUS;
        double splashDamage = 5.0d * splashFactor;

        return baseDamage + splashDamage;
    }

    private bool StringIsAnAsteroidModel(string name)
    {
        bool itIs =
               name == "AsteroidSmall1"
            || name == "AsteroidSmall2"
            || name == "AsteroidSmall3"
            || name == "Asteroid1"
            || name == "Asteroid2"
            || name == "Asteroid3"
            || name == "AsteroidLarge1";

        return itIs;
    }

    private bool IsADamageableRaycastHit(RaycastHit hit)
    {
        return (
               hit.transform.name == control.generation.asteroid.name + "(Clone)"
            || hit.transform.name == control.generation.enemy.name + "(Clone)"
            || hit.transform.name == "Player Collider"
            || hit.transform.name == "Body"
        );
    }

    private bool IsADamageableCollider(Collider collider)
    {
        return (
            StringIsAnAsteroidModel(collider.gameObject.name)
            || collider.gameObject.name == control.generation.enemy.name + "(Clone)"
            || collider.gameObject.name == "Player Collider"
            || collider.gameObject.name == "Body"
        );
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

            if (hit.transform.name == "CBodyAsteroid(Clone)")
            {
                Asteroid asteroidScript = hit.transform.GetComponent<Asteroid>();

                //Break apart asteroid
                if (!asteroidScript.isDestroying)
                {
                    Vector3 direction = (transform.position - hit.point).normalized;
                    asteroidScript.Damage(1, direction, hit.point, true);
                }
            }

            //Deactivate self
            Suck();
        }
    }

    private void EmissionAndLuminosityOffToOn()
    {
        bool notSucking = (Time.time < timePoolSpawned + TIME_FROM_START_SUCK);
        bool farFromPlayer = Vector3.Distance(transform.position, control.GetPlayerTransform().position) > MIN_GLOW_DISTANCE + (control.GetPlayerScript().GetComponent<Rigidbody>().velocity.magnitude / 25f);

        //Debug.LogFormat("{0}, {1}", NotSelfDestructingYet, isFarEnoughAwayFromPlayer);

        if (!transform.Find("Visible").gameObject.activeSelf)
        {
            transform.Find("Visible").gameObject.SetActive(notSucking && farFromPlayer);
        }
    }
}