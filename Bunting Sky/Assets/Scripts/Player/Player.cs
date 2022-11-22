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
    private float cameraZoom = 1f;
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
    private readonly float FP_CAM_INTERIOR_CLIPPING_PLANE_FAR = 0.1f; //1e21f;
    public GameObject tpCam;
    public GameObject tpModel;
    public GameObject fpModel;
    [System.NonSerialized] public static bool firstPerson = false;
    [System.NonSerialized] public static bool thirdPerson = false;
    public Material tractorBeamMaterial;
    public GameObject mapCam;
    public GameObject mapLight;
    private Vector3 mapOffset = Vector3.zero;
    [System.NonSerialized] Vector3 cameraOffsetPositionMagnitude = Vector3.zero;
    [System.NonSerialized] Vector3 cameraOffsetPosition = Vector3.zero;
    private readonly float CAMERA_OFFSET_POSITION_SMOOTHING_RATE = 0.91f; //0 to 1, approaching zero makes smoothing faster
    [System.NonSerialized] public readonly float CAMERA_OFFSET_POSITION_MAGNITUDE_MAX = 0.005f; //maximum positional camera shake allowed
    private float cameraOffsetRotationMagnitudePitch = 0f;
    private float cameraOffsetRotationMagnitudeYaw = 0f;
    private float cameraOffsetRotationYaw = 0f;
    private float cameraOffsetRotationPitch = 0f;
    private readonly float CAMERA_OFFSET_ROTATION_SMOOTHING_RATE = 0.91f; //0 to 1, approaching zero makes smoothing faster
    [System.NonSerialized] public readonly float CAMERA_OFFSET_ROTATION_MAGNITUDE_MAX = 5f; //maximum positional camera shake allowed
    [System.NonSerialized] public readonly Vector2 CAMERA_SHAKE_WEAPON = new Vector2(0.0003f, 0.0016f); //camera shake a small weapon creates when fired
    private Vector3 cameraOffsetAcceleration = Vector3.zero;
    [System.NonSerialized] public Vector3 acceleration = Vector3.zero;
    [System.NonSerialized] public Vector3[] accelerationPrevious = new Vector3[37]; //15 < a < 50 //37 //30
    private Vector3 lastVelocity = Vector3.zero;
    #endregion

    //Abilities
    //Spotlight
    public GameObject spotlight;
    [System.NonSerialized] public bool spotlightIsOn = false;
    [System.NonSerialized] private bool spotlightIsFlickeredOff = false;
    [System.NonSerialized] private readonly float SPOTLIGHT_PERIOD_ENABLED_MAX = 20f; //3f; //Maximum time in seconds active for
    [System.NonSerialized] private float spotlightEnergyRemaining = 20f; //3f; //Time in seconds active for (starts off as the same as enabled_max)
    [System.NonSerialized] private readonly float SPOTLIGHT_PERIOD_COOLDOWN = 3f; //Time in seconds before can use again
    [System.NonSerialized] private float spotlightEnergyDepletedTime = 0f; //Time last depleted at
    //Eclipse Vision
    [System.NonSerialized] public bool isOutlinesVisible = false;
    [System.NonSerialized] public float outlineFade = 1f; //Multiplier for outline intensity - DO NOT EDIT; this changes dynamically
    [System.NonSerialized] public readonly float OUTLINE_PERIOD_FADING_WHILE_ENABLED = 2f; //Fade out over this period of time, in seconds
    [System.NonSerialized] public readonly float OUTLINE_PERIOD_ENABLED = 6f; //3f; //Time in seconds outlines will show for
    [System.NonSerialized] public readonly float OUTLINE_PERIOD_COOLDOWN = 1.5f; //Time in seconds before outlines can be shown again
    [System.NonSerialized] public float outlineActivatedTime = -100f; //Time when the outline was last activated - CHANGES DYNAMICALLY

    #region Init fields: Movement
    //Movement
    public Rigidbody rb;
    //Thrust
    private Vector3 thrustVector;
    [System.NonSerialized] public readonly float THRUST = 13e3f * 0.6666666666666666666f; //13e3f; //10e3f; //12000f; //16e3f; //4e4f; //4e3f; //3e3f; //8416.65825f;
    private float thrustEngineWarmupMultiplier = 1f;
    private float thrustEngineWarmupMultiplierMax;
    private readonly float THRUST_REDUCTION_TO_COMPENSATE_NO_DRAG_MULTIPLIER = 0.1f; //How much thrust you have with matchVelocity setting turned off as compared to normal
    private readonly float THRUST_ENGINE_WARMUP_MULTIPLIER_MAX_STARTER = 3.0f; //6.0f; //9.0f; //5.0f; //16f; //multiplies forward thrust after holding forward for awhile
    private readonly float THRUST_ENGINE_WARMUP_SPEED = 0.25f; //0.5f; //3f;
    private readonly float THRUST_ENGINE_COOLDOWN_SPEED = 12f;
    private readonly float THRUST_FORWARD_MULTIPLIER = 1.1f; //extra thrust for moving forward rather than strafing
    private float thrustMultiplier = 1f; //internally used multiplier to keep track of total multiplier
    private float thrustCheat = 1f;
    [System.NonSerialized] public Vector3 lastForceAdded = Vector3.zero;
    //Torque
    private float torqueBaseStrength = 500f; //30f;
    private float angularDragWhenEnginesOn = 40f; //40f; //for smoothing
    //Engine glow
    private float engineBrightness = 0f;
    public Material engineGlowMat;
    private Color engineEmissionColor = new Color(191, 102, 43);
    private Color engineEmissionColorRunningHot = new Color(199, 35, 35);
    private Color engineEmissionColorRunning = new Color(191, 102, 43);
    public Color engineEmissionColorDead = new Color(49, 195, 255);
    private float engineEmissionIntensity = 1.3f * 0.00748f; //1.4f * 0.00748f; //1.631096f;
    public Light engineLight;
    //Ability to move at all
    private bool canAndIsMoving = false;
    private float tempEngineDisable = 0f;
    private bool tempEngineDisableButFlickering = false;
    private float tempEngineDisableCurrentDuration = 0f;
    private float tempEngineDisablePeriodToPossiblyFlicker = 0.1f; //how many seconds to in between each flicker chance
    private float tempEngineDisableFlickerChance = 0.25f; //1 = 100%

    //Movement: Relative drag
    [System.NonSerialized] public GameObject targetObject;
    public GameObject targetObjectLeadGhost;

    [System.NonSerialized] public GameObject cBodies;
    [System.NonSerialized] public readonly float ORBITAL_DRAG_MODE_THRESHOLD = 50f;
    [System.NonSerialized] public Vector3 velocityOfObjectDraggingRelativeTo = Vector3.zero; //used for fired projectiles
    private float distToClosestMoonOrMoonChild = 100f; //this should be greater than the orbitalDragModeThreshold so that the player starts with drag relative to system
    private Transform closestMoonOrMoonChildTransform;
    private float distToClosestAsteroid = 100f;
    private Transform closestAsteroidTransformToDragRelativeTo;
    private readonly float DRAG = 2f; //3f; //Drag amount for all drag modes

    //Movement: heighliner teleports
    [System.NonSerialized] public bool recentTeleport = false;
    [System.NonSerialized] public float collisionDamageInvulnerabilityTemporary = 0f;
    #endregion

    #region Init fields: Audio
    //Audio: Music
    public AudioSource music;
    public AudioClip songDrifting;
    public AudioClip songLifeSupportFailure;
    public AudioClip songHoghmanTransfer; //unused
    public AudioClip songWeWereHere; //unused
    public AudioClip songCombat;
    [System.NonSerialized] public float combatLastAggroTime = -1e5f; //The last time point where the player aggro'd an enemy
    [System.NonSerialized] public int nEnemiesAggrod = 0;
    [System.NonSerialized] public readonly float COMBAT_PERIOD_THRESHOLD_TIMEOUT = 5f; //How much time in seconds the player can be out of combat before the combat flag times out, ending combat music, etc.
    private float musicPlayTime = 30f; //max time until first song plays
    private readonly float MUSIC_PLAY_QUEUE_TIME = 30f; //time in seconds AFTER the previous song finishes playing before the next song plays
    private readonly float MUSIC_PLAY_QUEUE_VARIANCE_TIME = 90f; //random variance in time between songs
    [System.NonSerialized] public readonly float SOUND_MUSIC_VOLUME = 0.04f;
    private bool firstSong = true;

    //Audio: Sound Effects
    public AudioSource soundSourceRocket;
    public AudioClip soundClipRocket;
    [System.NonSerialized] public readonly float SOUND_ROCKET_VOLUME = 0.027f; //0.4f; //0.02f;
    private readonly float SOUND_ROCKET_VOLUME_DELTA_RATE = 0.2f; //0.1f;
    private readonly float SOUND_ROCKET_PITCH_MAX = 1.5f;
    private readonly float SOUND_ROCKET_PITCH_DELTA_RATE = 0.2f;
    [System.NonSerialized] public readonly float SOUND_IMPACT_VOLUME_SIGNIFICANT = 0.067f;
    private readonly float SOUND_IMPACT_VOLUME_INSIGNIFICANT = 0.027f;
    [System.NonSerialized] public readonly float SOUND_LASER_FIRE_VOLUME = 0.017f;
    [System.NonSerialized] public readonly float SOUND_LASER_RELOAD_VOLUME = 0.075f; //0.06f; //0.03f; //0.0175f; //0.02f;
    [System.NonSerialized] public readonly float SOUND_SEISMIC_CHARGE_FIRE_VOLUME = 0.067f;
    
    public AudioSource soundSourceLaser0;
    public AudioSource soundSourceLaser1;
    public AudioSource soundSourceLaser2;
    public AudioSource soundSourceLaser3;
    [System.NonSerialized] public byte soundSourceLaserArrayIndex = 0;
    [System.NonSerialized] public byte soundSourceLaserArrayLength = 4;
    public AudioClip soundClipLaser;

    public AudioSource soundSourceLaserReload;
    public AudioClip soundClipLaserReload;

    public AudioSource soundSourceSeismicCharge0;
    public AudioSource soundSourceSeismicCharge1;
    [System.NonSerialized] public int soundSourceSeismicChargeArrayIndex = 0;
    [System.NonSerialized] public int soundSourceSeismicChargeArrayLength = 2;
    public AudioClip soundClipSeismicCharge;
    public AudioSource soundSourceSeismicChargeExplosion;
    [System.NonSerialized] public readonly float SOUND_SEISMIC_CHARGE_EXPLOSION_VOLUME = 0.067f;

    public AudioSource soundSourceOreCollected;
    [System.NonSerialized] public readonly float SOUND_ORE_COLLECTED_VOLUME = 0.01f;
    public AudioClip soundClipOreCollected;

    public AudioSource soundSourceCurrencyChange;
    [System.NonSerialized] public readonly float SOUND_CURRENCY_CHANGE_VOLUME = 0.04f; //0.067f;

    public AudioSource soundSourceCollision;
    public AudioClip soundClipCollision;
    #endregion

    #region Init fields: Vitals
    //Vitals
    [System.NonSerialized] public double vitalsHealth = 10.0d; //hull integrity (10), fuel (30L), (deprecated) oxygen (840g)
    [System.NonSerialized] public double vitalsHealthMax = 10.0d;
    private bool healthInfiniteCheat = false;
    private readonly double VITALS_HEALTH_MAX_STARTER = 10.0d;
    private string lastDamageCause = "[no damage event detected]";
    [System.NonSerialized] public bool isDestroyed = false;
    [System.NonSerialized] public double vitalsFuel = 3.0d; //this is overridden by generation, as fuel needs to be reset every new game
    [System.NonSerialized] public double vitalsFuelMax = 4.0d;
    private readonly double VITALS_FUEL_MAX_STARTER = 4.0d;
    [System.NonSerialized] public double vitalsFuelConsumptionRate = 0.025d;
    [System.NonSerialized] public GameObject vitalsHealthUI;
    [System.NonSerialized] public TextMeshProUGUI vitalsHealthUIText;
    [System.NonSerialized] public GameObject vitalsFuelUI;
    [System.NonSerialized] public TextMeshProUGUI vitalsFuelUIText;
    [System.NonSerialized] public TextMeshProUGUI warningUIText;
    private float warningUIFlashTime = 0f;
    private float warningUIFlashPosition = 0f;
    private readonly float WARNING_UI_FLASH_RATE = 16f; //10f; //Must be even?
    private float warningUIFlashTotalDuration = 5f; //SET PROCEDURALLY BY RATE - This must be odd-numbered or it will not end smoothly (end while transparent)

    //TUTORIAL
    private int tutorialLevel = 0; //Which tutorial tip should be displayed next?
    private float tutorialTime = 0f; //The game time at which the next tutorial tip can be displayed
    private readonly float TUTORIAL_DELAY = 2f; //6f; //The standard delay between timed tutorial tips, in seconds
    private readonly float TUTORIAL_TIP_DURATION = 4f; //The standard delay between timed tutorial tips, in seconds
    private bool tutorialHasPressedForward = false;
    private bool tutorialHasPressedLeft = false;
    private bool tutorialHasPressedBackward = false;
    private bool tutorialHasPressedRight = false;
    private bool tutorialHasPressedDown = false;
    private bool tutorialHasPressedUp = false;
    [System.NonSerialized] public bool tutorialHasMinedAsteroid = false;
    [System.NonSerialized] public bool tutorialHasCollectedOre = false;
    [System.NonSerialized] public bool tutorialHasUsedStation = false;
    [System.NonSerialized] public int tutorialMoonVisitedID1 = -1;
    [System.NonSerialized] public int tutorialMoonVisitedID2 = -1;
    [System.NonSerialized] public int tutorialMoonVisitedID3 = -1;
    [System.NonSerialized] public bool tutorialHasUsedHeighliner = false;
    private bool tutorialHasPressedZoomInInMap = false;
    private bool tutorialHasPressedZoomOutInMap = false;
    private bool tutorialHasPressedPanMap = false;
    private bool tutorialHasPressedSetTarget = false;
    [System.NonSerialized] public bool tipHasBoughtOutline = false;
    [System.NonSerialized] public bool tipHasUsedOutline = false;
    [System.NonSerialized] public bool tipHasBoughtRefinery = false;
    [System.NonSerialized] public bool tipHasUsedRefinery = false;
    [System.NonSerialized] public bool tipHasBoughtSeismicCharges = false;
    [System.NonSerialized] public bool tipHasUsedSeismicCharges = false;
    private bool tutorialHasZoomedIn = false;
    private bool tutorialHasZoomedOut = false;

    //PARTICLES
    public GameObject damageParticlePrefab;
    private GameObject damageParticles; //We need a particle pool because the player ship moves around a lot (maybe could have just set particles to move relative to 0,0, but this works so I'm not changing it)
    [System.NonSerialized] public List<GameObject> damageParticlesPool = new List<GameObject>();
    private int damageParticlesPoolIndex = 0;
    private int damageParticlesPoolLength = 4;
    #endregion

    #region Init fields: Cargo
    //Cargo
    [System.NonSerialized] public double currency = 0.0d; //100.0; //ICC stands for interstellar crypto currency
    [System.NonSerialized] public double[] ore = new double[4] { 0.0d, 0.0d, 0.0d, 0.0d }; //0 = ClaySilicate, 1 = Platinoids, 2 = PreciousMetal, 3 = Water //we use double here because the refinery subtracts fractions
    [System.NonSerialized] public readonly double ORE_MAX_DEFAULT = 20.0d;
    [System.NonSerialized] public double oreMax = 20.0d;

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
    private readonly double REFINERY_ORE_WATER_IN_RATE = 1.0d; //0.1d;
    [System.NonSerialized] public readonly double REFINERY_FUEL_OUT_RATE = 0.1d; //0.75d; //0.075d;
    private readonly float REFINERY_TIME_BETWEEN_REFINES = 1.0f; //10.0f;
    private float refineryTimeAtLastRefine = 0f;
    private readonly float UPGRADE_ACCELERATION_MULTIPLIER = 2f; //how many times more thrust (and half as much more drag) we get per upgrade level - this DOES speed the player up
    private readonly float UPGRADE_TORQUE_FORCE_MULTIPLIER = 2f; //how many times more torque we get per upgrade level
    #endregion

    #region Init fields: Weapons
    //Tree
    [System.NonSerialized] public GameObject playerWeaponsTree;
    
    public PlayerWeaponLaser playerWeaponLaser;
    public PlayerWeaponSeismicCharge playerWeaponSeismicCharge;

    public struct Weapon
    {
        public readonly int ID;
        public readonly string NAME;

        public Weapon(int id, string name)
        {
            ID = id;
            NAME = name;
        }
    }
    [System.NonSerialized] public static readonly int WEAPON_ID_NONE = 0;
    [System.NonSerialized] public static Weapon weaponNone = new Weapon(WEAPON_ID_NONE, "<None>");
    [System.NonSerialized] public static readonly int WEAPON_ID_MINING_LASER = 1;
    [System.NonSerialized] public static Weapon weaponLaser = new Weapon(WEAPON_ID_MINING_LASER, "Nariman 40mm");
    [System.NonSerialized] public static readonly int WEAPON_ID_SEISMIC_CHARGES = 2;
    [System.NonSerialized] public static Weapon weaponSeismicCharges = new Weapon(WEAPON_ID_SEISMIC_CHARGES, "Seismic charges");
    [System.NonSerialized] public Weapon weaponSlot0 = weaponLaser; //Weapon in slot 0
    [System.NonSerialized] public Weapon weaponSlot1 = weaponNone; //Weapon in slot 1
    [System.NonSerialized] public int weaponSlotSelected = 0; //0 or 1

    [System.NonSerialized] public short weaponSelectedClipSize;
    [System.NonSerialized] public short weaponSelectedClipRemaining;
    [System.NonSerialized] public float weaponSelectedClipCooldownDuration;
    [System.NonSerialized] public float weaponSelectedClipCooldownCurrent;

    [System.NonSerialized] public float weaponSelectedSingleCooldownDuration;
    [System.NonSerialized] public float weaponSelectedSingleCooldownCurrent;

    [System.NonSerialized] public float weaponSelectedProjectileSpeed;

    [System.NonSerialized] public float weaponUsedRecently = 0f;
    #endregion

    //Skybox stars
    public ParticleSystem skyboxStarsParticleSystem;
    [System.NonSerialized] public readonly int SKYBOX_STARS_COUNT = 400;

    //Debug
    private Enemy.Strength enemyStengthToSpawn = Enemy.Strength.major;
    #endregion

    #region Start

    private void Start()
    {
        ////Dev mode modifications
        //if (control.IS_EDITOR)
        //{
        //    thrustCheat = 2f;
        //}

        //Damage particles pool
        damageParticles = control.generation.damageParticles; //get hierarchy reference
        for (int damageParticleIndex = 0; damageParticleIndex < damageParticlesPoolLength; damageParticleIndex++)
        {
            //Instantiate
            GameObject instanceDamageParticle = Instantiate(
                damageParticlePrefab,
                Vector3.zero,
                Quaternion.identity
            );

            //Hierarchy
            instanceDamageParticle.transform.parent = damageParticles.transform;

            //Add to pool
            damageParticlesPool.Add(instanceDamageParticle);
        }

        //MODEL
        DecideWhichModelsToRender();

        //SKYBOX
        skyboxStarsParticleSystem.Emit(SKYBOX_STARS_COUNT);

        //Setup the first tutorial tip to display after a short delay
        tutorialTime = Time.time + TUTORIAL_DELAY;
    }

    public void LateStart()
    {
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
        UpdateCameraSettings();
        
        //Vitals
        //We have to work with odd-numbered multiples of the inverse of the flash rate to end smoothly (end while it is transparent)
        warningUIFlashTotalDuration *= (1f / WARNING_UI_FLASH_RATE);

        //Update resources UI
        control.ui.UpdateAllPlayerResourcesUI();

        //Collection sound
        soundSourceOreCollected.clip = soundClipOreCollected;

        //AUDIO
        //Play the first song shortly after startup
        musicPlayTime = Time.time + 5f + UnityEngine.Random.Range(0f, musicPlayTime);

        //Init sounds (this is unnecessary)
        soundSourceRocket.clip = soundClipRocket;
        soundSourceLaser0.clip = soundClipLaser;
        soundSourceLaser1.clip = soundClipLaser;
        soundSourceLaser2.clip = soundClipLaser;
        soundSourceLaser3.clip = soundClipLaser;
        soundSourceLaserReload.clip = soundClipLaserReload;
        soundSourceCollision.clip = soundClipCollision;
        soundSourceSeismicCharge0.clip = soundClipSeismicCharge;
        soundSourceSeismicCharge1.clip = soundClipSeismicCharge;

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
        //CHEATS/DEBUG
        //---------------------------------------------------

        //control.ui.SetTip(
        //    "SpotlightOn: " + control.settings.spotlightOn.ToString()
        //    + " - menuSettingsToggleSpotlight.isOn: " + control.menu.menuSettingsToggleSpotlight.isOn.ToString()
        //);

        //I or O to cheat

        ////Constant velocity
        //if (binds.GetInput(binds.bindCheat1))
        //{
        //    rb.velocity = -transform.right * 8f;
        //    control.ui.SetTip("Velocity: " + rb.velocity);
        //}
        //if (binds.GetInput(binds.bindCheat2))
        //{
        //    rb.velocity = transform.right * 8f;
        //    control.ui.SetTip("Velocity: " + rb.velocity);
        //}

        ////Time warp
        //if (binds.GetInputDown(binds.bindCheat1))
        //{
        //    Time.timeScale += 0.25f;
        //
        //    control.ui.SetTip(Time.timeScale + "x");
        //}
        //
        //if (binds.GetInputDown(binds.bindCheat2))
        //{
        //    Time.timeScale -= 0.25f;
        //
        //    control.ui.SetTip(Time.timeScale + "x");
        //}

        ////Super thrust
        //if (binds.GetInputDown(binds.bindCheat1))
        //{
        //    thrustCheat += 0.25f;
        //
        //    control.ui.SetTip(thrustCheat + "x");
        //}
        //
        //if (binds.GetInputDown(binds.bindCheat2))
        //{
        //    thrustCheat -= 0.25f;
        //
        //    control.ui.SetTip(thrustCheat + "x");
        //}

        ////Toggle active nearest planet
        //if (binds.GetInputDown(binds.bindCheat1))
        //{
        //    GameObject instancePlanet = Control.GetClosestTransformFromHierarchy(control.generation.planets.transform, transform.position).gameObject;
        //    //instancePlanet.SetActive(!instancePlanet.activeSelf);
        //    int index = instancePlanet.GetComponent<PlanetarySystemBody>().planetarySystemIndex;
        //    int count = control.generation.planetarySystems[index].Count;
        //    control.ui.SetTip("Index: " + index + "; Count: " + count);
        //    for (int i = 0; i < count; i++)
        //    {
        //        GameObject instancePlanetarySystemBody = control.generation.planetarySystems[index][i];
        //        instancePlanetarySystemBody.SetActive(!instancePlanetarySystemBody.activeSelf);
        //    }
        //}

        ////Spawn an asteroid from the object pool
        //if (binds.GetInputDown(binds.bindCheat1))
        //{
        //    control.generation.AsteroidPoolSpawn(
        //        transform.position + (transform.forward * 20f),
        //        UnityEngine.Random.Range(0, Asteroid.SIZE_LENGTH),
        //        (byte)UnityEngine.Random.Range(0, Asteroid.TYPE_LENGTH)
        //    );
        //}

        ////Toggle all enabled asteroids' performance mode
        //if (binds.GetInputDown(binds.bindCheat2))
        //{
        //    for (int i = 0; i < control.generation.asteroidsEnabled.transform.childCount; i++)
        //    {
        //        Asteroid iAsteroidScript = control.generation.asteroidsEnabled.transform.GetChild(i).GetComponent<Asteroid>();
        //        iAsteroidScript.SetPerformant(!iAsteroidScript.performantMode);
        //    }
        //}

        ////Damage to 1hp
        //if (binds.GetInputDown(binds.bindCheat1))
        //{
        //    vitalsHealth = 0.1d;
        //}

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
        ////Unlock Seismic Charges (Z and X to select weapon)
        //if (binds.GetInputDown(binds.bindCheat1))
        //{
        //    upgradeLevels[control.commerce.UPGRADE_SEISMIC_CHARGES] = 1;
        //    UpdateUpgrades();
        //    control.ui.SetTip("Seismic charges upgrade unlocked");
        //}

        ////Free money
        //if (binds.GetInputDown(binds.bindCheat1))
        //{
        //    currency += 1000;
        //    control.ui.UpdateAllPlayerResourcesUI();
        //    control.ui.SetTip("+1000 currency");
        //}

        ////Temporarily disable engine
        //if (binds.GetInputDown(binds.bindCheat1))
        //{
        //    tempEngineDisable = 3f;
        //    control.ui.SetTip("Engines damaged!");
        //}

        ////Very low fuel
        //if (binds.GetInputDown(binds.bindCheat2))
        //{
        //    vitalsFuel = 0.05d;
        //    control.ui.UpdatePlayerVitalsDisplay();
        //    control.ui.SetTip("Running on fumes");
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

        //Cheats enabled only while in editor
        if (control.IS_EDITOR || true)
        {
            ////Spawn test ore
            //if (binds.GetInputDown(binds.bindCheat2))
            //{
            //    float startDistAway = 70f;
            //    float intervalDist = 40f;
            //    int amountOfEachType = 20;
            //
            //    for (int i = 0; i < amountOfEachType; i++)
            //    {
            //        control.generation.OrePoolSpawn(
            //            transform.position + (Vector3.forward * ((startDistAway - intervalDist) + (intervalDist * (i + 1)))),
            //            Asteroid.Type.water, rb.velocity
            //        );
            //    }
            //
            //    for (int i = 0; i < amountOfEachType; i++)
            //    {
            //        control.generation.OrePoolSpawn(
            //            transform.position + (Vector3.forward * ((startDistAway - intervalDist) + (intervalDist * (i + 1)))) + (Vector3.left * intervalDist),
            //            Asteroid.Type.platinoid, rb.velocity
            //        );
            //    }
            //
            //    for (int i = 0; i < amountOfEachType; i++)
            //    {
            //        control.generation.OrePoolSpawn(
            //            transform.position + (Vector3.forward * ((startDistAway - intervalDist) + (intervalDist * (i + 1)))) + (Vector3.right * intervalDist),
            //            Asteroid.Type.preciousMetal, rb.velocity
            //        );
            //    }
            //}

            //if (binds.GetInputDown(binds.bindCheat1))
            //{
            //    for (int i = 0; i < 18; i++)
            //    {
            //        control.generation.OrePoolSpawn(transform.position + Vector3.forward * 10f, Asteroid.Type.water, rb.velocity);
            //    }
            //}

            //if (binds.GetInputDown(binds.bindCheat2))
            //{
            //    control.generation.OrePoolSpawn(transform.position + Vector3.forward * 10f, Asteroid.Type.platinoid, rb.velocity);
            //    control.generation.OrePoolSpawn(transform.position + Vector3.forward * 30f, Asteroid.Type.preciousMetal, rb.velocity);
            //}

            //Teleport forward
            if (binds.GetInputDown(binds.bindCheat1))
            {
                transform.position += transform.forward * 150f;
            }

            ////Invulnerable
            //if (binds.GetInputDown(binds.bindCheat1))
            //{
            //    //Invulnerable
            //    healthInfiniteCheat = true;
            //}

            ////Eclipse vision upgrade
            //if (binds.GetInputDown(binds.bindCheat2))
            //{
            //    upgradeLevels[control.commerce.UPGRADE_OUTLINE] = 1;
            //    UpdateUpgrades();
            //}

            ////Spawn bandit
            //if (binds.GetInputDown(binds.bindCheat2))
            //{
            //    //Spawn next enemy in order of difficulty
            //    control.generation.EnemySpawn(transform.position + (transform.forward * 30f), enemyStengthToSpawn);
            //    enemyStengthToSpawn = (Enemy.Strength)(((int)enemyStengthToSpawn + 1) % Control.GetEnumLength(typeof(Enemy.Strength)));
            //}

            ////Unlock max cargo space
            //if (binds.GetInputDown(binds.bindCheat2))
            //{
            //    //Max cargo space
            //    upgradeLevels[control.commerce.UPGRADE_CARGO_SPACE] = int.Parse(control.commerce.upgradeDictionary[control.commerce.UPGRADE_CARGO_SPACE, control.commerce.UPGRADE_MAX_LEVEL]);
            //}

            //Unlock seismic charges
            if (binds.GetInputDown(binds.bindCheat2))
            {
                upgradeLevels[control.commerce.UPGRADE_SEISMIC_CHARGES] = 1;
                weaponSlot1 = weaponSeismicCharges;
            }

            //if (binds.GetInputDown(binds.bindCheat2))
            //{
            //    tutorialLevel = 11;
            //}

            //if (binds.GetInputDown(binds.bindCheat2))
            //{
            //    for (int i = 0; i < control.generation.planets.transform.childCount; i++)
            //    {
            //        //Planet planetScript = control.generation.planets.transform.GetChild(i).GetComponent<Planet>();
            //        //HeighlinerEntry heighlinerScript = planetScript.heighliner0.GetComponentInChildren<HeighlinerEntry>();
            //        //heighlinerScript.mapLineModel.transform.Rotate(0f, 10f, 0f);
            //        //heighlinerScript.mapLineModel.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            //        //heighlinerScript.mapLineModel.transform.rotation = heighlinerScript.mapLineModel.transform.parent.rotation;
            //    }
            //}

            //if (binds.GetInputDown(binds.bindCheat2))
            //{
            //    for (int i = 0; i < control.generation.planets.transform.childCount; i++)
            //    {
            //        GameObject planetInstanceHeighliner0 = control.generation.planets.transform.GetChild(i).GetComponent<Planet>().heighliner0;
            //        Vector3 planetInstanceHeighliner0ExitNodePosition = planetInstanceHeighliner0.GetComponentInChildren<HeighlinerEntry>().exitNode.transform.position;
            //        planetInstanceHeighliner0.transform.rotation = Quaternion.LookRotation(planetInstanceHeighliner0ExitNodePosition);
            //
            //        GameObject planetInstanceHeighliner1 = control.generation.planets.transform.GetChild(i).GetComponent<Planet>().heighliner1;
            //        Vector3 planetInstanceHeighliner1ExitNodePosition = planetInstanceHeighliner1.GetComponentInChildren<HeighlinerEntry>().exitNode.transform.position;
            //        planetInstanceHeighliner1.transform.rotation = Quaternion.LookRotation(planetInstanceHeighliner1ExitNodePosition);
            //    }
            //}

            //if (binds.GetInputDown(binds.bindCheat2))
            //{
            //    transform.rotation = Quaternion.LookRotation(control.generation.planets.transform.GetChild(0).position);
            //}

            ////Unlock double blowback
            //if (binds.GetInputDown(binds.bindCheat2))
            //{
            //    upgradeLevels[control.commerce.UPGRADE_FIRERATE]++;
            //}

            ////Unlock refinery
            //if (binds.GetInputDown(binds.bindCheat2))
            //{
            //    upgradeLevels[control.commerce.UPGRADE_REFINERY]++;
            //}

            ////Unlock thrust acceleration
            //if (binds.GetInputDown(binds.bindCheat2))
            //{
            //    upgradeLevels[control.commerce.UPGRADE_ACCELERATION]++;
            //}

            ////Give currency
            //if (binds.GetInputDown(binds.bindCheat2))
            //{
            //    currency += 1000;
            //    control.ui.UpdateAllPlayerResourcesUI();
            //}

            ////Spawn asteroid from pool
            //if (binds.GetInputDown(binds.bindCheat1))
            //{
            //    control.generation.AsteroidPoolSpawn(
            //        transform.position + (transform.forward * 10f),
            //        UnityEngine.Random.Range(0, Asteroid.SIZE_LENGTH),
            //        Asteroid.GetRandomType()
            //    );
            //}

            ////Print out asteroid pool bulk actions and reset counts
            //if (binds.GetInputDown(binds.bindCheat2))
            //{
            //    Debug.Log("------------------------------------------");
            //    Debug.Log("Asteroid.SetPoolStatus(): " + control.generation.callsAsteroidSetPoolStatus);
            //    Debug.Log("Asteroid.SetPoolStatus(false): " + control.generation.callsAsteroidSetPoolStatusFalse);
            //    Debug.Log("Generation.AsteroidPoolSpawnCluster(): " + control.generation.callsAsteroidPoolSpawnCluster);
            //    Debug.Log("Generation.AsteroidPoolSpawn(): " + control.generation.callsAsteroidPoolSpawn);
            //    Debug.Log("Asteroid.SetPoolStatus(true): " + control.generation.callsAsteroidSetPoolStatusTrue);
            //    Debug.Log("Asteroid.SetPoolStatus(true).transform.parent = Generation.asteroidsEnabled: " + control.generation.countAsteroidsPutInEnabledTree);
            //    Debug.Log("Generation.asteroidsEnabled.transform.childCount: " + control.generation.asteroidsEnabled.transform.childCount);
            //    control.generation.callsAsteroidSetPoolStatus = 0;
            //    control.generation.callsAsteroidSetPoolStatusFalse = 0;
            //    control.generation.callsAsteroidPoolSpawnCluster = 0;
            //    control.generation.callsAsteroidPoolSpawn = 0;
            //    control.generation.callsAsteroidSetPoolStatusTrue = 0;
            //    control.generation.countAsteroidsPutInEnabledTree = 0;
            //}

            ////Delete all inactive asteroids (really doesn't make much difference in performance at all!)
            //if (binds.GetInputDown(binds.bindCheat2))
            //{
            //    //int inactiveAsteroids = control.generation.asteroidsDetailed - control.generation.asteroidsEnabled.transform.childCount;
            //    for (int i = 0; i < control.generation.asteroidsEnabled.transform.childCount; i++)
            //    {
            //        Transform asteroidToDestroy = control.generation.asteroidsEnabled.transform.GetChild(i);
            //        if (asteroidToDestroy.GetComponent<Asteroid>().performantMode)
            //        {
            //            asteroidToDestroy.GetComponent<Asteroid>().BeginDestroying();
            //        }
            //    }
            //}

            ////Force tutorial level
            //if (binds.GetInputDown(binds.bindCheat2))
            //{
            //    tutorialLevel = 2;
            //}

            //Spawn ore
            //if (binds.GetInputDown(binds.bindCheat2))
            //{
            //    int oreCountToSpawn = 50;
            //    for (int i = 0; i < oreCountToSpawn; i++)
            //    {
            //        control.generation.OrePoolSpawn(
            //            transform.position + (10f * transform.forward) + (0.8f * new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value)),
            //            Asteroid.TYPE_PRECIOUS_METAL,
            //            rb.velocity
            //        );
            //    }
            //}
        }


        //Spawn
        //Press O to spawn asteroid
        //if (binds.GetInputDown(binds.bindCheat2))
        //{
        //    control.generation.SpawnAsteroidManually(
        //        transform.position + transform.forward * 3f,
        //        rb.velocity,
        //        CBodyAsteroid.GetRandomSize(),
        //        CBodyAsteroid.GetRandomType(),
        //        CBodyAsteroid.HEALTH_MAX
        //    );
        //    control.ui.SetTip("Spawned one asteroid.");
        //    upgradeLevels[control.commerce.UPGRADE_SEISMIC_CHARGES] = 1;
        //    control.ui.SetTip("Seismic charges unlocked.");
        //}

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

        //Very slow update (once per second)
        if (Time.frameCount % control.settings.targetFPS == 0)
        {
            VerySlowUpdate();
        }

        //Have the position mount follow the player position
        positionMount.transform.position = transform.position;

        //Setup the camera
        if (!fovSet)
        {
            UpdateCameraSettings();
        }

        //AUDIO
        UpdateAudio();

        //Don't run if paused
        if (!Menu.menuOpenAndGamePaused)
        {
            UpdateGetIfMoving();            //Check if moving at all so that it only has to be checked once per update
            UpdatePlayerWeapons();          //Shoot stuff

            UpdatePlayerEngineEffect();     //Set engine glow relative to movement

            //Engine temporarily disabled or flickering
            if (tempEngineDisable > 0f)
            {
                tempEngineDisableCurrentDuration += Time.deltaTime;
                tempEngineDisable = Mathf.Max(0f, tempEngineDisable - Time.deltaTime);

                if (tempEngineDisableCurrentDuration > tempEngineDisablePeriodToPossiblyFlicker)
                {
                    tempEngineDisableButFlickering = (UnityEngine.Random.value <= tempEngineDisableFlickerChance);
                    tempEngineDisableCurrentDuration = 0f;
                }
            }
            else
            {
                tempEngineDisableCurrentDuration = 0f;
                tempEngineDisableButFlickering = false;
            }

            //Fuel decrement
            if (canAndIsMoving)
            {
                vitalsFuel = Math.Max(0.0d, vitalsFuel - ((vitalsFuelConsumptionRate / (1.0d + (upgradeLevels[control.commerce.UPGRADE_FUEL_EFFICIENCY] * 0.33333333d))) * Time.deltaTime));
            }

            //Fuel increment (in-situ refinery)
            bool missingEnoughFuel = vitalsFuel < vitalsFuelMax - REFINERY_FUEL_OUT_RATE;
            bool hasUpgrade = upgradeLevels[control.commerce.UPGRADE_REFINERY] >= 1;
            bool hasEnoughOre = ore[(int)Asteroid.Type.water] >= REFINERY_ORE_WATER_IN_RATE;
            bool enoughTimeHasPassed = Time.time > refineryTimeAtLastRefine + REFINERY_TIME_BETWEEN_REFINES;
            if (missingEnoughFuel && hasUpgrade && hasEnoughOre && enoughTimeHasPassed && control.settings.refine)
            {
                ore[(int)Asteroid.Type.water] -= REFINERY_ORE_WATER_IN_RATE;
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

            //Collision immunity wears off
            float collisionImmunityPeriod = 0.25f; //Time in seconds the player will be immune for
            float collisionImmunityDecrementRate = Time.deltaTime / collisionImmunityPeriod;
            collisionDamageInvulnerabilityTemporary = Mathf.Max(0f, collisionDamageInvulnerabilityTemporary - collisionImmunityDecrementRate);

            //Recent weapon use movement penalty wears off
            float weaponUsedRecentlyPeriod = 0.25f; //Time in seconds the player will be immune for
            float weaponUsedRecentlyDecrementRate = Time.deltaTime / weaponUsedRecentlyPeriod;
            weaponUsedRecently = Mathf.Max(0f, weaponUsedRecently - weaponUsedRecentlyDecrementRate);

            //Show death reason constantly if dead
            if (isDestroyed)
            {
                //control.ui.SetTip("Your ship was destroyed, and you have died.\nCause: " + lastDamageCause);
                //control.ui.SetTip("You died.\nLast recorded warning message:\n" + lastDamageCause);
                control.ui.SetTip("Your ship has been destroyed, and you have died\nLast recorded warning: \"" + lastDamageCause + "\"");
            }
            
            //Map model
            if (UI.displayMap)
            {
                Transform mapModel = transform.parent.Find("Position Mount").Find("Centre Mount").Find("Ship Map Model");

                //Map model follows player's position exactly, but renders above everything else
                mapModel.position = new Vector3(transform.position.x, mapCam.transform.position.y - 200f, transform.position.z);
            }

            //ABILITIES
            UpdateAbilityEclipseVision();
            UpdateAbilityFlashlight();

            //Tutorial
            UpdateTutorial();

            //Tips
            //Outline - show until player uses it
            if (tipHasBoughtOutline && !tipHasUsedOutline)
            {
                control.ui.SetTip(
                    "Toggle eclipse vision with " + binds.GetBindAsPrettyString(control.binds.bindToggleOutline, true)
                );

                if (binds.GetInputDown(binds.bindToggleOutline))
                {
                    tipHasUsedOutline = true;
                }
            }
            //Seismic charges - show until player uses it
            if (tipHasBoughtSeismicCharges && !tipHasUsedSeismicCharges)
            {
                if (weaponSlotSelected == 0)
                {
                    control.ui.SetTip(
                        "Select seismic charges with " + binds.GetBindAsPrettyString(control.binds.bindSelectWeaponSlot1, true)
                    );
                }
                else
                {
                    control.ui.SetTip(
                        "Fire a seismic charge with " + binds.GetBindAsPrettyString(control.binds.bindPrimaryFire, true)
                    );
                }

                //Used flag toggled in weapons
            }
            //Refinery
            if (tipHasBoughtRefinery && !tipHasUsedRefinery)
            {
                control.ui.SetTip(
                    "The in-situ fuel refinery processes water ice from your cargo bay directly into usable fuel"
                    + "\nYou must have water ice in your cargo bay and the refinery toggled on for it to work"
                    + "\nToggle refinery with " + binds.GetBindAsPrettyString(control.binds.bindToggleRefine, true)
                );

                if (binds.GetInputDown(binds.bindToggleRefine))
                {
                    tipHasUsedRefinery = true;
                    control.menu.SetTipRefineState();
                }
            }


            //Camera shake
            //Generate actual offset
            cameraOffsetPosition = new Vector3(
                cameraOffsetPositionMagnitude.x * ((UnityEngine.Random.value * 2f) - 1f),
                cameraOffsetPositionMagnitude.y * ((UnityEngine.Random.value * 2f) - 1f),
                cameraOffsetPositionMagnitude.z * ((UnityEngine.Random.value * 2f) - 1f)
            );
            cameraOffsetRotationPitch = cameraOffsetRotationMagnitudePitch * ((UnityEngine.Random.value * 2f) - 1f);
            cameraOffsetRotationYaw = cameraOffsetRotationMagnitudeYaw * ((UnityEngine.Random.value * 2f) - 1f);

            //Reduce shaking over time
            float cameraOffsetPositionSmoothingMultiplier = CAMERA_OFFSET_POSITION_SMOOTHING_RATE;
            cameraOffsetPositionMagnitude = new Vector3(
                cameraOffsetPositionMagnitude.x * cameraOffsetPositionSmoothingMultiplier,
                cameraOffsetPositionMagnitude.y * cameraOffsetPositionSmoothingMultiplier,
                cameraOffsetPositionMagnitude.z * cameraOffsetPositionSmoothingMultiplier
            );
            float cameraOffsetRotationSmoothingMultiplier = CAMERA_OFFSET_ROTATION_SMOOTHING_RATE;
            cameraOffsetRotationMagnitudePitch *= cameraOffsetRotationSmoothingMultiplier;
            cameraOffsetRotationMagnitudeYaw *= cameraOffsetRotationSmoothingMultiplier;

            //Target ghost
            if (
                targetObject != null
                && (
                    targetObject.name == control.generation.enemy.name + "(Clone)"
                    || targetObject.name == control.generation.asteroid.name + "(Clone)"
                )
            )
            {
                //Check to make sure the target isn't in the process of destroying
                bool enable = false;
                if (targetObject.name == control.generation.enemy.name + "(Clone)")
                {
                    if (!targetObject.GetComponentInChildren<Enemy>().isDestroying && !targetObject.GetComponentInChildren<Enemy>().isDestroyed)
                    {
                        enable = true;
                    }
                }
                else if (targetObject.name == control.generation.asteroid.name + "(Clone)")
                {
                    if (!targetObject.GetComponentInChildren<Asteroid>().isDestroying && !targetObject.GetComponentInChildren<Asteroid>().isDestroyed)
                    {
                        enable = true;
                    }
                }

                //Set target ghost position
                if (enable)
                {
                    targetObjectLeadGhost.SetActive(true);
                    control.ui.targetObjectLeadGhostImage.gameObject.SetActive(true);

                    //Determine last force added for calculating target acceleration
                    Vector3 targetLastForceAdded = Vector3.zero;
                    if (targetObject.name == control.generation.enemy.name + "(Clone)")
                    {
                        targetLastForceAdded = targetObject.GetComponentInChildren<Enemy>().lastForceAdded;
                    }

                    //Position the target ghost in the trajectory of the target
                    targetObjectLeadGhost.transform.position = control.GetLeadPosition(
                        transform.position, rb.velocity, lastForceAdded, rb.mass,
                        weaponSelectedProjectileSpeed, 1f,
                        targetObject.transform.position, targetObject.GetComponentInChildren<Rigidbody>().velocity, targetLastForceAdded,
                        targetObject.GetComponentInChildren<Rigidbody>().mass,
                        false
                    );

                    //Align 2D UI element
                    control.ui.targetObjectLeadGhostImage.transform.position = control.ui.Get2DUICoordsFrom3DWorldPosition(
                        targetObjectLeadGhost.transform.position,
                        control.ui.targetObjectLeadGhostImage,
                        false
                    );
                }
                else
                {
                    //Disable
                    targetObjectLeadGhost.SetActive(false);
                    control.ui.targetObjectLeadGhostImage.gameObject.SetActive(false);
                }
            }
            else
            {
                //Disable
                targetObjectLeadGhost.SetActive(false);
                control.ui.targetObjectLeadGhostImage.gameObject.SetActive(false);
            }

            //Optical Zoom
            if (binds.GetInput(binds.bindCameraZoomFOV))
            {
                cameraZoom = 0.5f;
                UpdateCameraSettings();
            }
            else
            {
                cameraZoom = 1f;
                UpdateCameraSettings();
            }
        }
    }

    private void UpdateAbilityFlashlight()
    {
        //Energy and cooldown data
        //Cooldown period
        float timePastDisableMoment = SPOTLIGHT_PERIOD_COOLDOWN;
        if (spotlightEnergyRemaining <= 0f)
        {
            timePastDisableMoment = Time.time - spotlightEnergyDepletedTime;
        }
        //canUse
        bool canUse = spotlightEnergyRemaining > 0f || timePastDisableMoment >= SPOTLIGHT_PERIOD_COOLDOWN;
        //Cooldown UI
        control.ui.abilitySpotlight.Find("Mana").GetComponent<Image>().fillAmount = Mathf.Clamp01(1f - (spotlightEnergyRemaining / SPOTLIGHT_PERIOD_ENABLED_MAX));
        control.ui.abilitySpotlight.Find("Cooldown").GetComponent<Image>().fillAmount = Mathf.Clamp01(1f - (timePastDisableMoment / SPOTLIGHT_PERIOD_COOLDOWN));

        //Keybind
        if (binds.GetInputDown(binds.bindToggleSpotlight))
        {
            if (!spotlightIsOn && canUse)
            {
                spotlightIsOn = true;
            }
            else
            {
                spotlightIsOn = false;
            }

            DecideWhichModelsToRenderSpotlight();
        }

        //Energy and cooldown logic
        float flickerFrameIntervalFloat = control.settings.targetFPS / 53f;
        flickerFrameIntervalFloat *= 1f + (0.25f * control.GetRandomNeg1ToPos1());
        int flickerFrameInterval = Mathf.RoundToInt(flickerFrameIntervalFloat);
        if (spotlightIsFlickeredOff && Time.frameCount % flickerFrameInterval == 0)
        {
            spotlightIsFlickeredOff = false;
            DecideWhichModelsToRenderSpotlight();
        }
        if (spotlightIsOn)
        {
            //Decrement
            spotlightEnergyRemaining = Mathf.Max(0f, spotlightEnergyRemaining - Time.deltaTime);

            //Out of energy - turn off and begin cooldown period
            if (spotlightEnergyRemaining <= 0f)
            {
                spotlightEnergyDepletedTime = Time.time;
                spotlightIsOn = false;
                DecideWhichModelsToRenderSpotlight();
            }
            else if (spotlightEnergyRemaining <= 1f) //(spotlightEnergyRemaining / SPOTLIGHT_PERIOD_ENABLED_MAX <= 0.18f)
            {
                //Flickering
                if (
                    control.settings.spotlightFlicker
                    && Time.frameCount % flickerFrameInterval == 0
                    && UnityEngine.Random.value >= 0.7f
                )
                {
                    spotlightIsFlickeredOff = true;
                    DecideWhichModelsToRenderSpotlight();
                }
            }
        }
        else
        {
            //Increment
            if (canUse)
            {
                //Recharge when not on
                //If completely exhausted energy, only begin recharging after cooldown period is up
                spotlightEnergyRemaining = Mathf.Min(SPOTLIGHT_PERIOD_ENABLED_MAX, spotlightEnergyRemaining + Time.deltaTime);
            }
        }
    }

    private void UpdateAbilityEclipseVision()
    {
        float timeUntilCanUse = Time.time - (outlineActivatedTime + OUTLINE_PERIOD_ENABLED + OUTLINE_PERIOD_COOLDOWN);

        //Cooldown UI
        float timePastDisableMoment = Time.time - (outlineActivatedTime + OUTLINE_PERIOD_ENABLED);
        float remainingCDFraction = timePastDisableMoment / OUTLINE_PERIOD_COOLDOWN;
        control.ui.abilityEclipseVision.Find("Cooldown").GetComponent<Image>().fillAmount = Mathf.Max(0f, Mathf.Min(1f,
            1f - remainingCDFraction
        ));

        //Visible? (Including faded)
        if (Time.time < outlineActivatedTime + OUTLINE_PERIOD_ENABLED)
        {
            //Outline active
            if (
                outlineFade > 0f
                && isOutlinesVisible
            //&& Time.time >= outlineActivatedTime + OUTLINE_PERIOD_ENABLED - OUTLINE_PERIOD_FADING_WHILE_ENABLED
            )
            {
                //Fade outlines out over time
                outlineFade = Mathf.Max(0f, (outlineActivatedTime + OUTLINE_PERIOD_ENABLED - OUTLINE_PERIOD_FADING_WHILE_ENABLED - Time.time) / OUTLINE_PERIOD_FADING_WHILE_ENABLED);
                UpdateOutlines();
            }
            else
            {
                //Reset fade
                outlineFade = 1f;

                if (isOutlinesVisible)
                {
                    //Turn off
                    ToggleOutline();
                }
            }
        }

        //Toggle
        if (
            binds.GetInputDown(binds.bindToggleOutline)
            && upgradeLevels[control.commerce.UPGRADE_OUTLINE] >= 1
        )
        {
            //Can only turn on when cooldown allows it, but can turn off at any time
            if (
                timeUntilCanUse >= 0f
                || isOutlinesVisible
            )
            {
                ToggleOutline();
            }
        }
    }

    public void CameraShakeAdd(Vector2 shake)
    {
        //Position
        cameraOffsetPositionMagnitude = new Vector3(
            Mathf.Min(CAMERA_OFFSET_POSITION_MAGNITUDE_MAX, cameraOffsetPositionMagnitude.x + shake.x),
            Mathf.Min(CAMERA_OFFSET_POSITION_MAGNITUDE_MAX, cameraOffsetPositionMagnitude.y + shake.x),
            Mathf.Min(CAMERA_OFFSET_POSITION_MAGNITUDE_MAX, cameraOffsetPositionMagnitude.z + shake.x)
        );

        if (shake.x > CAMERA_OFFSET_POSITION_MAGNITUDE_MAX)
        {
            Debug.Log("Positional camera shake maxed out");
        }

        //Rotation
        cameraOffsetRotationMagnitudePitch = Mathf.Min(CAMERA_OFFSET_ROTATION_MAGNITUDE_MAX, cameraOffsetRotationMagnitudePitch + shake.y);
        cameraOffsetRotationMagnitudeYaw =   Mathf.Min(CAMERA_OFFSET_ROTATION_MAGNITUDE_MAX, cameraOffsetRotationMagnitudeYaw +   shake.y);

        if (shake.y > CAMERA_OFFSET_ROTATION_MAGNITUDE_MAX)
        {
            Debug.Log("Rotational camera shake maxed out");
        }
    }

    private void UpdateTutorial()
    {
        //Some of tutorial is in SlowUpdate()

        //Tutorial
        if (!isDestroyed && control.settings.tutorial && tutorialTime <= Time.time)
        {
            string confirmedInputColor = "#ffff00ff"; //yellow

            //Sequential tutorial items
            if (tutorialLevel == 0)
            {
                string forward;
                if (tutorialHasPressedForward) {
                    forward = "<color=" + confirmedInputColor + ">" + binds.GetBindAsPrettyString(binds.bindThrustForward, true) + "</color>";
                }
                else
                {
                    forward = binds.GetBindAsPrettyString(binds.bindThrustForward, true);
                }

                string left;
                if (tutorialHasPressedLeft)
                {
                    left = "<color=" + confirmedInputColor + ">" + binds.GetBindAsPrettyString(binds.bindThrustLeft, true) + "</color>";
                }
                else
                {
                    left = binds.GetBindAsPrettyString(binds.bindThrustLeft, true);
                }

                string backward;
                if (tutorialHasPressedBackward)
                {
                    backward = "<color=" + confirmedInputColor + ">" + binds.GetBindAsPrettyString(binds.bindThrustBackward, true) + "</color>";
                }
                else
                {
                    backward = binds.GetBindAsPrettyString(binds.bindThrustBackward, true);
                }

                string right;
                if (tutorialHasPressedRight)
                {
                    right = "<color=" + confirmedInputColor + ">" + binds.GetBindAsPrettyString(binds.bindThrustRight, true) + "</color>";
                }
                else
                {
                    right = binds.GetBindAsPrettyString(binds.bindThrustRight, true);
                }

                string down;
                if (tutorialHasPressedDown)
                {
                    down = "<color=" + confirmedInputColor + ">" + binds.GetBindAsPrettyString(binds.bindThrustDown, true) + "</color>";
                }
                else
                {
                    down = binds.GetBindAsPrettyString(binds.bindThrustDown, true);
                }

                string up;
                if (tutorialHasPressedUp)
                {
                    up = "<color=" + confirmedInputColor + ">" + binds.GetBindAsPrettyString(binds.bindThrustUp, true) + "</color>";
                }
                else
                {
                    up = binds.GetBindAsPrettyString(binds.bindThrustUp, true);
                }

                control.ui.SetTip(
                    "Fly with " + forward + ", " + left + ", " + backward + ", " + right + ", " + down + ", and " + up,
                    0f
                );

                //Show this tutorial tip until the player demonstrates understanding
                if (binds.GetInput(binds.bindThrustForward))    { tutorialHasPressedForward = true; }
                if (binds.GetInput(binds.bindThrustLeft))       { tutorialHasPressedLeft = true; }
                if (binds.GetInput(binds.bindThrustBackward))   { tutorialHasPressedBackward = true; }
                if (binds.GetInput(binds.bindThrustRight))      { tutorialHasPressedRight = true; }
                if (binds.GetInput(binds.bindThrustDown))       { tutorialHasPressedDown = true; }
                if (binds.GetInput(binds.bindThrustUp))         { tutorialHasPressedUp = true; }
                if (
                       tutorialHasPressedForward
                    && tutorialHasPressedLeft
                    && tutorialHasPressedBackward
                    && tutorialHasPressedRight
                    && tutorialHasPressedDown
                    && tutorialHasPressedUp
                )
                {
                    IncrementTutorial(0f);
                }
            }
            else if (tutorialLevel == 1)
            {
                control.ui.SetTip(
                    "You can skip this tutorial in Menu > Settings (press " + binds.GetBindAsPrettyString(binds.bindToggleMenu, true) + "), if you desire",
                    TUTORIAL_TIP_DURATION * 0.6f
                );
                IncrementTutorial(TUTORIAL_TIP_DURATION * 0.8f);
            }
            else if (tutorialLevel == 2)
            {
                control.ui.SetTip(
                    "If you forget a keybind or wish to change one, go to Menu > Keybinds (press " + binds.GetBindAsPrettyString(binds.bindToggleMenu, true) + ")",
                    TUTORIAL_TIP_DURATION
                );
                IncrementTutorial(TUTORIAL_TIP_DURATION);
            }
            else if (tutorialLevel == 3)
            {
                control.ui.SetTip(
                    "Mine asteroids for valuable materials\nFire your weapon with " + binds.GetBindAsPrettyString(binds.bindPrimaryFire, true),
                    0f
                );

                //Make the target (forced to the nearest asteroid) glow
                TargetGlow();

                //Show this tutorial tip until the player demonstrates understanding
                if (tutorialHasMinedAsteroid)
                {
                    //Because we forced an outline on the nearest asteroid, we now need to update all outlines before moving on
                    UpdateOutlines();
                    //Same for the target image colour
                    TargetReset();

                    IncrementTutorial(0f);
                }
            }
            else if (tutorialLevel == 4)
            {
                control.ui.SetTip(
                    "Collect the ore by moving close enough for your tractor beam to activate",
                    0f
                );

                //Show this tutorial tip until the player demonstrates understanding
                if (tutorialHasCollectedOre)
                {
                    IncrementTutorial(0f);
                }
            }
            else if (tutorialLevel == 5)
            {
                control.ui.SetTip(
                    "Sell your cargo at space stations to afford fuel, repairs, and upgrades",
                    0f
                );

                //Make the target (forced to the nearest space station) glow
                TargetGlow();

                //Show this tutorial tip until the player demonstrates understanding
                if (tutorialHasUsedStation)
                {
                    //Reset the colour of the target because we made it glow earlier
                    TargetReset();

                    //Reset target to null
                    control.ui.SetPlayerTargetObject(targetObject);

                    //Console
                    control.ui.UpdateTargetConsole();

                    IncrementTutorial(0f);
                }
            }
            else if (tutorialLevel == 6)
            {
                control.ui.SetTip(
                    "Targetting destructible objects displays a trajectory reticle to assist aiming"
                    + "\nThis is especially helpful when mining or in combat"
                    + "\nTarget what you're looking at with " + binds.GetBindAsPrettyString(binds.bindSetTarget, true),
                    0f
                );

                if (targetObject != null)
                {
                    if (targetObject.name != control.generation.station.name)
                    {
                        IncrementTutorial(0f);
                    }
                }
            }
            else if (tutorialLevel == 7)
            {
                control.ui.SetTip(
                    //"Different asteroid types may be found around other celestial bodies, but beware:\nbandits may be looking to steal your cargo, your ship, and your life\nGood luck, pilot",
                    "Explore the system to discover more valuable asteroids"
                    + "\nVisit new stations for varying ore prices and upgrade offers"
                    + "\nBut beware: bandits may be looking to steal your cargo and your life!"
                    + "\nGood luck, pilot",
                    TUTORIAL_TIP_DURATION * 3f
                );
                IncrementTutorial();
            }
            else if (tutorialLevel == 8 && tutorialMoonVisitedID1 != -1 && tutorialMoonVisitedID2 != -1) //visited 2 moons
            {
                control.ui.SetTip(
                    "Zoom in/out with " + binds.GetBindAsPrettyString(binds.bindCameraZoomFollowDistIn, true) + "/" + binds.GetBindAsPrettyString(binds.bindCameraZoomFollowDistOut, true),
                    0f
                );

                if (binds.GetInputDown(binds.bindCameraZoomFollowDistIn))
                {
                    tutorialHasZoomedIn = true;
                }
                if (binds.GetInputDown(binds.bindCameraZoomFollowDistOut))
                {
                    tutorialHasZoomedOut = true;
                }

                if (tutorialHasZoomedIn || tutorialHasZoomedOut)
                {
                    IncrementTutorial(0f);
                }
            }
            else if (tutorialLevel == 9)
            {
                control.ui.SetTip(
                    "Third-person can be useful in combination with free-look " + binds.GetBindAsPrettyString(binds.bindCameraFreeLook, true) + "\nThis allows you to see beside & behind your ship while travelling through the void\nBut first-person is best for mining and for combat",
                    TUTORIAL_TIP_DURATION
                );

                if (binds.GetInputDown(binds.bindCameraFreeLook))
                {
                    IncrementTutorial(TUTORIAL_TIP_DURATION);
                }
            }
            else if (tutorialLevel == 10 && tutorialMoonVisitedID1 != -1 && tutorialMoonVisitedID2 != -1 && tutorialMoonVisitedID3 != -1) //visited 3 moons
            {
                //Hasn't visited a heighliner, and has visited several moons
                control.ui.SetTip(
                    "Open your map with " + binds.GetBindAsPrettyString(binds.bindToggleMap, true),
                    0f
                );

                //Show this tutorial tip until the player demonstrates understanding
                if (binds.GetInput(binds.bindToggleMap))
                {
                    IncrementTutorial(0f);
                }
            }
            else if (tutorialLevel == 11)
            {
                if (UI.displayMap)
                {
                    string targetString;
                    if (tutorialHasPressedSetTarget)
                    {
                        targetString = "You can use the map to set targets too. Hover over a celestial body and press <color=" + confirmedInputColor + ">" + binds.GetBindAsPrettyString(binds.bindSetTarget, true) + "</color>";
                    }
                    else
                    {
                        targetString = "You can use the map to set targets too. Hover over a celestial body and press " + binds.GetBindAsPrettyString(binds.bindSetTarget, true);
                    }

                    string zoomInString;
                    if (tutorialHasPressedZoomInInMap)
                    {
                        zoomInString = "\nZoom in/out with <color=" + confirmedInputColor + ">" + binds.GetBindAsPrettyString(binds.bindCameraZoomFollowDistIn, true) + "</color>";
                    }
                    else
                    {
                        zoomInString = "\nZoom in and out with " + binds.GetBindAsPrettyString(binds.bindCameraZoomFollowDistIn, true);
                    }

                    string zoomOutString;
                    if (tutorialHasPressedZoomOutInMap)
                    {
                        zoomOutString = "<color=" + confirmedInputColor + ">" + binds.GetBindAsPrettyString(binds.bindCameraZoomFollowDistOut, true) + "</color>";
                    }
                    else
                    {
                        zoomOutString = binds.GetBindAsPrettyString(binds.bindCameraZoomFollowDistOut, true);
                    }

                    string panMapString;
                    if (tutorialHasPressedPanMap)
                    {
                        panMapString = "\nPan around the map with <color=" + confirmedInputColor + ">" + binds.GetBindAsPrettyString(binds.bindPanMap, true) + "</color>";
                    }
                    else
                    {
                        panMapString = "\nPan around the map with " + binds.GetBindAsPrettyString(binds.bindPanMap, true);
                    }

                    control.ui.SetTip(
                        targetString
                        + zoomInString + " and " + zoomOutString
                        + panMapString,
                        TUTORIAL_TIP_DURATION
                    );

                    //Show this tutorial tip until the player demonstrates understanding
                    if (binds.GetInput(binds.bindSetTarget)) { tutorialHasPressedSetTarget = true; }
                    if (binds.GetInput(binds.bindCameraZoomFollowDistIn)) { tutorialHasPressedZoomInInMap = true; }
                    if (binds.GetInput(binds.bindCameraZoomFollowDistOut)) { tutorialHasPressedZoomOutInMap = true; }
                    if (binds.GetInput(binds.bindPanMap)) { tutorialHasPressedPanMap = true; }
                    if (
                           tutorialHasPressedSetTarget
                        && tutorialHasPressedZoomInInMap
                        && tutorialHasPressedZoomOutInMap
                        && tutorialHasPressedPanMap
                    )
                    {
                        IncrementTutorial(TUTORIAL_TIP_DURATION);
                    }
                }
                else
                {
                    //If the player closes their map without looking, explain again
                    control.ui.SetTip(
                        "Open your map with " + binds.GetBindAsPrettyString(binds.bindToggleMap, true),
                        0f
                    );
                }
            }
            else if (tutorialLevel == 12)
            {
                if (!tutorialHasUsedHeighliner)
                {
                    control.ui.SetTip(
                        "Travel to neighbouring planetary systems via heighliners\nFind them in orbit around planets",
                        TUTORIAL_TIP_DURATION + 2f
                    );
                }

                IncrementTutorial();
            }
            else if (tutorialLevel == 13 && tutorialHasUsedHeighliner)
            {
                control.ui.SetTip(
                    "Going through heighliners updates your map.\n(Open map with " + binds.GetBindAsPrettyString(binds.bindToggleMap, true) + ")",
                    TUTORIAL_TIP_DURATION + 2f
                );

                //Tutorial complete; don't show it next time
                control.settings.tutorial = false;

                IncrementTutorial();
            }
        }
    }

    private void TargetGlow()
    {
        //Make the target glow
        float period = 0.3f; //0.6f;
        float intensity = (Time.time % period) * (2f / period);
        //Loop back and forth smoothly (this is also why we multiply by 2f instead of 1f above)
        intensity = Mathf.Abs(1f - intensity);
        //Apply the glow to the sprite colour
        //control.ui.targetImage.color = new Color(1f, 1f, 0f, intensity);
        control.ui.canvas.transform.Find("HUD Centre").Find("Waypoint").Find("Target").Find("Target Tutorial").gameObject.SetActive(true);
        control.ui.canvas.transform.Find("HUD Centre").Find("Waypoint").Find("Target").Find("Target Tutorial").GetComponent<Image>().color = new Color(1f, 1f, 0f, intensity);
    }

    public void TargetReset()
    {
        //Same for the target image colour
        //control.ui.targetImage.color = new Color(1f, 1f, 1f, 0.36078431372f);

        control.ui.canvas.transform.Find("HUD Centre").Find("Waypoint").Find("Target").Find("Target Tutorial").gameObject.SetActive(false);
    }

    private void IncrementTutorial()
    {
        tutorialLevel++;
        tutorialTime = Time.time + TUTORIAL_DELAY + TUTORIAL_TIP_DURATION + 1f; //Add a minimum delay before displaying next tip
    }
    private void IncrementTutorial(float delay)
    {
        tutorialLevel++;
        tutorialTime = Time.time + delay; //Add a minimum delay before displaying next tip
    }

    public double GetTotalOre()
    {
        return ore[(int)Asteroid.Type.platinoid] + ore[(int)Asteroid.Type.preciousMetal] + ore[(int)Asteroid.Type.water];
    }

    private void FixedUpdate()
    {
        //Update every n frames instead of every frame
        if (Time.frameCount % 3 == 0)
        {
            SlowFixedUpdate();
        }

        ////Don't run if paused
        //if (!Menu.menuOpenAndGamePaused)
        //{
        //    UpdatePlayerMovementTorque();   //Automatically torque the ship so that it is always facing "up" relative to the system
        //    UpdatePlayerMovementThrust();   //Move in the direction of player input
        //}

        if (!isDestroyed)
        {
            //Drag the ship relative to either the system or the nearest moon, depending on distance to nearest moon
            UpdatePlayerMovementDrag();
        }
    }

    private void LateUpdate()
    {
        UpdateCameraMovement();         //Make camera follow player at specified distance and height, plus speed feedback

        //Don't run if paused
        if (!Menu.menuOpenAndGamePaused)
        {
            UpdatePlayerMovementTorque();   //Automatically torque the ship so that it is always facing "up" relative to the system
            UpdatePlayerMovementThrust();   //Move in the direction of player input
        }

        //Need to force an update to FP cam AFTER torquing the ship, otherwise it lags
        UpdateFPCamMountPosition();
    }

    private void SlowUpdate()
    {
        control.ui.UpdatePlayerVitalsDisplay();
        UpdateWarningText();

        //Too close to the sun?
        float distToCStar = Vector3.Distance(transform.position, control.generation.instanceStarHome.transform.position);
        float maxDist = 150f;
        float maxBaseDPS = 7f; //max BASE dps BEFORE adding 1 and raising to power
        if (distToCStar < maxDist)
        {
            //Emit particles once per second
            bool emitParticles = false;
            if (Time.frameCount % control.settings.targetFPS == 0)
            {
                emitParticles = true;
            }

            //Damage
            DamagePlayer(
                Math.Max(
                    0d,
                    vitalsHealth - (Math.Pow(((maxDist - distToCStar)/maxDist) * maxBaseDPS, 2d) * Time.deltaTime)
                ),
                "overheat",
                0f,
                (control.generation.instanceStarHome.transform.position - transform.position).normalized,
                emitParticles
            );
        }

        if (control.settings.tutorial)
        {
            //Highlight nearest asteroid
            if (tutorialLevel == 3)
            {
                HighlightNearestAsteroid();
            }
            else if (tutorialLevel == 5)
            {
                //Target nearest space station
                Transform nearestSpaceStation = control.GetClosestSpecificTransformFromHierarchy(
                    control.generation.station.name + "(Clone)",
                    control.generation.moons.transform,
                    transform.position
                );

                control.ui.targetImage.gameObject.SetActive(true);
                control.ui.renderTarget = true;
                targetObject = nearestSpaceStation.gameObject;
                control.ui.UpdateTargetConsole();
            }
        }
    }

    private void VerySlowUpdate()
    {
        //Tutorial
        //Has the player visited a few moons?
        if (tutorialMoonVisitedID1 == -1 || tutorialMoonVisitedID2 == -1 || tutorialMoonVisitedID3 == -1)
        {
            if (distToClosestMoonOrMoonChild <= 60f)
            {
                if (tutorialMoonVisitedID1 == -1)
                { 
                    tutorialMoonVisitedID1 = GetNearestMoonID(); 
                }
                else if (tutorialMoonVisitedID2 == -1 && tutorialMoonVisitedID1 != GetNearestMoonID())
                {
                    tutorialMoonVisitedID2 = GetNearestMoonID();
                }
                else if (tutorialMoonVisitedID3 == -1 && tutorialMoonVisitedID2 != GetNearestMoonID() && tutorialMoonVisitedID1 != GetNearestMoonID())
                {
                    tutorialMoonVisitedID3 = GetNearestMoonID();
                }
            }
        }
    }

    private void HighlightNearestAsteroid()
    {
        //OUTLINE
        //Update outlines before proceeding so that outlines cannot get "sticky"
        UpdateOutlines();

        //Find nearest asteroid
        //Transform nearestAsteroid = control.GetClosestSpecificTransformFromHierarchy(
        //    control.generation.asteroid.name + "(Clone)",
        //    control.generation.asteroidsEnabled.transform,
        //    transform.position
        //);
        //Look for nearest asteroid that is medium or large, AND that isn't clay-silicate
        Transform nearestAsteroid = null;
        //Start with infinity distance away to compare to
        float closestDistanceSoFar = Mathf.Infinity;
        //Loop through all transforms
        Transform hierarchy = control.generation.asteroidsEnabled.transform;
        int nTransformsToCheck = hierarchy.childCount;
        for (int i = 0; i < nTransformsToCheck; i++)
        {
            //The transform that we are currently checking
            Transform transformCurrentlyChecking = hierarchy.GetChild(i);

            //Ensure this transform is one of the specific clones we want to check for
            if (transformCurrentlyChecking.gameObject.name == control.generation.asteroid.name + "(Clone)")
            {
                //Ensure the asteroid is the correct size and not a clay-silicate asteroid
                Asteroid asteroidScript = transformCurrentlyChecking.GetComponentInChildren<Asteroid>();
                if (
                    asteroidScript.type != Asteroid.Type.claySilicate
                    && (
                        asteroidScript.size == Asteroid.Size.medium
                        || asteroidScript.size == Asteroid.Size.large
                    )
                )
                {
                    //The distance from the player to that transform
                    float distanceToTransformToCheck = Vector3.Distance(transform.position, transformCurrentlyChecking.position);

                    //If the distance is closer than the last transform we checked
                    if (distanceToTransformToCheck < closestDistanceSoFar)
                    {
                        //Set this transform as the closest (so far)
                        closestDistanceSoFar = distanceToTransformToCheck;
                        nearestAsteroid = transformCurrentlyChecking;
                    }
                }
            }
        }

        //Highlight that asteroid
        if (!nearestAsteroid.GetComponentInChildren<Asteroid>().isDestroying)
        {
            Material material = nearestAsteroid.GetComponentInChildren<MeshRenderer>().material;
            material.SetFloat("_NightVisionOutline", 5f);
        }

        //TARGET
        //Force the player to target the nearest asteroid
        //control.ui.SetPlayerTargetObject(nearestAsteroid.gameObject);
        control.ui.targetImage.gameObject.SetActive(true);
        control.ui.renderTarget = true;
        targetObject = nearestAsteroid.gameObject;
        control.ui.UpdateTargetConsole();
    }

    private int GetNearestMoonID()
    {
        Transform visitedMoonTransfrom = control.GetClosestSpecificTransformFromHierarchy(
            control.generation.moon.name + "(Clone)",
            control.generation.moons.transform,
            transform.position
        );
        return visitedMoonTransfrom.GetSiblingIndex();
    }

    private void SlowFixedUpdate()
    {
        //If one exists, find the nearest moon or asteroid to determine whether or not to drag relative to it
        if (control.generation.moons.transform.childCount > 0)
        {
            closestMoonOrMoonChildTransform = Control.GetClosestTransformFromHierarchy(control.generation.moons.transform, transform.position);
            distToClosestMoonOrMoonChild = (transform.position - closestMoonOrMoonChildTransform.transform.position).magnitude;

            //Moonar discovery
            if (distToClosestMoonOrMoonChild < 100f)
            {
                if (closestMoonOrMoonChildTransform.gameObject.name == control.generation.moon.name + "(Clone)")
                {
                    //Moon
                    closestMoonOrMoonChildTransform.GetComponent<Moon>().isDiscovered = true;
                }
                else if (closestMoonOrMoonChildTransform.gameObject.name == control.generation.station.name + "(Clone)")
                {
                    //Station
                    closestMoonOrMoonChildTransform.GetComponentInChildren<StationDocking>().GetParentMoonScript().isDiscovered = true;
                }
                //else if (closestMoonOrMoonChildTransform.gameObject.name == control.generation.heighliner.name + "(Clone)")
                //{
                //    //Heighliner
                //    closestMoonOrMoonChildTransform.GetComponentInChildren<HeighlinerEntry>().GetParentPlanetScript().isDiscovered = true;
                //}
            }
        }
        if (control.generation.asteroidsEnabled.transform.childCount > 0)
        {
            //closestAsteroidTransform = Control.GetClosestTransformFromHierarchy(control.generation.asteroidsEnabled.transform, transform.position);

            //MANUALLY GET NEAREST ASTEROID BECAUSE WE NEED TO ENSURE IT IS NOT DESTROYED
            Transform hierarchy = control.generation.asteroidsEnabled.transform;

            //Start with infinity distance away to compare to
            float closestDistanceSoFar = Mathf.Infinity;

            //Loop through all transforms
            int nTransformsToCheck = hierarchy.childCount;
            for (int i = 0; i < nTransformsToCheck; i++)
            {
                //The transform that we are currently checking
                Transform transformToCheck = hierarchy.GetChild(i);
                Asteroid asteroidScript = transformToCheck.GetComponent<Asteroid>();

                //Only accept if not destroyed/destroying, and if not brand new
                if (
                    !asteroidScript.isDestroyed
                    && !asteroidScript.isDestroying
                    && asteroidScript.size != Asteroid.Size.small //never drag relative to small asteroids
                    && Time.time >= asteroidScript.timeDraggableRelative
                )
                {
                    //The distance to this particular asteroid from the player position
                    float distanceToTransformToCheck = Vector3.Distance(transform.position, transformToCheck.position);

                    //If the distance is closer than the last transform we checked
                    if (distanceToTransformToCheck < closestDistanceSoFar)
                    {
                        //Set this transform as the closest (so far)
                        closestDistanceSoFar = distanceToTransformToCheck;
                        closestAsteroidTransformToDragRelativeTo = transformToCheck;
                    }
                }
            }

            //distToClosestAsteroid = (transform.position - closestAsteroidTransform.transform.position).magnitude;
            distToClosestAsteroid = closestDistanceSoFar;
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
            !isDestroyed
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
            && (tempEngineDisable == 0 || tempEngineDisableButFlickering)
            && vitalsFuel > 0.0d
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
        //Subtract brightness over time by default
        engineBrightness = Mathf.Max(0f, engineBrightness - (1.5f * Time.deltaTime));

        //Add to engine brightness if moving (should be run AFTER UpdatePlayerMovementThrust, which is in LateUpdate())
        engineBrightness = Mathf.Max(engineBrightness, Mathf.Min(1f, engineBrightness + (thrustVector.normalized.magnitude / 40f)));

        //Decide colour
        Color engineEmissionColorToChangeTo;
        if (vitalsFuel > 0.0d)
        {
            if (thrustEngineWarmupMultiplier > (thrustEngineWarmupMultiplierMax - 1f) / 2f)
            {
                engineEmissionColorToChangeTo = engineEmissionColorRunningHot;
            }
            else
            {
                engineEmissionColorToChangeTo = engineEmissionColorRunning;
            }
        }
        else
        {
            engineEmissionColorToChangeTo = engineEmissionColorDead;
        }
        float colorChangeRate = 1f * Time.deltaTime; //Time in seconds to change colour
        engineEmissionColor.r += (engineEmissionColorToChangeTo.r - engineEmissionColor.r) * colorChangeRate;
        engineEmissionColor.g += (engineEmissionColorToChangeTo.g - engineEmissionColor.g) * colorChangeRate;
        engineEmissionColor.b += (engineEmissionColorToChangeTo.b - engineEmissionColor.b) * colorChangeRate;

        //Update engine LIGHT brightness
        engineLight.intensity = (1f + engineBrightness) * 1.575f;

        //Update total colour
        //If the colour intensity reaches zero the material can never get bright again for some reason, so we ensure the lowest value is never equal to zero
        engineGlowMat.SetColor("_EmissionColor", engineEmissionColor * engineEmissionIntensity * (1f + (engineBrightness)));
    }

    private void UpdatePlayerMovementDrag()
    {
        /*
         * Drag relative-to-object if possible, otherwise drag relative-to-universe
         * If no fuel, no drag
         * 
         * Which object we drag relative to is based on this hierarchy:
         * - Moons
         * - Asteroids
         * - Target
         * - System/Centre Star
         * 
         * Can set the relative drag to only happen when not moving to allow for more realistic (but less intuitive) acceleration by surrounding this with an if (!moving) check
         */

        if (vitalsFuel > 0.0d && tempEngineDisable <= 0f && control.settings.matchVelocity)
        {
            float dragEffective = DRAG * Mathf.Max(1f, upgradeLevels[control.commerce.UPGRADE_ACCELERATION] * UPGRADE_ACCELERATION_MULTIPLIER); //1.5x more drag after acc upgrade

            if (closestMoonOrMoonChildTransform != null && distToClosestMoonOrMoonChild <= ORBITAL_DRAG_MODE_THRESHOLD)
            {
                //Planetoid-relative drag (we check if the transform is null because planetoids are destructible)
                velocityOfObjectDraggingRelativeTo = closestMoonOrMoonChildTransform.GetComponent<Rigidbody>().velocity;
                rb.velocity = Control.GetVelocityDraggedRelative(rb.velocity, velocityOfObjectDraggingRelativeTo, dragEffective);
            }
            else if (closestAsteroidTransformToDragRelativeTo != null && distToClosestAsteroid <= ORBITAL_DRAG_MODE_THRESHOLD)
            {
                //Asteroid-relative drag (we check if the transform is null because asteroids are destructible)
                velocityOfObjectDraggingRelativeTo = closestAsteroidTransformToDragRelativeTo.GetComponent<Rigidbody>().velocity;
                rb.velocity = Control.GetVelocityDraggedRelative(rb.velocity, velocityOfObjectDraggingRelativeTo, dragEffective);
            }
            //else if (targetObject != null)
            //{
            //    //Target-relative drag
            //    velocityOfObjectDraggingRelativeTo = targetObject.GetComponent<Rigidbody>().velocity;
            //    rb.velocity = Control.GetVelocityDraggedRelative(rb.velocity, velocityOfObjectDraggingRelativeTo, DRAG);
            //}
            else
            {
                //Centre planet-relative drag
                velocityOfObjectDraggingRelativeTo = Vector3.zero;
                rb.velocity *= (1f - (dragEffective * Time.deltaTime));
            }
        }
    }

    private void UpdatePlayerMovementTorque()
    {
        //TODO
        //Don't overshoot: calculate if we have enough time to slow down rotation so we don't overshoot, and when we don't, torque in the opposite direction

        if (vitalsFuel > 0.0d)
        {
            if (tempEngineDisable <= 0f && control.settings.spinStabilizers)
            {
                //Angular drag to bring rotations to rest
                rb.angularDrag = angularDragWhenEnginesOn;
            }
            else
            {
                //Reset angular drag when engines not on
                rb.angularDrag = 0f;
            }

            //When to torque
            if (
                !isDestroyed
                && tempEngineDisable <= 0f
                && !binds.GetInput(binds.bindCameraFreeLook)
                && !Menu.menuOpenAndGamePaused
                && !Commerce.menuOpen
                && (
                    canAndIsMoving
                    || binds.GetInput(binds.bindAlignShipToReticle)
                    || (
                        vitalsFuel > 0.0d
                        && binds.GetInput(binds.bindPrimaryFire)
                    )
                )
            )
            {
                //Angular drag to smooth out torque
                rb.angularDrag = angularDragWhenEnginesOn;

                //Thank you Tobias, Conkex, HiddenMonk, and Derakon
                //https://answers.unity.com/questions/727254/use-rigidbodyaddtorque-with-quaternions-or-forward.html

                //TORQUE DRIECTION
                //Vector of where the camera is pointing
                //Vector3 shipRelativeToCamera = transform.position - tpCamMount.transform.position;
                //Vector3 shipRelativeToCamera = (tpCamMount.transform.position + tpCamMount.transform.forward) - tpCamMount.transform.position;
                Vector3 shipRelativeToCamera = tpCamMount.transform.forward;

                //The rotation to look at that point
                Quaternion rotationToWhereCameraIsLooking = Quaternion.LookRotation(shipRelativeToCamera);

                //The rotation from how the ship is currently rotated to where the camera is looking
                //Multiplying by inverse is equivalent to subtracting
                Quaternion rotation = rotationToWhereCameraIsLooking * Quaternion.Inverse(rb.rotation);

                //Parse Quaternion to Vector3
                Vector3 torqueVector = new Vector3(rotation.x, rotation.y, rotation.z) * rotation.w;

                //TORQURE STRENGTH
                //Base strength modifier
                //float torqueBaseStrength = 30f;

                ////Smoothing modifier so we don't overshoot
                ////If the rotation needed is very small but we have a ton of angular velocity, we need to slow down - not speed up
                ////How far the rotation is, where 2 is directly behind
                //float rotationDistance = (shipRelativeToCamera.normalized - transform.forward).magnitude;
                ////Larger values mean a smaller window in which the torque reduces
                //float threshold = 2f;
                ////Normalizing
                //float torqueDistanceModifier = Mathf.Min(1f, rotationDistance * threshold);

                //Adding all modifiers together
                float torqueStrength = torqueBaseStrength * rb.angularDrag * Time.deltaTime * Mathf.Max(1f, upgradeLevels[control.commerce.UPGRADE_TORQUE_FORCE] * UPGRADE_TORQUE_FORCE_MULTIPLIER); //torque upgrade improves torque by 1.5x

                //APPLY TORQUE
                Vector3 torqueFinal = torqueVector * torqueStrength;
                if (torqueFinal.magnitude != 0f) //so we don't get NaN error
                {
                    rb.AddTorque(torqueFinal);
                }
            }
        }
        else
        {
            //Reset angular drag when engines not on
            rb.angularDrag = 0f;
        }
    }

    private void UpdatePlayerMovementThrust()
    {
        //Reset
        thrustVector = Vector3.zero;
        thrustMultiplier = 1f;
        lastForceAdded = Vector3.zero;

        //Move if fuel
        if ((tempEngineDisable <= 0f || tempEngineDisableButFlickering) && vitalsFuel > 0.0d)
        {
            //Thrusting forward
            if (binds.GetInput(binds.bindThrustForward))
            {
                //Add forward to thrust vector
                thrustVector += transform.forward;

                //Faster if moving forward
                thrustMultiplier *= THRUST_FORWARD_MULTIPLIER;

                //Warming up; no weapons used recently
                if (weaponUsedRecently <= 0f)
                {
                    //Engine warmup jerk (increasing acceleration)
                    thrustEngineWarmupMultiplier = Mathf.Min(thrustEngineWarmupMultiplierMax,
                        thrustEngineWarmupMultiplier + (thrustEngineWarmupMultiplier * THRUST_ENGINE_WARMUP_SPEED * Time.deltaTime));

                    //Total multiplier (moving forward and warmed up)
                    thrustMultiplier *= thrustEngineWarmupMultiplier;
                }
            }

            //Forward thrust warmup is lost when no longer moving forward or when recently used a weapon
            if (!binds.GetInput(binds.bindThrustForward) || weaponUsedRecently > 0f)
            {
                //Engine warmup decrement
                thrustEngineWarmupMultiplier = Mathf.Max(1f, thrustEngineWarmupMultiplier - (Time.deltaTime * THRUST_ENGINE_COOLDOWN_SPEED));
            }

            //Improve thrust after acceleration upgrade (we also increase drag)
            if (upgradeLevels[control.commerce.UPGRADE_ACCELERATION] >= 1)
            {
                thrustMultiplier *= upgradeLevels[control.commerce.UPGRADE_ACCELERATION] * UPGRADE_ACCELERATION_MULTIPLIER;
            }

            //Reduce thrust when drag isn't present
            if (!control.settings.matchVelocity || tempEngineDisableButFlickering)
            {
                thrustMultiplier *= THRUST_REDUCTION_TO_COMPENSATE_NO_DRAG_MULTIPLIER;
            }

            //Debug thrust modifier
            thrustMultiplier *= thrustCheat;

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

                lastForceAdded = thrustVector.normalized * THRUST * thrustMultiplier * Time.deltaTime;
                rb.AddForce(lastForceAdded);
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
                mapCam.transform.position = transform.position + mapOffset + (Vector3.up * mapCam.GetComponent<Camera>().farClipPlane / 2f);

                //Map panning
                if (binds.GetInput(binds.bindPanMap))
                {
                    float mapRatio = 0.03f;
                    mapOffset -= Vector3.right * (binds.GetMouseMovementX() * mapCam.GetComponent<Camera>().orthographicSize * mapRatio);
                    mapOffset -= Vector3.forward * (binds.GetMouseMovementY() * mapCam.GetComponent<Camera>().orthographicSize * mapRatio);
                }

                //Set map zoom (default 1560)
                if (binds.GetInput(binds.bindCameraZoomFollowDistOut))
                {
                    mapCam.GetComponent<Camera>().orthographicSize = Mathf.Min(15000.0f, mapCam.GetComponent<Camera>().orthographicSize *= 1.1f);
                }
                else if (binds.GetInput(binds.bindCameraZoomFollowDistIn))
                {
                    mapCam.GetComponent<Camera>().orthographicSize = Mathf.Max(10.0f, mapCam.GetComponent<Camera>().orthographicSize *= 0.9f);
                }

                //Map model of the player ship scales with the map zoom so that it is always visible
                Transform mapModel = transform.parent.Find("Position Mount").Find("Centre Mount").Find("Ship Map Model");
                mapModel.localScale = Vector3.one * mapCam.GetComponent<Camera>().orthographicSize;
            }
            else
            {
                //Not map
                mapOffset = Vector3.zero;

                if (binds.GetInput(binds.bindCameraZoomFollowDistIn) || binds.GetInput(binds.bindCameraZoomFollowDistOut))
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
                !isDestroyed
                && vitalsFuel >= 0.0d
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
        if (music.clip == songCombat && (Time.time >= combatLastAggroTime + COMBAT_PERIOD_THRESHOLD_TIMEOUT || nEnemiesAggrod == 0))
        {
            //No longer in combat
            //Fade out and stop
            if (music.volume > 0f)
            {
                float fadeOutRate = 0.005f;
                music.volume = Mathf.Max(0f, music.volume - (Time.deltaTime * fadeOutRate));
            }
            else
            {
                //Return volume back to normal
                music.volume = control.settings.volumeAll * control.settings.volumeMusic * control.settings.VOLUME_MUSIC_MULTIPLIER;

                //Stop the combat song
                music.Stop();
            }
        }
        else if (Time.time < combatLastAggroTime + COMBAT_PERIOD_THRESHOLD_TIMEOUT)
        {
            //In combat
            if (music.clip != songCombat || Time.time >= musicPlayTime)
            {
                //Just entered combat
                //Fade out previous song
                if (music.volume > 0f)
                {
                    float fadeOutRate = 0.005f;
                    music.volume = Mathf.Max(0f, music.volume - (Time.deltaTime * fadeOutRate));
                }
                else
                {
                    //Return volume back to normal
                    music.volume = control.settings.volumeAll * control.settings.volumeMusic * control.settings.VOLUME_MUSIC_MULTIPLIER; //0.01 is default max music volume

                    //Stop previous song
                    music.Stop();

                    //Assign the track
                    music.clip = songCombat;

                    //Play the track
                    if (control.settings.music)
                    {
                        music.Play();
                    }

                    //Queue another song for after the current one finishes
                    musicPlayTime = Time.time + music.clip.length + UnityEngine.Random.Range(MUSIC_PLAY_QUEUE_TIME, MUSIC_PLAY_QUEUE_TIME + MUSIC_PLAY_QUEUE_VARIANCE_TIME);
                }
            }
        }
        else if (Time.time >= musicPlayTime)
        {
            //Not in combat; play regular song
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

        vitalsFuelMax = VITALS_FUEL_MAX_STARTER * (1.0d + (upgradeLevels[control.commerce.UPGRADE_TITAN_FUEL_TANK] * 0.5d));

        control.ui.UpdatePlayerVitalsDisplay();

        //Mining
        oreMax = ORE_MAX_DEFAULT * Mathf.Max(1f, 2f * upgradeLevels[control.commerce.UPGRADE_CARGO_SPACE]);

        //Movement
        thrustEngineWarmupMultiplierMax = THRUST_ENGINE_WARMUP_MULTIPLIER_MAX_STARTER * (1 + upgradeLevels[control.commerce.UPGRADE_RAPTOR_ENGINES]);

        //Abilities
        if (upgradeLevels[control.commerce.UPGRADE_OUTLINE] >= 1)
        {
            //Eclipse vision
            //Show CD UI
            control.ui.abilityEclipseVision.gameObject.SetActive(true);

            //Tip
            if (!tipHasBoughtOutline)
            {
                tipHasBoughtOutline = true;
            }
        }
        if (upgradeLevels[control.commerce.UPGRADE_REFINERY] >= 1 && !tipHasBoughtRefinery)
        {
            //Refinery tip
            tipHasBoughtRefinery = true;
        }

        //Weapons
        if (upgradeLevels[control.commerce.UPGRADE_SEISMIC_CHARGES] >= 1)
        {
            //Give mining laser weapon
            //Slot 0 is always the mining laser
            weaponSlot1 = weaponSeismicCharges;

            //Display tip
            tipHasBoughtSeismicCharges = true;
        }
        playerWeaponLaser.UpdateUpgrades();
        playerWeaponSeismicCharge.UpdateUpgrades();
        //control.ui.UpdatePlayerWeaponsUI();
        //This is called in update anyway
    }
    #endregion

    #region General methods: Movement
    #endregion

    #region General methods: Camera
    private void SetCameraFollowDistance()
    {
        //Camera follow distance
        //Zoom in/out
        float zoomRate = 0.2f;
        control.settings.cameraDistance = Math.Min(
            control.settings.CAMERA_DISTANCE_MAX,
            Math.Max(
                control.settings.CAMERA_DISTANCE_MIN,
                control.settings.cameraDistance - (((Convert.ToSingle(binds.GetInput(binds.bindCameraZoomFollowDistIn)) - 0.5f) * 2f) * zoomRate)
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

        if (!isDestroyed)
        {
            firstPerson = control.settings.cameraDistance <= control.settings.CAMERA_DISTANCE_MIN + 0.01f;
            thirdPerson = !firstPerson;
        }

        //First-person cameras & model
        fpCam.SetActive(firstPerson);
        fpCamInterior.SetActive(firstPerson);
        fpModel.SetActive(firstPerson);

        //Third-person camera & model
        tpCam.SetActive(thirdPerson || isDestroyed);
        tpModel.SetActive(thirdPerson);

        //Spotlight
        DecideWhichModelsToRenderSpotlight();

        //Jet glow
        //transform.Find("TP Model").Find("Blackness Behind Jet").gameObject.SetActive(!(isDestroyed || vitalsFuel <= 0.0d));
        //Instead of completely removing the jet glow we are changing its color
        //transform.Find("TP Model").Find("Jet Glow").gameObject.SetActive(!(isDestroyed || vitalsFuel <= 0.0d));

        //Ship direction reticles
        control.ui.playerShipDirectionReticleTree.SetActive(!isDestroyed);

        //Tractor beam (too bright in FP or too dim in TP without this) and tip text (avoid obstructing the player's view)
        if (firstPerson)
        {
            control.ui.tipText.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 275f, 0f);
            tractorBeamMaterial.SetFloat("minAlpha", 0.001f); //minAlpha, property 2/2, Vector1
        }
        else
        {
            control.ui.tipText.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 205f, 0f);
            tractorBeamMaterial.SetFloat("minAlpha", 0.005f);
        }
    }

    public void DecideWhichModelsToRenderSpotlight()
    {
        if (!isDestroyed && spotlightIsOn && !spotlightIsFlickeredOff)
        {
            transform.Find("Spotlight").gameObject.SetActive(true);
        }
        else
        {
            transform.Find("Spotlight").gameObject.SetActive(false);
        }
    }

    private void GetMouseToCameraTransform()
    {
        //Pitch
        centreMountPitch -= binds.GetMouseMovementY() * control.settings.mouseSensitivity * MOUSE_SENS_COEFF;
        //Yaw
        if (centreMountPitch >= 90 && centreMountPitch < 270)
        {
            //Normal
            centreMountYaw -= binds.GetMouseMovementX() * control.settings.mouseSensitivity * MOUSE_SENS_COEFF;
        }
        else
        {
            //Inverted
            centreMountYaw += binds.GetMouseMovementX() * control.settings.mouseSensitivity * MOUSE_SENS_COEFF;
        }
        //Roll
        centreMountRoll = 0f;

        centreMountYaw = Control.LoopEulerAngle(centreMountYaw);
        centreMountPitch = Control.LoopEulerAngle(centreMountPitch);
        centreMountRoll = Control.LoopEulerAngle(centreMountRoll);
    }

    private void UpdateMountPositions()
    {
        //CENTRE
        //Set the centre mount's transform
        //ACCELERATION
        //Calculate acceleration
        for (int i = accelerationPrevious.Length; i > 1; i--) //> 1 because we count up from 0, so index is offset from length
        {
            accelerationPrevious[i - 1] = accelerationPrevious[i - 2]; //-1 because we count up from 0, so index is offset from length
        }
        accelerationPrevious[0] = acceleration;
        acceleration = (rb.velocity - lastVelocity) / Time.fixedDeltaTime;

        //Average out acceleration changes so we don't have tons of noise
        Vector3 accelerationSmoothed = acceleration;
        for (int i = 0; i < accelerationPrevious.Length; i++)
        {
            accelerationSmoothed += accelerationPrevious[i];
        }
        accelerationSmoothed /= 1f + accelerationPrevious.Length; //adding one because acceleration is not a part of the accelerationPrevious array

        //Generate offset for the camera to use
        float cameraOffsetProportionalToAcceleration = 0.000003f;
        cameraOffsetAcceleration = accelerationSmoothed * cameraOffsetProportionalToAcceleration; //negative acceleration so we move back when we accelerate forward

        //Remember the previous velocity so we can calculate the acceleration next time
        lastVelocity = rb.velocity;

        //SHAKE
        //Camera shake rotation
        centreMountTran.localRotation = Quaternion.Euler(
            centreMountPitch + cameraOffsetRotationPitch,
            centreMountYaw + cameraOffsetRotationYaw,
            0f
        );

        //FIRST-PERSON
        UpdateFPCamMountPosition();

        //THIRD-PERSON
        UpdateTPCamMountPosition();
    }

    private void UpdateTPCamMountPosition()
    {
        tpCamMount.transform.localRotation = Quaternion.Euler(centreMountPitch, centreMountYaw, 0f);

        float cameraSpeedEffect = 1f; //1f + Mathf.Pow(rb.velocity.magnitude, 0.15f);
        Vector3 cameraUp = centreMountTran.up * (control.settings.cameraDistance * control.settings.cameraHeight) * cameraSpeedEffect;
        Vector3 cameraForward = centreMountTran.forward * control.settings.cameraDistance * cameraSpeedEffect;

        //Camera shake increases in third person
        //float cameraZoomDistanceRange = control.settings.CAMERA_DISTANCE_MAX - control.settings.CAMERA_DISTANCE_MIN;
        //float cameraZoomPercent = (control.settings.cameraDistance - control.settings.CAMERA_DISTANCE_MIN) / cameraZoomDistanceRange;
        float cameraZoomPercent = control.settings.cameraDistance / control.settings.CAMERA_DISTANCE_MAX;
        float cameraZoomOutBaseShakeIncrease = 0f;
        float cameraZoomOutProportionalShakeIncrease = 170f;
        float cameraTPShakeFactor = cameraZoomOutBaseShakeIncrease + (cameraZoomPercent * cameraZoomOutProportionalShakeIncrease);
        //float cameraTPShakeFactor = 50f;

        tpCamMount.transform.position = transform.position + cameraOffsetAcceleration + (cameraOffsetPosition * cameraTPShakeFactor) + cameraUp - cameraForward; //subtracting forward results in the camera following behind the player, this should be more performant than *-1
    }

    private void UpdateFPCamMountPosition()
    {
        fpCamMount.transform.position = transform.position + cameraOffsetAcceleration + cameraOffsetPosition + (transform.forward * 0.115f) + (transform.up * 0.008f);
    }

    public void UpdateCameraSettings()
    {
        //Clip planes
        fpCamInterior.GetComponent<Camera>().nearClipPlane = FP_CAM_INTERIOR_CLIPPING_PLANE_NEAR;
        fpCamInterior.GetComponent<Camera>().farClipPlane = FP_CAM_INTERIOR_CLIPPING_PLANE_FAR;

        //HFOV
        if (control.settings.hFieldOfView > 0f)
        {
            fpCamInterior.GetComponent<Camera>().fieldOfView = Camera.HorizontalToVerticalFieldOfView(control.settings.hFieldOfView * cameraZoom, fpCamInterior.GetComponent<Camera>().aspect);
            fpCam.GetComponent<Camera>().fieldOfView = Camera.HorizontalToVerticalFieldOfView(control.settings.hFieldOfView * cameraZoom, fpCam.GetComponent<Camera>().aspect);
            tpCam.GetComponent<Camera>().fieldOfView = Camera.HorizontalToVerticalFieldOfView(control.settings.hFieldOfView * cameraZoom, tpCam.GetComponent<Camera>().aspect);

            fovSet = true;
        }
    }
    #endregion

    #region General methods: Weapons
    public int GetWeaponSelectedID()
    {
        int weaponSelectedId;

        if (weaponSlotSelected == 0)
        {
            weaponSelectedId = weaponSlot0.ID;
        }
        else
        {
            weaponSelectedId = weaponSlot1.ID;
        }

        return weaponSelectedId;
    }

    public string GetWeaponSelectedName()
    {
        string weaponSelectedName;

        if (weaponSlotSelected == 0)
        {
            weaponSelectedName = weaponSlot0.NAME;
        }
        else
        {
            weaponSelectedName = weaponSlot1.NAME;
        }

        return weaponSelectedName;
    }

    private void WeaponsFire()
    {
        //Remember that a weapon was used recently (used for disabling afterburners)
        weaponUsedRecently = 1f;

        //Fire whichever weapon is selected
        if (GetWeaponSelectedID() == WEAPON_ID_MINING_LASER)
        {
            playerWeaponLaser.Fire();
            
            //Auto-torquing tip
            if (
                !binds.GetInput(binds.bindCameraFreeLook) &&
                !canAndIsMoving &&
                !binds.GetInput(binds.bindAlignShipToReticle) &&
                Mathf.Abs(Quaternion.Dot(transform.localRotation, centreMountTran.localRotation)) < control.ui.TIP_AIM_THRESHOLD_ACCURACY
                )
            {
                control.ui.tipAimNeedsHelpCertainty++;
            }
        }
        else if (GetWeaponSelectedID() == WEAPON_ID_SEISMIC_CHARGES)
        {
            playerWeaponSeismicCharge.Fire();
            tipHasUsedSeismicCharges = true;
        }
    }

    private void UpdateWeaponSelected()
    {
        //Select
        if (binds.GetInputDown(binds.bindSelectWeaponSlot0))
        {
            weaponSlotSelected = 0;
        }
        else if (binds.GetInputDown(binds.bindSelectWeaponSlot1))
        {
            weaponSlotSelected = 1;
        }

        //Get properties
        if (GetWeaponSelectedID() == WEAPON_ID_MINING_LASER)
        {
            weaponSelectedClipSize = playerWeaponLaser.clipSize;
            weaponSelectedClipRemaining = playerWeaponLaser.clipRemaining;
            weaponSelectedClipCooldownDuration = playerWeaponLaser.CLIP_COOLDOWN_DURATION;
            weaponSelectedClipCooldownCurrent = playerWeaponLaser.clipCooldownCurrent;

            weaponSelectedSingleCooldownDuration = playerWeaponLaser.SINGLE_COOLDOWN_DURATION;
            weaponSelectedSingleCooldownCurrent = playerWeaponLaser.singleCooldownCurrent;

            weaponSelectedProjectileSpeed = playerWeaponLaser.PROJECTILE_SPEED;
        }
        else if (GetWeaponSelectedID() == WEAPON_ID_SEISMIC_CHARGES)
        {
            weaponSelectedClipSize = playerWeaponSeismicCharge.clipSize;
            weaponSelectedClipRemaining = playerWeaponSeismicCharge.clipRemaining;
            weaponSelectedClipCooldownDuration = playerWeaponSeismicCharge.CLIP_COOLDOWN_DURATION;
            weaponSelectedClipCooldownCurrent = playerWeaponSeismicCharge.clipCooldownCurrent;

            weaponSelectedSingleCooldownDuration = playerWeaponSeismicCharge.SINGLE_COOLDOWN_DURATION;
            weaponSelectedSingleCooldownCurrent = playerWeaponSeismicCharge.singleCooldownCurrent;

            weaponSelectedProjectileSpeed = playerWeaponSeismicCharge.PROJECTILE_SPEED;
        }
        else
        {
            //None
            weaponSelectedClipSize = 0;
            weaponSelectedClipRemaining = 0;
            weaponSelectedClipCooldownDuration = 0f;
            weaponSelectedClipCooldownCurrent = 0f;

            weaponSelectedSingleCooldownDuration = 0f;
            weaponSelectedSingleCooldownCurrent = 1f;
        }

        //UI
        //control.ui.UpdateWeapons(weaponSlotSelected, upgradeLevels[control.commerce.UPGRADE_SEISMIC_CHARGES] >= 1);
        control.ui.UpdateWeapons();
    }

    
    #endregion

    #region General methods: Audio
    private void PlayMusic()
    {
        //Select the track
        if (firstSong && control.settings.tutorial)
        {
            //Only force the first song to be manually curated if the tutorial is enabled
            music.clip = songHoghmanTransfer;
            firstSong = false;
        }
        else
        {
            //Play a random song, but avoid the same song twice
            AudioClip nextSong = music.clip;
            for(int i = 0; i < 100; i++)
            {
                nextSong = GetNextSong();
                if (nextSong != music.clip)
                {
                    break;
                }
            }
            music.clip = nextSong;
        }

        //Play the track
        if (control.settings.music)
        {
            music.volume = control.settings.volumeAll * control.settings.volumeMusic * SOUND_MUSIC_VOLUME;
            music.Play();
        }
        
        //Queue another song for after the current one finishes
        musicPlayTime = Time.time + music.clip.length + UnityEngine.Random.Range(MUSIC_PLAY_QUEUE_TIME, MUSIC_PLAY_QUEUE_TIME + MUSIC_PLAY_QUEUE_VARIANCE_TIME);
    }

    private AudioClip GetNextSong()
    {
        float roll = UnityEngine.Random.value;
        if (roll >= 0.7f)
        {
            return songDrifting;
        }
        else if (roll >= 0.4f)
        {
            return songHoghmanTransfer;
        }
        else if (roll >= 0.1f)
        {
            return songLifeSupportFailure;
        }
        else
        {
            return songWeWereHere;
        }
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
        float rocketVolumeDeltaRate = SOUND_ROCKET_VOLUME_DELTA_RATE;
        if (tempEngineDisable > 0f)
        {
            rocketVolumeDeltaRate *= 1.5f;
        }

        if (canAndIsMoving)
        {
            float volumeRocketMax = SOUND_ROCKET_VOLUME * control.settings.volumeAll;

            //Increment
            soundSourceRocket.volume = Mathf.Min(volumeRocketMax, soundSourceRocket.volume + (Time.deltaTime * rocketVolumeDeltaRate));

            soundSourceRocket.pitch = Mathf.Min(SOUND_ROCKET_PITCH_MAX, soundSourceRocket.pitch + (Time.deltaTime * SOUND_ROCKET_PITCH_DELTA_RATE));
        }
        else
        {
            //Decrement
            soundSourceRocket.volume = Mathf.Max(0f, soundSourceRocket.volume - ((soundSourceRocket.volume * Time.deltaTime * rocketVolumeDeltaRate) * 32f));

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

    private void UpdateOutlines()
    {
        //Planetary systems and their celestial bodies
        int nPlanetarySystems = control.generation.planets.transform.childCount;
        for (int systemIndex = 0; systemIndex < nPlanetarySystems; systemIndex++)
        {
            //The first dimensions refers to which planetary system, the second dimensions are the bodies in that system - 0 is the planet itself, the rest are moons
            //
            //            Planetary system    Planet, moons
            //                     \/          \/
            //planetarySystems[systemIndex][bodyIndex]

            int nBodiesInSystem = control.generation.planetarySystems[systemIndex].Count;
            for (int bodyIndex = 0; bodyIndex < nBodiesInSystem; bodyIndex++)
            {
                if (bodyIndex == 0)
                {
                    GameObject thisPlanet = control.generation.planetarySystems[systemIndex][bodyIndex];

                    //Planet
                    UpdateOutlineMaterial(
                        Generation.HighlightableCBodyType.planet,
                        thisPlanet.GetComponentInChildren<MeshRenderer>().material
                    );

                    //Heighliners
                    UpdateOutlineMaterial(
                        Generation.HighlightableCBodyType.heighliner,
                        thisPlanet.GetComponent<Planet>().heighliner0.transform.Find("Model").GetComponentInChildren<MeshRenderer>().material
                    );
                    UpdateOutlineMaterial(
                        Generation.HighlightableCBodyType.heighliner,
                        thisPlanet.GetComponent<Planet>().heighliner1.transform.Find("Model").GetComponentInChildren<MeshRenderer>().material
                    );
                }
                else
                {
                    GameObject thisMoon = control.generation.planetarySystems[systemIndex][bodyIndex];

                    //Moons
                    UpdateOutlineMaterial(
                        Generation.HighlightableCBodyType.moon,
                        thisMoon.GetComponentInChildren<MeshRenderer>().material
                    );

                    //Station
                    if (thisMoon.GetComponent<Moon>().hasStation)
                    {
                        Transform stationModelTransform = thisMoon.GetComponent<Moon>().instancedStation.transform.Find("StationModel");

                        UpdateOutlineMaterial(
                            Generation.HighlightableCBodyType.station,
                            stationModelTransform.Find("StationModelMain").GetComponentInChildren<MeshRenderer>().material
                        );
                        UpdateOutlineMaterial(
                            Generation.HighlightableCBodyType.station,
                            stationModelTransform.Find("StationModelMainClimateControl").GetComponentInChildren<MeshRenderer>().material
                        );
                        UpdateOutlineMaterial(
                            Generation.HighlightableCBodyType.station,
                            stationModelTransform.Find("StationTower").GetComponentInChildren<MeshRenderer>().material
                        );
                        UpdateOutlineMaterial(
                            Generation.HighlightableCBodyType.station,
                            stationModelTransform.Find("StationDryDock").Find("StationModelArms").GetComponentInChildren<MeshRenderer>().material
                        );
                    }
                }
            }
        }

        //Asteroids
        int nAsteroidsEnabled = control.generation.asteroidsEnabled.transform.childCount;
        for (int asteroidIndex = 0; asteroidIndex < nAsteroidsEnabled; asteroidIndex++)
        {
            Transform asteroidTransform = control.generation.asteroidsEnabled.transform.GetChild(asteroidIndex);
            if (!asteroidTransform.GetComponentInChildren<Asteroid>().isDestroying)
            {
                UpdateOutlineMaterial(
                    Generation.HighlightableCBodyType.asteroid,
                    asteroidTransform.GetComponentInChildren<MeshRenderer>().material
                );
            }
        }

        //Bandits
        int nBandits = control.generation.enemies.transform.childCount;
        for (int banditIndex = 0; banditIndex < nBandits; banditIndex++)
        {
            Enemy banditScript = control.generation.enemies.transform.GetChild(banditIndex).GetComponent<Enemy>();
            if (!banditScript.isDestroying)
            {
                UpdateOutlineMaterial(
                    Generation.HighlightableCBodyType.bandit,
                    banditScript.model.transform.GetChild(0).GetComponent<MeshRenderer>().material
                );
            }
        }
    }

    public void UpdateOutlineMaterial(Generation.HighlightableCBodyType cBodyType, Material material)
    {
        if (isOutlinesVisible && upgradeLevels[control.commerce.UPGRADE_OUTLINE] >= 1)
        {
            float intensity = 1f;
            if      (cBodyType == Generation.HighlightableCBodyType.planet)     { intensity = 0.3f; }
            else if (cBodyType == Generation.HighlightableCBodyType.moon)       { intensity = 0.7f; }
            else if (cBodyType == Generation.HighlightableCBodyType.heighliner) { intensity = 3f; }
            else if (cBodyType == Generation.HighlightableCBodyType.station)    { intensity = 5f; }
            else if (cBodyType == Generation.HighlightableCBodyType.asteroid)   { intensity = 5f; }
            else if (cBodyType == Generation.HighlightableCBodyType.bandit)     { intensity = 5f; }
            else { Debug.LogError("Unrecognized HighlightableCBodyType: " + cBodyType); }

            material.SetFloat("_NightVisionOutline", intensity * outlineFade);
        }
        else
        {
            material.SetFloat("_NightVisionOutline", 0f);
        }
    }

    public void ToggleOutline()
    {
        //Enabling
        if (!isOutlinesVisible)
        {
            outlineActivatedTime = Time.time;
        }

        //Toggle
        isOutlinesVisible = !isOutlinesVisible;
        
        //Update shaders
        UpdateOutlines();
    }
    #endregion

    #region General methods: ClosestTransforms
    //private Transform GetClosestTransform(Transform[] transforms)
    //{
    //    Transform closestTransform = null;
    //    float closestDistanceSqr = Mathf.Infinity;
    //
    //    //The position to compare distances to, usually the player's position
    //    Vector3 sourcePosition = transform.position;
    //
    //    foreach (Transform transformToCheck in transforms)
    //    {
    //        Vector3 vectorToTransformToCheck = transformToCheck.position - sourcePosition;
    //
    //        float distanceSqrToTransformToCheck = vectorToTransformToCheck.sqrMagnitude;
    //        if (distanceSqrToTransformToCheck < closestDistanceSqr)
    //        {
    //            closestDistanceSqr = distanceSqrToTransformToCheck;
    //            closestTransform = transformToCheck;
    //        }
    //    }
    //
    //    return closestTransform;
    //}

    //private Transform GetClosestMoonTransform()
    //{
    //    Transform moonsFolderTransform = cBodies.transform.Find("Moons");
    //    Transform[] moonTransforms = new Transform[moonsFolderTransform.childCount];
    //    for (int i = 0; i < moonsFolderTransform.childCount; i++)
    //    {
    //        moonTransforms[i] = moonsFolderTransform.GetChild(i);
    //    }
    //
    //    return GetClosestTransform(moonTransforms);
    //}
    #endregion

    #region General methods: Damage
    void OnCollisionEnter(Collision collision)
    {
        //COLLISION PROPERTIES
        //Was it a weapon? Although bandit interactions are normally handled by the projectile class, sometimes a collision occurs first (is this actually true?)
        bool isBanditLaser = (collision.collider.name == control.generation.enemy.GetComponent<EnemyWeaponLaser>().enemyWeaponProjectileLaserPrefab.GetComponentInChildren<MeshCollider>().name);

        //Collision speed
        float impactIntoleranceThreshold = 5f;
        float impactIntoleranceRange = 6f;
        float impactMaxDamage = 3f;
        Vector3 impactDeltaV = collision.relativeVelocity;

        //SELF
        double damageToDeal = 0.0d;
        if (isBanditLaser || impactDeltaV.magnitude >= impactIntoleranceThreshold && collisionDamageInvulnerabilityTemporary <= 0f)
        {
            //Shake camera
            CameraShakeAdd(new Vector2(CAMERA_OFFSET_POSITION_MAGNITUDE_MAX, CAMERA_OFFSET_ROTATION_MAGNITUDE_MAX));

            //Play sound effect
            soundSourceCollision.volume = SOUND_IMPACT_VOLUME_SIGNIFICANT * control.settings.volumeAll; //we need to adjust the volume here because we lower it when the impact is insignificant
            soundSourceCollision.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
            soundSourceCollision.Play();

            //Damage
            if (isBanditLaser)
            {
                //Damage is hardcoded per-projectile
                damageToDeal = EnemyWeaponLaser.DAMAGE;
            }
            else
            {
                //Damage depends on impact speed
                damageToDeal = Mathf.Min(
                        impactIntoleranceThreshold * impactIntoleranceRange,
                        impactDeltaV.magnitude
                    ) / (impactIntoleranceThreshold * impactIntoleranceRange / impactMaxDamage);
            }
            //Subtract damage from current health
            double newHealthAmount = Math.Max(
                0.0,
                vitalsHealth - damageToDeal
            );
            
            //Damage event cause
            string cause;
            if (isBanditLaser)
            {
                //cause = "bandit laser impact";
                cause = "incoming fire";
            }
            else
            {
                //cause = "over-tolerance impact of " + (int)impactDeltaV.magnitude + " Δv";
                cause = (int)impactDeltaV.magnitude + " Δv impact";
            }

            //Stun time
            float stunTime = (float)damageToDeal;
            if (isBanditLaser)
            {
                stunTime = 0.75f;
            }

            //Damage the player
            DamagePlayer(
                newHealthAmount,
                cause,
                stunTime,
                (collision.collider.transform.position - transform.position).normalized,
                true
            );
        }
        else if (collisionDamageInvulnerabilityTemporary <= 0f)
        {
            //Play sound effect
            soundSourceCollision.volume = SOUND_IMPACT_VOLUME_INSIGNIFICANT * control.settings.volumeAll; //insignificant impacts are quieter
            soundSourceCollision.pitch = UnityEngine.Random.Range(0.4f, 0.8f);
            soundSourceCollision.Play();
        }

        //IMPACTED OBJECT
        //If an asteroid, deal damage to it
        if (collision.gameObject.name == control.generation.asteroid.name + "(Clone)")
        {
            //Get ref
            Asteroid asteroidScript = collision.transform.GetComponent<Asteroid>();
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
        }
    }

    public void DamagePlayer(double newHealthAmount, string cause, float tempEngineDisableDuration, Vector3 directionDamageCameFrom, bool emitParticles)
    {
        if (!isDestroyed)
        {
            //Damage
            if (!healthInfiniteCheat)
            {
                vitalsHealth = newHealthAmount;
            }
            
            //UI warning
            control.ui.UpdatePlayerVitalsDisplay(); //force a vitals update so that you can immediately see your health change
            FlashWarning("WARNING: " + cause + "\nHull integrity compromised"); //⚠
            lastDamageCause = cause;

            //Emit particles
            if (emitParticles)
            {
                EmitParticles(1, directionDamageCameFrom, false);
            }

            if (vitalsHealth <= 0d)
            {
                vitalsHealth = 0d;
                //Kill
                DestroyPlayer();
            }
            else
            {
                //Disable engines
                tempEngineDisable = Mathf.Max(tempEngineDisable, tempEngineDisableDuration);
            }
        }
    }

    private void DestroyPlayer()
    {
        //Emit particles
        EmitParticles(7, Vector3.zero, true);

        //Play sound
        //TODO

        //Remember is destroyed
        isDestroyed = true;

        //Hide models (after destroyed is called because their visibility is determined by that variable)
        DecideWhichModelsToRender();
    }

    private void EmitParticles(int amount, Vector3 direction, bool destroyAfter)
    {
        //Get the transform of the current particle from the pool
        Transform particleTran = damageParticles.transform.GetChild(damageParticlesPoolIndex);

        //Set particle system to current particle pool index
        GetComponent<ParticlesDamageRock>().particlesDamageRock = particleTran.GetComponent<ParticleSystem>();

        //Teleport particle system to player position
        particleTran.position = transform.position;

        //Emit particles
        GetComponent<ParticlesDamageRock>().EmitDamageParticles(amount, direction, transform.position, destroyAfter);

        //Advance index for the next call (but don't go over the limit)
        damageParticlesPoolIndex = (damageParticlesPoolIndex + 1) % damageParticlesPoolLength;
    }
    #endregion
    #endregion
}