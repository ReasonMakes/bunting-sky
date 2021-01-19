using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CBodyAsteroid : MonoBehaviour
{
    private GameObject model;
    private GameObject activeModel;
    public string sizeClassDisplay;
    public Rigidbody rb;

    public GameObject modelClassLarge;
    public GameObject modelClassMedium;
    public GameObject modelClassSmall;

    public Control control;

    public GameObject ore;

    public MeshRenderer meshRenderer;
    public Material matPlatinoid;
    public Material matPreciousMetal;
    public Material matWater;
    public byte type = 0; //0 = Platinoids, 1 = PreciousMetal, 2 = Water

    public ParticleSystem partSysShurikenDamage;
    private ParticleSystem.EmitParams partSysShurikenDamageEmitParameters;
    private int partSysShurikenDamageEmitCount = 0;
    private float partSysShurikenDamageShapeRadius = 0.1f;
    private float partSysShurikenDamageSizeMultiplier = 1f;

    private byte health = 4;
    public bool destroyed = false;
    private float destroyedTime = 0f;

    Transform playerTran;
    public SphereCollider targetCollider1;
    public SphereCollider targetCollider2;
    public SphereCollider targetCollider3;
    public SphereCollider targetCollider4;

    public bool separating = true;
    private float intersectingRepelForce = 0.05f;

    private void Start()
    {
        playerTran = control.instancePlayer.transform.Find("Body");
    }

    private void Update()
    {
        //Destruction
        if (!Menu.menuOpenAndGamePaused)
        {
            bool particlesFadedOut = destroyedTime >= partSysShurikenDamage.emission.rateOverTime.constant;
            bool playerBeyondArbitraryDistance = Vector3.Distance(transform.position, playerTran.transform.position) >= playerTran.GetComponent<Player>().ORBITAL_DRAG_MODE_THRESHOLD;
            if (destroyed && particlesFadedOut && playerBeyondArbitraryDistance)
            {
                Destroy(gameObject, 0f);
            }
            destroyedTime += Time.deltaTime;
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

        CBodyAsteroid[] asteroids = FindObjectsOfType<CBodyAsteroid>();
        foreach (CBodyAsteroid asteroid in asteroids)
        {
            if (asteroid != this)
            {
                if (activeModel.transform.GetComponent<MeshCollider>().bounds.Intersects(
                    asteroid.GetComponent<CBodyAsteroid>().activeModel.transform.GetComponent<MeshCollider>().bounds
                ))
                {
                    //Debug.Log("Intersecting");
                    rb.AddForce(intersectingRepelForce * (transform.position - asteroid.transform.position).normalized * Time.deltaTime);
                    return;
                }
            }
        }

        activeModel.transform.GetComponent<MeshCollider>().enabled = true;
        rb.detectCollisions = true;
        separating = false;
    }

    public string RandomSize()
    {
        //Randomly choose size and set size and type
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
                rb.mass = 0.0001f;
                partSysShurikenDamageEmitCount = 50;
                partSysShurikenDamageShapeRadius = 0.2f;
                partSysShurikenDamageSizeMultiplier = 1f;
                health = (byte)Random.Range(1, 3);
                break;

            case "Medium":
                model = modelClassMedium;
                partSysShurikenDamageEmitCount = 200;
                partSysShurikenDamageShapeRadius = 1.3f;
                partSysShurikenDamageSizeMultiplier = 1.2f;
                rb.mass = 0.001f;
                health = (byte)Random.Range(2, 5);
                break;

            case "Large":
                model = modelClassLarge;
                partSysShurikenDamageEmitCount = 350;
                partSysShurikenDamageShapeRadius = 3.2f;
                partSysShurikenDamageSizeMultiplier = 2f;
                rb.mass = 0.01f;
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

        SetParticleSystemDamageColour();
    }

    private void SetParticleSystemDamageColour()
    {
        //Assign type color to damage particle material
        Color activeModelMaterialColor = activeModel.transform.GetChild(0).GetComponent<MeshRenderer>().material.GetColor("_Tint");
        Color materialColorRGB = new Color(
            activeModelMaterialColor.r,
            activeModelMaterialColor.g,
            activeModelMaterialColor.b,
            1f
        );

        //REDUCE SATURATION
        //Convert to HSV colour space
        Color.RGBToHSV(
            materialColorRGB,
            out float materialColorRGB_H,
            out float materialColorRGB_S,
            out float materialColorRGB_V
        );
        //Modify saturation
        materialColorRGB_S *= 0.8f;
        //Convert back to RGB colour space
        materialColorRGB = Color.HSVToRGB(
            materialColorRGB_H,
            materialColorRGB_S,
            materialColorRGB_V
        );

        partSysShurikenDamageEmitParameters = new ParticleSystem.EmitParams
        {
            startColor = materialColorRGB
        };
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Collision with planetoid
        if (collision.collider.gameObject.name == control.cBodyPlanetoid.name + "(Clone)")
        {
            //Destroy self
            Damage(health, Vector3.zero, transform.position);
        }
    }

    public void Damage(byte damageAmount, Vector3 direction, Vector3 position)
    {
        health -= damageAmount;
        if (health > 0)
        {
            EmitDamageParticles(1, direction, position, false);
        }
        else
        {
            health = 0;
            EmitDamageParticles(7, Vector3.zero, position, true);
            BreakApart();
        }
    }

    private void EmitDamageParticles(int countMultiplier, Vector3 directionIn, Vector3 positionIn, bool destroyingEntireAsteroid)
    {
        /*
         * If destroyingEntireAsteroid flag is true:
         *  - emits particles in a sphere
         *  - with larger particles
         *  - with more particles
         *  - at a starting shape radius equal to the asteroid model radius
         *  - all from the centre of the asteroid, ignoring the specified position
         *  
         * Otherwise
         *  - emits 90% of particles in a cone shape
         *  - with the last 10% in a sphere shape
         *  - all from the specified position
         */

        //Shape radius/position, and size multiplier
        Vector3 directionOut;
        float sizeMultiplier = 1f;

        
        if (destroyingEntireAsteroid)
        {
            //Shape radius = model radius
            ParticleSystem.ShapeModule partSysShurikenDamageShapeModule = partSysShurikenDamage.shape;

            //For some reason this method just doesn't seem to return with the correct radius
            //Vector3 modelSize = activeModel.transform.GetChild(0).GetComponent<MeshFilter>().mesh.bounds.size;
            //smallestRadius = 0.5f * Mathf.Min(Mathf.Min(modelSize.x, modelSize.y), modelSize.z);
            //float averageRadius = (modelSize.x + modelSize.y + modelSize.z) / 6f; //divide by (n terms * 2) to get radius instead of diameter

            partSysShurikenDamageShapeModule.radius = partSysShurikenDamageShapeRadius;

            //Position and size
            //^^particleSystemDamageEmitParameters.position = transform.position;
            partSysShurikenDamageEmitParameters.position = Vector3.zero;
            partSysShurikenDamageEmitParameters.applyShapeToPosition = true;
            sizeMultiplier *= partSysShurikenDamageSizeMultiplier;
        }
        else
        {
            //Position
            partSysShurikenDamageEmitParameters.applyShapeToPosition = false;
            //^^particleSystemDamageEmitParameters.position = positionIn;
            partSysShurikenDamageEmitParameters.position = positionIn - transform.position;
        }

        //Velocity/rotation
        //particleSystemDamageEmitParameters.angularVelocity = 0f;
        //particleSystemDamageEmitParameters.rotation = 0f;
        
        //Per particle:
        float loops = partSysShurikenDamageEmitCount * countMultiplier;
        for (int i = 0; i <= loops; i++)
        {
            
            //Direction
            float directionCurve = Random.Range(0f, 3f);
            if (destroyingEntireAsteroid)
            {
                //Spherical because destroying entire asteroid
                directionOut = Random.insideUnitSphere.normalized;
            }
            else if (i >= loops - (loops * 0.1f))
            {
                //Cone has last 10% spherical
                directionOut = Random.insideUnitSphere.normalized;
            }
            else
            {
                //Cone
                float coneRadius = Random.Range(3f, 10f);
                directionOut = directionIn
                    + (Vector3.forward * Random.value * coneRadius)
                    + (Vector3.up * Random.value * coneRadius)
                    + (Vector3.right * Random.value * coneRadius);
            }

            //Velocity
            //^^particleSystemDamageEmitParameters.velocity = rb.velocity + (directionOut * directionCurve);
            partSysShurikenDamageEmitParameters.velocity = directionOut * directionCurve;

            //Size
            partSysShurikenDamageEmitParameters.startSize = Random.Range(0.03f * sizeMultiplier, 0.15f * sizeMultiplier);
            
            //Emit
            partSysShurikenDamage.Emit(
                partSysShurikenDamageEmitParameters,
                1
            );
        }
    }

    public void BreakApart()
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
                    for (int i = 0; i < Random.Range(5, 10); i++) SpawnOre();
                    for (int i = 0; i < Random.Range(2, 4); i++) SpawnAsteroid("Medium");
                    for (int i = 0; i < Random.Range(3, 8); i++) SpawnAsteroid("Small");
                    break;

                case "Medium":
                    for (int i = 0; i < Random.Range(3, 7); i++) SpawnOre();
                    for (int i = 0; i < Random.Range(2, 5); i++) SpawnAsteroid("Small");
                    break;

                case "Small":
                    for (int i = 0; i < Random.Range(1, 3); i++) SpawnOre();
                    break;
            }
            //in order to spawn more than one asteroid, setup a system which ignores collisions with siblings until no longer intersecting?

            //Destroy self
            destroyed = true;
        }
    }

    private void SpawnAsteroid(string size)
    {
        //Instantiate at parent position, plus some randomness
        GameObject instanceCBodyAsteroid = Instantiate(
            control.cBodyAsteroid,
            transform.position + (1.2f * new Vector3(Random.value, Random.value, Random.value)),
            Quaternion.Euler(
                Random.Range(0f, 360f),
                Random.Range(0f, 360f),
                Random.Range(0f, 360f)
            )
        );
        //Put in CBodies tree
        instanceCBodyAsteroid.transform.parent = control.cBodiesAsteroids.transform;

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
        CBodyAsteroid instanceCBodyAsteroidScript = instanceCBodyAsteroid.GetComponent<CBodyAsteroid>();
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
        instanceOre.transform.parent = control.ore.transform;

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