using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OppositeRotation : MonoBehaviour
{
    void Update()
    {
        //Subtract once to get to Quaternion.identity
        transform.rotation = transform.parent.rotation * Quaternion.Inverse(transform.parent.rotation);

        //Subtract again to go in the opposite direction
        transform.rotation = transform.parent.rotation * Quaternion.Inverse(transform.parent.rotation);
    }
}
