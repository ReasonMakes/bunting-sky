using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public static bool menuOpenAndGamePaused = false;

    public GameObject menuContainer;
    public GameObject menuMain;

    public GameObject menuSettings;
    public TMP_InputField menuSettingsMouseSensitivityIn;
    public TMP_InputField menuSettingsCameraDistanceIn;
    public TMP_InputField menuSettingsCameraHeightIn;
    public TMP_InputField menuSettingsTargetFPSIn;
    public Toggle menuSettingsToggleDisplayHUD;
    public Toggle menuSettingsToggleDisplayFPS;
    public Toggle menuSettingsToggleSpotlight;

    public GameObject menuKeybinds;

    [SerializeField] private Control control;

    private void Start()
    {
        menuContainer.SetActive(false);
        menuMain.SetActive(false);
        menuSettings.SetActive(false);
        menuKeybinds.SetActive(false);
    }

    private void Update()
    {
        //KEYBINDS
        //Menu toggle (includes cursor locking/unlocking)
        if (control.binds.GetInputDown(control.binds.bindToggleMenu))
        {
            MenuToggle();
        }

        //HUD toggle
        if (control.binds.GetInputDown(control.binds.bindToggleHUD))
        {
            MenuSettingsHUDToggle();
        }

        //FPS toggle
        if (control.binds.GetInputDown(control.binds.bindToggleFPS))
        {
            MenuSettingsFPSToggle();
        }

        //Spotlight toggle
        if (control.binds.GetInputDown(control.binds.bindToggleSpotlight))
        {
            control.menu.MenuSettingsSpotlightToggle();
        }
    }

    //MAIN
    public void MenuToggle()
    {
        //Toggle menu
        menuOpenAndGamePaused = !menuOpenAndGamePaused;

        menuContainer.gameObject.SetActive(menuOpenAndGamePaused);
        menuMain.SetActive(menuOpenAndGamePaused);
        if (!menuOpenAndGamePaused)
        {
            menuSettings.SetActive(false);
            menuKeybinds.SetActive(false);
        }

        //Toggle game pause
        Time.timeScale = System.Convert.ToByte(!menuOpenAndGamePaused);

        //Toggle cursor lock

        Cursor.lockState = menuOpenAndGamePaused ? CursorLockMode.None : CursorLockMode.Locked;

        /*
        if (menuOpenAndGamePaused)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        */

        //This is bad practice since assigning 1 or 0 to true or false is arbitrary
        //Cursor.lockState = (CursorLockMode)System.Convert.ToByte(!menuOpenAndGamePaused);

        //Toggle reticle
        control.reticle.SetActive(!menuOpenAndGamePaused);
    }

    public void MenuMainOpen()
    {
        //Disable all sub menus
        menuKeybinds.SetActive(false);
        menuSettings.SetActive(false);

        //Enable main menu
        menuMain.SetActive(true);
    }

    public void MenuQuit()
    {
        Application.Quit();
    }

    //SETTINGS
    public void MenuSettingsOpen()
    {
        menuMain.SetActive(false);
        menuSettings.SetActive(true);

        //Display current settings
        menuSettingsMouseSensitivityIn.text = control.settings.mouseSensitivity.ToString();
        menuSettingsCameraDistanceIn.text = control.settings.cameraDistance.ToString();
        menuSettingsCameraHeightIn.text = control.settings.cameraHeight.ToString();
        menuSettingsTargetFPSIn.text = control.settings.targetFPS.ToString();

        menuSettingsToggleDisplayHUD.isOn = control.settings.displayHUD;
        menuSettingsToggleDisplayFPS.isOn = control.settings.displayFPS;
        menuSettingsToggleSpotlight.isOn = control.settings.spotlightOn;
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
            control.systemInfo.text = "";
        }

        //Update in menu
        menuSettingsToggleDisplayFPS.isOn = control.settings.displayFPS;
    }

    public void MenuSettingsHUDToggle()
    {
        //Update in settings and save
        control.settings.displayHUD = !control.settings.displayHUD;
        control.settings.Save();

        //Update in game
        control.canvas.transform.Find("CameraReticle").gameObject.SetActive(control.settings.displayHUD);
        control.canvas.transform.Find("SystemInfo").gameObject.SetActive(control.settings.displayHUD);
        control.canvas.transform.Find("WarningText").gameObject.SetActive(control.settings.displayHUD);
        control.canvas.transform.Find("MovementMode").gameObject.SetActive(control.settings.displayHUD);
        control.canvas.transform.Find("MovementModeSelector").gameObject.SetActive(control.settings.displayHUD);
        control.canvas.transform.Find("Vitals").gameObject.SetActive(control.settings.displayHUD);
        control.canvas.transform.Find("Resources").gameObject.SetActive(control.settings.displayHUD);
        control.canvas.transform.Find("Weapons").gameObject.SetActive(control.settings.displayHUD);
        control.canvas.transform.Find("Waypoint").gameObject.SetActive(control.settings.displayHUD);

        //Update in menu
        menuSettingsToggleDisplayHUD.isOn = control.settings.displayHUD;
    }

    public void MenuSettingsSpotlightToggle()
    {
        //Update in settings and save
        control.settings.spotlightOn = !control.settings.spotlightOn;
        control.settings.Save();

        //Update in game
        control.instancePlayer.GetComponentInChildren<Player>().spotlight.SetActive(control.settings.spotlightOn);

        //Update in menu
        menuSettingsToggleSpotlight.isOn = control.settings.spotlightOn;
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

    //KEYBINDS
    public void MenuKeybindsOpen()
    {
        menuMain.SetActive(false);
        menuKeybinds.SetActive(true);
    }
}
