using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class Ore : MonoBehaviour
{
    public Control control;
    
    //Material
    public Material matGlowPlatinoid;
    public Material matGlowPreciousMetal;
    public Material matGlowWater;
    public byte type; //0 = ClaySilicate, 1 = Platinoids, 2 = PreciousMetal, 3 = Water

    //Auto-death & pooling
    //private float deathTime; //when to automatically absorb into the player and set as inactive, so that we don't have ore floating around forever
    private readonly float DEATH_DELAY = 20f;
    private readonly float DEATH_DELAY_ANIMATION_PORTION = 0.2f;
    public bool active = false;

    //Forces
    public Rigidbody rb;
    public Vector3 parentVelocity = Vector3.zero;
    private readonly float DRAG = 3f;
    private readonly float ATTRACT_STRENGTH = 20e4f; //150000f;
    private readonly float ABSORB_DIST = 0.15f;
    private float nextSlowUpdateCall = 0f;
    private readonly float SLOW_UPDATE_PERIOD = 2f; //how often to call a slow update, in seconds
    private float attractionTime;
    private readonly float ATTRACTION_DELAY_PERIOD = 1f;

    public void Enable(byte type, Vector3 parentVelocity)
    {
        active = true;

        //Hierarchy
        transform.parent = control.generation.oreEnabled.transform;
        control.generation.oreEnabled.name = "Enabled (" + control.generation.oreEnabled.transform.childCount + ")";
        control.generation.oreDisabled.name = "Disabled (" + control.generation.oreDisabled.transform.childCount + ")";

        //Pass values
        this.parentVelocity = parentVelocity;
        this.type = type;
        //Assign material equal to type
        switch (type) //0 = ClaySilicate, 1 = Platinoids, 2 = PreciousMetal, 3 = Water
        {
            case 1:
                GetComponent<MeshRenderer>().material = matGlowPlatinoid;
                break;

            case 2:
                GetComponent<MeshRenderer>().material = matGlowPreciousMetal;
                break;

            case 3:
                GetComponent<MeshRenderer>().material = matGlowWater;
                break;
        }

        //Setup timers
        //deathTime = Time.time + DEATH_DELAY + Random.Range(0f, 2f);
        nextSlowUpdateCall = Time.time + 0.25f + Random.Range(0f, SLOW_UPDATE_PERIOD);
        attractionTime = Time.time + ATTRACTION_DELAY_PERIOD;
    }

    public void Disable()
    {
        active = false;

        //Hierarchy
        transform.parent = control.generation.oreDisabled.transform;
        control.generation.oreEnabled.name = "Enabled (" + control.generation.oreEnabled.transform.childCount + ")";
        control.generation.oreDisabled.name = "Disabled (" + control.generation.oreDisabled.transform.childCount + ")";

        //Scale (to turn invisible)
        transform.localScale = new Vector3(
            0f,
            0f,
            0f
        );
    }

    void Update()
    {
        if (!Menu.menuOpenAndGamePaused && active)
        {
            //Get player data
            float distanceBetweenOreAndPlayer = Vector3.Distance(transform.position, control.GetPlayerTransform().position);

            //Forces (gravitate toward player, repel away from other ore, drag relative)
            DragOreRelative();
            if (control.GetPlayerScript().GetTotalOre() <= control.GetPlayerScript().oreMax - 1.0d) //if there is room for one more
            {
                if (Time.time >= attractionTime)
                {
                    AttractToPlayer(distanceBetweenOreAndPlayer);
                }
                if (Time.time >= nextSlowUpdateCall)
                {
                    RepelFromOtherOre();
                    nextSlowUpdateCall = Time.time + SLOW_UPDATE_PERIOD;
                }
            }
            
            //Scale down as the ore gets closer to the player (to look like it's being absorbed) or as ore gets closer to deathTime
            Scale(distanceBetweenOreAndPlayer);

            //When close enough to player, absorb into and increment ore counter
            if (
                distanceBetweenOreAndPlayer <= ABSORB_DIST
                && control.GetPlayerScript().GetTotalOre() <= control.GetPlayerScript().oreMax - 1.0d //if there is room for one more
            )
            {
                AbsorbIntoPlayer();
            }

            //If can't find way to player, absorb automagically
            //if (Time.time >= deathTime) AbsorbIntoPlayer();
        }
    }

    private void Scale(float distanceToPlayer)
    {
        float minimumScale = 0.025f;

        //Calculate scale for each type
        //Scale relative to limited player distance
        float scaleByPlayerDist = Mathf.Min(0.05f, 0.0025f + (0.025f * Mathf.Max(0f, distanceToPlayer - ABSORB_DIST)));
        //float scaleByDeathTime = Mathf.Min(1f, ((deathTime - Time.time) / DEATH_DELAY) * DEATH_DELAY_ANIMATION_PORTION);
        
        //Choose the scale type
        //float scale = Mathf.Max(minimumScale, Mathf.Min(scaleByPlayerDist, scaleByDeathTime));
        float scale = Mathf.Max(minimumScale, scaleByPlayerDist);

        //Scale
        transform.localScale = new Vector3(
            scale,
            scale,
            scale
        );
    }

    private void RepelFromOtherOre()
    {
        Ore[] oreArray = FindObjectsOfType<Ore>();
        foreach (Ore ore in oreArray)
        {
            if (ore != this)
            {
                float repelStrength = ATTRACT_STRENGTH * 0.25f; //0.008f;
                
                float inverseDistanceMax = 0.1f;
                float inverseDistanceMin = 0.001f;
                float distanceBetweenOre = Vector3.Distance(transform.position, ore.transform.position);
                float limitedInverseDistanceBetweenOre = Mathf.Max(inverseDistanceMin, Mathf.Min(inverseDistanceMax, 1f / distanceBetweenOre)) - inverseDistanceMin;

                Vector3 directionFromOre = (transform.position - ore.transform.position).normalized;

                rb.AddForce(directionFromOre * repelStrength * limitedInverseDistanceBetweenOre * Time.deltaTime);
            }
        }
    }

    private void AttractToPlayer(float distanceBetweenOreAndPlayer)
    {
        if (!control.GetPlayerScript().isDestroyed)
        {
            Vector3 directionToPlayer = (control.GetPlayerTransform().position - transform.position).normalized;
            float LimitedInverseDistanceBetweenPlayer = Mathf.Min(0.1f, 1f / distanceBetweenOreAndPlayer);
            rb.AddForce(directionToPlayer * ATTRACT_STRENGTH * LimitedInverseDistanceBetweenPlayer * Time.deltaTime);
        }
    }

    private void DragOreRelative()
    {
        /* 
         * Drag prevents the ore from endlessly orbiting the player
         * In-universe the excuse for this behaviour is it's part of the player's ship's tractor beam's functionality
         * 
         * Many issues here:
         * Draging relative to...
         * Player        if the player tries to move forward to collect the ore, the ore "runs away" because it is now dragging relative to the player's new velocity
         * Parent        if small asteroid, the ore flies off forward before the player has time to collect because the player is no longer dragging relative to the asteroid
         * System        if large asteroid, the ore flies off behind the asteroid before the player has time to collect it because the player is dragging relative to the asteroid
         * Player init   same issues as dragging relative to parent
         * 
         * Probably the best solution (and current implementation) is to drag relative to parent and then have an asteroid remnant object which the player follows the orbit of
         * The object can self-destruct once the player leaves the sphere of influence
         */
        
        //Drag relative to player
        //rb.velocity = control.DragRelative(rb.velocity, playerTransform.GetComponent<Rigidbody>().velocity, DRAG);

        //Drag relative to parent asteroid velocity (original system)
        //rb.velocity = Control.GetVelocityDraggedRelative(rb.velocity, parentVelocity, DRAG);

        //Drag relative to the system
        rb.velocity *= (1f - (DRAG * Time.deltaTime));

        //Drag relative to player velocity at ore's spawn time
        //rb.velocity = control.DragRelative(rb.velocity, playerVAtInit, DRAG);
    }

    private void AbsorbIntoPlayer()
    {
        //Tutorial has collected ore
        control.GetPlayerScript().tutorialHasCollectedOre = true;

        //Increment ore cargo
        control.GetPlayerScript().ore[type] += 1.0d;

        //Show tip if cargo full
        if (control.GetPlayerScript().GetTotalOre() > control.GetPlayerScript().oreMax - 1.0d) //no room for any more ore
        {
            control.ui.SetTip("Cargo bay full");
        }

        //Update resources display
        control.ui.UpdateAllPlayerResourcesUI();

        //Destroy if it hasn't been already
        Disable();
    }
}