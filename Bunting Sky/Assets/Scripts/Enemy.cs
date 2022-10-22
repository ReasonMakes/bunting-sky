using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [System.NonSerialized] public Control control;
    public SphereCollider targetCollider1;
    public SphereCollider targetCollider2;
    public Rigidbody rb;

    //Thrust
    private Vector3 thrustVector;
    private readonly float THRUST = 4000f;
    //Torque
    private float torqueBaseStrength = 600f;
    private float angularDragWhenEnginesOn = 40f; //for smoothing
    //Drag
    private readonly float DRAG = 3f;

    //Weapons
    private float weaponCooldown = 0f;
    private readonly float WEAPON_COOLDOWN_MAX = 2f; //Time in seconds between shots
    private readonly float WEAPON_COOLDOWN_WITHIN_BURST = 0.1f; //Time in seconds between shots
    private int weaponBurst = 3;
    private readonly int WEAPON_BURST_MAX = 4;

    //Behaviour settings
    [System.NonSerialized] public Vector3 spawnPointRaw = Vector3.zero;
    private Vector3 destination = Vector3.zero;
    private bool aggro = false;
    private readonly float DISTANCE_THRESHOLD_LESS_THAN_TO_AGGRO = 70f;
    private readonly float DISTANCE_THRESHOLD_GREATER_THAN_TO_MOVE_FORWARD = 9f;
    private readonly float DISTANCE_THRESHOLD_LESS_THAN_TO_STRAFE = 20f;
    private readonly float DISTANCE_THRESHOLD_LESS_THAN_TO_FIRE = 40f;
    [System.NonSerialized] public static readonly float DISTANCE_THRESHOLD_GREATER_THAN_PERFORMANT_MODE = 180f;
    private bool strafeRight = true;
    private float strafeDirectionChangeTimer = 2f;
    private readonly float STRAFE_PERIOD_MAX = 2f;

    [System.NonSerialized] public bool destroying = false;
    [System.NonSerialized] public bool destroyed = true;
    private float destroyingTime = 0f;
    [System.NonSerialized] public bool performantMode = false;

    [System.NonSerialized] public readonly static byte HEALTH_MAX = 4;
    [System.NonSerialized] public byte health = HEALTH_MAX;

    [System.NonSerialized] public int strength;
    [System.NonSerialized] public readonly static int STRENGTH_SMALL = 0;
    [System.NonSerialized] public readonly static int STRENGTH_MEDIUM = 1;
    [System.NonSerialized] public readonly static int STRENGTH_LARGE = 2;
    private GameObject modelGroup;
    [System.NonSerialized] public GameObject modelObject;
    public GameObject modelGroupStrengthWeak;
    public GameObject modelGroupStrengthMedium;
    public GameObject modelGroupStrengthLarge;
    public Material matEnemy;
    
    [System.NonSerialized] public MeshRenderer meshRenderer;

    public GameObject ore;

    [System.NonSerialized] public Vector3 rbMemVel;
    [System.NonSerialized] public Vector3 rbMemAngularVel;

    private void Start()
    {
        SetHitboxEnabledAndChoose(true);
        rb.detectCollisions = true;
        GetComponent<EnemyWeaponLaser>().control = control;
    }

    private void Update()
    {
        if (!performantMode)
        {
            if (!Menu.menuOpenAndGamePaused && !destroyed)
            {
                if (destroying)
                {
                    //DESTRUCTION
                    //Increment timer
                    destroyingTime += Time.deltaTime;

                    //Disable model and trigger volumes
                    DisableModelAndTriggerVolumes();

                    //Wait for particles to fade out before disabling trigger volume
                    bool particlesFadedOut = destroyingTime >= 20f; //15f; //particles technically don't fade out for 75 seconds, but they aren't actually visible after 9 seconds so this should be fine
                    bool playerBeyondArbitraryDistance = Vector3.Distance(transform.position, control.GetPlayerTransform().position) >= control.GetPlayerScript().ORBITAL_DRAG_MODE_THRESHOLD;
                    if (particlesFadedOut && playerBeyondArbitraryDistance)
                    {
                        //Disable
                        Disable();
                    }
                }
                else
                {
                    //BEHAVIOUR (moving and shooting)
                    if (control.GetPlayerScript().isDestroyed)
                    {
                        aggro = false;
                    }
                    else
                    {
                        aggro = (Vector3.Magnitude(control.GetPlayerTransform().position - transform.position) <= DISTANCE_THRESHOLD_LESS_THAN_TO_AGGRO);
                    }

                    if (aggro)
                    {
                        //destination = control.GetPlayerTransform().position;

                        //Lead, so that weapons fire is more likely to connect
                        destination = control.GetPlayerTransform().position; //raw target
                        float timeToTarget = Vector3.Magnitude(control.GetPlayerTransform().position - transform.position) / EnemyWeaponLaser.PROJECTILE_SPEED; //t = d/v; time in seconds it will take the weapon projectile to be at the target destination
                        destination += (control.GetPlayerScript().rb.velocity * timeToTarget); //target with added lead
                    }
                    else
                    {
                        destination = GetSpawnPoint();
                    }
                    UpdateEnemyMovementTorque();
                    UpdateEnemyMovementThrust();
                    UpdateEnemyMovementDrag();
                    UpdateEnemyWeaponsUse(); 
                }
            }
        }
    }

    private Vector3 GetSpawnPoint()
    {
        return spawnPointRaw + control.generation.verseSpace.transform.position;
    }

    private void UpdateEnemyMovementTorque()
    {
        //Thank you Tobias, Conkex, HiddenMonk, and Derakon
        //https://answers.unity.com/questions/727254/use-rigidbodyaddtorque-with-quaternions-or-forward.html

        //DRAG
        //Angular drag to smooth out torque
        rb.angularDrag = angularDragWhenEnginesOn;

        //DIRECTION
        //Vector to look toward
        Vector3 directionToDestinationToLookAt = Vector3.Normalize(destination - transform.position);

        //The rotation to look at that point
        Quaternion rotationToLookAtDestination = Quaternion.identity;
        if (directionToDestinationToLookAt != Vector3.zero)
        {
            rotationToLookAtDestination = Quaternion.LookRotation(directionToDestinationToLookAt);
        }

        //The rotation from how the ship is currently rotated to looking at the player
        //Multiplying by inverse is equivalent to subtracting
        Quaternion rotation = rotationToLookAtDestination * Quaternion.Inverse(rb.rotation);

        //Parse Quaternion to Vector3
        Vector3 torqueVector = new Vector3(rotation.x, rotation.y, rotation.z) * rotation.w;

        //STRENGTH
        //Adding all modifiers together
        float torqueStrength = torqueBaseStrength * rb.angularDrag * Time.deltaTime;

        //APPLY TORQUE
        Vector3 torqueFinal = torqueVector * torqueStrength;
        if (torqueFinal.magnitude != 0f) //so we don't get NaN error
        {
            rb.AddTorque(torqueFinal);
        }
    }

    private void UpdateEnemyMovementThrust()
    {
        //SETUP
        //Destination and direction to it
        Vector3 directionToPointToMoveTo = Vector3.Normalize(destination - transform.position);

        //Looking at destination?
        float dot = (Vector3.Dot(directionToPointToMoveTo, transform.forward) + 1f) / 2f; //0 to 1 depending on how accurately facing the player
        if (dot <= 0.5f) //clamp to either full thrust or no thrust
        {
            dot = 0f;
        }
        else
        {
            dot = 1f;
        }

        //THRUST DIRECTIONS
        //Forward direction
        Vector3 forwardVector = Vector3.zero;
        //Don't thrust forward if very close to destination
        if (Vector3.Magnitude(destination - transform.position) > DISTANCE_THRESHOLD_GREATER_THAN_TO_MOVE_FORWARD)
        {
            forwardVector = transform.forward * dot;
        }

        //Strafe direction
        Vector3 strafeVector = Vector3.zero;
        //Change left/right periodically
        strafeDirectionChangeTimer = Mathf.Max(0f, strafeDirectionChangeTimer - Time.deltaTime); //decrement 1f per second
        if (strafeDirectionChangeTimer <= 0f)
        {
            //Reset timer
            strafeDirectionChangeTimer = Random.value * STRAFE_PERIOD_MAX;

            //Change strafe direction
            strafeRight = !strafeRight;
        }
        //Strafe if aggro'd and within distance threshold
        if (aggro && Vector3.Magnitude(destination - transform.position) <= DISTANCE_THRESHOLD_LESS_THAN_TO_STRAFE)
        {
            if (strafeRight)
            {
                //Right
                strafeVector = transform.right * dot;
            }
            else
            {
                //Left
                strafeVector = -transform.right * dot;
            }
            
        }
        
        //COMBINING AND APPLYING
        //Add forward and strafe vectors together with weights
        thrustVector = (forwardVector * 1.5f) + strafeVector;

        //Thrust
        rb.AddForce(thrustVector.normalized * THRUST * Time.deltaTime);
    }

    private void UpdateEnemyMovementDrag()
    {
        rb.velocity *= (1f - (DRAG * Time.deltaTime));
    }

    private void UpdateEnemyWeaponsUse()
    {
        //Cooldown over time
        weaponCooldown = Mathf.Max(0f, weaponCooldown - Time.deltaTime);

        //Can fire?
        if (weaponCooldown <= 0f && Vector3.Magnitude(control.GetPlayerTransform().position - transform.position) <= DISTANCE_THRESHOLD_LESS_THAN_TO_FIRE)
        {
            //Fire only if aiming in the general direction of the player
            float aimLinedUp = (Vector3.Dot(Vector3.Normalize(control.GetPlayerTransform().position - transform.position), transform.forward) + 1f) / 2f; //0 to 1 depending on how accurately facing the player
            if (aimLinedUp >= 0.95f || weaponBurst < WEAPON_BURST_MAX) //How close to aiming at target before willing to attempt firing
            {
                //Fire
                GetComponent<EnemyWeaponLaser>().Fire();

                if (weaponBurst <= 1)
                {
                    weaponBurst = WEAPON_BURST_MAX;
                    weaponCooldown = WEAPON_COOLDOWN_MAX;
                }
                else
                {
                    weaponBurst--;
                    weaponCooldown = WEAPON_COOLDOWN_WITHIN_BURST;
                }
            }
        }
    }

    public void SetStrength(int strength)
    {
        //Set the internal field for size
        this.strength = strength;

        //Modify attributes based on size
        if (this.strength == STRENGTH_SMALL)
        {
            modelGroup = modelGroupStrengthWeak;
            GetComponent<ParticlesDamageRock>().partSysShurikenDamageEmitCount = 250;
            GetComponent<ParticlesDamageRock>().partSysShurikenDamageShapeRadius = 3.2f;
            GetComponent<ParticlesDamageRock>().partSysShurikenDamageSizeMultiplier = 2f;
            rb.mass = 0.5f;
            health = 4;
        }
        else if (this.strength == STRENGTH_MEDIUM)
        {
            modelGroup = modelGroupStrengthMedium;
            GetComponent<ParticlesDamageRock>().partSysShurikenDamageEmitCount = 250;
            GetComponent<ParticlesDamageRock>().partSysShurikenDamageShapeRadius = 3.2f;
            GetComponent<ParticlesDamageRock>().partSysShurikenDamageSizeMultiplier = 2f;
            rb.mass = 0.5f;
            health = 4;
        }
        else if (this.strength == STRENGTH_LARGE)
        {
            modelGroup = modelGroupStrengthLarge;
            GetComponent<ParticlesDamageRock>().partSysShurikenDamageEmitCount = 250;
            GetComponent<ParticlesDamageRock>().partSysShurikenDamageShapeRadius = 3.2f;
            GetComponent<ParticlesDamageRock>().partSysShurikenDamageSizeMultiplier = 2f;
            rb.mass = 0.5f;
            health = 4;
        }
        else
        {
            Debug.LogError("Unrecognized code: " + this.strength);
        }

        //Activate the model
        //Get how many child objects are in the model group for the selected size class
        //Randomly pick a number from 0 to that length (we don't have to subtract one to format for the index which counts from zero because Random.Range max is exclusive when working with ints)
        //Select the child of that randomly selected number
        //Set that game object to active
        modelObject = modelGroup.transform.GetChild(Random.Range(0, modelGroup.transform.childCount)).gameObject;
        modelObject.SetActive(true);

        //Assign material and update particle system
        modelObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = matEnemy;
        GetComponent<ParticlesDamageRock>().SetParticleSystemDamageColour(modelObject.transform.GetChild(0), GetComponent<ParticlesDamageRock>().saturationDefault);
    }

    public void Damage(byte damageAmount, Vector3 direction, Vector3 position, bool oreDrop)
    {
        health = (byte)Mathf.Max(0f, health - damageAmount);
        if (health > 0)
        {
            GetComponent<ParticlesDamageRock>().EmitDamageParticles(1, direction, position, false);
        }
        else
        {
            health = 0;
            GetComponent<ParticlesDamageRock>().EmitDamageParticles(7, Vector3.zero, position, true);
            BreakApart(oreDrop);
        }
    }

    public void BreakApart(bool oreDrop)
    {
        if (!destroying)
        {
            //Disable self
            destroying = true;
            DisableModelAndTriggerVolumes();

            //Spawn goodies
            if (oreDrop)
            {
                SpawnClusterFromPoolAndPassRigidbodyValues(2, 3);

                if (strength == STRENGTH_LARGE)
                {
                    for (int i = 0; i < Random.Range(9, 15 + 1); i++)
                    {
                        SpawnOre();
                    }
                }
                else if (strength == STRENGTH_MEDIUM)
                {
                    for (int i = 0; i < Random.Range(7, 12 + 1); i++)
                    {
                        SpawnOre();
                    }
                }
                else if (strength == STRENGTH_SMALL)
                {
                    for (int i = 0; i < Random.Range(5, 9 + 1); i++)
                    {
                        SpawnOre();
                    }
                }
            }

            //Play break apart sound effect
            GetComponent<AudioSource>().Play();
        }
    }

    private void SetEnabled(bool enabled)
    {
        //This method should not be called directly except by Enable() and Disable()

        //Disable target triggers
        targetCollider1.enabled = enabled;
        targetCollider2.enabled = enabled;

        //Disable performant mode
        SetPerformant(false);

        //Disable/enable model
        if (modelObject != null)
        {
            modelObject.SetActive(enabled);
        }
        
        //Destroy or undestroy
        destroying = false;
        if (enabled)
        {
            gameObject.SetActive(true);
            destroyed = false;
            targetCollider1.enabled = true;
            targetCollider2.enabled = true;
        }
        else
        {
            destroyingTime = 0f;
            destroyed = true;
            targetCollider1.enabled = false;
            targetCollider2.enabled = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.ResetInertiaTensor();
            gameObject.SetActive(false);
        }
    }

    public void Enable(Vector3 position, int strength)
    {
        SetStrength(strength);
        transform.position = position;
        transform.rotation = Quaternion.Euler(
            Random.Range(0f, 360f),
            Random.Range(0f, 360f),
            Random.Range(0f, 360f)
        );
        SetEnabled(true);
    }

    public void Disable()
    {
        SetEnabled(false);
    }

    public void SpawnClusterFromPoolAndPassRigidbodyValues(int minCount, int maxCount)
    {
        for (int i = 0; i < Random.Range(minCount, maxCount + 1); i++)
        {
            GameObject instanceAsteroid = control.generation.AsteroidPoolSpawn(
                transform.position + (1.2f * new Vector3(Random.value, Random.value, Random.value)),
                Asteroid.SIZE_SMALL,
                Asteroid.TYPE_PLATINOID
            );

            instanceAsteroid.GetComponent<Asteroid>().PassRigidbodyValuesAndAddRandomForce(
                Vector3.one * ((0.5f + (0.5f * Random.value)) * 5f), //rb.velocity,
                rb.angularVelocity,
                rb.inertiaTensor,
                rb.inertiaTensorRotation
            );
        }
    }

    public void SetPerformant(bool performance)
    {
        //Don't bother with setting to the same value we already are at
        if (performance == performantMode)
        {
            return;
        }

        //Disables Update(), rigidbody, mesh collider (to be swapped out for sphere collider), and trigger volumes for improved performance (makes a big difference with 100 asteroids)
        if (performance)
        {
            rbMemVel = rb.velocity;
            rbMemAngularVel = rb.angularVelocity;

            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.isKinematic = true;
        }
        else
        {
            rb.velocity = rbMemVel;
            rb.angularVelocity = rbMemAngularVel;

            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        performantMode = performance;
        targetCollider1.enabled = !performance;
        targetCollider2.enabled = !performance;
        SetHitboxEnabledAndChoose(true);
    }

    public void DisableModelAndTriggerVolumes()
    {
        modelObject.SetActive(false);
        targetCollider1.enabled = false;
        targetCollider2.enabled = false;
    }

    private void SetHitboxEnabledAndChoose(bool enabled)
    {
        if (enabled)
        {
            modelGroup.GetComponent<SphereCollider>().enabled = performantMode;
            modelObject.GetComponent<MeshCollider>().enabled = !performantMode;
        }
        else
        {
            modelGroup.GetComponent<SphereCollider>().enabled = false;
            modelObject.GetComponent<MeshCollider>().enabled = false;
        }
    }

    public void PassRigidbodyValuesAndAddRandomForce(Vector3 velocity, Vector3 angularVelocity, Vector3 inertiaTensor, Quaternion inertiaTensorRotation)
    {
        rb.velocity = velocity;
        rb.angularVelocity = angularVelocity;
        rb.inertiaTensor = inertiaTensor;
        rb.inertiaTensorRotation = inertiaTensorRotation;

        //Add random force
        rb.AddForce(25f * new Vector3(
            0.5f + (0.5f * Random.value),
            0.5f + (0.5f * Random.value),
            0.5f + (0.5f * Random.value)
        ));
        rb.AddTorque(100f * new Vector3(
            Random.value,
            Random.value,
            Random.value
        ));
    }

    private void SpawnOre()
    {
        //Spawn with some of the position and speed randomized
        GameObject instanceOre = Instantiate(
            ore,
            transform.position + (0.8f * new Vector3(Random.value, Random.value, Random.value)),
            Quaternion.identity
        );
        //Put in Ore tree
        instanceOre.transform.parent = control.generation.ores.transform;

        //Rigidbody
        Rigidbody instanceOreRb = instanceOre.GetComponent<Rigidbody>();
        instanceOreRb.velocity = rb.velocity;
        instanceOreRb.angularVelocity = rb.angularVelocity;
        instanceOreRb.inertiaTensor = rb.inertiaTensor;
        instanceOreRb.inertiaTensorRotation = rb.inertiaTensorRotation;
        instanceOreRb.AddForce(1000f * new Vector3(
            0.5f + (0.5f * Random.value),
            0.5f + (0.5f * Random.value),
            0.5f + (0.5f * Random.value)
        ));
        instanceOreRb.AddTorque(5000f * new Vector3(Random.value, Random.value, Random.value));

        //Script
        Ore instanceOreScript = instanceOre.GetComponent<Ore>();
        instanceOreScript.control = control;
        instanceOreScript.type = Asteroid.TYPE_PLATINOID;
        instanceOreScript.parentVelocity = rb.velocity;
    }
}