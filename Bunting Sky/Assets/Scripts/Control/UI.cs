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
    private float targetXMin;
    private float targetXMax;
    private float targetYMin;
    private float targetYMax;
    [System.NonSerialized] public bool renderTarget = false;
    private string targetTypeAndTitle = "No target";

    //Player resources
    [System.NonSerialized] public bool updatePlayerResourcesUIAnimations = true;
    [System.NonSerialized] public Image resourcesImageCurrency;
    [System.NonSerialized] public Image resourcesImagePlatinoid;
    [System.NonSerialized] public Image resourcesImagePreciousMetal;
    [System.NonSerialized] public Image resourcesImageWater;
    [System.NonSerialized] public TextMeshProUGUI resourcesTextCurrency;
    [System.NonSerialized] public TextMeshProUGUI resourcesTextPlatinoid;
    [System.NonSerialized] public TextMeshProUGUI resourcesTextPreciousMetal;
    [System.NonSerialized] public TextMeshProUGUI resourcesTextWater;

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

    private void Awake()
    {
        //Get references
        systemInfo = canvas.transform.Find("HUD Top-Right").Find("SystemInfo").GetComponent<TextMeshProUGUI>();
        cameraReticle = canvas.transform.Find("HUD Centre").Find("CameraReticle").gameObject;

        Transform waypointFolder = canvas.transform.Find("HUD Centre").Find("Waypoint");
        waypointImage = waypointFolder.Find("Waypoint").GetComponent<Image>();
        waypointTextType = waypointFolder.Find("Waypoint Type Text").GetComponent<TextMeshProUGUI>();
        waypointTextTitle = waypointFolder.Find("Waypoint Title Text").GetComponent<TextMeshProUGUI>();
        waypointTextBody = waypointFolder.Find("Waypoint Body Text").GetComponent<TextMeshProUGUI>();
        targetImage = waypointFolder.Find("Target").GetComponent<Image>();

        Transform resourcesFolder = canvas.transform.Find("HUD Top-Left").Find("Resources");
        resourcesImageCurrency = resourcesFolder.Find("Currency").GetComponent<Image>();
        resourcesImagePlatinoid = resourcesFolder.Find("Platinoid").GetComponent<Image>();
        resourcesImagePreciousMetal = resourcesFolder.Find("Precious Metals").GetComponent<Image>();
        resourcesImageWater = resourcesFolder.Find("Water").GetComponent<Image>();
        resourcesTextCurrency = resourcesFolder.Find("Currency Text").GetComponent<TextMeshProUGUI>();
        resourcesTextPlatinoid = resourcesFolder.Find("Platinoid Text").GetComponent<TextMeshProUGUI>();
        resourcesTextPreciousMetal = resourcesFolder.Find("Precious Metals Text").GetComponent<TextMeshProUGUI>();
        resourcesTextWater = resourcesFolder.Find("Water Text").GetComponent<TextMeshProUGUI>();

        Transform weaponsFolder = canvas.transform.Find("HUD Bottom-Right").Find("Weapons");
        weaponCooldown = weaponsFolder.Find("Cooldown").GetComponent<Image>();
        weaponSelectedClipRemainingText = weaponsFolder.Find("Selected Clip Remaining Text").GetComponent<TextMeshProUGUI>();
        weaponSelectedClipSizeText = weaponsFolder.Find("Selected Clip Size Text").GetComponent<TextMeshProUGUI>();
        weaponSelectedTitleText = weaponsFolder.Find("Selected Title Text").GetComponent<TextMeshProUGUI>();
        weaponAlternateTitleText = weaponsFolder.Find("Alternate Title Text").GetComponent<TextMeshProUGUI>();

        tipText = canvas.transform.Find("HUD Bottom").Find("Tips").Find("Tip Text").GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        //Waypoint
        waypointXMin = waypointImage.GetPixelAdjustedRect().width / 2;
        waypointXMax = Screen.width - waypointXMin;
        waypointYMin = waypointImage.GetPixelAdjustedRect().height / 2;
        waypointYMax = Screen.height - waypointYMin;

        //Target
        targetXMin = targetImage.GetPixelAdjustedRect().width / 2;
        targetXMax = Screen.width - targetXMin;
        targetYMin = targetImage.GetPixelAdjustedRect().height / 2;
        targetYMax = Screen.height - targetYMin;
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
            //AUTO TORQUING
            //Don't recommend auto torquing if firing when fuel is empty OR if player is auto torquing
            if (control.GetPlayerScript().vitalsFuel <= 0d || binds.GetInputDown(binds.bindAlignShipToReticle))
            {
                tipAimNeedsHelpCertainty = 0f;
            }

            if (tipAimNeedsHelpCertainty > TIP_AIM_THRESHOLD_CERTAINTY)
            {
                SetTip(
                    "Hold " + GetBindAsPrettyString(control.binds.bindAlignShipToReticle) + " to torque your ship in the direction you're looking!",
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

    public string GetBindAsPrettyString(short bind)
    {
        string pretty = "error";

        if (bind >= 1000 && bind <= 1004)
        {
            if (bind == control.binds.MOUSE_PRIMARY)            { pretty = "mouse primary"; }
            else if (bind == control.binds.MOUSE_SECONDARY)     { pretty = "mouse secondary"; }
            else if (bind == control.binds.MOUSE_MIDDLE)        { pretty = "mouse middle"; }
            else if (bind == control.binds.MOUSE_SCROLL_UP)     { pretty = "scroll up"; }
            else if (bind == control.binds.MOUSE_SCROLL_DOWN)   { pretty = "scroll down"; }
            else                                                { pretty = "unrecognized keycode"; }
        }
        else if (bind >= 48 && bind <= 57)
        {
            //Alpha-numerics, 0 through 9
            pretty = (bind - 48).ToString();
        }
        else
        {
            //Convert from our proprietary binds saving format of short back to KeyCode and read as string
            pretty = ((KeyCode)bind).ToString();

            //Add spaces in between capitals (useful for binds like "LeftShift")
            pretty = Control.InsertSpacesInFrontOfCapitals(pretty);

            //Make all lowercase
            pretty = pretty.ToLower();
        }

        //Surround with square brackets
        pretty = "[" + pretty + "]";

        return pretty;
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
        for (int heighlinerIndex = 0; heighlinerIndex < control.generation.heighlinerList.Count; heighlinerIndex++)
        {
            //Refs
            GameObject heighliner = control.generation.heighlinerList[heighlinerIndex];
            Transform heighlinerMapLineModel = heighliner.transform.Find("Jump Trigger Volume").Find("HeighlinerMapLineModel(Clone)");

            //Protect against null ref exception
            if (heighlinerMapLineModel != null)
            { 
                //Only display line after discovered
                if (heighliner.GetComponentInChildren<HeighlinerEntry>().isDiscovered)
                {
                    heighlinerMapLineModel.gameObject.SetActive(displayMap);
                }
            }
            else
            {
                Debug.Log("Heighliner has no map line model!");
            }
        }

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

        Vector3 waypointWorldPos = hit.collider.transform.position;

        Vector2 waypointUIPos = Camera.main.WorldToScreenPoint(waypointWorldPos);
        waypointUIPos.x = Mathf.Clamp(waypointUIPos.x, waypointXMin, waypointXMax);
        waypointUIPos.y = Mathf.Clamp(waypointUIPos.y, waypointYMin, waypointYMax);

        waypointImage.transform.position = waypointUIPos;

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

        waypointTextType.transform.position = new Vector2(waypointUIPos.x + WAYPOINT_X_OFFSET, waypointUIPos.y + WAYPOINT_Y_OFFSET + waypointTextBody.fontSize + waypointTextTitle.fontSize + waypointTextType.fontSize);
        waypointTextTitle.transform.position = new Vector2(waypointUIPos.x + WAYPOINT_X_OFFSET, waypointUIPos.y + WAYPOINT_Y_OFFSET + waypointTextBody.fontSize + waypointTextTitle.fontSize);
        waypointTextBody.transform.position = new Vector2(waypointUIPos.x + WAYPOINT_X_OFFSET, waypointUIPos.y + WAYPOINT_Y_OFFSET + waypointTextBody.fontSize);

        //Set as target too if LMB
        if (binds.GetInputDown(binds.bindSetTarget))
        {
            //Target
            SetPlayerTargetObject(hit.collider.transform.gameObject);

            //Console
            UpdateTargetConsole();
        }
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
                && targetObj.GetComponent<Enemy>().destroying
            )
            || (
                targetObj.name == control.generation.enemy.name + "(Clone)"
                && targetObj.GetComponent<Enemy>().destroyed
            )
        )
        {
            targetImage.gameObject.SetActive(false);
            renderTarget = false;
            return;
        }
        
        Vector3 targetWorldPos = control.generation.instancePlayer.GetComponentInChildren<Player>().targetObject.transform.position;

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

        targetImage.transform.position = targetUIPos;
    }

    public void SetPlayerTargetObject(GameObject objectToTarget)
    {
        //Set or toggle the target object

        //If the target hasn't been set to anything, we don't try to check if what's selected is currently the target since we already know the result and it would throw an error
        //If the player clicks on what is currently the target, it unsets it from being the target
        //If the player clicks on what is NOT already the target, we set that as the target

        if (control.generation.instancePlayer.GetComponentInChildren<Player>().targetObject == null)
        {
            control.generation.instancePlayer.GetComponentInChildren<Player>().targetObject = objectToTarget;

            if (!renderTarget)
            {
                targetImage.gameObject.SetActive(true);
                renderTarget = true;
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

        if (Physics.Raycast(waypointRaycastOrigin, waypointRaycastDirection, maxDist))
        {
            RaycastHit hit = new RaycastHit();
            Physics.Raycast(waypointRaycastOrigin, waypointRaycastDirection, out hit, maxDist);
            //Debug.DrawRay(waypointRaycastOrigin, waypointRaycastDirection * hit.distance, Color.green, Time.deltaTime, false);

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
                waypointTextTitle.text = hit.collider.gameObject.GetComponent<NameCelestial>().title;
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
                Transform heighlinerTransform = hit.collider.transform;
                Transform moonTransform = heighlinerTransform.parent.GetChild(heighlinerTransform.GetSiblingIndex() - 1);
                string moonName = moonTransform.GetComponent<NameCelestial>().title;
                string finalTitle = "Interplanetary. Orbiting " + moonName;
                if (heighlinerTransform.GetComponentInChildren<HeighlinerEntry>().isDiscovered)
                {
                    Transform exitNodeHeighliner = heighlinerTransform.GetComponentInChildren<HeighlinerEntry>().exitNode.transform;
                    Transform exitNodeHeighlinerMoon = exitNodeHeighliner.parent.GetChild(exitNodeHeighliner.GetSiblingIndex() - 1);
                    string exitNodeHeighlinerMoonTitle = exitNodeHeighlinerMoon.GetComponent<NameCelestial>().title;
                    finalTitle = "Interplanetary. " + moonName + " to " + exitNodeHeighlinerMoonTitle;
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
                int size = hit.collider.gameObject.GetComponent<Asteroid>().size;
                if (size == Asteroid.SIZE_SMALL)
                {
                    waypointTextTitle.text = "Small";
                }
                else if (size == Asteroid.SIZE_MEDIUM)
                {
                    waypointTextTitle.text = "Medium";
                }
                else if (size == Asteroid.SIZE_LARGE)
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
                waypointTextType.text = "Bandit";
                int strength = hit.collider.gameObject.GetComponent<Enemy>().strength;
                if (strength == Enemy.STRENGTH_MINOR)
                {
                    waypointTextTitle.text = "Minor";
                }
                else if (strength == Enemy.STRENGTH_MAJOR)
                {
                    waypointTextTitle.text = "Major";
                }
                else if (strength == Enemy.STRENGTH_ELITE)
                {
                    waypointTextTitle.text = "Elite";
                }

                waypointTextBody.text = GetDistanceAndDeltaVUI(hit.collider.gameObject, false);

                //Console waypoint
                consoleTargetTypeAndTitleText.text = waypointTextType.text + "\n" + waypointTextTitle.text;

                //Update UI
                SetWaypointUI(hit);
            }
            else
            {
                //Debug.Log("Undefined object " + hit.collider.gameObject.name + " hit " + hit.distance + " units away");
            }
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

        //Don't render when in first-person
        //if (!control.GetPlayerScript().isDestroyed && Player.firstPerson)
        //{
        //    renderWaypoint = false;
        //    waypointImage.gameObject.SetActive(true);
        //}
        //else
        //{
        //    waypointImage.gameObject.SetActive(renderWaypoint);
        //}
        waypointImage.gameObject.SetActive(renderWaypoint);
        waypointTextType.gameObject.SetActive(renderWaypoint);
        waypointTextTitle.gameObject.SetActive(renderWaypoint);
        waypointTextBody.gameObject.SetActive(renderWaypoint);

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
        resourcesTextWater.text = control.GetPlayerScript().ore[Asteroid.TYPE_WATER].ToString("F2") + " kg";
    }

    public void UpdateAllPlayerResourcesUI()
    {
        Player playerScript = control.GetPlayerScript();

        //Update values and start animations on a resource if its value changed
        UpdatePlayerResourceUI(ref resourcesTextCurrency,       ref resourcesImageCurrency,         playerScript.currency.ToString("F0") + " ICC",                           playerScript.soundSourceCoins);
        UpdatePlayerResourceUI(ref resourcesTextPlatinoid,      ref resourcesImagePlatinoid,        playerScript.ore[Asteroid.TYPE_PLATINOID].ToString("F0") + " kg",        playerScript.soundSourceOreCollected);
        UpdatePlayerResourceUI(ref resourcesTextPreciousMetal,  ref resourcesImagePreciousMetal,    playerScript.ore[Asteroid.TYPE_PRECIOUS_METAL].ToString("F0") + " kg",   playerScript.soundSourceOreCollected);
        UpdatePlayerResourceUI(ref resourcesTextWater,          ref resourcesImageWater,            playerScript.ore[Asteroid.TYPE_WATER].ToString("F0") + " kg",            playerScript.soundSourceOreCollected);

        //Update console
        UpdatePlayerConsole();

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

        //Update colour relative to limit
        //Currency
        if (textMeshCurrent == resourcesTextCurrency)
        {
            float resourceValueUnitInterval = (float)control.GetPlayerScript().currency / Mathf.Max(control.commerce.PRICE_REFUEL, control.commerce.PRICE_REPAIR);
            if (resourceValueUnitInterval >= 3f)
            {
                textMeshCurrent.color = COLOR_UI_TRANSLUCENT_WHITE;
            }
            else if (resourceValueUnitInterval >= 1f)
            {
                textMeshCurrent.color = COLOR_UI_TRANSLUCENT_AMBER;
            }
            else
            {
                textMeshCurrent.color = COLOR_UI_TRANSLUCENT_RED;
            }
        }
        //Ore
        if (textMeshCurrent == resourcesTextPlatinoid || textMeshCurrent == resourcesTextPreciousMetal || textMeshCurrent == resourcesTextWater)
        {
            float resourceValueUnitInterval = (float)(control.GetPlayerScript().GetTotalOre() / control.GetPlayerScript().oreMax);
            if (resourceValueUnitInterval < 0.5f)
            {
                textMeshCurrent.color = COLOR_UI_TRANSLUCENT_WHITE;
            }
            else if (resourceValueUnitInterval < 0.75f)
            {
                textMeshCurrent.color = COLOR_UI_TRANSLUCENT_AMBER;
            }
            else
            {
                textMeshCurrent.color = COLOR_UI_TRANSLUCENT_RED;
            }
        }
    }

    public void UpdateAllPlayerResourcesUIAnimations()
    {
        //Default looping to disabled. If there are any changes the animations make, they will turn the looping back on
        updatePlayerResourcesUIAnimations = false;

        //Animate
        UpdatePlayerResourcesUIAnimation(ref resourcesImageCurrency);
        UpdatePlayerResourcesUIAnimation(ref resourcesImagePlatinoid);
        UpdatePlayerResourcesUIAnimation(ref resourcesImagePreciousMetal);
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
            + "\n" + "Precious metal: " + resourcesTextPreciousMetal.text
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
            weaponSelectedTitleText.text = playerScript.weaponSlot0.NAME + " " + control.ui.GetBindAsPrettyString(binds.bindSelectWeaponSlot0);
            weaponAlternateTitleText.text = playerScript.weaponSlot1.NAME + " " + control.ui.GetBindAsPrettyString(binds.bindSelectWeaponSlot1);
        }
        else
        {
            //SELECTING SLOT 1
            weaponSelectedTitleText.text = playerScript.weaponSlot1.NAME + " " + control.ui.GetBindAsPrettyString(binds.bindSelectWeaponSlot1);
            weaponAlternateTitleText.text = playerScript.weaponSlot0.NAME + " " + control.ui.GetBindAsPrettyString(binds.bindSelectWeaponSlot0);
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
    #endregion
}
