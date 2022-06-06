using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    #region Init fields
    [System.NonSerialized] public Control control;
    private KeyBinds binds;

    #region Init fields: Camera
    //Camera
    private bool fovSet = false;
    private readonly float MOUSE_SENS_COEFF = 1f;
    public GameObject positionMount;
    private float centreMountPitch = 0f;
    private float centreMountYaw = 0f;
    private float centreMountRoll = 0f;
    public GameObject fpCamMount;
    public GameObject centreMount;
    [System.NonSerialized] public Transform centreMountTran;
    public GameObject tpCamMount;
    public GameObject fpCam;
    public GameObject fpCamInterior;
    private readonly float FP_CAM_INTERIOR_CLIPPING_PLANE_NEAR = 0.001f; //0.002f; //0.0005f;
    private readonly float FP_CAM_INTERIOR_CLIPPING_PLANE_FAR = 10f; //1e21f;
    public GameObject tpCam;
    public GameObject tpModel;
    public GameObject fpModel;
    [System.NonSerialized] public static bool firstPerson = false;
    [System.NonSerialized] public static bool thirdPerson = false;
    public GameObject mapCam;
    public GameObject mapLight;
    #endregion

    //Spotlight
    public GameObject spotlight;

    #region Init fields: Movement
    //Movement
    public Rigidbody rb;
    private Vector3 thrustVector;
    private readonly float THRUST = 4E3f; //3E3f; //8416.65825f;
    private float thrustEngineWarmupMultiplier = 1f;
    private float thrustEngineWarmupMultiplierMax;
    private readonly float THRUST_ENGINE_WARMUP_MULTIPLIER_MAX_STARTER = 9.0f; //5.0f; //16f;
    private readonly float THRUST_ENGINE_WARMUP_SPEED = 0.5f; //3f;
    private readonly float THRUST_ENGINE_COOLDOWN_SPEED = 12f;
    private readonly float THRUST_FORWARD_MULTIPLIER = 1.1f; //extra thrust for moving forward rather than strafing
    private float thrustMultiplier = 1f;
    private float engineBrightness = 0f;
    public Material engineGlowMat;
    private Color engineEmissionColor = new Color(191, 102, 43);
    private float engineEmissionIntensity = 1.3f * 0.00748f; //1.4f * 0.00748f; //1.631096f;
    public Light engineLight;
    private bool canAndIsMoving = false;

    //Movement: Relative drag
    [System.NonSerialized] public GameObject targetObject;
    [System.NonSerialized] public GameObject cBodies;
    [System.NonSerialized] public readonly float ORBITAL_DRAG_MODE_THRESHOLD = 50f;
    private float distToClosestPlanetoid = 100f; //this should be greater than the orbitalDragModeThreshold so that the player starts with drag relative to system
    private Transform closestPlanetoidTransform;
    private float distToClosestAsteroid = 100f;
    private Transform closestAsteroidTransform;
    private readonly float DRAG = 3f; //Drag amount for all drag modes
    #endregion

    #region Init fields: Audio
    //Audio: Music
    public AudioSource music;
    public AudioClip songDrifting;
    public AudioClip songLifeSupportFailure;
    public AudioClip songHoghmanTransfer; //unused
    public AudioClip songWeWereHere; //unused
    private float musicPlayTime = 30f; //max time until first song plays
    private readonly float MUSIC_PLAY_QUEUE_TIME = 60f;
    private readonly float MUSIC_PLAY_QUEUE_VARIANCE_TIME = 60f;
    private bool firstSong = true;

    //Audio: Sound Effects
    public AudioSource soundSourceRocket;
    public AudioClip soundClipRocket;
    private readonly float SOUND_ROCKET_MAX_VOLUME = 0.02f;
    private readonly float SOUND_ROCKET_VOLUME_DELTA_RATE = 0.1f;
    private readonly float SOUND_ROCKET_MAX_PITCH = 1.5f;
    private readonly float SOUND_ROCKET_PITCH_DELTA_RATE = 0.2f;

    public AudioSource soundSourceLaser0;
    public AudioSource soundSourceLaser1;
    public AudioSource soundSourceLaser2;
    public AudioSource soundSourceLaser3;
    [System.NonSerialized] public byte soundSourceLaserArrayIndex = 0;
    [System.NonSerialized] public byte soundSourceLaserArrayLength = 4;
    public AudioClip soundClipLaser;

    public AudioSource soundSourceLaserReload;
    public AudioClip soundClipLaserReload;

    public AudioSource soundSourceOreCollected;
    public AudioClip soundClipOreCollected;

    public AudioSource soundSourceCoins;
    public AudioClip soundClipCoins;

    public AudioSource soundSourceCollision;
    public AudioClip soundClipCollision;
    #endregion

    #region Init fields: Vitals
    //Vitals
    [System.NonSerialized] public double vitalsHealth = 10.0; //hull integrity (10), fuel (30L), (deprecated) oxygen (840g)
    [System.NonSerialized] public double vitalsHealthMax = 10.0;
    private readonly double VITALS_HEALTH_MAX_STARTER = 10.0;
    [System.NonSerialized] public static bool destroyed = false;
    [System.NonSerialized] public double vitalsFuel = 10.0;
    [System.NonSerialized] public double vitalsFuelMax = 15.0;
    private readonly double VITALS_FUEL_MAX_STARTER = 15.0;
    [System.NonSerialized] public double vitalsFuelConsumptionRate = 0.025;
    [System.NonSerialized] public GameObject vitalsHealthUI;
    [System.NonSerialized] public TextMeshProUGUI vitalsHealthUIText;
    [System.NonSerialized] public GameObject vitalsFuelUI;
    [System.NonSerialized] public TextMeshProUGUI vitalsFuelUIText;
    [System.NonSerialized] public TextMeshProUGUI warningUIText;
    private float warningUIFlashTime = 0f;
    private float warningUIFlashPosition = 0f;
    private readonly float WARNING_UI_FLASH_RATE = 10f;
    private float warningUIFlashTotalDuration = 5f; //This must be odd-numbered or it will not end smoothly (end while transparent)
    #endregion

    #region Init fields: Cargo
    //Cargo
    [System.NonSerialized] public double currency = 0.0; //100.0; //ICC stands for interstellar crypto currency
    [System.NonSerialized] public double[] ore;
    [System.NonSerialized] public readonly int ORE_PLATINOID = 0;
    [System.NonSerialized] public readonly int ORE_PRECIOUS_METAL = 1;
    [System.NonSerialized] public readonly int ORE_WATER = 2;

    /* 
     * Water ice doesn't sell for much BUT can be used in situ for fuel & oxygen if upgrade acquired
     * 
     * Precious metals are very valuable.
     * Includes: gold, nickel, silver, cobalt, lithium, etc.
     * 
     * Platinoids are the most basic cargo, very general-purpose for construction/repairs.
     * Includes ruthenium, rhodium, palladium, osmium, iridium, platinum, etc.
     * Valuable due to their high mechanical strength, good ductility, and stable electrical properties. They also have many useful catalytic properties and are resistant to chemical attack.
     */

    [System.NonSerialized] public int[] upgradeLevels;
    private bool upgradesInitialized = false;
    private readonly double REFINERY_ORE_WATER_IN_RATE = 0.1d;
    private readonly double REFINERY_FUEL_OUT_RATE = 0.075d;
    private readonly float REFINERY_TIME_BETWEEN_REFINES = 10.0f;
    private float refineryTimeAtLastRefine = 0f;
    #endregion

    #region Init fields: Weapons
    //Tree
    [System.NonSerialized] public GameObject playerWeaponsTree;
    
    public PlayerWeaponLaser playerWeaponLaser;
    public PlayerWeaponSeismicCharge playerWeaponSeismicCharge;
    [System.NonSerialized] public GameObject playerWeaponsTreeLaser;
    [System.NonSerialized] public GameObject playerWeaponsTreeSeismicCharge;

    [System.NonSerialized] public string weaponSelectedTitle = "Laser";

    [System.NonSerialized] public short weaponSelectedClipSize;
    [System.NonSerialized] public short weaponSelectedClipRemaining;
    [System.NonSerialized] public float weaponSelectedClipCooldownDuration;
    [System.NonSerialized] public float weaponSelectedClipCooldownCurrent;

    [System.NonSerialized] public float weaponSelectedSingleCooldownDuration;
    [System.NonSerialized] public float weaponSelectedSingleCooldownCurrent;
    #endregion

    //Skybox stars
    public ParticleSystem skyboxStarsParticleSystem;
    private readonly int SKYBOX_STARS_COUNT = 400;
    #endregion

    #region Start

    private void Start()
    {
        //WEAPONS TREES
        //Main
        playerWeaponsTree = new GameObject("Weapons");
        playerWeaponsTree.transform.parent = control.generation.verseSpace.transform;

        //Laser
        playerWeaponsTreeLaser = new GameObject("Laser");
        playerWeaponsTreeLaser.transform.parent = playerWeaponsTree.transform;

        //Seismic charge
        playerWeaponsTreeSeismicCharge = new GameObject("Seismic Charge");
        playerWeaponsTreeSeismicCharge.transform.parent = playerWeaponsTree.transform;

        //MODEL
        DecideWhichModelsToRender();

        //SKYBOX
        skyboxStarsParticleSystem.Emit(SKYBOX_STARS_COUNT);
    }

    public void LateStart()
    {
        /*
        //Print player ship model size
        Vector3 meshFilterSize = transform.GetChild(0).GetChild(0).GetComponent<MeshFilter>().mesh.bounds.size;
        Debug.Log(
            "x: " + meshFilterSize.x
            + ", y: " + meshFilterSize.y
            + ", z: " + meshFilterSize.z
        );
        */

        //KeyBinds reference
        binds = control.binds;

        //Upgrades
        upgradeLevels = new int[control.commerce.upgradeDictionary.GetLength(0)];
        upgradesInitialized = true;
        UpdateUpgrades();

        //Camera
        centreMountTran = centreMount.transform;
        centreMountPitch = centreMountTran.localRotation.x;
        centreMountYaw = centreMountTran.localRotation.y;
        centreMountRoll = centreMountTran.localRotation.z;
        SetCameraSettings();
        
        //Vitals
        //We have to work with odd-numbered multiples of the inverse of the flash rate to end smoothly (end while it is transparent)
        warningUIFlashTotalDuration *= (1f / WARNING_UI_FLASH_RATE);

        //ORE
        ore = new double[3]; //0 = Platinoids, 1 = PreciousMetal, 2 = Water

        //Update resources UI
        control.ui.UpdateAllPlayerResourcesUI();

        //Collection sound
        soundSourceOreCollected.clip = soundClipOreCollected;

        //WEAPONS
        //Called in update anyway
        //UpdateWeaponSelected();

        //AUDIO
        //Play the first song 0 to 30 seconds after startup
        musicPlayTime = Time.time + UnityEngine.Random.Range(0f, musicPlayTime);

        //Init sounds
        soundSourceRocket.clip = soundClipRocket;
        soundSourceLaser0.clip = soundClipLaser;
        soundSourceLaser1.clip = soundClipLaser;
        soundSourceLaser2.clip = soundClipLaser;
        soundSourceLaser3.clip = soundClipLaser;
        soundSourceLaserReload.clip = soundClipLaserReload;
        soundSourceCoins.clip = soundClipCoins;
        soundSourceCollision.clip = soundClipCollision;

        //Start rocket sound
        soundSourceRocket.Play();

        //Setup particle system
        GetComponent<ParticlesDamageRock>().SetParticleSystemDamageColour(tpModel.transform.Find("Ship").transform, 0.7f);
        GetComponent<ParticlesDamageRock>().partSysShurikenDamageEmitCount = 150;
        GetComponent<ParticlesDamageRock>().partSysShurikenDamageShapeRadius = 0.15f;
        GetComponent<ParticlesDamageRock>().partSysShurikenDamageSizeMultiplier = 0.2f;
    }
    #endregion

    #region Update/fixed update & their slow versions
    private void Update()
    {
        //DEBUG
        //---------------------------------------------------

        //control.ui.SetTip(
        //    "SpotlightOn: " + control.settings.spotlightOn.ToString()
        //    + " - menuSettingsToggleSpotlight.isOn: " + control.menu.menuSettingsToggleSpotlight.isOn.ToString()
        //);

        //I to cheat

        ////Repair ship to full
        //if (binds.GetInputDown(binds.bindCheat1))
        //{
        //    vitalsHealth = vitalsHealthMax;
        //    control.ui.UpdatePlayerVitalsDisplay();
        //
        //    control.ui.SetTip("Hull repaired to full integrity");
        //}
        //
        ////Unlock Reinforced Hull
        //if (binds.GetInputDown(binds.bindCheat1))
        //{
        //    upgradeLevels[control.commerce.UPGRADE_REINFORCED_HULL] = 1;
        //    UpdateUpgrades();
        //    control.ui.SetTip("Reinforced Hull upgrade unlocked");
        //}
        //
        //Unlock Seismic Charges (Z and X to select weapon)
        if (binds.GetInputDown(binds.bindCheat1))
        {
            upgradeLevels[control.commerce.UPGRADE_SEISMIC_CHARGES] = 1;
            UpdateUpgrades();
            control.ui.SetTip("Seismic charges upgrade unlocked");
        }
        
        ////Free money
        //if (binds.GetInputDown(binds.bindCheat1))
        //{
        //    currency += 1000;
        //    control.ui.UpdateAllPlayerResourcesUI();
        //    control.ui.SetTip("+1000 currency");
        //}

        //Unlock In Situ Refinery
        //if (binds.GetInputDown(binds.bindCheat1))
        //{
        //    upgradeLevels[control.commerce.UPGRADE_IN_SITU_FUEL_REFINERY] = 1;
        //    UpdateUpgrades();
        //    ore[ORE_WATER] += 10.0d;
        //    control.ui.SetTip("In situ fuel refinery upgrade unlocked");
        //}

        //Add ore water
        //if (binds.GetInputDown(binds.bindCheat1))
        //{
        //    ore[ORE_WATER] += 10.0;
        //    control.ui.UpdateAllPlayerResourcesUI();
        //}

        //Teleport forward
        /*
        if (binds.GetInputDown(binds.bindThrustVectorIncrease))
        {
            transform.position += transform.forward * 1e4f;
            Debug.Log("Teleported forward: distance to star " + (control.generation.instanceCentreStar.transform.position - transform.position).magnitude);
        }
        */

        //Spawn
        //Press O to spawn asteroid
        if (binds.GetInputDown(binds.bindCheat2))
        {
            control.generation.SpawnAsteroidManually(
                transform.position + transform.forward * 3f,
                rb.velocity,
                CBodyAsteroid.GetRandomSize(),
                CBodyAsteroid.GetRandomType(),
                CBodyAsteroid.HEALTH_MAX
            );
            control.ui.SetTip("Spawned one asteroid.");
            upgradeLevels[control.commerce.UPGRADE_SEISMIC_CHARGES] = 1;
            control.ui.SetTip("Seismic charges unlocked.");
        }
        
        /*
        if (binds.GetInputDown(binds.bindThrustVectorDecrease))
        {
            control.SpawnPlanetoidManually(transform.position + transform.forward * 20f, rb.velocity, null);
            Debug.Log("Spawned one planetoid");
        }
        */


        //Slow motion
        /*
        if (binds.GetInput(binds.bindPrimaryFire))
        {
            Time.timeScale = 0.01f;
        }
        else if (!Menu.menuOpenAndGamePaused)
        {
            Time.timeScale = 1f;
        }
        */

        //---------------------------------------------------

        //Slow update
        if (Time.frameCount % 3 == 0)
        {
            SlowUpdate();
        }

        //Have the position mount follow the player position
        positionMount.transform.position = transform.position;

        //Setup the camera
        if (!fovSet)
        {
            SetCameraSettings();
        }

        //AUDIO
        UpdateAudio();

        //Don't run if paused
        if (!Menu.menuOpenAndGamePaused)
        {
            UpdateGetIfMoving();            //Check if moving at all so that it only has to be checked once per update
            UpdatePlayerWeapons();          //Shoot stuff

            UpdatePlayerEngineEffect();     //Set engine glow relative to movement

            //Fuel decrement
            if (canAndIsMoving)
            {
                vitalsFuel = Math.Max(0.0, vitalsFuel - ((vitalsFuelConsumptionRate / (1 + upgradeLevels[control.commerce.UPGRADE_FUEL_EFFICIENCY])) * Time.deltaTime));
            }

            //Fuel increment (in-situ refinery)
            bool missingEnoughFuel = vitalsFuel < vitalsFuelMax - REFINERY_FUEL_OUT_RATE;
            bool hasUpgrade = upgradeLevels[control.commerce.UPGRADE_IN_SITU_FUEL_REFINERY] >= 1;
            bool hasEnoughOre = ore[ORE_WATER] > REFINERY_ORE_WATER_IN_RATE;
            bool enoughTimeHasPassed = Time.time > refineryTimeAtLastRefine + REFINERY_TIME_BETWEEN_REFINES;

            if (missingEnoughFuel && hasUpgrade && hasEnoughOre && enoughTimeHasPassed && control.settings.refine)
            {
                //control.ui.SetTip("Fuel produced by in situ refinery");
                ore[ORE_WATER] -= REFINERY_ORE_WATER_IN_RATE;
                vitalsFuel += REFINERY_FUEL_OUT_RATE;
                control.ui.UpdatePlayerOreWaterText();
                refineryTimeAtLastRefine = Time.time;
            }

            //Warn on loop if out of fuel
            if (vitalsFuel <= 0d && warningUIFlashTime <= 0f)
            {
                //UI warning
                FlashWarning("Fuel reserves empty");

                //Loop smoothly and indefinitely
                warningUIFlashTime = warningUIFlashTotalDuration * 100f;
            }

            //Without this it would be possible for the out of fuel warning to flash indefinitely if you ran out of fuel right as you entered a station
            if (vitalsFuel > 0d && warningUIText.text == "Fuel reserves empty")
            {
                warningUIFlashTime = 0f;
                warningUIFlashPosition = 1f;
            }
        }
    }

    private void FixedUpdate()
    {
        //Update every n frames instead of every frame
        if (Time.frameCount % 3 == 0)
        {
            SlowFixedUpdate();
        }

        //Don't run if paused
        if (!Menu.menuOpenAndGamePaused)
        {
            UpdatePlayerMovementTorque();   //Automatically torque the ship so that it is always facing "up" relative to the system
            UpdatePlayerMovementThrust();   //Move in the direction of player input
        }

        UpdatePlayerMovementDrag();         //Drag the ship relative to either the system or the nearest planetoid, depending on distance to nearest planetoid

        //Add thrust on/off to engine effect brightness (should be run AFTER UpdatePlayerMovementThrust)
        engineBrightness = Math.Max(engineBrightness, Math.Min(1f, engineBrightness + (thrustVector.normalized.magnitude / 40f)));
    }

    private void LateUpdate()
    {
        UpdateCameraMovement();         //Make camera follow player at specified distance and height, plus speed feedback
    }

    private void SlowUpdate()
    {
        control.ui.UpdatePlayerVitalsDisplay();
        UpdateWarningText();
    }

    private void SlowFixedUpdate()
    {
        //If one exists, find the nearest planetoid or asteroid to determine whether or not to drag relative to it
        if (cBodies.transform.Find("Planetoids").childCount > 0)
        {
            closestPlanetoidTransform = GetClosestPlanetoidTransform();
            distToClosestPlanetoid = (transform.position - closestPlanetoidTransform.transform.position).magnitude;
        }

        if (cBodies.transform.Find("Asteroids").childCount > 0)
        {
            closestAsteroidTransform = GetClosestAsteroidTransform();
            distToClosestAsteroid = (transform.position - closestAsteroidTransform.transform.position).magnitude;
        }
    }
    #endregion

    #region Methods called in update
    #region Methods called in update: Movement
    private void UpdateGetIfMoving()
    {
        //Ignore movement input if a menu is opened
        if
        (
            !destroyed
            && !Menu.menuOpenAndGamePaused
            && !Commerce.menuOpen
            && (
                binds.GetInput(binds.bindThrustForward)
                || binds.GetInput(binds.bindThrustLeft)
                || binds.GetInput(binds.bindThrustBackward)
                || binds.GetInput(binds.bindThrustRight)
                || binds.GetInput(binds.bindThrustUp)
                || binds.GetInput(binds.bindThrustDown)
            )
        )
        {
            canAndIsMoving = true;
        }
        else
        {
            canAndIsMoving = false;
        }
    }


    private void UpdatePlayerEngineEffect()
    {
        //Engine effect
        //Set engine emission colour
        engineGlowMat.SetColor("_EmissionColor", engineEmissionColor * engineEmissionIntensity);
        //If the colour intensity reaches zero the material can never get bright again for some reason, so we ensure the lowest value is never equal to zero
        engineGlowMat.SetColor("_EmissionColor", engineEmissionColor * engineEmissionIntensity * (1f + (engineBrightness)));
        engineLight.intensity = (1 + engineBrightness) * 0.5f * 3.15f;
        //Subtract from engine effect brightness
        engineBrightness = Math.Max(0, engineBrightness - (1.5f * Time.deltaTime));
    }

    private void UpdatePlayerMovementDrag()
    {
        /*
         * Drag-relative-to-object if possible, otherwise drag-relative-to-universe
         * 
         * Which object we drag relative to is based on this hierarchy:
         * - Planetoids
         * - Asteroids
         * - Target
         * - System/Centre Star
         * 
         * Can set the relative drag to only happen when not moving to allow for more realistic (but less intuitive) acceleration by surrounding this with an if (!moving) check
         */

        if (closestPlanetoidTransform != null && distToClosestPlanetoid <= ORBITAL_DRAG_MODE_THRESHOLD)
        {
            //Planetoid-relative drag (we check if the transform is null because planetoids are destructible)
            rb.velocity = Control.GetVelocityDraggedRelative(rb.velocity, closestPlanetoidTransform.GetComponent<Rigidbody>().velocity, DRAG);
        }
        else if (closestAsteroidTransform != null && distToClosestAsteroid <= ORBITAL_DRAG_MODE_THRESHOLD)
        {
            //Asteroid-relative drag (we check if the transform is null because asteroids are destructible)
            rb.velocity = Control.GetVelocityDraggedRelative(rb.velocity, closestAsteroidTransform.GetComponent<Rigidbody>().velocity, DRAG);
        }
        else if (targetObject != null)
        {
            //Target-relative drag
            rb.velocity = Control.GetVelocityDraggedRelative(rb.velocity, targetObject.GetComponent<Rigidbody>().velocity, DRAG);
        }
        else
        {
            //System/centre star-relative drag
            rb.velocity *= (1f - (DRAG * Time.deltaTime));
        }
    }

    private void UpdatePlayerMovementTorque()
    {
        //Manual roll for combat mode
        /*
        if(movementMode == 2)
        {
            if (Input.GetKey("q")) rb.AddTorque(transform.forward * playerRollTorque);
            if (Input.GetKey("e")) rb.AddTorque(transform.forward * -playerRollTorque);
        }
        */

        //Auto torque in the direction of camera
        if (vitalsFuel > 0.0 && !binds.GetInput(binds.bindCameraFreeLook) && (canAndIsMoving || binds.GetInput(binds.bindAlignShipToReticle)))
        {
            //Smoothly orient toward camera look direction

            float torqueStrengthFactor = 3f;

            //Pitch
            TorqueAxisRelative(torqueStrengthFactor * 500f, centreMountTran.forward, transform.forward);

            //Yaw
            TorqueAxisRelative(torqueStrengthFactor * 0.3f, centreMountTran.right, transform.right);

            //Roll
            TorqueAxisRelative(torqueStrengthFactor * 100f, centreMountTran.up, transform.up);


            //ORIGINAL SOLUTION
            /*
            //Pitch
            Vector3 pitchCameraToShipCross = Vector3.Cross(-fpCamMountTran.forward, transform.forward);
            float pitchDifference = Mathf.Abs(Vector3.Cross(rb.angularVelocity, transform.forward).magnitude);
            if (Mathf.Abs(pitchCameraToShipCross.magnitude) > errorThreshold * pitchDifference)
            {
                rb.AddTorque(pitchCameraToShipCross * torquePitch * turnRate * Time.deltaTime);
            }

            //Yaw
            Vector3 yawCameraToShipCross = Vector3.Cross(-fpCamMountTran.right, transform.right);
            float yawDifference = Mathf.Abs(Vector3.Cross(rb.angularVelocity, transform.right).magnitude);
            if (Mathf.Abs(yawCameraToShipCross.magnitude) > errorThreshold * yawDifference)
            {
                rb.AddTorque(yawCameraToShipCross * torqueYaw * turnRate * Time.deltaTime);
            }

            //Roll
            Vector3 rollCameraToShipCross = Vector3.Cross(-fpCamMountTran.up, transform.up);
            float rollDifference = Mathf.Abs(Vector3.Cross(rb.angularVelocity, transform.up).magnitude);
            if (Mathf.Abs(rollCameraToShipCross.magnitude) > errorThreshold * rollDifference)
            {
                rb.AddTorque(rollCameraToShipCross * torqueRoll * turnRate * Time.deltaTime);
            }
            */

            /*
            //Orient to always be "above" system plane
            if (transform.position.y >= 0)
            {
                if ((Mathf.Abs(Vector3.Cross(-fpCamMountTran.up, transform.up).magnitude)) > (0.25f * Mathf.Abs(Vector3.Cross(rb.angularVelocity, transform.up).magnitude)))
                {
                    rb.AddTorque(Vector3.Cross(-Vector3.up, transform.up) * playerRollTorque * Time.deltaTime);
                }
            }
            else
            {
                if ((Mathf.Abs(Vector3.Cross(-fpCamMountTran.up, -transform.up).magnitude)) > (0.25f * Mathf.Abs(Vector3.Cross(rb.angularVelocity, -transform.up).magnitude)))
                {
                    rb.AddTorque(Vector3.Cross(-Vector3.up, -transform.up) * playerRollTorque * Time.deltaTime);
                }
            }
            */

            //ALTERNATIVE SOLUTIONS
            /*
            float interpolateSpeed = 5f;
            Quaternion rotCurrent = rb.rotation;
            Quaternion rotTarget = Quaternion.LookRotation(fpCamMountTran.forward, fpCamMountTran.up);
            //rb.rotation = rotTarget;
            Quaternion rotInterpolate = Quaternion.Slerp(
                rotCurrent,
                rotTarget,
                interpolateSpeed * Time.deltaTime
            );
            //rb.rotation = rotInterpolate;
            Quaternion rotICT = Quaternion.Inverse(rotCurrent) * rotInterpolate; //ICT = InterpolatedDirectionFromCurrentToTarget
            //rb.rotation *= rotDiff;

            //Vector3 rotCurrentVector = rb.rotation * transform.forward;

            Vector3 vectorTorque = rotICT * Vector3.one;

            float torqueStr = 2000f * (vectorTorque - transform.forward).magnitude;

            rb.AddTorque(vectorTorque * torqueStr * Time.deltaTime);


            //THE ISSUE IS **ALONG WHAT AXIS SHOULD THE ROTATION BE APPLIED?**
            //HOW DOES rb.rotation DETERMINE THIS?
            */

            /*
             * using 3 points that you have (ship front, ship center, rotation target) you can calculate a plane,
             * then you can easily find a perpendicular axis to the center of the ship,
             * use that as axis you need to rotate the ship around,
             * you might also need to calculate 2nd force to level the ship if you have a free camera
             */
        }
    }

    private void UpdatePlayerMovementThrust()
    {
        //Debug.Log(playerThrustEngineWarmupMultiplier);

        //Reset vector
        thrustVector = Vector3.zero;

        //Move if fuel
        if (vitalsFuel > 0.0)
        {
            //Faster if moving forward
            if (binds.GetInput(binds.bindThrustForward))
            {
                //Add forward to thrust vector
                thrustVector += transform.forward;

                //Engine warmup jerk (increasing acceleration)
                thrustEngineWarmupMultiplier = Mathf.Min(thrustEngineWarmupMultiplierMax,
                    thrustEngineWarmupMultiplier + (thrustEngineWarmupMultiplier * THRUST_ENGINE_WARMUP_SPEED * Time.deltaTime));
                
                //Total multiplier
                thrustMultiplier = THRUST_FORWARD_MULTIPLIER * thrustEngineWarmupMultiplier;
            }
            else
            {
                //Engine warmup
                thrustEngineWarmupMultiplier = Mathf.Max(1f,
                    thrustEngineWarmupMultiplier - (Time.deltaTime * THRUST_ENGINE_COOLDOWN_SPEED));

                //Total multiplier
                thrustMultiplier = 1f;
            }
            
            //We don't want the player to be able to move if the moving check fails
            //(it's not just a shotcut to detect any input, it also detects if the player CAN move)
            //We exclude the above from this check as some parts like the engine warmup must be able to decrement even when unable to move
            if (canAndIsMoving)
            {
                if (binds.GetInput(binds.bindThrustBackward)) thrustVector += -transform.forward;
                if (binds.GetInput(binds.bindThrustLeft)) thrustVector += -transform.right;
                if (binds.GetInput(binds.bindThrustRight)) thrustVector += transform.right;
                if (binds.GetInput(binds.bindThrustUp)) thrustVector += transform.up;
                if (binds.GetInput(binds.bindThrustDown)) thrustVector += -transform.up;

                rb.AddForce(thrustVector.normalized * THRUST * thrustMultiplier * Time.deltaTime);
            }
        }
    }
    #endregion

    #region Methods called in update: Camera
    private void UpdateCameraMovement()
    {
        if (!Menu.menuOpenAndGamePaused && !Commerce.menuOpen)
        {
            //Map
            if (binds.GetInputDown(binds.bindToggleMap))
            {
                control.ui.ToggleMapView();
            }

            if (UI.displayMap)
            {
                //Set map to player position
                mapCam.transform.position = transform.position + (Vector3.up * mapCam.GetComponent<Camera>().farClipPlane / 2f);

                //Set map zoom (default 1560)
                if (binds.GetInput(binds.bindCameraZoomOut))
                {
                    mapCam.GetComponent<Camera>().orthographicSize = Mathf.Min(7000.0f, mapCam.GetComponent<Camera>().orthographicSize *= 1.1f);
                }
                else if (binds.GetInput(binds.bindCameraZoomIn))
                {
                    mapCam.GetComponent<Camera>().orthographicSize = Mathf.Max(10.0f, mapCam.GetComponent<Camera>().orthographicSize *= 0.9f);
                }
            }
            else
            {
                //Not map
                if (binds.GetInput(binds.bindCameraZoomIn) || binds.GetInput(binds.bindCameraZoomOut))
                {
                    SetCameraFollowDistance();
                }

                GetMouseToCameraTransform();
            }
        }

        //We do this outside of the menuOpen check so that the camera won't lag behind the player by one frame when the menu is opened
        UpdateMountPositions();
    }
    #endregion

    #region Methods called in update: UI
    private void UpdateWarningText()
    {
        if (warningUIFlashTime > 0f)
        {
            //Flash
            if (warningUIFlashPosition > -1f)
            {
                warningUIFlashPosition -= WARNING_UI_FLASH_RATE * Time.deltaTime;
            }
            else
            {
                warningUIFlashPosition = 1f;
            }

            warningUIText.color = new Color(1f, 0f, 0f, Mathf.Abs(warningUIFlashPosition));

            //Decrement and clean up
            warningUIFlashTime -= 1f * Time.deltaTime;

            //(Even though we JUST checked if flashTime > 0, this CAN be called because we JUST decremented the var)
            if (warningUIFlashTime <= 0f)
            {
                warningUIText.color = new Color(1f, 0f, 0f, 0f);
            }
        }
        else
        {
            warningUIFlashPosition = 1f;
        }
    }
    #endregion

    #region Methods called in update: Weapons
    private void UpdatePlayerWeapons()
    {
        if (upgradesInitialized)
        {
            //UI
            control.ui.UpdatePlayerWeaponsUI();

            //Cooldowns
            UpdateWeaponSelected();

            //Fire
            if
            (
                !destroyed
                && Application.isFocused
                && !Menu.menuOpenAndGamePaused
                && !Commerce.menuOpen
                && !UI.displayMap
                && binds.GetInput(binds.bindPrimaryFire)
                && weaponSelectedSingleCooldownCurrent <= 0f
                && weaponSelectedClipCooldownCurrent <= 0f
            )
            {
                WeaponsFire();
            }
        }
    }
    #endregion

    private void UpdateAudio()
    {
        //ROCKET SOUND
        AdjustRocketSound();

        //MUSIC
        if (Time.time >= musicPlayTime)
        {
            PlayMusic();
        }
    }
    #endregion

    #region General methods
    #region General methods: Upgrades
    public void UpdateUpgrades()
    {
        //Vitals
        vitalsHealthMax = VITALS_HEALTH_MAX_STARTER * (1 + upgradeLevels[control.commerce.UPGRADE_REINFORCED_HULL]);
        //vitalsHealth = vitalsHealthMax;

        vitalsFuelMax = VITALS_FUEL_MAX_STARTER * (1 + upgradeLevels[control.commerce.UPGRADE_TITAN_FUEL_TANK]);
        //Debug.LogFormat("{0}, {1}, {2}", vitalsFuelMax, VITALS_FUEL_MAX_STARTER, upgradeLevels[control.commerce.UPGRADE_TITAN_FUEL_TANK]);
        //vitalsFuel = vitalsFuelMax;

        control.ui.UpdatePlayerVitalsDisplay();

        //Movement
        thrustEngineWarmupMultiplierMax = THRUST_ENGINE_WARMUP_MULTIPLIER_MAX_STARTER * (1 + upgradeLevels[control.commerce.UPGRADE_RAPTOR_ENGINES]);

        //Weapons
        playerWeaponLaser.UpdateUpgrades();
        playerWeaponSeismicCharge.UpdateUpgrades();
        //control.ui.UpdatePlayerWeaponsUI();
        //This is called in update anyway
    }
    #endregion

    #region General methods: Movement
    private void TorqueAxisRelative(float torque, Vector3 cameraDirection, Vector3 playerShipDirection)
    {
        float errorThreshold = 0.25f;
        float turnRate = 0.12f; //0.12082853855005753739930955120829f;

        Vector3 cameraToShipCross = Vector3.Cross(-cameraDirection, playerShipDirection);
        float angleDifference = Mathf.Abs(Vector3.Cross(rb.angularVelocity, playerShipDirection).magnitude);

        if (Mathf.Abs(cameraToShipCross.magnitude) > errorThreshold * angleDifference)
        {
            rb.AddTorque(cameraToShipCross * torque * turnRate * Time.deltaTime);
        }
    }
    #endregion

    #region General methods: Camera
    private void SetCameraFollowDistance()
    {
        //Camera follow distance
        //Zoom in
        control.settings.cameraDistance = Math.Min(
            control.settings.CAMERA_DISTANCE_MAX,
            Math.Max(
                control.settings.CAMERA_DISTANCE_MIN,
                control.settings.cameraDistance - (((Convert.ToSingle(binds.GetInput(binds.bindCameraZoomIn)) - 0.5f) * 2f) / 40f)
            )
        );

        //Save follow distance to user settings file (really, this saves ALL settings
        control.settings.Save();

        //Check if should be first-person, third-person, or no person!
        DecideWhichModelsToRender();
    }

    public void DecideWhichModelsToRender()
    {
        //Decide if in first-person or third-person view
        //Defaults (will be values if destroyed)
        firstPerson = false;
        thirdPerson = false;

        if (!destroyed)
        {
            firstPerson = control.settings.cameraDistance <= control.settings.CAMERA_DISTANCE_MIN + 0.01f;
            thirdPerson = !firstPerson;
        }

        //First-person cameras & model
        fpCam.SetActive(firstPerson);
        fpCamInterior.SetActive(firstPerson);
        fpModel.SetActive(firstPerson);

        //Third-person camera & model
        tpCam.SetActive(thirdPerson || destroyed);
        tpModel.SetActive(thirdPerson);

        //Spotlight
        if (!destroyed && control.settings.spotlightOn)
        {
            transform.Find("Spotlight").gameObject.SetActive(true);
        }
        else
        {
            transform.Find("Spotlight").gameObject.SetActive(false);
        }

        //Jet glow
        transform.Find("Jet Glow").gameObject.SetActive(!destroyed);

        //Ship direction reticles
        control.ui.playerShipDirectionReticleTree.SetActive(!destroyed);
    }

    private void GetMouseToCameraTransform()
    {
        //Debug.LogFormat("Pitch {0}, Yaw {1}", fpCamPitch, fpCamYaw);

        //Pitch
        centreMountPitch -= Input.GetAxisRaw("Mouse Y") * control.settings.mouseSensitivity * MOUSE_SENS_COEFF;
        //Yaw
        if (centreMountPitch >= 90 && centreMountPitch < 270)
        {
            //Normal
            centreMountYaw -= Input.GetAxisRaw("Mouse X") * control.settings.mouseSensitivity * MOUSE_SENS_COEFF;
        }
        else
        {
            //Inverted
            centreMountYaw += Input.GetAxisRaw("Mouse X") * control.settings.mouseSensitivity * MOUSE_SENS_COEFF;
        }
        //Roll
        centreMountRoll = 0f;

        Control.LoopEulerAngle(centreMountYaw);
        Control.LoopEulerAngle(centreMountPitch);
        Control.LoopEulerAngle(centreMountRoll);
    }

    private void UpdateMountPositions()
    {
        //CENTRE
        //Set the centre mount's transform
        centreMountTran.localRotation = Quaternion.Euler(centreMountPitch, centreMountYaw, 0f);

        //FIRST-PERSON
        fpCamMount.transform.position = centreMountTran.position + (transform.forward * 0.115f) + (transform.up * 0.008f);

        //fpCamMount.transform.localRotation = Quaternion.Euler(fpCamPitch, fpCamYaw, 0f);

        //0.14 y
        //2.5 z

        //THIRD-PERSON
        //tpCamMount.transform.position = transform.position;
        tpCamMount.transform.localRotation = Quaternion.Euler(centreMountPitch, centreMountYaw, 0f);

        float cameraSpeedEffect = 1f; //1f + Mathf.Pow(rb.velocity.magnitude, 0.15f);
        Vector3 cameraUp = centreMountTran.up * (control.settings.cameraDistance * control.settings.cameraHeight) * cameraSpeedEffect;
        Vector3 cameraForward = centreMountTran.forward * control.settings.cameraDistance * cameraSpeedEffect;
        tpCamMount.transform.position = transform.position + cameraUp - cameraForward; //subtracting forward results in the camera following behind the player, this should be more performant than *-1
        //tpCamMount.transform.position = (transform.position + (fpCamMountTran.up * set_tpCamFollowDistance * set_tpCamFollowHeight) - (fpCamMountTran.forward * set_tpCamFollowDistance));
    }

    public void SetCameraSettings()
    {
        //Clip planes
        fpCamInterior.GetComponent<Camera>().nearClipPlane = FP_CAM_INTERIOR_CLIPPING_PLANE_NEAR;
        fpCamInterior.GetComponent<Camera>().farClipPlane = FP_CAM_INTERIOR_CLIPPING_PLANE_FAR;

        //HFOV
        if (control.settings.hFieldOfView > 0f)
        {
            fpCamInterior.GetComponent<Camera>().fieldOfView = Camera.HorizontalToVerticalFieldOfView(control.settings.hFieldOfView, fpCamInterior.GetComponent<Camera>().aspect);
            fpCam.GetComponent<Camera>().fieldOfView = Camera.HorizontalToVerticalFieldOfView(control.settings.hFieldOfView, fpCam.GetComponent<Camera>().aspect);
            tpCam.GetComponent<Camera>().fieldOfView = Camera.HorizontalToVerticalFieldOfView(control.settings.hFieldOfView, tpCam.GetComponent<Camera>().aspect);

            fovSet = true;
        }
    }
    #endregion

    #region General methods: Weapons
    private void WeaponsFire()
    {
        if (weaponSelectedTitle == "Laser")
        {
            playerWeaponLaser.Fire();

            //Debug.LogFormat("Ship angle: {0}, view angle: {1}", transform.localRotation.eulerAngles, centreMountTran.localRotation.eulerAngles);
            //Debug.Log("ship to look diff: " + (transform.localRotation.eulerAngles - centreMountTran.localRotation.eulerAngles).magnitude);
            //Debug.Log("ship to look diff: " + (transform.localRotation * Quaternion.Inverse(centreMountTran.localRotation)));
            //Debug.Log("ship to look diff: " + Mathf.Abs(Quaternion.Dot(transform.localRotation, centreMountTran.localRotation)));

            if (
                !binds.GetInput(binds.bindCameraFreeLook) &&
                Mathf.Abs(Quaternion.Dot(transform.localRotation, centreMountTran.localRotation)) < control.ui.TIP_AIM_THRESHOLD_ACCURACY
                )
            {
                control.ui.tipAimCertainty++;
                //control.ui.SetTip("Not aiming at centre!");
            }
        }
        else if (weaponSelectedTitle == "Seismic charges")
        {
            playerWeaponSeismicCharge.Fire();
        }
    }

    public void WeaponsDestroyTrees()
    {
        Control.DestroyAllChildren(playerWeaponsTreeLaser, 0f);
        Control.DestroyAllChildren(playerWeaponsTreeSeismicCharge, 0f);
    }

    private void UpdateWeaponSelected()
    {
        //Select
        if (binds.GetInputDown(binds.bindSelectWeapon1))
        {
            weaponSelectedTitle = "Laser";
        }
        else if (upgradeLevels[control.commerce.UPGRADE_SEISMIC_CHARGES] >= 1 && binds.GetInputDown(binds.bindSelectWeapon2))
        {
            weaponSelectedTitle = "Seismic charges";
        }

        //Get properties
        if (weaponSelectedTitle == "Laser")
        {
            //Properties
            weaponSelectedClipSize = playerWeaponLaser.clipSize;
            weaponSelectedClipRemaining = playerWeaponLaser.clipRemaining;
            weaponSelectedClipCooldownDuration = playerWeaponLaser.CLIP_COOLDOWN_DURATION;
            weaponSelectedClipCooldownCurrent = playerWeaponLaser.clipCooldownCurrent;

            weaponSelectedSingleCooldownDuration = playerWeaponLaser.SINGLE_COOLDOWN_DURATION;
            weaponSelectedSingleCooldownCurrent = playerWeaponLaser.singleCooldownCurrent;
        }
        else if (weaponSelectedTitle == "Seismic charges")
        {
            //Properties
            weaponSelectedClipSize = playerWeaponSeismicCharge.clipSize;
            weaponSelectedClipRemaining = playerWeaponSeismicCharge.clipRemaining;
            weaponSelectedClipCooldownDuration = playerWeaponSeismicCharge.CLIP_COOLDOWN_DURATION;
            weaponSelectedClipCooldownCurrent = playerWeaponSeismicCharge.clipCooldownCurrent;

            weaponSelectedSingleCooldownDuration = playerWeaponSeismicCharge.SINGLE_COOLDOWN_DURATION;
            weaponSelectedSingleCooldownCurrent = playerWeaponSeismicCharge.singleCooldownCurrent;
        }

        //UI
        control.ui.UpdateWeaponAlternate(weaponSelectedTitle, upgradeLevels[control.commerce.UPGRADE_SEISMIC_CHARGES] >= 1);
        control.ui.UpdateWeaponSelected(weaponSelectedTitle);
    }

    
    #endregion

    #region General methods: Audio
    private void PlayMusic()
    {
        //Select the track
        if (firstSong)
        {
            float songToPlay = UnityEngine.Random.value;
            if (songToPlay >= 0f && songToPlay < 0.5f)
            {
                music.clip = songDrifting;
            }
            else
            {
                music.clip = songLifeSupportFailure;
            }

            firstSong = false;
        }
        else
        {
            if (music.clip == songLifeSupportFailure)
            {
                music.clip = songDrifting;
            }
            else
            {
                music.clip = songLifeSupportFailure;
            }
        }

        /*
        if (songToPlay >= 0f && songToPlay < 0.25f)
        {
            music.clip = songDrifting;
        }
        else if (songToPlay >= 0.25f && songToPlay < 0.5f)
        {
            music.clip = songLifeSupportFailure;
        }
        else if (songToPlay >= 0.5f && songToPlay < 0.75f)
        {
            music.clip = songHoghmanTransfer;
        }
        else //0.75f and 1f
        {
            music.clip = songWeWereHere;
        }
        */

        //Play the track
        if (control.settings.music)
        {
            music.Play();
        }
        
        //Queue another song for after the current one finishes
        musicPlayTime = Time.time + music.clip.length + UnityEngine.Random.Range(MUSIC_PLAY_QUEUE_TIME, MUSIC_PLAY_QUEUE_TIME + MUSIC_PLAY_QUEUE_VARIANCE_TIME);
    }

    private void AdjustRocketSound()
    {
        //Don't loop endlessly while paused
        if (Menu.menuOpenAndGamePaused)
        {
            soundSourceRocket.Stop();
        }
        else if (!soundSourceRocket.isPlaying)
        {
            soundSourceRocket.Play();
        }

        //Adjust volume and pitch with movement
        if (canAndIsMoving)
        {
            soundSourceRocket.volume = Mathf.Min(SOUND_ROCKET_MAX_VOLUME, soundSourceRocket.volume + (Time.deltaTime * SOUND_ROCKET_VOLUME_DELTA_RATE));

            soundSourceRocket.pitch = Mathf.Min(SOUND_ROCKET_MAX_PITCH, soundSourceRocket.pitch + (Time.deltaTime * SOUND_ROCKET_PITCH_DELTA_RATE));
        }
        else
        {
            soundSourceRocket.volume = Mathf.Max(0f, soundSourceRocket.volume - ((soundSourceRocket.volume * Time.deltaTime * SOUND_ROCKET_VOLUME_DELTA_RATE) * 32f));

            soundSourceRocket.pitch = Mathf.Max(1f, soundSourceRocket.pitch - ((soundSourceRocket.pitch * Time.deltaTime * SOUND_ROCKET_PITCH_DELTA_RATE) * 32f));
        }
    }
    #endregion

    #region General methods: UI
    private void FlashWarning(string warningText)
    {
        warningUIText.text = warningText;
        warningUIFlashTime = warningUIFlashTotalDuration;
    }
    #endregion

    #region General methods: ClosestTransforms
    private Transform GetClosestTransform(Transform[] transforms)
    {
        Transform closestTransform = null;
        float closestDistanceSqr = Mathf.Infinity;

        //The position to compare distances to, usually the player's position
        Vector3 sourcePosition = transform.position;

        foreach (Transform transformToCheck in transforms)
        {
            Vector3 vectorToTransformToCheck = transformToCheck.position - sourcePosition;

            float distanceSqrToTransformToCheck = vectorToTransformToCheck.sqrMagnitude;
            if (distanceSqrToTransformToCheck < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqrToTransformToCheck;
                closestTransform = transformToCheck;
            }
        }

        return closestTransform;
    }

    private Transform GetClosestPlanetoidTransform()
    {
        Transform[] planetoidTransforms = new Transform[cBodies.transform.Find("Planetoids").childCount];
        for (int i = 0; i < cBodies.transform.Find("Planetoids").childCount; i++)
        {
            planetoidTransforms[i] = cBodies.transform.Find("Planetoids").GetChild(i);
        }

        return GetClosestTransform(planetoidTransforms);
    }

    private Transform GetClosestAsteroidTransform()
    {
        Transform[] asteroidTransforms = new Transform[cBodies.transform.Find("Asteroids").childCount];
        for (int i = 0; i < cBodies.transform.Find("Asteroids").childCount; i++)
        {
            asteroidTransforms[i] = cBodies.transform.Find("Asteroids").GetChild(i);
        }

        return GetClosestTransform(asteroidTransforms);
    }
    #endregion

    #region General methods: Damage
    void OnCollisionEnter(Collision collision)
    {
        //COLLISION PROPERTIES
        //Collision speed
        float impactIntoleranceThreshold = 5f;
        float impactIntoleranceRange = 6f;
        float impactMaxDamage = 3f;

        Vector3 impactDeltaV = collision.relativeVelocity;

        //SELF
        double damageToDeal = 0.0d;
        if (impactDeltaV.magnitude >= impactIntoleranceThreshold)
        {
            //Play sound effect
            soundSourceCollision.volume = 0.05f;
            soundSourceCollision.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
            soundSourceCollision.Play();

            //Damage
            damageToDeal = Math.Min(
                    impactIntoleranceThreshold * impactIntoleranceRange,
                    impactDeltaV.magnitude
                ) / (impactIntoleranceThreshold * impactIntoleranceRange / impactMaxDamage);

            double newHealthAmount = Math.Max(
                0.0,
                vitalsHealth - damageToDeal
            );

            DamagePlayer(
                newHealthAmount,
                "over-tolerance impact of " + (int)impactDeltaV.magnitude + " Δv"
            );
        }
        else
        {
            //Play sound effect
            soundSourceCollision.volume = 0.01f;
            soundSourceCollision.pitch = UnityEngine.Random.Range(0.4f, 0.8f);
            soundSourceCollision.Play();
        }

        //IMPACTED OBJECT
        //If an asteroid, deal damage to it
        if (collision.gameObject.name == control.generation.cBodyAsteroid.name + "(Clone)")
        {
            //Get ref
            CBodyAsteroid asteroidScript = collision.transform.GetComponent<CBodyAsteroid>();
            //Get direction and contact point (sloppy)
            Vector3 direction = (transform.position - collision.transform.position).normalized;
            Vector3 contactPoint = collision.transform.position;
            //Get damage to deal
            byte damageToDealToAsteroid = (byte)Mathf.RoundToInt((float)damageToDeal);

            //Deal damage
            if (damageToDealToAsteroid > 0)
            {
                asteroidScript.Damage(damageToDealToAsteroid, direction, contactPoint, true);
            }

            Debug.Log("Damage dealt to asteroid:" + damageToDealToAsteroid);
        }
    }

    public void DamagePlayer(double newHealthAmount, string cause)
    {
        if (!destroyed)
        {
            vitalsHealth = newHealthAmount;
            control.ui.UpdatePlayerVitalsDisplay(); //force a vitals update so that you can immediately see your health change
            FlashWarning("WARNING: " + cause + "\nHull integrity compromised"); //⚠
                                                                                //deathMessage = "You died.\nLast recorded warning message: " + cause
            
            if (vitalsHealth <= 0f)
            {
                DestroyPlayer();
            }
        }
    }

    private void DestroyPlayer()
    {
        //Emit particles
        GetComponent<ParticlesDamageRock>().EmitDamageParticles(7, Vector3.zero, transform.position, true);

        //Play sound
        //TODO

        //Remember is destroyed
        destroyed = true;

        //Hide models (after destroyed is called because their visibility is determined by that variable)
        DecideWhichModelsToRender();
    }
    #endregion
    #endregion
}
