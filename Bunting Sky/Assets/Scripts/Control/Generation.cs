using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Generation : MonoBehaviour
{
    //Control
    public Control control;

    //Generation type
    [System.NonSerialized] public readonly int GENERATION_TYPE_NEW_GAME = 0;
    [System.NonSerialized] public readonly int GENERATION_TYPE_LOADED_GAME = 1;
    [System.NonSerialized] public readonly int GENERATION_TYPE_RESTARTED_GAME = 2;

    //Player
    public GameObject playerPrefab;
    [System.NonSerialized] public GameObject instancePlayer;
    [System.NonSerialized] public bool playerSpawned = false;
    private GameObject playerSpawnPlanetoid;

    //Gravity
    public static int gravityInstanceIndex = 0;

    //CBodies
    [System.NonSerialized] public GameObject instanceCentreStar;
    private readonly int PLANETOIDS_RANGE_LOW = 6;
    private readonly int PLANETOIDS_RANGE_HIGH = 10;
    private readonly int ASTEROID_CLUSTERS_RANGE_LOW = 6;
    private readonly int ASTEROID_CLUSTERS_RANGE_HIGH = 9;
    private readonly int ASTEROIDS_CONCURRENT_MINIMUM = 16;
    private readonly int ASTEROIDS_CONCURRENT_MAXIMUM = 50;

    //Verse hierarchy
    public GameObject verseSpace;
        [System.NonSerialized] public GameObject cBodies;
            public GameObject cBodyStar;

            [System.NonSerialized] public GameObject cBodiesPlanetoids;
                public GameObject cBodyPlanetoid;
                public GameObject station;

            [System.NonSerialized] public GameObject cBodiesAsteroids;
                public GameObject cBodyAsteroid;

        [System.NonSerialized] public GameObject ores;

    //public GameObject weapons;

    private void Awake()
    {
        //Verse hierarchy
        cBodies = verseSpace.transform.Find("CBodies").gameObject;
        cBodiesPlanetoids = cBodies.transform.Find("Planetoids").gameObject;
        cBodiesAsteroids = cBodies.transform.Find("Asteroids").gameObject;
        ores = verseSpace.transform.Find("Ores").gameObject;
    }

    private void Start()
    {
        //Auto load
        LoadGame();

        //Auto saving
        InvokeRepeating("SaveGame", control.AUTO_SAVE_FREQUENCY, control.AUTO_SAVE_FREQUENCY);
    }

    private void Update()
    {
        //Slow update
        if (Time.frameCount % 20 == 0)
        {
            SlowUpdate();
        }
    }

    private void SlowUpdate()
    {
        //Asteroid count manager
        //Minimum
        if (cBodiesAsteroids.transform.childCount < ASTEROIDS_CONCURRENT_MINIMUM)
        {
            //Debug.Log("Under-limit");

            GenerateCBodiesAsteroids(Random.Range(ASTEROID_CLUSTERS_RANGE_LOW, ASTEROID_CLUSTERS_RANGE_HIGH), instanceCentreStar);
        }

        //Limit
        if (cBodiesAsteroids.transform.childCount >= ASTEROIDS_CONCURRENT_MAXIMUM)
        {
            //Debug.Log("Over-limit");

            GameObject asteroidToDestroy = cBodiesAsteroids.transform.GetChild(Random.Range(0, cBodiesAsteroids.transform.childCount)).gameObject;
            Destroy(asteroidToDestroy, 0f);
        }

        //Debug.Log(cBodiesAsteroids.transform.childCount);
    }

    #region Procedural generation
    public void GenerateGame(int generationType)
    {
        if (generationType == GENERATION_TYPE_RESTARTED_GAME)
        {
            //Destroy verse
            Destroy(instanceCentreStar, 0f);
            Control.DestroyAllChildren(cBodiesPlanetoids, 0f);
            Control.DestroyAllChildren(cBodiesAsteroids, 0f);
            Control.DestroyAllChildren(ores, 0f);
            Control.DestroyAllChildren(instancePlayer.GetComponentInChildren<Player>().playerWeaponsTreeLaser, 0f);

            //Destroy player
            playerSpawned = false;
            instancePlayer.GetComponentInChildren<Player>().warningUIText.color = new Color(1f, 0f, 0f, 0f);
            Destroy(instancePlayer, 0f);
        }

        //CENTRE STAR CELESTIAL BODY
        SpawnCBodyStar(Vector3.zero, null);

        //Planetoids
        playerSpawnPlanetoid = GenerateCBodiesPlanetoidsAndGetPlayerCoords(Random.Range(PLANETOIDS_RANGE_LOW, PLANETOIDS_RANGE_HIGH), instanceCentreStar); ;

        //Asteroids
        GenerateCBodiesAsteroids(Random.Range(ASTEROID_CLUSTERS_RANGE_LOW, ASTEROID_CLUSTERS_RANGE_HIGH), instanceCentreStar);

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
        instanceCentreStar = Instantiate(
            cBodyStar,
            position,
            Quaternion.Euler(0f, 0f, 0f)
        );

        //Put in CBodies tree
        instanceCentreStar.transform.parent = cBodies.transform;

        //Set name
        if (titleOverride == null)
        {
            instanceCentreStar.GetComponent<CelestialName>().GenerateName();
        }
        else
        {
            instanceCentreStar.GetComponent<CelestialName>().title = titleOverride;
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

        playerScript.control = control;
        playerScript.cBodies = cBodies;

        playerScript.vitalsHealthUI     = control.ui.canvas.transform.Find("HUD Bottom-Left").Find("Vitals").Find("VitalsHealth").gameObject;
        playerScript.vitalsHealthUIText = control.ui.canvas.transform.Find("HUD Bottom-Left").Find("Vitals").Find("VitalsHealthText").gameObject.GetComponent<TextMeshProUGUI>();
        playerScript.vitalsFuelUI       = control.ui.canvas.transform.Find("HUD Bottom-Left").Find("Vitals").Find("VitalsFuel").gameObject;
        playerScript.vitalsFuelUIText   = control.ui.canvas.transform.Find("HUD Bottom-Left").Find("Vitals").Find("VitalsFuelText").gameObject.GetComponent<TextMeshProUGUI>();
        playerScript.warningUIText      = control.ui.canvas.transform.Find("HUD Top").Find("WarningText").gameObject.GetComponent<TextMeshProUGUI>();

        playerScript.LateStart();

        control.ui.CreatePlayerShipDirectionReticles();

        //Remember
        playerSpawned = true;
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
            instanceCBodyPlanetoid.GetComponent<CBodyPlanetoid>().control = control;
            instanceCBodyPlanetoid.GetComponent<Gravity>().control = control;

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
            clusterSize = Control.LowBiasedRandomIntSquared(4); //range of 1 to 16 (4^2 = 16)

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
                instanceCBodyAsteroidScript.control = control;
                instanceCBodyGravityScript.control = control;
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
        instanceCBodyPlanetoid.GetComponent<CBodyPlanetoid>().control = control;
        instanceCBodyPlanetoid.GetComponent<Gravity>().control = control;

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
        instanceCBodyAsteroidScript.control = control;
        instanceCBodyAsteroid.GetComponent<Gravity>().control = control;

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

    #region: Saving and loading
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

            controlCentreStarName = instanceCentreStar.GetComponent<CelestialName>().title,
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

        LevelData.SaveGame(Application.persistentDataPath + Control.userDataFolder + Control.userLevelSaveFile, data);
    }

    public void LoadGame()
    {
        LevelData.Data data = LevelData.LoadGame(Application.persistentDataPath + Control.userDataFolder + Control.userLevelSaveFile);

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
                    for (int i2 = 0; i2 < StationDocking.upgradeButtons; i2++)
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

            //Asteroids (generate)
            GenerateCBodiesAsteroids(Random.Range(ASTEROID_CLUSTERS_RANGE_LOW, ASTEROID_CLUSTERS_RANGE_HIGH), instanceCentreStar);
        }

        //Update UI to reflect loaded data
        control.ui.UpdateAllPlayerResourcesUI();
    }
    #endregion
}
