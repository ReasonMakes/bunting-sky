using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    public Rigidbody rb;
    
    //smooth out gravitate addForce by adding a bit of the planned force every fixed update
    //basically on every gravitate call, calculate the amount of force to add, then in fixed update add that force divided by the amount of time in between updates

    //Maybe this should vary depending on distance to nearest cBody
    //That way there will be more fidelity in the physics when it's most relevant
    //But calculating distance may be intensive too
    public readonly short GRAVITY_SLOW_UPDATE_PERIOD = 90;
    [System.NonSerialized] public int gravityInstanceIndex;
    private float timeAtLastGravitate = 0f;
    private float deltaTimeSinceLastGravitate = 0f;

    private void Start()
    {
        //Generate an index for distributing the expensive gravity calculations over time
        GenerateGravitateIndex();
    }

    private void FixedUpdate()
    {
        if (!Menu.menuOpenAndGamePaused)
        {
            //Slow update to reduce the lag from calculating all of these for loops
            //We use a generated index so that not all instances of this class run the same expensive gravity calculations at the same time
            if ((Time.frameCount + gravityInstanceIndex) % GRAVITY_SLOW_UPDATE_PERIOD == 0)
            {
                SlowFixedUpdate();
            }
        }
    }

    private void SlowFixedUpdate()
    {
        GravitateTowardAllCBodies();
    }

    public void GravitateTowardAllCBodies()
    {
        //Keep track of time
        deltaTimeSinceLastGravitate = Time.time - timeAtLastGravitate;
        timeAtLastGravitate = Time.time;

        //Gravitate
        Gravity[] cBodyArray = FindObjectsOfType<Gravity>();
        foreach (Gravity cBody in cBodyArray)
        {
            //Don't gravitate toward self
            if (cBody != this)
            {
                //Don't gravitate toward destroyed asteroids
                if(cBody.name == "CBodyAsteroid(Clone)")
                {
                    if (cBody.GetComponent<CBodyAsteroid>().destroyed || cBody.GetComponent<CBodyAsteroid>().separating)
                    {
                        return;
                    }
                }

                Gravitate(cBody);
            }
        }
    }

    private void Gravitate(Gravity cBody)
    {
        //Gravitate toward a celestial body
        /*
         *  F = G * (m1 * m2 / r^2)
         * 
         *  F: newtons
         *  G: metres^3 * kilograms^−1 * seconds^−2
         *  m: kilograms
         *  r: metres
         */

        Vector3 forceVector = cBody.rb.position - rb.position;
        Vector3 forceDirection = forceVector.normalized;
        float forceDistanceSquared = forceVector.sqrMagnitude;

        //F = G * (m1 * m2 / r^2)
        float forceMagnitude = Control.GRAVITATIONAL_CONSTANT * ((rb.mass * cBody.rb.mass) / forceDistanceSquared);
        
        //Manually factor-in deltaTime since last method call since this is in SlowUpdate()
        rb.AddForce(forceMagnitude * forceDirection * deltaTimeSinceLastGravitate);
    }

    public void SetVelocityToOrbit(GameObject bodyToOrbit, float angleToStar)
    {
        //Set velocity to circularize orbit
        /*
        *  v = √(G * M / r)
        * 
        *  v: metres per second
        *  G: metres^3 * kilograms^−1 * seconds^−2
        *  M: kilograms
        *  r: metres
        */

        //This method runs into issues
        //Possibly because distance and speed units in unity don't have the same ratios as irl SI units do
        //Bandaid fixed using a dirty approximate compensation coefficient
        float dirtyApproxCompCoeff = 0.06f;

        Vector3 orbitalVector = bodyToOrbit.transform.position - transform.position; 
        float orbitalRadius = orbitalVector.magnitude;                               //r = |oV|
        float combinedMasses = bodyToOrbit.GetComponent<Rigidbody>().mass + rb.mass; //M = m1 + m2

        //Dreaded square root. Optimize later?
        float orbitalSpeed = Mathf.Sqrt(Control.GRAVITATIONAL_CONSTANT * combinedMasses / orbitalRadius); //v = √(G * M / r)

        Vector3 oribtalDirection = Quaternion.AngleAxis(90, Vector3.up) * orbitalVector.normalized;
        rb.velocity = dirtyApproxCompCoeff * orbitalSpeed * oribtalDirection;

        //Rotate the vector to the normal of the angle to the star to orbit, then multiply the magnitude by the orbital speed
        /*
        rb.velocity = Quaternion.AngleAxis(90, Vector3.up) * new Vector3(
            Mathf.Cos(angleToStar) * orbitalSpeed,
            0f,
            Mathf.Sin(angleToStar) * orbitalSpeed
        );
        */
    }

    private void GenerateGravitateIndex()
    {
        /* 
         *  Generates an index for distributing the expensive gravity calculations over time
         *  We use the golden ratio so that indices are unlikely to overlap 
         */

        //Debug.Log(gameObject.name + ": " + Control.gravityInstanceIndex);

        gravityInstanceIndex = Control.gravityInstanceIndex;
        //Increment
        Control.gravityInstanceIndex += (int)(GRAVITY_SLOW_UPDATE_PERIOD * 0.161803398874989484820458683436f);
        //Wrap
        if (Control.gravityInstanceIndex >= GRAVITY_SLOW_UPDATE_PERIOD) Control.gravityInstanceIndex -= GRAVITY_SLOW_UPDATE_PERIOD;
    }
}