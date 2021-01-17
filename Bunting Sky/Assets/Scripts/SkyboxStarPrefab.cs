using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxStarPrefab : MonoBehaviour
{
    private float sphereRadius = 800f;
    private float strength = 0.055f; //1.0f - strength

    void Start()
    {
        //Render queue
        GetComponent<Renderer>().material.renderQueue = -1;

        //Randomize distribution by Euler degree angles
        float randYaw = Random.Range(0.0f, 360.0f);
        float randPitch = Random.Range(0.0f, 360.0f);

        //Fix polar bias in distribution
        if (randPitch >= 180)
        {
            randPitch = Mathf.Pow(randPitch, 1.0f - strength);
        }
        else
        {
            randPitch = Mathf.Pow(randPitch, 2.0f);
        }

        //Distribute
        transform.localRotation = Quaternion.Euler(randPitch, randYaw, 0);
        transform.position += transform.forward * sphereRadius;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*
    public void PlaceInSkysphere()
    {

    }
    */
}