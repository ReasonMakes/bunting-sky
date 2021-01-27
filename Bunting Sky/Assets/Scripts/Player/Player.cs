using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    #region Todo, readme, and credits
    //COMBAT MODE which has the current movement scheme allowing you to dive and everything. When combat mode is off the ship is controlled by point and click and only moves along x and z axes similar to Spore
    //Point lights do not support shadows
    //COMBAT MUSIC FROM SUBNAUTICA
    //Not all space stations have dry docks, so you can't always repair but you CAN always refuel
    //Top-down map mode where all cBodies are highlighted/inflated just like scene view when highlighting cBodies group, and you can set targets

    //To advertise the release, make several tutorial videos explaining how the various features work and how to implement them in your own game
    //Have a brief intro with sick Subnuatica-esque beats showcasing the game and the feature the tutorial will teach
    //At the end of the video, mention where to get the game if curious about it

    //THERE SHOULD BE TWO MOVEMENT MODES -> BURN, and MANEOUVRE
    //BURN MODE HAS NO DRAG ANYWHERE, JUST LIKE IRL SPACE
    //MANEOUVRE HAS DRAG RELATIVE TO THE VELOCITY THAT WAS BURNED TO
    //CAN AUTOMATICALLY BURN TO MATCH CBODIES (RUNS BY DEFAULT WHEN WITHIN CURRENT DISTANCE THRESHOLD, BUT CAN TURN OFF IN SETTINGS)
    //OR: have no friction unless dragging relative


    //ADD SEISMIC CHARGE WEAPON FOR CLEANING UP SMALL ASTEROIDS (start without, but is a cheap purchase at a station)

    //Space station or starport?
    //Automatically show if you're too close and too fast to slow down in time before flying past target

    //TO GO TO A NEW SYSTEM: ship deploys some antimatter thing or something in front of it which becomes a mini singularity thing from interstellar ("spherical hole")
    //Just completely rip off those graphic

    //It's possible to improve the look of the asteroid damage particles by switching to a PBR shader graph system

    //Line rendered in-game showing the movement vector of the ship?

    //Could auto rotate camera roll around when upside down, like with Subnautica, but that may take away from the space-feel

    //Engine glow affects local lighting, so when in first person and that light source is gone, the surroundings suddenly look darker.
    //This can be fixed by not deleting the light source and rendering a cockpit model to block view to it. On the back wall of the cockpit could be a camera or something

    //ICC stands for interstellar crypto currency

    //!!!!!TOP PRIORITY!!!!!
    //MAKE RESTART GENERATE ASTEROIDS

    //Implement upgrades
    //Add seismic charge weapon

    //Asteroids should automatically destroy themselves is far enough away from the player and there is an overrage (AND NOT TARGETTED OR NEAR A TARGETTED OBJECT)
    //Asteroids should automatically generate if there aren't enough in the verse

    //Add menu scrolling
    //Add setting to turn off music
    //Add keybinds menu

    //Fix ship auto-torquing

    //!!!!!!!!!!

    /*
     * MOVEMENT MODES:
     * sublight, for moving around asteroids, stations, and dogfighting - has a constant acceleration and drag
     * burn (or something, maybe raptor engines something) - has increasing acceleration up to a max equal to what warp currently is, has drag
     * warp (or a better name for warping) - traveling in between solar systems, travels through to bulk or maybe uses alcubierre drive, no drag, limited range (can't go past several solar systems, have to make pit stop).
     *      Also has a long cooldown time (more than a minute) and is expensive resource-wise (need lots of water).
     *      https://images.gr-assets.com/hostedimages/1437669203ra/15612130.gif
     */

    //TODO:
    /*
     * Add sound system which can handle multiple sounds being played at once (probably a dedicated object under control with an array of sound components)
     * Add collection sound, engine sound
     * Add more planetoid models
     * Add asteroid moons to planetoids
     * Randomly vary overall scale of asteroids?
     * 
     * Add weapon reticle
     * 
     * Work on map
     */

    /*
     * Music to add(?):
     * 
     * Caleb Etheridge - Skyboy
     * Jordan Critz - A Ripple in Time
     * Nick Box - Where Ocean Meets Sky
     * Chelsea McGough - Distant Water
     * 
     * !Add music from Sebastian Lague's videos!
     * Shimmer - Frontier
     * Shimmer - A Beautiful Dream
     * 
     * Antti Luode - Brief Respite
     * Tide Electric - When Rain Comes
     * Aeroplanes - Reflections of Space and Time
     * Bad Snacks - In the Atmosphere
     * MK2 - Reflections
     * Jesse Gallagher - Nidra in the Sky with Ayla
     * Antti Luode - Far Away
     */

    /*
     * DEFAULT CONTROLS (keybinds are saved in C:\Users\[YOUR USERNAME HERE]\AppData\LocalLow\Reason Studios)
     * 
     * - Move: WASD, LEFT CONTROL, and SPACE (forward thrusting is much faster than translating/strafing in any other direction)
     * - Orient view: mouse movement
     * - Free look: hold right mouse button (when moving your ship will automatically torque in the direction you are looking unless you hold right mouse button)
     * - Zoom in/out (and toggle first/third-person): mouse scroll wheel
     * - Cycle movement modes: TAB (increase/reduce max engine thrust)
     * - Increase/decrease thrust: E/Q
     * 
     * - Set object as target: middle mouse button (will automatically match velocity)
     * - Toggle spotlight on/off: F
     * - Map: M
     * 
     * - Take screenshot: F2
     * - Toggle entire HUD: F3
     * - Toggle FPS display: F4
     * 
     * 
     */

    /*
     * CREDITS
     * 
     * Rocket sound by Zovex
     * Modified by Reason: EQ'd and looped
     * https://freesound.org/people/Zovex/sounds/237974/
     * http://creativecommons.org/publicdomain/zero/1.0/
     * 
     * Cannon ball (laser) sound by OGsoundFX
     * Modified by Reason to start at the transient and fade out more quickly
     * https://freesound.org/people/OGsoundFX/sounds/423105/
     * https://creativecommons.org/licenses/by/3.0/
     * 
     * Elevator (reloading) sound by calivintage
     * Modified by Reason to fit the reloading time
     * https://freesound.org/people/calivintage/sounds/95701/
     * https://creativecommons.org/licenses/sampling+/1.0/
     * 
     * Ore collection sound is original - made by Reason using Serum in Reaper
     * 
     * Coins sound by DWOBoyle
     * https://freesound.org/people/DWOBoyle/sounds/140382/
     * https://creativecommons.org/licenses/by/3.0/
     * 
     * Rock slide (asteroid explosion) sound by Opsaaaaa
     * Modified by Reason: clipped to start at the transient and end as the volume dies out, deepened the pitch of the low-end, hushed the high-end
     * https://freesound.org/people/Opsaaaaa/sounds/335337/
     * https://creativecommons.org/licenses/by/3.0/
     * 
     * Metal storm door (ship collision) sound by volivieri
     * Modified by Reason: clipped to start at the second hit and end more quickly, deepened the pitch of the low-end, boosted bass, added a low-pass filter
     * https://freesound.org/people/volivieri/sounds/161190/
     * http://creativecommons.org/licenses/by/3.0/
     * 
     */
    #endregion

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

    public GameObject mapCam;
    #endregion

    //Spotlight
    public GameObject spotlight;

    #region Init fields: Movement
    //Movement
    public Rigidbody rb;
    private Vector3 thrustVector;
    private readonly float THRUST = 8416.65825f;
    private float thrustEngineWarmupMultiplier = 1f;
    [System.NonSerialized] public float thrustEngineWarmupMultiplierMax = 16f;
    private readonly float THRUST_ENGINE_WARMUP_SPEED = 0.5f; //3f;
    private readonly float THRUST_ENGINE_COOLDOWN_SPEED = 12f;
    private readonly float THRUST_FORWARD_MULTIPLIER = 1.1f;
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

    /*
    public GameObject cBodyStar;
    public GameObject cBodyPlanetoid;
    public GameObject station;
    public GameObject cBodyAsteroid;
    */

    #region Init fields: Audio
    //Audio: Music
    public AudioSource music;
    public AudioClip songDrifting;
    public AudioClip songLifeSupportFailure;
    public AudioClip songHoghmanTransfer;
    public AudioClip songWeWereHere;
    private float musicPlayTime = 30f;

    //Audio: Sound Effects
    public AudioSource soundSourceRocket;
    public AudioClip soundClipRocket;
    private float soundRocketVolumeDeltaRate = 0.1f;
    private float soundRocketMaxVolume = 0.02f;
    private float soundRocketMaxPitch = 1.5f;
    private float soundRocketPitchDeltaRate = 0.2f;

    public AudioSource soundSourceLaser0;
    public AudioSource soundSourceLaser1;
    public AudioSource soundSourceLaser2;
    public AudioSource soundSourceLaser3;
    private byte soundSourceLaserArrayIndex = 0;
    private byte soundSourceLaserArrayLength = 4;
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
    [System.NonSerialized] public bool destroyed = false;
    [System.NonSerialized] public double vitalsFuel = 15.0;
    [System.NonSerialized] public double vitalsFuelMax = 15.0;
    [System.NonSerialized] public double vitalsFuelConsumptionRate = 0.1;
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
    //Cargo (displayed on map screen or when trading at a station)
    [System.NonSerialized] public double currency = 100.0;
    //public double resWater = 0.0;
    //public double resPreciousMetal = 0.0;
    //public double resPlatinoids = 0.0;
    [System.NonSerialized] public double[] ore; //0 = Platinoids, 1 = PreciousMetal, 2 = Water

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
    #endregion

    #region Init fields: Weapons
    //Tree
    private GameObject playerWeaponsTree;

    //Laser
    [System.NonSerialized] public GameObject playerWeaponsTreeLaser;
    public GameObject playerLaser;
    private List<GameObject> weaponLaserPool = new List<GameObject>();
    private short WeaponLaserPoolIndex = 0;
    private short weaponLaserPoolLength = 16;
    
    private short weaponLaserClipSize = 16;
    private short weaponLaserClipRemaining = 16;
    private float weaponLaserClipCooldownDuration = 1.5f; //reload period
    private float weaponLaserClipCooldownCurrent = 0f;

    private float weaponLaserSingleCooldownDuration = 0.2f;
    private float weaponLaserSingleCooldownCurrent = 0f;
    
    private float weaponLaserProjectileSpeed = 120f;
    private float weaponLaserLifetimeDuration = 2f;
    #endregion

    //Skybox stars
    public ParticleSystem skyboxStarsParticleSystem;
    private int starCount = 400;
    #endregion

    #region Start
    private void Start()
    {
        DecideWhichModelsToRender();

        skyboxStarsParticleSystem.Emit(starCount);
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

        //KeyBinds
        binds = control.binds;

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
        //Weapons trees
        playerWeaponsTree = new GameObject("Weapons");
        playerWeaponsTree.transform.parent = control.generation.verseSpace.transform;

        playerWeaponsTreeLaser = new GameObject("Laser");
        playerWeaponsTreeLaser.transform.parent = playerWeaponsTree.transform;

        //Set up object pooling
        for (int i = 0; i < weaponLaserPoolLength; i++)
        {
            GameObject instancePlayerLaser = Instantiate(playerLaser, Vector3.zero, Quaternion.identity);
            weaponLaserPool.Add(instancePlayerLaser);
            instancePlayerLaser.SetActive(false);

            //Put in weapons tree
            instancePlayerLaser.transform.parent = playerWeaponsTreeLaser.transform;

            //Pass control reference
            instancePlayerLaser.GetComponent<PlayerLaser>().control = control;
        }

        //UI
        control.ui.weaponSelectedClipSizeText.text = "" + weaponLaserClipSize;

        //AUDIO
        //Play the first song 0 to 30 seconds after startup
        musicPlayTime = Time.time + UnityEngine.Random.Range(0f, 30f);

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

        //Upgrades
        upgradeLevels = new int[control.commerce.upgradeDictionary.GetLength(0)];
    }
    #endregion

    #region Update/fixed update & their slow versions
    private void Update()
    {
        //DEBUG
        //---------------------------------------------------
        //Teleport forward
        if (binds.GetInputDown(binds.bindThrustVectorIncrease))
        {
            transform.position += transform.forward * 1e4f;
            Debug.Log("Teleported forward: distance to star " + (control.generation.instanceCBodyStar.transform.position - transform.position).magnitude);
        }

        //Spawn
        /*
        if (binds.GetInputDown(binds.bindThrustVectorDecrease))
        {
            control.generation.SpawnAsteroidManually(transform.position + transform.forward * 2f, rb.velocity, true);
            Debug.Log("Spawned one asteroid");
        }
        */
        
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

            //Decrement fuel
            if (canAndIsMoving)
            {
                vitalsFuel = Math.Max(0.0, vitalsFuel - (vitalsFuelConsumptionRate * Time.deltaTime));
            }

            //Warn on loop if out of fuel
            if (vitalsFuel <= 0 && warningUIFlashTime <= 0f)
            {
                FlashWarning("Fuel reserves empty");

                //Loop smoothly and indefinitely
                warningUIFlashTime = warningUIFlashTotalDuration * 100f;
            }

            //Update map player ship position
            transform.parent.Find("Ship Map Model").position = transform.position;
            /*
            transform.parent.Find("Ship Map Model").position.Set(
                transform.position.x,
                1000f,
                transform.position.z
            );
            */
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
        UpdateVitalsDisplay();
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
            if (binds.GetInputDown(binds.bindToggleMap)) ToggleMapView();
            if (UI.displayMap)
            {
                mapCam.transform.position = Vector3.zero + (Vector3.up * mapCam.GetComponent<Camera>().farClipPlane / 2f);
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
    private void UpdateVitalsDisplay()
    {
        vitalsHealthUI.GetComponent<Image>().fillAmount = (float)(vitalsHealth / vitalsHealthMax);
        vitalsHealthUIText.text = vitalsHealth.ToString("F1");
        vitalsFuelUI.GetComponent<Image>().fillAmount = (float)(vitalsFuel / vitalsFuelMax);
        vitalsFuelUIText.text = vitalsFuel.ToString("F1");
    }

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
        //Cooldowns
        playerWeaponCooldowns();
        
        //Fire
        if
        (
            !destroyed
            && Application.isFocused
            && !Menu.menuOpenAndGamePaused
            && !Commerce.menuOpen
            && !UI.displayMap
            && binds.GetInput(binds.bindPrimaryFire)
            && weaponLaserSingleCooldownCurrent <= 0f
            && weaponLaserClipCooldownCurrent <= 0f
        )
        {
            FireWeaponLaser();
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

        //Save follow distance to user settings file
        control.settings.Save();

        //Check if should be first-person, third-person, or no person!
        DecideWhichModelsToRender();
    }

    private void DecideWhichModelsToRender()
    {
        //Defaults (will be values if destroyed)
        bool firstPerson = false;
        bool thirdPerson = false;

        if (!destroyed)
        {
            firstPerson = control.settings.cameraDistance <= control.settings.CAMERA_DISTANCE_MIN + 0.01f;
            thirdPerson = !firstPerson;
        }

        fpCam.SetActive(firstPerson);
        fpCamInterior.SetActive(firstPerson);
        fpModel.SetActive(firstPerson);

        tpCam.SetActive(thirdPerson || destroyed);
        tpModel.SetActive(thirdPerson);

        transform.Find("Spotlight").gameObject.SetActive(!destroyed);
        transform.Find("Jet Glow").gameObject.SetActive(!destroyed);
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
    
    private void ToggleMapView()
    {
        UI.displayMap = !UI.displayMap;

        control.ui.ToggleMapUI();

        if (UI.displayMap)
        {
            //Ship cameras
            fpCam.SetActive(!UI.displayMap);
            tpCam.SetActive(!UI.displayMap);

            //Map camera
            mapCam.SetActive(UI.displayMap);

            //Background stars
            //skyboxStarsParticleSystem.transform.parent = mapCam.transform;

            //Map ship model
            transform.parent.Find("Ship Map Model").gameObject.SetActive(UI.displayMap);
        }
        else
        {
            //Ship cameras
            fpCam.SetActive(!UI.displayMap);
            DecideWhichModelsToRender(); 

            //Map camera
            mapCam.SetActive(UI.displayMap);

            //Background stars
            //skyboxStarsParticleSystem.transform.parent = positionMount.transform;

            //Map ship model
            transform.parent.Find("Ship Map Model").gameObject.SetActive(UI.displayMap);
        }
    }
    #endregion

    #region General methods: Weapons
    private void FireWeaponLaser()
    {
        //Pooling
        weaponLaserPool[WeaponLaserPoolIndex].SetActive(true);
        //Ignore collisions between the laser and the player (this does not seem necessary)
        //Physics.IgnoreCollision(weaponLaserPool[WeaponLaserPoolIndex].GetComponent<Collider>(), transform.GetComponent<Collider>());
        //Reset weapon instance
        weaponLaserPool[WeaponLaserPoolIndex].transform.position = transform.position + (transform.forward * 0.14f) - (transform.up * 0.015f);
        weaponLaserPool[WeaponLaserPoolIndex].GetComponent<Rigidbody>().rotation = transform.rotation * Quaternion.Euler(90, 270, 0);
        weaponLaserPool[WeaponLaserPoolIndex].transform.rotation = transform.rotation * Quaternion.Euler(90, 270, 0);
        weaponLaserPool[WeaponLaserPoolIndex].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        weaponLaserPool[WeaponLaserPoolIndex].GetComponent<Rigidbody>().velocity = rb.velocity + (weaponLaserProjectileSpeed * transform.forward);
        weaponLaserPool[WeaponLaserPoolIndex].GetComponent<PlayerLaser>().timeAtWhichThisSelfDestructs = weaponLaserLifetimeDuration;
        weaponLaserPool[WeaponLaserPoolIndex].GetComponent<PlayerLaser>().timeSpentAlive = 0f;

        //Iterate through list
        if (WeaponLaserPoolIndex < weaponLaserPoolLength - 1)
        {
            WeaponLaserPoolIndex++;
        }
        else
        {
            WeaponLaserPoolIndex = 0;
        }

        //Cooldown & ammo
        weaponLaserSingleCooldownCurrent = weaponLaserSingleCooldownDuration;
        weaponLaserClipRemaining--;

        //UI
        control.ui.weaponSelectedClipRemainingText.text = "" + weaponLaserClipRemaining;
        
        //Play sound effect
        switch (soundSourceLaserArrayIndex)
        {
            case 0:
                soundSourceLaser0.Play();
                break;
            case 1:
                soundSourceLaser1.Play();
                break;
            case 2:
                soundSourceLaser2.Play();
                break;
            case 3:
                soundSourceLaser3.Play();
                break;
        }

        //Increment and loop sound source array
        soundSourceLaserArrayIndex++;
        if (soundSourceLaserArrayIndex > soundSourceLaserArrayLength - 1) soundSourceLaserArrayIndex = 0;
    }

    private void playerWeaponCooldowns()
    {
        //Reload
        if (binds.GetInputDown(binds.bindPrimaryReload) && weaponLaserClipRemaining != weaponLaserClipSize)
        {
            weaponLaserClipRemaining = 0;
        }

        //Single
        if (weaponLaserSingleCooldownCurrent > 0f)
        {
            weaponLaserSingleCooldownCurrent -= Time.deltaTime;
        }

        //Clip
        if (weaponLaserClipCooldownCurrent > 0f)
        {
            weaponLaserClipCooldownCurrent -= Time.deltaTime;
        }
        else
        {
            control.ui.weaponSelectedClipRemainingText.text = "" + weaponLaserClipRemaining;
        }

        //Reloading
        if (weaponLaserClipRemaining == 0)
        {
            //Play sound
            soundSourceLaserReload.Play();
            //Start cooldown
            weaponLaserClipCooldownCurrent = weaponLaserClipCooldownDuration;
            //Reset clip
            weaponLaserClipRemaining = weaponLaserClipSize;
        }

        //UI
        control.ui.weaponCooldown.fillAmount = Mathf.Max(
            0f,
            weaponLaserSingleCooldownCurrent / weaponLaserSingleCooldownDuration,
            weaponLaserClipCooldownCurrent / weaponLaserClipCooldownDuration
        );
    }
    #endregion

    #region General methods: Audio
    private void PlayMusic()
    {
        //Select the track
        float songToPlay = UnityEngine.Random.value;

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

        //Play the track
        music.Play();
        
        //Queue another song to be played 10-20 minutes after the current one finishes
        musicPlayTime = Time.time + music.clip.length + UnityEngine.Random.Range(600f, 1200f);
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
            soundSourceRocket.volume = Mathf.Min(soundRocketMaxVolume, soundSourceRocket.volume + (Time.deltaTime * soundRocketVolumeDeltaRate));

            soundSourceRocket.pitch = Mathf.Min(soundRocketMaxPitch, soundSourceRocket.pitch + (Time.deltaTime * soundRocketPitchDeltaRate));
        }
        else
        {
            soundSourceRocket.volume = Mathf.Max(0f, soundSourceRocket.volume - ((soundSourceRocket.volume * Time.deltaTime * soundRocketVolumeDeltaRate) * 32f));

            soundSourceRocket.pitch = Mathf.Max(1f, soundSourceRocket.pitch - ((soundSourceRocket.pitch * Time.deltaTime * soundRocketPitchDeltaRate) * 32f));
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
        float impactIntoleranceThreshold = 5f;
        float impactIntoleranceRange = 6f;
        float impactMaxDamage = 3f;

        Vector3 impactDeltaV = collision.relativeVelocity;

        if (impactDeltaV.magnitude >= impactIntoleranceThreshold)
        {
            //Play sound effect
            soundSourceCollision.volume = 0.05f;
            soundSourceCollision.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
            soundSourceCollision.Play();

            //Damage
            double newHealthAmount = Math.Max(
                0.0,
                vitalsHealth - Math.Min(
                    impactIntoleranceThreshold * impactIntoleranceRange,
                    impactDeltaV.magnitude
                ) / (impactIntoleranceThreshold * impactIntoleranceRange / impactMaxDamage)
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
    }

    public void DamagePlayer(double newHealthAmount, string cause)
    {
        if (!destroyed)
        {
            vitalsHealth = newHealthAmount;
            UpdateVitalsDisplay(); //force a vitals update so that you can immediately see your health change
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
