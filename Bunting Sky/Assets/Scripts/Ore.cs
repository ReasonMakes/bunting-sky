using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ore : MonoBehaviour
{
    public Rigidbody rb;
    public Material matGlowPlatinoid;
    public Material matGlowPreciousMetal;
    public Material matGlowWater;
    public byte type; //0 = ClaySilicate, 1 = Platinoids, 2 = PreciousMetal, 3 = Water

    public Control control; //this is set by its instantiator
    private float deathTime;
    private readonly float DEATH_DELAY = 20f;
    private readonly float DEATH_DELAY_ANIMATION_PORTION = 0.2f;

    public Vector3 parentVelocity = Vector3.zero;
    //private Vector3 playerVAtInit = Vector3.zero;
    private Transform playerBodyTransform;
    private readonly float DRAG = 3f;
    private readonly float ATTRACT_STRENGTH = 150000f;
    private readonly float ABSORB_DIST = 0.15f;

    void Start()
    {
        //Setup deathTime
        deathTime = Time.time + DEATH_DELAY + Random.Range(0f, 2f);

        //Get player data
        playerBodyTransform = control.generation.instancePlayer.transform.Find("Body").transform;
        //playerVAtInit = playerTransform.GetComponent<Rigidbody>().velocity;

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
    }

    void Update()
    {
        if (!Menu.menuOpenAndGamePaused)
        {
            //Get player data
            float distanceBetweenOreAndPlayer = Vector3.Distance(transform.position, playerBodyTransform.position);

            //Forces (gravitate toward player, repel away from other ore, drag relative)
            AttractToPlayer(distanceBetweenOreAndPlayer);
            RepelFromOtherOre();
            DragOreRelative();

            //Scale down as the ore gets closer to the player (to look like it's being absorbed) or as ore gets closer to deathTime
            Scale(distanceBetweenOreAndPlayer);

            //When close enough to player, absorb into and increment ore counter
            if (distanceBetweenOreAndPlayer <= ABSORB_DIST) AbsorbIntoPlayer();

            //If can't find way to player, absorb automagically
            if (Time.time >= deathTime) AbsorbIntoPlayer();
        }
    }

    private void Scale(float distanceToPlayer)
    {
        //Calculate scale for each type
        //Scale relative to limited player distance
        float scaleByPlayerDist = Mathf.Min(0.05f, 0.0025f + (0.025f * Mathf.Max(0f, distanceToPlayer - ABSORB_DIST)));
        float scaleByDeathTime = Mathf.Min(1f, ((deathTime - Time.time) / DEATH_DELAY) * DEATH_DELAY_ANIMATION_PORTION);
        
        //Choose the scale type
        float scale = Mathf.Min(scaleByPlayerDist, scaleByDeathTime);

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
                float repelStrength = ATTRACT_STRENGTH * 0.008f;
                
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
        Vector3 directionToPlayer = (playerBodyTransform.position - transform.position).normalized;
        float LimitedInverseDistanceBetweenPlayer = Mathf.Min(0.1f, 1f / distanceBetweenOreAndPlayer);
        rb.AddForce(directionToPlayer * ATTRACT_STRENGTH * LimitedInverseDistanceBetweenPlayer * Time.deltaTime);
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

        //Drag relative to parent asteroid velocity
        rb.velocity = Control.GetVelocityDraggedRelative(rb.velocity, parentVelocity, DRAG);

        //Drag relative to the system
        //rb.velocity *= (1f - (DRAG * Time.deltaTime));

        //Drag relative to player velocity at ore's spawn time
        //rb.velocity = control.DragRelative(rb.velocity, playerVAtInit, DRAG);
    }

    private void AbsorbIntoPlayer()
    {
        //Only add to player inventory if player isn't in the middle of nowhere
        if (Vector3.Distance(transform.position, playerBodyTransform.position) < 600f)
        {
            //Add ore type to player inventory
            playerBodyTransform.GetComponent<Player>().ore[type]++;

            //Update resources display
            control.ui.UpdateAllPlayerResourcesUI();
        }

        //Destroy if it hasn't been already
        if (gameObject != null) Destroy(gameObject, 0f);
    }
}