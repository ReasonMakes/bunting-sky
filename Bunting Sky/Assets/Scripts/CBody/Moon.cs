using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moon : MonoBehaviour
{
    //Main references
    [System.NonSerialized] public Control control;
    public Rigidbody rb;
    public GameObject model;
    public int planetIndex = 0; //which planet is this moon orbiting

    //Properties
    [System.NonSerialized] public bool disabled = false;
    private float timeSpentDisabled = 0f;
    [System.NonSerialized] public bool isDiscovered = false;

    //Children
    public GameObject station;
    [System.NonSerialized] public bool hasStation = false;
    [System.NonSerialized] public GameObject instancedStation;

    public GameObject heighliner;
    [System.NonSerialized] public bool hasHeighliner = false;
    [System.NonSerialized] public GameObject instancedHeighliner;

    private void Start()
    {
        //Setup particle system
        GetComponent<ParticlesDamageRock>().SetParticleSystemDamageColour(model.transform, 0.7f);
        GetComponent<ParticlesDamageRock>().partSysShurikenDamageEmitCount = 2500;
        GetComponent<ParticlesDamageRock>().partSysShurikenDamageShapeRadius = 13f;
        GetComponent<ParticlesDamageRock>().partSysShurikenDamageSizeMultiplier = 5f;
    }

    private void Update()
    {
        //Debug.DrawLine(transform.position, transform.position + (Vector3.up * 100f), Color.green);

        //Moons are no longer destructible, so this code is just bloat
        //DestroySelfIfPlanned();
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    //Fatal collisions
    //    if 
    //    (
    //        !disabled
    //        && (
    //            collision.collider.gameObject.name == control.generation.moon.name + "(Clone)"
    //            || collision.collider.gameObject.name == control.generation.planet.name + "(Clone)"
    //        )
    //    )
    //    {
    //        //Get other planetoid to destroy itself
    //        //collision.collider.gameObject.GetComponent<CBodyPlanetoid>().DestroySelf();
    //
    //        //Destroy self
    //        DisableSelfAndPlanDestroy();
    //    }
    //}

    //public void DisableSelfAndPlanDestroy()
    //{
    //    //Emit particles
    //    GetComponent<ParticlesDamageRock>().EmitDamageParticles(7, Vector3.zero, transform.position, true);
    //
    //    //Disable self
    //    GetComponent<SphereCollider>().enabled = false; //Disable waypoint trigger
    //    rb.detectCollisions = false;
    //    model.SetActive(false);
    //    transform.Find("Map Model").gameObject.SetActive(false);
    //
    //    //Disable Station
    //    if (hasStation)
    //    {
    //        Destroy(instancedStation, 0f);
    //    }
    //
    //    //Spawn regular asteroids
    //    byte type = Asteroid.GetRandomType();
    //    for (int i = 0; i < 7; i++)
    //    {
    //        //Spawn asteroids
    //        GameObject instanceAsteroid = control.generation.AsteroidPoolSpawn(transform.position, Asteroid.GetRandomSize(), type);
    //
    //        //Spread out
    //        instanceAsteroid.transform.position += 16f * new Vector3(Random.value, Random.value, Random.value);
    //    }
    //
    //    //Play explosion sound if player is close enough
    //    if (Vector3.Distance(control.generation.instancePlayer.GetComponentInChildren<Player>().transform.position, transform.position) <= 750f)
    //    {
    //        GetComponent<AudioSource>().Play();
    //    }
    //    
    //    //Remember is disabled
    //    disabled = true;
    //}

    //private void DestroySelfIfPlanned()
    //{
    //    if (!Menu.menuOpenAndGamePaused)
    //    {
    //        bool particlesFadedOut = timeSpentDisabled >= GetComponent<ParticlesDamageRock>().particlesDamageRock.emission.rateOverTime.constant;
    //
    //        bool playerBeyondArbitraryDistance = Vector3.Distance(transform.position, control.GetPlayerTransform().position) >= control.GetPlayerScript().ORBITAL_DRAG_MODE_THRESHOLD;
    //
    //        if (disabled && particlesFadedOut && playerBeyondArbitraryDistance)
    //        {
    //            Debug.Log("Moon destroying");
    //            Destroy(gameObject, 0f);
    //        }
    //
    //        timeSpentDisabled += Time.deltaTime;
    //    }
    //}

    public GameObject SpawnStation(string titleOverride, bool generateOffers, float pricePlatinoid, float pricePreciousMetal, float priceWater, int[] upgradeIndex)
    {
        //Remember that this planetoid has a station oribting it, or at least we tried to spawn one (should this really be true before spawning it?)
        hasStation = true;

        //Instantiate
        instancedStation = Instantiate
        (
            station,
            transform.position + new Vector3(10f, 10f, 10f),
            Quaternion.Euler(270f, 0f, 270f)
        );

        //Set parent
        instancedStation.transform.parent = transform.parent;

        //Generate name
        if (titleOverride == null)
        {
            instancedStation.GetComponent<NameHuman>().GenerateName();
        }
        else
        {
            instancedStation.GetComponent<NameHuman>().title = titleOverride;
        }

        //Give control references
        StationDocking scriptStationDocking = instancedStation.GetComponentInChildren<StationDocking>();
        scriptStationDocking.control = control;
        instancedStation.GetComponentInChildren<StationOrbit>().control = control;

        //Offers
        if (generateOffers)
        {
            scriptStationDocking.GenerateCommerceOffers();
        }
        else
        {
            //Ore purchase offers
            scriptStationDocking.pricePlatinoid = pricePlatinoid;
            scriptStationDocking.pricePreciousMetal = pricePreciousMetal;
            scriptStationDocking.priceWater = priceWater;

            //Upgrades
            scriptStationDocking.upgradeIndexAtButton = upgradeIndex;
        }

        //Return coords so that player can spawn near station
        return gameObject;
    }

    //public void SpawnHeighliner(string titleOverride)
    //{
    //    //Remember that this moon has a satellite oribting it
    //    hasHeighliner = true;
    //
    //    //Spawn the heighliner
    //    instancedHeighliner = Instantiate
    //    (
    //        heighliner,
    //        transform.position + new Vector3(20f, 20f, 20f),
    //        Quaternion.Euler(0f, 0f, 0f)
    //    );
    //
    //    //Set parent
    //    instancedHeighliner.transform.parent = transform.parent;
    //
    //    //Set name
    //    instancedHeighliner.GetComponent<NameHuman>().title = titleOverride;
    //
    //    //Pass control reference
    //    instancedHeighliner.GetComponentInChildren<HeighlinerEntry>().control = control;
    //
    //    //Add to control list
    //    control.generation.heighlinerList.Add(instancedHeighliner);
    //
    //    //Set exit node
    //    if (control.generation.heighlinerCount == 0)
    //    {
    //        control.generation.heighlinerInitial = instancedHeighliner;
    //    }
    //    else if (control.generation.heighlinerCount >= 1 && control.generation.heighlinerCount <= (control.generation.nPlanetsPlanned * 2) - 2) //cant do initial (index[0]) as it links with final; can't do index[1] as it has no previous to link to
    //    {
    //        if (control.generation.heighlinerOpenLinker == null)
    //        {
    //            //If no open linker, make this the open linker and wait until another heighliner exists to connect to this one
    //            control.generation.heighlinerOpenLinker = instancedHeighliner;
    //        }
    //        else
    //        {
    //            //Set this heighliner's exit node, and connect its sister's exit node to it
    //            instancedHeighliner.GetComponentInChildren<HeighlinerEntry>().exitNode = control.generation.heighlinerOpenLinker;
    //            control.generation.heighlinerOpenLinker.GetComponentInChildren<HeighlinerEntry>().exitNode = instancedHeighliner;
    //
    //            //Reset generation to having no open linker at present
    //            control.generation.heighlinerOpenLinker = null;
    //        }
    //    }
    //    else if (control.generation.heighlinerCount == (control.generation.nPlanetsPlanned * 2) - 1) { //last heighliner is -1 because we haven't incremented the count yet
    //        //Set this heighliner's exit node, and connect its sister's exit node to it
    //        instancedHeighliner.GetComponentInChildren<HeighlinerEntry>().exitNode = control.generation.heighlinerInitial;
    //        control.generation.heighlinerInitial.GetComponentInChildren<HeighlinerEntry>().exitNode = instancedHeighliner;
    //    }
    //
    //    //Increment heighliner count
    //    control.generation.heighlinerCount++;
    //}
}