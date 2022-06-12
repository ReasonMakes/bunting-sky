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

        DestroySelfIfPlanned();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Fatal collisions
        if 
        (
            !disabled
            && (
                collision.collider.gameObject.name == control.generation.moon.name + "(Clone)"
                || collision.collider.gameObject.name == control.generation.planet.name + "(Clone)"
            )
        )
        {
            //Get other planetoid to destroy itself
            //collision.collider.gameObject.GetComponent<CBodyPlanetoid>().DestroySelf();

            //Destroy self
            DisableSelfAndPlanDestroy();
        }
    }

    public void DisableSelfAndPlanDestroy()
    {
        //Emit particles
        GetComponent<ParticlesDamageRock>().EmitDamageParticles(7, Vector3.zero, transform.position, true);

        //Disable self
        GetComponent<SphereCollider>().enabled = false; //Disable waypoint trigger
        rb.detectCollisions = false;
        model.SetActive(false);
        transform.Find("Map Model").gameObject.SetActive(false);

        //Disable Station
        if (hasStation)
        {
            Destroy(instancedStation, 0f);
        }

        //Spawn regular asteroids
        byte type = Asteroid.GetRandomType();
        for (int i = 0; i < 7; i++)
        {
            //Spawn
            GameObject asteroid = control.generation.SpawnAsteroid(
                transform.position,
                rb.velocity,
                Asteroid.GetRandomSize(),
                type,
                Asteroid.HEALTH_MAX
            );

            //Spread out
            asteroid.transform.position += 16f * new Vector3(Random.value, Random.value, Random.value);
        }

        //Play sound
        GetComponent<AudioSource>().Play();

        //Remember is disabled
        disabled = true;
    }

    private void DestroySelfIfPlanned()
    {
        if (!Menu.menuOpenAndGamePaused)
        {
            bool particlesFadedOut = timeSpentDisabled >= GetComponent<ParticlesDamageRock>().particlesDamageRock.emission.rateOverTime.constant;

            Transform playerTransform = control.generation.instancePlayer.transform.Find("Body").transform;
            bool playerBeyondArbitraryDistance = Vector3.Distance(transform.position, playerTransform.position) >= playerTransform.GetComponent<Player>().ORBITAL_DRAG_MODE_THRESHOLD;

            if (disabled && particlesFadedOut && playerBeyondArbitraryDistance)
            {
                Destroy(gameObject, 0f);
            }

            timeSpentDisabled += Time.deltaTime;
        }
    }

    public GameObject SpawnStation(bool forced, string titleOverride, bool generateOffers, float pricePlatinoid, float pricePreciousMetal, float priceWater, int[] upgradeIndex)
    {
        //Remember that this planetoid has a station oribting it, or at least we tried to spawn one (should this really be true before spawning it?)
        hasStation = true;

        //4 in 5 chance of having a space station. Option to force-spawn the station
        if (forced || Random.Range(0, 6) >= 1)
        {
            instancedStation = Instantiate
            (
                station,
                transform.position + new Vector3(10f, 10f, 10f),
                Quaternion.Euler(270f, 0f, 270f)
            );

            StationDocking scriptStationDocking = instancedStation.GetComponentInChildren<StationDocking>();

            //Set orbit
            instancedStation.GetComponent<StationOrbit>().planetoidToOrbit = gameObject;
            instancedStation.GetComponent<Rigidbody>().velocity = rb.velocity;
            //instancedStation.GetComponent<Gravity>().SetVelocityToOrbit(instancedStation.GetComponent<StationOrbit>().planetoidToOrbit, 0f);

            //Set parent
            instancedStation.transform.parent = transform.parent;

            //Give control references
            scriptStationDocking.control = control;
            instancedStation.GetComponentInChildren<StationOrbit>().control = control;

            //Generate name
            if (titleOverride == null)
            {
                instancedStation.GetComponent<NameHuman>().GenerateName();
            }
            else
            {
                instancedStation.GetComponent<NameHuman>().title = titleOverride;
            }

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
        }

        //Return coords so that player can spawn near station
        return gameObject;
    }

    public void SpawnHeighliner(string titleOverride)
    {
        //Remember that this planetoid has a station oribting it
        hasHeighliner = true;

        //Spawn the heighliner
        instancedHeighliner = Instantiate
        (
            heighliner,
            transform.position + new Vector3(20f, 20f, 20f),
            Quaternion.Euler(0f, 0f, 0f)
        );

        //Set parent
        instancedHeighliner.transform.parent = transform.parent;

        //Set name
        instancedHeighliner.GetComponent<NameHuman>().title = titleOverride;
    }
}