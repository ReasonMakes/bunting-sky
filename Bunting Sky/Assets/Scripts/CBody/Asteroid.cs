using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    private GameObject model;
    private GameObject activeModel;
    public string sizeClassDisplay;
    public Rigidbody rb;

    public GameObject modelClassLarge;
    public GameObject modelClassMedium;
    public GameObject modelClassSmall;

    [System.NonSerialized] public Control control;

    public GameObject ore;

    public MeshRenderer meshRenderer;
    public Material matPlatinoid;
    public Material matPreciousMetal;
    public Material matWater;
    [System.NonSerialized] public byte type = 0; //0 = Platinoids, 1 = PreciousMetal, 2 = Water

    //public GameObject particlesShurikenDamageObj;

    [System.NonSerialized] public readonly static byte HEALTH_MAX = 4;
    [System.NonSerialized] public byte health = HEALTH_MAX;
    [System.NonSerialized] public bool destroyed = false;
    private float destroyedTime = 0f;

    Transform playerTran;
    public SphereCollider targetCollider1;
    public SphereCollider targetCollider2;
    public SphereCollider targetCollider3;
    public SphereCollider targetCollider4;

    public bool separating = true;
    //private readonly float INTERSECTING_REPEL_FORCE = 0.03f;
    private readonly float INTERSECTING_REPEL_TELEPORT_STEP_DIST = 0.03f;

    private void Start()
    {
        playerTran = control.generation.instancePlayer.transform.Find("Body");
    }

    private void Update()
    {
        //Slow update
        if (Time.frameCount % 30 == 0)
        {
            SlowUpdate();
        }

        //Destruction
        if (!Menu.menuOpenAndGamePaused)
        {
            bool particlesFadedOut = destroyedTime >= GetComponent<ParticlesDamageRock>().particlesDamageRock.emission.rateOverTime.constant;
            bool playerBeyondArbitraryDistance = Vector3.Distance(transform.position, playerTran.transform.position) >= playerTran.GetComponent<Player>().ORBITAL_DRAG_MODE_THRESHOLD;
            if (destroyed && particlesFadedOut && playerBeyondArbitraryDistance)
            {
                Destroy(gameObject, 0f);
            }
            destroyedTime += Time.deltaTime;
        }
    }

    private void SlowUpdate()
    {
        //Destroy asteroids that are out of play
        if (Vector3.Distance(transform.position, control.generation.instanceHomePlanet.transform.position) > Mathf.Pow(control.generation.MOONS_SPACING_BASE_MAX, control.generation.MOONS_SPACING_POWER) + 250f
            && Vector3.Distance(transform.position, playerTran.position) > 400.0f)
        {
            //Debug.Log("Asteroid that was too far from centre star and player has been destroyed.");
            Destroy(gameObject, 0f);
        }
    }

    private void FixedUpdate()
    {
        if (!Menu.menuOpenAndGamePaused && separating)
        {
            Separate();
        }
    }

    private void Separate()
    {
        //Ignore all collisions until separated from siblings (problem: this will ignore collisions with player and with weapons)

        Asteroid[] asteroids = FindObjectsOfType<Asteroid>();
        foreach (Asteroid asteroid in asteroids)
        {
            if (asteroid != this)
            {
                if (activeModel.transform.GetComponent<MeshCollider>().bounds.Intersects(
                    asteroid.GetComponent<Asteroid>().activeModel.transform.GetComponent<MeshCollider>().bounds
                ))
                {
                    //Debug.Log("Intersecting");

                    Vector3 repelDir = (transform.position - asteroid.transform.position).normalized;
                    //rb.AddForce(INTERSECTING_REPEL_FORCE * (transform.position - asteroid.transform.position).normalized * Time.deltaTime);
                    //rb.AddForce(INTERSECTING_REPEL_FORCE * repelDir * Time.deltaTime);
                    transform.position += INTERSECTING_REPEL_TELEPORT_STEP_DIST * repelDir;

                    //Debug.Log(Time.time + ": Moved intersecting asteroid: " + repelDir);

                    return;
                }
            }
        }

        activeModel.transform.GetComponent<MeshCollider>().enabled = true;
        rb.detectCollisions = true;
        separating = false;
    }

    public static string GetRandomSize()
    {
        //Randomly choose size
        switch (Random.Range(0, 3)) //int range is exclusive, so have to add 1 to the max value
        {
            case 0:
                return "Small";
            case 1:
                return "Medium";
            case 2:
                return "Large";
            default:
                return "error";
        }
    }

    //THIS MUST BE CALLED BEFORE TYPE IS SET
    public void SetSize(string size)
    {
        //Set the internal field for size
        sizeClassDisplay = size;
        
        //Modify attributes based on size
        switch (size)
        {
            case "Small":
                model = modelClassSmall;
                rb.mass = 0.2f;
                GetComponent<ParticlesDamageRock>().partSysShurikenDamageEmitCount = 50;
                GetComponent<ParticlesDamageRock>().partSysShurikenDamageShapeRadius = 0.2f;
                GetComponent<ParticlesDamageRock>().partSysShurikenDamageSizeMultiplier = 1f;
                health = (byte)Random.Range(1, 3);
                break;

            case "Medium":
                model = modelClassMedium;
                GetComponent<ParticlesDamageRock>().partSysShurikenDamageEmitCount = 150;
                GetComponent<ParticlesDamageRock>().partSysShurikenDamageShapeRadius = 1.3f;
                GetComponent<ParticlesDamageRock>().partSysShurikenDamageSizeMultiplier = 1.2f;
                rb.mass = 1.0f;
                health = (byte)Random.Range(2, 5);
                break;

            case "Large":
                model = modelClassLarge;
                GetComponent<ParticlesDamageRock>().partSysShurikenDamageEmitCount = 250;
                GetComponent<ParticlesDamageRock>().partSysShurikenDamageShapeRadius = 3.2f;
                GetComponent<ParticlesDamageRock>().partSysShurikenDamageSizeMultiplier = 2f;
                rb.mass = 10.0f;
                health = (byte)Random.Range(4, 8);
                break;
        }

        //Activate the model
        //Get how many child objects are in the model group for the selected size class
        //Randomly pick a number from 0 to that length (we don't have to subtract one to format for the index which counts from zero because Random.Range max is exclusive when working with ints)
        //Select the child of that randomly selected number
        //Set that game object to active
        activeModel = model.transform.GetChild(Random.Range(0, model.transform.childCount)).gameObject;
        activeModel.SetActive(true);
    }

    //SIZE MUST BE SET BEFORE TYPE CAN BE
    public void SetType(byte typeToSetAs)
    {
        if (model == null) Debug.LogError("Error: must set asteroid size before setting type");

        //Set type
        type = typeToSetAs;

        //Assign material equal to type
        switch (type)
        {
            case 0:
                activeModel.transform.GetChild(0).GetComponent<MeshRenderer>().material = matPlatinoid;
                break;
            case 1:
                activeModel.transform.GetChild(0).GetComponent<MeshRenderer>().material = matPreciousMetal;
                break;
            case 2:
                activeModel.transform.GetChild(0).GetComponent<MeshRenderer>().material = matWater;
                break;
        }

        GetComponent<ParticlesDamageRock>().SetParticleSystemDamageColour(activeModel.transform.GetChild(0), GetComponent<ParticlesDamageRock>().saturationDefault);
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
        if (!destroyed)
        {
            //Disable self
            targetCollider1.enabled = false;
            targetCollider2.enabled = false;
            targetCollider3.enabled = false;
            targetCollider4.enabled = false;
            rb.detectCollisions = false;
            activeModel.SetActive(false);

            switch (sizeClassDisplay)
            {
                case "Large":
                    if (oreDrop) { for (int i = 0; i < Random.Range(5, 10); i++) SpawnOre(); }
                    for (int i = 0; i < Random.Range(2, 4); i++) SpawnAsteroid("Medium");
                    for (int i = 0; i < Random.Range(3, 8); i++) SpawnAsteroid("Small");
                    break;

                case "Medium":
                    if (oreDrop) { for (int i = 0; i < Random.Range(3, 7); i++) SpawnOre(); }
                    for (int i = 0; i < Random.Range(2, 5); i++) SpawnAsteroid("Small");
                    break;

                case "Small":
                    if (oreDrop) { for (int i = 0; i < Random.Range(1, 3); i++) SpawnOre(); }
                    break;
            }

            //Play sound effect
            GetComponent<AudioSource>().Play();

            //Destroy self
            destroyed = true;
        }
    }

    private void SpawnAsteroid(string size)
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