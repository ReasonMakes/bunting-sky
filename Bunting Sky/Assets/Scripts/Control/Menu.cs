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
    public Toggle menuSettingsToggleTutorial;
    public Toggle menuSettingsToggleFullscreen;
    public Toggle menuSettingsToggleMatchVelocity;
    public Toggle menuSettingsToggleSpinStabilizers;
    public Slider sliderVolumeAll;
    public Slider sliderVolumeMusic;

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
    private short BIND_ID_ZOOM_OPTICAL = 13;
    private short BIND_ID_SET_TARGET = 14;
    private short BIND_ID_FIRE = 15;
    private short BIND_ID_RELOAD = 16;
    private short BIND_ID_SPOTLIGHT = 17;
    private short BIND_ID_OUTLINE = 18;
    private short BIND_ID_MAP = 19;
    private short BIND_ID_REFINE = 20;
    private short BIND_ID_WEAPON1 = 21;
    private short BIND_ID_WEAPON2 = 22;
    private short BIND_ID_HUD = 23;
    private short BIND_ID_FPS = 24;
    private short BIND_ID_SCREENSHOT = 25;
    private short BIND_ID_MENU = 26;

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
    public TMP_Text menuBindsCameraZoomOptical;
    public TMP_Text menuBindsSetTarget;
    public TMP_Text menuBindsPrimaryFire;
    public TMP_Text menuBindsPrimaryReload;
    public TMP_Text menuBindsToggleSpotlight;
    public TMP_Text menuBindsToggleOutline;
    public TMP_Text menuBindsToggleMap;
    public TMP_Text menuBindsToggleRefine;
    public TMP_Text menuBindsSelectWeapon1;
    public TMP_Text menuBindsSelectWeapon2;
    public TMP_Text menuBindsToggleHUD;
    public TMP_Text menuBindsToggleFPS;
    public TMP_Text menuBindsSaveScreenshot;
    public TMP_Text menuBindsToggleMenu;

    public TMP_Text menuSubIsPaused;
    public TMP_Text menuButtonResumeLabel;

    [SerializeField] private Control control;

    private void Start()
    {
        menuContainer.SetActive(false);
        menuMain.SetActive(false);
        menuSettings.SetActive(false);
        menuKeybinds.SetActive(false);
        menuRestartConfirm.SetActive(false);

        if (control.IS_EDITOR)
        {
            menuSubIsPaused.text = "(Developer Mode)";
        }
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

        //Update tip text
        if (menuOpenAndGamePaused)
        {
            //Avoid being obstructed by the menu
            control.ui.tipText.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 115f, 0f);
        }
        else
        {
            //Reset tip text position
            control.GetPlayerScript().DecideWhichModelsToRender();
        }

        //Toggle game pause
        Time.timeScale = System.Convert.ToByte(!menuOpenAndGamePaused);

        //Toggle cursor lock
        Cursor.lockState = menuOpenAndGamePaused ? CursorLockMode.None : CursorLockMode.Locked;

        //Toggle reticle
        //control.ui.cameraReticle.SetActive(!menuOpenAndGamePaused);

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
        //control.generation.SaveGame();

        //Quit
        Application.Quit();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
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
        control.generation.GenerateGame(Generation.GenerationType.restarted);
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
        if (menuSettingsToggleTutorial.isOn         != control.settings.tutorial)           { menuSettingsToggleTutorial.isOn           = control.settings.tutorial;        MenuSettingsTutorialToggle();           }
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
            control.generation.instancePlayer.GetComponentInChildren<Player>().UpdateCameraSettings();
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

    //public void MenuSettingsOutlineToggle()
    //{
    //    //Can only turn on when cooldown allows it, but can turn off at any time
    //    if (control.GetPlayerScript().outlineCanUse || control.GetPlayerScript().isOutlinesVisible)
    //    {
    //        //Update cooldowns
    //        if (control.GetPlayerScript().outlineCanUse && !control.GetPlayerScript().isOutlinesVisible)
    //        {
    //            control.GetPlayerScript().outlineCanUse = false;
    //
    //            //Time at which the outlines themselves become disabled
    //            control.GetPlayerScript().outlineDisableTime = Time.time + control.GetPlayerScript().OUTLINE_PERIOD_ENABLED;
    //
    //            //Time at which the player can use outlines again
    //            control.GetPlayerScript().outlineCanUseAgainTime = Time.time
    //                + control.GetPlayerScript().OUTLINE_PERIOD_ENABLED       //period enabled for
    //                + control.GetPlayerScript().OUTLINE_PERIOD_FADING        //period animating from enabled to disabled for
    //                + control.GetPlayerScript().OUTLINE_PERIOD_COOLDOWN;     //period disabled for
    //        }
    //        
    //        //Rectify toggle button being out-of-phase with actual setting boolean
    //        //(Counter-intuitive: in this case isOn has JUST been changed, calling this method, so if they are equal now they would have been inequal prior to calling the method)
    //        if (menuSettingsToggleOutline.isOn == control.settings.outline)
    //        {
    //            //Rectifying this also calls the method again, causing the outline to still be updated from just one button toggle
    //            menuSettingsToggleOutline.isOn = !menuSettingsToggleOutline.isOn;
    //        }
    //        else
    //        {
    //            //Toggle outline setting
    //            control.settings.outline = !control.settings.outline;
    //            control.settings.Save();
    //        }
    //
    //        //Update outlines
    //        control.GetPlayerScript().ToggleOutline();
    //    }
    //}

    public void MenuSettingsRefineToggle()
    {
        control.settings.refine = !control.settings.refine;

        if (!menuOpenAndGamePaused)
        {
            SetTipRefineState();
        }

        control.settings.Save();
    }

    public void SetTipRefineState()
    {
        if (control.settings.refine)
        {
            if (control.GetPlayerScript().ore[(int)Asteroid.Type.water] >= control.GetPlayerScript().REFINERY_FUEL_OUT_RATE)
            {
                control.ui.SetTip("In-situ fuel refinery activated");
            }
            else
            {
                control.ui.SetTip("In-situ fuel refinery activated\nNot enough water ice in cargo bay to output fuel");
            }
        }
        else
        {
            control.ui.SetTip("In-situ fuel refinery deactivated");
        }
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

    public void MenuSettingsVolumeAllUpdate()
    {
        control.settings.volumeAll = sliderVolumeAll.value;
        control.settings.Save();

        //rocket volume is procedural
        control.GetPlayerScript().soundSourceLaser0.volume = control.GetPlayerScript().SOUND_LASER_FIRE_VOLUME * control.settings.volumeAll;
        control.GetPlayerScript().soundSourceLaser1.volume = control.GetPlayerScript().SOUND_LASER_FIRE_VOLUME * control.settings.volumeAll;
        control.GetPlayerScript().soundSourceLaser2.volume = control.GetPlayerScript().SOUND_LASER_FIRE_VOLUME * control.settings.volumeAll;
        control.GetPlayerScript().soundSourceLaser3.volume = control.GetPlayerScript().SOUND_LASER_FIRE_VOLUME * control.settings.volumeAll;
        control.GetPlayerScript().soundSourceLaserReload.volume = control.GetPlayerScript().SOUND_LASER_RELOAD_VOLUME * control.settings.volumeAll;
        control.GetPlayerScript().soundSourceSeismicCharge0.volume = control.GetPlayerScript().SOUND_SEISMIC_CHARGE_FIRE_VOLUME * control.settings.volumeAll;
        control.GetPlayerScript().soundSourceSeismicCharge1.volume = control.GetPlayerScript().SOUND_SEISMIC_CHARGE_FIRE_VOLUME * control.settings.volumeAll;
        control.GetPlayerScript().soundSourceSeismicChargeExplosion.volume = control.GetPlayerScript().SOUND_SEISMIC_CHARGE_EXPLOSION_VOLUME * control.settings.volumeAll;
        control.GetPlayerScript().soundSourceOreCollected.volume = control.GetPlayerScript().SOUND_ORE_COLLECTED_VOLUME * control.settings.volumeAll;
        control.GetPlayerScript().soundSourceCurrencyChange.volume = control.GetPlayerScript().SOUND_CURRENCY_CHANGE_VOLUME * control.settings.volumeAll;
        //collision/impact volume is procedural

        //Asteroids
        for (int i = 0; i < control.generation.asteroidsEnabled.transform.childCount; i++)
        {
            Asteroid instanceAsteroid = control.generation.asteroidsEnabled.transform.GetChild(i).GetComponent<Asteroid>();

            instanceAsteroid.soundExplosion.volume = instanceAsteroid.SOUND_EXPLOSION_VOLUME * control.settings.volumeAll;
            instanceAsteroid.soundHit.volume = instanceAsteroid.SOUND_HIT_VOLUME * control.settings.volumeAll;
        }
        for (int i = 0; i < control.generation.asteroidsDisabled.transform.childCount; i++)
        {
            Asteroid instanceAsteroid = control.generation.asteroidsDisabled.transform.GetChild(i).GetComponent<Asteroid>();

            instanceAsteroid.soundExplosion.volume = instanceAsteroid.SOUND_EXPLOSION_VOLUME * control.settings.volumeAll;
            instanceAsteroid.soundHit.volume = instanceAsteroid.SOUND_HIT_VOLUME * control.settings.volumeAll;
        }

        //Bandits
        for (int i = 0; i < control.generation.enemies.transform.childCount; i++)
        {
            Enemy instanceEnemy = control.generation.enemies.transform.GetChild(i).GetComponent<Enemy>();

            instanceEnemy.soundSourceExplosion.volume = instanceEnemy.SOUND_EXPLOSION_VOLUME * control.settings.volumeAll;
        }
    }

    public void MenuSettingsVolumeMusicUpdate()
    {
        control.settings.volumeMusic = sliderVolumeMusic.value;
        control.settings.Save();

        //Update current song
        control.GetPlayerScript().music.volume = control.settings.volumeAll * control.settings.volumeMusic * control.GetPlayerScript().SOUND_MUSIC_VOLUME;
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

    public void MenuSettingsTutorialToggle()
    {
        //Toggle music setting and save
        control.settings.tutorial = !control.settings.tutorial;
        control.settings.Save();
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
            else if (inputField > control.settings.asteroidsConcurrentMax) //min can't be more than max
            {
                inputField = control.settings.asteroidsConcurrentMax;
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
            else if (inputField < control.settings.asteroidsConcurrentMin) //max can't be less than min
            {
                inputField = control.settings.asteroidsConcurrentMin;
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
            control.ui.SetTip(
                "Press the key you want to bind that input to, then primary click anywhere outside of the button to bind it",
                0f
            );

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
                    if (menuKeybindsBindID == BIND_ID_ZOOM_IN) { control.binds.bindCameraZoomFollowDistIn = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_ZOOM_OUT) { control.binds.bindCameraZoomFollowDistOut = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_ZOOM_OPTICAL) { control.binds.bindCameraZoomFOV = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_SET_TARGET) { control.binds.bindSetTarget = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_FIRE) { control.binds.bindPrimaryFire = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_RELOAD) { control.binds.bindPrimaryReload = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_SPOTLIGHT) { control.binds.bindToggleSpotlight = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_OUTLINE) { control.binds.bindToggleOutline = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_MAP) { control.binds.bindToggleMap = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_REFINE) { control.binds.bindToggleRefine = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_WEAPON1) { control.binds.bindSelectWeaponSlot0 = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_WEAPON2) { control.binds.bindSelectWeaponSlot1 = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_HUD) { control.binds.bindToggleHUD = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_FPS) { control.binds.bindToggleFPS = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_SCREENSHOT) { control.binds.bindSaveScreenshot = inputCode; }
                    if (menuKeybindsBindID == BIND_ID_MENU) { control.binds.bindToggleMenu = inputCode; }

                    //Save the keybind
                    control.binds.Save();

                    //Update menu text
                    MenuKeybindsUpdateBindText();

                    //Exit bind setting mode
                    menuKeybindsIsSettingBind = false;
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

            ////Outline toggle
            //if (control.binds.GetInputDown(control.binds.bindToggleOutline))
            //{
            //    //Update the settings menu and toggle the actual outline
            //    //(Changing isOn also calls the method attached to that toggle button.)
            //    menuSettingsToggleOutline.isOn = !menuSettingsToggleOutline.isOn;
            //
            //    //Update outline in code
            //    //control.generation.instancePlayer.GetComponentInChildren<Player>().ToggleOutline();
            //}

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

    public void MenuKeybindsUpdateBindText() {
        menuBindsThrustForward.text         = control.binds.GetBindAsPrettyString(control.binds.bindThrustForward, false);
        menuBindsThrustLeft.text            = control.binds.GetBindAsPrettyString(control.binds.bindThrustLeft, false);
        menuBindsThrustBackward.text        = control.binds.GetBindAsPrettyString(control.binds.bindThrustBackward, false);
        menuBindsThrustRight.text           = control.binds.GetBindAsPrettyString(control.binds.bindThrustRight, false);
        menuBindsThrustUp.text              = control.binds.GetBindAsPrettyString(control.binds.bindThrustUp, false);
        menuBindsThrustDown.text            = control.binds.GetBindAsPrettyString(control.binds.bindThrustDown, false);
        menuBindsAlignShipToReticle.text    = control.binds.GetBindAsPrettyString(control.binds.bindAlignShipToReticle, false);
        menuBindsCheat1.text                = control.binds.GetBindAsPrettyString(control.binds.bindCheat1, false);
        menuBindsCheat2.text                = control.binds.GetBindAsPrettyString(control.binds.bindCheat2, false);
        menuBindsCycleMovementMode.text     = control.binds.GetBindAsPrettyString(control.binds.bindPanMap, false);
        menuBindsCameraFreeLook.text        = control.binds.GetBindAsPrettyString(control.binds.bindCameraFreeLook, false);
        menuBindsCameraZoomIn.text          = control.binds.GetBindAsPrettyString(control.binds.bindCameraZoomFollowDistIn, false);
        menuBindsCameraZoomOut.text         = control.binds.GetBindAsPrettyString(control.binds.bindCameraZoomFollowDistOut, false);
        menuBindsCameraZoomOptical.text     = control.binds.GetBindAsPrettyString(control.binds.bindCameraZoomFOV, false);
        menuBindsSetTarget.text             = control.binds.GetBindAsPrettyString(control.binds.bindSetTarget, false);
        menuBindsPrimaryFire.text           = control.binds.GetBindAsPrettyString(control.binds.bindPrimaryFire, false);
        menuBindsPrimaryReload.text         = control.binds.GetBindAsPrettyString(control.binds.bindPrimaryReload, false);
        menuBindsToggleSpotlight.text       = control.binds.GetBindAsPrettyString(control.binds.bindToggleSpotlight, false);
        menuBindsToggleOutline.text         = control.binds.GetBindAsPrettyString(control.binds.bindToggleOutline, false);
        menuBindsToggleMap.text             = control.binds.GetBindAsPrettyString(control.binds.bindToggleMap, false);
        menuBindsToggleRefine.text          = control.binds.GetBindAsPrettyString(control.binds.bindToggleRefine, false);
        menuBindsSelectWeapon1.text         = control.binds.GetBindAsPrettyString(control.binds.bindSelectWeaponSlot0, false);
        menuBindsSelectWeapon2.text         = control.binds.GetBindAsPrettyString(control.binds.bindSelectWeaponSlot1, false);
        menuBindsToggleHUD.text             = control.binds.GetBindAsPrettyString(control.binds.bindToggleHUD, false);
        menuBindsToggleFPS.text             = control.binds.GetBindAsPrettyString(control.binds.bindToggleFPS, false);
        menuBindsSaveScreenshot.text        = control.binds.GetBindAsPrettyString(control.binds.bindSaveScreenshot, false);
        menuBindsToggleMenu.text            = control.binds.GetBindAsPrettyString(control.binds.bindToggleMenu, false);
        menuButtonResumeLabel.text          = "Resume " + control.binds.GetBindAsPrettyString(control.binds.bindToggleMenu, true);
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

    public void MenuKeybindsCameraZoomOpticalSet()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsCameraZoomOptical.text = "";
        menuKeybindsBindID = BIND_ID_ZOOM_OPTICAL;
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

    public void MenuKeybindsToggleOutlineSet()
    {
        menuKeybindsIsSettingBind = true;
        menuBindsToggleOutline.text = "";
        menuKeybindsBindID = BIND_ID_OUTLINE;
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