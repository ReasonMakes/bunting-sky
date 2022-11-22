using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public Control control;
    public KeyBinds binds;

    //HUD
    public GameObject canvas;

    //System info
    [System.NonSerialized] public TextMeshProUGUI systemInfo;
    private readonly short FPS_PRINT_PERIOD = 60;

    //Waypoint
    [System.NonSerialized] public Image waypointImage;
    private float waypointXMin;
    private float waypointXMax;
    private float waypointYMin;
    private float waypointYMax;
    private readonly float WAYPOINT_X_OFFSET = 200f;
    private readonly float WAYPOINT_Y_OFFSET = -50f; //48f;
    private bool renderWaypoint = false;
    [System.NonSerialized] public TextMeshProUGUI waypointTextType;
    [System.NonSerialized] public TextMeshProUGUI waypointTextTitle;
    [System.NonSerialized] public TextMeshProUGUI waypointTextBody;

    //Target
    [System.NonSerialized] public Image targetImage;
    //private float targetXMin;
    //private float targetXMax;
    //private float targetYMin;
    //private float targetYMax;
    [System.NonSerialized] public bool renderTarget = false;
    private string targetTypeAndTitle = "No target";
    public Image targetObjectLeadGhostImage;

    //Player resources
    [System.NonSerialized] public bool updatePlayerResourcesUIAnimations = true;
    [System.NonSerialized] public Image resourcesImageCurrency;
    [System.NonSerialized] public Image resourcesImagePlatinoid;
    [System.NonSerialized] public Image resourcesImagePreciousMetals;
    [System.NonSerialized] public Image resourcesImageWater;
    [System.NonSerialized] public TextMeshProUGUI resourcesTextCurrency;
    [System.NonSerialized] public TextMeshProUGUI resourcesTextPlatinoid;
    [System.NonSerialized] public TextMeshProUGUI resourcesTextPreciousMetals;
    [System.NonSerialized] public TextMeshProUGUI resourcesTextWater;
    [System.NonSerialized] public TextMeshProUGUI resourcesTextTotalOre;
    [System.NonSerialized] public Image resourcesFillPlatinoid;
    [System.NonSerialized] public Image resourcesFillPreciousMetals;
    [System.NonSerialized] public Image resourcesFillWater;
    [System.NonSerialized] public Transform resourcesIconAndTextWater;
    [System.NonSerialized] public Transform resourcesIconAndTextPlatinoid;
    [System.NonSerialized] public Transform resourcesIconAndTextPreciousMetals;
    [System.NonSerialized] public Transform resourcesWater;
    [System.NonSerialized] public Transform resourcesPlatinoid;
    [System.NonSerialized] public Transform resourcesPreciousMetals;

    //Player weapons
    [System.NonSerialized] public Image weaponCooldown;
    [System.NonSerialized] public TextMeshProUGUI weaponSelectedClipRemainingText;
    [System.NonSerialized] public TextMeshProUGUI weaponSelectedClipSizeText;
    [System.NonSerialized] public TextMeshProUGUI weaponSelectedTitleText;
    [System.NonSerialized] public TextMeshProUGUI weaponAlternateTitleText;
    public Image weaponSelectedIcon;
    public Sprite weaponSelectedIconLaser;
    public Sprite weaponSelectedIconSeismicCharge;
    public Sprite weaponSelectedIconNone;

    //Player camera reticle
    [System.NonSerialized] public GameObject cameraReticle;

    //Player ship direction reticle
    [System.NonSerialized] public GameObject playerShipDirectionReticleTree;
    public GameObject playerShipDirectionReticlePrefab;
    [System.NonSerialized] public List<GameObject> playerShipDirectionReticleList = new List<GameObject>();
    private short playerShipDirectionReticleListLength = 2; //16;

    //Map
    [System.NonSerialized] public static bool displayMap = false;

    //Tips
    [System.NonSerialized] public TextMeshProUGUI tipText;
    [System.NonSerialized] public float tipAimNeedsHelpCertainty = 0f;
    private readonly float TIP_CERTAINTY_DECAY = 0.003f; //0.3f; //0.003f;
    private readonly float TIP_AIM_THRESHOLD_CERTAINTY = 2f; //4f;
    [System.NonSerialized] public readonly float TIP_AIM_THRESHOLD_ACCURACY = 0.995f;
    public string tipAimText;
    private float tipTextAlphaDecrementDelay = 0f;
    private readonly float TIP_TEXT_ALPHA_DECREMENT_RATE = 0.01f;

    //Colours
    private readonly Color COLOR_UI_TRANSLUCENT_WHITE = new Color(1f, 1f, 1f, 0.5f);
    private readonly Color COLOR_UI_TRANSLUCENT_AMBER = new Color(1f, 0.75f, 0f, 0.5f); //new Color(1f, 0.34f, 0.2f, 0.5f);
    private readonly Color COLOR_UI_TRANSLUCENT_RED = new Color(1f, 0f, 0f, 0.5f);

    //Abilities
    public Transform abilities;
    [System.NonSerialized] public Transform abilityEclipseVision;
    [System.NonSerialized] public Transform abilitySpotlight;

    private void Awake()
    {
        //System info
        systemInfo = canvas.transform.Find("HUD Top-Right").Find("SystemInfo").GetComponent<TextMeshProUGUI>();

        //Reticle
        cameraReticle = canvas.transform.Find("HUD Centre").Find("CameraReticle").gameObject;

        //Waypoint and target
        Transform waypointFolder = canvas.transform.Find("HUD Centre").Find("Waypoint");
        waypointImage = waypointFolder.Find("Waypoint").GetComponent<Image>();
        waypointTextType = waypointFolder.Find("Waypoint Type Text").GetComponent<TextMeshProUGUI>();
        waypointTextTitle = waypointFolder.Find("Waypoint Title Text").GetComponent<TextMeshProUGUI>();
        waypointTextBody = waypointFolder.Find("Waypoint Body Text").GetComponent<TextMeshProUGUI>();
        targetImage = waypointFolder.Find("Target").GetComponent<Image>();

        //Resources
        Transform resourcesFolder = canvas.transform.Find("HUD Top-Left").Find("Resources");

        resourcesImageCurrency =                resourcesFolder.Find("Currency").GetComponent<Image>();
        resourcesTextCurrency =                 resourcesFolder.Find("Currency Text").GetComponent<TextMeshProUGUI>();

        resourcesWater =                        resourcesFolder.Find("Total Ore").Find("Water");
        resourcesFillWater =                    resourcesWater.Find("Fill").GetComponent<Image>();
        resourcesIconAndTextWater =             resourcesWater.Find("Icon and Text");
        resourcesImageWater =                   resourcesIconAndTextWater.Find("Icon").GetComponent<Image>();
        resourcesTextWater =                    resourcesIconAndTextWater.Find("Text").GetComponent<TextMeshProUGUI>();

        resourcesPlatinoid =                    resourcesFolder.Find("Total Ore").Find("Platinoid");
        resourcesFillPlatinoid =                resourcesPlatinoid.Find("Fill").GetComponent<Image>();
        resourcesIconAndTextPlatinoid =         resourcesPlatinoid.Find("Icon and Text");
        resourcesImagePlatinoid =               resourcesIconAndTextPlatinoid.Find("Icon").GetComponent<Image>();
        resourcesTextPlatinoid =                resourcesIconAndTextPlatinoid.Find("Text").GetComponent<TextMeshProUGUI>();

        resourcesPreciousMetals =               resourcesFolder.Find("Total Ore").Find("Precious Metals");
        resourcesFillPreciousMetals =           resourcesPreciousMetals.Find("Fill").GetComponent<Image>();
        resourcesIconAndTextPreciousMetals =    resourcesPreciousMetals.Find("Icon and Text");
        resourcesImagePreciousMetals =          resourcesIconAndTextPreciousMetals.Find("Icon").GetComponent<Image>();
        resourcesTextPreciousMetals =           resourcesIconAndTextPreciousMetals.Find("Text").GetComponent<TextMeshProUGUI>();

        resourcesTextTotalOre =   resourcesFolder.Find("Total Ore Text").GetComponent<TextMeshProUGUI>();

        //Weapons
        Transform weaponsFolder = canvas.transform.Find("HUD Bottom-Right").Find("Weapons");
        weaponCooldown =                    weaponsFolder.Find("Cooldown").GetComponent<Image>();
        weaponSelectedClipRemainingText =   weaponsFolder.Find("Selected Clip Remaining Text").GetComponent<TextMeshProUGUI>();
        weaponSelectedClipSizeText =        weaponsFolder.Find("Selected Clip Size Text").GetComponent<TextMeshProUGUI>();
        weaponSelectedTitleText =           weaponsFolder.Find("Selected Title Text").GetComponent<TextMeshProUGUI>();
        weaponAlternateTitleText =          weaponsFolder.Find("Alternate Title Text").GetComponent<TextMeshProUGUI>();

        //Tips
        tipText = canvas.transform.Find("HUD Bottom").Find("Tips").Find("Tip Text").GetComponent<TextMeshProUGUI>();

        //Abilities
        abilityEclipseVision =  abilities.Find("Eclipse Vision");
        abilitySpotlight =      abilities.Find("Spotlight");
    }

    private void Start()
    {
        UpdateUIBindDisplays();

        //Waypoint
        waypointXMin = waypointImage.GetPixelAdjustedRect().width / 2;
        waypointXMax = Screen.width - waypointXMin;
        waypointYMin = waypointImage.GetPixelAdjustedRect().height / 2;
        waypointYMax = Screen.height - waypointYMin;

        //Target
        //targetXMin = targetImage.GetPixelAdjustedRect().width / 2;
        //targetXMax = Screen.width - targetXMin;
        //targetYMin = targetImage.GetPixelAdjustedRect().height / 2;
        //targetYMax = Screen.height - targetYMin;
    }

    private void Update()
    {
        //FPS display
        if (control.settings.displayFPS)
        {
            if (Time.frameCount % FPS_PRINT_PERIOD == 0)
            {
                control.fps = (int)(1f / Time.unscaledDeltaTime);
            }

            if (control.IS_EDITOR)
            {
                systemInfo.text = "test";


                string fps = control.fps.ToString() + "FPS";
                string asteroids = "Asteroids: " + control.generation.asteroidsEnabled.transform.childCount + " (" + control.generation.asteroidsDetailed + " detailed)";
                string ores = "Ores: " + control.generation.oreEnabled.transform.childCount;

                systemInfo.text = fps + "\n" + asteroids + "\n" + ores;
                //systemInfo.text = control.fps.ToString() + "FPS"
                //+ "\n Asteroids: " + control.generation.asteroidsEnabled.transform.childCount + " (" + control.generation.asteroidsDetailed + " detailed)"
                //+ "\n Ores: " + control.generation.oreEnabled.transform.childCount;
            }
            else
            {
                systemInfo.text = control.fps.ToString() + "FPS";
            }
        }

        //Resources animations
        if (updatePlayerResourcesUIAnimations)
        {
            UpdateAllPlayerResourcesUIAnimations();
        }

        //Tip certainty
        if (control.settings.tips)
        {
            UpdateTipCertainty();
        }
        
        //Tip animation
        if (tipText.color.a > 0f)
        {
            if (tipTextAlphaDecrementDelay <= 0f)
            {
                //Text slowly becomes translucent
                float tipTextAlphaAdjustment = tipText.color.a - (TIP_TEXT_ALPHA_DECREMENT_RATE * ((1f - tipText.color.a) + TIP_TEXT_ALPHA_DECREMENT_RATE));
                tipText.color = new Color(1f, 1f, 1f, Mathf.Max(0f, tipTextAlphaAdjustment));
            }
            else
            {
                //Decrement the decrement delay
                tipTextAlphaDecrementDelay = Mathf.Max(0f, tipTextAlphaDecrementDelay - Time.deltaTime);
            }
        }
    }

    private void LateUpdate()
    {
        if (control.generation.playerSpawned)
        {
            //THESE MUST BE CALLED IN LateUpdate() OTHERWISE THEY WILL RENDER TOO SOON, CAUSING A SHAKING EFFECT IN-GAME
            //Waypoint
            UpdateWaypointAndTargetUI();

            //Player Ship Facing Direction Reticle
            UpdatePlayerShipFacingDirectionReticleUI();
        }
    }

    #region Tip
    private void UpdateTipCertainty()
    {
        //Provide a tip if a player repeatedly demonstrates a lack of understanding about that game feature

        if (control.generation.playerSpawned)
        {
            Player playerScript = control.GetPlayerScript();

            //AUTO TORQUING
            //Cancel tip?
            bool isPlayerTargettingBandit = false;
            if (playerScript.targetObject != null)
            {
                isPlayerTargettingBandit = (control.GetPlayerScript().targetObject.name == control.generation.enemy.name + "(Clone)");
            }
            if (
                playerScript.vitalsFuel <= 0d
                || binds.GetInputDown(binds.bindAlignShipToReticle)
                || isPlayerTargettingBandit
                || Time.time < playerScript.combatLastAggroTime + playerScript.COMBAT_PERIOD_THRESHOLD_TIMEOUT
            )
            {
                tipAimNeedsHelpCertainty = 0f;
            }

            if (tipAimNeedsHelpCertainty > TIP_AIM_THRESHOLD_CERTAINTY)
            {
                SetTip(
                    "Hold " + control.binds.GetBindAsPrettyString(control.binds.bindAlignShipToReticle, true) + " to torque your ship in the direction you're looking!",
                    ref tipAimNeedsHelpCertainty
                );
            }

            tipAimNeedsHelpCertainty = Mathf.Max(0f, tipAimNeedsHelpCertainty - (TIP_CERTAINTY_DECAY * Time.deltaTime));
        }
    }

    public void SetTip(string text)
    {
        if (control.settings.tips || control.GetPlayerScript().isDestroyed)
        {
            tipText.text = text;
            tipText.color = Color.white; //reset alpha
        }
    }

    public void SetTip(string text, float duration)
    {
        if (control.settings.tips || control.GetPlayerScript().isDestroyed)
        {
            tipText.text = text;
            tipText.color = Color.white; //reset alpha

            tipTextAlphaDecrementDelay = duration;
        }
    }

    public void SetTip(string text, ref float certainty)
    {
        if (control.settings.tips || control.GetPlayerScript().isDestroyed)
        {
            tipText.text = text;
            tipText.color = Color.white; //reset alpha

            certainty = 0f;
        }
    }

    
    #endregion

    public void ToggleMapView()
    {
        //Map cannot be opened while menu is
        if (Menu.menuOpenAndGamePaused)
        {
            displayMap = false;
        }
        else
        {
            displayMap = !displayMap;
        }
        
        //Cursor and camera reticle
        Cursor.lockState = (CursorLockMode)System.Convert.ToByte(!displayMap);    //toggle cursor lock
        //cameraReticle.SetActive(!displayMap);

        //Player and map
        //Map light
        //control.GetPlayerScript().mapLight.SetActive(displayMap);
        //Map ship model
        control.GetPlayerScript().transform.parent.Find("Position Mount").Find("Centre Mount").Find("Ship Map Model").gameObject.SetActive(displayMap);

        //Heighliner map lines
        for (int planetIndex = 0; planetIndex < control.generation.planets.transform.childCount; planetIndex++)
        {
            //Refs
            GameObject planetInstance = control.generation.planets.transform.GetChild(planetIndex).gameObject;
            GameObject heighliner = planetInstance.GetComponent<Planet>().heighliner0; //we only care about heighliner0 and not heighliner1, because all 0s link to 1s
            HeighlinerEntry heighlinerScript = heighliner.GetComponentInChildren<HeighlinerEntry>(); //we only care about heighliner0 and not heighliner1, because all 0s link to 1s
            Transform heighlinerMapLineModel = heighliner.transform.Find("HeighlinerMapLineModel(Clone)");

            //Protect against null ref exception (happens for the first 5 frames, minimum)
            if (heighlinerScript.exitNode != null)
            {
                //Only display line after discovered
                if (heighlinerScript.isDiscovered)
                {
                    heighlinerMapLineModel.gameObject.SetActive(displayMap);
                }
            }
            else
            {
                Debug.Log("Heighliner has no map line model!");
            }
        }
        //for (int heighlinerIndex = 0; heighlinerIndex < control.generation.heighlinerList.Count; heighlinerIndex++)
        //{
        //    //Refs
        //    GameObject heighliner = control.generation.heighlinerList[heighlinerIndex];
        //    Transform heighlinerMapLineModel = heighliner.transform.Find("Jump Trigger Volume").Find("HeighlinerMapLineModel(Clone)");
        //
        //    //Protect against null ref exception
        //    if (heighlinerMapLineModel != null)
        //    { 
        //        //Only display line after discovered
        //        if (heighliner.GetComponentInChildren<HeighlinerEntry>().isDiscovered)
        //        {
        //            heighlinerMapLineModel.gameObject.SetActive(displayMap);
        //        }
        //    }
        //    else
        //    {
        //        Debug.Log("Heighliner has no map line model!");
        //    }
        //}

        if (displayMap)
        {
            //Ship cameras
            control.GetPlayerScript().fpCam.SetActive(!displayMap);
            control.GetPlayerScript().tpCam.SetActive(!displayMap);

            //Map camera
            control.GetPlayerScript().mapCam.SetActive(displayMap);

            //Background stars
            control.GetPlayerScript().skyboxStarsParticleSystem.gameObject.SetActive(false);

            //Enlarge player ship model
            //control.GetPlayerScript().tpModel.transform.localScale = Vector3.one * 200.0f;
        }
        else
        {
            //Ship cameras
            control.GetPlayerScript().fpCam.SetActive(!displayMap);
            control.GetPlayerScript().DecideWhichModelsToRender();

            //Map camera
            control.GetPlayerScript().mapCam.SetActive(displayMap);

            //Background stars (re-enable)
            control.GetPlayerScript().skyboxStarsParticleSystem.gameObject.SetActive(true);
            control.GetPlayerScript().skyboxStarsParticleSystem.Emit(control.GetPlayerScript().SKYBOX_STARS_COUNT);

            //Return size of player ship model to default
            //control.GetPlayerScript().tpModel.transform.localScale = Vector3.one * 4f;
        }
    }

    #region UI: Player ship direction reticle
    public void CreatePlayerShipDirectionReticles()
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
                playerShipDirectionReticlePrefab,
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

    private void UpdatePlayerShipFacingDirectionReticleUI()
    {
        if (playerShipDirectionReticleTree != null)
        {
            for (int i = 0; i <= playerShipDirectionReticleListLength - 1; i++)
            {
                //Get references
                Transform instancePlayerBodyTransform = control.generation.instancePlayer.transform.Find("Body");
                GameObject instancePlayerShipDirectionReticle = playerShipDirectionReticleList[i];
                DirectionReticle instancePlayerShipDirectionReticleScript = instancePlayerShipDirectionReticle.GetComponent<DirectionReticle>();

                Vector3 reticleWorldPos = instancePlayerBodyTransform.position
                    + ((instancePlayerBodyTransform.rotation * Vector3.forward)
                    * (2.5f + (i * 500.0f))
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
    }
    #endregion

    #region UI: Waypoint & target
    private void SetWaypointUI(RaycastHit hit)
    {
        renderWaypoint = true;

        Vector2 waypointUIPos = Get2DUICoordsFrom3DWorldPosition(
            hit.collider.transform.position,
            waypointImage,
            false
        );

        waypointImage.transform.position = waypointUIPos;

        waypointTextType.transform.position = new Vector2(waypointUIPos.x + WAYPOINT_X_OFFSET, waypointUIPos.y + WAYPOINT_Y_OFFSET + waypointTextBody.fontSize + waypointTextTitle.fontSize + waypointTextType.fontSize);
        waypointTextTitle.transform.position = new Vector2(waypointUIPos.x + WAYPOINT_X_OFFSET, waypointUIPos.y + WAYPOINT_Y_OFFSET + waypointTextBody.fontSize + waypointTextTitle.fontSize);
        waypointTextBody.transform.position = new Vector2(waypointUIPos.x + WAYPOINT_X_OFFSET, waypointUIPos.y + WAYPOINT_Y_OFFSET + waypointTextBody.fontSize);
    }

    public void UpdateTargetConsole()
    {
        TextMesh consoleTargetTypeAndTitleText = control.generation.instancePlayer.transform.Find("Body").Find("FP Model").Find("Interior").Find("Display Strut Left").Find("Target Type And Title Text").GetComponent<TextMesh>();
        targetTypeAndTitle = waypointTextType.text + "\n" + waypointTextTitle.text;
        consoleTargetTypeAndTitleText.text = targetTypeAndTitle;
    }

    private void SetPlayerTargetUI()
    {
        //Cancel and remove target if object (such as an asteroid) has been destroyed
        GameObject targetObj = control.generation.instancePlayer.GetComponentInChildren<Player>().targetObject;

        if (
            targetObj == null
            || (
                targetObj.name == control.generation.asteroid.name + "(Clone)"
                && targetObj.GetComponent<Asteroid>().isDestroying
            )
            || (
                targetObj.name == control.generation.asteroid.name + "(Clone)"
                && targetObj.GetComponent<Asteroid>().isDestroyed
            )
            || (
                targetObj.name == control.generation.enemy.name + "(Clone)"
                && targetObj.GetComponent<Enemy>().isDestroying
            )
            || (
                targetObj.name == control.generation.enemy.name + "(Clone)"
                && targetObj.GetComponent<Enemy>().isDestroyed
            )
        )
        {
            targetImage.gameObject.SetActive(false);
            renderTarget = false;
            return;
        }

        targetImage.transform.position = Get2DUICoordsFrom3DWorldPosition(
            control.GetPlayerScript().targetObject.transform.position, 
            targetImage,
            true
        );
    }

    public Vector2 Get2DUICoordsFrom3DWorldPosition(Vector3 worldPosition, Image image, bool clamp)
    {
        //Transform world to screen
        Vector2 uiPos = Camera.main.WorldToScreenPoint(worldPosition);

        //Clamp
        float targetXMin = image.GetPixelAdjustedRect().width / 2;
        float targetXMax = Screen.width - targetXMin;
        float targetYMin = image.GetPixelAdjustedRect().height / 2;
        float targetYMax = Screen.height - targetYMin;

        //If behind camera, insist on being against screen edge
        float dot = Vector3.Dot(worldPosition - Camera.main.transform.position, Camera.main.transform.forward);
        if (dot < 0f)
        {
            if (clamp)
            {
                //Invert the target position - otherwise it is inverted as it's "extruded" through the camera' Z axis, like a mirror does
                uiPos.x = Screen.width - uiPos.x;
                uiPos.y = Screen.height - uiPos.y;

                //Break screen up into a crossed rectangle and find which triangular section the target lies in - some var names here may be inaccurate
                Vector2 pointTopLeft = new Vector2(0f, 0f);
                Vector2 pointTopRight = new Vector2(Screen.width, 0f);
                Vector2 pointBottomLeft = new Vector2(0f, Screen.height);
                Vector2 pointBottomRight = new Vector2(Screen.width, Screen.height);
                Vector2 pointMiddle = new Vector2(Screen.width / 2f, Screen.height / 2f);

                bool inTopCross = control.GetIfPointLiesWithinTriangle(uiPos, pointMiddle, pointBottomLeft, pointBottomRight);
                bool inLeftCross = control.GetIfPointLiesWithinTriangle(uiPos, pointMiddle, pointTopLeft, pointBottomLeft);
                bool inBottomCross = control.GetIfPointLiesWithinTriangle(uiPos, pointMiddle, pointTopRight, pointTopLeft);
                bool inRightCross = control.GetIfPointLiesWithinTriangle(uiPos, pointMiddle, pointBottomRight, pointTopRight);

                //Debug.Log(
                //    "inTopCross: " + inTopCross
                //    + "    inLeftCross: " + inLeftCross
                //    + "\ninBottomCross: " + inBottomCross
                //    + "    inRightCross: " + inRightCross
                //);

                //Continue the target from wherever it is in the middle toward the edge of the screen
                float angleToEdge = Mathf.Atan2(uiPos.y - (Screen.height / 2f), uiPos.x - (Screen.width / 2f));
                Vector2 coordsToEdge = Vector2.zero;

                if (inTopCross)
                {
                    coordsToEdge.y = uiPos.y;
                    coordsToEdge.x = coordsToEdge.y / Mathf.Tan(angleToEdge); //x = y/Tan(angle)
                }
                else if (inBottomCross)
                {
                    coordsToEdge.y = Screen.height - uiPos.y;
                    coordsToEdge.x = coordsToEdge.y / Mathf.Tan(angleToEdge); //x = y/Tan(angle)
                }
                else if (inLeftCross)
                {
                    coordsToEdge.x = uiPos.x;
                    coordsToEdge.y = uiPos.y;
                }
                else if (inRightCross)
                {
                    coordsToEdge.x = Screen.width - uiPos.x;
                    coordsToEdge.y = Mathf.Tan(angleToEdge) * coordsToEdge.x; //y = tan(angle)x
                }

                float distanceToEdge = Mathf.Sqrt(Mathf.Pow(coordsToEdge.x, 2f) + Mathf.Pow(coordsToEdge.y, 2f));

                uiPos.x += Mathf.Cos(angleToEdge) * distanceToEdge;
                uiPos.y += Mathf.Sin(angleToEdge) * distanceToEdge;
            }
            else
            {
                uiPos.x = -image.GetPixelAdjustedRect().width;
                uiPos.y = -image.GetPixelAdjustedRect().height;
            }
        }

        //Clamp to screen edge
        if (clamp)
        {
            uiPos.x = Mathf.Clamp(uiPos.x, targetXMin, targetXMax);
            uiPos.y = Mathf.Clamp(uiPos.y, targetYMin, targetYMax);
        }
        
        return uiPos;
    }

    public void SetPlayerTargetObject(GameObject objectToTarget)
    {
        //Set or toggle the target object

        //If the target hasn't been set to anything, we don't try to check if what's selected is currently the target since we already know the result and it would throw an error
        //If the player clicks on what is currently the target, it unsets it from being the target
        //If the player clicks on what is NOT already the target, we set that as the target

        if (control.generation.instancePlayer.GetComponentInChildren<Player>().targetObject == null)
        {
            //control.generation.instancePlayer.GetComponentInChildren<Player>().targetObject = objectToTarget;

            //if (!renderTarget)
            //{
            //    targetImage.gameObject.SetActive(true);
            //    renderTarget = true;
            //}

            control.generation.instancePlayer.GetComponentInChildren<Player>().targetObject = objectToTarget;

            if (objectToTarget == null)
            {
                targetImage.gameObject.SetActive(false);
                renderTarget = false;
            }
            else
            {
                if (!renderTarget)
                {
                    targetImage.gameObject.SetActive(true);
                    renderTarget = true;
                }
            }
        }
        else
        {
            if (control.generation.instancePlayer.GetComponentInChildren<Player>().targetObject == objectToTarget)
            {
                targetImage.gameObject.SetActive(false);
                renderTarget = false;
                control.generation.instancePlayer.GetComponentInChildren<Player>().targetObject = null;
            }
            else
            {
                control.generation.instancePlayer.GetComponentInChildren<Player>().targetObject = objectToTarget;

                if (!renderTarget)
                {
                    targetImage.gameObject.SetActive(true);
                    renderTarget = true;
                }
            }
        }
    }

    private void UpdateWaypointAndTargetUI()
    {
        //Console
        TextMesh consoleTargetInfoText         = control.generation.instancePlayer.transform.Find("Body").Find("FP Model").Find("Interior").Find("Display Strut Right").Find("Target Info Text").GetComponent<TextMesh>();
        TextMesh consoleTargetTypeAndTitleText = control.generation.instancePlayer.transform.Find("Body").Find("FP Model").Find("Interior").Find("Display Strut Left").Find("Target Type And Title Text").GetComponent<TextMesh>();

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

        //Raycast for waypoint AND target
        RaycastHit rayHit = new RaycastHit();
        if (Physics.Raycast(waypointRaycastOrigin, waypointRaycastDirection, maxDist))
        {
            RaycastHit hit = new RaycastHit();
            Physics.Raycast(waypointRaycastOrigin, waypointRaycastDirection, out hit, maxDist);
            //Debug.DrawRay(waypointRaycastOrigin, waypointRaycastDirection * hit.distance, Color.green, Time.deltaTime, false);
            rayHit = hit; //to use with target later

            //Waypoint
            if (hit.collider.gameObject.name == control.generation.star.name + "(Clone)")
            {
                //Waypoint
                waypointTextType.text = "Star";
                waypointTextTitle.text = hit.collider.gameObject.GetComponent<NameCelestial>().title;
                waypointTextBody.text = GetDistanceAndDeltaVUI(hit.collider.gameObject, false);

                //Console waypoint
                consoleTargetTypeAndTitleText.text = waypointTextType.text + "\n" + waypointTextTitle.text;

                //Update UI
                SetWaypointUI(hit);
            }
            else if (hit.collider.gameObject.name == control.generation.planet.name + "(Clone)")
            {
                //Waypoint
                waypointTextType.text = "Planet";
                waypointTextTitle.text = hit.collider.gameObject.GetComponent<NameCelestial>().title;
                waypointTextBody.text = GetDistanceAndDeltaVUI(hit.collider.gameObject, false);
                
                //Console waypoint
                consoleTargetTypeAndTitleText.text = waypointTextType.text + "\n" + waypointTextTitle.text;

                //Update UI
                SetWaypointUI(hit);
            }
            else if (hit.collider.gameObject.name == control.generation.moon.name + "(Clone)")
            {
                //Waypoint
                waypointTextType.text = "Moon";
                string titleText = hit.collider.gameObject.GetComponent<NameCelestial>().title;
                //Has satellite?
                if (hit.collider.GetComponent<Moon>().isDiscovered)
                {
                    if (hit.collider.GetComponent<Moon>().hasStation)
                    {
                        titleText += " - has station";
                    }
                    else if (hit.collider.GetComponent<Moon>().hasHeighliner)
                    {
                        titleText += " - has heighliner";
                    }
                    else
                    {
                        titleText += " - devoid";
                    }
                }
                waypointTextTitle.text = titleText;
                waypointTextBody.text = GetDistanceAndDeltaVUI(hit.collider.gameObject, false);
                
                //Console waypoint
                consoleTargetTypeAndTitleText.text = waypointTextType.text + "\n" + waypointTextTitle.text;

                //Update UI
                SetWaypointUI(hit);
            }
            else if (!displayMap && hit.collider.gameObject.name == control.generation.station.name + "(Clone)")
            {
                //Waypoint
                waypointTextType.text = "Station";
                Transform stationTransform = hit.collider.transform;
                string stationTitle = stationTransform.GetComponent<NameHuman>().title;
                Transform moonTransform = stationTransform.parent.GetChild(stationTransform.GetSiblingIndex() - 1);
                string moonName = moonTransform.GetComponent<NameCelestial>().title;
                waypointTextTitle.text = stationTitle + " of " + moonName;
                waypointTextBody.text = GetDistanceAndDeltaVUI(hit.collider.gameObject, true);
                
                //Console waypoint
                consoleTargetTypeAndTitleText.text = waypointTextType.text + "\n" + waypointTextTitle.text;

                //Update UI
                SetWaypointUI(hit);
            }
            else if (!displayMap && hit.collider.gameObject.name == control.generation.heighliner.name + "(Clone)")
            {
                //Waypoint
                waypointTextType.text = "Heighliner";
                HeighlinerEntry heighlinerScript = hit.collider.transform.GetComponentInChildren<HeighlinerEntry>();
                string hostCBodyName = heighlinerScript.parentPlanet.GetComponent<NameCelestial>().title;
                string finalTitle = "Interplanetary. Orbiting " + hostCBodyName;
                if (heighlinerScript.isDiscovered)
                {
                    HeighlinerEntry exitNodeHeighlinerScript = heighlinerScript.exitNode.transform.GetComponentInChildren<HeighlinerEntry>();
                    string exitNodeHeighlinerMoonName = exitNodeHeighlinerScript.parentPlanet.GetComponent<NameCelestial>().title;
                    finalTitle = "Interplanetary. " + hostCBodyName + " to " + exitNodeHeighlinerMoonName;
                }
                waypointTextTitle.text = finalTitle;
                //waypointTextTitle.text = hit.collider.gameObject.GetComponent<NameHuman>().title;
                waypointTextBody.text = GetDistanceAndDeltaVUI(hit.collider.gameObject, false);

                //Console waypoint
                consoleTargetTypeAndTitleText.text = waypointTextType.text + "\n" + waypointTextTitle.text;

                //Update UI
                SetWaypointUI(hit);
            }
            else if (!displayMap && hit.collider.gameObject.name == control.generation.asteroid.name + "(Clone)")
            {
                //Waypoint
                waypointTextType.text = "Asteroid";
                Asteroid.Size size = hit.collider.gameObject.GetComponent<Asteroid>().size;
                if (size == Asteroid.Size.small)
                {
                    waypointTextTitle.text = "Small";
                }
                else if (size == Asteroid.Size.medium)
                {
                    waypointTextTitle.text = "Medium";
                }
                else if (size == Asteroid.Size.large)
                {
                    waypointTextTitle.text = "Large";
                }

                waypointTextBody.text = GetDistanceAndDeltaVUI(hit.collider.gameObject, false);
                
                //Console waypoint
                consoleTargetTypeAndTitleText.text = waypointTextType.text + "\n" + waypointTextTitle.text;

                //Update UI
                SetWaypointUI(hit);
            }
            else if (!displayMap && hit.collider.gameObject.name == control.generation.enemy.name + "(Clone)")
            {
                //Waypoint
                //Type
                waypointTextType.text = "Bandit";

                //Title
                //Read enum variable as string
                waypointTextTitle.text = hit.collider.gameObject.GetComponent<Enemy>().strength.ToString();
                //Capitalize first letter
                waypointTextTitle.text = waypointTextTitle.text.Substring(0, 1).ToUpper() + waypointTextTitle.text.Substring(1, waypointTextTitle.text.Length - 1);

                //Enemy.Strength strength = hit.collider.gameObject.GetComponent<Enemy>().strength;
                //if (strength == Enemy.Strength.minor)
                //{
                //    waypointTextTitle.text = "Minor";
                //}
                //else if (strength == Enemy.Strength.major)
                //{
                //    waypointTextTitle.text = "Major";
                //}
                //else if (strength == Enemy.Strength.elite)
                //{
                //    waypointTextTitle.text = "Elite";
                //}
                //else if (strength == Enemy.Strength.ultra)
                //{
                //    waypointTextTitle.text = "Ultra";
                //}

                //Body
                waypointTextBody.text = GetDistanceAndDeltaVUI(hit.collider.gameObject, false);

                //Console waypoint
                consoleTargetTypeAndTitleText.text = waypointTextType.text + "\n" + waypointTextTitle.text;

                //Update UI
                SetWaypointUI(hit);
            }
            //else
            //{
            //    Debug.Log("Undefined object " + hit.collider.gameObject.name + " hit " + hit.distance + " units away");
            //}
        }
        else if (control.generation.instancePlayer.GetComponentInChildren<Player>().targetObject != null)
        {
            //Console target
            consoleTargetTypeAndTitleText.text = targetTypeAndTitle;
            GetDistanceAndDeltaVUI(control.generation.instancePlayer.GetComponentInChildren<Player>().targetObject, false);
        }
        else
        {
            //Console default
            consoleTargetTypeAndTitleText.text = "No target";
            consoleTargetInfoText.text = "\n";
        }

        waypointImage.gameObject.SetActive(renderWaypoint);
        waypointTextType.gameObject.SetActive(renderWaypoint);
        waypointTextTitle.gameObject.SetActive(renderWaypoint);
        waypointTextBody.gameObject.SetActive(renderWaypoint);

        //Target
        if (binds.GetInputDown(binds.bindSetTarget))
        {
            //Target
            if (rayHit.collider == null)
            {
                if (!UI.displayMap)
                {
                    //Toggle off by pressing the target keybind while looking at nothing
                    SetPlayerTargetObject(null);
                }
            }
            else
            {
                //Target what we're looking at
                SetPlayerTargetObject(rayHit.collider.gameObject);
            }

            if (renderTarget)
            {
                UpdateTargetConsole();
            }
        }

        //Target
        if (renderTarget)
        {
            SetPlayerTargetUI();
        }
    }

    private string GetDistanceAndDeltaVUI(GameObject subject, bool isStation)
    {
        Transform playerTransform = control.generation.instancePlayer.transform.GetChild(0);
        Transform subjectTransform = subject.transform;
        Vector3 playerVelocity = playerTransform.GetComponent<Rigidbody>().velocity;

        //Distance
        float conversionRatio = 12.0f; //1 Unity unit = 12 metres
        float distance = Vector3.Distance(subjectTransform.position, playerTransform.position) * conversionRatio;
        string distanceDisplay = " ?, ";
        if (distance < 1e3f)
        {
            distanceDisplay = Mathf.RoundToInt(distance) + " s"; //" m";
        }
        else if (distance >= 1e3f)
        {
            distanceDisplay = (distance * 1e-3f).ToString("F2") + " c"; //" km";
        }
        else if (distance >= 1e6f)
        {
            distanceDisplay = (distance * 1e-6f).ToString("F2") + " k"; //" Mm";
        }
        else if (distance >= 1e9f)
        {
            distanceDisplay = (distance * 3.33564e-9f).ToString("F2") + " p"; //" lightsecond";
        }
        else if (distance >= 5.5594e11f)
        {
            distanceDisplay = (distance * 5.5594e-11f).ToString("F2") + " g"; //" lightminute";
        }
        else if (distance >= 1.057e16f)
        {
            distanceDisplay = (distance * 1.057e-16f).ToString("F2") + " l"; //" lightyear";
        }

        //DeltaV
        Vector3 subjectVelocity;
        if (isStation)
        {
            subjectVelocity = Vector3.zero;
            //subjectVelocity = subjectTransform.GetComponent<StationOrbit>().planetoidToOrbit.GetComponent<Rigidbody>().velocity;
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
        TextMesh consoleTargetInfoText = control.generation.instancePlayer.transform.Find("Body").Find("FP Model").Find("Interior").Find("Display Strut Right").Find("Target Info Text").GetComponent<TextMesh>();
        consoleTargetInfoText.text = distanceDisplay + "\n" + deltaVDisplay;

        //Return (for waypoint)
        return distanceDisplay + ", " + deltaVDisplay;
    }
    #endregion

    #region UI: Player resources
    public void UpdatePlayerVitalsDisplay()
    {
        Player playerScript = control.GetPlayerScript();
        
        float healthUnitInterval = (float)(playerScript.vitalsHealth / playerScript.vitalsHealthMax);
        playerScript.vitalsHealthUI.GetComponent<Image>().fillAmount = healthUnitInterval;
        if (healthUnitInterval > 0.5f)
        {
            playerScript.vitalsHealthUI.GetComponent<Image>().color = COLOR_UI_TRANSLUCENT_WHITE;
        }
        else if (healthUnitInterval > 0.25f)
        {
            playerScript.vitalsHealthUI.GetComponent<Image>().color = COLOR_UI_TRANSLUCENT_AMBER;
        }
        else
        {
            playerScript.vitalsHealthUI.GetComponent<Image>().color = COLOR_UI_TRANSLUCENT_RED;
        }
        playerScript.vitalsHealthUIText.text = playerScript.vitalsHealth.ToString("F2");
        
        float fuelUnitInterval = (float)(playerScript.vitalsFuel / playerScript.vitalsFuelMax);
        playerScript.vitalsFuelUI.GetComponent<Image>().fillAmount = fuelUnitInterval;
        if (fuelUnitInterval > 0.5f)
        {
            playerScript.vitalsFuelUI.GetComponent<Image>().color = COLOR_UI_TRANSLUCENT_WHITE;
        }
        else if (fuelUnitInterval > 0.25f)
        {
            playerScript.vitalsFuelUI.GetComponent<Image>().color = COLOR_UI_TRANSLUCENT_AMBER;
        }
        else
        {
            playerScript.vitalsFuelUI.GetComponent<Image>().color = COLOR_UI_TRANSLUCENT_RED;
        }
        playerScript.vitalsFuelUIText.text = playerScript.vitalsFuel.ToString("F2");

        UpdatePlayerConsole();
    }

    public void UpdatePlayerOreWaterText()
    {
        resourcesTextWater.text = control.GetPlayerScript().ore[(int)Asteroid.Type.water].ToString("F0") + " kg / " + control.GetPlayerScript().oreMax;
    }

    public void UpdateAllPlayerResourcesUI()
    {
        Player playerScript = control.GetPlayerScript();

        //Update values and start animations on a resource if its value changed
        UpdatePlayerResourceUI(ref resourcesTextCurrency,           ref resourcesImageCurrency,         playerScript.currency,                                  playerScript.soundSourceCurrencyChange);
        UpdatePlayerResourceUI(ref resourcesTextPlatinoid,          ref resourcesImagePlatinoid,        playerScript.ore[(int)Asteroid.Type.platinoid],         playerScript.soundSourceOreCollected);
        UpdatePlayerResourceUI(ref resourcesTextPreciousMetals,     ref resourcesImagePreciousMetals,   playerScript.ore[(int)Asteroid.Type.preciousMetal],     playerScript.soundSourceOreCollected);
        UpdatePlayerResourceUI(ref resourcesTextWater,              ref resourcesImageWater,            playerScript.ore[(int)Asteroid.Type.water],             playerScript.soundSourceOreCollected);

        //Fills within mixed fill bar
        resourcesFillWater.GetComponent<Image>().fillAmount = (float)(playerScript.ore[(int)Asteroid.Type.water] / control.GetPlayerScript().oreMax);
        resourcesFillPlatinoid.GetComponent<Image>().fillAmount = (float)(playerScript.ore[(int)Asteroid.Type.platinoid] / control.GetPlayerScript().oreMax);
        resourcesFillPreciousMetals.GetComponent<Image>().fillAmount = (float)(playerScript.ore[(int)Asteroid.Type.preciousMetal] / control.GetPlayerScript().oreMax);

        //Positions within mixed fill bar
        //Base positions
        float waterFillWidth = 384f * resourcesFillWater.GetComponent<Image>().fillAmount;
        float platinoidX = waterFillWidth;
        resourcesPlatinoid.GetComponent<RectTransform>().anchoredPosition = new Vector3(platinoidX, 0f, 0f);

        float platinoidFillWidth = 384f * resourcesFillPlatinoid.GetComponent<Image>().fillAmount;
        float preciousMetalsX = platinoidFillWidth + resourcesPlatinoid.GetComponent<RectTransform>().anchoredPosition.x;
        resourcesPreciousMetals.GetComponent<RectTransform>().anchoredPosition = new Vector3(preciousMetalsX, 0f, 0f);

        //Icon and text minimum distance between each other (prevents overlapping)
        float minDist = 66f; //total width of the icon + distance between edge of icon and edge of text + text width + padding

        float iconAndTextOffsetPlatinoid = 0f;
        if (playerScript.ore[(int)Asteroid.Type.water] > 0d)
        {
            iconAndTextOffsetPlatinoid = Mathf.Max(minDist - waterFillWidth, 0f);
        }
        resourcesIconAndTextPlatinoid.GetComponent<RectTransform>().anchoredPosition = new Vector3(iconAndTextOffsetPlatinoid, 0f, 0f);

        float iconAndTextOffsetPreciousMetals = 0f;
        if (playerScript.ore[(int)Asteroid.Type.platinoid] > 0d)
        {
            iconAndTextOffsetPreciousMetals = Mathf.Max((minDist + iconAndTextOffsetPlatinoid) - platinoidFillWidth, 0f);
        }
        resourcesIconAndTextPreciousMetals.GetComponent<RectTransform>().anchoredPosition = new Vector3(iconAndTextOffsetPreciousMetals, 0f, 0f);

        //Total ore text
        resourcesTextTotalOre.text = control.GetPlayerScript().GetTotalOre() + " kg / " + playerScript.oreMax + " kg";

        //Text colours
        //Currency
        float resourceValueRelative = (float)control.GetPlayerScript().currency / Mathf.Max(control.commerce.PRICE_REFUEL, control.commerce.PRICE_REPAIR);
        if (resourceValueRelative >= 3f)
        {
            resourcesTextCurrency.color = COLOR_UI_TRANSLUCENT_WHITE;
        }
        else if (resourceValueRelative >= 1f)
        {
            resourcesTextCurrency.color = COLOR_UI_TRANSLUCENT_AMBER;
        }
        else
        {
            resourcesTextCurrency.color = COLOR_UI_TRANSLUCENT_RED;
        }
        //Max ore
        resourceValueRelative = (float)(control.GetPlayerScript().GetTotalOre() / control.GetPlayerScript().oreMax);
        if (resourceValueRelative <= 0.5f)
        {
            resourcesTextTotalOre.color = COLOR_UI_TRANSLUCENT_WHITE;
        }
        else if (resourceValueRelative <= 0.75f)
        {
            resourcesTextTotalOre.color = COLOR_UI_TRANSLUCENT_AMBER;
        }
        else
        {
            resourcesTextTotalOre.color = COLOR_UI_TRANSLUCENT_RED;
        }

        //Update console
        UpdatePlayerConsole();

        //Set animations to update
        UpdateAllPlayerResourcesUIAnimations();
    }

    private void UpdatePlayerResourceUI(ref TextMeshProUGUI textMeshCurrent, ref Image image, double amount, AudioSource clip)
    {
        if (amount > 0d || image == resourcesImageCurrency)
        {
            image.gameObject.SetActive(true);
            textMeshCurrent.gameObject.SetActive(true);

            string textNew;
            if (image == resourcesImageCurrency)
            {
                textNew = amount.ToString("F0") + " ICC";
            }
            else
            {
                textNew = amount.ToString("F0") + " kg";
            }

            if (textMeshCurrent.text != textNew)
            {
                //Play sound
                clip.Play();

                //Update text
                textMeshCurrent.text = textNew;

                //Start animation (grow)
                float growAmount = 3f;
                image.rectTransform.sizeDelta = new Vector2(
                    (image.sprite.rect.width / 2) * growAmount,
                    (image.sprite.rect.height / 2) * growAmount
                );
            }
        }
        else
        {
            image.gameObject.SetActive(false);
            textMeshCurrent.gameObject.SetActive(false);
        }
    }

    public void UpdateAllPlayerResourcesUIAnimations()
    {
        //Default looping to disabled. If there are any changes the animations make, they will turn the looping back on
        updatePlayerResourcesUIAnimations = false;

        //Animate
        UpdatePlayerResourcesUIAnimation(ref resourcesImageCurrency);
        UpdatePlayerResourcesUIAnimation(ref resourcesImagePlatinoid);
        UpdatePlayerResourcesUIAnimation(ref resourcesImagePreciousMetals);
        UpdatePlayerResourcesUIAnimation(ref resourcesImageWater);
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

    private void UpdatePlayerConsole()
    {
        Player playerScript = control.GetPlayerScript();

        //Vitals
        TextMesh consoleVitalsText = control.generation.instancePlayer.transform.Find("Body").Find("FP Model").Find("Interior").Find("Console").Find("Vitals Text").GetComponent<TextMesh>();
        consoleVitalsText.text =
                     "Hull integrity: " + playerScript.vitalsHealth.ToString("F2")
            + "\n" + "Engine fuel: " + playerScript.vitalsFuel.ToString("F2");

        //Weapons
        TextMesh consoleWeaponsText = control.generation.instancePlayer.transform.Find("Body").Find("FP Model").Find("Interior").Find("Console").Find("Weapons Text").GetComponent<TextMesh>();
        consoleWeaponsText.text =
                     "Weapon: " + playerScript.GetWeaponSelectedName()
            + "\n" + "Cooldown: " + Mathf.RoundToInt(weaponCooldown.fillAmount * 100f).ToString() + "%";

        //Cargo
        TextMesh consoleCargoText = control.generation.instancePlayer.transform.Find("Body").Find("FP Model").Find("Interior").Find("Console").Find("Cargo Text").GetComponent<TextMesh>();
        consoleCargoText.text =
                     "Currency: " + resourcesTextCurrency.text
            + "\n" + "Platinoid: " + resourcesTextPlatinoid.text
            + "\n" + "Precious metal: " + resourcesTextPreciousMetals.text
            + "\n" + "Water ice: " + resourcesTextWater.text;
    }
    #endregion

    #region Player weapons
    public void UpdatePlayerWeaponsUI()
    {
        Player playerScript = control.generation.instancePlayer.GetComponentInChildren<Player>();

        //Clip max text
        weaponSelectedClipSizeText.text = playerScript.weaponSelectedClipSize.ToString();

        //Clip remaining text
        weaponSelectedClipRemainingText.text = playerScript.weaponSelectedClipRemaining.ToString();

        //Single and clip joint-cooldown bar
        weaponCooldown.fillAmount = Mathf.Max(
            0f,
            playerScript.weaponSelectedSingleCooldownCurrent / playerScript.weaponSelectedSingleCooldownDuration,
            playerScript.weaponSelectedClipCooldownCurrent / playerScript.weaponSelectedClipCooldownDuration
        );
    }

    public void UpdateWeapons()
    {
        Player playerScript = control.GetPlayerScript();

        //Name
        if (playerScript.weaponSlotSelected == 0)
        {
            //SELECTING SLOT 0
            weaponSelectedTitleText.text = playerScript.weaponSlot0.NAME + " " + control.binds.GetBindAsPrettyString(binds.bindSelectWeaponSlot0, true);
            weaponAlternateTitleText.text = playerScript.weaponSlot1.NAME + " " + control.binds.GetBindAsPrettyString(binds.bindSelectWeaponSlot1, true);
        }
        else
        {
            //SELECTING SLOT 1
            weaponSelectedTitleText.text = playerScript.weaponSlot1.NAME + " " + control.binds.GetBindAsPrettyString(binds.bindSelectWeaponSlot1, true);
            weaponAlternateTitleText.text = playerScript.weaponSlot0.NAME + " " + control.binds.GetBindAsPrettyString(binds.bindSelectWeaponSlot0, true);
        }

        //Ammo
        weaponSelectedClipSizeText.text = playerScript.weaponSelectedClipSize.ToString();
        weaponSelectedClipRemainingText.text = playerScript.weaponSelectedClipRemaining.ToString();

        //Sprite
        if (playerScript.GetWeaponSelectedID() == Player.WEAPON_ID_MINING_LASER)
        {
            //Sprite
            weaponSelectedIcon.sprite = weaponSelectedIconLaser;
        }
        else if (playerScript.GetWeaponSelectedID() == Player.WEAPON_ID_SEISMIC_CHARGES)
        {
            //Sprite
            weaponSelectedIcon.sprite = weaponSelectedIconSeismicCharge;
        }
        else
        {
            //None OR unknown weapon
            //Sprite
            weaponSelectedIcon.sprite = weaponSelectedIconNone;
        }
    }

    private void UpdateUIBindDisplays()
    {
        //Necessary to update the elements to reflect the player's settings on startup, otherwise they may be wrong if their settings are non-default

        //UpdateWeapons(); //Can't update until player has spawned
        control.ui.abilityEclipseVision.Find("Keybind").GetComponent<TextMeshProUGUI>().text = control.binds.GetBindAsPrettyString(control.binds.bindToggleOutline, true);
        control.ui.abilitySpotlight.Find("Keybind").GetComponent<TextMeshProUGUI>().text = control.binds.GetBindAsPrettyString(control.binds.bindToggleSpotlight, true);
        control.menu.MenuKeybindsUpdateBindText();
    }
    #endregion
}
