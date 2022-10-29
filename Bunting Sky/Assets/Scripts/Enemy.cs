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
    private float thrust = 10e3f; //8e3f; //6e3f; //4000f; //OVERRIDDEN BY DIFFICULTY
    //Torque
    private float torque = 10e3f; //3000f; //600f;
    private readonly float ANGULAR_DRAG = 40f; //for smoothing
    //Drag
    private readonly float DRAG = 3f;

    //Weapons - all overridden by difficulty
    private float weaponCooldown = 0f; //Current burst - DO NOT EDIT
    private float weaponReloadPeriod = 2f; //Time in seconds between bursts
    private float weaponInternalBurstCooldown = 0.075f; //0.1f; //Time in seconds between shots within the burst
    private int weaponBurstIndex = 0; //Which projectile out of the burst we are currently on - DO NOT EDIT
    private int weaponBurstLength = 12; //8; //4; //Total shots per burst

    //Behaviour settings
    [System.NonSerialized] public Vector3 spawnPointRaw = Vector3.zero;
    private Vector3 destination = Vector3.zero;
    private bool aggro = false;
    private float lastShotTime = 0f;
    private readonly float LAST_SHOT_MEMORY_PERIOD = 10f; //How long in seconds the bandit remembers that they were recently shot for
    private readonly float DISTANCE_THRESHOLD_LESS_THAN_TO_AGGRO = 140f;
    private readonly float DISTANCE_THRESHOLD_GREATER_THAN_TO_MOVE_FORWARD = 16f;
    private readonly float DISTANCE_THRESHOLD_LESS_THAN_TO_STRAFE = 30f;
    private readonly float DISTANCE_THRESHOLD_LESS_THAN_TO_FIRE = 70f;
    [System.NonSerialized] public static readonly float DISTANCE_THRESHOLD_GREATER_THAN_PERFORMANT_MODE = 300f;
    private bool strafeRight = true;
    private float strafeDirectionChangeTimer = 2f;
    private readonly float STRAFE_PERIOD_MAX = 2f;
    private bool canStrafe = true;
    private readonly float MANUAL_LEAD_MULTIPLIER = 1.5f; //Add offset to account for how slow the torque is
    private float destinationRandomOffsetMultiplier = 0.25f;   //"Inaccuracy" (which in actuality actually helps the aim a little bit
    public GameObject playerGhost;                             //by creating a larger area-of-denial, and therefore a higher random
                                                               //chance of hitting the player despite their impossible-to-hit movement speed)
                                                               //OVERRRIDDEN BY DIFFICULTY

    //Performance
    [System.NonSerialized] public bool destroying = false;
    [System.NonSerialized] public bool destroyed = true;
    private float destroyingTime = 0f;
    [System.NonSerialized] public bool performantMode = false;
    [System.NonSerialized] public Vector3 rbMemVel;
    [System.NonSerialized] public Vector3 rbMemAngularVel;

    //Health
    [System.NonSerialized] public byte health = 1; //overridden by difficulty

    //Type
    [System.NonSerialized] public int strength = 0;
    [System.NonSerialized] public readonly static int STRENGTH_MINOR = 0;
    [System.NonSerialized] public readonly static int STRENGTH_MAJOR = 1;
    [System.NonSerialized] public readonly static int STRENGTH_ELITE = 2;
    private GameObject modelGroup;
    [System.NonSerialized] public GameObject modelObject;
    public GameObject modelGroupStrengthWeak;
    public GameObject modelGroupStrengthMedium;
    public GameObject modelGroupStrengthLarge;
    public Material matEnemy;
    
    [System.NonSerialized] public MeshRenderer meshRenderer;

    public GameObject ore;

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
                    else if (Time.time <= lastShotTime + LAST_SHOT_MEMORY_PERIOD)
                    {
                        aggro = true;
                    }
                    else
                    {
                        aggro = (Vector3.Magnitude(control.GetPlayerTransform().position - transform.position) <= DISTANCE_THRESHOLD_LESS_THAN_TO_AGGRO);
                    }

                    if (aggro)
                    {
                        //Lead, so that weapons fire is more likely to connect

                        //Target position
                        destination = control.GetPlayerTransform().position;

                        //Time until the projectiles hit the target
                        //t = d/v; time in seconds it will take the weapon projectile to be at the target destination
                        float timeToTarget = Vector3.Magnitude(control.GetPlayerTransform().position - transform.position) / EnemyWeaponLaser.PROJECTILE_SPEED;

                        //Lead speed
                        destination += (control.GetPlayerScript().rb.velocity * (timeToTarget * MANUAL_LEAD_MULTIPLIER));

                        //Lead acceleration
                        //F = ma -> a = F/m
                        Vector3 playerAcceleration = control.GetPlayerScript().lastForceAdded / control.GetPlayerScript().rb.mass;
                        //displacement = velocity * deltaTime + (1/2)​(acceleration)(deltaTime^2)
                        Vector3 displacementFromAcceleration = (control.GetPlayerScript().rb.velocity * Time.deltaTime) + ((playerAcceleration * Mathf.Pow(Time.deltaTime, 2f)) / 2f);
                        destination += displacementFromAcceleration;

                        //Lead change in thrust direction (for if player is thrusting in a circle around the enemy)
                        //YET TO BE IMPLEMENTED

                        //Display target destination (before randomness)
                        playerGhost.transform.position = destination;

                        //Add random offset IF FIRING
                        float distanceThresholdMoreThanAddRandomOffset = 0f;
                        float distToPlayer = Vector3.Magnitude(control.GetPlayerTransform().position - transform.position);
                        float playerSpeedMaxRough = 22f;
                        float playerSpeedWeight = Mathf.Min(1f, control.GetPlayerScript().rb.velocity.magnitude / playerSpeedMaxRough);
                        float destinationOffsetMagnitude = distToPlayer * destinationRandomOffsetMultiplier * playerSpeedWeight;
                        if (distToPlayer >= distanceThresholdMoreThanAddRandomOffset && weaponBurstIndex < weaponBurstLength && control.GetPlayerScript().rb.velocity.magnitude > 1f)
                        {
                            destination += new Vector3(
                                (Random.value - 0.5f) * destinationOffsetMagnitude,
                                (Random.value - 0.5f) * destinationOffsetMagnitude,
                                (Random.value - 0.5f) * destinationOffsetMagnitude
                            );
                        }
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
        rb.angularDrag = ANGULAR_DRAG;

        //DIRECTION
        //Vector to look toward
        Vector3 directionToDestinationToLookAt = Vector3.Normalize(destination - transform.position);

        //The rotation to look at that point
        Quaternion rotationToLookAtDestination = Quaternion.identity;
        if (directionToDestinationToLookAt != Vector3.zero)
        {
            rotationToLookAtDestination = Quaternion.LookRotation(directionToDestinationToLookAt);
        }

        //The rotation from how the ship is currently rotated to looking at the destination
        //deltaRotation = finalRot - initialRot
        //(Multiplying by inverse is equivalent to subtracting)
        Quaternion rotation = rotationToLookAtDestination * Quaternion.Inverse(rb.rotation);

        //Parse Quaternion to Vector3
        Vector3 torqueVector = new Vector3(rotation.x, rotation.y, rotation.z) * rotation.w;

        //STRENGTH
        //Adding all modifiers together
        float torqueStrength = torque * rb.angularDrag * Time.deltaTime;

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
        if (aggro) // && Vector3.Magnitude(destination - transform.position) <= DISTANCE_THRESHOLD_LESS_THAN_TO_STRAFE)
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
        if (canStrafe)
        {
            float distToPlayer = Vector3.Magnitude(control.GetPlayerTransform().position - transform.position);
            float weight = 0f; //weight approaches 1f as the distance from the enemy to the player approaches DISTANCE_THRESHOLD_LESS_THAN_TO_STRAFE
            if (control.GetPlayerScript().weaponUsedRecently > 0f && distToPlayer < DISTANCE_THRESHOLD_LESS_THAN_TO_AGGRO)
            {
                float aggroDistMinusStrafeDist = DISTANCE_THRESHOLD_LESS_THAN_TO_AGGRO - DISTANCE_THRESHOLD_LESS_THAN_TO_STRAFE;
                weight = (aggroDistMinusStrafeDist - (distToPlayer - DISTANCE_THRESHOLD_LESS_THAN_TO_STRAFE)) / aggroDistMinusStrafeDist;
            }
            thrustVector = (forwardVector * (1f - weight)) + (strafeVector * (weight));
        }
        else
        {
            thrustVector = forwardVector;
        }

        //Thrust
        rb.AddForce(thrustVector.normalized * thrust * Time.deltaTime);
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
            if (aimLinedUp >= 0.95f || weaponBurstIndex < weaponBurstLength) //How close to aiming at target before willing to attempt firing
            {
                //Fire
                GetComponent<EnemyWeaponLaser>().Fire();

                if (weaponBurstIndex <= 1)
                {
                    weaponBurstIndex = weaponBurstLength;
                    weaponCooldown = weaponReloadPeriod;
                }
                else
                {
                    weaponBurstIndex--;
                    weaponCooldown = weaponInternalBurstCooldown;
                }
            }
        }
    }

    public void SetStrength(int strength)
    {
        //Set the internal field for size
        this.strength = strength;

        GetComponent<ParticlesDamageRock>().partSysShurikenDamageEmitCount = 250;
        GetComponent<ParticlesDamageRock>().partSysShurikenDamageShapeRadius = 3.8f;
        GetComponent<ParticlesDamageRock>().partSysShurikenDamageSizeMultiplier = 2f;
        rb.mass = 0.5f;

        //Modify attributes based on size
        if (this.strength == STRENGTH_MINOR)
        {
            //Physical size
            modelGroup = modelGroupStrengthWeak;
            
            //Difficulty
            health = 3;
            thrust = 4e3f;
            torque = 600f;
            destinationRandomOffsetMultiplier = 2f; //"inaccuracy" (some randomness actually helps to account for destination change during projectile travel time)

            weaponReloadPeriod = 2f; //Time in seconds between bursts
            weaponInternalBurstCooldown = 0.25f; //Time in seconds between shots within the burst
            weaponBurstLength = 4; //Total shots per burst
        }
        else if (this.strength == STRENGTH_MAJOR)
        {
            //Physical size
            modelGroup = modelGroupStrengthWeak;

            //Difficulty
            health = 6; //same as player's default
            thrust = 8e3f;
            torque = 10e3f;
            destinationRandomOffsetMultiplier = 0.25f; //"inaccuracy" (some randomness actually helps to account for destination change during projectile travel time)

            weaponReloadPeriod = 2f; //Time in seconds between bursts
            weaponInternalBurstCooldown = 0.1f; //Time in seconds between shots within the burst
            weaponBurstLength = 12; //Total shots per burst
        }
        else if (this.strength == STRENGTH_ELITE)
        {
            //Physical size
            modelGroup = modelGroupStrengthWeak;

            //Difficulty
            health = 10; //20hp is player max health after upgrading hull strength
            thrust = 10e3f;
            torque = 12e3f;
            destinationRandomOffsetMultiplier = 0.4f; //"inaccuracy" (some randomness actually helps to account for destination change during projectile travel time)

            weaponReloadPeriod = 1f; //Time in seconds between bursts
            weaponInternalBurstCooldown = 0.05f; //Time in seconds between shots within the burst
            weaponBurstLength = 18; //Total shots per burst
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

    public void Damage(byte damageAmount, Vector3 direction, Vector3 position, bool oreDrop, bool shotByPlayer)
    {
        health = (byte)Mathf.Max(0f, health - damageAmount);
        if (health > 0)
        {
            GetComponent<ParticlesDamageRock>().EmitDamageParticles(1, direction, position, false);

            if (shotByPlayer)
            {
                lastShotTime = Time.time;
            }
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
                if (strength == STRENGTH_ELITE)
                {
                    for (int i = 0; i < Random.Range(9, 15 + 1); i++)
                    {
                        SpawnOre();
                    }
                }
                else if (strength == STRENGTH_MAJOR)
                {
                    for (int i = 0; i < Random.Range(7, 12 + 1); i++)
                    {
                        SpawnOre();
                    }
                }
                else if (strength == STRENGTH_MINOR)
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

        //Reset weapon
        weaponBurstIndex = weaponBurstLength;

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

    private void SpawnOre()
    {
        //Pool spawning
        GameObject instanceOre = control.generation.OrePoolSpawn(
            transform.position + (0.8f * new Vector3(Random.value, Random.value, Random.value)),
            Asteroid.TYPE_PLATINOID,
            rb.velocity
        );

        //Pass rigidbody values
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
    }
}