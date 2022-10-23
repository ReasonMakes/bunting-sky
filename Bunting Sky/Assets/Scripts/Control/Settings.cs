using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Settings : MonoBehaviour
{
    //Control reference
    public Control control;

    //Input/output
    private IOBuffer ioBuffer;
    private string jsonSaveData;
    private string path;
    private string settingsSaveFile;
    private string fullPath;

    //Definitions
    [System.NonSerialized] public readonly float MOUSE_SENSITIVITY_MIN = 0.001f;
    [System.NonSerialized] public readonly float MOUSE_SENSITIVITY_MAX = 1000f;
    [System.NonSerialized] public readonly float H_FIELD_OF_VIEW_MIN = 0.1f;
    [System.NonSerialized] public readonly float H_FIELD_OF_VIEW_MAX = 360f;
    [System.NonSerialized] public readonly float CAMERA_DISTANCE_MIN = 0.605f;
    [System.NonSerialized] public readonly float CAMERA_DISTANCE_MAX = 20f;
    [System.NonSerialized] public readonly float CAMERA_HEIGHT_MIN = 0f;
    [System.NonSerialized] public readonly float CAMERA_HEIGHT_MAX = 1f;
    [System.NonSerialized] public readonly int TARGET_FPS_MIN = 1;
    [System.NonSerialized] public readonly int TARGET_FPS_MAX = 2000;
    [System.NonSerialized] public readonly int ASTEROIDS_MIN_MIN = 1;
    [System.NonSerialized] public readonly int ASTEROIDS_MIN_MAX = 1000;
    [System.NonSerialized] public readonly int ASTEROIDS_MAX_MIN = 1;
    [System.NonSerialized] public readonly int ASTEROIDS_MAX_MAX = 1000;

    //Settings initializations
    [System.NonSerialized] public float mouseSensitivity;
    [System.NonSerialized] public float hFieldOfView;
    [System.NonSerialized] public float cameraDistance;
    [System.NonSerialized] public float cameraHeight;
    [System.NonSerialized] public bool displayFPS;
    [System.NonSerialized] public bool displayHUD;
    [System.NonSerialized] public int targetFPS;
    [System.NonSerialized] public bool spotlight;
    [System.NonSerialized] public bool outline;
    [System.NonSerialized] public bool refine;
    [System.NonSerialized] public bool music;
    [System.NonSerialized] public bool tips;
    [System.NonSerialized] public bool fullscreen;
    [System.NonSerialized] public bool matchVelocity;
    [System.NonSerialized] public bool spinStabilizers;
    [System.NonSerialized] public int asteroidsConcurrentMin;
    [System.NonSerialized] public int asteroidsConcurrentMax;

    public void Start()
    {
        //Set path
        path = Application.persistentDataPath;
        settingsSaveFile = "/settings.json";
        fullPath = path + Control.userDataFolder + settingsSaveFile;

        //Set all settings to their default settings as a fail-safe
        SetSettingsToDefault();

        //Load user-saved settings. If no file exists, make one from the defaults
        Load(true);
    }

    public void InitIOBuffer()
    {
        //Defaults
        ioBuffer = new IOBuffer();
        ioBuffer.mouseSensitivity = 3f;
        ioBuffer.hFieldOfView = 103f;
        ioBuffer.cameraFollowDistance = CAMERA_DISTANCE_MIN; //0.025 increments from scroll wheel
        ioBuffer.cameraFollowHeight = 0.2f;
        ioBuffer.displayFPS = false;
        ioBuffer.displayHUD = true;
        ioBuffer.targetFPS = 300;
        ioBuffer.spotlightOn = true;
        ioBuffer.outlineOn = true;
        ioBuffer.refine = true;
        ioBuffer.music = true;
        ioBuffer.tips = true;
        ioBuffer.fullscreen = true;
        ioBuffer.matchVelocity = true;
        ioBuffer.spinStabilizers = true;
        ioBuffer.asteroidsConcurrentMin = 900;
        ioBuffer.asteroidsConcurrentMax = 1500;
    }

    public void SetIOBufferToSettings()
    {
        //Set all io buffer settings to the loaded settings
        ioBuffer.mouseSensitivity = mouseSensitivity;
        ioBuffer.hFieldOfView = hFieldOfView;
        ioBuffer.cameraFollowDistance = cameraDistance;
        ioBuffer.cameraFollowHeight = cameraHeight;
        ioBuffer.displayFPS = displayFPS;
        ioBuffer.displayHUD = displayHUD;
        ioBuffer.targetFPS = targetFPS;
        ioBuffer.spotlightOn = spotlight;
        ioBuffer.outlineOn = outline;
        ioBuffer.refine = refine;
        ioBuffer.music = music;
        ioBuffer.tips = tips;
        ioBuffer.fullscreen = fullscreen;
        ioBuffer.matchVelocity = matchVelocity;
        ioBuffer.spinStabilizers = spinStabilizers;
        ioBuffer.asteroidsConcurrentMin = asteroidsConcurrentMin;
        ioBuffer.asteroidsConcurrentMax = asteroidsConcurrentMax;
    }

    public void SetSettingsToIOBuffer()
    {
        //Set all settings to the io buffer's settings
        mouseSensitivity = ioBuffer.mouseSensitivity;
        hFieldOfView = ioBuffer.hFieldOfView;
        cameraDistance = ioBuffer.cameraFollowDistance;
        cameraHeight = ioBuffer.cameraFollowHeight;
        displayFPS = ioBuffer.displayFPS;
        displayHUD = ioBuffer.displayHUD;
        targetFPS = ioBuffer.targetFPS;
        spotlight = ioBuffer.spotlightOn;
        outline = ioBuffer.outlineOn;
        refine = ioBuffer.refine;
        music = ioBuffer.music;
        tips = ioBuffer.tips;
        fullscreen = ioBuffer.fullscreen;
        matchVelocity = ioBuffer.matchVelocity;
        spinStabilizers = ioBuffer.spinStabilizers;
        asteroidsConcurrentMin = ioBuffer.asteroidsConcurrentMin;
        asteroidsConcurrentMax = ioBuffer.asteroidsConcurrentMax;
    }

    //This entire object is what we save/load. It must contain duplicates for all settings definitions that its parent class has
    public class IOBuffer
    {
        //Settings initializations
        public float mouseSensitivity;
        public float hFieldOfView;
        public float cameraFollowDistance;
        public float cameraFollowHeight;
        public bool displayFPS;
        public bool displayHUD;
        public int targetFPS;
        public bool spotlightOn;
        public bool outlineOn;
        public bool refine;
        public bool music;
        public bool tips;
        public bool fullscreen;
        public bool matchVelocity;
        public bool spinStabilizers;
        public int asteroidsConcurrentMin;
        public int asteroidsConcurrentMax;
    }

    #region Save/load
    public void SetSettingsToDefault()
    {
        InitIOBuffer();
        SetSettingsToIOBuffer();
    }

    public void LoadIntoIOBuffer()
    {
        //Load all keybinds
        jsonSaveData = File.ReadAllText(fullPath);

        ioBuffer = JsonUtility.FromJson<IOBuffer>(jsonSaveData);
    }

    public void SaveFromIOBuffer()
    {
        //Save all keybinds
        jsonSaveData = JsonUtility.ToJson(ioBuffer, true);

        File.WriteAllText(fullPath, jsonSaveData);
    }

    public void Load(bool saveIfFileDoesNotExist)
    {
        if (File.Exists(fullPath))
        {
            LoadIntoIOBuffer();
            SetSettingsToIOBuffer();
        }
        else
        {
            Debug.Log("File does not exist: " + fullPath);

            //Create file is toggled on to create if file does not exist
            if (saveIfFileDoesNotExist)
            {
                //Create user data folder if needed
                if (!Directory.Exists(path + Control.userDataFolder))
                {
                    Debug.Log("Directory does not exist; creating directory: " + path + Control.userDataFolder);
                    Directory.CreateDirectory(path + Control.userDataFolder);
                }

                Debug.Log("File does not exist; creating file: " + fullPath);
                SaveFromIOBuffer();
            }
        }
    }

    public void Save()
    {
        SetIOBufferToSettings();
        SaveFromIOBuffer();
    }
    #endregion
}