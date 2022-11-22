using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [System.NonSerialized] public Control control;
    [System.NonSerialized] public bool hasGeneratedEntities;
    [System.NonSerialized] public GameObject heighliner0;
    [System.NonSerialized] public GameObject heighliner1;
    [System.NonSerialized] public Color tint;
    private Transform collideableTerrain;
    private Transform atmposphere;

    private void Start()
    {
        float roll = Random.value;

        //Planet type
        if (roll <= 0.75f)
        {
            //Planet with atmosphere
            collideableTerrain = transform.Find("Model").Find("Collideable Terrain").Find("Rocky");
            collideableTerrain.gameObject.SetActive(true);

            atmposphere = transform.Find("Model").Find("Atmosphere").Find("Outside");
            atmposphere.gameObject.SetActive(true);
        }
        else if (roll <= 0.25f)
        {
            //Gas giant
            collideableTerrain = transform.Find("Model").Find("Collideable Terrain").Find("Gas Giant Core");
            collideableTerrain.gameObject.SetActive(true);

            atmposphere = transform.Find("Model").Find("Atmosphere").Find("Gas Giant Volume");
            atmposphere.gameObject.SetActive(true);
        }
        else
        {
            //Default: rocky vacuum
            collideableTerrain = transform.Find("Model").Find("Collideable Terrain").Find("Rocky");
            collideableTerrain.gameObject.SetActive(true);
        }

        //Collider
        transform.Find("Sphere Collider").GetComponent<SphereCollider>().radius = control.GetVectorAverageComponents(collideableTerrain.localScale) * 1.75f;
        transform.Find("Mesh Collider").GetComponent<MeshCollider>().sharedMesh = collideableTerrain.GetComponent<MeshFilter>().mesh;
        transform.Find("Mesh Collider").transform.localScale = collideableTerrain.localScale;
        
        //Colour tint
        collideableTerrain.GetComponent<MeshRenderer>().material.SetColor("_Tint", tint);
        if (atmposphere != null)
        {
            atmposphere.GetComponent<MeshRenderer>().material.SetColor("_Tint", tint);
        }

        //Spin
        Rigidbody rb = GetComponent<Rigidbody>();
        //rb.AddTorque(6e3f * rb.mass * ((Random.value * 2f) - 1f) * Vector3.up);
        rb.angularVelocity = 0.1f * ((Random.value * 2f) - 1f) * Vector3.up;
    }
}