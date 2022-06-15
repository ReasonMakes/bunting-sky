using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [System.NonSerialized] public Control control;
    private Transform playerTran;
    public SphereCollider targetCollider1;
    public SphereCollider targetCollider2;
    public Rigidbody rb;

    [System.NonSerialized] public bool destroying = false;
    [System.NonSerialized] public bool destroyed = true;
    private float destroyingTime = 0f;
    [System.NonSerialized] public bool performantMode = false;

    [System.NonSerialized] public readonly static byte HEALTH_MAX = 4;
    [System.NonSerialized] public byte health = HEALTH_MAX;

    [System.NonSerialized] public bool separating = true;
    private readonly float INTERSECTING_REPEL_TELEPORT_STEP_DIST = 0.03f;

    [System.NonSerialized] public int size;
    [System.NonSerialized] public static int SIZE_SMALL = 0;
    [System.NonSerialized] public static int SIZE_MEDIUM = 1;
    [System.NonSerialized] public static int SIZE_LARGE = 2;
    private GameObject modelGroup;
    private GameObject modelObject;
    public GameObject modelGroupSizeSmall;
    public GameObject modelGroupSizeMedium;
    public GameObject modelGroupSizeLarge;

    [System.NonSerialized] public byte type = 0;
    [System.NonSerialized] public static readonly byte TYPE_PLATINOID = 0;
    [System.NonSerialized] public static readonly byte TYPE_PRECIOUS_METAL = 1;
    [System.NonSerialized] public static readonly byte TYPE_WATER = 2;
    public Material matPlatinoid;
    public Material matPreciousMetal;
    public Material matWater;
    
    [System.NonSerialized] public MeshRenderer meshRenderer;

    public GameObject ore;

    private void Start()
    {
        playerTran = control.generation.instancePlayer.transform.Find("Body");
    }

    private void Update()
    {
        if (!performantMode)
        {
            //Slow update
            if (Time.frameCount % 30 == 0)
            {
                SlowUpdate();
            }

            //Destruction
            if (!Menu.menuOpenAndGamePaused && !destroyed)
            {
                //bool particlesFadedOut = destroyingTime >= GetComponent<ParticlesDamageRock>().particlesDamageRock.emission.rateOverTime.constant;
                bool particlesFadedOut = destroyingTime >= 15f; //particles technically don't fade out for 75 seconds, but they aren't actually visible after 9 seconds so this should be fine
                bool playerBeyondArbitraryDistance = Vector3.Distance(transform.position, playerTran.transform.position) >= playerTran.GetComponent<Player>().ORBITAL_DRAG_MODE_THRESHOLD;
                if (destroying)
                {
                    //Debug.Log("Destroying. Time: " + destroyingTime + "/" + GetComponent<ParticlesDamageRock>().particlesDamageRock.emission.rateOverTime.constant);
                    //Debug.Log("Distance: " + Vector3.Distance(transform.position, playerTran.transform.position) + "/" + playerTran.GetComponent<Player>().ORBITAL_DRAG_MODE_THRESHOLD);

                    //Increment timer
                    destroyingTime += Time.deltaTime;

                    //Disable model and trigger volumes
                    modelObject.SetActive(false);
                    targetCollider1.enabled = false;
                    targetCollider2.enabled = false;

                    //Wait for particles to fade out before disabling trigger volume
                    if (particlesFadedOut && playerBeyondArbitraryDistance)
                    {
                        //Debug.Log("Disabled from particle fade out and player left");
                        //Disable
                        Disable();
                    }
                }
            }
        }
    }

    private void SlowUpdate()
    {
        //Destroy asteroids that are out of play
        if (!destroyed)
        {
            if (Vector3.Distance(transform.position, control.generation.instanceHomePlanet.transform.position) > Mathf.Pow(control.generation.MOONS_SPACING_BASE_MAX, control.generation.MOONS_SPACING_POWER) + 250f
            && Vector3.Distance(transform.position, playerTran.position) > 400.0f)
            {
                destroying = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!performantMode && !Menu.menuOpenAndGamePaused && !destroyed && separating)
        {
            Separate();
        }
    }

    private void Separate()
    {
        //Debug.Log("Checking to separate");

        //Ignore all collisions until separated from siblings (this will ignore collisions with player and with weapons, but should only last a few milliseconds)
        int nActiveAsteroids = control.generation.asteroidsEnabled.transform.childCount;
        if (nActiveAsteroids > 1)
        {
            for (int nActiveAsteroidsChecked = 0; nActiveAsteroidsChecked < nActiveAsteroids; nActiveAsteroidsChecked++)
            {
                Transform asteroidToCheck = control.generation.asteroidsEnabled.transform.GetChild(nActiveAsteroidsChecked);
                if (asteroidToCheck.gameObject != gameObject)
                {
                    if (modelObject.transform.GetComponent<MeshCollider>().bounds.Intersects(
                        asteroidToCheck.GetComponent<Asteroid>().modelObject.transform.GetComponent<MeshCollider>().bounds
                    ))
                    {
                        //Repel from the intersected asteroid
                        //Get repel direction
                        Vector3 repelDir = (transform.position - asteroidToCheck.transform.position).normalized;

                        //Ensure we have a direction, even if spawning exactly inside each other by chance
                        if (repelDir == Vector3.zero)
                        {
                            repelDir = new Vector3(Random.value, Random.value, Random.value);
                        }

                        //Debug.Log("Intersecting. Repel dir: " + repelDir);

                        //Repel
                        transform.position += INTERSECTING_REPEL_TELEPORT_STEP_DIST * repelDir;

                        //Skip the code below
                        return;
                    }
                }
            }
        }

        //Once we aren't intersecting anything anymore then we'll get to this point in the code
        //Debug.Log("Separated; re-enabling collisions");
        modelObject.transform.GetComponent<MeshCollider>().enabled = true;
        rb.detectCollisions = true;
        separating = false;
    }

    public static int GetRandomSize()
    {
        //Randomly choose size
        switch (Random.Range(0, 2 + 1)) //int range is exclusive, so have to add 1 to the max value
        {
            case 0:
                return SIZE_SMALL;
            case 1:
                return SIZE_MEDIUM;
            case 2:
                return SIZE_LARGE;
            default:
                return -1;
        }
    }

    //THIS MUST BE CALLED BEFORE TYPE IS SET
    public void SetSize(int size)
    {
        //Set the internal field for size
        this.size = size;

        //Modify attributes based on size
        if (this.size == SIZE_SMALL)
        {
            modelGroup = modelGroupSizeSmall;
            rb.mass = 0.2f;
            GetComponent<ParticlesDamageRock>().partSysShurikenDamageEmitCount = 50;
            GetComponent<ParticlesDamageRock>().partSysShurikenDamageShapeRadius = 0.2f;
            GetComponent<ParticlesDamageRock>().partSysShurikenDamageSizeMultiplier = 1f;
            health = (byte)Random.Range(1, 3);
        }
        else if (this.size == SIZE_MEDIUM)
        {
            modelGroup = modelGroupSizeMedium;
            GetComponent<ParticlesDamageRock>().partSysShurikenDamageEmitCount = 150;
            GetComponent<ParticlesDamageRock>().partSysShurikenDamageShapeRadius = 1.3f;
            GetComponent<ParticlesDamageRock>().partSysShurikenDamageSizeMultiplier = 1.2f;
            rb.mass = 1.0f;
            health = (byte)Random.Range(2, 5);
        }
        else if (this.size == SIZE_LARGE)
        {
            modelGroup = modelGroupSizeLarge;
            GetComponent<ParticlesDamageRock>().partSysShurikenDamageEmitCount = 250;
            GetComponent<ParticlesDamageRock>().partSysShurikenDamageShapeRadius = 3.2f;
            GetComponent<ParticlesDamageRock>().partSysShurikenDamageSizeMultiplier = 2f;
            rb.mass = 10.0f;
            health = (byte)Random.Range(4, 8);
        }

        //Activate the model
        //Get how many child objects are in the model group for the selected size class
        //Randomly pick a number from 0 to that length (we don't have to subtract one to format for the index which counts from zero because Random.Range max is exclusive when working with ints)
        //Select the child of that randomly selected number
        //Set that game object to active
        modelObject = modelGroup.transform.GetChild(Random.Range(0, modelGroup.transform.childCount)).gameObject;
        modelObject.SetActive(true);
    }

    //SIZE MUST BE SET BEFORE TYPE CAN BE
    public void SetType(byte typeToSetAs)
    {
        if (modelGroup == null) Debug.LogError("Error: must set asteroid size before setting type");

        //Set type
        type = typeToSetAs;

        //Assign material equal to type
        if (type == TYPE_PLATINOID)
        {
            modelObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = matPlatinoid; 
        }
        else if (type == TYPE_PRECIOUS_METAL)
        {
            modelObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = matPreciousMetal;
        }
        else if(type == TYPE_WATER)
        {
            modelObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = matWater;
        }

        GetComponent<ParticlesDamageRock>().SetParticleSystemDamageColour(modelObject.transform.GetChild(0), GetComponent<ParticlesDamageRock>().saturationDefault);
    }

    public static byte GetRandomType()
    {
        return (byte)Random.Range(0, Ore.typeLength);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Fatal collisions
        if (collision.collider.gameObject.name == control.generation.moon.name + "(Clone)"
            || collision.collider.gameObject.name == control.generation.planet.name + "(Clone)")
        {
            //Destroy self, but don't drop any ore
            Damage(health, Vector3.zero, transform.position, false);
        }
    }

    public void Damage(byte damageAmount, Vector3 direction, Vector3 position, bool oreDrop)
    {
        //Debug.Log("Type " + type + " asteroid damaged. " + health + " HP remaining.");

        health = (byte)Mathf.Max(0f, health - damageAmount);
        //health -= damageAmount;
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
            //Disable();
            destroying = true;

            //Spawn smaller asteroids
            if (size == SIZE_LARGE)
            {
                if (oreDrop) { for (int i = 0; i < Random.Range(5, 9 + 1); i++) SpawnOre(); };
                for (int i = 0; i < Random.Range(2, 3 + 1); i++) { control.generation.SpawnAsteroidFromPool(transform.position + (1.2f * new Vector3(Random.value, Random.value, Random.value)), Asteroid.SIZE_MEDIUM, type); }
                for (int i = 0; i < Random.Range(3, 6 + 1); i++) { control.generation.SpawnAsteroidFromPool(transform.position + (1.2f * new Vector3(Random.value, Random.value, Random.value)), Asteroid.SIZE_SMALL, type); }
            }
            else if (size == SIZE_MEDIUM)
            {
                if (oreDrop) { for (int i = 0; i < Random.Range(3, 6 + 1); i++) SpawnOre(); };
                for (int i = 0; i < Random.Range(2, 4 + 1); i++) { control.generation.SpawnAsteroidFromPool(transform.position + (1.2f * new Vector3(Random.value, Random.value, Random.value)), Asteroid.SIZE_SMALL, type); }
            }
            else if (size == SIZE_SMALL)
            {
                if (oreDrop) { for (int i = 0; i < Random.Range(1, 2 + 1); i++) SpawnOre(); };
            }
            //switch (sizeClassDisplay)
            //{
            //    case "Large":
            //        if (oreDrop) { for (int i = 0; i < Random.Range(5, 10); i++) SpawnOre(); }
            //        for (int i = 0; i < Random.Range(2, 4); i++) SpawnAsteroid("Medium");
            //        for (int i = 0; i < Random.Range(3, 8); i++) SpawnAsteroid("Small");
            //        break;
            //
            //    case "Medium":
            //        if (oreDrop) { for (int i = 0; i < Random.Range(3, 7); i++) SpawnOre(); }
            //        for (int i = 0; i < Random.Range(2, 5); i++) SpawnAsteroid("Small");
            //        break;
            //
            //    case "Small":
            //        if (oreDrop) { for (int i = 0; i < Random.Range(1, 3); i++) SpawnOre(); }
            //        break;
            //}

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
        if (enabled == true)
        {
            gameObject.SetActive(true);
            separating = true;
            destroyed = false;
            targetCollider1.enabled = true;
            targetCollider2.enabled = true;
            transform.parent = control.generation.asteroidsEnabled.transform;
            control.generation.asteroidsEnabled.name = "Enabled (" + control.generation.asteroidsEnabled.transform.childCount + ")";
            control.generation.asteroidsDisabled.name = "Disabled (" + control.generation.asteroidsDisabled.transform.childCount + ")";
        }
        else
        {
            destroyingTime = 0f;
            destroyed = true;
            targetCollider1.enabled = false;
            targetCollider2.enabled = false;
            transform.parent = control.generation.asteroidsDisabled.transform;
            control.generation.asteroidsEnabled.name = "Enabled (" + control.generation.asteroidsEnabled.transform.childCount + ")";
            control.generation.asteroidsDisabled.name = "Disabled (" + control.generation.asteroidsDisabled.transform.childCount + ")";
            rb.detectCollisions = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.ResetInertiaTensor();
            gameObject.SetActive(false);
        }
    }

    public void Enable(Vector3 position, int size, byte type)
    {
        SetSize(size);
        SetType(type);
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
        //Disables Update(), rigidbody, and mesh collider (to be swapped out for sphere collider) for improved performance (makes a big difference with 100 asteroids)
        performantMode = performance;
        rb.isKinematic = performance;
        if (!separating)
        {
            SetHitboxEnabledAndChoose(true);
        }
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

    private void SpawnAsteroid(int size)
    {
        //Instantiate at parent position, plus some randomness
        GameObject instanceCBodyAsteroid = Instantiate(
            control.generation.asteroid,
            transform.position + (1.2f * new Vector3(Random.value, Random.value, Random.value)),
            Quaternion.Euler(
                Random.Range(0f, 360f),
                Random.Range(0f, 360f),
                Random.Range(0f, 360f)
            )
        );
        //Put in CBodies tree
        instanceCBodyAsteroid.transform.parent = control.generation.asteroids.transform;

        //Rigidbody
        Rigidbody instanceCBodyAsteroidRb = instanceCBodyAsteroid.GetComponent<Rigidbody>();
        //Ignore all collisions unless explicitly enabled (once asteroid is separated from siblings)
        instanceCBodyAsteroidRb.detectCollisions = false;
        //Copy velocity and add some random impulse force
        instanceCBodyAsteroidRb.velocity = rb.velocity;
        instanceCBodyAsteroidRb.angularVelocity = rb.angularVelocity;
        instanceCBodyAsteroidRb.inertiaTensor = rb.inertiaTensor;
        instanceCBodyAsteroidRb.inertiaTensorRotation = rb.inertiaTensorRotation;
        instanceCBodyAsteroidRb.AddForce(25f * new Vector3(
            0.5f + (0.5f * Random.value),
            0.5f + (0.5f * Random.value),
            0.5f + (0.5f * Random.value)
        ));
        instanceCBodyAsteroidRb.AddTorque(100f * new Vector3(
            Random.value,
            Random.value,
            Random.value
        ));

        //Script
        Asteroid instanceCBodyAsteroidScript = instanceCBodyAsteroid.GetComponent<Asteroid>();
        instanceCBodyAsteroidScript.control = control;
        instanceCBodyAsteroidScript.SetSize(size);
        instanceCBodyAsteroidScript.SetType(type);
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
        instanceOreScript.type = type;
        instanceOreScript.parentVelocity = rb.velocity;
    }
}