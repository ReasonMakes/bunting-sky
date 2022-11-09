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

    [System.NonSerialized] public Vector3 lastForceAdded = Vector3.zero;

    //Weapons - all overridden by difficulty
    private float weaponCooldown = 0f; //Current burst - DO NOT EDIT
    private float weaponReloadPeriod = 2f; //Time in seconds between bursts
    private float weaponFirePeriod = 0.075f; //0.1f; //Time in seconds between shots within the burst
    private int weaponBurstIndex = 0; //Which projectile out of the burst we are currently on - DO NOT EDIT
    private int weaponMagSize = 12; //8; //4; //Total shots per burst
    private int tracerInterval = 5; //shots per tracer round

    //Behaviour settings
    [System.NonSerialized] public Vector3 spawnPointRaw = Vector3.zero;
    private Vector3 destinationPosition = Vector3.zero;
    private bool aggro = false;
    private float lastShotTime = -100f; //default to negative values so that we don't think we were just shot when we spawn!
    private readonly float LAST_SHOT_MEMORY_PERIOD = 10f; //How long in seconds the bandit remembers that they were recently shot for
    private readonly float DISTANCE_THRESHOLD_LESS_THAN_TO_AGGRO = 140f;
    private readonly float DISTANCE_THRESHOLD_GREATER_THAN_TO_MOVE_FORWARD = 16f;
    private float distanceThresholdLessThanToStrafe = 30f; //overwritten by difficulty
    private readonly float DISTANCE_THRESHOLD_LESS_THAN_TO_FIRE = 70f;
    [System.NonSerialized] public static readonly float DISTANCE_THRESHOLD_GREATER_THAN_PERFORMANT_MODE = 300f;
    private bool canStrafeHorizontally = true; //true;
    private bool strafeHorizontalRight = true;
    private float strafeHorizontalDirectionChangeTimer = 2f;
    private bool canStrafeVertically = false; //true;
    private bool strafeVerticalUp = true;
    private float strafeVerticalDirectionChangeTimer = 2f;
    private readonly float STRAFE_PERIOD_MIN = 0.5f; //3f;
    private readonly float STRAFE_PERIOD_MAX = 2.5f; //6f;
    public float inaccuracy = 0.25f;   //"Inaccuracy" (which in actuality actually helps the aim a little bit
    public GameObject playerGhost;                             //by creating a larger area-of-denial, and therefore a higher random
                                                               //chance of hitting the player despite their impossible-to-hit movement speed)
                                                               //OVERRRIDDEN BY DIFFICULTY

    //Performance
    [System.NonSerialized] public bool isDestroying = false;
    [System.NonSerialized] public bool isDestroyed = true;
    private float destroyingTime = 0f;
    [System.NonSerialized] public bool performantMode = false;
    [System.NonSerialized] public Vector3 rbMemVel;
    [System.NonSerialized] public Vector3 rbMemAngularVel;

    //Health
    [System.NonSerialized] public byte health = 1; //overridden by difficulty

    //Type
    public enum Strength {
        minor,
        major,
        elite,
        ultra
    };
    [System.NonSerialized] public Strength strength = Strength.minor;
    private int modelGroup = 0;
    [System.NonSerialized] public GameObject model;
    public Material matEnemy;
    [System.NonSerialized] public MeshRenderer meshRenderer;

    public GameObject ore;
    private readonly float ORE_POSITION_OFFSET_RANDOM_MAGNITUDE = 5f;

    //Death drops
    private int oreDropMin = 40;
    private int oreDropMax = 50;
    private Asteroid.Type oreDropType = Asteroid.Type.platinoid;

    //Sound
    public AudioSource soundSourceExplosion;
    [System.NonSerialized] public readonly float SOUND_EXPLOSION_VOLUME = 0.027f;

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
            if (!Menu.menuOpenAndGamePaused && !isDestroyed)
            {
                if (isDestroying)
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
                        if (aggro)
                        {
                            control.GetPlayerScript().nEnemiesAggrod--;
                            aggro = false;
                        }
                    }
                    else if (Time.time <= lastShotTime + LAST_SHOT_MEMORY_PERIOD)
                    {
                        if (!aggro)
                        {
                            control.GetPlayerScript().nEnemiesAggrod++;
                            aggro = true;
                        }
                    }
                    else
                    {
                        bool willAggroDueToProximity = (Vector3.Magnitude(control.GetPlayerTransform().position - transform.position) <= DISTANCE_THRESHOLD_LESS_THAN_TO_AGGRO);
                        if (!aggro && willAggroDueToProximity)
                        {
                            control.GetPlayerScript().nEnemiesAggrod++;
                            aggro = true;
                        }
                        else if (aggro && !willAggroDueToProximity)
                        {
                            control.GetPlayerScript().nEnemiesAggrod--;
                            aggro = false;
                        }
                    }

                    if (aggro)
                    {
                        //Lead, so that weapons fire is more likely to connect
                        float manualLeadMultiplier = 1f; //1.1f; //modifier to account for torque time 0.9 1.1
                        float manualPlayerVelocityMultiplier = 1f; //modifier to account for player acceleration //1.1
                        
                        //Improved aim for high difficulties
                        bool isAccurateBandit = false;
                        if ((int)strength < (int)Strength.elite)
                        {
                            manualLeadMultiplier = 1.1f;
                            manualPlayerVelocityMultiplier = 1.1f;
                            isAccurateBandit = true;
                        }

                        //Get destination position
                        destinationPosition = control.GetLeadPosition(
                            transform.position, rb.velocity, lastForceAdded, rb.mass,
                            EnemyWeaponLaser.PROJECTILE_SPEED, manualLeadMultiplier, 
                            control.GetPlayerTransform().position, control.GetPlayerScript().rb.velocity * manualPlayerVelocityMultiplier,
                            control.GetPlayerScript().lastForceAdded, control.GetPlayerScript().rb.mass,
                            isAccurateBandit
                        );

                        //Display destination position using ghost object (before randomness)
                        playerGhost.transform.position = destinationPosition;

                        //Add random offset IF FIRING and player moving
                        float distToPlayer = Vector3.Magnitude(control.GetPlayerTransform().position - transform.position);
                        //float playerSpeedMaxRough = 22f;
                        float playerSpeedWeight = 1f; //Mathf.Min(1f, control.GetPlayerScript().rb.velocity.magnitude / playerSpeedMaxRough);
                        float destinationOffsetMagnitude = Mathf.Max(1f, distToPlayer) * inaccuracy * playerSpeedWeight;
                        if (
                            GetIfAimingAtPlayer()
                            && GetIfWillingToFire()
                            && ((int)strength < (int)Strength.elite || control.GetPlayerScript().rb.velocity.magnitude > 1f) //elite+ difficulties don't vary their aim when the player stands still
                        )
                        { 
                            destinationPosition += new Vector3(
                                (Random.value - 0.5f) * destinationOffsetMagnitude,
                                (Random.value - 0.5f) * destinationOffsetMagnitude,
                                (Random.value - 0.5f) * destinationOffsetMagnitude
                            );
                        }
                    }
                    else
                    {
                        destinationPosition = GetSpawnPoint();
                    }
                    UpdateEnemyMovementTorque();
                    UpdateEnemyMovementThrust();
                    UpdateEnemyMovementDrag();
                    UpdateEnemyWeaponsUse(); 
                }
            }
        }

        //Keep track of last aggro time to detect when in combat!
        if (aggro)
        {
            control.GetPlayerScript().combatLastAggroTime = Time.time;
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
        Vector3 directionToDestinationToLookAt = Vector3.Normalize(destinationPosition - transform.position);

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
        Vector3 directionToPointToMoveTo = Vector3.Normalize(destinationPosition - transform.position);

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
        if (Vector3.Magnitude(destinationPosition - transform.position) > DISTANCE_THRESHOLD_GREATER_THAN_TO_MOVE_FORWARD)
        {
            forwardVector = transform.forward * dot;
        }

        //Strafe direction
        Vector3 strafeVector = Vector3.zero;

        //Change left/right & up/down periodically
        strafeHorizontalDirectionChangeTimer = Mathf.Max(0f, strafeHorizontalDirectionChangeTimer - Time.deltaTime); //decrement 1f per second
        strafeVerticalDirectionChangeTimer = Mathf.Max(0f, strafeVerticalDirectionChangeTimer - Time.deltaTime); //decrement 1f per second

        //Only change strafe direction when not firing/about to fire and when aiming at the player (so we don't get stuck in an infinite strafe loop)
        if (
            weaponCooldown > weaponFirePeriod * weaponMagSize
            && GetIfAimingAtPlayer()
        )
        {
            if (strafeHorizontalDirectionChangeTimer <= 0f)
            {
                //Reset timer
                strafeHorizontalDirectionChangeTimer = Random.Range(STRAFE_PERIOD_MIN, STRAFE_PERIOD_MAX);

                //Change strafe direction
                strafeHorizontalRight = !strafeHorizontalRight;
            }
            if (strafeVerticalDirectionChangeTimer <= 0f)
            {
                //Reset timer
                strafeVerticalDirectionChangeTimer = Random.Range(STRAFE_PERIOD_MIN, STRAFE_PERIOD_MAX);

                //Change strafe direction
                strafeVerticalUp = !strafeVerticalUp;
            }
        }
        
        //Strafe?
        if (aggro && GetIfAimingAtPlayer()) // && Vector3.Magnitude(destination - transform.position) <= DISTANCE_THRESHOLD_LESS_THAN_TO_STRAFE)
        {
            //Don't change strafe direction right before firing
            if (canStrafeHorizontally)
            {
                if (strafeHorizontalRight)
                {
                    //Right
                    strafeVector = transform.right;
                }
                else
                {
                    //Left
                    strafeVector = -transform.right;
                }
            }

            if (canStrafeVertically)
            {
                if (strafeVerticalUp)
                {
                    //Up
                    strafeVector += transform.up;
                }
                else
                {
                    //Down
                    strafeVector += -transform.up;
                }
            }
            
            //Normalize because we just added the horizontal and vertical strafe elements together
            strafeVector = strafeVector.normalized;

            //Factor in how accurately aiming at the player (strafe more when aiming at the player and less when trying to torque toward them)
            strafeVector *= dot;
        }
        
        //COMBINING AND APPLYING
        //Add forward and strafe vectors together with weights
        float distToPlayer = Vector3.Magnitude(control.GetPlayerTransform().position - transform.position);
        float strafeWeight = 0f; //weight approaches 1f as the distance from the enemy to the player approaches DISTANCE_THRESHOLD_LESS_THAN_TO_STRAFE
        if (
            (int)strength > (int)Strength.minor //minors don't plan ahead/are lazy and don't strafe if the player isn't shooting back, which because they have low thrust is a liability
            //|| (int)strength > (int)Strength.ultra //ultras wait until the player starts shooting to strafe, which is an improvement due to their high thrust - they dodge the shots
            || (control.GetPlayerScript().weaponUsedRecently > 0f && distToPlayer < DISTANCE_THRESHOLD_LESS_THAN_TO_AGGRO)
        )
        {
            float aggroDistMinusStrafeDist = DISTANCE_THRESHOLD_LESS_THAN_TO_AGGRO - distanceThresholdLessThanToStrafe;
            strafeWeight = (aggroDistMinusStrafeDist - (distToPlayer - distanceThresholdLessThanToStrafe)) / aggroDistMinusStrafeDist;
        }
        thrustVector = (forwardVector * (1f - strafeWeight)) + (strafeVector * (strafeWeight));

        //Thrust
        lastForceAdded = thrustVector.normalized * thrust * Time.deltaTime;
        rb.AddForce(lastForceAdded);
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
            if (GetIfWillingToFire())
            {
                //Ultras have weapon gimballing
                Vector3 gimbalDirection = transform.forward;
                if (strength == Strength.ultra)
                {
                    //Gimbal angular range (how accurately the bandit must be aiming at the player (relative to leading the target) for gimballing to activate)
                    if (GetAccuracyRelative() >= 0.999f)
                    {
                        //Gimbal snaps on to aim perfectly toward the destination for now
                        gimbalDirection = (destinationPosition - transform.position).normalized;
                    }
                }

                //Fire
                GetComponent<EnemyWeaponLaser>().Fire(gimbalDirection);

                if (weaponBurstIndex <= 1)
                {
                    //Reload
                    weaponBurstIndex = weaponMagSize;
                    weaponCooldown = weaponReloadPeriod;
                }
                else
                {
                    //Mag dump
                    weaponBurstIndex--;
                    weaponCooldown = weaponFirePeriod;
                }
            }
        }
    }

    private float GetAccuracyRaw()
    {
        //Accuracy relative to target's absolute position
        return (Vector3.Dot(Vector3.Normalize(control.GetPlayerTransform().position - transform.position), transform.forward) + 1f) / 2f;
    }

    private float GetAccuracyRelative()
    {
        //Accuracy relative to leading the target
        return (Vector3.Dot(Vector3.Normalize(destinationPosition - transform.position), transform.forward) + 1f) / 2f;
    }

    private bool GetIfAimingAtPlayer()
    {
        //0 to 1 depending on how accurately facing the player
        //float aimLinedUp = (Vector3.Dot(Vector3.Normalize(control.GetPlayerTransform().position - transform.position), transform.forward) + 1f) / 2f;
        //How close to aiming at target before willing to attempt firing
        float aimTolerance = 0.95f;
        bool aimingAtPlayer = GetAccuracyRelative() >= aimTolerance;

        return aimingAtPlayer;
    }

    private bool GetIfWillingToFire()
    {
        bool inTheMiddleOfABurst = weaponBurstIndex < weaponMagSize;

        bool willingToFire = (
            !control.GetPlayerScript().isDestroyed
            && (
                GetIfAimingAtPlayer()
                || inTheMiddleOfABurst
            )
        );

        return willingToFire;
    }

    public void SetStrength(Strength strength)
    {
        //Set the internal field for size
        this.strength = strength;

        //Difficulty
        if (this.strength == Strength.minor)
        {
            health = 6; //3;
            thrust = 2500f; //4e3f;
            torque = 600f;
            inaccuracy = 1f; //2f; //"inaccuracy" (some randomness actually helps to account for destination change during projectile travel time)

            weaponReloadPeriod = 2f; //Time in seconds between bursts
            weaponFirePeriod = 0.25f; //Time in seconds between shots within the burst
            weaponMagSize = 4; //Total shots per burst

            distanceThresholdLessThanToStrafe = 40f;
            tracerInterval = 1;

            oreDropMin = 12;
            oreDropMax = 20;
            oreDropType = Asteroid.Type.platinoid;
        }
        else if (this.strength == Strength.major)
        {
            health = 12; //6;
            thrust = 8e3f;
            torque = 16e3f; //10e3f;
            inaccuracy = 0.1f; //0.25f; //"inaccuracy" (some randomness actually helps to account for destination change during projectile travel time)

            weaponReloadPeriod = 2f; //Time in seconds between bursts
            weaponFirePeriod = 0.1f; //Time in seconds between shots within the burst
            weaponMagSize = 12; //Total shots per burst

            distanceThresholdLessThanToStrafe = 30f;
            tracerInterval = 3;

            oreDropMin = 20;
            oreDropMax = 30;
            oreDropType = Asteroid.Type.platinoid;
        }
        else if (this.strength == Strength.elite)
        {
            health = 8; //need less HP than majors because they are much harder to hit due to speed AND due to having to dodge their shots //10hp is player default; 20hp is player max health after upgrading hull strength
            //thrust = 10e3f;
            thrust = control.GetPlayerScript().THRUST;
            torque = 30e3f; //25e3f; //18e3f; //16e3f; //12e3f;
            inaccuracy = 0.1f; //"inaccuracy" (some randomness actually helps to account for destination change during projectile travel time)

            //weaponReloadPeriod = 1f; //Time in seconds between bursts
            //weaponReloadPeriod = control.GetPlayerScript().playerWeaponLaser.CLIP_COOLDOWN_DURATION; //Time in seconds between bursts
            weaponReloadPeriod = 1f; //0.7f; //Time in seconds between bursts
            //weaponInternalBurstCooldown = 0.03f; //0.05f //Time in seconds between shots within the burst
            //weaponInternalBurstCooldown = 1f - Mathf.Max(1f, 2f * control.GetPlayerScript().upgradeLevels[control.commerce.UPGRADE_FIRERATE]);
            weaponFirePeriod = 0.01f;
            //weaponBurstLength = 15; //Total shots per burst
            weaponMagSize = 2 * (control.GetPlayerScript().playerWeaponLaser.CLIP_SIZE_STARTER * int.Parse(control.commerce.upgradeDictionary[control.commerce.UPGRADE_DUAL_BATTERIES, control.commerce.UPGRADE_MAX_LEVEL]));

            distanceThresholdLessThanToStrafe = 30f;
            tracerInterval = 5;

            oreDropMin = 40;
            oreDropMax = 50;
            oreDropType = Asteroid.Type.preciousMetal;
        }
        else if (this.strength == Strength.ultra)
        {
            health = 10;

            distanceThresholdLessThanToStrafe = 1f;
            thrust = control.GetPlayerScript().THRUST;
            torque = 33e3f;

            inaccuracy = 0.07f; //0.0f;

            weaponReloadPeriod = 1f;
            weaponFirePeriod = 0.007f;
            weaponMagSize = 4 * (control.GetPlayerScript().playerWeaponLaser.CLIP_SIZE_STARTER * int.Parse(control.commerce.upgradeDictionary[control.commerce.UPGRADE_DUAL_BATTERIES, control.commerce.UPGRADE_MAX_LEVEL]));
            tracerInterval = 5;

            oreDropMin = 40;
            oreDropMax = 50;
            oreDropType = Asteroid.Type.preciousMetal;
        }
        else
        {
            Debug.LogError("Unrecognized strength code: " + this.strength);
        }

        //Set tracer interval
        EnemyWeaponLaser weaponScript = GetComponent<EnemyWeaponLaser>();
        int i = 0;
        foreach (GameObject projectile in weaponScript.POOL)
        {
            GameObject light = projectile.transform.Find("Point Light").gameObject;
            //GameObject emission = projectile.transform.Find("Emissive Model").gameObject;

            if (i % tracerInterval == 0)
            {
                light.SetActive(true);
                //emission.SetActive(true);
            }
            else
            {
                light.SetActive(false);
                //emission.SetActive(false);
            }
            
            i++;
        }

        //Model
        modelGroup = (int)strength;
        model = transform.Find("Model").GetChild(modelGroup).gameObject;
        model.SetActive(true);

        //Mesh collider
        model.GetComponent<MeshCollider>().sharedMesh = model.transform.GetChild(0).GetComponent<MeshFilter>().mesh;

        //Particle system
        model.transform.GetChild(0).GetComponent<MeshRenderer>().material = matEnemy;
        GetComponent<ParticlesDamageRock>().SetParticleSystemDamageColour(model.transform.GetChild(0), GetComponent<ParticlesDamageRock>().saturationDefault);
        GetComponent<ParticlesDamageRock>().partSysShurikenDamageEmitCount = 250;
        GetComponent<ParticlesDamageRock>().partSysShurikenDamageShapeRadius = 3.8f;
        GetComponent<ParticlesDamageRock>().partSysShurikenDamageSizeMultiplier = 2f;
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
            BreakApart(oreDrop);
        }
    }

    public void BreakApart(bool oreDrop)
    {
        if (!isDestroying)
        {
            //Disable self
            isDestroying = true;
            GetComponent<ParticlesDamageRock>().EmitDamageParticles(7, Vector3.zero, transform.position, true);
            control.GetPlayerScript().nEnemiesAggrod--;
            DisableModelAndTriggerVolumes();

            //Spawn drops
            if (oreDrop)
            {
                for (int i = 0; i < Random.Range(oreDropMin, oreDropMax + 1); i++)
                {
                    control.generation.OrePoolSpawnWithTraits(
                        transform.position + (ORE_POSITION_OFFSET_RANDOM_MAGNITUDE * new Vector3(Random.value, Random.value, Random.value)),
                        rb,
                        oreDropType
                    );
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
        weaponBurstIndex = weaponMagSize;

        //Disable target triggers
        targetCollider1.enabled = enabled;
        targetCollider2.enabled = enabled;

        //Disable performant mode
        SetPerformant(false);

        //Disable/enable model
        if (model != null)
        {
            model.SetActive(enabled);
        }
        
        //Destroy or undestroy
        isDestroying = false;
        if (enabled)
        {
            gameObject.SetActive(true);
            isDestroyed = false;
            targetCollider1.enabled = true;
            targetCollider2.enabled = true;
        }
        else
        {
            destroyingTime = 0f;
            isDestroyed = true;
            targetCollider1.enabled = false;
            targetCollider2.enabled = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.ResetInertiaTensor();
            gameObject.SetActive(false);
        }
    }

    public void Enable(Vector3 position, Strength strength)
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
        model.SetActive(false);
        targetCollider1.enabled = false;
        targetCollider2.enabled = false;
    }

    private void SetHitboxEnabledAndChoose(bool enabled)
    {
        if (enabled)
        {
            model.GetComponent<SphereCollider>().enabled = performantMode;
            model.GetComponent<MeshCollider>().enabled = !performantMode;
        }
        else
        {
            model.GetComponent<SphereCollider>().enabled = false;
            model.GetComponent<MeshCollider>().enabled = false;
        }
    }
}