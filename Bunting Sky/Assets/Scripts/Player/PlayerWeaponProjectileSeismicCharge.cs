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

    private readonly float PROJECTILE_LIFETIME_DURATION = 3f;
    [System.NonSerialized] public float timeSpentAlive;
    [System.NonSerialized] public float timeAtWhichThisSelfDestructs;
    private readonly float MIN_GLOW_DISTANCE = 1f;

    public GameObject explosion;
    private readonly float EXPLOSION_RADIUS = 30f;
    private readonly float EXPLOSION_PUSH_STRENGTH = 1f;
    private readonly float EXPLOSION_DURATION = 0.6f; //0.3f; //animation duration in seconds
    [System.NonSerialized] public bool exploded = false;

    private void Start()
    {
        //Ignore collisions with player
        Physics.IgnoreCollision(
            transform.Find("Non-Emissive Model").GetComponent<MeshCollider>(),
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
        timeAtWhichThisSelfDestructs = PROJECTILE_LIFETIME_DURATION;
        timeSpentAlive = 0f;
        startVelocity = control.generation.playerPrefab.GetComponentInChildren<Rigidbody>().velocity;
        exploded = false;
    }

    private void Update()
    {
        if (!Menu.menuOpenAndGamePaused)
        {
            //Make point light visible after awhile and invisible just before self-destruction
            UpdateEmissionAndLuminosity();
            
            //Raycast collisions
            //UpdateCollisionDetection();

            //Increment lifetime
            timeSpentAlive += Time.deltaTime;

            //Drag relative to player velocity at instantiation time
            rb.velocity = Control.GetVelocityDraggedRelative(rb.velocity, startVelocity, DRAG);

            //Deactivate self after lifetime expires
            if (timeSpentAlive >= timeAtWhichThisSelfDestructs)
            {
                Explode();
            }

            //Explosion animation
            if (exploded)
            {
                explosion.transform.localScale += (Vector3.one * EXPLOSION_RADIUS * 2f * (1f / transform.localScale.magnitude) * Time.deltaTime) / EXPLOSION_DURATION;
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

    private void Explode()
    {
        if (!exploded)
        {
            Invoke("DeactivateSelf", EXPLOSION_DURATION);

            exploded = true;
        }
    }

    private void DeactivateSelf()
    {
        /*
        GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        explosion.transform.parent = transform.parent;
        explosion.GetComponent<PlayerWeaponSeismicChargeProjectileExplosion>().control = control;
        */

        DealExplosionDamageAndForce();

        SetEmissionAndLuminosity(false);

        explosion.transform.localScale = Vector3.zero;
        explosion.SetActive(false);
        gameObject.SetActive(false);
    }

    private void DealExplosionDamageAndForce()
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
                        //Asteroid
                        if (hit.transform.name == control.generation.asteroid.name + "(Clone)")
                        {
                            Asteroid asteroidScript = hit.transform.GetComponent<Asteroid>();

                            //Don't bother with already destroyed asteroids
                            if (!asteroidScript.isDestroying)
                            {
                                //THIS RUNS FOUR TIMES BECAUSE IT IS HITTING THE TRIGGER COLLIDERS

                                //Explosion push force
                                //Vector3 directionFromEpicentreToHit = (hit.point - transform.position).normalized;
                                //Vector3 finalForceVector = directionFromEpicentreToHit * EXPLOSION_PUSH_STRENGTH * (1f - (distanceBetweenHitAndEpicentre / EXPLOSION_RADIUS));

                                //Explosion damage
                                Vector3 directionHitFrom = (transform.position - hit.point).normalized;
                                asteroidScript.Damage(2, directionHitFrom, hit.point, true);
                            }
                        }
                        else if (hit.transform.name == control.generation.enemy.name + "(Clone)")
                        {
                            //Calculate the direction from the projectile to the collider hit point
                            Vector3 direction = (transform.position - hit.point).normalized;

                            //Damage
                            Enemy enemyScript = hit.transform.GetComponent<Enemy>();
                            enemyScript.Damage(1, direction, hit.point, true, true);
                        }
                        else if (hit.transform.name == "Body")
                        {
                            //Calculate the direction from the projectile to the collider hit point
                            //Vector3 direction = (transform.position - hit.point).normalized;

                            //Damage
                            control.GetPlayerScript().DamagePlayer(
                                control.GetPlayerScript().vitalsHealth - 1.0d,
                                "seismic charge explosion",
                                1.0f,
                                (transform.position - control.GetPlayerTransform().position).normalized
                            );
                        }
                    }
                }
            }
        }
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
            Explode();
        }
    }

    private void UpdateEmissionAndLuminosity()
    {
        bool NotSelfDestructingYet = timeSpentAlive < timeAtWhichThisSelfDestructs - Time.deltaTime;
        bool isFarEnoughAwayFromPlayer = Vector3.Distance(transform.position, control.GetPlayerTransform().position) > MIN_GLOW_DISTANCE + (control.GetPlayerScript().GetComponent<Rigidbody>().velocity.magnitude / 25f);

        //Debug.LogFormat("{0}, {1}", NotSelfDestructingYet, isFarEnoughAwayFromPlayer);

        SetEmissionAndLuminosity(NotSelfDestructingYet && isFarEnoughAwayFromPlayer);
    }

    private void SetEmissionAndLuminosity(bool isOn)
    {
        transform.Find("Emissive Model").gameObject.SetActive(isOn);
        transform.Find("Point Light 1").gameObject.SetActive(isOn);
        transform.Find("Point Light 2").gameObject.SetActive(isOn);
    }
}