using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Control : MonoBehaviour
{
    //Physics
    public static readonly float GRAVITATIONAL_CONSTANT = 0.667408f * 1000f;// * 62.5f * 40000f;
    //In real-life, G = 6.674*10^−11 m3*kg^−1*s^−2
    //62.5 is the avg inverse of Time.deltaTime during development

    //Generate system
    public GameObject playerPrefab;
    [System.NonSerialized] public GameObject instancePlayer;
    private bool playerSpawned = false;
    private GameObject playerSpawnPlanetoid;
    public static int gravityInstanceIndex = 0;
    [System.NonSerialized] public GameObject instanceCBodyStar;
    private int planetoidsRangeLow = 6;
    private int planetoidsRangeHigh = 10;
    private int asteroidClustersRangeLow = 6;
    private int asteroidClustersRangeHigh = 9;
    [System.NonSerialized] public readonly int GENERATION_TYPE_NEW_GAME = 0;
    [System.NonSerialized] public readonly int GENERATION_TYPE_LOADED_GAME = 1;
    [System.NonSerialized] public readonly int GENERATION_TYPE_RESTARTED_GAME = 2;

    public GameObject verseSpace;
        public GameObject cBodies;
            public GameObject cBodyStar;

            public GameObject cBodiesPlanetoids;
                public GameObject cBodyPlanetoid;
                    public GameObject station;

            public GameObject cBodiesAsteroids;
                public GameObject cBodyAsteroid;

        public GameObject ore;

      //public GameObject weapons;

    //HUD
    public GameObject canvas;

    //FPS
    [System.NonSerialized] public int fps = 0;
    private readonly short FPS_PRINT_PERIOD = 60;
    public TextMeshProUGUI systemInfo;

    //Menu and cursor locking
    //public static bool windowIsFocused = true;
    public GameObject reticle;
    public Menu menu;
    public Commerce commerce;

    //Waypoint
    public Image waypoint;
    private float waypointXMin;
    private float waypointXMax;
    private float waypointYMin;
    private float waypointYMax;
    private readonly float WAYPOINT_X_OFFSET = 200f;
    private readonly float WAYPOINT_Y_OFFSET = -50f; //48f;
    private bool renderWaypoint = false;
    public TextMeshProUGUI textWaypointType;
    public TextMeshProUGUI textWaypointTitle;
    public TextMeshProUGUI textWaypointBody;

    //User data
    public KeyBinds binds;
    public Settings settings;
    public static string userDataFolder = "/user";
    public static string userLevelSaveFile = "/verse.bss"; //Bunting Sky Save
    public static string screenshotsFolder = "/screenshots";

    //Target
    public Image target;
    private float targetXMin;
    private float targetXMax;
    private float targetYMin;
    private float targetYMax;
    private bool renderTarget = false;
    private string targetTypeAndTitle = "No target";

    //Player Ship Direction Reticle
    [System.NonSerialized] public GameObject playerShipDirectionReticleTree;
    public GameObject playerShipDirectionReticle;
    private List<GameObject> playerShipDirectionReticleList = new List<GameObject>();
    private short playerShipDirectionReticleListLength = 16;
    private float playerShipDirectionReticleSpacing = 0.05f;
    private float playerShipDirectionReticleSpacingPower = 3f;
    private float playerShipDirectionReticleScale = 0.05f;
    private float playerShipDirectionReticleForwardOffset = 0.15f;

    //Player resources
    public bool updatePlayerResourcesUIAnimations = true;
    public Image imageCurrency;
    public Image imagePlatinoid;
    public Image imagePreciousMetal;
    public Image imageWater;
    public TextMeshProUGUI textCurrency;
    public TextMeshProUGUI textPlatinoid;
    public TextMeshProUGUI textPreciousMetal;
    public TextMeshProUGUI textWater;

    //Player weapons
    public TextMeshProUGUI weaponSelectedClipRemainingText;
    public TextMeshProUGUI weaponSelectedClipSizeText;
    public Image weaponCooldown;

    //Map
    [System.NonSerialized] public static bool displayMap = false;
    [System.NonSerialized] public static float mapScale = 10f;

    //Origin looping
    private readonly float ORIGIN_LOOP_DISTANCE = 20f;

    //Saving
    private readonly float AUTO_SAVE_FREQUENCY = 10f; //30f;

    private void Start()
    {
        //Waypoint
        waypointXMin = waypoint.GetPixelAdjustedRect().width / 2;
        waypointXMax = Screen.width - waypointXMin;
        waypointYMin = waypoint.GetPixelAdjustedRect().height / 2;
        waypointYMax = Screen.height - waypointYMin;

        //Target
        targetXMin = target.GetPixelAdjustedRect().width / 2;
        targetXMax = Screen.width - targetXMin;
        targetYMin = target.GetPixelAdjustedRect().height / 2;
        targetYMax = Screen.height - targetYMin;

        //FPS Target
        QualitySettings.vSyncCount = 0; //VSync
        Application.targetFrameRate = settings.targetFPS;
        fps = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);

        //Lock cursor
        Cursor.lockState = CursorLockMode.Locked;

        //Auto load
        LoadGame();

        //Auto saving
        InvokeRepeating("SaveGame", AUTO_SAVE_FREQUENCY, AUTO_SAVE_FREQUENCY);
    }

    private void Update()
    {

        //FPS target
        if (settings.targetFPS != Application.targetFrameRate) Application.targetFrameRate = settings.targetFPS;

        //FPS display
        if (settings.displayFPS)
        {
            if (Time.frameCount % FPS_PRINT_PERIOD == 0) fps = (int)(1f / Time.unscaledDeltaTime);
            systemInfo.text = fps.ToString() + "FPS";
            /*
                + "\nPosition: " + instancePlayer.transform.Find("Body").position
                + "\nPos relative verse: " + (instancePlayer.transform.Find("Body").position - verseSpace.transform.position);
            */
        }

        //Unlock cursor if game not focused
        if (!Application.isFocused)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        //Screenshot
        if (binds.GetInputDown(binds.bindSaveScreenshot)) SaveScreenshot();

        //Resources animations
        if (updatePlayerResourcesUIAnimations)
        {
            updatePlayerResourcesUIAnimations = false;
            UpdateAllPlayerResourcesUIAnimations();
        }

        //Origin looping
        if (instancePlayer.transform.Find("Body").position.magnitude > ORIGIN_LOOP_DISTANCE)
        {
            LoopWorldOrigin();
        }
    }

    private void LateUpdate()
    {
        if (playerSpawned)
        {
            //THESE MUST BE CALLED IN LateUpdate() OTHERWISE THEY WILL RENDER TOO SOON, CAUSING A SHAKING EFFECT IN-GAME
            //Waypoint
            UpdateWaypointAndTargetUI();

            //Player Ship Facing Direction Reticle
            UpdatePlayerShipFacingDirectionReticleUI();
        }
    }

    #region System generation
    public void GenerateGame(int generationType)
    {
        if (generationType == GENERATION_TYPE_RESTARTED_GAME)
        {
            //Destroy verse
            Destroy(instanceCBodyStar, 0f);
            DestroyAllChildren(cBodiesPlanetoids, 0f);
            DestroyAllChildren(cBodiesAsteroids, 0f);
            DestroyAllChildren(ore, 0f);
            DestroyAllChildren(instancePlayer.GetComponentInChildren<Player>().playerWeaponsTreeLaser, 0f);

            //Destroy player
            playerSpawned = false;
            instancePlayer.GetComponentInChildren<Player>().warningUIText.color = new Color(1f, 0f, 0f, 0f);
            Destroy(instancePlayer, 0f);
        }

        //CENTRE STAR CELESTIAL BODY
        SpawnCBodyStar(Vector3.zero, null);

        //Planetoids
        playerSpawnPlanetoid = GenerateCBodiesPlanetoidsAndGetPlayerCoords(Random.Range(planetoidsRangeLow, planetoidsRangeHigh), instanceCBodyStar); ;

        //Asteroids
        GenerateCBodiesAsteroids(Random.Range(asteroidClustersRangeLow, asteroidClustersRangeHigh), instanceCBodyStar);

        //Player
        SpawnPlayer(
            generationType,
            playerSpawnPlanetoid.transform.position + new Vector3(6f, 14f, 2f)
        );

        //Save generation (especially important for when we restart, but also good to save the type of world the player just generated if their computer crashes or something)
        SaveGame();
    }

    private void SpawnCBodyStar(Vector3 position, string titleOverride)
    {
        //Instantiate
        instanceCBodyStar = Instantiate(
            cBodyStar,
            position,
            Quaternion.Euler(0f, 0f, 0f)
        );

        //Put in CBodies tree
        instanceCBodyStar.transform.parent = cBodies.transform;

        //Set name
        if (titleOverride == null)
        {
            instanceCBodyStar.GetComponent<CelestialName>().GenerateName();
        }
        else
        {
            instanceCBodyStar.GetComponent<CelestialName>().title = titleOverride;
        }
    }

    private void SpawnPlayer(int generationType, Vector3 position)
    {
        //Instantiate at position, rotation, velocity
        instancePlayer = Instantiate(
            playerPrefab,
            Vector3.zero,
            Quaternion.identity
        );
        instancePlayer.transform.Find("Body").transform.position = position;

        if (generationType == GENERATION_TYPE_NEW_GAME || generationType == GENERATION_TYPE_RESTARTED_GAME)
        {
            instancePlayer.transform.Find("Body").transform.rotation = Quaternion.Euler(5f, 20f, 0f); //x = pitch, y = yaw, z = roll
            instancePlayer.GetComponentInChildren<Rigidbody>().velocity = playerSpawnPlanetoid.GetComponent<Rigidbody>().velocity;
        }
        
        //Script properties
        Player playerScript = instancePlayer.GetComponentInChildren<Player>();

        playerScript.control = this;
        playerScript.cBodies = cBodies;
        playerScript.vitalsHealthUI = canvas.transform.Find("HUD Bottom-Left").Find("Vitals").Find("VitalsHealth").gameObject;
        playerScript.vitalsHealthUIText = canvas.transform.Find("HUD Bottom-Left").Find("Vitals").Find("VitalsHealthText").gameObject.GetComponent<TextMeshProUGUI>();
        playerScript.vitalsFuelUI = canvas.transform.Find("HUD Bottom-Left").Find("Vitals").Find("VitalsFuel").gameObject;
        playerScript.vitalsFuelUIText = canvas.transform.Find("HUD Bottom-Left").Find("Vitals").Find("VitalsFuelText").gameObject.GetComponent<TextMeshProUGUI>();
        playerScript.warningUIText = canvas.transform.Find("HUD Top").Find("WarningText").gameObject.GetComponent<TextMeshProUGUI>();

        playerScript.LateStart();

        //UI
        CreatePlayerShipDirectionReticles();

        //Remember
        playerSpawned = true;
    }

    private void CreatePlayerShipDirectionReticles()
    {
        //Transform instancePlayerBodyTransform = instancePlayer.transform.Find("Body");

        //Create ship direction reticles
        playerShipDirectionReticleTree = new GameObject("Player Direction Reticle Tree");
        playerShipDirectionReticleTree.transform.parent = canvas.transform.Find("HUD Centre");
        playerShipDirectionReticleTree.transform.SetSiblingIndex(0); //Make sure this is drawn underneath everything else

        for (int i = 0; i < playerShipDirectionReticleListLength; i++)
        {
            //Instantiate
            GameObject instancePlayerShipDirectionReticle = Instantiate(
                playerShipDirectionReticle,
                Vector3.zero,
                Quaternion.identity
            );

            //Add to list
            playerShipDirectionReticleList.Add(instancePlayerShipDirectionReticle);

            //Put in tree
            //instancePlayerShipDirectionReticle.transform.parent = playerShipDirectionReticleTree.transform;
            instancePlayerShipDirectionReticle.transform.SetParent(playerShipDirectionReticleTree.transform, false);

            //Pass script vars
            DirectionReticle instancePlayerShipDirectionReticleScript = instancePlayerShipDirectionReticle.GetComponent<DirectionReticle>();
            instancePlayerShipDirectionReticleScript.index = i;
        }
    }

    private GameObject GenerateCBodiesPlanetoidsAndGetPlayerCoords(int nCBodiesPlanetoids, GameObject centreCBodyStar)
    {
        GameObject outPlayerSpawnPlanetoid = null;

        //Properties
        float minimumDistanceBetweenCBodies = 150f;
        float distanceOut = 1500f - minimumDistanceBetweenCBodies; //Minimum distance
        float randSpacing;
        float spawnRadius;
        float spawnAngle;

        //Spawn all
        for (int i = 0; i < nCBodiesPlanetoids; i++)
        {
            //Instance cBody
            randSpacing = Random.Range(0f, 400f) + Mathf.Pow(Random.Range(0f, 15f), 2f);
            spawnRadius = distanceOut + minimumDistanceBetweenCBodies + randSpacing;
            distanceOut = spawnRadius; //incremenet distanceOut for the next cBody
            spawnAngle = Random.Range(0f, 365f);

            GameObject instanceCBodyPlanetoid = Instantiate(
                cBodyPlanetoid,
                new Vector3(
                    Mathf.Cos(spawnAngle) * spawnRadius,
                    0f,
                    Mathf.Sin(spawnAngle) * spawnRadius
                ),
                Quaternion.Euler(
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f)
                )
            );

            //Put in CBodies tree
            instanceCBodyPlanetoid.transform.parent = cBodiesPlanetoids.transform;

            //Give control reference
            instanceCBodyPlanetoid.GetComponent<CBodyPlanetoid>().control = this;
            instanceCBodyPlanetoid.GetComponent<Gravity>().control = this;

            //Orbit central star
            instanceCBodyPlanetoid.GetComponent<Gravity>().SetVelocityToOrbit(centreCBodyStar, spawnAngle);

            //Spin
            instanceCBodyPlanetoid.GetComponent<Rigidbody>().AddTorque(Vector3.up * 6e5f * Random.Range(1f, 2f));

            //Generate name
            instanceCBodyPlanetoid.GetComponent<CelestialName>().GenerateName();

            //Spawn station
            if (i == 0)
            {
                //Force a station to spawn and return those coords to spawn the player there
                outPlayerSpawnPlanetoid = instanceCBodyPlanetoid.GetComponent<CBodyPlanetoid>().SpawnStation(true, null, true, 0f, 0f, 0f, null);
            }
            else
            {
                instanceCBodyPlanetoid.GetComponent<CBodyPlanetoid>().SpawnStation(false, null, true, 0f, 0f, 0f, null);
            }
        }

        return outPlayerSpawnPlanetoid;
    }

    private void GenerateCBodiesAsteroids(int nCBodiesAsteroidClusters, GameObject centreCBodyStar)
    {
        //Properties
        float minimumDistanceBetweenClusters = 100f;
        float distanceOut = 1300f - minimumDistanceBetweenClusters;
        float randSpacing;
        float spawnRadius;
        float spawnAngle;
        int clusterSize;
        byte clusterType;

        //Spawn all
        for (int i = 0; i < nCBodiesAsteroidClusters; i++)
        {
            //Instance cBody
            randSpacing = Random.Range(0f, 600f) + Mathf.Pow(Random.Range(0f, 15f), 2f);
            spawnRadius = distanceOut + minimumDistanceBetweenClusters + randSpacing;
            distanceOut = spawnRadius; //increment distanceOut for the next cBody
            spawnAngle = Random.Range(0f, 360f);
            clusterSize = LowBiasedRandomIntSquared(4); //range of 1 to 16 (4^2 = 16)

            //We don't have to add 1 here to format for Random.Range max being exclusive for ints because the length is already 1 greater than the index (since index starts at 0)
            clusterType = (byte)Random.Range(0, Ore.typeLength);

            for (int clusterI = 0; clusterI < clusterSize; clusterI++)
            {
                GameObject instanceCBodyAsteroid = Instantiate(
                    cBodyAsteroid,
                    new Vector3(
                        Mathf.Cos(spawnAngle) * spawnRadius,
                        0f,
                        Mathf.Sin(spawnAngle) * spawnRadius
                    ),
                    Quaternion.Euler(
                        Random.Range(0f, 360f),
                        Random.Range(0f, 360f),
                        Random.Range(0f, 360f)
                    )
                );

                //Put in CBodies tree
                instanceCBodyAsteroid.transform.parent = cBodiesAsteroids.transform;

                //Spread out within cluster
                instanceCBodyAsteroid.transform.position += 2f * new Vector3(Random.value, Random.value, Random.value);

                Gravity instanceCBodyGravityScript = instanceCBodyAsteroid.GetComponent<Gravity>();
                //Orbit central star
                instanceCBodyGravityScript.SetVelocityToOrbit(centreCBodyStar, spawnAngle);

                CBodyAsteroid instanceCBodyAsteroidScript = instanceCBodyAsteroid.GetComponent<CBodyAsteroid>();
                //Randomize size and type
                instanceCBodyAsteroidScript.SetSize(instanceCBodyAsteroidScript.RandomSize()); //MUST SET SIZE FIRST SO THAT MODEL IS SELECTED
                instanceCBodyAsteroidScript.SetType(clusterType);
                //Give control reference
                instanceCBodyAsteroidScript.control = this;
                instanceCBodyGravityScript.control = this;
            }
        }
    }

    public GameObject SpawnPlanetoidManually(Vector3 position, Vector3 velocity, string titleOverride, bool stationForced, string stationTitleOverride, bool stationGenerateOffers, float stationPricePlatinoid, float stationPricePreciousMetal, float stationPriceWater, int[] stationUpgradeIndex)
    {
        GameObject instanceCBodyPlanetoid = Instantiate(
                cBodyPlanetoid,
                position,
                Quaternion.Euler(
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f)
                )
            );

        //Put in CBodies tree
        instanceCBodyPlanetoid.transform.parent = cBodiesPlanetoids.transform;

        //Give control reference
        instanceCBodyPlanetoid.GetComponent<CBodyPlanetoid>().control = this;
        instanceCBodyPlanetoid.GetComponent<Gravity>().control = this;

        //Set velocity
        instanceCBodyPlanetoid.GetComponent<Rigidbody>().velocity = velocity;

        //Override title
        if (titleOverride == null)
        {
            instanceCBodyPlanetoid.GetComponent<CelestialName>().GenerateName();
        }
        else
        {
            instanceCBodyPlanetoid.GetComponent<CelestialName>().title = titleOverride;
        }

        //Spawn station?
        if (stationForced)
        {
            instanceCBodyPlanetoid.GetComponent<CBodyPlanetoid>().SpawnStation(
                stationForced,
                stationTitleOverride,
                stationGenerateOffers,
                stationPricePlatinoid,
                stationPricePreciousMetal,
                stationPriceWater,
                stationUpgradeIndex
            );
        }

        return instanceCBodyPlanetoid;
    }

    public GameObject SpawnAsteroidManually(Vector3 position, Vector3 velocity, bool randomType)
    {
        GameObject instanceCBodyAsteroid = Instantiate(
            cBodyAsteroid,
            position,
            Quaternion.Euler(
                Random.Range(0f, 360f),
                Random.Range(0f, 360f),
                Random.Range(0f, 360f)
            )
        );

        CBodyAsteroid instanceCBodyAsteroidScript = instanceCBodyAsteroid.GetComponent<CBodyAsteroid>();

        //Put in CBodies tree
        instanceCBodyAsteroid.transform.parent = cBodiesAsteroids.transform;

        //Give control reference
        instanceCBodyAsteroidScript.control = this;
        instanceCBodyAsteroid.GetComponent<Gravity>().control = this;

        //Set velocity
        instanceCBodyAsteroid.GetComponent<Rigidbody>().velocity = velocity;

        //Randomize size and type
        instanceCBodyAsteroidScript.SetSize(instanceCBodyAsteroidScript.RandomSize()); //MUST SET SIZE FIRST SO THAT MODEL IS SELECTED

        if (randomType)
        {
            instanceCBodyAsteroidScript.SetType((byte)Random.Range(0, Ore.typeLength));
        }
        else
        {
            instanceCBodyAsteroidScript.SetType(0);
        }
        
        return instanceCBodyAsteroid;
    }

    private void LoopWorldOrigin()
    {
        /*
         * The floating origin solution:
         * 
         * Because we are working with vast distances in space, floating point precision errors become a massive problem very quickly
         * To combat this, we loop everything back to the origin whenever the player's displacement is great enough
         * The player will be placed in the centre at (0,0,0) and all verse objects will move with the player so that the distances between them remain the same
         */

        Vector3 playerOldDistanceOut = instancePlayer.transform.Find("Body").position;
        
        //Player
        instancePlayer.transform.Find("Body").position = Vector3.zero;

        //Verse space
        verseSpace.transform.position -= playerOldDistanceOut;

        //Map camera
        instancePlayer.transform.Find("Position Mount").Find("Map Camera").position -= new Vector3(playerOldDistanceOut.x, 0f, playerOldDistanceOut.z);
    }

    private void DestroyAllChildren(GameObject parent, float timeDelay)
    {
        foreach (Transform child in parent.transform)
        {
            Destroy(child.gameObject, timeDelay);
        }
    }
    #endregion

    #region Waypoint & target UI
    private void SetWaypointUI(RaycastHit hit)
    {
        renderWaypoint = true;

        Vector3 waypointWorldPos = hit.collider.transform.position;

        Vector2 waypointUIPos = Camera.main.WorldToScreenPoint(waypointWorldPos);
        waypointUIPos.x = Mathf.Clamp(waypointUIPos.x, waypointXMin, waypointXMax);
        waypointUIPos.y = Mathf.Clamp(waypointUIPos.y, waypointYMin, waypointYMax);

        waypoint.transform.position = waypointUIPos;

        //Check if position is behind camera
        if (Vector3.Dot(waypointWorldPos - Camera.main.transform.position, Camera.main.transform.forward) < 0f)
        {
            if (waypointUIPos.x < Screen.width / 2f)
            {
                waypointUIPos.x = waypointXMax;
            }
            else
            {
                waypointUIPos.x = waypointXMin;
            }

            if (waypointUIPos.y < Screen.height / 2f)
            {
                waypointUIPos.y = waypointYMax;
            }
            else
            {
                waypointUIPos.y = waypointYMin;
            }
        }

        textWaypointType.transform.position = new Vector2(waypointUIPos.x + WAYPOINT_X_OFFSET, waypointUIPos.y + WAYPOINT_Y_OFFSET + textWaypointBody.fontSize + textWaypointTitle.fontSize + textWaypointType.fontSize);
        textWaypointTitle.transform.position = new Vector2(waypointUIPos.x + WAYPOINT_X_OFFSET, waypointUIPos.y + WAYPOINT_Y_OFFSET + textWaypointBody.fontSize + textWaypointTitle.fontSize);
        textWaypointBody.transform.position = new Vector2(waypointUIPos.x + WAYPOINT_X_OFFSET, waypointUIPos.y + WAYPOINT_Y_OFFSET + textWaypointBody.fontSize);

        //Set as target too if LMB
        if (binds.GetInputDown(binds.bindSetTarget))
        {
            //Target
            SetPlayerTargetObject(hit.collider.transform.gameObject);

            //Console
            TextMesh consoleTargetTypeAndTitleText = instancePlayer.transform.Find("Body").Find("FP Model").Find("Interior").Find("Console").Find("Target Type And Title Text").GetComponent<TextMesh>();
            targetTypeAndTitle = textWaypointType.text + "\n" + textWaypointTitle.text;
            consoleTargetTypeAndTitleText.text = targetTypeAndTitle;
        }
    }

    private void SetPlayerTargetUI()
    {
        //Cancel and remove target if object (such as an asteroid) has been destroyed
        if (instancePlayer.GetComponentInChildren<Player>().targetObject == null || (instancePlayer.GetComponentInChildren<Player>().targetObject.name == "CBodyAsteroid(Clone)" && instancePlayer.GetComponentInChildren<Player>().targetObject.GetComponent<CBodyAsteroid>().destroyed))
        {
            target.gameObject.SetActive(false);
            renderTarget = false;
            return;
        }

        Vector3 targetWorldPos = instancePlayer.GetComponentInChildren<Player>().targetObject.transform.position;

        Vector2 targetUIPos = Camera.main.WorldToScreenPoint(targetWorldPos);
        targetUIPos.x = Mathf.Clamp(targetUIPos.x, targetXMin, targetXMax);
        targetUIPos.y = Mathf.Clamp(targetUIPos.y, targetYMin, targetYMax);

        //Check if position is behind camera
        if (Vector3.Dot(targetWorldPos - Camera.main.transform.position, Camera.main.transform.forward) < 0f)
        {
            if (targetUIPos.x < Screen.width / 2f)
            {
                targetUIPos.x = targetXMax;
            }
            else
            {
                targetUIPos.x = targetXMin;
            }

            if (targetUIPos.y < Screen.height / 2f)
            {
                targetUIPos.y = targetYMax;
            }
            else
            {
                targetUIPos.y = targetYMin;
            }
        }

        target.transform.position = targetUIPos;
    }

    private void SetPlayerTargetObject(GameObject objectToTarget)
    {
        //Set or toggle the target object

        //If the target hasn't been set to anything, we don't try to check if what's selected is currently the target since we already know the result and it would throw an error
        //If the player clicks on what is currently the target, it unsets it from being the target
        //If the player clicks on what is NOT already the target, we set that as the target

        if (instancePlayer.GetComponentInChildren<Player>().targetObject == null)
        {
            instancePlayer.GetComponentInChildren<Player>().targetObject = objectToTarget;

            if (!renderTarget)
            {
                target.gameObject.SetActive(true);
                renderTarget = true;
            }
        }
        else
        {
            if (instancePlayer.GetComponentInChildren<Player>().targetObject == objectToTarget)
            {
                target.gameObject.SetActive(false);
                renderTarget = false;
                instancePlayer.GetComponentInChildren<Player>().targetObject = null;
            }
            else
            {
                instancePlayer.GetComponentInChildren<Player>().targetObject = objectToTarget;

                if (!renderTarget)
                {
                    target.gameObject.SetActive(true);
                    renderTarget = true;
                }
            }
        }
    }

    private void UpdateWaypointAndTargetUI()
    {
        //Console
        TextMesh consoleTargetInfoText = instancePlayer.transform.Find("Body").Find("FP Model").Find("Interior").Find("Console").Find("Target Info Text").GetComponent<TextMesh>();
        TextMesh consoleTargetTypeAndTitleText = instancePlayer.transform.Find("Body").Find("FP Model").Find("Interior").Find("Console").Find("Target Type And Title Text").GetComponent<TextMesh>();

        //Waypoint
        renderWaypoint = false;
        float maxDist = 10000f;
        //Transform playerFpCamMountTran = playerInstanced.transform.GetChild(0).gameObject.GetComponent<Player>().fpCamMountTran;

        Vector3 waypointRaycastOrigin;
        Vector3 waypointRaycastDirection;

        if (displayMap)
        {
            //Camera dimensions
            float orthographicHeight = 2f * Camera.main.orthographicSize;
            float orthographicWidth = orthographicHeight * Camera.main.aspect;

            //Cursor coordinate conversion
            float cursorNormalizedScreenSpaceX = ((Input.mousePosition.x - (Screen.width / 2f)) / Screen.width) * 2f;
            float cursorNormalizedScreenSpaceY = ((Input.mousePosition.y - (Screen.height / 2f)) / Screen.height) * 2f;

            float cursorWorldSpaceX = Camera.main.transform.position.x + (cursorNormalizedScreenSpaceX * orthographicWidth / 2f);
            float cursorWorldSpaceY = Camera.main.transform.position.z + (cursorNormalizedScreenSpaceY * orthographicHeight / 2f);

            //Set raycast origin
            waypointRaycastOrigin = new Vector3(
                cursorWorldSpaceX,
                Camera.main.transform.position.y,
                //we use y instead of z here because the orientation is rotated 90 degrees
                cursorWorldSpaceY
            );
            waypointRaycastDirection = Vector3.down;

            //Debug.Log("x: " + cursorNormalizedScreenSpaceX + "\ny: " + cursorNormalizedScreenSpaceY);
        }
        else
        {
            //Set raycast origin
            waypointRaycastOrigin = Camera.main.transform.position;
            waypointRaycastDirection = Camera.main.transform.forward;
        }

        if (Physics.Raycast(waypointRaycastOrigin, waypointRaycastDirection, maxDist))
        {
            RaycastHit hit = new RaycastHit();
            Physics.Raycast(waypointRaycastOrigin, waypointRaycastDirection, out hit, maxDist);
            //Debug.DrawRay(waypointRaycastOrigin, waypointRaycastDirection * hit.distance, Color.green, Time.deltaTime, false);

            if (hit.collider.gameObject.name == cBodyStar.name + "(Clone)")
            {
                //Waypoint
                textWaypointType.text = "Star";
                textWaypointTitle.text = hit.collider.gameObject.GetComponent<CelestialName>().title;
                textWaypointBody.text = GetDistanceAndDeltaV(hit.collider.gameObject, false);

                //Console waypoint
                consoleTargetTypeAndTitleText.text = textWaypointType.text + "\n" + textWaypointTitle.text;

                //Update UI
                SetWaypointUI(hit);
            }
            else if (hit.collider.gameObject.name == cBodyPlanetoid.name + "(Clone)")
            {
                //Waypoint
                textWaypointType.text = "Planetoid";
                textWaypointTitle.text = hit.collider.gameObject.GetComponent<CelestialName>().title;
                textWaypointBody.text = GetDistanceAndDeltaV(hit.collider.gameObject, false);

                //Console waypoint
                consoleTargetTypeAndTitleText.text = textWaypointType.text + "\n" + textWaypointTitle.text;

                //Update UI
                SetWaypointUI(hit);
            }
            else if (!displayMap && hit.collider.gameObject.name == station.name + "(Clone)")
            {
                //Waypoint
                textWaypointType.text = "Station";
                textWaypointTitle.text = hit.collider.gameObject.GetComponent<HumanName>().title;
                textWaypointBody.text = GetDistanceAndDeltaV(hit.collider.gameObject, true);

                //Console waypoint
                consoleTargetTypeAndTitleText.text = textWaypointType.text + "\n" + textWaypointTitle.text;

                //Update UI
                SetWaypointUI(hit);
            }
            else if (!displayMap && hit.collider.gameObject.name == cBodyAsteroid.name + "(Clone)")
            {
                //Waypoint
                textWaypointType.text = "Asteroid";
                textWaypointTitle.text = "Class: " + hit.collider.gameObject.GetComponent<CBodyAsteroid>().sizeClassDisplay;
                textWaypointBody.text = GetDistanceAndDeltaV(hit.collider.gameObject, false);

                //Console waypoint
                consoleTargetTypeAndTitleText.text = textWaypointType.text + "\n" + textWaypointTitle.text;

                //Update UI
                SetWaypointUI(hit);
            }
            else
            {
                //Debug.Log("Undefined object " + hit.collider.gameObject.name + " hit " + hit.distance + " units away");
            }
        }
        else if (instancePlayer.GetComponentInChildren<Player>().targetObject != null)
        {
            //Console target
            consoleTargetTypeAndTitleText.text = targetTypeAndTitle;
            GetDistanceAndDeltaV(instancePlayer.GetComponentInChildren<Player>().targetObject, false);
        }
        else
        {
            //Console default
            consoleTargetTypeAndTitleText.text = "No target";
            consoleTargetInfoText.text = "\n";
        }

        if (renderWaypoint)
        {
            waypoint.gameObject.SetActive(true);
            textWaypointType.gameObject.SetActive(true);
            textWaypointTitle.gameObject.SetActive(true);
            textWaypointBody.gameObject.SetActive(true);
        }
        else
        {
            waypoint.gameObject.SetActive(false);
            textWaypointType.gameObject.SetActive(false);
            textWaypointTitle.gameObject.SetActive(false);
            textWaypointBody.gameObject.SetActive(false);
        }

        //Target
        if (renderTarget)
        {
            SetPlayerTargetUI();
        }
    }

    private void UpdatePlayerShipFacingDirectionReticleUI()
    {
        for (int i = 0; i <= playerShipDirectionReticleListLength - 1; i++)
        {
            //Get references
            Transform instancePlayerBodyTransform = instancePlayer.transform.Find("Body");

            GameObject instancePlayerShipDirectionReticle = playerShipDirectionReticleList[i];
            DirectionReticle instancePlayerShipDirectionReticleScript = instancePlayerShipDirectionReticle.GetComponent<DirectionReticle>();

            //Position in front of player ship at distance relative to index
            Vector3 reticleWorldPos = instancePlayerBodyTransform.position
                + ((instancePlayerBodyTransform.rotation * Vector3.forward)
                * (playerShipDirectionReticleForwardOffset + (playerShipDirectionReticleSpacing * Mathf.Pow(1f + instancePlayerShipDirectionReticleScript.index, playerShipDirectionReticleSpacingPower)) * playerShipDirectionReticleScale)
            );

            //Transform 3D world space to 2D canvas space
            instancePlayerShipDirectionReticle.transform.position = Camera.main.WorldToScreenPoint(reticleWorldPos);

            //Don't render when behind camera
            if (Vector3.Dot(reticleWorldPos - Camera.main.transform.position, Camera.main.transform.forward) < 0f)
            {
                instancePlayerShipDirectionReticle.SetActive(false);
            }
            else
            {
                instancePlayerShipDirectionReticle.SetActive(true);
            }
        }
    }

    public void ToggleMapUI()
    {
        Cursor.lockState = (CursorLockMode)System.Convert.ToByte(!displayMap);    //toggle cursor lock
        reticle.SetActive(!displayMap);
    }

    public void UpdateAllPlayerResourcesUI()
    {
        Player playerScript = instancePlayer.transform.Find("Body").GetComponent<Player>();

        //Update values and start animations on a resource if its value changed
        UpdatePlayerResourceUI(ref textCurrency, ref imageCurrency, playerScript.currency.ToString("F2") + " ICC", playerScript.soundSourceCoins);
        UpdatePlayerResourceUI(ref textPlatinoid, ref imagePlatinoid, playerScript.ore[0].ToString("F2") + " g", playerScript.soundSourceOreCollected);
        UpdatePlayerResourceUI(ref textPreciousMetal, ref imagePreciousMetal, playerScript.ore[1].ToString("F2") + " g", playerScript.soundSourceOreCollected);
        UpdatePlayerResourceUI(ref textWater, ref imageWater, playerScript.ore[2].ToString("F2") + " g", playerScript.soundSourceOreCollected);

        //Update console
        TextMesh consoleCargoText = instancePlayer.transform.Find("Body").Find("FP Model").Find("Interior").Find("Console").Find("Cargo Text").GetComponent<TextMesh>();
        consoleCargoText.text = "Currency: " + textCurrency.text
            + "\n" + "Platinoid: " + textPlatinoid.text
            + "\n" + "Precious metal: " + textPreciousMetal.text
            + "\n" + "Water ice: " + textWater.text;

        //Set animations to update
        UpdateAllPlayerResourcesUIAnimations();
    }

    private void UpdatePlayerResourceUI(ref TextMeshProUGUI textMeshCurrent, ref Image image, string textNew, AudioSource clip)
    {
        float growAmount = 3f;

        if (textMeshCurrent.text != textNew)
        {
            //Play sound
            clip.Play();

            //Update text
            textMeshCurrent.text = textNew;

            //Start animation (grow)
            image.rectTransform.sizeDelta = new Vector2(
                (image.sprite.rect.width / 2) * growAmount,
                (image.sprite.rect.height / 2) * growAmount
            );
        }
    }

    public void UpdateAllPlayerResourcesUIAnimations()
    {
        UpdatePlayerResourcesUIAnimation(ref imageCurrency);
        UpdatePlayerResourcesUIAnimation(ref imagePlatinoid);
        UpdatePlayerResourcesUIAnimation(ref imagePreciousMetal);
        UpdatePlayerResourcesUIAnimation(ref imageWater);
    }

    private void UpdatePlayerResourcesUIAnimation(ref Image imageCurrent)
    {
        float shrinkRate = 200f;

        if (imageCurrent.rectTransform.sizeDelta.x > (imageCurrent.sprite.rect.width / 2) || imageCurrent.rectTransform.sizeDelta.y > (imageCurrent.sprite.rect.height / 2))
        {
            //Animate (shrink)
            imageCurrent.rectTransform.sizeDelta = new Vector2(
                Mathf.Max((imageCurrent.sprite.rect.width / 2), imageCurrent.rectTransform.sizeDelta.x - (Time.deltaTime * shrinkRate)),
                Mathf.Max((imageCurrent.sprite.rect.height / 2), imageCurrent.rectTransform.sizeDelta.y - (Time.deltaTime * shrinkRate))
            );

            //Loop until animation is finished
            updatePlayerResourcesUIAnimations = true;
        }
    }
    #endregion

    #region Saving
    public void SaveGame()
    {
        Debug.Log("Saving game");

        Player playerScript = instancePlayer.GetComponentInChildren<Player>();

        //World properties
        //Planetoids
        CBodyPlanetoid[] planetoidArray = FindObjectsOfType<CBodyPlanetoid>();

        float[,] controlScriptPlanetoidPosition = new float[planetoidArray.Length, 3];
        float[,] controlScriptPlanetoidVelocity = new float[planetoidArray.Length, 3];
        string[] controlScriptPlanetoidName = new string[planetoidArray.Length];
        bool[] controlScriptPlanetoidHasStation = new bool[planetoidArray.Length];

        string[] controlScriptPlanetoidStationTitle = new string[planetoidArray.Length];
        float[] controlScriptPlanetoidPricePlatinoid = new float[planetoidArray.Length];
        float[] controlScriptPlanetoidPricePreciousMetal = new float[planetoidArray.Length];
        float[] controlScriptPlanetoidPriceWater = new float[planetoidArray.Length];
        int[,] controlScriptPlanetoidStationUpgradeIndex = new int[planetoidArray.Length, StationDocking.upgradeButtons];

        byte planetoidArrayIndex = 0;
        foreach (CBodyPlanetoid planetoid in planetoidArray)
        {
            //Position
            controlScriptPlanetoidPosition[planetoidArrayIndex, 0] = planetoid.transform.position.x;
            controlScriptPlanetoidPosition[planetoidArrayIndex, 1] = planetoid.transform.position.y;
            controlScriptPlanetoidPosition[planetoidArrayIndex, 2] = planetoid.transform.position.z;

            //Velocity
            controlScriptPlanetoidVelocity[planetoidArrayIndex, 0] = planetoid.GetComponent<Rigidbody>().velocity.x;
            controlScriptPlanetoidVelocity[planetoidArrayIndex, 1] = planetoid.GetComponent<Rigidbody>().velocity.y;
            controlScriptPlanetoidVelocity[planetoidArrayIndex, 2] = planetoid.GetComponent<Rigidbody>().velocity.z;

            //Name
            controlScriptPlanetoidName[planetoidArrayIndex] = planetoid.GetComponent<CelestialName>().title;

            //Station
            controlScriptPlanetoidHasStation[planetoidArrayIndex] = planetoid.GetComponent<CBodyPlanetoid>().hasStation;
            if (planetoid.hasStation && planetoid.instancedStation != null)
            {
                controlScriptPlanetoidStationTitle[planetoidArrayIndex] = planetoid.instancedStation.GetComponent<HumanName>().title;
                controlScriptPlanetoidPricePlatinoid[planetoidArrayIndex] = planetoid.instancedStation.GetComponentInChildren<StationDocking>().pricePlatinoid;
                controlScriptPlanetoidPricePreciousMetal[planetoidArrayIndex] = planetoid.instancedStation.GetComponentInChildren<StationDocking>().pricePreciousMetal;
                controlScriptPlanetoidPriceWater[planetoidArrayIndex] = planetoid.instancedStation.GetComponentInChildren<StationDocking>().priceWater;
                //Concatenate the array so that we have the planetoid data along with the data for each upgrade offer's index
                for (int i = 0; i < StationDocking.upgradeButtons; i++)
                {
                    controlScriptPlanetoidStationUpgradeIndex[planetoidArrayIndex, i] = planetoid.instancedStation.GetComponentInChildren<StationDocking>().upgradeIndexOfButton[i];
                }
            }
            else
            {
                controlScriptPlanetoidStationTitle[planetoidArrayIndex] = null;
                controlScriptPlanetoidPricePlatinoid[planetoidArrayIndex] = 0f;
                controlScriptPlanetoidPricePreciousMetal[planetoidArrayIndex] = 0f;
                controlScriptPlanetoidPriceWater[planetoidArrayIndex] = 0f;
                //Concatenate the array so that we have the planetoid data along with the data for each upgrade offer's index
                for (int i = 0; i < StationDocking.upgradeButtons; i++)
                {
                    controlScriptPlanetoidStationUpgradeIndex[planetoidArrayIndex, i] = 0;
                }
            }
            
            
            //Increment
            planetoidArrayIndex++;
        }

        //Verse
        float[] controlScriptVersePosition = new float[3];
        controlScriptVersePosition[0] = verseSpace.transform.position.x;
        controlScriptVersePosition[1] = verseSpace.transform.position.y;
        controlScriptVersePosition[2] = verseSpace.transform.position.z;

        //Player
        float[] playerScriptPlayerPosition = new float[3];
        playerScriptPlayerPosition[0] = playerScript.transform.position.x;
        playerScriptPlayerPosition[1] = playerScript.transform.position.y;
        playerScriptPlayerPosition[2] = playerScript.transform.position.z;

        LevelData.Data data = new LevelData.Data
        {
            //World properties
            controlPlanetoidQuantity = (byte)planetoidArray.Length,
            controlPlanetoidPosition = controlScriptPlanetoidPosition,
            controlPlanetoidVelocity = controlScriptPlanetoidVelocity,
            controlPlanetoidName = controlScriptPlanetoidName,
            controlPlanetoidHasStation = controlScriptPlanetoidHasStation,

            controlPlanetoidStationTitle = controlScriptPlanetoidStationTitle,
            controlPlanetoidStationPricePlatinoid = controlScriptPlanetoidPricePlatinoid,
            controlPlanetoidStationPricePreciousMetal = controlScriptPlanetoidPricePreciousMetal,
            controlPlanetoidStationPriceWater = controlScriptPlanetoidPriceWater,
            controlPlanetoidStationUpgradeIndex = controlScriptPlanetoidStationUpgradeIndex,

            controlCentreStarName = instanceCBodyStar.GetComponent<CelestialName>().title,
            controlVerseSpacePosition = controlScriptVersePosition,

            playerPosition = playerScriptPlayerPosition,

            //Player properties
            playerThrustEngineWarmupMultiplierMax = playerScript.thrustEngineWarmupMultiplierMax,

            playerVitalsHealth = playerScript.vitalsHealth,
            playerVitalsHealthMax = playerScript.vitalsHealthMax,
            playerDestroyed = playerScript.destroyed,

            playerVitalsFuel = playerScript.vitalsFuel,
            playerVitalsFuelMax = playerScript.vitalsFuelMax,
            playerVitalsFuelConsumptionRate = playerScript.vitalsFuelConsumptionRate,

            playerCurrency = playerScript.currency,
            playerOre = playerScript.ore
        };

        LevelData.SaveGame(Application.persistentDataPath + userDataFolder + userLevelSaveFile, data);
    }

    public void LoadGame()
    {
        LevelData.Data data = LevelData.LoadGame(Application.persistentDataPath + userDataFolder + userLevelSaveFile);

        //Only load if a save file exists. If a save file doesn't exist, generate a new game
        if (data == null)
        {
            Debug.Log("No save exists; generating new game");
            GenerateGame(GENERATION_TYPE_NEW_GAME);
        }
        else
        {
            Debug.Log("Save exists; loading game");

            //VERSE
            //Centre Star
            SpawnCBodyStar(Vector3.zero, data.controlCentreStarName);

            //Verse position relative to origin
            verseSpace.transform.position = new Vector3(
                data.controlVerseSpacePosition[0],
                data.controlVerseSpacePosition[1],
                data.controlVerseSpacePosition[2]
            );

            //Planetoids
            for (byte i = 0; i < data.controlPlanetoidQuantity; i++)
            {
                if (data.controlPlanetoidHasStation[i])
                {
                    //Slice the array so that we have only the upgrade offers' indexes (since we are already looping through each planetoid)
                    int[] controlScriptPlanetoidStationUpgradeIndex = new int[StationDocking.upgradeButtons];
                    for(int i2 = 0; i2 < StationDocking.upgradeButtons; i2++)
                    {
                        controlScriptPlanetoidStationUpgradeIndex[i2] = data.controlPlanetoidStationUpgradeIndex[i, i2];
                    }

                    SpawnPlanetoidManually(
                        new Vector3(
                            data.controlPlanetoidPosition[i, 0],
                            data.controlPlanetoidPosition[i, 1],
                            data.controlPlanetoidPosition[i, 2]
                        ),
                        new Vector3(
                            data.controlPlanetoidVelocity[i, 0],
                            data.controlPlanetoidVelocity[i, 1],
                            data.controlPlanetoidVelocity[i, 2]
                        ),
                        data.controlPlanetoidName[i],
                        data.controlPlanetoidHasStation[i],
                        data.controlPlanetoidStationTitle[i],
                        false, //generate offers?
                        data.controlPlanetoidStationPricePlatinoid[i],
                        data.controlPlanetoidStationPricePreciousMetal[i],
                        data.controlPlanetoidStationPriceWater[i],
                        controlScriptPlanetoidStationUpgradeIndex
                    );
                }
                else
                {
                    SpawnPlanetoidManually(
                        new Vector3(
                            data.controlPlanetoidPosition[i, 0],
                            data.controlPlanetoidPosition[i, 1],
                            data.controlPlanetoidPosition[i, 2]
                        ),
                        new Vector3(
                            data.controlPlanetoidVelocity[i, 0],
                            data.controlPlanetoidVelocity[i, 1],
                            data.controlPlanetoidVelocity[i, 2]
                        ),
                        data.controlPlanetoidName[i],
                        data.controlPlanetoidHasStation[i],
                        null,
                        false, //generate offers?
                        0f,
                        0f,
                        0f,
                        null
                    );
                }
            }

            //PLAYER
            SpawnPlayer(
                GENERATION_TYPE_LOADED_GAME,
                new Vector3(
                    data.playerPosition[0],
                    data.playerPosition[1],
                    data.playerPosition[2]
                )
            );
            Player playerScript = instancePlayer.GetComponentInChildren<Player>();

            //Player properties
            playerScript.thrustEngineWarmupMultiplierMax = data.playerThrustEngineWarmupMultiplierMax;

            playerScript.vitalsHealth = data.playerVitalsHealth;
            playerScript.vitalsHealthMax = data.playerVitalsHealthMax;
            playerScript.destroyed = data.playerDestroyed;

            playerScript.vitalsFuel = data.playerVitalsFuel;
            playerScript.vitalsFuelMax = data.playerVitalsFuelMax;
            playerScript.vitalsFuelConsumptionRate = data.playerVitalsFuelConsumptionRate;
            
            playerScript.currency = data.playerCurrency;
            playerScript.ore = data.playerOre;
        }

        //Update UI to reflect loaded data
        UpdateAllPlayerResourcesUI();
    }

    public void SaveScreenshot()
    {
        string path;

        //Ensure save directory exists
        //User data folder
        path = Application.persistentDataPath + userDataFolder;
        if (!Directory.Exists(path))
        {
            Debug.Log("Directory does not exist; creating directory: " + path);
            Directory.CreateDirectory(path);
        }

        //Screenshots folder
        path = Application.persistentDataPath + userDataFolder + screenshotsFolder;
        if (!Directory.Exists(path))
        {
            Debug.Log("Directory does not exist; creating directory: " + path);
            Directory.CreateDirectory(path);
        }

        //Generate the filename based on time of screenshot
        //We use string formatting to ensure there are leading zeros to help system file explorers can accurately sort
        path = Application.persistentDataPath + userDataFolder + screenshotsFolder
            + "/" + System.DateTime.Now.Year
            + "-" + System.DateTime.Now.Month.ToString("d2")
            + "-" + System.DateTime.Now.Day.ToString("d2")
            + "_" + System.DateTime.Now.Hour.ToString("d2")
            + "-" + System.DateTime.Now.Minute.ToString("d2")
            + "-" + System.DateTime.Now.Second.ToString("d2")
            + "-" + System.DateTime.Now.Millisecond.ToString("d4")
            + ".png";

        ScreenCapture.CaptureScreenshot(path);
    }
    #endregion

    #region Math
    private string GetDistanceAndDeltaV(GameObject subject, bool isStation)
    {
        Transform playerTransform = instancePlayer.transform.GetChild(0);
        Transform subjectTransform = subject.transform;
        Vector3 playerVelocity = playerTransform.GetComponent<Rigidbody>().velocity;

        //Distance
        float conversionRatioUnitsToMetres = 4f; //1 Unity unit = 4 metres
        float distance = Vector3.Distance(subjectTransform.position, playerTransform.position) * conversionRatioUnitsToMetres;
        string distanceDisplay = " ?, ";
        if (distance < 1e3f)
        {
            distanceDisplay = Mathf.RoundToInt(distance) + " m";
        }
        else if (distance >= 1e3f)
        {
            distanceDisplay = (distance * 1e-3f).ToString("F2") + " km";
        }
        else if (distance >= 1e6f)
        {
            distanceDisplay = (distance * 1e-6f).ToString("F2") + " Mm";
        }
        else if (distance >= 1e9f)
        {
            distanceDisplay = (distance * 3.33564e-9f).ToString("F2") + " lightsecond";
        }
        else if (distance >= 5.5594e11f)
        {
            distanceDisplay = (distance * 5.5594e-11f).ToString("F2") + " lightminute";
        }
        else if (distance >= 1.057e16f)
        {
            distanceDisplay = (distance * 1.057e-16f).ToString("F2") + " lightyear";
        }

        //DeltaV
        Vector3 subjectVelocity;
        if (isStation)
        {
            subjectVelocity = subjectTransform.GetComponent<StationOrbit>().planetoidToOrbit.GetComponent<Rigidbody>().velocity;
        }
        else
        {
            subjectVelocity = subjectTransform.GetComponent<Rigidbody>().velocity;
        }
        float deltaV = (subjectVelocity - playerVelocity).magnitude;

        //DeltaV direction sign
        Vector3 signPlayerPosition = Vector3.zero; //playerTransform.position - playerTransform.position = 0
        Vector3 signHitPosition = subjectTransform.position - playerTransform.position;
        Vector3 signPlayerVelocity = Vector3.zero; //playerVelocity - playerVelocity = 0
        Vector3 signHitVelocity = subjectVelocity - playerVelocity;
        float signDotProduct = Vector3.Dot(
            signHitVelocity - signPlayerVelocity,
            signHitPosition - signPlayerPosition
        );
        string signPrint;
        if (signDotProduct > 0 && deltaV >= 1f)
        {
            signPrint = "-";
        }
        else if (signDotProduct < 0 && deltaV >= 1f)
        {
            signPrint = "+";
        }
        else
        {
            signPrint = "";
        }

        //Concatenate
        string deltaVDisplay = signPrint + (int)deltaV + " Δv";

        //Update console
        TextMesh consoleTargetInfoText = instancePlayer.transform.Find("Body").Find("FP Model").Find("Interior").Find("Console").Find("Target Info Text").GetComponent<TextMesh>();
        consoleTargetInfoText.text = distanceDisplay + "\n" + deltaVDisplay;

        //Return (for waypoint)
        return distanceDisplay + ", " + deltaVDisplay;
    }

    public Vector3 DragRelative(Vector3 velocityToSet, Vector3 otherVelocity, float dragCoefficient)
    {
        /*
         * This uses the same formula as for the default universe drag, except the drag is relative to the difference in velocities of the player body and the planetoid
         * We have to "add back in" the planetoid's velocity since we subtracted it out to get the deltaV
         */

        //Vector3 directionToUniverse = closestPlanetoidTransform.GetComponent<Rigidbody>().velocity.normalized;
        //Vector3 directionToPlayer = (rb.velocity - closestPlanetoidTransform.GetComponent<Rigidbody>().velocity).normalized;
        //float deltaSpeed = (rb.velocity - closestPlanetoidTransform.GetComponent<Rigidbody>().velocity).magnitude;

        Vector3 deltaV = velocityToSet - otherVelocity;
        return (deltaV * (1f - (dragCoefficient * Time.deltaTime))) + otherVelocity;
    }

    public static Vector3 RandomVector()
    {
        return 2f * (Random.value - 0.5f) * new Vector3(
            Random.value,
            Random.value,
            Random.value
        );
    }

    public static float LoopEulerAngle(float angle)
    {
        if (angle >= 360) angle -= 360;
        else if (angle < 0) angle += 360;

        return angle;
    }

    public static int LowBiasedRandomIntSquared(int valueMax)
    {
        /*
         * Randomly generates an int with a bias toward low numbers
         * (85% below the middle of the specified range and 15% above the middle of the specified range)
         * This is useful for generating asteroids with the rare chance of large clusters
         */

        //Randomize size (nneds to be shifted one to the right so that multiplication has grows the low-end number too)
        float value = Random.Range(1f, (float)valueMax);

        //Power (making distribution uneven, and unintentionally making smaller sizes rarer)
        value *= value;

        //Making larger sizes rarer by multiplying the inverse of the value by maximum value squared
        value = (1f / value) * valueMax * valueMax;

        //Round (to properly parse to int)
        value = Mathf.Round(value);

        //Return
        return (int)value;
    }

    /*
    private void TestLowBiasedRandomIntSquared()
    {
        int lows = 0;
        int highs = 0;
        int iterations = 1000 * 1000;

        for (int i = 0; i < iterations; i++)
        {
            int value = LowBiasedRandomIntSquared(4);
            //Debug.Log(value);
            if (value < 8)
            {
                lows++;
            }
            else
            {
                highs++;
            }
        }

        Debug.LogFormat("{0}% lows, {1}% highs",
            Mathf.Round(((float)lows / (float)iterations) * 100f),
            Mathf.Round(((float)highs / (float)iterations) * 100f)
        );
    }
    */

    
    #endregion
}