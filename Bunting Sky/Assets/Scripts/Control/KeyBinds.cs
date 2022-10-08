using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class KeyBinds : MonoBehaviour
{
    public Control control;

    //Input/output
    private IOBuffer ioBuffer;
    private string jsonSaveData;
    private string path;
    private string keybindsSaveFile;
    private string fullPath;

    //Mouse code definitions (1000 is subtracted later. TODO: make this into an enum)
    public readonly short MOUSE_PRIMARY = 1000;
    public readonly short MOUSE_SECONDARY = 1001;
    public readonly short MOUSE_MIDDLE = 1002;
    public readonly short MOUSE_SCROLL_UP = 1003;
    public readonly short MOUSE_SCROLL_DOWN = 1004;

    //Binds initializations
    [System.NonSerialized] public short bindThrustForward;
    [System.NonSerialized] public short bindThrustLeft;
    [System.NonSerialized] public short bindThrustBackward;
    [System.NonSerialized] public short bindThrustRight;
    [System.NonSerialized] public short bindThrustUp;
    [System.NonSerialized] public short bindThrustDown;
    [System.NonSerialized] public short bindAlignShipToReticle;

    [System.NonSerialized] public short bindCheat1;
    [System.NonSerialized] public short bindCheat2;

    [System.NonSerialized] public short bindPanMap;
    [System.NonSerialized] public short bindCameraFreeLook;
    [System.NonSerialized] public short bindCameraZoomIn;
    [System.NonSerialized] public short bindCameraZoomOut;

    [System.NonSerialized] public short bindSetTarget;
    [System.NonSerialized] public short bindPrimaryFire;
    [System.NonSerialized] public short bindPrimaryReload;

    [System.NonSerialized] public short bindToggleSpotlight;
    [System.NonSerialized] public short bindToggleOutline;
    [System.NonSerialized] public short bindToggleMap;
    [System.NonSerialized] public short bindToggleRefine;
    [System.NonSerialized] public short bindSelectWeapon1;
    [System.NonSerialized] public short bindSelectWeapon2;

    [System.NonSerialized] public short bindToggleHUD;
    [System.NonSerialized] public short bindToggleFPS;
    [System.NonSerialized] public short bindSaveScreenshot;

    [System.NonSerialized] public short bindToggleMenu;

    public void Start()
    {
        //Set path
        path = Application.persistentDataPath;
        keybindsSaveFile = "/keybinds.json";
        fullPath = path + Control.userDataFolder + keybindsSaveFile;

        //Set all keybinds to their default settings as a fail-safe
        SetBindsToDefault();

        //Load user-saved keybinds. If no file exists, make one from the defaults
        Load(true);
    }

    public void InitIOBuffer()
    {
        ioBuffer = new IOBuffer
        {
            //Set defaults
            bindThrustForward = (short)KeyCode.W,
            bindThrustLeft = (short)KeyCode.A,
            bindThrustBackward = (short)KeyCode.S,
            bindThrustRight = (short)KeyCode.D,
            bindThrustUp = (short)KeyCode.Space,
            bindThrustDown = (short)KeyCode.LeftControl,
            bindAlignShipToReticle = (short)KeyCode.LeftShift,

            bindCheat1 = (short)KeyCode.I,
            bindCheat2 = (short)KeyCode.O,

            bindPanMap = MOUSE_MIDDLE,
            bindCameraFreeLook = MOUSE_SECONDARY,
            bindCameraZoomIn = MOUSE_SCROLL_UP,
            bindCameraZoomOut = MOUSE_SCROLL_DOWN,

            bindSetTarget = MOUSE_MIDDLE,
            bindPrimaryFire = MOUSE_PRIMARY,
            bindPrimaryReload = (short)KeyCode.R,

            bindToggleSpotlight = (short)KeyCode.F,
            bindToggleOutline = (short)KeyCode.C,
            bindToggleMap = (short)KeyCode.M,
            bindToggleRefine = (short)KeyCode.T,
            bindSelectWeapon1 = (short)KeyCode.Z,
            bindSelectWeapon2 = (short)KeyCode.X,

            bindToggleHUD = (short)KeyCode.F3,
            bindToggleFPS = (short)KeyCode.F4,
            bindSaveScreenshot = (short)KeyCode.F2,

            bindToggleMenu = (short)KeyCode.Escape
        };
    }

    public void SetIOBufferToBinds()
    {
        //Set all io buffer binds to the loaded binds
        ioBuffer.bindThrustForward = bindThrustForward;
        ioBuffer.bindThrustLeft = bindThrustLeft;
        ioBuffer.bindThrustBackward = bindThrustBackward;
        ioBuffer.bindThrustRight = bindThrustRight;
        ioBuffer.bindThrustUp = bindThrustUp;
        ioBuffer.bindThrustDown = bindThrustDown;
        ioBuffer.bindAlignShipToReticle = bindAlignShipToReticle;

        ioBuffer.bindCheat1 = bindCheat1;
        ioBuffer.bindCheat2 = bindCheat2;

        ioBuffer.bindPanMap = bindPanMap;
        ioBuffer.bindCameraFreeLook = bindCameraFreeLook;
        ioBuffer.bindCameraZoomIn = bindCameraZoomIn;
        ioBuffer.bindCameraZoomOut = bindCameraZoomOut;

        ioBuffer.bindSetTarget = bindSetTarget;
        ioBuffer.bindPrimaryFire = bindPrimaryFire;
        ioBuffer.bindPrimaryReload = bindPrimaryReload;

        ioBuffer.bindToggleSpotlight = bindToggleSpotlight;
        ioBuffer.bindToggleOutline = bindToggleOutline;
        ioBuffer.bindToggleMap = bindToggleMap;
        ioBuffer.bindToggleRefine = bindToggleRefine;
        ioBuffer.bindSelectWeapon1 = bindSelectWeapon1;
        ioBuffer.bindSelectWeapon2 = bindSelectWeapon2;

        ioBuffer.bindToggleHUD = bindToggleHUD;
        ioBuffer.bindToggleFPS = bindToggleFPS;
        ioBuffer.bindSaveScreenshot = bindSaveScreenshot;

        ioBuffer.bindToggleMenu = bindToggleMenu;
    }

    public void SetBindsToIOBuffer()
    {
        //Set all binds to the io buffer's binds
        bindThrustForward = ioBuffer.bindThrustForward;
        bindThrustLeft = ioBuffer.bindThrustLeft;
        bindThrustBackward = ioBuffer.bindThrustBackward;
        bindThrustRight = ioBuffer.bindThrustRight;
        bindThrustUp = ioBuffer.bindThrustUp;
        bindThrustDown = ioBuffer.bindThrustDown;
        bindAlignShipToReticle = ioBuffer.bindAlignShipToReticle;

        bindCheat1 = ioBuffer.bindCheat1;
        bindCheat2 = ioBuffer.bindCheat2;

        bindPanMap = ioBuffer.bindPanMap;
        bindCameraFreeLook = ioBuffer.bindCameraFreeLook;
        bindCameraZoomIn = ioBuffer.bindCameraZoomIn;
        bindCameraZoomOut = ioBuffer.bindCameraZoomOut;

        bindSetTarget = ioBuffer.bindSetTarget;
        bindPrimaryFire = ioBuffer.bindPrimaryFire;
        bindPrimaryReload = ioBuffer.bindPrimaryReload;

        bindToggleSpotlight = ioBuffer.bindToggleSpotlight;
        bindToggleOutline = ioBuffer.bindToggleOutline;
        bindToggleMap = ioBuffer.bindToggleMap;
        bindToggleRefine = ioBuffer.bindToggleRefine;
        bindSelectWeapon1 = ioBuffer.bindSelectWeapon1;
        bindSelectWeapon2 = ioBuffer.bindSelectWeapon2;

        bindToggleHUD = ioBuffer.bindToggleHUD;
        bindToggleFPS = ioBuffer.bindToggleFPS;
        bindSaveScreenshot = ioBuffer.bindSaveScreenshot;

        bindToggleMenu = ioBuffer.bindToggleMenu;

        //Update game
        control.ui.UpdateTipBinds();
    }

    //This entire object is what we save/load. It must contain duplicates for all binds definitions that its parent class has
    public class IOBuffer
    {
        //Binds initializations
        public short bindThrustForward;
        public short bindThrustLeft;
        public short bindThrustBackward;
        public short bindThrustRight;
        public short bindThrustUp;
        public short bindThrustDown;
        public short bindAlignShipToReticle;

        public short bindCheat1;
        public short bindCheat2;

        public short bindPanMap;
        public short bindCameraFreeLook;
        public short bindCameraZoomIn;
        public short bindCameraZoomOut;

        public short bindSetTarget;
        public short bindPrimaryFire;
        public short bindPrimaryReload;

        public short bindToggleSpotlight;
        public short bindToggleOutline;
        public short bindToggleMap;
        public short bindToggleRefine;
        public short bindSelectWeapon1;
        public short bindSelectWeapon2;

        public short bindToggleHUD;
        public short bindToggleFPS;
        public short bindSaveScreenshot;

        public short bindToggleMenu;
    }

    #region Save/load
    public void SetBindsToDefault()
    {
        InitIOBuffer();
        SetBindsToIOBuffer();
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
            SetBindsToIOBuffer();
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
        SetIOBufferToBinds();
        SaveFromIOBuffer();
    }
    #endregion

    #region Input getters
    public bool GetInputDown(short inputCode)
    {
        //0 to 509 = KeyCode
        //1000 to 1002 = Mouse code

        if(inputCode <= 509)
        {
            return Input.GetKeyDown((KeyCode)inputCode);
        }
        else
        {
            //Subtract 1000 from the inputCode to format it to the 0, 1, 2 etc. codes expected for mouse input
            return Input.GetMouseButtonDown((inputCode - 1000));
        }
    }

    public bool GetInput(short inputCode)
    {
        //0 to 509 = KeyCode
        //1000 to 1002 = Mouse button
        //1003 and 1004 = Mouse scroll wheel

        if(inputCode <= 509)
        {
            return Input.GetKey((KeyCode)inputCode);
        }
        else if(inputCode <= 1002)
        {
            //Subtract 1000 from the inputCode to format it to the 0, 1, 2 etc. codes expected for mouse input
            return Input.GetMouseButton(inputCode - 1000);
        }
        else if (inputCode == MOUSE_SCROLL_UP)
        {
            return Input.mouseScrollDelta.y > 0;
        }
        else if (inputCode == MOUSE_SCROLL_DOWN)
        {
            return Input.mouseScrollDelta.y < 0;
        }
        else
        {
            return false;
        }
    }
    #endregion
}