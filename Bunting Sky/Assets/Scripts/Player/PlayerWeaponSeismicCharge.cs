using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponSeismicCharge : MonoBehaviour
{
    [System.NonSerialized] public Control control;
    [System.NonSerialized] public Player player;
    
    //This
    public GameObject playerWeaponProjectileSeismicChargePrefab;
    private readonly List<GameObject> POOL = new List<GameObject>();
    private readonly short POOL_LENGTH = 16;
    private short poolIndex = 0;

    private readonly float PROJECTILE_SPEED = 100f;
    
    [System.NonSerialized] public short clipSize;
    [System.NonSerialized] public readonly short CLIP_SIZE_STARTER = 2;
    [System.NonSerialized] public short clipRemaining;
    [System.NonSerialized] public readonly float CLIP_COOLDOWN_DURATION = 9f; //reload period
    [System.NonSerialized] public float clipCooldownCurrent = 0f;

    [System.NonSerialized] public readonly float SINGLE_COOLDOWN_DURATION = 0.5f;
    [System.NonSerialized] public float singleCooldownCurrent = 0f;

    private void Start()
    {
        //Set up object pooling
        for (int i = 0; i < POOL_LENGTH; i++)
        {
            GameObject instancePlayerWeaponProjectileSeismicCharge = Instantiate(playerWeaponProjectileSeismicChargePrefab, Vector3.zero, Quaternion.identity);
            POOL.Add(instancePlayerWeaponProjectileSeismicCharge);
            instancePlayerWeaponProjectileSeismicCharge.SetActive(false);

            //Put in weapons tree
            instancePlayerWeaponProjectileSeismicCharge.transform.parent = control.generation.playerProjectilesSeismicCharges.transform;

            //Pass control reference
            instancePlayerWeaponProjectileSeismicCharge.GetComponent<PlayerWeaponProjectileSeismicCharge>().control = control;
        }
    }

    private void Update()
    {
        UpdatePlayerWeaponCooldowns();
    }

    private void UpdatePlayerWeaponCooldowns()
    {
        //Reload
        if (control.binds.GetInputDown(control.binds.bindPrimaryReload) && clipRemaining != clipSize)
        {
            clipRemaining = 0;
        }

        //Single
        if (singleCooldownCurrent > 0f)
        {
            singleCooldownCurrent -= Time.deltaTime;
        }

        //Clip
        if (clipCooldownCurrent > 0f)
        {
            clipCooldownCurrent -= Time.deltaTime;
        }

        //Reloading
        if (clipRemaining == 0)
        {
            //Play sound
            player.soundSourceLaserReload.Play();
            //Start cooldown
            clipCooldownCurrent = CLIP_COOLDOWN_DURATION;
            //Reset clip
            clipRemaining = clipSize;
        }
    }

    public void UpdateUpgrades()
    {
        //This upgrade has been changed to only affect the mining laser
        //clipSize = (short)(CLIP_SIZE_STARTER * (1 + player.upgradeLevels[control.commerce.UPGRADE_DUAL_BATTERIES]));
        clipSize = (short)(CLIP_SIZE_STARTER);
        clipRemaining = clipSize;
    }

    public void Fire()
    {
        //Pooling
        POOL[poolIndex].SetActive(true);
        //Ignore collisions between the laser and the player (this does not seem necessary)
        //Physics.IgnoreCollision(weaponLaserPool[WeaponLaserPoolIndex].GetComponent<Collider>(), transform.GetComponent<Collider>());
        //Reset weapon instance
        POOL[poolIndex].GetComponent<PlayerWeaponProjectileSeismicCharge>().ResetPoolState(
            //transform.position + (transform.forward * 2f) - (transform.up * 0.015f),
            transform.position + (transform.forward * 0.14f) - (transform.up * 0.015f),
            transform.rotation * Quaternion.Euler(90, 270, 0),
            player.rb.velocity + (PROJECTILE_SPEED * transform.forward)
        );

        /*
        POOL[poolIndex].transform.position = transform.position + (transform.forward * 0.14f) - (transform.up * 0.015f);
        POOL[poolIndex].transform.rotation = transform.rotation * Quaternion.Euler(90, 270, 0);
        POOL[poolIndex].GetComponent<Rigidbody>().rotation = transform.rotation * Quaternion.Euler(90, 270, 0);
        POOL[poolIndex].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        POOL[poolIndex].GetComponent<Rigidbody>().velocity = player.rb.velocity + (PROJECTILE_SPEED * transform.forward);
        POOL[poolIndex].GetComponent<Rigidbody>().AddTorque(1000 * new Vector3(Random.value, Random.value, Random.value));
        POOL[poolIndex].GetComponent<PlayerWeaponProjectileSeismicCharge>().timeAtWhichThisSelfDestructs = PROJECTILE_LIFETIME_DURATION;
        POOL[poolIndex].GetComponent<PlayerWeaponProjectileSeismicCharge>().timeSpentAlive = 0f;
        POOL[poolIndex].GetComponent<PlayerWeaponProjectileSeismicCharge>().startVelocity = control.generation.instancePlayer.GetComponentInChildren<Rigidbody>().velocity;
        POOL[poolIndex].GetComponent<PlayerWeaponProjectileSeismicCharge>().exploded = false;
        */

        //Iterate through list
        if (poolIndex < POOL_LENGTH - 1)
        {
            poolIndex++;
        }
        else
        {
            poolIndex = 0;
        }

        //Cooldown & ammo
        singleCooldownCurrent = SINGLE_COOLDOWN_DURATION;
        clipRemaining--;

        //Play sound effect
        switch (player.soundSourceLaserArrayIndex)
        {
            case 0:
                player.soundSourceLaser0.Play();
                break;
            case 1:
                player.soundSourceLaser1.Play();
                break;
            case 2:
                player.soundSourceLaser2.Play();
                break;
            case 3:
                player.soundSourceLaser3.Play();
                break;
        }

        //Increment and loop sound source array
        player.soundSourceLaserArrayIndex++;
        if (player.soundSourceLaserArrayIndex > player.soundSourceLaserArrayLength - 1) player.soundSourceLaserArrayIndex = 0;
    }
}
