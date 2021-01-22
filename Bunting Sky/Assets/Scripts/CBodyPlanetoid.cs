using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CBodyPlanetoid : MonoBehaviour
{
    [System.NonSerialized] public Control control;
    public GameObject station;
    public Rigidbody rb;

    public GameObject model;

    [System.NonSerialized] public bool destroyed = false;
    private float destroyedTime = 0f;

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
        //Destruction
        if (!Menu.menuOpenAndGamePaused)
        {
            bool particlesFadedOut = destroyedTime >= GetComponent<ParticlesDamageRock>().partSysShurikenDamage.emission.rateOverTime.constant;

            Transform playerTransform = control.instancePlayer.transform.Find("Body").transform;
            bool playerBeyondArbitraryDistance = Vector3.Distance(transform.position, playerTransform.position) >= playerTransform.GetComponent<Player>().ORBITAL_DRAG_MODE_THRESHOLD;

            if (destroyed && particlesFadedOut && playerBeyondArbitraryDistance)
            {
                Destroy(gameObject, 0f);
            }

            destroyedTime += Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Collision with another planetoid
        if (!destroyed && collision.collider.gameObject.name == control.cBodyPlanetoid.name + "(Clone)")
        {
            //Get other planetoid to destroy itself
            //collision.collider.gameObject.GetComponent<CBodyPlanetoid>().DestroySelf();

            //Destroy self
            DestroySelf();
        }
    }

    public void DestroySelf()
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
        GetComponent<Gravity>().gravitateTowardCentreStartOnly = true;

        //Remember is disabled
        destroyed = true;

        //Spawn asteroid debris
        /*
        for(int i = 0; i < 4; i++)
        {
            //Spawn
            GameObject asteroid = control.SpawnAsteroidManually(transform.position, rb.velocity, false);

            //Spread out
            asteroid.transform.position += 10f * new Vector3(Random.value, Random.value, Random.value);

            //Destroy
            asteroid.GetComponent<CBodyAsteroid>().Damage(asteroid.GetComponent<CBodyAsteroid>().health, Vector3.zero, transform.position);
        }
        */

        //Spawn regular asteroids
        for(int i = 0; i < 7; i++)
        {
            //Spawn
            GameObject asteroid = control.SpawnAsteroidManually(transform.position, rb.velocity, false);

            //Spread out
            asteroid.transform.position += 16f * new Vector3(Random.value, Random.value, Random.value);
        }

        //Destroy self
        //Destroy(gameObject, 0f);
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
            instancedStation.transform.parent = transform;

            //Give control reference
            instancedStation.GetComponentInChildren<StationDocking>().control = control;
        }

        return stationCoords;
    }
}
