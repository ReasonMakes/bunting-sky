using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Control : MonoBehaviour
{
    public Menu menu;
    public Commerce commerce;

    [System.NonSerialized] public static Color colorTextDisabled = new Color(1f, 1f, 1f, 0.16f);
    [System.NonSerialized] public static Color colorTextDisabledAsComplete = new Color(0f, 1f, 0f, 0.2f);
    [System.NonSerialized] public static Color colorTextDisabledAsQuest = new Color(1f, 1f, 0f, 0.2f);
    [System.NonSerialized] public static Color colorTextEnabled = new Color(1f, 1f, 1f, 1f);

    //FPS
    [System.NonSerialized] public int fps = 0;
    
    //Physics
    public static readonly float GRAVITATIONAL_CONSTANT = 0.667408f * 1000f;// * 62.5f * 40000f;
    //In real-life, G = 6.674*10^−11 m3*kg^−1*s^−2
    //62.5 is the avg inverse of Time.deltaTime during development

    //Generate system
    public Generation generation;

    //Origin looping
    private readonly float ORIGIN_LOOP_RADIUS = 20f;
    [System.NonSerialized] public readonly float TAU = 6.28318530717958f;

    //User data
    public KeyBinds binds;
    public Settings settings;
    [System.NonSerialized] public static string userDataFolder = "/user";
    [System.NonSerialized] public static string userLevelSaveFile = "/verse.avs"; //Atroviolet save
    [System.NonSerialized] public static string screenshotsFolder = "/screenshots";
    [System.NonSerialized] public readonly float AUTO_SAVE_FREQUENCY = 60f; //10f; //30f;

    #if UNITY_EDITOR
    [System.NonSerialized] public readonly bool IS_EDITOR = true;
    #elif !UNITY_EDITOR
    [System.NonSerialized] public readonly bool IS_EDITOR = false;
    #endif

    //UI
    public UI ui;

    private void Start()
    {
        //FPS Target
        QualitySettings.vSyncCount = 0; //VSync
        Application.targetFrameRate = settings.targetFPS;
        fps = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);

        //Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        //FPS target
        if (settings.targetFPS != Application.targetFrameRate) Application.targetFrameRate = settings.targetFPS;

        //Unlock cursor if game not focused
        if (!Application.isFocused)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        //Screenshot
        if (binds.GetInputDown(binds.bindSaveScreenshot)) SaveScreenshot();

        //Origin looping (maybe this could go in LateUpdate() to avoid the UI shaking)
        if (generation.playerSpawned)
        {
            if (GetPlayerTransform().position.magnitude > ORIGIN_LOOP_RADIUS)
            {
                LoopWorldOrigin();
            }
        }
        
        //Map camera follows player
        //generation.instancePlayer.transform.Find("Position Mount").Find("Map Camera").position -= new Vector3(
        //    generation.instancePlayer.transform.Find("Body").position.x,
        //    0f,
        //    generation.instancePlayer.transform.Find("Body").position.z
        //);
    }

    public Transform GetPlayerTransform()
    {
        if (generation.playerSpawned)
        {
            return generation.instancePlayer.transform.Find("Body");
        }
        else
        {
            //The player hasn't spawned yet!
            return null;
        }
    }

    public Player GetPlayerScript()
    {
        if (generation.playerSpawned)
        {
            return generation.instancePlayer.transform.Find("Body").GetComponent<Player>();
        }
        else
        {
            //The player hasn't spawned yet!
            return null;
        }
    }

    private void LoopWorldOrigin()
    {
        /*
         * The floating origin solution:
         * 
         * Because we are working with vast distances in space, floating point precision errors become a massive problem very quickly
         * To combat this, we loop everything back to the origin whenever the player's displacement is great enough
         * The player will be placed in the centre at (0,0,0) and all verse objects will move with the player so that the distances between them remain the same
         */

        Vector3 playerOldDistanceOut = generation.instancePlayer.transform.Find("Body").position;

        //Player
        generation.instancePlayer.transform.Find("Body").position = Vector3.zero;

        //Verse space
        generation.verseSpace.transform.position -= playerOldDistanceOut;
    }

    public void SaveScreenshot()
    {
        string path;

        //Ensure save directory exists
        //User data folder
        path = Application.persistentDataPath + userDataFolder;
        if (!Directory.Exists(path))
        {
            Debug.Log("Directory does not exist; creating directory: " + path);
            Directory.CreateDirectory(path);
        }

        //Screenshots folder
        path = Application.persistentDataPath + userDataFolder + screenshotsFolder;
        if (!Directory.Exists(path))
        {
            Debug.Log("Directory does not exist; creating directory: " + path);
            Directory.CreateDirectory(path);
        }

        //Generate the filename based on time of screenshot
        //We use string formatting to ensure there are leading zeros to help system file explorers can accurately sort
        path = Application.persistentDataPath + userDataFolder + screenshotsFolder
            + "/" + System.DateTime.Now.Year
            + "-" + System.DateTime.Now.Month.ToString("d2")
            + "-" + System.DateTime.Now.Day.ToString("d2")
            + "_" + System.DateTime.Now.Hour.ToString("d2")
            + "-" + System.DateTime.Now.Minute.ToString("d2")
            + "-" + System.DateTime.Now.Second.ToString("d2")
            + "-" + System.DateTime.Now.Millisecond.ToString("d4")
            + ".png";

        ScreenCapture.CaptureScreenshot(path);
    }

    #region Static methods
    public static void DestroyAllChildren(GameObject parent, float timeDelay)
    {
        foreach (Transform child in parent.transform)
        {
            Destroy(child.gameObject, timeDelay);
        }
    }

    public static Vector3 GetVelocityDraggedRelative(Vector3 velocityToSet, Vector3 otherVelocity, float dragCoefficient)
    {
        /*
         * This uses the same formula as for the default universe drag, except the drag is relative to the difference in velocities of the player body and the planetoid
         * We have to "add back in" the planetoid's velocity since we subtracted it out to get the deltaV
         */

        Vector3 deltaV = velocityToSet - otherVelocity;
        return (deltaV * (1f - (dragCoefficient * Time.deltaTime))) + otherVelocity;
    }

    public static float LoopEulerAngle(float angle)
    {
        if (angle >= 360) angle -= 360;
        else if (angle < 0) angle += 360;

        return angle;
    }

    public static int LowBiasedRandomIntSquared(int valueRootOfMax)
    {
        /*
         * Randomly generates an int with a bias toward low numbers
         * (85% below the middle of the specified range and 15% above the middle of the specified range)
         * The returned int will have a max range that is the square of the input
         * This is useful for generating asteroids with the rare chance of large clusters
         */

        //Randomize size (needs to be shifted one to the right so that multiplication has grows the low-end number too)
        float value = Random.Range(1f, (float)valueRootOfMax);

        //Power (making distribution uneven, and unintentionally making smaller sizes rarer)
        value *= value;

        //Making larger sizes rarer by multiplying the inverse of the value by maximum value squared
        value = (1f / value) * valueRootOfMax * valueRootOfMax;

        //Round (to properly parse to int)
        value = Mathf.Round(value);

        //Return
        return (int)value;
    }

    public static bool GetTrueForPercentOfIndices(int index, int length, float percentage)
    {
        if (index % Mathf.RoundToInt((float)length / ((percentage / 100f) * (float)length)) == 0)
        {
            return true;
        }

        return false;
    }

    /*
    private void TestLowBiasedRandomIntSquared()
    {
        int lows = 0;
        int highs = 0;
        int iterations = 1000 * 1000;

        for (int i = 0; i < iterations; i++)
        {
            int value = LowBiasedRandomIntSquared(4);
            //Debug.Log(value);
            if (value < 8)
            {
                lows++;
            }
            else
            {
                highs++;
            }
        }

        Debug.LogFormat("{0}% lows, {1}% highs",
            Mathf.Round(((float)lows / (float)iterations) * 100f),
            Mathf.Round(((float)highs / (float)iterations) * 100f)
        );
    }
    */

    public static string InsertSpacesInFrontOfCapitals(string text)
    {
        /*
         * Method is by "Binary Worrier"
         * https://stackoverflow.com/questions/272633/add-spaces-before-capital-letters
         */

        if (string.IsNullOrWhiteSpace(text))
        {
            return "";
        }
        
        StringBuilder newText = new StringBuilder(text.Length * 2);
        newText.Append(text[0]);
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]) && text[i - 1] != ' ')
            {
                newText.Append(' ');
            }
            newText.Append(text[i]);
        }

        return newText.ToString();
    }

    public static Transform GetClosestTransformFromHierarchy(Transform hierarchy, Vector3 positionRelativeTo)
    {
        //VARIABLE TO RETURN LATER
        Transform closestTransform = null;

        //CHECK DISTANCES OF ALL TRANSFORMS IN HIERARCHY
        //Start with infinity distance away to compare to
        float closestDistanceSoFar = Mathf.Infinity;

        //Loop through all transforms
        int nTransformsToCheck = hierarchy.childCount;
        for (int i = 0; i < nTransformsToCheck; i++)
        {
            //The transform that we are currently checking
            Transform transformToCheck = hierarchy.GetChild(i);

            //The distance from the player to that transform
            float distanceToTransformToCheck = Vector3.Distance(positionRelativeTo, transformToCheck.position);

            //If the distance is closer than the last transform we checked
            if (distanceToTransformToCheck < closestDistanceSoFar)
            {
                //Set this transform as the closest (so far)
                closestDistanceSoFar = distanceToTransformToCheck;
                closestTransform = transformToCheck;
            }
        }

        //RETURN CLOSEST TRANSFORM
        return closestTransform;
    }

    public Transform GetClosestSpecificTransformFromHierarchy(string name, Transform hierarchy, Vector3 positionRelativeTo)
    {
        //VARIABLE TO RETURN LATER
        Transform closestTransform = null;

        //CHECK DISTANCES OF ALL TRANSFORMS IN HIERARCHY
        //Start with infinity distance away to compare to
        float closestDistanceSoFar = Mathf.Infinity;

        //Loop through all transforms
        int nTransformsToCheck = hierarchy.childCount;
        for (int i = 0; i < nTransformsToCheck; i++)
        {
            //The transform that we are currently checking
            Transform transformToCheck = hierarchy.GetChild(i);

            //Ensure this transform is one of the specific clones we want to check for
            if (transformToCheck.gameObject.name == name)
            {
                //The distance from the player to that transform
                float distanceToTransformToCheck = Vector3.Distance(positionRelativeTo, transformToCheck.position);

                //If the distance is closer than the last transform we checked
                if (distanceToTransformToCheck < closestDistanceSoFar)
                {
                    //Set this transform as the closest (so far)
                    closestDistanceSoFar = distanceToTransformToCheck;
                    closestTransform = transformToCheck;
                }
            }
        }

        //RETURN CLOSEST TRANSFORM
        return closestTransform;
    }

    public int GetIntFromStringIndex(string str, int index)
    {
        //We are working with ASCII here, so we need to subtract the ASCII value of the first alpha numerical character in the lister
        //Ex: list[i] = '3':
        //int strength = (int)list[i] - (int)'0';
        //int strength = 51 - 48; //'0' is encoded as 48 in ASCII
        //int strength = 3;
        return (int)str[index] - (int)'0';
    }
    #endregion

    public Vector3 GetPredictedTrajectoryWithProjectileLeading(
        Vector3 shooterPosition, Vector3 shooterVelocity, float shooterProjectileSpeed,
        float manualLeadMultiplier,
        Vector3 targetPosition, Vector3 targetVelocity, Vector3 targetLastForceAdded, float targetMass
    )
    {
        //Delta V
        Vector3 deltaV = targetVelocity - shooterVelocity;

        //Target position
        Vector3 predictedTrajectory = targetPosition;

        //Time until the projectiles hit the target
        //t = d/v; time in seconds it will take the weapon projectile to be at the target destination
        float timeToTarget = Vector3.Magnitude(targetPosition - shooterPosition) / shooterProjectileSpeed;

        //Lead speed + manual modification
        predictedTrajectory += (deltaV * (timeToTarget * manualLeadMultiplier));

        //Lead acceleration
        //F = ma -> a = F/m
        Vector3 targetAcceleration = targetLastForceAdded / targetMass;
        //displacement = velocity * deltaTime + (1/2)​(acceleration)(deltaTime^2)
        Vector3 displacementFromAcceleration = (deltaV * Time.deltaTime) + ((targetAcceleration * Mathf.Pow(Time.deltaTime, 2f)) / 2f);
        predictedTrajectory += displacementFromAcceleration;

        //Lead change in thrust direction (for if player is thrusting in a circle around the enemy)
        //YET TO BE IMPLEMENTED

        //Return prediction
        return predictedTrajectory;
    }

    public bool GetIfPointLiesWithinTriangle(Vector2 point, Vector2 triVertex1, Vector2 triVertex2, Vector2 triVertex3)
    {
        //barycentric coordinate system
        //http://totologic.blogspot.com/2014/01/accurate-point-in-triangle-test.html

        float denominator = ((triVertex2.y - triVertex3.y) * (triVertex1.x - triVertex3.x) + (triVertex3.x - triVertex2.x) * (triVertex1.y - triVertex3.y));
        float a =           ((triVertex2.y - triVertex3.y) * (point.x - triVertex3.x) + (triVertex3.x - triVertex2.x) * (point.y - triVertex3.y)) / denominator;
        float b =           ((triVertex3.y - triVertex1.y) * (point.x - triVertex3.x) + (triVertex1.x - triVertex3.x) * (point.y - triVertex3.y)) / denominator;
        float c =           1 - a - b;

        return 0 <= a && a <= 1
            && 0 <= b && b <= 1
            && 0 <= c && c <= 1;
    }
}