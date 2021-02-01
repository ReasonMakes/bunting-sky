using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    public Rigidbody rb;
    [System.NonSerialized] public Control control;

    //smooth out gravitate addForce by adding a bit of the planned force every fixed update
    //basically on every gravitate call, calculate the amount of force to add, then in fixed update add that force divided by the amount of time in between updates

    //Maybe this should vary depending on distance to nearest cBody
    //That way there will be more fidelity in the physics when it's most relevant
    //But calculating distance may be intensive too

    //We can also improve this by using Physics.OverlapSphere to only test for cbodies that are within relevant range (so we don't waste resources calculating negligible forces)

    public readonly short GRAVITY_SLOW_UPDATE_PERIOD = 90;
    [System.NonSerialized] public int gravityInstanceIndex;
    private float gravityTimePoint = 0f;
    //private float gravityDeltaTime = 0f;
    private Vector3 gravityForceVector = Vector3.zero;

    [System.NonSerialized] public bool gravitateTowardCentreStarOnly = false;

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

            //Add gravitation (semi) constantly at vector calculated intermittently
            rb.AddForce(gravityForceVector * Time.fixedDeltaTime);

            //Debug.Log(deltaTimeSinceLastGravitate);
        }
    }

    private void SlowFixedUpdate()
    {
        GravitateTowardAllCBodies();
    }

    public void GravitateTowardAllCBodies()
    {
        //Keep track of time
        //gravityDeltaTime = Time.time - gravityTimePoint;
        gravityTimePoint = Time.time;

        //Reset force vector
        gravityForceVector = Vector3.zero;

        //Gravitate
        //Debug.Log(gameObject.name);
        if (gravitateTowardCentreStarOnly)
        {
            gravityForceVector += GetInstantaneousGravityFromOneCBody(control.generation.instanceCentreStar.GetComponent<Gravity>());
        }
        else
        {
            Gravity[] cBodyArray = FindObjectsOfType<Gravity>();
            foreach (Gravity cBody in cBodyArray)
            {
                //Don't gravitate toward self
                if (cBody != this)
                {
                    //Don't gravitate toward destroyed asteroids
                    if (cBody.name == "CBodyAsteroid" + "(Clone)") //control.cBodyAsteroid.name
                    {
                        if (cBody.GetComponent<CBodyAsteroid>().destroyed || cBody.GetComponent<CBodyAsteroid>().separating)
                        {
                            return;
                        }
                    }

                    //Don't gravitate toward destroyed planetoids
                    if (cBody.name == "CBodyPlanetoid" + "(Clone)") //control.cBodyPlanetoid.name
                    {
                        if (cBody.GetComponent<CBodyPlanetoid>().disabled)
                        {
                            return;
                        }
                    }

                    gravityForceVector += GetInstantaneousGravityFromOneCBody(cBody);
                }
            }
        }
    }

    private Vector3 GetInstantaneousGravityFromOneCBody(Gravity cBody)
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
        return forceMagnitude * forceDirection;// * gravityDeltaTime;
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
        //Or possibly because we aren't applying a constant force - we are applying it in steps
        //Bandaid fixed using a dirty approximate compensation coefficient
        float dirtyApproxCompCoeff = 0.01f; //0.06f;

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

        gravityInstanceIndex = Generation.gravityInstanceIndex;
        //Increment
        Generation.gravityInstanceIndex += (int)(GRAVITY_SLOW_UPDATE_PERIOD * 0.161803398874989484820458683436f);
        //Wrap
        if (Generation.gravityInstanceIndex >= GRAVITY_SLOW_UPDATE_PERIOD) Generation.gravityInstanceIndex -= GRAVITY_SLOW_UPDATE_PERIOD;
    }
}