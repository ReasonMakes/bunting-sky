using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationOrbit : MonoBehaviour
{
    [System.NonSerialized] public Control control;
    [System.NonSerialized] public GameObject planetoidToOrbit;
    public Rigidbody rb;

    private void FixedUpdate()
    {
        ////Get orbit position goal
        //Vector3 orbitPositionGoal = new Vector3(
        //    planetoidToOrbit.transform.position.x + 10f,
        //    planetoidToOrbit.transform.position.y + 10f,
        //    planetoidToOrbit.transform.position.z + 10f
        //);
        //
        ////Attract
        //float attractStrength = 15000f; //150000f;
        //float distanceBetweenStationAndOrbitGoal = Vector3.Distance(transform.position, orbitPositionGoal);
        //Vector3 direction = (orbitPositionGoal - transform.position).normalized;
        //float LimitedInverseDistanceBetweenStationAndOrbitGoal = Mathf.Min(0.1f, 1f / distanceBetweenStationAndOrbitGoal);
        //rb.AddForce(direction * attractStrength * LimitedInverseDistanceBetweenStationAndOrbitGoal * Time.deltaTime);
        //
        ////Drag
        //float drag = 50f;
        //rb.velocity = Control.GetVelocityDraggedRelative(rb.velocity, planetoidToOrbit.GetComponent<Rigidbody>().velocity, drag);
    }
}