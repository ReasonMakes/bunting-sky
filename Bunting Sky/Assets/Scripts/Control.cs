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
    private Vector3 playerSpawnCoords;
    public static int gravityInstanceIndex = 0;
    [System.NonSerialized] public GameObject instanceCBodyStar;

    public GameObject cBodies;
        public GameObject cBodyStar;

        public GameObject cBodiesPlanetoids;
            public GameObject cBodyPlanetoid;
                public GameObject station;

        public GameObject cBodiesAsteroids;
            public GameObject cBodyAsteroid;

    public GameObject ore;

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
    private readonly float WAYPOINT_X_OFFSET = 150f;
    private readonly float WAYPOINT_Y_OFFSET = -75f; //48f;
    private bool renderWaypoint = false;
    public TextMeshProUGUI textWaypointType;
    public TextMeshProUGUI textWaypointTitle;
    public TextMeshProUGUI textWaypointBody;

    //User data
    public KeyBinds binds;
    public Settings settings;
    public static string userDataFolder = "/user";
    public static string screenshotsFolder = "/screenshots";

    //Target
    public Image target;
    private float targetXMin;
    private float targetXMax;
    private float targetYMin;
    private float targetYMax;
    private bool renderTarget = false;

    //Player Ship Direction Reticle
    private GameObject playerShipDirectionReticleTree;
    public GameObject playerShipDirectionReticle;
    private List<GameObject> playerShipDirectionReticleList = new List<GameObject>();
    private short playerShipDirectionReticleListLength = 13;
    private float playerShipDirectionReticleSpacing = 0.4f;
    private float playerShipDirectionReticleSpacingPower = 2f;

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

        //Generate starter system
        GenerateSystem(//Player, planetoids, asteroid clusters
            true,
            (byte)Random.Range(6, 10),
            (byte)Random.Range(6, 9)
        );
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

    private void UpdateWaypointAndTargetUI()
    {
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
            Debug.DrawRay(waypointRaycastOrigin, waypointRaycastDirection * hit.distance, Color.green, Time.deltaTime, false);

            if (hit.collider.gameObject.name == cBodyStar.name + "(Clone)")
            {
                textWaypointType.text = "Star";
                textWaypointTitle.text = hit.collider.gameObject.GetComponent<CBodyStarName>().title;
                textWaypointBody.text = GetDistanceAndDeltaV(hit, false);

                SetWaypointUI(hit);
            }
            else if (hit.collider.gameObject.name == cBodyPlanetoid.name + "(Clone)")
            {
                textWaypointType.text = "Planetoid";
                textWaypointTitle.text = hit.collider.gameObject.GetComponent<CBodyPlanetoidName>().title;
                textWaypointBody.text = GetDistanceAndDeltaV(hit, false);

                SetWaypointUI(hit);
            }
            else if (!displayMap && hit.collider.gameObject.name == station.name + "(Clone)")
            {
                textWaypointType.text = "Station";
                textWaypointTitle.text = hit.collider.gameObject.GetComponent<StationName>().title;
                textWaypointBody.text = GetDistanceAndDeltaV(hit, true);

                SetWaypointUI(hit);
            }
            else if (!displayMap && hit.collider.gameObject.name == cBodyAsteroid.name + "(Clone)")
            {
                textWaypointType.text = "Asteroid";
                textWaypointTitle.text = "Class: " + hit.collider.gameObject.GetComponent<CBodyAsteroid>().sizeClassDisplay;
                textWaypointBody.text = GetDistanceAndDeltaV(hit, false);

                SetWaypointUI(hit);
            }
            else
            {
                //Debug.Log("Undefined object " + hit.collider.gameObject.name + " hit " + hit.distance + " metres away");
            }
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
                * playerShipDirectionReticleSpacing * Mathf.Pow(1f + instancePlayerShipDirectionReticleScript.index, playerShipDirectionReticleSpacingPower)
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

    /*
    private void OnApplicationFocus(bool hasFocus)
    {
        windowIsFocused = hasFocus;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        windowIsFocused = !pauseStatus;
    }
    */

    #region System generation
    private void GenerateSystem(bool isStarter, byte nCBodiesPlanetoids, byte nCBodiesAsteroids)
    {
        //CENTRE STAR CELESTIAL BODY
        instanceCBodyStar = Instantiate(cBodyStar, new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f));
        //Put in CBodies tree
        instanceCBodyStar.transform.parent = cBodies.transform;

        //Planetoids
        playerSpawnCoords = GenerateCBodiesPlanetoidsAndGetPlayerCoords(nCBodiesPlanetoids, instanceCBodyStar);
        playerSpawnCoords += new Vector3(-1f, 2f, -5f);

        //Asteroids
        GenerateCBodiesAsteroids(nCBodiesAsteroids, instanceCBodyStar);

        //Player
        if (isStarter)  SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        instancePlayer = Instantiate(
            playerPrefab,
            Vector3.zero,
            Quaternion.identity
        );

        Transform instancePlayerBodyTransform = instancePlayer.transform.Find("Body").transform;
        instancePlayerBodyTransform.position = playerSpawnCoords;
        instancePlayerBodyTransform.rotation = Quaternion.Euler(15f, 20f, 0f); //x = pitch, y = yaw, z = roll

        Player playerScript = instancePlayer.GetComponentInChildren<Player>();

        playerScript.control = this;
        playerScript.cBodies = cBodies;
        playerScript.vitalsHealthUI = canvas.transform.Find("HUD Bottom-Left").Find("Vitals").Find("VitalsHealth").gameObject;
        playerScript.vitalsHealthUIText = canvas.transform.Find("HUD Bottom-Left").Find("Vitals").Find("VitalsHealthText").gameObject.GetComponent<TextMeshProUGUI>();
        playerScript.vitalsFuelUI = canvas.transform.Find("HUD Bottom-Left").Find("Vitals").Find("VitalsFuel").gameObject;
        playerScript.vitalsFuelUIText = canvas.transform.Find("HUD Bottom-Left").Find("Vitals").Find("VitalsFuelText").gameObject.GetComponent<TextMeshProUGUI>();
        playerScript.warningUIText = canvas.transform.Find("HUD Top").Find("WarningText").gameObject.GetComponent<TextMeshProUGUI>();

        playerScript.LateStart();

        CreatePlayerShipDirectionReticles();

        playerSpawned = true;
    }

    private void CreatePlayerShipDirectionReticles()
    {
        //Transform instancePlayerBodyTransform = instancePlayer.transform.Find("Body");

        //Create ship direction reticles
        playerShipDirectionReticleTree = new GameObject("Player Direction Reticle Tree");
        playerShipDirectionReticleTree.transform.parent = canvas.transform.Find("HUD Centre");

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

    private Vector3 GenerateCBodiesPlanetoidsAndGetPlayerCoords(byte nCBodiesPlanetoids, GameObject centreCBodyStar)
    {
        Vector3 outPlayerSpawnCoords = new Vector3(0f,0f,0f);

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

            //Spawn station
            if (i == 0)
            {
                //Force a station to spawn and return those coords to spawn the player there
                outPlayerSpawnCoords = instanceCBodyPlanetoid.GetComponent<CBodyPlanetoid>().SpawnStation(true);
            }
            else
            {
                instanceCBodyPlanetoid.GetComponent<CBodyPlanetoid>().SpawnStation(false);
            }
        }

        return outPlayerSpawnCoords;
    }

    private void GenerateCBodiesAsteroids(byte nCBodiesAsteroidClusters, GameObject centreCBodyStar)
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

    public GameObject SpawnPlanetoidManually(Vector3 position, Vector3 velocity)
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
        if (binds.GetInputDown(binds.bindSetTarget))    SetPlayerTargetObject(hit.collider.transform.gameObject);
    }

    private void SetPlayerTargetUI()
    {
        //Cancel and remove target if object (such as an asteroid) has been destroyed
        if(instancePlayer.GetComponentInChildren<Player>().targetObject == null || (instancePlayer.GetComponentInChildren<Player>().targetObject.name == "CBodyAsteroid(Clone)" && instancePlayer.GetComponentInChildren<Player>().targetObject.GetComponent<CBodyAsteroid>().destroyed))
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
    #endregion

    #region General methods
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

    public void ToggleMapUI()
    {
        Cursor.lockState = (CursorLockMode)System.Convert.ToByte(!displayMap);    //toggle cursor lock
        reticle.SetActive(!displayMap);
    }

    private string GetDistanceAndDeltaV(RaycastHit hit, bool isStation)
    {
        Transform playerTransform = instancePlayer.transform.GetChild(0);
        Transform hitTransform = hit.collider.gameObject.transform;
        Vector3 playerVelocity = playerTransform.GetComponent<Rigidbody>().velocity;

        //Distance
        float conversionRatioUnitsToMetres = 4f; //1 Unity unit = 4 metres
        float distance = Vector3.Distance(hitTransform.position, playerTransform.position) * conversionRatioUnitsToMetres;
        string distanceDisplay;
        if (distance >= 1000f)
        {
            distanceDisplay = (distance * 0.001f).ToString("F2") + " km, ";
        }
        else
        {
            distanceDisplay = (int)distance + " m, ";
        }
        

        //DeltaV
        Vector3 hitVelocity;
        if (isStation)
        {
            hitVelocity = hitTransform.GetComponent<StationOrbit>().planetoidToOrbit.GetComponent<Rigidbody>().velocity;
        }
        else
        {
            hitVelocity = hitTransform.GetComponent<Rigidbody>().velocity;
        }
        float deltaV = (hitVelocity - playerVelocity).magnitude;

        //DeltaV direction sign
        Vector3 signPlayerPosition = Vector3.zero; //playerTransform.position - playerTransform.position = 0
        Vector3 signHitPosition = hitTransform.position - playerTransform.position;
        Vector3 signPlayerVelocity = Vector3.zero; //playerVelocity - playerVelocity = 0
        Vector3 signHitVelocity = hitVelocity - playerVelocity;
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

        //Print
        string deltaVDisplay = signPrint + (int)deltaV + " Δv";

        //Return
        return distanceDisplay + deltaVDisplay;
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

    public static float ClampEulerAngle(float angle)
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

    public void UpdateAllPlayerResourcesUI()
    {
        Player playerScript = instancePlayer.transform.Find("Body").GetComponent<Player>();

        //Update values and start animations on a resource if its value changed
        UpdatePlayerResourceUI(ref textCurrency,      ref imageCurrency,      playerScript.currency.ToString("F2") + " ICC", playerScript.soundSourceCoins);
        UpdatePlayerResourceUI(ref textPlatinoid,     ref imagePlatinoid,     playerScript.ore[0].ToString("F2")   + " g",   playerScript.soundSourceOreCollected);
        UpdatePlayerResourceUI(ref textPreciousMetal, ref imagePreciousMetal, playerScript.ore[1].ToString("F2")   + " g",   playerScript.soundSourceOreCollected);
        UpdatePlayerResourceUI(ref textWater,         ref imageWater,         playerScript.ore[2].ToString("F2")   + " g",   playerScript.soundSourceOreCollected);

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
}