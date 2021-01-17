using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Settings : MonoBehaviour
{
    //Input/output
    private IOBuffer ioBuffer;
    private string jsonSaveData;
    private string path;

    //Settings initializations
    [System.NonSerialized] public float mouseSens;
    [System.NonSerialized] public float cameraFollowDist;
    [System.NonSerialized] public float cameraFollowHeight;
    [System.NonSerialized] public bool displayFPSCounter;
    [System.NonSerialized] public bool displayHUD;
    [System.NonSerialized] public bool spotlightOn;

    public void Start()
    {
        //Set path
        path = Application.persistentDataPath + "/user/settings.json";

        //Set all keybinds to their default settings as a fail-safe
        SetSettingsToDefault();

        //Load user-saved keybinds. If no file exists, make one from the defaults
        Load(true);
    }

    public void InitIOBuffer()
    {
        ioBuffer = new IOBuffer
        {
            //Set defaults
            mouseSens = 3f,
            cameraFollowDist = 0.3f, //0.025 increments, range of 0.08f to 2.4f, 0.08f for first-person
            cameraFollowHeight = 0.2f, //range of 0 to 1?
            displayFPSCounter = false,
            displayHUD = true,
            spotlightOn = true
        };
    }

    public void SetIOBufferToSettings()
    {
        //Set all io buffer settings to the loaded settings
        ioBuffer.mouseSens = mouseSens;
        ioBuffer.cameraFollowDist = cameraFollowDist;
        ioBuffer.cameraFollowHeight = cameraFollowHeight;
        ioBuffer.displayFPSCounter = displayFPSCounter;
        ioBuffer.displayHUD = displayHUD;
        ioBuffer.spotlightOn = spotlightOn;
    }

    public void SetSettingsToIOBuffer()
    {
        //Set all settings to the io buffer's settings
        mouseSens = ioBuffer.mouseSens;
        cameraFollowDist = ioBuffer.cameraFollowDist;
        cameraFollowHeight = ioBuffer.cameraFollowHeight;
        displayFPSCounter = ioBuffer.displayFPSCounter;
        displayHUD = ioBuffer.displayHUD;
        spotlightOn = ioBuffer.spotlightOn;
    }

    public void SetSettingsToDefault()
    {
        InitIOBuffer();
        SetSettingsToIOBuffer();
    }

    public void LoadIntoIOBuffer()
    {
        //Load all keybinds
        jsonSaveData = File.ReadAllText(path);

        ioBuffer = JsonUtility.FromJson<IOBuffer>(jsonSaveData);
    }

    public void SaveFromIOBuffer()
    {
        //Save all keybinds
        jsonSaveData = JsonUtility.ToJson(ioBuffer, true);

        File.WriteAllText(path, jsonSaveData);
    }

    public void Load(bool saveIfFileDoesNotExist)
    {
        if (File.Exists(path))
        {
            LoadIntoIOBuffer();
            SetSettingsToIOBuffer();
        }
        else
        {
            Debug.Log("File does not exist");

            if(saveIfFileDoesNotExist) SaveFromIOBuffer();
        }
    }

    public void Save()
    {
        SetIOBufferToSettings();
        SaveFromIOBuffer();
    }

    //This entire object is what we save/load. It must contain duplicates for all binds definitions that its parent class has
    public class IOBuffer
    {
        //Settings initializations
        public float mouseSens;
        public float cameraFollowDist;
        public float cameraFollowHeight;
        public bool displayFPSCounter;
        public bool displayHUD;
        public bool spotlightOn;
    }
}