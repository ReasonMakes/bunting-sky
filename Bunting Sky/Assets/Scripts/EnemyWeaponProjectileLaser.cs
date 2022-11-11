using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeaponProjectileLaser : MonoBehaviour
{
    [System.NonSerialized] public Control control;
    private Rigidbody rb;

    [System.NonSerialized] public float timeSpentAlive;
    [System.NonSerialized] public float timeAtWhichThisSelfDestructs;

    [System.NonSerialized] public bool canDamage = true;
    [System.NonSerialized] public MeshCollider parentMeshCollider;

    private void Start()
    {
        //Ignore collisions with the enemy who fired it
        Physics.IgnoreCollision(
            transform.Find("Non-Emissive Model").GetComponent<MeshCollider>(),
            parentMeshCollider
        );

        //General collision detection
        rb = GetComponent<Rigidbody>();
        rb.detectCollisions = true;
    }

    private void Update()
    {
        if (!Menu.menuOpenAndGamePaused)
        {
            //Raycast collisions
            UpdateCollisionDetection();

            //Increment lifetime
            timeSpentAlive += Time.deltaTime;

            //Deactivate self after lifetime expires
            if (timeSpentAlive >= timeAtWhichThisSelfDestructs)
            {
                SetDisabled();
            }
        }
    }

    private void SetDisabled()
    {
        transform.Find("Emissive Model").gameObject.SetActive(false);
        transform.Find("Point Light").gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    private void UpdateCollisionDetection()
    {
        /*
         * Unity's collision detection system is great for some things,
         * But for weapon projectiles it often doesn't do a good enough job at detecting them
         * So we use this custom method instead
         * 
         * Here we use raycasting to check the space in front of the projectile for collidables
         * The distance we check ahead increases with the projectile's speed to prevent phasing
         * 
         * We also have to be careful to ignore the trigger colliders since those are used for the waypoint and target system
         * 
         * In the raycast, we cast out from the transform.right direction since the model is rotated
         */

        float minimumRaycastDistance = 20f; //this value must be high enough that the projectile does not phase through objects directly in front of the player
        float raycastDistance = minimumRaycastDistance * rb.velocity.magnitude * Time.deltaTime;

        //if (Physics.Raycast(transform.position, transform.right, out RaycastHit hit, raycastDistance))

        LayerMask someLayerMask = -1;
        if (Physics.Raycast(transform.position, transform.right, out RaycastHit hit, raycastDistance, someLayerMask, QueryTriggerInteraction.Ignore))
        {
            TryInteract(hit.transform, hit.point);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        //Although interactions are normally handled by the raycast, collisions DO sometimes still occur
        TryInteract(collision.transform, transform.position);
    }

    private void TryInteract(Transform transformWeHit, Vector3 hitPoint)
    {
        if (transformWeHit.name == control.generation.asteroid.name + "(Clone)")
        {
            Asteroid asteroidScript = transformWeHit.GetComponent<Asteroid>();

            //Break apart asteroid
            if (!asteroidScript.isDestroying)
            {
                if (canDamage)
                {
                    //Calculate the direction from the laser to the asteroid hit point
                    Vector3 direction = (transform.position - hitPoint).normalized;

                    //Damage the asteroid
                    asteroidScript.Damage(1, direction, hitPoint, true);
                }
            }
        }
        else if (transformWeHit.name == "Body") //control.generation.playerPrefab.name + "(Clone)")
        {
            if (canDamage)
            {
                //Calculate the direction from the laser to the asteroid hit point
                Vector3 direction = (transform.position - hitPoint).normalized;
                float knockback = 200f;

                //Force
                control.GetPlayerScript().rb.AddForce(knockback * -direction);

                //Damage
                control.GetPlayerScript().DamagePlayer(
                    control.GetPlayerScript().vitalsHealth - 1.0d,
                    "incoming fire",
                    1.0f,
                    direction,
                    true
                );

                //Shake camera
                control.GetPlayerScript().CameraShakeAdd(new Vector2(control.GetPlayerScript().CAMERA_OFFSET_POSITION_MAGNITUDE_MAX, control.GetPlayerScript().CAMERA_OFFSET_ROTATION_MAGNITUDE_MAX));

                //Play sound effect
                control.GetPlayerScript().soundSourceCollision.volume = control.GetPlayerScript().SOUND_IMPACT_VOLUME_SIGNIFICANT * control.settings.volumeAll; //we need to adjust the volume here because we lower it when the impact is insignificant
                control.GetPlayerScript().soundSourceCollision.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
                control.GetPlayerScript().soundSourceCollision.Play();

            }
        }

        //Can no longer deal damage
        //canDamage = false;

        SetDisabled();
    }
}