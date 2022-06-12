using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class Menu : MonoBehaviour
{
    public static bool menuOpenAndGamePaused = false;

    public GameObject menuContainer;
    public GameObject menuMain;
    public GameObject menuRestartConfirm;

    public GameObject menuSettings;
    public TMP_InputField menuSettingsMouseSensitivityIn;
    public TMP_InputField menuSettingsHFieldOfViewIn;
    public TMP_InputField menuSettingsCameraDistanceIn;
    public TMP_InputField menuSettingsCameraHeightIn;
    public TMP_InputField menuSettingsTargetFPSIn;
    public TMP_InputField menuSettingsAsteroidsMinIn;
    public TMP_InputField menuSettingsAsteroidsMaxIn;
    public Toggle menuSettingsToggleDisplayHUD;
    public Toggle menuSettingsToggleDisplayFPS;
    public Toggle menuSettingsToggleSpotlight;
    public Toggle menuSettingsToggleRefine;
    public Toggle menuSettingsToggleMusic;
    public Toggle menuSettingsToggleTips;
    public Toggle menuSettingsToggleFullscreen;
    public Toggle menuSettingsToggleMatchVelocity;
    public Toggle menuSettingsToggleSpinStabilizers;

    public GameObject menuKeybinds;
    private bool menuKeybindsIsSettingBind = false;
    public short menuKeybindsBindID = 0;

    private short BIND_ID_THRUST_FORWARD = 0;
    private short BIND_ID_THRUST_LEFT = 1;
    private short BIND_ID_THRUST_BACKWARD = 2;
    private short BIND_ID_THRUST_RIGHT = 3;
    private short BIND_ID_THRUST_UP = 4;
    private short BIND_ID_THRUST_DOWN = 5;
    private short BIND_ID_ALIGN_SHIP = 6;
    private short BIND_ID_CHEAT1 = 7;
    private short BIND_ID_CHEAT2 = 8;
    private short BIND_ID_PAN_MAP = 9;
    private short BIND_ID_FREE_LOOK = 10;
    private short BIND_ID_ZOOM_IN = 11;
    private short BIND_ID_ZOOM_OUT = 12;
    private short BIND_ID_SET_TARGET = 13;
    private short BIND_ID_FIRE = 14;
    private short BIND_ID_RELOAD = 15;
    private short BIND_ID_SPOTLIGHT = 16;
    private short BIND_ID_MAP = 17;
    private short BIND_ID_REFINE = 18;
    private short BIND_ID_WEAPON1 = 19;
    private short BIND_ID_WEAPON2 = 20;
    private short BIND_ID_HUD = 21;
    private short BIND_ID_FPS = 22;
    private short BIND_ID_SCREENSHOT = 23;
    private short BIND_ID_MENU = 24;

    public TMP_Text menuBindsThrustForward;
    public TMP_Text menuBindsThrustLeft;
    public TMP_Text menuBindsThrustBackward;
    public TMP_Text menuBindsThrustRight;
    public TMP_Text menuBindsThrustUp;
    public TMP_Text menuBindsThrustDown;
    public TMP_Text menuBindsAlignShipToReticle;
    public TMP_Text menuBindsCheat1;
    public TMP_Text menuBindsCheat2;
    public TMP_Text menuBindsCycleMovementMode;
    public TMP_Text menuBindsCameraFreeLook;
    public TMP_Text menuBindsCameraZoomIn;
    public TMP_Text menuBindsCameraZoomOut;
    public TMP_Text menuBindsSetTarget;
    public TMP_Text menuBindsPrimaryFire;
    public TMP_Text menuBindsPrimaryReload;
    public TMP_Text menuBindsToggleSpotlight;
    public TMP_Text menuBindsToggleMap;
    public TMP_Text menuBindsToggleRefine;
    public TMP_Text menuBindsSelectWeapon1;
    public TMP_Text menuBindsSelectWeapon2;
    public TMP_Text menuBindsToggleHUD;
    public TMP_Text menuBindsToggleFPS;
    public TMP_Text menuBindsSaveScreenshot;
    public TMP_Text menuBindsToggleMenu;

    [SerializeField] private Control control;

    private void Start()
    {
        menuContainer.SetActive(false);
        menuMain.SetActive(false);
        menuSettings.SetActive(false);
        menuKeybinds.SetActive(false);
        menuRestartConfirm.SetActive(false);
    }

    private void Update()
    {
        //KEYBINDS
        MenuKeybindsCheckKeybindAssignment();
    }

    public void DisableAllSubMenus()
    {
        menuKeybinds.SetActive(false);
        menuSettings.SetActive(false);
        menuRestartConfirm.SetActive(false);
    }

    //MAIN
    #region Main Menu
    public void MenuToggle()
    {
        //Toggle menu
        menuOpenAndGamePaused = !menuOpenAndGamePaused;

        //Toggle UI
        menuContainer.gameObject.SetActive(menuOpenAndGamePaused);
        menuMain.SetActive(menuOpenAndGamePaused);
        if (!menuOpenAndGamePaused)
        {
            DisableAllSubMenus();
        }

        //Toggle game pause
        Time.timeScale = System.Convert.ToByte(!menuOpenAndGamePaused);

        //Toggle cursor lock
        Cursor.lockState = menuOpenAndGamePaused ? CursorLockMode.None : CursorLockMode.Locked;

        //Toggle reticle
        control.ui.cameraReticle.SetActive(!menuOpenAndGamePaused);

        //Disable map screen
        if (UI.displayMap)
        {
            control.ui.ToggleMapView();
        }
    }

    public void MenuMainOpen()
    {
        //Disable all sub-menus
        DisableAllSubMenus();

        //Enable main menu
        menuMain.SetActive(true);
    }

    public void MenuRestart()
    {
        MenuRestartConfirmOpen();
    }

    public void MenuSaveAndQuit()
    {
        //Save
        control.generation.SaveGame();

        //Quit
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif

        Application.Quit();
    }
    #endregion

    //RESTART
    #region Restart
    public void MenuRestartConfirmOpen()
    {
        menuMain.SetActive(false);
        menuRestartConfirm.SetActive(true);
    }

    public void MenuRestartConfirmed()
    {
        control.generation.GenerateGame(control.generation.GENERATION_TYPE_RESTARTED_GAME);
        MenuToggle();
    }
    #endregion

    //SETTINGS
    #region Settings
    public void MenuSettingsOpen()
    {
        menuMain.SetActive(false);
        menuSettings.SetActive(true);

        //Display current settings
        //Strings
        menuSettingsMouseSensitivityIn.text = control.settings.mouseSensitivity.ToString();
        menuSettingsHFieldOfViewIn.text = control.settings.hFieldOfView.ToString();
        menuSettingsCameraDistanceIn.text = control.settings.cameraDistance.ToString();
        menuSettingsCameraHeightIn.text = control.settings.cameraHeight.ToString();
        menuSettingsTargetFPSIn.text = control.settings.targetFPS.ToString();
        menuSettingsAsteroidsMinIn.text = control.settings.asteroidsConcurrentMin.ToString();
        menuSettingsAsteroidsMaxIn.text = control.settings.asteroidsConcurrentMax.ToString();

        //Toggles
        //Changing isOn activates the method, so we need to run methods twice to cancel-out running them once
        if (menuSettingsToggleDisplayHUD.isOn       != control.settings.displayHUD)         { menuSettingsToggleDisplayHUD.isOn         = control.settings.displayHUD;      MenuSettingsHUDToggle();                }
        if (menuSettingsToggleDisplayFPS.isOn       != control.settings.displayFPS)         { menuSettingsToggleDisplayFPS.isOn         = control.settings.displayFPS;      MenuSettingsFPSToggle();                }
        if (menuSettingsToggleFullscreen.isOn       != control.settings.fullscreen)         { menuSettingsToggleFullscreen.isOn         = control.settings.fullscreen;      MenuSettingsFullscreenToggle();         }
        if (menuSettingsToggleMatchVelocity.isOn    != control.settings.matchVelocity)      { menuSettingsToggleMatchVelocity.isOn      = control.settings.matchVelocity;   MenuSettingsMatchVelocityToggle();      }
        if (menuSettingsToggleSpinStabilizers.isOn  != control.settings.spinStabilizers)    { menuSettingsToggleSpinStabilizers.isOn    = control.settings.spinStabilizers; MenuSettingsSpinStabilizersToggle();    }
        if (menuSettingsToggleSpotlight.isOn        != control.settings.spotlight)          { menuSettingsToggleSpotlight.isOn          = control.settings.spotlight;       MenuSettingsSpotlightToggle();          }
        if (menuSettingsToggleRefine.isOn           != control.settings.refine)             { menuSettingsToggleRefine.isOn             = control.settings.refine;          MenuSettingsRefineToggle();             }
        if (menuSettingsToggleMusic.isOn            != control.settings.music)              { menuSettingsToggleMusic.isOn              = control.settings.music;           MenuSettingsMusicToggle();              }
        if (menuSettingsToggleTips.isOn             != control.settings.tips)               { menuSettingsToggleTips.isOn               = control.settings.tips;            MenuSettingsTipsToggle();               }

        //menuSettingsToggleDisplayHUD.isOn = control.settings.displayHUD;
        //menuSettingsToggleDisplayFPS.isOn = control.settings.displayFPS;
        //menuSettingsToggleFullscreen.isOn = control.settings.fullscreen;
        //menuSettingsToggleMatchVelocity.isOn = control.settings.matchVelocity;
        //menuSettingsToggleSpinStabilizers.isOn = control.settings.spinStabilizers;
        //
        ////Changing isOn activates the method, so we need to run it twice to cancel-out running it once
        //if (menuSettingsToggleSpotlight.isOn != control.settings.spotlight)
        //{
        //    menuSettingsToggleSpotlight.isOn = control.settings.spotlight;
        //    MenuSettingsSpotlightToggle();
        //}
        //
        //menuSettingsToggleRefine.isOn = control.settings.refine;
        //menuSettingsToggleMusic.isOn = control.settings.music;
        //menuSettingsToggleTips.isOn = control.settings.tips;
    }

    public void MenuSettingsMouseSensitivitySet()
    {
        //Ensure input is a float
        if (float.TryParse(menuSettingsMouseSensitivityIn.text, out float inputField))
        {
            //Ensure input value is within specified parameters
            if (inputField < control.settings.MOUSE_SENSITIVITY_MIN)
            {
                inputField = control.settings.MOUSE_SENSITIVITY_MIN;
            }
            else if (inputField > control.settings.MOUSE_SENSITIVITY_MAX)
            {
                inputField = control.settings.MOUSE_SENSITIVITY_MAX;
            }

            //Set in settings and save
            control.settings.mouseSensitivity = inputField;
            control.settings.Save();

            //Update in menu
            menuSettingsMouseSensitivityIn.text = inputField.ToString();
        }
    }

    public void MenuSettingsHFieldOfViewSet()
    {
        //Ensure input is a float
        if (float.TryParse(menuSettingsHFieldOfViewIn.text, out float inputField))
        {
            //Ensure input value is within specified parameters
            if (inputField < control.settings.H_FIELD_OF_VIEW_MIN)
            {
                inputField = control.settings.H_FIELD_OF_VIEW_MIN;
            }
            else if (inputField > control.settings.H_FIELD_OF_VIEW_MAX)
            {
                inputField = control.settings.H_FIELD_OF_VIEW_MAX;
            }

            //Set in settings and save
            control.settings.hFieldOfView = inputField;
            control.settings.Save();

            //Update in menu
            menuSettingsHFieldOfViewIn.text = inputField.ToString();

            //Update in game
            control.generation.instancePlayer.GetComponentInChildren<Player>().SetCameraSettings();
        }
    }

    public void MenuSettingsCameraDistanceSet()
    {
        //Ensure input is a float
        if (float.TryParse(menuSettingsCameraDistanceIn.text, out float inputField))
        {
            //Ensure input value is within specified parameters
            if (inputField < control.settings.CAMERA_DISTANCE_MIN)
            {
                inputField = control.settings.CAMERA_DISTANCE_MIN;
            }
            else if (inputField > control.settings.CAMERA_DISTANCE_MAX)
            {
                inputField = control.settings.CAMERA_DISTANCE_MAX;
            }

            //Update in settings and save
            control.settings.cameraDistance = inputField;
            control.settings.Save();

            //Update in menu
            menuSettingsCameraDistanceIn.text = inputField.ToString();
        }
    }

    public void MenuSettingsCameraHeightSet()
    {
        //Ensure input is a float
        if (float.TryParse(menuSettingsCameraHeightIn.text, out float inputField))
        {
            //Ensure input value is within specified parameters
            if (inputField < control.settings.CAMERA_HEIGHT_MIN)
            {
                inputField = control.settings.CAMERA_HEIGHT_MIN;
            }
            else if (inputField > control.settings.CAMERA_HEIGHT_MAX)
            {
                inputField = control.settings.CAMERA_HEIGHT_MAX;
            }

            //Update in settings and save
            control.settings.cameraHeight = inputField;
            control.settings.Save();

            //Update in menu
            menuSettingsCameraHeightIn.text = inputField.ToString();
        }
    }

    public void MenuSettingsFPSToggle()
    {
        //Update in settings and save
        control.settings.displayFPS = !control.settings.displayFPS;
        control.settings.Save();

        //Update in game
        if (!control.settings.displayFPS)
        {
            control.ui.systemInfo.text = "";
        }
    }

    public void MenuSettingsFullscreenToggle()
    {
        //Update in settings and save
        control.settings.fullscreen = !control.settings.fullscreen;
        control.settings.Save();

        //Update in game
        Screen.fullScreen = control.settings.fullscreen;
    }

    public void MenuSettingsMatchVelocityToggle()
    {
        //Update in settings and save
        control.settings.matchVelocity = !control.settings.matchVelocity;
        control.settings.Save();
    }

    public void MenuSettingsSpinStabilizersToggle()
    {
        //Update in settings and save
        control.settings.spinStabilizers = !control.settings.spinStabilizers;
        control.settings.Save();
    }

    public void MenuSettingsHUDToggle()
    {
        //Update in settings and save
        control.settings.displayHUD = !control.settings.displayHUD;
        control.settings.Save();

        //Update in game
        control.ui.canvas.transform.Find("HUD Centre").gameObject.SetActive(control.settings.displayHUD);
        control.ui.canvas.transform.Find("HUD Top-Left").gameObject.SetActive(control.settings.displayHUD);
        control.ui.canvas.transform.Find("HUD Top").gameObject.SetActive(control.settings.displayHUD);
        control.ui.canvas.transform.Find("HUD Top-Right").gameObject.SetActive(control.settings.displayHUD);
        control.ui.canvas.transform.Find("HUD Bottom-Right").gameObject.SetActive(control.settings.displayHUD);
        control.ui.canvas.transform.Find("HUD Bottom-Left").gameObject.SetActive(control.settings.displayHUD);
    }

    public void MenuSettingsSpotlightToggle()
    {
        //Rectify toggle button being out-of-phase with actual setting boolean
        //(Counter-intuitive: in this case isOn has JUST been changed, calling this method, so if they are equal now they would have been inequal prior to calling the method)
        if (menuSettingsToggleSpotlight.isOn == control.settings.spotlight)
        {
            //Rectifying this also calls the method again, causing the spotlight to still be updated from just one button toggle
            menuSettingsToggleSpotlight.isOn = !menuSettingsToggleSpotlight.isOn;
        }
        else
        {
            //Toggle spotlight setting
            control.settings.spotlight = !control.settings.spotlight;
            control.settings.Save();
        }
        
        //Update spotlight gameObject
        control.generation.instancePlayer.GetComponentInChildren<Player>().DecideWhichModelsToRender();
    }

    public void MenuSettingsRefineToggle()
    {
        control.settings.refine = !control.settings.refine;
        control.settings.Save();
    }

    public void MenuSettingsTargetFPSSet()
    {
        //Ensure input is a float
        if (int.TryParse(menuSettingsTargetFPSIn.text, out int inputField))
        {
            //Ensure input value is within specified parameters
            if (inputField < control.settings.TARGET_FPS_MIN)
            {
                inputField = control.settings.TARGET_FPS_MIN;
            }
            else if (inputField > control.settings.TARGET_FPS_MAX)
            {
                inputField = control.settings.TARGET_FPS_MAX;
            }

            //Update in settings and save
            control.settings.targetFPS = inputField;
            control.settings.Save();

            //Update in menu
            menuSettingsTargetFPSIn.text = inputField.ToString();
        }
    }

    public void MenuSettingsMusicToggle()
    {
        //Toggle music setting and save
        control.settings.music = !control.settings.music;
        control.settings.Save();

        //If disabling music and a song is currently playing, stop playing that song
        if (!control.settings.music && control.generation.instancePlayer.GetComponentInChildren<Player>().music.isPlaying)
        {
            control.generation.instancePlayer.GetComponentInChildren<Player>().music.Stop();
        }
    }

    public void MenuSettingsTipsToggle()
    {
        //Toggle music setting and save
        control.settings.tips = !control.settings.tips;
        control.settings.Save();

        control.ui.tipText.gameObject.SetActive(control.settings.tips);
    }

    public void MenuSettingsAsteroidsMinSet()
    {
        //Ensure input is a float
        if (int.TryParse(menuSettingsAsteroidsMinIn.text, out int inputField))
        {
            //Ensure input value is within specified parameters
            if (inputField < control.settings.ASTEROIDS_MIN_MIN)
            {
                inputField = control.settings.ASTEROIDS_MIN_MIN;
            }
            else if (inputField > control.settings.ASTEROIDS_MIN_MAX)
            {
                inputField = control.settings.ASTEROIDS_MIN_MAX;
            }

            //Update in settings and save
            control.settings.asteroidsConcurrentMin = inputField;
            control.settings.Save();

            //Update in menu
            menuSettingsAsteroidsMinIn.text = inputField.ToString();
        }
    }

    public void MenuSettingsAsteroidsMaxSet()
    {
        //Ensure input is a float
        if (int.TryParse(menuSettingsAsteroidsMaxIn.text, out int inputField))
        {
            //Ensure input value is within specified parameters
            if (inputField < control.settings.ASTEROIDS_MAX_MIN)
            {
                inputField = control.settings.ASTEROIDS_MAX_MIN;
            }
            else if (inputField > control.settings.ASTEROIDS_MAX_MAX)
            {
                inputField = control.settings.ASTEROIDS_MAX_MAX;
            }

            //Update in settings and save
            control.settings.asteroidsConcurrentMax = inputField;
            control.settings.Save();

            //Update in menu
            menuSettingsAsteroidsMaxIn.text = inputField.ToString();
        }
    }
    #endregion

    //KEYBINDS
    #region Keybinds
    public void MenuKeybindsCheckKeybindAssignment()
    {
        //Change keybind
        //Get whatever key was pressed (note: this will have to be expanded to fit mouse and joystick inputs)
        //We check if any key is pressed first to save performance, as foreach will loop through a lot of codes here
        //Thanks roojerry from the Unity forum
        if (menuKeybindsIsSettingBind)
        {
            if (Input.anyKey || control.binds.GetInput(control.binds.MOUSE_SCROLL_UP) || control.binds.GetInput(control.binds.MOUSE_SCROLL_DOWN))
            {
                //Get the bind, if there is one
                bool isBind = false;
                short inputCode = 0;
                if (control.binds.GetInput(control.binds.MOUSE_SCROLL_UP))
                {
                    isBind = true;
                    inputCode = control.binds.MOUSE_SCROLL_UP;
                }
                else if (control.binds.GetInput(control.binds.MOUSE_SCROLL_DOWN))
                {
                    isBind = true;
                    inputCode = control.binds.MOUSE_SCROLL_DOWN;
                }
                else
                {
                    foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
                    {
                        if (Input.GetKey(keyCode))
                        {
                            //Debug.Log("KeyCode down: " + keyCode);

                            isBind = true;
                            inputCode = (short)keyCode;
                        }
                    }
                }

                //Assign the bind
                if (isBind)
                {
                    //Assign bind
                    if (menuKeybindsBindID == BIND_ID_THRUST_FORWARD) { control.binds.bindThrustForward = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_THRUST_LEFT) { control.binds.bindThrustLeft = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_THRUST_BACKWARD) { control.binds.bindThrustBackward = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_THRUST_RIGHT) { control.binds.bindThrustRight = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_THRUST_UP) { control.binds.bindThrustUp = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_THRUST_DOWN) { control.binds.bindThrustDown = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_ALIGN_SHIP) { control.binds.bindAlignShipToReticle = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_CHEAT1) { control.binds.bindCheat1 = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_CHEAT2) { control.binds.bindCheat2 = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_PAN_MAP) { control.binds.bindPanMap = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_FREE_LOOK) { control.binds.bindCameraFreeLook = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_ZOOM_IN) { control.binds.bindCameraZoomIn = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_ZOOM_OUT) { control.binds.bindCameraZoomOut = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_SET_TARGET) { control.binds.bindSetTarget = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_FIRE) { control.binds.bindPrimaryFire = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_RELOAD) { control.binds.bindPrimaryReload = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_SPOTLIGHT) { control.binds.bindToggleSpotlight = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_MAP) { control.binds.bindToggleMap = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_REFINE) { control.binds.bindToggleRefine = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_WEAPON1) { control.binds.bindSelectWeapon1 = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_WEAPON2) { control.binds.bindSelectWeapon2 = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_HUD) { control.binds.bindToggleHUD = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_FPS) { control.binds.bindToggleFPS = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_SCREENSHOT) { control.binds.bindSaveScreenshot = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_MENU) { control.binds.bindToggleMenu = inputCode; }

                    //Update menu text
                    MenuKeybindsUpdateBindText();

                    //Exit bind setting mode
                    menuKeybindsIsSettingBind = false;

                    //Save the keybind
                    control.binds.Save();
                }
                else
                {
                    Debug.Log("Error! No bind received even though an input was detected.");
                }
            }
        }
        else
        {
            //IN-GAME SETTINGS KEYBINDS
            //Menu toggle (includes cursor locking/unlocking)
            if (!Commerce.menuOpen && control.binds.GetInputDown(control.binds.bindToggleMenu))
            {
                //Toggle menu
                MenuToggle();
            }

            //HUD toggle
            if (control.binds.GetInputDown(control.binds.bindToggleHUD))
            {
                menuSettingsToggleDisplayHUD.isOn = !menuSettingsToggleDisplayHUD.isOn;

                //MenuSettingsHUDToggle();
            }

            //FPS toggle
            if (control.binds.GetInputDown(control.binds.bindToggleFPS))
            {
                menuSettingsToggleDisplayFPS.isOn = !menuSettingsToggleDisplayFPS.isOn;

                //MenuSettingsFPSToggle();
            }

            //Spotlight toggle
            if (control.binds.GetInputDown(control.binds.bindToggleSpotlight))
            {
                //Update the settings menu and toggle the actual spotlight
                //(Changing isOn also calls the method attached to that toggle button. In this case: MenuSettingsSpotlightToggle())
                menuSettingsToggleSpotlight.isOn = !menuSettingsToggleSpotlight.isOn;

                //Update spotlight gameObject
                control.generation.instancePlayer.GetComponentInChildren<Player>().DecideWhichModelsToRender();
            }

            //Refine toggle
            if (control.binds.GetInputDown(control.binds.bindToggleRefine))
            {
                menuSettingsToggleRefine.isOn = !menuSettingsToggleRefine.isOn;
            }
        }
    }

    public void MenuKeybindsOpen()
    {
        menuMain.SetActive(false);
        menuKeybinds.SetActive(true);

        MenuKeybindsUpdateBindText();
    }

    private void MenuKeybindsUpdateBindText() {
        menuBindsThrustForward.text         = MenuKeybindsGetBindString(control.binds.bindThrustForward);
        menuBindsThrustLeft.text            = MenuKeybindsGetBindString(control.binds.bindThrustLeft);
        menuBindsThrustBackward.text        = MenuKeybindsGetBindString(control.binds.bindThrustBackward);
        menuBindsThrustRight.text           = MenuKeybindsGetBindString(control.binds.bindThrustRight);
        menuBindsThrustUp.text              = MenuKeybindsGetBindString(control.binds.bindThrustUp);
        menuBindsThrustDown.text            = MenuKeybindsGetBindString(control.binds.bindThrustDown);
        menuBindsAlignShipToReticle.text    = MenuKeybindsGetBindString(control.binds.bindAlignShipToReticle);
        menuBindsCheat1.text                = MenuKeybindsGetBindString(control.binds.bindCheat1);
        menuBindsCheat2.text                = MenuKeybindsGetBindString(control.binds.bindCheat2);
        menuBindsCycleMovementMode.text     = MenuKeybindsGetBindString(control.binds.bindPanMap);
        menuBindsCameraFreeLook.text        = MenuKeybindsGetBindString(control.binds.bindCameraFreeLook);
        menuBindsCameraZoomIn.text          = MenuKeybindsGetBindString(control.binds.bindCameraZoomIn);
        menuBindsCameraZoomOut.text         = MenuKeybindsGetBindString(control.binds.bindCameraZoomOut);
        menuBindsSetTarget.text             = MenuKeybindsGetBindString(control.binds.bindSetTarget);
        menuBindsPrimaryFire.text           = MenuKeybindsGetBindString(control.binds.bindPrimaryFire);
        menuBindsPrimaryReload.text         = MenuKeybindsGetBindString(control.binds.bindPrimaryReload);
        menuBindsToggleSpotlight.text       = MenuKeybindsGetBindString(control.binds.bindToggleSpotlight);
        menuBindsToggleMap.text             = MenuKeybindsGetBindString(control.binds.bindToggleMap);
        menuBindsToggleRefine.text          = MenuKeybindsGetBindString(control.binds.bindToggleRefine);
        menuBindsSelectWeapon1.text         = MenuKeybindsGetBindString(control.binds.bindSelectWeapon1);
        menuBindsSelectWeapon2.text         = MenuKeybindsGetBindString(control.binds.bindSelectWeapon2);
        menuBindsToggleHUD.text             = MenuKeybindsGetBindString(control.binds.bindToggleHUD);
        menuBindsToggleFPS.text             = MenuKeybindsGetBindString(control.binds.bindToggleFPS);
        menuBindsSaveScreenshot.text        = MenuKeybindsGetBindString(control.binds.bindSaveScreenshot);
        menuBindsToggleMenu.text            = MenuKeybindsGetBindString(control.binds.bindToggleMenu);
    }

    private string MenuKeybindsGetBindString(short bind)
    {
        //Default to error
        string bindString = "Error";

        //0 to 509 = KeyCode
        //MOUSE_PRIMARY = 1000
        //MOUSE_SECONDARY = 1001
        //MOUSE_MIDDLE = 1002
        //MOUSE_SCROLL_UP = 1003
        //MOUSE_SCROLL_DOWN = 1004
        if (bind >= 1000 && bind <= 1004)
        {
            switch (bind)
            {
                case 1000:
                    bindString = "Left Mouse";
                    break;

                case 1001:
                    bindString = "Right Mouse";
                    break;

                case 1002:
                    bindString = "Middle Mouse";
                    break;

                case 1003:
                    bindString = "Scroll Up";
                    break;

                case 1004:
                    bindString = "Scroll Down";
                    break;
            }
        }
        else //if (bind <= 509)
        {
            bindString = ((KeyCode)bind).ToString();
        }

        return bindString;
    }

    public void MenuKeybindsThurstForwardSet()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsThrustForward.text = "";
        menuKeybindsBindID = BIND_ID_THRUST_FORWARD;
    }

    public void MenuKeybindsThrustLeftSet()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsThrustLeft.text = "";
        menuKeybindsBindID = BIND_ID_THRUST_LEFT;
    }

    public void MenuKeybindsThrustBackwardSet()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsThrustBackward.text = "";
        menuKeybindsBindID = BIND_ID_THRUST_BACKWARD;
    }

    public void MenuKeybindsThrustRightSet()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsThrustRight.text = "";
        menuKeybindsBindID = BIND_ID_THRUST_RIGHT;
    }

    public void MenuKeybindsThrustUpSet()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsThrustUp.text = "";
        menuKeybindsBindID = BIND_ID_THRUST_UP;
    }

    public void MenuKeybindsThrustDownSet()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsThrustDown.text = "";
        menuKeybindsBindID = BIND_ID_THRUST_DOWN;
    }

    public void MenuKeybindsAlignShipToReticleSet()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsAlignShipToReticle.text = "";
        menuKeybindsBindID = BIND_ID_ALIGN_SHIP;
    }

    public void MenuKeybindsCheat1Set()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsCheat1.text = "";
        menuKeybindsBindID = BIND_ID_CHEAT1;
    }

    public void MenuKeybindsCheat2Set()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsCheat2.text = "";
        menuKeybindsBindID = BIND_ID_CHEAT2;
    }

    public void MenuKeybindsCycleMovementModeSet()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsCycleMovementMode.text = "";
        menuKeybindsBindID = BIND_ID_PAN_MAP;
    }

    public void MenuKeybindsCameraFreeLookSet()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsCameraFreeLook.text = "";
        menuKeybindsBindID = BIND_ID_FREE_LOOK;
    }

    public void MenuKeybindsCameraZoomInSet()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsCameraZoomIn.text = "";
        menuKeybindsBindID = BIND_ID_ZOOM_IN;
    }

    public void MenuKeybindsCameraZoomOutSet()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsCameraZoomOut.text = "";
        menuKeybindsBindID = BIND_ID_ZOOM_OUT;
    }

    public void MenuKeybindsSetTargetSet()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsSetTarget.text = "";
        menuKeybindsBindID = BIND_ID_SET_TARGET;
    }

    public void MenuKeybindsPrimaryFireSet()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsPrimaryFire.text = "";
        menuKeybindsBindID = BIND_ID_FIRE;
    }

    public void MenuKeybindsPrimaryReloadSet()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsPrimaryReload.text = "";
        menuKeybindsBindID = BIND_ID_RELOAD;
    }

    public void MenuKeybindsToggleSpotlightSet()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsToggleSpotlight.text = "";
        menuKeybindsBindID = BIND_ID_SPOTLIGHT;
    }

    public void MenuKeybindsToggleMapSet()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsToggleMap.text = "";
        menuKeybindsBindID = BIND_ID_MAP;
    }

    public void MenuKeybindsToggleRefineSet()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsToggleRefine.text = "";
        menuKeybindsBindID = BIND_ID_REFINE;
    }

    public void MenuKeybindsSelectWeapon1Set()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsSelectWeapon1.text = "";
        menuKeybindsBindID = BIND_ID_WEAPON1;
    }

    public void MenuKeybindsSelectWeapon2Set()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsSelectWeapon2.text = "";
        menuKeybindsBindID = BIND_ID_WEAPON2;
    }

    public void MenuKeybindsToggleHUDSet()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsToggleHUD.text = "";
        menuKeybindsBindID = BIND_ID_HUD;
    }

    public void MenuKeybindsToggleFPSSet()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsToggleFPS.text = "";
        menuKeybindsBindID = BIND_ID_FPS;
    }

    public void MenuKeybindsSaveScreenshotSet()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsSaveScreenshot.text = "";
        menuKeybindsBindID = BIND_ID_SCREENSHOT;
    }

    public void MenuKeybindsToggleMenuSet()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsToggleMenu.text = "";
        menuKeybindsBindID = BIND_ID_MENU;
    }
    #endregion Keybinds
}