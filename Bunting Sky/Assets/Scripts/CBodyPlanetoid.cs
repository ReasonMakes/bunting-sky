using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CBodyPlanetoid : MonoBehaviour
{
    [System.NonSerialized] public Control control;
    public GameObject station;
    public Rigidbody rb;

    public GameObject model;

    [System.NonSerialized] public bool disabled = false;
    private float timeSpentDisabled = 0f;

    private bool hasStation = false;
    private GameObject instancedStation;

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
        //Collision with another planetoid
        if (!disabled && collision.collider.gameObject.name == control.cBodyPlanetoid.name + "(Clone)")
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

        //Disable Station
        if (hasStation)
        {
            Destroy(instancedStation, 0f);
        }

        //Gravitate toward centre star only (so that the lack of the hitbox doesn't cause it to accelerate to infinity)
        GetComponent<Gravity>().gravitateTowardCentreStarOnly = true;

        //Spawn regular asteroids
        for(int i = 0; i < 7; i++)
        {
            //Spawn
            GameObject asteroid = control.SpawnAsteroidManually(transform.position, rb.velocity, false);

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
            bool particlesFadedOut = timeSpentDisabled >= GetComponent<ParticlesDamageRock>().partSysShurikenDamage.emission.rateOverTime.constant;

            Transform playerTransform = control.instancePlayer.transform.Find("Body").transform;
            bool playerBeyondArbitraryDistance = Vector3.Distance(transform.position, playerTransform.position) >= playerTransform.GetComponent<Player>().ORBITAL_DRAG_MODE_THRESHOLD;

            if (disabled && particlesFadedOut && playerBeyondArbitraryDistance)
            {
                Destroy(gameObject, 0f);
            }

            timeSpentDisabled += Time.deltaTime;
        }
    }

    public Vector3 SpawnStation(bool forced)
    {
        //Remember that this planetoid has a station oribting it
        hasStation = true;

        //Offset the station from the host cBody
        Vector3 stationCoords = new Vector3(transform.position.x + 10f, transform.position.y + 10f, transform.position.z + 10f);

        //4 in 5 chance of having a space station. Option to force-spawn the station
        if (forced || Random.Range(0f, 4f) >= 1f)
        {
            instancedStation = Instantiate
            (
                station,
                stationCoords,
                Quaternion.Euler(270f, 0f, 270f)
            );

            //Set orbit
            instancedStation.GetComponent<StationOrbit>().planetoidToOrbit = gameObject;
            instancedStation.GetComponent<Rigidbody>().velocity = rb.velocity;
            //instancedStation.GetComponent<Gravity>().SetVelocityToOrbit(instancedStation.GetComponent<StationOrbit>().planetoidToOrbit, 0f);

            //Set parent
            instancedStation.transform.parent = transform.parent;

            //Give control references
            instancedStation.GetComponentInChildren<StationDocking>().control = control;
            instancedStation.GetComponentInChildren<StationOrbit>().control = control;
        }

        //Return coords so that player can spawn near station
        return stationCoords;
    }
}
