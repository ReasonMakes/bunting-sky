using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationDocking : MonoBehaviour
{
    [System.NonSerialized] public Control control;

    private bool host = false;

    //private BoxCollider dryDockCollider;

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Enter: " + other.gameObject.name);

        if (!Commerce.menuOpen && other.gameObject.name == "Body")
        {
            //Is host
            host = true;

            //Open commerce menu
            control.commerce.MenuToggle();
        }
    }

    private void OnTriggerEXit(Collider other)
    {
        //Debug.Log("Exit: " + other.gameObject.name);

        if(other.gameObject.name == "Body")
        {
            //No longer host
            host = false;

            //Unlock commerce menu
            Commerce.menuLocked = false;
        }
    }

    private void Update()
    {
        if (Commerce.menuOpen && host)
        {
            control.instancePlayer.transform.Find("Body").transform.position = transform.position;
        }
        else
        {
            host = false;
        }
    }
}