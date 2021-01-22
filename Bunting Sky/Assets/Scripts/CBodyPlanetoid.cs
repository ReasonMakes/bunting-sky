using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CBodyPlanetoid : MonoBehaviour
{
    [System.NonSerialized] public Control control;
    public GameObject station;
    public Rigidbody rb;

    private bool destroying = false;

    private void OnCollisionEnter(Collision collision)
    {
        //Collision with another planetoid
        if (!destroying && collision.collider.gameObject.name == control.cBodyPlanetoid.name + "(Clone)")
        {
            destroying = true;

            //Spawn asteroid debris
            for(int i = 0; i < 4; i++)
            {
                //Spawn
                GameObject asteroid = control.SpawnAsteroidManually(transform.position, rb.velocity, false);

                //Spread out
                asteroid.transform.position += 10f * new Vector3(Random.value, Random.value, Random.value);

                //Destroy
                asteroid.GetComponent<CBodyAsteroid>().Damage(asteroid.GetComponent<CBodyAsteroid>().health, Vector3.zero, transform.position);
            }
            
            /*
            //Spawn regular asteroids
            for(int i = 0; i < 4; i++)
            {
                //Spawn
                GameObject asteroid = control.SpawnAsteroidManually(transform.position, rb.velocity);

                //Spread out
                asteroid.transform.position += 10f * new Vector3(Random.value, Random.value, Random.value);
            }
            */

            //Destroy self
            Destroy(gameObject, 0f);
        }
    }

    public Vector3 SpawnStation(bool forced)
    {
        //Offset the station from the host cBody
        Vector3 stationCoords = new Vector3(transform.position.x + 10f, transform.position.y + 10f, transform.position.z + 10f);

        //4 in 5 chance of having a space station. Option to force-spawn the station
        if (forced || Random.Range(0f, 4f) >= 1f)
        {
            GameObject instancedStation = Instantiate
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
