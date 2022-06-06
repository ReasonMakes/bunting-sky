using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeighlinerEntry : MonoBehaviour
{
    [System.NonSerialized] public Control control;
    
    private void Awake()
    {

    }

    private void Start()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        //Collision is with player
        if (other.gameObject.name == "Body")
        {
            Debug.Log("Player entered heighliner");
        }
    }

    /*
    private void OnTriggerEXit(Collider other)
    {
        if(other.gameObject.name == "Body")
        {

        }
    }
    */

    private void Update()
    {
        //Move player
        //control.generation.instancePlayer.transform.Find("Body").transform.position = transform.position;
    }
}