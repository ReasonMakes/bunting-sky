using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeaponLaser : MonoBehaviour
{
    [System.NonSerialized] public Control control;

    public Rigidbody parentEnemyRb;

    //This
    public GameObject enemyWeaponProjectileLaserPrefab;
    [System.NonSerialized] public readonly List<GameObject> POOL = new List<GameObject>();
    [System.NonSerialized] public readonly short POOL_LENGTH = 256; //96; //16;
    private short poolIndex = 0;

    [System.NonSerialized] public static readonly float PROJECTILE_SPEED = 50f; //80f; //120f;
    private float projectileLifetimeDuration = 4.5f; //THIS GETS CHANGED DYNAMICALLY

    [System.NonSerialized] public static readonly double DAMAGE = 1.0d;

    private void Start()
    {
        projectileLifetimeDuration = 19.2f;

        //Set up object pooling
        for (int i = 0; i < POOL_LENGTH; i++)
        {
            GameObject instanceEnemyWeaponProjectileLaser = Instantiate(enemyWeaponProjectileLaserPrefab, Vector3.zero, Quaternion.identity);
            POOL.Add(instanceEnemyWeaponProjectileLaser);
            instanceEnemyWeaponProjectileLaser.SetActive(false);

            //Put in weapons tree
            instanceEnemyWeaponProjectileLaser.transform.parent = control.generation.enemyProjectilesLasers.transform;

            //Pass control reference
            instanceEnemyWeaponProjectileLaser.GetComponent<EnemyWeaponProjectileLaser>().control = control;
        }
    }

    public void Fire(Vector3 gimbalDirection)
    {
        //Pooling
        POOL[poolIndex].SetActive(true);
        //Pass owning enemy reference
        POOL[poolIndex].GetComponent<EnemyWeaponProjectileLaser>().parentMeshCollider = GetComponent<Enemy>().model.GetComponent<MeshCollider>();
        //Reset weapon instance
        POOL[poolIndex].transform.position = transform.position; //transform.position + (transform.forward * 0.14f) - (transform.up * 0.015f);
        POOL[poolIndex].GetComponent<Rigidbody>().rotation = transform.rotation * Quaternion.Euler(90, 270, 0);
        POOL[poolIndex].transform.rotation = transform.rotation * Quaternion.Euler(90, 270, 0);
        POOL[poolIndex].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        //Carry on velocity parent had
        POOL[poolIndex].GetComponent<Rigidbody>().velocity = parentEnemyRb.velocity + (PROJECTILE_SPEED * gimbalDirection);
        //POOL[poolIndex].GetComponent<Rigidbody>().velocity = PROJECTILE_SPEED * transform.forward;

        POOL[poolIndex].GetComponent<EnemyWeaponProjectileLaser>().timeAtWhichThisSelfDestructs = projectileLifetimeDuration;
        POOL[poolIndex].GetComponent<EnemyWeaponProjectileLaser>().timeSpentAlive = 0f;
        POOL[poolIndex].GetComponent<EnemyWeaponProjectileLaser>().canDamage = true;

        //Iterate through list
        if (poolIndex < POOL_LENGTH - 1)
        {
            poolIndex++;
        }
        else
        {
            poolIndex = 0;
        }

        //Play sound effect
        switch (control.GetPlayerScript().soundSourceLaserArrayIndex) //we use multiple sounds to avoid the sound engine overloading and clipping
        {
            case 0:
                control.GetPlayerScript().soundSourceLaser0.Play();
                break;
            case 1:
                control.GetPlayerScript().soundSourceLaser1.Play();
                break;
            case 2:
                control.GetPlayerScript().soundSourceLaser2.Play();
                break;
            case 3:
                control.GetPlayerScript().soundSourceLaser3.Play();
                break;
        }

        //Increment and loop sound source array
        control.GetPlayerScript().soundSourceLaserArrayIndex++;
        if (control.GetPlayerScript().soundSourceLaserArrayIndex > control.GetPlayerScript().soundSourceLaserArrayLength - 1)
        {
            control.GetPlayerScript().soundSourceLaserArrayIndex = 0;
        }
    }
}
