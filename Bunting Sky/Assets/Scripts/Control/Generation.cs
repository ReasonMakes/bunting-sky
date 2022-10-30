using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Generation : MonoBehaviour
{
    #region Init
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
    [System.NonSerialized] private GameObject playerSpawnMoon;
    [System.NonSerialized] public int playerPlanetIndex = 0; //planetary system the player is currently in

    //CBodies
    private readonly int MOONS_RANGE_LOW = 4; //6;
    private readonly int MOONS_RANGE_HIGH = 10; //10;
    [System.NonSerialized] public readonly float MOONS_DISTANCE_OUT = 300f;
    [System.NonSerialized] public readonly float MOONS_SPACING_BASE_MAX = 50f;
    [System.NonSerialized] public readonly float MOONS_SPACING_POWER = 1.5f;
    private float maxMoonDist;

    private readonly int PLANETS_RANGE_LOW = 3; //6;
    private readonly int PLANETS_RANGE_HIGH = 5; //10;
    private float planetsSpacingBaseMax;
    private float PLANETS_SPACING_POWER = 1f;
    private float maxPlanetDist;

    private readonly int CLUSTER_TYPE_VOID_CLUMP       = 0;
    private readonly int CLUSTER_TYPE_PLANET_RINGS     = 1;
    private readonly int CLUSTER_TYPE_PLANET_CLUMP     = 2;
    private readonly int CLUSTER_TYPE_MOON_RINGS       = 3;
    private readonly int CLUSTER_TYPE_MOON_CLUMP       = 4;
    public int asteroidsDetailed = 0;

    private int hitboxSwapMoonsChild = 0;
    private int hitboxSwapPlanetsChild = 0;
    private int performanceModeSwapAsteroidsChild = 0;

    //Verse hierarchy
    public GameObject verseSpace;
        public GameObject cBodies;
            public GameObject star;
            [System.NonSerialized] public GameObject instanceStarHome;
            
            public GameObject planets;
            [System.NonSerialized] public int nPlanetsPlanned;
            [System.NonSerialized] public List<List<GameObject>> planetarySystems = new List<List<GameObject>>(); //For every planet there is a list of its children, and there is a list of each planet
                public GameObject planet;
                [System.NonSerialized] public GameObject instancePlanetHome;

                public GameObject moons;
                    public GameObject moon;
                    public GameObject station;
                    public GameObject heighliner;
                        [System.NonSerialized] public int heighlinerCount = 0;
                        [System.NonSerialized] public GameObject heighlinerInitial = null;
                        [System.NonSerialized] public GameObject heighlinerOpenLinker = null;

                public GameObject asteroids;
                    public GameObject asteroidsEnabled;
                    public GameObject asteroidsDisabled;
                        public GameObject asteroid;
                        [System.NonSerialized] public List<GameObject> asteroidsPool = new List<GameObject>();

        public GameObject ores;
            public GameObject oreEnabled;
            public GameObject oreDisabled;
                public GameObject ore;
                [System.NonSerialized] public List<GameObject> orePool = new List<GameObject>();
            
        public GameObject projectiles;
            public GameObject playerProjectiles;
                public GameObject playerProjectilesLasers;
                public GameObject playerProjectilesSeismicCharges;
            public GameObject enemyProjectiles;
                public GameObject enemyProjectilesLasers;

        public GameObject enemies;
            public GameObject enemy;

        public GameObject damageParticles;

    private void Start()
    {
        //Max distances, minus some padding
        maxMoonDist = MOONS_DISTANCE_OUT + (MOONS_RANGE_HIGH * Mathf.Pow(MOONS_SPACING_BASE_MAX, MOONS_SPACING_POWER));
        planetsSpacingBaseMax = maxMoonDist * 0.25f;
        maxPlanetDist = maxMoonDist + Mathf.Pow(planetsSpacingBaseMax, PLANETS_SPACING_POWER);

        //maxMoonDist = MOONS_DISTANCE_OUT + (MOONS_RANGE_HIGH * Mathf.Pow(MOONS_SPACING_BASE_MAX, MOONS_SPACING_POWER)) - 500f;
        //planetsSpacingBaseMax = maxMoonDist;
        //maxPlanetDist = maxMoonDist + Mathf.Pow(planetsSpacingBaseMax, PLANETS_SPACING_POWER) - 1000f;

        //Auto load
        TryLoadGameElseNewGame();

        //Auto saving
        InvokeRepeating("SaveGame", control.AUTO_SAVE_FREQUENCY, control.AUTO_SAVE_FREQUENCY);
    }
    #endregion

    #region Update
    private void Update()
    {
        //Checks one object per type per frame to avoid lag spikes
        SwapHitboxes();

        //Slow update
        if (Time.frameCount % 10 == 0)
        {
            SlowUpdate();
        }

        //Very slow update
        if (Time.frameCount % control.settings.targetFPS == 0) //Every 1 second
        {
            VerySlowUpdate();
        }
    }

    private void SlowUpdate()
    {
        SwapAsteroidPerformanceMode();
        SwapEnemyPerformanceMode();
    }

    private void VerySlowUpdate()
    {
        //AsteroidManageCount();
    }
    #endregion

    #region Performance swappers
    private void SwapHitboxes()
    {
        //Mesh Collider to Sphere collider swapper (for performance)


        //MOONS CHILDREN
        //Could be a moon, station, or heighliner (all have the same children collider names)
        //The transform to check whether to swap
        Transform transformToSwap = moons.transform.GetChild(hitboxSwapMoonsChild);

        //Check which collider to use
        bool useMesh = (
            Vector3.Distance(
            instancePlayer.transform.Find("Body").position,
            transformToSwap.position
            ) < 40f
        );

        //Use proper colliders
        transformToSwap.Find("Mesh Collider").gameObject.SetActive(useMesh);
        transformToSwap.Find("Sphere Collider").gameObject.SetActive(!useMesh);

        //Increment, unless at max
        hitboxSwapMoonsChild = (hitboxSwapMoonsChild + 1) % moons.transform.childCount;


        //PLANETS CHILDREN
        //The transform to check whether to swap
        Transform planetsChildTransformToSwap = planets.transform.GetChild(hitboxSwapPlanetsChild);

        //Check which collider to use
        bool planetsChildUseMesh = (
            Vector3.Distance(
            instancePlayer.transform.Find("Body").position,
            planetsChildTransformToSwap.position
            ) < 170f
        );

        //Use proper colliders
        planetsChildTransformToSwap.Find("Mesh Collider").gameObject.SetActive(planetsChildUseMesh);
        planetsChildTransformToSwap.Find("Sphere Collider").gameObject.SetActive(!planetsChildUseMesh);

        //Increment, unless at max
        hitboxSwapPlanetsChild = (hitboxSwapPlanetsChild + 1) % planets.transform.childCount;

        ////Could be a moon, station, or heighliner (all have the same children collider names)
        //for (int i = 0; i < moons.transform.childCount; i++)
        //{
        //    //The transform to check whether to swap
        //    Transform transformToSwap = moons.transform.GetChild(i);
        //
        //    //Check which collider to use
        //    bool useMesh = (
        //        Vector3.Distance(
        //        instancePlayer.transform.Find("Body").position,
        //        transformToSwap.position
        //        ) < 40f
        //    );
        //
        //    //Use proper colliders
        //    transformToSwap.Find("Mesh Collider").gameObject.SetActive(useMesh);
        //    transformToSwap.Find("Sphere Collider").gameObject.SetActive(!useMesh);
        //}
        //
        ////Planets
        //for (int i = 0; i < planets.transform.childCount; i++)
        //{
        //    //The transform to check whether to swap
        //    Transform transformToSwap = planets.transform.GetChild(i);
        //
        //    //Check which collider to use
        //    bool useMesh = (
        //        Vector3.Distance(
        //        instancePlayer.transform.Find("Body").position,
        //        transformToSwap.position
        //        ) < 170f
        //    );
        //
        //    //Use proper colliders
        //    transformToSwap.Find("Mesh Collider").gameObject.SetActive(useMesh);
        //    transformToSwap.Find("Sphere Collider").gameObject.SetActive(!useMesh);
        //}
    }

    private void SwapAsteroidPerformanceMode()
    {
        int detailed = 0;

        for (int i = 0; i < asteroidsEnabled.transform.childCount; i++)
        {
            //The transform to check whether to swap
            Transform transformToSwap = asteroidsEnabled.transform.GetChild(i);

            //Check which performance mode to use
            bool performant = (
                Vector3.Distance(
                control.GetPlayerTransform().position,
                transformToSwap.position
                ) >= Asteroid.distanceThresholdGreaterThanPerformantMode
            );

            //Use proper performance mode
            transformToSwap.GetComponent<Asteroid>().SetPerformant(performant);

            //Keep track of how many asteroids of each type we have
            if (!performant)
            {
                detailed++;
            }
        }

        asteroidsDetailed = detailed;
    }

    private void SwapEnemyPerformanceMode()
    {
        for (int i = 0; i < enemies.transform.childCount; i++)
        {
            //The transform to check whether to swap
            Transform transformToSwap = enemies.transform.GetChild(i);

            //Check which performance mode to use
            bool performant = (
                Vector3.Distance(
                control.GetPlayerTransform().position,
                transformToSwap.position
                ) >= Enemy.DISTANCE_THRESHOLD_GREATER_THAN_PERFORMANT_MODE
            );

            //Use proper performance mode
            transformToSwap.GetComponent<Enemy>().SetPerformant(performant);
        }
    }
    #endregion

    public void GenerateGame(int generationType)
    {
        if (generationType == GENERATION_TYPE_RESTARTED_GAME)
        {
            //Destroy verse
            Destroy(instanceStarHome, 0f);
            Control.DestroyAllChildren(planets, 0f);
            hitboxSwapPlanetsChild = 0;
            Control.DestroyAllChildren(moons, 0f);
            hitboxSwapMoonsChild = 0;
            Control.DestroyAllChildren(asteroidsEnabled, 0f);
            Control.DestroyAllChildren(asteroidsDisabled, 0f);
            Control.DestroyAllChildren(ores, 0f);
            Control.DestroyAllChildren(playerProjectilesLasers, 0f);
            Control.DestroyAllChildren(playerProjectilesSeismicCharges, 0f);
            Control.DestroyAllChildren(enemyProjectilesLasers, 0f);
            Control.DestroyAllChildren(enemies, 0f);
            planetarySystems.Clear();

            //Destroy player
            Destroy(control.ui.playerShipDirectionReticleTree, 0f);
            control.ui.playerShipDirectionReticleList.Clear();
            playerSpawned = false;
            instancePlayer.GetComponentInChildren<Player>().warningUIText.color = new Color(1f, 0f, 0f, 0f);
            Destroy(instancePlayer, 0f);
        }

        //Asteroids
        AsteroidPoolPopulate(1500);
        OrePoolPopulate(500);

        //Home star
        StarSpawn(null);

        //Planetary system (planets > moons > heighliners, stations > player)
        nPlanetsPlanned = (PLANETS_RANGE_HIGH + PLANETS_RANGE_LOW) / 2; //the player's first system should have a very stable number of planets for progression reasons
        //nPlanetsPlanned = Random.Range(PLANETS_RANGE_LOW, PLANETS_RANGE_HIGH + 1); //extrastellar systems should have varied planet numbers
        PlanetarySystemClusterSpawn(nPlanetsPlanned, generationType);

        //Save generation (especially important for when we restart, but also good to save the type of world the player just generated if their computer crashes or something)
        SaveGame();
    }

    #region Stars
    private void StarSpawn(string titleOverride)
    {
        //Instantiate
        instanceStarHome = Instantiate(
            star,
            Vector3.zero,
            Quaternion.Euler(0f, 0f, 0f)
        );

        //Put in CBodies tree
        instanceStarHome.transform.parent = cBodies.transform;

        //Set name
        if (titleOverride == null)
        {
            instanceStarHome.GetComponent<NameCelestial>().GenerateName();
        }
        else
        {
            instanceStarHome.GetComponent<NameCelestial>().title = titleOverride;
        }

        //Set light range
        instanceStarHome.GetComponentInChildren<Light>().range = maxPlanetDist * 2f;
    }
    #endregion

    #region Planets
    private void PlanetarySystemClusterSpawn(int nPlanets, int generationType)
    {
        //Spawn all planetary systems

        //Get positions for each planetary system
        Vector3[] position = new Vector3[nPlanets];
        position = GenerateOrbitalPositionsWithReservedAngles(nPlanets, (int)(nPlanets * 1.5f), 2f, 3800f, 500f);

        //Spawn each planetary system
        for (int planetaryIndex = 0; planetaryIndex < nPlanets; planetaryIndex++)
        {
            Vector3 positionToSpawnThisPlanet = position[planetaryIndex];
            positionToSpawnThisPlanet.y += Random.value * 100f; //offset the vertical axis
            PlanetarySystemSpawnAndPlayerSpawn(generationType, planetaryIndex, nPlanets, positionToSpawnThisPlanet, null);
        }
    }

    private Vector3[] GenerateOrbitalPositionsWithReservedAngles(int nPositions, int nAngles, float angleNoiseMagnitude, float distanceOut, float distanceOutNoiseMagnitude)
    {
        /*  Returns an array of positions in a ring around (0,0), with angle reserving to prevent overlapping
         * 
         *  nPositions - how many positions to return
         *  nAngles - how many discrete, evenly divided angles to reserve for each possible position
         *            this number needs to be higher than nPositions otherwise we will not be able to generate angles for each position
         *            a higher number results in finer angles, but more chance of overlapping
         *  angleNoiseMagnitude - magnitude of random offset angle in degrees, to make a more natural generation
         *  distanceOut - how far from the centre each position should be
         *  distanceOutNoiseMagnitude - similar to angleNoiseMagnitude; adds some random noise to how far out from centre the position will generate
         */

        //Create a list of position to return later
        Vector3[] positions = new Vector3[nPositions];

        //Create a list of all available degrees, defaulting to be available
        bool[] availableAngles = new bool[nAngles];
        for (int angleIndex = 0; angleIndex < availableAngles.Length; angleIndex++)
        {
            availableAngles[angleIndex] = true;
        }

        //Loop for each position
        for (int positionIndex = 0; positionIndex < nPositions; positionIndex++)
        {
            //Try to find an available angle for this position
            float angle = 0f;
            int triesMax = 50;
            for (int tryIndex = 0; tryIndex < triesMax; tryIndex++)
            {
                int angleToCheck = (int)Random.Range(0f, (float)nAngles);
                if (availableAngles[angleToCheck])
                {
                    //Set the angle to use as no longer available for other positions to use
                    availableAngles[angleToCheck] = false;

                    //Convert from angle index to degrees (because we add random noise in degrees)
                    angle = angleToCheck * (360f / availableAngles.Length);

                    //Add random noise to the angle
                    angle += Random.value * angleNoiseMagnitude;

                    //Convert from degrees to radians, since that's what the Mathf struct uses
                    angle *= 0.01745329251f; //TauRadiansPerTurn / degreesPerTurn = 6.28 / 360;

                    //Exit the try loop - we have found a working angle for this position
                    break;
                }
                else if (tryIndex == triesMax - 1)
                {
                    //Default to a random angle if we run out of tries
                    Debug.LogError("Ran out of tries trying to find an available angle to use for this position");
                    angle = Random.Range(0f, 360f);
                }
            }

            //Generate the distance away from centre
            float instanceDistanceOut = distanceOut + (Random.value * distanceOutNoiseMagnitude);

            //Assign the position
            positions[positionIndex] = new Vector3(
                Mathf.Cos(angle) * instanceDistanceOut,
                0f,
                Mathf.Sin(angle) * instanceDistanceOut
            );
        }

        //Return the positions Vector3 array
        return positions;
    }

    private GameObject PlanetarySystemSpawnAndPlayerSpawn(int generationType, int planetarySystemIndex, int nPlanets, Vector3 position, string titleOverride)
    {
        //PLANET
        GameObject instancePlanet = PlanetSpawn(position, planetarySystemIndex, titleOverride);

        //PLANETARY SYSTEM BODIES
        if (generationType != GENERATION_TYPE_LOADED_GAME)
        {
            //Moons
            int nMoons;
            if (planetarySystemIndex == 0)
            {
                //Home system will always have a stable number of moons
                nMoons = 5; //3 minimum to complete the tutorial
            }
            else
            {
                nMoons = Random.Range(MOONS_RANGE_LOW, MOONS_RANGE_HIGH + 1);
            }
            GameObject instanceLastMoonSpawnedInCluster = MoonClusterSpawn(nMoons, planetarySystemIndex, position);

            //If home planetary system
            if (planetarySystemIndex == 0)
            {
                //Set home moon
                playerSpawnMoon = instanceLastMoonSpawnedInCluster;

                //Player
                PlayerSpawn(
                    generationType,
                    playerSpawnMoon.transform.position + new Vector3(6f, 14f, 2f)
                );

                //Asteroid belt guaranteed at player's spawn planet
                //The player's spawn system only has simple resources - the player must travel elsewhere to collect new resource types
                byte asteroidType = Asteroid.TYPE_WATER;
                if (Random.value > 0.5f)
                {
                    asteroidType = Asteroid.TYPE_PLATINOID;
                }
                AsteroidPoolSpawnCluster(
                    CLUSTER_TYPE_PLANET_RINGS,
                    asteroidType,
                    position,
                    false
                );

                //One minor bandit is guaranteed to spawn at the player's spawn planet
                EnemySpawnCluster(
                    CLUSTER_TYPE_PLANET_CLUMP,
                    position,
                    Enemy.STRENGTH_MINOR.ToString()
                );
            }
            else
            {
                //Planetary systems OTHER than player spawn
                //Asteroid belt (some percent of all planets have one)
                float nPercentAsteroidBelts = 50f;
                if (Control.GetTrueForPercentOfIndices(planetarySystemIndex, nPlanets, nPercentAsteroidBelts))
                {
                    //Any asteroid type can spawn in other planetary systems than the one the player spawns at
                    AsteroidPoolSpawnCluster(
                        CLUSTER_TYPE_PLANET_RINGS,
                        Asteroid.GetRandomType(),
                        position,
                        false
                    );
                }

                //Bandits
                float nPercentEnemies = 50f;
                if (Control.GetTrueForPercentOfIndices(planetarySystemIndex, nPlanets, nPercentEnemies))
                {
                    float roll = Random.value;
                    if (roll <= 0.5f)
                    {
                        EnemySpawnCluster(
                            CLUSTER_TYPE_PLANET_CLUMP,
                            position,
                              Enemy.STRENGTH_MAJOR.ToString()
                            + Enemy.STRENGTH_MINOR.ToString()
                        );
                    }
                    else if (roll <= 0.7f)
                    {
                        EnemySpawnCluster(
                            CLUSTER_TYPE_PLANET_CLUMP,
                            position,
                              Enemy.STRENGTH_MAJOR.ToString()
                            + Enemy.STRENGTH_MAJOR.ToString()
                            + Enemy.STRENGTH_MINOR.ToString()
                            + Enemy.STRENGTH_MINOR.ToString()
                        );
                    }
                    else if (roll <= 0.9f)
                    {
                        EnemySpawnCluster(
                            CLUSTER_TYPE_PLANET_CLUMP,
                            position,
                              Enemy.STRENGTH_ELITE.ToString()
                            + Enemy.STRENGTH_MAJOR.ToString()
                            + Enemy.STRENGTH_MINOR.ToString()
                            + Enemy.STRENGTH_MINOR.ToString()
                        );
                    }
                    else
                    {
                        EnemySpawnCluster(
                            CLUSTER_TYPE_PLANET_CLUMP,
                            position,
                              Enemy.STRENGTH_ELITE.ToString()
                            + Enemy.STRENGTH_ELITE.ToString()
                        );
                    }
                }
            }
        }

        return instancePlanet;
    }

    private GameObject PlanetSpawn(Vector3 position, int planetarySystemIndex, string titleOverride)
    {
        //Instantiate
        GameObject instancePlanet = Instantiate(
            planet,
            position,
            Quaternion.Euler(0f, 0f, 0f)
        );

        //Update outline
        //control.GetPlayerScript().UpdateOutlineMaterial(Player.CBODY_TYPE_PLANET, instancePlanet.GetComponentInChildren<MeshRenderer>().material);

        //Put in hierarchy
        instancePlanet.transform.parent = planets.transform;

        //Expand planetary systems list
        planetarySystems.Add(new List<GameObject>());

        //Add to planetary systems list
        planetarySystems[planetarySystemIndex].Add(instancePlanet);
        instancePlanet.GetComponent<PlanetarySystemBody>().planetarySystemIndex = planetarySystemIndex;

        //Set name
        if (titleOverride == null)
        {
            instancePlanet.GetComponent<NameCelestial>().GenerateName();
        }
        else
        {
            instancePlanet.GetComponent<NameCelestial>().title = titleOverride;
        }

        //Set home planet
        Planet planetScript = instancePlanet.GetComponent<Planet>();
        if (planetarySystemIndex == 0)
        {
            //This is the player's home planetary system
            instancePlanetHome = instancePlanet;

            //Set asteroids types
            planetScript.asteroidType1 = Asteroid.TYPE_PLATINOID;
            planetScript.asteroidType2 = Asteroid.TYPE_WATER;
        }
        else
        {
            //Set asteroids types
            planetScript.asteroidType1 = Asteroid.GetRandomType();
            planetScript.asteroidType2 = Asteroid.GetRandomTypeExcluding(planetScript.asteroidType1);
        }

        return instancePlanet;
    }
    #endregion

    #region Moons
    private GameObject MoonClusterSpawn(int nMoons, int planetIndex, Vector3 planetPosition)
    {
        //At the end we will return the last generated moon
        GameObject instanceMoon = null;

        //Spawn all moons in this planetary system

        //Get positions for each moon
        Vector3[] moonPositions = new Vector3[nMoons];
        moonPositions = GenerateOrbitalPositionsWithReservedAngles(nMoons, (int)(nMoons * 2f), 2f, 500f, 350f); //nMoons, (int)(nMoons * 2f), 2f, 300f, 200f

        //Spawn each moon
        for (int moonIndex = 0; moonIndex < nMoons; moonIndex++)
        {
            Vector3 instanceMoonPosition = moonPositions[moonIndex];

            //Set all moon positions to be relative to parent planet's position
            instanceMoonPosition += planetPosition;

            //Generate a slight vertical offset per moon
            instanceMoonPosition.y += Random.value * 20f;

            //Spawn this moon
            if (moonIndex == nMoons - 1)
            {
                //Player spawn moon - guarantee a space station
                instanceMoon = MoonSpawn(
                    false,
                    planetIndex, moonIndex,
                    instanceMoonPosition,
                    true,
                    null, false,
                    null, false, 0f, 0f, 0f, null
                );
            }
            else
            {
                //Not player spawn moon - station generation is randomized
                instanceMoon = MoonSpawn(
                    false,
                    planetIndex, moonIndex,
                    instanceMoonPosition,
                    true,
                    null, false,
                    null, false, 0f, 0f, 0f, null
                );
            }
                

            //Spawn asteroid belt around this moon (some percentage of moons - other than player's spawn moon - have them)
            float nPercentAsteroidBelts = 50f;
            if (moonIndex == nMoons - 1)
            {
                //Player spawn moon - water asteroids only but GUARANTEED valuables
                AsteroidPoolSpawnCluster(
                    CLUSTER_TYPE_MOON_RINGS,
                    Asteroid.TYPE_WATER,
                    instanceMoonPosition,
                    true
                );
            }
            else if (planetIndex == 0)
            {
                if (Control.GetTrueForPercentOfIndices(moonIndex, nMoons, nPercentAsteroidBelts))
                {
                    //Player spawn SYSTEM - water and platinoid only
                    byte asteroidType = Asteroid.TYPE_WATER;
                    if (Random.value > 0.5f)
                    {
                        asteroidType = Asteroid.TYPE_PLATINOID;
                    }
                    AsteroidPoolSpawnCluster(
                        CLUSTER_TYPE_MOON_RINGS,
                        asteroidType,
                        instanceMoonPosition,
                        false
                    );
                }
            }
            else if (Control.GetTrueForPercentOfIndices(moonIndex, nMoons, nPercentAsteroidBelts))
            {
                //All other systems - any asteroid type
                AsteroidPoolSpawnCluster(
                    CLUSTER_TYPE_MOON_RINGS,
                    Asteroid.GetRandomType(),
                    instanceMoonPosition,
                    false
                );
            }
        }

        //Return the last moon spawned
        return instanceMoon;
    }

    public GameObject MoonSpawn(bool loaded, int planetarySystemIndex, int moonIndex, Vector3 position,
        bool forceStation, string titleOverride, bool ifLoadingIsStation, string stationTitleOverride,
        bool stationGenerateOffers, float stationPricePlatinoid, float stationPricePreciousMetal, float stationPriceWater, int[] stationUpgradeIndex)
    {
        //Generate a moon within a planetary system and return its station's coordinates for possible use in spawning the player

        //Instantiate
        GameObject instanceMoon = Instantiate(
                moon,
                position,
                Quaternion.Euler(
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f)
                )
            );

        //Add to planetary system list
        planetarySystems[planetarySystemIndex].Add(instanceMoon);
        instanceMoon.GetComponent<PlanetarySystemBody>().planetarySystemIndex = planetarySystemIndex;

        //Put in CBodies tree
        instanceMoon.transform.parent = moons.transform;

        //Give control reference
        instanceMoon.GetComponent<Moon>().control = control;

        //Spin
        instanceMoon.GetComponent<Rigidbody>().AddTorque(Vector3.up * 6e5f * Random.Range(1f, 2f));

        //Update outline
        //control.GetPlayerScript().UpdateOutlineMaterial(Player.CBODY_TYPE_MOON, instanceMoon.GetComponentInChildren<MeshRenderer>().material);

        //Generate (or load) name
        if (titleOverride == null)
        {
            instanceMoon.GetComponent<NameCelestial>().GenerateName();
        }
        else
        {
            instanceMoon.GetComponent<NameCelestial>().title = titleOverride;
        }

        //Spawn station?
        if (loaded) //has this data been loaded from a save?
        {
            if (ifLoadingIsStation) //does the save data dictate that this moon has a station?
            {
                instanceMoon.GetComponent<Moon>().SpawnStation(
                    stationTitleOverride,
                    stationGenerateOffers,
                    stationPricePlatinoid,
                    stationPricePreciousMetal,
                    stationPriceWater,
                    stationUpgradeIndex
                );
            }
        }
        else
        {
            //Spawn player station, other stations, and system heighliner
            if (moonIndex == 0)
            {
                //Force a station to spawn and return those coords to spawn the player there (mainly for player station, but also to ensure each planetary system has at least one station)
                instanceMoon.GetComponent<Moon>().SpawnStation(null, true, 0f, 0f, 0f, null);
            }
            else if (moonIndex == 1 || moonIndex == 2)
            {
                //Heighliner
                //Force a heighliner to spawn
                instanceMoon.GetComponent<Moon>().SpawnHeighliner("Heighliner");
            }
            else if (forceStation)
            {
                //Force a station to spawn (unless a heighliner is required)
                instanceMoon.GetComponent<Moon>().SpawnStation(null, true, 0f, 0f, 0f, null);
            }
            else
            {
                //Other stations (random chance)
                if (Random.value <= 0.75)
                {
                    instanceMoon.GetComponent<Moon>().SpawnStation(null, true, 0f, 0f, 0f, null);
                }
            }
        }

        return instanceMoon;
    }
    #endregion

    #region Player
    private void PlayerSpawn(int generationType, Vector3 position)
    {
        //Instantiate at position, rotation, velocity
        instancePlayer = Instantiate(
            playerPrefab,
            Vector3.zero,
            Quaternion.identity
        );
        instancePlayer.transform.Find("Body").transform.position = position;

        Player playerScript = instancePlayer.GetComponentInChildren<Player>();

        if (generationType == GENERATION_TYPE_NEW_GAME || generationType == GENERATION_TYPE_RESTARTED_GAME)
        {
            playerScript.vitalsHealth = playerScript.vitalsHealthMax;
            playerScript.vitalsFuel = playerScript.vitalsFuelMax * 0.75d;
            playerScript.isDestroyed = false;
            //instancePlayer.transform.Find("Body").transform.rotation = Quaternion.Euler(5f, 20f, 0f); //x = pitch, y = yaw, z = roll
            instancePlayer.GetComponentInChildren<Rigidbody>().velocity = playerSpawnMoon.GetComponent<Rigidbody>().velocity;
        }

        //Script properties
        instancePlayer.GetComponentInChildren<PlayerWeaponLaser>().control = control;
        instancePlayer.GetComponentInChildren<PlayerWeaponLaser>().player = instancePlayer.GetComponentInChildren<Player>();

        instancePlayer.GetComponentInChildren<PlayerWeaponSeismicCharge>().control = control;
        instancePlayer.GetComponentInChildren<PlayerWeaponSeismicCharge>().player = instancePlayer.GetComponentInChildren<Player>();

        playerScript.control = control;
        playerScript.cBodies = cBodies;

        playerScript.vitalsHealthUI = control.ui.canvas.transform.Find("HUD Bottom-Left").Find("Vitals").Find("VitalsHealth").gameObject;
        playerScript.vitalsHealthUIText = control.ui.canvas.transform.Find("HUD Bottom-Left").Find("Vitals").Find("VitalsHealthText").gameObject.GetComponent<TextMeshProUGUI>();
        playerScript.vitalsFuelUI = control.ui.canvas.transform.Find("HUD Bottom-Left").Find("Vitals").Find("VitalsFuel").gameObject;
        playerScript.vitalsFuelUIText = control.ui.canvas.transform.Find("HUD Bottom-Left").Find("Vitals").Find("VitalsFuelText").gameObject.GetComponent<TextMeshProUGUI>();
        playerScript.warningUIText = control.ui.canvas.transform.Find("HUD Top").Find("WarningText").gameObject.GetComponent<TextMeshProUGUI>();

        playerScript.LateStart();

        control.ui.CreatePlayerShipDirectionReticles();

        //Remember
        playerSpawned = true;
    }
#endregion

    #region Asteroids
    private void AsteroidPoolPopulate(int asteroidPoolLength)
    {
        for (int nAsteroids = 0; nAsteroids < asteroidPoolLength; nAsteroids++)
        {
            //OBJECT
            //Instantiate
            GameObject instanceAsteroid = Instantiate(
                asteroid,
                Vector3.zero,
                Quaternion.identity
            );

            //SCRIPT
            Asteroid instanceAsteroidScript = instanceAsteroid.GetComponent<Asteroid>();
            //Give control reference
            instanceAsteroidScript.control = control;
            //Ignore all collisions unless explicitly enabled (once asteroid is enabled and separated from siblings)
            instanceAsteroidScript.rb.detectCollisions = false;
            
            //ORGANIZATION
            //Put in hierarchy
            instanceAsteroid.transform.parent = asteroidsDisabled.transform;
            //Add to pool
            asteroidsPool.Add(instanceAsteroid);
            //Set as disabled until needed
            instanceAsteroidScript.Disable();
        }

        //Update hierarchy names
        asteroidsEnabled.name = "Enabled (" + asteroidsEnabled.transform.childCount + ")";
        asteroidsDisabled.name = "Disabled (" + asteroidsDisabled.transform.childCount + ")";
    }

    private void OrePoolPopulate(int orePoolLength)
    {
        for (int oreIndex = 0; oreIndex < orePoolLength; oreIndex++)
        {
            //Instantiate
            GameObject instanceOre = Instantiate(
                ore,
                Vector3.zero,
                Quaternion.identity
            );

            //Control ref
            instanceOre.GetComponent<Ore>().control = control;

            //Hierarchy
            instanceOre.transform.parent = oreDisabled.transform;

            //Add to pool
            orePool.Add(instanceOre);
        }

        //Update hierarchy names
        oreEnabled.name = "Enabled (" + oreEnabled.transform.childCount + ")";
        oreDisabled.name = "Disabled (" + oreDisabled.transform.childCount + ")";
    }

    public GameObject AsteroidPoolSpawn(Vector3 position, int size, byte type)
    {
        GameObject instanceAsteroid = null;

        //If we have room in the pool to draw from
        if (asteroidsDisabled.transform.childCount > 0)
        {
            //Remember which asteroid we're working with so we can return it later
            instanceAsteroid = asteroidsDisabled.transform.GetChild(0).gameObject;

            //Enable that asteroid
            instanceAsteroid.GetComponent<Asteroid>().Enable(position, size, type);

            //Add torque
            instanceAsteroid.GetComponent<Rigidbody>().AddTorque(50f * new Vector3(
                Mathf.Sqrt(Random.value),
                Mathf.Sqrt(Random.value),
                Mathf.Sqrt(Random.value)
            ));

            //Remember movement
            Asteroid instanceAsteroidScript = instanceAsteroid.GetComponent<Asteroid>();
            instanceAsteroidScript.rbMemVel = instanceAsteroidScript.rb.velocity;
            instanceAsteroidScript.rbMemAngularVel = instanceAsteroidScript.rb.angularVelocity;

            //Update outline
            //control.GetPlayerScript().UpdateOutlineMaterial(Player.CBODY_TYPE_ASTEROID, instanceAsteroidScript.modelObject.GetComponentInChildren<MeshRenderer>().material);
        }
        else
        {
            Debug.LogError("No free asteroids!");
            //TODO: later we could either expand the pool or reuse enabled asteroids
        }

        return instanceAsteroid;
    }

    public GameObject OrePoolSpawn(Vector3 position, byte type, Vector3 parentVelocity)
    {
        GameObject instanceOre = null;

        //If we have room in the pool to draw from
        if (oreDisabled.transform.childCount > 0)
        {
            //Remember which asteroid we're working with so we can return it later
            instanceOre = oreDisabled.transform.GetChild(0).gameObject;

            //Set the position
            instanceOre.transform.position = position;

            //Enable that asteroid
            instanceOre.GetComponent<Ore>().Enable(type, parentVelocity);
        }
        else
        {
            Debug.LogError("No free ores!");
            //TODO: later we could either expand the pool or reuse enabled asteroids
        }

        return instanceOre;
    }

    private void AsteroidPoolSpawnCluster(int clusterType, byte oreType, Vector3 position, bool guaranteeValuables)
    {
        if (clusterType == CLUSTER_TYPE_PLANET_RINGS || clusterType == CLUSTER_TYPE_MOON_RINGS)
        {
            int nAsteroids = 0;
            float chancePercentOfValuableType = 0f;
            float radius = 0f;
            
            if (clusterType == CLUSTER_TYPE_PLANET_RINGS)
            {
                radius = 250f; //200 //170
                nAsteroids = 100; //70;
                if (guaranteeValuables)
                {
                    chancePercentOfValuableType = 20f;
                }
                else
                {
                    chancePercentOfValuableType = Random.Range(0f, 30f); //10f;
                }
            }
            else if (clusterType == CLUSTER_TYPE_MOON_RINGS)
            {
                radius = 70f; //60
                nAsteroids = 40; //70;
                if (guaranteeValuables)
                {
                    chancePercentOfValuableType = 30f;
                }
                else
                {
                    chancePercentOfValuableType = Random.Range(0f, 40f); //10f;
                }
            }

            float radiusRandomness = 21f; //radius * 0.28f; //0.4 //0.12
            float heightRandomness = 20f; //radius * 0.28f; //0.3 //0.24 0.12
            float angle = 0f;
            float angleRandomness = 6f;

            for (int i = 0; i < nAsteroids; i++)
            {
                //Pick radius to spawn at
                float instanceAsteroidRadius = radius + (Random.Range(0f, 2f * radiusRandomness) - radiusRandomness);

                //Pick whether clay-silicate or valuable
                byte oreToSpawnAs = Asteroid.TYPE_CLAY_SILICATE;
                if (Control.GetTrueForPercentOfIndices(i, nAsteroids, chancePercentOfValuableType))
                {
                    oreToSpawnAs = oreType;
                }

                //Spawn the asteroid
                GameObject instanceAsteroid = AsteroidPoolSpawn(
                    position + new Vector3(
                        Mathf.Cos(Mathf.Deg2Rad * angle) * instanceAsteroidRadius,
                        0f, //this needs to be 0f because we are already ADDING to the moonar position - if we were to set this to position.y we would be doubling the y position!
                        Mathf.Sin(Mathf.Deg2Rad * angle) * instanceAsteroidRadius
                    ),
                    Random.Range(0, Asteroid.SIZE_LENGTH),
                    oreToSpawnAs
                );

                //Randomly move up/down relative the stellar plane
                instanceAsteroid.transform.position = new Vector3(
                    instanceAsteroid.transform.position.x,
                    instanceAsteroid.transform.position.y + (Random.value * heightRandomness),
                    instanceAsteroid.transform.position.z
                );

                //Increment angle
                angle += (360f / (float)nAsteroids) + (Random.Range(0f, 2f * angleRandomness) - angleRandomness);
            }
        }
    }

    private void EnemySpawnCluster(int clusterType, Vector3 position, string list)
    {
        //clusterType - an enum of several spawn patterns
        //position - where that spawn pattern will centre around
        //list - how many enemies and of what strength. Example: "2100" = 1x elite bandit, 1x major bandits, and 2x minor bandits
        //Should be written as Enemy.STRENGTH_ELITE.ToString() + Enemy.STRENGTH_MAJOR.ToString() + Enemy.STRENGTH_MINOR.ToString() + Enemy.STRENGTH_MINOR.ToString()

        //Amount
        int amount = list.Length;
        
        //Spawn pattern
        if (clusterType == CLUSTER_TYPE_PLANET_CLUMP)
        {
            //Position - where around the planet will the cluster be?
            float radiusFromPlanet = 150f;
            float angleFromPlanet = Random.value * 360f;
            Vector3 positionFromPlanet = position + new Vector3(
                Mathf.Cos(Mathf.Deg2Rad * angleFromPlanet) * radiusFromPlanet,
                position.y + 30f,
                Mathf.Sin(Mathf.Deg2Rad * angleFromPlanet) * radiusFromPlanet
            );

            if (amount == 1)
            {
                //Don't bother calculating offsets if there will only be one bandit spawned
                EnemySpawn(positionFromPlanet, GetIntFromStringIndex(list, 0));
            }
            else
            {
                for (int i = 0; i < amount; i++)
                {
                    //Position - where within the cluster will this bandit be?
                    float radiusFromCluster = 10f;
                    float angleFromCluster = Random.value * 360f;
                    Vector3 positionFromCluster = positionFromPlanet + new Vector3(
                        Mathf.Cos(Mathf.Deg2Rad * angleFromCluster) * radiusFromCluster,
                        positionFromPlanet.y + Random.Range(-10f, 10f),
                        Mathf.Sin(Mathf.Deg2Rad * angleFromCluster) * radiusFromCluster
                    );

                    //Spawn the bandit
                    EnemySpawn(positionFromCluster, GetIntFromStringIndex(list, i));
                }
            }
        }
    }

    private int GetIntFromStringIndex(string str, int index)
    {
        //We are working with ASCII here, so we need to subtract the ASCII value of the first alpha numerical character in the lister
        //Ex: list[i] = '3':
        //int strength = (int)list[i] - (int)'0';
        //int strength = 51 - 48; //'0' is encoded as 48 in ASCII
        //int strength = 3;
        return (int)str[index] - (int)'0';
    }

    public GameObject EnemySpawn(Vector3 position, int strength)
    {
        GameObject instanceEnemy = Instantiate(
            enemy,
            position,
            Quaternion.identity
        );

        //Hierarchy
        instanceEnemy.transform.parent = enemies.transform;

        //Update script
        instanceEnemy.GetComponent<Enemy>().control = control;
        instanceEnemy.GetComponent<Enemy>().spawnPointRaw = position;
        instanceEnemy.GetComponent<Enemy>().Enable(position, strength);

        return instanceEnemy;
    }
    #endregion

    #region: Saving and loading
    public void SaveGame()
    {
        //INIT ARRAYS TO BE USED FOR SAVING
        //Planets
        float[,] planetPosition = new float[planetarySystems.Count, 3];
        string[] planetName = new string[planetarySystems.Count];
        byte[] planetarySystemMoonQuantity = new byte[planetarySystems.Count];
        
        //Moons
        Moon[] moonArray = FindObjectsOfType<Moon>();
        float[,] moonPosition = new float[moonArray.Length, 3];
        string[] moonName = new string[moonArray.Length];
        bool[] moonHasStation = new bool[moonArray.Length];
        
        string[] stationTitle = new string[moonArray.Length];
        float[] stationPricePlatinoid = new float[moonArray.Length];
        float[] stationPricePreciousMetal = new float[moonArray.Length];
        float[] stationPriceWater = new float[moonArray.Length];
        int[,] stationUpgradeIndex = new int[moonArray.Length, StationDocking.upgradeButtons];
        
        //ASSIGN VERSE DATA TO THOSE ARRAYS
        for (int planetaryIndex = 0; planetaryIndex < planetarySystems.Count; planetaryIndex++)
        {
            //PLANETS
            GameObject instancePlanet = planetarySystems[planetaryIndex][0]; //0 is the planet, the rest of the list is moons
        
            //Position
            planetPosition[planetaryIndex, 0] = instancePlanet.transform.position.x;
            planetPosition[planetaryIndex, 1] = instancePlanet.transform.position.y;
            planetPosition[planetaryIndex, 2] = instancePlanet.transform.position.z;
        
            //Name
            planetName[planetaryIndex] = instancePlanet.GetComponent<NameCelestial>().title;
        
            //Number of moons
            planetarySystemMoonQuantity[planetaryIndex] = (byte)planetarySystems[planetaryIndex].Count;
        
            //Go until count - 1 because 0 is the planet so we offset everything by +1, and we don't want to go over
            for (int moonIndex = 0; moonIndex < planetarySystems[planetaryIndex].Count - 1; moonIndex++)
            {
                //MOONS
                GameObject instanceMoon = planetarySystems[planetaryIndex][moonIndex + 1]; //add 1 because 0 is the planet
                Moon instanceMoonScript = instanceMoon.GetComponent<Moon>(); //TODO: sometimes this is null
        
                //Position
                moonPosition[moonIndex, 0] = instanceMoon.transform.position.x;
                moonPosition[moonIndex, 1] = instanceMoon.transform.position.y;
                moonPosition[moonIndex, 2] = instanceMoon.transform.position.z;
        
                //Name
                moonName[moonIndex] = instanceMoon.GetComponent<NameCelestial>().title;
        
                //Station
                moonHasStation[moonIndex] = instanceMoon.GetComponent<Moon>().hasStation;
                if (instanceMoonScript.hasStation && instanceMoonScript.instancedStation != null)
                {
                    stationTitle[moonIndex] = instanceMoonScript.instancedStation.GetComponent<NameHuman>().title;
                    stationPricePlatinoid[moonIndex] = instanceMoonScript.instancedStation.GetComponentInChildren<StationDocking>().pricePlatinoid;
                    stationPricePreciousMetal[moonIndex] = instanceMoonScript.instancedStation.GetComponentInChildren<StationDocking>().pricePreciousMetal;
                    stationPriceWater[moonIndex] = instanceMoonScript.instancedStation.GetComponentInChildren<StationDocking>().priceWater;
        
                    //Concatenate the array so that we have the moon data along with the data for each upgrade offer's index
                    for (int upgradeButtonIndex = 0; upgradeButtonIndex < StationDocking.upgradeButtons; upgradeButtonIndex++)
                    {
                        stationUpgradeIndex[moonIndex, upgradeButtonIndex] = instanceMoonScript.instancedStation.GetComponentInChildren<StationDocking>().upgradeIndexAtButton[upgradeButtonIndex];
                    }
                }
                else
                {
                    stationTitle[moonIndex] = null;
                    stationPricePlatinoid[moonIndex] = 0f;
                    stationPricePreciousMetal[moonIndex] = 0f;
                    stationPriceWater[moonIndex] = 0f;
        
                    //Concatenate the array so that we have the moon data along with the data for each upgrade offer's index
                    for (int upgradeButtonIndex = 0; upgradeButtonIndex < StationDocking.upgradeButtons; upgradeButtonIndex++)
                    {
                        stationUpgradeIndex[moonIndex, upgradeButtonIndex] = control.commerce.UPGRADE_SOLD_OUT;
                    }
                }
            }
        }
        
        //Asteroids
        Asteroid[] asteroidArray = FindObjectsOfType<Asteroid>();
        
        float[,] asteroidPosition = new float[asteroidArray.Length, 3];
        float[,] asteroidVelocity = new float[asteroidArray.Length, 3];
        int[] asteroidSize = new int[asteroidArray.Length];
        byte[] asteroidType = new byte[asteroidArray.Length];
        byte[] asteroidHealth = new byte[asteroidArray.Length];
        
        byte asteroidArrayIndex = 0;
        foreach (Asteroid asteroid in asteroidArray)
        {
            //Position
            asteroidPosition[asteroidArrayIndex, 0] = asteroid.transform.position.x;
            asteroidPosition[asteroidArrayIndex, 1] = asteroid.transform.position.y;
            asteroidPosition[asteroidArrayIndex, 2] = asteroid.transform.position.z;
        
            //Velocity
            asteroidVelocity[asteroidArrayIndex, 0] = asteroid.GetComponent<Rigidbody>().velocity.x;
            asteroidVelocity[asteroidArrayIndex, 1] = asteroid.GetComponent<Rigidbody>().velocity.y;
            asteroidVelocity[asteroidArrayIndex, 2] = asteroid.GetComponent<Rigidbody>().velocity.z;
        
            //Size
            asteroidSize[asteroidArrayIndex] = asteroid.size;
        
            //Type
            asteroidType[asteroidArrayIndex] = asteroid.type;
        
            //Health
            asteroidHealth[asteroidArrayIndex] = asteroid.health;
        
            //Increment
            asteroidArrayIndex++;
        }
        
        //Verse
        float[] verseSpacePosition = new float[3];
        verseSpacePosition[0] = verseSpace.transform.position.x;
        verseSpacePosition[1] = verseSpace.transform.position.y;
        verseSpacePosition[2] = verseSpace.transform.position.z;
        
        //Player
        Player playerScript = instancePlayer.GetComponentInChildren<Player>();
        float[] playerPosition = new float[3];
        playerPosition[0] = playerScript.transform.position.x;
        playerPosition[1] = playerScript.transform.position.y;
        playerPosition[2] = playerScript.transform.position.z;
        
        //SAVE TO DATA CLASS
        LevelData.Data data = new LevelData.Data();
        
        //World properties
        //Centre star
        data.starName = instanceStarHome.GetComponent<NameCelestial>().title;
        
        //Planets
        data.planetQuantity = (byte)planetarySystems.Count;
        data.planetPosition = planetPosition;
        data.planetName = planetName;
        data.planetarySystemMoonQuantity = planetarySystemMoonQuantity;
        
        //Moons
        data.moonQuantity = (byte)moonArray.Length;
        data.moonPosition = moonPosition;
        data.moonName = moonName;
        data.moonHasStation = moonHasStation;
        
        //Stations
        data.stationTitle = stationTitle;
        data.stationPricePlatinoid = stationPricePlatinoid;
        data.stationPricePreciousMetal = stationPricePreciousMetal;
        data.stationPriceWater = stationPriceWater;
        data.stationUpgradeIndex = stationUpgradeIndex;
        
        //Asteroids
        data.asteroidQuantity = asteroidArray.Length;
        data.asteroidPosition = asteroidPosition;
        data.asteroidVelocity = asteroidVelocity;
        data.asteroidSize = asteroidSize;
        data.asteroidType = asteroidType;
        data.asteroidHealth = asteroidHealth;
        
        //Verse space
        data.verseSpacePosition = verseSpacePosition;
        
        //Player properties
        data.playerPosition = playerPosition;
        
        data.playerUpgrades = playerScript.upgradeLevels;
        
        data.playerVitalsHealth = playerScript.vitalsHealth;
        data.playerDestroyed = playerScript.isDestroyed;
        
        data.playerVitalsFuel = playerScript.vitalsFuel;
        
        data.playerCurrency = playerScript.currency;
        data.playerOre = playerScript.ore;
        
        //SAVE THE CLASS/GAME
        LevelData.SaveGame(Application.persistentDataPath + Control.userDataFolder + Control.userLevelSaveFile, data);
    }

    private void TryLoadGameElseNewGame()
    {
        LevelData.Data data = LevelData.LoadGame(Application.persistentDataPath + Control.userDataFolder + Control.userLevelSaveFile);

        //Only load if a save file exists. If a save file doesn't exist, generate a new game
        //ALWAYS generate a new game if in editor
        if (data == null || control.IS_EDITOR || true) //loading temporarily disabled
        {
            //Debug.Log("No save exists; generating new game");
            GenerateGame(GENERATION_TYPE_NEW_GAME);
        }
        else
        {
            //Debug.Log("Save exists; loading game");

            //Star
            StarSpawn(data.starName);

            //Planets
            for (int planetIndex = 0; planetIndex < data.planetQuantity; planetIndex++)
            {
                GameObject instancePlanet = PlanetSpawn(
                    new Vector3(
                        data.planetPosition[planetIndex, 0],
                        data.planetPosition[planetIndex, 1],
                        data.planetPosition[planetIndex, 2]
                    ),
                    planetIndex,
                    data.planetName[planetIndex]
                );

                //GameObject instancePlanet = PlanetarySystemSpawnAndPlayerSpawn(
                //    GENERATION_TYPE_LOADED_GAME,
                //    planetIndex,
                //    0,
                //    new Vector3(
                //        data.planetPosition[planetIndex, 0],
                //        data.planetPosition[planetIndex, 1],
                //        data.planetPosition[planetIndex, 2]
                //    ),
                //    data.planetName[planetIndex]
                //);

                //Expand the list
                planetarySystems.Add(new List<GameObject>());

                //Add planet to the list
                planetarySystems[planetIndex].Add(instancePlanet);

                //Moons
                for (int moonIndex = 0; moonIndex < data.planetarySystemMoonQuantity[planetIndex]; moonIndex++)
                {
                    GameObject instanceMoon;
                    if (data.moonHasStation[moonIndex])
                    {
                        //Slice the array so that we have only the upgrade offers' indices (since we are already looping through each moon)
                        int[] controlScriptPlanetoidStationUpgradeIndex = new int[StationDocking.upgradeButtons];
                        for (int upgrade = 0; upgrade < StationDocking.upgradeButtons; upgrade++)
                        {
                            controlScriptPlanetoidStationUpgradeIndex[upgrade] = data.stationUpgradeIndex[moonIndex, upgrade];
                        }

                        instanceMoon = MoonSpawn(
                            true,
                            planetIndex,
                            moonIndex,
                            new Vector3(
                                data.moonPosition[moonIndex, 0],
                                data.moonPosition[moonIndex, 1],
                                data.moonPosition[moonIndex, 2]
                            ),
                            data.moonHasStation[moonIndex],
                            data.moonName[moonIndex],
                            data.moonHasStation[moonIndex],
                            data.stationTitle[moonIndex],
                            false, //generate offers?
                            data.stationPricePlatinoid[moonIndex],
                            data.stationPricePreciousMetal[moonIndex],
                            data.stationPriceWater[moonIndex],
                            controlScriptPlanetoidStationUpgradeIndex
                        );
                    }
                    else
                    {
                        instanceMoon = MoonSpawn(
                            true,
                            planetIndex,
                            moonIndex,
                            new Vector3(
                                data.moonPosition[moonIndex, 0],
                                data.moonPosition[moonIndex, 1],
                                data.moonPosition[moonIndex, 2]
                            ),
                            data.moonHasStation[moonIndex],
                            data.moonName[moonIndex],
                            data.moonHasStation[moonIndex],
                            null,
                            false, //generate offers?
                            0f,
                            0f,
                            0f,
                            null
                        );
                    }

                    //Add the moon to the list
                    planetarySystems[planetIndex].Add(instanceMoon);
                }
            }
            
            //Asteroids
            for (byte i = 0; i < data.asteroidQuantity; i++)
            {
                //SpawnAsteroid(
                //    new Vector3(
                //        data.asteroidPosition[i, 0],
                //        data.asteroidPosition[i, 1],
                //        data.asteroidPosition[i, 2]
                //    ),
                //    new Vector3(
                //        data.asteroidVelocity[i, 0],
                //        data.asteroidVelocity[i, 1],
                //        data.asteroidVelocity[i, 2]
                //    ),
                //    data.asteroidSize[i],
                //    data.asteroidType[i],
                //    data.asteroidHealth[i]
                //);
            }

            //PLAYER
            PlayerSpawn(
                GENERATION_TYPE_LOADED_GAME,
                new Vector3(
                    data.playerPosition[0],
                    data.playerPosition[1],
                    data.playerPosition[2]
                )
            );
            Player playerScript = instancePlayer.GetComponentInChildren<Player>();

            //Player properties
            playerScript.upgradeLevels = data.playerUpgrades;
            playerScript.UpdateUpgrades();

            playerScript.vitalsHealth = data.playerVitalsHealth;
            playerScript.isDestroyed = data.playerDestroyed;

            playerScript.vitalsFuel = data.playerVitalsFuel;

            playerScript.currency = data.playerCurrency;
            playerScript.ore = data.playerOre;

            //Verse position relative to origin
            verseSpace.transform.position = new Vector3(
                data.verseSpacePosition[0],
                data.verseSpacePosition[1],
                data.verseSpacePosition[2]
            );
        }

        //Update UI to reflect possibly loaded data
        control.ui.UpdateAllPlayerResourcesUI();
    }
    #endregion
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               