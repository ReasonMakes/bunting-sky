using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponLaser : MonoBehaviour
{
    [System.NonSerialized] public Control control;
    [System.NonSerialized] public Player player;
    
    //This
    public GameObject playerWeaponProjectileLaserPrefab;
    private readonly List<GameObject> POOL = new List<GameObject>();
    private short poolIndex = 0;

    [System.NonSerialized] public readonly float PROJECTILE_SPEED = 50f; //80f; //120f;
    private float projectileLifetimeDuration = 4.5f; //THIS GETS CHANGED DYNAMICALLY IN Start()

    [System.NonSerialized] public short clipSize;
    [System.NonSerialized] public readonly short CLIP_SIZE_STARTER = 26; //30; //72; //16;
    [System.NonSerialized] public short clipRemaining = 16;
    [System.NonSerialized] public readonly float CLIP_COOLDOWN_DURATION = 1.2f; //1.5f; //reload period
    [System.NonSerialized] public float clipCooldownCurrent = 0f;

    [System.NonSerialized] public readonly float SINGLE_COOLDOWN_DURATION = 0.09f; //0.07f; //0.1f; //0.01f; //0.185f; //0.2f; //firerate
    [System.NonSerialized] public float singleCooldownCurrent = 0f;

    private void Start()
    {
        //Populate pool up to two clips at max upgrade
        for (int i = 0; i < (CLIP_SIZE_STARTER * 2 * int.Parse(control.commerce.upgradeDictionary[control.commerce.UPGRADE_DUAL_BATTERIES, control.commerce.UPGRADE_MAX_LEVEL])); i++)
        {
            GameObject instancePlayerWeaponProjectileLaser = Instantiate(playerWeaponProjectileLaserPrefab, Vector3.zero, Quaternion.identity);
            POOL.Add(instancePlayerWeaponProjectileLaser);
            instancePlayerWeaponProjectileLaser.SetActive(false);

            //Put in weapons tree
            instancePlayerWeaponProjectileLaser.transform.parent = control.generation.playerProjectilesLasers.transform;

            //Pass control reference
            instancePlayerWeaponProjectileLaser.GetComponent<PlayerWeaponProjectileLaser>().control = control;
        }

        //Calculate the lifetime duration as amount of time needed to burn through two full clips and reload (minus one shot for safety)
        projectileLifetimeDuration = (SINGLE_COOLDOWN_DURATION * (POOL.Count - 1)) + CLIP_COOLDOWN_DURATION;
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
            singleCooldownCurrent -= Time.deltaTime * Mathf.Max(1f, 2f * control.GetPlayerScript().upgradeLevels[control.commerce.UPGRADE_FIRERATE]);
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
        clipSize = (short)(CLIP_SIZE_STARTER * (1 + player.upgradeLevels[control.commerce.UPGRADE_DUAL_BATTERIES]));
        clipRemaining = clipSize;
    }

    public void Fire()
    {
        //RESET POOL INSTANCE
        POOL[poolIndex].SetActive(true);

        //Ignore collisions between the laser and the player (this does not seem necessary)
        //Physics.IgnoreCollision(weaponLaserPool[WeaponLaserPoolIndex].GetComponent<Collider>(), transform.GetComponent<Collider>());

        //Position and rotation
        //POOL[poolIndex].transform.position = transform.position + (transform.forward * 0.14f) - (transform.up * 0.015f);
        POOL[poolIndex].transform.position = transform.position + (transform.forward * 0.3f) - (transform.up * 0.05f);
        POOL[poolIndex].GetComponent<Rigidbody>().rotation = transform.rotation * Quaternion.Euler(90, 270, 0);
        POOL[poolIndex].transform.rotation = transform.rotation * Quaternion.Euler(90, 270, 0);
        POOL[poolIndex].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        //copying the player's velocity turns out to be very hard to intuit, but I've readded it with the introduction of the target lead indicator
        POOL[poolIndex].GetComponent<Rigidbody>().velocity = player.rb.velocity + (PROJECTILE_SPEED * transform.forward);
        //POOL[poolIndex].GetComponent<Rigidbody>().velocity = player.velocityOfObjectDraggingRelativeTo + PROJECTILE_SPEED * transform.forward;

        POOL[poolIndex].GetComponent<PlayerWeaponProjectileLaser>().timeAtWhichThisSelfDestructs = projectileLifetimeDuration;
        POOL[poolIndex].GetComponent<PlayerWeaponProjectileLaser>().timeSpentAlive = 0f;
        POOL[poolIndex].GetComponent<PlayerWeaponProjectileLaser>().canDamage = true;

        //Go to next index for next pool instance
        if (poolIndex < (CLIP_SIZE_STARTER * 2) - 1)
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

        int thresholdPitchIncrease = 5; //these last shots have a modified pitch
        float pitchModulationMagnitude = 0.0075f;
        if (clipRemaining <= thresholdPitchIncrease)
        {
            float newPitch = 1f - Mathf.Min(pitchModulationMagnitude * 2f, pitchModulationMagnitude * (thresholdPitchIncrease - clipRemaining));
            player.soundSourceLaser0.pitch = newPitch;
            player.soundSourceLaser1.pitch = newPitch;
            player.soundSourceLaser2.pitch = newPitch;
            player.soundSourceLaser3.pitch = newPitch;
        }
        else
        {
            player.soundSourceLaser0.pitch = 1f;
            player.soundSourceLaser1.pitch = 1f;
            player.soundSourceLaser2.pitch = 1f;
            player.soundSourceLaser3.pitch = 1f;
        }

        //Play sound effect
        switch (player.soundSourceLaserArrayIndex) //we use multiple sounds to avoid the sound engine overloading and clipping
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
