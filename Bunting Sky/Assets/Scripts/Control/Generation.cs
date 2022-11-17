using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Generation : MonoBehaviour
{
    #region Init
    //Control
    public Control control;

    //Generation types
    public enum GenerationType
    {
        none,
        newGame,
        loaded,
        restarted
    };
    private GenerationType genTypeQueued = GenerationType.none;
    private enum ClusterType
    {
        voidClump,
        planetRing,
        planetClump,
        moonRing,
        moonClump
    };

    //Generation amounts & distances
    public struct GenerationParamsCBody //because this is more than 16 bytes it should really be a class
    {
        public readonly int COUNT_RANGE_LOW;
        public readonly int COUNT_RANGE_HIGH;
        public readonly float DISTANCE_OUT;
        public readonly float DISTANCE_OUT_NOISE;
        public readonly float ANGLE_NOISE;
        public readonly float ANGLE_PADDING;

        public readonly int ASTEROID_COUNT;
        public readonly float ASTEROID_DISTANCE_OUT;
        public readonly float ASTEROID_DISTANCE_OUT_NOISE;
        public readonly float ASTEROID_PERCENT_VALUABLE_TYPE_RANGE_LOW;
        public readonly float ASTEROID_PERCENT_VALUABLE_TYPE_RANGE_HIGH;
        
        public GenerationParamsCBody
        (
            int rangeLow, int rangeHigh,
            float distanceOut, float distanceOutNoise,
            float angleNoise, float anglePadding,

            int asteroidCount,
            float asteroidDistanceOut, float asteroidDistanceOutNoise,
            float asteroidPercentValuableTypeRangeLow, float asteroidPercentValuableTypeRangeHigh
        )
        {
            COUNT_RANGE_LOW = rangeLow;
            COUNT_RANGE_HIGH = rangeHigh;
            DISTANCE_OUT = distanceOut;
            DISTANCE_OUT_NOISE = distanceOutNoise;
            ANGLE_NOISE = angleNoise;
            ANGLE_PADDING = anglePadding;

            ASTEROID_COUNT = asteroidCount;
            ASTEROID_DISTANCE_OUT = asteroidDistanceOut;
            ASTEROID_DISTANCE_OUT_NOISE = asteroidDistanceOutNoise;
            ASTEROID_PERCENT_VALUABLE_TYPE_RANGE_LOW = asteroidPercentValuableTypeRangeLow;
            ASTEROID_PERCENT_VALUABLE_TYPE_RANGE_HIGH = asteroidPercentValuableTypeRangeHigh;
        }
    }
    private readonly GenerationParamsCBody GEN_PARAMS_PLANETS = new GenerationParamsCBody(
        /*int RANGE_LOW*/ 1,
        /*int RANGE_HIGH*/ 5,
        /*float DISTANCE_OUT*/ 4500f, //3000f
        /*float DISTANCE_OUT_NOISE*/ 2000f, //3800f
        /*float ANGLE_NOISE*/ 2f,
        /*float ANGLE_PADDING*/ 5f,
        /*int ASTEROID_COUNT*/ 100,
        /*float ASTEROID_DISTANCE_OUT*/ 250f,
        /*float ASTEROID_DISTANCE_OUT_NOISE*/ 21f,
        /*float ASTEROID_PERCENT_VALUABLE_TYPE_RANGE_LOW*/ 0f,
        /*float ASTEROID_PERCENT_VALUABLE_TYPE_RANGE_HIGH*/ 30f
    );
    private readonly GenerationParamsCBody GEN_PARAMS_MOONS = new GenerationParamsCBody(
        /*int RANGE_LOW*/ 1,
        /*int RANGE_HIGH*/ 5,
        /*float DISTANCE_OUT*/ 350f, //500f
        /*float DISTANCE_OUT_NOISE*/ 200f, //350f
        /*float ANGLE_NOISE*/ 2f,
        /*float ANGLE_PADDING*/ 0f,
        /*int ASTEROID_COUNT*/ 40,
        /*float ASTEROID_DISTANCE_OUT*/ 70f,
        /*float ASTEROID_DISTANCE_OUT_NOISE*/ 21f,
        /*float ASTEROID_PERCENT_VALUABLE_TYPE_RANGE_LOW*/ 0f,
        /*float ASTEROID_PERCENT_VALUABLE_TYPE_RANGE_HIGH*/ 40f
    );

    //Eclipse Vision - affected objects
    public enum HighlightableCBodyType
    {
        planet,
        moon,
        asteroid,
        station,
        heighliner,
        bandit
    };

    //Colour palette
    private readonly string[,] COLOR_PALETTE = new string[10, 4]
    {
        {"6184D8", "50C5B7", "9CEC5B", "F0F465"},
        {"53DD6C", "63A088", "6F5E53", "8980F5"},
        {"788AA3", "92B6B1", "B2C9AB", "E8DDB5"},
        {"FA8334", "FFFD77", "FFE882", "388697"},
        {"8D8D92", "BEB2C8", "D7D6D6", "53D8FB"},
        {"FFEDDF", "C5D86D", "86615C", "AFE0CE"},
        {"BA1200", "9DD1F1", "508AA8", "C8E0F4"},
        {"7E8287", "9DA39A", "B98389", "DB2955"},
        {"A99F96", "DDA77B", "945D5E", "474B24"},
        {"F06543", "E8E9EB", "E0DFD5", "F09D51"}
    };

    //Player
    public GameObject playerPrefab;
    [System.NonSerialized] public GameObject instancePlayer;
    [System.NonSerialized] public bool playerSpawned = false;

    //Performance swaps
    public int asteroidsDetailed = 0;
    private int hitboxSwapIndexMoons = 0;
    private int hitboxSwapIndexPlanets = 0;

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
                        [System.NonSerialized] public int heighlinerExitNodesSet;
                        private bool heighlinersSetup = false;

                public GameObject asteroids;
                    public GameObject asteroidsEnabled;
                    public GameObject asteroidsDisabled;
                        public GameObject asteroid;

        public GameObject ores;
            public GameObject oreEnabled;
            public GameObject oreDisabled;
                public GameObject ore;
            
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
        //Auto load
        TryLoadGameElseNewGame();

        //Auto saving
        //InvokeRepeating("SaveGame", control.AUTO_SAVE_FREQUENCY, control.AUTO_SAVE_FREQUENCY);
    }
    #endregion

    #region Update
    private void Update()
    {
        //Wait to generate new game (otherwise we will be spawning asteroids at the same time we are destroying them!)
        if (genTypeQueued != GenerationType.none && asteroidsEnabled.transform.childCount == 0)
        {
            GenerateGameNew(genTypeQueued);
            genTypeQueued = GenerationType.none;
        }

        //Slow update
        if (Time.frameCount % 10 == 0)
        {
            SlowUpdate();
        }
    }

    private void SlowUpdate()
    {
        //Complete generation
        //Setup all heighliners' map models and rotations
        if (!heighlinersSetup)
        {
            SetupHeighlinerMapModelsAndRotation();
        }

        //Impostering
        if (playerSpawned)
        {
            //Entity performance mode swaps
            SwapAsteroidPerformanceMode();
            SwapEnemyPerformanceMode();

            //Mesh Collider to Sphere collider swapper (for performance) - checks one object per type - doing this here to avoid lag spikes
            SwapObjectGroupHitboxes(moons, ref hitboxSwapIndexMoons, 120f);
            SwapObjectGroupHitboxes(planets, ref hitboxSwapIndexPlanets, 170f);
        }
    }

    private void VerySlowUpdate()
    {
        //Populate asteroids in new system? (Should also be called when going through heighliner)

        //AsteroidManageCount();
    }
    #endregion

    #region Generalized Mesh Swapper
    private void SwapObjectGroupHitboxes(GameObject groupForWhichToSwapHitboxes, ref int indexWithinGroup, float swapDistanceThreshold)
    {
        //The transform to check whether to swap
        Transform transformToSwap = groupForWhichToSwapHitboxes.transform.GetChild(indexWithinGroup);

        //Swap?
        bool useMesh = (
            Vector3.Distance(
            control.GetPlayerTransform().position,
            transformToSwap.position
            ) < swapDistanceThreshold
        );

        //Swap
        transformToSwap.Find("Mesh Collider").gameObject.SetActive(useMesh);
        transformToSwap.Find("Sphere Collider").gameObject.SetActive(!useMesh);

        //Increment to the next transform in the folder, unless at max
        indexWithinGroup = (indexWithinGroup + 1) % groupForWhichToSwapHitboxes.transform.childCount;
    }
    #endregion

    #region Generation Start
    public void GenerateGame(GenerationType generationType)
    {
        //Destroy the previous game's objects
        if (generationType == GenerationType.restarted)
        {
            GenerateGameDestroyPreviousGame();
        }

        //Queue generating new game until we are finished destroying the previous game's objects if restarting, or instantly if new game
        genTypeQueued = generationType;
    }

    private void GenerateGameNew(GenerationType generationType)
    {
        //Asteroids
        AsteroidPoolPopulate(1500);
        OrePoolPopulate(500);

        //Home star
        StarSpawn(null);

        //Planetary system (planets > moons > heighliners, stations > player)
        nPlanetsPlanned = 3; //the player's first system should have a very stable number of planets for progression reasons
        //nPlanetsPlanned = Random.Range(PLANETS_RANGE_LOW, PLANETS_RANGE_HIGH + 1); //extrastellar systems should have varied planet numbers
        PlanetarySystemClusterSpawn(nPlanetsPlanned, generationType);

        //Update
        control.ui.UpdateAllPlayerResourcesUI();
        control.menu.sliderVolumeAll.value = control.settings.volumeAll;
        control.menu.sliderVolumeMusic.value = control.settings.volumeMusic;
        control.menu.MenuSettingsVolumeAllUpdate();   //changing slider values will call their respective methods only IF the setting is other than default,
        control.menu.MenuSettingsVolumeMusicUpdate(); //so we call these explicitly to cover that case

        //Save
        SaveGame();
    }

    private void GenerateGameDestroyPreviousGame()
    {
        //Destroy verse
        Destroy(instanceStarHome, 0f);

        Control.DestroyAllChildren(planets, 0f);
        hitboxSwapIndexPlanets = 0;
        foreach (List<GameObject> planetarySystemChildren in planetarySystems)
        {
            planetarySystemChildren.Clear();
        }
        planetarySystems.Clear();

        Control.DestroyAllChildren(moons, 0f);
        hitboxSwapIndexMoons = 0;

        Control.DestroyAllChildren(asteroidsEnabled, 0f);
        Control.DestroyAllChildren(asteroidsDisabled, 0f);

        Control.DestroyAllChildren(oreEnabled, 0f);
        Control.DestroyAllChildren(oreDisabled, 0f);

        Control.DestroyAllChildren(playerProjectilesLasers, 0f);
        Control.DestroyAllChildren(playerProjectilesSeismicCharges, 0f);

        Control.DestroyAllChildren(enemyProjectilesLasers, 0f);

        Control.DestroyAllChildren(enemies, 0f);

        //Destroy player
        Destroy(control.ui.playerShipDirectionReticleTree, 0f);
        control.ui.playerShipDirectionReticleList.Clear();
        if (playerSpawned)
        {
            playerSpawned = false;
            instancePlayer.GetComponentInChildren<Player>().warningUIText.color = new Color(1f, 0f, 0f, 0f);
            Destroy(instancePlayer, 0f);
        }
    }
#endregion

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
        instanceStarHome.GetComponentInChildren<Light>().range = GEN_PARAMS_PLANETS.DISTANCE_OUT * 2f;
    }
    #endregion

    #region Planets
    public void SpawnPlanetarySystemEntities(int planetarySystemIndex)
    {
        //Planetary systems OTHER than player spawn
        Vector3 systemPosition = planetarySystems[planetarySystemIndex][0].transform.position;

        //Asteroid belt (some percent of all planets have one)
        float nPercentAsteroidBelts = 50f;
        if (Control.GetTrueForPercentOfIndices(planetarySystemIndex, nPlanetsPlanned, nPercentAsteroidBelts))
        {
            //Any asteroid type can spawn in other planetary systems than the one the player spawns at
            AsteroidPoolSpawnCluster(
                ClusterType.planetRing,
                Asteroid.GetRandomType(),
                systemPosition,
                false
            );
        }

        //Bandits
        float nPercentEnemies = 85f; //50f;
        if (Control.GetTrueForPercentOfIndices(planetarySystemIndex, nPlanetsPlanned, nPercentEnemies))
        {
            float roll = Random.value;
            if (roll >= 0.65f) //35% chance
            {
                //Standard squad
                EnemySpawnCluster(
                    ClusterType.planetClump,
                    systemPosition,
                      ((int)Enemy.Strength.major).ToString()
                    + ((int)Enemy.Strength.major).ToString()
                    + ((int)Enemy.Strength.minor).ToString()
                    + ((int)Enemy.Strength.minor).ToString()
                );
            }

            else if (roll >= 0.4f) //25% chance
            {
                //Elite squad
                EnemySpawnCluster(
                    ClusterType.planetClump,
                    systemPosition,
                      ((int)Enemy.Strength.elite).ToString()
                    + ((int)Enemy.Strength.major).ToString()
                    + ((int)Enemy.Strength.minor).ToString()
                    + ((int)Enemy.Strength.minor).ToString()
                );
            }
            else if (roll >= 0.15f) //25% chance
            {
                //Strike team
                EnemySpawnCluster(
                    ClusterType.planetClump,
                    systemPosition,
                      ((int)Enemy.Strength.ultra).ToString()
                    + ((int)Enemy.Strength.elite).ToString()
                    + ((int)Enemy.Strength.elite).ToString()
                );
            }
            else //15% chance
            {
                //Hoard of minors
                EnemySpawnCluster(
                    ClusterType.planetClump,
                    systemPosition,
                      ((int)Enemy.Strength.minor).ToString()
                    + ((int)Enemy.Strength.minor).ToString()
                    + ((int)Enemy.Strength.minor).ToString()
                    + ((int)Enemy.Strength.minor).ToString()
                    + ((int)Enemy.Strength.minor).ToString()
                    + ((int)Enemy.Strength.minor).ToString()
                );
            }
        }
    }

    private void PlanetarySystemClusterSpawn(int nPlanets, GenerationType generationType)
    {
        //Spawn all planetary systems

        //Get positions for each planetary system
        float radiusPlanetSystem = GEN_PARAMS_MOONS.DISTANCE_OUT + GEN_PARAMS_MOONS.DISTANCE_OUT_NOISE
            + GEN_PARAMS_MOONS.ASTEROID_DISTANCE_OUT + GEN_PARAMS_MOONS.ASTEROID_DISTANCE_OUT_NOISE;
        float radiusStarToPlanet = GEN_PARAMS_PLANETS.DISTANCE_OUT;
        
        /*
         * https://www.omnicalculator.com/math/arc-length
         * To find arc length you can use the radius and chord length:
         * 
         * 1. Divide the chord length by double the radius. c/2r
         * 2. Find the inverse sine of the result (in radians). Asin(c/2r)
         * 3. Double the result of the inverse sine to get the central angle in radians. 2Asin(c/2r)
         * 4. Once you have the central angle in radians, multiply it by the radius to get the arc length. r2Asin(c/2r)
         */
        float arcLengthPlanetSystem = radiusStarToPlanet * 2f * Mathf.Asin(radiusPlanetSystem / (2f * radiusStarToPlanet));

        float circumferencePlanetAroundStar = control.TAU * radiusStarToPlanet; //C = τr
        int nAnglesMax = System.Convert.ToInt32(Mathf.Floor(circumferencePlanetAroundStar / arcLengthPlanetSystem));
        
        Vector3[] position = new Vector3[nPlanets];
        position = GenerateOrbitalPositionsWithReservedAngles(
            nPlanets,
            nAnglesMax, //(int)(anglesMax * 2f), //(int)(nPlanets * 1.5f),
            GEN_PARAMS_PLANETS.ANGLE_NOISE + GEN_PARAMS_PLANETS.ANGLE_PADDING,
            GEN_PARAMS_PLANETS.DISTANCE_OUT,
            GEN_PARAMS_PLANETS.DISTANCE_OUT_NOISE
        );

        //Spawn each planetary system
        for (int planetaryIndex = 0; planetaryIndex < nPlanets; planetaryIndex++)
        {
            Vector3 positionToSpawnThisPlanet = position[planetaryIndex];
            positionToSpawnThisPlanet.y += Random.value * 100f; //offset the vertical axis
            PlanetarySystemSpawn(
                generationType,
                planetaryIndex, nPlanets, positionToSpawnThisPlanet,
                null,
                true
            );
        }
    }

    private GameObject PlanetarySystemSpawn(
        GenerationType generationType,
        int planetarySystemIndex, int nPlanets, Vector3 position, string titleOverride,
        bool spawnAsteroids
    )
    {
        //Includes possibly spawning the player

        //Colour scheme
        int colorPaletteIndex = Random.Range(0, COLOR_PALETTE.GetLength(0));

        //Planet
        GameObject instancePlanet = PlanetSpawn(position, planetarySystemIndex, titleOverride, colorPaletteIndex);

        //System bodies
        if (generationType != GenerationType.loaded)
        {
            //Moons
            int nMoons;
            if (control.settings.tutorial && planetarySystemIndex == 0)
            {
                //If tutorial settings is ON then we always generate minimum number needed to complete tutorial,
                //                              but no more than that so that we encourage the player to travel
                //Tutorial automatically turns itself off upon completion, so replays will have more random start systems
                //3 minimum to complete the tutorial
                nMoons = 3; //5; //Random.Range(3, MOONS_RANGE_HIGH + 1);
            }
            else
            {
                nMoons = Random.Range(GEN_PARAMS_MOONS.COUNT_RANGE_LOW, GEN_PARAMS_MOONS.COUNT_RANGE_HIGH + 1);
            }
            GameObject instanceLastMoonSpawnedInCluster = MoonClusterSpawn(
                nMoons,
                planetarySystemIndex,
                position,
                (planetarySystemIndex == 0), //only spawn asteroids if this is the player's home system
                colorPaletteIndex
            );

            //Heighliners
            instancePlanet.GetComponent<Planet>().heighliner0 = SpawnHeighliner(position + Vector3.right * 200f, instancePlanet);
            instancePlanet.GetComponent<Planet>().heighliner1 = SpawnHeighliner(position + Vector3.left * 200f, instancePlanet);
            //Exit nodes: link and point at each other
            if (planetarySystemIndex >= 1) //Skip index 0 because there is nothing to link back to yet because we haven't generated the last heighliner yet
            {
                //Set the exit node of this planet's heighliner0 to the previous planet's heighliner1 - loop back to the start
                GameObject thisHeighliner0 = instancePlanet.GetComponent<Planet>().heighliner0;
                GameObject previousHeighliner1 = planets.transform.GetChild(planetarySystemIndex - 1).GetComponent<Planet>().heighliner1;

                //1.0 -> 0.1   ...   4.0 -> 3.1
                thisHeighliner0.GetComponentInChildren<HeighlinerEntry>().exitNode = previousHeighliner1; heighlinerExitNodesSet++;
                thisHeighliner0.transform.rotation = Quaternion.LookRotation(previousHeighliner1.transform.position);
                //0.1 -> 1.0   ...   3.1 -> 4.0
                previousHeighliner1.GetComponentInChildren<HeighlinerEntry>().exitNode = thisHeighliner0; heighlinerExitNodesSet++;
                previousHeighliner1.transform.rotation = Quaternion.LookRotation(thisHeighliner0.transform.position);

                //Link last planet's hieghliner1 to first planet's heighliner0
                if (planetarySystemIndex == nPlanets - 1)
                {
                    GameObject initialHeighliner0 = planets.transform.GetChild(0).GetComponent<Planet>().heighliner0;
                    GameObject thisHeighliner1 = instancePlanet.GetComponent<Planet>().heighliner1;

                    //0.0 -> 4.1
                    initialHeighliner0.GetComponentInChildren<HeighlinerEntry>().exitNode = thisHeighliner1; heighlinerExitNodesSet++;
                    initialHeighliner0.transform.rotation = Quaternion.LookRotation(thisHeighliner1.transform.position);
                    //4.1 -> 0.0
                    thisHeighliner1.GetComponentInChildren<HeighlinerEntry>().exitNode = initialHeighliner0; heighlinerExitNodesSet++;
                    thisHeighliner1.transform.rotation = Quaternion.LookRotation(initialHeighliner0.transform.position);
                }
            }

            //If home planetary system
            if (planetarySystemIndex == 0)
            {
                //Player
                PlayerSpawn(
                    generationType,
                    instanceLastMoonSpawnedInCluster.transform.position + new Vector3(6f, 14f, 2f)
                );

                //Asteroid belt guaranteed at player's spawn planet
                //The player's spawn system only has simple resources - the player must travel elsewhere to collect new resource types
                Asteroid.Type asteroidType = Asteroid.Type.water;
                if (Random.value > 0.5f)
                {
                    asteroidType = Asteroid.Type.platinoid;
                }
                if (spawnAsteroids)
                {
                    AsteroidPoolSpawnCluster(
                        ClusterType.planetRing,
                        asteroidType,
                        position,
                        false
                    );
                }
                instancePlanet.GetComponent<Planet>().hasGeneratedAsteroids = true;

                //One minor bandit is guaranteed to spawn at the player's spawn planet
                EnemySpawnCluster(
                    ClusterType.planetClump,
                    position,
                    ((int)Enemy.Strength.minor).ToString()
                );
            }
        }

        return instancePlanet;
    }

    private GameObject PlanetSpawn(Vector3 position, int planetarySystemIndex, string titleOverride, int colorPaletteIndex)
    {
        //Instantiate
        GameObject instancePlanet = Instantiate(
            planet,
            position,
            Quaternion.Euler(0f, 0f, 0f)
        );

        //Pass control ref
        instancePlanet.GetComponent<Planet>().control = control;

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

        //Set color/tint
        Color tint = control.GetColorFromHexString(COLOR_PALETTE[colorPaletteIndex, 0]);
        instancePlanet.GetComponent<Planet>().tint = tint;

        return instancePlanet;
    }
    #endregion

    #region Heighliners
    private void SetupHeighlinerMapModelsAndRotation()
    {
        if (heighlinerExitNodesSet >= nPlanetsPlanned * 2)
        {
            bool loopSuccess = true;

            for (int planetaryIndex = 0; planetaryIndex < nPlanetsPlanned; planetaryIndex++)
            {
                Planet planetScript = control.generation.planets.transform.GetChild(planetaryIndex).GetComponent<Planet>();

                if (!planetScript.heighliner0.GetComponentInChildren<HeighlinerEntry>().Setup())
                {
                    loopSuccess = false;
                }

                if (!planetScript.heighliner1.GetComponentInChildren<HeighlinerEntry>().Setup())
                {
                    loopSuccess = false;
                }
            }

            heighlinersSetup = loopSuccess;
        }
    }

    public GameObject SpawnHeighliner(Vector3 position, GameObject hostPlanet)
    {
        //Spawn the heighliner
        GameObject instanceHeighliner = Instantiate(
            heighliner,
            position,
            Quaternion.identity
        );

        //Setup script
        instanceHeighliner.GetComponentInChildren<HeighlinerEntry>().parentPlanet = hostPlanet;
        instanceHeighliner.GetComponentInChildren<HeighlinerEntry>().control = control;

        //Map lines and enter/exit nodes are setup by Update() calling SetupHeighlinerMapModelsAndRotation() after system generation complete

        return instanceHeighliner;
    }
    #endregion

    #region Moons
    public void SpawnMoonEntities(int planetIndex, int moonIndex, int nMoons, Vector3 instanceMoonPosition)
    {
        Asteroid.Type asteroidType = Asteroid.Type.water;
        bool isValuablesGuaranteed = false;

        //Decide whether to spawn, and if so with what properties
        float nPercentAsteroidBelts = 50f;
        if (moonIndex == nMoons - 1)
        {
            //Player spawn moon - water asteroids only but GUARANTEED valuables
            asteroidType = Asteroid.Type.water;
            isValuablesGuaranteed = true;
        }
        else if (planetIndex == 0)
        {
            //Not player spawn moon BUT IS player spawn planetary system
            if (Control.GetTrueForPercentOfIndices(moonIndex, nMoons, nPercentAsteroidBelts))
            {
                //Player spawn SYSTEM - water and platinoid only
                if (Random.value > 0.5f)
                {
                    asteroidType = Asteroid.Type.platinoid;
                }
                //else water, as defined at declaration time
            }
        }
        else if (Control.GetTrueForPercentOfIndices(moonIndex, nMoons, nPercentAsteroidBelts))
        {
            //All other systems - any asteroid type
            asteroidType = Asteroid.GetRandomType();
        }

        //Spawn the asteroids
        AsteroidPoolSpawnCluster(
            ClusterType.moonRing,
            asteroidType,
            instanceMoonPosition,
            isValuablesGuaranteed
        );
    }

    private GameObject MoonClusterSpawn(int nMoons, int planetIndex, Vector3 planetPosition, bool spawnAsteroids, int colorPaletteIndex)
    {
        //At the end we will return the last generated moon
        GameObject instanceMoon = null;

        //Spawn all moons in this planetary system

        //Get positions for each moon
        Vector3[] moonPositions = new Vector3[nMoons];
        moonPositions = GenerateOrbitalPositionsWithReservedAngles(
            nMoons,
            (int)(nMoons * 2f),
            GEN_PARAMS_MOONS.ANGLE_NOISE,
            GEN_PARAMS_MOONS.DISTANCE_OUT,
            GEN_PARAMS_MOONS.DISTANCE_OUT_NOISE
        );

        //Spawn each moon
        for (int moonIndex = 0; moonIndex < nMoons; moonIndex++)
        {
            Vector3 instanceMoonPosition = moonPositions[moonIndex];

            //Set all moon positions to be relative to parent planet's position
            instanceMoonPosition += planetPosition;

            //Generate a slight vertical offset per moon
            instanceMoonPosition.y += Random.value * 20f;

            //Color
            int colorSubIndex = (1 + moonIndex) % COLOR_PALETTE.GetLength(1); //loop through colour palette - start offset because the planet takes index 0
            Color tint = control.GetColorFromHexString(COLOR_PALETTE[colorPaletteIndex, colorSubIndex]);

            //Spawn this moon
            instanceMoon = MoonSpawn(
                false,
                planetIndex, moonIndex,
                instanceMoonPosition, tint,
                (moonIndex == nMoons - 1), //Player spawn moon - guarantee a space station
                null, false,
                null, false, 0f, 0f, 0f, null
            );

            //Spawn asteroid belt around this moon (some percentage of moons - other than player's spawn moon - have them)
            if (spawnAsteroids)
            {
                SpawnMoonEntities(planetIndex, moonIndex, nMoons, instanceMoonPosition);
            }
        }

        //Return the last moon spawned
        return instanceMoon;
    }

    public GameObject MoonSpawn
    (
        bool loaded, int planetarySystemIndex, int moonIndex, Vector3 position, Color tint,
        bool forceStation, string titleOverride, bool ifLoadingIsStation, string stationTitleOverride,
        bool stationGenerateOffers, float stationPricePlatinoid, float stationPricePreciousMetal, float stationPriceWater, int[] stationUpgradeIndex
    )
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

        //Set colour/tint
        instanceMoon.transform.Find("Model").GetComponent<MeshRenderer>().material.SetColor("_Tint", tint);
        instanceMoon.transform.Find("Map Model").GetComponent<MeshRenderer>().material.SetColor("_Tint", tint);

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

    #region Asteroids
    private void SwapAsteroidPerformanceMode()
    {
        int detailed = 0;

        for (int i = 0; i < asteroidsEnabled.transform.childCount; i++)
        {
            //The transform to check whether to swap
            Transform transformToSwap = asteroidsEnabled.transform.GetChild(i);

            Asteroid instanceAsteroidScript = transformToSwap.GetComponent<Asteroid>();

            //Check which performance mode to use (fast-moving asteroids must be farther away before switching to performance mode)
            bool performant = (
                Vector3.Distance( //moderately-far from player
                    control.GetPlayerTransform().position,
                    transformToSwap.position
                ) >= Asteroid.THRESHOLD_DISTANCE_MAX_PERFORMANCE_MODE * Mathf.Max(1f, transformToSwap.GetComponent<Asteroid>().rb.velocity.magnitude * 0.3f)

                && Time.time > instanceAsteroidScript.timeLastDamaged + instanceAsteroidScript.PERIOD_ACTIVE_AFTER_DAMAGED //not damaged recently
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

    public void UpdateAsteroidPoolHierarchyCount()
    {
        if (control.IS_EDITOR)
        {
            asteroidsEnabled.name = "Enabled (" + asteroidsEnabled.transform.childCount + ")";
            asteroidsDisabled.name = "Disabled (" + asteroidsDisabled.transform.childCount + ")";
        }
    }

    private void AsteroidPoolSpawnCluster(ClusterType clusterType, Asteroid.Type oreType, Vector3 position, bool guaranteeValuables)
    {

        if (clusterType == ClusterType.planetRing || clusterType == ClusterType.moonRing)
        {
            int nAsteroids = 0;
            float chancePercentOfValuableType = 0f;
            float radius = 0f;
            float radiusRandomness = 0f;

            if (clusterType == ClusterType.planetRing)
            {
                nAsteroids = GEN_PARAMS_PLANETS.ASTEROID_COUNT; //100; //70;
                radius = GEN_PARAMS_PLANETS.ASTEROID_DISTANCE_OUT; //250f; //200 //170
                radiusRandomness = GEN_PARAMS_PLANETS.ASTEROID_DISTANCE_OUT_NOISE; //21f; //radius * 0.28f; //0.4 //0.12

                if (guaranteeValuables)
                {
                    chancePercentOfValuableType = 20f;
                }
                else
                {
                    chancePercentOfValuableType = Random.Range(
                        GEN_PARAMS_PLANETS.ASTEROID_PERCENT_VALUABLE_TYPE_RANGE_LOW,
                        GEN_PARAMS_PLANETS.ASTEROID_PERCENT_VALUABLE_TYPE_RANGE_HIGH
                    ); //Random.Range(0f, 30f); //10f;
                }
            }
            else if (clusterType == ClusterType.moonRing)
            {
                nAsteroids = GEN_PARAMS_MOONS.ASTEROID_COUNT; //40; //70;
                radius = GEN_PARAMS_MOONS.ASTEROID_DISTANCE_OUT; //70f; //60
                radiusRandomness = GEN_PARAMS_MOONS.ASTEROID_DISTANCE_OUT_NOISE; //21f; //radius * 0.28f; //0.4 //0.12
                
                if (guaranteeValuables)
                {
                    chancePercentOfValuableType = 30f;
                }
                else
                {
                    chancePercentOfValuableType = Random.Range(
                        GEN_PARAMS_MOONS.ASTEROID_PERCENT_VALUABLE_TYPE_RANGE_LOW,
                        GEN_PARAMS_MOONS.ASTEROID_PERCENT_VALUABLE_TYPE_RANGE_HIGH
                    ); //Random.Range(0f, 40f); //10f;
                }
            }
            
            float heightRandomness = 15f; //20f; //radius * 0.28f; //0.3 //0.24 0.12
            float angle = 0f;
            float angleRandomness = 6f;

            for (int i = 0; i < nAsteroids; i++)
            {
                //Pick radius to spawn at
                float instanceAsteroidRadius = radius + (Random.Range(0f, 2f * radiusRandomness) - radiusRandomness);

                //Pick whether clay-silicate or valuable
                Asteroid.Type oreToSpawnAs = Asteroid.Type.claySilicate;
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
                    (Asteroid.Size)Random.Range(0, Control.GetEnumLength(typeof(Asteroid.Size))),
                    oreToSpawnAs
                );

                //Randomly move up/down relative the stellar plane
                instanceAsteroid.transform.position = new Vector3(
                    instanceAsteroid.transform.position.x,
                    instanceAsteroid.transform.position.y + (control.GetRandomNeg1ToPos1() * heightRandomness),
                    instanceAsteroid.transform.position.z
                );

                //Increment angle
                angle += (360f / (float)nAsteroids) + (Random.Range(0f, 2f * angleRandomness) - angleRandomness);
            }
        }
    }

    public GameObject AsteroidPoolSpawn(Vector3 position, Asteroid.Size size, Asteroid.Type type)
    {
        //Spawn a single asteroid from the pool (move an asteroid from disabled to enabled)

        GameObject instanceAsteroid = null;

        //If we have room in the pool to draw from
        if (asteroidsDisabled.transform.childCount > 0)
        {
            //Remember which asteroid we're working with so we can return it later
            instanceAsteroid = asteroidsDisabled.transform.GetChild(0).gameObject;

            //Enable that asteroid
            instanceAsteroid.GetComponent<Asteroid>().EnableInPool(position, size, type);

            //Add random torque
            float torqueMagnitude = Mathf.Pow(Random.value, 1f / 4f) * (150f * instanceAsteroid.GetComponent<Asteroid>().rb.mass); //120f; //60f;
            instanceAsteroid.GetComponent<Rigidbody>().AddTorque(torqueMagnitude * control.GetRandomDirection());

            //Remember movement
            Asteroid instanceAsteroidScript = instanceAsteroid.GetComponent<Asteroid>();
            instanceAsteroidScript.rbMemVel = instanceAsteroidScript.rb.velocity;
            instanceAsteroidScript.rbMemAngularVel = instanceAsteroidScript.rb.angularVelocity;

            //Update hierarchy name to reflect count
            UpdateAsteroidPoolHierarchyCount();
        }
        else
        {
            Debug.LogError("No free asteroids!");
            //TODO: later we could either expand the pool or reuse enabled asteroids
        }

        return instanceAsteroid;
    }

    private void AsteroidPoolPopulate(int asteroidPoolLength)
    {
        for (int nAsteroids = 0; nAsteroids < asteroidPoolLength; nAsteroids++)
        {
            //Instantiate
            GameObject instanceAsteroid = Instantiate(
                asteroid,
                Vector3.zero,
                Quaternion.identity
            );

            Asteroid instanceAsteroidScript = instanceAsteroid.GetComponent<Asteroid>();
            instanceAsteroidScript.control = control;
            instanceAsteroidScript.rb.detectCollisions = false;
            instanceAsteroidScript.DisableInPool();
        }

        //Update hierarchy names
        UpdateAsteroidPoolHierarchyCount();
    }
    #endregion

    #region Ore
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
        }

        //Update hierarchy names
        oreEnabled.name = "Enabled (" + oreEnabled.transform.childCount + ")";
        oreDisabled.name = "Disabled (" + oreDisabled.transform.childCount + ")";
    }

    public GameObject OrePoolSpawn(Vector3 position, Asteroid.Type type, Vector3 parentVelocity)
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

    public void OrePoolSpawnWithTraits(Vector3 position, Rigidbody rbInherit, Asteroid.Type type)
    {
        //Clay-silicate asteroids drop a mixture
        Asteroid.Type typeToSpawn = type;
        if (type == Asteroid.Type.claySilicate)
        {
            if (Random.value <= 0.75f)
            {
                typeToSpawn = Asteroid.Type.platinoid;
            }
            else
            {
                typeToSpawn = Asteroid.Type.water;
            }
        }

        //Pool spawning
        GameObject instanceOre = control.generation.OrePoolSpawn(
            position + (0.8f * control.GetRandomDirection()),
            typeToSpawn,
            rbInherit.velocity
        );

        //Pass rigidbody values
        Rigidbody instanceOreRb = instanceOre.GetComponent<Rigidbody>();
        instanceOreRb.velocity = rbInherit.velocity;
        instanceOreRb.angularVelocity = rbInherit.angularVelocity;
        instanceOreRb.inertiaTensor = rbInherit.inertiaTensor;
        instanceOreRb.inertiaTensorRotation = rbInherit.inertiaTensorRotation;

        //Add random forces
        float ejectionForce = 2e3f;
        instanceOreRb.AddForce(ejectionForce * control.GetRandomDirection());
        instanceOreRb.AddTorque(
            Random.Range(0f, 7000f) //Random.Range(3000f, 7000f) //5000f
            * control.GetRandomDirection()
        );
    }
    #endregion

    #region Bandits
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

    private void EnemySpawnCluster(ClusterType clusterType, Vector3 position, string list)
    {
        //clusterType - an enum of several spawn patterns
        //position - where that spawn pattern will centre around
        //list - how many enemies and of what strength. Example: "2100" = 1x elite bandit, 1x major bandits, and 2x minor bandits
        //Should be written as Enemy.STRENGTH_ELITE.ToString() + Enemy.STRENGTH_MAJOR.ToString() + Enemy.STRENGTH_MINOR.ToString() + Enemy.STRENGTH_MINOR.ToString()

        //Amount
        int amount = list.Length;
        
        //Spawn pattern
        if (clusterType == ClusterType.planetClump)
        {
            //Position - where around the planet will the cluster be?
            float radiusFromPlanet = 150f;
            float angleFromPlanet = Random.value * 360f;
            Vector3 clusterCenterPosition = position + new Vector3(
                Mathf.Cos(Mathf.Deg2Rad * angleFromPlanet) * radiusFromPlanet,
                10f, //we are ADDING to the position - the y has already been set
                Mathf.Sin(Mathf.Deg2Rad * angleFromPlanet) * radiusFromPlanet
            );

            Vector3 clusterPosition = clusterCenterPosition;
            if (amount > 1)
            {
                for (int i = 0; i < amount; i++)
                {
                    //Position - where within the cluster will this bandit be?
                    float radiusFromCluster = 10f;
                    float angleFromCluster = Random.value * 360f;
                    Vector3 clusterOffsetPosition = clusterCenterPosition + new Vector3(
                        Mathf.Cos(Mathf.Deg2Rad * angleFromCluster) * radiusFromCluster,
                        clusterCenterPosition.y + (Random.Range(5f, 15f) * control.GetRandomPositiveOrNegative()),
                        Mathf.Sin(Mathf.Deg2Rad * angleFromCluster) * radiusFromCluster
                    );

                    clusterPosition = clusterOffsetPosition;

                    //Spawn
                    EnemySpawn(clusterPosition, (Enemy.Strength)control.GetIntFromStringIndex(list, i));

                    //Debug.Log("Position " + position + ": " + list + ", list" + "[" + i + "] = " + control.GetIntFromStringIndex(list, i) + " = " + (Enemy.Strength)control.GetIntFromStringIndex(list, i));
                }
            }
            else
            {
                //Spawn - don't bother calculating offsets if there will only be one bandit spawned
                EnemySpawn(clusterPosition, (Enemy.Strength)control.GetIntFromStringIndex(list, 0));

                //Debug.Log("Position " + position + ": " + list + ", list" + "[" + 0 + "] = " + control.GetIntFromStringIndex(list, 0) + " = " + (Enemy.Strength)control.GetIntFromStringIndex(list, 0));
            }
        }
    }

    public GameObject EnemySpawn(Vector3 position, Enemy.Strength strength)
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

    #region Player
    private void PlayerSpawn(GenerationType generationType, Vector3 position)
    {
        //Remember that the player has spawned. This must be placed at the top of this method otherwise the player's own functions will null reference on the player itself!
        playerSpawned = true;

        //Instantiate at position, rotation, velocity
        instancePlayer = Instantiate(
            playerPrefab,
            Vector3.zero,
            Quaternion.identity
        );
        instancePlayer.transform.Find("Body").transform.position = position;

        Player playerScript = instancePlayer.GetComponentInChildren<Player>();

        if (generationType == GenerationType.newGame || generationType == GenerationType.restarted)
        {
            playerScript.vitalsHealth = playerScript.vitalsHealthMax;
            playerScript.vitalsFuel = playerScript.vitalsFuelMax * 0.75d;
            playerScript.isDestroyed = false;
            //instancePlayer.transform.Find("Body").transform.rotation = Quaternion.Euler(5f, 20f, 0f); //x = pitch, y = yaw, z = roll
            //instancePlayer.GetComponentInChildren<Rigidbody>().velocity = playerSpawnMoon.GetComponent<Rigidbody>().velocity;
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
    }
    #endregion

    #region Saving and loading
    public void SaveGame()
    {
        ////INIT ARRAYS TO BE USED FOR SAVING
        ////Planets
        //float[,] planetPosition = new float[planetarySystems.Count, 3];
        //string[] planetName = new string[planetarySystems.Count];
        //byte[] planetarySystemMoonQuantity = new byte[planetarySystems.Count];
        //
        ////Moons
        //Moon[] moonArray = FindObjectsOfType<Moon>();
        //float[,] moonPosition = new float[moonArray.Length, 3];
        //string[] moonName = new string[moonArray.Length];
        //bool[] moonHasStation = new bool[moonArray.Length];
        //
        //string[] stationTitle = new string[moonArray.Length];
        //float[] stationPricePlatinoid = new float[moonArray.Length];
        //float[] stationPricePreciousMetal = new float[moonArray.Length];
        //float[] stationPriceWater = new float[moonArray.Length];
        //int[,] stationUpgradeIndex = new int[moonArray.Length, StationDocking.upgradeButtons];
        //
        ////ASSIGN VERSE DATA TO THOSE ARRAYS
        //for (int planetaryIndex = 0; planetaryIndex < planetarySystems.Count; planetaryIndex++)
        //{
        //    //PLANETS
        //    GameObject instancePlanet = planetarySystems[planetaryIndex][0]; //0 is the planet, the rest of the list is moons
        //
        //    //Position
        //    planetPosition[planetaryIndex, 0] = instancePlanet.transform.position.x;
        //    planetPosition[planetaryIndex, 1] = instancePlanet.transform.position.y;
        //    planetPosition[planetaryIndex, 2] = instancePlanet.transform.position.z;
        //
        //    //Name
        //    planetName[planetaryIndex] = instancePlanet.GetComponent<NameCelestial>().title;
        //
        //    //Number of moons
        //    planetarySystemMoonQuantity[planetaryIndex] = (byte)planetarySystems[planetaryIndex].Count;
        //
        //    //Go until count - 1 because 0 is the planet so we offset everything by +1, and we don't want to go over
        //    for (int moonIndex = 0; moonIndex < planetarySystems[planetaryIndex].Count - 1; moonIndex++)
        //    {
        //        //MOONS
        //        GameObject instanceMoon = planetarySystems[planetaryIndex][moonIndex + 1]; //add 1 because 0 is the planet
        //        Moon instanceMoonScript = instanceMoon.GetComponent<Moon>(); //TODO: sometimes this is null
        //
        //        //Position
        //        moonPosition[moonIndex, 0] = instanceMoon.transform.position.x;
        //        moonPosition[moonIndex, 1] = instanceMoon.transform.position.y;
        //        moonPosition[moonIndex, 2] = instanceMoon.transform.position.z;
        //
        //        //Name
        //        moonName[moonIndex] = instanceMoon.GetComponent<NameCelestial>().title;
        //
        //        //Station
        //        moonHasStation[moonIndex] = instanceMoon.GetComponent<Moon>().hasStation;
        //        if (instanceMoonScript.hasStation && instanceMoonScript.instancedStation != null)
        //        {
        //            stationTitle[moonIndex] = instanceMoonScript.instancedStation.GetComponent<NameHuman>().title;
        //            stationPricePlatinoid[moonIndex] = instanceMoonScript.instancedStation.GetComponentInChildren<StationDocking>().pricePlatinoid;
        //            stationPricePreciousMetal[moonIndex] = instanceMoonScript.instancedStation.GetComponentInChildren<StationDocking>().pricePreciousMetal;
        //            stationPriceWater[moonIndex] = instanceMoonScript.instancedStation.GetComponentInChildren<StationDocking>().priceWater;
        //
        //            //Concatenate the array so that we have the moon data along with the data for each upgrade offer's index
        //            for (int upgradeButtonIndex = 0; upgradeButtonIndex < StationDocking.upgradeButtons; upgradeButtonIndex++)
        //            {
        //                stationUpgradeIndex[moonIndex, upgradeButtonIndex] = instanceMoonScript.instancedStation.GetComponentInChildren<StationDocking>().upgradeIndexAtButton[upgradeButtonIndex];
        //            }
        //        }
        //        else
        //        {
        //            stationTitle[moonIndex] = null;
        //            stationPricePlatinoid[moonIndex] = 0f;
        //            stationPricePreciousMetal[moonIndex] = 0f;
        //            stationPriceWater[moonIndex] = 0f;
        //
        //            //Concatenate the array so that we have the moon data along with the data for each upgrade offer's index
        //            for (int upgradeButtonIndex = 0; upgradeButtonIndex < StationDocking.upgradeButtons; upgradeButtonIndex++)
        //            {
        //                stationUpgradeIndex[moonIndex, upgradeButtonIndex] = control.commerce.UPGRADE_SOLD_OUT;
        //            }
        //        }
        //    }
        //}
        //
        ////Asteroids
        //Asteroid[] asteroidArray = FindObjectsOfType<Asteroid>();
        //
        //float[,] asteroidPosition = new float[asteroidArray.Length, 3];
        //float[,] asteroidVelocity = new float[asteroidArray.Length, 3];
        //int[] asteroidSize = new int[asteroidArray.Length];
        //byte[] asteroidType = new byte[asteroidArray.Length];
        //byte[] asteroidHealth = new byte[asteroidArray.Length];
        //
        //byte asteroidArrayIndex = 0;
        //foreach (Asteroid asteroid in asteroidArray)
        //{
        //    //Position
        //    asteroidPosition[asteroidArrayIndex, 0] = asteroid.transform.position.x;
        //    asteroidPosition[asteroidArrayIndex, 1] = asteroid.transform.position.y;
        //    asteroidPosition[asteroidArrayIndex, 2] = asteroid.transform.position.z;
        //
        //    //Velocity
        //    asteroidVelocity[asteroidArrayIndex, 0] = asteroid.GetComponent<Rigidbody>().velocity.x;
        //    asteroidVelocity[asteroidArrayIndex, 1] = asteroid.GetComponent<Rigidbody>().velocity.y;
        //    asteroidVelocity[asteroidArrayIndex, 2] = asteroid.GetComponent<Rigidbody>().velocity.z;
        //
        //    //Size
        //    asteroidSize[asteroidArrayIndex] = asteroid.size;
        //
        //    //Type
        //    asteroidType[asteroidArrayIndex] = asteroid.type;
        //
        //    //Health
        //    asteroidHealth[asteroidArrayIndex] = asteroid.health;
        //
        //    //Increment
        //    asteroidArrayIndex++;
        //}
        //
        ////Verse
        //float[] verseSpacePosition = new float[3];
        //verseSpacePosition[0] = verseSpace.transform.position.x;
        //verseSpacePosition[1] = verseSpace.transform.position.y;
        //verseSpacePosition[2] = verseSpace.transform.position.z;
        //
        ////Player
        //Player playerScript = instancePlayer.GetComponentInChildren<Player>();
        //float[] playerPosition = new float[3];
        //playerPosition[0] = playerScript.transform.position.x;
        //playerPosition[1] = playerScript.transform.position.y;
        //playerPosition[2] = playerScript.transform.position.z;
        //
        ////SAVE TO DATA CLASS
        //LevelData.Data data = new LevelData.Data();
        //
        ////World properties
        ////Centre star
        //data.starName = instanceStarHome.GetComponent<NameCelestial>().title;
        //
        ////Planets
        //data.planetQuantity = (byte)planetarySystems.Count;
        //data.planetPosition = planetPosition;
        //data.planetName = planetName;
        //data.planetarySystemMoonQuantity = planetarySystemMoonQuantity;
        //
        ////Moons
        //data.moonQuantity = (byte)moonArray.Length;
        //data.moonPosition = moonPosition;
        //data.moonName = moonName;
        //data.moonHasStation = moonHasStation;
        //
        ////Stations
        //data.stationTitle = stationTitle;
        //data.stationPricePlatinoid = stationPricePlatinoid;
        //data.stationPricePreciousMetal = stationPricePreciousMetal;
        //data.stationPriceWater = stationPriceWater;
        //data.stationUpgradeIndex = stationUpgradeIndex;
        //
        ////Asteroids
        //data.asteroidQuantity = asteroidArray.Length;
        //data.asteroidPosition = asteroidPosition;
        //data.asteroidVelocity = asteroidVelocity;
        //data.asteroidSize = asteroidSize;
        //data.asteroidType = asteroidType;
        //data.asteroidHealth = asteroidHealth;
        //
        ////Verse space
        //data.verseSpacePosition = verseSpacePosition;
        //
        ////Player properties
        //data.playerPosition = playerPosition;
        //
        //data.playerUpgrades = playerScript.upgradeLevels;
        //
        //data.playerVitalsHealth = playerScript.vitalsHealth;
        //data.playerDestroyed = playerScript.isDestroyed;
        //
        //data.playerVitalsFuel = playerScript.vitalsFuel;
        //
        //data.playerCurrency = playerScript.currency;
        //data.playerOre = playerScript.ore;
        //
        ////SAVE THE CLASS/GAME
        //LevelData.SaveGame(Application.persistentDataPath + Control.userDataFolder + Control.userLevelSaveFile, data);
    }

    private void TryLoadGameElseNewGame()
    {
        LevelData.Data data = LevelData.LoadGame(Application.persistentDataPath + Control.userDataFolder + Control.userLevelSaveFile);

        //Only load if a save file exists. If a save file doesn't exist, generate a new game
        //ALWAYS generate a new game if in editor
        if (data == null || control.IS_EDITOR || true) //the true keyword here disables loading ALWAYS - we want to temporarily disable until it's fixed
        {
            //Debug.Log("No save exists; generating new game");
            GenerateGame(GenerationType.newGame);
        }
        else
        {
            ////Debug.Log("Save exists; loading game");
            //
            ////Star
            //StarSpawn(data.starName);
            //
            ////Planets
            //for (int planetIndex = 0; planetIndex < data.planetQuantity; planetIndex++)
            //{
            //    GameObject instancePlanet = PlanetSpawn(
            //        new Vector3(
            //            data.planetPosition[planetIndex, 0],
            //            data.planetPosition[planetIndex, 1],
            //            data.planetPosition[planetIndex, 2]
            //        ),
            //        planetIndex,
            //        data.planetName[planetIndex],
            //        data.planetColor[planetIndex]
            //    );
            //
            //    //GameObject instancePlanet = PlanetarySystemSpawnAndPlayerSpawn(
            //    //    GENERATION_TYPE_LOADED_GAME,
            //    //    planetIndex,
            //    //    0,
            //    //    new Vector3(
            //    //        data.planetPosition[planetIndex, 0],
            //    //        data.planetPosition[planetIndex, 1],
            //    //        data.planetPosition[planetIndex, 2]
            //    //    ),
            //    //    data.planetName[planetIndex]
            //    //);
            //
            //    //Expand the list
            //    planetarySystems.Add(new List<GameObject>());
            //
            //    //Add planet to the list
            //    planetarySystems[planetIndex].Add(instancePlanet);
            //
            //    //Moons
            //    for (int moonIndex = 0; moonIndex < data.planetarySystemMoonQuantity[planetIndex]; moonIndex++)
            //    {
            //        GameObject instanceMoon;
            //        if (data.moonHasStation[moonIndex])
            //        {
            //            //Slice the array so that we have only the upgrade offers' indices (since we are already looping through each moon)
            //            int[] controlScriptPlanetoidStationUpgradeIndex = new int[StationDocking.upgradeButtons];
            //            for (int upgrade = 0; upgrade < StationDocking.upgradeButtons; upgrade++)
            //            {
            //                controlScriptPlanetoidStationUpgradeIndex[upgrade] = data.stationUpgradeIndex[moonIndex, upgrade];
            //            }
            //
            //            instanceMoon = MoonSpawn(
            //                true,
            //                planetIndex,
            //                moonIndex,
            //                new Vector3(
            //                    data.moonPosition[moonIndex, 0],
            //                    data.moonPosition[moonIndex, 1],
            //                    data.moonPosition[moonIndex, 2]
            //                ),
            //                data.moonTint[moonIndex],
            //                data.moonHasStation[moonIndex],
            //                data.moonName[moonIndex],
            //                data.moonHasStation[moonIndex],
            //                data.stationTitle[moonIndex],
            //                false, //generate offers?
            //                data.stationPricePlatinoid[moonIndex],
            //                data.stationPricePreciousMetal[moonIndex],
            //                data.stationPriceWater[moonIndex],
            //                controlScriptPlanetoidStationUpgradeIndex
            //            );
            //        }
            //        else
            //        {
            //            instanceMoon = MoonSpawn(
            //                true,
            //                planetIndex,
            //                moonIndex,
            //                new Vector3(
            //                    data.moonPosition[moonIndex, 0],
            //                    data.moonPosition[moonIndex, 1],
            //                    data.moonPosition[moonIndex, 2]
            //                ),
            //                data.moonTint[moonIndex],
            //                data.moonHasStation[moonIndex],
            //                data.moonName[moonIndex],
            //                data.moonHasStation[moonIndex],
            //                null,
            //                false, //generate offers?
            //                0f,
            //                0f,
            //                0f,
            //                null
            //            );
            //        }
            //
            //        //Add the moon to the list
            //        planetarySystems[planetIndex].Add(instanceMoon);
            //    }
            //}
            //
            ////Asteroids
            //for (byte i = 0; i < data.asteroidQuantity; i++)
            //{
            //    //SpawnAsteroid(
            //    //    new Vector3(
            //    //        data.asteroidPosition[i, 0],
            //    //        data.asteroidPosition[i, 1],
            //    //        data.asteroidPosition[i, 2]
            //    //    ),
            //    //    new Vector3(
            //    //        data.asteroidVelocity[i, 0],
            //    //        data.asteroidVelocity[i, 1],
            //    //        data.asteroidVelocity[i, 2]
            //    //    ),
            //    //    data.asteroidSize[i],
            //    //    data.asteroidType[i],
            //    //    data.asteroidHealth[i]
            //    //);
            //}
            //
            ////PLAYER
            //PlayerSpawn(
            //    GENERATION_TYPE_LOADED_GAME,
            //    new Vector3(
            //        data.playerPosition[0],
            //        data.playerPosition[1],
            //        data.playerPosition[2]
            //    )
            //);
            //Player playerScript = instancePlayer.GetComponentInChildren<Player>();
            //
            ////Player properties
            //playerScript.upgradeLevels = data.playerUpgrades;
            //playerScript.UpdateUpgrades();
            //
            //playerScript.vitalsHealth = data.playerVitalsHealth;
            //playerScript.isDestroyed = data.playerDestroyed;
            //
            //playerScript.vitalsFuel = data.playerVitalsFuel;
            //
            //playerScript.currency = data.playerCurrency;
            //playerScript.ore = data.playerOre;
            //
            ////Verse position relative to origin
            //verseSpace.transform.position = new Vector3(
            //    data.verseSpacePosition[0],
            //    data.verseSpacePosition[1],
            //    data.verseSpacePosition[2]
            //);
        }
    }
    #endregion

    #region Math
    private Vector3[] GenerateOrbitalPositionsWithReservedAngles
    (
        int nPositions, int nAngles, float angleNoiseMagnitudeDegrees, float distanceOut, float distanceOutNoiseMagnitude
    )
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

        if (nPositions > nAngles)
        {
            Debug.LogError("Not enough space to ensure no overlap for :" + nPositions + " positions at " + distanceOut + " radius requiring " + nAngles + " angles");
        }

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
                int angleToCheck = (int)Random.Range(0f, (float)nAngles); //-1 because we start at 0
                //Debug.Log("Checking angle " + angleToCheck + "/" + availableAngles.Length);
                if (availableAngles[angleToCheck])
                {
                    //Set the angle to use as no longer available for other positions to use
                    availableAngles[angleToCheck] = false;

                    //Convert from angle index to degrees (because we add random noise in degrees)
                    angle = angleToCheck * (360f / availableAngles.Length);

                    //Add random noise in degrees to the angle
                    angle += Random.value * angleNoiseMagnitudeDegrees;

                    //Convert from degrees to radians, since that's what the Mathf struct uses
                    angle *= 0.01745329251f; //TauRadiansPerTurn / degreesPerTurn = 6.28 / 360;

                    //Exit the try loop - we have found a working angle for this position
                    break;
                }
                else if (tryIndex == triesMax - 1)
                {
                    //Default to a random angle if we run out of tries
                    Debug.LogError("Ran out of tries trying to find an available angle to use for this position; assigning a random angle");
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
    #endregion
}