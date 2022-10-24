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
        nPlanetsPlanned = Random.Range(PLANETS_RANGE_LOW, PLANETS_RANGE_HIGH + 1);
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
        //Properties
        float distanceOut = maxMoonDist;
        float randSpacing;
        float spawnRadius;
        float spawnAngle;

        //Spawn all planetary systems
        for (int planetaryIndex = 0; planetaryIndex < nPlanets; planetaryIndex++)
        {
            //Generate planet position
            randSpacing = Mathf.Pow(Random.Range(0f, planetsSpacingBaseMax), Random.Range(1f, PLANETS_SPACING_POWER));
            spawnRadius = distanceOut + randSpacing;
            distanceOut = spawnRadius; //incremenet distanceOut for the next moon
            spawnAngle = Random.Range(0f, 360f);
            Vector3 position = new Vector3(
                Mathf.Cos(spawnAngle) * distanceOut,
                0f,
                Mathf.Sin(spawnAngle) * distanceOut
            );

            //Spawn planet
            PlanetarySystemSpawnAndPlayerSpawn(generationType, planetaryIndex, nPlanets, position, null);
        }
    }

    private GameObject PlanetarySystemSpawnAndPlayerSpawn(int generationType, int planetarySystemIndex, int nPlanets, Vector3 position, string titleOverride)
    {
        //PLANET
        GameObject instancePlanet = PlanetSpawn(position, planetarySystemIndex, titleOverride);

        //PLANETARY SYSTEM BODIES
        if (generationType != GENERATION_TYPE_LOADED_GAME)
        {
            //Moons
            GameObject instanceLastMoonSpawnedInCluster = MoonClusterSpawn(Random.Range(MOONS_RANGE_LOW, MOONS_RANGE_HIGH + 1), planetarySystemIndex, position);

            //Asteroid belt (some percent of all planets have one)
            float nPercentAsteroidBelts = 50f;
            if (Control.GetTrueForPercentOfIndices(planetarySystemIndex, nPlanets, nPercentAsteroidBelts))
            {
                AsteroidPoolSpawnCluster(
                    CLUSTER_TYPE_PLANET_RINGS,
                    Asteroid.GetRandomType(),
                    position,
                    false
                );
            }

            //Enemy (some percent of all planets have them)
            float nPercentEnemies = 50f;
            if (Control.GetTrueForPercentOfIndices(planetarySystemIndex, nPlanets, nPercentEnemies))
            {
                EnemySpawnCluster(CLUSTER_TYPE_PLANET_CLUMP, position);
            }

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
        Player.UpdateOutlineMaterial(Player.CBODY_TYPE_PLANET, instancePlanet.GetComponentInChildren<MeshRenderer>().material);

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
        GameObject instanceMoon = null;

        //Properties
        float distanceOut = MOONS_DISTANCE_OUT;
        float randSpacing;
        float spawnRadius;
        float spawnAngle;
        float verticalOffsetRange = 20f;

        //Spawn all
        for (int moonIndex = 0; moonIndex < nMoons; moonIndex++)
        {
            //Generate the position coords
            randSpacing = Mathf.Pow(Random.Range(0f, MOONS_SPACING_BASE_MAX), Random.Range(1f, MOONS_SPACING_POWER));
            spawnRadius = distanceOut + randSpacing;
            distanceOut = spawnRadius; //incremenet distanceOut for the next moon
            spawnAngle = Random.Range(0f, 360f);

            Vector3 position = new Vector3(
                Mathf.Cos(spawnAngle) * spawnRadius,
                Random.Range(-verticalOffsetRange, verticalOffsetRange),
                Mathf.Sin(spawnAngle) * spawnRadius
            );
            //Offset to "orbit" planet
            position += planetPosition;

            //Spawn the moon
            instanceMoon = MoonSpawn(
                false,
                planetIndex, moonIndex,
                position,
                null, false,
                null, false, 0f, 0f, 0f, null
            );

            //Debug.Log("moonIndex " + moonIndex + " / " + nMoons + " moons");
            //Spawn asteroid belt (some percentage of moons have them)
            float nPercentAsteroidBelts = 50f;
            if (moonIndex == nMoons - 1 || Control.GetTrueForPercentOfIndices(moonIndex, nMoons, nPercentAsteroidBelts))
            {
                AsteroidPoolSpawnCluster(
                    CLUSTER_TYPE_MOON_RINGS,
                    Asteroid.GetRandomType(),
                    position,
                    true
                );
            }
        }

        //Return the last moon spawned
        return instanceMoon;
    }

    public GameObject MoonSpawn(bool loaded, int planetarySystemIndex, int moonIndex, Vector3 position, string titleOverride, bool ifLoadingIsStation, string stationTitleOverride, bool stationGenerateOffers, float stationPricePlatinoid, float stationPricePreciousMetal, float stationPriceWater, int[] stationUpgradeIndex)
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
        Player.UpdateOutlineMaterial(Player.CBODY_TYPE_MOON, instanceMoon.GetComponentInChildren<MeshRenderer>().material);

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
                    loaded,
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
                instanceMoon.GetComponent<Moon>().SpawnStation(true, null, true, 0f, 0f, 0f, null);
            }
            else if (moonIndex == 1 || moonIndex == 2)
            {
                //Heighliner
                //Force a heighliner to spawn
                instanceMoon.GetComponent<Moon>().SpawnHeighliner("Heighliner");
            }
            else
            {
                //Other stations (random chance)
                if (Random.value <= 0.75)
                {
                    instanceMoon.GetComponent<Moon>().SpawnStation(false, null, true, 0f, 0f, 0f, null);
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
            Player.UpdateOutlineMaterial(Player.CBODY_TYPE_ASTEROID, instanceAsteroidScript.modelObject.GetComponentInChildren<MeshRenderer>().material);
        }
        else
        {
            Debug.Log("No free asteroids!");
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
            Debug.Log("No free ores!");
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
                radius = 170f;
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
                radius = 60f;
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

            float radiusRandomness = 0.12f * radius;
            float heightRandomness = 0.12f * radius;
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
                        position.y,
                        Mathf.Sin(Mathf.Deg2Rad * angle) * instanceAsteroidRadius
                    ),
                    Random.Range(0, Asteroid.SIZE_LENGTH),
                    oreToSpawnAs
                );

                //Randomly move up/down relative the stellar plane
                instanceAsteroid.transform.position = new Vector3(
                    instanceAsteroid.transform.position.x,
                    Random.Range(0f, 2f * heightRandomness) - heightRandomness,
                    instanceAsteroid.transform.position.z
                );

                //Increment angle
                angle += (360f / (float)nAsteroids) + (Random.Range(0f, 2f * angleRandomness) - angleRandomness);
            }
        }

        //GameObject instancePlanet = Control.GetClosestTransformFromHierarchy(control.generation.planets.transform, transform.position).gameObject;
        ////instancePlanet.SetActive(!instancePlanet.activeSelf);
        //int index = instancePlanet.GetComponent<PlanetarySystemBody>().planetarySystemIndex;
        //int count = control.generation.planetarySystems[index].Count;
        //control.ui.SetTip("Index: " + index + "; Count: " + count);
        //for (int i = 0; i < count; i++)
        //{
        //    GameObject instancePlanetarySystemBody = control.generation.planetarySystems[index][i];
        //    instancePlanetarySystemBody.SetActive(!instancePlanetarySystemBody.activeSelf);
        //}
    }

    //private void AsteroidManageCount()
    //{
    //    //Asteroid count manager
    //    //Minimum
    //    if (asteroids.transform.childCount < control.settings.asteroidsConcurrentMin)
    //    {
    //        int asteroidsToGenerate = control.settings.asteroidsConcurrentMin - asteroids.transform.childCount;
    //        int clustersToGenerate = asteroidsToGenerate / 4;
    //
    //        //GenerateAsteroidClusters(Random.Range(ASTEROID_CLUSTERS_RANGE_LOW, ASTEROID_CLUSTERS_RANGE_HIGH + 1));
    //        //GenerateAsteroidClusters(clustersToGenerate, 4);
    //    }
    //
    //    //Limit
    //    if (asteroids.transform.childCount >= control.settings.asteroidsConcurrentMax)
    //    {
    //        //Destroy asteroids that are far from the player
    //        //Limit number of attempts to prevent getting stuck
    //        int ASTEROIDS_TO_DESTROY = 5;
    //        int asteroidsDestroyed = 0;
    //        int ATTEMPT_QUIT = asteroids.transform.childCount;
    //        for (int attempt = 0; attempt < ATTEMPT_QUIT; attempt++)
    //        {
    //            GameObject asteroidToDestroy = asteroids.transform.GetChild(Random.Range(0, asteroids.transform.childCount)).gameObject;
    //
    //            if (Vector3.Distance(asteroidToDestroy.transform.position, instancePlayer.transform.Find("Body").position) > 250.0f)
    //            {
    //                //Destroy this asteroid and increment counts
    //                Destroy(asteroidToDestroy, 0f);
    //
    //                asteroidsDestroyed++;
    //                if (asteroidsDestroyed >= ASTEROIDS_TO_DESTROY)
    //                {
    //                    attempt = ATTEMPT_QUIT;
    //                }
    //            }
    //        }
    //    }
    //}

    private void EnemySpawnCluster(int clusterType, Vector3 position)
    {
        if (clusterType == CLUSTER_TYPE_PLANET_CLUMP)
        {
            int nEnemies = 1;
            float radius = 150f;
            float angle = Random.value * 360f;

            for (int i = 0; i < nEnemies; i++)
            {
                //Position to spawn at
                position += new Vector3(
                    Mathf.Cos(Mathf.Deg2Rad * angle) * radius,
                    30f,
                    Mathf.Sin(Mathf.Deg2Rad * angle) * radius
                );

                //Spawn the enemy
                EnemySpawn(position, Enemy.STRENGTH_SMALL);
            }
        }
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